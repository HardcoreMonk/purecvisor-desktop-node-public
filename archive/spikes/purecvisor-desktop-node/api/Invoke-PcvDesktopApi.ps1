[CmdletBinding()]
param(
    [string]$Prefix = 'http://127.0.0.1:7777/',
    [string]$HelperScriptPath,
    [string]$JobStorePath,
    [string]$WebRootPath,
    [string]$ApiToken,
    [string]$ApiTokenFile,
    [string]$ApiTokenProtectedFile,
    [switch]$AllowLan,
    [string]$EventLogPath,
    [switch]$EnsureFirewallRule,
    [string]$FirewallRuleName = 'PureCVisor Desktop Node API',
    [ValidateSet('private', 'domain', 'public', 'any')][string]$FirewallProfile = 'private',
    [ValidateRange(1, 64)][int]$WorkerCount = 1,
    [ValidateRange(1, 600)][int]$TimeoutSec = 30,
    [switch]$Once
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ModulePath = Join-Path $PSScriptRoot 'PcvDesktopApi.psm1'
Import-Module $ModulePath -Force

if ([string]::IsNullOrWhiteSpace($HelperScriptPath)) {
    $HelperScriptPath = Get-PcvDefaultHyperVHelperPath
}

Start-PcvDesktopApi `
    -Prefix $Prefix `
    -HelperScriptPath $HelperScriptPath `
    -JobStorePath $JobStorePath `
    -WebRootPath $WebRootPath `
    -ApiToken $ApiToken `
    -ApiTokenFile $ApiTokenFile `
    -ApiTokenProtectedFile $ApiTokenProtectedFile `
    -AllowLan:$AllowLan `
    -EventLogPath $EventLogPath `
    -EnsureFirewallRule:$EnsureFirewallRule `
    -FirewallRuleName $FirewallRuleName `
    -FirewallProfile $FirewallProfile `
    -WorkerCount $WorkerCount `
    -TimeoutSec $TimeoutSec `
    -Once:$Once
