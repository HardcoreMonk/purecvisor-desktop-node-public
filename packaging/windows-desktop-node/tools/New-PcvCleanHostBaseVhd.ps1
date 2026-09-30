[CmdletBinding(DefaultParameterSetName = 'Build')]
param(
    [Parameter(Mandatory, ParameterSetName = 'Build')][string]$SourceVhdPath,
    [Parameter(Mandatory, ParameterSetName = 'Build')][string]$PackagePath,
    [Parameter(Mandatory, ParameterSetName = 'Build')][string]$ExpectedKb,
    [Parameter(Mandatory, ParameterSetName = 'Build')][string]$ExpectedPackageSha256,
    [Parameter(Mandatory, ParameterSetName = 'Build')][int]$ExpectedUbr,
    [Parameter(ParameterSetName = 'Build')][string]$BuildDate = (Get-Date -Format 'yyyyMMdd'),
    [Parameter(ParameterSetName = 'Build')][string]$MountRoot = '',
    [Parameter(Mandatory, ParameterSetName = 'SetCurrent')][string]$SetCurrentBasePath,
    [switch]$Execute
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Builds a dated clean-host base VHD by servicing a copy of the evaluation VHD offline with one LCU package,
# or points current-base.json at a verified base. Without -Execute it only reports the plan. The source VHD
# is read, never written, and the package comes from an operator download; this tool never downloads.
$script:BaseBuild = 20348
$script:BaseSchema = 'pcv-clean-host-base-vhd-v1'
$script:CurrentSchema = 'pcv-clean-host-current-base-v1'
$script:CurrentFileName = 'current-base.json'
$script:RetainedPreviousBases = 2

function Resolve-PcvPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}
function Assert-PcvFile([string]$Path, [string]$Code) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "$Code|$Path" }
}
function Read-PcvJson([string]$Path, [string]$Code) {
    try { Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json }
    catch { throw "$Code|$Path" }
}
function Write-PcvJson([string]$Path, $Value) {
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 8) + "`n", [Text.UTF8Encoding]::new($false))
}
function Get-PcvSha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
function Get-PcvUtcNow { (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') }
# A Microsoft Store (MSIX) PowerShell cannot activate the image's dismhost.exe COM server (0x80040154).
function Test-PcvPackagedHost { $PSHOME -like '*\WindowsApps\*' }
function Expand-PcvMsuPackage {
    param([Parameter(Mandatory)][string]$PackagePath, [Parameter(Mandatory)][string]$Destination)
    & expand.exe "-F:*.cab" $PackagePath $Destination | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "PCV_BASE_VHD_MSU_EXPAND_FAILED|exit=$LASTEXITCODE|$PackagePath" }
}
function Test-PcvElevated {
    ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
}
function Get-PcvOfflineImageBuild {
    param([Parameter(Mandatory)][string]$MountPath)
    $hive = Join-Path $MountPath 'Windows\System32\config\SOFTWARE'
    if (-not (Test-Path -LiteralPath $hive -PathType Leaf)) { throw "PCV_BASE_VHD_OFFLINE_HIVE_MISSING|$hive" }
    $key = 'PCV_BASE_VHD_SOFTWARE'
    & reg.exe load "HKLM\$key" $hive | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "PCV_BASE_VHD_OFFLINE_HIVE_LOAD_FAILED|exit=$LASTEXITCODE" }
    try {
        $item = Get-ItemProperty -LiteralPath "Registry::HKEY_LOCAL_MACHINE\$key\Microsoft\Windows NT\CurrentVersion"
        [pscustomobject]@{ build = [int]$item.CurrentBuild; ubr = [int]$item.UBR; edition_id = [string]$item.EditionID }
    }
    finally {
        $item = $null
        [GC]::Collect()
        [GC]::WaitForPendingFinalizers()
        for ($attempt = 1; $attempt -le 5; $attempt++) {
            & reg.exe unload "HKLM\$key" | Out-Null
            if ($LASTEXITCODE -eq 0) { break }
            Start-Sleep -Seconds 2
        }
        if ($LASTEXITCODE -ne 0) { throw "PCV_BASE_VHD_OFFLINE_HIVE_UNLOAD_FAILED|exit=$LASTEXITCODE" }
    }
}
function Read-PcvBaseSidecar([string]$BasePath) {
    $sidecarPath = "$BasePath.base.json"
    Assert-PcvFile $sidecarPath 'PCV_BASE_VHD_SIDECAR_MISSING'
    $sidecar = Read-PcvJson $sidecarPath 'PCV_BASE_VHD_SIDECAR_INVALID'
    foreach ($name in @('schema', 'base_file', 'base_sha256', 'base_size_bytes', 'build', 'ubr', 'kb', 'created_utc')) {
        if ($null -eq $sidecar.PSObject.Properties[$name]) { throw "PCV_BASE_VHD_SIDECAR_INVALID|missing=$name|$sidecarPath" }
    }
    if ($sidecar.schema -ne $script:BaseSchema) { throw "PCV_BASE_VHD_SIDECAR_INVALID|schema=$($sidecar.schema)" }
    if ($sidecar.base_file -ne (Split-Path -Leaf $BasePath)) { throw "PCV_BASE_VHD_SIDECAR_MISMATCH|base_file=$($sidecar.base_file)" }
    [pscustomobject]@{ path = $sidecarPath; value = $sidecar }
}

