using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Api;

// 요청 바디 / 라우트 id / 쿼리 파싱을 담당한다. DesktopNodeApiRequestProcessor에서 분리되어
// 라우트 소유자가 프로세서를 거치지 않고도 파싱 로직을 사용할 수 있도록 한다.
internal static class DesktopNodeApiRequestParsing
{
    internal const int DefaultJobListLimit = 50;
    internal const int MaxJobListLimit = 200;

    internal static ParsedJson TryParseBody(string? body, string operation)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return ParsedJson.Fail(DesktopNodeApiResponseFactory.Failure(400, operation, "PCV_REQUEST_BODY_MISSING", "The request body is required.", "Pass a JSON object body for this endpoint.", false));
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return ParsedJson.Fail(DesktopNodeApiResponseFactory.Failure(400, operation, "PCV_INVALID_JSON", "The request body must be a JSON object.", "Pass a JSON object body for this endpoint.", false));
            }

            return ParsedJson.Success(document.RootElement.Clone());
        }
        catch (JsonException error)
        {
            return ParsedJson.Fail(DesktopNodeApiResponseFactory.Failure(400, operation, "PCV_INVALID_JSON", "The request body is not valid JSON.", error.Message, false));
        }
    }

    internal static RouteId DecodeRouteId(string encoded, string operation)
    {
        if (Regex.IsMatch(encoded, "%(?![0-9A-Fa-f]{2})", RegexOptions.CultureInvariant))
        {
            return RouteId.Fail(DesktopNodeApiResponseFactory.Failure(400, operation, "PCV_ROUTE_ID_INVALID", "The route id could not be decoded.", "The route id contains a malformed percent escape.", false));
        }

        var decoded = Uri.UnescapeDataString(encoded);
        if (string.IsNullOrWhiteSpace(decoded))
        {
            return RouteId.Fail(DesktopNodeApiResponseFactory.Failure(400, operation, "PCV_ROUTE_ID_INVALID", "The route id is required.", "Pass a non-empty VM id or VM name in the route path.", false));
        }

        return RouteId.Success(decoded);
    }

    internal static bool TryMatch(string path, string pattern, out Match match)
    {
        match = Regex.Match(path, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        return match.Success;
    }

    internal static JobListPage ParseJobListPage(string rawPath)
    {
        var limitValue = QueryValue(rawPath, "limit");
        var offsetValue = QueryValue(rawPath, "offset");

        if (!TryParseNonNegativeInt(limitValue, DefaultJobListLimit, out var limit))
        {
            return JobListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "job.list",
                "PCV_JOB_LIST_PAGE_INVALID",
                "The job list pagination query is invalid.",
                "Use integer limit and offset query values.",
                false));
        }

        if (limit < 1 || limit > MaxJobListLimit)
        {
            return JobListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "job.list",
                "PCV_JOB_LIST_LIMIT_OUT_OF_RANGE",
                $"The job list limit must be between 1 and {MaxJobListLimit}.",
                $"Requested limit '{limit}' is outside the supported range.",
                false));
        }

        if (!TryParseNonNegativeInt(offsetValue, 0, out var offset))
        {
            return JobListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "job.list",
                "PCV_JOB_LIST_PAGE_INVALID",
                "The job list pagination query is invalid.",
                "Use integer limit and offset query values.",
                false));
        }

        if (offset < 0)
        {
            return JobListPage.Fail(DesktopNodeApiResponseFactory.Failure(
                400,
                "job.list",
                "PCV_JOB_LIST_OFFSET_OUT_OF_RANGE",
                "The job list offset must be zero or greater.",
                $"Requested offset '{offset}' is outside the supported range.",
                false));
        }

        return JobListPage.Success(limit, offset);
    }

    internal static bool TryParseNonNegativeInt(string? value, int defaultValue, out int parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = defaultValue;
            return true;
        }

        if (int.TryParse(value, out parsed))
        {
            return true;
        }

        parsed = defaultValue;
        return false;
    }

    internal static string? QueryValue(string rawPath, string name)
    {
        var parts = rawPath.Split('?', 2);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
        {
            return null;
        }

        foreach (var pair in parts[1].Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var keyValue = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(keyValue[0].Replace("+", " "));
            if (!string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return keyValue.Length == 2
                ? Uri.UnescapeDataString(keyValue[1].Replace("+", " "))
                : string.Empty;
        }

        return null;
    }

    internal static string NormalizePath(string path)
    {
        var pathOnly = path.Split('?', 2)[0].TrimEnd('/');
        return string.IsNullOrWhiteSpace(pathOnly) ? "/" : pathOnly;
    }

    internal static string NormalizeRequestId(string? requestId)
    {
        return string.IsNullOrWhiteSpace(requestId)
            ? "req-" + Guid.NewGuid().ToString("N")
            : requestId.Trim();
    }

    internal sealed record ParsedJson(bool Ok, JsonElement? Value, DesktopNodeApiResponse? Response)
    {
        public static ParsedJson Success(JsonElement value) => new(true, value, null);

        public static ParsedJson Fail(DesktopNodeApiResponse response) => new(false, null, response);
    }

    internal sealed record JobListPage(bool Ok, int Limit, int Offset, DesktopNodeApiResponse? Response)
    {
        public static JobListPage Success(int limit, int offset) => new(true, limit, offset, null);

        public static JobListPage Fail(DesktopNodeApiResponse response) => new(false, 0, 0, response);
    }

    internal sealed record RouteId(bool Ok, string? Value, DesktopNodeApiResponse? Response)
    {
        public static RouteId Success(string value) => new(true, value, null);

        public static RouteId Fail(DesktopNodeApiResponse response) => new(false, null, response);
    }
}
