Set-StrictMode -Version Latest

function Get-PcvChildItemArray {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$LiteralPath
    )

    if (-not (Test-Path -LiteralPath $LiteralPath -PathType Container)) {
        return ,[object[]]@()
    }

    $items = @(Get-ChildItem -LiteralPath $LiteralPath -Force -ErrorAction Stop)
    return ,$items
}

Export-ModuleMember -Function Get-PcvChildItemArray
