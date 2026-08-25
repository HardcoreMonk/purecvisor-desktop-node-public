using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Installed;

internal static class InstalledContractVerifier
{
    internal const string SpecPath = "config/pcv-installed-smoke-contract-spec-v1.json";

    private const string ErrorCode = "PCV_DELIVERY_INSTALLED_INVALID";

    private static readonly LegacyBatchContractVerifier Core =
        new(
            SpecPath,
            "6854268b054d6fec6ebd908e3603aa905c678090781a780cf5b1242ce601e81d",
            "pcv-installed-smoke-contract-spec-v1",
            ErrorCode,
            [
                "api-host-job-hardening-installed-smoke",
                "installed-account-login-smoke",
                "installed-loopback-bootstrap-smoke",
                "installed-no-vnc-smoke",
                "internal-https-tls-lifecycle-smoke",
                "os-mutation-gate-smoke",
                "service-plan-p0-checkpoint-restore-reconciliation",
                "service-token-rotation-revoke-preflight",
            ],
            [10, 1, 1, 1, 1, 6, 5, 6],
            expectedContractCount: 31,
            expectedShouldSiteCount: 243,
            expectedRequiredLiteralCount: 512,
            expectedSourceCount: 10);

    private static readonly Lazy<bool> Semantics =
        new(ValidateSources, LazyThreadSafetyMode.ExecutionAndPublication);

    internal static void Verify(string key, int ordinal)
    {
        Core.Verify(key, ordinal);
        _ = Semantics.Value;
    }

    internal static void ValidateEvidence(InstalledSmokeEvidenceContract evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (evidence.HostMutationPerformed)
        {
            throw Invalid("host-mutation");
        }

        if (evidence.TokenValueObserved)
        {
            throw Invalid("token-observed");
        }

        if (!evidence.CleanupSteps.Contains("restore-original-state", StringComparer.Ordinal))
        {
            throw Invalid("cleanup");
        }

        if (evidence.RouteResult != "expected-boundary")
        {
            throw Invalid("route-result");
        }

        if (evidence.OverallStatus == "pass" && !evidence.InstalledExecutionObserved)
        {
            throw Invalid("fabricated-pass");
        }

        if (evidence.PublicTrustedSigning != "not-claimed" ||
            evidence.ExternalStablePublication != "not-claimed")
        {
            throw Invalid("claim-boundary");
        }
    }

