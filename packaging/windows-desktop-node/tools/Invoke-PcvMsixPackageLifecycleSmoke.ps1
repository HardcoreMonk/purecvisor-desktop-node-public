[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactRoot,
    [Parameter(Mandatory)][string]$TemplateLayoutRoot,
    [Parameter(Mandatory)][string]$BaselinePayloadRoot,
    [Parameter(Mandatory)][string]$TargetPayloadRoot,
    [Parameter(Mandatory)][string]$BaselineVersion,
    [Parameter(Mandatory)][string]$TargetVersion,
    [Parameter(Mandatory)][string]$MakeAppxPath,
    [Parameter(Mandatory)][string]$SignToolPath,
    [Parameter(Mandatory)][string]$SigningCertificateThumbprint,
    [string]$InstalledManifestPath = 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json',
    [string]$MsiServiceName = 'PureCVisorDesktopNode',
    [string]$SmokeServiceName = 'PureCVisorDesktopNodeMsixSmoke',
    [string]$PackageName = 'PureCVisor.DesktopNode.MsixSmoke',
    [switch]$PlanOnly,
    [switch]$Execute
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($PlanOnly -eq $Execute) { throw 'PCV_MSIX_LIFECYCLE_MODE_REQUIRED|Pass exactly one mode.' }

function Resolve-PcvFullPath([string]$Path) {
    if ([IO.Path]::IsPathRooted($Path)) { return [IO.Path]::GetFullPath($Path) }
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}
function Assert-PcvFile([string]$Path, [string]$Name) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "PCV_MSIX_LIFECYCLE_INPUT_MISSING|$Name|$Path" }
}
function Assert-PcvDirectory([string]$Path, [string]$Name) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { throw "PCV_MSIX_LIFECYCLE_INPUT_MISSING|$Name|$Path" }
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
function ConvertTo-PcvPackageVersion([string]$Version) {
    ($Version -replace '-admin-smoke$', '') + '.0'
}
function ConvertTo-PcvComparableVersion([string]$Version) {
    try { [version]($Version -replace '-admin-smoke$', '') }
    catch { throw "PCV_MSIX_LIFECYCLE_VERSION_ORDER_INVALID|$Version" }
}
function Assert-PcvManifestContract([string]$Text, [string]$ExpectedVersion, [string]$CertificateSubject) {
    try { [xml]$xml = $Text } catch { throw 'PCV_MSIX_LIFECYCLE_TEMPLATE_CONTRACT_INVALID|xml' }
    $identity = $xml.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']")
    $deviceFamily = $xml.SelectSingleNode("//*[local-name()='TargetDeviceFamily']")
    $applications = @($xml.SelectNodes("//*[local-name()='Application']"))
    $extensions = @($xml.SelectNodes("//*[local-name()='Extension' and @Category='windows.service']"))
    $services = @($xml.SelectNodes("//*[local-name()='Service']"))
    $application = if ($applications.Count -eq 1) { $applications[0] } else { $null }
    $extension = if ($extensions.Count -eq 1) { $extensions[0] } else { $null }
    $service = if ($services.Count -eq 1) { $services[0] } else { $null }
    $capabilities = @($xml.SelectNodes("//*[local-name()='Capability']") | ForEach-Object { [string]$_.Name })
    $expectedPublisher = 'CN=PureCVisor Desktop Node Internal Code Signing'
    $expectedArguments = 'listen --prefix http://127.0.0.1:7789/ --product-root . --data-root %ProgramData%\PureCVisor\desktop-node-msix-smoke --token-required false --firewall-enabled false'
    $requiredCapabilities = @('runFullTrust', 'packagedServices', 'localSystemServices')
    $missingCapabilities = @($requiredCapabilities | Where-Object { $_ -notin $capabilities })
    $unexpectedCapabilities = @($capabilities | Where-Object { $_ -notin $requiredCapabilities })

    if ($PackageName -ne 'PureCVisor.DesktopNode.MsixSmoke' -or
        $null -eq $identity -or [string]$identity.Name -ne 'PureCVisor.DesktopNode.MsixSmoke' -or
        [string]$identity.Publisher -ne $expectedPublisher -or [string]$identity.Publisher -ne $CertificateSubject -or
        [string]$identity.ProcessorArchitecture -ne 'x64' -or [string]$identity.Version -ne $ExpectedVersion -or
        $null -eq $deviceFamily -or [string]$deviceFamily.Name -ne 'Windows.Desktop' -or [string]$deviceFamily.MinVersion -ne '10.0.19041.0' -or
        $applications.Count -ne 1 -or $extensions.Count -ne 1 -or $services.Count -ne 1 -or
        $null -eq $application -or [string]$application.Id -ne 'DesktopNode' -or [string]$application.Executable -ne 'DesktopNode.Host.exe' -or
        [string]$application.EntryPoint -ne 'Windows.FullTrustApplication' -or [string]$application.RuntimeBehavior -ne 'packagedClassicApp' -or
        [string]$application.TrustLevel -ne 'mediumIL' -or $null -eq $extension -or
        [string]$extension.Executable -ne 'DesktopNode.Host.exe' -or [string]$extension.EntryPoint -ne 'Windows.FullTrustApplication' -or
        $null -eq $service -or [string]$service.Name -ne $SmokeServiceName -or [string]$service.StartupType -ne 'manual' -or
        [string]$service.StartAccount -ne 'localSystem' -or [string]$service.Arguments -ne $expectedArguments -or
        $capabilities.Count -ne $requiredCapabilities.Count -or $missingCapabilities.Count -ne 0 -or $unexpectedCapabilities.Count -ne 0) {
        throw 'PCV_MSIX_LIFECYCLE_TEMPLATE_CONTRACT_INVALID'
    }
}
function Assert-PcvInstalledSmoke([string]$ExpectedVersion) {
    $installed = Get-AppxPackage -Name $PackageName -ErrorAction Stop
    if ([string]$installed.Version -ne $ExpectedVersion) { throw "PCV_MSIX_LIFECYCLE_PACKAGE_VERSION_INVALID|expected=$ExpectedVersion" }
    $smoke = Get-CimInstance Win32_Service -Filter "Name='$SmokeServiceName'" -ErrorAction Stop
    if ([string]$smoke.StartMode -ne 'Manual' -or [string]$smoke.StartName -ne 'LocalSystem') {
        throw 'PCV_MSIX_LIFECYCLE_SMOKE_SERVICE_INVALID|StartType Manual Account LocalSystem'
    }
}

