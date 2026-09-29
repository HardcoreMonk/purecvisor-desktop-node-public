Set-StrictMode -Version Latest

# Every promotion wrote the EVIDENCE_INDEX and CONTROL_PLANE_INDEX sections by hand (0.42.78:
# ab88076, +32/+26 lines in the 0.42.77 format). This suite pins what the generator does instead:
# values from current-evidence.json, sections at the head of EVIDENCE_INDEX, closure line plus new
# section plus predecessor heading in CONTROL_PLANE_INDEX, and refusal when the inputs disagree.

Describe 'promotion index sections' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ToolPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvPromotionIndexSections.ps1'
        . $script:ToolPath

        $script:EvidenceLines = @(
            '# Desktop Node 증거 인덱스'
            ''
            '## 2026-09-20 `0.42.77` operational current promotion'
            ''
            '- old section.'
            ''
        )
        $script:ControlLines = @(
            '# Control Plane'
            ''
            '## Closure'
            ''
            '- closure paragraph.'
            '  2026-09-20 Lane 3가 `0.42.77-admin-smoke`로 다시 승격했다.'
            ''
            '## 2026-09-20 `0.42.77` current promotion'
            ''
            '- old section.'
            ''
            '## 2026-08-27 `0.42.75` predecessor promotion'
            ''
        )

        function New-PcvTestCurrentEvidence {
            ConvertFrom-PcvPromotionIndexJson -Json (@{
                schema_version = 1
                contract = 'pcv-current-evidence-v1'
                current = @{
                    version = '0.42.78-admin-smoke'
                    package_evidence = 'docs/ga-ready/evidence/package-04278.md'
                    fullgate_batch = 'gate-04278'
                    fullgate_evidence = 'docs/ga-ready/evidence/gate-04278.md'
                    functional_evidence = 'docs/ga-ready/evidence/functional-04278-carryforward.md'
                    installed_evidence = 'docs/ga-ready/evidence/card-04278.md'
                    clean_msi_sha256 = 'a' * 64
                    operational_msi_sha256 = 'b' * 64
                    payload_sha256 = 'c' * 64
                    provenance_commit = '0de176f' + ('d' * 33)
                }
                feature_qualification = @{ promotion_eligible = $true; blockers = @() }
                manual_admin = @{
                    latest_closed_baseline = '0.42.77-admin-smoke'
                    latest_closed_target = '0.42.78-admin-smoke'
                    latest_closed_descriptor = 'descriptor-04277-04278'
                }
                claims = @{ public_trusted_signing = $false; external_stable_publication = $false }
            } | ConvertTo-Json -Depth 5)
        }

        function New-PcvTestPromotionSpec {
            [pscustomobject]([ordered]@{
                schema_version = 1
                contract = 'pcv-promotion-index-sections-v1'
                date = '2026-09-27'
                previous_version = '0.42.77-admin-smoke'
                installed_version = '0.42.78-admin-smoke'
                p0_feature_ledger_version = '0.42.75-admin-smoke'
                pair_evidence = 'docs/ga-ready/evidence/pair-04277-04278.md'
                functional_note = '04275 PASS를 carry-forward한다.'
            })
        }
    }

    It 'puts the current promotion and descriptor consume sections directly below the EVIDENCE_INDEX title' {
        $promotion = Get-PcvPromotionIndexInput -Spec (New-PcvTestPromotionSpec) -CurrentEvidence (New-PcvTestCurrentEvidence)

        $lines = @((Invoke-PcvPromotionIndexSections -EvidenceIndexLines $script:EvidenceLines -ControlPlaneLines $script:ControlLines -Promotion $promotion).evidence_index)

        $lines[2] | Should -BeExactly '## 2026-09-27 `0.42.78` operational current promotion'
        $lines | Should -Contain '- functional carry-forward:'
        $lines | Should -Contain '  / `gate-04278`. operational MSI'
        $lines | Should -Contain ('  `' + ('b' * 64) + '`, provenance `0de176f`.')
        $lines | Should -Contain '## 2026-09-27 `0.42.77 -> 0.42.78` descriptor consume'
        $lines | Should -Contain '- 이 consume는 host mutation을 하지 않았다. 같은 날짜 Lane 3가 current를 `0.42.77-admin-smoke`에서'
        @($lines | Where-Object { $_.StartsWith('- main push:') }).Count | Should -Be 0
        $index = [array]::IndexOf($lines, '## 2026-09-20 `0.42.77` operational current promotion')
        $lines[$index - 1] | Should -BeExactly ''
        $lines[$index - 2] | Should -BeExactly '  주장하지 않는다.'
    }

    It 'adds the optional main push bullet before the canonical current sentence' {
        $spec = New-PcvTestPromotionSpec
        $spec | Add-Member -NotePropertyName main_push_evidence -NotePropertyValue 'docs/ga-ready/evidence/main-push-04278.md'
        $promotion = Get-PcvPromotionIndexInput -Spec $spec -CurrentEvidence (New-PcvTestCurrentEvidence)

        $lines = @(New-PcvEvidenceIndexPromotionLines -Promotion $promotion)

        $index = [array]::IndexOf($lines, '- main push: `docs/ga-ready/evidence/main-push-04278.md`.')
        $index | Should -BeGreaterThan 0
        $lines[$index + 1].StartsWith('- canonical current는 `0.42.78-admin-smoke`다.', [System.StringComparison]::Ordinal) | Should -BeTrue
    }

    It 'adds the closure line and new section and demotes the previous CONTROL_PLANE_INDEX heading' {
        $promotion = Get-PcvPromotionIndexInput -Spec (New-PcvTestPromotionSpec) -CurrentEvidence (New-PcvTestCurrentEvidence)

        $lines = @((Invoke-PcvPromotionIndexSections -EvidenceIndexLines $script:EvidenceLines -ControlPlaneLines $script:ControlLines -Promotion $promotion).control_plane_index)

        $closure = [array]::IndexOf($lines, '  2026-09-20 Lane 3가 `0.42.77-admin-smoke`로 다시 승격했다.')
        $lines[$closure + 1] | Should -BeExactly '  2026-09-27 Lane 3가 `0.42.78-admin-smoke`로 다시 승격했다.'
        $lines[$closure + 2] | Should -BeExactly ''
        $lines[$closure + 3] | Should -BeExactly '## 2026-09-27 `0.42.78` current promotion'
        $lines | Should -Contain '  로 04275 PASS를 carry-forward한다.'
        $lines | Should -Contain '## 2026-09-20 `0.42.77` predecessor promotion'
        $lines | Should -Not -Contain '## 2026-09-20 `0.42.77` current promotion'
        $lines | Should -Contain '## 2026-08-27 `0.42.75` predecessor promotion'
    }

    It 'refuses inputs that disagree with current-evidence.json' {
        $notEligible = New-PcvTestCurrentEvidence
        $notEligible.feature_qualification.promotion_eligible = $false
        $claimed = New-PcvTestCurrentEvidence
        $claimed.claims.public_trusted_signing = $true
        $otherTarget = New-PcvTestCurrentEvidence
        $otherTarget.manual_admin.latest_closed_target = '0.42.77-admin-smoke'
        $samePrevious = New-PcvTestPromotionSpec
        $samePrevious.previous_version = '0.42.78-admin-smoke'
        $unknown = New-PcvTestPromotionSpec
        $unknown | Add-Member -NotePropertyName notes -NotePropertyValue 'x'

        { Get-PcvPromotionIndexInput -Spec (New-PcvTestPromotionSpec) -CurrentEvidence $notEligible } | Should -Throw '*feature_qualification|not-eligible*'
        { Get-PcvPromotionIndexInput -Spec (New-PcvTestPromotionSpec) -CurrentEvidence $claimed } | Should -Throw '*current_evidence.claims|claimed*'
        { Get-PcvPromotionIndexInput -Spec (New-PcvTestPromotionSpec) -CurrentEvidence $otherTarget } | Should -Throw '*latest_closed_target|not-current:0.42.77-admin-smoke*'
        { Get-PcvPromotionIndexInput -Spec $samePrevious -CurrentEvidence (New-PcvTestCurrentEvidence) } | Should -Throw '*previous_version|same-as-current*'
        { Get-PcvPromotionIndexInput -Spec $unknown -CurrentEvidence (New-PcvTestCurrentEvidence) } | Should -Throw '*PCV_PROMOTION_INDEX_INVALID|notes|unknown-field*'
    }

    It 'refuses a second application, a previous heading mismatch, and a missing closure line' {
        $promotion = Get-PcvPromotionIndexInput -Spec (New-PcvTestPromotionSpec) -CurrentEvidence (New-PcvTestCurrentEvidence)
        $applied = Invoke-PcvPromotionIndexSections -EvidenceIndexLines $script:EvidenceLines -ControlPlaneLines $script:ControlLines -Promotion $promotion
        $spec = New-PcvTestPromotionSpec
        $spec.previous_version = '0.42.76-admin-smoke'
        $mismatch = Get-PcvPromotionIndexInput -Spec $spec -CurrentEvidence (New-PcvTestCurrentEvidence)
        $noClosure = @($script:ControlLines | Where-Object { -not $_.Contains('Lane 3가') })

        { Invoke-PcvPromotionIndexSections -EvidenceIndexLines $applied.evidence_index -ControlPlaneLines $script:ControlLines -Promotion $promotion } |
            Should -Throw '*evidence_index|already-applied*'
        { Invoke-PcvPromotionIndexSections -EvidenceIndexLines $script:EvidenceLines -ControlPlaneLines $script:ControlLines -Promotion $mismatch } |
            Should -Throw '*previous-heading-mismatch*'
        { Invoke-PcvPromotionIndexSections -EvidenceIndexLines $script:EvidenceLines -ControlPlaneLines $noClosure -Promotion $promotion } |
            Should -Throw '*control_plane_index|missing-closure-line*'
    }

    It 'reports stale targets until both indexes carry the sections' {
        $promotion = Get-PcvPromotionIndexInput -Spec (New-PcvTestPromotionSpec) -CurrentEvidence (New-PcvTestCurrentEvidence)
        $applied = Invoke-PcvPromotionIndexSections -EvidenceIndexLines $script:EvidenceLines -ControlPlaneLines $script:ControlLines -Promotion $promotion

        @(Get-PcvPromotionIndexStaleTargets -EvidenceIndexLines $script:EvidenceLines -ControlPlaneLines $script:ControlLines -Promotion $promotion) |
            Should -Be @('evidence_index', 'control_plane_index')
        @(Get-PcvPromotionIndexStaleTargets -EvidenceIndexLines $applied.evidence_index -ControlPlaneLines $script:ControlLines -Promotion $promotion) |
            Should -Be @('control_plane_index')
        @(Get-PcvPromotionIndexStaleTargets -EvidenceIndexLines $applied.evidence_index -ControlPlaneLines $applied.control_plane_index -Promotion $promotion).Count |
            Should -Be 0
    }

    It 'plans without writing, applies with the original newlines, and checks from the command line' {
        $directory = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-promotion-index-$([guid]::NewGuid().ToString('N'))"
        New-Item -ItemType Directory -Path $directory | Out-Null
        try {
            $paths = @{
                spec = Join-Path $directory 'spec.json'
                current = Join-Path $directory 'current-evidence.json'
                evidence = Join-Path $directory 'EVIDENCE_INDEX.md'
                control = Join-Path $directory 'CONTROL_PLANE_INDEX.md'
            }
            $utf8 = [System.Text.UTF8Encoding]::new($false)
            [System.IO.File]::WriteAllText($paths.spec, ((New-PcvTestPromotionSpec) | ConvertTo-Json), $utf8)
            [System.IO.File]::WriteAllText($paths.current, ((New-PcvTestCurrentEvidence) | ConvertTo-Json -Depth 5), $utf8)
            [System.IO.File]::WriteAllText($paths.evidence, ($script:EvidenceLines -join "`r`n"), $utf8)
            [System.IO.File]::WriteAllText($paths.control, ($script:ControlLines -join "`n"), $utf8)
            $arguments = @('-SpecPath', $paths.spec, '-CurrentEvidencePath', $paths.current, '-EvidenceIndexPath', $paths.evidence, '-ControlPlaneIndexPath', $paths.control)
            $before = [System.IO.File]::ReadAllText($paths.evidence)

            $plan = & pwsh -NoProfile -File $script:ToolPath @arguments | ConvertFrom-Json
            $plan.status | Should -Be 'planned'
            [System.IO.File]::ReadAllText($paths.evidence) | Should -BeExactly $before

            $applied = & pwsh -NoProfile -File $script:ToolPath @arguments -Apply | ConvertFrom-Json
            $applied.status | Should -Be 'updated'
            [System.IO.File]::ReadAllText($paths.evidence).StartsWith("# Desktop Node 증거 인덱스`r`n`r`n## 2026-09-27 ``0.42.78`` operational current promotion`r`n", [System.StringComparison]::Ordinal) |
                Should -BeTrue
            [System.IO.File]::ReadAllText($paths.control).Contains("`r`n") | Should -BeFalse

            $current = & pwsh -NoProfile -File $script:ToolPath @arguments -Check | ConvertFrom-Json
            $LASTEXITCODE | Should -Be 0
            $current.status | Should -Be 'current'
        }
        finally {
            Remove-Item -LiteralPath $directory -Recurse -Force
        }
    }
}
