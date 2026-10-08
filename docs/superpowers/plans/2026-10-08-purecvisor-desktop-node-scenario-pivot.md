# 시나리오 기준 전환과 Hyper-V 브라우저 콘솔 spike Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 완료 기준을 증거 상태에서 시연 가능한 사용자 시나리오로 바꾸는 ADR-0017을 제안하고, 첫 시나리오의 가장 큰 공백인 브라우저 콘솔을 Hyper-V `vmconnect` 경로(TCP `2179`, RDP preconnection)로 풀 수 있는지 spike로 확인한다.

**Architecture:** 근거는 2026-10-08 대화의 진단이다. 2026-09-08~10-08 public commit은 `docs` 274, `feat` 69, `fix` 25개이고, 완료 기준 C1~C7 중 6개가 CI·증거·backlog 상태다. Web Console 콘솔은 `vmconnect` handoff뿐이다(`docs/USER_GUIDE.md` 콘솔/noVNC). spike는 기존 noVNC WebSocket-to-TCP bridge(ADR-0010)를 재사용할 수 있는지 본다. branch는 `lane1/scenario-pivot-20261008` 하나이고 PR 하나로 merge한다.

**Tech Stack:** `dotnet test`, PCVCLI 설치본(`pcvcli vm create/start/stop/delete`), 저장소 밖 scratch probe(소켓만 사용), `gh`

## 사용자 결정 (2026-10-08)

승인 원문: `1,2,3` (2026-10-08 대화 보고의 다음 승인 1~3번). 1 "권장안을 ADR 초안과 완료 정의 v3(시나리오 기준), 절차 축소안으로 public 저장소에 문서화(Lane 1, PR, green CI 뒤 merge)", 2 "단계 1 첫 작업으로 Hyper-V 브라우저 콘솔 spike. 2179 RDP 경유 방식 확인, 실제 VM이 필요하면 `pcv-it-` 접두사 VM만", 3 "VMware 26H1 설치 보류, 2번 결과를 보고 재판단". 같은 날 기존 campaign 처리 답: `Task 10만 이관`.

| 항목 | 범위 |
| --- | --- |
| 1 | ADR-0017 제안(완료 정의 v3, 절차 축소안) 문서화. Lane 0/1. host mutation 없음. push, PR, green CI 뒤 merge. 계약 파일(정책, criteria, `pcvverify`, AGENTS.md, 회로 차단기)은 바꾸지 않는다 |
| 2 | Hyper-V 브라우저 콘솔 spike. 조사는 Lane 1, probe는 Lane 2. host mutation은 `pcv-it-` 접두사 VM 생성, 시작, 중지, 삭제만(끝 상태 `pcv-it-` VM `0`개). 외부 소프트웨어 설치 없음 |
| 3 | VMware Workstation 26H1 설치 보류. 이 campaign은 설치하지 않고 Task 4가 재판단 근거를 적는다 |
| 이관 | `completion-20261008` Task 10만 Task 6으로 옮긴다. Task 11(완료 정의 v2 판정)은 v3 제안으로 대체되어 폐기한다. Task 6은 원래 `deadline-wait` 정책 행(Lane 0/1, push, PR, green CI 뒤 merge)을 따른다 |
| ADR-0016 | standing approval은 `pcv-it-` 접두사 VM 생성·삭제로 한정 |

## Global Constraints

- 보존 VM `pcv-guest-installed-04253-r1`의 전원, Notes, 디스크를 바꾸지 않는다. 새 VM은 `pcv-it-console-spike` 하나이고 Task 3 끝에 지운다.
- 2179 probe는 RDP 협상 응답까지만 본다. CredSSP 인증을 시도하지 않고 Windows 계정 비밀번호나 token을 쓰지 않는다.
- VMware, RDP gateway, Guacamole 등 외부 소프트웨어를 설치하지 않는다. Windows 기능(WHP)과 방화벽을 바꾸지 않는다. 새 `Add-Type`/`P/Invoke`가 필요해지면 멈춘다.
- 이 campaign의 새 문서는 ADR-0017과 spike 문서 둘뿐이다(ADR-0017 절차 축소안을 이 campaign부터 따른다). evidence 문서와 ledger 행은 만들지 않는다.
- ADR-0017 채택 여부가 정해지기 전에는 큐가 끝나도 `pcv-campaign` §6 completion 연쇄로 새 campaign을 열지 않고 멈춘다.
- 범위 밖 발견은 backlog `undecided` 행으로 쓴다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회, Lane 3 30분·tool batch 12회(checkpoint마다).

