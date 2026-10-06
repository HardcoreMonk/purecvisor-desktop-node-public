using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.HyperV.Tests;

// Design pcv-interrupted-create-residue-v1: recovery of a create interrupted before DefineSystem, with fake file
// system and process probe because Required CI runs no Hyper-V.
public sealed class DesktopNodeHyperVVmCreateResidueTests
{
    private const string VmName = "pcv-probe-rc";
    private static readonly string Directory = Path.Combine(@"D:\PureCVisor\VMs", VmName);
    private static readonly string Vhd = Path.Combine(Directory, DesktopNodeHyperVVmCreateResidue.VhdFileName);
    private static readonly string Marker = DesktopNodeHyperVVmCreateResidue.MarkerPath(Directory);

    [Fact]
    public void NoResidueMeansNothingToRecover()
    {
        var files = new FakeFiles();

        var decision = Recover(files, running: false, references: []);

        Assert.False(decision.ResiduePresent);
        Assert.Null(decision.RejectReason);
        Assert.Empty(files.Deleted);
    }

    [Fact]
    public void DiskWithoutMarkerIsRefused()
    {
        var files = new FakeFiles(Vhd);

        var decision = Recover(files, running: false, references: []);

        Assert.Equal(DesktopNodeHyperVVmCreateResidue.NoCreateMarker, decision.RejectReason);
        Assert.Empty(files.Deleted);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{\"schema\":\"other\"}")]
    [InlineData("other-vm")]
    public void InvalidMarkerIsRefused(string marker)
    {
        var text = marker == "other-vm" ? MarkerText(vmName: "other-vm") : marker;
        var files = new FakeFiles(Vhd) { [Marker] = text };

        var decision = Recover(files, running: false, references: []);

        Assert.Equal(DesktopNodeHyperVVmCreateResidue.MarkerInvalid, decision.RejectReason);
        Assert.Empty(files.Deleted);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public void LiveOrUndecidableOwnerIsRefused(bool? running)
    {
        var files = new FakeFiles(Vhd) { [Marker] = MarkerText() };

        var decision = Recover(files, running, references: []);

        Assert.Equal(DesktopNodeHyperVVmCreateResidue.CreateInProgress, decision.RejectReason);
        Assert.Empty(files.Deleted);
    }

    [Fact]
    public void UnreadableReferencesAreRefused()
    {
        var files = new FakeFiles(Vhd) { [Marker] = MarkerText() };

        var decision = Recover(files, running: false, references: null);

        Assert.Equal(DesktopNodeHyperVVmCreateResidue.ReferenceCheckFailed, decision.RejectReason);
        Assert.Empty(files.Deleted);
    }

    [Fact]
    public void ReferencedDiskIsRefused()
    {
        var files = new FakeFiles(Vhd) { [Marker] = MarkerText() };

        var decision = Recover(files, running: false, references: [Vhd.ToUpperInvariant()]);

        Assert.Equal(DesktopNodeHyperVVmCreateResidue.ReferencedByVm, decision.RejectReason);
        Assert.Empty(files.Deleted);
    }

    [Fact]
    public void DeleteFailureIsRefused()
    {
        var files = new FakeFiles(Vhd) { [Marker] = MarkerText() };
        files.FailDelete = true;

        var decision = Recover(files, running: false, references: []);

        Assert.Equal(DesktopNodeHyperVVmCreateResidue.DeleteFailed, decision.RejectReason);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void MarkedResidueOfAGoneOwnerIsRemoved(bool directoryCreated, bool directoryRemoved)
    {
        var files = new FakeFiles(Vhd) { [Marker] = MarkerText(directoryCreated: directoryCreated) };

        var decision = Recover(files, running: false, references: [@"D:\PureCVisor\VMs\other\disk0.vhdx"]);

        Assert.Null(decision.RejectReason);
        Assert.Equal([Vhd, Marker], decision.Recovered!.RemovedFiles);
        Assert.Equal(directoryRemoved, decision.Recovered.RemovedDirectory);
        Assert.Equal(directoryRemoved, files.DirectoryDeleted);
    }

    [Fact]
    public void NewMarkerCarriesTheOwnerAndNoSecret()
    {
        var start = new DateTime(2026, 10, 7, 1, 2, 3, DateTimeKind.Utc);

        var marker = JsonNode.Parse(DesktopNodeHyperVVmCreateResidue.NewMarker(VmName, true, 4242, start, start.AddMinutes(5)))!.AsObject();

        Assert.Equal(DesktopNodeHyperVVmCreateResidue.MarkerSchema, marker["schema"]!.GetValue<string>());
        Assert.Equal(VmName, marker["vm_name"]!.GetValue<string>());
        Assert.Equal("disk0.vhdx", marker["vhd_file"]!.GetValue<string>());
        Assert.True(marker["directory_created"]!.GetValue<bool>());
        Assert.Equal(4242, marker["owner_process_id"]!.GetValue<int>());
        Assert.Equal(7, marker.Count);
    }

    [Fact]
    public void ManagedDeleteRemovesTheMarkerButKeepsOtherFiles()
    {
        var root = @"D:\PureCVisor\VMs\pcv-managed";
        var storage = new FakeStorage(
            Path.Combine(root, "disk0.vhdx"),
            Path.Combine(root, DesktopNodeHyperVVmCreateResidue.MarkerFileName),
            Path.Combine(root, "notes.txt"));

        var cleanup = DesktopNodeHyperVVmStorageCleanup.Run(root, [Path.Combine(root, "disk0.vhdx")], [], storage);

        Assert.Contains(Path.Combine(root, DesktopNodeHyperVVmCreateResidue.MarkerFileName), cleanup.RemovedFiles);
        Assert.Contains(Path.Combine(root, "disk0.vhdx"), cleanup.RemovedFiles);
        Assert.Contains(storage.Files, file => file.EndsWith("notes.txt", StringComparison.Ordinal));
        Assert.Contains(cleanup.Retained, retained => retained.Reason == DesktopNodeHyperVVmStorageCleanup.DirectoryNotEmpty);
    }

    private static DesktopNodeHyperVCreateResidueDecision Recover(FakeFiles files, bool? running, IReadOnlyCollection<string>? references) =>
        DesktopNodeHyperVVmCreateResidue.TryRecover(Directory, VmName, references, files, new FakeProbe(running));

    private static string MarkerText(string vmName = VmName, bool directoryCreated = true) =>
        DesktopNodeHyperVVmCreateResidue.NewMarker(
            vmName, directoryCreated, 4242, new DateTime(2026, 10, 6, 11, 0, 0, DateTimeKind.Utc), DateTime.UtcNow);

    private sealed class FakeProbe(bool? running) : IDesktopNodeHyperVProcessProbe
    {
        public bool? IsRunning(int processId, DateTime startUtc) => running;
    }

    private sealed class FakeFiles : Dictionary<string, string>, IDesktopNodeHyperVCreateResidueFileSystem
    {
        public FakeFiles(params string[] presentFiles) : base(StringComparer.OrdinalIgnoreCase)
        {
            foreach (var file in presentFiles)
            {
                this[file] = string.Empty;
            }
        }

        public bool FailDelete { get; set; }

        public List<string> Deleted { get; } = [];

        public bool DirectoryDeleted { get; private set; }

        public bool FileExists(string path) => ContainsKey(path);

        public string ReadAllText(string path) => this[path];

        public void WriteAllText(string path, string text) => this[path] = text;

        public void DeleteFile(string path)
        {
            if (FailDelete)
            {
                throw new IOException("in use");
            }

            Remove(path);
            Deleted.Add(path);
        }

        public bool IsDirectoryEmpty(string path) => Count == 0;

        public void DeleteEmptyDirectory(string path) => DirectoryDeleted = true;
    }

    private sealed class FakeStorage(params string[] files) : IDesktopNodeHyperVVmStorageFileSystem
    {
        public List<string> Files { get; } = [.. files];

        public bool DirectoryExists(string path) => true;

        public IEnumerable<string> EnumerateFiles(string directory) =>
            Files.Where(file => string.Equals(Path.GetDirectoryName(file), directory, StringComparison.OrdinalIgnoreCase)).ToArray();

        public IEnumerable<string> EnumerateDirectories(string directory) => [];

        public void DeleteFile(string path) => Files.Remove(path);

        public void DeleteEmptyDirectory(string path)
        {
        }
    }
}
