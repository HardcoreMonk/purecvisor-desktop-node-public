using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using DesktopNode.Api;

namespace DesktopNode.Host;

public sealed partial class DesktopNodeHostApplication : IDisposable
{
    private async Task WriteStaticFileAsync(HttpListenerResponse response, string requestPath)
    {
        if (requestPath.Split('?', 2)[0].Equals("/pcv-config.js", StringComparison.OrdinalIgnoreCase))
        {
            await WriteWebConfigScriptAsync(response).ConfigureAwait(false);
            return;
        }

        var root = Path.GetFullPath(options.WebRootPath!);
        var relativePath = requestPath.Split('?', 2)[0].TrimStart('/');
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            relativePath = "index.html";
        }

        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        var rootWithSeparator = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            await WriteTextAsync(response, 404, "application/json", "{\"ok\":false,\"error\":{\"code\":\"PCV_STATIC_FILE_NOT_FOUND\"}}").ConfigureAwait(false);
            return;
        }

        var bytes = await File.ReadAllBytesAsync(fullPath).ConfigureAwait(false);
        response.StatusCode = 200;
        response.ContentType = GetContentType(fullPath);
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
    }

    private async Task WriteWebConfigScriptAsync(HttpListenerResponse response)
    {
        var config = System.Text.Json.JsonSerializer.Serialize(new
        {
            apiBaseUrl = BaseUri.GetLeftPart(UriPartial.Authority)
        });
        await WriteTextAsync(
            response,
            200,
            "application/javascript",
            $"window.PCV_DESKTOP_NODE_CONFIG = Object.freeze({config});\n").ConfigureAwait(false);
    }

    private static async Task WriteTextAsync(
        HttpListenerResponse response,
        int statusCode,
        string contentType,
        string body,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(body);
        response.StatusCode = statusCode;
        response.ContentType = contentType;
        if (headers is not null)
        {
            foreach (var header in headers)
            {
                response.Headers[header.Key] = header.Value;
            }
        }

        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
    }

    private DesktopNodeApiResponse? Authorize(HttpListenerRequest request)
    {
        var path = request.Url?.AbsolutePath ?? "/";
        var remoteIsLoopback = IsLoopbackRemote(request.RemoteEndPoint);

        if (IsLoopbackSessionPath(path))
        {
            if (!remoteIsLoopback)
            {
                return Json(403, "PCV_LOOPBACK_SESSION_NOT_LOOPBACK", "Loopback session requires a loopback remote address.");
            }

            if (accountAuthReady)
            {
                return Json(409, "PCV_LOOPBACK_SESSION_DISABLED", "Loopback session is disabled because account auth is configured.");
            }

            return null;
        }

        if (IsUnauthenticatedAuthPath(path))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(token.Value))
        {
            return null;
        }

        var authorization = request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return accountAuthReady ? null : Json(401, "PCV_AUTH_REQUIRED", "Authorization bearer token is required.");
        }

        var providedToken = authorization["Bearer ".Length..].Trim();
        if (string.Equals(providedToken, token.Value, StringComparison.Ordinal))
        {
            return null;
        }

        var loopback = accountAuthService.ValidateLoopbackAccessToken(authorization);
        if (loopback.Ok)
        {
            if (accountAuthReady)
            {
                return Json(401, "PCV_LOOPBACK_SESSION_DISABLED", "Loopback session is disabled because account auth is configured.");
            }

            if (!remoteIsLoopback)
            {
                return Json(403, "PCV_LOOPBACK_SESSION_NOT_LOOPBACK", "Loopback session requires a loopback remote address.");
            }

            return null;
        }

        if (accountAuthReady)
        {
            return null;
        }

        return Json(403, "PCV_AUTH_FORBIDDEN", "Authorization bearer token was rejected.");
    }

    private static bool IsLoopbackSessionPath(string path) =>
        path.Equals("/api/v1/auth/loopback-session", StringComparison.OrdinalIgnoreCase);

    private static bool IsUnauthenticatedAuthPath(string path) =>
        path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/v1/auth/logout", StringComparison.OrdinalIgnoreCase);

    private static bool IsLoopbackRemote(EndPoint? endPoint)
    {
        if (endPoint is not IPEndPoint ip)
        {
            return false;
        }

        return IPAddress.IsLoopback(ip.Address);
    }

    private static DesktopNodeApiResponse Json(int statusCode, string code, string message)
    {
        return new DesktopNodeApiResponse(
            statusCode,
            "application/json",
            System.Text.Json.JsonSerializer.Serialize(new
            {
                ok = false,
                operation = "api.auth",
                error = new
                {
                    code,
                    message,
                    retryable = false
                }
            }));
    }

    private static string NormalizePrefix(string prefix)
    {
        var uri = new Uri(prefix);
        if (uri.Port != 0)
        {
            return prefix.EndsWith('/') ? prefix : prefix + "/";
        }

        var selectedPort = GetFreeLoopbackPort();
        var builder = new UriBuilder(uri)
        {
            Port = selectedPort
        };
        var normalized = builder.Uri.AbsoluteUri;
        return normalized.EndsWith('/') ? normalized : normalized + "/";
    }

    private static int GetFreeLoopbackPort()
    {
        var socket = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        try
        {
            return ((IPEndPoint)socket.LocalEndpoint).Port;
        }
        finally
        {
            socket.Stop();
        }
    }

    private static bool IsLoopbackPrefix(string prefix)
    {
        var host = new Uri(prefix).DnsSafeHost.ToLowerInvariant();
        return host is "127.0.0.1" or "localhost" or "::1";
    }

    private static ListenerBinding StartListener(string prefix, bool servesApi, bool servesStatic)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();
        return new ListenerBinding(
            listener,
            new Uri(prefix),
            servesApi,
            servesStatic,
            IsLoopbackPrefix(prefix));
    }

    private IReadOnlyDictionary<string, string>? CorsHeaders(HttpListenerRequest request)
    {
        if (string.IsNullOrWhiteSpace(allowedWebOrigin))
        {
            return null;
        }

        var origin = request.Headers["Origin"];
        if (!string.Equals(origin, allowedWebOrigin, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var requestedHeaders = request.Headers["Access-Control-Request-Headers"];
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Access-Control-Allow-Origin"] = origin!,
            ["Access-Control-Allow-Methods"] = "GET, POST, DELETE, OPTIONS",
            ["Access-Control-Allow-Headers"] = string.IsNullOrWhiteSpace(requestedHeaders)
                ? "Authorization, Content-Type, X-PCV-Request-Id, X-Request-Id"
                : requestedHeaders!,
            ["Access-Control-Max-Age"] = "600",
            ["Vary"] = "Origin"
        };
    }

    private static IReadOnlyDictionary<string, string>? MergeHeaders(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string>? second)
    {
        if (first is null || first.Count == 0)
        {
            return second;
        }

        if (second is null || second.Count == 0)
        {
            return first;
        }

        var merged = new Dictionary<string, string>(first, StringComparer.OrdinalIgnoreCase);
        foreach (var header in second)
        {
            merged[header.Key] = header.Value;
        }

        return merged;
    }

    private static string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".html" => "text/html",
            ".js" => "application/javascript",
            ".css" => "text/css",
            _ => "application/octet-stream"
        };
    }
}
