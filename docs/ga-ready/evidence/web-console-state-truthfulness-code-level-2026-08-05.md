# Web Console 운영 상태 진실성 code-level PASS (2026-08-05)

## Evidence boundary

- Spec: `docs/superpowers/specs/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness-design.md`
- Plan: `docs/superpowers/plans/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness.md`
- 근거 감사: `docs/project-status-audit-2026-08-05.md` §6 P1-2
- Result: `CODE_LEVEL_PASS`
- Host/service mutation performed: `false`
- Hyper-V/VM mutation performed: `false`
- Package build performed: `false`
- Installed listener execution: `not-run`
- Actual-VM validation performed: `false`
- Public trusted signing: `false`
- External stable publication: `false`

이 evidence는 code-level 범위다. 설치본 실제 브라우저 authenticated journey를 주장하지 않으며
operational anchor를 승격하지 않는다.

## 변경 내용

- `web/index.html`에서 조작된 운영값을 제거하고 실제 바인딩용 요소 id 8개를 노출했다.
- 계측 인프라가 없는 `WS`/`API latency` span을 삭제했다.
- `host.status`에 머신 이름 필드가 없으므로 호스트 식별자 자리를 `windows.caption`으로 재바인딩하고
  hero chip 라벨을 `호스트 모드`에서 `호스트 상태`로 바꿨다.
- `state.lastRefreshedAt`과 `hasRefreshedOperation()`으로 미로드와 실제 0을 구분한다.
- `hasRefreshedOperation()`은 기록된 실패 중 하나라도 `isAuthError()`면 **모든 operation**에 대해
  `false`를 반환한다. Local API는 401을 route별 operation으로 태그하지 않고 두 401 지점
  (`DesktopNodeHostApplication.Json`, `DesktopNodeApiAuthSessionHandler.AuthValidationFailure`)이
  모두 `operation: "api.auth"` 하나로 응답하기 때문이다. operation 일치만 검사하면 전면 401에서
  모든 route가 "성공"으로 읽혀 게이트가 무력화된다.
- `renderStatusBar()`와 `renderHeroChips()`는 표시 직전 `hasRefreshedOperation()`으로 게이트하며,
  hero chip의 `getHostReadinessLabel()` 호출은 이 게이트 뒤에 있다.
- footer 연결 표기와 `#connection-state` 배지는 동일한 `CONNECTION_STATE_LABELS` 맵을 공유해
  부분 실패에서 서로 모순되지 않는다. spec §6이 고정한 초기/미인증 행만 footer가
  `Not connected`로 표기한다.

## Known-open residual (이 slice 범위 밖)

`getHostReadinessLabel()`은 `state.host`가 `null`이어도 `Ready`를 반환한다. 이 fallback은
제거하지 않았고, 로드 게이트는 footer/hero 두 렌더 함수에만 적용했다. 함수 호출 지점은 총
`4`개이며 그중 게이트 뒤에 있는 것은 `renderHeroChips()` `1`개다. 나머지 `3`개는 게이트 없이
호출된다.

| 미게이트 호출 지점 | 영향 |
| --- | --- |
| `served-app.ts` `getPriorityItems()` | host readiness 우선순위 판정이 미로드 상태에서 `Ready`로 읽힌다 |
| `served-app.ts` `renderMetrics()` | `#metric-grid`가 전면 401에서 `Host Ready / VMs 0 / Running 0`을 렌더한다 |
| `served-app.ts` `renderOpsCockpit()` | `#ops-summary-panel`이 전면 401에서 `Host readiness Ready / VMs total/running 0 / 0`을 렌더한다 |

위 두 값은 browser fixture 전면 401 시나리오에서 실제로 렌더된 문자열을 관측한 것이다
(fixture 관측이며 설치본 브라우저 관측이 아니다). Metrics 패널과 Ops Cockpit은 이 slice의
변경 대상이 아니었고, 별도 slice로 남긴다.

## 검증 결과

아래는 이 문서 갱신 시점에 실제 실행해 관측한 결과다.

| 검증 | 결과 |
| --- | --- |
| `npm run browser:fixture --prefix web` | PASS (`browser fixture verification passed`) |
| `npm test --prefix web` | PASS (`served app.js is current`, `5 batches, 25 work items`) |
| `npm run verify:parity --prefix web` | PASS (`static parity manifest is current`, `static parity verification passed`) |
| `node --check web/app.js` | PASS |
| Web Pester (`web/tests`) | `49/49 PASS`, `0` failed |
| `git diff --check` | PASS (출력 없음) |
| `index.html` 잔존 조작값 | `0`건 |

