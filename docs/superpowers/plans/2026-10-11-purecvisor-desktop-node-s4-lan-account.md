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

## Nonclaims

- S4 시연과 완료 판정 `complete=true`를 주장하지 않는다. operational current는 `0.42.96-admin-smoke` 그대로다.
- public trusted signing과 external stable publication을 주장하지 않는다.
- legacy 콘솔 제거는 이 campaign의 산출물이 아니다.
