using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class HyperVDomainContractTests
{
    [Fact]
    public void HyperVDomainCatalogOwnsCurrentReadAndMutationOperations()
    {
        var operations = DesktopNodeHyperVDomain.Catalog
            .OrderBy(operation => operation.Operation, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(40, operations.Length);
        Assert.Equal(40, operations.Select(operation => operation.Operation).Distinct(StringComparer.Ordinal).Count());

        Assert.Equal(
            [
                "checkpoint.create",
                "checkpoint.delete",
                "checkpoint.list",
                "checkpoint.restore",
                "host.status",
                "network.inventory",
                "vm.attach",
                "vm.bandwidth",
                "vm.blkio-get",
                "vm.clone",
                "vm.clone.preview",
                "vm.cpu-stats",
                "vm.create",
                "vm.delete",
                "vm.disk-resize",
                "vm.eject",
                "vm.guest-agent-status",
                "vm.guest-ping",
                "vm.guest.channel.ensure",
                "vm.guest.channel.verify",
                "vm.guest.exec",
                "vm.limit",
                "vm.list",
                "vm.manage",
                "vm.memory-stats",
                "vm.pause",
                "vm.poweroff",
                "vm.qos.network.preview",
                "vm.qos.network.set",
                "vm.qos.storage.preview",
                "vm.qos.storage.set",
                "vm.rename",
                "vm.restart",
                "vm.resume",
                "vm.resume-saved",
                "vm.save",
                "vm.set-memory",
                "vm.set-vcpu",
                "vm.shutdown",
                "vm.start"
            ],
            operations.Select(operation => operation.Operation).ToArray());

        Assert.All(
            operations.Where(operation => operation.Kind == DesktopNodeHyperVOperationKind.Read),
            operation => Assert.False(DesktopNodeHyperVDomain.IsMutation(operation.Operation)));
        Assert.All(
            operations.Where(operation => operation.Kind == DesktopNodeHyperVOperationKind.Mutation),
            operation => Assert.True(DesktopNodeHyperVDomain.IsMutation(operation.Operation)));
    }

    [Theory]
    [InlineData("host.status", DesktopNodeHyperVOperationDomain.Host, "host-status-provider")]
    [InlineData("network.inventory", DesktopNodeHyperVOperationDomain.Network, "switch-provider")]
    [InlineData("vm.memory-stats", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.cpu-stats", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.blkio-get", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.bandwidth", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.qos.storage.preview", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.qos.network.preview", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.guest-agent-status", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.guest-ping", DesktopNodeHyperVOperationDomain.VmInventory, "vm-provider")]
    [InlineData("vm.guest.exec", DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider")]
    [InlineData("vm.guest.channel.verify", DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider")]
    [InlineData("vm.guest.channel.ensure", DesktopNodeHyperVOperationDomain.GuestExecution, "guest-execution-provider")]
    [InlineData("vm.create", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-create-provider")]
    [InlineData("vm.pause", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider")]
    [InlineData("vm.resume", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider")]
    [InlineData("vm.save", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider")]
    [InlineData("vm.resume-saved", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-power-state-provider")]
    [InlineData("vm.rename", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-rename-provider")]
    [InlineData("vm.manage", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-manage-provider")]
    [InlineData("vm.clone.preview", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-clone-provider")]
    [InlineData("vm.clone", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-clone-provider")]
    [InlineData("vm.eject", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider")]
    [InlineData("vm.attach", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-media-provider")]
    [InlineData("vm.limit", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider")]
    [InlineData("vm.qos.storage.set", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider")]
    [InlineData("vm.qos.network.set", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider")]
    [InlineData("vm.set-memory", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider")]
    [InlineData("vm.set-vcpu", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider")]
    [InlineData("vm.disk-resize", DesktopNodeHyperVOperationDomain.VmLifecycle, "vm-resource-mutation-provider")]
    [InlineData("checkpoint.restore", DesktopNodeHyperVOperationDomain.Checkpoint, "checkpoint-mutation-provider")]
    public void HyperVDomainClassifiesProviderBoundary(string operation, DesktopNodeHyperVOperationDomain domain, string providerBoundary)
    {
        var handled = DesktopNodeHyperVDomain.TryGetOperation(operation, out var descriptor);

        Assert.True(handled);
        Assert.NotNull(descriptor);
        Assert.Equal(domain, descriptor.Domain);
        Assert.Equal(providerBoundary, descriptor.ProviderBoundary);
    }

    [Fact]
    public void HyperVWmiProviderCatalogCoversDomainProviderBoundaries()
    {
        var providerCatalog = DesktopNodeHyperVWmiProviderCatalog.Entries
            .ToDictionary(entry => entry.ProviderBoundary, StringComparer.Ordinal);

        foreach (var operation in DesktopNodeHyperVDomain.Catalog)
        {
            Assert.True(providerCatalog.TryGetValue(operation.ProviderBoundary, out var providerEntry));
            Assert.Equal(operation.Domain, providerEntry.Domain);
            Assert.Contains(operation.Operation, providerEntry.Operations);
        }

        Assert.Equal(
            typeof(DesktopNodeHyperVWmiVmProvider),
            providerCatalog["vm-provider"].ImplementationType);
        Assert.Equal(
            typeof(DesktopNodeHyperVWmiCheckpointMutationProvider),
            providerCatalog["checkpoint-mutation-provider"].ImplementationType);
        Assert.Equal(
            typeof(IDesktopNodeHyperVVmPowerStateProvider),
            providerCatalog["vm-power-state-provider"].ProviderInterface);
        Assert.Equal(
            typeof(IDesktopNodeHyperVGuestExecutionProvider),
            providerCatalog["guest-execution-provider"].ProviderInterface);
    }

    [Fact]
    public void HyperVWmiProviderSetCreatesDefaultBoundaryMapFromCatalog()
    {
        var providerSet = DesktopNodeHyperVProviderSet.CreateDefaultWmi();
        var providerMap = providerSet.ToProviderBoundaryMap();
        var providerCatalog = DesktopNodeHyperVWmiProviderCatalog.Entries
            .ToDictionary(entry => entry.ProviderBoundary, StringComparer.Ordinal);

        Assert.Equal(
            providerCatalog.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray(),
            providerMap.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());

        foreach (var providerEntry in providerCatalog.Values)
        {
            Assert.True(providerMap.TryGetValue(providerEntry.ProviderBoundary, out var provider));
            Assert.True(providerEntry.ProviderInterface.IsInstanceOfType(provider));
            Assert.Equal(providerEntry.ImplementationType, provider.GetType());
        }

        Assert.Same(providerSet.SwitchProvider, providerMap["switch-provider"]);
        Assert.Same(providerSet.HostStatusProvider, providerMap["host-status-provider"]);
    }

    [Fact]
    public void HyperVWmiProviderCatalogPublishesProviderSetFactoryCallSites()
    {
        var providerSet = DesktopNodeHyperVProviderSet.CreateDefaultWmi();
        var providerMap = providerSet.ToProviderBoundaryMap();

        foreach (var providerEntry in DesktopNodeHyperVWmiProviderCatalog.Entries)
        {
            Assert.Equal("provider-set-factory-callsite-v1", providerEntry.FactoryCallSiteContract);
            Assert.StartsWith(
                "DesktopNodeHyperVProviderSet.CreateDefaultWmi",
                providerEntry.FactoryCallSite,
                StringComparison.Ordinal);

            var providerProperty = typeof(DesktopNodeHyperVProviderSet).GetProperty(providerEntry.ProviderSetPropertyName);
            Assert.NotNull(providerProperty);
            Assert.Same(providerProperty.GetValue(providerSet), providerMap[providerEntry.ProviderBoundary]);
        }

        Assert.Equal("VmProvider", DesktopNodeHyperVWmiProviderCatalog.Entries.Single(entry => entry.ProviderBoundary == "vm-provider").ProviderSetPropertyName);
        Assert.Equal("CheckpointMutationProvider", DesktopNodeHyperVWmiProviderCatalog.Entries.Single(entry => entry.ProviderBoundary == "checkpoint-mutation-provider").ProviderSetPropertyName);
    }

    [Fact]
    public void HyperVWmiHelperCatalogPublishesCommonProviderBoundary()
    {
        Assert.Equal("hyperv-wmi-common-helper-contract-v1", DesktopNodeHyperVWmiHelperCatalog.ContractKey);
        Assert.Equal(@"root\virtualization\v2", DesktopNodeHyperVWmiHelperCatalog.NamespacePath);
        Assert.Equal(
            "SELECT * FROM Msvm_ComputerSystem WHERE Description = 'Microsoft Virtual Machine'",
            DesktopNodeHyperVWmiHelperCatalog.VmQuery);

        var helpers = DesktopNodeHyperVWmiHelperCatalog.Entries
            .ToDictionary(entry => entry.HelperName, StringComparer.Ordinal);

        Assert.Equal(
            [
                "method-result-wait",
                "safe-property-read",
                "scope",
                "single-service",
                "vm-lookup"
            ],
            helpers.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());
        Assert.All(helpers.Values, helper => Assert.Equal("DesktopNodeHyperVWmiCommon", helper.Owner));
        Assert.Contains("DesktopNodeHyperVWmiVmProvider", helpers["vm-lookup"].Consumers);
        Assert.Contains("DesktopNodeHyperVWmiCheckpointMutationProvider", helpers["method-result-wait"].Consumers);

        foreach (var providerEntry in DesktopNodeHyperVWmiProviderCatalog.Entries)
        {
            Assert.Equal(DesktopNodeHyperVWmiHelperCatalog.NamespacePath, providerEntry.WmiNamespacePath);
        }
    }

    [Fact]
    public void HyperVAdapterDispatchCatalogCoversDomainProviderBoundaries()
    {
        var providerSet = DesktopNodeHyperVProviderSet.CreateDefaultWmi();
        var providerMap = providerSet.ToProviderBoundaryMap();
        var dispatchCatalog = DesktopNodeHyperVAdapterDispatchCatalog.Entries
            .ToDictionary(entry => entry.Operation, StringComparer.Ordinal);

        Assert.Equal(40, dispatchCatalog.Count);

        foreach (var operation in DesktopNodeHyperVDomain.Catalog)
        {
            Assert.True(dispatchCatalog.TryGetValue(operation.Operation, out var dispatchEntry));
            Assert.Equal(operation.Kind, dispatchEntry.Kind);
            Assert.Equal(operation.Domain, dispatchEntry.Domain);
            Assert.Equal(operation.ProviderBoundary, dispatchEntry.ProviderBoundary);
            Assert.True(providerMap.ContainsKey(dispatchEntry.ProviderBoundary));
        }

        Assert.Equal(
            DesktopNodeHyperVDomain.Catalog.Select(operation => operation.Operation).OrderBy(operation => operation, StringComparer.Ordinal).ToArray(),
            dispatchCatalog.Keys.OrderBy(operation => operation, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void HyperVAdapterDispatchCatalogPublishesPost04218HandlerContract()
    {
        Assert.Equal("vm-checkpoint-network-fixed", DesktopNodeHyperVAdapterDispatchCatalog.ContractKey);

        Assert.Equal(
            [
                "network.inventory"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.NetworkInventory));
        Assert.Equal(
            [
                "vm.list",
                "vm.memory-stats",
                "vm.cpu-stats",
                "vm.blkio-get",
                "vm.bandwidth",
                "vm.guest-agent-status",
                "vm.guest-ping",
                "vm.qos.network.preview",
                "vm.qos.storage.preview"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmList));
        Assert.Equal(
            [
                "vm.start",
                "vm.shutdown",
                "vm.poweroff",
                "vm.restart",
                "vm.pause",
                "vm.resume",
                "vm.save",
                "vm.resume-saved"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmPowerState));
        Assert.Equal(
            [
                "vm.rename"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmRename));
        Assert.Equal(
            [
                "vm.manage"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmManage));
        Assert.Equal(
            [
                "vm.clone.preview"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmClonePreview));
        Assert.Equal(
            [
                "vm.clone"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmClone));
        Assert.Equal(
            [
                "vm.eject",
                "vm.attach"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmMedia));
        Assert.Equal(
            [
                "vm.limit",
                "vm.qos.network.set",
                "vm.qos.storage.set",
                "vm.set-memory",
                "vm.set-vcpu",
                "vm.disk-resize"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation));
        Assert.Equal(
            [
                "vm.guest.exec",
                "vm.guest.channel.verify",
                "vm.guest.channel.ensure"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.GuestExecution));
        Assert.Equal(
            [
                "checkpoint.create",
                "checkpoint.restore",
                "checkpoint.delete"
            ],
            DesktopNodeHyperVAdapterDispatchCatalog.OperationsForHandler(DesktopNodeHyperVAdapterDispatchHandler.CheckpointMutation));

        Assert.All(
            DesktopNodeHyperVAdapterDispatchCatalog.Entries,
            entry => Assert.Equal(entry.ProviderBoundary, DesktopNodeHyperVDomain.Catalog.Single(operation => operation.Operation == entry.Operation).ProviderBoundary));
    }

    [Fact]
    public void HyperVNativeAdapterPublishesDelegateRegistryDispatchModel()
    {
        var adapter = DesktopNodeHyperVNativeAdapter.CreateDefault();

        Assert.Equal("handler-registry-delegate-map", DesktopNodeHyperVAdapterDispatchCatalog.DispatchModel);
        Assert.Equal(
            Enum.GetValues<DesktopNodeHyperVAdapterDispatchHandler>()
                .OrderBy(handler => handler.ToString(), StringComparer.Ordinal)
                .ToArray(),
            adapter.RegisteredDispatchHandlers
                .OrderBy(handler => handler.ToString(), StringComparer.Ordinal)
                .ToArray());
        Assert.DoesNotContain(
            DesktopNodeHyperVAdapterDispatchCatalog.Entries,
            entry => !adapter.RegisteredDispatchHandlers.Contains(entry.Handler));
    }

    [Fact]
    public void HyperVDispatchCatalogPublishesOperationTelemetryAndErrorContracts()
    {
        Assert.Equal("operation-level-telemetry-error-contract-v1", DesktopNodeHyperVAdapterDispatchCatalog.TelemetryErrorContractKey);

        foreach (var entry in DesktopNodeHyperVAdapterDispatchCatalog.Entries)
        {
            Assert.Equal($"hyperv.{entry.Operation}", entry.TelemetryOperation);
            Assert.StartsWith("PCV_NATIVE_", entry.ErrorCodePrefix, StringComparison.Ordinal);
            Assert.EndsWith("_", entry.ErrorCodePrefix, StringComparison.Ordinal);
            Assert.Contains(entry.ProviderBoundary, entry.ErrorBoundary, StringComparison.Ordinal);
            Assert.Equal(entry.Kind == DesktopNodeHyperVOperationKind.Mutation, entry.MutatesHost);
        }
    }

    [Fact]
    public void HyperVDomainRejectsOperationsOutsideTheNativeDomain()
    {
        var handled = DesktopNodeHyperVDomain.TryGetOperation("service-action.status", out var descriptor);

        Assert.False(handled);
        Assert.Null(descriptor);
        Assert.False(DesktopNodeHyperVDomain.Handles("service-action.status"));
    }
}
