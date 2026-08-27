# 차선 분리 개발 절차 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 코드 계약, 설치본 한 경로 프로브, current 승격이 한 checkpoint에서 섞이지 않게 Lane 0~3 예산·식별자·FAIL 승격 거부를 기계 계약으로 고정한다.

**Architecture:** Lane 1 기본값(`elapsed_minutes_limit=30`, `tool_batch_limit=18`)은 호환 키로 남긴다. 차선 수치는 JSON `lanes` 객체에만 추가한다. 현행 canonical operator id는 `GET /api/v1/vms/{id}`가 받는 표시 이름이다. P0 runner의 `vm get`/`vm delete`는 그 이름을 쓰고, CLI JSON의 내부 `PCV_*`를 summary `error`에 보존한다. inventory Id를 Hyper-V GUID로 바꾸는 제품 payload와 Lane 2 SavedOnly 재실행은 이 계획의 실행 범위가 아니다.

**Tech Stack:** C# / .NET 10, xUnit, `DesktopNode.Delivery.Tests`, `DesktopNode.Api.Tests`, PowerShell 7 runner source, UTF-8 SHA-256 source-file pin.

**Spec:** `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md`

## Global Constraints

- 한국어 본문, 식별자·경로·route·`PCV_*`는 원문.
- Required CI 네 shard: `dotnet`, `web`, `delivery`, `installer-policy`. 이 계획은 focused `delivery` + 관련 Api test만 로컬에서 돌린다. `installer-policy`는 커밋 후 clean HEAD에서만.
- `elapsed_minutes_limit=30`, `tool_batch_limit=18` 최상위 키를 바꾸지 않는다. 그것은 Lane 1 기본값이다.
- `config/pcv-development-policy-contract-spec-v1.json`의 `required_literal_count=652`, `legacy_should_site_count=371`, `legacy_contract_count=51`을 바꾸지 않는다. Pester `PcvAgentExecutionCircuitBreaker.Tests.ps1`은 수정하지 않는다.
- `AGENTS.md` generated current-evidence 블록은 편집하지 않는다. 정확한 기존 한 줄 `기본 한도는 30분, 도구 작업 묶음 18회, 정규 리뷰 1회와 제한 재검토 2회다.`를 삭제하거나 바꾸지 않는다.
- 이 계획 실행 중 Hyper-V VM, MSI, service, package, `docs/ga-ready/current-evidence.json` write를 하지 않는다.
- Lane 2 SavedOnly 재실행은 Task 1~4 PASS 뒤 별도 사용자 승인 없이 시작하지 않는다.
- public trusted signing / external stable publication을 주장하지 않는다.
- 커밋 메시지는 `docs:` / `test:` / `fix:` 접두사를 쓴다.

---

## File map

| File | Responsibility |
| --- | --- |
| `config/agent-execution-circuit-breaker.json` | 최상위 Lane 1 호환 키 + `lanes.0..3` 수치 |
| `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` | 차선별 시작 계약, Lane 2 45분/12, FAIL 프로브는 current 금지 |
| `AGENTS.md` | 회로 차단기 절에 차선 표, 작업 원칙에 식별자/승격 금지 한 줄씩. generated 블록 금지 |
| `src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs` | `ValidateCircuitBreaker` 속성 10개와 `lanes` 수치. spec SHA 재계산 |
| `config/pcv-development-policy-contract-spec-v1.json` | 바뀐 source_files 3개 SHA-256과 spec 자체 SHA를 verifier가 읽게 함 |
| `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md` | 문서 상태를 `approved-for-implementation`으로만 변경 |
| `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1` | get/delete 표시 이름, CLI 내부 `PCV_*`를 throw 선두에 |
| `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs` | GUID-only get 금지, current-evidence write 부재 |
| `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP0ActualVmSmoke.Tests.ps1` | adapter `vm-get-state`가 표시 이름과 내부 코드를 쓰게 함. Required CI 아님 |
| `src/DesktopNode.Api.Tests/DesktopNodeApiJsonReaderVmLookupTests.cs` | inventory 표시 이름만 있을 때 GUID lookup은 null |
| `docs/DOCUMENTATION_INDEX.md` | 이 계획 한 줄 |

