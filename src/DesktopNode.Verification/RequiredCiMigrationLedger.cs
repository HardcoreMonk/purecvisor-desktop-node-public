using System.Text.Json;

namespace DesktopNode.Verification;

internal sealed record RequiredCiMigrationLedgerResult(
    int FileCount,
    int ContractCount,
    int LocalPassCount,
    int CiPassCount,
    int CiPendingCount,
    string? ShadowSha);

internal static class RequiredCiMigrationLedger
{
    internal static RequiredCiMigrationLedgerResult Validate(
        string manifestJson,
        RequiredCiMode mode)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(manifestJson);
        }
        catch (JsonException)
        {
            throw Invalid("required-ci-ledger=json");
        }

        using (document)
        {
            RejectDuplicateProperties(document.RootElement);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !StringEquals(root, "contract", "pcv-development-verification-migration-manifest-v2") ||
                !IntEquals(root, "schema_version", 2))
            {
                throw Invalid("required-ci-ledger=header");
            }

            var entries = RequiredArray(root, "entries", 62);
            var contracts = RequiredArray(root, "contracts", 627);
            var locator = ValidateCutoverLocator(root, mode);
            var expectedParity = mode == RequiredCiMode.Shadow ? "mapped" : "cutover";
            var expectedCi = mode == RequiredCiMode.Shadow ? "pending" : "pass";
            ValidateRows(entries, expectedParity, expectedCi, mode, countContracts: false);
            var counts = ValidateRows(contracts, expectedParity, expectedCi, mode, countContracts: true);

            return new RequiredCiMigrationLedgerResult(
                entries.GetArrayLength(),
                contracts.GetArrayLength(),
                counts.LocalPass,
                counts.CiPass,
                counts.CiPending,
                locator);
        }
    }

    private static string? ValidateCutoverLocator(JsonElement root, RequiredCiMode mode)
    {
        if (mode == RequiredCiMode.Shadow)
        {
            if (root.TryGetProperty("cutover_locator", out _))
            {
                throw Invalid("required-ci-ledger:cutover-locator=unexpected");
            }

            return null;
        }

        if (!root.TryGetProperty("cutover_locator", out var locator) ||
            locator.ValueKind != JsonValueKind.Object ||
            locator.EnumerateObject().Count() != 4 ||
            !StringEquals(locator, "parity_status", "dual-run-pass") ||
            !locator.TryGetProperty("shadow_sha", out var shadow) ||
            shadow.ValueKind != JsonValueKind.String ||
            shadow.GetString() is not { } shadowSha ||
            !System.Text.RegularExpressions.Regex.IsMatch(
                shadowSha,
                "^[0-9a-f]{40}$",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant) ||
            !locator.TryGetProperty("shadow_run_id", out var runId) ||
            runId.ValueKind != JsonValueKind.Number ||
            !runId.TryGetInt64(out var numericRunId) ||
            numericRunId < 1 ||
            !locator.TryGetProperty("shadow_run_url", out var runUrl) ||
            runUrl.ValueKind != JsonValueKind.String ||
            !Uri.TryCreate(runUrl.GetString(), UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.Contains("/actions/runs/", StringComparison.Ordinal))
        {
            throw Invalid("required-ci-ledger:cutover-locator=invalid");
        }

        return shadowSha;
    }

    private static (int LocalPass, int CiPass, int CiPending) ValidateRows(
        JsonElement rows,
        string expectedParity,
        string expectedCi,
        RequiredCiMode mode,
        bool countContracts)
    {
        var localPass = 0;
        var ciPass = 0;
        var ciPending = 0;
        foreach (var item in rows.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object ||
                !StringEquals(item, "parity_status", expectedParity))
            {
                throw Invalid("required-ci-ledger=parity");
            }

            ValidateParity(item, "local_parity", "pass", requireEvidence: true);
            ValidateParity(item, "ci_parity", expectedCi, requireEvidence: mode == RequiredCiMode.Active);
            if (countContracts)
            {
                localPass++;
                if (mode == RequiredCiMode.Active)
                {
                    ciPass++;
                }
                else
                {
                    ciPending++;
                }
            }
        }

        return (localPass, ciPass, ciPending);
    }

    private static JsonElement RequiredArray(JsonElement root, string name, int count)
    {
        if (!root.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.Array ||
            value.GetArrayLength() != count)
        {
            throw Invalid($"required-ci-ledger:{name}=count");
        }

        return value;
    }

    private static void ValidateParity(
        JsonElement item,
        string property,
        string expectedStatus,
        bool requireEvidence)
    {
        if (!item.TryGetProperty(property, out var parity) ||
            parity.ValueKind != JsonValueKind.Object ||
            !StringEquals(parity, "status", expectedStatus) ||
            !parity.TryGetProperty("evidence", out var evidence))
        {
            throw Invalid($"required-ci-ledger:{property}=mismatch");
        }

        if (requireEvidence)
        {
            if (evidence.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(evidence.GetString()))
            {
                throw Invalid($"required-ci-ledger:{property}=evidence");
            }
        }
        else if (evidence.ValueKind != JsonValueKind.Null)
        {
            throw Invalid($"required-ci-ledger:{property}=evidence");
        }
    }

    private static bool StringEquals(JsonElement root, string property, string expected) =>
        root.TryGetProperty(property, out var value) &&
        value.ValueKind == JsonValueKind.String &&
        string.Equals(value.GetString(), expected, StringComparison.Ordinal);

    private static bool IntEquals(JsonElement root, string property, int expected) =>
        root.TryGetProperty(property, out var value) &&
        value.ValueKind == JsonValueKind.Number &&
        value.TryGetInt32(out var actual) &&
        actual == expected;

    private static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    throw Invalid("required-ci-ledger=duplicate-property");
                }

                RejectDuplicateProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in element.EnumerateArray())
            {
                RejectDuplicateProperties(child);
            }
        }
    }

    private static VerificationException Invalid(string detail) =>
        new(VerificationErrorCodes.ConfigInvalid, detail);
}
