[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$InstallerUrl,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$UpdatePackageUrl,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$PublicCatalogUri,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Fa-f0-9]{64}$')]
    [string]$MsiSha256,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Fa-f0-9]{64}$')]
    [string]$UpdatePackageSha256,

    [ValidateNotNullOrEmpty()]
    [string]$Channel = 'stable',

    [ValidateNotNullOrEmpty()]
    [string]$ServiceName = 'PureCVisorDesktopNode',

    [ValidateNotNullOrEmpty()]
    [string]$ProtectedTokenPath = '%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json',

    [ValidateNotNullOrEmpty()]
    [string]$DiagnosticsRoot = '%ProgramData%\PureCVisor\desktop-node\diagnostics',

    [string[]]$PreserveBranch = @(),

    [switch]$AllowLocalDescriptorWrite
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $AllowLocalDescriptorWrite.IsPresent) {
    throw 'PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_LOCAL_DESCRIPTOR_WRITE_REQUIRED: pass -AllowLocalDescriptorWrite to write the non-mutating execution bundle.'
}

function Assert-PcvHttpsUri {
    param(
        [Parameter(Mandatory = $true)][string]$Value,
        [Parameter(Mandatory = $true)][string]$Code
    )

    $uri = $null
    if (-not [System.Uri]::TryCreate($Value, [System.UriKind]::Absolute, [ref]$uri)) {
        throw $Code
    }

    if ($uri.Scheme -ne 'https') {
        throw $Code
    }
}

function Get-PcvOptionalProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        $Default = $null
    )

    if ($null -eq $Object) {
        return $Default
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $Default
    }

    $property.Value
}

function Invoke-PcvBundleComponent {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$ScriptName,
        [Parameter(Mandatory = $true)][hashtable]$Arguments,
        [Parameter(Mandatory = $true)][string]$ToolsRoot,
        [Parameter(Mandatory = $true)][string]$ComponentsRoot
    )

    $scriptPath = Join-Path $ToolsRoot $ScriptName
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
        throw "PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_COMPONENT_NOT_FOUND: $scriptPath"
    }

    $componentRoot = Join-Path $ComponentsRoot $Name
    New-Item -ItemType Directory -Force -Path $componentRoot | Out-Null
    $componentArguments = @{} + $Arguments
    $componentArguments['ArtifactRoot'] = $componentRoot

    & $scriptPath @componentArguments | Out-Null

    $summaryPath = Join-Path $componentRoot 'summary.json'
    if (-not (Test-Path -LiteralPath $summaryPath -PathType Leaf)) {
        throw "PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_SUMMARY_NOT_FOUND: $summaryPath"
    }

    $summary = Get-Content -Raw -LiteralPath $summaryPath | ConvertFrom-Json
    [ordered]@{
        name = $Name
        scope = [string](Get-PcvOptionalProperty -Object $summary -Name 'scope' -Default $Name)
        ok = [bool](Get-PcvOptionalProperty -Object $summary -Name 'ok' -Default $false)
        actual_execution = [string](Get-PcvOptionalProperty -Object $summary -Name 'actual_execution' -Default 'not-run')
        host_mutation_performed = [bool](Get-PcvOptionalProperty -Object $summary -Name 'host_mutation_performed' -Default $false)
        public_trusted_signing = [string](Get-PcvOptionalProperty -Object $summary -Name 'public_trusted_signing' -Default 'not-claimed')
        external_stable_publication = [string](Get-PcvOptionalProperty -Object $summary -Name 'external_stable_publication' -Default 'not-claimed')
        summary_path = $summaryPath
    }
}

