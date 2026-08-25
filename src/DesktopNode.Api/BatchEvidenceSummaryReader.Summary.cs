using System.Text.RegularExpressions;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

public sealed partial class BatchEvidenceSummaryReader
{
    private JsonElement BuildAvailableSummary(string runRoot)
    {
        var runSummaryPath = Path.Combine(runRoot, "summary.json");
        if (!IsReadableEvidencePath(runSummaryPath))
        {
            return WithError(
                "unavailable",
                "PCV_BATCH_EVIDENCE_REPARSE_POINT_REJECTED",
                "Batch evidence summary path contains a reparse point.",
                "The selected Batch Supervisor summary path contains a reparse point and was not read.");
        }

        var issues = new List<BatchEvidenceIssue>();
        using var batchDocument = JsonDocument.Parse(fileAccess.ReadAllText(runSummaryPath));
        var batch = batchDocument.RootElement;
        var routeRoot = ResolveChildArtifactRoot(batch, "service-msi-hyperv-admin-smoke");
        var osRoot = ResolveChildArtifactRoot(batch, "os-mutation-gate");
        var routeSummary = ReadChildJson(
            routeRoot,
            "summary.json",
            "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_MISSING",
            "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_PARSE_FAILED",
            "route_msi_hyperv summary");
        var osSummary = ReadChildJson(
            osRoot,
            "summary.json",
            "PCV_BATCH_EVIDENCE_OS_SUMMARY_MISSING",
            "PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED",
            "os_mutation summary");
        var provenance = ReadFirstChildJson(
            routeRoot,
            "*.provenance.json",
            "PCV_BATCH_EVIDENCE_PROVENANCE_MISSING",
            "PCV_BATCH_EVIDENCE_PROVENANCE_PARSE_FAILED",
            "msi provenance");
        var msiLifecycle = ReadChildJson(
            routeRoot,
            "msi-lifecycle-smoke.json",
            "PCV_BATCH_EVIDENCE_MSI_LIFECYCLE_MISSING",
            "PCV_BATCH_EVIDENCE_MSI_LIFECYCLE_PARSE_FAILED",
            "msi lifecycle");
        AddIssue(issues, routeSummary);
        AddIssue(issues, osSummary);
        AddIssue(issues, provenance);
        AddIssue(issues, msiLifecycle);

        var gpuSnapshots = BuildGpuSnapshotSummary(Path.Combine(runRoot, "gpu-snapshots.jsonl"), issues);
        var status = issues.Count == 0 ? StatusAvailable : StatusDegraded;

        return JsonFromObject(new SortedDictionary<string, object?>
        {
            ["schema_version"] = 1,
            ["configured"] = true,
            ["status"] = status,
            ["artifact_root"] = Redact(root),
            ["latest"] = new SortedDictionary<string, object?>
            {
                ["batch_id"] = ReadString(batch, "batch_id"),
                ["ok"] = ReadBool(batch, "ok"),
                ["status"] = ReadString(batch, "status"),
                ["total_steps"] = ReadInt(batch, "total_steps"),
                ["executed_steps"] = ReadInt(batch, "executed_steps"),
                ["steps"] = BuildStepSummaries(batch),
                ["gpu_snapshots"] = gpuSnapshots,
                ["release"] = BuildReleaseSummary(routeSummary, osSummary, provenance),
                ["route_msi_hyperv"] = BuildRouteSummary(routeSummary, msiLifecycle),
                ["os_mutation"] = BuildOsSummary(osSummary),
                ["host_final_state"] = BuildHostFinalState(routeSummary.Json, osSummary.Json)
            },
            ["manual_admin"] = BuildManualAdminArtifactSummary(),
            ["public_boundary"] = BuildPublicBoundaryArtifactSummary(),
            ["errors"] = BuildIssueObjects(issues)
        });
    }

