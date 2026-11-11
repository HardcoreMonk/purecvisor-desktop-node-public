using DesktopNode.Runtime;

namespace DesktopNode.Api;

// BL-0048: 콘솔 frame 과 input 은 전역 sync 를 요청 전체 동안 잡지 않는다.
// throttle.Enforce 만 짧게 lock 하고, 인증과 route 처리는 밖에서 한다.
public sealed partial class DesktopNodeApiRequestProcessor
{
    private static bool IsUnlockedConsoleRoute(DesktopNodeApiRequest request)
    {
        var method = request.Method.ToUpperInvariant();
        var path = DesktopNodeApiRequestParsing.NormalizePath(request.Path);
        return DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, path, "GetVmConsoleFrame", out _)
            || DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, path, "SendVmConsoleInput", out _);
    }

    private DesktopNodeApiResponse HandleUnlockedConsoleRoute(DesktopNodeApiRequest request)
    {
        DesktopNodeApiResponse? rateLimitResponse;
        lock (sync)
        {
            rateLimitResponse = throttle.Enforce(request);
        }

        if (rateLimitResponse is not null)
        {
            return DesktopNodeApiResponseFactory.AttachRequestId(rateLimitResponse, request.RequestId!);
        }

        DesktopNodeApiResponse response;
        try
        {
            response = DispatchUnlockedConsoleRoute(request);
        }
        catch (DesktopNodeJobStoreWriteException exception)
        {
            response = DesktopNodeApiResponseFactory.Json(503, DesktopNodeApiResponseFactory.Body(
                false,
                "job.store",
                null,
                DesktopNodeApiErrorMapping.ToApiError(exception.Error)));
        }
        catch (DesktopNodeJobStoreCommitException exception)
        {
            response = DesktopNodeApiResponseFactory.Json(503, DesktopNodeApiResponseFactory.Body(
                false,
                "job.store",
                null,
                DesktopNodeApiErrorMapping.JobStoreCommitError(
                    exception.Outcome,
                    "requested job transition",
                    jobRuntime.LoadBlock)));
        }

        return DesktopNodeApiResponseFactory.AttachRequestId(response, request.RequestId!);
    }

    private DesktopNodeApiResponse DispatchUnlockedConsoleRoute(DesktopNodeApiRequest request)
    {
        var method = request.Method.ToUpperInvariant();
        var path = DesktopNodeApiRequestParsing.NormalizePath(request.Path);
        if (method == "GET")
        {
            return RunUnlockedConsoleGet(request, method, path);
        }

        return HandleUnlockedConsoleAfterAuth(request, method, path, CancellationToken.None);
    }

    private DesktopNodeApiResponse RunUnlockedConsoleGet(DesktopNodeApiRequest request, string method, string path)
    {
        var timeoutSeconds = Math.Max(1, hardeningOptions.RouteTimeoutSeconds);
        var timeout = cancellationScopes.CreateRouteTimeoutScope();
        var routeTask = Task.Run(
            () => HandleUnlockedConsoleAfterAuth(request, method, path, timeout.Token),
            timeout.Token);
        if (routeTask.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
        {
            try
            {
                return routeTask.GetAwaiter().GetResult();
            }
            finally
            {
                timeout.Dispose();
            }
        }

        timeout.Cancel();
        _ = routeTask.ContinueWith(
            task =>
            {
                _ = task.Exception;
                timeout.Dispose();
            },
            TaskContinuationOptions.ExecuteSynchronously);
        return DesktopNodeApiRequestThrottle.RouteTimeoutResponse(
            timeoutSeconds,
            hardeningOptions.RetryAfterSeconds,
            request.RequestId!);
    }

    private DesktopNodeApiResponse HandleUnlockedConsoleAfterAuth(
        DesktopNodeApiRequest request,
        string method,
        string path,
        CancellationToken cancellationToken)
    {
        var authRouteResponse = authSessionHandler.TryHandle(request, method, path);
        if (authRouteResponse is not null)
        {
            return authRouteResponse;
        }

        var authorizationResponse = authSessionHandler.Authorize(request, method, path);
        if (authorizationResponse is not null)
        {
            return authorizationResponse;
        }

        return consoleRouteHandler.TryHandle(request, method, path, cancellationToken)
            ?? DesktopNodeApiResponseFactory.Failure(
                404,
                "api.route",
                "PCV_ROUTE_NOT_FOUND",
                $"No {method} route matches '{path}'.",
                "The requested route is not part of the Desktop Node API contract.",
                false);
    }
}
