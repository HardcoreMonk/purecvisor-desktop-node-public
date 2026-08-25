using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 큐에 올라온 job 을 하나 꺼내 provider 를 호출하고 결과를 확정하는 tick 을 소유한다.
// 읽는 사람이 놓치면 안 되는 두 가지가 있다.
//
// 1) sync 잠금 객체를 DesktopNodeApiRequestProcessor 와 공유한다. 이 잠금은 요청 처리와 worker
//    tick 사이의 상호 배제이므로, 소유자가 자기 잠금을 새로 만들면 배제가 깨진다. 그래서 잠금을
//    생성자로 받는다.
// 2) BeforeJobFinalization 은 Action? 이라 이 코드베이스의 callback-free 소유자 규칙과 충돌한다.
//    도메인 협력자가 아니라 provider 결과와 직렬화된 finalization 사이 경계를 결정적으로 만드는
//    테스트 seam 이므로 규칙의 유일한 예외로 남기고, guard 가 "정확히 이것 하나"임을 잠근다.
internal sealed class DesktopNodeApiJobWorker
{
    private readonly DesktopNodeJobRuntime jobRuntime;
    private readonly IDesktopNodeApiCancellationScopeFactory cancellationScopes;
    private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;
    private readonly object sync;

    public DesktopNodeApiJobWorker(
        DesktopNodeJobRuntime jobRuntime,
        IDesktopNodeApiCancellationScopeFactory cancellationScopes,
        DesktopNodeApiHyperVOperationInvoker operationInvoker,
        object sync)
    {
        this.jobRuntime = jobRuntime;
        this.cancellationScopes = cancellationScopes;
        this.operationInvoker = operationInvoker;
        this.sync = sync;
    }

    // Deterministic test seam for the provider-result/serialized-finalization boundary.
    public Action? BeforeJobFinalization { get; set; }

    public async Task<DesktopNodeApiWorkerTickResult> ProcessOneQueuedJobAsync(CancellationToken cancellationToken = default)
    {
        using var jobCancellation = cancellationScopes.CreateLinkedJobScope(cancellationToken);
        DesktopNodeStartedJob? started;
        DesktopNodeJobRuntimeSnapshot stateAfterStart;
        lock (sync)
        {
            started = jobRuntime.TryStartNext(jobCancellation.Cancel);
            stateAfterStart = jobRuntime.Snapshot();
        }

        if (started is null)
        {
            if (stateAfterStart.LoadBlock is not null || stateAfterStart.Queue.Count > 0)
            {
                var queuedJob = stateAfterStart.Queue.Count == 0
                    ? null
                    : stateAfterStart.Jobs.FirstOrDefault(job =>
                        string.Equals(job.JobId, stateAfterStart.Queue[0], StringComparison.Ordinal));
                return new DesktopNodeApiWorkerTickResult(
                    false,
                    queuedJob is null ? null : DesktopNodeApiResponseFactory.JobData(queuedJob),
                    stateAfterStart.LoadBlock is null
                        ? DesktopNodeApiErrorMapping.JobStoreCommitError(
                            DesktopNodeJobStoreCommitOutcome.NotCommitted,
                            "job start transition",
                            jobRuntime.LoadBlock)
                        : DesktopNodeApiErrorMapping.ToApiError(stateAfterStart.LoadBlock));
            }

            return new DesktopNodeApiWorkerTickResult(false, null, null);
        }

        DesktopNodeHyperVOperationResult result;
        try
        {
            result = await Task.Run(
                () => operationInvoker.Invoke(started.Operation, started.Parameters, jobCancellation.Token),
                jobCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (jobCancellation.IsCancellationRequested)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                started.Operation,
                "PCV_NATIVE_OPERATION_CANCELED",
                "The native operation was canceled before it completed.",
                "Cancellation was requested while the background worker was running the native operation.",
                retryable: true);
        }
        catch (Exception ex)
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                started.Operation,
                "PCV_NATIVE_OPERATION_FAILED",
                "The native operation failed.",
                ex.Message,
                retryable: true);
        }
        BeforeJobFinalization?.Invoke();
        DesktopNodeJobCompletionResult completion;
        lock (sync)
        {
            try
            {
                completion = jobRuntime.Complete(started, DesktopNodeApiErrorMapping.ToJobExecutionOutcome(result));
            }
            catch (DesktopNodeJobStoreCommitException exception)
            {
                var current = jobRuntime.Get(started.JobId).Job;
                return new DesktopNodeApiWorkerTickResult(
                    false,
                    current is null ? null : DesktopNodeApiResponseFactory.JobData(current),
                    DesktopNodeApiErrorMapping.JobStoreCommitError(exception.Outcome, "job completion transition", jobRuntime.LoadBlock));
            }
            finally
            {
                jobRuntime.DetachRunningCancellation(started.JobId);
            }

            if (!completion.Processed && jobRuntime.LoadBlock is not null)
            {
                var current = jobRuntime.Get(started.JobId).Job;
                return new DesktopNodeApiWorkerTickResult(
                    false,
                    current is null ? null : DesktopNodeApiResponseFactory.JobData(current),
                    DesktopNodeApiErrorMapping.ToApiError(jobRuntime.LoadBlock));
            }
        }

        return new DesktopNodeApiWorkerTickResult(
            completion.Processed,
            completion.Job is null ? null : DesktopNodeApiResponseFactory.JobData(completion.Job),
            null);
    }
}
