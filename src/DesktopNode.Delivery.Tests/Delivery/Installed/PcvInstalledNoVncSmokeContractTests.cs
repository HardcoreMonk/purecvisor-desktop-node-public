using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvInstalledNoVncSmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.installed-no-vnc-smoke.001",
        "packaging/windows-desktop-node/tests/PcvInstalledNoVncSmoke.Tests.ps1",
        1,
        "ships a target-backed noVNC installed streaming smoke runner with restore and redaction fields")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("installed-no-vnc-smoke", 1);
}
