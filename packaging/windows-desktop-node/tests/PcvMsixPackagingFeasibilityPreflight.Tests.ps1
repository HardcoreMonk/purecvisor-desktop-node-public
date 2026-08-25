Set-StrictMode -Version Latest

Describe 'PcvMsixPackagingFeasibilityPreflight contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvMsixPackagingFeasibilityPreflight.ps1'

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
                    msi_sha256 = ('D' * 64)
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

        function Invoke-TestMsixFeasibility {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot
            )

            $descriptorPath = Join-Path $TestDrive 'publication\PureCVisorDesktopNode-0.39.0-windows-x64.publication.json'
            New-TestPublicationDescriptor -Path $descriptorPath

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -PublicationDescriptorPath $descriptorPath `
                -ArtifactRoot $ArtifactRoot `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating MSIX feasibility summary' {
        $artifactRoot = Join-Path $TestDrive 'msix-packaging-feasibility'

        $summary = Invoke-TestMsixFeasibility -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'msix-packaging-feasibility-preflight'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.msix | Should -Be 'feasibility-blocked-by-service-packaging-design'
    }

    It 'records the exact MSIX feasibility check names' {
        $summary = Invoke-TestMsixFeasibility -ArtifactRoot (Join-Path $TestDrive 'msix-packaging-checks')

        @($summary.msix_checks | ForEach-Object { $_.name }) | Should -Be @(
            'publication-descriptor-schema-v1',
            'package-identity-preview-written',
            'service-packaging-design-required',
            'install-update-remove-evidence-required',
            'capability-boundary-required',
            'public-claim-not-made',
            'msix-build-not-executed'
        )
    }

    It 'writes an MSIX package manifest preview without building a package' {
        $artifactRoot = Join-Path $TestDrive 'msix-packaging-preview'

        $summary = Invoke-TestMsixFeasibility -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.package_manifest_preview_path | Should -BeTrue
        $preview = Get-Content -Raw -LiteralPath $summary.package_manifest_preview_path

        $preview | Should -Match '<Package '
        $preview | Should -Match 'IgnorableNamespaces="uap rescap"'
        $preview | Should -Match '<Identity '
        $preview | Should -Match 'Name="PureCVisor.DesktopNode"'
        $preview | Should -Match 'Publisher="CN=PureCVisor"'
        $preview | Should -Match 'Version="0\.39\.0\.0"'
        $preview | Should -Match '<DisplayName>PureCVisor Desktop Node</DisplayName>'
        $summary.command_plan.msix.build_status | Should -Be 'not-run'
        $summary.command_plan.msix.output | Should -Be 'not-built'
    }

    It 'records service packaging blockers before any MSIX pass claim' {
        $summary = Invoke-TestMsixFeasibility -ArtifactRoot (Join-Path $TestDrive 'msix-packaging-blockers')

        $summary.feasibility.status | Should -Be 'blocked-by-service-packaging-design'
        @($summary.feasibility.required_before_pass) | Should -Be @(
            'service-install-start-stop-design',
            'appxmanifest-capability-boundary',
            'install-update-remove-evidence',
            'public-signing-decision'
        )
        $summary.feasibility.msix_build | Should -Be 'not-run'
        $summary.feasibility.host_mutation | Should -Be 'not-run'
    }

    It 'requires plan-only mode' {
        $descriptorPath = Join-Path $TestDrive 'publication\PureCVisorDesktopNode-0.39.0-windows-x64.publication.json'
        New-TestPublicationDescriptor -Path $descriptorPath

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -PublicationDescriptorPath $descriptorPath `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation or MSIX build command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'Restart-Computer|Stop-Computer|shutdown\.exe|schtasks\.exe|winget\s+submit|git\s+push|gh\s+pr\s+create|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|Add-AppxPackage|Remove-AppxPackage|makeappx|signtool'
    }
}
