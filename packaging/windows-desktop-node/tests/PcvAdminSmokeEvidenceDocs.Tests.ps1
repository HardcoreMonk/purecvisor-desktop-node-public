Set-StrictMode -Version Latest

Describe 'Admin smoke evidence documentation' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path

        function Get-RepoText {
            param([Parameter(Mandatory)] [string] $RelativePath)

            Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot $RelativePath)
        }

        # C# 소스는 이것으로 읽는다. 아래 단언들은 "이 심볼이 이 계약에 존재한다"를 확인하려는
        # 것이지 "이 심볼이 이 파일에 있다"를 확인하려는 것이 아니다. 그런데 Get-RepoText 는 파일
        # 하나만 읽으므로, 타입을 partial 로 쪼개 심볼이 X.cs 에서 X.<Domain>.cs 로 옮겨가면 단언이
        # 계약 위반이 아니라 파일 배치 변화만으로 실패한다.
        #
        # 2026-08-09 에 실제로 그렇게 깨졌다. BatchEvidenceSummaryReader 를 partial 로 분해하자
        # batch_evidence_artifact 와 InferDescriptorBatchIdFromCampaignPath 단언이 red 가 됐고,
        # 원본에는 using 줄만 남아 있었다. 심볼은 사라지지 않았고 자리만 옮겼다.
        #
        # 그래서 파일이 아니라 partial 집합 전체를 본문으로 삼는다. 심볼이 정말 삭제되면 여전히
        # 실패하므로 단언이 약해지지 않는다.
        function Get-RepoSourceText {
            param([Parameter(Mandatory)] [string] $RelativePath)

            $full = Join-Path $script:RepoRoot $RelativePath
            $directory = Split-Path -Parent $full
            $stem = [System.IO.Path]::GetFileNameWithoutExtension($full)
            $extension = [System.IO.Path]::GetExtension($full)

            $parts = @(Get-ChildItem -LiteralPath $directory -Filter "$stem*$extension" -File |
                Sort-Object -Property Name)
            if ($parts.Count -eq 0) {
                throw "Get-RepoSourceText found no file for $RelativePath"
            }

            return ($parts | ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }) -join "`n"
        }
    }

    It 'records ADR-0006 internal private network distribution boundary and closes public distribution candidate' {
        $adr0005 = Get-RepoText -RelativePath 'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        $adr0006 = Get-RepoText -RelativePath 'docs/adr/0006-internal-private-network-distribution.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $internalMatrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $boundaryEvidence = Get-RepoText -RelativePath 'docs/ga-ready/evidence/internal-private-network-boundary-2026-05-10.md'
        $cleanHostEvidence = Get-RepoText -RelativePath 'docs/ga-ready/evidence/internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md'

        $adr0005 | Should -Match '상태:\s*미채택/종료'
        $adr0005 | Should -Match 'closed-not-adopted'
        $adr0006 | Should -Match '상태:\s*적용'
        $adr0006 | Should -Match 'internal-private-network-only'
        $adr0006 | Should -Match 'public trusted signing.*out-of-scope|Public trusted signing.*out-of-scope'

        $publicMatrix | Should -Match 'status:\s*closed-not-adopted'
        $publicMatrix | Should -Match 'superseding_matrix:\s*docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix | Should -Match 'public_trusted_signing:\s*out-of-scope'
        $publicMatrix | Should -Match 'winget_submission:\s*out-of-scope'
        $publicMatrix | Should -Match 'external_stable_publication:\s*out-of-scope'
        $publicMatrix | Should -Match 'clean_host_public_signed_install_update_rollback_smoke:\s*out-of-scope'

        $internalMatrix | Should -Match 'matrix_id:\s*INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX'
        $internalMatrix | Should -Match 'decision_marker:\s*internal-private-network-only'
        $internalMatrix | Should -Match 'public_trusted_signing:\s*out-of-scope'
        $internalMatrix | Should -Match 'winget_submission:\s*out-of-scope'
        $internalMatrix | Should -Match 'external_stable_publication:\s*out-of-scope'
        $internalMatrix | Should -Match 'clean_host_public_signed_install_update_rollback_smoke:\s*out-of-scope'
        $internalMatrix | Should -Match 'internal_signed_msi_status:\s*pass'
        $internalMatrix | Should -Match 'internal_updater_catalog_channel:\s*code-level-pass'
        $internalMatrix | Should -Match 'private_lan_smoke:\s*pass'
        $internalMatrix | Should -Match 'internal_update_rollback_smoke:\s*installed-destructive-pass'
        $internalMatrix | Should -Match 'internal_https_tls_lifecycle_installed_smoke:\s*pass'
        $internalMatrix | Should -Match 'internal_clean_host_install_update_rollback_smoke:\s*pass'
        $internalMatrix | Should -Match 'internal-https-tls-lifecycle-installed-2026-05-10-0397'
        $internalMatrix | Should -Match 'internal-clean-host-install-update-rollback-smoke-2026-05-10-0417'

        $boundaryEvidence | Should -Match 'actual_execution:\s*docs-only-boundary-reclassification'
        $boundaryEvidence | Should -Match 'host_mutation_performed:\s*false'
        $boundaryEvidence | Should -Match 'public_distribution_candidate:\s*closed-not-adopted'
        $boundaryEvidence | Should -Match 'internal_clean_host_install_update_rollback_smoke:\s*pass'
        $boundaryEvidence | Should -Match 'internal-clean-host-install-update-rollback-smoke-2026-05-10-0417'

        $cleanHostEvidence | Should -Match 'actual_execution:\s*hyper-v-dedicated-clean-host-installed-smoke'
        $cleanHostEvidence | Should -Match 'host_mutation_performed:\s*true'
        $cleanHostEvidence | Should -Match 'guest_product_mutation_performed:\s*true'
        $cleanHostEvidence | Should -Match 'internal_clean_host_install_update_rollback_smoke:\s*pass'
        $cleanHostEvidence | Should -Match '0\.39\.6-admin-smoke'
        $cleanHostEvidence | Should -Match '0\.39\.7-admin-smoke'
        $cleanHostEvidence | Should -Match '9b266867129cbf07abb8da7e2a26799d1221a16d955348505416810c48de12b1'
        $cleanHostEvidence | Should -Match '983d1eb64329928b69765a662605d29c3d2aaaa39d1a5857f990e5519438f91a'
        $cleanHostEvidence | Should -Match '1807d61f9d953c978cf382b5f447c02ebc6a12fbbecbc54c58c30f472084d40e'
        $cleanHostEvidence | Should -Match 'e9ff332ad2a0e33e6d6ae09b80d42fa961849494'
        $cleanHostEvidence | Should -Match 'install_exit_code:\s*0'
        $cleanHostEvidence | Should -Match 'update_exit_code:\s*0'
        $cleanHostEvidence | Should -Match 'rollback_exit_code:\s*0'
        $cleanHostEvidence | Should -Match 'final_web_status_code:\s*200'
        $cleanHostEvidence | Should -Match 'KB5082142'
        $cleanHostEvidence | Should -Match 'public_release:\s*not-claimed'

        foreach ($path in @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/USER_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'internal-private-network-only|INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX|ADR-0006'
            $content | Should -Match 'out-of-scope'
        }
    }

    It 'records installed account login smoke runner and noVNC bridge code-level evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md'
        $smokeRunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $verificationPolicy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $verificationOwnership = Get-RepoText -RelativePath 'docs/ga-ready/VERIFICATION_OWNERSHIP.md'
        $routeMatrix = Get-RepoText -RelativePath 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'

        $evidencePath | Should -Exist
        $smokeRunnerPath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match 'installed_account_login_smoke_runner:\s*packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke\.ps1'
        $evidence | Should -Match 'installed_account_login_smoke_execution:\s*installed-admin-smoke-pass'
        $evidence | Should -Match 'installed_account_login_smoke_artifact_root:\s*artifacts/installed-account-login-smoke-20260510-0410-final'
        $evidence | Should -Match 'novnc_bridge:\s*code-level-websocket-to-vnc-tcp-pass'
        $evidence | Should -Match 'novnc_websocket_path_template:\s*/api/v1/console/novnc/\{vm_id\}'
        $evidence | Should -Match 'host_mutation_performed:\s*true'
        $evidence | Should -Match 'token_value_observed:\s*false'
        $evidence | Should -Match 'password_value_observed:\s*false'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $matrix | Should -Match 'installed_account_login_smoke_runner:\s*installed-admin-smoke-runner'
        $matrix | Should -Match 'installed_account_login_smoke_execution:\s*installed-admin-smoke-pass'
        $matrix | Should -Match 'novnc_websocket_bridge:\s*code-level-pass'
        $matrix | Should -Match 'installed-account-login-novnc-bridge-code-level-2026-05-10'

        $ledger | Should -Match 'Evidence Group: Installed Account Login And noVNC Bridge Code-Level 2026-05-10'
        $ledger | Should -Match 'noVNC WebSocket-to-VNC TCP bridge code-level PASS'
        $ledger | Should -Match 'installed_account_login_smoke_execution=installed-admin-smoke-pass'

        $operationsGuide | Should -Match 'Invoke-PcvInstalledAccountLoginSmoke\.ps1'
        $operationsGuide | Should -Match '--novnc-target-host'
        $operationsGuide | Should -Match 'token/password/refresh token value'
        $packagingReadme | Should -Match 'Installed account login/noVNC bridge follow-up evidence'
        $packagingReadme | Should -Match 'WebSocket-to-VNC TCP bridge'
        $agents | Should -Match 'installed-account-login-novnc-bridge-code-level-2026-05-10'
        $adrIndex | Should -Match 'opt-in noVNC WebSocket-to-VNC TCP bridge code-level PASS'
        $releaseBoundary | Should -Match 'Installed account login artifact.*installed-account-login-smoke-20260510-0410-final'
        $verificationPolicy | Should -Match 'Account/RBAC/JWT/console/noVNC'
        $verificationOwnership | Should -Match 'Host xUnit loopback TCP/WebSocket test'
        $routeMatrix | Should -Match 'noVNC disabled until explicit target host/port'
    }

    It 'records frontend backend auth console live smoke and post 04210 deferred package execution' {
        $liveSmokePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md'
        $postFollowupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04210-followup-execution-2026-05-13.md'

        $liveSmokePath | Should -Exist
        $postFollowupPath | Should -Exist

        $liveSmoke = Get-Content -Raw -LiteralPath $liveSmokePath
        $postFollowup = Get-Content -Raw -LiteralPath $postFollowupPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $verificationPolicy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'

        foreach ($content in @($liveSmoke, $evidenceIndex, $controlPlaneIndex, $matrix, $ledger, $classification, $operationsGuide, $developerIndex, $adrIndex, $readme, $agents, $packagingReadme, $verificationPolicy, $releaseBoundary)) {
            $content | Should -Match 'frontend-backend-auth-console-live-smoke-2026-05-10'
            $content | Should -Match 'installed-account-login-browser-live-smoke-20260510-235543'
            $content | Should -Match 'web-console-installed-listener-browser-live-smoke-20260510-235543'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        $liveSmoke | Should -Match 'frontend-backend-auth-console-live-smoke-2026-05-10-235543'
        $liveSmoke | Should -Match 'route_coverage_metadata:\s*auth_logout-added'
        $liveSmoke | Should -Match 'api_handler_adapter_contract:\s*auth-console-routes-added'
        $liveSmoke | Should -Match 'installed_listener_execution:\s*installed-listener-browser-live-smoke-pass'
        $liveSmoke | Should -Match 'artifacts/installed-web-asset-refresh-20260510-235258'
        $liveSmoke | Should -Match '065b724b1a5e75bc87a491c6c0ca0d349a35cb2b8a90eb90ab9563d5edecf9e4'
        $liveSmoke | Should -Match '53c2cd53248cb57d586c50092ead1791ced3089912005f4f525be0b4d8c82bc9'
        $liveSmoke | Should -Match '7073e8b67d87f77987b7d776f8528e5a9e65d041240711a4f13b5cd4744e05de'
        $liveSmoke | Should -Match 'da2d25577f7058116f4e410592e6bd59bacefd1090cc3b661ca588481c45f2fa'
        $liveSmoke | Should -Match 'host_mutation_performed:\s*true'
        $liveSmoke | Should -Match 'token_value_observed:\s*false'
        $liveSmoke | Should -Match 'password_value_observed:\s*false'
        $liveSmoke | Should -Match 'POST /api/v1/auth/logout'
        $liveSmoke | Should -Match 'GET /api/v1/vms/\{vmId\}/console'

        foreach ($content in @($postFollowup, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor)) {
            $content | Should -Match 'post-04210-followup-execution-2026-05-13'
            $content | Should -Match '371e05055c7488f923c0038f87f1a1288054c271'
            $content | Should -Match '0\.42\.12-admin-smoke'
            $content | Should -Match 'deferred-until-next-product-payload-change'
            $content | Should -Match 'not-run-no-new-product-payload'
        }

        $postFollowup | Should -Match 'product_payload_change_detected:\s*false'
        $postFollowup | Should -Match 'latest_product_payload_provenance_commit:\s*987beb51025a5aa926df7d9a905019b4d6d29705'
        $postFollowup | Should -Match 'host_mutation_performed:\s*false'
        $postFollowup | Should -Match 'public_release:\s*not-claimed'
        $matrix | Should -Match 'frontend_backend_auth_console_live_smoke:\s*pass'
        $ledger | Should -Match 'Evidence Group: Frontend/Backend Auth Console Live Smoke 2026-05-10 235543'
        $ledger | Should -Match 'installed-listener-browser-live-smoke-pass'
        $ledger | Should -Match '7073e8b67d87f77987b7d776f8528e5a9e65d041240711a4f13b5cd4744e05de'
        $matrix | Should -Match 'post_04210_followup_product_payload_change_detected:\s*false'
        $descriptor | Should -Match 'post_04210_followup_package_build_decision:\s*`deferred-until-next-product-payload-change`'
    }

    It 'preserves historical target-backed noVNC and TUI evidence without restoring the removed TUI product' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md'
        $noVncRunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1'
        $tuiRunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $verificationOwnership = Get-RepoText -RelativePath 'docs/ga-ready/VERIFICATION_OWNERSHIP.md'

        $evidencePath | Should -Exist
        $noVncRunnerPath | Should -Exist
        $tuiRunnerPath | Should -Not -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match '0\.41\.1-admin-smoke'
        $evidence | Should -Match 'a3226ef637ea895d2f2a9956599e0d5e79d00410'
        $evidence | Should -Match '0583f71c4fcc1ed0da886e55f2fbac6713d8bc731fad7d33d6c189c214fcea6e'
        $evidence | Should -Match 'target_backed_novnc_installed_streaming_smoke:\s*pass'
        $evidence | Should -Match 'installed_tui_operator_smoke:\s*pass'
        $evidence | Should -Match 'Service `PathName` restored:\s*`true`'
        $evidence | Should -Match 'pcvtui\.exe --smoke-once runtime'
        $evidence | Should -Match 'token_value_observed:\s*false'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $matrix | Should -Match 'target_backed_novnc_installed_streaming_smoke:\s*pass'
        $matrix | Should -Match 'installed_tui_operator_smoke:\s*pass'
        $matrix | Should -Match 'installed_tui_operator_smoke_scope:\s*historical-predecessor-only'
        $matrix | Should -Match 'installed-novnc-tui-operator-smoke-2026-05-10-0411'

        $ledger | Should -Match 'Evidence Group: Installed noVNC And TUI Operator Smoke 2026-05-10 0411'
        $ledger | Should -Match 'target_backed_novnc_installed_streaming_smoke=pass'
        $ledger | Should -Match 'installed_tui_operator_smoke=pass'

        $operationsGuide | Should -Match 'Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke\.ps1'
        $operationsGuide | Should -Not -Match 'Invoke-PcvInstalledTuiOperatorSmoke\.ps1'
        $packagingReadme | Should -Match 'installed-novnc-tui-operator-smoke-2026-05-10-0411'
        $agents | Should -Match 'installed-novnc-tui-operator-smoke-2026-05-10-0411'
        $verificationOwnership | Should -Match 'target-backed-novnc-installed-streaming-smoke-20260510-0411'
        $verificationOwnership | Should -Match 'tui_product_status:\s*removed-from-active-product'
    }

    It 'records product TUI service plan closure evidence without service or public release claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $plan = Get-RepoText -RelativePath 'docs/superpowers/plans/2026-05-10-purecvisor-desktop-node-product-tui-service.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'

        $evidence | Should -Match 'product-tui-service-plan-closure-2026-05-10'
        $evidence | Should -Match 'DesktopNode\.Tui\.Tests.*115 passed'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'
        $evidence | Should -Match 'does not itself claim an installed service smoke'

        $ledger | Should -Match 'Evidence Group: Product TUI Service Plan Closure 2026-05-10'
        $ledger | Should -Match 'TUI focused test suite passed with 115 tests'
        $ledger | Should -Match 'Web Console and pcvcli\.exe remain independent Local API clients'
        $plan | Should -Match 'Closure Synchronization - 2026-05-10'
        $plan | Should -Match 'This plan has been executed and closed'
        $developerIndex | Should -Match 'product-tui-service-plan-closure-2026-05-10'
        $agents | Should -Match 'product-tui-service-plan-closure-2026-05-10'
    }

    It 'classifies the manual admin operator campaign buckets without unattended host mutation in Korean' {
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'

        $classification | Should -Match '기준 host gate: full admin host mutation gate'
        $classification | Should -Match '0\.42\.20-admin-smoke.*04220 evidence'
        $classification | Should -Match '0\.42\.18-admin-smoke.*historical|0\.42\.18-admin-smoke.*이전'
        $classification | Should -Match '0\.42\.12-admin-smoke.*04212 evidence.*historical|0\.42\.12-admin-smoke.*04212 evidence.*역사'
        $classification | Should -Match '0\.42\.11-admin-smoke.*04211 evidence.*historical|0\.42\.11-admin-smoke.*04211 evidence.*역사|이전 `0\.42\.11-admin-smoke`'
        $classification | Should -Match '0\.42\.9-admin-smoke.*0429 evidence.*historical|0\.42\.9-admin-smoke.*0429 evidence.*역사|이전 `0\.42\.9-admin-smoke`'
        $classification | Should -Match '0\.42\.8-admin-smoke.*0428 evidence.*historical|0\.42\.8-admin-smoke.*0428 evidence.*역사|이전 `0\.42\.8-admin-smoke`'
        $classification | Should -Match '0\.42\.7-admin-smoke.*0427 evidence.*historical|0\.42\.7-admin-smoke.*0427 evidence.*역사'
        $classification | Should -Match '0\.42\.3-admin-smoke.*0423 evidence.*historical'
        $classification | Should -Match '0\.42\.2-admin-smoke.*0422 evidence.*historical'
        $classification | Should -Match '0\.41\.5-admin-smoke.*0415 evidence.*historical|0\.41\.5-admin-smoke.*0415 evidence.*역사'
        $classification | Should -Match 'Batch Supervisor.*`-AllowHostMutation`'
        $classification | Should -Match '운영자 접근: installed account login smoke와 target-backed noVNC installed streaming smoke'
        $classification | Should -Match '임시 account/JWT 교체, service restart, noVNC target configuration mutation'
        $classification | Should -Match '내부 service hardening: internal HTTPS/TLS lifecycle installed smoke, Credential Manager default transition, Event Log default transition, service token rotation/revoke'
        $classification | Should -Match 'certificate, credential, token, Event Log/provider, service reload state'
        $classification | Should -Match 'Lifecycle/Packaging: internal clean-host install/update/rollback smoke, MSI/update/rollback, Burn/MSIX lifecycle'
        $classification | Should -Match 'install, update, rollback, repair, remove, clean-host environment mutation'
        $classification | Should -Match '묶음 manual-admin campaign을 다시 실행하기 전'
        $classification | Should -Match '최신 PASS:.*`0\.42\.26-admin-smoke`|최신 full admin host mutation PASS는 `0\.42\.26-admin-smoke`'
        $classification | Should -Match '이전 `0\.42\.2-admin-smoke` / 0422 evidence는 historical로 보존'
        $classification | Should -Match '이전에 요청된 0\.41\.2 rebaseline은 historical이며 `blocked-by-installed-version-mismatch`'
        $classification | Should -Match 'historical lifecycle runner 일부는 여전히 `0\.39\.x` 또는 `0\.38\.x` payload'
        $classification | Should -Match 'downgrade/restore run을 명시적으로 수락'
        $classification | Should -Match 'New-PcvManualAdminRebaselineReadiness\.ps1'
        $classification | Should -Match '현재 unattended host-mutating automation으로 옮길 open follow-up은 없다'
        $classification | Should -Not -Match '## Classification Rules'
        $classification | Should -Not -Match '## Approved Progression Scope'
        $classification | Should -Not -Match '## Current Follow-up Queue'
        $classification | Should -Not -Match '## Manual Admin Detail Matrix'
        $classification | Should -Not -Match '## Script Routing'
        $classification | Should -Not -Match '## Current Decision'
    }

    It 'records manual admin rebaseline readiness without host mutation' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-rebaseline-readiness-2026-05-10-0415.md'
        $toolPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'

        $evidencePath | Should -Exist
        $toolPath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match 'manual-admin-rebaseline-readiness-2026-05-10-0415'
        $evidence | Should -Match 'actual_execution:\s*local-readiness-descriptor-written'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'version:\s*0\.41\.5-admin-smoke'
        $evidence | Should -Match 'installed_version:\s*0\.41\.5-admin-smoke'
        $evidence | Should -Match 'requested_0412_status:\s*blocked-by-installed-version-mismatch'
        $evidence | Should -Match 'artifacts/manual-admin-rebaseline-readiness-20260510-0415'
        $evidence | Should -Match '3458c95cc67b8a8540cd10029e8b88f2d618159225fb6b8d76748bd06d922ae5'
        $evidence | Should -Match 'requires-current-lifecycle-runner-generation'
        $evidence | Should -Match 'requires-current-baseline-target-package-pair'
        $evidence | Should -Match 'requires-dedicated-host-current-package-pair'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $classification | Should -Match 'Manual admin 0\.41\.5 rebaseline readiness|manual-admin 0\.41\.5 rebaseline readiness'
        $classification | Should -Match 'baseline host 기준으로 `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation\.md`에 superseded|superseded for baseline host by `full-admin-host-mutation-gate-2026-05-10-0415-hostmutation\.md`'
        $classification | Should -Match 'AUTO-PREFLIGHT'
        $classification | Should -Match 'Credential Manager, Event Log, Burn/MSIX/MSI, update/rollback, clean-host'
    }

    It 'records the 0.41.5 manual admin operator and hardening follow-up evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-operator-hardening-followup-2026-05-10-0415.md'
        $lifecycleEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $aggregate = Get-RepoText -RelativePath 'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $verificationPolicy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $userGuide = Get-RepoText -RelativePath 'docs/USER_GUIDE.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $publicReleaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        $evidencePath | Should -Exist
        $lifecycleEvidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $lifecycleEvidence = Get-Content -Raw -LiteralPath $lifecycleEvidencePath

        $evidence | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $evidence | Should -Match 'artifacts/manual-admin-followup-20260510-0415'
        $evidence | Should -Match 'status:\s*operator-access-hardening-and-lifecycle-packaging-rebaseline-pass'
        $evidence | Should -Match 'operator_access_ok:\s*true'
        $evidence | Should -Match 'internal_service_hardening_ok:\s*true'
        $evidence | Should -Match 'lifecycle_packaging_ok:\s*true'
        $evidence | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $evidence | Should -Match 'artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416'
        $evidence | Should -Match 'installed_version:\s*0\.41\.5-admin-smoke'
        $evidence | Should -Match 'installed-account-login-smoke-rerun'
        $evidence | Should -Match 'target-backed-novnc-installed-streaming-smoke-rerun'
        $evidence | Should -Match 'c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106'
        $evidence | Should -Match 'path_name_restored=true'
        $evidence | Should -Match 'service-token-rotation-revoke'
        $evidence | Should -Match 'old_token_rejection_status=old-token-rejected-after-reload'
        $evidence | Should -Match 'windows-credential-manager-default-transition-installed'
        $evidence | Should -Match '6684061dd248ff2a9567bc251bf45b73ba1ef8174ed92e3f6cd24b2de3dfa615'
        $evidence | Should -Match 'old_source_rejection_status=protected-file-source-rejected-after-reload'
        $evidence | Should -Match 'internal-https-tls-lifecycle-installed'
        $evidence | Should -Match 'certificate_lifecycle=generate-bind-rotate-remove-pass'
        $evidence | Should -Match 'windows-event-log-default-transition-installed'
        $evidence | Should -Match 'b191c45c66a57f987e262d491eeb6d22ea7af5745c93c120d02e41f18592e4ab'
        $evidence | Should -Match 'provider_repair_status=provider-repair-pass'
        $evidence | Should -Match '0\.41\.5-admin-smoke.*0\.41\.6-admin-smoke|0\.41\.6-admin-smoke.*0\.41\.5-admin-smoke'
        $evidence | Should -Match '967ac29bf2928f1fec3a0bb72425d15d2eda65a2466b1cb29dd9183bb18928a3'
        $evidence | Should -Match '4e54c19ca6e6a9beec506613d66220c8b0bbbb579d0926d1d840f2cde7592161'
        $evidence | Should -Match 'Web Console is `http://127\.0\.0\.1/`'
        $evidence | Should -Match 'public trusted signing.*external stable publication'

        $lifecycleEvidence | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $lifecycleEvidence | Should -Match 'status:\s*pass'
        $lifecycleEvidence | Should -Match 'baseline_version:\s*0\.41\.5-admin-smoke'
        $lifecycleEvidence | Should -Match 'target_version:\s*0\.41\.6-admin-smoke'
        $lifecycleEvidence | Should -Match 'baseline_msi_sha256:\s*add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6'
        $lifecycleEvidence | Should -Match 'target_msi_sha256:\s*967ac29bf2928f1fec3a0bb72425d15d2eda65a2466b1cb29dd9183bb18928a3'
        $lifecycleEvidence | Should -Match 'update_package_sha256:\s*4e54c19ca6e6a9beec506613d66220c8b0bbbb579d0926d1d840f2cde7592161'
        $lifecycleEvidence | Should -Match 'installed_product_update_rollback:\s*pass'
        $lifecycleEvidence | Should -Match 'internal_clean_host_install_update_rollback_smoke:\s*pass'
        $lifecycleEvidence | Should -Match 'clean_host_windows_update_ubr:\s*5020'
        $lifecycleEvidence | Should -Match 'clean_host_final_api_unauthenticated_status_code:\s*401'
        $lifecycleEvidence | Should -Match 'failed_root_manifest_version:\s*0\.41\.6-admin-smoke'
        $lifecycleEvidence | Should -Match 'public_trusted_signing:\s*out-of-scope'
        $lifecycleEvidence | Should -Match 'external_stable_publication:\s*out-of-scope'

        $matrix | Should -Match 'manual_admin_operator_hardening_followup:\s*operator-access-hardening-and-lifecycle-packaging-rebaseline-pass'
        $matrix | Should -Match 'manual_admin_operator_access_status:\s*pass'
        $matrix | Should -Match 'manual_admin_internal_service_hardening_status:\s*pass'
        $matrix | Should -Match 'manual_admin_lifecycle_packaging_status:\s*pass-current-0415-to-0416-rebaseline'
        $matrix | Should -Match 'lifecycle_packaging_rebaseline:\s*pass'
        $matrix | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $classification | Should -Match 'Manual-admin operator/hardening follow-up'
        $classification | Should -Match 'Lifecycle/Packaging current rebaseline PASS'
        $ledger | Should -Match 'Evidence Group: Manual Admin Operator/Hardening Follow-up 2026-05-10 0\.41\.5'
        $ledger | Should -Match 'Evidence Group: Lifecycle/Packaging Rebaseline 2026-05-10 0415 to 0416'
        $ledger | Should -Match 'operator_access_ok=true, internal_service_hardening_ok=true, lifecycle_packaging_ok=true'
        $operationsGuide | Should -Match 'Manual-admin operator/hardening follow-up'
        $operationsGuide | Should -Match 'Lifecycle/Packaging current rebaseline'
        $developerIndex | Should -Match '0\.41\.5 manual-admin operator/hardening follow-up'
        $developerIndex | Should -Match 'Lifecycle/Packaging current rebaseline'
        $aggregate | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $aggregate | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $agents | Should -Match 'Latest manual-admin operator/hardening follow-up evidence'
        $agents | Should -Match 'Lifecycle/Packaging current rebaseline evidence'
        $readme | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $readme | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $verificationPolicy | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $verificationPolicy | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $guide | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $guide | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $userGuide | Should -Match '0\.41\.5 설치본 운영 evidence'
        $userGuide | Should -Match '0\.41\.6-admin-smoke'
        $adrIndex | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $adrIndex | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $publicReleaseBoundary | Should -Match 'manual-admin operator/hardening follow-up evidence'
        $publicReleaseBoundary | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $packagingReadme | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $packagingReadme | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
        $installerReadme | Should -Match 'manual-admin-operator-hardening-followup-2026-05-10-0415'
        $installerReadme | Should -Match 'lifecycle-packaging-rebaseline-2026-05-10-0415-0416'
    }

    It 'keeps the internal clean-host lifecycle runner aligned with the Web/API port split' {
        $scriptText = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1'

        $scriptText | Should -Match "baseline_web_console = Invoke-GuestHttpCheck -Uri 'http://127\.0\.0\.1/'"
        $scriptText | Should -Match "updated_web_console = Invoke-GuestHttpCheck -Uri 'http://127\.0\.0\.1/'"
        $scriptText | Should -Match "final_web_console = Invoke-GuestHttpCheck -Uri 'http://127\.0\.0\.1/'"
        $scriptText | Should -Not -Match "web_console = Invoke-GuestHttpCheck -Uri 'http://127\.0\.0\.1:7777/'"
    }

    It 'keeps the clean-host Windows Update NoContact recovery guard documented and code-level only' {
        $scriptText = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1'
        $evidence = Get-RepoText -RelativePath 'docs/ga-ready/evidence/clean-host-windows-update-nocontact-recovery-guard-2026-05-14.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'

        $scriptText | Should -Match 'WindowsUpdateNoContactRecoverySeconds'
        $scriptText | Should -Match 'DisableWindowsUpdateNoContactRecovery'
        $scriptText | Should -Match 'Get-PcvVmRecoverySnapshot'
        $scriptText | Should -Match 'Test-PcvNoContactIdleVm'
        $scriptText | Should -Match 'AllowNoContactRecovery'
        $scriptText | Should -Match 'automatic_recovery_performed'
        $scriptText | Should -Match 'recovery_actions'
        $scriptText | Should -Match 'post-windows-update-heartbeat-no-contact-cpu-idle'
        $scriptText | Should -Match 'windows_update_no_contact_recovery_enabled'
        $scriptText | Should -Match 'Stop-VM -Name \$Name -TurnOff -Force'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $descriptor, $classification, $operationsGuide, $readme, $agents, $adrIndex, $packagingReadme)) {
            $content | Should -Match 'clean-host-windows-update-nocontact-recovery-guard-2026-05-14'
            $content | Should -Match 'NoContact|NoContact recovery|heartbeat'
            $content | Should -Match 'recovery_actions|automatic_recovery_performed|WindowsUpdateNoContactRecoverySeconds'
            $content | Should -Match 'host_mutation_performed:\s*false|host mutation을 실행하지|code-level'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        $classification | Should -Match '실제 clean-host execution은 `MANUAL-ADMIN`'
    }

    It 'records post 04212 follow-up triage without opening 04213 package-pair' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04212-followup-execution-2026-05-14.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $ledger, $operationsGuide, $developerIndex, $adrIndex, $readme, $agents, $packagingReadme)) {
            $content | Should -Match 'post-04212-followup-execution-2026-05-14'
            $content | Should -Match '0f0cb3e2fd8d34570c7cff581c06f53a0fbc8eea'
            $content | Should -Match '0\.42\.13-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        foreach ($content in @($evidence, $descriptor, $matrix, $classification, $ledger)) {
            $content | Should -Match 'deferred-until-next-product-payload-change|not-run-no-product-payload'
        }

        $evidence | Should -Match 'product_payload_change_detected:\s*false'
        $evidence | Should -Match 'latest_product_payload_provenance_commit:\s*8f694dc2494314a6ddd7223f46ec0ba0ca8523e3'
        $evidence | Should -Match 'package_build_decision:\s*deferred-until-next-product-payload-change'
        $evidence | Should -Match 'full_admin_host_mutation_campaign_decision:\s*not-run-no-product-payload'
        $evidence | Should -Match 'manual_admin_package_pair_campaign_decision:\s*deferred-until-next-product-payload-change'
        $evidence | Should -Match 'clean_host_recovery_guard_decision:\s*ready-for-next-clean-host-run-not-executed'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Not -Match '0\.42\.13-admin-smoke.*PASS'

        $descriptor | Should -Match 'post_04212_followup_product_payload_change_detected:\s*`false`'
        $descriptor | Should -Match 'post_04212_followup_clean_host_recovery_guard_decision:\s*`ready-for-next-clean-host-run-not-executed`'
        $descriptor | Should -Match 'historical_post_04212_manual_admin_next_package_pair_candidate_status:\s*`not-opened-no-new-product-payload`'
        $matrix | Should -Match 'post_04212_followup_product_payload_change_detected:\s*false'
        $matrix | Should -Match 'post_04212_followup_full_admin_host_mutation_decision:\s*not-run-no-product-payload'
        $classification | Should -Match 'AUTO-PREFLIGHT` triage evidence'
        $ledger | Should -Match 'product_payload_change_detected=false'
    }

    It 'records post 04212 1-2-3-4-5 current-card follow-up without opening 04213 package or host mutation' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04212-followup-1-2-3-4-5-current-card-2026-05-14.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $ledger, $operationsGuide, $developerIndex, $readme, $agents, $packagingReadme)) {
            $content | Should -Match 'post-04212-followup-1-2-3-4-5-current-card-2026-05-14'
            $content | Should -Match '8224af81c00482145b6c08dcde8c92a039b2aa26'
            $content | Should -Match 'artifacts/web-console-current-card-20260514-04212-rerun-followup'
            $content | Should -Match 'full-admin-host-mutation-gate-20260514-04212-rerun'
            $content | Should -Match '0\.42\.12-admin-smoke'
            $content | Should -Match '0\.42\.13-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        foreach ($content in @($evidence, $descriptor, $matrix, $classification, $ledger)) {
            $content | Should -Match 'pass-dashboard-current-card-smoke-deferred-product-chain'
            $content | Should -Match 'deferred-until-next-product-payload-change|not-run-no-product-payload'
            $content | Should -Match 'dashboard_current_card_smoke.*pass|Dashboard.*current-card smoke.*PASS|Dashboard.*current-card.*PASS'
        }

        $evidence | Should -Match 'product_payload_change_detected:\s*false'
        $evidence | Should -Match 'product_payload_diff_scope:\s*no-changes-in-src-web-product-wrapper-installer'
        $evidence | Should -Match 'package_build_decision:\s*deferred-until-next-product-payload-change'
        $evidence | Should -Match 'full_admin_host_mutation_campaign_decision:\s*not-run-no-product-payload'
        $evidence | Should -Match 'manual_admin_package_pair_campaign_decision:\s*deferred-until-next-product-payload-change'
        $evidence | Should -Match 'clean_host_recovery_summary_key_decision:\s*not-executed-no-package-pair-campaign'
        $evidence | Should -Match 'dashboard_current_card_smoke:\s*pass'
        $evidence | Should -Match 'evidence_view_current_card_smoke:\s*pass'
        $evidence | Should -Match 'token_value_observed:\s*false'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Not -Match '0\.42\.13-admin-smoke.*PASS'

        $descriptor | Should -Match 'post_04212_followup_1_2_3_4_5_dashboard_current_card_smoke:\s*`pass`'
        $descriptor | Should -Match 'post_04212_followup_1_2_3_4_5_host_mutation_performed:\s*`false`'
        $matrix | Should -Match 'post_04212_followup_1_2_3_4_5_dashboard_current_card_smoke:\s*pass'
        $matrix | Should -Match 'post_04212_followup_1_2_3_4_5_host_mutation_performed:\s*false'
        $ledger | Should -Match 'Evidence Group: Post-04212 1-2-3-4-5 Current-card Follow-up 2026-05-14'
        $ledger | Should -Match 'token_value_observed_in_ui_text=false'
    }

    It 'preserves the explicit 0.42.12 evidence in GA-ready indexes while publishing newer current evidence' {
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'

        foreach ($content in @($evidenceIndex, $controlPlaneIndex)) {
            $content | Should -Match '0\.42\.18-admin-smoke'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation\.md'
            $content | Should -Match '0\.42\.12-admin-smoke'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation\.md'
            $content | Should -Match 'artifacts/batch-runs/full-admin-host-mutation-gate-20260514-140126-04212-explicit'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation\.md'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04212-hostmutation\.md'
            $content | Should -Match 'historical|이전'
            $content | Should -Match '0\.42\.11-admin-smoke'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation\.md'
            $content | Should -Match '0\.42\.9-admin-smoke'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation\.md'
            $content | Should -Match '0\.42\.8-admin-smoke'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation\.md'
            $content | Should -Match 'batch_evidence\.status=available|installed-listener-batch-evidence-available'
            $content | Should -Match 'AllowUnsignedDev'
            $content | Should -Match 'public trusted signing|외부 stable publication'
        }

        $evidenceIndex | Should -Match '## 2026-05-19 0\.42\.34 closure 현재'
        $evidenceIndex | Should -Match '## 역사'
        $evidenceIndex | Should -Match '0\.42\.11-admin-smoke.*historical|0\.42\.11-admin-smoke.*이전|이전.*0\.42\.11-admin-smoke'
        $evidenceIndex | Should -Match '0\.42\.7-admin-smoke.*historical|0\.42\.7-admin-smoke.*이전'
        $evidenceIndex | Should -Match '0\.42\.3-admin-smoke.*historical|0\.42\.3-admin-smoke.*이전'
        $evidenceIndex | Should -Match '0\.42\.2-admin-smoke.*historical|0\.42\.2-admin-smoke.*이전'
        $evidenceIndex | Should -Match '0\.41\.5-admin-smoke.*historical|0\.41\.5-admin-smoke.*역사'
        $controlPlaneIndex | Should -Match '## 최신 Evidence'
    }

    It 'records the 0.42.12 full admin host mutation rerun current-card evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $ledger)) {
            $content | Should -Match 'full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation'
            $content | Should -Match 'full-admin-host-mutation-gate-20260514-04212-rerun'
            $content | Should -Match '0\.42\.12-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        foreach ($content in @($evidence, $matrix, $ledger)) {
            $content | Should -Match 'b18d86c197a568ed9b5f6bb38580e568de7a989dda8d730e585684d1c5131b7a'
            $content | Should -Match 'b9c2c25b2ea88f67a0b0ffa5e7e03240eb0ce2fe'
        }

        $evidence | Should -Match 'Host mutation performed:\s*true'
        $evidence | Should -Match 'Dry run:\s*false'
        $evidence | Should -Match 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-04212-rerun'
        $evidence | Should -Match 'artifacts/os-mutation-gates-batch-profile-20260514-04212-rerun'
        $evidence | Should -Match 'artifacts/installed-batch-evidence-current-card-20260514-04212-rerun/summary\.json'
        $evidence | Should -Match 'batch_evidence\.status`: `available'
        $evidence | Should -Match 'latest\.batch_id`: `full-admin-host-mutation-gate-20260514-04212-rerun'
        $evidence | Should -Match 'latest\.release\.msi_sha256'
        $evidence | Should -Match 'Web Console `http://127\.0\.0\.1/` HTTP `200`'
        $evidence | Should -Match '`401` / `PCV_AUTH_REQUIRED`'
        $evidence | Should -Match 'Firewall final rule count: `0`'
        $evidence | Should -Match 'Event Log source present: `false`'
        $evidence | Should -Match 'service_path_has_batch_evidence_root`: `true'
        $evidence | Should -Match 'wrapper_repair_used_native_service_action`: `true'
        $evidence | Should -Match 'wrapper_repair_skipped_outer_start`: `true'
        $evidence | Should -Match '2026-05-13 04212 full gate.*historical predecessor'
        $evidence | Should -Match '0\.42\.13-admin-smoke.*열지 않았다'
        $descriptor | Should -Match 'post_04212_host_mutation_rerun_status:\s*`pass`'
        $descriptor | Should -Match 'post_04212_host_mutation_rerun_host_mutation_performed:\s*`true`'
        $matrix | Should -Match 'previous_04212_full_admin_host_mutation_gate_evidence'
        $ledger | Should -Match 'Evidence Group: Full Admin Host Mutation Gate 2026-05-14 0\.42\.12 Rerun Host Mutation'
    }

    It 'records the explicit 0.42.12 full admin host mutation current evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $verificationPolicy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $publicBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $ledger)) {
            $content | Should -Match 'full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation'
            $content | Should -Match 'full-admin-host-mutation-gate-20260514-140126-04212-explicit'
            $content | Should -Match '0\.42\.12-admin-smoke'
            $content | Should -Match '269b05534d963abc386cbf7d7193f428c8328e1aa2e6c6e3d393e70e938a78db'
            $content | Should -Match 'd338b8a99f3e1e3839ac89a6de0da034ff3da148'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        foreach ($content in @($operationsGuide, $developerIndex, $adrIndex, $readme, $agents, $packagingReadme, $verificationPolicy, $guide, $publicBoundary)) {
            $content | Should -Match 'full-admin-host-mutation-gate-2026-05-16-04220-hostmutation'
            $content | Should -Match 'full-admin-host-mutation-gate-20260516-04220'
        }

        $evidence | Should -Match 'Host mutation performed:\s*true'
        $evidence | Should -Match 'Dry run:\s*false'
        $evidence | Should -Match 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260514-140126-04212-explicit'
        $evidence | Should -Match 'artifacts/os-mutation-gates-batch-profile-20260514-140126-04212-explicit'
        $evidence | Should -Match 'artifacts/installed-batch-evidence-current-card-20260514-140126-04212-explicit/summary\.json'
        $evidence | Should -Match 'artifacts/web-console-current-card-20260514-140126-04212-explicit/summary\.json'
        $evidence | Should -Match 'batch_evidence\.status`: `available'
        $evidence | Should -Match 'latest\.batch_id`: `full-admin-host-mutation-gate-20260514-140126-04212-explicit'
        $evidence | Should -Match 'latest\.release\.msi_sha256'
        $evidence | Should -Match 'Web Console `http://127\.0\.0\.1/` HTTP `200`'
        $evidence | Should -Match '`401` / `PCV_AUTH_REQUIRED`'
        $evidence | Should -Match 'Firewall final rule count: `0`'
        $evidence | Should -Match 'Event Log source present: `false`'
        $evidence | Should -Match 'service_path_has_batch_evidence_root`: `true'
        $evidence | Should -Match 'wrapper_repair_used_native_service_action`: `true'
        $evidence | Should -Match 'wrapper_repair_skipped_outer_start`: `true'
        $evidence | Should -Match 'token_value_observed_in_ui_text=false'
        $evidence | Should -Match '2026-05-14 04212 rerun'
        $evidence | Should -Match '2026-05-13 04212 full gate'
        $evidence | Should -Match 'historical predecessor'
        $evidence | Should -Match '0\.42\.13-admin-smoke.*열지 않았다'
        $descriptor | Should -Match 'post_04212_host_mutation_explicit_status:\s*`pass`'
        $descriptor | Should -Match 'post_04212_host_mutation_explicit_host_mutation_performed:\s*`true`'
        $descriptor | Should -Match 'post_04212_host_mutation_explicit_batch:\s*`full-admin-host-mutation-gate-20260514-140126-04212-explicit`'
        $descriptor | Should -Match 'latest_full_admin_gate_batch:\s*`full-admin-host-mutation-gate-20260528-04256`'
        $matrix | Should -Match 'previous_04212_explicit_full_admin_host_mutation_gate_evidence:\s*docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-explicit-hostmutation\.md'
        $matrix | Should -Match 'latest_full_admin_host_mutation_gate_evidence:\s*docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation\.md'
        $matrix | Should -Match 'post_04212_host_mutation_explicit_execution:\s*pass'
        $matrix | Should -Match 'previous_04212_rerun_full_admin_host_mutation_gate_evidence'
        $ledger | Should -Match 'Evidence Group: Full Admin Host Mutation Gate 2026-05-14 0\.42\.12 Explicit Host Mutation'
    }

    It 'preserves the 0.42.8 full admin host mutation gate as historical evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation.md'

        $evidencePath | Should -Exist
        $content = Get-Content -Raw -LiteralPath $evidencePath

        $content | Should -Match '0\.42\.8-admin-smoke'
        $content | Should -Match 'full-admin-host-mutation-gate-20260512-233650-0428-r2'
        $content | Should -Match 'routeparity-service-msi-hyperv-batch-profile-20260512-233650-0428-r2'
        $content | Should -Match 'os-mutation-gates-batch-profile-20260512-233650-0428-r2'
        $content | Should -Match 'artifacts/admin-smoke-package-20260512-0428-postmerge'
        $content | Should -Match 'e2bc1c5a1b177deb78ce6a5f3faf674f440a769b8ec4ee605e73477c0e1b6687'
        $content | Should -Match '5397e580c98a34e8b7beb5b9773d1d857025315b'
        $content | Should -Match '01762ee3fd103981ac6fce121b6749e832dfabc7420123a6363f7fbe0e0f8f99'
        $content | Should -Match 'AllowUnsignedDev'
        $content | Should -Match 'Service/MSI/Hyper-V'
        $content | Should -Match 'firewall.*Event Log.*trust-store|Event Log.*firewall.*trust-store'
        $content | Should -Match 'Installed manifest version: `0\.42\.8-admin-smoke`|installed manifest `0\.42\.8-admin-smoke`'
        $content | Should -Match 'LAN listener smoke|LAN prefix'
        $content | Should -Match 'http://127\.0\.0\.1/'
        $content | Should -Match '/pcv-config\.js'
        $content | Should -Match 'PCV_AUTH_REQUIRED'
        $content | Should -Match 'batch_evidence\.status.*available|installed-listener-batch-evidence-available'
        $content | Should -Match 'Public trusted signing|public trusted signing'
        $content | Should -Match 'External stable publication|external stable publication|외부 stable publication'

        $paths = @(
            'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md',
            'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md',
            'docs/ga-ready/EVIDENCE_INDEX.md',
            'docs/ga-ready/CONTROL_PLANE_INDEX.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        )

        foreach ($path in $paths) {
            $doc = Get-RepoText -RelativePath $path
            $doc | Should -Match '0\.42\.8-admin-smoke'
            $doc | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0428-hostmutation\.md|full-admin-host-mutation-gate-2026-05-12-0428-hostmutation'
            $doc | Should -Match '5397e580c98a34e8b7beb5b9773d1d857025315b|01762ee3fd103981ac6fce121b6749e832dfabc7420123a6363f7fbe0e0f8f99|AllowUnsignedDev'
            $doc | Should -Match 'public trusted signing|외부 stable publication'
        }
    }

    It 'records post-0423 follow-up triage and next implementation slices without public claims' {
        $triagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-0423-followup-triage-2026-05-12.md'
        $planPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-05-12-purecvisor-desktop-node-post-0423-followup-slices.md'

        $triagePath | Should -Exist
        $planPath | Should -Exist

        $triage = Get-Content -Raw -LiteralPath $triagePath
        $plan = Get-Content -Raw -LiteralPath $planPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'

        $triage | Should -Match 'evidence_id:\s*post-0423-followup-triage-2026-05-12'
        $triage | Should -Match 'manual_admin_0423_rerun_decision:\s*required-next-package-pair'
        $triage | Should -Match 'baseline_version:\s*0\.42\.3-admin-smoke'
        $triage | Should -Match 'target_version:\s*0\.42\.4-admin-smoke'
        $triage | Should -Match 'package_pair_rebaseline_plan:\s*0\.42\.3-admin-smoke-to-0\.42\.4-admin-smoke'
        $triage | Should -Match 'host_mutation_performed:\s*false'
        $triage | Should -Match 'public_trusted_signing:\s*out-of-scope'
        $triage | Should -Match 'external_stable_publication:\s*out-of-scope'
        $triage | Should -Match 'manual-admin-campaign-2026-05-11-0420-0421'
        $triage | Should -Match 'Runtime/Core'
        $triage | Should -Match 'Host Ops'
        $triage | Should -Match 'Packaging / Manual-admin Campaign Orchestrator'

        $plan | Should -Match '0\.42\.3-admin-smoke'
        $plan | Should -Match '0\.42\.4-admin-smoke'
        $plan | Should -Match 'MANUAL-ADMIN 1-2-3-4'
        $plan | Should -Match 'Public Boundary Drift Guard'
        $plan | Should -Match 'Runtime/Core installed summary contract'
        $plan | Should -Match 'Host Ops runner preflight contract'

        foreach ($content in @($evidenceIndex, $controlPlaneIndex, $developerIndex)) {
            $content | Should -Match 'post-0423-followup-triage-2026-05-12'
            $content | Should -Match '2026-05-12-purecvisor-desktop-node-post-0423-followup-slices'
        }

        $matrix | Should -Match 'post_0423_package_pair_rebaseline_plan:\s*superseded-by-0425-0426-pass'
        $matrix | Should -Match 'post_0423_package_pair_baseline_version:\s*0\.42\.3-admin-smoke'
        $matrix | Should -Match 'post_0423_package_pair_target_version:\s*0\.42\.4-admin-smoke'
        $classification | Should -Match 'Post-0423 triage'
        $classification | Should -Match 'historical planning record'
        $classification | Should -Match '0425.*0426 PASS|0427.*0428 PASS'
        $classification | Should -Match 'Operator Access.*noVNC/TUI.*internal'
        $classification | Should -Match 'out-of-scope.*not-claimed'
    }

    It 'preserves manual admin 0423 to 0424 package-pair evidence as historical blocker without public claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md'
        $descriptorPath = Join-Path $script:RepoRoot 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'

        $evidencePath | Should -Exist
        $descriptorPath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $descriptor = Get-Content -Raw -LiteralPath $descriptorPath
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $adr0006 = Get-RepoText -RelativePath 'docs/adr/0006-internal-private-network-distribution.md'

        $evidence | Should -Match 'evidence_id:\s*manual-admin-campaign-2026-05-12-0423-0424'
        $evidence | Should -Match 'result:\s*PARTIAL_PASS_WITH_CLEAN_HOST_BLOCKER'
        $evidence | Should -Match 'baseline_version:\s*0\.42\.3-admin-smoke'
        $evidence | Should -Match 'target_version:\s*0\.42\.4-admin-smoke'
        $evidence | Should -Match 'host_mutation_performed:\s*true'
        $evidence | Should -Match 'public_trusted_signing:\s*out-of-scope'
        $evidence | Should -Match 'external_stable_publication:\s*out-of-scope'
        $evidence | Should -Match 'public_release:\s*not-claimed'
        $evidence | Should -Match '71eaeff1c6f244bc57e9c2ac9fa57b54676d00cfbf66ba119b37c9bb21949277'
        $evidence | Should -Match 'e6e8c5d24cef91d2765ec48c6ea58a49f16c0379d963512a90114da106980b2d'
        $evidence | Should -Match 'full-admin-host-mutation-gate-20260512-042902-0424'
        $evidence | Should -Match 'stale-product-registration-cleanup'
        $evidence | Should -Match 'update-0423-to-0424-versioned-summary'
        $evidence | Should -Match 'rollback-0424-to-0423-summary'
        $evidence | Should -Match 'clean-host-rerun'
        $evidence | Should -Match 'EventLogDefaultTransition'
        $evidence | Should -Match 'ConfigureInstalled'
        $evidence | Should -Match 'internal/admin-smoke evidence.*public release evidence가 아니다|public release evidence가 아니다'

        $descriptor | Should -Match 'manual-admin-next-campaign-descriptor-2026-05-14-post-04212-followup-triage'
        $descriptor | Should -Match 'status:\s*`closed-package-pair-04211-04212-pass`'
        $descriptor | Should -Match 'historical-partial-pass-clean-host-blocked'
        $descriptor | Should -Match 'ManualAdminCampaignDescriptor'
        $descriptor | Should -Match 'c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e'
        $descriptor | Should -Match '8f694dc2494314a6ddd7223f46ec0ba0ca8523e3'

        $matrix | Should -Match 'manual_admin_0423_0424_campaign:\s*historical-partial-pass-clean-host-blocked'
        $matrix | Should -Match 'manual_admin_0423_0424_current_claim:\s*false'
        $matrix | Should -Match 'manual_admin_0423_0424_clean_host:\s*blocked-by-baseline-msi-custom-action-sequence'
        $matrix | Should -Match 'installer_custom_action_sequence_fix:\s*code-level-applied-configure-before-eventlog'

        $classification | Should -Match 'Manual-admin 0423.*0424 campaign'
        $classification | Should -Match 'historical-partial-pass-clean-host-blocked'
        $classification | Should -Match 'ConfigureInstalled.*EventLogDefaultTransition'

        foreach ($content in @($evidenceIndex, $controlPlaneIndex, $developerIndex, $adrIndex, $adr0006)) {
            $content | Should -Match 'manual-admin-campaign-2026-05-12-0423-0424'
            $content | Should -Match 'clean-host|Clean-host'
            $content | Should -Match 'public trusted signing|외부 stable publication|Public trusted signing'
        }
    }

    It 'records post-0426 provenance rebuild and Batch Supervisor descriptor linkage' {
        $triagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-0426-manual-admin-followup-triage-2026-05-12.md'
        $campaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0425-0426.md'
        $descriptorPath = Join-Path $script:RepoRoot 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $descriptorBatchManifestHelperPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptorBatchManifest.ps1'

        $triagePath | Should -Exist
        $campaignPath | Should -Exist
        $descriptorPath | Should -Exist
        $descriptorBatchManifestHelperPath | Should -Exist

        $triage = Get-Content -Raw -LiteralPath $triagePath
        $campaign = Get-Content -Raw -LiteralPath $campaignPath
        $descriptor = Get-Content -Raw -LiteralPath $descriptorPath
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'

        $triage | Should -Match 'evidence_id:\s*post-0426-manual-admin-followup-triage-2026-05-12'
        $triage | Should -Match 'result:\s*PASS'
        $triage | Should -Match 'host_mutation_performed:\s*true'
        $triage | Should -Match 'next_admin_smoke_package_build_decision:\s*executed-0\.42\.7-admin-smoke'
        $triage | Should -Match 'next_full_admin_host_mutation_gate_decision:\s*executed-0\.42\.7-admin-smoke'
        $triage | Should -Match 'artifacts/admin-smoke-package-20260512-0426-postmerge'
        $triage | Should -Match '9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16'
        $triage | Should -Match '37f4d6b83d6caef1338e0a60e5df0a60209b51f8'
        $triage | Should -Match 'ManualAdminCampaignDescriptor'
        $triage | Should -Match 'historical-partial-pass-clean-host-blocked'
        $triage | Should -Match '새 version.*-AllowHostMutation'
        $triage | Should -Match '0\.42\.7-admin-smoke.*executed|0\.42\.7-admin-smoke.*실행'
        $triage | Should -Match 'dashboard/wiki current card.*installed-listener-batch-evidence-available'
        $triage | Should -Match 'full-admin-host-mutation-gate-20260512-181309-0427'
        $triage | Should -Match '256643b923a9a3b3763f6b3d457e1b6d7049bd959cb54da2f6cc946fe79c01b9'
        $triage | Should -Match '9e410497e5a0f9c79ebf086209ed5c8bba669c48dd5b6c34a00c74933f4ae3a4'
        $triage | Should -Match 'public_trusted_signing:\s*not-claimed'
        $triage | Should -Match 'external_stable_publication:\s*not-claimed'

        $campaign | Should -Match 'Post-merge Provenance 재빌드'
        $campaign | Should -Match '9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16'
        $descriptor | Should -Match 'closed-package-pair-04211-04212-pass'
        $descriptor | Should -Match 'ManualAdminCampaignDescriptor'
        $descriptor | Should -Match 'New-PcvManualAdminCampaignDescriptorBatchManifest\.ps1'
        $descriptor | Should -Match 'manual-admin-campaign-descriptor-20260514-04211-04212'
        $descriptor | Should -Match 'Invoke-PcvBatchSupervisor\.ps1.*manual-admin-campaign-descriptor-20260514-04211-04212'
        $descriptor | Should -Match 'full-admin-host-mutation-gate-20260513-0429-04211'

        $matrix | Should -Match 'post_0426_followup_triage:\s*pass'
        $matrix | Should -Match 'batch_supervisor_manual_admin_descriptor_profile:\s*available-non-mutating'
        $matrix | Should -Match 'manual_admin_next_descriptor_batch_manifest:\s*manual-admin-campaign-descriptor-20260514-04211-04212'
        $matrix | Should -Match 'manual_admin_04211_04212_descriptor_summary:\s*artifacts/manual-admin-campaign-20260514-04211-04212/manual-admin-campaign-descriptor-supervised/summary\.json'
        $matrix | Should -Match 'next_admin_smoke_package_build_decision:\s*executed-0\.42\.12-admin-smoke'
        $matrix | Should -Match 'next_full_admin_host_mutation_gate_decision:\s*executed-0\.42\.12-admin-smoke'
        $classification | Should -Match 'ManualAdminCampaignDescriptor'
        $controlPlaneIndex | Should -Match 'dashboard/wiki current card'
        $controlPlaneIndex | Should -Match 'batch_evidence\.latest'

        foreach ($content in @($evidenceIndex, $controlPlaneIndex, $operationsGuide, $readme, $developerIndex, $adrIndex)) {
            $content | Should -Match 'post-0426-manual-admin-followup-triage-2026-05-12|admin-smoke-package-20260512-0426-postmerge'
            $content | Should -Match 'public trusted signing|외부 stable publication|Public trusted signing|not-claimed'
        }

        foreach ($content in @($campaign, $developerIndex, $adrIndex)) {
            $content | Should -Match '9f8464c7b47c45be51679d68c11d19429d85746f55daa00211fb235995f5be16'
        }

        $packagingReadme | Should -Match 'public trusted signing|외부 stable publication|Public trusted signing|not-claimed'
    }

    It 'records 0.42.12 manual-admin package-pair closure' {
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md'
        $manualCampaignPath | Should -Exist

        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'

        foreach ($content in @($manualCampaign, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $operationsGuide, $readme, $agents, $packagingReadme)) {
            $content | Should -Match 'manual-admin-campaign-2026-05-14-04211-04212'
            $content | Should -Match '0\.42\.11-admin-smoke'
            $content | Should -Match '0\.42\.12-admin-smoke'
            $content | Should -Match 'c89aeb327a5c6c95c7f6d41e8f300be2ed1311a4efe17d5825c22f93bc32026e'
            $content | Should -Match '91aeda44b417ae7c80ee4d50793968a22cb55004c69e23470d7c6a3ded858e04'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $manualCampaign | Should -Match 'result:\s*PASS'
        $manualCampaign | Should -Match 'manual-admin-campaign-descriptor-20260514-04211-04212'
        $manualCampaign | Should -Match 'clean-host.*Windows Update|Windows Update'
        $manualCampaign | Should -Match 'KB5087545'
        $manualCampaign | Should -Match 'Burn'
        $manualCampaign | Should -Match 'MSIX'
        $manualCampaign | Should -Match 'installed runtime ops summary'
        $descriptor | Should -Match 'closed-package-pair-04211-04212-pass'
        $descriptor | Should -Match 'historical_post_04212_manual_admin_next_package_pair_candidate_status:\s*`not-opened-no-new-product-payload`'
        $matrix | Should -Match 'manual_admin_04211_04212_campaign:\s*pass'
        $matrix | Should -Match 'manual_admin_04211_04212_missing_evidence:\s*none'
    }

    It 'records 0.42.13 manual-admin package-pair closure and 0.42.14 selector guard package' {
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04212-04213.md'
        $selectorGuardPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/ops-summary-descriptor-selector-guard-package-2026-05-14-04214.md'

        $manualCampaignPath | Should -Exist
        $selectorGuardPath | Should -Exist

        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $selectorGuard = Get-Content -Raw -LiteralPath $selectorGuardPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'

        foreach ($content in @($manualCampaign, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification)) {
            $content | Should -Match 'manual-admin-campaign-2026-05-14-04212-04213'
            $content | Should -Match '0\.42\.12-admin-smoke'
            $content | Should -Match '0\.42\.13-admin-smoke'
            $content | Should -Match '414c6cf552723da8d2102b76412f3ef56cd8c06741172f6b75cdfd48986dad6a'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($selectorGuard, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $operationsGuide, $readme, $packagingReadme, $developerIndex, $adrIndex)) {
            $content | Should -Match 'ops-summary-descriptor-selector-guard-package-2026-05-14-04214'
            $content | Should -Match '0\.42\.14-admin-smoke'
            $content | Should -Match 'dabee54698ec4de72c31d2934d655af9ba3ecdda292aff096790fea24b7901eb'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $manualCampaign | Should -Match 'result:\s*PASS'
        $manualCampaign | Should -Match 'manual-admin-campaign-descriptor-20260514-04212-04213'
        $manualCampaign | Should -Match '638c186f5dd4f2f8201d883f51eab3447f365f512d5ba760c9'
        $manualCampaign | Should -Match 'automatic_recovery_performed=true'
        $manualCampaign | Should -Match 'recovery_actions=1'
        $manualCampaign | Should -Match 'Burn'
        $manualCampaign | Should -Match 'MSIX'
        $manualCampaign | Should -Match 'installed runtime ops summary'

        $selectorGuard | Should -Match 'BatchEvidenceSummaryReader\.cs'
        $selectorGuard | Should -Match 'OpsSummarySkipsManualAdminDescriptorWhenSelectingLatestOperationalEvidence'
        $selectorGuard | Should -Match 'batch_evidence\.status=available'
        $selectorGuard | Should -Match 'errors=\[\]'
        $selectorGuard | Should -Match 'Token value UI text exposure:\s*`false`'
        $selectorGuard | Should -Match 'artifacts/web-console-current-card-20260514-04214-selectorfix'
        $selectorGuard | Should -Match 'full-admin-host-mutation-gate-20260514-140126-04212-explicit'

        $descriptor | Should -Match 'historical_04212_04213_status:\s*`closed-package-pair-04212-04213-pass`'
        $descriptor | Should -Match 'historical_04214_04215_status:\s*`closed-package-pair-04214-04215-pass`'
        $descriptor | Should -Match 'historical_post_04212_manual_admin_next_package_pair_candidate_status:\s*`not-opened-no-new-product-payload`'
        $descriptor | Should -Match 'manual-admin-campaign-descriptor-20260514-04212-04213'
        $matrix | Should -Match 'manual_admin_04212_04213_campaign:\s*pass'
        $matrix | Should -Match 'manual_admin_04212_04213_missing_evidence:\s*none'
        $matrix | Should -Match 'ops_summary_descriptor_selector_guard:\s*pass'
        $matrix | Should -Match 'historical_previous_product_payload_package_build:\s*0\.42\.16-admin-smoke'
        $matrix | Should -Match 'previous_manual_admin_next_package_pair_candidate:\s*pending-next-product-payload-after-04216-fullgate'
    }

    It 'records 0.42.18 manual-admin package-pair and full admin host mutation closure' {
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-15-04216-04218.md'
        $hostMutationPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-15-04218-hostmutation.md'

        $manualCampaignPath | Should -Exist
        $hostMutationPath | Should -Exist

        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $hostMutation = Get-Content -Raw -LiteralPath $hostMutationPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $verificationPolicy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $publicBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'

        foreach ($content in @($manualCampaign, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification)) {
            $content | Should -Match 'manual-admin-campaign-2026-05-15-04216-04218'
            $content | Should -Match '0\.42\.18-admin-smoke'
            $content | Should -Match '459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af'
            $content | Should -Match '8526a18bcc5bfee09289bae27c8b5b1e97d5bd818401f046cdcb1e972c8b09bd'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }
        $manualCampaign | Should -Match '0\.42\.16-admin-smoke'
        $manualCampaign | Should -Match 'PCV_PRODUCT_UPDATE_START_FAILED'
        $manualCampaign | Should -Match 'self-contained'

        foreach ($content in @($hostMutation, $evidenceIndex, $controlPlaneIndex, $matrix, $classification)) {
            $content | Should -Match 'full-admin-host-mutation-gate-2026-05-15-04218-hostmutation'
            $content | Should -Match 'full-admin-host-mutation-gate-20260515-163107-04218'
            $content | Should -Match '0184e910ac3b3e21363342b02a980d7359ec3f60d87faddbdc68aa5c901c4f09'
            $content | Should -Match '9121d1f5e7fa83d803c484a44698d4fc8e825c19'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $manualCampaign | Should -Match 'result:\s*PASS'
        $manualCampaign | Should -Match 'manual-admin-campaign-descriptor-20260515-04216-04218'
        $manualCampaign | Should -Match 'automatic_recovery_performed=true'
        $manualCampaign | Should -Match 'recovery_actions=1'
        $manualCampaign | Should -Match 'Burn'
        $manualCampaign | Should -Match 'MSIX'
        $manualCampaign | Should -Match 'installed runtime ops summary'

        $hostMutation | Should -Match 'batch evidence status.*`available`|batch_evidence\.status=available'
        $hostMutation | Should -Match 'manual-admin-campaign-descriptor-20260515-04216-04218'
        $hostMutation | Should -Match 'descriptor excluded from operational latest.*`true`|descriptor_excluded_from_operational_latest.*true'
        $hostMutation | Should -Match 'artifacts/installed-current-card-20260515-04218-fullgate'

        $descriptor | Should -Match 'closed-package-pair-04216-04218-pass'
        $descriptor | Should -Match 'previous_manual_admin_next_package_pair_candidate:\s*`pending-next-product-payload-after-04218-fullgate`'
        $descriptor | Should -Match 'framework_dependent_regression_04217_status:\s*`superseded-by-04218-self-contained-package`'
        $matrix | Should -Match 'manual_admin_04216_04218_campaign:\s*pass'
        $matrix | Should -Match 'manual_admin_04216_04218_missing_evidence:\s*none'
        $matrix | Should -Match 'manual_admin_04216_04218_campaign:\s*pass'
        $matrix | Should -Match 'manual_admin_04216_04218_target_msi_sha256:\s*459a623660353d6eff4d74218cf3160b349788e55b2b1b49e533a5d4af3258af'
        $matrix | Should -Match 'previous_04218_full_admin_host_mutation_gate_version:\s*0\.42\.18-admin-smoke'
        $matrix | Should -Match 'previous_manual_admin_next_package_pair_candidate.*pending-next-product-payload-after-04218-fullgate'
        $matrix | Should -Match 'framework_dependent_regression_04217:\s*superseded-by-04218-self-contained-package'
        $matrix | Should -Match 'manual_admin_04215_04216_campaign:\s*pass'
    }

    It 'records post-04218 1-2-3-4-5-6 contract alignment without host mutation or public claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04218-contract-alignment-2026-05-15.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $runtimeBaseline = Get-RepoText -RelativePath 'docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md'
        $hypervBaseline = Get-RepoText -RelativePath 'docs/ga-ready/hyperv-domain-baseline-2026-05-11.md'
        $hostOpsBaseline = Get-RepoText -RelativePath 'docs/ga-ready/host-ops-boundary-baseline-2026-05-11.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $publicBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $operationsGuide, $developerIndex, $publicBoundary, $readme, $agents)) {
            $content | Should -Match 'post-04218-contract-alignment-2026-05-15'
            $content | Should -Match '0\.42\.18-admin-smoke'
            $content | Should -Match 'Runtime/Core|runtime_api_diagnostics_bridge|route/evidence bridge'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $evidence | Should -Match 'result:\s*PASS'
        $evidence | Should -Match 'actual_execution:\s*docs-and-contract-regression'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'runtime_api_diagnostics_bridge:\s*route-family-evidence-linked'
        $evidence | Should -Match 'hyperv_dispatch_catalog_contract:\s*vm-checkpoint-network-fixed'
        $evidence | Should -Match 'host_ops_lifecycle_buckets:\s*service-eventlog-firewall-truststore-data-root-separated'
        $evidence | Should -Match 'packaging_release_next_trigger:\s*pending-next-product-payload-after-04218-fullgate'
        $evidence | Should -Match 'operator_surface_journey_alignment:\s*web-console-tui-cli-current-card'
        $evidence | Should -Match 'public_boundary_preserved:\s*adr-0005-closed-adr-0006-internal-only'
        $evidence | Should -Match 'GET /api/v1/diagnostics/bundles'
        $evidence | Should -Match 'GET /api/v1/ops/summary'
        $evidence | Should -Match 'NetworkInventory'
        $evidence | Should -Match 'CheckpointMutation'
        $evidence | Should -Match 'service-eventlog-firewall-truststore-data-root-separated'
        $evidence | Should -Match 'Web Console.*TUI.*CLI'

        $runtimeBaseline | Should -Match 'runtime_api_diagnostics_bridge=route-family-evidence-linked'
        $runtimeBaseline | Should -Match 'DesktopNodeApiRuntimeRoutes'
        $runtimeBaseline | Should -Match 'ApiHandlerAdapterContract\.RouteFamily'
        $runtimeBaseline | Should -Match 'GET /api/v1/ops/summary'
        $runtimeBaseline | Should -Match 'GET /api/v1/diagnostics/bundles'

        $hypervBaseline | Should -Match 'DesktopNodeHyperVAdapterDispatchCatalog'
        $hypervBaseline | Should -Match 'PCV_NATIVE_DISPATCH_PROVIDER_BOUNDARY_DRIFT'
        $hypervBaseline | Should -Match 'hyperv_dispatch_catalog_contract=vm-checkpoint-network-fixed'
        $hypervBaseline | Should -Match 'vm-power-state-provider'
        $hypervBaseline | Should -Match 'checkpoint-mutation-provider'

        $hostOpsBaseline | Should -Match 'DesktopNodeHostOpsCatalog\.TryGetOperation'
        $hostOpsBaseline | Should -Match 'DesktopNodeHostOpsCatalog\.OperationBelongsTo'
        $hostOpsBaseline | Should -Match 'host_ops_lifecycle_buckets=service-eventlog-firewall-truststore-data-root-separated'
        $hostOpsBaseline | Should -Match 'service-lifecycle'
        $hostOpsBaseline | Should -Match 'event-log'
        $hostOpsBaseline | Should -Match 'firewall'
        $hostOpsBaseline | Should -Match 'trust-store'
        $hostOpsBaseline | Should -Match 'data-root'

        $matrix | Should -Match 'post_04218_contract_alignment:\s*pass'
        $matrix | Should -Match 'runtime_api_diagnostics_bridge:\s*route-family-evidence-linked'
        $matrix | Should -Match 'hyperv_dispatch_catalog_contract:\s*vm-checkpoint-network-fixed'
        $matrix | Should -Match 'host_ops_lifecycle_bucket_contract:\s*service-eventlog-firewall-truststore-data-root-separated'
        $matrix | Should -Match 'operator_surface_journey_alignment:\s*web-console-tui-cli-current-card'
        $matrix | Should -Match 'public_boundary_preserved_by_post_04218:\s*adr-0005-closed-adr-0006-internal-only'

        $descriptor | Should -Match 'post_04218_contract_alignment_status:\s*`pass`'
        $descriptor | Should -Match 'post_04218_packaging_release_next_trigger:\s*`pending-next-product-payload-after-04218-fullgate`'
        $classification | Should -Match 'AUTO-REPO'
        $classification | Should -Match 'package build, clean-host, full admin host mutation, public signing/publication을 실행하지 않는다'
        $operationsGuide | Should -Match '현재 운영자 Surface 여정'
        $operationsGuide | Should -Match 'Web Console과 CLI'
        $operationsGuide | Should -Match 'post-04218-contract-alignment-2026-05-15\.md.*TUI 포함 여정은[\s\S]*historical predecessor'
        $publicBoundary | Should -Match 'ADR-0005[\s\S]*closed-not-adopted'
        $publicBoundary | Should -Match 'ADR-0006[\s\S]*internal-private-network-only'
    }

    It 'records post-04218 runtime domain development slices as code-level evidence without host mutation or public claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04218-runtime-domain-slices-2026-05-15.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $operatorTerms = Get-RepoText -RelativePath 'docs/OPERATOR_SURFACE_TERMS.md'
        $apiContract = Get-RepoSourceText -RelativePath 'src/DesktopNode.Api/ApiHandlerAdapterContract.cs'
        $hypervDispatch = Get-RepoSourceText -RelativePath 'src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs'
        $hostOpsCatalog = Get-RepoSourceText -RelativePath 'src/DesktopNode.Host/Ops/DesktopNodeHostOpsCatalog.cs'
        $descriptorScript = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1'
        $tuiRendererPath = Join-Path $script:RepoRoot 'src/DesktopNode.Tui/TuiRenderer.cs'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $classification, $readme, $agents)) {
            $content | Should -Match 'post-04218-runtime-domain-slices-2026-05-15'
            $content | Should -Match 'CODE_LEVEL_PASS|code/test contract|code-contract-regression'
            $content | Should -Match 'host mutation performed: `false`|host_mutation_performed:\s*false|host mutation performed `false`'
            $content | Should -Match 'public trusted signing|external stable publication|외부 stable publication'
        }

        $evidence | Should -Match 'runtime_api_diagnostics_bridge:\s*route-family-evidence-linked'
        $evidence | Should -Match 'hyperv_dispatch_catalog_contract:\s*vm-checkpoint-network-fixed'
        $evidence | Should -Match 'host_ops_lifecycle_bucket_contract:\s*service-eventlog-firewall-truststore-data-root-separated'
        $evidence | Should -Match 'packaging_release_next_trigger:\s*product-payload-change-after-04218-fullgate'
        $evidence | Should -Match 'operator_surface_journey_alignment:\s*web-console-tui-cli-current-card'
        $evidence | Should -Match 'public_boundary_preserved:\s*adr-0005-closed-adr-0006-internal-only'

        $apiContract | Should -Match 'RuntimeApiDiagnosticsBridge'
        $apiContract | Should -Match 'route-family-evidence-linked'
        $apiContract | Should -Match 'runtime-api-job-runtime-contract'
        $apiContract | Should -Match 'runtime-api-ops-summary-current-card'
        $apiContract | Should -Match 'runtime-api-diagnostics-bundle-contract'

        $hypervDispatch | Should -Match 'ContractKey = "vm-checkpoint-network-fixed"'
        $hypervDispatch | Should -Match 'OperationsForHandler'
        $hostOpsCatalog | Should -Match 'LifecycleBucketContractKey = "service-eventlog-firewall-truststore-data-root-separated"'
        $hostOpsCatalog | Should -Match 'RequiredLifecycleSmokeBuckets'
        $descriptorScript | Should -Match 'product-payload-change-after-04218-fullgate'
        $descriptorScript | Should -Match 'release_candidate'
        $tuiRendererPath | Should -Not -Exist
        $operatorTerms | Should -Match 'Current-card 여정'
        $operatorTerms | Should -Match 'Web Console과 CLI는 GET /api/v1/ops/summary'
    }

    It 'records post-04218 follow-up execution with 0.42.19 package build and CI boundary guard' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04218-followup-execution-2026-05-15.md'
        $boundaryGuardPath = Join-Path $script:RepoRoot 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $evidencePath | Should -Exist
        $boundaryGuardPath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $boundaryGuard = Get-Content -Raw -LiteralPath $boundaryGuardPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $operatorTerms = Get-RepoText -RelativePath 'docs/OPERATOR_SURFACE_TERMS.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $classification, $readme, $agents)) {
            $content | Should -Match 'post-04218-followup-execution-2026-05-15'
            $content | Should -Match '0\.42\.19-admin-smoke'
            $content | Should -Match 'artifacts/admin-smoke-package-20260515-04219'
            $content | Should -Match 'public-boundary-ci-required'
            $content | Should -Match 'public trusted signing|external stable publication|외부 stable publication'
        }

        $evidence | Should -Match 'package_build_performed:\s*true'
        $evidence | Should -Match 'package_build_decision:\s*executed-0\.42\.19-admin-smoke'
        $evidence | Should -Match 'target_msi_sha256:\s*[0-9a-f]{64}'
        $evidence | Should -Match 'target_update_zip_sha256:\s*not-built'
        $evidence | Should -Match 'manual_admin_package_pair_campaign_decision:\s*not-run-package-build-only'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'
        $evidence | Should -Match 'runtime_route_registry_source:\s*ApiHandlerAdapterContract'
        $evidence | Should -Match 'hyperv_dispatch_model:\s*handler-registry-delegate-map'
        $evidence | Should -Match 'host_ops_family_helpers:\s*service-eventlog-firewall-truststore-data-root'
        $evidence | Should -Match 'operator_surface_snapshot_parity:\s*web-console-tui-cli-current-card'

        $boundaryGuard | Should -Match 'PUBLIC_BOUNDARY_CI_CONTRACT'
        $boundaryGuard | Should -Match 'ADR-0005.*closed-not-adopted'
        $boundaryGuard | Should -Match 'ADR-0006.*internal-private-network-only'
        $boundaryGuard | Should -Match 'required_verification: packaging-pester-public-boundary-guard'
        $boundaryGuard | Should -Match 'public_trusted_signing: not-claimed'
        $publicBoundary | Should -Match 'PUBLIC_BOUNDARY_CI_CONTRACT'
        $operatorTerms | Should -Match 'current-card snapshot parity'
        $classification | Should -Match 'public-boundary-ci-required'
    }

    It 'records post-04219 follow-up execution with descriptor readiness and required CI wiring' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04219-followup-execution-2026-05-16.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $batchSupervisor = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1'
        $ciWorkflow = Get-RepoText -RelativePath '.github/workflows/public-boundary.yml'

        foreach ($content in @($evidence, $evidenceIndex, $classification, $controlPlaneIndex)) {
            $content | Should -Match 'post-04219-followup-execution-2026-05-16'
            $content | Should -Match '0\.42\.19-admin-smoke'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260516-04218-04219'
            $content | Should -Match 'public-boundary-ci-required'
        }

        $evidence | Should -Match 'manual_admin_descriptor_execution:\s*executed'
        $evidence | Should -Match 'manual_admin_readiness_execution:\s*executed'
        $evidence | Should -Match 'full_admin_host_mutation_decision:\s*(executed|prepared)'
        $evidence | Should -Match 'runtime_queued_mutation_route_registry:\s*contract-backed'
        $evidence | Should -Match 'hyperv_operation_telemetry_error_contract:\s*operation-level-telemetry-error-contract-v1'
        $evidence | Should -Match 'host_ops_family_helpers:\s*service-eventlog-firewall-truststore-data-root-config-job-service-token-credential-manager'
        $evidence | Should -Match 'public_boundary_ci_workflow:\s*\.github/workflows/public-boundary\.yml'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $batchSupervisor | Should -Match 'public-boundary-ci-required'
        $batchSupervisor | Should -Match 'PcvAdminSmokeEvidenceDocs.Tests.ps1'
        $ciWorkflow | Should -Match 'PUBLIC_BOUNDARY_CI_CONTRACT'
        $ciWorkflow | Should -Match 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1'
    }

    It 'records post-04220 development slices with runtime Hyper-V host-ops and packaging code contracts' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04220-dev-slices-2026-05-16.md'
        $publicBoundaryPassPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md'
        $evidencePath | Should -Exist
        $publicBoundaryPassPath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $publicBoundaryPass = Get-Content -Raw -LiteralPath $publicBoundaryPassPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $apiContract = Get-RepoSourceText -RelativePath 'src/DesktopNode.Api/ApiHandlerAdapterContract.cs'
        $hypervCatalog = Get-RepoSourceText -RelativePath 'src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs'
        $hostOpsCatalog = Get-RepoSourceText -RelativePath 'src/DesktopNode.Host/Ops/DesktopNodeHostOpsCatalog.cs'
        $descriptorScript = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $developerIndex, $readme)) {
            $content | Should -Match 'post-04220-dev-slices-2026-05-16'
            $content | Should -Match '0\.42\.20-admin-smoke'
            $content | Should -Match 'CODE_LEVEL_PASS|code-level-pass|AUTO-REPO'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $evidence | Should -Match 'runtime_diagnostics_ops_summary_contract:\s*runtime-api-diagnostics-ops-summary-contract-v1'
        $evidence | Should -Match 'hyperv_wmi_common_helper_contract:\s*hyperv-wmi-common-helper-contract-v1'
        $evidence | Should -Match 'host_ops_mutation_boundary_contract:\s*service-eventlog-firewall-truststore-credential-manager-data-root'
        $evidence | Should -Match 'packaging_release_next_trigger:\s*product-payload-change-after-04220-fullgate'
        $evidence | Should -Match 'public_boundary_workflow_run_id:\s*25933428239'
        $evidence | Should -Match 'public_boundary_check_run_id:\s*76232707240'
        $evidence | Should -Match 'public_boundary_workflow_status:\s*pass'
        $evidence | Should -Match 'previous_public_boundary_blocker_run_id:\s*25931297085'
        $evidence | Should -Not -Match '(?m)^public_boundary_blocker:\s*GitHub billing/spending-limit'
        $evidence | Should -Match 'host_mutation_performed:\s*false'

        $publicBoundaryPass | Should -Match 'result:\s*PASS'
        $publicBoundaryPass | Should -Match 'run_id:\s*25933428239'
        $publicBoundaryPass | Should -Match 'job_id:\s*76232707240'
        $publicBoundaryPass | Should -Match 'public-boundary-ci-required'
        $publicBoundaryPass | Should -Match 'billing_status:\s*resolved-for-actions-run'
        $publicBoundaryPass | Should -Match 'previous_blocker_runs:\s*25930077313,\s*25931297085,\s*25933236528'

        $apiContract | Should -Match 'RuntimeEvidenceContract'
        $apiContract | Should -Match 'runtime-api-diagnostics-ops-summary-contract-v1'
        $hypervCatalog | Should -Match 'DesktopNodeHyperVWmiHelperCatalog'
        $hypervCatalog | Should -Match 'hyperv-wmi-common-helper-contract-v1'
        $hostOpsCatalog | Should -Match 'MutationBoundaryForOperation'
        $hostOpsCatalog | Should -Match 'windows-credential-manager'
        $descriptorScript | Should -Match 'product-payload-change-after-04220-fullgate'

        $matrix | Should -Match 'post_04220_dev_slices:\s*code-level-pass'
        $matrix | Should -Match 'post_04220_public_boundary_workflow_rerun:\s*pass'
        $matrix | Should -Match 'post_04220_public_boundary_workflow_rerun_id:\s*25933428239'
        $descriptor | Should -Match 'post_04220_packaging_release_next_trigger:\s*`product-payload-change-after-04220-fullgate`'
        $descriptor | Should -Match 'post_04220_public_boundary_status:\s*`pass`'
        $classification | Should -Match 'Post-04220 development slices'
    }

    It 'records 0.42.20 manual-admin package-pair, full host mutation, and public-boundary pass rerun' {
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04219-04220.md'
        $hostMutationPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04220-hostmutation.md'
        $publicBoundaryPassPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-rerun-2026-05-16-04220-pass.md'

        $manualCampaignPath | Should -Exist
        $hostMutationPath | Should -Exist
        $publicBoundaryPassPath | Should -Exist

        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $hostMutation = Get-Content -Raw -LiteralPath $hostMutationPath
        $publicBoundaryPass = Get-Content -Raw -LiteralPath $publicBoundaryPassPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $verificationPolicy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $publicBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'

        foreach ($content in @($manualCampaign, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification)) {
            $content | Should -Match 'manual-admin-campaign-2026-05-16-04219-04220'
            $content | Should -Match '0\.42\.20-admin-smoke'
            $content | Should -Match '794953bcf3c8f05d1a424b7cc83c1e93e43898d1201c9dc64e32d3e17510b84f'
            $content | Should -Match '8076f838ee6c3c2451ca22ba0a86cc134f2d8e32509529c73e5895c5b105405b'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($hostMutation, $publicBoundaryPass, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $operationsGuide, $readme, $agents, $packagingReadme, $developerIndex, $adrIndex, $verificationPolicy, $publicBoundary)) {
            $content | Should -Match 'full-admin-host-mutation-gate-2026-05-16-04220-hostmutation'
            $content | Should -Match 'full-admin-host-mutation-gate-20260516-04220'
            $content | Should -Match '12b7baec853f07430581e14603ceb6debfb467ece8bb98a509b51cc365836e3c'
            $content | Should -Match '0895d018935298721b25b5d9ce1ae083a6690c25'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        $manualCampaign | Should -Match 'result:\s*PASS'
        $manualCampaign | Should -Match 'manual-admin-campaign-descriptor-20260516-04219-04220'
        $manualCampaign | Should -Match 'Burn'
        $manualCampaign | Should -Match 'MSIX'
        $manualCampaign | Should -Match 'installed runtime ops summary'
        $manualCampaign | Should -Match 'bc495a018b1522b7dbbe35538f1c4560a94e6a6f524e98ab8369ca029a4ff7e2'
        $manualCampaign | Should -Match '09bcb2e7867183e733e3401329fb61797eef7fe3ba55891d6585cd49a2cff81b'

        $hostMutation | Should -Match 'batch evidence status.*`available`|batch_evidence\.status=available'
        $hostMutation | Should -Match 'artifacts/installed-current-card-20260516-04220-fullgate'
        $hostMutation | Should -Match '25930077313'
        $hostMutation | Should -Match 'billing-or-spending-limit'
        $hostMutation | Should -Match '25933428239'
        $hostMutation | Should -Match 'public boundary guard executed.*`true`|public_boundary_guard_executed.*true'

        $publicBoundaryPass | Should -Match 'result:\s*PASS'
        $publicBoundaryPass | Should -Match 'run_id:\s*25933428239'
        $publicBoundaryPass | Should -Match 'head_sha:\s*6e556e5199e796a8889a9dc47dc925db02c9cb45'
        $publicBoundaryPass | Should -Match 'public_boundary_guard_executed:\s*true'

        $descriptor | Should -Match 'previous_04219_04220_status:\s*`closed-package-pair-04219-04220-pass`'
        $descriptor | Should -Match 'previous_04220_manual_admin_next_package_pair_candidate:\s*`pending-next-product-payload-after-04220-fullgate`'
        $descriptor | Should -Match 'latest_full_admin_gate_batch:\s*`full-admin-host-mutation-gate-20260528-04256`'
        $matrix | Should -Match 'manual_admin_04219_04220_campaign:\s*pass'
        $matrix | Should -Match 'manual_admin_04219_04220_missing_evidence:\s*none'
        $matrix | Should -Match 'previous_04220_product_payload_package_build:\s*0\.42\.20-admin-smoke'
        $matrix | Should -Match 'previous_product_payload_package_build:\s*0\.42\.19-admin-smoke'
        $matrix | Should -Match 'previous_04220_full_admin_host_mutation_gate_version:\s*0\.42\.20-admin-smoke'
        $matrix | Should -Match 'previous_04220_manual_admin_next_package_pair_candidate:\s*pending-next-product-payload-after-04220-fullgate'
        $matrix | Should -Match 'latest_public_boundary_workflow_rerun:\s*pass'
        $matrix | Should -Match 'latest_public_boundary_workflow_rerun_id:\s*25933428239'
        $classification | Should -Match 'Manual-admin 04219→04220 campaign'
        $classification | Should -Match 'billing blocker 해소|billing blocker resolved'
    }

    It 'records public-boundary CI maintenance, branch-protection fallback, and no package-build decision' {
        $mainPushEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04220-pass.md'
        $mainPushEvidencePath | Should -Exist

        $mainPushEvidence = Get-Content -Raw -LiteralPath $mainPushEvidencePath
        $ciWorkflow = Get-RepoText -RelativePath '.github/workflows/public-boundary.yml'
        $boundaryGuard = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'

        $ciWorkflow | Should -Match 'actions/checkout@v6\.0\.2'
        $ciWorkflow | Should -Not -Match 'actions/checkout@v4'

        foreach ($content in @($mainPushEvidence, $boundaryGuard, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-16-04220-pass'
            $content | Should -Match '25933861585'
            $content | Should -Match '76234195716'
            $content | Should -Match '686e4201f823295dc65cde302f613a982ab8cade'
            $content | Should -Match 'public-boundary-ci-required'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($developerIndex, $readme)) {
            $content | Should -Match 'public-boundary'
            $content | Should -Match 'actions/checkout@v6\.0\.2'
        }

        $mainPushEvidence | Should -Match 'result:\s*PASS'
        $mainPushEvidence | Should -Match 'checkout_action_version:\s*actions/checkout@v4'
        $mainPushEvidence | Should -Match 'checkout_maintenance_target:\s*actions/checkout@v6\.0\.2'
        $mainPushEvidence | Should -Match 'node20_deprecation_warning_observed:\s*true'
        $mainPushEvidence | Should -Match 'package_build_decision:\s*deferred-no-product-payload-change-after-04220'
        $mainPushEvidence | Should -Match 'branch_protection_ruleset_status:\s*unavailable-private-repo-plan'
        $mainPushEvidence | Should -Match 'fallback_required_guard:\s*public-boundary-ci-required'

        $boundaryGuard | Should -Match 'checkout_action_version:\s*actions/checkout@v6\.0\.2'
        $boundaryGuard | Should -Match 'branch_protection_ruleset_status:\s*unavailable-private-repo-plan'
        $boundaryGuard | Should -Match 'fallback_required_guard:\s*public-boundary-ci-required'
        $boundaryGuard | Should -Match 'latest_main_push_run_id:\s*26636072420'
        $boundaryGuard | Should -Match 'previous_main_push_run_id:\s*26629340294'
        $boundaryGuard | Should -Match 'previous_04254_running_cancel_main_push_run_id:\s*26556328902'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_latest_main_push_run_id:\s*26511891436'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_rollforward_main_push_run_id:\s*26510159990'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_initial_main_push_run_id:\s*26494683032'
        $boundaryGuard | Should -Match 'previous_04253_provider_latest_main_push_run_id:\s*26494136304'
        $boundaryGuard | Should -Match 'previous_04250_latest_main_push_run_id:\s*26489610881'
        $boundaryGuard | Should -Match 'previous_04245_latest_main_push_run_id:\s*26413569064'
        $boundaryGuard | Should -Match 'previous_pr168_latest_main_push_run_id:\s*26233838385'
        $boundaryGuard | Should -Match 'previous_pr156_latest_main_push_run_id:\s*26017721669'
        $boundaryGuard | Should -Match 'previous_pr155_latest_main_push_run_id:\s*26013384587'
        $boundaryGuard | Should -Match 'historical_pr149_main_push_run_id:\s*25974335803'
        $boundaryGuard | Should -Match 'historical_scope_lock_main_push_run_id:\s*25958514394'
        $boundaryGuard | Should -Match 'historical_successor_main_push_run_id:\s*25938745434'
        $boundaryGuard | Should -Match 'historical_checkout_main_push_run_id:\s*25934411998'
        $boundaryGuard | Should -Match 'historical_main_push_run_id:\s*25933861585'
        $matrix | Should -Match 'latest_public_boundary_workflow_main_push:\s*pass'
        $matrix | Should -Match 'historical_04223_public_boundary_workflow_main_push_id:\s*25954744127'
        $matrix | Should -Match 'historical_04222_public_boundary_workflow_main_push_id:\s*25952150476'
        $matrix | Should -Match 'historical_04220_public_boundary_workflow_main_push_id:\s*25933861585'
        $matrix | Should -Match 'public_boundary_checkout_action_version:\s*actions/checkout@v6\.0\.2'
        $matrix | Should -Match 'next_product_payload_package_build_decision:\s*executed-0\.42\.26-admin-smoke'
        $descriptor | Should -Match 'post_04220_public_boundary_previous_main_push_run_id:\s*`25933861585`'
        $descriptor | Should -Match 'post_04220_package_build_decision:\s*`deferred-no-product-payload-change-after-04220`'
        $classification | Should -Match 'Branch protection fallback'
        $classification | Should -Match 'checkout@v6\.0\.2'
    }

    It 'records post-ci-maintenance development slices and next product payload candidate selection' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-ci-maintenance-dev-slices-2026-05-16.md'
        $mainPushEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-checkout-v602-pass.md'
        $evidencePath | Should -Exist
        $mainPushEvidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $mainPushEvidence = Get-Content -Raw -LiteralPath $mainPushEvidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $apiContract = Get-RepoSourceText -RelativePath 'src/DesktopNode.Api/ApiHandlerAdapterContract.cs'
        $hypervCatalog = Get-RepoSourceText -RelativePath 'src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs'
        $hostOpsCatalog = Get-RepoSourceText -RelativePath 'src/DesktopNode.Host/Ops/DesktopNodeHostOpsCatalog.cs'
        $descriptorScript = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1'

        foreach ($content in @($evidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $developerIndex, $readme)) {
            $content | Should -Match 'post-ci-maintenance-dev-slices-2026-05-16'
            $content | Should -Match '0\.42\.20-admin-smoke'
            $content | Should -Match '0\.42\.21-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $mainPushEvidence | Should -Match 'run_id:\s*25934411998'
        $mainPushEvidence | Should -Match 'job_id:\s*76236050409'
        $mainPushEvidence | Should -Match 'head_sha:\s*3933231e6e2abf3a398dfcc3fdc999b3df38dac6'
        $mainPushEvidence | Should -Match 'checkout_action_version:\s*actions/checkout@v6\.0\.2'
        $mainPushEvidence | Should -Not -Match 'Node\.js 20 actions are deprecated'

        $evidence | Should -Match 'runtime_api_registry_bridge_contract:\s*runtime-api-diagnostics-ops-summary-registry-bridge-v2'
        $evidence | Should -Match 'hyperv_provider_callsite_guard:\s*hyperv-wmi-provider-callsite-drift-guard-v1'
        $evidence | Should -Match 'host_ops_reason_code_contract:\s*host-ops-dryrun-mutation-reason-code-v1'
        $evidence | Should -Match 'manual_admin_descriptor_generation_contract:\s*manual-admin-descriptor-generation-contract-v2'
        $evidence | Should -Match 'next_product_payload_candidate:\s*0\.42\.21-admin-smoke'
        $evidence | Should -Match 'host_mutation_performed:\s*false'

        $apiContract | Should -Match 'runtime-api-diagnostics-ops-summary-registry-bridge-v2'
        $apiContract | Should -Match 'HandlerRegistryRouteKeys'
        $hypervCatalog | Should -Match 'provider-set-factory-callsite-v1'
        $hypervCatalog | Should -Match 'ProviderSetPropertyName'
        $hostOpsCatalog | Should -Match 'HOST_OPS_DRY_RUN'
        $hostOpsCatalog | Should -Match 'MutationEvidenceReasonForOperation'
        $descriptorScript | Should -Match 'manual-admin-descriptor-generation-contract-v2'
        $descriptorScript | Should -Match 'candidate-selected-awaiting-package-build'

        $matrix | Should -Match 'post_ci_maintenance_dev_slices:\s*code-level-pass'
        $matrix | Should -Match 'next_product_payload_candidate:\s*0\.42\.21-admin-smoke'
        $descriptor | Should -Match 'post_ci_maintenance_dev_slices_status:\s*`code-level-pass`'
        $descriptor | Should -Match 'post_ci_maintenance_next_product_payload_candidate:\s*`0\.42\.21-admin-smoke`'
        $classification | Should -Match 'Post-ci-maintenance development slices'
    }

    It 'records 0.42.21 package pair full host mutation and post merge public boundary current cards' {
        $publicBoundaryEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-pass.md'
        $packageEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04221.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04220-04221.md'
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04221-hostmutation.md'

        $publicBoundaryEvidencePath | Should -Exist
        $packageEvidencePath | Should -Exist
        $manualCampaignPath | Should -Exist
        $hostMutationEvidencePath | Should -Exist

        $publicBoundaryEvidence = Get-Content -Raw -LiteralPath $publicBoundaryEvidencePath
        $packageEvidence = Get-Content -Raw -LiteralPath $packageEvidencePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'

        foreach ($content in @($publicBoundaryEvidence, $packageEvidence, $manualCampaign, $hostMutationEvidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification)) {
            $content | Should -Match '0\.42\.21-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        foreach ($content in @($readme, $packagingReadme)) {
            $content | Should -Match 'admin-smoke|manual-admin|full admin'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $publicBoundaryEvidence | Should -Match 'run_id:\s*25935332346'
        $publicBoundaryEvidence | Should -Match 'job_id:\s*76239201416'
        $publicBoundaryEvidence | Should -Match 'head_sha:\s*280780682df42322da51f5dbf442d4601530646e'
        $publicBoundaryEvidence | Should -Match 'checkout_action_version:\s*actions/checkout@v6\.0\.2'

        $packageEvidence | Should -Match 'package_build_decision:\s*executed-0\.42\.21-admin-smoke'
        $packageEvidence | Should -Match 'artifacts/admin-smoke-package-20260516-04221'
        $manualCampaign | Should -Match '0\.42\.20-admin-smoke -> 0\.42\.21-admin-smoke'
        $manualCampaign | Should -Match 'manual-admin-descriptor-generation-contract-v2'
        $manualCampaign | Should -Match 'manual-admin-campaign-descriptor-20260516-04220-04221'
        $manualCampaign | Should -Match 'installed update/rollback.*pass|installed update/rollback`? \| `pass`'
        $hostMutationEvidence | Should -Match 'full-admin-host-mutation-gate-20260516-04221'
        $hostMutationEvidence | Should -Match 'batch_evidence\.status.*available|batch evidence status.*available|batch_evidence.status`: `available'
        $hostMutationEvidence | Should -Match 'runtime_api_registry_bridge_contract:\s*runtime-api-diagnostics-ops-summary-registry-bridge-v2'

        $matrix | Should -Match 'latest_manual_admin_package_pair:\s*0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke'
        $matrix | Should -Match 'previous_04225_manual_admin_package_pair:\s*0\.42\.24-admin-smoke -> 0\.42\.25-admin-smoke'
        $matrix | Should -Match 'latest_full_admin_host_mutation:\s*0\.42\.56-admin-smoke'
        $descriptor | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke`'
        $classification | Should -Match 'Manual-admin 04220→04221 campaign'
    }

    It 'records 0.42.21 successor public boundary, installed operator surface, and next trigger' {
        $successorPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04221-successor-pass.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04221.md'
        $post04221Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04221-successor-operator-surface-2026-05-16.md'

        $successorPath | Should -Exist
        $operatorSurfacePath | Should -Exist
        $post04221Path | Should -Exist

        $successor = Get-Content -Raw -LiteralPath $successorPath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $post04221 = Get-Content -Raw -LiteralPath $post04221Path
        $boundaryGuard = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $webServed = Get-RepoText -RelativePath 'web/src/served-app.ts'
        $webApp = Get-RepoText -RelativePath 'web/app.js'

        foreach ($content in @($successor, $operatorSurface, $post04221, $boundaryGuard, $matrix, $descriptor, $classification)) {
            $content | Should -Match '25938745434'
            $content | Should -Match '76250726268'
            $content | Should -Match '0\.42\.21-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        $successor | Should -Match 'head_sha:\s*d0b12bd41e1104f68e5684aa797b8050286e6a69'
        $operatorSurface | Should -Match 'artifact_root:\s*artifacts/installed-operator-surface-current-card-20260516-04221'
        $operatorSurface | Should -Match 'tui_operator_smoke:\s*pass'
        $operatorSurface | Should -Match 'cli_ops_summary_ok:\s*true'
        $operatorSurface | Should -Match 'runtime_policy_unauthenticated_status_code:\s*401'
        $operatorSurface | Should -Match 'runtime_api_registry_bridge_contract:\s*runtime-api-diagnostics-ops-summary-registry-bridge-v2'
        $operatorSurface | Should -Match 'host_mutation_performed:\s*false'

        $post04221 | Should -Match 'web_console_diagnostics_registry_bridge_direct_expose:\s*code-level-applied'
        $post04221 | Should -Match 'next_product_payload_candidate:\s*0\.42\.22-admin-smoke'
        $post04221 | Should -Match 'next_package_build_decision:\s*deferred-open-candidate-after-04221-web-diagnostics-direct-expose'
        $matrix | Should -Match 'post_04221_successor_operator_surface:\s*code-level-and-operator-surface-pass'
        $matrix | Should -Match 'latest_full_admin_host_mutation_gate_version:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'next_product_payload_package_build_decision:\s*executed-0\.42\.26-admin-smoke'
        $matrix | Should -Match 'previous_04221_manual_admin_next_package_pair_candidate:\s*pending-next-product-payload-after-04221-fullgate'
        $descriptor | Should -Match 'previous_status:\s*`closed-package-pair-04224-04225-pass-and-04226-fullgate-current-card-pass-with-04225-04226-candidate-open`'
        $descriptor | Should -Match 'previous_04226_initial_manual_admin_next_package_pair_candidate:\s*`0\.42\.25-admin-smoke -> 0\.42\.26-admin-smoke`'
        $descriptor | Should -Match 'post_04222_manual_admin_descriptor_overall_status:\s*`historical-descriptor-generated-then-burn-blocked`'
        ($webServed + $webApp) | Should -Match 'renderRuntimeApiRegistryBridge'
        ($webServed + $webApp) | Should -Match 'Runtime/API registry bridge'
        ($webServed + $webApp) | Should -Match 'ops summary direct expose'
    }

    It 'records 0.42.22 package host mutation current-card and descriptor blocked rebaseline' {
        $packageEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04222.md'
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04222-hostmutation.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04222.md'
        $descriptorEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04221-04222.md'
        $publicPostMergePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04222-postmerge-pass.md'
        $aggregatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04222-package-host-mutation-current-card-2026-05-16.md'

        foreach ($path in @($packageEvidencePath, $hostMutationEvidencePath, $operatorSurfacePath, $descriptorEvidencePath, $publicPostMergePath, $aggregatePath)) {
            $path | Should -Exist
        }

        $packageEvidence = Get-Content -Raw -LiteralPath $packageEvidencePath
        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $descriptorEvidence = Get-Content -Raw -LiteralPath $descriptorEvidencePath
        $publicPostMerge = Get-Content -Raw -LiteralPath $publicPostMergePath
        $aggregate = Get-Content -Raw -LiteralPath $aggregatePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $adr0005 = Get-RepoText -RelativePath 'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'
        $boundaryGuard = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'

        foreach ($content in @($packageEvidence, $hostMutationEvidence, $operatorSurface, $descriptorEvidence, $aggregate, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adrIndex, $adr0005)) {
            $content | Should -Match '0\.42\.22-admin-smoke|04222'
            $content | Should -Match '8a38995cc25a888f64473e9a2869740949ad6b24'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        foreach ($content in @($packageEvidence, $hostMutationEvidence, $operatorSurface, $aggregate, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adrIndex, $adr0005)) {
            $content | Should -Match '68f8f37e2dd9d49bc07d8a404ba32e558efca1bb42038084a57ed7ba6ae18bf3'
        }

        foreach ($content in @($hostMutationEvidence, $operatorSurface, $aggregate, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adrIndex, $adr0005)) {
            $content | Should -Match '35055d4f7570a0be7d8c2232488b28862cb3bc8ae3e7d9eaa6b3cb8a945cf35c'
        }

        $packageEvidence | Should -Match 'package_build_decision:\s*executed-0\.42\.22-admin-smoke'
        $packageEvidence | Should -Match 'artifacts/admin-smoke-package-20260516-04222'
        $hostMutationEvidence | Should -Match 'batch_id:\s*full-admin-host-mutation-gate-20260516-04222'
        $hostMutationEvidence | Should -Match 'host_mutation_performed:\s*true'
        $hostMutationEvidence | Should -Match 'batch_evidence\.status:\s*available'
        $hostMutationEvidence | Should -Match 'runtime_api_registry_bridge_route_count:\s*4'
        $operatorSurface | Should -Match 'artifact_root:\s*artifacts/installed-operator-surface-current-card-20260516-04222'
        $operatorSurface | Should -Match 'latest_batch_id:\s*full-admin-host-mutation-gate-20260516-04222'
        $operatorSurface | Should -Match 'runtime_api_registry_bridge_route_count:\s*4'
        $operatorSurface | Should -Match 'tui_operator_smoke:\s*pass'
        $operatorSurface | Should -Match 'cli_ops_summary_ok:\s*true'

        $descriptorEvidence | Should -Match 'manual-admin-campaign-descriptor-20260516-04221-04222'
        $descriptorEvidence | Should -Match 'overall_status:\s*blocked-by-missing-evidence'
        $descriptorEvidence | Should -Match 'missing count.*`?4|missing_count.*4'
        $descriptorEvidence | Should -Match 'not-pass count.*`?1|not_pass_count.*1'
        $publicPostMerge | Should -Match 'run_id:\s*25952150476'
        $publicPostMerge | Should -Match 'job_id:\s*76291983316'
        $publicPostMerge | Should -Match 'head_sha:\s*4136bb1c70aace6adf36a79792fbc5c9bcb31d12'

        $aggregate | Should -Match 'PACKAGE_HOST_MUTATION_CURRENT_CARD_PASS_WITH_DESCRIPTOR_BLOCKED'
        $aggregate | Should -Match 'public_boundary_postmerge_evidence:\s*docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04222-postmerge-pass\.md'
        $aggregate | Should -Match 'GET /api/v1/ops/summary -> OpsSummary'
        $aggregate | Should -Match 'GET /api/v1/diagnostics/bundles'
        $aggregate | Should -Match 'POST /api/v1/diagnostics/bundles -> CreateDiagnosticBundle'

        $matrix | Should -Match 'latest_product_payload_package:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'latest_full_admin_host_mutation_gate_version:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'post_04222_manual_admin_descriptor_status:\s*superseded-by-04221-04222-burn-blocker'
        $matrix | Should -Match 'post_04222_runtime_api_registry_bridge_route_count:\s*4'
        $descriptor | Should -Match 'current_full_admin_host_mutation_batch:\s*`full-admin-host-mutation-gate-20260528-04256`'
        $descriptor | Should -Match 'post_04222_manual_admin_descriptor_overall_status:\s*`historical-descriptor-generated-then-burn-blocked`'
        $classification | Should -Match '0\.42\.22 package build/current-card host mutation'
        $classification | Should -Match 'Manual-admin 04221→04222 descriptor candidate'
        $boundaryGuard | Should -Match 'latest_main_push_run_id:\s*26636072420'
        $boundaryGuard | Should -Match 'previous_main_push_run_id:\s*26629340294'
        $boundaryGuard | Should -Match 'previous_04254_running_cancel_main_push_run_id:\s*26556328902'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_latest_main_push_run_id:\s*26511891436'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_rollforward_main_push_run_id:\s*26510159990'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_initial_main_push_run_id:\s*26494683032'
        $boundaryGuard | Should -Match 'previous_04253_provider_latest_main_push_run_id:\s*26494136304'
        $boundaryGuard | Should -Match 'previous_04250_latest_main_push_run_id:\s*26489610881'
        $boundaryGuard | Should -Match 'previous_04245_latest_main_push_run_id:\s*26413569064'
        $boundaryGuard | Should -Match 'previous_pr168_latest_main_push_run_id:\s*26233838385'
        $boundaryGuard | Should -Match 'previous_pr156_latest_main_push_run_id:\s*26017721669'
        $boundaryGuard | Should -Match 'previous_pr155_latest_main_push_run_id:\s*26013384587'
        $adr0005 | Should -Match '0\.42\.22-admin-smoke'
        $adr0005 | Should -Match 'public trusted signing, external stable\s+publication, winget submission'
    }

    It 'records 0.42.23 package-pair campaign closure and 0.42.21 to 0.42.22 Burn blocker' {
        $packageEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04223.md'
        $campaignEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04222-04223.md'
        $blockerEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04221-04222-burn-blocked.md'

        foreach ($path in @($packageEvidencePath, $campaignEvidencePath, $blockerEvidencePath)) {
            $path | Should -Exist
        }

        $packageEvidence = Get-Content -Raw -LiteralPath $packageEvidencePath
        $campaignEvidence = Get-Content -Raw -LiteralPath $campaignEvidencePath
        $blockerEvidence = Get-Content -Raw -LiteralPath $blockerEvidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $adr0005 = Get-RepoText -RelativePath 'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($packageEvidence, $campaignEvidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adr0005, $developerIndex, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.23-admin-smoke'
            $content | Should -Match '2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406'
            $content | Should -Match '676b4177b10dc80209969066857bab6008ff2473'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($campaignEvidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $agents, $developerIndex, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match '6f7e2caeb70aff8f5b26702693cf3b6f9a893217d87a0dc0a47f4f76e07fbddb'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260516-04222-04223-closed|manual-admin-campaign-descriptor-supervised/summary\.json'
        }

        $packageEvidence | Should -Match 'package_build_decision:\s*executed-0\.42\.23-admin-smoke'
        $packageEvidence | Should -Match 'Credential Manager default transition idempotence'
        $campaignEvidence | Should -Match '0\.42\.22-admin-smoke -> 0\.42\.23-admin-smoke'
        $campaignEvidence | Should -Match 'pass-with-windows-update'
        $campaignEvidence | Should -Match 'Burn bundle SHA-256'
        $campaignEvidence | Should -Match 'MSIX lifecycle'
        $campaignEvidence | Should -Match 'missing count \| `0`'
        $campaignEvidence | Should -Match 'not pass count \| `0`'
        $campaignEvidence | Should -Match 'runtime-api-diagnostics-ops-summary-registry-bridge-v2'
        $campaignEvidence | Should -Match 'PCV_AUTH_REQUIRED'

        $blockerEvidence | Should -Match 'BLOCKED_BY_BURN_CREDENTIAL_MANAGER_IDEMPOTENCE'
        $blockerEvidence | Should -Match '0\.42\.21-admin-smoke -> 0\.42\.22-admin-smoke'
        $blockerEvidence | Should -Match 'exit `1603`'
        $blockerEvidence | Should -Match 'CredentialManagerDefaultTransition'
        $blockerEvidence | Should -Match 'superseded_by:\s*manual-admin-campaign-2026-05-16-04222-04223'

        $matrix | Should -Match 'latest_product_payload_package:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'latest_manual_admin_package_pair:\s*0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke'
        $matrix | Should -Match 'previous_04225_manual_admin_package_pair:\s*0\.42\.24-admin-smoke -> 0\.42\.25-admin-smoke'
        $matrix | Should -Match 'post_04223_manual_admin_descriptor_missing_count:\s*0'
        $matrix | Should -Match 'post_04221_04222_burn_blocker_evidence'
        $descriptor | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke`'
        $descriptor | Should -Match 'post_04223_manual_admin_descriptor_overall_status:\s*`pass`'
        $classification | Should -Match '0\.42\.23 package build/manual-admin package-pair'
    }

    It 'records 0.42.23 full host mutation current-card public boundary and next slice selection' {
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04223-hostmutation.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04223.md'
        $publicPostMergePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04223-postmerge-pass.md'
        $aggregatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04223-full-host-mutation-current-card-2026-05-16.md'

        foreach ($path in @($hostMutationEvidencePath, $operatorSurfacePath, $publicPostMergePath, $aggregatePath)) {
            $path | Should -Exist
        }

        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $publicPostMerge = Get-Content -Raw -LiteralPath $publicPostMergePath
        $aggregate = Get-Content -Raw -LiteralPath $aggregatePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $boundaryGuard = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $adr0005 = Get-RepoText -RelativePath 'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($hostMutationEvidence, $operatorSurface, $aggregate, $matrix, $descriptor, $adr0005, $developerIndex, $releaseBoundary, $installerReadme)) {
            $content | Should -Match '0\.42\.23-admin-smoke'
            $content | Should -Match 'ce0fb3e95c41310a70fe14fa42470670fe7d3622d06b52de3fea36dad87ed932'
            $content | Should -Match '2a628547ad506b0ed59e4dfef1c91c3db5d6c090e79901f5460fc76d48594406'
            $content | Should -Match 'd11a096086326004f27facd9612c2296ded15a4b'
            $content | Should -Match '676b4177b10dc80209969066857bab6008ff2473'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        foreach ($content in @($publicPostMerge, $matrix, $descriptor, $classification, $boundaryGuard, $developerIndex, $releaseBoundary, $installerReadme)) {
            $content | Should -Match '25954744127'
            $content | Should -Match '76299282407'
        }

        $hostMutationEvidence | Should -Match 'batch_id:\s*full-admin-host-mutation-gate-20260516-04223'
        $hostMutationEvidence | Should -Match 'host_mutation_performed:\s*true'
        $hostMutationEvidence | Should -Match 'batch_evidence\.status:\s*available'
        $hostMutationEvidence | Should -Match 'runtime_api_registry_bridge_route_count:\s*4'
        $operatorSurface | Should -Match 'artifact_root:\s*artifacts/installed-operator-surface-current-card-20260516-04223'
        $operatorSurface | Should -Match 'latest_batch_id:\s*full-admin-host-mutation-gate-20260516-04223'
        $operatorSurface | Should -Match 'service_token_storage:\s*windows-credential-manager'
        $operatorSurface | Should -Match 'tui_operator_smoke:\s*pass'
        $operatorSurface | Should -Match 'cli_ops_summary_ok:\s*true'
        $operatorSurface | Should -Match 'token_value_observed:\s*false'
        $publicPostMerge | Should -Match 'head_sha:\s*d11a096086326004f27facd9612c2296ded15a4b'
        $publicPostMerge | Should -Match 'checkout_action_version:\s*actions/checkout@v6\.0\.2'
        $aggregate | Should -Match 'FULL_HOST_MUTATION_CURRENT_CARD_PASS_NEXT_SLICE_SELECTED'
        $aggregate | Should -Match 'next_product_payload_candidate:\s*0\.42\.24-admin-smoke'
        $aggregate | Should -Match '0\.42\.23-admin-smoke -> 0\.42\.24-admin-smoke'
        $aggregate | Should -Match 'stale local codex branch cleanup'
        $aggregate | Should -Match '12개 삭제'
        $matrix | Should -Match 'latest_full_admin_host_mutation_gate_version:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'post_04223_stale_local_codex_branch_cleanup_deleted_count:\s*12'
        $descriptor | Should -Match 'post_04223_next_product_payload_candidate:\s*`0\.42\.24-admin-smoke`'
        $descriptor | Should -Match 'post_04223_next_slice_runtime_api:\s*`current-evidence-rollup`'
        $classification | Should -Match '0\.42\.23 full host mutation/current-card follow-up'
        $boundaryGuard | Should -Match 'latest_main_push_run_id:\s*26636072420'
        $boundaryGuard | Should -Match 'previous_main_push_run_id:\s*26629340294'
        $boundaryGuard | Should -Match 'previous_04254_running_cancel_main_push_run_id:\s*26556328902'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_latest_main_push_run_id:\s*26511891436'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_rollforward_main_push_run_id:\s*26510159990'
        $boundaryGuard | Should -Match 'previous_04253_evidence_closure_initial_main_push_run_id:\s*26494683032'
        $boundaryGuard | Should -Match 'previous_04253_provider_latest_main_push_run_id:\s*26494136304'
        $boundaryGuard | Should -Match 'previous_04250_latest_main_push_run_id:\s*26489610881'
        $boundaryGuard | Should -Match 'previous_04245_latest_main_push_run_id:\s*26413569064'
        $boundaryGuard | Should -Match 'previous_pr168_latest_main_push_run_id:\s*26233838385'
        $boundaryGuard | Should -Match 'previous_pr156_latest_main_push_run_id:\s*26017721669'
        $boundaryGuard | Should -Match 'previous_pr155_latest_main_push_run_id:\s*26013384587'
    }

    It 'records 0.42.24 current evidence rollup package fullgate descriptor and installed current-card' {
        $packageEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04224.md'
        $descriptorEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04223-04224.md'
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04224-hostmutation.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04224.md'

        foreach ($path in @($packageEvidencePath, $descriptorEvidencePath, $hostMutationEvidencePath, $operatorSurfacePath)) {
            $path | Should -Exist
        }

        $packageEvidence = Get-Content -Raw -LiteralPath $packageEvidencePath
        $descriptorEvidence = Get-Content -Raw -LiteralPath $descriptorEvidencePath
        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($packageEvidence, $hostMutationEvidence, $operatorSurface, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adrIndex, $readme, $agents, $developerIndex, $guide, $operations, $policy, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.24-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        foreach ($content in @($packageEvidence, $readme, $agents, $guide, $installerReadme)) {
            $content | Should -Match 'd2ffa8bb10e064cb9b0a0fc6c853835d7a571a9318ce29fd734140de2c0c766e'
            $content | Should -Match 'b974d6b541423f2e4160f726f96155b16f105e9d'
        }

        foreach ($content in @($hostMutationEvidence, $operatorSurface, $evidenceIndex, $controlPlaneIndex, $matrix, $classification, $adrIndex, $readme, $agents, $developerIndex, $guide, $operations, $policy, $installerReadme)) {
            $content | Should -Match '0147b3d97647e921fe39bc5a667e6790d8e3b1af0b36a579de988f1d04d74826'
            $content | Should -Match 'full-admin-host-mutation-gate-20260516-04224'
            $content | Should -Match 'runtime-api-current-evidence-rollup-v1'
        }

        foreach ($content in @($descriptorEvidence, $matrix, $descriptor, $classification, $adrIndex, $readme, $agents, $guide, $policy)) {
            $content | Should -Match 'manual-admin-campaign-descriptor-(2026-05-16-04223-04224|20260516-04223-04224)'
            $content | Should -Match 'blocked-by-missing-evidence'
        }

        $packageEvidence | Should -Match 'package_build_decision:\s*executed-0\.42\.24-admin-smoke'
        $packageEvidence | Should -Match 'artifact_root:\s*artifacts/admin-smoke-package-20260516-04224'
        $descriptorEvidence | Should -Match '0\.42\.23-admin-smoke -> 0\.42\.24-admin-smoke'
        $descriptorEvidence | Should -Match 'missing count \| `5`|missing_count.*5'
        $descriptorEvidence | Should -Match 'not-pass count \| `1`|not_pass_count.*1'
        $hostMutationEvidence | Should -Match 'host_mutation_performed:\s*true'
        $hostMutationEvidence | Should -Match 'batch_evidence\.status:\s*available'
        $hostMutationEvidence | Should -Match 'runtime_api_registry_bridge_route_count:\s*4'
        $operatorSurface | Should -Match 'artifact_root:\s*artifacts/installed-operator-surface-current-card-20260516-04224'
        $operatorSurface | Should -Match 'latest_batch_id:\s*full-admin-host-mutation-gate-20260516-04224'
        $operatorSurface | Should -Match 'web_console_status_code:\s*200'
        $operatorSurface | Should -Match 'pcv_config_status_code:\s*200'
        $operatorSurface | Should -Match 'runtime_policy_unauthenticated_error_code:\s*PCV_AUTH_REQUIRED'
        $operatorSurface | Should -Match 'cli_ops_summary_ok:\s*true'
        $operatorSurface | Should -Match 'tui_operator_smoke:\s*pass'
        $operatorSurface | Should -Match 'token_value_observed:\s*false'
        $matrix | Should -Match 'latest_product_payload_package:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'latest_full_admin_host_mutation_gate_version:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'post_04224_manual_admin_descriptor_missing_count:\s*5'
        $matrix | Should -Match 'post_04224_manual_admin_descriptor_not_pass_count:\s*1'
        $descriptor | Should -Match 'current_full_admin_host_mutation_batch:\s*`full-admin-host-mutation-gate-20260528-04256`'
        $descriptor | Should -Match 'post_04224_manual_admin_descriptor_missing_count:\s*`5`'
        $descriptor | Should -Match 'manual_admin_next_package_pair_descriptor_missing_count:\s*`0`'
        $descriptor | Should -Match 'manual_admin_next_package_pair_descriptor_not_pass_count:\s*`0`'
        $descriptor | Should -Match 'previous_04226_initial_manual_admin_next_package_pair_descriptor_missing_count:\s*`4`'
        $releaseBoundary | Should -Match '0\.42\.24-admin-smoke'
        $releaseBoundary | Should -Match 'public stable installer URL'
    }

    It 'records 0.42.25 fullgate current-card manual-admin closure and public boundary' {
        $packageEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-16-04225.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-16-04224-04225.md'
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04225-hostmutation.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04225.md'
        $publicBoundaryEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-16-04225-postmerge-pass.md'

        foreach ($path in @($packageEvidencePath, $manualCampaignPath, $hostMutationEvidencePath, $operatorSurfacePath, $publicBoundaryEvidencePath)) {
            $path | Should -Exist
        }

        $packageEvidence = Get-Content -Raw -LiteralPath $packageEvidencePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $publicBoundaryEvidence = Get-Content -Raw -LiteralPath $publicBoundaryEvidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundaryContract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'
        $apiReader = Get-RepoSourceText -RelativePath 'src/DesktopNode.Api/BatchEvidenceSummaryReader.cs'
        $apiBuilder = Get-RepoSourceText -RelativePath 'src/DesktopNode.Api/DesktopNodeApiOpsSummaryBuilder.cs'
        $webServed = Get-RepoText -RelativePath 'web/src/served-app.ts'
        $webFixtures = Get-RepoText -RelativePath 'web/src/user-visible-fixtures.ts'
        $webStaticTests = Get-RepoText -RelativePath 'web/tests/PcvDesktopWeb.Static.Tests.ps1'

        foreach ($content in @($packageEvidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adrIndex, $readme, $agents, $guide, $operations, $policy, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.25-admin-smoke'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $developerIndex | Should -Match 'manual-admin-campaign-2026-05-16-04224-04225'
        $developerIndex | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'

        foreach ($content in @($packageEvidence, $readme, $guide, $installerReadme)) {
            $content | Should -Match '5a3e8494dfaf756f57a4e3d193dc310afa5e45bcbf2497a1c51c8ccd47902d06'
            $content | Should -Match '403d4474c4b88136774600cc81ca2d941c0b5e4b'
        }

        foreach ($content in @($manualCampaign, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adrIndex, $readme, $agents, $developerIndex, $guide, $operations, $policy, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.24(-admin-smoke)?\s*(->|→)\s*0\.42\.25(-admin-smoke)?|04224→04225|04224-04225'
            $content | Should -Match 'manual-admin-campaign-descriptor-(20260516-04224-04225-closed|2026-05-16-04224-04225)'
            $content | Should -Match 'missing_count=0|missing_count\s*[:=]?\s*`?0`?|missing count[: ]+\|? `0`|missing count\s+`0`'
            $content | Should -Match 'not_pass_count=0|not_pass_count\s*[:=]?\s*`?0`?|not-pass count[: ]+\|? `0`|not-pass count\s+`0`'
        }

        foreach ($content in @($manualCampaign, $hostMutationEvidence, $operatorSurface, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $adrIndex, $readme, $agents, $developerIndex, $guide, $operations, $policy, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match 'e80ebbf3647e982c6d83b31ff5564468811e787a12ca10117eba13299150416b'
            $content | Should -Match '4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1'
        }

        foreach ($content in @($publicBoundaryEvidence, $matrix, $descriptor, $classification, $publicBoundaryContract, $releaseBoundary)) {
            $content | Should -Match '25959505688'
            $content | Should -Match '76312299500'
            $content | Should -Match '4b82aa4c50be78d5d52d91cbb9b1e80397a0c0a1'
        }

        foreach ($content in @($evidenceIndex, $controlPlaneIndex, $readme, $developerIndex)) {
            $content | Should -Match 'public-boundary'
            $content | Should -Match '04225|0\.42\.25-admin-smoke|PR #144|previous'
        }

        $packageEvidence | Should -Match 'package_build_decision:\s*executed-0\.42\.25-admin-smoke'
        $packageEvidence | Should -Match 'artifact_root:\s*artifacts/admin-smoke-package-20260516-04225'
        $manualCampaign | Should -Match 'readiness.*PASS|readiness_status:\s*pass'
        $manualCampaign | Should -Match 'installed update/rollback|product update/rollback'
        $manualCampaign | Should -Match 'clean-host|Windows Update'
        $manualCampaign | Should -Match 'Burn'
        $manualCampaign | Should -Match 'MSIX'
        $manualCampaign | Should -Match 'manual-admin-campaign-descriptor-20260516-04224-04225-closed'
        $hostMutationEvidence | Should -Match 'host_mutation_performed:\s*true'
        $hostMutationEvidence | Should -Match 'full-admin-host-mutation-gate-20260516-04225'
        $operatorSurface | Should -Match 'artifact_root:\s*artifacts/installed-operator-surface-current-card-20260516-04225'
        $operatorSurface | Should -Match 'latest_batch_id:\s*full-admin-host-mutation-gate-20260516-04225'
        $operatorSurface | Should -Match 'web_console_status_code:\s*200'
        $operatorSurface | Should -Match 'cli_ops_summary_ok:\s*true'
        $operatorSurface | Should -Match 'tui_operator_smoke:\s*pass'
        $publicBoundaryEvidence | Should -Match 'public_boundary_guard_executed:\s*true'
        $publicBoundaryContract | Should -Match 'latest_main_push_run_id:\s*26636072420'
        $publicBoundaryContract | Should -Match 'previous_main_push_run_id:\s*26629340294'
        $publicBoundaryContract | Should -Match 'previous_04254_running_cancel_main_push_run_id:\s*26556328902'
        $publicBoundaryContract | Should -Match 'previous_04253_evidence_closure_latest_main_push_run_id:\s*26511891436'
        $publicBoundaryContract | Should -Match 'previous_04253_evidence_closure_rollforward_main_push_run_id:\s*26510159990'
        $publicBoundaryContract | Should -Match 'previous_04253_evidence_closure_initial_main_push_run_id:\s*26494683032'
        $publicBoundaryContract | Should -Match 'previous_04253_provider_latest_main_push_run_id:\s*26494136304'
        $publicBoundaryContract | Should -Match 'previous_04250_latest_main_push_run_id:\s*26489610881'
        $publicBoundaryContract | Should -Match 'previous_04245_latest_main_push_run_id:\s*26413569064'
        $publicBoundaryContract | Should -Match 'previous_pr168_latest_main_push_run_id:\s*26233838385'
        $publicBoundaryContract | Should -Match 'previous_pr156_latest_main_push_run_id:\s*26017721669'
        $publicBoundaryContract | Should -Match 'historical_pr149_main_push_run_id:\s*25974335803'
        $publicBoundaryContract | Should -Match 'historical_scope_lock_main_push_run_id:\s*25958514394'
        $publicBoundaryContract | Should -Match 'previous_package_build_decision:\s*executed-0\.42\.29-admin-smoke-after-selector-package-chain-payload-change'
        $matrix | Should -Match 'latest_product_payload_package:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'latest_full_admin_host_mutation_gate_version:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'post_04225_manual_admin_descriptor_missing_count:\s*0'
        $matrix | Should -Match 'post_04225_manual_admin_descriptor_not_pass_count:\s*0'
        $descriptor | Should -Match 'manual_admin_next_package_pair_readiness_status:\s*`pass`'
        $descriptor | Should -Match 'manual_admin_next_package_pair_descriptor_missing_count:\s*`0`'
        $descriptor | Should -Match 'previous_04226_initial_manual_admin_next_package_pair_descriptor_missing_count:\s*`4`'
        $descriptor | Should -Match 'post_0425_manual_admin_descriptor_missing_count:\s*`0`'
        $apiReader | Should -Match 'batch_evidence_artifact'
        $apiReader | Should -Match 'artifact-discovered'
        $apiReader | Should -Match 'hasBatchSupervisorShape'
        $apiBuilder | Should -Match 'tracked-in-documentation'
        ($webServed + $webFixtures + $webStaticTests) | Should -Match 'artifact-discovered'
        ($webServed + $webFixtures + $webStaticTests) | Should -Match 'batch_evidence_artifact'
    }

    It 'records 0.42.26 manual-admin package-pair closure and current-card recheck' {
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md'
        $initialDescriptorPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-descriptor-2026-05-16-04225-04226.md'
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-16-04226.md'

        foreach ($path in @($manualCampaignPath, $initialDescriptorPath, $hostMutationEvidencePath, $operatorSurfacePath)) {
            $path | Should -Exist
        }

        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $initialDescriptor = Get-Content -Raw -LiteralPath $initialDescriptorPath
        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($manualCampaign, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $readme, $agents, $developerIndex, $guide, $operations, $policy, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.25-admin-smoke\s*(->|→)\s*0\.42\.26-admin-smoke|04225→04226|04225-04226'
            $content | Should -Match 'manual-admin-campaign-2026-05-17-04225-04226|manual-admin-campaign-descriptor-20260517-04225-04226-closed'
            $content | Should -Match '4476880ba536db430e2bc3d9245063c904c203dc1c013e4e93057171866d6fe4'
            $content | Should -Match 'f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7'
            $content | Should -Match 'd6500c01c972cbc7ca1e290e51120181ceea1501'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $manualCampaign | Should -Match 'result:\s*PASS'
        $manualCampaign | Should -Match 'descriptor_missing_count:\s*0'
        $manualCampaign | Should -Match 'descriptor_not_pass_count:\s*0'
        $manualCampaign | Should -Match 'current-card-recheck-after-docs/summary\.json'
        $manualCampaign | Should -Match 'runtime-api-current-evidence-rollup-v1'
        $manualCampaign | Should -Match 'runtime-api-diagnostics-ops-summary-registry-bridge-v2'
        $manualCampaign | Should -Match 'latest manual-admin package-pair'
        $initialDescriptor | Should -Match 'BLOCKED_BY_MISSING_EVIDENCE'
        $initialDescriptor | Should -Match '2026-05-17 installed update/rollback'
        $initialDescriptor | Should -Match 'PASS\s+evidence로 승격'
        $hostMutationEvidence | Should -Match 'full-admin-host-mutation-gate-20260516-04226'
        $operatorSurface | Should -Match 'latest_batch_id:\s*full-admin-host-mutation-gate-20260516-04226'
        $matrix | Should -Match 'latest_manual_admin_package_pair:\s*0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke'
        $matrix | Should -Match 'post_04226_manual_admin_closed_descriptor_missing_count:\s*0'
        $matrix | Should -Match 'post_04226_manual_admin_closed_descriptor_not_pass_count:\s*0'
        $descriptor | Should -Match 'previous_04226_status:\s*`closed-package-pair-04225-04226-pass-and-04226-fullgate-current-card-pass-awaiting-next-product-payload`'
        $descriptor | Should -Match 'previous_04226_manual_admin_next_package_pair_candidate:\s*`pending-next-product-payload-after-04226-package-pair`'
    }

    It 'records post-04226 current evidence ledger contract hardening and next payload trigger' {
        $ledgerPath = Join-Path $script:RepoRoot 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04226-ledger-contract-followup-2026-05-17.md'

        $ledgerPath | Should -Exist
        $followupPath | Should -Exist

        $ledger = Get-Content -Raw -LiteralPath $ledgerPath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $apiReader = Get-RepoSourceText -RelativePath 'src/DesktopNode.Api/BatchEvidenceSummaryReader.cs'
        $apiBuilder = Get-RepoSourceText -RelativePath 'src/DesktopNode.Api/DesktopNodeApiOpsSummaryBuilder.cs'
        $descriptorScript = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1'
        $batchSupervisor = Get-RepoText -RelativePath 'packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1'
        $webServed = Get-RepoText -RelativePath 'web/src/served-app.ts'
        $webStatic = Get-RepoText -RelativePath 'web/app.js'

        foreach ($content in @($ledger, $followup, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $developerIndex)) {
            $content | Should -Match 'current-evidence-ledger-2026-05-17-04226|CURRENT_EVIDENCE_LEDGER'
            $content | Should -Match 'post-04226-ledger-contract-followup-2026-05-17'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260517-04225-04226-closed'
            $content | Should -Match 'runtime-api-current-evidence-rollup-v1'
            $content | Should -Match 'manual-admin-descriptor-generation-contract-v2'
            $content | Should -Match 'post-04226-ledger-contract-merge'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($ledger, $followup, $matrix, $descriptor, $classification, $developerIndex)) {
            $content | Should -Match 'current_card_descriptor_batch_id'
            $content | Should -Match 'descriptor_schema_version[=:]?\s*`?2`?|manual_admin_descriptor_schema_version:\s*`2`|manual_admin_descriptor_schema_version:\s*2'
            $content | Should -Match 'pre_branch_product_payload_change_detected:\s*`?false`?|post_04226_pre_branch_product_payload_change_detected:\s*`?false`?|product payload.*false'
        }

        ($apiReader + $apiBuilder + $webServed + $webStatic) | Should -Match 'current_card_descriptor_batch_id'
        $apiReader | Should -Match 'InferDescriptorBatchIdFromCampaignPath'
        $descriptorScript | Should -Match 'descriptor_schema_version'
        $descriptorScript | Should -Match 'DescriptorBatchId'
        $batchSupervisor | Should -Match '-DescriptorBatchId'
        $followup | Should -Match 'host_mutation_performed:\s*`false`'
        $followup | Should -Match 'next_product_payload_package_build_trigger:\s*`post-04226-ledger-contract-merge`'
    }

    It 'records 0.42.27 Host Ops lifecycle package chain and current-card recheck' {
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-17-04227.md'
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-17-04227-hostmutation.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04226-04227.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04227.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04227-hostops-lifecycle-followup-2026-05-17.md'

        foreach ($path in @($packagePath, $hostMutationEvidencePath, $manualCampaignPath, $operatorSurfacePath, $followupPath)) {
            $path | Should -Exist
        }

        $package = Get-Content -Raw -LiteralPath $packagePath
        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($manualCampaign, $operatorSurface, $followup, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $readme, $agents, $developerIndex, $guide, $operations, $policy, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.26-admin-smoke\s*(->|→)\s*0\.42\.27-admin-smoke|04226→04227|04226-04227'
            $content | Should -Match 'manual-admin-campaign-2026-05-17-04226-04227|manual-admin-campaign-descriptor-20260517-04226-04227-closed'
            $content | Should -Match 'host-ops-lifecycle-descriptor-bridge-v1'
            $content | Should -Match 'service-action-eventlog-firewall-truststore-credential-manager-data-root-separated'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($manualCampaign, $operatorSurface, $followup, $matrix, $descriptor, $agents, $developerIndex, $guide, $operations, $policy)) {
            $content | Should -Match '7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9'
            $content | Should -Match '5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997'
            $content | Should -Match '69aba3eb3ff08c843f1a481818ddc86eac2f019b'
        }

        $package | Should -Match 'package_build_decision:\s*`executed-0\.42\.27-admin-smoke`'
        $package | Should -Match '0084d6ded5723ceb378c0805b9e9369e6626460bd6185d98e0a1028050f6be4a'
        $hostMutationEvidence | Should -Match 'full-admin-host-mutation-gate-20260517-04227'
        $hostMutationEvidence | Should -Match 'host_mutation_performed:\s*`true`'
        $manualCampaign | Should -Match 'result:\s*`PASS`|result:\s*PASS'
        $manualCampaign | Should -Match 'post-windows-update-heartbeat-no-contact-cpu-idle'
        $manualCampaign | Should -Match 'descriptor_missing_count:\s*`?0`?'
        $manualCampaign | Should -Match 'descriptor_not_pass_count:\s*`?0`?'
        $operatorSurface | Should -Match 'latest_batch_id:\s*`full-admin-host-mutation-gate-20260517-04227`'
        $operatorSurface | Should -Match 'host_ops_lifecycle_bucket_count:\s*`?6`?'
        $operatorSurface | Should -Match 'manual_admin_descriptor_missing_count:\s*`?0`?'
        $operatorSurface | Should -Match 'manual_admin_descriptor_not_pass_count:\s*`?0`?'
        $ledger | Should -Match 'current-evidence-ledger-2026-05-17-04227'
        $matrix | Should -Match 'previous_04227_latest_manual_admin_package_pair:\s*0\.42\.26-admin-smoke -> 0\.42\.27-admin-smoke'
        $matrix | Should -Match 'post_04227_manual_admin_closed_descriptor_missing_count:\s*0'
        $matrix | Should -Match 'post_04227_manual_admin_closed_descriptor_not_pass_count:\s*0'
        $descriptor | Should -Match 'previous_04227_status:\s*`closed-package-pair-04226-04227-pass-and-04228-fullgate-current-card-pass-awaiting-04227-04228-package-pair`'
        $descriptor | Should -Match 'previous_04227_manual_admin_next_package_pair_candidate:\s*`0\.42\.27-admin-smoke -> 0\.42\.28-admin-smoke`'
        $followup | Should -Match 'next_product_payload_package_build_trigger:\s*`next-product-payload-change-after-04227-package-pair`'
    }

    It 'preserves PR 150 post-merge public boundary closure as historical pre-04228 evidence' {
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr150-postmerge-pass.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04227-pr150-public-boundary-followup-2026-05-17.md'

        foreach ($path in @($publicBoundaryPath, $followupPath)) {
            $path | Should -Exist
        }

        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        foreach ($content in @($publicBoundary, $followup)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-17-04227-pr150-postmerge-pass'
            $content | Should -Match '25983307305'
            $content | Should -Match '76375957834'
            $content | Should -Match '6d4b5d95742044bdbd8def933fbc8cdefbba71b3'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $publicBoundary | Should -Match 'result:\s*PASS'
        $publicBoundary | Should -Match 'public_boundary_guard_executed:\s*true'
        $publicBoundary | Should -Match 'actions/checkout@v6\.0\.2'
        $publicBoundary | Should -Match 'Run public boundary evidence guard'
        $publicBoundary | Should -Match 'Verify packaging regression required step'
        $followup | Should -Match 'host_mutation_performed:\s*`false`'
        $followup | Should -Match 'product_payload_change_detected:\s*`false`'
        $followup | Should -Match 'package_chain_decision:\s*`deferred-0\.42\.28-admin-smoke-until-next-product-payload-change`'
        $followup | Should -Match 'host_ops_web_diagnostics_bucket_table_review:\s*`reviewed-deferred-next-operator-surface-product-payload-change`'
        $followup | Should -Match 'next_operator_surface_installed_account_novnc_smoke_trigger:\s*`next-operator-surface-product-payload-change`'
    }

    It 'records PR 151 public boundary and 0.42.28 Operator Surface package chain' {
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass.md'
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-17-04228.md'
        $hostMutationPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-17-04228-hostmutation.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04228.md'
        $accountNoVncPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-17-04228.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04228-operator-surface-admin-smoke-2026-05-17.md'

        foreach ($path in @($publicBoundaryPath, $packagePath, $hostMutationPath, $operatorSurfacePath, $accountNoVncPath, $followupPath)) {
            $path | Should -Exist
        }

        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $package = Get-Content -Raw -LiteralPath $packagePath
        $hostMutation = Get-Content -Raw -LiteralPath $hostMutationPath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $accountNoVnc = Get-Content -Raw -LiteralPath $accountNoVncPath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundaryContract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $adr0006 = Get-RepoText -RelativePath 'docs/adr/0006-internal-private-network-distribution.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($publicBoundary, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $publicBoundaryContract, $releaseBoundary, $readme, $agents, $packagingReadme, $installerReadme)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-17-04227-pr151-postmerge-pass'
            $content | Should -Match '25984814303'
            $content | Should -Match '76380096421'
            $content | Should -Match '26ae50fa7bef11b4919b441e706bde505463aded'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($package, $hostMutation, $operatorSurface, $accountNoVnc, $followup, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $releaseBoundary, $adr0006, $readme, $agents, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.28-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260517-04228'
            $content | Should -Match '223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e'
            $content | Should -Match 'a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74'
            $content | Should -Match 'b9676f6dc37d667ae0d60367e9f4e576a27e3864'
            $content | Should -Match 'host-ops-web-diagnostics-bucket-table-v1'
        }

        $package | Should -Match 'package_build_decision:\s*`executed-0\.42\.28-admin-smoke`'
        $hostMutation | Should -Match 'host_mutation_performed:\s*`true`'
        $operatorSurface | Should -Match 'latest_batch_id:\s*`full-admin-host-mutation-gate-20260517-04228`'
        $operatorSurface | Should -Match 'host_ops_lifecycle_bucket_count:\s*`?6`?'
        $accountNoVnc | Should -Match 'installed_account_login_smoke.*20260517-04228'
        $accountNoVnc | Should -Match 'target_backed_novnc.*20260517-04228'
        $accountNoVnc | Should -Match 'token/password 노출.*`false`'
        $ledger | Should -Match 'previous_04228_ledger_id:\s*`current-evidence-ledger-2026-05-17-04228`'
        $descriptor | Should -Match 'previous_04227_manual_admin_next_package_pair_candidate:\s*`0\.42\.27-admin-smoke -> 0\.42\.28-admin-smoke`'
        $matrix | Should -Match 'post_04227_04228_package_chain_status:\s*executed-after-operator-surface-product-payload-change|previous_post_04227_04228_package_chain_status:\s*closed-manual-admin-package-pair-04227-04228'
        $publicMatrix | Should -Match 'post_04227_04228_package_chain_status:\s*historical-closed-manual-admin-package-pair-04227-04228|previous_post_04227_04228_package_chain_status:\s*closed-manual-admin-package-pair-04227-04228'
        $publicBoundaryContract | Should -Match 'previous_package_build_decision:\s*executed-0\.42\.29-admin-smoke-after-selector-package-chain-payload-change'
        $adr0006 | Should -Match 'historical_04228_internal_admin_smoke:\s*0\.42\.28-admin-smoke|previous_scope_lock_internal_admin_smoke:\s*0\.42\.28-admin-smoke'
        $followup | Should -Match 'next_manual_admin_package_pair_candidate:\s*`0\.42\.27-admin-smoke -> 0\.42\.28-admin-smoke`'
    }

    It 'records 0.42.28 manual-admin package-pair closure and PR 152 public boundary current evidence' {
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04227-04228.md'
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04228.md'
        $accountNoVncPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-17-04228.md'

        foreach ($path in @($manualCampaignPath, $publicBoundaryPath, $operatorSurfacePath, $accountNoVncPath)) {
            $path | Should -Exist
        }

        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $accountNoVnc = Get-Content -Raw -LiteralPath $accountNoVncPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundaryContract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($manualCampaign, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $releaseBoundary, $readme, $agents, $developerIndex, $guide, $operations, $policy, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.27-admin-smoke\s*(->|→)\s*0\.42\.28-admin-smoke|04227→04228|04227-04228'
            $content | Should -Match 'manual-admin-campaign-2026-05-17-04227-04228|manual-admin-campaign-descriptor-20260517-04227-04228-closed'
            $content | Should -Match 'e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c'
            $content | Should -Match '223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e'
            $content | Should -Match 'b9676f6dc37d667ae0d60367e9f4e576a27e3864'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($publicBoundary, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $publicBoundaryContract, $releaseBoundary)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-17-04228-pr152-postmerge-pass'
            $content | Should -Match '25985786230'
            $content | Should -Match '76382711230'
            $content | Should -Match 'ca07514097f4e9524a7f3630d321c9666593c962'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        $manualCampaign | Should -Match 'result:\s*`?PASS`?'
        $manualCampaign | Should -Match 'descriptor_missing_count:\s*`?0`?'
        $manualCampaign | Should -Match 'descriptor_not_pass_count:\s*`?0`?'
        $manualCampaign | Should -Match 'KB5087545'
        $manualCampaign | Should -Match '9732074563d71344f6a8d19216134510f049844c2bb6eda28cc79520a4a4d37b'
        $manualCampaign | Should -Match 'ba5b817276ea201e7010374bfef7ae126d7ad5388d52798027227e82c07291b3'
        $manualCampaign | Should -Match 'e4bd703170c881400bb88f728bb44a9cf410957c9a53d35db729e16267232b8e'
        $manualCampaign | Should -Match 'current-card-recheck-after-descriptor'
        $manualCampaign | Should -Match 'installed-account-login-smoke-20260517-04228-packagepair'
        $manualCampaign | Should -Match 'target-backed-novnc-installed-streaming-smoke-20260517-04228-packagepair'
        $operatorSurface | Should -Match 'host_ops_web_diagnostics_bucket_table_contract:\s*`host-ops-web-diagnostics-bucket-table-v1`'
        $accountNoVnc | Should -Match 'token/password 노출.*`false`'
        $ledger | Should -Match 'previous_04228_manual_admin_package_pair:\s*`0\.42\.27-admin-smoke -> 0\.42\.28-admin-smoke`'
        $matrix | Should -Match 'previous_04228_latest_manual_admin_package_pair:\s*0\.42\.27-admin-smoke -> 0\.42\.28-admin-smoke'
        $publicBoundaryContract | Should -Match 'previous_pr152_latest_main_push_run_id:\s*25985786230'
        $descriptor | Should -Match 'previous_04228_status:\s*`closed-package-pair-04227-04228-pass-and-04228-fullgate-current-card-pass-awaiting-next-product-payload`'
        $descriptor | Should -Match 'previous_04228_current_manual_admin_package_pair:\s*`0\.42\.27-admin-smoke -> 0\.42\.28-admin-smoke`'
    }

    It 'records 0.42.29 selector package chain closure and PR 153 public boundary current evidence' {
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-17-04229.md'
        $hostMutationPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-17-04229-hostmutation.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-17-04229.md'
        $accountNoVncPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-17-04229.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md'
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04229-selector-package-chain-2026-05-17.md'

        foreach ($path in @($packagePath, $hostMutationPath, $operatorSurfacePath, $accountNoVncPath, $manualCampaignPath, $publicBoundaryPath, $followupPath)) {
            $path | Should -Exist
        }

        $package = Get-Content -Raw -LiteralPath $packagePath
        $hostMutation = Get-Content -Raw -LiteralPath $hostMutationPath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $accountNoVnc = Get-Content -Raw -LiteralPath $accountNoVncPath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundaryContract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $adr0006 = Get-RepoText -RelativePath 'docs/adr/0006-internal-private-network-distribution.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($package, $hostMutation, $operatorSurface, $accountNoVnc, $manualCampaign, $followup, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $releaseBoundary, $adr0006)) {
            $content | Should -Match '0\.42\.28-admin-smoke\s*(->|→)\s*0\.42\.29-admin-smoke|04228→04229|04228-04229'
            $content | Should -Match 'manual-admin-campaign-2026-05-17-04228-04229|manual-admin-campaign-descriptor-20260517-04228-04229-closed'
            $content | Should -Match '3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542'
            $content | Should -Match '2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d'
            $content | Should -Match 'd306712ad671c8a00d5c560765b8952e24a07502'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($publicBoundary, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $publicBoundaryContract, $releaseBoundary, $readme, $agents, $packagingReadme, $installerReadme)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-17-04229-pr153-postmerge-pass'
            $content | Should -Match '25987705546'
            $content | Should -Match '76388078056'
            $content | Should -Match 'd306712ad671c8a00d5c560765b8952e24a07502'
        }

        $package | Should -Match 'package_build_decision:\s*`executed-0\.42\.29-admin-smoke`'
        $package | Should -Match '2031c4b669e9a6bf18019302b7291f7484588548ca64bfeb4afa2abf2a09bf77'
        $package | Should -Match 'f18dbe5a813a55bc42698b9cd13275cf10265ea1dffed43cfccbba15fe15a085'
        $hostMutation | Should -Match 'full-admin-host-mutation-gate-20260517-04229'
        $hostMutation | Should -Match 'host_mutation_performed:\s*`true`'
        $operatorSurface | Should -Match 'latest_batch_id:\s*`full-admin-host-mutation-gate-20260517-04229`'
        $operatorSurface | Should -Match 'host_ops_web_diagnostics_bucket_table_contract:\s*`host-ops-web-diagnostics-bucket-table-v1`'
        $accountNoVnc | Should -Match 'installed-account-login-smoke-20260517-04229'
        $accountNoVnc | Should -Match 'target-backed-novnc-installed-streaming-smoke-20260517-04229'
        $accountNoVnc | Should -Match 'c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106'
        $manualCampaign | Should -Match 'KB5087545'
        $manualCampaign | Should -Match '6cb6e84e8636c5a55c886125235be62fda2505e3a969c336b50c827f9e63b462'
        $manualCampaign | Should -Match '9a6c6a50bd9212e43dea2f0250387002cf00998f5969dc8c57697da2ca587c41'
        $manualCampaign | Should -Match '49131f6321a68050609bc377e782b99e80c1a190b9a78080d1229dfdaad12c79'
        $manualCampaign | Should -Match 'descriptor_missing_count:\s*`?0`?'
        $manualCampaign | Should -Match 'descriptor_not_pass_count:\s*`?0`?'
        $ledger | Should -Match 'ledger_id:\s*`current-evidence-ledger-2026-05-29-04259-public-boundary-docs-maintenance-postpush-pass`'
        $ledger | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke`'
        $matrix | Should -Match 'latest_manual_admin_package_pair:\s*0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke'
        $publicBoundaryContract | Should -Match 'previous_pr153_latest_main_push_run_id:\s*25987705546'
        $descriptor | Should -Match 'status:\s*`closed-package-pair-04232-04234-pass-and-04234-fullgate-current-card-pass-awaiting-next-product-payload`'
        $descriptor | Should -Match 'previous_04234_manual_admin_next_package_pair_candidate:\s*`pending-next-product-payload-after-04234-package-pair`'
        $adr0006 | Should -Match 'scope_lock_latest_internal_admin_smoke:\s*0\.42\.56-admin-smoke'
        $publicMatrix | Should -Match 'scope_lock_latest_internal_admin_smoke:\s*0\.42\.56-admin-smoke'
        $publicMatrix | Should -Match 'scope_lock_latest_internal_admin_smoke_evidence:\s*docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256\.md'
    }

    It 'records PR 154 public boundary follow-up as historical deferral before 0.42.30 closure' {
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04230-pr154-public-boundary-followup-2026-05-18.md'

        foreach ($path in @($publicBoundaryPath, $followupPath)) {
            $path | Should -Exist
        }

        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundaryContract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($publicBoundary, $followup, $ledger, $matrix, $publicMatrix, $descriptor, $publicBoundaryContract)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-17-04230-pr154-postmerge-pass'
            $content | Should -Match '25989986761'
            $content | Should -Match '76394250912'
            $content | Should -Match 'd7f611dfc14a9fa1507f936559209513272b585a'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($publicBoundary, $followup, $ledger, $publicMatrix, $descriptor)) {
            $content | Should -Match 'deferred-no-product-payload-change-after-pr154|product payload 변경이 없어'
            $content | Should -Match 'not-run-no-product-payload-change-after-pr154|post_04230_pr154_package_chain_decision|0\.42\.30-admin-smoke.*열지 않|package chain.*보류|package chain.*deferred'
        }

        $publicBoundary | Should -Match 'result:\s*PASS'
        $publicBoundary | Should -Match 'workflow:\s*Public Boundary Contract'
        $publicBoundary | Should -Match 'fallback_required_guard:\s*public-boundary-ci-required'
        $publicBoundary | Should -Match 'checkout_action_version:\s*actions/checkout@v6\.0\.2'
        $followup | Should -Match 'pre_docs_branch_diff_result:\s*`empty`'
        $followup | Should -Match 'host_mutation_performed:\s*`false`'
        $ledger | Should -Match 'previous_pr154_public_boundary_main_push_run_id:\s*`25989986761`'
        $ledger | Should -Match 'current_full_admin_host_mutation:\s*`0\.42\.56-admin-smoke`'
        $ledger | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke`'
        $publicBoundaryContract | Should -Match 'previous_pr154_latest_main_push_run_id:\s*25989986761'
        $publicBoundaryContract | Should -Match 'previous_pr153_latest_main_push_run_id:\s*25987705546'
        $descriptor | Should -Match 'previous_04234_manual_admin_next_package_pair_candidate:\s*`pending-next-product-payload-after-04234-package-pair`'
        $descriptor | Should -Match 'manual_admin_next_package_pair_candidate_status:\s*`blocked-by-installed-baseline-version-mismatch`'
        $matrix | Should -Match 'latest_product_payload_package:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'post_04230_pr154_product_payload_change_detected:\s*false'
    }

    It 'records PR 155 public boundary follow-up and worktree triage as historical deferral before 0.42.30 closure' {
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04231-pr155-public-boundary-followup-2026-05-18.md'
        $worktreeTriagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/local-worktree-triage-2026-05-18-04231.md'

        foreach ($path in @($publicBoundaryPath, $followupPath, $worktreeTriagePath)) {
            $path | Should -Exist
        }

        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        $worktreeTriage = Get-Content -Raw -LiteralPath $worktreeTriagePath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundaryContract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($publicBoundary, $followup, $ledger, $matrix, $publicMatrix, $descriptor, $publicBoundaryContract)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-18-04231-pr155-postmerge-pass'
            $content | Should -Match '26013384587'
            $content | Should -Match '76458402221'
            $content | Should -Match '2eccbd5ec76e2a33e2ec96dd2002be45ba56d34f'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($publicBoundary, $followup, $ledger, $publicMatrix, $descriptor)) {
            $content | Should -Match 'deferred-no-product-payload-change-after-pr155|product payload 변경이 없어'
            $content | Should -Match 'not-run-no-product-payload-change-after-pr155|post_04231_pr155_package_chain_decision|0\.42\.30-admin-smoke.*열지 않|package chain.*보류|package chain.*deferred'
        }

        $publicBoundary | Should -Match 'result:\s*PASS'
        $publicBoundary | Should -Match 'workflow:\s*Public Boundary Contract'
        $publicBoundary | Should -Match 'fallback_required_guard:\s*public-boundary-ci-required'
        $publicBoundary | Should -Match 'checkout_action_version:\s*actions/checkout@v6\.0\.2'
        $followup | Should -Match 'pre_docs_branch_diff_result:\s*`empty`'
        $followup | Should -Match 'host_mutation_performed:\s*`false`'
        $worktreeTriage | Should -Match 'patch_equivalent_delete_candidate_count:\s*`13`'
        $worktreeTriage | Should -Match 'unmerged_unique_branch_count:\s*`0`'
        $worktreeTriage | Should -Match 'preserve_required_count:\s*`0`'
        $worktreeTriage | Should -Match 'patch-equivalent-delete-candidate-cherry-equivalent'
        $ledger | Should -Match 'previous_pr155_public_boundary_main_push_run_id:\s*`26013384587`'
        $ledger | Should -Match 'post_04231_local_worktree_patch_equivalent_delete_candidate_count:\s*`13`'
        $publicBoundaryContract | Should -Match 'previous_pr155_latest_main_push_run_id:\s*26013384587'
        $publicBoundaryContract | Should -Match 'previous_pr154_latest_main_push_run_id:\s*25989986761'
        $descriptor | Should -Match 'post_04231_local_worktree_patch_equivalent_delete_candidate_count:\s*`13`'
        $matrix | Should -Match 'post_04231_pr155_product_payload_change_detected:\s*false'
        $publicMatrix | Should -Match 'post_04231_pr155_product_payload_change_detected:\s*false'
    }

    It 'records PR 156 public boundary follow-up as historical deferral before 0.42.30 closure' {
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass.md'
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04232-pr156-public-boundary-followup-2026-05-18.md'

        foreach ($path in @($publicBoundaryPath, $followupPath)) {
            $path | Should -Exist
        }

        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $followup = Get-Content -Raw -LiteralPath $followupPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $publicBoundaryContract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($publicBoundary, $followup, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $publicBoundaryContract, $releaseBoundary)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-18-04232-pr156-postmerge-pass'
            $content | Should -Match '26017721669'
            $content | Should -Match '76471545641'
            $content | Should -Match 'a4509c552c003ee0fc87b54b26529686e6dfeb84'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($publicBoundary, $followup, $ledger, $publicMatrix, $descriptor)) {
            $content | Should -Match 'deferred-no-product-payload-change-after-pr156|historical-deferred-no-product-payload-change-after-pr156|product payload 변경이 없어'
            $content | Should -Match 'not-run-no-product-payload-change-after-pr156|post_04232_pr156_package_chain_decision|0\.42\.30-admin-smoke.*열지 않|package chain.*보류|package chain.*deferred|historical-deferred-until-followup-user-approved-04230-chain'
        }

        foreach ($content in @($evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $classification, $releaseBoundary, $readme, $agents, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.31-admin-smoke|0\.42\.32-admin-smoke'
            $content | Should -Match 'manual-admin-campaign-2026-05-19-04231-04232|full-admin-host-mutation-gate-20260519-04232|manual-admin-campaign-2026-05-18-04230-04231|full-admin-host-mutation-gate-20260518-04231'
        }

        $publicBoundary | Should -Match 'result:\s*PASS'
        $publicBoundary | Should -Match 'workflow:\s*Public Boundary Contract'
        $publicBoundary | Should -Match 'fallback_required_guard:\s*public-boundary-ci-required'
        $publicBoundary | Should -Match 'checkout_action_version:\s*actions/checkout@v6\.0\.2'
        $followup | Should -Match 'pre_docs_branch_diff_result:\s*`empty`'
        $followup | Should -Match 'host_mutation_performed:\s*`false`'
        $ledger | Should -Match 'ledger_id:\s*`current-evidence-ledger-2026-05-29-04259-public-boundary-docs-maintenance-postpush-pass`'
        $ledger | Should -Match 'current_public_boundary_pr:\s*`none-post-04259-public-boundary-docs-maintenance-main-push`'
        $ledger | Should -Match 'previous_04248_public_boundary_main_push_run_id:\s*`26445409133`'
        $ledger | Should -Match 'previous_pr169_public_boundary_main_push_run_id:\s*`26288103559`'
        $ledger | Should -Match 'previous_pr156_public_boundary_main_push_run_id:\s*`26017721669`'
        $ledger | Should -Match 'previous_pr155_public_boundary_main_push_run_id:\s*`26013384587`'
        $publicBoundaryContract | Should -Match 'latest_main_push_run_id:\s*26636072420'
        $publicBoundaryContract | Should -Match 'previous_04253_evidence_closure_latest_main_push_run_id:\s*26511891436'
        $publicBoundaryContract | Should -Match 'previous_04253_evidence_closure_rollforward_main_push_run_id:\s*26510159990'
        $publicBoundaryContract | Should -Match 'previous_04253_evidence_closure_initial_main_push_run_id:\s*26494683032'
        $publicBoundaryContract | Should -Match 'previous_04253_provider_latest_main_push_run_id:\s*26494136304'
        $publicBoundaryContract | Should -Match 'previous_04250_latest_main_push_run_id:\s*26489610881'
        $publicBoundaryContract | Should -Match 'previous_pr156_latest_main_push_run_id:\s*26017721669'
        $publicBoundaryContract | Should -Match 'previous_pr155_latest_main_push_run_id:\s*26013384587'
        $descriptor | Should -Match 'post_04232_pr156_package_chain_decision:\s*`historical-deferred-no-product-payload-change-after-pr156`'
        $ledger | Should -Match 'post_04232_pr156_package_chain_decision:\s*`historical-deferred-no-product-payload-change-after-pr156`'
        $matrix | Should -Match 'post_04232_pr156_product_payload_change_detected:\s*false'
        $publicMatrix | Should -Match 'post_04232_pr156_product_payload_change_detected:\s*false'
    }

    It 'records 0.42.34 package fullgate package-pair closure and installed Web TUI CLI current-card' {
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-19-04234.md'
        $hostMutationPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-19-04234-hostmutation.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-19-04232-04234.md'
        $operatorSurfacePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md'

        foreach ($path in @($packagePath, $hostMutationPath, $manualCampaignPath, $operatorSurfacePath)) {
            $path | Should -Exist
        }

        $package = Get-Content -Raw -LiteralPath $packagePath
        $hostMutation = Get-Content -Raw -LiteralPath $hostMutationPath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $operatorSurface = Get-Content -Raw -LiteralPath $operatorSurfacePath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $adr0006 = Get-RepoText -RelativePath 'docs/adr/0006-internal-private-network-distribution.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        foreach ($content in @($package, $hostMutation, $manualCampaign, $operatorSurface, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $adr0006, $readme, $agents, $developerIndex, $guide, $operations, $policy, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.34-admin-smoke'
            $content | Should -Match 'aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78'
            $content | Should -Match 'fc8cc284b7824172b8bf035858fb86b21bd26e5d'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        foreach ($content in @($hostMutation, $manualCampaign, $operatorSurface, $ledger, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification, $readme, $agents, $developerIndex, $guide, $policy, $releaseBoundary, $packagingReadme, $installerReadme)) {
            $content | Should -Match 'full-admin-host-mutation-gate-20260519-04234'
        }

        foreach ($content in @($package, $hostMutation, $manualCampaign, $operatorSurface, $ledger, $controlPlaneIndex, $matrix, $publicMatrix, $descriptor, $classification)) {
            $content | Should -Match 'a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5'
        }

        foreach ($content in @($manualCampaign, $ledger, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $readme, $agents, $packagingReadme, $installerReadme)) {
            $content | Should -Match '0\.42\.32-admin-smoke\s*(->|→)\s*0\.42\.34-admin-smoke|04232→04234|04232-04234'
            $content | Should -Match 'manual-admin-campaign-2026-05-19-04232-04234|manual-admin-campaign-descriptor-20260519-04232-04234-closed'
            $content | Should -Match 'da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad'
        }

        $package | Should -Match 'package_build_decision:\s*`pcvcli-linux-palette-and-utf8-interactive-shell`'
        $package | Should -Match 'cli_sha256:\s*`84d38979cb2b4cfab4060022a11d86e5db0f7b4ed7f87c2d90ad6ab377cec9f3`'
        $package | Should -Match 'tui_sha256:\s*`0100291dc1752b7f9a819e6792754228e1fb1b575b4350ddd1c0ca992acab78c`'
        $hostMutation | Should -Match 'host_mutation_performed:\s*`true`'
        $hostMutation | Should -Match 'final service `PureCVisorDesktopNode` `Running`/`Auto`'
        $hostMutation | Should -Match 'firewall final count `0`'
        $hostMutation | Should -Match 'Event Log source absent'
        $manualCampaign | Should -Match 'descriptor_schema_version:\s*`2`'
        $manualCampaign | Should -Match 'descriptor_summary:\s*`artifacts/manual-admin-campaign-20260519-04232-04234/manual-admin-campaign-descriptor/summary\.json`'
        $manualCampaign | Should -Match 'baseline_msi_sha256:\s*`3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9`'
        $manualCampaign | Should -Match 'target_msi_sha256:\s*`aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`'
        $manualCampaign | Should -Match 'burn_bundle_sha256:\s*`6d379bf63aef4729871dc89437096eb0bd35800ec220a629da753e5e09fcda79`'
        $manualCampaign | Should -Match 'msix_v1_sha256:\s*`c97954dac6cf6e716a9d32203e30a7d37d0bad4d7300eefcaaf7171ac626613c`'
        $manualCampaign | Should -Match 'msix_v2_sha256:\s*`5982680b03d48324be85e82011dc3bd21e6dc7a33241f503aef6d125f31c75c6`'
        $manualCampaign | Should -Match 'KB5087545'
        $manualCampaign | Should -Match 'missing_count=0'
        $manualCampaign | Should -Match 'not_pass_count=0'
        $operatorSurface | Should -Match 'machine_path_contains_install_dir:\s*`true`'
        $operatorSurface | Should -Match 'pcvcli_resolved_from_machine_path:\s*`true`'
        $operatorSurface | Should -Match 'pcvtui_resolved_from_machine_path:\s*`true`'
        $operatorSurface | Should -Match 'token_source:\s*`default-protected-token-file-auto-discovery`'
        $operatorSurface | Should -Match 'cli_host_status:\s*`pass`'
        $operatorSurface | Should -Match 'cli_json_vm_list:\s*`pass`'
        $operatorSurface | Should -Match 'cli_ops_summary:\s*`pass`'
        $operatorSurface | Should -Match 'tui_smoke_runtime:\s*`pass`'
        $operatorSurface | Should -Match 'token_value_observed:\s*`false`'
        $operatorSurface | Should -Match 'password_value_observed:\s*`false`'
        $ledger | Should -Match 'current-evidence-ledger-2026-05-29-04259-public-boundary-docs-maintenance-postpush-pass'
        $ledger | Should -Match 'post_04232_04234_package_chain_status:\s*`closed-manual-admin-package-pair-04232-04234`'
        $matrix | Should -Match 'latest_product_payload_package:\s*0\.42\.56-admin-smoke'
        $matrix | Should -Match 'latest_manual_admin_package_pair:\s*0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke'
        $publicMatrix | Should -Match 'scope_lock_latest_internal_admin_smoke:\s*0\.42\.56-admin-smoke'
        $descriptor | Should -Match 'status:\s*`closed-package-pair-04232-04234-pass-and-04234-fullgate-current-card-pass-awaiting-next-product-payload`'
        $descriptor | Should -Match 'previous_04234_manual_admin_next_package_pair_candidate:\s*`pending-next-product-payload-after-04234-package-pair`'
        $adr0006 | Should -Match 'scope_lock_latest_internal_admin_smoke:\s*0\.42\.56-admin-smoke'
    }

    It 'records 0.42.41 package-chain closure, installed current-card, actual VM TUI row projection, and PR 169 public boundary follow-up' {
        $package35Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04235.md'
        $hostMutation35Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04235-hostmutation.md'
        $manualCampaign35Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04234-04235.md'
        $package37Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04237.md'
        $operatorSurface37Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04237.md'
        $package38Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04238.md'
        $operatorSurface38Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04238.md'
        $manualCampaign38Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04237-04238.md'
        $package39Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04239.md'
        $hostMutation39Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04239-hostmutation.md'
        $operatorSurface39Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04239.md'
        $manualCampaign39Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md'
        $installedCliQosGuest39Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md'
        $package40Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-21-04240.md'
        $hostMutation40Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-21-04240-hostmutation.md'
        $manualCampaign40Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md'
        $operatorSurface40Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-21-04240.md'
        $actualVmQosGuest40Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-21-04240.md'
        $package41Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-22-04241.md'
        $hostMutation41Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-22-04241-hostmutation.md'
        $manualCampaign41Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-22-04240-04241.md'
        $operatorSurface41Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-22-04241.md'
        $actualVmQosGuest41Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md'
        $publicBoundary160Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-19-pr160-postmerge-pass.md'
        $publicBoundary162Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr162-postmerge-pass.md'
        $publicBoundary163Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr163-postmerge-pass.md'
        $publicBoundary164Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-20-pr164-postmerge-pass.md'
        $publicBoundary167Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr167-postmerge-pass.md'
        $publicBoundary168Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-21-pr168-postmerge-pass.md'
        $publicBoundary169Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-22-pr169-postmerge-pass.md'
        $postPr169FollowupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04241-pr169-public-boundary-followup-2026-05-22.md'
        $installedCliQosGuestToolPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInstalledCliQosGuestSmoke.ps1'

        foreach ($path in @($package35Path, $hostMutation35Path, $manualCampaign35Path, $package37Path, $operatorSurface37Path, $package38Path, $operatorSurface38Path, $manualCampaign38Path, $package39Path, $hostMutation39Path, $operatorSurface39Path, $manualCampaign39Path, $installedCliQosGuest39Path, $package40Path, $hostMutation40Path, $manualCampaign40Path, $operatorSurface40Path, $actualVmQosGuest40Path, $package41Path, $hostMutation41Path, $manualCampaign41Path, $operatorSurface41Path, $actualVmQosGuest41Path, $publicBoundary160Path, $publicBoundary162Path, $publicBoundary163Path, $publicBoundary164Path, $publicBoundary167Path, $publicBoundary168Path, $publicBoundary169Path, $postPr169FollowupPath, $installedCliQosGuestToolPath)) {
            $path | Should -Exist
        }

        $package35 = Get-Content -Raw -LiteralPath $package35Path
        $hostMutation35 = Get-Content -Raw -LiteralPath $hostMutation35Path
        $manualCampaign35 = Get-Content -Raw -LiteralPath $manualCampaign35Path
        $package37 = Get-Content -Raw -LiteralPath $package37Path
        $operatorSurface37 = Get-Content -Raw -LiteralPath $operatorSurface37Path
        $package38 = Get-Content -Raw -LiteralPath $package38Path
        $operatorSurface38 = Get-Content -Raw -LiteralPath $operatorSurface38Path
        $manualCampaign38 = Get-Content -Raw -LiteralPath $manualCampaign38Path
        $package39 = Get-Content -Raw -LiteralPath $package39Path
        $hostMutation39 = Get-Content -Raw -LiteralPath $hostMutation39Path
        $operatorSurface39 = Get-Content -Raw -LiteralPath $operatorSurface39Path
        $manualCampaign39 = Get-Content -Raw -LiteralPath $manualCampaign39Path
        $installedCliQosGuest39 = Get-Content -Raw -LiteralPath $installedCliQosGuest39Path
        $package40 = Get-Content -Raw -LiteralPath $package40Path
        $hostMutation40 = Get-Content -Raw -LiteralPath $hostMutation40Path
        $manualCampaign40 = Get-Content -Raw -LiteralPath $manualCampaign40Path
        $operatorSurface40 = Get-Content -Raw -LiteralPath $operatorSurface40Path
        $actualVmQosGuest40 = Get-Content -Raw -LiteralPath $actualVmQosGuest40Path
        $package41 = Get-Content -Raw -LiteralPath $package41Path
        $hostMutation41 = Get-Content -Raw -LiteralPath $hostMutation41Path
        $manualCampaign41 = Get-Content -Raw -LiteralPath $manualCampaign41Path
        $operatorSurface41 = Get-Content -Raw -LiteralPath $operatorSurface41Path
        $actualVmQosGuest41 = Get-Content -Raw -LiteralPath $actualVmQosGuest41Path
        $installedCliQosGuestTool = Get-Content -Raw -LiteralPath $installedCliQosGuestToolPath
        $publicBoundary160 = Get-Content -Raw -LiteralPath $publicBoundary160Path
        $publicBoundary162 = Get-Content -Raw -LiteralPath $publicBoundary162Path
        $publicBoundary163 = Get-Content -Raw -LiteralPath $publicBoundary163Path
        $publicBoundary164 = Get-Content -Raw -LiteralPath $publicBoundary164Path
        $publicBoundary167 = Get-Content -Raw -LiteralPath $publicBoundary167Path
        $publicBoundary168 = Get-Content -Raw -LiteralPath $publicBoundary168Path
        $publicBoundary169 = Get-Content -Raw -LiteralPath $publicBoundary169Path
        $postPr169Followup = Get-Content -Raw -LiteralPath $postPr169FollowupPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $guide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $operations = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $policy = Get-RepoText -RelativePath 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'

        $closure35EvidenceDocs = @($package35, $hostMutation35, $manualCampaign35, $evidenceIndex, $controlPlaneIndex)
        foreach ($content in $closure35EvidenceDocs) {
            $content | Should -Match '0\.42\.35-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260520-04235|full-admin-host-mutation-gate-2026-05-20-04235-hostmutation'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260520-04234-04235-closed|manual-admin-campaign-2026-05-20-04234-04235'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        $currentReferenceDocs = @($readme, $agents, $developerIndex, $guide, $operations, $policy, $releaseBoundary, $packagingReadme, $installerReadme)
        foreach ($content in $currentReferenceDocs) {
            $content | Should -Match '0\.42\.59-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260529-04259|manual-admin-campaign-descriptor-20260529-04258-04259-closed|manual-admin-campaign-2026-05-29-04258-04259|admin-smoke-package-2026-05-29-04259|installed-operator-surface-current-card-2026-05-29-04259'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        $closure35DigestDocs = @($package35, $hostMutation35, $manualCampaign35, $ledger, $evidenceIndex, $controlPlaneIndex)
        foreach ($content in $closure35DigestDocs) {
            $content | Should -Match '12d05f2d783dfdb1db3f1596cd266af17578e33fca3f4fec272aac7df5e22697'
            $content | Should -Match '71ccbe6188de9a52465beae9afc165f7777631bacbbc14a3137d0f9a6379994d|ba966f3c41d81579dc6f065988c5fc015d47a9b0c8c77b4f4c3bf5962c1806a1'
        }

        $fastFollow37Docs = @($package37, $operatorSurface37, $ledger, $evidenceIndex, $controlPlaneIndex, $agents)
        foreach ($content in $fastFollow37Docs) {
            $content | Should -Match '0\.42\.37-admin-smoke'
            $content | Should -Match 'admin-smoke-package-2026-05-20-04237|installed-operator-surface-current-card-2026-05-20-04237|installed-cli-vm-lifecycle-smoke-20260520-04237/summary\.json'
            $content | Should -Match 'pause|resume|rename|lifecycle|current-card'
        }

        $fastFollow37DigestDocs = @($package37, $operatorSurface37, $ledger, $evidenceIndex)
        foreach ($content in $fastFollow37DigestDocs) {
            $content | Should -Match '05dc31965af68792d21d919e19cb07997207d0514fd0ee39169d92129e95f67e'
            $content | Should -Match '1e2487bfe474daad624a3ef67837a278ab5d25a71c654f8b7c18c95e3cc94e9e'
        }

        foreach ($content in @($package37, $operatorSurface37, $ledger, $evidenceIndex, $controlPlaneIndex, $agents)) {
            $content | Should -Match 'installed-cli-vm-lifecycle-smoke-20260520-04237/summary\.json'
        }

        $current39Docs = @($package39, $hostMutation39, $operatorSurface39, $manualCampaign39, $ledger, $evidenceIndex, $controlPlaneIndex)
        foreach ($content in $current39Docs) {
            $content | Should -Match '0\.42\.39-admin-smoke|04239'
            $content | Should -Match 'full-admin-host-mutation-gate-20260520-04239|full-admin-host-mutation-gate-2026-05-20-04239-hostmutation'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260520-04238-04239-closed|manual-admin-campaign-2026-05-20-04238-04239|manual-admin-campaign-descriptor-20260521-04239-04240-closed|manual-admin-campaign-2026-05-21-04239-04240|installed-operator-surface-current-card-2026-05-20-04239'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        $current40Docs = @($package40, $hostMutation40, $manualCampaign40, $operatorSurface40, $ledger, $evidenceIndex, $controlPlaneIndex)
        foreach ($content in $current40Docs) {
            $content | Should -Match '0\.42\.40-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260521-04240|full-admin-host-mutation-gate-2026-05-21-04240-hostmutation|04240'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260521-04239-04240-closed|manual-admin-campaign-2026-05-21-04239-04240|closed-manual-admin-package-pair-04239-04240|04240'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        $current41Docs = @($package41, $hostMutation41, $manualCampaign41, $operatorSurface41, $actualVmQosGuest41)
        foreach ($content in $current41Docs) {
            $content | Should -Match '0\.42\.41-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260522-04241|full-admin-host-mutation-gate-2026-05-22-04241-hostmutation|manual-admin-campaign-descriptor-20260522-04240-04241-closed|manual-admin-campaign-2026-05-22-04240-04241|installed-operator-surface-current-card-2026-05-22-04241|web-tui-qos-guest-readback-actual-vm-2026-05-22-04241|admin-smoke-package-2026-05-22-04241'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication|not-claimed|excluded|out-of-scope'
        }

        $ledger | Should -Match 'ledger_id:\s*`current-evidence-ledger-2026-05-29-04259-public-boundary-docs-maintenance-postpush-pass`'
        $ledger | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke`'
        $ledger | Should -Match 'current_descriptor_batch_id:\s*`manual-admin-campaign-descriptor-20260528-04255-04256-closed`'
        $ledger | Should -Match 'latest_product_payload_package_smoke:\s*`0\.42\.56-admin-smoke`'
        $ledger | Should -Match 'latest_installed_operator_surface_smoke:\s*`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256\.md`'
        $ledger | Should -Match 'latest_operator_surface_qos_guest_actual_vm_evidence:\s*`docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241\.md`'
        $ledger | Should -Match 'latest_tui_row_projection_fix_package_chain_trigger:\s*`closed-by-0\.42\.41-installed-smoke`'
        $ledger | Should -Match 'latest_installed_cli_qos_guest_targeted_smoke:\s*`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239\.md`'
        $hostMutation35 | Should -Match 'host_mutation_performed:\s*`true`'
        $manualCampaign35 | Should -Match 'descriptor_schema_version:\s*`2`'
        $manualCampaign35 | Should -Match 'missing_count=0'
        $manualCampaign35 | Should -Match 'not_pass_count=0'
        $operatorSurface37 | Should -Match 'actual_vm_lifecycle_smoke:\s*`pass`'
        $operatorSurface37 | Should -Match 'batch_evidence_batch_id:\s*`full-admin-host-mutation-gate-20260520-04235`'
        $package38 | Should -Match '0\.42\.38-admin-smoke'
        $operatorSurface38 | Should -Match 'cli_json_vm_list:\s*`pass`'
        $operatorSurface38 | Should -Match 'tui_smoke_runtime:\s*`pass`'
        $operatorSurface38 | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.37-admin-smoke -> 0\.42\.38-admin-smoke`'
        $manualCampaign38 | Should -Match 'result:\s*`PASS`'
        $manualCampaign38 | Should -Match 'manual-admin-campaign-descriptor-20260520-04237-04238-closed'
        $manualCampaign38 | Should -Match 'clean-host-r2-windows-update'
        $manualCampaign38 | Should -Match 'KB5087545'
        $manualCampaign38 | Should -Match 'not_pass_count=0'
        $package39 | Should -Match 'b6fac120b145b5d0a8bf48a955037593756613d5bbe355bae96de59da4f0d805'
        $package39 | Should -Match '8ccf24a0a304b82dfcb0039c92149806539cf74977014bc3468c589e4ddf624f'
        $package39 | Should -Match '6fd931baf3de77435d0d11b92424cf6657ea4515'
        $hostMutation39 | Should -Match 'host_mutation_performed:\s*`true`'
        $hostMutation39 | Should -Match 'vm\.limit|vm limit'
        $hostMutation39 | Should -Match 'vm\.blkio-get|vm blkio-get'
        $hostMutation39 | Should -Match 'vm\.bandwidth|vm bandwidth'
        $hostMutation39 | Should -Match 'vm\.guest-agent-status|vm guest-agent-status'
        $hostMutation39 | Should -Match 'vm\.guest-ping|vm guest-ping'
        $manualCampaign39 | Should -Match 'descriptor_schema_version:\s*`2`'
        $manualCampaign39 | Should -Match 'KB5087545'
        $manualCampaign39 | Should -Match 'UBR `5139`'
        $manualCampaign39 | Should -Match 'missing_count=0'
        $manualCampaign39 | Should -Match 'not_pass_count=0'
        $manualCampaign39 | Should -Match '23c10a24e33ca706d7c89815b78c07b3a71a0ee94188c6d78ec188eca17ff9f4'
        $package40 | Should -Match '4979a3a60f96b8e8dbcda41bd722c33909c2faf39bc4cf88b8a79fb89e9628e8'
        $package40 | Should -Match '0c5e566f49bd4ef5c78249b3439a4441462a3c6b54433985be4b9badb9618666'
        $package40 | Should -Match 'adb7b8c77ff60b64c5ac4d840e2bdfac62a3793a'
        $hostMutation40 | Should -Match 'host_mutation_performed:\s*`true`'
        $hostMutation40 | Should -Match 'eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41'
        $hostMutation40 | Should -Match 'cd49f061dfd0e2e5afe45cd34befcfb28e02bbd9038eff1fbaef34f8c9616ea5'
        $hostMutation40 | Should -Match '최종 상태는 service\s+`PureCVisorDesktopNode` `Running`/`Auto`'
        $manualCampaign40 | Should -Match 'descriptor_schema_version:\s*`2`'
        $manualCampaign40 | Should -Match 'KB5087545'
        $manualCampaign40 | Should -Match 'UBR `5139`'
        $manualCampaign40 | Should -Match 'missing_count=0'
        $manualCampaign40 | Should -Match 'not_pass_count=0'
        $manualCampaign40 | Should -Match '96599dc4493e26e8cf467e19fabc5ab20306166896c1139bdbeb52566623ab25'
        $operatorSurface39 | Should -Match 'machine_path_contains_install_dir:\s*`true`'
        $operatorSurface39 | Should -Match 'pcvcli_resolved_from_machine_path:\s*`true`'
        $operatorSurface39 | Should -Match 'pcvtui_resolved_from_machine_path:\s*`true`'
        $operatorSurface39 | Should -Match 'token_source:\s*`default-protected-token-file-auto-discovery`'
        $operatorSurface39 | Should -Match 'cli_host_status:\s*`pass`'
        $operatorSurface39 | Should -Match 'cli_json_vm_list:\s*`pass`'
        $operatorSurface39 | Should -Match 'cli_ops_summary:\s*`pass`'
        $operatorSurface39 | Should -Match 'tui_smoke_runtime:\s*`pass`'
        $operatorSurface39 | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.38-admin-smoke -> 0\.42\.39-admin-smoke`'
        $operatorSurface39 | Should -Match 'token_value_observed:\s*`false`'
        $operatorSurface39 | Should -Match 'password_value_observed:\s*`false`'
        $operatorSurface40 | Should -Match 'artifact_root:\s*`artifacts/installed-operator-surface-current-card-20260521-04240`'
        $operatorSurface40 | Should -Match 'machine_path_contains_install_dir:\s*`true`'
        $operatorSurface40 | Should -Match 'pcvcli_resolved_from_machine_path:\s*`true`'
        $operatorSurface40 | Should -Match 'pcvtui_resolved_from_machine_path:\s*`true`'
        $operatorSurface40 | Should -Match 'cli_host_status:\s*`pass`'
        $operatorSurface40 | Should -Match 'cli_json_vm_list:\s*`pass`'
        $operatorSurface40 | Should -Match 'cli_ops_summary:\s*`pass`'
        $operatorSurface40 | Should -Match 'tui_smoke_runtime:\s*`pass`'
        $operatorSurface40 | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.39-admin-smoke -> 0\.42\.40-admin-smoke`'
        $operatorSurface40 | Should -Match 'batch_evidence_batch_id:\s*`full-admin-host-mutation-gate-20260521-04240`'
        $operatorSurface40 | Should -Match 'token_value_observed:\s*`false`'
        $operatorSurface40 | Should -Match 'password_value_observed:\s*`false`'
        $actualVmQosGuest40 | Should -Match 'result:\s*`PASS_WITH_TUI_INSTALLED_BLOCKER_AND_04241_TRIGGER`'
        $actualVmQosGuest40 | Should -Match 'actual_vm_name:\s*`pcv-ux-qos-04240`'
        $actualVmQosGuest40 | Should -Match 'web_no_overlap_check:\s*`pass`'
        $actualVmQosGuest40 | Should -Match 'web_overlap_failure_count:\s*`0`'
        $actualVmQosGuest40 | Should -Match 'installed_tui_actual_vm_row_projection:\s*`blocked-04240`'
        $actualVmQosGuest40 | Should -Match 'source_tui_row_projection_fix:\s*`pass-code-level`'
        $actualVmQosGuest40 | Should -Match 'package_chain_trigger:\s*`0\.42\.41-admin-smoke-required-for-installed-TUI-row-projection-fix`'
        $package41 | Should -Match 'd1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639'
        $package41 | Should -Match '21aeb02757495d8296151ce20dda987ef36fcb2f3320f5163131ffc90e65c361'
        $package41 | Should -Match '2f41da1073df6e65113ae8ddaeb183e9b55874f4'
        $hostMutation41 | Should -Match 'host_mutation_performed:\s*`true`'
        $hostMutation41 | Should -Match 'e080dbff6525754be7a35dfe316745f9c2f8878ad286a31ea66388ba6915d8fb'
        $hostMutation41 | Should -Match '132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b'
        $manualCampaign41 | Should -Match 'descriptor_schema_version:\s*`2`'
        $manualCampaign41 | Should -Match 'missing_count=0'
        $manualCampaign41 | Should -Match 'not_pass_count=0'
        $manualCampaign41 | Should -Match '9ab7e266c093b98982aa854c19f901a6bb133f51c66904b9bfcdf56d538fee73'
        $manualCampaign41 | Should -Match 'cbec6f5ee552229ec086a520ec6a530a922483cea519714e2a7ecb8797fd3a3f'
        $manualCampaign41 | Should -Match '6be5780e4efc37157020b40a82f07e4f544d368d7a0b064fe6f83ac6cf657b81'
        $manualCampaign41 | Should -Match '832604c1f8af235358594469a002971220710970b3590100372526d83c08817a'
        $operatorSurface41 | Should -Match 'artifact_root:\s*`artifacts/installed-operator-surface-current-card-20260522-04241`'
        $operatorSurface41 | Should -Match 'current_manual_admin_package_pair:\s*`0\.42\.40-admin-smoke -> 0\.42\.41-admin-smoke`'
        $operatorSurface41 | Should -Match 'batch_evidence_batch_id:\s*`full-admin-host-mutation-gate-20260522-04241`'
        $operatorSurface41 | Should -Match 'installed_tui_actual_vm_row_projection:\s*`pass`'
        $operatorSurface41 | Should -Match 'token_value_observed:\s*`false`'
        $operatorSurface41 | Should -Match 'password_value_observed:\s*`false`'
        $actualVmQosGuest41 | Should -Match 'result:\s*`PASS`'
        $actualVmQosGuest41 | Should -Match 'actual_vm_name:\s*`pcv-ux-qos-04241`'
        $actualVmQosGuest41 | Should -Match 'installed_tui_actual_vm_row_projection:\s*`pass`'
        $actualVmQosGuest41 | Should -Match 'package_chain_trigger:\s*`closed-by-0\.42\.41-installed-smoke`'
        $actualVmQosGuest41 | Should -Match 'cleanup'
        $installedCliQosGuest39 | Should -Match 'result:\s*`PASS`'
        $installedCliQosGuest39 | Should -Match 'artifact_root:\s*`artifacts/installed-cli-qos-guest-smoke-20260521-04239`'
        $installedCliQosGuest39 | Should -Match 'vm limit'
        $installedCliQosGuest39 | Should -Match 'blkio-get'
        $installedCliQosGuest39 | Should -Match 'bandwidth'
        $installedCliQosGuest39 | Should -Match 'guest-agent-status'
        $installedCliQosGuest39 | Should -Match 'guest-ping'
        $installedCliQosGuest39 | Should -Match 'web_tui_qos_guest_readback_decision:\s*`defer-direct-web-tui-control-no-product-payload-change`'
        $installedCliQosGuest39 | Should -Match 'unsupported_linux_semantics_decision:\s*`keep-out-of-scope-without-new-adr`'
        $installedCliQosGuest39 | Should -Match 'next_product_payload_package_candidate:\s*`0\.42\.40-admin-smoke`'
        $installedCliQosGuest39 | Should -Match 'package_chain_decision:\s*`not-opened-no-product-payload-change-docs-tools-evidence-only-after-pr164`'
        $installedCliQosGuest39 | Should -Match 'token_value_observed:\s*`false`'
        $installedCliQosGuest39 | Should -Match 'password_value_observed:\s*`false`'
        $installedCliQosGuestTool | Should -Match 'Invoke-PcvCliJob'
        $installedCliQosGuestTool | Should -Match 'default-protected-token-file-auto-discovery'
        $installedCliQosGuestTool | Should -Match "'vm', 'limit'"
        $ledger | Should -Match 'latest_manual_admin_candidate_status:\s*`pass-closed`'
        $publicBoundary160 | Should -Match 'pr:\s*`160`'
        $publicBoundary160 | Should -Match 'run_id:\s*`26101838192`'
        $publicBoundary160 | Should -Match 'job_id:\s*`76754696421`'
        $publicBoundary160 | Should -Match 'head_sha:\s*`51a21d7c8612f598b85eeb58818ad3d61136c320`'
        $publicBoundary162 | Should -Match 'pr:\s*`162`'
        $publicBoundary162 | Should -Match 'run_id:\s*`26156660639`'
        $publicBoundary162 | Should -Match 'job_id:\s*`76937705571`'
        $publicBoundary162 | Should -Match 'head_sha:\s*`39087469b2ed1752927cbf5a24c7410d5f96f22b`'
        $publicBoundary163 | Should -Match 'pr:\s*`163`'
        $publicBoundary163 | Should -Match 'run_id:\s*`26164349961`'
        $publicBoundary163 | Should -Match 'job_id:\s*`76964254604`'
        $publicBoundary163 | Should -Match 'head_sha:\s*`465e7b8ef79a1c05913107fa1364850e8dd387e9`'
        $publicBoundary164 | Should -Match 'pr:\s*`164`'
        $publicBoundary164 | Should -Match 'run_id:\s*`26170972989`'
        $publicBoundary164 | Should -Match 'job_id:\s*`76988240617`'
        $publicBoundary164 | Should -Match 'head_sha:\s*`03402f1607b735f2d92291ae6109d7986d9a57b8`'
        $publicBoundary167 | Should -Match 'pr:\s*`167`'
        $publicBoundary167 | Should -Match 'run_id:\s*`26228675428`'
        $publicBoundary167 | Should -Match 'job_id:\s*`77182631331`'
        $publicBoundary167 | Should -Match 'head_sha:\s*`f173f9857089de61ca1fb2b7a2da7839a3dd73a8`'
        $publicBoundary168 | Should -Match 'pr:\s*`168`'
        $publicBoundary168 | Should -Match 'run_id:\s*`26233838385`'
        $publicBoundary168 | Should -Match 'job_id:\s*`77201340972`'
        $publicBoundary168 | Should -Match 'head_sha:\s*`2f41da1073df6e65113ae8ddaeb183e9b55874f4`'
        $publicBoundary169 | Should -Match 'pr:\s*`169`'
        $publicBoundary169 | Should -Match 'run_id:\s*`26288103559`'
        $publicBoundary169 | Should -Match 'job_id:\s*`77380766318`'
        $publicBoundary169 | Should -Match 'head_sha:\s*`11b123311d718cf77e87ccc7b8dea7c5728dc463`'
        $postPr169Followup | Should -Match 'product_payload_change_detected:\s*`false`'
        $postPr169Followup | Should -Match 'next_product_payload_package_candidate:\s*`0\.42\.42-admin-smoke`'
        $postPr169Followup | Should -Match 'admin_smoke_package_chain_decision:\s*`not-run-no-product-payload-change-current-0\.42\.41-admin-smoke`'
        $postPr169Followup | Should -Match 'installed_account_novnc_smoke_decision:\s*`not-run-no-operator-surface-payload-change-after-pr169`'
        $postPr169Followup | Should -Match 'ga_ready_matrix_cross_check:\s*`pass-current-04241-anchor-and-pr169-public-boundary`'
        $ledger | Should -Match 'current_public_boundary_pr:\s*`none-post-04259-public-boundary-docs-maintenance-main-push`'
        $ledger | Should -Match 'previous_04248_public_boundary_main_push_run_id:\s*`26445409133`'
        $ledger | Should -Match 'previous_pr169_public_boundary_main_push_run_id:\s*`26288103559`'
        $ledger | Should -Match 'previous_pr168_public_boundary_main_push_run_id:\s*`26233838385`'
        $ledger | Should -Match 'previous_pr167_public_boundary_main_push_run_id:\s*`26228675428`'
        $ledger | Should -Match 'previous_pr164_public_boundary_main_push_run_id:\s*`26170972989`'
        $ledger | Should -Match 'previous_pr163_public_boundary_main_push_run_id:\s*`26164349961`'
        $ledger | Should -Match 'previous_pr162_public_boundary_main_push_run_id:\s*`26156660639`'
        $ledger | Should -Match 'previous_pr160_public_boundary_main_push_run_id:\s*`26101838192`'
        $ledger | Should -Match 'post_04238_04239_package_chain_status:\s*`closed-manual-admin-package-pair-04238-04239`'
        $ledger | Should -Match 'post_pr164_package_chain_decision:\s*`not-opened-no-product-payload-change-docs-tools-evidence-only`'
        $ledger | Should -Match 'post_04240_package_chain_status:\s*`closed-manual-admin-package-pair-04239-04240`'
        $ledger | Should -Match 'post_04241_package_chain_status:\s*`closed-manual-admin-package-pair-04240-04241`'
        $ledger | Should -Match 'post_04241_next_product_payload_package_trigger:\s*`installed-TUI-row-projection-fix-after-actual-VM-smoke`'
        $ledger | Should -Match 'post_pr169_package_chain_decision:\s*`not-run-no-product-payload-change-current-0\.42\.41-admin-smoke`'
        $ledger | Should -Match 'post_pr169_installed_account_novnc_smoke_decision:\s*`not-run-no-operator-surface-payload-change-after-pr169`'
    }

    It 'records ADR-0007 PCVCLI Hyper-V QoS guest-service parity scope and code-level evidence' {
        $adr0007Path = Join-Path $script:RepoRoot 'docs/adr/0007-pcvcli-hyperv-qos-guest-service-parity.md'
        $sliceEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/pcvcli-hyperv-qos-guest-service-slice-2026-05-20.md'
        $remainingEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/pcvcli-linux-parity-remaining-slice-2026-05-20.md'
        $backendGapPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/pcvcli-backend-command-gap-slice-2026-05-19.md'
        $installedCliQosGuestPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md'
        $webTuiQosGuestReadbackPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md'

        foreach ($path in @($adr0007Path, $sliceEvidencePath, $remainingEvidencePath, $backendGapPath, $installedCliQosGuestPath, $webTuiQosGuestReadbackPath)) {
            $path | Should -Exist
        }

        $adr0007 = Get-Content -Raw -LiteralPath $adr0007Path
        $sliceEvidence = Get-Content -Raw -LiteralPath $sliceEvidencePath
        $remainingEvidence = Get-Content -Raw -LiteralPath $remainingEvidencePath
        $backendGap = Get-Content -Raw -LiteralPath $backendGapPath
        $installedCliQosGuest = Get-Content -Raw -LiteralPath $installedCliQosGuestPath
        $webTuiQosGuestReadback = Get-Content -Raw -LiteralPath $webTuiQosGuestReadbackPath
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $cliUsage = Get-RepoText -RelativePath 'docs/CLI_COMMAND_USAGE.md'
        $cliReadme = Get-RepoText -RelativePath 'src/DesktopNode.Cli/README.md'
        $operatorTerms = Get-RepoText -RelativePath 'docs/OPERATOR_SURFACE_TERMS.md'
        $featureUsage = Get-RepoText -RelativePath 'docs/USER_FEATURE_USAGE_SPEC.md'
        $publicBoundary = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $manualAdminDescriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'

        foreach ($content in @($adr0007, $sliceEvidence, $remainingEvidence, $backendGap, $installedCliQosGuest, $webTuiQosGuestReadback)) {
            $content | Should -Match 'ADR-0007|DESKTOP_NODE_PCVCLI_HYPERV_QOS_GUEST_SERVICE_PARITY_DECISION|pcvcli-hyperv-qos-guest-service-slice'
            $content | Should -Match 'vm\.limit|vm limit'
            $content | Should -Match 'vm\.blkio-get|vm blkio-get'
            $content | Should -Match 'vm\.bandwidth|vm bandwidth'
            $content | Should -Match 'vm\.guest-agent-status|vm guest-agent-status'
            $content | Should -Match 'vm\.guest-ping|vm guest-ping'
            $content | Should -Match 'public trusted signing|Public trusted signing|external stable publication|외부 stable publication|not-claimed|out-of-scope'
        }

        $adrIndex | Should -Match 'ADR-0007'
        $adrIndex | Should -Match 'pcvcli-hyperv-qos-guest-service-parity'

        foreach ($content in @($cliUsage, $cliReadme, $operatorTerms, $featureUsage)) {
            $content | Should -Match 'limit'
            $content | Should -Match 'blkio-get'
            $content | Should -Match 'bandwidth'
            $content | Should -Match 'guest-agent-status'
            $content | Should -Match 'guest-ping'
            $content | Should -Match 'Hyper-V'
            $content | Should -Match 'Linux cgroup|qemu guest agent|Linux QoS|compatibility|호환 claim'
            $content | Should -Match 'public trusted signing|Public trusted signing|external stable publication|외부 stable publication|not-claimed|out-of-scope|범위 밖'
        }

        foreach ($content in @($adr0007, $sliceEvidence, $remainingEvidence, $backendGap, $installedCliQosGuest, $webTuiQosGuestReadback)) {
            $content | Should -Match 'vm\.limit|vm limit'
            $content | Should -Match 'vm\.blkio-get|vm blkio-get'
            $content | Should -Match 'vm\.bandwidth|vm bandwidth'
            $content | Should -Match 'vm\.guest-agent-status|vm guest-agent-status'
            $content | Should -Match 'vm\.guest-ping|vm guest-ping'
            $content | Should -Match 'public trusted signing|Public trusted signing|external stable publication|외부 stable publication|not-claimed|out-of-scope'
        }

        $adr0007 | Should -Match 'DESKTOP_NODE_PCVCLI_HYPERV_QOS_GUEST_SERVICE_PARITY_DECISION:\s*hyperv-semantic-readback-first'
        $adr0007 | Should -Match 'linux_cgroup_qemu_guest_agent_claim:\s*not-claimed'
        $adr0007 | Should -Match 'unsupported_linux_semantics:\s*linux-blkio-set-flags, vm\.guest-agent-ensure-channel, vm\.guest-exec'
        $adr0007 | Should -Match 'qos_mutation_followup_adr:\s*docs/adr/0008-hyperv-qos-mutation-policy\.md'
        $adr0007 | Should -Match 'web_tui_qos_guest_readback_decision:\s*implemented-readback-surface-no-direct-control'
        $adr0007 | Should -Match 'web_tui_qos_guest_readback_evidence:\s*docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21\.md'
        $adr0007 | Should -Match 'next_product_payload_package_chain_status:\s*closed-manual-admin-package-pair-04240-04241'
        $sliceEvidence | Should -Match 'result:\s*`PASS_CODE_LEVEL_AND_04239_PACKAGE_CHAIN_CLOSED`'
        $sliceEvidence | Should -Match 'next_package_decision:\s*`0\.42\.39-admin-smoke-required-after-merge`'
        $sliceEvidence | Should -Match 'package_chain_status:\s*`closed-0\.42\.39-admin-smoke-pass`'
        $installedCliQosGuest | Should -Match 'web_tui_qos_guest_readback_decision:\s*`defer-direct-web-tui-control-no-product-payload-change`'
        $installedCliQosGuest | Should -Match 'package_chain_decision:\s*`not-opened-no-product-payload-change-docs-tools-evidence-only-after-pr164`'
        $webTuiQosGuestReadback | Should -Match 'result:\s*`PASS_CODE_LEVEL_AND_04240_PACKAGE_CHAIN_CLOSED`'
        $webTuiQosGuestReadback | Should -Match 'product_payload_change_detected:\s*`true`'
        $webTuiQosGuestReadback | Should -Match 'next_product_payload_package_candidate:\s*`0\.42\.40-admin-smoke`'
        $webTuiQosGuestReadback | Should -Match 'package_chain_status:\s*`closed-manual-admin-package-pair-04239-04240`'
        $webTuiQosGuestReadback | Should -Match 'host_mutation_performed:\s*`false`'
        $webTuiQosGuestReadback | Should -Match 'QoS / Guest Readback'
        $webTuiQosGuestReadback | Should -Match 'G read selected VM QoS/guest'
        $webTuiQosGuestReadback | Should -Match 'GET /api/v1/vms/\{vm\}/blkio'
        $webTuiQosGuestReadback | Should -Match 'GET /api/v1/vms/\{vm\}/bandwidth'
        $webTuiQosGuestReadback | Should -Match 'GET /api/v1/vms/\{vm\}/guest-agent/status'
        $webTuiQosGuestReadback | Should -Match 'GET /api/v1/vms/\{vm\}/guest-agent/ping'
        $webTuiQosGuestReadback | Should -Match 'linux_blkio_compatible=false'
        $webTuiQosGuestReadback | Should -Match 'linux_bandwidth_compatible=false'
        $webTuiQosGuestReadback | Should -Match 'qemu_guest_agent=false'
        $webTuiQosGuestReadback | Should -Match 'guest_heartbeat_verified=false'
        $webTuiQosGuestReadback | Should -Match 'vm\.blkio-set'
        $webTuiQosGuestReadback | Should -Match 'vm\.guest-agent-ensure-channel'
        $webTuiQosGuestReadback | Should -Match 'vm\.guest-exec'
        $operatorTerms | Should -Match 'Web Console 선택 VM detail.*QoS / Guest Readback|QoS / Guest Readback'
        $operatorTerms | Should -Match '2026-05-21의 TUI readback 기록은 dated historical predecessor'
        $featureUsage | Should -Match '현재 Web Console 선택 VM detail'
        $featureUsage | Should -Not -Match '(?m)^\|\s*TUI\b'
        $ledger | Should -Match 'latest_operator_surface_qos_guest_readback_code_level:\s*`docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21\.md`'
        $ledger | Should -Match 'post_04240_product_payload_change_detected:\s*`true`'
        $ledger | Should -Match 'post_04240_package_chain_status:\s*`closed-manual-admin-package-pair-04239-04240`'
        $manualAdminDescriptor | Should -Match 'post_04240_manual_admin_package_pair_candidate:\s*`0\.42\.39-admin-smoke -> 0\.42\.40-admin-smoke`'
        $manualAdminDescriptor | Should -Match 'post_04240_manual_admin_package_pair_status:\s*`pass-closed`'
        $remainingEvidence | Should -Match 'PASS_SCOPE_LOCK_CLOSED_CODE_LEVEL_PROMOTED'
        $backendGap | Should -Match '0\.42\.39-admin-smoke'
        $publicBoundary | Should -Match 'current_evidence:\s*docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass\.md'
        $publicBoundary | Should -Match 'latest_main_push_pr:\s*none-post-04259-public-boundary-docs-maintenance-main-push'
        $publicBoundary | Should -Match 'latest_main_push_run_id:\s*26636072420'
        $publicBoundary | Should -Match 'previous_04253_evidence_closure_latest_main_push_run_id:\s*26511891436'
        $publicBoundary | Should -Match 'previous_04253_evidence_closure_rollforward_main_push_run_id:\s*26510159990'
        $publicBoundary | Should -Match 'previous_04253_evidence_closure_initial_main_push_run_id:\s*26494683032'
        $publicBoundary | Should -Match 'previous_04253_provider_latest_main_push_run_id:\s*26494136304'
        $publicBoundary | Should -Match 'previous_04250_latest_main_push_run_id:\s*26489610881'
        $publicBoundary | Should -Match 'previous_04248_latest_main_push_run_id:\s*26445409133'
        $publicBoundary | Should -Match 'previous_pr169_latest_main_push_run_id:\s*26288103559'
        $publicBoundary | Should -Match 'post_pr169_admin_smoke_package_chain_decision:\s*not-run-no-product-payload-change-current-0\.42\.41-admin-smoke'
        $publicBoundary | Should -Match 'previous_pr164_latest_main_push_run_id:\s*26170972989'
        $publicBoundary | Should -Match 'previous_pr163_latest_main_push_run_id:\s*26164349961'
        $publicBoundary | Should -Match 'previous_pr162_latest_main_push_run_id:\s*26156660639'
    }

    It 'records post 0.42.45 extension Phase 2 to 5 planning boundaries' {
        $adr0008Path = Join-Path $script:RepoRoot 'docs/adr/0008-hyperv-qos-mutation-policy.md'
        $adr0009Path = Join-Path $script:RepoRoot 'docs/adr/0009-guest-execution-security-boundary-candidate.md'
        $adr0010Path = Join-Path $script:RepoRoot 'docs/adr/0010-account-novnc-target-config-security-policy-candidate.md'
        $phase2SpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation-design.md'
        $phase2PlanPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md'
        $planningEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04245-extension-phase2-5-planning-2026-05-26.md'
        $qosCodeLevelEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26.md'
        $qosInstalledEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247.md'

        foreach ($path in @($adr0008Path, $adr0009Path, $adr0010Path, $phase2SpecPath, $phase2PlanPath, $planningEvidencePath, $qosCodeLevelEvidencePath, $qosInstalledEvidencePath)) {
            $path | Should -Exist
        }

        $adr0008 = Get-Content -Raw -LiteralPath $adr0008Path
        $adr0009 = Get-Content -Raw -LiteralPath $adr0009Path
        $adr0010 = Get-Content -Raw -LiteralPath $adr0010Path
        $phase2Spec = Get-Content -Raw -LiteralPath $phase2SpecPath
        $phase2Plan = Get-Content -Raw -LiteralPath $phase2PlanPath
        $planningEvidence = Get-Content -Raw -LiteralPath $planningEvidencePath
        $qosCodeLevelEvidence = Get-Content -Raw -LiteralPath $qosCodeLevelEvidencePath
        $qosInstalledEvidence = Get-Content -Raw -LiteralPath $qosInstalledEvidencePath
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $cliUsage = Get-RepoText -RelativePath 'docs/CLI_COMMAND_USAGE.md'
        $cliReadme = Get-RepoText -RelativePath 'src/DesktopNode.Cli/README.md'
        $featureUsage = Get-RepoText -RelativePath 'docs/USER_FEATURE_USAGE_SPEC.md'
        $webDesign = Get-RepoText -RelativePath 'web/DESIGN.md'

        $adr0008 | Should -Match 'DESKTOP_NODE_HYPERV_QOS_MUTATION_POLICY_DECISION:\s*installed-package-fullgate-actual-vm-manual-admin-closed'
        $adr0008 | Should -Match 'implementation_status:\s*installed-actual-vm-fullgate-and-manual-admin-closed'
        $adr0008 | Should -Match 'adr0007_boundary_change:\s*false'
        $adr0008 | Should -Match 'pcvcli vm blkio-set'
        $adr0008 | Should -Match 'pcvcli vm bandwidth-set'
        $adr0008 | Should -Match 'hyperv-qos-mutation-preview\.v1'
        $adr0008 | Should -Match 'vm\.qos\.storage\.set'
        $adr0008 | Should -Match 'vm\.qos\.network\.set'
        $adr0008 | Should -Match 'rollback_contract_required:\s*true'
        $adr0008 | Should -Match 'readback_after_apply_required:\s*true'
        $adr0008 | Should -Match 'web_tui_direct_control:\s*opened-phase3-qos-only'
        $adr0008 | Should -Match 'host_mutation_code_path:\s*implemented-wmi-storage-iops-and-network-port-bandwidth'
        $adr0008 | Should -Match 'host_mutation_performed:\s*true'
        $adr0008 | Should -Match 'package_build_performed:\s*0\.42\.47-admin-smoke'
        $adr0008 | Should -Match 'installed_actual_vm_smoke:\s*pass-installed-cli-qos-mutation-04247'
        $adr0008 | Should -Match 'manual_admin_package_pair:\s*closed-0\.42\.45-to-0\.42\.47'

        $phase2Spec | Should -Match 'DESKTOP_NODE_PHASE2_HYPERV_QOS_MUTATION_DESIGN_DECISION:\s*preview-queued-apply-readback-rollback'
        $phase2Spec | Should -Match 'POST /api/v1/vms/\{vm\}/qos/storage/preview'
        $phase2Spec | Should -Match 'POST /api/v1/vms/\{vm\}/qos/network/preview'
        $phase2Spec | Should -Match 'DesktopNodeHyperVQosMutationPlanner'
        $phase2Spec | Should -Match 'Phase 3 Web/TUI Direct Control'
        $phase2Spec | Should -Match 'host_mutation_performed:\s*false'

        $phase2Plan | Should -Match '# Phase 2 Hyper-V QoS Mutation Implementation Plan'
        $phase2Plan | Should -Match 'superpowers:subagent-driven-development'
        $phase2Plan | Should -Match 'DesktopNodeHyperVQosMutationPlanner'
        $phase2Plan | Should -Match 'vm\.qos\.storage\.set'
        $phase2Plan | Should -Match 'pcvcli vm bandwidth-set'
        $phase2Plan | Should -Match 'Invoke-PcvInstalledQosMutationSmoke\.ps1'

        $adr0009 | Should -Match 'DESKTOP_NODE_GUEST_EXECUTION_SECURITY_BOUNDARY_DECISION:\s*security-boundary-deferred'
        $adr0009 | Should -Match 'credential_policy:\s*required-before-implementation'
        $adr0009 | Should -Match 'audit_log_schema:\s*required-before-implementation'
        $adr0009 | Should -Match 'secret_redaction_policy:\s*required-before-implementation'
        $adr0009 | Should -Match 'timeout_cancel_policy:\s*required-before-implementation'
        $adr0009 | Should -Match 'rbac_capability:\s*guest\.exec'
        $adr0009 | Should -Match 'web_tui_guest_exec_control:\s*prohibited-until-security-boundary-applied'

        $adr0010 | Should -Match 'DESKTOP_NODE_ACCOUNT_NOVNC_TARGET_CONFIG_SECURITY_POLICY_DECISION:\s*security-policy-required-before-mutation'
        $adr0010 | Should -Match 'default_network_scope:\s*loopback-only'
        $adr0010 | Should -Match 'lan_exposure_gate:\s*explicit-operator-opt-in-required'
        $adr0010 | Should -Match 'rollback_contract_required:\s*true'
        $adr0010 | Should -Match 'web_tui_direct_config_control:\s*prohibited-until-policy-applied'

        $planningEvidence | Should -Match 'result:\s*`PASS_DOCS_ONLY`'
        $planningEvidence | Should -Match 'phase3_direct_control_guard:\s*`backend-policy-first`'
        $planningEvidence | Should -Match 'phase2_hyperv_qos_mutation_policy_adr:\s*`docs/adr/0008-hyperv-qos-mutation-policy\.md`'
        $planningEvidence | Should -Match 'phase4_guest_execution_security_adr:\s*`docs/adr/0009-guest-execution-security-boundary-candidate\.md`'
        $planningEvidence | Should -Match 'phase5_account_novnc_target_config_security_adr:\s*`docs/adr/0010-account-novnc-target-config-security-policy-candidate\.md`'
        $planningEvidence | Should -Match 'host_mutation_performed:\s*`false`'
        $planningEvidence | Should -Match 'package_build_performed:\s*`false`'

        $qosCodeLevelEvidence | Should -Match 'result:\s*`PASS_CODE_LEVEL`'
        $qosCodeLevelEvidence | Should -Match 'preview_contract:\s*`hyperv-qos-mutation-preview\.v1`'
        $qosCodeLevelEvidence | Should -Match 'native_operations:\s*`vm\.qos\.storage\.preview`'
        $qosCodeLevelEvidence | Should -Match 'cli_commands:\s*`pcvcli vm blkio-set`, `pcvcli vm bandwidth-set`'
        $qosCodeLevelEvidence | Should -Match 'host_mutation_code_path:\s*`implemented-wmi-storage-iops-and-network-port-bandwidth`'
        $qosCodeLevelEvidence | Should -Match 'host_mutation_performed:\s*`false`'
        $qosCodeLevelEvidence | Should -Match 'package_build_performed:\s*`superseded-by-0\.42\.47-installed-evidence`'
        $qosCodeLevelEvidence | Should -Match 'installed_actual_vm_smoke:\s*`superseded-by-0\.42\.47-installed-evidence`'

        $qosInstalledEvidence | Should -Match 'result:\s*`PASS_INSTALLED_WITH_MANUAL_ADMIN_CLOSED`'
        $qosInstalledEvidence | Should -Match 'package_version:\s*`0\.42\.47-admin-smoke`'
        $qosInstalledEvidence | Should -Match 'package_msi_sha256:\s*`9589086d092ee902b72ff7790cac5a25e6d806cdaac0d98e431a27048dc5e197`'
        $qosInstalledEvidence | Should -Match 'full_admin_host_mutation_batch:\s*`full-admin-host-mutation-gate-20260526-04247`'
        $qosInstalledEvidence | Should -Match 'installed_actual_vm_smoke_artifact:\s*`artifacts/installed-cli-qos-mutation-smoke-20260526-04247/summary\.json`'
        $qosInstalledEvidence | Should -Match 'manual_admin_package_pair:\s*`closed-0\.42\.45-to-0\.42\.47`'
        $qosInstalledEvidence | Should -Match '0\.42\.46 Superseded Diagnostic'

        $adrIndex | Should -Match '0008-hyperv-qos-mutation-policy'
        $adrIndex | Should -Match '0009-guest-execution-security-boundary-candidate'
        $adrIndex | Should -Match '0010-account-novnc-target-config-security-policy-candidate'
        $ledger | Should -Match 'post_04245_extension_phase2_5_planning_evidence:\s*`docs/ga-ready/evidence/post-04245-extension-phase2-5-planning-2026-05-26\.md`'
        $ledger | Should -Match 'post_04245_hyperv_qos_mutation_policy_adr:\s*`docs/adr/0008-hyperv-qos-mutation-policy\.md`'
        $ledger | Should -Match 'latest_hyperv_qos_mutation_code_level_evidence:\s*`docs/ga-ready/evidence/hyperv-qos-mutation-code-level-2026-05-26\.md`'
        $ledger | Should -Match 'latest_hyperv_qos_mutation_code_level_status:\s*`pass-code-level-promoted-by-04247-installed-smoke`'
        $ledger | Should -Match 'latest_hyperv_qos_mutation_installed_evidence:\s*`docs/ga-ready/evidence/hyperv-qos-mutation-installed-2026-05-26-04247\.md`'
        $ledger | Should -Match 'latest_hyperv_qos_mutation_installed_status:\s*`pass-installed-package-fullgate-actual-vm-manual-admin-closed`'
        $ledger | Should -Match 'latest_hyperv_qos_mutation_fullgate_batch:\s*`full-admin-host-mutation-gate-20260526-04247`'
        $ledger | Should -Match 'post_04245_phase3_direct_control_guard:\s*`backend-policy-first`'
        $ledger | Should -Match 'post_04245_extension_phase2_5_host_mutation_performed:\s*`false`'
        $evidenceIndex | Should -Match 'post-04245-extension-phase2-5-planning-2026-05-26'
        $evidenceIndex | Should -Match 'hyperv-qos-mutation-code-level-2026-05-26'
        $evidenceIndex | Should -Match 'hyperv-qos-mutation-installed-2026-05-26-04247'
        $controlPlaneIndex | Should -Match 'Phase 2 Hyper-V QoS Mutation Policy'
        $developerIndex | Should -Match 'Phase 2 Hyper-V QoS mutation 설치본 승격'

        foreach ($content in @($cliUsage, $cliReadme, $featureUsage, $webDesign)) {
            $content | Should -Match 'ADR-0008'
            $content | Should -Match 'vm blkio-set'
            $content | Should -Match 'vm bandwidth-set'
            $content | Should -Match 'ADR-0009|guest-exec|Guest Execution'
            $content | Should -Match 'backend-policy-first|지원 완료로 표시하지|미지원|다음 evidence gate|provider route|direct-control'
        }
    }

    It 'records ADR-0009 Guest Execution provider and direct-control contract' {
        $adr0009Path = Join-Path $script:RepoRoot 'docs/adr/0009-guest-execution-security-boundary.md'
        $adr0009CandidatePath = Join-Path $script:RepoRoot 'docs/adr/0009-guest-execution-security-boundary-candidate.md'
        $guestExecSpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary-design.md'
        $guestExecPlanPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md'
        $guestExecEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md'
        $guestExecCodeEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-provider-direct-control-code-level-2026-05-27-04253.md'
        $guestExecInstalledEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-27-04253.md'
        $guestExecActualSmokePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-actual-vm-web-tui-smoke-2026-05-27-04253-blocked.md'
        $guestExecInstalledWindowsSmokePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass.md'
        $guestExecCancelPolicyPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-running-cancel-policy-2026-05-27-04253.md'
        $guestExecRunningInterruptCodePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-running-interrupt-code-level-2026-05-28.md'
        $guestExecRunningCancelInstalledPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass.md'
        $guestExecRunningInterruptDesignPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-27-purecvisor-desktop-node-guest-execution-running-interrupt-cancel-design.md'
        $guestExecPackage04254Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04255.md'
        $guestExecFullgate04254Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04255-hostmutation.md'
        $persistentWindowsGuestPolicyPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/persistent-windows-guest-target-policy-2026-05-28-04255.md'
        $manualAdminBaselinePrepPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-04255-fullgate.md'
        $webTuiRunningCancelAffordancePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-tui-running-job-cancel-affordance-code-level-2026-05-28.md'
        $publicBoundary04254Path = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-28-04254-fullgate-evidence-rollforward-postpush-pass.md'

        foreach ($path in @($adr0009Path, $adr0009CandidatePath, $guestExecSpecPath, $guestExecPlanPath, $guestExecEvidencePath, $guestExecCodeEvidencePath, $guestExecInstalledEvidencePath, $guestExecActualSmokePath, $guestExecInstalledWindowsSmokePath, $guestExecCancelPolicyPath, $guestExecRunningInterruptCodePath, $guestExecRunningCancelInstalledPath, $guestExecRunningInterruptDesignPath, $guestExecPackage04254Path, $guestExecFullgate04254Path, $persistentWindowsGuestPolicyPath, $manualAdminBaselinePrepPath, $webTuiRunningCancelAffordancePath, $publicBoundary04254Path)) {
            $path | Should -Exist
        }

        $adr0009 = Get-Content -Raw -LiteralPath $adr0009Path
        $guestExecSpec = Get-Content -Raw -LiteralPath $guestExecSpecPath
        $guestExecPlan = Get-Content -Raw -LiteralPath $guestExecPlanPath
        $guestExecEvidence = Get-Content -Raw -LiteralPath $guestExecEvidencePath
        $guestExecCodeEvidence = Get-Content -Raw -LiteralPath $guestExecCodeEvidencePath
        $guestExecInstalledEvidence = Get-Content -Raw -LiteralPath $guestExecInstalledEvidencePath
        $guestExecActualSmoke = Get-Content -Raw -LiteralPath $guestExecActualSmokePath
        $guestExecInstalledWindowsSmoke = Get-Content -Raw -LiteralPath $guestExecInstalledWindowsSmokePath
        $guestExecCancelPolicy = Get-Content -Raw -LiteralPath $guestExecCancelPolicyPath
        $guestExecRunningInterruptCode = Get-Content -Raw -LiteralPath $guestExecRunningInterruptCodePath
        $guestExecRunningCancelInstalled = Get-Content -Raw -LiteralPath $guestExecRunningCancelInstalledPath
        $guestExecRunningInterruptDesign = Get-Content -Raw -LiteralPath $guestExecRunningInterruptDesignPath
        $guestExecPackage04254 = Get-Content -Raw -LiteralPath $guestExecPackage04254Path
        $guestExecFullgate04254 = Get-Content -Raw -LiteralPath $guestExecFullgate04254Path
        $persistentWindowsGuestPolicy = Get-Content -Raw -LiteralPath $persistentWindowsGuestPolicyPath
        $manualAdminBaselinePrep = Get-Content -Raw -LiteralPath $manualAdminBaselinePrepPath
        $webTuiRunningCancelAffordance = Get-Content -Raw -LiteralPath $webTuiRunningCancelAffordancePath
        $publicBoundary04254 = Get-Content -Raw -LiteralPath $publicBoundary04254Path
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $manualAdminDescriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $cliUsage = Get-RepoText -RelativePath 'docs/CLI_COMMAND_USAGE.md'
        $cliReadme = Get-RepoText -RelativePath 'src/DesktopNode.Cli/README.md'
        $featureUsage = Get-RepoText -RelativePath 'docs/USER_FEATURE_USAGE_SPEC.md'
        $webDesign = Get-RepoText -RelativePath 'web/DESIGN.md'

        $adr0009 | Should -Match 'DESKTOP_NODE_GUEST_EXECUTION_SECURITY_BOUNDARY_DECISION:\s*accepted-boundary-contract'
        $adr0009 | Should -Match 'implementation_status:\s*provider-direct-control-applied'
        $adr0009 | Should -Match 'credential_policy:\s*protected-secret-reference-only-no-raw-cli-args'
        $adr0009 | Should -Match 'audit_log_schema:\s*guest-execution-audit-v1-required'
        $adr0009 | Should -Match 'secret_redaction_policy:\s*guest-execution-redaction-v1-required'
        $adr0009 | Should -Match 'timeout_cancel_policy:\s*provider-timeout-queued-and-running-guest-execution-cancel'
        $adr0009 | Should -Match 'running_interrupt_decision:\s*installed-windows-guest-long-running-cancel-pass'
        $adr0009 | Should -Match 'running_interrupt_code_evidence:\s*docs/ga-ready/evidence/guest-execution-running-interrupt-code-level-2026-05-28\.md'
        $adr0009 | Should -Match 'running_interrupt_installed_evidence:\s*docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass\.md'
        $adr0009 | Should -Match 'rbac_capabilities:\s*operate, guest\.exec, guest\.channel\.configure, job\.cancel'
        $adr0009 | Should -Match 'web_tui_guest_exec_control:\s*enabled-with-confirmed-queued-provider-route'
        $adr0009 | Should -Match 'PCV_GUEST_EXEC_DISABLED'
        $adr0009 | Should -Match 'PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED'
        $adr0009 | Should -Match 'host_mutation_performed:\s*0\.42\.55-admin-smoke-full-admin-gate'
        $adr0009 | Should -Match 'package_build_performed:\s*0\.42\.55-admin-smoke'
        $adr0009 | Should -Match 'public_release:\s*not-claimed'
        $adr0009 | Should -Match 'next_product_payload_gate:\s*0\.42\.59-admin-smoke-package-fullgate-manual-admin'

        $guestExecSpec | Should -Match 'status:\s*`approved-boundary-contract`'
        $guestExecSpec | Should -Match 'credential_ref'
        $guestExecSpec | Should -Match 'guest-execution-audit-v1'
        $guestExecSpec | Should -Match 'guest-execution-redaction-v1'
        $guestExecSpec | Should -Match 'POST /api/v1/vms/\{vm\}/guest/exec/preview'
        $guestExecSpec | Should -Match 'pcvcli vm guest-agent-ensure-channel <vm> --dry-run'
        $guestExecSpec | Should -Match 'pcvcli vm guest-exec <vm> --credential-ref <ref>'
        $guestExecSpec | Should -Match 'package_build_performed:\s*`false`'

        $guestExecPlan | Should -Match '# Guest Execution Security Boundary Implementation Plan'
        $guestExecPlan | Should -Match 'Runtime policy'
        $guestExecPlan | Should -Match 'GuestExecutionRedactor'
        $guestExecPlan | Should -Match 'PowerShellDirectGuestExecutionProvider'
        $guestExecPlan | Should -Match 'Manual-admin package-pair'

        $guestExecEvidence | Should -Match 'result:\s*`PASS_DOCS_CONTRACT`'
        $guestExecEvidence | Should -Match 'product_payload_change_detected:\s*`false`'
        $guestExecEvidence | Should -Match 'host_mutation_performed:\s*`false`'
        $guestExecEvidence | Should -Match 'package_build_performed:\s*`false`'
        $guestExecEvidence | Should -Match 'manual_admin_package_pair_performed:\s*`false`'
        $guestExecEvidence | Should -Match 'not-run-no-product-payload-change-docs-contract-only'
        $guestExecEvidence | Should -Match 'public trusted signing|external stable publication'

        $guestExecCodeEvidence | Should -Match 'result:\s*`PASS_CODE_AND_INSTALLED_PROMOTED`'
        $guestExecCodeEvidence | Should -Match 'scope:\s*`guest-execution-provider-channel-verify-repair-web-tui-direct-control`'
        $guestExecCodeEvidence | Should -Match 'version:\s*`0\.42\.53-admin-smoke`'
        $guestExecCodeEvidence | Should -Match 'host_mutation_performed:\s*`true-via-04253-fullgate`'
        $guestExecCodeEvidence | Should -Match 'IDesktopNodeHyperVGuestExecutionProvider'
        $guestExecCodeEvidence | Should -Match 'guest-execution-audit-v1'
        $guestExecCodeEvidence | Should -Match 'guest-execution-redaction-v1'
        $guestExecInstalledEvidence | Should -Match 'guest_execution\.enabled=true'
        $guestExecInstalledEvidence | Should -Match 'execute_enabled=true'
        $guestExecInstalledEvidence | Should -Match 'guest-execution-preview\.v1'
        $guestExecInstalledEvidence | Should -Match 'vm\.guest\.exec'
        $guestExecInstalledEvidence | Should -Match 'Secret echo guard'
        $guestExecActualSmoke | Should -Match 'result:\s*`BLOCKED_BY_MISSING_PROTECTED_GUEST_CREDENTIAL_AND_EMPTY_VM_INVENTORY`'
        $guestExecActualSmoke | Should -Match 'vm_inventory:\s*`empty`'
        $guestExecActualSmoke | Should -Match 'credential_inventory:\s*`no-purecvisor-guest-credential-target`'
        $guestExecActualSmoke | Should -Match 'windows_install_media_inventory:\s*`found`'
        $guestExecActualSmoke | Should -Match 'windows_install_media_path:\s*`D:\\Downloads\\Windows\.iso`'
        $guestExecActualSmoke | Should -Match 'Rocky-10\.1-x86_64-minimal\.iso'
        $guestExecActualSmoke | Should -Match 'Windows\.iso'
        $guestExecActualSmoke | Should -Match 'windows_iso_boot_shell_smoke:\s*`pass-create-start-readback-poweroff-delete-cleaned-up`'
        $guestExecActualSmoke | Should -Match 'guest-execution-windows-iso-boot-shell-smoke-20260527-04253-r1'
        $guestExecActualSmoke | Should -Match 'job-e5ed7403a26a496baf0d481fee1c608c'
        $guestExecActualSmoke | Should -Match 'job-9f0d339612994e39850df0085757814f'
        $guestExecActualSmoke | Should -Match 'job-1bc7df7c12284b9c89ad3c37ec39a26a'
        $guestExecActualSmoke | Should -Match 'job-b2182509e138466eb45b7b5404f339cf'
        $guestExecActualSmoke | Should -Match 'job-b858de5e7fd7498a9f59a6573a19877a'
        $guestExecActualSmoke | Should -Match 'PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED'
        $guestExecActualSmoke | Should -Match 'pcvtui --smoke-once vm'
        $guestExecActualSmoke | Should -Match 'web_listener_status:\s*`200`'
        $guestExecActualSmoke | Should -Match 'guest-execution-audit-v1'
        $guestExecActualSmoke | Should -Match 'guest-execution-redaction-v1'
        $guestExecActualSmoke | Should -Match 'successor_evidence:\s*`docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-27-04253-pass\.md`'
        $guestExecInstalledWindowsSmoke | Should -Match 'result:\s*`PASS`'
        $guestExecInstalledWindowsSmoke | Should -Match 'pcv-guest-installed-04253-r1'
        $guestExecInstalledWindowsSmoke | Should -Match 'credential_ref_type:\s*`dpapi-local-machine`'
        $guestExecInstalledWindowsSmoke | Should -Match 'credential_ref:\s*`dpapi:<protected-file>`'
        $guestExecInstalledWindowsSmoke | Should -Match 'Channel verify job'
        $guestExecInstalledWindowsSmoke | Should -Match 'job-92e44ca99cde460b9e34567168dbb7cd'
        $guestExecInstalledWindowsSmoke | Should -Match 'job-0e05ae5a574d49a5822237337c1e9ad3'
        $guestExecInstalledWindowsSmoke | Should -Match 'windows-powershell-direct'
        $guestExecInstalledWindowsSmoke | Should -Match '/pcv-config\.js'
        $guestExecInstalledWindowsSmoke | Should -Match 'TUI smoke'
        $guestExecInstalledWindowsSmoke | Should -Match 'token/password value observed `false`'
        $guestExecCancelPolicy | Should -Match 'result:\s*`PASS_POLICY_CONFIRMED_RUNNING_INTERRUPT_NOT_SUPPORTED`'
        $guestExecCancelPolicy | Should -Match 'runtime_policy_cancel_queued_only:\s*`true`'
        $guestExecCancelPolicy | Should -Match 'runtime_policy_running_interrupt:\s*`false`'
        $guestExecCancelPolicy | Should -Match 'PCV_JOB_NOT_CANCELABLE'
        $guestExecRunningInterruptCode | Should -Match 'result:\s*`PASS_CODE_LEVEL_RUNNING_GUEST_EXEC_CANCEL_TOKEN_PATH`'
        $guestExecRunningInterruptCode | Should -Match 'job_cancel_running_response:\s*`202-cancel-requested`'
        $guestExecRunningInterruptCode | Should -Match 'terminal_state:\s*`canceled`'
        $guestExecRunningInterruptCode | Should -Match 'queued-and-running-guest-execution-cancel-with-provider-token-interrupt'
        $guestExecRunningInterruptCode | Should -Match 'PowerShell Direct bridge process kill on cancellation token'
        $guestExecRunningCancelInstalled | Should -Match 'result:\s*`PASS_INSTALLED_WINDOWS_GUEST_RUNNING_CANCEL`'
        $guestExecRunningCancelInstalled | Should -Match 'version:\s*`0\.42\.54-admin-smoke`'
        $guestExecRunningCancelInstalled | Should -Match 'create_job_id:\s*`job-b06eb90e549a481bbf4003399b5604f8`'
        $guestExecRunningCancelInstalled | Should -Match 'final_status:\s*`canceled`'
        $guestExecRunningCancelInstalled | Should -Match 'native_error_code:\s*`PCV_NATIVE_OPERATION_CANCELED`'
        $guestExecRunningCancelInstalled | Should -Match 'running_interrupt_observed:\s*`true`'
        $guestExecPackage04254 | Should -Match 'version:\s*`0\.42\.55-admin-smoke`'
        $guestExecPackage04254 | Should -Match 'msi_sha256:\s*`530d5605a99ff607a8030192a23fd4ba8bdb703793290b3e09e446dc61121627`'
        $guestExecPackage04254 | Should -Match 'provenance_commit:\s*`958052181012f7d1be6ccff535316bfaeeef07df`'
        $guestExecFullgate04254 | Should -Match 'result:\s*`PASS`'
        $guestExecFullgate04254 | Should -Match 'full_gate_msi_sha256:\s*`cfd4d3c1cc22fff41f5c9b0f79f2a40df17b4ae91b3f4e0e24f43e4d096230eb`'
        $guestExecFullgate04254 | Should -Match 'provenance_commit:\s*`958052181012f7d1be6ccff535316bfaeeef07df`'
        $guestExecRunningInterruptDesign | Should -Match 'status:\s*`installed-windows-guest-long-running-cancel-pass`'
        $guestExecRunningInterruptDesign | Should -Match 'Provider session handle registry'
        $guestExecRunningInterruptDesign | Should -Match 'installed_evidence:\s*`docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass\.md`'
        $persistentWindowsGuestPolicy | Should -Match 'result:\s*`POLICY_CONFIRMED_KEEP_AFTER_04255_FULLGATE`'
        $persistentWindowsGuestPolicy | Should -Match 'guest_family=windows'
        $persistentWindowsGuestPolicy | Should -Match 'persistent_policy=keep-until-next-evidence-cycle'
        $manualAdminBaselinePrep | Should -Match 'result:\s*`BLOCKED_BY_INSTALLED_BASELINE_VERSION_MISMATCH`'
        $manualAdminBaselinePrep | Should -Match 'blocked-by-installed-baseline-version-mismatch'
        $manualAdminBaselinePrep | Should -Match 'current-version-rebaseline-or-dedicated-clean-host'
        $webTuiRunningCancelAffordance | Should -Match 'result:\s*`PASS_CODE_LEVEL_PROMOTED_BY_04255_PACKAGE`'
        $webTuiRunningCancelAffordance | Should -Match 'Cancel running guest exec'
        $webTuiRunningCancelAffordance | Should -Match 'running-guest-execution'
        $publicBoundary04254 | Should -Match 'result:\s*`PASS`'
        $publicBoundary04254 | Should -Match 'run_id:\s*`26558089193`'
        $publicBoundary04254 | Should -Match 'job_id:\s*`78234262641`'
        $publicBoundary04254 | Should -Match 'head_sha:\s*`958052181012f7d1be6ccff535316bfaeeef07df`'

        $adrIndex | Should -Match '0009-guest-execution-security-boundary\.md'
        $adrIndex | Should -Match '0009-guest-execution-security-boundary-candidate\.md.*대체됨'
        $ledger | Should -Match 'latest_guest_execution_security_boundary_evidence:\s*`docs/ga-ready/evidence/guest-execution-provider-direct-control-code-level-2026-05-27-04253\.md`'
        $ledger | Should -Match 'latest_guest_execution_security_boundary_status:\s*`pass-code-installed-provider-and-actual-credentialed-smoke`'
        $ledger | Should -Match 'latest_guest_execution_package_gate_decision:\s*`package-fullgate-installed-current-card-actual-guest-exec-pass-manual-admin-readiness-blocked`'
        $ledger | Should -Match 'latest_phase4_guest_execution_status:\s*`provider-execute-channel-web-tui-direct-control-running-cancel-installed`'
        $ledger | Should -Match 'latest_phase4_guest_execution_actual_windows_credentialed_smoke_evidence:\s*`docs/ga-ready/evidence/guest-execution-installed-windows-vhd-web-tui-smoke-2026-05-28-04255-pass\.md`'
        $ledger | Should -Match 'latest_phase4_guest_execution_actual_windows_credentialed_smoke_status:\s*`pass-installed-windows-vhd-credentialed-guest-exec-04255`'
        $ledger | Should -Match 'latest_phase4_guest_execution_persistent_windows_guest_policy_evidence:\s*`docs/ga-ready/evidence/persistent-windows-guest-target-policy-2026-05-28-04255\.md`'
        $ledger | Should -Match 'latest_phase4_guest_execution_persistent_windows_guest_policy_status:\s*`keep-after-04255-fullgate`'
        $ledger | Should -Match 'latest_phase4_guest_execution_running_cancel_policy_evidence:\s*`docs/ga-ready/evidence/guest-execution-running-cancel-installed-2026-05-28-04254-pass\.md`'
        $ledger | Should -Match 'latest_phase4_guest_execution_running_cancel_policy_status:\s*`pass-installed-windows-guest-running-cancel`'
        $ledger | Should -Match 'latest_phase4_guest_execution_running_interrupt_design:\s*`docs/superpowers/specs/2026-05-27-purecvisor-desktop-node-guest-execution-running-interrupt-cancel-design\.md`'
        $ledger | Should -Match 'latest_manual_admin_04250_04254_baseline_host_prep_evidence:\s*`docs/ga-ready/evidence/manual-admin-package-pair-closure-2026-05-28-04250-04254-blocked-after-04255-fullgate\.md`'
        $ledger | Should -Match 'current_public_boundary_pr:\s*`none-post-04259-public-boundary-docs-maintenance-main-push`'
        $ledger | Should -Match 'latest_operator_surface_running_job_cancel_affordance_status:\s*`pass-installed-04255-current-card`'
        $evidenceIndex | Should -Match 'Guest Execution Security Boundary Contract'
        $controlPlaneIndex | Should -Match 'ADR-0009 Guest Execution Security Boundary Contract'
        $developerIndex | Should -Match 'guest-execution-security-boundary\.md'
        $manualAdminDescriptor | Should -Match 'latest_guest_execution_security_boundary_status:\s*`pass-code-installed-provider-and-actual-credentialed-smoke`'
        $manualAdminDescriptor | Should -Match 'latest_guest_execution_actual_windows_credentialed_smoke_status:\s*`pass-installed-windows-vhd-credentialed-guest-exec-04255`'
        $manualAdminDescriptor | Should -Match 'latest_guest_execution_persistent_windows_guest_policy_status:\s*`keep-after-04255-fullgate`'
        $manualAdminDescriptor | Should -Match 'latest_guest_execution_running_cancel_policy_status:\s*`pass-installed-windows-guest-running-cancel`'
        $manualAdminDescriptor | Should -Match 'latest_manual_admin_04250_04254_baseline_host_prep_status:\s*`blocked-missing-dedicated-baseline-host-after-04255-fullgate`'

        foreach ($content in @($cliUsage, $cliReadme, $featureUsage, $webDesign)) {
            $content | Should -Match 'ADR-0009'
            $content | Should -Match 'boundary\s*contract|security boundary\s*contract|보안 경계'
            $content | Should -Match '0\.42\.53|0\.42\.54|0\.42\.55|provider route|direct-control|queued provider|running cancel'
        }
    }

    It 'keeps internal admin-smoke public distribution evidence out of scope until ADR changes' {
        $adr0005 = Get-RepoText -RelativePath 'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        $adr0006 = Get-RepoText -RelativePath 'docs/adr/0006-internal-private-network-distribution.md'
        $publicMatrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $internalMatrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $operatorSurface = Get-RepoText -RelativePath 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04231.md'

        foreach ($content in @($adr0005, $adr0006, $publicMatrix, $internalMatrix, $releaseBoundary, $evidenceIndex, $operatorSurface)) {
            $content | Should -Match '0\.42\.31-admin-smoke|0\.42\.30-admin-smoke|0\.42\.29-admin-smoke|0\.42\.28-admin-smoke|0\.42\.27-admin-smoke|0\.42\.26-admin-smoke|0\.42\.25-admin-smoke|0\.42\.24-admin-smoke'
            $content | Should -Match 'ADR-0006|internal-private-network-only|DESKTOP_NODE_PRIVATE_NETWORK_DISTRIBUTION_DECISION'
            $content | Should -Match 'public trusted signing|Public trusted signing'
            $content | Should -Match 'external stable publication|외부 stable publication'
            $content | Should -Match 'out-of-scope|not-claimed'
        }

        $adr0005 | Should -Match '(?s)0\.42\.24-admin-smoke.*public trusted signing'
        $adr0005 | Should -Match '(?s)ADR 변경 없이는.*out-of-scope'
        $adr0006 | Should -Match 'scope_lock_latest_internal_admin_smoke:\s*0\.42\.56-admin-smoke'
        $publicMatrix | Should -Match 'scope_lock_latest_internal_admin_smoke:\s*0\.42\.56-admin-smoke'
        $publicMatrix | Should -Match 'scope_lock_latest_internal_admin_smoke_evidence:\s*docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256\.md'
        $publicMatrix | Should -Match 'scope_change_requires_adr:\s*true'
        $publicMatrix | Should -Match 'scope_lock_public_trusted_signing:\s*out-of-scope'
        $publicMatrix | Should -Match 'scope_lock_external_stable_publication:\s*out-of-scope'
        $internalMatrix | Should -Match 'public_distribution_candidate:\s*closed-not-adopted'
        $internalMatrix | Should -Match 'public_trusted_signing:\s*out-of-scope'
        $internalMatrix | Should -Match 'external_stable_publication:\s*out-of-scope'
        $operatorSurface | Should -Match 'public_trusted_signing:\s*`?not-claimed`?'
        $operatorSurface | Should -Match 'external_stable_publication:\s*`?not-claimed`?'
    }

    It 'records 0.42.11 native repair package-pair and historical full gate promotion' {
        $productEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/product-wrapper-native-repair-package-2026-05-13-04211.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-13-0429-04211.md'
        $hostMutationEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-04211-hostmutation.md'

        $productEvidencePath | Should -Exist
        $manualCampaignPath | Should -Exist
        $hostMutationEvidencePath | Should -Exist

        $productEvidence = Get-Content -Raw -LiteralPath $productEvidencePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $hostMutationEvidence = Get-Content -Raw -LiteralPath $hostMutationEvidencePath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'

        foreach ($content in @($productEvidence, $manualCampaign, $hostMutationEvidence, $evidenceIndex, $controlPlaneIndex, $matrix, $descriptor, $classification, $operationsGuide)) {
            $content | Should -Match '0\.42\.11-admin-smoke'
            $content | Should -Match '987beb51025a5aa926df7d9a905019b4d6d29705'
            $content | Should -Match '750d317864c509f76216cdbc6fde6c8baff0658565819583d07decec00fdb7e1'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        $manualCampaign | Should -Match '0\.42\.9-admin-smoke -> 0\.42\.11-admin-smoke'
        $manualCampaign | Should -Match '734114e0ea7c9d486a1d329cd551a6abc34d20f3801a944bd5dbcb8c1c4a9991'
        $manualCampaign | Should -Match 'clean-host.*Windows Update|Windows Update'
        $manualCampaign | Should -Match 'Burn'
        $manualCampaign | Should -Match 'MSIX'
        $manualCampaign | Should -Match 'manual-admin-campaign-descriptor-20260513-0429-04211'
        $productEvidence | Should -Match 'wrapper_repair_used_native_service_action|native service-action'
        $productEvidence | Should -Match 'wrapper_repair_skipped_outer_start|outer start'
        $hostMutationEvidence | Should -Match 'full-admin-host-mutation-gate-20260513-0429-04211'
        $hostMutationEvidence | Should -Match '902e175cd6354843da2c928e2b6772f04d40240f02783e4edfed460ba0f9fce2'
        $hostMutationEvidence | Should -Match 'batch_evidence\.status.*available|batch_evidence.status`: `available'
        $descriptor | Should -Match 'closed-package-pair-04211-04212-pass'
        $descriptor | Should -Match 'historical_post_04212_manual_admin_next_package_pair_candidate_status:\s*`not-opened-no-new-product-payload`'
        $matrix | Should -Match 'manual_admin_0429_04211_campaign:\s*pass'
        $matrix | Should -Match 'wrapper_repair_used_native_service_action:\s*true'
    }

    It 'records 0.42.10 duplicate outer start RCA and defers the next package-pair candidate' {
        $rcaPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210.md'
        $rcaPath | Should -Exist

        $rca = Get-Content -Raw -LiteralPath $rcaPath
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $classification = Get-RepoText -RelativePath 'docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'

        foreach ($content in @($rca, $evidenceIndex, $controlPlaneIndex, $descriptor, $matrix, $classification, $developerIndex, $adrIndex, $operationsGuide, $readme, $agents, $packagingReadme)) {
            $content | Should -Match 'product-wrapper-native-repair-duplicate-start-rca-2026-05-13-04210'
            $content | Should -Match '0\.42\.10-admin-smoke'
            $content | Should -Match '0\.42\.11-admin-smoke|04211'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed|out-of-scope'
        }

        foreach ($content in @($rca, $evidenceIndex, $controlPlaneIndex, $matrix, $developerIndex, $adrIndex, $operationsGuide, $readme, $packagingReadme)) {
            $content | Should -Match '1056|already running'
            $content | Should -Match 'native-service-action-controls-final-state'
        }

        $rca | Should -Match 'bf84deb1ddca4cd4af176fe273a54a42c1d24dfa564bb7e2614b241d10b4c273'
        $rca | Should -Match '05a107f4803ec8ed1e08f7aeba1b49fa3795c7d16565db8f904fd599ba07633f'
        $rca | Should -Match 'd7d5ba38ee1d4f74676477eb13701af65abca008'
        $rca | Should -Match '987beb51025a5aa926df7d9a905019b4d6d29705'
        $rca | Should -Match '04-wrapper-repair-installed-batch-root\.json'
        $rca | Should -Match '05-rollback-after-wrapper-start-1056\.json'
        $rca | Should -Match '14f56fd7348572e1757413657a68cd17c0aeca52'

        $descriptor | Should -Match 'manual_admin_next_package_pair_candidate_trigger:\s*`next-product-payload-change`'
        $descriptor | Should -Match 'historical_post_04212_manual_admin_next_package_pair_candidate_next_version_hint:\s*`0\.42\.13-admin-smoke`'
        $descriptor | Should -Match 'post_merge_package_provenance_decision:\s*`deferred-no-new-product-payload-after-04211`'
        $matrix | Should -Match 'manual_admin_0429_04210_duplicate_start_rca:\s*historical-closed-by-04211'
        $matrix | Should -Match 'previous_04221_manual_admin_next_package_pair_candidate_trigger:\s*web-console-diagnostics-direct-expose-after-04221'
        $matrix | Should -Match 'post_merge_04211_package_rebuild_decision:\s*deferred-no-new-product-payload'
    }

    It 'records 0.42.9 eventlog timeout package build and 0429 full gate promotion' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/batch-evidence-root-service-action-package-2026-05-13-0429.md'
        $evidencePath | Should -Exist

        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $hostMutationEvidence = Get-RepoText -RelativePath 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation.md'
        $candidateEvidence = Get-RepoText -RelativePath 'docs/ga-ready/evidence/manual-admin-campaign-candidate-2026-05-13-0428-0429.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlaneIndex = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $installerReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/installer/README.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'

        foreach ($content in @($evidence, $hostMutationEvidence, $candidateEvidence, $matrix, $packagingReadme, $installerReadme, $developerIndex, $readme, $agents)) {
            $content | Should -Match '0\.42\.9-admin-smoke'
            $content | Should -Match 'batch-evidence-root-service-action-package-2026-05-13-0429|admin-smoke-package-20260513-0429|full-admin-host-mutation-gate-20260513-040213-0429'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }
        foreach ($content in @($evidenceIndex, $controlPlaneIndex)) {
            $content | Should -Match '0\.42\.9-admin-smoke'
            $content | Should -Match 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-13-0429-hostmutation\.md'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication|not-claimed'
        }

        $evidence | Should -Match 'artifacts/admin-smoke-package-20260513-0429'
        $evidence | Should -Match 'a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb'
        $evidence | Should -Match 'f0620f2e18ae25de8751333684cb74b5051dcdc6'

        $evidence | Should -Match 'host_mutation_performed:\s*`?false'
        $evidence | Should -Match 'eventlog-default-transition-timeout-seconds'
        $evidence | Should -Match 'timeout_guard_status'
        $evidence | Should -Match 'BATCH_EVIDENCE_ROOT'
        $evidence | Should -Match 'repair-installed'
        $evidence | Should -Match '보존|preserve'
        $hostMutationEvidence | Should -Match 'full-admin-host-mutation-gate-20260513-040213-0429'
        $hostMutationEvidence | Should -Match '78d8737a9467d0d7b0a72971c71e27bd2604cc7cf5c080f3916d3a6953e48cd9'
        $hostMutationEvidence | Should -Match 'batch_evidence\.status.*available|batch_evidence.status`: `available'
        $candidateEvidence | Should -Match 'CANDIDATE_UPDATE_ROLLBACK_ONLY'
        $candidateEvidence | Should -Match '0\.42\.8-admin-smoke -> 0\.42\.9-admin-smoke'
        $candidateEvidence | Should -Match '7c813e94224056013d46de97199df74f3ecd3b572d7aa4fa3ac8c0b07446686f'
        $matrix | Should -Match 'batch_evidence_root_service_action:\s*native-wrapper-package-build-pass|batch_evidence_root_service_action:\s*code-level-and-package-build-pass'
        $matrix | Should -Match 'manual_admin_next_package_pair_candidate:\s*pending-next-product-payload-after-04212'
        $matrix | Should -Match 'manual_admin_0428_0429_candidate:\s*historical-update-rollback-only'
        $descriptor | Should -Match 'manual-admin-campaign-candidate-2026-05-13-0428-0429|full-admin-host-mutation-gate-2026-05-13-0429-hostmutation'
        $controlPlaneIndex | Should -Match 'full host mutation current claim은 04212 evidence가 소유|full host mutation current claim은 04212.*소유'
        $packagingReadme | Should -Match 'BATCH_EVIDENCE_ROOT'
        $installerReadme | Should -Match 'eventlog-default-transition-timeout-seconds|Event Log default transition timeout'
    }

    It 'preserves the 0.41.5 full admin host mutation gate as historical evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0415-hostmutation.md'

        $evidencePath | Should -Exist
        $content = Get-Content -Raw -LiteralPath $evidencePath

        $content | Should -Match '0\.41\.5-admin-smoke'
        $content | Should -Match 'full-admin-host-mutation-gate-20260510-195837-0415'
        $content | Should -Match 'routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415'
        $content | Should -Match 'os-mutation-gates-batch-profile-20260510-195837-0415'
        $content | Should -Match 'c9efe852db0e3fb4d120bc5058c56a38c7cb30db'
        $content | Should -Match 'add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6'
        $content | Should -Match 'AllowUnsignedDev'
        $content | Should -Match 'Service/MSI/Hyper-V'
        $content | Should -Match 'firewall.*Event Log.*trust-store|Event Log.*firewall.*trust-store'
        $content | Should -Match 'Installed manifest version: `0\.41\.5-admin-smoke`'
        $content | Should -Match 'LAN listener smoke'
        $content | Should -Match 'http://127\.0\.0\.1/'
        $content | Should -Match '/pcv-config\.js'
        $content | Should -Match 'PCV_AUTH_REQUIRED'
        $content | Should -Match 'public trusted signing.*external stable publication|public trusted signing.*외부 stable publication'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $doc = Get-RepoText -RelativePath $path
            $doc | Should -Match '0\.41\.5-admin-smoke'
            $doc | Should -Match 'full-admin-host-mutation-gate-20260510-195837-0415|routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415|os-mutation-gates-batch-profile-20260510-195837-0415'
            $doc | Should -Match 'c9efe852db0e3fb4d120bc5058c56a38c7cb30db|add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6|AllowUnsignedDev'
            $doc | Should -Match 'public trusted signing|외부 stable publication'
        }
    }

    It 'preserves the 0.41.2 full admin host mutation gate as historical evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0412-hostmutation.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'

        $evidencePath | Should -Exist
        $content = Get-Content -Raw -LiteralPath $evidencePath

        $content | Should -Match '0\.41\.2-admin-smoke'
        $content | Should -Match 'full-admin-host-mutation-gate-20260510-161416-0412'
        $content | Should -Match 'routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412'
        $content | Should -Match 'os-mutation-gates-batch-profile-20260510-161416-0412'
        $content | Should -Match 'd098f0fc631ff1799d7dd238a84e896fe8616230'
        $content | Should -Match 'ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0'
        $content | Should -Match 'AllowUnsignedDev'
        $content | Should -Match 'Service/MSI/Hyper-V'
        $content | Should -Match 'firewall.*Event Log.*trust-store|Event Log.*firewall.*trust-store'
        $content | Should -Match 'http://127\.0\.0\.1/'
        $content | Should -Match '/pcv-config\.js'
        $content | Should -Match 'PCV_AUTH_REQUIRED'
        $content | Should -Match 'public trusted signing.*external stable publication|public trusted signing.*외부 stable publication'
        $ledger | Should -Match 'Evidence Group: Full Admin Host Mutation Gate 2026-05-10 0\.41\.2 Host Mutation'
        $ledger | Should -Match 'full-admin-host-mutation-gate-20260510-161416-0412'
        $adrIndex | Should -Match '0\.41\.2-admin-smoke` full gate.*historical evidence'
    }

    It 'records the 0.41.0 full admin host mutation gate and installed account smoke as account-linked evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-10-0410-account-rerun.md'

        $evidencePath | Should -Exist
        $content = Get-Content -Raw -LiteralPath $evidencePath

        $content | Should -Match '0\.41\.0-admin-smoke'
        $content | Should -Match 'full-admin-host-mutation-gate-20260510-154831-0410-account-rerun'
        $content | Should -Match 'routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun'
        $content | Should -Match 'os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun'
        $content | Should -Match 'a3226ef637ea895d2f2a9956599e0d5e79d00410'
        $content | Should -Match 'cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d'
        $content | Should -Match 'AllowUnsignedDev'
        $content | Should -Match 'Service/MSI/Hyper-V'
        $content | Should -Match 'firewall.*Event Log.*trust-store|Event Log.*firewall.*trust-store'
        $content | Should -Match 'Installed account login smoke:\s*PASS|account login smoke.*PASS'
        $content | Should -Match 'public trusted signing.*외부 stable publication|public trusted signing.*external stable publication'
    }

    It 'records the 0.39.1 frontend host mutation run and installed Web Console QA evidence' {
        $hostEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-09-0391-frontend.md'
        $qaEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-console-installed-listener-qa-2026-05-09.md'
        $destructiveUiEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-console-destructive-lifecycle-ui-2026-05-09.md'
        $destructiveUiRunnerPath = Join-Path $script:RepoRoot 'web/scripts/capture-destructive-lifecycle-ui-qa.mjs'

        $hostEvidencePath | Should -Exist
        $qaEvidencePath | Should -Exist
        $destructiveUiEvidencePath | Should -Exist
        $destructiveUiRunnerPath | Should -Exist

        $hostEvidence = Get-Content -Raw -LiteralPath $hostEvidencePath
        $hostEvidence | Should -Match 'full-admin-host-mutation-gate-20260509-122028-0391-frontend'
        $hostEvidence | Should -Match 'routeparity-service-msi-hyperv-batch-profile-20260509-122028-0391-frontend'
        $hostEvidence | Should -Match 'os-mutation-gates-batch-profile-20260509-122028-0391-frontend'
        $hostEvidence | Should -Match 'd8e7e162a13817dc869f30712d77c5c036981786'
        $hostEvidence | Should -Match 'f5086e64a58bdb43a8196574dacf383d600c5cccca0f60aeb99ed3f95b65bd73'
        $hostEvidence | Should -Match 'host_mutation_performed:\s*true'
        $hostEvidence | Should -Match 'public_trusted_signing:\s*excluded'
        $hostEvidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $qaEvidence = Get-Content -Raw -LiteralPath $qaEvidencePath
        $qaEvidence | Should -Match 'artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b'
        $qaEvidence | Should -Match 'full-admin-host-mutation-gate-20260509-130105-0391-frontend-final2'
        $qaEvidence | Should -Match 'http://127\.0\.0\.1:7777/'
        $qaEvidence | Should -Match 'diagnostic_create_clicked:\s*true|Diagnostic create clicked:\s*`true`'
        $qaEvidence | Should -Match 'diagnostic_download_clicked:\s*true|Diagnostic download clicked:\s*`true`'
        $qaEvidence | Should -Match 'token_value_observed:\s*false'
        $qaEvidence | Should -Match 'dashboard-wide\.png'
        $qaEvidence | Should -Match 'troubleshooting-diagnostics\.png'
        $qaEvidence | Should -Match '509bb151d6794dd8bca2e073712f91f2f35ec875d347edb50d82ddca50f624ad'
        $qaEvidence | Should -Match '5259a93011735b74bec6c759138bfa79a4dbbc2b69fa85f7a930bd8acb1a02fe'
        $qaEvidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $qaEvidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $destructiveUiEvidence = Get-Content -Raw -LiteralPath $destructiveUiEvidencePath
        $destructiveUiEvidence | Should -Match 'web-console-destructive-lifecycle-ui-2026-05-09'
        $destructiveUiEvidence | Should -Match 'artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391'
        $destructiveUiEvidence | Should -Match 'capture-destructive-lifecycle-ui-qa\.mjs'
        $destructiveUiEvidence | Should -Match 'host_mutation_performed:\s*true'
        $destructiveUiEvidence | Should -Match 'mutation_source:\s*installed-listener-web-console-ui'
        $destructiveUiEvidence | Should -Match 'vm\.create'
        $destructiveUiEvidence | Should -Match 'vm\.restart'
        $destructiveUiEvidence | Should -Match 'vm\.poweroff'
        $destructiveUiEvidence | Should -Match 'checkpoint\.restore'
        $destructiveUiEvidence | Should -Match 'checkpoint\.delete'
        $destructiveUiEvidence | Should -Match 'vm\.delete'
        $destructiveUiEvidence | Should -Match 'cleanup\.vm_absent_after_delete=true'
        $destructiveUiEvidence | Should -Match 'token_value_observed:\s*false'
        $destructiveUiEvidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $destructiveUiEvidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $ledger | Should -Match 'full-admin-host-mutation-gate-20260509-122028-0391-frontend'
        $ledger | Should -Match 'web-console-installed-listener-qa-2026-05-09'
        $ledger | Should -Match 'artifacts/web-console-installed-listener-qa-20260509-130105-0391-frontend-final2b'
        $ledger | Should -Match 'web-console-destructive-lifecycle-ui-2026-05-09'
        $ledger | Should -Match 'artifacts/web-console-destructive-lifecycle-ui-20260509-150353-0391'
        $ledger | Should -Match 'cleanup\.vm_absent_after_delete=true'
        $ledger | Should -Match 'token_value_observed:\s*false'

        $aggregate = Get-RepoText -RelativePath 'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md'
        $aggregate | Should -Match 'full-admin-host-mutation-gate-20260509-122028-0391-frontend'
        $aggregate | Should -Match 'installed Web Console browser QA'
        $aggregate | Should -Match 'Web Console destructive lifecycle UI run'
        $aggregate | Should -Match 'pcv-spike-ui-20260509-150353'
        $aggregate | Should -Match 'token_value_observed=false'
    }

    It 'records the Web/API port split code-level evidence and current documentation contract' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-api-port-split-code-level-2026-05-10.md'
        $installedEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/web-api-port-split-installed-listener-2026-05-10.md'

        $evidencePath | Should -Exist
        $installedEvidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $installedEvidence = Get-Content -Raw -LiteralPath $installedEvidencePath

        $evidence | Should -Match 'web-api-port-split-code-level-2026-05-10'
        $evidence | Should -Match 'web_console_prefix:\s*http://127\.0\.0\.1:80/'
        $evidence | Should -Match 'web_console_url:\s*http://127\.0\.0\.1/'
        $evidence | Should -Match 'api_prefix:\s*http://127\.0\.0\.1:7777/'
        $evidence | Should -Match 'api_route_prefix:\s*http://127\.0\.0\.1:7777/api/v1/\.\.\.'
        $evidence | Should -Match 'web_api_same_port:\s*false'
        $evidence | Should -Match '/pcv-config\.js'
        $evidence | Should -Match 'PCV_API_ROUTE_ON_WEB_PORT'
        $evidence | Should -Match 'cors_allowed_origin:\s*web-listener-origin'
        $evidence | Should -Match 'installed_listener_execution:\s*not-run'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'https_443_binding:\s*not-run'
        $evidence | Should -Match 'tls_binding:\s*not-run'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'
        $evidence | Should -Match 'web-api-port-split-installed-listener-2026-05-10'

        $installedEvidence | Should -Match 'web-api-port-split-installed-listener-2026-05-10-0392'
        $installedEvidence | Should -Match 'artifacts/installed-port-split-20260510-010714-0392'
        $installedEvidence | Should -Match 'artifacts/web-console-installed-listener-qa-20260510-010714-0392-port80'
        $installedEvidence | Should -Match 'payload_version:\s*0\.39\.2-port-split-smoke'
        $installedEvidence | Should -Match 'installed_listener_execution:\s*installed-listener-pass'
        $installedEvidence | Should -Match 'host_mutation_performed:\s*true'
        $installedEvidence | Should -Match '--web-prefix "http://127\.0\.0\.1:80/"'
        $installedEvidence | Should -Match 'GET http://127\.0\.0\.1/ -> 200'
        $installedEvidence | Should -Match 'GET http://127\.0\.0\.1:7777/api/v1/runtime/policy -> 200'
        $installedEvidence | Should -Match 'PCV_API_ROUTE_ON_WEB_PORT'
        $installedEvidence | Should -Match 'OPTIONS http://127\.0\.0\.1:7777/api/v1/runtime/policy -> 204'
        $installedEvidence | Should -Match 'diagnostic create/download clicked:\s*`true`/`true`'
        $installedEvidence | Should -Match 'missing button labels:\s*`0`'
        $installedEvidence | Should -Match 'unlabeled inputs:\s*`0`'
        $installedEvidence | Should -Match 'token value observed:\s*`false`'
        $installedEvidence | Should -Match 'https_443_binding:\s*not-run'
        $installedEvidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $installedEvidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/USER_GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'web/DESIGN.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'http://127\.0\.0\.1/'
            $content | Should -Match 'http://127\.0\.0\.1:7777/api/v1'
            $content | Should -Match 'web-api-port-split-code-level-2026-05-10|web-api-port-split-installed-listener-2026-05-10|/pcv-config\.js|PCV_API_ROUTE_ON_WEB_PORT|Web/API port split'
        }
    }

    It 'does not keep 0.38.1 as a standalone canonical evidence document' {
        $legacyEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0381.md'

        $legacyEvidencePath | Should -Not -Exist
    }

    It 'preserves high-level references to the 0.41.0 full admin account rerun gate' {
        $paths = @(
            'README.md',
            'docs/GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match '0\.41\.0-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260510-154831-0410-account-rerun|routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun|os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun'
            $content | Should -Match 'a3226ef637ea895d2f2a9956599e0d5e79d00410|cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d|AllowUnsignedDev'
            $content | Should -Match 'public trusted signing|외부 stable publication'
        }
    }

    It 'records the 0.39.0 MSI service installed listener rerun as pass evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md'

        $evidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match '0\.39\.0-admin-smoke'
        $evidence | Should -Match 'service-msi-installed-listener-rerun-20260508-212615-0390'
        $evidence | Should -Match '4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee'
        $evidence | Should -Match '8d21654045ed75e81344556fa6444f118c62276a'
        $evidence | Should -Match 'diagnostic_bundle_installed_listener_execution: installed-listener-pass'
        $evidence | Should -Match 'diagnostic_bundle_installed_listener_blocker: none'
        $evidence | Should -Match 'POST.*201'
        $evidence | Should -Match 'GET.*200'
        $evidence | Should -Match 'redaction PASS|redaction은 PASS|redaction_pass|\\[REDACTED\\]'
        $evidence | Should -Match 'firewall.*trust-store.*LAN.*Event Log|Firewall mutation'
        $evidence | Should -Match 'public trusted signing.*외부 stable publication'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'diagnostic_bundle_installed_listener_execution: installed-listener-pass'
        $matrix | Should -Match 'diagnostic_bundle_installed_listener_blocker: none'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match '0\.39\.0-admin-smoke'
            $content | Should -Match 'service-msi-installed-listener-rerun-20260508-212615-0390'
            $content | Should -Match '4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee|8d21654045ed75e81344556fa6444f118c62276a|AllowUnsignedDev'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication'
        }
    }

    It 'records the 0.39.0 installed listener OS mutation gate as pass host mutation evidence' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/os-mutation-gate-installed-listener-rerun-2026-05-08-0390.md'

        $evidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match '0\.39\.0-admin-smoke'
        $evidence | Should -Match 'os-mutation-gate-installed-listener-rerun-20260508-220816-0390'
        $evidence | Should -Match 'os-mutation-gates-installed-listener-rerun-20260508-220816-0390'
        $evidence | Should -Match 'routeparity-service-msi-installed-listener-rerun-20260508-212615-0390'
        $evidence | Should -Match 'host_mutation_performed:\s*true'
        $evidence | Should -Match 'public_trusted_signing:\s*excluded'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'
        $evidence | Should -Match '\[redacted-private-endpoint\]:7777'
        $evidence | Should -Match 'Final firewall rule count:\s*`0`'
        $evidence | Should -Match 'Final Event Log source present:\s*`false`'
        $evidence | Should -Match 'E49CD75AF53CCF7FA73C97E47443096A4507FB7E'
        $evidence | Should -Match '8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6'
        $evidence | Should -Match 'Public trusted signing.*External stable publication|public trusted signing.*외부 stable publication'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'os-mutation-gate-installed-listener-rerun-20260508-220816-0390'
        $matrix | Should -Match 'Public trusted signing.*excluded|public trusted signing is `excluded`'
        $matrix | Should -Match 'external stable publication.*not-claimed|external stable publication is `not-claimed`'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match '0\.39\.0-admin-smoke'
            $content | Should -Match 'os-mutation-gate-installed-listener-rerun-20260508-220816-0390|os-mutation-gates-installed-listener-rerun-20260508-220816-0390'
            $content | Should -Match '192\.168\.1\.17:7777|LAN'
            $content | Should -Match 'public_trusted_signing=excluded|public trusted signing.*excluded|Public trusted signing.*excluded'
            $content | Should -Match 'external_stable_publication=not-claimed|external stable publication.*not-claimed|External stable publication.*not-claimed|외부 stable publication.*not-claimed'
        }
    }

    It 'records the internal MSIX package lifecycle smoke as pass evidence without public claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md'

        $evidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match 'msix-package-lifecycle-smoke-2026-05-10-0416'
        $evidence | Should -Match 'artifacts/msix-package-lifecycle-smoke-20260510-0416'
        $evidence | Should -Match 'host_mutation_performed:\s*true'
        $evidence | Should -Match 'public_trusted_signing:\s*excluded'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'
        $evidence | Should -Match 'PureCVisor\.DesktopNode\.MsixSmoke'
        $evidence | Should -Match 'PureCVisorDesktopNodeMsixSmoke'
        $evidence | Should -Match '0\.41\.5\.0'
        $evidence | Should -Match '0\.41\.6\.0'
        $evidence | Should -Match 'c2efc20e29d950f4e2abd924c13c003cb734bc46e95ccd5aacdd7a724a188674'
        $evidence | Should -Match '8329e0af985185515dac65353398763f5951852faecc928b9925de6fb03dc871'
        $evidence | Should -Match 'final package/service absence|Final smoke package absent'
        $evidence | Should -Match 'Public trusted signing.*외부 stable publication|public trusted signing.*외부 stable publication|Public trusted signing.*external stable publication'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'msix: build-install-update-remove-pass-internal-smoke'
        $matrix | Should -Match 'msix_lifecycle_host_mutation_performed:\s*true'
        $matrix | Should -Match 'msix_lifecycle_public_trusted_signing:\s*excluded'
        $matrix | Should -Match 'msix_lifecycle_external_stable_publication:\s*not-claimed'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'msix-package-lifecycle-smoke-2026-05-10-0416|msix-package-lifecycle-smoke-20260510-0416'
            $content | Should -Match 'PureCVisor\.DesktopNode\.MsixSmoke|0\.41\.5\.0|0\.41\.6\.0|build-install-update-remove-pass-internal-smoke'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication'
        }
    }

    It 'records the 0.39.1 MSI update package apply as pass evidence without public claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md'

        $evidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match 'msi-update-package-apply-2026-05-09-0391'
        $evidence | Should -Match 'artifacts/msi-update-package-20260509-0391'
        $evidence | Should -Match '0\.39\.1-admin-smoke'
        $evidence | Should -Match '9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914'
        $evidence | Should -Match 'd1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5'
        $evidence | Should -Match '8f0c4b6fbac8787932d0e966437fcc62d86e6068'
        $evidence | Should -Match 'host_mutation_performed:\s*true'
        $evidence | Should -Match 'public_trusted_signing:\s*excluded'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'
        $evidence | Should -Match 'MSI exit code.*0'
        $evidence | Should -Match 'Installed manifest.*0\.39\.1-admin-smoke'
        $evidence | Should -Match 'Loopback Web Console.*HTTP.*200'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'internal_msi_update_package_apply:\s*pass-internal-admin-smoke'
        $matrix | Should -Match 'internal_msi_update_package_apply_artifact_root:\s*artifacts/msi-update-package-20260509-0391'
        $matrix | Should -Match 'internal_msi_update_package_apply_public_trusted_signing:\s*excluded'
        $matrix | Should -Match 'internal_msi_update_package_apply_external_stable_publication:\s*not-claimed'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match '0\.39\.1-admin-smoke'
            $content | Should -Match 'msi-update-package-apply-2026-05-09-0391|msi-update-package-20260509-0391'
            $content | Should -Match '9c7e8ddd1ebcb8b03622e7f756c8e5a302391982ae42cb54cf45e823f9e38914|d1cb3a41d4b8ce71ec6ca468a1df525b04e244099a259a857b1ca3b276bbdca5|8f0c4b6fbac8787932d0e966437fcc62d86e6068'
            $content | Should -Match 'public trusted signing|Public trusted signing|외부 stable publication|external stable publication'
        }
    }

    It 'records the public distribution ops execution bundle without public or host mutation claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-distribution-ops-execution-bundle-2026-05-09.md'

        $evidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match 'public-distribution-ops-execution-bundle-2026-05-09'
        $evidence | Should -Match 'artifacts/public-distribution-ops-execution-bundle-20260509-0391'
        $evidence | Should -Match 'New-PcvPublicDistributionOperationsBundle\.ps1'
        $evidence | Should -Match 'public_distribution_ops_execution_bundle:\s*code-level-nonmutating-bundle-pass'
        $evidence | Should -Match 'actual_execution:\s*local-preflight-bundle-executed'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*not-claimed'
        $evidence | Should -Match 'codex/diagnostic-bundle-api-action'
        $evidence | Should -Match 'codex/full-admin-host-mutation-0389-evidence'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'public_distribution_ops_execution_bundle:\s*code-level-nonmutating-bundle-pass'
        $matrix | Should -Match 'public_distribution_ops_execution_bundle_actual_execution:\s*local-preflight-bundle-executed'
        $matrix | Should -Match 'public_distribution_ops_execution_bundle_host_mutation_performed:\s*false'
        $matrix | Should -Match 'public_distribution_ops_execution_bundle_public_trusted_signing:\s*not-claimed'
        $matrix | Should -Match 'public_distribution_ops_execution_bundle_external_stable_publication:\s*not-claimed'
        $matrix | Should -Match 'public_external_blocked_scan:\s*blocked-prerequisite-scan'
        $matrix | Should -Match 'catalog_publication_blocker:\s*missing-upload-endpoint-and-credentials'
        $matrix | Should -Match 'credential_manager_transition:\s*installed-local-system-default-transition-pass'
        $matrix | Should -Match 'credential_manager_mutation:\s*local-system-write-read-delete-and-protected-file-migration'
        $matrix | Should -Match 'service_credential_manager_default_transition:\s*installed-admin-smoke-pass'
        $matrix | Should -Match 'credential_manager_system_context_proof_runner:\s*code-level-native-service-action'
        $matrix | Should -Match 'credential_manager_system_context_proof:\s*installed-local-system-proof-pass'
        $matrix | Should -Match 'credential_manager_token_source_migration:\s*protected-file-to-credential-manager'
        $matrix | Should -Match 'credential_manager_old_source_rejection_status:\s*protected-file-source-rejected-after-reload'
        $matrix | Should -Match 'credential_manager_rollback_diagnostics_status:\s*written'
        $matrix | Should -Match 'event_log_provider_transition:\s*installed-provider-register-write-pass'
        $matrix | Should -Match 'event_log_provider_mutation:\s*registered'
        $matrix | Should -Match 'event_log_write_status:\s*write-query-pass'
        $matrix | Should -Match 'burn_bootstrapper_bundle_step:\s*build-install-repair-remove-pass-internal-smoke'
        $matrix | Should -Match 'burn_bootstrapper:\s*build-install-repair-remove-pass-internal-smoke'
        $matrix | Should -Match 'burn_bootstrapper_public_trusted_signing:\s*not-claimed'
        $matrix | Should -Match 'burn_bootstrapper_external_stable_publication:\s*not-claimed'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $ledger | Should -Match 'public-distribution-ops-execution-bundle-2026-05-09'
        $ledger | Should -Match 'artifacts/public-distribution-ops-execution-bundle-20260509-0391'
        $ledger | Should -Match 'host_mutation_performed=false'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'packaging/windows-desktop-node/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'public-distribution-ops-execution-bundle-2026-05-09|public-distribution-ops-execution-bundle-20260509-0391|New-PcvPublicDistributionOperationsBundle'
            $content | Should -Match 'public trusted signing|external stable publication|외부 stable publication|host_mutation_performed=false'
        }
    }

    It 'records public ops actual follow-up evidence and external blockers without public claims' {
        $burnEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md'
        $credentialEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/windows-credential-manager-transition-2026-05-09-0391.md'
        $eventLogEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/windows-event-log-provider-default-transition-2026-05-09-0391.md'
        $eventLogDefaultInstalledPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md'
        $blockedEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-external-gates-blocked-2026-05-09-0391.md'

        $burnEvidencePath | Should -Exist
        $credentialEvidencePath | Should -Exist
        $eventLogEvidencePath | Should -Exist
        $eventLogDefaultInstalledPath | Should -Exist
        $blockedEvidencePath | Should -Exist

        $burnEvidence = Get-Content -Raw -LiteralPath $burnEvidencePath
        $burnEvidence | Should -Match 'burn-bootstrapper-lifecycle-smoke-2026-05-10-0416'
        $burnEvidence | Should -Match 'artifacts/burn-bootstrapper-lifecycle-20260510-0416'
        $burnEvidence | Should -Match 'burn_bootstrapper:\s*build-install-repair-remove-pass-internal-smoke'
        $burnEvidence | Should -Match 'host_mutation_performed:\s*true'
        $burnEvidence | Should -Match '5e67bd3a1fed7262447531000328825180fd678b252170793cf88e50fc41535d'
        $burnEvidence | Should -Match 'final service `Running`|final service `Running`'
        $burnEvidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $burnEvidence | Should -Match 'external_stable_publication:\s*not-claimed'

        $credentialEvidence = Get-Content -Raw -LiteralPath $credentialEvidencePath
        $credentialEvidence | Should -Match 'windows-credential-manager-transition-2026-05-09-0391'
        $credentialEvidence | Should -Match 'artifacts/windows-credential-manager-transition-20260509-0391'
        $credentialEvidence | Should -Match 'credential_manager_transition:\s*capability-pass-service-transition-blocked'
        $credentialEvidence | Should -Match 'credential_manager_mutation:\s*current-user-smoke-write-read-delete'
        $credentialEvidence | Should -Match 'service_credential_manager_default_transition:\s*blocked-by-service-account-context'
        $credentialEvidence | Should -Match 'LocalSystem'
        $credentialEvidence | Should -Match 'token_value_observed:\s*false'

        $eventLogEvidence = Get-Content -Raw -LiteralPath $eventLogEvidencePath
        $eventLogEvidence | Should -Match 'windows-event-log-provider-default-transition-2026-05-09-0391'
        $eventLogEvidence | Should -Match 'artifacts/windows-event-log-provider-default-transition-20260509-0391'
        $eventLogEvidence | Should -Match 'event_log_provider_transition:\s*installed-provider-register-write-pass'
        $eventLogEvidence | Should -Match 'event_log_provider_mutation:\s*registered'
        $eventLogEvidence | Should -Match 'event_log_write_status:\s*write-query-pass'
        $eventLogEvidence | Should -Match 'PureCVisor Desktop Node'
        $eventLogEvidence | Should -Match '39100'

        $eventLogDefaultInstalled = Get-Content -Raw -LiteralPath $eventLogDefaultInstalledPath
        $eventLogDefaultInstalled | Should -Match 'windows-event-log-default-transition-installed-2026-05-10-0396'
        $eventLogDefaultInstalled | Should -Match 'artifacts/windows-event-log-default-transition-installed-20260510-0396'
        $eventLogDefaultInstalled | Should -Match 'event_log_default_transition:\s*installed-admin-smoke-pass'
        $eventLogDefaultInstalled | Should -Match 'event_log_hardening:\s*installed-default-writer-repair-remove-volume-schema-pass'
        $eventLogDefaultInstalled | Should -Match 'event_log_default_writer:\s*installed-admin-smoke-pass'
        $eventLogDefaultInstalled | Should -Match 'event_log_schema_version:\s*1'
        $eventLogDefaultInstalled | Should -Match '39101'
        $eventLogDefaultInstalled | Should -Match 'public_trusted_signing:\s*not-claimed'
        $eventLogDefaultInstalled | Should -Match 'external_stable_publication:\s*not-claimed'

        $blockedEvidence = Get-Content -Raw -LiteralPath $blockedEvidencePath
        $blockedEvidence | Should -Match 'public-external-gates-blocked-2026-05-09-0391'
        $blockedEvidence | Should -Match 'artifacts/public-external-gates-blocked-20260509-0391'
        $blockedEvidence | Should -Match 'timestamp_evidence:\s*blocked-by-missing-public-signing-cert-and-timestamp-url'
        $blockedEvidence | Should -Match 'external_stable_publication:\s*blocked-by-missing-upload-endpoint-and-credentials'
        $blockedEvidence | Should -Match 'winget_submission:\s*blocked-by-no-public-signed-stable-installer-and-public-url'
        $blockedEvidence | Should -Match 'clean_host_public_signed_install_update_rollback_smoke:\s*blocked-by-public-signing-publication-and-clean-host'
        $blockedEvidence | Should -Match 'host_mutation_performed:\s*false'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'public_external_blocked_scan:\s*blocked-prerequisite-scan'
        $matrix | Should -Match 'timestamp_evidence:\s*blocked-by-missing-public-signing-cert-and-timestamp-url'
        $matrix | Should -Match 'burn_bootstrapper:\s*build-install-repair-remove-pass-internal-smoke'
        $matrix | Should -Match 'burn_bootstrapper_lifecycle_artifact_root:\s*artifacts/burn-bootstrapper-lifecycle-20260510-0416'
        $matrix | Should -Match 'credential_manager_transition:\s*installed-local-system-default-transition-pass'
        $matrix | Should -Match 'event_log_provider_transition:\s*installed-provider-register-write-pass'
        $matrix | Should -Match 'event_log_default_transition:\s*installed-admin-smoke-pass'
        $matrix | Should -Match 'event_log_hardening:\s*installed-default-writer-repair-remove-volume-schema-pass'
        $matrix | Should -Match 'catalog_publication_blocker:\s*missing-upload-endpoint-and-credentials'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $ledger | Should -Match 'burn-bootstrapper-lifecycle-smoke-2026-05-10-0416'
        $ledger | Should -Match 'windows-credential-manager-transition-2026-05-09-0391'
        $ledger | Should -Match 'windows-event-log-provider-default-transition-2026-05-09-0391'
        $ledger | Should -Match 'windows-event-log-default-transition-installed-2026-05-10-0396'
        $ledger | Should -Match 'public-external-gates-blocked-2026-05-09-0391'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md',
            'packaging/windows-desktop-node/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'burn-bootstrapper-lifecycle-smoke-2026-05-10-0416|burn-bootstrapper-lifecycle-20260510-0416|build-install-repair-remove-pass-internal-smoke'
            $content | Should -Match 'public-external-gates-blocked-2026-05-09-0391|public-external-gates-blocked-20260509-0391|timestamp_evidence'
            $content | Should -Match 'windows-credential-manager-transition-2026-05-09-0391|windows-credential-manager-default-transition-installed-2026-05-10-0395|capability-pass-service-transition-blocked|installed-local-system-default-transition-pass'
            $content | Should -Match 'windows-event-log-provider-default-transition-2026-05-09-0391|windows-event-log-default-transition-installed-2026-05-10-0396|installed-provider-register-write-pass|installed-default-writer-repair-remove-volume-schema-pass'
            $content | Should -Match 'public trusted signing|external stable publication|외부 stable publication'
        }
    }

    It 'records the final seven public ops follow-up attempt without public release claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-ops-final-followup-attempt-2026-05-09-0391.md'

        $evidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match 'public-ops-final-followup-attempt-2026-05-09-0391'
        $evidence | Should -Match 'artifacts/public-ops-final-followup-attempt-20260509-0391'
        $evidence | Should -Match 'local-final-followup-prerequisite-scan-executed'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'remaining_follow_up_count:\s*7'
        $evidence | Should -Match '1-public-trusted-signing-timestamp'
        $evidence | Should -Match '2-external-stable-publication-catalog-upload'
        $evidence | Should -Match '3-winget-submission'
        $evidence | Should -Match '4-clean-host-public-signed-install-update-rollback'
        $evidence | Should -Match '5-windows-credential-manager-service-default-transition'
        $evidence | Should -Match '6-built-in-tls-certificate-lifecycle'
        $evidence | Should -Match '7-windows-event-log-provider-hardening'
        $evidence | Should -Match 'public_release:\s*not-claimed'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'public_ops_final_followup_attempt:\s*local-final-followup-prerequisite-scan'
        $matrix | Should -Match 'public_ops_final_followup_attempt_artifact_root:\s*artifacts/public-ops-final-followup-attempt-20260509-0391'
        $matrix | Should -Match 'remaining_public_ops_follow_up_count:\s*7'
        $matrix | Should -Match 'event_log_hardening:\s*installed-default-writer-repair-remove-volume-schema-pass'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $ledger | Should -Match 'public-ops-final-followup-attempt-2026-05-09-0391'
        $ledger | Should -Match '1-7 final public operations follow-up prerequisite scan'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md',
            'packaging/windows-desktop-node/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'public-ops-final-followup-attempt-2026-05-09-0391|public-ops-final-followup-attempt-20260509-0391|New-PcvPublicOpsFinalFollowupAttempt'
            $content | Should -Match 'remaining_follow_up_count: 7|remaining public ops follow-up count `7`|1-7'
            $content | Should -Match 'public trusted signing|external stable publication|외부 stable publication'
        }
    }

    It 'records public ops gate execution readiness and TLS code-level closure without public release claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-ops-gate-execution-readiness-2026-05-09-0392.md'

        $evidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath

        $evidence | Should -Match 'public-ops-gate-execution-readiness-2026-05-09-0392'
        $evidence | Should -Match 'artifacts/public-ops-gate-execution-readiness-20260509-0392'
        $evidence | Should -Match 'local-execution-readiness-descriptor-written'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'public_release:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*blocked-by-missing-upload-endpoint-and-credential'
        $evidence | Should -Match 'winget_submission:\s*blocked-by-missing-public-installer-url-or-submission-token'
        $evidence | Should -Match 'clean_host_public_signed_install_update_rollback_smoke:\s*blocked-by-missing-clean-host-runner-or-public-publication'
        $evidence | Should -Match 'credential_manager_system_context_proof:\s*blocked-by-missing-system-context-proof'
        $evidence | Should -Match 'tls_certificate_lifecycle:\s*partial-code-level-cert-generate-rotate-delete-pass'
        $evidence | Should -Match 'tls_private_key_material_written:\s*false'
        $evidence | Should -Match 'event_log_hardening:\s*provider-pass-default-writer-repair-remove-volume-guard-pending'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'public_ops_gate_execution_readiness:\s*partial-code-level-readiness-with-external-blockers'
        $matrix | Should -Match 'public_ops_gate_execution_readiness_artifact_root:\s*artifacts/public-ops-gate-execution-readiness-20260509-0392'
        $matrix | Should -Match 'tls_certificate_lifecycle:\s*partial-code-level-cert-generate-rotate-delete-pass'
        $matrix | Should -Match 'tls_binding:\s*not-run'
        $matrix | Should -Match 'event_log_hardening:\s*installed-default-writer-repair-remove-volume-schema-pass'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $ledger | Should -Match 'public-ops-gate-execution-readiness-2026-05-09-0392'
        $ledger | Should -Match 'six remaining public operations gate execution-readiness descriptor'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md',
            'packaging/windows-desktop-node/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'public-ops-gate-execution-readiness-2026-05-09-0392|public-ops-gate-execution-readiness-20260509-0392|New-PcvPublicOpsGateExecutionReadiness'
            $content | Should -Match 'partial-code-level-cert-generate-rotate-delete-pass'
            $content | Should -Match 'public trusted signing|external stable publication|외부 stable publication'
        }
    }

    It 'records public ops installed hardening code-level service actions without public release claims' {
        $evidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-ops-installed-hardening-code-level-2026-05-09-0393.md'
        $defaultTransitionEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395.md'
        $eventLogDefaultTransitionEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396.md'

        $evidencePath | Should -Exist
        $defaultTransitionEvidencePath | Should -Exist
        $eventLogDefaultTransitionEvidencePath | Should -Exist
        $evidence = Get-Content -Raw -LiteralPath $evidencePath
        $defaultTransitionEvidence = Get-Content -Raw -LiteralPath $defaultTransitionEvidencePath
        $eventLogDefaultTransitionEvidence = Get-Content -Raw -LiteralPath $eventLogDefaultTransitionEvidencePath

        $evidence | Should -Match 'public-ops-installed-hardening-code-level-2026-05-09-0393'
        $evidence | Should -Match 'credential-manager-system-proof'
        $evidence | Should -Match 'eventlog-repair'
        $evidence | Should -Match 'eventlog-write-test'
        $evidence | Should -Match 'eventlog-volume-guard'
        $evidence | Should -Match 'host_mutation_performed:\s*false'
        $evidence | Should -Match 'public_trusted_signing:\s*not-claimed'
        $evidence | Should -Match 'external_stable_publication:\s*blocked-by-missing-upload-endpoint-and-credential'
        $evidence | Should -Match 'credential_manager_system_context_proof_runner:\s*code-level-native-service-action'
        $evidence | Should -Match 'service_credential_manager_default_transition:\s*system-proof-runner-code-level-applied-service-default-transition-pending'
        $evidence | Should -Match 'event_log_hardening:\s*partial-code-level-repair-write-volume-guard-default-writer-pending'
        $evidence | Should -Match 'tls_binding:\s*not-run'
        $defaultTransitionEvidence | Should -Match 'windows-credential-manager-default-transition-installed-2026-05-10-0395'
        $defaultTransitionEvidence | Should -Match 'artifacts/windows-credential-manager-default-transition-installed-20260510-0395'
        $defaultTransitionEvidence | Should -Match 'installed-msi-local-system-custom-action'
        $defaultTransitionEvidence | Should -Match 'NT AUTHORITY\\SYSTEM'
        $defaultTransitionEvidence | Should -Match 'credential_manager_system_context_proof:\s*installed-local-system-proof-pass'
        $defaultTransitionEvidence | Should -Match 'service_credential_manager_default_transition:\s*installed-admin-smoke-pass'
        $defaultTransitionEvidence | Should -Match 'token_source_migration:\s*protected-file-to-credential-manager'
        $defaultTransitionEvidence | Should -Match 'old_source_rejection_status:\s*protected-file-source-rejected-after-reload'
        $defaultTransitionEvidence | Should -Match 'rollback_diagnostics_status:\s*written'
        $defaultTransitionEvidence | Should -Match 'token_value_observed:\s*false'
        $defaultTransitionEvidence | Should -Match 'public_trusted_signing:\s*not-claimed'

        $eventLogDefaultTransitionEvidence | Should -Match 'windows-event-log-default-transition-installed-2026-05-10-0396'
        $eventLogDefaultTransitionEvidence | Should -Match 'installed-msi-local-system-custom-action'
        $eventLogDefaultTransitionEvidence | Should -Match 'event_log_default_transition:\s*installed-admin-smoke-pass'
        $eventLogDefaultTransitionEvidence | Should -Match 'event_log_hardening:\s*installed-default-writer-repair-remove-volume-schema-pass'
        $eventLogDefaultTransitionEvidence | Should -Match 'event_log_default_writer:\s*installed-admin-smoke-pass'
        $eventLogDefaultTransitionEvidence | Should -Match 'event_log_schema_version:\s*1'
        $eventLogDefaultTransitionEvidence | Should -Match 'public_trusted_signing:\s*not-claimed'

        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md'
        $matrix | Should -Match 'public_ops_installed_hardening_code_level:\s*partial-code-level-credential-proof-runner-eventlog-hardening'
        $matrix | Should -Match 'credential_manager_system_context_proof_runner:\s*code-level-native-service-action'
        $matrix | Should -Match 'credential_manager_default_transition_evidence:\s*docs/ga-ready/evidence/windows-credential-manager-default-transition-installed-2026-05-10-0395\.md'
        $matrix | Should -Match 'credential_manager_transition:\s*installed-local-system-default-transition-pass'
        $matrix | Should -Match 'service_credential_manager_default_transition:\s*installed-admin-smoke-pass'
        $matrix | Should -Match 'credential_manager_system_context_proof:\s*installed-local-system-proof-pass'
        $matrix | Should -Match 'credential_manager_service_reload_status:\s*restarted'
        $matrix | Should -Match 'credential_manager_old_source_rejection_status:\s*protected-file-source-rejected-after-reload'
        $matrix | Should -Match 'credential_manager_rollback_diagnostics_status:\s*written'
        $matrix | Should -Match 'event_log_default_transition_evidence:\s*docs/ga-ready/evidence/windows-event-log-default-transition-installed-2026-05-10-0396\.md'
        $matrix | Should -Match 'event_log_default_transition:\s*installed-admin-smoke-pass'
        $matrix | Should -Match 'event_log_hardening:\s*installed-default-writer-repair-remove-volume-schema-pass'
        $matrix | Should -Match 'event_log_repair_status:\s*installed-provider-repair-pass'
        $matrix | Should -Match 'event_log_volume_guard_status:\s*installed-volume-guard-pass'
        $matrix | Should -Match 'event_log_remove_repair_status:\s*installed-provider-remove-restore-pass'
        $matrix | Should -Match 'event_log_default_writer:\s*installed-admin-smoke-pass'
        $matrix | Should -Match 'event_log_schema_version:\s*1'

        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $ledger | Should -Match 'windows-credential-manager-default-transition-installed-2026-05-10-0395'
        $ledger | Should -Match 'windows-event-log-default-transition-installed-2026-05-10-0396'
        $ledger | Should -Match 'public-ops-installed-hardening-code-level-2026-05-09-0393'
        $ledger | Should -Match 'Credential Manager SYSTEM proof runner and Event Log provider hardening service actions'

        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/GUIDE.md',
            'docs/OPERATIONS_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md',
            'packaging/windows-desktop-node/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match 'public-ops-installed-hardening-code-level-2026-05-09-0393|credential-manager-system-proof|eventlog-volume-guard|windows-credential-manager-default-transition-installed-2026-05-10-0395|windows-event-log-default-transition-installed-2026-05-10-0396'
            $content | Should -Match 'system-proof-runner-code-level-applied|installed-local-system-proof-pass|installed-admin-smoke-pass|installed-default-writer-repair-remove-volume-schema-pass|installed default writer|partial-code-level-repair-write-volume-guard'
            $content | Should -Match 'public trusted signing|external stable publication|외부 stable publication'
        }
    }

    It 'publishes an operations guide for installed service runbooks and public boundary guardrails' {
        $guidePath = Join-Path $script:RepoRoot 'docs/OPERATIONS_GUIDE.md'

        $guidePath | Should -Exist
        $guide = Get-Content -Raw -LiteralPath $guidePath

        $guide | Should -Match 'PureCVisor Desktop Node 운영 가이드'
        $guide | Should -Match 'PureCVisorDesktopNode'
        $guide | Should -Match '0\.39\.0-admin-smoke'
        $guide | Should -Match 'service-msi-installed-listener-rerun-20260508-212615-0390'
        $guide | Should -Match 'os-mutation-gate-installed-listener-rerun-20260508-220816-0390'
        $guide | Should -Match 'Diagnostic bundle'
        $guide | Should -Match 'Update와 rollback'
        $guide | Should -Match 'Public trusted signing.*not-claimed|Public trusted signing.*excluded|public trusted signing.*외부 stable publication'
        $guide | Should -Match '절대 하지 말 것'

        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $developerIndex | Should -Match 'docs/OPERATIONS_GUIDE\.md'

        $topGuide = Get-RepoText -RelativePath 'docs/GUIDE.md'
        $topGuide | Should -Match '운영 가이드: `docs/OPERATIONS_GUIDE\.md`'

        $readme = Get-RepoText -RelativePath 'README.md'
        $readme | Should -Match 'docs/OPERATIONS_GUIDE\.md'

        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $agents | Should -Match '운영 가이드: `docs/OPERATIONS_GUIDE\.md`'
    }

    It 'records the 0.38.8 elevated update rollback smoke as installed destructive pass evidence' {
        $paths = @(
            'README.md',
            'AGENTS.md',
            'docs/ADR_INDEX.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match '0\.38\.8-admin-smoke'
            $content | Should -Match 'product-update-rollback-mutation-20260507-0388-elevated-pass'
            $content | Should -Match '163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564'
            $content | Should -Match '0\.38\.6-admin-smoke'
            $content | Should -Match 'DesktopNode\.failed'
            $content | Should -Match 'succeeded/health'
            $content | Should -Match 'public trusted signing|외부 stable publication|external stable publication'
        }

        $evidence = Get-RepoText -RelativePath 'docs/ga-ready/evidence/product-update-rollback-mutation-2026-05-07-0388.md'
        $evidence | Should -Match 'Elevated Update PASS'
        $evidence | Should -Match 'Elevated Rollback PASS'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public trusted signing 또는 외부 stable publication evidence가 아니다'
    }

    It 'records 0.38.7 as the latest internal signed build instead of 0.38.4' {
        $paths = @(
            'AGENTS.md',
            'docs/ADR_INDEX.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Match '0\.38\.7-rc\.1'
            $content | Should -Match 'internal-enterprise-requiresigned-rc-msi-20260507-0387'
            $content | Should -Match 'c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602'
            $content | Should -Match 'dd4e7379c515b05eb82038404519c9e63f54bf51'
            $content | Should -Not -Match '0\.38\.4-rc\.1.*최신 internal signed build'
            $content | Should -Not -Match '0\.38\.4-rc\.1.*최신 internal enterprise'
            $content | Should -Not -Match 'Latest internal signed build.*0\.38\.4-rc\.1'
        }
    }

    It 'does not leave stale latest OS gate wording behind' {
        $paths = @(
            'AGENTS.md',
            'README.md',
            'docs/GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md'
        )

        foreach ($path in $paths) {
            $content = Get-RepoText -RelativePath $path
            $content | Should -Not -Match '최신 OS gate.*0\.35\.7-admin-smoke'
            $content | Should -Not -Match 'latest OS gate.*0\.35\.7-admin-smoke'
            $content | Should -Not -Match '최신 OS mutation gate.*0\.35\.7-admin-smoke'
            $content | Should -Not -Match '향후 별도 승인된 OS gate rerun이 성공하기 전까지 최신 OS gate evidence는 `0\.35\.7-admin-smoke`'
            $content | Should -Not -Match '최신[^.\r\n]*0\.37\.0-admin-smoke'
            $content | Should -Not -Match 'latest[^.\r\n]*0\.37\.0-admin-smoke'
            $content | Should -Not -Match '최신[^.\r\n]*0\.38\.2-admin-smoke'
            $content | Should -Not -Match 'latest[^.\r\n]*0\.38\.2-admin-smoke'
            $content | Should -Not -Match '최신[^.\r\n]*0\.38\.4-admin-smoke'
            $content | Should -Not -Match 'latest[^.\r\n]*0\.38\.4-admin-smoke'
            $content | Should -Not -Match '최신 (?:full admin host mutation|full gate|host mutation)[^`\r\n]*`0\.42\.8-admin-smoke`'
            $content | Should -Not -Match 'latest (?:full admin host mutation|full gate|host mutation)[^`\r\n]*`0\.42\.8-admin-smoke`'
            $content | Should -Not -Match '0\.42\.8-admin-smoke[^.\r\n]*(?:최신|latest) (?:full admin host mutation|full gate|host mutation)'
            $content | Should -Not -Match 'current claim(?:은| is)? 0428 evidence'
        }
    }

    It 'records post 0.42.55 follow-up triage and installed account noVNC rerun without opening a package pair' {
        $followupPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/post-04255-followup-execution-2026-05-28.md'
        $accountNoVncPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04255-followup.md'

        $followupPath | Should -Exist
        $accountNoVncPath | Should -Exist

        $followup = Get-Content -Raw -LiteralPath $followupPath
        $accountNoVnc = Get-Content -Raw -LiteralPath $accountNoVncPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'

        $followup | Should -Match 'followup_sequence:\s*`1-2-3-4-5-6`'
        $followup | Should -Match 'manual_admin_rebaseline_artifact_root:\s*`artifacts/manual-admin-campaign-20260528-04255-next/rebaseline-readiness`'
        $followup | Should -Match 'installed_version:\s*`0\.42\.55-admin-smoke`'
        $followup | Should -Match 'package_pair_decision:\s*`not-opened-no-next-product-payload-target`'
        $followup | Should -Match 'next_package_pair_candidate:\s*`0\.42\.55-admin-smoke -> next-admin-smoke-required`'
        $followup | Should -Match 'guest_execution_contract_status:\s*`carried-forward-0\.42\.55-provider-running-cancel`'
        $followup | Should -Match 'hyperv_qos_mutation_status:\s*`carried-forward-0\.42\.48-web-tui-direct-control`'
        $followup | Should -Match 'account_novnc_followup_evidence:\s*`docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04255-followup\.md`'

        $accountNoVnc | Should -Match 'installed_account_login_smoke_artifact_root:\s*`artifacts/installed-account-login-smoke-20260528-04255-followup`'
        $accountNoVnc | Should -Match 'target_backed_novnc_artifact_root:\s*`artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04255-followup`'
        $accountNoVnc | Should -Match 'token/password/refresh-token observed:\s*`false/false/false`'
        $accountNoVnc | Should -Match 'host_mutation_performed:\s*`true-service-config-temporary-restored`'
        $accountNoVnc | Should -Match 'public trusted signing 또는 외부 stable publication evidence가 아니다'

        foreach ($content in @($ledger, $evidenceIndex, $descriptor, $matrix)) {
            $content | Should -Match 'post-04255-followup-execution-2026-05-28'
            $content | Should -Match 'installed-account-novnc-operator-surface-smoke-2026-05-28-04255-followup'
            $content | Should -Match '0\.42\.55-admin-smoke -> next-admin-smoke-required'
            $content | Should -Match 'not-opened-no-next-product-payload-target'
        }
    }

    It 'records 0.42.56 package fullgate manual-admin and installed operator surface closure' {
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04256.md'
        $fullgatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04256-hostmutation.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04255-04256.md'
        $currentCardPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256.md'
        $accountNoVncPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04256.md'

        foreach ($path in @($packagePath, $fullgatePath, $manualCampaignPath, $currentCardPath, $accountNoVncPath)) {
            $path | Should -Exist
        }

        $package = Get-Content -Raw -LiteralPath $packagePath
        $fullgate = Get-Content -Raw -LiteralPath $fullgatePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $currentCard = Get-Content -Raw -LiteralPath $currentCardPath
        $accountNoVnc = Get-Content -Raw -LiteralPath $accountNoVncPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'

        $package | Should -Match 'version:\s*`0\.42\.56-admin-smoke`'
        $package | Should -Match 'msi_sha256:\s*`25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`'
        $package | Should -Match 'payload_aggregate_sha256:\s*`5670772a193c996fadc0dbe1a9e45ec0ab908bd124092d1a328c22b5e0c7e699`'
        $package | Should -Match 'provenance_commit:\s*`5594adc55b013a2bf3ade9c6ae7171ca37bdbeb0`'

        $fullgate | Should -Match 'batch_id:\s*`full-admin-host-mutation-gate-20260528-04256`'
        $fullgate | Should -Match 'full_gate_msi_sha256:\s*`085792312b3bba3ba241882156212b40f936748b08a0ad56ae4a877b24759dec`'
        $fullgate | Should -Match 'host_mutation_performed:\s*`true`'

        $manualCampaign | Should -Match 'baseline_version:\s*`0\.42\.55-admin-smoke`'
        $manualCampaign | Should -Match 'target_version:\s*`0\.42\.56-admin-smoke`'
        $manualCampaign | Should -Match 'descriptor_batch_id:\s*`manual-admin-campaign-descriptor-20260528-04255-04256-closed`'
        $manualCampaign | Should -Match 'missing_count:\s*`0`'
        $manualCampaign | Should -Match 'not_pass_count:\s*`0`'
        $manualCampaign | Should -Match 'update_zip_sha256:\s*`073a3d3d0a1e6ce6d4e09d2b66154ed957b42fe2bba6e30e4b101a9beac85a24`'
        $manualCampaign | Should -Match 'burn_bundle_sha256:\s*`f10204ab9e17a300c97b4e7e81e22a53ba5ca3db252a1bf7aff9b1bc48db729e`'
        $manualCampaign | Should -Match 'msix_v2_sha256:\s*`44db00ac736568b0de185711e099c2b109afddb4de97b2fcb6a5f163050c1e08`'

        $currentCard | Should -Match 'artifact_summary:\s*`artifacts/installed-operator-surface-current-card-20260528-04256/summary\.json`'
        $currentCard | Should -Match 'Manual admin next'
        $currentCard | Should -Match 'current\.manual_admin_next_package_pair'
        $currentCard | Should -Match 'MANUAL ADMIN NEXT'

        $accountNoVnc | Should -Match 'installed_account_login_smoke_artifact_root:\s*`artifacts/installed-account-login-smoke-20260528-04256`'
        $accountNoVnc | Should -Match 'target_backed_novnc_artifact_root:\s*`artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04256`'
        $accountNoVnc | Should -Match 'token/password/refresh-token observed:\s*`false/false/false`'

        foreach ($content in @($ledger, $evidenceIndex, $descriptor, $matrix)) {
            $content | Should -Match '0\.42\.55-admin-smoke -> 0\.42\.56-admin-smoke'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260528-04255-04256-closed'
            $content | Should -Match 'installed-operator-surface-current-card-2026-05-28-04256'
            $content | Should -Match 'installed-account-novnc-operator-surface-smoke-2026-05-28-04256'
        }
    }

    It 'records 0.42.57 package fullgate manual-admin and public-boundary current-card closure' {
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-28-04257.md'
        $fullgatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-28-04257-hostmutation.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-28-04256-04257.md'
        $currentCardPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04257.md'
        $accountNoVncPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-28-04257.md'

        foreach ($path in @($packagePath, $fullgatePath, $manualCampaignPath, $currentCardPath, $accountNoVncPath)) {
            $path | Should -Exist
        }

        $package = Get-Content -Raw -LiteralPath $packagePath
        $fullgate = Get-Content -Raw -LiteralPath $fullgatePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $currentCard = Get-Content -Raw -LiteralPath $currentCardPath
        $accountNoVnc = Get-Content -Raw -LiteralPath $accountNoVncPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'

        $package | Should -Match 'version:\s*`0\.42\.57-admin-smoke`'
        $package | Should -Match 'msi_sha256:\s*`2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`'
        $package | Should -Match 'payload_aggregate_sha256:\s*`c24512aec2dae7e73da4af24778451b3b3dfdc52d2c7914db61ceaaefae67e07`'
        $package | Should -Match 'provenance_commit:\s*`16cc0d6b592d7f2f9ead14c41d8f4ad0e1f28b76`'

        $fullgate | Should -Match 'batch_id:\s*`full-admin-host-mutation-gate-20260528-04257`'
        $fullgate | Should -Match 'full_gate_msi_sha256:\s*`809eacb97a49aeaa32fc0ea3dce8ac5bdeb7c66b8b4502352519a338a512847e`'
        $fullgate | Should -Match 'host_mutation_performed:\s*`true`'

        $manualCampaign | Should -Match 'baseline_version:\s*`0\.42\.56-admin-smoke`'
        $manualCampaign | Should -Match 'target_version:\s*`0\.42\.57-admin-smoke`'
        $manualCampaign | Should -Match 'descriptor_batch_id:\s*`manual-admin-campaign-descriptor-20260528-04256-04257-closed`'
        $manualCampaign | Should -Match 'update_zip_sha256:\s*`c50e846e51a568a184cd706dc71506cdad95d8248c4e89713f2f52b690236946`'
        $manualCampaign | Should -Match 'burn_bundle_sha256:\s*`a6d6f6d2378e57feafb6ca346464c08258a8822120458204f51570a2a96d0d04`'
        $manualCampaign | Should -Match 'msix_v2_sha256:\s*`c6345a59f533af24abcdce33deab0e6d0f43f6da33accab72baa1ac44e36fa3b`'

        $currentCard | Should -Match 'artifact_summary:\s*`artifacts/installed-operator-surface-current-card-20260528-04257/summary\.json`'
        $currentCard | Should -Match 'current\.public_boundary_main_push'
        $currentCard | Should -Match 'PUBLIC BOUNDARY CURRENT'
        $currentCard | Should -Match 'Public boundary head'

        $accountNoVnc | Should -Match 'installed_account_login_smoke_artifact_root:\s*`artifacts/installed-account-login-smoke-20260528-04257`'
        $accountNoVnc | Should -Match 'target_backed_novnc_artifact_root:\s*`artifacts/target-backed-novnc-installed-streaming-smoke-20260528-04257`'
        $accountNoVnc | Should -Match 'token/password/refresh-token observed:\s*`false/false/false`'

        foreach ($content in @($ledger, $evidenceIndex, $descriptor, $matrix)) {
            $content | Should -Match '0\.42\.56-admin-smoke -> 0\.42\.57-admin-smoke'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260528-04256-04257-closed'
            $content | Should -Match 'installed-operator-surface-current-card-2026-05-28-04257'
            $content | Should -Match 'installed-account-novnc-operator-surface-smoke-2026-05-28-04257'
        }
    }

    It 'records 0.42.58 package fullgate manual-admin and operator surface closure' {
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04257-main-push-pass.md'
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04258.md'
        $fullgatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04258-hostmutation.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04257-04258.md'
        $currentCardPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md'
        $accountNoVncPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-29-04258.md'

        foreach ($path in @($publicBoundaryPath, $packagePath, $fullgatePath, $manualCampaignPath, $currentCardPath, $accountNoVncPath)) {
            $path | Should -Exist
        }

        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $package = Get-Content -Raw -LiteralPath $packagePath
        $fullgate = Get-Content -Raw -LiteralPath $fullgatePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $currentCard = Get-Content -Raw -LiteralPath $currentCardPath
        $accountNoVnc = Get-Content -Raw -LiteralPath $accountNoVncPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'

        $publicBoundary | Should -Match 'latest_main_push_run_id:\s*`26587524245`'
        $publicBoundary | Should -Match 'latest_main_push_job_id:\s*`78337437665`'
        $publicBoundary | Should -Match 'latest_main_push_head_sha:\s*`96182b440b35c17183802ad323a123ff6e4b6730`'

        $package | Should -Match 'version:\s*`0\.42\.58-admin-smoke`'
        $package | Should -Match 'msi_sha256:\s*`6ae889eeb1b7134fab9618941748528f6260727abbc8ff36eee301b59dff6c0b`'
        $package | Should -Match 'payload_aggregate_sha256:\s*`9e162bc59527d107c0c6e35105bd5a0f17c7449a94e23cfe138cdc268f3d7184`'
        $package | Should -Match 'provenance_commit:\s*`96182b440b35c17183802ad323a123ff6e4b6730`'

        $fullgate | Should -Match 'batch_id:\s*`full-admin-host-mutation-gate-20260529-04258`'
        $fullgate | Should -Match 'full_gate_msi_sha256:\s*`7e0aef503b3f56eb116d5931c9560a3dcd2c4ba347f1eb24e4b505b28e6c2845`'
        $fullgate | Should -Match 'host_mutation_performed:\s*`true`'

        $manualCampaign | Should -Match 'baseline_version:\s*`0\.42\.57-admin-smoke`'
        $manualCampaign | Should -Match 'target_version:\s*`0\.42\.58-admin-smoke`'
        $manualCampaign | Should -Match 'descriptor_batch_id:\s*`manual-admin-campaign-descriptor-20260529-04257-04258-closed`'
        $manualCampaign | Should -Match 'update_zip_sha256:\s*`941190ac595db165c0ab7bc9d8c75c140208ae492780a8684dad19463913b16f`'
        $manualCampaign | Should -Match 'burn_bundle_sha256:\s*`97cc6292db711e6964a5a2e2fcea56620edd722c538510a672b840040f0eabc7`'
        $manualCampaign | Should -Match 'msix_v2_sha256:\s*`c65decc2f98aa4fcc37494ea116c7a41d021210874cf1057053f18f9a4f9e90e`'

        $currentCard | Should -Match 'artifact_summary:\s*`artifacts/installed-operator-surface-current-card-20260529-04258/summary\.json`'
        $currentCard | Should -Match 'full-admin-host-mutation-gate-20260529-04258'
        $currentCard | Should -Match 'manual-admin-campaign-descriptor-20260529-04257-04258-closed'

        $accountNoVnc | Should -Match 'installed_account_login_smoke_artifact_root:\s*`artifacts/installed-account-login-smoke-20260529-04258`'
        $accountNoVnc | Should -Match 'target_backed_novnc_artifact_root:\s*`artifacts/target-backed-novnc-installed-streaming-smoke-20260529-04258-r2`'
        $accountNoVnc | Should -Match 'token/password/refresh-token observed:\s*`false/false/false`'

        foreach ($content in @($ledger, $evidenceIndex, $descriptor, $matrix)) {
            $content | Should -Match '0\.42\.57-admin-smoke -> 0\.42\.58-admin-smoke'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260529-04257-04258-closed'
            $content | Should -Match 'installed-operator-surface-current-card-2026-05-29-04258'
            $content | Should -Match 'installed-account-novnc-operator-surface-smoke-2026-05-29-04258'
        }
    }

    It 'records 0.42.59 package fullgate manual-admin and operator surface closure' {
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-05-29-04259.md'
        $fullgatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-29-04259-hostmutation.md'
        $manualCampaignPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/manual-admin-campaign-2026-05-29-04258-04259.md'
        $currentCardPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04259.md'

        foreach ($path in @($packagePath, $fullgatePath, $manualCampaignPath, $currentCardPath)) {
            $path | Should -Exist
        }

        $package = Get-Content -Raw -LiteralPath $packagePath
        $fullgate = Get-Content -Raw -LiteralPath $fullgatePath
        $manualCampaign = Get-Content -Raw -LiteralPath $manualCampaignPath
        $currentCard = Get-Content -Raw -LiteralPath $currentCardPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'

        $package | Should -Match 'version:\s*`0\.42\.59-admin-smoke`'
        $package | Should -Match 'msi_sha256:\s*`6976e4f8c862f30884adfbdfda2fb4008aa877a30585e4acd35430750e480585`'
        $package | Should -Match 'payload_aggregate_sha256:\s*`666a1351d58963c7908aad4f66d6469de42747a7c7f70d1e30fb0e94771a5808`'
        $package | Should -Match 'provenance_commit:\s*`63d57feba605f82dabd44a96ed50a4d622f6310a`'

        $fullgate | Should -Match 'batch_id:\s*`full-admin-host-mutation-gate-20260529-04259`'
        $fullgate | Should -Match 'full_gate_msi_sha256:\s*`dff0fce83096ecdf16683307af327af35ae387ed02ac0504948de6633d425596`'
        $fullgate | Should -Match 'host_mutation_performed:\s*`true`'

        $manualCampaign | Should -Match 'baseline_version:\s*`0\.42\.58-admin-smoke`'
        $manualCampaign | Should -Match 'target_version:\s*`0\.42\.59-admin-smoke`'
        $manualCampaign | Should -Match 'descriptor_batch_id:\s*`manual-admin-campaign-descriptor-20260529-04258-04259-closed`'
        $manualCampaign | Should -Match 'update_zip_sha256:\s*`05951af066f0080c9c111de7e104fc8a9418812b68ca0fb246a573d89b6e44fb`'
        $manualCampaign | Should -Match 'burn_bundle_sha256:\s*`96bb7eed5c3a64cc505789ae604f6ea679017215a75ffaa6e5e721c609d8c518`'
        $manualCampaign | Should -Match 'msix_v2_sha256:\s*`a8fbd0e7119b742ebfa8c172a0941d2e8c711c4b5e949019ff75c7663d7dc835`'

        $currentCard | Should -Match 'artifact_summary:\s*`artifacts/installed-operator-surface-current-card-20260529-04259/summary\.json`'
        $currentCard | Should -Match 'full-admin-host-mutation-gate-20260529-04259'
        $currentCard | Should -Match 'manual-admin-campaign-descriptor-20260529-04258-04259-closed'

        foreach ($content in @($ledger, $evidenceIndex, $descriptor, $matrix)) {
            $content | Should -Match '0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260529-04258-04259-closed'
            $content | Should -Match 'installed-operator-surface-current-card-2026-05-29-04259'
            $content | Should -Match 'manual-admin-campaign-2026-05-29-04258-04259'
        }

        foreach ($content in @($ledger, $evidenceIndex, $descriptor)) {
            $content | Should -Match 'pass-code-level-promoted-by-04259-package-chain'
        }

        $matrix | Should -Match 'installed-account-novnc-operator-surface-smoke-2026-05-29-04258'
        $matrix | Should -Match 'not-run-no-account-novnc-payload-change-after-04258'
    }

    It 'records 0.42.59 public-boundary docs maintenance without opening another package candidate' {
        $publicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass.md'
        $predecessorPublicBoundaryPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-05-29-04259-admin-smoke-closure-postpush-pass.md'
        $publicBoundaryPath | Should -Exist
        $predecessorPublicBoundaryPath | Should -Exist

        $publicBoundary = Get-Content -Raw -LiteralPath $publicBoundaryPath
        $predecessorPublicBoundary = Get-Content -Raw -LiteralPath $predecessorPublicBoundaryPath
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlane = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $matrix = Get-RepoText -RelativePath 'docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md'
        $contract = Get-RepoText -RelativePath 'docs/ga-ready/PUBLIC_BOUNDARY_CI_CONTRACT.md'
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $readme = Get-RepoText -RelativePath 'README.md'
        $developerIndex = Get-RepoText -RelativePath 'docs/DEVELOPER_INDEX.md'
        $operationsGuide = Get-RepoText -RelativePath 'docs/OPERATIONS_GUIDE.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $adrIndex = Get-RepoText -RelativePath 'docs/ADR_INDEX.md'

        $publicBoundary | Should -Match 'result:\s*`PASS`'
        $publicBoundary | Should -Match 'run_id:\s*`26636072420`'
        $publicBoundary | Should -Match 'job_id:\s*`78496568595`'
        $publicBoundary | Should -Match 'head_sha:\s*`5a2f91762a6c2a8ab6b84d334fa6cb420474671f`'
        $publicBoundary | Should -Match 'host_mutation_performed:\s*`false`'
        $publicBoundary | Should -Match 'product_payload_change_detected:\s*`false`'
        $publicBoundary | Should -Match 'next_product_payload_package_candidate:\s*`0\.42\.60-admin-smoke`'
        $publicBoundary | Should -Match 'additional_package_candidate_opened:\s*`false`'
        $publicBoundary | Should -Match 'package_candidate_decision:\s*`unchanged-existing-04260-current-card-payload-candidate`'
        $publicBoundary | Should -Match 'recursive_evidence_policy:\s*`docs-maintenance-postpush-does-not-open-additional-package-candidate`'
        $publicBoundary | Should -Match 'installed_account_novnc_rerun_decision:\s*`not-run-no-account-novnc-payload-change-after-04258`'
        $publicBoundary | Should -Match 'actual_vm_guest_execution_qos_smoke_decision:\s*`not-run-no-guest-execution-or-qos-provider-payload-change-after-04259`'
        $publicBoundary | Should -Match 'public_trusted_signing:\s*`not-claimed`'
        $publicBoundary | Should -Match 'external_stable_publication:\s*`not-claimed`'

        foreach ($content in @($ledger, $evidenceIndex, $controlPlane, $descriptor, $matrix, $contract, $agents, $readme, $developerIndex, $operationsGuide, $releaseBoundary, $adrIndex)) {
            $content | Should -Match 'public-boundary-ci-main-push-2026-05-29-04259-docs-maintenance-postpush-pass'
            $content | Should -Match '26636072420'
            $content | Should -Match '5a2f91762a6c2a8ab6b84d334fa6cb420474671f'
            $content | Should -Match '0\.42\.60-admin-smoke'
            $content | Should -Match 'docs-maintenance-postpush'
        }

        foreach ($content in @($ledger, $descriptor, $matrix, $contract, $publicBoundary)) {
            $content | Should -Match 'not-run-no-account-novnc-payload-change-after-04258'
            $content | Should -Match 'not-run-no-guest-execution-or-qos-provider-payload-change-after-04259'
            $content | Should -Match 'docs-maintenance-postpush-does-not-open-additional-package-candidate'
        }

        $predecessorPublicBoundary | Should -Match 'run_id:\s*`26629340294`'
        $predecessorPublicBoundary | Should -Match 'current_evidence_payload_candidate:\s*`true`'
        $contract | Should -Match 'latest_main_push_job_id:\s*78496568595'
        $contract | Should -Match 'previous_main_push_run_id:\s*26629340294'
        $contract | Should -Match 'previous_04257_main_push_run_id:\s*26587524245'
        $ledger | Should -Match 'post_04259_public_boundary_package_chain_decision:\s*`opened-next-product-payload-candidate-current-evidence-rollup`'
        $ledger | Should -Match 'post_04259_public_boundary_docs_maintenance_package_chain_decision:\s*`no-new-package-candidate-existing-04260-current-card-payload-candidate`'
    }

    It 'records Guest Execution redaction hardening code-level evidence and next 0.42.59 gate' {
        $hardeningPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/guest-execution-redaction-hardening-code-level-2026-05-29.md'
        $hardeningPath | Should -Exist

        $hardening = Get-Content -Raw -LiteralPath $hardeningPath
        $adr = Get-RepoText -RelativePath 'docs/adr/0009-guest-execution-security-boundary.md'
        $plan = Get-RepoText -RelativePath 'docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-guest-execution-security-boundary.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'

        $hardening | Should -Match 'result:\s*`PASS_CODE_LEVEL`'
        $hardening | Should -Match 'problem_code:\s*`PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED`'
        $hardening | Should -Match 'next_package_gate_candidate:\s*`0\.42\.59-admin-smoke`'
        $hardening | Should -Match 'next_manual_admin_package_pair_candidate:\s*`0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke`'
        $hardening | Should -Match 'secret_value_observed:\s*`false`'
        $hardening | Should -Match 'credential_ref_value_observed:\s*`false`'
        $hardening | Should -Match 'host_mutation_performed:\s*`false`'
        $hardening | Should -Match 'package_build_performed:\s*`false`'
        $hardening | Should -Match 'public_trusted_signing:\s*`not-claimed`'
        $hardening | Should -Match 'external_stable_publication:\s*`not-claimed`'

        $adr | Should -Match 'secret_redaction_hardening_decision:\s*preview-and-execute-block-secret-like-material'
        $adr | Should -Match 'secret_redaction_hardening_evidence:\s*docs/ga-ready/evidence/guest-execution-redaction-hardening-code-level-2026-05-29\.md'
        $adr | Should -Match 'next_product_payload_gate:\s*0\.42\.59-admin-smoke-package-fullgate-manual-admin'
        $plan | Should -Match '\[x\] Secret-like token'
        $plan | Should -Match 'Guest Execution redaction hardening code-level'

        foreach ($content in @($ledger, $evidenceIndex, $descriptor)) {
            $content | Should -Match 'guest-execution-redaction-hardening-code-level-2026-05-29\.md'
            $content | Should -Match 'pass-code-level-promoted-by-04259-package-chain'
            $content | Should -Match '0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke'
            $content | Should -Match '0\.42\.59-admin-smoke'
        }
    }

    It 'records Hyper-V QoS mutation value hardening code-level evidence and next 0.42.59 gate' {
        $hardeningPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/hyperv-qos-mutation-value-hardening-code-level-2026-05-29.md'
        $hardeningPath | Should -Exist

        $hardening = Get-Content -Raw -LiteralPath $hardeningPath
        $adr = Get-RepoText -RelativePath 'docs/adr/0008-hyperv-qos-mutation-policy.md'
        $plan = Get-RepoText -RelativePath 'docs/superpowers/plans/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $descriptor = Get-RepoText -RelativePath 'docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md'
        $cliUsage = Get-RepoText -RelativePath 'docs/CLI_COMMAND_USAGE.md'
        $cliReadme = Get-RepoText -RelativePath 'src/DesktopNode.Cli/README.md'

        $hardening | Should -Match 'result:\s*`PASS_CODE_LEVEL`'
        $hardening | Should -Match 'problem_codes:\s*`PCV_VM_QOS_STORAGE_RANGE_INVALID`, `PCV_VM_QOS_NETWORK_RANGE_INVALID`'
        $hardening | Should -Match 'preview_native_adapter_called_on_invalid_range:\s*`false`'
        $hardening | Should -Match 'apply_job_created_on_invalid_range:\s*`false`'
        $hardening | Should -Match 'next_package_gate_candidate:\s*`0\.42\.59-admin-smoke`'
        $hardening | Should -Match 'next_manual_admin_package_pair_candidate:\s*`0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke`'
        $hardening | Should -Match 'host_mutation_performed:\s*`false`'
        $hardening | Should -Match 'package_build_performed:\s*`false`'
        $hardening | Should -Match 'public_trusted_signing:\s*`not-claimed`'
        $hardening | Should -Match 'external_stable_publication:\s*`not-claimed`'

        $adr | Should -Match 'value_boundary_hardening_decision:\s*api-cli-preflight-reject-invalid-ranges'
        $adr | Should -Match 'value_boundary_hardening_evidence:\s*docs/ga-ready/evidence/hyperv-qos-mutation-value-hardening-code-level-2026-05-29\.md'
        $adr | Should -Match 'value_boundary_problem_codes:\s*PCV_VM_QOS_STORAGE_RANGE_INVALID, PCV_VM_QOS_NETWORK_RANGE_INVALID'
        $adr | Should -Match 'next_product_payload_gate:\s*0\.42\.59-admin-smoke-package-fullgate-manual-admin'
        $plan | Should -Match '\[x\] Local API preview route'
        $plan | Should -Match 'Hyper-V QoS mutation value-hardening-code-level-2026-05-29|hyperv-qos-mutation-value-hardening-code-level-2026-05-29\.md'
        $cliUsage | Should -Match 'PCV_VM_QOS_STORAGE_RANGE_INVALID'
        $cliUsage | Should -Match 'PCV_VM_QOS_NETWORK_RANGE_INVALID'
        $cliReadme | Should -Match 'PCV_VM_QOS_STORAGE_RANGE_INVALID'
        $cliReadme | Should -Match 'PCV_VM_QOS_NETWORK_RANGE_INVALID'

        foreach ($content in @($ledger, $evidenceIndex, $descriptor)) {
            $content | Should -Match 'hyperv-qos-mutation-value-hardening-code-level-2026-05-29\.md'
            $content | Should -Match 'pass-code-level-promoted-by-04259-package-chain'
            $content | Should -Match 'PCV_VM_QOS_STORAGE_RANGE_INVALID'
            $content | Should -Match 'PCV_VM_QOS_NETWORK_RANGE_INVALID'
            $content | Should -Match '0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke'
            $content | Should -Match '0\.42\.59-admin-smoke'
        }
    }

    It 'keeps the active product boundary Web and CLI only' {
        $tuiSourcePath = Join-Path $script:RepoRoot 'src/DesktopNode.Tui'
        $tuiTestsPath = Join-Path $script:RepoRoot 'src/DesktopNode.Tui.Tests'
        $installedTuiSmokePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1'
        $solution = Get-RepoText -RelativePath 'src/DesktopNode.sln'
        $adr = Get-RepoText -RelativePath 'docs/adr/0011-cli-web-only-operator-surface.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $userGuide = Get-RepoText -RelativePath 'docs/USER_GUIDE.md'
        $featureUsage = Get-RepoText -RelativePath 'docs/USER_FEATURE_USAGE_SPEC.md'

        $tuiSourcePath | Should -Not -Exist
        $tuiTestsPath | Should -Not -Exist
        $installedTuiSmokePath | Should -Not -Exist
        $solution | Should -Not -Match 'DesktopNode\.Tui'
        $solution | Should -Not -Match 'F1471821-A992-4D9C-856E-CC104CF12239'
        $solution | Should -Not -Match '51F1CC27-5E90-4ACB-91CE-F2137119B66E'
        $adr | Should -Match 'DESKTOP_NODE_OPERATOR_SURFACE_DECISION:\s*cli-web-only'
        $adr | Should -Match 'DESKTOP_NODE_TUI_DECISION:\s*removed'
        $ledger | Should -Match 'active_operator_surface_decision:\s*`cli-web-only`'
        $ledger | Should -Match 'tui_product_status:\s*`removed-from-active-product`'
        $featureUsage | Should -Not -Match '(?m)^\|\s*TUI\b'
        $userGuide | Should -Not -Match '(?im)^#{1,6}\s+.*TUI\b'
        $userGuide | Should -Not -Match 'pcvtui\.exe'
    }

    It 'records the 0.42.65 historical anchor documents and the current canonical linkage' {
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-07-16-04265.md'
        $fullgatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-16-04265-hostmutation.md'
        $functionalPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/functional-correctness-actual-host-validation-2026-07-16-04265.md'
        $currentCardPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-16-04265.md'

        foreach ($path in @($packagePath, $fullgatePath, $functionalPath, $currentCardPath)) {
            $path | Should -Exist
        }

        $package = Get-Content -Raw -LiteralPath $packagePath
        $fullgate = Get-Content -Raw -LiteralPath $fullgatePath
        $functional = Get-Content -Raw -LiteralPath $functionalPath
        $currentCard = Get-Content -Raw -LiteralPath $currentCardPath
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlane = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'
        $releaseBoundary = Get-RepoText -RelativePath 'docs/PUBLIC_RELEASE_BOUNDARY.md'

        $package | Should -Match 'result:\s*`PACKAGE_BUILD_PASS`'
        $package | Should -Match 'version:\s*`0\.42\.65-admin-smoke`'
        $package | Should -Match 'msi_sha256:\s*`5709edb0d5f265393c8690c212dd6d1f61873f7cbbaa110b1654a2e380e6b748`'
        $package | Should -Match 'payload_aggregate_sha256:\s*`3b4fefb3c03c1a70ba804e959931bdec0ee36923139a84602e85be69e96e251a`'
        $package | Should -Match 'provenance_commit:\s*`4855947fe0199cedc978e8b40ffb45e96ced6876`'
        $package | Should -Match 'pcvtui_present:\s*`false`'
        $package | Should -Match 'msi_active_tui_file_rows:\s*`0`'

        $fullgate | Should -Match 'result:\s*`PASS`'
        $fullgate | Should -Match 'batch_id:\s*`full-admin-host-mutation-gate-20260716-04265`'
        $fullgate | Should -Match 'full_gate_msi_sha256:\s*`9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30`'
        $fullgate | Should -Match 'full_gate_payload_aggregate_sha256:\s*`5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28`'
        $fullgate | Should -Match 'executed_steps:\s*`2/2`'
        $fullgate | Should -Match 'os_mutation_steps:\s*`11/11`'
        $fullgate | Should -Match 'host_mutation_performed:\s*`true`'

        $functional | Should -Match 'result:\s*`PASS_WITH_DOCUMENTED_HOST_LIMITATION`'
        $functional | Should -Match '2048 Kbps.*2,048,000 bps'
        $functional | Should -Match 'PCV_VM_DISK_SHRINK_NOT_SUPPORTED'
        $functional | Should -Match 'PCV_HYPERV_WMI_JOB_FAILED'
        $functional | Should -Match '0x80070057'
        $functional | Should -Match 'validation_vm_cleanup:\s*`PASS`'

        $currentCard | Should -Match 'result:\s*`PASS`'
        $currentCard | Should -Match 'operator_surfaces:\s*`web,cli`'
        $currentCard | Should -Match 'tui_present:\s*`false`'
        $currentCard | Should -Match 'cli_exit_zero_count:\s*`3`'
        $currentCard | Should -Match 'web_http_200_count:\s*`2`'
        $currentCard | Should -Match 'service_state:\s*`Running/Automatic`'
        $currentCard | Should -Match 'remaining_test_vm_count:\s*`0`'
        $currentCard | Should -Match 'secret_observed:\s*`false`'

        foreach ($content in @($package, $fullgate, $functional, $currentCard)) {
            $content | Should -Match 'public_trusted_signing:\s*`not-claimed`'
            $content | Should -Match 'external_stable_publication:\s*`not-claimed`'
        }

        # The four 04265 documents above are historical records and stay pinned. The index and
        # ledger rows below track whatever the canonical record currently declares: pinning them
        # to one version made every legitimate anchor promotion fail this contract, which is the
        # same frozen-status defect recorded in docs/project-status-audit-2026-08-05.md section 3.2.
        $record = Get-Content -Raw -LiteralPath (
            Join-Path $script:RepoRoot 'docs/ga-ready/current-evidence.json') | ConvertFrom-Json
        $currentVersion = [regex]::Escape([string]$record.current.version)
        $currentBatch = [regex]::Escape([string]$record.current.fullgate_batch)

        foreach ($content in @($agents, $packagingReadme, $ledger, $evidenceIndex, $controlPlane)) {
            $content | Should -Match $currentVersion
            $content | Should -Match $currentBatch
        }
        $releaseBoundary | Should -Match 'internal-private-network-only'

        $ledger | Should -Match "\|\s*``full-admin-host-mutation-current``\s*\|\s*``pass``,\s*``$currentVersion``\s*\|"
        $ledger | Should -Match "\|\s*``package-build-current``\s*\|\s*``package-build-pass``,\s*``$currentVersion``\s*\|"
        $ledger | Should -Match "\|\s*``installed-operator-surface-smoke-latest``\s*\|\s*``pass``,\s*installed\s*``$currentVersion``\s*\|"
        $ledger | Should -Match ([regex]::Escape("current_manual_admin_package_pair: ``$($record.manual_admin.latest_closed_baseline) -> $($record.manual_admin.latest_closed_target)``"))
    }

    It 'records 0.42.62 WMI topology recovery current evidence' {
        $rcaPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/wmi-internal-switch-topology-recovery-2026-07-13-04260-04262.md'
        $packagePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/admin-smoke-package-2026-07-13-04262.md'
        $fullgatePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-13-04262-hostmutation.md'
        $currentCardPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-13-04262.md'

        foreach ($path in @($rcaPath, $packagePath, $fullgatePath, $currentCardPath)) {
            $path | Should -Exist
        }

        $rca = Get-Content -Raw -LiteralPath $rcaPath
        $package = Get-Content -Raw -LiteralPath $packagePath
        $fullgate = Get-Content -Raw -LiteralPath $fullgatePath
        $currentCard = Get-Content -Raw -LiteralPath $currentCardPath
        $agents = Get-RepoText -RelativePath 'AGENTS.md'
        $packagingReadme = Get-RepoText -RelativePath 'packaging/windows-desktop-node/README.md'
        $ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        $controlPlane = Get-RepoText -RelativePath 'docs/ga-ready/CONTROL_PLANE_INDEX.md'

        $rca | Should -Match '0\.42\.60_failure_code:\s*`PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`'
        $rca | Should -Match '0\.42\.61_failure_code:\s*`PCV_NETWORK_INVENTORY_FAILED`'
        $rca | Should -Match '0\.42\.61_exception_type:\s*`System\.InvalidOperationException`'
        $rca | Should -Match '0\.42\.60_package_and_msi_lifecycle:\s*`PASS`'
        $rca | Should -Match '0\.42\.61_package_and_msi_lifecycle:\s*`PASS`'
        $rca | Should -Match '0\.42\.60_os_mutation_performed:\s*`false`'
        $rca | Should -Match '0\.42\.61_os_mutation_performed:\s*`false`'
        $rca | Should -Match '0\.42\.60_pass_anchor:\s*`false`'
        $rca | Should -Match '0\.42\.61_pass_anchor:\s*`false`'

        $package | Should -Match 'version:\s*`0\.42\.62-admin-smoke`'
        $package | Should -Match 'msi_sha256:\s*`ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`'
        $package | Should -Match 'payload_aggregate_sha256:\s*`0b3f1c1e400204d6855221b4ac51873126e4c02a1e44380f5457b221475c080e`'
        $package | Should -Match 'provenance_commit:\s*`7f71f0a518c5b592f233373522d36b5401c3f1df`'
        $package | Should -Match 'signing_mode:\s*`AllowUnsignedDev`'
        $package | Should -Match 'signing_trust_model:\s*`LocalTest`'

        $fullgate | Should -Match 'result:\s*`PASS`'
        $fullgate | Should -Match 'version:\s*`0\.42\.62-admin-smoke`'
        $fullgate | Should -Match 'batch_id:\s*`full-admin-host-mutation-gate-20260713-04262`'
        $fullgate | Should -Match 'clean_package_msi_sha256:\s*`ae0b23710ce986ad3d068b494823af7b5cc7bf1d66021fa302db2dab7a313533`'
        $fullgate | Should -Match 'full_gate_msi_sha256:\s*`c7fc7b8003c1ad993b49d5a0c6444dd436d09e6c0210d01400fb8045ab404b0f`'
        $fullgate | Should -Match 'full_gate_payload_aggregate_sha256:\s*`ef653620a527c7528d3a97202cfdc32ad3f45bf70247171a2ca2fdb915852a2f`'
        $fullgate | Should -Match 'provenance_commit:\s*`7f71f0a518c5b592f233373522d36b5401c3f1df`'
        $fullgate | Should -Match 'executed_steps:\s*`2/2`'
        $fullgate | Should -Match 'host_mutation_performed:\s*`true`'

        $currentCard | Should -Match 'result:\s*`PASS`'
        $currentCard | Should -Match 'artifact_summary:\s*`artifacts/installed-operator-surface-current-card-20260713-04262/summary\.json`'
        $currentCard | Should -Match 'fullgate_batch:\s*`full-admin-host-mutation-gate-20260713-04262`'
        $currentCard | Should -Match 'cli_exit_zero_count:\s*`5`'
        $currentCard | Should -Match 'tui_exit_zero_count:\s*`2`'
        $currentCard | Should -Match 'web_http_200_count:\s*`3`'
        $currentCard | Should -Match 'Default Switch.*internal.*allow_management_os=true'
        $currentCard | Should -Match 'WSL \(Hyper-V firewall\).*internal.*allow_management_os=true'
        $currentCard | Should -Match 'service_state:\s*`Running/Automatic`'
        $currentCard | Should -Match 'token_value_observed:\s*`false`'
        $currentCard | Should -Match 'password_value_observed:\s*`false`'

        foreach ($content in @($rca, $package, $fullgate, $currentCard)) {
            $content | Should -Match 'evidence_scope:\s*`internal-admin-smoke-only`'
            $content | Should -Match 'public_trusted_signing:\s*`excluded|not-claimed`'
            $content | Should -Match 'external_stable_publication:\s*`not-claimed`'
        }

        foreach ($content in @($agents, $packagingReadme, $ledger, $evidenceIndex, $controlPlane)) {
            $content | Should -Match '0\.42\.62-admin-smoke'
            $content | Should -Match 'full-admin-host-mutation-gate-20260713-04262'
            $content | Should -Match 'wmi-internal-switch-topology-recovery-2026-07-13-04260-04262\.md'
            $content | Should -Match '0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke'
            $content | Should -Match 'manual-admin-campaign-descriptor-20260529-04258-04259-closed'
        }

        $ledger | Should -Match '\|\s*`full-admin-host-mutation-current`\s*\|\s*`pass`,\s*`0\.42\.62-admin-smoke`\s*\|'
        # `manual-admin-package-pair-current` has no 04262-era row to pin to: it exists only as the
        # current row, so a pinned version made every legitimate manual-admin closure fail here.
        # Same frozen-status defect as docs/project-status-audit-2026-08-05.md section 3.2, so this
        # tracks whatever the canonical record declares while the 04262 rows above stay pinned.
        $currentEvidenceRecord = Get-Content -Raw -LiteralPath (
            Join-Path $script:RepoRoot 'docs/ga-ready/current-evidence.json') | ConvertFrom-Json
        $currentPair = [regex]::Escape(
            "$($currentEvidenceRecord.manual_admin.latest_closed_baseline) -> $($currentEvidenceRecord.manual_admin.latest_closed_target)")
        $ledger | Should -Match "\|\s*``manual-admin-package-pair-current``\s*\|\s*``pass``,\s*``$currentPair``\s*\|"
        $ledger | Should -Match '\|\s*`package-build-current`\s*\|\s*`package-build-pass`,\s*`0\.42\.62-admin-smoke`\s*\|'
        $ledger | Should -Match '\|\s*`latest-product-payload-smoke`\s*\|\s*`pass`,\s*package\s*`0\.42\.62-admin-smoke`\s*\|'
        $ledger | Should -Match '\|\s*`installed-operator-surface-smoke-latest`\s*\|\s*`historical-predecessor-pass`,\s*installed\s*`0\.42\.62-admin-smoke`\s*\|'
        $ledger | Should -Match "\|\s*``manual-admin-package-pair-latest-candidate``\s*\|\s*``pass-closed``,\s*``$currentPair``\s*\|"
        $ledger | Should -Match '\|\s*`operator-surface-account-novnc-current`\s*\|\s*`pass`,\s*installed\s*`0\.42\.58-admin-smoke`\s*\|'
        $ledger | Should -Match 'current_manual_admin_update_zip_owner:\s*`0\.42\.58-admin-smoke -> 0\.42\.59-admin-smoke`'
    }

    It 'delegates active current summaries to the canonical JSON generator' {
        $recordPath = Join-Path $script:RepoRoot 'docs/ga-ready/current-evidence.json'
        $generatorPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1'
        $record = Get-Content -Raw -LiteralPath $recordPath | ConvertFrom-Json
        $generator = Get-Content -Raw -LiteralPath $generatorPath
        # The repository root README is the front door. It drifted to a 0.42.63 anchor
        # while the canonical record read 0.42.65 precisely because it sat outside this
        # list, so it is owned by the generator like every other current-facing document.
        $targets = @(
            'README.md',
            'AGENTS.md',
            'docs/ga-ready/EVIDENCE_INDEX.md',
            'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md',
            'docs/ga-ready/CONTROL_PLANE_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md'
        )

        $record.contract | Should -Be 'pcv-current-evidence-v1'
        @($record.current.operator_surfaces) | Should -Be @('web', 'cli')
        $record.current.tui_present | Should -BeFalse
        $record.claims.public_trusted_signing | Should -BeFalse
        $record.claims.external_stable_publication | Should -BeFalse

        foreach ($relativePath in $targets) {
            $content = Get-RepoText -RelativePath $relativePath
            $block = [regex]::Match(
                $content,
                '(?s)<!-- BEGIN GENERATED CURRENT EVIDENCE -->.*?<!-- END GENERATED CURRENT EVIDENCE -->').Value
            $block | Should -Not -BeNullOrEmpty -Because $relativePath
            $block | Should -Match ([regex]::Escape([string]$record.current.version))
            $block | Should -Match ([regex]::Escape([string]$record.current.fullgate_batch))
            $block | Should -Match ([regex]::Escape([string]$record.current.provenance_commit))
            $block | Should -Not -Match 'Web/TUI/CLI current-card'
            $generator | Should -Match ([regex]::Escape("'$relativePath'"))
        }
        $generator | Should -Match '\[switch\]\$Check'
        $generator | Should -Match 'PCV_CURRENT_EVIDENCE_STALE'
    }

    It 'keeps component archive baseline suites out of active ADR verification commands' {
        $ownership = Get-RepoText -RelativePath 'docs/ga-ready/VERIFICATION_OWNERSHIP.md'
        $ownership | Should -Match 'archive/spikes/purecvisor-desktop-node/tests'
        $ownership | Should -Match 'excluded from default required command'

        $adrRoot = Join-Path $script:RepoRoot 'docs/adr'
        $adrFiles = @(Get-ChildItem -LiteralPath $adrRoot -Filter '*.md' -File)
        $adrFiles.Count | Should -BeGreaterThan 0

        foreach ($adrFile in $adrFiles) {
            $content = Get-Content -Raw -LiteralPath $adrFile.FullName
            $content |
                Should -Not -Match "Invoke-Pester\s+-Path\s+'archive/spikes/purecvisor-desktop-node/tests'" `
                -Because "$($adrFile.Name) must not prescribe the excluded component/archive baseline suite as a verification command"
        }
    }

    # Salvaged from the abandoned PR #172 branch, whose own contract additionally
    # required all four of AGENTS.md / EVIDENCE_INDEX / CONTROL_PLANE_INDEX /
    # CURRENT_EVIDENCE_LEDGER to repeat every evidence filename. That duplication is
    # what the canonical current-evidence generator replaced, and three of those files
    # now carry generator-owned blocks, so linkage is asserted against the evidence
    # index alone.
    It 'preserves the post 0.42.62 operational follow-up evidence records' {
        $relativePaths = @(
            'docs/ga-ready/evidence/public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md',
            'docs/ga-ready/evidence/manual-admin-campaign-2026-07-13-04259-04262.md',
            'docs/ga-ready/evidence/secondary-hyperv-wmi-topology-smoke-2026-07-13-04262.md',
            'docs/ga-ready/evidence/post-04262-worktree-cleanup-2026-07-13.md'
        )
        $contents = foreach ($relativePath in $relativePaths) {
            $path = Join-Path $script:RepoRoot $relativePath
            $path | Should -Exist
            Get-Content -Raw -LiteralPath $path
        }

        $contents[0] | Should -Match 'result:\s*`PASS`'
        $contents[0] | Should -Match 'run_id:\s*`\d+`'
        $contents[0] | Should -Match 'job_id:\s*`\d+`'
        $contents[0] | Should -Match 'head_sha:\s*`[a-f0-9]{40}`'
        $contents[0] | Should -Match 'additional_package_candidate_opened:\s*`false`'
        $contents[1] | Should -Match 'result:\s*`BLOCKED_DEDICATED_BASELINE_HOST_REQUIRED`'
        $contents[1] | Should -Match 'baseline_version:\s*`0\.42\.59-admin-smoke`'
        $contents[1] | Should -Match 'target_version:\s*`0\.42\.62-admin-smoke`'
        $contents[2] | Should -Match 'result:\s*`BLOCKED_NO_SECONDARY_HYPERV_HOST`'
        $contents[3] | Should -Match 'removed_worktree_count:\s*`\d+`'
        $contents[3] | Should -Match 'preserved_worktree_count:\s*`\d+`'

        foreach ($content in $contents) {
            $content | Should -Match 'host_mutation_performed:\s*`false`'
            $content | Should -Match 'public_trusted_signing:\s*`(excluded|not-claimed)`'
            $content | Should -Match 'external_stable_publication:\s*`not-claimed`'
        }

        # These are historical records. They must not be readable as the current
        # manual-admin closure, which the canonical evidence record owns.
        $contents[1] | Should -Match 'manual_admin_current_closure_changed:\s*`false`'

        $evidenceIndex = Get-RepoText -RelativePath 'docs/ga-ready/EVIDENCE_INDEX.md'
        foreach ($relativePath in $relativePaths) {
            $evidenceIndex |
                Should -Match ([regex]::Escape((Split-Path -Leaf $relativePath))) `
                -Because 'salvaged evidence must stay discoverable from the evidence index'
        }
    }
}
