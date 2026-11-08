# Web Console 본문 원본 정합 설계

- 계약 id: `pcv-web-content-parity-v1`
- 상태: 제안(2026-11-09). 결정은 ADR-0020(제안)이고, 채택은 구현 campaign 승인 문장으로 한다.
- 근거: `docs/web-console-upstream-parity-audit-2026-11-09.md`(원본 `fd754e2` 대 Desktop Node `a1cd2e4`, 발견 61건)
- 선행 결정: ADR-0018(`single-edge-ui-v1`), ADR-0019(`sidebar-groups-vm-detail-tabs-v1`, `required-first-validate-inline-progress-inapp-confirm-v1`)
- 구현 계획(초안): `docs/superpowers/plans/2026-11-09-purecvisor-desktop-node-ui-parity-impl.md`

## 1. 배경

감사 결론은 이렇다. 셸과 공통 모듈은 원본 2.0.0과 같지만, 본문 화면은 2026-08 첫 공개본 배치를 그대로 쓴다.

- 모든 route가 옛 `render()`(`web/src/modules/monitor.ts:904-938`)와 `web/index.html:226-473`의 정적 섹션을 쓴다.
- 원본에서 옮긴 표·빈 상태·읽기 실패 부품은 쓰이지 않는다.
- VM 화면은 넓은 표와 오른쪽 320~380px 패널(`web/style.css:5212-5214`)이라 상세 6개 탭과 콘솔이 좁은 칸에 들어간다.

원본은 2026-11-05 뒤로 이 화면들을 바꾸지 않았다. 그래서 이 설계는 원본을 따라잡는 일이 아니라, Desktop Node가 건너뛴 본문 이식을 끝내는 일이다.

## 2. 목표와 비목표

목표:
- VM 화면을 원본처럼 목록·상세 골격으로 바꾼다.
- 대시보드, 네트워크, 작업, 이벤트, 진단, 증적, 계정 화면을 원본 부품(`HN.pagehead`, `statusPill`, `filterBar`, `createDataTable`, `PCV.components.emptyState`·`loadError`)으로 다시 그린다.
- 감사 6절의 지금 틀린 동작 9건을 고친다.
- 화면마다 렌더 함수가 본문을 그리는 모델로 옮기고, 마지막에 옛 `render()`와 정적 섹션을 지운다.

비목표:
- Linux runtime 화면(감사 분류 b 6건)과 WebSocket. 저장소 경계와 ADR-0018 그대로다.
- ADR-0019가 정한 것(감사 분류 c 5건): VM 상세 6개 탭, 헤더 조작 위치, 생성 마법사 단계, 삭제 이름 입력, 계정 생성 모달.
- 새 Local API. 5건(4.3절)은 따로 설계한다.
- 기본 테마. 어두운 `supanova-cyan` 기본은 BL-0026 사용자 결정이라 그대로 둔다. 원본 캡처의 흰 화면과 색이 다른 것은 의도다.
- 원본에 있지만 이번에 넣지 않는 것: VM 카드 보기(A-05), 즐겨찾기(A-10 일부), VM 비교(A-08 일부), 대시보드 기간 선택(사용률 API 뒤). 이유는 4.1절과 5절에 있다.

## 3. 원칙

1. 새 부품을 만들지 않는다. 원본에서 이미 옮긴 부품을 쓴다. 부품에 없는 동작이 필요하면 그 부품을 고치고 node 시험을 더한다.
2. 화면 하나씩 옮긴다. 옮긴 화면은 새 렌더 함수가 그리고, 아직 안 옮긴 화면은 옛 `render()`가 그린다. 한 PR이 한 단계다.
3. ADR-0019 상호작용 규칙(필수 입력 첫 칸, 요청 전 검사, 자리 진행 표시, 앱 안 확인, 한국어 우선)은 모든 새 화면에 적용한다.
4. 색은 theme token만 쓴다. 하드코딩된 패널 배경(`web/style.css:4795-4829`의 `rgba(17,26,43,.78)` 등)은 그 화면을 옮길 때 token으로 바꾼다.
5. 계약·시험이 붙잡는 id는 남기거나, 바꾸면 같은 PR에서 계약을 고친다(7절).
6. API 값이 없는 자리는 0이나 빈 칸으로 보이지 않는다. 그 칸을 빼고 API가 생기면 더한다(예: 모바일 `host.cpu`가 늘 0인 C-08).

