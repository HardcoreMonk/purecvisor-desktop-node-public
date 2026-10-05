using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// vm.attach/vm.eject 는 큐 등록 때 vm.list 의 dvd_media(붙은 ISO 경로) 집합과 VM id 를 잡는다. 조정은 같은 VM 의
// media 집합이 요청한 만큼만 바뀌었을 때 성공이다. dvd_media 를 읽지 못하면(필드 없음) 판정하지 않는다.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private const string VmMediaReconciliationSchema = "pcv-vm-media-reconciliation/v1";

    public JsonElement BuildVmMediaParameters(string operation, string vmName, string? isoPath, CancellationToken cancellationToken)
    {
        var parameters = new SortedDictionary<string, object?> { ["name"] = vmName };
        if (operation == "vm.attach")
        {
            parameters["iso_path"] = isoPath;
        }

        parameters["reconciliation"] = CaptureVmMediaBaseline(operation, vmName, isoPath, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(parameters);
    }

    private JsonElement CaptureVmMediaBaseline(string operation, string vmName, string? isoPath, CancellationToken cancellationToken)
    {
        var expectedAfter = new SortedDictionary<string, object?> { ["name"] = vmName };
        if (isoPath is not null)
        {
            expectedAfter["iso_path"] = isoPath;
        }

        JsonElement Unavailable(string code) => DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema"] = VmMediaReconciliationSchema,
            ["operation"] = operation,
            ["capture_status"] = "unavailable",
            ["capture_error_code"] = code,
            ["before"] = null,
            ["expected_after"] = expectedAfter
        });

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return Unavailable(readback.Error?.Code ?? "PCV_VM_LIST_FAILED");
            }

            var matches = MatchingVms(readback.Data.Value, vmName);
            if (matches.Length != 1 || string.IsNullOrWhiteSpace(DesktopNodeApiJsonReader.GetStringProperty(matches[0], "id")))
            {
                return Unavailable(matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS");
            }

            var media = DvdMediaPaths(matches[0]);
            if (media is null)
            {
                return Unavailable("PCV_VM_MEDIA_READBACK_UNAVAILABLE");
            }

            if (operation == "vm.eject" && media.Length == 0)
            {
                return Unavailable("PCV_VM_MEDIA_NOT_PRESENT");
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmMediaReconciliationSchema,
                ["operation"] = operation,
                ["capture_status"] = "captured",
                ["before"] = matches[0].Clone(),
                ["before_media"] = media,
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return Unavailable("PCV_VM_LIST_FAILED");
        }
    }

    private DesktopNodeApiResponse ReconcileVmMediaJob(DesktopNodeJobSnapshot job, CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var operation = job.Operation;
        var attach = operation == "vm.attach";
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var isoPath = DesktopNodeApiJsonReader.ReadString(job.Parameters, "iso_path");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");

        DesktopNodeApiResponse Required(string classification, string detail) => RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(jobId, classification, detail, operation))));

        var beforeMedia = metadata is { ValueKind: JsonValueKind.Object } value &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmMediaReconciliationSchema, StringComparison.Ordinal) &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(value, "operation"), operation, StringComparison.Ordinal) &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal) &&
            value.TryGetProperty("before_media", out var capturedMedia) &&
            capturedMedia.ValueKind == JsonValueKind.Array
                ? capturedMedia.EnumerateArray().Select(item => item.GetString()).Where(path => !string.IsNullOrWhiteSpace(path)).Select(path => NormalizeMediaPath(path!)).ToArray()
                : null;
        var before = metadata is { ValueKind: JsonValueKind.Object } baseline ? DesktopNodeApiJsonReader.ReadElement(baseline, "before") : null;
        var beforeId = before is { ValueKind: JsonValueKind.Object } beforeVm ? DesktopNodeApiJsonReader.GetStringProperty(beforeVm, "id") : null;
        if (beforeMedia is null || string.IsNullOrWhiteSpace(vmName) || string.IsNullOrWhiteSpace(beforeId) ||
            (attach && string.IsNullOrWhiteSpace(isoPath)))
        {
            return Required(
                "baseline-unavailable",
                $"The durable {operation} baseline was not captured or is not structurally valid.");
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_VM_LIST_FAILED";
            return Required(
                "readback-unavailable",
                $"Provider vm.list readback failed with {providerCode}; no mutation was attempted.");
        }

        var matching = MatchingVms(readback.Data.Value, vmName);
        if (matching.Length != 1 ||
            !string.Equals(DesktopNodeApiJsonReader.GetStringProperty(matching[0], "id"), beforeId, StringComparison.OrdinalIgnoreCase))
        {
            var identityClassification = matching.Length == 0
                ? "expected-target-not-observed"
                : matching.Length > 1
                    ? "ambiguous-duplicate-names"
                    : "identity-mismatch";
            return Required(
                identityClassification,
                $"Provider vm.list readback did not prove a unique VM identity for {operation} reconciliation.");
        }

        var afterMedia = DvdMediaPaths(matching[0]);
        if (afterMedia is null)
        {
            return Required(
                "readback-value-unavailable",
                "Provider vm.list readback did not report dvd_media.");
        }

        var requested = attach ? NormalizeMediaPath(isoPath!) : null;
        var applied = attach
            ? afterMedia.Count(path => path == requested) == 1 && !beforeMedia.Contains(requested)
            : IsSingleRemoval(beforeMedia, afterMedia);
        if (!applied)
        {
            var classification = afterMedia.Order(StringComparer.Ordinal).SequenceEqual(beforeMedia.Order(StringComparer.Ordinal))
                ? "not-applied"
                : "ambiguous-media-state";
            return Required(
                classification,
                $"Provider vm.list readback did not prove the {operation} postcondition.");
        }

        var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["action"] = "reconciled",
            ["operation"] = operation,
            ["reconciliation"] = new SortedDictionary<string, object?>
            {
                ["schema"] = VmMediaReconciliationSchema,
                ["classification"] = "postcondition-confirmed",
                ["before"] = before,
                ["expected_after"] = DesktopNodeApiJsonReader.ReadElement(metadata!.Value, "expected_after"),
                ["observed"] = matching[0]
            }
        });
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(true, "postcondition-confirmed", result)));
    }

    private static string[]? DvdMediaPaths(JsonElement vm)
    {
        return vm.ValueKind == JsonValueKind.Object &&
            vm.TryGetProperty("dvd_media", out var media) &&
            media.ValueKind == JsonValueKind.Array
                ? media.EnumerateArray()
                    .Select(item => DesktopNodeApiJsonReader.GetStringProperty(item, "path"))
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => NormalizeMediaPath(path!))
                    .ToArray()
                : null;
    }

    private static bool IsSingleRemoval(string[] before, string[] after)
    {
        if (after.Length != before.Length - 1)
        {
            return false;
        }

        var remaining = before.ToList();
        return after.All(remaining.Remove);
    }

    private static string NormalizeMediaPath(string path)
    {
        var trimmed = path.Trim();
        try
        {
            return Path.GetFullPath(trimmed).ToUpperInvariant();
        }
        catch (Exception)
        {
            return trimmed.ToUpperInvariant();
        }
    }
}
