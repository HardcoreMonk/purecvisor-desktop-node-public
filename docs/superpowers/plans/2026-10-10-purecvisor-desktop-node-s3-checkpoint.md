# S3 checkpoint browser Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** S3(checkpoint 생성·복원과 예약 실행을 브라우저에서 한다)를 이 호스트 설치본 `0.42.93-admin-smoke`에서 시연하고 criteria S3를 `passed`로 바꾼다. 캡처는 `docs/ga-ready/demo/s3-checkpoint-20261010/`에 보존하고 시연 기록 템플릿의 확인자 칸은 사용자가 채운다.

**Architecture:** 제품 route `POST /api/v1/vms/{id}/checkpoints`, `POST /api/v1/vms/{id}/checkpoints/{cp}/restore`, `DELETE /api/v1/vms/{id}/checkpoints/{cp}`, `POST /api/v1/vms/{id}/checkpoints/schedule`(`/preview`, `/clear`)와 worker `ProcessDueCheckpointSchedules`(250 ms tick, `DesktopNodeCheckpointScheduleStore`, 2026-09-21 `7e24c75`)가 이미 있고 `0.42.93-admin-smoke` source(`56e7cd0`)에 들어 있다. Web Console VM 상세에는 checkpoint 폼과 schedule readback(enabled, interval minutes, retention, last enqueued, next due)이 있다. 스크립트는 S2 `run-s2-clone-scenario.mjs` 형식(plan-only 기본, `--execute`는 Web Console이 쓰는 Local API만)이고, 브라우저 시연은 Playwright로 Web Console 폼을 눌러 한다. branch는 `lane1/s3-checkpoint-20261010`이다.

**Tech Stack:** Node.js(`npm run scenario:s3-checkpoint --prefix web`, `node --test`), Playwright(브라우저 캡처), PowerShell 7 + Pester 5, git/gh

## 사용자 결정 (2026-10-10)

승인 원문: `1,2,3,4` (2026-10-10 audit-green-lean-20261009 Task 7 FAIL 보고의 `next_approval_required`에 대한 답. 이 campaign은 그중 2 "S3 campaign: checkpoint 생성·복원·예약을 브라우저에서 시연"이다.)

| 항목 | 범위 |
| --- | --- |
| 1 | S3 시나리오 스크립트(plan-only + `--execute`)를 Lane 1에서 만들고 Lane 2에서 설치본에 1회 실행한다. host mutation은 ADR-0016 standing approval 범위(`pcv-it-` 접두사 VM 생성·설정·삭제)의 `pcv-it-s3-source` 하나뿐이며 checkpoint 생성·복원·예약·삭제는 그 VM의 설정이다. 설치본 Update/Rollback, service, 방화벽은 건드리지 않는다(기능이 `0.42.93`에 있음). |
| 2 | branch push와 PR을 연다. merge는 승인 문장에 없어 `next_approval_required`로 둔다. |
| 이관 | `audit-green-lean-20261009` Task 9(C5 runner 확인, `not_before` 2026-10-19)를 Task 4로 옮긴다. 문장과 push, PR, green CI 뒤 merge는 원래 승인 그대로다. |
| queue | PR #78(merge `1b04ff46`)의 release-train queue 행은 이 branch 첫 commit에서 더한다(Delivery 계약이 merge SHA를 요구). |
| 다음 | S4 campaign(승인 3)은 이 campaign이 닫힌 뒤 연다. |

## 사용자 결정 (2026-10-10, 2차)

승인 원문: `1. merge, 2. merge, 3,4 , 계속` (2026-10-10 Task 2 deadline-wait 보고의 `next_approval_required` 1~4에 대한 답)

