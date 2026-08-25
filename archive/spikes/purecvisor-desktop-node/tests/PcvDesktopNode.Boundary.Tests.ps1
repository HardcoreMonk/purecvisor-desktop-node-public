Set-StrictMode -Version Latest

Describe 'Desktop Node runtime promotion boundary' {
    BeforeAll {
        $script:Root = Resolve-Path (Join-Path $PSScriptRoot '..')
        $script:RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')
    }

    It 'records the Phase 11 keep-spike decision in the root README' {
        $readmePath = Join-Path $script:Root 'README.md'
        Test-Path -LiteralPath $readmePath | Should -BeTrue

        $content = Get-Content -LiteralPath $readmePath -Raw
        $content | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike'
        $content | Should -Match 'archive/spikes/purecvisor-desktop-node/\*\*'
        $content | Should -Match 'Linux `purecvisorsd`'
        $content | Should -Match 'Single Edge'
    }

    It 'keeps the Phase 11 design and plan explicit about promotion gates' {
        $specPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md'
        $planPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision.md'
        Test-Path -LiteralPath $specPath | Should -BeTrue
        Test-Path -LiteralPath $planPath | Should -BeTrue

        $spec = Get-Content -LiteralPath $specPath -Raw
        $plan = Get-Content -LiteralPath $planPath -Raw
        $spec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike'
        $spec | Should -Match '서명된 installer'
        $spec | Should -Match '업데이트'
        $spec | Should -Match '롤백'
        $spec | Should -Match '로그 수집'
        $spec | Should -Match '서비스 복구'
        $plan | Should -Match 'Desktop Node root boundary suite'
    }

    It 'separates the Desktop Node gate from the Single Edge release gate' {
        $boundaryPath = Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md'
        $policyPath = Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'

        $boundary = Get-Content -LiteralPath $boundaryPath -Raw
        $policy = Get-Content -LiteralPath $policyPath -Raw

        $boundary | Should -Match 'Phase 11'
        $boundary | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $boundary | Should -Match 'DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service'
        $policy | Should -Match 'Desktop Node runtime promotion decision'
        $policy | Should -Match 'component/archive baseline 소유권'
        $policy | Should -Match 'Linux Single Edge 릴리스 게이트와 Desktop Node 내부 전용 제품 런타임 판단은 분리'
    }

    It 'documents the Phase 12 service-first product wrapper boundary' {
        $phase12Spec = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime-design.md') -Raw
        $phase12Plan = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime.md') -Raw
        $rootReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'archive/spikes/purecvisor-desktop-node/README.md') -Raw
        $releaseBoundary = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw

        $phase12Spec | Should -Match 'DESKTOP_NODE_PHASE12_RUNTIME_DECISION: service-first-product-wrapper'
        $phase12Plan | Should -Match 'packaging/windows-desktop-node'
        $rootReadme | Should -Match 'Phase 12'
        $rootReadme | Should -Match 'service-first'
        $releaseBoundary | Should -Match 'Phase 12'
        $releaseBoundary | Should -Match 'Service-first'
    }

    It 'documents the Phase 13 WinSW service wrapper boundary' {
        $phase13Spec = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper-design.md') -Raw
        $phase13Plan = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $verificationPolicy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $rootReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'archive/spikes/purecvisor-desktop-node/README.md') -Raw

        $phase13Spec | Should -Match 'DESKTOP_NODE_PHASE13_SERVICE_DECISION: winsw-service-wrapper'
        $phase13Plan | Should -Match 'WinSW'
        $phase13Plan | Should -Match 'AllowUnauthenticatedStatic'
        $developerIndex | Should -Match 'Phase 13'
        $verificationPolicy | Should -Match 'Desktop Node Phase 13 WinSW product wrapper 변경'
        $rootReadme | Should -Match 'Phase 13'
        $rootReadme | Should -Match 'WinSW'
    }

    It 'documents the Phase 19 evidence-first keep-spike redecision' {
        $phase19SpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md'
        Test-Path -LiteralPath $phase19SpecPath | Should -BeTrue

        $phase19Spec = Get-Content -LiteralPath $phase19SpecPath -Raw
        $rootReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'archive/spikes/purecvisor-desktop-node/README.md') -Raw
        $releaseBoundary = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $verificationPolicy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw

        $phase19Spec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike'
        $phase19Spec | Should -Match 'DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike'
        $phase19Spec | Should -Match 'Signed release build evidence'
        $phase19Spec | Should -Match 'Elevated MSI lifecycle smoke'
        $phase19Spec | Should -Match 'Hyper-V lifecycle integration evidence'
        $phase19Spec | Should -Match 'Release/version policy'
        $phase19Spec | Should -Match '장기 운영 로그 정책 evidence'

        $rootReadme | Should -Match 'Phase 19'
        $rootReadme | Should -Match 'evidence-first-keep-spike'
        $releaseBoundary | Should -Match 'Phase 19'
        $releaseBoundary | Should -Match 'ADR-0004가 내부 전용 서비스 범위에서 이를 대체'
        $developerIndex | Should -Match 'Phase 19 제품 승격 재판정'
        $verificationPolicy | Should -Match 'Desktop Node Phase 19 제품 승격 재판정'
    }

    It 'documents the Phase 24 Local API job runtime boundary candidate' {
        $phase24SpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md'
        $phase24PlanPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary.md'
        Test-Path -LiteralPath $phase24SpecPath | Should -BeTrue
        Test-Path -LiteralPath $phase24PlanPath | Should -BeTrue

        $phase24Spec = Get-Content -LiteralPath $phase24SpecPath -Raw
        $phase24Plan = Get-Content -LiteralPath $phase24PlanPath -Raw
        $rootReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'README.md') -Raw
        $apiReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'archive/spikes/purecvisor-desktop-node/api/README.md') -Raw
        $hypervReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'archive/spikes/purecvisor-desktop-node/hyperv/README.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $verificationPolicy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $releaseBoundary = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw
        $roadmap = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md') -Raw

        $phase24Spec | Should -Match 'DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first'
        $phase24Spec | Should -Match 'GET /api/v1/runtime/policy'
        $phase24Spec | Should -Match 'not-planned-unless-runtime-boundary-deepens'
        $phase24Spec | Should -Match 'windows-hyperv-orchestration-not-dataplane'
        $phase24Spec | Should -Match 'plan-contract-injectable-runner-diagnostics'
        $phase24Spec | Should -Match 'network.inventory'
        $phase24Spec | Should -Match 'C\+\+23 native runtime 구현'
        $phase24Plan | Should -Match 'job_runtime` contract'
        $phase24Plan | Should -Match 'network.inventory'
        $phase24Plan | Should -Match 'Expected 1, but got \$null'

        $rootReadme | Should -Match 'DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE'
        $apiReadme | Should -Match 'Phase 24 상태'
        $apiReadme | Should -Match 'job_runtime'
        $apiReadme | Should -Match 'GET` \| `/api/v1/network/inventory`'
        $hypervReadme | Should -Match 'network.inventory'
        $developerIndex | Should -Match 'Phase 24 Local API job runtime boundary 후보'
        $verificationPolicy | Should -Match 'Desktop Node Phase 24 Local API job runtime boundary 변경'
        $verificationPolicy | Should -Match 'Hyper-V non-integration suite'
        $releaseBoundary | Should -Match '공개 release boundary, GA 승격 판단'
        $roadmap | Should -Match 'Phase 24'
        $roadmap | Should -Match 'Local API job runtime public boundary'
    }

    It 'documents the Phase 25 .NET and TypeScript transition plus service host replacement without promoting GA' {
        $phase25SpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition-design.md'
        $phase25PlanPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase25-dotnet-typescript-transition.md'
        $hostReplacementSpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement-design.md'
        $hostReplacementPlanPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-05-01-purecvisor-desktop-node-dotnet-windows-service-host-replacement.md'
        $dataRootRemovePlanPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-05-04-purecvisor-desktop-node-data-root-remove-handoff.md'
        Test-Path -LiteralPath $phase25SpecPath | Should -BeTrue
        Test-Path -LiteralPath $phase25PlanPath | Should -BeTrue
        Test-Path -LiteralPath $hostReplacementSpecPath | Should -BeTrue
        Test-Path -LiteralPath $hostReplacementPlanPath | Should -BeTrue
        Test-Path -LiteralPath $dataRootRemovePlanPath | Should -BeTrue

        $phase25Spec = Get-Content -LiteralPath $phase25SpecPath -Raw
        $phase25Plan = Get-Content -LiteralPath $phase25PlanPath -Raw
        $hostReplacementSpec = Get-Content -LiteralPath $hostReplacementSpecPath -Raw
        $hostReplacementPlan = Get-Content -LiteralPath $hostReplacementPlanPath -Raw
        $dataRootRemovePlan = Get-Content -LiteralPath $dataRootRemovePlanPath -Raw
        $phase25SolutionPath = Join-Path $script:RepoRoot 'src/DesktopNode.sln'
        $phase25ContractsPath = Join-Path $script:RepoRoot 'src/DesktopNode.Contracts/RuntimePolicy.cs'
        $phase25TestsPath = Join-Path $script:RepoRoot 'src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs'
        $phase25HostPath = Join-Path $script:RepoRoot 'src/DesktopNode.Host/DesktopNode.Host.csproj'
        $rootReadme = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'README.md') -Raw
        $agents = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'AGENTS.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $verificationPolicy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $releaseBoundary = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw
        $roadmap = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md') -Raw
        $follower = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'follower.md') -Raw
        Test-Path -LiteralPath $phase25SolutionPath | Should -BeTrue
        Test-Path -LiteralPath $phase25ContractsPath | Should -BeTrue
        Test-Path -LiteralPath $phase25TestsPath | Should -BeTrue
        Test-Path -LiteralPath $phase25HostPath | Should -BeTrue

        $phase25Spec | Should -Match 'DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first'
        $phase25Spec | Should -Match '\.NET contract mirror'
        $phase25Spec | Should -Match 'TypeScript Web Console'
        $phase25Spec | Should -Match 'PowerShell Windows adapter'
        $phase25Spec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike'
        $phase25Spec | Should -Match 'DesktopNode.Host.exe'
        $phase25Spec | Should -Match 'service-host-default'
        $phase25Spec | Should -Match 'C\+\+23 native runtime 구현'
        $hostReplacementSpec | Should -Match 'DesktopNode.Host.exe'
        $hostReplacementSpec | Should -Match 'listener owner'
        $hostReplacementPlan | Should -Match 'artifacts/dotnet-host-admin-smoke-20260501-213444'
        $dataRootRemovePlan | Should -Match 'remove-installed --remove-data'
        $dataRootRemovePlan | Should -Match 'data-root-remove --remove-data'
        $dataRootRemovePlan | Should -Match 'Installed destructive service/data-root lifecycle smoke'
        $phase25Plan | Should -Match 'src/DesktopNode.sln'
        $phase25Plan | Should -Match 'dotnet test src/DesktopNode.sln'
        $phase25Plan | Should -Match '첫 .NET contract mirror slice를 실행했다'
        $phase25Plan | Should -Match 'RuntimePolicyDeclaresDotNetAsDefaultServiceHost'

        $rootReadme | Should -Match 'DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE'
        $rootReadme | Should -Match 'DESKTOP_NODE_PHASE25_SERVICE_HOST_REPLACEMENT'
        $rootReadme | Should -Match 'src/DesktopNode.Contracts'
        $agents | Should -Match 'Phase 25 .NET/TypeScript 전환 후보'
        $agents | Should -Match '.NET Windows Service Host replacement'
        $agents | Should -Match '2026-05-04-purecvisor-desktop-node-data-root-remove-handoff'
        $developerIndex | Should -Match 'Phase 25 .NET/TypeScript 전환 후보 확인'
        $developerIndex | Should -Match '.NET Windows Service Host replacement 확인'
        $developerIndex | Should -Match '2026-05-04-purecvisor-desktop-node-data-root-remove-handoff'
        $verificationPolicy | Should -Match 'Desktop Node Phase 25 .NET/TypeScript 전환 변경'
        $releaseBoundary | Should -Match 'Phase 25 후보'
        $releaseBoundary | Should -Match '공개 release boundary, GA 승격 판단'
        $releaseBoundary | Should -Match 'DesktopNode.Host.exe'
        $releaseBoundary | Should -Match 'data-root-remove --remove-data'
        $roadmap | Should -Match 'Phase 25'
        $roadmap | Should -Match '.NET service host replacement'
        $follower | Should -Match '.NET/TypeScript mixed runtime transition'
        $follower | Should -Match 'dotnet-windows-service-host-default-with-keep-spike'
        $follower | Should -Match 'data-root-remove'
    }

    It 'documents the Desktop Node ADR index and current decision source' {
        $adrIndexPath = Join-Path $script:RepoRoot 'docs/ADR_INDEX.md'
        $adrTemplatePath = Join-Path $script:RepoRoot 'docs/adr/0000-template.md'
        $adr0001Path = Join-Path $script:RepoRoot 'docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md'
        $adr0002Path = Join-Path $script:RepoRoot 'docs/adr/0002-release-version-policy.md'
        $adr0003Path = Join-Path $script:RepoRoot 'docs/adr/0003-internal-trusted-signing-policy.md'
        $adr0004Path = Join-Path $script:RepoRoot 'docs/adr/0004-ga-ready-product-runtime-candidate.md'

        Test-Path -LiteralPath $adrIndexPath | Should -BeTrue
        Test-Path -LiteralPath $adrTemplatePath | Should -BeTrue
        Test-Path -LiteralPath $adr0001Path | Should -BeTrue
        Test-Path -LiteralPath $adr0002Path | Should -BeTrue
        Test-Path -LiteralPath $adr0003Path | Should -BeTrue
        Test-Path -LiteralPath $adr0004Path | Should -BeTrue

        $adrIndex = Get-Content -LiteralPath $adrIndexPath -Raw
        $adr0001 = Get-Content -LiteralPath $adr0001Path -Raw
        $adr0002 = Get-Content -LiteralPath $adr0002Path -Raw
        $adr0003 = Get-Content -LiteralPath $adr0003Path -Raw
        $adr0004 = Get-Content -LiteralPath $adr0004Path -Raw
        $agents = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'AGENTS.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $releaseBoundary = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/PUBLIC_RELEASE_BOUNDARY.md') -Raw

        $adrIndex | Should -Match 'DESKTOP_NODE_DOCS_DECISION: lightweight-adr-index'
        $adrIndex | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $adrIndex | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adrIndex | Should -Match 'DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service'
        $adrIndex | Should -Match 'DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike'
        $adrIndex | Should -Match 'DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike'
        $adrIndex | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $adrIndex | Should -Match '0001-standalone-windows-repo-and-evidence-first-keep-spike'
        $adrIndex | Should -Match '0002-release-version-policy'
        $adrIndex | Should -Match '0003-internal-trusted-signing-policy'
        $adrIndex | Should -Match '0004-ga-ready-product-runtime-candidate'

        $adr0001 | Should -Match '상태: 적용 중'
        $adr0001 | Should -Match 'Linux `purecvisor-single`'
        $adr0001 | Should -Match '제품 런타임 승격 판단은 ADR-0004가 대체'
        $adr0001 | Should -Match 'Public trusted/stable signing evidence'
        $adr0001 | Should -Match 'Elevated MSI lifecycle 전체 exit 0 smoke'
        $adr0001 | Should -Match 'Hyper-V lifecycle integration evidence'

        $adr0002 | Should -Match '상태: 적용 중'
        $adr0002 | Should -Match 'DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike'
        $adr0002 | Should -Match 'product.release_channel'
        $adr0002 | Should -Match 'unsigned `AllowUnsignedDev` build를 거부'
        $adr0002 | Should -Match '외부 stable 공개 배포'
        $adr0002 | Should -Match 'DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service'

        $adr0003 | Should -Match '상태: 적용 중'
        $adr0003 | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $adr0003 | Should -Match 'InternalEnterprise'
        $adr0003 | Should -Match 'public trusted signing evidence'
        $adr0003 | Should -Match 'DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service'

        $adr0004 | Should -Match '상태: 적용 중'
        $adr0004 | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adr0004 | Should -Match 'DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service'
        $adr0004 | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime'

        $agents | Should -Match 'docs/ADR_INDEX.md'
        $agents | Should -Match 'docs/adr/'
        $developerIndex | Should -Match 'docs/ADR_INDEX.md'
        $releaseBoundary | Should -Match 'docs/ADR_INDEX.md'
    }

    It 'documents the GA-ready product runtime current decision supporting docs' {
        $adrCandidatePath = Join-Path $script:RepoRoot 'docs/adr/0004-ga-ready-product-runtime-candidate.md'
        $redesignSpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md'
        $routeMatrixPath = Join-Path $script:RepoRoot 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'
        $repoMigrationPath = Join-Path $script:RepoRoot 'docs/ga-ready/REPO_MIGRATION_MAP.md'
        $verificationOwnershipPath = Join-Path $script:RepoRoot 'docs/ga-ready/VERIFICATION_OWNERSHIP.md'

        Test-Path -LiteralPath $adrCandidatePath | Should -BeTrue
        Test-Path -LiteralPath $redesignSpecPath | Should -BeTrue
        Test-Path -LiteralPath $routeMatrixPath | Should -BeTrue
        Test-Path -LiteralPath $repoMigrationPath | Should -BeTrue
        Test-Path -LiteralPath $verificationOwnershipPath | Should -BeTrue

        $adrCandidate = Get-Content -LiteralPath $adrCandidatePath -Raw
        $redesignSpec = Get-Content -LiteralPath $redesignSpecPath -Raw
        $routeMatrix = Get-Content -LiteralPath $routeMatrixPath -Raw
        $repoMigration = Get-Content -LiteralPath $repoMigrationPath -Raw
        $verificationOwnership = Get-Content -LiteralPath $verificationOwnershipPath -Raw
        $adrIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/ADR_INDEX.md') -Raw
        $developerIndex = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md') -Raw
        $guide = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/GUIDE.md') -Raw
        $roadmap = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md') -Raw
        $follower = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'follower.md') -Raw

        $adrCandidate | Should -Match '상태: 적용 중'
        $adrCandidate | Should -Match '대체 범위: ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adrCandidate | Should -Match 'DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service'
        $adrCandidate | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime'
        $adrCandidate | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE'
        $adrCandidate | Should -Match '현재 적용 목표 상태는 PowerShell-free product ops/runtime'
        $adrCandidate | Should -Match '현재 적용 결정이 된다'
        $adrCandidate | Should -Match '## Aggregate GA-ready Decision Gate'
        $adrCandidate | Should -Match 'ADR-0004를 current decision으로 승격하기 전'
        $adrCandidate | Should -Match 'GA 범위의 `current-route`와 `product-operation` row'
        $adrCandidate | Should -Match '제품 runtime/request path에는 PowerShell helper가 없어야 한다'
        $adrCandidate | Should -Match '활성 제품 경로에는 `spikes/\*\*`가 없어야 한다'
        $adrCandidate | Should -Match 'repo migration preflight evidence'
        $adrCandidate | Should -Match 'verification ownership replacement evidence'
        $adrCandidate | Should -Match 'Evidence Freshness Rule'
        $adrCandidate | Should -Match 'stale evidence'
        $adrCandidate | Should -Match 'release_gate = release-approval-required'
        $adrCandidate | Should -Match '별도 release approval 전에는 실행하지 않는다'
        $adrCandidate | Should -Match '## Aggregate Gate Closure Report'
        $adrCandidate | Should -Match 'aggregate-gate-closure-2026-05-05\.md'
        $adrCandidate | Should -Match 'aggregate_gate_status = closed'
        $adrCandidate | Should -Match 'public_trusted_signing: excluded'
        $adrCandidate | Should -Match 'external_stable_publication: not-claimed'
        $adrCandidate | Should -Match '## ADR-0001 Replacement Scope'
        $adrCandidate | Should -Match '대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단'
        $adrCandidate | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $adrCandidate | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $adrCandidate | Should -Match '## Current Decision Promotion Procedure'
        $adrCandidate | Should -Match '이 ADR 적용 diff'
        $adrCandidate | Should -Match 'ADR-0004 상태를 `적용 중`'
        $adrCandidate | Should -Match '제안 중인 ADR 후보 섹션에서 제거'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 ADR-0004 하나'

        $redesignSpec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $redesignSpec | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime'
        $redesignSpec | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE'
        $redesignSpec | Should -Match 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'
        $redesignSpec | Should -Match '상세 route contract'
        $redesignSpec | Should -Not -Match '\| Route/Operation \|'
        $redesignSpec | Should -Not -Match 'DELETE /api/v1/vms/\{id\}/checkpoints/\{name\}'

        $routeMatrix | Should -Match '## Field Schema'
        $routeMatrix | Should -Match '## Current Owner Invariants'
        $routeMatrix | Should -Match '## Current Owner Resolution Rule'
        $routeMatrix | Should -Match '## Mixed History Resolution Rule'
        $routeMatrix | Should -Match '`mixed-history`은 service product operation row에만 허용한다'
        $routeMatrix | Should -Match 'actual current code path와 evidence source'
        $routeMatrix | Should -Match '`mixed-history` 자체를 promotion evidence 또는 target owner로 간주하지 않는다'
        $routeMatrix | Should -Match '## Target Owner Invariants'
        $routeMatrix | Should -Match '## Implementation Basis Invariants'
        $routeMatrix | Should -Match '## Job Runtime Risk Inheritance Rule'
        $routeMatrix | Should -Match '## Job Route Parameter Rule'
        $routeMatrix | Should -Match 'Job route path parameter는 `job_id`로 통일한다'
        $routeMatrix | Should -Match '`id`와 `jobId`는 code variable 또는 internal compatibility name'
        $routeMatrix | Should -Match '## VM Route Parameter Rule'
        $routeMatrix | Should -Match 'VM route path parameter는 기존 served API 계약인 `id`를 유지한다'
        $routeMatrix | Should -Match 'VM route `id`는 VM `id` 또는 `name` lookup key'
        $routeMatrix | Should -Match '`vmId`는 code variable 또는 internal compatibility name'
        $routeMatrix | Should -Match '`vm_id`로 바꾸는 것은 이 alignment slice 범위가 아니다'
        $routeMatrix | Should -Match '## Checkpoint Route Parameter Rule'
        $routeMatrix | Should -Match 'route_surface'
        $routeMatrix | Should -Match 'current-route'
        $routeMatrix | Should -Match 'future-route'
        $routeMatrix | Should -Match 'product-operation'
        $routeMatrix | Should -Match 'not-implemented'
        $routeMatrix | Should -Match 'route_surface = future-route'
        $routeMatrix | Should -Match 'tier1-read-only'
        $routeMatrix | Should -Match 'tier2-reversible-mutation'
        $routeMatrix | Should -Match 'tier3-destructive-or-persistent'
        $routeMatrix | Should -Match 'transition-helper'
        $routeMatrix | Should -Match '## State Invariants'
        $routeMatrix | Should -Match '## Route Surface Invariants'
        $routeMatrix | Should -Match '## Served Route Scope Rule'
        $routeMatrix | Should -Match 'side-by-side contract-only route 후보'
        $routeMatrix | Should -Match '`GET /api/v1/jobs`는 현재 served route'
        $routeMatrix | Should -Match 'Job runtime read surface는 `GET /api/v1/jobs`와 `GET /api/v1/jobs/\{job_id\}` row'
        $routeMatrix | Should -Match 'Contract mirror aggregate route 후보인 `POST /api/v1/vms/\{vmId\}/lifecycle/\{action\}`'
        $routeMatrix | Should -Match 'VM lifecycle served surface는 현재 `POST /api/v1/vms/\{id\}/start`, `shutdown`, `poweroff`, `restart`, `DELETE /api/v1/vms/\{id\}` 개별 row'
        $routeMatrix | Should -Match '\| `GET /api/v1/jobs` \|'
        $routeMatrix | Should -Match '## Future Route Execution Guard'
        $routeMatrix | Should -Match 'Phase 26 alignment slice에서 구현하거나 실제 Local API route/product operation으로 등록하지 않는다'
        $routeMatrix | Should -Match '별도 implementation plan'
        $routeMatrix | Should -Match 'route contract'
        $routeMatrix | Should -Match 'not-found/idempotency contract'
        $routeMatrix | Should -Match 'destructive cleanup proof'
        $routeMatrix | Should -Match 'explicit admin opt-in evidence'
        $routeMatrix | Should -Match '## Native-First Helper Fallback Rule'
        $routeMatrix | Should -Match 'current_owner = dotnet-native'
        $routeMatrix | Should -Match 'product request path에서 PowerShell helper fallback을 사용하지 않는다'
        $routeMatrix | Should -Match 'promotion_state = current-native'
        $routeMatrix | Should -Match 'promotion_state'
        $routeMatrix | Should -Match 'current-native'
        $routeMatrix | Should -Match 'ga-ready-candidate'
        $routeMatrix | Should -Match 'promotion_state = transition-helper'
        $routeMatrix | Should -Match 'fallback_policy = transition-helper'
        $routeMatrix | Should -Match 'promotion_state = blocked'
        $routeMatrix | Should -Match 'fallback_policy = blocked'
        $routeMatrix | Should -Match 'risk_tier = tier1-read-only'
        $routeMatrix | Should -Match 'admin_smoke_required = installed-non-mutating'
        $routeMatrix | Should -Match 'risk_tier = tier2-reversible-mutation'
        $routeMatrix | Should -Match 'risk_tier = tier3-destructive-or-persistent'
        $routeMatrix | Should -Match 'admin_smoke_required = explicit-admin-opt-in'
        $routeMatrix | Should -Match 'release_gate'
        $routeMatrix | Should -Match 'release-approval-required'
        $routeMatrix | Should -Match 'release_gate = release-approval-required'
        $routeMatrix | Should -Match 'Release-gated pre-release evidence boundary'
        $routeMatrix | Should -Match 'ADR-0004 승격 전에 `blocked`를 해소할 수 있지만'
        $routeMatrix | Should -Match 'release execution이 아니라 pre-release evidence'
        $routeMatrix | Should -Match 'package/trust contract validation'
        $routeMatrix | Should -Match 'manifest/hash/provenance validation'
        $routeMatrix | Should -Match 'dry-run planning'
        $routeMatrix | Should -Match 'non-mutating ownership checks'
        $routeMatrix | Should -Match 'rollback plan validation'
        $routeMatrix | Should -Match 'redaction evidence'
        $routeMatrix | Should -Match 'no-auto-reboot evidence'
        $routeMatrix | Should -Match 'stable publication'
        $routeMatrix | Should -Match 'public trusted signing execution'
        $routeMatrix | Should -Match 'certificate store write/delete'
        $routeMatrix | Should -Match 'external update/rollback activation'
        $routeMatrix | Should -Match 'ga-ready-candidate'
        $routeMatrix | Should -Match 'execution-approved가 될 수 없다'
        $routeMatrix | Should -Match '## Aggregate GA-ready Decision Gate'
        $routeMatrix | Should -Match 'ADR-0004를 current decision으로 승격하기 전'
        $routeMatrix | Should -Match 'GA 범위의 `current-route`와 `product-operation` row'
        $routeMatrix | Should -Match 'promotion_state = transition-helper'
        $routeMatrix | Should -Match 'promotion_state = blocked'
        $routeMatrix | Should -Match '0개'
        $routeMatrix | Should -Match '`future-route` row는 GA 범위 제외 사유'
        $routeMatrix | Should -Match '별도 implementation plan requirement'
        $routeMatrix | Should -Match '제품 runtime/request path에는 PowerShell helper가 없어야 한다'
        $routeMatrix | Should -Match '활성 제품 경로에는 `spikes/\*\*`가 없어야 한다'
        $routeMatrix | Should -Match 'repo migration preflight evidence'
        $routeMatrix | Should -Match 'verification ownership replacement evidence'
        $routeMatrix | Should -Match '## PowerShell-Free Product Path Closure Rule'
        $routeMatrix | Should -Match 'product runtime/request/admin execution path'
        $routeMatrix | Should -Match 'PowerShell helper를 사용하지 않아야'
        $routeMatrix | Should -Match 'current_owner = powershell-helper'
        $routeMatrix | Should -Match 'current_owner = dotnet-request-processor-powershell-helper'
        $routeMatrix | Should -Match '다시 등장하면 target owner evidence가 있더라도 aggregate GA-ready gate closure로 계산할 수 없다'
        $routeMatrix | Should -Match 'fallback_policy = transition-helper'
        $routeMatrix | Should -Match 'helper fallback 제거 evidence'
        $routeMatrix | Should -Match 'fallback_policy = test-only'
        $routeMatrix | Should -Match 'product execution path fallback으로 사용할 수 없다'
        $routeMatrix | Should -Match '## Active Product Path Classification Rule'
        $routeMatrix | Should -Match 'runtime/service/API/CLI/Web Console execution'
        $routeMatrix | Should -Match 'packaging input'
        $routeMatrix | Should -Match 'installer input'
        $routeMatrix | Should -Match 'static asset source'
        $routeMatrix | Should -Match 'generated parity manifest'
        $routeMatrix | Should -Match 'required verification command'
        $routeMatrix | Should -Match 'CI/local verification command'
        $routeMatrix | Should -Match 'developer command documentation'
        $routeMatrix | Should -Match 'active product path로 간주'
        $routeMatrix | Should -Match 'archive/spikes/\*\*'
        $routeMatrix | Should -Match 'historical/read-only baseline intent'
        $routeMatrix | Should -Match 'product execution, packaging, required verification source로 사용할 수 없다'
        $routeMatrix | Should -Match 'docs command update evidence'
        $routeMatrix | Should -Match '## Aggregate Gate Closure Report Contract'
        $routeMatrix | Should -Match 'docs/ga-ready/evidence/aggregate-gate-closure-<YYYY-MM-DD>\.md'
        $routeMatrix | Should -Match 'Closure report는 Markdown record'
        $routeMatrix | Should -Match 'machine-readable JSON은 만들지 않는다'
        $routeMatrix | Should -Match 'ga_scope_current_route_count'
        $routeMatrix | Should -Match 'ga_scope_product_operation_count'
        $routeMatrix | Should -Match 'future_route_exclusion_count'
        $routeMatrix | Should -Match 'transition_helper_count'
        $routeMatrix | Should -Match 'blocked_count'
        $routeMatrix | Should -Match 'powershell_current_owner_count'
        $routeMatrix | Should -Match 'powershell_fallback_count'
        $routeMatrix | Should -Match 'active_spikes_path_count'
        $routeMatrix | Should -Match 'repo_migration_preflight_status'
        $routeMatrix | Should -Match 'docs_command_update_status'
        $routeMatrix | Should -Match 'verification_ownership_replacement_status'
        $routeMatrix | Should -Match 'tier2_admin_evidence_status'
        $routeMatrix | Should -Match 'tier3_admin_evidence_status'
        $routeMatrix | Should -Match 'release_gated_prerelease_evidence_status'
        $routeMatrix | Should -Match 'lan_gated_preapproval_evidence_status'
        $routeMatrix | Should -Match 'stale_evidence_count'
        $routeMatrix | Should -Match 'waived_evidence_count'
        $routeMatrix | Should -Match 'waiver_only_gate_satisfaction_count'
        $routeMatrix | Should -Match 'aggregate_gate_status'
        $routeMatrix | Should -Match '`open`, `closed`, `blocked`'
        $routeMatrix | Should -Match 'required status field가 모두 `pass`'
        $routeMatrix | Should -Match '그 외 미실행 또는 미완료 상태는 `aggregate_gate_status = open`'
        $routeMatrix | Should -Match '## ADR Promotion Procedure Rule'
        $routeMatrix | Should -Match 'ADR-0004는 `aggregate_gate_status = closed` closure report를 근거로 current decision으로 적용됐다'
        $routeMatrix | Should -Match 'Public trusted signing, external stable publication'
        $routeMatrix | Should -Match '현재 적용 중인 ADR 표'
        $routeMatrix | Should -Match '제안 중인 ADR 후보 섹션'
        $routeMatrix | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 current source는 ADR-0004 하나'
        $routeMatrix | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $routeMatrix | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $routeMatrix | Should -Match '`tier2-reversible-mutation`과 `tier3-destructive-or-persistent` row'
        $routeMatrix | Should -Match 'explicit admin opt-in evidence'
        $routeMatrix | Should -Match '## GA Scope Classification Rule'
        $routeMatrix | Should -Match '`route_surface = current-route`와 `route_surface = product-operation` row는 기본적으로 GA-scope'
        $routeMatrix | Should -Match '`route_surface = future-route` row만 GA-scope에서 제외'
        $routeMatrix | Should -Match '제외 사유와 별도 implementation plan requirement'
        $routeMatrix | Should -Match 'release_gate = release-approval-required'
        $routeMatrix | Should -Match 'network_exposure_gate = lan-exposure-approval-required'
        $routeMatrix | Should -Match 'GA-scope 제외 사유가 아니며'
        $routeMatrix | Should -Match 'execution approval 또는 exposure approval 분리'
        $routeMatrix | Should -Match '별도 ADR/task approval로 제품 범위를 줄여야'
        $routeMatrix | Should -Match 'aggregate GA-ready gate closure로 계산할 수 없다'
        $routeMatrix | Should -Match '## Evidence Freshness Rule'
        $routeMatrix | Should -Match 'commit SHA'
        $routeMatrix | Should -Match 'artifact/package version'
        $routeMatrix | Should -Match 'route/operation row id'
        $routeMatrix | Should -Match 'current owner'
        $routeMatrix | Should -Match 'target owner'
        $routeMatrix | Should -Match 'implementation basis'
        $routeMatrix | Should -Match 'fallback policy'
        $routeMatrix | Should -Match 'promotion state'
        $routeMatrix | Should -Match 'admin smoke requirement'
        $routeMatrix | Should -Match 'release gate'
        $routeMatrix | Should -Match 'network exposure gate'
        $routeMatrix | Should -Match 'runner version'
        $routeMatrix | Should -Match 'host capability snapshot'
        $routeMatrix | Should -Match 'exact command mode'
        $routeMatrix | Should -Match 'Evidence 기록 이후 current owner'
        $routeMatrix | Should -Match 'package contract'
        $routeMatrix | Should -Match 'service host'
        $routeMatrix | Should -Match 'installer custom action'
        $routeMatrix | Should -Match 'route matrix gate'
        $routeMatrix | Should -Match 'stale로 간주'
        $routeMatrix | Should -Match 'historical context'
        $routeMatrix | Should -Match 'aggregate GA-ready gate 충족에 사용할 수 없다'
        $routeMatrix | Should -Match '별도 approval waiver'
        $routeMatrix | Should -Match '## Evidence Ledger Contract'
        $routeMatrix | Should -Match 'docs/ga-ready/evidence/'
        $routeMatrix | Should -Match 'ga-ready-evidence-ledger-2026-05-04\.md'
        $routeMatrix | Should -Match 'Markdown evidence ledger'
        $routeMatrix | Should -Match 'machine-readable JSON은 만들지 않는다'
        $routeMatrix | Should -Match 'evidence_id'
        $routeMatrix | Should -Match 'route_or_operation'
        $routeMatrix | Should -Match '## Evidence Row Identity Rule'
        $routeMatrix | Should -Match '`route_or_operation`은 route matrix의 `Route/Operation` cell과 정확히 일치'
        $routeMatrix | Should -Match 'evidence row identity'
        $routeMatrix | Should -Match 'duplicate matrix row는 허용하지 않는다'
        $routeMatrix | Should -Match 'route path, operation name, route_surface, current_owner, target_owner, implementation_basis, fallback_policy, promotion_state, admin_smoke_required, release_gate, network_exposure_gate'
        $routeMatrix | Should -Match '기존 evidence는 stale로 간주'
        $routeMatrix | Should -Match 'rename 전후 row를 같은 evidence로 병합하지 않는다'
        $routeMatrix | Should -Match '새 `route_or_operation`에 대해 rerun evidence 또는 별도 approval waiver'
        $routeMatrix | Should -Match 'route_surface'
        $routeMatrix | Should -Match 'risk_tier'
        $routeMatrix | Should -Match 'current_owner'
        $routeMatrix | Should -Match 'commit_sha'
        $routeMatrix | Should -Match 'artifact_or_package_version'
        $routeMatrix | Should -Match 'target_owner'
        $routeMatrix | Should -Match 'implementation_basis'
        $routeMatrix | Should -Match 'fallback_policy'
        $routeMatrix | Should -Match 'promotion_state'
        $routeMatrix | Should -Match 'admin_smoke_required'
        $routeMatrix | Should -Match 'release_gate'
        $routeMatrix | Should -Match 'network_exposure_gate'
        $routeMatrix | Should -Match 'runner_version'
        $routeMatrix | Should -Match 'host_capability_snapshot'
        $routeMatrix | Should -Match 'exact_command_mode'
        $routeMatrix | Should -Match 'result'
        $routeMatrix | Should -Match 'created_at'
        $routeMatrix | Should -Match 'stale_triggers'
        $routeMatrix | Should -Match 'waiver_status'
        $routeMatrix | Should -Match '## Evidence Waiver Policy'
        $routeMatrix | Should -Match 'Waiver는 aggregate GA-ready gate 자체를 통과시키는 용도가 아니다'
        $routeMatrix | Should -Match '특정 stale evidence record를 제한적으로 대체하는 예외'
        $routeMatrix | Should -Match 'target owner, implementation basis, risk tier, release gate, network exposure gate는 낮출 수 없다'
        $routeMatrix | Should -Match 'waiver_id'
        $routeMatrix | Should -Match 'evidence_id'
        $routeMatrix | Should -Match 'scope'
        $routeMatrix | Should -Match 'reason'
        $routeMatrix | Should -Match 'risk_acceptance_owner'
        $routeMatrix | Should -Match 'expires_at'
        $routeMatrix | Should -Match 'replacement_evidence_required'
        $routeMatrix | Should -Match 'approval_reference'
        $routeMatrix | Should -Match 'Waiver-only gate satisfaction is forbidden'
        $routeMatrix | Should -Match 'tier3-destructive-or-persistent'
        $routeMatrix | Should -Match 'release_gate = release-approval-required'
        $routeMatrix | Should -Match 'trust-store'
        $routeMatrix | Should -Match 'firewall LAN exposure'
        $routeMatrix | Should -Match 'require rerun evidence'
        $routeMatrix | Should -Match '## Evidence Field Format and Enum Rule'
        $routeMatrix | Should -Match 'route matrix Field Schema enum을 그대로 재사용한다'
        $routeMatrix | Should -Match '`route_surface`, `risk_tier`, `current_owner`, `target_owner`, `implementation_basis`, `fallback_policy`, `promotion_state`, `admin_smoke_required`, `release_gate`, `network_exposure_gate`'
        $routeMatrix | Should -Match '`result` allowed values'
        $routeMatrix | Should -Match '`pass`, `fail`, `blocked`, `not-run`'
        $routeMatrix | Should -Match '`waiver_status` allowed values'
        $routeMatrix | Should -Match '`none`, `requested`, `approved`, `rejected`, `expired`'
        $routeMatrix | Should -Match 'full 40-char SHA'
        $routeMatrix | Should -Match '최소 12-char abbreviated SHA'
        $routeMatrix | Should -Match 'ISO-8601 timestamp'
        $routeMatrix | Should -Match '명시적 milestone reference'
        $routeMatrix | Should -Match '`scope`, `reason`, `host_capability_snapshot`, `approval_reference`'
        $routeMatrix | Should -Match '비워둘 수 없다'
        $routeMatrix | Should -Match '별도 release approval 전에는 실행하지 않는다'
        $routeMatrix | Should -Match 'ADR-0004는 내부 전용 서비스 current decision으로 적용됐다'
        $routeMatrix | Should -Match 'network_exposure_gate'
        $routeMatrix | Should -Match 'lan-exposure-approval-required'
        $routeMatrix | Should -Match 'network_exposure_gate = lan-exposure-approval-required'
        $routeMatrix | Should -Match 'LAN exposure pre-approval evidence boundary'
        $routeMatrix | Should -Match 'LAN exposure approval 전에 `blocked`를 해소할 수 있지만'
        $routeMatrix | Should -Match 'firewall execution이 아니라 pre-LAN evidence'
        $routeMatrix | Should -Match 'rule tuple validation'
        $routeMatrix | Should -Match 'loopback default preservation proof'
        $routeMatrix | Should -Match 'token source proof'
        $routeMatrix | Should -Match 'non-mutating firewall ownership checks'
        $routeMatrix | Should -Match 'scope planning'
        $routeMatrix | Should -Match 'conflict diagnostics'
        $routeMatrix | Should -Match 'firewall rule create/update/delete'
        $routeMatrix | Should -Match 'non-loopback listener exposure'
        $routeMatrix | Should -Match 'token source mutation'
        $routeMatrix | Should -Match 'external network reachability proof'
        $routeMatrix | Should -Match 'exposure-approved가 될 수 없다'
        $routeMatrix | Should -Match '## Auth and Exposure Boundary'
        $routeMatrix | Should -Match 'single_bearer_token'
        $routeMatrix | Should -Match 'no-default-account'
        $routeMatrix | Should -Match 'additive local auth surface'
        $routeMatrix | Should -Match 'RBAC enforcement는 Local API request processor가 소유한다'
        $routeMatrix | Should -Match 'explicit `--novnc-target-host`/`--novnc-target-port` 구성 전까지 disabled'
        $routeMatrix | Should -Match 'loopback static asset bypass'
        $routeMatrix | Should -Match 'unauthenticated-static-only'
        $routeMatrix | Should -Match 'non-loopback static assets require bearer auth'
        $routeMatrix | Should -Match 'LAN mode requires `-AllowLan` and a token source'
        $routeMatrix | Should -Match 'PCV_LAN_TOKEN_REQUIRED'
        $routeMatrix | Should -Match 'PCV_PREFIX_NOT_LOOPBACK'
        $routeMatrix | Should -Match 'dotnet-config-migration-action'
        $routeMatrix | Should -Match 'target_owner = dotnet-config-migration-action'
        $routeMatrix | Should -Match 'dotnet-job-store-migration-action'
        $routeMatrix | Should -Match 'target_owner = dotnet-job-store-migration-action'
        $routeMatrix | Should -Match 'dotnet-token-storage-action'
        $routeMatrix | Should -Match 'target_owner = dotnet-token-storage-action'
        $routeMatrix | Should -Match 'dotnet-data-root-action'
        $routeMatrix | Should -Match 'target_owner = dotnet-data-root-action'
        $routeMatrix | Should -Match 'target_owner = windows-native-package'
        $routeMatrix | Should -Match 'windows-eventlog-action'
        $routeMatrix | Should -Match 'target_owner = windows-eventlog-action'
        $routeMatrix | Should -Match 'windows-firewall-action'
        $routeMatrix | Should -Match 'target_owner = windows-firewall-action'
        $routeMatrix | Should -Match 'windows-trust-store-action'
        $routeMatrix | Should -Match 'target_owner = windows-trust-store-action'
        $routeMatrix | Should -Match 'windows-certificate-store-api'
        $routeMatrix | Should -Match 'product-config-migration-plan'
        $routeMatrix | Should -Match 'implementation_basis = product-config-migration-plan'
        $routeMatrix | Should -Match 'job-store-migration-plan'
        $routeMatrix | Should -Match 'implementation_basis = job-store-migration-plan'
        $routeMatrix | Should -Match 'dpapi-local-machine-token-plan'
        $routeMatrix | Should -Match 'implementation_basis = dpapi-local-machine-token-plan'
        $routeMatrix | Should -Match 'token source inventory, single-source precondition, existing protected token no-overwrite'
        $routeMatrix | Should -Match 'legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required'
        $routeMatrix | Should -Match 'command line token value forbidden, diagnostics redaction evidence'
        $routeMatrix | Should -Match 'eventlog-registration-plan'
        $routeMatrix | Should -Match 'implementation_basis = eventlog-registration-plan'
        $routeMatrix | Should -Match 'exact event source name, exact channel/log name, owned event source manifest/evidence, missing-or-owned-source precondition, foreign-source conflict blocks'
        $routeMatrix | Should -Match 'conflict diagnostics only, post-registration binding evidence, owned-source-only removal'
        $routeMatrix | Should -Match 'registry delete limited to owned event source registration'
        $routeMatrix | Should -Match 'missing-source idempotency, cleanup diagnostics only, post-removal absence evidence'
        $routeMatrix | Should -Match 'MSI default action이 아니다'
        $routeMatrix | Should -Match 'firewall-rule-plan'
        $routeMatrix | Should -Match 'implementation_basis = firewall-rule-plan'
        $routeMatrix | Should -Match 'LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name'
        $routeMatrix | Should -Match 'exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence'
        $routeMatrix | Should -Match 'missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule'
        $routeMatrix | Should -Match 'firewall write limited to owned allow rule, firewall delete limited to owned allow rule'
        $routeMatrix | Should -Match 'no service mutation, no eventlog mutation, no trust store mutation'
        $routeMatrix | Should -Match 'conflict diagnostics only, post-enable rule binding evidence, owned-rule-only removal'
        $routeMatrix | Should -Match 'missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence'
        $routeMatrix | Should -Match 'no default install/repair/MSI execution'
        $routeMatrix | Should -Match 'default install/repair/MSI action이 아니다'
        $routeMatrix | Should -Match 'data-root-lifecycle-plan'
        $routeMatrix | Should -Match 'implementation_basis = data-root-lifecycle-plan'
        $routeMatrix | Should -Match 'data-root-lifecycle-plan`은 `REMOVE_DATA=1`'
        $routeMatrix | Should -Match 'implementation_basis = windows-certificate-store-api'
        $routeMatrix | Should -Match 'exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint'
        $routeMatrix | Should -Match 'subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location'
        $routeMatrix | Should -Match 'ADR-0003 internal trust policy binding, internal/public trust model separation'
        $routeMatrix | Should -Match 'missing-or-owned-certificate precondition, subject collision diagnostics'
        $routeMatrix | Should -Match 'no overwrite of existing foreign certificate, certificate store write limited to approved certificate'
        $routeMatrix | Should -Match 'thumbprint/store binding evidence, post-install trust binding evidence'
        $routeMatrix | Should -Match 'owned certificate evidence, certificate store delete limited to owned certificate'
        $routeMatrix | Should -Match 'owned-certificate-only removal, foreign certificate conflict blocks'
        $routeMatrix | Should -Match 'missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence'
        $routeMatrix | Should -Match 'service status'
        $routeMatrix | Should -Match 'service start'
        $routeMatrix | Should -Match 'service stop'
        $routeMatrix | Should -Not -Match '\| service start/stop \|'
        $routeMatrix | Should -Match 'owned service identity'
        $routeMatrix | Should -Match 'exact SCM binary path/product root binding'
        $routeMatrix | Should -Match 'foreign service blocks'
        $routeMatrix | Should -Match 'missing-service diagnostics'
        $routeMatrix | Should -Match 'no config mutation'
        $routeMatrix | Should -Match 'no service delete'
        $routeMatrix | Should -Match 'service started state'
        $routeMatrix | Should -Match 'already-running idempotency'
        $routeMatrix | Should -Match 'listener health after start'
        $routeMatrix | Should -Match 'timeout/recovery'
        $routeMatrix | Should -Match 'stop idempotency'
        $routeMatrix | Should -Match 'already-stopped idempotency'
        $routeMatrix | Should -Match 'stop wait timeout'
        $routeMatrix | Should -Match 'stop wait timeout diagnostics'
        $routeMatrix | Should -Match 'service install create'
        $routeMatrix | Should -Match 'service configure update'
        $routeMatrix | Should -Not -Match '\| service install/configure \|'
        $routeMatrix | Should -Match 'protected token bootstrap'
        $routeMatrix | Should -Match 'service install create'
        $routeMatrix | Should -Match 'service configure update'
        $routeMatrix | Should -Match 'DesktopNode\.Host\.exe service-action configure-installed'
        $routeMatrix | Should -Match 'DesktopNode\.Host\.exe service-action repair-installed'
        $routeMatrix | Should -Match 'stable MSI repair evidence'
        $routeMatrix | Should -Match 'service name ownership'
        $routeMatrix | Should -Match 'exact SCM binary path/product root binding'
        $routeMatrix | Should -Match 'protected token path/listener args/service account/start mode/failure policy'
        $routeMatrix | Should -Match 'idempotent install behavior'
        $routeMatrix | Should -Match 'raw token 비노출'
        $routeMatrix | Should -Match 'token source inventory'
        $routeMatrix | Should -Match 'single-source precondition'
        $routeMatrix | Should -Match 'existing protected token no-overwrite'
        $routeMatrix | Should -Match 'legacy token migration'
        $routeMatrix | Should -Match 'legacy raw migration only when protected token missing'
        $routeMatrix | Should -Match 'source conflict diagnostics'
        $routeMatrix | Should -Match 'owned legacy token source required'
        $routeMatrix | Should -Match 'protected token schema'
        $routeMatrix | Should -Match 'ACL hardening'
        $routeMatrix | Should -Match 'service command line protected file path only'
        $routeMatrix | Should -Match 'command line token value forbidden'
        $routeMatrix | Should -Match 'service repair missing service recreation'
        $routeMatrix | Should -Match 'service repair config drift correction'
        $routeMatrix | Should -Not -Match '\| service repair \|'
        $routeMatrix | Should -Match 'service absent precondition'
        $routeMatrix | Should -Match 'product root/manifest/protected-token ownership'
        $routeMatrix | Should -Match 'no product/data root deletion in repair path'
        $routeMatrix | Should -Match 'service uninstall stop/delete'
        $routeMatrix | Should -Match 'product root removal preserve-data'
        $routeMatrix | Should -Match 'service uninstall remove-data request'
        $routeMatrix | Should -Not -Match '\| service uninstall preserve-data \|'
        $routeMatrix | Should -Not -Match '\| service uninstall remove-data \|'
        $routeMatrix | Should -Match 'data root remove'
        $routeMatrix | Should -Match 'protected token path/listener args correction'
        $routeMatrix | Should -Not -Match 'conditional 3010'
        $routeMatrix | Should -Match 'owned service identity'
        $routeMatrix | Should -Match 'stop-before-delete'
        $routeMatrix | Should -Match 'stop idempotency'
        $routeMatrix | Should -Match 'service deletion confirmation'
        $routeMatrix | Should -Match 'stable MSI uninstall preserve'
        $routeMatrix | Should -Match 'removes legacy WinSW root files'
        $routeMatrix | Should -Match 'remove-installed handoff/data-root-remove sequence'
        $routeMatrix | Should -Match 'no direct token/data delete before service absence'
        $routeMatrix | Should -Match 'data-root-remove --remove-data'
        $routeMatrix | Should -Match 'exact allowlist'
        $routeMatrix | Should -Match 'protected token/job/event/install/diagnostics delete proof'
        $routeMatrix | Should -Match 'service log preservation'
        $routeMatrix | Should -Match 'final reinstall proof'
        $routeMatrix | Should -Not -Match 'service install/repair/remove'
        $routeMatrix | Should -Match 'Event Log source registration'
        $routeMatrix | Should -Match 'Event Log source removal'
        $routeMatrix | Should -Not -Match '\| Event Log registration \|'
        $routeMatrix | Should -Match 'firewall rule enable LAN exposure'
        $routeMatrix | Should -Match 'firewall rule removal'
        $routeMatrix | Should -Not -Match '\| firewall rule changes \|'
        $routeMatrix | Should -Match 'trust store install'
        $routeMatrix | Should -Match 'trust store removal'
        $routeMatrix | Should -Not -Match '\| trust store changes \|'
        $routeMatrix | Should -Match '## OS Mutation Execution Guard'
        $routeMatrix | Should -Match '기본 install/repair/diagnostics/MSI 경로에서 실행하지 않는다'
        $routeMatrix | Should -Match 'service-action eventlog-register'
        $routeMatrix | Should -Match 'service-action eventlog-remove'
        $routeMatrix | Should -Match '실제 source 제거는 별도 explicit admin opt-in smoke'
        $routeMatrix | Should -Match 'exact event source name'
        $routeMatrix | Should -Match 'exact channel/log name'
        $routeMatrix | Should -Match 'owned event source manifest/evidence'
        $routeMatrix | Should -Match 'missing-or-owned-source precondition'
        $routeMatrix | Should -Match 'foreign-source conflict blocks'
        $routeMatrix | Should -Match 'exact log/source binding'
        $routeMatrix | Should -Match 'no overwrite of existing foreign source'
        $routeMatrix | Should -Match 'registry write limited to event source registration'
        $routeMatrix | Should -Match 'no service mutation'
        $routeMatrix | Should -Match 'no firewall mutation'
        $routeMatrix | Should -Match 'no trust store mutation'
        $routeMatrix | Should -Match 'conflict diagnostics only'
        $routeMatrix | Should -Match 'post-registration binding evidence'
        $routeMatrix | Should -Match 'registry delete limited to owned event source registration'
        $routeMatrix | Should -Match 'cleanup diagnostics only'
        $routeMatrix | Should -Match 'post-removal absence evidence'
        $routeMatrix | Should -Match 'no MSI/default execution'
        $routeMatrix | Should -Match 'owned-source-only removal'
        $routeMatrix | Should -Match 'missing-source idempotency'
        $routeMatrix | Should -Match 'deferred policy와 host mutation 미수행 evidence'
        $routeMatrix | Should -Match 'network_exposure_gate = lan-exposure-approval-required'
        $routeMatrix | Should -Match 'exact certificate source artifact'
        $routeMatrix | Should -Match 'artifact hash evidence'
        $routeMatrix | Should -Match 'subject/issuer/serial validity evidence'
        $routeMatrix | Should -Match 'LocalMachine Root/TrustedPublisher exact store/location'
        $routeMatrix | Should -Match 'ADR-0003 internal trust policy binding'
        $routeMatrix | Should -Match 'missing-or-owned-certificate precondition'
        $routeMatrix | Should -Match 'subject collision diagnostics'
        $routeMatrix | Should -Match 'exact certificate identity/thumbprint'
        $routeMatrix | Should -Match 'no overwrite of existing foreign certificate'
        $routeMatrix | Should -Match 'certificate store write limited to approved certificate'
        $routeMatrix | Should -Match 'no eventlog mutation'
        $routeMatrix | Should -Match 'thumbprint/store binding evidence'
        $routeMatrix | Should -Match 'post-install trust binding evidence'
        $routeMatrix | Should -Match 'owned-certificate-only removal'
        $routeMatrix | Should -Match 'missing-certificate idempotency'
        $routeMatrix | Should -Match 'local payload update'
        $routeMatrix | Should -Match 'rollback restore'
        $routeMatrix | Should -Match 'package-contract'
        $routeMatrix | Should -Match 'implementation_basis = package-contract'
        $routeMatrix | Should -Match 'internal `RequireSigned` stable `0.35.3` payload update from stable `0.35.2`'
        $routeMatrix | Should -Match 'schema v1 payload manifest/version match'
        $routeMatrix | Should -Match 'retained previous root stable `0.35.2` manifest/hash validation'
        $routeMatrix | Should -Match 'rollback from `0.35.3`'
        $routeMatrix | Should -Match 'failed root diagnostics preservation'
        $routeMatrix | Should -Match 'staged source outside active root'
        $routeMatrix | Should -Match 'binary payload activation'
        $routeMatrix | Should -Match 'no config mutation'
        $routeMatrix | Should -Match 'no data root mutation'
        $routeMatrix | Should -Match 'no token mutation'
        $routeMatrix | Should -Match 'no service identity mutation'
        $routeMatrix | Should -Match 'service stop/start health'
        $routeMatrix | Should -Match 'post-rollback manifest/version/health'
        $routeMatrix | Should -Match 'data/token preservation'
        $routeMatrix | Should -Match 'stable-internal-release-update-rollback-20260505-015550-0352-0353'
        $routeMatrix | Should -Not -Match '\| update/rollback \|'
        $routeMatrix | Should -Match 'product config schema validation'
        $routeMatrix | Should -Match 'product config migration apply'
        $routeMatrix | Should -Not -Match '\| product config migration \|'
        $routeMatrix | Should -Match 'job store schema mismatch detection'
        $routeMatrix | Should -Match 'job store migration apply'
        $routeMatrix | Should -Match 'schema v1 product manifest validation'
        $routeMatrix | Should -Match 'update payload preflight validation'
        $routeMatrix | Should -Match 'dry-run config migration descriptor only'
        $routeMatrix | Should -Match 'diagnostics redaction evidence'
        $routeMatrix | Should -Match 'no config write'
        $routeMatrix | Should -Match 'no config write/backup/service mutation'
        $routeMatrix | Should -Match 'no service mutation'
        $routeMatrix | Should -Match 'no migration execution'
        $routeMatrix | Should -Match 'validation writes forbidden'
        $routeMatrix | Should -Match 'explicit admin opt-in before config write'
        $routeMatrix | Should -Match 'current config source inventory'
        $routeMatrix | Should -Match 'current schema owner resolution'
        $routeMatrix | Should -Match 'owned source config path evidence'
        $routeMatrix | Should -Match 'source path/version evidence'
        $routeMatrix | Should -Match 'source/target schema version evidence'
        $routeMatrix | Should -Match 'migration plan id/version'
        $routeMatrix | Should -Match 'validation preflight descriptor required'
        $routeMatrix | Should -Match 'backup path inside owned config backup root'
        $routeMatrix | Should -Match 'atomic config replace'
        $routeMatrix | Should -Match 'no job store mutation'
        $routeMatrix | Should -Match 'partial config migration forbidden evidence'
        $routeMatrix | Should -Match 'rollback on migration failure'
        $routeMatrix | Should -Match 'rollback result diagnostics'
        $routeMatrix | Should -Match 'read-only-or-blocked-with-diagnostics'
        $routeMatrix | Should -Match 'schema mismatch returns blocked diagnostics'
        $routeMatrix | Should -Match 'runtime read must not mutate jobs.json'
        $routeMatrix | Should -Match 'no quarantine move/write'
        $routeMatrix | Should -Match 'migration handoff descriptor only'
        $routeMatrix | Should -Match 'no migration execution'
        $routeMatrix | Should -Match 'current job store path inventory'
        $routeMatrix | Should -Match 'current job schema owner evidence'
        $routeMatrix | Should -Match 'owned job store path evidence'
        $routeMatrix | Should -Match 'source job store version evidence'
        $routeMatrix | Should -Match 'source/target schema version evidence'
        $routeMatrix | Should -Match 'migration plan id/version'
        $routeMatrix | Should -Match 'service stopped precondition'
        $routeMatrix | Should -Match 'runtime writer stopped evidence'
        $routeMatrix | Should -Match 'backup path inside owned job-store backup root'
        $routeMatrix | Should -Match 'destructive rewrite disabled by default'
        $routeMatrix | Should -Match 'atomic job store replace'
        $routeMatrix | Should -Match 'no config mutation'
        $routeMatrix | Should -Match 'no token mutation'
        $routeMatrix | Should -Match 'no service identity mutation'
        $routeMatrix | Should -Match 'partial job store migration forbidden evidence'
        $routeMatrix | Should -Match 'rollback on migration failure'
        $routeMatrix | Should -Match 'rollback result diagnostics'
        $routeMatrix | Should -Match 'recovery evidence'
        $routeMatrix | Should -Match 'explicit admin opt-in before job store write'
        $routeMatrix | Should -Match 'GET /api/v1/runtime/policy'
        $routeMatrix | Should -Match 'secret 비노출'
        $routeMatrix | Should -Match 'POST /api/v1/vms/\{id\}/shutdown'
        $routeMatrix | Should -Match 'POST /api/v1/vms/\{id\}/restart'
        $routeMatrix | Should -Match 'graceful shutdown semantics'
        $routeMatrix | Should -Match 'stop-start sequencing'
        $routeMatrix | Should -Match 'GET /api/v1/jobs/\{job_id\}'
        $routeMatrix | Should -Match 'POST /api/v1/jobs/\{job_id\}/cancel'
        $routeMatrix | Should -Match 'POST /api/v1/jobs/\{job_id\}/retry'
        $routeMatrix | Should -Not -Match 'GET /api/v1/jobs/\{id\}'
        $routeMatrix | Should -Not -Match 'POST /api/v1/jobs/\{id\}/cancel'
        $routeMatrix | Should -Not -Match 'POST /api/v1/jobs/\{id\}/retry'
        $routeMatrix | Should -Match 'GET /api/v1/vms/\{id\}/checkpoints'
        $routeMatrix | Should -Match 'POST /api/v1/vms/\{id\}/checkpoints/\{checkpoint_id\}/restore'
        $routeMatrix | Should -Match 'DELETE /api/v1/vms/\{id\}/checkpoints/\{checkpoint_id\}'
        $routeMatrix | Should -Match 'DELETE /api/v1/vms/\{id\}'
        $routeMatrix | Should -Match 'future implementation plan'
        $routeMatrix | Should -Not -Match '/checkpoints/\{name\}'
        $routeMatrix | Should -Match 'name`/`checkpoint_name'
        $routeMatrix | Should -Match '원본 job operation'
        $routeMatrix | Should -Match 'GA-ready gate, release gate, network exposure gate'
        $routeMatrix | Should -Match 'not-yet-defined'
        $routeMatrix | Should -Match 'current_owner = not-yet-defined'
        $routeMatrix | Should -Match 'GA-ready blocker'
        $routeMatrix | Should -Match 'GET /api/v1/vms'

        $schemaEnums = @{}
        $inFieldSchema = $false
        foreach ($line in ($routeMatrix -split "`r?`n")) {
            if ($line -eq '## Field Schema') {
                $inFieldSchema = $true
                continue
            }
            if ($inFieldSchema -and $line -match '^## ') {
                break
            }
            if (-not $inFieldSchema) {
                continue
            }

            $schemaMatch = [regex]::Match($line, '^\|\s*`(?<field>[^`]+)`\s*\|\s*yes\s*\|\s*(?<values>.+?)\s*\|$')
            if ($schemaMatch.Success) {
                $enumValues = [regex]::Matches($schemaMatch.Groups['values'].Value, '`([^`]+)`') | ForEach-Object { $_.Groups[1].Value }
                if (@($enumValues).Count -gt 0) {
                    $schemaEnums[$schemaMatch.Groups['field'].Value] = @($enumValues)
                }
            }
        }

        foreach ($field in @('route_surface', 'domain', 'risk_tier', 'current_owner', 'target_owner', 'implementation_basis', 'fallback_policy', 'promotion_state', 'admin_smoke_required', 'release_gate', 'network_exposure_gate')) {
            $schemaEnums.ContainsKey($field) | Should -BeTrue
        }

        $matrixRows = foreach ($line in ($routeMatrix -split "`r?`n")) {
            if (
                $line -match '^\|' -and
                $line -notmatch '^\|\s*-+' -and
                $line -notmatch '^\|\s*(Route/Operation|Operation)\s*\|'
            ) {
                $cells = $line.Trim().Trim('|').Split('|').ForEach({ $_.Trim() })
                if ($cells.Count -eq 13) {
                    [pscustomobject]@{
                        Name = $cells[0]
                        RouteSurface = $cells[1] -replace '^`|`$', ''
                        Domain = $cells[2] -replace '^`|`$', ''
                        RiskTier = $cells[3] -replace '^`|`$', ''
                        CurrentOwner = $cells[4] -replace '^`|`$', ''
                        TargetOwner = $cells[5] -replace '^`|`$', ''
                        ImplementationBasis = $cells[6] -replace '^`|`$', ''
                        FallbackPolicy = $cells[7] -replace '^`|`$', ''
                        PromotionState = $cells[8] -replace '^`|`$', ''
                        AdminSmokeRequired = $cells[9] -replace '^`|`$', ''
                        GaReadyGate = $cells[10]
                        ReleaseGate = $cells[11] -replace '^`|`$', ''
                        NetworkExposureGate = $cells[12] -replace '^`|`$', ''
                    }
                }
            }
        }

        @($matrixRows).Count | Should -BeGreaterThan 0
        $duplicateMatrixRows = @($matrixRows | Group-Object -Property Name | Where-Object { $_.Count -gt 1 } | Select-Object -ExpandProperty Name)
        $duplicateMatrixRows | Should -BeNullOrEmpty
        $matrixRows.Name | Should -Contain '`GET /api/v1/jobs`'
        $matrixRows.Name | Should -Contain '`GET /api/v1/jobs/{job_id}`'
        $matrixRows.Name | Should -Contain '`POST /api/v1/jobs/{job_id}/cancel`'
        $matrixRows.Name | Should -Contain '`POST /api/v1/jobs/{job_id}/retry`'
        $matrixRows.Name | Should -Not -Contain '`GET /api/v1/jobs/{id}`'
        $matrixRows.Name | Should -Not -Contain '`POST /api/v1/jobs/{id}/cancel`'
        $matrixRows.Name | Should -Not -Contain '`POST /api/v1/jobs/{id}/retry`'
        $matrixRows.Name | Should -Contain '`GET /api/v1/vms/{id}`'
        $matrixRows.Name | Should -Contain '`POST /api/v1/vms/{id}/shutdown`'
        $matrixRows.Name | Should -Contain '`DELETE /api/v1/vms/{id}`'
        $matrixRows.Name | Should -Not -Contain '`GET /api/v1/vms/{vm_id}`'
        $matrixRows.Name | Should -Not -Contain '`GET /api/v1/vms/{vmId}`'
        $matrixRows.Name | Should -Not -Contain '`DELETE /api/v1/vms/{vm_id}`'
        $matrixRows.Name | Should -Not -Contain '`DELETE /api/v1/vms/{vmId}`'
        $matrixRows.Name | Should -Not -Contain '`POST /api/v1/vms/{vmId}/lifecycle/{action}`'
        $matrixRows.Name | Should -Not -Contain '`POST /api/v1/vms/{id}/lifecycle/{action}`'
        foreach ($row in $matrixRows) {
            $schemaEnums['route_surface'] | Should -Contain $row.RouteSurface
            $schemaEnums['domain'] | Should -Contain $row.Domain
            $schemaEnums['risk_tier'] | Should -Contain $row.RiskTier
            $schemaEnums['current_owner'] | Should -Contain $row.CurrentOwner
            $schemaEnums['target_owner'] | Should -Contain $row.TargetOwner
            $schemaEnums['implementation_basis'] | Should -Contain $row.ImplementationBasis
            $schemaEnums['fallback_policy'] | Should -Contain $row.FallbackPolicy
            $schemaEnums['promotion_state'] | Should -Contain $row.PromotionState
            $schemaEnums['admin_smoke_required'] | Should -Contain $row.AdminSmokeRequired
            $schemaEnums['release_gate'] | Should -Contain $row.ReleaseGate
            $schemaEnums['network_exposure_gate'] | Should -Contain $row.NetworkExposureGate

            if ($row.RouteSurface -eq 'future-route') {
                @('product config migration apply', 'job store migration apply') | Should -Contain $row.Name
                $row.CurrentOwner | Should -Be 'not-implemented'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.GaReadyGate | Should -Match 'future implementation exclusion'
                $row.GaReadyGate | Should -Match 'separate implementation plan'
            }
            if ($row.CurrentOwner -eq 'not-implemented') {
                $row.RouteSurface | Should -Be 'future-route'
            }
            if ($row.Name -eq '`DELETE /api/v1/vms/{id}`') {
                $row.RouteSurface | Should -Be 'current-route'
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.GaReadyGate | Should -Match 'C# WMI `DestroySystem`'
                $row.GaReadyGate | Should -Match 'managed marker guard'
                $row.GaReadyGate | Should -Match 'not-found/idempotency contract'
                $row.GaReadyGate | Should -Match '0.30.1-admin-smoke'
                $row.GaReadyGate | Should -Match 'repeat `action=absent`'
                $row.GaReadyGate | Should -Match 'unmanaged guard block'
            }

            if ($row.Name -eq '`GET /api/v1/network/inventory`') {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.GaReadyGate | Should -Match 'no helper retry'
            }

            if ($row.Name -eq '`POST /api/v1/jobs/{job_id}/retry`') {
                $row.Domain | Should -Be 'job-runtime'
                $row.RiskTier | Should -Be 'tier2-reversible-mutation'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
            }

            if ($row.Name -in @('`POST /api/v1/vms/{id}/shutdown`', '`POST /api/v1/vms/{id}/restart`')) {
                $row.Domain | Should -Be 'vm-lifecycle'
                $row.RiskTier | Should -Be 'tier2-reversible-mutation'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
            }

            $serviceProductOpsRows = @('service status', 'service start', 'service stop', 'service install create', 'service configure update', 'service repair missing service recreation', 'service repair config drift correction', 'service uninstall stop/delete', 'product root removal preserve-data', 'service uninstall remove-data request')
            $nativeServiceProductOpsRows = @('service install create', 'service configure update', 'service repair missing service recreation', 'service repair config drift correction', 'service uninstall stop/delete', 'product root removal preserve-data', 'service uninstall remove-data request')
            if ($row.CurrentOwner -eq 'mixed-history') {
                $row.Name | Should -BeNullOrEmpty
            }
            if ($nativeServiceProductOpsRows -contains $row.Name) {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.RouteSurface | Should -Be 'product-operation'
                $row.Domain | Should -Be 'product-ops'
                $row.TargetOwner | Should -Be 'dotnet-service-action'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.GaReadyGate | Should -Match 'stable'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
            }
            if ($row.Name -in @('service status', 'service start', 'service stop')) {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.TargetOwner | Should -Be 'dotnet-service-action'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.GaReadyGate | Should -Match 'code-level native SCM controller'
                $row.GaReadyGate | Should -Match 'service-action-status-start-stop'
                $row.GaReadyGate | Should -Not -Match 'pending'
            }

            if ($row.Name -in @('service start', 'service stop')) {
                $row.Domain | Should -Be 'product-ops'
                $row.RiskTier | Should -Be 'tier2-reversible-mutation'
                $row.TargetOwner | Should -Be 'dotnet-service-action'
                $row.ImplementationBasis | Should -Be 'windows-native-api'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'foreign service blocks'
                $row.GaReadyGate | Should -Match 'missing-service diagnostics'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no service delete'
            }
            if ($row.Name -eq 'service start') {
                $row.GaReadyGate | Should -Match 'service started state'
                $row.GaReadyGate | Should -Match 'already-running idempotency'
                $row.GaReadyGate | Should -Match 'listener health after start'
                $row.GaReadyGate | Should -Match 'timeout/recovery'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'already-stopped idempotency'
                $row.GaReadyGate | Should -Not -Match 'stop wait timeout diagnostics'
            }
            if ($row.Name -eq 'service stop') {
                $row.GaReadyGate | Should -Match 'stop idempotency'
                $row.GaReadyGate | Should -Match 'already-stopped idempotency'
                $row.GaReadyGate | Should -Match 'stop wait timeout'
                $row.GaReadyGate | Should -Match 'stop wait timeout diagnostics'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'listener health after start'
            }

            if ($row.Name -in @('service install create', 'service configure update', 'service repair missing service recreation', 'service repair config drift correction', 'service uninstall stop/delete', 'product root removal preserve-data', 'service uninstall remove-data request')) {
                $row.Domain | Should -Be 'product-ops'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.TargetOwner | Should -Be 'dotnet-service-action'
                $row.ImplementationBasis | Should -Be 'windows-native-api'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
            }
            if ($row.Name -eq 'service install create') {
                $row.GaReadyGate | Should -Match 'DesktopNode\.Host\.exe service-action configure-installed'
                $row.GaReadyGate | Should -Match 'MSI stable install path'
                $row.GaReadyGate | Should -Match 'service name ownership'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args'
                $row.GaReadyGate | Should -Match 'service account'
                $row.GaReadyGate | Should -Match 'start mode'
                $row.GaReadyGate | Should -Match 'failure policy'
                $row.GaReadyGate | Should -Match 'idempotent install behavior'
                $row.GaReadyGate | Should -Match 'fresh stable internal evidence'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'protected token bootstrap'
                $row.GaReadyGate | Should -Not -Match 'existing config reuse'
                $row.GaReadyGate | Should -Not -Match 'repair path only'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
            }
            if ($row.Name -eq 'service configure update') {
                $row.GaReadyGate | Should -Match 'DesktopNode\.Host\.exe service-action repair-installed'
                $row.GaReadyGate | Should -Match 'stable MSI repair path'
                $row.GaReadyGate | Should -Match 'owned service precondition'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args preservation'
                $row.GaReadyGate | Should -Match 'data preservation'
                $row.GaReadyGate | Should -Match 'fresh stable internal evidence'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'missing-service precondition'
                $row.GaReadyGate | Should -Not -Match 'no overwrite of existing foreign service'
            }
            if ($row.Name -eq 'service repair missing service recreation') {
                $row.GaReadyGate | Should -Match 'native service action install/repair owner'
                $row.GaReadyGate | Should -Match 'service absent recreate contract'
                $row.GaReadyGate | Should -Match 'product root/manifest/protected-token ownership'
                $row.GaReadyGate | Should -Match 'exact SCM binary path binding'
                $row.GaReadyGate | Should -Match 'no product/data root deletion in repair path'
                $row.GaReadyGate | Should -Match 'stable MSI reinstall-after-absent'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
                $row.GaReadyGate | Should -Not -Match 'initial install path'
            }
            if ($row.Name -eq 'service repair config drift correction') {
                $row.GaReadyGate | Should -Match 'native service action repair/configure owner'
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'protected token path/listener args correction'
                $row.GaReadyGate | Should -Match 'foreign service block/code-level tests'
                $row.GaReadyGate | Should -Match 'stable MSI repair evidence'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'SCM service recreate'
                $row.GaReadyGate | Should -Not -Match 'service absent precondition'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
            }
            if ($row.Name -eq 'service uninstall stop/delete') {
                $row.GaReadyGate | Should -Match 'stable MSI uninstall preserve/remove-data stop-before-delete'
                $row.GaReadyGate | Should -Match 'service deletion confirmation'
                $row.GaReadyGate | Should -Match 'missing-service wait'
                $row.GaReadyGate | Should -Match 'no product/data direct mutation by service action'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'product root allowlist'
                $row.GaReadyGate | Should -Not -Match 'ProgramData preserve evidence'
                $row.GaReadyGate | Should -Not -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Not -Match 'REMOVE_DATA=1 request validation'
            }
            if ($row.Name -eq 'product root removal preserve-data') {
                $row.GaReadyGate | Should -Match 'stable MSI uninstall preserve deletes current product payload'
                $row.GaReadyGate | Should -Match 'leaves ProgramData/token/data root intact'
                $row.GaReadyGate | Should -Match 'removes legacy WinSW root files'
                $row.GaReadyGate | Should -Match 'final active root has no legacy WinSW root files'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'service stop/delete'
                $row.GaReadyGate | Should -Not -Match 'delete service only'
                $row.GaReadyGate | Should -Not -Match 'service deletion confirmation'
                $row.GaReadyGate | Should -Not -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Not -Match 'REMOVE_DATA=1 request validation'
                $row.GaReadyGate | Should -Not -Match 'ProgramData delete allowlist'
                $row.GaReadyGate | Should -Not -Match 'sensitive token ACL repair'
            }
            if ($row.Name -eq 'service uninstall remove-data request') {
                $row.GaReadyGate | Should -Match 'stable MSI `REMOVE_DATA=1` request'
                $row.GaReadyGate | Should -Match 'service deleted/absent precondition'
                $row.GaReadyGate | Should -Match 'remove-installed handoff/data-root-remove sequence'
                $row.GaReadyGate | Should -Match 'no direct token/data delete before service absence'
                $row.GaReadyGate | Should -Match 'final reinstall proof'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'service stopped/deleted precondition'
                $row.GaReadyGate | Should -Not -Match 'service stop/delete'
                $row.GaReadyGate | Should -Not -Match 'product root allowlist'
                $row.GaReadyGate | Should -Not -Match 'ProgramData delete allowlist'
                $row.GaReadyGate | Should -Not -Match 'sensitive token ACL repair'
            }

            if ($row.CurrentOwner -eq 'not-yet-defined') {
                $row.Name | Should -Be 'product config migration apply'
            }
            if ($row.Name -eq 'product config schema validation') {
                $row.CurrentOwner | Should -Be 'dotnet-runtime'
                $row.TargetOwner | Should -Be 'dotnet-runtime'
                $row.ImplementationBasis | Should -Be 'dotnet-runtime'
                $row.RiskTier | Should -Be 'tier1-read-only'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'schema v1 product manifest validation'
                $row.GaReadyGate | Should -Match 'update payload preflight validation'
                $row.GaReadyGate | Should -Match 'dry-run config migration descriptor only'
                $row.GaReadyGate | Should -Match 'diagnostics redaction'
                $row.GaReadyGate | Should -Match 'no config write'
                $row.GaReadyGate | Should -Match 'no config write/backup/service mutation'
                $row.GaReadyGate | Should -Match 'no config write/backup/service mutation'
                $row.GaReadyGate | Should -Match 'stable payload manifest evidence'
                $row.GaReadyGate | Should -Not -Match 'service-start block on validation failure'
                $row.GaReadyGate | Should -Not -Match 'validation writes forbidden evidence'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.RouteSurface | Should -Be 'product-operation'
            }

            if ($row.TargetOwner -eq 'dotnet-config-migration-action') {
                $row.Name | Should -Be 'product config migration apply'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.TargetOwner | Should -Be 'dotnet-config-migration-action'
                $row.ImplementationBasis | Should -Be 'product-config-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'DesktopNode\.Host\.exe service-action config-migration-apply'
                $row.GaReadyGate | Should -Match 'actual apply path'
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'stopped service proof'
                $row.GaReadyGate | Should -Match 'owned product manifest'
                $row.GaReadyGate | Should -Match 'source schema v1'
                $row.GaReadyGate | Should -Match 'config backup'
                $row.GaReadyGate | Should -Match 'same-directory temp write/replace'
                $row.GaReadyGate | Should -Match 'rollback diagnostics'
                $row.GaReadyGate | Should -Match 'Installed destructive admin smoke PASS evidence'
                $row.GaReadyGate | Should -Match '0\.38\.6-admin-smoke'
                $row.GaReadyGate | Should -Match 'artifacts/config-jobstore-migration-apply-installed-20260507-0386'
                $row.GaReadyGate | Should -Match 'Public trusted signing/external stable publication excluded'
                $row.GaReadyGate | Should -Not -Match 'future implementation exclusion'
                $row.GaReadyGate | Should -Not -Match 'no GA current product operation writes product config'
            }
            if ($row.TargetOwner -eq 'dotnet-token-storage-action') {
                $row.Name | Should -Be 'protected token bootstrap'
            }
            if ($row.Name -eq 'protected token bootstrap') {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.TargetOwner | Should -Be 'dotnet-token-storage-action'
                $row.ImplementationBasis | Should -Be 'dpapi-local-machine-token-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'DPAPI LocalMachine protected token file'
                $row.GaReadyGate | Should -Match 'raw token not exposed'
                $row.GaReadyGate | Should -Match 'existing token no-overwrite'
                $row.GaReadyGate | Should -Match 'protected file path only in SCM command line'
                $row.GaReadyGate | Should -Match 'REMOVE_DATA final reinstall proof'
                $row.GaReadyGate | Should -Match 'fresh stable internal evidence'
            }
            if ($row.TargetOwner -eq 'dotnet-data-root-action') {
                $row.Name | Should -Be 'data root remove'
            }
            if ($row.Name -eq 'data root remove') {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.TargetOwner | Should -Be 'dotnet-data-root-action'
                $row.ImplementationBasis | Should -Be 'data-root-lifecycle-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'data-root-remove --remove-data'
                $row.GaReadyGate | Should -Match 'exact allowlist'
                $row.GaReadyGate | Should -Match 'service absent precondition'
                $row.GaReadyGate | Should -Match 'protected token/job/event/install/diagnostics delete proof'
                $row.GaReadyGate | Should -Match 'service log preservation'
                $row.GaReadyGate | Should -Match 'final reinstall proof'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'ProgramData delete allowlist'
                $row.GaReadyGate | Should -Not -Match 'sensitive token ACL repair'
            }

            if ($row.TargetOwner -eq 'dotnet-job-store-migration-action') {
                $row.Name | Should -Be 'job store migration apply'
            }
            if ($row.Name -eq 'job store schema mismatch detection') {
                $row.CurrentOwner | Should -Be 'dotnet-runtime'
                $row.TargetOwner | Should -Be 'dotnet-runtime'
                $row.ImplementationBasis | Should -Be 'dotnet-runtime'
                $row.RiskTier | Should -Be 'tier1-read-only'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'read-only-or-blocked-with-diagnostics'
                $row.GaReadyGate | Should -Match 'schema mismatch behavior'
                $row.GaReadyGate | Should -Match 'schema mismatch returns blocked diagnostics'
                $row.GaReadyGate | Should -Match 'runtime read must not mutate jobs.json'
                $row.GaReadyGate | Should -Match 'no quarantine move/write'
                $row.GaReadyGate | Should -Match 'migration handoff descriptor only'
                $row.GaReadyGate | Should -Match 'no migration execution'
                $row.GaReadyGate | Should -Match 'xUnit evidence'
                $row.GaReadyGate | Should -Match 'blocked-diagnostics-no-mutation'
                $row.GaReadyGate | Should -Not -Match 'current quarantine move/write behavior'
                $row.GaReadyGate | Should -Not -Match 'moved under explicit'
                $row.GaReadyGate | Should -Not -Match 'atomic job store replace'
                $row.GaReadyGate | Should -Not -Match 'destructive rewrite disabled by default'
            }
            if ($row.Name -eq 'job store migration apply') {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.RouteSurface | Should -Be 'product-operation'
                $row.TargetOwner | Should -Be 'dotnet-job-store-migration-action'
                $row.ImplementationBasis | Should -Be 'job-store-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'DesktopNode\.Host\.exe service-action job-store-migration-apply'
                $row.GaReadyGate | Should -Match 'actual apply path'
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'stopped service/runtime writer proof'
                $row.GaReadyGate | Should -Match 'owned `jobs\.json`'
                $row.GaReadyGate | Should -Match 'source schema v1'
                $row.GaReadyGate | Should -Match 'job store backup'
                $row.GaReadyGate | Should -Match 'same-directory temp write/replace'
                $row.GaReadyGate | Should -Match 'recovery diagnostics'
                $row.GaReadyGate | Should -Match 'Runtime now loads schema v2 migration stores'
                $row.GaReadyGate | Should -Match 'Installed destructive admin smoke PASS evidence'
                $row.GaReadyGate | Should -Match '0\.38\.6-admin-smoke'
                $row.GaReadyGate | Should -Match 'artifacts/config-jobstore-migration-apply-installed-20260507-0386'
                $row.GaReadyGate | Should -Match 'Public trusted signing/external stable publication excluded'
                $row.GaReadyGate | Should -Not -Match 'future implementation exclusion'
                $row.GaReadyGate | Should -Not -Match 'no GA current product operation rewrites job store'
            }

            if ($row.TargetOwner -eq 'windows-native-package') {
                @('local payload update', 'rollback restore') | Should -Contain $row.Name
            }
            if ($row.TargetOwner -eq 'windows-eventlog-action') {
                @('Event Log source registration', 'Event Log source removal') | Should -Contain $row.Name
            }
            if ($row.Name -in @('Event Log source registration', 'Event Log source removal')) {
                $row.TargetOwner | Should -Be 'windows-eventlog-action'
            }
            if ($row.TargetOwner -eq 'windows-firewall-action') {
                @('firewall rule enable LAN exposure', 'firewall rule removal') | Should -Contain $row.Name
            }
            if ($row.Name -in @('firewall rule enable LAN exposure', 'firewall rule removal')) {
                $row.TargetOwner | Should -Be 'windows-firewall-action'
            }

            if ($row.TargetOwner -eq 'windows-trust-store-action') {
                @('trust store install', 'trust store removal') | Should -Contain $row.Name
            }
            if ($row.Name -in @('trust store install', 'trust store removal')) {
                $row.TargetOwner | Should -Be 'windows-trust-store-action'
            }

            if ($row.ImplementationBasis -eq 'eventlog-registration-plan') {
                @('Event Log source registration', 'Event Log source removal') | Should -Contain $row.Name
            }
            if ($row.ImplementationBasis -eq 'product-config-migration-plan') {
                $row.Name | Should -Be 'product config migration apply'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.ImplementationBasis | Should -Be 'product-config-migration-plan'
            }
            if ($row.ImplementationBasis -eq 'job-store-migration-plan') {
                $row.Name | Should -Be 'job store migration apply'
            }
            if ($row.Name -eq 'job store migration apply') {
                $row.ImplementationBasis | Should -Be 'job-store-migration-plan'
            }
            if ($row.ImplementationBasis -eq 'dpapi-local-machine-token-plan') {
                $row.Name | Should -Be 'protected token bootstrap'
            }
            if ($row.Name -eq 'protected token bootstrap') {
                $row.ImplementationBasis | Should -Be 'dpapi-local-machine-token-plan'
            }
            if ($row.Name -in @('Event Log source registration', 'Event Log source removal')) {
                $row.ImplementationBasis | Should -Be 'eventlog-registration-plan'
            }
            if ($row.ImplementationBasis -eq 'firewall-rule-plan') {
                @('firewall rule enable LAN exposure', 'firewall rule removal') | Should -Contain $row.Name
            }
            if ($row.Name -in @('firewall rule enable LAN exposure', 'firewall rule removal')) {
                $row.ImplementationBasis | Should -Be 'firewall-rule-plan'
            }
            if ($row.ImplementationBasis -eq 'data-root-lifecycle-plan') {
                $row.Name | Should -Be 'data root remove'
            }
            if ($row.Name -eq 'data root remove') {
                $row.ImplementationBasis | Should -Be 'data-root-lifecycle-plan'
            }
            if ($row.ImplementationBasis -eq 'windows-certificate-store-api') {
                @('trust store install', 'trust store removal') | Should -Contain $row.Name
            }
            if ($row.Name -in @('trust store install', 'trust store removal')) {
                $row.ImplementationBasis | Should -Be 'windows-certificate-store-api'
            }
            $row.ImplementationBasis | Should -Not -Be 'approved-system-executable'

            if ($row.Domain -eq 'operating-system-ops') {
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                if ($row.Name -in @(
                    'Event Log source registration',
                    'Event Log source removal',
                    'firewall rule enable LAN exposure',
                    'firewall rule removal',
                    'trust store install',
                    'trust store removal'
                )) {
                    $row.CurrentOwner | Should -Be 'dotnet-native'
                    $row.FallbackPolicy | Should -Be 'none'
                    $row.PromotionState | Should -Be 'current-native'
                }
                else {
                    $row.FallbackPolicy | Should -Be 'blocked'
                    $row.PromotionState | Should -Be 'blocked'
                }
            }
            if ($row.Name -eq 'Event Log source registration') {
                $row.GaReadyGate | Should -Match 'DesktopNode\.Host\.exe service-action eventlog-register'
                $row.GaReadyGate | Should -Match 'code-level registry-backed event source action'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in'
                $row.GaReadyGate | Should -Match 'exact event source name'
                $row.GaReadyGate | Should -Match 'exact channel/log name'
                $row.GaReadyGate | Should -Match 'owned event source manifest/evidence'
                $row.GaReadyGate | Should -Match 'missing-or-owned-source precondition'
                $row.GaReadyGate | Should -Match 'foreign-source conflict blocks'
                $row.GaReadyGate | Should -Match 'exact log/source binding'
                $row.GaReadyGate | Should -Match 'no overwrite of existing foreign source'
                $row.GaReadyGate | Should -Match 'registry write limited to event source registration'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no firewall mutation'
                $row.GaReadyGate | Should -Match 'no trust store mutation'
                $row.GaReadyGate | Should -Match 'conflict diagnostics only'
                $row.GaReadyGate | Should -Match 'post-registration binding evidence'
                $row.GaReadyGate | Should -Match 'no MSI/default execution'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'owned-source-only removal'
                $row.GaReadyGate | Should -Not -Match 'missing-source idempotency'
                $row.GaReadyGate | Should -Not -Match 'source identity'
                $row.GaReadyGate | Should -Not -Match 'channel/source existence'
                $row.GaReadyGate | Should -Not -Match 'registry delete limited to owned event source registration'
                $row.GaReadyGate | Should -Not -Match 'cleanup diagnostics only'
                $row.GaReadyGate | Should -Not -Match 'post-removal absence evidence'
                $row.ReleaseGate | Should -Be 'none'
                $row.NetworkExposureGate | Should -Be 'none'
            }
            if ($row.Name -eq 'Event Log source removal') {
                $row.GaReadyGate | Should -Match 'DesktopNode\.Host\.exe service-action eventlog-remove'
                $row.GaReadyGate | Should -Match 'code-level registry-backed event source removal action'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in'
                $row.GaReadyGate | Should -Match 'exact event source name'
                $row.GaReadyGate | Should -Match 'exact channel/log name'
                $row.GaReadyGate | Should -Match 'owned event source manifest/evidence'
                $row.GaReadyGate | Should -Match 'exact log/source binding'
                $row.GaReadyGate | Should -Match 'owned-source-only removal'
                $row.GaReadyGate | Should -Match 'foreign-source conflict blocks'
                $row.GaReadyGate | Should -Match 'registry delete limited to owned event source registration'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no firewall mutation'
                $row.GaReadyGate | Should -Match 'no trust store mutation'
                $row.GaReadyGate | Should -Match 'missing-source idempotency'
                $row.GaReadyGate | Should -Match 'cleanup diagnostics only'
                $row.GaReadyGate | Should -Match 'post-removal absence evidence'
                $row.GaReadyGate | Should -Match 'no MSI/default execution'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Match 'xUnit evidence `EventLogRemoveDeletesOwnedSourceWithoutExternalCommands`'
                $row.GaReadyGate | Should -Match '`EventLogRemoveTreatsMissingSourceAsIdempotentSuccess`'
                $row.GaReadyGate | Should -Not -Match 'missing-or-owned-source precondition'
                $row.GaReadyGate | Should -Not -Match 'no overwrite of existing foreign source'
                $row.GaReadyGate | Should -Not -Match 'channel/source existence'
                $row.GaReadyGate | Should -Not -Match 'registry write limited to event source registration'
                $row.GaReadyGate | Should -Not -Match 'post-registration binding evidence'
                $row.GaReadyGate | Should -Not -Match 'cleanup evidence'
                $row.ReleaseGate | Should -Be 'none'
                $row.NetworkExposureGate | Should -Be 'none'
            }
            if ($row.Name -eq 'firewall rule enable LAN exposure') {
                $row.GaReadyGate | Should -Match 'LAN exposure approval'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in'
                $row.GaReadyGate | Should -Match 'loopback default preservation'
                $row.GaReadyGate | Should -Match 'exact rule name'
                $row.GaReadyGate | Should -Match 'exact direction'
                $row.GaReadyGate | Should -Match 'exact protocol'
                $row.GaReadyGate | Should -Match 'exact local port'
                $row.GaReadyGate | Should -Match 'exact profile'
                $row.GaReadyGate | Should -Match 'exact remote address scope'
                $row.GaReadyGate | Should -Match 'missing-or-owned-rule precondition'
                $row.GaReadyGate | Should -Match 'foreign-rule conflict blocks'
                $row.GaReadyGate | Should -Match 'no overwrite of existing foreign rule'
                $row.GaReadyGate | Should -Match 'firewall write limited to owned allow rule'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no eventlog mutation'
                $row.GaReadyGate | Should -Match 'no trust store mutation'
                $row.GaReadyGate | Should -Match 'conflict diagnostics only'
                $row.GaReadyGate | Should -Match 'post-enable rule binding evidence'
                $row.GaReadyGate | Should -Match 'no default install/repair/MSI execution'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'owned-rule-only removal'
                $row.GaReadyGate | Should -Not -Match 'owned rule evidence'
                $row.GaReadyGate | Should -Not -Match 'firewall delete limited to owned allow rule'
                $row.GaReadyGate | Should -Not -Match 'missing-rule idempotency'
                $row.GaReadyGate | Should -Not -Match 'cleanup evidence'
                $row.GaReadyGate | Should -Not -Match 'cleanup diagnostics only'
                $row.GaReadyGate | Should -Not -Match 'post-removal absence evidence'
                $row.GaReadyGate | Should -Not -Match 'exact rule identity/profile/scope'
                $row.ReleaseGate | Should -Be 'none'
                $row.NetworkExposureGate | Should -Be 'lan-exposure-approval-required'
            }
            if ($row.Name -eq 'firewall rule removal') {
                $row.GaReadyGate | Should -Match 'explicit admin opt-in'
                $row.GaReadyGate | Should -Match 'exact rule name'
                $row.GaReadyGate | Should -Match 'exact direction'
                $row.GaReadyGate | Should -Match 'exact protocol'
                $row.GaReadyGate | Should -Match 'exact local port'
                $row.GaReadyGate | Should -Match 'exact profile'
                $row.GaReadyGate | Should -Match 'exact remote address scope'
                $row.GaReadyGate | Should -Match 'owned rule evidence'
                $row.GaReadyGate | Should -Match 'owned-rule-only removal'
                $row.GaReadyGate | Should -Match 'foreign-rule conflict blocks'
                $row.GaReadyGate | Should -Match 'firewall delete limited to owned allow rule'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no eventlog mutation'
                $row.GaReadyGate | Should -Match 'no trust store mutation'
                $row.GaReadyGate | Should -Match 'missing-rule idempotency'
                $row.GaReadyGate | Should -Match 'cleanup diagnostics only'
                $row.GaReadyGate | Should -Match 'post-removal absence evidence'
                $row.GaReadyGate | Should -Match 'no default install/repair/MSI execution'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'LAN exposure approval'
                $row.GaReadyGate | Should -Not -Match 'no overwrite of existing foreign rule'
                $row.GaReadyGate | Should -Not -Match 'firewall write limited to owned allow rule'
                $row.GaReadyGate | Should -Not -Match 'post-enable rule binding evidence'
                $row.GaReadyGate | Should -Not -Match 'missing-or-owned-rule precondition'
                $row.GaReadyGate | Should -Not -Match 'cleanup evidence'
                $row.ReleaseGate | Should -Be 'none'
                $row.NetworkExposureGate | Should -Be 'none'
            }
            if ($row.Name -eq 'trust store install') {
                $row.GaReadyGate | Should -Match 'release approval'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in'
                $row.GaReadyGate | Should -Match 'exact certificate source artifact'
                $row.GaReadyGate | Should -Match 'artifact hash evidence'
                $row.GaReadyGate | Should -Match 'exact certificate identity/thumbprint'
                $row.GaReadyGate | Should -Match 'subject/issuer/serial validity evidence'
                $row.GaReadyGate | Should -Match 'LocalMachine Root/TrustedPublisher exact store/location'
                $row.GaReadyGate | Should -Match 'ADR-0003 internal trust policy binding'
                $row.GaReadyGate | Should -Match 'internal/public trust model separation'
                $row.GaReadyGate | Should -Match 'missing-or-owned-certificate precondition'
                $row.GaReadyGate | Should -Match 'subject collision diagnostics'
                $row.GaReadyGate | Should -Match 'no overwrite of existing foreign certificate'
                $row.GaReadyGate | Should -Match 'certificate store write limited to approved certificate'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no firewall mutation'
                $row.GaReadyGate | Should -Match 'no eventlog mutation'
                $row.GaReadyGate | Should -Match 'thumbprint/store binding evidence'
                $row.GaReadyGate | Should -Match 'post-install trust binding evidence'
                $row.GaReadyGate | Should -Match 'no default install/repair/MSI execution'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'owned-certificate-only removal'
                $row.GaReadyGate | Should -Not -Match 'missing-certificate idempotency'
                $row.GaReadyGate | Should -Not -Match 'LocalMachine Root/TrustedPublisher scope'
                $row.GaReadyGate | Should -Not -Match 'exact store/location match'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.NetworkExposureGate | Should -Be 'none'
            }
            if ($row.Name -eq 'trust store removal') {
                $row.GaReadyGate | Should -Match 'release approval'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in'
                $row.GaReadyGate | Should -Match 'exact certificate identity/thumbprint'
                $row.GaReadyGate | Should -Match 'subject/issuer/serial validity evidence'
                $row.GaReadyGate | Should -Match 'LocalMachine Root/TrustedPublisher exact store/location'
                $row.GaReadyGate | Should -Match 'owned certificate evidence'
                $row.GaReadyGate | Should -Match 'thumbprint/store binding evidence'
                $row.GaReadyGate | Should -Match 'owned-certificate-only removal'
                $row.GaReadyGate | Should -Match 'foreign certificate conflict blocks'
                $row.GaReadyGate | Should -Match 'certificate store delete limited to owned certificate'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no firewall mutation'
                $row.GaReadyGate | Should -Match 'no eventlog mutation'
                $row.GaReadyGate | Should -Match 'missing-certificate idempotency'
                $row.GaReadyGate | Should -Match 'cleanup diagnostics only'
                $row.GaReadyGate | Should -Match 'post-removal absence evidence'
                $row.GaReadyGate | Should -Match 'no default install/repair/MSI execution'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'LocalMachine Root/TrustedPublisher scope'
                $row.GaReadyGate | Should -Not -Match 'exact certificate source artifact'
                $row.GaReadyGate | Should -Not -Match 'artifact hash evidence'
                $row.GaReadyGate | Should -Not -Match 'subject collision diagnostics'
                $row.GaReadyGate | Should -Not -Match 'no overwrite of existing foreign certificate'
                $row.GaReadyGate | Should -Not -Match 'certificate store write limited to approved certificate'
                $row.GaReadyGate | Should -Not -Match 'post-install trust binding evidence'
                $row.GaReadyGate | Should -Not -Match 'cleanup evidence'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.NetworkExposureGate | Should -Be 'none'
            }

            $releaseApprovalRows = @('local payload update', 'rollback restore', 'trust store install', 'trust store removal')
            if ($row.ReleaseGate -eq 'release-approval-required') {
                $releaseApprovalRows | Should -Contain $row.Name
            }
            if ($releaseApprovalRows -contains $row.Name) {
                $row.ReleaseGate | Should -Be 'release-approval-required'
            }

            if ($row.Name -eq 'local payload update') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'windows-native-package'
                $row.ImplementationBasis | Should -Be 'package-contract'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'internal `RequireSigned` stable `0.35.3` payload update from stable `0.35.2`'
                $row.GaReadyGate | Should -Match 'schema v1 payload manifest/version match'
                $row.GaReadyGate | Should -Match 'payload manifest/version match'
                $row.GaReadyGate | Should -Match 'staged source outside active root'
                $row.GaReadyGate | Should -Match 'binary payload activation'
                $row.GaReadyGate | Should -Match 'no config/data/token/service identity mutation'
                $row.GaReadyGate | Should -Match 'service stop/start health'
                $row.GaReadyGate | Should -Match 'stable-internal-release-update-rollback-20260505-015550-0352-0353'
                $row.GaReadyGate | Should -Not -Match 'config migration dry-run'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'manifest/payload version match'
                $row.GaReadyGate | Should -Not -Match 'product config schema validation pass required'
            }
            if ($row.Name -eq 'rollback restore') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'windows-native-package'
                $row.ImplementationBasis | Should -Be 'package-contract'
                $row.FallbackPolicy | Should -Be 'none'
                $row.PromotionState | Should -Be 'current-native'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'retained previous root stable `0.35.2` manifest/hash validation'
                $row.GaReadyGate | Should -Match 'rollback from `0.35.3`'
                $row.GaReadyGate | Should -Match 'failed root diagnostics preservation'
                $row.GaReadyGate | Should -Match 'post-rollback manifest/version/health'
                $row.GaReadyGate | Should -Match 'data/token preservation'
                $row.GaReadyGate | Should -Match 'stable-internal-release-update-rollback-20260505-015550-0352-0353'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'staged rollback activation'
            }

            if ($row.NetworkExposureGate -eq 'lan-exposure-approval-required') {
                @('firewall rule enable LAN exposure', '`GET WebSocket /api/v1/console/novnc/{vm_id}`') | Should -Contain $row.Name
            }
            if ($row.Name -eq 'firewall rule enable LAN exposure') {
                $row.NetworkExposureGate | Should -Be 'lan-exposure-approval-required'
            }
            if ($row.Name -eq '`GET WebSocket /api/v1/console/novnc/{vm_id}`') {
                $row.NetworkExposureGate | Should -Be 'lan-exposure-approval-required'
            }
            if ($row.Name -eq 'firewall rule removal') {
                $row.NetworkExposureGate | Should -Be 'none'
            }

            switch ($row.RiskTier) {
                'tier1-read-only' { @('none', 'installed-non-mutating') | Should -Contain $row.AdminSmokeRequired }
                'tier2-reversible-mutation' { $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in' }
                'tier3-destructive-or-persistent' { $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in' }
                default { throw "Unexpected risk_tier '$($row.RiskTier)' in $($row.Name)" }
            }

            switch ($row.PromotionState) {
                'current-native' { @('none', 'test-only') | Should -Contain $row.FallbackPolicy }
                'transition-helper' { $row.FallbackPolicy | Should -Be 'transition-helper' }
                'blocked' { $row.FallbackPolicy | Should -Be 'blocked' }
                'ga-ready-candidate' { @('none', 'test-only') | Should -Contain $row.FallbackPolicy }
                default { throw "Unexpected promotion_state '$($row.PromotionState)' in $($row.Name)" }
            }
        }

        $repoMigration | Should -Match 'spikes/purecvisor-desktop-node/hyperv/\*\*'
        $repoMigration | Should -Match 'src/DesktopNode.HyperV/\*\*'
        $repoMigration | Should -Match 'archive/spikes/purecvisor-desktop-node/hyperv/\*\*'
        $repoMigration | Should -Match 'behavior 변경과 분리'
        $repoMigration | Should -Match '승인 시 목표 상태'
        $repoMigration | Should -Match 'physical archive move opt-in'
        $repoMigration | Should -Match '`git mv` 이동'
        $repoMigration | Should -Match 'rollback 기준'
        $repoMigration | Should -Match 'archive target 검증'
        $repoMigration | Should -Match 'Phase 26 alignment 첫 slice 당시에는 파일 이동을 하지 않았다'
        $repoMigration | Should -Match '2026-05-03 Web Console served asset/root migration slice'
        $repoMigration | Should -Match 'spikes/purecvisor-desktop-node/web/\*\*'
        $repoMigration | Should -Match 'web/\*\*'
        $repoMigration | Should -Match 'active product target으로 이동됨'
        $repoMigration | Should -Match 'web/src/served-app\.ts'
        $repoMigration | Should -Match 'web/app\.js'
        $repoMigration | Should -Match 'build:served'
        $repoMigration | Should -Match 'check:served'
        $repoMigration | Should -Match 'source path inventory'
        $repoMigration | Should -Match 'import/relative path graph'
        $repoMigration | Should -Match 'packaging/static asset input binding'
        $repoMigration | Should -Match 'generated parity manifest update'
        $repoMigration | Should -Match 'docs command update'
        $repoMigration | Should -Match 'no behavior change evidence'
        $repoMigration | Should -Match 'archive target read-only intent'
        $repoMigration | Should -Match 'rollback restore 기준'
        $repoMigration | Should -Match '관련 Pester/npm/`verify:parity`/`node --check` evidence'
        $repoMigration | Should -Match '이동 후 inventory'

        $verificationOwnership | Should -Match 'xUnit'
        $verificationOwnership | Should -Match 'browser-level fixture 후보'
        $verificationOwnership | Should -Match 'npm/package-owned'
        $verificationOwnership | Should -Match 'loopback fixture'
        $verificationOwnership | Should -Match 'static asset load'
        $verificationOwnership | Should -Match 'initial render'
        $verificationOwnership | Should -Match 'deterministic `GET /api/v1/runtime/policy` connection'
        $verificationOwnership | Should -Match 'optional bearer 401/200 handling'
        $verificationOwnership | Should -Match 'token/redaction 확인'
        $verificationOwnership | Should -Match '제외 범위'
        $verificationOwnership | Should -Match 'API route contract'
        $verificationOwnership | Should -Match 'route parity'
        $verificationOwnership | Should -Match 'service/MSI/firewall/Event Log/trust store mutation'
        $verificationOwnership | Should -Match 'LAN exposure'
        $verificationOwnership | Should -Match 'Playwright required dependency'
        $verificationOwnership | Should -Match '후속 도구 후보'
        $verificationOwnership | Should -Match 'required dependency가 아니다'
        $verificationOwnership | Should -Match 'Pester는 PowerShell component/runtime behavior suite'
        $verificationOwnership | Should -Match '## Pester Retirement Gate'
        $verificationOwnership | Should -Match 'active required command는 product-owned package/xUnit/npm 검증'
        $verificationOwnership | Should -Match 'Legacy Pester suite는 component/archive baseline'
        $verificationOwnership | Should -Match '대체 xUnit/npm/package/browser fixture evidence'
        $verificationOwnership | Should -Match 'owner replacement'
        $verificationOwnership | Should -Match 'equivalent coverage mapping'
        $verificationOwnership | Should -Match 'archive baseline path'
        $verificationOwnership | Should -Match 'docs command update'
        $verificationOwnership | Should -Match 'CI/local command replacement'
        $verificationOwnership | Should -Match 'rollback 기준'
        $verificationOwnership | Should -Match 'PowerShell helper 또는 `spikes/\*\*`가 active product path'
        $verificationOwnership | Should -Match 'component/archive baseline으로 분리'
        $verificationOwnership | Should -Match '## Default Command Ownership'
        $verificationOwnership | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"
        $verificationOwnership | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'"
        $verificationOwnership | Should -Match "Invoke-Pester -Path 'web/tests'"
        $verificationOwnership | Should -Match 'dotnet test src/DesktopNode\.sln'
        $verificationOwnership | Should -Match 'git diff --check'
        $verificationOwnership | Should -Match '## Component/Archive Baseline'
        $verificationOwnership | Should -Match 'archive/spikes/purecvisor-desktop-node/api/tests'
        $verificationOwnership | Should -Match 'excluded from default required command'
        $verificationOwnership | Should -Match 'PCV_POST_REBOOT_PROFILE_RETIRED'
        $verificationOwnership | Should -Match 'Root documentation guard'
        $verificationOwnership | Should -Match 'no-auto-reboot'
        $verificationOwnership | Should -Match '## Diagnostics and Redaction Boundary'
        $verificationOwnership | Should -Match 'diagnostics bundle manifest'
        $verificationOwnership | Should -Match 'events\.jsonl'
        $verificationOwnership | Should -Match 'install\.jsonl'
        $verificationOwnership | Should -Match 'bearer token'
        $verificationOwnership | Should -Match 'API token'
        $verificationOwnership | Should -Match 'Authorization'
        $verificationOwnership | Should -Match 'api-token\.dpapi\.json'
        $verificationOwnership | Should -Match 'private key'
        $verificationOwnership | Should -Match 'PFX password'
        $verificationOwnership | Should -Match 'certificate'
        $verificationOwnership | Should -Match '\[REPO_ROOT\]'
        $verificationOwnership | Should -Match '\[DATA_ROOT\]'
        $verificationOwnership | Should -Match '## Data Root Lifecycle Boundary'
        $verificationOwnership | Should -Match 'Program Files product root lifecycle'
        $verificationOwnership | Should -Match 'ProgramData data root lifecycle'
        $verificationOwnership | Should -Match '기본 uninstall은 ProgramData data root를 보존'
        $verificationOwnership | Should -Match 'Repair는 protected token file'
        $verificationOwnership | Should -Match 'legacy raw token file'
        $verificationOwnership | Should -Match 'job store'
        $verificationOwnership | Should -Match 'events\.jsonl'
        $verificationOwnership | Should -Match 'install\.jsonl'
        $verificationOwnership | Should -Match 'diagnostics directory'
        $verificationOwnership | Should -Match 'REMOVE_DATA=1'
        $verificationOwnership | Should -Match 'RemoveData'
        $verificationOwnership | Should -Match 'api-token\.dpapi\.json'
        $verificationOwnership | Should -Match 'api-token\.txt'
        $verificationOwnership | Should -Match 'jobs\.json'
        $verificationOwnership | Should -Match 'Service host log directory'
        $verificationOwnership | Should -Match 'WiX는 ProgramData path 계산만 담당'
        $verificationOwnership | Should -Match 'data-root ACL을 직접 소유하지 않는다'
        $verificationOwnership | Should -Match 'data_acl'
        $verificationOwnership | Should -Match 'SYSTEM/Administrators boundary'
        $verificationOwnership | Should -Match 'ACL repair'

        $adrIndex | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adrIndex | Should -Match 'DESKTOP_NODE_SERVICE_DISTRIBUTION_DECISION: internal-only-service'
        $adrIndex | Should -Match '## 현재 적용 중인 ADR'
        $adrIndex | Should -Match '## 제안 중인 ADR 후보'
        $adrIndex | Should -Match '0004-ga-ready-product-runtime-candidate'
        $adrIndex | Should -Match '현재 제안 중인 ADR 후보는 없다'
        $developerIndex | Should -Match '내부 전용 GA-ready 제품 런타임 결정 확인'
        $guide | Should -Match '내부 전용 GA-ready 제품 런타임 결정'
        $roadmap | Should -Match 'Phase 26'
        $roadmap | Should -Match '완료/적용'
        $roadmap | Should -Match 'route promotion matrix'
        $follower | Should -Match '내부 전용 GA-ready 제품 런타임 유지'
        $follower | Should -Match 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'
    }

    It 'records the GA-ready repo migration preflight archive move evidence' {
        $preflightPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/repo-migration-preflight-2026-05-04.md'

        Test-Path -LiteralPath $preflightPath | Should -BeTrue

        $preflight = Get-Content -LiteralPath $preflightPath -Raw
        $preflight | Should -Match 'evidence_id: repo-migration-preflight-2026-05-04'
        $preflight | Should -Match 'migration_status: pass'
        $preflight | Should -Match 'migration_status_reason: physical-archive-move-executed-2026-05-05'
        $preflight | Should -Match 'physical_spikes_file_count: 46'
        $preflight | Should -Match 'active_product_path_count_initial: 61'
        $preflight | Should -Match 'active_product_path_count_current: 0'
        $preflight | Should -Match 'component_archive_spikes_reference_count_current: 0'
        $preflight | Should -Match 'component_archive_path_reference_count_current: 22'
        $preflight | Should -Match 'installer_payload_spike_source_count: 0'
        $preflight | Should -Match 'standalone_product_asset_spike_source_count: 0'
        $preflight | Should -Match 'post_reboot_active_spike_command_count: 0'
        $preflight | Should -Match 'docs_required_spike_command_count: 0'
        $preflight | Should -Match 'verification_ownership_map_updated: yes'
        $preflight | Should -Match 'verification_ownership_replacement_status: pass'
        $preflight | Should -Match 'archive_readonly_rollback_evidence_status: pass'
        $preflight | Should -Match 'archive_inventory_path: docs/ga-ready/evidence/archive-spikes-inventory-2026-05-04.json'
        $preflight | Should -Match 'postmove_inventory_path: docs/ga-ready/evidence/archive-spikes-inventory-postmove-2026-05-05.json'
        $preflight | Should -Match 'packaging/windows-desktop-node/installer/build.ps1'
        $preflight | Should -Match 'MSI payload는 product wrapper'
        $preflight | Should -Match 'PcvPostRebootVerification.psm1'
        $preflight | Should -Match 'HyperVNonIntegration'
        $preflight | Should -Match 'PCV_POST_REBOOT_PROFILE_RETIRED'
        $preflight | Should -Match 'Invoke-PcvRouteParityMutationSmoke.ps1'
        $preflight | Should -Match 'Route parity mutation smoke의 spike service module import'
        $preflight | Should -Match 'MSI installer payload spike staging'
        $preflight | Should -Match 'standalone product wrapper asset copy boundary'
        $preflight | Should -Match 'PcvDesktopNodeProduct.Invoke.Tests.ps1'
        $preflight | Should -Match 'PcvDesktopNodeProduct.Plan.Tests.ps1'
        $preflight | Should -Match 'docs command update'
        $preflight | Should -Match 'archive/spikes/\*\*'
        $preflight | Should -Match 'source path는 absent'
        $preflight | Should -Match '`git mv` 이동'
        $preflight | Should -Match 'pass'
    }

    It 'keeps active required verification command docs off spike paths' {
        $docs = @(
            'AGENTS.md',
            'README.md',
            'docs/DEVELOPMENT_VERIFICATION_POLICY.md',
            'docs/PUBLIC_RELEASE_BOUNDARY.md',
            'follower.md'
        )

        foreach ($relativePath in $docs) {
            $text = Get-Content -LiteralPath (Join-Path $script:RepoRoot $relativePath) -Raw
            $text | Should -Not -Match "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node"
        }

        $policy = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md') -Raw
        $policy | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"
        $policy | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'"
        $policy | Should -Match "Invoke-Pester -Path 'web/tests'"
        $policy | Should -Match 'Component/archive baseline'
    }

    It 'records the GA-ready evidence ledger and aggregate closure blockers' {
        $ledgerPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md'
        $closurePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md'
        $stableReleasePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/stable-internal-release-update-rollback-2026-05-05.md'

        Test-Path -LiteralPath $ledgerPath | Should -BeTrue
        Test-Path -LiteralPath $closurePath | Should -BeTrue
        Test-Path -LiteralPath $stableReleasePath | Should -BeTrue

        $ledger = Get-Content -LiteralPath $ledgerPath -Raw
        $closure = Get-Content -LiteralPath $closurePath -Raw
        $stableRelease = Get-Content -LiteralPath $stableReleasePath -Raw

        $ledger | Should -Match 'evidence_id: service-action-status-start-stop-20260504'
        $ledger | Should -Match 'route_or_operation: service status'
        $ledger | Should -Match 'route_or_operation: service stop'
        $ledger | Should -Match 'route_or_operation: service start'
        $ledger | Should -Match 'artifact_or_package_version: artifacts/service-action-status-start-stop-20260504-002359'
        $ledger | Should -Match 'target_owner: dotnet-service-action'
        $ledger | Should -Match 'implementation_basis: windows-native-api'
        $ledger | Should -Match 'evidence_id: job-store-schema-mismatch-blocked-diagnostics-20260504'
        $ledger | Should -Match 'route_or_operation: job store schema mismatch detection'
        $ledger | Should -Match 'promotion_state: current-native'
        $ledger | Should -Match 'result: pass'
        $ledger | Should -Match 'waiver_status: none'

        $closure | Should -Match 'report_id: aggregate-gate-closure-2026-05-05'
        $closure | Should -Match 'ga_scope_current_route_count: 18'
        $closure | Should -Match 'ga_scope_product_operation_count: 22'
        $closure | Should -Match 'future_route_exclusion_count: 2'
        $closure | Should -Match 'transition_helper_count: 0'
        $closure | Should -Match 'blocked_count: 0'
        $closure | Should -Match 'powershell_current_owner_count: 0'
        $closure | Should -Match 'powershell_fallback_count: 0'
        $closure | Should -Match 'active_spikes_path_count: 0'
        $closure | Should -Match 'component_archive_spikes_reference_count: 22'
        $closure | Should -Match 'repo_migration_preflight_status: pass'
        $closure | Should -Match 'docs_command_update_status: pass'
        $closure | Should -Match 'verification_ownership_replacement_status: pass'
        $closure | Should -Match 'archive_readonly_rollback_evidence_status: pass'
        $closure | Should -Match 'tier2_admin_evidence_status: pass'
        $closure | Should -Match 'tier3_admin_evidence_status: pass'
        $closure | Should -Match 'release_gated_prerelease_evidence_status: pass'
        $closure | Should -Match 'lan_gated_preapproval_evidence_status: pass'
        $closure | Should -Match 'stable_internal_release_update_rollback_status: pass'
        $closure | Should -Match 'public_trusted_signing: excluded'
        $closure | Should -Match 'external_stable_publication: not-claimed'
        $closure | Should -Match 'stale_evidence_count: 0'
        $closure | Should -Match 'waiver_only_gate_satisfaction_count: 0'
        $closure | Should -Match 'aggregate_gate_status: closed'

        $stableRelease | Should -Match 'evidence_id: stable-internal-release-update-rollback-2026-05-05'
        $stableRelease | Should -Match 'artifact_root: artifacts/stable-internal-release-update-rollback-20260505-015550-0352-0353'
        $stableRelease | Should -Match 'release_version: 0.35.2'
        $stableRelease | Should -Match 'update_payload_version: 0.35.3'
        $stableRelease | Should -Match 'rollback_target_version: 0.35.2'
        $stableRelease | Should -Match 'trust_model: InternalEnterprise'
        $stableRelease | Should -Match 'public_trusted_signing: excluded'
        $stableRelease | Should -Match 'external_stable_publication: not-claimed'
        $stableRelease | Should -Match 'execution_status: pass'
        $stableRelease | Should -Match 'rollback_final_state_status: pass'
        $stableRelease | Should -Match 'Final active product version: `0.35.2`'
        $stableRelease | Should -Match 'Final failed root version: `0.35.3`'
        $stableRelease | Should -Match 'Active product root legacy WinSW root files: none'
    }

    It 'records archive read-only rollback evidence after moving files' {
        $archiveEvidencePath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/archive-readonly-rollback-2026-05-04.md'

        Test-Path -LiteralPath $archiveEvidencePath | Should -BeTrue

        $archiveEvidence = Get-Content -LiteralPath $archiveEvidencePath -Raw
        $archiveEvidence | Should -Match 'evidence_id: archive-readonly-rollback-2026-05-04'
        $archiveEvidence | Should -Match 'archive_status: physical-archive-read-only'
        $archiveEvidence | Should -Match 'file_move_execution: pass-2026-05-05'
        $archiveEvidence | Should -Match 'archive_write_execution: pass-2026-05-05'
        $archiveEvidence | Should -Match 'rollback_restore_status: proof-defined'
        $archiveEvidence | Should -Match 'source_inventory_status: pass'
        $archiveEvidence | Should -Match 'hash_inventory_status: pass'
        $archiveEvidence | Should -Match 'archive-spikes-inventory-2026-05-04.json'
        $archiveEvidence | Should -Match 'archive-spikes-inventory-postmove-2026-05-05.json'
        $archiveEvidence | Should -Match 'public_trusted_signing: excluded'
        $archiveEvidence | Should -Match 'archive/spikes/\*\*'
        $archiveEvidence | Should -Match 'product execution source로 사용할 수 없다'
        $archiveEvidence | Should -Match 'packaging input으로 사용할 수 없다'
        $archiveEvidence | Should -Match 'required verification command source로 사용할 수 없다'
        $archiveEvidence | Should -Match 'git tracked restore'
        $archiveEvidence | Should -Match 'hash inventory'
        $archiveEvidence | Should -Match 'no behavior change'
        $archiveEvidence | Should -Match 'source path absent'
        $archiveEvidence | Should -Match 'archived file count는 46개'
    }

    It 'records release LAN and OS gated preapproval evidence separately from execution approval' {
        $preapprovalPath = Join-Path $script:RepoRoot 'docs/ga-ready/evidence/release-lan-os-gated-preapproval-2026-05-04.md'

        Test-Path -LiteralPath $preapprovalPath | Should -BeTrue

        $preapproval = Get-Content -LiteralPath $preapprovalPath -Raw
        $preapproval | Should -Match 'evidence_id: release-lan-os-gated-preapproval-2026-05-04'
        $preapproval | Should -Match 'public_trusted_signing: excluded'
        $preapproval | Should -Match 'release_execution: not-approved'
        $preapproval | Should -Match 'lan_exposure_execution: not-approved'
        $preapproval | Should -Match 'os_mutation_execution: scoped-opt-in-recorded'
        $preapproval | Should -Match 'followup_stable_internal_release_execution: pass-2026-05-05'
        $preapproval | Should -Match 'followup_scoped_lan_exposure_execution: pass-2026-05-05'
        $preapproval | Should -Match 'followup_aggregate_gate_status: closed-candidate-2026-05-05'
        $preapproval | Should -Match 'release_gated_prerelease_evidence_status: pass'
        $preapproval | Should -Match 'lan_gated_preapproval_evidence_status: pass'
        $preapproval | Should -Match 'local payload update'
        $preapproval | Should -Match 'rollback restore'
        $preapproval | Should -Match 'trust store install'
        $preapproval | Should -Match 'trust store removal'
        $preapproval | Should -Match 'firewall rule enable LAN exposure'
        $preapproval | Should -Match 'Event Log source registration'
        $preapproval | Should -Match 'Event Log source removal'
        $preapproval | Should -Match 'firewall rule removal'
        $preapproval | Should -Match 'allowed_pre_release_evidence'
        $preapproval | Should -Match 'forbidden_execution'
        $preapproval | Should -Match 'no-auto-reboot'
        $preapproval | Should -Match 'aggregate_gate_effect: blocked'
        $preapproval | Should -Match 'Stable Internal Release/Update/Rollback 실행'
        $preapproval | Should -Match 'stable-internal-release-update-rollback-20260505-015550-0352-0353'
    }
}
