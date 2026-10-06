using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification;

// Builds the lane3-promotion-docs-v1 spec from the previous spec and the train facts file. Every value comes from a
// facts document, the previous spec, the train version, or the input narrative (design pcv-train-lane3-spec-generator-v1 §2).
internal sealed partial class TrainLane3SpecBuilder
{
    private const string RotationContract = "pcv-manual-admin-descriptor-chain-rotation-v1";
    private const string NextCandidate = "next-admin-smoke-required";

    private readonly TrainLane3SpecInput input;
    private readonly JsonObject previous;
    private readonly IReadOnlyDictionary<string, TrainEvidenceDocument> documents;

    // main-push cites the post-merge run of a two-PR train; main-push-payload cites the payload commit run of a single-PR
    // train (design pcv-single-pr-train-v1). A facts file carries exactly one of them.
    private readonly string mainPushTemplate;

    internal TrainLane3SpecBuilder(string repositoryRoot, TrainLane3SpecInput input)
    {
        this.input = input;
        previous = JsonNode.Parse(ReadRepositoryFile(repositoryRoot, input.PreviousSpec)) as JsonObject
            ?? throw TrainLane3SpecInput.Invalid("previous-spec-invalid", input.PreviousSpec);
        var facts = TrainEvidenceFactsReader.Parse(ReadRepositoryFile(repositoryRoot, input.Facts));
        if (facts.Version != input.Version)
        {
            throw TrainLane3SpecInput.Invalid("facts-version-mismatch", input.Facts);
        }

        var byTemplate = new Dictionary<string, TrainEvidenceDocument>(StringComparer.Ordinal);
        foreach (var document in facts.Documents)
        {
            if (!byTemplate.TryAdd(document.Template, document))
            {
                throw TrainLane3SpecInput.Invalid("facts-duplicate-template", document.Template);
            }
        }

        documents = byTemplate;
        var mainPush = new[] { "main-push", "main-push-payload" }.Where(byTemplate.ContainsKey).ToList();
        mainPushTemplate = mainPush.Count switch
        {
            1 => mainPush[0],
            0 => throw TrainLane3SpecInput.Invalid("facts-document-missing", "main-push"),
            _ => throw TrainLane3SpecInput.Invalid("facts-main-push-ambiguous", input.Facts)
        };
        if (PreviousString("descriptor_chain", "next_previous_tag") != input.PreviousTag)
        {
            throw TrainLane3SpecInput.Invalid("previous-spec-tag-mismatch", input.PreviousSpec);
        }

        RequireTrain(repositoryRoot);
        Require(Value("pair-consume", "target_version") == input.Version, "pair-consume.target_version");
        Require(Value("fullgate", "version") == input.Version, "fullgate.version");
        Require(Value("pair-consume", "approved_target_msi_sha256") == Value("package", "clean_package_msi_sha256"), "target-msi");
    }

    private string Version => input.Version;

    private string Tag => input.Tag;

    private string BaselineVersion => Value("pair-consume", "baseline_version");

    private string Pair => $"{BaselineVersion} -> {Version}";

    private string Date => Value(mainPushTemplate, "date");

    private string ConsumeRoot => Value("pair-consume", "artifact_root");

    private string ConsumeSummary => ConsumeRoot + "/manual-admin-campaign-descriptor/summary.json";

