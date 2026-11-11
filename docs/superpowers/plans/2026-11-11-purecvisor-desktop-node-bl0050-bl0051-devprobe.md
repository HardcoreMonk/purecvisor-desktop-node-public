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

- [ ] BL-0050: 콘솔 frame·input 요청을 전역 요청 한도에서 뺀다. 콘솔 route는 VM별 한도(frame 간격, 입력 초당 50)를 그대로 쓴다. 다른 route의 전역 한도는 그대로다.
- [ ] BL-0051: loopback 모드 첫 방문에서 두 번째 갱신을 시작시키는 호출을 찾아 없앤다. node 시험으로 설치본 trace의 순서를 재현하고, `ops/summary` 1회·취소 0회를 확인한다.
- [ ] 시험: Api 시험(콘솔 요청이 전역 한도를 소비하지 않고, 다른 route는 그대로 429), node 시험, 기존 시험 유지. `release-train.json` queue에 행을 더한다. 로컬 commit.

## Task 3: PR

- [ ] clean HEAD 종료 검증(PR gate), push, PR, green CI 뒤 merge, main push run 확인.

## Task 4: dev probe (Lane 2)

- [ ] `main` merge commit에서 개발 build `0.43.7-admin-smoke`와 update catalog를 만든다(`artifacts/devprobe-package-20261111-04307`).
- [ ] 계정을 보관한다(보호 위치 백업, data root에서 치우기, service 재시작). `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`를 DryRun 뒤 실행해 dev build를 올린다.
- [ ] 측정
  - 대시보드(loopback 모드, 새 context 여섯 번): loopback 세션 POST 1회, `ops/summary` 1회, 취소 0회, 목록 시각
  - 콘솔 입력(`pcv-it-s1r-ubuntu` 사용자 정보 화면): 사람이 치는 속도로 Backspace와 20자 두 번. 입력 요청 중앙값, 거절 0건, 두 칸 일치, 텍스트 폼(`pcv`, `pcvs1r`)
- [ ] `-Action Rollback`(DryRun 뒤 실행)으로 `0.43.6`에 되돌리고, 계정을 복원한다(해시·ACL 비교). 끝 상태를 확인한다.

## Task 5: 기록과 마무리

- [ ] evidence `docs/ga-ready/evidence/lane2-devprobe-2026-11-11-04307.md`를 쓴다. campaign을 닫고 다음 승인(통과하면 release train 출발과 S1~S3 재시연)을 적는다. push, PR, green CI 뒤 merge.

## Nonclaims

- dev probe는 승격 근거가 아니다. operational current는 `0.43.4-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
