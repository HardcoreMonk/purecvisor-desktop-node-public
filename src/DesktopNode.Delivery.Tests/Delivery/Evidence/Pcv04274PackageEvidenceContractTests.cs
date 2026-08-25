using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "Delivery")]
public sealed class Pcv04274PackageEvidenceContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.001",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        1,
        "pins the canonical current evidence record to the exact 0.42.74 tuple")]
    public void Contract001() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 1);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.002",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        2,
        "records the clean 0.42.74 package as current")]
    public void Contract002() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 2);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.003",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        3,
        "closes the 0.42.73 -> 0.42.74 pair as current and opens the next not-opened pair")]
    public void Contract003() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 3);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.004",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        4,
        "indexes 0.42.74 as generated current and keeps the save defect visible")]
    public void Contract004() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 4);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.005",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        5,
        "records the 0.42.74 fullgate PASS as current")]
    public void Contract005() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 5);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.006",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        6,
        "promotes the 0.42.74 installed current-card with carried-forward token evidence")]
    public void Contract006() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 6);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.007",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        7,
        "records the 0.42.74 functional actual-VM PASS as current")]
    public void Contract007() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 7);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.008",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        8,
        "keeps the 0.42.74 P0 actual-VM save failure as an open defect after promotion")]
    public void Contract008() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 8);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.009",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        9,
        "records the 0.42.73 -> 0.42.74 manual-admin pair PASS as current")]
    public void Contract009() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 9);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.010",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        10,
        "links the closed package pair exactly from the manual-admin descriptor")]
    public void Contract010() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 10);

    [PcvLegacyContract(
        "pcv.delivery.04274-package-evidence.011",
        "packaging/windows-desktop-node/tests/Pcv04274PackageEvidence.Tests.ps1",
        11,
        "links the exact promotion chain from the current ledger and evidence index")]
    public void Contract011() =>
        D2EvidenceContractVerifier.Verify("04274-package-evidence", 11);

}