$baselineComparable = ConvertTo-PcvComparableVersion $BaselineVersion
$targetComparable = ConvertTo-PcvComparableVersion $TargetVersion
if ($targetComparable -le $baselineComparable) { throw 'PCV_MSIX_LIFECYCLE_VERSION_ORDER_INVALID|target must be greater than baseline' }

$root = Resolve-PcvFullPath $ArtifactRoot
$layout = Resolve-PcvFullPath $TemplateLayoutRoot
$baselinePayload = Resolve-PcvFullPath $BaselinePayloadRoot
$targetPayload = Resolve-PcvFullPath $TargetPayloadRoot
$makeAppx = Resolve-PcvFullPath $MakeAppxPath
$signTool = Resolve-PcvFullPath $SignToolPath
Assert-PcvDirectory $layout 'template-layout'
Assert-PcvDirectory $baselinePayload 'baseline-payload'
Assert-PcvDirectory $targetPayload 'target-payload'
Assert-PcvFile (Join-Path $layout 'AppxManifest.xml.template') 'manifest-template'
Assert-PcvFile $makeAppx 'makeappx'
Assert-PcvFile $signTool 'signtool'
$thumbprint = ($SigningCertificateThumbprint -replace '\s', '').ToUpperInvariant()
if ($thumbprint -notmatch '^[A-F0-9]{40,64}$') { throw 'PCV_MSIX_LIFECYCLE_THUMBPRINT_INVALID' }

$actions = @(
    [ordered]@{ id = 'build-baseline'; command = "'$makeAppx' pack /d '<baseline-layout>' /p '<baseline.msix>' /o" },
    [ordered]@{ id = 'build-target'; command = "'$makeAppx' pack /d '<target-layout>' /p '<target.msix>' /o" },
    [ordered]@{ id = 'validate-publisher'; command = 'Identity Publisher equals certificate Subject; ProcessorArchitecture x64; service StartType Manual Account LocalSystem; runFullTrust packagedServices localSystemServices' },
    [ordered]@{ id = 'sign-verify-baseline'; command = "'$signTool' sign <store-flag> /sha1 <redacted> '<baseline.msix>'; verify /pa" },
    [ordered]@{ id = 'sign-verify-target'; command = "'$signTool' sign <store-flag> /sha1 <redacted> '<target.msix>'; verify /pa" },
    [ordered]@{ id = 'install-baseline'; command = "Add-AppxPackage '<baseline.msix>' -ForceApplicationShutdown" },
    [ordered]@{ id = 'update-target'; command = "Add-AppxPackage '<target.msix>' -ForceUpdateFromAnyVersion -ForceApplicationShutdown" },
    [ordered]@{ id = 'remove-package'; command = "Remove-AppxPackage '<package-full-name>'" },
    [ordered]@{ id = 'verify-final-state'; command = 'package/service absent; MSI Status Running StartType Automatic; target_manifest_unchanged' }
)
$plan = [ordered]@{
    schema_version = 1
    contract = 'pcv-msix-package-lifecycle-v1'
    mode = if ($PlanOnly) { 'PlanOnly' } else { 'Execute' }
    artifact_root = $root
    host_mutation_performed = $false
    actions = $actions
    certificate_generation = 'forbidden'
}
New-Item -ItemType Directory -Force -Path $root | Out-Null
Write-PcvJson (Join-Path $root 'plan.json') $plan
if ($PlanOnly) {
    $planned = [ordered]@{ ok = $true; status = 'PLANNED'; host_mutation_performed = $false }
    Write-PcvJson (Join-Path $root 'summary.json') $planned
    return [pscustomobject]$planned
}