if ($PSCmdlet.ParameterSetName -eq 'SetCurrent') {
    $basePath = Resolve-PcvPath $SetCurrentBasePath
    Assert-PcvFile $basePath 'PCV_BASE_VHD_BASE_MISSING'
    $sidecar = Read-PcvBaseSidecar $basePath
    $baseItem = Get-Item -LiteralPath $basePath
    if ([int64]$sidecar.value.base_size_bytes -ne $baseItem.Length) {
        throw "PCV_BASE_VHD_SIDECAR_MISMATCH|size=$($baseItem.Length)|sidecar=$($sidecar.value.base_size_bytes)"
    }
    $baseSha256 = Get-PcvSha256 $basePath
    if ($baseSha256 -ne $sidecar.value.base_sha256) { throw "PCV_BASE_VHD_SIDECAR_MISMATCH|sha256=$baseSha256" }

    $cacheRoot = Split-Path -Parent $basePath
    $currentPath = Join-Path $cacheRoot $script:CurrentFileName
    $previousBaseFile = $null
    if (Test-Path -LiteralPath $currentPath -PathType Leaf) {
        $previous = Read-PcvJson $currentPath 'PCV_BASE_VHD_CURRENT_INVALID'
        $previousBaseFile = [string]$previous.base_file
    }
    $generated = @(Get-ChildItem -LiteralPath $cacheRoot -Filter '*.vhd.base.json' -File |
        ForEach-Object {
            $value = Read-PcvJson $_.FullName 'PCV_BASE_VHD_SIDECAR_INVALID'
            [pscustomobject]@{ base_file = [string]$value.base_file; created_utc = [string]$value.created_utc }
        } |
        Where-Object { $_.base_file -ne $baseItem.Name } |
        Sort-Object -Property created_utc -Descending)
    $pruneCandidates = @($generated | Select-Object -Skip $script:RetainedPreviousBases | ForEach-Object { $_.base_file })

    $current = [ordered]@{
        schema = $script:CurrentSchema
        base_file = $baseItem.Name
        sidecar_file = Split-Path -Leaf $sidecar.path
        base_sha256 = $baseSha256
        build = [int]$sidecar.value.build
        ubr = [int]$sidecar.value.ubr
        kb = [string]$sidecar.value.kb
        previous_base_file = $previousBaseFile
        updated_utc = Get-PcvUtcNow
    }
    if ($Execute) { Write-PcvJson $currentPath $current }
    return [pscustomobject][ordered]@{
        mode = if ($Execute) { 'execute' } else { 'plan' }
        action = 'set-current'
        current_path = $currentPath
        current = $current
        prune_candidates = $pruneCandidates
        prune_performed = $false
        writes_performed = [bool]$Execute
    }
}

