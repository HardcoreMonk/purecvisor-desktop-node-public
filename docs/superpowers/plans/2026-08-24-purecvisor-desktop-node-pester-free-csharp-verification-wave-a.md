# Pester-free C# Verification Wave A Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 비관리자 개발 검증의 단일 C# 진입점, versioned suite catalog, lane/path planner, shell-free process boundary, 결정론적 JSON summary를 구축하되 후속 migration 전에는 plan-only 이외의 완료 주장을 fail-closed로 차단한다.

**Architecture:** `DesktopNode.Verification`은 저장소 경계에서 catalog와 CLI 요청을 검증하고 `VerificationPlanner`가 effective lane과 suite 집합을 계산한다. 실제 child process는 argument-array 기반 `IProcessRunner` 뒤에 격리하고, Wave A catalog의 `activation_state=plan-only-foundation`이 application 진입점의 비-plan-only 실행을 거부한다. `DesktopNode.Verification.Tests`는 policy, process, timeout/cancel, redaction, atomic summary를 가짜 port와 소수의 안전한 실제 프로세스로 검증한다.

**Tech Stack:** C# / .NET 10, `System.Text.Json`, `System.Diagnostics.Process`, xUnit 2.9.3, JSON Schema draft 2020-12, Visual Studio 2026 또는 `dotnet` CLI.

## Global constraints

- Source design: `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-design.md`
- Written-spec approval: `2026-08-24 user-approved`
- Scope: Wave A only. Web, Installer, Packaging/evidence migration과 required CI cutover는 각각 Wave B~E 계획이 소유한다.
- Existing required workflow `.github/workflows/development-gates.yml`은 이 계획에서 수정하지 않는다.
- Existing PowerShell selector/runner와 Pester 62개 파일은 삭제하거나 required/non-required 상태를 바꾸지 않는다.
- 제품 project/API/WMI provider, MSI, Service/SCM, firewall, Event Log, trust store, Credential Manager, Hyper-V/VM을 참조하거나 실행하지 않는다.
- ADR-0009 Guest PowerShell Direct transport를 변경하지 않는다.
- `DesktopNode.Verification`의 product `ProjectReference` 수는 `0`이다.
- Wave A catalog는 `activation_state=plan-only-foundation`이다. application의 비-plan-only 요청은 child process 시작 전에 `PCV_VERIFY_CONFIG_INVALID`로 종료한다.
- `--plan-only` summary의 `ok=true`는 plan/catalog 검증 성공만 뜻한다. Fast/Full/Release 실행 PASS 또는 Pester-free cutover evidence가 아니다.
- Artifact는 repository `artifacts/<run-id>/` 또는 `RUNNER_TEMP`의 하위 디렉터리에만 쓴다. repository root, `artifacts/` 자체, drive root, user profile은 거부한다.
- 새 문서 본문은 한국어를 사용하고 contract/suite/error identifier는 원문을 유지한다.
- 모든 구현 task는 RED → GREEN → commit 순서를 지킨다. 테스트를 Skip 처리하지 않는다.
- Change tier는 `M`, verification lane은 기존 required gates의 `Full`이다. Host mutation과 package campaign은 열지 않는다.

## Baseline and completion boundary

- Base: `main` / `bee07214cd4f2f061b30996f766b9976a9527abd`
- Existing `.NET Release`: 7 test assemblies, `967/967` PASS, skipped `0`.
- Existing Pester inventory: Packaging 55 + Installer 6 + Web 1 = 62 files / 20,166 lines.
- Existing required jobs: `dotnet-tests`, `web-tests`, `packaging-pester`, `installer-web-pester`.
- Wave A is complete only when the new project tests pass, Full/M plan-only produces seven ordered `planned` rows, an execution attempt is blocked before `IProcessRunner`, legacy CI files are unchanged, and the code-level evidence says `host_mutation_performed=false`.
- Required CI Pester `0` and required non-admin PowerShell `0` remain Wave E completion conditions and are not claimed here.

---

## File map

| File | Responsibility |
| --- | --- |
| `src/DesktopNode.Verification/DesktopNode.Verification.csproj` | Cross-platform .NET 10 console; no product reference; `pcvverify` assembly. |
| `src/DesktopNode.Verification/Program.cs` | Thin async entrypoint and exit code forwarding. |
| `src/DesktopNode.Verification/VerificationContracts.cs` | Enums, request/plan/result records, error codes and `VerificationException`. |
| `src/DesktopNode.Verification/VerificationCatalog.cs` | JSON DTO, loader, schema/catalog invariant validation, exact suite/shard order. |
| `src/DesktopNode.Verification/VerificationFileSystem.cs` | Replaceable filesystem port and physical adapter shared by catalog/summary owners. |
| `src/DesktopNode.Verification/VerificationOptions.cs` | `verify` CLI parsing and duplicate/mutual-exclusion checks. |
| `src/DesktopNode.Verification/RepositoryPaths.cs` | Repository discovery and artifact-root containment. |
| `src/DesktopNode.Verification/VerificationPolicy.cs` | Change-tier classification, lane promotion, Fast selection, shard/partial planning. |
| `src/DesktopNode.Verification/VerificationProcess.cs` | Executable/argument guard, `ProcessStartInfo`, bounded/redacted output, timeout/cancel/tree kill. |
| `src/DesktopNode.Verification/VerificationExecutor.cs` | Plan-only projection and bounded-parallel execution result aggregation. |
| `src/DesktopNode.Verification/VerificationSummaryWriter.cs` | v2 JSON serialization and same-directory temporary-file atomic rename. |
| `src/DesktopNode.Verification/VerificationApplication.cs` | Parse → locate → validate → plan → execute/project → summary orchestration. |
| `src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj` | xUnit owner for the Wave A runner. |
| `src/DesktopNode.Verification.Tests/VerificationProjectContractTests.cs` | Target framework, assembly, reference, solution inclusion guard. |
| `src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs` | Canonical catalog/request/plan/report fakes shared by focused runner tests. |
| `src/DesktopNode.Verification.Tests/VerificationCatalogTests.cs` | Schema, seven suites, four shards, allowlist, forbidden command validation. |
| `src/DesktopNode.Verification.Tests/VerificationOptionsTests.cs` | CLI grammar and fail-closed input cases. |
| `src/DesktopNode.Verification.Tests/RepositoryPathsTests.cs` | Worktree discovery and safe artifact containment. |
| `src/DesktopNode.Verification.Tests/VerificationPolicyTests.cs` | Tier/lane/path matrix and shard/partial scope. |
| `src/DesktopNode.Verification.Tests/VerificationProcessTests.cs` | Argument fidelity, allowlist, redaction/cap, safe success, timeout/tree kill. |
| `src/DesktopNode.Verification.Tests/VerificationExecutorTests.cs` | Zero-call plan-only, max concurrency 4, ordering, failure/timeout/cancel aggregation. |
| `src/DesktopNode.Verification.Tests/VerificationSummaryWriterTests.cs` | Exact v2 shape, deterministic order, atomic operation sequence and cleanup. |
| `src/DesktopNode.Verification.Tests/VerificationApplicationTests.cs` | Plan-only end-to-end and activation lock proof. |
| `config/development-verification-suites.json` | Versioned seven-suite/four-shard catalog in plan-only foundation state. |
| `config/development-verification-suites.schema.json` | Strict catalog JSON Schema. |
| `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-design.md` | Written-spec approval state; no design-body change. |
| `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-wave-a.md` | This approved Wave A execution plan. |
| `docs/DEVELOPMENT_VERIFICATION_POLICY.md` | Wave A preview command and non-cutover boundary. |
| `docs/DEVELOPER_INDEX.md` | Design, plan and evidence entrypoints. |
| `docs/ga-ready/EVIDENCE_INDEX.md` | Wave A code-level evidence locator. |
| `docs/ga-ready/evidence/pester-free-csharp-verification-wave-a-foundation-2026-08-24.md` | Commands, counts, summary hash and non-mutation/non-promotion claims. |

## Interface ledger

```csharp
internal enum VerificationLane { Fast, Full, Release }
internal enum ChangeTier { S, M, L }
internal enum ExecutionScope { Lane, Shard, Partial }
internal enum SuiteStatus { Planned, Passed, Failed, Missing, TimedOut, Cancelled }

internal sealed record VerificationRequest(
    VerificationLane RequestedLane,
    ChangeTier RequestedChangeTier,
    IReadOnlyList<string> ChangedPaths,
    string ArtifactRoot,
    IReadOnlyList<string> SuiteIds,
    string? ShardId,
    bool PlanOnly);

internal sealed record VerificationPlan(
    VerificationRequest Request,
    VerificationLane EffectiveLane,
    ChangeTier EffectiveChangeTier,
    IReadOnlyList<string> TierReasons,
    string? PromotionReason,
    ExecutionScope ExecutionScope,
    string? ShardId,
    bool ReleasePreflight,
    IReadOnlyList<SuiteDefinition> Suites);

internal sealed record ProcessInvocation(
    string SuiteId,
    string FileName,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory,
    TimeSpan Timeout,
    IReadOnlyList<string> AllowedExecutables,
    int OutputLimitCharacters = 8192);

internal sealed record ProcessExecutionResult(
    int? ExitCode,
    long DurationMs,
    bool TimedOut,
    bool Cancelled,
    string StandardOutput,
    string StandardError,
    string OutputSha256);

internal interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(ProcessInvocation invocation, CancellationToken cancellationToken);
}

internal interface IVerificationClock
{
    DateTimeOffset UtcNow { get; }
}

internal interface IVerificationFileSystem
{
    string ReadAllText(string path);
    void CreateDirectory(string path);
    Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken);
    bool FileExists(string path);
    void MoveFile(string source, string destination, bool overwrite);
    void DeleteFile(string path);
}
```

The names above are fixed for every task below. Later tasks must not introduce aliases such as `VerificationRunPlan`, `ICommandRunner`, or `SuiteResultStatus`.

---

### Task 1: Add the isolated .NET 10 console and test projects

**Files:**
- Create: `src/DesktopNode.Verification/DesktopNode.Verification.csproj`
- Create: `src/DesktopNode.Verification/Program.cs`
- Create: `src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj`
- Create: `src/DesktopNode.Verification.Tests/VerificationProjectContractTests.cs`
- Modify: `src/DesktopNode.sln`

- [ ] **Step 1: Create the test project and failing project-contract test**

Create `DesktopNode.Verification.Tests.csproj` first without a production `ProjectReference`, so the RED command runs the test rather than failing to locate a test project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
```

Create `VerificationProjectContractTests.cs` with this complete test. It deliberately fails before the production project and both solution entries exist.

```csharp
using System.Xml.Linq;

namespace DesktopNode.Verification.Tests;

public sealed class VerificationProjectContractTests
{
    [Fact]
    public void VerificationProjectIsNet10ConsoleWithoutProductReferences()
    {
        var root = FindRepositoryRoot();
        var projectPath = Path.Combine(root, "src", "DesktopNode.Verification", "DesktopNode.Verification.csproj");
        var project = XDocument.Load(projectPath);

        Assert.Equal("Exe", project.Descendants("OutputType").Single().Value);
        Assert.Equal("net10.0", project.Descendants("TargetFramework").Single().Value);
        Assert.Equal("pcvverify", project.Descendants("AssemblyName").Single().Value);
        Assert.Empty(project.Descendants("ProjectReference"));
    }

    [Fact]
    public void SolutionContainsVerificationAndItsTestsExactlyOnce()
    {
        var solution = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "DesktopNode.sln"));

        Assert.Equal(1, Count(solution, "DesktopNode.Verification\\DesktopNode.Verification.csproj"));
        Assert.Equal(1, Count(solution, "DesktopNode.Verification.Tests\\DesktopNode.Verification.Tests.csproj"));
    }

    private static int Count(string value, string needle) =>
        value.Split(needle, StringSplitOptions.None).Length - 1;

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.sln")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("PCV_VERIFY_CONFIG_INVALID|repository-root-not-found");
    }
}
```

- [ ] **Step 2: Run the focused test and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationProjectContractTests --nologo`

