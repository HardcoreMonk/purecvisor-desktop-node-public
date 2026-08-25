using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "Delivery")]
public sealed class PcvWave2BReconciliationDecisionContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.wave2-breconciliation-decision.001",
        "packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1",
        1,
        "publishes a versioned non-mutating decision fixture")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("wave2-breconciliation-decision", 1);

    [PcvLegacyContract(
        "pcv.delivery.wave2-breconciliation-decision.002",
        "packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1",
        2,
        "covers every one of the 22 current mutation operations exactly once")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("wave2-breconciliation-decision", 2);

    [PcvLegacyContract(
        "pcv.delivery.wave2-breconciliation-decision.003",
        "packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1",
        3,
        "requires expected state readback timeout and operator action for every family")]
    public void Contract003() =>
        ReconciliationContractVerifier.Verify("wave2-breconciliation-decision", 3);

    [PcvLegacyContract(
        "pcv.delivery.wave2-breconciliation-decision.004",
        "packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1",
        4,
        "keeps the required create delete rename QoS and checkpoint families explicit")]
    public void Contract004() =>
        ReconciliationContractVerifier.Verify("wave2-breconciliation-decision", 4);

    [PcvLegacyContract(
        "pcv.delivery.wave2-breconciliation-decision.005",
        "packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1",
        5,
        "keeps persisted-running recovery fail-closed and Guest Execution outside Wave 2B")]
    public void Contract005() =>
        ReconciliationContractVerifier.Verify("wave2-breconciliation-decision", 5);

    [PcvLegacyContract(
        "pcv.delivery.wave2-breconciliation-decision.006",
        "packaging/windows-desktop-node/tests/PcvWave2BReconciliationDecision.Tests.ps1",
        6,
        "does not introduce a new timeout or a public reconciliation error contract")]
    public void Contract006() =>
        ReconciliationContractVerifier.Verify("wave2-breconciliation-decision", 6);
}
