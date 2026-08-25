using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

internal static class ManualAdminContractVerifier
{
    internal const string SpecPath =
        "config/pcv-manual-admin-readiness-contract-spec-v1.json";

    private const string ErrorCode = "PCV_DELIVERY_MANUAL_ADMIN_INVALID";

    private static readonly LegacyBatchContractVerifier Core =
        new(
            SpecPath,
            "2bf42d86a4304e7293a16e29604afcdc581e21e8b9c2801d9d887a6496905847",
            "pcv-manual-admin-readiness-contract-spec-v1",
            ErrorCode,
            [
                "manual-admin-baseline-reservation",
                "manual-admin-campaign-descriptor",
                "manual-admin-descriptor-currency",
                "manual-admin-rebaseline-readiness",
                "public-ops-final-followup-attempt",
                "public-ops-gate-execution-readiness",
                "public-signed-update-rollback-smoke-preflight",
                "windows-credential-manager-transition-preflight",
            ],
            [3, 5, 6, 10, 3, 5, 7, 6],
            expectedContractCount: 45,
            expectedShouldSiteCount: 274,
            expectedRequiredLiteralCount: 989,
            expectedSourceCount: 9);

    private static readonly Lazy<bool> Semantics =
        new(ValidateSources, LazyThreadSafetyMode.ExecutionAndPublication);

    internal static void Verify(string key, int ordinal)
    {
        Core.Verify(key, ordinal);
        _ = Semantics.Value;
    }

    internal static void ValidateReadiness(
        ManualAdminReadinessContract descriptor,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (descriptor.GeneratedAt > now ||
            descriptor.ExpiresAt <= now ||
            descriptor.ExpiresAt <= descriptor.GeneratedAt)
        {
            throw Invalid("stale-descriptor");
        }

        if (string.IsNullOrWhiteSpace(descriptor.BaselineVersion) ||
            string.IsNullOrWhiteSpace(descriptor.TargetVersion) ||
            descriptor.BaselineVersion != descriptor.InstalledVersion ||
            descriptor.BaselineVersion == descriptor.TargetVersion)
        {
            throw Invalid("package-pair");
        }

        if (descriptor.Blockers.Count == 0 ||
            descriptor.Blockers.Any(string.IsNullOrWhiteSpace) ||
            descriptor.Blockers.Distinct(StringComparer.Ordinal).Count() !=
                descriptor.Blockers.Count)
        {
            throw Invalid("blockers");
        }

        if (descriptor.HostMutationPerformed ||
            descriptor.PublicTrustedSigning != "not-claimed" ||
            descriptor.ExternalStablePublication != "not-claimed")
        {
            throw Invalid("claim-boundary");
        }

        foreach (var field in descriptor.CredentialFields)
        {
            if (string.IsNullOrWhiteSpace(field.Key) ||
                string.IsNullOrWhiteSpace(field.Value) ||
                Regex.IsMatch(
                    field.Key,
                    "(?i)(password|secret|access.?token|private.?key|credential.?value)") ||
                Regex.IsMatch(
                    field.Value,
                    @"(?i)(bearer\s+|password\s*=|secret\s*=|token\s*=)"))
            {
                throw Invalid("credential-field");
            }
        }
    }

    internal static void ValidateSourceSafety(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (Regex.IsMatch(
                source,
                "(?i)Restart-Computer|Stop-Computer|shutdown\\.exe|schtasks\\.exe|" +
                "winget\\s+submit|git\\s+push|gh\\s+pr\\s+create|msiexec|" +
                "Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|" +
                "New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|" +
                "trust-store-remove|cmdkey|CredWrite|CredDelete|New-EventLog|" +
                "Register-EventSource"))
        {
            throw Invalid("source-mutation");
        }
    }

