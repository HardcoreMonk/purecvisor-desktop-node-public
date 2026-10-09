using System.Text;
using System.Text.Json;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

// docs/FEATURE_IMPLEMENTATION_LEDGER.md 의 "Feature ID 요약" 표는 config/desktop-node-feature-surface-ledger.json 의 투영이다
// (2026-10-09 감사 §9, ADR-0017 §2.4). 쓰기는 packaging/windows-desktop-node/tools/Update-PcvFeatureLedgerDoc.ps1 이 하고,
// Required CI 는 PowerShell 을 돌리지 않으므로 이 시험이 같은 렌더링으로 표가 계약과 같은지 대조한다. 기능 PR 은 표를 손으로
// 고치지 않는다. 도구의 동작 시험은 packaging/windows-desktop-node/manual-admin-tests/PcvFeatureLedgerDoc.Tests.ps1 이다.
[Trait("Category", "Delivery")]
public sealed class PcvFeatureLedgerDocContractTests
{
    private const string Tool = "packaging/windows-desktop-node/tools/Update-PcvFeatureLedgerDoc.ps1";
    private const string Ledger = "config/desktop-node-feature-surface-ledger.json";
    private const string Doc = "docs/FEATURE_IMPLEMENTATION_LEDGER.md";
    private const string BeginMarker = "<!-- BEGIN GENERATED FEATURE ID SUMMARY -->";
    private const string EndMarker = "<!-- END GENERATED FEATURE ID SUMMARY -->";

    [Fact]
    public void FeatureIdSummaryTableMatchesTheSurfaceLedger()
    {
        var repository = RepositoryContractContext.Find();
        using var ledger = repository.LoadJson(Ledger);
        var expected = Render(ledger.RootElement);

        var doc = repository.ReadUtf8Text(Doc).Replace("\r\n", "\n");
        var begin = doc.IndexOf(BeginMarker, StringComparison.Ordinal);
        var end = doc.IndexOf(EndMarker, StringComparison.Ordinal);
        Assert.True(begin >= 0 && end > begin, "the generated block markers must appear with begin before end");
        Assert.Equal(begin, doc.LastIndexOf(BeginMarker, StringComparison.Ordinal));
        Assert.Equal(end, doc.LastIndexOf(EndMarker, StringComparison.Ordinal));

        var actual = doc.Substring(begin, end + EndMarker.Length - begin);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GeneratorToolKeepsTheCheckModeAndDoesNotTouchTheHost()
    {
        var source = RepositoryContractContext.Find().ReadUtf8Text(Tool);

        foreach (var token in new[]
        {
            "Set-StrictMode -Version Latest",
            "[switch]$Check",
            "'pcv-feature-ledger-doc-v1'",
            "[System.Text.Json.JsonDocument]::Parse(",
            BeginMarker,
            EndMarker,
            "PCV_FEATURE_LEDGER_DOC_STALE|",
            "PCV_FEATURE_LEDGER_DOC_MARKERS_INVALID|",
        })
        {
            Assert.Contains(token, source, StringComparison.Ordinal);
        }

        foreach (var forbidden in new[]
        {
            "msiexec", "Set-Service", "Start-Service", "Stop-Service", "New-VM", "Remove-VM",
            "Restart-Computer", "NetFirewallRule", "Invoke-WebRequest", "git push",
        })
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string Render(JsonElement root)
    {
        Assert.Equal("pcv-feature-surface-ledger-v1", root.GetProperty("contract").GetString());

        var builder = new StringBuilder();
        builder.Append(BeginMarker).Append('\n');
        builder.Append("| Feature ID | Title | Routes | Web | CLI |\n");
        builder.Append("|---|---|---:|---:|---:|\n");
        foreach (var feature in root.GetProperty("features").EnumerateArray())
        {
            var id = feature.GetProperty("feature_id").GetString()!;
            builder
                .Append("| <a id=\"").Append(id.Replace('.', '-')).Append("\"></a>`").Append(id).Append("` | ")
                .Append(feature.GetProperty("title").GetString()).Append(" | ")
                .Append(feature.GetProperty("routes").GetArrayLength()).Append(" | ")
                .Append(Count(feature, "web")).Append(" | ")
                .Append(Count(feature, "cli")).Append(" |\n");
        }

        builder.Append(EndMarker);
        return builder.ToString();
    }

    private static string Count(JsonElement feature, string surface)
    {
        var present = 0;
        var excluded = 0;
        foreach (var route in feature.GetProperty("routes").EnumerateArray())
        {
            foreach (var name in route.GetProperty("present_surfaces").EnumerateArray())
            {
                if (name.GetString() == surface)
                {
                    present++;
                }
            }

            foreach (var exclusion in route.GetProperty("excluded_surfaces").EnumerateArray())
            {
                if (exclusion.GetProperty("surface").GetString() == surface)
                {
                    excluded++;
                }
            }
        }

        return $"{present} present / {excluded} excluded";
    }
}
