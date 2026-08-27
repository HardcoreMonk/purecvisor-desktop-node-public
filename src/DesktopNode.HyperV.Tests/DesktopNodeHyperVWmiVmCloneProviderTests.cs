using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVWmiVmCloneProviderTests
{
    [Fact]
    public void CopyVhdxCopiesSourceBytes()
    {
        using var home = new TempHome();
        var source = Path.Combine(home.Root, "source.vhdx");
        var target = Path.Combine(home.Root, "target.vhdx");
        var payload = new byte[] { 1, 2, 3, 4, 5, 250, 251, 252 };
        File.WriteAllBytes(source, payload);

        DesktopNodeHyperVWmiVmCloneProvider.CopyVhdx(source, target, CancellationToken.None);

        Assert.Equal(payload, File.ReadAllBytes(target));
        Assert.Equal(payload, File.ReadAllBytes(source));
    }

    [Fact]
    public void CopyPlannedDisksCancelDeletesTargetDirectory()
    {
        using var home = new TempHome();
        var source = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        var directory = Path.Combine(home.Root, "lab-vm-2");
        var target = Path.Combine(directory, "disk0.vhdx");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        File.WriteAllBytes(source, new byte[(1024 * 1024) + 128]);
        var plan = new DesktopNodeHyperVVmClonePlan(
            "lab-vm",
            "lab-vm-2",
            "preview",
            2,
            directory,
            1,
            new FileInfo(source).Length,
            [new DesktopNodeHyperVVmCloneDiskPlan(source, target)]);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => DesktopNodeHyperVWmiVmCloneProvider.CopyPlannedDisks(plan, home.Root, cts.Token));

        Assert.False(File.Exists(target));
        Assert.False(Directory.Exists(directory));
        Assert.True(File.Exists(source));
    }

    [Fact]
    public void GuardFailureCopiesZeroDisks()
    {
        using var home = new TempHome();
        var sourcePath = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        File.WriteAllBytes(sourcePath, [9, 8, 7]);
        var request = new DesktopNodeHyperVVmCloneRequest("lab-vm", "lab-vm-2", home.Root);
        var source = ValidSource(sourcePath) with { Managed = false };
        var target = Path.Combine(home.Root, "lab-vm-2", "disk0.vhdx");

        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            () => DesktopNodeHyperVWmiVmCloneProvider.InvokeCopyFromSnapshot(
                source,
                request,
                targetExists: false,
                CancellationToken.None,
                afterCopy: (_, _, _) => throw new InvalidOperationException("define must not run")));

        Assert.Equal("PCV_VM_NOT_MANAGED_BY_PURECVISOR", error.Code);
        Assert.False(File.Exists(target));
        Assert.False(Directory.Exists(Path.Combine(home.Root, "lab-vm-2")));
        Assert.Equal(sourcePath, Assert.Single(Directory.GetFiles(home.Root, "*", SearchOption.AllDirectories)));
    }

    [Fact]
    public void PreviewFromSnapshotDoesNotCopyDisks()
    {
        using var home = new TempHome();
        var sourcePath = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        File.WriteAllBytes(sourcePath, [1, 1, 2, 3]);
        var request = new DesktopNodeHyperVVmCloneRequest("lab-vm", "lab-vm-2", home.Root);

        var plan = DesktopNodeHyperVWmiVmCloneProvider.PreviewFromSnapshot(
            ValidSource(sourcePath),
            request,
            targetExists: false);

        Assert.Equal("preview", plan.Action);
        Assert.Equal(Path.GetFullPath(Path.Combine(home.Root, "lab-vm-2")), plan.Directory);
        Assert.Equal(sourcePath, Assert.Single(plan.Disks).Source);
        Assert.False(Directory.Exists(plan.Directory));
        Assert.False(File.Exists(Assert.Single(plan.Disks).Target));
        Assert.Equal(sourcePath, Assert.Single(Directory.GetFiles(home.Root, "*", SearchOption.AllDirectories)));
    }

    [Fact]
    public void InvokeCopyFromSnapshotCopiesIndependentVhdxWhenDefineSystemIsNoOp()
    {
        using var home = new TempHome();
        var sourcePath = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        var payload = Enumerable.Range(0, 4096).Select(value => (byte)(value % 251)).ToArray();
        File.WriteAllBytes(sourcePath, payload);
        var request = new DesktopNodeHyperVVmCloneRequest("lab-vm", "lab-vm-2", home.Root);
        var defineCount = 0;

        var info = DesktopNodeHyperVWmiVmCloneProvider.InvokeCopyFromSnapshot(
            ValidSource(sourcePath),
            request,
            targetExists: false,
            CancellationToken.None,
            afterCopy: (_, copied, _) =>
            {
                defineCount += 1;
                Assert.Equal(payload, File.ReadAllBytes(Assert.Single(copied)));
            });

        var target = Path.Combine(home.Root, "lab-vm-2", "disk0.vhdx");
        Assert.Equal(1, defineCount);
        Assert.Equal("clone", info.Action);
        Assert.Equal("lab-vm", info.Source);
        Assert.Equal("lab-vm-2", info.Name);
        Assert.Equal(Path.Combine(home.Root, "lab-vm-2"), info.Directory);
        Assert.Equal(target, Assert.Single(info.Disks));
        Assert.Equal(payload, File.ReadAllBytes(target));
        Assert.Equal(payload, File.ReadAllBytes(sourcePath));
    }

    [Fact]
    public void InvokeCopyFromSnapshotRollsBackTargetWhenDefineSystemFails()
    {
        using var home = new TempHome();
        var sourcePath = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        File.WriteAllBytes(sourcePath, [4, 5, 6, 7]);
        var request = new DesktopNodeHyperVVmCloneRequest("lab-vm", "lab-vm-2", home.Root);

        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            () => DesktopNodeHyperVWmiVmCloneProvider.InvokeCopyFromSnapshot(
                ValidSource(sourcePath),
                request,
                targetExists: false,
                CancellationToken.None,
                afterCopy: (_, copied, _) =>
                {
                    Assert.True(File.Exists(Assert.Single(copied)));
                    throw new DesktopNodeHyperVNativeOperationException(
                        "PCV_VM_CLONE_FAILED",
                        "DefineSystem failed.",
                        "Forced define failure for rollback.",
                        true);
                }));

        Assert.Equal("PCV_VM_CLONE_FAILED", error.Code);
        Assert.False(Directory.Exists(Path.Combine(home.Root, "lab-vm-2")));
        Assert.False(File.Exists(Path.Combine(home.Root, "lab-vm-2", "disk0.vhdx")));
        Assert.True(File.Exists(sourcePath));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public void InvokeCopyFromSnapshotRejectsDotTargetsWithoutWipingVmRoot(string targetName)
    {
        using var home = new TempHome();
        var sourceDir = Path.Combine(home.Root, "lab-vm");
        var sourcePath = Path.Combine(sourceDir, "disk0.vhdx");
        Directory.CreateDirectory(sourceDir);
        File.WriteAllBytes(sourcePath, [1, 2, 3, 4]);
        var request = new DesktopNodeHyperVVmCloneRequest("lab-vm", targetName, home.Root);

        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            () => DesktopNodeHyperVWmiVmCloneProvider.InvokeCopyFromSnapshot(
                ValidSource(sourcePath),
                request,
                targetExists: false,
                CancellationToken.None,
                afterCopy: (_, _, _) => throw new InvalidOperationException("define must not run")));

        Assert.Equal("PCV_VM_NAME_INVALID", error.Code);
        Assert.True(Directory.Exists(home.Root));
        Assert.True(Directory.Exists(sourceDir));
        Assert.True(File.Exists(sourcePath));
        Assert.Equal(sourcePath, Assert.Single(Directory.GetFiles(home.Root, "*", SearchOption.AllDirectories)));
        if (targetName == "..")
        {
            Assert.True(Directory.Exists(Path.GetDirectoryName(home.Root)));
        }
    }

    [Fact]
    public void InvokeCopyFromSnapshotDoesNotWipePreExistingSiblingDirectory()
    {
        using var home = new TempHome();
        var sourcePath = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        var sibling = Path.Combine(home.Root, "lab-vm-2");
        var keep = Path.Combine(sibling, "keep.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
        Directory.CreateDirectory(sibling);
        File.WriteAllBytes(sourcePath, [4, 5, 6, 7]);
        File.WriteAllText(keep, "keep");
        var request = new DesktopNodeHyperVVmCloneRequest("lab-vm", "lab-vm-2", home.Root);

        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            () => DesktopNodeHyperVWmiVmCloneProvider.InvokeCopyFromSnapshot(
                ValidSource(sourcePath),
                request,
                targetExists: false,
                CancellationToken.None,
                afterCopy: (_, copied, _) =>
                {
                    Assert.True(File.Exists(Assert.Single(copied)));
                    throw new DesktopNodeHyperVNativeOperationException(
                        "PCV_VM_CLONE_FAILED",
                        "DefineSystem failed.",
                        "Forced define failure for rollback.",
                        true);
                }));

        Assert.Equal("PCV_VM_CLONE_FAILED", error.Code);
        Assert.True(Directory.Exists(sibling));
        Assert.True(File.Exists(keep));
        Assert.Equal("keep", File.ReadAllText(keep));
        Assert.False(File.Exists(Path.Combine(sibling, "disk0.vhdx")));
        Assert.True(File.Exists(sourcePath));
    }

    [Fact]
    public void CopyPlannedDisksCancelLeavesPreExistingSiblingDirectory()
    {
        using var home = new TempHome();
        var source = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        var directory = Path.Combine(home.Root, "lab-vm-2");
        var keep = Path.Combine(directory, "keep.txt");
        var target = Path.Combine(directory, "disk0.vhdx");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(source, new byte[(1024 * 1024) + 128]);
        File.WriteAllText(keep, "keep");
        var plan = new DesktopNodeHyperVVmClonePlan(
            "lab-vm",
            "lab-vm-2",
            "preview",
            2,
            directory,
            1,
            new FileInfo(source).Length,
            [new DesktopNodeHyperVVmCloneDiskPlan(source, target)]);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => DesktopNodeHyperVWmiVmCloneProvider.CopyPlannedDisks(plan, home.Root, cts.Token));

        Assert.True(Directory.Exists(directory));
        Assert.True(File.Exists(keep));
        Assert.Equal("keep", File.ReadAllText(keep));
        Assert.False(File.Exists(target));
        Assert.True(File.Exists(source));
    }

    [Fact]
    public void CopyPlannedDisksRejectsVmRootDirectoryWithoutWipingSource()
    {
        using var home = new TempHome();
        var source = Path.Combine(home.Root, "lab-vm", "disk0.vhdx");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        File.WriteAllBytes(source, [9, 9, 9]);
        var target = Path.Combine(home.Root, "disk0.vhdx");
        var plan = new DesktopNodeHyperVVmClonePlan(
            "lab-vm",
            ".",
            "preview",
            2,
            home.Root,
            1,
            new FileInfo(source).Length,
            [new DesktopNodeHyperVVmCloneDiskPlan(source, target)]);

        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            () => DesktopNodeHyperVWmiVmCloneProvider.CopyPlannedDisks(plan, home.Root, CancellationToken.None));

        Assert.Equal("PCV_VM_NAME_INVALID", error.Code);
        Assert.True(Directory.Exists(home.Root));
        Assert.True(File.Exists(source));
        Assert.False(File.Exists(target));
        Assert.Equal(source, Assert.Single(Directory.GetFiles(home.Root, "*", SearchOption.AllDirectories)));
    }

    [Fact]
    public void SecurityInspectFailureIsFailClosed()
    {
        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            DesktopNodeHyperVWmiVmCloneProvider.ThrowIfSecurityInspectFailed);

        Assert.Equal("PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED", error.Code);
        Assert.False(error.Retryable);
        Assert.Equal("PCV_VM_CLONE_SECURITY_FEATURES_UNSUPPORTED", DesktopNodeHyperVWmiVmCloneProvider.SecurityFeaturesInspectFailed().Code);
    }

    [Fact]
    public void CheckpointInspectFailureIsFailClosed()
    {
        var error = Assert.Throws<DesktopNodeHyperVNativeOperationException>(
            DesktopNodeHyperVWmiVmCloneProvider.ThrowIfCheckpointInspectFailed);

        Assert.Equal("PCV_VM_CLONE_CHECKPOINTS_PRESENT", error.Code);
        Assert.False(error.Retryable);
        Assert.Equal("PCV_VM_CLONE_CHECKPOINTS_PRESENT", DesktopNodeHyperVWmiVmCloneProvider.CheckpointsInspectFailed().Code);
    }

    private static DesktopNodeHyperVVmCloneSourceSnapshot ValidSource(string sourcePath)
    {
        return new DesktopNodeHyperVVmCloneSourceSnapshot(
            "lab-vm",
            true,
            2,
            "Off",
            0,
            [new DesktopNodeHyperVVmCloneDiskSnapshot(sourcePath, new FileInfo(sourcePath).Length, true)],
            false);
    }

    private sealed class TempHome : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "pcv-clone-" + Guid.NewGuid().ToString("N"));

        public TempHome()
        {
            Directory.CreateDirectory(Root);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Root))
                {
                    Directory.Delete(Root, recursive: true);
                }
            }
            catch
            {
            }
        }
    }
}
