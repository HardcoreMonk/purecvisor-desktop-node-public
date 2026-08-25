using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

[Trait("Category", "Delivery")]
public sealed class PcvInstalledAccountLoginSmokeContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.installed-account-login-smoke.001",
        "packaging/windows-desktop-node/tests/PcvInstalledAccountLoginSmoke.Tests.ps1",
        1,
        "ships an installed account login smoke runner with redacted evidence fields")]
    public void Contract001() =>
        InstalledContractVerifier.Verify("installed-account-login-smoke", 1);
}

