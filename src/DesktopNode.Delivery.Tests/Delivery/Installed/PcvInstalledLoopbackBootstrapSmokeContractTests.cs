using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvInstalledLoopbackBootstrapSmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.installed-loopback-bootstrap-smoke.001",
        "packaging/windows-desktop-node/tests/PcvInstalledLoopbackBootstrapSmoke.Tests.ps1",
        1,
        "ships a redacted installed loopback session and Chromium bootstrap runner")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("installed-loopback-bootstrap-smoke", 1);
}
