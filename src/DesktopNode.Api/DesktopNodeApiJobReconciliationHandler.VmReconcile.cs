using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private DesktopNodeApiResponse ReconcileVmDeleteJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            !TryReadCapturedDeleteBaseline(metadata, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable vm.delete baseline was not captured or is not structurally valid.",
                    "vm.delete"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_VM_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider vm.list readback failed with {providerCode}; no mutation was attempted.",
                    "vm.delete"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matching.Length == 0)
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "vm.delete",
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = baseline.Schema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = baseline.Before,
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    },
                    ["observed"] = new SortedDictionary<string, object?>
                    {
                        ["name"] = vmName,
                        ["state"] = "absent"
                    }
                }
            });
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    true,
                    "postcondition-confirmed",
                    result)));
        }

        var beforeId = DesktopNodeApiJsonReader.ReadString(baseline.Before, "id");
        var classification = matching.Length > 1
            ? "ambiguous-multiple-targets"
            : string.Equals(beforeId, DesktopNodeApiJsonReader.GetStringProperty(matching[0], "id"), StringComparison.Ordinal) &&
                IsManagedVm(matching[0])
                ? "not-applied"
                : IsManagedVm(matching[0])
                    ? "target-recreated-or-identity-changed"
                    : "target-name-collision-unmanaged";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider readback still contains the delete target or an ambiguous name collision; absence was not proven.",
                "vm.delete"));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
    }

    private DesktopNodeApiResponse ReconcileVmCreateJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            !TryReadCapturedVmCreateBaseline(metadata, vmName, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable vm.create baseline was not captured or is not structurally valid.",
                    "vm.create"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_VM_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider vm.list readback failed with {providerCode}; no mutation was attempted.",
                    "vm.create"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matching.Length == 1 && CreateFingerprintMatches(baseline, matching[0]))
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "vm.create",
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = baseline.Schema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = null,
                    ["expected_after"] = baseline.ExpectedAfter,
                    ["observed"] = matching[0]
                }
            });
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    true,
                    "postcondition-confirmed",
                    result)));
        }

        var classification = matching.Length == 0
            ? "not-applied"
            : matching.Length > 1
                ? "ambiguous-duplicate-names"
                : !IsManagedVm(matching[0])
                    ? "unmanaged-collision"
                    : "target-fingerprint-mismatch";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider vm.list readback did not prove exactly one managed VM with the captured create postcondition.",
                "vm.create"));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
    }

    private DesktopNodeApiResponse ReconcileVmShutdownJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            !TryReadCapturedVmShutdownBaseline(metadata, vmName, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable vm.shutdown baseline was not captured or is not structurally valid.",
                    "vm.shutdown"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_VM_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider vm.list readback failed with {providerCode}; no mutation was attempted.",
                    "vm.shutdown"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matching.Length == 1 &&
            ShutdownIdentityMatches(baseline.BeforeFingerprint, matching[0]))
        {
            var observedState = NormalizePowerState(DesktopNodeApiJsonReader.GetStringProperty(matching[0], "state"));
            if (observedState == "off")
            {
                var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["action"] = "reconciled",
                    ["operation"] = "vm.shutdown",
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
                    new DesktopNodeJobReconciliationAssessment(
                        true,
                        "postcondition-confirmed",
                        result)));
            }

            var sameAsBaseline = string.Equals(observedState, baseline.BeforeState, StringComparison.Ordinal);
            var classification = sameAsBaseline
                ? "not-applied"
                : "incomplete-power-state";
            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    false,
                    classification,
                    null,
                    ReconciliationRequiredError(
                        jobId,
                        classification,
                        "Provider vm.list readback did not prove the captured VM is Off.",
                        "vm.shutdown"))));
        }

        var identityClassification = matching.Length == 0
            ? "expected-target-not-observed"
            : matching.Length > 1
                ? "ambiguous-duplicate-names"
                : "identity-mismatch";
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                identityClassification,
                null,
                ReconciliationRequiredError(
                    jobId,
                    identityClassification,
                    "Provider vm.list readback did not prove a unique VM identity for shutdown reconciliation.",
                    "vm.shutdown"))));
    }

    private DesktopNodeApiResponse ReconcileVmRestartJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            !TryReadCapturedVmRestartBaseline(metadata, vmName, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable vm.restart baseline was not captured or is not structurally valid.",
                    "vm.restart"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke("vm.list", DesktopNodeApiResponseFactory.EmptyObject(), readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_VM_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider vm.list readback failed with {providerCode}; no mutation was attempted.",
                    "vm.restart"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = DesktopNodeApiJsonReader.EnumerateVmList(readback.Data.Value)
            .Where(vm => string.Equals(DesktopNodeApiJsonReader.GetStringProperty(vm, "name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matching.Length == 1 &&
            ShutdownIdentityMatches(baseline.BeforeFingerprint, matching[0]))
        {
            var observedState = NormalizePowerState(DesktopNodeApiJsonReader.GetStringProperty(matching[0], "state"));
            if (observedState == "off" || observedState != "running")
            {
                return RenderReconciliationResult(jobRuntime.Reconcile(
                    jobId,
                    new DesktopNodeJobReconciliationAssessment(
                        false,
                        "incomplete-power-state",
                        null,
                        ReconciliationRequiredError(
                            jobId,
                            "incomplete-power-state",
                            "Provider vm.list readback did not prove the captured VM is Running after restart.",
                            "vm.restart"))));
            }

            var observedLastPoweredOn = DesktopNodeApiJsonReader.GetStringProperty(matching[0], "last_powered_on");
            if (!TryReadTimestamp(baseline.LastPoweredOn, out var baselineTimestamp) ||
                !TryReadTimestamp(observedLastPoweredOn, out var observedTimestamp))
            {
                return RenderReconciliationResult(jobRuntime.Reconcile(
                    jobId,
                    new DesktopNodeJobReconciliationAssessment(
                        false,
                        "timestamp-unavailable",
                        null,
                        ReconciliationRequiredError(
                            jobId,
                            "timestamp-unavailable",
                            "Provider vm.list readback did not prove last_powered_on advanced after restart.",
                            "vm.restart"))));
            }

            if (observedTimestamp > baselineTimestamp)
            {
                var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["action"] = "reconciled",
                    ["operation"] = "vm.restart",
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
                    new DesktopNodeJobReconciliationAssessment(
                        true,
                        "postcondition-confirmed",
                        result)));
            }

            return RenderReconciliationResult(jobRuntime.Reconcile(
                jobId,
                new DesktopNodeJobReconciliationAssessment(
                    false,
                    "not-applied",
                    null,
                    ReconciliationRequiredError(
                        jobId,
                        "not-applied",
                        "Provider vm.list readback did not prove last_powered_on advanced after restart.",
                        "vm.restart"))));
        }

        var identityClassification = matching.Length == 0
            ? "expected-target-not-observed"
            : matching.Length > 1
                ? "ambiguous-duplicate-names"
                : "identity-mismatch";
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                identityClassification,
                null,
                ReconciliationRequiredError(
                    jobId,
                    identityClassification,
                    "Provider vm.list readback did not prove a unique VM identity for restart reconciliation.",
                    "vm.restart"))));
    }
}
