using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVDvdMediaPlanTests
{
    [Fact]
    public void EjectWithMediaRemovesTheMediaSasd()
    {
        var action = DesktopNodeHyperVDvdMediaPlan.Decide(mediaPresent: true, operation: "vm.eject");

        Assert.Equal(DesktopNodeHyperVDvdMediaPlan.Action.EjectRemoveMedia, action);
    }

    [Fact]
    public void EjectWithoutMediaIsAlreadyEmpty()
    {
        var action = DesktopNodeHyperVDvdMediaPlan.Decide(mediaPresent: false, operation: "vm.eject");

        Assert.Equal(DesktopNodeHyperVDvdMediaPlan.Action.EjectAlreadyEmpty, action);
    }

    [Fact]
    public void AttachWithMediaModifiesTheExistingMedia()
    {
        var action = DesktopNodeHyperVDvdMediaPlan.Decide(mediaPresent: true, operation: "vm.attach");

        Assert.Equal(DesktopNodeHyperVDvdMediaPlan.Action.AttachModifyMedia, action);
    }

    [Fact]
    public void AttachWithoutMediaAddsMediaUnderTheDrive()
    {
        var action = DesktopNodeHyperVDvdMediaPlan.Decide(mediaPresent: false, operation: "vm.attach");

        Assert.Equal(DesktopNodeHyperVDvdMediaPlan.Action.AttachAddMedia, action);
    }

    [Fact]
    public void ProviderRemovesMediaAndAddsUnderAnEmptyDrive()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "src",
            "DesktopNode.HyperV",
            "DesktopNodeHyperVWmiVmMediaProvider.cs"));

        Assert.Contains("RemoveResourceSettings", source, StringComparison.Ordinal);
        Assert.Contains("Microsoft:Hyper-V:Synthetic DVD Drive", source, StringComparison.Ordinal);
        Assert.Contains("Microsoft:Hyper-V:Virtual CD/DVD Disk", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Array.Empty<string>()", source, StringComparison.Ordinal);
        Assert.Contains("FindDvdDriveRasd", source, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
