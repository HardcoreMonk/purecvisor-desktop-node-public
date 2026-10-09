Set-StrictMode -Version Latest

Describe 'PcvPrGate plan-only and approval contract' {
    BeforeAll {
        $script:Tool = Join-Path $PSScriptRoot '../tools/Invoke-PcvPrGate.ps1'
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:Variable = 'PCV_HYPERV_INTEGRATION_APPROVAL'
        $script:Phrase = 'pcv-it- VM creation (ADR-0016 standing approval)'

        function New-CampaignFixture([string]$Root) {
            New-Item -ItemType Directory -Force -Path (Join-Path $Root 'docs/ga-ready') | Out-Null
            @{ approval_locator = ('User-Approval: 2026-10-09 "' + $script:Phrase + '"') } | ConvertTo-Json |
                Set-Content -LiteralPath (Join-Path $Root 'docs/ga-ready/active-campaign.json')
            $Root
        }

        function Read-Summary([string]$ArtifactRoot) {
            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }

        function Invoke-WithApproval([string]$Value, [scriptblock]$Body) {
            $saved = [Environment]::GetEnvironmentVariable($script:Variable)
            try {
                [Environment]::SetEnvironmentVariable($script:Variable, $Value)
                & $Body
            }
            finally {
                [Environment]::SetEnvironmentVariable($script:Variable, $saved)
            }
        }
    }

    It 'plans build, Release test and ratchet without running anything' {
        $root = Join-Path $TestDrive 'plan'
        & $script:Tool -RepoRoot $script:RepoRoot -ArtifactRoot $root -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0
        $summary = Read-Summary $root
        $summary.contract | Should -Be 'pcv-pr-gate-v1'
        $summary.ok | Should -BeTrue
        $summary.plan_only | Should -BeTrue
        $summary.host_mutation_performed | Should -BeFalse
        $summary.integration_approval | Should -Be 'not-requested'
        @($summary.steps | ForEach-Object id) | Should -Be @('build', 'dotnet-test-release', 'module-size-ratchet')
        @($summary.steps | ForEach-Object status | Sort-Object -Unique) | Should -Be @('planned')
        @($summary.steps | Where-Object host_mutation).Count | Should -Be 0
    }

    It 'refuses Integration without the approval variable and runs nothing' {
        $root = Join-Path $TestDrive 'no-approval'
        Invoke-WithApproval $null {
            & $script:Tool -RepoRoot $script:RepoRoot -ArtifactRoot $root -Integration -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 2
        }
        $summary = Read-Summary $root
        $summary.ok | Should -BeFalse
        $summary.integration_approval | Should -Be 'PCV_PR_GATE_INTEGRATION_APPROVAL_MISSING'
        $summary.host_mutation_performed | Should -BeFalse
        @($summary.steps | ForEach-Object id) | Should -Contain 'hyperv-integration'
    }

    It 'refuses an approval phrase that is not in the campaign locator' {
        $repo = New-CampaignFixture (Join-Path $TestDrive 'repo-bad')
        $root = Join-Path $TestDrive 'bad-approval'
        Invoke-WithApproval 'some other sentence that is long enough' {
            & $script:Tool -RepoRoot $repo -ArtifactRoot $root -Integration -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 2
        }
        (Read-Summary $root).integration_approval | Should -Be 'PCV_PR_GATE_INTEGRATION_APPROVAL_NOT_IN_LOCATOR'
    }

    It 'accepts the campaign locator phrase and marks the integration step as host mutation' {
        $repo = New-CampaignFixture (Join-Path $TestDrive 'repo-ok')
        $root = Join-Path $TestDrive 'ok-approval'
        Invoke-WithApproval $script:Phrase {
            & $script:Tool -RepoRoot $repo -ArtifactRoot $root -Integration -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0
        }
        $summary = Read-Summary $root
        $summary.ok | Should -BeTrue
        $summary.integration_approval | Should -Be 'ok'
        $summary.host_mutation_performed | Should -BeFalse
        ($summary.steps | Where-Object id -eq 'hyperv-integration').host_mutation | Should -BeTrue
        ($summary.steps | Where-Object id -eq 'hyperv-integration').command -join ' ' | Should -Be 'dotnet test src/DesktopNode.HyperV.IntegrationTests -c Release --nologo'
    }
}
