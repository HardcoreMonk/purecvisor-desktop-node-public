Set-StrictMode -Version Latest

Describe 'PcvUpdaterCatalogPublicationPreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvUpdaterCatalogPublicationPreflight.ps1'

        function New-TestUpdaterCatalog {
            param(
                [Parameter(Mandatory)][string]$Path,
                [string]$Channel = 'stable',
                [string]$PackageUri = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.zip'
            )

            $catalog = [ordered]@{
                schema_version = 1
                product = 'PureCVisor Desktop Node'
                publication = [ordered]@{
                    public_trusted_signing = 'not-claimed'
                    external_stable_publication = 'not-claimed'
                    catalog_publication = 'not-published'
                }
                channels = @(
                    [ordered]@{
                        name = $Channel
                        version = '0.39.0'
                        package_uri = $PackageUri
                        sha256 = ('B' * 64)
                        release_channel = $Channel
                        signing_mode = 'RequireSigned'
                    }
                )
            }

            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
            $catalog | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding utf8
        }

        function Invoke-TestPublicationPreflight {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot,
                [string]$Channel = 'stable',
                [string]$PublicCatalogUri = 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json',
                [string]$PackageUri = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.zip'
            )

            $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
            New-TestUpdaterCatalog -Path $catalogPath -Channel $Channel -PackageUri $PackageUri

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -CatalogPath $catalogPath `
                -Channel $Channel `
                -PublicCatalogUri $PublicCatalogUri `
                -ArtifactRoot $ArtifactRoot `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating updater catalog publication summary' {
        $artifactRoot = Join-Path $TestDrive 'updater-catalog-publication-preflight'

        $summary = Invoke-TestPublicationPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'updater-catalog-publication-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.catalog_publication | Should -Be 'not-published'
        $summary.channel | Should -Be 'stable'
        $summary.public_catalog_uri | Should -Be 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json'
    }

    It 'records the exact publication preflight check names' {
        $summary = Invoke-TestPublicationPreflight -ArtifactRoot (Join-Path $TestDrive 'updater-catalog-publication-checks')

        @($summary.publication_checks | ForEach-Object { $_.name }) | Should -Be @(
            'catalog-schema-v1',
            'selected-channel-present',
            'catalog-uri-https',
            'package-uri-https',
            'package-sha256-present',
            'public-claim-not-made',
            'publication-not-executed'
        )
    }

    It 'writes a publication preview catalog for the selected channel' {
        $artifactRoot = Join-Path $TestDrive 'updater-catalog-publication-preview'

        $summary = Invoke-TestPublicationPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.preview_catalog_path | Should -BeTrue
        $preview = Get-Content -Raw -LiteralPath $summary.preview_catalog_path | ConvertFrom-Json

        $preview.schema_version | Should -Be 1
        $preview.product | Should -Be 'PureCVisor Desktop Node'
        $preview.publication.public_trusted_signing | Should -Be 'not-claimed'
        $preview.publication.external_stable_publication | Should -Be 'not-claimed'
        $preview.publication.catalog_publication | Should -Be 'not-published'
        @($preview.channels).Count | Should -Be 1
        $preview.channels[0].name | Should -Be 'stable'
        $preview.channels[0].version | Should -Be '0.39.0'
        $preview.channels[0].package_uri | Should -Be 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.zip'
        $preview.channels[0].sha256 | Should -Be ('B' * 64)
    }

    It 'records the dry-run update command plan' {
        $summary = Invoke-TestPublicationPreflight -ArtifactRoot (Join-Path $TestDrive 'updater-catalog-publication-command')

        $summary.command_plan.update.update_catalog_uri | Should -Be 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json'
        $summary.command_plan.update.update_channel | Should -Be 'stable'
        $summary.command_plan.update.download_root | Should -Not -BeNullOrEmpty
        $summary.command_plan.update.dry_run_only | Should -BeTrue
        $summary.command_plan.update.actual_execution | Should -Be 'not-run'
    }

    It 'requires plan-only mode' {
        $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
        New-TestUpdaterCatalog -Path $catalogPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -CatalogPath $catalogPath `
            -Channel stable `
            -PublicCatalogUri 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json' `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects a non-HTTPS public catalog URI' {
        $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
        New-TestUpdaterCatalog -Path $catalogPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -CatalogPath $catalogPath `
            -Channel stable `
            -PublicCatalogUri 'file:///C:/tmp/catalog.json' `
            -ArtifactRoot (Join-Path $TestDrive 'non-https-catalog') `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects a non-HTTPS package URI for publication preview' {
        $catalogPath = Join-Path $TestDrive 'catalog\purecvisor-desktop-node.catalog.json'
        New-TestUpdaterCatalog -Path $catalogPath -PackageUri 'file:///C:/tmp/PureCVisorDesktopNode-0.39.0-windows-x64.zip'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -CatalogPath $catalogPath `
            -Channel stable `
            -PublicCatalogUri 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json' `
            -ArtifactRoot (Join-Path $TestDrive 'non-https-package') `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation or publication submission command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+submit|git\s+push|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove'
    }
}
