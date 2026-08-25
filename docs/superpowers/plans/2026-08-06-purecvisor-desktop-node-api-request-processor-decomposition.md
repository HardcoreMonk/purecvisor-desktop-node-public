# DesktopNodeApiRequestProcessor 도메인 분해 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`(`3,367`줄)에서 공용 helper와 도메인 경로 처리를 소유자 타입으로 옮겨, 남은 대형 모듈 2종 중 백엔드 쪽을 해소한다.

**Architecture:** 새 패턴을 발명하지 않는다. 이 파일은 이미 wave 1A~1D에서 `DesktopNodeApiDiagnosticsHandler` / `DesktopNodeApiAuthSessionHandler` / `DesktopNodeApiOpsSummaryHandler` 세 개의 **callback-free 소유자**를 떼어냈고, `ApiArchitectureOwnershipTests`가 그 형태를 IL 수준에서 잠그고 있다. 이 작업은 같은 형태를 남은 도메인에 반복 적용한다. `HandleCore`는 계속 유일한 dispatcher로 남고, 각 소유자는 `TryHandle`을 노출하며, 소유자는 `Func`/`Action`/`Delegate` 필드·파라미터를 갖지 않고 `DesktopNodeApiRequestProcessor`를 역참조하지 않는다.

순서는 **공용 helper 먼저, 도메인 소유자 나중**이다. 도메인 블록 전부가 `Json`/`Body`/`Failure`/`Read*` 같은 private static helper에 의존하므로, helper가 공용 타입으로 나가기 전에는 어떤 도메인도 파일을 떠날 수 없다.

**Tech Stack:** C# / .NET 10 (`net10.0-windows`), xUnit, `System.Reflection.Metadata`(ownership guard), Pester 5(라인 수 gate)

## Global Constraints

- **공개 표면을 바꾸지 않는다.** `DesktopNodeApiRequestProcessor`의 `public` 멤버 — `CreateDefault`, `Handle`, `ProcessOneQueuedJob`, `ProcessWorkerPool`, `RunWorkerLoopAsync` — 와 `internal`인 `CreateWithDependencies`, `BeforeJobFinalization`은 이름·시그니처·동작을 유지한다. 파일 상단의 public record `5`종(`DesktopNodeApiRequest`, `DesktopNodeApiResponse`, `DesktopNodeApiHardeningOptions`, `DesktopNodeApiError`, `DesktopNodeApiWorkerTickResult`)도 이 파일에 남긴다.
- **동작을 바꾸지 않는다.** 순수 이동만 한다. 응답 본문, 상태 코드, 오류 코드, 평가 순서, 라우팅 우선순위 중 어느 것도 바꾸지 않는다. 개선 아이디어는 이동과 같은 커밋에 섞지 않는다.
- **소유자는 callback-free여야 한다.** 새로 만드는 소유자 타입은 `sealed`이고, `Func<>`/`Action<>`/`System.Delegate` 타입의 필드나 메서드 파라미터를 갖지 않으며, `DesktopNodeApiRequestProcessor` 타입의 필드나 파라미터를 갖지 않는다. 이것이 wave 1B~1D가 세운 규칙이고 `ApiArchitectureOwnershipTests`가 이미 그 세 소유자에 대해 강제하고 있다.
- **`HandleCore`가 유일한 dispatcher다.** 소유자 진입점 호출은 `HandleCore`에서만 일어난다. 기존 ownership 테스트가 `CallerName == "HandleCore"`를 확인하고 있고 새 guard도 같은 방식으로 확인한다.
- **`HandleCore`의 기존 분기 순서를 유지한다.** 특히 auth가 먼저다. `ApiArchitectureOwnershipTests.RequestProcessorDelegatesAuthSessionBehaviorToCallbackFreeOwner`가 `handleCoreOwnerCalls.Take(2) == ["TryHandle", "Authorize"]`를 단언한다.
- 각 task는 `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`의 `DesktopNodeApiRequestProcessor.cs` ceiling을 이동 후 실측값으로 **낮춘다**. gate는 ceiling이 실측보다 `50`줄 넘게 위에 남으면 실패하므로, 낮추지 않으면 task가 끝나지 않는다.
- 각 task 종료 시 `dotnet test src/DesktopNode.sln`이 통과해야 한다. 기준선은 `842/842`(Api.Tests `236`)이며 task마다 ownership guard가 최소 `1`건씩 늘어난다.
- **private reflection 금지.** `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json`이 테스트 코드의 `private_reflection.current_occurrence_count`를 `0`으로 고정한다. ownership guard는 `BindingFlags`가 아니라 `PEReader`/`MetadataReader`로 쓴다. `ApiArchitectureOwnershipTests`가 그 패턴을 이미 세워 뒀으므로 helper를 재사용한다.
- 모든 신규/수정 문서는 한국어 본문으로 쓴다. 코드 식별자, 명령어, 파일 경로는 원문을 유지한다(AGENTS.md 작업 원칙).
- `packaging/windows-desktop-node/tests`, `installer/tests`, `web/tests` Pester는 module-size-ratchet fixture를 제외하면 변경되지 않아야 한다. 변경되면 이동이 순수하지 않았다는 신호다.
- 설치본을 만들지 않고 operational anchor를 승격하지 않는다. `public_trusted_signing`과 `external_stable_publication`은 이 작업 범위 밖이며 주장하지 않는다.

## 안전망

이 리팩토링은 아래 자산이 있어서 가능하다. 착수 전 존재를 확인한다.

| 자산 | 값 |
| --- | --- |
| Api characterization 테스트 | `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs` — `4,128`줄 |
| ownership guard 전례 | `src/DesktopNode.Api.Tests/ApiArchitectureOwnershipTests.cs` — `934`줄, IL 호출 그래프까지 읽음 |
| job store characterization | `ApiJobStoreGoldenCharacterizationTests.cs`, `ApiJobStoreFailureCharacterizationTests.cs` |
| Api.Tests 전체 | `236` tests |
| 솔루션 전체 | `842` tests |
| internals 접근 | `src/DesktopNode.Api/DesktopNode.Api.csproj`에 `<InternalsVisibleTo Include="DesktopNode.Api.Tests" />` 존재 |
| 라인 수 gate | `packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1` |

## File Structure