    private object[] BuildStepSummaries(JsonElement batch)
    {
        if (!batch.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return results.EnumerateArray()
            .Select(result => new SortedDictionary<string, object?>
            {
                ["step_id"] = ReadString(result, "step_id"),
                ["ok"] = ReadBool(result, "ok"),
                ["exit_code"] = ReadInt(result, "exit_code"),
                ["timed_out"] = ReadBool(result, "timed_out"),
                ["retry_count"] = ReadInt(result, "retry_count"),
                ["attempt_count"] = ReadInt(result, "attempt_count"),
                ["final_attempt"] = ReadInt(result, "final_attempt"),
                ["duration_ms"] = ReadInt(result, "duration_ms")
            })
            .ToArray();
    }

    private SortedDictionary<string, object?> BuildGpuSnapshotSummary(string path, List<BatchEvidenceIssue> issues)
    {
        var fullPath = Path.GetFullPath(path);
        if (!fileAccess.FileExists(fullPath))
        {
            issues.Add(new BatchEvidenceIssue(
                "PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_MISSING",
                "GPU snapshot evidence is missing.",
                $"GPU snapshot evidence is missing at {Redact(fullPath)}."));
            return new SortedDictionary<string, object?>
            {
                ["status"] = StatusMissing,
                ["present"] = false,
                ["count"] = 0,
                ["status_counts"] = new SortedDictionary<string, int>(),
                ["peak_adapter_mib"] = null,
                ["peak_process_mib"] = null
            };
        }

        var count = 0;
        decimal? peakAdapterMib = null;
        decimal? peakProcessMib = null;
        var statusCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);

        if (!IsReadableEvidencePath(fullPath))
        {
            issues.Add(new BatchEvidenceIssue(
                "PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED",
                "GPU snapshot evidence could not be read.",
                $"GPU snapshot evidence path was rejected at {Redact(fullPath)}."));
            return GpuSnapshotResult(StatusUnavailable, present: false, count, statusCounts, peakAdapterMib, peakProcessMib);
        }

        try
        {
            foreach (var line in fileAccess.ReadLines(fullPath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                try
                {
                    using var document = JsonDocument.Parse(line);
                    var snapshot = document.RootElement;
                    count++;
                    var status = ReadString(snapshot, "status") ?? "unknown";
                    statusCounts[status] = statusCounts.TryGetValue(status, out var current) ? current + 1 : 1;
                    peakAdapterMib = Max(peakAdapterMib, ReadPeakMib(snapshot, "adapter_memory"));
                    peakProcessMib = Max(peakProcessMib, ReadPeakMib(snapshot, "process_memory"));
                }
                catch (JsonException error)
                {
                    issues.Add(new BatchEvidenceIssue(
                        "PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED",
                        "GPU snapshot evidence JSON could not be parsed.",
                        Redact(error.Message)));
                    return GpuSnapshotResult(StatusUnavailable, present: true, count, statusCounts, peakAdapterMib, peakProcessMib);
                }
            }
        }
        catch (IOException error)
        {
            issues.Add(new BatchEvidenceIssue(
                "PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED",
                "GPU snapshot evidence could not be read.",
                Redact(error.Message)));
            return GpuSnapshotResult(StatusUnavailable, present: true, count, statusCounts, peakAdapterMib, peakProcessMib);
        }
        catch (UnauthorizedAccessException error)
        {
            issues.Add(new BatchEvidenceIssue(
                "PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED",
                "GPU snapshot evidence could not be read.",
                Redact(error.Message)));
            return GpuSnapshotResult(StatusUnavailable, present: true, count, statusCounts, peakAdapterMib, peakProcessMib);
        }

        return GpuSnapshotResult(StatusAvailable, present: true, count, statusCounts, peakAdapterMib, peakProcessMib);
    }

