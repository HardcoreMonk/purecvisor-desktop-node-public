Set-StrictMode -Version Latest

Describe 'StrictMode child-item array helper' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging\windows-desktop-node\tools\PcvStrictCollection.psm1'
        Import-Module -Force -Name $script:ModulePath
    }

    It 'reproduces the if-assignment unwrap that broke clean-host residue Count readback' {
        $root = Join-Path ([System.IO.Path]::GetTempPath()) ('pcv-strict-count-' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $root | Out-Null
        try {
            New-Item -ItemType Directory -Path (Join-Path $root 'only') | Out-Null
            $entries = if ($true) {
                @(Get-ChildItem -LiteralPath $root -Force)
            }
            $entries.GetType().FullName | Should -Be 'System.IO.DirectoryInfo'
        }
        finally {
            Remove-Item -LiteralPath $root -Recurse -Force
        }
    }

    It 'returns Count 0/1/2 under StrictMode for missing, one-child, and two-child directories' {
        $missing = Join-Path ([System.IO.Path]::GetTempPath()) ('pcv-strict-missing-' + [guid]::NewGuid().ToString('N'))
        (Get-PcvChildItemArray -LiteralPath $missing).Count | Should -Be 0

        $root = Join-Path ([System.IO.Path]::GetTempPath()) ('pcv-strict-ok-' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $root | Out-Null
        try {
            (Get-PcvChildItemArray -LiteralPath $root).Count | Should -Be 0
            New-Item -ItemType Directory -Path (Join-Path $root 'a') | Out-Null
            (Get-PcvChildItemArray -LiteralPath $root).Count | Should -Be 1
            New-Item -ItemType Directory -Path (Join-Path $root 'b') | Out-Null
            (Get-PcvChildItemArray -LiteralPath $root).Count | Should -Be 2
        }
        finally {
            Remove-Item -LiteralPath $root -Recurse -Force
        }
    }
}
