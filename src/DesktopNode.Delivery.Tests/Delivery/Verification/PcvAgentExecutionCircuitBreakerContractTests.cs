using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

[Trait("Category", "Delivery")]
public sealed class PcvAgentExecutionCircuitBreakerContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.agent-execution-circuit-breaker.001",
        "packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1",
        1,
        "pins the bounded execution limits in a machine-readable contract")]
    public void Contract001() =>
        DevelopmentPolicyContractVerifier.Verify("agent-circuit-breaker", 1);

    [PcvLegacyContract(
        "pcv.delivery.agent-execution-circuit-breaker.002",
        "packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1",
        2,
        "makes AGENTS load the normative policy without editing generated evidence")]
    public void Contract002() =>
        DevelopmentPolicyContractVerifier.Verify("agent-circuit-breaker", 2);

    [PcvLegacyContract(
        "pcv.delivery.agent-execution-circuit-breaker.003",
        "packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1",
        3,
        "requires stop-only behavior after budget exhaustion and forbids adjacent native expansion")]
    public void Contract003() =>
        DevelopmentPolicyContractVerifier.Verify("agent-circuit-breaker", 3);
}