    private static SortedDictionary<string, object?> GpuSnapshotResult(
        string status,
        bool present,
        int count,
        SortedDictionary<string, int> statusCounts,
        decimal? peakAdapterMib,
        decimal? peakProcessMib)
    {
        return new SortedDictionary<string, object?>
        {
            ["status"] = status,
            ["present"] = present,
            ["count"] = count,
            ["status_counts"] = statusCounts,
            ["peak_adapter_mib"] = peakAdapterMib,
            ["peak_process_mib"] = peakProcessMib
        };
    }

    private SortedDictionary<string, object?> BuildReleaseSummary(
        EvidenceJsonReadResult routeSummary,
        EvidenceJsonReadResult osSummary,
        EvidenceJsonReadResult provenance)
    {
        return new SortedDictionary<string, object?>
        {
            ["status"] = ReleaseStatus(routeSummary, osSummary, provenance),
            ["version"] = ReadString(provenance.Json, "product", "version") ?? ReadString(routeSummary.Json, "version") ?? ReadString(osSummary.Json, "version"),
            ["git_commit"] = ReadString(provenance.Json, "git_commit"),
            ["msi_sha256"] = ReadString(provenance.Json, "msi", "sha256"),
            ["signing_mode"] = ReadString(provenance.Json, "signing_mode"),
            ["public_trusted_signing"] = ReadString(osSummary.Json, "public_trusted_signing") ?? "excluded",
            ["external_stable_publication"] = ReadString(osSummary.Json, "external_stable_publication") ?? "not-claimed"
        };
    }

    private static SortedDictionary<string, object?> BuildRouteSummary(EvidenceJsonReadResult routeSummary, EvidenceJsonReadResult msiLifecycle)
    {
        return new SortedDictionary<string, object?>
        {
            ["status"] = CombineAreaStatus(routeSummary.Status, msiLifecycle.Status),
            ["ok"] = ReadBool(routeSummary.Json, "ok"),
            ["version"] = ReadString(routeSummary.Json, "version"),
            ["boot_time_unchanged"] = ReadBool(routeSummary.Json, "boot_time_unchanged"),
            ["msi_lifecycle_ok"] = ReadBool(msiLifecycle.Json, "ok"),
            ["msi_lifecycle_step_count"] = ReadArrayLength(msiLifecycle.Json, "steps")
        };
    }

    private static SortedDictionary<string, object?> BuildOsSummary(EvidenceJsonReadResult osSummary)
    {
        return new SortedDictionary<string, object?>
        {
            ["status"] = osSummary.Status,
            ["ok"] = ReadBool(osSummary.Json, "ok"),
            ["version"] = ReadString(osSummary.Json, "version"),
            ["boot_time_unchanged"] = ReadBool(osSummary.Json, "boot_time_unchanged"),
            ["firewall_rule_count"] = ReadInt(osSummary.Json, "final_firewall_rule_count"),
            ["eventlog_source_present"] = ReadBool(osSummary.Json, "final_eventlog_source_present")
        };
    }

    private static string ReleaseStatus(
        EvidenceJsonReadResult routeSummary,
        EvidenceJsonReadResult osSummary,
        EvidenceJsonReadResult provenance)
    {
        if (provenance.Status == StatusUnavailable)
        {
            return StatusUnavailable;
        }

        return provenance.Status == StatusAvailable || routeSummary.Status == StatusAvailable || osSummary.Status == StatusAvailable
            ? provenance.Status == StatusAvailable ? StatusAvailable : StatusDegraded
            : StatusMissing;
    }

    private static string CombineAreaStatus(string primaryStatus, string secondaryStatus)
    {
        if (primaryStatus is StatusUnavailable or StatusMissing)
        {
            return primaryStatus;
        }

        return secondaryStatus == StatusAvailable ? StatusAvailable : StatusDegraded;
    }