---

### Task 1: Lane 예산을 회로 차단기 계약에 고정한다

**Files:**
- Modify: `src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs` (`ValidateCircuitBreaker`, `ExpectedSpecSha256`)
- Modify: `config/agent-execution-circuit-breaker.json`
- Modify: `docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md`
- Modify: `AGENTS.md` (generated 블록 아래 회로 차단기 절과 `## 작업 원칙`만)
- Modify: `config/pcv-development-policy-contract-spec-v1.json` (`source_files` SHA 3개)
- Modify: `docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md` (문서 상태 한 줄)
- Modify: `docs/DOCUMENTATION_INDEX.md`

**Interfaces:**
- Consumes: 기존 최상위 키 9개와 `pcv-agent-execution-circuit-breaker-v1`
- Produces: 10번째 키 `lanes` (`0`/`1`/`2`/`3` 각각 `elapsed_minutes_limit`, `tool_batch_limit`, `review_pass_limit`, `narrow_rereview_pass_limit`)

- [ ] **Step 1: Write the failing verifier assertions**

`ValidateCircuitBreaker`의 `EnumerateObject().Count() != 9`를 `!= 10`으로 바꾸고, `lanes` 수치 검사를 추가한다. JSON이 아직 9개 속성이면 이 검사가 FAIL해야 한다.

```csharp
if (root.EnumerateObject().Count() != 10 ||
    root.GetProperty("schema_version").GetInt32() != 1 ||
    root.GetProperty("contract").GetString() !=
        "pcv-agent-execution-circuit-breaker-v1" ||
    root.GetProperty("default_checkpoint_count").GetInt32() != 1 ||
    root.GetProperty("elapsed_minutes_limit").GetInt32() != 30 ||
    root.GetProperty("tool_batch_limit").GetInt32() != 18 ||
    root.GetProperty("review_pass_limit").GetInt32() != 1 ||
    root.GetProperty("narrow_rereview_pass_limit").GetInt32() != 2 ||
    root.GetProperty("same_failure_limit").GetInt32() != 3 ||
    root.GetProperty("progress_warning_percent").GetInt32() != 70)
{
    throw Invalid("circuit-breaker-contract");
}

var lanes = root.GetProperty("lanes");
if (lanes.EnumerateObject().Count() != 4 ||
    lanes.GetProperty("0").GetProperty("elapsed_minutes_limit").GetInt32() != 10 ||
    lanes.GetProperty("0").GetProperty("tool_batch_limit").GetInt32() != 6 ||
    lanes.GetProperty("0").GetProperty("review_pass_limit").GetInt32() != 0 ||
    lanes.GetProperty("0").GetProperty("narrow_rereview_pass_limit").GetInt32() != 0 ||
    lanes.GetProperty("1").GetProperty("elapsed_minutes_limit").GetInt32() != 30 ||
    lanes.GetProperty("1").GetProperty("tool_batch_limit").GetInt32() != 18 ||
    lanes.GetProperty("1").GetProperty("review_pass_limit").GetInt32() != 1 ||
    lanes.GetProperty("1").GetProperty("narrow_rereview_pass_limit").GetInt32() != 2 ||
    lanes.GetProperty("2").GetProperty("elapsed_minutes_limit").GetInt32() != 45 ||
    lanes.GetProperty("2").GetProperty("tool_batch_limit").GetInt32() != 12 ||
    lanes.GetProperty("2").GetProperty("review_pass_limit").GetInt32() != 1 ||
    lanes.GetProperty("2").GetProperty("narrow_rereview_pass_limit").GetInt32() != 0 ||
    lanes.GetProperty("3").GetProperty("elapsed_minutes_limit").GetInt32() != 30 ||
    lanes.GetProperty("3").GetProperty("tool_batch_limit").GetInt32() != 12 ||
    lanes.GetProperty("3").GetProperty("review_pass_limit").GetInt32() != 1 ||
    lanes.GetProperty("3").GetProperty("narrow_rereview_pass_limit").GetInt32() != 2)
{
    throw Invalid("circuit-breaker-lanes");
}

RequireTokens(
    policy,
    "circuit-breaker-policy",
    "30분",
    "18회",
    "21분",
    "13번째",
    "Lane 0",
    "Lane 1",
    "Lane 2",
    "Lane 3",
    "45분",
    "current_evidence_written",
    "추가 patch는 금지",
    "새 테스트도 금지",
    "Add-Type",
    "P/Invoke",
    "사용자의 명시적 승인");
RequireTokens(
    agents,
    "agents-policy-link",
    "docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md",
    "config/agent-execution-circuit-breaker.json",
    "`vague_resume_policy`: `one-bounded-checkpoint`",
    "`out_of_scope_findings`: `report-only`",
    "Lane 0",
    "FAIL 프로브는 current를 못 쓴다",
    "canonical operator id");
```

