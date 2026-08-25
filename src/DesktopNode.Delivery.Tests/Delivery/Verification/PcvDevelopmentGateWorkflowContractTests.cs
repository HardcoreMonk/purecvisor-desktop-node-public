using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvDevelopmentGateWorkflowContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.development-gate-workflow.001",
        "packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1",
        1,
        "exists and covers the active non-mutating development gates")]
    public void Contract001() =>
        DevelopmentPolicyContractVerifier.Verify("gate-workflow", 1);
}