Expected: the test command runs and both assertions FAIL because the production project and its two solution entries do not exist yet.

- [ ] **Step 3: Create the production project and fail-closed bootstrap entrypoint**

Create `DesktopNode.Verification.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>pcvverify</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <InternalsVisibleTo Include="DesktopNode.Verification.Tests" />
  </ItemGroup>
</Project>
```

Create the temporary, concrete fail-closed `Program.cs`; Task 8 replaces its body after the application exists:

```csharp
namespace DesktopNode.Verification;

public static class Program
{
    public static int Main(string[] args)
    {
        Console.Error.WriteLine("PCV_VERIFY_CONFIG_INVALID|activation_state=foundation-bootstrap");
        return 2;
    }
}
```

- [ ] **Step 4: Add the production reference to the test project**

```powershell
dotnet add src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj reference src/DesktopNode.Verification/DesktopNode.Verification.csproj
```

Expected project delta:

```xml
<ItemGroup>
  <ProjectReference Include="..\DesktopNode.Verification\DesktopNode.Verification.csproj" />
</ItemGroup>
```

- [ ] **Step 5: Add both projects to the solution and verify GREEN**

Run:

```powershell
dotnet sln src/DesktopNode.sln add src/DesktopNode.Verification/DesktopNode.Verification.csproj
dotnet sln src/DesktopNode.sln add src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationProjectContractTests --nologo
```

Expected: `Passed: 2`, `Failed: 0`, `Skipped: 0`.

- [ ] **Step 6: Commit**

```powershell
git add src/DesktopNode.sln src/DesktopNode.Verification src/DesktopNode.Verification.Tests
git commit -m "build: scaffold C# verification runner"
```

---

### Task 2: Define and strictly validate the versioned seven-suite catalog

**Files:**
- Create: `config/development-verification-suites.schema.json`
- Create: `config/development-verification-suites.json`
- Create: `src/DesktopNode.Verification/VerificationContracts.cs`
- Create: `src/DesktopNode.Verification/VerificationCatalog.cs`
- Create: `src/DesktopNode.Verification/VerificationFileSystem.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationCatalogTests.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs`

**Catalog invariants:**

- Contract is `pcv-development-verification-suite-catalog-v1`, schema version is integer `1`, activation is exactly `plan-only-foundation`.
- Suite order is exactly `dotnet`, `web-typecheck`, `web-parity`, `delivery-contracts`, `installer-contracts`, `evidence-check`, `policy-boundaries`.
- Shards are exactly `dotnet`, `web`, `delivery`, `installer-policy`; their union contains all seven suite IDs exactly once.
- Process executable allowlist, after `Path.GetFileName(...).ToLowerInvariant()`, is exactly `dotnet`, `dotnet.exe`, `node`, `node.exe`, `npm`, `npm.cmd`, `git`, `git.exe`.
- A process executor requires `file_name` and may have an argument array; a managed executor requires `managed_handler` and forbids `file_name`/arguments.
- `pwsh`, `powershell`, `Invoke-Pester` in an executable or argument throws `PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN`.
- `msiexec`, `sc.exe`, `New-VM`, `Start-VM`, `Stop-VM`, `Start-Service`, `Stop-Service`, `Install-Module`, `AllowHostMutation` in catalog commands throws `PCV_VERIFY_CONFIG_INVALID`.
- Each timeout is `1..overall_timeout_seconds`; overall timeout is positive; max parallelism is exactly `4`.
- Duplicate suite IDs, duplicate shard IDs, unknown shard members, missing/duplicated union members, unknown JSON properties, wrong `$schema`, missing schema file, wrong schema `$id` all fail before planning.

- [ ] **Step 1: Write catalog RED tests**

Create `VerificationCatalogTests.cs` with these cases and a helper that copies the canonical JSON into its own GUID-named temporary directory, mutates it through `JsonNode`, and calls `new VerificationCatalogLoader(fileSystem).Load(catalogPath, schemaPath)`:

```csharp
[Fact]
public void CanonicalCatalogHasSevenSuitesAndFourDisjointShards()
{
    var catalog = LoadCanonical();

    Assert.Equal("pcv-development-verification-suite-catalog-v1", catalog.Contract);
    Assert.Equal("plan-only-foundation", catalog.ActivationState);
    Assert.Equal(4, catalog.MaxParallelism);
    Assert.Equal([
        "dotnet", "web-typecheck", "web-parity", "delivery-contracts",
        "installer-contracts", "evidence-check", "policy-boundaries"
    ], catalog.Suites.Select(suite => suite.Id));
    Assert.Equal(["dotnet", "web", "delivery", "installer-policy"], catalog.Shards.Select(shard => shard.Id));
    Assert.Equal(7, catalog.Shards.SelectMany(shard => shard.SuiteIds).Distinct(StringComparer.Ordinal).Count());
}

[Theory]
[InlineData("pwsh", "-NoProfile", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
[InlineData("dotnet", "Invoke-Pester", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
[InlineData("msiexec", "/i", "PCV_VERIFY_CONFIG_INVALID")]
[InlineData("dotnet", "AllowHostMutation", "PCV_VERIFY_CONFIG_INVALID")]
public void CatalogRejectsForbiddenExecutableOrArgument(string fileName, string argument, string code)
{
    var exception = Assert.Throws<VerificationException>(() => LoadMutated(root =>
    {
        var executor = root["suites"]![0]!["executor"]!.AsObject();
        executor["file_name"] = fileName;
        executor["arguments"] = new JsonArray(argument);
    }));

    Assert.Equal(code, exception.Code);
}

[Fact]
public void CatalogRejectsDuplicateOrIncompleteShardUnion()
{
    var exception = Assert.Throws<VerificationException>(() => LoadMutated(root =>
        root["shards"]![1]!["suite_ids"] = new JsonArray("dotnet", "web-parity")));

    Assert.Equal("PCV_VERIFY_CONFIG_INVALID", exception.Code);
    Assert.Contains("shard-union", exception.Detail, StringComparison.Ordinal);
}

[Fact]
public void CatalogRejectsUnknownJsonProperty()
{
    var exception = Assert.Throws<VerificationException>(() => LoadMutated(root =>
        root["unexpected"] = true));

    Assert.Equal("PCV_VERIFY_CONFIG_INVALID", exception.Code);
    Assert.Contains("catalog-json", exception.Detail, StringComparison.Ordinal);
}
```

The helper uses `JsonNode.Parse`, `JsonSerializerOptions.WriteIndented=true`, and deletes its temporary directory in `Dispose`; it must not mutate repository config. Put reusable `FindRepositoryRoot`, `AllowedExecutables`, `Canonical`, `LoadCanonical`, and `LoadMutated` members in `VerificationTestFixtures.cs` under the single static owner `VerificationCatalogFixture`; later tasks extend this file with the explicitly named fixtures they consume.

- [ ] **Step 2: Run the focused tests and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationCatalogTests --nologo`

Expected: FAIL because the catalog types and files do not exist.

- [ ] **Step 3: Add exact error and catalog contracts**

Create `VerificationContracts.cs` with the enums/records from the Interface ledger and these error definitions:

```csharp
namespace DesktopNode.Verification;

internal static class VerificationErrorCodes
{
    internal const string ConfigInvalid = "PCV_VERIFY_CONFIG_INVALID";
    internal const string UnknownSuite = "PCV_VERIFY_UNKNOWN_SUITE";
    internal const string ProcessFailed = "PCV_VERIFY_PROCESS_FAILED";
    internal const string Timeout = "PCV_VERIFY_TIMEOUT";
    internal const string Cancelled = "PCV_VERIFY_CANCELLED";
    internal const string ParityUnmapped = "PCV_VERIFY_PARITY_UNMAPPED";
    internal const string NonAdminPowerShellForbidden = "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN";
    internal const string ArtifactRootInvalid = "PCV_VERIFY_ARTIFACT_ROOT_INVALID";
}

internal sealed class VerificationException(string code, string detail)
    : Exception($"{code}|{detail}")
{
    internal string Code { get; } = code;
    internal string Detail { get; } = detail;
}
```

Add the remaining Interface-ledger types exactly once in the same namespace. Add these catalog records in `VerificationCatalog.cs`:

```csharp
internal sealed record VerificationCatalog(
    int SchemaVersion,
    string Contract,
    string ActivationState,
    int MaxParallelism,
    int OverallTimeoutSeconds,
    IReadOnlyList<string> AllowedExecutables,
    IReadOnlyList<SuiteDefinition> Suites,
    IReadOnlyList<ShardDefinition> Shards);

internal sealed record SuiteDefinition(
    string Id,
    string Owner,
    string MigrationState,
    string ExecutorKind,
    string? FileName,
    IReadOnlyList<string> Arguments,
    string? ManagedHandler,
    int TimeoutSeconds);

internal sealed record ShardDefinition(string Id, IReadOnlyList<string> SuiteIds);
```

Use private JSON DTO records with `[JsonPropertyName]` for `$schema`, snake_case fields and nested executor fields. Deserialize with `PropertyNameCaseInsensitive=false` and `UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow`. Wrap `JsonException`, `IOException`, and null-root errors as `PCV_VERIFY_CONFIG_INVALID|catalog-json=<reason>` without embedding file contents.

Create `VerificationFileSystem.cs` with `PhysicalVerificationFileSystem : IVerificationFileSystem`; every method is a one-line wrapper over `File`/`Directory`, and `WriteAllTextAsync` uses `new UTF8Encoding(false)`. Construct the catalog loader as `new VerificationCatalogLoader(fileSystem)` so catalog tests may substitute the same port used by the summary writer.

- [ ] **Step 4: Add the strict schema**

Create a draft 2020-12 schema with `$id=pcv-development-verification-suite-catalog-schema-v1`, top-level `additionalProperties:false`, and required fields:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "pcv-development-verification-suite-catalog-schema-v1",
  "type": "object",
  "additionalProperties": false,
  "required": [
    "$schema", "schema_version", "contract", "activation_state",
    "max_parallelism", "overall_timeout_seconds", "allowed_executables", "suites", "shards"
  ],
  "properties": {
    "$schema": { "const": "./development-verification-suites.schema.json" },
    "schema_version": { "const": 1 },
    "contract": { "const": "pcv-development-verification-suite-catalog-v1" },
    "activation_state": { "const": "plan-only-foundation" },
    "max_parallelism": { "const": 4 },
    "overall_timeout_seconds": { "type": "integer", "minimum": 1, "maximum": 3600 },
    "allowed_executables": {
      "type": "array",
      "minItems": 8,
      "maxItems": 8,
      "uniqueItems": true,
      "items": { "enum": ["dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"] }
    },
    "suites": {
      "type": "array",
      "minItems": 7,
      "maxItems": 7,
      "items": { "$ref": "#/$defs/suite" }
    },
    "shards": {
      "type": "array",
      "minItems": 4,
      "maxItems": 4,
      "items": { "$ref": "#/$defs/shard" }
    }
  },
  "$defs": {
    "suite": {
      "type": "object",
      "additionalProperties": false,
      "required": ["id", "owner", "migration_state", "timeout_seconds", "executor"],
      "properties": {
        "id": { "type": "string", "minLength": 1 },
        "owner": { "enum": ["csharp", "node"] },
        "migration_state": {
          "enum": ["native-existing", "wave-a-foundation", "wave-b-pending", "wave-c-pending", "wave-d-pending"]
        },
        "timeout_seconds": { "type": "integer", "minimum": 1, "maximum": 3600 },
        "executor": { "$ref": "#/$defs/executor" }
      }
    },
    "executor": {
      "oneOf": [
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["kind", "file_name", "arguments"],
          "properties": {
            "kind": { "const": "process" },
            "file_name": { "type": "string", "minLength": 1 },
            "arguments": { "type": "array", "items": { "type": "string" } }
          }
        },
        {
          "type": "object",
          "additionalProperties": false,
          "required": ["kind", "managed_handler"],
          "properties": {
            "kind": { "const": "managed" },
            "managed_handler": { "enum": ["current-evidence-check", "policy-boundaries"] }
          }
        }
      ]
    },
    "shard": {
      "type": "object",
      "additionalProperties": false,
      "required": ["id", "suite_ids"],
      "properties": {
        "id": { "type": "string", "minLength": 1 },
        "suite_ids": {
          "type": "array",
          "minItems": 1,
          "uniqueItems": true,
          "items": { "type": "string", "minLength": 1 }
        }
      }
    }
  }
}
```

