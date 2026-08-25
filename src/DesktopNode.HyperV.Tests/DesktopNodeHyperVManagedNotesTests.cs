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