## Task 1: ADR-0017 제안

- [x] `docs/adr/0017-scenario-delivery-completion.md`를 상태 `제안`으로 쓰고 `docs/ADR_INDEX.md`의 제안 후보 줄을 고친다. 맥락(위 수치), 결정(완료 정의 v3: 시나리오 S1~S4, 단계 완료는 설치본 시나리오 스크립트 1회 통과와 시연 기록), 절차 축소안(train은 단계 완료 때만, 평소 기능 PR은 Lane 1 + Required CI + 설치본 smoke 하나, 기능 PR당 plan/evidence 문서 1개 이하, pin·ledger·index 동기화의 자동화나 폐지 후보, completion autopilot 정지), 채택 때 바뀌는 계약 목록을 적는다. 검증 `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. 로컬 commit.

실행 기록(2026-10-08): `docs/adr/0017-scenario-delivery-completion.md`(상태 `제안`, 결정 마커 4개, 시나리오 S1~S4, 절차 축소 6항, 채택 때 바뀌는 계약 6묶음)와 `docs/ADR_INDEX.md` 제안 후보 줄을 썼다. 계약 파일은 바꾸지 않았다. host mutation 없음.

## Task 2: 브라우저 콘솔 방식 조사

- [ ] `docs/superpowers/specs/2026-10-08-purecvisor-desktop-node-hyperv-browser-console-spike.md`를 쓴다. Hyper-V `vmconnect` 경로(TCP `2179`, RDP preconnection PDU의 VM GUID, CredSSP)와 브라우저 쪽 후보(기존 noVNC WebSocket-to-TCP bridge + 브라우저 RDP client, Windows용 HTML5 RDP gateway, Apache Guacamole)를 라이선스, Windows service 적합성, 인증 처리, 저장소 경계(ADR-0006, ADR-0010, Linux stack 금지)로 비교하고 Task 3 probe 절차를 확정한다. 검증 Delivery tests, `git diff --check`. 로컬 commit.

## Task 3: 2179 probe (Lane 2)

- [ ] PCVCLI(설치본 service 경로)로 `pcv-it-console-spike` VM을 만들고 켠다. 저장소 밖 scratch probe로 `127.0.0.1:2179`에 preconnection PDU v2(VM GUID)와 X.224 Connection Request(RDP_NEG_REQ)를 보내 응답(선택 protocol, 실패 코드)을 기록하고, GUID 없음·틀린 GUID와 대조한다. VM을 끄고 지워 `pcv-it-` VM `0`개를 확인한다. raw 결과는 `artifacts/console-spike-20261008/`, 요약은 spike 문서 probe 절에 적는다. 로컬 commit.

## Task 4: 판단 기록

- [ ] spike 문서에 결론을 쓴다. 브라우저 콘솔 구현 방식 권장안 하나, S1 구현 task 초안, VMware 26H1 설치 재판단(승인 3), 남은 위험과 다음 승인 문장. 검증 Delivery tests, `git diff --check`. 로컬 commit.

## Task 5: 종료와 PR

- [ ] clean HEAD에서 `dotnet test src/DesktopNode.Delivery.Tests -c Release`와 `git diff --check origin/main...HEAD`를 돌리고 push, PR, green CI 뒤 merge한다. Task 4의 다음 승인 문장을 campaign `next_approval_required`에 둔다.

## Task 6: C5 runner 확인 (2026-10-19 이후)

- [ ] `not_before` 2026-10-19. 이관 전 `completion-20261008` Task 10이다. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 보고하고 멈춘다. push, PR, green CI 뒤 merge.

## Nonclaims

- operational current는 `0.42.93-admin-smoke` 그대로다. 제품 payload를 바꾸지 않는다.
- ADR-0017은 제안이다. 채택 전까지 완료 정의 v2, `pcvverify completion`, autopilot 정책 파일은 그대로다.
- spike는 브라우저 콘솔의 구현이나 지원을 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
