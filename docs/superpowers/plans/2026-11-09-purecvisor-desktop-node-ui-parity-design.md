# Web Console 원본 정합 감사와 개편 설계 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 원본 purecvisor 프론트엔드 전체(`fd754e2`, Release 2.0.4)와 Desktop Node Web Console(`main` `a1cd2e4`, 설치본 `0.42.99-admin-smoke`)을 비교한 2026-11-09 감사를 저장소 문서로 남긴다. 그 결과로 본문 화면(VM 목록·상세 골격, 대시보드, 네트워크, 작업, 이벤트, 진단)을 원본 부품으로 다시 그리는 개편 설계와 구현 campaign 계획을 쓴다. 이 campaign은 문서만 바꾼다.

**Architecture:** 근거는 2026-11-09 감사다. Claude가 지휘했고 하위 에이전트 세 개가 VM(A 23건), 셸·공통(B 18건), 데이터 화면(C 20건)을 나눠 코드 위치와 가짜 API 화면 캡처로 비교했다. Claude는 핵심 주장(`.vm-workspace` 격자 `c76a831` 이후 그대로, 이식된 `HN.*` 부품 미사용, 모바일 전원·삭제 함수 부재, 로그인 노드 상태 미기록)을 코드로 다시 확인했다. ADR-0018(`single-edge-ui-v1`) 구조와 ADR-0019 정보 구조·상호작용 규칙은 유지한다. 본문 화면 골격과 부품 사용 규칙은 새로 정한다. branch 하나 `lane1/ui-parity-design-20261109`, PR 하나다.

**Tech Stack:** Markdown 문서, Delivery 계약 시험(`dotnet test src/DesktopNode.Delivery.Tests -c Release`), PR gate(`packaging/windows-desktop-node/tools/Invoke-PcvPrGate.ps1`)

## 사용자 결정 (2026-11-09)

승인 원문: `1` (Web Console 원본 정합 감사 보고의 다음 승인 1 "UI 개편 설계 승인"에 대한 답)

| 항목 | 범위 |
| --- | --- |
| 1 | 감사 문서와 개편 설계 문서를 쓴다. 설계가 정한 결정은 ADR로 남긴다 |
| 2 | 구현 campaign 계획까지 세운다. 구현 campaign을 여는 것은 다음 승인이다 |
| 3 | 설계에서 정할 것: VM 화면을 원본처럼 목록·상세 골격으로 바꾸는 방식, 대시보드·네트워크·작업·이벤트·진단을 원본 부품으로 다시 그리는 순서, Local API가 먼저 필요한 5건을 따로 ADR로 뗄지 |
| 차선 | Lane 1 문서 변경만. host mutation 없음. `current-evidence.json`을 쓰지 않는다 |
| commit/PR/merge | task마다 로컬 commit. 마지막 task에서 push, PR 하나, Required CI 다섯 check green 뒤 merge(2026-11-09 상시 merge 승인) |

## Global Constraints

- 문서만 바꾼다. `web/**`, `src/**`, `config/**`, `packaging/**`는 바꾸지 않는다. 생성물 `web/app.bundle.js`, `web/sw.js`도 그대로다.
- 원본은 commit SHA와 `ui/` 상대 경로·줄 번호로만 인용한다. 원본 clone과 화면 캡처는 저장소 밖 작업 폴더에만 두고 문서에 그 경로를 쓰지 않는다.
- public 문서에는 사용자 홈 경로, LAN 사설 IP, 호스트명, token·password 값을 적지 않는다.
- Linux 전용 화면(컨테이너, VPC/OVN, ZFS 스토리지, 백업, iSCSI, DPDK, SR-IOV, GPU, 보안 이벤트, 자가치유 등)은 경계 밖으로 분류만 하고 설계에 넣지 않는다.
- 보존 VM `pcv-guest-installed-04253-r1`, template `pcv-it-s2-source`, BL-0028 부모 VHD, 설치된 service와 계정은 건드리지 않는다.
- 역할: 이 campaign의 문서는 Claude가 쓴다. 구현 campaign에서는 grok이 구현하고 Claude가 리뷰한다.
- 한도: Lane 1 30분·tool batch 18회(checkpoint마다).

## Task 1: 감사 문서

