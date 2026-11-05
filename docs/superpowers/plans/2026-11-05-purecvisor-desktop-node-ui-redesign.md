# Web Console UI/UX 재설계 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** S4 사용자 시연(2026-11-05)에서 드러난 Web Console의 배치·상호작용 결함을 고친다. VM 상세를 탭으로 나누고, 복제·삭제·복원을 앱 안 모달로 바꾸고, 콘솔을 맞춤·전체 화면으로 쓸 수 있게 하고, 새 VM 마법사와 템플릿·계정 화면, 비밀번호 재설정을 더한다. 옛 콘솔은 동결했다가 퇴역한다. 같은 campaign에서 `s4-lan-account-20261011`의 남은 task(시연 A, 끝 상태, S4 판정, C5 확인)를 이관해 이어 간다.

**Architecture:** 설계 근거는 grok 조사(2026-11-05, purecvisor-single `c45dbf5`와 이 저장소 `cbe5459` 비교)와 Claude 설계안이다. Desktop Node는 ADR-0018(`single-edge-ui-v1`)로 purecvisor-single의 셸·Supanova 테마·모듈 순서를 이미 들여왔으므로 기술 기반은 그대로 두고 화면 구성과 상호작용 규칙을 바꾼다. 구현 방식은 새 셸 모듈 점진 교체다: `web/index.html`, `web/src/modules/*.ts`, `web/src/bootstrap.ts`, `web/style.css`를 화면 단위로 고치고 `modules.json` 구조와 MSI payload 파일 이름은 유지한다. 옛 콘솔(`web/index.legacy.html`, `web/app.js`, `web/src/served-app.ts`, `web/src/served/*.ts`)은 Task 9 전까지 고치지 않는다. 그래서 옛 콘솔을 읽는 계약(`web/contracts/web-static-contracts.mjs` `verifyVmLifecycleActions` 등)은 그대로 통과한다. Linux 전용 화면·코드(컨테이너, ZFS, OVS/OVN, noVNC, VNC WebSocket, TOTP)는 가져오지 않는다. 설계 문서는 Task 1이 `docs/superpowers/specs/2026-11-05-purecvisor-desktop-node-ui-redesign-design.md`로 쓴다. branch는 단계마다 `lane1/ui-redesign-<단계>-<yyyymmdd>`이고 첫 branch는 `lane1/ui-redesign-20261105`다.

**Tech Stack:** TypeScript Web Console(`web/src`, `npm run build:served`), node 시험(`web/node-tests`), Playwright, axe-core, C# Local API·PCVCLI(Task 8), Pester(Task 9)

## 사용자 결정 (2026-11-05)

승인 원문: `1,2,3,4,5` (Web Console 재설계안의 승인 요청 1~5번에 대한 답)

| 항목 | 범위 |
| --- | --- |
| 1 | 정보 구조: 사이드바 6개 묶음(템플릿·계정 신설)과 VM 상세 6개 탭(요약, 콘솔, 체크포인트, 하드웨어, 게스트, 작업 기록) |
| 2 | 화면 설계 방향 W1~W7(VM 목록, VM 상세 요약과 더 보기 메뉴, 복제 모달, 콘솔 탭, 체크포인트 탭, 새 VM 마법사, 삭제 확인 모달). 세부 간격과 문구는 구현 PR 스크린샷으로 다시 검토한다 |
| 3 | 구현 방식: 새 셸 모듈 점진 교체. 옛 콘솔은 동결 뒤 Task 9에서 퇴역하며, 2026-10-10 승인 3의 legacy-retirement campaign을 이 campaign에 합친다 |
| 4 | 비밀번호 재설정 API를 이번 범위에 포함한다(Task 8: API·CLI·Web) |
| 5 | campaign을 열고 Task 1(P0)부터 시작한다. S4 시연과 트레인 출발 순서는 그대로 두고 병행한다 |
| campaign 운영 | 같은 날 질문 답 `UI 캠페인 + S4 이관`: 이 campaign을 활성으로 열고 `s4-lan-account-20261011`의 Task 5·6·7·2를 Task 11·12·13·14로 이관한다 |
| 차선 | Lane 1 개발. 단계마다 push, PR. merge는 PR마다 사용자의 merge 승인 문장을 받은 뒤 한다(`merge_policy=none`) |
| Lane 2 | 이관한 Task 12만. 범위는 S4 campaign의 기존 승인 그대로다: 설치 service를 merge된 `main` 빌드본 `DesktopNode.Host.exe` `configure-installed`로 loopback 복귀, 방화벽 관리 규칙 두 개 제거, 에이전트 operator 계정 disable, ADR-0016 `pcv-it-s4-*` VM 정리 |
| 트레인 | 2026-11-05 승인 `1,2`의 2(트레인 출발)는 S4 판정(Task 13) 뒤다. 그때 트레인을 이 campaign 안 task로 넣을지, 이 campaign을 잠시 닫고 train campaign을 열지 `next_approval_required`로 묻는다 |

