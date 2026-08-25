using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "Delivery")]
public sealed class PcvWave2CVmDeleteReconciliationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-delete-reconciliation.001",
        "packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1",
        1,
        "publishes a code-level, ownership-gated, non-host-mutating decision fixture")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-delete-reconciliation", 1);

    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-delete-reconciliation.002",
        "packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1",
        2,
        "requires managed ownership and stable identity before absence can reconcile")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-delete-reconciliation", 2);

    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-delete-reconciliation.003",
        "packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1",
        3,
        "pins additive route and operator parity without a new public job status")]
    public void Contract003() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-delete-reconciliation", 3);

    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-delete-reconciliation.004",
        "packaging/windows-desktop-node/tests/PcvWave2CVmDeleteReconciliation.Tests.ps1",
        4,
        "keeps actual VM smoke and package promotion out of this code slice")]
    public void Contract004() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-delete-reconciliation", 4);
}
