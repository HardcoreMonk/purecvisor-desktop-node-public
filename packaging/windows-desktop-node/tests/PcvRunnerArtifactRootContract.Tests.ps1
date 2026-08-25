Set-StrictMode -Version Latest

Describe 'Runner -ArtifactRoot path contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ToolsRoot = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools'

        # An unguarded Join-Path (Get-Location) $ArtifactRoot turns an absolute -ArtifactRoot into
        # D:\repo\D:\repo\..., which is how the 2026-08-06 clean-host run lost its first attempt.
        # The repaired form keeps that same expression as the relative else-branch, so the defect is
        # the missing IsPathRooted guard rather than the Join-Path text on its own.
        $script:RelativeJoinPattern = 'Join-Path\s*\(\s*Get-Location\s*\)\s*\$ArtifactRoot'
        $script:RootedGuardPattern = 'IsPathRooted\(\s*\$ArtifactRoot\s*\)'

        function Get-PcvArtifactRootRunner {
            Get-ChildItem -LiteralPath $script:ToolsRoot -File -Include '*.ps1', '*.psm1' -Recurse |
                Where-Object {
                    (Get-Content -Raw -LiteralPath $_.FullName) -match '(?m)^\s*\[string\]\$ArtifactRoot'
                }
        }

        function Test-PcvArtifactRootResolution {
            param(
                [Parameter(Mandatory)][AllowEmptyString()][string]$Text
            )

            if ($Text -notmatch $script:RelativeJoinPattern) { return 'no-relative-join' }
            if ($Text -match $script:RootedGuardPattern) { return 'guarded' }
            'relative-only'
        }
    }

    It 'finds the runners that take an -ArtifactRoot parameter' {
        @(Get-PcvArtifactRootRunner).Count | Should -BeGreaterThan 0
    }

    It 'guards every relative artifact-root join with a rooted-path branch' {
        $offenders = @()

        foreach ($runner in Get-PcvArtifactRootRunner) {
            $verdict = Test-PcvArtifactRootResolution -Text (Get-Content -Raw -LiteralPath $runner.FullName)
            if ($verdict -eq 'relative-only') {
                $offenders += $runner.Name
            }
        }

        # 37e07a78 repaired only Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1 while seven
        # sibling runners kept the same defect, so this guard covers the directory, not one file.
        ($offenders -join ', ') | Should -BeNullOrEmpty
    }

    It 'separates a relative-only runner from a guarded one instead of passing vacuously' {
        $relativeOnly = 'param([string]$ArtifactRoot)
$artifactRootFull = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))'
        $guarded = 'param([string]$ArtifactRoot)
$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) { $ArtifactRoot }
    else { Join-Path (Get-Location) $ArtifactRoot }'
        $noJoin = 'param([string]$ArtifactRoot)
$artifactRootFull = [System.IO.Path]::GetFullPath($ArtifactRoot)'

        Test-PcvArtifactRootResolution -Text $relativeOnly | Should -Be 'relative-only'
        Test-PcvArtifactRootResolution -Text $guarded | Should -Be 'guarded'
        Test-PcvArtifactRootResolution -Text $noJoin | Should -Be 'no-relative-join'
    }
}
