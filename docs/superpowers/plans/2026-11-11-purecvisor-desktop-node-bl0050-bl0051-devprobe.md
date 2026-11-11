# BL-0050·BL-0051 수정과 설치본 dev probe Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** train `0.43.6` probe에서 남은 두 결함을 고친다. 하나는 콘솔 요청이 전역 요청 한도에 세지는 것(BL-0050)이고, 하나는 loopback 첫 갱신이 두 번 시작되는 것(BL-0051)이다. train을 보내기 전에 이 호스트 설치본에서 제품 Update와 Rollback으로 효과를 먼저 확인한다(dev probe). train 계정 백업을 지운다.

**Architecture:** 근거는 `docs/ga-ready/evidence/lane2-dashboard-console-probe-2026-11-11-04306.md`와 backlog BL-0050·BL-0051이다. dev probe 절차는 `docs/DEVELOPMENT_PROCEDURE.md` §10과 선례 `docs/superpowers/plans/2026-10-08-purecvisor-desktop-node-s1-installed.md`(`Invoke-PcvDesktopNodeProduct.ps1 -Action Update`, `-Action Rollback`)를 따른다. dev probe는 승격 근거가 아니다. 구현은 grok(Herdr `grok-status`), 리뷰와 Lane 2는 Claude가 한다.

**Tech Stack:** C# Local API throttle, TypeScript Web Console, `build.ps1`, `New-PcvAdminSmokeUpdatePackage.ps1`, `Invoke-PcvDesktopNodeProduct.ps1`, Playwright·CDP, `gh`

## 사용자 결정 (2026-11-11)

승인 원문: `"1,2 : counts , 3 삭제"` (`train-04306-20261111` 마무리 보고의 다음 승인 1~3에 대한 답). 각 번호의 문장은 아래와 같다.

| 항목 | 범위 |
| --- | --- |
| 1 | "(권장) 콘솔 입력 요청 한도와 loopback 첫 갱신 재시작 수정과 설치본 dev probe: Lane 1 개발(BL-0050 콘솔 frame·input을 전역 요청 한도에서 빼고 VM별 한도만 적용, BL-0051 loopback 모드 첫 갱신의 두 번째 refreshAll 원인 제거), grok 구현, push, PR, green CI 뒤 merge. 그 뒤 Lane 2 dev probe(이 호스트 설치본에 제품 Update로 개발 build를 올려 대시보드 첫 로드와 pcv-it-s1r-ubuntu 콘솔 입력(키 시간, 20자 두 칸 일치, 텍스트 폼)을 재고 제품 Rollback으로 되돌림, 계정 파일 보관과 복원, 설치본 승격 없음). 결과는 문서 PR, green CI 뒤 merge. release train 출발은 dev probe 통과 뒤 따로 승인받는다." Task 2~5 |
| 2 | "backlog 분류: BL-0050(콘솔 요청이 전역 요청 한도에 세짐)과 BL-0051(loopback 모드 첫 갱신의 두 번째 시작)을 counts 또는 out-of-scope로 정한다." 답 `counts`. Task 1 |
| 3 | "train 계정 백업 삭제: train 0.43.6 동안 저장소 밖 보호 위치에 둔 백업 accounts.json, fullgate가 만든 빈 계정 저장소, ACL 기록을 지운다. data root의 계정 파일은 그대로 둔다. 지우기 전에 data root 계정 파일과 백업의 SHA-256이 같은지 확인한다. 기록은 다음 문서 PR에 남긴다." 답 `삭제`. Task 1 |
| dev probe | 개발 build version은 `0.43.7-admin-smoke`(꼬리표 `04307`)이고 출력은 `artifacts/devprobe-package-20261111-04307`이다. 다음 train과 이름이 겹치지 않는다. 제품 Update로 설치본을 `0.43.6` → `0.43.7` dev build로 바꾸고, 재고 나서 제품 Rollback으로 `0.43.6`에 되돌린다. 계정은 loopback 모드 측정 동안 보관했다가 복원한다. 설치본 승격, `current-evidence.json` 쓰기, MSI 설치·제거, REMOVE_DATA는 하지 않는다 |
| merge | 2026-11-09 상시 승인 `"앞으로 Required CI가 green이면 PR merge도 자동 승인"`(`merge_policy=after-green-ci`). merge 뒤 main push run을 확인한다 |
| ADR-0016 | standing approval(2026-10-07)은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정한다. 이번 probe는 기존 `pcv-it-s1r-ubuntu` 콘솔 입력만 쓴다 |

## Global Constraints

