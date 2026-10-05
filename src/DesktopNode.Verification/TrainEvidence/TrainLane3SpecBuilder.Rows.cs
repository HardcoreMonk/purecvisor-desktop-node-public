using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification;

// Ledger rows, index sections, and the whole spec. Row status and evidence are fixed sentences filled from facts;
// rule texts come from the input narrative except the closed pair candidate rule, which is the same every train.
internal sealed partial class TrainLane3SpecBuilder
{
    private const string LedgerRowsContract = "pcv-current-evidence-ledger-rows-rotation-v1";
    private const string IndexSectionsContract = "pcv-promotion-index-sections-v1";
    private const string FunctionalRowId = "functional-correctness-actual-host-latest";

    private const string PairCandidateRule =
        "readiness/update-rollback/clean-host-Windows-Update/Burn/MSIX/runtime ops PASS, `runner_count=6`, `missing_count=0`, `not_pass_count=0`.";

    private const string NextPairEvidence = "`docs/ga-ready/release-train.json` 다음 train 출발 승인 후 연다";

    internal JsonObject Build() => new()
    {
        ["schema_version"] = 1,
        ["contract"] = "pcv-lane3-promotion-docs-v1",
        ["descriptor_chain"] = DescriptorChain(),
        ["ledger_head"] = LedgerHead(),
        ["ledger_rows"] = LedgerRows(),
        ["index_sections"] = IndexSections()
    };

    internal JsonObject LedgerRows()
    {
        var operational =
            $"operational MSI SHA-256 `{Value("fullgate", "operational_fullgate_msi_sha256")}`; " +
            $"payload SHA-256 `{Value("fullgate", "operational_fullgate_payload_aggregate_sha256")}`";
        var pairEvidence =
            $"`{DocumentPath("pair-consume")}`; descriptor `{Value("pair-consume", "descriptor_batch_id")}`; " +
            $"update ZIP SHA-256 `{Value("pair-consume", "update_package_sha256")}`";
        return new JsonObject
        {
            ["schema_version"] = 1,
            ["contract"] = LedgerRowsContract,
            ["promoted_tag"] = Tag,
            ["supersede"] = new JsonArray(
                Row(
                    "full-admin-host-mutation-current",
                    $"`pass`, `{Version}`",
                    $"`{DocumentPath("fullgate")}`; `{Value("fullgate", "batch_evidence_root")}`; {operational}; " +
                    $"provenance `{Value("fullgate", "provenance_commit")}`"),
                Row("manual-admin-package-pair-current", $"`pass`, `{Pair}`", pairEvidence),
                Row(
                    "package-build-current",
                    $"`package-build-pass`, `{Version}`",
                    $"`{DocumentPath("package")}`; `{Value("package", "artifact_root")}`; " +
                    $"MSI SHA-256 `{Value("package", "clean_package_msi_sha256")}`"),
                Row(
                    "installed-operator-surface-smoke-latest",
                    $"`pass`, installed `{Version}`",
                    $"`{DocumentPath("current-card")}`; `{Value("current-card", "artifact_summary")}`; " +
                    $"summary SHA-256 `{Value("current-card", "summary_sha256")}`"),
                Row("manual-admin-package-pair-latest-candidate", $"`pass-closed`, `{Pair}`", pairEvidence, PairCandidateRule)),
            ["replace"] = new JsonArray(
                Row(
                    "latest-product-payload-smoke",
                    $"`pass`, package `{Version}`",
                    $"`{DocumentPath("package")}`; `{DocumentPath("fullgate")}`; {operational}"),
                Row(
                    FunctionalRowId,
                    $"`pass`, carry-forward `{Version}`",
                    $"`{DocumentPath("functional-carryforward")}`; {FunctionalPredecessor()}"),
                Row(
                    "manual-admin-package-pair-next",
                    $"`not-opened-awaiting-next-release-train`, `{Version} -> {NextCandidate}`",
                    NextPairEvidence))
        };
    }

    internal JsonObject IndexSections() => new()
    {
        ["schema_version"] = 1,
        ["contract"] = IndexSectionsContract,
        ["date"] = Date,
        ["previous_version"] = BaselineVersion,
        ["installed_version"] = Version,
        ["p0_feature_ledger_version"] = PreviousString("index_sections", "p0_feature_ledger_version"),
        ["pair_evidence"] = DocumentPath("pair-consume"),
        ["functional_note"] = input.FunctionalNote,
        ["main_push_evidence"] = DocumentPath("main-push")
    };

    internal static string Serialize(JsonObject spec)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
               {
                   Indented = true,
                   Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
               }))
        {
            spec.WriteTo(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    }

    private JsonObject Row(string id, string status, string evidence, string? rule = null) => new()
    {
        ["id"] = id,
        ["status"] = status,
        ["evidence"] = evidence,
        ["rule"] = rule ?? input.Rules[id]
    };

    // The carry-forward predecessor (04275 evidence, summary, SHA-256) does not change between trains, so it is taken
    // from the previous spec's row: everything after the previous train's own carry-forward document.
    private string FunctionalPredecessor()
    {
        var row = (previous["ledger_rows"]?["replace"] as JsonArray)?
            .OfType<JsonObject>()
            .SingleOrDefault(item => item["id"]?.GetValue<string>() == FunctionalRowId);
        var evidence = row?["evidence"] is JsonValue value && value.GetValueKind() == JsonValueKind.String
            ? value.GetValue<string>()
            : throw TrainLane3SpecInput.Invalid("previous-spec-invalid", "ledger_rows.replace." + FunctionalRowId);
        var separator = evidence.IndexOf("; ", StringComparison.Ordinal);
        return separator > 0 && evidence.AsSpan(separator + 2).StartsWith("predecessor ", StringComparison.Ordinal)
            ? evidence[(separator + 2)..]
            : throw TrainLane3SpecInput.Invalid("previous-spec-invalid", "ledger_rows.replace." + FunctionalRowId + ".evidence");
    }
}
