[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactRoot,
    [Parameter(Mandatory)][string]$TargetMsiPath,
    [Parameter(Mandatory)][string]$TargetVersion,
    [Parameter(Mandatory)][string]$WixPath,
    [Parameter(Mandatory)][string]$ProductHelperPath,
    [string]$InstalledManifestPath = 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json',
    [string]$ExpectedProductRoot = '',
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [switch]$PlanOnly,
    [switch]$Execute
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($PlanOnly -eq $Execute) { throw 'PCV_BURN_LIFECYCLE_MODE_REQUIRED|Pass exactly one mode.' }

function Resolve-PcvFullPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}
function Assert-PcvFile([string]$Path, [string]$Name) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "PCV_BURN_LIFECYCLE_INPUT_MISSING|$Name|$Path" }
}
function Write-PcvJson([string]$Path, $Value) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 24) + "`n", [Text.UTF8Encoding]::new($false))
}
function Invoke-PcvProcess([string]$Id, [string]$File, [string[]]$Arguments) {
    $process = Start-Process -FilePath $File -ArgumentList $Arguments -Wait -PassThru -NoNewWindow
    $script:summary.exits[$Id] = [int]$process.ExitCode
    if ($process.ExitCode -ne 0) { throw "$Id exit $($process.ExitCode)" }
}
function Get-PcvProductState([string]$Phase, [bool]$MustExist) {
    $manifest = if (Test-Path -LiteralPath $InstalledManifestPath -PathType Leaf) {
        Get-Content -Raw -LiteralPath $InstalledManifestPath | ConvertFrom-Json
    } else { $null }
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    $productRootExists = Test-Path -LiteralPath $script:productRoot -PathType Container

    if (-not $MustExist) {
        if ($null -ne $service -or $null -ne $manifest -or $productRootExists) { throw "$Phase product remains" }
        return [ordered]@{ Phase = $Phase; Absent = $true }
    }

    if ($null -eq $manifest -or $null -eq $service -or -not $productRootExists) {
        throw "PCV_BURN_LIFECYCLE_PRESTATE_INVALID|$Phase|required product state missing"
    }
    $cim = Get-CimInstance Win32_Service -Filter "Name='$ServiceName'" -ErrorAction Stop
    $startType = if ([string]$cim.StartMode -eq 'Auto') { 'Automatic' } else { [string]$cim.StartMode }
    $state = [ordered]@{
        Phase = $Phase
        Status = [string]$service.Status
        StartType = $startType
        Version = [string]$manifest.version
        ProductRoot = $script:productRoot
    }
    if ($state.Status -ne 'Running' -or $state.StartType -ne 'Automatic' -or $state.Version -ne $TargetVersion) {
        throw "PCV_BURN_LIFECYCLE_PRESTATE_INVALID|$Phase|target state invalid"
    }
    $state
}

$root = Resolve-PcvFullPath $ArtifactRoot
$msi = Resolve-PcvFullPath $TargetMsiPath
$wix = Resolve-PcvFullPath $WixPath
$productHelper = Resolve-PcvFullPath $ProductHelperPath
Assert-PcvFile $msi 'target-msi'
Assert-PcvFile $wix 'wix'
Assert-PcvFile $productHelper 'product-helper'

$bundle = Join-Path $root "PureCVisorDesktopNode-$TargetVersion-bootstrapper.exe"
$source = Join-Path $root 'Bundle.wxs'
$actions = @(
    [ordered]@{ id = 'build-target-bundle'; command = "'$wix' build '$source' -ext WixToolset.BootstrapperApplications.wixext -o '$bundle'" },
    [ordered]@{ id = 'install'; command = "'$bundle' /install /quiet /norestart /log '<install.log>'" },
    [ordered]@{ id = 'repair'; command = "'$bundle' /repair /quiet /norestart /log '<repair.log>'" },
    [ordered]@{ id = 'remove'; command = "'$bundle' /uninstall /quiet /norestart /log '<remove.log>'" },
    [ordered]@{ id = 'restore-target-msi'; command = "msiexec /i '$msi' /qn /norestart REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /L*v '<restore.log>'" },
    [ordered]@{ id = 'repair-installed'; command = "'$productHelper' -Action RepairInstalled" },
    [ordered]@{ id = 'verify-target-running'; command = "Status Running StartType Automatic manifest '$TargetVersion'" }
)
$plan = [ordered]@{
    schema_version = 1
    contract = 'pcv-burn-bootstrapper-lifecycle-v1'
    mode = if ($PlanOnly) { 'PlanOnly' } else { 'Execute' }
    artifact_root = $root
    host_mutation_performed = $false
    actions = $actions
}
New-Item -ItemType Directory -Force -Path $root | Out-Null
Write-PcvJson (Join-Path $root 'plan.json') $plan
if ($PlanOnly) {
    $planned = [ordered]@{ ok = $true; status = 'PLANNED'; host_mutation_performed = $false }
    Write-PcvJson (Join-Path $root 'summary.json') $planned
    return [pscustomobject]$planned
}

