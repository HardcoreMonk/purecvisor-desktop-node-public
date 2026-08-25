# DesktopNodeHostServiceAction 도메인 분해 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`(`4,069`줄)의 도메인별 native action 구현을 이미 존재하는 `Ops/` 도메인 클래스로 옮겨, 서비스 코어의 유일한 잔여 항목인 대형 모듈을 해소한다.

**Architecture:** 새 계층을 만들지 않는다. `ExecuteAsync`는 이미 `Ops.DesktopNode*Ops.Execute()`로 분기하고, 그 Ops 클래스는 다시 `DesktopNodeHostServiceAction.ExecuteNative*ActionForOps()`를 호출해 같은 파일로 돌아온다. 이 **왕복(boomerang)** 을 제거하는 것이 이 작업의 전부다. 각 도메인의 구현과 private helper를 해당 Ops 클래스로 옮기고 `ExecuteNative*ActionForOps` forwarder를 삭제하면, 호출은 `ExecuteAsync -> Ops.X.Execute` 한 방향으로 끝난다.

**Tech Stack:** C# / .NET 10 (`net10.0-windows`), xUnit, `System.Reflection.Metadata`(ownership guard), Pester 5(라인 수 gate)

## Global Constraints

- 공개 표면 4개는 이름·시그니처·동작을 바꾸지 않는다: `CreatePlan`, `ExecuteAsync`(4 오버로드), `EnsureProtectedTokenFile`, `EnsureAccountAuthBootstrapFiles`. 호출자는 `Program.cs` 1곳과 테스트 `69`곳이다.
- `ExecuteNative*ActionForOps` forwarder `9`개는 이동 후 **삭제**한다. 남겨두면 왕복이 그대로 남는다.
- 이동 대상 코드의 **동작을 바꾸지 않는다.** 이름 변경, 시그니처 변경, 로직 정리, 오류 코드 변경 금지. 순수 이동만 한다. 개선은 별도 변경으로 분리한다.
- 각 task는 `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`의 `DesktopNodeHostServiceAction.cs` ceiling을 이동 후 실측값으로 **낮춘다**. gate는 ceiling이 실측보다 `50`줄 넘게 위에 남으면 실패하므로 낮추지 않으면 task가 끝나지 않는다.
- 각 task 종료 시 `dotnet test src/DesktopNode.sln`이 통과해야 한다. 기준선은 `825/825`이며 task마다 ownership guard가 `1`건씩 늘어난다.
- 모든 신규/수정 문서는 한국어 본문으로 쓴다. 코드 식별자, 명령어, 파일 경로는 원문을 유지한다(AGENTS.md 작업 원칙).
- `packaging/windows-desktop-node/tests`, `installer/tests`, `web/tests` Pester는 이 작업에서 변경되지 않아야 한다. 변경되면 이동이 순수하지 않았다는 신호다.

## 안전망

이 리팩토링은 아래 자산이 있어서 가능하다. 착수 전 존재를 확인한다.

| 자산 | 값 |
| --- | --- |
| characterization 테스트 | `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs` — `84` tests / `3,543`줄 |
| Host.Tests 전체 | `187` tests |
| 솔루션 전체 | `825` tests |
| internals 접근 | `src/DesktopNode.Host/DesktopNode.Host.csproj`에 `<InternalsVisibleTo Include="DesktopNode.Host.Tests" />` 존재 |
| 라인 수 gate | `packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1` |
| ownership guard 전례 | `src/DesktopNode.Runtime.Tests/RuntimeArchitectureOwnershipTests.cs` |

## File Structure

**수정(대상):** `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
남는 책임: DTO record `11`개, `CreatePlan`과 그 helper, `ExecuteAsync` 분기, 비 native command 실행 경로, token file helper.

**수정(수용):** 아래 `9`개는 이미 존재하며 현재는 `19`~`26`줄짜리 위임 shim이다. 각자 자기 도메인 구현을 소유하게 된다.

| 파일 | 소유하게 될 도메인 | 현재 |
| --- | --- | ---: |
| `src/DesktopNode.Host/Ops/DesktopNodeFirewallOps.cs` | 방화벽 규칙 | `19`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeTrustStoreOps.cs` | 신뢰 저장소 인증서 | `19`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeEventLogOps.cs` | Event Log source/기본 전환 | `19`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeConfigMigrationOps.cs` | config 마이그레이션 | `22`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeJobStoreMigrationOps.cs` | job store 마이그레이션 | `22`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs` | Credential Manager 전환 | `26`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeServiceLifecycleOps.cs` | 서비스 configure/repair/remove | `26`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeDataRootLifecycleOps.cs` | data root 삭제와 ACL | `22`줄 |
| `src/DesktopNode.Host/Ops/DesktopNodeServiceTokenOps.cs` | 서비스 token 회전/폐기 | `24`줄 |

**생성:** `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs` — 도메인이 제자리를 떠나지 않게 잠그는 guard. task마다 `[Fact]` `1`개씩 추가한다.

**수정:** `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json` — task마다 ceiling 하향.

## 이동 대상 지도

착수 시점 `71fdfcf2` 기준 줄 번호다. **각 task는 이동 직전에 줄 번호를 다시 확인한다.** 앞선 task가 이동을 마치면 뒤 task의 줄 번호가 밀린다. 메서드 이름이 진짜 기준이고 줄 번호는 참고값이다.

