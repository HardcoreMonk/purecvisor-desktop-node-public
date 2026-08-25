using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

internal static class D2EvidenceContractVerifier
{
    private static readonly RepositoryContractContext Repository =
        RepositoryContractContext.Find();

    private static readonly string[] Evidence04273 =
    [
        "docs/ga-ready/evidence/admin-smoke-package-2026-08-14-04273.md",
        "docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-14-04273-hostmutation.md",
        "docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-14-04273.md",
        "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md",
        "docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md",
        "docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md",
        "docs/ga-ready/evidence/operational-credential-rebootstrap-recovery-r2-2026-08-09-04272.md",
        "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md",
    ];

    private static readonly string[] ExpectedFeatureIds =
    [
        "pcv.checkpoint.restore",
        "pcv.vm.managed-import",
        "pcv.vm.media-attach",
        "pcv.vm.saved-lifecycle",
    ];

    private static readonly string[] ExpectedStages =
    [
        "code_tested",
        "packaged",
        "installed_tested",
        "actual_vm_tested",
        "manual_admin_tested",
    ];

    internal static void Verify(string owner, int ordinal)
    {
        switch (owner)
        {
            case "04273-promotion-evidence":
                Verify04273(ordinal);
                break;
            case "04274-package-evidence":
                Verify04274(ordinal);
                break;
            case "current-evidence-generation":
                VerifyCurrent(ordinal);
                break;
            case "feature-evidence-promotion":
                VerifyFeature(ordinal);
                break;
            case "job-store04265-reader-compatibility":
                VerifyJobStore(ordinal);
                break;
            default:
                throw new InvalidDataException($"PCV_DELIVERY_D2_INVALID|{owner}|unknown-owner");
        }
    }

