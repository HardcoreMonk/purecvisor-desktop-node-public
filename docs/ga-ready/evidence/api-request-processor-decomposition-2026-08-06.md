# DesktopNodeApiRequestProcessor 도메인 분해 (2026-08-06)

evidence_id: `api-request-processor-decomposition-2026-08-06`
result: `PASS`
evidence_scope: `source-decomposition-with-il-level-ownership-guards`
host_mutation_performed: `false`
guest_command_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

계획서: `docs/superpowers/plans/2026-08-06-purecvisor-desktop-node-api-request-processor-decomposition.md`

`docs/project-status-audit-2026-08-05.md`가 남긴 대형 모듈 `2`종 중 백엔드 쪽을 해소한다.
프런트엔드 `web/src/served-app.ts`(`4,005`줄)는 이 작업의 범위가 아니다.

## 1. 결과

| 항목 | 착수 | 종료 |
| --- | ---: | ---: |
| `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` | `3,367`줄 | **`495`줄** |
| `DesktopNode.Api` 전체 | `7,635`줄 | `7,927`줄 |
| 솔루션 테스트 | `842` | **`856`** |
| callback adapter | `2`종 | `0`종 |

`DesktopNode.Api` 전체 줄 수가 `292`줄 늘어난 것은 타입 선언, 생성자, using이 `13`개 파일로 복제된
비용이다. 이동 자체는 순증을 만들지 않았다.

## 2. 새 소유자

전부 `sealed`이고, `Func`/`Action`/`Delegate` 필드·파라미터가 없으며, `DesktopNodeApiRequestProcessor`를
역참조하지 않는다. wave 1A~1D가 `DesktopNodeApiDiagnosticsHandler` / `DesktopNodeApiAuthSessionHandler` /
`DesktopNodeApiOpsSummaryHandler`에 세운 형태를 그대로 반복했다.

| 파일 | 줄 | 소유 |
| --- | ---: | --- |
| `DesktopNodeApiResponseFactory.cs` | `140` | 응답 봉투 생성 `11`종 |
| `DesktopNodeApiJsonReader.cs` | `226` | `JsonElement` 읽기 `14`종 |
| `DesktopNodeApiRequestParsing.cs` | `185` | 본문·경로·쿼리 파싱 `8`종 + 중첩 record `3`종 |
| `DesktopNodeApiErrorMapping.cs` | `62` | 오류 매핑 `4`종 |
| `DesktopNodeApiHyperVOperationInvoker.cs` | `74` | native adapter 호출 |
| `DesktopNodeApiConsoleRouteHandler.cs` | `120` | console capabilities/session |
| `DesktopNodeApiGuestExecutionRouteHandler.cs` | `206` | guest exec/channel preview와 ADR-0009 차단 |
| `DesktopNodeApiJobRouteHandler.cs` | `169` | job list/get/cancel/retry, vm delete-status |
| `DesktopNodeApiJobReconciliationHandler.cs` | `856` | reconcile 경로와 baseline 캡처 |
| `DesktopNodeApiVmMutationRouteHandler.cs` | `770` | queued mutation과 QoS |
| `DesktopNodeApiVmReadRouteHandler.cs` | `154` | GET 조회 경로 종단 |
| `DesktopNodeApiJobWorker.cs` | `131` | worker tick |
| `DesktopNodeApiRequestThrottle.cs` | `137` | rate limit 창과 route timeout 응답 |

**삭제:** `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`. 이 파일의
`DesktopNodeApiJobRuntimeHandler`와 `DesktopNodeApiConsoleHandler`는 `Func` `7`개를 받아 곧바로
processor의 private 메서드로 되돌려 보내는 callback adapter였다. wave 1이 diagnostics/auth/ops에서
없앤 형태가 job과 console에는 남아 있었고, 이번에 callback-free 소유자로 대체하며 제거했다.

## 3. 순수하지 않은 이동 `3`건

이동의 대부분은 본문을 한 글자도 바꾸지 않았다. 아래 `3`건은 예외이며 각각 이유가 있다.

### 3.1 `JobStoreCommitError` 시그니처 변경

계획서는 이 메서드가 "인스턴스 상태를 쓰지 않으므로 `static`으로 올린다"고 적었다. **그 가정이
틀렸다.** 본문이 `jobRuntime.LoadBlock`을 두 번 읽는다. 측정으로 드러났고, 세 번째 파라미터
`DesktopNodeJobRuntimeError? loadBlock`을 추가해 호출부 `3`곳이 `jobRuntime.LoadBlock`을 넘기도록
했다. 읽는 값과 읽는 시점이 모두 같으므로 동작은 동일하다.

### 3.2 `sync` 잠금 객체 공유

