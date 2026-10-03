using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVVmStorageCleanupTests
{
    private const string Root = @"D:\PureCVisor\VMs\lab";

    [Fact]
    public void RemovesUnreferencedDiskImagesInsideTheRootAndTheEmptyDirectories()
    {
        var fileSystem = new FakeFileSystem(
            [Root, Root + @"\Virtual Machines"],
            [Root + @"\disk0.vhdx", Root + @"\disk0_1A2B.avhdx"]);

        var result = DesktopNodeHyperVVmStorageCleanup.Run(Root, [Root + @"\disk0_1A2B.avhdx"], [], fileSystem);

        Assert.Equal([Root + @"\disk0.vhdx", Root + @"\disk0_1A2B.avhdx"], result.RemovedFiles.Order(StringComparer.OrdinalIgnoreCase));
        Assert.Equal([Root + @"\Virtual Machines", Root], result.RemovedDirectories);
        Assert.Empty(result.Retained);
        Assert.Empty(fileSystem.Files);
        Assert.Empty(fileSystem.Directories);
    }

    [Fact]
    public void KeepsDisksOutsideTheRootAndDisksARemainingVmStillUses()
    {
        var fileSystem = new FakeFileSystem([Root], [Root + @"\disk0.vhdx", Root + @"\shared.vhdx"]);

        var result = DesktopNodeHyperVVmStorageCleanup.Run(
            Root,
            [Root + @"\disk0.vhdx", @"E:\data\extra.vhdx"],
            [Root + @"\SHARED.vhdx"],
            fileSystem);

        Assert.Equal([Root + @"\disk0.vhdx"], result.RemovedFiles);
        Assert.Contains(result.Retained, item => item.Path == @"E:\data\extra.vhdx" && item.Reason == DesktopNodeHyperVVmStorageCleanup.OutsideConfigurationRoot);
        Assert.Contains(result.Retained, item => item.Path == Root + @"\shared.vhdx" && item.Reason == DesktopNodeHyperVVmStorageCleanup.ReferencedByRemainingVm);
        Assert.Contains(result.Retained, item => item.Path == Root && item.Reason == DesktopNodeHyperVVmStorageCleanup.DirectoryNotEmpty);
        Assert.Contains(Root + @"\shared.vhdx", fileSystem.Files);
    }

    [Fact]
    public void NeverDeletesNonDiskFilesAndKeepsTheirDirectory()
    {
        var fileSystem = new FakeFileSystem([Root], [Root + @"\disk0.vhdx", Root + @"\notes.txt"]);

        var result = DesktopNodeHyperVVmStorageCleanup.Run(Root, [Root + @"\disk0.vhdx"], [], fileSystem);

        Assert.Equal([Root + @"\disk0.vhdx"], result.RemovedFiles);
        Assert.Empty(result.RemovedDirectories);
        Assert.Contains(Root + @"\notes.txt", fileSystem.Files);
        Assert.Contains(result.Retained, item => item.Path == Root && item.Reason == DesktopNodeHyperVVmStorageCleanup.DirectoryNotEmpty);
    }

    [Theory]
    [InlineData(null, DesktopNodeHyperVVmStorageCleanup.NoConfigurationRoot)]
    [InlineData("relative\\lab", DesktopNodeHyperVVmStorageCleanup.NoConfigurationRoot)]
    [InlineData(@"D:\", DesktopNodeHyperVVmStorageCleanup.ConfigurationRootTooBroad)]
    [InlineData(@"D:\VMs", DesktopNodeHyperVVmStorageCleanup.ConfigurationRootTooBroad)]
    public void DeletesNothingWithoutADedicatedRoot(string? root, string reason)
    {
        var fileSystem = new FakeFileSystem([@"D:\VMs"], [@"D:\VMs\disk0.vhdx"]);

        var result = DesktopNodeHyperVVmStorageCleanup.Run(root, [@"D:\VMs\disk0.vhdx"], [], fileSystem);

        Assert.Empty(result.RemovedFiles);
        Assert.Empty(result.RemovedDirectories);
        Assert.Equal(reason, Assert.Single(result.Retained).Reason);
        Assert.Contains(@"D:\VMs\disk0.vhdx", fileSystem.Files);
    }

    [Fact]
    public void DeletesNothingWhenRemainingReferencesAreUnknown()
    {
        var fileSystem = new FakeFileSystem([Root], [Root + @"\disk0.vhdx"]);

        var result = DesktopNodeHyperVVmStorageCleanup.Run(Root, [Root + @"\disk0.vhdx"], remainingVmDisks: null, fileSystem);

        Assert.Empty(result.RemovedFiles);
        Assert.Equal(DesktopNodeHyperVVmStorageCleanup.ReferenceCheckFailed, Assert.Single(result.Retained).Reason);
        Assert.Contains(Root + @"\disk0.vhdx", fileSystem.Files);
    }

    [Fact]
    public void ReportsADiskThatCannotBeDeletedAndKeepsGoing()
    {
        var fileSystem = new FakeFileSystem([Root], [Root + @"\disk0.vhdx", Root + @"\disk1.vhdx"])
        {
            Locked = { Root + @"\disk0.vhdx" }
        };

        var result = DesktopNodeHyperVVmStorageCleanup.Run(Root, [Root + @"\disk0.vhdx"], [], fileSystem);

        Assert.Equal([Root + @"\disk1.vhdx"], result.RemovedFiles);
        var failed = Assert.Single(result.Retained, item => item.Path == Root + @"\disk0.vhdx");
        Assert.StartsWith(DesktopNodeHyperVVmStorageCleanup.DeleteFailedPrefix, failed.Reason, StringComparison.Ordinal);
    }

    private sealed class FakeFileSystem(IEnumerable<string> directories, IEnumerable<string> files) : IDesktopNodeHyperVVmStorageFileSystem
    {
        public HashSet<string> Directories { get; } = new(directories, StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Files { get; } = new(files, StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Locked { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool DirectoryExists(string path) => Directories.Contains(path);

        public IEnumerable<string> EnumerateFiles(string directory) =>
            Files.Where(file => string.Equals(Path.GetDirectoryName(file), directory, StringComparison.OrdinalIgnoreCase)).ToArray();

        public IEnumerable<string> EnumerateDirectories(string directory) =>
            Directories.Where(dir => string.Equals(Path.GetDirectoryName(dir), directory, StringComparison.OrdinalIgnoreCase)).ToArray();

        public void DeleteFile(string path)
        {
            if (Locked.Contains(path))
            {
                throw new IOException("The process cannot access the file because it is being used by another process.");
            }

            Files.Remove(path);
        }

        public void DeleteEmptyDirectory(string path)
        {
            if (Files.Any(file => file.StartsWith(path + "\\", StringComparison.OrdinalIgnoreCase)) ||
                Directories.Any(dir => dir.StartsWith(path + "\\", StringComparison.OrdinalIgnoreCase)))
            {
                throw new IOException("The directory is not empty.");
            }

            Directories.Remove(path);
        }
    }
}
