using System.Text.Json;
using DesktopNode.Contracts;
using DesktopNode.HyperV;
using DesktopNode.Runtime;

namespace DesktopNode.Api;

internal static class DesktopNodeApiOpsSummaryBuilder
{
    public static JsonElement Build(
        DesktopNodeHyperVOperationResult hostResult,
        DesktopNodeHyperVOperationResult vmResult,
        IReadOnlyList<JsonElement> jobRows,
        object runtimePolicy,
        JsonElement batchEvidence,
        string? diagnosticsRoot = null,
        DesktopNodeJobStoreHealthSnapshot? jobStoreHealth = null,
        DesktopNodeFeatureQualificationSnapshot? featureQualification = null)
    {
        var qualification = featureQualification ?? DesktopNodeFeatureQualificationSnapshot.Unavailable();
        var vms = vmResult.Ok && vmResult.Data is not null && vmResult.Data.Value.ValueKind == JsonValueKind.Array
            ? vmResult.Data.Value.EnumerateArray().Select(vm => vm.Clone()).ToArray()
            : [];

        var errors = new List<object>();
        if (!hostResult.Ok && hostResult.Error is not null)
        {
            errors.Add(new SortedDictionary<string, object?>
            {
                ["operation"] = hostResult.Operation,
                ["code"] = hostResult.Error.Code,
                ["message"] = hostResult.Error.Message,
                ["retryable"] = hostResult.Error.Retryable
            });
        }

        if (!vmResult.Ok && vmResult.Error is not null)
        {
            errors.Add(new SortedDictionary<string, object?>
            {
                ["operation"] = vmResult.Operation,
                ["code"] = vmResult.Error.Code,
                ["message"] = vmResult.Error.Message,
                ["retryable"] = vmResult.Error.Retryable
            });
        }

        var vmCounts = BuildVmCounts(vms);
        var jobCounts = BuildJobCounts(jobRows);

        return JsonFromObject(new SortedDictionary<string, object?>
        {
            ["batch_evidence"] = batchEvidence,
            ["current_evidence"] = BuildCurrentEvidenceRollup(batchEvidence, qualification),
            ["errors"] = errors,
            ["host"] = hostResult.Ok ? hostResult.Data : null,
            ["installed_runtime"] = BuildInstalledRuntimeSummary(runtimePolicy, batchEvidence, diagnosticsRoot),
            ["job_counts"] = jobCounts,
            ["job_store"] = jobStoreHealth ?? new DesktopNodeJobStoreHealthSnapshot(
                "healthy",
                false,
                null,
                Array.Empty<DesktopNodeJobRuntimeObservation>()),
            ["recent_activity"] = jobRows.Take(8).ToArray(),
            ["runtime_policy"] = runtimePolicy,
            ["signals"] = BuildOpsSignals(hostResult, vmCounts, jobCounts, errors.Count, batchEvidence, qualification),
            ["vm_counts"] = vmCounts
        });
    }

