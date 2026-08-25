namespace DesktopNode.HyperV;

public enum DesktopNodeHyperVAdapterDispatchHandler
{
    HostStatus,
    NetworkInventory,
    VmList,
    CheckpointList,
    CheckpointMutation,
    VmPowerState,
    VmCreate,
    VmDelete,
    VmRename,
    VmManage,
    VmMedia,
    VmResourceMutation,
    GuestExecution
}

public sealed record DesktopNodeHyperVAdapterDispatchCatalogEntry(
    string Operation,
    DesktopNodeHyperVOperationKind Kind,
    DesktopNodeHyperVOperationDomain Domain,
    string ProviderBoundary,
    DesktopNodeHyperVAdapterDispatchHandler Handler)
{
    public string TelemetryOperation => $"hyperv.{Operation}";

    public string ErrorCodePrefix => $"PCV_NATIVE_{Operation.Replace('.', '_').ToUpperInvariant()}_";

    public string ErrorBoundary => $"{ProviderBoundary}:{Handler}";

    public bool MutatesHost => Kind == DesktopNodeHyperVOperationKind.Mutation;
}

public static class DesktopNodeHyperVAdapterDispatchCatalog
{
    public const string ContractKey = "vm-checkpoint-network-fixed";
    public const string DispatchModel = "handler-registry-delegate-map";
    public const string TelemetryErrorContractKey = "operation-level-telemetry-error-contract-v1";

    private static readonly DesktopNodeHyperVAdapterDispatchCatalogEntry[] CatalogEntries =
    [
        new("host.status", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.Host, "host-status-provider", DesktopNodeHyperVAdapterDispatchHandler.HostStatus),
        new("network.inventory", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.Network, "switch-provider", DesktopNodeHyperVAdapterDispatchHandler.NetworkInventory),
        new("vm.list", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.memory-stats", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.cpu-stats", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.blkio-get", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.bandwidth", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.guest-agent-status", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.guest-ping", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.qos.network.preview", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("vm.qos.storage.preview", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider", DesktopNodeHyperVAdapterDispatchHandler.VmList),
        new("checkpoint.list", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-provider", DesktopNodeHyperVAdapterDispatchHandler.CheckpointList),
        new("vm.create", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-create-provider", DesktopNodeHyperVAdapterDispatchHandler.VmCreate),
        new("vm.start", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.shutdown", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.poweroff", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.restart", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.pause", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.resume", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.save", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.resume-saved", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider", DesktopNodeHyperVAdapterDispatchHandler.VmPowerState),
        new("vm.rename", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-rename-provider", DesktopNodeHyperVAdapterDispatchHandler.VmRename),
        new("vm.manage", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-manage-provider", DesktopNodeHyperVAdapterDispatchHandler.VmManage),
        new("vm.eject", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider", DesktopNodeHyperVAdapterDispatchHandler.VmMedia),
        new("vm.attach", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider", DesktopNodeHyperVAdapterDispatchHandler.VmMedia),
        new("vm.limit", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation),
        new("vm.qos.network.set", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation),
        new("vm.qos.storage.set", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation),
        new("vm.guest.exec", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider", DesktopNodeHyperVAdapterDispatchHandler.GuestExecution),
        new("vm.guest.channel.verify", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider", DesktopNodeHyperVAdapterDispatchHandler.GuestExecution),
        new("vm.guest.channel.ensure", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider", DesktopNodeHyperVAdapterDispatchHandler.GuestExecution),
        new("vm.set-memory", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation),
        new("vm.set-vcpu", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation),
        new("vm.disk-resize", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation),
        new("vm.delete", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-delete-provider", DesktopNodeHyperVAdapterDispatchHandler.VmDelete),
        new("checkpoint.create", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.CheckpointMutation),
        new("checkpoint.restore", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.CheckpointMutation),
        new("checkpoint.delete", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-mutation-provider", DesktopNodeHyperVAdapterDispatchHandler.CheckpointMutation)
    ];

    public static IReadOnlyList<DesktopNodeHyperVAdapterDispatchCatalogEntry> Entries => CatalogEntries;

    public static IReadOnlyList<string> OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler handler)
    {
        return CatalogEntries
            .Where(entry => entry.Handler == handler)
            .Select(entry => entry.Operation)
            .ToArray();
    }

    public static bool TryGetEntry(string operation, out DesktopNodeHyperVAdapterDispatchCatalogEntry entry)
    {
        foreach (var candidate in CatalogEntries)
        {
            if (string.Equals(candidate.Operation, operation, StringComparison.Ordinal))
            {
                entry = candidate;
                return true;
            }
        }

        entry = null!;
        return false;
    }
}
