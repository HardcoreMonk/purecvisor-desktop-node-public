Set-StrictMode -Version Latest

# Every promotion rotated the CURRENT_EVIDENCE_LEDGER.md status table by hand (0.42.78: ff77943,
# five superseded rows and three replaced rows). This suite pins the rules the generator replaces
# that edit with: predecessor row shape, new row directly below, anchor table only, templated
# predecessor rules with overrides, and no second application. The head keys reuse the descriptor
# chain tool.

Describe 'current evidence ledger rows rotation' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ToolPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceLedgerRows.ps1'
        $script:DescriptorToolPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Update-PcvManualAdminDescriptorChain.ps1'
        . $script:ToolPath
        . $script:DescriptorToolPath

        $script:FixtureLines = @(
            '# Ledger'
            ''
            '<!-- BEGIN GENERATED CURRENT EVIDENCE -->'
            '## Current operational evidence (generated)'
            ''
            '- Version: `0.42.77-admin-smoke`'
            '<!-- END GENERATED CURRENT EVIDENCE -->'
            ''
            'ledger_id: `ledger-1`'
            'current_full_admin_host_mutation: `0.42.77-admin-smoke`'
            'previous_04275_current_full_admin_host_mutation: `0.42.75-admin-smoke`'
            ''
            '## Historical Table'
            ''
            '| ledger key | 상태 | Evidence | 규칙 |'
            '| --- | --- | --- | --- |'
            '| `package-build-current` | `package-build-pass`, `0.42.30-admin-smoke` | `docs/old.md` | historical row. |'
            ''
            '## 현재 Anchor'
            ''
            '| ledger key | current 상태 | Evidence | 운영 규칙 |'
            '| --- | --- | --- | --- |'
            '| `full-admin-host-mutation-current` | `pass`, `0.42.75-admin-smoke` | `docs/gate-04275.md`; `gate-04275`; predecessor after 04277 promotion | 04275 fullgate PASS는 predecessor다. |'
            '| `full-admin-host-mutation-current` | `pass`, `0.42.77-admin-smoke` | `docs/gate-04277.md`; `artifacts/batch-runs/gate-04277`; operational MSI SHA-256 `aaa` | fullgate 04277 PASS. |'
            '| `manual-admin-package-pair-current` | `pass`, `0.42.75-admin-smoke -> 0.42.77-admin-smoke` | `docs/pair-04277.md`; descriptor `d-04277` | pair 04277 PASS. |'
            '| `package-build-current` | `package-build-pass`, `0.42.77-admin-smoke` | `docs/package-04277.md`; `artifacts/package-04277` | package 04277. |'
            '| `latest-product-payload-smoke` | `pass`, package `0.42.77-admin-smoke` | `docs/package-04277.md` | payload 04277. |'
            '| `custom-row-current` | `pass`, `0.42.77-admin-smoke` | `docs/custom-04277.md` | custom 04277. |'
            ''
            '## Later Section'
            ''
        )

        function New-PcvTestLedgerRowsSpec {
            param(
                [object[]]$Supersede = @(),
                [object[]]$Replace = @(),
                [string]$PromotedTag = '04278'
            )

            [pscustomobject]@{
                schema_version = 1
                contract = 'pcv-current-evidence-ledger-rows-rotation-v1'
                promoted_tag = $PromotedTag
                supersede = @($Supersede)
                replace = @($Replace)
            }
        }

        function New-PcvTestLedgerRow {
            param(
                [Parameter(Mandatory)][string]$Id,
                [string]$Status = '`pass`, `0.42.78-admin-smoke`',
                [string]$Evidence = '`docs/new-04278.md`',
                [string]$Rule = 'new 04278 row.',
                [string]$PredecessorRule
            )

            $row = [ordered]@{ id = $Id; status = $Status; evidence = $Evidence; rule = $Rule }
            if ($PSBoundParameters.ContainsKey('PredecessorRule')) { $row.predecessor_rule = $PredecessorRule }
            [pscustomobject]$row
        }
    }

    It 'shortens the current row to a predecessor row and inserts the new row directly below' {
        $spec = New-PcvTestLedgerRowsSpec -Supersede @(New-PcvTestLedgerRow -Id 'full-admin-host-mutation-current')

        $lines = @((Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec $spec).lines)

        $index = [array]::IndexOf($lines, '| `full-admin-host-mutation-current` | `pass`, `0.42.75-admin-smoke` | `docs/gate-04275.md`; `gate-04275`; predecessor after 04277 promotion | 04275 fullgate PASS는 predecessor다. |')
        $index | Should -BeGreaterThan 0
        $lines[$index + 1] | Should -BeExactly '| `full-admin-host-mutation-current` | `pass`, `0.42.77-admin-smoke` | `docs/gate-04277.md`; `gate-04277`; predecessor after 04278 promotion | 04277 fullgate PASS는 predecessor다. |'
        $lines[$index + 2] | Should -BeExactly '| `full-admin-host-mutation-current` | `pass`, `0.42.78-admin-smoke` | `docs/new-04278.md` | new 04278 row. |'
        $lines.Count | Should -Be ($script:FixtureLines.Count + 1)
    }

    It 'keeps only the evidence document for non-fullgate rows and templates pair and package rules' {
        $spec = New-PcvTestLedgerRowsSpec -Supersede @(
            New-PcvTestLedgerRow -Id 'manual-admin-package-pair-current' -Status '`pass`, `0.42.77-admin-smoke -> 0.42.78-admin-smoke`'
            New-PcvTestLedgerRow -Id 'package-build-current' -Status '`package-build-pass`, `0.42.78-admin-smoke`'
        )

        $lines = @((Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec $spec).lines)

        $lines | Should -Contain '| `manual-admin-package-pair-current` | `pass`, `0.42.75-admin-smoke -> 0.42.77-admin-smoke` | `docs/pair-04277.md`; predecessor after 04278 promotion | 04275→04277 pair PASS는 predecessor다. |'
        $lines | Should -Contain '| `package-build-current` | `package-build-pass`, `0.42.77-admin-smoke` | `docs/package-04277.md`; predecessor after 04278 promotion | 04277 package PASS는 predecessor다. |'
        $lines | Should -Contain '| `package-build-current` | `package-build-pass`, `0.42.30-admin-smoke` | `docs/old.md` | historical row. |'
    }

    It 'uses predecessor_rule when given and requires it for rows without a template' {
        $withRule = New-PcvTestLedgerRowsSpec -Supersede @(New-PcvTestLedgerRow -Id 'custom-row-current' -PredecessorRule '04277 custom는 immediate predecessor다.')
        $withoutRule = New-PcvTestLedgerRowsSpec -Supersede @(New-PcvTestLedgerRow -Id 'custom-row-current')

        (Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec $withRule).lines |
            Should -Contain '| `custom-row-current` | `pass`, `0.42.77-admin-smoke` | `docs/custom-04277.md`; predecessor after 04278 promotion | 04277 custom는 immediate predecessor다. |'
        { Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec $withoutRule } |
            Should -Throw '*PCV_LEDGER_ROWS_INVALID|custom-row-current|predecessor-rule-required*'
    }

    It 'replaces a single-line row in place' {
        $spec = New-PcvTestLedgerRowsSpec -Replace @(New-PcvTestLedgerRow -Id 'latest-product-payload-smoke' -Status '`pass`, package `0.42.78-admin-smoke`')

        $rotation = Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec $spec

        $index = [array]::IndexOf($script:FixtureLines, '| `latest-product-payload-smoke` | `pass`, package `0.42.77-admin-smoke` | `docs/package-04277.md` | payload 04277. |')
        $rotation.lines[$index] | Should -BeExactly '| `latest-product-payload-smoke` | `pass`, package `0.42.78-admin-smoke` | `docs/new-04278.md` | new 04278 row. |'
        $rotation.lines.Count | Should -Be $script:FixtureLines.Count
    }

    It 'refuses a second application and a missing or ambiguous current row' {
        $spec = New-PcvTestLedgerRowsSpec -Supersede @(New-PcvTestLedgerRow -Id 'full-admin-host-mutation-current')
        $rotated = @((Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec $spec).lines)
        $last = [array]::IndexOf($script:FixtureLines, '| `custom-row-current` | `pass`, `0.42.77-admin-smoke` | `docs/custom-04277.md` | custom 04277. |')
        $ambiguous = @($script:FixtureLines[0..$last]) +
            @('| `custom-row-current` | `pass`, `0.42.76-admin-smoke` | `docs/custom-04276.md` | custom 04276. |') +
            @($script:FixtureLines[($last + 1)..($script:FixtureLines.Count - 1)])

        { Invoke-PcvLedgerRowsRotation -Lines $rotated -Spec $spec } |
            Should -Throw '*supersede.full-admin-host-mutation-current|already-rotated*'
        { Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec (New-PcvTestLedgerRowsSpec -Supersede @(New-PcvTestLedgerRow -Id 'unknown-row' -PredecessorRule 'x.')) } |
            Should -Throw '*supersede.unknown-row|current-row-count:0*'
        { Invoke-PcvLedgerRowsRotation -Lines $ambiguous -Spec (New-PcvTestLedgerRowsSpec -Supersede @(New-PcvTestLedgerRow -Id 'custom-row-current' -PredecessorRule 'x.')) } |
            Should -Throw '*supersede.custom-row-current|current-row-count:2*'
    }

    It 'rejects invalid specs' {
        $valid = New-PcvTestLedgerRow -Id 'package-build-current'
        $badContract = New-PcvTestLedgerRowsSpec -Supersede @($valid)
        $badContract.contract = 'other'

        { Test-PcvLedgerRowsSpec -Spec $badContract } | Should -Throw '*PCV_LEDGER_ROWS_INVALID|contract|other*'
        { Test-PcvLedgerRowsSpec -Spec (New-PcvTestLedgerRowsSpec -Supersede @($valid) -PromotedTag '4278') } | Should -Throw '*promoted_tag|4278*'
        { Test-PcvLedgerRowsSpec -Spec (New-PcvTestLedgerRowsSpec) } | Should -Throw '*supersede|empty*'
        { Test-PcvLedgerRowsSpec -Spec (New-PcvTestLedgerRowsSpec -Supersede @($valid) -Replace @($valid)) } | Should -Throw '*replace`[0`].id|duplicate:package-build-current*'
        { Test-PcvLedgerRowsSpec -Spec (New-PcvTestLedgerRowsSpec -Supersede @(New-PcvTestLedgerRow -Id 'package-build-current' -Rule 'a | b')) } | Should -Throw '*supersede`[0`].rule|invalid-cell*'
        { Test-PcvLedgerRowsSpec -Spec (New-PcvTestLedgerRowsSpec -Replace @(New-PcvTestLedgerRow -Id 'latest-product-payload-smoke' -PredecessorRule 'x.')) } | Should -Throw '*replace`[0`].predecessor_rule|unknown-field*'
    }

    It 'reports stale ids until the rotation is applied' {
        $spec = New-PcvTestLedgerRowsSpec `
            -Supersede @(New-PcvTestLedgerRow -Id 'full-admin-host-mutation-current') `
            -Replace @(New-PcvTestLedgerRow -Id 'latest-product-payload-smoke')

        @(Get-PcvLedgerRowsStaleIds -Lines $script:FixtureLines -Spec $spec) |
            Should -Be @('full-admin-host-mutation-current', 'latest-product-payload-smoke')
        $rotated = @((Invoke-PcvLedgerRowsRotation -Lines $script:FixtureLines -Spec $spec).lines)
        @(Get-PcvLedgerRowsStaleIds -Lines $rotated -Spec $spec).Count | Should -Be 0
    }

    It 'rotates the ledger head keys with the descriptor chain tool without touching the table' {
        $spec = [pscustomobject]@{
            schema_version = 1
            contract = 'pcv-manual-admin-descriptor-chain-rotation-v1'
            previous_tag = '04277'
            next_previous_tag = '04278'
            values = [pscustomobject]@{ current_full_admin_host_mutation = '0.42.78-admin-smoke' }
        }

        $lines = @((Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $spec).lines)

        $index = [array]::IndexOf($lines, 'current_full_admin_host_mutation: `0.42.78-admin-smoke`')
        $lines[$index + 1] | Should -BeExactly 'previous_04277_current_full_admin_host_mutation: `0.42.77-admin-smoke`'
        $lines[$index + 2] | Should -BeExactly 'previous_04275_current_full_admin_host_mutation: `0.42.75-admin-smoke`'
        @($lines | Where-Object { $_.StartsWith('|') }) | Should -Be @($script:FixtureLines | Where-Object { $_.StartsWith('|') })
    }

    It 'plans without writing, applies with the original newline, and checks from the command line' {
        $directory = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-ledger-rows-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $directory | Out-Null
        try {
            $ledgerPath = Join-Path $directory 'ledger.md'
            $specPath = Join-Path $directory 'spec.json'
            [System.IO.File]::WriteAllText($ledgerPath, ($script:FixtureLines -join "`r`n"), [System.Text.UTF8Encoding]::new($false))
            $spec = New-PcvTestLedgerRowsSpec -Replace @(New-PcvTestLedgerRow -Id 'latest-product-payload-smoke' -Rule 'released at 2026-09-29T00:00:00Z.')
            [System.IO.File]::WriteAllText($specPath, ($spec | ConvertTo-Json -Depth 5), [System.Text.UTF8Encoding]::new($false))
            $before = [System.IO.File]::ReadAllText($ledgerPath)

            $plan = & pwsh -NoProfile -File $script:ToolPath -SpecPath $specPath -LedgerPath $ledgerPath | ConvertFrom-Json
            $plan.status | Should -Be 'planned'
            [System.IO.File]::ReadAllText($ledgerPath) | Should -BeExactly $before

            $stale = & pwsh -NoProfile -File $script:ToolPath -SpecPath $specPath -LedgerPath $ledgerPath -Check | ConvertFrom-Json
            $LASTEXITCODE | Should -Be 1
            $stale.error | Should -BeLike 'PCV_LEDGER_ROWS_STALE|latest-product-payload-smoke*'

            $applied = & pwsh -NoProfile -File $script:ToolPath -SpecPath $specPath -LedgerPath $ledgerPath -Apply | ConvertFrom-Json
            $applied.replaced_count | Should -Be 1
            $after = [System.IO.File]::ReadAllText($ledgerPath)
            $after | Should -BeLike '*released at 2026-09-29T00:00:00Z.*'
            ($after -split "`r`n").Count | Should -Be $script:FixtureLines.Count

            $current = & pwsh -NoProfile -File $script:ToolPath -SpecPath $specPath -LedgerPath $ledgerPath -Check | ConvertFrom-Json
            $LASTEXITCODE | Should -Be 0
            $current.status | Should -Be 'current'
        }
        finally {
            Remove-Item -LiteralPath $directory -Recurse -Force
        }
    }
}
