using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

// packaging/windows-desktop-node/tools/Invoke-PcvPrGate.ps1 은 PR 을 열기 전에 개발 호스트에서 돌리는 gate 다(2026-10-09 감사
// §10, campaign audit-green-lean-20261009 Task 6). Required CI 는 PowerShell 을 돌리지 않으므로 이 시험이 source 계약을
// 고정한다: CI 와 같은 Release 구성, module size ratchet, ADR-0016 통합 시험은 campaign approval_locator 안 문장으로만 열림.
// 동작 시험은 packaging/windows-desktop-node/manual-admin-tests/PcvPrGate.Tests.ps1 이다.
[Trait("Category", "Delivery")]
public sealed class PcvPrGateContractTests
{
    private const string Tool = "packaging/windows-desktop-node/tools/Invoke-PcvPrGate.ps1";

    [Fact]
    public void GateRunsReleaseTestsRatchetAndGatesIntegrationOnTheCampaignApproval()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(Tool);

        foreach (var token in new[]
        {
            "Set-StrictMode -Version Latest",
            "[switch]$Integration",
            "[switch]$PlanOnly",
            "'pcv-pr-gate-v1'",
            "'PCV_HYPERV_INTEGRATION_APPROVAL'",
            "'docs/ga-ready/active-campaign.json'",
            "PCV_PR_GATE_INTEGRATION_APPROVAL_MISSING",
            "PCV_PR_GATE_INTEGRATION_APPROVAL_NOT_IN_LOCATOR",
            "'dotnet', 'test', 'src/DesktopNode.sln', '-c', 'Release'",
            "PcvModuleSizeRatchet.Tests.ps1",
            "'src/DesktopNode.HyperV.IntegrationTests', '-c', 'Release'",
            "-HostMutation $true",
            "host_mutation_performed",
            "if ($refused) { exit 2 }",
        })
        {
            Assert.Contains(token, source, StringComparison.Ordinal);
        }

        foreach (var forbidden in new[]
        {
            "msiexec", "Set-Service", "Start-Service", "Stop-Service", "New-VM", "Remove-VM",
            "Restart-Computer", "NetFirewallRule", "Invoke-WebRequest", "git push",
        })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void IntegrationStepIsNeverPlannedWithoutTheIntegrationSwitch()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(Tool);
        var integrationStep = source.IndexOf("-Id 'hyperv-integration'", StringComparison.Ordinal);
        var integrationGuard = source.IndexOf("if ($Integration) {", StringComparison.Ordinal);

        Assert.True(integrationGuard >= 0 && integrationStep > integrationGuard, "the integration step must sit under the -Integration guard");
    }
}
