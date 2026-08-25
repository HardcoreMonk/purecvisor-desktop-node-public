using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

[Trait("Category", "Delivery")]
public sealed class PcvRunnerArtifactRootContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.runner-artifact-root-contract.001",
        "packaging/windows-desktop-node/tests/PcvRunnerArtifactRootContract.Tests.ps1",
        1,
        "finds the runners that take an -ArtifactRoot parameter")]
    public void Contract001() =>
        OrchestrationContractVerifier.Verify("artifact-root", 1);

    [PcvLegacyContract(
        "pcv.delivery.runner-artifact-root-contract.002",
        "packaging/windows-desktop-node/tests/PcvRunnerArtifactRootContract.Tests.ps1",
        2,
        "guards every relative artifact-root join with a rooted-path branch")]
    public void Contract002() =>
        OrchestrationContractVerifier.Verify("artifact-root", 2);

    [PcvLegacyContract(
        "pcv.delivery.runner-artifact-root-contract.003",
        "packaging/windows-desktop-node/tests/PcvRunnerArtifactRootContract.Tests.ps1",
        3,
        "separates a relative-only runner from a guarded one instead of passing vacuously")]
    public void Contract003() =>
        OrchestrationContractVerifier.Verify("artifact-root", 3);
}
