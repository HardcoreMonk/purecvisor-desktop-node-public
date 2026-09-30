using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 전원 상태 전이 job(vm.start, vm.poweroff)은 같은 규칙으로 조정한다. 큐 등록 때 vm.list readback 으로
// before-state 와 identity fingerprint 를 잡고, 조정 때 같은 identity 의 VM 이 기대 전원 상태에 있을 때만 성공이다.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private const string VmPowerStateReconciliationSchema = "pcv-vm-power-state-reconciliation/v1";

    internal static string? ExpectedPowerState(string? operation)
    {
        return operation switch
        {
            "vm.start" => "running",
            "vm.poweroff" => "off",
            _ => null
        };
    }

    public JsonElement BuildVmPowerStateParameters(
        string operation,
        string vmName,
        CancellationToken cancellationToken)
    {
        var expectedState = ExpectedPowerState(operation)
            ?? throw new ArgumentException($"Operation '{operation}' has no power-state reconciliation.", nameof(operation));
        var reconciliation = CaptureVmPowerStateBaseline(operation, vmName, expectedState, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["reconciliation"] = reconciliation
        });
    }

    private JsonElement CaptureVmPowerStateBaseline(
        string operation,
        string vmName,
        string expectedState,
        CancellationToken cancellationToken)
    {
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["name"] = vmName,
            ["state"] = expectedState
        };

        JsonElement Unavailable(string code) => DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema"] = VmPowerStateReconciliationSchema,
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
            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = VmPowerStateReconciliationSchema,
                ["operation"] = operation,
                ["capture_status"] = "captured",
                ["before"] = before,
                ["before_fingerprint"] = BuildVmShutdownIdentityFingerprint(before),
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return Unavailable("PCV_VM_LIST_FAILED");
        }
    }

    private static bool TryReadCapturedVmPowerStateBaseline(
        JsonElement? metadata,
        string operation,
        string vmName,
        out VmPowerStateBaseline baseline)
    {
        baseline = null!;
        var expectedState = ExpectedPowerState(operation);
        if (expectedState is null || metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), VmPowerStateReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "operation"), operation, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal))
        {
            return false;
        }

        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var beforeFingerprint = DesktopNodeApiJsonReader.ReadElement(value, "before_fingerprint");
        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(value, "expected_after");
        if (before is null ||
            beforeFingerprint is null ||
            expectedAfter is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            beforeFingerprint.Value.ValueKind != JsonValueKind.Object ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "name"), vmName, StringComparison.Ordinal) ||
            !string.Equals(NormalizePowerState(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "state")), expectedState, StringComparison.Ordinal))
        {
            return false;
        }

        baseline = new VmPowerStateBaseline(
            VmPowerStateReconciliationSchema,
            before.Value.Clone(),
            beforeFingerprint.Value.Clone(),
            NormalizePowerState(DesktopNodeApiJsonReader.GetStringProperty(before.Value, "state")),
            expectedState,
            expectedAfter.Value.Clone());
        return true;
    }

    private DesktopNodeApiResponse ReconcileVmPowerStateJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var operation = job.Operation;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");

        DesktopNodeApiResponse Required(string classification, string detail) => RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(jobId, classification, detail, operation))));

        if (string.IsNullOrWhiteSpace(vmName) ||
            !TryReadCapturedVmPowerStateBaseline(metadata, operation, vmName, out var baseline))
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
        if (matching.Length != 1 || !ShutdownIdentityMatches(baseline.BeforeFingerprint, matching[0]))
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

        var observedState = NormalizePowerState(DesktopNodeApiJsonReader.GetStringProperty(matching[0], "state"));
        if (!string.Equals(observedState, baseline.ExpectedState, StringComparison.Ordinal))
        {
            var classification = string.Equals(observedState, baseline.BeforeState, StringComparison.Ordinal)
                ? "not-applied"
                : "incomplete-power-state";
            return Required(
                classification,
                $"Provider vm.list readback did not prove the captured VM is {baseline.ExpectedState} after {operation}.");
        }

        var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["action"] = "reconciled",
            ["operation"] = operation,
            ["reconciliation"] = new SortedDictionary<string, object?>
            {
                ["schema"] = baseline.Schema,
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

    private sealed record VmPowerStateBaseline(
        string Schema,
        JsonElement Before,
        JsonElement BeforeFingerprint,
        string BeforeState,
        string ExpectedState,
        JsonElement ExpectedAfter);
}
