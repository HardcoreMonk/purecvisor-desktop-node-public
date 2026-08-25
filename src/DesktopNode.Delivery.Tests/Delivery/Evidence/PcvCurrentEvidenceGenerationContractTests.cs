using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "Delivery")]
public sealed class PcvCurrentEvidenceGenerationContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.001",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        1,
        "contains a well-formed CLI Web only anchor")]
    public void Contract001() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 1);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.002",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        2,
        "requires a schema-valid blocked feature qualification for current 04274")]
    public void Contract002() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 2);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.003",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        3,
        "rejects contradictory eligibility and blocker combinations in the schema")]
    public void Contract003() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 3);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.004",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        4,
        "keeps schema and runtime validation in parity for invalid qualification values")]
    public void Contract004() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 4);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.005",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        5,
        "renders the feature qualification independently of operational current")]
    public void Contract005() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 5);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.006",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        6,
        "rejects a blocked candidate before writing any source or target file")]
    public void Contract006() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 6);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.007",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        7,
        "rejects a case-only blocked candidate before write mode changes an isolated repository")]
    public void Contract007() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 7);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.008",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        8,
        "rejects malformed SHA and missing evidence references")]
    public void Contract008() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 8);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.009",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        9,
        "renders one bounded CLI Web current block")]
    public void Contract009() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 9);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.010",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        10,
        "fails Check when a target block is stale without writing")]
    public void Contract010() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 10);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.011",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        11,
        "keeps exactly one current block in every owned document")]
    public void Contract011() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 11);

    [PcvLegacyContract(
        "pcv.delivery.current-evidence-generation.012",
        "packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1",
        12,
        "publishes the canonical record as the API current evidence asset")]
    public void Contract012() =>
        D2EvidenceContractVerifier.Verify("current-evidence-generation", 12);

}
