using System.Collections.Concurrent;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Runtime;

public sealed partial class DesktopNodeJobRuntime
{
    public DesktopNodeJobCommandResult Cancel(string jobId)
    {
        Action? requestCancellationToSignal = null;
        DesktopNodeJobCommandResult? committedResult = null;
        lock (stateSync)
        {
            if (loadBlock is not null)
            {
                return Rejected(loadBlock);
            }

            if (!jobs.TryGetValue(jobId, out var job))
            {
                return NotFound(jobId);
            }

            if (string.Equals(job.Status, "running", StringComparison.Ordinal) &&
                IsRunningInterruptEligible(job.Operation))
            {
                if (!string.IsNullOrWhiteSpace(job.CanceledAt) &&
                    string.Equals(job.Error?.Code, "PCV_JOB_CANCEL_REQUESTED", StringComparison.Ordinal))
                {
                    return new DesktopNodeJobCommandResult(
                        DesktopNodeJobCommandOutcome.CancellationRequested,
                        Project(job),
                        null);
                }

                if (!runningCancellations.TryGetValue(jobId, out var requestCancellation))
                {
                    return Rejected(new DesktopNodeJobRuntimeError(
                        "PCV_JOB_CANCEL_NOT_INTERRUPTIBLE",
                        $"Job '{jobId}' is running but cannot be interrupted.",
                        "The running job cancellation token is not registered, so the provider cannot be signaled safely.",
                        false));
                }

                var candidateJob = CloneJob(job);
                candidateJob.CanceledAt ??= Now();
                candidateJob.UpdatedAt = candidateJob.CanceledAt;
                candidateJob.Error = new DesktopNodeJobRuntimeError(
                    "PCV_JOB_CANCEL_REQUESTED",
                    $"Cancellation was requested for running job '{jobId}'.",
                    "The cancellation request is durable before the runtime attempts to signal the provider outside the state lock.",
                    false);
                var candidateJobs = CreateCandidateJobsUnsafe(candidateJob);
                var candidateQueue = new Queue<string>(queue);
                var candidatePrunedTerminalJobs = prunedTerminalJobs;
                EnforceRetention(candidateJobs, ref candidatePrunedTerminalJobs);
                CommitCandidateStateUnsafe(
                    candidateJobs,
                    candidateQueue,
                    candidatePrunedTerminalJobs);
                requestCancellationToSignal = requestCancellation;
                committedResult = new DesktopNodeJobCommandResult(
                    DesktopNodeJobCommandOutcome.CancellationRequested,
                    Project(candidateJob),
                    null);
            }
            else
            {
                var candidateJob = CloneJob(job);
                var runtimeJob = ToPolicyJob(candidateJob);
                if (!runtimeJob.Ok)
                {
                    return Rejected(runtimeJob.Error!);
                }

                var decision = JobStateTransitionPolicy.Cancel(runtimeJob.Job!);
                if (!decision.Ok)
                {
                    return Rejected(FromPolicyError(decision.Error)!);
                }

                ApplyPolicyJob(candidateJob, decision.Job!);
                candidateJob.CanceledAt = Now();
                candidateJob.UpdatedAt = candidateJob.CanceledAt;
                var candidateJobs = CreateCandidateJobsUnsafe(candidateJob);
                var candidateQueue = new Queue<string>(queue);
                RemoveFromQueue(candidateQueue, jobId);
                var candidatePrunedTerminalJobs = prunedTerminalJobs;
                EnforceRetention(candidateJobs, ref candidatePrunedTerminalJobs);
                CommitCandidateStateUnsafe(
                    candidateJobs,
                    candidateQueue,
                    candidatePrunedTerminalJobs);
                return new DesktopNodeJobCommandResult(
                    DesktopNodeJobCommandOutcome.Canceled,
                    Project(candidateJob),
                    null);
            }
        }

        try
        {
            requestCancellationToSignal!();
            return committedResult!;
        }
        catch (Exception)
        {
            var signalError = new DesktopNodeJobRuntimeError(
                "PCV_JOB_CANCEL_SIGNAL_FAILED",
                $"The durable cancellation request for job '{jobId}' could not be signaled to the provider.",
                "The job remains running with a durable cancellation request, but provider acknowledgement has not been observed.",
                false,
                "Inspect the running provider operation and job state. Do not submit a duplicate mutation; wait for terminal state or restart only after following the interrupted-job reconciliation procedure.");
            lock (stateSync)
            {
                if (jobs.TryGetValue(jobId, out var currentJob) &&
                    string.Equals(currentJob.Status, "running", StringComparison.Ordinal) &&
                    string.Equals(
                        currentJob.Error?.Code,
                        "PCV_JOB_CANCEL_REQUESTED",
                        StringComparison.Ordinal))
                {
                    cancelSignalAttentionJobIds.Add(jobId);
                }
                RecordObservationUnsafe(
                    "cancel-signal-failed",
                    signalError.Code,
                    DesktopNodeJobStoreCommitOutcome.Committed,
                    signalError.RecommendedAction);
            }

            return committedResult! with
            {
                Error = signalError
            };
        }
    }

