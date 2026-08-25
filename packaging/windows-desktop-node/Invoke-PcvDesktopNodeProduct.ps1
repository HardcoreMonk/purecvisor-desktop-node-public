param(
    [ValidateSet(
        'Plan',
        'Install',
        'Update',
        'Rollback',
        'Uninstall',
        'Status',
        'CollectDiagnostics',
        'ConfigureInstalled',
        'RepairInstalled',
        'RemoveInstalled'
    )]
    [string]$Action = 'Plan',
    [string]$SourceRoot = '',
    [string]$ProductRoot = '',
    [string]$DataRoot = '',
    [string]$ServiceName = '',
    [string]$DisplayName = '',
    [string]$Prefix = '',
    [string]$WinSwPath = '',
    [string]$ServiceAccount = '',
    [string]$Version = '',
    [string]$SourceUri = '',
    [string]$ExpectedSha256 = '',
    [string]$DownloadRoot = '',
    [string]$UpdateCatalogUri = '',
    [string]$UpdateChannel = '',
    [string]$BatchEvidenceRoot = '',
    [int]$WorkerCount = 0,
    [int]$TimeoutSec = 0,
    [switch]$RemoveData,
    [switch]$DryRun,
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-PcvCommandJson {
    param([Parameter(Mandatory)]$InputObject)

    $InputObject | ConvertTo-PcvProductJson | Write-Output
}

try {
    $modulePath = Join-Path $PSScriptRoot 'PcvDesktopNodeProduct.psm1'
    Import-Module $modulePath -Force

    if ([string]::IsNullOrWhiteSpace($SourceRoot)) {
        $installedServiceModule = Join-Path $PSScriptRoot 'service\PcvDesktopService.psm1'
        $installedHostExe = Join-Path $PSScriptRoot 'DesktopNode.Host.exe'
        if ((Test-Path -LiteralPath $installedHostExe -PathType Leaf) -or
            (Test-Path -LiteralPath $installedServiceModule -PathType Leaf)) {
            $SourceRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path
        } else {
            $SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
        }
    }

    $planArgs = @{
        Action = $Action
        SourceRoot = $SourceRoot
        RemoveData = $RemoveData
    }

    if (-not [string]::IsNullOrWhiteSpace($ProductRoot)) {
        $planArgs.ProductRoot = $ProductRoot
    }
    if (-not [string]::IsNullOrWhiteSpace($DataRoot)) {
        $planArgs.DataRoot = $DataRoot
    }
    if (-not [string]::IsNullOrWhiteSpace($ServiceName)) {
        $planArgs.ServiceName = $ServiceName
    }
    if (-not [string]::IsNullOrWhiteSpace($DisplayName)) {
        $planArgs.DisplayName = $DisplayName
    }
    if (-not [string]::IsNullOrWhiteSpace($Prefix)) {
        $planArgs.Prefix = $Prefix
    }
    if (-not [string]::IsNullOrWhiteSpace($WinSwPath)) {
        $planArgs.WinSwPath = $WinSwPath
    }
    if (-not [string]::IsNullOrWhiteSpace($ServiceAccount)) {
        $planArgs.ServiceAccount = $ServiceAccount
    }
    if (-not [string]::IsNullOrWhiteSpace($Version)) {
        $planArgs.Version = $Version
    }
    if (-not [string]::IsNullOrWhiteSpace($SourceUri)) {
        $planArgs.SourceUri = $SourceUri
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedSha256)) {
        $planArgs.ExpectedSha256 = $ExpectedSha256
    }
    if (-not [string]::IsNullOrWhiteSpace($DownloadRoot)) {
        $planArgs.DownloadRoot = $DownloadRoot
    }
    if (-not [string]::IsNullOrWhiteSpace($UpdateCatalogUri)) {
        $planArgs.UpdateCatalogUri = $UpdateCatalogUri
    }
    if (-not [string]::IsNullOrWhiteSpace($UpdateChannel)) {
        $planArgs.UpdateChannel = $UpdateChannel
    }
    if (-not [string]::IsNullOrWhiteSpace($BatchEvidenceRoot)) {
        $planArgs.BatchEvidenceRoot = $BatchEvidenceRoot
    }
    if ($WorkerCount -gt 0) {
        $planArgs.WorkerCount = $WorkerCount
    }
    if ($TimeoutSec -gt 0) {
        $planArgs.TimeoutSec = $TimeoutSec
    }

    $plan = New-PcvDesktopNodeProductPlan @planArgs

    if ($Action -eq 'Plan') {
        Write-PcvCommandJson -InputObject $plan
        exit 0
    }

    if ($DryRun -or $WhatIf) {
        Write-PcvCommandJson -InputObject (New-PcvDesktopNodeDryRunResult -Plan $plan)
        exit 0
    }

    $result = Invoke-PcvDesktopNodeProductAction -Plan $plan
    Write-PcvCommandJson -InputObject $result
    if ($result.ok) {
        exit 0
    }
    exit 1
} catch {
    $message = $_.Exception.Message
    $errorResult = [ordered]@{
        ok = $false
        error = [ordered]@{
            code = 'PCV_PRODUCT_COMMAND_FAILED'
            message = 'Desktop Node product command failed.'
            detail = $message
        }
    }

    if (Get-Command ConvertTo-PcvProductJson -ErrorAction SilentlyContinue) {
        Write-PcvCommandJson -InputObject $errorResult
    } else {
        $errorResult | ConvertTo-Json -Depth 32 | Write-Output
    }
    exit 1
}
