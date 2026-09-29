Set-StrictMode -Version Latest

# The 0.42.78 promotion took four commits of hand edits (064f6f2, 7345d62, ff77943, ab88076). The
# orchestrator runs the generated-block, descriptor, ledger, index, and spec-pin tools from one
# spec. This suite pins the spec split, the step order, and the per-mode arguments; the committed
# 0.42.78 spec is the example the procedure document points to.

Describe 'Lane 3 promotion docs orchestration' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ToolPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvLane3PromotionDocs.ps1'
        $script:FixturePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-04278.json'
        . $script:ToolPath

        function Invoke-PcvTestSplit {
            param([Parameter(Mandatory)][string]$Json)

            $directory = Join-Path ([System.IO.Path]::GetTempPath()) "pcv-lane3-split-$([guid]::NewGuid().ToString('N'))"
            New-Item -ItemType Directory -Path $directory | Out-Null
            try {
                $paths = Split-PcvLane3PromotionSpec -Json $Json -Directory $directory
                [pscustomobject]@{
                    names = @($paths.Keys)
                    texts = @($paths.Values | ForEach-Object { [System.IO.File]::ReadAllText($_) })
                }
            }
            finally {
                Remove-Item -LiteralPath $directory -Recurse -Force
            }
        }
    }

    It 'runs the generated blocks first and the spec pins last' {
        @($script:PcvLane3Steps.name) | Should -Be @('current_evidence_docs', 'descriptor_chain', 'ledger_head', 'ledger_rows', 'index_sections', 'spec_pins')
        foreach ($step in $script:PcvLane3Steps) {
            Test-Path -LiteralPath (Join-Path (Split-Path -Parent $script:ToolPath) $step.tool) -PathType Leaf | Should -BeTrue
        }
    }

    It 'splits the 0.42.78 example into the four tool specs verbatim' {
        $json = [System.IO.File]::ReadAllText($script:FixturePath)

        $split = Invoke-PcvTestSplit -Json $json

        $split.names | Should -Be @('descriptor_chain', 'ledger_head', 'ledger_rows', 'index_sections')
        ($split.texts[0] | ConvertFrom-Json).contract | Should -Be 'pcv-manual-admin-descriptor-chain-rotation-v1'
        @(($split.texts[0] | ConvertFrom-Json -AsHashtable).values.Keys).Count | Should -Be 45
        $split.texts[0] | Should -Match '"updated_at": "2026-09-27T\d{2}:\d{2}:\d{2}'
        ($split.texts[1] | ConvertFrom-Json).contract | Should -Be 'pcv-manual-admin-descriptor-chain-rotation-v1'
        ($split.texts[2] | ConvertFrom-Json).contract | Should -Be 'pcv-current-evidence-ledger-rows-rotation-v1'
        ($split.texts[3] | ConvertFrom-Json).contract | Should -Be 'pcv-promotion-index-sections-v1'
    }

    It 'refuses a wrong contract, an unknown section, and a missing section' {
        $spec = [System.IO.File]::ReadAllText($script:FixturePath) | ConvertFrom-Json -AsHashtable
        $wrongContract = $spec.Clone(); $wrongContract.contract = 'other'
        $unknown = $spec.Clone(); $unknown.notes = @{}
        $missing = $spec.Clone(); $missing.Remove('ledger_rows')

        { Invoke-PcvTestSplit -Json ($wrongContract | ConvertTo-Json -Depth 8) } | Should -Throw '*PCV_LANE3_PROMOTION_DOCS_INVALID|contract|invalid*'
        { Invoke-PcvTestSplit -Json ($unknown | ConvertTo-Json -Depth 8) } | Should -Throw '*|notes|unknown-field*'
        { Invoke-PcvTestSplit -Json ($missing | ConvertTo-Json -Depth 8) } | Should -Throw '*|ledger_rows|missing*'
    }

    It 'maps each mode to the arguments of each tool' {
        $specPaths = @{ descriptor_chain = 'd.json'; ledger_head = 'h.json'; ledger_rows = 'r.json'; index_sections = 'i.json' }
        $steps = @{}
        foreach ($step in $script:PcvLane3Steps) { $steps[$step.name] = $step }
        function Get-Tail {
            param([string]$Name, [string]$Mode)

            @(Get-PcvLane3StepArguments -Step $steps[$Name] -RepoRoot 'R' -SpecPaths $specPaths -Mode $Mode | Select-Object -Skip 5)
        }

        Get-Tail -Name 'current_evidence_docs' -Mode 'dry-run' | Should -Be @('-Check')
        @(Get-Tail -Name 'current_evidence_docs' -Mode 'apply').Count | Should -Be 0
        Get-Tail -Name 'descriptor_chain' -Mode 'dry-run' | Should -Be @('-SpecPath', 'd.json')
        Get-Tail -Name 'ledger_head' -Mode 'apply' | Should -Be @('-SpecPath', 'h.json', '-DescriptorPath', 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md', '-Apply')
        Get-Tail -Name 'index_sections' -Mode 'check' | Should -Be @('-SpecPath', 'i.json', '-Check')
        Get-Tail -Name 'spec_pins' -Mode 'apply' | Should -Be @('-Apply')
    }
}
