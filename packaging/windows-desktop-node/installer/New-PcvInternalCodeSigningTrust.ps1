[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('Ensure', 'Inspect')]
    [string]$Action = 'Ensure',

    [ValidateSet('CurrentUser', 'LocalMachine')]
    [string]$SigningStoreScope = 'CurrentUser',

    [ValidateSet('CurrentUser', 'LocalMachine')]
    [string]$TrustStoreScope = 'CurrentUser',

    [string]$RootSubject = 'CN=PureCVisor Internal Code Signing Root CA',
    [string]$LeafSubject = 'CN=PureCVisor Desktop Node Internal Code Signing',

    [ValidateRange(1, 30)]
    [int]$RootYears = 10,

    [ValidateRange(1, 10)]
    [int]$LeafYears = 2,

    [string]$PublicCertificateOutputRoot,

    [switch]$ForceNew,
    [switch]$SkipTrustInstall,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($WhatIfPreference) {
    $DryRun = $true
}

$codeSigningEku = '1.3.6.1.5.5.7.3.3'

function Write-PcvJsonAndExit {
    param(
        [Parameter(Mandatory)]
        [object]$Payload,

        [Parameter(Mandatory)]
        [int]$ExitCode
    )

    $Payload | ConvertTo-Json -Depth 12 -Compress
    exit $ExitCode
}

function New-PcvInternalTrustError {
    param(
        [Parameter(Mandatory)]
        [string]$Code,

        [Parameter(Mandatory)]
        [string]$Message,

        [string]$Detail = ''
    )

    $payload = [ordered]@{
        ok = $false
        error = [ordered]@{
            code = $Code
            message = $Message
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($Detail)) {
        $payload.error.detail = $Detail
    }

    $payload
}

function Get-PcvIsAdministrator {
    $principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
    $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Get-PcvCertStorePath {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('CurrentUser', 'LocalMachine')]
        [string]$Scope,

        [Parameter(Mandatory)]
        [string]$StoreName
    )

    "Cert:\$Scope\$StoreName"
}

function Test-PcvCodeSigningCertificate {
    param(
        [Parameter(Mandatory)]
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate
    )

    foreach ($eku in @($Certificate.EnhancedKeyUsageList)) {
        if ($eku.ObjectId -eq $script:codeSigningEku) {
            return $true
        }
    }

    return $false
}

function Find-PcvCertificateBySubject {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('CurrentUser', 'LocalMachine')]
        [string]$Scope,

        [Parameter(Mandatory)]
        [string]$StoreName,

        [Parameter(Mandatory)]
        [string]$Subject,

        [switch]$RequirePrivateKey,
        [switch]$RequireCodeSigning
    )

    $storePath = Get-PcvCertStorePath -Scope $Scope -StoreName $StoreName
    if (-not (Test-Path -LiteralPath $storePath)) {
        return $null
    }

    $candidates = @(Get-ChildItem -LiteralPath $storePath | Where-Object {
        $_.Subject -eq $Subject -and
        (-not $RequirePrivateKey -or $_.HasPrivateKey) -and
        (-not $RequireCodeSigning -or (Test-PcvCodeSigningCertificate -Certificate $_))
    } | Sort-Object NotAfter -Descending)

    if ($candidates.Count -eq 0) {
        return $null
    }

    $candidates[0]
}

function ConvertTo-PcvCertificateSummary {
    param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate,
        [string]$Store,
        [bool]$Created = $false,
        [string]$PublicCertificatePath = ''
    )

    if ($null -eq $Certificate) {
        return $null
    }

    $summary = [ordered]@{
        subject = $Certificate.Subject
        issuer = $Certificate.Issuer
        thumbprint = $Certificate.Thumbprint
        store = $Store
        not_before = $Certificate.NotBefore.ToString('o')
        not_after = $Certificate.NotAfter.ToString('o')
        has_private_key = [bool]$Certificate.HasPrivateKey
        code_signing = Test-PcvCodeSigningCertificate -Certificate $Certificate
        created = $Created
    }

    if (-not [string]::IsNullOrWhiteSpace($PublicCertificatePath)) {
        $summary.public_certificate_path = $PublicCertificatePath
    }

    $summary
}

function Export-PcvPublicCertificate {
    param(
        [Parameter(Mandatory)]
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate,

        [Parameter(Mandatory)]
        [string]$OutputRoot,

        [Parameter(Mandatory)]
        [string]$FileName
    )

    New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
    $path = Join-Path $OutputRoot $FileName
    Export-Certificate -Cert $Certificate -FilePath $path -Force | Out-Null
    (Resolve-Path -LiteralPath $path).Path
}

