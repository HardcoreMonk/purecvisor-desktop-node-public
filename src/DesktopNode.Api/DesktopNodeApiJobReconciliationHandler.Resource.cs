using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 자원 변경 job(vm.set-memory, vm.set-vcpu, vm.disk-resize)은 큐 등록 때 요청 값과 before 값을 잡는다.
// identity fingerprint 는 바뀌는 자원 값을 빼고 VM id 와 불변 속성만 쓴다. disk-resize 는 provider 와 같은 규칙
// (첫 번째로 연결된 VHD/VHDX)으로 대상 경로를 고정하고, 조정 때 그 경로의 size_gb 를 읽는다.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private const string VmResourceReconciliationSchema = "pcv-vm-resource-reconciliation/v1";

    internal static string? ResourceValueProperty(string? operation)
    {
        return operation switch
        {
            "vm.set-memory" => "memory_mb",
            "vm.set-vcpu" => "cpu",
            "vm.disk-resize" => "disk_gb",
            _ => null
        };
    }

    public JsonElement BuildVmResourceParameters(
        string operation,
        string vmName,
        int requestedValue,
        CancellationToken cancellationToken)
    {
        var valueProperty = ResourceValueProperty(operation)
            ?? throw new ArgumentException($"Operation '{operation}' has no resource reconciliation.", nameof(operation));
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            [valueProperty] = requestedValue,
            ["reconciliation"] = CaptureVmResourceBaseline(operation, vmName, valueProperty, requestedValue, cancellationToken)
        });
    }

    private JsonElement CaptureVmResourceBaseline(
        string operation,
        string vmName,
        string valueProperty,
        int requestedValue,
        CancellationToken cancellationToken)
    {
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            [valueProperty] = requestedValue
        };

        JsonElement Unavailable(string code) => DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema"] = VmResourceReconciliationSchema,
            ["operation"] = operation,
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
            string? diskPath = null;
            if (operation == "vm.disk-resize")
            {
                diskPath = FirstAttachedVirtualDiskPath(before);
                if (diskPath is null)
                {
                    return Unavailable("PCV_VM_DISK_NOT_FOUND");
                }

                expectedAfter["disk_path"] = diskPath;
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmResourceReconciliationSchema,
                ["operation"] = operation,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmResourceIdentityFingerprint(before),
                ["before_value"] = ReadResourceValue(before, operation, diskPath),
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return Unavailable("PCV_VM_LIST_FAILED");
        }
    }

    private static bool TryReadCapturedVmResourceBaseline(
        JsonElement? metadata,
        string operation,
        string vmName,
        int requestedValue,
        out VmResourceBaseline baseline)
    {
        baseline = null!;
        var valueProperty = ResourceValueProperty(operation);
        if (valueProperty is null || metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var beforeFingerprint = DesktopNodeApiJsonReader.ReadElement(value, "before_fingerprint");
        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(value, "expected_after");
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmResourceReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "operation"), operation, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal) ||
            before is null ||
            beforeFingerprint is null ||
            expectedAfter is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            beforeFingerprint.Value.ValueKind != JsonValueKind.Object ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "name"), vmName, StringComparison.Ordinal) ||
            DesktopNodeApiJsonReader.ReadInt(expectedAfter.Value, valueProperty) != requestedValue)
        {
            return false;
        }

        var diskPath = DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "disk_path");
        if (operation == "vm.disk-resize" && string.IsNullOrWhiteSpace(diskPath))
        {
            return false;
        }

        baseline = new VmResourceBaseline(
            before.Value.Clone(),
            beforeFingerprint.Value.Clone(),
            DesktopNodeApiJsonReader.ReadInt(value, "before_value"),
            diskPath,
            expectedAfter.Value.Clone());
        return true;
    }

    private DesktopNodeApiResponse ReconcileVmResourceJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var operation = job.Operation;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var requestedValue = DesktopNodeApiJsonReader.ReadInt(job.Parameters, ResourceValueProperty(operation)!);
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");

        DesktopNodeApiResponse Required(string classification, string detail) => RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(jobId, classification, detail, operation))));

        if (string.IsNullOrWhiteSpace(vmName) || requestedValue is null ||
            !TryReadCapturedVmResourceBaseline(metadata, operation, vmName, requestedValue.Value, out var baseline))
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

        var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matching.Length != 1 || !ResourceIdentityMatches(baseline.BeforeFingerprint, matching[0]))
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

        var observedValue = ReadResourceValue(matching[0], operation, baseline.DiskPath);
        if (observedValue != requestedValue)
        {
            var classification = observedValue is null
                ? "readback-value-unavailable"
                : observedValue == baseline.BeforeValue
                    ? "not-applied"
                    : "incomplete-resource-value";
            return Required(
                classification,
                $"Provider vm.list readback did not prove the requested {operation} value {requestedValue}.");
        }

        var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["action"] = "reconciled",
            ["operation"] = operation,
            ["reconciliation"] = new SortedDictionary<string, object?>
            {
                ["schema"] = VmResourceReconciliationSchema,
                ["classification"] = "postcondition-confirmed",
                ["before"] = baseline.Before,
                ["expected_after"] = baseline.ExpectedAfter,
                ["observed"] = matching[0]
            }
        });
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(true, "postcondition-confirmed", result)));
    }

    private static int? ReadResourceValue(JsonElement vm, string operation, string? diskPath)
    {
        var element = operation switch
        {
            "vm.set-memory" => DesktopNodeApiJsonReader.ReadNestedElement(vm, "memory", "startup_mb"),
            "vm.set-vcpu" => DesktopNodeApiJsonReader.ReadNestedElement(vm, "cpu", "count"),
            _ => VirtualDisks(vm)
                .Where(disk => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(disk, "path"), diskPath, StringComparison.OrdinalIgnoreCase))
                .Select(disk => DesktopNodeApiJsonReader.ReadElement(disk, "size_gb"))
                .FirstOrDefault()
        };
        return element is { ValueKind: JsonValueKind.Number } number && number.TryGetInt32(out var parsed) ? parsed : null;
    }

    private static string? FirstAttachedVirtualDiskPath(JsonElement vm)
    {
        return VirtualDisks(vm)
            .Where(disk => DesktopNodeApiJsonReader.ReadElement(disk, "attached") is { ValueKind: JsonValueKind.True })
            .Select(disk => DesktopNodeApiJsonReader.GetStringProperty(disk, "path"))
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) &&
                (path.EndsWith(".vhdx", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".vhd", StringComparison.OrdinalIgnoreCase)));
    }

    private static IEnumerable<JsonElement> VirtualDisks(JsonElement vm)
    {
        return vm.ValueKind == JsonValueKind.Object &&
            vm.TryGetProperty("storage", out var storage) &&
            storage.ValueKind == JsonValueKind.Array
                ? storage.EnumerateArray()
                : [];
    }

    private static JsonElement BuildVmResourceIdentityFingerprint(JsonElement vm)
    {
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["id"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "id"),
            ["platform"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "platform"),
            ["guest_family"] = DesktopNodeApiJsonReader.GetStringProperty(vm, "guest_family"),
            ["generation"] = DesktopNodeApiJsonReader.ReadElement(vm, "generation"),
            ["managed_by_purecvisor"] = DesktopNodeApiJsonReader.ReadElement(vm, "managed_by_purecvisor")
        });
    }

    private static bool ResourceIdentityMatches(JsonElement beforeFingerprint, JsonElement observed)
    {
        return beforeFingerprint.ValueKind == JsonValueKind.Object &&
            observed.ValueKind == JsonValueKind.Object &&
            JsonNode.DeepEquals(
                JsonNode.Parse(beforeFingerprint.GetRawText()),
                JsonNode.Parse(BuildVmResourceIdentityFingerprint(observed).GetRawText()));
    }

    private sealed record VmResourceBaseline(
        JsonElement Before,
        JsonElement BeforeFingerprint,
        int? BeforeValue,
        string? DiskPath,
        JsonElement ExpectedAfter);
}