| Task | 도메인 | 이동할 메서드 |
| ---: | --- | --- |
| 1 | 방화벽 | `ExecuteNativeFirewallActionForOps`, `NativeFirewallFailure` |
| 2 | 신뢰 저장소 | `ExecuteNativeTrustStoreActionForOps`, `NativeTrustStoreFailure` |
| 3 | Event Log | `ExecuteNativeEventLogActionForOps`, `ExecuteEventLogDefaultTransitionWithTimeout`, `ExecuteEventLogDefaultTransitionCore`, `WriteEventLogDefaultTransitionEvidence`, `NativeEventLogFailure` |
| 4 | config 마이그레이션 | `ExecuteNativeConfigMigrationActionForOps`, `ExecuteNativeConfigMigrationAction`, `ApplyNativeConfigMigration`, `NativeConfigMigrationFailure`, `TryReadProductManifest` |
| 5 | job store 마이그레이션 | `ExecuteNativeJobStoreMigrationActionForOps`, `ExecuteNativeJobStoreMigrationAction`, `ApplyNativeJobStoreMigration`, `NativeJobStoreMigrationFailure`, `TryReadJobStore` |
| 6 | Credential Manager | `ExecuteNativeCredentialManagerActionForOps`, `CredentialManagerResult`, `ExecuteNativeCredentialManagerDefaultTransition`, `CreateCredentialManagerTransitionDescriptor`, `CredentialManagerTransitionResult`, `WriteCredentialManagerTransitionEvidence`, `WriteCredentialManagerTransitionRollbackDiagnostics`, `FixedTimeEquals`, `CreateServiceConfigurationFromBinaryPath`, `UsesCredentialManagerTokenSource` |
| 7 | 서비스 lifecycle | `ExecuteNativeServiceActionForOps`, `ExecuteNativeServiceAction`, `ExecuteNativeConfigureOrRepair`, `ExecuteNativeRemove` |
| 8 | data root lifecycle | `ExecuteNativeDataRootLifecycleActionForOps`, `ExecuteNativeDataRootLifecycleAction`, `ExecuteNativeDataRootRemove`, `RemoveDataPaths`, `RemoveOwnedJobStoreTempFiles`, `PrepareDirectoryForDelete`, `PrepareFileForDelete`, `RestoreFileDeleteAcl`, `RestoreDirectoryDeleteAcl`, `AllowDeleteForServiceAdministrators` |
| 9 | 서비스 token | `ExecuteNativeServiceTokenActionForOps`, `ExecuteNativeServiceTokenRotationRevoke`, `WriteServiceTokenRotationAudit` |

Task `9` 이후 `EnsureProtectedTokenFile`, `EnsureAccountAuthBootstrapFiles`, `WriteProtectedTokenFile`, `ReadProtectedTokenSha256`, `CreateToken`, `EnsureResultTokenPath`는 **남긴다.** 공개 표면이고 `CreatePlan`/`ExecuteAsync` 경로가 함께 쓴다.

---

### Task 1: 방화벽 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeFirewallActionForOps` ≈1055-1153, `NativeFirewallFailure` ≈3437-3457 제거)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeFirewallOps.cs`
- Create: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: 없음(첫 task)
- Produces: `DesktopNode.Host.Ops.DesktopNodeFirewallOps.Execute(DesktopNodeHostOptions, DesktopNodeHostServiceActionPlan, IDesktopNodeWindowsFirewallController)` — 시그니처 불변, 이제 구현을 직접 소유. 이후 task는 `HostServiceActionOwnershipTests.AssertServiceActionDoesNotDeclare` / `AssertOpsTypeDeclares` / `AssertServiceActionDeclares` helper를 재사용한다.

- [ ] **Step 1: ownership guard 테스트 작성 (실패해야 함)**

`src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs` 생성:

```csharp
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DesktopNode.Host;

namespace DesktopNode.Host.Tests;

// DesktopNodeHostServiceAction 은 CreatePlan/ExecuteAsync/token 표면만 소유한다. 도메인 native
// action 구현이 이 타입으로 되돌아오면 ExecuteAsync -> Ops -> ServiceAction 왕복이 되살아나므로
// 각 도메인마다 "떠났는지"와 "도착했는지"를 함께 잠근다.
//
// BindingFlags 대신 metadata 를 읽는 이유: csharp-architecture-test-migration.json 이 test 코드의
// private_reflection.current_occurrence_count 를 0 으로 고정하고 있고,
// RuntimeArchitectureOwnershipTests 가 PEReader 를 그 정책에 맞는 패턴으로 이미 세워 뒀다.
public sealed class HostServiceActionOwnershipTests
{
    private const string HostNamespace = "DesktopNode.Host";
    private const string OpsNamespace = "DesktopNode.Host.Ops";

    internal static string[] GetDeclaredMethodNames(string typeNamespace, string typeName)
    {
        using var assemblyStream = File.OpenRead(typeof(DesktopNodeHostServiceAction).Assembly.Location);
        using var peReader = new PEReader(assemblyStream);
        var metadata = peReader.GetMetadataReader();
        var typeHandle = metadata.TypeDefinitions.Single(handle =>
        {
            var definition = metadata.GetTypeDefinition(handle);
            return metadata.GetString(definition.Namespace) == typeNamespace &&
                metadata.GetString(definition.Name) == typeName;
        });

        return metadata.GetTypeDefinition(typeHandle)
            .GetMethods()
            .Select(handle => metadata.GetString(metadata.GetMethodDefinition(handle).Name))
            .ToArray();
    }

    internal static void AssertServiceActionDoesNotDeclare(params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(HostNamespace, nameof(DesktopNodeHostServiceAction));
        foreach (var methodName in methodNames)
        {
            Assert.DoesNotContain(methodName, declared);
        }
    }

    internal static void AssertServiceActionDeclares(params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(HostNamespace, nameof(DesktopNodeHostServiceAction));
        foreach (var methodName in methodNames)
        {
            Assert.Contains(methodName, declared);
        }
    }

    internal static void AssertOpsTypeDeclares(string opsTypeName, params string[] methodNames)
    {
        var declared = GetDeclaredMethodNames(OpsNamespace, opsTypeName);
        foreach (var methodName in methodNames)
        {
            Assert.Contains(methodName, declared);
        }
    }

    [Fact]
    public void FirewallDomainLivesInFirewallOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeFirewallActionForOps",
            "NativeFirewallFailure");
        AssertOpsTypeDeclares(
            "DesktopNodeFirewallOps",
            "Execute",
            "NativeFirewallFailure");
    }
}
```

