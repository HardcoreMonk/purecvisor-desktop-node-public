using System.Diagnostics;
using System.Text.Json;
using DesktopNode.Api;
using DesktopNode.HyperV;

namespace DesktopNode.Api.Tests;

[CollectionDefinition("API hardening timing isolation", DisableParallelization = true)]
public sealed class ApiHardeningTimingIsolationCollection
{
}

[Collection("API hardening timing isolation")]
public sealed class ApiHardeningRequestProcessorTests
{
    [Fact]
    public void RequestRateLimitReturnsRetryAfterProblemDetailsWhenClientExceedsWindow()
    {
        var clock = new ManualClock(DateTimeOffset.Parse("2026-05-08T00:00:00Z"));
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 30,
                RequestLimitPerMinute: 1,
                BurstLimit: 0,
                RetryAfterSeconds: 7,
                Clock: clock.Now));

        var first = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-a"));
        var second = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-a"));

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(429, second.StatusCode);
        Assert.Equal("application/problem+json", second.ContentType);
        Assert.NotNull(second.Headers);
        Assert.Equal("7", second.Headers!["Retry-After"]);

        using var document = JsonDocument.Parse(second.Body);
        Assert.Equal("about:blank", document.RootElement.GetProperty("type").GetString());
        Assert.Equal("Too Many Requests", document.RootElement.GetProperty("title").GetString());
        Assert.Equal(429, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("PCV_RATE_LIMIT_EXCEEDED", document.RootElement.GetProperty("code").GetString());
        Assert.True(document.RootElement.GetProperty("retryable").GetBoolean());
        Assert.Equal(
            "Wait for the Retry-After interval, then retry with a lower request rate.",
            document.RootElement.GetProperty("recommended_action").GetString());
        Assert.Equal(7, document.RootElement.GetProperty("retry_after_seconds").GetInt32());
        Assert.Equal("rate.limit", document.RootElement.GetProperty("operation").GetString());
        Assert.StartsWith("req-", document.RootElement.GetProperty("request_id").GetString());
    }

    [Fact]
    public void RequestRateLimitWindowExpiresAfterOneMinute()
    {
        var clock = new ManualClock(DateTimeOffset.Parse("2026-05-08T00:00:00Z"));
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 30,
                RequestLimitPerMinute: 1,
                BurstLimit: 0,
                RetryAfterSeconds: 7,
                Clock: clock.Now));

        Assert.Equal(200, processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-a")).StatusCode);
        Assert.Equal(429, processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-a")).StatusCode);

        clock.Advance(TimeSpan.FromSeconds(61));

        Assert.Equal(200, processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-a")).StatusCode);
    }

    [Fact]
    public void RequestRateLimitSeparatesClientIdentities()
    {
        var clock = new ManualClock(DateTimeOffset.Parse("2026-05-08T00:00:00Z"));
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 30,
                RequestLimitPerMinute: 1,
                BurstLimit: 0,
                RetryAfterSeconds: 7,
                Clock: clock.Now));

        Assert.Equal(200, processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-a")).StatusCode);
        Assert.Equal(200, processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-b")).StatusCode);
        Assert.Equal(429, processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", ClientIdentity: "operator-a")).StatusCode);
    }

    [Fact]
    public void RouteTimeoutReturnsProblemDetailsWhenNativeRouteExceedsDeadline()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new DelayedNativeHyperVAdapter(TimeSpan.FromSeconds(2)),
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 1,
                RequestLimitPerMinute: 100,
                BurstLimit: 0,
                RetryAfterSeconds: 7));

        var stopwatch = Stopwatch.StartNew();
        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/vms",
            RequestId: "req-route-timeout",
            ClientIdentity: "operator-a"));
        stopwatch.Stop();

        Assert.Equal(504, response.StatusCode);
        Assert.Equal("application/problem+json", response.ContentType);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(1_750));

        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("Gateway Timeout", document.RootElement.GetProperty("title").GetString());
        Assert.Equal(504, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("PCV_ROUTE_TIMEOUT", document.RootElement.GetProperty("code").GetString());
        Assert.True(document.RootElement.GetProperty("retryable").GetBoolean());
        Assert.Equal(
            "Check the job or route status, then retry after the Retry-After interval if the operation is safe to repeat.",
            document.RootElement.GetProperty("recommended_action").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("route_timeout_seconds").GetInt32());
        Assert.Equal("route.timeout", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("req-route-timeout", document.RootElement.GetProperty("request_id").GetString());
    }

    [Fact]
    public void TimedOutRouteAndFollowingRequestsKeepIndependentRequestIds()
    {
        var adapter = new CancellationIgnoringBlockingReadNativeHyperVAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: adapter,
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 1,
                RequestLimitPerMinute: 100,
                BurstLimit: 0,
                RetryAfterSeconds: 7));

        try
        {
            var timedOut = processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/vms",
                RequestId: "req-overlap-a",
                ClientIdentity: "operator-a"));

            Assert.Equal(504, timedOut.StatusCode);
            Assert.True(adapter.ReadEntered);
            Assert.False(adapter.ReadExited);
            using (var timedOutDocument = JsonDocument.Parse(timedOut.Body))
            {
                Assert.Equal("req-overlap-a", timedOutDocument.RootElement.GetProperty("request_id").GetString());
            }

            var overlapping = processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/runtime/policy",
                RequestId: "req-overlap-b",
                ClientIdentity: "operator-b"));

            Assert.Equal(200, overlapping.StatusCode);
            Assert.False(adapter.ReadExited);
            using (var overlappingDocument = JsonDocument.Parse(overlapping.Body))
            {
                Assert.Equal("req-overlap-b", overlappingDocument.RootElement.GetProperty("request_id").GetString());
            }

            adapter.ReleaseRead();
            Assert.True(adapter.WaitForReadExit(TimeSpan.FromSeconds(3)));

            var following = processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/runtime/policy",
                RequestId: "req-overlap-c",
                ClientIdentity: "operator-c"));

            Assert.Equal(200, following.StatusCode);
            using var followingDocument = JsonDocument.Parse(following.Body);
            Assert.Equal("req-overlap-c", followingDocument.RootElement.GetProperty("request_id").GetString());
        }
        finally
        {
            adapter.ReleaseRead();
        }
    }

    [Fact]
    public void RouteTimeoutPassesCancellationToNativeAdapter()
    {
        var trace = new List<string>();
        var adapter = new CancellationObservingNativeHyperVAdapter();
        var cancellationScopes = new RecordingDesktopNodeApiCancellationScopeFactory(trace);
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(CancellationScopes: cancellationScopes),
            nativeAdapter: adapter,
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 1,
                RequestLimitPerMinute: 100,
                BurstLimit: 0,
                RetryAfterSeconds: 7));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/vms",
            RequestId: "req-timeout-cancel",
            ClientIdentity: "operator-a"));

        Assert.Equal(504, response.StatusCode);
        Assert.True(adapter.WaitForCancellation(TimeSpan.FromSeconds(3)));
        Assert.Single(cancellationScopes.RouteScopes);
        Assert.Equal(cancellationScopes.RouteScopes[0].Token, adapter.ReceivedToken);
        Assert.True(WaitUntil(() => cancellationScopes.RouteScopes[0].Disposed));
        var traceSnapshot = trace.ToArray();
        Assert.Contains("cancellation.route.create", traceSnapshot);
        Assert.Contains("cancellation.route.cancel", traceSnapshot);
        Assert.True(
            Array.IndexOf(traceSnapshot, "cancellation.route.cancel") <
            Array.IndexOf(traceSnapshot, "cancellation.route.dispose"));
    }

    [Fact]
    public void ControlledRouteTimeoutProbeIsNotAvailableByDefault()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 1,
                RequestLimitPerMinute: 100,
                BurstLimit: 0,
                RetryAfterSeconds: 7));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/runtime/route-timeout-probe",
            RequestId: "req-disabled-route-timeout-probe",
            ClientIdentity: "operator-a"));

        Assert.Equal(404, response.StatusCode);
        Assert.Contains("PCV_ROUTE_NOT_FOUND", response.Body);
    }

    [Fact]
    public void ControlledRouteTimeoutProbeReturnsProblemDetailsWhenEnabledDelayExceedsDeadline()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 1,
                RequestLimitPerMinute: 100,
                BurstLimit: 0,
                RetryAfterSeconds: 7,
                ControlledRouteTimeoutProbeDelayMilliseconds: 1_500));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/runtime/route-timeout-probe",
            RequestId: "req-controlled-route-timeout",
            ClientIdentity: "operator-a"));

        Assert.Equal(504, response.StatusCode);
        Assert.Equal("application/problem+json", response.ContentType);
        Assert.Equal("7", response.Headers?["Retry-After"]);

        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("PCV_ROUTE_TIMEOUT", document.RootElement.GetProperty("code").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("route_timeout_seconds").GetInt32());
        Assert.Equal("req-controlled-route-timeout", document.RootElement.GetProperty("request_id").GetString());
    }

    [Fact]
    public void RequestRateLimitInProcessLoadKeepsSuccessBudgetAndProblemDetailsStable()
    {
        var report = RunInProcessHardeningLoad(
            totalRequests: 64,
            requestLimitPerMinute: 16,
            burstLimit: 4,
            retryAfterSeconds: 9);

        Assert.Equal(64, report.TotalRequests);
        Assert.Equal(20, report.SuccessCount);
        Assert.Equal(44, report.RateLimitedCount);
        Assert.Empty(report.UnexpectedStatusCodes);
        Assert.All(report.RateLimitBodies, body =>
        {
            using var document = JsonDocument.Parse(body);
            Assert.Equal("PCV_RATE_LIMIT_EXCEEDED", document.RootElement.GetProperty("code").GetString());
            Assert.Equal(429, document.RootElement.GetProperty("status").GetInt32());
            Assert.Equal(9, document.RootElement.GetProperty("retry_after_seconds").GetInt32());
            Assert.Equal("rate.limit", document.RootElement.GetProperty("operation").GetString());
        });
    }

    [Fact]
    public async Task BackgroundWorkerDoesNotBlockReadRoutesWhileNativeMutationRuns()
    {
        var adapter = new BlockingMutationNativeHyperVAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(nativeAdapter: adapter);
        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
        Assert.Equal(202, create.StatusCode);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var worker = processor.RunWorkerLoopAsync(cts.Token, workerCount: 1, idleDelay: TimeSpan.FromMilliseconds(10));
        await adapter.WaitForMutationEnteredAsync(cts.Token);

        DesktopNodeApiResponse response;
        try
        {
            response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy"));
            Assert.False(adapter.MutationExited);
        }
        finally
        {
            adapter.ReleaseMutation();
            await cts.CancelAsync();
            await worker.WaitAsync(TimeSpan.FromSeconds(5));
        }

        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public void BackgroundWorkerPreservesQueuedJobWhenStartSaveFailsAndReturnsStructuredError()
    {
        var store = new RecordingDesktopNodeApiJobStore
        {
            FailOnWriteAttempt = 2
        };
        var adapter = new CountingMutationNativeHyperVAdapter();
        var processor = DesktopNodeApiRequestProcessor.CreateWithDependencies(
            new DesktopNodeApiRuntimeDependencies(JobStore: store),
            nativeAdapter: adapter);
        var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
        Assert.Equal(202, create.StatusCode);
        using var createdDocument = JsonDocument.Parse(create.Body);
        var jobId = createdDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

        var tick = processor.ProcessOneQueuedJob();
        var response = processor.Handle(new DesktopNodeApiRequest("GET", $"/api/v1/jobs/{jobId}"));
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");

        Assert.False(tick.Processed);
        Assert.Equal("PCV_JOB_STORE_SAVE_FAILED", tick.Error!.Code);
        Assert.DoesNotContain("attempt 2", tick.Error.Detail, StringComparison.Ordinal);
        Assert.Equal("queued", data.GetProperty("status").GetString());
        Assert.Equal(jobId, tick.Job!.Value.GetProperty("job_id").GetString());
        Assert.Equal("queued", tick.Job.Value.GetProperty("status").GetString());
        Assert.Equal(0, adapter.InvokeCount);
        Assert.Equal(2, store.AttemptedSnapshots.Count);
        using var durableDocument = JsonDocument.Parse(store.DurableSnapshot!);
        using var failedAttemptDocument = JsonDocument.Parse(store.AttemptedSnapshots[1]);
        Assert.Equal("queued", durableDocument.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
        Assert.Equal("running", failedAttemptDocument.RootElement.GetProperty("jobs")[0].GetProperty("status").GetString());
    }

    private static HardeningLoadReport RunInProcessHardeningLoad(
        int totalRequests,
        int requestLimitPerMinute,
        int burstLimit,
        int retryAfterSeconds)
    {
        var clock = new ManualClock(DateTimeOffset.Parse("2026-05-08T00:00:00Z"));
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            hardeningOptions: new DesktopNodeApiHardeningOptions(
                RouteTimeoutSeconds: 30,
                RequestLimitPerMinute: requestLimitPerMinute,
                BurstLimit: burstLimit,
                RetryAfterSeconds: retryAfterSeconds,
                Clock: clock.Now));
        var responses = new DesktopNodeApiResponse[totalRequests];

        Parallel.For(0, totalRequests, index =>
        {
            responses[index] = processor.Handle(new DesktopNodeApiRequest(
                "GET",
                "/api/v1/runtime/policy",
                RequestId: $"req-load-{index:D3}",
                ClientIdentity: "load-test-client"));
        });

        var successCount = responses.Count(response => response.StatusCode == 200);
        var rateLimitedResponses = responses
            .Where(response => response.StatusCode == 429)
            .ToArray();
        var unexpectedStatusCodes = responses
            .Where(response => response.StatusCode is not 200 and not 429)
            .Select(response => response.StatusCode)
            .ToArray();

        return new HardeningLoadReport(
            TotalRequests: totalRequests,
            SuccessCount: successCount,
            RateLimitedCount: rateLimitedResponses.Length,
            UnexpectedStatusCodes: unexpectedStatusCodes,
            RateLimitBodies: rateLimitedResponses.Select(response => response.Body).ToArray());
    }

    private sealed class ManualClock(DateTimeOffset now)
    {
        private DateTimeOffset now = now;

        public DateTimeOffset Now() => now;

        public void Advance(TimeSpan delta)
        {
            now = now.Add(delta);
        }
    }

    private sealed class BlockingMutationNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
    {
        private readonly TaskCompletionSource<bool> mutationEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ManualResetEventSlim releaseMutation = new(false);
        private readonly ManualResetEventSlim mutationExited = new(false);

        public bool MutationExited => mutationExited.IsSet;

        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            if (operation == "vm.start")
            {
                mutationEntered.TrySetResult(true);
                try
                {
                    if (!releaseMutation.Wait(TimeSpan.FromSeconds(5)))
                    {
                        throw new TimeoutException("Mutation was not released.");
                    }
                }
                finally
                {
                    mutationExited.Set();
                }
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                {
                    ["name"] = parameters.TryGetProperty("name", out var name) ? name.GetString() : "alpha",
                    ["action"] = "start"
                }),
                Error: null);
            return true;
        }

        public Task WaitForMutationEnteredAsync(CancellationToken cancellationToken) =>
            mutationEntered.Task.WaitAsync(cancellationToken);

        public void ReleaseMutation() => releaseMutation.Set();
    }

    private sealed class CountingMutationNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
    {
        private int invokeCount;

        public int InvokeCount => invokeCount;

        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            Interlocked.Increment(ref invokeCount);
            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
                {
                    ["name"] = parameters.TryGetProperty("name", out var name) ? name.GetString() : "alpha",
                    ["action"] = operation
                }),
                Error: null);
            return true;
        }
    }

    private sealed class DelayedNativeHyperVAdapter(TimeSpan delay) : IDesktopNodeHyperVNativeAdapter
    {
        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            var deadline = DateTimeOffset.UtcNow.Add(delay);
            while (DateTimeOffset.UtcNow < deadline)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    result = DesktopNodeHyperVOperationResult.Failure(
                        operation,
                        "PCV_NATIVE_OPERATION_CANCELED",
                        "The native operation was canceled before it completed.",
                        "The route timeout cancellation token was observed by the delayed test adapter.",
                        true);
                    return true;
                }

                Thread.Sleep(50);
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(Array.Empty<object>()),
                Error: null);
            return true;
        }
    }

    private sealed class CancellationIgnoringBlockingReadNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
    {
        private readonly ManualResetEventSlim readEntered = new(false);
        private readonly ManualResetEventSlim releaseRead = new(false);
        private readonly ManualResetEventSlim readExited = new(false);

        public bool ReadEntered => readEntered.IsSet;

        public bool ReadExited => readExited.IsSet;

        public bool TryInvoke(
            string operation,
            JsonElement parameters,
            CancellationToken cancellationToken,
            out DesktopNodeHyperVOperationResult result)
        {
            if (operation == "vm.list")
            {
                readEntered.Set();
                try
                {
                    _ = releaseRead.Wait(TimeSpan.FromSeconds(5));
                }
                finally
                {
                    readExited.Set();
                }
            }

            result = new DesktopNodeHyperVOperationResult(
                Ok: true,
                Operation: operation,
                Data: JsonSerializer.SerializeToElement(Array.Empty<object>()),
                Error: null);
            return true;
        }

        public void ReleaseRead() => releaseRead.Set();

        public bool WaitForReadExit(TimeSpan timeout) => readExited.Wait(timeout);
    }

    private sealed class CancellationObservingNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
    {
        private readonly ManualResetEventSlim observedCancellation = new(false);

        public CancellationToken ReceivedToken { get; private set; }

        public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
        {
            ReceivedToken = cancellationToken;
            var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
            while (DateTimeOffset.UtcNow < deadline)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    observedCancellation.Set();
                    result = DesktopNodeHyperVOperationResult.Failure(
                        operation,
                        "PCV_NATIVE_OPERATION_CANCELED",
                        "The native operation was canceled before it completed.",
                        "The route timeout cancellation token was observed by the native adapter.",
                        true,
                        "Check route status and retry only if the operation is safe to repeat.");
                    return true;
                }

                Thread.Sleep(50);
            }

            result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_TEST_TIMEOUT_NOT_OBSERVED", "Cancellation was not observed.", "The test adapter did not receive cancellation.", true);
            return true;
        }

        public bool WaitForCancellation(TimeSpan timeout) => observedCancellation.Wait(timeout);
    }

    private sealed record HardeningLoadReport(
        int TotalRequests,
        int SuccessCount,
        int RateLimitedCount,
        IReadOnlyList<int> UnexpectedStatusCodes,
        IReadOnlyList<string> RateLimitBodies);

    // Waits by sleeping rather than spinning. SpinWait burns CPU that the work being waited
    // on needs, so on a loaded runner a short deadline can expire while the work is merely
    // starved. The timeout here is a hang guard, not a performance threshold.
    private static bool WaitUntil(Func<bool> condition)
    {
        var deadline = Environment.TickCount64 + (long)TimeSpan.FromSeconds(60).TotalMilliseconds;
        while (!condition())
        {
            if (Environment.TickCount64 >= deadline)
            {
                return false;
            }

            Thread.Sleep(15);
        }

        return true;
    }

}
