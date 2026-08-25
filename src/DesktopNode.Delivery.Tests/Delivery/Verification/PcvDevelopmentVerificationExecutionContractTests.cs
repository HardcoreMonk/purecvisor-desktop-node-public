using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvDevelopmentVerificationExecutionContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.development-verification-execution.001",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerificationExecution.Tests.ps1",
        1,
        "records selected, skipped and failed suites without hiding scope")]
    public void Contract001() =>
        DevelopmentPolicyContractVerifier.Verify("verification-execution", 1);

    [PcvLegacyContract(
        "pcv.delivery.development-verification-execution.002",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerificationExecution.Tests.ps1",
        2,
        "plans selected suites without invoking the command runner")]
    public void Contract002() =>
        DevelopmentPolicyContractVerifier.Verify("verification-execution", 2);

    [PcvLegacyContract(
        "pcv.delivery.development-verification-execution.003",
        "packaging/windows-desktop-node/tests/PcvDevelopmentVerificationExecution.Tests.ps1",
        3,
        "defines only non-mutating development suite commands")]
    public void Contract003() =>
        DevelopmentPolicyContractVerifier.Verify("verification-execution", 3);
}