    private static bool ValidateSources()
    {
        var reservation = Core.Source(
            "packaging/windows-desktop-node/tools/PcvManualAdminBaselineReservation.psm1");
        RequireTokens(
            reservation,
            "reservation-contract",
            "pcv-manual-admin-baseline-reservation-v1",
            "PCV_MANUAL_ADMIN_BASELINE_VERSION_MISMATCH",
            "PCV_MANUAL_ADMIN_BASELINE_VERSION_ORDER_INVALID",
            "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_EXPIRED",
            "PCV_MANUAL_ADMIN_BASELINE_RESERVATION_CONSUMED",
            "reservation-consumed.json",
            "status = 'reserved'");

        var campaign = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1");
        RequireTokens(
            campaign,
            "campaign-contract",
            "[switch]$PlanOnly",
            "PCV_MANUAL_ADMIN_CAMPAIGN_DESCRIPTOR_PLAN_ONLY_REQUIRED",
            "blocked-by-missing-evidence",
            "baseline_version = $BaselineVersion",
            "target_version = $TargetVersion",
            "actual_execution = 'not-run'",
            "host_mutation_performed = $false",
            "public_trusted_signing = 'not-claimed'",
            "external_stable_publication = 'not-claimed'");

        var currency = Core.CombinedSource("manual-admin-descriptor-currency");
        RequireTokens(
            currency,
            "currency-contract",
            "descriptor_id:",
            "current_manual_admin_package_pair:",
            "current_manual_admin_campaign:",
            "current_full_admin_host_mutation_gate:",
            "current_full_admin_host_mutation_operational_msi_sha256:",
            "current_full_admin_host_mutation_provenance_commit:");

        var rebaseline = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1");
        RequireTokens(
            rebaseline,
            "rebaseline-contract",
            "PCV_MANUAL_ADMIN_REBASELINE_PLAN_ONLY_REQUIRED",
            "blocked-by-installed-baseline-version-mismatch",
            "blocked-by-baseline-target-version-match",
            "reservation-required-before-actual-execution",
            "reserved-and-matched",
            "mixed_version_input_policy = 'reject-baseline-target-match-or-installed-baseline-mismatch'",
            "actual_execution = 'not-run'",
            "host_mutation_performed = $false");

        var finalAttempt = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvPublicOpsFinalFollowupAttempt.ps1");
        RequireTokens(
            finalAttempt,
            "final-followup-contract",
            "[switch]$AllowLocalEvidenceWrite",
            "PCV_PUBLIC_OPS_FINAL_FOLLOWUP_LOCAL_EVIDENCE_WRITE_REQUIRED",
            "scope = 'public-ops-final-followup-attempt'",
            "host_mutation_performed = $false",
            "public_release = 'not-claimed'",
            "remaining_follow_up_items = @($followUps)");

        var gate = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvPublicOpsGateExecutionReadiness.ps1");
        RequireTokens(
            gate,
            "gate-readiness-contract",
            "[switch]$AllowLocalEvidenceWrite",
            "PCV_PUBLIC_OPS_GATE_EXECUTION_READINESS_LOCAL_EVIDENCE_WRITE_REQUIRED",
            "PCV_PUBLIC_OPS_GATE_CREDENTIAL_SYSTEM_PROOF_TOKEN_VALUE_OBSERVED",
            "token_value_observed",
            "host_mutation_performed = $false",
            "public_release = 'not-claimed'",
            "public_trusted_signing = 'not-claimed'");

        var signedSmoke = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1");
        RequireOrdered(
            signedSmoke,
            "signed-smoke-check-order",
            "'catalog-schema-v1'",
            "'selected-channel-present'",
            "'package-uri-https'",
            "'package-sha256-present'",
            "'baseline-version-present'",
            "'clean-host-profile-recorded'",
            "'public-trusted-signing-required'",
            "'external-stable-publication-required'",
            "'signed-update-rollback-smoke-not-executed'",
            "'host-mutation-not-executed'");
        RequireTokens(
            signedSmoke,
            "signed-smoke-boundary",
            "public_signed_update_rollback_smoke = 'blocked-by-public-signing-and-publication'",
            "clean_host_smoke_status = 'not-run'",
            "public_trusted_signing = 'not-claimed'",
            "external_stable_publication = 'not-claimed'");

        var credential = Core.Source(
            "packaging/windows-desktop-node/tools/New-PcvWindowsCredentialManagerTransitionPreflight.ps1");
        RequireOrdered(
            credential,
            "credential-check-order",
            "'service-name-present'",
            "'credential-target-present'",
            "'current-token-storage-recorded'",
            "'target-token-storage-recorded'",
            "'token-value-not-read'",
            "'credential-write-not-executed'",
            "'credential-delete-not-executed'",
            "'rollback-diagnostics-required'",
            "'service-reload-required'",
            "'host-mutation-not-executed'");
        RequireTokens(
            credential,
            "credential-boundary",
            "token_value_observed = $false",
            "credential_manager_transition = 'blocked-by-no-mutation-preflight'",
            "credential_manager_mutation = 'not-run'");

        foreach (var source in new[]
                 {
                     reservation,
                     campaign,
                     rebaseline,
                     finalAttempt,
                     gate,
                     signedSmoke,
                     credential,
                 })
        {
            ValidateSourceSafety(source);
        }

        ValidateReadiness(
            new ManualAdminReadinessContract(
                GeneratedAt: DateTimeOffset.Parse("2026-08-26T00:00:00Z"),
                ExpiresAt: DateTimeOffset.Parse("2026-08-27T00:00:00Z"),
                BaselineVersion: "0.42.73-admin-smoke",
                TargetVersion: "0.42.74-admin-smoke",
                InstalledVersion: "0.42.73-admin-smoke",
                Blockers: ["public-signing-required"],
                HostMutationPerformed: false,
                PublicTrustedSigning: "not-claimed",
                ExternalStablePublication: "not-claimed",
                CredentialFields: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["credential_manager_mutation"] = "not-run",
                }),
            DateTimeOffset.Parse("2026-08-26T12:00:00Z"));
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

internal sealed record ManualAdminReadinessContract(
    DateTimeOffset GeneratedAt,
    DateTimeOffset ExpiresAt,
    string BaselineVersion,
    string TargetVersion,
    string InstalledVersion,
    IReadOnlyList<string> Blockers,
    bool HostMutationPerformed,
    string PublicTrustedSigning,
    string ExternalStablePublication,
    IReadOnlyDictionary<string, string> CredentialFields);