- [x] Claude. `docs/web-console-upstream-parity-audit-2026-11-09.md`를 쓴다. 기준(원본 `fd754e2`, ADR-0018 출처 `ed147de`, Desktop Node `a1cd2e4`, 설치본 `0.42.99`), 방법, 결론(원인은 원본 변경이 아니라 본문 영역 미이식), 원본 화면 대응표, 발견 61건(A·B·C, 원본 위치, Desktop Node 위치, 분류, 체감, 규모), 지금 틀린 동작, 분류 집계, Nonclaims를 담는다. Desktop Node 쪽 줄 번호는 `a1cd2e4`에서 표본으로 다시 확인한다. 검증: `git diff --check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

실행 기록(2026-11-09): 감사 문서를 썼다. 발견 61건(A 23, B 18, C 20)의 분류는 a 33, a-API 5, b 6, c 5, d 10, same 2다. 지금 틀린 동작 9건을 따로 모았다. `a1cd2e4`에서 다시 확인한 것: `web/`이 설치본 payload `f7426d1`과 같음, `.vm-workspace` 격자(`web/style.css:5212-5214`, `c76a831` 2026-08-25부터), `createDataTable`·`emptyStatePro`·`renderLoadError`의 정의 파일 밖 호출 0, `mobile.ts:219,242,328-329`가 부르는 네 함수가 export(`vm.ts:2738`)와 정의 어디에도 없음, `lh-state`를 쓰는 TS 없음. 원본 clone과 캡처 경로는 문서에 적지 않았다(홈 경로·사설 IP 검색 0건). 검증: `git diff --check` 통과, `dotnet test src/DesktopNode.Delivery.Tests -c Release` 785/785. host mutation 없음.

## Task 2: 개편 설계와 ADR

- [x] Claude. 설계 `docs/superpowers/specs/2026-11-09-purecvisor-desktop-node-web-console-upstream-parity-design.md`와 ADR-0020을 쓰고 `docs/ADR_INDEX.md`에 절을 더한다. 설계는 승인 3의 세 질문에 답한다: VM 목록·상세 골격(선택 전 전체 폭 표, 선택 뒤 260px 목록과 넓은 상세), 본문 화면 재작성 순서와 화면별 부품(`HN.pagehead`, `filterBar`, `createDataTable`, `components.emptyState`·`loadError`), Local API 선행 5건의 처리. 계약·시험 영향과 단계를 적는다. 검증: `git diff --check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

실행 기록(2026-11-09): 설계(`pcv-web-content-parity-v1`)와 ADR-0020(결정 마커 4개, 상태 제안)을 썼고 `docs/ADR_INDEX.md`에 2026-11-09 절과 제안 후보 표를 더했다. 세 질문의 답: (1) VM은 고르지 않으면 전체 폭 표, 고르면 왼쪽 260px 목록과 넓은 상세, hash `#/vms/<vm-id>/<tab>`, 목록·골격은 새 모듈 `vm-list.ts`로 나눔(`vm.ts` 줄 수 상한). (2) 순서는 동작 결함(U1), VM 골격(U2), VM 행 메뉴·일괄(U3), 렌더 등록표와 네트워크(U4), 작업·이벤트(U5), 대시보드(U6), 진단·증적·계정(U7), 옛 렌더러 퇴역(U8). (3) API 선행 5건은 이 개편에서 떼고 묶음 셋으로 나눔. metrics만 새 ADR, ISO 목록과 계정 관리는 설계 문서. 확인한 사실: 무인증 route는 `auth/*` 넷뿐이라 B-07은 새 route 없이 고침, 네트워크 스위치는 `type` 값을 줌. ADR-0020 채택은 구현 campaign 승인 문장으로 미뤘다. 검증: `git diff --check` 통과, `dotnet test src/DesktopNode.Delivery.Tests -c Release` 785/785, 홈 경로·사설 IP 검색 0건. host mutation 없음.

## Task 3: 구현 campaign 계획

- [ ] Claude. 구현 campaign 계획 `docs/superpowers/plans/2026-11-09-purecvisor-desktop-node-ui-parity-impl.md`를 승인 대기 초안으로 쓴다. 설계 단계마다 task 하나(Lane 1, grok 구현·Claude 리뷰), 파일 범위, 고칠 계약·시험, 검증 명령, PR 단위, release train 연결을 적는다. campaign 파일은 열지 않는다. 검증: `git diff --check`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`. 로컬 commit.

## Task 4: 닫기와 merge

- [ ] campaign을 닫는다(`status=closed`, `next_task=null`, `next_step` 결과 요약, `next_approval_required`에 구현 campaign 열기 문장). `docs/DOCUMENTATION_INDEX.md` 현재 기준 줄을 고친다. clean HEAD에서 `git diff --check origin/main...HEAD`, `dotnet test src/DesktopNode.Delivery.Tests -c Release`, PR gate를 돌린다. push, PR, Required CI 다섯 check green 뒤 merge, merge commit의 `main` push run green 확인.

## Nonclaims

- operational current `0.42.99-admin-smoke`를 바꾸지 않는다. 설치본 화면은 이 campaign으로 바뀌지 않는다. 화면이 바뀌는 것은 구현 campaign 뒤 release train을 지난 다음이다.
- 감사의 화면 비교는 가짜 API 데이터와 정적 서버로 찍은 캡처와 코드 대조다. 설치본 브라우저 동작을 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
