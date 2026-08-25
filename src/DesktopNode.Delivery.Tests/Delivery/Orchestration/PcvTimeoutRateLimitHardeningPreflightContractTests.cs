using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Orchestration;

[Trait("Category", "Delivery")]
public sealed class PcvTimeoutRateLimitHardeningPreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.timeout-rate-limit-hardening-preflight.001",
        "packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1",
        1,
        "creates a non-mutating timeout and rate-limit hardening summary")]
    public void Contract001() =>
        OrchestrationContractVerifier.Verify("timeout-rate-limit", 1);

    [PcvLegacyContract(
        "pcv.delivery.timeout-rate-limit-hardening-preflight.002",
        "packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1",
        2,
        "records the exact timeout and rate-limit preflight check names")]
    public void Contract002() =>
        OrchestrationContractVerifier.Verify("timeout-rate-limit", 2);

    [PcvLegacyContract(
        "pcv.delivery.timeout-rate-limit-hardening-preflight.003",
        "packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1",
        3,
        "writes a timeout and rate-limit hardening plan preview without applying middleware or load tests")]
    public void Contract003() =>
        OrchestrationContractVerifier.Verify("timeout-rate-limit", 3);

    [PcvLegacyContract(
        "pcv.delivery.timeout-rate-limit-hardening-preflight.004",
        "packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        OrchestrationContractVerifier.Verify("timeout-rate-limit", 4);

    [PcvLegacyContract(
        "pcv.delivery.timeout-rate-limit-hardening-preflight.005",
        "packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1",
        5,
        "rejects routes outside the Local API namespace")]
    public void Contract005() =>
        OrchestrationContractVerifier.Verify("timeout-rate-limit", 5);

    [PcvLegacyContract(
        "pcv.delivery.timeout-rate-limit-hardening-preflight.006",
        "packaging/windows-desktop-node/tests/PcvTimeoutRateLimitHardeningPreflight.Tests.ps1",
        6,
        "does not contain host mutation, service command, HTTP execution, or load generation text")]
    public void Contract006() =>
        OrchestrationContractVerifier.Verify("timeout-rate-limit", 6);
}