- [ ] **Step 2: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FirewallDomainLivesInFirewallOps`
Expected: FAIL — `Assert.DoesNotContain() Failure`. `ExecuteNativeFirewallActionForOps`가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 3: 구현 이동**

`DesktopNodeHostServiceAction.cs`에서 `ExecuteNativeFirewallActionForOps` 본문과 `NativeFirewallFailure` 전체를 잘라낸다. `DesktopNodeFirewallOps.cs`의 `Execute` 메서드 본문을 `return DesktopNodeHostServiceAction.ExecuteNativeFirewallActionForOps(...)` 한 줄에서 잘라낸 본문으로 교체하고, `NativeFirewallFailure`를 같은 클래스에 `private static`으로 붙인다. `DesktopNodeHostServiceAction.cs`에서 두 메서드를 삭제한다.

`DesktopNodeFirewallOps.cs`에 필요한 `using`을 추가한다. 이동한 코드가 참조하는 타입 기준으로 최소한 아래가 필요하다:

```csharp
using DesktopNode.Host;
```

로직·이름·오류 코드는 그대로 둔다. 순수 이동이다.

- [ ] **Step 4: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `188` tests (기존 `187` + guard `1`).

- [ ] **Step 5: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `826` tests, 실패 `0`. 특히 `DesktopNodeHostServiceActionTests`의 `84`건이 전부 통과해야 한다. 한 건이라도 실패하면 이동이 순수하지 않았다는 뜻이므로 되돌리고 원인을 먼저 찾는다.

- [ ] **Step 6: 라인 수 ceiling 하향**

실측:

```powershell
@(Get-Content -LiteralPath src/DesktopNode.Host/DesktopNodeHostServiceAction.cs).Count
```

`packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`에서 `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` 항목의 `max_lines`를 그 값으로 바꾼다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`. 실패하면 ceiling을 안 낮췄거나 `50`줄 slack을 넘긴 것이다.

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Host/DesktopNodeHostServiceAction.cs src/DesktopNode.Host/Ops/DesktopNodeFirewallOps.cs src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the firewall native action into DesktopNodeFirewallOps"
```

---

### Task 2: 신뢰 저장소 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeTrustStoreActionForOps` ≈1154-1251, `NativeTrustStoreFailure` ≈3458-3478 제거)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeTrustStoreOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare(params string[])`, `AssertOpsTypeDeclares(string opsTypeName, params string[])`
- Produces: `DesktopNode.Host.Ops.DesktopNodeTrustStoreOps.Execute(DesktopNodeHostOptions, DesktopNodeHostServiceActionPlan, IDesktopNodeWindowsTrustStoreController)` — 시그니처 불변, 구현 직접 소유

- [ ] **Step 1: guard 테스트 추가 (실패해야 함)**

`HostServiceActionOwnershipTests.cs`에 추가:

```csharp
    [Fact]
    public void TrustStoreDomainLivesInTrustStoreOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeTrustStoreActionForOps",
            "NativeTrustStoreFailure");
        AssertOpsTypeDeclares(
            "DesktopNodeTrustStoreOps",
            "Execute",
            "NativeTrustStoreFailure");
    }
```

- [ ] **Step 2: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter TrustStoreDomainLivesInTrustStoreOps`
Expected: FAIL — `ExecuteNativeTrustStoreActionForOps`가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 3: 구현 이동**

`ExecuteNativeTrustStoreActionForOps` 본문을 `DesktopNodeTrustStoreOps.Execute`로 옮기고, `NativeTrustStoreFailure`를 같은 클래스에 `private static`으로 옮긴다. `DesktopNodeHostServiceAction.cs`에서 두 메서드를 삭제한다. 로직은 그대로 둔다.

- [ ] **Step 4: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `189` tests.

- [ ] **Step 5: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `827` tests, 실패 `0`.

- [ ] **Step 6: 라인 수 ceiling 하향**

`@(Get-Content -LiteralPath src/DesktopNode.Host/DesktopNodeHostServiceAction.cs).Count`로 실측해 `module-size-ratchet.json`의 `max_lines`를 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the trust store native action into DesktopNodeTrustStoreOps"
```

---

### Task 3: Event Log 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeEventLogActionForOps` ≈566-759, `ExecuteEventLogDefaultTransitionWithTimeout` ≈760-819, `ExecuteEventLogDefaultTransitionCore` ≈820-891, `WriteEventLogDefaultTransitionEvidence` ≈967-1019, `NativeEventLogFailure` ≈3414-3436 제거)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeEventLogOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare`, `AssertOpsTypeDeclares`
- Produces: `DesktopNode.Host.Ops.DesktopNodeEventLogOps.Execute(DesktopNodeHostOptions, DesktopNodeHostServiceActionPlan, IDesktopNodeWindowsEventLogController)` — 시그니처 불변, 기본 전환 경로와 evidence 기록까지 소유

- [ ] **Step 1: guard 테스트 추가 (실패해야 함)**

```csharp
    [Fact]
    public void EventLogDomainLivesInEventLogOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeEventLogActionForOps",
            "ExecuteEventLogDefaultTransitionWithTimeout",
            "ExecuteEventLogDefaultTransitionCore",
            "WriteEventLogDefaultTransitionEvidence",
            "NativeEventLogFailure");
        AssertOpsTypeDeclares(
            "DesktopNodeEventLogOps",
            "Execute",
            "ExecuteEventLogDefaultTransitionCore",
            "NativeEventLogFailure");
    }
```

- [ ] **Step 2: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter EventLogDomainLivesInEventLogOps`
Expected: FAIL — 다섯 메서드가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 3: 구현 이동**

다섯 메서드를 `DesktopNodeEventLogOps`로 옮긴다. `ExecuteNativeEventLogActionForOps`의 본문이 `Execute`가 되고 나머지 넷은 `private static`으로 붙는다. timeout 경로가 `System.Threading` 타입을 쓰므로 `using` 누락 여부를 빌드로 확인한다. `DesktopNodeHostServiceAction.cs`에서 다섯 메서드를 삭제한다.

- [ ] **Step 4: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `190` tests.

- [ ] **Step 5: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `828` tests, 실패 `0`.

