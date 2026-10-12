# S4 LAN 계정 사용 시나리오 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** ADR-0017 시나리오 S4("LAN의 다른 PC에서 계정으로 로그인해 S1~S3을 쓴다")를 연다. 첫 PR은 승인된 대로 criteria S3를 닫고(설치본 `0.42.96` 재시연 PASS) C5 확인 task를 이관하며, S4를 어떻게 시연할지 설계 문서를 쓴다. 설계가 정한 구현·시연 task는 그 설계의 결정이 승인된 뒤 이 plan에 더한다.

**Architecture:** 완료 기준은 `config/project-completion-criteria.json` S4와 `docs/adr/0017-scenario-delivery-completion.md`다. 2026-10-11 Task 7 조사에서 확인한 사실: Host의 `/pcv-config.js`는 API listener prefix의 authority를 `apiBaseUrl`로 준다(`DesktopNodeHostApplication.StaticAuth.cs`). 그래서 원격 브라우저에 맞는 API 주소를 주는지 LAN prefix마다 확인해야 한다. 계정은 `pcvcli account create`(`POST /api/v1/accounts`, 첫 계정 admin)로 만들고, 계정이 생기면 loopback session 발급이 닫힌다. LAN listener와 방화벽 변경 도구는 `Invoke-PcvOsMutationGateSmoke.ps1`에 있다. S1 시연 guest는 Ubuntu Server라 브라우저가 없다. branch는 `lane1/s4-lan-account-20261011`이다.

**Tech Stack:** C# Host listener와 static auth, `pcvcli account`, Windows Firewall, Hyper-V guest(ADR-0016 `pcv-it-` VM), Playwright, `pcvverify completion`

## 사용자 결정 (2026-10-11)

승인 원문: `1,2,3`의 3 (campaign `train-04296-20261011` Task 8 보류 보고): `train 뒤 S4 campaign, 그 뒤 legacy-retirement campaign(2026-10-10 승인 2·3 그대로). criteria S3 닫기와 C5 확인(Task 12, 2026-10-19 이후) 이관은 S4 campaign 첫 PR에서 한다.`

| 항목 | 범위 |
| --- | --- |
| 2026-10-10 승인 2 | `train 뒤 S4 campaign을 연다(2026-10-10 승인 3의 순서; 설치본 --allow-lan/firewall 변경은 그 campaign의 기존 사전 승인 범위 안에서만).` |
| 2026-10-09 사전 승인 | 감사 개선안 3: `S4 campaign(LAN의 다른 PC에서 계정 로그인 후 S1~S3, 이어서 release train 1회). S3 뒤에 연다. 설치본 --allow-lan과 방화벽 변경은 2026-10-09 사전 승인이다.` |
| 이 campaign의 첫 범위 | Lane 1만 연다(criteria S3 닫기, C5 이관, S4 설계 문서, push/PR, green CI 뒤 merge). host mutation은 설계 결정 승인 뒤 task로 더한다 |
| 2026-10-10 승인 3 | `train 뒤 legacy-retirement campaign을 연다 ...` S4 campaign이 닫힌 뒤 연다 |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·구성·삭제로 한정, disk는 artifacts 아래 |
| 이관 | `train-04296-finish-20261011` Task 8(C5 runner 확인, `not_before` 2026-10-19) → Task 2 |

## 사용자 결정 (2026-10-12)

승인 원문: `1` (campaign `s4-lan-account-20261011` `next_approval_required` 1번). 설계 권장안 D1-A, D2-A, D3, D4, D5를 승인하고 설계 4절 task 1~5를 이 campaign에 더한다. Lane 1 PR은 green CI 뒤 merge. Lane 2는 사전 승인(`--allow-lan`, 방화벽)과 계정 생성·disable, ADR-0016 `pcv-it-s4-*` VM 생성·삭제.

