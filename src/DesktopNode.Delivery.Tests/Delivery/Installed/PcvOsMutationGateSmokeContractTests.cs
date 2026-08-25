using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvOsMutationGateSmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.os-mutation-gate-smoke.001",
        "packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1",
        1,
        "creates summary.json in plan-only mode")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("os-mutation-gate-smoke", 1);

    [PcvLegacyContract(
        "pcv.delivery.os-mutation-gate-smoke.002",
        "packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1",
        2,
        "records the exact OS gate step names in plan-only mode")]
    public void Contract002() =>
        InstalledContractVerifier.Verify("os-mutation-gate-smoke", 2);

    [PcvLegacyContract(
        "pcv.delivery.os-mutation-gate-smoke.003",
        "packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1",
        3,
        "records evidence classification anchors in plan-only summary")]
    public void Contract003() =>
        InstalledContractVerifier.Verify("os-mutation-gate-smoke", 3);

    [PcvLegacyContract(
        "pcv.delivery.os-mutation-gate-smoke.004",
        "packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1",
        4,
        "plans bearer-required LAN probes for runtime policy and static web assets")]
    public void Contract004() =>
        InstalledContractVerifier.Verify("os-mutation-gate-smoke", 4);

    [PcvLegacyContract(
        "pcv.delivery.os-mutation-gate-smoke.005",
        "packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1",
        5,
        "states that plan-only did not perform host mutation")]
    public void Contract005() =>
        InstalledContractVerifier.Verify("os-mutation-gate-smoke", 5);

    [PcvLegacyContract(
        "pcv.delivery.os-mutation-gate-smoke.006",
        "packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1",
        6,
        "does not contain reboot or scheduled task command text")]
    public void Contract006() =>
        InstalledContractVerifier.Verify("os-mutation-gate-smoke", 6);
}
