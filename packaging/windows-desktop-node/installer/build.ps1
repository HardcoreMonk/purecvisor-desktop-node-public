[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Version,

    [string]$MsiProductVersion,

    [string]$DesktopNodeHostPath,

    [string]$DesktopNodeCliPath,

    [Parameter(Mandatory)]
    [string]$OutputRoot,

    [ValidateSet('RequireSigned', 'AllowUnsignedDev')]
    [string]$SigningMode = 'RequireSigned',

    [ValidateSet('Unspecified', 'LocalTest', 'InternalEnterprise', 'PublicTrusted')]
    [string]$SigningTrustModel = 'Unspecified',

    [string]$SignToolPath,
    [string]$CertificateThumbprint,
    [string]$CertificatePath,
    [string]$TimestampUrl,
    [string]$WixPath = 'wix',
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'PcvDesktopNodeInstaller.Build.psm1') -Force

$buildInput = @{
    Version = $Version
    MsiProductVersion = $MsiProductVersion
    DesktopNodeHostPath = $DesktopNodeHostPath
    DesktopNodeCliPath = $DesktopNodeCliPath
    OutputRoot = $OutputRoot
    SigningMode = $SigningMode
    SigningTrustModel = $SigningTrustModel
    SignToolPath = $SignToolPath
    CertificateThumbprint = $CertificateThumbprint
    CertificatePath = $CertificatePath
    TimestampUrl = $TimestampUrl
    WixPath = $WixPath
    DryRun = [bool]$DryRun
}

$payload = Invoke-PcvDesktopNodeInstallerBuild -Input $buildInput
$payload | ConvertTo-Json -Depth 12 -Compress

if ([bool]$payload.ok) {
    exit 0
}

$exitCode = [int]$payload.exit_code
if ($exitCode -eq 0) {
    $exitCode = 1
}
exit $exitCode