기존 `RequireTokens` 호출을 이 내용으로 교체한다. `required_literals` 배열과 `required_literal_count`는 건드리지 않는다.

- [ ] **Step 2: Run test to verify it fails**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PcvAgentExecutionCircuitBreakerContractTests --nologo
```

Expected: FAIL `PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|circuit-breaker-contract` (JSON에 `lanes` 없음).

- [ ] **Step 3: Write minimal contract and docs**

`config/agent-execution-circuit-breaker.json` 전체:

```json
{
  "schema_version": 1,
  "contract": "pcv-agent-execution-circuit-breaker-v1",
  "default_checkpoint_count": 1,
  "elapsed_minutes_limit": 30,
  "tool_batch_limit": 18,
  "review_pass_limit": 1,
  "narrow_rereview_pass_limit": 2,
  "same_failure_limit": 3,
  "progress_warning_percent": 70,
  "lanes": {
    "0": {
      "elapsed_minutes_limit": 10,
      "tool_batch_limit": 6,
      "review_pass_limit": 0,
      "narrow_rereview_pass_limit": 0
    },
    "1": {
      "elapsed_minutes_limit": 30,
      "tool_batch_limit": 18,
      "review_pass_limit": 1,
      "narrow_rereview_pass_limit": 2
    },
    "2": {
      "elapsed_minutes_limit": 45,
      "tool_batch_limit": 12,
      "review_pass_limit": 1,
      "narrow_rereview_pass_limit": 0
    },
    "3": {
      "elapsed_minutes_limit": 30,
      "tool_batch_limit": 12,
      "review_pass_limit": 1,
      "narrow_rereview_pass_limit": 2
    }
  }
}
```

`docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md` 시작 계약 절 아래에 차선 표를 추가한다. 기존 30분/18회/21분/13번째 문장은 유지한다.

```markdown
## 작업 차선

모호한 `재개`는 다음 한 checkpoint이며, 그 checkpoint는 정확히 한 차선에 속한다.

| 차선 | elapsed | tool batch | review | mutation |
| --- | ---: | ---: | --- | --- |
| Lane 0 권위 읽기 | 10분 | 6 | 0 | false |
| Lane 1 계약 | 30분 | 18 | 정규 1 + 제한 재검토 2 | false |
| Lane 2 설치본 프로브 | 45분 | 12 | 정규 1 | 사용자 명시 opt-in |
| Lane 3 승격 | 30분 | 12 | 정규 1 + 제한 재검토 2 | current-evidence만. host mutation 별도 |

