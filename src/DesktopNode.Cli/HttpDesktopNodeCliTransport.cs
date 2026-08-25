using System.Net.Http.Headers;
using System.Text;

namespace DesktopNode.Cli;

public sealed class HttpDesktopNodeCliTransport(HttpClient? client = null) : IDesktopNodeCliTransport, IDisposable
{
    private readonly HttpClient client = client ?? new HttpClient();
    private readonly bool ownsClient = client is null;

    public async Task<DesktopNodeCliTransportResponse> SendAsync(
        DesktopNodeCliRequest request,
        DesktopNodeCliOptions options,
        string? bearerToken,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(new HttpMethod(request.Method), BuildUri(options.ApiBaseUrl, request.Path));
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        if (!string.IsNullOrWhiteSpace(request.Body))
        {
            message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
        }

        using var response = await client.SendAsync(message, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var headers = response.Headers.Concat(response.Content.Headers)
            .ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase);

        return new DesktopNodeCliTransportResponse(
            (int)response.StatusCode,
            response.Content.Headers.ContentType?.MediaType ?? string.Empty,
            body,
            headers);
    }

    public void Dispose()
    {
        if (ownsClient)
        {
            client.Dispose();
        }
    }

    private static Uri BuildUri(string apiBaseUrl, string path)
    {
        if (!Uri.TryCreate(apiBaseUrl.TrimEnd('/') + path, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("PCV_CLI_API_INVALID|Invalid API base URL.");
        }

        return uri;
    }
}
