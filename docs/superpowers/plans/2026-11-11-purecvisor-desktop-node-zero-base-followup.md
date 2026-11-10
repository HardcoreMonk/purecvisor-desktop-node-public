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

- [ ] 보호 위치의 `pcv-train-accounts-backup-04304`를 지운다.
  - 지우기 전: data root 계정 파일과 백업 `accounts.json`의 SHA-256과 크기가 같은지, data root 계정 파일의 ACL이 백업의 ACL 기록과 같은지 본다. 다르면 지우지 않고 멈춘다.
  - 지운 뒤: data root 계정 파일의 SHA-256과 ACL이 그대로인지 본다. service Running/Automatic, Web `200`, loopback 세션 `409` `PCV_LOOPBACK_SESSION_DISABLED`도 확인한다.
  - 이 plan에 실행 기록을 쓰고 로컬 commit한다.

## Task 2: train 묶음 출발 기준 (승인 3)

- [ ] `docs/DEVELOPMENT_PROCEDURE.md` §10의 출발 기준을 고친다.
  - 0.43.1~0.43.4처럼 표시 수정 하나마다 train을 보내지 않는다.
  - 조기 출발 사유(데이터 손실, 설치·업그레이드 실패, 보안 결함)가 아닌 표시·문구 수정은 queue에 쌓아 둔다.
  - 이런 수정은 기능 변경과 함께 싣거나 queue가 정한 수만큼 모였을 때 출발을 제안한다.
  - 에이전트가 다음 승인으로 train 출발을 올릴 때도 이 기준을 따른다.
  - pin 대상이면 같은 commit에서 `Update-PcvContractSpecPins.ps1 -Apply`를 돌린다.
  - 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 3: PR A (문서)

- [ ] clean HEAD에서 Delivery 시험과 `npm run test:public-source-safety --prefix web`를 돌린다. push하고 PR을 연다. Required CI 다섯 개가 green이면 merge하고 main push run을 확인한다.

## Task 4: 브라우저 시나리오 스크립트 (승인 2, Lane 1)

- [ ] `web/scripts/run-browser-scenario.mjs`와 node 시험을 만든다(grok 구현, Claude 리뷰). Playwright로 현행 Web Console을 조작하고, 시나리오는 `--scenario s1|s2|s3`로 고른다.
  - 로그인: 계정이 있으면 로그인 화면에 `--username`과 환경 변수 `PCV_SCENARIO_PASSWORD`를 넣는다. 계정이 없으면 loopback 세션을 쓴다. token과 비밀번호는 출력하지 않는다.
  - S1: VM 만들기 마법사로 ISO VM을 만들고 시작한다. 콘솔 화면이 비어 있지 않을 때까지 기다리고, 키보드 입력 뒤 화면이 바뀌는지 본다.
  - S2: 꺼진 VM을 template으로 잠그고 복제한다. 복제 job이 끝나는 시간을 잰다(기준 60초).
  - S3: checkpoint를 만들고 복원한다. 예약을 미리 보고 저장하고, 원하면 첫 예약 실행을 기다린 뒤 예약을 지운다.
  - 공통: `--plan-only`(기본)는 요청 없이 단계만 출력한다. `--check`는 로그인 뒤 화면과 버튼만 찾고 제출하지 않는다(읽기 전용). 단계마다 캡처와 `summary.json`(계약 `pcv-browser-scenario-v1`)을 `artifacts/browser-scenario-<yyyymmdd>/` 아래에 쓴다. 끝에 만든 `pcv-it-` VM을 지우는 `--cleanup`이 있다.
  - 이름 제한: 스크립트는 `pcv-it-`로 시작하지 않는 VM에는 쓰기 동작을 하지 않는다.
  - 검증: `npm run test:required --prefix web`(새 node 시험 포함), `git diff --check`. 로컬 commit.

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
