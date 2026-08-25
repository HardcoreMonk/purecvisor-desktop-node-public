using System.Collections.Concurrent;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Runtime;

public sealed partial class DesktopNodeJobRuntime
{
    private DesktopNodeJobSnapshot CreateUnsafe(
        DesktopNodeJobCreateCommand command,
        DesktopNodeJobRequestContext context)
    {
        if (loadBlock is not null)
        {
            throw new DesktopNodeJobStoreWriteException(
                loadBlock,
                DesktopNodeJobStoreCommitOutcome.Indeterminate,
                new DesktopNodeJobStoreCommitException(
                    DesktopNodeJobStoreCommitOutcome.Indeterminate,
                    "The job store is blocked because its authoritative snapshot is unresolved."));
        }

        var now = Now();
        var jobId = command.JobId ?? "job-" + Guid.NewGuid().ToString("N");
        var queued = DesktopNodeJob.CreateQueued(
            jobId,
            command.Operation,
            command.Parameters.Clone(),
            command.RetryOf,
            command.Attempt);
        var job = new MutableJob(
            queued.JobId,
            queued.Operation,
            ToPersistedStatus(queued.Status),
            command.Parameters.Clone(),
            null,
            null,
            queued.RetryOf,
            context.RequestId,
            context.CorrelationId ?? queued.RetryOf ?? queued.JobId,
            queued.Attempt,
            null,
            now,
            now);

        var candidateJobs = new Dictionary<string, MutableJob>(jobs, StringComparer.Ordinal)
        {
            [job.JobId] = job
        };
        var candidateQueue = new Queue<string>(queue);
        candidateQueue.Enqueue(job.JobId);
        var candidatePrunedTerminalJobs = prunedTerminalJobs;
        EnforceRetention(candidateJobs, ref candidatePrunedTerminalJobs);

        WriteCreateCandidateSnapshotUnsafe(candidateJobs.Values, candidateQueue);

        jobs = candidateJobs;
        queue = candidateQueue;
        prunedTerminalJobs = candidatePrunedTerminalJobs;
        return Project(job);
    }

    private Dictionary<string, MutableJob> CreateCandidateJobsUnsafe(MutableJob candidateJob)
    {
        return new Dictionary<string, MutableJob>(jobs, StringComparer.Ordinal)
        {
            [candidateJob.JobId] = candidateJob
        };
    }

    private void CommitCandidateStateUnsafe(
        Dictionary<string, MutableJob> candidateJobs,
        Queue<string> candidateQueue,
        int candidatePrunedTerminalJobs)
    {
        WriteSnapshotUnsafe(candidateJobs.Values, candidateQueue);
        jobs = candidateJobs;
        queue = candidateQueue;
        prunedTerminalJobs = candidatePrunedTerminalJobs;
    }

    private static MutableJob CloneJob(MutableJob source)
    {
        return new MutableJob(
            source.JobId,
            source.Operation,
            source.Status,
            source.Parameters,
            source.Result,
            source.Error,
            source.RetryOf,
            source.RequestId,
            source.CorrelationId,
            source.Attempt,
            source.CanceledAt,
            source.CreatedAt,
            source.UpdatedAt);
    }

    private static void RemoveFromQueue(Queue<string> candidateQueue, string jobId)
    {
        if (candidateQueue.Count == 0)
        {
            return;
        }

        var remaining = candidateQueue
            .Where(id => !string.Equals(id, jobId, StringComparison.Ordinal))
            .ToArray();
        candidateQueue.Clear();
        foreach (var id in remaining)
        {
            candidateQueue.Enqueue(id);
        }
    }

    private void WriteSnapshotUnsafe(
        IEnumerable<MutableJob> snapshotJobs,
        IEnumerable<string> snapshotQueue)
    {
        if (store is null)
        {
            return;
        }

        if (loadBlock is not null)
        {
            throw new DesktopNodeJobStoreCommitException(
                DesktopNodeJobStoreCommitOutcome.Indeterminate,
                "The job store is blocked because its authoritative snapshot is unresolved.");
        }

        var result = WriteStoreSnapshotUnsafe(
            SerializeSnapshotUnsafe(snapshotJobs, snapshotQueue));
        if (result.Outcome == DesktopNodeJobStoreCommitOutcome.Committed)
        {
            storeWriteAttentionRequired = false;
            return;
        }

        RecordPersistenceFailureUnsafe(result.Outcome);

        if (result.Outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate)
        {
            loadBlock = IndeterminateCommitBlock();
        }

        throw new DesktopNodeJobStoreCommitException(
            result.Outcome,
            "The job store did not commit the requested snapshot.",
            result.Failure);
    }

