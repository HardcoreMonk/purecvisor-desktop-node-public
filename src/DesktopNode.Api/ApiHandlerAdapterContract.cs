namespace DesktopNode.Api;

public enum MutationStance
{
    ReadOnly,
    ProductOperation,
    QueuedMutation
}

public sealed record ApiHandlerRouteContract(
    string RouteTemplate,
    string Method,
    string AuthPolicy,
    string? RequiredPermission,
    MutationStance MutationStance,
    string OperationId,
    string OperationName,
    string FeatureId,
    string DefaultOwner,
    string RouteFamily);

public sealed record ApiRouteFamilyContract(
    string RouteFamily,
    string DefaultOwner,
    string EvidenceBridge,
    IReadOnlyList<string> Routes);

public sealed record ApiRuntimeEvidenceFamilyContract(
    string RouteFamily,
    string DefaultOwner,
    string EvidenceBridge,
    IReadOnlyList<string> ReadRoutes,
    IReadOnlyList<string> ProductOperationRoutes,
    IReadOnlyList<string> RequiredPermissions);

public sealed record ApiRuntimeEvidenceContract(
    string ContractKey,
    string RegistryBridgeContractKey,
    string HandlerRegistrySource,
    string DocumentationAnchor,
    IReadOnlyList<string> HandlerRegistryRouteKeys,
    IReadOnlyList<ApiRuntimeEvidenceFamilyContract> Families)
{
    public string FormatHandlerRegistryRouteKey(ApiHandlerRouteContract route)
    {
        return $"{route.Method} {route.RouteTemplate} -> {route.OperationName} [{route.EvidenceBridge()}]";
    }
}