## 4. 결정

### 4.1 VM 목록·상세 골격 (승인 질문 1)

원본 master-detail을 따른다. 화면 상태는 두 가지다.

| 상태 | 배치 |
| --- | --- |
| 고르지 않음 | `HN.pagehead`(제목 "가상 머신", 한 줄 설명, 주 버튼 `+ 새 VM`) 아래에 도구 줄(검색, 상태 칩 5개와 개수, 정렬)과 표를 전체 폭으로 둔다. 표 열은 지금 7열(이름, 상태, 세대, vCPU, 메모리, 표시, 조작) 그대로다 |
| 골랐음 | 목록이 왼쪽 260px 열로 줄어든다(이름, 상태 배지, 템플릿 배지만). 나머지 폭은 상세다. 상세 헤더는 이름 h2 하나, 상태 배지, 전원 조작, 콘솔 열기, 더 보기 메뉴(ADR-0019)이고, 그 아래 6개 탭이 한 줄에 놓인다 |

- 선택 카드는 지운다. 이름은 상세 헤더에 한 번만 나온다(A-03). id `vm-detail-panel`, `close-vm-detail`은 유지한다.
- 닫기 버튼과 Esc는 "고르지 않음"으로 돌아간다.
- 콘솔 탭은 넓어진 상세 폭에서 맞춤으로 그린다. 1440px 창에서 `1024x768` 프레임 배율이 약 0.33에서 0.85 안팎으로 오른다(A-02). 실제 크기와 전체 화면은 ADR-0019 그대로다.
- 바로 가는 링크(A-11): hash `#/vms/<vm-id>/<tab>`. `core.ts` `getHashView`(`:351-354`)는 지금 화면 이름만 읽는다. 뒤의 두 조각을 읽어 VM과 탭을 고르고, 뒤로 가기·앞으로 가기로 이동한다. `<vm-id>`는 `GET /api/v1/vms` 항목의 식별자다. 없는 VM이면 목록으로 돌아가고 toast로 알린다.
- 빈 상태와 읽기 실패(A-09): VM이 없으면 `components.emptyState`와 `+ 새 VM` CTA, 읽기 실패면 `components.loadError`와 다시 시도 버튼이다.
- 행 메뉴(A-07): 행 끝 `⋯` 버튼과 우클릭(ContextMenu 키, Shift+F10 포함)이 같은 메뉴를 연다. 항목은 ADR-0019 더 보기 메뉴와 같고, 상태에 맞지 않는 항목은 끈다. 행 주 조작 하나는 그대로 둔다.
- 정렬과 이동(A-10): 열 머리를 누르면 그 열로 정렬하고, 다시 누르면 방향을 바꾼다. 목록에 포커스가 있으면 j/k로 행을 옮긴다. 즐겨찾기는 넣지 않는다. 사용자별 저장 위치가 필요한데 체감이 낮다.
- 일괄 조작(A-08): 표 첫 열 체크박스와, 하나 이상 고르면 나타나는 선택 막대(선택 n개, 시작, 종료, 삭제, 선택 해제). 일괄 삭제 확인 모달은 대상 이름 목록을 보이고, 고른 개수를 입력해야 버튼이 켜진다(ADR-0019 규칙 4의 이름 입력을 여러 대상으로 넓힌 것). VM마다 job 하나이고, 자리 진행 줄을 VM마다 보인다. 비교는 넣지 않는다. 원본 비교는 사용률 값 위주라 metrics API 뒤에 다시 본다.
- 카드 보기(A-05)는 넣지 않는다. 260px 목록과 역할이 겹치고, 목록 렌더러가 둘이 된다.
- 600px 이하: 목록과 상세를 한 번에 하나만 보인다. 상세를 열면 목록을 숨기고 헤더에 뒤로 버튼을 둔다. `mobile.ts`의 카드 목록은 유지하고 B-11을 고친다.
- `web/src/modules/vm.ts`는 2,739줄로 줄 수 ratchet 상한이다. 목록·골격·행 메뉴·일괄 조작은 새 모듈 `vm-list.ts`로 나누고 `modules.json`에서 `vm` 앞에 둔다. `vm.ts`는 상세만 남긴다. 줄 수 상한은 올리지 않는다.