$certificate = Get-ChildItem Cert:\CurrentUser\My, Cert:\LocalMachine\My -ErrorAction SilentlyContinue |
    Where-Object { ($_.Thumbprint -replace '\s', '').ToUpperInvariant() -eq $thumbprint -and $_.HasPrivateKey } |
    Select-Object -First 1
$codeSigningEku = @($certificate.EnhancedKeyUsageList | Where-Object { [string]$_.ObjectId -eq '1.3.6.1.5.5.7.3.3' })
if ($null -eq $certificate -or $certificate.NotBefore -gt (Get-Date) -or $certificate.NotAfter -le (Get-Date) -or $codeSigningEku.Count -eq 0) {
    throw 'PCV_MSIX_LIFECYCLE_CERTIFICATE_INVALID'
}
if (@(Get-AppxPackage -Name $PackageName -ErrorAction SilentlyContinue).Count -ne 0 -or
    $null -ne (Get-Service -Name $SmokeServiceName -ErrorAction SilentlyContinue)) {
    throw 'PCV_MSIX_LIFECYCLE_PREEXISTING_SMOKE_IDENTITY|preexisting-smoke-identity'
}
$beforeManifest = Get-Content -Raw -LiteralPath $InstalledManifestPath
$beforeMsi = Get-CimInstance Win32_Service -Filter "Name='$MsiServiceName'" -ErrorAction Stop
if ([string]$beforeMsi.State -ne 'Running' -or [string]$beforeMsi.StartMode -ne 'Auto') { throw 'PCV_MSIX_LIFECYCLE_MSI_STATE_INVALID' }

