# 후속 작업 계획 (2026-08-07)

이 문서는 2026-08-06 세션이 남긴 미해결 항목을 **다음 착수자가 코드를 읽지 않고도 시작할 수 있는
수준**으로 정리한다. 각 항목은 근거 문서, 실측한 현재 상태, 착수 조건, 완료 조건, 함정을 갖는다.

선행 기록:

- `docs/followup-work-record-2026-08-06.md` — 2026-08-06 작업 기록 (§1~§14)
- `docs/project-status-audit-2026-08-05.md` — 대형 모듈과 P2-2 항목의 출처
- `docs/ga-ready/EVIDENCE_INDEX.md` — evidence 색인

이 문서의 모든 수치는 `3a390c01`(2026-08-06 최종 push) 기준으로 **직접 측정**했다. 인용이 아니다.

## 0. 우선순위

| # | 항목 | 규모 | 위험 | 선행 조건 |
| ---: | --- | --- | --- | --- |
| 1 | ~~`web/src/served-app.ts` 분해~~ | 큼 | 낮음 | **2026-08-08 완료** |
| 2 | ~~wave 1 소유자 helper 사본 제거~~ | 작음 | 낮음 | **2026-08-08 완료** |
| 3 | ~~`ServiceTokenRotationRevoke` 간헐 실패~~ | 작음 | 낮음 | **2026-08-09 재현+보강** |
| 4 | ~~ADR-0009 argv fidelity 조항~~ | 작음 | 낮음 | **2026-08-08 완료 (A 선택)** |
| 5 | ~~FC-12(b) 수정의 설치본 반영~~ | 큼 | 중간 | **2026-08-08/09 완료** (`0.42.71` package/fullgate/manual-admin/current-card + guest-exec smoke) |
| 6 | `DesktopNodeApiRequestProcessor` 추가 축소 | 작음 | 낮음 | **권고하지 않음** |
| 7 | ~~HyperV / HostServiceAction / JobRuntime 대형 모듈 partial~~ | 중간 | 낮음 | **2026-08-09 완료** |
| 8 | ~~BatchEvidence / HostApplication / 테스트 fixture 분해~~ | 중간 | 낮음 | **2026-08-09 완료** |

`1`~`5`와 ServiceToken 보강, guest-exec 설치본 왕복, 대형 모듈·fixture partial 분해는
2026-08-09 기준으로 닫혔다.

---

## 1. `web/src/served-app.ts` 분해 — 2026-08-08 완료

> **2026-08-08 완료.** evidence는 `docs/ga-ready/evidence/served-app-decomposition-2026-08-08.md`가
> 소유한다. `4,005` → `413`줄, 신규 part `18`개(최대 `mutate.ts` `422`줄), 라쳇 상한 `413`으로 하향.
> §1.3이 제시한 정규화 diff는 그보다 강한 결과가 나왔다 — 최상위 선언 `272`개가 문자 단위로
> 일치하고 **순서조차 바뀌지 않았다.** 연속 구간을 원본 순서로 잘랐기 때문이다.
>
> **이 절이 놓친 함정이 `2`개 있었다.**
>
> 1. `served-app.ts` `1`행이 `// @ts-nocheck`였다. `4,005`줄 전체가 타입 검사 면제였고, 분할하자
>    `tsc`가 `328`건을 보고했다. part마다 지시자를 이어받아 해결했다(tsconfig `exclude`가 아니라 —
>    새 part가 조용히 면제되지 않게). 타입 부채는 갚지 않았다. evidence §3.
> 2. §1.7은 `servedSourceParts`가 `build-served-asset.mjs`에만 있는 것처럼 적었으나 **같은 목록이
>    `scripts/verify-static-parity.mjs`와 `web/tests/PcvDesktopWeb.Static.Tests.ps1`에도 있었고 둘 다
>    stale이 됐다.** 각각 `verify:parity`와 Pester `1`건을 실패시켜 발견했다. Pester 쪽은 빌드
>    스크립트에서 파생하도록 바꿔 네 번째 사본을 만들지 않았다. evidence §4.
>
> §1.4가 우려한 `CONNECTION_STATE_LABELS` TDZ 제약은 **실재하지 않았다.** 최상위 실행문이
> 마지막 `1`줄뿐이고 그것은 `init`을 등록만 하므로, 평가 중에 실행되는 함수가 없다. evidence §5.

