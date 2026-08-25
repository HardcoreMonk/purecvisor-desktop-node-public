using System.Text.Json;
using DesktopNode.Service;

namespace DesktopNode.Service.Tests;

public sealed class ServiceHostCandidateContractTests
{
    [Fact]
    public void DefaultContractMakesDotNetServiceHostTheProductDefault()
    {
        var contract = ServiceHostCandidateContract.CreateDefault();

        Assert.Equal("default", contract.ServiceHostReplacementStance);
        Assert.Equal("replaces-winsw", contract.ProductServiceReplacementStance);
    }

    [Fact]
    public void DefaultContractUsesDotNetServiceHostAsDefaultOwner()
    {
        var contract = ServiceHostCandidateContract.CreateDefault();

        Assert.Equal("dotnet-windows-service-host", contract.Owner);
        Assert.Equal("dotnet-windows-service-host", contract.DefaultOwner);
        Assert.Equal("packaging/windows-desktop-node", contract.DefaultProductWrapperPath);
    }

    [Fact]
    public void DefaultContractAllowsWindowsServiceLaunchMode()
    {
        var contract = ServiceHostCandidateContract.CreateDefault();

        Assert.Contains("console-listen", contract.AllowedLaunchModes);
        Assert.Contains("windows-service", contract.AllowedLaunchModes);
        Assert.DoesNotContain("product-service", contract.AllowedLaunchModes);
    }

    [Fact]
    public void DefaultContractRecordsProtectedTokenFilePreferenceWithoutInlineToken()
    {
        var contract = ServiceHostCandidateContract.CreateDefault();
        var serialized = JsonSerializer.Serialize(contract);

        Assert.Equal("protected-token-file-preferred", contract.TokenSourceStance);
        Assert.DoesNotContain("raw-token", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("inline-token", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token-value", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DefaultContractDoesNotContainHostMutationCommandStrings()
    {
        var contract = ServiceHostCandidateContract.CreateDefault();
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
