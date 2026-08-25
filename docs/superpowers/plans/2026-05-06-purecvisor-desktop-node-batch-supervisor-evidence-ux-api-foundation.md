# Batch Supervisor Evidence UX/API Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Batch Supervisor evidence root를 Local API/Web Console에서 읽을 수 있는 read-only `ops.summary.batch_evidence` contract로 노출한다.

**Architecture:** 새 mutation route를 만들지 않고 기존 `GET /api/v1/ops/summary` 응답에 `batch_evidence`를 추가한다. API는 설정된 evidence root만 읽고, root가 없거나 JSON이 깨진 경우 summary route를 실패시키지 않고 degraded evidence object와 signal을 반환한다. Host는 `--batch-evidence-root` listen option을 파싱해 API processor에 전달하되 기본값은 미설정 상태로 유지한다.

**Tech Stack:** C#/.NET 10 Local API/Host, System.Text.Json, xUnit, TypeScript API types, PowerShell 7/Pester 5, Markdown docs.

## 실행 종료 정리

- 상태: 완료.
- 병합 경로: Batch 2 evidence API hardening PR `#4`와 후속 closure PR `#6`가 이 foundation plan의 실제 완료 상태를 main에 반영했다.
- 구현 commit: `c3163e23fad504677aac5d55f07c8124b9fb4d56`
- merge commit: `49dae6a5a6c1d79cd0deb936475ac4a8fe8f8940`
- closure commit: `6b375486c0c1aadd9f1cb52b2ee118f4f8a2945f`
- 종료 evidence: `docs/ga-ready/evidence/batch-follow-up-closure-2026-05-06.md`

이 plan은 최초 `0.38.0` 예시 contract를 포함하지만, 작성/구현 당시 canonical evidence baseline은 `0.38.2-admin-smoke`였다. 실제 main 구현은 이후 `0.38.4` fixture와 degraded child evidence hardening으로 승계됐다. 2026-05-06 후속 실행 이후 full admin host mutation 기준은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0384.md`의 `0.38.4-admin-smoke`가 승계했고, 2026-05-07 이후 internal signed build 기준은 `0.38.7-rc.1` evidence가 승계했다.

---

## Boundary

- 실제 Hyper-V, MSI, service, firewall, Event Log, trust-store mutation을 실행하지 않는다.
- API는 artifact 파일을 읽기만 한다. artifact directory 생성, 복사, 삭제, quarantine, migration은 하지 않는다.
- HTTP request로 artifact path를 받지 않는다. Evidence root는 process start option `--batch-evidence-root` 또는 test constructor argument로만 주입한다.
- `summary.json`, `gpu-snapshots.jsonl`, route parity/OS gate summary/provenance를 요약하되 command arguments, stdout/stderr, token file content, bearer token value는 응답에 싣지 않는다.
- Public trusted signing 또는 외부 stable publication으로 해석되는 field를 만들지 않는다. `AllowUnsignedDev` evidence는 `public_trusted_signing="excluded"`와 함께 노출한다.

## File Structure

- Create: `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs`
  - Batch Supervisor artifact root를 read-only로 파싱한다.
  - `summary.json`, `gpu-snapshots.jsonl`, child route/OS artifact summary, provenance를 compact DTO로 변환한다.
  - Missing/malformed artifact를 exception leak 없이 `status`, `errors`로 표현한다.
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
  - `CreateDefault(..., string? batchEvidenceRoot = null)` parameter를 추가한다.
  - `BuildOpsSummary()`에 `batch_evidence`와 evidence signal을 추가한다.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - `ops.summary.batch_evidence` configured/missing/malformed/redaction/GPU count tests를 추가한다.
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`
  - 새 route가 아니라 기존 `OpsSummary` read-only route가 evidence를 소유한다는 guard를 추가한다.
