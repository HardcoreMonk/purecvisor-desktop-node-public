# PureCVisor Desktop Node GA-ready Phase 26 Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-05-02 당시 GA-ready 제품 재설계의 첫 구현 slice로 ADR-0001 supersede 후보, route promotion matrix, repo migration map, verification ownership map을 문서와 root documentation guard로 고정한다.

**Architecture:** 이 plan은 behavior change가 아니라 documentation/contract alignment였고, 당시 적용 결정은 ADR index의 `keep-spike`가 우선했다. 2026-05-05 이후 현재 적용 결정은 ADR-0004의 `ga-ready-product-runtime`/`internal-only-service`이며, 이 plan 본문은 후보 시점의 이력으로 보존한다.

**Tech Stack:** Markdown, PowerShell 7, Pester 5, existing Desktop Node root documentation suite, git diff check.

---

## 현행화 메모

이 plan 본문은 2026-05-02 GA-ready alignment 첫 문서/contract slice의 실행 기록을 보존한다. 이후 2026-05-03 후속 slice에서 Web Console 제품 경로는 repo-root `web/**`로 이동했고 served `web/app.js`는 `web/src/served-app.ts` build output이 됐다. Tier 1 read route의 PowerShell helper fallback은 제거됐으며 checkpoint create/restore/delete는 C# native mutation adapter로 전환됐다. 2026-05-05 aggregate closure 이후 ADR-0004가 내부 전용 서비스 current decision으로 적용됐다. 현재 단일 진실은 `docs/ADR_INDEX.md`, `docs/adr/0004-ga-ready-product-runtime-candidate.md`, `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/ga-ready/REPO_MIGRATION_MAP.md`, `docs/ga-ready/VERIFICATION_OWNERSHIP.md`와 active entrypoint 문서다.

## Scope

이 plan은 `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`의 첫 구현 slice만 실행한다.

포함:

- ADR-0001을 대체하려는 `상태: 제안` ADR 후보 추가
- route promotion matrix 문서 추가
- repo migration map 문서 추가
- verification ownership map 문서 추가
- high-level docs와 root documentation tests 연결

제외:

- PowerShell helper 제거
- route 구현 변경
- `spikes/**` 파일 이동
- MSI install/repair/uninstall 같은 관리자 mutation
- stable release 발행
- Playwright 또는 다른 browser automation dependency 도입

## File Structure

- Create: `docs/adr/0004-ga-ready-product-runtime-candidate.md`
  - 기존 ADR 템플릿 형식으로 ADR-0001 supersede 후보를 기록한다. 적용 전까지 현재 ADR-0001이 우선임을 명시한다.
- Create: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
  - API route와 product operation별 current owner, target owner, implementation basis, risk tier, fallback policy, promotion state, GA-ready gate, release gate, network exposure gate를 고정한다.
  - Markdown 문서로 유지하되 field schema와 enum 허용값을 명시한다.
- Create: `docs/ga-ready/REPO_MIGRATION_MAP.md`
  - `spikes/**`에서 `src/**`, `web/**`, `archive/**`로 이동할 target map을 고정한다.