Assert-PcvHttpsUri -Value $InstallerUrl -Code 'PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_INSTALLER_URI_UNTRUSTED'
Assert-PcvHttpsUri -Value $UpdatePackageUrl -Code 'PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_UPDATE_URI_UNTRUSTED'
Assert-PcvHttpsUri -Value $PublicCatalogUri -Code 'PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_CATALOG_URI_UNTRUSTED'

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$toolsRoot = Join-Path $repoRoot 'packaging/windows-desktop-node/tools'
$inputsRoot = Join-Path $artifactRootFull 'inputs'
$componentsRoot = Join-Path $artifactRootFull 'components'
New-Item -ItemType Directory -Force -Path $inputsRoot, $componentsRoot | Out-Null
$preservedBranches = @(
    foreach ($branchValue in @($PreserveBranch)) {
        foreach ($branch in ([string]$branchValue -split '[,;]')) {
            $trimmed = $branch.Trim()
            if (-not [string]::IsNullOrWhiteSpace($trimmed)) {
                $trimmed
            }
        }
    }
)

$publicationDescriptorPath = Join-Path $inputsRoot "PureCVisorDesktopNode-$Version-windows-x64.publication.json"
$catalogPath = Join-Path $inputsRoot 'purecvisor-desktop-node-catalog.input.json'

$publicationDescriptor = [ordered]@{
    schema_version = '1'
    product = [ordered]@{
        name = 'PureCVisor Desktop Node'
        version = $Version
        release_channel = $Channel
    }
    artifact = [ordered]@{
        base_name = "PureCVisorDesktopNode-$Version-windows-x64"
        msi_path = "PureCVisorDesktopNode-$Version-windows-x64.msi"
        msi_sha256 = $MsiSha256
        signing_mode = 'RequireSigned'
        signing_trust_model = 'PublicTrustedCandidate'
    }
    publication = [ordered]@{
        mode = 'internal-artifact-descriptor-only'
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        burn_bootstrapper = 'not-built'
        msix = 'not-built'
        winget_manifest = 'not-generated'
        catalog_publication = 'not-published'
    }
}
$publicationDescriptor | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $publicationDescriptorPath -Encoding utf8

$catalog = [ordered]@{
    schema_version = 1
    product = [ordered]@{
        id = 'PureCVisor.DesktopNode'
        name = 'PureCVisor Desktop Node'
    }
    publication = [ordered]@{
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        catalog_publication = 'not-published'
    }
    channels = @(
        [ordered]@{
            name = $Channel
            version = $Version
            package_uri = $UpdatePackageUrl
            sha256 = $UpdatePackageSha256
            release_channel = $Channel
            signing_mode = 'RequireSigned'
            rollback_compatible_from = '0.38.8'
        }
    )
}
$catalog | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $catalogPath -Encoding utf8

