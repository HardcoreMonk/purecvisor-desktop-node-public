namespace DesktopNode.HyperV;

public static class DesktopNodeHyperVManagedNotes
{
    public const string Marker = "managed-by=purecvisor-desktop-node";
    public const string TemplateLockMarker = "template-lock=true";

    public static bool IsManagedNotes(string? notes)
    {
        return ContainsMarker(notes, Marker);
    }

    public static bool IsTemplateLocked(string? notes)
    {
        return ContainsMarker(notes, TemplateLockMarker);
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

    public static string? OperatorNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var lines = notes
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(static line =>
                !line.Contains(Marker, StringComparison.OrdinalIgnoreCase) &&
                !line.Contains(TemplateLockMarker, StringComparison.OrdinalIgnoreCase))
            .Select(static line => line.Trim())
            .Where(static line => line.Length > 0)
            .ToArray();
        return lines.Length == 0 ? null : string.Join('\n', lines);
    }

    public static string ApplyTemplateLock(string? notes, bool locked)
    {
        var stripped = StripMarkerLines(notes, TemplateLockMarker);
        if (!locked)
        {
            return stripped ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(stripped))
        {
            return TemplateLockMarker;
        }

        return stripped + Environment.NewLine + TemplateLockMarker;
    }

    private static string? StripMarkerLines(string? notes, string marker)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var lines = notes
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.Contains(marker, StringComparison.OrdinalIgnoreCase))
            .Select(static line => line.Trim())
            .Where(static line => line.Length > 0)
            .ToArray();
        return lines.Length == 0 ? null : string.Join(Environment.NewLine, lines);
    }

    private static bool ContainsMarker(string? notes, string marker)
    {
        return !string.IsNullOrWhiteSpace(notes) &&
            notes.Contains(marker, StringComparison.OrdinalIgnoreCase);
    }
}
