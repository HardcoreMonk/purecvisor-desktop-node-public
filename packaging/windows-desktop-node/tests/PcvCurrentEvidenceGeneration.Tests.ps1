Set-StrictMode -Version Latest

Describe 'current evidence canonical record' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/current-evidence.json'
        $script:SchemaPath = Join-Path $script:RepoRoot 'docs/ga-ready/current-evidence.schema.json'
        $script:GeneratorPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1'
        if (Test-Path -LiteralPath $script:GeneratorPath -PathType Leaf) {
            . $script:GeneratorPath
        }
        $script:Record = Get-Content -Raw -LiteralPath $script:EvidencePath | ConvertFrom-Json
    }

    It 'contains a well-formed CLI Web only anchor' {
        $script:EvidencePath | Should -Exist
        $script:SchemaPath | Should -Exist
        $record = Get-Content -Raw -LiteralPath $script:EvidencePath | ConvertFrom-Json

        $record.schema_version | Should -Be 1
        $record.contract | Should -Be 'pcv-current-evidence-v1'
        # Asserts the anchor's shape rather than one pinned version. Pinning made every
        # legitimate anchor promotion fail this contract for the wrong reason.
        $record.current.version | Should -Match '^0\.\d+\.\d+-admin-smoke$'
        @($record.current.operator_surfaces) | Should -Be @('web', 'cli')
        $record.current.tui_present | Should -BeFalse
        $record.current.provenance_commit | Should -Match '^[0-9a-f]{40}$'
    }

    It 'requires a schema-valid blocked feature qualification for current 04274' {
        $json = Get-Content -Raw -LiteralPath $script:EvidencePath
        $json | Test-Json -SchemaFile $script:SchemaPath -ErrorAction Stop | Should -BeTrue
        $record = $json | ConvertFrom-Json -Depth 64

        $record.feature_qualification.schema_version | Should -Be 1
        $record.feature_qualification.contract | Should -Be 'pcv-feature-promotion-decision-v1'
        $record.feature_qualification.promotion_eligible | Should -BeTrue
        @($record.feature_qualification.blockers).Count | Should -Be 0
    }

    It 'rejects contradictory eligibility and blocker combinations in the schema' {
        $eligible = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
        $eligible.feature_qualification.promotion_eligible = $true
        $eligible.feature_qualification.blockers = @()
        ($eligible | ConvertTo-Json -Depth 64) |
            Test-Json -SchemaFile $script:SchemaPath -ErrorAction Stop |
            Should -BeTrue

        $contradictory = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
        $contradictory.feature_qualification.promotion_eligible = $false
        ($contradictory | ConvertTo-Json -Depth 64) |
            Test-Json -SchemaFile $script:SchemaPath -ErrorVariable schemaErrors -ErrorAction SilentlyContinue |
            Should -BeFalse
        @($schemaErrors).Count | Should -BeGreaterThan 0
    }

    It 'keeps schema and runtime validation in parity for invalid qualification values' {
        $cases = @(
            [pscustomobject]@{
                name = 'contract-case'
                expected_field = 'feature_qualification.contract'
            },
            [pscustomobject]@{
                name = 'contract-key-case'
                expected_field = 'feature_qualification.contract'
            },
            [pscustomobject]@{
                name = 'qualification-extra-property'
                expected_field = 'feature_qualification.unexpected'
            },
            [pscustomobject]@{
                name = 'blocker-extra-property'
                expected_field = 'feature_qualification.blockers.unexpected'
            },
            [pscustomobject]@{
                name = 'feature-id-case'
                expected_field = 'feature_qualification.blockers.feature_id'
            },
            [pscustomobject]@{
                name = 'stage-case'
                expected_field = 'feature_qualification.blockers.stage'
            },
            [pscustomobject]@{
                name = 'verdict-case'
                expected_field = 'feature_qualification.blockers.verdict'
            },
            [pscustomobject]@{
                name = 'schema-version-string'
                expected_field = 'feature_qualification.schema_version'
            },
            [pscustomobject]@{
                name = 'root-schema-version-string'
                expected_field = 'schema_version'
            },
            [pscustomobject]@{
                name = 'blockers-scalar'
                expected_field = 'feature_qualification.blockers'
            },
            [pscustomobject]@{
                name = 'blocked-empty'
                expected_field = 'feature_qualification.blockers'
            },
            [pscustomobject]@{
                name = 'current-version-case'
                expected_field = 'current.version'
            }
        )
        $failures = @(
            foreach ($case in $cases) {
                $record = $script:Record |
                    ConvertTo-Json -Depth 64 |
                    ConvertFrom-Json -Depth 64
                switch ($case.name) {
                    'contract-case' {
                        $record.feature_qualification.contract = 'PCV-FEATURE-PROMOTION-DECISION-V1'
                    }
                    'contract-key-case' {
                        $record.feature_qualification = [pscustomobject]@{
                            schema_version = $record.feature_qualification.schema_version
                            Contract = $record.feature_qualification.contract
                            promotion_eligible = $record.feature_qualification.promotion_eligible
                            blockers = $record.feature_qualification.blockers
                        }
                    }
                    'qualification-extra-property' {
                        $record.feature_qualification |
                            Add-Member -NotePropertyName 'unexpected' -NotePropertyValue $true
                    }
                    'blocker-extra-property' {
                        $record.feature_qualification.promotion_eligible = $false
                        $record.feature_qualification.blockers = @(
                            [pscustomobject]@{
                                feature_id = 'pcv.vm.saved-lifecycle'
                                stage = 'actual_vm_tested'
                                verdict = 'fail'
                            }
                        )
                        $record.feature_qualification.blockers[0] |
                            Add-Member -NotePropertyName 'unexpected' -NotePropertyValue $true
                    }
                    'feature-id-case' {
                        $record.feature_qualification.promotion_eligible = $false
                        $record.feature_qualification.blockers = @(
                            [pscustomobject]@{
                                feature_id = 'PCV.vm.saved-lifecycle'
                                stage = 'actual_vm_tested'
                                verdict = 'fail'
                            }
                        )
                    }
                    'stage-case' {
                        $record.feature_qualification.promotion_eligible = $false
                        $record.feature_qualification.blockers = @(
                            [pscustomobject]@{
                                feature_id = 'pcv.vm.saved-lifecycle'
                                stage = 'ACTUAL_VM_TESTED'
                                verdict = 'fail'
                            }
                        )
                    }
                    'verdict-case' {
                        $record.feature_qualification.promotion_eligible = $false
                        $record.feature_qualification.blockers = @(
                            [pscustomobject]@{
                                feature_id = 'pcv.vm.saved-lifecycle'
                                stage = 'actual_vm_tested'
                                verdict = 'FAIL'
                            }
                        )
                    }
                    'schema-version-string' {
                        $record.feature_qualification.schema_version = '1'
                    }
                    'root-schema-version-string' {
                        $record.schema_version = '1'
                    }
                    'blockers-scalar' {
                        $record.feature_qualification.promotion_eligible = $false
                        $failBlocker = [pscustomobject]@{
                            feature_id = 'pcv.vm.saved-lifecycle'
                            stage = 'actual_vm_tested'
                            verdict = 'fail'
                        }
                        $record.feature_qualification.blockers = $failBlocker
                    }
                    'blocked-empty' {
                        $record.feature_qualification.promotion_eligible = $false
                    }
                    'current-version-case' {
                        $record.current.version = '0.42.75-ADMIN-SMOKE'
                    }
                }

                $schemaErrors = @()
                $schemaValid = ($record | ConvertTo-Json -Depth 64) |
                    Test-Json -SchemaFile $script:SchemaPath `
                        -ErrorVariable schemaErrors `
                        -ErrorAction SilentlyContinue
                if ($schemaValid) {
                    "schema-accepted:$($case.name)"
                }

                try {
                    if ($case.name -in @('current-version-case', 'root-schema-version-string')) {
                        [void](Test-PcvCurrentEvidenceRecord -Record $record -RepoRoot $script:RepoRoot)
                    }
                    else {
                        [void](Test-PcvFeatureQualification -Qualification $record.feature_qualification)
                    }
                    "runtime-accepted:$($case.name)"
                }
                catch {
                    $prefix = "PCV_CURRENT_EVIDENCE_INVALID|$($case.expected_field)|"
                    if (-not ([string]$_).StartsWith($prefix, [System.StringComparison]::Ordinal)) {
                        "wrong-error:$($case.name):$($_.Exception.Message)"
                    }
                }
            }
        )

        $failures | Should -BeNullOrEmpty
    }

    It 'renders the feature qualification independently of operational current' {
        $block = ConvertTo-PcvCurrentEvidenceMarkdown -Record $script:Record

        $block | Should -Match 'Feature qualification:'
        $block | Should -Match 'promotion_eligible=true'
        $block | Should -Match 'blocker_count=0'
        $block | Should -Match 'blockers=none'
        $block | Should -Match ([regex]::Escape([string]$script:Record.current.version))
    }

    It 'rejects a blocked candidate before writing any source or target file' {
        $candidatePath = Join-Path $TestDrive '04275-blocked.json'
        $candidate = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
        $candidate.current.version = '0.42.76-admin-smoke'
        $candidate.feature_qualification.promotion_eligible = $false
        $candidate.feature_qualification.blockers = @(
            [pscustomobject]@{
                feature_id = 'pcv.vm.saved-lifecycle'
                stage = 'actual_vm_tested'
                verdict = 'fail'
            }
        )
        $candidate | ConvertTo-Json -Depth 64 | Set-Content -LiteralPath $candidatePath -Encoding utf8

        $ownedPaths = @(
            $candidatePath,
            $script:EvidencePath,
            (Join-Path $script:RepoRoot 'README.md'),
            (Join-Path $script:RepoRoot 'AGENTS.md'),
            (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md'),
            (Join-Path $script:RepoRoot 'docs/ga-ready/EVIDENCE_INDEX.md'),
            (Join-Path $script:RepoRoot 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'),
            (Join-Path $script:RepoRoot 'docs/ga-ready/CONTROL_PLANE_INDEX.md'),
            (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'),
            (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/README.md')
        )
        $before = @{}
        foreach ($path in $ownedPaths) {
            $before[$path] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        }

        $output = & pwsh -NoProfile -File $script:GeneratorPath `
            -EvidencePath $candidatePath `
            -RepoRoot $script:RepoRoot `
            -Check 2>&1
        $LASTEXITCODE | Should -Be 1
        ($output -join "`n") | Should -Match 'PCV_FEATURE_PROMOTION_BLOCKED'
        foreach ($path in $ownedPaths) {
            (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash |
                Should -Be $before[$path] -Because $path
        }
    }

    It 'rejects a case-only blocked candidate before write mode changes an isolated repository' {
        $fixtureRoot = Join-Path $TestDrive 'write-mode-repository'
        $canonicalPath = Join-Path $fixtureRoot 'docs/ga-ready/current-evidence.json'
        $candidatePath = Join-Path $fixtureRoot 'blocked-case-candidate.json'
        [void](New-Item -ItemType Directory -Path (Split-Path -Parent $canonicalPath) -Force)

        $canonical = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
        $canonical | ConvertTo-Json -Depth 64 |
            Set-Content -LiteralPath $canonicalPath -Encoding utf8
        $candidate = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
        $candidate.current.version = '0.42.75-ADMIN-SMOKE'
        $candidate | ConvertTo-Json -Depth 64 |
            Set-Content -LiteralPath $candidatePath -Encoding utf8

        $targetRelativePaths = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ga-ready/EVIDENCE_INDEX.md',
            'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md',
            'docs/ga-ready/CONTROL_PLANE_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md'
        )
        $targetPaths = @(
            foreach ($relativePath in $targetRelativePaths) {
                $targetPath = Join-Path $fixtureRoot $relativePath
                [void](New-Item -ItemType Directory -Path (Split-Path -Parent $targetPath) -Force)
                @"
# fixture: $relativePath
<!-- BEGIN GENERATED CURRENT EVIDENCE -->
stale
<!-- END GENERATED CURRENT EVIDENCE -->
fixture tail
"@ | Set-Content -LiteralPath $targetPath -Encoding utf8 -NoNewline
                $targetPath
            }
        )
        $ownedPaths = @($candidatePath, $canonicalPath) + $targetPaths
        $before = @{}
        foreach ($path in $ownedPaths) {
            $before[$path] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        }

        $output = & pwsh -NoProfile -File $script:GeneratorPath `
            -EvidencePath $candidatePath `
            -RepoRoot $fixtureRoot 2>&1
        $LASTEXITCODE | Should -Be 1
        ($output -join "`n") | Should -Match 'PCV_FEATURE_PROMOTION_BLOCKED'
        foreach ($path in $ownedPaths) {
            (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash |
                Should -Be $before[$path] -Because $path
        }
    }

    It 'rejects malformed SHA and missing evidence references' {
        Get-Command Test-PcvCurrentEvidenceRecord -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        {
            Test-PcvCurrentEvidenceRecord `
                -Record ([pscustomobject]@{ schema_version = 1 }) `
                -RepoRoot $script:RepoRoot
        } | Should -Throw '*PCV_CURRENT_EVIDENCE_INVALID*'
    }

    It 'renders one bounded CLI Web current block' {
        Get-Command ConvertTo-PcvCurrentEvidenceMarkdown -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty

        $block = ConvertTo-PcvCurrentEvidenceMarkdown -Record $script:Record

        $block | Should -Match '<!-- BEGIN GENERATED CURRENT EVIDENCE -->'
        $block | Should -Match ([regex]::Escape([string]$script:Record.current.version))
        $block | Should -Match 'Web Console.*PCVCLI'
        $block | Should -Match 'tui_present.*false'
        $block | Should -Not -Match 'Web/TUI/CLI current-card'
    }

    It 'fails Check when a target block is stale without writing' {
        Get-Command Update-PcvCurrentEvidenceDocument -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        $target = Join-Path $TestDrive 'stale.md'
        @'
# Test
<!-- BEGIN GENERATED CURRENT EVIDENCE -->
stale
<!-- END GENERATED CURRENT EVIDENCE -->
historical text
'@ | Set-Content -LiteralPath $target -NoNewline
        $before = Get-Content -Raw -LiteralPath $target
        $block = ConvertTo-PcvCurrentEvidenceMarkdown -Record $script:Record

        { Update-PcvCurrentEvidenceDocument -Path $target -Block $block -Check } |
            Should -Throw '*PCV_CURRENT_EVIDENCE_STALE*'
        (Get-Content -Raw -LiteralPath $target) | Should -BeExactly $before
    }

    It 'keeps exactly one current block in every owned document' {
        $block = ConvertTo-PcvCurrentEvidenceMarkdown -Record $script:Record
        $targets = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ga-ready/EVIDENCE_INDEX.md',
            'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md',
            'docs/ga-ready/CONTROL_PLANE_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md'
        )

        foreach ($relativePath in $targets) {
            $target = Join-Path $script:RepoRoot $relativePath
            $content = Get-Content -Raw -LiteralPath $target
            ([regex]::Matches($content, '<!-- BEGIN GENERATED CURRENT EVIDENCE -->').Count) |
                Should -Be 1 -Because $relativePath
            ([regex]::Matches($content, '<!-- END GENERATED CURRENT EVIDENCE -->').Count) |
                Should -Be 1 -Because $relativePath
            { Update-PcvCurrentEvidenceDocument -Path $target -Block $block -Check } |
                Should -Not -Throw -Because $relativePath
        }

        $agents = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'AGENTS.md')
        $agents.IndexOf('## 2026-07-13 historical TUI predecessor') |
            Should -BeGreaterThan $agents.IndexOf('<!-- END GENERATED CURRENT EVIDENCE -->')
        $readme = Get-Content -Raw -LiteralPath (
            Join-Path $script:RepoRoot 'packaging/windows-desktop-node/README.md')
        $readme.IndexOf('## 2026-07-13 historical TUI predecessor') |
            Should -BeGreaterThan $readme.IndexOf('<!-- END GENERATED CURRENT EVIDENCE -->')
    }

    It 'publishes the canonical record as the API current evidence asset' {
        $projectPath = Join-Path $script:RepoRoot 'src/DesktopNode.Api/DesktopNode.Api.csproj'
        [xml]$project = Get-Content -Raw -LiteralPath $projectPath
        $content = @($project.SelectNodes('/Project/ItemGroup/Content') | Where-Object {
                $_.Include -eq '..\..\docs\ga-ready\current-evidence.json'
            })

        $content.Count | Should -Be 1
        [string]$content[0].Link | Should -Be 'evidence\current-evidence.json'
        [string]$content[0].CopyToOutputDirectory | Should -Be 'PreserveNewest'
        [string]$content[0].CopyToPublishDirectory | Should -Be 'PreserveNewest'
    }
}
