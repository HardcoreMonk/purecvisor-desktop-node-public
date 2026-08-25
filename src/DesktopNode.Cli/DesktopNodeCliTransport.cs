namespace DesktopNode.Cli;

public interface IDesktopNodeCliTransport
{
    Task<DesktopNodeCliTransportResponse> SendAsync(
        DesktopNodeCliRequest request,
        DesktopNodeCliOptions options,
        string? bearerToken,
        CancellationToken cancellationToken);
}

public sealed record DesktopNodeCliTransportResponse(
    int StatusCode,
    string ContentType,
    string Body,
    IReadOnlyDictionary<string, string>? Headers = null);
