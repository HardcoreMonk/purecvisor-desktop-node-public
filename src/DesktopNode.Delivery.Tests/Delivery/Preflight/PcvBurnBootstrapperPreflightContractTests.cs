using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvBurnBootstrapperPreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.001",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        1,
        "creates a non-mutating Burn bootstrapper preflight summary")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.002",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        2,
        "records the exact Burn preflight check names")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.003",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        3,
        "writes a WiX Burn authoring preview without building a bundle")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.004",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        4,
        "plans the Burn build with the canonical WiX 5 bootstrapper extension id")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.005",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        5,
        "records MSI chain hash binding and lifecycle evidence still required")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.006",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        6,
        "requires plan-only mode")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 6);

    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.007",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        7,
        "rejects non-HTTPS MSI URLs for public bootstrapper preview")]
    public void Contract007() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 7);

    [PcvLegacyContract(
        "pcv.delivery.burn-bootstrapper-preflight.008",
        "packaging/windows-desktop-node/tests/PcvBurnBootstrapperPreflight.Tests.ps1",
        8,
        "does not contain host mutation or publication submission command text")]
    public void Contract008() =>
        PreflightContractVerifier.Verify("burn-bootstrapper-preflight", 8);
}
