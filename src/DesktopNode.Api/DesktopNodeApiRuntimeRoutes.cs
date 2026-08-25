using System.Text;
using System.Text.RegularExpressions;

namespace DesktopNode.Api;

internal sealed record DesktopNodeApiRouteMatch(
    ApiHandlerRouteContract Route,
    IReadOnlyDictionary<string, string> Parameters);

internal static class DesktopNodeApiRuntimeRoutes
{
    private const string RuntimeOwner = "dotnet-runtime";

    private static readonly RouteMatcher[] ContractRouteMatchers = ApiHandlerAdapterContract
        .CreateDefault()
        .Routes
        .Select(CreateMatcher)
        .ToArray();

    private static readonly RouteMatcher[] RuntimeRouteMatchers = ContractRouteMatchers
        .Where(matcher => string.Equals(matcher.Route.DefaultOwner, RuntimeOwner, StringComparison.Ordinal))
        .ToArray();

    private static readonly RouteMatcher[] QueuedMutationRouteMatchers = ContractRouteMatchers
        .Where(matcher => matcher.Route.MutationStance == MutationStance.QueuedMutation)
        .ToArray();

    public static IReadOnlyList<ApiHandlerRouteContract> RouteRegistrations => RuntimeRouteMatchers
        .Select(matcher => matcher.Route)
        .ToArray();

    public static IReadOnlyList<ApiHandlerRouteContract> QueuedMutationRouteRegistrations => QueuedMutationRouteMatchers
        .Select(matcher => matcher.Route)
        .ToArray();

    public static bool TryMatchOperation(string method, string path, string operationName, out DesktopNodeApiRouteMatch match)
    {
        if (TryMatch(method, path, out match) &&
            string.Equals(match.Route.OperationName, operationName, StringComparison.Ordinal))
        {
            return true;
        }

        match = null!;
        return false;
    }

    public static string? RequiredPermissionFor(string method, string path)
    {
        return TryMatchContract(method, path, out var match)
            ? match.Route.RequiredPermission
            : null;
    }

    public static bool TryMatch(string method, string path, out DesktopNodeApiRouteMatch match)
    {
        return TryMatch(RuntimeRouteMatchers, method, path, out match);
    }

    public static bool TryMatchContract(string method, string path, out DesktopNodeApiRouteMatch match)
    {
        return TryMatch(ContractRouteMatchers, method, path, out match);
    }

    public static bool TryMatchQueuedMutation(string method, string path, out DesktopNodeApiRouteMatch match)
    {
        return TryMatch(QueuedMutationRouteMatchers, method, path, out match);
    }

    public static bool IsQueuedMutationRoute(string method, string path)
    {
        return TryMatchQueuedMutation(method, path, out _);
    }

    public static bool UsesJobStore(string method, string path)
    {
        return IsJobRoute(method, path) ||
            TryMatchOperation(method, path, "OpsSummary", out _) ||
            IsQueuedMutationRoute(method, path);
    }

    public static bool IsAccountAuthRoute(string method, string path)
    {
        return IsRouteFamily(method, path, "auth");
    }

    public static bool IsDiagnosticsRoute(string method, string path)
    {
        return IsRouteFamily(method, path, "diagnostics");
    }

    public static bool IsJobRoute(string method, string path)
    {
        return IsRouteFamily(method, path, "jobs");
    }

    public static bool IsConsoleRoute(string method, string path)
    {
        return IsRouteFamily(method, path, "console");
    }

    public static bool IsOpsSummaryRoute(string method, string path)
    {
        return IsRouteFamily(method, path, "ops-summary");
    }

    private static bool IsRouteFamily(string method, string path, string routeFamily)
    {
        return TryMatch(method, path, out var match) &&
            string.Equals(match.Route.RouteFamily, routeFamily, StringComparison.Ordinal);
    }

    private static bool TryMatch(
        IReadOnlyList<RouteMatcher> matchers,
        string method,
        string path,
        out DesktopNodeApiRouteMatch match)
    {
        foreach (var matcher in matchers)
        {
            if (!string.Equals(matcher.Route.Method, method, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var routeMatch = matcher.Regex.Match(path);
            if (!routeMatch.Success)
            {
                continue;
            }

            var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var parameterName in matcher.ParameterNames)
            {
                parameters[parameterName] = Uri.UnescapeDataString(routeMatch.Groups[parameterName].Value);
            }

            match = new DesktopNodeApiRouteMatch(matcher.Route, parameters);
            return true;
        }

        match = null!;
        return false;
    }

    private static RouteMatcher CreateMatcher(ApiHandlerRouteContract route)
    {
        var parameterNames = new List<string>();
        var builder = new StringBuilder("^");
        for (var index = 0; index < route.RouteTemplate.Length; index++)
        {
            var current = route.RouteTemplate[index];
            if (current != '{')
            {
                builder.Append(Regex.Escape(current.ToString()));
                continue;
            }

            var end = route.RouteTemplate.IndexOf('}', index + 1);
            if (end <= index + 1)
            {
                throw new InvalidOperationException($"Invalid API route template '{route.RouteTemplate}'.");
            }

            var parameterName = route.RouteTemplate[(index + 1)..end];
            parameterNames.Add(parameterName);
            builder.Append("(?<");
            builder.Append(parameterName);
            builder.Append(">[^/]+)");
            index = end;
        }

        builder.Append("$");
        return new RouteMatcher(
            route,
            new Regex(builder.ToString(), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase),
            parameterNames.ToArray());
    }

    private sealed record RouteMatcher(
        ApiHandlerRouteContract Route,
        Regex Regex,
        IReadOnlyList<string> ParameterNames);
}