**출처:** `docs/project-status-audit-2026-08-05.md`가 기록한 대형 모듈 중 프런트엔드 쪽.
백엔드 절반은 `docs/ga-ready/evidence/api-request-processor-decomposition-2026-08-06.md`로 끝났다.

### 1.1 실측한 현재 상태

| 항목 | 값 |
| --- | ---: |
| `web/src/served-app.ts` | `4,005`줄 |
| top-level `function` 선언 | `217`개 |
| top-level `const`/`let`/`var` 선언 | **`1`개** (`CONNECTION_STATE_LABELS`, `2155`행) |
| `web/src/` 전체 | `5,023`줄 |
| 라쳇 상한 | `4,005` (`module-size-ratchet.json`) |

### 1.2 분해 seam은 이미 존재한다 — 새로 만들 필요가 없다

`web/scripts/build-served-asset.mjs`가 **파일 목록을 순서대로 이어붙여** `web/app.js`를 만든다.

```js
const servedSourceParts = [
  "src/served/types.ts",
  "src/served/state.ts",
  "src/served/routes.ts",
  "src/served/errors.ts",
  "src/served-app.ts"      // <- 남은 단일 덩어리
];
```

`src/served/` 아래로 이미 `5`개 파일(`872`줄)이 분리돼 있다. 즉 이 작업은 **호스트 파일의 `Ops/`
클래스와 같은 상황**이다. 착지점이 이미 있고, 배열에 항목을 추가하며 코드를 옮기면 된다.
번들러도, 모듈 시스템 도입도, 빌드 파이프라인 변경도 필요 없다.

| 기존 파일 | 줄 |
| --- | ---: |
| `web/src/served/types.ts` | `185` |
| `web/src/served/state.ts` | `169` |
| `web/src/served/routes.ts` | `105` |
| `web/src/served/errors.ts` | `196` |
| `web/src/served/api-client.ts` | `217` |

### 1.3 이 작업의 결정적 이점 — 순수 이동을 기계로 증명할 수 있다

`ts.transpileModule`이 `module: ts.ModuleKind.None`으로 전체를 한 번에 컴파일한다. 즉 산출물
`web/app.js`는 **모든 part의 함수를 하나의 최상위 스코프에 담은 단일 파일**이다.

따라서 분해 전후로 `web/app.js`를 diff했을 때 차이가

- `// --- <path> ---` 주석 표식 줄, 그리고
- 함수 정의의 **순서**

**뿐이라면 이동은 순수하다.** 함수 본문이 한 글자라도 바뀌면 diff에 나타난다. 이것은 C# 쪽 분해에는
없었던 검증 수단이다. 계획서는 이 diff를 완료 조건에 포함해야 한다.

절차:

1. 분해 전 `node scripts/build-served-asset.mjs --write` 후 `web/app.js`를 별도로 보관
2. 분해 후 다시 `--write`
3. 두 산출물을 정규화(주석 표식 제거 + 함수 단위 정렬)해 비교 → 차이 `0`이어야 한다

### 1.4 유일한 순서 제약

concatenation 후 한 스코프이므로 `function` 선언은 **호이스팅된다.** 파일 순서를 거의 자유롭게
정할 수 있다. 예외는 top-level `const` **`1`개**뿐이다.

- `CONNECTION_STATE_LABELS` (`served-app.ts:2155`) — `const`는 호이스팅되지 않고 TDZ에 있다.
  이 값을 참조하는 함수가 **로드 시점에 실행되지 않는다면** 순서는 무관하다. 착수 시 이
  전제를 실제로 확인하고, 확인이 안 되면 이 `const`를 `src/served/state.ts`나 새 상수 파일의
  **앞쪽**에 배치한다.

