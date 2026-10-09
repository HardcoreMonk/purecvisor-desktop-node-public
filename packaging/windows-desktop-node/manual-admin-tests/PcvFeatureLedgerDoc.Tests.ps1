Set-StrictMode -Version Latest

Describe 'PcvFeatureLedgerDoc generator' {
    BeforeAll {
        $script:Tool = Join-Path $PSScriptRoot '../tools/Update-PcvFeatureLedgerDoc.ps1'
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:LedgerRelative = 'config/desktop-node-feature-surface-ledger.json'
        $script:DocRelative = 'docs/FEATURE_IMPLEMENTATION_LEDGER.md'

        function New-LedgerFixture([string]$Root) {
            New-Item -ItemType Directory -Force -Path (Join-Path $Root 'config'), (Join-Path $Root 'docs') | Out-Null
            Copy-Item -LiteralPath (Join-Path $script:RepoRoot $script:LedgerRelative) -Destination (Join-Path $Root $script:LedgerRelative)
            Copy-Item -LiteralPath (Join-Path $script:RepoRoot $script:DocRelative) -Destination (Join-Path $Root $script:DocRelative)
            Join-Path $Root $script:DocRelative
        }

        function Set-StaleRow([string]$DocPath) {
            $text = Get-Content -Raw -LiteralPath $DocPath
            $stale = $text.Replace('| Runtime policy | 1 |', '| Runtime policy | 99 |')
            if ($stale -ceq $text) { throw 'fixture did not find the runtime policy row' }
            [System.IO.File]::WriteAllText($DocPath, $stale, [System.Text.UTF8Encoding]::new($false))
        }
    }

    It 'reports the repository table as current without writing' {
        $before = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot $script:DocRelative)
        $result = & $script:Tool -RepoRoot $script:RepoRoot -Check | ConvertFrom-Json
        $result.ok | Should -BeTrue
        $result.contract | Should -Be 'pcv-feature-ledger-doc-v1'
        $result.status | Should -Be 'current'
        (Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot $script:DocRelative)) | Should -Be $before
    }

    It 'fails Check when the table is stale and writes nothing' {
        $doc = New-LedgerFixture (Join-Path $TestDrive 'stale')
        Set-StaleRow $doc
        $before = Get-Content -Raw -LiteralPath $doc
        $result = & $script:Tool -RepoRoot (Join-Path $TestDrive 'stale') -Check | ConvertFrom-Json
        $result.ok | Should -BeFalse
        $result.error | Should -Match 'PCV_FEATURE_LEDGER_DOC_STALE'
        (Get-Content -Raw -LiteralPath $doc) | Should -Be $before
    }

    It 'rewrites only the generated block in write mode' {
        $doc = New-LedgerFixture (Join-Path $TestDrive 'write')
        Set-StaleRow $doc
        $result = & $script:Tool -RepoRoot (Join-Path $TestDrive 'write') | ConvertFrom-Json
        $result.ok | Should -BeTrue
        $result.status | Should -Be 'updated'
        (Get-Content -Raw -LiteralPath $doc) | Should -Be (Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot $script:DocRelative))
    }

    It 'refuses a document without exactly one marker pair' {
        $doc = New-LedgerFixture (Join-Path $TestDrive 'markers')
        $text = Get-Content -Raw -LiteralPath $doc
        [System.IO.File]::WriteAllText($doc, $text.Replace('<!-- END GENERATED FEATURE ID SUMMARY -->', ''), [System.Text.UTF8Encoding]::new($false))
        $result = & $script:Tool -RepoRoot (Join-Path $TestDrive 'markers') -Check | ConvertFrom-Json
        $result.ok | Should -BeFalse
        $result.error | Should -Match 'PCV_FEATURE_LEDGER_DOC_MARKERS_INVALID'
    }
}
