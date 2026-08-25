using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvUpdaterCatalogPublicationPreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.001",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        1,
        "creates a non-mutating updater catalog publication summary")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.002",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        2,
        "records the exact publication preflight check names")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.003",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        3,
        "writes a publication preview catalog for the selected channel")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.004",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        4,
        "records the dry-run update command plan")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.005",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        5,
        "requires plan-only mode")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.006",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        6,
        "rejects a non-HTTPS public catalog URI")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 6);

    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.007",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        7,
        "rejects a non-HTTPS package URI for publication preview")]
    public void Contract007() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 7);

    [PcvLegacyContract(
        "pcv.delivery.updater-catalog-publication-preflight.008",
        "packaging/windows-desktop-node/tests/PcvUpdaterCatalogPublicationPreflight.Tests.ps1",
        8,
        "does not contain host mutation or publication submission command text")]
    public void Contract008() =>
        PreflightContractVerifier.Verify("updater-catalog-publication-preflight", 8);
}