Lane 1 경고는 21분과 13번째 묶음이다. Lane 2 경고는 32분과 9번째 묶음이다.
차선을 바꾸면 새 시작 계약을 공개한다. 같은 checkpoint에서 예산을 소급 확장하지 않는다.
Lane 2 `overall_verdict=FAIL` summary는 `actual_vm_tested=pass` 입력이 될 수 없다.
에이전트 종료 보고는 `lane=`, `working_authority=`, `current_evidence_written=false|true`를 포함한다.
`current_evidence_written=true`는 Lane 3가 아니면 즉시 회로를 연다.
```

`AGENTS.md` 회로 차단기 절에서 기존 `기본 한도는 30분...` 줄을 **그대로** 두고 그 아래에만 추가한다.

```markdown
- 차선: Lane 0 권위 읽기, Lane 1 계약, Lane 2 설치본 프로브, Lane 3 승격. 한 checkpoint는 한 차선만.
- FAIL 프로브는 current를 못 쓴다.
- canonical operator id는 `GET /api/v1/vms/{id}`가 받는 문자열이며 현행은 VM 표시 이름이다.
```

`## 작업 원칙` 끝에 같은 두 금지를 한 줄씩 반복한다.

스펙 파일 상태를 `approved-for-implementation`으로 바꾼다.

`docs/DOCUMENTATION_INDEX.md` 계획 목록에 이 파일 한 줄을 2026-08-25 cutover 계획 뒤에 넣는다.

- [ ] **Step 4: Refresh source SHA-256 pins**

`DevelopmentPolicyContractVerifier.Hash`와 같은 방식으로 계산한다.

```powershell
$enc = [Text.UTF8Encoding]::new($false)
function Get-PolicyTextHash([string]$Rel) {
    $path = Join-Path (Get-Location) $Rel
    $text = [IO.File]::ReadAllText($path, $enc)
    ([BitConverter]::ToString([Security.Cryptography.SHA256]::HashData($enc.GetBytes($text))) -replace '-','').ToLowerInvariant()
}
Get-PolicyTextHash 'config/agent-execution-circuit-breaker.json'
Get-PolicyTextHash 'docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md'
Get-PolicyTextHash 'AGENTS.md'
```

세 값을 `config/pcv-development-policy-contract-spec-v1.json`의 해당 `source_files[].sha256`에 넣는다. 그 다음 **spec JSON 자체** 해시를 같은 함수로 계산해 `ExpectedSpecSha256`에 넣는다.

```powershell
Get-PolicyTextHash 'config/pcv-development-policy-contract-spec-v1.json'
```

`ExpectedSpecSha256`은 `DevelopmentPolicyContractVerifier.cs`의 기존 상수와 같은 64 hex lowercase다. `required_literal_count`, `legacy_should_site_count`, Pester 파일 SHA는 변경하지 않는다.

- [ ] **Step 5: Run tests to verify they pass**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PcvAgentExecutionCircuitBreakerContractTests --nologo
```

Expected: PASS. `source-sha` 또는 `spec-sha`면 Step 4 해시를 다시 계산한다. 동일 원인으로 세 번 실패하면 회로를 열고 중단한다.

- [ ] **Step 6: Commit**

```text
git add config/agent-execution-circuit-breaker.json docs/AGENT_EXECUTION_CIRCUIT_BREAKER.md AGENTS.md src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs config/pcv-development-policy-contract-spec-v1.json docs/superpowers/specs/2026-08-27-purecvisor-desktop-node-lane-separated-development-procedure-design.md docs/DOCUMENTATION_INDEX.md
git commit -m "docs: pin lane budgets in the agent circuit breaker"
```

---

### Task 2: canonical operator id로 get/delete 하게 한다

**Files:**
- Modify: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1` (`Get-ProductVmState`, saved-lifecycle 호출, product `vm delete` 인자)
- Modify: `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs`
- Modify: `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP0ActualVmSmoke.Tests.ps1` (`invoke-cli`의 `vm-get-state`)
- Create: `src/DesktopNode.Api.Tests/DesktopNodeApiJsonReaderVmLookupTests.cs`

