using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvServiceTokenRotationRevokePreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.service-token-rotation-revoke-preflight.001",
        "packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1",
        1,
        "creates a non-mutating service token rotation revoke summary")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("service-token-rotation-revoke-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.service-token-rotation-revoke-preflight.002",
        "packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1",
        2,
        "records the exact service token rotation revoke check names")]
    public void Contract002() =>
        InstalledContractVerifier.Verify("service-token-rotation-revoke-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.service-token-rotation-revoke-preflight.003",
        "packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1",
        3,
        "writes a rotation revoke plan preview without token generation or writes")]
    public void Contract003() =>
        InstalledContractVerifier.Verify("service-token-rotation-revoke-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.service-token-rotation-revoke-preflight.004",
        "packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        InstalledContractVerifier.Verify("service-token-rotation-revoke-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.service-token-rotation-revoke-preflight.005",
        "packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1",
        5,
        "rejects an unsupported rotation mode")]
    public void Contract005() =>
        InstalledContractVerifier.Verify("service-token-rotation-revoke-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.service-token-rotation-revoke-preflight.006",
        "packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1",
        6,
        "does not contain host mutation, token generation, token write, or service reload command text")]
    public void Contract006() =>
        InstalledContractVerifier.Verify("service-token-rotation-revoke-preflight", 6);
}
