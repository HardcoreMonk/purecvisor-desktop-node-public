Set-StrictMode -Version Latest

Describe 'PcvBurnBootstrapperPreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvBurnBootstrapperPreflight.ps1'

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
                    msi_sha256 = ('C' * 64)
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

        function Invoke-TestBurnPreflight {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot,
                [string]$MsiUrl = 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi'
            )

            $descriptorPath = Join-Path $TestDrive 'publication\PureCVisorDesktopNode-0.39.0-windows-x64.publication.json'
            New-TestPublicationDescriptor -Path $descriptorPath

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -PublicationDescriptorPath $descriptorPath `
                -ArtifactRoot $ArtifactRoot `
                -MsiUrl $MsiUrl `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating Burn bootstrapper preflight summary' {
        $artifactRoot = Join-Path $TestDrive 'burn-bootstrapper-preflight'

        $summary = Invoke-TestBurnPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'burn-bootstrapper-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.burn_bootstrapper | Should -Be 'not-built'
    }

    It 'records the exact Burn preflight check names' {
        $summary = Invoke-TestBurnPreflight -ArtifactRoot (Join-Path $TestDrive 'burn-bootstrapper-checks')

        @($summary.burn_checks | ForEach-Object { $_.name }) | Should -Be @(
            'publication-descriptor-schema-v1',
            'msi-url-https',
            'msi-sha256-present',
            'bundle-upgrade-code-valid',
            'wix-burn-authoring-preview-written',
            'public-claim-not-made',
            'bundle-build-not-executed'
        )
    }

    It 'writes a WiX Burn authoring preview without building a bundle' {
        $artifactRoot = Join-Path $TestDrive 'burn-bootstrapper-preview'

        $summary = Invoke-TestBurnPreflight -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.bundle_authoring_preview_path | Should -BeTrue
        $preview = Get-Content -Raw -LiteralPath $summary.bundle_authoring_preview_path

        $preview | Should -Match '<Wix '
        $preview | Should -Match '<Bundle '
        $preview | Should -Match 'Name="PureCVisor Desktop Node"'
        $preview | Should -Match 'Version="0\.39\.0"'
        $preview | Should -Match '<MsiPackage '
        $preview | Should -Match 'DownloadUrl="https://downloads\.example\.invalid/PureCVisorDesktopNode-0\.39\.0-windows-x64\.msi"'
        $preview | Should -Match 'Compressed="no"'
        $summary.command_plan.wix.build_status | Should -Be 'not-run'
        $summary.command_plan.wix.output | Should -Be 'not-built'
    }

    It 'plans the Burn build with the canonical WiX 5 bootstrapper extension id' {
        $summary = Invoke-TestBurnPreflight -ArtifactRoot (Join-Path $TestDrive 'burn-bootstrapper-extension')

        # WiX 5 renamed WixToolset.Bal.wixext to WixToolset.BootstrapperApplications.wixext. The old
        # id only resolves to a shim whose payload is the renamed dll, which WiX reports as damaged.
        $summary.command_plan.wix.build_command | Should -Match '-ext\s+WixToolset\.BootstrapperApplications\.wixext'
        $summary.command_plan.wix.build_command | Should -Not -Match 'WixToolset\.Bal\.wixext'
    }

    It 'records MSI chain hash binding and lifecycle evidence still required' {
        $summary = Invoke-TestBurnPreflight -ArtifactRoot (Join-Path $TestDrive 'burn-bootstrapper-chain')

        $summary.chain.msi_url | Should -Be 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi'
        $summary.chain.msi_sha256 | Should -Be ('C' * 64)
        $summary.chain.package_version | Should -Be '0.39.0'
        $summary.chain.repair_remove_evidence | Should -Be 'required-before-pass'
        $summary.chain.payload_hash_binding | Should -Be 'preview-only'
    }

    It 'requires plan-only mode' {
        $descriptorPath = Join-Path $TestDrive 'publication\PureCVisorDesktopNode-0.39.0-windows-x64.publication.json'
        New-TestPublicationDescriptor -Path $descriptorPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -PublicationDescriptorPath $descriptorPath `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -MsiUrl 'https://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'rejects non-HTTPS MSI URLs for public bootstrapper preview' {
        $descriptorPath = Join-Path $TestDrive 'publication\PureCVisorDesktopNode-0.39.0-windows-x64.publication.json'
        New-TestPublicationDescriptor -Path $descriptorPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -PublicationDescriptorPath $descriptorPath `
            -ArtifactRoot (Join-Path $TestDrive 'non-https-msi') `
            -MsiUrl 'http://downloads.example.invalid/PureCVisorDesktopNode-0.39.0-windows-x64.msi' `
            -PlanOnly 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation or publication submission command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+submit|git\s+push|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove'
    }
}
