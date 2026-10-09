# 감사 개선안 green-lean Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-10-09 제로베이스 감사 보고서의 개선안 1~6을 실행한다. 두 저장소 `main`을 green으로 돌리고, 작업 저장소를 public 하나로 모으고, 목적 문장과 시연 기록 규칙을 고정하고, 기능 PR의 동기화 고정비와 WMI 경로 검증 공백을 줄인다. S3·S4 campaign은 열지 않는다.

**Architecture:** 감사 보고서(Claude Doc `6081d005-6fee-464e-b762-16335ad8f111`) §10~§13이 근거다. public `main` red는 `ApiConsoleFrameRouteTests.FrameRouteLimitsTheSameVmTo100Milliseconds`의 타이밍 flake(기대 429, 실제 200)이며 frame throttle이 `Environment.TickCount64`를 직접 읽는다. `DesktopNodeApiRequestProcessor`에는 이미 `Func<DateTimeOffset>? Clock` 옵션이 있다. Verification 실행기는 실패 테스트명을 `summary.json`에만 담고 CI 로그에 쓰지 않는다. private `main` red(Release 전용 Verification 테스트 1건과 ratchet 2건)는 private snapshot 고유 문제라 고치지 않고 private를 archive한다. ADR-0016 통합 테스트 `src/DesktopNode.HyperV.IntegrationTests`와 생성기 `Update-PcvCurrentEvidenceDocs.ps1`, `Update-PcvCurrentEvidenceLedgerRows.ps1`, `New-PcvPromotionIndexSections.ps1`가 이미 있다. branch는 `lane1/audit-green-lean-20261009`(public)와 `archive/private-readonly-20261009`(private)다.

**Tech Stack:** .NET 10 (`dotnet test -c Release`), PowerShell 7 + Pester 5, Node.js, git/gh

## 사용자 결정 (2026-10-09)

승인 원문: `보고서 추가, - 1번과 2번 승인 - 3~6번 승인 - 승인: 2번의 두 저장소 push와 PR, 5·7번의 pcv-it- VM 생성(ADR-0016 standing approval 문장), 8번의 설치본 --allow-lan과 방화벽 변경(host mutation).` (감사 보고서 §13 개선안 표 1~8번에 대한 답. 7·8번 작업은 미승인이고 mutation 사전 승인만 locator에 적는다.)