    private static SortedDictionary<string, object?> BuildInstalledRuntimeSummary(
        object runtimePolicy,
        JsonElement batchEvidence,
        string? diagnosticsRoot)
    {
        var runtimePolicyJson = JsonFromObject(runtimePolicy);
        var latestEvidence = ReadElement(batchEvidence, "latest");
        var release = ReadElement(latestEvidence, "release");
        var hostFinalState = ReadElement(latestEvidence, "host_final_state");
        var routeMsiHyperV = ReadElement(latestEvidence, "route_msi_hyperv");
        var publicTrustedSigning = ReadString(release, "public_trusted_signing") ?? "not-claimed";
        var externalStablePublication = ReadString(release, "external_stable_publication") ?? "not-claimed";
        var runtimeEvidence = ApiHandlerAdapterContract.CreateDefault().RuntimeEvidenceContract;

        return new SortedDictionary<string, object?>
        {
            ["auth_boundary"] = new SortedDictionary<string, object?>
            {
                ["runtime_core_boundary"] = ReadString(runtimePolicyJson, "runtime_core", "auth_session", "boundary"),
                ["static_asset_loopback"] = ReadString(runtimePolicyJson, "network", "static_asset_auth", "loopback"),
                ["static_asset_non_loopback"] = ReadString(runtimePolicyJson, "network", "static_asset_auth", "non_loopback"),
                ["token_storage"] = ReadString(runtimePolicyJson, "auth", "token_storage"),
                ["unauthenticated_api_error_code"] = "PCV_AUTH_REQUIRED"
            },
            ["diagnostics"] = new SortedDictionary<string, object?>
            {
                ["root_status"] = string.IsNullOrWhiteSpace(diagnosticsRoot) ? "not_configured" : "configured",
                ["runtime_api_registry_bridge"] = new SortedDictionary<string, object?>
                {
                    ["contract_key"] = runtimeEvidence.RegistryBridgeContractKey,
                    ["documentation_anchor"] = runtimeEvidence.DocumentationAnchor,
                    ["handler_registry_source"] = runtimeEvidence.HandlerRegistrySource,
                    ["route_keys"] = runtimeEvidence.HandlerRegistryRouteKeys
                },
                ["runtime_policy_bundle_root"] = ReadString(runtimePolicyJson, "runtime_core", "diagnostics", "bundle_root")
            },
            ["distribution_claim"] = IsPublicReleaseClaim(publicTrustedSigning, externalStablePublication)
                ? "public-release-claimed"
                : "internal-only-not-public-release",
            ["evidence_anchor"] = ReadString(latestEvidence, "batch_id"),
            ["evidence_status"] = ReadString(batchEvidence, "status"),
            ["external_stable_publication"] = externalStablePublication,
            ["public_trusted_signing"] = publicTrustedSigning,
            ["service_state"] = ReadString(hostFinalState, "service_state"),
            ["version"] = ReadString(release, "version") ?? ReadString(routeMsiHyperV, "version")
        };
    }

    private static SortedDictionary<string, object?> BuildCurrentEvidenceRollup(
        JsonElement batchEvidence,
        DesktopNodeFeatureQualificationSnapshot qualification)
    {
        var latestEvidence = ReadElement(batchEvidence, "latest");
        var release = ReadElement(latestEvidence, "release");
        var hostFinalState = ReadElement(latestEvidence, "host_final_state");
        var routeMsiHyperV = ReadElement(latestEvidence, "route_msi_hyperv");
        var osMutation = ReadElement(latestEvidence, "os_mutation");

        return new SortedDictionary<string, object?>
        {
            ["contract_key"] = "runtime-api-current-evidence-rollup-v1",
            ["feature_qualification"] = BuildFeatureQualification(qualification),
            ["full_admin_host_mutation"] = new SortedDictionary<string, object?>
            {
                ["latest"] = new SortedDictionary<string, object?>
                {
                    ["batch_id"] = ReadString(latestEvidence, "batch_id"),
                    ["evidence_status"] = ReadString(batchEvidence, "status"),
                    ["external_stable_publication"] = ReadString(release, "external_stable_publication") ?? "not-claimed",
                    ["git_commit"] = ReadString(release, "git_commit"),
                    ["msi_sha256"] = ReadString(release, "msi_sha256"),
                    ["ok"] = ReadBool(latestEvidence, "ok"),
                    ["os_mutation_ok"] = ReadBool(osMutation, "ok"),
                    ["os_mutation_status"] = ReadString(osMutation, "status"),
                    ["public_trusted_signing"] = ReadString(release, "public_trusted_signing") ?? "not-claimed",
                    ["route_msi_hyperv_ok"] = ReadBool(routeMsiHyperV, "ok"),
                    ["route_msi_hyperv_status"] = ReadString(routeMsiHyperV, "status"),
                    ["run_status"] = ReadString(latestEvidence, "status"),
                    ["service_state"] = ReadString(hostFinalState, "service_state"),
                    ["signing_mode"] = ReadString(release, "signing_mode"),
                    ["source"] = "batch_evidence",
                    ["status"] = CurrentEvidenceBatchStatus(batchEvidence, latestEvidence),
                    ["version"] = ReadString(release, "version") ?? ReadString(routeMsiHyperV, "version")
                }
            },
            ["host_ops"] = BuildHostOpsCurrentEvidence(),
            ["manual_admin"] = BuildManualAdminCurrentEvidence(batchEvidence),
            ["public_boundary"] = BuildPublicBoundaryCurrentEvidence(batchEvidence),
            ["schema_version"] = 1,
            ["source"] = "ops-summary"
        };
    }

