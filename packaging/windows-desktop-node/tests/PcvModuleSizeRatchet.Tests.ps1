Set-StrictMode -Version Latest

Describe 'Module size ratchet' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:FixtureRelativePath = 'packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json'
        $script:Fixture = Get-Content -Raw -LiteralPath (
            Join-Path $script:RepoRoot $script:FixtureRelativePath) | ConvertFrom-Json

        function Measure-RepoFileLines {
            param([Parameter(Mandatory)] [string] $RelativePath)

            $path = Join-Path $script:RepoRoot $RelativePath
            $path | Should -Exist
            # Counts lines the way `wc -l` and a reviewer both see them, independent of
            # whether the working copy checked out CRLF or LF.
            $text = Get-Content -Raw -LiteralPath $path
            return ($text -replace "`r`n", "`n").TrimEnd("`n").Split("`n").Count
        }
    }

    It 'declares a well-formed ratchet contract' {
        $script:Fixture.contract | Should -Be 'pcv-module-size-ratchet-v1'
        $script:Fixture.slack_lines | Should -BeGreaterThan 0
        @($script:Fixture.modules).Count | Should -BeGreaterThan 0

        foreach ($module in $script:Fixture.modules) {
            $module.path | Should -Not -BeNullOrEmpty
            $module.owner | Should -Not -BeNullOrEmpty
            $module.max_lines | Should -BeGreaterThan 0
        }
    }

    It 'keeps every ratcheted module at or below its recorded ceiling' {
        foreach ($module in $script:Fixture.modules) {
            $actual = Measure-RepoFileLines -RelativePath $module.path
            $actual |
                Should -BeLessOrEqual $module.max_lines `
                -Because "$($module.path) grew to $actual lines against a ceiling of $($module.max_lines). Split the module or justify a new ceiling in $($script:FixtureRelativePath)."
        }
    }

    It 'requires the ceiling to be tightened once a module actually shrinks' {
        # Without this the ceiling drifts upward in meaning: a module could be split in half
        # and the ratchet would still permit it to grow all the way back.
        foreach ($module in $script:Fixture.modules) {
            $actual = Measure-RepoFileLines -RelativePath $module.path
            ($module.max_lines - $actual) |
                Should -BeLessOrEqual $script:Fixture.slack_lines `
                -Because "$($module.path) is now $actual lines but its ceiling is still $($module.max_lines). Lower max_lines in $($script:FixtureRelativePath) so the ratchet keeps holding."
        }
    }
}
