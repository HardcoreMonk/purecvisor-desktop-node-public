using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "Delivery")]
public sealed class PcvFeatureEvidencePromotionContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.feature-evidence-promotion.001",
        "packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",
        1,
        "provides a schema-valid P0 feature ledger")]
    public void Contract001() =>
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 1);

    [PcvLegacyContract(
        "pcv.delivery.feature-evidence-promotion.002",
        "packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",
        2,
        "assigns stable ids surfaces stages and evidence to all four P0 features")]
    public void Contract002() =>
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 2);

    [PcvLegacyContract(
        "pcv.delivery.feature-evidence-promotion.003",
        "packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",
        3,
        "records the known 04274 Saved failure without downgrading the other P0 slices")]
    public void Contract003() =>
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 3);

    [PcvLegacyContract(
        "pcv.delivery.feature-evidence-promotion.004",
        "packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",
        4,
        "blocks the known 04274 Saved actual VM failure")]
    public void Contract004() =>
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 4);

    [PcvLegacyContract(
        "pcv.delivery.feature-evidence-promotion.005",
        "packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",
        5,
        "blocks every candidate feature missing manual admin evidence")]
    public void Contract005() =>
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 5);

    [PcvLegacyContract(
        "pcv.delivery.feature-evidence-promotion.006",
        "packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",
        6,
        "allows a candidate only when all required feature stages pass")]
    public void Contract006() =>
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 6);

    [PcvLegacyContract(
        "pcv.delivery.feature-evidence-promotion.007",
        "packaging/windows-desktop-node/tests/PcvFeatureEvidencePromotion.Tests.ps1",
        7,
        "serializes the same blocker ordering and SHA-256 across three evaluations")]
    public void Contract007() =>
        D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 7);

}