$componentSteps = @()
$componentSteps += Invoke-PcvBundleComponent `
    -Name 'public-distribution-descriptor' `
    -ScriptName 'New-PcvPublicDistributionDescriptor.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        Version = $Version
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'public-distribution-readiness' `
    -ScriptName 'New-PcvPublicDistributionReadiness.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        PublicationDescriptorPath = $publicationDescriptorPath
        InstallerUrl = $InstallerUrl
        InstallerSha256 = $MsiSha256
        SigningProvider = 'AzureArtifactSigning'
        ReleaseApproval = 'approved-for-local-nonmutating-bundle-only'
        PlanOnly = $true
    }

$readinessSummaryPath = Join-Path $componentsRoot 'public-distribution-readiness\summary.json'
$readinessSummary = Get-Content -Raw -LiteralPath $readinessSummaryPath | ConvertFrom-Json
$wingetManifestPath = [string](Get-PcvOptionalProperty -Object $readinessSummary -Name 'winget_manifest_preview_path')
if ([string]::IsNullOrWhiteSpace($wingetManifestPath) -or -not (Test-Path -LiteralPath $wingetManifestPath -PathType Leaf)) {
    throw 'PCV_PUBLIC_DISTRIBUTION_OPS_BUNDLE_WINGET_MANIFEST_NOT_WRITTEN'
}

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'burn-bootstrapper-preflight' `
    -ScriptName 'New-PcvBurnBootstrapperPreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        PublicationDescriptorPath = $publicationDescriptorPath
        MsiUrl = $InstallerUrl
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'msix-packaging-feasibility-preflight' `
    -ScriptName 'New-PcvMsixPackagingFeasibilityPreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        PublicationDescriptorPath = $publicationDescriptorPath
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'winget-manifest-compliance-preflight' `
    -ScriptName 'New-PcvWingetManifestCompliancePreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        ManifestPath = $wingetManifestPath
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'updater-catalog-publication-preflight' `
    -ScriptName 'New-PcvUpdaterCatalogPublicationPreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        CatalogPath = $catalogPath
        Channel = $Channel
        PublicCatalogUri = $PublicCatalogUri
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'public-signed-update-rollback-smoke-preflight' `
    -ScriptName 'New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        CatalogPath = $catalogPath
        Channel = $Channel
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'windows-credential-manager-transition-preflight' `
    -ScriptName 'New-PcvWindowsCredentialManagerTransitionPreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        ServiceName = $ServiceName
        CredentialTarget = "PureCVisor/$ServiceName/api-token"
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'windows-event-log-provider-transition-preflight' `
    -ScriptName 'New-PcvWindowsEventLogProviderTransitionPreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        ServiceName = $ServiceName
        ProviderName = 'PureCVisor-DesktopNode'
        LogName = 'Application'
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'builtin-tls-certificate-lifecycle-preflight' `
    -ScriptName 'New-PcvBuiltinTlsCertificateLifecyclePreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        ServiceName = $ServiceName
        CertificateSubject = 'CN=PureCVisor Desktop Node Local API'
        HttpsBindPrefix = 'https://127.0.0.1:7778/'
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'service-token-rotation-revoke-preflight' `
    -ScriptName 'New-PcvServiceTokenRotationRevokePreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        ServiceName = $ServiceName
        ProtectedTokenPath = $ProtectedTokenPath
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'timeout-rate-limit-hardening-preflight' `
    -ScriptName 'New-PcvTimeoutRateLimitHardeningPreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        ServiceName = $ServiceName
        PlanOnly = $true
    }

$componentSteps += Invoke-PcvBundleComponent `
    -Name 'diagnostic-bundle-server-preflight' `
    -ScriptName 'New-PcvDiagnosticBundleServerPreflight.ps1' `
    -ToolsRoot $toolsRoot `
    -ComponentsRoot $componentsRoot `
    -Arguments @{
        ServiceName = $ServiceName
        DiagnosticsRoot = $DiagnosticsRoot
        PlanOnly = $true
    }

$executionBundlePath = Join-Path $artifactRootFull 'execution-bundle.json'
$followUpWorkItemsPath = Join-Path $artifactRootFull 'follow-up-work-items.json'

$followUpWorkItems = @(
    [ordered]@{ id = 'burn-bootstrapper'; area = 'distribution'; state = 'preflight-executed-no-build'; next_evidence = 'build/install/repair/remove bootstrapper lifecycle' },
    [ordered]@{ id = 'msix'; area = 'distribution'; state = 'feasibility-preflight-executed-internal-smoke-recorded'; next_evidence = 'public package publication decision' },
    [ordered]@{ id = 'winget-manifest'; area = 'distribution'; state = 'offline-compliance-executed-no-submit'; next_evidence = 'winget validate and repository submission approval' },
    [ordered]@{ id = 'updater-catalog-publication'; area = 'distribution'; state = 'publication-preview-executed-no-upload'; next_evidence = 'externally reachable catalog publication' },
    [ordered]@{ id = 'public-signed-update-rollback-smoke'; area = 'distribution'; state = 'blocked-by-public-signing-and-publication'; next_evidence = 'clean-host public signed update and rollback smoke' },
    [ordered]@{ id = 'windows-credential-manager-transition'; area = 'operations'; state = 'transition-plan-executed-no-credential-mutation'; next_evidence = 'credential write/reload/rollback diagnostics' },
    [ordered]@{ id = 'windows-event-log-provider-transition'; area = 'operations'; state = 'provider-plan-executed-no-provider-mutation'; next_evidence = 'provider registration and default writer transition' },
    [ordered]@{ id = 'builtin-tls-certificate-lifecycle'; area = 'operations'; state = 'lifecycle-plan-executed-no-key-or-binding-mutation'; next_evidence = 'certificate generation, binding, trust, rotation, removal' },
    [ordered]@{ id = 'service-token-rotation-revoke'; area = 'operations'; state = 'rotation-plan-executed-no-token-mutation'; next_evidence = 'protected token replacement, reload, old-token rejection' },
    [ordered]@{ id = 'timeout-rate-limit-hardening'; area = 'operations'; state = 'preflight-executed-code-level-load-evidence-already-recorded'; next_evidence = 'installed listener load smoke if promoted' },
    [ordered]@{ id = 'diagnostic-bundle-server'; area = 'operations'; state = 'preflight-executed-installed-listener-evidence-already-recorded'; next_evidence = 'pagination/list route if required' }
)