`217`개 함수 중 top-level에서 즉시 실행되는 것이 있는지도 착수 시 확인한다. 있다면 그것이 두 번째
순서 제약이다.

### 1.5 제안하는 분해 축

`served-app.ts`의 함수 이름이 이미 도메인을 드러낸다. 착수자는 아래를 출발점으로 삼되, 실제 함수
목록을 다시 세어 조정한다.

| 새 파일 | 담을 것 (함수 이름 접두어 기준) |
| --- | --- |
| `src/served/evidence.ts` | `getBatchEvidence`, `getCurrentEvidence*`, `evidence*`, `renderEvidence*`, `renderCurrentEvidence*`, `collectEvidenceIssues` |
| `src/served/summary.ts` | `readSummaryValue`, `getSummary*`, `summarySignal*`, `getRuntimeExposure`, `getTokenPolicyLabel`, `getHostReadinessLabel`, `normalizeSummaryIssue` |
| `src/served/table.ts` | `normalizeSearchText`, `collectRowText`, `filterRowsByQuery`, `sortRowsByKey`, `renderTableStateSummary` |
| `src/served/pending.ts` | `*VmActionPending`, `*CheckpointActionPending`, 관련 key 생성기 |
| `src/served/rbac.ts` | `getAccountRoleLabel`, `getAccountPermissions`, `accountRbacModeEnabled`, `rbacAllows`, `requireRbac`, `applyAccountSessionPayload`, `isAuthError`, `tokenRequiredRouteStatus` |
| `src/served/render-vm.ts` | `renderVms`, `matchesVmFilter`, `compareVms`, `getVmUpdatedValue`, `renderCheckpointList` |
| `src/served/render-network.ts` | `getNetworkInventory`, `getNetworkSwitches`, `formatNetworkBoolean`, `renderNetwork*` |
| `src/served/render-qos.ts` | `renderReadbackCard`, `readback*`, `renderVmQosGuestReadback`, `renderQosControlResult`, `getSelectedVmQosControl` |
| `src/served-app.ts` | 남는 것: 초기화, 이벤트 배선, 최상위 렌더 오케스트레이션 |

### 1.6 완료 조건

- `web/src/served-app.ts` 라쳇 상한을 실측값으로 하향, 신규 파일 중 `500`줄을 넘는 것은
  **생성 시점에 라쳇 등록**한다. 등록하지 않으면 대형 모듈을 이름만 바꿔 옮긴 것이 된다
  (2026-08-06 백엔드 분해가 이 이유로 신규 모듈 `2`종을 등록했다).
- `npm test`(= `tsc --noEmit` + `check:served` + `check:frontend-batches`) 통과
- `npm run verify:parity` 통과
- `Invoke-Pester web/tests` 통과
- §1.3의 `app.js` 정규화 diff가 `0`

### 1.7 함정

- **`servedSourceParts`에 파일을 추가하지 않으면 조용히 빠진다.** 배열에 없는 파일은 번들에
  들어가지 않고, `tsc --noEmit`은 통과할 수 있다. 새 파일마다 배열 등록을 확인한다.
- `web/app.js`는 생성물이지만 저장소에 커밋된다. `--write`를 잊으면 `check:served`가 실패한다.
- `web/src/user-visible-fixtures.ts`(`475`줄)는 `servedSourceParts`에 없다. 이 작업 대상이 아니다.

---

## 2. wave 1 소유자의 helper 사본 제거 — 2026-08-08 완료

**출처:** `docs/superpowers/plans/2026-08-06-purecvisor-desktop-node-api-request-processor-decomposition.md`
task `12`. 2026-08-06 세션에서 **실행하지 않았다.**

