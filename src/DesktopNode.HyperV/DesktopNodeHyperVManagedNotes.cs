namespace DesktopNode.HyperV;

public static class DesktopNodeHyperVManagedNotes
{
    public const string Marker = "managed-by=purecvisor-desktop-node";

    public static bool IsManagedNotes(string? notes)
    {
        return !string.IsNullOrWhiteSpace(notes) &&
            notes.Contains(Marker, StringComparison.OrdinalIgnoreCase);
    }

    public static string AppendManagedMarker(string? notes)
    {
        if (IsManagedNotes(notes))
        {
            return notes!;
        }

        if (string.IsNullOrWhiteSpace(notes))
        {
            return Marker;
        }

        return notes + Environment.NewLine + Marker;
    }
}