**수정(대상):** `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
남는 책임: public record `5`종, 생성자와 factory `2`종, `Handle`, `HandleCoreWithRouteTimeout`, `HandleCore` 라우팅, 소유자 필드와 dispatch. 목표는 `450`줄 이하.

**생성:** 모두 `src/DesktopNode.Api/` 아래.

| 파일 | 종류 | 소유하게 될 것 |
| --- | --- | --- |
| `DesktopNodeApiResponseFactory.cs` | `internal static` | 응답 봉투 생성 |
| `DesktopNodeApiJsonReader.cs` | `internal static` | `JsonElement` 읽기 |
| `DesktopNodeApiRequestParsing.cs` | `internal static` | 본문·경로·쿼리 파싱 |
| `DesktopNodeApiErrorMapping.cs` | `internal static` | runtime/provider 오류 → API 오류 |
| `DesktopNodeApiHyperVOperationInvoker.cs` | `internal sealed` | native adapter 호출과 지원 operation 목록 |
| `DesktopNodeApiConsoleRouteHandler.cs` | `internal sealed` | console capabilities/session 경로 |
| `DesktopNodeApiGuestExecutionRouteHandler.cs` | `internal sealed` | guest exec/channel preview와 차단 경로 |
| `DesktopNodeApiJobRouteHandler.cs` | `internal sealed` | job list/get/cancel/retry, vm delete-status |
| `DesktopNodeApiJobReconciliationHandler.cs` | `internal sealed` | reconcile 경로와 baseline 캡처 |
| `DesktopNodeApiVmMutationRouteHandler.cs` | `internal sealed` | queued mutation과 QoS |
| `DesktopNodeApiJobWorker.cs` | `internal sealed` | worker tick과 loop |
| `DesktopNodeApiRequestThrottle.cs` | `internal sealed` | rate limit 창과 timeout 응답 |

**생성(테스트):** `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs` — task마다 `[Fact]` `1`개 이상 추가한다. 기존 `ApiArchitectureOwnershipTests`는 wave 1 소유자를 잠그고 있으므로 건드리지 않고, 새 파일이 wave 2 소유자를 잠근다.

**삭제(최종):** `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`의 `DesktopNodeApiJobRuntimeHandler`와 `DesktopNodeApiConsoleHandler`. 둘 다 `Func<>` 파라미터로 처리기를 넘겨받는 **callback adapter**이고, wave 1이 세운 callback-free 규칙보다 앞선 세대다. Task 5와 Task 7이 각각을 callback-free 소유자로 대체하면서 제거한다.

**수정:** `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json` — task마다 ceiling 하향.

## 이동 대상 지도

착수 시점 `67ffc40c` 기준 줄 번호다. **각 task는 이동 직전에 줄 번호를 다시 확인한다.** 앞선 task가 이동을 마치면 뒤 task의 줄 번호가 밀린다. 메서드 이름이 진짜 기준이고 줄 번호는 참고값이다.

| Task | 도착지 | 이동할 멤버 | 착수 시점 줄 |
| ---: | --- | --- | --- |
| 1 | `DesktopNodeApiResponseFactory` | `GetStatusForOperationResult`, `Failure`, `Body`, `JobData`, `EmptyObject`, `JsonFromObject`, `Json`, `AttachRequestId`, `SerializeResponsePayload`, `OperationResponse`, `JobCreated` | 2357-2360, 2663-2726, 3165-3217 |
| 2 | `DesktopNodeApiJsonReader` | `FindVm`, `ReadNestedElement`, `EnumerateVmList`, `EnumerateCheckpointList`, `MatchesVmId`, `GetStringProperty`, `ReadString`, `ReadInt`, `ReadBool`, `ReadStringList`, `ReadStringDictionary`, `ReadElement`, `ReadOperationResult`, `ReadApiError` | 2809-2825, 2967-3163 |
| 3 | `DesktopNodeApiRequestParsing` | `TryParseBody`+`ParsedJson`, `DecodeRouteId`+`RouteId`, `TryMatch`, `ParseJobListPage`+`JobListPage`, `TryParseNonNegativeInt`, `QueryValue`, `NormalizePath`, `NormalizeRequestId`, `DefaultJobListLimit`, `MaxJobListLimit` | 56-57, 2770-2807, 3219-3330, 3346-3363 |
| 4 | `DesktopNodeApiErrorMapping`, `DesktopNodeApiHyperVOperationInvoker` | `ToRuntimeError`, `ToApiError`, `JobStoreCommitError`, `ToJobExecutionOutcome` / `InvokeHyperVOperation`, `IsNativeOperationCandidate` | 2044-2092, 2193-2248 |
| 5 | `DesktopNodeApiConsoleRouteHandler` | `CreateConsoleRuntimePolicy`, `HandleConsoleCapabilities`, `HandleVmConsoleSession`, `BuildConsoleCapabilities`, `BuildVmConsoleSession`, `FormatNoVncWebSocketPath` | 1841-1928 |
| 6 | `DesktopNodeApiGuestExecutionRouteHandler` | `HandleGuestExecPreviewRoute`, `HandleGuestChannelPreviewRoute`, `HandleGuestExecutionBoundaryRoute`, `GuestExecutionOperationFor` | 1666-1839 |
| 7 | `DesktopNodeApiJobRouteHandler` | `BuildJobListResponse`, `HandleJobGet`, `HandleJobCancel`, `HandleJobRetry`, `HandleVmDeleteStatus` | 1213-1298, 2728-2768 |
| 8 | `DesktopNodeApiJobReconciliationHandler` | `HandleJobReconcile`, `ReconcileVmDeleteJob`, `ReconcileCheckpointCreateJob`, `RenderReconciliationResult`, `ReconciliationRequiredError`, 스키마 상수 `3`종, `BuildVmRenameParameters`, `BuildVmDeleteParameters`, `BuildCheckpointCreateParameters`, `CaptureVmRenameBaseline`, `CaptureVmDeleteBaseline`, `CaptureCheckpointCreateBaseline`, `BuildVmRenameFingerprint`, `BuildVmDeleteFingerprint`, `IsManagedVm`, `RenameFingerprintMatches`, `TryReadCapturedRenameBaseline`, `TryReadCapturedDeleteBaseline`, `TryReadCapturedCheckpointCreateBaseline`, `VmRenameBaseline`, `VmDeleteBaseline`, `VmCheckpointCreateBaseline` | 1300-1664, 2362-2404, 2406-2661, 2827-2965, 3332-3344 |
| 9 | `DesktopNodeApiVmMutationRouteHandler` | `HandleQueuedMutationRoute`, `QueueVmLimit`, `HandleQosPreviewRoute`, `QueueVmQosMutation`, `ValidateQosRange`, `BuildQosParameters`, `QueueVmResourceMutation`, `QueueVmGuestExec`, `QueueVmGuestChannelVerify`, `QueueVmGuestChannelEnsure`, `MaxQosPolicyValue` | 58, 494-1197 |
| 10 | `DesktopNodeApiJobWorker` | `ProcessOneQueuedJobAsync`, `CreateJob` | 2099-2191, 2333-2350 |
| 11 | `DesktopNodeApiRequestThrottle` | `EnforceRequestRateLimit`, `RateLimitExceededResponse`, `RouteTimeoutResponse`, `requestWindows` | 1930-2042 |
| 12 | (중복 제거) | wave 1 소유자 `3`종의 helper 사본 | 별도 파일 |

`HandleControlledRouteTimeoutProbe`(1199-1211)는 `hardeningOptions`에만 의존하는 진단 seam이므로 **남긴다.**

---

### Task 1: 응답 생성 helper 이동

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiResponseFactory.cs`
- Create: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: 없음(첫 task)
- Produces: `internal static class DesktopNodeApiResponseFactory`가 아래를 노출한다. 이후 모든 task가 이 이름을 쓴다.
  - `int GetStatusForOperationResult(DesktopNodeHyperVOperationResult result)`
  - `DesktopNodeApiResponse Failure(int statusCode, string operation, string code, string message, string detail, bool retryable, string? recommendedAction = null)`
  - `object Body(bool ok, string operation, object? data, DesktopNodeApiError? error)`
  - `JsonElement JobData(DesktopNodeJobSnapshot job)`
  - `JsonElement EmptyObject()`
  - `JsonElement JsonFromObject(object value)`
  - `DesktopNodeApiResponse Json(int statusCode, object payload)`
  - `DesktopNodeApiResponse AttachRequestId(DesktopNodeApiResponse response, string requestId)`
  - `string SerializeResponsePayload(object payload)`
  - `DesktopNodeApiResponse OperationResponse(DesktopNodeHyperVOperationResult result)`
  - `DesktopNodeApiResponse JobCreated(DesktopNodeJobSnapshot job)`
- 이후 task는 `ApiRequestProcessorDecompositionOwnershipTests`의 `GetDeclaredMethodNames` / `AssertProcessorDoesNotDeclare` / `AssertTypeDeclares` helper를 재사용한다.

- [ ] **Step 1: ownership guard 테스트 작성 (실패해야 함)**

`src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs` 생성:

