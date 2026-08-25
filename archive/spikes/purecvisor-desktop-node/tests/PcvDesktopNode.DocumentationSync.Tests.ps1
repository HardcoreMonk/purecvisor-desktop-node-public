Set-StrictMode -Version Latest

Describe 'Desktop Node documentation synchronization guard' {
    BeforeAll {
        $script:RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')
        $script:HighLevelDocs = @(
            'README.md',
            'AGENTS.md',
            'follower.md',
            'docs/USER_GUIDE.md',
            'docs/GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/ADR_INDEX.md',
            'docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md',
            'docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md',
            'docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'archive/spikes/purecvisor-desktop-node/README.md',
            'archive/spikes/purecvisor-desktop-node/api/README.md',
            'archive/spikes/purecvisor-desktop-node/cli/README.md',
            'archive/spikes/purecvisor-desktop-node/hyperv/README.md',
            'archive/spikes/purecvisor-desktop-node/service/README.md'
        )
    }

    It 'keeps Desktop Node pass counts out of high-level status documents' {
        foreach ($relativePath in $script:HighLevelDocs) {
            $path = Join-Path $script:RepoRoot $relativePath
            Test-Path -LiteralPath $path | Should -BeTrue

            $content = Get-Content -LiteralPath $path -Raw
            $content | Should -Not -Match '\b\d+\s+passed,\s+0\s+failed(?:,\s+\d+\s+NotRun)?'
        }
    }

    It 'prevents stale Phase 13 backlog wording in active documents' {
        $forbiddenPatterns = @(
            'P0 예정\.\s*Desktop Node Phase 13',
            'Phase 13은 native Windows service host 또는 검증된 service wrapper를 도입해야 한다',
            'Phase 13은 loopback static asset 무인증, API bearer 유지로 이 접근 경계를 해소한다',
            '전체 Local API 기대 결과를 `85 passed, 0 failed`로 갱신했다'
        )

        foreach ($relativePath in $script:HighLevelDocs) {
            $path = Join-Path $script:RepoRoot $relativePath
            $content = Get-Content -LiteralPath $path -Raw

            foreach ($pattern in $forbiddenPatterns) {
                $content | Should -Not -Match $pattern
            }
        }
    }

    It 'prevents stale Phase 14 backlog wording in active documents' {
        $forbiddenPatterns = @(
            'Phase 14 \| Task 5까지 구현 중',
            'Phase 14 검증을 계속 진행한다',
            '다음 단계는 Task 6 end-to-end verification이다',
            'plan 작성 완료, 구현 대기'
        )

        foreach ($relativePath in $script:HighLevelDocs) {
            $path = Join-Path $script:RepoRoot $relativePath
            $content = Get-Content -LiteralPath $path -Raw

            foreach ($pattern in $forbiddenPatterns) {
                $content | Should -Not -Match $pattern
            }
        }
    }

    It 'prevents stale Phase 15 backlog wording in active documents' {
        $forbiddenPatterns = @(
            'Phase 15 후보\s*\|\s*미작성',
            'Phase 15 준비',
            'Phase 15 이후 후보',
            'DPAPI 또는 Windows Credential Manager 기반 token storage를 확정한다',
            'Windows Credential Manager 또는 DPAPI token storage'
        )

        foreach ($relativePath in $script:HighLevelDocs) {
            $path = Join-Path $script:RepoRoot $relativePath
            $content = Get-Content -LiteralPath $path -Raw

            foreach ($pattern in $forbiddenPatterns) {
                $content | Should -Not -Match $pattern
            }
        }
    }

    It 'prevents stale Phase 19 backlog wording in active documents' {
        $forbiddenPatterns = @(
            'Phase 19: Desktop Node 제품 승격 재판정\s*\n\s*- `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지할지, 별도 GA gate로 전환할지 결정한다',
            'signed release build, MSI smoke, update/rollback, diagnostics, LAN/TLS, Hyper-V integration gate를 다시 판정한다',
            'Phase 19 후보: 제품 승격 재판정',
            'Phase 19 후보\s*\|\s*미작성',
            'Phase 11 기준 Desktop Node는 제품 런타임으로 승격하지 않고',
            'signed release build, 업데이트, 롤백, 로그 수집, 서비스 복구, 배포 단위, version policy',
            'Phase 18까지의 wrapper는'
        )

        foreach ($relativePath in $script:HighLevelDocs) {
            $path = Join-Path $script:RepoRoot $relativePath
            $content = Get-Content -LiteralPath $path -Raw

            foreach ($pattern in $forbiddenPatterns) {
                $content | Should -Not -Match $pattern
            }
        }
    }

    It 'points high-level docs to the verification policy instead of duplicating expected counts' {
        $readme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'README.md') -Raw
        $follower = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'follower.md') -Raw

        $readme | Should -Match 'docs/DEVELOPMENT_VERIFICATION_POLICY\.md'
        $follower | Should -Match 'DEVELOPMENT_VERIFICATION_POLICY\.md'
        $follower | Should -Match '완료 증거'
    }

    It 'pins the project-close development acceleration standard' {
        $policy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw

        $policy | Should -Match '프로젝트 종료까지 고정'
        $policy | Should -Match 'public trusted signing'
        $policy | Should -Match 'scope 밖'
        $policy | Should -Match 'AllowUnsignedDev'
        $policy | Should -Match 'internal `RequireSigned`'
        $policy | Should -Match 'admin-smoke evidence'
        $policy | Should -Match '작은 vertical slice'
        $policy | Should -Match 'PowerShell-backed current owner 0개'
        $policy | Should -Match 'active product `spikes/\*\*` reference 0개'
        $policy | Should -Match 'archive/spikes/purecvisor-desktop-node/\*\*'
        $policy | Should -Match 'archive/read-only rollback evidence'
        $policy | Should -Match 'milestone마다 `AllowUnsignedDev` 또는 internal `RequireSigned` installed smoke'
        $policy | Should -Match '실제 Hyper-V/Service/MSI host mutation'

        $developerIndex | Should -Match '개발 가속 고정 기준'
        $developerIndex | Should -Match 'docs/DEVELOPMENT_VERIFICATION_POLICY\.md'
    }

    It 'keeps installed product usage and latest OS mutation evidence discoverable' {
        $readme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'README.md') -Raw
        $userGuide = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/USER_GUIDE.md') -Raw
        $guide = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/GUIDE.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $policy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $releaseBoundary = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw
        $agents = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'AGENTS.md') -Raw
        $productWrapper = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/README.md') -Raw
        $installer = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/installer/README.md') -Raw

        $readme | Should -Match '제품 사용 빠른 시작'
        $readme | Should -Match 'http://127\.0\.0\.1:7777/'
        $readme | Should -Match 'PureCVisorDesktopNode'
        $readme | Should -Match 'api-token\.dpapi\.json'
        $readme | Should -Match 'docs/USER_GUIDE\.md'
        $userGuide | Should -Match 'PureCVisor Desktop Node 유저 가이드'
        $userGuide | Should -Match 'Web Console'
        $userGuide | Should -Match 'Tracked Jobs'
        $userGuide | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
        $userGuide | Should -Match 'Public trusted signing'
        $userGuide | Should -Match 'rollback/final-state proof'
        $guide | Should -Match '제품 사용 빠른 시작'
        $guide | Should -Match 'docs/USER_GUIDE\.md'
        $developerIndex | Should -Match '제품 실행/사용 확인'
        $developerIndex | Should -Match 'docs/USER_GUIDE\.md'
        $productWrapper | Should -Match '설치본 사용'
        $productWrapper | Should -Match 'docs/USER_GUIDE\.md'
        $installer | Should -Match '설치 후 사용'
        $installer | Should -Match 'docs/USER_GUIDE\.md'

        foreach ($content in @($readme, $guide, $policy, $releaseBoundary, $agents, $productWrapper, $installer)) {
            $content | Should -Match '0\.35\.7-admin-smoke'
            $content | Should -Match 'os-mutation-gates-20260505-180434-0357-rerun'
        }

        $policy | Should -Match 'final service는 loopback-only `Running`'
        $releaseBoundary | Should -Match 'Public trusted signing과 외부 stable publication'
    }

    It 'keeps batch-supervised admin smoke evidence separate from the latest OS mutation gate' {
        $docsRequiringBatchEvidence = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md',
            'docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md',
            'docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md'
        )

        foreach ($relativePath in $docsRequiringBatchEvidence) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match '0\.36\.1-admin-smoke'
            $content | Should -Match 'batch-supervisor-host-mutating-admin-smoke-20260505-201026'
            $content | Should -Match 'routeparity-service-msi-hyperv-batch-supervised-20260505-201026-0361'
        }

        $docsPreservingLatestOsGate = @(
            'README.md',
            'AGENTS.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'docs/ga-ready/evidence/batch-supervised-admin-smoke-2026-05-05-0361.md'
        )

        foreach ($relativePath in $docsPreservingLatestOsGate) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match '0\.35\.7-admin-smoke'
            $content | Should -Match 'os-mutation-gates-20260505-180434-0357-rerun'
            $content | Should -Not -Match '0\.36\.1-admin-smoke` 현재 HEAD OS gate'
            $content | Should -Not -Match '최신 OS gate는 `0\.36\.1-admin-smoke`'
            $content | Should -Not -Match '0\.36\.1-admin-smoke`.*Hyper-V/MSI/firewall/LAN/Event Log/internal trust-store gate'
        }
    }

    It 'keeps current aggregate snapshot aligned after migration apply installed smoke' {
        $currentSnapshotDocs = @(
            'README.md',
            'follower.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md'
        )

        foreach ($relativePath in $currentSnapshotDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match '0\.38\.6-admin-smoke'
            $content | Should -Match 'future-route` exclusion 0개'
            $content | Should -Match 'product-operation` 24개'
            $content | Should -Match 'current-native` 42개'
            $content | Should -Not -Match '현재 재계산 값은 GA-scope blocked row 0개, PowerShell-backed current owner 0개, active product `spikes/\*\*` reference 0개, future implementation exclusion 2개'
            $content | Should -Not -Match 'installed destructive admin smoke evidence는 별도 gate다'
        }

        $policy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $policy | Should -Match 'artifacts/config-jobstore-migration-apply-installed-20260507-0386'
    }

    It 'keeps 0.38.7 signed build separate from blocked host mutation evidence' {
        $evidencePath = 'docs/ga-ready/evidence/host-mutation-signed-build-attempt-2026-05-07-0387.md'
        $docsRequiring0387Attempt = @(
            'README.md',
            'follower.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            $evidencePath
        )

        foreach ($relativePath in $docsRequiring0387Attempt) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match '0\.38\.7-rc\.1'
            $content | Should -Match 'internal-enterprise-requiresigned-rc-msi-20260507-0387'
            $content | Should -Match 'c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602'
            $content | Should -Match 'PCV_BATCH_ADMIN_REQUIRED'
            $content | Should -Match 'full-admin-host-mutation-gate-20260507-0387'
            $content | Should -Match 'product-update-rollback-mutation-20260507-0387'
            $content | Should -Match 'host_mutation_performed=false|host_mutation_performed: false'
            $content | Should -Match '0\.38\.9-admin-smoke'
            $content | Should -Not -Match '0\.38\.7-admin-smoke.*gate가 모두 PASS'
            $content | Should -Not -Match '0\.38\.7-admin-smoke.*full admin host mutation gate PASS'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot $evidencePath) -Raw
        $evidence | Should -Match 'signed_rc_authenticode_status: Valid'
        $evidence | Should -Match 'signtool_verify_exit: 0'
        $evidence | Should -Match 'update_rollback_host_mutation_performed: false'
        $evidence | Should -Match 'sc\.exe stop PureCVisorDesktopNode'
        $evidence | Should -Match 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT'
        $evidence | Should -Match 'public trusted signing: excluded'
        $evidence | Should -Match 'external stable publication: not-claimed'
        $evidence | Should -Match '최신 실제 full admin host mutation PASS evidence는 `0\.38\.9-admin-smoke`'
    }

    It 'keeps Beta Web Dashboard smoke scoped to read-only static fixture evidence' {
        $evidencePath = 'docs/ga-ready/evidence/beta-web-dashboard-smoke-2026-05-07.md'
        $docsRequiringBetaSmoke = @(
            'README.md',
            'follower.md',
            'packaging/windows-desktop-node/README.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            $evidencePath
        )

        foreach ($relativePath in $docsRequiringBetaSmoke) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'beta-web-dashboard-smoke-2026-05-07'
            $content | Should -Match 'beta-web-dashboard-smoke-20260507-025743'
            $content | Should -Match 'WebRegression'
            $content | Should -Match '26 tests'
            $content | Should -Match 'total_steps=4|total_steps 4'
            $content | Should -Match 'executed_steps=4|executed_steps 4'
            $content | Should -Match 'read-only/static fixture|read-only/static fixture beta'
            $content | Should -Not -Match 'Beta-0 Web Dashboard.*Hyper-V.*PASS'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot $evidencePath) -Raw
        $evidence | Should -Match 'Hyper-V VM 생성/삭제'
        $evidence | Should -Match 'installed update/rollback mutation'
        $evidence | Should -Match 'public trusted signing'
        $evidence | Should -Match '외부 stable publication'
    }

    It 'keeps 0.38.8 update rollback elevated pass separate from public release claims' {
        $evidencePath = 'docs/ga-ready/evidence/product-update-rollback-mutation-2026-05-07-0388.md'
        $docsRequiring0388Pass = @(
            'README.md',
            'follower.md',
            'AGENTS.md',
            'docs/ADR_INDEX.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/GUIDE.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'packaging/windows-desktop-node/installer/README.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            $evidencePath
        )

        foreach ($relativePath in $docsRequiring0388Pass) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match '0\.38\.8-admin-smoke'
            $content | Should -Match 'product-update-rollback-mutation-20260507-0388-elevated-pass'
            $content | Should -Match '163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564'
            $content | Should -Match '0\.38\.6-admin-smoke'
            $content | Should -Match 'DesktopNode\.failed'
            $content | Should -Match 'succeeded/health'
            $content | Should -Match 'health:? `?200`?|health 200|health_status_code: 200'
            $content | Should -Match 'host_mutation_performed=true|host_mutation_performed: true|host_mutation_performed=`true`'
            $content | Should -Match 'public trusted signing|Public trusted signing'
            $content | Should -Match '외부 stable publication|external stable publication'
            $content | Should -Not -Match '0\.38\.8-admin-smoke.*public trusted signing.*PASS'
            $content | Should -Not -Match '0\.38\.8-admin-smoke.*external stable publication.*PASS'
        }

        $routeMatrix = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md') -Raw
        $routeMatrix | Should -Match 'local payload update'
        $routeMatrix | Should -Match 'rollback restore'
        $routeMatrix | Should -Match 'product-update-rollback-mutation-20260507-0388-elevated-pass'
        $routeMatrix | Should -Match 'succeeded/health'
        $routeMatrix | Should -Match 'DesktopNode\.failed'

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot $evidencePath) -Raw
        $evidence | Should -Match 'build_and_blocked_attempt_root: artifacts/product-update-rollback-mutation-20260507-0388'
        $evidence | Should -Match 'elevated_pass_root: artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass'
        $evidence | Should -Match 'transaction_journal_status: succeeded'
        $evidence | Should -Match 'transaction_journal_stage: health'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match '0\.38\.8-admin-smoke` installed destructive update/rollback smoke는 elevated PASS'
        $evidence | Should -Match 'public trusted signing 또는 외부 stable publication evidence가 아니다'
    }

    It 'keeps packaging future phase separate from completed Operator Web UX expansion' {
        $evidencePath = 'docs/ga-ready/evidence/packaging-operator-backlog-rebaseline-2026-05-07.md'
        $backlogPath = 'docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md'
        $followerPath = 'follower.md'
        $contentDocs = @(
            $evidencePath,
            $backlogPath,
            $followerPath
        )

        foreach ($relativePath in $contentDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'packaging-operator-backlog-rebaseline-2026-05-07'
            $content | Should -Match 'future-noncurrent'
            $content | Should -Match 'Burn bootstrapper'
            $content | Should -Match 'MSIX'
            $content | Should -Match 'winget manifest'
            $content | Should -Match 'network download updater'
            $content | Should -Match 'full transactional rollback'
            $content | Should -Match 'Windows Credential Manager transition'
            $content | Should -Match 'default Windows Event Log writer/provider transition'
            $content | Should -Match 'built-in TLS certificate lifecycle'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match '외부 stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot $evidencePath) -Raw
        $evidence | Should -Match 'Operator/Web UX 계획 문서는 현재 모두 checkbox closure 상태다'
        $evidence | Should -Match '남은 UX 후보는 새 backlog'
        $evidence | Should -Match 'unchecked 0'

        $operatorPlanPaths = @(
            'docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p0.md',
            'docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p1.md',
            'docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p2.md',
            'docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-vm-delete-ui.md',
            'docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-web-dashboard-ops-cockpit-redesign.md'
        )

        foreach ($relativePath in $operatorPlanPaths) {
            $plan = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw
            $plan | Should -Not -Match '(?m)^- \[ \]'
        }
    }

    It 'keeps Network Inventory Web view scoped to read-only fixture evidence' {
        $evidencePath = 'docs/ga-ready/evidence/web-console-network-inventory-view-2026-05-07.md'
        $docsRequiringNetworkView = @(
            $evidencePath,
            'AGENTS.md',
            'follower.md',
            'docs/USER_GUIDE.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/ga-ready/evidence/packaging-operator-backlog-rebaseline-2026-05-07.md',
            'docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md'
        )

        foreach ($relativePath in $docsRequiringNetworkView) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'web-console-network-inventory-view-2026-05-07|Network Inventory'
            $content | Should -Match '/api/v1/network/inventory'
            $content | Should -Match 'read-only'
            $content | Should -Not -Match 'Network Inventory.*public trusted signing.*PASS'
            $content | Should -Not -Match 'Network Inventory.*외부 stable publication.*PASS'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot $evidencePath) -Raw
        $evidence | Should -Match 'Default Switch'
        $evidence | Should -Match 'fixture-ethernet'
        $evidence | Should -Match 'New-VMSwitch'
        $evidence | Should -Match 'Hyper-V switch 생성/삭제'
        $evidence | Should -Match 'service/MSI/trust-store/LAN/update mutation'
        $evidence | Should -Match 'PASS, 27 tests'

        $backlogEvidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/packaging-operator-backlog-rebaseline-2026-05-07.md') -Raw
        $backlogSpec = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md') -Raw
        $backlogEvidence | Should -Not -Match '(?m)^- `/api/v1/network/inventory` 상세 inventory page$'
        $backlogSpec | Should -Not -Match '(?m)^- `/api/v1/network/inventory` 상세 inventory page$'
    }

    It 'keeps network download updater source gate separate from full updater publication' {
        $evidencePath = 'docs/ga-ready/evidence/network-download-update-source-gate-2026-05-07.md'
        $docsRequiringSourceGate = @(
            $evidencePath,
            'docs/ga-ready/evidence/packaging-operator-backlog-rebaseline-2026-05-07.md',
            'docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md'
        )

        foreach ($relativePath in $docsRequiringSourceGate) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'network-download-update-source-gate-2026-05-07'
            $content | Should -Match 'source gate|source-gate'
            $content | Should -Match 'SHA-256|sha256'
            $content | Should -Match 'file/HTTPS|file/https'
            $content | Should -Match 'PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match '외부 stable publication|external stable publication'
            $content | Should -Not -Match 'network download updater.*implemented'
            $content | Should -Not -Match 'full updater.*구현 완료'
            $content | Should -Not -Match 'full updater.*PASS'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot $evidencePath) -Raw
        $evidence | Should -Match 'mode: local-or-https-package-with-sha256'
        $evidence | Should -Match 'update-source-preflight'
        $evidence | Should -Match 'host mutation은 수행하지 않았다'
        $evidence | Should -Match 'Tests Passed: 66'
    }

    It 'keeps update transaction journal diagnostics separate from full transactional rollback' {
        $evidencePath = 'docs/ga-ready/evidence/update-transaction-journal-diagnostics-2026-05-07.md'
        $docsRequiringJournal = @(
            $evidencePath,
            'docs/ga-ready/evidence/packaging-operator-backlog-rebaseline-2026-05-07.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md',
            'packaging/windows-desktop-node/README.md',
            'README.md',
            'follower.md'
        )

        foreach ($relativePath in $docsRequiringJournal) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'update-transaction-journal-diagnostics-2026-05-07'
            $content | Should -Match 'transaction journal|update transaction journal'
            $content | Should -Match 'failed-rolled-back'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match '외부 stable publication|external stable publication'
            $content | Should -Not -Match 'full transactional rollback.*implemented'
            $content | Should -Not -Match 'full transactional rollback.*PASS'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot $evidencePath) -Raw
        $evidence | Should -Match 'mode: single-active-update-journal'
        $evidence | Should -Match 'update-transaction.begin'
        $evidence | Should -Match 'full_transactional_filesystem: false'
        $evidence | Should -Match 'host mutation은 수행하지 않았다'
        $evidence | Should -Match 'Tests Passed: 82'
    }

    It 'keeps release gate, TypeScript Web Console, and .NET Host replacement links discoverable' {
        $draftReadyGatePath = 'docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-draft-pr-ready-gate.md'
        $typescriptBoundaryPath = 'docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-typescript-web-console-boundary-scaffold-design.md'
        $dotNetHostReplacementPath = 'docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md'
        $requiredDocs = @(
            'docs/DEVELOPER_INDEX.md',
            'follower.md',
            'docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match ([regex]::Escape($draftReadyGatePath))
            $content | Should -Match ([regex]::Escape($typescriptBoundaryPath))
            $content | Should -Match ([regex]::Escape($dotNetHostReplacementPath))
        }
    }

    It 'keeps ADR-0005 public distribution expansion as a proposed no-mutation candidate' {
        $requiredDocs = @(
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md',
            'docs/ADR_INDEX.md',
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/public-distribution-operations-expansion-phase1-2026-05-07.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'follower.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'ADR-0005'
            $content | Should -Match 'public-distribution-operations-expansion-candidate'
            $content | Should -Match 'PUBLIC_DISTRIBUTION_GATE_MATRIX'
            $content | Should -Match 'New-PcvPublicDistributionDescriptor\.ps1'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $adrIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ADR_INDEX.md') -Raw
        $adrIndex | Should -Match '제안 중인 ADR 후보'
        $adrIndex | Should -Not -Match 'ADR-0005.*적용 중'

        $matrix = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md') -Raw
        foreach ($pattern in @(
            'public trusted signing',
            'Burn bootstrapper',
            'MSIX',
            'winget manifest',
            'updater catalog publication',
            'public signed update/rollback smoke',
            'Windows Credential Manager transition',
            'default Windows Event Log writer/provider transition',
            'built-in TLS certificate lifecycle'
        )) {
            $matrix | Should -Match $pattern
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-distribution-operations-expansion-phase1-2026-05-07.md') -Raw
        $evidence | Should -Match 'actual_execution: not-run'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps public distribution readiness preflight scoped to preview and manual validation' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/public-distribution-readiness-preflight-2026-05-07.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'public-distribution-readiness-preflight'
            $content | Should -Match 'New-PcvPublicDistributionReadiness\.ps1'
            $content | Should -Match 'winget manifest preview'
            $content | Should -Match 'winget validate'
            $content | Should -Match 'not-submitted'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-distribution-readiness-preflight-2026-05-07.md') -Raw
        $evidence | Should -Match 'actual_execution: not-run'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
        $evidence | Should -Match 'Microsoft Learn'
    }

    It 'keeps updater catalog publication preflight scoped to preview and not-published evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/updater-catalog-publication-preflight-2026-05-07.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'updater-catalog-publication-preflight'
            $content | Should -Match 'New-PcvUpdaterCatalogPublicationPreflight\.ps1'
            $content | Should -Match 'catalog_publication: not-published'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/updater-catalog-publication-preflight-2026-05-07.md') -Raw
        $evidence | Should -Match 'catalog publication preview'
        $evidence | Should -Match 'publication-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps Burn bootstrapper preflight scoped to authoring preview and not-built evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/burn-bootstrapper-preflight-2026-05-07.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'burn-bootstrapper-preflight'
            $content | Should -Match 'New-PcvBurnBootstrapperPreflight\.ps1'
            $content | Should -Match 'burn_bootstrapper: not-built'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/burn-bootstrapper-preflight-2026-05-07.md') -Raw
        $evidence | Should -Match 'WiX Burn authoring preview'
        $evidence | Should -Match 'bundle-build-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps MSIX packaging feasibility preflight scoped to blocked preview evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/msix-packaging-feasibility-preflight-2026-05-07.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'msix-packaging-feasibility-preflight'
            $content | Should -Match 'New-PcvMsixPackagingFeasibilityPreflight\.ps1'
            $content | Should -Match 'msix: feasibility-blocked-by-service-packaging-design'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/msix-packaging-feasibility-preflight-2026-05-07.md') -Raw
        $evidence | Should -Match 'MSIX package manifest preview'
        $evidence | Should -Match 'msix-build-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps winget manifest compliance preflight scoped to offline validation and not-submitted evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/winget-manifest-compliance-preflight-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'winget-manifest-compliance-preflight'
            $content | Should -Match 'New-PcvWingetManifestCompliancePreflight\.ps1'
            $content | Should -Match 'winget_submission: not-submitted'
            $content | Should -Match 'validation_status: offline-compliance-pass'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/winget-manifest-compliance-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'offline compliance preflight'
        $evidence | Should -Match 'winget-cli-validation-not-executed'
        $evidence | Should -Match 'winget-submission-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps public signed update rollback smoke preflight scoped to blocked clean-host evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/public-signed-update-rollback-smoke-preflight-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'public-signed-update-rollback-smoke-preflight'
            $content | Should -Match 'New-PcvPublicSignedUpdateRollbackSmokePreflight\.ps1'
            $content | Should -Match 'public_signed_update_rollback_smoke: blocked-by-public-signing-and-publication'
            $content | Should -Match 'clean_host_smoke_status: not-run'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/public-signed-update-rollback-smoke-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'clean-host smoke plan preview'
        $evidence | Should -Match 'signed-update-rollback-smoke-not-executed'
        $evidence | Should -Match 'host-mutation-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps Windows Credential Manager transition preflight scoped to no-mutation evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/windows-credential-manager-transition-preflight-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'windows-credential-manager-transition-preflight'
            $content | Should -Match 'New-PcvWindowsCredentialManagerTransitionPreflight\.ps1'
            $content | Should -Match 'credential_manager_transition: blocked-by-no-mutation-preflight'
            $content | Should -Match 'credential_manager_mutation: not-run'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/windows-credential-manager-transition-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'Windows Credential Manager transition plan preview'
        $evidence | Should -Match 'token-value-not-read'
        $evidence | Should -Match 'credential-write-not-executed'
        $evidence | Should -Match 'host-mutation-not-executed'
        $evidence | Should -Match 'token_value_observed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps Windows Event Log provider transition preflight scoped to no-mutation evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/windows-event-log-provider-transition-preflight-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'windows-event-log-provider-transition-preflight'
            $content | Should -Match 'New-PcvWindowsEventLogProviderTransitionPreflight\.ps1'
            $content | Should -Match 'event_log_provider_transition: blocked-by-no-mutation-preflight'
            $content | Should -Match 'event_log_provider_mutation: not-run'
            $content | Should -Match 'event_log_write_status: not-run'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/windows-event-log-provider-transition-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'Windows Event Log provider transition plan preview'
        $evidence | Should -Match 'provider-registration-not-executed'
        $evidence | Should -Match 'event-write-not-executed'
        $evidence | Should -Match 'host-mutation-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps built-in TLS certificate lifecycle preflight scoped to no-mutation evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/builtin-tls-certificate-lifecycle-preflight-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'builtin-tls-certificate-lifecycle-preflight'
            $content | Should -Match 'New-PcvBuiltinTlsCertificateLifecyclePreflight\.ps1'
            $content | Should -Match 'tls_certificate_lifecycle: blocked-by-no-mutation-preflight'
            $content | Should -Match 'tls_certificate_mutation: not-run'
            $content | Should -Match 'private_key_material_created: false'
            $content | Should -Match 'trust_store_mutation: not-run'
            $content | Should -Match 'lan_binding_mutation: not-run'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/builtin-tls-certificate-lifecycle-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'Built-in TLS certificate lifecycle plan preview'
        $evidence | Should -Match 'private-key-not-created'
        $evidence | Should -Match 'certificate-import-not-executed'
        $evidence | Should -Match 'host-mutation-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps service token rotation revoke preflight scoped to no-mutation evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/service-token-rotation-revoke-preflight-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'service-token-rotation-revoke-preflight'
            $content | Should -Match 'New-PcvServiceTokenRotationRevokePreflight\.ps1'
            $content | Should -Match 'service_token_rotation_revoke: blocked-by-no-mutation-preflight'
            $content | Should -Match 'service_token_mutation: not-run'
            $content | Should -Match 'service_token_value_observed: false'
            $content | Should -Match 'new_token_value_created: false'
            $content | Should -Match 'service_reload_status: not-run'
            $content | Should -Match 'old_token_rejection_status: not-run'
            $content | Should -Match 'token_rotation_audit_status: not-run'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/service-token-rotation-revoke-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'Service token rotation revoke plan preview'
        $evidence | Should -Match 'token-value-not-read'
        $evidence | Should -Match 'protected-token-write-not-executed'
        $evidence | Should -Match 'service-reload-not-executed'
        $evidence | Should -Match 'host-mutation-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps diagnostic bundle server-side preflight scoped to no-mutation evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/diagnostic-bundle-server-preflight-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'diagnostic-bundle-server-preflight'
            $content | Should -Match 'New-PcvDiagnosticBundleServerPreflight\.ps1'
            $content | Should -Match 'diagnostic_bundle_server_generation: blocked-by-no-mutation-preflight'
            $content | Should -Match 'diagnostic_bundle_api_action: not-run'
            $content | Should -Match 'diagnostic_bundle_archive_created: false'
            $content | Should -Match 'diagnostic_bundle_download_served: false'
            $content | Should -Match 'diagnostic_bundle_redaction_status: not-run'
            $content | Should -Match 'diagnostic_bundle_authz_status: not-run'
            $content | Should -Match 'diagnostic_bundle_retention_status: not-run'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/diagnostic-bundle-server-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'Diagnostic bundle server-side plan preview'
        $evidence | Should -Match 'archive-creation-not-executed'
        $evidence | Should -Match 'download-serving-not-executed'
        $evidence | Should -Match 'redaction-not-executed'
        $evidence | Should -Match 'host-mutation-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps diagnostic bundle server code-level evidence scoped to API action evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ADR_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md',
            'README.md',
            'AGENTS.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'diagnostic-bundle-server-code-level'
            $content | Should -Match 'diagnostic_bundle_server_generation: partial-code-level-api-action'
            $content | Should -Match 'diagnostic_bundle_api_action: code-level-applied'
            $content | Should -Match 'diagnostic_bundle_archive_created: code-level-created'
            $content | Should -Match 'diagnostic_bundle_download_served: code-level-download-served'
            $content | Should -Match 'diagnostic_bundle_redaction_status: code-level-applied'
            $content | Should -Match 'diagnostic_bundle_authz_status: token-required-route-contract'
            $content | Should -Match 'diagnostic_bundle_retention_status: code-level-applied'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/diagnostic-bundle-server-code-level-2026-05-08.md') -Raw
        $evidence | Should -Match 'POST /api/v1/diagnostics/bundles'
        $evidence | Should -Match 'GET /api/v1/diagnostics/bundles/\{bundle_id\}/download'
        $evidence | Should -Match '\.bundle\.json'
        $evidence | Should -Match '\[REDACTED\]'
        $evidence | Should -Match '--diagnostics-root'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'

        $apiTest = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Api.Tests/ApiDiagnosticBundleRequestProcessorTests.cs') -Raw
        $apiTest | Should -Match 'DiagnosticBundleCreateWritesRedactedDownloadableArtifactAndAppliesRetention'
        $apiTest | Should -Match 'MaxBundleCount: 2'
        $apiTest | Should -Match 'super-secret'
        $apiTest | Should -Match 'X-PCV-Diagnostic-Bundle-Id'

        $api = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs') -Raw
        $api | Should -Match 'DesktopNodeDiagnosticBundleOptions'
        $api | Should -Match '/api/v1/diagnostics/bundles'
        $api | Should -Match '/api/v1/diagnostics/bundles/'

        $product = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1') -Raw
        $product | Should -Match '--diagnostics-root'
    }

    It 'keeps diagnostic bundle Host listener code-level evidence separate from installed listener claims' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ADR_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md',
            'README.md',
            'AGENTS.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'diagnostic-bundle-listener-code-level'
            $content | Should -Match 'diagnostic_bundle_host_listener_execution: code-level-host-listener'
            $content | Should -Match 'diagnostic_bundle_installed_listener_execution: not-run'
            $content | Should -Match 'diagnostic_bundle_request_id_propagation: code-level-host-header'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/diagnostic-bundle-listener-code-level-2026-05-08.md') -Raw
        $evidence | Should -Match 'X-PCV-Request-Id'
        $evidence | Should -Match 'X-Request-Id'
        $evidence | Should -Match 'X-PCV-Diagnostic-Bundle-Id'
        $evidence | Should -Match 'listener-diag-create'
        $evidence | Should -Match '\.bundle\.json'
        $evidence | Should -Match '\[REDACTED\]'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'

        $hostTest = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs') -Raw
        $hostTest | Should -Match 'DiagnosticBundleRoutesWorkThroughTokenProtectedHostListener'
        $hostTest | Should -Match 'X-PCV-Request-Id'
        $hostTest | Should -Match 'listener-diag-create'
        $hostTest | Should -Match 'X-PCV-Diagnostic-Bundle-Id'

        $hostSource = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Host/DesktopNodeHostApplication.cs') -Raw
        $hostSource | Should -Match 'ResolveRequestId'
        $hostSource | Should -Match 'X-PCV-Request-Id'
        $hostSource | Should -Match 'X-Request-Id'
    }

    It 'keeps diagnostic bundle product wrapper code-level evidence separate from installed listener claims' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ADR_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md',
            'README.md',
            'AGENTS.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'diagnostic-bundle-product-wrapper-code-level'
            $content | Should -Match 'diagnostic_bundle_product_wrapper_delegation: code-level-product-action-orchestrator'
            $content | Should -Match 'code-level-product-wrapper'
            $content | Should -Match 'diagnostic_bundle_installed_listener_execution: not-run'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/diagnostic-bundle-product-wrapper-code-level-2026-05-08.md') -Raw
        $evidence | Should -Match 'Invoke-PcvDesktopNodeProductAction -Action CollectDiagnostics'
        $evidence | Should -Match 'New-PcvDesktopNodeDiagnosticBundle'
        $evidence | Should -Match 'product-wrapper-delegation-redacted\.json'
        $evidence | Should -Match 'installed_listener_blocker: superseded-by-native-service-action-config-rerun-required'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'

        $productTest = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1') -Raw
        $productTest | Should -Match 'runs CollectDiagnostics through the product action orchestrator'
        $productTest | Should -Match 'product-wrapper-delegation-redacted\.json'
        $productTest | Should -Match 'code-level-product-action-orchestrator'

        $productModule = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1') -Raw
        $productModule | Should -Match 'wrapperDelegationStatus'
        $productModule | Should -Match 'product-wrapper-delegation-redacted\.json'
        $productModule | Should -Match 'code-level-product-action-orchestrator'
    }

    It 'keeps diagnostic bundle native service-action config code-level evidence separate from installed listener pass claims' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ADR_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'packaging/windows-desktop-node/README.md',
            'README.md',
            'AGENTS.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'diagnostic-bundle-native-service-action-config-code-level'
            $content | Should -Match 'diagnostic_bundle_installed_listener_execution: not-run|installed listener execution.*not-run|installed service listener execution.*not-run'
            $content | Should -Match 'elevated-rerun-required-after-native-service-action-config-fix|새 elevated rerun|다음 elevated'
            $content | Should -Match 'public trusted signing|public_trusted_signing: not-claimed'
            $content | Should -Match 'external stable publication|external_stable_publication: not-claimed'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md') -Raw
        $evidence | Should -Match '--diagnostics-root'
        $evidence | Should -Match '--api-token-protected-file'
        $evidence | Should -Match '--route-timeout-seconds 30'
        $evidence | Should -Match '--request-limit-per-minute 120'
        $evidence | Should -Match '--request-burst-limit 20'
        $evidence | Should -Match '--retry-after-seconds 15'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'installed diagnostic bundle listener PASS'

        $hostSource = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Host/DesktopNodeHostServiceAction.cs') -Raw
        $hostSource | Should -Match '--diagnostics-root'
        $hostSource | Should -Match '--route-timeout-seconds'
        $hostSource | Should -Match '--request-limit-per-minute'
        $hostSource | Should -Match '--request-burst-limit'
        $hostSource | Should -Match '--retry-after-seconds'

        $hostTest = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs') -Raw
        $hostTest | Should -Match 'ConfigureInstalledUsesNativeServiceControllerWithoutExternalCommands'
        $hostTest | Should -Match '--diagnostics-root'
        $hostTest | Should -Match '--route-timeout-seconds 30'
    }

    It 'keeps timeout and rate-limit hardening preflight scoped to no-mutation evidence' {
        $requiredDocs = @(
            'docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'timeout-rate-limit-hardening-preflight'
            $content | Should -Match 'New-PcvTimeoutRateLimitHardeningPreflight\.ps1'
            $content | Should -Match 'timeout_rate_limit_hardening: blocked-by-no-mutation-preflight'
            $content | Should -Match 'route_timeout_policy: not-applied'
            $content | Should -Match 'request_limit_policy: not-applied'
            $content | Should -Match 'retry_semantics_status: not-run'
            $content | Should -Match 'ui_api_error_contract_status: not-run'
            $content | Should -Match 'load_test_status: not-run'
            $content | Should -Match 'server_config_mutation: not-run'
            $content | Should -Match 'actual_execution: not-run'
            $content | Should -Match 'host_mutation_performed: false'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/timeout-rate-limit-hardening-preflight-2026-05-08.md') -Raw
        $evidence | Should -Match 'Timeout and rate-limit hardening plan preview'
        $evidence | Should -Match 'middleware-not-enabled'
        $evidence | Should -Match 'load-test-not-executed'
        $evidence | Should -Match 'server-config-not-mutated'
        $evidence | Should -Match 'host-mutation-not-executed'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps timeout and rate-limit hardening code-level evidence scoped to partial route and request-limit implementation' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ADR_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'timeout-rate-limit-hardening-route-timeout-code-level'
            $content | Should -Match 'timeout_rate_limit_hardening: partial-code-level-route-and-request-limit'
            $content | Should -Match 'route_timeout_policy: code-level-applied'
            $content | Should -Match 'request_limit_policy: code-level-applied'
            $content | Should -Match 'retry_semantics_status: retry-after-problem-details-code-level'
            $content | Should -Match 'ui_api_error_contract_status: problem-details-json-code-level'
            $content | Should -Match 'load_test_status: not-run'
            $content | Should -Match 'server_config_mutation: not-run'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/timeout-rate-limit-hardening-route-timeout-code-level-2026-05-08.md') -Raw
        $evidence | Should -Match 'PCV_RATE_LIMIT_EXCEEDED'
        $evidence | Should -Match 'PCV_ROUTE_TIMEOUT'
        $evidence | Should -Match 'Gateway Timeout'
        $evidence | Should -Match 'route_timeout_seconds'
        $evidence | Should -Match 'Retry-After'
        $evidence | Should -Match 'application/problem\+json'
        $evidence | Should -Match 'load test'
        $evidence | Should -Match 'server_config_mutation: not-run'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'
    }

    It 'keeps timeout and rate-limit server config code-level evidence scoped to product plan wiring' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ADR_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'README.md',
            'AGENTS.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'timeout-rate-limit-hardening-server-config-code-level'
            $content | Should -Match 'timeout_rate_limit_hardening: partial-code-level-route-request-and-server-config'
            $content | Should -Match 'route_timeout_policy: code-level-applied'
            $content | Should -Match 'request_limit_policy: code-level-applied'
            $content | Should -Match 'retry_semantics_status: retry-after-problem-details-code-level'
            $content | Should -Match 'ui_api_error_contract_status: problem-details-json-code-level'
            $content | Should -Match 'load_test_status: not-run'
            $content | Should -Match 'server_config_mutation: code-level-product-and-native-service-plan-applied'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/timeout-rate-limit-hardening-server-config-code-level-2026-05-08.md') -Raw
        $evidence | Should -Match '--route-timeout-seconds 30'
        $evidence | Should -Match '--request-limit-per-minute 120'
        $evidence | Should -Match '--request-burst-limit 20'
        $evidence | Should -Match '--retry-after-seconds 15'
        $evidence | Should -Match 'service\.hardening'
        $evidence | Should -Match 'installed service mutation'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'

        $policy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $policy | Should -Match 'timeout-rate-limit-hardening-server-config-code-level-2026-05-08\.md'
        $policy | Should -Match 'packaging product plan Pester'
    }

    It 'keeps timeout and rate-limit load test code-level evidence scoped to in-process request processor evidence' {
        $requiredDocs = @(
            'docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md',
            'docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md',
            'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md',
            'docs/DEVELOPER_INDEX.md',
            'docs/ADR_INDEX.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'packaging/windows-desktop-node/README.md',
            'README.md',
            'AGENTS.md',
            'follower.md',
            'docs/adr/0005-public-distribution-operations-expansion-candidate.md'
        )

        foreach ($relativePath in $requiredDocs) {
            $content = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw

            $content | Should -Match 'timeout-rate-limit-hardening-load-test-code-level'
            $content | Should -Match 'timeout_rate_limit_hardening: partial-code-level-route-request-server-config-and-load'
            $content | Should -Match 'route_timeout_policy: code-level-applied'
            $content | Should -Match 'request_limit_policy: code-level-applied'
            $content | Should -Match 'retry_semantics_status: retry-after-problem-details-code-level'
            $content | Should -Match 'ui_api_error_contract_status: problem-details-json-code-level'
            $content | Should -Match 'load_test_status: code-level-inprocess-pass'
            $content | Should -Match 'server_config_mutation: code-level-product-and-native-service-plan-applied'
            $content | Should -Match 'public trusted signing'
            $content | Should -Match 'external stable publication'
        }

        $evidence = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ga-ready/evidence/timeout-rate-limit-hardening-load-test-code-level-2026-05-08.md') -Raw
        $evidence | Should -Match '64'
        $evidence | Should -Match 'HTTP 200'
        $evidence | Should -Match '20'
        $evidence | Should -Match 'HTTP 429'
        $evidence | Should -Match '44'
        $evidence | Should -Match 'PCV_RATE_LIMIT_EXCEEDED'
        $evidence | Should -Match 'retry_after_seconds=9'
        $evidence | Should -Match 'installed listener'
        $evidence | Should -Match 'external load generator'
        $evidence | Should -Match 'host_mutation_performed: false'
        $evidence | Should -Match 'public_trusted_signing: not-claimed'
        $evidence | Should -Match 'external_stable_publication: not-claimed'

        $apiTests = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs') -Raw
        $apiTests | Should -Match 'RequestRateLimitInProcessLoadKeepsSuccessBudgetAndProblemDetailsStable'
        $apiTests | Should -Match 'totalRequests: 64'
        $apiTests | Should -Match 'requestLimitPerMinute: 16'
        $apiTests | Should -Match 'burstLimit: 4'
        $apiTests | Should -Match 'retryAfterSeconds: 9'

        $policy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $policy | Should -Match 'timeout-rate-limit-hardening-load-test-code-level-2026-05-08\.md'
        $policy | Should -Match 'Installed listener load'
    }
}
