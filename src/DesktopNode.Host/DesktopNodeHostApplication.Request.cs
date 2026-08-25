using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using DesktopNode.Api;

namespace DesktopNode.Host;

public sealed partial class DesktopNodeHostApplication : IDisposable
{
    private async Task RunAsync(ListenerBinding binding, CancellationToken token)
    {
        while (!token.IsCancellationRequested && binding.Listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await binding.Listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (HttpListenerException)
            {
                return;
            }

            var requestTask = Task.Run(() => HandleAsync(binding, context, token), CancellationToken.None);
            if (options.RequestLifetimeMode == DesktopNodeRequestLifetimeMode.TrackedAsyncSerialized)
            {
                TrackRequest(requestTask);
            }
        }
    }

    private void TrackRequest(Task requestTask)
    {
        var requestId = Interlocked.Increment(ref nextRequestId);
        requestTasks[requestId] = requestTask;
        _ = requestTask.ContinueWith(
            completedTask =>
            {
                _ = completedTask.Exception;
                requestTasks.TryRemove(requestId, out var ignored);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private async Task HandleAsync(
        ListenerBinding binding,
        HttpListenerContext context,
        CancellationToken requestToken)
    {
        DesktopNodeRequestAdmission.Lease? admissionLease = null;
        try
        {
            var request = context.Request;
            var path = request.Url?.AbsolutePath ?? "/";
            var isApiPath = path.Equals("/api", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
            if (isApiPath && !binding.ServesApi)
            {
                await WriteTextAsync(
                    context.Response,
                    404,
                    "application/json",
                    "{\"ok\":false,\"error\":{\"code\":\"PCV_API_ROUTE_ON_WEB_PORT\",\"message\":\"API routes are served by the Local API listener, not the Web Console listener.\"}}").ConfigureAwait(false);
                return;
            }

            var isStaticRequest =
                request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
                !isApiPath &&
                binding.ServesStatic &&
                !string.IsNullOrWhiteSpace(options.WebRootPath);

            if (isStaticRequest && (string.IsNullOrWhiteSpace(token.Value) || binding.IsLoopback))
            {
                await WriteStaticFileAsync(context.Response, request.Url?.PathAndQuery ?? "/").ConfigureAwait(false);
                return;
            }

            if (!binding.ServesApi && !isStaticRequest)
            {
                await WriteTextAsync(
                    context.Response,
                    404,
                    "application/json",
                    "{\"ok\":false,\"error\":{\"code\":\"PCV_WEB_ROUTE_NOT_FOUND\",\"message\":\"The Web Console listener only serves static assets.\"}}").ConfigureAwait(false);
                return;
            }

            if (binding.ServesApi && request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                await WriteTextAsync(
                    context.Response,
                    204,
                    "text/plain",
                    string.Empty,
                    CorsHeaders(request)).ConfigureAwait(false);
                return;
            }

            if (binding.ServesApi && request.IsWebSocketRequest && TryMatchNoVncBridgePath(path, out var vmId))
            {
                await HandleNoVncBridgeAsync(context, vmId, requestToken).ConfigureAwait(false);
                return;
            }

            var auth = Authorize(request);
            if (auth is not null)
            {
                await WriteTextAsync(context.Response, auth.StatusCode, auth.ContentType, auth.Body, CorsHeaders(request)).ConfigureAwait(false);
                return;
            }

            if (isStaticRequest)
            {
                await WriteStaticFileAsync(context.Response, request.Url?.PathAndQuery ?? "/").ConfigureAwait(false);
                return;
            }

            var bodyAdmission = await TryAcquireRequestAdmissionAsync(request, context.Response, requestToken).ConfigureAwait(false);
            if (!bodyAdmission.Accepted)
            {
                return;
            }

            admissionLease = bodyAdmission.Lease;

            var body = await ReadRequestBodyAsync(request, options.MaxRequestBodyBytes, requestToken).ConfigureAwait(false);
            var response = processor.Handle(new DesktopNodeApiRequest(
                request.HttpMethod,
                path.TrimEnd('/'),
                body,
                RequestId: ResolveRequestId(request),
                ClientIdentity: request.RemoteEndPoint?.Address.ToString(),
                Authorization: request.Headers["Authorization"],
                RemoteIsLoopback: IsLoopbackRemote(request.RemoteEndPoint)));
            await WriteTextAsync(context.Response, response.StatusCode, response.ContentType, response.Body, MergeHeaders(response.Headers, CorsHeaders(request))).ConfigureAwait(false);
        }
        catch (DesktopNodeHostRequestBodyTooLargeException ex)
        {
            await WriteTextAsync(
                context.Response,
                413,
                "application/problem+json",
                RequestBodyTooLargeProblem(ex.MaxBytes, ResolveRequestId(context.Request)),
                CorsHeaders(context.Request)).ConfigureAwait(false);
        }
        finally
        {
            admissionLease?.Dispose();
            context.Response.Close();
        }
    }

    private async ValueTask<(bool Accepted, DesktopNodeRequestAdmission.Lease? Lease)> TryAcquireRequestAdmissionAsync(
        HttpListenerRequest request,
        HttpListenerResponse response,
        CancellationToken requestToken)
    {
        if (requestAdmission is null)
        {
            return (true, null);
        }

        var lease = await requestAdmission.TryEnterAsync(requestToken).ConfigureAwait(false);
        if (lease is not null)
        {
            return (true, lease);
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Retry-After"] = Math.Max(1, options.RetryAfterSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        await WriteTextAsync(
            response,
            503,
            "application/problem+json",
            RequestAdmissionLimitProblem(ResolveRequestId(request)),
            MergeHeaders(headers, CorsHeaders(request))).ConfigureAwait(false);
        return (false, null);
    }

    private static async Task<string?> ReadRequestBodyAsync(
        HttpListenerRequest request,
        int maxRequestBodyBytes,
        CancellationToken cancellationToken)
    {
        if (!request.HasEntityBody)
        {
            return null;
        }

        var maxBytes = Math.Clamp(
            maxRequestBodyBytes,
            DesktopNodeHostOptions.MinimumMaxRequestBodyBytes,
            DesktopNodeHostOptions.MaximumMaxRequestBodyBytes);

        if (request.ContentLength64 > maxBytes)
        {
            throw new DesktopNodeHostRequestBodyTooLargeException(maxBytes);
        }

        await using var memory = new MemoryStream();
        var buffer = new byte[Math.Min(8192, maxBytes)];
        while (true)
        {
            var remainingWithOverflowSentinel = (int)Math.Min(buffer.Length, maxBytes - memory.Length + 1);
            var read = await request.InputStream.ReadAsync(buffer.AsMemory(0, remainingWithOverflowSentinel), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            if (memory.Length + read > maxBytes)
            {
                throw new DesktopNodeHostRequestBodyTooLargeException(maxBytes);
            }

            memory.Write(buffer, 0, read);
        }

        return request.ContentEncoding.GetString(memory.ToArray());
    }

    private static string RequestBodyTooLargeProblem(long maxBytes, string? requestId)
    {
        var id = string.IsNullOrWhiteSpace(requestId) ? "req-" + Guid.NewGuid().ToString("N") : requestId;
        return System.Text.Json.JsonSerializer.Serialize(new SortedDictionary<string, object?>
        {
            ["type"] = "about:blank",
            ["title"] = "Payload Too Large",
            ["status"] = 413,
            ["code"] = "PCV_REQUEST_BODY_TOO_LARGE",
            ["operation"] = "request.body",
            ["message"] = "The API request body is larger than the configured listener limit.",
            ["detail"] = $"The listener rejected the request before reading the full body. Configured limit is {maxBytes} bytes.",
            ["recommended_action"] = "Send a smaller JSON body or restart the service with a larger --max-request-body-bytes value within the supported range.",
            ["request_id"] = id,
            ["retryable"] = false,
            ["max_request_body_bytes"] = maxBytes
        }, DesktopNode.Contracts.RuntimePolicyContract.JsonOptions);
    }

    private static string RequestAdmissionLimitProblem(string? requestId)
    {
        var id = string.IsNullOrWhiteSpace(requestId) ? "req-" + Guid.NewGuid().ToString("N") : requestId;
        return System.Text.Json.JsonSerializer.Serialize(new SortedDictionary<string, object?>
        {
            ["type"] = "about:blank",
            ["title"] = "Service Unavailable",
            ["status"] = 503,
            ["code"] = "PCV_REQUEST_ADMISSION_LIMIT_EXCEEDED",
            ["operation"] = "request.admission",
            ["message"] = "The listener request admission limit was exceeded.",
            ["detail"] = "The request was rejected before the body was read because active and waiting request capacity is exhausted.",
            ["recommended_action"] = "Retry after the Retry-After interval and reduce concurrent request pressure.",
            ["request_id"] = id,
            ["retryable"] = true
        }, DesktopNode.Contracts.RuntimePolicyContract.JsonOptions);
    }

    private static string? ResolveRequestId(HttpListenerRequest request)
    {
        var pcvRequestId = request.Headers["X-PCV-Request-Id"];
        if (!string.IsNullOrWhiteSpace(pcvRequestId))
        {
            return pcvRequestId;
        }

        var requestId = request.Headers["X-Request-Id"];
        return string.IsNullOrWhiteSpace(requestId) ? null : requestId;
    }

}
