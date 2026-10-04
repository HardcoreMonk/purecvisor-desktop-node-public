using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

// Builds the generated values of the pair evidence documents of one orchestrated train. Every template literal that
// states a result (exit 0, HTTP 401, Upgrade row 513, ...) is checked against the artifacts here; a mismatch fails.
internal sealed class TrainFactsBuilder(string repositoryRoot, TrainFactsInput input)
{
    private const string Orchestrator = "pair orchestrator";
    private static readonly Regex UpgradeDetected = new("WIX_UPGRADE_DETECTED property\\. Its value is '([^']+)'", RegexOptions.CultureInvariant);

    private IReadOnlyList<JsonObject>? observations;

    private string V => input.Version;
    private string B => input.BaselineVersion;
    private string Train => Display(V);

    internal Dictionary<string, string> Build(string template) => template switch
    {
        "package" => Package(),
        "ops-summary" => OpsSummary(),
        "update-rollback" => UpdateRollback(),
        "clean-host" => CleanHost(),
        "burn" => Burn(),
        "msix" => Msix(),
        "pair-descriptor" => PairDescriptor(),
        "fullgate" => Fullgate(),
        "current-card" => CurrentCard(),
        _ => throw TrainFactsInput.Invalid("template-unknown", template)
    };

    private Dictionary<string, string> Package()
    {
        var facts = PackageFacts(Source("package_root"));
        Require(facts.Str("version") == V, "package", "version");
        Require(facts.Str("wix_version").StartsWith("5.0.2", StringComparison.Ordinal), "package", "wix-version");
        Require(facts.Long("remove_existing_products_sequence") == 1401, "package", "remove-existing-products");
        Require(facts.Array("msi_upgrade_rows").OfType<JsonObject>().Any(row =>
            TrainFactsFiles.Scalar(row["version_max"]) == facts.Str("msi_product_version") && TrainFactsFiles.Scalar(row["attributes"]) == "513"),
            "package", "upgrade-row");
        Require(facts.Str("host_product_version") == facts.Str("cli_product_version"), "package", "cli-product-version");
        Require(facts.Str("host_product_version") == V + "+" + facts.Str("provenance_commit"), "package", "host-product-version");

        var count = facts.Str("payload_file_count");
        var zipNote = $"update ZIP은 `New-PcvAdminSmokeUpdatePackage.ps1`가 payload {count}개 파일을 root에 두고 deflate와 `/` 구분자로 만들었다";
        if (input.Sources.TryGetValue("baseline_package_root", out var baselineRoot) &&
            File.Exists(TrainFactsFiles.Full(repositoryRoot, baselineRoot + "/package-facts.json")))
        {
            var same = PackageFacts(baselineRoot).Array("update_zip_entries").Select(TrainFactsFiles.Scalar)
                .SequenceEqual(facts.Array("update_zip_entries").Select(TrainFactsFiles.Scalar));
            zipNote += $"(항목 구성은 {Display(B)} ZIP과 {(same ? "같다" : "다르다")})";
        }

        var values = Common("package");
        Add(values, "source_commit", facts.Str("provenance_commit"));
        Add(values, "release_train", V);
        Add(values, "artifact_root", Source("package_root"));
        Add(values, "clean_package_msi_sha256", facts.Str("msi_sha256"));
        Add(values, "clean_package_payload_aggregate_sha256", facts.Str("payload_aggregate_sha256"));
        Add(values, "product_wrapper_sha256", facts.Str("product_wrapper_sha256"));
        Add(values, "service_host_sha256", facts.Str("service_host_sha256"));
        Add(values, "cli_sha256", facts.Str("cli_sha256"));
        Add(values, "update_zip_sha256", facts.Str("update_zip_sha256"));
        Add(values, "payload_file_count", count);
        Add(values, "wix_version", facts.Str("wix_version"));
        Add(values, "build_utc", facts.Str("build_utc"));
        Add(values, "msi_product_version", facts.Str("msi_product_version"));
        Add(values, "zip_note", zipNote + ".");
        Add(values, "host_product_version", facts.Str("host_product_version"));
        if (facts.OptStr("build_seconds") is { } seconds)
        {
            Add(values, "build_seconds", seconds);
        }

        return values;
    }