> **2026-08-08 완료.** evidence는
> `docs/ga-ready/evidence/wave1-owner-helper-copy-removal-2026-08-08.md`가 소유한다.
> 솔루션 테스트 `856` → `857`, `DesktopNode.Api` `7,927` → `7,775`줄.
>
> **아래 §2.2 표의 `11`개는 틀렸다. 실제로는 `14`개였다.** 착수 시 다시 세어 발견한 누락 `3`개는
> `DesktopNodeApiDiagnosticsHandler.JsonFromObject`,
> `DesktopNodeApiOpsSummaryQuery.JobData`, `DesktopNodeApiOpsSummaryQuery.EmptyObject`다. 뒤의 둘은
> `DesktopNodeApiOpsSummaryHandler.cs` 안에 있지만 **클래스가 다르다** — 표를 만들 때 파일이 아니라
> handler 클래스만 훑은 것이 원인이다. 셋 다 관측 동작이 정규 버전과 같아 함께 제거했다.
> 원문은 아래에 그대로 둔다.
>
> §2.2가 "유일하게 실제 확인이 필요"하다고 지목한 `EmptyObject`는 임시 probe로 등가성을 측정해
> 닫았다. `ValueKind`·`GetRawText()`·속성 수·재직렬화·반환 후 `Clone()` 접근이 모두 일치한다.
> 상세는 evidence §3이다.

### 2.1 2026-08-06 기록의 정정

evidence §8.2와 follow-up §14.6은 원래 "auth 쪽 `Body`는 시그니처가 달라 대조가 선행돼야 한다"고
적었다. **그 차단 사유는 실재하지 않는다.** 2026-08-07에 대조했고 아래가 결과다. 두 문서 모두
정정을 달았다.

### 2.2 대조표 (2026-08-07 실측)

정규 버전은 `DesktopNodeApiResponseFactory` / `DesktopNodeApiRequestParsing`이다.

| 사본 | 위치 | 정규 버전과의 차이 | 판정 |
| --- | --- | --- | --- |
| `Body` | `DesktopNodeApiDiagnosticsHandler.cs:463` | 없음 | **제거** |
| `SerializeResponsePayload` | `DesktopNodeApiDiagnosticsHandler.cs:488` | 없음 | **제거** |
| `Json` | `DesktopNodeApiDiagnosticsHandler.cs:474` | 정규 버전이 지역변수 `body`를 경유. 출력 동일 | **제거** |
| `Failure` | `DesktopNodeApiDiagnosticsHandler.cs:452` | 파라미터 `6`개. 정규 버전은 `7`개이고 `7`번째가 `= null`. `6`인자 호출의 출력 동일 | **제거** |
| `Body` | `DesktopNodeApiAuthSessionHandler.cs:247` | **시그니처 동일**, 줄바꿈만 다름 | **제거** |
| `Failure` | `DesktopNodeApiAuthSessionHandler.cs:229` | **시그니처 동일**(`7`개), 줄바꿈만 다름 | **제거** |
| `Json` | `DesktopNodeApiAuthSessionHandler.cs:262` | 직렬화를 인라인. 출력 동일 | **제거** |
| `TryParseBody` | `DesktopNodeApiAuthSessionHandler.cs:169` | 줄바꿈만 다름. 오류 코드·메시지 `3`종 모두 동일 | **제거** |
| `ParsedJson` | `DesktopNodeApiAuthSessionHandler.cs:270` | 없음 | **제거** |
| `EmptyObject` | `DesktopNodeApiAuthSessionHandler.cs:209` | **구현이 다르다.** auth는 `JsonSerializer.SerializeToElement(...)`, 정규 버전은 `JsonFromObject(...)`(= `JsonDocument.Parse` + `Clone`). 빈 객체 `{}`에 대한 관측 값은 같지만 같은 코드가 아니다 | 제거 가능하나 **`11`개 중 유일하게 실제 확인이 필요한 항목** |
| `Json` | `DesktopNodeApiOpsSummaryHandler.cs:149` | 직렬화를 인라인. 출력 동일 | **제거** |
| `AuthValidationFailure` | `DesktopNodeApiAuthSessionHandler.cs:222` | 정규 버전에 없음. `DesktopNodeAuthValidationResult`를 받는 auth 고유 wrapper | **유지** |
| `AuthResult` | `DesktopNodeApiAuthSessionHandler.cs:216` | 정규 버전에 없음. auth 고유 | **유지** |

