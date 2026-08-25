using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvManualAdminBaselineReservationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.manual-admin-baseline-reservation.001",
        "packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1",
        1,
        "binds the campaign to the installed N-1 version without exposing host identity")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("manual-admin-baseline-reservation", 1);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-baseline-reservation.002",
        "packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1",
        2,
        "rejects installed version mismatch invalid order expiry and empty resource")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("manual-admin-baseline-reservation", 2);

    [PcvLegacyContract(
        "pcv.delivery.manual-admin-baseline-reservation.003",
        "packaging/windows-desktop-node/tests/PcvManualAdminBaselineReservation.Tests.ps1",
        3,
        "creates no file in PlanOnly and atomically refuses overwrite in write mode")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("manual-admin-baseline-reservation", 3);
}