| 항목 | 범위 |
| --- | --- |
| D1 | A. 사용자의 실제 PC에서 사용자가 S1~S3을 시연한다. 그 전에 에이전트가 이 호스트 브라우저로 `http://<host-lan-ip>/` 리허설 R을 돈다. R은 S4 판정 근거가 아니다 |
| D2 | A. 설치 service를 API `http://<host-lan-ip>:7777/`, Web `http://<host-lan-ip>:80/`로 재구성한다. 제품 변경 없음 |
| D3 | 방화벽 관리 규칙 두 개(7777, 80, Private, LocalSubnet). Web 규칙 이름은 `PureCVisor Desktop Node Web LAN`. 시연 뒤 loopback 복귀와 규칙 0 |
| D4 | 사용자가 admin 계정을 직접 만든다. 에이전트 리허설 계정은 operator이며 실행 경계 비밀번호로 만든 뒤 disable한다 |
| D5 | `web/scripts/run-s4-lan-scenario.mjs`. 비밀번호는 환경 변수. VM 이름은 `pcv-it-s4-*`. disk는 artifacts 아래 |
| Lane 1 | push, PR, green CI 뒤 merge. `current-evidence.json`은 쓰지 않는다 |
| Lane 2 | `--allow-lan`, 방화벽, 계정 생성·disable, `pcv-it-s4-*` VM 생성·삭제 |
| 유지 | Task 2(C5 runner 확인)는 `not_before` 2026-10-19 그대로 큐에 남긴다 |

## 사용자 결정 (2026-10-12, 2차)

승인 원문: `1,2` (Task 4 정지 보고의 다음 승인). Grok이 Task 4 직전에 `configure-installed`가 prefix를 loopback으로 고정하고
`--allow-lan`을 service 명령줄에 넣지 않는 것을 찾았고, Claude가 코드로 확인했다(설계 2절 "설치 service 구성" 행, D2 정정).

| 항목 | 범위 |
| --- | --- |
| 1 | D2 정정 A′. `configure-installed`에 `--prefix`, `--web-prefix`를 넣는 Lane 1 제품 변경(Host, Host.Tests, 관련 문서, release-train queue 행). push, PR, green CI 뒤 merge. Task 8로 Task 4 앞에 둔다 |
| 2 | Task 4와 Task 6의 설치 service 재구성·복귀는 merge된 `main` 빌드본 `DesktopNode.Host.exe`로 한다. service 실행 파일은 설치본 그대로다. release train은 S4 뒤 1회(2026-10-09 사전 승인) |
| 상태 | Task 3 보정은 PR #92로 merge됐다. 사용자 admin 계정 `<admin>`이 있어 Task 4 시작 조건을 채웠다 |

## Global Constraints

- 설치본은 operational current `0.42.96-admin-smoke`다. 보존 VM `pcv-guest-installed-04253-r1`과 template `pcv-it-s2-source`는 건드리지 않는다.
- public 문서에는 사용자 홈 경로, LAN 사설 IP, 호스트명, token·password 값을 적지 않는다. LAN 주소는 `<host-lan-ip>` 같은 자리 표시로 쓴다.
- 계정 비밀번호와 token은 실행 경계에서 만들고 명령줄·summary·evidence·캡처에 남기지 않는다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: criteria S3 닫기, C5 이관, S4 설계 (첫 PR)

- [x] `config/project-completion-criteria.json` S3를 `passed`, `demo_record`를 `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md`로 바꾸고 ADR 인덱스·문서 인덱스의 S3 상태를 맞춘다. S4 설계 `docs/superpowers/specs/2026-10-11-purecvisor-desktop-node-s4-lan-account-design.md`를 쓴다: "다른 PC" 실현 방법(물리 PC, Default Switch guest, External switch guest의 장단점과 권장안), LAN listener prefix와 `/pcv-config.js` `apiBaseUrl`이 원격 브라우저에서 맞는지, 계정 bootstrap과 loopback session 종료의 영향, 방화벽 규칙 범위와 끝 상태, S1~S3을 원격에서 하는 시연 순서, 필요한 제품 변경과 그 검증, 시연 뒤 되돌리기. `pcvverify completion`으로 S3 `met`을 확인하고, `dotnet test src/DesktopNode.Verification.Tests -c Release`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. push, PR, green CI 뒤 merge. 설계의 결정 사항은 campaign `next_approval_required`에 번호 문장으로 둔다.