    internal JsonObject DescriptorChain()
    {
        var values = new JsonObject
        {
            ["descriptor_id"] = $"manual-admin-next-campaign-descriptor-{Date}-{Tag}-promotion-closure",
            ["updated_at"] = input.UpdatedAt,
            ["current_status"] =
                $"closed-package-pair-{TrainLane3SpecInput.TagOf(BaselineVersion)}-{Tag}-and-{Tag}-fullgate-functional-{FunctionalMode()}-current-card-pass",
            ["current_manual_admin_package_pair"] = Pair,
            ["current_manual_admin_campaign"] = DocumentPath("pair-consume"),
            ["current_manual_admin_campaign_root"] = ConsumeRoot,
            ["current_manual_admin_target_package_root"] = Value("package", "artifact_root"),
            ["current_manual_admin_target_msi_sha256"] = Value("pair-consume", "approved_target_msi_sha256"),
            ["current_manual_admin_update_package_sha256"] = Value("pair-consume", "update_package_sha256"),
            ["current_manual_admin_descriptor_batch_manifest"] = Value("pair-consume", "descriptor_batch_id"),
            ["current_manual_admin_descriptor_summary"] = ConsumeSummary,
            ["current_manual_admin_days_since_previous_closure"] = DaysSincePreviousClosure(),
            ["current_installed_operator_surface_current_card_evidence"] = DocumentPath("current-card"),
            ["current_installed_operator_surface_current_card_summary"] = Value("current-card", "artifact_summary"),
            ["current_installed_operator_surface_current_card_summary_sha256"] = Value("current-card", "summary_sha256"),
            ["current_functional_correctness_actual_host_evidence"] = DocumentPath("functional-carryforward"),
            ["latest_manual_admin_candidate_package_pair"] = Pair,
            ["latest_manual_admin_candidate_campaign"] = DocumentPath("pair-consume"),
            ["latest_manual_admin_candidate_campaign_root"] = ConsumeRoot,
            ["latest_manual_admin_candidate_descriptor_batch_manifest"] = Value("pair-consume", "descriptor_batch_id"),
            ["latest_manual_admin_candidate_descriptor_summary"] = ConsumeSummary,
            ["latest_manual_admin_candidate_target_msi_sha256"] = Value("pair-consume", "approved_target_msi_sha256"),
            ["latest_manual_admin_candidate_update_package_sha256"] = Value("pair-consume", "update_package_sha256"),
            ["latest_manual_admin_candidate_provenance_commit"] = Value("package", "source_commit"),
            ["next_manual_admin_package_pair_trigger"] = $"release-train-departure-after-{Tag}",
            ["next_manual_admin_package_pair_candidate"] = $"{Version} -> {NextCandidate}",
            ["next_manual_admin_package_pair_candidate_status"] = "not-opened-awaiting-next-release-train",
            ["next_manual_admin_package_pair_payload_change_source_commit_range"] =
                $"{Value("fullgate", "provenance_commit")}..{NextCandidate}",
            ["next_manual_admin_package_pair_payload_changed_source_file_count"] = "0",
            ["current_full_admin_host_mutation_gate"] = DocumentPath("fullgate"),
            ["current_full_admin_host_mutation_batch"] = Value("fullgate", "batch_id"),
            ["latest_full_admin_gate_batch"] = Value("fullgate", "batch_id"),
            ["current_full_admin_host_mutation_current_card"] = Value("current-card", "artifact_summary"),
            ["current_full_admin_host_mutation_payload_aggregate_sha256"] =
                Value("fullgate", "operational_fullgate_payload_aggregate_sha256"),
            ["current_full_admin_host_mutation_operational_msi_sha256"] = Value("fullgate", "operational_fullgate_msi_sha256"),
            ["current_full_admin_host_mutation_provenance_commit"] = Value("fullgate", "provenance_commit"),
            ["current_full_admin_host_mutation_routeparity_artifact_root"] = Value("fullgate", "routeparity_artifact_root"),
            ["current_full_admin_host_mutation_os_mutation_artifact_root"] = Value("fullgate", "os_mutation_artifact_root"),
            ["current_required_ci_docs_only_package_candidate_decision"] = $"retain-{Version}-no-new-candidate",
            ["current_public_boundary_main_push_compatibility_alias_semantics"] = mainPushTemplate == "main-push"
                ? $"{Tag}-pr{Value("main-push", "pr")}-postmerge-not-provider-required-authority"
                : $"{Tag}-payload-main-push-not-provider-required-authority",
            ["current_public_boundary_main_push_evidence"] = DocumentPath(mainPushTemplate),
            ["current_public_boundary_main_push_run_id"] = Value(mainPushTemplate, "public_boundary_run_id"),
            ["current_public_boundary_main_push_job_id"] = Value(mainPushTemplate, "public_boundary_job_id"),
            ["current_public_boundary_main_push_head_sha"] = Value(mainPushTemplate, "head_sha"),
            ["current_public_boundary_main_push_package_candidate_decision"] = Value(mainPushTemplate, "package_candidate_decision")
        };
        return Rotation(values);
    }