```csharp
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

// wave 1 은 diagnostics/auth/ops 소유자를 떼어내고 ApiArchitectureOwnershipTests 로 잠갔다.
// 이 파일은 그 다음 물결이 떼어낸 소유자를 같은 방식으로 잠근다. 떠난 것과 도착한 것을 함께
// 확인해야 "옮긴 척하고 원본을 남겨둔" 상태가 통과하지 않는다.
//
// BindingFlags 대신 metadata 를 읽는 이유: csharp-architecture-test-migration.json 이 테스트 코드의
// private_reflection.current_occurrence_count 를 0 으로 고정하고 있다.
public sealed class ApiRequestProcessorDecompositionOwnershipTests
{
    private const string ApiNamespace = "DesktopNode.Api";

    internal static string[] GetDeclaredMethodNames(string typeName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeApiRequestProcessor).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeHandle = metadata.TypeDefinitions.Single(handle =>
        {
            var definition = metadata.GetTypeDefinition(handle);
            return metadata.GetString(definition.Namespace) == ApiNamespace &&
                metadata.GetString(definition.Name) == typeName;
        });

        return metadata.GetTypeDefinition(typeHandle)
            .GetMethods()
            .Select(handle => metadata.GetString(metadata.GetMethodDefinition(handle).Name))
            .ToArray();
    }

    internal static void AssertProcessorDoesNotDeclare(params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(nameof(DesktopNodeApiRequestProcessor));
        foreach (var methodName in methodNames)
        {
            Assert.DoesNotContain(methodName, declared);
        }
    }

    internal static void AssertTypeDeclares(string typeName, params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(typeName);
        foreach (var methodName in methodNames)
        {
            Assert.Contains(methodName, declared);
        }
    }

    [Fact]
    public void ResponseEnvelopeConstructionLeavesTheProcessor()
    {
        var moved = new[]
        {
            "GetStatusForOperationResult",
            "Failure",
            "Body",
            "JobData",
            "EmptyObject",
            "JsonFromObject",
            "Json",
            "AttachRequestId",
            "SerializeResponsePayload",
            "OperationResponse",
            "JobCreated"
        };

        AssertProcessorDoesNotDeclare(moved);
        AssertTypeDeclares("DesktopNodeApiResponseFactory", moved);
    }
}
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~ApiRequestProcessorDecompositionOwnershipTests`
Expected: FAIL — `DesktopNodeApiResponseFactory` 타입이 없어 `Single()`이 던진다.

- [ ] **Step 3: `DesktopNodeApiResponseFactory` 생성**

`src/DesktopNode.Api/DesktopNodeApiResponseFactory.cs`를 만들고, 위 `11`개 멤버를 `DesktopNodeApiRequestProcessor`에서 **잘라내어** 붙인다. 모두 이미 `static`이거나 인스턴스 상태를 쓰지 않으므로 시그니처를 바꿀 필요가 없다. 접근 한정자는 `internal static`으로 올린다. 본문은 한 글자도 바꾸지 않는다.

필요한 `using`: `System.Text.Json`, `DesktopNode.Contracts`, `DesktopNode.HyperV`, `DesktopNode.Runtime`.

- [ ] **Step 4: 호출부 갱신**

`DesktopNodeApiRequestProcessor.cs`에서 위 `11`개 이름의 호출을 `DesktopNodeApiResponseFactory.<이름>`으로 바꾼다. 파일 상단에 `using static DesktopNode.Api.DesktopNodeApiResponseFactory;`를 **쓰지 않는다** — ownership guard가 IL 호출의 `DeclaringType`을 읽으므로 정적 import 여부와 무관하게 통과하지만, 이후 task 리뷰어가 호출 지점을 눈으로 찾을 수 있어야 한다.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `843` 이상(신규 guard `1`건 추가), 실패 `0`.

- [ ] **Step 6: 라인 수 ratchet 하향**

Run: `(Get-Content src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs | Measure-Object -Line).Lines`
측정값을 `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`의 `DesktopNodeApiRequestProcessor.cs` `max_lines`에 넣는다. 같은 항목에 `"decomposition_plan": "docs/superpowers/plans/2026-08-06-purecvisor-desktop-node-api-request-processor-decomposition.md"`를 추가한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1"`
Expected: PASS

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiResponseFactory.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): move response envelope construction out of the request processor"
```

---

### Task 2: JSON 읽기 helper 이동

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiJsonReader.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `DesktopNodeApiResponseFactory`(이 task의 대상은 응답을 만들지 않으므로 실제 참조는 없다)
- Produces: `internal static class DesktopNodeApiJsonReader` — `FindVm`, `ReadNestedElement`, `EnumerateVmList`, `EnumerateCheckpointList`, `MatchesVmId`, `GetStringProperty`, `ReadString`, `ReadInt`, `ReadBool`, `ReadStringList`, `ReadStringDictionary`, `ReadElement`, `ReadOperationResult`, `ReadApiError`. 전부 시그니처 불변.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

`ApiRequestProcessorDecompositionOwnershipTests.cs`에 추가:

```csharp
    [Fact]
    public void JsonElementReadingLeavesTheProcessor()
    {
        var moved = new[]
        {
            "FindVm",
            "ReadNestedElement",
            "EnumerateVmList",
            "EnumerateCheckpointList",
            "MatchesVmId",
            "GetStringProperty",
            "ReadString",
            "ReadInt",
            "ReadBool",
            "ReadStringList",
            "ReadStringDictionary",
            "ReadElement",
            "ReadOperationResult",
            "ReadApiError"
        };

        AssertProcessorDoesNotDeclare(moved);
        AssertTypeDeclares("DesktopNodeApiJsonReader", moved);
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~JsonElementReadingLeavesTheProcessor`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiJsonReader` 생성**

`14`개 메서드를 잘라내어 붙인다. `internal static`으로 올린다. 본문 변경 금지.

필요한 `using`: `System.Text.Json`, `DesktopNode.Api`(같은 namespace이므로 불필요), `DesktopNode.HyperV`(`ReadOperationResult`가 `DesktopNodeHyperVOperationResult`를 돌려준다).

- [ ] **Step 4: 호출부 갱신**

`DesktopNodeApiRequestProcessor.cs`의 호출을 `DesktopNodeApiJsonReader.<이름>`으로 바꾼다.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`

- [ ] **Step 6: 라인 수 ratchet 하향** (Task 1 Step 6과 동일 절차)

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiJsonReader.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): move JsonElement reading out of the request processor"
```

---

### Task 3: 요청 파싱 helper 이동

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiRequestParsing.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory.Failure`(파싱 실패 응답을 만든다)
- Produces: `internal static class DesktopNodeApiRequestParsing` — `TryParseBody`, `DecodeRouteId`, `TryMatch`, `ParseJobListPage`, `TryParseNonNegativeInt`, `QueryValue`, `NormalizePath`, `NormalizeRequestId`와 상수 `DefaultJobListLimit = 50`, `MaxJobListLimit = 200`. 중첩 record `ParsedJson`, `RouteId`, `JobListPage`는 이 타입의 중첩 타입으로 함께 옮긴다. Task 7~9가 `ParsedJson`/`RouteId`를, Task 7이 `JobListPage`를 쓴다.

**주의:** `ParsedJson`, `RouteId`, `JobListPage`는 지금 `DesktopNodeApiRequestProcessor`의 `private sealed record`다. `DesktopNodeApiAuthSessionHandler`도 **자기 자신의 `ParsedJson`을 따로 갖고 있다**(`DesktopNodeApiAuthSessionHandler.cs:169`). 두 타입은 이름만 같고 별개다. 이 task는 processor 쪽만 옮기고 auth 쪽은 건드리지 않는다. 통합은 Task 12가 판단한다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void RequestParsingLeavesTheProcessor()
    {
        var moved = new[]
        {
            "TryParseBody",
            "DecodeRouteId",
            "TryMatch",
            "ParseJobListPage",
            "TryParseNonNegativeInt",
            "QueryValue",
            "NormalizePath",
            "NormalizeRequestId"
        };

        AssertProcessorDoesNotDeclare(moved);
        AssertTypeDeclares("DesktopNodeApiRequestParsing", moved);
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~RequestParsingLeavesTheProcessor`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiRequestParsing` 생성**

`8`개 메서드, `2`개 상수, `3`개 중첩 record를 잘라내어 붙인다. record는 `internal sealed record`로 올린다. `Failure` 호출은 Task 1이 만든 `DesktopNodeApiResponseFactory.Failure`로 쓴다.

필요한 `using`: `System.Text.Json`, `System.Text.RegularExpressions`.

- [ ] **Step 4: 호출부 갱신**

`DesktopNodeApiRequestProcessor.cs`의 호출과 `ParsedJson`/`RouteId`/`JobListPage` 타입 참조를 `DesktopNodeApiRequestParsing.<이름>`으로 바꾼다.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`

- [ ] **Step 6: 라인 수 ratchet 하향**

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiRequestParsing.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): move request body and route parsing out of the request processor"
```

---

### Task 4: 오류 매핑과 native 호출 분리

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiErrorMapping.cs`
- Create: `src/DesktopNode.Api/DesktopNodeApiHyperVOperationInvoker.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: 없음
- Produces:
  - `internal static class DesktopNodeApiErrorMapping` — `ToRuntimeError`, `ToApiError`, `JobStoreCommitError`, `ToJobExecutionOutcome`.

    **정정.** 계획 작성 시 `JobStoreCommitError`가 인스턴스 상태를 쓰지 않는다고 적었는데 **틀렸다.** 본문이 `jobRuntime.LoadBlock`을 두 번 읽는다. 따라서 순수 `static` 승격이 불가능하다. 세 번째 파라미터 `DesktopNodeJobRuntimeError? loadBlock`을 추가하고 호출부가 `jobRuntime.LoadBlock`을 넘긴다. 호출부는 `3`곳(`Handle`의 commit 예외 처리, `ProcessOneQueuedJobAsync`의 start 실패 분기와 completion 예외 분기)이며 전부 `jobRuntime`에 접근할 수 있다. 시그니처가 바뀌므로 이 항목은 **순수 이동이 아니다** — 종료 조건의 비순수 이동 목록에 포함한다.
  - `internal sealed class DesktopNodeApiHyperVOperationInvoker` — 생성자 `(IDesktopNodeHyperVNativeAdapter nativeAdapter)`, 메서드 `DesktopNodeHyperVOperationResult Invoke(string operation, JsonElement parameters, CancellationToken cancellationToken = default)`, `static bool IsNativeOperationCandidate(string operation)`. Task 8, 9, 10이 이 타입을 생성자 의존성으로 받는다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void ErrorMappingAndNativeInvocationLeaveTheProcessor()
    {
        AssertProcessorDoesNotDeclare(
            "ToRuntimeError",
            "ToApiError",
            "JobStoreCommitError",
            "ToJobExecutionOutcome",
            "InvokeHyperVOperation",
            "IsNativeOperationCandidate");
        AssertTypeDeclares(
            "DesktopNodeApiErrorMapping",
            "ToRuntimeError",
            "ToApiError",
            "JobStoreCommitError",
            "ToJobExecutionOutcome");
        AssertTypeDeclares(
            "DesktopNodeApiHyperVOperationInvoker",
            "Invoke",
            "IsNativeOperationCandidate");
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~ErrorMappingAndNativeInvocationLeaveTheProcessor`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiErrorMapping` 생성**

`4`개 메서드를 잘라내어 붙인다.

- [ ] **Step 4: `DesktopNodeApiHyperVOperationInvoker` 생성**

```csharp
using System.Text.Json;
using DesktopNode.HyperV;

