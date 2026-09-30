using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// checkpoint.delete 는 큐 등록 때 대상 checkpoint 하나의 identity(vm_name, name, created_at)를 잡는다.
// 조정은 같은 이름의 checkpoint 가 readback 에서 사라졌을 때만 성공이다. 같은 이름이 남아 있으면
// 원래 checkpoint 인지 새로 만든 것인지 모두 운영자 확인 대상이다.
internal sealed partial class DesktopNodeApiJobReconciliationHandler
{
    private const string CheckpointDeleteReconciliationSchema = "pcv-checkpoint-delete-reconciliation/v1";

    public JsonElement BuildCheckpointDeleteParameters(
        string vmName,
        string checkpointName,
        CancellationToken cancellationToken)
    {
        var reconciliation = CaptureCheckpointDeleteBaseline(vmName, checkpointName, cancellationToken);
        return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["checkpoint_name"] = checkpointName,
            ["vm_name"] = vmName,
            ["reconciliation"] = reconciliation
        });
    }

    private JsonElement CaptureCheckpointDeleteBaseline(
        string vmName,
        string checkpointName,
        CancellationToken cancellationToken)
    {
        var expectedAfter = new SortedDictionary<string, object?>
        {
            ["state"] = "absent",
            ["name"] = checkpointName,
            ["vm_name"] = vmName
        };

        JsonElement Unavailable(string code) => DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema"] = CheckpointDeleteReconciliationSchema,
            ["capture_status"] = "unavailable",
            ["capture_error_code"] = code,
            ["before"] = null,
            ["expected_after"] = expectedAfter
        });

        try
        {
            using var readbackTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            readbackTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, hardeningOptions.RouteTimeoutSeconds)));
            var readback = operationInvoker.Invoke(
                "checkpoint.list",
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?> { ["vm_name"] = vmName }),
                readbackTimeout.Token);
            if (!readback.Ok || readback.Data is null)
            {
                return Unavailable(readback.Error?.Code ?? "PCV_CHECKPOINT_LIST_FAILED");
            }

            var matching = MatchingCheckpoints(readback.Data.Value, vmName, checkpointName);
            if (matching.Length != 1)
            {
                return Unavailable(matching.Length == 0 ? "PCV_CHECKPOINT_NOT_FOUND" : "PCV_CHECKPOINT_IDENTITY_AMBIGUOUS");
            }

            return DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema"] = CheckpointDeleteReconciliationSchema,
                ["capture_status"] = "captured",
                ["before"] = matching[0].Clone(),
                ["expected_after"] = expectedAfter
            });
        }
        catch (Exception)
        {
            return Unavailable("PCV_CHECKPOINT_LIST_FAILED");
        }
    }

    private static bool TryReadCapturedCheckpointDeleteBaseline(
        JsonElement? metadata,
        string vmName,
        string checkpointName,
        out CheckpointDeleteBaseline baseline)
    {
        baseline = null!;
        if (metadata is null || metadata.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var value = metadata.Value;
        var before = DesktopNodeApiJsonReader.ReadElement(value, "before");
        var expectedAfter = DesktopNodeApiJsonReader.ReadElement(value, "expected_after");
        if (!string.Equals(DesktopNodeApiJsonReader.ReadString(value, "schema"), CheckpointDeleteReconciliationSchema, StringComparison.Ordinal) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(value, "capture_status"), "captured", StringComparison.Ordinal) ||
            before is null ||
            expectedAfter is null ||
            before.Value.ValueKind != JsonValueKind.Object ||
            !string.Equals(DesktopNodeApiJsonReader.GetStringProperty(before.Value, "name"), checkpointName, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(DesktopNodeApiJsonReader.GetStringProperty(before.Value, "vm_name"), vmName, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(DesktopNodeApiJsonReader.ReadString(expectedAfter.Value, "state"), "absent", StringComparison.Ordinal))
        {
            return false;
        }

        baseline = new CheckpointDeleteBaseline(
            before.Value.Clone(),
            DesktopNodeApiJsonReader.GetStringProperty(before.Value, "created_at"),
            expectedAfter.Value.Clone());
        return true;
    }

    private DesktopNodeApiResponse ReconcileCheckpointDeleteJob(
        DesktopNodeJobSnapshot job,
        CancellationToken cancellationToken)
    {
        var jobId = job.JobId;
        var vmName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name");
        var checkpointName = DesktopNodeApiJsonReader.ReadString(job.Parameters, "checkpoint_name");
        var metadata = DesktopNodeApiJsonReader.ReadElement(job.Parameters, "reconciliation");

        DesktopNodeApiResponse Required(string classification, string detail) => RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(
                false,
                classification,
                null,
                ReconciliationRequiredError(jobId, classification, detail, "checkpoint.delete"))));

        if (string.IsNullOrWhiteSpace(vmName) || string.IsNullOrWhiteSpace(checkpointName) ||
            !TryReadCapturedCheckpointDeleteBaseline(metadata, vmName, checkpointName, out var baseline))
        {
            return Required(
                "baseline-unavailable",
                "The durable checkpoint.delete baseline was not captured or is not structurally valid.");
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
            return Required(
                "readback-unavailable",
                $"Provider checkpoint.list readback failed with {providerCode}; no mutation was attempted.");
        }

        var matching = MatchingCheckpoints(readback.Data.Value, vmName, checkpointName);
        if (matching.Length == 1)
        {
            var sameIdentity = string.Equals(
                DesktopNodeApiJsonReader.GetStringProperty(matching[0], "created_at"),
                baseline.CreatedAt,
                StringComparison.Ordinal);
            return Required(
                sameIdentity ? "not-applied" : "replacement-observed",
                sameIdentity
                    ? "Provider checkpoint.list readback still shows the captured checkpoint."
                    : "Provider checkpoint.list readback shows a checkpoint with the same name but a different created_at.");
        }

        if (matching.Length > 1)
        {
            return Required(
                "ambiguous-duplicate-names",
                "Provider checkpoint.list readback shows more than one checkpoint with the captured name.");
        }

        var result = DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
        {
            ["action"] = "reconciled",
            ["operation"] = "checkpoint.delete",
            ["reconciliation"] = new SortedDictionary<string, object?>
            {
                ["schema"] = CheckpointDeleteReconciliationSchema,
                ["classification"] = "postcondition-confirmed",
                ["before"] = baseline.Before,
                ["expected_after"] = baseline.ExpectedAfter,
                ["observed"] = null
            }
        });
        return RenderReconciliationResult(jobRuntime.Reconcile(
            jobId,
            new DesktopNodeJobReconciliationAssessment(true, "postcondition-confirmed", result)));
    }

    private sealed record CheckpointDeleteBaseline(
        JsonElement Before,
        string? CreatedAt,
        JsonElement ExpectedAfter);
}
