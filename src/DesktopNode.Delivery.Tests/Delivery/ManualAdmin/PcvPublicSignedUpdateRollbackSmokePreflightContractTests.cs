using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvPublicSignedUpdateRollbackSmokePreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.public-signed-update-rollback-smoke-preflight.001",
        "packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",
        1,
        "creates a non-mutating public signed update rollback smoke preflight summary")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("public-signed-update-rollback-smoke-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.public-signed-update-rollback-smoke-preflight.002",
        "packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",
        2,
        "records the exact public signed smoke preflight check names")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("public-signed-update-rollback-smoke-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.public-signed-update-rollback-smoke-preflight.003",
        "packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",
        3,
        "writes a clean-host smoke plan preview without executing update or rollback")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("public-signed-update-rollback-smoke-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.public-signed-update-rollback-smoke-preflight.004",
        "packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        ManualAdminContractVerifier.Verify("public-signed-update-rollback-smoke-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.public-signed-update-rollback-smoke-preflight.005",
        "packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",
        5,
        "rejects a non-HTTPS package URI")]
    public void Contract005() =>
        ManualAdminContractVerifier.Verify("public-signed-update-rollback-smoke-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.public-signed-update-rollback-smoke-preflight.006",
        "packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",
        6,
        "rejects already claimed public signing or publication states without evidence import")]
    public void Contract006() =>
        ManualAdminContractVerifier.Verify("public-signed-update-rollback-smoke-preflight", 6);

    [PcvLegacyContract(
        "pcv.delivery.public-signed-update-rollback-smoke-preflight.007",
        "packaging/windows-desktop-node/tests/PcvPublicSignedUpdateRollbackSmokePreflight.Tests.ps1",
        7,
        "does not contain host mutation, installer, or update execution command text")]
    public void Contract007() =>
        ManualAdminContractVerifier.Verify("public-signed-update-rollback-smoke-preflight", 7);
}
