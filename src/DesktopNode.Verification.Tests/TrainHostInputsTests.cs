using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class TrainHostInputsTests
{
    private const string InputPath = "docs/ga-ready/trains/0.42.91-admin-smoke.host-inputs.json";
    private const string FactsPath = "docs/ga-ready/trains/0.42.91-admin-smoke.evidence-facts.json";
    private const string ManifestGoldenPath =
        "src/DesktopNode.Verification.Tests/Fixtures/train-host-inputs/0.42.91-admin-smoke.fullgate-manifest.json";

    // The 0.42.91 train ran its hand-edited capture script from this checkout; the facts pin that script's SHA-256.
    private const string OriginalRepositoryRoot = @"D:\data\projects\codex-zone\purecvisor-desktop-node-public";

    [Fact]
    public void CurrentCardTemplateRebuildsThe04291CaptureScript()
    {
        var input = TrainHostInputs.Parse(File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, InputPath)));

        var text = input.RenderCurrentCard(VerificationCatalogFixture.RepositoryRoot, OriginalRepositoryRoot);

        Assert.Equal(CurrentCardFact("capture_script_sha256"), TrainHostInputsCommand.Sha256(text));
        Assert.Equal("artifacts/installed-operator-surface-current-card-20261006-04291.capture.ps1", input.CurrentCardOutput);
    }

    [Fact]
    public void FullgateManifestTemplateRebuildsThe04291ManifestWithDocumentationValues()
    {
        var input = TrainHostInputs.Parse(File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, InputPath)));

        var text = input.RenderFullgateManifest(
            VerificationCatalogFixture.RepositoryRoot, OriginalRepositoryRoot, "http://192.0.2.10:7777/", "pcv-operator");

        Assert.Equal(File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, ManifestGoldenPath)), text);
        Assert.Equal(
            "full-admin-host-mutation-gate-20261006-04291",
            JsonDocument.Parse(text).RootElement.GetProperty("batch_id").GetString());
        Assert.Equal("artifacts/batch-manifests/full-admin-host-mutation-gate-20261006-04291.json", input.FullgateManifestOutput);
    }

    [Fact]
    public void FullgateManifestWithoutTheLanPrefixStopsAndWritesNothing()
    {
        var root = TemporaryRepository(facts => facts);
        try
        {
            var result = Run(root, "fullgate-manifest", "--write", _ => null);

            Assert.False(result.GetProperty("ok").GetBoolean());
            Assert.Equal(
                "train-host-inputs:lan-prefix-missing:" + TrainHostInputs.LanPrefixVariable,
                result.GetProperty("error_detail").GetString());
            Assert.False(Directory.Exists(Full(root, "artifacts")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void WriteCreatesTheCaptureScriptOnceInATemporaryRepository()
    {
        var root = TemporaryRepository(facts => facts);
        try
        {
            var planned = Run(root, "current-card", "--plan");
            var written = Run(root, "current-card", "--write");
            var again = Run(root, "current-card", "--write");

            Assert.Equal("planned", planned.GetProperty("status").GetString());
            Assert.Equal("written", written.GetProperty("status").GetString());
            Assert.Equal(planned.GetProperty("sha256").GetString(), written.GetProperty("sha256").GetString());
            var script = File.ReadAllText(Full(root, "artifacts/installed-operator-surface-current-card-20261006-04291.capture.ps1"));
            Assert.Contains($"Set-Location '{root}'", script, StringComparison.Ordinal);
            Assert.DoesNotContain("{{", script, StringComparison.Ordinal);
            Assert.False(again.GetProperty("ok").GetBoolean());
            Assert.Equal(
                "train-host-inputs:output-exists:artifacts/installed-operator-surface-current-card-20261006-04291.capture.ps1",
                again.GetProperty("error_detail").GetString());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void MissingFactStopsWithItsNameAndWritesNothing()
    {
        var root = TemporaryRepository(facts =>
        {
            Document(facts, "fullgate").Remove("provenance_commit");
            return facts;
        });
        try
        {
            var result = Run(root, "current-card", "--write");

            Assert.False(result.GetProperty("ok").GetBoolean());
            Assert.Equal("train-host-inputs:fact-missing:fullgate.provenance_commit", result.GetProperty("error_detail").GetString());
            Assert.False(Directory.Exists(Full(root, "artifacts")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static JsonElement Run(string root, string kind, string mode, Func<string, string?>? environment = null)
    {
        using var output = new StringWriter();
        TrainHostInputsCommand.Run(["train-host-inputs", "--input", InputPath, "--kind", kind, mode], root, output, environment);
        return JsonDocument.Parse(output.ToString()).RootElement.Clone();
    }

    private static string TemporaryRepository(Func<JsonObject, JsonObject> editFacts)
    {
        var root = Path.Combine(Path.GetTempPath(), "pcv-host-inputs-" + Guid.NewGuid().ToString("N"));
        Write(root, "src/DesktopNode.sln", string.Empty);
        Write(root, "config/development-verification-suites.json", "{}");
        foreach (var path in new[] { InputPath, TrainHostInputs.CurrentCardTemplate, TrainHostInputs.FullgateManifestTemplate })
        {
            Write(root, path, File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, path)));
        }

        var facts = JsonNode.Parse(File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, FactsPath)))!.AsObject();
        Write(root, FactsPath, editFacts(facts).ToJsonString());
        return root;
    }

    private static string CurrentCardFact(string key)
    {
        var facts = JsonNode.Parse(File.ReadAllText(Full(VerificationCatalogFixture.RepositoryRoot, FactsPath)))!.AsObject();
        return Document(facts, "current-card")[key]!.GetValue<string>();
    }

    private static JsonObject Document(JsonObject facts, string template) =>
        facts["documents"]!.AsArray()
            .Select(document => document!.AsObject())
            .Single(document => document["template"]!.GetValue<string>() == template)["values"]!.AsObject();

    private static void Write(string root, string relativePath, string text)
    {
        var path = Full(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    private static string Full(string root, string relativePath) =>
        Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
}