    private static SortedDictionary<string, object?> BuildFeatureQualification(
        DesktopNodeFeatureQualificationSnapshot qualification)
    {
        var result = new SortedDictionary<string, object?>
        {
            ["blockers"] = qualification.Blockers
                .Select(blocker => new SortedDictionary<string, object?>
                {
                    ["feature_id"] = blocker.FeatureId,
                    ["stage"] = blocker.Stage,
                    ["verdict"] = blocker.Verdict
                })
                .ToArray(),
            ["contract"] = qualification.Contract,
            ["promotion_eligible"] = qualification.PromotionEligible,
            ["schema_version"] = qualification.SchemaVersion,
            ["status"] = qualification.Status
        };
        if (!string.IsNullOrWhiteSpace(qualification.ErrorCode))
        {
            result["error_code"] = qualification.ErrorCode;
        }
        return result;
    }

    private static SortedDictionary<string, object?> BuildHostOpsCurrentEvidence()
    {
        var descriptor = HostOpsLifecycleDescriptor.CreateDefault();
        return new SortedDictionary<string, object?>
        {
            ["lifecycle_descriptor"] = new SortedDictionary<string, object?>
            {
                ["buckets"] = descriptor.Buckets
                    .Select(bucket => new SortedDictionary<string, object?>
                    {
                        ["bucket_key"] = bucket.BucketKey,
                        ["mutation_boundary"] = bucket.MutationBoundary,
                        ["operation_family"] = bucket.OperationFamily,
                        ["operations"] = bucket.Operations,
                        ["owner"] = bucket.Owner
                    })
                    .ToArray(),
                ["contract_key"] = descriptor.ContractKey,
                ["host_mutation_performed"] = false,
                ["lifecycle_bucket_contract_key"] = descriptor.LifecycleBucketContractKey,
                ["schema_version"] = descriptor.SchemaVersion,
                ["source"] = HostOpsLifecycleDescriptor.SourceName,
                ["status"] = "contract-linked"
            }
        };
    }

    private static SortedDictionary<string, object?> BuildManualAdminCurrentEvidence(JsonElement batchEvidence)
    {
        var manualAdmin = ReadElement(batchEvidence, "manual_admin");
        var latestPackagePair = ReadElement(manualAdmin, "latest_package_pair");
        var nextPackagePair = ReadElement(manualAdmin, "next_package_pair");
        return new SortedDictionary<string, object?>
        {
            ["latest_package_pair"] = latestPackagePair is not null && latestPackagePair.Value.ValueKind == JsonValueKind.Object
                ? BuildManualAdminLatestPackagePair(latestPackagePair.Value)
                : BuildManualAdminDocumentationPackagePair(),
            ["next_package_pair"] = nextPackagePair is not null && nextPackagePair.Value.ValueKind == JsonValueKind.Object
                ? BuildManualAdminNextPackagePair(nextPackagePair.Value)
                : BuildManualAdminDocumentationNextPackagePair()
        };
    }

