using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using DesktopNode.Api;

namespace DesktopNode.Host;

public sealed partial class DesktopNodeHostApplication : IDisposable
{
    private bool TryMatchNoVncBridgePath(string path, out string vmId)
    {
        vmId = string.Empty;
        var template = string.IsNullOrWhiteSpace(options.NoVncWebSocketPath)
            ? "/api/v1/console/novnc/{vm_id}"
            : options.NoVncWebSocketPath;
        var pattern = "^" + Regex.Escape(template)
            .Replace("\\{vm_id}", "([^/]+)", StringComparison.OrdinalIgnoreCase) + "$";
        var match = Regex.Match(path, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return false;
        }

        vmId = Uri.UnescapeDataString(match.Groups[1].Value);
        return !string.IsNullOrWhiteSpace(vmId);
    }

    private async Task HandleNoVncBridgeAsync(
        HttpListenerContext context,
        string vmId,
        CancellationToken requestToken)
    {
        if (!options.NoVncBridgeEnabled ||
            string.IsNullOrWhiteSpace(options.NoVncTargetHost) ||
            !options.NoVncTargetPort.HasValue)
        {
            await WriteTextAsync(
                context.Response,
                404,
                "application/json",
                "{\"ok\":false,\"operation\":\"console.novnc.bridge\",\"error\":{\"code\":\"PCV_NOVNC_BRIDGE_NOT_CONFIGURED\",\"message\":\"noVNC bridge is not configured for this listener.\",\"retryable\":false}}",
                CorsHeaders(context.Request)).ConfigureAwait(false);
            return;
        }

        var auth = AuthorizeNoVncBridge(context.Request);
        if (auth is not null)
        {
            await WriteTextAsync(context.Response, auth.StatusCode, auth.ContentType, auth.Body, CorsHeaders(context.Request)).ConfigureAwait(false);
            return;
        }

        var admission = await TryAcquireRequestAdmissionAsync(context.Request, context.Response, requestToken).ConfigureAwait(false);
        if (!admission.Accepted)
        {
            return;
        }

        using var admissionLease = admission.Lease;

        using var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(options.NoVncTargetHost!, options.NoVncTargetPort.Value)
            .WaitAsync(TimeSpan.FromSeconds(10), requestToken)
            .ConfigureAwait(false);

        WebSocketContext webSocketContext;
        try
        {
            webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null).ConfigureAwait(false);
        }
        catch (WebSocketException)
        {
            return;
        }

        using var webSocket = webSocketContext.WebSocket;
        await using var targetStream = tcpClient.GetStream();
        var clientToTarget = CopyWebSocketToStreamAsync(webSocket, targetStream, requestToken);
        var targetToClient = CopyStreamToWebSocketAsync(targetStream, webSocket, requestToken);
        await Task.WhenAny(clientToTarget, targetToClient).ConfigureAwait(false);

        try
        {
            if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bridge closed", CancellationToken.None).ConfigureAwait(false);
            }
        }
        catch (WebSocketException)
        {
        }
    }

    private DesktopNodeApiResponse? AuthorizeNoVncBridge(HttpListenerRequest request)
    {
        var authorization = request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Json(401, "PCV_AUTH_REQUIRED", "Authorization bearer token is required.");
        }

        var providedToken = authorization["Bearer ".Length..].Trim();
        if (!string.IsNullOrWhiteSpace(token.Value) &&
            string.Equals(providedToken, token.Value, StringComparison.Ordinal))
        {
            return null;
        }

        if (accountAuthReady)
        {
            var validation = accountAuthService.ValidateAccessToken(authorization);
            if (validation.Ok &&
                validation.Principal is not null &&
                accountAuthService.HasPermission(validation.Principal, "console.view"))
            {
                return null;
            }
        }

        return Json(403, "PCV_AUTH_FORBIDDEN", "Authorization bearer token was rejected.");
    }

    private static async Task CopyWebSocketToStreamAsync(WebSocket webSocket, Stream target, CancellationToken token)
    {
        var buffer = new byte[8192];
        while (!token.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(buffer, token).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            await target.WriteAsync(buffer.AsMemory(0, result.Count), token).ConfigureAwait(false);
            await target.FlushAsync(token).ConfigureAwait(false);
        }
    }

    private static async Task CopyStreamToWebSocketAsync(Stream source, WebSocket webSocket, CancellationToken token)
    {
        var buffer = new byte[8192];
        while (!token.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            var read = await source.ReadAsync(buffer, token).ConfigureAwait(false);
            if (read <= 0)
            {
                break;
            }

            await webSocket.SendAsync(
                buffer.AsMemory(0, read),
                WebSocketMessageType.Binary,
                endOfMessage: true,
                cancellationToken: token).ConfigureAwait(false);
        }
    }

}
