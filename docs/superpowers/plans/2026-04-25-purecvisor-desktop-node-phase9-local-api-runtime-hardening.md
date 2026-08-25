# PureCVisor Desktop Node Phase 9 Local API runtime hardening 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Local API runtime 정책을 명시적으로 노출하고 manual retry 상한을 추가한다.

**Architecture:** 기존 `PcvDesktopApi.psm1` 단일 module 패턴을 유지한다. 새 runtime policy helper와 route는 read-only이고, retry 상한은 기존 `Retry-PcvApiJob` 경계에만 추가한다.

**Tech Stack:** PowerShell 7, Pester 5, JSON-over-HttpListener contract tests.

---

## 파일 구조

- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - `Get-PcvApiRuntimePolicy`, retry 상한, `/api/v1/runtime/policy` route를 담당한다.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
  - runtime policy route contract와 read-only method gate를 검증한다.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1`
  - manual retry 상한을 검증한다.
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
  - Phase 9 status와 runtime policy route를 문서화한다.
- Modify: `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `AGENTS.md`, `follower.md`
  - Phase 9 진입점과 검증 기준을 반영한다.

## 완료 상태

- [x] Phase 9 범위 확인
- [x] runtime policy route red test 작성
- [x] runtime policy read-only gate test 작성
- [x] retry limit red test 작성
- [x] red test 실패 확인
- [x] runtime policy helper/route 구현
- [x] runtime policy read-only method gate 구현
- [x] retry limit 구현
- [x] focused API tests green 확인
- [x] README와 상위 문서 갱신
- [x] 전체 Desktop Node 기본 검증 실행
- [x] 완료 증거 갱신

## Task 1: runtime policy and retry red tests

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1`

- [x] **Step 1: runtime policy route test 작성**

`GET /api/v1/runtime/policy`가 JSON file persistence, no automatic retry, queued-only cancel, bounded worker, no CORS, single bearer token auth 결정을 반환하는지 검증한다.

- [x] **Step 2: retry limit test 작성**

`attempt=3`인 failed job을 retry하면 `PCV_JOB_RETRY_LIMIT_REACHED`로 거부하는지 검증한다.

- [x] **Step 3: runtime policy read-only gate test 작성**

`POST /api/v1/runtime/policy`가 `PCV_METHOD_NOT_ALLOWED`로 거부되는지 검증한다.

- [x] **Step 4: red 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1' -Output Detailed"
```

Expected: runtime policy route가 없고 retry limit이 없어 실패한다.

Observed red: runtime policy route는 `404 PCV_ROUTE_NOT_FOUND`, retry limit test는 `202` retry accepted로 실패했다. read-only gate 추가 전 `POST /api/v1/runtime/policy`는 `404`를 반환해 `405` 기대값으로 실패했다.

## Task 2: implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [x] **Step 1: runtime policy helper 추가**

`Get-PcvApiRuntimePolicy`를 추가하고 export한다.

- [x] **Step 2: route 추가**

`Invoke-PcvApiRequest`의 GET route map에 `/api/v1/runtime/policy`를 추가한다.

- [x] **Step 3: read-only method gate 추가**

`POST /api/v1/runtime/policy`를 `405 PCV_METHOD_NOT_ALLOWED`로 거부한다.

- [x] **Step 4: retry limit 추가**

`Retry-PcvApiJob`에 `MaxAttempts` 기본값 3을 추가하고, 초과 시 409 response를 반환한다.

- [x] **Step 5: focused green 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1' -Output Detailed"
```

Expected: focused API tests pass.

Observed green: focused contract/job-control files에서 24 passed, 0 failed.

## Task 3: documentation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `AGENTS.md`
- Modify: `follower.md`

- [x] **Step 1: API README 갱신**

Phase 9 status, `/api/v1/runtime/policy`, retry 상한, deferred decisions를 기록한다.

- [x] **Step 2: 상위 문서 갱신**

Phase 9 설계/계획 링크와 검증 기준을 추가한다.

- [x] **Step 3: follower 갱신**

Local API runtime hardening을 완료 항목으로 옮기고 다음 P1을 P0로 승격한다.

## Task 4: verification

**Files:**
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase9-local-api-runtime-hardening.md`

- [x] **Step 1: 기본 검증 실행**

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

- [x] **Step 2: 완료 증거 갱신**

검증 결과 수치를 이 문서의 완료 증거 절에 반영한다.

## 완료 증거

2026-04-25 검증 결과:

- Local API: 85 passed, 0 failed
- Service packaging: 12 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: `node --check spikes/purecvisor-desktop-node/web/app.js` exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- Service smoke: `PrepareTokenFile -WhatIf`, `Config`, `Install -WhatIf` exit 0
- CLI smoke: `Invoke-PcvDesktopCli.ps1 --help` exit 0
- `git diff --check`: exit 0, CRLF 변환 경고만 출력
