using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

internal sealed class CurrentEvidenceException : Exception
{
    internal CurrentEvidenceException(
        string field,
        string detail,
        string code = "PCV_CURRENT_EVIDENCE_INVALID",
        Exception? inner = null)
        : base($"{code}|{field}|{detail}", inner)
    {
    }
}

internal sealed record CurrentEvidenceVerification(
    CurrentEvidenceRecord Record,
    string RenderedBlock,
    IReadOnlyList<CurrentEvidenceTargetResult> Targets);

internal sealed record CurrentEvidenceTargetResult(string Path, string Status);

internal sealed record CurrentEvidenceRecord(
    int SchemaVersion,
    string Contract,
    CurrentOperationalEvidence Current,
    FeatureQualification FeatureQualification,
    ManualAdminEvidence ManualAdmin,
    EvidenceClaims Claims);

internal sealed record CurrentOperationalEvidence(
    string Version,
    IReadOnlyList<string> OperatorSurfaces,
    bool TuiPresent,
    string PackageEvidence,
    string FullgateBatch,
    string FullgateEvidence,
    string FunctionalEvidence,
    string InstalledEvidence,
    string CleanMsiSha256,
    string OperationalMsiSha256,
    string PayloadSha256,
    string ProvenanceCommit);

internal sealed record FeatureQualification(
    int SchemaVersion,
    string Contract,
    bool PromotionEligible,
    IReadOnlyList<FeatureQualificationBlocker> Blockers);

internal sealed record FeatureQualificationBlocker(
    string FeatureId,
    string Stage,
    string Verdict);

internal sealed record ManualAdminEvidence(
    string LatestClosedBaseline,
    string LatestClosedTarget,
    string LatestClosedDescriptor,
    string? BlockedBaseline,
    string? BlockedTarget,
    string? BlockedReason);

internal sealed record EvidenceClaims(
    bool PublicTrustedSigning,
    bool ExternalStablePublication);

internal static class CurrentEvidenceVerifier
{
    internal const string RecordPath = "docs/ga-ready/current-evidence.json";
    internal const string SchemaPath = "docs/ga-ready/current-evidence.schema.json";

    private const string BeginMarker = "<!-- BEGIN GENERATED CURRENT EVIDENCE -->";
    private const string EndMarker = "<!-- END GENERATED CURRENT EVIDENCE -->";

    private static readonly UTF8Encoding StrictUtf8 =
        new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private static readonly Regex VersionPattern =
        new("^0\\.\\d+\\.\\d+-admin-smoke$", RegexOptions.CultureInvariant);

    private static readonly Regex Sha256Pattern =
        new("^[0-9a-f]{64}$", RegexOptions.CultureInvariant);

    private static readonly Regex CommitPattern =
        new("^[0-9a-f]{40}$", RegexOptions.CultureInvariant);

