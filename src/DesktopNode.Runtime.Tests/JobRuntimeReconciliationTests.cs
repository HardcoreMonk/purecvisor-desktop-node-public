using System.Text.Json;
using DesktopNode.Runtime;

namespace DesktopNode.Runtime.Tests;

public sealed class JobRuntimeReconciliationTests
{
    [Fact]
    public void ConfirmedInterruptedVmRenameReconciliationPersistsSucceededState()
    {
        var store = new RecordingJobStore(InterruptedRenameSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-rename-reconcile",
            new DesktopNodeJobReconciliationAssessment(
                true,
                "postcondition-confirmed",
                JsonSerializer.SerializeToElement(new { action = "reconciled" })));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Reconciled, result.Outcome);
        Assert.Equal("succeeded", result.Job!.Status);
        Assert.Null(result.Job.Error);
        Assert.Equal("reconciled", result.Job.Result!.Value.GetProperty("action").GetString());
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciled");
        Assert.Contains("\"status\":\"succeeded\"", store.DurableSnapshot, StringComparison.Ordinal);
    }

    [Fact]
    public void UnconfirmedInterruptedVmRenameReconciliationLeavesJobFailedAndRecordsAttention()
    {
        var store = new RecordingJobStore(InterruptedRenameSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-rename-reconcile",
            new DesktopNodeJobReconciliationAssessment(false, "not-applied", null));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Required, result.Outcome);
        Assert.Equal("failed", result.Job!.Status);
        Assert.Equal("PCV_JOB_INTERRUPTED", result.Job.Error!.Code);
        Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", result.Error!.Code);
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciliation-required");
        Assert.Equal(InterruptedRenameSnapshot(), store.DurableSnapshot);
    }

    [Fact]
    public void ConfirmedInterruptedVmDeleteReconciliationPersistsSucceededState()
    {
        var store = new RecordingJobStore(InterruptedDeleteSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-delete-reconcile",
            new DesktopNodeJobReconciliationAssessment(
                true,
                "postcondition-confirmed",
                JsonSerializer.SerializeToElement(new { action = "reconciled", operation = "vm.delete" })));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Reconciled, result.Outcome);
        Assert.Equal("succeeded", result.Job!.Status);
        Assert.Null(result.Job.Error);
        Assert.Equal("vm.delete", result.Job.Result!.Value.GetProperty("operation").GetString());
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciled");
        Assert.Contains("\"status\":\"succeeded\"", store.DurableSnapshot, StringComparison.Ordinal);
    }

    [Fact]
    public void UnconfirmedInterruptedVmDeleteReconciliationLeavesJobFailedAndRecordsAttention()
    {
        var store = new RecordingJobStore(InterruptedDeleteSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-delete-reconcile",
            new DesktopNodeJobReconciliationAssessment(false, "not-applied", null));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Required, result.Outcome);
        Assert.Equal("failed", result.Job!.Status);
        Assert.Equal("PCV_JOB_INTERRUPTED", result.Job.Error!.Code);
        Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", result.Error!.Code);
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciliation-required");
        Assert.Equal(InterruptedDeleteSnapshot(), store.DurableSnapshot);
    }

    [Fact]
    public void ConfirmedInterruptedCheckpointCreateReconciliationPersistsSucceededState()
    {
        var store = new RecordingJobStore(InterruptedCheckpointCreateSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-checkpoint-create-reconcile",
            new DesktopNodeJobReconciliationAssessment(
                true,
                "postcondition-confirmed",
                JsonSerializer.SerializeToElement(new { action = "reconciled", operation = "checkpoint.create" })));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Reconciled, result.Outcome);
        Assert.Equal("succeeded", result.Job!.Status);
        Assert.Null(result.Job.Error);
        Assert.Equal("checkpoint.create", result.Job.Result!.Value.GetProperty("operation").GetString());
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciled");
        Assert.Contains("\"status\":\"succeeded\"", store.DurableSnapshot, StringComparison.Ordinal);
    }

    [Fact]
    public void UnconfirmedInterruptedCheckpointCreateReconciliationLeavesJobFailedAndRecordsAttention()
    {
        var store = new RecordingJobStore(InterruptedCheckpointCreateSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-checkpoint-create-reconcile",
            new DesktopNodeJobReconciliationAssessment(false, "not-applied", null));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Required, result.Outcome);
        Assert.Equal("failed", result.Job!.Status);
        Assert.Equal("PCV_JOB_INTERRUPTED", result.Job.Error!.Code);
        Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", result.Error!.Code);
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciliation-required");
        Assert.Equal(InterruptedCheckpointCreateSnapshot(), store.DurableSnapshot);
    }

    [Fact]
    public void ConfirmedInterruptedCheckpointRestoreReconciliationPersistsSucceededState()
    {
        var store = new RecordingJobStore(InterruptedCheckpointRestoreSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-checkpoint-restore-reconcile",
            new DesktopNodeJobReconciliationAssessment(
                true,
                "postcondition-confirmed",
                JsonSerializer.SerializeToElement(new { action = "reconciled", operation = "checkpoint.restore" })));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Reconciled, result.Outcome);
        Assert.Equal("succeeded", result.Job!.Status);
        Assert.Null(result.Job.Error);
        Assert.Equal("checkpoint.restore", result.Job.Result!.Value.GetProperty("operation").GetString());
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciled");
        Assert.Contains("\"status\":\"succeeded\"", store.DurableSnapshot, StringComparison.Ordinal);
    }

    [Fact]
    public void UnconfirmedInterruptedCheckpointRestoreReconciliationLeavesJobFailedAndRecordsAttention()
    {
        var store = new RecordingJobStore(InterruptedCheckpointRestoreSnapshot());
        var runtime = new DesktopNodeJobRuntime(store);

        var result = runtime.Reconcile(
            "job-checkpoint-restore-reconcile",
            new DesktopNodeJobReconciliationAssessment(false, "not-applied", null));

        Assert.Equal(DesktopNodeJobReconciliationOutcome.Required, result.Outcome);
        Assert.Equal("failed", result.Job!.Status);
        Assert.Equal("PCV_JOB_INTERRUPTED", result.Job.Error!.Code);
        Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", result.Error!.Code);
        Assert.Contains(runtime.Snapshot().StoreHealth.RecentEvents, item => item.Event == "job-reconciliation-required");
        Assert.Equal(InterruptedCheckpointRestoreSnapshot(), store.DurableSnapshot);
    }

    private static string InterruptedRenameSnapshot()
    {
        return """
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-rename-reconcile",
              "operation": "vm.rename",
              "status": "failed",
              "params": {
                "name": "lab-vm",
                "new_name": "renamed-vm",
                "reconciliation": {
                  "schema": "pcv-vm-rename-reconciliation/v1",
                  "capture_status": "captured",
                  "before": { "id": "vm-id", "name": "lab-vm", "state": "off" },
                  "before_fingerprint": { "state": "off" },
                  "expected_after": { "name": "renamed-vm" }
                }
              },
              "result": null,
              "error": {
                "code": "PCV_JOB_INTERRUPTED",
                "message": "The persisted job was running when the runtime stopped.",
                "detail": "The provider side effect is unresolved.",
                "retryable": false,
                "recommended_action": "Inspect provider readback and reconcile before retrying."
              },
              "retry_of": null,
              "request_id": "req-rename-reconcile",
              "correlation_id": "corr-rename-reconcile",
              "attempt": 1,
              "canceled_at": null,
              "created_at": "2026-08-03T00:00:00.0000000Z",
              "updated_at": "2026-08-03T00:00:01.0000000Z"
            }
          ],
          "queue": []
        }
        """;
    }

    private static string InterruptedDeleteSnapshot()
    {
        return """
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-delete-reconcile",
              "operation": "vm.delete",
              "status": "failed",
              "params": {
                "name": "lab-vm",
                "reconciliation": {
                  "schema": "pcv-vm-delete-reconciliation/v1",
                  "capture_status": "captured",
                  "before": { "id": "vm-id", "name": "lab-vm", "managed_by_purecvisor": true },
                  "before_fingerprint": { "managed_by_purecvisor": true },
                  "expected_after": { "name": "lab-vm", "state": "absent" }
                }
              },
              "result": null,
              "error": {
                "code": "PCV_JOB_INTERRUPTED",
                "message": "The persisted job was running when the runtime stopped.",
                "detail": "The provider side effect is unresolved.",
                "retryable": false,
                "recommended_action": "Inspect provider readback and reconcile before retrying."
              },
              "retry_of": null,
              "request_id": "req-delete-reconcile",
              "correlation_id": "corr-delete-reconcile",
              "attempt": 1,
              "canceled_at": null,
              "created_at": "2026-08-03T00:00:00.0000000Z",
              "updated_at": "2026-08-03T00:00:01.0000000Z"
            }
          ],
          "queue": []
        }
        """;
    }

    private static string InterruptedCheckpointCreateSnapshot()
    {
        return """
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-checkpoint-create-reconcile",
              "operation": "checkpoint.create",
              "status": "failed",
              "params": {
                "vm_name": "lab-vm",
                "checkpoint_name": "before-upgrade",
                "reconciliation": {
                  "schema": "pcv-checkpoint-create-reconciliation/v1",
                  "capture_status": "captured",
                  "before": null,
                  "expected_before": { "state": "absent", "name": "before-upgrade", "vm_name": "lab-vm" },
                  "expected_after": { "state": "present", "name": "before-upgrade", "vm_name": "lab-vm" }
                }
              },
              "result": null,
              "error": {
                "code": "PCV_JOB_INTERRUPTED",
                "message": "The persisted job was running when the runtime stopped.",
                "detail": "The provider side effect is unresolved.",
                "retryable": false,
                "recommended_action": "Inspect provider readback and reconcile before retrying."
              },
              "retry_of": null,
              "request_id": "req-checkpoint-create-reconcile",
              "correlation_id": "corr-checkpoint-create-reconcile",
              "attempt": 1,
              "canceled_at": null,
              "created_at": "2026-08-03T00:00:00.0000000Z",
              "updated_at": "2026-08-03T00:00:01.0000000Z"
            }
          ],
          "queue": []
        }
        """;
    }

    private static string InterruptedCheckpointRestoreSnapshot()
    {
        return """
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-checkpoint-restore-reconcile",
              "operation": "checkpoint.restore",
              "status": "failed",
              "params": {
                "vm_name": "lab-vm",
                "checkpoint_name": "requested",
                "reconciliation": {
                  "schema": "pcv-checkpoint-restore-reconciliation/v1",
                  "capture_status": "captured",
                  "before": { "current_name": "old", "vm_name": "lab-vm" },
                  "expected_after": { "current_name": "requested", "vm_name": "lab-vm", "is_current": true }
                }
              },
              "result": null,
              "error": {
                "code": "PCV_JOB_INTERRUPTED",
                "message": "The persisted job was running when the runtime stopped.",
                "detail": "The provider side effect is unresolved.",
                "retryable": false,
                "recommended_action": "Inspect provider readback and reconcile before retrying."
              },
              "retry_of": null,
              "request_id": "req-checkpoint-restore-reconcile",
              "correlation_id": "corr-checkpoint-restore-reconcile",
              "attempt": 1,
              "canceled_at": null,
              "created_at": "2026-08-14T00:00:00.0000000Z",
              "updated_at": "2026-08-14T00:00:01.0000000Z"
            }
          ],
          "queue": []
        }
        """;
    }

    private sealed class RecordingJobStore(string initialSnapshot) : IDesktopNodeJobStore
    {
        public string Location => "recording://reconciliation/jobs.json";

        public string? DurableSnapshot { get; private set; } = initialSnapshot;

        public bool Exists() => DurableSnapshot is not null;

        public string ReadSnapshot() => DurableSnapshot!;

        public DesktopNodeJobStoreWriteResult WriteSnapshot(string json)
        {
            DurableSnapshot = json;
            return DesktopNodeJobStoreWriteResult.Committed;
        }

        public void Quarantine(string suffix) => DurableSnapshot = null;
    }
}
