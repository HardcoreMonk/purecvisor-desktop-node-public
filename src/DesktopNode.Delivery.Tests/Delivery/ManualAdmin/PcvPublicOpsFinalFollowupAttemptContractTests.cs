using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvPublicOpsFinalFollowupAttemptContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.public-ops-final-followup-attempt.001",
        "packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1",
        1,
        "requires an explicit local evidence write opt-in")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("public-ops-final-followup-attempt", 1);

    [PcvLegacyContract(
        "pcv.delivery.public-ops-final-followup-attempt.002",
        "packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1",
        2,
        "records all seven requested public operations follow-up items without making public claims")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("public-ops-final-followup-attempt", 2);

    [PcvLegacyContract(
        "pcv.delivery.public-ops-final-followup-attempt.003",
        "packaging/windows-desktop-node/tests/PcvPublicOpsFinalFollowupAttempt.Tests.ps1",
        3,
        "does not contain host mutation, external submission, or publication command text")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("public-ops-final-followup-attempt", 3);
}
