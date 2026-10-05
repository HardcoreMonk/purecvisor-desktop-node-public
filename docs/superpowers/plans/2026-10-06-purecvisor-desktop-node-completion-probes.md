# 완료 조건 C4·C5 설치본 probe Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 완료 정의(`docs/superpowers/specs/2026-10-06-purecvisor-desktop-node-completion-definition-design.md`)의 남은 조건을 닫는다. C4는 설치본 `0.42.90`에서 P1-6, P1-7, P1-9, P2-11, P1-10을 operation 단위로 실제 확인해 닫고, C5는 2026-10-19 Ubuntu 26 runner 이동 뒤 첫 `main` CI로 닫는다.

**Architecture:** product payload를 바꾸지 않는 Lane 2 설치본 probe다(0.42.84 `lane2-development-completion-actual-vm-2026-09-30-04284`와 같은 방식). 결과는 operation 단위 evidence와 `docs/FEATURE_IMPLEMENTATION_LEDGER.md` "비후보 actual-VM 관측" 행으로 남기고 operational current는 바꾸지 않는다. branch `lane2/completion-probes-20261006` 하나, PR 하나. 결함이 나오면 그 항목은 Lane 1 수정과 다음 release train으로 넘기고 정차한다.

**Tech Stack:** 설치본 `pcvcli`(`C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`), Local API loopback, Hyper-V, `gh`

## 사용자 결정 (2026-10-06)

승인 원문: `1,2,3` (campaign `completion-definition-20261006` `next_approval_required`).

| 항목 | 범위 |
| --- | --- |
| 1 | Lane 2 설치본 probe(`0.42.90`, product payload 변경 없음): P1-6 inventory 시각·메모, P1-7 template lock, P1-9 admin account create/disable, P2-11 noVNC target preview/set/clear. host mutation은 probe VM 생성·삭제, probe 계정 생성·비활성화, noVNC target 설정 뒤 원복. PASS 행 기록, push, PR, green CI 뒤 merge |
| 2 | Lane 2 설치본 probe: P1-10 family reconcile(create, shutdown, restart, QoS). job을 서비스 프로세스 정지·재시작으로 끊고 reconcile. host mutation은 probe VM 생성·삭제와 서비스 재시작. evidence 기록, push, PR, green CI 뒤 merge |
| 3 | 2026-10-19 뒤 Ubuntu 26 `ubuntu-latest`에서 첫 `main` Development Gates와 Public Boundary run 확인(Lane 0/1, host mutation 없음). 실패하면 `ubuntu-24.04` pin을 판단 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. probe VM 이름은 `pcv-probe-c4-*`이고 끝나면 지운다.
- 계정이 하나도 없으면 probe 계정이 첫 admin이 되고 비활성화가 거절된다. 그 경우 계정을 만들지 않고 멈춘 뒤 묻는다. probe 계정 비밀번호는 실행 경계에서 무작위로 만들어 환경 변수로만 넘기고 기록하지 않는다. 비활성화된 probe 계정은 남는다(설계상 delete 없음).
- noVNC target은 시작 전 상태(`%ProgramData%\PureCVisor\desktop-node\novnc-target.json` 유무와 내용, `console capabilities`)를 기록하고 끝에 같은 상태로 되돌린다. LAN target을 설정하지 않는다.
- token, password는 command line, summary, evidence에 남기지 않는다.
- 설치본, service, 보존 VM의 끝 상태를 매 Lane 2 task 끝에 확인한다(service Running/Automatic, Web `200`, probe VM `0`).
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: P1-6, P1-7 probe

- [ ] probe VM을 만든다. `vm list`의 `created_at`, `last_powered_on`, `notes` readback(시작·종료 전후)을 확인한다. template-lock 뒤 start는 허용되고 직접 mutation(예: rename, memory 변경)은 거절되는지, template-unlock 뒤 다시 허용되는지 확인한다. probe VM을 지운다. evidence `lane2-completion-inventory-template-lock-actual-vm-2026-10-06-04290`. 로컬 commit.

## Task 2: P1-9, P2-11 probe

- [ ] 계정 목록을 먼저 본다(없으면 멈춤). probe 계정 `pcv-probe-c4-operator`(operator)를 만들고 목록에 나오는지, disable 뒤 상태가 바뀌는지 확인한다. noVNC target은 시작 상태를 기록하고 loopback target으로 preview, set, capabilities readback, clear(또는 원래 파일 복원)를 확인한다. evidence `lane2-completion-account-novnc-target-2026-10-06-04290`. 로컬 commit.

## Task 3: P1-10 family reconcile probe

- [ ] probe VM으로 `vm.create`, `vm.shutdown`, `vm.restart`, QoS mutation job을 각각 진행 중에 서비스 프로세스를 끊어 `PCV_JOB_INTERRUPTED`로 만들고 `job reconcile`이 provider readback으로 `postcondition-confirmed`(또는 설계가 정한 판정)를 내는지 확인한다. 끝나면 probe VM을 지우고 service 상태를 확인한다. evidence `lane2-completion-family-reconcile-actual-vm-2026-10-06-04290`. 로컬 commit.

## Task 4: 기록과 C4 판정

- [ ] `docs/FEATURE_IMPLEMENTATION_LEDGER.md` "비후보 actual-VM 관측" 절에 PASS operation 행을 더한다. 완료 정의 §4에 C4 판정 보정을 적는다(기존 감사 문서는 덮어쓰지 않는다). 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

## Task 5: 종료 검증과 merge

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`, Pester 네 종, `npm run test:required --prefix web`, Required CI 네 shard를 로컬로 돌린다. 로컬 commit 뒤 push, PR, green CI 뒤 merge.

## Task 6: C5 runner 확인 (2026-10-19 이후)

- [ ] 2026-10-19 이후에만 실행한다. 그 전에 이 task에 오면 기한 대기로 멈춘다. GitHub runner image가 Ubuntu 26으로 바뀐 뒤 첫 `main` Development Gates와 Public Boundary run이 green인지 확인한다. green이면 완료 정의 §4에 C5 충족과 프로젝트 완료 판정을 적고 campaign을 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. 로컬 commit, push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.90-admin-smoke` 그대로다. probe는 승격 근거가 아니다.
- public trusted signing과 external stable publication을 주장하지 않는다.
