using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.Contracts;

public sealed record RuntimePolicyResponse(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("data")] RuntimePolicyData Data,
    [property: JsonPropertyName("error")] object? Error);

public sealed record RuntimePolicyData(
    [property: JsonPropertyName("runtime_core")] RuntimeCorePolicy RuntimeCore,
    [property: JsonPropertyName("job_runtime")] JobRuntimePolicy JobRuntime,
    [property: JsonPropertyName("auth")] RuntimePolicyAuthPolicy Auth,
    [property: JsonPropertyName("network")] RuntimePolicyNetworkPolicy Network,
    [property: JsonPropertyName("console")] RuntimePolicyConsolePolicy Console,
    [property: JsonPropertyName("guest_execution")] RuntimePolicyGuestExecutionPolicy GuestExecution);

public sealed record RuntimeCorePolicy(
    [property: JsonPropertyName("contract_version")] int ContractVersion,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("api_routes")] RuntimeCoreApiRoutePolicy ApiRoutes,
    [property: JsonPropertyName("auth_session")] RuntimeCoreAuthSessionPolicy AuthSession,
    [property: JsonPropertyName("job_runtime")] RuntimeCoreJobRuntimePolicy JobRuntime,
    [property: JsonPropertyName("diagnostics")] RuntimeCoreDiagnosticsPolicy Diagnostics);

public sealed record RuntimeCoreApiRoutePolicy(
    [property: JsonPropertyName("auth_session")] IReadOnlyList<string> AuthSession,
    [property: JsonPropertyName("job_runtime")] IReadOnlyList<string> JobRuntime,
    [property: JsonPropertyName("diagnostics")] IReadOnlyList<string> Diagnostics);

public sealed record RuntimeCoreAuthSessionPolicy(
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("routes")] IReadOnlyList<string> Routes,
    [property: JsonPropertyName("token_storage")] string TokenStorage,
    [property: JsonPropertyName("session_storage")] string SessionStorage,
    [property: JsonPropertyName("boundary")] string Boundary);

public sealed record RuntimeCoreJobRuntimePolicy(
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("routes")] IReadOnlyList<string> Routes,
    [property: JsonPropertyName("state_store")] string StateStore,
    [property: JsonPropertyName("worker")] string Worker,
    [property: JsonPropertyName("mutation_boundary")] string MutationBoundary);

public sealed record RuntimeCoreDiagnosticsPolicy(
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("routes")] IReadOnlyList<string> Routes,
    [property: JsonPropertyName("bundle_root")] string BundleRoot,
    [property: JsonPropertyName("redaction")] string Redaction,
    [property: JsonPropertyName("retention")] string Retention);

public sealed record JobRuntimePolicy(
    [property: JsonPropertyName("contract_version")] int ContractVersion,
    [property: JsonPropertyName("owner")] string Owner,
    [property: JsonPropertyName("state_store")] JobRuntimeStateStorePolicy StateStore,
    [property: JsonPropertyName("dispatch")] JobRuntimeDispatchPolicy Dispatch,
    [property: JsonPropertyName("control")] JobRuntimeControlPolicy Control,
    [property: JsonPropertyName("host_mutation")] string HostMutation,
    [property: JsonPropertyName("orchestration")] JobRuntimeOrchestrationPolicy Orchestration,
    [property: JsonPropertyName("native_core")] JobRuntimeNativeCorePolicy NativeCore,
    [property: JsonPropertyName("managed_core")] JobRuntimeManagedCorePolicy ManagedCore);

public sealed record JobRuntimeStateStorePolicy(
    [property: JsonPropertyName("backend")] string Backend,
    [property: JsonPropertyName("persistence")] string Persistence,
    [property: JsonPropertyName("corrupt_store")] string CorruptStore,
    [property: JsonPropertyName("unsupported_future_version")] string UnsupportedFutureVersion);

public sealed record JobRuntimeDispatchPolicy(
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("helper_boundary")] string HelperBoundary,
    [property: JsonPropertyName("native_probe_operations")] IReadOnlyList<string> NativeProbeOperations,
    [property: JsonPropertyName("native_mutation_operations")] IReadOnlyList<string> NativeMutationOperations,
    [property: JsonPropertyName("mutation_dispatch")] string MutationDispatch);

public sealed record JobRuntimeControlPolicy(
    [property: JsonPropertyName("cancel")] JobRuntimeCancelPolicy Cancel,
    [property: JsonPropertyName("retry")] JobRuntimeRetryPolicy Retry);

public sealed record JobRuntimeCancelPolicy(
    [property: JsonPropertyName("queued_only")] bool QueuedOnly,
    [property: JsonPropertyName("running_interrupt")] bool RunningInterrupt,
    [property: JsonPropertyName("running_interrupt_operations")] IReadOnlyList<string> RunningInterruptOperations);

