using System.Xml;
using System.Xml.Linq;

namespace DesktopNode.Delivery.Tests.Installer;

internal sealed class WixSourceContractVerifier
{
    internal const string NamespaceUri = "http://wixtoolset.org/schemas/v4/wxs";
    private const string ErrorCode = "PCV_INSTALLER_WIX_SOURCE_INVALID";
    private static readonly string[] ActionIds =
    [
        "ConfigureInstalled",
        "RepairInstalled",
        "EventLogDefaultTransition",
        "EventLogDefaultTransitionRepair",
        "CredentialManagerDefaultTransition",
        "RemoveInstalled",
        "DataRootRemove",
    ];

    internal WixSourceContractVerifier(string productSource, string actionsSource, string projectSource, string webPayloadSource)
    {
        ProductDocument = Parse(productSource, "product");
        ActionsDocument = Parse(actionsSource, "actions");
        WebPayloadDocument = Parse(webPayloadSource, "web-payload");
        ProjectDocument = Parse(projectSource, "project", requireWixNamespace: false);
        Namespace = NamespaceUri;
        Package = Single(ProductDocument.Descendants(Namespace + "Package"), "package");
        ProductFiles = UniqueById(ProductDocument.Descendants(Namespace + "File"), "product-file");
        ComponentGroupReferences = ProductDocument.Descendants(Namespace + "ComponentGroupRef")
            .Select(RequiredId)
            .ToHashSet(StringComparer.Ordinal);
        CustomActionReferences = ProductDocument.Descendants(Namespace + "CustomActionRef")
            .Select(RequiredId)
            .ToHashSet(StringComparer.Ordinal);
        CustomActions = UniqueById(ActionsDocument.Descendants(Namespace + "CustomAction"), "custom-action");
        SetProperties = UniqueById(ActionsDocument.Descendants(Namespace + "SetProperty"), "set-property");
        SequenceActions = UniqueByAttribute(
            ActionsDocument.Descendants(Namespace + "InstallExecuteSequence").Elements(Namespace + "Custom"),
            "Action",
            "sequence");
        WixProjectSources = ProjectDocument.Descendants("Compile")
            .Select(element => RequiredAttribute(element, "Include"))
            .ToArray();
        ValidateComplete(productSource, actionsSource);
    }

    internal XNamespace Namespace { get; }
    internal XDocument ProductDocument { get; }
    internal XDocument ActionsDocument { get; }
    internal XDocument WebPayloadDocument { get; }
    internal XDocument ProjectDocument { get; }
    internal XElement Package { get; }
    internal IReadOnlyDictionary<string, XElement> ProductFiles { get; }
    internal IReadOnlySet<string> ComponentGroupReferences { get; }
    internal IReadOnlySet<string> CustomActionReferences { get; }
    internal IReadOnlyDictionary<string, XElement> CustomActions { get; }
    internal IReadOnlyDictionary<string, XElement> SetProperties { get; }
    internal IReadOnlyDictionary<string, XElement> SequenceActions { get; }
    internal IReadOnlyList<string> WixProjectSources { get; }

    internal int ProductRootActionCount => CustomActions.Values.Count(action =>
        RequiredAttribute(action, "ExeCommand").Contains(
            "--product-root \"[INSTALLFOLDER].\"",
            StringComparison.Ordinal));

