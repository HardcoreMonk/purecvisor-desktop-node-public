using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class CheckpointSchedulePolicyTests
{
    [Fact]
    public void PreviewAcceptsManagedDailyScheduleWithOperate()
    {
        var result = CheckpointSchedulePolicy.EvaluatePreview(Valid());

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Equal("lab-vm", result.VmName);
        Assert.True(result.Enabled);
        Assert.Equal(1440, result.IntervalMinutes);
        Assert.Equal(8, result.RetentionMax);
        Assert.Equal(CheckpointSchedulePolicy.ActionPreview, result.Action);
    }

    [Fact]
    public void SetAcceptsServiceBearerAndTrimsVmName()
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with
        {
            VmName = "  lab-vm  ",
            Auth = new CheckpointScheduleAuthContext(HasServiceBearer: true)
        });

        Assert.True(result.Ok);
        Assert.Equal("lab-vm", result.VmName);
        Assert.Equal(CheckpointSchedulePolicy.ActionSet, result.Action);
    }

    [Fact]
    public void SetAllowsScheduleWhenAlreadyAtRetentionCap()
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with { CurrentCheckpointCount = 8 });

        Assert.True(result.Ok);
        Assert.Equal(8, result.RetentionMax);
        Assert.Equal(CheckpointSchedulePolicy.ActionSet, result.Action);
    }

    [Fact]
    public void ClearAcceptsOperateWithoutIntervalOrRetention()
    {
        var result = CheckpointSchedulePolicy.EvaluateClear(new CheckpointScheduleRequest(
            "lab-vm",
            new CheckpointScheduleAuthContext(HasOperate: true)));

        Assert.True(result.Ok);
        Assert.False(result.Enabled);
        Assert.Null(result.IntervalMinutes);
        Assert.Null(result.RetentionMax);
        Assert.Equal(CheckpointSchedulePolicy.ActionClear, result.Action);
    }

    [Fact]
    public void DueCreateAcceptsWhenUnderRetentionAndDiskFloor()
    {
        var result = CheckpointSchedulePolicy.EvaluateDueCreate(Valid() with
        {
            CurrentCheckpointCount = 2,
            AvailableBytes = CheckpointSchedulePolicy.MinFreeBytes,
            EstimatedCheckpointBytes = 1
        });

        Assert.True(result.Ok);
        Assert.Equal(CheckpointSchedulePolicy.ActionDueCreate, result.Action);
    }

    [Theory]
    [InlineData("preview")]
    [InlineData("set")]
    [InlineData("clear")]
    [InlineData("due-create")]
    public void RejectsMissingOperateBeforeVmValidation(string operation)
    {
        var request = new CheckpointScheduleRequest(null, new CheckpointScheduleAuthContext());
        var result = Evaluate(operation, request);

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.Forbidden, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetRejectsMissingVmName(string? vmName)
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with { VmName = vmName });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.VmRequired, result.ErrorCode);
    }

    [Fact]
    public void SetRejectsUnmanagedVm()
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with { Managed = false });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.NotManaged, result.ErrorCode);
    }

    [Fact]
    public void SetRejectsTemplateLockedVm()
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with { TemplateLocked = true });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.TemplateLocked, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(59)]
    [InlineData(10081)]
    public void SetRejectsIntervalOutsideBounds(int? interval)
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with { IntervalMinutes = interval });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.IntervalInvalid, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetRejectsMissingOrNonPositiveRetentionAsInfiniteAutoProtect(int? retention)
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with { RetentionMax = retention });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.RetentionRequired, result.ErrorCode);
    }

    [Fact]
    public void SetRejectsRetentionAboveMaximum()
    {
        var result = CheckpointSchedulePolicy.EvaluateSet(Valid() with { RetentionMax = 33 });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.RetentionInvalid, result.ErrorCode);
    }

    [Fact]
    public void DueCreateRejectsDisabledSchedule()
    {
        var result = CheckpointSchedulePolicy.EvaluateDueCreate(Valid() with { Enabled = false });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.NotEnabled, result.ErrorCode);
    }

    [Fact]
    public void DueCreateRejectsWhenRetentionCapIsReached()
    {
        var result = CheckpointSchedulePolicy.EvaluateDueCreate(Valid() with { CurrentCheckpointCount = 8 });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.CapacityExceeded, result.ErrorCode);
    }

    [Fact]
    public void DueCreateRejectsWhenFreeBytesAreBelowFloor()
    {
        var result = CheckpointSchedulePolicy.EvaluateDueCreate(Valid() with
        {
            AvailableBytes = CheckpointSchedulePolicy.MinFreeBytes - 1
        });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.CapacityExceeded, result.ErrorCode);
    }

    [Fact]
    public void DueCreateRejectsWhenEstimatedSizeExceedsAvailableBytes()
    {
        var result = CheckpointSchedulePolicy.EvaluateDueCreate(Valid() with
        {
            AvailableBytes = CheckpointSchedulePolicy.MinFreeBytes,
            EstimatedCheckpointBytes = CheckpointSchedulePolicy.MinFreeBytes + 1
        });

        Assert.False(result.Ok);
        Assert.Equal(CheckpointScheduleProblemCodes.CapacityExceeded, result.ErrorCode);
    }

    [Fact]
    public void BoundsMatchTheDesignContract()
    {
        Assert.Equal("pcv-checkpoint-schedule-v1", CheckpointSchedulePolicy.Schema);
        Assert.Equal("operate", CheckpointSchedulePolicy.PermissionOperate);
        Assert.Equal(60, CheckpointSchedulePolicy.MinIntervalMinutes);
        Assert.Equal(10_080, CheckpointSchedulePolicy.MaxIntervalMinutes);
        Assert.Equal(1, CheckpointSchedulePolicy.MinRetention);
        Assert.Equal(32, CheckpointSchedulePolicy.MaxRetention);
        Assert.Equal(10L * 1024 * 1024 * 1024, CheckpointSchedulePolicy.MinFreeBytes);
    }

    private static CheckpointScheduleEvaluation Evaluate(string operation, CheckpointScheduleRequest request)
    {
        return operation switch
        {
            "preview" => CheckpointSchedulePolicy.EvaluatePreview(request),
            "set" => CheckpointSchedulePolicy.EvaluateSet(request),
            "clear" => CheckpointSchedulePolicy.EvaluateClear(request),
            _ => CheckpointSchedulePolicy.EvaluateDueCreate(request)
        };
    }

    private static CheckpointScheduleRequest Valid()
    {
        return new CheckpointScheduleRequest(
            "lab-vm",
            new CheckpointScheduleAuthContext(HasOperate: true),
            Enabled: true,
            IntervalMinutes: 1440,
            RetentionMax: 8,
            Managed: true);
    }
}