    private static SortedDictionary<string, object?> BuildManualAdminLatestPackagePair(JsonElement packagePair)
    {
        var descriptorBatchId = ReadString(packagePair, "descriptor_batch_id") ??
            ReadString(packagePair, "current_card_descriptor_batch_id");
        var evidence = ReadString(packagePair, "evidence");

        return new SortedDictionary<string, object?>
        {
            ["baseline_version"] = ReadString(packagePair, "baseline_version"),
            ["current_card_descriptor_batch_id"] = descriptorBatchId,
            ["descriptor_batch_id"] = descriptorBatchId,
            ["descriptor_contract_key"] = ReadString(packagePair, "descriptor_contract_key") ??
                ReadString(packagePair, "descriptor_generation_contract"),
            ["descriptor_generation_contract"] = ReadString(packagePair, "descriptor_generation_contract"),
            ["descriptor_overall_status"] = ReadString(packagePair, "descriptor_overall_status") ??
                ReadString(packagePair, "overall_status"),
            ["descriptor_schema_version"] = ReadInt(packagePair, "descriptor_schema_version") ??
                ReadInt(packagePair, "schema_version"),
            ["descriptor_source"] = ReadString(packagePair, "descriptor_source") ??
                ReadString(packagePair, "source"),
            ["descriptor_summary"] = ReadString(packagePair, "descriptor_summary") ?? evidence,
            ["evidence"] = evidence,
            ["external_stable_publication"] = ReadString(packagePair, "external_stable_publication") ?? "not-claimed",
            ["missing_count"] = ReadInt(packagePair, "missing_count"),
            ["not_pass_count"] = ReadInt(packagePair, "not_pass_count"),
            ["package_pair"] = ReadString(packagePair, "package_pair"),
            ["public_trusted_signing"] = ReadString(packagePair, "public_trusted_signing") ?? "not-claimed",
            ["runner_count"] = ReadInt(packagePair, "runner_count"),
            ["source"] = ReadString(packagePair, "source"),
            ["status"] = ReadString(packagePair, "status"),
            ["target_version"] = ReadString(packagePair, "target_version")
        };
    }

    private static SortedDictionary<string, object?> BuildManualAdminNextPackagePair(JsonElement packagePair)
    {
        var baselineVersion = ReadString(packagePair, "baseline_version");
        var targetVersion = ReadString(packagePair, "target_version") ?? "next-admin-smoke-required";
        var pair = ReadString(packagePair, "package_pair") ??
            (baselineVersion is not null ? $"{baselineVersion} -> {targetVersion}" : null);

        return new SortedDictionary<string, object?>
        {
            ["baseline_version"] = baselineVersion,
            ["decision"] = ReadString(packagePair, "decision") ?? ReadString(packagePair, "package_pair_decision"),
            ["evidence"] = ReadString(packagePair, "evidence"),
            ["external_stable_publication"] = ReadString(packagePair, "external_stable_publication") ?? "not-claimed",
            ["host_mutation_performed"] = ReadBool(packagePair, "host_mutation_performed") ?? false,
            ["package_pair"] = pair,
            ["public_trusted_signing"] = ReadString(packagePair, "public_trusted_signing") ?? "not-claimed",
            ["source"] = ReadString(packagePair, "source"),
            ["status"] = ReadString(packagePair, "status"),
            ["target_version"] = targetVersion
        };
    }

    private static SortedDictionary<string, object?> BuildManualAdminDocumentationPackagePair()
    {
        const string descriptorBatchId = "manual-admin-campaign-descriptor-20260516-04222-04223-closed";
        const string evidence = "docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md";

        return new SortedDictionary<string, object?>
        {
            ["baseline_version"] = "0.42.22-admin-smoke",
            ["current_card_descriptor_batch_id"] = descriptorBatchId,
            ["descriptor_batch_id"] = descriptorBatchId,
            ["descriptor_contract_key"] = "manual-admin-descriptor-generation-contract-v2",
            ["descriptor_generation_contract"] = "manual-admin-descriptor-generation-contract-v2",
            ["descriptor_overall_status"] = "pass",
            ["descriptor_schema_version"] = 2,
            ["descriptor_source"] = "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
            ["descriptor_summary"] = evidence,
            ["evidence"] = evidence,
            ["external_stable_publication"] = "not-claimed",
            ["missing_count"] = 0,
            ["not_pass_count"] = 0,
            ["package_pair"] = "0.42.22-admin-smoke -> 0.42.23-admin-smoke",
            ["public_trusted_signing"] = "not-claimed",
            ["source"] = "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
            ["status"] = "tracked-in-documentation",
            ["target_version"] = "0.42.23-admin-smoke"
        };
    }