public sealed record ApiHandlerAdapterContract(
    string AdapterStance,
    string DefaultOwner,
    IReadOnlyList<ApiHandlerRouteContract> Routes)
{
    private const string DotNetRuntimeOwner = "dotnet-runtime";
    private const string DotNetNativeAdapterOwner = "dotnet-native-adapter";
    private const string RuntimeEvidenceContractKey = "runtime-api-diagnostics-ops-summary-contract-v1";
    private const string RuntimeRegistryBridgeContractKey = "runtime-api-diagnostics-ops-summary-registry-bridge-v2";
    private const string RuntimeEvidenceDocumentationAnchor = "docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md#runtime-api-diagnostics-ops-summary";

    public string RuntimeApiDiagnosticsBridge => "route-family-evidence-linked";

    public IReadOnlyList<ApiRouteFamilyContract> RouteFamilies => BuildRouteFamilies(Routes);

    public ApiRuntimeEvidenceContract RuntimeEvidenceContract => BuildRuntimeEvidenceContract(Routes);

    public static ApiHandlerAdapterContract CreateDefault()
    {
        return new ApiHandlerAdapterContract(
            AdapterStance: "product-default-dotnet-runtime",
            DefaultOwner: DotNetRuntimeOwner,
            Routes:
            [
                RuntimeReadOnly("/api/v1/runtime/policy", "runtime.policy", "RuntimePolicy", "pcv.runtime.policy", "runtime-policy", requiredPermission: null),
                NativeReadOnly("/api/v1/host/status", "host.status", "HostStatus", "pcv.host.status", "hyperv-host"),
                NativeReadOnly("/api/v1/vms", "vm.list", "ListVms", "pcv.vm.inventory", "hyperv-vm"),
                RuntimeReadOnly("/api/v1/jobs", "job.list", "ListJobs", "pcv.job.lifecycle", "jobs"),
                RuntimeReadOnly("/api/v1/jobs/{jobId}", "job.detail", "GetJob", "pcv.job.lifecycle", "jobs"),
                RuntimeProductOperation("/api/v1/jobs/{jobId}/cancel", "job.cancel", "CancelJob", "pcv.job.lifecycle", "jobs"),
                RuntimeProductOperation("/api/v1/jobs/{jobId}/retry", "job.retry", "RetryJob", "pcv.job.lifecycle", "jobs"),
                RuntimeProductOperation("/api/v1/jobs/{jobId}/reconcile", "job.reconcile", "ReconcileJob", "pcv.job.lifecycle", "jobs"),
                RuntimeReadOnly("/api/v1/ops/summary", "ops.summary", "OpsSummary", "pcv.ops.summary", "ops-summary"),
                RuntimeReadOnly("/api/v1/diagnostics/bundles", "diagnostic.bundle.list", "ListDiagnosticBundles", "pcv.diagnostics.bundle", "diagnostics", "diagnostics.read"),
                RuntimeProductOperation("/api/v1/diagnostics/bundles", "diagnostic.bundle.create", "CreateDiagnosticBundle", "pcv.diagnostics.bundle", "diagnostics", requiredPermission: "diagnostics.create"),
                RuntimeReadOnly("/api/v1/diagnostics/bundles/{bundleId}/download", "diagnostic.bundle.download", "DownloadDiagnosticBundle", "pcv.diagnostics.bundle", "diagnostics", "diagnostics.read"),
                RuntimeProductOperation("/api/v1/auth/login", "auth.login", "LoginAccount", "pcv.account.session", "auth", "NoBearerTokenRequired", requiredPermission: null),
                RuntimeProductOperation("/api/v1/auth/loopback-session", "auth.loopback-session", "CreateLoopbackSession", "pcv.account.session", "auth", "NoBearerTokenRequired", requiredPermission: null),
                RuntimeProductOperation("/api/v1/auth/refresh", "auth.refresh", "RefreshAccount", "pcv.account.session", "auth", "NoBearerTokenRequired", requiredPermission: null),
                RuntimeProductOperation("/api/v1/auth/logout", "auth.logout", "LogoutAccount", "pcv.account.session", "auth", "NoBearerTokenRequired", requiredPermission: null),
                RuntimeReadOnly("/api/v1/auth/session", "auth.session", "GetAccountSession", "pcv.account.session", "auth"),
                RuntimeReadOnly("/api/v1/auth/rbac", "auth.rbac", "GetAccountRbac", "pcv.account.session", "auth"),
                RuntimeReadOnly("/api/v1/console/capabilities", "console.capabilities", "GetConsoleCapabilities", "pcv.console.capabilities", "console"),
                NativeReadOnly("/api/v1/network/inventory", "network.inventory", "NetworkInventory", "pcv.network.inventory", "hyperv-network"),
                RuntimeReadOnly("/api/v1/vms/{vmId}/delete-status", "vm.delete-status", "GetVmDeleteStatus", "pcv.vm.delete", "jobs"),
                NativeReadOnly("/api/v1/vms/{vmId}", "vm.detail", "GetVm", "pcv.vm.inventory", "hyperv-vm"),
                RuntimeReadOnly("/api/v1/vms/{vmId}/console", "console.session", "GetVmConsoleSession", "pcv.vm.console-handoff", "console", "console.view"),
                NativeReadOnly("/api/v1/vms/{vmId}/memory-stats", "vm.memory-stats", "GetVmMemoryStats", "pcv.vm.telemetry", "hyperv-vm"),
                NativeReadOnly("/api/v1/vms/{vmId}/cpu-stats", "vm.cpu-stats", "GetVmCpuStats", "pcv.vm.telemetry", "hyperv-vm"),
                NativeReadOnly("/api/v1/vms/{vmId}/blkio", "vm.blkio-get", "GetVmBlockIoPolicy", "pcv.vm.qos", "hyperv-vm-qos"),
                NativeReadOnly("/api/v1/vms/{vmId}/bandwidth", "vm.bandwidth", "GetVmBandwidthPolicy", "pcv.vm.qos", "hyperv-vm-qos"),
                NativeReadOnly("/api/v1/vms/{vmId}/guest-agent/status", "vm.guest-agent-status", "GetVmGuestAgentStatus", "pcv.vm.guest-service-readback", "hyperv-guest-service"),
                NativeReadOnly("/api/v1/vms/{vmId}/guest-agent/ping", "vm.guest-ping", "PingVmGuestAgent", "pcv.vm.guest-service-readback", "hyperv-guest-service"),
                RuntimeProductOperation("/api/v1/vms/{vmId}/guest/exec/preview", "vm.guest.exec.preview", "PreviewVmGuestExec", "pcv.vm.guest-execution", "guest-execution", requiredPermission: "guest.exec"),
                RuntimeProductOperation("/api/v1/vms/{vmId}/guest/channel/preview", "vm.guest.channel.preview", "PreviewVmGuestChannel", "pcv.vm.guest-channel", "guest-execution", requiredPermission: "guest.channel.configure"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/guest/exec", "vm.guest.exec", "QueueVmGuestExec", "pcv.vm.guest-execution", "guest-execution", requiredPermission: "guest.exec"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/guest/channel/verify", "vm.guest.channel.verify", "QueueVerifyVmGuestChannel", "pcv.vm.guest-channel", "guest-execution", requiredPermission: "guest.channel.configure"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/guest/channel", "vm.guest.channel.ensure", "QueueEnsureVmGuestChannel", "pcv.vm.guest-channel", "guest-execution", requiredPermission: "guest.channel.configure"),
                NativeReadOnly("/api/v1/vms/{vmId}/checkpoints", "checkpoint.list", "ListVmCheckpoints", "pcv.checkpoint.lifecycle", "hyperv-checkpoint"),
                NativeProductOperation("/api/v1/vms/{vmId}/qos/storage/preview", "vm.qos.storage.preview", "PreviewVmStorageQos", "pcv.vm.qos", "hyperv-vm-qos"),
                NativeProductOperation("/api/v1/vms/{vmId}/qos/network/preview", "vm.qos.network.preview", "PreviewVmNetworkQos", "pcv.vm.qos", "hyperv-vm-qos"),
                NativeQueuedMutation("/api/v1/vms", "vm.create", "QueueCreateVm", "pcv.vm.create", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/checkpoints", "checkpoint.create", "QueueCreateVmCheckpoint", "pcv.checkpoint.lifecycle", "hyperv-checkpoint"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/checkpoints/{checkpointId}/restore", "checkpoint.restore", "QueueRestoreVmCheckpoint", "pcv.checkpoint.restore", "hyperv-checkpoint"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/checkpoints/{checkpointId}", "checkpoint.delete", "QueueDeleteVmCheckpoint", "pcv.checkpoint.lifecycle", "hyperv-checkpoint", "DELETE"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/start", "vm.start", "QueueStartVm", "pcv.vm.power-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/shutdown", "vm.shutdown", "QueueShutdownVm", "pcv.vm.power-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/poweroff", "vm.poweroff", "QueuePowerOffVm", "pcv.vm.power-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/restart", "vm.restart", "QueueRestartVm", "pcv.vm.power-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/pause", "vm.pause", "QueuePauseVm", "pcv.vm.pause-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/resume", "vm.resume", "QueueResumeVm", "pcv.vm.pause-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/save", "vm.save", "QueueSaveVm", "pcv.vm.saved-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/resume-saved", "vm.resume-saved", "QueueResumeSavedVm", "pcv.vm.saved-lifecycle", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/rename", "vm.rename", "QueueRenameVm", "pcv.vm.rename", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/manage", "vm.manage", "QueueManageVm", "pcv.vm.managed-import", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/eject", "vm.eject", "QueueEjectVmMedia", "pcv.vm.media-eject", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/attach", "vm.attach", "QueueAttachVmMedia", "pcv.vm.media-attach", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/limit", "vm.limit", "QueueSetVmLimit", "pcv.vm.resource-limits", "hyperv-vm-qos"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/qos/storage", "vm.qos.storage.set", "QueueSetVmStorageQos", "pcv.vm.qos", "hyperv-vm-qos"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/qos/network", "vm.qos.network.set", "QueueSetVmNetworkQos", "pcv.vm.qos", "hyperv-vm-qos"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/set-memory", "vm.set-memory", "QueueSetVmMemory", "pcv.vm.resource-limits", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/set-vcpu", "vm.set-vcpu", "QueueSetVmVcpu", "pcv.vm.resource-limits", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}/disk-resize", "vm.disk-resize", "QueueResizeVmDisk", "pcv.vm.resource-limits", "hyperv-vm"),
                NativeQueuedMutation("/api/v1/vms/{vmId}", "vm.delete", "QueueDeleteVm", "pcv.vm.delete", "hyperv-vm", "DELETE")
            ]);
    }

    private static ApiHandlerRouteContract RuntimeReadOnly(
        string routeTemplate,
        string operationId,
        string operationName,
        string featureId,
        string routeFamily,
        string? requiredPermission = "read")
    {
        return new ApiHandlerRouteContract(
            RouteTemplate: routeTemplate,
            Method: "GET",
            AuthPolicy: "ReadTokenRequired",
            RequiredPermission: requiredPermission,
            MutationStance: MutationStance.ReadOnly,
            OperationId: operationId,
            OperationName: operationName,
            FeatureId: featureId,
            DefaultOwner: DotNetRuntimeOwner,
            RouteFamily: routeFamily);
    }

    private static ApiHandlerRouteContract NativeReadOnly(
        string routeTemplate,
        string operationId,
        string operationName,
        string featureId,
        string routeFamily)
    {
        return new ApiHandlerRouteContract(
            RouteTemplate: routeTemplate,
            Method: "GET",
            AuthPolicy: "ReadTokenRequired",
            RequiredPermission: "read",
            MutationStance: MutationStance.ReadOnly,
            OperationId: operationId,
            OperationName: operationName,
            FeatureId: featureId,
            DefaultOwner: DotNetNativeAdapterOwner,
            RouteFamily: routeFamily);
    }

    private static ApiHandlerRouteContract RuntimeProductOperation(
        string routeTemplate,
        string operationId,
        string operationName,
        string featureId,
        string routeFamily,
        string authPolicy = "TokenRequired",
        string? requiredPermission = "operate")
    {
        return new ApiHandlerRouteContract(
            RouteTemplate: routeTemplate,
            Method: "POST",
            AuthPolicy: authPolicy,
            RequiredPermission: requiredPermission,
            MutationStance: MutationStance.ProductOperation,
            OperationId: operationId,
            OperationName: operationName,
            FeatureId: featureId,
            DefaultOwner: DotNetRuntimeOwner,
            RouteFamily: routeFamily);
    }

    private static ApiHandlerRouteContract NativeProductOperation(
        string routeTemplate,
        string operationId,
        string operationName,
        string featureId,
        string routeFamily,
        string requiredPermission = "operate")
    {
        return new ApiHandlerRouteContract(
            RouteTemplate: routeTemplate,
            Method: "POST",
            AuthPolicy: "TokenRequired",
            RequiredPermission: requiredPermission,
            MutationStance: MutationStance.ProductOperation,
            OperationId: operationId,
            OperationName: operationName,
            FeatureId: featureId,
            DefaultOwner: DotNetNativeAdapterOwner,
            RouteFamily: routeFamily);
    }

    private static ApiHandlerRouteContract NativeQueuedMutation(
        string routeTemplate,
        string operationId,
        string operationName,
        string featureId,
        string routeFamily,
        string method = "POST",
        string requiredPermission = "operate")
    {
        return new ApiHandlerRouteContract(
            RouteTemplate: routeTemplate,
            Method: method,
            AuthPolicy: "TokenRequired",
            RequiredPermission: requiredPermission,
            MutationStance: MutationStance.QueuedMutation,
            OperationId: operationId,
            OperationName: operationName,
            FeatureId: featureId,
            DefaultOwner: DotNetNativeAdapterOwner,
            RouteFamily: routeFamily);
    }

    private static IReadOnlyList<ApiRouteFamilyContract> BuildRouteFamilies(IReadOnlyList<ApiHandlerRouteContract> routes)
    {
        return routes
            .GroupBy(route => route.RouteFamily, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ApiRouteFamilyContract(
                RouteFamily: group.Key,
                DefaultOwner: ResolveFamilyOwner(group),
                EvidenceBridge: EvidenceBridgeForRouteFamily(group.Key),
                Routes: group
                    .Select(route => $"{route.Method} {route.RouteTemplate}")
                    .OrderBy(route => route, StringComparer.Ordinal)
                    .ToArray()))
            .ToArray();
    }

    private static ApiRuntimeEvidenceContract BuildRuntimeEvidenceContract(IReadOnlyList<ApiHandlerRouteContract> routes)
    {
        var families = routes
            .Where(route => route.RouteFamily is "diagnostics" or "ops-summary")
            .GroupBy(route => route.RouteFamily, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ApiRuntimeEvidenceFamilyContract(
                RouteFamily: group.Key,
                DefaultOwner: ResolveFamilyOwner(group),
                EvidenceBridge: EvidenceBridgeForRouteFamily(group.Key),
                ReadRoutes: FormatRoutes(group, MutationStance.ReadOnly),
                ProductOperationRoutes: FormatRoutes(group, MutationStance.ProductOperation),
                RequiredPermissions: group
                    .Select(route => route.RequiredPermission)
                    .Where(permission => !string.IsNullOrWhiteSpace(permission))
                    .Cast<string>()
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(permission => permission, StringComparer.Ordinal)
                    .ToArray()))
            .ToArray();

        var registryRouteKeys = routes
            .Where(route => route.RouteFamily is "diagnostics" or "ops-summary")
            .Select(route => FormatHandlerRegistryRouteKey(route))
            .OrderBy(route => route, StringComparer.Ordinal)
            .ToArray();

        return new ApiRuntimeEvidenceContract(
            RuntimeEvidenceContractKey,
            RegistryBridgeContractKey: RuntimeRegistryBridgeContractKey,
            HandlerRegistrySource: "DesktopNodeApiRuntimeRoutes",
            DocumentationAnchor: RuntimeEvidenceDocumentationAnchor,
            HandlerRegistryRouteKeys: registryRouteKeys,
            Families: families);
    }

    private static IReadOnlyList<string> FormatRoutes(IEnumerable<ApiHandlerRouteContract> routes, MutationStance mutationStance)
    {
        return routes
            .Where(route => route.MutationStance == mutationStance)
            .Select(route => $"{route.Method} {route.RouteTemplate}")
            .OrderBy(route => route, StringComparer.Ordinal)
            .ToArray();
    }

    private static string ResolveFamilyOwner(IEnumerable<ApiHandlerRouteContract> routes)
    {
        var owners = routes
            .Select(route => route.DefaultOwner)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return owners.Length == 1 ? owners[0] : "mixed";
    }

    private static string EvidenceBridgeForRouteFamily(string routeFamily)
    {
        return routeFamily switch
        {
            "jobs" => "runtime-api-job-runtime-contract",
            "ops-summary" => "runtime-api-ops-summary-current-card",
            "diagnostics" => "runtime-api-diagnostics-bundle-contract",
            "guest-execution" => "guest-execution-security-boundary-contract",
            _ => "route-family-contract"
        };
    }

    private static string FormatHandlerRegistryRouteKey(ApiHandlerRouteContract route)
    {
        return $"{route.Method} {route.RouteTemplate} -> {route.OperationName} [{route.EvidenceBridge()}]";
    }
}

public static class ApiHandlerRouteContractExtensions
{
    public static string EvidenceBridge(this ApiHandlerRouteContract route)
    {
        return route.RouteFamily switch
        {
            "jobs" => "runtime-api-job-runtime-contract",
            "ops-summary" => "runtime-api-ops-summary-current-card",
            "diagnostics" => "runtime-api-diagnostics-bundle-contract",
            "guest-execution" => "guest-execution-security-boundary-contract",
            _ => "route-family-contract"
        };
    }
}