    private void WriteCreateCandidateSnapshotUnsafe(
        IEnumerable<MutableJob> snapshotJobs,
        IEnumerable<string> snapshotQueue)
    {
        if (store is null)
        {
            return;
        }

        var result = WriteStoreSnapshotUnsafe(
            SerializeSnapshotUnsafe(snapshotJobs, snapshotQueue));
        if (result.Outcome == DesktopNodeJobStoreCommitOutcome.Committed)
        {
            storeWriteAttentionRequired = false;
            return;
        }

        RecordPersistenceFailureUnsafe(result.Outcome);

        if (result.Outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate)
        {
            loadBlock = IndeterminateCommitBlock();
        }

        throw new DesktopNodeJobStoreWriteException(
            result.Outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate
                ? loadBlock!
                : new DesktopNodeJobRuntimeError(
                    "PCV_JOB_STORE_SAVE_FAILED",
                    "The job store did not acknowledge the queued job snapshot.",
                    "The candidate snapshot was not published to this process. The on-disk commit state requires recovery if the store failed after replacement.",
                    false,
                    "Inspect Desktop Node service diagnostics and confirm that no matching job exists before retrying; restore job-store write access if the write did not commit."),
            result.Outcome,
            result.Failure ?? new IOException("The job store returned a failed commit outcome."));
    }

    private DesktopNodeJobStoreWriteResult WriteStoreSnapshotUnsafe(string json)
    {
        try
        {
            return store!.WriteSnapshot(json);
        }
        catch (Exception exception)
        {
            return DesktopNodeJobStoreWriteResult.Indeterminate(exception);
        }
    }

    private static DesktopNodeJobRuntimeError IndeterminateCommitBlock()
    {
        return new DesktopNodeJobRuntimeError(
            "PCV_JOB_STORE_SAVE_FAILED",
            "The job store commit outcome is indeterminate.",
            "The candidate snapshot was not published to this process, and job-store mutation and dispatch are blocked until the pending commit is reconciled on restart.",
            false,
            "Stop job processing and preserve jobs.json plus its pending-commit guard for diagnostics. Restore directory access and restart Desktop Node; if it remains blocked, follow the Operations Guide pending-commit recovery procedure instead of deleting the guard or retrying the mutation.");
    }

    private static DesktopNodeJobRuntimeError CompletionPersistenceBlock(
        DesktopNodeJobStoreCommitOutcome outcome)
    {
        var detail = outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate
            ? "The terminal snapshot has an indeterminate commit outcome. The live job remains running, but the authoritative durable state is unknown until the pending guard is reconciled; the external side effect is unresolved and further job mutation and dispatch are blocked."
            : "The terminal snapshot was not committed. The live job and the confirmed previous durable job remain running, so the external side effect is unresolved and further job mutation and dispatch are blocked.";
        return new DesktopNodeJobRuntimeError(
            "PCV_JOB_STORE_SAVE_FAILED",
            "The provider result could not be durably recorded.",
            detail,
            false,
            "Preserve jobs.json and any pending-commit guard, inspect provider readback for the affected operation, and reconcile the job manually before restarting processing. Do not retry the mutation while its prior side effect is uncertain.");
    }

    private static DesktopNodeJobRuntimeError RecoveryPersistenceBlock(
        DesktopNodeJobStoreCommitOutcome outcome)
    {
        var detail = outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate
            ? "The interrupted-job recovery snapshot has an indeterminate commit outcome. The authoritative durable state is unknown until the pending guard is reconciled, and mutation and dispatch are blocked to prevent an uncertain external side effect from being replayed."
            : "The interrupted-job recovery snapshot was not committed. The confirmed previous persisted running state remains authoritative, and mutation and dispatch are blocked to prevent an uncertain external side effect from being replayed.";
        return new DesktopNodeJobRuntimeError(
            "PCV_JOB_STORE_SAVE_FAILED",
            "Interrupted job recovery could not be durably recorded.",
            detail,
            false,
            "Preserve jobs.json and any pending-commit guard, restore store access, and restart through the documented interrupted-job reconciliation procedure. Do not retry the provider mutation automatically.");
    }