**Interfaces:**
- Consumes: Task 1 문서의 canonical operator id = 표시 이름. Hyper-V cleanup은 계속 `Get-VM -Id`
- Produces: `Get-ProductVmState -OperatorId $ManagedVm`; product delete `'vm', 'delete', $record.name, '--yes'`

- [ ] **Step 1: Write the failing tests**

`PcvServicePlanP0ActualVmSmokeContractTests`에 Fact를 추가한다.

```csharp
[Fact]
public void PinsCanonicalOperatorIdForProductGetAndDelete()
{
    var source = Source();
    RequireTokens(
        source,
        "Get-ProductVmState",
        "-OperatorId",
        "$ManagedVm",
        "'vm', 'get', $OperatorId",
        "'vm', 'delete', $record.name");
    Assert.DoesNotContain(
        "'vm', 'get', $Id",
        source,
        StringComparison.Ordinal);
    Assert.DoesNotContain(
        "Get-ProductVmState -Id $Record.id",
        source,
        StringComparison.Ordinal);
    Assert.DoesNotContain(
        "'vm', 'delete', $record.id",
        source,
        StringComparison.Ordinal);
}
```

`DesktopNodeApiJsonReaderVmLookupTests.cs`:

```csharp
using System.Text.Json;
using DesktopNode.Api;
using Xunit;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeApiJsonReaderVmLookupTests
{
    [Fact]
    public void FindVmMatchesDisplayNameAndRejectsUnmappedGuid()
    {
        using var document = JsonDocument.Parse(
            """[{"id":"pcv-p0-04275-behavior-managed","name":"pcv-p0-04275-behavior-managed"}]""");
        var data = document.RootElement;
        var byName = DesktopNodeApiJsonReader.FindVm(data, "pcv-p0-04275-behavior-managed");
        var byGuid = DesktopNodeApiJsonReader.FindVm(data, "b153fd4f-8adc-4835-8f72-750fe0649d19");
        Assert.NotNull(byName);
        Assert.Null(byGuid);
    }
}
```

`InternalsVisibleTo`가 `DesktopNode.Api.Tests`이므로 `internal FindVm`을 호출할 수 있다.