실행 기록(2026-10-11): criteria S3를 `passed`, `demo_record` `docs/ga-ready/demo/s3-checkpoint-demo-2026-10-11-04296.md`로 닫고 ADR 인덱스·문서 인덱스·개발자 인덱스의 S3 상태를 맞췄다. 설계 `docs/superpowers/specs/2026-10-11-purecvisor-desktop-node-s4-lan-account-design.md`를 썼다. 코드 확인: LAN prefix는 `--allow-lan`과 token source를 요구하고 API·Web prefix는 각각 하나, `/pcv-config.js` `apiBaseUrl`은 API prefix authority, 방화벽 관리 규칙은 기본 7777만(Web 80은 따로), 계정은 삭제 명령이 없고 하나라도 있으면 loopback 세션이 닫힌다. 결정 D1~D5와 권장안(사용자 실제 PC 시연 + 에이전트 리허설, 제품 변경 없는 LAN prefix 재구성, 규칙 두 개와 loopback 복귀, 사용자 admin 계정)은 campaign `next_approval_required`에 있다. `pcvverify completion`(로컬 head) S3 `met=true`, S4 `false`. `dotnet test src/DesktopNode.Verification.Tests -c Release` 675/676(dirty tree PolicyBoundary 1건, commit 뒤 확인), `DesktopNode.Delivery.Tests` 781/781, `Update-PcvContractSpecPins.ps1 -Check` current.

## Task 2: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `train-04296-finish-20261011` Task 8(원래 `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR, green CI 뒤 merge.

## Task 3: S4 LAN 시나리오 스크립트 (설계 4절 task 1)

- [x] `web/scripts/run-s4-lan-scenario.mjs`와 `web/node-tests/s4-lan-scenario.test.mjs`를 추가하고 `web/package.json`에 `scenario:s4-lan`과 `test:web-contracts` 항목을 더한다. 시연 순서표는 `docs/ga-ready/demo/s4-lan-runbook.md`다. 기본은 plan-only이고 이 task는 `--execute`로 설치본에 요청을 보내지 않는다. 검증: `node web/scripts/run-s4-lan-scenario.mjs`, `node --test web/node-tests/s4-lan-scenario.test.mjs`, `npm run test:required --prefix web`, `npm run test:public-source-safety --prefix web`, `git diff --check`, clean HEAD에서 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `dotnet test src/DesktopNode.Verification.Tests -c Release`. push, PR, green CI 뒤 merge. host mutation 없음. S4 통과를 주장하지 않는다.

실행 기록(2026-10-12): `run-s4-lan-scenario.mjs`는 기본 plan-only이고 `--execute`만 요청을 보낸다. 비밀번호는 `PCV_S4_PASSWORD`만 읽으며 `--password`는 거절한다. 시연 순서표는 `docs/ga-ready/demo/s4-lan-runbook.md`다. `test:web-contracts`에 시험을 넣고 `web-verification-architecture-boundary.test.mjs`의 pin을 맞췄다. `node web/scripts/run-s4-lan-scenario.mjs` exit 0, `node --test web/node-tests/s4-lan-scenario.test.mjs` 9/9, `npm run test:required --prefix web` exit 0(`test:web-contracts` 273/273, parity 통과), `git diff --check` 통과. 새 파일 public source safety 스캔 발견 0. `npm run test:public-source-safety --prefix web`는 19/20이고 실패 1건은 기존 두 문서의 `network.observed-private-endpoint`(BL에 이미 있음)다. 설치본에 `--execute`를 보내지 않았다. S4 통과를 주장하지 않는다. Delivery·Verification 시험은 이 commit의 clean HEAD에서 돌린다.

보정(2026-10-12): 계정이 0개일 때 admin이 아닌 역할은 `409 PCV_ACCOUNT_BOOTSTRAP_ADMIN_REQUIRED`다. 런북은 사용자가 loopback에서 admin을 먼저 만들고, LAN 재구성 뒤에 에이전트가 operator를 만들도록 바꿨다. admin이 생기면 loopback 세션이 닫힌다. `pcvcli` 전역 옵션은 명령어 앞이고, loopback 단계는 `--protected-token-file <token-file>`, LAN 단계의 operator 생성은 `--api http://<host-lan-ip>:7777`을 함께 쓴다. disable은 loopback 복귀 다음이며 기본 주소와 `--protected-token-file`이다. Task 4 전체 실행은 `--skip-auto`이고 기대는 `result=pass-without-auto`다. R은 자동 checkpoint를 증명하지 않는다. 스크립트 cleanup은 남은 console VM을 delete 전에 poweroff하고, poweroff 실패는 `cleanup_error`에 남긴 뒤 delete를 계속한다. `createVm`은 `job_id`를 받은 직후 `createdVms`에 넣는다. `--skip-auto`는 s3이 선택됐을 때만 `pass-without-auto`다. 설계 1절 2항은 S3를 "checkpoint 생성·복원·예약"이라고 하면서 "기존 시나리오와 같은 판정 값"이라고도 한다. 기존 S3 통과는 자동 checkpoint를 기다리므로, 작업 지시서 3절(2026-10-12)은 사용자 시연 A의 S3를 예약 실행까지 확인하는 것으로 정했다. 런북 3절 표의 S3 확인 문장("예약 checkpoint가 생기고")은 그대로 두고, 표 아래에 `next_due_at`이 지나야 예약 checkpoint가 생긴다는 문장을 더했다. 리허설 R은 `--skip-auto`, `result=pass-without-auto`다. 검증 수치는 이 문단 아래 보정 검증에 적는다.