    private static void Verify04273(int ordinal)
    {
        switch (ordinal)
        {
            case 1:
                Assert.Equal(8, Evidence04273.Length);
                for (var index = 0; index < Evidence04273.Length; index++)
                {
                    var path = Evidence04273[index];
                    Assert.True(File.Exists(Absolute(path)), path);
                    var signing = index == 1 ? "excluded" : "not-claimed";
                    RequireMetadata(path, new Dictionary<string, string>
                    {
                        ["result"] = "PASS",
                        ["public_trusted_signing"] = signing,
                        ["external_stable_publication"] = "not-claimed",
                    });
                }

                break;
            case 2:
                var record = ParseNode("docs/ga-ready/current-evidence.json");
                Assert.NotEqual(
                    "0.42.73-admin-smoke",
                    record["current"]!["version"]!.GetValue<string>());
                RequireMetadata(
                    "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
                    new Dictionary<string, string>
                    {
                        ["previous_04273_current_manual_admin_package_pair"] =
                            "0.42.72-admin-smoke -> 0.42.73-admin-smoke",
                        ["previous_04273_current_manual_admin_campaign"] =
                            "docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md",
                        ["previous_04273_current_manual_admin_descriptor_batch_manifest"] =
                            "manual-admin-campaign-descriptor-20260814-04272-04273-closed",
                        ["previous_04273_current_full_admin_host_mutation_batch"] =
                            "full-admin-host-mutation-gate-20260814-04273",
                        ["previous_04273_current_full_admin_host_mutation_operational_msi_sha256"] =
                            "3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1",
                        ["previous_04273_current_full_admin_host_mutation_payload_aggregate_sha256"] =
                            "a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85",
                        ["previous_04273_current_full_admin_host_mutation_provenance_commit"] =
                            "b84441f0750a9f77fd0588a86912dbdb68b94f0c",
                        ["previous_04273_current_installed_operator_surface_current_card_evidence"] =
                            "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md",
                    });
                break;
            case 3:
                const string token =
                    "docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md";
                RequireMetadata(token, new Dictionary<string, string>
                {
                    ["r4_runner_raw_sha256"] =
                        "c6e138a008315bc2b75b76eb51a202cb75163cd37b961e4a9dfb5f14c2b98414",
                    ["r4_runner_contract_sha256"] =
                        "259547e6eb82d66f172f7bf5f02d9171af1a6b84bcf2d9f8680780b7eb0b424f",
                    ["final_summary_sha256"] =
                        "285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136",
                    ["current_claim_eligible"] = "true",
                    ["historical_retry2_host_mutation_performed"] = "true",
                    ["read_only_reconciliation_host_mutation_performed"] = "false",
                    ["host_mutation_performed"] = "false",
                    ["token_value_recorded"] = "false",
                });
                RequireMatches(token, [
                    @"\|\s*classification\s*\|\s*`native-rotation-succeeded-verifier-false-negative-reconciled`\s*\|",
                    @"\|\s*direct auth readback\s*\|\s*old token HTTP `403`, new token HTTP `200`\s*\|",
                    @"\|\s*secret scan\s*\|\s*findings `0`, read failures `0`, raw values recorded `false`\s*\|",
                ]);
                break;
            case 4:
                RequireMetadata(
                    "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md",
                    new Dictionary<string, string>
                    {
                        ["promotion_ledger_status"] = "promoted-current",
                        ["token_rotation_evidence"] =
                            "docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md",
                        ["token_rotation_r4_summary"] =
                            "artifacts/installed-token-rotation-smoke-reconciliation-r4-20260810-04272/summary.json",
                        ["token_rotation_r4_summary_sha256"] =
                            "285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136",
                        ["token_rotation_status"] =
                            "carry-forward-no-token-payload-change-after-04272",
                        ["latest_manual_admin_package_pair"] =
                            "0.42.72-admin-smoke -> 0.42.73-admin-smoke",
                    });
                break;
            case 5:
                foreach (var path in new[]
                {
                    "docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md",
                    "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-14-04273.md",
                    "docs/ga-ready/evidence/manual-admin-campaign-2026-08-14-04272-04273.md",
                    "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-09-pr186-postmerge-pass.md",
                    "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md",
                    "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md",
                })
                {
                    Assert.DoesNotMatch(
                        new Regex(
                            "(?-i:PENDING)|(?i:\\bdeferred\\b|\\bdraft\\b|\\bR2(?:\\s+|-)final\\b)"),
                        Text(path));
                }

                break;
            case 6:
                Verify04273PostPush();
                break;
            case 7:
                var indexText = Text("docs/ga-ready/EVIDENCE_INDEX.md");
                foreach (var path in Evidence04273)
                {
                    Assert.Contains(path, indexText, StringComparison.Ordinal);
                }

                Assert.Contains(
                    "manual-admin-campaign-descriptor-20260814-04272-04273-closed",
                    indexText,
                    StringComparison.Ordinal);
                Assert.Contains(
                    "285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136",
                    indexText,
                    StringComparison.Ordinal);
                RequireMatches("docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md", [
                    "285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136",
                    Regex.Escape(Evidence04273[5]),
                    Regex.Escape(Evidence04273[7]),
                ]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ordinal));
        }
    }

    private static void Verify04273PostPush()
    {
        const string path =
            "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-14-04273-promotion-postpush-pass.md";
        RequireMetadata(path, new Dictionary<string, string>
        {
            ["result"] = "PASS",
            ["scope"] = "post-04273-promotion-main-push",
            ["run_id"] = "31737488576",
            ["job_id"] = "94572517694",
            ["head_sha"] = "291435e374efef7f9639b820ac197c11e2c7e8a4",
            ["development_gates_run_id"] = "31737488562",
            ["product_payload_change_detected"] = "false",
            ["changed_path_count"] = "17",
            ["product_payload_path_count"] = "0",
            ["current_version_anchor"] = "0.42.73-admin-smoke",
            ["additional_package_candidate_opened"] = "false",
            ["package_candidate_decision"] =
                "docs-only-followup-retains-0.42.73-admin-smoke",
            ["public_trusted_signing"] = "not-claimed",
            ["external_stable_publication"] = "not-claimed",
        });
        RequireMatches(path, [
            @"\|\s*`web-tests`\s*\|\s*`94572517696`\s*\|\s*`success`\s*\|",
            @"\|\s*`dotnet-tests`\s*\|\s*`94572517725`\s*\|\s*`success`\s*\|",
            @"\|\s*`packaging-pester`\s*\|\s*`94572517728`\s*\|\s*`success`\s*\|",
            @"\|\s*`installer-web-pester`\s*\|\s*`94572517741`\s*\|\s*`success`\s*\|",
        ]);
        var descriptor = "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md";
        RequireMetadata(descriptor, new Dictionary<string, string>
        {
            ["previous_04273_current_public_boundary_main_push_evidence"] = path,
            ["previous_04273_current_public_boundary_main_push_run_id"] = "31737488576",
            ["previous_04273_current_public_boundary_main_push_job_id"] = "94572517694",
            ["previous_04273_current_public_boundary_main_push_head_sha"] =
                "291435e374efef7f9639b820ac197c11e2c7e8a4",
            ["previous_04273_current_public_boundary_main_push_product_payload_change_detected"] =
                "false",
            ["previous_04273_current_public_boundary_main_push_package_candidate_decision"] =
                "docs-only-followup-retains-0.42.73-admin-smoke",
            ["previous_pr187_current_public_boundary_main_push_evidence"] =
                "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md",
        });
        var ledger = "docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md";
        RequireMetadata(ledger, new Dictionary<string, string>
        {
            ["previous_04273_current_public_boundary_main_push_evidence"] = path,
            ["previous_04273_current_public_boundary_main_push_run_id"] = "31737488576",
            ["previous_04273_current_public_boundary_main_push_job_id"] = "94572517694",
            ["previous_04273_current_public_boundary_main_push_head_sha"] =
                "291435e374efef7f9639b820ac197c11e2c7e8a4",
            ["previous_04273_current_public_boundary_product_payload_change_detected"] = "false",
            ["previous_04273_current_public_boundary_package_candidate_decision"] =
                "docs-only-followup-retains-0.42.73-admin-smoke",
            ["previous_pr187_public_boundary_main_push_evidence"] =
                "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-12-pr187-postmerge-pass.md",
        });
        RequireMatches(ledger, [
            @"\|\s*`public-boundary-04273-promotion-predecessor`\s*\|\s*`pass`, 0\.42\.73 promotion main push\s*\|",
            @"\|\s*`public-boundary-pr187-predecessor`\s*\|",
            "docs-only-followup-retains-0\\.42\\.73-admin-smoke",
        ]);
        var index = Text("docs/ga-ready/EVIDENCE_INDEX.md");
        Assert.Contains(path, index, StringComparison.Ordinal);
        Assert.Contains("product payload 경로는 `0`개", index, StringComparison.Ordinal);
        Assert.DoesNotContain("0.42.74", Text(path), StringComparison.Ordinal);
    }

    private static void Verify04274(int ordinal)
    {
        switch (ordinal)
        {
            case 1:
                var record = D2CurrentEvidenceVerifier.Validate(
                    Text(D2CurrentEvidenceVerifier.RecordPath),
                    Repository);
                Assert.Equal(1, record.SchemaVersion);
                Assert.Equal("pcv-current-evidence-v1", record.Contract);
                Assert.Equal("0.42.74-admin-smoke", record.Current.Version);
                Assert.Equal(["web", "cli"], record.Current.OperatorSurfaces);
                Assert.False(record.Current.TuiPresent);
                Assert.Equal(
                    "docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md",
                    record.Current.PackageEvidence);
                Assert.Equal(
                    "full-admin-host-mutation-gate-20260820-04274",
                    record.Current.FullgateBatch);
                Assert.Equal(
                    "f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e",
                    record.Current.CleanMsiSha256);
                Assert.Equal(
                    "2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787",
                    record.Current.OperationalMsiSha256);
                Assert.Equal(
                    "c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d",
                    record.Current.PayloadSha256);
                Assert.Equal(
                    "adc04673b569ef9b587371fdb23bc11ceb14e2e2",
                    record.Current.ProvenanceCommit);
                Assert.Equal("0.42.73-admin-smoke", record.ManualAdmin.LatestClosedBaseline);
                Assert.Equal("0.42.74-admin-smoke", record.ManualAdmin.LatestClosedTarget);
                Assert.False(record.Claims.PublicTrustedSigning);
                Assert.False(record.Claims.ExternalStablePublication);
                break;
            case 2:
                Verify04274Package();
                break;
            case 3:
                Verify04274DescriptorCurrent();
                break;
            case 4:
                Verify04274Indexes();
                break;
            case 5:
                Verify04274Fullgate();
                break;
            case 6:
                Verify04274Card();
                break;
            case 7:
                RequireMetadata(
                    "docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md",
                    new Dictionary<string, string>
                    {
                        ["result"] = "PASS",
                        ["summary_sha256"] =
                            "5395286b74ca7dabd3edccbb63c0b006c32999a4c350559e8b90ddb1ea1fb4b8",
                        ["vm_name"] = "pcv-fc-cf-04274",
                        ["host_mutation_performed"] = "true",
                        ["canonical_current_evidence"] = "0.42.74-admin-smoke",
                        ["public_trusted_signing"] = "not-claimed",
                    });
                break;
            case 8:
                const string p0 =
                    "docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md";
                RequireMetadata(p0, new Dictionary<string, string>
                {
                    ["result"] = "FAIL",
                    ["summary_sha256"] =
                        "11d8d1b34d6e6ff49e2ebb81bc234d20b7eab9f1299baa36ce8daac9c9b14e5d",
                    ["host_mutation_performed"] = "true",
                    ["canonical_current_evidence"] = "0.42.74-admin-smoke",
                    ["public_trusted_signing"] = "not-claimed",
                });
                RequireMatches(p0, ["32775", "RequestedState `6`", "PCV_VM_NOT_MANAGED_BY_PURECVISOR", "열린 결함"]);
                break;
            case 9:
                const string pair =
                    "docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md";
                RequireMetadata(pair, new Dictionary<string, string>
                {
                    ["result"] = "PASS",
                    ["baseline_version"] = "0.42.73-admin-smoke",
                    ["target_version"] = "0.42.74-admin-smoke",
                    ["descriptor_batch_id"] =
                        "manual-admin-campaign-descriptor-20260820-04273-04274-closed",
                    ["target_msi_sha256"] =
                        "f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e",
                    ["update_zip_sha256"] =
                        "cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1",
                    ["host_mutation_performed"] = "true",
                    ["canonical_current_evidence"] = "0.42.74-admin-smoke",
                    ["canonical_current_changed"] = "true",
                    ["public_trusted_signing"] = "not-claimed",
                });
                RequireMatches(pair, ["runner_count=6", "KB5120242"]);
                break;
            case 10:
                Verify04274DescriptorLinks();
                break;
            case 11:
                Verify04274CurrentAnchor();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ordinal));
        }
    }

    private static void Verify04274Package()
    {
        const string path =
            "docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md";
        RequireMetadata(path, new Dictionary<string, string>
        {
            ["result"] = "PASS",
            ["version"] = "0.42.74-admin-smoke",
            ["source_commit"] = "adc04673b569ef9b587371fdb23bc11ceb14e2e2",
            ["artifact_root"] = "artifacts/admin-smoke-package-20260820-04274",
            ["signing_mode"] = "AllowUnsignedDev",
            ["signing_trust_model"] = "LocalTest",
            ["clean_package_msi_sha256"] =
                "f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e",
            ["clean_package_payload_aggregate_sha256"] =
                "c55cd17d14fed521252e6fee1bf08c828410339b23172fadb01dbd19f7d2578e",
            ["payload_file_count"] = "8",
            ["host_mutation_performed"] = "false",
            ["package_installed"] = "false",
            ["canonical_current_evidence"] = "0.42.74-admin-smoke",
            ["canonical_current_changed"] = "true",
            ["public_trusted_signing"] = "not-claimed",
            ["external_stable_publication"] = "not-claimed",
        });
        RequireMatches(path, [
            "full admin host mutation",
            "manual-admin-campaign-descriptor-20260820-04273-04274-closed",
        ]);
    }

    private static void Verify04274DescriptorCurrent()
    {
        RequireMetadata(
            "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
            new Dictionary<string, string>
            {
                ["current_manual_admin_package_pair"] =
                    "0.42.73-admin-smoke -> 0.42.74-admin-smoke",
                ["latest_manual_admin_candidate_status"] = "pass-closed",
                ["latest_manual_admin_candidate_package_pair"] =
                    "0.42.73-admin-smoke -> 0.42.74-admin-smoke",
                ["next_manual_admin_package_pair_candidate"] =
                    "0.42.74-admin-smoke -> next-admin-smoke-required",
                ["next_manual_admin_package_pair_candidate_status"] =
                    "not-opened-awaiting-next-product-payload",
                ["current_manual_admin_update_package_sha256"] =
                    "cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1",
                ["current_manual_admin_descriptor_batch_manifest"] =
                    "manual-admin-campaign-descriptor-20260820-04273-04274-closed",
                ["current_manual_admin_target_msi_sha256"] =
                    "f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e",
                ["current_full_admin_host_mutation_provenance_commit"] =
                    "adc04673b569ef9b587371fdb23bc11ceb14e2e2",
                ["current_public_boundary_main_push_package_candidate_decision"] =
                    "landed-already-validated-as-0.42.74-admin-smoke",
                ["current_public_boundary_main_push_evidence"] =
                    "docs/ga-ready/evidence/public-boundary-ci-main-push-2026-08-21-04274-p0-landing-pass.md",
                ["current_public_boundary_main_push_run_id"] = "32388996125",
                ["current_public_boundary_main_push_job_id"] = "96490306459",
                ["current_public_boundary_main_push_head_sha"] =
                    "5f9cecfd5507e7e5dd726601aae3760e4e1b558c",
                ["current_public_boundary_main_push_product_payload_change_detected"] = "true",
            });
    }

    private static void Verify04274Indexes()
    {
        RequireMatches("docs/ga-ready/EVIDENCE_INDEX.md", [
            "docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md",
            "docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md",
            "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md",
            "canonical current는 `0\\.42\\.74-admin-smoke`다",
            "docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md",
            "docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md",
            "docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md",
        ]);
        RequireMatches("docs/ga-ready/CONTROL_PLANE_INDEX.md", [
            "docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md",
            "operational current는 `0\\.42\\.74-admin-smoke`다",
        ]);
        RequireMatches("docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md", [
            @"\|\s*`manual-admin-package-pair-next`\s*\|\s*`not-opened-awaiting-next-product-payload`,\s*`0\.42\.74-admin-smoke -> next-admin-smoke-required`\s*\|",
            @"\|\s*`service-plan-p0-save-open-defect`\s*\|\s*`fail-open`",
            @"\|\s*`package-build-current`\s*\|\s*`package-build-pass`,\s*`0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`full-admin-host-mutation-current`\s*\|\s*`pass`,\s*`0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`installed-operator-surface-smoke-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.74-admin-smoke`\s*\|",
        ]);
    }

    private static void Verify04274Fullgate()
    {
        const string path =
            "docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md";
        RequireMetadata(path, new Dictionary<string, string>
        {
            ["result"] = "PASS",
            ["version"] = "0.42.74-admin-smoke",
            ["batch_id"] = "full-admin-host-mutation-gate-20260820-04274",
            ["operational_fullgate_msi_sha256"] =
                "2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787",
            ["operational_fullgate_payload_aggregate_sha256"] =
                "c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d",
            ["provenance_commit"] = "adc04673b569ef9b587371fdb23bc11ceb14e2e2",
            ["host_mutation_performed"] = "true",
            ["canonical_current_evidence"] = "0.42.74-admin-smoke",
            ["canonical_current_changed"] = "true",
            ["public_trusted_signing"] = "excluded",
            ["external_stable_publication"] = "not-claimed",
        });
        RequireMatches(path, ["pcv-spike-api-79522716", "PCV_VM_NOT_MANAGED_BY_PURECVISOR", @"remaining_pcv_vms=\[\]"]);
    }

    private static void Verify04274Card()
    {
        RequireMetadata(
            "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md",
            new Dictionary<string, string>
            {
                ["result"] = "PASS",
                ["version"] = "0.42.74-admin-smoke",
                ["tui_present"] = "false",
                ["summary_sha256"] =
                    "531fc614da5edb0e11994b021383491ccb8830115d59fb211c6c330f5b25f8c8",
                ["cli_exit_zero_count"] = "3",
                ["web_http_200_count"] = "2",
                ["secret_observed"] = "false",
                ["host_mutation_performed"] = "false",
                ["promotion_ledger_status"] = "promoted-current",
                ["canonical_current_evidence"] = "0.42.74-admin-smoke",
                ["canonical_current_changed"] = "true",
                ["latest_manual_admin_package_pair"] =
                    "0.42.73-admin-smoke -> 0.42.74-admin-smoke",
                ["token_rotation_evidence"] =
                    "docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md",
                ["token_rotation_r4_summary_sha256"] =
                    "285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136",
                ["token_rotation_status"] =
                    "carry-forward-no-token-payload-change-after-04272",
                ["public_trusted_signing"] = "not-claimed",
                ["external_stable_publication"] = "not-claimed",
            });
    }

    private static void Verify04274DescriptorLinks()
    {
        RequireMetadata(
            "docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md",
            new Dictionary<string, string>
            {
                ["current_manual_admin_package_pair"] =
                    "0.42.73-admin-smoke -> 0.42.74-admin-smoke",
                ["current_manual_admin_campaign"] =
                    "docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md",
                ["current_manual_admin_campaign_root"] =
                    "artifacts/manual-admin-campaign-20260820-04273-04274",
                ["current_manual_admin_target_package_root"] =
                    "artifacts/admin-smoke-package-20260820-04274",
                ["current_manual_admin_target_msi_sha256"] =
                    "f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e",
                ["current_manual_admin_update_package_sha256"] =
                    "cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1",
                ["current_manual_admin_descriptor_batch_manifest"] =
                    "manual-admin-campaign-descriptor-20260820-04273-04274-closed",
                ["current_manual_admin_descriptor_summary"] =
                    "artifacts/manual-admin-campaign-20260820-04273-04274/manual-admin-campaign-descriptor/summary.json",
                ["current_installed_operator_surface_current_card_evidence"] =
                    "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md",
                ["latest_manual_admin_candidate_package_pair"] =
                    "0.42.73-admin-smoke -> 0.42.74-admin-smoke",
                ["latest_manual_admin_candidate_campaign"] =
                    "docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md",
                ["latest_manual_admin_candidate_descriptor_batch_manifest"] =
                    "manual-admin-campaign-descriptor-20260820-04273-04274-closed",
                ["latest_manual_admin_candidate_status"] = "pass-closed",
                ["current_full_admin_host_mutation_gate"] =
                    "docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md",
                ["current_full_admin_host_mutation_batch"] =
                    "full-admin-host-mutation-gate-20260820-04274",
                ["current_full_admin_host_mutation_payload_aggregate_sha256"] =
                    "c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d",
                ["current_full_admin_host_mutation_operational_msi_sha256"] =
                    "2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787",
                ["current_full_admin_host_mutation_provenance_commit"] =
                    "adc04673b569ef9b587371fdb23bc11ceb14e2e2",
            });
    }

    private static void Verify04274CurrentAnchor()
    {
        var ledger = Text("docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md");
        var match = Regex.Match(ledger, @"(?s)## 현재 Anchor\s*(?<body>.*?)(?=\r?\n## |\z)");
        Assert.True(match.Success);
        var anchor = match.Groups["body"].Value;
        foreach (var pattern in new[]
        {
            @"\|\s*`full-admin-host-mutation-current`\s*\|\s*`pass`,\s*`0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`manual-admin-package-pair-current`\s*\|\s*`pass`,\s*`0\.42\.73-admin-smoke -> 0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`package-build-current`\s*\|\s*`package-build-pass`,\s*`0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`latest-product-payload-smoke`\s*\|\s*`pass`,\s*package\s*`0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`functional-correctness-actual-host-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`installed-operator-surface-smoke-latest`\s*\|\s*`pass`,\s*installed\s*`0\.42\.74-admin-smoke`\s*\|",
            @"\|\s*`service-plan-p0-save-open-defect`\s*\|\s*`fail-open`",
            "285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136",
        })
        {
            Assert.Matches(new Regex(pattern, RegexOptions.IgnoreCase), anchor);
        }

        foreach (var path in new[]
        {
            "docs/ga-ready/evidence/admin-smoke-package-2026-08-20-04274.md",
            "docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-20-04274-hostmutation.md",
            "docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-20-04274.md",
            "docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-08-20-04274.md",
            "docs/ga-ready/evidence/manual-admin-campaign-2026-08-20-04273-04274.md",
            "docs/ga-ready/evidence/installed-token-rotation-smoke-2026-08-09-04272.md",
            "docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md",
        })
        {
            Assert.Contains(path, anchor, StringComparison.Ordinal);
        }

        RequireMatches("docs/ga-ready/EVIDENCE_INDEX.md", [
            "manual-admin-campaign-descriptor-20260820-04273-04274-closed",
            "285661fe50ade63169b6cfc85ff1dcf754a679e30152bd04d166581b4d762136",
        ]);
    }

    private static void VerifyCurrent(int ordinal)
    {
        var canonicalJson = Text(D2CurrentEvidenceVerifier.RecordPath);
        var record = D2CurrentEvidenceVerifier.Validate(
            canonicalJson,
            Repository);
        switch (ordinal)
        {
            case 1:
                Assert.Equal(1, record.SchemaVersion);
                Assert.Equal("pcv-current-evidence-v1", record.Contract);
                Assert.Matches("^0\\.\\d+\\.\\d+-admin-smoke$", record.Current.Version);
                Assert.Equal(["web", "cli"], record.Current.OperatorSurfaces);
                Assert.False(record.Current.TuiPresent);
                Assert.Matches("^[0-9a-f]{40}$", record.Current.ProvenanceCommit);
                break;
            case 2:
                Assert.Equal(1, record.FeatureQualification.SchemaVersion);
                Assert.Equal(
                    "pcv-feature-promotion-decision-v1",
                    record.FeatureQualification.Contract);
                Assert.False(record.FeatureQualification.PromotionEligible);
                var blocker = Assert.Single(record.FeatureQualification.Blockers);
                Assert.Equal("pcv.vm.saved-lifecycle", blocker.FeatureId);
                Assert.Equal("actual_vm_tested", blocker.Stage);
                Assert.Equal("fail", blocker.Verdict);
                break;
            case 3:
                var eligible = ParseNodeText(canonicalJson);
                eligible["feature_qualification"]!["promotion_eligible"] = true;
                eligible["feature_qualification"]!["blockers"] = new JsonArray();
                Assert.True(D2CurrentEvidenceVerifier.Validate(
                    eligible.ToJsonString(),
                    Repository).FeatureQualification.PromotionEligible);
                var contradictory = ParseNodeText(canonicalJson);
                contradictory["feature_qualification"]!["promotion_eligible"] = true;
                Assert.Throws<InvalidDataException>(() =>
                    D2CurrentEvidenceVerifier.Validate(
                        contradictory.ToJsonString(),
                        Repository));
                break;
            case 4:
                VerifyCurrentInvalidCases(canonicalJson);
                break;
            case 5:
                var qualificationBlock = D2CurrentEvidenceVerifier.Render(record);
                Assert.Contains("Feature qualification:", qualificationBlock, StringComparison.Ordinal);
                Assert.Contains("promotion_eligible=false", qualificationBlock, StringComparison.Ordinal);
                Assert.Contains("blocker_count=1", qualificationBlock, StringComparison.Ordinal);
                Assert.Contains(
                    "pcv.vm.saved-lifecycle/actual_vm_tested/fail",
                    qualificationBlock,
                    StringComparison.Ordinal);
                break;
            case 6:
                var candidateRecord = record with
                {
                    Current = record.Current with { Version = "0.42.75-admin-smoke" },
                };
                var blocked = Assert.Throws<InvalidDataException>(() =>
                    D2CurrentEvidenceVerifier.AssertPromotionAllowed(candidateRecord, record));
                Assert.StartsWith(
                    "PCV_FEATURE_PROMOTION_BLOCKED|0.42.75-admin-smoke|blockers=1",
                    blocked.Message,
                    StringComparison.Ordinal);
                break;
            case 7:
                var caseCandidate = ParseNodeText(canonicalJson);
                caseCandidate["current"]!["version"] = "0.42.74-ADMIN-SMOKE";
                Assert.Throws<InvalidDataException>(() =>
                    D2CurrentEvidenceVerifier.Validate(
                        caseCandidate.ToJsonString(),
                        Repository));
                break;
            case 8:
                Assert.Throws<InvalidDataException>(() =>
                    D2CurrentEvidenceVerifier.Validate(
                        """{"schema_version":1}""",
                        Repository));
                var missing = ParseNodeText(canonicalJson);
                missing["current"]!["installed_evidence"] =
                    "docs/ga-ready/evidence/missing.md";
                Assert.Throws<InvalidDataException>(() =>
                    D2CurrentEvidenceVerifier.Validate(
                        missing.ToJsonString(),
                        Repository));
                break;
            case 9:
                var block = D2CurrentEvidenceVerifier.Render(record);
                Assert.StartsWith("<!-- BEGIN GENERATED CURRENT EVIDENCE -->", block);
                Assert.Contains(record.Current.Version, block, StringComparison.Ordinal);
                Assert.Contains("Web Console and PCVCLI", block, StringComparison.Ordinal);
                Assert.Contains("tui_present=false", block, StringComparison.Ordinal);
                Assert.DoesNotContain("Web/TUI/CLI current-card", block, StringComparison.Ordinal);
                break;
            case 10:
                const string stale =
                    "# Test\n<!-- BEGIN GENERATED CURRENT EVIDENCE -->\nstale\n" +
                    "<!-- END GENERATED CURRENT EVIDENCE -->\nhistorical text";
                Assert.Throws<InvalidDataException>(() =>
                    D2CurrentEvidenceVerifier.VerifyDocument(
                        "fixture.md",
                        stale,
                        D2CurrentEvidenceVerifier.Render(record)));
                Assert.Contains("stale", stale, StringComparison.Ordinal);
                break;
            case 11:
                Assert.Equal(
                    8,
                    D2CurrentEvidenceVerifier.VerifyOwnedDocuments(record, Repository));
                var agents = Text("AGENTS.md");
                Assert.True(
                    agents.IndexOf("## 2026-07-13 historical TUI predecessor", StringComparison.Ordinal) >
                    agents.IndexOf("<!-- END GENERATED CURRENT EVIDENCE -->", StringComparison.Ordinal));
                var readme = Text("packaging/windows-desktop-node/README.md");
                Assert.True(
                    readme.IndexOf("## 2026-07-13 historical TUI predecessor", StringComparison.Ordinal) >
                    readme.IndexOf("<!-- END GENERATED CURRENT EVIDENCE -->", StringComparison.Ordinal));
                break;
            case 12:
                var project = XDocument.Parse(Text("src/DesktopNode.Api/DesktopNode.Api.csproj"));
                var item = Assert.Single(
                    project.Descendants("Content"),
                    element => (string?)element.Attribute("Include") ==
                        @"..\..\docs\ga-ready\current-evidence.json");
                Assert.Equal(@"evidence\current-evidence.json", item.Element("Link")?.Value);
                Assert.Equal("PreserveNewest", item.Element("CopyToOutputDirectory")?.Value);
                Assert.Equal("PreserveNewest", item.Element("CopyToPublishDirectory")?.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ordinal));
        }
    }

    private static void VerifyCurrentInvalidCases(string canonicalJson)
    {
        var mutations = new Action<JsonObject>[]
        {
            root => root["feature_qualification"]!["contract"] =
                "PCV-FEATURE-PROMOTION-DECISION-V1",
            root => root["feature_qualification"] = new JsonObject
            {
                ["schema_version"] = root["feature_qualification"]!["schema_version"]!.DeepClone(),
                ["Contract"] = root["feature_qualification"]!["contract"]!.DeepClone(),
                ["promotion_eligible"] = root["feature_qualification"]!["promotion_eligible"]!.DeepClone(),
                ["blockers"] = root["feature_qualification"]!["blockers"]!.DeepClone(),
            },
            root => root["feature_qualification"]!.AsObject()["unexpected"] = true,
            root => root["feature_qualification"]!["blockers"]!.AsArray()[0]!["unexpected"] = true,
            root => root["feature_qualification"]!["blockers"]!.AsArray()[0]!["feature_id"] =
                "PCV.vm.saved-lifecycle",
            root => root["feature_qualification"]!["blockers"]!.AsArray()[0]!["stage"] =
                "ACTUAL_VM_TESTED",
            root => root["feature_qualification"]!["blockers"]!.AsArray()[0]!["verdict"] = "FAIL",
            root => root["feature_qualification"]!["schema_version"] = "1",
            root => root["schema_version"] = "1",
            root => root["feature_qualification"]!["blockers"] =
                root["feature_qualification"]!["blockers"]!.AsArray()[0]!.DeepClone(),
            root => root["feature_qualification"]!["blockers"] = new JsonArray(),
            root => root["current"]!["version"] = "0.42.74-ADMIN-SMOKE",
        };

        Assert.Equal(12, mutations.Length);

        foreach (var mutate in mutations)
        {
            var value = ParseNodeText(canonicalJson);
            mutate(value);
            Assert.Throws<InvalidDataException>(() =>
                D2CurrentEvidenceVerifier.Validate(
                    value.ToJsonString(),
                    Repository));
        }
    }

    private static void VerifyFeature(int ordinal)
    {
        const string ledgerPath = "config/desktop-node-feature-evidence-ledger.json";
        var ledger = ParseDocument(ledgerPath);
        var root = ledger.RootElement;
        switch (ordinal)
        {
            case 1:
                Assert.True(File.Exists(Absolute("config/desktop-node-feature-evidence-ledger.schema.json")));
                Assert.Equal(JsonValueKind.Object, root.ValueKind);
                Assert.Contains(
                    "pcv-feature-evidence-ledger",
                    Text("config/desktop-node-feature-evidence-ledger.schema.json"),
                    StringComparison.Ordinal);
                break;
            case 2:
                VerifyFeatureLedgerShape(root);
                break;
            case 3:
                var features = root.GetProperty("features").EnumerateArray().ToArray();
                var saved = Assert.Single(
                    features,
                    feature => feature.GetProperty("feature_id").GetString() ==
                        "pcv.vm.saved-lifecycle");
                Assert.Equal("fail", saved.GetProperty("current").GetProperty("verdict").GetString());
                Assert.All(
                    features.Where(feature =>
                        feature.GetProperty("feature_id").GetString() != "pcv.vm.saved-lifecycle"),
                    feature => Assert.Equal(
                        "pass",
                        feature.GetProperty("current").GetProperty("verdict").GetString()));
                break;
            case 4:
                var failed = EvaluateFeatureDecision(
                    root,
                    ParseDocument(
                        "packaging/windows-desktop-node/tests/fixtures/feature-evidence-promotion/04274-p0-fail.json")
                        .RootElement);
                Assert.False(failed.Eligible);
                Assert.Contains(
                    failed.Blockers,
                    blocker => blocker == ("pcv.vm.saved-lifecycle", "actual_vm_tested", "fail"));
                break;
            case 5:
                var missing = EvaluateFeatureDecision(
                    root,
                    ParseDocument(
                        "packaging/windows-desktop-node/tests/fixtures/feature-evidence-promotion/04275-missing-manual-admin.json")
                        .RootElement);
                Assert.False(missing.Eligible);
                Assert.Equal(4, missing.Blockers.Count);
                Assert.All(missing.Blockers, blocker =>
                {
                    Assert.Equal("manual_admin_tested", blocker.Stage);
                    Assert.Equal("missing", blocker.Verdict);
                });
                break;
            case 6:
                var passed = EvaluateFeatureDecision(
                    root,
                    ParseDocument(
                        "packaging/windows-desktop-node/tests/fixtures/feature-evidence-promotion/04275-all-pass.json")
                        .RootElement);
                Assert.True(passed.Eligible);
                Assert.Empty(passed.Blockers);
                break;
            case 7:
                var observation = ParseDocument(
                    "packaging/windows-desktop-node/tests/fixtures/feature-evidence-promotion/04275-missing-manual-admin.json")
                    .RootElement;
                var decisions = Enumerable.Range(0, 3)
                    .Select(_ => EvaluateFeatureDecision(root, observation))
                    .ToArray();
                var hashes = decisions.Select(DecisionHash).Distinct(StringComparer.Ordinal).ToArray();
                Assert.Single(hashes);
                Assert.Equal(ExpectedFeatureIds, decisions[0].Blockers.Select(row => row.FeatureId));
                Assert.All(decisions[0].Blockers, row =>
                    Assert.Equal("manual_admin_tested", row.Stage));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ordinal));
        }
    }

    private static void VerifyFeatureLedgerShape(JsonElement root)
    {
        Assert.Equal(1, root.GetProperty("schema_version").GetInt32());
        Assert.Equal("pcv-feature-evidence-ledger-v1", root.GetProperty("contract").GetString());
        var features = root.GetProperty("features").EnumerateArray().ToArray();
        Assert.Equal(4, features.Length);
        Assert.Equal(
            ExpectedFeatureIds,
            features.Select(feature => feature.GetProperty("feature_id").GetString())
                .Order(StringComparer.Ordinal));
        Assert.Equal(4, features.Select(feature =>
            feature.GetProperty("feature_id").GetString()).Distinct(StringComparer.Ordinal).Count());
        foreach (var feature in features)
        {
            Assert.Matches(
                "^pcv\\.[a-z0-9._-]+$",
                feature.GetProperty("feature_id").GetString()!);
            var surfaces = feature.GetProperty("surfaces").EnumerateArray()
                .Select(value => value.GetString()!).ToArray();
            Assert.NotEmpty(surfaces);
            Assert.All(surfaces, surface => Assert.Contains(surface, new[] { "api", "cli", "web" }));
            Assert.NotEmpty(feature.GetProperty("operations").EnumerateArray());
            Assert.Equal(
                ExpectedStages,
                feature.GetProperty("required_stages").EnumerateArray()
                    .Select(value => value.GetString()));
            Assert.True(feature.GetProperty("candidate_required").GetBoolean());
            var current = feature.GetProperty("current");
            Assert.Equal("0.42.74-admin-smoke", current.GetProperty("version").GetString());
            Assert.Contains(
                current.GetProperty("verdict").GetString(),
                new[] { "pass", "fail", "blocked", "missing" });
            var evidence = current.GetProperty("evidence").GetString()!;
            Assert.StartsWith("docs/ga-ready/evidence/", evidence, StringComparison.Ordinal);
            Assert.True(File.Exists(Absolute(evidence)));
        }
    }

    private static FeatureDecision EvaluateFeatureDecision(
        JsonElement ledger,
        JsonElement observation)
    {
        var observed = observation.GetProperty("features").EnumerateArray()
            .ToDictionary(
                feature => feature.GetProperty("feature_id").GetString()!,
                feature => feature.GetProperty("stages").EnumerateArray()
                    .ToDictionary(
                        stage => stage.GetProperty("name").GetString()!,
                        stage => stage.GetProperty("verdict").GetString()!,
                        StringComparer.Ordinal),
                StringComparer.Ordinal);
        var blockers = new List<(string FeatureId, string Stage, string Verdict)>();
        foreach (var feature in ledger.GetProperty("features").EnumerateArray())
        {
            var featureId = feature.GetProperty("feature_id").GetString()!;
            foreach (var stage in feature.GetProperty("required_stages").EnumerateArray()
                .Select(value => value.GetString()!))
            {
                if (!observed.TryGetValue(featureId, out var stages) ||
                    !stages.TryGetValue(stage, out var verdict))
                {
                    blockers.Add((featureId, stage, "missing"));
                }
                else if (verdict != "pass")
                {
                    blockers.Add((featureId, stage, verdict));
                }
            }
        }

        return new FeatureDecision(blockers.Count == 0, blockers);
    }

    private static string DecisionHash(FeatureDecision decision)
    {
        var json = JsonSerializer.Serialize(new
        {
            schema_version = 1,
            contract = "pcv-feature-promotion-decision-v1",
            promotion_eligible = decision.Eligible,
            blockers = decision.Blockers.Select(blocker => new
            {
                feature_id = blocker.FeatureId,
                stage = blocker.Stage,
                verdict = blocker.Verdict,
            }),
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)))
            .ToLowerInvariant();
    }

    private static void VerifyJobStore(int ordinal)
    {
        const string runner =
            "packaging/windows-desktop-node/tools/Invoke-PcvJobStore04265ReaderCompatibility.ps1";
        const string writer =
            "packaging/windows-desktop-node/tools/fixtures/PcvJobStoreFixtureWriter/Program.cs";
        var source = Text(runner);
        var writerSource = Text(writer);
        switch (ordinal)
        {
            case 1:
                Assert.True(File.Exists(Absolute(runner)));
                RequireAll(source, [
                    "artifacts/admin-smoke-package-20260716-04265/host-publish/DesktopNode.Host.exe",
                    "95e219e779fce5c4fa8162aa31cd97e68370664ffd1aa465237dbdb769383c83",
                    "0.42.65-admin-smoke+4855947fe0199cedc978e8b40ffb45e96ced6876",
                    "PCV_04265_READER_FROZEN_HOST_HASH_MISMATCH",
                    "PCV_04265_READER_FROZEN_HOST_VERSION_MISMATCH",
                ]);
                Assert.True(File.Exists(Absolute(writer)));
                Assert.DoesNotContain(
                    "PCV_JOB_CANCELLED",
                    source + writerSource,
                    StringComparison.Ordinal);
                break;
            case 2:
                RequireAll(source, [
                    "request_scope = 'GET /api/v1/jobs only'",
                    "native_operation_requests = 0",
                    "host_mutation_performed = $false",
                ]);
                Assert.DoesNotMatch(
                    new Regex(
                        "msiexec|Start-Service|Stop-Service|Restart-Service|New-Service|" +
                        "Remove-Service|sc\\.exe|Get-VM|New-VM|Set-VM|Remove-VM|" +
                        "Checkpoint-VM|Restore-VMSnapshot|New-NetFirewallRule|" +
                        "Remove-NetFirewallRule|netsh\\s+http",
                        RegexOptions.IgnoreCase),
                    source);
                break;
            case 3:
                RequireAll(source, [
                    "dry-run-pinned-binary-no-listener",
                    "fixture_plans",
                    "generated_by_current_writer",
                    "queue_count",
                    "passes_planned",
                    "backup_restore_planned",
                    "manual_snapshot_assembly",
                    "admin_required",
                    "host_mutation_performed",
                ]);
                RequireAll(writerSource, ["schema_version", "queue", "jobs"]);
                break;
            case 4:
                var hashIndex = source.IndexOf(
                    "PCV_04265_READER_FROZEN_HOST_HASH_MISMATCH",
                    StringComparison.Ordinal);
                var listenerIndex = source.IndexOf("Start-Process", StringComparison.Ordinal);
                Assert.True(hashIndex >= 0);
                Assert.True(listenerIndex < 0 || hashIndex < listenerIndex);
                RequireAll(source, ["ok = $false", "host_mutation_performed = $false"]);
                break;
            case 5:
                RequireAll(source, [
                    "frozen-04265-binary-high-loopback-reader",
                    "pass_count",
                    "backup_restore_performed",
                    "jobs_json_hash_unchanged",
                    "native_operation_requests",
                    "hyperv_routes_invoked",
                    "service_mutation_performed",
                    "GET /api/v1/jobs only",
                    "queue-fifo-readonly-probe",
                    "terminal-reader",
                    "PCV_JOB_STORE_SAVE_FAILED",
                    "127.0.0.1",
                    "/api/v1/jobs",
                ]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ordinal));
        }
    }

    private static void RequireMetadata(
        string path,
        IReadOnlyDictionary<string, string> expected)
    {
        var text = Text(path);
        foreach (var entry in expected)
        {
            var pattern =
                $"(?m)^{Regex.Escape(entry.Key)}:\\s*`{Regex.Escape(entry.Value)}`\\s*$";
            Assert.Matches(
                new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                text);
        }
    }

    private static void RequireMatches(string path, IEnumerable<string> patterns)
    {
        var text = Text(path);
        foreach (var pattern in patterns)
        {
            Assert.Matches(
                new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                text);
        }
    }

    private static void RequireAll(string text, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            Assert.Contains(value, text, StringComparison.Ordinal);
        }
    }

    private static string Text(string path) => Repository.ReadUtf8Text(path);

    private static string Absolute(string path) =>
        Path.Combine(Repository.RootPath, path.Replace('/', Path.DirectorySeparatorChar));

    private static JsonObject ParseNode(string path) => ParseNodeText(Text(path));

    private static JsonObject ParseNodeText(string text) =>
        JsonNode.Parse(text)?.AsObject()
        ?? throw new InvalidDataException("PCV_DELIVERY_D2_INVALID|json|null");

    private static JsonDocument ParseDocument(string path) =>
        JsonDocument.Parse(Text(path), new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 64,
        });

    private sealed record FeatureDecision(
        bool Eligible,
        IReadOnlyList<(string FeatureId, string Stage, string Verdict)> Blockers);
}
