using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed record DesktopNodeHyperVVmExportRequest(
    string VmName,
    string Directory,
    string AllowedRoot);

public sealed record DesktopNodeHyperVVmExportInfo(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("directory")] string Directory,
    [property: JsonPropertyName("package_kind")] string PackageKind,
    [property: JsonPropertyName("vm_name")] string VmName);

public sealed record DesktopNodeHyperVVmImportRequest(
    string TargetName,
    string Directory,
    string AllowedRoot,
    bool GenerateNewId,
    string VmRoot = DesktopNodeHyperVVmImportRequest.DefaultVmRoot)
{
    public const string DefaultVmRoot = @"D:\PureCVisor\VMs";
}

public sealed record DesktopNodeHyperVVmImportDiskPlan(
    string PackageSource,
    string Target);

public sealed record DesktopNodeHyperVVmImportInfo(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("directory")] string Directory,
    [property: JsonPropertyName("package_kind")] string PackageKind,
    [property: JsonPropertyName("vm_name")] string VmName,
    [property: JsonPropertyName("generate_new_id")] bool GenerateNewId,
    [property: JsonPropertyName("apply_managed_marker")] bool ApplyManagedMarker,
    [property: JsonPropertyName("vm_directory")] string? VmDirectory = null);
