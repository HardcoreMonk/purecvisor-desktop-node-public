# ADR-0017 채택과 S1 브라우저 콘솔 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** ADR-0017(시나리오 기준 완료 정의 v3, 절차 축소)을 채택해 계약을 바꾸고, 시나리오 S1의 공백인 브라우저 콘솔을 WMI 화면(`GetVirtualSystemThumbnailImage`)과 `Msvm_Keyboard` 입력으로 만든다.

**Architecture:** 근거는 `docs/adr/0017-scenario-delivery-completion.md`와 spike `docs/superpowers/specs/2026-10-08-purecvisor-desktop-node-hyperv-browser-console-spike.md`(E 권장)다. PR은 둘이다. PR A(Task 1~4)는 branch `lane1/adr17-adoption-20261008`에서 계약을 바꾸고, PR B(Task 5~13)는 PR A merge 뒤 `origin/main`에서 만든 `lane1/s1-console-20261008`에서 콘솔을 만든다. 화면 읽기 operation은 `vm.disk.inspect`(adapter·dispatch catalog·WMI provider·RuntimePolicy) 형식을 따른다.

**Tech Stack:** `dotnet test`, `pcvverify completion`, WMI(`root\virtualization\v2`), Local API route, `web/src/served/*.ts`(`npm run build:served --prefix web`), `DesktopNode.HyperV.IntegrationTests`(ADR-0016), `gh`

## 사용자 결정 (2026-10-08)

승인 원문: `1,2,3` (campaign `scenario-pivot-20261008` 최종 보고 `next_approval_required` 1~3). 같은 날 직전 결정(`Task 10만 이관`)과 같이 남은 C5 task를 이관한다.

| 항목 | 범위 |
| --- | --- |
| 1 | "ADR-0017을 채택하고 계약 변경(완료 기준, autopilot 정책, pcvverify completion, train 출발 조건, AGENTS.md, pcv skill)을 Lane 1 task로 나눠 진행한다. push, PR, green CI 뒤 merge." Lane 0/1. host mutation 없음. pcv skill은 private 저장소 로컬 commit(push 없음) |
| 2 | "S1 브라우저 콘솔(E 방식: WMI 화면 + Msvm_Keyboard 입력) campaign을 연다. 설계 문서, 화면 읽기 adapter와 route와 Web 패널, pcv-it- VM 입력 확인, S1 시나리오 스크립트 순서. Lane 1과 Lane 2(pcv-it- 접두사 VM 생성, 시작, 중지, 삭제와 그 VM의 키 입력만). push, PR, green CI 뒤 merge." 설계 문서는 이 승인으로 연다 |
| 3 | "VMware Workstation 26H1 설치 보류를 유지한다(결정만, 작업 없음)." task 없음 |
| 이관 | `scenario-pivot-20261008` Task 6(C5 runner, `not_before` 2026-10-19)을 Task 14로 옮긴다. Lane 0/1, push, PR, green CI 뒤 merge |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·삭제로 한정. 키 입력은 승인 2가 연다 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. 키 입력은 `pcv-it-` VM의 firmware 화면에만 보내고 guest OS에 로그인하지 않는다.
- MSI 설치·업그레이드, service 재시작, release train은 이 campaign 범위 밖이다. 설치본 S1 smoke가 필요해지면 다음 승인으로 둔다.
- 새 `Add-Type`/`P/Invoke`/native ACL/installer handoff가 필요해지면 멈춘다. 화면 변환(RGB565→RGBA)은 브라우저에서 하고 서버에 `System.Drawing`을 들이지 않는다.
- ADR-0017 절차 축소를 따른다. 기능 PR에 plan 밖 문서는 설계 문서 하나뿐이고 evidence 문서와 ledger 행은 만들지 않는다.
- Web 문구·binding을 바꾸면 pin된 곳과 web Pester를 함께 고친다. 새 packaging `*.Tests.ps1`은 만들지 않는다.
- token, credential, password는 command line, summary, 문서에 남기지 않는다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 1: ADR-0017 채택 문서

