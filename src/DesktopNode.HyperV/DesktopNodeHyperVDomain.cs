namespace DesktopNode.HyperV;

public enum DesktopNodeHyperVOperationKind
{
    Read,
    Mutation
}

public enum DesktopNodeHyperVOperationDomain
{
    Host,
    Network,
    VmInventory,
    VmLifecycle,
    Checkpoint,
    GuestExecution
}

public sealed record DesktopNodeHyperVDomainOperation(
    string Operation,
    DesktopNodeHyperVOperationKind Kind,
    DesktopNodeHyperVOperationDomain Domain,
    string ProviderBoundary);

public static class DesktopNodeHyperVDomain
{
    private static readonly DesktopNodeHyperVDomainOperation[] Operations =
    [
        new("host.status", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.Host, "host-status-provider"),
        new("network.inventory", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.Network, "switch-provider"),
        new("vm.list", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.memory-stats", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.cpu-stats", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.blkio-get", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.bandwidth", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.qos.storage.preview", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.qos.network.preview", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.guest-agent-status", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("vm.guest-ping", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider"),
        new("checkpoint.list", DesktopNodeHyperVOperationKind.Read, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-provider"),
        new("vm.create", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-create-provider"),
        new("vm.start", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.shutdown", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.poweroff", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.restart", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.pause", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.resume", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.save", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.resume-saved", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider"),
        new("vm.rename", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-rename-provider"),
        new("vm.manage", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-manage-provider"),
        new("vm.eject", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider"),
        new("vm.attach", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider"),
        new("vm.limit", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider"),
        new("vm.set-memory", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider"),
        new("vm.set-vcpu", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider"),
        new("vm.disk-resize", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider"),
        new("vm.qos.storage.set", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider"),
        new("vm.qos.network.set", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider"),
        new("vm.guest.exec", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider"),
        new("vm.guest.channel.verify", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider"),
        new("vm.guest.channel.ensure", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider"),
        new("vm.delete", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-delete-provider"),
        new("checkpoint.create", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-mutation-provider"),
        new("checkpoint.restore", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-mutation-provider"),
        new("checkpoint.delete", DesktopNodeHyperVOperationKind.Mutation, DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-mutation-provider")
    ];

    public static IReadOnlyList<DesktopNodeHyperVDomainOperation> Catalog => Operations;

    public static bool Handles(string operation)
    {
        return TryGetOperation(operation, out _);
    }

    public static bool IsMutation(string operation)
    {
        return TryGetOperation(operation, out var descriptor) &&
            descriptor.Kind == DesktopNodeHyperVOperationKind.Mutation;
    }

    public static bool TryGetOperation(string operation, out DesktopNodeHyperVDomainOperation descriptor)
    {
        foreach (var candidate in Operations)
        {
            if (string.Equals(candidate.Operation, operation, StringComparison.Ordinal))
            {
                descriptor = candidate;
                return true;
            }
        }

        descriptor = null!;
        return false;
    }
}
