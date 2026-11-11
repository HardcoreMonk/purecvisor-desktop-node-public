# BL-0048·BL-0049 설치본 흐름 원인 수정 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train `0.43.5` probe에서 드러난 두 결함을 고친다. 대시보드 첫 로드(BL-0048)와 브라우저 콘솔 입력 요청 시간(BL-0049)이 설치본 흐름에서 실제로 좋아지게 한다. 이번에는 가짜 지연 시험만으로 끝내지 않는다. 설치본에서 읽기만 해서 얻은 실제 호출 순서와 시간을 근거로 고치고, 그 모양을 시험으로 옮긴다. train 계정 백업을 지운다.

**Architecture:** 근거는 `docs/ga-ready/evidence/lane2-dashboard-console-probe-2026-11-11-04305.md`와 backlog BL-0048·BL-0049다. 설치본은 승격하지 않은 `0.43.5` fullgate build이고 #152·#153 코드가 들어 있다. branch는 `lane1/bl0048-bl0049-fix-20261111` 하나다. 조사는 Claude가, 구현은 grok(Herdr `grok-status`, 별도 worktree)이 하고 Claude가 리뷰한다.

**Tech Stack:** Playwright(설치본 Web Console 읽기, CDP 요청 initiator), PowerShell CIM/WMI 읽기 시간 측정, TypeScript Web Console, C# HyperV adapter, `gh`

## 사용자 결정 (2026-11-11)

승인 원문: `"1,2 counts , 3"` (`train-04305-20261111` 마무리 보고의 다음 승인 1~3에 대한 답). 각 번호의 문장은 아래와 같다.

| 항목 | 범위 |
| --- | --- |
| 1 | "(권장) 설치본 개선 실패 원인 수정: Lane 1 개발(BL-0049 콘솔 입력 요청 시간: adapter의 키마다 GetVms 조회와 입력 경로 측정, BL-0048 대시보드 첫 로드: 설치본 흐름의 ops/summary 취소와 동시 요청 측정), host mutation 없음, grok 구현. 확인은 설치본과 같은 흐름의 측정 시험으로 한다. push, PR, green CI 뒤 merge. 그 뒤 release train 출발은 따로 승인받는다." dev probe는 답에 없어 하지 않는다(Lane 2 없음). Task 2~5 |
| 2 | "backlog 분류: BL-0048(대시보드 첫 로드 개선이 설치본에서 나타나지 않음)과 BL-0049(콘솔 입력 요청 시간 개선이 설치본에서 나타나지 않음)를 counts 또는 out-of-scope로 정한다." 답 `counts`: BL-0048·BL-0049를 `counts`로 분류한다. Task 1 |
| 3 | "train 계정 백업 삭제: train 0.43.5 동안 저장소 밖 보호 위치에 둔 백업 accounts.json, fullgate가 만든 빈 계정 저장소, ACL 기록을 지운다. data root의 계정 파일은 그대로 둔다. 지우기 전에 data root 계정 파일과 백업의 SHA-256이 같은지 확인한다. 기록은 다음 문서 PR에 남긴다." Task 1 |
| 읽기 | 설치본 Web Console을 브라우저로 여는 것과 WMI 읽기 조회는 host mutation이 아니다. VM에 키를 보내거나 VM·service·설치본을 바꾸지 않는다 |
| merge | 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`(`merge_policy=after-green-ci`). merge 뒤 main push run을 확인한다 |

## Global Constraints

- host mutation 없음. VM에 키를 보내지 않고 설치본, service, 계정, 방화벽을 바꾸지 않는다. 예외는 승인 3의 저장소 밖 백업 파일 삭제다.
- 보존 VM 둘과 `pcv-it-s1r-ubuntu`(Running, 설치 프로그램 화면)를 바꾸지 않는다.
- token, 비밀번호, LAN 주소, 호스트명, 사용자 홈 경로를 문서에 적지 않는다.
- 새 route, 새 권한, 새 `Add-Type`/P/Invoke/native ACL, 새 packaging `*.Tests.ps1`을 만들지 않는다. PowerShell helper fallback을 더하지 않는다.
- Web 규칙: `npm run build:served --prefix web`, i18n 키, axe violation 0, `--prune` 없음.
- 제품 payload를 바꾸는 PR은 `release-train.json` queue에 행을 더한다. train 출발은 따로 승인받는다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 계정 백업 삭제와 분류 (승인 2·3)

- [x] 보호 위치의 `pcv-train-accounts-backup-04305`를 지운다. 지우기 전에 data root 계정 파일과 백업 `accounts.json`의 SHA-256, 크기, ACL 표기가 같은지 본다. 다르면 지우지 않는다. 지운 뒤 data root 계정 파일이 그대로인지 본다.
  - backlog BL-0048·BL-0049를 `counts`로 분류한다(근거: 사용자 결정 `counts`).
  - 로컬 commit.

실행 기록(2026-11-11 17:13 KST):
- 지우기 전: data root 계정 파일과 백업 `accounts.json`의 SHA-256이 같았다(1,421 bytes). data root 계정 파일의 ACL도 백업의 ACL 기록과 같았다.
- 백업 `accounts.json`, fullgate가 만든 빈 저장소(174 bytes), ACL 기록 파일을 지웠다. 비게 된 백업 폴더도 지웠다.
- 지운 뒤: data root 계정 파일의 SHA-256과 ACL이 그대로다. service Running/Automatic, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`다.
- backlog BL-0048·BL-0049를 `counts`로 분류했다(상태 `open`).

