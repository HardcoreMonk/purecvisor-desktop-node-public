# 개발 공정 최적화 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-10-06 개발 공정 단계별 점검(이중 검증)에서 찾은 낭비 네 가지를 줄인다. 시험이 남기는 headless Edge가 fullgate MSI 제거를 늦추는 문제(F1), 종료 검증의 중복 실행(F2), train 호스트 입력과 probe 스크립트의 수작업(F6)을 고치고, train과 Lane 3를 PR 하나로 합치는 방안(F3)과 실제 Hyper-V 어댑터 integration 단계(F5)는 설계 문서로만 정한다.

**Architecture:** 근거는 점검 실측이다. fullgate `msi-lifecycle-smoke`의 uninstall 두 번이 09-30 `40`초에서 10-06 `340`초로 늘었고(`summary.json` 단계 시각과 msiexec 로그 시작·종료 시각이 일치), 그 직전 살아 있던 `pcv-loopback-browser-*` headless Edge는 `9`→`15`→`33`→`43`개였다. 종료 검증 실측 `339`초 중 `68`초는 solution test와 `test:required`를 shard가 다시 도는 중복이다. branch `lane1/process-optimization-20261006` 하나, 마지막 task에서 PR 하나.

**Tech Stack:** C# xUnit(`DesktopNode.Host.Tests`), PowerShell 7 도구와 Pester 5(`packaging/windows-desktop-node`), `DesktopNode.Verification`(`pcvverify`), 문서(`docs/DEVELOPMENT_PROCEDURE.md`, `docs/superpowers/specs/**`), private 저장소 `.claude/skills/pcv-ship/SKILL.md`

## 사용자 결정 (2026-10-06)

승인 원문: `1,2,3,4` (개발 공정 단계별 점검 최종 보고의 다음 승인 후보에 대한 답). 이어서 열린 campaign 교체 질문에 `교체, Task 12 보존`, 권한 질문에 `push, PR, green CI 뒤 merge`로 답했다.

| 항목 | 범위 |
| --- | --- |
| 1 | F1: Host.Tests loopback browser 시험의 Edge 누수 수정(Lane 1)과 이 호스트에 남은 headless Edge 정리. 정리 대상은 `pcv-loopback-browser-*` user-data-dir를 쓰는 프로세스와 그 temp 폴더뿐이다 |
| 2 | F2: `pcv-ship` 종료 검증에서 shard와 겹치는 직접 실행 제거 |
| 3 | F6: current-card와 fullgate 입력을 train facts에서 만들고 Lane 2 probe 스크립트를 저장소에서 추적 |
| 4 | F3, F5: 설계 문서만 |
| 교체 | `train-04291-20261006`을 닫는다. Task 12(2026-10-19 뒤 C5 확인)는 2026-10-06 `1,2,3` 항목 3 승인 그대로 `next_approval_required`에 남겨 다시 연다 |
| 권한 | Lane 0/1, task마다 로컬 commit, 마지막에 push, PR, green CI 뒤 merge. host mutation(service, MSI, VM, firewall, 설치본)과 Lane 2/3는 없다 |

## Global Constraints

- 서비스, MSI, VM, firewall, 설치본, `current-evidence.json`을 바꾸지 않는다. F1의 인과 확인용 fullgate 재실행은 이 campaign 밖이다(별도 승인 필요).
- Edge 정리는 command line에 `pcv-loopback-browser-`가 있는 `msedge.exe`와 `%TEMP%\pcv-loopback-browser-*` 폴더만 대상으로 한다. 다른 user-data-dir를 쓰는 사용자 브라우저는 건드리지 않는다.
- probe 스크립트에 password, token, credential 상수를 두지 않는다. 실행 경계에서 만들고 환경 변수로만 넘긴다. 기존 artifact와 evidence는 고치지 않고, 저장소로 가져올 때는 복사본을 일반화한다.
- 새 packaging `*.Tests.ps1`나 도구는 Delivery inventory, verification migration manifest, contract spec pin(`Update-PcvContractSpecPins.ps1`)을 같은 commit에서 맞춘다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다). Lane 2·3 task는 없다.

## Task 1: F1 browser 시험 누수 수정과 정리

- [x] `src/DesktopNode.Host.Tests/DesktopNodeHostLoopbackBootstrapBrowserTests.cs`가 띄운 브라우저를 끝에 확실히 닫게 한다(DevTools `Browser.close`, 남은 프로세스는 그 user-data-dir를 command line에 가진 것만 종료, 폴더 삭제 재시도). 검증: `dotnet test src/DesktopNode.Host.Tests -c Release` 두 번 실행 뒤 새로 남은 `pcv-loopback-browser` 프로세스·폴더 `0`. 그다음 이 호스트에 남은 누수 인스턴스와 temp 폴더를 정리하고 개수를 기록한다. 로컬 commit.

실행 기록(2026-10-06): 남은 브라우저 main의 부모 프로세스는 모두 이미 없었다(예: PID `29616`의 부모 `38064`). `Process.Start`로 띄운 `msedge.exe`가 실제 브라우저를 다른 프로세스로 넘기고 끝나서 `finally`의 `HasExited`가 참이 되고 `Kill`이 돌지 않았다. 수정: 끝에 DevTools 브라우저 연결로 `Browser.close`를 보내고 10초 기다린 뒤, 그 user-data-dir를 command line에 가진 `msedge.exe`·`chrome.exe`만 `Win32_Process`(Host→HyperV 참조로 들어오는 `System.Management`)로 찾아 종료한다. 폴더 삭제는 재시도하고 시험 token 파일도 지운다. 본문이 PASS하면 남은 프로세스 `0`을 `Assert.Empty`로 확인한다. Release build 경고 `0`. `dotnet test src/DesktopNode.Host.Tests -c Release` 두 번 `216/216`, 실행 뒤 새 `pcv-loopback-browser` 프로세스 `0`, temp 폴더 `80`→`80`(새 폴더는 지워짐), token 파일 `69`→`69`. 정리: 누수 인스턴스 `47`개(프로세스 `470`개, 이 호스트 `msedge` `475`→`5`)와 temp 폴더 `80`개를 지웠다. report-only: 이전 실행이 남긴 `%TEMP%\pcv-browser-gate-token-*.txt` `69`개(합성 시험 token)는 승인한 정리 범위 밖이라 두었다. service, MSI, VM 변경 없음.