| 항목 | 범위 |
| --- | --- |
| 1 | public flake 1건을 clock 주입으로 결정적으로 수정, Verification 실행기가 실패 테스트명을 CI 로그에 출력, `pcv-ship`에 "merge 뒤 `main` run green 확인" 추가. Lane 1, 로컬 commit. |
| 2 | private 저장소를 read-only archive로 선언하고 skill 4종과 `CLAUDE.md`를 public으로 이전, AGENTS에 "public 수록 금지: 사용자 경로·LAN IP·호스트명" 한 줄. Lane 1. 두 저장소 branch push와 PR 허용. merge는 승인 문장에 없어 열지 않는다. |
| 3 | 목적 문장을 "Windows Hyper-V 기반 가상화 관리 계층"으로 정정하고 WHP는 v2 후보 한 줄. Lane 1, 로컬 commit. |
| 4 | ADR-0017 §2.4 동기화 4항목 처분: feature ledger 행은 생성기, ledger·index 절과 Lane 3 pin은 train 전용 도구, current-card 유지. Lane 1, 로컬 commit. |
| 5 | 로컬 PR gate 스크립트(Release 테스트 + ratchet + ADR-0016 `pcv-it-` 통합 테스트). Lane 1 작성, Lane 2 실행 1회. host mutation은 ADR-0016 범위(`pcv-it-` 접두사 VM 생성·설정·삭제, `artifacts/hyperv-integration/<run id>/` 아래 디스크)로 한정한다. standing approval 문장: "5·7번의 pcv-it- VM 생성(ADR-0016 standing approval 문장)". |
| 6 | 시연 기록 템플릿에 확인자·일시 칸, 캡처는 `docs/ga-ready/demo/<id>/`에 보존. Lane 1, 로컬 commit. |
| 7·8 | 작업 미승인. "8번의 설치본 --allow-lan과 방화벽 변경(host mutation)" 사전 승인은 locator에만 남기고 이 campaign은 쓰지 않는다. |
| 이관 | `s2-template-clone-20261009` Task 2(C5 runner 확인, `not_before` 2026-10-19)를 Task 9로 옮긴다. 문장과 push, PR, green CI 뒤 merge는 원래 승인 그대로다. |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`과 template VM `pcv-it-s2-source`, `current-evidence.json`, `release-train.json`의 `trains`를 바꾸지 않는다. 통합 테스트가 만드는 VM은 `pcv-it-<run id>-` 접두사뿐이고 끝에 `pcv-it-` 신규 VM 0개여야 한다.
- guest 인증 정보와 token은 기록하지 않는다. public 문서에는 사용자 경로, LAN IP, 호스트명을 적지 않는다.
- product payload를 바꾸는 Task 1은 같은 PR에서 `release-train.json` `queue`에 한 행을 더한다(절차 §10).
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다). Lane 3는 열지 않는다.

## Task 1: public flake 수정과 실패 테스트명 출력

- [x] `DesktopNodeApiConsoleRouteHandler.Frame.cs`의 throttle이 `Environment.TickCount64` 대신 processor `Clock`을 쓰게 하고, `ApiConsoleFrameRouteTests.FrameRouteLimitsTheSameVmTo100Milliseconds`는 고정 clock으로 429를 결정적으로 검증한다(100 ms 뒤 200도 한 건 추가). `VerificationExecutor`가 실패 suite의 `Failed <test>`와 `Error Message` 줄을 stderr에 써서 CI 로그에 남기고 테스트를 더한다. `release-train.json` `queue`에 이 PR 행을 더한다. 검증 `dotnet test src/DesktopNode.Api.Tests -c Release`(3회), `dotnet test src/DesktopNode.Verification.Tests -c Release`, `Invoke-Pester packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1`. 로컬 commit.

실행 기록(2026-10-09): frame throttle이 `DesktopNodeApiHardeningOptions.Now()`(ms since epoch)를 읽고 `DesktopNodeApiRequestProcessor`가 hardening options를 console handler에 넘긴다. 테스트는 고정 clock으로 200, 429, 99 ms 뒤 429, 100 ms 뒤 200을 검증한다. 실패 줄 echo는 `VerificationExecutor`가 아니라 TextWriter를 가진 `VerificationApplication.WriteFailedSuiteDiagnostics`에 두었고, 실패·시간 초과 suite의 `Failed <name>`, `실패 <name>`, `[-] <name>` 줄만 `failed-test suite=<id> test=<name>`과 `failed-suite suite=<id> status=<s> exit=<n> failed_tests=<n>` 줄로 stderr에 쓴다(suite당 50줄, sanitizer 적용). `Error Message` 줄은 임의 출력이라 echo하지 않는다. 검증: `dotnet test src/DesktopNode.Api.Tests -c Release` 517/517(frame 테스트 3회 모두 5/5), `dotnet test src/DesktopNode.Verification.Tests -c Release` 670/671(남은 1건 `PolicyBoundaryMatchesCanonicalActivationState`는 dirty tree 기대 실패, commit 뒤 clean HEAD에서 재확인), ratchet Pester 3/3, `git diff --check` 통과. `release-train.json` queue 행은 PR 번호가 필요해 Task 8에서 더한다(전례 `44e51dd`).

## Task 2: skill과 CLAUDE.md를 public으로, 작업 저장소 하나로

- [x] private `.claude/skills/{pcv-campaign,pcv-campaign-open,pcv-ship,pcv-goal}/SKILL.md`를 public `.claude/skills/`로 옮기고 `$pub` 절대 경로를 저장소 루트 기준으로 고친다. private `CLAUDE.md`를 public `CLAUDE.md`로 옮기되 기존 gstack 라우팅 블록은 뒤에 보존하고 "현재 evidence는 public이 소유" 절을 맞춘다. `pcv-ship`에 "merge 뒤 `main` 사후 run green 확인" 단계를 더한다. `AGENTS.md` 저장소 경계에 "public 수록 금지: 사용자 경로, LAN IP, 호스트명" 한 줄. 사용자 메모리 파일은 `~/.claude/projects/D--data-projects-codex-zone-purecvisor-desktop-node/memory/`에서 `...-public/memory/`로 복사한다(저장소 밖, commit 없음). 검증 `git diff --check`, `Invoke-Pester packaging/windows-desktop-node/tests/PcvAgentExecutionCircuitBreaker.Tests.ps1`, `Invoke-Pester packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`. 로컬 commit.

실행 기록(2026-10-09): skill 4종을 `.claude/skills/`에 두고 `$pub` 정의를 "이 저장소 루트"로 바꿨다. `pcv-ship` §3에 6번 단계(모든 merge에서 merge commit의 `main` push run을 `gh run watch --exit-status`로 기다리고 red면 revert PR 또는 보고 후 정지)와 §1에 "C# 시험은 `-c Release`" 문장을 더했고 §4 `goal-evidence`에 `main_run=`을 넣었다. `CLAUDE.md`는 private 지침을 이 저장소 기준으로 옮기고(§2 campaign 경로, §3 public 수록 금지·PowerShell Direct 예외, §4 HostOps 설명 정정과 Verification·skills 행, §5 `-c Release`와 PolicyBoundary 함정) 기존 gstack 라우팅은 §8로 보존했다. `AGENTS.md` 저장소 경계에 public 수록 금지와 작업 저장소 단일화 두 줄을 더했다. 메모리 21개 파일과 `.claude/settings.local.json`(gitignore)을 복사했다. 검증: `git diff --check` 통과, Pester 93/93(`PcvAgentExecutionCircuitBreaker` + `PcvAdminSmokeEvidenceDocs`), `Update-PcvCurrentEvidenceDocs.ps1 -Check` ok. 다음 세션부터는 이 저장소를 cwd로 열어야 skill과 메모리가 보인다.

## Task 3: private 저장소 archive 선언 (private, push·PR)

- [x] private 저장소 `README.md`, `AGENTS.md`, `CLAUDE.md` 맨 위(생성 블록 밖)에 "2026-10-09부터 read-only archive. 작업·skill·campaign은 `purecvisor-desktop-node-public`" 안내를 넣고 skill 디렉터리에 같은 안내 `README.md`를 둔다. branch `archive/private-readonly-20261009`, 검증 `git diff --check`, `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `Invoke-Pester packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`. 로컬 commit, push, PR. merge는 `next_approval_required`.

