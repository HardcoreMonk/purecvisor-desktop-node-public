namespace DesktopNode.Contracts;

public static class CheckpointScheduleProblemCodes
{
    public const string Forbidden = "PCV_CHECKPOINT_SCHEDULE_FORBIDDEN";
    public const string VmRequired = "PCV_CHECKPOINT_SCHEDULE_VM_REQUIRED";
    public const string NotManaged = "PCV_CHECKPOINT_SCHEDULE_NOT_MANAGED";
    public const string TemplateLocked = "PCV_CHECKPOINT_SCHEDULE_TEMPLATE_LOCKED";
    public const string IntervalInvalid = "PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID";
    public const string RetentionRequired = "PCV_CHECKPOINT_SCHEDULE_RETENTION_REQUIRED";
    public const string RetentionInvalid = "PCV_CHECKPOINT_SCHEDULE_RETENTION_INVALID";
    public const string NotEnabled = "PCV_CHECKPOINT_SCHEDULE_NOT_ENABLED";
    public const string CapacityExceeded = "PCV_CHECKPOINT_SCHEDULE_CAPACITY_EXCEEDED";
}

public sealed record CheckpointScheduleAuthContext(
    bool HasOperate = false,
    bool HasServiceBearer = false);

public sealed record CheckpointScheduleRequest(
    string? VmName,
    CheckpointScheduleAuthContext Auth,
    bool Enabled = false,
    int? IntervalMinutes = null,
    int? RetentionMax = null,
    bool Managed = false,
    bool TemplateLocked = false,
    int CurrentCheckpointCount = 0,
    long? AvailableBytes = null,
    long? EstimatedCheckpointBytes = null);

public sealed record CheckpointScheduleEvaluation(
    bool Ok,
    string? ErrorCode,
    string? VmName,
    bool Enabled,
    int? IntervalMinutes,
    int? RetentionMax,
    string? Action);

public static class CheckpointSchedulePolicy
{
    public const string Schema = "pcv-checkpoint-schedule-v1";
    public const string ActionPreview = "preview";
    public const string ActionSet = "set";
    public const string ActionClear = "clear";
    public const string ActionDueCreate = "due-create";
    public const string PermissionOperate = "operate";
    public const int MinIntervalMinutes = 60;
    public const int MaxIntervalMinutes = 10_080;
    public const int MinRetention = 1;
    public const int MaxRetention = 32;
    public const long MinFreeBytes = 10L * 1024 * 1024 * 1024;

    public static CheckpointScheduleEvaluation EvaluatePreview(CheckpointScheduleRequest request)
    {
        return EvaluateEnabled(request, ActionPreview);
    }

    public static CheckpointScheduleEvaluation EvaluateSet(CheckpointScheduleRequest request)
    {
        return EvaluateEnabled(request, ActionSet);
    }

    public static CheckpointScheduleEvaluation EvaluateClear(CheckpointScheduleRequest request)
    {
        if (RejectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var vmName = NormalizeName(request.VmName);
        if (vmName is null)
        {
            return Reject(CheckpointScheduleProblemCodes.VmRequired);
        }

        return new CheckpointScheduleEvaluation(
            true,
            null,
            vmName,
            false,
            null,
            null,
            ActionClear);
    }

    public static CheckpointScheduleEvaluation EvaluateDueCreate(CheckpointScheduleRequest request)
    {
        if (RejectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        if (!request.Enabled)
        {
            return Reject(CheckpointScheduleProblemCodes.NotEnabled);
        }

        var enabled = EvaluateEnabled(request, ActionDueCreate);
        if (!enabled.Ok)
        {
            return enabled;
        }

        if (request.CurrentCheckpointCount >= enabled.RetentionMax)
        {
            return Reject(CheckpointScheduleProblemCodes.CapacityExceeded);
        }

        if (request.AvailableBytes is { } available && available < MinFreeBytes)
        {
            return Reject(CheckpointScheduleProblemCodes.CapacityExceeded);
        }

        if (request.AvailableBytes is { } remaining &&
            request.EstimatedCheckpointBytes is { } estimated &&
            estimated > remaining)
        {
            return Reject(CheckpointScheduleProblemCodes.CapacityExceeded);
        }

        return enabled;
    }

    private static CheckpointScheduleEvaluation EvaluateEnabled(
        CheckpointScheduleRequest request,
        string action)
    {
        if (RejectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var vmName = NormalizeName(request.VmName);
        if (vmName is null)
        {
            return Reject(CheckpointScheduleProblemCodes.VmRequired);
        }

        if (!request.Managed)
        {
            return Reject(CheckpointScheduleProblemCodes.NotManaged);
        }

        if (request.TemplateLocked)
        {
            return Reject(CheckpointScheduleProblemCodes.TemplateLocked);
        }

        if (request.IntervalMinutes is not { } interval ||
            interval < MinIntervalMinutes ||
            interval > MaxIntervalMinutes)
        {
            return Reject(CheckpointScheduleProblemCodes.IntervalInvalid);
        }

        if (request.RetentionMax is not { } retention || retention <= 0)
        {
            return Reject(CheckpointScheduleProblemCodes.RetentionRequired);
        }

        if (retention < MinRetention || retention > MaxRetention)
        {
            return Reject(CheckpointScheduleProblemCodes.RetentionInvalid);
        }

        return new CheckpointScheduleEvaluation(
            true,
            null,
            vmName,
            true,
            interval,
            retention,
            action);
    }

    private static string? RejectAuth(CheckpointScheduleAuthContext auth)
    {
        return auth.HasOperate || auth.HasServiceBearer
            ? null
            : CheckpointScheduleProblemCodes.Forbidden;
    }

    private static string? NormalizeName(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static CheckpointScheduleEvaluation Reject(string code)
    {
        return new CheckpointScheduleEvaluation(false, code, null, false, null, null, null);
    }
}
