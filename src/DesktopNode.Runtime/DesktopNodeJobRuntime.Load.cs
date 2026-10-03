using System.Collections.Concurrent;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Runtime;

public sealed partial class DesktopNodeJobRuntime
{
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
}
