using System.Text.RegularExpressions;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

public sealed partial class BatchEvidenceSummaryReader
{
    private readonly string? root;
    private readonly IBatchEvidenceFileAccess fileAccess;

    private const string StatusAvailable = "available";
    private const string StatusDegraded = "degraded";
    private const string StatusMissing = "missing";
    private const string StatusUnavailable = "unavailable";

    private sealed record BatchEvidenceIssue(string Code, string Message, string Detail);

    private sealed record EvidenceJsonReadResult(JsonElement? Json, string Status, BatchEvidenceIssue? Issue)
    {
        public bool HasJson => Json.HasValue;
    }

    private sealed record ArtifactSummary(string Path, JsonElement? Json);

    public BatchEvidenceSummaryReader(string? root)
        : this(root, PhysicalBatchEvidenceFileAccess.Instance)
    {
    }

    internal BatchEvidenceSummaryReader(string? root, IBatchEvidenceFileAccess fileAccess)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);

        this.root = string.IsNullOrWhiteSpace(root) ? null : Path.GetFullPath(root);
        this.fileAccess = fileAccess;
    }

    public JsonElement Read()
    {
        if (root is null)
        {
            return JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["configured"] = false,
                ["status"] = "not_configured",
                ["artifact_root"] = null,
                ["latest"] = null,
                ["errors"] = Array.Empty<object>()
            });
        }

        if (!fileAccess.DirectoryExists(root))
        {
            return WithError(
                "missing",
                "PCV_BATCH_EVIDENCE_ROOT_MISSING",
                "Batch evidence root was configured but does not exist.",
                "The configured batch evidence root is unavailable.");
        }

        if (!IsPathWithinConfiguredRootWithoutReparsePoints(root, root, fileAccess.GetAttributes))
        {
            return WithError(
                "unavailable",
                "PCV_BATCH_EVIDENCE_REPARSE_POINT_REJECTED",
                "Batch evidence root contains a reparse point.",
                "The configured batch evidence root or one of the selected evidence paths is a reparse point and was not read.");
        }

        try
        {
            var runRoot = ResolveLatestRunRoot(root);
            if (runRoot is null)
            {
                return WithError(
                    "missing",
                    "PCV_BATCH_EVIDENCE_SUMMARY_MISSING",
                    "No Batch Supervisor summary.json was found under the configured evidence root.",
                    "The configured batch evidence root does not contain a summary.json file.");
            }

            return BuildAvailableSummary(runRoot);
        }
        catch (JsonException error)
        {
            return WithError(
                "unavailable",
                "PCV_BATCH_EVIDENCE_PARSE_FAILED",
                "Batch evidence JSON could not be parsed.",
                Redact(error.Message));
        }
        catch (IOException error)
        {
            return WithError(
                "unavailable",
                "PCV_BATCH_EVIDENCE_READ_FAILED",
                "Batch evidence could not be read.",
                Redact(error.Message));
        }
        catch (UnauthorizedAccessException error)
        {
            return WithError(
                "unavailable",
                "PCV_BATCH_EVIDENCE_READ_FORBIDDEN",
                "Batch evidence could not be read with the current process identity.",
                Redact(error.Message));
        }
    }

}
