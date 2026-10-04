[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CampaignId,
    [Parameter(Mandatory)][string]$BaselineVersion,
    [Parameter(Mandatory)][string]$TargetVersion,
    [Parameter(Mandatory)][string]$BaselinePackageRoot,
    [Parameter(Mandatory)][string]$TargetPackageRoot,
    [Parameter(Mandatory)][string]$BaselineUpdatePackagePath,
    [Parameter(Mandatory)][string]$TargetUpdatePackagePath,
    [Parameter(Mandatory)][string]$BaselineUpdateCatalogPath,
    [Parameter(Mandatory)][string]$TargetUpdateCatalogPath,
    [Parameter(Mandatory)][string]$InstalledManifestPath,
    [Parameter(Mandatory)][string]$ArtifactRoot,
    [Parameter(Mandatory)][string]$BaseVhdPath,
    [Parameter(Mandatory)][string]$VMSwitchName,
    [Parameter(Mandatory)][string]$WixPath,
    [Parameter(Mandatory)][string]$MakeAppxPath,
    [Parameter(Mandatory)][string]$SignToolPath,
    [Parameter(Mandatory)][string]$SigningCertificateThumbprint,
    [Parameter(Mandatory)][string]$MsixTemplateLayoutRoot,
    [string]$InternalCertificatePath = '',
    [string]$BaselineReservationPath = '',
    [string]$HostIdentityOverride = '',
    [string]$VmName = '',
    [string]$InstalledPcvCliPath = 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe',
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [pscredential]$GuestCredential,
    [switch]$PlanOnly,
    [switch]$Execute
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $PlanOnly -and -not $Execute) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_MODE_REQUIRED|Pass exactly one mode.' }
if ($PlanOnly -and $Execute) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_MODE_CONFLICT|Modes are mutually exclusive.' }

