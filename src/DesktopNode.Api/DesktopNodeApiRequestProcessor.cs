using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DesktopNode.Contracts;
using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

public sealed record DesktopNodeApiRequest(
    string Method,
    string Path,
    string? Body = null,
    string? RequestId = null,
    string? ClientIdentity = null,
    string? Authorization = null, bool RemoteIsLoopback = false);

public sealed record DesktopNodeApiResponse(
    int StatusCode,
    string ContentType,
    string Body,
    IReadOnlyDictionary<string, string>? Headers = null);

public sealed record DesktopNodeApiHardeningOptions(
    int RouteTimeoutSeconds = 30,
    int RequestLimitPerMinute = 120,
    int BurstLimit = 20,
    int RetryAfterSeconds = 15,
    Func<DateTimeOffset>? Clock = null,
    int ControlledRouteTimeoutProbeDelayMilliseconds = 0)
{
    public int EffectiveRequestLimit => Math.Max(0, RequestLimitPerMinute) + Math.Max(0, BurstLimit);

    public DateTimeOffset Now() => Clock?.Invoke() ?? DateTimeOffset.UtcNow;
}

public sealed record DesktopNodeApiError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("retryable")] bool Retryable,
    [property: JsonPropertyName("recommended_action")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RecommendedAction = null);