제거 대상 `11`개, 유지 `2`개다. 유지 대상 `2`개는 정규 버전과 **이름이 겹치지 않으므로** 남겨도
"네 번째 진실 원본" 문제를 만들지 않는다.

`11`개 중 `10`개는 줄바꿈·기본 인자·지역변수 차이뿐이라 대조가 끝났다. **`EmptyObject` 하나만
구현 방식이 다르므로**, 그것만 착수 시 실제로 확인한다. 이것이 2026-08-06에 "대조가 선행돼야
한다"고 적었던 것의 실제 크기다 — 항목 `13`개가 아니라 `1`개였다.

### 2.3 착수 순서

1. 위 `11`개를 지우고 호출부를 `DesktopNodeApiResponseFactory.*` / `DesktopNodeApiRequestParsing.*`로
   바꾼다. `AuthValidationFailure`와 `AuthResult`의 **본문 안** `Json`/`Body` 호출도 함께 바꾼다.
2. guard를 추가한다. `ApiRequestProcessorDecompositionOwnershipTests`에
   `Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies`를 넣고, 세 소유자가 `Json`/`Body`/
   `Failure`/`SerializeResponsePayload`/`TryParseBody`를 **선언하지 않음**을 단언한다.
   `AssertProcessorDoesNotDeclare`와 같은 형태의 helper가 이미 그 파일에 있다.
3. guard 비공허를 실측한다. 사본 하나를 되살려 그 `[Fact]` 하나만 실패하는지 확인하고 되돌린다.

### 2.4 완료 조건

`dotnet test src/DesktopNode.sln` 통과. 착수 시점 기준선은 `856`이며 guard `1`건이 늘어난다.
`ApiArchitectureOwnershipTests`가 세 소유자의 필드·메서드를 단언하므로 **함께** 통과해야 한다.

### 2.5 함정

- `ApiArchitectureOwnershipTests.RequestProcessorDelegatesDiagnosticsBehaviorToCallbackFreeOwner`가
  diagnostics 소유자의 `retiredProcessorMethods` 목록과 생성자 시그니처를 단언한다. 사본 제거는
  거기 걸리지 않지만, 실패하면 **단언을 지우지 말고** 무엇이 바뀌었는지 먼저 확인한다.
  2026-08-06에 같은 상황에서 `ResolveActor` 단언을 목적 보존 형태로 갱신한 전례가 있다
  (evidence §4).
- diagnostics의 `Failure`는 파라미터가 `6`개다. 정규 버전으로 바꿔도 호출부는 그대로 컴파일되지만,
  누군가 나중에 `7`번째 인자를 넘기면 응답에 `recommended_action`이 **추가된다.** 그것은 계약
  변경이므로 이 작업에서는 하지 않는다.

---

## 3. `ServiceTokenRotationRevoke...RedactedAudit` 간헐 실패 — 2026-08-09 재현+보강

**출처:** `docs/followup-work-record-2026-08-06.md` §12.
**evidence:** `docs/ga-ready/evidence/service-token-rotation-replace-hardening-2026-08-09.md`.

> **2026-08-09.** 전체 `DesktopNode.Host.Tests` 부하에서 재현됐다. 진단 단언이
> `PCV_HOST_SERVICE_TOKEN_ROTATION_FAILED` / `바꿀 파일을 제거할 수 없습니다` /
> `backup_write_status=written` / `atomic_replace_status=not-run`을 출력했다. 제품 경로는
> `File.Copy`+`File.Replace(null)`를 backup 경로 포함 단일 `File.Replace` + `IOException`
> short retry로 바꿨다. `File.Move` 통일은 하지 않았다(§12.3 반증 유지).