| 항목 | 범위 |
| --- | --- |
| 1 | S3 PR `lane1/s3-checkpoint-20261010`는 green CI 뒤 merge한다(`merge_policy=after-green-ci`). |
| 2 | private PR #190(archive 선언)을 red CI 상태로 merge한다. 2026-10-10 04:39Z merge commit `50dffbc9`(Lane 0, red CI는 사용자 결정으로 수용). |
| 3 | BL-0016 Lane 1 수정을 Task 2b로 Task 2 뒤에 끼워 넣는다. push·PR은 이 campaign branch, merge는 1번과 같이 green CI 뒤다. |
| 4 | S3 판정 보류. Task 3는 시연 기록(FAIL 원인 포함)·backlog·스크립트 push·PR로 바꾸고 criteria S3는 `open` 그대로 둔다. 브라우저 예약 저장 재시연은 S4 campaign의 train(0.42.94, BL-0016 포함) 뒤 S4 campaign 안 task다. |
| 계속 | Task 2의 남은 단계(2026-10-10 14:26 Asia/Seoul 이후)와 큐를 이어 돈다. 기다리는 동안 Task 2b를 먼저 돈다. |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`, template VM `pcv-it-s2-source`, `current-evidence.json`, `release-train.json`의 `trains`를 바꾸지 않는다. 시연 VM은 `pcv-it-s3-source` 하나이고 끝에 checkpoint와 함께 지운다. 끝 상태 `pcv-it-s3-*` VM 0개.
- 캡처와 기록에 사용자 홈 경로, LAN 사설 IP, 호스트명, token을 적지 않는다(AGENTS.md 저장소 경계). 캡처는 PNG 500 KB 이하.
- 시연 기록은 `docs/ga-ready/demo/TEMPLATE.md` 형식이다. 확인자·확인 일시·확인 방법은 사용자가 채우며, 비어 있는 동안 기록은 에이전트 자가 보고다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다). Lane 3는 열지 않는다.

## Task 1: S3 시나리오 스크립트

- [x] `web/scripts/run-s3-checkpoint-scenario.mjs`와 `npm run scenario:s3-checkpoint`를 추가한다. 기본 plan-only는 HTTP 없이 단계만 출력한다. `--execute`는 loopback session → `pcv-it-s3-source` 생성(managed Generation 2, cpu 1, memory 512 MB, disk 8 GB, DVD는 기존 smoke ISO, Off) → checkpoint `s3-cp1` 생성 → 목록 1개 확인 → `s3-cp1` 복원 → schedule 설정(`interval_minutes`는 계약 최소값, `retention_max` 2) → `next_due_at` 뒤 두 번째 checkpoint가 생기는지 bounded(interval + 90초) 확인 → schedule clear → checkpoint 삭제 → VM 삭제 순서로 Local API만 부르고 `artifacts/s3-checkpoint-20261010/summary.json`에 단계별 job 결과와 elapsed를 적는다. token은 메모리에만 둔다. `node --test web/node-tests/s3-checkpoint-scenario.test.mjs`(plan-only 출력, 인자 기본값)와 `node --check`로 확인한다. 로컬 commit.

실행 기록(2026-10-10): `CheckpointSchedulePolicy`는 interval 60~10080분, retention 1~32, due create는 현재 checkpoint 수가 retention 미만이고 여유 공간 10 GiB 이상일 때만 허용한다. `last_enqueued_at`이 없는 schedule은 enable 즉시 due이므로 스크립트는 interval 60(최소값), retention 2로 설정하고 첫 예약 checkpoint가 120초 안에 생기는지 본다(`--auto-bound-ms`로 조정). 그 뒤 `next_due_at`은 한 interval 뒤로 밀리고 count 2 ≥ retention이라 다음 due는 capacity로 거절된다. 단계: session, create, checkpoint-create(`{name}`), checkpoint-list, checkpoint-restore, schedule-preview, schedule-set(`{interval_minutes, retention_max}`), auto-checkpoint, schedule-clear, delete-vm(finally에서 schedule clear와 VM 삭제, `vm_remaining` 확인). 기본 경로는 저장소 `artifacts/` 아래이고 ISO는 `artifacts/smoke-media-20261003/pcv-route-parity-smoke-20261003.iso`라 사용자 경로가 없다. 검증: `node --check` 통과, `node --test` 2/2, `npm test` 통과, web Pester 50/50, `git diff --check` 통과. `npm run test:public-source-safety`는 이 변경과 무관한 S1 문서 2건(`network.observed-private-endpoint`)만 남았고 backlog BL-0015로 적었다. 감사 plan의 private PR URL 한 곳이 `provider.private-archive`에 걸려 지웠다.

## Task 2: S3 설치본 시연 (Lane 2)

- [ ] 설치본 version·service·Web 상태를 읽고 `pcv-it-s3-*` VM이 없음을 확인한 뒤 `run-s3-checkpoint-scenario.mjs --execute`를 1회 돌린다. 같은 VM에 대해 Playwright로 Web Console(`http://127.0.0.1/`) VM 상세를 열어 checkpoint 폼으로 생성·복원·예약을 한 번씩 수행하고 checkpoint 목록과 schedule readback(enabled, next due, last enqueued) 화면을 PNG로 캡처해 `docs/ga-ready/demo/s3-checkpoint-20261010/`에 둔다. 끝에 `pcv-it-s3-source`와 checkpoint를 지우고 보존 VM Off, template Off를 확인한다. 실패하면 원인을 기록하고 멈춘다. 로컬 commit.

