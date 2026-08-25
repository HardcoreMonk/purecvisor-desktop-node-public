using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvWindowsCredentialManagerTransitionPreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.windows-credential-manager-transition-preflight.001",
        "packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1",
        1,
        "creates a non-mutating Windows Credential Manager transition summary")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("windows-credential-manager-transition-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.windows-credential-manager-transition-preflight.002",
        "packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1",
        2,
        "records the exact Credential Manager transition check names")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("windows-credential-manager-transition-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.windows-credential-manager-transition-preflight.003",
        "packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1",
        3,
        "writes a transition plan preview without reading or writing token values")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("windows-credential-manager-transition-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.windows-credential-manager-transition-preflight.004",
        "packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        ManualAdminContractVerifier.Verify("windows-credential-manager-transition-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.windows-credential-manager-transition-preflight.005",
        "packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1",
        5,
        "rejects a credential target with control characters")]
    public void Contract005() =>
        ManualAdminContractVerifier.Verify("windows-credential-manager-transition-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.windows-credential-manager-transition-preflight.006",
        "packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1",
        6,
        "does not contain host mutation, service restart, or credential mutation command text")]
    public void Contract006() =>
        ManualAdminContractVerifier.Verify("windows-credential-manager-transition-preflight", 6);
}