namespace DesktopNode.Api;

// InvokeHyperVOperation 은 processor, 예약 경로, 조정 경로, worker 네 곳에서 호출된다.
// 소유자를 분리하지 않으면 그 네 도메인이 전부 processor 로 되돌아와야 한다.
internal sealed class DesktopNodeApiHyperVOperationInvoker
{
    private readonly IDesktopNodeHyperVNativeAdapter nativeAdapter;

    public DesktopNodeApiHyperVOperationInvoker(IDesktopNodeHyperVNativeAdapter nativeAdapter)
    {
        this.nativeAdapter = nativeAdapter;
    }

    public DesktopNodeHyperVOperationResult Invoke(
        string operation,
        JsonElement parameters,
        CancellationToken cancellationToken = default)
    {
        // 본문은 기존 InvokeHyperVOperation 을 그대로 옮긴다.
    }

    internal static bool IsNativeOperationCandidate(string operation)
    {
        // 본문은 기존 IsNativeOperationCandidate 를 그대로 옮긴다.
    }
}
```

- [ ] **Step 5: processor 배선**

`DesktopNodeApiRequestProcessor`에 `private readonly DesktopNodeApiHyperVOperationInvoker operationInvoker;` 필드를 추가하고 생성자에서 `new DesktopNodeApiHyperVOperationInvoker(nativeAdapter)`로 초기화한다. `nativeAdapter` 필드는 `DesktopNodeApiOpsSummaryQuery` 생성에도 쓰이므로 **남긴다.** 기존 `InvokeHyperVOperation(...)` 호출을 `operationInvoker.Invoke(...)`로 바꾼다.

- [ ] **Step 6: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`

- [ ] **Step 7: 라인 수 ratchet 하향**

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiErrorMapping.cs src/DesktopNode.Api/DesktopNodeApiHyperVOperationInvoker.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): give error mapping and native invocation their own owners"
```

---

### Task 5: console 경로 소유자 분리

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiConsoleRouteHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs` (`DesktopNodeApiConsoleHandler` 삭제)
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory.Json`/`Body`, `DesktopNodeApiRequestParsing.DecodeRouteId`
- Produces: `internal sealed class DesktopNodeApiConsoleRouteHandler` — 생성자 `(DesktopNodeConsoleOptions consoleOptions)`, `DesktopNodeApiResponse? TryHandle(string method, string normalizedPath)`, `RuntimePolicyConsolePolicy CreateRuntimePolicy()`.

**설명:** 현재 console 경로는 `DesktopNodeApiConsoleHandler.TryHandle(method, path, Func<...> capabilities, Func<string,...> vmConsoleSession)`으로 dispatch된다. 이 `Func` 두 개가 곧바로 processor의 private 메서드로 되돌아온다. wave 1이 diagnostics/auth/ops에서 없앤 바로 그 형태다. 이 task는 라우팅 조건(`DesktopNodeApiRuntimeRoutes.TryMatchOperation`의 `"GetConsoleCapabilities"`, `"GetVmConsoleSession"`)을 새 소유자 안으로 옮기고 callback adapter를 삭제한다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void ConsoleRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "CreateConsoleRuntimePolicy",
            "HandleConsoleCapabilities",
            "HandleVmConsoleSession",
            "BuildConsoleCapabilities",
            "BuildVmConsoleSession",
            "FormatNoVncWebSocketPath");
        AssertTypeDeclares("DesktopNodeApiConsoleRouteHandler", "TryHandle", "CreateRuntimePolicy");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiConsoleRouteHandler");
        AssertApiAssemblyDoesNotDefine("DesktopNodeApiConsoleHandler");
    }
```

같은 파일에 helper `2`개를 추가한다. `AssertTypeIsCallbackFreeOwner`는 `ApiArchitectureOwnershipTests`가 wave 1 소유자에 대해 이미 확인하는 것 — 필드/파라미터에 `Func`·`Action`·`Delegate`가 없고 `DesktopNodeApiRequestProcessor` 역참조가 없으며 `sealed`이고 `abstract`가 아님 — 을 이름으로 일반화한 것이다. `ApiArchitectureOwnershipTests`의 `MetadataTypeNameProvider`, `MetadataField`, `IsCallbackType`이 그 파일의 `private` 중첩 타입이므로, 새 파일에 필요한 최소한만 다시 세운다.

