using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public interface IDesktopNodeHyperVNativeAdapter
{
    bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result);
}

public interface IDesktopNodeHyperVSwitchProvider
{
    IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVHostStatusProvider
{
    DesktopNodeHyperVHostStatusData GetStatus(CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmProvider
{
    IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVCheckpointProvider
{
    IReadOnlyList<DesktopNodeHyperVCheckpointInfo> GetCheckpoints(string vmName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVCheckpointMutationProvider
{
    DesktopNodeHyperVCheckpointMutationInfo Invoke(string operation, string vmName, string checkpointName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmPowerStateProvider
{
    DesktopNodeHyperVVmPowerStateInfo Invoke(string operation, string vmName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmCreateProvider
{
    DesktopNodeHyperVVmCreateInfo Invoke(DesktopNodeHyperVVmCreateRequest request, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmDeleteProvider
{
    DesktopNodeHyperVVmDeleteInfo Invoke(string vmName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmRenameProvider
{
    DesktopNodeHyperVVmRenameInfo Invoke(string vmName, string newName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmManageProvider
{
    DesktopNodeHyperVVmManageInfo Invoke(string vmName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmCloneProvider
{
    DesktopNodeHyperVVmClonePlan Preview(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken);

    DesktopNodeHyperVVmCloneInfo Invoke(
        DesktopNodeHyperVVmCloneRequest request,
        CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmMediaProvider
{
    DesktopNodeHyperVVmMediaInfo Invoke(
        DesktopNodeHyperVVmMediaRequest request,
        CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmResourceMutationProvider
{
    DesktopNodeHyperVVmResourceMutationInfo Invoke(DesktopNodeHyperVVmResourceMutationRequest request, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVGuestExecutionProvider
{
    DesktopNodeHyperVGuestExecutionInfo Invoke(DesktopNodeHyperVGuestExecutionRequest request, CancellationToken cancellationToken);
}

public sealed record DesktopNodeHyperVOperationResult(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("data")] JsonElement? Data,
    [property: JsonPropertyName("error")] DesktopNodeHyperVError? Error)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
    };

    public static DesktopNodeHyperVOperationResult FromJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var data = root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind != JsonValueKind.Null
            ? dataElement.Clone()
            : (JsonElement?)null;
        var error = root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind != JsonValueKind.Null
            ? errorElement.Deserialize<DesktopNodeHyperVError>(JsonOptions)
            : null;

        return new DesktopNodeHyperVOperationResult(
            Ok: root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean(),
            Operation: root.TryGetProperty("operation", out var operationElement) ? operationElement.GetString() ?? string.Empty : string.Empty,
            Data: data,
            Error: error);
    }

    public static DesktopNodeHyperVOperationResult Failure(
        string operation,
        string code,
        string message,
        string detail,
        bool retryable,
        string? recommendedAction = null)
    {
        return new DesktopNodeHyperVOperationResult(
            Ok: false,
            Operation: operation,
            Data: null,
            Error: new DesktopNodeHyperVError(code, message, detail, retryable, recommendedAction));
    }
}

public sealed record DesktopNodeHyperVError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("retryable")] bool Retryable,
    [property: JsonPropertyName("recommended_action")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RecommendedAction = null);

public sealed record DesktopNodeHyperVSwitchInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("is_default")] bool IsDefault,
    [property: JsonPropertyName("allow_management_os")] bool? AllowManagementOs,
    [property: JsonPropertyName("net_adapter_interface_description")] string? NetAdapterInterfaceDescription);

public sealed record DesktopNodeHyperVVmInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("platform")] string Platform,
    [property: JsonPropertyName("guest_family")] string GuestFamily,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("cpu")] DesktopNodeHyperVVmCpuInfo Cpu,
    [property: JsonPropertyName("memory")] DesktopNodeHyperVVmMemoryInfo Memory,
    [property: JsonPropertyName("generation")] int? Generation,
    [property: JsonPropertyName("storage")] IReadOnlyList<DesktopNodeHyperVVmDiskInfo> Storage,
    [property: JsonPropertyName("network")] IReadOnlyList<DesktopNodeHyperVVmNetworkInfo> Network,
    [property: JsonPropertyName("checkpoints")] DesktopNodeHyperVVmCheckpointInfo Checkpoints,
    [property: JsonPropertyName("console")] DesktopNodeHyperVVmConsoleInfo Console,
    [property: JsonPropertyName("managed_by_purecvisor")] bool ManagedByPurecvisor);

public sealed record DesktopNodeHyperVVmCpuInfo(
    [property: JsonPropertyName("count")] int? Count);

public sealed record DesktopNodeHyperVVmMemoryInfo(
    [property: JsonPropertyName("startup_mb")] int? StartupMb,
    [property: JsonPropertyName("assigned_mb")] int? AssignedMb,
    [property: JsonPropertyName("dynamic")] bool Dynamic);

public sealed record DesktopNodeHyperVVmDiskInfo(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("path")] string? Path,
    [property: JsonPropertyName("size_gb")] int? SizeGb,
    [property: JsonPropertyName("attached")] bool Attached);

public sealed record DesktopNodeHyperVVmNetworkInfo(
    [property: JsonPropertyName("switch")] string? Switch,
    [property: JsonPropertyName("mode")] string? Mode);

public sealed record DesktopNodeHyperVVmCheckpointInfo(
    [property: JsonPropertyName("count")] int? Count);

public sealed record DesktopNodeHyperVVmConsoleInfo(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("available_local")] bool AvailableLocal);

public sealed record DesktopNodeHyperVCheckpointInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vm_name")] string VmName,
    [property: JsonPropertyName("created_at")] string? CreatedAt,
    [property: JsonPropertyName("is_current")] bool? IsCurrent = null,
    [property: JsonIgnore] string? InstanceId = null);

public sealed record DesktopNodeHyperVCheckpointMutationInfo(
    [property: JsonPropertyName("vm_name")] string VmName,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string? Action);

public sealed record DesktopNodeHyperVVmPowerStateInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action);

public sealed record DesktopNodeHyperVVmCreateRequest(
    string Name,
    string IsoPath,
    int Cpu,
    int MemoryMb,
    int DiskGb,
    string VmRoot,
    int Generation);

public sealed record DesktopNodeHyperVVmCreateInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("vm_dir")] string VmDirectory,
    [property: JsonPropertyName("vhd_path")] string VhdPath,
    [property: JsonPropertyName("iso_path")] string IsoPath,
    [property: JsonPropertyName("switch")] string SwitchName,
    [property: JsonPropertyName("generation")] int Generation,
    [property: JsonPropertyName("steps")] IReadOnlyList<string> Steps);

public sealed record DesktopNodeHyperVVmDeleteInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action);

public sealed record DesktopNodeHyperVVmRenameInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("new_name")] string NewName,
    [property: JsonPropertyName("action")] string Action);

public sealed record DesktopNodeHyperVVmManageInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action);

public sealed record DesktopNodeHyperVVmCloneDiskSnapshot(
    string SourcePath,
    long FileLength,
    bool IndependentVhdx);

public sealed record DesktopNodeHyperVVmCloneSourceSnapshot(
    string Name,
    bool Managed,
    int Generation,
    string PowerState,
    int CheckpointCount,
    IReadOnlyList<DesktopNodeHyperVVmCloneDiskSnapshot> Disks,
    bool SecurityFeaturesPresent);

public sealed record DesktopNodeHyperVVmCloneRequest(
    string SourceName,
    string TargetName,
    string VmRoot);

public sealed record DesktopNodeHyperVVmCloneDiskPlan(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("target")] string Target);

public sealed record DesktopNodeHyperVVmClonePlan(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("generation")] int Generation,
    [property: JsonPropertyName("directory")] string Directory,
    [property: JsonPropertyName("disk_count")] int DiskCount,
    [property: JsonPropertyName("planned_copy_bytes")] long PlannedCopyBytes,
    [property: JsonPropertyName("disks")] IReadOnlyList<DesktopNodeHyperVVmCloneDiskPlan> Disks);

public sealed record DesktopNodeHyperVVmCloneInfo(
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("directory")] string Directory,
    [property: JsonPropertyName("disks")] IReadOnlyList<string> Disks);

public sealed record DesktopNodeHyperVVmMediaRequest(
    string Operation,
    string VmName,
    string? IsoPath = null);

public sealed record DesktopNodeHyperVVmMediaInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("iso_path")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? IsoPath = null);

