using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvDevelopmentGateWorkflowContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.development-gate-workflow.001",
        "packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1",
        1,
        "exists and covers the active non-mutating development gates")]
    public void Contract001()
    {
        var workflow = RepositoryContractContext.Find().ReadUtf8Text(
            ".github/workflows/development-gates.yml");
        string[] activeTokens =
        [
            "name: dotnet",
            "name: web",
            "name: delivery",
            "name: installer-policy",
            "Run dotnet shard",
            "Run web shard",
            "Run delivery shard",
            "Run installer and policy shard",
            "--shard dotnet",
            "--shard web",
            "--shard delivery",
            "--shard installer-policy",
        ];
        foreach (var token in activeTokens)
        {
            Assert.Contains(token, workflow, StringComparison.Ordinal);
        }

        string[] forbiddenExecutableTokens =
        [
            "Invoke-Pester",
            "Install-Module",
            "shell: pwsh",
            "shell: powershell",
            "Run legacy",
        ];
        foreach (var token in forbiddenExecutableTokens)
        {
            Assert.DoesNotContain(token, workflow, StringComparison.OrdinalIgnoreCase);
        }
    }
}