- [ ] **Step 2: Run tests to verify they fail**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PinsCanonicalOperatorIdForProductGetAndDelete --nologo
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter FullyQualifiedName~FindVmMatchesDisplayNameAndRejectsUnmappedGuid --nologo
```

Expected: Delivery FAIL (runner still uses `$Id` / `$record.id`). Api test는 현행 `MatchesVmId`가 표시 이름만 있으면 GUID를 못 찾으므로 **PASS할 수 있다**. Api test가 PASS면 현행 lookup 계약을 고정한 것이므로 그대로 둔다. Delivery FAIL가 이 Task의 RED다.

- [ ] **Step 3: Write minimal runner and adapter changes**

`Get-ProductVmState`를 CLI get으로만 통일한다. adapter shortcut `product-vm-state`는 제거한다.

```powershell
function Get-ProductVmState {
    param(
        [Parameter(Mandatory)][string]$OperatorId,
        [Parameter(Mandatory)][string]$Phase
    )

    $result = Invoke-PcvCliJson -StepName 'vm-get-state' -Arguments @('vm', 'get', $OperatorId)
    $data = Get-ObjectPropertyValue -InputObject $result.Json -Name 'data'
    $state = Get-ObjectPropertyValue -InputObject $data -Name 'state'
    if ($null -eq $state) {
        $state = Get-ObjectPropertyValue -InputObject $data -Name 'power_state'
    }
    return ([string]$state).ToLowerInvariant()
}
```

호출부:

```powershell
$productSaved = Get-ProductVmState -OperatorId $ManagedVm -Phase 'after-save'
...
$productStateAfterResume = Get-ProductVmState -OperatorId $ManagedVm -Phase 'after-resume'
```

product delete 두 곳(`cleanup-delete-*`, `managed-delete`)의 세 번째 인자를 `$record.name`으로 바꾼다. `Remove-VM -VM $current`와 `Get-VM -Id`는 유지한다.

adapter `invoke-cli`에 `vm-get-state` 분기를 추가한다. `vm-get-state`가 create/start job JSON을 그대로 반환하면 state readback이 빈 문자열이 된다.

```powershell
'invoke-cli' {
    $step = [string]$Payload.step
    $arguments = @($Payload.arguments)
    $state.LastEnqueuedStep = $step
    if ($step -like 'cleanup-delete-*') {
        $state.ProductDeleteVmIds.Add([string]$arguments[2]) | Out-Null
    }
    if ($step -eq 'vm-get-state') {
        $requested = [string]$arguments[2]
        if ($requested -ne 'pcv-p0-04275-behavior-managed') {
            return [pscustomobject]@{
                exit_code = 1
                stdout = (@{ error = @{ code = 'PCV_VM_NOT_FOUND' } } | ConvertTo-Json -Compress)
                stderr = ''
            }
        }
        $productState = if ($state.ManagedState -eq 'Saved') {
            $state.SaveProductState
        }
        else {
            $state.ResumeProductState
        }
        return [pscustomobject]@{
            exit_code = 0
            stdout = (@{ data = @{ name = $requested; state = $productState } } | ConvertTo-Json -Compress)
            stderr = ''
        }
    }
    # existing default job enqueue JSON
}
```

기존 SavedOnly adapter 시나리오가 표시 이름으로 get하도록, `ManagedVm = 'pcv-p0-04275-behavior-managed'` 값이 이미 `Invoke-P0BehaviorScenario`에 있다.

- [ ] **Step 4: Run tests to verify they pass**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PcvServicePlanP0ActualVmSmokeContractTests --nologo
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter FullyQualifiedName~DesktopNodeApiJsonReaderVmLookupTests --nologo
```

Expected: PASS.

선택: 로컬에서만 Pester adapter 시나리오를 돌려도 된다. Required CI 완료 증명은 위 dotnet filter다.

```text
# optional residue, not Required CI
# Invoke-Pester packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP0ActualVmSmoke.Tests.ps1
```

- [ ] **Step 5: Commit**

```text
git add packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1 src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP0ActualVmSmoke.Tests.ps1 src/DesktopNode.Api.Tests/DesktopNodeApiJsonReaderVmLookupTests.cs
git commit -m "fix: use display-name operator id for P0 get and delete"
```

---

### Task 3: CLI 내부 실패 코드를 summary.error에 보존한다