    private static SortedDictionary<string, object?> BuildManualAdminDocumentationNextPackagePair()
    {
        return new SortedDictionary<string, object?>
        {
            ["baseline_version"] = "0.42.56-admin-smoke",
            ["decision"] = "opened-public-boundary-current-evidence-rollup-payload",
            ["evidence"] = "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md",
            ["external_stable_publication"] = "not-claimed",
            ["host_mutation_performed"] = false,
            ["package_pair"] = "0.42.56-admin-smoke -> 0.42.57-admin-smoke",
            ["public_trusted_signing"] = "not-claimed",
            ["source"] = "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
            ["status"] = "candidate-selected-public-boundary-current-rollup",
            ["target_version"] = "0.42.57-admin-smoke"
        };
    }

    private static SortedDictionary<string, object?> BuildPublicBoundaryCurrentEvidence(JsonElement batchEvidence)
    {
        var publicBoundary = ReadElement(batchEvidence, "public_boundary");
        var latestMainPush = ReadElement(publicBoundary, "latest_main_push");
        return new SortedDictionary<string, object?>
        {
            ["latest_main_push"] = latestMainPush is not null && latestMainPush.Value.ValueKind == JsonValueKind.Object
                ? latestMainPush.Value.Clone()
                : new SortedDictionary<string, object?>
                {
                    ["evidence"] = "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04256-manual-admin-closure-postpush-pass.md",
                    ["external_stable_publication"] = "not-claimed",
                    ["head_sha"] = "7a7d5de822bdb058b04149eeeef0a7eb462828b5",
                    ["job_id"] = "78303066840",
                    ["public_trusted_signing"] = "not-claimed",
                    ["run_id"] = "26578120570",
                    ["source"] = "docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md",
                    ["status"] = "tracked-in-documentation"
                }
        };
    }

    private static SortedDictionary<string, object?> BuildVmCounts(IReadOnlyList<JsonElement> vms)
    {
        var running = 0;
        var checkpointWarnings = 0;
        foreach (var vm in vms)
        {
            var state = ReadString(vm, "state") ?? ReadString(vm, "status") ?? string.Empty;
            if (state.Contains("running", StringComparison.OrdinalIgnoreCase))
            {
                running++;
            }

            var checkpointCount = ReadInt(vm, "checkpoints_count") ??
                (vm.TryGetProperty("checkpoints", out var checkpoints) ? ReadInt(checkpoints, "count") : null) ??
                0;
            if (checkpointCount >= 10)
            {
                checkpointWarnings++;
            }
        }

        return new SortedDictionary<string, object?>
        {
            ["checkpoint_warnings"] = checkpointWarnings,
            ["running"] = running,
            ["total"] = vms.Count
        };
    }

    private static SortedDictionary<string, object?> BuildJobCounts(IReadOnlyList<JsonElement> jobRows)
    {
        var counts = new SortedDictionary<string, object?>
        {
            ["canceled"] = 0,
            ["failed"] = 0,
            ["queued"] = 0,
            ["running"] = 0,
            ["succeeded"] = 0
        };

        foreach (var job in jobRows)
        {
            var status = ReadString(job, "status") ?? "unknown";
            if (counts.ContainsKey(status))
            {
                counts[status] = (int)counts[status]! + 1;
            }
        }

        return counts;
    }