public sealed record JobRuntimeRetryPolicy(
    [property: JsonPropertyName("manual_only")] bool ManualOnly,
    [property: JsonPropertyName("failed_error_retryable_only")] bool FailedErrorRetryableOnly,
    [property: JsonPropertyName("max_attempts")] int MaxAttempts,
    [property: JsonPropertyName("creates_new_job")] bool CreatesNewJob);

public sealed record JobRuntimeOrchestrationPolicy(
    [property: JsonPropertyName("primary")] string Primary,
    [property: JsonPropertyName("contract")] string Contract);

public sealed record JobRuntimeNativeCorePolicy(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("revisit_when")] string RevisitWhen);

public sealed record JobRuntimeManagedCorePolicy(
    [property: JsonPropertyName("candidate")] string Candidate,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("host_replacement")] string HostReplacement);

public sealed record RuntimePolicyAuthPolicy(
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("multi_user")] bool MultiUser,
    [property: JsonPropertyName("rbac")] bool Rbac,
    [property: JsonPropertyName("token_storage")] string TokenStorage,
    [property: JsonPropertyName("roles")] IReadOnlyList<string>? Roles = null,
    [property: JsonPropertyName("grant_types")] IReadOnlyList<string>? GrantTypes = null,
    [property: JsonPropertyName("session_storage")] string? SessionStorage = null,
    [property: JsonPropertyName("access_token_ttl_seconds")] int? AccessTokenTtlSeconds = null,
    [property: JsonPropertyName("refresh_token_ttl_seconds")] int? RefreshTokenTtlSeconds = null,
    [property: JsonPropertyName("loopback_session_available")] bool LoopbackSessionAvailable = false);

public sealed record RuntimePolicyConsolePolicy(
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("windows_console")] string WindowsConsole,
    [property: JsonPropertyName("novnc")] string NoVnc,
    [property: JsonPropertyName("transport")] string Transport);

public sealed record RuntimePolicyStaticAssetAuthPolicy(
    [property: JsonPropertyName("loopback")] string Loopback,
    [property: JsonPropertyName("non_loopback")] string NonLoopback);

public sealed record RuntimePolicyNetworkPolicy(
    [property: JsonPropertyName("default_exposure")] string DefaultExposure,
    [property: JsonPropertyName("current_exposure")] string CurrentExposure,
    [property: JsonPropertyName("lan_mode")] string LanMode,
    [property: JsonPropertyName("static_asset_auth")] RuntimePolicyStaticAssetAuthPolicy StaticAssetAuth);

public sealed record RuntimePolicyGuestExecutionPolicy(
    [property: JsonPropertyName("enabled")] bool Enabled,
    [property: JsonPropertyName("preview_enabled")] bool PreviewEnabled,
    [property: JsonPropertyName("execute_enabled")] bool ExecuteEnabled,
    [property: JsonPropertyName("channel_preview_enabled")] bool ChannelPreviewEnabled,
    [property: JsonPropertyName("channel_verify_enabled")] bool ChannelVerifyEnabled,
    [property: JsonPropertyName("channel_repair_enabled")] bool ChannelRepairEnabled,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("adr")] string Adr,
    [property: JsonPropertyName("credential_policy")] string CredentialPolicy,
    [property: JsonPropertyName("credential_storage_policy")] IReadOnlyList<string> CredentialStoragePolicy,
    [property: JsonPropertyName("audit_schema")] string AuditSchema,
    [property: JsonPropertyName("redaction_policy")] string RedactionPolicy,
    [property: JsonPropertyName("timeout")] RuntimePolicyGuestExecutionTimeoutPolicy Timeout,
    [property: JsonPropertyName("required_capabilities")] IReadOnlyList<string> RequiredCapabilities,
    [property: JsonPropertyName("problem_codes")] IReadOnlyList<string> ProblemCodes,
    [property: JsonPropertyName("routes")] IReadOnlyList<string> Routes);

public sealed record RuntimePolicyGuestExecutionTimeoutPolicy(
    [property: JsonPropertyName("default_seconds")] int DefaultSeconds,
    [property: JsonPropertyName("max_seconds")] int MaxSeconds,
    [property: JsonPropertyName("cancel")] string Cancel);

public static class RuntimePolicyContract
{
    private static readonly IReadOnlyList<string> RunningInterruptOperationNames =
        Array.AsReadOnly(
        [
            "vm.guest.exec",
            "vm.guest.channel.verify"
        ]);

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public static IReadOnlyList<string> RunningInterruptOperations => RunningInterruptOperationNames;

    public static bool IsRunningInterruptOperation(string? operation)
    {
        return !string.IsNullOrWhiteSpace(operation) &&
            RunningInterruptOperationNames.Contains(operation, StringComparer.Ordinal);
    }

