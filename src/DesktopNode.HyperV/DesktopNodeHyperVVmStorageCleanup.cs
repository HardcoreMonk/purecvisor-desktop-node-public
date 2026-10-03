using System.Text.Json.Serialization;

namespace DesktopNode.HyperV;

public sealed record DesktopNodeHyperVVmStorageCleanupInfo(
    [property: JsonPropertyName("configuration_root")] string? ConfigurationRoot,
    [property: JsonPropertyName("removed_files")] IReadOnlyList<string> RemovedFiles,
    [property: JsonPropertyName("removed_directories")] IReadOnlyList<string> RemovedDirectories,
    [property: JsonPropertyName("retained")] IReadOnlyList<DesktopNodeHyperVRetainedStorage> Retained);

public sealed record DesktopNodeHyperVRetainedStorage(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("reason")] string Reason);

public interface IDesktopNodeHyperVVmStorageFileSystem
{
    bool DirectoryExists(string path);

    IEnumerable<string> EnumerateFiles(string directory);

    IEnumerable<string> EnumerateDirectories(string directory);

    void DeleteFile(string path);

    void DeleteEmptyDirectory(string path);
}

public sealed class DesktopNodeHyperVPhysicalVmStorageFileSystem : IDesktopNodeHyperVVmStorageFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IEnumerable<string> EnumerateFiles(string directory) => Directory.EnumerateFiles(directory);

    public IEnumerable<string> EnumerateDirectories(string directory) => Directory.EnumerateDirectories(directory);

    public void DeleteFile(string path) => File.Delete(path);

    public void DeleteEmptyDirectory(string path) => Directory.Delete(path, recursive: false);
}

// Managed delete (WMI DestroySystem) removes the VM definition but leaves its disks. vm.create gives every
// managed VM a dedicated ConfigurationDataRoot (<vm_root>\<name>), so after the VM is gone the disk images
// inside that root belong to nobody. This removes only those images, keeps anything a remaining VM still
// references or that sits outside the root, and never fails the delete that already happened.
public static class DesktopNodeHyperVVmStorageCleanup
{
    public const string OutsideConfigurationRoot = "outside-configuration-root";
    public const string ReferencedByRemainingVm = "referenced-by-remaining-vm";
    public const string NoConfigurationRoot = "no-configuration-root";
    public const string ConfigurationRootTooBroad = "configuration-root-too-broad";
    public const string ReferenceCheckFailed = "reference-check-failed";
    public const string DirectoryNotEmpty = "directory-not-empty";
    public const string DeleteFailedPrefix = "delete-failed: ";

    private static readonly string[] DiskImageExtensions = [".vhdx", ".vhd", ".avhdx", ".avhd"];

    public static DesktopNodeHyperVVmStorageCleanupInfo Run(
        string? configurationRoot,
        IReadOnlyCollection<string> attachedDisks,
        IReadOnlyCollection<string>? remainingVmDisks,
        IDesktopNodeHyperVVmStorageFileSystem fileSystem)
    {
        var removedFiles = new List<string>();
        var removedDirectories = new List<string>();
        var retained = new List<DesktopNodeHyperVRetainedStorage>();

        var root = NormalizeRoot(configurationRoot, out var rootProblem);
        if (root is null)
        {
            retained.AddRange(Distinct(attachedDisks).Select(disk => new DesktopNodeHyperVRetainedStorage(disk, rootProblem!)));
            return new DesktopNodeHyperVVmStorageCleanupInfo(configurationRoot, removedFiles, removedDirectories, retained);
        }

        if (remainingVmDisks is null)
        {
            retained.AddRange(Distinct(attachedDisks).Select(disk => new DesktopNodeHyperVRetainedStorage(disk, ReferenceCheckFailed)));
            return new DesktopNodeHyperVVmStorageCleanupInfo(root, removedFiles, removedDirectories, retained);
        }

        var referenced = new HashSet<string>(remainingVmDisks.Select(SafeFullPath).OfType<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var disk in Distinct(attachedDisks))
        {
            if (!IsInside(root, SafeFullPath(disk)))
            {
                retained.Add(new DesktopNodeHyperVRetainedStorage(disk, OutsideConfigurationRoot));
            }
        }

        if (!fileSystem.DirectoryExists(root))
        {
            return new DesktopNodeHyperVVmStorageCleanupInfo(root, removedFiles, removedDirectories, retained);
        }

        foreach (var file in EnumerateDiskImages(root, fileSystem))
        {
            if (referenced.Contains(file))
            {
                retained.Add(new DesktopNodeHyperVRetainedStorage(file, ReferencedByRemainingVm));
                continue;
            }

            try
            {
                fileSystem.DeleteFile(file);
                removedFiles.Add(file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                retained.Add(new DesktopNodeHyperVRetainedStorage(file, DeleteFailedPrefix + ex.Message));
            }
        }

        if (!RemoveEmptyDirectories(root, fileSystem, removedDirectories))
        {
            retained.Add(new DesktopNodeHyperVRetainedStorage(root, DirectoryNotEmpty));
        }

        return new DesktopNodeHyperVVmStorageCleanupInfo(root, removedFiles, removedDirectories, retained);
    }

    private static string? NormalizeRoot(string? configurationRoot, out string? problem)
    {
        problem = NoConfigurationRoot;
        if (string.IsNullOrWhiteSpace(configurationRoot) || !Path.IsPathFullyQualified(configurationRoot))
        {
            return null;
        }

        var root = SafeFullPath(configurationRoot)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (string.IsNullOrEmpty(root))
        {
            return null;
        }

        // A drive or share root, or its direct child, is never a per-VM directory.
        var driveRoot = Path.GetPathRoot(root)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? string.Empty;
        var relative = root.Length > driveRoot.Length ? root[driveRoot.Length..].Trim(Path.DirectorySeparatorChar) : string.Empty;
        if (relative.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length < 2)
        {
            problem = ConfigurationRootTooBroad;
            return null;
        }

        problem = null;
        return root;
    }

    private static IEnumerable<string> EnumerateDiskImages(string directory, IDesktopNodeHyperVVmStorageFileSystem fileSystem)
    {
        foreach (var file in fileSystem.EnumerateFiles(directory))
        {
            if (DiskImageExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            {
                yield return SafeFullPath(file) ?? file;
            }
        }

        foreach (var child in fileSystem.EnumerateDirectories(directory))
        {
            foreach (var file in EnumerateDiskImages(child, fileSystem))
            {
                yield return file;
            }
        }
    }

    private static bool RemoveEmptyDirectories(string directory, IDesktopNodeHyperVVmStorageFileSystem fileSystem, List<string> removed)
    {
        var empty = true;
        foreach (var child in fileSystem.EnumerateDirectories(directory).ToArray())
        {
            empty &= RemoveEmptyDirectories(child, fileSystem, removed);
        }

        if (!empty || fileSystem.EnumerateFiles(directory).Any())
        {
            return false;
        }

        try
        {
            fileSystem.DeleteEmptyDirectory(directory);
            removed.Add(directory);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool IsInside(string root, string? path) =>
        path is not null && path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> Distinct(IEnumerable<string> paths) =>
        paths.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase);

    private static string? SafeFullPath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }
}
