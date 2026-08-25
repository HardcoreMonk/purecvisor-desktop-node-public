using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

[Trait("Category", "Delivery")]
public sealed class PcvBatchSupervisorContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.001",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        1,
        "builds a non-mutating packaging regression manifest")]
    public void Contract001() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 1);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.002",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        2,
        "writes dry-run artifacts without executing commands")]
    public void Contract002() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 2);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.003",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        3,
        "keeps a completed real-run summary intact across a later dry-run of the same artifact root")]
    public void Contract003() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 3);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.004",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        4,
        "rejects host mutation steps without explicit allowance")]
    public void Contract004() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 4);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.005",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        5,
        "rejects automatic reboot capable commands even when host mutation is allowed")]
    public void Contract005() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 5);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.006",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        6,
        "times out a hanging process and records heartbeat plus failed summary")]
    public void Contract006() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 6);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.007",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        7,
        "records GPU adapter and process counter snapshots while a step runs")]
    public void Contract007() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 7);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.008",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        8,
        "resumes by skipping successful matching command fingerprints")]
    public void Contract008() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 8);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.009",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        9,
        "redacts tokens and known paths from arguments and captured output")]
    public void Contract009() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 9);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.010",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        10,
        "runs the CLI entrypoint from a manifest file")]
    public void Contract010() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 10);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.011",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        11,
        "records a real process-start failure without waiting")]
    public void Contract011() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 11);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.012",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        12,
        "builds WebRegression profile without host mutation commands")]
    public void Contract012() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 12);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.013",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        13,
        "requires the public boundary guard in packaging regression manifests")]
    public void Contract013() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 13);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.014",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        14,
        "saves a generated manifest file")]
    public void Contract014() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 14);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.015",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        15,
        "builds ServiceMsiHyperVAdminSmoke as one guarded admin mutation step")]
    public void Contract015() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 15);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.016",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        16,
        "passes explicit batch evidence root into ServiceMsiHyperVAdminSmoke route runner")]
    public void Contract016() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 16);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.017",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        17,
        "builds OsMutationGate as one guarded admin mutation step")]
    public void Contract017() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 17);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.018",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        18,
        "builds ManualAdminCampaignDescriptor as one non-mutating descriptor step")]
    public void Contract018() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 18);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.019",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        19,
        "writes the next manual-admin descriptor batch manifest without host mutation steps")]
    public void Contract019() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 19);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.020",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        20,
        "builds FullAdminHostMutationGate as ordered route parity then OS gate steps")]
    public void Contract020() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 20);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.021",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        21,
        "rejects all admin profiles without explicit host mutation allowance")]
    public void Contract021() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 21);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.022",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        22,
        "rejects all admin profiles from a non-elevated shell even with host mutation allowance")]
    public void Contract022() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 22);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.023",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        23,
        "dry-runs an admin profile with explicit approval without creating step results")]
    public void Contract023() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 23);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.024",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        24,
        "keeps generated admin profile arguments free of reboot and scheduled-task commands")]
    public void Contract024() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 24);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.025",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        25,
        "retries a failed step when retry_count is configured and preserves each attempt")]
    public void Contract025() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 25);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.026",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        26,
        "fails after retry_count is exhausted and points resume at the failed step")]
    public void Contract026() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 26);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.027",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        27,
        "sets retry_count defaults for host-mutating admin profile steps")]
    public void Contract027() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 27);

    [PcvLegacyContract(
        "pcv.delivery.batch-supervisor.028",
        "packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1",
        28,
        "requires profile options for admin profiles")]
    public void Contract028() =>
        OrchestrationContractVerifier.Verify("batch-supervisor", 28);
}