`DesktopNodeApiJobWorker`와 `DesktopNodeApiRequestThrottle`은 자기 잠금을 만들지 않고
processor의 `sync`를 생성자로 받는다. 이 잠금은 요청 처리와 worker tick 사이의 상호 배제이므로,
소유자가 새 잠금을 만들면 배제가 조용히 사라진다. `Handle`이 이미 `sync`를 잡은 채
`throttle.Enforce`를 호출하는 재진입 관계도 그대로 유지된다.

### 3.3 `BeforeJobFinalization` 예외

이 seam은 `Action?`이라 callback-free 규칙과 충돌한다. 도메인 협력자가 아니라 provider 결과와
직렬화된 finalization 사이 경계를 결정적으로 만드는 테스트 seam이므로 예외로 남겼다. processor의
`internal Action? BeforeJobFinalization`은 상태를 갖지 않고 worker로 위임하는 속성으로 바꿨다 —
사본이 둘이면 테스트가 한쪽을 걸고 다른 쪽을 관찰할 수 있다.

`AssertDeclaredCallbackFieldsAre`가 이 예외의 크기를 `"정확히 이 하나"`로 고정한다. 예외를 조용히
두면 다음 사람이 두 번째 callback을 추가한다.

## 4. 기존 guard 수정 `1`건

`ApiArchitectureOwnershipTests.RequestProcessorDelegatesAuthSessionBehaviorToCallbackFreeOwner`가
`processorCalls`에 `DesktopNodeApiAuthSessionHandler.ResolveActor` 호출이 있음을 단언하고 있었고,
이번 이동으로 **실패했다.** actor를 해석하던 `HandleGuestExecPreviewRoute` / `QueueVmGuestExec` 등이
processor를 떠났기 때문이다.

단언의 목적 — actor 해석은 auth 소유자에 남고 누구도 다시 구현하지 않는다 — 은 바뀌지 않았으므로,
호출자를 processor로 고정하는 대신 실제 호출자(`DesktopNodeApiGuestExecutionRouteHandler`,
`DesktopNodeApiVmMutationRouteHandler`)를 확인하도록 바꿨다. 단언을 삭제하거나 약화하지 않았다.

## 5. 신규 guard와 비공허 실측

`src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs` — `[Fact]` `14`개.
`BindingFlags`가 아니라 `PEReader`/`MetadataReader`로 읽는다
(`csharp-architecture-test-migration.json`이 테스트 코드의 private reflection을 `0`으로 고정한다).
IL 호출 그래프 디코더는 `ApiArchitectureOwnershipTests`의 것을 빌려 쓴다 — 두 벌을 두면 서로 다르게
틀릴 수 있다.

guard가 공허하지 않음을 실측했다. 각 probe는 확인 후 되돌렸다.

| probe | 주입한 결함 | 결과 |
| --- | --- | --- |
| 1 | `DesktopNodeApiConsoleRouteHandler`에 `Func<string, string>?` 필드 추가 | `ConsoleRoutesUseACallbackFreeOwner` **FAIL** |
| 2 | `DesktopNodeApiJobWorker`에 두 번째 `Action?` seam 추가 | `WorkerOwnerCarriesExactlyOneDeclaredCallbackSeam` **FAIL** |
| 3 | 삭제한 `DesktopNodeApiConsoleHandler` 타입을 다시 선언 | `ConsoleRoutesUseACallbackFreeOwner` **FAIL** |

세 probe 모두 의도한 테스트 **하나만** 실패시켰다. 나머지 `247`건은 통과했다.

## 6. 검증

| 명령 | 결과 |
| --- | --- |
| `dotnet build src/DesktopNode.Api/DesktopNode.Api.csproj` | 오류 `0`, 경고 `0` |
| `dotnet test src/DesktopNode.sln` | 통과 `856`, 실패 `0`, 건너뜀 `0` |
| `Invoke-Pester packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1` | 통과 `3`, 실패 `0` |

프로젝트별: Contracts `21`, Service `11`, Cli `113`, Runtime `126`, HyperV `137`, Host `198`,
Api `250`(착수 `236`).

## 7. 라쳇

`packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`:

- `DesktopNodeApiRequestProcessor.cs` 상한 `3,367` → **`495`**
- `DesktopNodeApiJobReconciliationHandler.cs` **신규 등록**, 상한 `856`
- `DesktopNodeApiVmMutationRouteHandler.cs` **신규 등록**, 상한 `770`

새로 만든 대형 모듈 `2`종을 생성 시점에 라쳇에 넣었다. 넣지 않으면 이 작업은 `3,367`줄짜리 모듈을
`856`줄과 `770`줄짜리 **무제한** 모듈로 옮긴 것이 되고, 라쳇이 막으려던 것을 그대로 통과시킨다.

## 8. 계획서와 다르게 끝난 것

정직하게 적는다. 아래 `3`건은 계획서가 약속한 것과 결과가 다르다.