- [ ] **Step 6: Event Log Pester 회귀 확인**

Event Log 전환은 packaging runner도 계약을 가진다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvWindowsEventLogDefaultTransitionSmoke.Tests.ps1','packaging/windows-desktop-node/tests/PcvWindowsEventLogProviderTransitionPreflight.Tests.ps1'"`
Expected: PASS, 실패 `0`. 이 두 스위트는 이 task에서 변경되지 않아야 한다.

- [ ] **Step 7: 라인 수 ceiling 하향**

실측해 `module-size-ratchet.json`을 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the event log native action into DesktopNodeEventLogOps"
```

---

### Task 4: config 마이그레이션 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeConfigMigrationActionForOps` ≈1460-1482, `ExecuteNativeConfigMigrationAction` ≈1483-1583, `ApplyNativeConfigMigration` ≈1584-1737, `NativeConfigMigrationFailure` ≈2062-2114, `TryReadProductManifest` ≈2166-2209 제거)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeConfigMigrationOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare`, `AssertOpsTypeDeclares`
- Produces: `DesktopNode.Host.Ops.DesktopNodeConfigMigrationOps.Execute(...)` — 기존 시그니처 불변. `DesktopNodeHostConfigMigrationDescriptor`와 `DesktopNodeHostConfigMigrationSource` record는 `DesktopNodeHostServiceAction.cs`에 그대로 둔다(Task 10에서 함께 정리).

- [ ] **Step 1: guard 테스트 추가 (실패해야 함)**

```csharp
    [Fact]
    public void ConfigMigrationDomainLivesInConfigMigrationOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeConfigMigrationActionForOps",
            "ExecuteNativeConfigMigrationAction",
            "ApplyNativeConfigMigration",
            "NativeConfigMigrationFailure",
            "TryReadProductManifest");
        AssertOpsTypeDeclares(
            "DesktopNodeConfigMigrationOps",
            "Execute",
            "ApplyNativeConfigMigration",
            "TryReadProductManifest");
    }
```

- [ ] **Step 2: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter ConfigMigrationDomainLivesInConfigMigrationOps`
Expected: FAIL — 다섯 메서드가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 3: 구현 이동**

다섯 메서드를 `DesktopNodeConfigMigrationOps`로 옮긴다. JSON 처리를 쓰므로 `using System.Text.Json;`과 `using System.Text.Json.Nodes;`가 필요하다. `DesktopNodeHostServiceAction.cs`에서 다섯 메서드를 삭제한다.

`TryReadProductManifest`가 Task 5의 job store 경로에서도 쓰이는지 이동 전에 확인한다:

```bash
grep -n "TryReadProductManifest" src/DesktopNode.Host/DesktopNodeHostServiceAction.cs
```

호출자가 config 경로 밖에도 있으면 이 메서드는 옮기지 말고 `DesktopNodeHostServiceAction`에 남긴 뒤 guard 테스트의 `AssertServiceActionDoesNotDeclare` 목록과 `AssertOpsTypeDeclares` 목록에서 `TryReadProductManifest`를 뺀다.

- [ ] **Step 4: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `191` tests.

- [ ] **Step 5: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `829` tests, 실패 `0`.

- [ ] **Step 6: 라인 수 ceiling 하향**

실측해 `module-size-ratchet.json`을 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the config migration native action into DesktopNodeConfigMigrationOps"
```

---

### Task 5: job store 마이그레이션 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeJobStoreMigrationActionForOps` ≈1738-1760, `ExecuteNativeJobStoreMigrationAction` ≈1761-1910, `ApplyNativeJobStoreMigration` ≈1911-2061, `NativeJobStoreMigrationFailure` ≈2115-2165, `TryReadJobStore` ≈2210-2249 제거)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeJobStoreMigrationOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare`, `AssertOpsTypeDeclares`
- Produces: `DesktopNode.Host.Ops.DesktopNodeJobStoreMigrationOps.Execute(...)` — 기존 시그니처 불변

- [ ] **Step 1: guard 테스트 추가 (실패해야 함)**

```csharp
    [Fact]
    public void JobStoreMigrationDomainLivesInJobStoreMigrationOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeJobStoreMigrationActionForOps",
            "ExecuteNativeJobStoreMigrationAction",
            "ApplyNativeJobStoreMigration",
            "NativeJobStoreMigrationFailure",
            "TryReadJobStore");
        AssertOpsTypeDeclares(
            "DesktopNodeJobStoreMigrationOps",
            "Execute",
            "ApplyNativeJobStoreMigration",
            "TryReadJobStore");
    }
```

- [ ] **Step 2: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter JobStoreMigrationDomainLivesInJobStoreMigrationOps`
Expected: FAIL — 다섯 메서드가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 3: 구현 이동**

다섯 메서드를 `DesktopNodeJobStoreMigrationOps`로 옮긴다. `DesktopNodeHostServiceAction.cs`에서 삭제한다.

`RemoveOwnedJobStoreTempFiles`는 이름이 job store지만 **data root 삭제 경로**에서 호출된다. Task 8이 소유하므로 이 task에서는 건드리지 않는다. 이동 전에 확인한다:

```bash
grep -n "RemoveOwnedJobStoreTempFiles" src/DesktopNode.Host/DesktopNodeHostServiceAction.cs
```

- [ ] **Step 4: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `192` tests.

- [ ] **Step 5: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `830` tests, 실패 `0`. `DesktopNode.Runtime.Tests`의 job store durability `37`건이 함께 통과해야 한다.

- [ ] **Step 6: packaging Pester 전체 회귀 확인**

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"`
Expected: PASS `485/485`, 실패 `0`. 이 스위트는 이 계획 전체에서 건수가 변하지 않아야 한다. 변하면 이동이 순수하지 않았다는 신호다.

- [ ] **Step 7: 라인 수 ceiling 하향**