**Files:**
- Modify: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1` (`Invoke-PcvCliJson`)
- Modify: `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs`
- Modify: `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP0ActualVmSmoke.Tests.ps1`

**Interfaces:**
- Consumes: Task 2의 `vm-get-state` invoke-cli 경로와 `Get-SafeFailureCode` (메시지에서 첫 `PCV_[A-Z0-9_]+`)
- Produces: throw 문자열이 `PCV_VM_NOT_FOUND|vm-get-state|exit=1`처럼 **내부 코드가 선두**에 온다. `PCV_P0_COMMAND_FAILED`는 내부 코드가 없을 때만 쓴다.

- [ ] **Step 1: Write the failing tests**

C# 토큰:

```csharp
[Fact]
public void PinsInnerProblemCodeAheadOfGenericCommandFailure()
{
    var source = Source();
    RequireTokens(
        source,
        "function Invoke-PcvCliJson",
        "error",
        "code",
        "PCV_P0_COMMAND_FAILED");
    AssertOrdered(
        source,
        "function Invoke-PcvCliJson",
        "$cliErrorCode",
        "throw");
    Assert.Contains("PCV_P0_COMMAND_FAILED", source, StringComparison.Ordinal);
}
```

Pester behavior (adapter, host mutation 없음):

```powershell
It 'preserves PCV_VM_NOT_FOUND from vm get instead of PCV_P0_COMMAND_FAILED' {
    $run = Invoke-P0BehaviorScenario -Name 'get-not-found' -Configure {
        param($state)
        $state.VmGetNotFound = $true
    }
    $run.Summary.overall_verdict | Should -Be 'FAIL'
    $run.Summary.error | Should -Be 'PCV_VM_NOT_FOUND'
    $run.Summary.error | Should -Not -Be 'PCV_P0_COMMAND_FAILED'
    $run.Summary.queued_jobs.'vm-save'.status | Should -Be 'succeeded'
    $run.Summary.slice_verdicts.saved_lifecycle | Should -Be 'FAIL'
    $run.Summary.cleanup.verdict | Should -Be 'PASS'
}
```

`New-P0BehaviorRuntime`의 `$state` 초기값에 `VmGetNotFound = $false`를 추가한다. Step 1에서는 이 키가 없으므로 시나리오가 다른 오류로 실패하거나 error가 `PCV_P0_COMMAND_FAILED`다. 그것이 RED다.

- [ ] **Step 2: Run the C# token test to verify it fails**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PinsInnerProblemCodeAheadOfGenericCommandFailure --nologo
```

Expected: FAIL (`$cliErrorCode` 없음).

- [ ] **Step 3: Write minimal CLI error extraction**

`Invoke-PcvCliJson`에서 `$payload`를 만든 뒤, 실패 throw 직전:

```powershell
function Get-CliProblemCode {
    param($Payload)
    foreach ($candidate in @(
        (Get-ObjectPropertyValue -InputObject (Get-ObjectPropertyValue -InputObject $Payload -Name 'error') -Name 'code'),
        (Get-ObjectPropertyValue -InputObject $Payload -Name 'code')
    )) {
        if ([string]$candidate -match '^PCV_[A-Z0-9_]+$') {
            return [string]$candidate
        }
    }
    return $null
}

# inside Invoke-PcvCliJson, replace the generic throw:
if ($exitCode -ne 0 -and -not $AllowFailure.IsPresent) {
    $cliErrorCode = Get-CliProblemCode -Payload $payload
    if ([string]::IsNullOrWhiteSpace($cliErrorCode)) {
        $cliErrorCode = 'PCV_P0_COMMAND_FAILED'
    }
    throw "$cliErrorCode|$StepName|exit=$exitCode"
}
```

adapter `vm-get-state`에서 `$state.VmGetNotFound`이면:

```powershell
if ($step -eq 'vm-get-state' -and $state.VmGetNotFound) {
    return [pscustomobject]@{
        exit_code = 1
        stdout = (@{ error = @{ code = 'PCV_VM_NOT_FOUND' } } | ConvertTo-Json -Compress)
        stderr = ''
    }
}
```

`Get-SafeFailureCode`는 첫 `PCV_*`를 고르므로 선두가 `PCV_VM_NOT_FOUND`면 summary `error`가 그 값이 된다.