    private static readonly Regex FeatureIdPattern =
        new("^pcv\\.[a-z0-9._-]+$", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> FeatureStages = new(StringComparer.Ordinal)
    {
        "code_tested",
        "packaged",
        "installed_tested",
        "actual_vm_tested",
        "manual_admin_tested",
    };

    private static readonly HashSet<string> BlockerVerdicts = new(StringComparer.Ordinal)
    {
        "fail",
        "blocked",
        "missing",
    };

    internal static IReadOnlyList<string> OwnedRelativePaths { get; } = Array.AsReadOnly([
        "README.md",
        "AGENTS.md",
        "docs/DEVELOPER_INDEX.md",
        "docs/ga-ready/EVIDENCE_INDEX.md",
        "docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md",
        "docs/ga-ready/CONTROL_PLANE_INDEX.md",
        "docs/DEVELOPMENT_VERIFICATION_POLICY.md",
        "packaging/windows-desktop-node/README.md",
    ]);

    internal static CurrentEvidenceVerification Verify(
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var root = ResolveRoot(repositoryRoot);
        _ = ReadRelative(root, SchemaPath, "schema", cancellationToken);
        var record = ValidateJson(
            ReadRelative(root, RecordPath, "root", cancellationToken),
            root,
            cancellationToken);
        var block = Render(record);
        var targets = new List<CurrentEvidenceTargetResult>(OwnedRelativePaths.Count);

        foreach (var relativePath in OwnedRelativePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = ReadRelative(root, relativePath, relativePath, cancellationToken);
            VerifyDocument(relativePath, text, block);
            targets.Add(new CurrentEvidenceTargetResult(relativePath, "current"));
        }

        return new CurrentEvidenceVerification(
            record,
            block,
            Array.AsReadOnly(targets.ToArray()));
    }

    internal static CurrentEvidenceRecord ValidateJson(
        string json,
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rootPath = ResolveRoot(repositoryRoot);
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 64,
                });
        }
        catch (JsonException error)
        {
            throw Invalid("root", "malformed-json", error);
        }

        using (document)
        {
            RejectDuplicateProperties(document.RootElement, "root");
            var root = document.RootElement;
            RequireExactObject(
                root,
                "root",
                "schema_version",
                "contract",
                "current",
                "feature_qualification",
                "manual_admin",
                "claims");

            var schemaVersion = RequireInteger(root, "schema_version", "schema_version");
            if (schemaVersion != 1)
            {
                throw Invalid("schema_version", schemaVersion.ToString());
            }

            var contract = RequireString(root, "contract", "contract");
            if (contract != "pcv-current-evidence-v1")
            {
                throw Invalid("contract", contract);
            }

            var current = ParseCurrent(
                RequireObject(root, "current", "current"),
                rootPath,
                cancellationToken);
            var qualification = ParseQualification(
                RequireObject(root, "feature_qualification", "feature_qualification"));
            var manualAdmin = ParseManualAdmin(
                RequireObject(root, "manual_admin", "manual_admin"));
            var claims = ParseClaims(
                RequireObject(root, "claims", "claims"));

            return new CurrentEvidenceRecord(
                schemaVersion,
                contract,
                current,
                qualification,
                manualAdmin,
                claims);
        }
    }

    internal static string Render(CurrentEvidenceRecord record)
    {
        var blockers = record.FeatureQualification.Blockers.Count == 0
            ? "none"
            : string.Join(
                ',',
                record.FeatureQualification.Blockers.Select(
                    blocker => $"{blocker.FeatureId}/{blocker.Stage}/{blocker.Verdict}"));

        var lines = new List<string>
        {
            BeginMarker,
            "## Current operational evidence (generated)",
            string.Empty,
            $"- Version: `{record.Current.Version}`",
            "- Active operator surfaces: Web Console and PCVCLI; " +
                $"`tui_present={Lower(record.Current.TuiPresent)}`.",
            $"- Package evidence: `{record.Current.PackageEvidence}`.",
            "- Full admin host mutation: " +
                $"`{record.Current.FullgateBatch}` / `{record.Current.FullgateEvidence}`.",
            $"- Actual-VM functional evidence: `{record.Current.FunctionalEvidence}`.",
            "- Feature qualification: " +
                $"`contract={record.FeatureQualification.Contract}`; " +
                $"`promotion_eligible={Lower(record.FeatureQualification.PromotionEligible)}`; " +
                $"`blocker_count={record.FeatureQualification.Blockers.Count}`; " +
                $"`blockers={blockers}`.",
            "- Installed CLI/Web current-card: " +
                $"`{record.Current.InstalledEvidence}`; CLI exit 0, Web HTTP 200, " +
                "service Running/Automatic, TUI absent.",
            $"- Clean MSI SHA-256: `{record.Current.CleanMsiSha256}`.",
            $"- Operational MSI SHA-256: `{record.Current.OperationalMsiSha256}`.",
            "- Operational payload aggregate SHA-256: " +
                $"`{record.Current.PayloadSha256}`.",
            $"- Provenance commit: `{record.Current.ProvenanceCommit}`.",
            "- Latest closed manual-admin pair: " +
                $"`{record.ManualAdmin.LatestClosedBaseline} -> " +
                $"{record.ManualAdmin.LatestClosedTarget}` / " +
                $"`{record.ManualAdmin.LatestClosedDescriptor}`.",
        };

        if (record.ManualAdmin.BlockedReason is not null)
        {
            lines.Add(
                "- Blocked follow-up: " +
                $"`{record.ManualAdmin.BlockedBaseline} -> {record.ManualAdmin.BlockedTarget}` / " +
                $"`{record.ManualAdmin.BlockedReason}`.");
        }

        lines.Add(
            "- Claims: " +
            $"`public_trusted_signing={Lower(record.Claims.PublicTrustedSigning)}`; " +
            $"`external_stable_publication={Lower(record.Claims.ExternalStablePublication)}`.");
        lines.Add(EndMarker);
        return string.Join('\n', lines);
    }

    internal static void AssertPromotionAllowed(
        CurrentEvidenceRecord proposed,
        CurrentEvidenceRecord canonical)
    {
        if (!proposed.Current.Version.Equals(
                canonical.Current.Version,
                StringComparison.Ordinal) &&
            !proposed.FeatureQualification.PromotionEligible)
        {
            throw new CurrentEvidenceException(
                proposed.Current.Version,
                $"blockers={proposed.FeatureQualification.Blockers.Count}",
                "PCV_FEATURE_PROMOTION_BLOCKED");
        }
    }

    internal static void VerifyDocument(string owner, string content, string expectedBlock)
    {
        var normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal);
        var block = expectedBlock.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');
        if (Count(normalized, BeginMarker) != 1 || Count(normalized, EndMarker) != 1)
        {
            throw Invalid(owner, "marker-cardinality");
        }

        var begin = normalized.IndexOf(BeginMarker, StringComparison.Ordinal);
        var end = normalized.IndexOf(EndMarker, StringComparison.Ordinal);
        if (begin < 0 || end <= begin)
        {
            throw Invalid(owner, "marker-order");
        }

        var observed = normalized.Substring(begin, end + EndMarker.Length - begin);
        if (!observed.Equals(block, StringComparison.Ordinal))
        {
            throw new CurrentEvidenceException(
                owner,
                "generated-block",
                "PCV_CURRENT_EVIDENCE_STALE");
        }
    }

    private static CurrentOperationalEvidence ParseCurrent(
        JsonElement current,
        string repositoryRoot,
        CancellationToken cancellationToken)
    {
        RequireExactObject(
            current,
            "current",
            "version",
            "operator_surfaces",
            "tui_present",
            "package_evidence",
            "fullgate_batch",
            "fullgate_evidence",
            "functional_evidence",
            "installed_evidence",
            "clean_msi_sha256",
            "operational_msi_sha256",
            "payload_sha256",
            "provenance_commit");

        var version = RequireString(current, "version", "current.version");
        RequirePattern(version, VersionPattern, "current.version", version);

        var surfacesElement = RequireArray(
            current,
            "operator_surfaces",
            "current.operator_surfaces");
        var surfaces = surfacesElement.EnumerateArray()
            .Select((item, index) =>
                RequireStringValue(item, $"current.operator_surfaces[{index}]"))
            .ToArray();
        if (!surfaces.SequenceEqual(["web", "cli"], StringComparer.Ordinal))
        {
            throw Invalid("current.operator_surfaces", string.Join(',', surfaces));
        }

        var tui = RequireBoolean(current, "tui_present", "current.tui_present");
        if (tui)
        {
            throw Invalid("current.tui_present", "must-be-false");
        }

        var packageEvidence = RequireEvidencePath(current, "package_evidence");
        var fullgateBatch = RequireString(current, "fullgate_batch", "current.fullgate_batch");
        if (string.IsNullOrEmpty(fullgateBatch))
        {
            throw Invalid("current.fullgate_batch", "empty");
        }

        var fullgateEvidence = RequireEvidencePath(current, "fullgate_evidence");
        var functionalEvidence = RequireEvidencePath(current, "functional_evidence");
        var installedEvidence = RequireEvidencePath(current, "installed_evidence");
        var cleanMsi = RequireString(current, "clean_msi_sha256", "current.clean_msi_sha256");
        var operationalMsi = RequireString(
            current,
            "operational_msi_sha256",
            "current.operational_msi_sha256");
        var payload = RequireString(current, "payload_sha256", "current.payload_sha256");
        RequirePattern(cleanMsi, Sha256Pattern, "current.clean_msi_sha256", "invalid-sha256");
        RequirePattern(
            operationalMsi,
            Sha256Pattern,
            "current.operational_msi_sha256",
            "invalid-sha256");
        RequirePattern(payload, Sha256Pattern, "current.payload_sha256", "invalid-sha256");

        var commit = RequireString(
            current,
            "provenance_commit",
            "current.provenance_commit");
        RequirePattern(commit, CommitPattern, "current.provenance_commit", "invalid-commit");

        foreach (var (field, path) in new[]
        {
            ("package_evidence", packageEvidence),
            ("fullgate_evidence", fullgateEvidence),
            ("functional_evidence", functionalEvidence),
            ("installed_evidence", installedEvidence),
        })
        {
            cancellationToken.ThrowIfCancellationRequested();
            string evidence;
            try
            {
                evidence = ReadRelative(
                    repositoryRoot,
                    path,
                    $"current.{field}",
                    cancellationToken);
            }
            catch (CurrentEvidenceException error)
                when (error.Message.StartsWith(
                    $"PCV_CURRENT_EVIDENCE_INVALID|current.{field}|",
                    StringComparison.Ordinal))
            {
                throw Invalid($"current.{field}", $"missing-reference:{path}", error);
            }

            if (!Regex.IsMatch(
                evidence,
                Regex.Escape(version),
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                throw Invalid($"current.{field}", $"version-mismatch:{path}");
            }
        }

        return new CurrentOperationalEvidence(
            version,
            Array.AsReadOnly(surfaces),
            tui,
            packageEvidence,
            fullgateBatch,
            fullgateEvidence,
            functionalEvidence,
            installedEvidence,
            cleanMsi,
            operationalMsi,
            payload,
            commit);
    }

    private static FeatureQualification ParseQualification(JsonElement qualification)
    {
        RequireExactObject(
            qualification,
            "feature_qualification",
            "schema_version",
            "contract",
            "promotion_eligible",
            "blockers");
        var schemaVersion = RequireInteger(
            qualification,
            "schema_version",
            "feature_qualification.schema_version");
        if (schemaVersion != 1)
        {
            throw Invalid(
                "feature_qualification.schema_version",
                schemaVersion.ToString());
        }

        var contract = RequireString(
            qualification,
            "contract",
            "feature_qualification.contract");
        if (contract != "pcv-feature-promotion-decision-v1")
        {
            throw Invalid("feature_qualification.contract", contract);
        }

        var eligible = RequireBoolean(
            qualification,
            "promotion_eligible",
            "feature_qualification.promotion_eligible");
        var blockersElement = RequireArray(
            qualification,
            "blockers",
            "feature_qualification.blockers");
        var blockers = new List<FeatureQualificationBlocker>();
        foreach (var blocker in blockersElement.EnumerateArray())
        {
            RequireExactObject(
                blocker,
                "feature_qualification.blockers",
                "feature_id",
                "stage",
                "verdict");
            var featureId = RequireString(
                blocker,
                "feature_id",
                "feature_qualification.blockers.feature_id");
            var stage = RequireString(
                blocker,
                "stage",
                "feature_qualification.blockers.stage");
            var verdict = RequireString(
                blocker,
                "verdict",
                "feature_qualification.blockers.verdict");
            RequirePattern(
                featureId,
                FeatureIdPattern,
                "feature_qualification.blockers.feature_id",
                featureId);
            if (!FeatureStages.Contains(stage))
            {
                throw Invalid("feature_qualification.blockers.stage", stage);
            }

            if (!BlockerVerdicts.Contains(verdict))
            {
                throw Invalid("feature_qualification.blockers.verdict", verdict);
            }

            blockers.Add(new FeatureQualificationBlocker(featureId, stage, verdict));
        }

        if (eligible && blockers.Count != 0)
        {
            throw Invalid(
                "feature_qualification.blockers",
                "eligible-must-be-empty");
        }

        if (!eligible && blockers.Count == 0)
        {
            throw Invalid(
                "feature_qualification.blockers",
                "blocked-must-not-be-empty");
        }

        return new FeatureQualification(
            schemaVersion,
            contract,
            eligible,
            Array.AsReadOnly(blockers.ToArray()));
    }

    private static ManualAdminEvidence ParseManualAdmin(JsonElement manual)
    {
        RequireAllowedObject(
            manual,
            "manual_admin",
            ["latest_closed_baseline", "latest_closed_target", "latest_closed_descriptor"],
            ["blocked_baseline", "blocked_target", "blocked_reason"]);
        var baseline = RequireNonEmptyString(
            manual,
            "latest_closed_baseline",
            "manual_admin.latest_closed_baseline");
        var target = RequireNonEmptyString(
            manual,
            "latest_closed_target",
            "manual_admin.latest_closed_target");
        var descriptor = RequireNonEmptyString(
            manual,
            "latest_closed_descriptor",
            "manual_admin.latest_closed_descriptor");

        var optional = new[] { "blocked_baseline", "blocked_target", "blocked_reason" };
        var present = optional.Where(name => manual.TryGetProperty(name, out _)).ToArray();
        if (present.Length is not (0 or 3))
        {
            throw Invalid("manual_admin.blocked_*", "partial-blocked-triple");
        }

        string? blockedBaseline = null;
        string? blockedTarget = null;
        string? blockedReason = null;
        if (present.Length == 3)
        {
            blockedBaseline = RequireNonEmptyString(
                manual,
                "blocked_baseline",
                "manual_admin.blocked_baseline");
            blockedTarget = RequireNonEmptyString(
                manual,
                "blocked_target",
                "manual_admin.blocked_target");
            blockedReason = RequireNonEmptyString(
                manual,
                "blocked_reason",
                "manual_admin.blocked_reason");
        }

        return new ManualAdminEvidence(
            baseline,
            target,
            descriptor,
            blockedBaseline,
            blockedTarget,
            blockedReason);
    }

    private static EvidenceClaims ParseClaims(JsonElement claims)
    {
        RequireExactObject(
            claims,
            "claims",
            "public_trusted_signing",
            "external_stable_publication");
        var signing = RequireBoolean(
            claims,
            "public_trusted_signing",
            "claims.public_trusted_signing");
        var publication = RequireBoolean(
            claims,
            "external_stable_publication",
            "claims.external_stable_publication");
        if (signing)
        {
            throw Invalid("claims.public_trusted_signing", "must-be-false");
        }

        if (publication)
        {
            throw Invalid("claims.external_stable_publication", "must-be-false");
        }

        return new EvidenceClaims(signing, publication);
    }

    private static string RequireEvidencePath(JsonElement current, string name)
    {
        var field = $"current.{name}";
        var path = RequireString(current, name, field);
        if (!path.StartsWith("docs/ga-ready/evidence/", StringComparison.Ordinal) ||
            !path.EndsWith(".md", StringComparison.Ordinal) ||
            !IsRelativeRepositoryPath(path))
        {
            throw Invalid(field, "invalid-path");
        }

        return path;
    }

    private static string RequireNonEmptyString(
        JsonElement owner,
        string name,
        string field)
    {
        var value = RequireString(owner, name, field);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw Invalid(field, "empty");
    }

    private static void RequirePattern(
        string value,
        Regex pattern,
        string field,
        string detail)
    {
        if (!pattern.IsMatch(value))
        {
            throw Invalid(field, detail);
        }
    }

    private static JsonElement RequireObject(
        JsonElement owner,
        string name,
        string field) =>
        RequireProperty(owner, name, field, JsonValueKind.Object);

    private static JsonElement RequireArray(
        JsonElement owner,
        string name,
        string field) =>
        RequireProperty(owner, name, field, JsonValueKind.Array);

    private static string RequireString(
        JsonElement owner,
        string name,
        string field) =>
        RequireStringValue(
            RequireProperty(owner, name, field, JsonValueKind.String),
            field);

    private static string RequireStringValue(JsonElement value, string field) =>
        value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? throw Invalid(field, "null")
            : throw Invalid(field, "must-be-string");

    private static int RequireInteger(
        JsonElement owner,
        string name,
        string field)
    {
        var value = RequireProperty(owner, name, field, JsonValueKind.Number);
        return value.TryGetInt32(out var integer)
            ? integer
            : throw Invalid(field, "must-be-integer");
    }

    private static bool RequireBoolean(
        JsonElement owner,
        string name,
        string field)
    {
        var value = GetProperty(owner, name, field);
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw Invalid(field, "must-be-boolean"),
        };
    }

    private static JsonElement RequireProperty(
        JsonElement owner,
        string name,
        string field,
        JsonValueKind kind)
    {
        var value = GetProperty(owner, name, field);
        return value.ValueKind == kind
            ? value
            : throw Invalid(field, $"must-be-{kind.ToString().ToLowerInvariant()}");
    }

    private static JsonElement GetProperty(
        JsonElement owner,
        string name,
        string field)
    {
        if (owner.ValueKind != JsonValueKind.Object ||
            !owner.TryGetProperty(name, out var value))
        {
            throw Invalid(field, "missing");
        }

        return value;
    }

    private static void RequireExactObject(
        JsonElement value,
        string field,
        params string[] names) =>
        RequireAllowedObject(value, field, names, []);

    private static void RequireAllowedObject(
        JsonElement value,
        string field,
        IReadOnlyList<string> required,
        IReadOnlyList<string> optional)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(field, "must-be-object");
        }

        var allowed = required.Concat(optional).ToHashSet(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                throw Invalid($"{field}.{property.Name}", "unexpected");
            }
        }

        foreach (var name in required)
        {
            if (!value.TryGetProperty(name, out _))
            {
                throw Invalid(
                    field == "root" ? name : $"{field}.{name}",
                    "missing");
            }
        }
    }

    private static void RejectDuplicateProperties(JsonElement value, string field)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    throw Invalid(
                        field == "root" ? property.Name : $"{field}.{property.Name}",
                        "duplicate");
                }

                RejectDuplicateProperties(
                    property.Value,
                    field == "root" ? property.Name : $"{field}.{property.Name}");
            }

            return;
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in value.EnumerateArray())
            {
                RejectDuplicateProperties(item, $"{field}[{index++}]");
            }
        }
    }

    private static string ResolveRoot(string repositoryRoot)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(repositoryRoot) ||
                !Path.IsPathFullyQualified(repositoryRoot))
            {
                throw Invalid("repository_root", "invalid");
            }

            var root = Path.GetFullPath(repositoryRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Directory.Exists(root))
            {
                throw Invalid("repository_root", "missing");
            }

            return root;
        }
        catch (CurrentEvidenceException)
        {
            throw;
        }
        catch (Exception error) when (error is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw Invalid("repository_root", "invalid", error);
        }
    }

    private static string ReadRelative(
        string repositoryRoot,
        string relativePath,
        string field,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsRelativeRepositoryPath(relativePath))
        {
            throw Invalid(field, "invalid-path");
        }

        var root = ResolveRoot(repositoryRoot);
        var cursor = root;
        foreach (var segment in relativePath.Split('/'))
        {
            cancellationToken.ThrowIfCancellationRequested();
            cursor = Path.Combine(cursor, segment);
            if (!File.Exists(cursor) && !Directory.Exists(cursor))
            {
                throw Invalid(field, "missing");
            }

            if ((File.GetAttributes(cursor) & FileAttributes.ReparsePoint) != 0)
            {
                throw Invalid(field, "reparse-point");
            }
        }

        if (!File.Exists(cursor))
        {
            throw Invalid(field, "not-file");
        }

        try
        {
            return File.ReadAllText(cursor, StrictUtf8);
        }
        catch (DecoderFallbackException error)
        {
            throw Invalid(field, "utf8", error);
        }
        catch (IOException error)
        {
            throw Invalid(field, "read", error);
        }
    }

    private static bool IsRelativeRepositoryPath(string path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !Path.IsPathRooted(path) &&
        !path.Contains('\\') &&
        !path.Contains('\0') &&
        path.Split('/').All(segment => segment is not ("" or "." or ".."));

    private static int Count(string value, string needle)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(needle, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += needle.Length;
        }

        return count;
    }

    private static string Lower(bool value) => value ? "true" : "false";

    private static CurrentEvidenceException Invalid(
        string field,
        string detail,
        Exception? inner = null) =>
        new(field, detail, inner: inner);
}