실측해 `module-size-ratchet.json`을 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the job store migration native action into DesktopNodeJobStoreMigrationOps"
```

---

### Task 6: Credential Manager 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeCredentialManagerActionForOps` ≈892-966, `CredentialManagerResult` ≈1020-1054, `ExecuteNativeCredentialManagerDefaultTransition` ≈2706-3061, `CreateCredentialManagerTransitionDescriptor` ≈3257-3304, `CredentialManagerTransitionResult` ≈3305-3328, `WriteCredentialManagerTransitionEvidence` ≈3329-3342, `WriteCredentialManagerTransitionRollbackDiagnostics` ≈3343-3376, `FixedTimeEquals` ≈3377-3392 제거)

**실행 후 기록 (실제로는 열 개를 옮겼다):** 위 여덟 개 외에, credential-manager 롤백 경로에서만 쓰이는
`CreateServiceConfigurationFromBinaryPath`와 `UsesCredentialManagerTokenSource`도 함께 옮겼다 —
원래 계획은 전자를 Task 7 소유로 표시했지만(줄 69 표), 실제 호출자는 credential-manager 도메인
뿐이었다. 반대로 `CreateServiceConfiguration`과 `ExtractNamedArgumentValue`는 `ExecuteNativeConfigureOrRepair`(Task 7 소유)에서도 호출되는 것을 확인해 옮기지 않고 `private` → `internal`로만
넓혀 남겼다. `UsesProtectedFileTokenSource`도 같은 이유로 넓혀 남겼다 — `ExecuteNativeServiceTokenRotationRevoke`(Task 9 소유)가 호출한다. 아래 Task 7/9 주의 참조.
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare`, `AssertOpsTypeDeclares`
- Produces: `DesktopNode.Host.Ops.DesktopNodeCredentialManagerOps.Execute(...)` — 기존 시그니처 불변. 이 task가 가장 큰 단일 이동(`≈600`줄)이다.

- [ ] **Step 1: guard 테스트 추가 (실패해야 함)**

```csharp
    [Fact]
    public void CredentialManagerDomainLivesInCredentialManagerOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeCredentialManagerActionForOps",
            "CredentialManagerResult",
            "ExecuteNativeCredentialManagerDefaultTransition",
            "CreateCredentialManagerTransitionDescriptor",
            "CredentialManagerTransitionResult",
            "WriteCredentialManagerTransitionEvidence",
            "WriteCredentialManagerTransitionRollbackDiagnostics",
            "FixedTimeEquals");
        AssertOpsTypeDeclares(
            "DesktopNodeCredentialManagerOps",
            "Execute",
            "ExecuteNativeCredentialManagerDefaultTransition",
            "FixedTimeEquals");
    }
```

- [ ] **Step 2: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter CredentialManagerDomainLivesInCredentialManagerOps`
Expected: FAIL — 여덟 메서드가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 3: 구현 이동**

여덟 메서드를 `DesktopNodeCredentialManagerOps`로 옮긴다. `FixedTimeEquals`는 상수 시간 비교이므로 `using System.Security.Cryptography;`가 필요하고, evidence 기록은 `using System.Text.Json;`을 쓴다. `DesktopNodeHostServiceAction.cs`에서 여덟 메서드를 삭제한다.

`FixedTimeEquals`가 credential 경로 밖에서도 호출되는지 이동 전에 확인한다:

```bash
grep -n "FixedTimeEquals" src/DesktopNode.Host/DesktopNodeHostServiceAction.cs
```

token 경로에서도 쓰면 옮기지 말고 남긴 뒤 guard 목록 양쪽에서 `FixedTimeEquals`를 뺀다.

- [ ] **Step 4: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `193` tests.

- [ ] **Step 5: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `831` tests, 실패 `0`.

- [ ] **Step 6: Credential Manager Pester 회귀 확인**

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvWindowsCredentialManagerTransitionPreflight.Tests.ps1'"`
Expected: PASS, 실패 `0`.

- [ ] **Step 7: 라인 수 ceiling 하향**