    private Dictionary<string, string> OpsSummary()
    {
        var root = Campaign("installed-runtime-ops-summary");
        var summary = Load(root + "/summary.json");
        var after = Observation("after:installed-runtime-ops-summary");
        Require(summary.Bool("ok") && summary.Str("status") == "PASS" && summary.Get("error") is null, "ops-summary", "result");
        Require(summary.Str("operation") == "ops.summary" && summary.Long("errors_count") == 0, "ops-summary", "operation-or-errors");
        Require(summary.Long("unauthenticated", "status_code") == 401 && summary.OptStr("unauthenticated", "error_code") == "PCV_AUTH_REQUIRED",
            "ops-summary", "unauthenticated");
        Require(summary.Long("token_like_count") == 0, "ops-summary", "token-like-output");
        Require(summary.Str("installed_version") == V, "ops-summary", "installed-version");
        RequireRunning(after, "ops-summary");

        var values = Common("ops-summary");
        Add(values, "installed_version", V);
        Add(values, "artifact_root", root);
        Add(values, "summary_sha256", summary.Str("ops_summary_sha256"));
        Add(values, "context", $"release train `{Train}`의 `{Display(B)} -> {Train}` {Orchestrator}가 마지막 bucket으로 runtime ops를 캡처했다. target `{V}`(Host `+{Short(HostCommit(after))}`)가 설치된 상태다.");
        Add(values, "capture_note", $"설치본 `pcvcli.exe`가 기본 보호 token 파일로 `ops summary`를 읽었다. 결과는 CLI exit `0`, 응답 `ok=true`, operation `ops.summary`, stderr `{summary.Str("stderr_bytes")}` byte다. token 값은 출력하거나 기록하지 않았고, summary에 token 형태 문자열은 `0`개다.");
        Add(values, "installed_product_version", Str(after, "host_product_version"));
        Add(values, "vm_summary", $"`{summary.Str("vm_total")}` ({VmSummary(after)})");
        return values;
    }

