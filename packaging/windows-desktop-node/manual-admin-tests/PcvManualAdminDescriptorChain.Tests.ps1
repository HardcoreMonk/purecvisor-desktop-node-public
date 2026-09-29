Set-StrictMode -Version Latest

# Every promotion demoted about 45 descriptor keys to `previous_<tag>_*` by hand (0.42.78: ff77943,
# 89 insertions). This suite pins the rotation rules the generator replaces that edit with: old
# value directly below the key, `next_*` tagged with the promoted version, first occurrence only,
# dated sections untouched, and no second application of the same rotation.

Describe 'manual-admin descriptor chain rotation' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ToolPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Update-PcvManualAdminDescriptorChain.ps1'
        $script:DescriptorPath = Join-Path $script:RepoRoot 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        . $script:ToolPath

        $script:FixtureLines = @(
            '# Descriptor'
            ''
            '## Reading'
            ''
            '| prefix | meaning |'
            ''
            'descriptor_id: `d-04277`'
            'previous_04275_descriptor_id: `d-04275`'
            'current_status: `closed-04277`'
            'next_trigger: `after-04277`'
            'next_trigger_status: `not-opened`'
            'latest_gate_batch: `gate-04277`'
            'problem_codes: `A`, `B`'
            'latest_gate_batch: `gate-04270-historical`'
            ''
            '## 0.42.26 Closure'
            ''
            'current_status: `closed-04226`'
            ''
        )

        function New-PcvTestDescriptorChainSpec {
            param(
                [Parameter(Mandatory)][System.Collections.IDictionary]$Values,
                [string]$PreviousTag = '04277',
                [string]$NextPreviousTag = '04278'
            )

            [pscustomobject]@{
                schema_version = 1
                contract = 'pcv-manual-admin-descriptor-chain-rotation-v1'
                previous_tag = $PreviousTag
                next_previous_tag = $NextPreviousTag
                values = [pscustomobject]$Values
            }
        }
    }

    It 'demotes the old value to a previous_ line directly below the key' {
        $spec = New-PcvTestDescriptorChainSpec -Values ([ordered]@{
            descriptor_id = 'd-04278'
            current_status = 'closed-04278'
        })

        $lines = @((Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $spec).lines)

        $index = [array]::IndexOf($lines, 'descriptor_id: `d-04278`')
        $index | Should -BeGreaterThan 0
        $lines[$index + 1] | Should -BeExactly 'previous_04277_descriptor_id: `d-04277`'
        $lines[$index + 2] | Should -BeExactly 'previous_04275_descriptor_id: `d-04275`'
        $statusIndex = [array]::IndexOf($lines, 'current_status: `closed-04278`')
        $lines[$statusIndex + 1] | Should -BeExactly 'previous_04277_current_status: `closed-04277`'
    }

    It 'tags next_ keys with the promoted version and snapshots unchanged values' {
        $spec = New-PcvTestDescriptorChainSpec -Values ([ordered]@{
            next_trigger = 'after-04278'
            next_trigger_status = 'not-opened'
        })

        $rotation = Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $spec

        $rotation.lines | Should -Contain 'previous_04278_next_trigger: `after-04277`'
        $rotation.lines | Should -Contain 'previous_04278_next_trigger_status: `not-opened`'
        @($rotation.changes | Where-Object { $_.value_changed }).key | Should -Be @('next_trigger')
    }

    It 'rotates only the first occurrence and leaves dated sections untouched' {
        $spec = New-PcvTestDescriptorChainSpec -Values ([ordered]@{
            latest_gate_batch = 'gate-04278'
            current_status = 'closed-04278'
        })

        $rotation = Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $spec
        $lines = @($rotation.lines)

        $lines.Count | Should -Be ($script:FixtureLines.Count + 2)
        @($rotation.duplicate_keys) | Should -Be @('latest_gate_batch')
        $lines | Should -Contain 'latest_gate_batch: `gate-04270-historical`'
        $sectionIndex = [array]::IndexOf($lines, '## 0.42.26 Closure')
        $lines[($sectionIndex + 1)..($lines.Count - 1)] | Should -Be $script:FixtureLines[16..18]
    }

    It 'reads spec strings verbatim instead of letting JSON parsing convert them' {
        # ConvertFrom-Json turned the 0.42.78 updated_at value into DateTime during the replay of
        # ff77943; the rotation must write back exactly the text the spec carries.
        $spec = ConvertFrom-PcvDescriptorChainSpecJson -Json (@'
{"schema_version":1,"contract":"pcv-manual-admin-descriptor-chain-rotation-v1","previous_tag":"04277",
 "next_previous_tag":"04278","values":{"updated_at":"2026-09-27T15:48:21+09:00","run_id":36299263811}}
'@)

        $spec.values.updated_at | Should -BeExactly '2026-09-27T15:48:21+09:00'
        $spec.values.run_id | Should -BeNullOrEmpty
        { Test-PcvDescriptorChainSpec -Spec $spec } | Should -Throw '*values.run_id|invalid-value'
    }

    It 'refuses a second application of the same rotation' {
        $spec = New-PcvTestDescriptorChainSpec -Values ([ordered]@{ current_status = 'closed-04278' })
        $once = Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $spec

        { Invoke-PcvDescriptorChainRotation -Lines $once.lines -Spec $spec } |
            Should -Throw '*already-rotated:previous_04277_current_status*'
    }

    It 'refuses keys and values it cannot rotate faithfully' {
        $cases = @(
            @{ values = [ordered]@{ current_unknown = 'x' }; detail = '*missing-in-descriptor*' }
            @{ values = [ordered]@{ problem_codes = 'C' }; detail = '*unsupported-value-format*' }
            @{ values = [ordered]@{ previous_04277_current_status = 'x' }; detail = '*invalid-key*' }
            @{ values = [ordered]@{ current_status = 'has`tick' }; detail = '*invalid-value*' }
            @{ values = [ordered]@{ current_status = 36299263811 }; detail = '*invalid-value*' }
        )
        foreach ($case in $cases) {
            $spec = New-PcvTestDescriptorChainSpec -Values $case.values
            { Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $spec } |
                Should -Throw $case.detail
        }

        $badTag = New-PcvTestDescriptorChainSpec -Values ([ordered]@{ current_status = 'x' }) -PreviousTag '4277'
        { Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $badTag } |
            Should -Throw '*|previous_tag|4277'
    }

    It 'reports stale keys until the rotation is applied' {
        $spec = New-PcvTestDescriptorChainSpec -Values ([ordered]@{
            descriptor_id = 'd-04278'
            next_trigger = 'after-04278'
        })

        @(Get-PcvDescriptorChainStaleKeys -Lines $script:FixtureLines -Spec $spec) |
            Should -Be @('descriptor_id', 'next_trigger')
        $rotated = Invoke-PcvDescriptorChainRotation -Lines $script:FixtureLines -Spec $spec
        @(Get-PcvDescriptorChainStaleKeys -Lines $rotated.lines -Spec $spec) | Should -BeNullOrEmpty
    }

    It 'round-trips the live descriptor head block without touching any other line' {
        $original = [System.IO.File]::ReadAllText($script:DescriptorPath).Replace("`r`n", "`n").Split("`n")
        # 09999 never occurs in the live chain, so the already-rotated guard cannot fire here.
        $spec = New-PcvTestDescriptorChainSpec -PreviousTag '09999' -NextPreviousTag '09999' -Values ([ordered]@{
            descriptor_id = 'round-trip-descriptor'
            current_status = 'round-trip-status'
            next_manual_admin_package_pair_candidate_status = 'round-trip-next'
            latest_full_admin_gate_batch = 'round-trip-gate'
        })

        $rotation = Invoke-PcvDescriptorChainRotation -Lines $original -Spec $spec
        $restored = [System.Collections.Generic.List[string]]::new([string[]]$rotation.lines)
        foreach ($change in @($rotation.changes)) {
            $index = $restored.IndexOf("$($change.key): ``$($change.new)``")
            $restored[$index + 1] | Should -BeExactly "$($change.previous_key): ``$($change.old)``"
            $restored[$index] = "$($change.key): ``$($change.old)``"
            $restored.RemoveAt($index + 1)
        }

        @($rotation.lines).Count | Should -Be ($original.Count + 4)
        @($rotation.duplicate_keys) | Should -Contain 'latest_full_admin_gate_batch'
        ($restored.ToArray() -join "`n") | Should -BeExactly ($original -join "`n")
    }

    It 'runs as a script: dry-run keeps the file, apply rotates, check confirms, reapply fails' {
        $descriptor = Join-Path $TestDrive 'descriptor.md'
        $specPath = Join-Path $TestDrive 'spec.json'
        [System.IO.File]::WriteAllText($descriptor, ($script:FixtureLines -join "`n"), [System.Text.UTF8Encoding]::new($false))
        New-PcvTestDescriptorChainSpec -Values ([ordered]@{ current_status = 'closed-04278' }) |
            ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $specPath -Encoding utf8NoBOM
        $before = (Get-FileHash -LiteralPath $descriptor).Hash

        $dryRun = & $script:ToolPath -SpecPath $specPath -DescriptorPath $descriptor | ConvertFrom-Json
        $dryRun.ok | Should -BeTrue
        $dryRun.status | Should -Be 'planned'
        (Get-FileHash -LiteralPath $descriptor).Hash | Should -Be $before

        (& $script:ToolPath -SpecPath $specPath -DescriptorPath $descriptor -Check | ConvertFrom-Json).ok | Should -BeFalse

        $applied = & $script:ToolPath -SpecPath $specPath -DescriptorPath $descriptor -Apply | ConvertFrom-Json
        $applied.status | Should -Be 'updated'
        $applied.rotated_key_count | Should -Be 1
        $text = [System.IO.File]::ReadAllText($descriptor)
        $text | Should -Match "(?m)^current_status: ``closed-04278``\nprevious_04277_current_status: ``closed-04277``$"
        $text.Contains("`r") | Should -BeFalse

        (& $script:ToolPath -SpecPath $specPath -DescriptorPath $descriptor -Check | ConvertFrom-Json).status |
            Should -Be 'current'
        $reapply = & $script:ToolPath -SpecPath $specPath -DescriptorPath $descriptor -Apply | ConvertFrom-Json
        $reapply.ok | Should -BeFalse
        $reapply.error | Should -Match 'already-rotated'
    }
}
