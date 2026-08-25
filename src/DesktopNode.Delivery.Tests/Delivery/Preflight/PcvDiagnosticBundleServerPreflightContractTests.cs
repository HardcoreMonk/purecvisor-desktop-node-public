using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvDiagnosticBundleServerPreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.diagnostic-bundle-server-preflight.001",
        "packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1",
        1,
        "creates a non-mutating diagnostic bundle server-side summary")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("diagnostic-bundle-server-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.diagnostic-bundle-server-preflight.002",
        "packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1",
        2,
        "records the exact diagnostic bundle server preflight check names")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("diagnostic-bundle-server-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.diagnostic-bundle-server-preflight.003",
        "packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1",
        3,
        "writes a server-side diagnostic bundle plan preview without archive or download execution")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("diagnostic-bundle-server-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.diagnostic-bundle-server-preflight.004",
        "packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("diagnostic-bundle-server-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.diagnostic-bundle-server-preflight.005",
        "packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1",
        5,
        "rejects routes outside the Local API namespace")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("diagnostic-bundle-server-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.diagnostic-bundle-server-preflight.006",
        "packaging/windows-desktop-node/tests/PcvDiagnosticBundleServerPreflight.Tests.ps1",
        6,
        "does not contain host mutation, archive creation, wrapper execution, or service command text")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("diagnostic-bundle-server-preflight", 6);
}