    private static bool IsReconciliationSupportedOperation(string operation)
    {
        return string.Equals(operation, "vm.rename", StringComparison.Ordinal) ||
            string.Equals(operation, "vm.delete", StringComparison.Ordinal) ||
            string.Equals(operation, "checkpoint.create", StringComparison.Ordinal) ||
            string.Equals(operation, "checkpoint.restore", StringComparison.Ordinal);
    }

    private static DesktopNodeJobRuntimeError ReconciliationRequiredError(
        string jobId,
        string classification,
        string detail,
        string? operation = null)
    {
        var mutation = operation switch
        {
            "vm.delete" => "delete",
            "checkpoint.create" => "checkpoint create",
            "checkpoint.restore" => "checkpoint restore",
            _ => "rename"
        };
        return new DesktopNodeJobRuntimeError(
            "PCV_JOB_RECONCILIATION_REQUIRED",
            $"Job '{jobId}' requires operator reconciliation.",
            $"{detail} Classification: {classification}.",
            false,
            $"Inspect provider readback and Event Log/diagnostics, confirm whether the {mutation} applied, and do not submit a duplicate mutation until the side effect is known.");
    }

    private void RecordPersistenceFailureUnsafe(DesktopNodeJobStoreCommitOutcome outcome)
    {
        RecordObservationUnsafe(
            outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate
                ? "save-indeterminate"
                : "save-not-committed",
            "PCV_JOB_STORE_SAVE_FAILED",
            outcome,
            outcome == DesktopNodeJobStoreCommitOutcome.Indeterminate
                ? "Preserve jobs.json and its pending-commit guard, stop mutation processing, and restart only through the documented reconciliation procedure."
                : "Restore job-store write access and retry only the control request; never replay an uncertain provider mutation automatically.");
    }

    private void RecordObservationUnsafe(
        string eventName,
        string code,
        DesktopNodeJobStoreCommitOutcome? outcome,
        string? recommendedAction)
    {
        var observation = new DesktopNodeJobRuntimeObservation(
            eventName,
            code,
            outcome?.ToString().ToLowerInvariant(),
            DateTimeOffset.UtcNow.ToString("o"),
            recommendedAction);
        recentObservations.Add(observation);
        if (recentObservations.Count > MaxRecentObservations)
        {
            recentObservations.RemoveRange(0, recentObservations.Count - MaxRecentObservations);
        }

        switch (eventName)
        {
            case "save-not-committed":
            case "save-indeterminate":
            case "running-recovery-persistence-failed":
            case "completion-persistence-failed":
                storeWriteAttentionRequired = true;
                break;
            case "running-recovered":
                storeRecovered = true;
                break;
        }

        QueueSinkObservation(observation);
    }

    private void QueueSinkObservation(DesktopNodeJobRuntimeObservation observation)
    {
        if (eventSink is null)
        {
            return;
        }

        while (true)
        {
            var pending = Volatile.Read(ref pendingSinkObservationCount);
            if (pending >= MaxRecentObservations)
            {
                return;
            }

            if (Interlocked.CompareExchange(
                    ref pendingSinkObservationCount,
                    pending + 1,
                    pending) == pending)
            {
                break;
            }
        }

        pendingSinkObservations.Enqueue(observation);
        ScheduleSinkDrain();
    }

    private void ScheduleSinkDrain()
    {
        if (Interlocked.CompareExchange(ref sinkDrainScheduled, 1, 0) != 0)
        {
            return;
        }

        ThreadPool.QueueUserWorkItem(
            static state => ((DesktopNodeJobRuntime)state!).DrainSinkObservations(),
            this,
            preferLocal: false);
    }

    private void DrainSinkObservations()
    {
        while (pendingSinkObservations.TryDequeue(out var observation))
        {
            Interlocked.Decrement(ref pendingSinkObservationCount);
            try
            {
                eventSink!.Write(observation);
            }
            catch
            {
                // Observability must never change persistence or recovery outcomes.
            }
        }

        Volatile.Write(ref sinkDrainScheduled, 0);
        if (!pendingSinkObservations.IsEmpty)
        {
            ScheduleSinkDrain();
        }
    }

    private static DesktopNodeJobRuntimeError IndeterminateLoadBlock()
    {
        return new DesktopNodeJobRuntimeError(
            "PCV_JOB_STORE_LOAD_FAILED",
            "The authoritative job-store state could not be established.",
            "Job-store mutation and dispatch are blocked because the primary snapshot or a pending commit could not be read and reconciled.",
            false,
            "Stop job processing and preserve jobs.json plus any pending-commit guard for diagnostics. Restore directory access and restart Desktop Node; if it remains blocked, follow the Operations Guide pending-commit recovery procedure without deleting or editing either file.");
    }

