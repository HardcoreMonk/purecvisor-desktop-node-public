# PureCVisor Desktop Node Web Console 운영 상태 진실성 설계

- Design-ID: `purecvisor-desktop-node-web-console-state-truthfulness-v1`
- 작성 기준: `2026-08-05`, `Asia/Seoul`
- 문서 상태: `approved-design`
- 승인 locator: `User-Approval: web-console-state-truthfulness-design-20260805`
- 기준 커밋: `ff8a50ce` (`codex/adr-verification-command-reconcile`)
- 근거 감사: `docs/project-status-audit-2026-08-05.md` §6 P1-2
- 선행 평가: `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md` §6.2 P0-2
- 이 설계가 수행하는 host mutation: `false`
- 변경 대상 경계: `web/**`와 `web/tests/**`만. 제품 .NET 코드, packaging, workflow, evidence는 제외

## 1. 문제

Web Console의 footer `.status-bar`와 hero `.hero-chips`는 **어떤 코드도 갱신하지 않는 순수 정적
HTML**이다. `served-app.ts`와 생성물 `app.js` 어디에도 두 영역에 대한 참조가 없다.

실측 확인 결과는 다음과 같다.

| 영역 | 코드 갱신 | 실태 |
| --- | --- | --- |
| `.status-bar` 7개 span | 없음 | `Connected`, `pcv-node-a`, `Updated 0s ago`, `VM: 3/3`, `API: 10ms avg`가 영구 고정 |
| `.hero-chips` 3개 span | 없음 | `활성 워크로드 4/5`, `호스트 모드 Desktop`, `최근 경고 0`이 영구 고정 |
| `#asset-count` | 있음 | 정적 seed `3`이 첫 렌더 전까지 노출 |
| `#vm-asset-list` | 있음 | 정적 `pcv-node-a` 행이 첫 렌더 전까지 노출 |
| `#connection-state` | 있음 | 이미 실제 상태 반영 |

2026-07-16 평가서는 설치본 실제 Chromium에서 `/host/status`, `/vms`, `/network/inventory`,
`/runtime/policy`, `/ops/summary`, `/console/capabilities`, `/jobs`, `/diagnostics/bundles`
8개가 모두 HTTP 401인 상태에서도 위 값이 정상 운영값처럼 표시되는 것을 확인했다.

이는 시각 품질 문제가 아니라 **운영 상태의 진실성** 문제다. 운영자가 연결 실패와 정상 상태를 구분할 수
없다.

## 2. 목표와 비목표

### 목표

- 화면에 표시되는 모든 운영값은 실제 `state`에서만 나온다.
- 값을 확보하지 못한 상태(미인증, 에러, 초기)는 중립 표기 `—`로 표시한다.
- 정적 HTML은 어떤 조작된 운영값도 포함하지 않는다. 하이드레이션이 실패해도 가짜가 노출될 수 없다.
- 회귀를 기존 required 게이트 안에서 막는다.

### 비목표

- Web secure bootstrap / account auth 구성 (2026-07-16 P0-1). 별도 slice로 남긴다.
- 401 fan-out 억제와 login 안내 UX (2026-07-16 P0-3). 별도 slice로 남긴다.
- 시각 디자인, 레이아웃, 스타일 변경.
- API 응답시간 계측 인프라 도입.
- `web/src/served-app.ts` 대형 모듈 분리.

## 3. 결정

### 3.1 미갱신 영역은 실제 상태에 바인딩한다

`.status-bar`와 `.hero-chips`를 삭제하지 않고 실제 `state`에 연결한다. 화면 구성은 유지하면서 정보만
진실해진다.

대안으로 검토한 두 가지는 채택하지 않았다.

- **영역 삭제**: `#connection-state`/`#asset-count`가 유사 정보를 이미 제공하지만, footer는 뷰와 무관하게
  항상 보이는 유일한 전역 상태 표시줄이다. 삭제하면 전역 상태 가시성을 잃는다.
- **중립값 치환만**: 거짓말은 사라지지만 인증 성공 후에도 계속 `—`로 남아 표시 영역이 죽는다.

### 3.2 뒷단 데이터가 없는 필드는 삭제한다

`WS`와 `API latency`는 계측 인프라가 존재하지 않는다. 앱에는 WebSocket 클라이언트가 없고
(`noVNC bridge` 설정 개념은 별개다) 응답시간을 재는 코드도 없다. 두 span을 삭제한다.

`Updated`는 `lastRefreshedAt` 타임스탬프 하나로 구현 가능하고 데이터 신선도라는 실질 가치가 있으므로
유지하고 실제 값을 채운다.

