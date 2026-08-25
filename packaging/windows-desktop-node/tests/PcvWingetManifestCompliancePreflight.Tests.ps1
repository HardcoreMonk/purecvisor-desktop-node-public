Set-StrictMode -Version Latest

Describe 'PcvWingetManifestCompliancePreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvWingetManifestCompliancePreflight.ps1'

        function New-TestWingetManifest {
            param(
                [Parameter(Mandatory)][string]$Path,
                [string]$InstallerUrl = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi',
                [string]$InstallerSha256 = ('D' * 64)
            )

            $lines = @(
                'PackageIdentifier: PureCVisor.DesktopNode',
                'PackageVersion: 0.39.0',
                'PackageLocale: en-US',
                'Publisher: PureCVisor',
                'PackageName: PureCVisor Desktop Node',
                'License: Proprietary',
                'ShortDescription: Local Windows Desktop Node management service.',
                'Installers:',
                '  - Architecture: x64',
                '    InstallerType: msi',
                "    InstallerUrl: $InstallerUrl",
                "    InstallerSha256: $InstallerSha256",
                'ManifestType: singleton',
                'ManifestVersion: 1.12.0'
            )

            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
            Set-Content -LiteralPath $Path -Value $lines -Encoding utf8
        }

        function Invoke-TestWingetCompliance {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot,
                [string]$InstallerUrl = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi',
                [string]$InstallerSha256 = ('D' * 64)
            )

            $manifestPath = Join-Path $TestDrive 'winget\PureCVisor.DesktopNode.yaml'
            New-TestWingetManifest -Path $manifestPath -InstallerUrl $InstallerUrl -InstallerSha256 $InstallerSha256

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ManifestPath $manifestPath `
                -ArtifactRoot $ArtifactRoot `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating winget manifest compliance summary' {
        $artifactRoot = Join-Path $TestDrive 'winget-manifest-compliance-preflight'

        $summary = Invoke-TestWingetCompliance -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'winget-manifest-compliance-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.winget_submission | Should -Be 'not-submitted'
        $summary.validation_status | Should -Be 'offline-compliance-pass'
    }

    It 'records the exact winget compliance check names' {
        $summary = Invoke-TestWingetCompliance -ArtifactRoot (Join-Path $TestDrive 'winget-compliance-checks')

        @($summary.compliance_checks | ForEach-Object { $_.name }) | Should -Be @(
            'manifest-file-present',
            'singleton-manifest-type',
            'manifest-version-supported',
            'package-identifier-present',
            'package-version-winget-compatible',
            'installer-url-https',
            'installer-sha256-valid',
            'installer-type-msi',
            'winget-cli-validation-not-executed',
            'winget-submission-not-executed',
            'public-claim-not-made'
        )
    }

    It 'writes normalized manifest metadata without running winget validation' {
        $artifactRoot = Join-Path $TestDrive 'winget-compliance-normalized'

        $summary = Invoke-TestWingetCompliance -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.normalized_manifest_path | Should -BeTrue
        $normalized = Get-Content -Raw -LiteralPath $summary.normalized_manifest_path | ConvertFrom-Json

        $normalized.package_identifier | Should -Be 'PureCVisor.DesktopNode'
        $normalized.package_version | Should -Be '0.39.0'
        $normalized.manifest_type | Should -Be 'singleton'
        $normalized.manifest_version | Should -Be '1.12.0'
        $normalized.installer.architecture | Should -Be 'x64'
        $normalized.installer.installer_type | Should -Be 'msi'
        $normalized.installer.installer_url | Should -Be 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi'
        $normalized.installer.installer_sha256 | Should -Be ('D' * 64)
        $summary.command_plan.winget_cli.validation_status | Should -Be 'not-run'
        $summary.command_plan.winget_cli.submission_status | Should -Be 'not-submitted'
    }

    It 'requires plan-only mode' {
        $manifestPath = Join-Path $TestDrive 'winget\PureCVisor.DesktopNode.yaml'
        New-TestWingetManifest -Path $manifestPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ManifestPath $manifestPath `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects a non-HTTPS installer URL' {
        $manifestPath = Join-Path $TestDrive 'winget\PureCVisor.DesktopNode.yaml'
        New-TestWingetManifest -Path $manifestPath -InstallerUrl 'http://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ManifestPath $manifestPath `
            -ArtifactRoot (Join-Path $TestDrive 'non-https') `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects an invalid installer SHA-256' {
        $manifestPath = Join-Path $TestDrive 'winget\PureCVisor.DesktopNode.yaml'
        New-TestWingetManifest -Path $manifestPath -InstallerSha256 'not-a-sha'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ManifestPath $manifestPath `
            -ArtifactRoot (Join-Path $TestDrive 'bad-sha') `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation or winget CLI execution command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+(validate|submit|install|upgrade)|git\s+push|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove'
    }
}
