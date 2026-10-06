using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed record DesktopNodeHyperVVmCreateMarker(
    [property: JsonPropertyName("schema")] string Schema,
    [property: JsonPropertyName("vm_name")] string VmName,
    [property: JsonPropertyName("vhd_file")] string VhdFile,
    [property: JsonPropertyName("directory_created")] bool DirectoryCreated,
    [property: JsonPropertyName("owner_process_id")] int OwnerProcessId,
    [property: JsonPropertyName("owner_process_start_utc")] string OwnerProcessStartUtc,
    [property: JsonPropertyName("created_utc")] string CreatedUtc);

public sealed record DesktopNodeHyperVRecoveredResidue(
    [property: JsonPropertyName("removed_files")] IReadOnlyList<string> RemovedFiles,
    [property: JsonPropertyName("removed_directory")] bool RemovedDirectory);

public sealed record DesktopNodeHyperVCreateResidueDecision(
    bool ResiduePresent,
    string? RejectReason,
    DesktopNodeHyperVRecoveredResidue? Recovered);

public interface IDesktopNodeHyperVCreateResidueFileSystem
{
    bool FileExists(string path);

    string ReadAllText(string path);

    void WriteAllText(string path, string text);

    void DeleteFile(string path);

    bool IsDirectoryEmpty(string path);

    void DeleteEmptyDirectory(string path);
}

public sealed class DesktopNodeHyperVPhysicalCreateResidueFileSystem : IDesktopNodeHyperVCreateResidueFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public void WriteAllText(string path, string text) => File.WriteAllText(path, text);

    public void DeleteFile(string path) => File.Delete(path);

    public bool IsDirectoryEmpty(string path) => Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any();

    public void DeleteEmptyDirectory(string path) => Directory.Delete(path, recursive: false);
}

public interface IDesktopNodeHyperVProcessProbe
{
    // true when the process with this id and start time is running, false when it is gone, null when undecidable.
    bool? IsRunning(int processId, DateTime startUtc);
}

public sealed class DesktopNodeHyperVSystemProcessProbe : IDesktopNodeHyperVProcessProbe
{
    public bool? IsRunning(int processId, DateTime startUtc)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return Math.Abs((process.StartTime.ToUniversalTime() - startUtc).TotalSeconds) < 1;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception exception) when (exception is InvalidOperationException or Win32Exception or NotSupportedException)
        {
            return null;
        }
    }
}

// A vm.create interrupted before DefineSystem leaves <vm_root>\<name>\disk0.vhdx and no VM (design
// pcv-interrupted-create-residue-v1). The next create of the same name removes that residue only when the create
// marker proves PureCVisor wrote it, its owner process is gone and nothing references the disk; otherwise it rejects
// with a reason and deletes nothing.
public static class DesktopNodeHyperVVmCreateResidue
{
    public const string MarkerFileName = ".pcv-create-pending.json";
    public const string MarkerSchema = "pcv-vm-create-pending/v1";
    public const string VhdFileName = "disk0.vhdx";
    public const string NoCreateMarker = "no-create-marker";
    public const string MarkerInvalid = "marker-invalid";
    public const string CreateInProgress = "create-in-progress";
    public const string ReferencedByVm = "referenced-by-vm";
    public const string ReferenceCheckFailed = "reference-check-failed";
    public const string DeleteFailed = "delete-failed";

    private static readonly JsonSerializerOptions MarkerJson = new() { WriteIndented = true };

    public static string MarkerPath(string vmDirectory) => Path.Combine(vmDirectory, MarkerFileName);

    public static string NewMarker(string vmName, bool directoryCreated, int ownerProcessId, DateTime ownerStartUtc, DateTime nowUtc) =>
        JsonSerializer.Serialize(
            new DesktopNodeHyperVVmCreateMarker(
                MarkerSchema,
                vmName,
                VhdFileName,
                directoryCreated,
                ownerProcessId,
                ownerStartUtc.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
                nowUtc.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)),
            MarkerJson) + "\n";

    public static DesktopNodeHyperVCreateResidueDecision TryRecover(
        string vmDirectory,
        string vmName,
        IReadOnlyCollection<string>? referencedDisks,
        IDesktopNodeHyperVCreateResidueFileSystem fileSystem,
        IDesktopNodeHyperVProcessProbe processProbe)
    {
        var vhdPath = Path.Combine(vmDirectory, VhdFileName);
        var markerPath = MarkerPath(vmDirectory);
        var vhdPresent = fileSystem.FileExists(vhdPath);
        var markerPresent = fileSystem.FileExists(markerPath);
        if (!vhdPresent && !markerPresent)
        {
            return new DesktopNodeHyperVCreateResidueDecision(false, null, null);
        }

        if (!markerPresent)
        {
            return Reject(NoCreateMarker);
        }

        DesktopNodeHyperVVmCreateMarker? marker;
        try
        {
            marker = JsonSerializer.Deserialize<DesktopNodeHyperVVmCreateMarker>(fileSystem.ReadAllText(markerPath));
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return Reject(MarkerInvalid);
        }

        if (marker is null ||
            marker.Schema != MarkerSchema ||
            !string.Equals(marker.VmName, vmName, StringComparison.OrdinalIgnoreCase) ||
            marker.VhdFile != VhdFileName ||
            !DateTime.TryParse(
                marker.OwnerProcessStartUtc,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var ownerStartUtc))
        {
            return Reject(MarkerInvalid);
        }

        if (processProbe.IsRunning(marker.OwnerProcessId, ownerStartUtc) != false)
        {
            return Reject(CreateInProgress);
        }

        if (referencedDisks is null)
        {
            return Reject(ReferenceCheckFailed);
        }

        var fullVhdPath = Path.GetFullPath(vhdPath);
        if (referencedDisks.Any(disk => string.Equals(SafeFullPath(disk), fullVhdPath, StringComparison.OrdinalIgnoreCase)))
        {
            return Reject(ReferencedByVm);
        }

        var removed = new List<string>();
        try
        {
            if (vhdPresent)
            {
                fileSystem.DeleteFile(vhdPath);
                removed.Add(vhdPath);
            }

            fileSystem.DeleteFile(markerPath);
            removed.Add(markerPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new DesktopNodeHyperVCreateResidueDecision(true, DeleteFailed, null);
        }

        var removedDirectory = false;
        if (marker.DirectoryCreated && fileSystem.IsDirectoryEmpty(vmDirectory))
        {
            try
            {
                fileSystem.DeleteEmptyDirectory(vmDirectory);
                removedDirectory = true;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // The empty folder is harmless; the next create reuses it.
            }
        }

        return new DesktopNodeHyperVCreateResidueDecision(true, null, new DesktopNodeHyperVRecoveredResidue(removed, removedDirectory));
    }

    private static DesktopNodeHyperVCreateResidueDecision Reject(string reason) => new(true, reason, null);

    private static string? SafeFullPath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }
}
