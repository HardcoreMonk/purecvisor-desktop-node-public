using System.Text.Json;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Reconciliation;

internal static class ReconciliationContractVerifier
{
    internal const string SpecPath =
        "config/pcv-reconciliation-lifecycle-contract-spec-v1.json";

    private const string ErrorCode = "PCV_DELIVERY_RECONCILIATION_INVALID";

    private static readonly LegacyBatchContractVerifier Core =
        new(
            SpecPath,
            "2553899a52981600fdbccc30d00291a50c6f34e7c08b8a9bff2b075e90fb48d1",
            "pcv-reconciliation-lifecycle-contract-spec-v1",
            ErrorCode,
            [
                "post-reboot-verification",
                "wave2-breconciliation-decision",
                "wave2-ccheckpoint-create-reconciliation",
                "wave2-cvm-delete-reconciliation",
                "wave2-cvm-rename-reconciliation",
                "windows-event-log-default-transition-smoke",
                "windows-event-log-provider-transition-preflight",
                "winget-manifest-compliance-preflight",
            ],
            [21, 6, 4, 4, 4, 2, 6, 7],
            expectedContractCount: 54,
            expectedShouldSiteCount: 321,
            expectedRequiredLiteralCount: 1084,
            expectedSourceCount: 10);

    private static readonly Lazy<bool> Semantics =
        new(ValidateSources, LazyThreadSafetyMode.ExecutionAndPublication);

    internal static void Verify(string key, int ordinal)
    {
        Core.Verify(key, ordinal);
        _ = Semantics.Value;
    }

    internal static void ValidateTransition(string from, string to)
    {
        var allowed = from switch
        {
            "pending" => to == "running",
            "running" => to is "completed" or "failed",
            _ => false,
        };

        if (!allowed)
        {
            throw Invalid("illegal-transition");
        }
    }

    internal static void ValidateCheckpoint(
        DateTimeOffset capturedAt,
        DateTimeOffset now,
        TimeSpan maximumAge)
    {
        if (maximumAge <= TimeSpan.Zero || capturedAt > now || now - capturedAt > maximumAge)
        {
            throw Invalid("stale-checkpoint");
        }
    }

    internal static void ValidateCleanup(IReadOnlyList<string> cleanupSteps)
    {
        ArgumentNullException.ThrowIfNull(cleanupSteps);
        if (!cleanupSteps.Contains("unregister-task", StringComparer.Ordinal) ||
            cleanupSteps.Any(string.IsNullOrWhiteSpace) ||
            cleanupSteps.Distinct(StringComparer.Ordinal).Count() != cleanupSteps.Count)
        {
            throw Invalid("missing-cleanup");
        }
    }

    internal static void ValidateLifecycle(IReadOnlyList<string> lifecycle)
    {
        ArgumentNullException.ThrowIfNull(lifecycle);
        string[] expected = ["write-evidence", "persist-completion", "unregister-task"];
        if (!lifecycle.SequenceEqual(expected, StringComparer.Ordinal))
        {
            throw Invalid("lifecycle-order");
        }
    }

    internal static void ValidateEventLogPreflight(bool hostMutationPerformed)
    {
        if (hostMutationPerformed)
        {
            throw Invalid("event-log-mutation");
        }
    }

    internal static void ValidateWinget(WingetManifestContract manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        if (!Regex.IsMatch(manifest.PackageIdentifier, "^[A-Za-z0-9][A-Za-z0-9.-]+$") ||
            !Uri.TryCreate(manifest.InstallerUrl, UriKind.Absolute, out var installerUri) ||
            installerUri.Scheme != Uri.UriSchemeHttps ||
            !Regex.IsMatch(manifest.InstallerSha256, "^[A-Fa-f0-9]{64}$") ||
            manifest.InstallerType != "msi" ||
            manifest.ManifestType != "singleton" ||
            manifest.ManifestVersion != "1.12.0")
        {
            throw Invalid("invalid-winget-field");
        }
    }