### 3.3 성공적으로 로드된 값만 표시한다

렌더 함수는 `state`에 값이 없으면 `—`를 쓴다. fallback 샘플값을 만들지 않는다.

## 4. 변경 지점

| 파일 | 변경 |
| --- | --- |
| `web/index.html` | footer/hero span에 `id` 부여, 조작된 초기값을 중립값으로 교체, `WS`/`API latency` span 삭제, hero chip 라벨 `호스트 모드` → `호스트 상태`, `#asset-count` seed를 `—`로, `#vm-asset-list` 정적 행을 `<p class="muted">VM 자산을 불러오는 중…</p>` 단일 플레이스홀더로 교체 |
| `web/src/served/state.ts` | `lastRefreshedAt: number \| null` 필드 추가 (초기값 `null`) |
| `web/src/served-app.ts` | `renderStatusBar()`, `renderHeroChips()` 추가 후 `render()`에 등록. 갱신 성공 경로에서 `state.lastRefreshedAt` 기록 |
| `web/app.js` | `node scripts/build-served-asset.mjs`로 재생성. 직접 편집 금지 |
| `web/generated/**` | static parity manifest 재생성 |
| `web/tests/PcvDesktopWeb.Static.Tests.ps1` | 정적 조작값 회귀 가드 추가 |
| `web/scripts/verify-browser-fixture.mjs` | 미인증 렌더 동작 회귀 가드 추가 |

`render()`는 단일 진입점이므로 두 함수를 목록에 등록하는 것으로 전체 갱신 경로가 연결된다.

## 5. 데이터 바인딩 계약

| 표시 항목 | 요소 id | 소스 | 값 없을 때 |
| --- | --- | --- | --- |
| 연결 상태 | `status-connection` | `state.connectionState` | `Not connected` |
| Windows 판 | `status-host` | `readNested(state.host, ['windows', 'caption'])` | `—` |
| 갱신 시각 | `status-updated` | `state.lastRefreshedAt` | `Updated —` |
| VM 수 | `status-vm-count` | `getSummaryVmCounts()` | `VM: —` |
| 현재 뷰 | `status-view` | `state.activeView` | 항상 존재 |
| 활성 워크로드 | `hero-workload` | `getSummaryVmCounts()` running/total | `—` |
| 호스트 상태 | `hero-host-mode` | `getHostReadinessLabel()` | `—` |
| 최근 경고 | `hero-alerts` | `state.partialFailures.length` | `—` |

`readNested`, `getSummaryVmCounts()`, `getHostReadinessLabel()`은 `served-app.ts`에 이미 존재하는
헬퍼다. 새 추출 규칙을 만들지 않고 기존 패턴을 따른다.

### 5.1 호스트 식별자는 존재하지 않는다

`GET /api/v1/host/status`의 실제 스키마(`DesktopNodeHyperVHostStatusData`)는
`supported`, `reasons[]`, `windows{caption,version,edition}`, `admin{elevated}`,
`hyperv{feature_enabled,vmms_running,default_switch_present}`가 전부다. **머신 이름 필드도 `mode`
필드도 없다.**

따라서 정적 `pcv-node-a`는 단순한 stale 값이 아니라 **API가 제공하지 않는 정보를 지어낸 것**이다.
제품 .NET 코드 변경은 이 설계의 비목표이므로, 해당 자리는 실재하는
`windows.caption`으로 재바인딩하고 hero chip 라벨은 `호스트 모드` → `호스트 상태`로 바꾼다.

### 5.2 `getHostReadinessLabel()` fallback 게이트 (필수)

`getHostReadinessLabel()`은 `state.host?.supported === false`가 아닐 때 `Ready`를 반환한다.
`state.host`가 `null`인 미인증 상태에서도 `Ready`가 나온다. 이는 정확히 이 설계가 제거하려는
가짜 상태이므로 **반드시 로드 여부로 먼저 게이트한 뒤에만 호출한다.**

### 5.3 로드 여부 게이트

`getSummaryVmCounts()`는 `state.vms`가 비어 있으면 `0/0`을 반환하므로 "미로드"와 "실제 0"을 구분하지
못한다. 다음 헬퍼로 판정한다.

- 최초 성공 갱신이 있었는가: `state.lastRefreshedAt !== null`
- 해당 route가 이번 갱신에서 실패하지 않았는가: `state.partialFailures`에 같은 `operation`이 없음

`collectRefreshFailures()`가 채우는 필드명은 `label`이 아니라 **`operation`**이다
(`host.status`, `vm.list` 등).