    private static bool ValidateSources()
    {
        var api = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1");
        RequireTokens(
            api,
            "api-hardening-contract",
            "[switch]$DryRun",
            "PCV_REQUEST_BODY_TOO_LARGE",
            "PCV_RATE_LIMIT_EXCEEDED",
            "PCV_ROUTE_TIMEOUT",
            "application/problem+json",
            "Retry-After",
            "diagnostics_readability",
            "console_capabilities",
            "token_value_observed = $false",
            "password_value_observed = $false",
            "host_mutation_performed = $false",
            "actual_execution = 'dry-run-no-http'");

        var account = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1");
        RequireTokens(
            account,
            "account-contract",
            "[switch]$DryRun",
            "/api/v1/auth/login",
            "/api/v1/auth/session",
            "/api/v1/auth/rbac",
            "/api/v1/console/capabilities",
            "account_file_backup",
            "jwt_signing_key_backup",
            "acl_restore_status",
            "token_value_observed = $false",
            "password_value_observed = $false",
            "public_trusted_signing = 'not-claimed'");

        var loopback = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvInstalledLoopbackBootstrapSmoke.ps1");
        RequireTokens(
            loopback,
            "loopback-contract",
            "/api/v1/auth/loopback-session",
            "/api/v1/auth/session",
            "/api/v1/runtime/policy",
            "/pcv-config.js",
            "pcvDesktopAccountSession.v1",
            "token_value_observed = $false",
            "host_mutation_performed = $false");

        var noVnc = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1");
        RequireTokens(
            noVnc,
            "novnc-contract",
            "[switch]$DryRun",
            "/api/v1/console/novnc/{vm_id}",
            "target_backed_novnc_installed_streaming_smoke",
            "path_name_restored",
            "restore_path_result",
            "token_value_observed = $false",
            "public_trusted_signing = 'not-claimed'");

        var tls = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1");
        RequireOrdered(
            tls,
            "tls-token-source-guard",
            "PCV_TLS_SMOKE_TOKEN_SOURCE_MISMATCH",
            "PCV_TLS_SMOKE_TOKEN_SOURCE_MISMATCH",
            "PCV_TLS_SMOKE_TOKEN_SOURCE_PATH_MISMATCH",
            "PCV_TLS_SMOKE_ADMIN_REQUIRED");

        var osGate = Core.Source(
            "packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1");
        RequireOrdered(
            osGate,
            "os-gate-step-order",
            "'preflight'",
            "'config-migration-apply-service-running'",
            "'eventlog-register'",
            "'eventlog-remove'",
            "'firewall-enable'",
            "'lan-listener-ip-smoke'",
            "'firewall-remove'",
            "'export-existing-internal-trust-certs'",
            "'trust-store-install-existing'",
            "'trust-store-remove-existing'",
            "'trust-store-restore-existing'");
        RequireTokens(
            osGate,
            "os-gate-plan-boundary",
            "[switch]$PlanOnly",
            "actual_execution = 'not-run'",
            "host_mutation_performed = $false",
            "public_trusted_signing = 'excluded'",
            "external_stable_publication = 'not-claimed'",
            "'bearer-required'");

        var reconciliation = Core.Source(
            "packaging/windows-desktop-node/tests/fixtures/service-plan-p0-checkpoint-restore-reconciliation.json");
        RequireTokens(
            reconciliation,
            "restore-reconciliation",
            @"""operation"": ""checkpoint.restore""",
            @"""status"": ""code_complete""",
            @"""host_mutation_performed"": false",
            @"""actual_vm_validation_performed"": false",
            @"""is_current"": true",
            @"""presence_only"": false",
            @"""provider_mutation_called"": false",
            @"""promotion_not_triggered"": true");
        RequireTokens(
            Core.Source(
                "packaging/windows-desktop-node/tests/fixtures/csharp-architecture-wave2c-checkpoint-create-reconciliation.json"),
            "create-exclusion",
            @"""operation"": ""checkpoint.create""",
            @"""excluded_operations""",
            @"""checkpoint.restore""");

        var token = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvServiceTokenRotationRevokePreflight.ps1");
        RequireOrdered(
            token,
            "token-check-order",
            "'service-name-present'",
            "'current-token-storage-recorded'",
            "'protected-token-path-recorded'",
            "'rotation-mode-recorded'",
            "'token-value-not-read'",
            "'new-token-not-generated'",
            "'protected-token-write-not-executed'",
            "'service-reload-not-executed'",
            "'old-token-rejection-not-executed'",
            "'audit-record-not-written'",
            "'host-mutation-not-executed'");
        RequireTokens(
            token,
            "token-plan-boundary",
            "[switch]$PlanOnly",
            "service_token_rotation_revoke = 'blocked-by-no-mutation-preflight'",
            "service_token_mutation = 'not-run'",
            "service_token_value_observed = $false",
            "new_token_value_created = $false",
            "host_mutation_performed = $false");

        ValidateEvidence(new InstalledSmokeEvidenceContract(
            HostMutationPerformed: false,
            TokenValueObserved: false,
            CleanupSteps: ["restore-original-state"],
            RouteResult: "expected-boundary",
            OverallStatus: "not-run",
            InstalledExecutionObserved: false,
            PublicTrustedSigning: "not-claimed",
            ExternalStablePublication: "not-claimed"));
        return true;
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

internal sealed record InstalledSmokeEvidenceContract(
    bool HostMutationPerformed,
    bool TokenValueObserved,
    IReadOnlyList<string> CleanupSteps,
    string RouteResult,
    string OverallStatus,
    bool InstalledExecutionObserved,
    string PublicTrustedSigning,
    string ExternalStablePublication);
