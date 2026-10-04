using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class TrainFactsBuilderTests
{
    [Fact]
    public void BuildsRenderableFactsForAnOrchestratedPair()
    {
        using var train = OrchestratedTrain.Create();

        var (exitCode, result) = train.RunFacts();

        Assert.Equal(0, exitCode);
        Assert.True(result.GetProperty("ok").GetBoolean(), result.ToString());
        Assert.Equal(9, result.GetProperty("documents").GetArrayLength());
        Assert.Equal(0, train.RunEvidence("--write"));
        Assert.Equal(0, train.RunEvidence("--check"));
        var update = train.ReadEvidence("update-rollback");
        Assert.Contains("## 최종 Update", update, StringComparison.Ordinal);
        Assert.Contains("-UpdateCatalogUri artifacts/pkg-new/PureCVisorDesktopNode-0.42.90-admin-smoke-update-catalog.json", update, StringComparison.Ordinal);
        Assert.Contains("| `DesktopNode.previous` | `0.42.89-admin-smoke` |", update, StringComparison.Ordinal);
        var burn = train.ReadEvidence("burn");
        Assert.DoesNotContain("preupdate_root", burn, StringComparison.Ordinal);
        Assert.Contains("사전 Update 없이", burn, StringComparison.Ordinal);
        Assert.Contains("| ARP `PureCVisor Desktop Node` | `{AAAAAAAA-0000-0000-0000-000000000001}` `0.42.90` (1개) |", burn, StringComparison.Ordinal);
        var ops = train.ReadEvidence("ops-summary");
        Assert.Contains("| VM | `1` (`pcv-keep` Off) |", ops, StringComparison.Ordinal);
        var cleanHost = train.ReadEvidence("clean-host");
        Assert.Contains("-RemoveVmOnFailure", cleanHost, StringComparison.Ordinal);
        Assert.Contains("약 135초", cleanHost, StringComparison.Ordinal);
        Assert.Contains("base_vhd_source: `explicit`", cleanHost, StringComparison.Ordinal);
        var package = train.ReadEvidence("package");
        Assert.Contains("(항목 구성은 0.42.89 ZIP과 같다).", package, StringComparison.Ordinal);
        var fullgate = train.ReadEvidence("fullgate");
        Assert.Contains("| `service-msi-hyperv-admin-smoke` | `PASS` | `0` | `1` | `300.500s` |", fullgate, StringComparison.Ordinal);
        Assert.Contains("`<user-temp>\\pcv-hyperv-api-smoke\\pcv-spike-api-1234`", fullgate, StringComparison.Ordinal);
        var card = train.ReadEvidence("current-card");
        Assert.Contains("installed_status: `installed_non_promoted_candidate`", card, StringComparison.Ordinal);
    }

    [Fact]
    public void FailsWhenAnArtifactContradictsATemplateLiteral()
    {
        using var train = OrchestratedTrain.Create();
        train.EditJson("artifacts/campaign/installed-runtime-ops-summary/summary.json",
            json => json["unauthenticated"]!["status_code"] = 200);

        var (exitCode, result) = train.RunFacts();

        Assert.Equal(2, exitCode);
        Assert.Equal("train-facts:fact-mismatch:ops-summary:unauthenticated", result.GetProperty("error_detail").GetString());
        Assert.False(File.Exists(train.FactsPath));
    }

    [Theory]
    [InlineData("package", "version", "x", "train-facts:narrative-conflict:package:version")]
    [InlineData("fullgate", "run_context", null, "train-evidence:missing-value:")]
    public void RejectsNarrativeConflictsAndMissingNarrative(string template, string key, string? value, string expectedPrefix)
    {
        using var train = OrchestratedTrain.Create();
        train.EditJson(OrchestratedTrain.InputPath, json =>
        {
            var narrative = json["narrative"]![template]!.AsObject();
            if (value is null)
            {
                narrative.Remove(key);
            }
            else
            {
                narrative[key] = value;
            }
        });

        var (exitCode, result) = train.RunFacts();

        Assert.Equal(2, exitCode);
        Assert.StartsWith(expectedPrefix, result.GetProperty("error_detail").GetString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("package_root", "D:/outside")]
    [InlineData("campaign_root", "artifacts/../docs")]
    [InlineData("unknown_source", "x")]
    public void RejectsSourcesOutsideArtifacts(string key, string value)
    {
        using var train = OrchestratedTrain.Create();
        train.EditJson(OrchestratedTrain.InputPath, json => json["sources"]![key] = value);

        var (exitCode, result) = train.RunFacts();

        Assert.Equal(2, exitCode);
        Assert.StartsWith("train-facts:source-", result.GetProperty("error_detail").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public void KeepsOtherTemplateDocumentsAlreadyInTheFactsFile()
    {
        using var train = OrchestratedTrain.Create();
        train.Write(OrchestratedTrain.FactsRelative, """
            {"schema_version":1,"contract":"pcv-train-evidence-facts-v1","version":"0.42.90-admin-smoke","documents":[
            {"template":"main-push","path":"docs/ga-ready/evidence/main-push-x.md","values":{"k":"v"}},
            {"template":"package","path":"docs/ga-ready/evidence/old-package.md","values":{"k":"v"}}]}
            """);

        Assert.Equal(0, train.RunFacts().ExitCode);

        var facts = TrainEvidenceFactsReader.Parse(File.ReadAllText(train.FactsPath));
        Assert.Equal(10, facts.Documents.Count);
        Assert.Equal("main-push", facts.Documents[^1].Template);
        Assert.DoesNotContain(facts.Documents, document => document.Path == "docs/ga-ready/evidence/old-package.md");
    }

    private sealed class OrchestratedTrain : IDisposable
    {
        internal const string InputPath = "artifacts/train-facts-input.json";
        internal const string FactsRelative = "docs/ga-ready/trains/0.42.90-admin-smoke.evidence-facts.json";
        private const string V = "0.42.90-admin-smoke";
        private const string B = "0.42.89-admin-smoke";
        private const string Commit = "1111111222222233333334444444555555566666";
        private const string BaselineCommit = "9999999888888877777776666666555555544444";
        private const string ArpCode = "{AAAAAAAA-0000-0000-0000-000000000001}";
        private const string FinalArpCode = "{BBBBBBBB-0000-0000-0000-000000000002}";
        private static readonly string HostSha = new('a', 64);
        private static readonly string CliSha = new('b', 64);

        private OrchestratedTrain(string root) => Root = root;

        internal string Root { get; }

        internal string FactsPath => Path.Combine(Root, FactsRelative.Replace('/', Path.DirectorySeparatorChar));

        internal static OrchestratedTrain Create()
        {
            var train = new OrchestratedTrain(Path.Combine(Path.GetTempPath(), "pcv-train-facts-" + Guid.NewGuid().ToString("N")));
            train.Write("src/DesktopNode.sln", string.Empty);
            train.Write("config/development-verification-suites.json", "{}");
            var templates = Path.Combine(VerificationCatalogFixture.RepositoryRoot, "docs", "ga-ready", "trains", "templates");
            foreach (var file in Directory.GetFiles(templates))
            {
                train.Write("docs/ga-ready/trains/templates/" + Path.GetFileName(file), File.ReadAllText(file));
            }

            Directory.CreateDirectory(Path.Combine(train.Root, "docs", "ga-ready", "evidence"));
            train.WritePackages();
            train.WriteCampaign();
            train.WriteFullgate();
            train.WriteCard();
            train.WriteInput();
            return train;
        }

        internal (int ExitCode, JsonElement Result) RunFacts()
        {
            using var output = new StringWriter();
            var exitCode = TrainFactsCommand.Run(["train-facts", "--input", InputPath], Root, output);
            return (exitCode, JsonDocument.Parse(output.ToString()).RootElement.Clone());
        }

        internal int RunEvidence(string mode)
        {
            using var output = new StringWriter();
            return TrainEvidenceCommand.Run(["train-evidence", "--facts", FactsRelative, mode], Root, output);
        }

        internal string ReadEvidence(string template) =>
            File.ReadAllText(Path.Combine(Root, "docs", "ga-ready", "evidence", $"{template}-2026-10-11-04290.md"));

        internal void EditJson(string relativePath, Action<JsonObject> edit)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var json = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            edit(json);
            File.WriteAllText(path, json.ToJsonString());
        }

        internal void Write(string relativePath, string content)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }

        private void WriteJson(string relativePath, object value) => Write(relativePath, JsonSerializer.Serialize(value));

        private static object PackageFacts(string version, string commit) => new Dictionary<string, object?>
        {
            ["schema_version"] = 1, ["contract"] = "pcv-admin-smoke-package-facts-v1", ["version"] = version,
            ["msi_product_version"] = version.Replace("-admin-smoke", string.Empty), ["provenance_commit"] = commit,
            ["build_utc"] = "2026-10-11T01:00:00Z", ["build_seconds"] = 40, ["wix_version"] = "5.0.2+aa65968c",
            ["msi_sha256"] = new string('1', 64), ["payload_aggregate_sha256"] = new string('2', 64), ["payload_file_count"] = 8,
            ["product_wrapper_sha256"] = new string('3', 64), ["service_host_sha256"] = new string('4', 64), ["cli_sha256"] = new string('5', 64),
            ["host_product_version"] = version + "+" + commit, ["cli_product_version"] = version + "+" + commit, ["manifest_version"] = version,
            ["update_zip_sha256"] = new string('6', 64), ["update_zip_entries"] = new[] { "DesktopNode.Host.exe", "pcvcli.exe" },
            ["msi_upgrade_rows"] = new object[]
            {
                new Dictionary<string, object> { ["version_min"] = "", ["version_max"] = version.Replace("-admin-smoke", string.Empty), ["attributes"] = 513 },
                new Dictionary<string, object> { ["version_min"] = version.Replace("-admin-smoke", string.Empty), ["version_max"] = "", ["attributes"] = 2 }
            },
            ["remove_existing_products_sequence"] = 1401, ["host_mutation_performed"] = false
        };

        private void WritePackages()
        {
            WriteJson("artifacts/pkg-new/package-facts.json", PackageFacts(V, Commit));
            WriteJson("artifacts/pkg-old/package-facts.json", PackageFacts(B, BaselineCommit));
        }

        private static Dictionary<string, object?> Observation(string point, int second, string manifest, string? previous, string? failed)
        {
            var commit = manifest == V ? Commit : BaselineCommit;
            return new Dictionary<string, object?>
            {
                ["point"] = point, ["at"] = $"2026-10-11T02:{second / 60:00}:{second % 60:00}.0000000+00:00",
                ["manifest_version"] = manifest, ["host_product_version"] = manifest + "+" + commit,
                ["previous_version"] = previous, ["failed_version"] = failed, ["service_state"] = "Running", ["service_start_mode"] = "Auto",
                ["web_status"] = 200, ["boot_time"] = "2026-10-10T00:00:00.0000000Z", ["firewall_rule_count"] = 0,
                ["vms"] = new[] { new Dictionary<string, string> { ["name"] = "pcv-keep", ["state"] = "Off" } },
                ["arp"] = new[] { new Dictionary<string, string> { ["product_code"] = ArpCode, ["display_version"] = manifest.Replace("-admin-smoke", string.Empty) } }
            };
        }

        private void WriteCampaign()
        {
            const string c = "artifacts/campaign/";
            WriteJson(c + "observations.json", new Dictionary<string, object>
            {
                ["schema_version"] = 1, ["contract"] = "pcv-manual-admin-pair-observations-v1", ["campaign_id"] = "campaign",
                ["entries"] = new[]
                {
                    Observation("start", 0, B, null, null),
                    Observation("before:lifecycle-product-update-rollback", 10, B, null, null),
                    Observation("after:lifecycle-update", 40, V, B, null),
                    Observation("after:lifecycle-rollback", 70, B, null, V),
                    Observation("after:lifecycle-product-update-rollback", 100, V, B, V),
                    Observation("before:clean-host-windows-update", 100, V, B, V),
                    Observation("after:clean-host-windows-update", 235, V, B, V),
                    Observation("before:burn-bootstrapper-lifecycle", 235, V, B, V),
                    Observation("after:burn-bootstrapper-lifecycle", 270, V, B, V),
                    Observation("before:msix-package-lifecycle-smoke", 270, V, B, V),
                    Observation("after:msix-package-lifecycle-smoke", 290, V, B, V),
                    Observation("before:installed-runtime-ops-summary", 290, V, B, V),
                    Observation("after:installed-runtime-ops-summary", 295, V, B, V)
                }
            });
            const string lifecycle = c + "lifecycle/product-update-rollback/";
            WriteJson(lifecycle + "update-summary.json", new { ok = true, action = "Update", installed_version = V, service_status = "Running" });
            WriteJson(lifecycle + "rollback-summary.json", new { ok = true, action = "Rollback", installed_version = B, service_status = "Running" });
            WriteJson(lifecycle + "final-update-summary.json", new { ok = true, action = "Update", installed_version = V, service_status = "Running" });
            WriteJson(c + "clean-host-windows-update/summary.json", new Dictionary<string, object?>
            {
                ["ok"] = true, ["internal_clean_host_install_update_rollback_smoke"] = "pass", ["blocker"] = "none",
                ["install_exit_code"] = 0, ["update_exit_code"] = 0, ["rollback_exit_code"] = 0,
                ["baseline_manifest_version"] = B, ["updated_manifest_version"] = V, ["final_manifest_version"] = B,
                ["final_service"] = new { state = "Running", start_mode = "Auto" }, ["final_web_status_code"] = 200,
                ["failed_root_exists_after_rollback"] = true, ["token_value_observed"] = false, ["base_vhd_source"] = "explicit",
                ["base_vhd_path"] = "D:\\cache\\20348.5622-20260930.vhd", ["base_vhd_ubr"] = 5622, ["vm_switch_name"] = "Default Switch",
                ["vm_name"] = "pcv-campaign", ["baseline_msi_sha256"] = new string('7', 64), ["update_package_sha256"] = new string('6', 64),
                ["powershell_direct"] = new { ok = true, attempts = 2 },
                ["windows_update_preparation"] = new { reboot_performed = false, update = new { update_count = 0 } }
            });
            WriteJson(c + "burn-bootstrapper-lifecycle/summary.json", new Dictionary<string, object?>
            {
                ["ok"] = true, ["status"] = "PASS", ["restoration_status"] = "PASS", ["final_manifest_version"] = V, ["final_service_status"] = "Running",
                ["exits"] = new Dictionary<string, int> { ["build"] = 0, ["install"] = 0, ["repair"] = 0, ["remove"] = 0, ["restore-target-msi"] = 0 }
            });
            Write(c + $"burn-bootstrapper-lifecycle/PureCVisorDesktopNode-{V}-bootstrapper.exe", "bundle");
            WriteJson(c + "msix-package-lifecycle-smoke/summary.json", new Dictionary<string, object?>
            {
                ["ok"] = true, ["status"] = "PASS",
                ["exits"] = new Dictionary<string, int> { ["pack-baseline"] = 0, ["sign-baseline"] = 0, ["verify-baseline"] = 0, ["pack-target"] = 0, ["sign-target"] = 0, ["verify-target"] = 0 },
                ["cleanup_status"] = "PASS", ["cleanup_probe_status"] = "PASS", ["final_absence_probe_status"] = "PASS",
                ["final_package_absent"] = true, ["final_smoke_service_absent"] = true, ["target_manifest_unchanged"] = true,
                ["final_msi_service_status"] = "Running", ["final_msi_service_start_type"] = "Automatic"
            });
            Write(c + "msix-package-lifecycle-smoke/baseline.msix", "baseline");
            Write(c + "msix-package-lifecycle-smoke/target.msix", "target");
            WriteJson(c + "installed-runtime-ops-summary/summary.json", new Dictionary<string, object?>
            {
                ["ok"] = true, ["status"] = "PASS", ["error"] = null, ["installed_version"] = V, ["service_status"] = "Running",
                ["operation"] = "ops.summary", ["ops_summary_sha256"] = new string('8', 64), ["stderr_bytes"] = 0, ["errors_count"] = 0,
                ["vm_total"] = 1, ["token_like_count"] = 0, ["unauthenticated"] = new { status_code = 401, error_code = "PCV_AUTH_REQUIRED" }
            });
            WriteJson(c + "manual-admin-rebaseline-readiness/summary.json", new { package_pair_input_status = "ready-current-baseline-target-package-pair" });
            WriteJson(c + "manual-admin-campaign-descriptor/summary.json", new Dictionary<string, object?>
            {
                ["ok"] = true, ["overall_status"] = "pass", ["runner_count"] = 6, ["missing_count"] = 0, ["not_pass_count"] = 0,
                ["plan_only"] = true, ["host_mutation_performed"] = false, ["descriptor_batch_id"] = "campaign-closed",
                ["release_candidate"] = new { next_candidate_version = V }
            });
            Write(c + "manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json", "{}");
        }

        private void WriteFullgate()
        {
            const string id = "full-admin-host-mutation-gate-20261011-04290";
            const string route = "artifacts/routeparity-service-msi-hyperv-batch-profile-20261011-04290/";
            WriteJson($"artifacts/batch-manifests/{id}.result.json", new Dictionary<string, object?> { ["ok"] = true, ["status"] = "completed", ["executed_steps"] = 2, ["failed_step_id"] = null });
            WriteJson($"artifacts/batch-runs/{id}/summary.json", new { ok = true });
            WriteJson($"artifacts/batch-runs/{id}/step-results/001-service-msi-hyperv-admin-smoke.json", new { exit_code = 0, attempt_count = 1, duration_ms = 300500 });
            WriteJson($"artifacts/batch-runs/{id}/step-results/002-os-mutation-gate.json", new { exit_code = 0, attempt_count = 1, duration_ms = 11000 });
            WriteJson("artifacts/os-mutation-gates-batch-profile-20261011-04290/summary.json", new { ok = true });
            WriteJson(route + "summary.json", new { ok = true });
            WriteJson(route + "same-version-preflight.json", new { residual_product_codes = new[] { ArpCode }, same_version_upgrade_expected = true });
            WriteJson(route + "msi-lifecycle-smoke.json", new Dictionary<string, object?>
            {
                ["ok"] = true, ["boot_time_unchanged"] = true,
                ["steps"] = new[] { "install", "repair", "uninstall-preserve", "install-remove-data", "uninstall-remove-data", "final-restore-install" }
                    .Select(name => new { name, exit_code = 0 }).ToArray(),
                ["installed_build"] = new { ok = true, installed_commit = Commit, gate_commit = Commit },
                ["same_version_arp"] = new { ok = true, product_codes = new[] { FinalArpCode } }
            });
            WriteJson(route + $"PureCVisorDesktopNode-{V}-windows-x64.provenance.json", new Dictionary<string, object?>
            {
                ["git_commit"] = Commit, ["msi"] = new { sha256 = new string('c', 64) },
                ["payload"] = new { aggregate_sha256 = new string('d', 64), product_wrapper_sha256 = new string('3', 64) },
                ["service_host"] = new { sha256 = HostSha }, ["cli"] = new { sha256 = CliSha }
            });
            WriteJson(route + "hyperv-api-route-smoke.json", new Dictionary<string, object?>
            {
                ["vm_delete"] = new
                {
                    job = new
                    {
                        storage_cleanup = new
                        {
                            configuration_root = "C:\\Temp\\pcv-hyperv-api-smoke\\pcv-spike-api-1234",
                            removed_files = new[] { "C:\\Temp\\pcv-hyperv-api-smoke\\pcv-spike-api-1234\\disk0.vhdx" },
                            removed_directories = new[]
                            {
                                "C:\\Temp\\pcv-hyperv-api-smoke\\pcv-spike-api-1234\\Snapshots",
                                "C:\\Temp\\pcv-hyperv-api-smoke\\pcv-spike-api-1234\\Virtual Machines",
                                "C:\\Temp\\pcv-hyperv-api-smoke\\pcv-spike-api-1234"
                            },
                            retained = Array.Empty<string>()
                        }
                    }
                }
            });
            var log = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes($"Property(S): WIX_UPGRADE_DETECTED property. Its value is '{ArpCode}'.\r\n")).ToArray();
            foreach (var name in new[] { "install.log", "uninstall-preserve.log", "uninstall-remove-data.log", "final-restore-install.log" })
            {
                var path = Path.Combine(Root, route.Replace('/', Path.DirectorySeparatorChar), "msi-logs", name);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, log);
            }
        }

        private void WriteCard()
        {
            WriteJson("artifacts/card/summary.json", new Dictionary<string, object?>
            {
                ["status"] = "pass", ["version"] = V, ["installed_manifest_version"] = V, ["installed_product_version"] = V + "+" + Commit,
                ["display_version"] = "0.42.90", ["arp_entry_count"] = 1, ["tui_present"] = false, ["fullgate_batch"] = "full-admin-host-mutation-gate-20261011-04290",
                ["clean_package_msi_sha256"] = new string('1', 64), ["operational_fullgate_msi_sha256"] = new string('c', 64),
                ["clean_package_payload_aggregate_sha256"] = new string('2', 64), ["operational_fullgate_payload_aggregate_sha256"] = new string('d', 64),
                ["provenance_commit"] = Commit, ["cli_exit_zero_count"] = 3, ["cli_json_ok_count"] = 3, ["web_http_200_count"] = 2,
                ["service_state"] = "Running/Auto", ["service_start_name"] = "LocalSystem", ["service_uses_credential_manager"] = true,
                ["service_has_raw_or_protected_token_flag"] = false, ["remaining_test_vm_count"] = 0, ["installed_cli_sha256"] = CliSha,
                ["installed_host_sha256"] = HostSha, ["operational_payload_cli_match"] = true, ["operational_payload_host_match"] = true,
                ["clean_payload_host_match"] = false, ["secret_observed"] = false, ["promotion_ledger_status"] = "not-promoted",
                ["canonical_current_evidence"] = B, ["launched_at"] = "2026-10-11T03:00:00Z"
            });
            Write("artifacts/card/capture-current-card.ps1", "# capture");
        }

        private void WriteInput()
        {
            var documents = TrainFactsInput.BuildableTemplates.ToDictionary(
                template => template, template => $"docs/ga-ready/evidence/{template}-2026-10-11-04290.md");
            WriteJson(InputPath, new Dictionary<string, object>
            {
                ["schema_version"] = 1, ["contract"] = "pcv-train-facts-input-v1", ["version"] = V, ["baseline_version"] = B,
                ["canonical_current_evidence"] = B, ["date"] = "2026-10-11", ["documents"] = documents,
                ["sources"] = new Dictionary<string, string>
                {
                    ["package_root"] = "artifacts/pkg-new", ["baseline_package_root"] = "artifacts/pkg-old", ["campaign_root"] = "artifacts/campaign",
                    ["target_update_catalog"] = $"artifacts/pkg-new/PureCVisorDesktopNode-{V}-update-catalog.json",
                    ["fullgate_batch_id"] = "full-admin-host-mutation-gate-20261011-04290", ["iso_path"] = "artifacts/smoke-media/route.iso",
                    ["current_card_root"] = "artifacts/card", ["signer_thumbprint"] = "ABCDEF", ["fullgate_final_firewall_rule_count"] = "0"
                },
                ["narrative"] = new Dictionary<string, Dictionary<string, string>>
                {
                    ["package"] = new() { ["background"] = "배경 문장.", ["train_carriages"] = "`PR #40`", ["lane2_probes"] = "`sample`" },
                    ["fullgate"] = new() { ["run_context"] = "실행 문장.", ["vm_summary"] = "`pcv-keep` Off만 남음" },
                    ["current-card"] = new() { ["script_note"] = "스크립트 문장." }
                }
            });
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}