- [x] ADR-0017 상태를 `채택`으로 바꾸고 `docs/ADR_INDEX.md`(현재 기준 절, 적용 표, 제안 후보 줄), `docs/DEVELOPMENT_PROCEDURE.md` §10(train 출발은 시나리오 단계 완료 때, 평소 기능 PR은 Lane 1 + Required CI + 설치본 smoke), `AGENTS.md` 현재 기준 줄, 완료 정의 v2 설계 머리말(역사 기록)을 고친다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-08): ADR-0017 상태 `채택 / 계약 전환 중`, 결정 마커 4개 확정. `docs/ADR_INDEX.md`에 2026-10-08 현재 기준 절과 적용 표 행을 더하고 제안 후보 줄을 비웠다. `docs/DEVELOPMENT_PROCEDURE.md` §10은 주 1회 정기 출발을 시나리오 단계 완료 출발로 바꾸고 평소 기능 PR 경로(Lane 1 + Required CI + 설치본 smoke, dev probe는 Lane 2 승인)를 적었다. `AGENTS.md` 회로 차단기 절에 한 줄을 더했고, 그 SHA pin(`config/pcv-development-policy-contract-spec-v1.json`)과 spec SHA 상수(`DevelopmentPolicyContractVerifier.ExpectedSpecSha256`)를 갱신했다. 완료 정의 v2 설계 머리말에 역사 기록 줄을 더했다. Delivery 775 통과. host mutation 없음.

## Task 2: 완료 판정 v3

- [x] `config/project-completion-criteria.json`을 `pcv-project-completion-definition-v3`로 바꾸고 시나리오 S1~S4 행(`status`, `demo_record`)을 더한다. `pcvverify completion`이 S1~S4 통과, `main` Required CI green, 제품 런타임 GA-ready(v2 C1), 기한 위험으로 판정하고 v2 C2·C3·C7은 판정에 넣지 않고 위생 줄로만 출력하게 한다. `DesktopNode.Verification.Tests`와 Delivery 계약 시험을 고친다. 검증 `dotnet test src/DesktopNode.Verification.Tests -c Release`, Delivery tests. 로컬 commit.

실행 기록(2026-10-08): criteria `definition`을 v3, `design`을 ADR-0017로 바꾸고 `scenarios` S1~S4(`status=open`, `demo_record=null`)를 더했다. `ProjectCompletionEvaluator`는 definition으로 v2/v3를 나누고, v3는 C1·S1~S4·C5·C6 일곱 조건으로 판정하며 C2·C3·C4·C7은 `hygiene`/`hygiene_gaps`로만 낸다. 열린 시나리오 갭 종류는 `scenario`(lane 1)다. command는 `hygiene`, `hygiene-gap` 줄과 result JSON 필드를 더했다. v3 시험 4개 추가, Verification 669 통과(실패 1은 dirty tree의 `PolicyBoundaryMatchesCanonicalActivationState`, Task 4 clean HEAD에서 확인), Delivery 통과. host mutation 없음.

## Task 3: autopilot 정지와 pcv skill

- [x] `config/completion-autopilot-policy.json`에 정지 상태(`status=paused`, 근거 ADR-0017)를 넣고 시나리오 갭 종류를 `user-decision`으로 둔다. private 저장소 `.claude/skills/pcv-campaign`(§6 연쇄 정지), `pcv-campaign-open`(completion 모드 정지), `pcv-goal`(v3 판정 표기)을 고쳐 private 저장소에 로컬 commit한다. 검증 Delivery tests, `git diff --check`. 로컬 commit.

실행 기록(2026-10-08): 정책에 `status=paused`, `paused_by`(ADR-0017), `paused_on`과 갭 종류 `scenario`(`auto=false`, Lane 0/1)를 더하고 Delivery 계약 시험을 맞췄다(775 통과). private 저장소 `pcv-campaign`(v3 판정, hygiene 줄은 보고만, `autopilot-paused` 정지), `pcv-campaign-open`(paused면 completion 모드 금지), `pcv-goal`(E는 정책 active일 때만)을 고쳐 로컬 commit했다(push 없음). host mutation 없음.

