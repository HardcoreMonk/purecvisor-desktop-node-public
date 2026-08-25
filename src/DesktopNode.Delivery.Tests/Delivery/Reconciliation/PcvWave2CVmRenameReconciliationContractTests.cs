using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "Delivery")]
public sealed class PcvWave2CVmRenameReconciliationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-rename-reconciliation.001",
        "packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1",
        1,
        "publishes a code-level, non-host-mutating decision fixture")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-rename-reconciliation", 1);

    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-rename-reconciliation.002",
        "packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1",
        2,
        "pins the additive reconcile route and operator parity")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-rename-reconciliation", 2);

    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-rename-reconciliation.003",
        "packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1",
        3,
        "fails closed when readback is unavailable or ambiguous")]
    public void Contract003() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-rename-reconciliation", 3);

    [PcvLegacyContract(
        "pcv.delivery.wave2-cvm-rename-reconciliation.004",
        "packaging/windows-desktop-node/tests/PcvWave2CVmRenameReconciliation.Tests.ps1",
        4,
        "keeps actual VM smoke and package promotion out of this code slice")]
    public void Contract004() =>
        ReconciliationContractVerifier.Verify("wave2-cvm-rename-reconciliation", 4);
}