### 4.2 화면별 렌더 모델과 다시 그리는 순서 (승인 질문 2)

렌더 모델(B-01):
- `nav.ts`에 화면 등록표를 둔다: `PCV.views.register(name, { render(mount, ctx) })`. 등록된 화면은 nav가 그 `render`를 부르고, 옛 `renderActiveView()`는 등록된 화면을 건너뛴다.
- 옮긴 화면의 `index.html` 섹션은 빈 mount(`<section id="view-<name>">`)만 남긴다.
- 화면 `render`는 `HN.pagehead` → 도구 줄(`filterBar`) → 읽는 동안 skeleton → 표(`createDataTable`) 또는 `emptyState` 또는 `loadError` 순서로 그린다. 주기 새로 고침은 활성 화면의 `render`만 다시 부른다.
- 모든 화면을 옮긴 뒤(U8) 옛 `render()`, `renderActiveView()`, 정적 섹션, 옛 명령 팔레트(`#command-palette`)를 지운다.

화면별 구성:

| 화면 | 머리와 도구 | 본문 | 발견 |
| --- | --- | --- | --- |
| 네트워크 | pagehead(설명), `filterBar` 종류 chip(전체와 스위치 `type` 값별)과 개수 | `createDataTable`(이름, 종류, 연결 VM 수, 어댑터, 상태 pill). 768px 이하는 카드. 연결 VM 수는 `vm.list`의 network로 브라우저에서 센다. 읽기 전용 그대로다 | C-01, C-04, C-05, C-06 |
| 작업 | pagehead, 상태 chip(전체, 진행 중, 성공, 실패, 취소)과 종류 select | `createDataTable`(접수 시각, 종류, 대상, 상태 pill, 걸린 시간, 오류 요약), 정렬·검색·CSV, 다음 페이지 버튼 | C-11, C-12 |
| 이벤트 | pagehead, 심각도 chip | 원본 이벤트 센터처럼 위에 심각도별 개수 카드, 아래 표. 작업 목록을 다시 그리는 활동 패널은 지우고 작업 화면 링크로 바꾼다 | C-09, C-10 |
| 대시보드 | pagehead(빠른 작업은 주 버튼 `+ 새 VM` 하나와 보조 링크) | 위 KPI 카드(VM 실행 중·꺼짐·템플릿, 진행 중 job, 최근 24시간 실패 job, Hyper-V 준비 상태), 가운데 워크로드 표(상위 VM, 행을 누르면 VM 상세), 오른쪽 경고 피드(실패 job과 ops 요약 incident). hero, 고정 메모, 두 번째 머리 "운영 콕핏"은 지운다. 사용률 게이지와 기간 선택은 metrics API 뒤에 더한다 | B-03, B-04, B-05, C-20 패턴 |
| 진단 | pagehead | 원본 호스트 상태처럼 두 열 카드: Hyper-V 준비 상태와 권장 조치, service·listener, 현재 세션(로그인 폼 없이 사용자·역할·로그아웃), 진단 번들 받기. registry bridge, Host Ops bucket 표, 경계 칩은 접힌 "고급" 절로 옮긴다 | C-16, B-14 |
| 증적 | pagehead | 운영자에게 필요한 값(설치 version, operational current, 마지막 검증 시각) 카드. GPU snapshot, Public boundary head 같은 release 엔지니어링 값은 접힌 절로 옮긴다 | C-18 |
| 계정 | pagehead, 역할 chip | `createDataTable`과 역할 pill. 생성은 ADR-0019 모달 그대로 | C-13 |