### 3.1 상태 (역사): 착수할 수 없다

원인 미확정이고, **재현되지 않는다.** 2026-08-06 세션에서 `82`회 재현을 시도해 실패했다. 최초
관측은 `1`회이며 실패 메시지가 남지 않았다.

가설 하나는 반증됐다. 구현자와 조사자가 각각 `File.Replace`의 일시적 공유 위반을 의심하고
"일관성을 위해 `File.Move`로 바꾸자"고 제안했으나, 실측 결과 `File.Replace`는
`ReadWrite|Delete` 공유 아래에서 `0/60` 실패, `File.Move`는 `60/60` 실패였다. 제안대로 고쳤다면
**코드가 더 나빠졌다.**

### 3.2 이미 해 둔 것

`src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`의 단언을 진단 정보를 실어 나르는
형태로 바꿨다. 다음에 재현되면 실패 메시지가 `error_code`, `error_message`,
`service_token_mutation`, `atomic_replace_status`, `backup_write_status`, `service_reload_status`,
양쪽 token hash를 그대로 출력한다.

### 3.3 착수 조건

**재현 관측 1회.** 그 전까지는 아무 것도 하지 않는다. `82`회 무재현은 부재의 증명이 아니지만,
근거 없는 수정을 시작할 근거도 아니다.

재현되면 `superpowers:systematic-debugging`의 Phase 1부터 시작한다. 실패 메시지가 이미 원인을
말하고 있을 가능성이 높다.

### 3.4 함정

- **"어차피 flaky니 재시도를 넣자"는 증상 처리다.** Iron Law에 걸린다. 근본 원인 없이 재시도를
  넣으면 다음 재현이 영원히 오지 않고 원인도 영원히 모른다.
- 테스트를 `Skip`하지 않는다. 관측 가능성을 스스로 없애는 행위다.

---

## 4. ADR-0009에 argv fidelity 조항이 없다 — 2026-08-08 완료

**출처:** `docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md`.

> **2026-08-08 결정: A.** 사용자가 §4.2의 선택지 `A`(조항 추가)를 골랐다.
> `docs/adr/0009-guest-execution-security-boundary.md`에 `## Argv Fidelity 경계` 절과
> 결정 마커 `argv_fidelity_policy` / `argv_fidelity_evidence`, 검증 Gate `10`번을 추가했다.
> 조항 본문은 "guest 실행 경계를 넘는 인자는 데이터로 전달하며 guest 측에서 코드로 재해석하지
> 않는다"이며, 금지 대상(공백·따옴표·subexpression·문장 구분자·파이프·리다이렉션)과 UTF-8 왕복
> 보존을 표로 고정했다. 계약은 `GuestExecutionArgvFidelityTests` `6`건이 이미 잠그고 있으므로
> 구현 변경은 없다 — 문서가 테스트를 뒤따라온 것이다.

### 4.1 상태

FC-12(b)의 근본 원인은 인코딩이 아니라 **argv를 공백으로 이어붙여 guest에서 재파싱한 것**이었다.
`DesktopNodeHyperVPowerShellDirectGuestExecutionProvider`는 이제 argv를 데이터로 전달하고
`GuestExecutionArgvFidelityTests` `6`건이 그 계약을 잠근다.

그러나 **ADR-0009 자체에는 argv fidelity 조항이 없다.** PCVCLI 계약 문서가
`pcvcli vm guest-exec <vm> -- <command>`로 argv 전달을 문서화하고 있었을 뿐이고, 구현이 그것을
지키지 않아도 어떤 ADR도 위반되지 않는 상태였다.

### 4.2 결정이 필요하다

ADR 개정은 결정 행위이므로 이 문서가 대신 결정하지 않는다. 선택지는 둘이다.