function Resolve-PcvPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}
function Assert-PcvFile([string]$Path, [string]$Name) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_INPUT_MISSING|$Name|$Path" }
}
function Assert-PcvDirectory([string]$Path, [string]$Name) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_INPUT_MISSING|$Name|$Path" }
}
function Read-PcvJson([string]$Path) {
    try { Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json }
    catch { throw "PCV_MANUAL_ADMIN_CAMPAIGN_JSON_INVALID|$Path" }
}
function Write-PcvJson([string]$Path, $Value) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 32) + "`n", [Text.UTF8Encoding]::new($false))
}
function Get-PcvSha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Test-PcvChildPath([string]$Path, [string]$Parent) {
    $root = (Resolve-PcvPath $Parent).TrimEnd([IO.Path]::DirectorySeparatorChar)
    (Resolve-PcvPath $Path).StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)
}
function Complete-PcvCampaignClosure {
    param(
        [Parameter(Mandatory)]$Summary,
        [Parameter(Mandatory)][string]$SummaryPath,
        [Parameter(Mandatory)][string]$CampaignRoot,
        [Parameter(Mandatory)][string]$DescriptorRoot,
        [Parameter(Mandatory)][string]$DescriptorPath,
        [Parameter(Mandatory)][string]$ReservationPath,
        [Parameter(Mandatory)][switch]$DescriptorValidated
    )
    if (-not $DescriptorValidated) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_DESCRIPTOR_NOT_VALIDATED' }
    if (-not (Test-PcvChildPath $DescriptorRoot $CampaignRoot) -or -not (Test-PcvChildPath $SummaryPath $CampaignRoot)) {
        throw 'PCV_MANUAL_ADMIN_CAMPAIGN_ARTIFACT_ESCAPE'
    }
    try {
        [void](Set-PcvManualAdminBaselineReservationState -ReservationPath $ReservationPath -State consumed `
            -Now ([datetimeoffset]::UtcNow) -Reason 'manual-admin-package-pair-campaign-pass')
    } catch {
        $consumeError = $_.Exception.Message
        $cleanupError = $null
        try {
            if (Test-Path -LiteralPath $DescriptorRoot) { Remove-Item -LiteralPath $DescriptorRoot -Recurse -Force }
        } catch { $cleanupError = $_.Exception.Message }
        $Summary['ok'] = $false
        $Summary['descriptor_eligible'] = $false
        $Summary['closed_descriptor_emitted'] = $false
        $Summary['descriptor_path'] = $null
        $Summary['closure_error'] = $consumeError
        $Summary['descriptor_cleanup_status'] = if ($cleanupError) { 'FAIL' } else { 'PASS' }
        $Summary['descriptor_cleanup_error'] = $cleanupError
        Write-PcvJson $SummaryPath $Summary
        if ($cleanupError) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_RESERVATION_CONSUME_FAILED|$consumeError|descriptor_cleanup=$cleanupError" }
        throw $consumeError
    }
    $Summary['closed_descriptor_emitted'] = $true
    $Summary['descriptor_path'] = $DescriptorPath
}
function Assert-PcvPackage([string]$Root, [string]$Version) {
    Assert-PcvDirectory $Root "$Version-package-root"
    $payload = Join-Path $Root 'payload'
    $manifest = Join-Path $payload 'product-manifest.json'
    $msi = Join-Path $Root "PureCVisorDesktopNode-$Version-windows-x64.msi"
    $sidecar = "$msi.sha256"
    $publication = Join-Path $Root "PureCVisorDesktopNode-$Version-windows-x64.publication.json"
    Assert-PcvDirectory $payload "$Version-payload"
    Assert-PcvFile $manifest "$Version-payload-manifest"
    Assert-PcvFile $msi "$Version-msi"
    Assert-PcvFile $sidecar "$Version-msi-sha256"
    Assert-PcvFile $publication "$Version-publication"
    if ([string](Read-PcvJson $manifest).version -ne $Version) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_PACKAGE_VERSION_MISMATCH|$Version" }
    $actual = Get-PcvSha256 $msi
    $match = [regex]::Match((Get-Content -Raw $sidecar), '(?i)\b[a-f0-9]{64}\b')
    if (-not $match.Success -or $match.Value.ToLowerInvariant() -ne $actual) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_MSI_HASH_MISMATCH|$Version" }
    $descriptor = Read-PcvJson $publication
    try {
        $publishedVersion = [string]$descriptor.product.version
        $publishedMsiSha = [string]$descriptor.artifact.msi_sha256
        $publishedSigningMode = [string]$descriptor.artifact.signing_mode
        $publishedTrustedSigning = [string]$descriptor.publication.public_trusted_signing
        $publishedExternalPublication = [string]$descriptor.publication.external_stable_publication
    } catch { throw "PCV_MANUAL_ADMIN_CAMPAIGN_PUBLICATION_INVALID|$Version|schema" }
    if ($publishedVersion -ne $Version -or $publishedMsiSha -ne $actual -or
        $publishedSigningMode -ne 'AllowUnsignedDev' -or $publishedTrustedSigning -ne 'not-claimed' -or
        $publishedExternalPublication -ne 'not-claimed') {
        throw "PCV_MANUAL_ADMIN_CAMPAIGN_PUBLICATION_INVALID|$Version"
    }
    [pscustomobject]@{ root = $Root; payload = $payload; manifest = $manifest; msi = $msi; sha256 = $actual }
}
function Assert-PcvCatalog([string]$CatalogPath, [string]$PackagePath, [string]$Version) {
    Assert-PcvFile $CatalogPath "$Version-update-catalog"
    Assert-PcvFile $PackagePath "$Version-update-package"
    $catalog = Read-PcvJson $CatalogPath
    $channel = @($catalog.channels | Where-Object name -eq 'admin-smoke')
    if ($channel.Count -ne 1) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_CATALOG_INVALID|$Version|channel" }
    $entry = $channel[0]
    try { $uri = [uri][string]$entry.source_uri } catch { throw "PCV_MANUAL_ADMIN_CAMPAIGN_CATALOG_INVALID|$Version|uri" }
    if (-not $uri.IsFile -or (Resolve-PcvPath $uri.LocalPath) -ne (Resolve-PcvPath $PackagePath) -or
        [string]$entry.version -ne $Version -or [string]$entry.channel -ne 'admin-smoke' -or
        [string]$entry.release_channel -ne 'admin-smoke' -or [string]$entry.signing_mode -ne 'AllowUnsignedDev' -or
        [string]$entry.expected_sha256 -ne (Get-PcvSha256 $PackagePath)) {
        throw "PCV_MANUAL_ADMIN_CAMPAIGN_CATALOG_INVALID|$Version|content"
    }
    ([uri](Resolve-PcvPath $CatalogPath)).AbsoluteUri
}
function Get-PcvHostIdentity {
    if ($HostIdentityOverride) { return $HostIdentityOverride }
    try {
        $machineGuid = [string](Get-ItemPropertyValue -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Cryptography' -Name MachineGuid -ErrorAction Stop)
        "$machineGuid|$env:COMPUTERNAME"
    } catch { throw 'PCV_MANUAL_ADMIN_BASELINE_HOST_IDENTITY_UNAVAILABLE' }
}
function Assert-PcvInstalledState([string]$Version) {
    $manifest = Read-PcvJson $script:installedManifestFull
    $service = Get-Service -Name $ServiceName -ErrorAction Stop
    if ([string]$manifest.version -ne $Version -or [string]$service.Status -ne 'Running') {
        throw "PCV_MANUAL_ADMIN_INSTALLED_STATE_INVALID|expected=$Version"
    }
    [ordered]@{ version = [string]$manifest.version; service_status = [string]$service.Status }
}
function Test-PcvExactBucketPass([string]$Id, $Summary) {
    if ($null -eq $Summary) { return $false }
    switch ($Id) {
        'manual-admin-rebaseline-readiness' {
            return [string]$Summary.reservation_status -eq 'reserved-and-matched' -and [bool]$Summary.actual_execution_eligible -and
                [string]$Summary.package_pair_input_status -eq 'ready-current-baseline-target-package-pair' -and
                [string]$Summary.baseline_version -eq $BaselineVersion -and [string]$Summary.target_version -eq $TargetVersion
        }
        'lifecycle-product-update-rollback' {
            return [bool]$Summary.ok -and [string]$Summary.status -eq 'PASS' -and
                (Test-Path $Summary.update_summary_path) -and (Test-Path $Summary.rollback_summary_path) -and (Test-Path $Summary.final_update_summary_path)
        }
        'clean-host-windows-update' {
            return [bool]$Summary.ok -and [string]$Summary.internal_clean_host_install_update_rollback_smoke -eq 'pass' -and
                [string]$Summary.blocker -eq 'none' -and [int]$Summary.install_exit_code -eq 0 -and [int]$Summary.update_exit_code -eq 0 -and
                [int]$Summary.rollback_exit_code -eq 0 -and [string]$Summary.baseline_manifest_version -eq $BaselineVersion -and
                [string]$Summary.updated_manifest_version -eq $TargetVersion -and [string]$Summary.final_manifest_version -eq $BaselineVersion -and
                [string]$Summary.final_service.state -eq 'Running' -and [int]$Summary.final_web_status_code -eq 200
        }
        'burn-bootstrapper-lifecycle' {
            return [bool]$Summary.ok -and [string]$Summary.status -eq 'PASS' -and [string]$Summary.final_manifest_version -eq $TargetVersion -and
                [string]$Summary.final_service_status -eq 'Running' -and @($Summary.exits.PSObject.Properties.Value | Where-Object { [int]$_ -ne 0 }).Count -eq 0
        }
        'msix-package-lifecycle-smoke' {
            return [bool]$Summary.ok -and [string]$Summary.status -eq 'PASS' -and [bool]$Summary.final_package_absent -and
                [bool]$Summary.final_smoke_service_absent -and [string]$Summary.final_msi_service_status -eq 'Running'
        }
        'installed-runtime-ops-summary' {
            return [bool]$Summary.ok -and [string]$Summary.status -eq 'PASS' -and $null -eq $Summary.error -and
                [string]$Summary.installed_version -eq $TargetVersion -and [string]$Summary.service_status -eq 'Running' -and
                [bool]$Summary.output.ok -and $null -eq $Summary.output.error
        }
    }
    $false
}

$root = Resolve-PcvPath $ArtifactRoot
$baselineRoot = Resolve-PcvPath $BaselinePackageRoot
$targetRoot = Resolve-PcvPath $TargetPackageRoot
$script:installedManifestFull = Resolve-PcvPath $InstalledManifestPath
$baseVhd = Resolve-PcvPath $BaseVhdPath
$baselineZip = Resolve-PcvPath $BaselineUpdatePackagePath
$targetZip = Resolve-PcvPath $TargetUpdatePackagePath
$baselineCatalog = Resolve-PcvPath $BaselineUpdateCatalogPath
$targetCatalog = Resolve-PcvPath $TargetUpdateCatalogPath
$wix = Resolve-PcvPath $WixPath
$makeAppx = Resolve-PcvPath $MakeAppxPath
$signTool = Resolve-PcvPath $SignToolPath
$pcvcli = Resolve-PcvPath $InstalledPcvCliPath
$layout = Resolve-PcvPath $MsixTemplateLayoutRoot
$internalCertificate = if ($InternalCertificatePath) { Resolve-PcvPath $InternalCertificatePath } else { '' }

if ($BaselineVersion -notmatch '^\d+\.\d+\.\d+-admin-smoke$' -or $TargetVersion -notmatch '^\d+\.\d+\.\d+-admin-smoke$') {
    throw 'PCV_MANUAL_ADMIN_CAMPAIGN_VERSION_INVALID'
}
$baselineComparable = [version]($BaselineVersion -replace '-admin-smoke$', '')
$targetComparable = [version]($TargetVersion -replace '-admin-smoke$', '')
if ($baselineComparable -ge $targetComparable) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_VERSION_INVALID|baseline must be less than target' }
if ([string]::IsNullOrWhiteSpace($CampaignId) -or [string]::IsNullOrWhiteSpace($VMSwitchName)) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_INPUT_INVALID' }
$baseline = Assert-PcvPackage $baselineRoot $BaselineVersion
$target = Assert-PcvPackage $targetRoot $TargetVersion
$baselineCatalogUri = Assert-PcvCatalog $baselineCatalog $baselineZip $BaselineVersion
$targetCatalogUri = Assert-PcvCatalog $targetCatalog $targetZip $TargetVersion
foreach ($item in @(
        @($script:installedManifestFull, 'installed-manifest'), @($baseVhd, 'base-vhd'), @($wix, 'wix'),
        @($makeAppx, 'makeappx'), @($signTool, 'signtool'), @($pcvcli, 'installed-pcvcli'))) {
    Assert-PcvFile $item[0] $item[1]
}
Assert-PcvDirectory $layout 'msix-template-layout'
Assert-PcvFile (Join-Path $layout 'AppxManifest.xml.template') 'msix-manifest-template'
if ($internalCertificate) { Assert-PcvFile $internalCertificate 'internal-certificate' }
$installedVersion = [string](Read-PcvJson $script:installedManifestFull).version
if ($installedVersion -notin @($BaselineVersion, $TargetVersion)) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_INSTALLED_VERSION_INVALID|$installedVersion" }
$thumbprint = ($SigningCertificateThumbprint -replace '\s', '').ToUpperInvariant()
if ($thumbprint -notmatch '^[A-F0-9]{40,64}$') { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_CERTIFICATE_FORMAT_INVALID' }

$toolRoot = $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $toolRoot '../../..')).Path
$helpers = [ordered]@{
    readiness = Join-Path $toolRoot 'New-PcvManualAdminRebaselineReadiness.ps1'
    clean = Join-Path $toolRoot 'Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1'
    descriptor = Join-Path $toolRoot 'New-PcvManualAdminCampaignDescriptor.ps1'
    reservation_module = Join-Path $toolRoot 'PcvManualAdminBaselineReservation.psm1'
    reservation_writer = Join-Path $toolRoot 'New-PcvManualAdminBaselineReservation.ps1'
    product = Join-Path $repoRoot 'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1'
    burn = Join-Path $toolRoot 'Invoke-PcvBurnBootstrapperLifecycle.ps1'
    msix = Join-Path $toolRoot 'Invoke-PcvMsixPackageLifecycleSmoke.ps1'
}
foreach ($entry in $helpers.GetEnumerator()) { Assert-PcvFile $entry.Value "canonical-$($entry.Key)" }

$bucketDefinitions = @(
    [ordered]@{ id = 'manual-admin-rebaseline-readiness'; relative_summary_path = 'manual-admin-rebaseline-readiness/summary.json'; pass_contract = @('reservation_status=reserved-and-matched','actual_execution_eligible=true','package_pair_input_status=ready-current-baseline-target-package-pair'); actions = @("& '$($helpers.readiness)' -Version '$BaselineVersion' -BaselineVersion '$BaselineVersion' -TargetVersion '$TargetVersion' -RouteParityArtifactRoot '$baselineRoot' -TargetPackageArtifactRoot '$targetRoot' -ForActualExecution -PlanOnly") },
    [ordered]@{ id = 'lifecycle-product-update-rollback'; relative_summary_path = 'lifecycle/product-update-rollback/summary.json'; actions = @("& '$($helpers.product)' -Action Update -UpdateCatalogUri '$targetCatalogUri' -UpdateChannel 'admin-smoke'", "& '$($helpers.product)' -Action Rollback", "& '$($helpers.product)' -Action Update -UpdateCatalogUri '$targetCatalogUri' -UpdateChannel 'admin-smoke'") },
    [ordered]@{ id = 'clean-host-windows-update'; relative_summary_path = 'clean-host-windows-update/summary.json'; actions = @("& '$($helpers.clean)' -VmRoot '<campaign-clean-host-vm-root>' -RemoveVmOnSuccess -RemoveVmOnFailure -UpdateChannel 'admin-smoke' -TargetSigningMode 'AllowUnsignedDev' -GuestPassword '<guest-credential-at-execution-boundary>'") },
    [ordered]@{ id = 'burn-bootstrapper-lifecycle'; relative_summary_path = 'burn-bootstrapper-lifecycle/summary.json'; actions = @("& '$($helpers.burn)' -TargetMsiPath '$($target.msi)' -TargetVersion '$TargetVersion' -WixPath '$wix' -Execute") },
    [ordered]@{ id = 'msix-package-lifecycle-smoke'; relative_summary_path = 'msix-package-lifecycle-smoke/summary.json'; actions = @("& '$($helpers.msix)' -TemplateLayoutRoot '$layout' -BaselinePayloadRoot '$($baseline.payload)' -TargetPayloadRoot '$($target.payload)' -Execute") },
    [ordered]@{ id = 'installed-runtime-ops-summary'; relative_summary_path = 'installed-runtime-ops-summary/summary.json'; actions = @("& '$pcvcli' --json ops summary") }
)
foreach ($bucket in $bucketDefinitions) {
    $bucket.summary_path = Join-Path $root $bucket.relative_summary_path
    if (-not (Test-PcvChildPath $bucket.summary_path $root)) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_ARTIFACT_ESCAPE' }
}
$alignmentActions = @(
    [ordered]@{ id = 'align-installed-target-to-baseline'; command = "Update baseline catalog '$baselineCatalogUri'" },
    [ordered]@{ id = 'verify-baseline-manifest-and-service'; command = "manifest '$BaselineVersion'; service Running" },
    [ordered]@{ id = 'create-or-validate-dedicated-host-reservation'; command = 'canonical reservation writer/validator' },
    [ordered]@{ id = 'run-readiness'; command = "readiness Version '$BaselineVersion' ForActualExecution" }
)
$plan = [ordered]@{
    schema_version = 1; contract = 'pcv-manual-admin-package-pair-campaign-v1'; campaign_id = $CampaignId
    mode = if ($PlanOnly) { 'PlanOnly' } else { 'Execute' }; baseline_version = $BaselineVersion; target_version = $TargetVersion
    installed_version = $installedVersion; artifact_root = $root; host_mutation_performed = $false
    baseline_alignment = [ordered]@{ required = $installedVersion -eq $TargetVersion; actions = $alignmentActions }
    credential_handling = '<guest-credential-at-execution-boundary>'; signing_certificate = 'format-validated-thumbprint-redacted'
    certificate_generation = 'forbidden'; public_trusted_signing = 'not-claimed'; external_stable_publication = 'out-of-scope'; buckets = $bucketDefinitions
}
New-Item -ItemType Directory -Force -Path $root | Out-Null
Write-PcvJson (Join-Path $root 'plan.json') $plan
$summary = [ordered]@{
    schema_version = 1; contract = 'pcv-manual-admin-package-pair-campaign-summary-v1'; campaign_id = $CampaignId
    mode = if ($PlanOnly) { 'PlanOnly' } else { 'Execute' }; ok = [bool]$PlanOnly; host_mutation_performed = $false
    bucket_results = @(); all_buckets_pass = $false; descriptor_eligible = $false; closed_descriptor_emitted = $false
    descriptor_path = $null; restoration_attempted = $false; public_trusted_signing = 'not-claimed'; external_stable_publication = 'out-of-scope'
}
Write-PcvJson (Join-Path $root 'summary.json') $summary
if ($PlanOnly) { return [pscustomobject]$summary }

if ($null -eq $GuestCredential) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_GUEST_CREDENTIAL_REQUIRED' }
$principal = [Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_ELEVATION_REQUIRED' }
if ($null -eq (Get-Command Get-VMHost -ErrorAction SilentlyContinue) -or $null -eq (Get-VMHost -ErrorAction Stop)) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_HYPERV_REQUIRED' }
if ($null -eq (Get-VMSwitch -Name $VMSwitchName -ErrorAction SilentlyContinue)) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_VM_SWITCH_NOT_FOUND' }
$certificate = Get-ChildItem Cert:\CurrentUser\My, Cert:\LocalMachine\My -ErrorAction SilentlyContinue |
    Where-Object { ($_.Thumbprint -replace '\s', '').ToUpperInvariant() -eq $thumbprint -and $_.HasPrivateKey } | Select-Object -First 1
if ($null -eq $certificate -or $certificate.NotBefore -gt (Get-Date) -or $certificate.NotAfter -le (Get-Date) -or
    @($certificate.EnhancedKeyUsageList | Where-Object ObjectId -eq '1.3.6.1.5.5.7.3.3').Count -eq 0) {
    throw 'PCV_MANUAL_ADMIN_CAMPAIGN_CERTIFICATE_NOT_FOUND'
}

Import-Module $helpers.reservation_module -Force
$mutationStarted = $false
$currentVersion = $installedVersion
$reservationPath = if ($BaselineReservationPath) { Resolve-PcvPath $BaselineReservationPath } else { Join-Path $root 'baseline-reservation/reservation.json' }
function Invoke-PcvProduct([hashtable]$Arguments) {
    $output = & $helpers.product @Arguments
    if (-not $?) { throw 'PCV_MANUAL_ADMIN_PRODUCT_FAILED' }
    try { ($output -join "`n") | ConvertFrom-Json } catch { throw 'PCV_MANUAL_ADMIN_PRODUCT_JSON_INVALID' }
}
# Host state around every bucket for the train evidence facts generator (design pcv-train-pair-orchestrator-v1 3a).
# No token, credential, or guest password is read or written here; a failed probe is recorded, never thrown.
$observationsPath = Join-Path $root 'observations.json'
$observations = [ordered]@{ schema_version = 1; contract = 'pcv-manual-admin-pair-observations-v1'; campaign_id = $CampaignId; entries = @() }
function Get-PcvVersionAt([string]$Path) {
    if (Test-Path -LiteralPath $Path) { [string](Read-PcvJson $Path).version } else { $null }
}
function Add-PcvObservation([string]$Point) {
    $entry = [ordered]@{ point = $Point; at = [datetimeoffset]::UtcNow.ToString('o') }
    try {
        $productRoot = Split-Path -Parent $script:installedManifestFull
        $entry.manifest_version = Get-PcvVersionAt $script:installedManifestFull
        $hostExe = Join-Path $productRoot 'DesktopNode.Host.exe'
        $entry.host_product_version = if (Test-Path -LiteralPath $hostExe) { (Get-Item -LiteralPath $hostExe).VersionInfo.ProductVersion } else { $null }
        $entry.previous_version = Get-PcvVersionAt (Join-Path "$productRoot.previous" 'product-manifest.json')
        $entry.failed_version = Get-PcvVersionAt (Join-Path "$productRoot.failed" 'product-manifest.json')
        $service = Get-CimInstance Win32_Service -Filter "Name='$ServiceName'" -ErrorAction SilentlyContinue
        $entry.service_state = if ($service) { [string]$service.State } else { 'absent' }
        $entry.service_start_mode = if ($service) { [string]$service.StartMode } else { $null }
        $entry.web_status = try { [int](Invoke-WebRequest -Uri 'http://127.0.0.1/' -UseBasicParsing -TimeoutSec 20).StatusCode } catch { 0 }
        $entry.boot_time = (Get-CimInstance Win32_OperatingSystem -ErrorAction SilentlyContinue).LastBootUpTime.ToUniversalTime().ToString('o')
        $entry.firewall_rule_count = @(Get-NetFirewallRule -DisplayName 'PureCVisor*' -ErrorAction SilentlyContinue).Count
        $entry.vms = @(Get-VM -ErrorAction SilentlyContinue | ForEach-Object { [ordered]@{ name = $_.Name; state = [string]$_.State } })
        $entry.arp = @(Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*', 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue |
            Where-Object { $_.PSObject.Properties['DisplayName'] -and [string]$_.DisplayName -like '*PureCVisor*' } |
            ForEach-Object { [ordered]@{ product_code = $_.PSChildName; display_version = if ($_.PSObject.Properties['DisplayVersion']) { [string]$_.DisplayVersion } else { $null } } })
    } catch {
        $entry.observation_error = $_.Exception.GetType().Name
    }
    $observations.entries += ,$entry
    Write-PcvJson $observationsPath $observations
}
function Ensure-PcvReservation {
    $identity = Get-PcvHostIdentity
    if (Test-Path -LiteralPath $reservationPath) {
        [void](Test-PcvManualAdminBaselineReservation -ReservationPath $reservationPath -ExpectedCampaignId $CampaignId `
            -ExpectedBaselineVersion $BaselineVersion -ExpectedTargetVersion $TargetVersion `
            -ExpectedHostFingerprint (Get-PcvManualAdminBaselineSha256 $identity) -Now ([datetimeoffset]::UtcNow))
    } else {
        $reservationRoot = Split-Path -Parent $reservationPath
        & $helpers.reservation_writer -CampaignId $CampaignId -BaselineVersion $BaselineVersion -TargetVersion $TargetVersion `
            -ReservationKind dedicated-host -ResourceReference $CampaignId -InstalledManifestPath $script:installedManifestFull `
            -ArtifactRoot $reservationRoot -HostIdentityOverride $identity | Out-Null
        if (-not (Test-Path -LiteralPath $reservationPath)) { throw 'PCV_MANUAL_ADMIN_CAMPAIGN_RESERVATION_CREATE_FAILED' }
    }
    $identity
}

$bucketResults = @()
$stoppedAfter = $null
Add-PcvObservation 'start'
try {
    if ($installedVersion -eq $TargetVersion) {
        $mutationStarted = $true
        [void](Invoke-PcvProduct @{ Action = 'Update'; UpdateCatalogUri = $baselineCatalogUri; UpdateChannel = 'admin-smoke' })
        $currentVersion = $BaselineVersion
        [void](Assert-PcvInstalledState $BaselineVersion)
        Add-PcvObservation 'after:baseline-alignment'
    }
    $hostIdentity = Ensure-PcvReservation
    foreach ($bucket in $bucketDefinitions) {
        $bucketRoot = Split-Path -Parent $bucket.summary_path
        New-Item -ItemType Directory -Force -Path $bucketRoot | Out-Null
        $mutationStarted = $true
        Add-PcvObservation "before:$($bucket.id)"
        switch ($bucket.id) {
            'manual-admin-rebaseline-readiness' {
                & $helpers.readiness -ArtifactRoot $bucketRoot -Version $BaselineVersion -BaselineVersion $BaselineVersion -TargetVersion $TargetVersion `
                    -InstalledManifestPath $script:installedManifestFull -RouteParityArtifactRoot $baselineRoot -TargetPackageArtifactRoot $targetRoot `
                    -CampaignId $CampaignId -BaselineReservationPath $reservationPath -HostIdentityOverride $hostIdentity -ForActualExecution -PlanOnly | Out-Null
            }
            'lifecycle-product-update-rollback' {
                $mutationStarted = $true
                $productUpdateSummaryPath = Join-Path $bucketRoot 'update-summary.json'
                $productRollbackSummaryPath = Join-Path $bucketRoot 'rollback-summary.json'
                $productFinalSummaryPath = Join-Path $bucketRoot 'final-update-summary.json'
                $result = Invoke-PcvProduct @{ Action = 'Update'; UpdateCatalogUri = $targetCatalogUri; UpdateChannel = 'admin-smoke' }
                $currentVersion = $TargetVersion; $state = Assert-PcvInstalledState $TargetVersion
                $updateSummary = [ordered]@{ ok = [bool]$result.ok; action = 'Update'; installed_version = $state.version; service_status = $state.service_status }
                Write-PcvJson $productUpdateSummaryPath $updateSummary
                Add-PcvObservation 'after:lifecycle-update'
                $result = Invoke-PcvProduct @{ Action = 'Rollback' }
                $currentVersion = $BaselineVersion; $state = Assert-PcvInstalledState $BaselineVersion
                $rollbackSummary = [ordered]@{ ok = [bool]$result.ok; action = 'Rollback'; installed_version = $state.version; service_status = $state.service_status }
                Write-PcvJson $productRollbackSummaryPath $rollbackSummary
                Add-PcvObservation 'after:lifecycle-rollback'
                $result = Invoke-PcvProduct @{ Action = 'Update'; UpdateCatalogUri = $targetCatalogUri; UpdateChannel = 'admin-smoke' }
                $currentVersion = $TargetVersion; $state = Assert-PcvInstalledState $TargetVersion
                $finalSummary = [ordered]@{ ok = [bool]$result.ok; action = 'Update'; installed_version = $state.version; service_status = $state.service_status }
                Write-PcvJson $productFinalSummaryPath $finalSummary
                Write-PcvJson $bucket.summary_path ([ordered]@{
                    ok = $updateSummary.ok -and $rollbackSummary.ok -and $finalSummary.ok; status = 'PASS'
                    update_summary_path = $productUpdateSummaryPath; rollback_summary_path = $productRollbackSummaryPath; final_update_summary_path = $productFinalSummaryPath
                })
            }
            'clean-host-windows-update' {
                $cleanArguments = @{
                    ArtifactRoot = $bucketRoot; BaseVhdPath = $baseVhd; BaselineMsiPath = $baseline.msi; UpdatePackagePath = $targetZip
                    VmRoot = (Join-Path $bucketRoot 'vm'); VmName = if ($VmName) { $VmName } else { "pcv-$CampaignId" }; VMSwitchName = $VMSwitchName
                    GuestUser = $GuestCredential.UserName; GuestPassword = $GuestCredential.GetNetworkCredential().Password
                    BaselineVersion = $BaselineVersion; TargetVersion = $TargetVersion; UpdateChannel = 'admin-smoke'; TargetSigningMode = 'AllowUnsignedDev'
                    InstallWindowsUpdates = $true; RemoveVmOnSuccess = $true; RemoveVmOnFailure = $true
                }
                if ($internalCertificate) { $cleanArguments.InternalRootCertificatePath = $internalCertificate }
                & $helpers.clean @cleanArguments | Out-Null
            }
            'burn-bootstrapper-lifecycle' {
                & $helpers.burn -ArtifactRoot $bucketRoot -TargetMsiPath $target.msi -TargetVersion $TargetVersion -WixPath $wix `
                    -ProductHelperPath $helpers.product -InstalledManifestPath $script:installedManifestFull -ServiceName $ServiceName -Execute | Out-Null
                $currentVersion = $TargetVersion
            }
            'msix-package-lifecycle-smoke' {
                & $helpers.msix -ArtifactRoot $bucketRoot -TemplateLayoutRoot $layout -BaselinePayloadRoot $baseline.payload -TargetPayloadRoot $target.payload `
                    -BaselineVersion $BaselineVersion -TargetVersion $TargetVersion -MakeAppxPath $makeAppx -SignToolPath $signTool `
                    -SigningCertificateThumbprint $thumbprint -InstalledManifestPath $script:installedManifestFull -MsiServiceName $ServiceName -Execute | Out-Null
            }
            'installed-runtime-ops-summary' {
                $opsOutputPath = Join-Path $bucketRoot 'ops-summary.json'
                $opsStderrPath = Join-Path $bucketRoot 'ops-summary.stderr.txt'
                $raw = & $pcvcli --json ops summary 2> $opsStderrPath
                if ($LASTEXITCODE -ne 0) { throw 'PCV_MANUAL_ADMIN_RUNTIME_OPS_EXIT' }
                $rawText = $raw -join "`n"
                [IO.File]::WriteAllText($opsOutputPath, $rawText + "`n", [Text.UTF8Encoding]::new($false))
                $ops = $rawText | ConvertFrom-Json
                $state = Assert-PcvInstalledState $TargetVersion
                $errorValue = if ($ops.PSObject.Properties.Name -contains 'error') { $ops.error } else { $null }
                # Unauthenticated read must be refused; only the status and error code are kept.
                $unauthenticated = try {
                    [void](Invoke-WebRequest -Uri 'http://127.0.0.1:7777/api/v1/ops/summary' -UseBasicParsing -TimeoutSec 15)
                    [ordered]@{ status_code = 200; error_code = $null }
                } catch {
                    $code = try { (($_.ErrorDetails.Message | ConvertFrom-Json).error.code) } catch { $null }
                    [ordered]@{ status_code = [int]$_.Exception.Response.StatusCode; error_code = $code }
                }
                $tokenPattern = '(?i)(bearer\s+[a-z0-9._-]{16,}|"(api_)?token"\s*:\s*"[^"]{16,}"|eyJ[a-z0-9_-]{10,})'
                Write-PcvJson $bucket.summary_path ([ordered]@{
                    ok = [bool]$ops.ok -and $null -eq $errorValue; status = 'PASS'; error = $errorValue
                    installed_version = $state.version; service_status = $state.service_status
                    operation = [string]$ops.operation; ops_summary_sha256 = Get-PcvSha256 $opsOutputPath
                    stderr_bytes = (Get-Item -LiteralPath $opsStderrPath).Length
                    errors_count = @($ops.data.errors).Count; vm_total = $ops.data.vm_counts.total
                    token_like_count = [regex]::Matches($rawText, $tokenPattern).Count
                    unauthenticated = $unauthenticated
                    output = [ordered]@{ ok = [bool]$ops.ok; error = $errorValue }
                })
            }
        }
        Add-PcvObservation "after:$($bucket.id)"
        $bucketSummary = Read-PcvJson $bucket.summary_path
        $status = if (Test-PcvExactBucketPass $bucket.id $bucketSummary) { 'PASS' } else { 'FAIL' }
        $bucketResults += ,([ordered]@{ id = $bucket.id; status = $status; summary_path = $bucket.summary_path })
        if ($status -ne 'PASS') { $stoppedAfter = $bucket.id; break }
    }
} catch {
    $stoppedAfter = if ($stoppedAfter) { $stoppedAfter } elseif ($bucketResults.Count -lt 6) { $bucketDefinitions[$bucketResults.Count].id } else { 'post-bucket' }
    $bucketResults += ,([ordered]@{ id = $stoppedAfter; status = 'FAIL'; error = $_.Exception.Message })
} finally {
    if ($mutationStarted -and $currentVersion -ne $TargetVersion) {
        $summary.restoration_attempted = $true
        try {
            [void](Invoke-PcvProduct @{ Action = 'Update'; UpdateCatalogUri = $targetCatalogUri; UpdateChannel = 'admin-smoke' })
            [void](Assert-PcvInstalledState $TargetVersion)
            $summary.restoration_status = 'PASS'
        } catch { $summary.restoration_status = 'FAIL' }
        Add-PcvObservation 'after:restoration'
    }
}

$allPass = $bucketResults.Count -eq 6 -and @($bucketResults | Where-Object status -ne 'PASS').Count -eq 0
$summary.bucket_results = $bucketResults; $summary.all_buckets_pass = $allPass; $summary.descriptor_eligible = $allPass
$summary.ok = $allPass; $summary.host_mutation_performed = $mutationStarted
$summary.observations_path = $observationsPath
if ($allPass) {
    $descriptorRoot = Join-Path $root 'manual-admin-campaign-descriptor'
    $productUpdateSummaryPath = Join-Path $root 'lifecycle/product-update-rollback/update-summary.json'
    $productRollbackSummaryPath = Join-Path $root 'lifecycle/product-update-rollback/rollback-summary.json'
    & $helpers.descriptor -ArtifactRoot $descriptorRoot -CampaignArtifactRoot $root -BaselineVersion $BaselineVersion -TargetVersion $TargetVersion `
        -ReadinessSummaryPath $bucketDefinitions[0].summary_path -ProductUpdateSummaryPath $productUpdateSummaryPath `
        -ProductRollbackSummaryPath $productRollbackSummaryPath -CleanHostSummaryPath $bucketDefinitions[2].summary_path `
        -BurnLifecycleSummaryPath $bucketDefinitions[3].summary_path -MsixLifecycleSummaryPath $bucketDefinitions[4].summary_path `
        -InstalledRuntimeOpsSummaryPath $bucketDefinitions[5].summary_path -DescriptorBatchId "$CampaignId-closed" -PlanOnly | Out-Null
    $descriptorSummary = Read-PcvJson (Join-Path $descriptorRoot 'summary.json')
    if (-not [bool]$descriptorSummary.ok -or [string]$descriptorSummary.overall_status -ne 'pass' -or
        [int]$descriptorSummary.runner_count -ne 6 -or [int]$descriptorSummary.missing_count -ne 0 -or [int]$descriptorSummary.not_pass_count -ne 0) {
        throw 'PCV_MANUAL_ADMIN_CAMPAIGN_DESCRIPTOR_NOT_CLOSED'
    }
    Complete-PcvCampaignClosure -Summary $summary -SummaryPath (Join-Path $root 'summary.json') -CampaignRoot $root `
        -DescriptorRoot $descriptorRoot -DescriptorPath ([string]$descriptorSummary.descriptor_path) `
        -ReservationPath $reservationPath -DescriptorValidated
}
Write-PcvJson (Join-Path $root 'summary.json') $summary
[pscustomobject]$summary
if (-not $allPass) { throw "PCV_MANUAL_ADMIN_CAMPAIGN_FAILED|$stoppedAfter" }