## 이관 기록 (2026-11-05)

| 새 task | 원래 task | 상태 |
| --- | --- | --- |
| Task 11 | `s4-lan-account-20261011` Task 5 (S4 사용자 시연 A) | 진행 중. 로그인, S1(생성·콘솔 입력 반영) 확인. S2에서 복제 이름 칸 배치 결함으로 멈췄고, 자원 격자의 `Target VM name` 칸으로 우회한다 |
| Task 12 | 같은 campaign Task 6 (S4 끝 상태) | 대기 |
| Task 13 | 같은 campaign Task 7 (criteria S4 판정) | 대기 |
| Task 14 | 같은 campaign Task 2 (C5 runner 확인, `not_before` 2026-10-19) | 날짜 조건 지남. 대기 |

## Global Constraints

- 설치본은 operational current `0.42.96-admin-smoke`다. 이 campaign은 `current-evidence.json`을 쓰지 않는다. 보존 VM `pcv-guest-installed-04253-r1`과 template `pcv-it-s2-source`는 건드리지 않는다.
- 옛 콘솔 파일은 Task 9 전까지 바꾸지 않는다. 새 셸 변경이 옛 계약 시험을 깨면 멈추고 보고한다.
- 새 UI 문구는 i18n 키로 둔다. ko가 기본이고 en은 같은 키로 맞춘다(`web/node-tests/i18n-dictionary.test.mjs`).
- 새 서드파티 자산(폰트, 아이콘, 라이브러리)을 넣지 않는다. 넣어야 하면 멈추고 고지 변경과 함께 묻는다.
- public 문서에는 사용자 홈 경로, LAN 사설 IP, 호스트명, token·password 값을 적지 않는다. LAN 주소는 `<host-lan-ip>`로 쓴다.
- 계정 비밀번호와 token은 실행 경계에서 만들고 명령줄·summary·evidence·캡처에 남기지 않는다.
- 역할: Claude는 설계·작업 분배·리뷰, grok은 기능 조사와 구현이다. grok 결과는 Claude가 diff와 시험으로 확인한 뒤 commit한다.
- 한도: Lane 1 30분·tool batch 18회, Lane 2 45분·tool batch 12회(checkpoint마다).

## Task 1: 설계 확정 (P0)

- [x] Claude. 설계 문서 `docs/superpowers/specs/2026-11-05-purecvisor-desktop-node-ui-redesign-design.md`(진단, 원칙, 정보 구조, W1~W7, 디자인 체계, 구현 방식, 단계)를 쓰고, VM 상세 탭과 상호작용 규칙(필수 입력 첫 칸, 요청 전 검사, 진행 표시, 앱 안 확인, 한국어 우선)을 ADR-0019로 남긴다(ADR-0018 유지). `docs/ADR_INDEX.md`에 줄을 더하고, `web/DESIGN.md`의 남색 권장 토큰과 `0.42.74`·`promotion_eligible=false` 문장을 구현 기준(Supanova 기본, 현재 증거는 생성 블록 참조)으로 고친다. VM 목록 응답에 template lock 값이 있는지 확인해 W1 템플릿 표시에 API 보강이 필요한지 설계 문서에 적는다. 검증: `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `npm run test:required --prefix web`, `git diff --check`, clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`. push, PR. merge는 사용자 승인 뒤.