실행 기록(2026-10-10, 중단·deadline-wait): 설치본 `0.42.93-admin-smoke`(service 실행, Web `http://127.0.0.1/` 200), 시작 시 `pcv-it-s3-*` VM 0개. 스크립트 `--execute` 첫 판은 "schedule이 enable 즉시 due"라는 가정이 틀려 auto-checkpoint 단계가 bound 안에 끝나지 않았다. `checkpoint.schedule.set`은 Apply에서 `last_enqueued_at=UtcNow`를 찍으므로 첫 예약 실행은 한 interval(최소 60분) 뒤다. 스크립트를 `--skip-auto`(set까지, VM 보존)·`--verify-only --expect-count=N`(목록·readback 확인)·`--cleanup-only`로 나누고 기본 bound를 interval+120초로 바꿨다(`node --test` 2/2). 브라우저(Playwright, Web Console VM 상세 `pcv-it-s3-source`): checkpoint 생성 `s3-ui-cp`, 복원 `s3-cp1`, schedule clear는 폼 클릭으로 성공했고(job succeeded) 캡처 01~04다. **FAIL(설치본 결함, BL-0016)**: `Preview schedule`/`Save schedule` 클릭은 4회 모두 "Form submission canceled because the form is not connected"로 취소되어 요청이 나가지 않고 입력값도 지워졌다. 원인(페이지 안 계측으로 확인): VM 상세 click 위임 핸들러(설치본 `app.js:5572`, source `web/src/served-app.ts` `els.vmDetailPanel` click)가 `button[data-action]` 전부에 대해 끝에서 `render()`를 불러 click 처리 중 form이 detach되고, 브라우저가 submit 알고리즘을 취소해 submit 핸들러(`queueCheckpointScheduleControl`)가 돌지 않는다. `type="submit"` + `data-action` 버튼 13종이 같은 패턴이고 `vm-clone`만 early return 예외다. 같은 폼에 `form.requestSubmit(Save)`로 submit 핸들러에 직접 들어가면 confirm → `POST /api/v1/vms/pcv-it-s3-source/checkpoints/schedule` → job succeeded → readback `enabled / interval 60 / retention 3 / last enqueued 2026-10-10T04:24:15Z / next due 2026-10-10T05:24:15Z`(캡처 05). 중간에 브라우저 loopback session이 만료되어 403 `PCV_AUTH_FORBIDDEN`이 났고 stale token을 비운 뒤 세션을 다시 만들어 진행했다(stale token이 남아 있으면 `ensureLoopbackSession`이 세션을 다시 만들지 않는 점은 report-only). 남은 단계(2026-10-10 14:26 Asia/Seoul 이후): `--execute --verify-only --expect-count=3`로 예약 checkpoint 1개 추가 확인·캡처, UI로 schedule clear, `--execute --cleanup-only`, 끝 상태 확인(`pcv-it-s3-*` 0, 보존 VM Off, template Off). 현재 host 상태: `pcv-it-s3-source` Off, checkpoint 2(`s3-cp1`, `s3-ui-cp`), schedule enabled(retention 3이라 예약 checkpoint는 최대 1개 더 생기고 그 뒤 capacity로 막힘), `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off. 판정: 브라우저 예약 저장 경로가 설치본 결함으로 막혀 S3는 `0.42.93`에서 통과할 수 없다. 수정은 Lane 1(BL-0016), 설치본 반영은 train이 필요하다.

## Task 2b: BL-0016 수정 (Lane 1, 2026-10-10 2차 결정 3)

- [x] `web/src/served-app.ts`의 `els.vmDetailPanel` click 위임 핸들러가 form 안 `type="submit"` 버튼이면 아무것도 하지 않고 돌아가게 고쳐(그 폼의 submit 핸들러가 처리) 재렌더로 submit이 취소되지 않게 한다. `vm-clone`의 기존 early return은 그대로 둔다. `npm run build:served --prefix web`으로 `web/app.js`를 다시 만들고, 회귀 테스트(`web/node-tests`의 정적 검사: 핸들러에 guard가 있고 `web/app.js`에도 들어 있음)를 더해 CI가 돌리는 npm 스크립트에 연결한다. 검증: `npm test --prefix web`, `npm run test:web-contracts --prefix web`, `Invoke-Pester web/tests`, `git diff --check`. 로컬 commit. product payload(`web/app.js`)가 바뀌므로 release-train queue 행은 merge 뒤 merge SHA로 더한다.

실행 기록(2026-10-10): `web/src/served-app.ts` `els.vmDetailPanel` click 핸들러 머리에 `if (button.type === 'submit' && button.form) return;` guard(BL-0016 주석)를 넣었다. `vm-clone` early return은 그대로다. `npm run build:served`로 `web/app.js`를 다시 만들었고(guard는 bundle 5740행, +5줄) `check:served`·static parity·browser fixture가 current다. 회귀 테스트 `web/node-tests/vm-detail-submit-guard.test.mjs` 3건(source의 click 핸들러 머리에 guard가 `state.error = null`보다 앞에 있음, `web/app.js`에 같은 guard가 있음, schedule Preview/Save가 `type="submit"` + `data-action`임)을 `test:web-contracts`에 붙이고 그 명령을 pin한 `web-verification-architecture-boundary.test.mjs`의 `EXPECTED_WEB_CONTRACTS_SCRIPT`를 같이 갱신했다. 검증: 새 테스트 3/3, `npm test` 통과, `test:web-contracts` 239/239, `verify:parity` 통과, web Pester 50/50, `git diff --check` 통과. host mutation 없음. BL-0016 행은 PR merge 뒤 `closed_by`에 merge SHA를 적는다. release-train queue 행도 merge 뒤다.

## Task 3: 시연 기록과 criteria, push·PR

- [ ] (2026-10-10 2차 결정 4로 변경) `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-10.md`를 TEMPLATE.md 형식으로 쓴다(확인자 칸은 비움, 판정은 "브라우저 생성·복원·clear 통과, 예약 저장 FAIL(BL-0016), 예약 실행은 submit 핸들러 직접 호출로 설정해 확인"). `config/project-completion-criteria.json` S3는 `open` 그대로 두고 `docs/DOCUMENTATION_INDEX.md`에 기록을 더한다. clean HEAD에서 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `npm test --prefix web`, `Invoke-Pester packaging/windows-desktop-node/tests`, `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check`. push, PR, green CI 뒤 merge(2차 결정 1). merge 뒤 release-train queue 행(Task 2b의 `web/app.js`)을 merge SHA로 더한다.

## Task 4: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `audit-green-lean-20261009` Task 9(그 전 `s2-template-clone-20261009` Task 2, `s1-installed-20261008` Task 11, `adr17-s1-console-20261008` Task 14)다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- S3 통과는 Task 2 시연과 스크립트 결과로만 주장하고, S4는 주장하지 않는다. 확인자 칸이 비어 있는 동안 기록은 자가 보고다.
- operational current는 `0.42.93-admin-smoke` 그대로다. Lane 3와 `current-evidence.json` 쓰기는 없다.
- public trusted signing과 external stable publication을 주장하지 않는다.
