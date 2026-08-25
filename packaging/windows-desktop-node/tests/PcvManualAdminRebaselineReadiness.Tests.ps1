Set-StrictMode -Version Latest

Describe 'PcvManualAdminRebaselineReadiness contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1'
        $script:ReservationModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvManualAdminBaselineReservation.psm1'
        Import-Module $script:ReservationModulePath -Force

        function New-TestInstalledManifest {
            param(
                [Parameter(Mandatory)][string]$Path,
                [string]$Version = '0.41.2-admin-smoke'
            )

            $manifest = [ordered]@{
                schema_version = 1
                product = 'PureCVisor Desktop Node'
                version = $Version
                paths = [ordered]@{
                    source_root = 'D:\repo\artifacts\routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412\payload'
                    product_root = 'C:\Program Files\PureCVisor\DesktopNode'
                    manifest_path = 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json'
                }
            }

            New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
            $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding utf8
        }

        function New-TestRouteArtifact {
            param([Parameter(Mandatory)][string]$Path)

            New-Item -ItemType Directory -Force -Path (Join-Path $Path 'payload') | Out-Null
            Set-Content -LiteralPath (Join-Path $Path 'PureCVisorDesktopNode-0.41.2-admin-smoke-windows-x64.msi') -Value 'fake-msi' -Encoding utf8
            Set-Content -LiteralPath (Join-Path $Path 'PureCVisorDesktopNode-0.41.2-admin-smoke-windows-x64.msi.sha256') -Value 'ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0 *PureCVisorDesktopNode-0.41.2-admin-smoke-windows-x64.msi' -Encoding utf8
            Set-Content -LiteralPath (Join-Path $Path 'PureCVisorDesktopNode-0.41.2-admin-smoke-windows-x64.publication.json') -Value '{}' -Encoding utf8
        }

        function New-TestPackageArtifact {
            param(
                [Parameter(Mandatory)][string]$Path,
                [Parameter(Mandatory)][string]$Version
            )

            New-Item -ItemType Directory -Force -Path (Join-Path $Path 'payload') | Out-Null
            Set-Content -LiteralPath (Join-Path $Path "PureCVisorDesktopNode-$Version-windows-x64.msi") -Value "fake-msi-$Version" -Encoding utf8
            Set-Content -LiteralPath (Join-Path $Path "PureCVisorDesktopNode-$Version-windows-x64.msi.sha256") -Value 'ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0 *msi' -Encoding utf8
            Set-Content -LiteralPath (Join-Path $Path "PureCVisorDesktopNode-$Version-windows-x64.publication.json") -Value '{}' -Encoding utf8
        }

        function New-TestReservation {
            param(
                [Parameter(Mandatory)][string]$Path,
                [string]$CampaignId = 'c-04264-04265',
                [string]$BaselineVersion = '0.42.64-admin-smoke',
                [string]$TargetVersion = '0.42.65-admin-smoke',
                [string]$HostIdentity = 'host-secret',
                [datetimeoffset]$Now = [datetimeoffset]'2026-07-16T00:00:00Z',
                [datetimeoffset]$ExpiresAt = [datetimeoffset]'2026-07-18T00:00:00Z'
            )

            $record = New-PcvManualAdminBaselineReservationRecord `
                -CampaignId $CampaignId `
                -BaselineVersion $BaselineVersion `
                -TargetVersion $TargetVersion `
                -InstalledVersion $BaselineVersion `
                -HostIdentity $HostIdentity `
                -ReservationKind dedicated-host `
                -ResourceReference 'lab-host-a' `
                -Now $Now `
                -ExpiresAt $ExpiresAt
            New-Item -ItemType Directory -Path (Split-Path -Parent $Path) -Force | Out-Null
            $record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $Path -Encoding utf8
            $record
        }
        function Invoke-TestReadiness {
            param(
                [Parameter(Mandatory)][string]$ArtifactRoot,
                [string]$InstalledManifestPath = (Join-Path $TestDrive 'installed\product-manifest.json'),
                [string]$RouteArtifactRoot = (Join-Path $TestDrive 'route-0412')
            )

            New-TestInstalledManifest -Path $InstalledManifestPath
            New-TestRouteArtifact -Path $RouteArtifactRoot

            & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
                -ArtifactRoot $ArtifactRoot `
                -InstalledManifestPath $InstalledManifestPath `
                -RouteParityArtifactRoot $RouteArtifactRoot `
                -Version '0.41.2-admin-smoke' `
                -PlanOnly | Out-Null
            $LASTEXITCODE | Should -Be 0

            Get-Content -Raw -LiteralPath (Join-Path $ArtifactRoot 'summary.json') | ConvertFrom-Json
        }
    }

    It 'creates a non-mutating 0.41.2 manual admin rebaseline readiness summary' {
        $artifactRoot = Join-Path $TestDrive 'manual-admin-rebaseline'

        $summary = Invoke-TestReadiness -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        $summary.ok | Should -BeTrue
        $summary.scope | Should -Be 'manual-admin-rebaseline-readiness'
        $summary.plan_only | Should -BeTrue
        $summary.actual_execution | Should -Be 'not-run'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.version | Should -Be '0.41.2-admin-smoke'
        $summary.installed_version | Should -Be '0.41.2-admin-smoke'
        $summary.current_msi_present | Should -BeTrue
        $summary.current_payload_present | Should -BeTrue
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'not-claimed'
        $summary.reservation_status | Should -Be 'reservation-required-before-actual-execution'
        $summary.actual_execution_eligible | Should -BeFalse
    }

    It 'blocks historical destructive runner defaults until a current package or dedicated host is selected' {
        $summary = Invoke-TestReadiness -ArtifactRoot (Join-Path $TestDrive 'manual-admin-historical-defaults')

        $summary.rebaseline_required | Should -BeTrue
        $summary.installed_version_matches_requested | Should -BeTrue
        $summary.requested_version_status | Should -Be 'matches-installed-version'
        $summary.direct_historical_default_execution | Should -Be 'blocked'
        $summary.safe_execution_boundary | Should -Be 'current-version-rebaseline-or-dedicated-clean-host'
        @($summary.historical_runner_defaults | ForEach-Object { $_.runner }) | Should -Contain 'Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1'
        @($summary.historical_runner_defaults | ForEach-Object { $_.runner }) | Should -Contain 'Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1'
        @($summary.historical_runner_defaults | ForEach-Object { $_.runner }) | Should -Contain 'Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1'
        @($summary.historical_runner_defaults | Where-Object { $_.status -ne 'blocked-until-rebaseline' }).Count | Should -Be 0
    }

    It 'blocks a requested rebaseline version that is older than the installed manifest version' {
        $artifactRoot = Join-Path $TestDrive 'manual-admin-version-mismatch'
        $manifest = Join-Path $TestDrive 'installed-mismatch\product-manifest.json'
        $route = Join-Path $TestDrive 'route-0412-mismatch'
        New-TestInstalledManifest -Path $manifest -Version '0.41.5-admin-smoke'
        New-TestRouteArtifact -Path $route

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -InstalledManifestPath $manifest `
            -RouteParityArtifactRoot $route `
            -Version '0.41.2-admin-smoke' `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $summary.ok | Should -BeTrue
        $summary.installed_version | Should -Be '0.41.5-admin-smoke'
        $summary.version | Should -Be '0.41.2-admin-smoke'
        $summary.installed_version_matches_requested | Should -BeFalse
        $summary.requested_version_status | Should -Be 'blocked-by-installed-version-mismatch'
        ($summary.campaign_buckets | Where-Object id -eq 'credential-manager').status | Should -Be 'blocked-by-installed-version-mismatch'
        ($summary.campaign_buckets | Where-Object id -eq 'event-log').status | Should -Be 'blocked-by-installed-version-mismatch'
    }

    It 'writes a plan preview for Credential Manager, Event Log, Burn MSIX MSI, update rollback, and clean-host' {
        $artifactRoot = Join-Path $TestDrive 'manual-admin-plan'

        $summary = Invoke-TestReadiness -ArtifactRoot $artifactRoot

        Test-Path -LiteralPath $summary.plan_preview_path | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.plan_preview_path | ConvertFrom-Json
        @($plan.campaign_buckets | ForEach-Object { $_.id }) | Should -Be @(
            'credential-manager',
            'event-log',
            'burn-msix-msi',
            'update-rollback',
            'clean-host'
        )
        ($plan.campaign_buckets | Where-Object id -eq 'credential-manager').runner_command | Should -Match '-Version 0\.41\.2-admin-smoke'
        ($plan.campaign_buckets | Where-Object id -eq 'event-log').runner_command | Should -Match '-Version 0\.41\.2-admin-smoke'
        ($plan.campaign_buckets | Where-Object id -eq 'update-rollback').status | Should -Be 'requires-current-baseline-target-package-pair'
        ($plan.campaign_buckets | Where-Object id -eq 'clean-host').status | Should -Be 'requires-dedicated-host-current-package-pair'
    }

    It 'orchestrates an explicit baseline target package pair and rejects mixed input before host mutation' {
        $artifactRoot = Join-Path $TestDrive 'manual-admin-package-pair'
        $manifest = Join-Path $TestDrive 'installed-0423\product-manifest.json'
        $baselineRoute = Join-Path $TestDrive 'route-0423'
        $targetPackage = Join-Path $TestDrive 'target-0424'
        New-TestInstalledManifest -Path $manifest -Version '0.42.3-admin-smoke'
        New-TestPackageArtifact -Path $baselineRoute -Version '0.42.3-admin-smoke'
        New-TestPackageArtifact -Path $targetPackage -Version '0.42.4-admin-smoke'

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -InstalledManifestPath $manifest `
            -RouteParityArtifactRoot $baselineRoute `
            -TargetPackageArtifactRoot $targetPackage `
            -Version '0.42.3-admin-smoke' `
            -BaselineVersion '0.42.3-admin-smoke' `
            -TargetVersion '0.42.4-admin-smoke' `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $summary.package_pair_mode | Should -BeTrue
        $summary.baseline_version | Should -Be '0.42.3-admin-smoke'
        $summary.target_version | Should -Be '0.42.4-admin-smoke'
        $summary.package_pair_input_status | Should -Be 'ready-current-baseline-target-package-pair'
        $summary.mixed_version_input_policy | Should -Be 'reject-baseline-target-match-or-installed-baseline-mismatch'
        $summary.target_msi_present | Should -BeTrue
        $plan = Get-Content -Raw -LiteralPath $summary.plan_preview_path | ConvertFrom-Json
        ($plan.campaign_buckets | Where-Object id -eq 'credential-manager').runner_command | Should -Match '-Version 0\.42\.4-admin-smoke'
        ($plan.campaign_buckets | Where-Object id -eq 'event-log').runner_command | Should -Match '-Version 0\.42\.4-admin-smoke'
        ($plan.campaign_buckets | Where-Object id -eq 'update-rollback').target_version | Should -Be '0.42.4-admin-smoke'
        ($plan.campaign_buckets | Where-Object id -eq 'clean-host').target_version | Should -Be '0.42.4-admin-smoke'

        $blockedRoot = Join-Path $TestDrive 'manual-admin-package-pair-blocked'
        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $blockedRoot `
            -InstalledManifestPath $manifest `
            -RouteParityArtifactRoot $baselineRoute `
            -TargetPackageArtifactRoot $baselineRoute `
            -Version '0.42.3-admin-smoke' `
            -BaselineVersion '0.42.3-admin-smoke' `
            -TargetVersion '0.42.3-admin-smoke' `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $blocked = Get-Content -Raw -LiteralPath (Join-Path $blockedRoot 'summary.json') | ConvertFrom-Json
        $blocked.package_pair_input_status | Should -Be 'blocked-by-baseline-target-version-match'
        $blocked.host_mutation_performed | Should -BeFalse
    }

    It 'rejects missing mismatched expired and consumed reservations before execution eligibility' {
        Get-Command Test-PcvManualAdminBaselineReservation -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty
        $path = Join-Path $TestDrive 'reservation-validation/reservation.json'
        $record = New-TestReservation -Path $path
        $fingerprint = Get-PcvManualAdminBaselineSha256 -Value 'host-secret'
        $expected = @{
            ReservationPath = $path
            ExpectedCampaignId = 'c-04264-04265'
            ExpectedBaselineVersion = '0.42.64-admin-smoke'
            ExpectedTargetVersion = '0.42.65-admin-smoke'
            ExpectedHostFingerprint = $fingerprint
            Now = [datetimeoffset]'2026-07-16T12:00:00Z'
        }

        $missing = $expected.Clone()
        $missing.ReservationPath = Join-Path $TestDrive 'missing.json'
        { Test-PcvManualAdminBaselineReservation @missing } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_REQUIRED*'
        $campaign = $expected.Clone()
        $campaign.ExpectedCampaignId = 'different-campaign'
        { Test-PcvManualAdminBaselineReservation @campaign } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_MISMATCH*'
        $baseline = $expected.Clone()
        $baseline.ExpectedBaselineVersion = '0.42.63-admin-smoke'
        { Test-PcvManualAdminBaselineReservation @baseline } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_MISMATCH*'
        $target = $expected.Clone()
        $target.ExpectedTargetVersion = '0.42.66-admin-smoke'
        { Test-PcvManualAdminBaselineReservation @target } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_MISMATCH*'
        # $host is a read-only PowerShell automatic variable, so the splat uses its own name.
        $hostFingerprint = $expected.Clone()
        $hostFingerprint.ExpectedHostFingerprint = ('f' * 64)
        { Test-PcvManualAdminBaselineReservation @hostFingerprint } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_MISMATCH*'
        $expired = $expected.Clone()
        $expired.Now = [datetimeoffset]'2026-07-18T00:00:01Z'
        { Test-PcvManualAdminBaselineReservation @expired } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_EXPIRED*'

        $consumed = $record.PSObject.Copy()
        $consumed.status = 'consumed'
        $consumed | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $path -Encoding utf8
        { Test-PcvManualAdminBaselineReservation @expected } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_CONSUMED*'
    }

    It 'accepts a matching reservation for actual execution eligibility without mutation' {
        $artifactRoot = Join-Path $TestDrive 'actual-eligibility'
        $manifest = Join-Path $TestDrive 'installed-04264/product-manifest.json'
        $baseline = Join-Path $TestDrive 'route-04264'
        $target = Join-Path $TestDrive 'target-04265'
        $reservationPath = Join-Path $TestDrive 'reservation-04264-04265/reservation.json'
        New-TestInstalledManifest -Path $manifest -Version '0.42.64-admin-smoke'
        New-TestPackageArtifact -Path $baseline -Version '0.42.64-admin-smoke'
        New-TestPackageArtifact -Path $target -Version '0.42.65-admin-smoke'
        [void](New-TestReservation -Path $reservationPath)

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot $artifactRoot `
            -InstalledManifestPath $manifest `
            -RouteParityArtifactRoot $baseline `
            -TargetPackageArtifactRoot $target `
            -Version '0.42.64-admin-smoke' `
            -BaselineVersion '0.42.64-admin-smoke' `
            -TargetVersion '0.42.65-admin-smoke' `
            -CampaignId 'c-04264-04265' `
            -BaselineReservationPath $reservationPath `
            -HostIdentityOverride 'host-secret' `
            -Now ([datetimeoffset]'2026-07-16T12:00:00Z') `
            -ForActualExecution `
            -PlanOnly | Out-Null
        $LASTEXITCODE | Should -Be 0

        $summary = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'summary.json') | ConvertFrom-Json
        $summary.reservation_status | Should -Be 'reserved-and-matched'
        $summary.actual_execution_eligible | Should -BeTrue
        $summary.host_mutation_performed | Should -BeFalse
        $summary.actual_execution | Should -Be 'not-run'
    }

    It 'writes an immutable consumed sidecar and prevents reuse' {
        Get-Command Set-PcvManualAdminBaselineReservationState -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty
        $path = Join-Path $TestDrive 'reservation-consume/reservation.json'
        [void](New-TestReservation -Path $path)
        $before = Get-Content -Raw -LiteralPath $path

        $sidecar = Set-PcvManualAdminBaselineReservationState `
            -ReservationPath $path `
            -State consumed `
            -Now ([datetimeoffset]'2026-07-16T13:00:00Z') `
            -Reason 'target-installation-accepted'

        (Get-Content -Raw -LiteralPath $path) | Should -BeExactly $before
        $sidecar.status | Should -Be 'consumed'
        Test-Path -LiteralPath (Join-Path (Split-Path -Parent $path) 'reservation-consumed.json') |
            Should -BeTrue
        { Test-PcvManualAdminBaselineReservation `
                -ReservationPath $path `
                -ExpectedCampaignId 'c-04264-04265' `
                -ExpectedBaselineVersion '0.42.64-admin-smoke' `
                -ExpectedTargetVersion '0.42.65-admin-smoke' `
                -ExpectedHostFingerprint (Get-PcvManualAdminBaselineSha256 -Value 'host-secret') `
                -Now ([datetimeoffset]'2026-07-16T14:00:00Z') } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_CONSUMED*'
    }
    It 'requires plan-only mode' {
        $manifest = Join-Path $TestDrive 'installed\product-manifest.json'
        $route = Join-Path $TestDrive 'route-0412'
        New-TestInstalledManifest -Path $manifest
        New-TestRouteArtifact -Path $route

        & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:EntryPoint `
            -ArtifactRoot (Join-Path $TestDrive 'non-plan') `
            -InstalledManifestPath $manifest `
            -RouteParityArtifactRoot $route `
            -Version '0.41.2-admin-smoke' 2>$null | Out-Null

        $LASTEXITCODE | Should -Not -Be 0
    }

    It 'does not contain host mutation or installer execution command text' {
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint

        $scriptText | Should -Not -Match 'msiexec|Start-Service|Stop-Service|Restart-Service|sc\.exe|New-VM|Remove-VM|Add-AppxPackage|Remove-AppxPackage|New-NetFirewallRule|Remove-NetFirewallRule|trust-store-install|trust-store-remove|cmdkey|CredWrite|CredDelete'
    }
}