$storeFlag = if ([string]$certificate.PSParentPath -match 'LocalMachine') { '/sm' } else { $null }
$summary = [ordered]@{
    ok = $false
    status = 'FAIL'
    host_mutation_performed = $false
    exits = [ordered]@{}
    package_install_attempted = $false
    package_installed = $false
    cleanup_attempted = $false
    cleanup_status = 'NOT-RUN'
    cleanup_error = $null
    cleanup_probe_status = 'NOT-RUN'
    cleanup_probe_error = $null
    final_absence_probe_status = 'NOT-RUN'
    final_absence_probe_error = $null
}
$failure = $null
try {
    $packages = @()
    foreach ($item in @(
        @{ label = 'baseline'; payload = $baselinePayload; version = $BaselineVersion },
        @{ label = 'target'; payload = $targetPayload; version = $TargetVersion }
    )) {
        $layoutRoot = Join-Path $root "$($item.label)-layout"
        Copy-Item -LiteralPath $layout -Destination $layoutRoot -Recurse -Force
        foreach ($payloadEntry in [IO.Directory]::GetFileSystemEntries($item.payload)) {
            Copy-Item -LiteralPath $payloadEntry -Destination $layoutRoot -Recurse -Force
        }
        $manifestPath = Join-Path $layoutRoot 'AppxManifest.xml'
        Copy-Item -LiteralPath (Join-Path $layoutRoot 'AppxManifest.xml.template') -Destination $manifestPath -Force
        $manifestText = Get-Content -Raw -LiteralPath $manifestPath
        $manifestText = $manifestText.Replace('{{Publisher}}', [string]$certificate.Subject).
            Replace('{{Version}}', (ConvertTo-PcvPackageVersion $item.version)).
            Replace('{{PackageName}}', $PackageName).
            Replace('{{ServiceName}}', $SmokeServiceName)
        Assert-PcvManifestContract $manifestText (ConvertTo-PcvPackageVersion $item.version) ([string]$certificate.Subject)
        [IO.File]::WriteAllText($manifestPath, $manifestText, [Text.UTF8Encoding]::new($false))
        $packagePath = Join-Path $root "$($item.label).msix"
        $packages += $packagePath
    }

    for ($index = 0; $index -lt 2; $index++) {
        $label = @('baseline', 'target')[$index]
        $layoutRoot = Join-Path $root "$label-layout"
        $packagePath = $packages[$index]
        Invoke-PcvProcess "pack-$label" $makeAppx @('pack', '/d', $layoutRoot, '/p', $packagePath, '/o')
        $signArguments = @('sign')
        if ($storeFlag) { $signArguments += $storeFlag }
        $signArguments += @('/fd', 'SHA256', '/sha1', $thumbprint, $packagePath)
        Invoke-PcvProcess "sign-$label" $signTool $signArguments
        Invoke-PcvProcess "verify-$label" $signTool @('verify', '/pa', '/v', $packagePath)
    }

    $summary.package_install_attempted = $true
    $summary.host_mutation_performed = $true
    Add-AppxPackage -Path $packages[0] -ForceApplicationShutdown -ErrorAction Stop
    $summary.package_installed = $true
    Assert-PcvInstalledSmoke (ConvertTo-PcvPackageVersion $BaselineVersion)
    Add-AppxPackage -Path $packages[1] -ForceUpdateFromAnyVersion -ForceApplicationShutdown -ErrorAction Stop
    Assert-PcvInstalledSmoke (ConvertTo-PcvPackageVersion $TargetVersion)
} catch {
    $failure = $_
} finally {
    if ($summary.package_install_attempted) {
        $summary.cleanup_attempted = $true
        try {
            $packagesToRemove = @(Get-AppxPackage -Name $PackageName -ErrorAction Stop)
            $summary.cleanup_probe_status = 'PASS'
            foreach ($package in $packagesToRemove) {
                Remove-AppxPackage -Package $package.PackageFullName -ErrorAction Stop
            }
            $summary.cleanup_status = 'PASS'
        } catch {
            $summary.cleanup_status = 'FAIL'
            $summary.cleanup_error = $_.Exception.Message
            if ($summary.cleanup_probe_status -ne 'PASS') {
                $summary.cleanup_probe_status = 'FAIL'
                $summary.cleanup_probe_error = $_.Exception.Message
            }
            if ($null -eq $failure) { $failure = $_ }
        }
    }

    try {
        $remaining = @(Get-AppxPackage -Name $PackageName -ErrorAction Stop)
        $summary.final_absence_probe_status = 'PASS'
        $summary.final_package_absent = $remaining.Count -eq 0
    } catch {
        $summary.final_absence_probe_status = 'FAIL'
        $summary.final_absence_probe_error = $_.Exception.Message
        $summary.final_package_absent = $false
        if ($null -eq $failure) { $failure = $_ }
    }
    $smokeService = Get-Service -Name $SmokeServiceName -ErrorAction SilentlyContinue
    $msiService = Get-CimInstance Win32_Service -Filter "Name='$MsiServiceName'" -ErrorAction SilentlyContinue
    $afterManifest = if (Test-Path -LiteralPath $InstalledManifestPath -PathType Leaf) { Get-Content -Raw -LiteralPath $InstalledManifestPath } else { '' }
    $summary.final_smoke_service_absent = $null -eq $smokeService
    $summary.final_msi_service_status = if ($msiService) { [string]$msiService.State } else { $null }
    $summary.final_msi_service_start_type = if ($null -eq $msiService) {
        $null
    } elseif ([string]$msiService.StartMode -eq 'Auto') {
        'Automatic'
    } else {
        [string]$msiService.StartMode
    }
    $summary.target_manifest_unchanged = $beforeManifest -eq $afterManifest
    if (-not $summary.final_package_absent -or -not $summary.final_smoke_service_absent -or
        $summary.final_msi_service_status -ne 'Running' -or $summary.final_msi_service_start_type -ne 'Automatic' -or
        -not $summary.target_manifest_unchanged) {
        if ($null -eq $failure) { $failure = [Exception]'final state invalid' }
    }
    if ($null -eq $failure) {
        $summary.ok = $true
        $summary.status = 'PASS'
    } else {
        $failureException = if ($failure -is [System.Management.Automation.ErrorRecord]) {
            $failure.Exception
        }
        else {
            $failure
        }
        $summary.error = $failureException.Message
    }
    Write-PcvJson (Join-Path $root 'summary.json') $summary
}

[pscustomobject]$summary
if ($null -ne $failure) { throw $failure }
