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
        DevelopmentPolicyContractVerifier.Verify("gate-workflow", 1);
        var workflow = RepositoryContractContext.Find().ReadUtf8Text(
            ".github/workflows/development-gates.yml");
        string[] shadowTokens =
        [
            "Run legacy dotnet",
            "Run legacy web",
            "Run legacy packaging Pester",
            "Run legacy installer and Web Pester",
            "Run replacement dotnet",
            "Run replacement web",
            "Run replacement delivery",
            "Run replacement installer-policy",
            "name: legacy-packaging",
            "name: replacement-delivery",
        ];
        foreach (var token in shadowTokens)
        {
            Assert.Contains(token, workflow, StringComparison.Ordinal);
        }
    }
}