    private Dictionary<string, string> UpdateRollback()
    {
        var root = Campaign("lifecycle/product-update-rollback");
        var update = Load(root + "/update-summary.json");
        var rollback = Load(root + "/rollback-summary.json");
        var final = Load(root + "/final-update-summary.json");
        var before = Observation("before:lifecycle-product-update-rollback");
        var afterUpdate = Observation("after:lifecycle-update");
        var after = Observation("after:lifecycle-product-update-rollback");
        Require(update.Bool("ok") && update.Str("installed_version") == V && update.Str("service_status") == "Running", "update-rollback", "update");
        Require(rollback.Bool("ok") && rollback.Str("installed_version") == B && rollback.Str("service_status") == "Running", "update-rollback", "rollback");
        Require(final.Bool("ok") && final.Str("installed_version") == V && final.Str("service_status") == "Running", "update-rollback", "final-update");
        Require(Str(before, "manifest_version") == B, "update-rollback", "baseline-installed");
        Require(Str(afterUpdate, "manifest_version") == V, "update-rollback", "after-update-manifest");
        RequireRunning(afterUpdate, "update-rollback");
        RequireRunning(after, "update-rollback");
        Require(Str(after, "manifest_version") == V, "update-rollback", "final-manifest");
        Require(Str(before, "boot_time") == Str(after, "boot_time"), "update-rollback", "reboot");

        var facts = PackageFacts(Source("package_root"));
        var catalog = Source("target_update_catalog");
        var updateCommand = $"command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri {catalog} -UpdateChannel admin-smoke";
        var values = Common("update-rollback");
        Add(values, "baseline_version", B);
        Add(values, "artifact_root", root);
        Add(values, "source_payload", catalog);
        Add(values, "target_msi_sha256", facts.Str("msi_sha256"));
        Add(values, "target_provenance_commit", facts.Str("provenance_commit"));
        Add(values, "update_summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, update.RelativePath));
        Add(values, "rollback_summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, rollback.RelativePath));
        Add(values, "prestate", $"release train `{Train}` {Orchestrator}의 update/rollback bucket이다. 시작 전 설치본은 baseline `{B}`(Host `+{Short(HostCommit(before))}`)였다. `DesktopNode.previous`: {Cell(before, "previous_version")}, `DesktopNode.failed`: {Cell(before, "failed_version")}, service {Str(before, "service_state")}/{StartMode(before)}, VM {VmSummary(before)}.");
        Add(values, "update_command", updateCommand);
        Add(values, "update_executed", "summary: update-summary.json");
        Add(values, "host_after_update", Str(afterUpdate, "host_product_version"));
        Add(values, "rollback_command", "command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback");
        Add(values, "rollback_executed", "summary: rollback-summary.json");
        Add(values, "final_update_command", updateCommand);
        Add(values, "final_update_state", $"summary: final-update-summary.json, manifest {V}");
        Add(values, "final_manifest_version", V);
        Add(values, "final_host_product_version", Str(after, "host_product_version"));
        Add(values, "final_failed_version", Opt(after, "failed_version") ?? "none");
        Add(values, "final_previous_cell", Cell(after, "previous_version"));
        Add(values, "vm_cell", VmSummary(before) == VmSummary(after) ? "변화 없음" : VmSummary(after));
        return values;
    }

    private Dictionary<string, string> CleanHost()
    {
        var root = Campaign("clean-host-windows-update");
        var summary = Load(root + "/summary.json");
        var after = Observation("after:clean-host-windows-update");
        Require(summary.Bool("ok") && summary.Str("internal_clean_host_install_update_rollback_smoke") == "pass" && summary.Str("blocker") == "none",
            "clean-host", "result");
        Require(summary.Long("install_exit_code") == 0 && summary.Long("update_exit_code") == 0 && summary.Long("rollback_exit_code") == 0,
            "clean-host", "exit-codes");
        Require(summary.Str("baseline_manifest_version") == B && summary.Str("updated_manifest_version") == V && summary.Str("final_manifest_version") == B,
            "clean-host", "manifests");
        Require(summary.Str("final_service", "state") == "Running" && summary.Str("final_service", "start_mode") == "Auto" &&
            summary.Long("final_web_status_code") == 200, "clean-host", "final-service");
        Require(summary.Bool("failed_root_exists_after_rollback") && !summary.Bool("token_value_observed") &&
            summary.Str("base_vhd_source") is "current-base" or "explicit" && summary.Bool("powershell_direct", "ok"), "clean-host", "state");
        var vmName = summary.Str("vm_name");
        Require(!after.ContainsVm(vmName), "clean-host", "vm-removed");

        var baseFile = Path.GetFileName(summary.Str("base_vhd_path"));
        var values = Common("clean-host");
        Add(values, "baseline_version", B);
        Add(values, "artifact_root", root);
        Add(values, "summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, summary.RelativePath));
        Add(values, "vm_name", vmName);
        Add(values, "base_vhd_source", summary.Str("base_vhd_source"));
        Add(values, "base_vhd_file", baseFile);
        Add(values, "baseline_msi_sha256", summary.Str("baseline_msi_sha256"));
        Add(values, "update_package_sha256", summary.Str("update_package_sha256"));
        Add(values, "context", $"release train `{Train}` {Orchestrator}의 clean-host bucket이다. runner는 `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`이고 약 {Seconds("clean-host-windows-update")}초 걸렸다.");
        Add(values, "baseline_msi_path", $"{Source("baseline_package_root")}/PureCVisorDesktopNode-{B}-windows-x64.msi");
        Add(values, "update_package_path", $"{Source("package_root")}/PureCVisorDesktopNode-{V}-update.zip");
        Add(values, "runner_extra_line", $"-VmRoot {root}/vm -BaseVhdPath <image-cache>/{baseFile} -VMSwitchName {summary.Str("vm_switch_name")}");
        Add(values, "runner_flags_line", "-UpdateChannel admin-smoke -TargetSigningMode AllowUnsignedDev -InstallWindowsUpdates -RemoveVmOnSuccess -RemoveVmOnFailure");
        Add(values, "base_note", $"base VHD는 {Orchestrator}가 `-BaseVhdPath`로 넘겼다(UBR `{summary.Str("base_vhd_ubr")}`). guest 인증 정보는 실행 경계에서 만들어 넘겼고 기록하지 않았다.");
        Add(values, "psdirect_attempts", summary.Str("powershell_direct", "attempts"));
        Add(values, "windows_update_row", $"| Windows Update | 대상 `{summary.Str("windows_update_preparation", "update", "update_count")}`개, 재부팅 {(summary.Bool("windows_update_preparation", "reboot_performed") ? "있음" : "없음")} |");
        Add(values, "host_state_note", $"이 호스트의 설치본은 `{Str(after, "manifest_version")}`로 남았다. VM은 {VmSummary(after)}다.");
        return values;
    }

    private Dictionary<string, string> Burn()
    {
        var root = Campaign("burn-bootstrapper-lifecycle");
        var summary = Load(root + "/summary.json");
        var after = Observation("after:burn-bootstrapper-lifecycle");
        Require(summary.Bool("ok") && summary.Str("status") == "PASS" && summary.Str("restoration_status") == "PASS", "burn", "result");
        Require(summary.Str("final_manifest_version") == V && summary.Str("final_service_status") == "Running", "burn", "final-state");
        var exits = summary.Get("exits") as JsonObject ?? throw TrainFactsInput.Invalid("artifact-field-missing", summary.RelativePath + ":exits");
        Require(exits.Select(pair => pair.Key).Order(StringComparer.Ordinal).SequenceEqual(["build", "install", "remove", "repair", "restore-target-msi"]) &&
            exits.All(pair => TrainFactsFiles.Scalar(pair.Value) == "0"), "burn", "exits");
        RequireRunning(after, "burn");
        var arp = after.Arp();
        Require(arp.Count == 1 && arp[0].DisplayVersion == Display(V) && Str(after, "manifest_version") == V, "burn", "arp");

        var bundle = $"{root}/PureCVisorDesktopNode-{V}-bootstrapper.exe";
        var values = Common("burn");
        Add(values, "artifact_root", root);
        Add(values, "bundle", bundle);
        Add(values, "bundle_sha256", TrainFactsFiles.Sha256(repositoryRoot, bundle));
        Add(values, "bundle_bytes", TrainFactsFiles.Length(repositoryRoot, bundle).ToString(CultureInfo.InvariantCulture));
        Add(values, "summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, summary.RelativePath));
        Add(values, "wix_version", PackageFacts(Source("package_root")).Str("wix_version"));
        Add(values, "context", $"release train `{Train}` {Orchestrator}의 Burn bucket이다. 직전 update/rollback bucket이 target `{V}`로 끝나 사전 Update 없이 `Invoke-PcvBurnBootstrapperLifecycle.ps1 -Execute`를 실행했다(약 `{Seconds("burn-bootstrapper-lifecycle")}`초).");
        Add(values, "target_msi_path", $"{Source("package_root")}/PureCVisorDesktopNode-{V}-windows-x64.msi");
        Add(values, "runner_tail_line", "-ProductHelperPath packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -InstalledManifestPath <installed product-manifest.json> -ServiceName PureCVisorDesktopNode -Execute");
        Add(values, "arp_entry", $"`{arp[0].ProductCode}` `{arp[0].DisplayVersion}` (1개)");
        Add(values, "installed_product_version", Str(after, "host_product_version"));
        Add(values, "vm_summary", VmSummary(after));
        return values;
    }

    private Dictionary<string, string> Msix()
    {
        var root = Campaign("msix-package-lifecycle-smoke");
        var summary = Load(root + "/summary.json");
        var after = Observation("after:msix-package-lifecycle-smoke");
        var exits = summary.Get("exits") as JsonObject ?? throw TrainFactsInput.Invalid("artifact-field-missing", summary.RelativePath + ":exits");
        Require(summary.Bool("ok") && summary.Str("status") == "PASS" && exits.Count == 6 && exits.All(pair => TrainFactsFiles.Scalar(pair.Value) == "0"),
            "msix", "result");
        Require(summary.Str("cleanup_status") == "PASS" && summary.Str("cleanup_probe_status") == "PASS" && summary.Str("final_absence_probe_status") == "PASS" &&
            summary.Bool("final_package_absent") && summary.Bool("final_smoke_service_absent") && summary.Bool("target_manifest_unchanged"), "msix", "cleanup");
        Require(summary.Str("final_msi_service_status") == "Running" && summary.Str("final_msi_service_start_type") == "Automatic" &&
            Str(after, "manifest_version") == V, "msix", "final-state");

        var values = Common("msix");
        Add(values, "baseline_display_version", Display(B));
        Add(values, "display_version", Train);
        Add(values, "artifact_root", root);
        Add(values, "summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, summary.RelativePath));
        Add(values, "baseline_msix_sha256", TrainFactsFiles.Sha256(repositoryRoot, root + "/baseline.msix"));
        Add(values, "baseline_msix_bytes", TrainFactsFiles.Length(repositoryRoot, root + "/baseline.msix").ToString(CultureInfo.InvariantCulture));
        Add(values, "target_msix_sha256", TrainFactsFiles.Sha256(repositoryRoot, root + "/target.msix"));
        Add(values, "target_msix_bytes", TrainFactsFiles.Length(repositoryRoot, root + "/target.msix").ToString(CultureInfo.InvariantCulture));
        Add(values, "signer_thumbprint", Source("signer_thumbprint"));
        Add(values, "context", $"release train `{Train}` {Orchestrator}의 MSIX bucket이다. 분리된 패키지 ID `PureCVisor.DesktopNode.MsixSmoke`로 두 버전을 pack했다. 내부 코드 서명 인증서로 서명·검증한 뒤 설치, 갱신, 제거했다. 약 `{Seconds("msix-package-lifecycle-smoke")}`초.");
        Add(values, "baseline_line", $"- baseline `{Display(B)}.0`: `{Source("baseline_package_root").Replace("artifacts/", string.Empty, StringComparison.Ordinal)}/payload`");
        Add(values, "target_line", $"- target `{Train}.0`: `{Source("package_root").Replace("artifacts/", string.Empty, StringComparison.Ordinal)}/payload`");
        Add(values, "summary_note", "summary는 상태 필드만 담는다. MSIX 바이트 SHA는 이번 payload로 새로 계산한 값이다.");
        Add(values, "version", V);
        return values;
    }

    private Dictionary<string, string> PairDescriptor()
    {
        var root = Campaign("manual-admin-campaign-descriptor");
        var summary = Load(root + "/summary.json");
        var readiness = Load(Campaign("manual-admin-rebaseline-readiness") + "/summary.json");
        Require(summary.Bool("ok") && summary.Str("overall_status") == "pass" && summary.Long("runner_count") == 6 &&
            summary.Long("missing_count") == 0 && summary.Long("not_pass_count") == 0, "pair-descriptor", "result");
        Require(summary.Bool("plan_only") && !summary.Bool("host_mutation_performed"), "pair-descriptor", "plan-only");
        Require(summary.Str("release_candidate", "next_candidate_version") == V, "pair-descriptor", "next-candidate");
        Require(readiness.Str("package_pair_input_status") == "ready-current-baseline-target-package-pair", "pair-descriptor", "readiness");

        var descriptor = root + "/manual-admin-campaign.descriptor.json";
        var values = Common("pair-descriptor");
        Add(values, "baseline_version", B);
        Add(values, "descriptor_batch_id", summary.Str("descriptor_batch_id"));
        Add(values, "artifact_root", root);
        Add(values, "descriptor_path", descriptor);
        Add(values, "descriptor_sha256", TrainFactsFiles.Sha256(repositoryRoot, descriptor));
        Add(values, "summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, summary.RelativePath));
        Add(values, "readiness_root", Campaign("manual-admin-rebaseline-readiness"));
        Add(values, "readiness_summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, readiness.RelativePath));
        Add(values, "context", $"release train `{Train}` {Orchestrator}가 여섯 bucket을 돈 뒤 닫은 descriptor다.");
        Add(values, "update_rollback_evidence", EvidenceId("update-rollback"));
        Add(values, "clean_host_evidence", EvidenceId("clean-host"));
        Add(values, "burn_evidence", EvidenceId("burn"));
        Add(values, "msix_evidence", EvidenceId("msix"));
        Add(values, "ops_summary_evidence", EvidenceId("ops-summary"));
        return values;
    }

    private Dictionary<string, string> Fullgate()
    {
        var batchId = Source("fullgate_batch_id");
        const string prefix = "full-admin-host-mutation-gate-";
        Require(batchId.StartsWith(prefix, StringComparison.Ordinal), "fullgate", "batch-id");
        var tag = batchId[prefix.Length..];
        var batchRoot = "artifacts/batch-runs/" + batchId;
        var routeRoot = "artifacts/routeparity-service-msi-hyperv-batch-profile-" + tag;
        var osRoot = "artifacts/os-mutation-gates-batch-profile-" + tag;
        var result = Load("artifacts/batch-manifests/" + batchId + ".result.json");
        Require(result.Bool("ok") && result.Str("status") == "completed" && result.Long("executed_steps") == 2 && result.Get("failed_step_id") is null,
            "fullgate", "batch-result");
        var lifecycle = Load(routeRoot + "/msi-lifecycle-smoke.json");
        string[] phases = ["install", "repair", "uninstall-preserve", "install-remove-data", "uninstall-remove-data", "final-restore-install"];
        Require(lifecycle.Bool("ok") && lifecycle.Bool("boot_time_unchanged") &&
            lifecycle.Array("steps").OfType<JsonObject>().Select(step => (TrainFactsFiles.Scalar(step["name"]), TrainFactsFiles.Scalar(step["exit_code"])))
                .SequenceEqual(phases.Select(name => ((string?)name, (string?)"0"))), "fullgate", "msi-lifecycle");
        var preflight = Load(routeRoot + "/same-version-preflight.json");
        var residual = preflight.Array("residual_product_codes").Select(TrainFactsFiles.Scalar).OfType<string>().ToList();
        Require(preflight.Bool("same_version_upgrade_expected") && residual.Count == 1, "fullgate", "preflight");
        var installLog = MsiLog(routeRoot + "/msi-logs/install.log");
        Require(UpgradeDetected.Match(installLog).Groups[1].Value == residual[0], "fullgate", "upgrade-detected");
        foreach (var log in new[] { "install.log", "uninstall-preserve.log", "uninstall-remove-data.log", "final-restore-install.log" })
        {
            var text = MsiLog(routeRoot + "/msi-logs/" + log);
            Require(!text.Contains("another client exists", StringComparison.Ordinal) && !text.Contains("Won't Overwrite", StringComparison.Ordinal),
                "fullgate", "msi-log-" + log);
        }

        var provenance = Load(SingleFile(routeRoot, "*.provenance.json"));
        var commit = provenance.Str("git_commit");
        Require(lifecycle.Bool("installed_build", "ok") && lifecycle.Str("installed_build", "installed_commit") == commit &&
            lifecycle.Str("installed_build", "gate_commit") == commit, "fullgate", "installed-build");
        var arpCodes = lifecycle.Array("same_version_arp", "product_codes").Select(TrainFactsFiles.Scalar).OfType<string>().ToList();
        Require(lifecycle.Bool("same_version_arp", "ok") && arpCodes.Count == 1, "fullgate", "same-version-arp");
        var card = Load(Source("current_card_root") + "/summary.json");
        Require(card.Str("installed_host_sha256") == provenance.Str("service_host", "sha256") &&
            card.Str("installed_cli_sha256") == provenance.Str("cli", "sha256"), "fullgate", "installed-hashes");
        Require(card.Str("installed_product_version") == V + "+" + commit, "fullgate", "installed-product-version");
        Require(Source("fullgate_final_firewall_rule_count") == "0", "fullgate", "final-firewall");
        var cleanup = FindStorageCleanup(Load(routeRoot + "/hyperv-api-route-smoke.json").Get());
        var configurationRoot = TrainFactsFiles.Scalar(cleanup?["configuration_root"]) ?? throw TrainFactsInput.Invalid("artifact-field-missing", "storage_cleanup");
        var marker = configurationRoot.IndexOf("\\pcv-hyperv-api-smoke\\", StringComparison.OrdinalIgnoreCase);
        var vmName = Path.GetFileName(configurationRoot);
        Require(marker >= 0 &&
            Leaves(cleanup!["removed_files"]).SequenceEqual(["disk0.vhdx"]) &&
            Leaves(cleanup["removed_directories"]).SequenceEqual(["Snapshots", "Virtual Machines", vmName]) &&
            Leaves(cleanup["retained"]).Count == 0, "fullgate", "storage-cleanup");

        var steps = Directory.GetFiles(TrainFactsFiles.Full(repositoryRoot, batchRoot + "/step-results"), "0*-*.json")
            .Select(Path.GetFileName).Where(name => !name!.Contains("attempt", StringComparison.Ordinal)).Order(StringComparer.Ordinal).ToList();
        Require(steps.Count == 2, "fullgate", "step-results");
        var values = Common("fullgate");
        Add(values, "release_train", V);
        Add(values, "batch_id", batchId);
        Add(values, "batch_evidence_root", batchRoot);
        Add(values, "batch_manifest", "artifacts/batch-manifests/" + batchId + ".json");
        Add(values, "routeparity_artifact_root", routeRoot);
        Add(values, "os_mutation_artifact_root", osRoot);
        Add(values, "batch_summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, batchRoot + "/summary.json"));
        Add(values, "routeparity_summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, routeRoot + "/summary.json"));
        Add(values, "os_summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, osRoot + "/summary.json"));
        Add(values, "operational_fullgate_msi_sha256", provenance.Str("msi", "sha256"));
        Add(values, "operational_fullgate_payload_aggregate_sha256", provenance.Str("payload", "aggregate_sha256"));
        Add(values, "service_host_sha256", provenance.Str("service_host", "sha256"));
        Add(values, "cli_sha256", provenance.Str("cli", "sha256"));
        Add(values, "product_wrapper_sha256", provenance.Str("payload", "product_wrapper_sha256"));
        Add(values, "installed_product_version", V + "+" + commit);
        Add(values, "arp_entry_count_before", residual.Count.ToString(CultureInfo.InvariantCulture));
        Add(values, "arp_entry_count_after", arpCodes.Count.ToString(CultureInfo.InvariantCulture));
        Add(values, "provenance_commit", commit);
        Add(values, "iso_path", Source("iso_path"));
        if (input.Sources.ContainsKey("campaign_root"))
        {
            var last = Observation("after:installed-runtime-ops-summary");
            Add(values, "prestate", $"pair가 남긴 clean package `{Train}`(`{residual[0]}`, ARP {residual.Count}개)이 설치된 채로 시작했다. 같은 version 재설치(A) 덕분에 손으로 비우지 않았다. VM {VmSummary(last)}, `PureCVisor` firewall rule `{Str(last, "firewall_rule_count")}`.");
        }

        Add(values, "service_step_row", StepRow(batchRoot + "/step-results/" + steps[0], "service-msi-hyperv-admin-smoke"));
        Add(values, "os_step_row", StepRow(batchRoot + "/step-results/" + steps[1], "os-mutation-gate"));
        Add(values, "preflight_row", $"| `same-version-preflight` | 잔여 `{ShortCode(residual[0])}` 기록, `same_version_upgrade_expected=true` |");
        Add(values, "install_log_row", $"| install log | `WIX_UPGRADE_DETECTED`=`{ShortCode(residual[0])}` |");
        Add(values, "build_check_row", $"| 사후 build commit 검사 | `ok=true`, `{Short(commit)}` == gate build |");
        Add(values, "arp_check_row", $"| 사후 같은 version ARP 검사 | `ok=true`, `{arpCodes[0]}` 1개 |");
        Add(values, "route_vm_context", $"route smoke는 설치본 `{Train}`로 VM `{vmName}`을 만들고, checkpoint create/restore/delete 뒤 `vm delete`를 실행한다. 그 job 결과 `storage_cleanup`은 다음과 같다.");
        Add(values, "configuration_root_row", $"| `configuration_root` | `<user-temp>{configurationRoot[marker..]}` |");
        Add(values, "final_arp_row", $"| ARP `PureCVisor Desktop Node` | `{arpCodes[0]}` `{Train}` (1개) |");
        Add(values, "final_hash_row", $"| 설치본 Host / CLI SHA-256 | gate build와 같음 (`{provenance.Str("service_host", "sha256")[..8]}…` / `{provenance.Str("cli", "sha256")[..8]}…`) |");
        Add(values, "final_product_version_row", $"| 설치본 Host ProductVersion | `{V}+{Short(commit)}…` |");
        return values;
    }

    private Dictionary<string, string> CurrentCard()
    {
        var root = Source("current_card_root");
        var card = Load(root + "/summary.json");
        Require(card.Str("status") == "pass" && card.Str("version") == V && !card.Bool("secret_observed") && !card.Bool("tui_present"), "current-card", "result");
        Require(card.Long("cli_exit_zero_count") == 3 && card.Long("cli_json_ok_count") == 3 && card.Long("web_http_200_count") == 2, "current-card", "surfaces");
        Require(card.Bool("operational_payload_host_match") && card.Bool("operational_payload_cli_match") && !card.Bool("clean_payload_host_match"),
            "current-card", "payload-match");
        Require(card.Str("service_start_name") == "LocalSystem" && card.Bool("service_uses_credential_manager") &&
            !card.Bool("service_has_raw_or_protected_token_flag"), "current-card", "service");
        Require(card.Str("promotion_ledger_status") == "not-promoted" && card.Str("canonical_current_evidence") == input.CanonicalCurrent,
            "current-card", "promotion");

        var values = Common("current-card");
        Add(values, "display_version", Train);
        foreach (var key in new[]
                 {
                     "installed_manifest_version", "installed_product_version", "fullgate_batch", "clean_package_msi_sha256",
                     "operational_fullgate_msi_sha256", "clean_package_payload_aggregate_sha256", "operational_fullgate_payload_aggregate_sha256",
                     "provenance_commit", "cli_exit_zero_count", "web_http_200_count", "service_state", "arp_entry_count", "remaining_test_vm_count",
                     "promotion_ledger_status"
                 })
        {
            Add(values, key, card.Str(key));
        }

        Add(values, "artifact_root", root);
        Add(values, "artifact_summary", root + "/summary.json");
        Add(values, "summary_sha256", TrainFactsFiles.Sha256(repositoryRoot, card.RelativePath));
        Add(values, "capture_script", root + "/capture-current-card.ps1");
        Add(values, "capture_script_sha256", TrainFactsFiles.Sha256(repositoryRoot, root + "/capture-current-card.ps1"));
        Add(values, "arp_display_version", card.Str("display_version"));
        Add(values, "latest_manual_admin_package_pair", $"{B} -> {V}");
        Add(values, "latest_manual_admin_descriptor", Load(Campaign("manual-admin-campaign-descriptor") + "/summary.json").Str("descriptor_batch_id"));
        Add(values, "installed_status", "installed_non_promoted_candidate");
        Add(values, "canonical_current_changed", "false");
        Add(values, "capture_note", $"캡처 시각은 `{card.Str("launched_at")}`다. 설치본은 fullgate `{card.Str("fullgate_batch")}`가 마지막에 다시 설치한 operational MSI다. 설치본 Host/CLI SHA-256(`{card.Str("installed_host_sha256")[..8]}…`/`{card.Str("installed_cli_sha256")[..8]}…`)은 fullgate operational payload와 같고 clean package payload와는 다르다. 남은 테스트 VM은 `{card.Str("remaining_test_vm_count")}`개다. secret 형태 문자열은 관측되지 않았다.");
        Add(values, "promotion_note", $"결과는 `installed_non_promoted_candidate`다. Lane 3 승격은 하지 않았고 operational current는 `{input.CanonicalCurrent}`다.");
        return values;
    }

    private Dictionary<string, string> Common(string template)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["evidence_id"] = EvidenceId(template),
            ["canonical_current_evidence"] = input.CanonicalCurrent
        };
        if (template is not ("update-rollback" or "pair-descriptor" or "current-card"))
        {
            values["date"] = input.Date;
        }

        if (template is not ("ops-summary" or "msix"))
        {
            values["version"] = V;
        }

        if (template == "current-card")
        {
            values["date"] = input.Date;
        }

        return values;
    }

    private string EvidenceId(string template) =>
        input.Documents.TryGetValue(template, out var path)
            ? Path.GetFileNameWithoutExtension(path)
            : throw TrainFactsInput.Invalid("document-missing", template);

    private string Source(string key) =>
        input.Sources.TryGetValue(key, out var value) ? value : throw TrainFactsInput.Invalid("source-missing", key);

    private string Campaign(string child) => Source("campaign_root") + "/" + child;

    private TrainFactsJson Load(string relativePath) => TrainFactsJson.Load(repositoryRoot, relativePath);

    private TrainFactsJson PackageFacts(string packageRoot)
    {
        var facts = Load(packageRoot + "/package-facts.json");
        Require(facts.Str("contract") == "pcv-admin-smoke-package-facts-v1", "package", "facts-contract");
        return facts;
    }

    private string SingleFile(string root, string pattern)
    {
        var matches = Directory.GetFiles(TrainFactsFiles.Full(repositoryRoot, root), pattern);
        Require(matches.Length == 1, "fullgate", "single-" + pattern);
        return root + "/" + Path.GetFileName(matches[0]);
    }

    private string MsiLog(string relativePath)
    {
        var bytes = File.ReadAllBytes(TrainFactsFiles.Full(repositoryRoot, relativePath));
        return bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE ? Encoding.Unicode.GetString(bytes) : Encoding.UTF8.GetString(bytes);
    }

    private string StepRow(string relativePath, string expectedId)
    {
        var step = Load(relativePath);
        Require(step.Long("exit_code") == 0 && Path.GetFileName(relativePath).Contains(expectedId, StringComparison.Ordinal), "fullgate", "step-" + expectedId);
        var seconds = (step.Long("duration_ms") / 1000m).ToString("0.000", CultureInfo.InvariantCulture);
        return $"| `{expectedId}` | `PASS` | `0` | `{step.Str("attempt_count")}` | `{seconds}s` |";
    }

    private static JsonObject? FindStorageCleanup(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject item:
                if (item["storage_cleanup"] is JsonObject found)
                {
                    return found;
                }

                foreach (var (_, child) in item)
                {
                    if (FindStorageCleanup(child) is { } nested)
                    {
                        return nested;
                    }
                }

                return null;
            case JsonArray items:
                return items.Select(FindStorageCleanup).FirstOrDefault(result => result is not null);
            default:
                return null;
        }
    }

    private static List<string> Leaves(JsonNode? node) =>
        (node as JsonArray ?? []).Select(TrainFactsFiles.Scalar).OfType<string>().Select(path => Path.GetFileName(path.TrimEnd('\\', '/'))).ToList();

    private TrainFactsObservation Observation(string point)
    {
        observations ??= Load(Campaign("observations.json")).Array("entries").OfType<JsonObject>().ToList();
        var entry = observations.LastOrDefault(item => TrainFactsFiles.Scalar(item["point"]) == point)
            ?? throw TrainFactsInput.Invalid("observation-missing", point);
        Require(entry["observation_error"] is null, "observations", point);
        return new TrainFactsObservation(entry);
    }

    private int Seconds(string bucket)
    {
        var before = DateTimeOffset.Parse(Str(Observation("before:" + bucket), "at"), CultureInfo.InvariantCulture);
        var after = DateTimeOffset.Parse(Str(Observation("after:" + bucket), "at"), CultureInfo.InvariantCulture);
        return (int)Math.Round((after - before).TotalSeconds);
    }

    private void RequireRunning(TrainFactsObservation entry, string document) =>
        Require(Str(entry, "service_state") == "Running" && Str(entry, "service_start_mode") == "Auto" && Str(entry, "web_status") == "200",
            document, "service-or-web");

    private static string Str(TrainFactsObservation entry, string key) =>
        Opt(entry, key) ?? throw TrainFactsInput.Invalid("observation-field-missing", key);

    private static string? Opt(TrainFactsObservation entry, string key) => TrainFactsFiles.Scalar(entry.Entry[key]);

    private static string Cell(TrainFactsObservation entry, string key) => Opt(entry, key) is { } value ? $"`{value}`" : "없음";

    private static string StartMode(TrainFactsObservation entry) => Str(entry, "service_start_mode") == "Auto" ? "Automatic" : Str(entry, "service_start_mode");

    private static string VmSummary(TrainFactsObservation entry)
    {
        var vms = (entry.Entry["vms"] as JsonArray ?? []).OfType<JsonObject>()
            .Select(vm => $"`{TrainFactsFiles.Scalar(vm["name"])}` {TrainFactsFiles.Scalar(vm["state"])}").ToList();
        return vms.Count == 0 ? "없음" : string.Join(", ", vms);
    }

    private static string HostCommit(TrainFactsObservation entry)
    {
        var version = Str(entry, "host_product_version");
        var plus = version.IndexOf('+');
        return plus < 0 ? throw TrainFactsInput.Invalid("observation-field-invalid", "host_product_version") : version[(plus + 1)..];
    }

    private static string Display(string version) => version.Replace("-admin-smoke", string.Empty, StringComparison.Ordinal);

    private static string Short(string commit) => commit.Length >= 7 ? commit[..7] : commit;

    private static string ShortCode(string productCode) => productCode.Length > 9 ? productCode[..9] + "-…}" : productCode;

    private static void Add(Dictionary<string, string> values, string key, string value)
    {
        if (string.IsNullOrEmpty(value) || value.Contains('\n') || value.Contains('\r') || !values.TryAdd(key, value))
        {
            throw TrainFactsInput.Invalid("generated-value-invalid", key);
        }
    }

    private static void Require(bool condition, string document, string what)
    {
        if (!condition)
        {
            throw TrainFactsInput.Invalid("fact-mismatch", document + ":" + what);
        }
    }
}

internal sealed record TrainFactsArp(string ProductCode, string DisplayVersion);

internal sealed record TrainFactsObservation(JsonObject Entry)
{
    internal bool ContainsVm(string name) =>
        (Entry["vms"] as JsonArray ?? []).OfType<JsonObject>().Any(vm => TrainFactsFiles.Scalar(vm["name"]) == name);

    internal IReadOnlyList<TrainFactsArp> Arp() =>
        (Entry["arp"] as JsonArray ?? []).OfType<JsonObject>()
            .Select(item => new TrainFactsArp(TrainFactsFiles.Scalar(item["product_code"]) ?? string.Empty, TrainFactsFiles.Scalar(item["display_version"]) ?? string.Empty))
            .ToList();
}