- [ ] **Step 5: Add the canonical catalog**

Create `development-verification-suites.json` with no PowerShell command:

```json
{
  "$schema": "./development-verification-suites.schema.json",
  "schema_version": 1,
  "contract": "pcv-development-verification-suite-catalog-v1",
  "activation_state": "plan-only-foundation",
  "max_parallelism": 4,
  "overall_timeout_seconds": 1200,
  "allowed_executables": ["dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"],
  "suites": [
    {
      "id": "dotnet",
      "owner": "csharp",
      "migration_state": "native-existing",
      "timeout_seconds": 900,
      "executor": {
        "kind": "process",
        "file_name": "dotnet",
        "arguments": ["test", "src/DesktopNode.sln", "-c", "Release", "--nologo"]
      }
    },
    {
      "id": "web-typecheck",
      "owner": "node",
      "migration_state": "native-existing",
      "timeout_seconds": 600,
      "executor": {
        "kind": "process",
        "file_name": "npm",
        "arguments": ["test", "--prefix", "web"]
      }
    },
    {
      "id": "web-parity",
      "owner": "node",
      "migration_state": "wave-b-pending",
      "timeout_seconds": 600,
      "executor": {
        "kind": "process",
        "file_name": "npm",
        "arguments": ["run", "verify:parity", "--prefix", "web"]
      }
    },
    {
      "id": "delivery-contracts",
      "owner": "csharp",
      "migration_state": "wave-d-pending",
      "timeout_seconds": 900,
      "executor": {
        "kind": "process",
        "file_name": "dotnet",
        "arguments": ["test", "src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj", "-c", "Release", "--filter", "Category=Delivery", "--nologo"]
      }
    },
    {
      "id": "installer-contracts",
      "owner": "csharp",
      "migration_state": "wave-c-pending",
      "timeout_seconds": 900,
      "executor": {
        "kind": "process",
        "file_name": "dotnet",
        "arguments": ["test", "src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj", "-c", "Release", "--filter", "Category=Installer", "--nologo"]
      }
    },
    {
      "id": "evidence-check",
      "owner": "csharp",
      "migration_state": "wave-d-pending",
      "timeout_seconds": 300,
      "executor": { "kind": "managed", "managed_handler": "current-evidence-check" }
    },
    {
      "id": "policy-boundaries",
      "owner": "csharp",
      "migration_state": "wave-a-foundation",
      "timeout_seconds": 300,
      "executor": { "kind": "managed", "managed_handler": "policy-boundaries" }
    }
  ],
  "shards": [
    { "id": "dotnet", "suite_ids": ["dotnet"] },
    { "id": "web", "suite_ids": ["web-typecheck", "web-parity"] },
    { "id": "delivery", "suite_ids": ["delivery-contracts", "evidence-check"] },
    { "id": "installer-policy", "suite_ids": ["installer-contracts", "policy-boundaries"] }
  ]
}
```

- [ ] **Step 6: Implement loader validation with deterministic failure details**

`new VerificationCatalogLoader(fileSystem).Load(catalogPath, schemaPath)` must perform these checks in order and throw the exact detail prefix shown:

1. schema file exists and root `$id` matches → `schema-file` / `schema-id`.
2. catalog JSON deserializes with no extra field → `catalog-json`.
3. `$schema`, version, contract, activation, parallelism, overall timeout → `catalog-header`.
4. allowed set equals the eight normalized names → `executable-allowlist`.
5. suite order, uniqueness, timeout, executor union → `suite-definition:<id>`.
6. command guard → the specific PowerShell code or `catalog-command-forbidden:<id>`.
7. shard order, uniqueness, known members and exact disjoint union → `shard-union`.

Return immutable arrays (`Array.AsReadOnly`) so tests cannot mutate the loaded catalog.

- [ ] **Step 7: Re-run catalog tests**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationCatalogTests --nologo`

Expected: all catalog tests PASS, failed `0`, skipped `0`.

- [ ] **Step 8: Commit**

```powershell
git add config/development-verification-suites.json config/development-verification-suites.schema.json src/DesktopNode.Verification/VerificationContracts.cs src/DesktopNode.Verification/VerificationCatalog.cs src/DesktopNode.Verification/VerificationFileSystem.cs src/DesktopNode.Verification.Tests/VerificationCatalogTests.cs src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs
git commit -m "feat(verification): add versioned suite catalog"
```

---

### Task 3: Parse the approved CLI grammar and contain repository/artifact paths

**Files:**
- Create: `src/DesktopNode.Verification/VerificationOptions.cs`
- Create: `src/DesktopNode.Verification/RepositoryPaths.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationOptionsTests.cs`
- Create: `src/DesktopNode.Verification.Tests/RepositoryPathsTests.cs`

**CLI contract:**

```text
pcvverify verify
  --lane Fast|Full|Release
  --change-tier S|M|L
  --changed-path <repository-relative-path>...
  --artifact-root <path>
  [--suite <id>...]
  [--shard dotnet|web|delivery|installer-policy]
  [--plan-only]
```

`--lane`, `--change-tier`, `--artifact-root` and at least one `--changed-path` are required exactly once. `--suite` may repeat but the same ID may not repeat. `--shard` may occur once. `--suite` and `--shard` are mutually exclusive. Unknown commands/options, inline `--name=value`, empty values, rooted changed paths and `..` traversal are configuration errors. Changed paths normalize `\` to `/` and collapse an initial `./`; ordinal duplicate paths collapse to one entry while preserving first-seen order.

- [ ] **Step 1: Write CLI RED tests**

Create `VerificationOptionsTests.cs`:

```csharp
namespace DesktopNode.Verification.Tests;

public sealed class VerificationOptionsTests
{
    [Fact]
    public void ParsesRepeatedPathsAndSuitesWithoutShellExpansion()
    {
        var request = VerificationOptions.Parse([
            "verify", "--lane", "Full", "--change-tier", "M",
            "--changed-path", "src/DesktopNode.Api/Program.cs",
            "--changed-path", @"web\src\app.ts",
            "--artifact-root", "artifacts/verification-wave-a",
            "--suite", "dotnet", "--suite", "web-typecheck", "--plan-only"
        ]);

        Assert.Equal(VerificationLane.Full, request.RequestedLane);
        Assert.Equal(ChangeTier.M, request.RequestedChangeTier);
        Assert.Equal(["src/DesktopNode.Api/Program.cs", "web/src/app.ts"], request.ChangedPaths);
        Assert.Equal(["dotnet", "web-typecheck"], request.SuiteIds);
        Assert.Null(request.ShardId);
        Assert.True(request.PlanOnly);
    }

    [Theory]
    [InlineData("--suite-and-shard")]
    [InlineData("--duplicate-suite")]
    [InlineData("--missing-changed-path")]
    [InlineData("--rooted-changed-path")]
    [InlineData("--traversal-changed-path")]
    [InlineData("--unknown-option")]
    public void RejectsInvalidGrammarBeforePlanning(string invalidCase)
    {
        var args = InvalidArguments.For(invalidCase);
        var exception = Assert.Throws<VerificationException>(() => VerificationOptions.Parse(args));

        Assert.Equal("PCV_VERIFY_CONFIG_INVALID", exception.Code);
        Assert.StartsWith("cli:", exception.Detail, StringComparison.Ordinal);
    }
}
```

Define `InvalidArguments.For` in the same file as a private static method returning these exact mutations of the valid base command:

| Case | Mutation |
| --- | --- |
| `--suite-and-shard` | Append `--suite dotnet --shard web`. |
| `--duplicate-suite` | Append `--suite dotnet --suite dotnet`. |
| `--missing-changed-path` | Remove both `--changed-path` and its value. |
| `--rooted-changed-path` | Use `D:\outside\file.cs`. |
| `--traversal-changed-path` | Use `../outside/file.cs`. |
| `--unknown-option` | Append `--base-ref main`. |

- [ ] **Step 2: Write path-boundary RED tests**

Create `RepositoryPathsTests.cs` with temporary directories only:

```csharp
[Fact]
public void FindsWorktreeRootBySolutionAndCatalogAnchors()
{
    using var tree = TemporaryRepository.Create();
    var nested = Directory.CreateDirectory(Path.Combine(tree.Root, "src", "nested", "deeper")).FullName;

    Assert.Equal(tree.Root, RepositoryLocator.Find(nested));
}

[Fact]
public void AcceptsOnlyStrictArtifactsOrRunnerTempDescendants()
{
    using var tree = TemporaryRepository.Create();
    var runnerTemp = Directory.CreateDirectory(Path.Combine(tree.Parent, "runner-temp")).FullName;

    Assert.Equal(
        Path.Combine(tree.Root, "artifacts", "wave-a"),
        ArtifactRootPolicy.ResolveAndValidate(tree.Root, "artifacts/wave-a", runnerTemp, tree.UserProfile));
    Assert.Equal(
        Path.Combine(runnerTemp, "wave-a"),
        ArtifactRootPolicy.ResolveAndValidate(tree.Root, Path.Combine(runnerTemp, "wave-a"), runnerTemp, tree.UserProfile));
}

[Theory]
[InlineData(".")]
[InlineData("artifacts")]
[InlineData("../outside")]
[InlineData("%UNRESOLVED%/wave-a")]
[InlineData("~/wave-a")]
public void RejectsBroadOrUnresolvedArtifactRoots(string candidate)
{
    using var tree = TemporaryRepository.Create();
    var exception = Assert.Throws<VerificationException>(() =>
        ArtifactRootPolicy.ResolveAndValidate(tree.Root, candidate, null, tree.UserProfile));

    Assert.Equal("PCV_VERIFY_ARTIFACT_ROOT_INVALID", exception.Code);
}
```

`TemporaryRepository.Create` writes empty `src/DesktopNode.sln` and `config/development-verification-suites.json` anchors under a GUID-named temp directory, exposes a separate fake `UserProfile`, and recursively deletes only its own resolved root in `Dispose`.

- [ ] **Step 3: Run focused tests and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter "FullyQualifiedName~VerificationOptionsTests|FullyQualifiedName~RepositoryPathsTests" --nologo`

Expected: FAIL because parser and path owners do not exist.

- [ ] **Step 4: Implement the parser as a single-pass argument-array parser**

`VerificationOptions.Parse(IReadOnlyList<string> args)` must:

1. Require `args[0] == "verify"` using ordinal comparison.
2. Consume one following array element for every value option; never concatenate a command string.
3. Parse lane/tier case-insensitively but serialize canonical enum names.
4. Reject a second singleton option with `PCV_VERIFY_CONFIG_INVALID|cli:duplicate-option=<name>`.
5. Reject duplicate suite IDs with `cli:duplicate-suite=<id>`.
6. Reject `--suite` plus `--shard` with `cli:suite-and-shard-mutually-exclusive`.
7. Normalize changed paths and reject `Path.IsPathRooted`, empty segments resolving to parent traversal, and any normalized path equal to `..` or starting `../`.
8. Return the Interface-ledger `VerificationRequest`; do not read Git or environment variables.

Use this value reader so a following option is never consumed as data:

