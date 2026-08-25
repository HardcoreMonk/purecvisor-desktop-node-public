using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

[Trait("Category", "Delivery")]
public sealed class PcvBuiltinTlsCertificateLifecyclePreflightContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.builtin-tls-certificate-lifecycle-preflight.001",
        "packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1",
        1,
        "creates a non-mutating built-in TLS certificate lifecycle summary")]
    public void Contract001() =>
        PreflightContractVerifier.Verify("builtin-tls-certificate-lifecycle-preflight", 1);

    [PcvLegacyContract(
        "pcv.delivery.builtin-tls-certificate-lifecycle-preflight.002",
        "packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1",
        2,
        "records the exact built-in TLS lifecycle check names")]
    public void Contract002() =>
        PreflightContractVerifier.Verify("builtin-tls-certificate-lifecycle-preflight", 2);

    [PcvLegacyContract(
        "pcv.delivery.builtin-tls-certificate-lifecycle-preflight.003",
        "packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1",
        3,
        "writes a lifecycle plan preview without creating certificate material")]
    public void Contract003() =>
        PreflightContractVerifier.Verify("builtin-tls-certificate-lifecycle-preflight", 3);

    [PcvLegacyContract(
        "pcv.delivery.builtin-tls-certificate-lifecycle-preflight.004",
        "packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1",
        4,
        "requires plan-only mode")]
    public void Contract004() =>
        PreflightContractVerifier.Verify("builtin-tls-certificate-lifecycle-preflight", 4);

    [PcvLegacyContract(
        "pcv.delivery.builtin-tls-certificate-lifecycle-preflight.005",
        "packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1",
        5,
        "rejects a non-HTTPS bind prefix")]
    public void Contract005() =>
        PreflightContractVerifier.Verify("builtin-tls-certificate-lifecycle-preflight", 5);

    [PcvLegacyContract(
        "pcv.delivery.builtin-tls-certificate-lifecycle-preflight.006",
        "packaging/windows-desktop-node/tests/PcvBuiltinTlsCertificateLifecyclePreflight.Tests.ps1",
        6,
        "does not contain host mutation, certificate creation, trust-store, or TLS binding command text")]
    public void Contract006() =>
        PreflightContractVerifier.Verify("builtin-tls-certificate-lifecycle-preflight", 6);
}