## Task 2: F2 종료 검증 중복 제거

- [x] 종료 검증 정의에서 Required CI shard가 이미 도는 직접 실행(`dotnet test src/DesktopNode.sln`, `npm run test:required`)을 뺀다. public `docs/DEVELOPMENT_PROCEDURE.md`에 같은 목록이 있으면 함께 고치고, 계획 실행 기록에 쓰는 assembly별 시험 수를 shard 결과에서 읽는 방법을 적는다. private `.claude/skills/pcv-ship/SKILL.md` 표를 맞춘다(private 저장소 로컬 commit). 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-06): public §4 clean-HEAD 검증은 이미 네 shard만 적고 있었고, `AGENTS.md`와 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`도 `test:required`를 따로 다시 돌리지 말라고 적는다. 중복은 private `pcv-ship` 표의 "위 셋과 Pester 네 종, Required CI 네 shard"에만 있었다. 그 행을 `dotnet build`, 네 shard, Pester 네 종으로 바꿨다(private 로컬 commit). public §4에는 넓은 변경·train 종료 검증에 더하는 build와 Pester 네 종의 경로, 그리고 assembly별 시험 수를 dotnet shard `summary.json` `results[0].standard_output`의 `- <assembly>.dll` 줄에서 읽는 방법을 적었다. 확인: 2026-10-06 측정 run의 dotnet shard stdout에 assembly `9`개의 통과 수가 모두 있다. 줄인 시간은 종료 검증마다 약 `68`초(solution test `42`초, `test:required` `23`초)다. contract spec pin `current`(변경 `0`), Delivery `763/763`, `git diff --check` 통과. report-only: shard stdout의 한국어 라벨이 코드페이지 문제로 깨져 기록된다(숫자와 assembly 이름은 온전함).

## Task 3: F6 설계

- [ ] `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-train-host-inputs-design.md`에 세 가지를 정한다. current-card 캡처를 저장소 도구로 옮겨 train facts에서 SHA 상수·provenance·version을 읽는 방법, fullgate batch manifest를 직전 manifest 복사 대신 train facts에서 만드는 방법, Lane 2 probe 스크립트의 위치·매개변수·비밀 값 경계·plan-only 시험. 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 4: F6 current-card 캡처 도구

- [ ] Task 3 설계대로 current-card 캡처를 저장소 도구로 옮기고 `docs/DEVELOPMENT_PROCEDURE.md` §10 train task 4를 새 도구로 바꾼다. 실제 캡처는 하지 않고 plan-only/Pester로 시험한다. 검증: 영향 Pester, `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 5: F6 fullgate manifest 생성

- [ ] Task 3 설계대로 fullgate batch manifest 생성을 도구로 만들고 §10 fullgate 단계를 바꾼다. 0.42.91 manifest를 golden으로 재현하는 시험을 둔다. fullgate는 실행하지 않는다. 검증: 영향 Pester 또는 C# 시험, Delivery, `git diff --check`. 로컬 commit.

## Task 6: F6 probe 스크립트 추적

- [ ] 2026-10-06 완료 probe(P1-6·P1-7, P1-9·P2-11, P1-10)에 쓴 스크립트를 Task 3 설계 위치로 가져와 매개변수로 일반화하고 공통 helper로 묶는다. 실행하지 않고 plan-only/구문 시험만 한다. 검증: 영향 Pester, Delivery, `git diff --check`. 로컬 commit.

## Task 7: F3 설계

- [ ] train과 Lane 3를 PR 하나로 합치는 설계를 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-single-pr-train-design.md`에 쓴다. 지금 Lane 3가 소비하는 main push CI evidence를 merge 전 PR CI run(head SHA 고정)으로 바꿀 때의 위험과 대안을 적는다. 설계만, `proposed`. 검증: Delivery, `git diff --check`. 로컬 commit.

## Task 8: F5 설계

- [ ] 실제 Hyper-V 어댑터를 in-process로 일회용 VM에 돌리는 opt-in integration 단계 설계를 `docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-hyperv-adapter-integration-tier-design.md`에 쓴다. 09-27 뒤 제품 수정 `9`건이 모두 실제 Hyper-V에서 발견된 근거, host mutation 정책과 ADR 필요 여부, 승인 경계를 적는다. 설계만, `proposed`. 검증: Delivery, `git diff --check`. 로컬 commit.

## Task 9: 종료 검증과 merge

- [ ] clean HEAD에서 Task 2로 바뀐 종료 검증 정의대로 돌린다. campaign을 닫고(`next_approval_required`에 train-04291 Task 12 재개와 F1 인과 확인용 fullgate 재실행 승인 후보) 로컬 commit한 뒤 push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.91-admin-smoke` 그대로다. package, pair, fullgate, current-card, Lane 2 probe, Lane 3를 실행하지 않는다.
- F1의 원인은 상관관계로만 확인했다. fullgate 단축은 다음 fullgate에서 확인한다.
- public trusted signing과 external stable publication을 주장하지 않는다.
