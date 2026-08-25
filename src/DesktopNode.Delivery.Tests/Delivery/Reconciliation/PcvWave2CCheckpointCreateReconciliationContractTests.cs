using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "Delivery")]
public sealed class PcvWave2CCheckpointCreateReconciliationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.wave2-ccheckpoint-create-reconciliation.001",
        "packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1",
        1,
        "publishes a code-level, read-only-baseline, non-host-mutating decision fixture")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("wave2-ccheckpoint-create-reconciliation", 1);

    [PcvLegacyContract(
        "pcv.delivery.wave2-ccheckpoint-create-reconciliation.002",
        "packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1",
        2,
        "requires absent pre-state and fails closed for existing or ambiguous rows")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("wave2-ccheckpoint-create-reconciliation", 2);

    [PcvLegacyContract(
        "pcv.delivery.wave2-ccheckpoint-create-reconciliation.003",
        "packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1",
        3,
        "pins additive route and operator parity without enabling restore reconciliation")]
    public void Contract003() =>
        ReconciliationContractVerifier.Verify("wave2-ccheckpoint-create-reconciliation", 3);

    [PcvLegacyContract(
        "pcv.delivery.wave2-ccheckpoint-create-reconciliation.004",
        "packaging/windows-desktop-node/tests/PcvWave2CCheckpointCreateReconciliation.Tests.ps1",
        4,
        "keeps actual VM smoke and package promotion out of this code slice")]
    public void Contract004() =>
        ReconciliationContractVerifier.Verify("wave2-ccheckpoint-create-reconciliation", 4);
}