보정 검증(2026-10-12): `node web/scripts/run-s4-lan-scenario.mjs` exit 0(plan-only, 요청 없음). `node --test web/node-tests/s4-lan-scenario.test.mjs` 12/12. `npm run test:required --prefix web` exit 0(`test:web-contracts` 276/276, parity 통과). pin 시험은 실패하지 않았다. `npm run test:public-source-safety --prefix web`는 19/20이고 실패 1건은 기존 두 문서 `docs/ga-ready/demo/s1-browser-console-demo-2026-10-08.md`와 `docs/superpowers/plans/2026-10-08-purecvisor-desktop-node-s1-installed.md`의 `network.observed-private-endpoint`다. 새 발견은 없다. `git diff --check` 통과. host mutation 없음. S4 통과를 주장하지 않는다. Delivery·Verification 시험은 이 commit의 clean HEAD에서 돌린다.

## Task 8: `configure-installed` LAN prefix 옵션 (D2 정정 A′, Task 4 앞)

- [x] Lane 1 제품 변경. `DesktopNode.Host.exe service-action configure-installed|repair-installed`가 `--prefix`, `--web-prefix`를 받는다.
  - 옵션이 없으면 지금과 같은 service 명령줄이다(API `http://127.0.0.1:7777/`, Web `http://127.0.0.1:80/`, `--allow-lan` 없음).
  - 둘 중 하나라도 loopback이 아니면 `--allow-lan`이 필요하고(없으면 `PCV_HOST_PREFIX_NOT_LOOPBACK`), service 명령줄에 `--allow-lan`을 넣는다.
  - 거절: wildcard host(`+`, `*`, `0.0.0.0`, `[::]`), http가 아닌 scheme, API와 Web host 불일치, LAN에서 API port가 7777이 아닌 값
    (CSP `connect-src http://*:7777`). 형식 검증은 `listen` 모드와 같은 규칙을 쓴다.
  - 명령 경로(`DesktopNodeHostServiceAction.cs`)와 native 구성(`DesktopNodeHostServiceAction.ServiceConfig.cs`)이 같은 값을 쓴다. native
    구성은 지금처럼 현재 service의 `--api-token-credential-target`과 `--batch-evidence-root`를 유지한다.
  - `listen` 모드, 방화벽 ops, MSI custom action은 바꾸지 않는다.
  - 시험(`DesktopNode.Host.Tests`): 기본 명령줄 불변, LAN 명령줄의 prefix와 `--allow-lan`, `--allow-lan` 누락, wildcard, host 불일치,
    API port, 두 경로 일치, LAN에서도 credential target과 batch evidence root 유지.
  - 문서: `packaging/windows-desktop-node/README.md`의 service-action 옵션 설명(`--batch-evidence-root` 줄 근처)과, configure-installed
    인수를 고정하는 다른 문서·catalog·시험이 있으면 함께 맞춘다.
  - `docs/ga-ready/release-train.json` `queue`에 행을 더한다. area `host`, risk_tier `M`, `lane2_probe` family `host.service.configure-lan`,
    design은 S4 설계 문서다. 형식은 PR #86의 queue 행(`e31b180`)과 같다.
  - 검증: `dotnet test src/DesktopNode.Host.Tests -c Release`, commit 뒤 clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`,
    packaging Pester, `git diff --check`. push, PR, green CI 뒤 merge. host mutation 없음. S4 통과를 주장하지 않는다.

실행 기록(2026-10-12): `configure-installed|repair-installed`만 `--prefix`와 `--web-prefix`를 읽는다. 생략하면 service 명령줄은 API `http://127.0.0.1:7777/`, Web `http://127.0.0.1:80/`이고 `--allow-lan`이 없다. loopback이 아니면 `--allow-lan`이 필요하고 명령줄에 들어간다. 거절 코드는 wildcard `PCV_HOST_PREFIX_WILDCARD`, http가 아닌 scheme `PCV_HOST_PREFIX_SCHEME`, host 불일치 `PCV_HOST_PREFIX_HOST_MISMATCH`, `--allow-lan` 누락 `PCV_HOST_PREFIX_NOT_LOOPBACK`, LAN API port `PCV_HOST_PREFIX_API_PORT`다. `listen`은 `0.0.0.0`과 `--allow-lan`을 그대로 받는다. 방화벽 ops와 MSI custom action은 바꾸지 않았다. native 구성은 `--api-token-credential-target`과 `--batch-evidence-root`를 유지한다. `dotnet test src/DesktopNode.Host.Tests -c Release` 242/242. `release-train.json` queue 행은 PR 번호가 생긴 뒤 같은 branch에 넣는다. host mutation 없음. S4 통과를 주장하지 않는다. `dotnet test src/DesktopNode.sln -c Release`, packaging Pester, `git diff --check`는 이 commit의 clean HEAD에서 돌린다.