| 선택 | 내용 |
| --- | --- |
| A | ADR-0009에 조항 추가: "guest 실행 경계를 넘는 인자는 데이터로 전달하며 guest 측에서 코드로 재해석하지 않는다." 테스트가 이미 있으므로 문서만 따라온다. |
| B | 개정하지 않는다. 테스트가 계약을 잠그고 있으므로 ADR 없이도 회귀는 막힌다. 대신 ADR과 실제 보안 경계 사이에 기록되지 않은 간극이 남는다. |

권고는 **A**다. 이번 결함이 "문서화된 계약을 구현이 지키지 않아도 아무 게이트도 울리지 않는다"는
구조에서 나왔고, B는 그 구조를 그대로 둔다.

---

## 5. FC-12(b) 수정의 설치본 반영 — 2026-08-08/09 완료

**출처:** `docs/ga-ready/evidence/guest-exec-argv-fidelity-fc-12b-closure-2026-08-06.md` Nonclaims.

> **2026-08-08/09 완료.** `0.42.71-admin-smoke` package / fullgate / manual-admin
> `0.42.70 -> 0.42.71` / installed current-card로 설치본에 반영됐다.
>
> | evidence | 결과 |
> | --- | --- |
> | package | `docs/ga-ready/evidence/admin-smoke-package-2026-08-08-04271.md` |
> | fullgate | `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-08-08-04271-hostmutation.md` (`GuestArgvInvocation` 설치본 문자열 PASS) |
> | manual-admin | `docs/ga-ready/evidence/manual-admin-campaign-2026-08-08-04270-04271.md` |
> | current-card | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-08-09-04271.md` |
>
> credentialed Windows guest `guest-exec` 왕복 재실행은 별도 smoke로 남겨 두었고, fullgate
> Hyper-V route smoke + 설치본 문자열 + source tests가 설치본 반영 조건을 닫는다.

### 5.1 상태 (역사)

설치본 `0.42.70-admin-smoke`에는 수정 전 argv join 코드가 들어 있었다. 2026-08-06 검증은
source/guest 확인이었고 operational anchor는 당시 `0.42.70`이었다. 위 완료 블록이 그 뒤
상태를 소유한다.

---

## 6. `DesktopNodeApiRequestProcessor` 추가 축소 — 권고하지 않음

**출처:** `docs/ga-ready/evidence/api-request-processor-decomposition-2026-08-06.md` §8.1.

계획서 목표는 `450`줄이었고 결과는 `495`줄이다. 남은 `495`줄의 구성은 아래와 같다.

| 구성 | 줄 |
| --- | ---: |
| public record `5`종 | `52` |
| 생성자와 factory `2`종 | `85` |
| `Handle` / `HandleCoreWithRouteTimeout` | `75` |
| `HandleCore` dispatch | `120` |
| route timeout probe | `13` |
| worker 공개 표면 `3`종 | `85` |
| 필드와 using | `65` |

더 줄이려면 남은 것 중 하나를 **공개 표면에서 떼어내야 한다.** 그것은 호출자 계약 변경이고,
이 항목이 원래 해결하려던 문제(대형 모듈)와 무관하다. `495`줄은 라쳇에 고정돼 있으므로 다시
늘어나지 않는다. **닫힌 것으로 취급한다.**

---

## 부록: 이 문서를 갱신할 때

- 항목이 끝나면 이 문서에서 지우지 말고 **완료 표시와 evidence 링크**를 남긴다. 지우면 왜 그렇게
  결정했는지가 사라진다.
- 수치를 인용하지 말고 다시 측정한다. 2026-08-06 세션에서 문서화된 주장이 측정에 반증된 사례가
  `4`건 있었다(§10.4 `MEASURE` 전제, §13 인코딩 가설, §14.3 `JobStoreCommitError` 가정,
  그리고 이 문서 §2.1의 auth `Body` 시그니처 주장).
  **2026-08-08에 `5`번째가 나왔다** — 이 문서 §2.2의 사본 `11`개 주장이 실제로는 `14`개였다.
  이번에는 이 문서 자신이 반증 대상이었다. 계획서를 쓴 사람도 같은 규칙을 적용받는다.
