using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Contracts;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Preflight;

internal sealed class PreflightContractVerifier
{
    internal const string SpecPath = "config/pcv-preflight-contract-spec-v1.json";

    private const string ExpectedSpecSha256 =
        "2f1423f294eae4f6bc4b360eb9270b563d44646298c955a807ae3c98a9a014ad";

    private static readonly string[] ExpectedKeys =
    [
        "builtin-tls-certificate-lifecycle-preflight",
        "burn-bootstrapper-preflight",
        "diagnostic-bundle-server-preflight",
        "msix-packaging-feasibility-preflight",
        "public-distribution-descriptor",
        "public-distribution-operations-bundle",
        "public-distribution-readiness",
        "updater-catalog-publication-preflight",
    ];

    private static readonly int[] ExpectedCounts = [6, 8, 6, 6, 6, 6, 6, 8];

    private static readonly Lazy<PreflightContractVerifier> Default =
        new(() => new PreflightContractVerifier(),
            LazyThreadSafetyMode.ExecutionAndPublication);

    private readonly RepositoryContractContext repository;
    private readonly PreflightSpec spec;
    private readonly Dictionary<string, string> sources;
    private readonly Dictionary<string, string> sourcePathByKey;
    private readonly Lazy<bool> binding;

    private PreflightContractVerifier()
    {
        repository = RepositoryContractContext.Find();
        spec = LoadSpec();
        ValidateSpec();
        sources = spec.SourceFiles.ToDictionary(
            source => source.Path,
            source => repository.ReadUtf8Text(source.Path),
            StringComparer.Ordinal);
        sourcePathByKey = ExpectedKeys
            .Select((key, index) => (key, spec.SourceFiles[index].Path))
            .ToDictionary(pair => pair.key, pair => pair.Path, StringComparer.Ordinal);
        binding = new Lazy<bool>(ValidateBinding, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal static void Verify(string key, int ordinal) =>
        Default.Value.VerifyContract(key, ordinal);

    internal void VerifyContract(string key, int ordinal)
    {
        _ = binding.Value;
        var contract = spec.Contracts.SingleOrDefault(candidate =>
            candidate.Key == key && candidate.Ordinal == ordinal);
        if (contract is null)
        {
            throw Invalid("ordinal");
        }

        var source = SourceForKey(key);
        foreach (var literal in contract.RequiredLiterals)
        {
            if (!source.Contains(literal, StringComparison.Ordinal))
            {
                throw Invalid($"literal-{key}-{ordinal:D3}");
            }
        }
    }

    internal static void ValidateDescriptor(PreflightDescriptorContract descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!descriptor.PlanOnly ||
            descriptor.HostMutationPerformed ||
            descriptor.PublicTrustedSigning != "not-claimed" ||
            descriptor.ExternalStablePublication != "not-claimed" ||
            descriptor.CatalogPublication != "not-published")
        {
            throw Invalid("publication-boundary");
        }

        if (descriptor.Blockers.Count == 0 ||
            descriptor.Blockers.Any(string.IsNullOrWhiteSpace) ||
            descriptor.Blockers.Distinct(StringComparer.Ordinal).Count() !=
                descriptor.Blockers.Count)
        {
            throw Invalid("blockers");
        }

        if (descriptor.PackageChannel != "stable")
        {
            throw Invalid("package-channel");
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
        const string executableMutation =
            "(?i)Restart-Computer|Stop-Computer|shutdown\\.exe|schtasks\\.exe|" +
            "winget\\s+submit|git\\s+push|gh\\s+pr\\s+create|msiexec|" +
            "Start-Service|Stop-Service|Restart-Service|New-VM|Remove-VM|" +
            "New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|" +
            "trust-store-remove|New-SelfSignedCertificate|Import-Certificate|" +
            "Import-PfxCertificate|Export-PfxCertificate|certutil|" +
            "netsh\\s+http|Add-AppxPackage|Remove-AppxPackage|makeappx|" +
            "signtool|Compress-Archive|Start-Process";
        if (Regex.IsMatch(source, executableMutation))
        {
            throw Invalid("source-mutation");
        }
    }

    private PreflightSpec LoadSpec()
    {
        var text = repository.ReadUtf8Text(SpecPath);
        if (Hash(text) != ExpectedSpecSha256)
        {
            throw Invalid("spec-sha");
        }

        try
        {
            using var json = JsonContract.Parse(SpecPath, text);
            return JsonSerializer.Deserialize<PreflightSpec>(
                    json.Root.GetRawText(),
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        PropertyNameCaseInsensitive = false,
                        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                    })
                ?? throw Invalid("spec-json");
        }
        catch (JsonException error)
        {
            throw Invalid("spec-json", error);
        }
        catch (NotSupportedException error)
        {
            throw Invalid("spec-json", error);
        }
    }

    private void ValidateSpec()
    {
        if (spec.Contract != "pcv-preflight-contract-spec-v1" ||
            spec.LegacyContractCount != 52 ||
            spec.LegacyShouldSiteCount != 256 ||
            spec.RequiredLiteralCount != 722 ||
            spec.SourceFiles.Count != 8 ||
            spec.LegacyFiles.Count != 8 ||
            spec.Contracts.Count != 52 ||
            spec.SourceFiles.Select(source => source.Path)
                .Distinct(StringComparer.Ordinal).Count() != spec.SourceFiles.Count)
        {
            throw Invalid("spec-contract");
        }

        var contractIndex = 0;
        var literalCount = 0;
        for (var fileIndex = 0; fileIndex < spec.LegacyFiles.Count; fileIndex++)
        {
            var file = spec.LegacyFiles[fileIndex];
            if (file.Key != ExpectedKeys[fileIndex] ||
                file.ContractCount != ExpectedCounts[fileIndex] ||
                file.ShouldSiteCount < 1 ||
                file.Sha256.Length != 64 ||
                spec.SourceFiles[fileIndex].Sha256.Length != 64)
            {
                throw Invalid("spec-file-order");
            }

            for (var ordinal = 1; ordinal <= file.ContractCount; ordinal++)
            {
                var contract = spec.Contracts[contractIndex++];
                if (contract.Key != file.Key ||
                    contract.Ordinal != ordinal ||
                    string.IsNullOrWhiteSpace(contract.Name) ||
                    contract.RequiredLiterals.Count == 0 ||
                    contract.RequiredLiterals.Any(string.IsNullOrWhiteSpace) ||
                    contract.RequiredLiterals.Distinct(StringComparer.Ordinal).Count() !=
                        contract.RequiredLiterals.Count)
                {
                    throw Invalid("spec-contract-order");
                }

                literalCount += contract.RequiredLiterals.Count;
            }
        }

        if (contractIndex != spec.Contracts.Count ||
            literalCount != spec.RequiredLiteralCount ||
            spec.LegacyFiles.Sum(file => file.ShouldSiteCount) != spec.LegacyShouldSiteCount)
        {
            throw Invalid("spec-count");
        }
    }

    private bool ValidateBinding()
    {
        foreach (var source in spec.SourceFiles)
        {
            if (!sources.TryGetValue(source.Path, out var text) || Hash(text) != source.Sha256)
            {
                throw Invalid("source-sha");
            }
        }

        foreach (var file in spec.LegacyFiles)
        {
            var legacy = repository.ReadUtf8Text(file.Path);
            if (Hash(legacy) != file.Sha256)
            {
                throw Invalid("legacy-sha");
            }

            var parsed = LegacyPesterContractParser.Parse(file.Path, legacy);
            var expected = spec.Contracts.Where(contract => contract.Key == file.Key).ToArray();
            if (parsed.Count != expected.Length)
            {
                throw Invalid("legacy-count");
            }

            for (var index = 0; index < parsed.Count; index++)
            {
                if (parsed[index].Ordinal != expected[index].Ordinal ||
                    parsed[index].Name != expected[index].Name)
                {
                    throw Invalid("legacy-order");
                }
            }
        }

        ValidatePlanOnlySources();
        ValidateOrderedContracts();
        ValidateClaimBoundary();
        return true;
    }

    private void ValidatePlanOnlySources()
    {
        string[] planOnlyKeys =
        [
            "builtin-tls-certificate-lifecycle-preflight",
            "burn-bootstrapper-preflight",
            "diagnostic-bundle-server-preflight",
            "msix-packaging-feasibility-preflight",
            "public-distribution-descriptor",
            "public-distribution-readiness",
            "updater-catalog-publication-preflight",
        ];
        foreach (var key in planOnlyKeys)
        {
            var source = SourceForKey(key);
            RequireTokens(
                source,
                $"plan-only-{key}",
                "[switch]$PlanOnly",
                "if (-not $PlanOnly.IsPresent)",
                "plan_only = $PlanOnly.IsPresent",
                "actual_execution = 'not-run'",
                "host_mutation_performed = $false",
                "public_trusted_signing = 'not-claimed'",
                "external_stable_publication = 'not-claimed'");
            ValidateSourceSafety(source);
        }

        var bundle = SourceForKey("public-distribution-operations-bundle");
        RequireTokens(
            bundle,
            "operations-bundle-boundary",
            "[switch]$AllowLocalDescriptorWrite",
            "PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_LOCAL_DESCRIPTOR_WRITE_REQUIRED",
            "actual_execution = 'local-preflight-bundle-executed'",
            "host_mutation_performed = $false",
            "public_trusted_signing = 'not-claimed'",
            "external_stable_publication = 'not-claimed'",
            "catalog_publication = 'not-published'",
            "winget_submission = 'not-submitted'",
            "credential_manager_mutation = 'not-run'");
        ValidateSourceSafety(bundle);
    }

    private void ValidateOrderedContracts()
    {
        RequireOrdered(
            SourceForKey("builtin-tls-certificate-lifecycle-preflight"),
            "tls-check-order",
            "'service-name-present'",
            "'certificate-subject-present'",
            "'https-bind-prefix-recorded'",
            "'current-tls-mode-recorded'",
            "'target-tls-mode-recorded'",
            "'private-key-not-created'",
            "'certificate-import-not-executed'",
            "'trust-store-mutation-not-executed'",
            "'lan-binding-not-executed'",
            "'host-mutation-not-executed'");

        var burn = SourceForKey("burn-bootstrapper-preflight");
        RequireOrdered(
            burn,
            "burn-check-order",
            "'publication-descriptor-schema-v1'",
            "'msi-url-https'",
            "'msi-sha256-present'",
            "'bundle-upgrade-code-valid'",
            "'wix-burn-authoring-preview-written'",
            "'public-claim-not-made'",
            "'bundle-build-not-executed'");
        RequireTokens(
            burn,
            "burn-wix5",
            "WixToolset.BootstrapperApplications.wixext",
            "repair_remove_evidence = 'required-before-pass'",
            "payload_hash_binding = 'preview-only'");

        RequireOrdered(
            SourceForKey("diagnostic-bundle-server-preflight"),
            "diagnostic-check-order",
            "'service-name-present'",
            "'diagnostics-root-recorded'",
            "'api-route-recorded'",
            "'download-route-recorded'",
            "'authz-policy-recorded'",
            "'archive-creation-not-executed'",
            "'download-serving-not-executed'",
            "'redaction-not-executed'",
            "'retention-not-executed'",
            "'wrapper-execution-not-delegated'",
            "'host-mutation-not-executed'");

        RequireOrdered(
            SourceForKey("msix-packaging-feasibility-preflight"),
            "msix-check-order",
            "'publication-descriptor-schema-v1'",
            "'package-identity-preview-written'",
            "'service-packaging-design-required'",
            "'install-update-remove-evidence-required'",
            "'capability-boundary-required'",
            "'public-claim-not-made'",
            "'msix-build-not-executed'");

        RequireOrdered(
            SourceForKey("public-distribution-descriptor"),
            "distribution-gate-order",
            "'public-signing-preflight'",
            "'burn-bootstrapper-plan'",
            "'msix-feasibility-plan'",
            "'winget-manifest-plan'",
            "'updater-catalog-publication-plan'",
            "'public-signed-update-rollback-smoke-plan'",
            "'credential-manager-transition-plan'",
            "'eventlog-provider-default-plan'",
            "'tls-certificate-lifecycle-plan'",
            "'token-rotation-mutation-plan'",
            "'diagnostics-server-action-plan'",
            "'timeout-rate-limit-hardening-plan'");

        RequireOrdered(
            SourceForKey("public-distribution-operations-bundle"),
            "operations-component-order",
            "'public-distribution-descriptor'",
            "'public-distribution-readiness'",
            "'burn-bootstrapper-preflight'",
            "'msix-packaging-feasibility-preflight'",
            "'winget-manifest-compliance-preflight'",
            "'updater-catalog-publication-preflight'",
            "'public-signed-update-rollback-smoke-preflight'",
            "'windows-credential-manager-transition-preflight'",
            "'windows-event-log-provider-transition-preflight'",
            "'builtin-tls-certificate-lifecycle-preflight'",
            "'service-token-rotation-revoke-preflight'",
            "'timeout-rate-limit-hardening-preflight'",
            "'diagnostic-bundle-server-preflight'");

        RequireOrdered(
            SourceForKey("public-distribution-readiness"),
            "readiness-gate-order",
            "'public-signing-inputs'",
            "'winget-manifest-preview'",
            "'winget-validation-command'",
            "'winget-submission-plan'",
            "'msix-service-packaging-feasibility'",
            "'public-publication-blocker'");

        RequireOrdered(
            SourceForKey("updater-catalog-publication-preflight"),
            "updater-check-order",
            "'catalog-schema-v1'",
            "'selected-channel-present'",
            "'catalog-uri-https'",
            "'package-uri-https'",
            "'package-sha256-present'",
            "'public-claim-not-made'",
            "'publication-not-executed'");
    }

    private void ValidateClaimBoundary()
    {
        RequireTokens(
            SourceForKey("builtin-tls-certificate-lifecycle-preflight"),
            "tls-blocker",
            "tls_certificate_lifecycle = 'blocked-by-no-mutation-preflight'",
            "tls_certificate_mutation = 'not-run'",
            "blockers = @(");
        RequireTokens(
            SourceForKey("diagnostic-bundle-server-preflight"),
            "diagnostic-blocker",
            "diagnostic_bundle_server_generation = 'blocked-by-no-mutation-preflight'",
            "diagnostic_bundle_redaction_status = 'not-run'",
            "blockers = @(");
        RequireTokens(
            SourceForKey("msix-packaging-feasibility-preflight"),
            "msix-blocker",
            "msix = 'feasibility-blocked-by-service-packaging-design'",
            "status = 'blocked-by-service-packaging-design'",
            "build_status = 'not-run'");
        RequireTokens(
            SourceForKey("public-distribution-readiness"),
            "readiness-blocker",
            "submission = 'not-submitted'",
            "status = 'blocked-by-service-packaging-design'");
        RequireTokens(
            SourceForKey("updater-catalog-publication-preflight"),
            "updater-not-published",
            "catalog_publication = 'not-published'",
            "actual_execution = 'not-run'");

        ValidateDescriptor(new PreflightDescriptorContract(
            PlanOnly: true,
            HostMutationPerformed: false,
            PublicTrustedSigning: "not-claimed",
            ExternalStablePublication: "not-claimed",
            CatalogPublication: "not-published",
            PackageChannel: "stable",
            Blockers: ["public-signing-required"],
            CredentialFields: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["credential_manager_mutation"] = "not-run",
            }));
    }