- Create: `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
  - Pester 중심 legacy 검증에서 xUnit/npm/browser-level fixture 후보/package contract 중심으로 이동하는 ownership map을 고정한다.
  - Playwright는 이 plan의 target verification이 아니라 후속 browser-level fixture 구현 slice의 도구 후보로만 기록한다.
- Modify: `docs/ADR_INDEX.md`
  - ADR-0004를 `현재 적용 중인 ADR` 표에 넣지 않는다.
  - 별도 `제안 중인 ADR 후보` 섹션을 만들고 ADR-0004와 `docs/ga-ready/**` supporting docs를 연결한다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - GA-ready alignment entrypoint를 추가한다.
- Modify: `docs/GUIDE.md`
  - 주요 진입점과 경계 요약에 GA-ready alignment docs를 추가한다.
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
  - Phase 26을 `제안/정렬 plan 작성` 상태로 추가한다.
- Modify: `follower.md`
  - 다음 우선순위 1번에 Phase 26 alignment plan과 산출물을 연결한다.
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
  - GA-ready candidate가 discoverable하되 current decision을 바꾸지 않는다는 documentation guard를 단계적으로 추가한다.

## Engineering Review Addendum

plan-eng-review 결과, 이 plan은 behavior change가 아니라 documentation/contract alignment로 유지한다. 구현 전 보강 사항은 충돌 preflight, 실행 의존성, table parser 실패 조건, 단일 writer 전략이다.

### What Already Exists

- 현재 적용 decision source는 `docs/ADR_INDEX.md`와 `docs/adr/0001-standalone-windows-repo-and-evidence-first-keep-spike.md`다.
- Phase 25 이후 기본 service host와 listener owner는 `DesktopNode.Host.exe`이며, `host.status`와 `network.inventory` 일부 native adapter evidence가 이미 있다.
- `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`는 Desktop Node root documentation guard의 현재 진입점이다.
- `docs/ga-ready/**`는 리뷰 시점에는 아직 존재하지 않으므로 이 plan의 첫 실행자가 새 supporting docs의 최초 owner가 된다.

### NOT in Scope for This Implementation Slice

- PowerShell helper 제거, route 구현 변경, `spikes/**` 파일 이동은 하지 않는다.
- MSI install/repair/uninstall, service install/start/stop/delete, firewall/Event Log/trust store mutation은 실행하지 않는다.
- machine-readable JSON, evidence ledger 파일, aggregate closure report 파일은 만들지 않는다.
- ADR-0004를 current decision으로 승격하지 않고, `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 현재 적용 source를 유지한다.

### Execution Dependency Diagram

```text
Task 0 preflight
  - ADR-0004 filename availability
  - docs/ga-ready supporting doc collision check
    |
    v
Task 1 RED documentation guard
    |
    v
Task 2 ADR-0004 candidate + spec marker alignment
    |
    v
Task 3 route promotion matrix + parser/invariant guard
    |
    v
Task 4 repo migration map + verification ownership map
    |
    v
Task 5 ADR/index/developer docs/roadmap/follower links
    |
    v
Task 6 full verification + diff review + commit
```

### Failure Modes and Stop Conditions

- ADR-0004 filename collision이 있으면 새 ADR 번호를 자동 선택하지 않고 중단한다.
- `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/ga-ready/REPO_MIGRATION_MAP.md`, `docs/ga-ready/VERIFICATION_OWNERSHIP.md`, `docs/ga-ready/evidence` 중 하나라도 이미 있으면 중단하고 기존 문서의 reuse/rename/archive 결정을 받는다.
- Task 1 RED가 ADR 후보 파일 missing 이외의 이유로 실패하면 중단한다.
- route matrix parser가 row를 하나도 찾지 못하거나 duplicate `Route/Operation` identity를 발견하면 중단한다.
- `docs/ADR_INDEX.md`가 ADR-0004를 current ADR 표에 넣거나 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 잃으면 중단한다.
- 첫 slice에서 evidence ledger 또는 aggregate closure report 파일이 생기면 scope violation으로 중단한다.

### Test and Parser Guard Review

- RED/GREEN 순서는 유지한다. 먼저 missing ADR candidate로 실패시키고, 이후 ADR/spec/matrix/supporting docs를 추가하면서 같은 documentation guard를 확장한다.
- Markdown table parser는 machine-readable JSON을 만들지 않고 field schema enum과 table row를 직접 파싱한다.
- Parser guard는 row count, duplicate route identity, enum membership, state/fallback invariant, risk/admin smoke invariant를 모두 실패 조건으로 둔다.
- Root documentation suite와 `git diff --check`는 이 plan의 기본 완료 검증이다.

### Worktree and Parallelization Strategy

- 권장 실행 방식은 단일 writer다. 이 slice는 같은 Pester file과 같은 docs index를 반복 수정하므로 병렬 worker가 write conflict를 만들 가능성이 높다.
- 병렬화가 필요하면 read-only 검토만 분리한다. 예를 들면 한 worker는 generated route matrix row consistency를 읽기 전용으로 검토하고, 다른 worker는 repo migration/verification map copy를 읽기 전용으로 검토한다.
- 실제 파일 생성과 Pester test 교체는 한 작업자가 순서대로 수행한다.

## Task 0: Check ADR Number and Supporting Doc Availability

**Files:**

- Read: `docs/adr/0004-ga-ready-product-runtime-candidate.md`
- Read: `docs/ADR_INDEX.md`
- Read: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- Read: `docs/ga-ready/REPO_MIGRATION_MAP.md`
- Read: `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
- Read: `docs/ga-ready/evidence`

- [ ] **Step 1: Confirm ADR-0004 is available**

Run:

```powershell
Test-Path -LiteralPath 'docs/adr/0004-ga-ready-product-runtime-candidate.md'
```

Expected:

```text
False
```

- [ ] **Step 2: Confirm GA-ready supporting docs are not already claimed**

Run:

```powershell
$gaReadyTargets = @(
    'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md',
    'docs/ga-ready/REPO_MIGRATION_MAP.md',
    'docs/ga-ready/VERIFICATION_OWNERSHIP.md',
    'docs/ga-ready/evidence'
)

$gaReadyTargets | ForEach-Object {
    [pscustomobject]@{
        Path = $_
        Exists = Test-Path -LiteralPath $_
    }
}
```

Expected:

```text
Path                                             Exists
----                                             ------
docs/ga-ready/ROUTE_PROMOTION_MATRIX.md          False
docs/ga-ready/REPO_MIGRATION_MAP.md              False
docs/ga-ready/VERIFICATION_OWNERSHIP.md          False
docs/ga-ready/evidence                           False
```

- [ ] **Step 3: Stop on collision**

If the ADR availability command returns `True`, stop. Do not overwrite the existing file and do not choose a new ADR number automatically. Read `docs/ADR_INDEX.md`, report the collision, and ask the user which ADR number to use.

If any GA-ready target file already exists, or if `docs/ga-ready/evidence` already exists, stop. Do not overwrite or merge automatically. Read the existing path and ask whether to reuse, rename, or archive it. An existing empty `docs/ga-ready` directory by itself is not a collision.

## Task 1: Add Minimal RED Guard For ADR Candidate

**Files:**

- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [ ] **Step 1: Add the initial failing Pester test**

Open `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`. Append this `It` block inside the existing `Describe 'Desktop Node runtime promotion boundary'` block, after the current ADR index test:

```powershell
    It 'documents the GA-ready product runtime candidate without changing the current decision' {
        $adrCandidatePath = Join-Path $script:RepoRoot 'docs/adr/0004-ga-ready-product-runtime-candidate.md'

        Test-Path -LiteralPath $adrCandidatePath | Should -BeTrue
    }
```

- [ ] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1' -FullName '*GA-ready product runtime candidate*' -Output Detailed"
```

Expected:

```text
Failed tests:
[-] documents the GA-ready product runtime candidate without changing the current decision
Expected $true, but got $false.
```

The only expected failure is that `docs/adr/0004-ga-ready-product-runtime-candidate.md` does not exist yet.

## Task 2: Add Proposed ADR-0004

**Files:**

- Create: `docs/adr/0004-ga-ready-product-runtime-candidate.md`
- Modify: `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [ ] **Step 1: Create the ADR candidate**

Create `docs/adr/0004-ga-ready-product-runtime-candidate.md` with exactly this content:

~~~~markdown
# ADR-0004: GA-ready 제품 런타임 후보

- 상태: 제안
- 날짜: 2026-05-02
- 결정 마커:
  - `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime`
  - `DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime`
- 대체 대상: 승인 시 ADR-0001 대체

## 맥락

Desktop Node는 Phase 25에서 `DesktopNode.Host.exe`를 기본 service host와 listener owner로 교체했고, 후속 slice에서 Tier 1 read route, VM create/start/shutdown/poweroff/restart/delete, checkpoint create/restore/delete를 C# native adapter product path로 전환했다. `0.30.1-admin-smoke` installed destructive smoke는 VM delete managed/delete-repeat/unmanaged-guard evidence를 추가했다. 그러나 product ops mutation에는 아직 PowerShell-backed/mixed-history 경계가 남아 있고, `spikes/purecvisor-desktop-node/{api,hyperv,service,cli}/**`는 active component/adapter 검증 경계이며, 일부 Pester 중심 legacy 검증도 남아 있다.

GA-ready 제품 runtime이 되려면 Windows 관리자가 설치, 수리, 삭제, 업데이트, 진단, 복구를 하나의 제품 경계로 이해할 수 있어야 한다. 이 ADR 후보의 승인 시 목표 상태는 PowerShell-free product ops/runtime이다.

이 ADR은 현재 적용 결정이 아니다. 적용 전까지 `docs/ADR_INDEX.md`의 현재 적용 ADR과 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`가 우선한다.

## 결정

승인 시 Desktop Node의 제품 승격 목표를 `ga-ready-product-runtime`으로 바꾼다.

GA-ready 기준:

- 제품 runtime/request path에서 PowerShell helper dependency를 제거한다.
- 제품 배포/운영 경로에서 PowerShell dependency를 제거한다.
- Hyper-V 조작은 C# WMI/CIM adapter 중심으로 전환한다.
- `spikes/**`는 활성 제품 경로에서 제거하거나 `archive/**`로 이동한다.

Stable publication, public trusted signing, external release는 이 ADR 승인만으로 실행하지 않는다. 별도 GA gate와 release approval이 필요하다.

## GA gate와 release gate 분리

이 ADR 후보가 승인되어도 stable publication, public trusted signing, external release는 실행하지 않는다.

- GA-ready product runtime: 제품 runtime/ops/repo/test architecture가 GA 가능한 형태인지 판단한다.
- Release execution: selected trust model, signed stable MSI lifecycle, update/rollback compatibility, publication target, release notes를 별도 승인한다.

## Aggregate GA-ready Decision Gate

ADR-0004를 current decision으로 승격하기 전에는 다음 aggregate gate가 닫혀야 한다.

- GA 범위의 `current-route`와 `product-operation` row는 `promotion_state = transition-helper` 또는 `promotion_state = blocked`가 0개여야 한다.
- `future-route` row는 GA 범위 제외 사유와 별도 implementation plan requirement를 명시해야 한다.
- 제품 runtime/request path에는 PowerShell helper가 없어야 한다.
- 활성 제품 경로에는 `spikes/**`가 없어야 한다.
- repo migration preflight evidence와 verification ownership replacement evidence가 완료되어야 한다.
- `tier2-reversible-mutation`과 `tier3-destructive-or-persistent` row는 explicit admin opt-in evidence가 완료되어야 하며, Evidence Freshness Rule을 만족하지 않는 stale evidence는 aggregate GA-ready gate 충족에 사용할 수 없다.
- `release_gate = release-approval-required` row는 GA-ready 판정과 release execution을 분리하며, 별도 release approval 전에는 실행하지 않는다.

이 aggregate gate가 닫히기 전에는 ADR-0004를 current decision으로 승격하지 않는다.

## Aggregate Gate Closure Report

ADR-0004 current decision 승격 PR은 `docs/ga-ready/evidence/aggregate-gate-closure-<YYYY-MM-DD>.md` Markdown closure report를 포함해야 한다.
이 report가 `aggregate_gate_status = closed`일 때만 승격할 수 있다.
첫 Phase 26 alignment slice에서는 closure report를 만들지 않는다.

## ADR-0001 Replacement Scope

ADR-0004 승인 시 대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단이다.
`DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`와 `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`는 별도 ADR이 바꾸기 전까지 현재 적용 결정으로 유지한다.
승격 PR은 ADR-0001을 `대체됨`으로 바꾸기 전에 ADR-0001의 비제품승격 current marker를 `docs/ADR_INDEX.md`의 현재 적용 결정 또는 별도 current ADR에 보존해야 한다.

## Current Decision Promotion Procedure

ADR-0004 current decision 승격은 이 Phase 26 alignment slice와 별도 PR에서만 수행한다.
승격 PR은 같은 diff 안에서 ADR-0004 상태를 `적용 중`으로 바꾸고, `docs/ADR_INDEX.md` 현재 적용 중인 ADR 표와 결정 마커를 갱신하며, ADR-0004를 제안 중인 ADR 후보 섹션에서 제거해야 한다.
승격 PR에는 `aggregate_gate_status = closed` closure report가 포함되어야 하며, closure report가 없거나 `closed`가 아니면 중단한다.
승격 후 `PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 하나만 남아야 한다.

## 근거

- 현재 Phase 25 route parity는 transition fallback과 GA blocker를 명확히 구분하지 않는다.
- `spikes/**` 활성 경로가 남아 있으면 제품 runtime source와 historical baseline이 섞인다.
- 제품 검증 primary가 Pester legacy suite에 머무르면 .NET Host/API/runtime owner와 검증 owner가 어긋난다.
- 승인 전 후보 ADR로 분리하면 현재 `keep-spike` 결정을 유지하면서 후속 Phase 26 alignment를 준비할 수 있다.

## 영향 범위

- 포함 경로:
  - `src/DesktopNode.*`
  - `web/**`
  - `packaging/windows-desktop-node/**`
  - `docs/**`
  - `archive/**` 후보
- 제외 경로:
  - Linux `purecvisorsd`
  - Linux Single Edge UI/API
  - KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- 운영 또는 검증 영향:
  - 이 ADR 후보 추가만으로 host mutation은 실행하지 않는다.
  - 실제 Hyper-V mutation, MSI lifecycle, firewall rule enable/removal, Event Log source registration/removal, trust store install/removal은 계속 explicit admin opt-in gate다.

## 대안

### ADR-0001 유지

선택하지 않는다. `keep-spike`는 현재 적용 결정으로는 안전하지만, PowerShell-free product ops/runtime과 `spikes/**` 제거 목표를 route별로 진행하기에는 방향성이 부족하다.

### Phase spec만 유지하고 ADR 후보를 만들지 않음

선택하지 않는다. 제품 승격 목표와 공개 경계, installer/service/update/security policy를 바꾸려는 결정은 ADR 후보로 보여야 한다.

### 즉시 ADR-0001 대체

선택하지 않는다. route matrix, repo migration map, verification ownership map이 먼저 고정되어야 한다.

## 검증 기준

문서/ADR 후보 변경:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

후속 route 전환 plan은 `dotnet test src/DesktopNode.sln`과 installed non-mutating/admin opt-in smoke 기준을 route tier에 맞게 추가한다.

## 관련 문서

- `docs/ADR_INDEX.md`
- `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`
- `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- `docs/ga-ready/REPO_MIGRATION_MAP.md`
- `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
~~~~

- [ ] **Step 2: Align the existing redesign spec marker and route matrix ownership**

In `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`, keep:

```text
PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime
```

Replace:

```text
DESKTOP_NODE_GA_READY_REDESIGN_DECISION: powershell-free-product-ops-runtime
```

with:

```text
DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime
```

In the same spec, keep `## Route Promotion Matrix` as a high-level overview only. Remove the inline detailed route table and route rows from that spec. Replace the section body with Korean prose that says:

- Detailed route rows, field schema, enum allowed values, and gate invariants are owned only by `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`.
- The redesign spec describes why route-level promotion is required, but it does not duplicate route rows.
- If the matrix and spec disagree, `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` is the `상세 route contract` for this candidate.

- [ ] **Step 3: Extend the Pester test for ADR and redesign spec content**

Replace the test added in Task 1 with this expanded version:

```powershell
    It 'documents the GA-ready product runtime candidate without changing the current decision' {
        $adrCandidatePath = Join-Path $script:RepoRoot 'docs/adr/0004-ga-ready-product-runtime-candidate.md'
        $redesignSpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md'

        Test-Path -LiteralPath $adrCandidatePath | Should -BeTrue
        Test-Path -LiteralPath $redesignSpecPath | Should -BeTrue

        $adrCandidate = Get-Content -LiteralPath $adrCandidatePath -Raw
        $redesignSpec = Get-Content -LiteralPath $redesignSpecPath -Raw

        $adrCandidate | Should -Match '상태: 제안'
        $adrCandidate | Should -Match '대체 대상: 승인 시 ADR-0001 대체'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adrCandidate | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $adrCandidate | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
        $adrCandidate | Should -Match '승인 시 목표 상태는 PowerShell-free product ops/runtime'
        $adrCandidate | Should -Match '현재 적용 결정이 아니다'
        $adrCandidate | Should -Match 'GA gate와 release gate 분리'
        $adrCandidate | Should -Match 'Release execution'
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
        $adrCandidate | Should -Match 'ADR-0004를 current decision으로 승격하지 않는다'
        $adrCandidate | Should -Match '## Aggregate Gate Closure Report'
        $adrCandidate | Should -Match 'aggregate-gate-closure-<YYYY-MM-DD>\.md'
        $adrCandidate | Should -Match 'aggregate_gate_status = closed'
        $adrCandidate | Should -Match '첫 Phase 26 alignment slice에서는 closure report를 만들지 않는다'
        $adrCandidate | Should -Match '## ADR-0001 Replacement Scope'
        $adrCandidate | Should -Match '대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단'
        $adrCandidate | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $adrCandidate | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $adrCandidate | Should -Match '## Current Decision Promotion Procedure'
        $adrCandidate | Should -Match '이 Phase 26 alignment slice와 별도 PR'
        $adrCandidate | Should -Match 'ADR-0004 상태를 `적용 중`'
        $adrCandidate | Should -Match '제안 중인 ADR 후보 섹션에서 제거'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 하나만'

        $redesignSpec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $redesignSpec | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $redesignSpec | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
        $redesignSpec | Should -Match 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'
        $redesignSpec | Should -Match '상세 route contract'
        $redesignSpec | Should -Not -Match '\| Route/Operation \|'
        $redesignSpec | Should -Not -Match 'DELETE /api/v1/vms/\{id\}/checkpoints/\{name\}'
    }
```

- [ ] **Step 4: Run the targeted test**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1' -FullName '*GA-ready product runtime candidate*' -Output Detailed"
```

Expected:

```text
Tests Passed: 1, Failed: 0
```

## Task 3: Add Route Promotion Matrix

**Files:**

- Create: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [ ] **Step 1: Create the `docs/ga-ready` directory**

Run:

```powershell
New-Item -ItemType Directory -Path 'docs/ga-ready' -Force | Out-Null
Test-Path -LiteralPath 'docs/ga-ready'
```

Expected:

```text
True
```

- [ ] **Step 2: Create the route matrix document**

Create `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` with exactly this content:

~~~~markdown
# GA-ready Route Promotion Matrix

이 문서는 Desktop Node API route와 product operation별 current owner, target owner, implementation basis, fallback policy, promotion state, GA-ready gate, release gate, network exposure gate를 고정한다.

현재 적용 결정은 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`다. 이 matrix는 GA-ready redesign 후보의 transition contract이며, 현재 적용 ADR을 즉시 대체하지 않는다.

## Field Schema

| Field | Required | Allowed values |
|---|---:|---|
| `route` | yes | HTTP route path or product operation name |
| `route_surface` | yes | `current-route`, `future-route`, `product-operation` |
| `domain` | yes | `host-read`, `network-read`, `vm-read`, `vm-lifecycle`, `checkpoint-lifecycle`, `job-runtime`, `product-ops`, `operating-system-ops` |
| `risk_tier` | yes | `tier1-read-only`, `tier2-reversible-mutation`, `tier3-destructive-or-persistent` |
| `current_owner` | yes | `dotnet-native`, `dotnet-request-processor-powershell-helper`, `powershell-helper`, `product-wrapper`, `dotnet-runtime`, `mixed-history`, `not-yet-defined`, `not-implemented` |
| `target_owner` | yes | `dotnet-host-adapter`, `dotnet-hyperv-adapter`, `dotnet-runtime`, `dotnet-job-store-migration-action`, `dotnet-service-action`, `dotnet-token-storage-action`, `dotnet-config-migration-action`, `dotnet-data-root-action`, `windows-native-package`, `windows-eventlog-action`, `windows-firewall-action`, `windows-trust-store-action` |
| `implementation_basis` | yes | `registry-wmi-service`, `wmi-cim`, `dotnet-runtime`, `product-config-migration-plan`, `job-store-migration-plan`, `windows-native-api`, `dpapi-local-machine-token-plan`, `windows-certificate-store-api`, `eventlog-registration-plan`, `firewall-rule-plan`, `data-root-lifecycle-plan`, `package-contract`, `approved-system-executable` |
| `fallback_policy` | yes | `none`, `test-only`, `transition-helper`, `blocked` |
| `promotion_state` | yes | `current-native`, `transition-helper`, `blocked`, `ga-ready-candidate` |
| `admin_smoke_required` | yes | `none`, `installed-non-mutating`, `explicit-admin-opt-in` |
| `ga_ready_gate` | yes | concise Korean gate text |
| `release_gate` | yes | `none`, `release-approval-required` |
| `network_exposure_gate` | yes | `none`, `lan-exposure-approval-required` |

## Fallback Policy

- `none`: product fallback is not used.
- `test-only`: fixture or injectable test fallback is allowed, but product fallback is not used.
- `transition-helper`: PowerShell helper fallback is allowed only before promotion.
- `blocked`: route is a GA-ready blocker until target owner parity exists.

## Promotion State

- `current-native`: current product row is already implemented without product PowerShell fallback.
- `transition-helper`: current product row still allows PowerShell helper fallback during transition.
- `blocked`: current product row cannot be promoted until target owner implementation and evidence exist.
- `ga-ready-candidate`: current product row has target owner evidence and can be promoted after review.

## State Invariants

- `promotion_state = current-native` allows only `fallback_policy = none` or `fallback_policy = test-only`.
- `promotion_state = transition-helper` requires `fallback_policy = transition-helper`.
- `promotion_state = blocked` requires `fallback_policy = blocked`.
- `promotion_state = ga-ready-candidate` allows only `fallback_policy = none` or `fallback_policy = test-only`.
- `risk_tier = tier1-read-only` allows only `admin_smoke_required = none` or `admin_smoke_required = installed-non-mutating`.
- `risk_tier = tier2-reversible-mutation` requires `admin_smoke_required = explicit-admin-opt-in`.
- `risk_tier = tier3-destructive-or-persistent` requires `admin_smoke_required = explicit-admin-opt-in`.

## Aggregate GA-ready Decision Gate

ADR-0004를 current decision으로 승격하기 전에는 route matrix와 supporting docs 기준으로 다음 aggregate gate가 닫혀야 한다.

- GA 범위의 `current-route`와 `product-operation` row는 `promotion_state = transition-helper` 또는 `promotion_state = blocked`가 0개여야 한다.
- `future-route` row는 GA 범위 제외 사유와 별도 implementation plan requirement를 명시해야 한다.
- 제품 runtime/request path에는 PowerShell helper가 없어야 한다.
- 활성 제품 경로에는 `spikes/**`가 없어야 한다.
- repo migration preflight evidence와 verification ownership replacement evidence가 완료되어야 한다.
- `tier2-reversible-mutation`과 `tier3-destructive-or-persistent` row는 explicit admin opt-in evidence가 완료되어야 하며, Evidence Freshness Rule을 만족하지 않는 stale evidence는 aggregate GA-ready gate 충족에 사용할 수 없다.
- `release_gate = release-approval-required` row는 GA-ready 판정과 release execution을 분리하며, 별도 release approval 전에는 실행하지 않는다.

이 aggregate gate가 닫히기 전에는 ADR-0004를 current decision으로 승격하지 않는다.

## GA Scope Classification Rule

- `route_surface = current-route`와 `route_surface = product-operation` row는 기본적으로 GA-scope다.
- `route_surface = future-route` row만 GA-scope에서 제외할 수 있으며, 제외 사유와 별도 implementation plan requirement를 기록해야 한다.
- `release_gate = release-approval-required`와 `network_exposure_gate = lan-exposure-approval-required`는 GA-scope 제외 사유가 아니며, execution approval 또는 exposure approval 분리만 의미한다.
- `current-route` 또는 `product-operation` row를 GA-scope 밖으로 빼려면 별도 ADR/task approval로 제품 범위를 줄여야 하며, 그 전에는 aggregate GA-ready gate closure로 계산할 수 없다.

## PowerShell-Free Product Path Closure Rule

- GA-scope `current-route` 또는 `product-operation` row는 product runtime/request/admin execution path에서 PowerShell helper를 사용하지 않아야 aggregate GA-ready gate closure로 계산할 수 있다.
- `current_owner = powershell-helper` 또는 `current_owner = dotnet-request-processor-powershell-helper` row는 target owner evidence가 있더라도 current owner가 갱신되기 전까지 aggregate GA-ready gate closure로 계산할 수 없다.
- `fallback_policy = transition-helper` row는 helper fallback 제거 evidence가 있기 전까지 aggregate GA-ready gate closure로 계산할 수 없다.
- `fallback_policy = test-only`는 fixture or injectable test fallback에만 허용하며 product execution path fallback으로 사용할 수 없다.

## Active Product Path Classification Rule

- `spikes/**` path가 runtime/service/API/CLI/Web Console execution, packaging input, installer input, static asset source, generated parity manifest, required verification command, CI/local verification command, or developer command documentation에 남아 있으면 active product path로 간주한다.
- `archive/spikes/**` reference는 historical/read-only baseline intent일 때만 허용하며 product execution, packaging, required verification source로 사용할 수 없다.
- `docs/**` command가 `spikes/**`를 required product path로 실행하도록 안내하면 active product path로 간주한다.
- Aggregate GA-ready gate closure에는 `spikes/**` active product path가 0개라는 repo migration preflight evidence와 docs command update evidence가 필요하다.

## Aggregate Gate Closure Report Candidate

첫 slice에서는 실제 closure report 파일을 만들지 않는다.
후속 closure report 후보 위치는 `docs/ga-ready/evidence/aggregate-gate-closure-<YYYY-MM-DD>.md`다.
Closure report는 Markdown record이며 machine-readable JSON은 만들지 않는다.

각 closure report는 Markdown 안에서 다음 필드를 가져야 한다.

| Field | Required | Allowed values | Description |
|---|---:|---|---|
| `report_id` | yes | non-empty string | closure report stable id |
| `created_at` | yes | ISO-8601 timestamp | report creation time |
| `source_commit_sha` | yes | full 40-char SHA or minimum 12-char abbreviated SHA | source tree under review |
| `route_matrix_commit_sha` | yes | full 40-char SHA or minimum 12-char abbreviated SHA | route matrix version used for counts |
| `ga_scope_current_route_count` | yes | integer >= 0 | GA-scope `current-route` row count |
| `ga_scope_product_operation_count` | yes | integer >= 0 | GA-scope `product-operation` row count |
| `future_route_exclusion_count` | yes | integer >= 0 | `future-route` rows excluded with reason and implementation plan requirement |
| `transition_helper_count` | yes | integer >= 0 | GA-scope rows with `promotion_state = transition-helper` |
| `blocked_count` | yes | integer >= 0 | GA-scope rows with `promotion_state = blocked` |
| `powershell_current_owner_count` | yes | integer >= 0 | GA-scope rows with PowerShell-backed current owner |
| `powershell_fallback_count` | yes | integer >= 0 | GA-scope rows with product execution `fallback_policy = transition-helper` |
| `active_spikes_path_count` | yes | integer >= 0 | active product path references under `spikes/**` |
| `repo_migration_preflight_status` | yes | `pass`, `fail`, `blocked`, `not-run` | repo migration preflight evidence status |
| `docs_command_update_status` | yes | `pass`, `fail`, `blocked`, `not-run` | docs command update evidence status |
| `verification_ownership_replacement_status` | yes | `pass`, `fail`, `blocked`, `not-run` | replacement verification owner evidence status |
| `tier2_admin_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | tier2 explicit admin opt-in evidence status |
| `tier3_admin_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | tier3 explicit admin opt-in evidence status |
| `release_gated_prerelease_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | release-gated pre-release evidence status |
| `lan_gated_preapproval_evidence_status` | yes | `pass`, `fail`, `blocked`, `not-run` | LAN-gated pre-approval evidence status |
| `stale_evidence_count` | yes | integer >= 0 | stale evidence records used by neither rerun nor approved limited waiver |
| `waived_evidence_count` | yes | integer >= 0 | approved limited waiver count |
| `waiver_only_gate_satisfaction_count` | yes | integer >= 0 | rows attempting to satisfy a gate by waiver alone |
| `aggregate_gate_status` | yes | `open`, `closed`, `blocked` | final aggregate gate state |

`aggregate_gate_status = closed`가 되려면 `transition_helper_count`, `blocked_count`, `powershell_current_owner_count`, `powershell_fallback_count`, `active_spikes_path_count`, `stale_evidence_count`, `waiver_only_gate_satisfaction_count`가 모두 `0`이어야 하며 required status field가 모두 `pass`여야 한다.
`aggregate_gate_status = blocked`는 GA-scope row가 blocked 상태로 남아 있거나, 금지된 PowerShell/product fallback/active spikes path/stale evidence/waiver-only gate satisfaction이 하나라도 있을 때 사용한다.
그 외 미실행 또는 미완료 상태는 `aggregate_gate_status = open`으로 둔다.

## ADR Promotion Procedure Rule

- Phase 26 alignment slice는 ADR 후보와 supporting docs만 만들며 ADR-0004를 current decision으로 승격하지 않는다.
- ADR-0004 승격 PR은 `aggregate_gate_status = closed` closure report 없이 진행할 수 없다.
- 승격 PR은 ADR-0004 상태, `docs/ADR_INDEX.md` 현재 적용 중인 ADR 표, 결정 마커, 제안 중인 ADR 후보 섹션을 같은 diff에서 갱신해야 한다.
- 승격 후 `PRODUCT_RUNTIME_PROMOTION_DECISION`의 current source는 하나만 남아야 한다.
- ADR-0001의 `DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo`와 `DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned`는 별도 ADR이 바꾸기 전까지 현재 적용 결정으로 보존해야 한다.
- ADR number collision, missing closure report, non-closed closure report, duplicate current product runtime promotion source, or missing preserved non-promotion current marker는 승격 중단 조건이다.

## Evidence Freshness Rule

`tier2-reversible-mutation` 또는 `tier3-destructive-or-persistent` row의 explicit admin opt-in evidence는 다음 scope를 기록해야 aggregate GA-ready gate를 충족할 수 있다.

- commit SHA
- artifact/package version
- route/operation row id
- current owner
- target owner
- implementation basis
- fallback policy
- promotion state
- admin smoke requirement
- release gate
- network exposure gate
- runner version
- host capability snapshot
- exact command mode

Evidence 기록 이후 current owner, target owner, implementation basis, fallback policy, promotion state, admin smoke requirement, release gate, network exposure gate, package contract, service host, installer custom action, route matrix gate가 변경되면 해당 evidence는 stale로 간주한다.
Stale evidence는 historical context로만 남기며 aggregate GA-ready gate 충족에 사용할 수 없다.
Stale evidence는 rerun하거나 별도 approval waiver로만 다시 gate 충족 근거가 될 수 있다.

## Evidence Ledger Candidate

첫 slice에서는 실제 evidence 파일을 만들지 않는다.
후속 evidence ledger 후보 위치는 `docs/ga-ready/evidence/`다.
Ledger는 Markdown evidence ledger 후보이며 machine-readable JSON은 만들지 않는다.

각 evidence record는 Markdown 안에서 다음 필드를 가져야 한다.

| Field | Required | Description |
|---|---:|---|
| `evidence_id` | yes | evidence record stable id |
| `route_or_operation` | yes | route path or product operation name |
| `route_surface` | yes | route matrix `route_surface` value |
| `risk_tier` | yes | route matrix `risk_tier` value |
| `current_owner` | yes | route matrix `current_owner` value |
| `commit_sha` | yes | implementation/evidence commit SHA |
| `artifact_or_package_version` | yes | artifact or package version under test |
| `target_owner` | yes | route matrix `target_owner` value |
| `implementation_basis` | yes | route matrix `implementation_basis` value |
| `fallback_policy` | yes | route matrix `fallback_policy` value |
| `promotion_state` | yes | route matrix `promotion_state` value |
| `admin_smoke_required` | yes | route matrix `admin_smoke_required` value |
| `release_gate` | yes | route matrix `release_gate` value |
| `network_exposure_gate` | yes | route matrix `network_exposure_gate` value |
| `runner_version` | yes | smoke/test runner version |
| `host_capability_snapshot` | yes | host capability snapshot reference or summary |
| `exact_command_mode` | yes | exact command mode used for the evidence |
| `result` | yes | pass/fail/blocked result summary |
| `created_at` | yes | evidence creation timestamp |
| `stale_triggers` | yes | freshness triggers that would stale this record |
| `waiver_status` | yes | none/requested/approved and approval reference |

## Evidence Row Identity Rule

- `route_or_operation`은 route matrix의 `Route/Operation` cell과 정확히 일치해야 하며 evidence row identity로 사용한다.
- 같은 `route_or_operation` 값을 가진 duplicate matrix row는 허용하지 않는다.
- route path, operation name, route_surface, current_owner, target_owner, implementation_basis, fallback_policy, promotion_state, admin_smoke_required, release_gate, network_exposure_gate 중 하나가 바뀌면 기존 evidence는 stale로 간주한다.
- Evidence ledger는 rename 전후 row를 같은 evidence로 병합하지 않는다. Rename 후에는 새 `route_or_operation`에 대해 rerun evidence 또는 별도 approval waiver가 필요하다.

## Evidence Waiver Policy

Waiver는 aggregate GA-ready gate 자체를 통과시키는 용도가 아니다.
Waiver는 특정 stale evidence record를 제한적으로 대체하는 예외이며, waiver가 있더라도 row의 target owner, implementation basis, risk tier, release gate, network exposure gate는 낮출 수 없다.

각 waiver record는 Markdown 안에서 다음 필드를 가져야 한다.

| Field | Required | Description |
|---|---:|---|
| `waiver_id` | yes | waiver stable id |
| `evidence_id` | yes | waived stale evidence record id |
| `scope` | yes | route/operation and condition covered by the waiver |
| `reason` | yes | reason rerun evidence is not available |
| `risk_acceptance_owner` | yes | named owner accepting residual risk |
| `expires_at` | yes | waiver expiry timestamp or milestone |
| `replacement_evidence_required` | yes | replacement evidence requirement |
| `approval_reference` | yes | ADR/task/review approval reference |

Waiver-only gate satisfaction is forbidden for `tier3-destructive-or-persistent`, `release_gate = release-approval-required`, trust-store, and firewall LAN exposure rows.
Those rows require rerun evidence, even if a waiver exists.

## Evidence Field Format and Enum Rule

Evidence ledger와 waiver record field는 다음 format과 enum rule을 따른다.

- `route_surface`, `risk_tier`, `current_owner`, `target_owner`, `implementation_basis`, `fallback_policy`, `promotion_state`, `admin_smoke_required`, `release_gate`, `network_exposure_gate`는 route matrix Field Schema enum을 그대로 재사용한다.
- `result` allowed values는 `pass`, `fail`, `blocked`, `not-run`이다.
- `waiver_status` allowed values는 `none`, `requested`, `approved`, `rejected`, `expired`다.
- `commit_sha`는 full 40-char SHA를 우선 사용하며, 최소 12-char abbreviated SHA를 허용한다.
- `created_at`과 `expires_at`은 ISO-8601 timestamp 또는 명시적 milestone reference만 허용한다.
- `scope`, `reason`, `host_capability_snapshot`, `approval_reference`는 자유 텍스트지만 비워둘 수 없다.

## Route Surface Invariants

- `route_surface = current-route`는 현재 구현된 Local API route에만 사용한다.
- `route_surface = future-route`는 현재 이 matrix에 실제 row로 남아 있지 않다.
- `route_surface = future-route` row는 반드시 `current_owner = not-implemented`, `fallback_policy = blocked`, `promotion_state = blocked`여야 한다.
- `route_surface = product-operation`은 HTTP API route가 아닌 product operation row에만 사용한다.

## Served Route Scope Rule

- `route_surface = current-route`는 실제 served Local API route만 의미한다.
- side-by-side contract-only route 후보는 실제 request processor 또는 PowerShell Local API available routes에 등록되기 전까지 matrix row로 추가하지 않는다.
- `GET /api/v1/jobs`는 현재 contract-only 후보이며, 실제 served route가 아니므로 matrix에 포함하지 않는다.
- Job runtime read surface는 현재 `GET /api/v1/jobs/{job_id}` row로만 표현한다.
- Contract mirror aggregate route 후보인 `POST /api/v1/vms/{vmId}/lifecycle/{action}`는 실제 served route가 아니므로 matrix row로 추가하지 않는다.
- VM lifecycle served surface는 현재 `POST /api/v1/vms/{id}/start`, `shutdown`, `poweroff`, `restart` 개별 row로만 표현한다.

## Future Route Execution Guard

- `route_surface = future-route` row는 Phase 26 alignment slice에서 구현하거나 실제 Local API route로 등록하지 않는다.
- `future-route` row를 `current-route`로 변경하려면 별도 future route implementation plan이 먼저 route contract, not-found/idempotency contract, destructive cleanup proof, explicit admin opt-in evidence requirement를 정의해야 한다.
- `future-route` row는 위 evidence가 승인되기 전까지 `current_owner = not-implemented`, `fallback_policy = blocked`, `promotion_state = blocked`를 유지한다.

## Native-First Helper Fallback Rule

- `GET /api/v1/network/inventory` row는 현재 구현처럼 `current_owner = dotnet-native`로 기록한다.
- 이 row의 `fallback_policy = transition-helper`와 `promotion_state = transition-helper`는 현재 owner가 helper라는 뜻이 아니라, topology parity가 불완전할 때 PowerShell helper fallback을 유지한다는 GA-ready blocker다.
- 이 row는 switch type, `allow_management_os`, external adapter field parity를 helper 없이 보존하기 전까지 `promotion_state = current-native`로 승격할 수 없다.

## Job Runtime Risk Inheritance Rule

- `POST /api/v1/jobs/{job_id}/retry` row는 retry state transition의 route owner를 나타낸다.
- retry로 다시 queued 되는 underlying operation은 원본 job operation의 `risk_tier`, `admin_smoke_required`, cleanup evidence를 상속한다.
- 원본 operation이 `tier2-reversible-mutation` 또는 `tier3-destructive-or-persistent`이면 retry 실행/검증은 기본 non-mutating verification에 포함하지 않고 `explicit-admin-opt-in` evidence에서만 다룬다.
- retry route는 원본 operation의 GA-ready gate, release gate, network exposure gate를 낮추거나 우회할 수 없다.

## Job Route Parameter Rule

- Job route path parameter는 `job_id`로 통일한다.
- `id`와 `jobId`는 code variable 또는 internal compatibility name으로만 다루며 route identity parameter로 사용하지 않는다.

## VM Route Parameter Rule

- VM route path parameter는 기존 served API 계약인 `id`를 유지한다.
- VM route `id`는 VM `id` 또는 `name` lookup key를 의미한다.
- `vmId`는 code variable 또는 internal compatibility name으로만 다루며 route identity parameter로 사용하지 않는다.
- `vm_id`로 바꾸는 것은 이 alignment slice 범위가 아니며 별도 API route contract migration이 없으면 matrix row에 추가하지 않는다.

## Checkpoint Route Parameter Rule

- Checkpoint route path parameter는 `checkpoint_id`로 통일한다.
- `name`과 `checkpoint_name`은 request body/helper compatibility alias로만 다루며 route identity parameter로 사용하지 않는다.

## Current Owner Invariants

- `current_owner = not-yet-defined`은 `product config migration apply` row에만 허용한다.
- `product config migration apply` row는 현재 책임을 과장하지 않도록 반드시 `current_owner = not-yet-defined`여야 한다.
- `current_owner = not-implemented`는 `route_surface = future-route` row에만 허용한다.

## Current Owner Resolution Rule

- `not-yet-defined`은 이 alignment plan에서만 허용하는 임시 계획 상태다.
- `product config migration apply` 구현 plan을 작성하기 전에는 코드/문서 근거로 `current_owner`를 `not-yet-defined`가 아닌 구체 enum 값으로 해소하거나, 구현 범위에서 제외하고 `promotion_state = blocked` 유지 사유를 기록해야 한다.
- `product config migration apply`는 current config source inventory, current schema owner resolution, owned source config path evidence, source path/version evidence, migration plan id/version, service stopped precondition 없이 구현을 시작할 수 없으며, `not-yet-defined` 해소 전 구현 금지 상태를 유지한다.

## Mixed History Resolution Rule

- `mixed-history`은 service product operation row에만 허용한다.
- `mixed-history` row는 wrapper, installer, service host 이력이 섞여 있음을 표시하는 임시 current owner 상태일 뿐이다.
- service product operation 구현 plan을 작성하기 전에는 actual current code path와 evidence source를 구체 owner로 해소하거나, 구현 범위에서 제외하고 `promotion_state = blocked` 유지 사유를 기록해야 한다.
- `mixed-history` 자체를 promotion evidence 또는 target owner로 간주하지 않는다.

## Target Owner Invariants

- `target_owner = dotnet-config-migration-action`은 `product config migration apply` row에만 허용한다.
- `product config migration apply` row는 반드시 `target_owner = dotnet-config-migration-action`이어야 한다.
- `target_owner = dotnet-job-store-migration-action`은 `job store migration apply` row에만 허용한다.
- `job store migration apply` row는 반드시 `target_owner = dotnet-job-store-migration-action`이어야 한다.
- `job store migration apply`에서 `current_owner = dotnet-runtime`은 read/schema mismatch detection의 현재 owner 근거일 뿐이며, migration mutation은 반드시 `dotnet-job-store-migration-action`이 소유한다.
- `target_owner = dotnet-token-storage-action`은 `protected token bootstrap` row에만 허용한다.
- `protected token bootstrap` row는 반드시 `target_owner = dotnet-token-storage-action`이어야 한다.
- `target_owner = dotnet-data-root-action`은 `data root remove` row에만 허용한다.
- `data root remove` row는 반드시 `target_owner = dotnet-data-root-action`이어야 한다.
- `target_owner = windows-native-package`는 `local payload update`, `rollback restore` row에만 허용한다.
- `local payload update`, `rollback restore` row는 반드시 `target_owner = windows-native-package`여야 한다.
- `target_owner = windows-eventlog-action`은 `Event Log source registration`, `Event Log source removal` row에만 허용한다.
- `Event Log source registration`, `Event Log source removal` row는 반드시 `target_owner = windows-eventlog-action`이어야 한다.
- `target_owner = windows-firewall-action`은 `firewall rule enable LAN exposure`, `firewall rule removal` row에만 허용한다.
- `firewall rule enable LAN exposure`, `firewall rule removal` row는 반드시 `target_owner = windows-firewall-action`이어야 한다.
- `target_owner = windows-trust-store-action`은 `trust store install`, `trust store removal` row에만 허용한다.
- `trust store install`, `trust store removal` row는 반드시 `target_owner = windows-trust-store-action`이어야 한다.

## Implementation Basis Invariants

- `implementation_basis = dpapi-local-machine-token-plan`은 `protected token bootstrap` row에만 허용한다.
- `protected token bootstrap` row는 반드시 `implementation_basis = dpapi-local-machine-token-plan`이어야 한다.
- `dpapi-local-machine-token-plan`은 raw token 비노출, token source inventory, single-source precondition, existing protected token no-overwrite, legacy token migration, legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required, protected token schema, ACL hardening, service command line protected file path only, command line token value forbidden, diagnostics redaction evidence 전용이다.
- `implementation_basis = product-config-migration-plan`은 `product config migration apply` row에만 허용한다.
- `product config migration apply` row는 반드시 `implementation_basis = product-config-migration-plan`이어야 한다.
- `product-config-migration-plan`은 current config source inventory, current schema owner resolution, owned source config path evidence, source path/version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, validation preflight descriptor required, backup path inside owned config backup root, atomic config replace, no data root mutation, no token mutation, no job store mutation, no service identity mutation, partial config migration forbidden evidence, rollback on migration failure, rollback result diagnostics, cleanup evidence, service-start preflight decision descriptor only, validation writes forbidden, explicit admin opt-in before config write 전용이다.
- `implementation_basis = job-store-migration-plan`은 `job store migration apply` row에만 허용한다.
- `job store migration apply` row는 반드시 `implementation_basis = job-store-migration-plan`이어야 한다.
- `job-store-migration-plan`은 current job store path inventory, current job schema owner evidence, owned job store path evidence, source job store version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, runtime writer stopped evidence, backup path inside owned job-store backup root, destructive rewrite disabled by default, atomic job store replace, no config mutation, no token mutation, no service identity mutation, partial job store migration forbidden evidence, rollback on migration failure, rollback result diagnostics, recovery evidence, explicit admin opt-in before job store write 전용이다.
- `job store migration apply`는 current job store path inventory, current job schema owner evidence, owned job store path evidence, source job store version evidence, migration plan id/version, runtime writer stopped evidence 없이 구현을 시작할 수 없으며, job store ownership/schema/migration plan/runtime-writer stopped evidence가 불명확하면 `promotion_state = blocked`를 유지한다.
- `implementation_basis = eventlog-registration-plan`은 `Event Log source registration`, `Event Log source removal` row에만 허용한다.
- `Event Log source registration`, `Event Log source removal` row는 반드시 `implementation_basis = eventlog-registration-plan`이어야 한다.
- `eventlog-registration-plan`은 exact event source name, exact channel/log name, owned event source manifest/evidence, missing-or-owned-source precondition, foreign-source conflict blocks, exact log/source binding, no overwrite of existing foreign source, registry write limited to event source registration, registry delete limited to owned event source registration, no service mutation, no firewall mutation, no trust store mutation, conflict diagnostics only, post-registration binding evidence, owned-source-only removal, missing-source idempotency, cleanup diagnostics only, post-removal absence evidence, no MSI/default execution 전용이며 MSI default action이 아니다.
- `implementation_basis = firewall-rule-plan`은 `firewall rule enable LAN exposure`, `firewall rule removal` row에만 허용한다.
- `firewall rule enable LAN exposure`, `firewall rule removal` row는 반드시 `implementation_basis = firewall-rule-plan`이어야 한다.
- `firewall-rule-plan`은 `windows-firewall-action`, LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence, owned-rule-only removal, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution 전용이며 default install/repair/MSI action이 아니다.
- `implementation_basis = data-root-lifecycle-plan`은 `data root remove` row에만 허용한다.
- `data root remove` row는 반드시 `implementation_basis = data-root-lifecycle-plan`이어야 한다.
- `data-root-lifecycle-plan`은 `REMOVE_DATA=1`, remove-data handoff descriptor required, exact data root path allowlist, owned data root marker/evidence, service deleted/absent precondition, installed service blocks delete diagnostics, protected token delete only within owned data root, no product root mutation, no service mutation, locked-file abort before partial delete, delete manifest/journal evidence, post-delete absence evidence, no partial delete success evidence, diagnostics evidence 전용이다.
- `implementation_basis = package-contract`는 `local payload update`, `rollback restore` row에만 허용한다.
- `local payload update`, `rollback restore` row는 반드시 `implementation_basis = package-contract`여야 한다.
- `package-contract`는 ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation 전용이다.
- `package-contract`가 channel/version/update payload/root evidence와 일치하지 않으면 update/rollback은 activation 또는 restore 없이 blocked diagnostics만 반환한다.
- `implementation_basis = windows-certificate-store-api`는 `trust store install`, `trust store removal` row에만 허용한다.
- `trust store install`, `trust store removal` row는 반드시 `implementation_basis = windows-certificate-store-api`여야 한다.
- `windows-certificate-store-api`는 release approval, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, owned certificate evidence, thumbprint/store binding evidence, certificate store delete limited to owned certificate, no service mutation, no firewall mutation, no eventlog mutation, post-install trust binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution 전용이다.

## Approved System Executable Rule

- 첫 slice에서는 `implementation_basis = approved-system-executable` row를 만들지 않는다.
- `approved-system-executable`은 schema enum 후보로만 남기며, 현재 matrix row에서는 사용 count가 반드시 0이어야 한다.
- 후속 slice에서 외부 executable 실행이 필요하면 해당 implementation plan이 ADR/task approval required, exact executable path and publisher/hash evidence, non-shell invocation only, argument schema with allowed flags/values, no user-controlled raw arguments, working directory fixed, environment variable allowlist, no token/secret on command line, no implicit reboot, timeout/exit-code contract, stdout/stderr redaction, dry-run/WhatIf where supported, no chained shell, admin opt-in, post-run evidence, examples are candidates only, not allowlist를 먼저 정의해야 한다.
- executable identity 또는 argument ownership이 불명확하면 implementation basis는 blocked로 유지한다.

## Release Gate

- `none`: GA-ready product runtime 판정만으로 해당 row의 promotion 가능 여부를 판단한다.
- `release-approval-required`: GA-ready evidence가 있어도 stable publication, public trusted signing, external release, signed update/rollback 실행은 별도 release approval 전까지 금지한다.

## Network Exposure Gate

- `none`: LAN exposure approval 없이 해당 row의 GA-ready promotion 판단이 가능하다.
- `lan-exposure-approval-required`: loopback-only 기본 정책, LAN mode opt-in, token source, firewall scope 변경을 별도 network exposure approval 전까지 금지한다.

## Auth and Exposure Boundary

- GA-ready 후보의 Local API auth mode는 `single_bearer_token`이다.
- `multi_user = false`와 `rbac = false`를 유지한다. multi-user/RBAC은 이 alignment slice의 숨은 scope가 아니며, 필요하면 별도 ADR/plan으로 다룬다.
- loopback static asset bypass는 Web Console bootstrap을 위한 `unauthenticated-static-only` 정책으로만 허용한다.
- non-loopback static assets require bearer auth. LAN mode에서 static asset과 API route는 같은 bearer token boundary 안에 있어야 한다.
- LAN mode requires `-AllowLan` and a token source. token source 없이 LAN prefix를 열 수 없으며 `PCV_LAN_TOKEN_REQUIRED` error contract를 유지한다.
- non-loopback prefix without explicit LAN opt-in은 `PCV_PREFIX_NOT_LOOPBACK` error contract를 유지한다.

## API Route Matrix

| Route/Operation | Route surface | Domain | Risk tier | Current owner | Target owner | Implementation basis | Fallback policy | Promotion state | Admin smoke required | GA-ready gate | Release gate | Network exposure gate |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `GET /api/v1/host/status` | `current-route` | `host-read` | `tier1-read-only` | `dotnet-native` | `dotnet-host-adapter` | `registry-wmi-service` | `test-only` | `current-native` | `installed-non-mutating` | OS, Hyper-V, VMMS, admin, default switch parity와 installed smoke | `none` | `none` |
| `GET /api/v1/network/inventory` | `current-route` | `network-read` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | switch type, `allow_management_os`, external adapter parity, native structured failure, no helper retry | `none` | `none` |
| `GET /api/v1/vms` | `current-route` | `vm-read` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | helper JSON contract와 VM identity/state, CPU/startup memory/generation/checkpoint count summary field parity, storage/network parity, native structured failure, no helper retry | `none` | `none` |
| `GET /api/v1/vms/{id}` | `current-route` | `vm-read` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | native `vm.list` result parity, detail field parity, missing VM error contract, no helper retry | `none` | `none` |
| `GET /api/v1/vms/{id}/checkpoints` | `current-route` | `checkpoint-lifecycle` | `tier1-read-only` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `installed-non-mutating` | native VM inventory guard, empty checkpoint list success, checkpoint list field parity, missing VM error contract, no helper retry | `none` | `none` |
| `POST /api/v1/vms/{id}/start` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `RequestStateChange(Enabled=2)`, job lifecycle, idempotency, cleanup evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/shutdown` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `Msvm_ShutdownComponent.InitiateShutdown`, graceful shutdown semantics, shutdown integration unavailable structured failure, Windows Server 2022 Evaluation guest successful shutdown smoke, timeout, recovery evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/poweroff` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `RequestStateChange(Disabled=3)`, safe stop semantics, timeout, cleanup evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/restart` | `current-route` | `vm-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `RequestStateChange(Reset=11)`, reset restart semantics, stop-start sequencing fallback forbidden, timeout, recovery evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/checkpoints` | `current-route` | `checkpoint-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `CreateSnapshot`, post-create rename/list visibility, duplicate-name error, display-name parity, installed mutation smoke cleanup evidence | `none` | `none` |
| `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore` | `current-route` | `checkpoint-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `ApplySnapshot`, not-found error, timeout/recovery, `vm.poweroff-before-restore` minimum stable restore condition, `0.29.0-admin-smoke` installed restore mutation cleanup evidence | `none` | `none` |
| `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}` | `current-route` | `checkpoint-lifecycle` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `DestroySnapshot`, post-delete absence, not-found error, display-name parity, installed mutation smoke cleanup evidence | `none` | `none` |
| `POST /api/v1/vms` | `current-route` | `vm-lifecycle` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | native Generation 2 create, Generation 1 invalid error contract, explicit admin smoke, cleanup, rollback, no-auto-reboot evidence | `none` | `none` |
| `DELETE /api/v1/vms/{id}` | `current-route` | `vm-lifecycle` | `tier3-destructive-or-persistent` | `dotnet-native` | `dotnet-hyperv-adapter` | `wmi-cim` | `none` | `current-native` | `explicit-admin-opt-in` | C# WMI `DestroySystem`, managed marker guard, not-found/idempotency contract, `0.30.1-admin-smoke` managed delete `action=delete`, repeat `action=absent`, unmanaged guard block, cleanup/no-auto-reboot evidence | `none` | `none` |
| `GET /api/v1/runtime/policy` | `current-route` | `job-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | token storage, network exposure, `job_runtime` policy shape, secret 비노출 | `none` | `none` |
| `GET /api/v1/jobs/{job_id}` | `current-route` | `job-runtime` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `none` | persisted state read, not-found error, recovery read tests | `none` | `none` |
| `POST /api/v1/jobs/{job_id}/cancel` | `current-route` | `job-runtime` | `tier2-reversible-mutation` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `explicit-admin-opt-in` | queued-only cancel state transition, not-cancelable error, persistence recovery tests | `none` | `none` |
| `POST /api/v1/jobs/{job_id}/retry` | `current-route` | `job-runtime` | `tier2-reversible-mutation` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `none` | `current-native` | `explicit-admin-opt-in` | failed retryable-only retry, attempt limit, `retry_of` lineage, admin opt-in for underlying mutation evidence | `none` | `none` |

## Product Ops Matrix

| Operation | Route surface | Domain | Risk tier | Current owner | Target owner | Implementation basis | Fallback policy | Promotion state | Admin smoke required | GA-ready gate | Release gate | Network exposure gate |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| service status | `product-operation` | `product-ops` | `tier1-read-only` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `ga-ready-candidate` | `installed-non-mutating` | `DesktopNode.Host.exe service-action status` code-level native SCM controller, service identity read, exact binary path ownership check, installed non-mutating smoke pending | `none` | `none` |
| service start | `product-operation` | `product-ops` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `ga-ready-candidate` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action start` code-level native SCM controller, owned service identity, exact SCM binary path/product root binding, foreign service blocks before mutation, missing-service diagnostics, no config mutation, no service delete, service started state, already-running idempotency, listener health after start, timeout/recovery, no-auto-reboot evidence, explicit admin smoke pending | `none` | `none` |
| service stop | `product-operation` | `product-ops` | `tier2-reversible-mutation` | `dotnet-native` | `dotnet-service-action` | `windows-native-api` | `none` | `ga-ready-candidate` | `explicit-admin-opt-in` | `DesktopNode.Host.exe service-action stop` code-level native SCM controller, owned service identity, exact SCM binary path/product root binding, foreign service blocks before mutation, missing-service diagnostics, no config mutation, no service delete, stop idempotency, already-stopped idempotency, stop wait timeout, stop wait timeout diagnostics, no-auto-reboot evidence, explicit admin smoke pending | `none` | `none` |
| service install create | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `mixed-history` | `dotnet-service-action` | `windows-native-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | initial install path, missing-service precondition, service name ownership identity, foreign service conflict blocks, SCM service identity, exact SCM binary path/product root binding, no overwrite of existing foreign service, conflict diagnostics only, binary path, protected token path, listener args, service account, start type, failure policy, idempotent already-installed behavior, no-auto-reboot evidence | `none` | `none` |
| service configure update | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `mixed-history` | `dotnet-service-action` | `windows-native-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | existing owned service precondition, owned-field-only config update, exact SCM binary path/product root binding, foreign binary path blocks, config drift diagnostics before mutation, config drift diff, protected token path, listener args update, data preservation, rollback/recovery on failed config update, no-auto-reboot evidence | `none` | `none` |
| protected token bootstrap | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `product-wrapper` | `dotnet-token-storage-action` | `dpapi-local-machine-token-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | raw token 비노출, token source inventory, single-source precondition, existing protected token no-overwrite, legacy token migration, legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required, protected token schema, ACL hardening, service command line protected file path only, command line token value forbidden, diagnostics redaction evidence | `none` | `none` |
| service repair missing service recreation | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `mixed-history` | `dotnet-service-action` | `windows-native-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | repair path only, service absent precondition, product root exists, owned product root evidence, existing config reuse, existing config ownership evidence, protected token path preservation, protected token ownership evidence, exact SCM binary path/product root binding, config schema validation before recreate, foreign existing service blocks, SCM service recreate, SCM binary path, service identity, no product root creation/removal, no config rewrite, no token rewrite, no data root creation, no token bootstrap, no-auto-reboot evidence | `none` | `none` |
| service repair config drift correction | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `mixed-history` | `dotnet-service-action` | `windows-native-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | repair path only, existing owned service, owned service identity, exact SCM binary path/product root binding, owned-field-only repair, allowed repair drift fields = protected token path, listener args, config drift diagnostics before mutation, config drift diff, protected token path/listener args update, foreign binary path blocks, non-repair drift handoff to service configure update, data preservation, rollback/recovery, no SCM recreate, no config rewrite, no token rewrite, no product root creation/removal, no-auto-reboot evidence | `none` | `none` |
| service uninstall stop/delete | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `mixed-history` | `dotnet-service-action` | `windows-native-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | owned service identity, exact SCM binary path/product root binding, foreign service blocks, stop-before-delete sequencing, stop idempotency, delete service only, delete idempotency, service deletion confirmation, missing-service idempotency, missing-service idempotent diagnostics, no product root delete, no data root delete, no config delete, no token delete, no REMOVE_DATA handoff, no-auto-reboot evidence | `none` | `none` |
| product root removal preserve-data | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `mixed-history` | `dotnet-service-action` | `windows-native-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | service absent/deleted precondition, owned product root evidence, exact product root allowlist, binary payload only delete, config/data/token preserve allowlist, ProgramData preserve evidence, data root delete forbidden evidence, protected token preserved evidence, no ProgramData delete, no protected token delete, locked-file abort before partial delete, locked-file abort diagnostics, partial product root delete forbidden evidence, cleanup diagnostics evidence, no-auto-reboot evidence | `none` | `none` |
| service uninstall remove-data request | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `mixed-history` | `dotnet-service-action` | `windows-native-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | REMOVE_DATA=1 request validation, explicit remove-data intent source, service deleted/absent precondition, service deletion confirmation required, handoff descriptor only, data-root-remove handoff evidence, no direct data root mutation, no direct ProgramData delete, no direct protected token delete, missing-service idempotent diagnostics, no-auto-reboot evidence | `none` | `none` |
| data root remove | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `product-wrapper` | `dotnet-data-root-action` | `data-root-lifecycle-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | `REMOVE_DATA=1` explicit opt-in, remove-data handoff descriptor required, exact data root path allowlist, owned data root marker/evidence, service deleted/absent precondition, installed service blocks delete diagnostics, protected token delete only within owned data root, no product root mutation, no service mutation, locked-file abort before partial delete, delete manifest/journal evidence, post-delete absence evidence, no partial delete success evidence, diagnostics evidence, no-auto-reboot evidence | `none` | `none` |
| local payload update | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `product-wrapper` | `windows-native-package` | `package-contract` | `blocked` | `blocked` | `explicit-admin-opt-in` | signed/approved package manifest required, manifest hash verification, ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation, exact product root ownership evidence, service stopped precondition, active root snapshot before activation, staged root outside active root, binary payload only activation, no config mutation, no data root mutation, no token mutation, no service identity mutation, atomic activation or full rollback, partial activation forbidden evidence, post-activation manifest/version evidence, service start health check, rollback attempt on failure, rollback result diagnostics, no-auto-reboot evidence | `release-approval-required` | `none` |
| rollback restore | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `product-wrapper` | `windows-native-package` | `package-contract` | `blocked` | `blocked` | `explicit-admin-opt-in` | retained previous root, previous root manifest/hash verification, previous root ownership evidence, ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation, service stopped precondition, current active root snapshot before rollback, staged rollback root outside active root, binary payload only restore, no config mutation, no data root mutation, no token mutation, no service identity mutation, atomic rollback or current root preservation, failed root preservation, partial restore forbidden evidence, invalid previous manifest rejection, post-rollback manifest/version evidence, rollback health check after restore, rollback result diagnostics, no-auto-reboot evidence | `release-approval-required` | `none` |
| product config schema validation | `product-operation` | `product-ops` | `tier1-read-only` | `product-wrapper` | `dotnet-runtime` | `dotnet-runtime` | `blocked` | `blocked` | `none` | read-only config inventory, owned config path evidence, schema version parse evidence, config schema compatibility, dry-run validation before service start, service-start preflight decision descriptor only, validation failure diagnostics, diagnostics redaction evidence, no config write, no backup write, no service mutation, no migration execution | `none` | `none` |
| product config migration apply | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `not-yet-defined` | `dotnet-config-migration-action` | `product-config-migration-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | current config source inventory, current schema owner resolution, owned source config path evidence, source path/version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, validation preflight descriptor required, backup path inside owned config backup root, atomic config replace, no data root mutation, no token mutation, no job store mutation, no service identity mutation, partial config migration forbidden evidence, rollback on migration failure, rollback result diagnostics, cleanup evidence, service-start preflight decision descriptor only, validation writes forbidden, explicit admin opt-in before config write | `none` | `none` |
| job store schema mismatch detection | `product-operation` | `product-ops` | `tier1-read-only` | `dotnet-runtime` | `dotnet-runtime` | `dotnet-runtime` | `blocked` | `blocked` | `none` | `read-only-or-blocked-with-diagnostics` schema mismatch behavior, schema mismatch returns blocked diagnostics, runtime read must not mutate jobs.json, no quarantine move/write, migration handoff descriptor only, no migration execution, diagnostics evidence | `none` | `none` |
| job store migration apply | `product-operation` | `product-ops` | `tier3-destructive-or-persistent` | `dotnet-runtime` | `dotnet-job-store-migration-action` | `job-store-migration-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | current job store path inventory, current job schema owner evidence, owned job store path evidence, source job store version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, runtime writer stopped evidence, backup path inside owned job-store backup root, destructive rewrite disabled by default, atomic job store replace, no config mutation, no token mutation, no service identity mutation, partial job store migration forbidden evidence, rollback on migration failure, rollback result diagnostics, recovery evidence, explicit admin opt-in before job store write | `none` | `none` |
| Event Log source registration | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `powershell-helper` | `windows-eventlog-action` | `eventlog-registration-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | explicit admin opt-in, exact event source name, exact channel/log name, owned event source manifest/evidence, missing-or-owned-source precondition, foreign-source conflict blocks, exact log/source binding, no overwrite of existing foreign source, registry write limited to event source registration, no service mutation, no firewall mutation, no trust store mutation, conflict diagnostics only, post-registration binding evidence, no MSI/default execution, no-auto-reboot evidence | `none` | `none` |
| Event Log source removal | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `powershell-helper` | `windows-eventlog-action` | `eventlog-registration-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | explicit admin opt-in, exact event source name, exact channel/log name, owned event source manifest/evidence, exact log/source binding, owned-source-only removal, foreign-source conflict blocks, registry delete limited to owned event source registration, no service mutation, no firewall mutation, no trust store mutation, missing-source idempotency, cleanup diagnostics only, post-removal absence evidence, no MSI/default execution, no-auto-reboot evidence | `none` | `none` |
| firewall rule enable LAN exposure | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `powershell-helper` | `windows-firewall-action` | `firewall-rule-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence, no default install/repair/MSI execution, no-auto-reboot evidence | `none` | `lan-exposure-approval-required` |
| firewall rule removal | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `powershell-helper` | `windows-firewall-action` | `firewall-rule-plan` | `blocked` | `blocked` | `explicit-admin-opt-in` | explicit admin opt-in, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence, owned-rule-only removal, foreign-rule conflict blocks, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution, no-auto-reboot evidence | `none` | `none` |
| trust store install | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `powershell-helper` | `windows-trust-store-action` | `windows-certificate-store-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | release approval, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, no service mutation, no firewall mutation, no eventlog mutation, thumbprint/store binding evidence, post-install trust binding evidence, no default install/repair/MSI execution, no-auto-reboot evidence | `release-approval-required` | `none` |
| trust store removal | `product-operation` | `operating-system-ops` | `tier3-destructive-or-persistent` | `powershell-helper` | `windows-trust-store-action` | `windows-certificate-store-api` | `blocked` | `blocked` | `explicit-admin-opt-in` | release approval, explicit admin opt-in, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, owned certificate evidence, thumbprint/store binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, certificate store delete limited to owned certificate, no service mutation, no firewall mutation, no eventlog mutation, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution, no-auto-reboot evidence | `release-approval-required` | `none` |

## Package Contract Execution Guard

- `local payload update`, `rollback restore`는 ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation이 모두 있어야 실행할 수 있다.
- `local payload update`는 package manifest/hash/root ownership/service stopped evidence 또는 ADR-0002 channel/version/update payload binding이 불명확하면 activation 없이 blocked diagnostics만 반환한다.
- `rollback restore`는 previous root/hash/ownership/service stopped evidence 또는 ADR-0002 channel/version/previous root slot binding이 불명확하면 restore 없이 blocked diagnostics만 반환한다.

## OS Mutation Execution Guard

- `Event Log source registration`, `Event Log source removal`, `firewall rule enable LAN exposure`, `firewall rule removal`, `trust store install`, `trust store removal`은 기본 install/repair/diagnostics/MSI 경로에서 실행하지 않는다.
- `Event Log source registration`, `Event Log source removal`은 source 등록과 제거를 별도 explicit admin opt-in smoke에서만 실행하고, 기본 diagnostics는 deferred policy와 host mutation 미수행 evidence만 기록한다.
- `Event Log source removal`은 source/channel ownership 또는 log/source binding이 불명확하면 registry delete 없이 blocked diagnostics만 반환한다.
- `firewall rule enable LAN exposure`는 `network_exposure_gate = lan-exposure-approval-required`, LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence가 모두 있어야 실행할 수 있다.
- `firewall rule enable LAN exposure`는 rule tuple/ownership/scope가 불명확하면 firewall write 없이 blocked diagnostics만 반환한다.
- `firewall rule removal`은 explicit admin opt-in, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence, owned-rule-only removal, foreign-rule conflict blocks, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence가 모두 있어야 실행할 수 있으며 LAN exposure를 열지 않는다.
- `firewall rule removal`은 rule tuple/ownership/scope가 불명확하면 firewall delete 없이 blocked diagnostics만 반환한다.
- `trust store install`은 `release_gate = release-approval-required`, release approval, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, no service mutation, no firewall mutation, no eventlog mutation, thumbprint/store binding evidence, post-install trust binding evidence가 모두 있어야 실행할 수 있다.
- `trust store install`은 artifact/identity/store ownership이 불명확하면 certificate store write 없이 blocked diagnostics만 반환한다.
- `trust store removal`은 `release_gate = release-approval-required`, release approval, explicit admin opt-in, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, owned certificate evidence, thumbprint/store binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, certificate store delete limited to owned certificate, no service mutation, no firewall mutation, no eventlog mutation, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence가 모두 있어야 실행할 수 있다.
- `trust store removal`은 certificate identity/store ownership이 불명확하면 certificate store delete 없이 blocked diagnostics만 반환한다.
- 여섯 row의 실행 evidence는 no-auto-reboot와 mutation 전후 diagnostics를 포함해야 한다.

External system executable execution is not included in `windows-native-api`. `sc.exe`, `msiexec.exe`, and `netsh.exe` are examples of possible `approved-system-executable` candidates, not an allowlist. This first slice must keep zero matrix rows with `implementation_basis = approved-system-executable`. If a later slice needs any non-PowerShell executable, that operation must use `implementation_basis = approved-system-executable` and first define ADR/task approval required, exact executable path and publisher/hash evidence, non-shell invocation only, argument schema with allowed flags/values, no user-controlled raw arguments, working directory fixed, environment variable allowlist, no token/secret on command line, no implicit reboot, timeout/exit-code contract, stdout/stderr redaction, dry-run/WhatIf where supported, no chained shell, admin opt-in, post-run evidence, and examples are candidates only, not allowlist in its implementation plan. If executable identity or argument ownership is unclear, the implementation basis remains blocked.

## Promotion Rule

Tier 1 promoted 조건:

- C# WMI/CIM adapter가 helper contract와 같은 public field를 반환한다.
- unsupported host, missing feature, access denied, not found error contract가 고정된다.
- xUnit contract test와 installed non-mutating route smoke가 통과한다.
- transition fallback 제거 조건이 닫힌다.

Tier 2 promoted 조건:

- Tier 1 조건을 만족한다.
- queued/running/succeeded/failed job state가 기존 public contract와 호환된다.
- cancel/retry/idempotency/timeout behavior가 테스트된다.
- 실패 중간 상태에서 cleanup 또는 safe recovery evidence가 있다.
- 관리자 opt-in smoke가 자동 reboot 없이 통과한다.

Tier 3 promoted 조건:

- Tier 2 조건을 만족한다.
- explicit admin opt-in smoke가 있다.
- no-auto-reboot evidence가 있다.
- rollback 또는 remove-data cleanup evidence가 있다.
- signing/channel/provenance policy와 충돌하지 않는다.
- diagnostics bundle이 변경 전후 상태와 cleanup 결과를 설명한다.

Release gate 조건:

- `release_gate = none`은 release execution 승인 없이 GA-ready promotion 판단이 가능하다.
- `release_gate = release-approval-required`는 GA-ready promotion이 가능해도 stable publication, public trusted signing, external release, signed update/rollback 실행을 별도 release approval 전까지 금지한다.
- `release_gate = release-approval-required`는 `local payload update`, `rollback restore`, `trust store install`, `trust store removal` row에만 허용한다.
- `local payload update`, `rollback restore`, `trust store install`, `trust store removal` row는 반드시 `release_gate = release-approval-required`여야 한다.

Release-gated pre-release evidence boundary:

- `release_gate = release-approval-required` row는 ADR-0004 승격 전에 `blocked`를 해소할 수 있지만, 그 근거는 release execution이 아니라 pre-release evidence여야 한다.
- 허용 evidence는 package/trust contract validation, manifest/hash/provenance validation, dry-run planning, non-mutating ownership checks, rollback plan validation, redaction evidence, no-auto-reboot evidence다.
- 금지 evidence는 stable publication, public trusted signing execution, certificate store write/delete, external update/rollback activation이다.
- Release approval 전에는 이 row가 `ga-ready-candidate`가 될 수는 있어도 execution-approved가 될 수 없다.

Network exposure gate 조건:

- `network_exposure_gate = none`은 LAN exposure approval 없이 GA-ready promotion 판단이 가능하다.
- `network_exposure_gate = lan-exposure-approval-required`는 release approval과 별개로 loopback-only 기본 정책, LAN mode opt-in, token source, firewall scope 변경 approval이 필요하다.
- `network_exposure_gate = lan-exposure-approval-required`는 `firewall rule enable LAN exposure` row에만 허용한다.
- `firewall rule enable LAN exposure` row는 반드시 `network_exposure_gate = lan-exposure-approval-required`여야 한다.
- `firewall rule removal` row는 반드시 `network_exposure_gate = none`이어야 한다.

LAN exposure pre-approval evidence boundary:

- `network_exposure_gate = lan-exposure-approval-required` row는 LAN exposure approval 전에 `blocked`를 해소할 수 있지만, 그 근거는 firewall execution이 아니라 pre-LAN evidence여야 한다.
- 허용 evidence는 rule tuple validation, loopback default preservation proof, token source proof, non-mutating firewall ownership checks, scope planning, conflict diagnostics, redaction evidence, no-auto-reboot evidence다.
- 금지 evidence는 firewall rule create/update/delete, non-loopback listener exposure, token source mutation, external network reachability proof다.
- LAN approval 전에는 이 row가 `ga-ready-candidate`가 될 수는 있어도 exposure-approved가 될 수 없다.
~~~~

- [ ] **Step 3: Extend the Pester test for route matrix content**

Replace the test body with this version:

```powershell
    It 'documents the GA-ready product runtime candidate without changing the current decision' {
        $adrCandidatePath = Join-Path $script:RepoRoot 'docs/adr/0004-ga-ready-product-runtime-candidate.md'
        $redesignSpecPath = Join-Path $script:RepoRoot 'docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md'
        $routeMatrixPath = Join-Path $script:RepoRoot 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'

        Test-Path -LiteralPath $adrCandidatePath | Should -BeTrue
        Test-Path -LiteralPath $redesignSpecPath | Should -BeTrue
        Test-Path -LiteralPath $routeMatrixPath | Should -BeTrue

        $adrCandidate = Get-Content -LiteralPath $adrCandidatePath -Raw
        $redesignSpec = Get-Content -LiteralPath $redesignSpecPath -Raw
        $routeMatrix = Get-Content -LiteralPath $routeMatrixPath -Raw

        $adrCandidate | Should -Match '상태: 제안'
        $adrCandidate | Should -Match '대체 대상: 승인 시 ADR-0001 대체'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adrCandidate | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $adrCandidate | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
        $adrCandidate | Should -Match '승인 시 목표 상태는 PowerShell-free product ops/runtime'
        $adrCandidate | Should -Match '현재 적용 결정이 아니다'
        $adrCandidate | Should -Match 'GA gate와 release gate 분리'
        $adrCandidate | Should -Match 'Release execution'
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
        $adrCandidate | Should -Match 'ADR-0004를 current decision으로 승격하지 않는다'
        $adrCandidate | Should -Match '## Aggregate Gate Closure Report'
        $adrCandidate | Should -Match 'aggregate-gate-closure-<YYYY-MM-DD>\.md'
        $adrCandidate | Should -Match 'aggregate_gate_status = closed'
        $adrCandidate | Should -Match '첫 Phase 26 alignment slice에서는 closure report를 만들지 않는다'
        $adrCandidate | Should -Match '## ADR-0001 Replacement Scope'
        $adrCandidate | Should -Match '대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단'
        $adrCandidate | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $adrCandidate | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $adrCandidate | Should -Match '## Current Decision Promotion Procedure'
        $adrCandidate | Should -Match '이 Phase 26 alignment slice와 별도 PR'
        $adrCandidate | Should -Match 'ADR-0004 상태를 `적용 중`'
        $adrCandidate | Should -Match '제안 중인 ADR 후보 섹션에서 제거'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 하나만'

        $redesignSpec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $redesignSpec | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $redesignSpec | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
        $redesignSpec | Should -Match 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'
        $redesignSpec | Should -Match '상세 route contract'
        $redesignSpec | Should -Not -Match '\| Route/Operation \|'
        $redesignSpec | Should -Not -Match 'DELETE /api/v1/vms/\{id\}/checkpoints/\{name\}'

        $routeMatrix | Should -Match '## Field Schema'
        $routeMatrix | Should -Match '## API Route Matrix'
        $routeMatrix | Should -Match '## Product Ops Matrix'
        $routeMatrix | Should -Match 'implementation_basis'
        $routeMatrix | Should -Match 'route_surface'
        $routeMatrix | Should -Match 'current-route'
        $routeMatrix | Should -Match 'future-route'
        $routeMatrix | Should -Match 'product-operation'
        $routeMatrix | Should -Match 'not-implemented'
        $routeMatrix | Should -Match 'route_surface = future-route'
        $routeMatrix | Should -Match '## State Invariants'
        $routeMatrix | Should -Match '## Route Surface Invariants'
        $routeMatrix | Should -Match '## Served Route Scope Rule'
        $routeMatrix | Should -Match 'side-by-side contract-only route 후보'
        $routeMatrix | Should -Match '`GET /api/v1/jobs`는 현재 contract-only 후보'
        $routeMatrix | Should -Match 'Job runtime read surface는 현재 `GET /api/v1/jobs/\{job_id\}` row'
        $routeMatrix | Should -Match 'Contract mirror aggregate route 후보인 `POST /api/v1/vms/\{vmId\}/lifecycle/\{action\}`'
        $routeMatrix | Should -Match 'VM lifecycle served surface는 현재 `POST /api/v1/vms/\{id\}/start`, `shutdown`, `poweroff`, `restart` 개별 row'
        $routeMatrix | Should -Not -Match '\| `GET /api/v1/jobs` \|'
        $routeMatrix | Should -Match '## Future Route Execution Guard'
        $routeMatrix | Should -Match 'Phase 26 alignment slice에서 구현하거나 실제 Local API route로 등록하지 않는다'
        $routeMatrix | Should -Match '별도 implementation plan'
        $routeMatrix | Should -Match 'route contract'
        $routeMatrix | Should -Match 'not-found/idempotency contract'
        $routeMatrix | Should -Match 'destructive cleanup proof'
        $routeMatrix | Should -Match 'explicit admin opt-in evidence'
        $routeMatrix | Should -Match '## Native-First Helper Fallback Rule'
        $routeMatrix | Should -Match 'current_owner = dotnet-native'
        $routeMatrix | Should -Match 'topology parity가 불완전할 때 PowerShell helper fallback'
        $routeMatrix | Should -Match 'promotion_state = current-native'
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
        $routeMatrix | Should -Match '## Current Owner Invariants'
        $routeMatrix | Should -Match '## Current Owner Resolution Rule'
        $routeMatrix | Should -Match '## Mixed History Resolution Rule'
        $routeMatrix | Should -Match '`mixed-history`은 service product operation row에만 허용한다'
        $routeMatrix | Should -Match 'actual current code path와 evidence source'
        $routeMatrix | Should -Match '`mixed-history` 자체를 promotion evidence 또는 target owner로 간주하지 않는다'
        $routeMatrix | Should -Match '## Target Owner Invariants'
        $routeMatrix | Should -Match '## Implementation Basis Invariants'
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
        $routeMatrix | Should -Match 'current owner가 갱신되기 전까지 aggregate GA-ready gate closure로 계산할 수 없다'
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
        $routeMatrix | Should -Match '## Aggregate Gate Closure Report Candidate'
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
        $routeMatrix | Should -Match 'ADR 후보와 supporting docs만 만들며 ADR-0004를 current decision으로 승격하지 않는다'
        $routeMatrix | Should -Match 'closure report 없이 진행할 수 없다'
        $routeMatrix | Should -Match '현재 적용 중인 ADR 표'
        $routeMatrix | Should -Match '제안 중인 ADR 후보 섹션'
        $routeMatrix | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 current source는 하나만'
        $routeMatrix | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $routeMatrix | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $routeMatrix | Should -Match 'missing preserved non-promotion current marker'
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
        $routeMatrix | Should -Match '## Evidence Ledger Candidate'
        $routeMatrix | Should -Match 'docs/ga-ready/evidence/'
        $routeMatrix | Should -Match 'Markdown evidence ledger 후보'
        $routeMatrix | Should -Match '첫 slice에서는 실제 evidence 파일을 만들지 않는다'
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
        $routeMatrix | Should -Match 'ADR-0004를 current decision으로 승격하지 않는다'
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
        $routeMatrix | Should -Match 'owned rule evidence'
        $routeMatrix | Should -Match 'missing-or-owned-rule precondition'
        $routeMatrix | Should -Match 'foreign-rule conflict blocks'
        $routeMatrix | Should -Match 'exact rule name'
        $routeMatrix | Should -Match 'exact direction'
        $routeMatrix | Should -Match 'exact protocol'
        $routeMatrix | Should -Match 'exact local port'
        $routeMatrix | Should -Match 'exact profile'
        $routeMatrix | Should -Match 'exact remote address scope'
        $routeMatrix | Should -Match 'no overwrite of existing foreign rule'
        $routeMatrix | Should -Match 'firewall write limited to owned allow rule'
        $routeMatrix | Should -Match 'firewall delete limited to owned allow rule'
        $routeMatrix | Should -Match 'no eventlog mutation'
        $routeMatrix | Should -Match 'conflict diagnostics only'
        $routeMatrix | Should -Match 'post-enable rule binding evidence'
        $routeMatrix | Should -Match 'owned-rule-only removal'
        $routeMatrix | Should -Match 'missing-rule idempotency'
        $routeMatrix | Should -Match 'post-removal absence evidence'
        $routeMatrix | Should -Match 'no default install/repair/MSI execution'
        $routeMatrix | Should -Match '## Auth and Exposure Boundary'
        $routeMatrix | Should -Match 'single_bearer_token'
        $routeMatrix | Should -Match 'multi_user = false'
        $routeMatrix | Should -Match 'rbac = false'
        $routeMatrix | Should -Match 'loopback static asset bypass'
        $routeMatrix | Should -Match 'unauthenticated-static-only'
        $routeMatrix | Should -Match 'non-loopback static assets require bearer auth'
        $routeMatrix | Should -Match 'LAN mode requires `-AllowLan` and a token source'
        $routeMatrix | Should -Match 'PCV_LAN_TOKEN_REQUIRED'
        $routeMatrix | Should -Match 'PCV_PREFIX_NOT_LOOPBACK'
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
        $routeMatrix | Should -Match 'initial install path'
        $routeMatrix | Should -Match 'missing-service precondition'
        $routeMatrix | Should -Match 'service name ownership identity'
        $routeMatrix | Should -Match 'foreign service conflict blocks'
        $routeMatrix | Should -Match 'SCM service identity'
        $routeMatrix | Should -Match 'exact SCM binary path/product root binding'
        $routeMatrix | Should -Match 'no overwrite of existing foreign service'
        $routeMatrix | Should -Match 'conflict diagnostics only'
        $routeMatrix | Should -Match 'service account'
        $routeMatrix | Should -Match 'start type'
        $routeMatrix | Should -Match 'failure policy'
        $routeMatrix | Should -Match 'idempotent already-installed behavior'
        $routeMatrix | Should -Match 'existing owned service precondition'
        $routeMatrix | Should -Match 'owned-field-only config update'
        $routeMatrix | Should -Match 'foreign binary path blocks'
        $routeMatrix | Should -Match 'config drift diagnostics before mutation'
        $routeMatrix | Should -Match 'config drift diff'
        $routeMatrix | Should -Match 'listener args update'
        $routeMatrix | Should -Match 'rollback/recovery on failed config update'
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
        $routeMatrix | Should -Match 'repair path only'
        $routeMatrix | Should -Match 'service absent precondition'
        $routeMatrix | Should -Match 'product root exists'
        $routeMatrix | Should -Match 'owned product root evidence'
        $routeMatrix | Should -Match 'existing config reuse'
        $routeMatrix | Should -Match 'existing config ownership evidence'
        $routeMatrix | Should -Match 'protected token path preservation'
        $routeMatrix | Should -Match 'protected token ownership evidence'
        $routeMatrix | Should -Match 'config schema validation before recreate'
        $routeMatrix | Should -Match 'foreign existing service blocks'
        $routeMatrix | Should -Match 'no product root creation/removal'
        $routeMatrix | Should -Match 'no config rewrite'
        $routeMatrix | Should -Match 'no token rewrite'
        $routeMatrix | Should -Match 'no data root creation'
        $routeMatrix | Should -Match 'no token bootstrap'
        $routeMatrix | Should -Match 'service uninstall stop/delete'
        $routeMatrix | Should -Match 'product root removal preserve-data'
        $routeMatrix | Should -Match 'service uninstall remove-data request'
        $routeMatrix | Should -Not -Match '\| service uninstall preserve-data \|'
        $routeMatrix | Should -Not -Match '\| service uninstall remove-data \|'
        $routeMatrix | Should -Match 'data root remove'
        $routeMatrix | Should -Match 'SCM service recreate'
        $routeMatrix | Should -Match 'owned-field-only repair'
        $routeMatrix | Should -Match 'allowed repair drift fields = protected token path, listener args'
        $routeMatrix | Should -Match 'non-repair drift handoff to service configure update'
        $routeMatrix | Should -Match 'no SCM recreate'
        $routeMatrix | Should -Match 'protected token path/listener args update'
        $routeMatrix | Should -Match 'rollback/recovery'
        $routeMatrix | Should -Not -Match 'conditional 3010'
        $routeMatrix | Should -Match 'owned service identity'
        $routeMatrix | Should -Match 'stop-before-delete sequencing'
        $routeMatrix | Should -Match 'delete service only'
        $routeMatrix | Should -Match 'stop idempotency'
        $routeMatrix | Should -Match 'delete idempotency'
        $routeMatrix | Should -Match 'service deletion confirmation'
        $routeMatrix | Should -Match 'missing-service idempotency'
        $routeMatrix | Should -Match 'missing-service idempotent diagnostics'
        $routeMatrix | Should -Match 'no product root delete'
        $routeMatrix | Should -Match 'no data root delete'
        $routeMatrix | Should -Match 'no config delete'
        $routeMatrix | Should -Match 'no token delete'
        $routeMatrix | Should -Match 'no REMOVE_DATA handoff'
        $routeMatrix | Should -Match 'service absent/deleted precondition'
        $routeMatrix | Should -Match 'owned product root evidence'
        $routeMatrix | Should -Match 'exact product root allowlist'
        $routeMatrix | Should -Match 'binary payload only delete'
        $routeMatrix | Should -Match 'config/data/token preserve allowlist'
        $routeMatrix | Should -Match 'ProgramData preserve evidence'
        $routeMatrix | Should -Match 'data root delete forbidden evidence'
        $routeMatrix | Should -Match 'protected token preserved evidence'
        $routeMatrix | Should -Match 'data-root-remove handoff evidence'
        $routeMatrix | Should -Match 'REMOVE_DATA=1 request validation'
        $routeMatrix | Should -Match 'explicit remove-data intent source'
        $routeMatrix | Should -Match 'service deleted/absent precondition'
        $routeMatrix | Should -Match 'service deletion confirmation required'
        $routeMatrix | Should -Match 'handoff descriptor only'
        $routeMatrix | Should -Match 'no direct data root mutation'
        $routeMatrix | Should -Match 'no direct ProgramData delete'
        $routeMatrix | Should -Match 'no direct protected token delete'
        $routeMatrix | Should -Match 'no ProgramData delete'
        $routeMatrix | Should -Match 'no protected token delete'
        $routeMatrix | Should -Match 'locked-file abort before partial delete'
        $routeMatrix | Should -Match 'partial product root delete forbidden evidence'
        $routeMatrix | Should -Match 'cleanup diagnostics evidence'
        $routeMatrix | Should -Match 'REMOVE_DATA=1'
        $routeMatrix | Should -Match 'remove-data handoff descriptor required'
        $routeMatrix | Should -Match 'exact data root path allowlist'
        $routeMatrix | Should -Match 'owned data root marker/evidence'
        $routeMatrix | Should -Match 'service deleted/absent precondition'
        $routeMatrix | Should -Match 'installed service blocks delete diagnostics'
        $routeMatrix | Should -Match 'protected token delete only within owned data root'
        $routeMatrix | Should -Match 'no product root mutation'
        $routeMatrix | Should -Match 'no service mutation'
        $routeMatrix | Should -Match 'delete manifest/journal evidence'
        $routeMatrix | Should -Match 'post-delete absence evidence'
        $routeMatrix | Should -Match 'locked-file abort diagnostics'
        $routeMatrix | Should -Match 'no partial delete success evidence'
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
        $routeMatrix | Should -Match 'source 등록과 제거를 별도 explicit admin opt-in smoke'
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
        $routeMatrix | Should -Match 'owned rule evidence'
        $routeMatrix | Should -Match 'missing-or-owned-rule precondition'
        $routeMatrix | Should -Match 'foreign-rule conflict blocks'
        $routeMatrix | Should -Match 'exact rule name'
        $routeMatrix | Should -Match 'exact direction'
        $routeMatrix | Should -Match 'exact protocol'
        $routeMatrix | Should -Match 'exact local port'
        $routeMatrix | Should -Match 'exact profile'
        $routeMatrix | Should -Match 'exact remote address scope'
        $routeMatrix | Should -Match 'no overwrite of existing foreign rule'
        $routeMatrix | Should -Match 'firewall write limited to owned allow rule'
        $routeMatrix | Should -Match 'firewall delete limited to owned allow rule'
        $routeMatrix | Should -Match 'no eventlog mutation'
        $routeMatrix | Should -Match 'conflict diagnostics only'
        $routeMatrix | Should -Match 'post-enable rule binding evidence'
        $routeMatrix | Should -Match 'owned-rule-only removal'
        $routeMatrix | Should -Match 'missing-rule idempotency'
        $routeMatrix | Should -Match 'post-removal absence evidence'
        $routeMatrix | Should -Match 'no default install/repair/MSI execution'
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
        $routeMatrix | Should -Match 'signed/approved package manifest required'
        $routeMatrix | Should -Match 'manifest hash verification'
        $routeMatrix | Should -Match 'ADR-0002 channel/version contract binding'
        $routeMatrix | Should -Match 'source/target release_channel evidence'
        $routeMatrix | Should -Match 'update payload manifest version match'
        $routeMatrix | Should -Match 'from-version/to-version compatibility'
        $routeMatrix | Should -Match 'rc/stable RequireSigned trust_model evidence'
        $routeMatrix | Should -Match 'downgrade forbidden except rollback'
        $routeMatrix | Should -Match 'single previous root slot'
        $routeMatrix | Should -Match 'data root preservation'
        $routeMatrix | Should -Match 'failed root diagnostics preservation'
        $routeMatrix | Should -Match 'exact product root ownership evidence'
        $routeMatrix | Should -Match 'service stopped precondition'
        $routeMatrix | Should -Match 'active root snapshot before activation'
        $routeMatrix | Should -Match 'staged root outside active root'
        $routeMatrix | Should -Match 'binary payload only activation'
        $routeMatrix | Should -Match 'no config mutation'
        $routeMatrix | Should -Match 'no data root mutation'
        $routeMatrix | Should -Match 'no token mutation'
        $routeMatrix | Should -Match 'no service identity mutation'
        $routeMatrix | Should -Match 'atomic activation or full rollback'
        $routeMatrix | Should -Match 'partial activation forbidden evidence'
        $routeMatrix | Should -Match 'post-activation manifest/version evidence'
        $routeMatrix | Should -Match 'service start health check'
        $routeMatrix | Should -Match 'rollback attempt on failure'
        $routeMatrix | Should -Match 'rollback result diagnostics'
        $routeMatrix | Should -Match 'retained previous root'
        $routeMatrix | Should -Match 'previous root manifest/hash verification'
        $routeMatrix | Should -Match 'previous root ownership evidence'
        $routeMatrix | Should -Match 'current active root snapshot before rollback'
        $routeMatrix | Should -Match 'staged rollback root outside active root'
        $routeMatrix | Should -Match 'binary payload only restore'
        $routeMatrix | Should -Match 'atomic rollback or current root preservation'
        $routeMatrix | Should -Match 'failed root preservation'
        $routeMatrix | Should -Match 'partial restore forbidden evidence'
        $routeMatrix | Should -Match 'invalid previous manifest rejection'
        $routeMatrix | Should -Match 'post-rollback manifest/version evidence'
        $routeMatrix | Should -Match 'rollback health check after restore'
        $routeMatrix | Should -Not -Match '\| update/rollback \|'
        $routeMatrix | Should -Match 'product config schema validation'
        $routeMatrix | Should -Match 'product config migration apply'
        $routeMatrix | Should -Not -Match '\| product config migration \|'
        $routeMatrix | Should -Match 'job store schema mismatch detection'
        $routeMatrix | Should -Match 'job store migration apply'
        $routeMatrix | Should -Match 'read-only config inventory'
        $routeMatrix | Should -Match 'owned config path evidence'
        $routeMatrix | Should -Match 'schema version parse evidence'
        $routeMatrix | Should -Match 'config schema compatibility'
        $routeMatrix | Should -Match 'dry-run validation before service start'
        $routeMatrix | Should -Match 'service-start preflight decision descriptor only'
        $routeMatrix | Should -Match 'validation failure diagnostics'
        $routeMatrix | Should -Match 'diagnostics redaction evidence'
        $routeMatrix | Should -Match 'no config write'
        $routeMatrix | Should -Match 'no backup write'
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
        $routeMatrix | Should -Match 'future route implementation plan'
        $routeMatrix | Should -Not -Match '/checkpoints/\{name\}'
        $routeMatrix | Should -Match 'name`/`checkpoint_name'
        $routeMatrix | Should -Match '원본 job operation'
        $routeMatrix | Should -Match 'GA-ready gate, release gate, network exposure gate'
        $routeMatrix | Should -Match 'not-yet-defined'
        $routeMatrix | Should -Match 'current_owner = not-yet-defined'
        $routeMatrix | Should -Match 'dotnet-host-adapter'
        $routeMatrix | Should -Match 'dotnet-hyperv-adapter'
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
        $routeMatrix | Should -Match 'registry-wmi-service'
        $routeMatrix | Should -Match 'windows-native-api'
        $routeMatrix | Should -Match 'product-config-migration-plan'
        $routeMatrix | Should -Match 'implementation_basis = product-config-migration-plan'
        $routeMatrix | Should -Match 'job-store-migration-plan'
        $routeMatrix | Should -Match 'implementation_basis = job-store-migration-plan'
        $routeMatrix | Should -Match 'dpapi-local-machine-token-plan'
        $routeMatrix | Should -Match 'implementation_basis = dpapi-local-machine-token-plan'
        $routeMatrix | Should -Match 'token source inventory, single-source precondition, existing protected token no-overwrite'
        $routeMatrix | Should -Match 'legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required'
        $routeMatrix | Should -Match 'command line token value forbidden, diagnostics redaction evidence'
        $routeMatrix | Should -Match 'windows-certificate-store-api'
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
        $routeMatrix | Should -Match 'approved-system-executable'
        $routeMatrix | Should -Match 'not an allowlist'
        $routeMatrix | Should -Match '## Approved System Executable Rule'
        $routeMatrix | Should -Match '현재 matrix row에서는 사용 count가 반드시 0'
        $routeMatrix | Should -Match 'first slice must keep zero matrix rows'
        $routeMatrix | Should -Match 'ADR/task approval required'
        $routeMatrix | Should -Match 'exact executable path and publisher/hash evidence'
        $routeMatrix | Should -Match 'non-shell invocation only'
        $routeMatrix | Should -Match 'argument schema with allowed flags/values'
        $routeMatrix | Should -Match 'no user-controlled raw arguments'
        $routeMatrix | Should -Match 'working directory fixed'
        $routeMatrix | Should -Match 'environment variable allowlist'
        $routeMatrix | Should -Match 'no token/secret on command line'
        $routeMatrix | Should -Match 'no implicit reboot'
        $routeMatrix | Should -Match 'timeout/exit-code contract'
        $routeMatrix | Should -Match 'stdout/stderr redaction'
        $routeMatrix | Should -Match 'dry-run/WhatIf where supported'
        $routeMatrix | Should -Match 'no chained shell'
        $routeMatrix | Should -Match 'admin opt-in'
        $routeMatrix | Should -Match 'post-run evidence'
        $routeMatrix | Should -Match 'implementation basis remains blocked'
        $routeMatrix | Should -Match 'wmi-cim'
        $routeMatrix | Should -Match 'tier1-read-only'
        $routeMatrix | Should -Match 'tier2-reversible-mutation'
        $routeMatrix | Should -Match 'tier3-destructive-or-persistent'
        $routeMatrix | Should -Match 'dotnet-request-processor-powershell-helper'
        $routeMatrix | Should -Match 'transition-helper'
        $routeMatrix | Should -Match 'GA-ready blocker'
        $routeMatrix | Should -Match 'GET /api/v1/vms'

        $schemaEnums = @{}
        foreach ($line in ($routeMatrix -split "`r?`n")) {
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
        $matrixRows.Name | Should -Not -Contain '`GET /api/v1/jobs`'
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
                $row.Name | Should -Be '`DELETE /api/v1/vms/{id}`'
                $row.CurrentOwner | Should -Be 'not-implemented'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.GaReadyGate | Should -Match 'future route implementation plan'
                $row.GaReadyGate | Should -Match 'not-found/idempotency contract'
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
                $row.FallbackPolicy | Should -Be 'transition-helper'
                $row.PromotionState | Should -Be 'transition-helper'
                $row.GaReadyGate | Should -Match 'fallback 제거'
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
            if ($row.CurrentOwner -eq 'mixed-history') {
                $serviceProductOpsRows | Should -Contain $row.Name
                $row.RouteSurface | Should -Be 'product-operation'
                $row.Domain | Should -Be 'product-ops'
                $row.TargetOwner | Should -Be 'dotnet-service-action'
                $row.PromotionState | Should -Be 'blocked'
            }
            if ($serviceProductOpsRows -contains $row.Name) {
                $row.CurrentOwner | Should -Be 'mixed-history'
                $row.TargetOwner | Should -Not -Be 'mixed-history'
                $row.PromotionState | Should -Be 'blocked'
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
                $row.GaReadyGate | Should -Match 'initial install path'
                $row.GaReadyGate | Should -Match 'missing-service precondition'
                $row.GaReadyGate | Should -Match 'service name ownership identity'
                $row.GaReadyGate | Should -Match 'foreign service conflict blocks'
                $row.GaReadyGate | Should -Match 'SCM service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'no overwrite of existing foreign service'
                $row.GaReadyGate | Should -Match 'conflict diagnostics only'
                $row.GaReadyGate | Should -Match 'binary path'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args'
                $row.GaReadyGate | Should -Match 'service account'
                $row.GaReadyGate | Should -Match 'start type'
                $row.GaReadyGate | Should -Match 'failure policy'
                $row.GaReadyGate | Should -Match 'idempotent already-installed behavior'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'protected token bootstrap'
                $row.GaReadyGate | Should -Not -Match 'existing config reuse'
                $row.GaReadyGate | Should -Not -Match 'repair path only'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
            }
            if ($row.Name -eq 'service configure update') {
                $row.GaReadyGate | Should -Match 'existing owned service precondition'
                $row.GaReadyGate | Should -Match 'owned-field-only config update'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'foreign binary path blocks'
                $row.GaReadyGate | Should -Match 'config drift diagnostics before mutation'
                $row.GaReadyGate | Should -Match 'config drift diff'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args update'
                $row.GaReadyGate | Should -Match 'data preservation'
                $row.GaReadyGate | Should -Match 'rollback/recovery on failed config update'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'missing-service precondition'
                $row.GaReadyGate | Should -Not -Match 'no overwrite of existing foreign service'
            }
            if ($row.Name -eq 'service repair missing service recreation') {
                $row.GaReadyGate | Should -Match 'repair path only'
                $row.GaReadyGate | Should -Match 'service absent precondition'
                $row.GaReadyGate | Should -Match 'product root exists'
                $row.GaReadyGate | Should -Match 'owned product root evidence'
                $row.GaReadyGate | Should -Match 'existing config reuse'
                $row.GaReadyGate | Should -Match 'existing config ownership evidence'
                $row.GaReadyGate | Should -Match 'protected token path preservation'
                $row.GaReadyGate | Should -Match 'protected token ownership evidence'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'config schema validation before recreate'
                $row.GaReadyGate | Should -Match 'foreign existing service blocks'
                $row.GaReadyGate | Should -Match 'SCM service recreate'
                $row.GaReadyGate | Should -Match 'SCM binary path'
                $row.GaReadyGate | Should -Match 'service identity'
                $row.GaReadyGate | Should -Match 'no product root creation/removal'
                $row.GaReadyGate | Should -Match 'no config rewrite'
                $row.GaReadyGate | Should -Match 'no token rewrite'
                $row.GaReadyGate | Should -Match 'no data root creation'
                $row.GaReadyGate | Should -Match 'no token bootstrap'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
                $row.GaReadyGate | Should -Not -Match 'initial install path'
            }
            if ($row.Name -eq 'service repair config drift correction') {
                $row.GaReadyGate | Should -Match 'repair path only'
                $row.GaReadyGate | Should -Match 'existing owned service'
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'owned-field-only repair'
                $row.GaReadyGate | Should -Match 'allowed repair drift fields = protected token path, listener args'
                $row.GaReadyGate | Should -Match 'config drift diagnostics before mutation'
                $row.GaReadyGate | Should -Match 'config drift diff'
                $row.GaReadyGate | Should -Match 'protected token path/listener args update'
                $row.GaReadyGate | Should -Match 'foreign binary path blocks'
                $row.GaReadyGate | Should -Match 'non-repair drift handoff to service configure update'
                $row.GaReadyGate | Should -Match 'data preservation'
                $row.GaReadyGate | Should -Match 'rollback/recovery'
                $row.GaReadyGate | Should -Match 'no SCM recreate'
                $row.GaReadyGate | Should -Match 'no config rewrite'
                $row.GaReadyGate | Should -Match 'no token rewrite'
                $row.GaReadyGate | Should -Match 'no product root creation/removal'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'SCM service recreate'
                $row.GaReadyGate | Should -Not -Match 'service absent precondition'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
            }
            if ($row.Name -eq 'service uninstall stop/delete') {
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'foreign service blocks'
                $row.GaReadyGate | Should -Match 'stop-before-delete sequencing'
                $row.GaReadyGate | Should -Match 'stop idempotency'
                $row.GaReadyGate | Should -Match 'delete service only'
                $row.GaReadyGate | Should -Match 'delete idempotency'
                $row.GaReadyGate | Should -Match 'service deletion confirmation'
                $row.GaReadyGate | Should -Match 'missing-service idempotency'
                $row.GaReadyGate | Should -Match 'missing-service idempotent diagnostics'
                $row.GaReadyGate | Should -Match 'no product root delete'
                $row.GaReadyGate | Should -Match 'no data root delete'
                $row.GaReadyGate | Should -Match 'no config delete'
                $row.GaReadyGate | Should -Match 'no token delete'
                $row.GaReadyGate | Should -Match 'no REMOVE_DATA handoff'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'product root allowlist'
                $row.GaReadyGate | Should -Not -Match 'ProgramData preserve evidence'
                $row.GaReadyGate | Should -Not -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Not -Match 'REMOVE_DATA=1 request validation'
            }
            if ($row.Name -eq 'product root removal preserve-data') {
                $row.GaReadyGate | Should -Match 'service absent/deleted precondition'
                $row.GaReadyGate | Should -Match 'owned product root evidence'
                $row.GaReadyGate | Should -Match 'exact product root allowlist'
                $row.GaReadyGate | Should -Match 'binary payload only delete'
                $row.GaReadyGate | Should -Match 'config/data/token preserve allowlist'
                $row.GaReadyGate | Should -Match 'ProgramData preserve evidence'
                $row.GaReadyGate | Should -Match 'data root delete forbidden evidence'
                $row.GaReadyGate | Should -Match 'protected token preserved evidence'
                $row.GaReadyGate | Should -Match 'no ProgramData delete'
                $row.GaReadyGate | Should -Match 'no protected token delete'
                $row.GaReadyGate | Should -Match 'locked-file abort before partial delete'
                $row.GaReadyGate | Should -Match 'locked-file abort diagnostics'
                $row.GaReadyGate | Should -Match 'partial product root delete forbidden evidence'
                $row.GaReadyGate | Should -Match 'cleanup diagnostics evidence'
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
                $row.GaReadyGate | Should -Match 'REMOVE_DATA=1 request validation'
                $row.GaReadyGate | Should -Match 'explicit remove-data intent source'
                $row.GaReadyGate | Should -Match 'service deleted/absent precondition'
                $row.GaReadyGate | Should -Match 'service deletion confirmation required'
                $row.GaReadyGate | Should -Match 'handoff descriptor only'
                $row.GaReadyGate | Should -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Match 'no direct data root mutation'
                $row.GaReadyGate | Should -Match 'no direct ProgramData delete'
                $row.GaReadyGate | Should -Match 'no direct protected token delete'
                $row.GaReadyGate | Should -Match 'missing-service idempotent diagnostics'
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
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-runtime'
                $row.ImplementationBasis | Should -Be 'dotnet-runtime'
                $row.RiskTier | Should -Be 'tier1-read-only'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'read-only config inventory'
                $row.GaReadyGate | Should -Match 'owned config path evidence'
                $row.GaReadyGate | Should -Match 'schema version parse evidence'
                $row.GaReadyGate | Should -Match 'config schema compatibility'
                $row.GaReadyGate | Should -Match 'dry-run validation before service start'
                $row.GaReadyGate | Should -Match 'service-start preflight decision descriptor only'
                $row.GaReadyGate | Should -Match 'validation failure diagnostics'
                $row.GaReadyGate | Should -Match 'diagnostics redaction evidence'
                $row.GaReadyGate | Should -Match 'no config write'
                $row.GaReadyGate | Should -Match 'no backup write'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no migration execution'
                $row.GaReadyGate | Should -Not -Match 'service-start block on validation failure'
                $row.GaReadyGate | Should -Not -Match 'validation writes forbidden evidence'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.CurrentOwner | Should -Be 'not-yet-defined'
            }

            if ($row.TargetOwner -eq 'dotnet-config-migration-action') {
                $row.Name | Should -Be 'product config migration apply'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.TargetOwner | Should -Be 'dotnet-config-migration-action'
                $row.ImplementationBasis | Should -Be 'product-config-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'current config source inventory'
                $row.GaReadyGate | Should -Match 'current schema owner resolution'
                $row.GaReadyGate | Should -Match 'owned source config path evidence'
                $row.GaReadyGate | Should -Match 'source path/version evidence'
                $row.GaReadyGate | Should -Match 'source/target schema version evidence'
                $row.GaReadyGate | Should -Match 'migration plan id/version'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'validation preflight descriptor required'
                $row.GaReadyGate | Should -Match 'backup path inside owned config backup root'
                $row.GaReadyGate | Should -Match 'atomic config replace'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no job store mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'partial config migration forbidden evidence'
                $row.GaReadyGate | Should -Match 'rollback on migration failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'cleanup evidence'
                $row.GaReadyGate | Should -Match 'service-start preflight decision descriptor only'
                $row.GaReadyGate | Should -Match 'validation writes forbidden'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in before config write'
                $row.GaReadyGate | Should -Not -Match 'backup/restore'
                $row.GaReadyGate | Should -Not -Match 'service-start health check'
                $row.GaReadyGate | Should -Not -Match 'explicit admin opt-in before config/data mutation'
            }
            if ($row.TargetOwner -eq 'dotnet-token-storage-action') {
                $row.Name | Should -Be 'protected token bootstrap'
            }
            if ($row.Name -eq 'protected token bootstrap') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-token-storage-action'
                $row.ImplementationBasis | Should -Be 'dpapi-local-machine-token-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'raw token 비노출'
                $row.GaReadyGate | Should -Match 'token source inventory'
                $row.GaReadyGate | Should -Match 'single-source precondition'
                $row.GaReadyGate | Should -Match 'existing protected token no-overwrite'
                $row.GaReadyGate | Should -Match 'legacy token migration'
                $row.GaReadyGate | Should -Match 'legacy raw migration only when protected token missing'
                $row.GaReadyGate | Should -Match 'source conflict diagnostics'
                $row.GaReadyGate | Should -Match 'owned legacy token source required'
                $row.GaReadyGate | Should -Match 'protected token schema'
                $row.GaReadyGate | Should -Match 'ACL hardening'
                $row.GaReadyGate | Should -Match 'service command line protected file path only'
                $row.GaReadyGate | Should -Match 'command line token value forbidden'
                $row.GaReadyGate | Should -Match 'diagnostics redaction evidence'
            }
            if ($row.TargetOwner -eq 'dotnet-data-root-action') {
                $row.Name | Should -Be 'data root remove'
            }
            if ($row.Name -eq 'data root remove') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-data-root-action'
                $row.ImplementationBasis | Should -Be 'data-root-lifecycle-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'REMOVE_DATA=1'
                $row.GaReadyGate | Should -Match 'remove-data handoff descriptor required'
                $row.GaReadyGate | Should -Match 'exact data root path allowlist'
                $row.GaReadyGate | Should -Match 'owned data root marker/evidence'
                $row.GaReadyGate | Should -Match 'service deleted/absent precondition'
                $row.GaReadyGate | Should -Match 'installed service blocks delete diagnostics'
                $row.GaReadyGate | Should -Match 'protected token delete only within owned data root'
                $row.GaReadyGate | Should -Match 'no product root mutation'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'locked-file abort before partial delete'
                $row.GaReadyGate | Should -Match 'locked-file abort diagnostics'
                $row.GaReadyGate | Should -Match 'delete manifest/journal evidence'
                $row.GaReadyGate | Should -Match 'post-delete absence evidence'
                $row.GaReadyGate | Should -Match 'no partial delete success evidence'
                $row.GaReadyGate | Should -Match 'diagnostics evidence'
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
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'read-only-or-blocked-with-diagnostics'
                $row.GaReadyGate | Should -Match 'schema mismatch behavior'
                $row.GaReadyGate | Should -Match 'schema mismatch returns blocked diagnostics'
                $row.GaReadyGate | Should -Match 'runtime read must not mutate jobs.json'
                $row.GaReadyGate | Should -Match 'no quarantine move/write'
                $row.GaReadyGate | Should -Match 'migration handoff descriptor only'
                $row.GaReadyGate | Should -Match 'no migration execution'
                $row.GaReadyGate | Should -Match 'diagnostics evidence'
                $row.GaReadyGate | Should -Not -Match 'current quarantine move/write behavior'
                $row.GaReadyGate | Should -Not -Match 'moved under explicit'
                $row.GaReadyGate | Should -Not -Match 'atomic job store replace'
                $row.GaReadyGate | Should -Not -Match 'destructive rewrite disabled by default'
            }
            if ($row.Name -eq 'job store migration apply') {
                $row.CurrentOwner | Should -Be 'dotnet-runtime'
                $row.TargetOwner | Should -Be 'dotnet-job-store-migration-action'
                $row.ImplementationBasis | Should -Be 'job-store-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'current job store path inventory'
                $row.GaReadyGate | Should -Match 'current job schema owner evidence'
                $row.GaReadyGate | Should -Match 'owned job store path evidence'
                $row.GaReadyGate | Should -Match 'source job store version evidence'
                $row.GaReadyGate | Should -Match 'source/target schema version evidence'
                $row.GaReadyGate | Should -Match 'migration plan id/version'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Match 'runtime writer stopped evidence'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'backup path inside owned job-store backup root'
                $row.GaReadyGate | Should -Match 'destructive rewrite disabled by default'
                $row.GaReadyGate | Should -Match 'atomic job store replace'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'partial job store migration forbidden evidence'
                $row.GaReadyGate | Should -Match 'rollback on migration failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'recovery evidence'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in before job store write'
                $row.GaReadyGate | Should -Not -Match 'backup/restore'
                $row.GaReadyGate | Should -Not -Match 'explicit admin opt-in before data mutation'
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
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
            }
            if ($row.Name -eq 'Event Log source registration') {
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
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'signed/approved package manifest required'
                $row.GaReadyGate | Should -Match 'manifest hash verification'
                $row.GaReadyGate | Should -Match 'ADR-0002 channel/version contract binding'
                $row.GaReadyGate | Should -Match 'source/target release_channel evidence'
                $row.GaReadyGate | Should -Match 'update payload manifest version match'
                $row.GaReadyGate | Should -Match 'from-version/to-version compatibility'
                $row.GaReadyGate | Should -Match 'rc/stable RequireSigned trust_model evidence'
                $row.GaReadyGate | Should -Match 'downgrade forbidden except rollback'
                $row.GaReadyGate | Should -Match 'single previous root slot'
                $row.GaReadyGate | Should -Match 'data root preservation'
                $row.GaReadyGate | Should -Match 'failed root diagnostics preservation'
                $row.GaReadyGate | Should -Match 'exact product root ownership evidence'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'active root snapshot before activation'
                $row.GaReadyGate | Should -Match 'staged root outside active root'
                $row.GaReadyGate | Should -Match 'binary payload only activation'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'atomic activation or full rollback'
                $row.GaReadyGate | Should -Match 'partial activation forbidden evidence'
                $row.GaReadyGate | Should -Match 'post-activation manifest/version evidence'
                $row.GaReadyGate | Should -Not -Match 'config migration dry-run'
                $row.GaReadyGate | Should -Match 'service start health check'
                $row.GaReadyGate | Should -Match 'rollback attempt on failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'manifest/payload version match'
                $row.GaReadyGate | Should -Not -Match 'staged payload activation'
                $row.GaReadyGate | Should -Not -Match 'product config schema validation pass required'
            }
            if ($row.Name -eq 'rollback restore') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'windows-native-package'
                $row.ImplementationBasis | Should -Be 'package-contract'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'retained previous root'
                $row.GaReadyGate | Should -Match 'previous root manifest/hash verification'
                $row.GaReadyGate | Should -Match 'previous root ownership evidence'
                $row.GaReadyGate | Should -Match 'ADR-0002 channel/version contract binding'
                $row.GaReadyGate | Should -Match 'source/target release_channel evidence'
                $row.GaReadyGate | Should -Match 'update payload manifest version match'
                $row.GaReadyGate | Should -Match 'from-version/to-version compatibility'
                $row.GaReadyGate | Should -Match 'rc/stable RequireSigned trust_model evidence'
                $row.GaReadyGate | Should -Match 'downgrade forbidden except rollback'
                $row.GaReadyGate | Should -Match 'single previous root slot'
                $row.GaReadyGate | Should -Match 'data root preservation'
                $row.GaReadyGate | Should -Match 'failed root diagnostics preservation'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'current active root snapshot before rollback'
                $row.GaReadyGate | Should -Match 'staged rollback root outside active root'
                $row.GaReadyGate | Should -Match 'binary payload only restore'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'atomic rollback or current root preservation'
                $row.GaReadyGate | Should -Match 'failed root preservation'
                $row.GaReadyGate | Should -Match 'partial restore forbidden evidence'
                $row.GaReadyGate | Should -Match 'invalid previous manifest rejection'
                $row.GaReadyGate | Should -Match 'post-rollback manifest/version evidence'
                $row.GaReadyGate | Should -Match 'rollback health check after restore'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'staged rollback activation'
            }

            if ($row.NetworkExposureGate -eq 'lan-exposure-approval-required') {
                $row.Name | Should -Be 'firewall rule enable LAN exposure'
            }
            if ($row.Name -eq 'firewall rule enable LAN exposure') {
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
    }
```

- [ ] **Step 4: Run the targeted test**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1' -FullName '*GA-ready product runtime candidate*' -Output Detailed"
```

Expected:

```text
Tests Passed: 1, Failed: 0
```

## Task 4: Add Migration and Verification Ownership Maps

**Files:**

- Create: `docs/ga-ready/REPO_MIGRATION_MAP.md`
- Create: `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [ ] **Step 1: Create the repo migration map**

Create `docs/ga-ready/REPO_MIGRATION_MAP.md` with exactly this content:

~~~~markdown
# GA-ready Repo Migration Map

이 문서는 GA-ready redesign 후보에서 `spikes/**`를 활성 제품 경로에서 제거하거나 archive로 이동하기 위한 migration target을 고정한다.

현재 적용 결정은 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`다. 이 문서는 현재 적용 ADR을 즉시 대체하지 않는다.

## Target Layout

```text
src/
  DesktopNode.Host/
  DesktopNode.Api/
  DesktopNode.Runtime/
  DesktopNode.Contracts/
  DesktopNode.HyperV/
  DesktopNode.Service/
web/
  package.json
  src/
  tests/
packaging/
  windows-desktop-node/
docs/
  adr/
  superpowers/
  ga-ready/
archive/
  spikes/
```

## Migration Rule

- 제품 runtime source는 `src/**` 또는 `web/**`에 둔다.
- `spikes/**`는 historical/archive baseline으로 축소한다.
- 경로 이동은 behavior 변경과 분리한다.
- 문서 링크와 검증 command는 migration map에서 함께 갱신한다.
- `packaging/windows-desktop-node/**`의 product root/source root contract는 migration slice마다 검증한다.

## Migration Execution Guard

- 이 map은 승인 시 목표 상태를 기록한 target map이며, 파일 이동 실행 승인이 아니다.
- 파일 이동은 별도 implementation plan에서 import/package/test path 갱신 범위, rollback 기준, archive target 검증을 정의하기 전에는 실행하지 않는다.
- 각 migration slice는 behavior 변경과 경로 이동을 분리하고, 관련 문서 링크와 검증 command 갱신을 같은 review scope에 포함해야 한다.
- 첫 slice는 파일 이동을 하지 않는다.
- 후속 migration 전에는 source path inventory, import/relative path graph, packaging/static asset input binding, generated parity manifest update, docs command update, no behavior change evidence, archive target read-only intent, rollback restore 기준, 관련 Pester/npm/`verify:parity`/`node --check` evidence가 필요하다.
- 위 evidence가 없으면 migration은 blocked로 둔다.

## Map

| Current path | Active product target | Archive target | Migration condition |
|---|---|---|---|
| `spikes/purecvisor-desktop-node/api/**` | `src/DesktopNode.Api/**` | `archive/spikes/api/**` | route matrix owner가 .NET으로 이동한 뒤 archive |
| `spikes/purecvisor-desktop-node/hyperv/**` | `src/DesktopNode.HyperV/**` | `archive/spikes/hyperv/**` | C# WMI/CIM parity route가 promoted 된 뒤 archive |
| `spikes/purecvisor-desktop-node/service/**` | `src/DesktopNode.Service/**` | `archive/spikes/service/**` | product service-action의 승인 시 목표 상태가 PowerShell-free가 된 뒤 archive |
| `spikes/purecvisor-desktop-node/cli/**` | 없음 | `archive/spikes/cli/**` | GA-ready target에는 포함하지 않고, 제품 CLI가 필요하면 별도 `src/DesktopNode.Cli/**` 설계로 추가 |
| `spikes/purecvisor-desktop-node/web/**` | `web/**` | `archive/spikes/web/**` | TypeScript build output serving이 기본값이 된 뒤 archive |
| `packaging/windows-desktop-node/**` | `packaging/windows-desktop-node/**` | 없음 | PowerShell orchestration 제거 slice별 갱신 |

## First Migration Constraint

첫 migration implementation은 파일 이동을 하지 않는다. 먼저 route matrix, ADR 후보, verification ownership이 current docs와 tests에 연결되어야 한다.
~~~~

- [ ] **Step 2: Create the verification ownership map**

Create `docs/ga-ready/VERIFICATION_OWNERSHIP.md` with exactly this content:

~~~~markdown
# GA-ready Verification Ownership

이 문서는 GA-ready redesign 후보에서 제품 검증 primary owner를 Pester 중심 legacy suite에서 xUnit/npm/browser-level fixture 후보/package contract 중심으로 옮기는 기준을 고정한다.

현재 적용 결정은 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`다. 이 문서는 현재 적용 ADR을 즉시 대체하지 않는다.

## Ownership Rule

- .NET product path는 xUnit과 installed smoke contract를 primary로 둔다.
- Web Console은 npm과 TypeScript typecheck를 primary로 두며, browser-level fixture는 후속 후보로만 둔다.
- Browser-level fixture 구현 도구는 후속 implementation slice에서 선택하며, 첫 후보 범위는 npm/package-owned loopback fixture로 제한한다.
- Playwright는 후속 도구 후보이며 이 alignment slice에서 dependency로 도입하지 않는다.
- Packaging은 installer/package contract tests와 signed/unsigned channel policy tests를 primary로 둔다.
- Operator smoke는 no-auto-reboot, install/repair/uninstall/remove-data, update/rollback, diagnostics bundle, cleanup evidence를 유지한다.
- Pester는 PowerShell component/runtime behavior suite에서 archive compatibility verification으로 축소한다.
- Root documentation guard와 policy synchronization guard는 ADR/문서 경계 검증으로 유지한다.

## Pester Retirement Gate

- 첫 slice에서 Pester suite는 계속 required verification이다.
- Suite별 retirement는 대체 xUnit/npm/package/browser fixture evidence가 생긴 뒤에만 허용한다.
- 각 retirement는 owner replacement, equivalent coverage mapping, archive baseline path, docs command update, CI/local command replacement, rollback 기준을 기록해야 한다.
- PowerShell helper 또는 `spikes/**`가 active path에 남아 있으면 해당 Pester suite는 archive-only로 낮추지 않는다.

## Browser-level Fixture Candidate Contract

- Browser-level fixture 후보는 Web Console package가 소유하는 후속 npm/package-owned loopback fixture 후보로만 둔다.
- 첫 구현 slice의 최소 검증 대상은 static asset load, initial render, deterministic `GET /api/v1/runtime/policy` connection, optional bearer 401/200 handling, token/redaction 확인으로 좁힌다.
- Browser-level fixture의 제외 범위는 API route contract, route parity, Hyper-V, service/MSI/firewall/Event Log/trust store mutation, LAN exposure, Playwright required dependency다.
- Browser-level fixture는 API route contract, route parity, Hyper-V mutation, installer lifecycle, release signing 검증을 대체하지 않는다.
- Playwright는 이 fixture를 구현할 때 검토할 후속 도구 후보이며, 이 alignment slice의 required dependency가 아니다.

## Diagnostics and Redaction Boundary

- Diagnostics evidence는 diagnostics bundle manifest, `events.jsonl`, `install.jsonl`, service logs, lifecycle step name, exit code, redacted tool stdout/stderr, cleanup result를 포함할 수 있다.
- Diagnostics evidence는 raw bearer token, API token, `Authorization` header value, `api-token.dpapi.json` content, legacy raw token file content, password, private key, PFX password, certificate secret material을 포함하면 안 된다.
- Release/signing diagnostics는 certificate file path, private key path, PFX password, signing tool secret arguments를 redacted value로만 기록한다.
- Path redaction은 repo root와 data root를 각각 `[REPO_ROOT]`, `[DATA_ROOT]` token으로 치환한다.
- Redaction evidence는 operation code, artifact name, sanitized path token, exit code, cleanup status처럼 secret 없는 troubleshooting field를 유지해야 한다.

## Data Root Lifecycle Boundary

- Program Files product root lifecycle과 ProgramData data root lifecycle은 분리한다.
- 기본 uninstall은 ProgramData data root를 보존한다.
- Repair는 protected token file, legacy raw token file, job store, `events.jsonl`, `install.jsonl`, diagnostics directory를 보존한다.
- `REMOVE_DATA=1` 또는 explicit `RemoveData`만 ProgramData delete target을 연다.
- `REMOVE_DATA=1` delete target은 `api-token.dpapi.json`, `api-token.txt`, `jobs.json`, `events.jsonl`, `install.jsonl`, diagnostics directory로 제한한다.
- Service host log directory는 현재 RemoveData delete target에 포함하지 않는다.
- WiX는 ProgramData path 계산만 담당하고 data-root ACL을 직접 소유하지 않는다.
- Product action `data_acl` policy가 sensitive token file ACL ownership, SYSTEM/Administrators boundary, `RemoveData` 전 ACL repair를 소유한다.
- ACL repair 대상 sensitive token file은 `api-token.dpapi.json`과 `api-token.txt`다.

## Map

| Area | Current verification | Target verification | Transition rule |
|---|---|---|---|
| API contract | Pester + xUnit | xUnit + installed route smoke | route owner가 .NET이면 xUnit이 primary |
| Hyper-V read routes | Pester + admin smoke | xUnit adapter tests + installed non-mutating smoke | helper fallback 제거 전까지 both |
| Hyper-V mutation routes | Pester + admin opt-in | xUnit job tests + admin opt-in route smoke | destructive operation은 explicit opt-in 유지 |
| Web Console | Pester static tests + npm parity | npm + TypeScript + 후속 browser-level fixture 후보 | served build output 전환 전까지 parity 유지 |
| Packaging/MSI | Pester installer tests | package contract tests + installed lifecycle smoke | PowerShell 제거 slice마다 package tests 갱신 |
| Release/signing | Pester/build script checks | channel/provenance/signing contract + signed lifecycle evidence | public/internal trust model 구분 유지 |

## Non-mutating Default

기본 검증은 host mutation을 실행하지 않는다. 실제 Hyper-V mutation, service install create, service configure update, service repair missing service recreation, service repair config drift correction, service uninstall stop/delete, product root removal preserve-data, service uninstall remove-data request, service start, service stop, firewall rule enable/removal, Event Log source registration/removal, trust store install/removal, MSI lifecycle은 explicit admin opt-in smoke에서만 실행한다.
~~~~

- [ ] **Step 3: Extend the Pester test for all supporting docs**

Replace the test body with this version:

```powershell
    It 'documents the GA-ready product runtime candidate without changing the current decision' {
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

        $adrCandidate | Should -Match '상태: 제안'
        $adrCandidate | Should -Match '대체 대상: 승인 시 ADR-0001 대체'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adrCandidate | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $adrCandidate | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
        $adrCandidate | Should -Match '승인 시 목표 상태는 PowerShell-free product ops/runtime'
        $adrCandidate | Should -Match '현재 적용 결정이 아니다'
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
        $adrCandidate | Should -Match 'ADR-0004를 current decision으로 승격하지 않는다'
        $adrCandidate | Should -Match '## Aggregate Gate Closure Report'
        $adrCandidate | Should -Match 'aggregate-gate-closure-<YYYY-MM-DD>\.md'
        $adrCandidate | Should -Match 'aggregate_gate_status = closed'
        $adrCandidate | Should -Match '첫 Phase 26 alignment slice에서는 closure report를 만들지 않는다'
        $adrCandidate | Should -Match '## ADR-0001 Replacement Scope'
        $adrCandidate | Should -Match '대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단'
        $adrCandidate | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $adrCandidate | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $adrCandidate | Should -Match '## Current Decision Promotion Procedure'
        $adrCandidate | Should -Match '이 Phase 26 alignment slice와 별도 PR'
        $adrCandidate | Should -Match 'ADR-0004 상태를 `적용 중`'
        $adrCandidate | Should -Match '제안 중인 ADR 후보 섹션에서 제거'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 하나만'

        $redesignSpec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $redesignSpec | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $redesignSpec | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
        $redesignSpec | Should -Match 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'
        $redesignSpec | Should -Match '상세 route contract'
        $redesignSpec | Should -Not -Match '\| Route/Operation \|'
        $redesignSpec | Should -Not -Match 'DELETE /api/v1/vms/\{id\}/checkpoints/\{name\}'

        $routeMatrix | Should -Match '## Field Schema'
        $routeMatrix | Should -Match '## API Route Matrix'
        $routeMatrix | Should -Match '## Product Ops Matrix'
        $routeMatrix | Should -Match 'implementation_basis'
        $routeMatrix | Should -Match 'route_surface'
        $routeMatrix | Should -Match 'current-route'
        $routeMatrix | Should -Match 'future-route'
        $routeMatrix | Should -Match 'product-operation'
        $routeMatrix | Should -Match 'not-implemented'
        $routeMatrix | Should -Match 'route_surface = future-route'
        $routeMatrix | Should -Match '## State Invariants'
        $routeMatrix | Should -Match '## Route Surface Invariants'
        $routeMatrix | Should -Match '## Served Route Scope Rule'
        $routeMatrix | Should -Match 'side-by-side contract-only route 후보'
        $routeMatrix | Should -Match '`GET /api/v1/jobs`는 현재 contract-only 후보'
        $routeMatrix | Should -Match 'Job runtime read surface는 현재 `GET /api/v1/jobs/\{job_id\}` row'
        $routeMatrix | Should -Match 'Contract mirror aggregate route 후보인 `POST /api/v1/vms/\{vmId\}/lifecycle/\{action\}`'
        $routeMatrix | Should -Match 'VM lifecycle served surface는 현재 `POST /api/v1/vms/\{id\}/start`, `shutdown`, `poweroff`, `restart` 개별 row'
        $routeMatrix | Should -Not -Match '\| `GET /api/v1/jobs` \|'
        $routeMatrix | Should -Match '## Future Route Execution Guard'
        $routeMatrix | Should -Match 'Phase 26 alignment slice에서 구현하거나 실제 Local API route로 등록하지 않는다'
        $routeMatrix | Should -Match '별도 implementation plan'
        $routeMatrix | Should -Match 'route contract'
        $routeMatrix | Should -Match 'not-found/idempotency contract'
        $routeMatrix | Should -Match 'destructive cleanup proof'
        $routeMatrix | Should -Match 'explicit admin opt-in evidence'
        $routeMatrix | Should -Match '## Native-First Helper Fallback Rule'
        $routeMatrix | Should -Match 'current_owner = dotnet-native'
        $routeMatrix | Should -Match 'topology parity가 불완전할 때 PowerShell helper fallback'
        $routeMatrix | Should -Match 'promotion_state = current-native'
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
        $routeMatrix | Should -Match '## Current Owner Invariants'
        $routeMatrix | Should -Match '## Current Owner Resolution Rule'
        $routeMatrix | Should -Match '## Mixed History Resolution Rule'
        $routeMatrix | Should -Match '`mixed-history`은 service product operation row에만 허용한다'
        $routeMatrix | Should -Match 'actual current code path와 evidence source'
        $routeMatrix | Should -Match '`mixed-history` 자체를 promotion evidence 또는 target owner로 간주하지 않는다'
        $routeMatrix | Should -Match '## Target Owner Invariants'
        $routeMatrix | Should -Match '## Implementation Basis Invariants'
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
        $routeMatrix | Should -Match 'current owner가 갱신되기 전까지 aggregate GA-ready gate closure로 계산할 수 없다'
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
        $routeMatrix | Should -Match '## Aggregate Gate Closure Report Candidate'
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
        $routeMatrix | Should -Match 'ADR 후보와 supporting docs만 만들며 ADR-0004를 current decision으로 승격하지 않는다'
        $routeMatrix | Should -Match 'closure report 없이 진행할 수 없다'
        $routeMatrix | Should -Match '현재 적용 중인 ADR 표'
        $routeMatrix | Should -Match '제안 중인 ADR 후보 섹션'
        $routeMatrix | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 current source는 하나만'
        $routeMatrix | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $routeMatrix | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $routeMatrix | Should -Match 'missing preserved non-promotion current marker'
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
        $routeMatrix | Should -Match '## Evidence Ledger Candidate'
        $routeMatrix | Should -Match 'docs/ga-ready/evidence/'
        $routeMatrix | Should -Match 'Markdown evidence ledger 후보'
        $routeMatrix | Should -Match '첫 slice에서는 실제 evidence 파일을 만들지 않는다'
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
        $routeMatrix | Should -Match 'ADR-0004를 current decision으로 승격하지 않는다'
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
        $routeMatrix | Should -Match 'multi_user = false'
        $routeMatrix | Should -Match 'rbac = false'
        $routeMatrix | Should -Match 'loopback static asset bypass'
        $routeMatrix | Should -Match 'unauthenticated-static-only'
        $routeMatrix | Should -Match 'non-loopback static assets require bearer auth'
        $routeMatrix | Should -Match 'LAN mode requires `-AllowLan` and a token source'
        $routeMatrix | Should -Match 'PCV_LAN_TOKEN_REQUIRED'
        $routeMatrix | Should -Match 'PCV_PREFIX_NOT_LOOPBACK'
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
        $routeMatrix | Should -Match 'initial install path'
        $routeMatrix | Should -Match 'missing-service precondition'
        $routeMatrix | Should -Match 'service name ownership identity'
        $routeMatrix | Should -Match 'foreign service conflict blocks'
        $routeMatrix | Should -Match 'SCM service identity'
        $routeMatrix | Should -Match 'exact SCM binary path/product root binding'
        $routeMatrix | Should -Match 'no overwrite of existing foreign service'
        $routeMatrix | Should -Match 'conflict diagnostics only'
        $routeMatrix | Should -Match 'service account'
        $routeMatrix | Should -Match 'start type'
        $routeMatrix | Should -Match 'failure policy'
        $routeMatrix | Should -Match 'idempotent already-installed behavior'
        $routeMatrix | Should -Match 'existing owned service precondition'
        $routeMatrix | Should -Match 'owned-field-only config update'
        $routeMatrix | Should -Match 'foreign binary path blocks'
        $routeMatrix | Should -Match 'config drift diagnostics before mutation'
        $routeMatrix | Should -Match 'config drift diff'
        $routeMatrix | Should -Match 'listener args update'
        $routeMatrix | Should -Match 'rollback/recovery on failed config update'
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
        $routeMatrix | Should -Match 'repair path only'
        $routeMatrix | Should -Match 'service absent precondition'
        $routeMatrix | Should -Match 'product root exists'
        $routeMatrix | Should -Match 'owned product root evidence'
        $routeMatrix | Should -Match 'existing config reuse'
        $routeMatrix | Should -Match 'existing config ownership evidence'
        $routeMatrix | Should -Match 'protected token path preservation'
        $routeMatrix | Should -Match 'protected token ownership evidence'
        $routeMatrix | Should -Match 'config schema validation before recreate'
        $routeMatrix | Should -Match 'foreign existing service blocks'
        $routeMatrix | Should -Match 'no product root creation/removal'
        $routeMatrix | Should -Match 'no config rewrite'
        $routeMatrix | Should -Match 'no token rewrite'
        $routeMatrix | Should -Match 'no data root creation'
        $routeMatrix | Should -Match 'no token bootstrap'
        $routeMatrix | Should -Match 'service uninstall stop/delete'
        $routeMatrix | Should -Match 'product root removal preserve-data'
        $routeMatrix | Should -Match 'service uninstall remove-data request'
        $routeMatrix | Should -Not -Match '\| service uninstall preserve-data \|'
        $routeMatrix | Should -Not -Match '\| service uninstall remove-data \|'
        $routeMatrix | Should -Match 'data root remove'
        $routeMatrix | Should -Match 'SCM service recreate'
        $routeMatrix | Should -Match 'owned-field-only repair'
        $routeMatrix | Should -Match 'allowed repair drift fields = protected token path, listener args'
        $routeMatrix | Should -Match 'non-repair drift handoff to service configure update'
        $routeMatrix | Should -Match 'no SCM recreate'
        $routeMatrix | Should -Match 'protected token path/listener args update'
        $routeMatrix | Should -Match 'rollback/recovery'
        $routeMatrix | Should -Not -Match 'conditional 3010'
        $routeMatrix | Should -Match 'owned service identity'
        $routeMatrix | Should -Match 'stop-before-delete sequencing'
        $routeMatrix | Should -Match 'delete service only'
        $routeMatrix | Should -Match 'stop idempotency'
        $routeMatrix | Should -Match 'delete idempotency'
        $routeMatrix | Should -Match 'service deletion confirmation'
        $routeMatrix | Should -Match 'missing-service idempotency'
        $routeMatrix | Should -Match 'missing-service idempotent diagnostics'
        $routeMatrix | Should -Match 'no product root delete'
        $routeMatrix | Should -Match 'no data root delete'
        $routeMatrix | Should -Match 'no config delete'
        $routeMatrix | Should -Match 'no token delete'
        $routeMatrix | Should -Match 'no REMOVE_DATA handoff'
        $routeMatrix | Should -Match 'service absent/deleted precondition'
        $routeMatrix | Should -Match 'owned product root evidence'
        $routeMatrix | Should -Match 'exact product root allowlist'
        $routeMatrix | Should -Match 'binary payload only delete'
        $routeMatrix | Should -Match 'config/data/token preserve allowlist'
        $routeMatrix | Should -Match 'ProgramData preserve evidence'
        $routeMatrix | Should -Match 'data root delete forbidden evidence'
        $routeMatrix | Should -Match 'protected token preserved evidence'
        $routeMatrix | Should -Match 'data-root-remove handoff evidence'
        $routeMatrix | Should -Match 'REMOVE_DATA=1 request validation'
        $routeMatrix | Should -Match 'explicit remove-data intent source'
        $routeMatrix | Should -Match 'service deleted/absent precondition'
        $routeMatrix | Should -Match 'service deletion confirmation required'
        $routeMatrix | Should -Match 'handoff descriptor only'
        $routeMatrix | Should -Match 'no direct data root mutation'
        $routeMatrix | Should -Match 'no direct ProgramData delete'
        $routeMatrix | Should -Match 'no direct protected token delete'
        $routeMatrix | Should -Match 'no ProgramData delete'
        $routeMatrix | Should -Match 'no protected token delete'
        $routeMatrix | Should -Match 'locked-file abort before partial delete'
        $routeMatrix | Should -Match 'partial product root delete forbidden evidence'
        $routeMatrix | Should -Match 'cleanup diagnostics evidence'
        $routeMatrix | Should -Match 'REMOVE_DATA=1'
        $routeMatrix | Should -Match 'remove-data handoff descriptor required'
        $routeMatrix | Should -Match 'exact data root path allowlist'
        $routeMatrix | Should -Match 'owned data root marker/evidence'
        $routeMatrix | Should -Match 'service deleted/absent precondition'
        $routeMatrix | Should -Match 'installed service blocks delete diagnostics'
        $routeMatrix | Should -Match 'protected token delete only within owned data root'
        $routeMatrix | Should -Match 'no product root mutation'
        $routeMatrix | Should -Match 'no service mutation'
        $routeMatrix | Should -Match 'delete manifest/journal evidence'
        $routeMatrix | Should -Match 'post-delete absence evidence'
        $routeMatrix | Should -Match 'locked-file abort diagnostics'
        $routeMatrix | Should -Match 'no partial delete success evidence'
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
        $routeMatrix | Should -Match 'source 등록과 제거를 별도 explicit admin opt-in smoke'
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
        $routeMatrix | Should -Match 'owned rule evidence'
        $routeMatrix | Should -Match 'missing-or-owned-rule precondition'
        $routeMatrix | Should -Match 'foreign-rule conflict blocks'
        $routeMatrix | Should -Match 'exact rule name'
        $routeMatrix | Should -Match 'exact direction'
        $routeMatrix | Should -Match 'exact protocol'
        $routeMatrix | Should -Match 'exact local port'
        $routeMatrix | Should -Match 'exact profile'
        $routeMatrix | Should -Match 'exact remote address scope'
        $routeMatrix | Should -Match 'no overwrite of existing foreign rule'
        $routeMatrix | Should -Match 'firewall write limited to owned allow rule'
        $routeMatrix | Should -Match 'firewall delete limited to owned allow rule'
        $routeMatrix | Should -Match 'no eventlog mutation'
        $routeMatrix | Should -Match 'conflict diagnostics only'
        $routeMatrix | Should -Match 'post-enable rule binding evidence'
        $routeMatrix | Should -Match 'owned-rule-only removal'
        $routeMatrix | Should -Match 'missing-rule idempotency'
        $routeMatrix | Should -Match 'post-removal absence evidence'
        $routeMatrix | Should -Match 'no default install/repair/MSI execution'
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
        $routeMatrix | Should -Match 'signed/approved package manifest required'
        $routeMatrix | Should -Match 'manifest hash verification'
        $routeMatrix | Should -Match 'ADR-0002 channel/version contract binding'
        $routeMatrix | Should -Match 'source/target release_channel evidence'
        $routeMatrix | Should -Match 'update payload manifest version match'
        $routeMatrix | Should -Match 'from-version/to-version compatibility'
        $routeMatrix | Should -Match 'rc/stable RequireSigned trust_model evidence'
        $routeMatrix | Should -Match 'downgrade forbidden except rollback'
        $routeMatrix | Should -Match 'single previous root slot'
        $routeMatrix | Should -Match 'data root preservation'
        $routeMatrix | Should -Match 'failed root diagnostics preservation'
        $routeMatrix | Should -Match 'exact product root ownership evidence'
        $routeMatrix | Should -Match 'service stopped precondition'
        $routeMatrix | Should -Match 'active root snapshot before activation'
        $routeMatrix | Should -Match 'staged root outside active root'
        $routeMatrix | Should -Match 'binary payload only activation'
        $routeMatrix | Should -Match 'no config mutation'
        $routeMatrix | Should -Match 'no data root mutation'
        $routeMatrix | Should -Match 'no token mutation'
        $routeMatrix | Should -Match 'no service identity mutation'
        $routeMatrix | Should -Match 'atomic activation or full rollback'
        $routeMatrix | Should -Match 'partial activation forbidden evidence'
        $routeMatrix | Should -Match 'post-activation manifest/version evidence'
        $routeMatrix | Should -Match 'service start health check'
        $routeMatrix | Should -Match 'rollback attempt on failure'
        $routeMatrix | Should -Match 'rollback result diagnostics'
        $routeMatrix | Should -Match 'retained previous root'
        $routeMatrix | Should -Match 'previous root manifest/hash verification'
        $routeMatrix | Should -Match 'previous root ownership evidence'
        $routeMatrix | Should -Match 'current active root snapshot before rollback'
        $routeMatrix | Should -Match 'staged rollback root outside active root'
        $routeMatrix | Should -Match 'binary payload only restore'
        $routeMatrix | Should -Match 'atomic rollback or current root preservation'
        $routeMatrix | Should -Match 'failed root preservation'
        $routeMatrix | Should -Match 'partial restore forbidden evidence'
        $routeMatrix | Should -Match 'invalid previous manifest rejection'
        $routeMatrix | Should -Match 'post-rollback manifest/version evidence'
        $routeMatrix | Should -Match 'rollback health check after restore'
        $routeMatrix | Should -Not -Match '\| update/rollback \|'
        $routeMatrix | Should -Match 'product config schema validation'
        $routeMatrix | Should -Match 'product config migration apply'
        $routeMatrix | Should -Not -Match '\| product config migration \|'
        $routeMatrix | Should -Match 'job store schema mismatch detection'
        $routeMatrix | Should -Match 'job store migration apply'
        $routeMatrix | Should -Match 'read-only config inventory'
        $routeMatrix | Should -Match 'owned config path evidence'
        $routeMatrix | Should -Match 'schema version parse evidence'
        $routeMatrix | Should -Match 'config schema compatibility'
        $routeMatrix | Should -Match 'dry-run validation before service start'
        $routeMatrix | Should -Match 'service-start preflight decision descriptor only'
        $routeMatrix | Should -Match 'validation failure diagnostics'
        $routeMatrix | Should -Match 'diagnostics redaction evidence'
        $routeMatrix | Should -Match 'no config write'
        $routeMatrix | Should -Match 'no backup write'
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
        $routeMatrix | Should -Match 'future route implementation plan'
        $routeMatrix | Should -Not -Match '/checkpoints/\{name\}'
        $routeMatrix | Should -Match 'name`/`checkpoint_name'
        $routeMatrix | Should -Match '원본 job operation'
        $routeMatrix | Should -Match 'GA-ready gate, release gate, network exposure gate'
        $routeMatrix | Should -Match 'not-yet-defined'
        $routeMatrix | Should -Match 'current_owner = not-yet-defined'
        $routeMatrix | Should -Match 'dotnet-host-adapter'
        $routeMatrix | Should -Match 'dotnet-hyperv-adapter'
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
        $routeMatrix | Should -Match 'registry-wmi-service'
        $routeMatrix | Should -Match 'windows-native-api'
        $routeMatrix | Should -Match 'product-config-migration-plan'
        $routeMatrix | Should -Match 'implementation_basis = product-config-migration-plan'
        $routeMatrix | Should -Match 'job-store-migration-plan'
        $routeMatrix | Should -Match 'implementation_basis = job-store-migration-plan'
        $routeMatrix | Should -Match 'dpapi-local-machine-token-plan'
        $routeMatrix | Should -Match 'implementation_basis = dpapi-local-machine-token-plan'
        $routeMatrix | Should -Match 'token source inventory, single-source precondition, existing protected token no-overwrite'
        $routeMatrix | Should -Match 'legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required'
        $routeMatrix | Should -Match 'command line token value forbidden, diagnostics redaction evidence'
        $routeMatrix | Should -Match 'windows-certificate-store-api'
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
        $routeMatrix | Should -Match 'approved-system-executable'
        $routeMatrix | Should -Match 'not an allowlist'
        $routeMatrix | Should -Match '## Approved System Executable Rule'
        $routeMatrix | Should -Match '현재 matrix row에서는 사용 count가 반드시 0'
        $routeMatrix | Should -Match 'first slice must keep zero matrix rows'
        $routeMatrix | Should -Match 'ADR/task approval required'
        $routeMatrix | Should -Match 'exact executable path and publisher/hash evidence'
        $routeMatrix | Should -Match 'non-shell invocation only'
        $routeMatrix | Should -Match 'argument schema with allowed flags/values'
        $routeMatrix | Should -Match 'no user-controlled raw arguments'
        $routeMatrix | Should -Match 'working directory fixed'
        $routeMatrix | Should -Match 'environment variable allowlist'
        $routeMatrix | Should -Match 'no token/secret on command line'
        $routeMatrix | Should -Match 'no implicit reboot'
        $routeMatrix | Should -Match 'timeout/exit-code contract'
        $routeMatrix | Should -Match 'stdout/stderr redaction'
        $routeMatrix | Should -Match 'dry-run/WhatIf where supported'
        $routeMatrix | Should -Match 'no chained shell'
        $routeMatrix | Should -Match 'admin opt-in'
        $routeMatrix | Should -Match 'post-run evidence'
        $routeMatrix | Should -Match 'implementation basis remains blocked'
        $routeMatrix | Should -Match 'wmi-cim'
        $routeMatrix | Should -Match 'tier1-read-only'
        $routeMatrix | Should -Match 'tier2-reversible-mutation'
        $routeMatrix | Should -Match 'tier3-destructive-or-persistent'
        $routeMatrix | Should -Match 'dotnet-request-processor-powershell-helper'
        $routeMatrix | Should -Match 'transition-helper'
        $routeMatrix | Should -Match 'GA-ready blocker'
        $routeMatrix | Should -Match 'GET /api/v1/vms'

        $schemaEnums = @{}
        foreach ($line in ($routeMatrix -split "`r?`n")) {
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
        $matrixRows.Name | Should -Not -Contain '`GET /api/v1/jobs`'
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
                $row.Name | Should -Be '`DELETE /api/v1/vms/{id}`'
                $row.CurrentOwner | Should -Be 'not-implemented'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.GaReadyGate | Should -Match 'future route implementation plan'
                $row.GaReadyGate | Should -Match 'not-found/idempotency contract'
            }
            if ($row.CurrentOwner -eq 'not-implemented') {
                $row.RouteSurface | Should -Be 'future-route'
            }
            if ($row.Name -eq '`DELETE /api/v1/vms/{id}`') {
                $row.RouteSurface | Should -Be 'future-route'
                $row.CurrentOwner | Should -Be 'not-implemented'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.GaReadyGate | Should -Match 'future route implementation plan'
                $row.GaReadyGate | Should -Match 'not-found/idempotency contract'
            }

            if ($row.Name -eq '`GET /api/v1/network/inventory`') {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.FallbackPolicy | Should -Be 'transition-helper'
                $row.PromotionState | Should -Be 'transition-helper'
                $row.GaReadyGate | Should -Match 'fallback 제거'
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
            if ($row.CurrentOwner -eq 'mixed-history') {
                $serviceProductOpsRows | Should -Contain $row.Name
                $row.RouteSurface | Should -Be 'product-operation'
                $row.Domain | Should -Be 'product-ops'
                $row.TargetOwner | Should -Be 'dotnet-service-action'
                $row.PromotionState | Should -Be 'blocked'
            }
            if ($serviceProductOpsRows -contains $row.Name) {
                $row.CurrentOwner | Should -Be 'mixed-history'
                $row.TargetOwner | Should -Not -Be 'mixed-history'
                $row.PromotionState | Should -Be 'blocked'
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
                $row.GaReadyGate | Should -Match 'initial install path'
                $row.GaReadyGate | Should -Match 'missing-service precondition'
                $row.GaReadyGate | Should -Match 'service name ownership identity'
                $row.GaReadyGate | Should -Match 'foreign service conflict blocks'
                $row.GaReadyGate | Should -Match 'SCM service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'no overwrite of existing foreign service'
                $row.GaReadyGate | Should -Match 'conflict diagnostics only'
                $row.GaReadyGate | Should -Match 'binary path'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args'
                $row.GaReadyGate | Should -Match 'service account'
                $row.GaReadyGate | Should -Match 'start type'
                $row.GaReadyGate | Should -Match 'failure policy'
                $row.GaReadyGate | Should -Match 'idempotent already-installed behavior'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'protected token bootstrap'
                $row.GaReadyGate | Should -Not -Match 'existing config reuse'
                $row.GaReadyGate | Should -Not -Match 'repair path only'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
            }
            if ($row.Name -eq 'service configure update') {
                $row.GaReadyGate | Should -Match 'existing owned service precondition'
                $row.GaReadyGate | Should -Match 'owned-field-only config update'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'foreign binary path blocks'
                $row.GaReadyGate | Should -Match 'config drift diagnostics before mutation'
                $row.GaReadyGate | Should -Match 'config drift diff'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args update'
                $row.GaReadyGate | Should -Match 'data preservation'
                $row.GaReadyGate | Should -Match 'rollback/recovery on failed config update'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'missing-service precondition'
                $row.GaReadyGate | Should -Not -Match 'no overwrite of existing foreign service'
            }
            if ($row.Name -eq 'service repair missing service recreation') {
                $row.GaReadyGate | Should -Match 'repair path only'
                $row.GaReadyGate | Should -Match 'service absent precondition'
                $row.GaReadyGate | Should -Match 'product root exists'
                $row.GaReadyGate | Should -Match 'owned product root evidence'
                $row.GaReadyGate | Should -Match 'existing config reuse'
                $row.GaReadyGate | Should -Match 'existing config ownership evidence'
                $row.GaReadyGate | Should -Match 'protected token path preservation'
                $row.GaReadyGate | Should -Match 'protected token ownership evidence'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'config schema validation before recreate'
                $row.GaReadyGate | Should -Match 'foreign existing service blocks'
                $row.GaReadyGate | Should -Match 'SCM service recreate'
                $row.GaReadyGate | Should -Match 'SCM binary path'
                $row.GaReadyGate | Should -Match 'service identity'
                $row.GaReadyGate | Should -Match 'no product root creation/removal'
                $row.GaReadyGate | Should -Match 'no config rewrite'
                $row.GaReadyGate | Should -Match 'no token rewrite'
                $row.GaReadyGate | Should -Match 'no data root creation'
                $row.GaReadyGate | Should -Match 'no token bootstrap'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
                $row.GaReadyGate | Should -Not -Match 'initial install path'
            }
            if ($row.Name -eq 'service repair config drift correction') {
                $row.GaReadyGate | Should -Match 'repair path only'
                $row.GaReadyGate | Should -Match 'existing owned service'
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'owned-field-only repair'
                $row.GaReadyGate | Should -Match 'allowed repair drift fields = protected token path, listener args'
                $row.GaReadyGate | Should -Match 'config drift diagnostics before mutation'
                $row.GaReadyGate | Should -Match 'config drift diff'
                $row.GaReadyGate | Should -Match 'protected token path/listener args update'
                $row.GaReadyGate | Should -Match 'foreign binary path blocks'
                $row.GaReadyGate | Should -Match 'non-repair drift handoff to service configure update'
                $row.GaReadyGate | Should -Match 'data preservation'
                $row.GaReadyGate | Should -Match 'rollback/recovery'
                $row.GaReadyGate | Should -Match 'no SCM recreate'
                $row.GaReadyGate | Should -Match 'no config rewrite'
                $row.GaReadyGate | Should -Match 'no token rewrite'
                $row.GaReadyGate | Should -Match 'no product root creation/removal'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'SCM service recreate'
                $row.GaReadyGate | Should -Not -Match 'service absent precondition'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
            }
            if ($row.Name -eq 'service uninstall stop/delete') {
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'foreign service blocks'
                $row.GaReadyGate | Should -Match 'stop-before-delete sequencing'
                $row.GaReadyGate | Should -Match 'stop idempotency'
                $row.GaReadyGate | Should -Match 'delete service only'
                $row.GaReadyGate | Should -Match 'delete idempotency'
                $row.GaReadyGate | Should -Match 'service deletion confirmation'
                $row.GaReadyGate | Should -Match 'missing-service idempotency'
                $row.GaReadyGate | Should -Match 'missing-service idempotent diagnostics'
                $row.GaReadyGate | Should -Match 'no product root delete'
                $row.GaReadyGate | Should -Match 'no data root delete'
                $row.GaReadyGate | Should -Match 'no config delete'
                $row.GaReadyGate | Should -Match 'no token delete'
                $row.GaReadyGate | Should -Match 'no REMOVE_DATA handoff'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'product root allowlist'
                $row.GaReadyGate | Should -Not -Match 'ProgramData preserve evidence'
                $row.GaReadyGate | Should -Not -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Not -Match 'REMOVE_DATA=1 request validation'
            }
            if ($row.Name -eq 'product root removal preserve-data') {
                $row.GaReadyGate | Should -Match 'service absent/deleted precondition'
                $row.GaReadyGate | Should -Match 'owned product root evidence'
                $row.GaReadyGate | Should -Match 'exact product root allowlist'
                $row.GaReadyGate | Should -Match 'binary payload only delete'
                $row.GaReadyGate | Should -Match 'config/data/token preserve allowlist'
                $row.GaReadyGate | Should -Match 'ProgramData preserve evidence'
                $row.GaReadyGate | Should -Match 'data root delete forbidden evidence'
                $row.GaReadyGate | Should -Match 'protected token preserved evidence'
                $row.GaReadyGate | Should -Match 'no ProgramData delete'
                $row.GaReadyGate | Should -Match 'no protected token delete'
                $row.GaReadyGate | Should -Match 'locked-file abort before partial delete'
                $row.GaReadyGate | Should -Match 'locked-file abort diagnostics'
                $row.GaReadyGate | Should -Match 'partial product root delete forbidden evidence'
                $row.GaReadyGate | Should -Match 'cleanup diagnostics evidence'
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
                $row.GaReadyGate | Should -Match 'REMOVE_DATA=1 request validation'
                $row.GaReadyGate | Should -Match 'explicit remove-data intent source'
                $row.GaReadyGate | Should -Match 'service deleted/absent precondition'
                $row.GaReadyGate | Should -Match 'service deletion confirmation required'
                $row.GaReadyGate | Should -Match 'handoff descriptor only'
                $row.GaReadyGate | Should -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Match 'no direct data root mutation'
                $row.GaReadyGate | Should -Match 'no direct ProgramData delete'
                $row.GaReadyGate | Should -Match 'no direct protected token delete'
                $row.GaReadyGate | Should -Match 'missing-service idempotent diagnostics'
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
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-runtime'
                $row.ImplementationBasis | Should -Be 'dotnet-runtime'
                $row.RiskTier | Should -Be 'tier1-read-only'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'read-only config inventory'
                $row.GaReadyGate | Should -Match 'owned config path evidence'
                $row.GaReadyGate | Should -Match 'schema version parse evidence'
                $row.GaReadyGate | Should -Match 'config schema compatibility'
                $row.GaReadyGate | Should -Match 'dry-run validation before service start'
                $row.GaReadyGate | Should -Match 'service-start preflight decision descriptor only'
                $row.GaReadyGate | Should -Match 'validation failure diagnostics'
                $row.GaReadyGate | Should -Match 'diagnostics redaction evidence'
                $row.GaReadyGate | Should -Match 'no config write'
                $row.GaReadyGate | Should -Match 'no backup write'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no migration execution'
                $row.GaReadyGate | Should -Not -Match 'service-start block on validation failure'
                $row.GaReadyGate | Should -Not -Match 'validation writes forbidden evidence'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.CurrentOwner | Should -Be 'not-yet-defined'
            }

            if ($row.TargetOwner -eq 'dotnet-config-migration-action') {
                $row.Name | Should -Be 'product config migration apply'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.TargetOwner | Should -Be 'dotnet-config-migration-action'
                $row.ImplementationBasis | Should -Be 'product-config-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'current config source inventory'
                $row.GaReadyGate | Should -Match 'current schema owner resolution'
                $row.GaReadyGate | Should -Match 'owned source config path evidence'
                $row.GaReadyGate | Should -Match 'source path/version evidence'
                $row.GaReadyGate | Should -Match 'source/target schema version evidence'
                $row.GaReadyGate | Should -Match 'migration plan id/version'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'validation preflight descriptor required'
                $row.GaReadyGate | Should -Match 'backup path inside owned config backup root'
                $row.GaReadyGate | Should -Match 'atomic config replace'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no job store mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'partial config migration forbidden evidence'
                $row.GaReadyGate | Should -Match 'rollback on migration failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'cleanup evidence'
                $row.GaReadyGate | Should -Match 'service-start preflight decision descriptor only'
                $row.GaReadyGate | Should -Match 'validation writes forbidden'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in before config write'
                $row.GaReadyGate | Should -Not -Match 'backup/restore'
                $row.GaReadyGate | Should -Not -Match 'service-start health check'
                $row.GaReadyGate | Should -Not -Match 'explicit admin opt-in before config/data mutation'
            }
            if ($row.TargetOwner -eq 'dotnet-token-storage-action') {
                $row.Name | Should -Be 'protected token bootstrap'
            }
            if ($row.Name -eq 'protected token bootstrap') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-token-storage-action'
                $row.ImplementationBasis | Should -Be 'dpapi-local-machine-token-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'raw token 비노출'
                $row.GaReadyGate | Should -Match 'token source inventory'
                $row.GaReadyGate | Should -Match 'single-source precondition'
                $row.GaReadyGate | Should -Match 'existing protected token no-overwrite'
                $row.GaReadyGate | Should -Match 'legacy token migration'
                $row.GaReadyGate | Should -Match 'legacy raw migration only when protected token missing'
                $row.GaReadyGate | Should -Match 'source conflict diagnostics'
                $row.GaReadyGate | Should -Match 'owned legacy token source required'
                $row.GaReadyGate | Should -Match 'protected token schema'
                $row.GaReadyGate | Should -Match 'ACL hardening'
                $row.GaReadyGate | Should -Match 'service command line protected file path only'
                $row.GaReadyGate | Should -Match 'command line token value forbidden'
                $row.GaReadyGate | Should -Match 'diagnostics redaction evidence'
            }
            if ($row.TargetOwner -eq 'dotnet-data-root-action') {
                $row.Name | Should -Be 'data root remove'
            }
            if ($row.Name -eq 'data root remove') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-data-root-action'
                $row.ImplementationBasis | Should -Be 'data-root-lifecycle-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'REMOVE_DATA=1'
                $row.GaReadyGate | Should -Match 'remove-data handoff descriptor required'
                $row.GaReadyGate | Should -Match 'exact data root path allowlist'
                $row.GaReadyGate | Should -Match 'owned data root marker/evidence'
                $row.GaReadyGate | Should -Match 'service deleted/absent precondition'
                $row.GaReadyGate | Should -Match 'installed service blocks delete diagnostics'
                $row.GaReadyGate | Should -Match 'protected token delete only within owned data root'
                $row.GaReadyGate | Should -Match 'no product root mutation'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'locked-file abort before partial delete'
                $row.GaReadyGate | Should -Match 'locked-file abort diagnostics'
                $row.GaReadyGate | Should -Match 'delete manifest/journal evidence'
                $row.GaReadyGate | Should -Match 'post-delete absence evidence'
                $row.GaReadyGate | Should -Match 'no partial delete success evidence'
                $row.GaReadyGate | Should -Match 'diagnostics evidence'
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
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'read-only-or-blocked-with-diagnostics'
                $row.GaReadyGate | Should -Match 'schema mismatch behavior'
                $row.GaReadyGate | Should -Match 'schema mismatch returns blocked diagnostics'
                $row.GaReadyGate | Should -Match 'runtime read must not mutate jobs.json'
                $row.GaReadyGate | Should -Match 'no quarantine move/write'
                $row.GaReadyGate | Should -Match 'migration handoff descriptor only'
                $row.GaReadyGate | Should -Match 'no migration execution'
                $row.GaReadyGate | Should -Match 'diagnostics evidence'
                $row.GaReadyGate | Should -Not -Match 'current quarantine move/write behavior'
                $row.GaReadyGate | Should -Not -Match 'moved under explicit'
                $row.GaReadyGate | Should -Not -Match 'atomic job store replace'
                $row.GaReadyGate | Should -Not -Match 'destructive rewrite disabled by default'
            }
            if ($row.Name -eq 'job store migration apply') {
                $row.CurrentOwner | Should -Be 'dotnet-runtime'
                $row.TargetOwner | Should -Be 'dotnet-job-store-migration-action'
                $row.ImplementationBasis | Should -Be 'job-store-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'current job store path inventory'
                $row.GaReadyGate | Should -Match 'current job schema owner evidence'
                $row.GaReadyGate | Should -Match 'owned job store path evidence'
                $row.GaReadyGate | Should -Match 'source job store version evidence'
                $row.GaReadyGate | Should -Match 'source/target schema version evidence'
                $row.GaReadyGate | Should -Match 'migration plan id/version'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Match 'runtime writer stopped evidence'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'backup path inside owned job-store backup root'
                $row.GaReadyGate | Should -Match 'destructive rewrite disabled by default'
                $row.GaReadyGate | Should -Match 'atomic job store replace'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'partial job store migration forbidden evidence'
                $row.GaReadyGate | Should -Match 'rollback on migration failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'recovery evidence'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in before job store write'
                $row.GaReadyGate | Should -Not -Match 'backup/restore'
                $row.GaReadyGate | Should -Not -Match 'explicit admin opt-in before data mutation'
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
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
            }
            if ($row.Name -eq 'Event Log source registration') {
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
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'signed/approved package manifest required'
                $row.GaReadyGate | Should -Match 'manifest hash verification'
                $row.GaReadyGate | Should -Match 'ADR-0002 channel/version contract binding'
                $row.GaReadyGate | Should -Match 'source/target release_channel evidence'
                $row.GaReadyGate | Should -Match 'update payload manifest version match'
                $row.GaReadyGate | Should -Match 'from-version/to-version compatibility'
                $row.GaReadyGate | Should -Match 'rc/stable RequireSigned trust_model evidence'
                $row.GaReadyGate | Should -Match 'downgrade forbidden except rollback'
                $row.GaReadyGate | Should -Match 'single previous root slot'
                $row.GaReadyGate | Should -Match 'data root preservation'
                $row.GaReadyGate | Should -Match 'failed root diagnostics preservation'
                $row.GaReadyGate | Should -Match 'exact product root ownership evidence'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'active root snapshot before activation'
                $row.GaReadyGate | Should -Match 'staged root outside active root'
                $row.GaReadyGate | Should -Match 'binary payload only activation'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'atomic activation or full rollback'
                $row.GaReadyGate | Should -Match 'partial activation forbidden evidence'
                $row.GaReadyGate | Should -Match 'post-activation manifest/version evidence'
                $row.GaReadyGate | Should -Not -Match 'config migration dry-run'
                $row.GaReadyGate | Should -Match 'service start health check'
                $row.GaReadyGate | Should -Match 'rollback attempt on failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'manifest/payload version match'
                $row.GaReadyGate | Should -Not -Match 'staged payload activation'
                $row.GaReadyGate | Should -Not -Match 'product config schema validation pass required'
            }
            if ($row.Name -eq 'rollback restore') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'windows-native-package'
                $row.ImplementationBasis | Should -Be 'package-contract'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'retained previous root'
                $row.GaReadyGate | Should -Match 'previous root manifest/hash verification'
                $row.GaReadyGate | Should -Match 'previous root ownership evidence'
                $row.GaReadyGate | Should -Match 'ADR-0002 channel/version contract binding'
                $row.GaReadyGate | Should -Match 'source/target release_channel evidence'
                $row.GaReadyGate | Should -Match 'update payload manifest version match'
                $row.GaReadyGate | Should -Match 'from-version/to-version compatibility'
                $row.GaReadyGate | Should -Match 'rc/stable RequireSigned trust_model evidence'
                $row.GaReadyGate | Should -Match 'downgrade forbidden except rollback'
                $row.GaReadyGate | Should -Match 'single previous root slot'
                $row.GaReadyGate | Should -Match 'data root preservation'
                $row.GaReadyGate | Should -Match 'failed root diagnostics preservation'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'current active root snapshot before rollback'
                $row.GaReadyGate | Should -Match 'staged rollback root outside active root'
                $row.GaReadyGate | Should -Match 'binary payload only restore'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'atomic rollback or current root preservation'
                $row.GaReadyGate | Should -Match 'failed root preservation'
                $row.GaReadyGate | Should -Match 'partial restore forbidden evidence'
                $row.GaReadyGate | Should -Match 'invalid previous manifest rejection'
                $row.GaReadyGate | Should -Match 'post-rollback manifest/version evidence'
                $row.GaReadyGate | Should -Match 'rollback health check after restore'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'staged rollback activation'
            }

            if ($row.NetworkExposureGate -eq 'lan-exposure-approval-required') {
                $row.Name | Should -Be 'firewall rule enable LAN exposure'
            }
            if ($row.Name -eq 'firewall rule enable LAN exposure') {
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
        $repoMigration | Should -Match 'archive/spikes/hyperv/\*\*'
        $repoMigration | Should -Match 'behavior 변경과 분리'
        $repoMigration | Should -Match '승인 시 목표 상태'
        $repoMigration | Should -Match '파일 이동 실행 승인이 아니다'
        $repoMigration | Should -Match '별도 implementation plan'
        $repoMigration | Should -Match 'rollback 기준'
        $repoMigration | Should -Match 'archive target 검증'
        $repoMigration | Should -Match '첫 slice는 파일 이동을 하지 않는다'
        $repoMigration | Should -Match 'source path inventory'
        $repoMigration | Should -Match 'import/relative path graph'
        $repoMigration | Should -Match 'packaging/static asset input binding'
        $repoMigration | Should -Match 'generated parity manifest update'
        $repoMigration | Should -Match 'docs command update'
        $repoMigration | Should -Match 'no behavior change evidence'
        $repoMigration | Should -Match 'archive target read-only intent'
        $repoMigration | Should -Match 'rollback restore 기준'
        $repoMigration | Should -Match '관련 Pester/npm/`verify:parity`/`node --check` evidence'
        $repoMigration | Should -Match 'migration은 blocked'

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
        $verificationOwnership | Should -Match '첫 slice에서 Pester suite는 계속 required verification'
        $verificationOwnership | Should -Match '대체 xUnit/npm/package/browser fixture evidence'
        $verificationOwnership | Should -Match 'owner replacement'
        $verificationOwnership | Should -Match 'equivalent coverage mapping'
        $verificationOwnership | Should -Match 'archive baseline path'
        $verificationOwnership | Should -Match 'docs command update'
        $verificationOwnership | Should -Match 'CI/local command replacement'
        $verificationOwnership | Should -Match 'rollback 기준'
        $verificationOwnership | Should -Match 'PowerShell helper 또는 `spikes/\*\*`가 active path'
        $verificationOwnership | Should -Match 'archive-only로 낮추지 않는다'
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
    }
```

- [ ] **Step 4: Run the targeted test**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1' -FullName '*GA-ready product runtime candidate*' -Output Detailed"
```

Expected:

```text
Tests Passed: 1, Failed: 0
```

## Task 5: Link Candidate Docs in Indexes

**Files:**

- Modify: `docs/ADR_INDEX.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/GUIDE.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `follower.md`
- Modify: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [ ] **Step 1: Update `docs/ADR_INDEX.md`**

Keep the `## 현재 적용 중인 ADR` table unchanged. Add this new section immediately after that table:

```markdown
## 제안 중인 ADR 후보

| ADR | 상태 | 결정 후보 | 관련 문서 |
|-----|------|-----------|-----------|
| `docs/adr/0004-ga-ready-product-runtime-candidate.md` | 제안 | 승인 시 `keep-spike` 대체 후보, PowerShell-free product ops/runtime GA-ready target | GA-ready redesign spec, route promotion matrix, repo migration map, verification ownership map |
```

In the decision marker block, do not replace `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`. Add this candidate marker below existing Phase 25 markers:

```text
DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime
```

In related entrypoints, add:

```markdown
- `docs/adr/0004-ga-ready-product-runtime-candidate.md` (제안, 현재 적용 결정이 아님)
- `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` (GA-ready 후보 route matrix)
- `docs/ga-ready/REPO_MIGRATION_MAP.md` (GA-ready 후보 repo migration map)
- `docs/ga-ready/VERIFICATION_OWNERSHIP.md` (GA-ready 후보 verification ownership map)
```

- [ ] **Step 2: Update `docs/DEVELOPER_INDEX.md`**

Under "먼저 볼 문서", keep the existing GA-ready redesign spec row and add this row immediately after it:

```markdown
| GA-ready Phase 26 정렬 문서 확인 | `docs/adr/0004-ga-ready-product-runtime-candidate.md`, `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/ga-ready/REPO_MIGRATION_MAP.md`, `docs/ga-ready/VERIFICATION_OWNERSHIP.md` |
```

Under "현재 runtime 전환 상태", add:

```markdown
- Phase 26 정렬 후보는 ADR-0004 제안, route promotion matrix, repo migration map, verification ownership map을 통해 `ga-ready-product-runtime` 목표를 문서화하지만 현재 적용 결정은 계속 `keep-spike`다.
```

- [ ] **Step 3: Update `docs/GUIDE.md`**

Under "주요 진입점", add this row after the GA-ready redesign spec entry:

```markdown
- GA-ready Phase 26 정렬 문서: `docs/adr/0004-ga-ready-product-runtime-candidate.md`, `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`, `docs/ga-ready/REPO_MIGRATION_MAP.md`, `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
```

Under "현재 경계 요약", add:

```markdown
- ADR-0004는 `ga-ready-product-runtime` 제안이며 현재 적용 결정은 아니다. 현재 적용 결정은 ADR index의 `keep-spike`다.
```

- [ ] **Step 4: Update `follower.md`**

Keep "GA-ready 제품 재설계 후보 정렬" as the first item in "다음 우선순위". Add these bullets to that item:

```markdown
   - ADR 후보: `docs/adr/0004-ga-ready-product-runtime-candidate.md`
   - Route matrix: `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md`
   - Repo migration map: `docs/ga-ready/REPO_MIGRATION_MAP.md`
   - Verification ownership map: `docs/ga-ready/VERIFICATION_OWNERSHIP.md`
```

- [ ] **Step 5: Update the phase roadmap**

In `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`, add this row immediately after the Phase 25 row in the `## 전체 Phase 순서` table:

```markdown
| Phase 26 | 제안/정렬 plan 작성 | GA-ready 제품 런타임 후보를 current decision과 분리해 ADR 후보, route promotion matrix, repo migration map, verification ownership map으로 고정한다. | ADR-0004 후보, GA-ready redesign spec, Phase 26 alignment plan, `docs/ga-ready/**` | 실제 route 구현, PowerShell 제거, `spikes/**` 이동 전 ADR 후보와 matrix guard를 먼저 통과해야 한다. |
```

In the `## Phase 24 이후 후보: GA 차단 gate 해소` numbered list, append this item:

```markdown
7. Phase 26 GA-ready alignment는 `keep-spike` 현재 적용 결정을 즉시 대체하지 않고, ADR-0004 후보와 `docs/ga-ready/**` matrix 문서로 PowerShell-free product ops/runtime 승인 시 목표 상태를 검증 가능하게 정렬한다.
```

In the "관련 후속 gate와 Phase 25 Web Console 경계" list, add:

```markdown
- Phase 26 GA-ready alignment 판단은 `docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md`와 `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-ga-ready-phase26-alignment.md`를 따른다.
```

- [ ] **Step 6: Extend the Pester test for index links**

Replace the test body with this final version:

```powershell
    It 'documents the GA-ready product runtime candidate without changing the current decision' {
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

        $adrCandidate | Should -Match '상태: 제안'
        $adrCandidate | Should -Match '대체 대상: 승인 시 ADR-0001 대체'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $adrCandidate | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $adrCandidate | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
        $adrCandidate | Should -Match '승인 시 목표 상태는 PowerShell-free product ops/runtime'
        $adrCandidate | Should -Match '현재 적용 결정이 아니다'
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
        $adrCandidate | Should -Match 'ADR-0004를 current decision으로 승격하지 않는다'
        $adrCandidate | Should -Match '## Aggregate Gate Closure Report'
        $adrCandidate | Should -Match 'aggregate-gate-closure-<YYYY-MM-DD>\.md'
        $adrCandidate | Should -Match 'aggregate_gate_status = closed'
        $adrCandidate | Should -Match '첫 Phase 26 alignment slice에서는 closure report를 만들지 않는다'
        $adrCandidate | Should -Match '## ADR-0001 Replacement Scope'
        $adrCandidate | Should -Match '대체 범위는 ADR-0001의 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 제품 승격 판단'
        $adrCandidate | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $adrCandidate | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $adrCandidate | Should -Match '## Current Decision Promotion Procedure'
        $adrCandidate | Should -Match '이 Phase 26 alignment slice와 별도 PR'
        $adrCandidate | Should -Match 'ADR-0004 상태를 `적용 중`'
        $adrCandidate | Should -Match '제안 중인 ADR 후보 섹션에서 제거'
        $adrCandidate | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 현재 적용 source는 하나만'

        $redesignSpec | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime'
        $redesignSpec | Should -Match 'DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE: powershell-free-product-ops-runtime'
        $redesignSpec | Should -Not -Match 'DESKTOP_NODE_GA_READY_REDESIGN_DECISION'
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
        $routeMatrix | Should -Match '`GET /api/v1/jobs`는 현재 contract-only 후보'
        $routeMatrix | Should -Match 'Job runtime read surface는 현재 `GET /api/v1/jobs/\{job_id\}` row'
        $routeMatrix | Should -Match 'Contract mirror aggregate route 후보인 `POST /api/v1/vms/\{vmId\}/lifecycle/\{action\}`'
        $routeMatrix | Should -Match 'VM lifecycle served surface는 현재 `POST /api/v1/vms/\{id\}/start`, `shutdown`, `poweroff`, `restart` 개별 row'
        $routeMatrix | Should -Not -Match '\| `GET /api/v1/jobs` \|'
        $routeMatrix | Should -Match '## Future Route Execution Guard'
        $routeMatrix | Should -Match 'Phase 26 alignment slice에서 구현하거나 실제 Local API route로 등록하지 않는다'
        $routeMatrix | Should -Match '별도 implementation plan'
        $routeMatrix | Should -Match 'route contract'
        $routeMatrix | Should -Match 'not-found/idempotency contract'
        $routeMatrix | Should -Match 'destructive cleanup proof'
        $routeMatrix | Should -Match 'explicit admin opt-in evidence'
        $routeMatrix | Should -Match '## Native-First Helper Fallback Rule'
        $routeMatrix | Should -Match 'current_owner = dotnet-native'
        $routeMatrix | Should -Match 'topology parity가 불완전할 때 PowerShell helper fallback'
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
        $routeMatrix | Should -Match 'current owner가 갱신되기 전까지 aggregate GA-ready gate closure로 계산할 수 없다'
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
        $routeMatrix | Should -Match '## Aggregate Gate Closure Report Candidate'
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
        $routeMatrix | Should -Match 'ADR 후보와 supporting docs만 만들며 ADR-0004를 current decision으로 승격하지 않는다'
        $routeMatrix | Should -Match 'closure report 없이 진행할 수 없다'
        $routeMatrix | Should -Match '현재 적용 중인 ADR 표'
        $routeMatrix | Should -Match '제안 중인 ADR 후보 섹션'
        $routeMatrix | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION`의 current source는 하나만'
        $routeMatrix | Should -Match 'DESKTOP_NODE_REPOSITORY_DECISION: standalone-windows-repo'
        $routeMatrix | Should -Match 'DESKTOP_NODE_INTERNAL_SIGNING_DECISION: internal-root-leaf-requiresigned'
        $routeMatrix | Should -Match 'missing preserved non-promotion current marker'
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
        $routeMatrix | Should -Match '## Evidence Ledger Candidate'
        $routeMatrix | Should -Match 'docs/ga-ready/evidence/'
        $routeMatrix | Should -Match 'Markdown evidence ledger 후보'
        $routeMatrix | Should -Match '첫 slice에서는 실제 evidence 파일을 만들지 않는다'
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
        $routeMatrix | Should -Match 'ADR-0004를 current decision으로 승격하지 않는다'
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
        $routeMatrix | Should -Match 'multi_user = false'
        $routeMatrix | Should -Match 'rbac = false'
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
        $routeMatrix | Should -Match 'initial install path'
        $routeMatrix | Should -Match 'missing-service precondition'
        $routeMatrix | Should -Match 'service name ownership identity'
        $routeMatrix | Should -Match 'foreign service conflict blocks'
        $routeMatrix | Should -Match 'SCM service identity'
        $routeMatrix | Should -Match 'exact SCM binary path/product root binding'
        $routeMatrix | Should -Match 'no overwrite of existing foreign service'
        $routeMatrix | Should -Match 'conflict diagnostics only'
        $routeMatrix | Should -Match 'service account'
        $routeMatrix | Should -Match 'start type'
        $routeMatrix | Should -Match 'failure policy'
        $routeMatrix | Should -Match 'idempotent already-installed behavior'
        $routeMatrix | Should -Match 'existing owned service precondition'
        $routeMatrix | Should -Match 'owned-field-only config update'
        $routeMatrix | Should -Match 'foreign binary path blocks'
        $routeMatrix | Should -Match 'config drift diagnostics before mutation'
        $routeMatrix | Should -Match 'config drift diff'
        $routeMatrix | Should -Match 'listener args update'
        $routeMatrix | Should -Match 'rollback/recovery on failed config update'
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
        $routeMatrix | Should -Match 'repair path only'
        $routeMatrix | Should -Match 'service absent precondition'
        $routeMatrix | Should -Match 'product root exists'
        $routeMatrix | Should -Match 'owned product root evidence'
        $routeMatrix | Should -Match 'existing config reuse'
        $routeMatrix | Should -Match 'existing config ownership evidence'
        $routeMatrix | Should -Match 'protected token path preservation'
        $routeMatrix | Should -Match 'protected token ownership evidence'
        $routeMatrix | Should -Match 'config schema validation before recreate'
        $routeMatrix | Should -Match 'foreign existing service blocks'
        $routeMatrix | Should -Match 'no product root creation/removal'
        $routeMatrix | Should -Match 'no config rewrite'
        $routeMatrix | Should -Match 'no token rewrite'
        $routeMatrix | Should -Match 'no data root creation'
        $routeMatrix | Should -Match 'no token bootstrap'
        $routeMatrix | Should -Match 'service uninstall stop/delete'
        $routeMatrix | Should -Match 'product root removal preserve-data'
        $routeMatrix | Should -Match 'service uninstall remove-data request'
        $routeMatrix | Should -Not -Match '\| service uninstall preserve-data \|'
        $routeMatrix | Should -Not -Match '\| service uninstall remove-data \|'
        $routeMatrix | Should -Match 'data root remove'
        $routeMatrix | Should -Match 'SCM service recreate'
        $routeMatrix | Should -Match 'owned-field-only repair'
        $routeMatrix | Should -Match 'allowed repair drift fields = protected token path, listener args'
        $routeMatrix | Should -Match 'non-repair drift handoff to service configure update'
        $routeMatrix | Should -Match 'no SCM recreate'
        $routeMatrix | Should -Match 'protected token path/listener args update'
        $routeMatrix | Should -Match 'rollback/recovery'
        $routeMatrix | Should -Not -Match 'conditional 3010'
        $routeMatrix | Should -Match 'owned service identity'
        $routeMatrix | Should -Match 'stop-before-delete sequencing'
        $routeMatrix | Should -Match 'delete service only'
        $routeMatrix | Should -Match 'stop idempotency'
        $routeMatrix | Should -Match 'delete idempotency'
        $routeMatrix | Should -Match 'service deletion confirmation'
        $routeMatrix | Should -Match 'missing-service idempotency'
        $routeMatrix | Should -Match 'missing-service idempotent diagnostics'
        $routeMatrix | Should -Match 'no product root delete'
        $routeMatrix | Should -Match 'no data root delete'
        $routeMatrix | Should -Match 'no config delete'
        $routeMatrix | Should -Match 'no token delete'
        $routeMatrix | Should -Match 'no REMOVE_DATA handoff'
        $routeMatrix | Should -Match 'service absent/deleted precondition'
        $routeMatrix | Should -Match 'owned product root evidence'
        $routeMatrix | Should -Match 'exact product root allowlist'
        $routeMatrix | Should -Match 'binary payload only delete'
        $routeMatrix | Should -Match 'config/data/token preserve allowlist'
        $routeMatrix | Should -Match 'ProgramData preserve evidence'
        $routeMatrix | Should -Match 'data root delete forbidden evidence'
        $routeMatrix | Should -Match 'protected token preserved evidence'
        $routeMatrix | Should -Match 'data-root-remove handoff evidence'
        $routeMatrix | Should -Match 'REMOVE_DATA=1 request validation'
        $routeMatrix | Should -Match 'explicit remove-data intent source'
        $routeMatrix | Should -Match 'service deleted/absent precondition'
        $routeMatrix | Should -Match 'service deletion confirmation required'
        $routeMatrix | Should -Match 'handoff descriptor only'
        $routeMatrix | Should -Match 'no direct data root mutation'
        $routeMatrix | Should -Match 'no direct ProgramData delete'
        $routeMatrix | Should -Match 'no direct protected token delete'
        $routeMatrix | Should -Match 'no ProgramData delete'
        $routeMatrix | Should -Match 'no protected token delete'
        $routeMatrix | Should -Match 'locked-file abort before partial delete'
        $routeMatrix | Should -Match 'partial product root delete forbidden evidence'
        $routeMatrix | Should -Match 'cleanup diagnostics evidence'
        $routeMatrix | Should -Match 'REMOVE_DATA=1'
        $routeMatrix | Should -Match 'remove-data handoff descriptor required'
        $routeMatrix | Should -Match 'exact data root path allowlist'
        $routeMatrix | Should -Match 'owned data root marker/evidence'
        $routeMatrix | Should -Match 'service deleted/absent precondition'
        $routeMatrix | Should -Match 'installed service blocks delete diagnostics'
        $routeMatrix | Should -Match 'protected token delete only within owned data root'
        $routeMatrix | Should -Match 'no product root mutation'
        $routeMatrix | Should -Match 'no service mutation'
        $routeMatrix | Should -Match 'delete manifest/journal evidence'
        $routeMatrix | Should -Match 'post-delete absence evidence'
        $routeMatrix | Should -Match 'locked-file abort diagnostics'
        $routeMatrix | Should -Match 'no partial delete success evidence'
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
        $routeMatrix | Should -Match 'source 등록과 제거를 별도 explicit admin opt-in smoke'
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
        $routeMatrix | Should -Match 'signed/approved package manifest required'
        $routeMatrix | Should -Match 'manifest hash verification'
        $routeMatrix | Should -Match 'ADR-0002 channel/version contract binding'
        $routeMatrix | Should -Match 'source/target release_channel evidence'
        $routeMatrix | Should -Match 'update payload manifest version match'
        $routeMatrix | Should -Match 'from-version/to-version compatibility'
        $routeMatrix | Should -Match 'rc/stable RequireSigned trust_model evidence'
        $routeMatrix | Should -Match 'downgrade forbidden except rollback'
        $routeMatrix | Should -Match 'single previous root slot'
        $routeMatrix | Should -Match 'data root preservation'
        $routeMatrix | Should -Match 'failed root diagnostics preservation'
        $routeMatrix | Should -Match 'exact product root ownership evidence'
        $routeMatrix | Should -Match 'service stopped precondition'
        $routeMatrix | Should -Match 'active root snapshot before activation'
        $routeMatrix | Should -Match 'staged root outside active root'
        $routeMatrix | Should -Match 'binary payload only activation'
        $routeMatrix | Should -Match 'no config mutation'
        $routeMatrix | Should -Match 'no data root mutation'
        $routeMatrix | Should -Match 'no token mutation'
        $routeMatrix | Should -Match 'no service identity mutation'
        $routeMatrix | Should -Match 'atomic activation or full rollback'
        $routeMatrix | Should -Match 'partial activation forbidden evidence'
        $routeMatrix | Should -Match 'post-activation manifest/version evidence'
        $routeMatrix | Should -Match 'service start health check'
        $routeMatrix | Should -Match 'rollback attempt on failure'
        $routeMatrix | Should -Match 'rollback result diagnostics'
        $routeMatrix | Should -Match 'retained previous root'
        $routeMatrix | Should -Match 'previous root manifest/hash verification'
        $routeMatrix | Should -Match 'previous root ownership evidence'
        $routeMatrix | Should -Match 'current active root snapshot before rollback'
        $routeMatrix | Should -Match 'staged rollback root outside active root'
        $routeMatrix | Should -Match 'binary payload only restore'
        $routeMatrix | Should -Match 'atomic rollback or current root preservation'
        $routeMatrix | Should -Match 'failed root preservation'
        $routeMatrix | Should -Match 'partial restore forbidden evidence'
        $routeMatrix | Should -Match 'invalid previous manifest rejection'
        $routeMatrix | Should -Match 'post-rollback manifest/version evidence'
        $routeMatrix | Should -Match 'rollback health check after restore'
        $routeMatrix | Should -Not -Match '\| update/rollback \|'
        $routeMatrix | Should -Match 'product config schema validation'
        $routeMatrix | Should -Match 'product config migration apply'
        $routeMatrix | Should -Not -Match '\| product config migration \|'
        $routeMatrix | Should -Match 'job store schema mismatch detection'
        $routeMatrix | Should -Match 'job store migration apply'
        $routeMatrix | Should -Match 'read-only config inventory'
        $routeMatrix | Should -Match 'owned config path evidence'
        $routeMatrix | Should -Match 'schema version parse evidence'
        $routeMatrix | Should -Match 'config schema compatibility'
        $routeMatrix | Should -Match 'dry-run validation before service start'
        $routeMatrix | Should -Match 'service-start preflight decision descriptor only'
        $routeMatrix | Should -Match 'validation failure diagnostics'
        $routeMatrix | Should -Match 'diagnostics redaction evidence'
        $routeMatrix | Should -Match 'no config write'
        $routeMatrix | Should -Match 'no backup write'
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
        $routeMatrix | Should -Match 'future route implementation plan'
        $routeMatrix | Should -Not -Match '/checkpoints/\{name\}'
        $routeMatrix | Should -Match 'name`/`checkpoint_name'
        $routeMatrix | Should -Match '원본 job operation'
        $routeMatrix | Should -Match 'GA-ready gate, release gate, network exposure gate'
        $routeMatrix | Should -Match 'not-yet-defined'
        $routeMatrix | Should -Match 'current_owner = not-yet-defined'
        $routeMatrix | Should -Match 'GA-ready blocker'
        $routeMatrix | Should -Match 'GET /api/v1/vms'

        $schemaEnums = @{}
        foreach ($line in ($routeMatrix -split "`r?`n")) {
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
        $matrixRows.Name | Should -Not -Contain '`GET /api/v1/jobs`'
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
                $row.Name | Should -Be '`DELETE /api/v1/vms/{id}`'
                $row.CurrentOwner | Should -Be 'not-implemented'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.GaReadyGate | Should -Match 'future route implementation plan'
                $row.GaReadyGate | Should -Match 'not-found/idempotency contract'
            }
            if ($row.CurrentOwner -eq 'not-implemented') {
                $row.RouteSurface | Should -Be 'future-route'
            }
            if ($row.Name -eq '`DELETE /api/v1/vms/{id}`') {
                $row.RouteSurface | Should -Be 'future-route'
                $row.CurrentOwner | Should -Be 'not-implemented'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.GaReadyGate | Should -Match 'future route implementation plan'
                $row.GaReadyGate | Should -Match 'not-found/idempotency contract'
            }

            if ($row.Name -eq '`GET /api/v1/network/inventory`') {
                $row.CurrentOwner | Should -Be 'dotnet-native'
                $row.FallbackPolicy | Should -Be 'transition-helper'
                $row.PromotionState | Should -Be 'transition-helper'
                $row.GaReadyGate | Should -Match 'fallback 제거'
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
            if ($row.CurrentOwner -eq 'mixed-history') {
                $serviceProductOpsRows | Should -Contain $row.Name
                $row.RouteSurface | Should -Be 'product-operation'
                $row.Domain | Should -Be 'product-ops'
                $row.TargetOwner | Should -Be 'dotnet-service-action'
                $row.PromotionState | Should -Be 'blocked'
            }
            if ($serviceProductOpsRows -contains $row.Name) {
                $row.CurrentOwner | Should -Be 'mixed-history'
                $row.TargetOwner | Should -Not -Be 'mixed-history'
                $row.PromotionState | Should -Be 'blocked'
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
                $row.GaReadyGate | Should -Match 'initial install path'
                $row.GaReadyGate | Should -Match 'missing-service precondition'
                $row.GaReadyGate | Should -Match 'service name ownership identity'
                $row.GaReadyGate | Should -Match 'foreign service conflict blocks'
                $row.GaReadyGate | Should -Match 'SCM service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'no overwrite of existing foreign service'
                $row.GaReadyGate | Should -Match 'conflict diagnostics only'
                $row.GaReadyGate | Should -Match 'binary path'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args'
                $row.GaReadyGate | Should -Match 'service account'
                $row.GaReadyGate | Should -Match 'start type'
                $row.GaReadyGate | Should -Match 'failure policy'
                $row.GaReadyGate | Should -Match 'idempotent already-installed behavior'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'protected token bootstrap'
                $row.GaReadyGate | Should -Not -Match 'existing config reuse'
                $row.GaReadyGate | Should -Not -Match 'repair path only'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
            }
            if ($row.Name -eq 'service configure update') {
                $row.GaReadyGate | Should -Match 'existing owned service precondition'
                $row.GaReadyGate | Should -Match 'owned-field-only config update'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'foreign binary path blocks'
                $row.GaReadyGate | Should -Match 'config drift diagnostics before mutation'
                $row.GaReadyGate | Should -Match 'config drift diff'
                $row.GaReadyGate | Should -Match 'protected token path'
                $row.GaReadyGate | Should -Match 'listener args update'
                $row.GaReadyGate | Should -Match 'data preservation'
                $row.GaReadyGate | Should -Match 'rollback/recovery on failed config update'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'missing-service precondition'
                $row.GaReadyGate | Should -Not -Match 'no overwrite of existing foreign service'
            }
            if ($row.Name -eq 'service repair missing service recreation') {
                $row.GaReadyGate | Should -Match 'repair path only'
                $row.GaReadyGate | Should -Match 'service absent precondition'
                $row.GaReadyGate | Should -Match 'product root exists'
                $row.GaReadyGate | Should -Match 'owned product root evidence'
                $row.GaReadyGate | Should -Match 'existing config reuse'
                $row.GaReadyGate | Should -Match 'existing config ownership evidence'
                $row.GaReadyGate | Should -Match 'protected token path preservation'
                $row.GaReadyGate | Should -Match 'protected token ownership evidence'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'config schema validation before recreate'
                $row.GaReadyGate | Should -Match 'foreign existing service blocks'
                $row.GaReadyGate | Should -Match 'SCM service recreate'
                $row.GaReadyGate | Should -Match 'SCM binary path'
                $row.GaReadyGate | Should -Match 'service identity'
                $row.GaReadyGate | Should -Match 'no product root creation/removal'
                $row.GaReadyGate | Should -Match 'no config rewrite'
                $row.GaReadyGate | Should -Match 'no token rewrite'
                $row.GaReadyGate | Should -Match 'no data root creation'
                $row.GaReadyGate | Should -Match 'no token bootstrap'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'owned-field-only config update'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
                $row.GaReadyGate | Should -Not -Match 'initial install path'
            }
            if ($row.Name -eq 'service repair config drift correction') {
                $row.GaReadyGate | Should -Match 'repair path only'
                $row.GaReadyGate | Should -Match 'existing owned service'
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'owned-field-only repair'
                $row.GaReadyGate | Should -Match 'allowed repair drift fields = protected token path, listener args'
                $row.GaReadyGate | Should -Match 'config drift diagnostics before mutation'
                $row.GaReadyGate | Should -Match 'config drift diff'
                $row.GaReadyGate | Should -Match 'protected token path/listener args update'
                $row.GaReadyGate | Should -Match 'foreign binary path blocks'
                $row.GaReadyGate | Should -Match 'non-repair drift handoff to service configure update'
                $row.GaReadyGate | Should -Match 'data preservation'
                $row.GaReadyGate | Should -Match 'rollback/recovery'
                $row.GaReadyGate | Should -Match 'no SCM recreate'
                $row.GaReadyGate | Should -Match 'no config rewrite'
                $row.GaReadyGate | Should -Match 'no token rewrite'
                $row.GaReadyGate | Should -Match 'no product root creation/removal'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'SCM service recreate'
                $row.GaReadyGate | Should -Not -Match 'service absent precondition'
                $row.GaReadyGate | Should -Not -Match 'idempotent config repair'
                $row.GaReadyGate | Should -Not -Match 'conditional 3010'
            }
            if ($row.Name -eq 'service uninstall stop/delete') {
                $row.GaReadyGate | Should -Match 'owned service identity'
                $row.GaReadyGate | Should -Match 'exact SCM binary path/product root binding'
                $row.GaReadyGate | Should -Match 'foreign service blocks'
                $row.GaReadyGate | Should -Match 'stop-before-delete sequencing'
                $row.GaReadyGate | Should -Match 'stop idempotency'
                $row.GaReadyGate | Should -Match 'delete service only'
                $row.GaReadyGate | Should -Match 'delete idempotency'
                $row.GaReadyGate | Should -Match 'service deletion confirmation'
                $row.GaReadyGate | Should -Match 'missing-service idempotency'
                $row.GaReadyGate | Should -Match 'missing-service idempotent diagnostics'
                $row.GaReadyGate | Should -Match 'no product root delete'
                $row.GaReadyGate | Should -Match 'no data root delete'
                $row.GaReadyGate | Should -Match 'no config delete'
                $row.GaReadyGate | Should -Match 'no token delete'
                $row.GaReadyGate | Should -Match 'no REMOVE_DATA handoff'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'product root allowlist'
                $row.GaReadyGate | Should -Not -Match 'ProgramData preserve evidence'
                $row.GaReadyGate | Should -Not -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Not -Match 'REMOVE_DATA=1 request validation'
            }
            if ($row.Name -eq 'product root removal preserve-data') {
                $row.GaReadyGate | Should -Match 'service absent/deleted precondition'
                $row.GaReadyGate | Should -Match 'owned product root evidence'
                $row.GaReadyGate | Should -Match 'exact product root allowlist'
                $row.GaReadyGate | Should -Match 'binary payload only delete'
                $row.GaReadyGate | Should -Match 'config/data/token preserve allowlist'
                $row.GaReadyGate | Should -Match 'ProgramData preserve evidence'
                $row.GaReadyGate | Should -Match 'data root delete forbidden evidence'
                $row.GaReadyGate | Should -Match 'protected token preserved evidence'
                $row.GaReadyGate | Should -Match 'no ProgramData delete'
                $row.GaReadyGate | Should -Match 'no protected token delete'
                $row.GaReadyGate | Should -Match 'locked-file abort before partial delete'
                $row.GaReadyGate | Should -Match 'locked-file abort diagnostics'
                $row.GaReadyGate | Should -Match 'partial product root delete forbidden evidence'
                $row.GaReadyGate | Should -Match 'cleanup diagnostics evidence'
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
                $row.GaReadyGate | Should -Match 'REMOVE_DATA=1 request validation'
                $row.GaReadyGate | Should -Match 'explicit remove-data intent source'
                $row.GaReadyGate | Should -Match 'service deleted/absent precondition'
                $row.GaReadyGate | Should -Match 'service deletion confirmation required'
                $row.GaReadyGate | Should -Match 'handoff descriptor only'
                $row.GaReadyGate | Should -Match 'data-root-remove handoff evidence'
                $row.GaReadyGate | Should -Match 'no direct data root mutation'
                $row.GaReadyGate | Should -Match 'no direct ProgramData delete'
                $row.GaReadyGate | Should -Match 'no direct protected token delete'
                $row.GaReadyGate | Should -Match 'missing-service idempotent diagnostics'
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
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-runtime'
                $row.ImplementationBasis | Should -Be 'dotnet-runtime'
                $row.RiskTier | Should -Be 'tier1-read-only'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'read-only config inventory'
                $row.GaReadyGate | Should -Match 'owned config path evidence'
                $row.GaReadyGate | Should -Match 'schema version parse evidence'
                $row.GaReadyGate | Should -Match 'config schema compatibility'
                $row.GaReadyGate | Should -Match 'dry-run validation before service start'
                $row.GaReadyGate | Should -Match 'service-start preflight decision descriptor only'
                $row.GaReadyGate | Should -Match 'validation failure diagnostics'
                $row.GaReadyGate | Should -Match 'diagnostics redaction evidence'
                $row.GaReadyGate | Should -Match 'no config write'
                $row.GaReadyGate | Should -Match 'no backup write'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'no migration execution'
                $row.GaReadyGate | Should -Not -Match 'service-start block on validation failure'
                $row.GaReadyGate | Should -Not -Match 'validation writes forbidden evidence'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.CurrentOwner | Should -Be 'not-yet-defined'
            }

            if ($row.TargetOwner -eq 'dotnet-config-migration-action') {
                $row.Name | Should -Be 'product config migration apply'
            }
            if ($row.Name -eq 'product config migration apply') {
                $row.TargetOwner | Should -Be 'dotnet-config-migration-action'
                $row.ImplementationBasis | Should -Be 'product-config-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'current config source inventory'
                $row.GaReadyGate | Should -Match 'current schema owner resolution'
                $row.GaReadyGate | Should -Match 'owned source config path evidence'
                $row.GaReadyGate | Should -Match 'source path/version evidence'
                $row.GaReadyGate | Should -Match 'source/target schema version evidence'
                $row.GaReadyGate | Should -Match 'migration plan id/version'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'validation preflight descriptor required'
                $row.GaReadyGate | Should -Match 'backup path inside owned config backup root'
                $row.GaReadyGate | Should -Match 'atomic config replace'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no job store mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'partial config migration forbidden evidence'
                $row.GaReadyGate | Should -Match 'rollback on migration failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'cleanup evidence'
                $row.GaReadyGate | Should -Match 'service-start preflight decision descriptor only'
                $row.GaReadyGate | Should -Match 'validation writes forbidden'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in before config write'
                $row.GaReadyGate | Should -Not -Match 'backup/restore'
                $row.GaReadyGate | Should -Not -Match 'service-start health check'
                $row.GaReadyGate | Should -Not -Match 'explicit admin opt-in before config/data mutation'
            }
            if ($row.TargetOwner -eq 'dotnet-token-storage-action') {
                $row.Name | Should -Be 'protected token bootstrap'
            }
            if ($row.Name -eq 'protected token bootstrap') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-token-storage-action'
                $row.ImplementationBasis | Should -Be 'dpapi-local-machine-token-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'raw token 비노출'
                $row.GaReadyGate | Should -Match 'token source inventory'
                $row.GaReadyGate | Should -Match 'single-source precondition'
                $row.GaReadyGate | Should -Match 'existing protected token no-overwrite'
                $row.GaReadyGate | Should -Match 'legacy token migration'
                $row.GaReadyGate | Should -Match 'legacy raw migration only when protected token missing'
                $row.GaReadyGate | Should -Match 'source conflict diagnostics'
                $row.GaReadyGate | Should -Match 'owned legacy token source required'
                $row.GaReadyGate | Should -Match 'protected token schema'
                $row.GaReadyGate | Should -Match 'ACL hardening'
                $row.GaReadyGate | Should -Match 'service command line protected file path only'
                $row.GaReadyGate | Should -Match 'command line token value forbidden'
                $row.GaReadyGate | Should -Match 'diagnostics redaction evidence'
            }
            if ($row.TargetOwner -eq 'dotnet-data-root-action') {
                $row.Name | Should -Be 'data root remove'
            }
            if ($row.Name -eq 'data root remove') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'dotnet-data-root-action'
                $row.ImplementationBasis | Should -Be 'data-root-lifecycle-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'REMOVE_DATA=1'
                $row.GaReadyGate | Should -Match 'remove-data handoff descriptor required'
                $row.GaReadyGate | Should -Match 'exact data root path allowlist'
                $row.GaReadyGate | Should -Match 'owned data root marker/evidence'
                $row.GaReadyGate | Should -Match 'service deleted/absent precondition'
                $row.GaReadyGate | Should -Match 'installed service blocks delete diagnostics'
                $row.GaReadyGate | Should -Match 'protected token delete only within owned data root'
                $row.GaReadyGate | Should -Match 'no product root mutation'
                $row.GaReadyGate | Should -Match 'no service mutation'
                $row.GaReadyGate | Should -Match 'locked-file abort before partial delete'
                $row.GaReadyGate | Should -Match 'locked-file abort diagnostics'
                $row.GaReadyGate | Should -Match 'delete manifest/journal evidence'
                $row.GaReadyGate | Should -Match 'post-delete absence evidence'
                $row.GaReadyGate | Should -Match 'no partial delete success evidence'
                $row.GaReadyGate | Should -Match 'diagnostics evidence'
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
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'none'
                $row.GaReadyGate | Should -Match 'read-only-or-blocked-with-diagnostics'
                $row.GaReadyGate | Should -Match 'schema mismatch behavior'
                $row.GaReadyGate | Should -Match 'schema mismatch returns blocked diagnostics'
                $row.GaReadyGate | Should -Match 'runtime read must not mutate jobs.json'
                $row.GaReadyGate | Should -Match 'no quarantine move/write'
                $row.GaReadyGate | Should -Match 'migration handoff descriptor only'
                $row.GaReadyGate | Should -Match 'no migration execution'
                $row.GaReadyGate | Should -Match 'diagnostics evidence'
                $row.GaReadyGate | Should -Not -Match 'current quarantine move/write behavior'
                $row.GaReadyGate | Should -Not -Match 'moved under explicit'
                $row.GaReadyGate | Should -Not -Match 'atomic job store replace'
                $row.GaReadyGate | Should -Not -Match 'destructive rewrite disabled by default'
            }
            if ($row.Name -eq 'job store migration apply') {
                $row.CurrentOwner | Should -Be 'dotnet-runtime'
                $row.TargetOwner | Should -Be 'dotnet-job-store-migration-action'
                $row.ImplementationBasis | Should -Be 'job-store-migration-plan'
                $row.RiskTier | Should -Be 'tier3-destructive-or-persistent'
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.GaReadyGate | Should -Match 'current job store path inventory'
                $row.GaReadyGate | Should -Match 'current job schema owner evidence'
                $row.GaReadyGate | Should -Match 'owned job store path evidence'
                $row.GaReadyGate | Should -Match 'source job store version evidence'
                $row.GaReadyGate | Should -Match 'source/target schema version evidence'
                $row.GaReadyGate | Should -Match 'migration plan id/version'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Match 'runtime writer stopped evidence'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'backup path inside owned job-store backup root'
                $row.GaReadyGate | Should -Match 'destructive rewrite disabled by default'
                $row.GaReadyGate | Should -Match 'atomic job store replace'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'partial job store migration forbidden evidence'
                $row.GaReadyGate | Should -Match 'rollback on migration failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'recovery evidence'
                $row.GaReadyGate | Should -Match 'explicit admin opt-in before job store write'
                $row.GaReadyGate | Should -Not -Match 'backup/restore'
                $row.GaReadyGate | Should -Not -Match 'explicit admin opt-in before data mutation'
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
                $row.FallbackPolicy | Should -Be 'blocked'
                $row.PromotionState | Should -Be 'blocked'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
            }
            if ($row.Name -eq 'Event Log source registration') {
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
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'signed/approved package manifest required'
                $row.GaReadyGate | Should -Match 'manifest hash verification'
                $row.GaReadyGate | Should -Match 'ADR-0002 channel/version contract binding'
                $row.GaReadyGate | Should -Match 'source/target release_channel evidence'
                $row.GaReadyGate | Should -Match 'update payload manifest version match'
                $row.GaReadyGate | Should -Match 'from-version/to-version compatibility'
                $row.GaReadyGate | Should -Match 'rc/stable RequireSigned trust_model evidence'
                $row.GaReadyGate | Should -Match 'downgrade forbidden except rollback'
                $row.GaReadyGate | Should -Match 'single previous root slot'
                $row.GaReadyGate | Should -Match 'data root preservation'
                $row.GaReadyGate | Should -Match 'failed root diagnostics preservation'
                $row.GaReadyGate | Should -Match 'exact product root ownership evidence'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'active root snapshot before activation'
                $row.GaReadyGate | Should -Match 'staged root outside active root'
                $row.GaReadyGate | Should -Match 'binary payload only activation'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'atomic activation or full rollback'
                $row.GaReadyGate | Should -Match 'partial activation forbidden evidence'
                $row.GaReadyGate | Should -Match 'post-activation manifest/version evidence'
                $row.GaReadyGate | Should -Not -Match 'config migration dry-run'
                $row.GaReadyGate | Should -Match 'service start health check'
                $row.GaReadyGate | Should -Match 'rollback attempt on failure'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'manifest/payload version match'
                $row.GaReadyGate | Should -Not -Match 'staged payload activation'
                $row.GaReadyGate | Should -Not -Match 'product config schema validation pass required'
            }
            if ($row.Name -eq 'rollback restore') {
                $row.CurrentOwner | Should -Be 'product-wrapper'
                $row.TargetOwner | Should -Be 'windows-native-package'
                $row.ImplementationBasis | Should -Be 'package-contract'
                $row.AdminSmokeRequired | Should -Be 'explicit-admin-opt-in'
                $row.ReleaseGate | Should -Be 'release-approval-required'
                $row.GaReadyGate | Should -Match 'retained previous root'
                $row.GaReadyGate | Should -Match 'previous root manifest/hash verification'
                $row.GaReadyGate | Should -Match 'previous root ownership evidence'
                $row.GaReadyGate | Should -Match 'ADR-0002 channel/version contract binding'
                $row.GaReadyGate | Should -Match 'source/target release_channel evidence'
                $row.GaReadyGate | Should -Match 'update payload manifest version match'
                $row.GaReadyGate | Should -Match 'from-version/to-version compatibility'
                $row.GaReadyGate | Should -Match 'rc/stable RequireSigned trust_model evidence'
                $row.GaReadyGate | Should -Match 'downgrade forbidden except rollback'
                $row.GaReadyGate | Should -Match 'single previous root slot'
                $row.GaReadyGate | Should -Match 'data root preservation'
                $row.GaReadyGate | Should -Match 'failed root diagnostics preservation'
                $row.GaReadyGate | Should -Match 'service stopped precondition'
                $row.GaReadyGate | Should -Not -Match 'drained'
                $row.GaReadyGate | Should -Match 'current active root snapshot before rollback'
                $row.GaReadyGate | Should -Match 'staged rollback root outside active root'
                $row.GaReadyGate | Should -Match 'binary payload only restore'
                $row.GaReadyGate | Should -Match 'no config mutation'
                $row.GaReadyGate | Should -Match 'no data root mutation'
                $row.GaReadyGate | Should -Match 'no token mutation'
                $row.GaReadyGate | Should -Match 'no service identity mutation'
                $row.GaReadyGate | Should -Match 'atomic rollback or current root preservation'
                $row.GaReadyGate | Should -Match 'failed root preservation'
                $row.GaReadyGate | Should -Match 'partial restore forbidden evidence'
                $row.GaReadyGate | Should -Match 'invalid previous manifest rejection'
                $row.GaReadyGate | Should -Match 'post-rollback manifest/version evidence'
                $row.GaReadyGate | Should -Match 'rollback health check after restore'
                $row.GaReadyGate | Should -Match 'rollback result diagnostics'
                $row.GaReadyGate | Should -Match 'no-auto-reboot evidence'
                $row.GaReadyGate | Should -Not -Match 'staged rollback activation'
            }

            if ($row.NetworkExposureGate -eq 'lan-exposure-approval-required') {
                $row.Name | Should -Be 'firewall rule enable LAN exposure'
            }
            if ($row.Name -eq 'firewall rule enable LAN exposure') {
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
        $repoMigration | Should -Match 'archive/spikes/hyperv/\*\*'
        $repoMigration | Should -Match 'behavior 변경과 분리'
        $repoMigration | Should -Match '승인 시 목표 상태'
        $repoMigration | Should -Match '파일 이동 실행 승인이 아니다'
        $repoMigration | Should -Match '별도 implementation plan'
        $repoMigration | Should -Match 'rollback 기준'
        $repoMigration | Should -Match 'archive target 검증'
        $repoMigration | Should -Match '첫 slice는 파일 이동을 하지 않는다'
        $repoMigration | Should -Match 'source path inventory'
        $repoMigration | Should -Match 'import/relative path graph'
        $repoMigration | Should -Match 'packaging/static asset input binding'
        $repoMigration | Should -Match 'generated parity manifest update'
        $repoMigration | Should -Match 'docs command update'
        $repoMigration | Should -Match 'no behavior change evidence'
        $repoMigration | Should -Match 'archive target read-only intent'
        $repoMigration | Should -Match 'rollback restore 기준'
        $repoMigration | Should -Match '관련 Pester/npm/`verify:parity`/`node --check` evidence'
        $repoMigration | Should -Match 'migration은 blocked'

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
        $verificationOwnership | Should -Match '첫 slice에서 Pester suite는 계속 required verification'
        $verificationOwnership | Should -Match '대체 xUnit/npm/package/browser fixture evidence'
        $verificationOwnership | Should -Match 'owner replacement'
        $verificationOwnership | Should -Match 'equivalent coverage mapping'
        $verificationOwnership | Should -Match 'archive baseline path'
        $verificationOwnership | Should -Match 'docs command update'
        $verificationOwnership | Should -Match 'CI/local command replacement'
        $verificationOwnership | Should -Match 'rollback 기준'
        $verificationOwnership | Should -Match 'PowerShell helper 또는 `spikes/\*\*`가 active path'
        $verificationOwnership | Should -Match 'archive-only로 낮추지 않는다'
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

        $adrIndex | Should -Match 'PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike'
        $adrIndex | Should -Match '## 현재 적용 중인 ADR'
        $adrIndex | Should -Match '## 제안 중인 ADR 후보'
        $adrIndex | Should -Match '0004-ga-ready-product-runtime-candidate'
        $adrIndex | Should -Match '현재 적용 결정이 아님'
        $developerIndex | Should -Match 'GA-ready Phase 26 정렬 문서 확인'
        $guide | Should -Match 'GA-ready Phase 26 정렬 문서'
        $roadmap | Should -Match 'Phase 26'
        $roadmap | Should -Match '제안/정렬 plan 작성'
        $roadmap | Should -Match 'route promotion matrix'
        $follower | Should -Match 'GA-ready 제품 재설계 후보 정렬'
        $follower | Should -Match 'docs/ga-ready/ROUTE_PROMOTION_MATRIX.md'
    }
```

- [ ] **Step 7: Run the targeted test**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1' -FullName '*GA-ready product runtime candidate*' -Output Detailed"
```

Expected:

```text
Tests Passed: 1, Failed: 0
```

## Task 6: Full Verification and Commit

**Files:**

- All files from Tasks 1-5

- [ ] **Step 1: Scan for incomplete markers**

Run:

```powershell
rg -n "TB[D]|TO[DO]|FIX[ME]|place[holder]|미[정]|나[중]" docs/adr/0004-ga-ready-product-runtime-candidate.md docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md docs/ga-ready/ROUTE_PROMOTION_MATRIX.md docs/ga-ready/REPO_MIGRATION_MAP.md docs/ga-ready/VERIFICATION_OWNERSHIP.md docs/ADR_INDEX.md docs/DEVELOPER_INDEX.md docs/GUIDE.md follower.md spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1
```

Expected: no output and exit code `1` from `rg`.

- [ ] **Step 2: Run the root documentation suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected:

```text
Tests Passed:
Failed: 0
```

- [ ] **Step 3: Run whitespace/static diff check**

Run:

```powershell
git diff --check
```

Expected: no output and exit code `0`.

- [ ] **Step 4: Inspect final diff**

Run:

```powershell
git diff --stat
git status --short
```

Expected changed files:

```text
M docs/ADR_INDEX.md
M docs/DEVELOPER_INDEX.md
M docs/GUIDE.md
M follower.md
M docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md
M docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md
M spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1
?? docs/adr/0004-ga-ready-product-runtime-candidate.md
?? docs/ga-ready/ROUTE_PROMOTION_MATRIX.md
?? docs/ga-ready/REPO_MIGRATION_MAP.md
?? docs/ga-ready/VERIFICATION_OWNERSHIP.md
```

- [ ] **Step 5: Commit**

Run:

```powershell
git add docs/adr/0004-ga-ready-product-runtime-candidate.md docs/superpowers/specs/2026-05-02-purecvisor-desktop-node-ga-ready-redesign-design.md docs/ga-ready/ROUTE_PROMOTION_MATRIX.md docs/ga-ready/REPO_MIGRATION_MAP.md docs/ga-ready/VERIFICATION_OWNERSHIP.md docs/ADR_INDEX.md docs/DEVELOPER_INDEX.md docs/GUIDE.md docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md follower.md spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1
git commit -m "Document GA-ready Phase 26 alignment"
```

Expected:

```text
[branch <sha>] Document GA-ready Phase 26 alignment
```

## Self-Review Checklist

- [ ] Task 0 stops on ADR number collision.
- [ ] Task 0 stops on existing GA-ready target docs or `docs/ga-ready/evidence` before writing.
- [ ] Task 0 allows an existing empty `docs/ga-ready` directory when target docs and evidence directory are absent.
- [ ] Engineering review execution dependency diagram matches the task order and keeps ADR/index/test updates sequenced.
- [ ] Engineering review failure modes include RED failure reasons, table parser empty/duplicate row failure, current decision leakage, and first-slice evidence file scope violation.
- [ ] Worktree strategy keeps actual writes single-worker and limits parallelization to read-only validation.
- [ ] ADR-0004 is explicitly `상태: 제안`.
- [ ] ADR-0004 follows the local ADR template shape.
- [ ] ADR-0004 keeps `PRODUCT_RUNTIME_PROMOTION_DECISION: ga-ready-product-runtime` but uses `DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE`, not `DESKTOP_NODE_GA_READY_REDESIGN_DECISION`.
- [ ] Existing GA-ready redesign spec also uses `DESKTOP_NODE_GA_READY_REDESIGN_CANDIDATE`, not `DESKTOP_NODE_GA_READY_REDESIGN_DECISION`.
- [ ] Existing GA-ready redesign spec links to `docs/ga-ready/ROUTE_PROMOTION_MATRIX.md` as the single detailed route contract and does not keep an inline route table.
- [ ] `docs/ADR_INDEX.md` keeps ADR-0004 out of `현재 적용 중인 ADR`.
- [ ] `docs/ADR_INDEX.md` keeps `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` present.
- [ ] Phase roadmap includes Phase 26 as `제안/정렬 plan 작성`.
- [ ] Top-level docs describe PowerShell-free product ops/runtime as an approval target, not the current state.
- [ ] No document claims stable publication, public trusted signing, or GA release has happened.
- [ ] ADR-0004 separates GA-ready product runtime from release execution.
- [ ] ADR-0004 aggregate GA-ready decision gate requires zero `transition-helper`/`blocked` GA-scope `current-route`/`product-operation` rows before current decision promotion.
- [ ] Aggregate gate keeps `future-route` excluded unless exclusion reason and separate implementation plan requirement are recorded.
- [ ] Aggregate gate requires no PowerShell helper in product runtime/request path and no active product path under `spikes/**`.
- [ ] Aggregate gate requires repo migration preflight, verification ownership replacement, tier2/tier3 admin opt-in evidence, and release-gate separation before ADR-0004 promotion.
- [ ] GA scope classification rule treats all `current-route` and `product-operation` rows as GA-scope by default.
- [ ] GA scope classification rule allows only `future-route` rows to be excluded with an exclusion reason and separate implementation plan requirement.
- [ ] GA scope classification rule says release and network exposure gates are approval separation, not GA-scope exclusion.
- [ ] GA scope classification rule requires separate ADR/task approval before any `current-route` or `product-operation` row can be removed from product scope, and such removal cannot count as aggregate gate closure before approval.
- [ ] PowerShell-free product path closure rule covers product runtime, request, and admin execution paths.
- [ ] PowerShell-free product path closure rule prevents `current_owner = powershell-helper` or `current_owner = dotnet-request-processor-powershell-helper` rows from counting as aggregate gate closure before current owner is updated.
- [ ] PowerShell-free product path closure rule prevents `fallback_policy = transition-helper` rows from counting as aggregate gate closure before helper fallback removal evidence exists.
- [ ] PowerShell-free product path closure rule allows `fallback_policy = test-only` only for fixture/injectable test fallback, not product execution fallback.
- [ ] Active product path classification rule treats `spikes/**` in runtime/service/API/CLI/Web Console execution, packaging or installer input, static asset source, generated parity manifest, required verification, CI/local verification, or developer command docs as active product path.
- [ ] Active product path classification rule allows `archive/spikes/**` only as historical/read-only baseline, not product execution, packaging, or required verification source.
- [ ] Active product path classification rule requires repo migration preflight and docs command update evidence before aggregate GA-ready gate closure.
- [ ] ADR-0004 requires an aggregate gate closure report before current decision promotion, and the first Phase 26 alignment slice does not create the report.
- [ ] Aggregate gate closure report candidate location is `docs/ga-ready/evidence/aggregate-gate-closure-<YYYY-MM-DD>.md`, uses Markdown only, and does not introduce machine-readable JSON.
- [ ] Aggregate gate closure report schema includes current-route/product-operation counts, future-route exclusions, transition-helper/blocked counts, PowerShell owner/fallback counts, active `spikes/**` counts, required evidence status fields, stale/waived/waiver-only counts, and `aggregate_gate_status`.
- [ ] `aggregate_gate_status = closed` requires zero transition-helper, blocked, PowerShell owner, PowerShell fallback, active `spikes/**`, stale evidence, and waiver-only gate satisfaction counts plus all required status fields at `pass`.
- [ ] ADR-0001 replacement scope limits ADR-0004 promotion to the product runtime promotion decision and preserves standalone Windows repository and internal trusted signing current markers unless separate ADRs change them.
- [ ] ADR promotion procedure requires a separate promotion PR with `aggregate_gate_status = closed`, ADR-0004 status set to `적용 중`, ADR index current/proposed sections updated in the same diff, and exactly one current `PRODUCT_RUNTIME_PROMOTION_DECISION` source.
- [ ] ADR promotion procedure stops on ADR number collision, missing/non-closed closure report, duplicate current product runtime promotion source, or missing preserved non-promotion current marker.
- [ ] Evidence freshness rule requires tier2/tier3 admin opt-in evidence to record commit SHA, artifact/package version, route/operation row id, current owner, target owner, implementation basis, fallback policy, promotion state, admin smoke requirement, release gate, network exposure gate, runner version, host capability snapshot, and exact command mode.
- [ ] Evidence freshness rule marks evidence stale after current owner, target owner, implementation basis, fallback policy, promotion state, admin smoke requirement, release gate, network exposure gate, package contract, service host, installer custom action, or route matrix gate changes.
- [ ] Stale evidence remains historical context only and cannot satisfy aggregate GA-ready gates unless rerun or covered by separate approval waiver.
- [ ] Evidence ledger candidate location is `docs/ga-ready/evidence/`, but the first slice does not create evidence files.
- [ ] Evidence ledger candidate uses Markdown records only and does not introduce machine-readable JSON.
- [ ] Evidence ledger candidate schema includes `evidence_id`, `route_or_operation`, `route_surface`, `risk_tier`, `current_owner`, `commit_sha`, `artifact_or_package_version`, `target_owner`, `implementation_basis`, `fallback_policy`, `promotion_state`, `admin_smoke_required`, `release_gate`, `network_exposure_gate`, `runner_version`, `host_capability_snapshot`, `exact_command_mode`, `result`, `created_at`, `stale_triggers`, and `waiver_status`.
- [ ] Evidence row identity rule treats `route_or_operation` as an exact match to the route matrix `Route/Operation` cell and forbids duplicate matrix rows with the same identity.
- [ ] Evidence row identity rule makes route path, operation name, route_surface, current_owner, target_owner, implementation_basis, fallback_policy, promotion_state, admin_smoke_required, release_gate, or network_exposure_gate changes stale existing evidence.
- [ ] Evidence row identity rule forbids merging renamed rows into the same evidence record; renamed rows require rerun evidence or separate approval waiver.
- [ ] Evidence waiver policy says waiver cannot satisfy the aggregate GA-ready gate by itself and only replaces a specific stale evidence record in limited scope.
- [ ] Evidence waiver policy schema includes `waiver_id`, `evidence_id`, `scope`, `reason`, `risk_acceptance_owner`, `expires_at`, `replacement_evidence_required`, and `approval_reference`.
- [ ] Evidence waiver policy forbids waiver-only gate satisfaction for `tier3-destructive-or-persistent`, `release_gate = release-approval-required`, trust-store, and firewall LAN exposure rows; these require rerun evidence.
- [ ] Evidence field format rule reuses route matrix enums for `route_surface`, `risk_tier`, `current_owner`, `target_owner`, `implementation_basis`, `fallback_policy`, `promotion_state`, `admin_smoke_required`, `release_gate`, and `network_exposure_gate`.
- [ ] Evidence field format rule limits `result` to `pass`, `fail`, `blocked`, `not-run` and `waiver_status` to `none`, `requested`, `approved`, `rejected`, `expired`.
- [ ] Evidence field format rule requires full 40-char SHA by default, allows minimum 12-char abbreviated SHA, and limits `created_at`/`expires_at` to ISO-8601 timestamp or explicit milestone reference.
- [ ] Evidence field format rule requires non-empty free text for `scope`, `reason`, `host_capability_snapshot`, and `approval_reference`.
- [ ] Release-gated pre-release evidence boundary allows `release_gate = release-approval-required` rows to clear `blocked` before ADR-0004 promotion only with pre-release evidence, not release execution.
- [ ] Release-gated pre-release evidence boundary allows package/trust contract validation, manifest/hash/provenance validation, dry-run planning, non-mutating ownership checks, rollback plan validation, redaction evidence, and no-auto-reboot evidence.
- [ ] Release-gated pre-release evidence boundary forbids stable publication, public trusted signing execution, certificate store write/delete, and external update/rollback activation before release approval.
- [ ] Release approval 전 `release_gate = release-approval-required` row can become `ga-ready-candidate` but not execution-approved.
- [ ] LAN exposure pre-approval evidence boundary allows `network_exposure_gate = lan-exposure-approval-required` rows to clear `blocked` before LAN approval only with pre-LAN evidence, not firewall execution.
- [ ] LAN exposure pre-approval evidence boundary allows rule tuple validation, loopback default preservation proof, token source proof, non-mutating firewall ownership checks, scope planning, conflict diagnostics, redaction evidence, and no-auto-reboot evidence.
- [ ] LAN exposure pre-approval evidence boundary forbids firewall rule create/update/delete, non-loopback listener exposure, token source mutation, and external network reachability proof before LAN approval.
- [ ] LAN approval 전 `network_exposure_gate = lan-exposure-approval-required` row can become `ga-ready-candidate` but not exposure-approved.
- [ ] Route matrix separates API routes from product operations while keeping shared owner/implementation/fallback/promotion state/GA-ready gate/release gate/network exposure gate schema and enum allowed values.
- [ ] Route matrix가 `route_surface`로 `current-route`, `future-route`, `product-operation`을 구분한다.
- [x] Route matrix가 `DELETE /api/v1/vms/{id}`를 `current-route`, `current_owner = dotnet-native`, `fallback_policy = none`, `promotion_state = current-native`로 갱신하고 `0.30.1-admin-smoke` installed destructive smoke evidence를 기록한다.
- [ ] Served route scope rule이 side-by-side contract-only route 후보를 실제 served route 등록 전까지 matrix에서 제외한다.
- [ ] Served route scope rule이 contract mirror aggregate route 후보인 `POST /api/v1/vms/{vmId}/lifecycle/{action}`를 matrix row에서 제외한다.
- [ ] Future route execution guard가 Phase 26에서 `future-route` 구현/등록을 금지하고, `current-route` 전환 전 별도 implementation plan과 evidence를 요구한다.
- [ ] Route matrix가 `GET /api/v1/network/inventory`를 native-first/helper-fallback current route로 표현해 `current_owner = dotnet-native`, `fallback_policy = transition-helper`, `promotion_state = transition-helper`를 고정한다.
- [ ] Route matrix separates read-only `service status` from tier3 `service install create`, `service configure update`, service repair missing service recreation, service repair config drift correction, `service uninstall stop/delete`, `product root removal preserve-data`, and uninstall remove-data request rows.
- [ ] Route matrix separates tier2 `service start` and `service stop` from tier3 `service install create`, `service configure update`, service repair missing service recreation, service repair config drift correction, `service uninstall stop/delete`, `product root removal preserve-data`, and uninstall remove-data request rows.
- [ ] Route matrix separates `service start` from `service stop` and removes the aggregate `service start/stop` row.
- [ ] Route matrix keeps `service start` gated by owned service identity, exact SCM binary path/product root binding, foreign service blocks, missing-service diagnostics, no config mutation, no service delete, service started state, already-running idempotency, listener health after start, timeout/recovery, and no-auto-reboot evidence.
- [ ] Route matrix keeps `service stop` gated by owned service identity, exact SCM binary path/product root binding, foreign service blocks, missing-service diagnostics, no config mutation, no service delete, stop idempotency, already-stopped idempotency, stop wait timeout, stop wait timeout diagnostics, and no-auto-reboot evidence.
- [ ] Route matrix 명시: service start/stop은 foreign/unknown service identity 또는 product root 밖 binary path 발견 시 host mutation 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix separates `service repair missing service recreation` from `service repair config drift correction` and removes the aggregate `service repair` row.
- [ ] Route matrix keeps `service repair missing service recreation` gated by repair path only, service absent precondition, product root exists, owned product root evidence, existing config reuse, existing config ownership evidence, protected token path preservation, protected token ownership evidence, exact SCM binary path/product root binding, config schema validation before recreate, foreign existing service blocks, SCM service recreate, SCM binary path, service identity, no product root creation/removal, no config rewrite, no token rewrite, no data root creation, no token bootstrap, and no-auto-reboot evidence.
- [ ] Route matrix 명시: service repair missing service recreation은 서비스가 이미 있거나 product root/config/token ownership이 불명확하면 SCM recreate 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix keeps `service repair config drift correction` gated by repair path only, existing owned service, owned service identity, exact SCM binary path/product root binding, owned-field-only repair, allowed repair drift fields = protected token path, listener args, config drift diagnostics before mutation, config drift diff, protected token path/listener args update, foreign binary path blocks, non-repair drift handoff to service configure update, data preservation, rollback/recovery, no SCM recreate, no config rewrite, no token rewrite, no product root creation/removal, and no-auto-reboot evidence.
- [ ] Route matrix 명시: service repair config drift correction은 service identity/binary binding이 불명확하거나 drift가 허용 field 밖이면 host mutation 없이 blocked diagnostics 또는 configure handoff만 반환한다.
- [ ] Route matrix separates protected token bootstrap from `service install create` and `service configure update`.
- [ ] Route matrix separates `service install create` from `service configure update` and removes the aggregate `service install/configure` row.
- [ ] Route matrix keeps `service install create` limited to initial install path, missing-service precondition, service name ownership identity, foreign service conflict blocks, SCM service identity, exact SCM binary path/product root binding, no overwrite of existing foreign service, conflict diagnostics only, binary path, protected token path, listener args, service account, start type, failure policy, idempotent already-installed behavior, and no-auto-reboot evidence.
- [ ] Route matrix keeps `service configure update` limited to existing owned service precondition, owned-field-only config update, exact SCM binary path/product root binding, foreign binary path blocks, config drift diagnostics before mutation, config drift diff, protected token path, listener args update, data preservation, rollback/recovery on failed config update, and no-auto-reboot evidence.
- [ ] Route matrix 명시: service install/configure는 foreign/unknown service identity 또는 product root 밖 binary path 발견 시 host mutation 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix assigns protected token bootstrap to `dotnet-token-storage-action` with `dpapi-local-machine-token-plan`.
- [ ] Route matrix keeps raw token non-exposure, token source inventory, single-source precondition, existing protected token no-overwrite, legacy token migration, legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required, protected token schema, ACL hardening, service command line protected file path only, command line token value forbidden, and diagnostics redaction evidence on protected token bootstrap.
- [ ] Route matrix 명시: protected token bootstrap은 protected/raw token 동시 존재, ownership/schema/ACL evidence 불명확, 또는 command line token value 입력 시 host mutation 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix separates `service uninstall stop/delete` from `product root removal preserve-data` and removes the aggregate `service uninstall preserve-data` row.
- [ ] Route matrix keeps `service uninstall stop/delete` limited to owned service identity, exact SCM binary path/product root binding, foreign service blocks, stop-before-delete sequencing, stop idempotency, delete service only, delete idempotency, service deletion confirmation, missing-service idempotency, missing-service idempotent diagnostics, no product root delete, no data root delete, no config delete, no token delete, no REMOVE_DATA handoff, and no-auto-reboot evidence.
- [ ] Route matrix 명시: service uninstall stop/delete는 service identity/binary binding이 불명확하면 host mutation 없이 blocked diagnostics만 반환하고, data 삭제는 `service uninstall remove-data request`와 `data root remove`로만 흐른다.
- [ ] Route matrix keeps `product root removal preserve-data` limited to service absent/deleted precondition, owned product root evidence, exact product root allowlist, binary payload only delete, config/data/token preserve allowlist, ProgramData preserve evidence, data root delete forbidden evidence, protected token preserved evidence, no ProgramData delete, no protected token delete, locked-file abort before partial delete, locked-file abort diagnostics, partial product root delete forbidden evidence, cleanup diagnostics evidence, and no-auto-reboot evidence.
- [ ] Route matrix 명시: product root removal preserve-data는 service가 아직 설치돼 있거나 product root ownership이 불명확하거나 locked file이 있으면 host mutation 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix keeps data-root-remove handoff evidence off `product root removal preserve-data`.
- [ ] Route matrix separates `service uninstall remove-data request` from `data root remove`.
- [ ] Route matrix keeps `service uninstall remove-data request` limited to REMOVE_DATA=1 request validation, explicit remove-data intent source, service deleted/absent precondition, service deletion confirmation required, handoff descriptor only, data-root-remove handoff evidence, no direct data root mutation, no direct ProgramData delete, no direct protected token delete, missing-service idempotent diagnostics, and no-auto-reboot evidence.
- [ ] Route matrix 명시: service uninstall remove-data request는 service가 stopped 상태일 뿐이거나 service deletion confirmation이 없으면 host mutation 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix assigns `data root remove` to `dotnet-data-root-action` with `data-root-lifecycle-plan`.
- [ ] Route matrix keeps `data root remove` limited to REMOVE_DATA=1 explicit opt-in, remove-data handoff descriptor required, exact data root path allowlist, owned data root marker/evidence, service deleted/absent precondition, installed service blocks delete diagnostics, protected token delete only within owned data root, no product root mutation, no service mutation, locked-file abort before partial delete, delete manifest/journal evidence, post-delete absence evidence, no partial delete success evidence, diagnostics evidence, and no-auto-reboot evidence.
- [ ] Route matrix 명시: data root remove는 handoff descriptor가 없거나 data root ownership이 불명확하거나 service가 아직 설치돼 있으면 삭제 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix defines `Mixed History Resolution Rule` and limits `mixed-history` to service product operation rows.
- [ ] Route matrix requires service product operation implementation plans to resolve actual current code path and evidence source before promotion.
- [ ] Route matrix prevents `mixed-history` from being treated as promotion evidence or target owner.
- [ ] Route matrix does not keep the stale aggregate `service install/repair/remove` row.
- [ ] Route matrix separates `Event Log source registration`, `Event Log source removal`, `firewall rule enable LAN exposure`, and `firewall rule removal`.
- [ ] Route matrix defines `OS Mutation Execution Guard` so default install/repair/diagnostics/MSI paths do not execute Event Log, firewall, or trust store mutations.
- [ ] Route matrix separates `Event Log source registration` from `Event Log source removal` and removes the aggregate `Event Log registration` row.
- [ ] Route matrix keeps `Event Log source registration` limited to explicit admin opt-in, exact event source name, exact channel/log name, owned event source manifest/evidence, missing-or-owned-source precondition, foreign-source conflict blocks, exact log/source binding, no overwrite of existing foreign source, registry write limited to event source registration, no service mutation, no firewall mutation, no trust store mutation, conflict diagnostics only, post-registration binding evidence, no MSI/default execution, and no-auto-reboot evidence.
- [ ] Route matrix 명시: Event Log source registration은 source/channel ownership 또는 log/source binding이 불명확하면 registry write 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix keeps `Event Log source removal` limited to explicit admin opt-in, exact event source name, exact channel/log name, owned event source manifest/evidence, exact log/source binding, owned-source-only removal, foreign-source conflict blocks, registry delete limited to owned event source registration, no service mutation, no firewall mutation, no trust store mutation, missing-source idempotency, cleanup diagnostics only, post-removal absence evidence, no MSI/default execution, and no-auto-reboot evidence.
- [ ] Route matrix 명시: Event Log source removal은 source/channel ownership 또는 log/source binding이 불명확하면 registry delete 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix separates `firewall rule enable LAN exposure` from `firewall rule removal` and removes the aggregate `firewall rule changes` row.
- [ ] Route matrix keeps `firewall rule enable LAN exposure` limited to LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence, no default install/repair/MSI execution, no-auto-reboot evidence, and `lan-exposure-approval-required`.
- [ ] Route matrix 명시: firewall rule enable LAN exposure는 rule tuple/ownership/scope가 불명확하면 firewall write 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix keeps `firewall rule removal` limited to explicit admin opt-in, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence, owned-rule-only removal, foreign-rule conflict blocks, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution, no-auto-reboot evidence, and `network_exposure_gate = none`.
- [ ] Route matrix 명시: firewall rule removal은 rule tuple/ownership/scope가 불명확하면 firewall delete 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix separates `trust store install` from `trust store removal` and removes the aggregate `trust store changes` row.
- [ ] Route matrix keeps `trust store install` limited to release approval, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, no service mutation, no firewall mutation, no eventlog mutation, thumbprint/store binding evidence, post-install trust binding evidence, no default install/repair/MSI execution, no-auto-reboot evidence, and `release-approval-required`.
- [ ] Route matrix 명시: trust store install은 artifact/identity/store ownership이 불명확하면 certificate store write 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix keeps `trust store removal` limited to release approval, explicit admin opt-in, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, owned certificate evidence, thumbprint/store binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, certificate store delete limited to owned certificate, no service mutation, no firewall mutation, no eventlog mutation, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution, no-auto-reboot evidence, and `release-approval-required`.
- [ ] Route matrix 명시: trust store removal은 certificate identity/store ownership이 불명확하면 certificate store delete 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix separates release approval from LAN exposure approval with `network_exposure_gate`.
- [ ] Route matrix가 auth/exposure boundary를 `single_bearer_token`, `multi_user = false`, `rbac = false`, loopback static asset bypass only, non-loopback bearer-required static assets, LAN mode requires `-AllowLan` and a token source로 고정한다.
- [ ] Route matrix가 `GET /api/v1/runtime/policy`를 current-native tier1 read-only job-runtime policy route로 포함한다.
- [ ] Route matrix가 VM lifecycle `shutdown`과 `restart`를 tier2 explicit admin opt-in route로 포함하고 각각 graceful shutdown/stop-start sequencing gate를 분리한다.
- [ ] Route matrix separates read-only job get from tier2 job cancel/retry with explicit admin opt-in.
- [ ] Route matrix가 job retry risk inheritance를 정의해 retry가 원본 operation의 risk/admin/release/network gate를 우회하지 못하게 한다.
- [ ] Route matrix가 job route path parameter를 `job_id`로 통일하고 `id`/`jobId`는 route identity parameter로 사용하지 않는다.
- [ ] Route matrix가 VM route path parameter `{id}`를 기존 served API 계약으로 유지하고, `vmId`/`vm_id`를 route identity parameter로 사용하지 않는다.
- [ ] Route matrix가 checkpoint list를 tier1 read-only로, checkpoint restore를 tier2 explicit admin opt-in으로 포함한다.
- [ ] Route matrix가 checkpoint route parameter를 `checkpoint_id`로 통일하고 `name`/`checkpoint_name`은 alias contract로 분리한다.
- [ ] Route matrix separates aggregate `update/rollback` into `local payload update` and `rollback restore`.
- [ ] Route matrix separates `local payload update` and `rollback restore` from `product config schema validation`, `product config migration apply`, `job store schema mismatch detection`, and `job store migration apply`.
- [ ] Route matrix keeps `local payload update` gated by signed/approved package manifest required, manifest hash verification, ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation, exact product root ownership evidence, service stopped precondition, active root snapshot before activation, staged root outside active root, binary payload only activation, no config mutation, no data root mutation, no token mutation, no service identity mutation, atomic activation or full rollback, partial activation forbidden evidence, post-activation manifest/version evidence, service start health check, rollback attempt on failure, rollback result diagnostics, no-auto-reboot evidence, and release approval.
- [ ] Route matrix 명시: local payload update는 manifest/hash/root ownership/service stopped evidence 또는 ADR-0002 channel/version/update payload binding 중 하나라도 불명확하면 activation 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix keeps `rollback restore` gated by retained previous root, previous root manifest/hash verification, previous root ownership evidence, ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, failed root diagnostics preservation, service stopped precondition, current active root snapshot before rollback, staged rollback root outside active root, binary payload only restore, no config mutation, no data root mutation, no token mutation, no service identity mutation, atomic rollback or current root preservation, failed root preservation, partial restore forbidden evidence, invalid previous manifest rejection, post-rollback manifest/version evidence, rollback health check after restore, rollback result diagnostics, no-auto-reboot evidence, and release approval.
- [ ] Route matrix 명시: rollback restore는 previous root/hash/ownership/service stopped evidence 또는 ADR-0002 channel/version/previous root slot binding이 불명확하면 restore 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix makes `release-approval-required` exclusive to `local payload update`, `rollback restore`, `trust store install`, and `trust store removal`.
- [ ] Route matrix separates read-only `product config schema validation` from tier3 `product config migration apply`.
- [ ] Route matrix keeps `product config schema validation` as `product-wrapper` current owner with `dotnet-runtime` target/basis, read-only config inventory, owned config path evidence, schema version parse evidence, config schema compatibility, dry-run validation before service start, service-start preflight decision descriptor only, validation failure diagnostics, diagnostics redaction evidence, no config write, no backup write, no service mutation, and no migration execution.
- [ ] Route matrix 명시: product config schema validation은 config path ownership/schema parse가 불명확하면 write 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix keeps `product config migration apply` blocked under `dotnet-config-migration-action` with `product-config-migration-plan`.
- [ ] Route matrix keeps `product config migration apply` gated by current config source inventory, current schema owner resolution, owned source config path evidence, source path/version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, validation preflight descriptor required, backup path inside owned config backup root, atomic config replace, no data root mutation, no token mutation, no job store mutation, no service identity mutation, partial config migration forbidden evidence, rollback on migration failure, rollback result diagnostics, cleanup evidence, service-start preflight decision descriptor only, validation writes forbidden, and explicit admin opt-in before config write.
- [ ] Route matrix 명시: product config migration apply는 config ownership/schema/migration plan/service stopped evidence가 없으면 config write 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix makes `not-yet-defined` current owner exclusive to `product config migration apply`.
- [ ] Route matrix defines a resolution rule for `not-yet-defined` before any `product config migration apply` implementation plan.
- [ ] Route matrix 명시: `product config migration apply`는 `not-yet-defined` 해소 전 구현 금지이며, 해소되지 않으면 `promotion_state = blocked`를 유지한다.
- [ ] Route matrix makes `dotnet-config-migration-action` exclusive to `product config migration apply`.
- [ ] Route matrix makes `product-config-migration-plan` exclusive to `product config migration apply` and defines it as current config source inventory, current schema owner resolution, owned source config path evidence, source path/version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, validation preflight descriptor required, backup path inside owned config backup root, atomic config replace, no data root mutation, no token mutation, no job store mutation, no service identity mutation, partial config migration forbidden evidence, rollback on migration failure, rollback result diagnostics, cleanup evidence, service-start preflight decision descriptor only, validation writes forbidden, and explicit admin opt-in before config write.
- [ ] Route matrix makes `dotnet-token-storage-action` exclusive to `protected token bootstrap`.
- [ ] Route matrix makes `dotnet-data-root-action` exclusive to `data root remove`.
- [ ] Route matrix makes `windows-native-package` exclusive to `local payload update` and `rollback restore`.
- [ ] Route matrix makes `package-contract` exclusive to `local payload update` and `rollback restore`.
- [ ] Route matrix defines `package-contract` as ADR-0002 channel/version contract binding, source/target release_channel evidence, update payload manifest version match, from-version/to-version compatibility, rc/stable RequireSigned trust_model evidence, downgrade forbidden except rollback, single previous root slot, data root preservation, and failed root diagnostics preservation only.
- [ ] Route matrix assigns `Event Log source registration` and `Event Log source removal` to `windows-eventlog-action`, not `windows-native-package`.
- [ ] Route matrix assigns `firewall rule enable LAN exposure` and `firewall rule removal` to `windows-firewall-action`, not `windows-native-package`.
- [ ] Route matrix separates read-only `job store schema mismatch detection` from tier3 `job store migration apply`.
- [ ] Route matrix keeps `job store schema mismatch detection` as `dotnet-runtime` owned with `read-only-or-blocked-with-diagnostics`, schema mismatch returns blocked diagnostics, runtime read must not mutate jobs.json, no quarantine move/write, migration handoff descriptor only, no migration execution, and diagnostics evidence.
- [ ] Route matrix 명시: actual job store migration execution is allowed only in `job store migration apply`, not in `job store schema mismatch detection`.
- [ ] Route matrix keeps `job store migration apply` blocked under `dotnet-job-store-migration-action` with `job-store-migration-plan`.
- [ ] Route matrix keeps `job store migration apply` gated by current job store path inventory, current job schema owner evidence, owned job store path evidence, source job store version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, runtime writer stopped evidence, backup path inside owned job-store backup root, destructive rewrite disabled by default, atomic job store replace, no config mutation, no token mutation, no service identity mutation, partial job store migration forbidden evidence, rollback on migration failure, rollback result diagnostics, recovery evidence, and explicit admin opt-in before job store write.
- [ ] Route matrix 명시: job store migration apply는 job store ownership/schema/migration plan/runtime-writer stopped evidence가 없으면 job store write 없이 blocked diagnostics만 반환한다.
- [ ] Route matrix makes `dotnet-job-store-migration-action` exclusive to `job store migration apply`.
- [ ] Route matrix 명시: `job store migration apply`에서 `dotnet-runtime`은 read/schema mismatch detection의 현재 owner 근거일 뿐이며, migration mutation은 `dotnet-job-store-migration-action`이 소유한다.
- [ ] Route matrix 명시: `job store migration apply`는 job store ownership/schema/migration plan/runtime-writer stopped evidence가 불명확하면 `promotion_state = blocked`를 유지한다.
- [ ] Route matrix makes `job-store-migration-plan` exclusive to `job store migration apply` and defines it as current job store path inventory, current job schema owner evidence, owned job store path evidence, source job store version evidence, source/target schema version evidence, migration plan id/version, service stopped precondition, runtime writer stopped evidence, backup path inside owned job-store backup root, destructive rewrite disabled by default, atomic job store replace, no config mutation, no token mutation, no service identity mutation, partial job store migration forbidden evidence, rollback on migration failure, rollback result diagnostics, recovery evidence, and explicit admin opt-in before job store write.
- [ ] Route matrix assigns `Event Log source registration` and `Event Log source removal` to `eventlog-registration-plan` implementation basis.
- [ ] Route matrix makes `eventlog-registration-plan` limited to exact event source name, exact channel/log name, owned event source manifest/evidence, missing-or-owned-source precondition, foreign-source conflict blocks, exact log/source binding, no overwrite of existing foreign source, registry write limited to event source registration, registry delete limited to owned event source registration, no service mutation, no firewall mutation, no trust store mutation, conflict diagnostics only, post-registration binding evidence, owned-source-only removal, missing-source idempotency, cleanup diagnostics only, post-removal absence evidence, no MSI/default execution, and not an MSI default action.
- [ ] Route matrix assigns `firewall rule enable LAN exposure` and `firewall rule removal` to `firewall-rule-plan` implementation basis.
- [ ] Route matrix makes `firewall-rule-plan` limited to LAN exposure approval, explicit admin opt-in, loopback default preservation, exact rule name, exact direction, exact protocol, exact local port, exact profile, exact remote address scope, owned rule evidence, missing-or-owned-rule precondition, foreign-rule conflict blocks, no overwrite of existing foreign rule, firewall write limited to owned allow rule, firewall delete limited to owned allow rule, no service mutation, no eventlog mutation, no trust store mutation, conflict diagnostics only, post-enable rule binding evidence, owned-rule-only removal, missing-rule idempotency, cleanup diagnostics only, post-removal absence evidence, no default install/repair/MSI execution, and not a default install/repair/MSI action.
- [ ] Route matrix makes `dpapi-local-machine-token-plan` exclusive to `protected token bootstrap`.
- [ ] Route matrix defines `dpapi-local-machine-token-plan` as raw token non-exposure, token source inventory, single-source precondition, existing protected token no-overwrite, legacy raw migration only when protected token missing, source conflict diagnostics, owned legacy token source required, protected token schema, ACL hardening, service command line protected file path only, command line token value forbidden, and diagnostics redaction evidence only.
- [ ] Route matrix makes `data-root-lifecycle-plan` exclusive to `data root remove` and defines it as `REMOVE_DATA=1`, remove-data handoff descriptor required, exact data root path allowlist, owned data root marker/evidence, service deleted/absent precondition, installed service blocks delete diagnostics, protected token delete only within owned data root, no product root mutation, no service mutation, locked-file abort before partial delete, delete manifest/journal evidence, post-delete absence evidence, no partial delete success evidence, and diagnostics evidence.
- [ ] Route matrix makes `lan-exposure-approval-required` exclusive to `firewall rule enable LAN exposure`.
- [ ] Route matrix keeps `firewall rule removal` at `network_exposure_gate = none`.
- [ ] Route matrix makes `windows-trust-store-action` exclusive to `trust store install` and `trust store removal`.
- [ ] Route matrix makes `windows-certificate-store-api` exclusive to `trust store install` and `trust store removal`.
- [ ] Route matrix defines `windows-certificate-store-api` as release approval, explicit admin opt-in, exact certificate source artifact, artifact hash evidence, exact certificate identity/thumbprint, subject/issuer/serial validity evidence, LocalMachine Root/TrustedPublisher exact store/location, ADR-0003 internal trust policy binding, internal/public trust model separation, missing-or-owned-certificate precondition, subject collision diagnostics, no overwrite of existing foreign certificate, certificate store write limited to approved certificate, owned certificate evidence, thumbprint/store binding evidence, certificate store delete limited to owned certificate, no service mutation, no firewall mutation, no eventlog mutation, post-install trust binding evidence, owned-certificate-only removal, foreign certificate conflict blocks, missing-certificate idempotency, cleanup diagnostics only, post-removal absence evidence, and no default install/repair/MSI execution only.
- [ ] Route matrix defines `Approved System Executable Rule` while keeping `approved-system-executable` as schema enum only in the first slice.
- [ ] Route matrix has zero rows with `implementation_basis = approved-system-executable` in the first slice.
- [ ] Route matrix requires ADR/task approval required, exact executable path and publisher/hash evidence, non-shell invocation only, argument schema with allowed flags/values, no user-controlled raw arguments, working directory fixed, environment variable allowlist, no token/secret on command line, no implicit reboot, timeout/exit-code contract, stdout/stderr redaction, dry-run/WhatIf where supported, no chained shell, admin opt-in, post-run evidence, and examples are candidates only, not allowlist before any later `approved-system-executable` row.
- [ ] Route matrix keeps `approved-system-executable` implementation basis blocked when executable identity or argument ownership is unclear.
- [ ] Route matrix defines state invariants that prevent `promotion_state` and `fallback_policy` contradictions.
- [ ] Route matrix defines risk/admin smoke invariants that prevent mutation rows from using non-admin default verification.
- [ ] Pester parser fails on duplicate route matrix `Route/Operation` identity.
- [ ] Pester parses Markdown table rows to enforce `promotion_state` and `fallback_policy` invariants without introducing machine-readable JSON.
- [ ] Pester parses Field Schema enum allowed values and Markdown table rows without introducing machine-readable JSON.
- [ ] Repo migration map separates active product target from archive target under `docs/ga-ready/**`.
- [ ] Repo migration map이 파일 이동 실행 승인이 아니라 승인 시 목표 상태이며, 이동 전 별도 implementation plan, rollback 기준, archive 검증이 필요하다고 명시한다.
- [ ] Repo migration map keeps the first slice as no file movement.
- [ ] Repo migration map requires source path inventory, import/relative path graph, packaging/static asset input binding, generated parity manifest update, docs command update, no behavior change evidence, archive target read-only intent, rollback restore 기준, and related Pester/npm/`verify:parity`/`node --check` evidence before any later migration.
- [ ] Repo migration map keeps migration blocked when that preflight evidence is missing.
- [ ] Verification ownership map does not make Playwright a required dependency in this slice.
- [ ] Verification ownership map defines browser-level fixture 후보 as 후속 npm/package-owned loopback fixture limited to static asset load, initial render, deterministic `GET /api/v1/runtime/policy` connection, optional bearer 401/200 handling, and token/redaction 확인.
- [ ] Verification ownership map excludes API route contract, route parity, Hyper-V, service/MSI/firewall/Event Log/trust store mutation, LAN exposure, and Playwright required dependency from the browser-level fixture 후보.
- [ ] Verification ownership map keeps Pester suite as required verification in the first slice.
- [ ] Verification ownership map requires owner replacement, equivalent coverage mapping, archive baseline path, docs command update, CI/local command replacement, and rollback 기준 before suite-level Pester retirement.
- [ ] Verification ownership map prevents any Pester suite from being lowered to archive-only while PowerShell helper or `spikes/**` remains in the active path.
- [ ] Verification ownership map includes diagnostics/redaction boundary for diagnostics bundle, JSONL logs, bearer/API token, signing secrets, and repo/data path tokens.
- [ ] Verification ownership map includes data root lifecycle boundary for default uninstall/repair preserve, `REMOVE_DATA=1` delete targets, Program Files vs owned data root separation, handoff descriptor ownership, and protected token deletion only within owned data root.
- [ ] Verification ownership map moves product primary verification toward xUnit/npm/browser-level fixture candidate/package contract while keeping admin opt-in mutation boundaries.
- [ ] The first slice does not remove PowerShell helper, move `spikes/**`, or run administrator mutation.

## GSTACK REVIEW REPORT

| Review | Trigger | Why | Runs | Status | Findings |
|--------|---------|-----|------|--------|----------|
| CEO Review | `/plan-ceo-review` | Scope & strategy | 0 | not-run | Not requested in this slice |
| Codex Review | `codex review` | Independent 2nd opinion | 0 | not-run | Not requested in this slice |
| Eng Review | `/plan-eng-review` | Architecture & tests (required) | 1 | clean | resolved: supporting doc collision preflight, execution dependency diagram, parser duplicate row guard, single-writer strategy |
| Design Review | `/plan-design-review` | UI/UX gaps | 1 | clean | score: 6/10 -> 8/10, 4 decisions |
| DX Review | `/plan-devex-review` | Developer experience gaps | 0 | not-run | Not requested in this slice |

- UNRESOLVED: 0 design decisions.
- UNRESOLVED: 0 engineering decisions.
- VERDICT: DESIGN + ENG CLEARED for the GA-ready spec alignment; DX review and independent codex review remain optional because they were not requested for this slice.
