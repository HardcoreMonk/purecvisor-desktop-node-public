using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.Contracts;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

public sealed class ApiCheckpointScheduleDueWorkerTests
{
    private static readonly DateTimeOffset FrozenNow = new(2026, 9, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DueTickEnqueuesCheckpointCreateWithoutNativeCreate()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddHours(-25));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(root.FilePath, nativeCalls);

        var ticks = processor.ProcessDueCheckpointSchedules();

        Assert.Equal("enqueued", Assert.Single(ticks).Outcome);
        Assert.Equal("checkpoint.create", ticks[0].Operation);
        Assert.NotNull(ticks[0].JobId);
        Assert.DoesNotContain("checkpoint.create", nativeCalls);
        Assert.Contains("vm.list", nativeCalls);
        Assert.Contains("checkpoint.list", nativeCalls);

        var jobs = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
        using var document = JsonDocument.Parse(jobs.Body);
        var job = document.RootElement.GetProperty("data").GetProperty("jobs").EnumerateArray().Single();
        Assert.Equal("checkpoint.create", job.GetProperty("operation").GetString());
        Assert.Equal("queued", job.GetProperty("status").GetString());
        Assert.Equal("lab-vm", job.GetProperty("params").GetProperty("vm_name").GetString());
        Assert.StartsWith("pcv-schedule-", job.GetProperty("params").GetProperty("checkpoint_name").GetString(), StringComparison.Ordinal);

        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        Assert.Equal(
            FrozenNow.ToString("o"),
            file.RootElement.GetProperty("schedules").GetProperty("lab-vm").GetProperty("last_enqueued_at").GetString());
    }

    [Fact]
    public void DueTickDoesNotEnqueueWhenIntervalHasNotElapsed()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddMinutes(-10));
        var processor = CreateProcessor(root.FilePath, []);

        var ticks = processor.ProcessDueCheckpointSchedules();

        Assert.Equal("skipped", Assert.Single(ticks).Outcome);
        Assert.Equal("not-due", ticks[0].ReasonCode);
        var jobs = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
        using var document = JsonDocument.Parse(jobs.Body);
        Assert.Equal(0, document.RootElement.GetProperty("data").GetProperty("jobs").GetArrayLength());
    }

    [Fact]
    public void DueTickSkipsWhenRetentionCapacityIsExceededAndNothingIsSafeToPrune()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddHours(-25));
        var processor = CreateProcessor(
            root.FilePath,
            [],
            currentCount: 8,
            checkpointListJson: """{"ok":true,"operation":"checkpoint.list","data":[{"name":"manual-before","created_at":"2026-09-01T00:00:00Z"}],"error":null}""");

        var ticks = processor.ProcessDueCheckpointSchedules();

        Assert.Equal("skipped", Assert.Single(ticks).Outcome);
        Assert.Equal(CheckpointScheduleProblemCodes.CapacityExceeded, ticks[0].ReasonCode);
        Assert.Null(ticks[0].JobId);
    }

    [Fact]
    public void DueTickEnqueuesDeleteForOldestScheduledCheckpointWhenAtRetentionCap()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddHours(-25));
        var nativeCalls = new List<string>();
        var processor = CreateProcessor(
            root.FilePath,
            nativeCalls,
            currentCount: 8,
            checkpointListJson: """{"ok":true,"operation":"checkpoint.list","data":[{"name":"manual-before","created_at":"2026-08-01T00:00:00Z"},{"name":"pcv-schedule-20260901000000Z","created_at":"2026-09-01T00:00:00Z"},{"name":"pcv-schedule-20260910000000Z","created_at":"2026-09-10T00:00:00Z"}],"error":null}""");

        var ticks = processor.ProcessDueCheckpointSchedules();

        Assert.Equal("enqueued", Assert.Single(ticks).Outcome);
        Assert.Equal("checkpoint.delete", ticks[0].Operation);
        Assert.DoesNotContain("checkpoint.delete", nativeCalls);
        Assert.DoesNotContain("checkpoint.create", nativeCalls);

        var jobs = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
        using var document = JsonDocument.Parse(jobs.Body);
        var job = document.RootElement.GetProperty("data").GetProperty("jobs").EnumerateArray().Single();
        Assert.Equal("checkpoint.delete", job.GetProperty("operation").GetString());
        Assert.Equal("pcv-schedule-20260901000000Z", job.GetProperty("params").GetProperty("checkpoint_name").GetString());
        Assert.Equal("lab-vm", job.GetProperty("params").GetProperty("vm_name").GetString());

        using var file = JsonDocument.Parse(File.ReadAllText(root.FilePath));
        Assert.Equal(
            FrozenNow.AddHours(-25).ToUniversalTime().ToString("o"),
            file.RootElement.GetProperty("schedules").GetProperty("lab-vm").GetProperty("last_enqueued_at").GetString());
    }

    [Fact]
    public void DueTickDoesNotAutoRetryPruneWhileDeleteIsQueued()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddHours(-25));
        var processor = CreateProcessor(
            root.FilePath,
            [],
            currentCount: 8,
            checkpointListJson: """{"ok":true,"operation":"checkpoint.list","data":[{"name":"pcv-schedule-20260901000000Z","created_at":"2026-09-01T00:00:00Z"}],"error":null}""");

        Assert.Equal("enqueued", Assert.Single(processor.ProcessDueCheckpointSchedules()).Outcome);
        var second = processor.ProcessDueCheckpointSchedules();
        Assert.Equal("skipped", Assert.Single(second).Outcome);
        Assert.Equal("active-checkpoint-delete", second[0].ReasonCode);
        var jobs = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
        using var document = JsonDocument.Parse(jobs.Body);
        Assert.Equal(1, document.RootElement.GetProperty("data").GetProperty("jobs").GetArrayLength());
    }

    [Fact]
    public void DueTickSkipsUnmanagedAndTemplateLockedVms()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddHours(-25));
        var unmanaged = CreateProcessor(root.FilePath, [], managed: false);
        Assert.Equal(CheckpointScheduleProblemCodes.NotManaged, Assert.Single(unmanaged.ProcessDueCheckpointSchedules()).ReasonCode);

        var locked = CreateProcessor(root.FilePath, [], templateLock: true);
        Assert.Equal(CheckpointScheduleProblemCodes.TemplateLocked, Assert.Single(locked.ProcessDueCheckpointSchedules()).ReasonCode);
    }

    [Fact]
    public void DueTickSkipsWhenCheckpointCreateIsAlreadyQueued()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddHours(-25));
        var processor = CreateProcessor(root.FilePath, []);
        var queued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/lab-vm/checkpoints",
            """{"name":"manual-before"}""",
            ServiceBearerAccepted: true));
        Assert.Equal(202, queued.StatusCode);

        var ticks = processor.ProcessDueCheckpointSchedules();

        Assert.Equal("skipped", Assert.Single(ticks).Outcome);
        Assert.Equal("active-checkpoint-create", ticks[0].ReasonCode);
        var jobs = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));
        using var document = JsonDocument.Parse(jobs.Body);
        Assert.Equal(1, document.RootElement.GetProperty("data").GetProperty("jobs").GetArrayLength());
        Assert.Equal(
            "manual-before",
            document.RootElement.GetProperty("data").GetProperty("jobs")[0].GetProperty("params").GetProperty("checkpoint_name").GetString());
    }

    [Fact]
    public void SecondDueTickDoesNotAutoRetryAfterEnqueue()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddHours(-25));
        var processor = CreateProcessor(root.FilePath, []);

        Assert.Equal("enqueued", Assert.Single(processor.ProcessDueCheckpointSchedules()).Outcome);
        var second = processor.ProcessDueCheckpointSchedules();
        Assert.Equal("skipped", Assert.Single(second).Outcome);
        Assert.Equal("not-due", second[0].ReasonCode);
    }

    [Fact]
    public void VmDetailOverlaysCheckpointScheduleReadback()
    {
        using var root = new TempScheduleRoot();
        WriteEnabledSchedule(root.FilePath, lastEnqueuedAt: FrozenNow.AddMinutes(-10));
        var processor = CreateProcessor(root.FilePath, []);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/lab-vm"));
        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var schedule = document.RootElement.GetProperty("data").GetProperty("checkpoint_schedule");
        Assert.Equal(CheckpointSchedulePolicy.Schema, schedule.GetProperty("schema").GetString());
        Assert.True(schedule.GetProperty("enabled").GetBoolean());
        Assert.Equal(1440, schedule.GetProperty("interval_minutes").GetInt32());
        Assert.Equal(8, schedule.GetProperty("retention_max").GetInt32());
        Assert.Equal("waiting", schedule.GetProperty("status").GetString());
        Assert.Equal("cli-api", schedule.GetProperty("configure_via").GetString());
        Assert.False(schedule.TryGetProperty("host", out _));
        Assert.False(schedule.TryGetProperty("allow_lan_target", out _));
    }

    [Fact]
    public void VmDetailReadbackIsDisabledWhenScheduleFileIsAbsent()
    {
        using var root = new TempScheduleRoot();
        var processor = CreateProcessor(root.FilePath, []);
        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/vms/lab-vm"));
        using var document = JsonDocument.Parse(response.Body);
        var schedule = document.RootElement.GetProperty("data").GetProperty("checkpoint_schedule");
        Assert.False(schedule.GetProperty("enabled").GetBoolean());
        Assert.Equal("disabled", schedule.GetProperty("status").GetString());
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        string filePath,
        List<string> nativeCalls,
        bool managed = true,
        bool templateLock = false,
        int currentCount = 2,
        string? checkpointListJson = null)
    {
        var vmJson = $$"""
        {"ok":true,"operation":"vm.list","data":[{"id":"vm-id","name":"lab-vm","platform":"hyperv","guest_family":"windows","state":"off","cpu":{"count":2},"memory":{"startup_mb":4096},"generation":2,"checkpoints":{"count":{{currentCount}}},"managed_by_purecvisor":{{managed.ToString().ToLowerInvariant()}},"template_lock":{{templateLock.ToString().ToLowerInvariant()}}}],"error":null}
        """;
        return DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingDueNativeAdapter(
                nativeCalls,
                vmJson,
                checkpointListJson ?? """{"ok":true,"operation":"checkpoint.list","data":[],"error":null}"""),
            checkpointScheduleFilePath: filePath,
            hardeningOptions: new DesktopNodeApiHardeningOptions(Clock: () => FrozenNow));
    }

    private static void WriteEnabledSchedule(string filePath, DateTimeOffset lastEnqueuedAt)
    {
        File.WriteAllText(filePath, $$"""
        {
          "schema": "pcv-checkpoint-schedule-file-v1",
          "schedules": {
            "lab-vm": {
              "schema": "pcv-checkpoint-schedule-v1",
              "enabled": true,
              "vm_name": "lab-vm",
              "interval_minutes": 1440,
              "retention_max": 8,
              "last_enqueued_at": "{{lastEnqueuedAt.ToUniversalTime():o}}"
            }
          }
        }
        """);
    }

    private sealed class RecordingDueNativeAdapter(List<string> calls, string vmListJson, string checkpointListJson) : IDesktopNodeHyperVNativeAdapter
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

            if (string.Equals(operation, "checkpoint.list", StringComparison.Ordinal))
            {
                result = DesktopNodeHyperVOperationResult.FromJson(checkpointListJson);
                return true;
            }

            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                $"The native adapter did not handle '{operation}'.",
                "Due-create/prune must enqueue checkpoint.create or checkpoint.delete and must not invoke them natively.",
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
            Directory = Path.Combine(Path.GetTempPath(), "pcv-checkpoint-due-" + Guid.NewGuid().ToString("N"));
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
