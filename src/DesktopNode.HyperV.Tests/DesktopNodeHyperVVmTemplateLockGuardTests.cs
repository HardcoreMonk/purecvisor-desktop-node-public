namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVVmTemplateLockGuardTests
{
    [Theory]
    [InlineData("vm.start")]
    [InlineData("vm.clone")]
    [InlineData("vm.create")]
    [InlineData("vm.template.lock")]
    public void AllowsStartCloneAndCreate(string operation)
    {
        Assert.True(DesktopNodeHyperVVmTemplateLockGuard.IsMutationAllowed(operation));
        Assert.False(DesktopNodeHyperVVmTemplateLockGuard.TryReject(operation, templateLock: true, "alpha", out _));
    }

    [Theory]
    [InlineData("vm.delete")]
    [InlineData("vm.rename")]
    [InlineData("vm.save")]
    [InlineData("vm.attach")]
    [InlineData("vm.guest.file")]
    [InlineData("vm.network.connect")]
    [InlineData("checkpoint.create")]
    public void RejectsDirectMutationsWhenLocked(string operation)
    {
        Assert.True(DesktopNodeHyperVVmTemplateLockGuard.TryReject(operation, templateLock: true, "alpha", out var error));
        Assert.NotNull(error);
        Assert.Equal(DesktopNodeHyperVVmTemplateLockGuard.LockedCode, error!.Code);
        Assert.False(error.Retryable);
    }

    [Fact]
    public void AllowsMutationsWhenNotLocked()
    {
        Assert.False(DesktopNodeHyperVVmTemplateLockGuard.TryReject("vm.delete", templateLock: false, "alpha", out _));
    }
}
