namespace DesktopNode.Cli;

public sealed record DesktopNodeCliRequest(
    string Method,
    string Path,
    string? Body = null,
    string? OutputPath = null);
