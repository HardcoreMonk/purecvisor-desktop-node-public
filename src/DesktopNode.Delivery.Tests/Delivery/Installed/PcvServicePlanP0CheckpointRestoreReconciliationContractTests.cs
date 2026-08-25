using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvServicePlanP0CheckpointRestoreReconciliationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.service-plan-p0-checkpoint-restore-reconciliation.001",
        "packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1",
        1,
        "publishes a code-level, read-only-baseline, non-host-mutating restore decision fixture")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("service-plan-p0-checkpoint-restore-reconciliation", 1);

    [PcvLegacyContract(
        "pcv.delivery.service-plan-p0-checkpoint-restore-reconciliation.002",
        "packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1",
        2,
        "requires current=true postcondition and fails closed for presence-only or unreadable current")]
    public void Contract002() =>
        InstalledContractVerifier.Verify("service-plan-p0-checkpoint-restore-reconciliation", 2);

    [PcvLegacyContract(
        "pcv.delivery.service-plan-p0-checkpoint-restore-reconciliation.003",
        "packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1",
        3,
        "pins additive route and restore operator parity without a new HTTP route")]
    public void Contract003() =>
        InstalledContractVerifier.Verify("service-plan-p0-checkpoint-restore-reconciliation", 3);

    [PcvLegacyContract(
        "pcv.delivery.service-plan-p0-checkpoint-restore-reconciliation.004",
        "packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1",
        4,
        "leaves the Wave 2C create fixture excluding restore")]
    public void Contract004() =>
        InstalledContractVerifier.Verify("service-plan-p0-checkpoint-restore-reconciliation", 4);

    [PcvLegacyContract(
        "pcv.delivery.service-plan-p0-checkpoint-restore-reconciliation.005",
        "packaging/windows-desktop-node/tests/PcvServicePlanP0CheckpointRestoreReconciliation.Tests.ps1",
        5,
        "keeps actual VM smoke and package promotion out of this code slice")]
    public void Contract005() =>
        InstalledContractVerifier.Verify("service-plan-p0-checkpoint-restore-reconciliation", 5);
}

