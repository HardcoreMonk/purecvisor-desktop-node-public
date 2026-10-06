using DesktopNode.HyperV;

namespace DesktopNode.HyperV.Tests;

public sealed class DesktopNodeHyperVManagedNotesTests
{
    [Fact]
    public void AppendManagedMarkerWritesMarkerOnlyWhenNotesAreEmpty()
    {
        Assert.Equal(DesktopNodeHyperVManagedNotes.Marker, DesktopNodeHyperVManagedNotes.AppendManagedMarker(null));
        Assert.Equal(DesktopNodeHyperVManagedNotes.Marker, DesktopNodeHyperVManagedNotes.AppendManagedMarker(string.Empty));
        Assert.Equal(DesktopNodeHyperVManagedNotes.Marker, DesktopNodeHyperVManagedNotes.AppendManagedMarker("  "));
        Assert.False(DesktopNodeHyperVManagedNotes.IsManagedNotes(null));
        Assert.True(DesktopNodeHyperVManagedNotes.IsManagedNotes(DesktopNodeHyperVManagedNotes.Marker));
    }

    [Fact]
    public void AppendManagedMarkerKeepsExistingNotesAndAddsOneMarkerLine()
    {
        var updated = DesktopNodeHyperVManagedNotes.AppendManagedMarker("lab imported from workstation");

        Assert.StartsWith("lab imported from workstation", updated, StringComparison.Ordinal);
        Assert.EndsWith(DesktopNodeHyperVManagedNotes.Marker, updated, StringComparison.Ordinal);
        Assert.Contains(Environment.NewLine, updated, StringComparison.Ordinal);
        Assert.Equal(1, CountMarkerOccurrences(updated));
        Assert.True(DesktopNodeHyperVManagedNotes.IsManagedNotes(updated));
    }

    [Fact]
    public void AppendManagedMarkerDoesNotAddASecondMarkerLineWhenAlreadyManaged()
    {
        var existing = "keep this text" + Environment.NewLine + DesktopNodeHyperVManagedNotes.Marker;

        var updated = DesktopNodeHyperVManagedNotes.AppendManagedMarker(existing);

        Assert.Equal(existing, updated);
        Assert.Equal(1, CountMarkerOccurrences(updated));
        Assert.True(DesktopNodeHyperVManagedNotes.IsManagedNotes(updated));
        Assert.True(DesktopNodeHyperVManagedNotes.IsManagedNotes("MANAGED-BY=purecvisor-desktop-node"));
    }

    [Fact]
    public void OperatorNotesStripsManagedMarkerAndKeepsOperatorText()
    {
        Assert.Null(DesktopNodeHyperVManagedNotes.OperatorNotes(null));
        Assert.Null(DesktopNodeHyperVManagedNotes.OperatorNotes(DesktopNodeHyperVManagedNotes.Marker));
        Assert.Equal(
            "lab imported from workstation",
            DesktopNodeHyperVManagedNotes.OperatorNotes(
                "lab imported from workstation" + Environment.NewLine + DesktopNodeHyperVManagedNotes.Marker));
        Assert.Equal("keep this", DesktopNodeHyperVManagedNotes.OperatorNotes("keep this"));
        Assert.Null(DesktopNodeHyperVManagedNotes.OperatorNotes(DesktopNodeHyperVManagedNotes.TemplateLockMarker));
        Assert.Equal(
            "lab gold image",
            DesktopNodeHyperVManagedNotes.OperatorNotes(
                "lab gold image" + Environment.NewLine +
                DesktopNodeHyperVManagedNotes.Marker + Environment.NewLine +
                DesktopNodeHyperVManagedNotes.TemplateLockMarker));
        Assert.True(DesktopNodeHyperVManagedNotes.IsTemplateLocked(
            DesktopNodeHyperVManagedNotes.Marker + Environment.NewLine +
            DesktopNodeHyperVManagedNotes.TemplateLockMarker));
        Assert.False(DesktopNodeHyperVManagedNotes.IsTemplateLocked(DesktopNodeHyperVManagedNotes.Marker));
        Assert.Equal(
            DesktopNodeHyperVManagedNotes.Marker + Environment.NewLine + DesktopNodeHyperVManagedNotes.TemplateLockMarker,
            DesktopNodeHyperVManagedNotes.ApplyTemplateLock(DesktopNodeHyperVManagedNotes.Marker, locked: true));
        Assert.Equal(
            DesktopNodeHyperVManagedNotes.Marker,
            DesktopNodeHyperVManagedNotes.ApplyTemplateLock(
                DesktopNodeHyperVManagedNotes.Marker + Environment.NewLine + DesktopNodeHyperVManagedNotes.TemplateLockMarker,
                locked: false));
    }

    [Fact]
    public void NotesPropertyIsOneElementThatKeepsEveryMarkerLine()
    {
        var locked = DesktopNodeHyperVManagedNotes.ToNotesProperty(DesktopNodeHyperVManagedNotes.ApplyTemplateLock(
            "lab gold image" + Environment.NewLine + DesktopNodeHyperVManagedNotes.Marker,
            locked: true));
        var lockedElement = Assert.Single(locked);
        Assert.Equal(
            "lab gold image\n" + DesktopNodeHyperVManagedNotes.Marker + "\n" + DesktopNodeHyperVManagedNotes.TemplateLockMarker,
            lockedElement);
        Assert.True(DesktopNodeHyperVManagedNotes.IsManagedNotes(lockedElement));
        Assert.True(DesktopNodeHyperVManagedNotes.IsTemplateLocked(lockedElement));

        var unlockedElement = Assert.Single(DesktopNodeHyperVManagedNotes.ToNotesProperty(
            DesktopNodeHyperVManagedNotes.ApplyTemplateLock(lockedElement, locked: false)));
        Assert.True(DesktopNodeHyperVManagedNotes.IsManagedNotes(unlockedElement));
        Assert.False(DesktopNodeHyperVManagedNotes.IsTemplateLocked(unlockedElement));

        var managedElement = Assert.Single(DesktopNodeHyperVManagedNotes.ToNotesProperty(
            DesktopNodeHyperVManagedNotes.AppendManagedMarker("operator note" + Environment.NewLine + "second line")));
        Assert.Equal("operator note\nsecond line\n" + DesktopNodeHyperVManagedNotes.Marker, managedElement);
        Assert.Single(DesktopNodeHyperVManagedNotes.ToNotesProperty(null));
    }

    [Fact]
    public void ManageProviderWritesNotesAsOneElement()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "src", "DesktopNode.sln")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        var source = File.ReadAllText(Path.Combine(root!.FullName, "src", "DesktopNode.HyperV", "DesktopNodeHyperVWmiVmManageProvider.cs"));
        Assert.DoesNotContain("StringSplitOptions", source, StringComparison.Ordinal);
        Assert.Equal(2, source.Split("DesktopNodeHyperVManagedNotes.ToNotesProperty(").Length - 1);
    }

    private static int CountMarkerOccurrences(string notes)
    {
        var count = 0;
        var start = 0;
        while (true)
        {
            var index = notes.IndexOf(DesktopNodeHyperVManagedNotes.Marker, start, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return count;
            }

            count += 1;
            start = index + DesktopNodeHyperVManagedNotes.Marker.Length;
        }
    }
}
