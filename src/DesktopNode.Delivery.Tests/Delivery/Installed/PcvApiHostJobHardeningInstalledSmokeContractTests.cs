using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvApiHostJobHardeningInstalledSmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.001",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        1,
        "ships a runner with body cap, route timeout, rate-limit, job, worker, and redaction evidence fields")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 1);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.002",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        2,
        "writes a dry-run summary without requiring an installed service or admin mutation")]
    public void Contract002() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 2);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.003",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        3,
        "requires body cap problem-details content type and job cancel contract in the pass gate")]
    public void Contract003() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 3);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.004",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        4,
        "requires rate-limit Retry-After and problem-details content type when the opt-in probe runs")]
    public void Contract004() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 4);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.005",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        5,
        "requires controlled route-timeout 504, Retry-After, and problem-details when the opt-in probe runs")]
    public void Contract005() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 5);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.006",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        6,
        "requires recorded diagnostics and console read probes in the pass gate")]
    public void Contract006() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 6);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.007",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        7,
        "allows success response evidence to have no expected problem code")]
    public void Contract007() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 7);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.008",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        8,
        "decodes byte-array Invoke-WebRequest content before extracting problem codes")]
    public void Contract008() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 8);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.009",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        9,
        "keeps Task 6 artifacts free of Task 7 wording and raw-token README examples")]
    public void Contract009() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 9);

    [PcvLegacyContract(
        "pcv.delivery.api-host-job-hardening-installed-smoke.010",
        "packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1",
        10,
        "does not contain host mutation, service reconfiguration, installer, or public publication commands")]
    public void Contract010() =>
        InstalledContractVerifier.Verify("api-host-job-hardening-installed-smoke", 10);
}

