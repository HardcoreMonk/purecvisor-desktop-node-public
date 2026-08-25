using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 이 클래스는 런타임/프로바이더 오류를 API 오류로 매핑하는 책임만 담당하며,
// DesktopNodeApiRequestProcessor에서 그대로 추출되었다.
internal static class DesktopNodeApiErrorMapping
{
    internal static JobError? ToRuntimeError(DesktopNodeHyperVError? error)
    {
        return error is null
            ? null
            : new JobError(error.Code, error.Message, error.Detail, error.Retryable);
    }

    internal static DesktopNodeApiError? ToApiError(DesktopNodeJobRuntimeError? error)
    {
        return error is null
            ? null
            : new DesktopNodeApiError(
                error.Code,
                error.Message,
                error.Detail,
                error.Retryable,
                error.RecommendedAction);
    }

    internal static DesktopNodeApiError JobStoreCommitError(
        DesktopNodeJobStoreCommitOutcome outcome,
        string transition,
        DesktopNodeJobRuntimeError? loadBlock)
    {
        if (loadBlock is not null)
        {
            return ToApiError(loadBlock)!;
        }

        return new DesktopNodeApiError(
            "PCV_JOB_STORE_SAVE_FAILED",
            "The job state transition could not be saved.",
            outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate
                ? $"The {transition} has an indeterminate durable outcome. Job mutation is blocked until the authoritative snapshot is reconciled."
                : $"The {transition} was not committed. The previous durable job state remains authoritative, and no uncommitted candidate state was published.",
            false,
            "Restore job-store write access, inspect the current job by its correlation or job ID, and retry only after confirming the previous durable state.");
    }

    internal static DesktopNodeJobExecutionOutcome ToJobExecutionOutcome(DesktopNodeHyperVOperationResult result)
    {
        return new DesktopNodeJobExecutionOutcome(
            result.Ok,
            JsonSerializer.SerializeToElement(result, RuntimePolicyContract.JsonOptions),
            ToRuntimeError(result.Error),
            CancellationAcknowledged: string.Equals(
                result.Error?.Code,
                "PCV_NATIVE_OPERATION_CANCELED",
                StringComparison.Ordinal));
    }
}