## Task 2: 설치본 흐름 조사 (Claude, 읽기만)

- [x] 대시보드(BL-0048)
  - 설치본 Web Console 첫 방문을 Playwright와 CDP `Network.requestWillBeSent` initiator 스택으로 기록한다.
  - `ops/summary`, `host/status`, `vms`, `auth/loopback-session` 등 요청마다 누가(어느 함수가) 불렀는지, 언제 시작·끝났는지, 어느 것이 취소됐는지 적는다. 계정 모드라 loopback 세션은 `409`인 설치본 상태 그대로다. 시나리오 계정 로그인 흐름도 본다.
  - 각 Local API 요청의 서버 처리 시간도 같은 시간선에 놓는다. 동시 요청이 서로를 늦추는지 본다.
- [x] 콘솔 입력(BL-0049): 키 하나의 서버 경로에서 읽기 단계의 시간을 잰다.
  - 대상: `root\virtualization\v2` 연결(`CreateScope`), VM 찾기(`FindVm`), `Msvm_Keyboard` 관계 찾기, `GetVms()` 전체 목록, `Msvm_Keyboard` path로 객체 열기.
  - `InvokeMethod`(키 보내기)는 host mutation이라 재지 않는다. 비슷한 읽기 비용으로 추정한다.
  - 서버 입력 감사 기록 쓰기 비용(파일 append)도 본다.
- [x] 결과를 이 plan에 쓰고 원인 후보를 순서대로 정한다. 로컬 commit.

실행 기록(2026-11-11 17:15~17:30 KST, 설치본 `0.43.5-admin-smoke+8173777`, 읽기만):
- 대시보드 첫 로드 trace(계정 모드, 시나리오 계정 로그인, CDP initiator 스택; `artifacts/bl0048-trace-20261111/run1/trace.json`)
  - 페이지 로드 직후 loopback 세션 요청이 두 경로에서 두 번씩 나갔다(0ms·340ms).
    - `init` → `refreshAll` → `runRefreshAll` → `pcvEnsureLoopbackSession`
    - `pcvBootstrap` → `restoreSession` → `ensureLoopbackSession`(`api.ts`)
    - 계정 모드라 모두 `409`다.
  - 로그인(1.34초) 뒤 대시보드 단계 세 요청을 동시에 보냈다.
    - `ops/summary` 2.6초, `host/status` 3.8초, `vms`와 `runtime/policy` 4.6초. 각 요청의 서버 처리 시간이 응답 시간과 같다.
    - 따로 부르면 `ops/summary` 약 1.8초, `host status` 약 1.6초, `vm list` 약 0.65초(pcvcli, 시작 시간 포함)다. 결국 합만큼 걸린다.
  - 두 번째 묶음 일곱 요청은 모두 약 877ms 뒤 함께 끝났다.
  - 계정 모드에서는 취소가 없고 `ops/summary`가 한 번이다.
- 원인 1(서버, BL-0048·BL-0049 공통): `DesktopNodeApiRequestProcessor.Handle`이 모든 API 요청을 전역 `lock (sync)` 안에서 처리한다. 서버는 요청을 한 번에 하나만 처리한다.
  - 그래서 #152의 동시 요청은 효과가 없다.
  - 취소된 요청도 줄에서 끝까지 처리돼 뒤 요청을 늦춘다.
  - 콘솔 입력 요청은 계속 도는 frame 요청(장당 약 0.85초)과 다른 폴링 뒤에서 기다린다.
- 원인 2(Web, BL-0048 loopback 모드): loopback 세션을 만드는 두 경로가 서로 다른 token을 만든다. #152 리뷰 수정의 "인증이 바뀌면 새로 시작" 규칙이 앞 갱신을 취소한다. train `0.43.5` probe(계정 보관 중, loopback 모드)의 `ops/summary` 3회·취소 2회와 같은 모양이다.
- 콘솔 입력 경로 WMI 읽기 시간(PowerShell CIM과 System.Management, 각 3회)
  - 연결 5~33ms, VM 찾기 7~21ms, 전체 VM 목록 5~12ms
  - `Msvm_Keyboard` 관계 찾기 약 210ms. #153이 이미 캐시한다.
  - WMI 읽기는 2.4초의 작은 부분이다. 대부분은 전역 lock 대기로 본다.