public sealed record DesktopNodeHyperVVmResourceMutationRequest(
    string Operation,
    string Name,
    int? MemoryMb,
    int? Cpu,
    int? DiskGb,
    string? DiskPath,
    string? QosDisk = null,
    string? QosAdapter = null,
    int? MaximumIops = null,
    int? MinimumIops = null,
    int? MaximumKbps = null,
    int? MinimumKbps = null,
    string? RequestId = null);

public sealed record DesktopNodeHyperVVmResourceMutationInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("memory_mb")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? MemoryMb = null,
    [property: JsonPropertyName("cpu")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? Cpu = null,
    [property: JsonPropertyName("disk_gb")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? DiskGb = null,
    [property: JsonPropertyName("disk_path")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? DiskPath = null,
    [property: JsonPropertyName("evidence")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, object?>? Evidence = null);

public sealed record DesktopNodeHyperVGuestExecutionRequest(
    string Operation,
    string Name,
    string? CredentialRef,
    IReadOnlyList<string> Command,
    IReadOnlyDictionary<string, string> Environment,
    int TimeoutSeconds,
    string? RequestId = null,
    string? Actor = null,
    bool RepairConfirmed = false);

public sealed record DesktopNodeHyperVGuestExecutionInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("transport")] string Transport,
    [property: JsonPropertyName("terminal_state")] string TerminalState,
    [property: JsonPropertyName("host_mutation_performed")] bool HostMutationPerformed,
    [property: JsonPropertyName("guest_command_performed")] bool GuestCommandPerformed,
    [property: JsonPropertyName("timeout_sec")] int TimeoutSeconds,
    [property: JsonPropertyName("exit_code")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? ExitCode = null,
    [property: JsonPropertyName("stdout_byte_count")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? StdoutByteCount = null,
    [property: JsonPropertyName("stderr_byte_count")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? StderrByteCount = null,
    [property: JsonPropertyName("stdout_digest")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? StdoutDigest = null,
    [property: JsonPropertyName("stderr_digest")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? StderrDigest = null,
    [property: JsonPropertyName("audit")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, object?>? Audit = null,
    [property: JsonPropertyName("evidence")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, object?>? Evidence = null);

public sealed record DesktopNodeHyperVWmiVmSummary(
    string Id,
    string Name,
    object? EnabledState,
    object? ProcessorCount,
    object? StartupMemoryQuantity,
    string? StartupMemoryQuantityUnits,
    string? GenerationSubtype,
    int? CheckpointCount,
    string? Notes,
    IReadOnlyList<DesktopNodeHyperVWmiVmStorageSummary>? Storage = null,
    IReadOnlyList<DesktopNodeHyperVWmiVmNetworkSummary>? Network = null);

public sealed record DesktopNodeHyperVWmiVmStorageSummary(string? Path, bool Attached);

public sealed record DesktopNodeHyperVWmiVmNetworkSummary(string? SwitchName);

public sealed record DesktopNodeHyperVHostStatusData(
    [property: JsonPropertyName("supported")] bool Supported,
    [property: JsonPropertyName("reasons")] IReadOnlyList<string> Reasons,
    [property: JsonPropertyName("windows")] DesktopNodeHyperVHostWindowsInfo Windows,
    [property: JsonPropertyName("admin")] DesktopNodeHyperVHostAdminInfo Admin,
    [property: JsonPropertyName("hyperv")] DesktopNodeHyperVHostHyperVInfo HyperV);

public sealed record DesktopNodeHyperVHostWindowsInfo(
    [property: JsonPropertyName("caption")] string? Caption,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("edition")] string Edition);

public sealed record DesktopNodeHyperVHostAdminInfo(
    [property: JsonPropertyName("elevated")] bool Elevated);

public sealed record DesktopNodeHyperVHostHyperVInfo(
    [property: JsonPropertyName("feature_enabled")] bool FeatureEnabled,
    [property: JsonPropertyName("vmms_running")] bool VmmsRunning,
    [property: JsonPropertyName("default_switch_present")] bool DefaultSwitchPresent);