    public static RuntimePolicyResponse CreateDefault(
        string tokenStorage = "none",
        string currentExposure = "loopback",
        RuntimePolicyAuthPolicy? authPolicy = null,
        RuntimePolicyConsolePolicy? consolePolicy = null)
    {
        return new RuntimePolicyResponse(
            Ok: true,
            Operation: "runtime.policy",
            Data: new RuntimePolicyData(
                RuntimeCore: new RuntimeCorePolicy(
                    ContractVersion: 1,
                    Owner: "DesktopNode.Api",
                    ApiRoutes: new RuntimeCoreApiRoutePolicy(
                        AuthSession:
                        [
                            "/api/v1/auth/login",
                            "/api/v1/auth/refresh",
                            "/api/v1/auth/logout",
                            "/api/v1/auth/session",
                            "/api/v1/auth/rbac"
                        ],
                        JobRuntime:
                        [
                            "/api/v1/jobs",
                            "/api/v1/jobs/{jobId}",
                            "/api/v1/jobs/{jobId}/cancel",
                            "/api/v1/jobs/{jobId}/retry"
                        ],
                        Diagnostics:
                        [
                            "/api/v1/diagnostics/bundles",
                            "/api/v1/diagnostics/bundles/{bundleId}/download"
                        ]),
                    AuthSession: new RuntimeCoreAuthSessionPolicy(
                        Owner: "DesktopNode.Api.AccountAuth",
                        Mode: authPolicy?.Mode ?? "single_bearer_token",
                        Routes:
                        [
                            "/api/v1/auth/login",
                            "/api/v1/auth/refresh",
                            "/api/v1/auth/logout",
                            "/api/v1/auth/session",
                            "/api/v1/auth/rbac"
                        ],
                        TokenStorage: authPolicy?.TokenStorage ?? tokenStorage,
                        SessionStorage: authPolicy?.SessionStorage ?? "none",
                        Boundary: "loopback-default-token-required-nonloopback-static-assets-bearer-required"),
                    JobRuntime: new RuntimeCoreJobRuntimePolicy(
                        Owner: "DesktopNode.Runtime",
                        Routes:
                        [
                            "/api/v1/jobs",
                            "/api/v1/jobs/{jobId}",
                            "/api/v1/jobs/{jobId}/cancel",
                            "/api/v1/jobs/{jobId}/retry"
                        ],
                        StateStore: "json-file-snapshot",
                        Worker: "bounded-synchronous-worker-tick",
                        MutationBoundary: "native-adapter-queued-mutation-boundary"),
                    Diagnostics: new RuntimeCoreDiagnosticsPolicy(
                        Owner: "DesktopNode.Api.Diagnostics",
                        Routes:
                        [
                            "/api/v1/diagnostics/bundles",
                            "/api/v1/diagnostics/bundles/{bundleId}/download"
                        ],
                        BundleRoot: "configured-diagnostics-root",
                        Redaction: "diagnostic-bundle-redaction-required",
                        Retention: "configured-retention-with-pagination")),
                JobRuntime: new JobRuntimePolicy(
                    ContractVersion: 1,
                    Owner: "local-api",
                    StateStore: new JobRuntimeStateStorePolicy(
                        Backend: "script-scope-memory",
                        Persistence: "json-file-snapshot",
                        CorruptStore: "quarantine-and-start-empty",
                        UnsupportedFutureVersion: "blocked-diagnostics-no-mutation"),
                    Dispatch: new JobRuntimeDispatchPolicy(
                        Mode: "bounded-synchronous-worker-tick",
                        HelperBoundary: "dotnet-native-read-vm-qos-preview-guestservice-guest-execution-create-lifecycle-media-resource-delete-checkpoint-mutation",
                        NativeProbeOperations:
                        [
                            "host.status",
                            "network.inventory",
                            "vm.list",
                            "vm.memory-stats",
                            "vm.cpu-stats",
                            "vm.blkio-get",
                            "vm.bandwidth",
                            "vm.qos.storage.preview",
                            "vm.qos.network.preview",
                            "vm.guest-agent-status",
                            "vm.guest-ping",
                            "checkpoint.list"
                        ],
                        NativeMutationOperations:
                        [
                            "vm.create",
                            "vm.start",
                            "vm.shutdown",
                            "vm.poweroff",
                            "vm.restart",
                            "vm.pause",
                            "vm.resume",
                            "vm.save",
                            "vm.resume-saved",
                            "vm.rename",
                            "vm.manage",
                            "vm.clone.preview",
                            "vm.clone",
                            "vm.eject",
                            "vm.attach",
                            "vm.limit",
                            "vm.qos.storage.set",
                            "vm.qos.network.set",
                            "vm.guest.exec",
                            "vm.guest.channel.verify",
                            "vm.guest.channel.ensure",
                            "vm.set-memory",
                            "vm.set-vcpu",
                            "vm.disk-resize",
                            "vm.delete",
                            "checkpoint.create",
                            "checkpoint.restore",
                            "checkpoint.delete"
                        ],
                        MutationDispatch: "native-vm-qos-guest-execution-and-mutation-create-lifecycle-media-resource-delete-checkpoint-mutation"),
                    Control: new JobRuntimeControlPolicy(
                        Cancel: new JobRuntimeCancelPolicy(
                            QueuedOnly: false,
                            RunningInterrupt: RunningInterruptOperations.Count > 0,
                            RunningInterruptOperations: RunningInterruptOperations),
                        Retry: new JobRuntimeRetryPolicy(
                            ManualOnly: true,
                            FailedErrorRetryableOnly: true,
                            MaxAttempts: 3,
                            CreatesNewJob: true)),
                    HostMutation: "native-read-routes-vm-qos-preview-guestservice-guest-execution-create-lifecycle-media-resource-delete-checkpoint-and-qos-mutation",
                    Orchestration: new JobRuntimeOrchestrationPolicy(
                        Primary: "dotnet",
                        Contract: "dotnet-native-adapter-contract-tests-admin-smoke"),
                    NativeCore: new JobRuntimeNativeCorePolicy(
                        Status: "read-route-vm-qos-preview-guestservice-guest-execution-resource-checkpoint-and-qos-mutation-started",
                        Reason: "host.status,network.inventory,vm.list,vm.memory-stats,vm.cpu-stats,vm.blkio-get,vm.bandwidth,vm.qos.storage.preview,vm.qos.network.preview,vm.guest-agent-status,vm.guest-ping,checkpoint.list,vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.pause,vm.resume,vm.save,vm.resume-saved,vm.rename,vm.manage,vm.clone.preview,vm.clone,vm.eject,vm.attach,vm.limit,vm.qos.storage.set,vm.qos.network.set,vm.guest.exec,vm.guest.channel.verify,vm.guest.channel.ensure,vm.set-memory,vm.set-vcpu,vm.disk-resize,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete",
                        RevisitWhen: "next-read-route-or-mutation-adapter-parity"),
                    ManagedCore: new JobRuntimeManagedCorePolicy(
                        Candidate: "dotnet",
                        Status: "service-host-default",
                        HostReplacement: "dotnet-windows-service-host")),
                Auth: authPolicy ?? new RuntimePolicyAuthPolicy(
                    Mode: "single_bearer_token",
                    MultiUser: false,
                    Rbac: false,
                    TokenStorage: tokenStorage),
                Network: new RuntimePolicyNetworkPolicy(
                    DefaultExposure: "loopback",
                    CurrentExposure: currentExposure,
                    LanMode: "preview-admin-opt-in",
                    StaticAssetAuth: new RuntimePolicyStaticAssetAuthPolicy(
                        Loopback: "unauthenticated-static-only",
                        NonLoopback: "bearer-required")),
                Console: consolePolicy ?? new RuntimePolicyConsolePolicy(
                    Mode: "windows-hyperv-console-handoff",
                    WindowsConsole: "vmconnect",
                    NoVnc: "not_configured",
                    Transport: "local-handoff"),
                GuestExecution: new RuntimePolicyGuestExecutionPolicy(
                    Enabled: true,
                    PreviewEnabled: true,
                    ExecuteEnabled: true,
                    ChannelPreviewEnabled: true,
                    ChannelVerifyEnabled: true,
                    ChannelRepairEnabled: true,
                    Status: GuestExecutionSecurityContract.ProviderEnabledStatus,
                    Adr: GuestExecutionSecurityContract.AdrPath,
                    CredentialPolicy: "credential-ref-only",
                    CredentialStoragePolicy:
                    [
                        "windows-credential-manager",
                        "dpapi-local-machine"
                    ],
                    AuditSchema: GuestExecutionSecurityContract.AuditSchema,
                    RedactionPolicy: GuestExecutionSecurityContract.RedactionPolicy,
                    Timeout: new RuntimePolicyGuestExecutionTimeoutPolicy(
                        DefaultSeconds: 60,
                        MaxSeconds: 600,
                        Cancel: "queued-and-running-guest-execution-cancel-with-provider-token-interrupt"),
                    RequiredCapabilities: GuestExecutionSecurityContract.RequiredCapabilities,
                    ProblemCodes: GuestExecutionSecurityContract.ProblemCodes,
                    Routes:
                    [
                        "GET /api/v1/runtime/policy",
                        "POST /api/v1/vms/{vmId}/guest/exec/preview",
                        "POST /api/v1/vms/{vmId}/guest/exec",
                        "POST /api/v1/vms/{vmId}/guest/channel/preview",
                        "POST /api/v1/vms/{vmId}/guest/channel/verify",
                        "POST /api/v1/vms/{vmId}/guest/channel"
                    ])),
            Error: null);
    }
}
