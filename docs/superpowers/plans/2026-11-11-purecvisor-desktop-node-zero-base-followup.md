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
| 계정 | 권장안 자동 승인(`"앞으로는 권장 사항 자동 승인 처리 한다."`, 2026-11-08)으로 연다. 설치본은 계정 모드라 loopback 세션이 `409`다. 브라우저 시나리오는 이번에만 쓸 operator 계정 `pcv-s13-operator`로 로그인한다. 계정은 `pcvcli account create --password-env`(service bearer, protected token file)로 만들고, 끝나면 `pcvcli account disable`로 끈다. 비밀번호는 실행 경계에서 만들어 환경 변수로만 넘긴다. 기존 계정 세 개(`pcv-admin`, `pcv-admin2`, `pcv-s4-operator`)는 바꾸지 않는다. 계정 파일을 옮기거나 service를 다시 시작하지 않는다 |
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

- [ ] `--execute`를 구현한다(grok 구현, Claude 리뷰). 모든 쓰기 동작은 `assertScenarioVmName`을 거친다.
  - S1: VM 만들기 마법사로 ISO VM을 만들고 시작한다. 콘솔 화면이 비어 있지 않을 때까지 기다리고, 키보드 입력 뒤 화면이 바뀌는지 본다.
  - S2: 꺼진 VM을 template으로 잠그고 복제한다. 복제 job이 끝나는 시간을 잰다(기준 60초).
  - S3: checkpoint를 만들고 복원한다. 예약을 미리 보고 저장한다. `--wait-schedule`이면 첫 예약 실행을 기다린 뒤 예약을 지운다.
  - `--cleanup`: 만든 `pcv-it-` VM을 지운다(template 잠금을 먼저 푼다).
  - 계정이 생기면(Task 6) `--check`를 설치본에 돌려 선택자를 확인한다. 이 확인은 Task 6 기록에 쓴다.
  - 검증: `npm run test:required --prefix web`, `git diff --check`. 로컬 commit.

## Task 5: PR B (스크립트)

- [ ] clean HEAD 종료 검증, push, PR, green CI 뒤 merge, main push run 확인.

## Task 6: 계정 준비와 S1 시작 (Lane 2)

- [ ] 사전 확인: 설치본 version, service, 계정 목록, `pcv-it-` VM 0개, 보존 VM 상태를 확인한다.
  - `pcv-s13-operator`를 만든다(위 `계정` 행).
  - 스크립트 `--check`로 화면을 확인한다.
  - S1을 `--execute`로 돌린다: Ubuntu ISO(`ubuntu-26.04.1-live-server-amd64.iso`)로 `pcv-it-s1r-ubuntu`(Gen 2, vCPU 2, 4096MB, 32GB, Default Switch)를 만들고 시작한다.
  - 그 뒤 브라우저 콘솔(키보드 입력과 텍스트 보내기)로 Ubuntu Server 설치를 시작한다.

## Task 7: S1 설치 완료와 네트워크 (Lane 2)

- [ ] 브라우저 콘솔로 설치를 끝내고 재부팅한다. 로그인한 뒤 `ip -4 addr`와 `ping`(주소와 이름)으로 네트워크를 확인한다. guest 비밀번호는 실행 경계에서 만들고 남기지 않는다. 화면 캡처는 `artifacts/` 아래에 둔다.

## Task 8: S2 설치된 VM 복제 (Lane 2)

- [ ] `pcv-it-s1r-ubuntu`를 끄고 template으로 잠근다. 스크립트 S2로 `pcv-it-s2r-clone`을 복제한다(복제 시간을 기록하고 60초 기준과 비교). 복제본을 시작해 브라우저 콘솔에서 로그인 화면과 네트워크를 확인한다.

## Task 9: S3 checkpoint (Lane 2)

- [ ] 스크립트 S3로 `pcv-it-s2r-clone`에서 checkpoint를 만들고 복원한다. 예약을 미리 보고 저장한 뒤, 첫 예약 실행을 확인하고 예약을 지운다.

## Task 10: 정리 (Lane 2)

- [ ] 이번에 만든 `pcv-it-` VM을 브라우저로 지운다(template 잠금을 먼저 푼다). `pcv-s13-operator`를 끈다.
  - 끝 상태 확인: `pcv-it-` VM은 이번 것이 없고 보존 VM 둘만 남는다. 계정은 `pcv-admin2`만 enabled다. 설치본·service·listener·방화벽은 그대로다.

## Task 11: PR C (재시연 기록)

- [ ] `docs/ga-ready/demo/s1-s3-browser-reproof-2026-11-11.md`에 Task 6~10 결과(설치본, 단계별 시각·job·캡처 경로, S2 복제 시간, FAIL과 관찰)를 쓴다. 판정 입력(`config/project-completion-criteria.json`)은 바꾸지 않는다. 종료 검증, push, PR, green CI 뒤 merge.

## Task 12: 대시보드 첫 로드 시간 (승인 4)

- [ ] 원인을 측정하고 고친다(grok 구현, Claude 리뷰). 대상은 `ops/summary` 응답 5~7초와 첫 갱신의 취소 경로다.
  - `ops/summary`가 무엇을 기다리는지 서버 코드로 측정하고 줄인다. 예: WMI 조회가 순서대로 도는지, 캐시가 없는지.
  - 첫 방문에서 취소되는 `ops/summary` 요청(probe에서 2~3회)이 왜 생기는지 찾아 없앤다.
  - 시험: 지연 mock(느린 `ops/summary`)으로 첫 갱신이 한 번만 요청하고 결과를 그리는지 확인한다. 서버 쪽은 C# 단위 시험으로 본다.
  - 제품 payload가 바뀌므로 `release-train.json` queue에 한 행을 더한다. train 출발은 하지 않는다.
  - 검증: `dotnet test src/DesktopNode.sln -c Release`(clean HEAD), `npm run test:required --prefix web`, web Pester, `git diff --check`. 로컬 commit.

## Task 13: PR D와 마무리

- [ ] campaign을 닫는다(`next_approval_required`에 다음 승인 문장). clean HEAD 종료 검증 뒤 push, PR, green CI 뒤 merge, main push run 확인.

## Nonclaims

- operational current는 `0.43.4-admin-smoke` 그대로다. 대시보드 수정은 다음 train 승인 전까지 설치본에 없다.
- 재시연은 이 호스트의 Lane 2 시연이다. 승격 근거가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.