    private void ValidateComplete(string productSource, string actionsSource)
    {
        RequireAttribute(Package, "Name", "PureCVisor Desktop Node", "package-name");
        RequireAttribute(Package, "Manufacturer", "PureCVisor", "package-manufacturer");
        RequireAttribute(Package, "Version", "$(var.MsiProductVersion)", "package-version");
        RequireAttribute(Package, "Scope", "perMachine", "package-scope");
        var upgradeCode = RequiredAttribute(Package, "UpgradeCode");
        if (upgradeCode.Length != 38 || upgradeCode[0] != '{' || upgradeCode[^1] != '}' ||
            !Guid.TryParse(upgradeCode[1..^1], out _))
        {
            throw Invalid("package-upgrade-code");
        }

        RequireElement(ProductDocument, "StandardDirectory", "Id", "ProgramFiles64Folder");
        var vendorDirectory = RequireElement(ProductDocument, "Directory", "Id", "PURECVISORFOLDER");
        RequireAttribute(vendorDirectory, "Name", "PureCVisor", "directory:PURECVISORFOLDER");
        var installDirectory = RequireElement(ProductDocument, "Directory", "Id", "INSTALLFOLDER");
        RequireAttribute(installDirectory, "Name", "DesktopNode", "directory:INSTALLFOLDER");

        foreach (var id in new[] { "DesktopNodePayloadComponents", "DesktopNodeProductWrapperComponents", "DesktopNodeWebPayloadComponents" })
        {
            if (!ComponentGroupReferences.Contains(id))
            {
                throw Invalid($"component-group-ref:{id}");
            }
        }
        // ADR-0018: the generated WebPayload.wxs fragment owns the Single Edge web payload components; every
        // component must carry exactly one File under the web folder tree with an auto GUID.
        var webPayloadGroup = WebPayloadDocument.Descendants(Namespace + "ComponentGroup")
            .SingleOrDefault(element => (string?)element.Attribute("Id") == "DesktopNodeWebPayloadComponents")
            ?? throw Invalid("web-payload:component-group");
        foreach (var component in webPayloadGroup.Elements(Namespace + "Component"))
        {
            var file = Single(component.Elements(Namespace + "File"), "web-payload:file");
            if ((string?)component.Attribute("Guid") != "*" || (string?)file.Attribute("KeyPath") != "yes"
                || !((string?)file.Attribute("Source") ?? string.Empty).StartsWith("$(var.PayloadRoot)\\web\\", StringComparison.Ordinal))
            {
                throw Invalid($"web-payload:component:{(string?)component.Attribute("Id")}");
            }
        }
        // BL-0017 (train 0.42.94 stop): a product ZIP update can create the web folders before an MSI install and
        // Windows Installer only deletes empty folders it created, so every generated directory needs exactly one
        // RemoveFolder (On="uninstall") and Product.wxs must remove the web folder and INSTALLFOLDER the same way.
        var webRemoveTargets = webPayloadGroup.Elements(Namespace + "Component")
            .SelectMany(component => component.Elements(Namespace + "RemoveFolder")
                .Select(remove => (string?)remove.Attribute("Directory") ?? RequiredAttribute(component, "Directory")))
            .ToList();
        foreach (var directoryId in WebPayloadDocument.Descendants(Namespace + "Directory").Select(RequiredId))
        {
            if (webRemoveTargets.Count(target => target == directoryId) != 1)
            {
                throw Invalid($"web-payload:remove-folder:{directoryId}");
            }
        }
        foreach (var remove in WebPayloadDocument.Descendants(Namespace + "RemoveFolder"))
        {
            if ((string?)remove.Attribute("On") != "uninstall")
            {
                throw Invalid($"web-payload:remove-folder-on:{(string?)remove.Attribute("Id")}");
            }
        }
        foreach (var (directoryId, removeId) in new[] { ("DesktopNodeWebFolder", "RemoveWebFolder"), ("INSTALLFOLDER", "RemoveInstallFolder") })
        {
            var remove = Single(
                ProductDocument.Descendants(Namespace + "RemoveFolder").Where(element => (string?)element.Attribute("Id") == removeId),
                $"product:remove-folder:{removeId}");
            if (remove.Parent is null || RequiredAttribute(remove.Parent, "Directory") != directoryId || (string?)remove.Attribute("On") != "uninstall")
            {
                throw Invalid($"product:remove-folder:{removeId}");
            }
        }
        foreach (var id in ActionIds)
        {
            if (!CustomActionReferences.Contains(id))
            {
                throw Invalid($"custom-action-ref:{id}");
            }
            if (!CustomActions.ContainsKey(id))
            {
                throw Invalid($"custom-action:{id}");
            }
        }
        if (CustomActionReferences.Contains("CredentialManagerDefaultTransitionRepair") ||
            CustomActions.ContainsKey("CredentialManagerDefaultTransitionRepair") ||
            SetProperties.ContainsKey("CredentialManagerDefaultTransitionRepairData"))
        {
            throw Invalid("credential-manager-repair-duplicate");
        }

        foreach (var id in ActionIds)
        {
            var action = CustomActions[id];
            RequireAttribute(action, "Directory", "INSTALLFOLDER", $"action-directory:{id}");
            if (action.Attribute("Property") is not null)
            {
                throw Invalid($"action-property:{id}");
            }
            RequireAttribute(action, "Execute", "deferred", $"action-execute:{id}");
            RequireAttribute(action, "Impersonate", "no", $"action-impersonate:{id}");
            RequireAttribute(action, "Return", "check", $"action-return:{id}");
            var command = RequiredAttribute(action, "ExeCommand");
            foreach (var required in new[]
            {
                "\"[INSTALLFOLDER]DesktopNode.Host.exe\"",
                "--product-root \"[INSTALLFOLDER].\"",
                "--data-root \"[DESKTOP_NODE_DATA_ROOT]\"",
                "--service-exe \"[INSTALLFOLDER]DesktopNode.Host.exe\"",
            })
            {
                if (!command.Contains(required, StringComparison.Ordinal))
                {
                    throw Invalid($"action-command:{id}");
                }
            }
            if (command.Contains("[REMOVE_DATA]", StringComparison.Ordinal))
            {
                throw Invalid($"action-raw-remove-data:{id}");
            }
        }

        RequireCommand("ConfigureInstalled", "service-action configure-installed", "[BATCH_EVIDENCE_ROOT_SWITCH]");
        RequireCommand("RepairInstalled", "service-action repair-installed", "[BATCH_EVIDENCE_ROOT_SWITCH]");
        RequireCommand("EventLogDefaultTransition", "service-action eventlog-default-transition", "--eventlog-default-transition-timeout-seconds 60");
        RequireCommand("EventLogDefaultTransitionRepair", "service-action eventlog-default-transition", "--eventlog-default-transition-timeout-seconds 60");
        RequireCommand("CredentialManagerDefaultTransition", "service-action credential-manager-default-transition");
        RequireCommand("RemoveInstalled", "service-action remove-installed", "[REMOVE_DATA_SWITCH]");
        RequireCommand("DataRootRemove", "service-action data-root-remove", "--remove-data");

        RequireSetProperty("DESKTOP_NODE_DATA_ROOT", "[CommonAppDataFolder]PureCVisor\\desktop-node");
        RequireSetProperty("REMOVE_DATA_SWITCH", "--remove-data");
        RequireSetProperty("BATCH_EVIDENCE_ROOT_SWITCH", "--batch-evidence-root \"[BATCH_EVIDENCE_ROOT]\"");
        foreach (var id in new[]
        {
            "ConfigureInstalledData", "RepairInstalledData", "EventLogDefaultTransitionData",
            "EventLogDefaultTransitionRepairData", "CredentialManagerDefaultTransitionData",
            "RemoveInstalledData", "DataRootRemoveData",
        })
        {
            if (!SetProperties.ContainsKey(id))
            {
                throw Invalid($"set-property:{id}");
            }
        }

        RequireSequence("ConfigureInstalled", "After", "InstallFiles", "NOT Installed");
        RequireSequence("EventLogDefaultTransition", "After", "ConfigureInstalled", "NOT Installed");
        RequireSequence("CredentialManagerDefaultTransition", "After", "EventLogDefaultTransition", "NOT Installed");
        RequireSequence("EventLogDefaultTransitionRepair", "After", "CredentialManagerDefaultTransition", "Installed AND NOT REMOVE~=\"ALL\"");
        RequireSequence("RepairInstalled", "After", "EventLogDefaultTransitionRepair", "Installed AND NOT REMOVE~=\"ALL\"");
        RequireSequence("DataRootRemove", "After", "RemoveInstalled", "REMOVE~=\"ALL\" AND REMOVE_DATA=\"1\"");

        if (ProductRootActionCount != 7)
        {
            throw Invalid("product-root-count");
        }
        if (CustomActions.Values.Any(action =>
            RequiredAttribute(action, "ExeCommand").Contains("--product-root \"[INSTALLFOLDER]\"", StringComparison.Ordinal)))
        {
            throw Invalid("product-root-trailing-slash");
        }

        foreach (var (id, source) in new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["DesktopNodeServiceHost"] = "$(var.PayloadRoot)\\DesktopNode.Host.exe",
            ["DesktopNodeCli"] = "$(var.PayloadRoot)\\pcvcli.exe",
            ["DesktopNodeProductEntryPoint"] = "$(var.PayloadRoot)\\Invoke-PcvDesktopNodeProduct.ps1",
            ["DesktopNodeProductModule"] = "$(var.PayloadRoot)\\PcvDesktopNodeProduct.psm1",
            ["DesktopNodeWebIndex"] = "$(var.PayloadRoot)\\web\\index.html",
        })
        {
            var file = RequireDictionaryValue(ProductFiles, id, "product-file");
            RequireAttribute(file, "Source", source, $"product-file-source:{id}");
        }
        if (!ProductFiles.ContainsKey("DesktopNodeWebApp"))
        {
            throw Invalid("product-file:DesktopNodeWebApp");
        }
        foreach (var forbidden in new[]
        {
            "DesktopNodeApiFolder", "DesktopNodeHyperVFolder", "DesktopNodeServiceFolder",
            "DesktopNodeTui", "pcvtui.exe",
        })
        {
            if (productSource.Contains(forbidden, StringComparison.OrdinalIgnoreCase))
            {
                throw Invalid($"product-forbidden:{forbidden}");
            }
        }

