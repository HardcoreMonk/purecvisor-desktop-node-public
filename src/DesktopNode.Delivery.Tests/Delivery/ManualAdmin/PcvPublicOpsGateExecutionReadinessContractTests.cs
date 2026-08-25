using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvPublicOpsGateExecutionReadinessContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.public-ops-gate-execution-readiness.001",
        "packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1",
        1,
        "requires explicit local evidence write opt-in")]
    public void Contract001() =>
        ManualAdminContractVerifier.Verify("public-ops-gate-execution-readiness", 1);

    [PcvLegacyContract(
        "pcv.delivery.public-ops-gate-execution-readiness.002",
        "packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1",
        2,
        "records the six remaining requested gates as blocked or pending without public claims")]
    public void Contract002() =>
        ManualAdminContractVerifier.Verify("public-ops-gate-execution-readiness", 2);

    [PcvLegacyContract(
        "pcv.delivery.public-ops-gate-execution-readiness.003",
        "packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1",
        3,
        "stages catalog/package locally and runs a non-mutating TLS certificate lifecycle slice when opted in")]
    public void Contract003() =>
        ManualAdminContractVerifier.Verify("public-ops-gate-execution-readiness", 3);

    [PcvLegacyContract(
        "pcv.delivery.public-ops-gate-execution-readiness.004",
        "packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1",
        4,
        "imports a SYSTEM-context credential proof artifact when one is supplied")]
    public void Contract004() =>
        ManualAdminContractVerifier.Verify("public-ops-gate-execution-readiness", 4);

    [PcvLegacyContract(
        "pcv.delivery.public-ops-gate-execution-readiness.005",
        "packaging/windows-desktop-node/tests/PcvPublicOpsGateExecutionReadiness.Tests.ps1",
        5,
        "does not contain direct public submission, clean-host execution, or host mutation command text")]
    public void Contract005() =>
        ManualAdminContractVerifier.Verify("public-ops-gate-execution-readiness", 5);
}