```csharp
    private static bool IsCallbackType(string typeName)
    {
        return typeName == "System.Delegate" ||
            typeName.StartsWith("System.Func`", StringComparison.Ordinal) ||
            typeName.StartsWith("System.Action`", StringComparison.Ordinal);
    }

    internal static void AssertTypeIsCallbackFreeOwner(string typeName)
    {
        // 필드 타입과 모든 메서드 파라미터 타입을 MetadataReader 로 읽어
        // IsCallbackType 과 "DesktopNode.Api.DesktopNodeApiRequestProcessor" 를 배제하고,
        // TypeAttributes.Sealed 이며 Abstract 가 아님을 확인한다.
    }

    internal static void AssertApiAssemblyDoesNotDefine(string typeName)
    {
        // DesktopNode.Api namespace 의 TypeDefinition 이름 목록에 typeName 이 없음을 확인한다.
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~ConsoleRoutesUseACallbackFreeOwner`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiConsoleRouteHandler` 생성**

`6`개 메서드를 옮기고, `DesktopNodeApiConsoleHandler.TryHandle`의 라우팅 분기를 새 `TryHandle` 본문으로 흡수한다. `consoleOptions`를 생성자로 받는다.

- [ ] **Step 4: processor 배선과 adapter 삭제**

- `DesktopNodeApiRequestProcessor`에 `private readonly DesktopNodeApiConsoleRouteHandler consoleRouteHandler;`를 추가하고 생성자에서 초기화한다.
- `HandleCore`의 `DesktopNodeApiConsoleHandler.TryHandle(method, path, HandleConsoleCapabilities, HandleVmConsoleSession)`을 `consoleRouteHandler.TryHandle(method, path)`로 바꾼다. **위치를 바꾸지 않는다.**
- `/api/v1/runtime/policy` 분기의 `CreateConsoleRuntimePolicy()`를 `consoleRouteHandler.CreateRuntimePolicy()`로 바꾼다.
- `DesktopNodeApiRuntimeCoreHandlers.cs`에서 `DesktopNodeApiConsoleHandler` 클래스를 삭제한다.
- `consoleOptions` 필드는 `DesktopNodeApiOpsSummaryQuery` 생성에도 쓰이므로 **남긴다.**

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`

- [ ] **Step 6: 라인 수 ratchet 하향**

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiConsoleRouteHandler.cs src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): replace the console callback adapter with a callback-free owner"
```

---

### Task 6: guest execution 경로 소유자 분리

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiGuestExecutionRouteHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory`, `DesktopNodeApiRequestParsing`, `DesktopNodeApiJsonReader`, `DesktopNodeApiAuthSessionHandler.ResolveActor`
- Produces: `internal sealed class DesktopNodeApiGuestExecutionRouteHandler` — 생성자 `(DesktopNodeApiAuthSessionHandler authSessionHandler)`, `DesktopNodeApiResponse? TryHandle(DesktopNodeApiRequest request, string method, string normalizedPath)`.

**설명:** 지금 `HandleCore`는 guest 관련 분기를 `3`개 갖는다 — exec preview 정규식, channel preview 정규식, `HandleGuestExecutionBoundaryRoute`. 세 분기의 상대 순서는 유지하되 하나의 `TryHandle` 안으로 흡수한다. `TryHandle`은 preview 두 개를 먼저 시도하고 그 다음 boundary를 시도한다 — 현재 `HandleCore`와 같은 순서다. ADR-0009가 고정한 보안 경계이므로 차단 응답의 코드·메시지·권고를 한 글자도 바꾸지 않는다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void GuestExecutionRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "HandleGuestExecPreviewRoute",
            "HandleGuestChannelPreviewRoute",
            "HandleGuestExecutionBoundaryRoute",
            "GuestExecutionOperationFor");
        AssertTypeDeclares("DesktopNodeApiGuestExecutionRouteHandler", "TryHandle");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiGuestExecutionRouteHandler");
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~GuestExecutionRoutesUseACallbackFreeOwner`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiGuestExecutionRouteHandler` 생성**

`4`개 메서드를 옮기고 `TryHandle`을 추가한다. `TryHandle`은 `HandleCore`가 갖고 있던 정규식 `2`개(`^/api/v1/vms/([^/]*)/guest/exec/preview$`, `^/api/v1/vms/([^/]*)/guest/channel/preview$`)와 `POST` 조건을 그대로 가져온다.

- [ ] **Step 4: processor 배선**

`private readonly DesktopNodeApiGuestExecutionRouteHandler guestExecutionRouteHandler;`를 추가하고 생성자에서 `authSessionHandler` 생성 이후에 초기화한다. `HandleCore`의 guest 분기 `3`개를 `guestExecutionRouteHandler.TryHandle(request, method, path)` 하나로 대체한다. **위치는 기존 첫 guest 분기 자리다** — QoS preview 분기 뒤, `POST` 404 fallthrough 앞.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`. 특히 `ApiRuntimePolicyRequestProcessorTests`의 guest execution 차단 케이스가 통과해야 한다.

- [ ] **Step 6: 라인 수 ratchet 하향**

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiGuestExecutionRouteHandler.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): give guest execution preview and boundary routes their own owner"
```

---

### Task 7: job 조회·제어 경로 소유자 분리

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiJobRouteHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory`, `DesktopNodeApiRequestParsing`, `DesktopNodeApiJsonReader`, `DesktopNodeApiErrorMapping`
- Produces: `internal sealed class DesktopNodeApiJobRouteHandler` — 생성자 `(DesktopNodeJobRuntime jobRuntime)`, 메서드:
  - `DesktopNodeApiResponse? TryHandle(DesktopNodeApiRequest request, string method, string normalizedPath)` — list/get/cancel/retry 담당. **reconcile은 담당하지 않는다**(Task 8 소유).
  - `DesktopNodeApiResponse HandleVmDeleteStatus(string vmName)` — `HandleCore`의 `vm.delete-status` 분기가 직접 호출한다.

**설명:** `DesktopNodeApiJobRuntimeHandler`(callback adapter)는 `5`개 `Func`을 받는다. 그중 `reconcileJob`은 Task 8이 가져가므로, 이 task는 adapter를 아직 **삭제하지 않고** reconcile 하나만 남긴 형태로 축소한다. Task 8이 마지막 callback을 없애면서 adapter를 삭제한다. 이 순서를 지키는 이유: reconcile 경로는 `1,000`줄에 가까운 baseline 로직을 끌고 오므로 job 조회 이동과 한 커밋에 섞으면 리뷰가 불가능해진다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void JobQueryAndControlRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "BuildJobListResponse",
            "HandleJobGet",
            "HandleJobCancel",
            "HandleJobRetry",
            "HandleVmDeleteStatus");
        AssertTypeDeclares("DesktopNodeApiJobRouteHandler", "TryHandle", "HandleVmDeleteStatus");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiJobRouteHandler");
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~JobQueryAndControlRoutesUseACallbackFreeOwner`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiJobRouteHandler` 생성**

`5`개 메서드를 옮긴다. `HandleJobRetry(jobId, requestId)`는 `TryHandle`이 `request.RequestId!`를 전달한다 — 지금 callback이 클로저로 캡처하던 값과 같다.

`TryHandle`은 `DesktopNodeApiRuntimeRoutes.TryMatchOperation`으로 `"ListJobs"`, `"GetJob"`, `"CancelJob"`, `"RetryJob"`을 이 순서로 확인한다. `ListJobs`는 원본 경로(`request.Path`)를 쓴다 — 쿼리 문자열이 필요하기 때문이며 지금 adapter가 `originalPath`를 따로 받는 이유다.

- [ ] **Step 4: processor 배선과 adapter 축소**

- `private readonly DesktopNodeApiJobRouteHandler jobRouteHandler;`를 추가하고 생성자에서 초기화한다.
- `HandleCore`에서 `DesktopNodeApiJobRuntimeHandler.TryHandle(...)` 호출 앞에 `jobRouteHandler.TryHandle(request, method, path)`를 두고, adapter 호출은 `reconcileJob` 하나만 남긴다.
- `DesktopNodeApiRuntimeCoreHandlers.cs`의 `DesktopNodeApiJobRuntimeHandler.TryHandle` 시그니처에서 `listJobs`, `getJob`, `cancelJob`, `retryJob` 파라미터와 해당 분기를 제거한다.
- `vm.delete-status` 분기는 `jobRouteHandler.HandleVmDeleteStatus(routeId.Value!)`로 바꾼다.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`

- [ ] **Step 6: 라인 수 ratchet 하향**

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiJobRouteHandler.cs src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): give job query and control routes a callback-free owner"
```