## Task 4: S4 리허설 R (설계 4절 task 2)

- [ ] Lane 2. 시작 조건: 사용자 admin 계정이 있고 Task 8이 merge됐다. admin 계정이 없으면 Task 4는 멈추고 사용자에게 요청한다. Task 8이 merge된 `main` 빌드본 `DesktopNode.Host.exe service-action configure-installed --prefix http://<host-lan-ip>:7777/ --web-prefix http://<host-lan-ip>:80/ --allow-lan`으로 설치 service를 재구성하고(전후 service 명령줄을 읽어 prefix와 `--allow-lan` 말고 바뀐 것이 없는지 확인), 방화벽 관리 규칙 두 개(7777, 80, Private, LocalSubnet, Web 규칙 이름 `PureCVisor Desktop Node Web LAN`)를 연다. 그 다음 에이전트 operator 계정을 실행 경계 비밀번호로 만든다. 이 호스트 브라우저로 `http://<host-lan-ip>/` 리허설을 하고 `run-s4-lan-scenario.mjs --phases=none` 뒤 전체 `--execute --skip-auto`를 1회 돌린다. 기대는 `result=pass-without-auto`다. R은 자동 checkpoint를 증명하지 않는다. 비밀번호, token, 실제 LAN IP는 기록하지 않는다. 끝 상태는 Task 6까지 유지한다.

## Task 5: S4 사용자 시연 A (설계 4절 task 3)

- [ ] Task 4 전에 만든 admin 계정으로 자기 PC 브라우저에서 S1~S3을 시연한다. 에이전트는 시연 전후 상태(service 구성, 방화벽 규칙, 계정 목록, VM)를 읽어 기록하고 확인자 칸은 사용자가 채운다. 이 기록만 "다른 PC" 시연이다. 리허설 R은 S4 판정 근거로 쓰지 않는다.

## Task 6: S4 끝 상태 (설계 4절 task 4)

- [ ] Lane 2. 끝 상태는 loopback 복귀(같은 `main` 빌드본 `configure-installed`를 prefix 옵션과 `--allow-lan` 없이 실행, Task 4 전 service 명령줄과 같아야 한다), 방화벽 규칙 0, 에이전트 operator 계정 disable, `pcv-it-s4-*` VM 정리다. 기록한다. 계정 파일 삭제는 이 task가 하지 않는다.

## Task 7: criteria S4 판정 (설계 4절 task 5)

- [ ] Lane 1. 시연 기록이 설계 1절의 판정을 모두 담으면 `config/project-completion-criteria.json` S4를 `passed`로 두고 시연 기록 링크를 적는다. push, PR, green CI 뒤 merge. Task 8 제품 변경이 queue에 있으므로 S4 판정 뒤 release train 1회(2026-10-09 사전 승인)를 다음 campaign으로 연다. 이 campaign은 train을 실행하지 않는다. 이 task가 끝나면 남은 Task 2(C5, 2026-10-19 이후)를 `next_task`로 둔다. 판정 전에는 S4 통과를 주장하지 않는다.

## Nonclaims

- S4 시연과 완료 판정 `complete=true`를 주장하지 않는다. operational current는 `0.42.96-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
- legacy 콘솔 제거는 이 campaign의 산출물이 아니다.
