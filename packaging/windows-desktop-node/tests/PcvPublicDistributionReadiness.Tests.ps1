Set-StrictMode -Version Latest

Describe 'PcvPublicDistributionReadiness preflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvPublicDistributionReadiness.ps1'

        function New-TestPublicationDescriptor {
            param(
                [Parameter(Mandatory)][string]$Path,
                [string]$Version = '0.39.0'
            )

            $descriptor = [ordered]@{
                schema_version = '1'
                product = [ordered]@{
                    name = 'PureCVisor Desktop Node'
                    version = $Version
                    release_channel = 'stable'
                }
                artifact = [ordered]@{
                    base_name = "PureCVisorDesktopNode-$Version-windows-x64"
                    msi_path = "D:\artifacts\PureCVisorDesktopNode-$Version-windows-x64.msi"
                    msi_sha256 = ('A' * 64)
                    signing_mode = 'RequireSigned'
                    signing_trust_model = 'PublicTrustedCandidate'
                }
                publication = [ordered]@{
                    mode = 'internal-artifact-descriptor-only'
                    public_trusted_signing = 'not-claimed'
                    external_stable_publication = 'not-claimed'
                    burn_bootstrapper = 'not-built'
                    msix = 'not-built'
                    winget_manifest = 'not-generated'
                    catalog_publication = 'not-published'
                }
            }

            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
            $descriptor | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding utf8
        }

        function Invoke-TestReadiness {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot
            )

            $descriptorPath = Join-Path $TestDrive 'publication\PureCVisorDesktopNode-0.39.0-windows-x64.publication.json'
            New-TestPublicationDescriptor -Path $descriptorPath

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -PublicationDescriptorPath $descriptorPath `
                -ArtifactRoot $ArtifactRoot `
                -InstallerUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi' `
                -InstallerSha256 ('A' * 64) `
                -SigningProvider AzureArtifactSigning `
                -ReleaseApproval 'approved-for-dry-run-readiness-only' `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0
            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating public distribution readiness summary' {
        $artifactRoot = Join-Path $TestDrive 'public-distribution-readiness'

        $summary = Invoke-TestReadiness -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'public-distribution-readiness-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
    }

    It 'records the exact readiness gate names' {
        $summary = Invoke-TestReadiness -ArtifactRoot (Join-Path $TestDrive 'public-distribution-readiness-gates')

        @($summary.gates | ForEach-Object { $_.name }) | Should -Be @(
            'public-signing-inputs',
            'winget-manifest-preview',
            'winget-validation-command',
            'winget-submission-plan',
            'msix-service-packaging-feasibility',
            'public-publication-blocker'
        )
    }

    It 'writes a winget singleton manifest preview with required package fields' {
        $artifactRoot = Join-Path $TestDrive 'public-distribution-readiness-winget'
        $summary = Invoke-TestReadiness -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.winget_manifest_preview_path | Should -BeTrue
        $manifest = Get-Content -Raw -LiteralPath $summary.winget_manifest_preview_path

        $manifest | Should -Match '# yaml-language-server: \$schema=https://aka\.ms/winget-manifest\.singleton\.1\.12\.0\.schema\.json'
        $manifest | Should -Match 'PackageIdentifier: PureCVisor\.DesktopNode'
        $manifest | Should -Match 'PackageVersion: 0\.39\.0'
        $manifest | Should -Match 'PackageLocale: en-US'
        $manifest | Should -Match 'Publisher: PureCVisor'
        $manifest | Should -Match 'PackageName: PureCVisor Desktop Node'
        $manifest | Should -Match 'License: Proprietary'
        $manifest | Should -Match 'ShortDescription: Local Windows Desktop Node management service\.'
        $manifest | Should -Match 'InstallerType: msi'
        $manifest | Should -Match 'InstallerUrl: https://downloads\.example\.invalid/PureCVisorDesktopNode-0\.39\.0-windows-x64\.msi'
        $manifest | Should -Match ('InstallerSha256: ' + ('A' * 64))
        $manifest | Should -Match 'ManifestType: singleton'
        $manifest | Should -Match 'ManifestVersion: 1\.12\.0'
    }

    It 'keeps winget validation and submission as explicit manual follow-up' {
        $summary = Invoke-TestReadiness -ArtifactRoot (Join-Path $TestDrive 'public-distribution-readiness-validation')

        $summary.command_plan.winget.validate_command | Should -Be 'winget validate <manifest-preview-folder>'
        $summary.command_plan.winget.submission | Should -Be 'not-submitted'
        $summary.command_plan.winget.repository | Should -Be 'https://github.com/microsoft/winget-pkgs'
    }

    It 'requires plan-only mode' {
        $descriptorPath = Join-Path $TestDrive 'publication\PureCVisorDesktopNode-0.39.0-windows-x64.publication.json'
        New-TestPublicationDescriptor -Path $descriptorPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -PublicationDescriptorPath $descriptorPath `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -InstallerUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi' `
            -InstallerSha256 ('A' * 64) 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation or publication submission command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+submit|git\s+push|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove'
    }
}