public sealed record DesktopNodeApiWorkerTickResult(
    [property: JsonPropertyName("processed")] bool Processed,
    [property: JsonPropertyName("job")] JsonElement? Job,
    [property: JsonPropertyName("error")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    DesktopNodeApiError? Error = null);

public sealed partial class DesktopNodeApiRequestProcessor
{
    private readonly string tokenStorage;
    private readonly string currentExposure;
    private readonly DesktopNodeJobRuntime jobRuntime;
    private readonly IDesktopNodeApiCancellationScopeFactory cancellationScopes;
    private readonly DesktopNodeApiHardeningOptions hardeningOptions;
    private readonly DesktopNodeApiDiagnosticsHandler diagnosticsHandler;
    private readonly DesktopNodeApiAuthSessionHandler authSessionHandler;
    private readonly DesktopNodeApiOpsSummaryHandler opsSummaryHandler;
    private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;
    private readonly DesktopNodeApiRequestThrottle throttle;
    private readonly DesktopNodeApiConsoleRouteHandler consoleRouteHandler;
    private readonly DesktopNodeApiGuestExecutionRouteHandler guestExecutionRouteHandler;
    private readonly DesktopNodeApiJobRouteHandler jobRouteHandler;
    private readonly DesktopNodeApiJobReconciliationHandler reconciliationHandler;
    private readonly DesktopNodeApiVmMutationRouteHandler vmMutationRouteHandler;
    private readonly DesktopNodeApiJobWorker jobWorker;
    private readonly DesktopNodeApiVmReadRouteHandler vmReadRouteHandler;
    private readonly object sync = new();

    // Deterministic test seam for the provider-result/serialized-finalization boundary.
    // The tick itself lives on DesktopNodeApiJobWorker now, so this forwards rather than
    // storing state: two copies of the seam would let a test arm one and observe the other.
    internal Action? BeforeJobFinalization
    {
        get => jobWorker.BeforeJobFinalization;
        set => jobWorker.BeforeJobFinalization = value;
    }

    private DesktopNodeApiRequestProcessor(
        string tokenStorage,
        string currentExposure,
        IDesktopNodeHyperVNativeAdapter nativeAdapter,
        DesktopNodeApiRuntimeDependencies runtimeDependencies,
        string? batchEvidenceRoot,
        DesktopNodeApiHardeningOptions? hardeningOptions,
        DesktopNodeDiagnosticBundleOptions? diagnosticBundleOptions,
        DesktopNodeAccountAuthOptions? accountAuthOptions,
        DesktopNodeConsoleOptions? consoleOptions,
        string? currentEvidencePath)
    {
        this.tokenStorage = tokenStorage;
        this.currentExposure = currentExposure;
        cancellationScopes = runtimeDependencies.CancellationScopes ?? SystemDesktopNodeApiCancellationScopeFactory.Instance;
        this.hardeningOptions = hardeningOptions ?? new DesktopNodeApiHardeningOptions();
        authSessionHandler = new DesktopNodeApiAuthSessionHandler(accountAuthOptions);
        var resolvedConsoleOptions = consoleOptions ?? new DesktopNodeConsoleOptions();
        jobRuntime = runtimeDependencies.JobRuntime ??
            new DesktopNodeJobRuntime(runtimeDependencies.JobStore, runtimeDependencies.JobClock);
        diagnosticsHandler = new DesktopNodeApiDiagnosticsHandler(
            diagnosticBundleOptions,
            jobRuntime);
        var featureQualification = DesktopNodeCurrentEvidenceProvider.Load(currentEvidencePath);
        opsSummaryHandler = new DesktopNodeApiOpsSummaryHandler(
            new DesktopNodeApiOpsSummaryQuery(
                nativeAdapter,
                jobRuntime,
                authSessionHandler,
                tokenStorage,
                currentExposure,
                resolvedConsoleOptions,
                batchEvidenceRoot,
                diagnosticsHandler.DiagnosticsRoot,
                featureQualification));
        operationInvoker = new DesktopNodeApiHyperVOperationInvoker(nativeAdapter);
        // sync is handed to the throttle and the worker rather than each making its own:
        // this lock is the mutual exclusion between request handling and the worker tick,
        // so a second lock object would silently drop that exclusion.
        throttle = new DesktopNodeApiRequestThrottle(this.hardeningOptions, sync);
        consoleRouteHandler = new DesktopNodeApiConsoleRouteHandler(resolvedConsoleOptions);
        guestExecutionRouteHandler = new DesktopNodeApiGuestExecutionRouteHandler(authSessionHandler);
        jobRouteHandler = new DesktopNodeApiJobRouteHandler(jobRuntime);
        vmReadRouteHandler = new DesktopNodeApiVmReadRouteHandler(operationInvoker, jobRouteHandler);
        reconciliationHandler = new DesktopNodeApiJobReconciliationHandler(
            jobRuntime,
            operationInvoker,
            this.hardeningOptions);
        vmMutationRouteHandler = new DesktopNodeApiVmMutationRouteHandler(
            jobRuntime,
            operationInvoker,
            reconciliationHandler,
            authSessionHandler);
        jobWorker = new DesktopNodeApiJobWorker(
            jobRuntime,
            cancellationScopes,
            operationInvoker,
            sync);
    }

    public DesktopNodeApiResponse Handle(DesktopNodeApiRequest request)
    {
        var normalizedRequest = request with
        {
            RequestId = DesktopNodeApiRequestParsing.NormalizeRequestId(request.RequestId)
        };

        lock (sync)
        {
            var rateLimitResponse = throttle.Enforce(normalizedRequest);
            if (rateLimitResponse is not null)
            {
                return DesktopNodeApiResponseFactory.AttachRequestId(rateLimitResponse, normalizedRequest.RequestId!);
            }

            DesktopNodeApiResponse response;
            try
            {
                response = HandleCoreWithRouteTimeout(normalizedRequest);
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

            return DesktopNodeApiResponseFactory.AttachRequestId(response, normalizedRequest.RequestId!);
        }
    }

    private DesktopNodeApiResponse HandleCoreWithRouteTimeout(DesktopNodeApiRequest request)
    {
        var method = request.Method.ToUpperInvariant();
        var path = DesktopNodeApiRequestParsing.NormalizePath(request.Path);
        if (method != "GET" || !path.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase))
        {
            return HandleCore(request);
        }

        var timeoutSeconds = Math.Max(1, hardeningOptions.RouteTimeoutSeconds);
        var timeout = cancellationScopes.CreateRouteTimeoutScope();
        var routeTask = Task.Run(() => HandleCore(request, timeout.Token), timeout.Token);
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

    private DesktopNodeApiResponse HandleCore(DesktopNodeApiRequest request, CancellationToken cancellationToken = default)
    {
        var method = request.Method.ToUpperInvariant();
        var path = DesktopNodeApiRequestParsing.NormalizePath(request.Path);
        var isQueuedMutationRoute = DesktopNodeApiRuntimeRoutes.TryMatchQueuedMutation(method, path, out var queuedMutationMatch);

        if (method is not ("GET" or "POST") && !isQueuedMutationRoute)
        {
            return DesktopNodeApiResponseFactory.Failure(405, "api.route", "PCV_METHOD_NOT_ALLOWED", $"HTTP method '{method}' is not allowed for this endpoint.", "Supported methods are GET, POST, and DELETE for VM/checkpoint delete jobs.", false);
        }

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

        var jobStoreLoadBlock = jobRuntime.LoadBlock;
        if (jobStoreLoadBlock is not null &&
            DesktopNodeApiRuntimeRoutes.UsesJobStore(method, path) &&
            !DesktopNodeApiRuntimeRoutes.IsOpsSummaryRoute(method, path))
        {
            return DesktopNodeApiResponseFactory.Json(409, DesktopNodeApiResponseFactory.Body(false, "job.store", null, DesktopNodeApiErrorMapping.ToApiError(jobStoreLoadBlock)));
        }

        if (method == "GET" && path == "/api/v1/runtime/route-timeout-probe" &&
            hardeningOptions.ControlledRouteTimeoutProbeDelayMilliseconds > 0)
        {
            return HandleControlledRouteTimeoutProbe(cancellationToken);
        }

        var jobRouteResponse = jobRouteHandler.TryHandle(request, method, path);
        if (jobRouteResponse is not null)
        {
            return jobRouteResponse;
        }

        var reconciliationRouteResponse = reconciliationHandler.TryHandle(method, path, cancellationToken);
        if (reconciliationRouteResponse is not null)
        {
            return reconciliationRouteResponse;
        }

        var diagnosticsRouteResponse = diagnosticsHandler.TryHandle(request, method, path);
        if (diagnosticsRouteResponse is not null)
        {
            return diagnosticsRouteResponse;
        }

        if (isQueuedMutationRoute)
        {
            return vmMutationRouteHandler.HandleQueuedMutationRoute(request, queuedMutationMatch, cancellationToken);
        }

        if (path == "/api/v1/runtime/policy")
        {
            if (method != "GET")
            {
                return DesktopNodeApiResponseFactory.Json(405, new
                {
                    ok = false,
                    error = new
                    {
                        code = "PCV_API_METHOD_NOT_ALLOWED",
                        message = "Method is not allowed for this route."
                    }
                });
            }

            return DesktopNodeApiResponseFactory.Json(200, RuntimePolicyContract.CreateDefault(
                tokenStorage,
                currentExposure,
                authSessionHandler.CreateRuntimePolicy(tokenStorage),
                consoleRouteHandler.CreateRuntimePolicy()));
        }

        var consoleRouteResponse = consoleRouteHandler.TryHandle(method, path);
        if (consoleRouteResponse is not null)
        {
            return consoleRouteResponse;
        }

        var opsSummaryResponse = opsSummaryHandler.TryHandle(
            method,
            path,
            cancellationToken);
        if (opsSummaryResponse is not null)
        {
            return opsSummaryResponse;
        }

        var qosPreviewResponse = vmMutationRouteHandler.TryHandleQosPreview(
            request,
            method,
            path,
            cancellationToken);
        if (qosPreviewResponse is not null)
        {
            return qosPreviewResponse;
        }

        var guestExecutionResponse = guestExecutionRouteHandler.TryHandle(request, method, path);
        if (guestExecutionResponse is not null)
        {
            return guestExecutionResponse;
        }

        if (method == "POST")
        {
            return DesktopNodeApiResponseFactory.Failure(404, "api.route", "PCV_ROUTE_NOT_FOUND", $"No POST route matches '{path}'.", "The requested POST route is not part of the Desktop Node API contract.", false);
        }

        return vmReadRouteHandler.Handle(method, path, cancellationToken);
    }

    private DesktopNodeApiResponse HandleControlledRouteTimeoutProbe(CancellationToken cancellationToken)
    {
        var delayMilliseconds = Math.Clamp(
            hardeningOptions.ControlledRouteTimeoutProbeDelayMilliseconds,
            1,
            600_000);
        Task.Delay(delayMilliseconds, cancellationToken).GetAwaiter().GetResult();
        return DesktopNodeApiResponseFactory.Json(200, DesktopNodeApiResponseFactory.Body(true, "runtime.route_timeout_probe", new SortedDictionary<string, object?>
        {
            ["delay_ms"] = delayMilliseconds,
            ["controlled_probe"] = true
        }, null));
    }

    public DesktopNodeApiWorkerTickResult ProcessOneQueuedJob()
    {
        return jobWorker.ProcessOneQueuedJobAsync().GetAwaiter().GetResult();
    }

    public IReadOnlyList<DesktopNodeApiWorkerTickResult> ProcessWorkerPool(int workerCount = 1)
    {
        var results = new List<DesktopNodeApiWorkerTickResult>();
        // Compatibility wrapper: keep one mutation worker per tick regardless of requested count.
        var boundedWorkerCount = Math.Clamp(workerCount, 1, 1);
        for (var index = 0; index < boundedWorkerCount; index++)
        {
            var tick = jobWorker.ProcessOneQueuedJobAsync().GetAwaiter().GetResult();
            if (!tick.Processed)
            {
                if (tick.Error is not null)
                {
                    results.Add(tick);
                }

                break;
            }

            results.Add(tick);
        }

        return results;
    }

    public async Task RunWorkerLoopAsync(
        CancellationToken cancellationToken,
        int workerCount = 1,
        TimeSpan? idleDelay = null)
    {
        var delay = idleDelay ?? TimeSpan.FromMilliseconds(250);
        while (!cancellationToken.IsCancellationRequested)
        {
            var processed = false;
            // The Desktop Node API runtime currently runs one background mutation worker.
            var boundedWorkerCount = Math.Clamp(workerCount, 1, 1);
            for (var index = 0; index < boundedWorkerCount; index++)
            {
                DesktopNodeApiWorkerTickResult tick;
                try
                {
                    tick = await jobWorker.ProcessOneQueuedJobAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch
                {
                    break;
                }

                if (tick.Error is not null)
                {
                    // A NotCommitted start leaves the job durably queued and is safe to
                    // reevaluate after the normal poll delay. Completion uncertainty sets
                    // the runtime load block, so reevaluation cannot replay the provider.
                    break;
                }

                if (!tick.Processed)
                {
                    break;
                }

                processed = true;
            }

            if (processed)
            {
                continue;
            }

            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

}