$manifestForRoot = Get-Content -Raw -LiteralPath $InstalledManifestPath | ConvertFrom-Json
$resolvedProductRoot = if ($ExpectedProductRoot) {
    $ExpectedProductRoot
} elseif ($manifestForRoot.paths.product_root) {
    [string]$manifestForRoot.paths.product_root
} else { '' }
if ([string]::IsNullOrWhiteSpace($resolvedProductRoot)) { throw 'PCV_BURN_LIFECYCLE_PRESTATE_INVALID|expected product root missing' }
$script:productRoot = Resolve-PcvFullPath $resolvedProductRoot
$prestate = Get-PcvProductState 'prestate' $true

$summary = [ordered]@{
    ok = $false
    status = 'FAIL'
    host_mutation_performed = $false
    mutation_started = $false
    restoration_attempted = $false
    restoration_status = 'NOT-RUN'
    restoration_error = $null
    exits = [ordered]@{}
    states = [ordered]@{ prestate = $prestate }
}
$failure = $null
try {
    $escapedMsi = [Security.SecurityElement]::Escape($msi)
    $wxs = "<Wix xmlns=`"http://wixtoolset.org/schemas/v4/wxs`"><Bundle Name=`"PureCVisor Desktop Node`" Version=`"$(($TargetVersion -replace '-admin-smoke','') + '.0')`" Manufacturer=`"PureCVisor`" UpgradeCode=`"{8F455BB4-640E-47A2-A982-338C7A6318B5}`"><BootstrapperApplication><bal:WixStandardBootstrapperApplication xmlns:bal=`"http://wixtoolset.org/schemas/v4/wxs/bal`" Theme=`"hyperlinkLicense`" LicenseUrl=`"`" /></BootstrapperApplication><Chain><MsiPackage Id=`"PureCVisorDesktopNodeMsi`" SourceFile=`"$escapedMsi`" Compressed=`"yes`" Vital=`"yes`" Permanent=`"no`" /></Chain></Bundle></Wix>"
    [IO.File]::WriteAllText($source, $wxs, [Text.UTF8Encoding]::new($false))
    Invoke-PcvProcess 'build' $wix @('build', $source, '-ext', 'WixToolset.BootstrapperApplications.wixext', '-o', $bundle)

    $summary.mutation_started = $true
    $summary.host_mutation_performed = $true
    Invoke-PcvProcess 'install' $bundle @('/install', '/quiet', '/norestart', '/log', (Join-Path $root 'install.log'))
    $summary.states.'install-state' = Get-PcvProductState 'install-state' $true
    Invoke-PcvProcess 'repair' $bundle @('/repair', '/quiet', '/norestart', '/log', (Join-Path $root 'repair.log'))
    $summary.states.'repair-state' = Get-PcvProductState 'repair-state' $true
    Invoke-PcvProcess 'remove' $bundle @('/uninstall', '/quiet', '/norestart', '/log', (Join-Path $root 'remove.log'))
    $summary.states.'remove-absence' = Get-PcvProductState 'remove-absence' $false
} catch {
    $failure = $_
} finally {
    if ($summary.mutation_started) {
        $summary.restoration_attempted = $true
        try {
            Invoke-PcvProcess 'restore-target-msi' 'msiexec.exe' @('/i', $msi, '/qn', '/norestart', 'REBOOT=ReallySuppress', 'MSIRESTARTMANAGERCONTROL=Disable', '/L*v', (Join-Path $root 'restore-target-msi.log'))
            & $productHelper -Action RepairInstalled | Out-Null
            if (-not $?) { throw 'RepairInstalled failed' }
            $final = Get-PcvProductState 'final-target-state' $true
            $summary.final_manifest_version = $final.Version
            $summary.final_service_status = $final.Status
            $summary.final_service_start_type = $final.StartType
            $summary.restoration_status = 'PASS'
        } catch {
            $summary.restoration_status = 'FAIL'
            $summary.restoration_error = $_.Exception.Message
            if ($null -eq $failure) { $failure = $_ }
        }
    }
    if ($null -eq $failure) {
        $summary.ok = $true
        $summary.status = 'PASS'
    } else {
        $summary.error = $failure.Exception.Message
    }
    Write-PcvJson (Join-Path $root 'summary.json') $summary
}

[pscustomobject]$summary
if ($null -ne $failure) { throw $failure }