실행 기록(2026-11-05): 설계 문서와 ADR-0019(결정 마커 4개)를 썼고 `docs/ADR_INDEX.md`에 2026-11-05 절을 더했다. `web/DESIGN.md`는 `0.42.74`·`promotion_eligible=false` 문장을 current-evidence 참조로 바꾸고, 권장 token을 `supanova` 기본값(`web/style.css:2271-2284`)으로 고치고, ADR-0019 상호작용 규칙 절을 더했다. grok API 조사: `GET /api/v1/vms` 항목에 `template_lock`(잠겼을 때만 직렬화)이 있어 W1 템플릿 배지·필터와 템플릿 화면은 API 변경이 필요 없다. 복제 대상 이름 규칙은 `IsValidHyperVName`(1~128자, 앞뒤 공백·제어 문자·`/`·`\`·`.`·`..` 금지)이고 생성은 `IsValidVmCreateName`(1~63자)이다. 설계안 페이지의 복제 도움말 문구를 이 규칙으로 고쳤다. 계정은 재설정 route가 없고 access JWT 무효화 수단이 없어 부록 A에 `credential_version`/`cv` claim 설계를 넣었다. 검증: `git diff --check` 통과, `npm ci --prefix web` 뒤 `npm run test:required --prefix web` exit 0(eslint 경고 1건, 오류 0), `dotnet test src/DesktopNode.Delivery.Tests -c Release` 781/781. 새 문서에 사설 IP·홈 경로·호스트명 없음. 솔루션 전체는 이 commit의 clean HEAD에서 돈다. host mutation 없음.

## Task 2: 공통 컴포넌트 (P1)

- [ ] grok 구현, Claude 리뷰. 새 셸에 확인 모달(제목, 결과 설명, 이름 입력 확인 옵션, 포커스 가둠, Esc), 입력 칸 검사와 칸 아래 오류 표시, 알림, job 진행 줄(접수·실행 중·완료·실패), 탭, 빈 상태와 읽기 실패 구분 컴포넌트를 `web/src/modules`에 더하고 `modules.json` 순서를 맞춘다. node 시험으로 각 컴포넌트의 상태와 키보드 동작을 고정한다. 검증: `npm run build:served --prefix web`, `npm run test:required --prefix web`, `git diff --check`. push, PR. merge는 사용자 승인 뒤.

## Task 3: VM 상세 탭, 헤더 조작, 복제 모달 (P2)

- [ ] grok 구현, Claude 리뷰. VM 상세를 6개 탭으로 나누고 마지막 탭을 기억한다. 헤더에는 상태 배지, 전원 조작, 콘솔 열기, 더 보기 메뉴(복제, 템플릿 잠금, 내보내기, vmconnect, 강제 끄기, 삭제)를 둔다. 자원 격자는 하드웨어 탭으로 옮긴다. 복제는 모달 하나로 바꾼다: 새 VM 이름이 첫 칸이고 `<원본>-clone`을 제안하며, 규칙·중복을 요청 전에 검사하고 `clone/preview` 결과를 모달 안에 보인 뒤 시작한다. 새 셸에서 복제의 `window.confirm`을 없앤다. 빈 이름이면 API를 부르지 않고, 이름이 있으면 `{confirm_name, name}`으로 preview 다음 clone을 부르는 시험을 더한다. 검증: Task 2와 같고, 탭 전환 가시성 시험을 포함한다. push, PR. merge는 사용자 승인 뒤.

## Task 4: 콘솔 탭 (P3)

- [ ] grok 구현, Claude 리뷰. 기본 프레임을 `1024x768`으로 바꾸고, 창에 맞춤(기본)과 실제 크기, 전체 화면(Fullscreen API), Ctrl+Alt+Del, 갱신 fps 선택, 키보드 입력 상태 표시를 더한다. frame·input API는 그대로 쓰고 noVNC·WebSocket은 넣지 않는다. 맞춤 배율과 입력 상태를 시험으로 고정한다. 검증: Task 2와 같다. push, PR. merge는 사용자 승인 뒤.

## Task 5: 체크포인트 탭과 예약 즉시 반영 (P4)

- [ ] grok 구현, Claude 리뷰. checkpoint 목록·만들기·복원·삭제를 탭으로 옮기고 복원·삭제는 확인 모달로 바꾼다. 예약 저장·해제와 checkpoint 생성·복원 job이 끝나면 선택 VM 상세를 다시 읽고 탭을 다시 그린다(BL-0020). 빈 목록과 읽기 실패를 구분한다. 검증: Task 2와 같고, 저장 직후 카드가 갱신되는 시험을 포함한다. BL-0020은 PR 번호로 닫는다. push, PR. merge는 사용자 승인 뒤.

## Task 6: 새 VM 마법사 (P5)

- [ ] grok 구현, Claude 리뷰. 생성 dialog를 네 단계(기본, 자원, 설치 미디어와 네트워크, 확인)로 바꾼다. 1단계는 이름 규칙과 중복을 통과해야 다음으로 간다. 예시 경로를 초기값으로 채우지 않는다. 스위치 목록은 `network.inventory`에서 읽는다. 만들기 뒤 job 진행 줄을 보인다. 검증: Task 2와 같다. push, PR. merge는 사용자 승인 뒤.

## Task 7: 사이드바 개편, 템플릿·계정 화면, 한국어화 (P6a)

- [ ] grok 구현, Claude 리뷰. 사이드바를 운영·워크로드·인프라·관제·시스템·도움말 묶음으로 바꾸고 템플릿 화면(잠긴 VM 목록, 복제 주 조작)과 admin 전용 계정 화면(목록, 생성, 비활성화)을 진단에서 분리한다. 상세 화면의 영어 고정 문구를 i18n 키로 옮긴다. Task 1이 template 표시에 API 보강이 필요하다고 적었으면 그 변경은 이 task 앞에 별도 task로 승인받는다. 검증: Task 2와 같다. push, PR. merge는 사용자 승인 뒤.

## Task 8: 비밀번호 재설정 (P6b)

- [ ] Claude가 먼저 짧은 설계(누가 누구의 비밀번호를 바꾸는지, 본인 변경, 감사 기록, 마지막 admin 보호, 세션 무효화)를 Task 1 설계 문서 부록으로 쓰고, grok이 Local API route, 권한, PCVCLI `account` 하위 명령, 계정 화면을 구현한다. 새 route는 route·operation·permission 고정값(카탈로그, snapshot SHA, Host fixture, ratchet 등)을 같은 PR에서 맞춘다. 제품 payload 변경이므로 `release-train.json` `queue` 행을 더한다. 검증: `dotnet test src/DesktopNode.sln -c Release`(clean HEAD), `npm run test:required --prefix web`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`, `git diff --check`. push, PR. merge는 사용자 승인 뒤.

## Task 9: 옛 콘솔 퇴역 (P7, legacy-retirement)

- [ ] grok 구현, Claude 리뷰. `web/index.legacy.html`, `web/app.js`, `web/styles.css`, `web/src/served-app.ts`, `web/src/served/*.ts`를 걷어 내고, 그 파일을 읽는 계약 시험(`web-static-contracts.mjs`, parity manifest, browser fixture), Pester, `web/payload-manifest.json`과 `WebPayload.wxs`(`Update-PcvWebPayloadWix.ps1 -Apply`), 서비스 워커 프리캐시를 새 셸 기준으로 옮긴다. 시작 전에 옛 콘솔에만 있는 기능이 남았는지 grok이 목록을 내고, 남은 것이 있으면 멈추고 묻는다. 검증: `npm run test:required --prefix web`, `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"`, `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'"`, clean HEAD에서 `dotnet test src/DesktopNode.sln -c Release`. 제품 payload 변경이므로 queue 행을 더한다. push, PR. merge는 사용자 승인 뒤.

## Task 10: 품질 게이트와 종료 검증 (P8)

- [ ] grok 구현, Claude 리뷰. axe 접근성 검사를 새 셸 주요 화면에 걸고, 화면 전환·VM 탭·모달을 브라우저 시험으로 고정하며, 화면별 스크린샷 기록을 남긴다. clean HEAD 종료 검증(Required CI 네 shard 로컬 실행 또는 `Invoke-PcvPrGate.ps1`). push, PR. merge는 사용자 승인 뒤. 설치본 반영과 S1~S3 재시연은 release train으로 하며 `next_approval_required`에 둔다.

## Task 11: S4 사용자 시연 A (이관: `s4-lan-account-20261011` Task 5)

- [ ] 원래 문장: Task 4 전에 만든 admin 계정으로 자기 PC 브라우저에서 S1~S3을 시연한다. 에이전트는 시연 전후 상태(service 구성, 방화벽 규칙, 계정 목록, VM)를 읽어 기록하고 확인자 칸은 사용자가 채운다. 이 기록만 "다른 PC" 시연이다. 리허설 R은 S4 판정 근거로 쓰지 않는다. 2026-11-05 변경: 사용자가 `pcv-admin` 비밀번호를 잊어 사용자 요청으로 새 admin `pcv-admin2`를 만들고(비밀번호는 사용자 전용 파일로만 전달), 다른 PC 로그인 확인 뒤 `pcv-admin`을 disable했다. 시연 기록은 `docs/ga-ready/demo/s4-lan-demo-2026-11-05.md`다. 사용자가 시연 결과를 알리면 UI 큐보다 먼저 이 task를 처리한다.

## Task 12: S4 끝 상태 (이관: 같은 campaign Task 6)

- [ ] Lane 2. 원래 문장: 끝 상태는 loopback 복귀(같은 `main` 빌드본 `configure-installed`를 prefix 옵션과 `--allow-lan` 없이 실행, Task 4 전 service 명령줄과 같아야 한다), 방화벽 규칙 0, 에이전트 operator 계정 disable, `pcv-it-s4-*` VM 정리다. 기록한다. 계정 파일 삭제는 이 task가 하지 않는다. 2026-11-05 메모: 지금 `main`의 `configure-installed`는 BL-0023 수정으로 실행 중 service를 다시 시작한다.

## Task 13: criteria S4 판정 (이관: 같은 campaign Task 7)

- [ ] Lane 1. 원래 문장: 시연 기록이 설계 1절의 판정을 모두 담으면 `config/project-completion-criteria.json` S4를 `passed`로 두고 시연 기록 링크를 적는다. push, PR, green CI 뒤 merge(이 campaign에서는 사용자 merge 승인 뒤). 판정 전에는 S4 통과를 주장하지 않는다. 판정 뒤 release train 출발 방식을 `next_approval_required`로 묻는다.

## Task 14: C5 runner 확인 (이관: 같은 campaign Task 2)

- [ ] 원래 문장: `not_before` 2026-10-19. Ubuntu 26 runner의 첫 `main` Development Gates와 Public Boundary run이 green이면 `config/project-completion-criteria.json` 위험 행을 `status=closed`, `closed_by`에 run id로 닫는다. 실패하면 `ubuntu-24.04` pin을 판단해 backlog 행으로 보고한다. push, PR. merge는 사용자 승인 뒤.

## Nonclaims

- operational current는 `0.42.96-admin-smoke` 그대로다. 이 campaign의 UI 변경은 release train 전까지 설치본에 들어가지 않는다.
- S4 통과는 Task 13 판정 전에는 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