실측해 `module-size-ratchet.json`을 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the credential manager native action into DesktopNodeCredentialManagerOps"
```

---

### Task 7: 서비스 lifecycle 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeServiceActionForOps`, `ExecuteNativeServiceAction`, `ExecuteNativeConfigureOrRepair`, `ExecuteNativeRemove` 제거. `NativeServiceFailure`, `IsStopped`, `CreateServiceConfiguration`, `ExtractNamedArgumentValue`는 **남긴다** — 아래 주의 참조)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeServiceLifecycleOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare`, `AssertOpsTypeDeclares`
- Produces: `DesktopNode.Host.Ops.DesktopNodeServiceLifecycleOps.Execute(...)` — 기존 시그니처 불변

**주의 1 — `NativeServiceFailure`와 `IsStopped`는 옮기지 않는다.** Task 4가 이 둘을 `internal`로
넓혀 `DesktopNodeConfigMigrationOps`에서 호출하고 있다. 원래 계획대로 삭제하면 Task 4의 산출물이
컴파일되지 않는다. 두 메서드는 도메인 전용이 아니라 **도메인 간 공유 helper**이므로
`DesktopNodeHostServiceAction`에 `internal`로 남긴다. 같은 이유로 Task 4가 넓힌 `Require`,
`IsOwnedService`, `IsSupportedMigrationPlan`과 Task 5가 넓힌 `OwnedFileExists`,
`IsOwnedFileAccessFailure`도 남긴다. 이는 계획의 "새 계층을 만들지 않는다" 원칙과 일치한다 —
공유 helper를 위한 새 클래스를 만들지 않고 원래 자리에 둔다.

현재까지 `internal`로 넓혀 공유 중인 helper 전체 목록(`9`개): `Require`, `IsOwnedService`,
`NativeServiceFailure`, `IsStopped`, `IsSupportedMigrationPlan`, `OwnedFileExists`,
`IsOwnedFileAccessFailure`, `CreateServiceConfiguration`, `ExtractNamedArgumentValue`. 이동
대상에서 제외한다. (`UsesProtectedFileTokenSource`도 Task 6이 넓혔지만 Task 9 소관이라 아래
Task 9 주의에 기록한다.)

**주의 2 — Task 6이 이미 해결함(더 이상 조사 불필요):** `CreateServiceConfiguration`과 그 helper
4개(`AddOptionalQuotedArgument`, `NormalizeOptionalPath`, `ExtractNamedArgumentValue`,
`CreateServiceConfigurationFromBinaryPath`) 중 `CreatePlan`이 쓰는 것이 있는지 확인하라는 것이
원래 주의였다. Task 6이 credential-manager 이동 과정에서 전체 호출자를 확인했다: `CreatePlan`은
이 helper들을 쓰지 않지만, `ExecuteNativeCredentialManagerDefaultTransition`(Task 6, 이미 이동
완료)이 `CreateServiceConfiguration`과 `ExtractNamedArgumentValue`를 호출한다. 그래서 이 둘은
`internal`로 넓혀 `DesktopNodeHostServiceAction`에 남겼다. `AddOptionalQuotedArgument`와
`NormalizeOptionalPath`는 `CreateServiceConfiguration` 내부에서만 쓰여 그 안에 자연히 남는다(넓힐
필요조차 없다 — `private`인 채로 같은 클래스 안에서 호출된다). `CreateServiceConfigurationFromBinaryPath`는 credential-manager 롤백 경로에서만 쓰여 이미
`DesktopNodeCredentialManagerOps`로 옮겨졌다(`private`, 그 파일 안에서만 호출). 따라서 Task 7은
lifecycle 본체 4개만 옮기면 된다 — helper 5개 중 옮길 수 있는 것은 없다.

- [ ] **Step 1: 공유 helper 호출자 확인(참고용 재확인)**

```bash
grep -rn "CreateServiceConfiguration\|AddOptionalQuotedArgument\|NormalizeOptionalPath\|ExtractNamedArgumentValue\|CreateServiceConfigurationFromBinaryPath" src/DesktopNode.Host/
```

주의 2에서 이미 결론이 났으므로 이 grep은 확인용이다 — `DesktopNodeCredentialManagerOps.cs`에서도
호출되는 것이 보이면 주의 2의 결론과 일치하는지 대조한다.

- [ ] **Step 2: guard 테스트 추가 (실패해야 함)**

주의 2에서 확인했듯 helper 5개 중 옮길 수 있는 것은 없다. lifecycle 본체 4개만 목록에 넣는다.

```csharp
    [Fact]
    public void ServiceLifecycleDomainLivesInServiceLifecycleOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeServiceActionForOps",
            "ExecuteNativeServiceAction",
            "ExecuteNativeConfigureOrRepair",
            "ExecuteNativeRemove");
        AssertOpsTypeDeclares(
            "DesktopNodeServiceLifecycleOps",
            "Execute",
            "ExecuteNativeConfigureOrRepair",
            "ExecuteNativeRemove");
    }
```

- [ ] **Step 3: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter ServiceLifecycleDomainLivesInServiceLifecycleOps`
Expected: FAIL — 네 메서드가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 4: 구현 이동**

lifecycle 본체 4개를 `DesktopNodeServiceLifecycleOps`로 옮긴다. `DesktopNodeHostServiceAction.cs`에서 삭제한다. helper 5개는 주의 2에 따라 그대로 둔다.

- [ ] **Step 5: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `194` tests.

- [ ] **Step 6: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `832` tests, 실패 `0`. `DesktopNodeWindowsServiceControllerTests`가 함께 통과해야 한다.

- [ ] **Step 7: 라인 수 ceiling 하향**

실측해 `module-size-ratchet.json`을 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the service lifecycle native action into DesktopNodeServiceLifecycleOps"
```

---

### Task 8: data root lifecycle 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeDataRootLifecycleActionForOps` ≈1416-1423, `ExecuteNativeDataRootLifecycleAction` ≈1424-1459, `ExecuteNativeDataRootRemove`, `RemoveDataPaths`, `RemoveOwnedJobStoreTempFiles`, `PrepareDirectoryForDelete` ≈3586-3604, `PrepareFileForDelete` ≈3605-3610, `RestoreFileDeleteAcl` ≈3611-3618, `RestoreDirectoryDeleteAcl` ≈3619-3626, `AllowDeleteForServiceAdministrators` ≈3627-3642 제거)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeDataRootLifecycleOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare`, `AssertOpsTypeDeclares`
- Produces: `DesktopNode.Host.Ops.DesktopNodeDataRootLifecycleOps.Execute(...)` — 기존 시그니처 불변. ACL 조작 helper 전체를 소유한다.

**주의 1 — `CreateRemoveDataHandoff`는 이 task 대상이 아니다.** Task 7이 유일한 호출자
`ExecuteNativeRemove`와 함께 이미 `DesktopNodeServiceLifecycleOps`로 옮겼다. 이름은 data root
계열이지만 실제 소속은 service lifecycle이다. `DesktopNodeServiceLifecycleOps.cs`를 건드리지 않는다.

**주의 2:** `RemoveDataPaths`는 `ExecuteAsync`의 비 native command 경로에서도 쓰일 수 있다.
Step 1에서 호출자를 확인하고, `ExecuteAsync` 범위에서 호출되면 옮기지 않고 남긴다.

- [ ] **Step 1: 공유 helper 호출자 확인**

```bash
grep -n "CreateRemoveDataHandoff\|RemoveDataPaths" src/DesktopNode.Host/DesktopNodeHostServiceAction.cs
```

`ExecuteAsync`(≈410-565) 범위에서 호출되면 그 메서드는 옮기지 않고 남긴다. 남기는 메서드는 Step 2의 guard 목록에서 뺀다.

- [ ] **Step 2: guard 테스트 추가 (실패해야 함)**

```csharp
    [Fact]
    public void DataRootLifecycleDomainLivesInDataRootLifecycleOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeDataRootLifecycleActionForOps",
            "ExecuteNativeDataRootLifecycleAction",
            "ExecuteNativeDataRootRemove",
            "RemoveOwnedJobStoreTempFiles",
            "PrepareDirectoryForDelete",
            "PrepareFileForDelete",
            "RestoreFileDeleteAcl",
            "RestoreDirectoryDeleteAcl",
            "AllowDeleteForServiceAdministrators");
        AssertOpsTypeDeclares(
            "DesktopNodeDataRootLifecycleOps",
            "Execute",
            "ExecuteNativeDataRootRemove",
            "AllowDeleteForServiceAdministrators");
    }