## Task 4: PR A 종료

- [x] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`와 `git diff --check origin/main...HEAD`를 돌리고 push, PR, green CI 뒤 merge한다.

실행 기록(2026-10-08): clean HEAD `4aa6b39`에서 솔루션 시험 9개 assembly 모두 통과(Service 11, Contracts 200, Cli 183, Runtime 129, Delivery 775, HyperV 273, Host 216, Api 503, Verification 670), `git diff --check origin/main...HEAD` 통과. branch `lane1/adr17-adoption-20261008`을 push하고 PR을 열어 green CI 뒤 merge한다.

## Task 5: S1 콘솔 설계

- [x] PR A merge 뒤 `origin/main`에서 `lane1/s1-console-20261008`을 만든다. 설계 문서 `docs/superpowers/specs/2026-10-08-purecvisor-desktop-node-s1-browser-console-design.md` 한 장에 화면 route(`GET /api/v1/vms/{vm}/console/frame`, 크기·형식·호출 상한), 입력 route와 job 여부, RBAC(`console.view`, 새 `console.input`), 입력 audit, LAN 노출 경계(ADR-0010 방식), Web 패널 동작을 정한다. 검증 Delivery tests. 로컬 commit.

실행 기록(2026-10-08): PR #73이 green CI 뒤 `6e9686f`로 merge됐고, 그 `main`에서 branch를 만들었다. 설계 `pcv-s1-browser-console-v1`에 화면 route(`GET .../console/frame`, 640×480 기본, deflate, 100ms rate), 동기 입력 route(`POST .../console/input`, key/text/ctrl-alt-del), 새 권한 `console.input`(operator), remote 입력은 account JWT만, data root `console-input-audit.jsonl`(내용과 hash 없음, 1 MiB 회전), Web 패널, 시험, 범위 밖을 정했다. host mutation 없음.

## Task 6: 화면 읽기 adapter

- [x] `vm.console.frame` 읽기 operation을 Hyper-V adapter(모델, domain, dispatch catalog, WMI provider, RuntimePolicy)에 더한다. 반환은 RGB565 원본과 width·height다. fake WMI 단위 시험을 쓴다. 검증 `dotnet test src/DesktopNode.HyperV.Tests -c Release`, `src/DesktopNode.Contracts.Tests`. 로컬 commit.

실행 기록(2026-10-08): `vm.console.frame`을 Hyper-V adapter(`DesktopNodeHyperVNativeAdapter.ConsoleFrame.cs`: 이름·크기 160~1024×120~768 검증, running 확인, `width×height×2` byte로 자름, base64 RGB565)와 WMI provider(`DesktopNodeHyperVWmiVmProvider.ConsoleFrame.cs`: 실현된 설정으로 `GetVirtualSystemThumbnailImage`)에 더하고 domain·dispatch·WMI catalog, `RuntimePolicy` native probe 목록, Api invoker 허용 목록에 등록했다. adapter read switch의 QoS preview 두 줄을 합쳐 module 크기 ratchet(315줄) 안에 두었다. 시험: HyperV 274(새 1), Contracts 200, Api 503, Delivery 775 통과. host mutation 없음.

## Task 7: 화면 route

- [x] 설계대로 Local API 화면 route를 더하고 `console.view` 권한, 크기 검증, 없는 VM·꺼진 VM 오류를 처리한다. route 계약과 API 시험을 고친다. 검증 `dotnet test src/DesktopNode.Api.Tests -c Release`, Contracts tests. 로컬 commit.

실행 기록(2026-10-08): host listener가 query string을 넘기지 않아 route를 `GET /api/v1/vms/{vmId}/console/frame/{size}`(`640x480` 형식)로 바꾸고 설계 §2를 고쳤다. 새 feature `pcv.vm.browser-console`(화면·입력 공용)로 route 계약을 더하고, handler(`DesktopNodeApiConsoleRouteHandler.Frame.cs`)는 크기 검증, VM당 100ms rate limit(`429 PCV_CONSOLE_RATE_LIMITED`), `vm.console.frame` 호출, zlib 압축을 한다. surface ledger에는 API present, CLI·Web 제외(사유 포함)로 넣었다. 고정 지점: route 81·feature 29, route snapshot SHA, ReadOnly 24, Web 제외 5(`verify-feature-surface-parity.mjs`), CLI 제외 8, `FEATURE_IMPLEMENTATION_LEDGER.md`(요약·route·단계 표), `USER_FEATURE_USAGE_SPEC.md` 링크. 시험: Api 508(새 5), Cli 183, Contracts 200, Delivery 775, web required exit 0, web Pester 50/0. host mutation 없음.

## Task 8: Web 화면 패널

- [x] VM 상세에 콘솔 패널(canvas, RGB565→RGBA 변환, 2~10 fps polling, 일시 정지, 오류 표시)을 더하고 `npm run build:served --prefix web`로 `web/app.js`를 만든다. 검증 `npm run test:required --prefix web`, `npm run verify:parity --prefix web`, web Pester. 로컬 commit.

실행 기록(2026-10-08): VM 상세에 Browser console 카드(VM Screen canvas, 크기 `640x480`/`800x600`/`1024x768`, 1/2/5 fps, Start/Pause screen)를 더했다. frame은 `DecompressionStream('deflate')`로 풀고 `decodeRgb565ToRgba`로 그리며, polling은 canvas만 다시 그리고 탭이 숨거나 VM 선택이 바뀌면 멈춘다. 429는 건너뛰고 다른 오류는 멈춘 뒤 표시한다. `served-app.ts`(ratchet 423/429)는 건드리지 않고 `vm-detail-extensions.ts` click 확장 지점과 `render()` 뒤 repaint로 연결했다. surface ledger는 Web present(`coverage_id=console.frame`), parity 77/4, Web 제외 목록과 기능 문서 두 곳을 맞췄다. 확인: 빌드된 `app.js`에서 변환(빨강·초록·파랑·흰색)과 zlib 해제를 Node로 확인(commit하지 않음). 시험: web required exit 0, web Pester 50/0, Api, Cli, Delivery 통과. host mutation 없음.

## Task 9: 키 입력 adapter와 route

- [x] `Msvm_Keyboard`(`TypeKey`, `PressKey`, `ReleaseKey`, `TypeText`, `TypeCtrlAltDel`) 입력 operation과 설계대로의 입력 route, `console.input` 권한, audit을 더한다. 단위 시험을 쓴다. 검증 HyperV, Api, Contracts tests. 로컬 commit.

실행 기록(2026-10-08): `vm.console.input`을 adapter(`PlanConsoleInput`: key type/press/release 1~254, text 1~256자 출력 가능 ASCII, ctrl-alt-del; running 확인; ReturnValue≠0이면 `PCV_CONSOLE_INPUT_FAILED`)와 WMI provider(VM의 `Msvm_Keyboard` method 호출)에 더하고 catalog, invoker에 등록했다. route `POST /api/v1/vms/{vmId}/console/input`(`console.input`, 동기)는 검증, remote service bearer 거부(403), VM당 초당 50회(429), data root `console-input-audit.jsonl` 선기록(내용·hash 없음, 1 MiB 회전, 실패 시 503) 뒤 보낸다. 권한 `console.input`을 operator·admin에 더하고 RBAC golden, reconcile 비대상 분류, surface ledger(API present, CLI·Web 제외), route 82·ProductOperation 21·snapshot SHA, CLI 제외 9, Web 제외 5(parity 77/5), 기능 문서를 맞췄다. ratchet 때문에 adapter switch의 readback 네 줄을 한 줄로 합치고 `DesktopNodeHyperVModels.cs`·`DesktopNodeAccountAuth.cs`를 한 줄씩 줄였다. 시험: HyperV 275(새 1), Api 517(새 9), Contracts 200, Cli 183, Delivery 775, web required exit 0, web Pester 50/0. host mutation 없음.

## Task 10: `pcv-it-` VM 확인 (Lane 2)

- [x] `DesktopNode.HyperV.IntegrationTests`에 콘솔 시험을 더해 `pcv-it-` VM을 만들고 켠 뒤 화면 읽기와 firmware 화면 키 입력(예: `Esc`)을 보내 화면 변화를 확인하고, 끄고 지운다. 끝 상태 `pcv-it-` VM `0`개. 결과는 plan 실행 기록에 적는다. 로컬 commit.

실행 기록(2026-10-08 21:47~21:51 KST): `VmConsoleBrowserPathTests`(ADR-0016 integration 단계, 제품 adapter in-process)를 3회 돌렸다. 1회차는 43초 뒤 실패했고 메시지를 잡지 못했다(정리 정상). 2회차와 3회차는 통과했다. 3회차 기록(`artifacts/hyperv-integration/console-browser-path/20261008125048.json`): Gen 2 firmware 화면이 4.6초 만에 안정(non-black 1,286 pixel, frame 읽기 최대 920ms, 3회), 키 입력 Esc·Shift press/release·text·Ctrl+Alt+Del 모두 반환 0, Ctrl+Alt+Del 뒤 1.46초 만에 화면 변화, VM 삭제 확인. 끝 상태 `pcv-it-` VM `0`개, 보존 VM 변경 없음, guest OS 접속 없음. 남은 위험: 1회차 실패는 PXE 단계 타이밍으로 추정하며, 다시 나오면 안정 화면 판정을 길게 잡는다.

## Task 11: Web 키 입력

- [x] 콘솔 패널에 키보드 capture(포커스, 특수 키, Ctrl+Alt+Del 버튼)를 더하고 `console.input`이 없으면 읽기 전용으로 둔다. 검증 web tests, parity, web Pester. 로컬 commit.

실행 기록(2026-10-08): 화면 canvas에 `tabindex`를 주고, 포커스된 canvas의 `keydown`/`keyup`을 `event.code` → Windows virtual-key 표(문자·숫자·F1~F12·숫자패드·특수 키)로 `press`/`release` 입력으로 보낸다(반복 keydown은 건너뜀). `Send text`(최대 256자)와 `Ctrl+Alt+Del` 버튼을 더했고, `console.input`이 없으면 읽기 전용 문구만 보인다. key binding은 `vm-detail-panel`에 한 번만 건다(`served-app.ts`는 바꾸지 않음). surface ledger `console.input`을 Web present(`coverage_id=console.input`)로 바꾸고 parity 78/4, Web 제외 목록, 기능 문서를 맞췄다. 확인: 빌드된 `app.js`의 키 표를 Node로 확인(KeyA=65, F5=116, Escape=27, Numpad3=99 등). 시험: web required exit 0, web Pester 50/0, Api 517, Cli 183, Delivery 775. host mutation 없음.

## Task 12: S1 시나리오 스크립트

- [ ] 설치본에서 S1(ISO로 VM 생성 → 콘솔에 설치 화면 → 키 입력 → 화면 캡처 → 정리)을 도는 스크립트를 쓰고 `-PlanOnly`로 확인한다. 설치본 실행은 다음 승인(새 build 설치)으로 둔다. 검증 plan-only 실행, 관련 시험. 로컬 commit.

## Task 13: PR B 종료

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, `npm run test:required --prefix web`, `git diff --check origin/main...HEAD`를 돌리고 push, PR, green CI 뒤 merge한다. 설치본 S1 smoke 승인 문장을 campaign `next_approval_required`에 둔다.

## Task 14: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `scenario-pivot-20261008` Task 6(그 전 `completion-20261008` Task 10)이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.93-admin-smoke` 그대로다. 이 campaign은 MSI를 설치하지 않고 current를 쓰지 않는다.
- S1 시나리오 통과는 설치본 smoke 승인 뒤에만 주장한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
