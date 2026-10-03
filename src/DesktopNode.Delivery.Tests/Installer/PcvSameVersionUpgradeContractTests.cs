using System.Xml.Linq;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

// Option A of docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-same-version-rebuild-installer-design.md:
// a rebuilt MSI of the same version must replace the installed product instead of registering beside it.
[Trait("Category", "Delivery")]
public sealed class PcvSameVersionUpgradeContractTests
{
    private static readonly XNamespace Wix = "http://wixtoolset.org/schemas/v4/wxs";

    [Fact]
    public void MajorUpgradeAllowsSameVersionAndRemovesThePreviousProductFirst()
    {
        var product = XDocument.Parse(
            RepositoryContractContext.Find().ReadUtf8Text("packaging/windows-desktop-node/installer/Product.wxs"));
        var majorUpgrade = Assert.Single(product.Descendants(Wix + "MajorUpgrade"));

        Assert.Equal("yes", (string?)majorUpgrade.Attribute("AllowSameVersionUpgrades"));
        Assert.Equal("afterInstallValidate", (string?)majorUpgrade.Attribute("Schedule"));
        Assert.False(string.IsNullOrWhiteSpace((string?)majorUpgrade.Attribute("DowngradeErrorMessage")));
        Assert.Null(majorUpgrade.Attribute("AllowDowngrades"));
    }
}