    public DesktopNodeJobCommandResult Retry(
        string jobId,
        DesktopNodeJobRequestContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        lock (stateSync)
        {
            if (loadBlock is not null)
            {
                return Rejected(loadBlock);
            }

            if (!jobs.TryGetValue(jobId, out var job))
            {
                return NotFound(jobId);
            }

            var runtimeJob = ToPolicyJob(job);
            if (!runtimeJob.Ok)
            {
                return Rejected(runtimeJob.Error!);
            }

            var retryJobId = "job-" + Guid.NewGuid().ToString("N");
            var decision = JobStateTransitionPolicy.Retry(runtimeJob.Job!, retryJobId);
            if (!decision.Ok)
            {
                return Rejected(FromPolicyError(decision.Error)!);
            }

            var retryJob = CreateUnsafe(
                new DesktopNodeJobCreateCommand(
                    decision.Job!.Operation,
                    job.Parameters,
                    decision.Job.RetryOf,
                    decision.Job.Attempt,
                    decision.Job.JobId),
                new DesktopNodeJobRequestContext(context.RequestId, job.CorrelationId));
            return new DesktopNodeJobCommandResult(
                DesktopNodeJobCommandOutcome.Retried,
                retryJob,
                null);
        }
    }

    public DesktopNodeJobReconciliationResult Reconcile(
        string jobId,
        DesktopNodeJobReconciliationAssessment assessment)
    {
        ArgumentNullException.ThrowIfNull(assessment);

        lock (stateSync)
        {
            if (loadBlock is not null)
            {
                return new DesktopNodeJobReconciliationResult(
                    DesktopNodeJobReconciliationOutcome.Rejected,
                    null,
                    loadBlock);
            }

            if (!jobs.TryGetValue(jobId, out var job))
            {
                return new DesktopNodeJobReconciliationResult(
                    DesktopNodeJobReconciliationOutcome.NotFound,
                    null,
                    new DesktopNodeJobRuntimeError(
                        "PCV_JOB_NOT_FOUND",
                        $"Job '{jobId}' was not found.",
                        "The job was not found in the current memory store.",
                        false));
            }

            if (job.Status != "failed" ||
                !IsReconciliationSupportedOperation(job.Operation) ||
                !string.Equals(job.Error?.Code, "PCV_JOB_INTERRUPTED", StringComparison.Ordinal))
            {
                var rejected = ReconciliationRequiredError(
                    jobId,
                    assessment.Classification,
                    "Only an interrupted vm.rename, vm.delete, checkpoint.create, or checkpoint.restore job can be reconciled by this contract.",
                    job.Operation);
                RecordObservationUnsafe(
                    "job-reconciliation-required",
                    rejected.Code,
                    DesktopNodeJobStoreCommitOutcome.Committed,
                    rejected.RecommendedAction);
                return new DesktopNodeJobReconciliationResult(
                    DesktopNodeJobReconciliationOutcome.Required,
                    Project(job),
                    rejected);
            }

            if (!assessment.PostconditionConfirmed || assessment.Result is null)
            {
                var required = assessment.Error ?? ReconciliationRequiredError(
                    jobId,
                    assessment.Classification,
                    $"The provider readback did not prove the expected {job.Operation} postcondition.",
                    job.Operation);
                RecordObservationUnsafe(
                    "job-reconciliation-required",
                    required.Code,
                    DesktopNodeJobStoreCommitOutcome.Committed,
                    required.RecommendedAction);
                return new DesktopNodeJobReconciliationResult(
                    DesktopNodeJobReconciliationOutcome.Required,
                    Project(job),
                    required);
            }

            var candidateJob = CloneJob(job);
            candidateJob.Status = "succeeded";
            candidateJob.Result = assessment.Result.Value.Clone();
            candidateJob.Error = null;
            candidateJob.UpdatedAt = Now();
            var candidateJobs = CreateCandidateJobsUnsafe(candidateJob);
            var candidateQueue = new Queue<string>(queue);
            var candidatePrunedTerminalJobs = prunedTerminalJobs;
            EnforceRetention(candidateJobs, ref candidatePrunedTerminalJobs);
            try
            {
                CommitCandidateStateUnsafe(
                    candidateJobs,
                    candidateQueue,
                    candidatePrunedTerminalJobs);
            }
            catch (DesktopNodeJobStoreCommitException exception)
            {
                loadBlock = CompletionPersistenceBlock(exception.Outcome);
                RecordObservationUnsafe(
                    "completion-persistence-failed",
                    loadBlock.Code,
                    exception.Outcome,
                    loadBlock.RecommendedAction);
                throw;
            }

            RecordObservationUnsafe(
                "job-reconciled",
                "PCV_JOB_RECONCILED",
                DesktopNodeJobStoreCommitOutcome.Committed,
                $"The {job.Operation} postcondition was confirmed from provider readback; no duplicate mutation was submitted.");
            return new DesktopNodeJobReconciliationResult(
                DesktopNodeJobReconciliationOutcome.Reconciled,
                Project(candidateJob),
                null);
        }
    }

