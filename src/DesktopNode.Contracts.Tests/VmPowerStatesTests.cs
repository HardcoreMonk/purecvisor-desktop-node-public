using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class VmPowerStatesTests
{
    [Theory]
    [InlineData("Off")]
    [InlineData("off")]
    [InlineData("stopped")]
    [InlineData(" Stopped ")]
    public void AcceptsApiAndNativeInventoryOffVocabulary(string powerState)
    {
        Assert.True(VmPowerStates.IsOff(powerState));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("running")]
    [InlineData("stopping")]
    [InlineData("saved")]
    [InlineData("paused")]
    [InlineData("unknown")]
    public void RejectsEveryOtherState(string? powerState)
    {
        Assert.False(VmPowerStates.IsOff(powerState));
    }
}
