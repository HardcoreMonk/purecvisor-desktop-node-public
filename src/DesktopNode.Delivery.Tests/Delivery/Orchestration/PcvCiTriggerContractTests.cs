using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

[Trait("Category", "Delivery")]
public sealed class PcvCiTriggerContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.ci-trigger-contract.001",
        "packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1",
        1,
        "runs Development Gates for pull requests, main pushes, and manual dispatch only")]
    public void Contract001() =>
        OrchestrationContractVerifier.Verify("ci-trigger", 1);

    [PcvLegacyContract(
        "pcv.delivery.ci-trigger-contract.002",
        "packaging/windows-desktop-node/tests/PcvCiTriggerContract.Tests.ps1",
        2,
        "runs Public Boundary Contract for pull requests, main pushes, and manual dispatch only")]
    public void Contract002() =>
        OrchestrationContractVerifier.Verify("ci-trigger", 2);
}
