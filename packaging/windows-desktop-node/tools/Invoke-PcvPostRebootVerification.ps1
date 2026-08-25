param(
    [Parameter(Mandatory)][string]$StateFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvPostRebootVerification.psm1'
Import-Module $modulePath -Force

if (-not (Test-Path -LiteralPath $StateFile -PathType Leaf)) {
    throw "PCV_POST_REBOOT_STATE_NOT_FOUND|Post-reboot state file was not found.|Path: '$StateFile'."
}

$result = Invoke-PcvPostRebootVerification -StateFile $StateFile
$result | ConvertTo-Json -Depth 32

if (-not $result.ok) {
    exit 1
}
