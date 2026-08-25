using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

// 이 클래스는 DesktopNodeApiRequestProcessor에서 추출된 응답 봉투(response envelope) 구성 로직을
// 전담합니다. 각 라우트 소유자가 프로세서를 거치지 않고도 표준화된 응답을 만들 수 있도록 합니다.
internal static class DesktopNodeApiResponseFactory
{
    internal static DesktopNodeApiResponse JobCreated(DesktopNodeJobSnapshot job)
    {
        return Json(202, Body(true, "job.create", JobData(job), null));
    }

    internal static DesktopNodeApiResponse OperationResponse(DesktopNodeHyperVOperationResult result)
    {
        return Json(GetStatusForOperationResult(result), result);
    }

    internal static int GetStatusForOperationResult(DesktopNodeHyperVOperationResult result)
    {
        if (result.Ok)
        {
            return 200;
        }

        var code = result.Error?.Code ?? string.Empty;
        if (code.Contains("TIMEOUT", StringComparison.OrdinalIgnoreCase))
        {
            return 504;
        }

        if (code.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase))
        {
            return 404;
        }

        if (code.Contains("INVALID", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("MISSING", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("ALREADY_EXISTS", StringComparison.OrdinalIgnoreCase))
        {
            return 400;
        }

        if (code.Contains("HOST_NOT_READY", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("NOT_ENABLED", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("NOT_RUNNING", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("DEFAULT_SWITCH", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("ADMIN_REQUIRED", StringComparison.OrdinalIgnoreCase))
        {
            return 409;
        }

        return 502;
    }

    internal static DesktopNodeApiResponse Failure(
        int statusCode,
        string operation,
        string code,
        string message,
        string detail,
        bool retryable,
        string? recommendedAction = null)
    {
        return Json(statusCode, Body(false, operation, null, new DesktopNodeApiError(code, message, detail, retryable, recommendedAction)));
    }

    internal static object Body(bool ok, string operation, object? data, DesktopNodeApiError? error)
    {
        return new SortedDictionary<string, object?>
        {
            ["data"] = data,
            ["error"] = error,
            ["ok"] = ok,
            ["operation"] = operation
        };
    }

    internal static JsonElement JobData(DesktopNodeJobSnapshot job)
    {
        return JsonSerializer.SerializeToElement(job, RuntimePolicyContract.JsonOptions);
    }

    internal static JsonElement EmptyObject()
    {
        return JsonFromObject(new SortedDictionary<string, object?>());
    }

    internal static JsonElement JsonFromObject(object value)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, RuntimePolicyContract.JsonOptions));
        return document.RootElement.Clone();
    }

    internal static DesktopNodeApiResponse Json(int statusCode, object payload)
    {
        var body = SerializeResponsePayload(payload);
        return new DesktopNodeApiResponse(
            StatusCode: statusCode,
            ContentType: "application/json",
            Body: body);
    }

    internal static DesktopNodeApiResponse AttachRequestId(
        DesktopNodeApiResponse response,
        string requestId)
    {
        if (!string.Equals(response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            return response;
        }

        using var document = JsonDocument.Parse(response.Body);
        if (document.RootElement.ValueKind != JsonValueKind.Object ||
            document.RootElement.TryGetProperty("request_id", out _))
        {
            return response;
        }

        var enriched = new SortedDictionary<string, object?>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            enriched[property.Name] = property.Value.Clone();
        }

        enriched["request_id"] = requestId;
        return response with
        {
            Body = JsonSerializer.Serialize(enriched, RuntimePolicyContract.JsonOptions)
        };
    }

    internal static string SerializeResponsePayload(object payload)
    {
        return JsonSerializer.Serialize(payload, RuntimePolicyContract.JsonOptions);
    }
}