    private static object[] BuildOpsSignals(
        DesktopNodeHyperVOperationResult hostResult,
        SortedDictionary<string, object?> vmCounts,
        SortedDictionary<string, object?> jobCounts,
        int errorCount,
        JsonElement batchEvidence,
        DesktopNodeFeatureQualificationSnapshot qualification)
    {
        var hostReady = hostResult.Ok &&
            hostResult.Data is not null &&
            hostResult.Data.Value.TryGetProperty("supported", out var supported) &&
            supported.ValueKind == JsonValueKind.True;
        var failedJobs = (int)jobCounts["failed"]!;
        var checkpointWarnings = (int)vmCounts["checkpoint_warnings"]!;
        var batchEvidenceStatus = ReadString(batchEvidence, "status") ?? "not_configured";
        var batchEvidenceTone = batchEvidenceStatus is "available" or "not_configured" ? "ok" : "warn";

        return
        [
            new SortedDictionary<string, object?> { ["key"] = "host-readiness", ["label"] = "Host readiness", ["tone"] = hostReady ? "ok" : "warn", ["value"] = hostReady ? "Ready" : "Needs attention" },
            new SortedDictionary<string, object?> { ["key"] = "failed-jobs", ["label"] = "Failed jobs", ["tone"] = failedJobs > 0 ? "error" : "ok", ["value"] = failedJobs },
            new SortedDictionary<string, object?> { ["key"] = "checkpoint-warning", ["label"] = "Checkpoint warnings", ["tone"] = checkpointWarnings > 0 ? "warn" : "ok", ["value"] = checkpointWarnings },
            new SortedDictionary<string, object?> { ["key"] = "summary-errors", ["label"] = "Summary errors", ["tone"] = errorCount > 0 ? "warn" : "ok", ["value"] = errorCount },
            new SortedDictionary<string, object?> { ["key"] = "batch-evidence", ["label"] = "Batch evidence", ["tone"] = batchEvidenceTone, ["value"] = batchEvidenceStatus },
            new SortedDictionary<string, object?>
            {
                ["key"] = "feature-promotion",
                ["label"] = "Feature promotion",
                ["tone"] = qualification.Status == "eligible" ? "ok" : "error",
                ["value"] = qualification.Status == "unavailable"
                    ? "unavailable"
                    : qualification.Blockers.Count
            }
        ];
    }

    private static string? ReadString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var value) ||
            value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static string? ReadString(JsonElement? element, params string[] path)
    {
        var current = element;
        foreach (var name in path)
        {
            current = ReadElement(current, name);
            if (current is null)
            {
                return null;
            }
        }

        if (current is null)
        {
            return null;
        }

        var value = current.Value;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static JsonElement? ReadElement(JsonElement? element, string name)
    {
        return element is not null &&
            element.Value.ValueKind == JsonValueKind.Object &&
            element.Value.TryGetProperty(name, out var value) &&
            value.ValueKind != JsonValueKind.Null
            ? value.Clone()
            : null;
    }

    private static bool? ReadBool(JsonElement? element, params string[] path)
    {
        var current = element;
        foreach (var name in path)
        {
            current = ReadElement(current, name);
            if (current is null)
            {
                return null;
            }
        }

        if (current is null)
        {
            return null;
        }

        var value = current.Value;
        return value.ValueKind == JsonValueKind.True
            ? true
            : value.ValueKind == JsonValueKind.False
                ? false
                : null;
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.TryGetInt32(out var parsed)
            ? parsed
            : null;
    }

    private static JsonElement JsonFromObject(object value)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, RuntimePolicyContract.JsonOptions));
        return document.RootElement.Clone();
    }

    private static bool IsPublicReleaseClaim(string publicTrustedSigning, string externalStablePublication)
    {
        static bool Claimed(string value)
        {
            return value.Contains("pass", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("published", StringComparison.OrdinalIgnoreCase);
        }

        return Claimed(publicTrustedSigning) && Claimed(externalStablePublication);
    }

    private static string CurrentEvidenceBatchStatus(JsonElement batchEvidence, JsonElement? latestEvidence)
    {
        var status = ReadString(batchEvidence, "status") ?? "not_configured";
        if (latestEvidence is null || latestEvidence.Value.ValueKind == JsonValueKind.Null)
        {
            return status;
        }

        return status;
    }
}