function Import-PcvPublicCertificateToStore {
    param(
        [Parameter(Mandatory)]
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$Certificate,

        [Parameter(Mandatory)]
        [ValidateSet('CurrentUser', 'LocalMachine')]
        [string]$Scope,

        [Parameter(Mandatory)]
        [string]$StoreName
    )

    $storePath = Get-PcvCertStorePath -Scope $Scope -StoreName $StoreName
    $tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-internal-trust-$([Guid]::NewGuid().ToString('n'))"
    try {
        New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null
        $certPath = Join-Path $tempRoot 'certificate.cer'
        Export-Certificate -Cert $Certificate -FilePath $certPath -Force | Out-Null
        Import-Certificate -FilePath $certPath -CertStoreLocation $storePath | Out-Null
    }
    finally {
        if (Test-Path -LiteralPath $tempRoot) {
            Remove-Item -LiteralPath $tempRoot -Recurse -Force
        }
    }

    Find-PcvCertificateBySubject -Scope $Scope -StoreName $StoreName -Subject $Certificate.Subject
}

function New-PcvInternalTrustPlan {
    [ordered]@{
        action = $Action
        signing_store_scope = $SigningStoreScope
        signing_store = Get-PcvCertStorePath -Scope $SigningStoreScope -StoreName 'My'
        trust_store_scope = $TrustStoreScope
        root_subject = $RootSubject
        leaf_subject = $LeafSubject
        root_years = $RootYears
        leaf_years = $LeafYears
        force_new = [bool]$ForceNew
        install_trust = -not [bool]$SkipTrustInstall
        public_certificate_output_root = $PublicCertificateOutputRoot
        local_machine_admin_required = ($SigningStoreScope -eq 'LocalMachine' -or ($TrustStoreScope -eq 'LocalMachine' -and -not [bool]$SkipTrustInstall))
        administrator = Get-PcvIsAdministrator
        signing_trust_model = 'InternalEnterprise'
        secrets_recorded = $false
    }
}

if (-not [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::Windows)) {
    Write-PcvJsonAndExit `
        -ExitCode 1 `
        -Payload (New-PcvInternalTrustError `
            -Code 'PCV_INTERNAL_TRUST_WINDOWS_REQUIRED' `
            -Message 'Internal code-signing trust bootstrap requires Windows certificate stores.')
}

$plan = New-PcvInternalTrustPlan

if ($DryRun) {
    Write-PcvJsonAndExit -ExitCode 0 -Payload ([ordered]@{
        ok = $true
        dry_run = $true
        plan = $plan
    })
}

if ($plan.local_machine_admin_required -and -not [bool]$plan.administrator) {
    Write-PcvJsonAndExit `
        -ExitCode 1 `
        -Payload (New-PcvInternalTrustError `
            -Code 'PCV_INTERNAL_TRUST_ADMIN_REQUIRED' `
            -Message 'LocalMachine signing or trust stores require an elevated PowerShell session.')
}

$rootSigningStore = Get-PcvCertStorePath -Scope $SigningStoreScope -StoreName 'My'
$leafSigningStore = $rootSigningStore
$rootTrustStore = Get-PcvCertStorePath -Scope $TrustStoreScope -StoreName 'Root'
$publisherTrustStore = Get-PcvCertStorePath -Scope $TrustStoreScope -StoreName 'TrustedPublisher'

