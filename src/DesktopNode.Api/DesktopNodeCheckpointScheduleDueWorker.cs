using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

public sealed record DesktopNodeCheckpointScheduleDueResult(
    string VmName,
    string Outcome,
    string? ReasonCode,
    string? JobId,
    string? Operation = null);

internal sealed class DesktopNodeCheckpointScheduleDueWorker
{
    public const string CheckpointNamePrefix = "pcv-schedule-";

    private readonly DesktopNodeCheckpointScheduleStore store;
    private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;
    private readonly DesktopNodeApiJobReconciliationHandler reconciliationHandler;
    private readonly DesktopNodeJobRuntime jobRuntime;
    private readonly string? scheduleFilePath;

    public DesktopNodeCheckpointScheduleDueWorker(
        DesktopNodeCheckpointScheduleStore store,
        DesktopNodeApiHyperVOperationInvoker operationInvoker,
        DesktopNodeApiJobReconciliationHandler reconciliationHandler,
        DesktopNodeJobRuntime jobRuntime,
        string? scheduleFilePath)
    {
        this.store = store;
        this.operationInvoker = operationInvoker;
        this.reconciliationHandler = reconciliationHandler;
        this.jobRuntime = jobRuntime;
        this.scheduleFilePath = scheduleFilePath;
    }

    public IReadOnlyList<DesktopNodeCheckpointScheduleDueResult> Tick(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var schedules = store.List().Where(item => item.Enabled).ToArray();
        if (schedules.Length == 0)
        {
            return [];
        }

        var inventory = operationInvoker.Invoke(
            "vm.list",
            DesktopNodeApiResponseFactory.EmptyObject(),
            cancellationToken);
        if (!inventory.Ok)
        {
            return schedules
                .Select(item => new DesktopNodeCheckpointScheduleDueResult(
                    item.VmName,
                    "skipped",
                    inventory.Error?.Code ?? "PCV_VM_LIST_FAILED",
                    null))
                .ToArray();
        }

        var jobs = jobRuntime.Snapshot().Jobs;
        var availableBytes = ReadAvailableBytes();
        var results = new List<DesktopNodeCheckpointScheduleDueResult>();
        foreach (var schedule in schedules)
        {
            results.Add(ProcessOne(schedule, inventory.Data, jobs, availableBytes, now, cancellationToken));
        }

        return results;
    }

    private DesktopNodeCheckpointScheduleDueResult ProcessOne(
        CheckpointScheduleState schedule,
        JsonElement? inventory,
        IReadOnlyList<DesktopNodeJobSnapshot> jobs,
        long? availableBytes,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var vmName = schedule.VmName;
        if (!DesktopNodeCheckpointScheduleStore.IsDue(schedule, now))
        {
            return new DesktopNodeCheckpointScheduleDueResult(vmName, "skipped", "not-due", null);
        }

        if (HasActiveCheckpointMutation(jobs, vmName, "checkpoint.create"))
        {
            return new DesktopNodeCheckpointScheduleDueResult(vmName, "skipped", "active-checkpoint-create", null);
        }

        if (HasActiveCheckpointMutation(jobs, vmName, "checkpoint.delete"))
        {
            return new DesktopNodeCheckpointScheduleDueResult(vmName, "skipped", "active-checkpoint-delete", null);
        }

        var vm = DesktopNodeApiJsonReader.FindVm(inventory, vmName);
        if (vm is null)
        {
            return new DesktopNodeCheckpointScheduleDueResult(vmName, "skipped", "PCV_VM_NOT_FOUND", null);
        }

        var currentCount = DesktopNodeApiJsonReader.ReadNestedElement(vm.Value, "checkpoints", "count");
        var memoryMb = DesktopNodeApiJsonReader.ReadNestedElement(vm.Value, "memory", "startup_mb");
        long? estimatedBytes = memoryMb is { } memory &&
            memory.ValueKind == JsonValueKind.Number &&
            memory.TryGetInt32(out var parsedMemory)
            ? parsedMemory * 1024L * 1024L
            : null;
        var evaluation = CheckpointSchedulePolicy.EvaluateDueCreate(new CheckpointScheduleRequest(
            vmName,
            new CheckpointScheduleAuthContext(HasServiceBearer: true),
            Enabled: true,
            IntervalMinutes: schedule.IntervalMinutes,
            RetentionMax: schedule.RetentionMax,
            Managed: DesktopNodeApiJsonReader.ReadBool(vm.Value, "managed_by_purecvisor"),
            TemplateLocked: DesktopNodeApiJsonReader.ReadBool(vm.Value, "template_lock"),
            CurrentCheckpointCount: currentCount is { } count &&
                count.ValueKind == JsonValueKind.Number &&
                count.TryGetInt32(out var parsedCount)
                ? parsedCount
                : 0,
            AvailableBytes: availableBytes,
            EstimatedCheckpointBytes: estimatedBytes));
        if (!evaluation.Ok)
        {
            if (string.Equals(evaluation.ErrorCode, CheckpointScheduleProblemCodes.CapacityExceeded, StringComparison.Ordinal))
            {
                return TryEnqueuePrune(vmName, jobs, cancellationToken);
            }

            return new DesktopNodeCheckpointScheduleDueResult(vmName, "skipped", evaluation.ErrorCode, null);
        }

        var checkpointName = $"{CheckpointNamePrefix}{now.ToUniversalTime():yyyyMMddTHHmmssZ}";
        var parameters = reconciliationHandler.BuildCheckpointCreateParameters(
            vmName,
            checkpointName,
            cancellationToken);
        var job = jobRuntime.Create(
            new DesktopNodeJobCreateCommand("checkpoint.create", parameters),
            new DesktopNodeJobRequestContext($"req-checkpoint-schedule-due-{Guid.NewGuid():N}"));
        store.MarkEnqueued(vmName, now);
        return new DesktopNodeCheckpointScheduleDueResult(vmName, "enqueued", null, job.JobId, "checkpoint.create");
    }