- 보존 VM 둘과 BL-0028 parent VHD를 바꾸지 않는다. `pcv-it-s1r-ubuntu`는 콘솔 입력 대상으로만 쓴다.
- token, 비밀번호, 계정 해시, LAN 주소, 호스트명, 사용자 홈 경로를 문서·명령줄·캡처에 남기지 않는다. 비밀 값은 실행 경계에서 만든다.
- 새 route, 새 권한, 새 `Add-Type`/P/Invoke/native ACL, 새 packaging `*.Tests.ps1`을 만들지 않는다. PowerShell helper fallback을 더하지 않는다.
- Web 규칙: `npm run build:served --prefix web`, axe violation 0, `--prune` 없음.
- 제품 payload를 바꾸는 PR은 `release-train.json` queue에 행을 더한다. train 출발은 dev probe 통과 뒤 따로 승인받는다.
- Rollback이 실패하면 멈추고 보고한다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: 계정 백업 삭제와 분류 (승인 2·3)

- [x] 보호 위치의 `pcv-train-accounts-backup-04306`을 지운다. 지우기 전에 data root 계정 파일과 백업 `accounts.json`의 SHA-256, 크기, ACL 표기가 같은지 본다. 다르면 지우지 않는다. backlog BL-0050·BL-0051을 `counts`로 분류한다. 로컬 commit.

실행 기록(2026-11-11 20:17 KST):
- 지우기 전: data root 계정 파일과 백업 `accounts.json`의 SHA-256이 같았다(1,421 bytes). data root 계정 파일의 ACL도 백업의 ACL 기록과 같았다.
- 백업 `accounts.json`, fullgate가 만든 빈 저장소(174 bytes), ACL 기록 파일을 지웠다. 비게 된 백업 폴더도 지웠다.
- 지운 뒤: data root 계정 파일의 SHA-256과 ACL이 그대로다. service Running, Web `200`, loopback 세션 `409`다.
- backlog BL-0050·BL-0051을 `counts`로 분류했다(상태 `open`).

## Task 2: 수정 (grok 구현, Claude 리뷰)

- [x] BL-0050: 콘솔 frame·input 요청을 전역 요청 한도에서 뺀다. 콘솔 route는 VM별 한도(frame 간격, 입력 초당 50)를 그대로 쓴다. 다른 route의 전역 한도는 그대로다.
- [x] BL-0051: loopback 모드 첫 방문에서 두 번째 갱신을 시작시키는 호출을 찾아 없앤다. node 시험으로 설치본 trace의 순서를 재현하고, `ops/summary` 1회·취소 0회를 확인한다.
- [x] 시험: Api 시험(콘솔 요청이 전역 한도를 소비하지 않고, 다른 route는 그대로 429), node 시험, 기존 시험 유지. `release-train.json` queue에 행을 더한다. 로컬 commit.

실행 기록(2026-11-11 20:17~20:45 KST): grok이 구현했고 Claude가 리뷰했다. commit `04b1a91`.
- BL-0051 원인: 갱신을 두 번 부르는 코드가 아니라 page reload였다.
  - `web/sw.js`의 첫 `clients.claim`이 `controllerchange`를 일으켰고, `pcvRegisterServiceWorker`가 controller 유무와 관계없이 `location.reload()`를 했다.
  - reload가 진행 중인 첫 `ops/summary`를 취소했고, 다시 로드된 문서가 두 번째 요청을 보냈다(trace 478~479ms).
  - node 시험으로 재현하면 `refreshAll`은 `init`에서 한 번이다. `_finishLogin`의 `loadAll` 두 번은 진행 중인 갱신에 합류한다(`ops/summary` 1회, 취소 0회).
- BL-0051 수정: 등록 전에 controller가 이미 있던 탭에서만 `controllerchange`에 reload한다(worker 교체). 첫 claim은 reload하지 않는다.
  - 남는 영향(report-only): 처음 방문한 탭은 이후 worker가 바뀌어도 다음 탐색 때까지 기존 JS를 쓴다.
- BL-0050 수정: `HandleUnlockedConsoleRoute`에서 `throttle.Enforce`를 뺐다. 콘솔 frame·input은 전역 한도를 소비하지도, 그 한도로 거절되지도 않는다. VM별 한도(입력 초당 50, frame 100ms)는 그대로다. 다른 route는 그대로 전역 한도다.
  - 남는 위험(report-only): 콘솔 경로의 전체 상한은 VM 수에 비례한다. loopback 전용 내부 서비스라 받아들인다.
