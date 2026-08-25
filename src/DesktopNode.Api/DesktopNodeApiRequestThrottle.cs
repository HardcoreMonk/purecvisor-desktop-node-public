using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

// 클라이언트별 요청 창(rate limit)과 경로 timeout 응답을 소유한다.
//
// Enforce 는 lock (sync) 를 잡는데, 호출자인 DesktopNodeApiRequestProcessor.Handle 이 이미 같은
// sync 를 잡고 있다(Monitor 는 같은 스레드에서 재진입 가능하다). 소유자가 자기 잠금을 새로 만들면
// 그 관계가 깨지므로 잠금을 생성자로 받는다. requestWindows 는 이 잠금 아래에서만 접근되므로
// 함께 옮긴다.
internal sealed class DesktopNodeApiRequestThrottle
{
    private readonly DesktopNodeApiHardeningOptions hardeningOptions;
    private readonly object sync;
    private readonly Dictionary<string, Queue<DateTimeOffset>> requestWindows = new(StringComparer.Ordinal);

    public DesktopNodeApiRequestThrottle(DesktopNodeApiHardeningOptions hardeningOptions, object sync)
    {
        this.hardeningOptions = hardeningOptions;
        this.sync = sync;
    }

    public DesktopNodeApiResponse? Enforce(DesktopNodeApiRequest request)
    {
        var path = DesktopNodeApiRequestParsing.NormalizePath(request.Path);
        if (!path.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var limit = hardeningOptions.EffectiveRequestLimit;
        if (limit <= 0)
        {
            return null;
        }

        var now = hardeningOptions.Now();
        var cutoff = now.Subtract(TimeSpan.FromMinutes(1));
        if (string.IsNullOrWhiteSpace(request.ClientIdentity))
        {
            return null;
        }

        var clientIdentity = request.ClientIdentity.Trim();
        lock (sync)
        {
            if (!requestWindows.TryGetValue(clientIdentity, out var window))
            {
                window = new Queue<DateTimeOffset>();
                requestWindows[clientIdentity] = window;
            }

            while (window.Count > 0 && window.Peek() <= cutoff)
            {
                window.Dequeue();
            }

            if (window.Count >= limit)
            {
                return RateLimitExceededResponse(
                    hardeningOptions.RetryAfterSeconds,
                    request.RequestId!);
            }

            window.Enqueue(now);
        }

        return null;
    }

    private static DesktopNodeApiResponse RateLimitExceededResponse(
        int retryAfterSeconds,
        string requestId)
    {
        var boundedRetryAfterSeconds = Math.Max(1, retryAfterSeconds);
        var payload = new SortedDictionary<string, object?>
        {
            ["code"] = "PCV_RATE_LIMIT_EXCEEDED",
            ["detail"] = "The Local API request limit was exceeded for the current client identity.",
            ["instance"] = requestId,
            ["message"] = "The Local API request limit was exceeded for the current client identity.",
            ["operation"] = "rate.limit",
            ["recommended_action"] = "Wait for the Retry-After interval, then retry with a lower request rate.",
            ["request_id"] = requestId,
            ["retry_after_seconds"] = boundedRetryAfterSeconds,
            ["retryable"] = true,
            ["status"] = 429,
            ["title"] = "Too Many Requests",
            ["type"] = "about:blank"
        };
        var headers = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Retry-After"] = boundedRetryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        return new DesktopNodeApiResponse(
            429,
            "application/problem+json",
            JsonSerializer.Serialize(payload, RuntimePolicyContract.JsonOptions),
            headers);
    }

    internal static DesktopNodeApiResponse RouteTimeoutResponse(
        int routeTimeoutSeconds,
        int retryAfterSeconds,
        string requestId)
    {
        var boundedRetryAfterSeconds = Math.Max(1, retryAfterSeconds);
        var payload = new SortedDictionary<string, object?>
        {
            ["code"] = "PCV_ROUTE_TIMEOUT",
            ["detail"] = "The Local API route exceeded the configured response deadline.",
            ["instance"] = requestId,
            ["message"] = "The Local API route timed out before the response deadline.",
            ["operation"] = "route.timeout",
            ["recommended_action"] = "Check the job or route status, then retry after the Retry-After interval if the operation is safe to repeat.",
            ["request_id"] = requestId,
            ["retry_after_seconds"] = boundedRetryAfterSeconds,
            ["retryable"] = true,
            ["route_timeout_seconds"] = routeTimeoutSeconds,
            ["status"] = 504,
            ["title"] = "Gateway Timeout",
            ["type"] = "about:blank"
        };
        var headers = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Retry-After"] = boundedRetryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        return new DesktopNodeApiResponse(
            504,
            "application/problem+json",
            JsonSerializer.Serialize(payload, RuntimePolicyContract.JsonOptions),
            headers);
    }
}