순서(구현 단계, 10절): 동작 결함을 먼저 고치고, 사용자가 지적한 VM 화면을 다음에 한다. VM 골격은 `vm.ts`의 표 렌더러와 `index.html`·`style.css`만 바꾸므로 렌더 모델 이행을 기다리지 않는다. 그 뒤 가장 작은 네트워크 화면으로 렌더 모델을 세우고, 작업·이벤트, 대시보드, 진단·증적·계정 순서로 옮긴다. 마지막에 옛 렌더러를 지운다.

### 4.3 Local API가 먼저 필요한 5건 (승인 질문 3)

이번 개편에서 뗀다. 레이아웃 PR과 route 추가 PR을 섞지 않는다. route 하나를 더하면 고정값 약 15곳(route catalog, coverage, snapshot SHA, Host fixture, ratchet)과 release train queue 행이 함께 바뀌기 때문이다. 5건은 API 묶음 셋으로 나눠 각자 따로 설계한다.

| API 묶음 | 발견 | ADR | 이유 |
| --- | --- | --- | --- |
| VM·호스트 사용률 metrics | A-06 목록 CPU 막대, A-14 성능 탭, C-08 모니터·차트 | 새 ADR이 필요하다 | 주기 표본, 보관 기간, 읽기 동시성(ADR-0012), WMI 읽기 비용을 정하는 새 runtime 동작이다 |
| ISO 목록 | A-17 ISO 찾아보기 | 기존 ADR 안에서 설계 문서로 정한다 | read route 하나지만 호스트 파일 목록을 보이므로, 보여 줄 폴더와 경로 표시 규칙을 설계에서 정한다 |
| 계정 관리 | C-14 역할 변경, 삭제, 재활성화 | 설계 문서로 정한다 | 마지막 admin 보호, 감사, 세션 무효화는 ADR-0019 설계 부록 A(비밀번호 재설정)의 규칙을 넓히면 된다 |

API가 생기기 전까지 그 자리는 4.1절과 3절 원칙 6대로 비워 두지 않고 뺀다.

### 4.4 지금 틀린 동작

| id | 고치는 방법 |
| --- | --- |
| B-11 | `mobile.ts`가 부르는 함수를 실제 export로 바꾼다: 전원은 `PCV.vm.queueVmLifecycle`, 삭제는 `PCV.vm.queueVmDelete`. 일괄 삭제는 4.1절 일괄 삭제 확인 모달을 쓴다. `mobile.ts`가 부르는 `PCV.vm.*`가 export에 있는지 node 시험으로 고정한다 |
| B-07 | 무인증 health route는 없고 새로 만들지 않는다(로그인 전 노출 범위가 늘어난다). 로그인 화면 노드 칸은 로그인 없이 알 수 있는 것(Web Console version, 접속 주소)만 보이고, 노드 상태는 로그인 뒤 상태바에서 본다 |
| B-08 | TLS pill은 지금 연결의 scheme을 보인다(`http`이면 "HTTP·내부망"). 빈 pill은 남기지 않는다 |
| B-09 | 명령 팔레트를 새 모달 하나로 합친다. Ctrl+K도 새 모달을 연다. 옛 `#command-palette`는 U8에서 지운다 |
| B-10 | 도움말이 안내하는 단축키를 실제로 등록하거나 안내에서 뺀다. 등록하는 것은 Ctrl+K(팔레트), `/`(화면 검색), `?`(단축키 도움말), Esc(닫기)다 |
| B-12, C-19 | 로그아웃(`api.ts:112-126`)에서 열린 modal과 dialog를 모두 닫는다. 원본 2.0.1 `_teardownSession`과 같다 |
| B-14 | 로그인 폼은 로그인 오버레이 하나다. 경고 영역과 진단 화면의 폼은 지우고 "다시 로그인" 버튼이 오버레이를 연다 |
| C-10, C-11 | 4.2절 이벤트·작업 행 |
| A-23 | 요약 탭 readback의 원시 필드 이름을 i18n 라벨로 바꾸고, Linux 전용 필드(`linux_blkio_compatible` 등)는 보이지 않는다 |
| 그 밖의 S | B-15 도움말 카탈로그에 비밀번호 route 2개를 더하고 계정 화면 열 값을 고친다. B-18 사이드바 `aria-label`을 되살린다. C-17 화면 제목을 메뉴 이름과 맞춘다 |

