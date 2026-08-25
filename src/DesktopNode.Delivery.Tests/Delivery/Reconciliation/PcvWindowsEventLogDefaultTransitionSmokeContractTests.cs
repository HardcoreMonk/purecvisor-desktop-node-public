using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "Delivery")]
public sealed class PcvWindowsEventLogDefaultTransitionSmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-default-transition-smoke.001",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogDefaultTransitionSmoke.Tests.ps1",
        1,
        "runs an installed MSI LocalSystem Event Log default transition smoke")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("windows-event-log-default-transition-smoke", 1);

    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-default-transition-smoke.002",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogDefaultTransitionSmoke.Tests.ps1",
        2,
        "uses the native host action instead of direct PowerShell Event Log mutation cmdlets")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("windows-event-log-default-transition-smoke", 2);
}