| # | 계획 | 실제 | 판단 |
| ---: | --- | --- | --- |
| 1 | processor `450`줄 이하 | `495`줄 | 미달. 원인은 §8.1 |
| 2 | task `12` = wave 1 소유자 helper 사본 제거 | **하지 않았다** | §8.2 |
| 3 | task `12`개 / 커밋 `12`개 | wave `3`회 / 커밋 `1`회 | §8.3 |

### 8.1 `450`줄 목표 미달

계획서의 `450`줄 추정은 `HandleCore`가 dispatch만 남길 것으로 봤는데, 실제로는 GET 조회 분기
`6`종과 최종 fallthrough가 `HandleCore` 안에 그대로 있었다. 계획서의 이동 지도에 그 블록이 없었던
것이 원인이다. 계획에 없던 `DesktopNodeApiVmReadRouteHandler`(`154`줄)를 추가로 만들어
`617` → `495`줄까지 줄였고, 남은 `495`줄은 public record `5`종(`52`), 생성자와 factory(`85`),
`Handle`/`HandleCoreWithRouteTimeout`(`75`), `HandleCore` dispatch(`120`), timeout probe(`13`),
worker 공개 표면 `3`종(`85`), 필드와 using(`65`)이다. 더 줄이려면 남은 것 중 하나를 공개 표면에서
떼어내야 하므로 이 작업의 범위를 벗어난다.

### 8.2 wave 1 소유자의 helper 사본은 남아 있다

`DesktopNodeApiDiagnosticsHandler`, `DesktopNodeApiAuthSessionHandler`,
`DesktopNodeApiOpsSummaryHandler`는 각자 `Json` / `Body` / `Failure` / `SerializeResponsePayload` /
`TryParseBody` / `ParsedJson` 사본을 갖고 있다. 이번에 `DesktopNodeApiResponseFactory`와
`DesktopNodeApiRequestParsing`이 생겼으므로 그 사본은 네 번째 진실 원본이다.

**제거하지 않았다.** 남은 작업으로 넘긴다.

> **2026-08-07 정정.** 이 절은 원래 "`DesktopNodeApiAuthSessionHandler.Body`는 시그니처가 다르고
> `AuthValidationFailure` 같은 자체 wrapper도 있어 문자 단위 대조가 선행돼야 한다"고 적었다.
> **그 차단 사유는 실재하지 않는다.** 2026-08-07에 대조한 결과 auth 쪽 `Body`는 시그니처가 같고
> 줄바꿈만 다르다. `Failure`도 파라미터 `7`개로 정규 버전과 같다. `AuthValidationFailure`와
> `AuthResult`는 auth 고유 wrapper가 맞지만 정규 버전과 이름이 겹치지 않으므로 대체 대상이 아니다.
> 대조 결과와 제거 계획은 `docs/followup-work-plan-2026-08-07.md` §2에 있다.

> **2026-08-08 종결.** 사본을 제거했다. evidence는
> `docs/ga-ready/evidence/wave1-owner-helper-copy-removal-2026-08-08.md`가 소유한다.
> 이 절이 열거한 `Json` / `Body` / `Failure` / `SerializeResponsePayload` / `TryParseBody` /
> `ParsedJson` 외에 `JsonFromObject`, `JobData`, `EmptyObject`도 사본이었고, 최종 제거 수는
> `14`개다. `AuthValidationFailure`와 `AuthResult` `2`종만 auth 고유 wrapper로 남았다.
> `Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies`가 재도입을 막는다.
> 이로써 §8의 미이행 `3`건 중 `2`번이 닫혔다.

### 8.3 실행 단위

계획서는 task마다 커밋 `1`개를 요구했다. 실제로는 사용자가 병렬 실행을 지시해, 신규 파일 생성을
`3`개 wave로 병렬화하고 원본 제거·재배선을 직렬로 한 번에 처리했다. `13`개 파일이 모두 같은
원본 파일에서 나오므로 구현을 task 단위로 병렬화하면 충돌한다 — 신규 파일 생성만 병렬화하고
processor 수정은 직렬로 두는 것이 안전하게 얻을 수 있는 최대 병렬도였다. 결과적으로 커밋이
`1`개이므로 task 단위 이분 탐색은 불가능하다.

## Nonclaims

- 설치본을 만들지 않았고 operational anchor를 승격하지 않는다. `0.42.70-admin-smoke` 그대로다.
- 동작 변경을 주장하지 않는다. 이 작업은 순수 이동이며 §3의 `3`건만 예외다.
- 성능 개선을 주장하지 않는다. 측정하지 않았다.
- `web/src/served-app.ts` 분해는 범위 밖이며 손대지 않았다.
- public trusted signing과 external stable publication은 범위 밖이며 주장하지 않는다.
