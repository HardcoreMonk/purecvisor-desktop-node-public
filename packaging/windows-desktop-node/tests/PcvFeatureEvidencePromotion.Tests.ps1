Set-StrictMode -Version Latest

Describe 'Desktop Node feature evidence ledger' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:LedgerPath = Join-Path $script:RepoRoot 'config/desktop-node-feature-evidence-ledger.json'
        $script:SchemaPath = Join-Path $script:RepoRoot 'config/desktop-node-feature-evidence-ledger.schema.json'
        $script:ExpectedFeatureIds = @(
            'pcv.checkpoint.restore',
            'pcv.vm.managed-import',
            'pcv.vm.media-attach',
            'pcv.vm.saved-lifecycle'
        )
        $script:ExpectedStages = @(
            'code_tested',
            'packaged',
            'installed_tested',
            'actual_vm_tested',
            'manual_admin_tested'
        )
    }

    It 'provides a schema-valid P0 feature ledger' {
        $script:SchemaPath | Should -Exist
        $script:LedgerPath | Should -Exist

        $json = Get-Content -Raw -LiteralPath $script:LedgerPath
        $json | Test-Json -SchemaFile $script:SchemaPath -ErrorAction Stop | Should -BeTrue
    }

    It 'assigns stable ids surfaces stages and evidence to all four P0 features' {
        $ledger = Get-Content -Raw -LiteralPath $script:LedgerPath | ConvertFrom-Json -Depth 64

        $ledger.schema_version | Should -Be 1
        $ledger.contract | Should -Be 'pcv-feature-evidence-ledger-v1'
        @($ledger.features).Count | Should -Be 4
        @($ledger.features.feature_id | Sort-Object) | Should -Be $script:ExpectedFeatureIds
        @($ledger.features.feature_id | Select-Object -Unique).Count | Should -Be 4

        foreach ($feature in $ledger.features) {
            $feature.feature_id | Should -Match '^pcv\.[a-z0-9._-]+$'
            @($feature.surfaces).Count | Should -BeGreaterThan 0
            @($feature.surfaces | Where-Object { $_ -notin @('api', 'cli', 'web') }).Count | Should -Be 0
            @($feature.operations).Count | Should -BeGreaterThan 0
            @($feature.required_stages) | Should -Be $script:ExpectedStages
            $feature.candidate_required | Should -BeTrue
            $feature.current.version | Should -Be '0.42.74-admin-smoke'
            $feature.current.verdict | Should -BeIn @('pass', 'fail', 'blocked', 'missing')
            $feature.current.evidence | Should -Match '^docs/ga-ready/evidence/.+\.md$'
            (Join-Path $script:RepoRoot $feature.current.evidence) | Should -Exist
        }
    }

    It 'records the known 04274 Saved failure without downgrading the other P0 slices' {
        $ledger = Get-Content -Raw -LiteralPath $script:LedgerPath | ConvertFrom-Json -Depth 64
        $saved = @($ledger.features | Where-Object feature_id -EQ 'pcv.vm.saved-lifecycle')
        $other = @($ledger.features | Where-Object feature_id -NE 'pcv.vm.saved-lifecycle')

        $saved.Count | Should -Be 1
        $saved[0].current.verdict | Should -Be 'fail'
        @($other.current.verdict | Where-Object { $_ -ne 'pass' }).Count | Should -Be 0
    }
}

Describe 'Desktop Node feature promotion eligibility' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:LedgerPath = Join-Path $script:RepoRoot 'config/desktop-node-feature-evidence-ledger.json'
        $script:PromotionModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvFeatureEvidencePromotion.psm1'
        $script:PromotionFixtureRoot = Join-Path $PSScriptRoot 'fixtures/feature-evidence-promotion'

        if (Test-Path -LiteralPath $script:PromotionModulePath) {
            Import-Module -Name $script:PromotionModulePath -Force
        }
    }

    AfterAll {
        if (Get-Module -Name PcvFeatureEvidencePromotion) {
            Remove-Module -Name PcvFeatureEvidencePromotion -Force
        }
    }

    It 'blocks the known 04274 Saved actual VM failure' {
        $fixture = Join-Path $script:PromotionFixtureRoot '04274-p0-fail.json'
        $failed = Test-PcvFeaturePromotionEligibility -LedgerPath $script:LedgerPath -ObservationPath $fixture

        $failed.promotion_eligible | Should -BeFalse
        $failed.blockers.feature_id | Should -Contain 'pcv.vm.saved-lifecycle'
        $failed.blockers.stage | Should -Contain 'actual_vm_tested'
        @($failed.blockers | Where-Object {
                $_.feature_id -eq 'pcv.vm.saved-lifecycle' -and
                $_.stage -eq 'actual_vm_tested' -and
                $_.verdict -eq 'fail'
            }).Count | Should -Be 1
    }

    It 'blocks every candidate feature missing manual admin evidence' {
        $fixture = Join-Path $script:PromotionFixtureRoot '04275-missing-manual-admin.json'
        $missing = Test-PcvFeaturePromotionEligibility -LedgerPath $script:LedgerPath -ObservationPath $fixture

        $missing.promotion_eligible | Should -BeFalse
        $missing.blockers.stage | Should -Contain 'manual_admin_tested'
        @($missing.blockers | Where-Object stage -EQ 'manual_admin_tested').Count | Should -Be 4
        @($missing.blockers | Where-Object verdict -NE 'missing').Count | Should -Be 0
    }

    It 'allows a candidate only when all required feature stages pass' {
        $fixture = Join-Path $script:PromotionFixtureRoot '04275-all-pass.json'
        $passed = Test-PcvFeaturePromotionEligibility -LedgerPath $script:LedgerPath -ObservationPath $fixture

        $passed.schema_version | Should -Be 1
        $passed.contract | Should -Be 'pcv-feature-promotion-decision-v1'
        $passed.promotion_eligible | Should -BeTrue
        @($passed.blockers).Count | Should -Be 0
    }

    It 'serializes the same blocker ordering and SHA-256 across three evaluations' {
        $fixture = Join-Path $script:PromotionFixtureRoot '04275-missing-manual-admin.json'
        $decisions = @(1..3 | ForEach-Object {
                Test-PcvFeaturePromotionEligibility -LedgerPath $script:LedgerPath -ObservationPath $fixture
            })
        $hashes = @($decisions | ForEach-Object {
                $json = $_ | ConvertTo-Json -Depth 16 -Compress
                $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
                [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
            })

        @($hashes | Select-Object -Unique).Count | Should -Be 1
        @($decisions[0].blockers.feature_id) | Should -Be @(
            'pcv.checkpoint.restore',
            'pcv.vm.managed-import',
            'pcv.vm.media-attach',
            'pcv.vm.saved-lifecycle'
        )
        @($decisions[0].blockers.stage | Select-Object -Unique) | Should -Be @('manual_admin_tested')
    }
}