    private string SerializeSnapshotUnsafe(
        IEnumerable<MutableJob> snapshotJobs,
        IEnumerable<string> snapshotQueue)
    {
        var snapshot = new SortedDictionary<string, object?>
        {
            ["version"] = schemaVersion,
            ["saved_at"] = Now(),
            ["jobs"] = snapshotJobs.Select(Project).ToArray(),
            ["queue"] = snapshotQueue.ToArray()
        };
        return JsonSerializer.Serialize(snapshot, RuntimePolicyContract.JsonOptions);
    }

    private static int EnforceRetention(
        IDictionary<string, MutableJob> stateJobs,
        ref int statePrunedTerminalJobs)
    {
        var pruned = 0;
        var staleTerminalJobIds = stateJobs.Values
            .Where(job => IsTerminalStatus(job.Status))
            .OrderByDescending(job => ParsePersistedTimestamp(job.UpdatedAt))
            .ThenByDescending(job => ParsePersistedTimestamp(job.CreatedAt))
            .Skip(MaxRetainedTerminalJobs)
            .Select(job => job.JobId)
            .ToArray();

        foreach (var jobId in staleTerminalJobIds)
        {
            if (stateJobs.Remove(jobId))
            {
                statePrunedTerminalJobs++;
                pruned++;
            }
        }

        return pruned;
    }