    private static SortedDictionary<string, object?> BuildHostFinalState(JsonElement? routeSummary, JsonElement? osSummary)
    {
        var serviceState = ReadString(osSummary, "final_service", "state") ??
            ReadString(osSummary, "final_service", "State") ??
            ReadString(routeSummary, "final_service", "state") ??
            ReadString(routeSummary, "final_service", "State");
        var trustStore = ReadElement(osSummary, "final_trust_store");

        return new SortedDictionary<string, object?>
        {
            ["service_state"] = serviceState,
            ["firewall_rule_count"] = ReadInt(osSummary, "final_firewall_rule_count"),
            ["eventlog_source_present"] = ReadBool(osSummary, "final_eventlog_source_present"),
            ["trust_root_present"] = ReadBool(trustStore, "root_present"),
            ["trust_publisher_present"] = ReadBool(trustStore, "publisher_present"),
            ["boot_time_unchanged"] = ReadBool(osSummary, "boot_time_unchanged") ?? ReadBool(routeSummary, "boot_time_unchanged")
        };
    }

    private SortedDictionary<string, object?> BuildManualAdminArtifactSummary()
    {
        var latest = EnumerateSummaryFiles()
            .Select(path => new ArtifactSummary(path, TryReadArtifactSummary(path)))
            .Where(candidate => IsManualAdminPackagePairSummary(candidate.Json))
            .OrderByDescending(candidate => GetManualAdminTargetVersionSortKey(candidate.Json))
            .ThenByDescending(candidate => GetManualAdminDescriptorIdSortKey(candidate.Path, candidate.Json))
            .ThenByDescending(candidate => GetEvidenceSummarySortTime(Path.GetDirectoryName(candidate.Path)!))
            .FirstOrDefault();

        return new SortedDictionary<string, object?>
        {
            ["latest_package_pair"] = latest is not null && latest.Json is not null
                ? BuildManualAdminPackagePairSummary(latest.Path, latest.Json.Value)
                : null
        };
    }

    private SortedDictionary<string, object?> BuildPublicBoundaryArtifactSummary()
    {
        var latest = EnumerateSummaryFiles()
            .Select(path => new ArtifactSummary(path, TryReadArtifactSummary(path)))
            .Where(candidate => IsPublicBoundaryMainPushSummary(candidate.Json))
            .OrderByDescending(candidate => GetEvidenceSummarySortTime(Path.GetDirectoryName(candidate.Path)!))
            .FirstOrDefault();

        return new SortedDictionary<string, object?>
        {
            ["latest_main_push"] = latest is not null && latest.Json is not null
                ? BuildPublicBoundaryMainPushSummary(latest.Path, latest.Json.Value)
                : null
        };
    }

    private SortedDictionary<string, object?> BuildManualAdminPackagePairSummary(string path, JsonElement summary)
    {
        var baselineVersion = ReadString(summary, "baseline_version");
        var targetVersion = ReadString(summary, "target_version");
        var packagePair = ReadString(summary, "package_pair") ??
            (baselineVersion is not null && targetVersion is not null ? $"{baselineVersion} -> {targetVersion}" : null);
        var descriptorSummary = ToArtifactEvidencePath(path);
        var descriptorGenerationContract = ReadString(summary, "manual_admin_descriptor_generation_contract") ??
            ReadString(summary, "descriptor_generation_contract");
        var descriptorBatchId = ReadString(summary, "descriptor_batch_id") ??
            ReadString(summary, "batch_id") ??
            InferDescriptorBatchId(path) ??
            InferDescriptorBatchIdFromCampaignPath(path);

        return new SortedDictionary<string, object?>
        {
            ["baseline_version"] = baselineVersion,
            ["current_card_descriptor_batch_id"] = descriptorBatchId,
            ["descriptor_batch_id"] = descriptorBatchId,
            ["descriptor_contract_key"] = ReadString(summary, "descriptor_contract_key") ?? descriptorGenerationContract,
            ["descriptor_generation_contract"] = descriptorGenerationContract,
            ["descriptor_overall_status"] = ReadString(summary, "overall_status"),
            ["descriptor_schema_version"] = ReadInt(summary, "descriptor_schema_version") ?? ReadInt(summary, "schema_version"),
            ["descriptor_source"] = "manual-admin-campaign-descriptor-summary",
            ["descriptor_summary"] = descriptorSummary,
            ["evidence"] = descriptorSummary,
            ["external_stable_publication"] = ReadString(summary, "external_stable_publication") ?? "not-claimed",
            ["missing_count"] = ReadInt(summary, "missing_count"),
            ["not_pass_count"] = ReadInt(summary, "not_pass_count"),
            ["package_pair"] = packagePair,
            ["public_trusted_signing"] = ReadString(summary, "public_trusted_signing") ?? "not-claimed",
            ["runner_count"] = ReadInt(summary, "runner_count"),
            ["source"] = "batch_evidence_artifact",
            ["status"] = "artifact-discovered",
            ["target_version"] = targetVersion
        };
    }