- Modify: `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
  - `--batch-evidence-root` listen option을 파싱한다.
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.cs`
  - Host option 값을 API processor에 전달한다.
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
  - listen option parse guard를 추가한다.
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`
  - real HTTP `/api/v1/ops/summary`에서 configured evidence root가 전달되는지 검증한다.
- Modify: `web/src/api-types.ts`
  - `OpsSummaryResponse["data"].batch_evidence` type을 추가한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Batch Supervisor evidence API boundary, degraded behavior, redaction contract를 문서화한다.
- Modify: `packaging/windows-desktop-node/README.md`
  - `--batch-evidence-root` read-only runtime option과 Web Console 연결 의도를 문서화한다.

## Contract Shape

`GET /api/v1/ops/summary`는 기존 field를 유지하고 `data.batch_evidence`를 추가한다.

```json
{
  "ok": true,
  "operation": "ops.summary",
  "data": {
    "batch_evidence": {
      "schema_version": 1,
      "configured": true,
      "status": "available",
      "artifact_root": "[BATCH_EVIDENCE_ROOT]",
      "latest": {
        "batch_id": "full-admin-host-mutation-gate-20260506-001432-0380",
        "ok": true,
        "status": "completed",
        "total_steps": 2,
        "executed_steps": 2,
        "steps": [
          {
            "step_id": "service-msi-hyperv-admin-smoke",
            "ok": true,
            "exit_code": 0,
            "timed_out": false,
            "retry_count": 1,
            "attempt_count": 1,
            "final_attempt": 1,
            "duration_ms": 120322
          }
        ],
        "gpu_snapshots": {
          "present": true,
          "count": 24,
          "status_counts": {
            "collected": 24
          },
          "peak_adapter_mib": 3912.45,
          "peak_process_mib": 1512.12
        },
        "release": {
          "version": "0.38.0-admin-smoke",
          "git_commit": "267fe6afa0480ebc3b03431490bc37fa251261ae",
          "msi_sha256": "b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9",
          "signing_mode": "AllowUnsignedDev",
          "public_trusted_signing": "excluded",
          "external_stable_publication": "not-claimed"
        },
        "host_final_state": {
          "service_state": "Running",
          "firewall_rule_count": 0,
          "eventlog_source_present": false,
          "trust_root_present": true,
          "trust_publisher_present": true,
          "boot_time_unchanged": true
        }
      },
      "errors": []
    }
  }
}
```

Missing root example:

```json
{
  "schema_version": 1,
  "configured": true,
  "status": "missing",
  "artifact_root": "[BATCH_EVIDENCE_ROOT]",
  "latest": null,
  "errors": [
    {
      "code": "PCV_BATCH_EVIDENCE_ROOT_MISSING",
      "message": "Batch evidence root was configured but does not exist.",
      "retryable": false
    }
  ]
}
```

Unconfigured example:

```json
{
  "schema_version": 1,
  "configured": false,
  "status": "not_configured",
  "artifact_root": null,
  "latest": null,
  "errors": []
}
```

## Task 1: API Failing Tests

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

- [x] **Step 1: configured evidence root test를 추가한다**

Add this test near existing `OpsSummaryReturnsReadOnlyAggregateSnapshot` tests:

```csharp
[Fact]
public void OpsSummaryIncludesBatchEvidenceWhenRootIsConfigured()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-batch-evidence-" + Guid.NewGuid().ToString("N"));
    try
    {
        var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-001432-0380");
        Directory.CreateDirectory(batchRun);
        var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380");
        var osRoot = Path.Combine(root, "os-mutation-gates-batch-profile-20260506-001432-0380");
        Directory.CreateDirectory(routeRoot);
        Directory.CreateDirectory(osRoot);

        File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
        {
          "schema_version": 1,
          "ok": true,
          "status": "completed",
          "batch_id": "full-admin-host-mutation-gate-20260506-001432-0380",
          "artifact_root": "{{batchRun.Replace("\\", "\\\\")}}",
          "total_steps": 2,
          "executed_steps": 2,
          "results": [
            {
              "step_id": "service-msi-hyperv-admin-smoke",
              "ok": true,
              "exit_code": 0,
              "timed_out": false,
              "retry_count": 1,
              "attempt_count": 1,
              "final_attempt": 1,
              "duration_ms": 120322,
              "stdout": "secret-token-value",
              "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
            },
            {
              "step_id": "os-mutation-gate",
              "ok": true,
              "exit_code": 0,
              "timed_out": false,
              "retry_count": 0,
              "attempt_count": 1,
              "final_attempt": 1,
              "duration_ms": 10021,
              "arguments": ["-ArtifactRoot", "{{osRoot.Replace("\\", "\\\\")}}"]
            }
          ]
        }
        """);
        File.WriteAllText(Path.Combine(batchRun, "gpu-snapshots.jsonl"), """
        {"schema_version":1,"ts":"2026-05-05T15:14:38.0000000Z","status":"collected","adapter_memory":[{"mib":3912.45}],"process_memory":[{"mib":1512.12}]}
        {"schema_version":1,"ts":"2026-05-05T15:14:43.0000000Z","status":"unavailable","adapter_memory":[],"process_memory":[],"error":"counter unavailable"}
        """);
        File.WriteAllText(Path.Combine(routeRoot, "summary.json"), """
        {"schema_version":1,"ok":true,"version":"0.38.0-admin-smoke","boot_time_unchanged":true,"final_service":{"State":"Running"}}
        """);
        File.WriteAllText(Path.Combine(routeRoot, "PureCVisorDesktopNode-0.38.0-admin-smoke-windows-x64.provenance.json"), """
        {"schema_version":"1","product":{"version":"0.38.0-admin-smoke"},"git_commit":"267fe6afa0480ebc3b03431490bc37fa251261ae","msi":{"sha256":"b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9"},"signing_mode":"AllowUnsignedDev"}
        """);
        File.WriteAllText(Path.Combine(routeRoot, "msi-lifecycle-smoke.json"), """
        {"ok":true,"steps":[{"name":"install","ok":true,"exit_code":0},{"name":"repair","ok":true,"exit_code":0}]}
        """);
        File.WriteAllText(Path.Combine(osRoot, "summary.json"), """
        {"schema_version":1,"ok":true,"version":"0.38.0-admin-smoke","public_trusted_signing":"excluded","external_stable_publication":"not-claimed","boot_time_unchanged":true,"final_service":{"state":"Running"},"final_firewall_rule_count":0,"final_eventlog_source_present":false,"final_trust_store":{"root_present":true,"publisher_present":true}}
        """);

        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
            }),
            batchEvidenceRoot: root);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
        Assert.Equal(1, evidence.GetProperty("schema_version").GetInt32());
        Assert.True(evidence.GetProperty("configured").GetBoolean());
        Assert.Equal("available", evidence.GetProperty("status").GetString());
        var latest = evidence.GetProperty("latest");
        Assert.Equal("full-admin-host-mutation-gate-20260506-001432-0380", latest.GetProperty("batch_id").GetString());
        Assert.Equal(2, latest.GetProperty("total_steps").GetInt32());
        Assert.Equal(2, latest.GetProperty("gpu_snapshots").GetProperty("count").GetInt32());
        Assert.Equal(1, latest.GetProperty("gpu_snapshots").GetProperty("status_counts").GetProperty("collected").GetInt32());
        Assert.Equal("0.38.0-admin-smoke", latest.GetProperty("release").GetProperty("version").GetString());
        Assert.Equal("AllowUnsignedDev", latest.GetProperty("release").GetProperty("signing_mode").GetString());
        Assert.Equal("excluded", latest.GetProperty("release").GetProperty("public_trusted_signing").GetString());
        Assert.Equal("Running", latest.GetProperty("host_final_state").GetProperty("service_state").GetString());
        Assert.DoesNotContain("secret-token-value", response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
    }
    finally
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
```

- [x] **Step 2: missing root와 malformed summary tests를 추가한다**

Add:

```csharp
[Fact]
public void OpsSummaryReportsMissingBatchEvidenceRootWithoutFailing()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-missing-batch-evidence-" + Guid.NewGuid().ToString("N"));
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(
        nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
        {
            ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
            ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
        }),
        batchEvidenceRoot: root);

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

    Assert.Equal(200, response.StatusCode);
    using var document = JsonDocument.Parse(response.Body);
    var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
    Assert.True(evidence.GetProperty("configured").GetBoolean());
    Assert.Equal("missing", evidence.GetProperty("status").GetString());
    Assert.Equal("PCV_BATCH_EVIDENCE_ROOT_MISSING", evidence.GetProperty("errors")[0].GetProperty("code").GetString());
    Assert.Contains(
        document.RootElement.GetProperty("data").GetProperty("signals").EnumerateArray(),
        signal => signal.GetProperty("key").GetString() == "batch-evidence" &&
                  signal.GetProperty("tone").GetString() == "warn");
}

[Fact]
public void OpsSummaryReportsMalformedBatchEvidenceWithoutLeakingPaths()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-malformed-batch-evidence-" + Guid.NewGuid().ToString("N"));
    try
    {
        var batchRun = Path.Combine(root, "broken-run");
        Directory.CreateDirectory(batchRun);
        File.WriteAllText(Path.Combine(batchRun, "summary.json"), "{not-json");
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
            {
                ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
                ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
            }),
            batchEvidenceRoot: root);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var evidence = document.RootElement.GetProperty("data").GetProperty("batch_evidence");
        Assert.Equal("unavailable", evidence.GetProperty("status").GetString());
        Assert.Equal("PCV_BATCH_EVIDENCE_PARSE_FAILED", evidence.GetProperty("errors")[0].GetProperty("code").GetString());
        Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
    }
    finally
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
```

- [x] **Step 3: tests가 먼저 실패하는지 확인한다**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~OpsSummary"
```

Expected:

```text
FAIL: CreateDefault has no batchEvidenceRoot parameter
```

## Task 2: API Evidence Reader

**Files:**
- Create: `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`

- [x] **Step 1: `BatchEvidenceSummaryReader` skeleton을 추가한다**

Create `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesktopNode.Api;

public sealed class BatchEvidenceSummaryReader
{
    private readonly string? root;

    public BatchEvidenceSummaryReader(string? root)
    {
        this.root = string.IsNullOrWhiteSpace(root) ? null : Path.GetFullPath(root);
    }

    public JsonElement Read()
    {
        if (root is null)
        {
            return JsonFromObject(new SortedDictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["configured"] = false,
                ["status"] = "not_configured",
                ["artifact_root"] = null,
                ["latest"] = null,
                ["errors"] = Array.Empty<object>()
            });
        }

        if (!Directory.Exists(root))
        {
            return WithError("missing", "PCV_BATCH_EVIDENCE_ROOT_MISSING", "Batch evidence root was configured but does not exist.", root);
        }

        try
        {
            var runRoot = ResolveLatestRunRoot(root);
            if (runRoot is null)
            {
                return WithError("missing", "PCV_BATCH_EVIDENCE_SUMMARY_MISSING", "No Batch Supervisor summary.json was found under the configured evidence root.", root);
            }

            return BuildAvailableSummary(runRoot);
        }
        catch (JsonException error)
        {
            return WithError("unavailable", "PCV_BATCH_EVIDENCE_PARSE_FAILED", "Batch evidence JSON could not be parsed.", Redact(error.Message));
        }
        catch (IOException error)
        {
            return WithError("unavailable", "PCV_BATCH_EVIDENCE_READ_FAILED", "Batch evidence could not be read.", Redact(error.Message));
        }
        catch (UnauthorizedAccessException error)
        {
            return WithError("unavailable", "PCV_BATCH_EVIDENCE_READ_FORBIDDEN", "Batch evidence could not be read with the current process identity.", Redact(error.Message));
        }
    }
}
```

This step intentionally does not compile until helper methods are added.

- [x] **Step 2: root selection과 redaction helpers를 추가한다**

In the same class, add methods with these names and behavior:

```csharp
private static string? ResolveLatestRunRoot(string root)
{
    var directSummary = Path.Combine(root, "summary.json");
    if (File.Exists(directSummary))
    {
        return root;
    }

    return Directory.EnumerateDirectories(root)
        .Where(directory => File.Exists(Path.Combine(directory, "summary.json")))
        .OrderByDescending(directory => File.GetLastWriteTimeUtc(Path.Combine(directory, "summary.json")))
        .FirstOrDefault();
}

private string Redact(string? value)
{
    if (string.IsNullOrEmpty(value))
    {
        return string.Empty;
    }

    var redacted = value.Replace(root ?? string.Empty, "[BATCH_EVIDENCE_ROOT]", StringComparison.OrdinalIgnoreCase);
    var repoRoot = FindRepoRoot();
    if (!string.IsNullOrWhiteSpace(repoRoot))
    {
        redacted = redacted.Replace(repoRoot, "[REPO_ROOT]", StringComparison.OrdinalIgnoreCase);
    }

    return redacted;
}

private static string? FindRepoRoot()
{
    var directory = AppContext.BaseDirectory;
    while (!string.IsNullOrWhiteSpace(directory))
    {
        if (File.Exists(Path.Combine(directory, "src", "DesktopNode.sln")) ||
            File.Exists(Path.Combine(directory, "DesktopNode.sln")) ||
            Directory.Exists(Path.Combine(directory, ".git")))
        {
            return directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        directory = Directory.GetParent(directory)?.FullName;
    }

    return null;
}
```

- [x] **Step 3: available summary builder를 구현한다**

Implement `BuildAvailableSummary(string runRoot)` so it reads:

```text
runRoot/summary.json
runRoot/gpu-snapshots.jsonl when present
route parity artifact root from service-msi-hyperv-admin-smoke -ArtifactRoot argument or stdout
os mutation artifact root from os-mutation-gate -ArtifactRoot argument or stdout
routeRoot/summary.json
routeRoot/*provenance.json
routeRoot/msi-lifecycle-smoke.json
osRoot/summary.json
```

The method must return only compact fields:

```csharp
private JsonElement BuildAvailableSummary(string runRoot)
{
    using var batchDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(runRoot, "summary.json")));
    var batch = batchDocument.RootElement;
    var routeRoot = ResolveChildArtifactRoot(batch, "service-msi-hyperv-admin-smoke");
    var osRoot = ResolveChildArtifactRoot(batch, "os-mutation-gate");
    var routeSummary = TryReadJson(Path.Combine(routeRoot ?? string.Empty, "summary.json"));
    var osSummary = TryReadJson(Path.Combine(osRoot ?? string.Empty, "summary.json"));
    var provenance = TryReadFirstJson(routeRoot, "*.provenance.json");
    var msiLifecycle = TryReadJson(Path.Combine(routeRoot ?? string.Empty, "msi-lifecycle-smoke.json"));

    return JsonFromObject(new SortedDictionary<string, object?>
    {
        ["schema_version"] = 1,
        ["configured"] = true,
        ["status"] = "available",
        ["artifact_root"] = Redact(root),
        ["latest"] = new SortedDictionary<string, object?>
        {
            ["batch_id"] = ReadString(batch, "batch_id"),
            ["ok"] = ReadBool(batch, "ok"),
            ["status"] = ReadString(batch, "status"),
            ["total_steps"] = ReadInt(batch, "total_steps"),
            ["executed_steps"] = ReadInt(batch, "executed_steps"),
            ["steps"] = BuildStepSummaries(batch),
            ["gpu_snapshots"] = BuildGpuSnapshotSummary(Path.Combine(runRoot, "gpu-snapshots.jsonl")),
            ["release"] = BuildReleaseSummary(routeSummary, osSummary, provenance),
            ["route_msi_hyperv"] = BuildRouteSummary(routeSummary, msiLifecycle),
            ["os_mutation"] = BuildOsSummary(osSummary),
            ["host_final_state"] = BuildHostFinalState(routeSummary, osSummary)
        },
        ["errors"] = Array.Empty<object>()
    });
}
```

Use `JsonDocument` clones or object projection before documents are disposed. Do not return a `JsonElement` that points to a disposed document.

- [x] **Step 4: `DesktopNodeApiRequestProcessor`에 reader를 연결한다**

Modify constructor and factory:

```csharp
private readonly BatchEvidenceSummaryReader batchEvidenceReader;

private DesktopNodeApiRequestProcessor(
    string tokenStorage,
    string currentExposure,
    IDesktopNodeHyperVNativeAdapter nativeAdapter,
    string? jobStorePath,
    string? batchEvidenceRoot)
{
    this.tokenStorage = tokenStorage;
    this.currentExposure = currentExposure;
    this.nativeAdapter = nativeAdapter;
    this.jobStorePath = string.IsNullOrWhiteSpace(jobStorePath) ? null : jobStorePath;
    batchEvidenceReader = new BatchEvidenceSummaryReader(batchEvidenceRoot);
    LoadJobStore();
}

public static DesktopNodeApiRequestProcessor CreateDefault(
    string tokenStorage = "none",
    string currentExposure = "loopback",
    IDesktopNodeHyperVNativeAdapter? nativeAdapter = null,
    string? jobStorePath = null,
    string? batchEvidenceRoot = null)
{
    return new DesktopNodeApiRequestProcessor(
        tokenStorage,
        currentExposure,
        nativeAdapter ?? DesktopNodeHyperVNativeAdapter.CreateDefault(),
        jobStorePath,
        batchEvidenceRoot);
}
```

In `BuildOpsSummary()`:

```csharp
var batchEvidence = batchEvidenceReader.Read();

return JsonFromObject(new SortedDictionary<string, object?>
{
    ["batch_evidence"] = batchEvidence,
    ["errors"] = errors,
    ["host"] = hostResult.Ok ? hostResult.Data : null,
    ["job_counts"] = jobCounts,
    ["recent_activity"] = jobRows.Take(8).ToArray(),
    ["runtime_policy"] = RuntimePolicyContract.CreateDefault(tokenStorage, currentExposure).Data,
    ["signals"] = BuildOpsSignals(hostResult, vmCounts, jobCounts, errors.Count, batchEvidence),
    ["vm_counts"] = vmCounts
});
```

Update `BuildOpsSignals` signature and append:

```csharp
var batchEvidenceStatus = ReadString(batchEvidence, "status") ?? "not_configured";
var batchEvidenceTone = batchEvidenceStatus is "available" or "not_configured" ? "ok" : "warn";
signals.Add(new SortedDictionary<string, object?>
{
    ["key"] = "batch-evidence",
    ["label"] = "Batch evidence",
    ["tone"] = batchEvidenceTone,
    ["value"] = batchEvidenceStatus
});
```

- [x] **Step 5: API tests를 통과시킨다**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~OpsSummary"
```

Expected:

```text
Passed
```

## Task 3: Route Ownership Guard

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`

- [x] **Step 1: 기존 OpsSummary route가 evidence owner임을 guard한다**

Add to `DefaultContractMapsPhase25RouteCandidates`:

```csharp
Assert.DoesNotContain(contract.Routes, route => route.RouteTemplate.Contains("/evidence", StringComparison.OrdinalIgnoreCase));
```

Add to `DefaultContractKeepsDotNetProductOwners`:

```csharp
Assert.Equal(
    "dotnet-runtime",
    routes[("GET", "/api/v1/ops/summary")].DefaultOwner);
```

- [x] **Step 2: route contract tests를 실행한다**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHandlerAdapterContractTests"
```

Expected:

```text
Passed
```

## Task 4: Host Listen Option

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.cs`
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`

- [x] **Step 1: failing option parse test를 추가한다**

In `DesktopNodeHostOptionsTests.cs`, add:

```csharp
[Fact]
public void ListenOptionsParseBatchEvidenceRoot()
{
    var options = DesktopNodeHostOptions.Parse([
        "listen",
        "--prefix",
        "http://127.0.0.1:7777/",
        "--batch-evidence-root",
        "artifacts\\batch-runs"
    ]);

    Assert.Equal("artifacts\\batch-runs", options.BatchEvidenceRootPath);
}
```

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~ListenOptionsParseBatchEvidenceRoot"
```

Expected:

```text
FAIL: DesktopNodeHostOptions does not contain BatchEvidenceRootPath
```

- [x] **Step 2: Host option을 구현한다**

In `DesktopNodeHostOptions.cs`, add property:

```csharp
public string? BatchEvidenceRootPath { get; init; }
```

In listen parse return object:

```csharp
BatchEvidenceRootPath = GetValue(values, "--batch-evidence-root"),
```

In `DesktopNodeHostApplication.StartAsync`, pass the option:

```csharp
DesktopNodeApiRequestProcessor.CreateDefault(
    token.Storage,
    isLoopback ? "loopback" : "lan",
    jobStorePath: options.JobStorePath,
    batchEvidenceRoot: options.BatchEvidenceRootPath),
```

- [x] **Step 3: HTTP host integration test를 추가한다**

In `DesktopNodeHostApplicationTests.cs`, add a test that starts a loopback host with a temp evidence root and reads `/api/v1/ops/summary`. Reuse the same minimal artifact writer shape from Task 1, but keep the artifact contents smaller:

```csharp
[Fact]
public async Task OpsSummaryIncludesBatchEvidenceFromHostOption()
{
    var evidenceRoot = Path.Combine(Path.GetTempPath(), "pcv-host-batch-evidence-" + Guid.NewGuid().ToString("N"));
    var webRoot = Path.Combine(Path.GetTempPath(), "pcv-host-web-" + Guid.NewGuid().ToString("N"));
    try
    {
        var runRoot = Path.Combine(evidenceRoot, "run");
        Directory.CreateDirectory(runRoot);
        Directory.CreateDirectory(webRoot);
        File.WriteAllText(Path.Combine(runRoot, "summary.json"), """
        {"schema_version":1,"ok":true,"status":"completed","batch_id":"run","total_steps":0,"executed_steps":0,"results":[]}
        """);

        using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
        {
            Prefix = "http://127.0.0.1:0/",
            WebRootPath = webRoot,
            BatchEvidenceRootPath = evidenceRoot
        });

        using var client = new HttpClient { BaseAddress = host.BaseUri };
        var json = await client.GetStringAsync("/api/v1/ops/summary");

        Assert.Contains("\"batch_evidence\"", json, StringComparison.Ordinal);
        Assert.Contains("\"batch_id\":\"run\"", json, StringComparison.Ordinal);
    }
    finally
    {
        if (Directory.Exists(evidenceRoot))
        {
            Directory.Delete(evidenceRoot, recursive: true);
        }

        if (Directory.Exists(webRoot))
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }
}
```

- [x] **Step 4: Host tests를 실행한다**

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~BatchEvidence|FullyQualifiedName~ListenOptionsParseBatchEvidenceRoot"
```

Expected:

```text
Passed
```

## Task 5: Web API Types

**Files:**
- Modify: `web/src/api-types.ts`

- [x] **Step 1: `BatchEvidenceSummary` types를 추가한다**

Add:

```ts
export type BatchEvidenceSummary = {
  schema_version: number;
  configured: boolean;
  status: "not_configured" | "missing" | "available" | "unavailable" | string;
  artifact_root?: string | null;
  latest?: BatchEvidenceRunSummary | null;
  errors?: ApiError[];
};

export type BatchEvidenceRunSummary = {
  batch_id?: string | null;
  ok?: boolean | null;
  status?: string | null;
  total_steps?: number | null;
  executed_steps?: number | null;
  steps?: BatchEvidenceStepSummary[];
  gpu_snapshots?: {
    present?: boolean;
    count?: number;
    status_counts?: Record<string, number>;
    peak_adapter_mib?: number | null;
    peak_process_mib?: number | null;
  };
  release?: {
    version?: string | null;
    git_commit?: string | null;
    msi_sha256?: string | null;
    signing_mode?: string | null;
    public_trusted_signing?: string | null;
    external_stable_publication?: string | null;
  };
  host_final_state?: Record<string, unknown>;
};

export type BatchEvidenceStepSummary = {
  step_id?: string | null;
  ok?: boolean | null;
  exit_code?: number | null;
  timed_out?: boolean | null;
  retry_count?: number | null;
  attempt_count?: number | null;
  final_attempt?: number | null;
  duration_ms?: number | null;
};
```

Extend `OpsSummaryResponse`:

```ts
batch_evidence?: BatchEvidenceSummary;
```

- [x] **Step 2: web type/static tests를 실행한다**

Run:

```powershell
npm test --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Expected:

```text
Passed
```

## Task 6: Docs

**Files:**
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `packaging/windows-desktop-node/README.md`

- [x] **Step 1: verification policy에 API boundary를 추가한다**

In `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, extend the Batch Supervisor / Hang Guard section:

```markdown
- Local API `GET /api/v1/ops/summary`는 `--batch-evidence-root`가 설정된 경우 Batch Supervisor evidence를 `batch_evidence` read-only object로 요약할 수 있다. 이 API는 artifact path를 request parameter로 받지 않고, 설정된 root 밖을 읽지 않는다.
- Evidence root가 없거나 JSON parse가 실패해도 ops summary route 자체는 실패하지 않는다. `batch_evidence.status`는 `missing` 또는 `unavailable`로 내려가며 `signals[key=batch-evidence]`가 `warn` tone을 반환한다.
- `batch_evidence` 응답은 command stdout/stderr, bearer token, protected token file content, absolute local evidence root를 노출하지 않는다.
```

- [x] **Step 2: packaging README에 host option을 추가한다**

In `packaging/windows-desktop-node/README.md`, add under Batch Supervisor evidence description:

```markdown
Installed or development listener can expose a compact read-only evidence summary through `GET /api/v1/ops/summary` by starting `DesktopNode.Host.exe listen` with `--batch-evidence-root <path>`. The listener never accepts an evidence path from HTTP requests; missing or malformed evidence is reported as degraded `batch_evidence` rather than a failed summary route.
```

Keep Korean wording where surrounding document uses Korean.

## Task 7: Verification And Commit

**Files:**
- All files modified above.

- [x] **Step 1: focused verification을 실행한다**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj
npm test --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Expected:

```text
Passed
```

- [x] **Step 2: required broader verification을 실행한다**

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected:

```text
Passed
git diff --check exit 0
```

- [x] **Step 3: commit한다**

Run:

```powershell
git status -sb
git add src/DesktopNode.Api src/DesktopNode.Api.Tests src/DesktopNode.Host src/DesktopNode.Host.Tests web/src/api-types.ts docs/DEVELOPMENT_VERIFICATION_POLICY.md packaging/windows-desktop-node/README.md
git commit -m "Add read-only batch evidence summary"
git status -sb
```

Expected:

```text
Clean worktree after commit
```

Push is separate operator approval:

```powershell
git push
```

## Self-Review

- Spec coverage: Batch Supervisor evidence root, GPU snapshot count, route/MSI/OS evidence, redaction, missing/malformed degraded behavior, and Web API type foundation are each covered by a task.
- Placeholder scan: This plan contains no deferred implementation placeholders. Every task has target files, concrete snippets, commands, and expected outcomes.
- Type consistency: The API field is consistently named `batch_evidence`; TypeScript uses `BatchEvidenceSummary`; Host option is consistently named `BatchEvidenceRootPath` and CLI flag `--batch-evidence-root`.

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-06-purecvisor-desktop-node-batch-supervisor-evidence-ux-api-foundation.md`.

Two execution options:

1. **Subagent-Driven (recommended)** - one fresh worker for API reader/tests, one for Host option/tests, then inline integration review.
2. **Inline Execution** - execute tasks in this session using executing-plans with checkpoints after Tasks 2, 4, and 7.