- 결과
  - Api 550/550(콘솔이 전역 한도를 소비하지 않음, 다른 route 예산과 분리, 다른 route는 여전히 `PCV_RATE_LIMIT_EXCEEDED`, VM별 한도 시험 유지)
  - `npm run test:required --prefix web` exit 0(node 433/433, 첫 claim reload 없음과 기존 controller reload 시험 포함, browser gate `ok=true` violations 0)
  - web Pester 50/50, `git diff --check` clean
- `release-train.json` queue에 PR #159 행을 더했다(queue 4행).
- PR #159 첫 CI에서 #157의 시험 `ConsoleInputDoesNotHoldTheGlobalLockWhileTheAdapterRuns`가 실패했다. route-timeout probe를 3초 기다리는 자리다. 그 probe는 서버 안에서 공유 thread pool을 쓴다.
  - 막힌 쪽은 release 전까지 끝나지 않으므로, 기다림을 30초로, frame 중 입력 상한을 10초로 늘렸다(시험만 바꿨다). 그 class 5건을 다섯 번 반복해 모두 통과했다.

## Task 3: PR

- [x] clean HEAD 종료 검증(PR gate), push, PR, green CI 뒤 merge, main push run 확인.

실행 기록(2026-11-11): PR #159의 첫 CI는 #157의 시간 의존 시험 하나로 `dotnet`이 실패했다. 대기 수정(`297eb7c`) 뒤 Required CI 다섯 개가 green이었고, merge commit `dc042b8`의 main push run Development Gates `43015553438`과 Public Boundary `43015553465`가 success다.

## Task 4: dev probe (Lane 2)

- [x] `main` merge commit에서 개발 build `0.43.7-admin-smoke`와 update catalog를 만든다(`artifacts/devprobe-package-20261111-04307`).
- [x] 계정을 보관한다(보호 위치 백업, data root에서 치우기, service 재시작). `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`를 DryRun 뒤 실행해 dev build를 올린다.
- [x] 측정
  - 대시보드(loopback 모드, 새 context 여섯 번): loopback 세션 POST 1회, `ops/summary` 1회, 취소 0회, 목록 시각
  - 콘솔 입력(`pcv-it-s1r-ubuntu` 사용자 정보 화면): 사람이 치는 속도로 Backspace와 20자 두 번. 입력 요청 중앙값, 거절 0건, 두 칸 일치, 텍스트 폼(`pcv`, `pcvs1r`)
- [x] `-Action Rollback`(DryRun 뒤 실행)으로 `0.43.6`에 되돌리고, 계정을 복원한다(해시·ACL 비교). 끝 상태를 확인한다.

실행 기록(2026-11-11 20:52~20:58 KST): 상세는 evidence `docs/ga-ready/evidence/lane2-devprobe-2026-11-11-04307.md`에 있다.
- 개발 build `0.43.7-admin-smoke`는 `dc042b8`에서 만들었다(MSI `a404ac5d…`). 계정 보관 뒤 제품 Update(3초)를 했고, 측정 뒤 제품 Rollback(2초)으로 `0.43.6+bfb30c1`에 되돌렸다. 계정을 복원했다(해시·ACL 일치).
- 대시보드(loopback 모드, 여섯 번): 통과. 매번 loopback POST 1회, 문서 로드 1회, `ops/summary` 1회, 취소 0회. 목록은 4.3~5.1초다(0.43.6 8.5초).
- 콘솔 키 입력: 통과. 요청 204건, 중앙값 168ms, 최대 512ms, 거절 0건이다. 같은 20자를 두 칸에 넣은 결과가 일치했다.
- 텍스트 보내기 폼: 실패. `pcv` → 빈 칸, `pcvs1r` → `n`. backlog BL-0047을 다시 열었다. S1 설치는 키보드 입력으로 할 수 있어 막히지 않는다.

## Task 5: 기록과 마무리

- [x] evidence `docs/ga-ready/evidence/lane2-devprobe-2026-11-11-04307.md`를 쓴다. campaign을 닫고 다음 승인(통과하면 release train 출발과 S1~S3 재시연)을 적는다. push, PR, green CI 뒤 merge.

실행 기록(Task 5, 2026-11-11): evidence를 쓰고 BL-0050·BL-0051을 닫았다(PR #159, dev probe 통과). BL-0047을 다시 열었다. campaign을 닫는다. 다음 승인은 release train `0.43.7` 출발, S1~S3 재시연, BL-0047, dev probe 계정 백업 삭제다.

## Nonclaims

- dev probe는 승격 근거가 아니다. operational current는 `0.43.4-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