    public DesktopNodeStartedJob? TryStartNext(Action requestCancellation)
    {
        ArgumentNullException.ThrowIfNull(requestCancellation);

        lock (stateSync)
        {
            if (loadBlock is not null)
            {
                return null;
            }

            while (queue.Count > 0)
            {
                var jobId = queue.Peek();
                if (!jobs.TryGetValue(jobId, out var job) || job.Status != "queued")
                {
                    queue.Dequeue();
                    continue;
                }

                var candidateJob = CloneJob(job);
                var candidateJobs = CreateCandidateJobsUnsafe(candidateJob);
                var candidateQueue = new Queue<string>(queue);
                candidateQueue.Dequeue();
                DesktopNodeStartedJob? startedJob = null;
                var runtimeJob = ToPolicyJob(candidateJob);
                if (!runtimeJob.Ok)
                {
                    candidateJob.Status = "failed";
                    candidateJob.Result = null;
                    candidateJob.Error = runtimeJob.Error;
                    candidateJob.UpdatedAt = Now();
                }
                else
                {
                    var started = JobStateTransitionPolicy.Start(runtimeJob.Job!);
                    if (!started.Ok)
                    {
                        candidateJob.Status = "failed";
                        candidateJob.Result = null;
                        candidateJob.Error = FromPolicyError(started.Error);
                        candidateJob.UpdatedAt = Now();
                    }
                    else
                    {
                        ApplyPolicyJob(candidateJob, started.Job!);
                        candidateJob.UpdatedAt = Now();
                        startedJob = new DesktopNodeStartedJob(
                            candidateJob.JobId,
                            candidateJob.Operation,
                            candidateJob.Parameters.Clone());
                    }
                }

                var candidatePrunedTerminalJobs = prunedTerminalJobs;
                EnforceRetention(candidateJobs, ref candidatePrunedTerminalJobs);
                try
                {
                    CommitCandidateStateUnsafe(
                        candidateJobs,
                        candidateQueue,
                        candidatePrunedTerminalJobs);
                }
                catch (DesktopNodeJobStoreCommitException)
                {
                    return null;
                }

                if (startedJob is null)
                {
                    continue;
                }

                runningCancellations[candidateJob.JobId] = requestCancellation;
                return startedJob;
            }

            return null;
        }
    }

