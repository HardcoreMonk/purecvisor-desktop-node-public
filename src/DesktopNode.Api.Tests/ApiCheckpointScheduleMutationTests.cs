using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiCheckpointScheduleMutationTests
{
    [Fact]
    public void SetQueuesJobWritesFileSkipsCheckpointCreateAndReloadsFromDisk()
    {
        using var root = new TempScheduleRoot();
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        Assert.Equal(["vm.list"], nativeCalls);
        using (var queuedDocument = JsonDocument.Parse(queued.Body))
        {
            Assert.Equal("queued", queuedDocument.RootElement.GetProperty("data").GetProperty("status").GetString());
            Assert.Equal("checkpoint.schedule.set", queuedDocument.RootElement.GetProperty("data").GetProperty("operation").GetString());
        }

        var tick = processor.ProcessOneQueuedJob();

        Assert.True(tick.Processed);
        Assert.Equal(["vm.list"], nativeCalls);
        Assert.Equal("succeeded", tick.Job!.Value.GetProperty("status").GetString());
        Assert.Equal("set", tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());
        Assert.Equal(
            DesktopNodeCheckpointScheduleStore.AuditSchema,
            tick.Job.Value.GetProperty("result").GetProperty("data").GetProperty("audit").GetProperty("contract").GetString());

        using (var file = JsonDocument.Parse(File.ReadAllText(root.FilePath)))
        {
            Assert.Equal(DesktopNodeCheckpointScheduleStore.FileSchema, file.RootElement.GetProperty("schema").GetString());
            var entry = file.RootElement.GetProperty("schedules").GetProperty("lab-vm");
            Assert.Equal(CheckpointSchedulePolicy.Schema, entry.GetProperty("schema").GetString());
            Assert.True(entry.GetProperty("enabled").GetBoolean());
            Assert.Equal(1440, entry.GetProperty("interval_minutes").GetInt32());
            Assert.Equal(8, entry.GetProperty("retention_max").GetInt32());
        }

        var reloaded = new DesktopNodeCheckpointScheduleStore(root.FilePath);
        Assert.True(reloaded.TryReadCurrent("lab-vm", out var payload));
        Assert.True(payload.GetProperty("enabled").GetBoolean());
        Assert.True(payload.GetProperty("file_present").GetBoolean());
        Assert.Equal(1440, payload.GetProperty("interval_minutes").GetInt32());
        Assert.Equal(8, payload.GetProperty("retention_max").GetInt32());
    }

    [Fact]
    public void SetAllowsRetentionAtCapacityBecauseDueCreateIsOutOfSlice()
    {
        using var root = new TempScheduleRoot();
        var processor = CreateProcessor(root.FilePath, [], currentCount: 8);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));

        Assert.Equal(202, queued.StatusCode);
        Assert.True(processor.ProcessOneQueuedJob().Processed);
        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        Assert.True(file.RootElement.GetProperty("schedules").GetProperty("lab-vm").GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public void ClearWritesDisabledEntrySoMissingFileIsNotConfirmedClear()
    {
        using var root = new TempScheduleRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-checkpoint-schedule-file-v1",
          "schedules": {
            "lab-vm": {
              "schema": "pcv-checkpoint-schedule-v1",
              "enabled": true,
              "vm_name": "lab-vm",
              "interval_minutes": 1440,
              "retention_max": 8
            }
          }
        }
        """);
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls);

        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule/clear",
            ServiceBearerAccepted: true));
        Assert.Equal(202, queued.StatusCode);
        Assert.Empty(nativeCalls);

        var tick = processor.ProcessOneQueuedJob();
        Assert.True(tick.Processed);
        Assert.Empty(nativeCalls);
        Assert.Equal("clear", tick.Job!.Value.GetProperty("result").GetProperty("data").GetProperty("action").GetString());

        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        var entry = file.RootElement.GetProperty("schedules").GetProperty("lab-vm");
        Assert.False(entry.GetProperty("enabled").GetBoolean());
        Assert.True(File.Exists(root.FilePath));
    }

    [Fact]
    public void SetPreservesExtraJsonAndRejectsUnmanagedWithoutWriting()
    {
        using var root = new TempScheduleRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-checkpoint-schedule-file-v1",
          "lab_tag": "keep-me",
          "schedules": {
            "other-vm": {
              "schema": "pcv-checkpoint-schedule-v1",
              "enabled": true,
              "note": "keep-entry"
            }
          }
        }
        """);
        var processor = CreateProcessor(root.FilePath, [], managed: false);

        var rejected = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));
        Assert.Equal(400, rejected.StatusCode);
        Assert.Contains(CheckpointScheduleProblemCodes.NotManaged, rejected.Body, StringComparison.Ordinal);
        Assert.False(processor.ProcessOneQueuedJob().Processed);

        var managedProcessor = CreateProcessor(root.FilePath, []);
        var queued = managedProcessor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));
        Assert.Equal(202, queued.StatusCode);
        Assert.True(managedProcessor.ProcessOneQueuedJob().Processed);

        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        Assert.Equal("keep-me", file.RootElement.GetProperty("lab_tag").GetString());
        Assert.Equal("keep-entry", file.RootElement.GetProperty("schedules").GetProperty("other-vm").GetProperty("note").GetString());
        Assert.True(file.RootElement.GetProperty("schedules").GetProperty("lab-vm").GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public void SetRejectsMissingRetentionAndDoesNotWriteFile()
    {
        using var root = new TempScheduleRoot();
        var processor = CreateProcessor(root.FilePath, []);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule",
            """{"interval_minutes":1440}""",
            ServiceBearerAccepted: true));

        Assert.Equal(400, response.StatusCode);
        Assert.Contains(CheckpointScheduleProblemCodes.RetentionRequired, response.Body, StringComparison.Ordinal);
        Assert.False(File.Exists(root.FilePath));
        Assert.False(processor.ProcessOneQueuedJob().Processed);
    }

    [Fact]
    public void SetRejectsTemplateLockedAndMissingVm()
    {
        using var root = new TempScheduleRoot();
        var locked = CreateProcessor(root.FilePath, [], templateLock: true);
        var lockedResponse = locked.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));
        Assert.Equal(400, lockedResponse.StatusCode);
        Assert.Contains(CheckpointScheduleProblemCodes.TemplateLocked, lockedResponse.Body, StringComparison.Ordinal);

        var missing = CreateProcessor(root.FilePath, [], vmName: "other-vm");
        var missingResponse = missing.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints/schedule",
            """{"interval_minutes":1440,"retention_max":8}""",
            ServiceBearerAccepted: true));
        Assert.Equal(404, missingResponse.StatusCode);
        Assert.Contains("PCV_VM_NOT_FOUND", missingResponse.Body, StringComparison.Ordinal);
        Assert.False(File.Exists(root.FilePath));
    }

    [Fact]
    public void WriteFailureRestoresPreviousFileBytes()
    {
        using var root = new TempScheduleRoot();
        var store = new DesktopNodeCheckpointScheduleStore(root.FilePath);
        var first = store.Apply(
            "checkpoint.schedule.set",
            JsonSerializer.SerializeToElement(new
            {
                vm_name = "lab-vm",
                interval_minutes = 1440,
                retention_max = 8
            }));
        Assert.True(first.Ok);
        var previous = File.ReadAllText(root.FilePath);
        Directory.CreateDirectory(root.FilePath + ".tmp");

        var second = store.Apply(
            "checkpoint.schedule.set",
            JsonSerializer.SerializeToElement(new
            {
                vm_name = "lab-vm",
                interval_minutes = 180,
                retention_max = 4
            }));

        Assert.False(second.Ok);
        Assert.Equal("PCV_CHECKPOINT_SCHEDULE_STORE_WRITE_FAILED", second.Error!.Code);
        Assert.Equal(previous, File.ReadAllText(root.FilePath));
        using var file = JsonDocument.Parse(previous);
        Assert.Equal(1440, file.RootElement.GetProperty("schedules").GetProperty("lab-vm").GetProperty("interval_minutes").GetInt32());
    }

    [Fact]
    public void InterruptedSetReconcileConfirmsFileWithoutRewritingMutation()
    {
        using var root = new TempScheduleRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-checkpoint-schedule-file-v1",
          "schedules": {
            "lab-vm": {
              "schema": "pcv-checkpoint-schedule-v1",
              "enabled": true,
              "vm_name": "lab-vm",
              "interval_minutes": 1440,
              "retention_max": 8
            }
          }
        }
        """);
        var jobStorePath = Path.Combine(root.Directory, "jobs.json");
        File.WriteAllText(jobStorePath, InterruptedScheduleJobStoreJson("checkpoint.schedule.set", enabledAfter: true));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls, jobStorePath);
        var before = File.ReadAllText(root.FilePath);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/jobs/job-schedule-reconcile/reconcile"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(nativeCalls);
        Assert.Equal(before, File.ReadAllText(root.FilePath));
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("succeeded", document.RootElement.GetProperty("data").GetProperty("status").GetString());
        Assert.Equal(
            "postcondition-confirmed",
            document.RootElement.GetProperty("data").GetProperty("result").GetProperty("reconciliation").GetProperty("classification").GetString());
    }

    [Fact]
    public void InterruptedSetReconcileRequiresManualActionWhenFileStillMatchesBefore()
    {
        using var root = new TempScheduleRoot();
        var jobStorePath = Path.Combine(root.Directory, "jobs.json");
        File.WriteAllText(jobStorePath, InterruptedScheduleJobStoreJson("checkpoint.schedule.set", enabledAfter: true));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls, jobStorePath);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/jobs/job-schedule-reconcile/reconcile"));

        Assert.Equal(409, response.StatusCode);
        Assert.Empty(nativeCalls);
        Assert.False(File.Exists(root.FilePath));
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("PCV_JOB_RECONCILIATION_REQUIRED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
        Assert.Contains("not-applied", document.RootElement.GetProperty("error").GetProperty("detail").GetString(), StringComparison.Ordinal);
        Assert.Equal("failed", document.RootElement.GetProperty("data").GetProperty("status").GetString());
    }

    [Fact]
    public void InterruptedClearReconcileConfirmsDisabledFileWithoutNativeInvoke()
    {
        using var root = new TempScheduleRoot();
        File.WriteAllText(root.FilePath, """
        {
          "schema": "pcv-checkpoint-schedule-file-v1",
          "schedules": {
            "lab-vm": {
              "schema": "pcv-checkpoint-schedule-v1",
              "enabled": false,
              "vm_name": "lab-vm"
            }
          }
        }
        """);
        var jobStorePath = Path.Combine(root.Directory, "jobs.json");
        File.WriteAllText(jobStorePath, InterruptedScheduleJobStoreJson("checkpoint.schedule.clear", enabledAfter: false));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls, jobStorePath);

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/jobs/job-schedule-reconcile/reconcile"));

        Assert.Equal(200, response.StatusCode);
        Assert.Empty(nativeCalls);
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("succeeded", document.RootElement.GetProperty("data").GetProperty("status").GetString());
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        string filePath,
        List<string> nativeCalls,
        string? jobStorePath = null,
        bool managed = true,
        bool templateLock = false,
        string vmName = "lab-vm",
        int currentCount = 2)
    {
        var vmJson = $$"""
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"{{vmName}}","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":{{currentCount}}},"managed_by_purecvisor":{{managed.ToString().ToLowerInvariant()}},"template_lock":{{templateLock.ToString().ToLowerInvariant()}}}],"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingScheduleNativeAdapter(nativeCalls, vmJson),
            jobStorePath: jobStorePath,
            checkpointScheduleFilePath: filePath);
    }

    private static string InterruptedScheduleJobStoreJson(string operation, bool enabledAfter)
    {
        var expected = enabledAfter
            ? """{"enabled":true,"file_present":true,"interval_minutes":1440,"retention_max":8,"vm_name":"lab-vm"}"""
            : """{"enabled":false,"file_present":true,"interval_minutes":null,"retention_max":null,"vm_name":"lab-vm"}""";
        return $$"""
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-schedule-reconcile",
              "operation": "{{operation}}",
              "status": "failed",
              "params": {
                "interval_minutes": 1440,
                "retention_max": 8,
                "vm_name": "lab-vm",
                "reconciliation": {
                  "schema": "pcv-checkpoint-schedule-reconciliation/v1",
                  "capture_status": "captured",
                  "vm_name": "lab-vm",
                  "before": { "enabled": false, "file_present": false, "interval_minutes": null, "retention_max": null, "vm_name": "lab-vm" },
                  "expected_after": {{expected}}
                }
              },
              "result": null,
              "error": { "code": "PCV_JOB_INTERRUPTED", "message": "Interrupted.", "detail": "Provider side effect is unresolved.", "retryable": false, "recommended_action": "Reconcile the provider state." },
              "retry_of": null,
              "request_id": "req-schedule-reconcile",
              "correlation_id": "corr-schedule-reconcile",
              "attempt": 1,
              "canceled_at": null,
              "created_at": "2026-09-21T00:00:00.0000000Z",
              "updated_at": "2026-09-21T00:00:01.0000000Z"
            }
          ],
          "queue": []
        }
        """;
    }

    private sealed class RecordingScheduleNativeAdapter(List<string> calls, string vmListJson) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            calls.Add(operation);
            if (string.Equals(operation, "vm.list", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(vmListJson);
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Checkpoint schedule set/clear must not create a checkpoint.",
                false);
            return false;
        }
    }

    private sealed class TempScheduleRoot : IDisposable
    {
        public string Directory { get; }
        public string FilePath { get; }

        public TempScheduleRoot()
        {
            Directory = Path.Combine(Path.GetTempPath(), "pcv-checkpoint-schedule-" + Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(Directory);
            FilePath = Path.Combine(Directory, "checkpoint-schedules.json");
        }

        public void Dispose()
        {
            try
            {
                System.IO.Directory.Delete(Directory, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}