    private DesktopNodeCheckpointScheduleDueResult TryEnqueuePrune(
        string vmName,
        IReadOnlyList<DesktopNodeJobSnapshot> jobs,
        CancellationToken cancellationToken)
    {
        var list = operationInvoker.Invoke(
            "checkpoint.list",
            DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
            {
                ["vm_name"] = vmName
            }),
            cancellationToken);
        if (!list.Ok || list.Data is null)
        {
            return new DesktopNodeCheckpointScheduleDueResult(
                vmName,
                "skipped",
                list.Error?.Code ?? CheckpointScheduleProblemCodes.CapacityExceeded,
                null);
        }

        var target = SelectOldestScheduledCheckpoint(list.Data.Value);
        if (target is null)
        {
            return new DesktopNodeCheckpointScheduleDueResult(
                vmName,
                "skipped",
                CheckpointScheduleProblemCodes.CapacityExceeded,
                null);
        }

        if (HasFailedDelete(jobs, vmName, target))
        {
            return new DesktopNodeCheckpointScheduleDueResult(vmName, "skipped", "prune-failed", null);
        }

        var job = jobRuntime.Create(
            new DesktopNodeJobCreateCommand(
                "checkpoint.delete",
                DesktopNodeApiResponseFactory.JsonFromObject(new SortedDictionary<string, object?>
                {
                    ["checkpoint_name"] = target,
                    ["vm_name"] = vmName
                })),
            new DesktopNodeJobRequestContext($"req-checkpoint-schedule-prune-{Guid.NewGuid():N}"));
        return new DesktopNodeCheckpointScheduleDueResult(vmName, "enqueued", null, job.JobId, "checkpoint.delete");
    }

    private static string? SelectOldestScheduledCheckpoint(JsonElement data)
    {
        return DesktopNodeApiJsonReader.EnumerateCheckpointList(data)
            .Select(checkpoint => new
            {
                Name = DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "name") ??
                    DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "id"),
                CreatedAt = DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "created_at") ??
                    DesktopNodeApiJsonReader.GetStringProperty(checkpoint, "creation_time")
            })
            .Where(checkpoint =>
                !string.IsNullOrWhiteSpace(checkpoint.Name) &&
                checkpoint.Name.StartsWith(CheckpointNamePrefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(checkpoint =>
                DateTimeOffset.TryParse(checkpoint.CreatedAt, out var created)
                    ? created
                    : DateTimeOffset.MaxValue)
            .ThenBy(checkpoint => checkpoint.Name, StringComparer.OrdinalIgnoreCase)
            .Select(checkpoint => checkpoint.Name)
            .FirstOrDefault();
    }

    private static bool HasActiveCheckpointMutation(
        IReadOnlyList<DesktopNodeJobSnapshot> jobs,
        string vmName,
        string operation)
    {
        foreach (var job in jobs)
        {
            if (!string.Equals(job.Operation, operation, StringComparison.Ordinal) ||
                (!string.Equals(job.Status, "queued", StringComparison.Ordinal) &&
                    !string.Equals(job.Status, "running", StringComparison.Ordinal)))
            {
                continue;
            }

            var jobVm = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name");
            if (string.Equals(jobVm, vmName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasFailedDelete(
        IReadOnlyList<DesktopNodeJobSnapshot> jobs,
        string vmName,
        string checkpointName)
    {
        foreach (var job in jobs)
        {
            if (!string.Equals(job.Operation, "checkpoint.delete", StringComparison.Ordinal) ||
                !string.Equals(job.Status, "failed", StringComparison.Ordinal))
            {
                continue;
            }

            var jobVm = DesktopNodeApiJsonReader.ReadString(job.Parameters, "vm_name");
            var jobCheckpoint = DesktopNodeApiJsonReader.ReadString(job.Parameters, "checkpoint_name");
            if (string.Equals(jobVm, vmName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(jobCheckpoint, checkpointName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private long? ReadAvailableBytes()
    {
        if (string.IsNullOrWhiteSpace(scheduleFilePath))
        {
            return null;
        }

        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(scheduleFilePath));
            if (string.IsNullOrWhiteSpace(root))
            {
                return null;
            }

            return new DriveInfo(root).AvailableFreeSpace;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return null;
        }
    }
}