최초 evidence 커밋(`8299f7a4`) 시점에는 Installer Pester `49/49 PASS`, Packaging Pester
`466/466 PASS`도 함께 관측했다. 이 수정 wave는 `web/**`와 evidence 문서만 변경했으므로 두
suite를 재실행하지 않았다.

동작 가드는 구현 전 RED를 먼저 확인했다. fixture의 401 operation을 제품 계약
(`api.auth`)으로 교정하자 `unauthenticated status bar is missing VM: —`로 실패했고, 관측된
렌더값은 footer `VM: 0/0`, hero `0/0` / `Ready`였다. `hasRefreshedOperation()` 수정 후 GREEN이 됐다.

## Nonclaims

- Web secure bootstrap 또는 account auth 구성을 완료하지 않았다.
- 미인증 401 fan-out을 제거하지 않았다.
- 설치본 실제 Chromium E2E를 required gate에 추가하지 않았다. 미인증/세션 만료 표시 동작은
  browser fixture(Node `vm` 컨텍스트) 관측이며 설치본 브라우저 관측이 아니다.
- `getHostReadinessLabel()`의 `Ready` fallback을 제거하지 않았다. 위 known-open residual의
  미게이트 호출 지점 `3`개는 이 slice에서 고치지 않았다.
- Installer/Packaging Pester는 이 수정 wave에서 재실행하지 않았다.
- 제품 .NET 코드, packaging 제품 wrapper, workflow를 변경하지 않았다.

## 부록 A. Known-open residual closure addendum (2026-08-05)

이 부록은 위 snapshot을 수정하거나 소급 재해석하지 않는다. 위 `Known-open residual` 절이
남긴 미게이트 호출 지점 `3`개에 대한 후속 slice의 구현·검증 상태만 추가한다.

### 해소한 항목

- `getHostReadinessLabel()`의 `Ready` fallback을 제거했다. 이전 구현은
  `state.host?.supported === false`로 판정해 `state.host`가 `null`일 때도 `Ready`를 반환했다.
  이제 `ops.summary` readiness가 없고 `host.status`가 로드되지 않았으면 `—`를 반환한다.
- 게이트를 호출 지점이 아니라 helper 안에 두었으므로 `getPriorityItems()`, `renderMetrics()`,
  `renderOpsCockpit()`, `renderHeroChips()` `4`개 지점이 한 번에 닫혔다.
- `renderMetrics()`와 `renderOpsCockpit()`의 VM 수치는 `hasRefreshedOperation('vm.list')`로
  게이트했다. `getSummaryVmCounts()`가 `vms.length`로 fallback해 미로드와 실제 `0`을 구분하지
  못하기 때문이다.
- `renderHeroChips()`의 중복 외부 게이트를 제거했다. helper가 자체 게이트하므로 `ops.summary`만
  성공한 경우 hero chip과 두 패널이 이제 같은 값을 표시한다.

### 변경하지 않은 항목

- Job 수치는 게이트하지 않았다. `getSummaryJobCounts()`는 browser-tracked job에도 기반하며
  이 값은 서버 없이도 실재한다.
- `getPriorityItems()`의 임계 판정 로직은 그대로다. 카운트를 `> 0` 비교에만 쓰므로 미로드
  상태에서 경보를 만들지 않는다.

### 검증

동작 가드를 먼저 추가해 RED를 확인했다: `unauthenticated metric grid must not include Ready`.
단언은 카드 label을 포함한 정확한 형태로 작성해 Jobs 카드의 동일 숫자쌍과 혼동되지 않게 했다.

| 검증 | 결과 |
| --- | --- |
| `npm run browser:fixture --prefix web` | PASS |
| `npm test --prefix web` | PASS |
| `npm run verify:parity --prefix web` | PASS |
| `node --check web/app.js` | PASS |
| Web Pester | `49/49` PASS |
| Packaging Pester | `467/467` PASS |
| Installer Pester | `49/49` PASS |

이 addendum도 code-level 범위이며 설치본 브라우저 관측, operational anchor 승격, public trusted
signing, external stable publication을 주장하지 않는다.
