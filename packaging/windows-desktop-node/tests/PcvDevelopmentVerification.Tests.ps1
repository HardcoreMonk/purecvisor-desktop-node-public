BeforeAll {
    $script:ModulePath = Join-Path $PSScriptRoot '../tools/PcvDevelopmentVerification.psm1'
    if (Test-Path -LiteralPath $script:ModulePath -PathType Leaf) {
        Import-Module $script:ModulePath -Force
    }
}

Describe 'Resolve-PcvDevelopmentVerificationSelection' {
    It 'selects dotnet for a source-only tier S change' {
        Get-Command Resolve-PcvDevelopmentVerificationSelection -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $result = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast `
            -ChangeTier S `
            -ChangedPath @('src/DesktopNode.Core/InternalHelper.cs')

        $result.effective_lane | Should -Be 'Fast'
        @($result.suites) | Should -Be @('dotnet')
    }

    It 'selects npm and Web Pester for a Web change' {
        Get-Command Resolve-PcvDevelopmentVerificationSelection -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $result = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast `
            -ChangeTier S `
            -ChangedPath @('web/src/app.ts')

        @($result.suites) | Should -Be @('web-npm', 'web-pester')
    }

    It 'selects the current evidence check for canonical evidence changes' {
        Get-Command Resolve-PcvDevelopmentVerificationSelection -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $result = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast `
            -ChangeTier S `
            -ChangedPath @('docs/ga-ready/current-evidence.json')

        $result.effective_lane | Should -Be 'Release'
        $result.change_tier | Should -Be 'L'
        @($result.tier_reasons) | Should -Contain 'current-evidence-anchor'
        @($result.suites) | Should -Contain 'current-evidence-check'
    }

    It 'promotes an unknown path to Full' {
        Get-Command Resolve-PcvDevelopmentVerificationSelection -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $result = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast `
            -ChangeTier S `
            -ChangedPath @('unclassified/new.txt')

        $result.effective_lane | Should -Be 'Full'
        $result.promotion_reason | Should -Be 'unknown-change-scope'
    }

    It 'promotes tier M to Full and tier L to Release' {
        Get-Command Resolve-PcvDevelopmentVerificationSelection -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $medium = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast -ChangeTier M -ChangedPath @('src/a.cs')
        $large = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast -ChangeTier L -ChangedPath @('src/a.cs')

        $medium.effective_lane | Should -Be 'Full'
        $large.effective_lane | Should -Be 'Release'
    }
}
Describe 'Resolve-PcvDevelopmentChangeTier' {
    It 'keeps an internal single-module source change at S' {
        Get-Command Resolve-PcvDevelopmentChangeTier -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $result = Resolve-PcvDevelopmentChangeTier `
            -RequestedTier S `
            -ChangedPath @('src/DesktopNode.Core/InternalHelper.cs')

        $result.requested_tier | Should -Be 'S'
        $result.effective_tier | Should -Be 'S'
        @($result.reasons).Count | Should -Be 0
    }

    It 'promotes API contract and general packaging changes to at least M' {
        $api = Resolve-PcvDevelopmentChangeTier `
            -RequestedTier S `
            -ChangedPath @('src/DesktopNode.Api/Program.cs')
        $packaging = Resolve-PcvDevelopmentChangeTier `
            -RequestedTier S `
            -ChangedPath @('packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1')

        $api.effective_tier | Should -Be 'M'
        @($api.reasons) | Should -Contain 'api-cli-web-contract'
        $packaging.effective_tier | Should -Be 'M'
        @($packaging.reasons) | Should -Contain 'packaging-contract'
    }

    It 'forces L for installer host mutation security current evidence public and signing boundaries' {
        $cases = @(
            @{ path = 'packaging/windows-desktop-node/installer/build.ps1'; reason = 'installer-lifecycle' },
            @{ path = 'packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1'; reason = 'host-mutation-boundary' },
            @{ path = 'docs/adr/0009-guest-execution-security-boundary.md'; reason = 'security-policy-boundary' },
            @{ path = 'docs/ga-ready/current-evidence.json'; reason = 'current-evidence-anchor' },
            @{ path = 'docs/PUBLIC_RELEASE_BOUNDARY.md'; reason = 'public-release-boundary' },
            @{ path = 'packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1'; reason = 'signing-publication-boundary' }
        )

        foreach ($case in $cases) {
            $result = Resolve-PcvDevelopmentChangeTier `
                -RequestedTier S `
                -ChangedPath @($case.path)
            $result.effective_tier | Should -Be 'L' -Because $case.path
            @($result.reasons) | Should -Contain $case.reason -Because $case.path
        }
    }

    It 'uses the path-derived tier for lane promotion while unknown scope only forces Full' {
        $api = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast -ChangeTier S -ChangedPath @('src/DesktopNode.Api/Program.cs')
        $hostMutation = Resolve-PcvDevelopmentVerificationSelection `
            -Lane Fast -ChangeTier S `
            -ChangedPath @('packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1')
        $unknownTier = Resolve-PcvDevelopmentChangeTier `
            -RequestedTier S -ChangedPath @('unclassified/new.txt')

        $api.effective_lane | Should -Be 'Full'
        $api.change_tier | Should -Be 'M'
        $hostMutation.effective_lane | Should -Be 'Release'
        $hostMutation.change_tier | Should -Be 'L'
        $unknownTier.effective_tier | Should -Be 'S'
    }
}
