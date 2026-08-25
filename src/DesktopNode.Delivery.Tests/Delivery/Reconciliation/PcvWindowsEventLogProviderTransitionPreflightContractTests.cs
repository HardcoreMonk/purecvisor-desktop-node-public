using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "Delivery")]
public sealed class PcvWindowsEventLogProviderTransitionPreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-provider-transition-preflight.001",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1",
        1,
        "creates a non-mutating Windows Event Log provider transition summary")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("windows-event-log-provider-transition-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-provider-transition-preflight.002",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1",
        2,
        "records the exact Event Log provider transition check names")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("windows-event-log-provider-transition-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-provider-transition-preflight.003",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1",
        3,
        "writes a provider transition plan preview without registry or event writes")]
    public void Contract003() =>
        ReconciliationContractVerifier.Verify("windows-event-log-provider-transition-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-provider-transition-preflight.004",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        ReconciliationContractVerifier.Verify("windows-event-log-provider-transition-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-provider-transition-preflight.005",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1",
        5,
        "rejects a provider name with control characters")]
    public void Contract005() =>
        ReconciliationContractVerifier.Verify("windows-event-log-provider-transition-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.windows-event-log-provider-transition-preflight.006",
        "packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1",
        6,
        "does not contain host mutation, registry provider, or event write command text")]
    public void Contract006() =>
        ReconciliationContractVerifier.Verify("windows-event-log-provider-transition-preflight", 6);
}
