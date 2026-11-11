# 2026-11-11 제로베이스 감사 후속 조치 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-11-11 제로베이스 감사의 후속 조치 네 가지를 한 campaign으로 끝낸다. 가장 중요한 것은 현행 설치본 `0.43.4-admin-smoke`의 Web Console로 S1~S3를 실제 VM에서 다시 보이는 것이다. Ubuntu를 설치하고, 설치된 VM을 template으로 복제하고, checkpoint 생성·복원·예약을 브라우저에서 한다. 그 흐름은 다시 돌릴 수 있는 브라우저 시나리오 스크립트로 남긴다. 그 밖에 train 계정 백업 삭제, train 묶음 출발 기준, 대시보드 첫 로드 시간 수정을 한다.

**Architecture:** 근거는 `docs/project-status-audit-2026-11-11-r2.md` §5와 campaign `train-04304-20261111`의 `next_approval_required` 1~4다. PR은 네 개다.
- PR A: 백업 삭제 기록과 train 출발 기준(문서)
- PR B: 브라우저 시나리오 스크립트(`web/scripts`)
- PR C: 설치본 재시연 기록
- PR D: 대시보드 첫 로드 수정과 campaign 마무리

순서는 감사 권장안(S1~S3 재증명)을 앞에 둔다. 구현은 grok(Herdr `grok-status`)이 하고, Claude가 설계·리뷰·Lane 2 실행을 한다.

**Tech Stack:** Playwright(Node), TypeScript Web Console, C# Local API, `pcvcli`, Hyper-V(ADR-0016 `pcv-it-` VM), `gh`

## 사용자 결정 (2026-11-11)

승인 원문: `"1,2,3,4"` (`train-04304-20261111` 마무리 보고의 다음 승인 1~4에 대한 답). 각 번호의 문장은 아래와 같다.