    private SortedDictionary<string, object?> BuildPublicBoundaryMainPushSummary(string path, JsonElement summary)
    {
        return new SortedDictionary<string, object?>
        {
            ["evidence"] = ToArtifactEvidencePath(path),
            ["external_stable_publication"] = ReadString(summary, "external_stable_publication") ?? "not-claimed",
            ["head_sha"] = ReadString(summary, "head_sha"),
            ["job_id"] = ReadString(summary, "job_id") ?? ReadString(summary, "check_run_id"),
            ["public_trusted_signing"] = ReadString(summary, "public_trusted_signing") ?? "not-claimed",
            ["run_id"] = ReadString(summary, "run_id"),
            ["source"] = "batch_evidence_artifact",
            ["status"] = "artifact-discovered"
        };
    }

    private static bool IsManualAdminPackagePairSummary(JsonElement? summary)
    {
        if (summary is null || summary.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var scope = ReadString(summary, "scope");
        var overallStatus = ReadString(summary, "overall_status");
        return string.Equals(scope, "manual-admin-campaign-descriptor", StringComparison.Ordinal) &&
            string.Equals(overallStatus, "pass", StringComparison.OrdinalIgnoreCase) &&
            ReadString(summary, "baseline_version") is not null &&
            ReadString(summary, "target_version") is not null;
    }

    private static (int Major, int Minor, int Patch) GetManualAdminTargetVersionSortKey(JsonElement? summary)
    {
        var targetVersion = ReadString(summary, "target_version");
        var match = Regex.Match(targetVersion ?? string.Empty, @"(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)");
        return match.Success
            ? (
                int.Parse(match.Groups["major"].Value),
                int.Parse(match.Groups["minor"].Value),
                int.Parse(match.Groups["patch"].Value))
            : (-1, -1, -1);
    }

    private static long GetManualAdminDescriptorIdSortKey(string path, JsonElement? summary)
    {
        var descriptorId = ReadString(summary, "descriptor_batch_id") ??
            ReadString(summary, "batch_id") ??
            InferDescriptorBatchId(path) ??
            InferDescriptorBatchIdFromCampaignPath(path) ??
            path;
        var match = Regex.Match(descriptorId, @"(?<date>20\d{6})(?:-(?<time>\d{6}))?");
        if (!match.Success)
        {
            return 0;
        }

        var sortable = match.Groups["date"].Value + (match.Groups["time"].Success ? match.Groups["time"].Value : "000000");
        return long.TryParse(sortable, out var parsed) ? parsed : 0;
    }

    private static bool IsPublicBoundaryMainPushSummary(JsonElement? summary)
    {
        if (summary is null || summary.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var scope = ReadString(summary, "scope");
        var result = ReadString(summary, "result");
        var status = ReadString(summary, "status");
        return string.Equals(scope, "public-boundary-ci-required-main-push", StringComparison.Ordinal) &&
            (string.Equals(result, "PASS", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "pass", StringComparison.OrdinalIgnoreCase));
    }

}
