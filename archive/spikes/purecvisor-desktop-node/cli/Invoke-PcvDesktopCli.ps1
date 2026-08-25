[CmdletBinding()]
param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Arguments
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ModulePath = Join-Path $PSScriptRoot 'PcvDesktopCli.psm1'
Import-Module $ModulePath -Force

$result = Invoke-PcvDesktopCli -Arguments $Arguments

if (-not [string]::IsNullOrWhiteSpace($result.stdout)) {
    Write-Output $result.stdout
}

if (-not [string]::IsNullOrWhiteSpace($result.stderr)) {
    [Console]::Error.WriteLine($result.stderr)
}

exit ([int]$result.exit_code)
