using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "Delivery")]
public sealed class Pcv04273PromotionEvidenceContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.04273-promotion-evidence.001",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        1,
        "keeps the exact eight repository-owned promotion evidence documents at PASS with bounded nonclaims")]
    public void Contract001() =>
        D2EvidenceContractVerifier.Verify("04273-promotion-evidence", 1);

    [PcvLegacyContract(
        "pcv.delivery.04273-promotion-evidence.002",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        2,
        "demotes the 0.42.73 tuple instead of deleting it")]
    public void Contract002() =>
        D2EvidenceContractVerifier.Verify("04273-promotion-evidence", 2);

    [PcvLegacyContract(
        "pcv.delivery.04273-promotion-evidence.003",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        3,
        "binds the final token claim to the exact R4 runner and summary evidence")]
    public void Contract003() =>
        D2EvidenceContractVerifier.Verify("04273-promotion-evidence", 3);

    [PcvLegacyContract(
        "pcv.delivery.04273-promotion-evidence.004",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        4,
        "keeps the 0.42.73 current-card as a historical promoted record with carried-forward token evidence")]
    public void Contract004() =>
        D2EvidenceContractVerifier.Verify("04273-promotion-evidence", 4);

    [PcvLegacyContract(
        "pcv.delivery.04273-promotion-evidence.005",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        5,
        "contains no provisional promotion marker in the final promotion records")]
    public void Contract005() =>
        D2EvidenceContractVerifier.Verify("04273-promotion-evidence", 5);

    [PcvLegacyContract(
        "pcv.delivery.04273-promotion-evidence.006",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        6,
        "records the 0.42.73 promotion main push without opening another package candidate")]
    public void Contract006() =>
        D2EvidenceContractVerifier.Verify("04273-promotion-evidence", 6);

    [PcvLegacyContract(
        "pcv.delivery.04273-promotion-evidence.007",
        "packaging/windows-desktop-node/tests/Pcv04273PromotionEvidence.Tests.ps1",
        7,
        "keeps the 0.42.73 promotion chain discoverable from the evidence index")]
    public void Contract007() =>
        D2EvidenceContractVerifier.Verify("04273-promotion-evidence", 7);

}
