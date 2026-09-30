using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// vm.network.connect 와 vm.manage 는 큐 등록 때 VM 전체 readback 을 before 로 잡고 VM id 로 identity 를 판정한다.
// network.connect 는 provider 가 첫 번째 synthetic NIC 을 다시 연결하지만 vm.list 는 연결된 switch 이름만 주므로,
// 연결된 adapter 가 요청 switch 하나뿐이고 before 에 그 switch 가 없었을 때만 성공이다.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private const string VmNetworkConnectReconciliationSchema = "pcv-vm-network-connect-reconciliation/v1";
    private const string VmManageReconciliationSchema = "pcv-vm-manage-reconciliation/v1";

    public JsonElement BuildVmNetworkConnectParameters(string vmName, string switchName, CancellationToken cancellationToken)
    {
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["switch"] = switchName,
            ["vm_name"] = vmName,
            ["reconciliation"] = CaptureVmReadbackBaseline(
                VmNetworkConnectReconciliationSchema,
                vmName,
                new SortedDictionary<string, object?> { ["name"] = vmName, ["switch"] = switchName },
                cancellationToken)
        });
    }

    public JsonElement BuildVmManageParameters(string vmName, CancellationToken cancellationToken)
    {
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["reconciliation"] = CaptureVmReadbackBaseline(
                VmManageReconciliationSchema,
                vmName,
                new SortedDictionary<string, object?> { ["name"] = vmName, ["managed_by_purecvisor"] = true },
                cancellationToken)
        });
    }

    private JsonElement CaptureVmReadbackBaseline(
        string schema,
        string vmName,
        SortedDictionary<string, object?> expectedAfter,
        CancellationToken cancellationToken)
    {
        JsonElement Unavailable(string code) => DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema"] = schema,
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

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = schema,
                ["capture_status"] = "captured",
                ["before"] = matches[0].Clone(),
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return Unavailable("PCV_VM_LIST_FAILED");
        }
    }

    private DesktopNodeApiResponse ReconcileVmReadbackJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var operation = job.Operation;
        var networkConnect = operation == "vm.network.connect";
        var schema = networkConnect ? VmNetworkConnectReconciliationSchema : VmManageReconciliationSchema;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, networkConnect ? "vm_name" : "name");
        var requestedSwitch = DesktopNodeApiJsonReader.ReadString(job.Parameters, "switch");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");

        DesktopNodeApiResponse Required(string classification, string detail) => RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(jobId, classification, detail, operation))));

        var before = metadata is { ValueKind: JsonValueKind.Object } value &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), schema, StringComparison.Ordinal) &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal) &&
            DesktopNodeApiJsonReader.ReadElement(value, "expected_after") is { ValueKind: JsonValueKind.Object } expected &&
            string.Equals(DesktopNodeApiJsonReader.ReadString(expected, "name"), vmName, StringComparison.Ordinal) &&
            DesktopNodeApiJsonReader.ReadElement(value, "before") is { ValueKind: JsonValueKind.Object } captured
                ? captured
                : (JsonElement?)null;
        var beforeId = before is null ? null : DesktopNodeApiJsonReader.GetStringProperty(before.Value, "id");
        if (string.IsNullOrWhiteSpace(vmName) || string.IsNullOrWhiteSpace(beforeId) ||
            (networkConnect && string.IsNullOrWhiteSpace(requestedSwitch)))
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

        string? classification;
        if (networkConnect)
        {
            var beforeSwitches = ConnectedSwitches(before!.Value);
            var afterSwitches = ConnectedSwitches(matching[0]);
            classification = afterSwitches.Length == 1 &&
                string.Equals(afterSwitches[0], requestedSwitch, StringComparison.OrdinalIgnoreCase) &&
                !beforeSwitches.Contains(requestedSwitch, StringComparer.OrdinalIgnoreCase)
                    ? null
                    : afterSwitches.SequenceEqual(beforeSwitches, StringComparer.OrdinalIgnoreCase)
                        ? "not-applied"
                        : "ambiguous-adapter-state";
        }
        else
        {
            classification = IsManagedVm(matching[0]) ? null : "not-applied";
        }

        if (classification is not null)
        {
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
                ["schema"] = schema,
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

    private static JsonElement[] MatchingVms(JsonElement data, string vmName)
    {
        return DesktopNodeApiJsonReader.EnumerateVmList(data)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string[] ConnectedSwitches(JsonElement vm)
    {
        return vm.ValueKind == JsonValueKind.Object &&
            vm.TryGetProperty("network", out var network) &&
            network.ValueKind == JsonValueKind.Array
                ? network.EnumerateArray()
                    .Select(adapter => DesktopNodeApiJsonReader.GetStringProperty(adapter, "switch"))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!)
                    .ToArray()
                : [];
    }
}