---

### Task 8: reconciliation 소유자 분리

가장 큰 task다. `HandleJobReconcile` 계열과 baseline 캡처가 같은 스키마 상수·fingerprint를 공유하므로 쪼개면 오히려 왕복이 생긴다.

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiJobReconciliationHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs` (`DesktopNodeApiJobRuntimeHandler` 삭제)
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory`, `DesktopNodeApiJsonReader`, `DesktopNodeApiErrorMapping`, `DesktopNodeApiHyperVOperationInvoker`
- Produces: `internal sealed class DesktopNodeApiJobReconciliationHandler` — 생성자 `(DesktopNodeJobRuntime jobRuntime, DesktopNodeApiHyperVOperationInvoker operationInvoker, DesktopNodeApiHardeningOptions hardeningOptions)`, 메서드:
  - `DesktopNodeApiResponse? TryHandle(string method, string normalizedPath, CancellationToken cancellationToken)`
  - `JsonElement BuildVmRenameParameters(string oldName, string newName, CancellationToken cancellationToken)`
  - `JsonElement BuildVmDeleteParameters(string vmName, CancellationToken cancellationToken)`
  - `JsonElement BuildCheckpointCreateParameters(string vmName, string checkpointName, CancellationToken cancellationToken)`

  Task 9의 `DesktopNodeApiVmMutationRouteHandler`가 뒤의 `Build*Parameters` `3`개를 생성자 의존성으로 받아 호출한다.

**주의:** `Build*Parameters`는 queueing 시점에 provider readback으로 baseline을 캡처한다. 즉 큐 등록 경로가 조정 소유자에 의존한다. 이것은 왕복이 아니다 — 방향이 한쪽(mutation → reconciliation)뿐이고 reconciliation은 mutation을 알지 못한다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void ReconciliationUsesACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "HandleJobReconcile",
            "ReconcileVmDeleteJob",
            "ReconcileCheckpointCreateJob",
            "RenderReconciliationResult",
            "ReconciliationRequiredError",
            "BuildVmRenameParameters",
            "BuildVmDeleteParameters",
            "BuildCheckpointCreateParameters",
            "CaptureVmRenameBaseline",
            "CaptureVmDeleteBaseline",
            "CaptureCheckpointCreateBaseline",
            "BuildVmRenameFingerprint",
            "BuildVmDeleteFingerprint",
            "IsManagedVm",
            "RenameFingerprintMatches",
            "TryReadCapturedRenameBaseline",
            "TryReadCapturedDeleteBaseline",
            "TryReadCapturedCheckpointCreateBaseline");
        AssertTypeDeclares(
            "DesktopNodeApiJobReconciliationHandler",
            "TryHandle",
            "BuildVmRenameParameters",
            "BuildVmDeleteParameters",
            "BuildCheckpointCreateParameters");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiJobReconciliationHandler");
        AssertApiAssemblyDoesNotDefine("DesktopNodeApiJobRuntimeHandler");
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~ReconciliationUsesACallbackFreeOwner`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiJobReconciliationHandler` 생성**

`18`개 메서드, 스키마 상수 `3`개, baseline record `3`개를 옮긴다. `TryHandle`은 `DesktopNodeApiRuntimeRoutes.TryMatchOperation(method, normalizedPath, "ReconcileJob", out var match)`로 매칭하고 `HandleJobReconcile(match.Parameters["jobId"], cancellationToken)`을 호출한다.

- [ ] **Step 4: processor 배선과 adapter 삭제**

- `private readonly DesktopNodeApiJobReconciliationHandler reconciliationHandler;`를 추가하고 생성자에서 초기화한다(`operationInvoker`와 `hardeningOptions` 초기화 이후).
- `HandleCore`의 `DesktopNodeApiJobRuntimeHandler.TryHandle(...)` 호출을 `reconciliationHandler.TryHandle(method, path, cancellationToken)`으로 대체한다. Task 7이 남긴 자리 그대로다.
- `DesktopNodeApiRuntimeCoreHandlers.cs`에서 `DesktopNodeApiJobRuntimeHandler`를 삭제한다. 파일에 남는 타입이 없으면 파일도 삭제한다.
- `HandleQueuedMutationRoute`의 `Build*Parameters` 호출을 `reconciliationHandler.Build*Parameters`로 바꾼다.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`. 특히 wave 2B/2C가 남긴 조정 characterization이 통과해야 한다.

- [ ] **Step 6: 라인 수 ratchet 하향**

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiJobReconciliationHandler.cs src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): give job reconciliation and its baselines a callback-free owner"
```

---

### Task 9: queued mutation과 QoS 소유자 분리

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory`, `DesktopNodeApiRequestParsing`, `DesktopNodeApiJsonReader`, `DesktopNodeApiHyperVOperationInvoker`, `DesktopNodeApiJobReconciliationHandler`(`Build*Parameters`)
- Produces: `internal sealed class DesktopNodeApiVmMutationRouteHandler` — 생성자 `(DesktopNodeJobRuntime jobRuntime, DesktopNodeApiHyperVOperationInvoker operationInvoker, DesktopNodeApiJobReconciliationHandler reconciliationHandler)`, 메서드:
  - `DesktopNodeApiResponse HandleQueuedMutationRoute(DesktopNodeApiRequest request, DesktopNodeApiRouteMatch routeMatch, CancellationToken cancellationToken)`
  - `DesktopNodeApiResponse? TryHandleQosPreview(DesktopNodeApiRequest request, string method, string normalizedPath, CancellationToken cancellationToken)`

**주의:** `CreateJob`은 Task 10이 `DesktopNodeApiJobWorker`로 가져간다. 하지만 이 소유자도 `CreateJob`이 필요하다. 둘 다 `jobRuntime.Create(...)`의 얇은 wrapper이므로, 이 task는 소유자 안에 자기 `CreateJob` private helper를 두고 Task 10은 worker 쪽에서 쓰지 않는다 — worker는 `jobRuntime.TryStartNext`만 쓴다. **Task 10 착수 시 `CreateJob`의 남은 호출자가 이 소유자뿐인지 확인한다.**

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void QueuedMutationRoutesUseACallbackFreeOwner()
    {
        AssertProcessorDoesNotDeclare(
            "HandleQueuedMutationRoute",
            "QueueVmLimit",
            "HandleQosPreviewRoute",
            "QueueVmQosMutation",
            "ValidateQosRange",
            "BuildQosParameters",
            "QueueVmResourceMutation",
            "QueueVmGuestExec",
            "QueueVmGuestChannelVerify",
            "QueueVmGuestChannelEnsure");
        AssertTypeDeclares(
            "DesktopNodeApiVmMutationRouteHandler",
            "HandleQueuedMutationRoute",
            "TryHandleQosPreview");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiVmMutationRouteHandler");
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~QueuedMutationRoutesUseACallbackFreeOwner`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiVmMutationRouteHandler` 생성**

`10`개 메서드와 상수 `MaxQosPolicyValue`를 옮긴다. `TryHandleQosPreview`는 `HandleCore`의 QoS preview 정규식 분기(`^/api/v1/vms/([^/]*)/qos/(storage|network)/preview$`, `POST`)를 흡수한다.

- [ ] **Step 4: processor 배선**

`private readonly DesktopNodeApiVmMutationRouteHandler vmMutationRouteHandler;`를 추가하고 생성자에서 초기화한다(`reconciliationHandler` 이후). `HandleCore`의 두 분기를 각각 소유자 호출로 바꾼다. **위치와 순서를 바꾸지 않는다.**

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`

