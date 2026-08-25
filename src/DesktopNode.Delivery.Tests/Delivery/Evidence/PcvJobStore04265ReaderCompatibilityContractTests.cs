using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "Delivery")]
public sealed class PcvJobStore04265ReaderCompatibilityContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.job-store04265-reader-compatibility.001",
        "packaging/windows-desktop-node/tests/PcvJobStore04265ReaderCompatibility.Tests.ps1",
        1,
        "pins the frozen host by artifact path, SHA-256, and exact ProductVersion")]
    public void Contract001() =>
        D2EvidenceContractVerifier.Verify("job-store04265-reader-compatibility", 1);

    [PcvLegacyContract(
        "pcv.delivery.job-store04265-reader-compatibility.002",
        "packaging/windows-desktop-node/tests/PcvJobStore04265ReaderCompatibility.Tests.ps1",
        2,
        "keeps the runner isolated from service, installer, admin, and Hyper-V mutation commands")]
    public void Contract002() =>
        D2EvidenceContractVerifier.Verify("job-store04265-reader-compatibility", 2);

    [PcvLegacyContract(
        "pcv.delivery.job-store04265-reader-compatibility.003",
        "packaging/windows-desktop-node/tests/PcvJobStore04265ReaderCompatibility.Tests.ps1",
        3,
        "dry-runs current-writer schemas with the frozen host or verifies immutable public exclusion evidence")]
    public void Contract003() =>
        D2EvidenceContractVerifier.Verify("job-store04265-reader-compatibility", 3);

    [PcvLegacyContract(
        "pcv.delivery.job-store04265-reader-compatibility.004",
        "packaging/windows-desktop-node/tests/PcvJobStore04265ReaderCompatibility.Tests.ps1",
        4,
        "rejects an unpinned host before launching a listener")]
    public void Contract004() =>
        D2EvidenceContractVerifier.Verify("job-store04265-reader-compatibility", 4);

    [PcvLegacyContract(
        "pcv.delivery.job-store04265-reader-compatibility.005",
        "packaging/windows-desktop-node/tests/PcvJobStore04265ReaderCompatibility.Tests.ps1",
        5,
        "reads current-writer stores with the frozen host or verifies immutable compatibility evidence")]
    public void Contract005() =>
        D2EvidenceContractVerifier.Verify("job-store04265-reader-compatibility", 5);

}
