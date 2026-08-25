[CmdletBinding()]
param(
    [string]$InputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ModulePath = Join-Path $PSScriptRoot 'PcvHyperV.psm1'
Import-Module $ModulePath -Force

function Read-PcvInputJson {
    param([string]$Path)

    if ($Path) {
        if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
            return $null
        }
        return Get-Content -LiteralPath $Path -Raw
    }

    if ([Console]::IsInputRedirected) {
        return [Console]::In.ReadToEnd()
    }

    return $null
}

$raw = Read-PcvInputJson -Path $InputPath
if ([string]::IsNullOrWhiteSpace($raw)) {
    New-PcvResponse `
        -Ok $false `
        -Operation 'unknown' `
        -Data $null `
        -ErrorObject (New-PcvError `
            -Code 'PCV_INPUT_MISSING' `
            -Message 'No JSON request was provided.' `
            -Detail 'Pass -InputPath or pipe a JSON request to stdin.' `
            -Retryable $false) |
        ConvertTo-PcvJson
    exit 2
}

try {
    $request = $raw | ConvertFrom-Json -Depth 20
    if (-not $request.PSObject.Properties.Name.Contains('operation')) {
        New-PcvResponse `
            -Ok $false `
            -Operation 'unknown' `
            -Data $null `
            -ErrorObject (New-PcvError `
                -Code 'PCV_OPERATION_MISSING' `
                -Message 'Request JSON does not contain an operation field.' `
                -Detail 'The request must include an operation string and a params object.' `
                -Retryable $false) |
            ConvertTo-PcvJson
        exit 2
    }

    Invoke-PcvOperation -Request $request | ConvertTo-PcvJson
    exit 0
}
catch {
    New-PcvResponse `
        -Ok $false `
        -Operation 'unknown' `
        -Data $null `
        -ErrorObject (New-PcvError `
            -Code 'PCV_RUNNER_EXCEPTION' `
            -Message 'The Hyper-V helper runner failed before completing the operation.' `
            -Detail $_.Exception.Message `
            -Retryable $false) |
        ConvertTo-PcvJson
    exit 1
}