$sourcePath = Resolve-PcvPath $SourceVhdPath
$packageFullPath = Resolve-PcvPath $PackagePath
Assert-PcvFile $sourcePath 'PCV_BASE_VHD_SOURCE_MISSING'
Assert-PcvFile $packageFullPath 'PCV_BASE_VHD_PACKAGE_MISSING'
if ($ExpectedKb -notmatch '^KB\d{6,8}$') { throw "PCV_BASE_VHD_KB_INVALID|$ExpectedKb" }
$packageLeaf = Split-Path -Leaf $packageFullPath
if ($packageLeaf -notmatch '\.(msu|cab)$') { throw "PCV_BASE_VHD_PACKAGE_TYPE_INVALID|$packageLeaf" }
if ($packageLeaf.IndexOf($ExpectedKb, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
    throw "PCV_BASE_VHD_PACKAGE_KB_MISMATCH|$packageLeaf|$ExpectedKb"
}
if ($BuildDate -notmatch '^20\d{6}$') { throw "PCV_BASE_VHD_DATE_INVALID|$BuildDate" }
if ($ExpectedUbr -le 0) { throw "PCV_BASE_VHD_UBR_INVALID|$ExpectedUbr" }
$packageSha256 = Get-PcvSha256 $packageFullPath
if ($packageSha256 -ne $ExpectedPackageSha256.ToLowerInvariant()) {
    throw "PCV_BASE_VHD_PACKAGE_HASH_MISMATCH|actual=$packageSha256|expected=$ExpectedPackageSha256"
}

$cacheRoot = Split-Path -Parent $sourcePath
$baseFile = "$($script:BaseBuild).$ExpectedUbr-$BuildDate.vhd"
$basePath = Join-Path $cacheRoot $baseFile
$sidecarPath = "$basePath.base.json"
if ([string]::Equals($basePath, $sourcePath, [StringComparison]::OrdinalIgnoreCase)) {
    throw "PCV_BASE_VHD_TARGET_IS_SOURCE|$basePath"
}
foreach ($existing in @($basePath, $sidecarPath)) {
    if (Test-Path -LiteralPath $existing) { throw "PCV_BASE_VHD_TARGET_EXISTS|$existing" }
}
$mountPath = if ([string]::IsNullOrWhiteSpace($MountRoot)) {
    Join-Path ([IO.Path]::GetTempPath()) "pcv-base-vhd-mount-$($script:BaseBuild).$ExpectedUbr-$BuildDate"
}
else {
    Resolve-PcvPath $MountRoot
}
$dismLogPath = Join-Path $cacheRoot "$baseFile.dism.log"
$packageExtractPath = "$mountPath.packages"

$result = [ordered]@{
    mode = if ($Execute) { 'execute' } else { 'plan' }
    action = 'build'
    source_path = $sourcePath
    package_path = $packageFullPath
    package_sha256 = $packageSha256
    kb = $ExpectedKb.ToUpperInvariant()
    expected_ubr = $ExpectedUbr
    base_path = $basePath
    sidecar_path = $sidecarPath
    mount_path = $mountPath
    dism_log_path = $dismLogPath
    steps = @('expand-package', 'copy-source', 'mount-image', 'add-servicing-stack', 'add-package', 'verify-offline-ubr', 'dismount-save', 'hash-base', 'write-sidecar')
    source_modified = $false
    download_performed = $false
    current_base_changed = $false
    writes_performed = $false
}
if (-not $Execute) { return [pscustomobject]$result }

if (-not (Test-PcvElevated)) { throw 'PCV_BASE_VHD_ELEVATION_REQUIRED|Mounting and servicing a VHD requires an administrator shell.' }
if (Test-PcvPackagedHost) {
    throw "PCV_BASE_VHD_PACKAGED_HOST_UNSUPPORTED|$PSHOME|Run from Windows PowerShell (powershell.exe) or a non-Store PowerShell."
}
foreach ($workPath in @($mountPath, $packageExtractPath)) {
    if ((Test-Path -LiteralPath $workPath) -and @(Get-ChildItem -LiteralPath $workPath -Force).Count -gt 0) {
        throw "PCV_BASE_VHD_MOUNT_NOT_EMPTY|$workPath"
    }
}

$sourceSha256 = Get-PcvSha256 $sourcePath
$result.writes_performed = $true
New-Item -ItemType Directory -Force -Path $mountPath | Out-Null
$mounted = $false
$saved = $false
try {
    # The combined SSU+LCU .msu cannot be added to the 20348.169 image in one step: the LCU needs a newer
    # servicing stack than the image has (0x800f0823). Apply the SSU cab first, then the LCU cab.
    $applyPackages = @()
    if ($packageLeaf -match '\.msu$') {
        New-Item -ItemType Directory -Force -Path $packageExtractPath | Out-Null
        Expand-PcvMsuPackage -PackagePath $packageFullPath -Destination $packageExtractPath
        $servicingStack = @(Get-ChildItem -LiteralPath $packageExtractPath -Filter 'SSU-*.cab' -File)
        $cumulative = @(Get-ChildItem -LiteralPath $packageExtractPath -Filter '*.cab' -File |
            Where-Object { $_.Name -notlike 'SSU-*' -and $_.Name.IndexOf($ExpectedKb, [StringComparison]::OrdinalIgnoreCase) -ge 0 })
        if ($servicingStack.Count -gt 1 -or $cumulative.Count -ne 1) {
            throw "PCV_BASE_VHD_MSU_LAYOUT_INVALID|ssu=$($servicingStack.Count)|lcu=$($cumulative.Count)"
        }
        $applyPackages = @($servicingStack) + @($cumulative)
    }
    else {
        $applyPackages = @(Get-Item -LiteralPath $packageFullPath)
    }
    $appliedPackages = @($applyPackages | ForEach-Object {
            [ordered]@{ file = $_.Name; sha256 = Get-PcvSha256 $_.FullName; role = if ($_.Name -like 'SSU-*') { 'servicing-stack' } else { 'cumulative' } }
        })

    Copy-Item -LiteralPath $sourcePath -Destination $basePath
    (Get-Item -LiteralPath $basePath).IsReadOnly = $false
    Mount-WindowsImage -ImagePath $basePath -Index 1 -Path $mountPath -LogPath $dismLogPath | Out-Null
    $mounted = $true
    foreach ($package in $applyPackages) {
        Add-WindowsPackage -Path $mountPath -PackagePath $package.FullName -LogPath $dismLogPath | Out-Null
    }
    $offline = Get-PcvOfflineImageBuild -MountPath $mountPath
    if ($offline.build -ne $script:BaseBuild) { throw "PCV_BASE_VHD_BUILD_MISMATCH|actual=$($offline.build)|expected=$($script:BaseBuild)" }
    if ($offline.ubr -ne $ExpectedUbr) { throw "PCV_BASE_VHD_UBR_MISMATCH|actual=$($offline.ubr)|expected=$ExpectedUbr" }
    Dismount-WindowsImage -Path $mountPath -Save -LogPath $dismLogPath | Out-Null
    $saved = $true
}
catch {
    if ($mounted -and -not $saved) {
        try { Dismount-WindowsImage -Path $mountPath -Discard -LogPath $dismLogPath | Out-Null }
        catch { Write-Warning "PCV_BASE_VHD_DISCARD_FAILED|$($_.Exception.Message)" }
    }
    Remove-Item -LiteralPath $basePath -Force -ErrorAction SilentlyContinue
    throw
}
finally {
    if ((Test-Path -LiteralPath $mountPath) -and @(Get-ChildItem -LiteralPath $mountPath -Force).Count -eq 0) {
        Remove-Item -LiteralPath $mountPath -Force
    }
    if (Test-Path -LiteralPath $packageExtractPath) {
        Remove-Item -LiteralPath $packageExtractPath -Recurse -Force
    }
}

$baseItem = Get-Item -LiteralPath $basePath
$sidecar = [ordered]@{
    schema = $script:BaseSchema
    base_file = $baseFile
    base_sha256 = Get-PcvSha256 $basePath
    base_size_bytes = $baseItem.Length
    build = $offline.build
    ubr = $offline.ubr
    edition_id = $offline.edition_id
    kb = $ExpectedKb.ToUpperInvariant()
    package_file = $packageLeaf
    package_sha256 = $packageSha256
    applied_packages = $appliedPackages
    source_file = Split-Path -Leaf $sourcePath
    source_sha256 = $sourceSha256
    component_cleanup = $false
    download_performed = $false
    created_utc = Get-PcvUtcNow
}
Write-PcvJson $sidecarPath $sidecar
$result.base_sha256 = $sidecar.base_sha256
$result.offline_build = $offline.build
$result.offline_ubr = $offline.ubr
[pscustomobject]$result
