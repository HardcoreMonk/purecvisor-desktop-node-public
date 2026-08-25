using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvInternalHttpsTlsLifecycleSmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.internal-https-tls-lifecycle-smoke.001",
        "packaging/windows-desktop-node/tests/PcvInternalHttpsTlsLifecycleSmoke.Tests.ps1",
        1,
        "fails before TLS binding mutation when the installed token source does not match the declared protected-file baseline")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("internal-https-tls-lifecycle-smoke", 1);
}