    private static bool ValidateSources()
    {
        ValidatePostRebootSources();
        ValidateWave2BFixture();
        ValidateWave2CFixture(
            "packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json",
            "pcv-checkpoint-create-reconciliation/v1",
            "checkpoint.create",
            "Reconcile checkpoint");
        ValidateWave2CFixture(
            "packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-vm-delete-reconciliation.json",
            "pcv-vm-delete-reconciliation/v1",
            "vm.delete",
            "Reconcile delete");
        ValidateWave2CFixture(
            "packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-vm-rename-reconciliation.json",
            "pcv-vm-rename-reconciliation/v1",
            "vm.rename",
            "Reconcile rename");
        ValidateEventLogSources();
        ValidateWingetSource();

        ValidateTransition("pending", "running");
        ValidateCheckpoint(
            DateTimeOffset.Parse("2026-08-26T11:55:00Z"),
            DateTimeOffset.Parse("2026-08-26T12:00:00Z"),
            TimeSpan.FromMinutes(10));
        ValidateCleanup(["unregister-task"]);
        ValidateLifecycle(["write-evidence", "persist-completion", "unregister-task"]);
        ValidateEventLogPreflight(hostMutationPerformed: false);
        ValidateWinget(new WingetManifestContract(
            PackageIdentifier: "PureCVisor.DesktopNode",
            InstallerUrl: "https://downloads.example.invalid/PureCVisorDesktopNode.msi",
            InstallerSha256: new string('D', 64),
            InstallerType: "msi",
            ManifestType: "singleton",
            ManifestVersion: "1.12.0"));
        return true;
    }

    private static void ValidatePostRebootSources()
    {
        var module = Core.Source(
            "packaging/windows-desktop-node/tools/PcvPostRebootVerification.psm1");
        var runner = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1");
        var register = Core.Source(
            "packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1");

        RequireTokens(
            module,
            "post-reboot-contract",
            "@('ProductStatus', 'PackagingRegression')",
            "PCV_POST_REBOOT_PROFILE_RETIRED",
            "PCV_POST_REBOOT_PRINCIPAL_NOT_ALLOWED",
            "LocalSystemAtStartup",
            "Bearer)\\s+",
            "[REPO_ROOT]",
            "[EVIDENCE_ROOT]",
            "New-PcvPostRebootScheduledTaskPlan",
            "if (-not $DryRun)",
            "reason = 'verification-failed'",
            "reason = 'already-completed-cleanup-disabled'",
            "action = 'unregister-task'");
        RequireAbsent(module, "post-reboot-product-boundary", "archive/spikes/");
        RequireOrdered(
            module,
            "post-reboot-completion-order",
            "Write-PcvPostRebootJsonFile -Path (Join-Path $evidenceDir 'post-reboot-result.json') -InputObject $result",
            "Write-PcvPostRebootTextFile -Path (Join-Path $evidenceDir 'post-reboot-summary.md')",
            "Write-PcvPostRebootJsonFile -Path $completeMarker",
            "$cleanup = & $UnregisterTask -TaskName");
        RequireTokens(
            runner,
            "post-reboot-runner",
            "PCV_POST_REBOOT_STATE_NOT_FOUND",
            "Invoke-PcvPostRebootVerification -StateFile $StateFile",
            "if (-not $result.ok)");
        RequireTokens(
            register,
            "post-reboot-entrypoint",
            "[switch]$DryRun",
            "[switch]$Reboot",
            "PCV_POST_REBOOT_AUTO_REBOOT_DISABLED",
            "-DryRun:$DryRun");
    }