```csharp
private static string RequiredValue(IReadOnlyList<string> args, ref int index, string option)
{
    if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
    {
        throw new VerificationException(VerificationErrorCodes.ConfigInvalid, $"cli:missing-value={option}");
    }

    var value = args[++index];
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new VerificationException(VerificationErrorCodes.ConfigInvalid, $"cli:empty-value={option}");
    }

    return value;
}
```

- [ ] **Step 5: Implement repository discovery and strict descendant checks**

Use these signatures:

```csharp
internal static class RepositoryLocator
{
    internal static string Find(string startDirectory);
}

internal static class ArtifactRootPolicy
{
    internal static string ResolveAndValidate(
        string repositoryRoot,
        string requestedRoot,
        string? runnerTemp,
        string? userProfile);
}
```

`RepositoryLocator.Find` walks parents and requires both `src/DesktopNode.sln` and `config/development-verification-suites.json`; reaching the filesystem root throws `PCV_VERIFY_CONFIG_INVALID|repository-root-not-found`.

`ResolveAndValidate` performs `Path.GetFullPath(requestedRoot, repositoryRoot)`, rejects strings containing `%`, `$(`, `${`, or starting `~`, rejects drive/filesystem root and repository root, then uses this strict containment rule:

```csharp
private static bool IsStrictDescendant(string candidate, string parent)
{
    var relative = Path.GetRelativePath(Path.GetFullPath(parent), Path.GetFullPath(candidate));
    return relative != "." &&
        !Path.IsPathRooted(relative) &&
        relative != ".." &&
        !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
        !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);
}
```

Allow only strict descendants of `<repo>/artifacts` or a non-empty `runnerTemp`. Independently reject a candidate equal to or below `userProfile`. Every rejection uses `PCV_VERIFY_ARTIFACT_ROOT_INVALID` and a non-secret reason token; do not include an environment dump.

