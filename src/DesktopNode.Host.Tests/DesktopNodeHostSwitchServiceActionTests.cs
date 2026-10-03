using DesktopNode.Contracts;
using DesktopNode.Host;
using DesktopNode.Host.Ops;

namespace DesktopNode.Host.Tests;

public sealed partial class DesktopNodeHostServiceActionTests
{
    [Fact]
    public void SwitchCreatePlanDeclaresNativeSwitchOperation()
    {
        var plan = DesktopNodeHostServiceAction.CreatePlan(
            NativeActionOptions.WithAction("switch-create").WithSwitch("pcv-lab-internal", "internal"));

        Assert.Equal("hyperv-switch", plan.OperationFamily);
        Assert.Equal("switch-create", plan.NativeSwitchOperation);
        Assert.Empty(plan.Commands);
    }

    [Fact]
    public async Task SwitchCreateDryRunDoesNotMutate()
    {
        var controller = new FakeHyperVSwitchController();

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("switch-create").WithSwitch("pcv-lab-internal", "internal", dryRun: true),
            controller);

        Assert.True(result.Ok);
        Assert.Equal("switch-create", result.Action);
        Assert.Equal(["query"], controller.Calls);
        Assert.False(result.HyperVSwitch?.Exists);
    }

    [Fact]
    public async Task SwitchCreateInvokesControllerAfterPolicyPass()
    {
        var controller = new FakeHyperVSwitchController();

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("switch-create").WithSwitch("pcv-lab-internal", "internal", allowManagementOs: true),
            controller);

        Assert.True(result.Ok);
        Assert.Equal(["query", "create"], controller.Calls);
        Assert.True(result.HyperVSwitch?.Exists);
        Assert.Equal("internal", result.HyperVSwitch?.Type);
        Assert.True(result.HyperVSwitch?.AllowManagementOs);
    }

    [Fact]
    public async Task SwitchCreateRejectsReservedNameBeforeMutation()
    {
        var controller = new FakeHyperVSwitchController();

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("switch-create").WithSwitch("Default Switch", "internal"),
            controller);

        Assert.False(result.Ok);
        Assert.Equal(NetworkChangeProblemCodes.SwitchNameReserved, result.ErrorCode);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task SwitchCreateRejectsExistingSwitch()
    {
        var controller = new FakeHyperVSwitchController
        {
            Snapshot = new DesktopNodeHyperVSwitchMutationSnapshot(
                "pcv-lab-internal",
                true,
                "internal",
                0,
                true,
                true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("switch-create").WithSwitch("pcv-lab-internal", "internal"),
            controller);

        Assert.False(result.Ok);
        Assert.Equal(NetworkChangeProblemCodes.AlreadyExists, result.ErrorCode);
        Assert.Equal(["query"], controller.Calls);
    }

    [Fact]
    public async Task SwitchRemoveDeletesUnusedProductSwitch()
    {
        var controller = new FakeHyperVSwitchController
        {
            Snapshot = new DesktopNodeHyperVSwitchMutationSnapshot(
                "pcv-lab-internal",
                true,
                "internal",
                0,
                true,
                true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("switch-remove").WithSwitch("pcv-lab-internal"),
            controller);

        Assert.True(result.Ok);
        Assert.Equal(["query", "remove"], controller.Calls);
        Assert.False(result.HyperVSwitch?.Exists);
    }

    [Fact]
    public async Task SwitchRemoveRejectsInUseSwitch()
    {
        var controller = new FakeHyperVSwitchController
        {
            Snapshot = new DesktopNodeHyperVSwitchMutationSnapshot(
                "pcv-lab-internal",
                true,
                "internal",
                2,
                true,
                true)
        };

        var result = await DesktopNodeHostServiceAction.ExecuteAsync(
            NativeActionOptions.WithAction("switch-remove").WithSwitch("pcv-lab-internal"),
            controller);

        Assert.False(result.Ok);
        Assert.Equal(NetworkChangeProblemCodes.SwitchInUse, result.ErrorCode);
        Assert.Equal(["query"], controller.Calls);
    }
}