    private static void ValidateWave2BFixture()
    {
        const string path =
            "packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2b-reconciliation.json";
        using var document = ParseJson(path, Core.Source(path));
        var root = document.RootElement;

        RequireJsonString(root, "schema_version", "pcv-job-reconciliation-decision/v1", "wave2b-schema");
        RequireJsonString(root, "decision_id", "wave2b-operation-reconciliation-v1", "wave2b-id");
        RequireJsonString(root, "status", "code_complete", "wave2b-status");
        RequireJsonFalse(root, "implementation_change", "wave2b-mutation-boundary");
        RequireJsonFalse(root, "product_behavior_changed", "wave2b-mutation-boundary");
        RequireJsonFalse(root, "host_mutation_performed", "wave2b-mutation-boundary");
        RequireJsonFalse(root, "hyperv_mutation_performed", "wave2b-mutation-boundary");
        RequireJsonFalse(root, "actual_vm_validation_performed", "wave2b-mutation-boundary");

        var operations = root.GetProperty("operation_families")
            .EnumerateArray()
            .SelectMany(family => family.GetProperty("operations").EnumerateArray())
            .Select(operation => operation.GetString() ?? string.Empty)
            .ToArray();
        string[] expectedOperations =
        [
            "vm.create", "vm.start", "vm.shutdown", "vm.poweroff", "vm.restart",
            "vm.pause", "vm.resume", "vm.rename", "vm.eject", "vm.limit",
            "vm.set-memory", "vm.set-vcpu", "vm.disk-resize", "vm.qos.storage.set",
            "vm.qos.network.set", "vm.guest.exec", "vm.guest.channel.verify",
            "vm.guest.channel.ensure", "vm.delete", "checkpoint.create",
            "checkpoint.restore", "checkpoint.delete",
        ];
        if (operations.Length != 22 ||
            operations.Distinct(StringComparer.Ordinal).Count() != 22 ||
            !expectedOperations.All(operation => operations.Contains(operation, StringComparer.Ordinal)))
        {
            throw Invalid("wave2b-operations");
        }

        var familyIds = root.GetProperty("operation_families")
            .EnumerateArray()
            .Select(family => family.GetProperty("family_id").GetString() ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
        string[] requiredFamilies = ["vm-create", "vm-delete", "vm-rename", "vm-qos", "checkpoint"];
        if (!requiredFamilies.All(familyIds.Contains))
        {
            throw Invalid("wave2b-families");
        }

        var recovery = root.GetProperty("running_recovery");
        RequireJsonString(recovery, "current_projection_status", "failed", "wave2b-recovery");
        RequireJsonString(recovery, "error_code", "PCV_JOB_INTERRUPTED", "wave2b-recovery");
        RequireJsonFalse(recovery, "retryable", "wave2b-recovery");
        RequireJsonFalse(recovery, "automatic_retry", "wave2b-recovery");

        var timeout = root.GetProperty("timeout_policy");
        if (timeout.GetProperty("route_timeout_default_seconds").GetInt32() != 30 ||
            !timeout.GetProperty("route_timeout_configured_range_seconds")
                .EnumerateArray().Select(value => value.GetInt32()).SequenceEqual([1, 3600]) ||
            timeout.GetProperty("new_reconciliation_timeout_introduced").GetBoolean() ||
            Core.Source(path).Contains("PCV_JOB_RECONCILIATION_REQUIRED", StringComparison.Ordinal))
        {
            throw Invalid("wave2b-timeout-boundary");
        }
    }

    private static void ValidateWave2CFixture(
        string path,
        string schema,
        string operation,
        string webAction)
    {
        using var document = ParseJson(path, Core.Source(path));
        var root = document.RootElement;
        RequireJsonString(root, "schema_version", schema, "wave2c-schema");
        RequireJsonString(root, "operation", operation, "wave2c-operation");
        RequireJsonString(root, "status", "code_complete", "wave2c-status");
        RequireJsonFalse(root, "host_mutation_performed", "wave2c-host-boundary");
        RequireJsonFalse(root, "hyperv_mutation_performed", "wave2c-host-boundary");
        RequireJsonFalse(root, "actual_vm_validation_performed", "wave2c-host-boundary");
        RequireJsonFalse(root, "package_candidate_created", "wave2c-package-boundary");
        if (!root.GetProperty("promotion_not_triggered").GetBoolean())
        {
            throw Invalid("wave2c-package-boundary");
        }

        var route = root.GetProperty("route");
        RequireJsonString(route, "template", "/api/v1/jobs/{jobId}/reconcile", "wave2c-route");
        RequireJsonString(route, "operation_name", "ReconcileJob", "wave2c-route");
        RequireJsonString(route, "required_permission", "operate", "wave2c-route");
        var manual = root.GetProperty("required_manual_mapping");
        if (manual.GetProperty("http_status").GetInt32() != 409 ||
            manual.GetProperty("retryable").GetBoolean())
        {
            throw Invalid("wave2c-fail-closed");
        }
        RequireJsonString(
            manual,
            "error_code",
            "PCV_JOB_RECONCILIATION_REQUIRED",
            "wave2c-fail-closed");
        RequireJsonString(manual, "job_status_unchanged", "failed", "wave2c-fail-closed");
        RequireJsonString(
            root.GetProperty("operator_surface_parity"),
            "web_action",
            webAction,
            "wave2c-operator-parity");
        RequireJsonString(
            root.GetProperty("verification_scope"),
            "actual_vm_smoke",
            "NOT_RUN_BY_DESIGN",
            "wave2c-verification-boundary");
    }

    private static void ValidateEventLogSources()
    {
        var installed = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1");
        RequireTokens(
            installed,
            "event-log-installed-contract",
            "windows-event-log-default-transition-installed",
            "eventlog-default-transition.json",
            "msiexec.exe",
            "installer/build.ps1",
            "Get-WinEvent",
            "--event-log-writer",
            "PCV_EVENTLOG_DEFAULT_SMOKE_SERVICE_PATH_MISMATCH",
            "PCV_EVENTLOG_DEFAULT_SMOKE_WRITER_MISMATCH");
        RequireAbsent(
            installed,
            "event-log-native-action",
            "New-EventLog",
            "Remove-EventLog",
            "Write-EventLog",
            "New-ItemProperty",
            "Set-ItemProperty",
            "Remove-ItemProperty",
            "--api-token ");

        var preflight = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvWindowsEventLogProviderTransitionPreflight.ps1");
        RequireTokens(
            preflight,
            "event-log-preflight-boundary",
            "PCV_WINDOWS_EVENT_LOG_PROVIDER_TRANSITION_PLAN_ONLY_REQUIRED",
            "plan_only = $PlanOnly.IsPresent",
            "actual_execution = 'not-run'",
            "host_mutation_performed = $false",
            "event_log_provider_transition = 'blocked-by-no-mutation-preflight'",
            "event_log_provider_mutation = 'not-run'");
        RequireOrdered(
            preflight,
            "event-log-check-order",
            "'service-name-present'",
            "'provider-name-present'",
            "'log-name-present'",
            "'current-writer-recorded'",
            "'target-writer-recorded'",
            "'provider-registration-not-executed'",
            "'provider-removal-not-executed'",
            "'event-write-not-executed'",
            "'retention-volume-guard-required'",
            "'host-mutation-not-executed'");
        RequireAbsent(
            preflight,
            "event-log-preflight-mutation",
            "Restart-Computer",
            "Start-Service",
            "Stop-Service",
            "New-EventLog",
            "Remove-EventLog",
            "Write-EventLog",
            "wevtutil",
            "eventcreate",
            "New-ItemProperty",
            "Set-ItemProperty",
            "Remove-ItemProperty",
            "HKLM:");
        ValidateEventLogPreflight(hostMutationPerformed: false);
    }

    private static void ValidateWingetSource()
    {
        var source = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1");
        RequireTokens(
            source,
            "winget-boundary",
            "PCV_WINGET_MANIFEST_COMPLIANCE_PLAN_ONLY_REQUIRED",
            "manifest_type = $manifestType",
            "manifest_version = $manifestVersion",
            "installer_type = $installerType",
            "installer_url = $installerUrl",
            "installer_sha256 = $installerSha256",
            "plan_only = $PlanOnly.IsPresent",
            "host_mutation_performed = $false",
            "validation_status = 'not-run'",
            "submission_status = 'not-submitted'");
        RequireOrdered(
            source,
            "winget-check-order",
            "'manifest-file-present'",
            "'singleton-manifest-type'",
            "'manifest-version-supported'",
            "'package-identifier-present'",
            "'package-version-winget-compatible'",
            "'installer-url-https'",
            "'installer-sha256-valid'",
            "'installer-type-msi'",
            "'winget-cli-validation-not-executed'",
            "'winget-submission-not-executed'",
            "'public-claim-not-made'");
        RequireAbsent(
            source,
            "winget-execution-boundary",
            "Restart-Computer",
            "winget validate",
            "winget submit",
            "winget install",
            "winget upgrade",
            "git push",
            "gh pr create",
            "msiexec");
    }

    private static JsonDocument ParseJson(string path, string source)
    {
        try
        {
            return JsonDocument.Parse(source);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"{ErrorCode}|fixture-json-{path}", error);
        }
    }

