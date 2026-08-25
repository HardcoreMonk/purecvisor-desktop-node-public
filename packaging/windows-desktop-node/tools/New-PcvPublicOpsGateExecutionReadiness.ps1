[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [AllowEmptyString()]
    [string]$CatalogPath,

    [AllowEmptyString()]
    [string]$PackagePath,

    [AllowEmptyString()]
    [string]$LocalPublicationRoot,

    [AllowEmptyString()]
    [string]$PublicCatalogUri,

    [AllowEmptyString()]
    [string]$PublicInstallerUrl,

    [ValidateNotNullOrEmpty()]
    [string]$CatalogUploadTokenEnvVar = 'PCV_PUBLIC_CATALOG_UPLOAD_TOKEN',

    [ValidateNotNullOrEmpty()]
    [string]$WingetSubmissionTokenEnvVar = 'PCV_WINGET_SUBMISSION_TOKEN',

    [AllowEmptyString()]
    [string]$CleanHostRunnerPath,

    [AllowEmptyString()]
    [string]$CredentialSystemProofPath,

    [ValidateNotNullOrEmpty()]
    [string]$TlsCertificateSubject = 'CN=PureCVisor Desktop Node Local API',

    [switch]$AllowLocalPublicationStaging,

    [switch]$RunLocalTlsLifecycle,

    [switch]$AllowLocalEvidenceWrite
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $AllowLocalEvidenceWrite.IsPresent) {
    throw 'PCV_PUBLIC_OPS_GATE_EXECUTION_READINESS_LOCAL_EVIDENCE_WRITE_REQUIRED: pass -AllowLocalEvidenceWrite to write the local execution-readiness descriptor.'
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

function Copy-PcvLocalPublicationArtifact {
    param(
        [Parameter(Mandatory = $true)][string]$SourcePath,
        [Parameter(Mandatory = $true)][string]$DestinationPath
    )

    Copy-Item -LiteralPath $SourcePath -Destination $DestinationPath -Force
    $shaPath = "$DestinationPath.sha256"
    $sha256 = (Get-FileHash -LiteralPath $DestinationPath -Algorithm SHA256).Hash.ToLowerInvariant()
    Set-Content -LiteralPath $shaPath -Value $sha256 -Encoding utf8

    [ordered]@{
        path = $DestinationPath
        sha256_path = $shaPath
        sha256 = $sha256
    }
}

function New-PcvEphemeralPublicCertificate {
    param(
        [Parameter(Mandatory = $true)][string]$Subject,
        [Parameter(Mandatory = $true)][int]$ValidDays
    )

    $rsa = [System.Security.Cryptography.RSA]::Create(2048)
    try {
        $request = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
            $Subject,
            $rsa,
            [System.Security.Cryptography.HashAlgorithmName]::SHA256,
            [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)

        $request.CertificateExtensions.Add(
            [System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new($false, $false, 0, $true))
        $request.CertificateExtensions.Add(
            [System.Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
                [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature,
                $true))

        $cert = $request.CreateSelfSigned(
            [System.DateTimeOffset]::UtcNow.AddMinutes(-5),
            [System.DateTimeOffset]::UtcNow.AddDays($ValidDays))

        [ordered]@{
            certificate = $cert
            key = $rsa
        }
    } catch {
        $rsa.Dispose()
        throw
    }
}

function Write-PcvTlsLifecycleEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$ArtifactRootFull,
        [Parameter(Mandatory = $true)][string]$Subject
    )

    $tlsRoot = Join-Path $ArtifactRootFull 'tls'
    New-Item -ItemType Directory -Force -Path $tlsRoot | Out-Null

    $initial = New-PcvEphemeralPublicCertificate -Subject $Subject -ValidDays 30
    $rotated = New-PcvEphemeralPublicCertificate -Subject $Subject -ValidDays 90

    try {
        $initialPath = Join-Path $tlsRoot 'initial-public-certificate.cer'
        $rotatedPath = Join-Path $tlsRoot 'rotated-public-certificate.cer'

        [System.IO.File]::WriteAllBytes(
            $initialPath,
            $initial.certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
        [System.IO.File]::WriteAllBytes(
            $rotatedPath,
            $rotated.certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))

        $initialHash = (Get-FileHash -LiteralPath $initialPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $rotatedHash = (Get-FileHash -LiteralPath $rotatedPath -Algorithm SHA256).Hash.ToLowerInvariant()

        $lifecycle = [ordered]@{
            schema_version = 1
            scope = 'public-ops-gate-tls-certificate-lifecycle-code-level'
            certificate_generation = 'code-level-pass'
            rotation = 'code-level-pass'
            deletion = 'code-level-private-key-disposed'
            binding = 'not-run'
            trust_store_mutation = 'not-run'
            lan_binding_mutation = 'not-run'
            host_mutation_performed = $false
            private_key_material_written = $false
            certificate_subject = $Subject
            initial_thumbprint = $initial.certificate.Thumbprint
            rotated_thumbprint = $rotated.certificate.Thumbprint
            initial_public_certificate_path = $initialPath
            rotated_public_certificate_path = $rotatedPath
            initial_public_certificate_sha256 = $initialHash
            rotated_public_certificate_sha256 = $rotatedHash
        }

        $lifecyclePath = Join-Path $ArtifactRootFull 'tls-certificate-lifecycle.json'
        $lifecycle | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $lifecyclePath -Encoding utf8

        [ordered]@{
            status = 'partial-code-level-cert-generate-rotate-delete-pass'
            path = $lifecyclePath
        }
    } finally {
        if ($null -ne $initial.certificate) { $initial.certificate.Dispose() }
        if ($null -ne $initial.key) { $initial.key.Dispose() }
        if ($null -ne $rotated.certificate) { $rotated.certificate.Dispose() }
        if ($null -ne $rotated.key) { $rotated.key.Dispose() }
    }
}

function Import-PcvCredentialSystemProof {
    param(
        [Parameter(Mandatory = $true)][string]$ProofPath,
        [Parameter(Mandatory = $true)][string]$ArtifactRootFull
    )

    $proofFull = [System.IO.Path]::GetFullPath($ProofPath)
    if (-not (Test-Path -LiteralPath $proofFull -PathType Leaf)) {
        throw "PCV_PUBLIC_OPS_GATE_CREDENTIAL_SYSTEM_PROOF_NOT_FOUND: $proofFull"
    }

    $proof = Get-Content -Raw -LiteralPath $proofFull | ConvertFrom-Json
    if ([string]$proof.identity -ne 'NT AUTHORITY\SYSTEM') {
        throw 'PCV_PUBLIC_OPS_GATE_CREDENTIAL_SYSTEM_PROOF_IDENTITY_REQUIRED'
    }

    foreach ($propertyName in @('credential_write_status', 'credential_read_status', 'credential_delete_status')) {
        if ([string]$proof.$propertyName -ne 'pass') {
            throw "PCV_PUBLIC_OPS_GATE_CREDENTIAL_SYSTEM_PROOF_$($propertyName.ToUpperInvariant())_REQUIRED"
        }
    }

    if ([bool]$proof.token_value_observed) {
        throw 'PCV_PUBLIC_OPS_GATE_CREDENTIAL_SYSTEM_PROOF_TOKEN_VALUE_OBSERVED'
    }

    $copyPath = Join-Path $ArtifactRootFull 'credential-manager-system-proof.imported.json'
    Copy-Item -LiteralPath $proofFull -Destination $copyPath -Force

    [ordered]@{
        status = 'system-context-proof-import-pass'
        path = $copyPath
        proof = $proof
    }
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$publicCatalogUriPresent = Test-PcvHttpsUri -Value $PublicCatalogUri
$publicInstallerUrlPresent = Test-PcvHttpsUri -Value $PublicInstallerUrl
$catalogUploadCredentialPresent = Test-PcvEnvValue -Name $CatalogUploadTokenEnvVar
$wingetSubmissionCredentialPresent = Test-PcvEnvValue -Name $WingetSubmissionTokenEnvVar
$cleanHostRunnerPresent = Test-PcvExistingFile -Path $CleanHostRunnerPath

$catalogPublicationStatus = 'not-uploaded'
$externalPublicationStatus = if ($publicCatalogUriPresent -and $catalogUploadCredentialPresent) {
    'ready-for-upload-not-run'
} else {
    'blocked-by-missing-upload-endpoint-and-credential'
}
$localPublication = $null

if ($AllowLocalPublicationStaging.IsPresent) {
    if (-not (Test-PcvExistingFile -Path $CatalogPath)) {
        throw 'PCV_PUBLIC_OPS_GATE_LOCAL_PUBLICATION_CATALOG_REQUIRED'
    }
    if (-not (Test-PcvExistingFile -Path $PackagePath)) {
        throw 'PCV_PUBLIC_OPS_GATE_LOCAL_PUBLICATION_PACKAGE_REQUIRED'
    }
    if ([string]::IsNullOrWhiteSpace($LocalPublicationRoot)) {
        throw 'PCV_PUBLIC_OPS_GATE_LOCAL_PUBLICATION_ROOT_REQUIRED'
    }

    $localPublicationRootFull = [System.IO.Path]::GetFullPath($LocalPublicationRoot)
    New-Item -ItemType Directory -Force -Path $localPublicationRootFull | Out-Null

    $catalogCopy = Copy-PcvLocalPublicationArtifact `
        -SourcePath ([System.IO.Path]::GetFullPath($CatalogPath)) `
        -DestinationPath (Join-Path $localPublicationRootFull 'purecvisor-desktop-node-catalog.json')
    $packageCopy = Copy-PcvLocalPublicationArtifact `
        -SourcePath ([System.IO.Path]::GetFullPath($PackagePath)) `
        -DestinationPath (Join-Path $localPublicationRootFull ([System.IO.Path]::GetFileName($PackagePath)))

    $catalogPublicationStatus = 'local-staging-pass-external-not-claimed'
    $externalPublicationStatus = 'local-staging-pass-external-not-claimed'
    $localPublication = [ordered]@{
        root = $localPublicationRootFull
        catalog_staged_path = $catalogCopy.path
        catalog_sha256_path = $catalogCopy.sha256_path
        catalog_sha256 = $catalogCopy.sha256
        package_staged_path = $packageCopy.path
        package_sha256_path = $packageCopy.sha256_path
        package_sha256 = $packageCopy.sha256
        external_stable_publication = 'not-claimed'
    }
}

$wingetStatus = if ($publicInstallerUrlPresent -and $wingetSubmissionCredentialPresent) {
    'submission-inputs-present-not-submitted'
} else {
    'blocked-by-missing-public-installer-url-or-submission-token'
}

$cleanHostStatus = if ($cleanHostRunnerPresent -and $publicInstallerUrlPresent) {
    'clean-host-runner-handoff-ready-not-run'
} else {
    'blocked-by-missing-clean-host-runner-or-public-publication'
}

$credentialProofStatus = 'blocked-by-missing-system-context-proof'
$credentialTransitionStatus = 'blocked-by-missing-system-context-proof'
$credentialProof = $null
if (-not [string]::IsNullOrWhiteSpace($CredentialSystemProofPath)) {
    $credentialImport = Import-PcvCredentialSystemProof -ProofPath $CredentialSystemProofPath -ArtifactRootFull $artifactRootFull
    $credentialProofStatus = $credentialImport.status
    $credentialTransitionStatus = $credentialImport.status
    $credentialProof = $credentialImport.proof
}

$tlsStatus = 'blocked-by-missing-local-tls-lifecycle-opt-in'
$tlsLifecyclePath = $null
if ($RunLocalTlsLifecycle.IsPresent) {
    $tlsResult = Write-PcvTlsLifecycleEvidence -ArtifactRootFull $artifactRootFull -Subject $TlsCertificateSubject
    $tlsStatus = $tlsResult.status
    $tlsLifecyclePath = $tlsResult.path
}

$eventLogHardeningStatus = 'provider-pass-default-writer-repair-remove-volume-guard-pending'
$eventLogPlanPath = Join-Path $artifactRootFull 'event-log-provider-hardening.plan.json'
[ordered]@{
    schema_version = 1
    scope = 'windows-event-log-provider-hardening-readiness'
    provider_registration = 'previous-installed-provider-register-write-pass'
    default_writer = 'pending'
    repair = 'pending'
    remove = 'pending'
    volume_guard = 'pending'
    host_mutation_performed = $false
    public_release = 'not-claimed'
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $eventLogPlanPath -Encoding utf8

$gates = @(
    [ordered]@{
        id = 'external-stable-publication-catalog-upload'
        state = $externalPublicationStatus
        next_required_evidence = 'upload endpoint, credential, immutable catalog/package URL, SHA-256 binding, and upload audit'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = 'winget-submission'
        state = $wingetStatus
        next_required_evidence = 'public signed stable installer URL, manifest validation, and submission reference'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = 'clean-host-public-signed-install-update-rollback'
        state = $cleanHostStatus
        next_required_evidence = 'clean host runner, public signed installer, public catalog, update, rollback, final service health'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = 'windows-credential-manager-service-default-transition'
        state = $credentialTransitionStatus
        next_required_evidence = 'SYSTEM-context service credential write/read/delete proof, service reload, old source rejection, rollback diagnostics'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = 'built-in-tls-certificate-lifecycle'
        state = $tlsStatus
        next_required_evidence = 'certificate generation, binding, rotation, removal, private key policy, and trust boundary evidence'
        public_claim = 'not-claimed'
    },
    [ordered]@{
        id = 'windows-event-log-provider-hardening'
        state = $eventLogHardeningStatus
        next_required_evidence = 'default writer, repair/remove smoke, schema/versioning, and event volume guard'
        public_claim = 'not-claimed'
    }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    evidence_id = 'public-ops-gate-execution-readiness-2026-05-09-0392'
    scope = 'public-ops-gate-execution-readiness'
    version = $Version
    actual_execution = 'local-execution-readiness-descriptor-written'
    host_mutation_performed = $false
    mutates_host = $false
    public_release = 'not-claimed'
    public_trusted_signing = 'not-claimed'
    external_stable_publication = $externalPublicationStatus
    catalog_publication = $catalogPublicationStatus
    winget_submission = $wingetStatus
    clean_host_public_signed_install_update_rollback_smoke = $cleanHostStatus
    credential_manager_system_context_proof = $credentialProofStatus
    service_credential_manager_default_transition = $credentialTransitionStatus
    tls_certificate_lifecycle = $tlsStatus
    tls_lifecycle_path = $tlsLifecyclePath
    event_log_hardening = $eventLogHardeningStatus
    event_log_plan_path = $eventLogPlanPath
    local_publication = $localPublication
    credential_manager_proof = $credentialProof
    prerequisite_snapshot = [ordered]@{
        public_catalog_uri_present = $publicCatalogUriPresent
        catalog_upload_credential_present = $catalogUploadCredentialPresent
        public_installer_url_present = $publicInstallerUrlPresent
        winget_submission_credential_present = $wingetSubmissionCredentialPresent
        clean_host_runner_present = $cleanHostRunnerPresent
        credential_system_proof_present = $null -ne $credentialProof
        local_publication_staging = $AllowLocalPublicationStaging.IsPresent
        local_tls_lifecycle = $RunLocalTlsLifecycle.IsPresent
    }
    gates = @($gates)
}

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$gatesPath = Join-Path $artifactRootFull 'gates.json'

$summary | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $summaryPath -Encoding utf8
@($gates) | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $gatesPath -Encoding utf8

$summary
