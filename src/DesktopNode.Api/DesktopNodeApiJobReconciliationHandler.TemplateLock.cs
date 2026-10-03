using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// vm.template.lock 은 큐 등록 때 요청한 locked 값과 before 값을 잡는다. vm.list 는 잠기지 않은 VM 의
// template_lock 을 생략하므로 없으면 false 다. identity 는 자원 변경과 같은 불변 fingerprint 를 쓴다.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private const string VmTemplateLockReconciliationSchema = "pcv-vm-template-lock-reconciliation/v1";

    public JsonElement BuildVmTemplateLockParameters(string vmName, bool locked, CancellationToken cancellationToken)
    {
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["locked"] = locked,
            ["reconciliation"] = CaptureVmTemplateLockBaseline(vmName, locked, cancellationToken)
        });
    }

    private JsonElement CaptureVmTemplateLockBaseline(string vmName, bool locked, CancellationToken cancellationToken)
    {
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["template_lock"] = locked
        };

        JsonElement Unavailable(string code) => DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema"] = VmTemplateLockReconciliationSchema,
            ["capture_status"] = "unavailable",
            ["capture_error_code"] = code,
            ["before"] = null,
            ["before_fingerprint"] = null,
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

            var matches = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
                .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (matches.Length != 1)
            {
                return Unavailable(matches.Length == 0 ? "PCV_VM_NOT_FOUND" : "PCV_VM_IDENTITY_AMBIGUOUS");
            }

            var before = matches[0].Clone();
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmTemplateLockReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmResourceIdentityFingerprint(before),
                ["before_template_lock"] = ReadTemplateLock(before),
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return Unavailable("PCV_VM_LIST_FAILED");
        }
    }

    private DesktopNodeApiResponse ReconcileVmTemplateLockJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var requested = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "locked");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");

        DesktopNodeApiResponse Required(string classification, string detail) => RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(jobId, classification, detail, "vm.template.lock"))));

        var baselineValid = metadata is { ValueKind: JsonValueKind.Object } value &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmTemplateLockReconciliationSchema, StringComparison.Ordinal) &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal) &&
            DesktopNodeApiJsonReader.ReadElement(value, "before_fingerprint") is { ValueKind: JsonValueKind.Object } &&
            DesktopNodeApiJsonReader.ReadElement(value, "expected_after") is { ValueKind: JsonValueKind.Object } expected &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(expected, "name"), vmName, StringComparison.Ordinal) &&
            DesktopNodeApiJsonReader.ReadElement(expected, "template_lock") is { } expectedLock &&
            requested is { } requestedLock &&
            expectedLock.ValueKind == requestedLock.ValueKind &&
            requestedLock.ValueKind is JsonValueKind.True or JsonValueKind.False;
        if (string.IsNullOrWhiteSpace(vmName) || !baselineValid)
        {
            return Required(
                "baseline-unavailable",
                "The durable vm.template.lock baseline was not captured or is not structurally valid.");
        }

        var baseline = metadata!.Value;
        var requestedLocked = requested!.Value.ValueKind == JsonValueKind.True;
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

        var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matching.Length != 1 ||
            !ResourceIdentityMatches(DesktopNodeApiJsonReader.ReadElement(baseline, "before_fingerprint")!.Value, matching[0]))
        {
            var identityClassification = matching.Length == 0
                ? "expected-target-not-observed"
                : matching.Length > 1
                    ? "ambiguous-duplicate-names"
                    : "identity-mismatch";
            return Required(
                identityClassification,
                "Provider vm.list readback did not prove a unique VM identity for template-lock reconciliation.");
        }

        if (ReadTemplateLock(matching[0]) != requestedLocked)
        {
            return Required(
                "not-applied",
                $"Provider vm.list readback did not prove template_lock={requestedLocked.ToString().ToLowerInvariant()}.");
        }

        var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["action"] = "reconciled",
            ["operation"] = "vm.template.lock",
            ["reconciliation"] = new SortedDictionary<string, object?>
            {
                ["schema"] = VmTemplateLockReconciliationSchema,
                ["classification"] = "postcondition-confirmed",
                ["before"] = DesktopNodeApiJsonReader.ReadElement(baseline, "before"),
                ["expected_after"] = DesktopNodeApiJsonReader.ReadElement(baseline, "expected_after"),
                ["observed"] = matching[0]
            }
        });
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(true, "postcondition-confirmed", result)));
    }

    private static bool ReadTemplateLock(JsonElement vm)
    {
        return DesktopNodeApiJsonReader.ReadElement(vm, "template_lock") is { ValueKind: JsonValueKind.True };
    }
}