    public void DetachRunningCancellation(string jobId)
    {
        lock (stateSync)
        {
            runningCancellations.Remove(jobId);
        }
    }

    public DesktopNodeJobCompletionResult Complete(
        DesktopNodeStartedJob started,
        DesktopNodeJobExecutionOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(started);
        ArgumentNullException.ThrowIfNull(outcome);

        lock (stateSync)
        {
            if (loadBlock is not null)
            {
                return new DesktopNodeJobCompletionResult(false, null);
            }

            if (!jobs.TryGetValue(started.JobId, out var job))
            {
                return new DesktopNodeJobCompletionResult(false, null);
            }

            var candidateJob = CloneJob(job);
            var candidateJobs = CreateCandidateJobsUnsafe(candidateJob);
            var candidateQueue = new Queue<string>(queue);
            var runtimeJob = ToPolicyJob(candidateJob);
            if (!runtimeJob.Ok)
            {
                candidateJob.Status = "failed";
                candidateJob.Result = null;
                candidateJob.Error = runtimeJob.Error;
                candidateJob.UpdatedAt = Now();
            }
            else if (!string.IsNullOrWhiteSpace(candidateJob.CanceledAt) && outcome.CancellationAcknowledged)
            {
                candidateJob.Status = "canceled";
                candidateJob.Result = outcome.ProviderResult.Clone();
                candidateJob.Error = new DesktopNodeJobRuntimeError(
                    "PCV_JOB_CANCELED",
                    $"Job '{candidateJob.JobId}' was canceled while running.",
                    "The guest execution provider acknowledged the running cancellation request before the job completed.",
                    false);
                candidateJob.UpdatedAt = Now();
            }
            else
            {
                var completed = JobStateTransitionPolicy.Complete(
                    runtimeJob.Job!,
                    outcome.Ok
                        ? HelperExecutionResult.Success(outcome.ProviderResult.Clone())
                        : HelperExecutionResult.Failure(outcome.Error ?? new JobError(
                            "PCV_NATIVE_OPERATION_FAILED",
                            "The native operation failed.",
                            "The native adapter returned a failed result without an error payload.",
                            Retryable: true)));
                if (!completed.Ok)
                {
                    candidateJob.Status = "failed";
                    candidateJob.Result = null;
                    candidateJob.Error = FromPolicyError(completed.Error);
                }
                else
                {
                    ApplyPolicyJob(candidateJob, completed.Job!);
                }

                candidateJob.UpdatedAt = Now();
            }

            var candidatePrunedTerminalJobs = prunedTerminalJobs;
            EnforceRetention(candidateJobs, ref candidatePrunedTerminalJobs);
            try
            {
                CommitCandidateStateUnsafe(
                    candidateJobs,
                    candidateQueue,
                    candidatePrunedTerminalJobs);
            }
            catch (DesktopNodeJobStoreCommitException exception)
            {
                loadBlock = CompletionPersistenceBlock(exception.Outcome);
                RecordObservationUnsafe(
                    "completion-persistence-failed",
                    loadBlock.Code,
                    exception.Outcome,
                    loadBlock.RecommendedAction);
                throw;
            }
            cancelSignalAttentionJobIds.Remove(started.JobId);
            return new DesktopNodeJobCompletionResult(true, Project(candidateJob));
        }
    }
}
