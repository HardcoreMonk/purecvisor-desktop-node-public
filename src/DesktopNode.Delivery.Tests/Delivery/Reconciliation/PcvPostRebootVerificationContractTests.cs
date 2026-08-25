using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

[Trait("Category", "Delivery")]
public sealed class PcvPostRebootVerificationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.001",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        1,
        "builds the PackagingRegression profile without host mutation commands")]
    public void Contract001() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 1);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.002",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        2,
        "keeps active post-reboot profiles product-owned without spike command paths")]
    public void Contract002() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 2);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.003",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        3,
        "retires the HyperVNonIntegration profile from active post-reboot verification")]
    public void Contract003() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 3);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.004",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        4,
        "builds a LocalSystemAtStartup state file contract for repo-local commands")]
    public void Contract004() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 4);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.005",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        5,
        "rejects LocalSystemAtStartup when user profile resources are required")]
    public void Contract005() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 5);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.006",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        6,
        "redacts bearer tokens, secret keys, and known paths from text")]
    public void Contract006() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 6);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.007",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        7,
        "normalizes relative evidence paths before storing state and scheduled task arguments")]
    public void Contract007() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 7);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.008",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        8,
        "builds a LocalSystem AtStartup scheduled task plan")]
    public void Contract008() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 8);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.009",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        9,
        "writes a state file without registering a task in dry-run mode")]
    public void Contract009() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 9);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.010",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        10,
        "registers a post-reboot task only through the explicit registration path")]
    public void Contract010() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 10);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.011",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        11,
        "adds continuation profile commands to the state contract")]
    public void Contract011() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 11);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.012",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        12,
        "runs the pre-reboot entrypoint in dry-run mode without task registration")]
    public void Contract012() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 12);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.013",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        13,
        "rejects automatic reboot requests at the entrypoint")]
    public void Contract013() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 13);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.014",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        14,
        "rejects retired HyperVNonIntegration profile at the entrypoint")]
    public void Contract014() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 14);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.015",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        15,
        "runs commands, writes redacted artifacts, and unregisters the task")]
    public void Contract015() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 15);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.016",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        16,
        "runs continuation commands after post-reboot verification succeeds")]
    public void Contract016() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 16);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.017",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        17,
        "skips continuation commands when post-reboot verification fails")]
    public void Contract017() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 17);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.018",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        18,
        "marks required command failure as overall failure and still writes evidence")]
    public void Contract018() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 18);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.019",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        19,
        "does not rerun commands after completion and still unregisters the task")]
    public void Contract019() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 19);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.020",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        20,
        "records cleanup failure without losing command evidence")]
    public void Contract020() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 20);

    [PcvLegacyContract(
        "pcv.delivery.post-reboot-verification.021",
        "packaging/windows-desktop-node/tests/PcvPostRebootVerification.Tests.ps1",
        21,
        "persists completion artifacts before unregistering the scheduled task")]
    public void Contract021() =>
        ReconciliationContractVerifier.Verify("post-reboot-verification", 21);
}