```

- [ ] **Step 3: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter DataRootLifecycleDomainLivesInDataRootLifecycleOps`
Expected: FAIL — 열한 메서드가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 4: 구현 이동**

메서드를 `DesktopNodeDataRootLifecycleOps`로 옮긴다. ACL 조작이 `using System.Security.AccessControl;`과 `using System.Security.Principal;`을 쓴다. `DesktopNodeHostServiceAction.cs`에서 삭제하고, 그 결과 `DesktopNodeHostServiceAction.cs` 상단의 두 `using`이 미사용이 되면 함께 제거한다.

- [ ] **Step 5: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `195` tests.

- [ ] **Step 6: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `833` tests, 실패 `0`.

- [ ] **Step 7: 라인 수 ceiling 하향**

실측해 `module-size-ratchet.json`을 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the data root lifecycle native action into DesktopNodeDataRootLifecycleOps"
```

---

### Task 9: 서비스 token 회전/폐기 도메인 이동

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs` (`ExecuteNativeServiceTokenActionForOps` ≈1373-1415, `ExecuteNativeServiceTokenRotationRevoke` ≈2522-2705, `WriteServiceTokenRotationAudit` ≈3750-3773 제거)
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeServiceTokenOps.cs`
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1의 `AssertServiceActionDoesNotDeclare`, `AssertOpsTypeDeclares`
- Produces: `DesktopNode.Host.Ops.DesktopNodeServiceTokenOps.Execute(...)` — 기존 시그니처 불변

**주의:** `EnsureProtectedTokenFile`, `EnsureAccountAuthBootstrapFiles`, `WriteProtectedTokenFile`, `ReadProtectedTokenSha256`, `CreateToken`, `EnsureResultTokenPath`는 **옮기지 않는다.** 앞의 둘은 Global Constraints가 고정한 공개 표면이고, 나머지 넷은 그 구현이다. 회전/폐기 경로가 이들을 호출하므로 Ops에서 `DesktopNodeHostServiceAction.EnsureProtectedTokenFile(...)`을 호출하는 방향이 된다(Ops → ServiceAction, 왕복 아님).

**주의 2 — `UsesProtectedFileTokenSource`도 옮기지 않는다(이미 준비됨):** `ExecuteNativeServiceTokenRotationRevoke`가 이 helper를 호출한다. Task 6이 credential-manager 이동 중 같은 helper를 발견해 이미 `private` → `internal`로 넓혀 `DesktopNodeHostServiceAction`에 남겨 뒀다(Task 7 주의 1의 공유 helper 목록 참조). 원래 이 task의 파일 목록에는 애초에 포함되어 있지 않았으므로 실수로 지울 위험은 낮지만, Step 1에서 `DesktopNodeHostServiceAction`이 이미 `internal static bool UsesProtectedFileTokenSource(...)`를 선언하고 있는 것을 보면 그대로 두고 `DesktopNodeHostServiceAction.UsesProtectedFileTokenSource(...)`로 호출하면 된다.

- [ ] **Step 1: guard 테스트 추가 (실패해야 함)**

```csharp
    [Fact]
    public void ServiceTokenRotationLivesInServiceTokenOps()
    {
        AssertServiceActionDoesNotDeclare(
            "ExecuteNativeServiceTokenActionForOps",
            "ExecuteNativeServiceTokenRotationRevoke",
            "WriteServiceTokenRotationAudit");
        AssertOpsTypeDeclares(
            "DesktopNodeServiceTokenOps",
            "Execute",
            "ExecuteNativeServiceTokenRotationRevoke",
            "WriteServiceTokenRotationAudit");

        // 공개 token 표면은 그대로 남아야 한다. 이 단언이 없으면 위 이동이 공개 표면까지
        // 함께 옮겨가도 통과한다.
        AssertServiceActionDeclares(
            "EnsureProtectedTokenFile",
            "EnsureAccountAuthBootstrapFiles");
    }
```

- [ ] **Step 2: 테스트를 실행해 실패를 확인**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter ServiceTokenRotationLivesInServiceTokenOps`
Expected: FAIL — 세 메서드가 아직 `DesktopNodeHostServiceAction`에 있다.

- [ ] **Step 3: 구현 이동**

세 메서드를 `DesktopNodeServiceTokenOps`로 옮긴다. 토큰 값은 로그·evidence에 남기지 않는 기존 동작을 그대로 유지한다. `DesktopNodeHostServiceAction.cs`에서 세 메서드를 삭제한다.

- [ ] **Step 4: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
Expected: PASS — `196` tests. `DesktopNodeHostTokenResolverTests`가 함께 통과해야 한다.

- [ ] **Step 5: 솔루션 전체 회귀 확인**

Run: `dotnet test src/DesktopNode.sln`
Expected: PASS — `834` tests, 실패 `0`.

- [ ] **Step 6: token 회전 Pester 회귀 확인**

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvServiceTokenRotationRevokePreflight.Tests.ps1'"`
Expected: PASS, 실패 `0`.

- [ ] **Step 7: 라인 수 ceiling 하향**

실측해 `module-size-ratchet.json`을 갱신한다.

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1'"`
Expected: PASS `3/3`.

- [ ] **Step 8: 커밋**

```bash
git add src/DesktopNode.Host src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): move the service token rotation action into DesktopNodeServiceTokenOps"
```

---

### Task 10: forwarder 제거 확정과 evidence 기록