    private string SourceForKey(string key)
    {
        if (!sourcePathByKey.TryGetValue(key, out var path) ||
            !sources.TryGetValue(path, out var source))
        {
            throw Invalid("source-not-declared");
        }

        return source;
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

    private static string Hash(string text) =>
        Convert.ToHexString(SHA256.HashData(new UTF8Encoding(false).GetBytes(text)))
            .ToLowerInvariant();

    private static InvalidDataException Invalid(string detail, Exception? inner = null) =>
        new($"PCV_DELIVERY_PREFLIGHT_INVALID|{detail}", inner);

    private sealed record PreflightSpec(
        string Contract,
        int LegacyContractCount,
        int LegacyShouldSiteCount,
        int RequiredLiteralCount,
        IReadOnlyList<PreflightSourceFile> SourceFiles,
        IReadOnlyList<PreflightLegacyFile> LegacyFiles,
        IReadOnlyList<PreflightSpecContract> Contracts);

    private sealed record PreflightSourceFile(string Path, string Sha256);

    private sealed record PreflightLegacyFile(
        string Key,
        string Path,
        string Sha256,
        int ContractCount,
        int ShouldSiteCount);

    private sealed record PreflightSpecContract(
        string Key,
        int Ordinal,
        string Name,
        IReadOnlyList<string> RequiredLiterals);
}

internal sealed record PreflightDescriptorContract(
    bool PlanOnly,
    bool HostMutationPerformed,
    string PublicTrustedSigning,
    string ExternalStablePublication,
    string CatalogPublication,
    string PackageChannel,
    IReadOnlyList<string> Blockers,
    IReadOnlyDictionary<string, string> CredentialFields);