`state.lastRefreshedAt`은 `refreshAll()`이 갱신 주기를 완료했을 때 기록한다. 실패한 route는 위 게이트가
개별적으로 걸러내므로 `Updated` 값은 마지막 갱신 시도 완료 시점을 가리킨다.

## 6. 상태별 표시 규칙

| 상태 | footer 연결 | footer 호스트/VM | hero chip |
| --- | --- | --- | --- |
| 초기 (로드 전) | `Not connected` | `—` | `—` |
| 미인증 (401) | `Not connected` | `—` | `—` |
| 부분 실패 | `state.connectionState` 반영 | 로드된 값만, 나머지 `—` | 로드된 값만 |
| 정상 | `Connected` | 실제 host/VM 값 | 실제 값 |

핵심 불변식: **어떤 상태에서도 로드되지 않은 값 자리에 그럴듯한 숫자나 이름이 들어가지 않는다.**

## 7. 테스트 전략

두 가드 모두 기존 required 게이트 안에 배치한다. 새 CI job을 만들지 않는다.

### 7.1 정적 가드 — `web/tests/PcvDesktopWeb.Static.Tests.ps1`

`Development Gates`의 `installer-web-pester` job이 실행한다.

`web/index.html`이 다음 조작된 운영값을 포함하지 않을 것:

| 금지 패턴 | 비고 |
| --- | --- |
| `>Connected<` | 요소 경계로 앵커한다. 새 초기값 `>Not connected<`는 매치되지 않는다 |
| `VM: 3/3` | |
| `API: 10ms avg` | |
| `Updated 0s ago` | |
| `<strong>4/5</strong>` | 활성 워크로드 조작값 |
| `pcv-node-a` | 조작된 호스트 식별자 |
| `<span id="asset-count">[0-9]` | 숫자 seed |

**앵커 주의:** PowerShell `-Match`는 기본이 대소문자 무시다. 따라서 `Connected`를 그대로 검사하면
새 초기값 `Not connected`에 오탐한다. 반드시 `>Connected<`처럼 요소 경계를 포함해 앵커한다.

### 7.2 동작 가드 — `web/scripts/verify-browser-fixture.mjs`

`npm run verify:parity`가 실행하며 `web-tests` job에 포함된다.

인증 실패 fixture로 렌더한 뒤 다음을 확인할 것:

- footer 연결 표시가 `Not connected`
- footer/hero에 조작된 운영값이 없음
- 값 미확보 자리가 `—`

### 7.3 RED 우선

두 가드를 **먼저** 추가해 현재 코드에서 실패하는 것을 확인한 뒤 구현한다. 실패 사유가 의도한 것과
일치하지 않으면 가드를 고친다.

## 8. 검증 명령

```powershell
npm test --prefix web
npm run generate:parity --prefix web
npm run verify:parity --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
node --check web/app.js
git diff --check
```

`app.js`는 생성물이므로 `build-served-asset.mjs` 재생성 후 `check:served`가 current여야 한다.
`index.html` 변경은 static parity manifest 재생성을 요구한다.

## 9. 위험과 완화

| 위험 | 완화 |
| --- | --- |
| `app.js` 직접 편집으로 source와 어긋남 | `check:served`가 required 게이트에서 stale을 잡는다 |
| parity manifest 미갱신 | `verify:parity`가 required 게이트에서 실패한다 |
| footer 요소 id 누락 시 조용한 무동작 | 렌더 함수는 요소 부재를 방어하되, 동작 가드가 실제 표시값을 검사한다 |
| 정적 가드가 문구 변경에 과민 반응 | 조작된 **운영값**만 검사하고 레이블/구조는 검사하지 않는다 |
| `state.host` 키 구조 변동 | 기존 `readNested` 패턴을 쓰고 실패 시 `—`로 떨어진다 |

## 10. 완료 조건

- 401 상태에서 `Connected`, `pcv-node-a`, `VM: 3/3`, `API: 10ms avg`, `4/5` 표시 `0`건
- `index.html`에 조작된 운영값 `0`건
- 정적 가드와 동작 가드가 RED → GREEN을 거쳐 통과
- §8 검증 명령 전건 통과
- 제품 .NET 코드, packaging 제품 wrapper, workflow, current/GA evidence 변경 `0`건

## 11. 이 설계가 주장하지 않는 것

- Web secure bootstrap 또는 account auth 구성 완료
- 401 fan-out 제거 또는 login UX 완성
- 설치본 실제 브라우저 E2E required gate 추가
- public trusted signing 또는 external stable publication
- operational anchor 승격
