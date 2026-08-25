using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvDotNetQualityToolsContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.001",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        1,
        "collects each project into an isolated fixed result directory and records tool versions")]
    public void Contract001() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 1);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.002",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        2,
        "rejects an artifact root outside the declared repository before running dotnet")]
    public void Contract002() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 2);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.003",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        3,
        "rejects a reparse-point results directory before recursive cleanup")]
    public void Contract003() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 3);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.004",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        4,
        "rejects duplicate test project names before running dotnet")]
    public void Contract004() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 4);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.005",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        5,
        "requires explicit WriteBaseline and records reproducible source provenance plus quality metrics")]
    public void Contract005() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 5);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.006",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        6,
        "rejects inputs and outputs outside the declared repository")]
    public void Contract006() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 6);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.007",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        7,
        "rejects a reparse-point results input before reading external artifacts")]
    public void Contract007() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 7);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.008",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        8,
        "rejects a reparse-point baseline parent before writing outside the repository")]
    public void Contract008() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 8);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.009",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        9,
        "rejects malformed TRX XML")]
    public void Contract009() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 9);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.010",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        10,
        "rejects malformed Cobertura XML")]
    public void Contract010() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 10);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.011",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        11,
        "rejects duplicate project artifacts")]
    public void Contract011() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 11);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.012",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        12,
        "accepts byte-identical Cobertura attachment copies while retaining one canonical result")]
    public void Contract012() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 12);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.013",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        13,
        "fails when skipped tests increase")]
    public void Contract013() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 13);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.014",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        14,
        "fails on any line coverage decline beyond 0.0 percentage points")]
    public void Contract014() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 14);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.015",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        15,
        "fails on any branch coverage decline beyond 0.0 percentage points")]
    public void Contract015() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 15);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.016",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        16,
        "requires every removed test to have an owned migration to a current replacement")]
    public void Contract016() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 16);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.017",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        17,
        "does not authorize a removed test with a planned migration")]
    public void Contract017() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 17);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.018",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        18,
        "allows a one-to-one wildcard namespace move only when each suffix induces one current replacement")]
    public void Contract018() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 18);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.019",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        19,
        "rejects any aggregate test count decrease even when the removed test is mapped")]
    public void Contract019() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 19);

    [PcvLegacyContract(
        "pcv.delivery.dot-net-quality-tools.020",
        "packaging/windows-desktop-node/tests/PcvDotNetQualityTools.Tests.ps1",
        20,
        "rejects migration entries whose replacement is absent")]
    public void Contract020() =>
        DevelopmentPolicyContractVerifier.Verify("quality-tools", 20);
}
