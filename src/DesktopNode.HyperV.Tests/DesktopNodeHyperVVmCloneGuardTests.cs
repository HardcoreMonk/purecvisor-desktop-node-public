using System.Text.Json;
using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVVmCloneGuardTests
{
    [Fact]
    public void TryPlanRejectsUnmanagedSource()
    {
        AssertRejected(
            ValidSource() with { Managed = false },
            ValidRequest(),
            targetExists: false,
            "PCV_VM_NOT_MANAGED_BY_PURECVISOR");
    }

    [Fact]
    public void TryPlanRejectsGenerationOtherThanTwo()
    {
        AssertRejected(
            ValidSource() with { Generation = 1 },
            ValidRequest(),
            targetExists: false,
            "PCV_VM_GENERATION_UNSUPPORTED");
    }

    [Fact]
    public void TryPlanRejectsSourceThatIsNotOff()
    {
        AssertRejected(
            ValidSource() with { PowerState = "Running" },
            ValidRequest(),
            targetExists: false,
            "PCV_VM_CLONE_SOURCE_NOT_OFF");
    }

    [Fact]
    public void TryPlanRejectsSourceWithCheckpoints()
    {
        AssertRejected(
            ValidSource() with { CheckpointCount = 1 },
            ValidRequest(),
            targetExists: false,
            "PCV_VM_CLONE_CHECKPOINTS_PRESENT");
    }

    [Fact]
    public void TryPlanRejectsDiskThatIsNotIndependentVhdx()
    {
        AssertRejected(
            ValidSource() with
            {
                Disks =
                [
                    new DesktopNodeHyperVVmCloneDiskSnapshot(@"D:\vms\lab-vm\disk0.vhdx", 1024, IndependentVhdx: true),
                    new DesktopNodeHyperVVmCloneDiskSnapshot(@"D:\vms\lab-vm\disk1.avhdx", 512, IndependentVhdx: false),
                ],
            },
            ValidRequest(),
            targetExists: false,
            "PCV_VM_CLONE_DISK_NOT_INDEPENDENT");
    }

    [Fact]
    public void TryPlanRejectsSecurityFeatures()
    {
        AssertRejected(
            ValidSource() with { SecurityFeaturesPresent = true },
            ValidRequest(),
            targetExists: false,
            "PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED");
    }

    [Fact]
    public void TryPlanRejectsExistingTarget()
    {
        AssertRejected(
            ValidSource(),
            ValidRequest(),
            targetExists: true,
            "PCV_VM_ALREADY_EXISTS");
    }

    [Fact]
    public void TryPlanRejectsOrdinalEqualSourceAndTargetNames()
    {
        AssertRejected(
            ValidSource(),
            new DesktopNodeHyperVVmCloneRequest("lab-vm", "lab-vm", @"D:\vms"),
            targetExists: false,
            "PCV_VM_CLONE_NAME_CONFLICT");
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public void TryPlanRejectsDotAndDotDotTargetNames(string targetName)
    {
        AssertRejected(
            ValidSource(),
            new DesktopNodeHyperVVmCloneRequest("lab-vm", targetName, @"D:\vms"),
            targetExists: false,
            "PCV_VM_NAME_INVALID");
    }

    [Fact]
    public void TryPlanBuildsPreviewPlanForManagedIndependentSource()
    {
        var ok = DesktopNodeHyperVVmCloneGuard.TryPlan(
            ValidSource(),
            ValidRequest(),
            targetExists: false,
            out var plan,
            out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal("lab-vm", plan.Source);
        Assert.Equal("lab-vm-2", plan.Name);
        Assert.Equal("preview", plan.Action);
        Assert.Equal(2, plan.Generation);
        Assert.Equal(Path.GetFullPath(Path.Combine(@"D:\vms", "lab-vm-2")), plan.Directory);
        Assert.Equal(1, plan.DiskCount);
        Assert.Equal(1024, plan.PlannedCopyBytes);
        var disk = Assert.Single(plan.Disks);
        Assert.Equal(@"D:\vms\lab-vm\disk0.vhdx", disk.Source);
        Assert.Equal(Path.Combine(plan.Directory, "disk0.vhdx"), disk.Target);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(plan));
        var root = document.RootElement;
        Assert.Equal("lab-vm", root.GetProperty("source").GetString());
        Assert.Equal("lab-vm-2", root.GetProperty("name").GetString());
        Assert.Equal("preview", root.GetProperty("action").GetString());
        Assert.Equal(2, root.GetProperty("generation").GetInt32());
        Assert.Equal(plan.Directory, root.GetProperty("directory").GetString());
        Assert.Equal(1, root.GetProperty("disk_count").GetInt32());
        Assert.Equal(1024, root.GetProperty("planned_copy_bytes").GetInt64());
        var jsonDisk = Assert.Single(root.GetProperty("disks").EnumerateArray());
        Assert.Equal(@"D:\vms\lab-vm\disk0.vhdx", jsonDisk.GetProperty("source").GetString());
        Assert.Equal(Path.Combine(plan.Directory, "disk0.vhdx"), jsonDisk.GetProperty("target").GetString());
    }

    [Fact]
    public void CloneInfoSerializesSpecJsonNames()
    {
        var info = new DesktopNodeHyperVVmCloneInfo(
            "lab-vm",
            "lab-vm-2",
            "clone",
            @"D:\vms\lab-vm-2",
            [@"D:\vms\lab-vm-2\disk0.vhdx"]);

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(info));
        var root = document.RootElement;
        Assert.Equal("lab-vm", root.GetProperty("source").GetString());
        Assert.Equal("lab-vm-2", root.GetProperty("name").GetString());
        Assert.Equal("clone", root.GetProperty("action").GetString());
        Assert.Equal(@"D:\vms\lab-vm-2", root.GetProperty("directory").GetString());
        Assert.Equal(@"D:\vms\lab-vm-2\disk0.vhdx", Assert.Single(root.GetProperty("disks").EnumerateArray()).GetString());
    }

    private static DesktopNodeHyperVVmCloneSourceSnapshot ValidSource()
    {
        return new DesktopNodeHyperVVmCloneSourceSnapshot(
            "lab-vm",
            true,
            2,
            "Off",
            0,
            [new DesktopNodeHyperVVmCloneDiskSnapshot(@"D:\vms\lab-vm\disk0.vhdx", 1024, true)],
            false);
    }

    private static DesktopNodeHyperVVmCloneRequest ValidRequest()
    {
        return new DesktopNodeHyperVVmCloneRequest("lab-vm", "lab-vm-2", @"D:\vms");
    }

    private static void AssertRejected(
        DesktopNodeHyperVVmCloneSourceSnapshot source,
        DesktopNodeHyperVVmCloneRequest request,
        bool targetExists,
        string code)
    {
        var ok = DesktopNodeHyperVVmCloneGuard.TryPlan(source, request, targetExists, out _, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Equal(code, error.Code);
        Assert.False(error.Retryable);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.Detail));
    }
}
