using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed partial class DesktopNodeHyperVNativeAdapter : IDesktopNodeHyperVNativeAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
    };

    private readonly IDesktopNodeHyperVSwitchProvider switchProvider;
    private readonly IDesktopNodeHyperVHostStatusProvider hostStatusProvider;
    private readonly IDesktopNodeHyperVVmProvider vmProvider;
    private readonly IDesktopNodeHyperVCheckpointProvider checkpointProvider;
    private readonly IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider;
    private readonly IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider;
    private readonly IDesktopNodeHyperVVmCreateProvider vmCreateProvider;
    private readonly IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider;
    private readonly IDesktopNodeHyperVVmRenameProvider vmRenameProvider;
    private readonly IDesktopNodeHyperVVmManageProvider vmManageProvider;
    private readonly IDesktopNodeHyperVVmCloneProvider vmCloneProvider;
    private readonly IDesktopNodeHyperVVmExportProvider vmExportProvider;
    private readonly IDesktopNodeHyperVVmImportProvider vmImportProvider;
    private readonly IDesktopNodeHyperVVmNetworkConnectProvider vmNetworkConnectProvider;
    private readonly IDesktopNodeHyperVVmMediaProvider vmMediaProvider;
    private readonly IDesktopNodeHyperVVmResourceMutationProvider vmResourceMutationProvider;
    private readonly IDesktopNodeHyperVGuestExecutionProvider guestExecutionProvider;
    private readonly IReadOnlyDictionary<DesktopNodeHyperVAdapterDispatchHandler, HyperVAdapterDispatchInvoker> dispatchHandlers;

    public DesktopNodeHyperVNativeAdapter(
        IDesktopNodeHyperVSwitchProvider switchProvider,
        IDesktopNodeHyperVHostStatusProvider hostStatusProvider,
        IDesktopNodeHyperVVmProvider vmProvider,
        IDesktopNodeHyperVCheckpointProvider checkpointProvider,
        IDesktopNodeHyperVCheckpointMutationProvider checkpointMutationProvider,
        IDesktopNodeHyperVVmPowerStateProvider vmPowerStateProvider,
        IDesktopNodeHyperVVmCreateProvider vmCreateProvider,
        IDesktopNodeHyperVVmDeleteProvider vmDeleteProvider,
        IDesktopNodeHyperVVmRenameProvider vmRenameProvider,
        IDesktopNodeHyperVVmManageProvider vmManageProvider,
        IDesktopNodeHyperVVmCloneProvider vmCloneProvider,
        IDesktopNodeHyperVVmMediaProvider vmMediaProvider,
        IDesktopNodeHyperVVmResourceMutationProvider vmResourceMutationProvider,
        IDesktopNodeHyperVGuestExecutionProvider? guestExecutionProvider = null,
        IDesktopNodeHyperVVmExportProvider? vmExportProvider = null,
        IDesktopNodeHyperVVmImportProvider? vmImportProvider = null,
        IDesktopNodeHyperVVmNetworkConnectProvider? vmNetworkConnectProvider = null)
    {
        this.switchProvider = switchProvider;
        this.hostStatusProvider = hostStatusProvider;
        this.vmProvider = vmProvider;
        this.checkpointProvider = checkpointProvider;
        this.checkpointMutationProvider = checkpointMutationProvider;
        this.vmPowerStateProvider = vmPowerStateProvider;
        this.vmCreateProvider = vmCreateProvider;
        this.vmDeleteProvider = vmDeleteProvider;
        this.vmRenameProvider = vmRenameProvider;
        this.vmManageProvider = vmManageProvider;
        this.vmCloneProvider = vmCloneProvider;
        this.vmExportProvider = vmExportProvider ?? new DesktopNodeHyperVWmiVmExportProvider();
        this.vmImportProvider = vmImportProvider ?? new DesktopNodeHyperVWmiVmImportProvider();
        this.vmNetworkConnectProvider = vmNetworkConnectProvider ?? new DesktopNodeHyperVWmiVmNetworkConnectProvider();
        this.vmMediaProvider = vmMediaProvider;
        this.vmResourceMutationProvider = vmResourceMutationProvider;
        this.guestExecutionProvider = guestExecutionProvider ?? new DesktopNodeHyperVPowerShellDirectGuestExecutionProvider();
        dispatchHandlers = new Dictionary<DesktopNodeHyperVAdapterDispatchHandler, HyperVAdapterDispatchInvoker>
        {
            [DesktopNodeHyperVAdapterDispatchHandler.HostStatus] = InvokeHostStatus,
            [DesktopNodeHyperVAdapterDispatchHandler.NetworkInventory] = InvokeNetworkInventory,
            [DesktopNodeHyperVAdapterDispatchHandler.VmList] = InvokeVmList,
            [DesktopNodeHyperVAdapterDispatchHandler.CheckpointList] = InvokeCheckpointList,
            [DesktopNodeHyperVAdapterDispatchHandler.CheckpointMutation] = InvokeCheckpointMutation,
            [DesktopNodeHyperVAdapterDispatchHandler.VmPowerState] = InvokeVmPowerState,
            [DesktopNodeHyperVAdapterDispatchHandler.VmCreate] = InvokeVmCreate,
            [DesktopNodeHyperVAdapterDispatchHandler.VmDelete] = InvokeVmDelete,
            [DesktopNodeHyperVAdapterDispatchHandler.VmRename] = InvokeVmRename,
            [DesktopNodeHyperVAdapterDispatchHandler.VmManage] = InvokeVmManage,
            [DesktopNodeHyperVAdapterDispatchHandler.VmClonePreview] = InvokeVmClonePreview,
            [DesktopNodeHyperVAdapterDispatchHandler.VmClone] = InvokeVmClone,
            [DesktopNodeHyperVAdapterDispatchHandler.VmExport] = InvokeVmExport,
            [DesktopNodeHyperVAdapterDispatchHandler.VmImport] = InvokeVmImport,
            [DesktopNodeHyperVAdapterDispatchHandler.VmNetworkConnect] = InvokeVmNetworkConnect,
            [DesktopNodeHyperVAdapterDispatchHandler.VmMedia] = InvokeVmMedia,
            [DesktopNodeHyperVAdapterDispatchHandler.VmResourceMutation] = InvokeVmResourceMutation,
            [DesktopNodeHyperVAdapterDispatchHandler.GuestExecution] = InvokeGuestExecution
        };
    }

    public IReadOnlyCollection<DesktopNodeHyperVAdapterDispatchHandler> RegisteredDispatchHandlers => dispatchHandlers.Keys.ToArray();

    public static DesktopNodeHyperVNativeAdapter CreateDefault()
    {
        return new DesktopNodeHyperVNativeAdapter(DesktopNodeHyperVProviderSet.CreateDefaultWmi());
    }

    public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        if (!DesktopNodeHyperVDomain.TryGetOperation(operation, out var domainOperation))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                "The native adapter does not handle this operation.",
                "The operation is not part of the native Hyper-V domain catalog.",
                false);
            return false;
        }

        if (!DesktopNodeHyperVAdapterDispatchCatalog.TryGetEntry(operation, out var dispatch))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_ROUTE_NOT_HANDLED",
                "The native adapter does not handle this operation.",
                "The operation is in the native Hyper-V domain catalog but has no adapter dispatch handler.",
                false);
            return false;
        }

        if (dispatch.Kind != domainOperation.Kind ||
            dispatch.Domain != domainOperation.Domain ||
            !string.Equals(dispatch.ProviderBoundary, domainOperation.ProviderBoundary, StringComparison.Ordinal))
        {
            result = DesktopNodeHyperVOperationResult.Failure(
                operation,
                "PCV_NATIVE_DISPATCH_PROVIDER_BOUNDARY_DRIFT",
                "The native adapter dispatch catalog does not match the Hyper-V domain catalog.",
                "The operation provider boundary, domain, or mutation kind drifted between catalogs.",
                false);
            return false;
        }

        if (!dispatchHandlers.TryGetValue(dispatch.Handler, out var handler))
        {
            throw new InvalidOperationException($"Unsupported Hyper-V adapter dispatch handler '{dispatch.Handler}'.");
        }

        if (TryRejectTemplateLockedMutation(operation, domainOperation, parameters, cancellationToken, out result))
        {
            return true;
        }

        return handler(operation, parameters, cancellationToken, out result);
    }

    private bool TryRejectTemplateLockedMutation(
        string operation,
        DesktopNodeHyperVDomainOperation domainOperation,
        JsonElement parameters,
        CancellationToken cancellationToken,
        out DesktopNodeHyperVOperationResult result)
    {
        result = default!;
        if (domainOperation.Kind != DesktopNodeHyperVOperationKind.Mutation ||
            DesktopNodeHyperVVmTemplateLockGuard.IsMutationAllowed(operation))
        {
            return false;
        }

        var vmName = GetStringProperty(parameters, "name") ??
            GetStringProperty(parameters, "vm_name") ??
            GetStringProperty(parameters, "source");
        if (string.IsNullOrWhiteSpace(vmName))
        {
            return false;
        }

        IReadOnlyList<DesktopNodeHyperVVmInfo> vms;
        try
        {
            vms = vmProvider.GetVms(cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }

        var vm = vms.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, vmName, StringComparison.Ordinal) ||
            string.Equals(candidate.Id, vmName, StringComparison.Ordinal));
        if (vm is null ||
            !DesktopNodeHyperVVmTemplateLockGuard.TryReject(operation, vm.TemplateLock, vm.Name, out var error) ||
            error is null)
        {
            return false;
        }

        result = DesktopNodeHyperVOperationResult.Failure(
            operation,
            error.Code,
            error.Message,
            error.Detail,
            error.Retryable);
        return true;
    }

    private delegate bool HyperVAdapterDispatchInvoker(
        string operation,
        JsonElement parameters,
        CancellationToken cancellationToken,
        out DesktopNodeHyperVOperationResult result);

    private bool InvokeHostStatus(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeHostStatus(operation, cancellationToken, out result);
    }

    private bool InvokeNetworkInventory(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeNetworkInventory(operation, cancellationToken, out result);
    }

    private bool InvokeVmList(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return operation switch
        {
            "vm.memory-stats" or "vm.cpu-stats" => TryInvokeVmStats(operation, parameters, cancellationToken, out result),
            "vm.blkio-get" => TryInvokeVmInventoryReadback(operation, parameters, cancellationToken, out result),
            "vm.disk.inspect" => TryInvokeVmDiskInspect(operation, parameters, cancellationToken, out result),
            "vm.bandwidth" => TryInvokeVmInventoryReadback(operation, parameters, cancellationToken, out result),
            "vm.qos.storage.preview" => TryInvokeVmQosPreview(operation, parameters, cancellationToken, out result),
            "vm.qos.network.preview" => TryInvokeVmQosPreview(operation, parameters, cancellationToken, out result),
            "vm.guest-agent-status" => TryInvokeVmInventoryReadback(operation, parameters, cancellationToken, out result),
            "vm.guest-ping" => TryInvokeVmInventoryReadback(operation, parameters, cancellationToken, out result),
            _ => TryInvokeVmList(operation, cancellationToken, out result)
        };
    }

    private bool InvokeCheckpointList(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeCheckpointList(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeCheckpointMutation(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeCheckpointMutation(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmPowerState(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmPowerState(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmCreate(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmCreate(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmDelete(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmDelete(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmRename(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmRename(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmManage(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return operation == "vm.template.lock"
            ? TryInvokeVmTemplateLock(operation, parameters, cancellationToken, out result)
            : TryInvokeVmManage(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmClonePreview(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmClone(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmClone(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmClone(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmExport(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmExport(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmImport(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmImport(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmNetworkConnect(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmNetworkConnect(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmMedia(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmMedia(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeVmResourceMutation(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeVmResourceMutation(operation, parameters, cancellationToken, out result);
    }

    private bool InvokeGuestExecution(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        return TryInvokeGuestExecution(operation, parameters, cancellationToken, out result);
    }

    private static DesktopNodeHyperVProviderSet RequireProviderSet(DesktopNodeHyperVProviderSet providerSet)
    {
        ArgumentNullException.ThrowIfNull(providerSet);
        return providerSet;
    }
}
