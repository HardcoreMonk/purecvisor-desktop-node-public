using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

[Trait("Category", "Delivery")]
public sealed class PcvConfigJobStoreMigrationApplySmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.config-job-store-migration-apply-smoke.001",
        "packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1",
        1,
        "creates summary.json in plan-only mode")]
    public void Contract001() =>
        OrchestrationContractVerifier.Verify("job-store-migration", 1);

    [PcvLegacyContract(
        "pcv.delivery.config-job-store-migration-apply-smoke.002",
        "packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1",
        2,
        "records the exact installed migration apply step names in plan-only mode")]
    public void Contract002() =>
        OrchestrationContractVerifier.Verify("job-store-migration", 2);

    [PcvLegacyContract(
        "pcv.delivery.config-job-store-migration-apply-smoke.003",
        "packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1",
        3,
        "records supported migration plan identities in the command plan")]
    public void Contract003() =>
        OrchestrationContractVerifier.Verify("job-store-migration", 3);

    [PcvLegacyContract(
        "pcv.delivery.config-job-store-migration-apply-smoke.004",
        "packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1",
        4,
        "states that plan-only did not perform host mutation")]
    public void Contract004() =>
        OrchestrationContractVerifier.Verify("job-store-migration", 4);

    [PcvLegacyContract(
        "pcv.delivery.config-job-store-migration-apply-smoke.005",
        "packaging/windows-desktop-node/tests/PcvConfigJobStoreMigrationApplySmoke.Tests.ps1",
        5,
        "does not contain reboot, scheduler, firewall, trust-store, or Hyper-V mutation command text")]
    public void Contract005() =>
        OrchestrationContractVerifier.Verify("job-store-migration", 5);
}