- [ ] **Step 6: 라인 수 ratchet 하향**

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): give queued VM mutation and QoS routes a callback-free owner"
```

---

### Task 10: worker tick 소유자 분리

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiJobWorker.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory.JobData`, `DesktopNodeApiErrorMapping`, `DesktopNodeApiHyperVOperationInvoker`
- Produces: `internal sealed class DesktopNodeApiJobWorker` — 생성자 `(DesktopNodeJobRuntime jobRuntime, IDesktopNodeApiCancellationScopeFactory cancellationScopes, DesktopNodeApiHyperVOperationInvoker operationInvoker, object sync)`, 메서드 `Task<DesktopNodeApiWorkerTickResult> ProcessOneQueuedJobAsync(CancellationToken cancellationToken = default)`, 속성 `Action? BeforeJobFinalization { get; set; }`.

**주의 — 두 가지가 순수 이동이 아니다. 둘 다 리뷰 대상이다.**

1. **`sync` 잠금 객체를 공유한다.** `ProcessOneQueuedJobAsync`는 `lock (sync)`를 두 번 잡는데, 같은 `sync`를 `Handle`도 잡는다. 이 잠금은 요청 처리와 worker tick의 상호 배제이므로 **반드시 같은 객체여야 한다.** 소유자에 새 잠금을 만들면 상호 배제가 깨진다. 생성자로 `object sync`를 넘기는 것이 그 이유이며, 주석으로 남긴다.
2. **`BeforeJobFinalization`은 `Action?`이다.** callback-free 규칙과 정면으로 충돌한다. 이것은 `internal` 테스트 seam이고 도메인 협력자가 아니므로, `AssertTypeIsCallbackFreeOwner`를 이 타입에 적용하지 **않는다.** 대신 이 속성 하나만 예외임을 guard에 명시한다. 규칙을 조용히 우회하지 않고 예외를 기록하는 것이 요점이다.

`ProcessOneQueuedJob`, `ProcessWorkerPool`, `RunWorkerLoopAsync`는 **public 표면이므로 processor에 남기고**, 본문에서 `jobWorker.ProcessOneQueuedJobAsync(...)`로 위임한다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void WorkerTickExecutionLeavesTheProcessor()
    {
        AssertProcessorDoesNotDeclare("ProcessOneQueuedJobAsync", "CreateJob");
        AssertTypeDeclares("DesktopNodeApiJobWorker", "ProcessOneQueuedJobAsync");

        // public 표면은 남는다.
        var processorMethods = GetDeclaredMethodNames(nameof(DesktopNodeApiRequestProcessor));
        Assert.Contains("ProcessOneQueuedJob", processorMethods);
        Assert.Contains("ProcessWorkerPool", processorMethods);
        Assert.Contains("RunWorkerLoopAsync", processorMethods);
    }

    [Fact]
    public void WorkerOwnerCarriesExactlyOneDeclaredCallbackSeam()
    {
        // BeforeJobFinalization 은 provider-result/finalization 경계를 결정적으로 만드는
        // 테스트 seam 이다. 도메인 협력자가 아니므로 callback-free 규칙의 유일한 예외로
        // 명시해 둔다 - 예외를 조용히 두면 다음 사람이 두 번째 callback 을 추가한다.
        AssertDeclaredCallbackFieldsAre("DesktopNodeApiJobWorker", "BeforeJobFinalization");
    }
```

`AssertDeclaredCallbackFieldsAre`는 해당 타입의 필드 중 `IsCallbackType`인 것의 이름 집합이 인자와 정확히 일치함을 확인한다. 자동 구현 속성의 backing field 이름(`<BeforeJobFinalization>k__BackingField`)이 나오므로 `<` 와 `>k__BackingField` 를 벗겨 비교한다.

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~WorkerTickExecutionLeavesTheProcessor`
Expected: FAIL

- [ ] **Step 3: `CreateJob`의 남은 호출자 확인**

Run: `rg -n "CreateJob\(" src/DesktopNode.Api/`
Task 9 이후 호출자는 `DesktopNodeApiVmMutationRouteHandler`뿐이어야 한다. 그렇다면 processor의 `CreateJob`을 삭제한다(Task 9가 소유자 안에 자기 사본을 이미 두었다). 다른 호출자가 남아 있으면 이 task를 진행하지 말고 보고한다.

- [ ] **Step 4: `DesktopNodeApiJobWorker` 생성**

`ProcessOneQueuedJobAsync`를 옮기고 `BeforeJobFinalization` 속성을 옮긴다.

- [ ] **Step 5: processor 배선**

`private readonly DesktopNodeApiJobWorker jobWorker;`를 추가하고 생성자에서 `new DesktopNodeApiJobWorker(jobRuntime, cancellationScopes, operationInvoker, sync)`로 초기화한다. `internal Action? BeforeJobFinalization`은 public 표면이므로 processor에 남기되 `jobWorker.BeforeJobFinalization`으로 위임하는 속성으로 바꾼다:

```csharp
    // Deterministic test seam for the provider-result/serialized-finalization boundary.
    internal Action? BeforeJobFinalization
    {
        get => jobWorker.BeforeJobFinalization;
        set => jobWorker.BeforeJobFinalization = value;
    }
```

`ProcessOneQueuedJob`, `ProcessWorkerPool`, `RunWorkerLoopAsync`의 `ProcessOneQueuedJobAsync(...)` 호출을 `jobWorker.ProcessOneQueuedJobAsync(...)`로 바꾼다.

- [ ] **Step 6: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`. 특히 `ApiJobStoreFailureCharacterizationTests`와 `ApiJobStoreGoldenCharacterizationTests`가 통과해야 한다 — 이 둘이 `BeforeJobFinalization` seam과 잠금 동작을 확인한다.

- [ ] **Step 7: 라인 수 ratchet 하향**

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiJobWorker.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): give the queued job worker tick its own owner"
```

---

### Task 11: rate limit과 timeout 응답 소유자 분리

**Files:**
- Create: `src/DesktopNode.Api/DesktopNodeApiRequestThrottle.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory`, `DesktopNodeApiRequestParsing.NormalizePath`
- Produces: `internal sealed class DesktopNodeApiRequestThrottle` — 생성자 `(DesktopNodeApiHardeningOptions hardeningOptions, object sync)`, 메서드 `DesktopNodeApiResponse? Enforce(DesktopNodeApiRequest request)`, `static DesktopNodeApiResponse RouteTimeoutResponse(int timeoutSeconds, int retryAfterSeconds, string requestId)`.

**주의 — 잠금.** `EnforceRequestRateLimit`은 지금 `lock (sync)`를 잡는데, 호출자인 `Handle`이 이미 같은 `sync`를 잡고 있다(`Monitor`는 재진입 가능하므로 동작한다). 소유자에 **새 잠금을 만들지 않는다.** Task 10과 같은 이유로 `sync`를 생성자로 넘긴다. `requestWindows` 사전은 소유자로 옮긴다 — 이 잠금 아래에서만 접근되기 때문이다.

- [ ] **Step 1: guard `[Fact]` 추가 (실패해야 함)**

```csharp
    [Fact]
    public void RequestThrottlingLeavesTheProcessor()
    {
        AssertProcessorDoesNotDeclare(
            "EnforceRequestRateLimit",
            "RateLimitExceededResponse",
            "RouteTimeoutResponse");
        AssertTypeDeclares("DesktopNodeApiRequestThrottle", "Enforce", "RouteTimeoutResponse");
        AssertTypeIsCallbackFreeOwner("DesktopNodeApiRequestThrottle");
    }
```