## 5. 넣지 않는 원본 기능

| 원본 기능 | 이유 |
| --- | --- |
| VM 카드 보기(A-05) | 260px 목록과 역할이 겹치고 렌더러가 둘이 된다 |
| 즐겨찾기(A-10 일부) | 사용자별 저장 위치가 필요한데 체감이 낮다 |
| VM 비교(A-08 일부) | 원본 비교는 사용률 값 위주라 metrics API 뒤에 다시 본다 |
| 대시보드 기간 선택, 사용률 게이지 | metrics API 뒤 |
| 스위치 생성·편집·삭제(C-07) | Hyper-V 스위치 편집 route가 없고, 호스트 네트워크를 바꾸는 host mutation이다. 따로 설계한다 |
| Linux 전용(A-18, A-19 일부, A-21, C-15, C-20 화면) | 저장소 경계 |

## 6. 디자인 체계

- token은 `web/style.css`의 현재 기본 테마를 쓴다. 새 화면에서 색 값을 직접 쓰지 않는다(리뷰에서 확인한다).
- `web/DESIGN.md`의 기본 테마 설명이 두 곳(`:79-81`, `:84-100`)에서 다르다. U8에서 BL-0026 결정(어두운 기본)대로 하나로 고친다.
- 부품 상태는 ADR-0019 설계 6절 그대로다(normal, hover, focus, active, disabled, loading, empty, error).
- 새 문구는 i18n 키로 두고 ko를 기본, en을 같은 키로 맞춘다(`web/node-tests/i18n-dictionary.test.mjs`).

## 7. 계약과 시험 영향

구현 PR마다 같은 PR에서 고친다.

- 생성물: `npm run build:served --prefix web`로 `web/app.bundle.js`와 `web/sw.js` CACHE_NAME을 만든다. 손으로 고치지 않는다.
- 모듈 순서: 새 모듈(`vm-list.ts`)은 `web/src/modules.json` 순서표 검사(`web/scripts/build-served-asset.mjs`)를 맞춘다.
- VM 골격을 붙잡는 곳(감사 A 묶음 조사):
  - `web/tests/PcvDesktopWeb.Static.Tests.ps1:484`
  - `web/contracts/web-static-contracts.mjs:1080`
  - `web/node-tests/single-edge-shell.test.mjs:43`
  - `web/node-tests/vm-checkpoints-tab.test.mjs:970-971`(CSS 정규식)
  - `web/node-tests/web-static-contracts-negative.test.mjs:303`
  - `web/scripts/verify-browser-fixture.mjs:41-44`
  - `config/browser-ui-gates-baseline.json`
  - `web/scripts/capture-destructive-lifecycle-ui-qa.mjs`
- Web 문구·binding을 바꾸면 pin된 다섯 곳과 web Pester를 함께 고친다(옛 계약 시험 관례).
- ratchet: `vm.ts` 줄 수 상한과 `lint:domsafe` ceiling을 올리지 않는다. `HN.*` 부품이 문자열 HTML을 돌려주면 domsafe가 허용하는 경로로만 붙인다. 구현 첫 단계에서 부품 반환 형식을 확인한다.
- product payload: Web 파일은 MSI payload다. 구현 PR마다 `docs/ga-ready/release-train.json` `queue` 행을 더한다.

## 8. 시험 전략

