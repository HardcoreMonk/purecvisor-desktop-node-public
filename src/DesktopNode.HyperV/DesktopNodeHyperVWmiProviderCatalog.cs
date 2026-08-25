namespace DesktopNode.HyperV;

public sealed record DesktopNodeHyperVWmiHelperCatalogEntry(
    string HelperName,
    string Owner,
    IReadOnlyList<string> Consumers);

public sealed record DesktopNodeHyperVWmiProviderCatalogEntry(
    string ProviderBoundary,
    string WmiNamespacePath,
    DesktopNodeHyperVOperationDomain Domain,
    Type ProviderInterface,
    Type ImplementationType,
    IReadOnlyList<string> Operations,
    string ProviderSetPropertyName,
    string FactoryCallSite)
{
    public string FactoryCallSiteContract => "provider-set-factory-callsite-v1";
}

public static class DesktopNodeHyperVWmiHelperCatalog
{
    public const string ContractKey = "hyperv-wmi-common-helper-contract-v1";
    public const string NamespacePath = DesktopNodeHyperVWmiCommon.NamespacePath;
    public const string VmQuery = DesktopNodeHyperVWmiCommon.VmQuery;

    private static readonly DesktopNodeHyperVWmiHelperCatalogEntry[] CatalogEntries =
    [
        new(
            "scope",
            nameof(DesktopNodeHyperVWmiCommon),
            [
                nameof(DesktopNodeHyperVNativeHostStatusProvider),
                nameof(DesktopNodeHyperVWmiCheckpointMutationProvider),
                nameof(DesktopNodeHyperVWmiCheckpointProvider),
                nameof(DesktopNodeHyperVWmiSwitchProvider),
                nameof(DesktopNodeHyperVWmiVmCreateProvider),
                nameof(DesktopNodeHyperVWmiVmDeleteProvider),
                nameof(DesktopNodeHyperVWmiVmMediaProvider),
                nameof(DesktopNodeHyperVWmiVmRenameProvider),
                nameof(DesktopNodeHyperVWmiVmManageProvider),
                nameof(DesktopNodeHyperVWmiVmResourceMutationProvider),
                nameof(DesktopNodeHyperVWmiVmPowerStateProvider),
                nameof(DesktopNodeHyperVWmiVmProvider)
            ]),
        new(
            "vm-lookup",
            nameof(DesktopNodeHyperVWmiCommon),
            [
                nameof(DesktopNodeHyperVWmiCheckpointMutationProvider),
                nameof(DesktopNodeHyperVWmiCheckpointProvider),
                nameof(DesktopNodeHyperVWmiVmCreateProvider),
                nameof(DesktopNodeHyperVWmiVmDeleteProvider),
                nameof(DesktopNodeHyperVWmiVmMediaProvider),
                nameof(DesktopNodeHyperVWmiVmRenameProvider),
                nameof(DesktopNodeHyperVWmiVmManageProvider),
                nameof(DesktopNodeHyperVWmiVmResourceMutationProvider),
                nameof(DesktopNodeHyperVWmiVmPowerStateProvider),
                nameof(DesktopNodeHyperVWmiVmProvider)
            ]),
        new(
            "single-service",
            nameof(DesktopNodeHyperVWmiCommon),
            [
                nameof(DesktopNodeHyperVWmiCheckpointMutationProvider),
                nameof(DesktopNodeHyperVWmiVmCreateProvider),
                nameof(DesktopNodeHyperVWmiVmDeleteProvider),
                nameof(DesktopNodeHyperVWmiVmMediaProvider),
                nameof(DesktopNodeHyperVWmiVmRenameProvider),
                nameof(DesktopNodeHyperVWmiVmManageProvider)
            ]),
        new(
            "method-result-wait",
            nameof(DesktopNodeHyperVWmiCommon),
            [
                nameof(DesktopNodeHyperVWmiCheckpointMutationProvider),
                nameof(DesktopNodeHyperVWmiVmCreateProvider),
                nameof(DesktopNodeHyperVWmiVmDeleteProvider),
                nameof(DesktopNodeHyperVWmiVmMediaProvider),
                nameof(DesktopNodeHyperVWmiVmRenameProvider),
                nameof(DesktopNodeHyperVWmiVmManageProvider),
                nameof(DesktopNodeHyperVWmiVmResourceMutationProvider),
                nameof(DesktopNodeHyperVWmiVmPowerStateProvider)
            ]),
        new(
            "safe-property-read",
            nameof(DesktopNodeHyperVWmiCommon),
            [
                nameof(DesktopNodeHyperVNativeHostStatusProvider),
                nameof(DesktopNodeHyperVWmiCheckpointMutationProvider),
                nameof(DesktopNodeHyperVWmiCheckpointProvider),
                nameof(DesktopNodeHyperVWmiSwitchProvider),
                nameof(DesktopNodeHyperVWmiVmCreateProvider),
                nameof(DesktopNodeHyperVWmiVmMediaProvider),
                nameof(DesktopNodeHyperVWmiVmRenameProvider),
                nameof(DesktopNodeHyperVWmiVmManageProvider),
                nameof(DesktopNodeHyperVWmiVmResourceMutationProvider),
                nameof(DesktopNodeHyperVWmiVmProvider)
            ])
    ];

    public static IReadOnlyList<DesktopNodeHyperVWmiHelperCatalogEntry> Entries => CatalogEntries;
}