실행 기록(2026-10-09): private 저장소 branch `archive/private-readonly-20261009`, commit `636f0647`, PR #190(`https://github.com/HardcoreMonk/purecvisor-desktop-node/pull/190`) open. 세 문서의 H1 바로 아래, 생성 블록 밖에 archive 안내 blockquote를 넣었고 `.claude/skills/README.md`에 동결 사본 안내를 뒀다. 검증: `git diff --check` 통과, `PcvAdminSmokeEvidenceDocs` Pester 90/90, `Update-PcvCurrentEvidenceDocs.ps1 -Check` ok. private `main` CI는 손대지 않았다(red 유지, 유지보수 종료). merge는 `next_approval_required` 1번이다.

## Task 4: 목적 문장과 시연 기록 규칙

- [x] public `README.md`와 `AGENTS.md` 저장소 경계에 "Windows Hyper-V 기반 가상화 관리 계층(WMI `root\virtualization\v2`). WHP 자체 VMM은 범위 밖, v2 후보" 한 문단. `docs/ga-ready/demo/TEMPLATE.md`에 시나리오, 설치본 version, 스크립트 결과, 캡처 경로 `docs/ga-ready/demo/<id>/`, 확인자와 확인 일시 칸을 두고 `docs/DEVELOPMENT_PROCEDURE.md` 시연 기록 문장에 캡처 보존 규칙을 더한다. 검증 `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `Invoke-Pester packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`. 로컬 commit.

실행 기록(2026-10-09): `README.md` 184행 아래에 제품 정의 문단, `AGENTS.md` 저장소 경계 둘째 줄에 제품 정의 bullet을 넣었다. `docs/ga-ready/demo/TEMPLATE.md`(설치본, 실행, 화면 캡처 보존 규칙, 확인자·확인 일시·확인 방법 표, 정리, 판정)를 만들고 `docs/DEVELOPMENT_PROCEDURE.md` §5 끝에 시연 기록 규칙 문단(S3부터 적용)을 더했다. 기존 S1·S2 기록은 바꾸지 않았다. 검증: `git diff --check` 통과, Pester 93/93(`PcvAdminSmokeEvidenceDocs` + `PcvAgentExecutionCircuitBreaker`), `Update-PcvCurrentEvidenceDocs.ps1 -Check` ok.

## Task 5: 동기화 4항목 처분과 feature ledger 생성기

- [x] `docs/DEVELOPMENT_PROCEDURE.md`에 처분 표(feature ledger 행 = 생성기 `-Check`, `CURRENT_EVIDENCE_LEDGER.md`·`EVIDENCE_INDEX.md` 절 = train Lane 3 도구 전용, Lane 3 spec SHA pin = train 전용, current-card = 유지)를 적고 기능 PR은 이 네 항목을 손으로 고치지 않는다고 명시한다. `packaging/windows-desktop-node/tools/Update-PcvFeatureLedgerDoc.ps1`이 `config/desktop-node-feature-surface-ledger.json`에서 `docs/FEATURE_IMPLEMENTATION_LEDGER.md`의 Feature ID 표를 생성하고 `-Check`를 지원하며, Pester 테스트와 CI `packaging-pester` job의 `-Check` 단계를 더한다. 기존 Pester 계약이 설계 변경을 요구하면 멈추고 보고한다. 검증 `Invoke-Pester packaging/windows-desktop-node/tests`. 로컬 commit.

실행 기록(2026-10-09): `Update-PcvFeatureLedgerDoc.ps1`(계약 `pcv-feature-ledger-doc-v1`, 마커 `<!-- BEGIN/END GENERATED FEATURE ID SUMMARY -->`, `-Check`는 쓰지 않음)을 만들고 `docs/FEATURE_IMPLEMENTATION_LEDGER.md`의 Feature ID 요약 표를 마커로 감쌌다. 생성 결과는 기존 표 29행과 byte 단위로 같았다(`-Check` `status=current`). Required CI는 PowerShell을 돌리지 않으므로 CI gate는 `packaging-pester` 단계가 아니라 Delivery 시험 `PcvFeatureLedgerDocContractTests`(같은 렌더링으로 표 대조 + 도구 source 계약)로 두었고, 도구 동작 시험은 packaging Pester inventory 밖인 `manual-admin-tests/PcvFeatureLedgerDoc.Tests.ps1` 4건(current, stale 거부, write, 마커 검증)이다. `DEVELOPMENT_PROCEDURE.md` §3에 처분 표를 적었다. 검증 중 Delivery 시험 50건이 `source-sha`로 실패해 원인을 찾았다: `AGENTS.md`는 `pcv-development-policy-contract-spec-v1.json`의 pin 대상인데 Task 2·4가 pin 없이 고쳤다. `Update-PcvContractSpecPins.ps1 -Apply`로 spec pin과 verifier 상수를 갱신했고 그 규칙을 `CLAUDE.md` §7에 적었다. 검증: Delivery 777/777(Release), manual-admin Pester 4/4, 생성기 `-Check` ok, pins `-Check` current, `git diff --check` 통과. 범위 좁힘: 같은 문서의 82-route surface 투영 표는 아직 손으로 쓴다(backlog BL 행).

## Task 6: 로컬 PR gate 스크립트

- [x] `packaging/windows-desktop-node/tools/Invoke-PcvPrGate.ps1`: `dotnet test src/DesktopNode.sln -c Release`, `PcvModuleSizeRatchet.Tests.ps1`, `-Integration` 스위치면 `PCV_HYPERV_INTEGRATION_APPROVAL`을 확인하고 `dotnet test src/DesktopNode.HyperV.IntegrationTests -c Release`를 돌린다. 결과는 `artifacts/pr-gate/<yyyymmdd-HHmmss>/summary.json`. `-PlanOnly`는 명령만 출력한다. Pester는 plan-only와 승인 없는 `-Integration` 거절만 시험한다. `pcv-ship` push 전 단계에 이 스크립트를 넣는다. 검증 `Invoke-Pester packaging/windows-desktop-node/tests/PcvPrGate.Tests.ps1`, `Invoke-PcvPrGate.ps1 -PlanOnly`. 로컬 commit.

실행 기록(2026-10-10): `Invoke-PcvPrGate.ps1`(계약 `pcv-pr-gate-v1`)은 build → `dotnet test src/DesktopNode.sln -c Release --no-build` → ratchet Pester(in-process) → `-Integration`이면 `dotnet test src/DesktopNode.HyperV.IntegrationTests -c Release` 순서로 돌고, 첫 실패에서 멈춘다. `-Integration`은 `PCV_HYPERV_INTEGRATION_APPROVAL`이 active campaign `approval_locator` 안 문자열(20자 이상)일 때만 열리고 아니면 아무것도 돌리지 않고 exit 2다(`PCV_PR_GATE_INTEGRATION_APPROVAL_MISSING` / `_NOT_IN_LOCATOR`). `summary.json`에 step별 status·exit_code·duration_ms와 `host_mutation_performed`를 적는다. Pester는 packaging inventory 밖 `manual-admin-tests/PcvPrGate.Tests.ps1` 4건(plan-only 3단계, 승인 변수 없음 거절, locator 밖 문장 거절, locator 문장 수락 + host_mutation 표시)이고, CI gate는 Delivery `PcvPrGateContractTests` 2건(source 계약, integration 단계가 `-Integration` guard 아래)이다. `pcv-ship` §1에 3번 단계(PR 전 gate 실행, Hyper-V 어댑터 변경이면 standing approval로 `-Integration`)를 넣었다. 허용 파일 추가: `src/DesktopNode.HyperV.IntegrationTests/HyperVIntegrationFixture.cs`의 잔재 검사를 `pcv-it-` 전체에서 run 접두사 `^pcv-it-[0-9]{14}-`로 좁혔다. S2가 남긴 Off template `pcv-it-s2-source`가 있으면 fixture가 시작을 거부해 Task 7이 돌 수 없었기 때문이며, Dispose의 before/after VM 이름 비교는 그대로다. 검증: `-PlanOnly` exit 0, 승인 없는 `-Integration -PlanOnly` exit 2, manual-admin Pester 4/4, Delivery 779/779(Release), IntegrationTests 빌드 성공, `git diff --check` 통과. host mutation 없음.

## Task 7: PR gate 실행 1회 (Lane 2)

- [ ] 이 호스트에서 `PCV_HYPERV_INTEGRATION_APPROVAL`을 campaign locator 안 문자열로 두고 `Invoke-PcvPrGate.ps1 -Integration`을 한 번 돌린다. 끝에 `pcv-it-` 신규 VM 0개, 보존 VM Off, `summary.json` `ok=true`를 확인하고 이 plan에 실행 기록을 적는다. 실패하면 원인을 기록하고 멈춘다. 로컬 commit(기록만).

실행 기록(2026-10-10 00:58~01:00 KST, FAIL): `-Integration -PlanOnly`로 승인 판정 `ok`를 확인한 뒤 `Invoke-PcvPrGate.ps1 -Integration`을 1회 돌렸다. build 7.6초, `dotnet test src/DesktopNode.sln -c Release` 41초 전건 통과(Api 517, Verification 671, Delivery 779, HyperV 275, Host 216, Contracts 200, Cli 183, Runtime 129, Service 11), ratchet 3/3, 통합 시험 2건 중 1건 FAIL(33초): `VmPowerStateVocabularyTests.CreatedOffVmReadsAsStoppedAndPassesTheSharedOffCheck`가 `vm.create` 성공 직후 `vm.list`에서 `PCV_NATIVE_VM_LIST_IDENTITY_STATE_INCOMPLETE`를 받았다. `VmConsoleBrowserPathTests`는 통과. run id `20261009155949`. 생성한 `pcv-it-20261009155949-off`는 fixture가 지웠고 `artifacts/hyperv-integration/20261009155949.summary.json`은 VM 이름 before/after 동일(`pcv-it-s2-source`, `pcv-guest-installed-04253-r1`), storage root 제거, cleanup_problems 0이다. 끝 상태: run 접두사 VM 0개, 보존 VM Off, template Off. gate `artifacts/pr-gate/task7-run/summary.json`은 `ok=false`, `integration_approval=ok`, `host_mutation_performed=true`. 원인 후보: `DesktopNodeHyperVNativeAdapter.Reads.cs`의 `TryInvokeVmList`는 한 행이라도 id/name/state가 비면 inventory 전체를 fail-closed로 거절하는데, 막 정의한 VM의 WMI 행이 잠시 불완전하다(create 반환 시점과 전이 상태 어휘). fixture 시작 시 `vm.list`는 성공했으므로 template VM 때문은 아니다. Lane 2 FAIL이라 current에 쓰지 않고 멈춘다. backlog BL-0014. 수정은 Lane 1 승인(`next_approval_required` 4) 뒤 이 task를 다시 돈다.

## Task 8: 종료 검증과 push·PR

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, `npm test --prefix web`, `npm run verify:parity --prefix web`, packaging·installer·web Pester, `Update-PcvCurrentEvidenceDocs.ps1 -Check`, `git diff --check`. `lane1/audit-green-lean-20261009`를 push하고 PR을 연다. merge는 `next_approval_required` 1번이다.

## Task 9: C5 runner 확인 (2026-10-19 이후, 이관)

- [ ] `not_before` 2026-10-19. 이관 전 `s2-template-clone-20261009` Task 2(그 전 `s1-installed-20261008` Task 11, `adr17-s1-console-20261008` Task 14)다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.93-admin-smoke` 그대로다. Lane 3와 `current-evidence.json` 쓰기는 없다.
- S3·S4 시연과 완료 판정 v3 `complete=true`를 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