try {
    $createdRoot = $false
    $createdLeaf = $false
    $root = $null
    $leaf = $null

    if ($Action -eq 'Ensure') {
        if (-not $ForceNew) {
            $root = Find-PcvCertificateBySubject -Scope $SigningStoreScope -StoreName 'My' -Subject $RootSubject -RequirePrivateKey
        }

        if ($null -eq $root) {
            $root = New-SelfSignedCertificate `
                -Type Custom `
                -Subject $RootSubject `
                -CertStoreLocation $rootSigningStore `
                -KeyAlgorithm RSA `
                -KeyLength 3072 `
                -HashAlgorithm SHA256 `
                -KeyExportPolicy NonExportable `
                -KeyUsage CertSign, CRLSign, DigitalSignature `
                -KeyUsageProperty Sign `
                -TextExtension @('2.5.29.19={critical}{text}ca=1&pathlength=1') `
                -NotAfter (Get-Date).AddYears($RootYears)
            $createdRoot = $true
        }

        if (-not $ForceNew) {
            $leaf = Find-PcvCertificateBySubject `
                -Scope $SigningStoreScope `
                -StoreName 'My' `
                -Subject $LeafSubject `
                -RequirePrivateKey `
                -RequireCodeSigning
        }

        if ($null -eq $leaf) {
            $leaf = New-SelfSignedCertificate `
                -Type CodeSigningCert `
                -Subject $LeafSubject `
                -Signer $root `
                -CertStoreLocation $leafSigningStore `
                -KeyAlgorithm RSA `
                -KeyLength 3072 `
                -HashAlgorithm SHA256 `
                -KeyExportPolicy NonExportable `
                -KeyUsage DigitalSignature `
                -TextExtension @("2.5.29.37={text}$codeSigningEku") `
                -NotAfter (Get-Date).AddYears($LeafYears)
            $createdLeaf = $true
        }

        $rootPublicPath = ''
        $leafPublicPath = ''
        if (-not [string]::IsNullOrWhiteSpace($PublicCertificateOutputRoot)) {
            $rootPublicPath = Export-PcvPublicCertificate -Certificate $root -OutputRoot $PublicCertificateOutputRoot -FileName 'PureCVisor-Internal-CodeSigning-Root.cer'
            $leafPublicPath = Export-PcvPublicCertificate -Certificate $leaf -OutputRoot $PublicCertificateOutputRoot -FileName 'PureCVisor-DesktopNode-Internal-CodeSigning.cer'
        }

        $trustedRoot = $null
        $trustedPublisher = $null
        if (-not $SkipTrustInstall) {
            $trustedRoot = Import-PcvPublicCertificateToStore -Certificate $root -Scope $TrustStoreScope -StoreName 'Root'
            $trustedPublisher = Import-PcvPublicCertificateToStore -Certificate $leaf -Scope $TrustStoreScope -StoreName 'TrustedPublisher'
        }

        Write-PcvJsonAndExit -ExitCode 0 -Payload ([ordered]@{
            ok = $true
            action = $Action
            signing_trust_model = 'InternalEnterprise'
            signing = [ordered]@{
                root = ConvertTo-PcvCertificateSummary -Certificate $root -Store $rootSigningStore -Created $createdRoot -PublicCertificatePath $rootPublicPath
                leaf = ConvertTo-PcvCertificateSummary -Certificate $leaf -Store $leafSigningStore -Created $createdLeaf -PublicCertificatePath $leafPublicPath
            }
            trust = [ordered]@{
                installed = -not [bool]$SkipTrustInstall
                root_store = $rootTrustStore
                trusted_publisher_store = $publisherTrustStore
                root = ConvertTo-PcvCertificateSummary -Certificate $trustedRoot -Store $rootTrustStore
                publisher = ConvertTo-PcvCertificateSummary -Certificate $trustedPublisher -Store $publisherTrustStore
            }
            build_arguments = [ordered]@{
                SigningMode = 'RequireSigned'
                SigningTrustModel = 'InternalEnterprise'
                CertificateThumbprint = $leaf.Thumbprint
            }
            secrets_recorded = $false
        })
    }

    $existingRoot = Find-PcvCertificateBySubject -Scope $SigningStoreScope -StoreName 'My' -Subject $RootSubject -RequirePrivateKey
    $existingLeaf = Find-PcvCertificateBySubject -Scope $SigningStoreScope -StoreName 'My' -Subject $LeafSubject -RequirePrivateKey -RequireCodeSigning
    $trustedRoot = Find-PcvCertificateBySubject -Scope $TrustStoreScope -StoreName 'Root' -Subject $RootSubject
    $trustedPublisher = Find-PcvCertificateBySubject -Scope $TrustStoreScope -StoreName 'TrustedPublisher' -Subject $LeafSubject

    Write-PcvJsonAndExit -ExitCode 0 -Payload ([ordered]@{
        ok = $true
        action = $Action
        signing_trust_model = 'InternalEnterprise'
        signing = [ordered]@{
            root = ConvertTo-PcvCertificateSummary -Certificate $existingRoot -Store $rootSigningStore
            leaf = ConvertTo-PcvCertificateSummary -Certificate $existingLeaf -Store $leafSigningStore
        }
        trust = [ordered]@{
            installed = ($null -ne $trustedRoot -and $null -ne $trustedPublisher)
            root_store = $rootTrustStore
            trusted_publisher_store = $publisherTrustStore
            root = ConvertTo-PcvCertificateSummary -Certificate $trustedRoot -Store $rootTrustStore
            publisher = ConvertTo-PcvCertificateSummary -Certificate $trustedPublisher -Store $publisherTrustStore
        }
        secrets_recorded = $false
    })
}
catch {
    Write-PcvJsonAndExit `
        -ExitCode 1 `
        -Payload (New-PcvInternalTrustError `
            -Code 'PCV_INTERNAL_TRUST_FAILED' `
            -Message 'Internal code-signing trust bootstrap failed.' `
            -Detail $_.Exception.Message)
}
