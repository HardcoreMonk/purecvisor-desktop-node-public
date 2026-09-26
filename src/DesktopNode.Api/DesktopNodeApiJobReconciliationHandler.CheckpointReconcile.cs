using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private DesktopNodeApiResponse ReconcileCheckpointCreateJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name");
        var checkpointName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "checkpoint_name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            string.IsNullOrWhiteSpace(checkpointName) ||
            !TryReadCapturedCheckpointCreateBaseline(metadata, vmName, checkpointName, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable checkpoint.create baseline was not captured or is not structurally valid.",
                    "checkpoint.create"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke(
            "checkpoint.list",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?> { ["vm_name"] = vmName }),
            readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_CHECKPOINT_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider checkpoint.list readback failed with {providerCode}; no mutation was attempted.",
                    "checkpoint.create"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = DesktopNodeApiJsonReader.EnumerateCheckpointList(readback.Data.Value)
            .Where(checkpoint =>
                string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "name"), checkpointName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "vm_name"), vmName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matching.Length == 1)
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "checkpoint.create",
                ["reconciliation"] = new SortedDictionary<string, object?>
                {
                    ["schema"] = baseline.Schema,
                    ["classification"] = "postcondition-confirmed",
                    ["before"] = baseline.Before,
                    ["expected_before"] = new SortedDictionary<string, object?>
                    {
                        ["state"] = "absent",
                        ["name"] = checkpointName,
                        ["vm_name"] = vmName
                    },
                    ["expected_after"] = new SortedDictionary<string, object?>
                    {
                        ["state"] = "present",
                        ["name"] = checkpointName,
                        ["vm_name"] = vmName
                    },
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
            : "ambiguous-duplicate-checkpoint-names";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider checkpoint.list readback did not prove exactly one checkpoint with the captured absent pre-state.",
                "checkpoint.create"));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
    }

    private DesktopNodeApiResponse ReconcileCheckpointRestoreJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name");
        var checkpointName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "checkpoint_name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");
        if (string.IsNullOrWhiteSpace(vmName) ||
            string.IsNullOrWhiteSpace(checkpointName) ||
            !TryReadCapturedCheckpointRestoreBaseline(metadata, vmName, checkpointName, out var baseline))
        {
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "baseline-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "baseline-unavailable",
                    "The durable checkpoint.restore baseline was not captured or is not structurally valid.",
                    "checkpoint.restore"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
        var readback = operationInvoker.Invoke(
            "checkpoint.list",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?> { ["vm_name"] = vmName }),
            readbackTimeout.Token);
        if (!readback.Ok || readback.Data is null)
        {
            var providerCode = readback.Error?.Code ?? "PCV_CHECKPOINT_LIST_FAILED";
            var assessment = new DesktopNodeJobReconciliationAssessment(
                false,
                "readback-unavailable",
                null,
                ReconciliationRequiredError(
                    jobId,
                    "readback-unavailable",
                    $"Provider checkpoint.list readback failed with {providerCode}; no mutation was attempted.",
                    "checkpoint.restore"));
            return RenderReconciliationResult(jobRuntime.Reconcile(jobId, assessment));
        }

        var matching = MatchingCheckpoints(readback.Data.Value, vmName, checkpointName);
        var currentTrue = CurrentTrueCheckpoints(readback.Data.Value);
        if (matching.Length == 1 && currentTrue.Length == 1 && ReadIsCurrent(matching[0]) == true)
        {
            var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["action"] = "reconciled",
                ["operation"] = "checkpoint.restore",
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

        var classification = matching.Length == 0
            ? "not-applied"
            : matching.Length > 1
                ? "ambiguous-duplicate-checkpoint-names"
                : ReadIsCurrent(matching[0]) == false
                    ? "not-applied"
                    : "current-unavailable";
        var requiredAssessment = new DesktopNodeJobReconciliationAssessment(
            false,
            classification,
            null,
            ReconciliationRequiredError(
                jobId,
                classification,
                "Provider checkpoint.list readback did not prove the requested checkpoint is uniquely current.",
                "checkpoint.restore"));
        return RenderReconciliationResult(jobRuntime.Reconcile(jobId, requiredAssessment));
    }
}