        var environment = RequireElement(ProductDocument, "Environment", "Id", "DesktopNodeMachinePath");
        foreach (var (name, value) in new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Name"] = "PATH", ["Value"] = "[INSTALLFOLDER]", ["Part"] = "last",
            ["Action"] = "set", ["System"] = "yes", ["Permanent"] = "no",
        })
        {
            RequireAttribute(environment, name, value, $"environment:{name}");
        }

        if ((string?)ProjectDocument.Root?.Attribute("Sdk") != "WixToolset.Sdk/5.0.2" ||
            !WixProjectSources.SequenceEqual(["Product.wxs", "ProductActions.wxs", "WebPayload.wxs"], StringComparer.Ordinal))
        {
            throw Invalid("wix-project-sources");
        }

        foreach (var forbidden in new[] { "POWERSHELLEXE", "powershell.exe", "WinSwPath", "ApiToken=", "API_TOKEN" })
        {
            if (actionsSource.Contains(forbidden, StringComparison.OrdinalIgnoreCase))
            {
                throw Invalid($"actions-forbidden:{forbidden}");
            }
        }
    }

    private void RequireCommand(string id, params string[] values)
    {
        var command = RequiredAttribute(RequireDictionaryValue(CustomActions, id, "custom-action"), "ExeCommand");
        if (values.Any(value => !command.Contains(value, StringComparison.Ordinal)))
        {
            throw Invalid($"action-command:{id}");
        }
    }

    private void RequireSetProperty(string id, string value)
    {
        var property = RequireDictionaryValue(SetProperties, id, "set-property");
        RequireAttribute(property, "Value", value, $"set-property-value:{id}");
    }

    private void RequireSequence(string id, string relation, string target, string condition)
    {
        var sequence = RequireDictionaryValue(SequenceActions, id, "sequence");
        RequireAttribute(sequence, relation, target, $"sequence-{relation}:{id}");
        RequireAttribute(sequence, "Condition", condition, $"sequence-condition:{id}");
    }

    private XElement RequireElement(XDocument document, string name, string attribute, string value) =>
        Single(
            document.Descendants(Namespace + name)
                .Where(element => (string?)element.Attribute(attribute) == value),
            $"element:{name}:{value}");

    private static T RequireDictionaryValue<T>(
        IReadOnlyDictionary<string, T> dictionary,
        string key,
        string detail)
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            throw Invalid($"{detail}:{key}");
        }
        return value;
    }

    private static XDocument Parse(string source, string detail, bool requireWixNamespace = true)
    {
        try
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using var text = new StringReader(source);
            using var reader = XmlReader.Create(text, settings);
            var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            if (requireWixNamespace && document.Root?.Name.NamespaceName != NamespaceUri)
            {
                throw Invalid($"namespace:{detail}");
            }
            return document;
        }
        catch (XmlException error)
        {
            throw new InvalidDataException($"{ErrorCode}|xml:{detail}", error);
        }
    }

    private static string RequiredId(XElement element) => RequiredAttribute(element, "Id");

    private static IReadOnlyDictionary<string, XElement> UniqueById(
        IEnumerable<XElement> elements,
        string detail) => UniqueByAttribute(elements, "Id", detail);

    private static IReadOnlyDictionary<string, XElement> UniqueByAttribute(
        IEnumerable<XElement> elements,
        string attribute,
        string detail)
    {
        var result = new Dictionary<string, XElement>(StringComparer.Ordinal);
        foreach (var element in elements)
        {
            var key = RequiredAttribute(element, attribute);
            if (!result.TryAdd(key, element))
            {
                throw Invalid($"duplicate:{detail}:{key}");
            }
        }
        return result;
    }

    private static string RequiredAttribute(XElement element, string name)
    {
        var value = (string?)element.Attribute(name);
        return string.IsNullOrEmpty(value) ? throw Invalid($"attribute:{element.Name.LocalName}:{name}") : value;
    }

    private static void RequireAttribute(
        XElement element,
        string name,
        string expected,
        string detail)
    {
        if ((string?)element.Attribute(name) != expected)
        {
            throw Invalid(detail);
        }
    }

    private static XElement Single(IEnumerable<XElement> values, string detail)
    {
        var rows = values.ToArray();
        return rows.Length switch
        {
            1 => rows[0],
            0 => throw Invalid(detail),
            _ => throw Invalid($"duplicate:{detail}"),
        };
    }

    private static InvalidDataException Invalid(string detail) => new($"{ErrorCode}|{detail}");
}
