# PureCVisor Desktop Node Phase 11 제품 런타임 승격 판단 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node를 제품 런타임으로 승격하지 않고 격리 spike로 유지한다는 결정을 문서와 검증 gate에 남긴다.

**Architecture:** 기능 코드는 이동하지 않는다. Phase 11은 root README, 설계/계획 문서, release boundary, verification policy, developer index, follower queue를 동기화하고 root boundary Pester suite로 결정 drift를 막는다.

**Tech Stack:** Markdown, PowerShell 7, Pester 5.

---

## 파일 구조

- Create: `spikes/purecvisor-desktop-node/README.md`
  - Desktop Node spike의 root entrypoint, keep-spike 결정, directory 역할, 승격 gate를 기록한다.
- Create: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`
  - root README, Phase 11 설계/계획, release boundary, verification policy가 같은 결정을 담는지 검증한다.
- Create: `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
  - 제품 런타임 승격 판단과 승격 전 gate를 기록한다.
- Create: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision.md`
  - 실행 단계와 검증 증거를 기록한다.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - Desktop Node Phase 11 keep-spike 결정을 공개 릴리스 경계에 반영한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Desktop Node runtime promotion decision 문서 검증 gate를 추가한다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Phase 11 설계/계획과 root README 진입점을 추가한다.
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
  - 현재 구현 상태와 roadmap을 Phase 11까지 갱신한다.
- Modify: `AGENTS.md`
  - Phase 11 문서 진입점과 spike 격리 규칙을 반영한다.
- Modify: `follower.md`
  - Phase 11을 완료 처리하고 다음 코어 ADR 후속을 P0로 승격한다.

## 완료 상태

- [x] Phase 11 범위 확정
- [x] Desktop Node root boundary suite red test 작성
- [x] red 실패 확인
- [x] Phase 11 설계 문서 작성
- [x] Phase 11 구현 계획 작성
- [x] Desktop Node root README 작성
- [x] release boundary와 verification policy 갱신
- [x] developer index와 AGENTS 갱신
- [x] follower queue 갱신
- [x] root boundary suite green 확인
- [x] Desktop Node 기본 suite smoke 확인
- [x] 완료 증거 갱신

## Task 1: root boundary red test

**Files:**
- Create: `spikes/purecvisor-desktop-node/tests/PcvDesktopNode.Boundary.Tests.ps1`

- [x] **Step 1: root README decision test 작성**

`spikes/purecvisor-desktop-node/README.md`에 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`, `spikes/purecvisor-desktop-node/**`, Linux `purecvisorsd`, Single Edge 경계가 있는지 검증한다.

- [x] **Step 2: Phase 11 spec/plan gate test 작성**

Phase 11 설계와 계획 문서가 존재하고 signed release installer, 업데이트, 롤백, 로그 수집, 서비스 복구 gate를 기록하는지 검증한다.

- [x] **Step 3: release boundary/policy separation test 작성**

`docs/PUBLIC_RELEASE_BOUNDARY.md`와 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`가 Phase 11 decision과 Desktop Node root boundary suite를 기록하는지 검증한다.

- [x] **Step 4: red 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: root README, Phase 11 spec/plan, release boundary/policy 표식이 없어 실패한다.

Observed red: 3 failed. root README 없음, Phase 11 spec/plan 없음, release boundary에 Phase 11 decision 없음.

## Task 2: decision docs

**Files:**
- Create: `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`
- Create: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision.md`
- Create: `spikes/purecvisor-desktop-node/README.md`

- [x] **Step 1: 설계 문서 작성**

Desktop Node를 제품 런타임으로 승격하지 않고 `spikes/purecvisor-desktop-node/**` spike로 유지한다는 결정을 기록한다.

- [x] **Step 2: 승격 전 gate 기록**

signed release installer, Windows Desktop Node 배포 단위, version policy, 업데이트, 롤백, 로그 수집, 서비스 복구, 관리자 권한 integration gate를 승격 전 필수 조건으로 기록한다.

- [x] **Step 3: root README 작성**

디렉터리별 역할, 현재 상태, keep-spike decision, 검증 명령, 제품 승격 gate를 한 곳에 모은다.

## Task 3: boundary docs

**Files:**
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- Modify: `AGENTS.md`

- [x] **Step 1: release boundary 갱신**

Phase 11 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`와 승격 전 필수 gate를 `Desktop Node spike 경계`에 추가한다.

- [x] **Step 2: verification policy 갱신**

`Desktop Node runtime promotion decision` 변경 유형과 Desktop Node root boundary suite 실행 명령을 추가한다.

- [x] **Step 3: developer index와 AGENTS 갱신**

Phase 11 설계/계획/root README 진입점을 추가하고, 작업 추천 경로에 승격 판단 문서를 추가한다.

- [x] **Step 4: MVP 설계 갱신**

Phase 8/9/10/11 현재 구현 상태와 최신 기대 검증 값을 반영한다.

## Task 4: follower and verification

**Files:**
- Modify: `follower.md`
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision.md`

- [x] **Step 1: follower queue 갱신**

Phase 11을 완료 항목으로 옮기고 다음 작업을 `P0. 코어 ADR 후속 - ADR-0018 fire-and-forget audit`로 승격한다.

- [x] **Step 2: root boundary suite green 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: 3 passed, 0 failed.

Observed green: 3 passed, 0 failed.

- [x] **Step 3: Desktop Node 기본 suite smoke 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Expected: 기존 기대값 유지. `git diff --check`는 CRLF 변환 경고 외 오류 없음.

## 완료 증거

2026-04-25 검증 결과:

- Desktop Node root boundary suite: 3 passed, 0 failed
- Local API: 85 passed, 0 failed
- Service packaging: 12 passed, 0 failed
- CLI: 11 passed, 0 failed
- Web Console static suite: 11 passed, 0 failed
- Web JavaScript syntax: `node --check spikes/purecvisor-desktop-node/web/app.js` exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- Service smoke: `PrepareTokenFile -WhatIf`, `Config`, `Install -WhatIf` exit 0
- CLI smoke: `Invoke-PcvDesktopCli.ps1 --help` exit 0
- `git diff --check`: exit 0, CRLF 변환 경고만 출력