    private static DateTimeOffset ParsePersistedTimestamp(string value)
    {
        return DateTimeOffset.Parse(
            value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private void LoadUnsafe()
    {
        jobs.Clear();
        queue.Clear();

        if (store is null)
        {
            return;
        }

        string snapshotJson;
        try
        {
            if (!store.Exists())
            {
                return;
            }

            snapshotJson = store.ReadSnapshot();
        }
        catch (DesktopNodeJobStoreCorruptSnapshotException exception)
        {
            loadBlock = new DesktopNodeJobRuntimeError(
                "PCV_JOB_STORE_CORRUPT",
                "The job store failed structural or semantic validation.",
                exception.Message + " No quarantine, recovery rewrite, or job-store write was performed.",
                false,
                "Stop mutation processing and preserve jobs.json plus any pending-commit guard. Restore a verified backup or repair the store only through an approved offline recovery procedure.");
            RecordObservationUnsafe(
                "load-blocked",
                loadBlock.Code,
                null,
                loadBlock.RecommendedAction);
            return;
        }
        catch (DesktopNodeJobStoreCommitException exception)
        {
            loadBlock = IndeterminateLoadBlock();
            RecordObservationUnsafe(
                "load-blocked",
                loadBlock.Code,
                exception.Outcome,
                loadBlock.RecommendedAction);
            return;
        }

        var validation = DesktopNodeJobStoreSnapshotValidator.Validate(snapshotJson);
        if (validation.Kind == DesktopNodeJobStoreSnapshotValidationKind.UnsupportedFuture)
        {
            var futureVersion = validation.SchemaVersion!.Value;
            loadBlock = new DesktopNodeJobRuntimeError(
                "PCV_JOB_STORE_SCHEMA_UNSUPPORTED",
                "The job store schema version is newer than this runtime supports.",
                $"The job store has version {futureVersion}; this runtime only supports versions 1 and 2. No quarantine, migration, or job store write was performed.",
                false,
                "Stop mutation processing and use the approved job-store migration procedure or reinstall the newer compatible runtime. Do not edit, quarantine, or overwrite the store.");
            RecordObservationUnsafe(
                "load-blocked",
                loadBlock.Code,
                null,
                loadBlock.RecommendedAction);
            return;
        }

        if (validation.Kind == DesktopNodeJobStoreSnapshotValidationKind.Corrupt)
        {
            loadBlock = new DesktopNodeJobRuntimeError(
                "PCV_JOB_STORE_CORRUPT",
                "The job store failed structural or semantic validation.",
                validation.Detail ?? "The job store does not match the supported v1/v2 semantic contract. No quarantine, recovery rewrite, or job-store write was performed.",
                false,
                "Stop mutation processing and preserve jobs.json plus any pending-commit guard. Restore a verified backup or repair the store only through an approved offline recovery procedure.");
            RecordObservationUnsafe(
                "load-blocked",
                loadBlock.Code,
                null,
                loadBlock.RecommendedAction);
            return;
        }

        var root = validation.Root!.Value;
        var version = validation.SchemaVersion!.Value;
        schemaVersion = version;

        var persistedJobs = new Dictionary<string, MutableJob>(StringComparer.Ordinal);
        if (root.TryGetProperty("jobs", out var jobsElement) && jobsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var jobElement in jobsElement.EnumerateArray())
            {
                var job = TryLoadJob(jobElement);
                if (job is not null)
                {
                    persistedJobs[job.JobId] = job;
                }
            }
        }

        var persistedQueue = new Queue<string>();
        if (root.TryGetProperty("queue", out var queueElement) && queueElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var queuedJobIdElement in queueElement.EnumerateArray())
            {
                if (queuedJobIdElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var jobId = queuedJobIdElement.GetString();
                if (!string.IsNullOrWhiteSpace(jobId) &&
                    persistedJobs.TryGetValue(jobId, out var job) &&
                    job.Status == "queued")
                {
                    persistedQueue.Enqueue(jobId);
                }
            }
        }

        var candidateJobs = persistedJobs.ToDictionary(
            pair => pair.Key,
            pair => CloneJob(pair.Value),
            StringComparer.Ordinal);
        var candidateQueue = new Queue<string>(persistedQueue);
        var recoveredRunningCount = RecoverPersistedRunningJobs(candidateJobs);
        var candidatePrunedTerminalJobs = prunedTerminalJobs;
        var prunedOnLoad = EnforceRetention(candidateJobs, ref candidatePrunedTerminalJobs);
        if (recoveredRunningCount > 0 || prunedOnLoad > 0)
        {
            try
            {
                WriteSnapshotUnsafe(candidateJobs.Values, candidateQueue);
            }
            catch (DesktopNodeJobStoreCommitException exception)
            {
                jobs = persistedJobs;
                queue = persistedQueue;
                loadBlock = RecoveryPersistenceBlock(exception.Outcome);
                RecordObservationUnsafe(
                    "running-recovery-persistence-failed",
                    loadBlock.Code,
                    exception.Outcome,
                    loadBlock.RecommendedAction);
                return;
            }
        }

        jobs = candidateJobs;
        queue = candidateQueue;
        prunedTerminalJobs = candidatePrunedTerminalJobs;
        if (recoveredRunningCount > 0)
        {
            RecordObservationUnsafe(
                "running-recovered",
                "PCV_JOB_INTERRUPTED",
                DesktopNodeJobStoreCommitOutcome.Committed,
                "Inspect provider readback and reconcile each interrupted operation manually before considering a new mutation.");
        }
    }

    private MutableJob? TryLoadJob(JsonElement jobElement)
    {
        var jobId = ReadString(jobElement, "job_id");
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return null;
        }

        var status = ReadString(jobElement, "status") ?? string.Empty;
        var result = ReadElement(jobElement, "result");
        var error = ReadError(jobElement, "error");
        var updatedAt = ReadString(jobElement, "updated_at")!;

        return new MutableJob(
            jobId,
            ReadString(jobElement, "operation") ?? string.Empty,
            status,
            ReadElement(jobElement, "params") ?? EmptyObject(),
            result,
            error,
            ReadString(jobElement, "retry_of"),
            ReadString(jobElement, "request_id"),
            ReadString(jobElement, "correlation_id") ?? jobId,
            ReadInt(jobElement, "attempt") ?? 1,
            ReadString(jobElement, "canceled_at"),
            ReadString(jobElement, "created_at")!,
            updatedAt);
    }

    private int RecoverPersistedRunningJobs(Dictionary<string, MutableJob> candidateJobs)
    {
        var recoveredCount = 0;
        foreach (var job in candidateJobs.Values.Where(job => job.Status == "running"))
        {
            var recovered = JobStateTransitionPolicy.RecoverPersistedRunningJob(new DesktopNodeJob(
                JobId: job.JobId,
                Operation: job.Operation,
                Status: JobStatus.Running,
                Parameters: job.Parameters.Clone(),
                Result: null,
                Error: null,
                RetryOf: job.RetryOf,
                Attempt: job.Attempt));
            ApplyPolicyJob(job, recovered);
            job.UpdatedAt = Now();
            recoveredCount++;
        }

        return recoveredCount;
    }
}
