[CmdletBinding()]
param(
    [ValidateSet(
        'Install',
        'Uninstall',
        'Start',
        'Stop',
        'Restart',
        'Status',
        'Config',
        'PrepareTokenFile',
        'PrepareProtectedTokenFile',
        'RotateProtectedTokenFile',
        'RevokeProtectedTokenFile'
    )][string]$Action = 'Status',
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [string]$DisplayName = 'PureCVisor Desktop Node',
    [string]$Description = 'PureCVisor Desktop Node Local API service.',
    [string]$PwshPath = '',
    [string]$ApiScriptPath,
    [string]$ServiceAccount = 'LocalSystem',
    [string]$Prefix = 'http://127.0.0.1:7777/',
    [string]$HelperScriptPath,
    [string]$JobStorePath,
    [string]$WebRootPath,
    [string]$ApiToken,
    [string]$ApiTokenFile,
    [string]$ApiTokenProtectedFile,
    [string]$TokenValue,
    [ValidateRange(16, 128)][int]$TokenByteLength = 32,
    [string]$AdminPrincipal = 'BUILTIN\Administrators',
    [switch]$AllowLan,
    [string]$EventLogPath,
    [switch]$EnsureFirewallRule,
    [string]$FirewallRuleName = 'PureCVisor Desktop Node API',
    [ValidateSet('private', 'domain', 'public', 'any')][string]$FirewallProfile = 'private',
    [ValidateRange(1, 64)][int]$WorkerCount = 1,
    [ValidateRange(1, 600)][int]$TimeoutSec = 30,
    [ValidateSet('auto', 'demand', 'disabled')][string]$StartupType = 'auto',
    [switch]$Force,
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ModulePath = Join-Path $PSScriptRoot 'PcvDesktopService.psm1'
Import-Module $ModulePath -Force

if ([string]::IsNullOrWhiteSpace($PwshPath)) {
    $PwshPath = Resolve-PcvDesktopServicePwshPath
}

if ([string]::IsNullOrWhiteSpace($ApiScriptPath)) {
    $desktopRoot = Split-Path -Parent $PSScriptRoot
    $ApiScriptPath = Join-Path (Join-Path $desktopRoot 'api') 'Invoke-PcvDesktopApi.ps1'
}

if ($Action -eq 'PrepareTokenFile') {
    $tokenFilePath = $ApiTokenFile
    if ([string]::IsNullOrWhiteSpace($tokenFilePath)) {
        $tokenFilePath = Get-PcvDesktopServiceDefaultTokenFilePath
    }

    if ($WhatIf) {
        [ordered]@{
            ok = $true
            action = 'preparetokenfile'
            path = $tokenFilePath
            service_account = $ServiceAccount
            service_acl_principal = (Resolve-PcvServiceAccountAclPrincipal -ServiceAccount $ServiceAccount)
            commands = (New-PcvTokenFileAclCommand `
                -Path $tokenFilePath `
                -ServiceAccount $ServiceAccount `
                -AdminPrincipal $AdminPrincipal)
        } | ConvertTo-Json -Depth 30
        exit 0
    }

    $prepareParams = @{
        Path = $tokenFilePath
        TokenByteLength = $TokenByteLength
        ServiceAccount = $ServiceAccount
        AdminPrincipal = $AdminPrincipal
        Force = $Force
    }
    if ($PSBoundParameters.ContainsKey('TokenValue')) {
        $prepareParams.Token = $TokenValue
    }

    $result = New-PcvDesktopServiceTokenFile @prepareParams
    $result | ConvertTo-Json -Depth 30
    if (-not $result.ok) {
        exit 1
    }
    exit 0
}

if ($Action -in @('PrepareProtectedTokenFile', 'RotateProtectedTokenFile', 'RevokeProtectedTokenFile')) {
    $tokenFilePath = $ApiTokenProtectedFile
    if ([string]::IsNullOrWhiteSpace($tokenFilePath)) {
        $tokenFilePath = Get-PcvDesktopServiceDefaultProtectedTokenFilePath
    }

    if ($WhatIf) {
        $whatIfResult = [ordered]@{
            ok = $true
            action = $Action.ToLowerInvariant()
            path = $tokenFilePath
            storage = 'dpapi-local-machine'
            service_account = $ServiceAccount
            service_acl_principal = (Resolve-PcvServiceAccountAclPrincipal -ServiceAccount $ServiceAccount)
        }
        if ($Action -ne 'RevokeProtectedTokenFile') {
            $whatIfResult.commands = (New-PcvTokenFileAclCommand `
                -Path $tokenFilePath `
                -ServiceAccount $ServiceAccount `
                -AdminPrincipal $AdminPrincipal)
        }
        $whatIfResult | ConvertTo-Json -Depth 30
        exit 0
    }

    if ($Action -eq 'RevokeProtectedTokenFile') {
        $removeResult = Remove-PcvDesktopServiceProtectedTokenFile -Path $tokenFilePath
        $removeResult['action'] = $Action.ToLowerInvariant()
        $removeResult | ConvertTo-Json -Depth 30
        if (-not $removeResult.ok) {
            exit 1
        }
        exit 0
    }

    $prepareParams = @{
        Path = $tokenFilePath
        TokenByteLength = $TokenByteLength
        ServiceAccount = $ServiceAccount
        AdminPrincipal = $AdminPrincipal
        Force = ($Force -or $Action -eq 'RotateProtectedTokenFile')
    }
    if ($PSBoundParameters.ContainsKey('TokenValue')) {
        $prepareParams.Token = $TokenValue
    }

    $result = New-PcvDesktopServiceProtectedTokenFile @prepareParams
    $result['action'] = $Action.ToLowerInvariant()
    $result | ConvertTo-Json -Depth 30
    if (-not $result.ok) {
        exit 1
    }
    exit 0
}

$config = New-PcvDesktopServiceConfig `
    -ServiceName $ServiceName `
    -DisplayName $DisplayName `
    -Description $Description `
    -PwshPath $PwshPath `
    -ApiScriptPath $ApiScriptPath `
    -ServiceAccount $ServiceAccount `
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
    -StartupType $StartupType

if ($Action -eq 'Config') {
    $config | ConvertTo-Json -Depth 30
    exit 0
}

$commands = New-PcvDesktopServiceCommand -Config $config -Action $Action
if ($WhatIf) {
    [ordered]@{
        ok = $true
        action = $Action.ToLowerInvariant()
        service_name = $config.service_name
        commands = $commands
    } | ConvertTo-Json -Depth 30
    exit 0
}

$result = Invoke-PcvDesktopServiceCommand -Config $config -Action $Action
$result | ConvertTo-Json -Depth 30
if (-not $result.ok) {
    exit 1
}
