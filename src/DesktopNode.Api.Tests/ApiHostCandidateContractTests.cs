using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiHostCandidateContractTests
{
    [Fact]
    public void DefaultContractIncludesPhase25PublicRouteCandidates()
    {
        var contract = ApiHostCandidateContract.CreateDefault();

        Assert.Contains("/api/v1/runtime/policy", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/host/status", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/vms", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/jobs", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/jobs/{jobId}", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/jobs/{jobId}/cancel", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/jobs/{jobId}/retry", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/jobs/{jobId}/reconcile", contract.PublicRouteCandidates);
        Assert.Contains("/api/v1/ops/summary", contract.PublicRouteCandidates);
    }

    [Fact]
    public void DefaultContractUsesDotNetApiHostAsProductDefault()
    {
        var contract = ApiHostCandidateContract.CreateDefault();

        Assert.Equal("default", contract.HostReplacementStance);
        Assert.Equal("default", contract.RuntimeReplacementStance);
    }

    [Fact]
    public void DefaultContractKeepsLocalApiOwnershipAndDotNetDefaultOwner()
    {
        var contract = ApiHostCandidateContract.CreateDefault();

        Assert.Equal("local-api", contract.Owner);
        Assert.Equal("dotnet-runtime", contract.DefaultOwner);
    }

    [Fact]
    public void DefaultContractDoesNotContainHostMutationCommandStrings()
    {
        var contract = ApiHostCandidateContract.CreateDefault();
        var serialized = JsonSerializer.Serialize(contract);
        var forbiddenCommands = new[]
        {
            "Install-Service",
            "Start-Service",
            "Stop-Service",
            "DeleteService",
            "msiexec",
            "New-NetFirewallRule",
            "Remove-NetFirewallRule",
            "New-EventLog",
            "CreateEventSource",
            "Register-ScheduledTask",
            "Unregister-ScheduledTask",
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
