[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ServiceName,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$CertificateSubject,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$HttpsBindPrefix,

    [ValidateNotNullOrEmpty()]
    [string]$CurrentTlsMode = 'external-terminator-or-none',

    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly.IsPresent) {
    throw 'PCV_BUILTIN_TLS_CERTIFICATE_LIFECYCLE_PLAN_ONLY_REQUIRED: pass -PlanOnly to write the lifecycle preflight descriptor.'
}

foreach ($value in @($ServiceName, $CertificateSubject, $HttpsBindPrefix, $CurrentTlsMode)) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw 'PCV_BUILTIN_TLS_CERTIFICATE_LIFECYCLE_FIELD_REQUIRED'
    }

    if ($value -match '[\x00-\x1F]') {
        throw 'PCV_BUILTIN_TLS_CERTIFICATE_LIFECYCLE_FIELD_INVALID'
    }
}

$prefixUri = $null
if (-not [System.Uri]::TryCreate($HttpsBindPrefix, [System.UriKind]::Absolute, [ref]$prefixUri)) {
    throw 'PCV_BUILTIN_TLS_CERTIFICATE_LIFECYCLE_BIND_PREFIX_INVALID'
}

if ($prefixUri.Scheme -ne 'https') {
    throw 'PCV_BUILTIN_TLS_CERTIFICATE_LIFECYCLE_BIND_PREFIX_NOT_HTTPS'
}

$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)
New-Item -ItemType Directory -Force -Path $artifactRootFull | Out-Null

$lifecyclePlanPath = Join-Path $artifactRootFull 'builtin-tls-certificate-lifecycle.plan-preview.json'
$plannedOperations = @(
    'inspect-current-tls-policy',
    'generate-service-certificate',
    'plan-certificate-private-key-storage',
    'bind-https-listener',
    'install-trust-anchor-if-approved',
    'rotate-certificate',
    'remove-certificate-and-binding'
)

$lifecyclePlan = [ordered]@{
    schema_version = 1
    scope = 'builtin-tls-certificate-lifecycle-preflight'
    service_name = $ServiceName
    certificate_subject = $CertificateSubject
    https_bind_prefix = $HttpsBindPrefix
    current_tls_mode = $CurrentTlsMode
    target_tls_mode = 'built-in-service-certificate'
    private_key_material_created = $false
    certificate_import_status = 'not-run'
    trust_store_mutation = 'not-run'
    lan_binding_mutation = 'not-run'
    rotation_status = 'not-run'
    removal_status = 'not-run'
    planned_operations = $plannedOperations
    planned_observations = @(
        'certificate-generation-required',
        'private-key-storage-policy-required',
        'https-binding-required',
        'trust-boundary-approval-required',
        'rotation-and-removal-required'
    )
}

$lifecyclePlan | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $lifecyclePlanPath -Encoding utf8

$lifecycleChecks = @(
    [ordered]@{ name = 'service-name-present'; status = 'pass' },
    [ordered]@{ name = 'certificate-subject-present'; status = 'pass' },
    [ordered]@{ name = 'https-bind-prefix-recorded'; status = 'pass' },
    [ordered]@{ name = 'current-tls-mode-recorded'; status = 'pass' },
    [ordered]@{ name = 'target-tls-mode-recorded'; status = 'pass' },
    [ordered]@{ name = 'private-key-not-created'; status = 'not-run' },
    [ordered]@{ name = 'certificate-import-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'trust-store-mutation-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'lan-binding-not-executed'; status = 'not-run' },
    [ordered]@{ name = 'host-mutation-not-executed'; status = 'not-run' }
)

$summary = [ordered]@{
    schema_version = 1
    ok = $true
    scope = 'builtin-tls-certificate-lifecycle-preflight'
    plan_only = $PlanOnly.IsPresent
    actual_execution = 'not-run'
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    tls_certificate_lifecycle = 'blocked-by-no-mutation-preflight'
    tls_certificate_mutation = 'not-run'
    private_key_material_created = $false
    trust_store_mutation = 'not-run'
    lan_binding_mutation = 'not-run'
    service_name = $ServiceName
    certificate_subject = $CertificateSubject
    https_bind_prefix = $HttpsBindPrefix
    current_tls_mode = $CurrentTlsMode
    target_tls_mode = 'built-in-service-certificate'
    lifecycle_plan_path = $lifecyclePlanPath
    lifecycle_checks = @($lifecycleChecks)
    blockers = @(
        'certificate material is not created in this preflight',
        'private key storage and rotation policy must be implemented before pass',
        'trust boundary and HTTPS binding evidence must be implemented before pass'
    )
}

$summaryJson = $summary | ConvertTo-Json -Depth 10
Set-Content -LiteralPath (Join-Path $artifactRootFull 'summary.json') -Value $summaryJson -Encoding utf8
$summaryJson
