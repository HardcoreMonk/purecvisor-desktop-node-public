using System.Text.Json;
using System.Xml.Linq;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

// BL-0030 (BL-0029 background): an installed Desktop Node runs its own Test-PcvDesktopNodeUpdatePayload against a
// newer payload. 0.42.96 required web\app.js, web\index.html and web\styles.css, a newer payload dropped two of them,
// and 0.42.96 installs could not update. The committed fixture lists what each supported installed version's
// preflight requires; this class fails a PR whose web payload manifest or MSI payload (the update ZIP is the MSI
// payload root) no longer ships one of those files. It reads no git history, so shallow CI clones work. Lane 3
// promotion refreshes the fixture (docs/DEVELOPMENT_PROCEDURE.md §10).
[Trait("Category", "Installer")]
public sealed class UpdatePreflightCompatibilityContractTests
{
    private const string FixturePath = "packaging/windows-desktop-node/tests/fixtures/update-preflight-compatibility.json";
    private const string PayloadRootPrefix = @"$(var.PayloadRoot)\";
    private static readonly XNamespace Wix = WixSourceContractVerifier.NamespaceUri;

    [Fact]
    public void FixtureNamesTheOperationalCurrentAndTheRetainedOlderInstalls()
    {
        var repository = RepositoryContractContext.Find();
        var fixture = ReadFixture(repository.ReadUtf8Text(FixturePath));
        using var current = JsonDocument.Parse(repository.ReadUtf8Text("docs/ga-ready/current-evidence.json"));
        var currentVersion = current.RootElement.GetProperty("current").GetProperty("version").GetString();

        Assert.Equal("pcv-update-preflight-compatibility-v1", fixture.Contract);
        var operational = Assert.Single(fixture.Sources, source => source.Role == "operational-current");
        Assert.True(
            operational.Version == currentVersion,
            $"Lane 3 promotion refreshes {FixturePath}: operational-current is {operational.Version}, current-evidence.json is {currentVersion}.");
        var retained = Assert.Single(fixture.Sources, source => source.Version == "0.42.96-admin-smoke");
        Assert.Equal("retained-older-install", retained.Role);
        Assert.Equal(["app.js", "index.html", "styles.css"], retained.WebFiles);
        Assert.All(fixture.Sources, source =>
        {
            Assert.Matches("^[0-9a-f]{40}$", source.SourceCommit);
            Assert.NotEmpty(source.RuntimeFiles);
            Assert.NotEmpty(source.WebFiles);
        });
    }

    [Fact]
    public void CurrentPayloadStillShipsEveryFileAnInstalledPreflightRequires()
    {
        var repository = RepositoryContractContext.Find();
        var violations = FindViolations(
            ReadFixture(repository.ReadUtf8Text(FixturePath)),
            repository.ReadUtf8Text("web/payload-manifest.json"),
            repository.ReadUtf8Text("packaging/windows-desktop-node/installer/Product.wxs"),
            repository.ReadUtf8Text("packaging/windows-desktop-node/installer/WebPayload.wxs"));

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void DroppingAFileThatAnOlderPreflightRequiresIsReported()
    {
        var repository = RepositoryContractContext.Find();
        var fixture = ReadFixture(repository.ReadUtf8Text(FixturePath));
        var manifest = repository.ReadUtf8Text("web/payload-manifest.json").Replace("\"app.js\",", string.Empty, StringComparison.Ordinal);
        var webPayload = repository.ReadUtf8Text("packaging/windows-desktop-node/installer/WebPayload.wxs")
            .Replace(@"Source=""$(var.PayloadRoot)\web\styles.css""", @"Source=""$(var.PayloadRoot)\web\styles.removed""", StringComparison.Ordinal);
        var product = repository.ReadUtf8Text("packaging/windows-desktop-node/installer/Product.wxs")
            .Replace(@"Source=""$(var.PayloadRoot)\pcvcli.exe""", @"Source=""$(var.PayloadRoot)\pcvcli.removed""", StringComparison.Ordinal);

        var violations = FindViolations(fixture, manifest, product, webPayload);

        Assert.Contains("0.42.96-admin-smoke web/app.js missing from web/payload-manifest.json", violations);
        Assert.Contains("0.42.96-admin-smoke web/styles.css missing from the MSI web payload", violations);
        Assert.Contains("0.42.96-admin-smoke pcvcli.exe missing from the MSI payload", violations);
        Assert.Contains("0.42.98-admin-smoke pcvcli.exe missing from the MSI payload", violations);
        Assert.DoesNotContain(violations, violation => violation.Contains("index.html", StringComparison.Ordinal));
    }

    private static List<string> FindViolations(Fixture fixture, string manifestJson, string productWxs, string webPayloadWxs)
    {
        using var manifest = JsonDocument.Parse(manifestJson);
        var manifestFiles = manifest.RootElement.GetProperty("core").EnumerateArray()
            .Concat(manifest.RootElement.GetProperty("files").EnumerateArray())
            .Select(item => item.GetString() ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        var msiSources = XDocument.Parse(productWxs).Descendants(Wix + "File")
            .Concat(XDocument.Parse(webPayloadWxs).Descendants(Wix + "File"))
            .Select(file => (string?)file.Attribute("Source") ?? string.Empty)
            .Where(source => source.StartsWith(PayloadRootPrefix, StringComparison.Ordinal))
            .Select(source => source[PayloadRootPrefix.Length..])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var violations = new List<string>();
        foreach (var source in fixture.Sources)
        {
            foreach (var runtimeFile in source.RuntimeFiles)
            {
                if (!msiSources.Contains(runtimeFile.Replace('/', '\\')))
                {
                    violations.Add($"{source.Version} {runtimeFile} missing from the MSI payload");
                }
            }

            foreach (var webFile in source.WebFiles)
            {
                if (!manifestFiles.Contains(webFile))
                {
                    violations.Add($"{source.Version} web/{webFile} missing from web/payload-manifest.json");
                }

                if (!msiSources.Contains(@"web\" + webFile.Replace('/', '\\')))
                {
                    violations.Add($"{source.Version} web/{webFile} missing from the MSI web payload");
                }
            }
        }

        return violations;
    }

    private static Fixture ReadFixture(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        var sources = root.GetProperty("sources").EnumerateArray()
            .Select(source => new FixtureSource(
                source.GetProperty("version").GetString() ?? string.Empty,
                source.GetProperty("role").GetString() ?? string.Empty,
                source.GetProperty("source_commit").GetString() ?? string.Empty,
                Strings(source.GetProperty("runtime_files")),
                Strings(source.GetProperty("web_files"))))
            .ToArray();
        Assert.NotEmpty(sources);
        Assert.Equal(sources.Length, sources.Select(source => source.Version).Distinct(StringComparer.Ordinal).Count());
        return new Fixture(root.GetProperty("contract").GetString() ?? string.Empty, sources);
    }

    private static string[] Strings(JsonElement array) =>
        array.EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray();

    private sealed record Fixture(string Contract, IReadOnlyList<FixtureSource> Sources);

    private sealed record FixtureSource(
        string Version,
        string Role,
        string SourceCommit,
        IReadOnlyList<string> RuntimeFiles,
        IReadOnlyList<string> WebFiles);
}