$executionBundle = [ordered]@{
    schema_version = 1
    scope = 'public-distribution-ops-execution-bundle'
    version = $Version
    artifact_root = $artifactRootFull
    inputs = [ordered]@{
        publication_descriptor_path = $publicationDescriptorPath
        catalog_path = $catalogPath
        winget_manifest_path = $wingetManifestPath
    }
    component_steps = @($componentSteps)
    follow_up_work_items = @($followUpWorkItems)
}
$executionBundle | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $executionBundlePath -Encoding utf8
$followUpWorkItems | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $followUpWorkItemsPath -Encoding utf8

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'public-distribution-ops-execution-bundle'
    version = $Version
    artifact_root = $artifactRootFull
    public_distribution_ops_execution_bundle = 'code-level-nonmutating-bundle-pass'
    public_distribution_ops_execution_bundle_artifact_root = $artifactRootFull
    public_distribution_ops_execution_bundle_host_mutation_performed = $false
    public_distribution_ops_execution_bundle_public_trusted_signing = 'not-claimed'
    public_distribution_ops_execution_bundle_external_stable_publication = 'not-claimed'
    actual_execution = 'local-preflight-bundle-executed'
    mutates_host = $false
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    public_release = 'not-claimed'
    branch_preservation = [ordered]@{
        status = 'preserved'
        branches = @($preservedBranches)
    }
    component_statuses = [ordered]@{
        burn_bootstrapper_bundle_step = 'preflight-executed-no-build'
        msix_bundle_step = 'feasibility-preflight-executed-no-public-publication'
        winget_bundle_step = 'offline-compliance-executed-no-submit'
        winget_submission = 'not-submitted'
        catalog_bundle_step = 'publication-preview-executed-no-upload'
        catalog_publication = 'not-published'
        public_signed_update_rollback_bundle_step = 'smoke-plan-executed-no-clean-host-run'
        public_signed_update_rollback_smoke = 'blocked-by-public-signing-and-publication'
        clean_host_smoke_status = 'not-run'
        credential_manager_bundle_step = 'transition-plan-executed-no-credential-mutation'
        credential_manager_mutation = 'not-run'
        event_log_bundle_step = 'provider-plan-executed-no-provider-mutation'
        event_log_provider_mutation = 'not-run'
        tls_bundle_step = 'lifecycle-plan-executed-no-key-or-binding-mutation'
        tls_certificate_mutation = 'not-run'
        service_token_bundle_step = 'rotation-plan-executed-no-token-mutation'
        service_token_mutation = 'not-run'
        timeout_rate_limit_hardening = 'blocked-by-no-mutation-preflight'
        diagnostic_bundle_bundle_step = 'server-preflight-executed-no-new-host-mutation'
    }
    component_steps = @($componentSteps)
    execution_bundle_path = $executionBundlePath
    follow_up_work_items_path = $followUpWorkItemsPath
    command_plan = [ordered]@{
        safety = [ordered]@{
            allowed_mode = 'local-preflight-descriptor-bundle'
            host_mutation = 'not-run'
            external_publication = 'blocked-until-public-signing-and-publication'
            public_trusted_signing = 'not-claimed'
        }
    }
}

$summaryJson = $summary | ConvertTo-Json -Depth 14
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson
