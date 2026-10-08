using System.Text.Json;

namespace DesktopNode.Api.Tests;

// 2026-09-30 개발 완료 campaign 이 닫은 surface 상태를 고정한다. Web 제외는 정책 근거가 있는 네 route 뿐이고
// (noVNC target 저장 폼은 SERVICE_PLAN §7.2 "지금은 열지 않음", vm.limit 은 명시적 QoS/자원 제어와 중복),
// CLI 제외는 계정 session 과 전역 console capability discovery 뿐이다. 제외를 늘리려면 이 테스트와 근거를 함께 바꾼다.
public sealed class ApiSurfaceCompletionContractTests
{
    [Fact]
    public void WebExclusionsAreOnlyThePolicyBackedRoutes()
    {
        Assert.Equal(
            ["console.frame", "console.novnc-target.clear", "console.novnc-target.preview", "console.novnc-target.set", "vm.limit"],
            ExcludedOperations("web"));
    }

    [Fact]
    public void CliExclusionsAreOnlyAccountSessionAndConsoleDiscovery()
    {
        Assert.Equal(
            ["auth.login", "auth.logout", "auth.loopback-session", "auth.rbac", "auth.refresh", "auth.session", "console.capabilities", "console.frame"],
            ExcludedOperations("cli"));
    }

    [Fact]
    public void EveryRouteHasABindingOrAReasonOnEachOperatorSurface()
    {
        foreach (var route in LedgerRoutes())
        {
            var operation = route.GetProperty("operation_id").GetString()!;
            var present = route.GetProperty("present_surfaces").EnumerateArray().Select(item => item.GetString()!).ToHashSet(StringComparer.Ordinal);
            var excluded = route.GetProperty("excluded_surfaces").EnumerateArray().ToArray();
            foreach (var surface in new[] { "web", "cli" })
            {
                var exclusion = excluded.SingleOrDefault(item => item.GetProperty("surface").GetString() == surface);
                if (present.Contains(surface))
                {
                    Assert.True(exclusion.ValueKind == JsonValueKind.Undefined, $"{operation} {surface} is both present and excluded");
                    Assert.True(route.GetProperty("surface_bindings").TryGetProperty(surface, out _), $"{operation} {surface} has no binding");
                }
                else
                {
                    Assert.False(string.IsNullOrWhiteSpace(exclusion.GetProperty("reason").GetString()), $"{operation} {surface} has no exclusion reason");
                }
            }
        }
    }

    private static string[] ExcludedOperations(string surface)
    {
        return LedgerRoutes()
            .Where(route => route.GetProperty("excluded_surfaces").EnumerateArray().Any(item => item.GetProperty("surface").GetString() == surface))
            .Select(route => route.GetProperty("operation_id").GetString()!)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<JsonElement> LedgerRoutes()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "config", "desktop-node-feature-surface-ledger.json")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory!.FullName, "config", "desktop-node-feature-surface-ledger.json")));
        return document.RootElement.GetProperty("features").EnumerateArray()
            .SelectMany(feature => feature.GetProperty("routes").EnumerateArray())
            .Select(route => route.Clone())
            .ToArray();
    }
}