**Files:**
- Modify: `src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs`
- Create: `docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`
- Modify: `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md`
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json`

**Interfaces:**
- Consumes: Task 1-9가 만든 `Ops.*` 도메인 소유와 `GetDeclaredMethodNames` / `AssertServiceActionDeclares`
- Produces: 없음(마지막 task)

- [ ] **Step 1: 왕복이 완전히 사라졌는지 잠그는 테스트 추가 (실패할 수도, 통과할 수도 있음)**

```csharp
    [Fact]
    public void NoOpsForwarderRemainsOnHostServiceAction()
    {
        var declared = GetDeclaredMethodNames("DesktopNode.Host", nameof(DesktopNodeHostServiceAction))
            .Where(name => name.EndsWith("ForOps", StringComparison.Ordinal))
            .ToArray();

        // ExecuteAsync -> Ops.X.Execute -> DesktopNodeHostServiceAction.*ForOps 왕복을 없애는 것이
        // 이 분해의 목적이다. ForOps 이름이 하나라도 남아 있으면 그 도메인은 아직 돌아온다.
        Assert.Empty(declared);
    }

    [Fact]
    public void HostServiceActionKeepsOnlyItsPublicSurface()
    {
        AssertServiceActionDeclares(
            "CreatePlan",
            "ExecuteAsync",
            "EnsureProtectedTokenFile",
            "EnsureAccountAuthBootstrapFiles");
    }
```

- [ ] **Step 2: 테스트 실행**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "NoOpsForwarderRemainsOnHostServiceAction|HostServiceActionKeepsOnlyItsPublicSurface"`
Expected: 두 건 모두 PASS. `NoOpsForwarderRemainsOnHostServiceAction`이 실패하면 Task 1-9 중 forwarder를 지우지 않고 남긴 도메인이 있다는 뜻이므로, 실패 메시지가 알려주는 이름의 도메인 task로 돌아가 forwarder를 삭제한다.

- [ ] **Step 3: 전체 필수 검증 실행**

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests'"
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
git diff --check
```

Expected: 전부 통과. `installer/tests` `49/49`, `web/tests` `49/49`는 이 작업에서 변경되지 않아야 한다. 변경됐다면 이동이 순수하지 않았다.

- [ ] **Step 4: 최종 라인 수 기록과 ceiling 확정**

```powershell
@(Get-Content -LiteralPath src/DesktopNode.Host/DesktopNodeHostServiceAction.cs).Count
Get-ChildItem src/DesktopNode.Host/Ops -Filter *.cs | ForEach-Object { "{0,6}  {1}" -f @(Get-Content -LiteralPath $_.FullName).Count, $_.Name }
```

`module-size-ratchet.json`의 `DesktopNodeHostServiceAction.cs` `max_lines`를 최종 실측값으로 맞춘다.

- [ ] **Step 5: evidence 문서 작성**

`docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`를 만든다. 아래를 포함한다.

```markdown
# DesktopNodeHostServiceAction 도메인 분해 (2026-08-06)

evidence_id: `host-service-action-decomposition-2026-08-06`
result: `PASS`
evidence_scope: `code-level-refactor-no-host-mutation`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 분해 결과

| 파일 | 착수 전 | 종료 후 |
| --- | ---: | ---: |
| `DesktopNodeHostServiceAction.cs` | `4,069` | <최종 실측> |
| `Ops/` 9개 합계 | `199` | <최종 실측> |

## 제거한 구조

`ExecuteAsync -> Ops.X.Execute -> DesktopNodeHostServiceAction.ExecuteNativeXActionForOps` 왕복을
도메인 `9`개 전부에서 제거했다. `*ForOps` forwarder는 `0`개다.

## 불변으로 유지한 공개 표면

`CreatePlan`, `ExecuteAsync`(4 오버로드), `EnsureProtectedTokenFile`,
`EnsureAccountAuthBootstrapFiles`. 호출자 `Program.cs` `1`곳과 테스트 `69`곳은 수정하지 않았다.

## 검증

<Step 3의 실제 결과를 값과 함께 기록한다>

## Nonclaims

- 동작 변경을 하지 않았다. 순수 이동이며 새 기능이나 오류 코드 변경을 주장하지 않는다.
- 이 evidence는 code-level 범위이며 설치본 관측이나 anchor 승격을 주장하지 않는다.
```

- [ ] **Step 6: 평가서에 처리 상태 추가**

`docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md` 끝에 addendum 절을 덧붙인다. 기존 snapshot은 수정하지 않는다(저장소 관례).

```markdown
## 12. 서비스 코어 대형 모듈 처리 addendum (2026-08-06)

§4.3-1과 §8 P1-7의 `DesktopNodeHostServiceAction` 분해를 완료했다. `4,069`줄에서 <최종 실측>줄로
줄었고 도메인 `9`개가 `Ops/` 소유로 옮겨졌다. 상세는
`docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`가 소유한다.

§8 P1-7의 나머지 두 파일(`DesktopNodeApiRequestProcessor.cs`, `web/src/served-app.ts`)은 별도
계획으로 남는다. §4.3-3 public signing은 ADR-0006이 `closed-not-adopted`로 닫은 범위 밖 항목이다.
```

- [ ] **Step 7: 커밋**

```bash
git add src/DesktopNode.Host.Tests/HostServiceActionOwnershipTests.cs docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json
git commit -m "refactor(host): confirm the ops forwarder removal and record the decomposition evidence"
```

---

## 이 계획이 다루지 않는 것

- `DesktopNodeApiRequestProcessor.cs`(`3,024`줄)와 `DesktopNodeHyperVNativeAdapter.cs`(`1,891`줄)는 백엔드 소속이라 별도 계획으로 분리한다. §8 P1-7은 세 파일을 한 항목으로 묶었지만 각각 독립적으로 동작·검증 가능한 단위다.
- `web/src/served-app.ts`(`3,719`줄)는 프론트엔드 소속이며 secure bootstrap 작업과 겹칠 수 있으므로 그 결정 이후로 미룬다.
- 동작 개선, 오류 코드 정리, 인터페이스 재설계는 하지 않는다. 이 계획은 순수 이동만 다룬다.
- `Ops/DesktopNodeHostOpsCatalog.cs`(`176`줄)는 대상이 아니다. 이미 도메인 dispatch 역할만 한다.
