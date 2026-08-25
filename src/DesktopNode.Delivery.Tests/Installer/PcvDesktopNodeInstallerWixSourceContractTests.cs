using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

[Trait("Category", "Installer")]
public sealed class PcvDesktopNodeInstallerWixSourceContractTests
{
    private readonly WixSourceContractVerifier verifier;

    public PcvDesktopNodeInstallerWixSourceContractTests()
    {
        var repository = RepositoryContractContext.Find();
        verifier = new WixSourceContractVerifier(
            repository.ReadUtf8Text("packaging/windows-desktop-node/installer/Product.wxs"),
            repository.ReadUtf8Text("packaging/windows-desktop-node/installer/ProductActions.wxs"),
            repository.ReadUtf8Text("packaging/windows-desktop-node/installer/PureCVisorDesktopNode.wixproj"));
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.001", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 1, "defines a per-machine MSI product with a fixed UpgradeCode")]
    public void Contract001()
    {
        Assert.Equal("PureCVisor Desktop Node", (string?)verifier.Package.Attribute("Name"));
        Assert.Equal("PureCVisor", (string?)verifier.Package.Attribute("Manufacturer"));
        Assert.Equal("$(var.MsiProductVersion)", (string?)verifier.Package.Attribute("Version"));
        Assert.NotEqual("$(var.ProductVersion)", (string?)verifier.Package.Attribute("Version"));
        Assert.True(Guid.TryParse(((string)verifier.Package.Attribute("UpgradeCode")!)[1..^1], out _));
        Assert.Equal("perMachine", (string?)verifier.Package.Attribute("Scope"));
        Assert.Single(verifier.ProductDocument.Descendants(verifier.Namespace + "StandardDirectory"), element => (string?)element.Attribute("Id") == "ProgramFiles64Folder");
        Assert.Single(verifier.ProductDocument.Descendants(verifier.Namespace + "Directory"), element => (string?)element.Attribute("Id") == "PURECVISORFOLDER" && (string?)element.Attribute("Name") == "PureCVisor");
        Assert.Single(verifier.ProductDocument.Descendants(verifier.Namespace + "Directory"), element => (string?)element.Attribute("Id") == "INSTALLFOLDER" && (string?)element.Attribute("Name") == "DesktopNode");
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.002", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 2, "keeps MSI file ownership separate from service configuration actions")]
    public void Contract002()
    {
        Assert.Contains("DesktopNodePayloadComponents", verifier.ComponentGroupReferences);
        Assert.Contains("DesktopNodeProductWrapperComponents", verifier.ComponentGroupReferences);
        foreach (var id in new[]
        {
            "ConfigureInstalled", "RepairInstalled", "EventLogDefaultTransition",
            "EventLogDefaultTransitionRepair", "CredentialManagerDefaultTransition",
            "RemoveInstalled", "DataRootRemove",
        })
        {
            Assert.Contains(id, verifier.CustomActionReferences);
        }
        Assert.DoesNotContain("CredentialManagerDefaultTransitionRepair", verifier.CustomActionReferences);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.003", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 3, "maps install repair uninstall and remove-data custom actions without raw token properties")]
    public void Contract003()
    {
        foreach (var id in new[]
        {
            "ConfigureInstalled", "RepairInstalled", "EventLogDefaultTransition",
            "EventLogDefaultTransitionRepair", "CredentialManagerDefaultTransition",
            "RemoveInstalled", "DataRootRemove",
        })
        {
            Assert.Contains(id, verifier.CustomActions.Keys);
            Assert.Contains($"{id}Data", verifier.SetProperties.Keys);
        }
        Assert.DoesNotContain("CredentialManagerDefaultTransitionRepair", verifier.CustomActions.Keys);
        Assert.Equal("--remove-data", (string?)verifier.SetProperties["REMOVE_DATA_SWITCH"].Attribute("Value"));
        Assert.Equal("--batch-evidence-root \"[BATCH_EVIDENCE_ROOT]\"", (string?)verifier.SetProperties["BATCH_EVIDENCE_ROOT_SWITCH"].Attribute("Value"));
        Assert.Equal(2, verifier.CustomActions.Values.Count(action => ((string?)action.Attribute("ExeCommand"))?.Contains("eventlog-default-transition", StringComparison.Ordinal) == true && ((string?)action.Attribute("ExeCommand"))?.Contains("--eventlog-default-transition-timeout-seconds 60", StringComparison.Ordinal) == true));
        Assert.DoesNotContain(verifier.CustomActions.Values, action => ((string?)action.Attribute("ExeCommand"))?.Contains("[REMOVE_DATA]", StringComparison.Ordinal) == true);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.004", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 4, "passes the installed payload root as SourceRoot for MSI product actions")]
    public void Contract004()
    {
        Assert.Equal(7, verifier.ProductRootActionCount);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.005", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 5, "runs deferred product actions from the installed payload directory")]
    public void Contract005()
    {
        foreach (var action in verifier.CustomActions.Values)
        {
            Assert.Equal("INSTALLFOLDER", (string?)action.Attribute("Directory"));
            Assert.Null(action.Attribute("Property"));
        }
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.006", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 6, "calculates ProgramData paths without illegal property references")]
    public void Contract006()
    {
        var property = verifier.SetProperties["DESKTOP_NODE_DATA_ROOT"];
        Assert.Equal("[CommonAppDataFolder]PureCVisor\\desktop-node", (string?)property.Attribute("Value"));
        Assert.DoesNotContain(
            verifier.ActionsDocument.Descendants(verifier.Namespace + "Property"),
            element => (string?)element.Attribute("Id") == "DESKTOP_NODE_DATA_ROOT");
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.007", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 7, "does not pass a quoted trailing-backslash INSTALLFOLDER as ProductRoot")]
    public void Contract007()
    {
        foreach (var action in verifier.CustomActions.Values)
        {
            var command = (string)action.Attribute("ExeCommand")!;
            Assert.DoesNotContain("--product-root \"[INSTALLFOLDER]\"", command, StringComparison.Ordinal);
            Assert.Contains("--product-root \"[INSTALLFOLDER].\"", command, StringComparison.Ordinal);
        }
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.008", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 8, "installs only product-owned Desktop Node MSI payload assets")]
    public void Contract008()
    {
        foreach (var id in new[]
        {
            "DesktopNodeServiceHost", "DesktopNodeCli", "DesktopNodeWebApp",
            "DesktopNodeProductEntryPoint", "DesktopNodeProductModule", "DesktopNodeWebIndex",
        })
        {
            Assert.Contains(id, verifier.ProductFiles.Keys);
        }
        var sources = verifier.ProductFiles.Values.Select(file => (string?)file.Attribute("Source")).ToArray();
        Assert.Contains("$(var.PayloadRoot)\\DesktopNode.Host.exe", sources);
        Assert.Contains("$(var.PayloadRoot)\\pcvcli.exe", sources);
        Assert.Contains("$(var.PayloadRoot)\\web\\index.html", sources);
        Assert.DoesNotContain(sources, source => source?.Contains("\\api\\", StringComparison.OrdinalIgnoreCase) == true);
        Assert.DoesNotContain(sources, source => source?.Contains("\\hyperv\\", StringComparison.OrdinalIgnoreCase) == true);
        Assert.DoesNotContain(sources, source => source?.Contains("\\service\\", StringComparison.OrdinalIgnoreCase) == true);
        Assert.DoesNotContain(sources, source => source?.Contains("pcvtui.exe", StringComparison.OrdinalIgnoreCase) == true);
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.009", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 9, "adds the installed Desktop Node folder to the machine PATH for CLI discovery")]
    public void Contract009()
    {
        var environment = Assert.Single(
            verifier.ProductDocument.Descendants(verifier.Namespace + "Environment"),
            element => (string?)element.Attribute("Id") == "DesktopNodeMachinePath");
        Assert.Equal("PATH", (string?)environment.Attribute("Name"));
        Assert.Equal("[INSTALLFOLDER]", (string?)environment.Attribute("Value"));
        Assert.Equal("last", (string?)environment.Attribute("Part"));
        Assert.Equal("set", (string?)environment.Attribute("Action"));
        Assert.Equal("yes", (string?)environment.Attribute("System"));
        Assert.Equal("no", (string?)environment.Attribute("Permanent"));
    }

    [PcvLegacyContract("pcv.installer.desktop-node-installer-wix-source.010", "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1", 10, "includes all WiX source files in the project")]
    public void Contract010()
    {
        Assert.Equal("WixToolset.Sdk/5.0.2", (string?)verifier.ProjectDocument.Root?.Attribute("Sdk"));
        Assert.Equal(["Product.wxs", "ProductActions.wxs"], verifier.WixProjectSources);
    }
}
