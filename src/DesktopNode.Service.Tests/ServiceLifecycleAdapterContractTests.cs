using System.Text.Json;
using DesktopNode.Service;

namespace DesktopNode.Service.Tests;

public sealed class ServiceLifecycleAdapterContractTests
{
    [Fact]
    public void DefaultLifecycleAdapterUsesDotNetServiceHostAsDefaultOwner()
    {
        var contract = ServiceLifecycleAdapterContract.CreateDefault();

        Assert.Equal("dotnet-service-action-runner", contract.Owner);
        Assert.Equal("dotnet-windows-service-host", contract.DefaultOwner);
        Assert.Equal("scm-dotnet-host", contract.DefaultActionStance);
        Assert.Equal("default-product-path", contract.CandidateStance);
    }

    [Fact]
    public void DefaultLifecycleAdapterExposesExpectedActionSet()
    {
        var contract = ServiceLifecycleAdapterContract.CreateDefault();

        Assert.Equal(
            [
                "install",
                "configure",
                "repair",
                "start",
                "stop",
                "status",
                "remove",
                "remove-data"
            ],
            contract.Actions.Select(action => action.Name));
    }

    [Fact]
    public void RemoveDataRequiresExplicitOptIn()
    {
        var contract = ServiceLifecycleAdapterContract.CreateDefault();
        var removeData = Assert.Single(contract.Actions, action => action.Name == "remove-data");

        Assert.True(removeData.RequiresExplicitOptIn);
        Assert.Equal("scm-dotnet-host", removeData.Stance);
        Assert.Equal("protected-token-file-required", removeData.TokenSourceStance);
    }

    [Fact]
    public void StartAndStopRequireExplicitOptInButStatusDoesNot()
    {
        var contract = ServiceLifecycleAdapterContract.CreateDefault();
        var status = Assert.Single(contract.Actions, action => action.Name == "status");
        var start = Assert.Single(contract.Actions, action => action.Name == "start");
        var stop = Assert.Single(contract.Actions, action => action.Name == "stop");

        Assert.False(status.RequiresExplicitOptIn);
        Assert.True(start.RequiresExplicitOptIn);
        Assert.True(stop.RequiresExplicitOptIn);
    }

    [Fact]
    public void DefaultLifecycleAdapterPrefersProtectedTokenFileSources()
    {
        var contract = ServiceLifecycleAdapterContract.CreateDefault();
        var serialized = JsonSerializer.Serialize(contract);

        Assert.Equal("protected-token-file-preferred", contract.TokenSourceStance);
        Assert.All(contract.Actions, action => Assert.NotEqual("inline-token", action.TokenSourceStance));
        Assert.Contains(contract.Actions, action => action.TokenSourceStance == "protected-token-file-required");
        Assert.DoesNotContain("raw-token", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token-value", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DefaultLifecycleAdapterDoesNotContainHostMutationCommandStrings()
    {
        var contract = ServiceLifecycleAdapterContract.CreateDefault();
        var serialized = JsonSerializer.Serialize(contract);
        var forbiddenCommands = new[]
        {
            "Install-Service",
            "Start-Service",
            "Stop-Service",
            "DeleteService",
            "sc.exe",
            "New-Service",
            "msiexec",
            "New-NetFirewallRule",
            "Remove-NetFirewallRule",
            "New-EventLog",
            "CreateEventSource",
            "Register-ScheduledTask",
            "Unregister-ScheduledTask",
            "New-ScheduledTask",
            "Restart-Computer",
            "New-VM",
            "Remove-VM"
        };

        foreach (var forbiddenCommand in forbiddenCommands)
        {
            Assert.DoesNotContain(forbiddenCommand, serialized, StringComparison.OrdinalIgnoreCase);
        }
    }
}
