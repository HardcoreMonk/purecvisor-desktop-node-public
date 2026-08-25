# PureCVisor Desktop Node Phase 10 Web Console/CLI 제품화 후속 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Web Console에 checkpoint controls와 persistent browser job history를 추가하고, CLI에 token file UX를 연결한다.

**Architecture:** 새 backend 없이 기존 Local API endpoint만 사용한다. Web Console은 static asset 범위에서 상태와 localStorage helper를 추가하고, CLI는 기존 global option parser에서 `--token-file`을 resolve해 기존 `ApiToken` transport 계약으로 전달한다.

**Tech Stack:** Vanilla JavaScript, static HTML/CSS, PowerShell 7, Pester 5.

---

## 파일 구조

- Modify: `spikes/purecvisor-desktop-node/web/app.js`
  - checkpoint fetch/action helpers, checkpoint rendering, localStorage job history helper를 담당한다.
- Modify: `spikes/purecvisor-desktop-node/web/index.html`
  - jobs section에 history clear control을 추가한다.
- Modify: `spikes/purecvisor-desktop-node/web/styles.css`
  - checkpoint list와 compact action form styling을 추가한다.
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - checkpoint UI action 표식과 browser job history 표식을 검증한다.
- Modify: `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`
  - `--token-file`, token source conflict, token file read/trim validation을 담당한다.
- Modify: `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`
  - CLI token file success/conflict/missing/empty 경계를 검증한다.
- Modify: `spikes/purecvisor-desktop-node/api/README.md`, `spikes/purecvisor-desktop-node/cli/README.md`
  - Phase 10 사용법과 검증 기대값을 기록한다.
- Modify: `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `AGENTS.md`, `follower.md`
  - Phase 10 진입점과 검증 기준을 반영한다.

## 완료 상태

- [x] Phase 10 범위 확정
- [x] 설계 문서 작성
- [x] Web checkpoint UI red test 작성
- [x] Web job history red test 작성
- [x] CLI token-file red tests 작성
- [x] red test 실패 확인
- [x] Web Console checkpoint UI 구현
- [x] Web Console job history persistence 구현
- [x] CLI token-file 구현
- [x] focused green 확인
- [x] README와 상위 문서 갱신
- [x] 전체 Desktop Node 기본 검증 실행
- [x] 완료 증거 갱신

## Task 1: red tests

**Files:**
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`

- [x] **Step 1: Web checkpoint UI test 작성**

`app.js`에 `/checkpoints`, `checkpoint-create`, `checkpoint-restore`, `checkpoint-delete`, `loadCheckpoints` 표식이 있는지 검증한다.

- [x] **Step 2: Web browser job history test 작성**

`app.js`와 `index.html`에 `localStorage`, `pcvDesktopTrackedJobs.v1`, `clear-job-history` 표식이 있는지 검증한다.

- [x] **Step 3: CLI token-file success test 작성**

임시 token file을 만들고 `--token-file <path>`가 trimmed token을 transport의 `ApiToken`으로 전달하는지 검증한다.

- [x] **Step 4: CLI token source conflict/missing/empty tests 작성**

`--token`과 `--token-file` 동시 사용, missing file, empty file을 exit code `2`로 거부하는지 검증한다.

- [x] **Step 5: red 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests','spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
```

Expected: 새 checkpoint/job history/token-file 표식과 parser 기능이 없어 실패한다.

Observed red: Web checkpoint UI와 browser job history 표식 2개가 실패했고, CLI token-file success/conflict/missing/empty 경계 3개가 실패했다.

## Task 2: Web Console implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/web/app.js`
- Modify: `spikes/purecvisor-desktop-node/web/index.html`
- Modify: `spikes/purecvisor-desktop-node/web/styles.css`

- [x] **Step 1: state와 storage helper 추가**

`selectedVmCheckpoints`, `checkpointPending`, `JOB_HISTORY_KEY`, `loadTrackedJobsFromStorage`, `saveTrackedJobsToStorage`, `clearTrackedJobHistory`를 추가한다.

- [x] **Step 2: checkpoint helper 추가**

`getCheckpointId`, `getCheckpointName`, `loadCheckpoints`, `queueCheckpointCreate`, `queueCheckpointRestore`, `queueCheckpointDelete`를 추가한다.

- [x] **Step 3: detail panel render 확장**

VM detail panel에 checkpoint form과 checkpoint rows를 렌더링한다. restore/delete는 `confirm()`을 사용한다.

- [x] **Step 4: event binding 확장**

VM detail panel click/submit handler에서 checkpoint create/refresh/restore/delete를 처리하고, jobs section의 `Clear history` button을 처리한다.

- [x] **Step 5: focused Web green 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

Expected: Web static suite와 syntax check가 통과한다.

Observed green: Web static suite 11 passed, 0 failed; `node --check spikes/purecvisor-desktop-node/web/app.js` exit 0.

## Task 3: CLI token-file implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`

- [x] **Step 1: usage 갱신**

usage에 `[--token-file PATH]`를 추가한다.

- [x] **Step 2: token file resolver 추가**

`Resolve-PcvCliTokenFile`을 추가해 missing/empty file을 구조화된 parser error로 반환한다.

- [x] **Step 3: global parser 확장**

`ConvertFrom-PcvCliArguments`에서 `--token-file`을 파싱하고 `--token`과 conflict를 거부한다.

- [x] **Step 4: focused CLI green 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --help
```

Expected: CLI suite와 help smoke가 통과한다.

Observed green: CLI suite 11 passed, 0 failed; `Invoke-PcvDesktopCli.ps1 --help` exit 0.

## Task 4: documentation and verification

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `spikes/purecvisor-desktop-node/cli/README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `AGENTS.md`
- Modify: `follower.md`
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase10-web-cli-productization.md`

- [x] **Step 1: README 갱신**

Phase 10 status, checkpoint UI, browser job history, CLI `--token-file`, 검증 기대값을 기록한다.

- [x] **Step 2: 상위 문서 갱신**

Phase 10 설계/계획 링크와 Desktop Node spike 경계를 추가한다.

- [x] **Step 3: follower 갱신**

Web Console/CLI 제품화 후속을 완료 항목으로 옮기고 다음 P1을 P0로 승격한다.

- [x] **Step 4: 전체 기본 검증 실행**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --help
git diff --check
```

## 완료 증거

2026-04-25 검증 결과:

- Local API: 85 passed, 0 failed
- Service packaging: 12 passed, 0 failed
- CLI: 11 passed, 0 failed
- Web Console static suite: 11 passed, 0 failed
- Web JavaScript syntax: `node --check spikes/purecvisor-desktop-node/web/app.js` exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- Service smoke: `PrepareTokenFile -WhatIf`, `Config`, `Install -WhatIf` exit 0
- CLI smoke: `Invoke-PcvDesktopCli.ps1 --help` exit 0
- `git diff --check`: exit 0, CRLF 변환 경고만 출력