- [ ] **Step 6: Re-run focused tests**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter "FullyQualifiedName~VerificationOptionsTests|FullyQualifiedName~RepositoryPathsTests" --nologo`

Expected: all parser/path tests PASS with skipped `0`.

- [ ] **Step 7: Commit**

```powershell
git add src/DesktopNode.Verification/VerificationOptions.cs src/DesktopNode.Verification/RepositoryPaths.cs src/DesktopNode.Verification.Tests/VerificationOptionsTests.cs src/DesktopNode.Verification.Tests/RepositoryPathsTests.cs
git commit -m "feat(verification): validate CLI and artifact boundaries"
```

---

### Task 4: Port tier/lane/path selection into a deterministic planner

**Files:**
- Create: `src/DesktopNode.Verification/VerificationPolicy.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationPolicyTests.cs`
- Modify: `src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs`

**Tier rules:**

| Minimum | Normalized path rule | Reason |
| --- | --- | --- |
| `L` | `packaging/windows-desktop-node/installer/**` | `installer-lifecycle` |
| `L` | Packaging tool/test name contains `HostMutation`, `OsMutation`, `FullAdminHostMutation` | `host-mutation-boundary` |
| `L` | `docs/adr/0003-*`, `0009-*`, `0010-*`, or security/credential/token/tls/trust policy | `security-policy-boundary` |
| `L` | canonical current evidence, evidence indexes/ledger/control plane, verification policy, packaging README, `AGENTS.md` | `current-evidence-anchor` |
| `L` | public release boundary/matrix, ADR-0005, release/publish workflow | `public-release-boundary` |
| `L` | path contains sign/signing/signed/publication/publish | `signing-publication-boundary` |
| `M` | `src/DesktopNode.Api/**`, `src/DesktopNode.Cli/**`, Web api/contract/client/auth source/test | `api-cli-web-contract` |
| `M` | any `packaging/windows-desktop-node/**` | `packaging-contract` |
| `M` | verifier projects, the two new config files, this design/plan, or `development-gates.yml` | `development-verification-boundary` |
| `M` | more than one of `src:<project>`, `web`, `packaging` domains changes | `cross-module-change` |

Reasons retain first-seen rule order and appear once. A requested higher tier never demotes. `L` forces Release; `Fast+M` forces Full. An unclassified path keeps its derived tier but forces Full with `promotion_reason=unknown-change-scope`.

**Fast suite mapping before any promotion:**

| Path | Suite IDs |
| --- | --- |
| `src/**`, `*.sln`, `*.csproj` | `dotnet` |
| `web/**` | `web-typecheck`, `web-parity` |
| installer subtree | `installer-contracts` |
| general packaging subtree | `delivery-contracts` |
| canonical current-evidence anchors | `evidence-check` |
| ordinary docs | `policy-boundaries` |
| unclassified | promote to Full and select all seven |

- [ ] **Step 1: Write the RED matrix tests**

Create `VerificationPolicyTests.cs` with at least these exact assertions:

```csharp
[Theory]
[InlineData("Fast", "S", "src/DesktopNode.Runtime/Internal.cs", "Fast", "S", "dotnet")]
[InlineData("Fast", "S", "src/DesktopNode.Api/Program.cs", "Full", "M", "dotnet,web-typecheck,web-parity,delivery-contracts,installer-contracts,evidence-check,policy-boundaries")]
[InlineData("Fast", "S", "packaging/windows-desktop-node/installer/Product.wxs", "Release", "L", "dotnet,web-typecheck,web-parity,delivery-contracts,installer-contracts,evidence-check,policy-boundaries")]
[InlineData("Fast", "S", "unclassified/new.txt", "Full", "S", "dotnet,web-typecheck,web-parity,delivery-contracts,installer-contracts,evidence-check,policy-boundaries")]
public void ResolvesLaneTierAndOrderedSuites(
    string lane, string tier, string path, string expectedLane, string expectedTier, string expectedSuites)
{
    var catalog = VerificationCatalogFixture.Canonical();
    var request = VerificationRequestFixture.Create(lane, tier, [path]);

    var plan = VerificationPlanner.Create(request, catalog);

    Assert.Equal(expectedLane, plan.EffectiveLane.ToString());
    Assert.Equal(expectedTier, plan.EffectiveChangeTier.ToString());
    Assert.Equal(expectedSuites.Split(','), plan.Suites.Select(suite => suite.Id));
}

[Fact]
public void SuiteSelectionIsPartialAndSortedByCatalogOrder()
{
    var request = VerificationRequestFixture.Create("Full", "M", ["src/a.cs"]) with
    {
        SuiteIds = ["policy-boundaries", "dotnet"]
    };

    var plan = VerificationPlanner.Create(request, VerificationCatalogFixture.Canonical());

    Assert.Equal(ExecutionScope.Partial, plan.ExecutionScope);
    Assert.Null(plan.ShardId);
    Assert.Equal(["dotnet", "policy-boundaries"], plan.Suites.Select(suite => suite.Id));
}

[Theory]
[InlineData("dotnet", "dotnet")]
[InlineData("web", "web-typecheck,web-parity")]
[InlineData("delivery", "delivery-contracts,evidence-check")]
[InlineData("installer-policy", "installer-contracts,policy-boundaries")]
public void UsesOnlyCatalogDefinedShards(string shard, string expected)
{
    var request = VerificationRequestFixture.Create("Full", "M", [".github/workflows/development-gates.yml"]) with { ShardId = shard };
    var plan = VerificationPlanner.Create(request, VerificationCatalogFixture.Canonical());

    Assert.Equal(ExecutionScope.Shard, plan.ExecutionScope);
    Assert.Equal(shard, plan.ShardId);
    Assert.Equal(expected.Split(','), plan.Suites.Select(suite => suite.Id));
}
```

Also assert: Full has all seven; Release has all seven and `ReleasePreflight=true`; unknown suite throws `PCV_VERIFY_UNKNOWN_SUITE`; duplicate suite and suite+shard are rejected defensively; cross-module change is M; reasons are unique and ordered; `--suite` cannot turn `ExecutionScope` into Lane.

- [ ] **Step 2: Run policy tests and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationPolicyTests --nologo`

Expected: FAIL because `VerificationPlanner` does not exist.

- [ ] **Step 3: Implement normalized classification and lane promotion**

Create these internal owners:

```csharp
internal sealed record TierResolution(
    ChangeTier RequestedTier,
    ChangeTier EffectiveTier,
    IReadOnlyList<string> Reasons);

internal static class ChangeTierPolicy
{
    internal static TierResolution Resolve(ChangeTier requestedTier, IReadOnlyList<string> changedPaths);
}

internal static class VerificationPlanner
{
    internal static VerificationPlan Create(VerificationRequest request, VerificationCatalog catalog);
}
```

Extend `VerificationTestFixtures.cs` with `VerificationRequestFixture.Create(string lane, string tier, IReadOnlyList<string> paths)`. It parses the two enums, uses `artifacts/test-run` as the artifact root, empty suite IDs, null shard and `PlanOnly=true`; each test changes fields through record `with` expressions.

Use compiled, culture-invariant regular expressions corresponding exactly to the table. Rank tiers with a function returning S=1, M=2, L=3; add a reason through one helper that both deduplicates and raises the effective tier. Do not infer L for unknown paths.

- [ ] **Step 4: Implement scope selection in this fixed order**

1. Validate suite/shard mutual exclusion and requested suite uniqueness again.
2. Resolve tier and effective lane.
3. If `SuiteIds.Count>0`, look up every ID, throw `PCV_VERIFY_UNKNOWN_SUITE|suite=<id>` when absent, sort by catalog order and set Partial.
4. Else if `ShardId` exists, use the exact catalog shard order and set Shard.
5. Else if effective lane is Full/Release, use all suites and set Lane.
6. Else compute the Fast union. If a path is unknown or no suite is selected, promote to Full, set `unknown-change-scope`, and use all suites.
7. Set `ReleasePreflight` only when effective lane is Release.

Promotion reason precedence is `tier-l-requires-release`, then `tier-m-requires-full`, then `unknown-change-scope`; unknown scope does not overwrite a tier promotion.

- [ ] **Step 5: Re-run policy tests**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationPolicyTests --nologo`

Expected: all policy tests PASS with skipped `0`.

- [ ] **Step 6: Commit**

```powershell
git add src/DesktopNode.Verification/VerificationPolicy.cs src/DesktopNode.Verification.Tests/VerificationPolicyTests.cs src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs
git commit -m "feat(verification): port lane and path planning"
```

---

### Task 5: Build the shell-free, bounded and redacted process boundary

**Files:**
- Create: `src/DesktopNode.Verification/VerificationProcess.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationProcessTests.cs`

**Process terminal contract (defined in Task 2 and implemented here):**

```csharp
internal sealed record ProcessExecutionResult(
    int? ExitCode,
    long DurationMs,
    bool TimedOut,
    bool Cancelled,
    string StandardOutput,
    string StandardError,
    string OutputSha256);
```

`TimedOut` and `Cancelled` cannot both be true. Timeout/cancel results use `ExitCode=null`; an ordinary nonzero process preserves its exit code. `OutputSha256` is lowercase SHA-256 over `UTF8(redacted_stdout + "\n" + redacted_stderr)` after truncation. Output fields are capped independently at 8,192 characters and append `...[truncated]` within that cap.

- [ ] **Step 1: Write process-boundary RED tests**

Create these cases in `VerificationProcessTests.cs`:

```csharp
[Fact]
public void BuildsArgumentListWithoutACommandShell()
{
    var invocation = new ProcessInvocation(
        "dotnet", "dotnet", ["test", @"path with spaces\project.csproj", "--filter", "Name=a;b"],
        VerificationCatalogFixture.FindRepositoryRoot(), TimeSpan.FromSeconds(5),
        VerificationCatalogFixture.AllowedExecutables);

    var startInfo = ProcessStartInfoFactory.Create(invocation);

    Assert.False(startInfo.UseShellExecute);
    Assert.True(startInfo.RedirectStandardOutput);
    Assert.True(startInfo.RedirectStandardError);
    Assert.True(startInfo.CreateNoWindow);
    Assert.Equal(invocation.Arguments, startInfo.ArgumentList);
    Assert.Null(startInfo.Environment["PCV_TOKEN"]);
}

[Fact]
public void ResolvesNpmCmdOnlyOnWindows()
{
    Assert.Equal(
        OperatingSystem.IsWindows() ? "npm.cmd" : "npm",
        ProcessExecutableResolver.Resolve("npm"));
}

// Also add a real SystemProcessRunner `npm --version` test. On Windows it must
// exercise the shell-free node.exe + npm-cli.js launch described in Step 4;
// checking the catalog/platform string npm.cmd alone is insufficient.

[Theory]
[InlineData("pwsh", "-NoProfile", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
[InlineData("powershell.exe", "-File", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
[InlineData("dotnet", "Invoke-Pester", "PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN")]
[InlineData("cmd.exe", "/c", "PCV_VERIFY_CONFIG_INVALID")]
[InlineData("msiexec.exe", "/i", "PCV_VERIFY_CONFIG_INVALID")]
public void RejectsShellPowerShellAndMutationCommands(string fileName, string argument, string code)
{
    var exception = Assert.Throws<VerificationException>(() =>
        ProcessCommandGuard.Validate(fileName, [argument], VerificationCatalogFixture.AllowedExecutables));

    Assert.Equal(code, exception.Code);
}

[Fact]
public void RedactsSecretsAndCapsEachStream()
{
    var text = "Authorization: Bearer top-secret\n--token raw-token\n\"password\":\"pw\"\n" + new string('x', 9000);

    var sanitized = ProcessOutputSanitizer.Sanitize(text, @"D:\repo", 8192);

    Assert.DoesNotContain("top-secret", sanitized, StringComparison.Ordinal);
    Assert.DoesNotContain("raw-token", sanitized, StringComparison.Ordinal);
    Assert.DoesNotContain("\"pw\"", sanitized, StringComparison.Ordinal);
    Assert.Contains("[REDACTED]", sanitized, StringComparison.Ordinal);
    Assert.Equal(8192, sanitized.Length);
    Assert.EndsWith("...[truncated]", sanitized, StringComparison.Ordinal);
}

[Fact]
public async Task RunsSafeDotnetProcessAndPreservesTerminalResult()
{
    var runner = new SystemProcessRunner();
    var result = await runner.RunAsync(
        new ProcessInvocation(
            "fixture", "dotnet", ["--version"], VerificationCatalogFixture.FindRepositoryRoot(),
            TimeSpan.FromSeconds(30), VerificationCatalogFixture.AllowedExecutables),
        CancellationToken.None);

    Assert.Equal(0, result.ExitCode);
    Assert.False(result.TimedOut);
    Assert.False(result.Cancelled);
    Assert.NotEmpty(result.OutputSha256);
}

[Fact]
public async Task TimeoutKillsTheEntireProcessTreeAndReturnsTimedOut()
{
    var runner = new SystemProcessRunner();
    var result = await runner.RunAsync(
        new ProcessInvocation(
            "fixture", "dotnet", ["--info"], VerificationCatalogFixture.FindRepositoryRoot(),
            TimeSpan.FromMilliseconds(1), VerificationCatalogFixture.AllowedExecutables),
        CancellationToken.None);

    Assert.Null(result.ExitCode);
    Assert.True(result.TimedOut);
    Assert.False(result.Cancelled);
}
```

Add a separate already-cancelled-token test that returns `Cancelled=true` without starting a process. The timeout test is the only timing-sensitive case; retry is forbidden. If `dotnet --info` ever completes inside 1 ms, replace it with `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --no-restore --filter __pcv_no_test__`, still without a shell.

- [ ] **Step 2: Run process tests and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationProcessTests --nologo`

Expected: FAIL because process types do not exist.

- [ ] **Step 3: Implement defense-in-depth command validation**

`ProcessCommandGuard.Validate(fileName, arguments, allowedExecutables)` normalizes only `Path.GetFileName(fileName)` and rejects paths whose normalized name is not in the catalog set. It scans the executable and each individual argument, case-insensitively, for these token sets:

```csharp
private static readonly string[] PowerShellTokens =
    ["pwsh", "pwsh.exe", "powershell", "powershell.exe", "invoke-pester"];

private static readonly string[] MutationTokens =
    ["msiexec", "msiexec.exe", "sc.exe", "new-vm", "start-vm", "stop-vm",
     "start-service", "stop-service", "install-module", "allowhostmutation"];
```

Match complete executable names and delimited argument tokens; do not reject a benign substring inside a longer identifier. PowerShell matches use `PCV_VERIFY_NONADMIN_PWSH_FORBIDDEN|command=<normalized-name>`; mutation/unallowlisted matches use `PCV_VERIFY_CONFIG_INVALID|process-command-forbidden=<normalized-name>`.

- [ ] **Step 4: Implement exact ProcessStartInfo construction**

`ProcessExecutableResolver.Resolve` maps catalog `npm` to `npm.cmd` only when
`OperatingSystem.IsWindows()` and otherwise returns the catalog name unchanged. That
return value is the platform catalog-name resolution contract, not permission to launch a
Windows command script through a hidden shell. A real RED run established that
`npm.cmd` cannot be executed with `UseShellExecute=false`.

For non-Windows-`npm` commands, `ProcessStartInfoFactory.Create` uses the validated
resolved executable as `FileName` and adds each caller argument to `ArgumentList` in
order. For Windows `npm`, it resolves `npm.cmd` from `PATH` only to locate the adjacent
trusted `node.exe` and `node_modules/npm/bin/npm-cli.js`; `FileName` is the validated Node
executable, the CLI script is the first `ArgumentList` entry, and every caller argument
follows unchanged and in order. Discovery failure is a fixed, non-secret process failure.
Neither route may use `cmd.exe`, `ComSpec`, `Arguments`, `bash`,
`ProcessStartInfo.Verb`, or another shell fallback.

Both routes set `WorkingDirectory`, `UseShellExecute=false`,
`RedirectStandardOutput=true`, `RedirectStandardError=true`,
`RedirectStandardInput=false`, and `CreateNoWindow=true`. They clear inherited
environment keys whose names contain `TOKEN`, `PASSWORD`, `SECRET`, `CREDENTIAL`, or
equal `AUTHORIZATION`, using ordinal-ignore-case comparison. Validate the catalog,
platform-resolved, and effective executable names through the sealed allowlist.

- [ ] **Step 5: Implement bounded capture, redaction and hashing**

Read stdout and stderr concurrently. Retain at most 65,536 characters per stream while draining the remainder so the child cannot deadlock. Use `ProcessInvocation.WorkingDirectory` as the validated repository-root redaction anchor. Redact its occurrences to `[REPO_ROOT]`, bearer headers, `--token/--password/--secret` following values, and JSON property values whose key contains token/password/secret/credential. Then cap to the invocation limit, appending the truncation suffix inside the cap. Compute the digest from the two redacted/capped strings and never serialize the pre-redaction buffers.

- [ ] **Step 6: Implement timeout/cancel and tree cleanup**

`SystemProcessRunner.RunAsync` follows this terminal sequence:

1. If the caller token is already cancelled, return Cancelled without creating `Process`.
2. Validate command, create process, start stopwatch, start both output drains, and call `WaitForExitAsync(linkedToken)` where the linked source has `CancelAfter(invocation.Timeout)`.
3. On `OperationCanceledException`, classify caller cancellation before timeout, call `process.Kill(entireProcessTree:true)` when still running, await an uncancelled `WaitForExitAsync`, then await both drains.
4. Return exactly one terminal state and stop/dispose the process in `finally`.
5. Process start/read failures throw `PCV_VERIFY_PROCESS_FAILED|suite=<suite-id>;reason=<exception-type>` without copying exception data or command arguments.

- [ ] **Step 7: Re-run process tests**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationProcessTests --nologo`

Expected: safe process PASS, timeout/cancel PASS, all redaction/guard tests PASS, skipped `0`.

- [ ] **Step 8: Commit**

```powershell
git add src/DesktopNode.Verification/VerificationProcess.cs src/DesktopNode.Verification.Tests/VerificationProcessTests.cs
git commit -m "feat(verification): add safe process boundary"
```

---

### Task 6: Aggregate suites with bounded parallelism and one terminal result each

**Files:**
- Create: `src/DesktopNode.Verification/VerificationExecutor.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationExecutorTests.cs`
- Modify: `src/DesktopNode.Verification/VerificationContracts.cs`
- Modify: `src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs`

**Execution records:**

```csharp
internal sealed record SuiteExecutionRecord(
    string SuiteId,
    SuiteStatus Status,
    string MigrationState,
    int? ExitCode,
    long DurationMs,
    bool TimedOut,
    bool Cancelled,
    string? StandardOutput,
    string? StandardError,
    string? OutputSha256,
    string? ErrorCode);

internal sealed record VerificationExecutionReport(
    long DurationMs,
    IReadOnlyList<SuiteExecutionRecord> Results);

internal interface IManagedSuiteRunner
{
    Task<SuiteExecutionRecord> RunAsync(
        SuiteDefinition suite,
        string repositoryRoot,
        CancellationToken cancellationToken);
}
```

Wave A production uses `UnavailableManagedSuiteRunner`, which returns `Missing` plus `PCV_VERIFY_PARITY_UNMAPPED`; the application activation lock prevents this runner from being reached. Later Waves replace handlers individually. Plan-only always returns `Planned` and calls neither process nor managed runner.

- [ ] **Step 1: Write executor RED tests with recording fakes**

Create `VerificationExecutorTests.cs` with `RecordingProcessRunner` and `RecordingManagedSuiteRunner`. The process fake increments an atomic current-concurrency counter, updates maximum, awaits a supplied gate/delay, decrements in `finally`, and returns a configured `ProcessExecutionResult`.

```csharp
[Fact]
public async Task PlanOnlyReturnsOrderedPlannedRowsWithoutCallingAnyRunner()
{
    var process = new RecordingProcessRunner(failIfCalled: true);
    var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
    var executor = new VerificationExecutor(process, managed);
    var plan = VerificationPlanFixture.Full(planOnly: true);

    var report = await executor.ExecuteAsync(
        plan, VerificationCatalogFixture.Canonical(), VerificationCatalogFixture.FindRepositoryRoot(), CancellationToken.None);

    Assert.Equal(7, report.Results.Count);
    Assert.All(report.Results, result => Assert.Equal(SuiteStatus.Planned, result.Status));
    Assert.Equal(plan.Suites.Select(suite => suite.Id), report.Results.Select(result => result.SuiteId));
    Assert.Equal(0, process.CallCount);
    Assert.Equal(0, managed.CallCount);
}

[Fact]
public async Task RunsAtMostFourSuitesAndRestoresCatalogOrder()
{
    var process = new RecordingProcessRunner(delay: TimeSpan.FromMilliseconds(40));
    var executor = new VerificationExecutor(process, new RecordingManagedSuiteRunner());
    var catalog = VerificationCatalogFixture.SevenProcessSuites(maxParallelism: 4);
    var plan = VerificationPlanFixture.ForCatalog(catalog, planOnly: false);

    var report = await executor.ExecuteAsync(plan, catalog, VerificationCatalogFixture.FindRepositoryRoot(), CancellationToken.None);

    Assert.Equal(4, process.MaximumConcurrency);
    Assert.Equal(catalog.Suites.Select(suite => suite.Id), report.Results.Select(result => result.SuiteId));
    Assert.All(report.Results, result => Assert.Equal(SuiteStatus.Passed, result.Status));
}

[Theory]
[InlineData(7, false, false, "Failed", "PCV_VERIFY_PROCESS_FAILED")]
[InlineData(null, true, false, "TimedOut", "PCV_VERIFY_TIMEOUT")]
[InlineData(null, false, true, "Cancelled", "PCV_VERIFY_CANCELLED")]
public async Task MapsEveryTerminalProcessStateWithoutPassCompression(
    int? exitCode, bool timedOut, bool cancelled, string expectedStatus, string expectedCode)
{
    var process = new RecordingProcessRunner(result: new ProcessExecutionResult(
        exitCode, 12, timedOut, cancelled, "", "", new string('0', 64)));
    var catalog = VerificationCatalogFixture.OneProcessSuite();

    var report = await new VerificationExecutor(process, new RecordingManagedSuiteRunner()).ExecuteAsync(
        VerificationPlanFixture.ForCatalog(catalog, planOnly: false), catalog, VerificationCatalogFixture.FindRepositoryRoot(), CancellationToken.None);

    Assert.Equal(expectedStatus, report.Results.Single().Status.ToString());
    Assert.Equal(expectedCode, report.Results.Single().ErrorCode);
}
```

Add tests for managed `Missing`, caller cancellation, overall timeout cancellation, a thrown process exception becoming Failed, and `max_parallelism` never exceeding four even when a test fixture supplies a larger value.

Extend `VerificationTestFixtures.cs` with `VerificationPlanFixture.Full(bool planOnly)` and `ForCatalog(VerificationCatalog catalog, bool planOnly)`, plus the two recording runner classes. `ForCatalog` constructs a Lane/Full/M plan whose suite array is the catalog array and whose request differs only in the supplied PlanOnly flag. Recording fakes expose `CallCount` and configured terminal results; only the process fake exposes `MaximumConcurrency`.

- [ ] **Step 2: Run executor tests and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationExecutorTests --nologo`

Expected: FAIL because executor records and owner do not exist.

- [ ] **Step 3: Add the execution records and fail-closed managed runner**

Add the records/interfaces above to `VerificationContracts.cs`. Implement:

```csharp
internal sealed class UnavailableManagedSuiteRunner : IManagedSuiteRunner
{
    public Task<SuiteExecutionRecord> RunAsync(
        SuiteDefinition suite,
        string repositoryRoot,
        CancellationToken cancellationToken) => Task.FromResult(new SuiteExecutionRecord(
            suite.Id,
            SuiteStatus.Missing,
            suite.MigrationState,
            null,
            0,
            false,
            false,
            null,
            null,
            null,
            VerificationErrorCodes.ParityUnmapped));
}
```

- [ ] **Step 4: Implement plan-only projection before any semaphore or runner**

`VerificationExecutor.ExecuteAsync(plan, catalog, repositoryRoot, token)` checks `plan.Request.PlanOnly` first. It maps selected suites to `Planned`, zero duration, null outputs/error and returns immediately. This branch must not create a `ProcessInvocation`, semaphore, linked cancellation source, or call either runner.

- [ ] **Step 5: Implement actual bounded execution for unit-level foundation coverage**

For non-plan-only unit calls:

1. Create a linked overall timeout source from `catalog.OverallTimeoutSeconds`.
2. Create `SemaphoreSlim(Math.Min(catalog.MaxParallelism, 4))`.
3. Start one task per selected suite; acquire/release the semaphore in `try/finally`.
4. Process suites build `ProcessInvocation` directly from catalog arrays, including `catalog.AllowedExecutables`, and call `IProcessRunner`.
5. Managed suites call `IManagedSuiteRunner`.
6. Map process states in priority order: caller token cancellation → Cancelled; overall timeout while caller token is not cancelled → TimedOut; runner TimedOut; runner Cancelled; exit 0 Passed; other exit Failed.
7. Catch `VerificationException`/other exceptions per suite and return Failed with only an approved error code; never lose a result row.
8. Await all tasks, reorder through the selected suite index, and return the report.

Do not stop sibling suites merely because one process exits nonzero. Only caller cancellation or overall timeout cancels siblings; this preserves independent failure diagnostics.

- [ ] **Step 6: Re-run executor tests**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationExecutorTests --nologo`

Expected: all executor tests PASS, observed maximum concurrency exactly `4`, skipped `0`.

- [ ] **Step 7: Commit**

```powershell
git add src/DesktopNode.Verification/VerificationContracts.cs src/DesktopNode.Verification/VerificationExecutor.cs src/DesktopNode.Verification.Tests/VerificationExecutorTests.cs src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs
git commit -m "feat(verification): add bounded suite executor"
```

---

### Task 7: Serialize the v2 result contract and commit it atomically

**Files:**
- Create: `src/DesktopNode.Verification/VerificationSummaryWriter.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationSummaryWriterTests.cs`
- Modify: `src/DesktopNode.Verification/VerificationContracts.cs`
- Modify: `src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs`

**JSON root order:** `schema_version`, `contract`, `requested_lane`, `effective_lane`, `requested_change_tier`, `change_tier`, `tier_reasons`, `promotion_reason`, `execution_scope`, `shard_id`, `plan_only`, `catalog_activation_state`, `ok`, `error_code`, `started_at`, `completed_at`, `duration_ms`, `results`.

Null optional fields are omitted, not emitted as JSON null. Results remain in catalog order. Timestamps use UTC round-trip format. `execution_scope` is `lane|shard|partial`; status mapping is exactly `Planned→planned`, `Passed→passed`, `Failed→failed`, `Missing→missing`, `TimedOut→timed_out`, `Cancelled→cancelled`. A plan-only report is `ok=true` only when every selected row is Planned. An actual report is `ok=true` only when every selected row is Passed; Missing/Failed/TimedOut/Cancelled can never be summarized as PASS.

- [ ] **Step 1: Write exact-shape and atomic-write RED tests**

Create `VerificationSummaryWriterTests.cs`:

```csharp
[Fact]
public void PlanOnlySummaryHasDeterministicContractAndOrdering()
{
    var started = DateTimeOffset.Parse("2026-08-24T00:00:00Z");
    var plan = VerificationPlanFixture.Full(planOnly: true);
    var report = VerificationReportFixture.Planned(plan.Suites);

    var summary = VerificationSummaryFactory.Create(
        plan, VerificationCatalogFixture.Canonical(), report, started, started);
    var json = VerificationJson.Serialize(summary);

    using var document = JsonDocument.Parse(json);
    var root = document.RootElement;
    Assert.Equal(2, root.GetProperty("schema_version").GetInt32());
    Assert.Equal("pcv-development-verification-summary-v2", root.GetProperty("contract").GetString());
    Assert.Equal("Full", root.GetProperty("requested_lane").GetString());
    Assert.Equal("Full", root.GetProperty("effective_lane").GetString());
    Assert.Equal("lane", root.GetProperty("execution_scope").GetString());
    Assert.True(root.GetProperty("plan_only").GetBoolean());
    Assert.True(root.GetProperty("ok").GetBoolean());
    Assert.Equal("plan-only-foundation", root.GetProperty("catalog_activation_state").GetString());
    Assert.Equal(7, root.GetProperty("results").GetArrayLength());
    Assert.All(root.GetProperty("results").EnumerateArray(), row =>
        Assert.Equal("planned", row.GetProperty("status").GetString()));
    Assert.DoesNotContain("Authorization", json, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public async Task WriterUsesSameDirectoryTemporaryFileThenAtomicMove()
{
    var fileSystem = new RecordingVerificationFileSystem();
    var writer = new AtomicVerificationSummaryWriter(fileSystem);
    var root = Path.Combine("D:\\repo", "artifacts", "wave-a");

    var path = await writer.WriteAsync(root, VerificationSummaryFixture.Success(), CancellationToken.None);

    Assert.Equal(Path.Combine(root, "summary.json"), path);
    Assert.Equal(["create-directory", "write-temp", "move-overwrite"], fileSystem.Operations);
    Assert.StartsWith(root + Path.DirectorySeparatorChar, fileSystem.TempPath, StringComparison.OrdinalIgnoreCase);
    Assert.False(fileSystem.FileExists(fileSystem.TempPath));
}

[Fact]
public async Task WriterDeletesTemporaryFileWhenMoveFails()
{
    var fileSystem = new RecordingVerificationFileSystem(failMove: true);
    var writer = new AtomicVerificationSummaryWriter(fileSystem);

    await Assert.ThrowsAsync<IOException>(() => writer.WriteAsync(
        Path.Combine("D:\\repo", "artifacts", "wave-a"), VerificationSummaryFixture.Success(), CancellationToken.None));

    Assert.Contains("delete-temp", fileSystem.Operations);
}
```

Also compare two plan-only serializations built with the same injected timestamps byte-for-byte, assert Partial and Shard cannot serialize as Lane, and assert Missing/timeout/cancel each makes `ok=false` with the matching root `error_code` chosen by first catalog-order failure.

Extend `VerificationTestFixtures.cs` with `VerificationReportFixture.Planned`, `VerificationSummaryFixture.Success`, and `RecordingVerificationFileSystem`. The recording filesystem stores file contents in an ordinal-ignore-case dictionary, removes the source key on `MoveFile`, records the three operation names used in the assertions, and throws `IOException` before moving when `failMove=true`.

- [ ] **Step 2: Run summary tests and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationSummaryWriterTests --nologo`

Expected: FAIL because summary types/writer do not exist.

- [ ] **Step 3: Add exact JSON records**

Add records with `[JsonPropertyName]` in the root order above:

```csharp
internal sealed record VerificationSummary(
    int SchemaVersion,
    string Contract,
    string RequestedLane,
    string EffectiveLane,
    string RequestedChangeTier,
    string ChangeTier,
    IReadOnlyList<string> TierReasons,
    string? PromotionReason,
    string ExecutionScope,
    string? ShardId,
    bool PlanOnly,
    string CatalogActivationState,
    bool Ok,
    string? ErrorCode,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    long DurationMs,
    IReadOnlyList<VerificationSuiteSummary> Results);

internal sealed record VerificationSuiteSummary(
    string SuiteId,
    string Status,
    string MigrationState,
    int? ExitCode,
    long DurationMs,
    bool TimedOut,
    bool Cancelled,
    string? StandardOutput,
    string? StandardError,
    string? OutputSha256,
    string? ErrorCode);
```

Apply a `[JsonPropertyName("...")]` attribute to every constructor property. `VerificationJson.Options` uses `WriteIndented=true`, `DefaultIgnoreCondition=WhenWritingNull`, no naming-policy inference, and UTF-8 without BOM when writing.

- [ ] **Step 4: Implement summary projection and PASS math**

`VerificationSummaryFactory.Create(plan, catalog, report, startedAt, completedAt)` maps each `SuiteExecutionRecord` through the explicit scope/status strings above, calculates duration as `max(0, completed-started)` for plan-only and uses report duration for actual, and chooses `ErrorCode` from the first non-Planned/non-Passed row. For plan-only, any status other than Planned is failure. For actual, any status other than Passed is failure.

Add this error-path factory for Task 8:

```csharp
internal static VerificationSummary CreateFailure(
    VerificationRequest request,
    VerificationPlan? plan,
    string catalogActivationState,
    string errorCode,
    DateTimeOffset startedAt,
    DateTimeOffset completedAt);
```

It uses requested values when `plan` is absent, sets effective values to the requested values, `execution_scope` from the request (`partial` when suite IDs exist, `shard` when shard exists, otherwise `lane`), `ok=false`, the supplied error code, duration from the timestamps, and an empty results array. It does not accept or serialize an exception message.

- [ ] **Step 5: Implement the atomic writer over the existing filesystem port**

`AtomicVerificationSummaryWriter.WriteAsync` receives the Task 2 filesystem port and:

1. Calls `CreateDirectory(artifactRoot)`.
2. Serializes once.
3. Writes `summary.json.<guid-N>.tmp` in the same directory.
4. Moves to `summary.json` with `overwrite:true`.
5. In `finally`, deletes the temp file only if it still exists.

Do not write directly to `summary.json`, do not create a backup outside the artifact root, and do not include the GUID in the summary.

- [ ] **Step 6: Re-run summary tests**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationSummaryWriterTests --nologo`

Expected: exact-shape/determinism/atomic-cleanup tests PASS, skipped `0`.

- [ ] **Step 7: Commit**

```powershell
git add src/DesktopNode.Verification/VerificationContracts.cs src/DesktopNode.Verification/VerificationSummaryWriter.cs src/DesktopNode.Verification.Tests/VerificationSummaryWriterTests.cs src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs
git commit -m "feat(verification): write deterministic atomic summaries"
```

---

### Task 8: Wire the application while enforcing the Wave A activation lock

**Files:**
- Create: `src/DesktopNode.Verification/VerificationApplication.cs`
- Create: `src/DesktopNode.Verification.Tests/VerificationApplicationTests.cs`
- Modify: `src/DesktopNode.Verification/Program.cs`
- Modify: `src/DesktopNode.Verification.Tests/VerificationProjectContractTests.cs`
- Modify: `src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs`

**Exit codes:**

- `0`: a valid plan-only request wrote an `ok=true` v2 summary.
- `1`: reserved for an activated future actual run that wrote an `ok=false` suite summary.
- `2`: CLI/config/schema/path/activation error. If artifact root was safely resolved, write an `ok=false` summary; otherwise emit only compact error JSON to stderr.

- [ ] **Step 1: Write application RED tests**

Use a physical GUID temp repository containing the canonical config/schema and empty `src/DesktopNode.sln`, plus recording process/managed runners and fixed clock:

```csharp
[Fact]
public async Task FullPlanOnlyWritesSevenRowsAndStartsNoChildProcess()
{
    using var repository = ApplicationRepositoryFixture.Create();
    var process = new RecordingProcessRunner(failIfCalled: true);
    var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
    var output = new StringWriter();
    var error = new StringWriter();
    var application = repository.CreateApplication(process, managed, FixedVerificationClock.At("2026-08-24T00:00:00Z"));

    var exitCode = await application.RunAsync([
        "verify", "--lane", "Full", "--change-tier", "M",
        "--changed-path", ".github/workflows/development-gates.yml",
        "--artifact-root", "artifacts/wave-a-plan", "--plan-only"
    ], output, error, CancellationToken.None);

    Assert.Equal(0, exitCode);
    Assert.Equal(0, process.CallCount);
    Assert.Equal(0, managed.CallCount);
    Assert.Equal(string.Empty, error.ToString());
    using var summary = JsonDocument.Parse(File.ReadAllText(
        Path.Combine(repository.Root, "artifacts", "wave-a-plan", "summary.json")));
    Assert.True(summary.RootElement.GetProperty("ok").GetBoolean());
    Assert.Equal(7, summary.RootElement.GetProperty("results").GetArrayLength());
}

[Fact]
public async Task ActualExecutionIsBlockedBeforeEitherRunner()
{
    using var repository = ApplicationRepositoryFixture.Create();
    var process = new RecordingProcessRunner(failIfCalled: true);
    var managed = new RecordingManagedSuiteRunner(failIfCalled: true);
    var application = repository.CreateApplication(process, managed, FixedVerificationClock.At("2026-08-24T00:00:00Z"));

    var exitCode = await application.RunAsync([
        "verify", "--lane", "Full", "--change-tier", "M",
        "--changed-path", "src/DesktopNode.Runtime/A.cs",
        "--artifact-root", "artifacts/wave-a-actual-blocked"
    ], TextWriter.Null, TextWriter.Null, CancellationToken.None);

    Assert.Equal(2, exitCode);
    Assert.Equal(0, process.CallCount);
    Assert.Equal(0, managed.CallCount);
    using var summary = JsonDocument.Parse(File.ReadAllText(
        Path.Combine(repository.Root, "artifacts", "wave-a-actual-blocked", "summary.json")));
    Assert.False(summary.RootElement.GetProperty("ok").GetBoolean());
    Assert.Equal("PCV_VERIFY_CONFIG_INVALID", summary.RootElement.GetProperty("error_code").GetString());
    Assert.Empty(summary.RootElement.GetProperty("results").EnumerateArray());
}
```

Add tests for Fast source scope, Partial scope, Shard scope, invalid artifact root creating no directory, malformed catalog creating a failure summary, and deterministic stderr JSON that contains code/detail but no environment/token values.

Extend `VerificationTestFixtures.cs` with `FixedVerificationClock` and `ApplicationRepositoryFixture`. The application repository fixture copies the canonical catalog/schema bytes, creates the solution anchor and repository `artifacts` directory, passes its root through `currentDirectory`, returns null `RUNNER_TEMP`, uses a separate fake user-profile directory, and deletes only its GUID-owned parent in `Dispose`.

- [ ] **Step 2: Run application tests and confirm RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationApplicationTests --nologo`

Expected: FAIL because application orchestration does not exist.

- [ ] **Step 3: Implement dependency-injected application orchestration**

Use this constructor and entry method:

```csharp
internal sealed class VerificationApplication(
    IProcessRunner processRunner,
    IManagedSuiteRunner managedSuiteRunner,
    IVerificationFileSystem fileSystem,
    IVerificationClock clock,
    Func<string> currentDirectory,
    Func<string?> runnerTemp,
    Func<string?> userProfile)
{
    internal Task<int> RunAsync(
        IReadOnlyList<string> args,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken);
}
```

Run the pipeline in this order: parse → repository locate → artifact validate → schema/catalog load → plan → activation check → executor → summary factory → atomic writer. The activation check is:

```csharp
if (!request.PlanOnly && !string.Equals(catalog.ActivationState, "active", StringComparison.Ordinal))
{
    throw new VerificationException(
        VerificationErrorCodes.ConfigInvalid,
        $"activation-state={catalog.ActivationState};actual-execution=false");
}
```

Catch only at the application boundary. If repository and artifact root are known safe, call `VerificationSummaryFactory.CreateFailure(request, plan, catalog?.ActivationState ?? "unavailable", exception.Code, startedAt, clock.UtcNow)`, atomically write it, and emit the summary path to stdout. Before safe resolution, serialize a compact anonymous object `{ schema_version=2, contract, ok=false, error_code, error_detail }` to stderr. `error_detail` is the reason token, never `Exception.ToString()`.

- [ ] **Step 4: Replace the bootstrap Program with the async default composition**

```csharp
namespace DesktopNode.Verification;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };
        Console.CancelKeyPress += handler;

        var fileSystem = new PhysicalVerificationFileSystem();
        var application = new VerificationApplication(
            new SystemProcessRunner(),
            new UnavailableManagedSuiteRunner(),
            fileSystem,
            new SystemVerificationClock(),
            () => Directory.GetCurrentDirectory(),
            () => Environment.GetEnvironmentVariable("RUNNER_TEMP"),
            () => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        try
        {
            return await application.RunAsync(
                args, Console.Out, Console.Error, cancellation.Token).ConfigureAwait(false);
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }
}
```

`SystemVerificationClock.UtcNow` returns `DateTimeOffset.UtcNow`. `SystemProcessRunner` uses only the planner-created `ProcessInvocation.WorkingDirectory` as its redaction root; `VerificationApplication` sets that value to the discovered repository root.

Update the Task 1 bootstrap assertion in `VerificationProjectContractTests` for the final async entrypoint contract. Change that test to return `Task`, await `Program.Main([])`, keep exit code `2`, and validate the compact v2 stderr error JSON instead of the superseded `activation_state=foundation-bootstrap` line. This test update is required because `Task<int>` cannot satisfy the earlier synchronous `Assert.Equal(2, Program.Main([]))` call.

- [ ] **Step 5: Re-run application and all new-project tests**

Run:

```powershell
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationApplicationTests --nologo
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --nologo
```

Expected: application tests and the complete new test assembly PASS, failed `0`, skipped `0`.

- [ ] **Step 6: Run the real PlanOnly CLI smoke**

Run:

```powershell
dotnet run --project src/DesktopNode.Verification -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-verification-csharp-wave-a-plan --plan-only
node -e "const s=require('./artifacts/development-verification-csharp-wave-a-plan/summary.json'); if(!s.ok||s.contract!=='pcv-development-verification-summary-v2'||s.execution_scope!=='lane'||s.results.length!==7||s.results.some(x=>x.status!=='planned')) process.exit(1)"
```

Expected: both commands exit `0`; summary has seven planned rows and `catalog_activation_state=plan-only-foundation`.

- [ ] **Step 7: Prove the activation lock from the real CLI**

Run the same command without `--plan-only`, using artifact root `artifacts/development-verification-csharp-wave-a-actual-blocked`.

Expected: exit `2`; its summary has `ok=false`, `error_code=PCV_VERIFY_CONFIG_INVALID`, `results=[]`; no dotnet/npm/git suite child is started by the runner.

- [ ] **Step 8: Commit**

```powershell
git add src/DesktopNode.Verification/Program.cs src/DesktopNode.Verification/VerificationApplication.cs src/DesktopNode.Verification.Tests/VerificationApplicationTests.cs src/DesktopNode.Verification.Tests/VerificationProjectContractTests.cs src/DesktopNode.Verification.Tests/VerificationTestFixtures.cs
git commit -m "feat(verification): expose plan-only C# entrypoint"
```

---

### Task 9: Lock architecture boundaries and publish code-level Wave A evidence

**Files:**
- Create: `src/DesktopNode.Verification.Tests/VerificationArchitectureBoundaryTests.cs`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Create: `docs/ga-ready/evidence/pester-free-csharp-verification-wave-a-foundation-2026-08-24.md`

- [ ] **Step 1: Write architecture-boundary RED assertions**

Create `VerificationArchitectureBoundaryTests.cs`:

```csharp
using System.Text.Json;
using System.Xml.Linq;

namespace DesktopNode.Verification.Tests;

public sealed class VerificationArchitectureBoundaryTests
{
    [Fact]
    public void ProductionProjectHasNoProductOrPowerShellDependency()
    {
        var root = FindRepositoryRoot();
        var project = XDocument.Load(Path.Combine(
            root, "src", "DesktopNode.Verification", "DesktopNode.Verification.csproj"));

        Assert.Empty(project.Descendants("ProjectReference"));
        Assert.DoesNotContain(project.Descendants("PackageReference"), reference =>
            reference.Attribute("Include")?.Value.Contains("PowerShell", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void ProductionSourcesDoNotReferenceProductWmiOrInstallerApis()
    {
        var sourceRoot = Path.Combine(FindRepositoryRoot(), "src", "DesktopNode.Verification");
        var source = string.Join("\n", Directory.EnumerateFiles(sourceRoot, "*.cs")
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(File.ReadAllText));

        Assert.DoesNotContain("System.Management", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Microsoft.Management.Infrastructure", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WindowsInstaller", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogContainsNoPowerShellAndRemainsPlanOnlyFoundation()
    {
        var text = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "config", "development-verification-suites.json"));
        using var document = JsonDocument.Parse(text);

        Assert.Equal("plan-only-foundation", document.RootElement.GetProperty("activation_state").GetString());
        Assert.DoesNotContain("pwsh", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("powershell", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invoke-Pester", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EvidenceRefusesCutoverMutationAndPromotionClaims()
    {
        var evidence = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), "docs", "ga-ready", "evidence",
            "pester-free-csharp-verification-wave-a-foundation-2026-08-24.md"));

        Assert.Contains("host_mutation_performed=false", evidence, StringComparison.Ordinal);
        Assert.Contains("required_ci_pester_zero=false", evidence, StringComparison.Ordinal);
        Assert.Contains("required_ci_nonadmin_powershell_zero=false", evidence, StringComparison.Ordinal);
        Assert.Contains("cutover_completed=false", evidence, StringComparison.Ordinal);
        Assert.Contains("public_trusted_signing=false", evidence, StringComparison.Ordinal);
        Assert.Contains("external_stable_publication=false", evidence, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "DesktopNode.sln"))) return directory.FullName;
        }
        throw new InvalidOperationException("PCV_VERIFY_CONFIG_INVALID|repository-root-not-found");
    }
}
```

The first three architecture tests should pass from earlier tasks. The evidence test is RED because its dated evidence file has not been created yet; do not create an empty file to bypass it.

- [ ] **Step 2: Run the boundary tests and confirm the evidence-only RED**

Run: `dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationArchitectureBoundaryTests --nologo`

Expected: three architecture tests PASS and `EvidenceRefusesCutoverMutationAndPromotionClaims` FAIL because the evidence file is absent.

- [ ] **Step 3: Run the pre-evidence Wave A verification set once**

Run:

```powershell
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName!~EvidenceRefusesCutoverMutationAndPromotionClaims"
dotnet run --project src/DesktopNode.Verification -c Release -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-verification-csharp-wave-a-plan --plan-only
node -e "const fs=require('node:fs'),crypto=require('node:crypto');const p='artifacts/development-verification-csharp-wave-a-plan/summary.json';const b=fs.readFileSync(p);const s=JSON.parse(b);if(!s.ok||s.results.length!==7||s.results.some(x=>x.status!=='planned'))process.exit(1);console.log(crypto.createHash('sha256').update(b).digest('hex'))"
```

Expected: every selected test PASS/skipped `0`; the only excluded test is the already-observed evidence-file RED from Step 2. Plan-only exits `0`; Node prints one 64-character lowercase summary SHA-256. Record the selected test count and the exact exclusion in the evidence so it cannot be mistaken for the final complete assembly result.

- [ ] **Step 4: Prove legacy required workflow was not changed by Wave A**

Run:

```powershell
git diff --name-only bee07214cd4f2f061b30996f766b9976a9527abd -- .github/workflows/development-gates.yml
git diff --name-only bee07214cd4f2f061b30996f766b9976a9527abd -- packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1 packaging/windows-desktop-node/tools/PcvDevelopmentVerificationRunner.psm1 packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1
```

Expected: both commands print no paths. If either prints a path, stop and revert only the Wave A change to that path before documenting evidence.

- [ ] **Step 5: Add a dated policy section without replacing the current entrypoint**

At the top of the dated body in `DEVELOPMENT_VERIFICATION_POLICY.md`, add `## 2026-08-24 C# verification Wave A foundation` and state all of the following:

- command contract and catalog/schema paths;
- seven suite IDs and four shard IDs;
- `activation_state=plan-only-foundation` means only plan projection is available;
- current required workflow and `Invoke-PcvDevelopmentVerification.ps1` remain authoritative during Waves A~D;
- non-plan-only is fail-closed before child process;
- no product/host/admin/Guest transport behavior changed;
- Pester-free and non-admin PowerShell-free claims remain unearned until Wave E.

- [ ] **Step 6: Write the code-level evidence from observed outputs**

The evidence document must record:

- design ID and Wave A plan path;
- base/head commit IDs;
- pre-evidence selected test assembly passed/failed/skipped counts from Step 3 and the exact single-test exclusion;
- plan-only summary locator, seven ordered suite IDs, four shard IDs, activation state and observed SHA-256;
- actual-mode blocked command, exit `2`, error code and empty results;
- the two empty path-diff checks from Step 4;
- `host_mutation_performed=false`, `msi_or_service_mutation=false`, `actual_vm_tested=false`;
- `required_ci_pester_zero=false`, `required_ci_nonadmin_powershell_zero=false`, `cutover_completed=false`;
- `public_trusted_signing=false`, `external_stable_publication=false`, operational current unchanged at `0.42.74-admin-smoke`.

Do not call this PASS evidence for Wave B~E or change `docs/ga-ready/current-evidence.json`.

- [ ] **Step 7: Add document index entries**

Add a dated bullet to `DEVELOPER_INDEX.md` linking the approved design, this implementation plan, catalog/schema, policy section and evidence. Add a dated section at the top of `EVIDENCE_INDEX.md` linking the Wave A evidence and repeating the non-cutover boundary in one sentence.

- [ ] **Step 8: Verify the boundary test turns GREEN, check docs and commit**

Run:

```powershell
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationArchitectureBoundaryTests --nologo
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
git diff --check
rg -n "plan-only-foundation|required_ci_pester_zero=false|required_ci_nonadmin_powershell_zero=false" docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/evidence/pester-free-csharp-verification-wave-a-foundation-2026-08-24.md
```

Expected: all four boundary tests and the complete Release test assembly PASS with skipped `0`; diff check exits `0`; every boundary term has at least one intended match and the evidence has both explicit false claims. Add the observed final complete assembly count to the evidence, then rerun the boundary test once so the recorded count is verified after the final document edit.

Commit:

```powershell
git add src/DesktopNode.Verification.Tests/VerificationArchitectureBoundaryTests.cs docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/evidence/pester-free-csharp-verification-wave-a-foundation-2026-08-24.md
git commit -m "docs: record C# verification Wave A evidence"
```

---

### Task 10: Run the unchanged Full gate and perform the Wave A completion audit

**Files:**
- Verify only; change a file only when a failing test identifies a Wave A defect.

- [ ] **Step 1: Restore once and run the complete .NET Release solution**

Run:

```powershell
dotnet restore src/DesktopNode.sln
dotnet test src/DesktopNode.sln -c Release --no-restore --nologo
```

Expected: all eight test assemblies PASS, failed `0`, skipped `0`; the original seven assemblies retain at least their `967` passing tests.

- [ ] **Step 2: Run the unchanged Web required commands**

Run:

```powershell
npm ci --prefix web
npm test --prefix web
npm run verify:parity --prefix web
```

Expected: all commands exit `0`; served asset/parity checks report no drift.

- [ ] **Step 3: Run the still-required transitional Pester gates once**

Until Wave E cutover, the existing required workflow remains authoritative. Run exactly once with Pester 5.7.1 already available:

```powershell
pwsh -NoProfile -Command "$r=Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru; if($r.FailedCount -gt 0){exit 1}"
pwsh -NoProfile -Command "$r=Invoke-Pester -Path @('packaging/windows-desktop-node/installer/tests','web/tests') -Output Detailed -PassThru; if($r.FailedCount -gt 0){exit 1}"
```

Expected: both commands exit `0`. A failure is investigated normally; do not retry, Skip, weaken assertions or switch `activation_state` to hide it.

- [ ] **Step 4: Re-run both positive and negative C# CLI evidence paths**

Run Full/M plan-only into a fresh artifact directory and validate it with Node. Then run without `--plan-only` into another fresh directory.

Expected positive: exit `0`, `contract=pcv-development-verification-summary-v2`, `execution_scope=lane`, `results=7`, all Planned, `ok=true`.

Expected negative: exit `2`, `error_code=PCV_VERIFY_CONFIG_INVALID`, `catalog_activation_state=plan-only-foundation`, `results=0`, `ok=false`.

- [ ] **Step 5: Audit diff scope and forbidden boundary**

Run:

```powershell
git diff --check bee07214cd4f2f061b30996f766b9976a9527abd
git diff --name-only bee07214cd4f2f061b30996f766b9976a9527abd
rg -n -i '"file_name"\s*:\s*"(pwsh|powershell)|Invoke-Pester|msiexec|sc\.exe' config/development-verification-suites.json
git status --short --branch
```

Expected: diff check exit `0`; changed files are only this plan's file map plus the already-approved design-status update; catalog scan has zero matches; worktree has no uncommitted source/doc change after evidence commit. Ignored `artifacts/**` may exist locally.

- [ ] **Step 6: Compare implementation against every Wave A design bullet**

Record this completion matrix in the final handoff, with command/evidence locators rather than unsupported claims:

| Requirement | Required proof |
| --- | --- |
| console/test projects | solution list + eight-assembly PASS |
| versioned catalog/schema | catalog tests + strict schema paths |
| lane/path policy | matrix tests |
| fake and real safe process boundary | process tests, argument-array source |
| parallel/timeout/cancel | executor/process tests |
| JSON v2 and atomic write | summary tests + summary SHA-256 |
| forbidden PowerShell guard | negative tests + zero catalog scan |
| existing CI unchanged | empty base diff for workflow/tools |
| plan-only foundation only | positive summary + negative activation summary |
| no mutation/promotion | evidence false fields and unchanged operational current |

- [ ] **Step 7: Stop at the Wave B checkpoint**

Do not change CI, Web Pester, migration manifest parity status, or catalog activation in this plan. Hand off the Wave A commits and request a separate Wave B design/plan execution decision.

---

## Rollback and failure handling

- Each task is a separate commit. Revert the first failing task commit and its dependents; do not reset the worktree or discard unrelated user changes.
- Catalog/schema failure is fail-closed; keep `activation_state=plan-only-foundation` and repair the contract/test together.
- A process timeout/cancel defect must retain its failed terminal row and kill evidence; increasing timeout is not the first repair.
- An artifact containment failure writes nothing outside an already validated root.
- Existing workflow/Pester files are untouched, so reverting the Wave A commits restores the exact pre-Wave-A developer path.
- No task authorizes MSI, service, Hyper-V, VM, firewall, Event Log, trust-store, Credential Manager or Guest Execution mutation.

## Deferred work, with owners

- Wave B: migrate `web/tests/PcvDesktopWeb.Static.Tests.ps1` to Node/TypeScript, add its manifest mapping and dual-run parity.
- Wave C: create Installer traits in `DesktopNode.Delivery.Tests`, migrate six Installer Pester files and add mapping rows.
- Wave D: migrate 55 Packaging Pester files plus current evidence checker to C#, complete the 62-row migration manifest.
- Wave E: prove local/CI dual-run parity, change the four required jobs to C#/Node shards, enforce required Pester/PowerShell occurrence zero, activate catalog execution, and preserve legacy Pester as non-required reference.

Wave A alone cannot satisfy or claim any deferred completion condition.

## Design coverage self-check

| Approved design section | Plan owner | Wave A disposition |
| --- | --- | --- |
| §4.1 C# runner | Tasks 1, 3~8 | Implemented behind plan-only activation lock. |
| §4.2 runner tests/ports | Tasks 1~8 | xUnit, fake process/managed/filesystem/clock plus safe process smoke. |
| §4.3 Delivery tests | Waves C/D | Explicitly absent from Wave A; catalog rows remain pending. |
| §4.4 Web verifier | Wave B | Existing npm commands cataloged; Web Pester parity not claimed. |
| §4.5 current evidence checker | Wave D | Managed handler remains pending and actual execution is locked. |
| §5 command/lane/suite/shard rules | Tasks 2~4, 8 | Exact CLI, seven suites, four shards, partial/shard scope and promotion matrix. |
| §6 v2 summary | Task 7 | Deterministic ordering, explicit terminal math, atomic `summary.json`. |
| §7 safety/error boundary | Tasks 2, 3, 5~8 | Allowlist, argument array, timeout/cancel/tree kill, redaction, containment, all eight error codes. |
| §8 four-job CI structure | Tasks 9~10 / Wave E | Wave A proves workflow unchanged; Wave E performs cutover. |
| §9 migration waves | Deferred owner list | B, C, D, E remain separate checkpoints. |
| §10 62-row manifest | Waves B~D | Mapping starts with migrated Web row and completes 62/62 in D; no false Wave A mapping. |
| §11 runner strategy | Tasks 2~10 | Unit/contract strategy covered; dual-run/cutover performance evidence deferred. |
| §12 admin/Guest boundary | Global constraints, Task 9 | Explicitly unchanged and excluded from the runner. |
| §13 cutover conditions | Wave E | All nine remain unclaimed in Wave A evidence. |
| §14 non-scope | Global constraints | No deletion, admin conversion, Guest transport, package/mutation, transport, signing/publication. |
| §15 implementation boundary | Entire plan | Only Wave A is executable here; stop checkpoint is explicit. |

Self-review result: no uncovered Wave A requirement. Every program-level requirement outside Wave A has a named later-wave owner and cannot be promoted by this plan.