- [ ] **Step 4: Run tests to verify they pass**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PcvServicePlanP0ActualVmSmokeContractTests --nologo
```

Expected: PASS.

Pester behavior는 선택 로컬 확인이다. 실패해도 Required CI 범위가 아니면 Task 3을 문서 계약 PASS로 닫지 말고 adapter 분기를 고친다. 이 시나리오는 host mutation이 없다.

- [ ] **Step 5: Commit**

```text
git add packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1 src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP0ActualVmSmoke.Tests.ps1
git commit -m "fix: preserve inner PCV codes in P0 runner summaries"
```

---

### Task 4: FAIL 프로브가 current를 못 쓰게 계약을 고정한다

**Files:**
- Modify: `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs`
- Modify: `src/DesktopNode.Delivery.Tests/Delivery/Evidence/PcvFeatureEvidencePromotionContractTests.cs` (새 ordinal 없이 기존 004 호출을 이름 있는 Fact로 한 줄 더 노출)

**Interfaces:**
- Consumes: `D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 4)`와 04274 P0 fail fixture
- Produces: runner 소스가 `docs/ga-ready/current-evidence.json`과 `Update-PcvCurrentEvidence`를 포함하지 않음

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void Lane2RunnerDoesNotWriteCurrentEvidence()
{
    var source = Source();
    Assert.DoesNotContain(
        "docs/ga-ready/current-evidence.json",
        source,
        StringComparison.Ordinal);
    Assert.DoesNotContain(
        "Update-PcvCurrentEvidence",
        source,
        StringComparison.Ordinal);
}

[Fact]
public void Lane2FailObservationDoesNotMakePromotionEligible()
{
    D2EvidenceContractVerifier.Verify("feature-evidence-promotion", 4);
}
```

두 번째 Fact는 이미 004가 같은 검증을 하므로 추가 즉시 PASS할 수 있다. 첫 Fact가 RED다. runner가 경로 문자열을 주석으로도 가지면 FAIL이므로 주석에 current-evidence 경로를 쓰지 않는다.

`PcvFeatureEvidencePromotionContractTests.cs`에 동일 이름의 위 두 번째 Fact를 넣지 말고, ManualAdmin 테스트에서 `D2EvidenceContractVerifier`를 호출한다. 같은 어셈블리다.

- [ ] **Step 2: Run tests**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~Lane2RunnerDoesNotWriteCurrentEvidence --nologo
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~Lane2FailObservationDoesNotMakePromotionEligible --nologo
```

Expected: 첫 테스트는 runner에 해당 문자열이 없으면 이미 PASS. 있으면 FAIL이므로 문자열을 제거한다. 둘째는 기존 004와 같으면 PASS. 둘 다 PASS가 이 Task의 GREEN이다. 새 promotion ordinal을 만들지 않는다.

- [ ] **Step 3: Write minimal implementation**

runner에 current-evidence 경로가 있으면 제거한다. 없어야 정상이다. 구현이 없으면 테스트만 남긴다.

- [ ] **Step 4: Run the broader delivery slice**

Run:

```text
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PcvServicePlanP0ActualVmSmokeContractTests --nologo
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PcvFeatureEvidencePromotionContractTests --nologo
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter FullyQualifiedName~PcvAgentExecutionCircuitBreakerContractTests --nologo
```

Expected: PASS.

- [ ] **Step 5: Commit**

```text
git add src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP0ActualVmSmokeContractTests.cs packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP0ActualVmSmoke.ps1
git commit -m "test: reject current-evidence writes from the P0 runner"
```

---

## Out of plan (do not execute)

Lane 2 SavedOnly 재실행, 04275 package/fullgate, `current-evidence.json` 승격, inventory `Id` GUID 통일, `pcvverify campaign-tooling` dual-hash는 이 계획의 체크박스가 아니다. Task 1~4 PASS 뒤 사용자가 명시적으로 Lane 2를 열 때만 새 시작 계약으로 실행한다.

---

## Self-review

1. **Spec coverage:** §3.1~3.6 차선/권위/식별자/예산/FAIL 승격/최소 가드 → Task 1~4. §4 Lane 2 실행 → out of plan. §5 파일 지도 → File map. §6 오류 코드 → Task 3. §7 비목표 → Global Constraints. §8 성공 기준 1·2·3·4·5·6 → Task 1/2/4 테스트와 mutation 금지. §9 슬라이스 5 → Out of plan.
2. **Placeholders:** TBD/TODO 없음. 해시 값은 실행 시 재계산한다. 계산 함수는 verifier와 동일하다.
3. **Types:** `lanes.0..3` 키, `Get-ProductVmState -OperatorId`, `$cliErrorCode`가 Task 2/3에서 같은 이름이다.
