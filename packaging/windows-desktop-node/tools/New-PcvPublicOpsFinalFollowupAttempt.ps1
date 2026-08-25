[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [ValidateNotNullOrEmpty()]
    [string]$ServiceName = 'PureCVisorDesktopNode',

    [ValidateNotNullOrEmpty()]
    [string]$EventLogProviderName = 'PureCVisor Desktop Node',

    [AllowEmptyString()]
    [string]$PublicSigningCertificatePath,

    [AllowEmptyString()]
    [string]$PublicSigningThumbprint,

    [AllowEmptyString()]
    [string]$TimestampUrl,

    [AllowEmptyString()]
    [string]$CatalogUploadUri,

    [ValidateNotNullOrEmpty()]
    [string]$CatalogUploadTokenEnvVar = 'PCV_PUBLIC_CATALOG_UPLOAD_TOKEN',

    [AllowEmptyString()]
    [string]$PublicInstallerUrl,

    [ValidateNotNullOrEmpty()]
    [string]$WingetSubmissionTokenEnvVar = 'PCV_WINGET_SUBMISSION_TOKEN',

    [AllowEmptyString()]
    [string]$CleanHostRunnerPath,

    [AllowEmptyString()]
    [string]$CredentialTarget,

    [AllowEmptyString()]
    [string]$SystemCredentialRunnerPath,

    [AllowEmptyString()]
    [string]$TlsBindPrefix,

    [switch]$AllowLocalEvidenceWrite
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $AllowLocalEvidenceWrite.IsPresent) {
    throw 'PCV_PUBLIC_OPS_FINAL_FOLLOWUP_LOCAL_EVIDENCE_WRITE_REQUIRED: pass -AllowLocalEvidenceWrite to write the local evidence descriptor.'
}