- node 시험(`web/node-tests`):
  - VM 골격 상태(고르지 않음·골랐음의 class와 열 수), hash `#/vms/<id>/<tab>` 해석과 없는 VM 처리
  - 행 메뉴 키보드(ContextMenu, Shift+F10, 화살표, Esc), 상태에 맞지 않는 항목 꺼짐
  - 일괄 삭제 확인의 개수 입력 전 버튼 꺼짐, VM마다 job 하나
  - `mobile.ts`가 부르는 `PCV.vm.*`가 모두 export에 있음(B-11 재발 방지)
  - 화면 등록표: 등록된 화면은 옛 `renderActiveView()`가 건너뜀, `emptyState`와 `loadError`가 서로 다른 문구
  - 로그아웃 뒤 열린 modal 0개, 명령 팔레트 하나
- 브라우저: `npm run browser:ui-gates`(axe 위반 0 기준선 유지)와 browser fixture. 단계마다 가짜 API로 1440, 768, 390 폭 캡처를 원본 캡처와 나란히 PR에서 검토한다.
- 설치본 동작은 release train 뒤 설치본 시연 기록으로만 주장한다.

## 9. 위험

| 위험 | 대응 |
| --- | --- |
| 옛 `render()`와 새 화면 렌더가 같은 DOM을 함께 고친다 | 등록된 화면은 옛 렌더가 건너뛴다. 화면을 옮긴 PR이 그 화면의 옛 part 호출을 지운다 |
| VM 골격 변경이 콘솔·체크포인트 탭 시험을 깬다 | 7절 목록을 같은 PR에서 고친다. 탭 내용은 바꾸지 않는다 |
| `vm.ts` 줄 수 상한 | 4.1절대로 `vm-list.ts`로 나눈다 |
| 단계가 길어져 설치본 반영이 늦다 | U1(동작 결함)과 U2·U3(VM)이 merge되면 그 묶음만으로 release train을 출발할 수 있다(출발은 사용자 승인) |

## 10. 단계

| 단계 | 내용 | 발견 | 규모 |
| --- | --- | --- | --- |
| U1 | 지금 틀린 동작과 작은 정리 | B-07, B-08, B-09(합치기), B-10, B-11, B-12, C-19, B-14, B-15, B-18, C-17, A-23 | M |
| U2 | VM 목록·상세 골격, `vm-list.ts` 분리, 바로 가는 링크, 빈 상태와 읽기 실패 | A-01, A-02, A-03, A-04, A-09, A-11 | M |
| U3 | VM 행 메뉴, 정렬과 j/k, 일괄 조작 | A-07, A-08, A-10 | M |
| U4 | 화면 등록표와 네트워크 | B-01(바탕), B-02, C-01, C-02(네트워크), C-04, C-05, C-06 | M |
| U5 | 작업과 이벤트 | C-03, C-09, C-10, C-11, C-12, B-13 | M |
| U6 | 대시보드 | B-03, B-04, B-05 | M |
| U7 | 진단, 증적, 계정 | C-16, C-18, C-13 | M |
| U8 | 옛 렌더러·정적 섹션·옛 팔레트 퇴역, 남은 하드코딩 색, `web/DESIGN.md` 정리, 품질 게이트 | B-01(마무리), B-09(마무리), C-02, B-06 문서 | M |

- 담당: grok 구현, Claude 리뷰(기존 역할 분담). 단계마다 Lane 1 PR 하나이고 Required CI green 뒤 merge한다.
- 설치본 반영: 단계 PR들은 release train queue에 쌓인다. 출발 시점과 묶음은 사용자 승인이다.

## Nonclaims

- 이 설계는 화면 코드를 바꾸지 않는다. operational current `0.42.99-admin-smoke`를 바꾸지 않는다.
- 원본 화면을 그대로 복제한다고 주장하지 않는다. 원본 부품과 골격을 쓰고, ADR-0019 결정과 BL-0026 테마 결정은 유지한다.
- Linux runtime 화면을 가져온다고 주장하지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