| 항목 | 범위 |
| --- | --- |
| 1 | "train 계정 백업 삭제: train `0.43.4` 동안 저장소 밖 보호 위치에 둔 백업 `accounts.json`, fullgate가 만든 빈 계정 저장소, ACL 기록을 지운다. data root의 계정 파일은 그대로 둔다. 지우기 전에 data root 계정 파일과 백업의 SHA-256이 같은지 확인한다. 기록은 문서 PR 하나로 남기고 Required CI green 뒤 merge한다." Task 1, 3 |
| 2 | "(권장) 현행 설치본 S1~S3 브라우저 재증명: Lane 1 개발(브라우저 시나리오 스크립트 web/scripts), host mutation 없음. push, PR, green CI 뒤 merge. 그 뒤 Lane 2 설치본 현행 버전에서 브라우저 시나리오 실행과 S1 Ubuntu 설치·S2 설치된 VM 복제 재시연(ADR-0016 pcv-it- VM 생성·시작·checkpoint·복제·삭제, 설치본·service·MSI 변경 없음). 시연 기록 PR, green CI 뒤 merge." Task 4~11 |
| 3 | "release train 묶음 출발 기준: Lane 1 문서·정책 변경(DEVELOPMENT_PROCEDURE §10 train 출발 기준 명확화, 필요한 spec pin 갱신), host mutation 없음. push, PR, green CI 뒤 merge." Task 2, 3 |
| 4 | "대시보드 첫 로드 시간: Lane 1 개발(ops/summary 응답 시간과 Web 첫 갱신 취소 경로, 지연 mock 시험), host mutation 없음. push, PR, green CI 뒤 merge. 설치본 반영은 다음 train 출발 승인으로." Task 12, 13 |
| merge | 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`(`merge_policy=after-green-ci`). merge 뒤 main push run을 확인한다 |
| 계정 | 사용자 결정 `"임시 operator 계정 (권장)"`(2026-11-11, 로그인 방법 질문에 대한 답)으로 연다. 처음에는 권장안 자동 승인으로 열려 했으나 auto mode가 승인 2 문장 밖이라며 막아 사용자에게 물었다. 설치본은 계정 모드라 loopback 세션이 `409`다. 브라우저 시나리오는 이번에만 쓸 operator 계정 `pcv-s13-operator`로 로그인한다. 계정은 `pcvcli account create --password-env`(service bearer, protected token file)로 만들고, 끝나면 `pcvcli account disable`로 끈다. 비밀번호는 실행 경계에서 만들어 환경 변수로만 넘긴다. 실행 사이에는 저장소 밖 DPAPI 보호 임시 파일에만 두고, 계정을 끈 뒤 지운다. 기존 계정 세 개(`pcv-admin`, `pcv-admin2`, `pcv-s4-operator`)는 바꾸지 않는다. 계정 파일을 옮기거나 service를 다시 시작하지 않는다 |
| Task 7 재개 | Task 6~7 중단 보고마다 사용자가 다음 승인 1번에 `"1"`로 답했다(2026-11-11, 세 번). 첫째는 새 예산(45분·12회)으로 키마다 화면 변화를 확인하며 설치를 마친다, 둘째는 글자마다 키 이벤트로 사용자 정보를 다시 넣는다, 셋째는 frame 읽기 속도를 따로 재고 640×480으로 낮춰 마친다(화면이 멈추면 멈춤)였다 |
| S1 중단 | 네 번째 중단 보고의 다음 승인 1에 대한 답 `"1"`(2026-11-11): S1 브라우저 설치를 멈추고 콘솔 입력 결함(BL-0046·BL-0047)부터 Lane 1로 고친다(host mutation 없음, grok 구현). 시나리오 CDP 도우미 결함도 함께 고치고, Task 12(대시보드)도 이 campaign에서 PR로 merge한다. `pcv-it-s1r-ubuntu`는 그대로 두고 S1~S3 재시연은 고친 버전이 train으로 설치된 뒤 한다. 그래서 Task 8~10은 다음 승인으로 넘긴다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM의 생성·구성·삭제로 한정한다. 이번 VM은 `pcv-it-s1r-*`, `pcv-it-s2r-*`이고 disk는 Local API가 정하는 기본 위치에 둔다 |

## Global Constraints

- 보존 대상: VM `pcv-guest-installed-04253-r1`, `pcv-it-s2-source`, BL-0028 parent VHD를 바꾸지 않는다. 설치본 `0.43.4-admin-smoke+7b929ec`, service, MSI, 방화벽, listener를 바꾸지 않는다. `current-evidence.json`을 쓰지 않는다.
- 비밀 값:
  - 계정 비밀번호, guest 비밀번호, token은 실행 경계에서 만든다. 명령줄, summary, evidence, 캡처에 남기지 않는다.
  - 장기 token은 `--protected-token-file`로만 쓴다.
  - 문서에는 사용자 홈 경로, LAN 사설 IP, 호스트명을 적지 않는다. Hyper-V Default Switch NAT 주소는 적어도 된다.
- 브라우저 시나리오 스크립트는 Web Console 화면을 조작한다(Local API 직접 호출이 아니다). `--plan-only`가 기본이고, `--execute`일 때만 요청을 보낸다. 비밀번호는 환경 변수로만 받는다.
- 새 route, 새 권한, 새 `Add-Type`/P/Invoke/native ACL, 새 packaging `*.Tests.ps1`을 만들지 않는다.
- Web 규칙: `web/src`를 고치면 `npm run build:served --prefix web`를 돌리고, 문구는 i18n 키로 쓴다. axe violation 0이고 `--prune`은 쓰지 않는다.
- grok 지시는 약 20분 분량으로 쓰고 마감 시각을 적는다. grok은 commit·push하지 않고 `docs/**`를 고치지 않는다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: train 계정 백업 삭제 (승인 1)

- [x] 보호 위치의 `pcv-train-accounts-backup-04304`를 지운다.
  - 지우기 전: data root 계정 파일과 백업 `accounts.json`의 SHA-256과 크기가 같은지, data root 계정 파일의 ACL이 백업의 ACL 기록과 같은지 본다. 다르면 지우지 않고 멈춘다.
  - 지운 뒤: data root 계정 파일의 SHA-256과 ACL이 그대로인지 본다. service Running/Automatic, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`도 확인한다.
  - 이 plan에 실행 기록을 쓰고 로컬 commit한다.

실행 기록(2026-11-11 07:59 KST):
- 지우기 전: data root 계정 파일과 백업 `accounts.json`의 SHA-256이 같았다(1,153 bytes). data root 계정 파일의 ACL도 백업의 ACL 기록과 같았다.
- 백업 `accounts.json`, fullgate가 만든 빈 저장소, ACL 기록 파일을 지웠다. 비게 된 백업 폴더도 지웠다.
- 지운 뒤: data root 계정 파일의 SHA-256과 ACL이 지우기 전과 같다. service Running/Automatic, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`(계정 모드)다.

## Task 2: train 묶음 출발 기준 (승인 3)

- [x] `docs/DEVELOPMENT_PROCEDURE.md` §10의 출발 기준을 고친다.
  - 0.43.1~0.43.4처럼 표시 수정 하나마다 train을 보내지 않는다.
  - 조기 출발 사유(데이터 손실, 설치·업그레이드 실패, 보안 결함)가 아닌 표시·문구 수정은 queue에 쌓아 둔다.
  - 이런 수정은 기능 변경과 함께 싣거나 queue가 정한 수만큼 모였을 때 출발을 제안한다.
  - 에이전트가 다음 승인으로 train 출발을 올릴 때도 이 기준을 따른다.
  - pin 대상이면 같은 commit에서 `Update-PcvContractSpecPins.ps1 -Apply`를 돌린다.
  - 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-11-11): `docs/DEVELOPMENT_PROCEDURE.md` §10 출발 조건 바로 아래에 "묶음 출발 기준" 항목을 더했다.
- 에이전트는 네 경우에만 train 출발을 제안한다. 조기 출발 사유, 시나리오 단계 종료, 시연할 수 있는 동작을 바꾸는 `queue` 행, `queue` 행 `3`개 이상이다.
- 표시·문구 수정뿐이고 `3`개 미만이면 `queue`에 두고, 마무리 보고에 행 수와 다음 출발 조건을 적는다. 사용자가 직접 요청하면 이 기준과 관계없이 출발한다.
- 이 문서는 spec pin 대상이 아니다(`Update-PcvContractSpecPins.ps1 -Check` current, 바뀐 spec 0).
- 결과: Delivery 785/785, `git diff --check` clean.

## Task 3: PR A (문서)

- [x] clean HEAD에서 Delivery 시험과 `npm run test:public-source-safety --prefix web`를 돌린다. push하고 PR을 연다. Required CI 다섯 개가 green이면 merge하고 main push run을 확인한다.

실행 기록(2026-11-11): clean HEAD `951dc1f`에서 `npm run test:public-source-safety --prefix web` exit 0이었다. Delivery 785/785는 Task 2에서 돌렸다.
- PR #149(`lane1/zero-base-followup-20261111`)를 열었다.
- 첫 CI에서 `dotnet` shard가 Host 브라우저 시험 하나(`DesktopNodeHostLoopbackBootstrapBrowserTests.ChromiumOpensLoopbackConsoleWithoutServiceTokenPaste`)로 실패했다. 이 PR은 문서만 바꿨다. 실패한 job을 한 번 다시 돌리자 pass했다(flake로 본다).
- Required CI 다섯 개가 green인 것을 보고 `--match-head-commit 951dc1f`로 merge했다(merge commit `6b29894`). main push run Development Gates `42920679012`와 Public Boundary `42920679190`이 success다.

## Task 4a: 브라우저 시나리오 스크립트 뼈대 (승인 2, Lane 1)

Task 4는 grok 지시 크기(약 20분) 때문에 4a와 4b로 나눴다(2026-11-11). 범위는 같다.

- [x] `web/scripts/browser-scenario-cdp.mjs`(Edge + CDP 도우미, `verify-browser-ui-gates.mjs`와 같은 방식, 새 npm 의존성 없음)와 `web/scripts/run-browser-scenario.mjs`를 만든다. grok이 구현하고 Claude가 리뷰한다.
  - 인자는 `readScenarioArgs`로 읽는다. 시나리오는 `--scenario s1|s2|s3`로 고른다.
  - 기본은 `--plan-only`다.
  - 로그인: 계정이 있으면 로그인 화면에 `--username`과 환경 변수 `PCV_SCENARIO_PASSWORD`를 넣는다. 계정이 없으면 loopback 세션을 쓴다. token과 비밀번호는 출력하지 않는다.
  - `--check`: 로그인 뒤 화면과 버튼을 찾기만 하고 제출하지 않는다(읽기 전용).
  - `assertScenarioVmName`은 `pcv-it-`로 시작하지 않는 이름이면 쓰기를 막는다.
  - `summary.json`(계약 `pcv-browser-scenario-v1`)과 단계별 캡처를 `artifacts/browser-scenario-<yyyymmdd>/` 아래에 쓴다.
  - node 시험 `web/node-tests/browser-scenario.test.mjs`를 더한다.
  - 검증: `npm run test:required --prefix web`, `git diff --check`. 로컬 commit.

실행 기록(2026-11-11): grok이 08:02~08:19에 구현했고 Claude가 리뷰했다.
- `web/scripts/browser-scenario-cdp.mjs`: Edge + CDP 도우미다. headless가 기본이고 창은 1440×900이다. `fill`은 값을 `Runtime.callFunctionOn` 인자로 넘긴다. 기존 gate 스크립트는 그대로다.
- `web/scripts/run-browser-scenario.mjs`: plan-only 단계 목록, `--check`(로그인, 버전, VM 목록, 만들기 마법사 칸, 상세 전원 버튼, 콘솔 탭, checkpoint 탭, template 표와 복제 폼), `assertScenarioVmName`, 비밀 값을 지우는 `serializeScenarioSummary`가 들어 있다. `--execute`는 아직 exit 2다.
- `web/package.json`에 `scenario:browser`를 더했다. `test:web-contracts`에 새 시험을 넣었고, 그 명령 문자열 pin(`web-verification-architecture-boundary.test.mjs`)도 맞췄다.
- 확인된 화면 구조:
  - 마법사에는 generation 입력과 switch 선택이 없다(Generation 2 고정, switch는 만든 뒤 하드웨어 탭에서 바꾼다).
  - template 잠그기는 VM 행 더보기 메뉴에 있다.
  - 복제·checkpoint 만들기의 시작 버튼에는 `data-action`이 없다.
- 결과: 새 node 시험 8/8. `npm run test:required --prefix web` exit 0이다(`test:web-contracts` 408/408, browser gate `ok=true` assertions 121 failed 0 violations 0, 기준선 변화 없음). 첫 실행은 gate `loginReady` 대기 시간 초과였고 다시 돌리니 통과했다. `git diff --check` clean.
- 리뷰 메모(4b에서 고친다):
  - 셸 준비 판정이 `connectionState === "connected"`만 인정한다. operator는 계정 목록이 `403`이라 `degraded`일 수 있다.
  - 오류 메시지를 버린다. 비밀 값을 지운 뒤 summary에 남겨야 한다.

## Task 4b: 브라우저 시나리오 실행 단계 (승인 2, Lane 1)

- [x] `--execute`를 구현한다(grok 구현, Claude 리뷰). 모든 쓰기 동작은 `assertScenarioVmName`을 거친다.
  - S1: VM 만들기 마법사로 ISO VM을 만들고 시작한다. 콘솔 화면이 비어 있지 않을 때까지 기다리고, 키보드 입력 뒤 화면이 바뀌는지 본다.
  - S2: 꺼진 VM을 template으로 잠그고 복제한다. 복제 job이 끝나는 시간을 잰다(기준 60초).
  - S3: checkpoint를 만들고 복원한다. 예약을 미리 보고 저장한다. `--wait-schedule`이면 첫 예약 실행을 기다린 뒤 예약을 지운다.
  - `--cleanup`: 만든 `pcv-it-` VM을 지운다(template 잠금을 먼저 푼다).
  - 계정이 생기면(Task 6) `--check`를 설치본에 돌려 선택자를 확인한다. 이 확인은 Task 6 기록에 쓴다.
  - 검증: `npm run test:required --prefix web`, `git diff --check`. 로컬 commit.

실행 기록(2026-11-11): grok이 08:25~08:41에 구현했고 Claude가 리뷰하고 고쳤다.
- 계정
  - 08:27 시나리오 계정을 처음 만들려 했을 때 auto mode classifier가 거부했다(`Access Added As A Means`). 승인 2 문장에 계정 생성이 없어서다.
  - 사용자에게 로그인 방법을 물었고 답은 `"임시 operator 계정 (권장)"`이었다. 08:33에 `pcvcli account create`로 `pcv-s13-operator`(operator)를 만들었다(Task 6에서 앞당김).
  - 비밀번호는 실행 경계에서 만들어 저장소 밖 DPAPI 보호 임시 파일에만 둔다. 기존 계정 세 개는 그대로다.
- grok 구현
  - `--execute`: S1(마법사 생성, switch 연결, 시작, 콘솔 frame과 키 입력), S2(끄기, template 잠금, 복제, `clone_seconds`, `--start`), S3(checkpoint 만들기·복원, 예약 미리보기·저장, `--wait-schedule`이면 첫 예약 실행 확인 뒤 지우기), `--cleanup --vm a,b`(잠금 풀기, 끄기, 삭제, 없어진 것 확인)
  - `--manual-login`, `--profile`
  - job 판정은 화면 모델 `state.trackedJobs`의 새 job status다(`classifyJobStatus`).
  - 셸 준비는 `judgeShellReady`다(`connected` 또는 `degraded`).
  - 오류 메시지는 비밀 값을 지운 뒤 단계 detail과 stderr에 남긴다.
- Claude 리뷰 수정
  - (가) 쓰기 클릭은 모두 `clickOwned`를 거친다. 누르는 요소(또는 가장 가까운 `[data-vm-id]` 소유자)의 VM id가 대상 `pcv-it-` VM의 id와 다르면 거절한다.
    - 대상은 상세 종료·시작, 더보기 메뉴의 잠금·풀기·삭제, 네트워크 연결, checkpoint 만들기·복원·예약 저장·지우기다.
    - `selectVm`은 상세 패널이 그 VM을 보일 때까지 기다린다. 상세 화면이 늦게 바뀌어 보존 VM에 동작이 가는 일을 막는다.
  - (나) 첫 화면(모듈 로딩) 동안 계정 모드 셸의 `auth` 상태를 실패로 보던 것을 고쳤다. 로그인 화면이나 준비된 셸이 나올 때까지 기다린다. `auth`/`error`는 셸이 보이고 갱신이 끝난 뒤에만 실패다.
    - 이 문제는 설치본 `--check`에서 0ms 로그인 실패로 드러났다.
- 설치본 읽기 전용 `--check`(4b 코드, `--vm pcv-it-s2-source`):
  - 8단계 모두 pass다(`artifacts/browser-scenario-20261111/check-4b-live2/`).
  - 로그인 8.6초, 버전 `v0.43.4-admin-smoke`다. 마법사 칸, 상세 시작 버튼, 콘솔 탭, checkpoint 탭, template 표와 복제 폼을 찾았다.
  - summary에 비밀 값이 없다. 제출과 확인 수락은 하지 않았다.
- 결과: 새 node 시험 14/14, `npm run test:required --prefix web` exit 0(`test:web-contracts` 414/414, browser gate `ok=true` assertions 121 failed 0 violations 0, 기준선 변화 없음), web Pester 50/50, `git diff --check` clean.

## Task 5: PR B (스크립트)

- [x] clean HEAD 종료 검증, push, PR, green CI 뒤 merge, main push run 확인.

실행 기록(2026-11-11): clean HEAD `a323316`에서 PR gate를 돌렸다.
- 첫 실행은 `ok=False`였다. Host 시험 `ChromiumOpensLoopbackConsoleWithoutServiceTokenPaste`가 `http://127.0.0.1:4410/` prefix 충돌(`HttpListenerException`)로 실패했다.
  - PR #149 CI에서 실패한 것과 같은 시험이다. 이 PR은 `web/scripts`와 문서만 바꿨다.
  - 범위 밖 발견으로 backlog BL-0045(`undecided`)에 적었다.
- 두 번째 실행은 `ok=True`다(`artifacts/pr-gate/` build, `dotnet-test-release`, module-size-ratchet). assembly별 시험 수: Service 11, Contracts 201, Cli 186, Runtime 129, HyperV 288, Delivery 785, Host 246, Api 545, Verification 691.
- PR #150(`lane1/browser-scenario-20261111`)의 Required CI 다섯 개가 green이었다. `--match-head-commit 17658ec`로 merge했다(merge commit `6da5c47`). main push run Development Gates `42927035425`와 Public Boundary `42927035387`이 success다.

## Task 6: 계정 준비와 S1 시작 (Lane 2)

- [x] 사전 확인: 설치본 version, service, 계정 목록, `pcv-it-` VM 0개, 보존 VM 상태를 확인한다.
  - `pcv-s13-operator`를 만든다(위 `계정` 행).
  - 스크립트 `--check`로 화면을 확인한다.
  - S1을 `--execute`로 돌린다: Ubuntu ISO(`ubuntu-26.04.1-live-server-amd64.iso`)로 `pcv-it-s1r-ubuntu`(Gen 2, vCPU 2, 4096MB, 32GB, Default Switch)를 만들고 시작한다.
  - 그 뒤 브라우저 콘솔(키보드 입력과 텍스트 보내기)로 Ubuntu Server 설치를 시작한다.

실행 기록(2026-11-11 08:55~09:00 KST, `main` `6da5c47`의 스크립트):
- 사전 확인: 설치본 ARP 1개(`0.43.4`), service Running/Automatic, VM은 보존 VM 두 개(Off)뿐이었다. 시나리오 계정은 Task 4b에서 만들었고, Task 4b `--check`로 화면을 확인했다.
- S1 `--execute`(`npm run scenario:browser` 같은 스크립트, `--vm pcv-it-s1r-ubuntu --cpu 2 --memory 4096 --disk 32`, Ubuntu `ubuntu-26.04.1-live-server-amd64.iso`)는 77초에 `result=pass`다(`artifacts/browser-scenario-20261111/s1-execute/`).
  - 로그인 9.3초(계정)
  - 마법사 생성 job `succeeded`, 확인 대화상자는 없다.
  - `Default Switch` 연결 job `succeeded`(확인 수락)
  - 시작 job `succeeded`
  - 콘솔 frame 5.4초 뒤 보임. Enter 뒤 7.5초에 화면이 바뀌었다(hash `3460962542`→`3849845320`). 화면에는 Ubuntu 커널 부팅 로그가 보였다.
- summary에 비밀 값이 없다.
- 관찰(Task 11에서 고친다): execute summary의 `installed_version`이 비어 있다. 생성 뒤 마법사 대화상자가 "완료" 상태로 열린 채 남는다(화면 표시).
- Ubuntu 설치는 scratchpad 도우미가 이어서 한다. 도우미는 저장소에 넣지 않는다.
  - 도우미가 하는 일: 같은 CDP 도우미로 로그인한다. VM을 고르고 콘솔 탭을 연다. canvas에 포커스를 두고 키를 보내거나 텍스트 보내기 폼을 쓴다. canvas를 PNG로 저장한다.
  - 언어 English → 키보드 English (US) → 설치 유형 Ubuntu Server 화면까지 왔다(`artifacts/browser-scenario-20261111/s1-install/i01`~`i03`).
  - 로그인 직후 바로 보낸 키는 한 번 반영되지 않았다. 1.5초 기다린 뒤 보내니 반영됐다.
  - 게스트 비밀번호는 실행 경계에서 만들어 저장소 밖 DPAPI 보호 임시 파일에 두었다.
- Lane 2 예산(12회)에 닿아 여기서 Task 6을 닫고, 설치 마무리는 Task 7에서 잇는다.

## Task 7: S1 설치 시도 기록과 결함 분류 (Lane 2 시도, Lane 1 기록)

- [x] 브라우저 콘솔로 설치를 끝내고 재부팅한 뒤, 로그인해서 네트워크를 확인하려 했다. 설치를 끝내지 못해 시도와 결함을 기록하는 task로 바꿨다(사용자 결정 `S1 중단`).

실행 기록(2026-11-11 09:01~10:12 KST, Lane 2 checkpoint 네 번):
- 상세는 시도 기록 `docs/ga-ready/demo/s1-browser-reproof-2026-11-11.md`에 있다.
- 설치 진행: 저장소 요약 확인 → 확인 대화상자 Continue → 사용자 정보 화면. 이름·서버 이름·사용자 이름은 정확히 들어갔다. 비밀번호 두 칸이 일치하지 않아 Done을 누르지 못했다.
- 원인 조사
  - 서버 감사 기록상 키 이벤트는 모두 순서대로 서버에 도착했다.
  - 경합이 없을 때 frame route는 장당 0.82~0.95초로 정상이다.
  - 브라우저 화면이 멈춘 것은 저장소 밖 도우미가 남긴 Edge들(한때 62개)이 같은 VM 화면을 동시에 요청했기 때문이다.
  - 남은 결함은 콘솔 입력이다. 요청 처리가 press·release 각각 약 2초다. 키를 보통 속도로 보내면 일부가 빠진다. 요청마다 기다려도 비밀번호 두 칸이 달랐다. 텍스트 보내기 폼은 글자가 더 많이 빠진다.
- 첫 세 checkpoint는 Lane 2 예산(12회)을 넘겨 멈췄다. 첫 번째는 약 16회에서 멈췄고 70% 진행 보고를 빠뜨렸다. 세 번 모두 사용자 승인으로 다시 열었다.
- backlog에 BL-0046(콘솔 키 입력 느림·불일치)과 BL-0047(텍스트 폼 글자 누락)을 `counts`로 더했다(사용자 결정 `S1 중단`).
- 끝 상태
  - `pcv-it-s1r-ubuntu` Running(설치 프로그램 사용자 정보 화면)
  - 보존 VM 두 개 Off, 바뀌지 않음
  - 계정 `pcv-s13-operator` enabled
  - 남은 시나리오 Edge 0개
  - 설치본·service·MSI 변경 없음

## Task 8: S2 설치된 VM 복제 (Lane 2)

이 campaign에서 하지 않는다(사용자 결정 `S1 중단`). 다음 승인 "S1~S3 재시연"으로 넘긴다.

- [ ] `pcv-it-s1r-ubuntu`를 끄고 template으로 잠근다. 스크립트 S2로 `pcv-it-s2r-clone`을 복제한다(복제 시간을 기록하고 60초 기준과 비교). 복제본을 시작해 브라우저 콘솔에서 로그인 화면과 네트워크를 확인한다.

## Task 9: S3 checkpoint (Lane 2)

이 campaign에서 하지 않는다(사용자 결정 `S1 중단`). 다음 승인 "S1~S3 재시연"으로 넘긴다.

- [ ] 스크립트 S3로 `pcv-it-s2r-clone`에서 checkpoint를 만들고 복원한다. 예약을 미리 보고 저장한 뒤, 첫 예약 실행을 확인하고 예약을 지운다.

## Task 10: 정리 (Lane 2)

이 campaign에서 하지 않는다(사용자 결정 `S1 중단`). 다음 승인 "S1~S3 재시연"으로 넘긴다.

- [ ] 이번에 만든 `pcv-it-` VM을 브라우저로 지운다(template 잠금을 먼저 푼다). `pcv-s13-operator`를 끈다.
  - 끝 상태 확인: `pcv-it-` VM은 이번 것이 없고 보존 VM 둘만 남는다. 계정은 `pcv-admin2`만 enabled다. 설치본·service·listener·방화벽은 그대로다.

## Task 11: PR C (시도 기록)

- [x] Task 5~7 기록, 시도 기록 문서 `docs/ga-ready/demo/s1-browser-reproof-2026-11-11.md`, backlog BL-0046·BL-0047을 PR 하나로 merge한다. 판정 입력(`config/project-completion-criteria.json`)은 바꾸지 않는다. 종료 검증(Delivery, public source safety), push, PR, green CI 뒤 merge.

실행 기록(2026-11-11): Delivery 785/785, `test:public-source-safety` exit 0이었다. PR #151(`lane2/s1-s3-browser-reproof-20261111`)의 Required CI 다섯 개가 green이었고 `--match-head-commit b56209e`로 merge했다(merge commit `6e0d675`).

## Task 12: 대시보드 첫 로드 시간 (승인 4)

- [x] 원인을 측정하고 고친다(grok 구현, Claude 리뷰). 대상은 `ops/summary` 응답 5~7초와 첫 갱신의 취소 경로다.
  - `ops/summary`가 무엇을 기다리는지 서버 코드로 측정하고 줄인다. 예: WMI 조회가 순서대로 도는지, 캐시가 없는지.
  - 첫 방문에서 취소되는 `ops/summary` 요청(probe에서 2~3회)이 왜 생기는지 찾아 없앤다.
  - 시험: 지연 mock(느린 `ops/summary`)으로 첫 갱신이 한 번만 요청하고 결과를 그리는지 확인한다. 서버 쪽은 C# 단위 시험으로 본다.
  - 제품 payload가 바뀌므로 `release-train.json` queue에 한 행을 더한다. train 출발은 하지 않는다.
  - 검증: `dotnet test src/DesktopNode.sln -c Release`(clean HEAD), `npm run test:required --prefix web`, web Pester, `git diff --check`. 로컬 commit.

실행 기록(2026-11-11): grok이 별도 worktree에서 구현했고(Lane 2 시도와 병렬), Claude가 리뷰했다. commit `ff40acd`.
- 원인
  - 첫 방문에 `init()`의 `refreshAll()`과 로그인 완료(`_finishLogin` → `window.loadAll()`)가 겹친다.
  - 새 호출이 앞 갱신을 취소하고 12단계를 처음부터 하나씩 다시 돌았다. 설치본 probe의 `ops/summary` 취소 2~3회와 같은 모양이다.
- 고친 것
  - (가) `refreshAll(options)`: 자동 호출은 같은 인증으로 도는 갱신에 합류한다. 사용자 새로 고침, 연결 변경, token 지우기, 계정 변경 뒤 갱신, API base 변경은 `{ force: true }`로 새로 시작한다.
  - (나) 대시보드 단계(`ops.summary`, `host.status`, `vm.list`)를 동시에 돌려 먼저 그리고, 나머지 단계는 그 뒤 동시에 돈다.
  - (다) auth 오류는 남은 요청을 취소하고 `connectionState='auth'`다.
  - (라) `loadHost`·`loadVms`·`loadRuntimePolicy`·`refreshSelectedVm`도 늦게 온 취소 결과가 새 값을 덮지 않는다(BL-0043과 같은 `isSupersededRequest`).
  - BL-0044 읽는 중 문구 조건은 그대로다.
- Claude 리뷰 수정: 처음 구현은 자동 호출을 무조건 합류시켰다. 그래서 token 검사를 지난 갱신이 돌 때 로그인이 끝나면 새 token으로 다시 읽지 않을 수 있었다. 갱신이 token 검사를 통과할 때의 인증 값과 지금 인증 값이 같거나, 아직 검사 전일 때만 합류하게 고쳤다.
- 같은 가짜 지연(`ops/summary` 1,500ms, `host.status` 1,200ms, `vm.list` 500ms, 나머지 100ms)에서 숫자
  - 첫 갱신 3,884ms → 1,622ms(단계 합 3,800ms보다 짧다)
  - 첫 요청 50ms 뒤 자동 재호출 때 `ops/summary` 2회 → 1회
- 서버 `ops/summary`의 `host.status`·`vm.list` 동시 실행은 adapter(COM WMI)의 동시 호출 안전성을 증명하지 못해 하지 않았다. 범위 밖 관찰은 report-only다:
  - 클라이언트 `host.status`·`vm.list`와 서버 summary 사이에 WMI 일이 겹친다.
  - job 폴링 tick이 `ops/summary`를 더 부를 수 있다.
- 결과
  - 새 node 시험 6건(T12 4건, 인증 합류 2건). BL-0043·BL-0044 시험 유지.
  - `npm run test:required --prefix web` exit 0(`test:web-contracts` 420/420, browser gate `ok=true` assertions 121 failed 0 violations 0, 기준선 변화 없음)
  - web Pester 50/50, `git diff --check` clean
- 제품 payload가 바뀌어 `release-train.json` queue에 PR #152 행을 더했다. train 출발은 하지 않는다.

## Task 12b: 브라우저 콘솔 입력 결함과 시나리오 도우미 (사용자 결정 `S1 중단`)

- [x] BL-0046·BL-0047을 고친다(grok 구현, Claude 리뷰).
  - 서버 `console.input`이 요청당 약 2초인 원인을 측정하고 줄인다.
  - 키 이벤트가 순서대로 정확히 guest에 들어가게 한다. 예: Web Console이 키마다 press와 release를 따로 보내는 방식, 요청이 겹칠 때의 순서, 텍스트 입력 속도.
  - 시나리오 CDP 도우미의 결함도 고친다: Edge 종료 누수, 키 누름과 뗌 사이 간격, execute summary의 설치 버전, 생성 뒤 마법사 닫기.
  - 제품 payload가 바뀌므로 `release-train.json` queue에 한 행을 더한다.
  - 검증: 해당 C# 시험 프로젝트(`-c Release`), `npm run test:required --prefix web`, web Pester, `git diff --check`. 로컬 commit.

실행 기록(2026-11-11): grok이 별도 worktree에서 10:23~10:50에 구현했고 Claude가 리뷰했다. commit `2570919`(PR #152 merge 위로 rebase, 생성물은 합쳐진 소스에서 다시 만들었다).
- 원인 판단
  - (가) WMI provider가 요청마다 VM을 찾고 `Msvm_Keyboard` 관계를 따라갔다(요청당 약 2초의 주된 후보).
  - (나) Web Console은 keydown의 press와 keyup의 release를 기다리지 않고 보냈고, service도 같은 VM 요청을 동시에 처리할 수 있었다. release가 press보다 먼저 실행될 수 있다.
  - (다) `TypeText`가 문자열을 한꺼번에 넣었다.
- 고친 것
  - service
    - 같은 VM 입력은 도착 순서대로 하나씩 처리한다(adapter 레인과 provider 세션).
    - `Msvm_Keyboard` path를 VM id(WMI `Name`, GUID)로 캐시하고, 실패하면 한 번만 다시 찾는다.
    - `kind=text`는 글자마다 `TypeText`를 부르고 글자 사이를 50ms 띄운다.
    - 응답 계약, 길이 256, printable ASCII, 초당 50 제한은 그대로다.
  - Web Console
    - VM별 입력 큐로 앞 요청이 끝난 뒤 다음을 보낸다.
    - 429는 200ms 뒤 한 번 다시 보낸다.
    - 64개를 넘게 쌓이면 상태 줄에 알린다(i18n `vm.console.input.queue_full`).
  - 시나리오 도우미: Edge 종료 확인과 재종료, keyDown과 keyUp 사이 120ms, execute summary `installed_version`, S1 생성 뒤 마법사 닫기
  - HyperV 시험 두 건의 저장소 루트 찾기가 git worktree(`.git` 파일)에서도 된다.
- 리뷰
  - 캐시 키가 VM 이름이 아니라 GUID라서, 이름을 바꾸거나 같은 이름으로 다시 만든 VM에 입력이 잘못 가지 않는다(확인).
  - 키마다 adapter의 `GetVms()` 조회는 남는다. 설치본에서 키당 시간이 크게 남으면 다음 후보다(report-only).
- 결과
  - HyperV 292/292(새 키보드 시험 4건)
  - `npm run test:required --prefix web` exit 0(node 422, 콘솔 큐 4건·도우미 4건 포함, browser gate `ok=true` violations 0, 기준선 변화 없음)
  - web Pester 50/50, `git diff --check` clean
- 설치본에서 확인할 항목(다음 train probe): 키 하나에 걸리는 시간, 20자 비밀번호 두 칸 입력 일치, 텍스트 폼
- PR #153 첫 CI에서 새 순서 시험 두 건이 `dotnet` shard에서 실패했다(로컬에서는 통과).
  - 원인: 시험이 일부러 막아 둔 작업과 다른 작업을 공유 thread pool(`Task.Run`)에서 5초 안에 돌리려 했다. 작은 CI runner에서 병렬 시험과 겹치면 모자랄 수 있다.
  - 작업을 전용 스레드(`TaskCreationOptions.LongRunning`)로 돌리고 대기 한도를 30초로 늘렸다(시험만 바꿨다).
  - HyperV 292/292. 키보드 시험만 열 번 반복해 모두 4/4였다.
- 제품 payload가 바뀌어 `release-train.json` queue에 PR #153 행을 더했다.

## Task 13: PR D와 마무리

- [x] Task 12b를 PR로 merge하고 campaign을 닫는다(`next_approval_required`에 다음 승인 문장: train 출발, S1~S3 재시연). clean HEAD 종료 검증 뒤 push, PR, green CI 뒤 merge, main push run 확인.

실행 기록(Task 13, 2026-11-11): Task 12b와 같은 branch에서 campaign을 닫았다. 다음 승인은 세 가지다.
- release train `0.43.5` 출발: queue #152·#153. 시연할 수 있는 동작을 바꾸는 행이라 §10 묶음 출발 기준 3에 맞는다.
- 그 설치본에서 S1~S3 재시연
- BL-0045 분류
clean HEAD 종료 검증과 PR merge 결과는 PR 본문과 다음 campaign 기록에 남는다.

## Nonclaims

- operational current는 `0.43.4-admin-smoke` 그대로다. 대시보드 수정은 다음 train 승인 전까지 설치본에 없다.
- 재시연은 이 호스트의 Lane 2 시연이다. 승격 근거가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.