    internal JsonObject LedgerHead() => Rotation(new JsonObject
    {
        ["current_full_admin_host_mutation"] = Version,
        ["current_manual_admin_package_pair"] = Pair,
        ["current_descriptor_batch_id"] = Value("pair-consume", "descriptor_batch_id"),
        ["current_manual_admin_evidence"] = DocumentPath("pair-consume")
    });

    private JsonObject Rotation(JsonObject values) => new()
    {
        ["schema_version"] = 1,
        ["contract"] = RotationContract,
        ["previous_tag"] = input.PreviousTag,
        ["next_previous_tag"] = Tag,
        ["values"] = values
    };

    // Only the carry-forward functional evidence exists so far; an actual-host suite needs its own template first.
    private string FunctionalMode() => documents.ContainsKey("functional-carryforward")
        ? "carryforward"
        : throw TrainLane3SpecInput.Invalid("functional-mode-unsupported", input.Facts);

    private string DaysSincePreviousClosure()
    {
        var current = ParseDate(Date, "main-push.date");
        var previousDate = ParseDate(PreviousString("index_sections", "date"), "previous index_sections.date");
        var days = current.DayNumber - previousDate.DayNumber;
        return days >= 0
            ? days.ToString(CultureInfo.InvariantCulture)
            : throw TrainLane3SpecInput.Invalid("date-before-previous-closure", Date);
    }

    private void RequireTrain(string repositoryRoot)
    {
        var train = JsonNode.Parse(ReadRepositoryFile(repositoryRoot, "docs/ga-ready/release-train.json"));
        var record = (train?["trains"] as JsonArray)?
            .OfType<JsonObject>()
            .SingleOrDefault(item => item["version"]?.GetValue<string>() == Version)
            ?? throw TrainLane3SpecInput.Invalid("train-missing", Version);
        var status = record["status"]?.GetValue<string>();
        if (status is not ("running" or "promoted"))
        {
            throw TrainLane3SpecInput.Invalid("train-status", Version + ":" + status);
        }
    }

    private string Value(string template, string key) =>
        Document(template).Values.TryGetValue(key, out var value) && value.Length > 0
            ? value
            : throw TrainLane3SpecInput.Invalid("facts-value-missing", template + "." + key);

    private string DocumentPath(string template) => Document(template).Path;

    private TrainEvidenceDocument Document(string template) =>
        documents.TryGetValue(template, out var document)
            ? document
            : throw TrainLane3SpecInput.Invalid("facts-document-missing", template);

    private string PreviousString(string section, string key) =>
        previous[section]?[key] is JsonValue value && value.GetValueKind() == JsonValueKind.String
            ? value.GetValue<string>()
            : throw TrainLane3SpecInput.Invalid("previous-spec-invalid", section + "." + key);

    private static void Require(bool condition, string subject)
    {
        if (!condition)
        {
            throw TrainLane3SpecInput.Invalid("facts-inconsistent", subject);
        }
    }

    private static DateOnly ParseDate(string text, string subject) =>
        DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw TrainLane3SpecInput.Invalid("date-invalid", subject);

    private static string ReadRepositoryFile(string repositoryRoot, string relativePath)
    {
        var full = Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(full)
            ? File.ReadAllText(full, Encoding.UTF8)
            : throw TrainLane3SpecInput.Invalid("file-missing", relativePath);
    }
}