function Test-PcvHttpsUri {
    param([AllowEmptyString()][string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $false
    }

    $uri = $null
    if (-not [System.Uri]::TryCreate($Value, [System.UriKind]::Absolute, [ref]$uri)) {
        return $false
    }

    $uri.Scheme -eq 'https'
}

function Test-PcvExistingFile {
    param([AllowEmptyString()][string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $false
    }

    Test-Path -LiteralPath ([System.IO.Path]::GetFullPath($Path)) -PathType Leaf
}

function Test-PcvEnvValue {
    param([Parameter(Mandatory = $true)][string]$Name)

    $processValue = [Environment]::GetEnvironmentVariable($Name, 'Process')
    $userValue = [Environment]::GetEnvironmentVariable($Name, 'User')
    $machineValue = [Environment]::GetEnvironmentVariable($Name, 'Machine')

    -not [string]::IsNullOrWhiteSpace($processValue) -or
        -not [string]::IsNullOrWhiteSpace($userValue) -or
        -not [string]::IsNullOrWhiteSpace($machineValue)
}

function Find-PcvCommandPath {
    param([Parameter(Mandatory = $true)][string[]]$Names)

    foreach ($name in $Names) {
        $command = Get-Command -Name $name -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $command) {
            return [string]$command.Source
        }
    }

    if ($Names -contains 'signtool.exe') {
        $programFilesX86 = ${env:ProgramFiles(x86)}
        if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
            $windowsKitsRoot = Join-Path $programFilesX86 'Windows Kits/10/bin'
            if (Test-Path -LiteralPath $windowsKitsRoot -PathType Container) {
            $candidate = Get-ChildItem -LiteralPath $windowsKitsRoot -Directory |
                Sort-Object -Property Name -Descending |
                ForEach-Object { Join-Path $_.FullName 'x64/signtool.exe' } |
                Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
                Select-Object -First 1

                if ($null -ne $candidate) {
                    return [string]$candidate
                }
            }
        }
    }

    $null
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$publicCertificateFilePresent = Test-PcvExistingFile -Path $PublicSigningCertificatePath
$publicThumbprintPresent = -not [string]::IsNullOrWhiteSpace($PublicSigningThumbprint)
$publicSigningMaterialPresent = $publicCertificateFilePresent -or $publicThumbprintPresent
$timestampUrlPresent = Test-PcvHttpsUri -Value $TimestampUrl
$catalogUploadEndpointPresent = Test-PcvHttpsUri -Value $CatalogUploadUri
$catalogUploadCredentialPresent = Test-PcvEnvValue -Name $CatalogUploadTokenEnvVar
$publicInstallerUrlPresent = Test-PcvHttpsUri -Value $PublicInstallerUrl
$wingetSubmissionCredentialPresent = Test-PcvEnvValue -Name $WingetSubmissionTokenEnvVar
$cleanHostRunnerPresent = Test-PcvExistingFile -Path $CleanHostRunnerPath
$credentialTargetPresent = -not [string]::IsNullOrWhiteSpace($CredentialTarget)
$systemCredentialRunnerPresent = Test-PcvExistingFile -Path $SystemCredentialRunnerPath
$tlsBindPrefixPresent = Test-PcvHttpsUri -Value $TlsBindPrefix

$timestampStatus = if ($publicSigningMaterialPresent -and $timestampUrlPresent) {
    'ready-for-execution-not-run'
} else {
    'blocked-by-missing-public-signing-cert-and-timestamp-url'
}

$externalPublicationStatus = if ($catalogUploadEndpointPresent -and $catalogUploadCredentialPresent) {
    'ready-for-publication-not-run'
} else {
    'blocked-by-missing-upload-endpoint-and-credentials'
}

$wingetStatus = if ($publicSigningMaterialPresent -and $publicInstallerUrlPresent -and $wingetSubmissionCredentialPresent) {
    'ready-for-submission-not-submitted'
} else {
    'blocked-by-no-public-signed-stable-installer-and-public-url'
}

$cleanHostStatus = if ($publicSigningMaterialPresent -and $catalogUploadEndpointPresent -and $catalogUploadCredentialPresent -and $publicInstallerUrlPresent -and $cleanHostRunnerPresent) {
    'ready-for-clean-host-smoke-not-run'
} else {
    'blocked-by-public-signing-publication-and-clean-host'
}

$serviceCredentialStatus = if ($credentialTargetPresent -and $systemCredentialRunnerPresent) {
    'ready-for-system-context-smoke-not-run'
} else {
    'blocked-by-service-account-context'
}

$followUps = @(
    [ordered]@{
        id = '1-public-trusted-signing-timestamp'
        title = 'Public trusted signing provider/cert chain/timestamp evidence'
        state = $timestampStatus
        blocker = if ($timestampStatus -eq 'ready-for-execution-not-run') { 'execution intentionally not performed by this descriptor' } else { 'missing public signing material or trusted timestamp URL' }
        next_required_evidence = 'public code signing provider proof, certificate chain, timestamped signature verification, and SignTool verification artifact'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = '2-external-stable-publication-catalog-upload'
        title = 'External stable publication and catalog upload'
        state = $externalPublicationStatus
        blocker = if ($externalPublicationStatus -eq 'ready-for-publication-not-run') { 'publication intentionally not performed by this descriptor' } else { 'missing external upload endpoint or credential' }
        next_required_evidence = 'externally reachable stable catalog/package URL, immutable SHA-256 binding, upload audit, and channel resolver smoke'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = '3-winget-submission'
        title = 'Winget submission'
        state = $wingetStatus
        blocker = if ($wingetStatus -eq 'ready-for-submission-not-submitted') { 'submission intentionally not performed by this descriptor' } else { 'missing public signed stable installer URL or submission credential' }
        next_required_evidence = 'winget repository submission reference, public installer URL, installer SHA-256, and validation result'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = '4-clean-host-public-signed-install-update-rollback'
        title = 'Clean-host public signed install/update/rollback smoke'
        state = $cleanHostStatus
        blocker = if ($cleanHostStatus -eq 'ready-for-clean-host-smoke-not-run') { 'clean-host smoke intentionally not performed by this descriptor' } else { 'missing public signing, external publication, public installer URL, or clean host runner' }
        next_required_evidence = 'fresh clean-host install, public signed update, rollback, final health, and no-reboot/service-state artifact'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = '5-windows-credential-manager-service-default-transition'
        title = 'Windows Credential Manager service default transition'
        state = $serviceCredentialStatus
        blocker = if ($serviceCredentialStatus -eq 'ready-for-system-context-smoke-not-run') { 'SYSTEM-context smoke intentionally not performed by this descriptor' } else { 'installed service runs as LocalSystem; current-user capability smoke does not prove service token resolution' }
        next_required_evidence = 'service credential target option, SYSTEM-context write/read/delete, service reload, old source rejection, rollback diagnostics, and token redaction'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = '6-built-in-tls-certificate-lifecycle'
        title = 'Built-in TLS certificate lifecycle'
        state = 'blocked-by-no-mutation-preflight'
        blocker = if ($tlsBindPrefixPresent) { 'TLS lifecycle mutation implementation and approval still required' } else { 'no HTTPS bind prefix or TLS lifecycle mutation implementation supplied' }
        next_required_evidence = 'certificate generation, private key protection, HTTPS/LAN binding, trust boundary, rotation, removal, and cleanup artifact'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = '7-windows-event-log-provider-hardening'
        title = 'Windows Event Log provider hardening'
        state = 'provider-pass-default-writer-repair-remove-volume-guard-pending'
        blocker = 'provider registration/write PASS exists; default writer policy, repair/remove behavior, and log volume guard remain to harden'
        next_required_evidence = 'default writer enablement, provider repair/remove smoke, event schema/versioning, retention or volume guard, and service diagnostics integration'
        public_claim = 'not-claimed'
    }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    evidence_id = 'public-ops-final-followup-attempt-2026-05-09-0391'
    scope = 'public-ops-final-followup-attempt'
    version = $Version
    service_name = $ServiceName
    actual_execution = 'local-final-followup-prerequisite-scan-executed'
    host_mutation_performed = $false
    mutates_host = $false
    public_release = 'not-claimed'
    public_trusted_signing = if ($publicSigningMaterialPresent) { 'ready-for-execution-not-run' } else { 'blocked-by-missing-public-signing-material' }
    timestamp_evidence = $timestampStatus
    external_stable_publication = $externalPublicationStatus
    catalog_publication = 'not-uploaded'
    winget_submission = $wingetStatus
    clean_host_public_signed_install_update_rollback_smoke = $cleanHostStatus
    credential_manager_transition = 'capability-pass-service-transition-blocked'
    service_credential_manager_default_transition = $serviceCredentialStatus
    tls_certificate_lifecycle = 'blocked-by-no-mutation-preflight'
    event_log_provider_transition = 'installed-provider-register-write-pass'
    event_log_hardening = 'provider-pass-default-writer-repair-remove-volume-guard-pending'
    prerequisite_snapshot = [ordered]@{
        signtool_path = Find-PcvCommandPath -Names @('signtool.exe', 'signtool')
        winget_path = Find-PcvCommandPath -Names @('winget.exe', 'winget')
        github_cli_path = Find-PcvCommandPath -Names @('gh.exe', 'gh')
        public_signing_certificate_file_present = $publicCertificateFilePresent
        public_signing_thumbprint_present = $publicThumbprintPresent
        timestamp_url_present = $timestampUrlPresent
        catalog_upload_endpoint_present = $catalogUploadEndpointPresent
        catalog_upload_credential_present = $catalogUploadCredentialPresent
        public_installer_url_present = $publicInstallerUrlPresent
        winget_submission_credential_present = $wingetSubmissionCredentialPresent
        clean_host_runner_present = $cleanHostRunnerPresent
        credential_target_present = $credentialTargetPresent
        system_credential_runner_present = $systemCredentialRunnerPresent
        tls_bind_prefix_present = $tlsBindPrefixPresent
        event_log_provider_name = $EventLogProviderName
    }
    remaining_follow_up_count = @($followUps).Count
    remaining_follow_up_items = @($followUps)
}

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$itemsPath = Join-Path $artifactRootFull 'remaining-follow-up-items.json'

$summary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $summaryPath -Encoding utf8
@($followUps) | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $itemsPath -Encoding utf8

$summary
