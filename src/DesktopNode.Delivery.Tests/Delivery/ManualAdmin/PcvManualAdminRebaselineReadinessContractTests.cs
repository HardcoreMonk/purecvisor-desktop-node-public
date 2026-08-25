using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvManualAdminRebaselineReadinessContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.001",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        1,
        "creates a non-mutating 0.41.2 manual admin rebaseline readiness summary")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 1);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.002",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        2,
        "blocks historical destructive runner defaults until a current package or dedicated host is selected")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 2);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.003",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        3,
        "blocks a requested rebaseline version that is older than the installed manifest version")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 3);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.004",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        4,
        "writes a plan preview for Credential Manager, Event Log, Burn MSIX MSI, update rollback, and clean-host")]
    public void Contract004() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 4);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.005",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        5,
        "orchestrates an explicit baseline target package pair and rejects mixed input before host mutation")]
    public void Contract005() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 5);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.006",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        6,
        "rejects missing mismatched expired and consumed reservations before execution eligibility")]
    public void Contract006() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 6);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.007",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        7,
        "accepts a matching reservation for actual execution eligibility without mutation")]
    public void Contract007() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 7);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.008",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        8,
        "writes an immutable consumed sidecar and prevents reuse")]
    public void Contract008() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 8);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.009",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        9,
        "requires plan-only mode")]
    public void Contract009() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 9);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-rebaseline-readiness.010",
        "packaging/windows-desktop-node/tests/PcvManualAdminRebaselineReadiness.Tests.ps1",
        10,
        "does not contain host mutation or installer execution command text")]
    public void Contract010() =>
        ManualAdminContractVerifier.Verify("manual-admin-rebaseline-readiness", 10);
}
