using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvMsixPackagingFeasibilityPreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.msix-packaging-feasibility-preflight.001",
        "packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1",
        1,
        "creates a non-mutating MSIX feasibility summary")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("msix-packaging-feasibility-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.msix-packaging-feasibility-preflight.002",
        "packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1",
        2,
        "records the exact MSIX feasibility check names")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("msix-packaging-feasibility-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.msix-packaging-feasibility-preflight.003",
        "packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1",
        3,
        "writes an MSIX package manifest preview without building a package")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("msix-packaging-feasibility-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.msix-packaging-feasibility-preflight.004",
        "packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1",
        4,
        "records service packaging blockers before any MSIX pass claim")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("msix-packaging-feasibility-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.msix-packaging-feasibility-preflight.005",
        "packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1",
        5,
        "requires plan-only mode")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("msix-packaging-feasibility-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.msix-packaging-feasibility-preflight.006",
        "packaging/windows-desktop-node/tests/PcvMsixPackagingFeasibilityPreflight.Tests.ps1",
        6,
        "does not contain host mutation or MSIX build command text")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("msix-packaging-feasibility-preflight", 6);
}

