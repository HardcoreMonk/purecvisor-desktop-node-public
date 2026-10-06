using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Verification;

// Tracked Lane 2 probes (design pcv-train-host-inputs-v1 section 3.4). Required CI runs no PowerShell, so the scripts are
// held to static contracts: catalogued, parameterised, plan-only before any host call, and free of private values.
public sealed class Lane2ProbeCatalogContractTests
{
    private const string ProbeDirectory = "packaging/windows-desktop-node/lane2-probes";
    private const string CatalogPath = ProbeDirectory + "/catalog.json";
    private const string CommonPath = ProbeDirectory + "/PcvLane2ProbeCommon.ps1";

    private static readonly Regex SecretConstant = new(
        "(?i)(password|passwd|secret|token|credential)\\w*\\s*=\\s*['\"][^'\"]+['\"]", RegexOptions.CultureInvariant);

    private static readonly Regex PrivateAddress = new(
        "\\b(10\\.[0-9]{1,3}|192\\.168|172\\.(1[6-9]|2[0-9]|3[01]))\\.[0-9]{1,3}\\.[0-9]{1,3}\\b", RegexOptions.CultureInvariant);

    private static readonly Regex TrainRunTag = new("\\b[0-9]{8}-[0-9]{5}\\b", RegexOptions.CultureInvariant);

    private static readonly string Root = RepositoryContractContext.Find().RootPath;

    public static TheoryData<string> ProbeScripts()
    {
        var data = new TheoryData<string>();
        foreach (var script in Catalog().Select(probe => probe.Script))
        {
            data.Add(script);
        }

        return data;
    }

    [Fact]
    public void CatalogListsEveryProbeScriptOnce()
    {
        using var catalog = JsonDocument.Parse(File.ReadAllText(Full(CatalogPath)));
        Assert.Equal("pcv-lane2-probe-catalog-v1", catalog.RootElement.GetProperty("contract").GetString());
        Assert.Equal(CommonPath, catalog.RootElement.GetProperty("common").GetString());

        var probes = Catalog();
        Assert.Equal(probes.Count, probes.Select(probe => probe.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.All(probes, probe =>
        {
            Assert.True(File.Exists(Full(probe.Script)), probe.Script);
            Assert.NotEmpty(probe.ServicePlanItems);
            Assert.False(string.IsNullOrWhiteSpace(probe.HostMutation), probe.Id);
        });

        var onDisk = Directory.GetFiles(Full(ProbeDirectory), "*.ps1")
            .Select(path => ProbeDirectory + "/" + Path.GetFileName(path))
            .Order(StringComparer.Ordinal);
        Assert.Equal(probes.Select(probe => probe.Script).Append(CommonPath).Order(StringComparer.Ordinal), onDisk);
    }

    [Theory]
    [MemberData(nameof(ProbeScripts))]
    public void ProbeScriptPlansBeforeAnyHostCall(string script)
    {
        var text = File.ReadAllText(Full(script));

        Assert.Contains("[Parameter(Mandatory)][string]$ArtifactRoot", text, StringComparison.Ordinal);
        Assert.Contains("[Parameter(Mandatory)][string]$EvidenceId", text, StringComparison.Ordinal);
        Assert.Contains("[switch]$PlanOnly", text, StringComparison.Ordinal);
        Assert.Contains(". (Join-Path $PSScriptRoot 'PcvLane2ProbeCommon.ps1')", text, StringComparison.Ordinal);

        var planOnly = text.IndexOf("if ($PlanOnly) {", StringComparison.Ordinal);
        var initialize = text.IndexOf("Initialize-PcvLane2Probe -ArtifactRoot", StringComparison.Ordinal);
        var firstCli = text.IndexOf("Invoke-PcvProbeCli '", StringComparison.Ordinal);
        Assert.True(planOnly >= 0 && initialize > planOnly, script);
        Assert.True(firstCli > initialize, script);
    }

    [Fact]
    public void ProbeFilesCarryNoSecretsPrivateAddressesOrTrainTags()
    {
        foreach (var path in Catalog().Select(probe => probe.Script).Append(CommonPath).Append(CatalogPath))
        {
            var text = File.ReadAllText(Full(path));
            Assert.False(SecretConstant.IsMatch(text), path + ": secret constant");
            Assert.False(PrivateAddress.IsMatch(text), path + ": private address");
            Assert.False(TrainRunTag.IsMatch(text), path + ": train run tag");
            Assert.DoesNotContain("codex-zone", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void CommonInitializationRefusesAnExistingArtifactRoot()
    {
        var text = File.ReadAllText(Full(CommonPath));

        Assert.Contains("if (Test-Path -LiteralPath $ArtifactRoot) { throw", text, StringComparison.Ordinal);
        Assert.Contains("function ConvertTo-PcvArray", text, StringComparison.Ordinal);
        Assert.Contains("$Plan['host_mutation_performed'] = $false", text, StringComparison.Ordinal);
    }

    private static List<ProbeEntry> Catalog()
    {
        using var catalog = JsonDocument.Parse(File.ReadAllText(Full(CatalogPath)));
        return catalog.RootElement.GetProperty("probes").EnumerateArray()
            .Select(probe => new ProbeEntry(
                probe.GetProperty("id").GetString()!,
                probe.GetProperty("script").GetString()!,
                probe.GetProperty("service_plan_items").EnumerateArray().Select(item => item.GetString()!).ToList(),
                probe.GetProperty("host_mutation").GetString()!))
            .ToList();
    }

    private static string Full(string relativePath) =>
        Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private sealed record ProbeEntry(string Id, string Script, IReadOnlyList<string> ServicePlanItems, string HostMutation);
}
