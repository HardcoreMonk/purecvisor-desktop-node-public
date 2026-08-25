Set-StrictMode -Version Latest

Describe 'PcvPublicSignedUpdateRollbackSmokePreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvPublicSignedUpdateRollbackSmokePreflight.ps1'

        function New-TestCatalog {
            param(
                [Parameter(Mandatory)][string]$Path,
                [string]$Channel = 'stable',
                [string]$PackageUri = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.zip',
                [string]$PublicTrustedSigning = 'not-claimed',
                [string]$ExternalStablePublication = 'not-claimed'
            )

            $catalog = [ordered]@{
                schema_version = 1
                product = 'PureCVisor Desktop Node'
                publication = [ordered]@{
                    public_trusted_signing = $PublicTrustedSigning
                    external_stable_publication = $ExternalStablePublication
                    catalog_publication = 'not-published'
                }
                channels = @(
                    [ordered]@{
                        name = $Channel
                        version = '0.39.0'
                        package_uri = $PackageUri
                        sha256 = ('E' * 64)
                        release_channel = $Channel
                        signing_mode = 'RequireSigned'
                        rollback_compatible_from = '0.38.8'
                    }
                )
            }

            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
            $catalog | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding utf8
        }

        function Invoke-TestPreflight {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot,
                [string]$Channel = 'stable',
                [string]$PackageUri = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.zip'
            )

            $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
            New-TestCatalog -Path $catalogPath -Channel $Channel -PackageUri $PackageUri

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -CatalogPath $catalogPath `
                -Channel $Channel `
                -ArtifactRoot $ArtifactRoot `
                -BaselineVersion '0.38.8' `
                -CleanHostProfile 'clean-windows-hyperv-public-smoke' `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating public signed update rollback smoke preflight summary' {
        $artifactRoot = Join-Path $TestDrive 'public-signed-update-rollback-smoke-preflight'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'public-signed-update-rollback-smoke-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.public_signed_update_rollback_smoke | Should -Be 'blocked-by-public-signing-and-publication'
        $summary.clean_host_smoke_status | Should -Be 'not-run'
        $summary.channel | Should -Be 'stable'
    }

    It 'records the exact public signed smoke preflight check names' {
        $summary = Invoke-TestPreflight -ArtifactRoot (Join-Path $TestDrive 'public-signed-checks')

        @($summary.preflight_checks | ForEach-Object { $_.name }) | Should -Be @(
            'catalog-schema-v1',
            'selected-channel-present',
            'package-uri-https',
            'package-sha256-present',
            'baseline-version-present',
            'clean-host-profile-recorded',
            'public-trusted-signing-required',
            'external-stable-publication-required',
            'signed-update-rollback-smoke-not-executed',
            'host-mutation-not-executed'
        )
    }

    It 'writes a clean-host smoke plan preview without executing update or rollback' {
        $artifactRoot = Join-Path $TestDrive 'public-signed-smoke-plan'

        $summary = Invoke-TestPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.smoke_plan_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.smoke_plan_path | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.scope | Should -Be 'public-signed-update-rollback-smoke-preflight'
        $plan.clean_host_profile | Should -Be 'clean-windows-hyperv-public-smoke'
        $plan.baseline_version | Should -Be '0.38.8'
        $plan.target_version | Should -Be '0.39.0'
        $plan.update.channel | Should -Be 'stable'
        $plan.update.package_uri | Should -Be 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.zip'
        $plan.update.sha256 | Should -Be ('E' * 64)
        $plan.smoke_status | Should -Be 'not-run'
        $plan.required_evidence | Should -Contain 'public-signed-install'
        $plan.required_evidence | Should -Contain 'public-signed-update'
        $plan.required_evidence | Should -Contain 'rollback-final-state'
        $plan.required_evidence | Should -Contain 'clean-host-health'
    }

    It 'requires plan-only mode' {
        $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
        New-TestCatalog -Path $catalogPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -CatalogPath $catalogPath `
            -Channel stable `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects a non-HTTPS package URI' {
        $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
        New-TestCatalog -Path $catalogPath -PackageUri 'file:///C:/tmp/PureCVisorDesktopNode-0.39.0-windows-x64.zip'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -CatalogPath $catalogPath `
            -Channel stable `
            -ArtifactRoot (Join-Path $TestDrive 'non-https-package') `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects already claimed public signing or publication states without evidence import' {
        $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
        New-TestCatalog -Path $catalogPath -PublicTrustedSigning 'claimed'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -CatalogPath $catalogPath `
            -Channel stable `
            -ArtifactRoot (Join-Path $TestDrive 'claimed') `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation, installer, or update execution command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|msiexec|sc\.exe|Start-Service|Stop-Service|Invoke-PcvDesktopNodeProduct|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove'
    }
}