public static class DesktopNodeHyperVWmiProviderCatalog
{
    private static readonly DesktopNodeHyperVWmiProviderCatalogEntry[] CatalogEntries =
    [
        new(
            "host-status-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.Host,
            typeof(IDesktopNodeHyperVHostStatusProvider),
            typeof(DesktopNodeHyperVNativeHostStatusProvider),
            ["host.status"],
            nameof(DesktopNodeHyperVProviderSet.HostStatusProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.HostStatusProvider)}"),
        new(
            "switch-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.Network,
            typeof(IDesktopNodeHyperVSwitchProvider),
            typeof(DesktopNodeHyperVWmiSwitchProvider),
            ["network.inventory"],
            nameof(DesktopNodeHyperVProviderSet.SwitchProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.SwitchProvider)}"),
        new(
            "vm-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmInventory,
            typeof(IDesktopNodeHyperVVmProvider),
            typeof(DesktopNodeHyperVWmiVmProvider),
            [
                "vm.list",
                "vm.memory-stats",
                "vm.cpu-stats",
                "vm.blkio-get",
                "vm.bandwidth",
                "vm.qos.storage.preview",
                "vm.qos.network.preview",
                "vm.guest-agent-status",
                "vm.guest-ping"
            ],
            nameof(DesktopNodeHyperVProviderSet.VmProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmProvider)}"),
        new(
            "checkpoint-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.Checkpoint,
            typeof(IDesktopNodeHyperVCheckpointProvider),
            typeof(DesktopNodeHyperVWmiCheckpointProvider),
            ["checkpoint.list"],
            nameof(DesktopNodeHyperVProviderSet.CheckpointProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.CheckpointProvider)}"),
        new(
            "vm-create-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmLifecycle,
            typeof(IDesktopNodeHyperVVmCreateProvider),
            typeof(DesktopNodeHyperVWmiVmCreateProvider),
            ["vm.create"],
            nameof(DesktopNodeHyperVProviderSet.VmCreateProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmCreateProvider)}"),
        new(
            "vm-power-state-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmLifecycle,
            typeof(IDesktopNodeHyperVVmPowerStateProvider),
            typeof(DesktopNodeHyperVWmiVmPowerStateProvider),
            ["vm.start", "vm.shutdown", "vm.poweroff", "vm.restart", "vm.pause", "vm.resume", "vm.save", "vm.resume-saved"],
            nameof(DesktopNodeHyperVProviderSet.VmPowerStateProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmPowerStateProvider)}"),
        new(
            "vm-rename-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmLifecycle,
            typeof(IDesktopNodeHyperVVmRenameProvider),
            typeof(DesktopNodeHyperVWmiVmRenameProvider),
            ["vm.rename"],
            nameof(DesktopNodeHyperVProviderSet.VmRenameProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmRenameProvider)}"),
        new(
            "vm-manage-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmLifecycle,
            typeof(IDesktopNodeHyperVVmManageProvider),
            typeof(DesktopNodeHyperVWmiVmManageProvider),
            ["vm.manage"],
            nameof(DesktopNodeHyperVProviderSet.VmManageProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmManageProvider)}"),
        new(
            "vm-delete-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmLifecycle,
            typeof(IDesktopNodeHyperVVmDeleteProvider),
            typeof(DesktopNodeHyperVWmiVmDeleteProvider),
            ["vm.delete"],
            nameof(DesktopNodeHyperVProviderSet.VmDeleteProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmDeleteProvider)}"),
        new(
            "vm-media-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmLifecycle,
            typeof(IDesktopNodeHyperVVmMediaProvider),
            typeof(DesktopNodeHyperVWmiVmMediaProvider),
            ["vm.eject", "vm.attach"],
            nameof(DesktopNodeHyperVProviderSet.VmMediaProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmMediaProvider)}"),
        new(
            "vm-resource-mutation-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.VmLifecycle,
            typeof(IDesktopNodeHyperVVmResourceMutationProvider),
            typeof(DesktopNodeHyperVWmiVmResourceMutationProvider),
            ["vm.set-memory", "vm.set-vcpu", "vm.disk-resize", "vm.limit", "vm.qos.storage.set", "vm.qos.network.set"],
            nameof(DesktopNodeHyperVProviderSet.VmResourceMutationProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.VmResourceMutationProvider)}"),
        new(
            "guest-execution-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.GuestExecution,
            typeof(IDesktopNodeHyperVGuestExecutionProvider),
            typeof(DesktopNodeHyperVPowerShellDirectGuestExecutionProvider),
            ["vm.guest.exec", "vm.guest.channel.verify", "vm.guest.channel.ensure"],
            nameof(DesktopNodeHyperVProviderSet.GuestExecutionProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.GuestExecutionProvider)}"),
        new(
            "checkpoint-mutation-provider",
            DesktopNodeHyperVWmiHelperCatalog.NamespacePath,
            DesktopNodeHyperVOperationDomain.Checkpoint,
            typeof(IDesktopNodeHyperVCheckpointMutationProvider),
            typeof(DesktopNodeHyperVWmiCheckpointMutationProvider),
            ["checkpoint.create", "checkpoint.restore", "checkpoint.delete"],
            nameof(DesktopNodeHyperVProviderSet.CheckpointMutationProvider),
            $"{nameof(DesktopNodeHyperVProviderSet)}.{nameof(DesktopNodeHyperVProviderSet.CreateDefaultWmi)}:{nameof(DesktopNodeHyperVProviderSet.CheckpointMutationProvider)}")
    ];

    public static IReadOnlyList<DesktopNodeHyperVWmiProviderCatalogEntry> Entries => CatalogEntries;

    public static bool TryGetEntry(string providerBoundary, out DesktopNodeHyperVWmiProviderCatalogEntry entry)
    {
        foreach (var candidate in CatalogEntries)
        {
            if (string.Equals(candidate.ProviderBoundary, providerBoundary, StringComparison.Ordinal))
            {
                entry = candidate;
                return true;
            }
        }

        entry = null!;
        return false;
    }
}