- [ ] **Step 2: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~RequestThrottlingLeavesTheProcessor`
Expected: FAIL

- [ ] **Step 3: `DesktopNodeApiRequestThrottle` 생성**

`3`개 메서드와 `requestWindows` 필드를 옮긴다. `EnforceRequestRateLimit`은 `Enforce`로 이름만 바꾼다 — 타입 이름이 이미 문맥을 준다. 본문 로직은 바꾸지 않는다.

- [ ] **Step 4: processor 배선**

`private readonly DesktopNodeApiRequestThrottle throttle;`를 추가하고 생성자에서 `new DesktopNodeApiRequestThrottle(this.hardeningOptions, sync)`로 초기화한다. `Handle`의 `EnforceRequestRateLimit(normalizedRequest)`를 `throttle.Enforce(normalizedRequest)`로, `HandleCoreWithRouteTimeout`의 `RouteTimeoutResponse(...)`를 `DesktopNodeApiRequestThrottle.RouteTimeoutResponse(...)`로 바꾼다. `requestWindows` 필드를 processor에서 제거한다.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`. 특히 `ApiHardeningRequestProcessorTests`가 통과해야 한다.

- [ ] **Step 6: 라인 수 ratchet 하향**

목표: `DesktopNodeApiRequestProcessor.cs` `450`줄 이하. 이 시점 실측값이 `450`을 넘으면 남은 책임을 세어 보고한다.

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiRequestThrottle.cs src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(api): give request rate limiting and route timeout responses their own owner"
```

---

### Task 12: wave 1 소유자의 helper 사본 제거

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiDiagnosticsHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiOpsSummaryHandler.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs`

**Interfaces:**
- Consumes: `DesktopNodeApiResponseFactory`, `DesktopNodeApiRequestParsing`
- Produces: 없음(정리 task)

**설명:** wave 1이 세 소유자를 떼어낼 때 `Json`, `Body`, `Failure`, `SerializeResponsePayload`, `TryParseBody`, `ParsedJson`을 각자 복사해 갔다. Task 1과 Task 3이 공용 타입을 만든 지금, 사본은 네 번째 진실 원본이다. 이 task가 사본을 지우고 공용 타입을 쓰게 한다.

**먼저 확인할 것:** 세 사본이 원본과 **정말 같은지**. `DesktopNodeApiAuthSessionHandler.Body`는 시그니처가 다르고(`DesktopNodeApiAuthSessionHandler.cs:247`), `AuthValidationFailure`라는 자체 wrapper도 있다. 다르면 다른 것이다 — 억지로 합치지 말고, 정말 동일한 것만 지운다. 무엇을 남겼고 왜 남겼는지 커밋 메시지에 쓴다.

- [ ] **Step 1: 사본 대조**

각 사본을 `DesktopNodeApiResponseFactory` / `DesktopNodeApiRequestParsing`의 대응 멤버와 문자 단위로 대조하고, 동일한 것과 다른 것을 표로 정리한다.

- [ ] **Step 2: guard `[Fact]` 추가 (실패해야 함)**

Step 1에서 **동일하다고 확인된 것만** 대상으로 한다.

```csharp
    [Fact]
    public void Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies()
    {
        // Step 1 대조에서 공용 타입과 동일하다고 확인된 이름만 여기 넣는다.
        // 시그니처가 다른 사본(DesktopNodeApiAuthSessionHandler.Body 등)은 대상이 아니다.
        foreach (var typeName in new[]
        {
            "DesktopNodeApiDiagnosticsHandler",
            "DesktopNodeApiOpsSummaryHandler"
        })
        {
            var declared = GetDeclaredMethodNames(typeName);
            Assert.DoesNotContain("Json", declared);
            Assert.DoesNotContain("SerializeResponsePayload", declared);
        }
    }
```

- [ ] **Step 3: 테스트를 돌려 실패를 확인**

Run: `dotnet test src/DesktopNode.Api.Tests --filter FullyQualifiedName~Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies`
Expected: FAIL

- [ ] **Step 4: 사본 제거**

동일한 사본을 지우고 공용 타입 호출로 바꾼다. 다른 것은 남기고 왜 다른지 주석을 단다.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS, 실패 `0`. `ApiArchitectureOwnershipTests`가 세 소유자의 필드·메서드를 단언하므로 함께 통과해야 한다.

- [ ] **Step 6: 커밋**

```bash
git add src/DesktopNode.Api/DesktopNodeApiDiagnosticsHandler.cs src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs src/DesktopNode.Api/DesktopNodeApiOpsSummaryHandler.cs src/DesktopNode.Api.Tests/ApiRequestProcessorDecompositionOwnershipTests.cs
git commit -m "refactor(api): drop duplicated response helpers from the wave 1 owners"
```

---

## 종료 조건

전 task 완료 후 아래를 모두 만족해야 한다.

| 항목 | 기준 |
| --- | --- |
| `dotnet test src/DesktopNode.sln` | 실패 `0`, 통과 `842` + 신규 guard |
| `DesktopNodeApiRequestProcessor.cs` | `450`줄 이하 |
| `module-size-ratchet.json` | `DesktopNodeApiRequestProcessor.cs` ceiling이 실측값 |
| `PcvModuleSizeRatchet.Tests.ps1` | 통과 |
| `ApiArchitectureOwnershipTests` | wave 1 단언 전부 통과(회귀 없음) |
| callback adapter | `DesktopNodeApiJobRuntimeHandler`, `DesktopNodeApiConsoleHandler` 부재 |
| Pester(ratchet fixture 제외) | 변경 없음 |

**증거 문서:** 완료 후 `docs/ga-ready/evidence/api-request-processor-decomposition-2026-08-06.md`를 쓴다. `docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`와 같은 형식이며, 착수/종료 라인 수, task별 이동 목록, 테스트 수 변화, 그리고 **비순수 이동 `3`건**을 명시한다.

| # | 항목 | 무엇이 순수하지 않은가 |
| ---: | --- | --- |
| 1 | Task 4 `JobStoreCommitError` | 인스턴스 상태(`jobRuntime.LoadBlock`)를 읽으므로 파라미터 `1`개를 추가했다. 계획 작성 시 가정이 틀렸고 측정으로 드러났다. |
| 2 | Task 10·11 `sync` 공유 | 소유자가 자기 잠금을 만들지 않고 processor의 `sync`를 생성자로 받는다. 새 잠금을 만들면 요청 처리와 worker tick의 상호 배제가 깨진다. |
| 3 | Task 10 `BeforeJobFinalization` | `Action?`이라 callback-free 규칙과 충돌한다. 테스트 seam이므로 예외로 두되 guard가 "정확히 이것 하나"임을 잠근다. |

**남기는 것:** `web/src/served-app.ts`(`4,005`줄) 프런트엔드 분해는 이 계획의 범위가 아니다. 백엔드가 끝난 뒤 별도 계획서로 다룬다.

## Self-Review

- **범위 대조:** 착수 시점 `3,367`줄 중 이 계획이 이동시키지 않는 것은 public record `5`종(`42`줄), 생성자·factory(`104`줄), `Handle`/`HandleCoreWithRouteTimeout`(`78`줄), `HandleCore`(`255`줄, dispatch로 축소), `HandleControlledRouteTimeoutProbe`(`13`줄), 소유자 필드·using이다. 합계가 `450`줄 이하 목표와 맞는다.
- **placeholder 점검:** 모든 step이 실행할 명령과 기대 결과를 갖는다. "적절히 처리" 류 표현 없음.
- **타입 일관성:** Task 4가 만드는 `DesktopNodeApiHyperVOperationInvoker.Invoke`를 Task 8·9·10이 같은 이름으로 쓴다. Task 8이 만드는 `Build*Parameters` `3`종을 Task 9가 같은 시그니처로 쓴다. Task 1의 `DesktopNodeApiResponseFactory` 멤버 이름이 전 task에서 동일하다.
- **의존 순서:** helper(1~4) → 단순 소유자(5~7) → 큰 소유자(8~9) → 잠금 공유 소유자(10~11) → 정리(12). 뒤 task가 앞 task 산출물만 소비하고 역방향 의존이 없다.