    private static void RequireJsonString(
        JsonElement element,
        string name,
        string expected,
        string detail)
    {
        if (!element.TryGetProperty(name, out var value) || value.GetString() != expected)
        {
            throw Invalid(detail);
        }
    }

    private static void RequireJsonFalse(JsonElement element, string name, string detail)
    {
        if (!element.TryGetProperty(name, out var value) || value.GetBoolean())
        {
            throw Invalid(detail);
        }
    }

    private static void RequireTokens(string source, string detail, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (!source.Contains(token, StringComparison.Ordinal))
            {
                throw Invalid(detail);
            }
        }
    }

    private static void RequireAbsent(string source, string detail, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (source.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                throw Invalid(detail);
            }
        }
    }

    private static void RequireOrdered(string source, string detail, params string[] tokens)
    {
        var offset = 0;
        foreach (var token in tokens)
        {
            var index = source.IndexOf(token, offset, StringComparison.Ordinal);
            if (index < 0)
            {
                throw Invalid(detail);
            }

            offset = index + token.Length;
        }
    }

    private static InvalidDataException Invalid(string detail) =>
        new($"{ErrorCode}|{detail}");
}

internal sealed record WingetManifestContract(
    string PackageIdentifier,
    string InstallerUrl,
    string InstallerSha256,
    string InstallerType,
    string ManifestType,
    string ManifestVersion);
