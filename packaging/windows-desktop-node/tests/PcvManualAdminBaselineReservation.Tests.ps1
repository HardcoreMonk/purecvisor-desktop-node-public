Set-StrictMode -Version Latest

Describe 'manual admin baseline reservation' {
    BeforeAll {
        $script:ModulePath = Join-Path $PSScriptRoot '../tools/PcvManualAdminBaselineReservation.psm1'
        $script:EntryPoint = Join-Path $PSScriptRoot '../tools/New-PcvManualAdminBaselineReservation.ps1'
        if (Test-Path -LiteralPath $script:ModulePath -PathType Leaf) {
            Import-Module $script:ModulePath -Force
        }
    }

    It 'binds the campaign to the installed N-1 version without exposing host identity' {
        Get-Command New-PcvManualAdminBaselineReservationRecord -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty

        $record = New-PcvManualAdminBaselineReservationRecord `
            -CampaignId 'c-04264-04265' `
            -BaselineVersion '0.42.64-admin-smoke' `
            -TargetVersion '0.42.65-admin-smoke' `
            -InstalledVersion '0.42.64-admin-smoke' `
            -HostIdentity 'host-secret' `
            -ReservationKind dedicated-host `
            -ResourceReference 'lab-host-a' `
            -Now ([datetimeoffset]'2026-07-16T00:00:00Z') `
            -ExpiresAt ([datetimeoffset]'2026-07-18T00:00:00Z')

        $record.schema_version | Should -Be 1
        $record.contract | Should -Be 'pcv-manual-admin-baseline-reservation-v1'
        $record.status | Should -Be 'reserved'
        $record.host_fingerprint_sha256 | Should -Match '^[0-9a-f]{64}$'
        ([guid]$record.reservation_id).ToString() | Should -Be $record.reservation_id
        ($record | ConvertTo-Json -Depth 8) | Should -Not -Match 'host-secret'
    }

    It 'rejects installed version mismatch invalid order expiry and empty resource' {
        $base = @{
            CampaignId = 'c-04264-04265'
            BaselineVersion = '0.42.64-admin-smoke'
            TargetVersion = '0.42.65-admin-smoke'
            InstalledVersion = '0.42.64-admin-smoke'
            HostIdentity = 'host-secret'
            ReservationKind = 'dedicated-host'
            ResourceReference = 'lab-host-a'
            Now = [datetimeoffset]'2026-07-16T00:00:00Z'
            ExpiresAt = [datetimeoffset]'2026-07-18T00:00:00Z'
        }

        $mismatch = $base.Clone()
        $mismatch.InstalledVersion = '0.42.63-admin-smoke'
        { New-PcvManualAdminBaselineReservationRecord @mismatch } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_VERSION_MISMATCH*'
        $order = $base.Clone()
        $order.TargetVersion = '0.42.64-admin-smoke'
        { New-PcvManualAdminBaselineReservationRecord @order } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_VERSION_ORDER_INVALID*'
        $expiry = $base.Clone()
        $expiry.ExpiresAt = $base.Now
        { New-PcvManualAdminBaselineReservationRecord @expiry } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_EXPIRY_INVALID*'
        $resource = $base.Clone()
        $resource.ResourceReference = '   '
        { New-PcvManualAdminBaselineReservationRecord @resource } |
            Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESOURCE_INVALID*'
    }

    It 'creates no file in PlanOnly and atomically refuses overwrite in write mode' {
        $script:EntryPoint | Should -Exist
        $manifest = Join-Path $TestDrive 'installed/product-manifest.json'
        $artifactRoot = Join-Path $TestDrive 'reservation'
        New-Item -ItemType Directory -Path (Split-Path -Parent $manifest) -Force | Out-Null
        @{ schema_version = 1; version = '0.42.64-admin-smoke' } |
            ConvertTo-Json | Set-Content -LiteralPath $manifest -Encoding utf8

        & $script:EntryPoint `
            -CampaignId 'c-04264-04265' `
            -BaselineVersion '0.42.64-admin-smoke' `
            -TargetVersion '0.42.65-admin-smoke' `
            -ReservationKind dedicated-host `
            -ResourceReference 'lab-host-a' `
            -InstalledManifestPath $manifest `
            -ArtifactRoot $artifactRoot `
            -HostIdentityOverride 'host-secret' `
            -Now ([datetimeoffset]'2026-07-16T00:00:00Z') `
            -ExpiryHours 48 `
            -PlanOnly | Out-Null
        Test-Path -LiteralPath (Join-Path $artifactRoot 'reservation.json') | Should -BeFalse

        & $script:EntryPoint `
            -CampaignId 'c-04264-04265' `
            -BaselineVersion '0.42.64-admin-smoke' `
            -TargetVersion '0.42.65-admin-smoke' `
            -ReservationKind dedicated-host `
            -ResourceReference 'lab-host-a' `
            -InstalledManifestPath $manifest `
            -ArtifactRoot $artifactRoot `
            -HostIdentityOverride 'host-secret' `
            -Now ([datetimeoffset]'2026-07-16T00:00:00Z') `
            -ExpiryHours 48 | Out-Null
        Test-Path -LiteralPath (Join-Path $artifactRoot 'reservation.json') | Should -BeTrue

        { & $script:EntryPoint `
                -CampaignId 'c-04264-04265' `
                -BaselineVersion '0.42.64-admin-smoke' `
                -TargetVersion '0.42.65-admin-smoke' `
                -ReservationKind dedicated-host `
                -ResourceReference 'lab-host-a' `
                -InstalledManifestPath $manifest `
                -ArtifactRoot $artifactRoot `
                -HostIdentityOverride 'host-secret' `
                -Now ([datetimeoffset]'2026-07-16T00:00:00Z') `
                -ExpiryHours 48 } | Should -Throw '*PCV_MANUAL_ADMIN_BASELINE_RESERVATION_EXISTS*'
    }
}