- 원인 3(작음): adapter가 키마다 `GetVms()`(전체 VM 요약)를 부른다. 설치본 `vm list`는 약 0.65초다. 키 하나에 VM 하나의 상태만 보면 된다.
- 고칠 순서: (1) 콘솔 frame·input route를 전역 lock 밖에서 처리 (2) loopback 세션 만들기를 한 경로로 합치기 (3) 키마다 `GetVms()` 대신 VM 하나 상태 조회

## Task 3: 수정 (grok 구현, Claude 리뷰)

- [x] Task 2의 원인에 맞춰 고친다.
  - 대시보드: 설치본에서 본 실제 호출 순서를 node 시험으로 재현한다(같은 순서, 같은 취소). 고친 뒤 그 시험에서 `ops/summary`가 한 번이고 화면이 더 빨리 차야 한다.
  - 콘솔 입력: 키당 반복되는 비용(연결, VM 목록 등)을 없앤다. 단위 시험으로 키 N개에 비싼 단계가 몇 번 일어나는지 센다.
  - 제품 payload가 바뀌므로 `release-train.json` queue에 행을 더한다.
  - 검증: 해당 C# 시험(`-c Release`), `npm run test:required --prefix web`, web Pester, `git diff --check`. 로컬 commit.

실행 기록(2026-11-11 17:18~17:50 KST): grok이 구현했고 Claude가 리뷰했다. commit `39976fd`.
- 원인 1(전역 lock)
  - 콘솔 frame(`GetVmConsoleFrame`)과 input(`SendVmConsoleInput`)은 rate limit(`throttle.Enforce`)만 전역 `sync` 안에서 하고, 인증과 route 처리는 밖에서 한다(`DesktopNodeApiRequestProcessor.ConsoleLock.cs`). 다른 route는 그대로 lock 안이다.
  - frame 간격과 input 창 사전은 `TryAdd`/`TryUpdate`로 갱신한다. 감사 기록은 기존 lock 그대로다.
  - 리뷰: 우회 경로가 원래 경로와 같은 검사를 하는 것을 확인했다. method, 인증 `TryHandle`/`Authorize`, 요청 id, job store 예외 처리다. frame 최소 간격 100ms는 그대로다.
- 원인 2(loopback 세션 중복): `api.ts`의 공유 진행 중 요청(`_sharedLoopbackPayload`)을 `desktop-api.ts` `createLoopbackSession`도 쓴다. 한 페이지에서 POST는 한 번이고 token은 하나다.
- 원인 3: 콘솔 입력은 `FindVmRunState`(VM 하나 찾기와 `EnabledState`, template 잠금은 그 VM `Notes`)로 본다. 키마다 전체 VM 요약을 읽지 않는다. frame은 여전히 `GetVms`다(report-only).
- 남는 위험(report-only)
  - 콘솔 인증이 전역 lock 밖에서 계정 저장소를 읽는다. 계정 변경은 lock을 그대로 잡는다.
  - 다른 route가 lock을 오래 잡으면 콘솔 요청은 rate limit 확인에서만 기다린다.
- 결과
  - Api 549/549(콘솔 lock 우회 4건: frame 중 input 200, input이 lock을 잡지 않음, VM별 순서, rate limit 그대로)
  - HyperV 293/293
  - `npm run test:required --prefix web` exit 0(node 430/430, browser gate `ok=true` violations 0, 기준선 변화 없음)
  - web Pester 50/50, `git diff --check` clean
  - module 줄 상한 안이다.
- 제품 payload가 바뀌어 `release-train.json` queue에 PR #157 행을 더했다(queue 3행).

## Task 4: PR

- [ ] clean HEAD 종료 검증(PR gate는 원래 checkout에서 detached로), push, PR, green CI 뒤 merge, main push run 확인.

## Task 5: 마무리

- [ ] campaign을 닫는다. 다음 승인(train 출발과 dev probe 여부)을 `next_approval_required`에 적는다. 이 기록은 Task 4 PR에 함께 담는다.

## Nonclaims

- operational current는 `0.43.4-admin-smoke` 그대로다. 설치본은 승격하지 않은 `0.43.5` fullgate build다.
- 이 campaign은 설치본에서 고친 결과를 확인하지 않는다(dev probe 없음). 확인은 다음 train probe에서 한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
