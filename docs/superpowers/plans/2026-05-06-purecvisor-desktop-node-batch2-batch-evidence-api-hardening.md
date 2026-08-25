# Batch 2 Batch Evidence API Hardening 구현 계획

> **Agent 작업자 필수:** 이 계획을 구현할 때는 `superpowers:subagent-driven-development`(권장) 또는 `superpowers:executing-plans`를 사용한다. 진행 추적은 체크박스(`- [x]`) 단위로 한다.

**목표:** `ops.summary.batch_evidence`가 최신 Batch Supervisor summary 자체를 읽을 수 있으면 HTTP 200을 유지하고, child evidence 누락/파싱 실패/읽기 실패/containment 거부는 `batch_evidence.status="degraded"`와 sanitized error code로 표현한다.

**아키텍처:** `BatchEvidenceSummaryReader`를 서버 측 단일 reader로 유지한다. Top-level batch summary는 최신 run 선택과 canonical identity의 기준으로 유지하고, route/MSI/Hyper-V, OS mutation, provenance, MSI lifecycle, GPU snapshot은 독립 sub-area로 읽는다. Web Console은 같은 구조화 계약을 받아 `degraded`를 `unavailable`과 구분되는 warning 상태로 렌더링한다.

**기술 스택:** C#/.NET `System.Text.Json`, xUnit `src/DesktopNode.Api.Tests`, TypeScript static Web Console, Pester 5, 기존 npm parity 검증.

---

## 실행 종료 정리

- 상태: 완료
- 구현 commit: `c3163e23fad504677aac5d55f07c8124b9fb4d56`
- PR: `#4` `Harden batch evidence summary degradation`
- Merge commit: `49dae6a5a6c1d79cd0deb936475ac4a8fe8f8940`
- 병합 시각: `2026-05-06T10:57:58Z`
- 종료 evidence: `docs/ga-ready/evidence/batch-follow-up-closure-2026-05-06.md`
- 검증: `dotnet test src\DesktopNode.sln`, packaging/installer/web Pester, `npm test --prefix web`, `npm run verify:parity --prefix web`, `node --check web\app.js`, `git diff --check`
- 경계: Hyper-V, MSI, service, firewall, Event Log, trust-store, LAN, signed build, public trusted signing, 외부 stable publication을 실행하거나 주장하지 않았다.

## 현재 코드 기준

- API 응답은 `DesktopNodeApiRequestProcessor.Handle(new DesktopNodeApiRequest(...))`로 검증한다.
- `ops.summary` 응답의 batch evidence 위치는 root가 아니라 `data.batch_evidence`다.
- Batch Supervisor summary fixture는 `steps`가 아니라 `results` 배열을 사용한다.
- Child artifact root는 `results[].step_id`가 `service-msi-hyperv-admin-smoke` 또는 `os-mutation-gate`인 entry에서 찾는다.
- Child artifact root는 `arguments: ["-ArtifactRoot", "<path>"]` 또는 `stdout: "ArtifactRoot=<path>"` 패턴으로 해석한다.
- 기존 happy-path 기준 테스트는 `OpsSummaryIncludesBatchEvidenceWhenRootIsConfigured`이며 `0.38.2-admin-smoke` canonical evidence field를 유지한다.
- `BuildOpsSignals`는 `batch_evidence.status`가 `available` 또는 `not_configured`가 아니면 `batch-evidence` signal을 `warn`으로 만든다. `degraded`도 이 규칙을 그대로 사용한다.

---

## 범위

- [x] `batch_evidence.status` 값으로 `degraded`를 추가한다.
- [x] 기존 status 값 `not_configured`, `missing`, `available`, `unavailable`을 유지한다.
- [x] 최신 top-level `summary.json`을 읽고 파싱할 수 있지만 child evidence가 누락, malformed, unreadable, containment rejected, incomplete이면 `degraded`를 반환한다.
- [x] 최신 top-level `summary.json` 자체가 누락되면 `missing`, 파싱/읽기 실패면 `unavailable`을 유지한다.
- [x] child evidence sub-area(`route_msi_hyperv`, `os_mutation`, `gpu_snapshots`)에 `status`를 추가한다.
- [x] 기존 클라이언트가 보는 `status`, `latest`, `errors`는 유지한다.
- [x] 응답 JSON이 configured artifact root, repository root, stdout/stderr 본문, raw command arguments, bearer token, API token 값, protected token file 값을 노출하지 않는 테스트를 추가한다.

## 비목표

- [x] Hyper-V VM 생성, MSI install/uninstall, firewall mutation, Event Log mutation, trust-store mutation을 재실행하지 않는다.
- [x] Web Dashboard 레이아웃을 재설계하지 않는다. `degraded` status 표시와 fixture coverage만 추가한다.
- [x] Batch Supervisor 실행, retry, heartbeat, GPU snapshot capture 동작을 바꾸지 않는다.
- [x] `0.38.1`을 canonical evidence로 복원하지 않는다. Batch 2도 `0.38.2-admin-smoke`를 최신 canonical evidence로 유지한다.
- [x] HTTP request로 artifact path를 받는 경로를 추가하지 않는다. Evidence root는 product configuration/environment에서만 온다.

---

## 계약

### Top-Level Status 규칙

- [x] `not_configured`: batch evidence root가 설정되지 않았다.
- [x] `missing`: configured root가 없거나, root 아래 최신 Batch Supervisor summary가 없다.
- [x] `unavailable`: 선택된 최신 top-level batch summary를 안전하게 읽거나 파싱할 수 없다.
- [x] `available`: 최신 batch summary와 기대 child evidence를 모두 읽어 canonical sub-area를 채울 수 있다.
- [x] `degraded`: 최신 batch summary는 읽을 수 있지만 하나 이상의 child sub-area가 누락, malformed, unreadable, containment rejected, incomplete다.

### Error Code 규칙

기존 top-level code는 유지한다.

- [x] `PCV_BATCH_EVIDENCE_ROOT_MISSING`
- [x] `PCV_BATCH_EVIDENCE_REPARSE_POINT_REJECTED`
- [x] `PCV_BATCH_EVIDENCE_SUMMARY_MISSING`
- [x] `PCV_BATCH_EVIDENCE_PARSE_FAILED`
- [x] `PCV_BATCH_EVIDENCE_READ_FAILED`
- [x] `PCV_BATCH_EVIDENCE_READ_FORBIDDEN`

Child-level sanitized code를 추가한다.

- [x] `PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_MISSING`
- [x] `PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_PARSE_FAILED`
- [x] `PCV_BATCH_EVIDENCE_OS_SUMMARY_MISSING`
- [x] `PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED`
- [x] `PCV_BATCH_EVIDENCE_PROVENANCE_MISSING`
- [x] `PCV_BATCH_EVIDENCE_PROVENANCE_PARSE_FAILED`
- [x] `PCV_BATCH_EVIDENCE_MSI_LIFECYCLE_MISSING`
- [x] `PCV_BATCH_EVIDENCE_MSI_LIFECYCLE_PARSE_FAILED`
- [x] `PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_MISSING`
- [x] `PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED`
- [x] `PCV_BATCH_EVIDENCE_CHILD_ROOT_REJECTED`

규칙:

- [x] `status="degraded"`일 때 child-level error는 top-level `errors` 배열에 들어간다.
- [x] Web Console local detail에 필요하면 영향을 받은 sub-area의 `errors`에도 같은 sanitized issue를 둘 수 있다.
- [x] Malformed child JSON은 해당 child만 `unavailable`로 만들고 top-level `latest` identity는 유지한다.
- [x] Child root가 configured root 밖이거나 reparse point면 `PCV_BATCH_EVIDENCE_CHILD_ROOT_REJECTED`를 기록하고 해당 sub-area를 `unavailable`로 둔다.

### Redaction

- [x] Absolute configured evidence root는 `[BATCH_EVIDENCE_ROOT]`로 대체한다.
- [x] Absolute repository root는 `[REPO_ROOT]`로 대체한다.
- [x] `stdout`, `stderr`, raw batch command `arguments`는 `ops.summary` 응답 DTO에 포함하지 않는다.
- [x] `Authorization: Bearer ...` 값은 `[REDACTED_TOKEN]`으로 대체한다.
- [x] `PCV_API_TOKEN`, `ApiToken`, `ApiTokenProtectedFile`, `api-token`, `token_file` 같은 token-bearing fragment 값은 `[REDACTED_TOKEN]`으로 대체한다.
- [x] `BatchEvidenceSummaryReader`는 token file content를 읽지 않는다.
- [x] Error detail은 가능한 file name 또는 category name만 사용하고, full path가 들어가면 redaction을 거친다.

---

## Task 1: API RED 테스트 추가

**파일:**

- 수정: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

- [x] **Step 1: Missing child artifacts RED 테스트 추가**

`OpsSummaryReportsDegradedBatchEvidenceForMissingChildArtifacts`를 추가한다. 현재 구현에서는 child root나 child summary가 없으면 null summary로 `available`이 내려갈 수 있으므로 이 테스트는 RED여야 한다.

```csharp
[Fact]
public void OpsSummaryReportsDegradedBatchEvidenceForMissingChildArtifacts()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-missing-child-" + Guid.NewGuid().ToString("N"));
    try
    {
        var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-145506-0382");
        Directory.CreateDirectory(batchRun);
        var missingRouteRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382");

        File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
        {
          "schema_version": 1,
          "ok": true,
          "status": "completed",
          "batch_id": "full-admin-host-mutation-gate-20260506-145506-0382",
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
              "duration_ms": 108824,
              "arguments": ["-ArtifactRoot", "{{missingRouteRoot.Replace("\\", "\\\\")}}"]
            }
          ]
        }
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
        var data = document.RootElement.GetProperty("data");
        var evidence = data.GetProperty("batch_evidence");
        Assert.Equal("degraded", evidence.GetProperty("status").GetString());
        Assert.Equal("full-admin-host-mutation-gate-20260506-145506-0382", evidence.GetProperty("latest").GetProperty("batch_id").GetString());
        Assert.Contains(
            evidence.GetProperty("errors").EnumerateArray(),
            error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_MISSING");
        Assert.Contains(
            data.GetProperty("signals").EnumerateArray(),
            signal => signal.GetProperty("key").GetString() == "batch-evidence" &&
                      signal.GetProperty("tone").GetString() == "warn" &&
                      signal.GetProperty("value").GetString() == "degraded");
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

- [x] **Step 2: Malformed child artifacts RED 테스트 추가**

`OpsSummaryReportsDegradedBatchEvidenceForMalformedChildArtifacts`를 추가한다. 현재 구현은 `TryReadChildJson`의 `JsonException`이 top-level catch로 올라가 `unavailable`이 되므로 RED여야 한다.

```csharp
[Fact]
public void OpsSummaryReportsDegradedBatchEvidenceForMalformedChildArtifacts()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-malformed-child-" + Guid.NewGuid().ToString("N"));
    try
    {
        var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-145506-0382");
        var routeRoot = Path.Combine(root, "routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382");
        Directory.CreateDirectory(batchRun);
        Directory.CreateDirectory(routeRoot);

        File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
        {
          "schema_version": 1,
          "ok": true,
          "status": "completed",
          "batch_id": "full-admin-host-mutation-gate-20260506-145506-0382",
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
              "duration_ms": 108824,
              "arguments": ["-ArtifactRoot", "{{routeRoot.Replace("\\", "\\\\")}}"]
            }
          ]
        }
        """);
        File.WriteAllText(Path.Combine(routeRoot, "summary.json"), "{ malformed json");

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
        Assert.Equal("degraded", evidence.GetProperty("status").GetString());
        Assert.Equal("unavailable", evidence.GetProperty("latest").GetProperty("route_msi_hyperv").GetProperty("status").GetString());
        Assert.Contains(
            evidence.GetProperty("errors").EnumerateArray(),
            error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_PARSE_FAILED");
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

- [x] **Step 3: Malformed GPU snapshots RED 테스트 추가**

`OpsSummaryReportsDegradedBatchEvidenceForMalformedGpuSnapshots`를 추가한다. 현재 `BuildGpuSnapshotSummary`는 JSONL malformed line에서 `JsonException`을 top-level로 올릴 수 있으므로 RED여야 한다.

```csharp
[Fact]
public void OpsSummaryReportsDegradedBatchEvidenceForMalformedGpuSnapshots()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-malformed-gpu-" + Guid.NewGuid().ToString("N"));
    try
    {
        var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-145506-0382");
        Directory.CreateDirectory(batchRun);
        File.WriteAllText(Path.Combine(batchRun, "summary.json"), """
        {"schema_version":1,"ok":true,"status":"completed","batch_id":"full-admin-host-mutation-gate-20260506-145506-0382","total_steps":1,"executed_steps":1,"results":[]}
        """);
        File.WriteAllText(Path.Combine(batchRun, "gpu-snapshots.jsonl"), "{ malformed jsonl");

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
        Assert.Equal("degraded", evidence.GetProperty("status").GetString());
        Assert.Equal("unavailable", evidence.GetProperty("latest").GetProperty("gpu_snapshots").GetProperty("status").GetString());
        Assert.Contains(
            evidence.GetProperty("errors").EnumerateArray(),
            error => error.GetProperty("code").GetString() == "PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED");
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

- [x] **Step 4: Degraded redaction RED 테스트 추가**

`OpsSummaryRedactsBatchEvidenceSensitiveMaterialAcrossDegradedErrors`를 추가한다.

```csharp
[Fact]
public void OpsSummaryRedactsBatchEvidenceSensitiveMaterialAcrossDegradedErrors()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-degraded-redaction-" + Guid.NewGuid().ToString("N"));
    try
    {
        var batchRun = Path.Combine(root, "full-admin-host-mutation-gate-20260506-145506-0382");
        var missingChild = Path.Combine(root, "missing-child");
        Directory.CreateDirectory(batchRun);
        File.WriteAllText(Path.Combine(batchRun, "summary.json"), $$"""
        {
          "schema_version": 1,
          "ok": true,
          "status": "completed",
          "batch_id": "full-admin-host-mutation-gate-20260506-145506-0382",
          "total_steps": 1,
          "executed_steps": 1,
          "results": [
            {
              "step_id": "service-msi-hyperv-admin-smoke",
              "ok": true,
              "exit_code": 0,
              "timed_out": false,
              "duration_ms": 1,
              "arguments": ["--api-token", "secret-token-value", "-ArtifactRoot", "{{missingChild.Replace("\\", "\\\\")}}"],
              "stdout": "Authorization: Bearer secret-bearer-value",
              "stderr": "ApiTokenProtectedFile=C:\\\\secret\\\\token.txt"
            }
          ]
        }
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
        Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token-value", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-bearer-value", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ApiTokenProtectedFile", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stdout", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stderr", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("arguments", response.Body, StringComparison.OrdinalIgnoreCase);
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

- [x] **Step 5: RED suite 실행**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~OpsSummary"
```

예상 결과: 새 테스트가 실패한다. 실패 이유는 child JSON parse failure가 top-level `unavailable`으로 승격되거나, child missing이 `available`로 남거나, `degraded`/child status/error code가 아직 없기 때문이다.

---

## Task 2: `BatchEvidenceSummaryReader` hardening

**파일:**

- 수정: `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs`

- [x] **Step 1: 내부 issue/read result type 추가**

`BatchEvidenceSummaryReader` 안에 private record를 추가한다.

```csharp
private sealed record BatchEvidenceIssue(string Code, string Message, string? Detail = null)
{
    public SortedDictionary<string, object?> ToJson()
    {
        return new SortedDictionary<string, object?>
        {
            ["code"] = Code,
            ["message"] = Message,
            ["detail"] = Detail,
            ["retryable"] = false
        };
    }
}

private sealed record EvidenceJsonReadResult(JsonElement? Json, BatchEvidenceIssue? Issue)
{
    public bool HasJson => Json.HasValue;
}

private sealed record GpuSnapshotReadResult(SortedDictionary<string, object?> Summary, BatchEvidenceIssue? Issue);
```

- [x] **Step 2: top-level과 child read boundary 분리**

Top-level summary는 계속 fail-fast로 둔다.

```csharp
private JsonElement ReadRequiredTopLevelJson(string path)
{
    using var document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement.Clone();
}
```

Child JSON은 non-throwing으로 바꾼다.

```csharp
private EvidenceJsonReadResult ReadChildJson(
    string? path,
    string missingCode,
    string parseCode,
    string description)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return new EvidenceJsonReadResult(
            null,
            new BatchEvidenceIssue(missingCode, $"{description} evidence path was not discovered."));
    }

    if (!IsReadableEvidencePath(path))
    {
        return new EvidenceJsonReadResult(
            null,
            new BatchEvidenceIssue(missingCode, $"{description} evidence is missing or rejected.", Redact(path)));
    }

    try
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.GetFullPath(path)));
        return new EvidenceJsonReadResult(document.RootElement.Clone(), null);
    }
    catch (JsonException error)
    {
        return new EvidenceJsonReadResult(
            null,
            new BatchEvidenceIssue(parseCode, $"{description} evidence JSON could not be parsed.", Redact(error.Message)));
    }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException)
    {
        return new EvidenceJsonReadResult(
            null,
            new BatchEvidenceIssue(parseCode, $"{description} evidence could not be read.", Redact(error.Message)));
    }
}
```

- [x] **Step 3: child root resolver가 rejection issue를 남기게 변경**

`ResolveChildArtifactRoot`는 기존 containment/reparse point check를 약하게 만들지 않는다. Root가 발견되지 않으면 missing, root가 configured root 밖이거나 reparse point면 `PCV_BATCH_EVIDENCE_CHILD_ROOT_REJECTED`를 issue로 남긴다.

구현 지침:

- `NormalizeChildRoot`가 현재처럼 unusable path를 `null`로 뭉개지 않도록 `ResolveChildArtifactRoot(..., List<BatchEvidenceIssue> issues)`에서 rejected와 missing을 구분한다.
- `ResolveRepoRootRedactedPath` 처리와 configured root containment는 유지한다.
- `stdout`과 `arguments`는 root discovery에만 사용하고 response DTO에는 직렬화하지 않는다.

- [x] **Step 4: `BuildAvailableSummary`를 degraded-aware로 변경**

`BuildAvailableSummary`에서 child issue를 누적하고 status를 결정한다.

```csharp
private JsonElement BuildAvailableSummary(string runRoot)
{
    var issues = new List<BatchEvidenceIssue>();
    var runSummaryPath = Path.Combine(runRoot, "summary.json");
    if (!IsReadableEvidencePath(runSummaryPath))
    {
        return WithError(
            "unavailable",
            "PCV_BATCH_EVIDENCE_REPARSE_POINT_REJECTED",
            "Batch evidence summary path contains a reparse point.",
            "The selected Batch Supervisor summary path contains a reparse point and was not read.");
    }

    var batch = ReadRequiredTopLevelJson(runSummaryPath);
    var routeRoot = ResolveChildArtifactRoot(batch, "service-msi-hyperv-admin-smoke", issues);
    var osRoot = ResolveChildArtifactRoot(batch, "os-mutation-gate", issues);

    var routeSummary = ReadChildJson(
        routeRoot is null ? null : Path.Combine(routeRoot, "summary.json"),
        "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_MISSING",
        "PCV_BATCH_EVIDENCE_ROUTE_SUMMARY_PARSE_FAILED",
        "route_msi_hyperv");
    var osSummary = ReadChildJson(
        osRoot is null ? null : Path.Combine(osRoot, "summary.json"),
        "PCV_BATCH_EVIDENCE_OS_SUMMARY_MISSING",
        "PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED",
        "os_mutation");
    var provenance = ReadFirstChildJson(
        routeRoot,
        "*.provenance.json",
        "PCV_BATCH_EVIDENCE_PROVENANCE_MISSING",
        "PCV_BATCH_EVIDENCE_PROVENANCE_PARSE_FAILED",
        "msi provenance");
    var msiLifecycle = ReadChildJson(
        routeRoot is null ? null : Path.Combine(routeRoot, "msi-lifecycle-smoke.json"),
        "PCV_BATCH_EVIDENCE_MSI_LIFECYCLE_MISSING",
        "PCV_BATCH_EVIDENCE_MSI_LIFECYCLE_PARSE_FAILED",
        "msi lifecycle");
    var gpuSnapshots = BuildGpuSnapshotSummary(Path.Combine(runRoot, "gpu-snapshots.jsonl"));

    AddIssue(issues, routeSummary.Issue);
    AddIssue(issues, osSummary.Issue);
    AddIssue(issues, provenance.Issue);
    AddIssue(issues, msiLifecycle.Issue);
    AddIssue(issues, gpuSnapshots.Issue);

    var status = issues.Count == 0 ? "available" : "degraded";
    return JsonFromObject(new SortedDictionary<string, object?>
    {
        ["schema_version"] = 1,
        ["configured"] = true,
        ["status"] = status,
        ["artifact_root"] = Redact(root),
        ["latest"] = new SortedDictionary<string, object?>
        {
            ["batch_id"] = ReadString(batch, "batch_id"),
            ["ok"] = ReadBool(batch, "ok"),
            ["status"] = ReadString(batch, "status"),
            ["total_steps"] = ReadInt(batch, "total_steps"),
            ["executed_steps"] = ReadInt(batch, "executed_steps"),
            ["steps"] = BuildStepSummaries(batch),
            ["gpu_snapshots"] = gpuSnapshots.Summary,
            ["release"] = BuildReleaseSummary(routeSummary.Json, osSummary.Json, provenance.Json),
            ["route_msi_hyperv"] = BuildRouteSummary(routeSummary, msiLifecycle),
            ["os_mutation"] = BuildOsSummary(osSummary),
            ["host_final_state"] = BuildHostFinalState(routeSummary.Json, osSummary.Json)
        },
        ["errors"] = issues.Select(issue => issue.ToJson()).ToArray()
    });
}
```

`AddIssue` helper:

```csharp
private static void AddIssue(List<BatchEvidenceIssue> issues, BatchEvidenceIssue? issue)
{
    if (issue is not null)
    {
        issues.Add(issue);
    }
}
```

- [x] **Step 5: child status를 route/OS/GPU summary에 추가**

기존 `BuildRouteSummary(JsonElement?, JsonElement?)`와 `BuildOsSummary(JsonElement?)`를 result-aware signature로 바꾼다.

```csharp
private static string ChildStatus(EvidenceJsonReadResult result)
{
    if (result.HasJson)
    {
        return "available";
    }

    return result.Issue?.Code.EndsWith("_MISSING", StringComparison.Ordinal) == true
        ? "missing"
        : "unavailable";
}
```

`BuildRouteSummary`에는 `status`, `ok`, `version`, `boot_time_unchanged`, `msi_lifecycle_ok`, `msi_lifecycle_step_count`를 유지한다. `BuildOsSummary`에는 `status`, `ok`, `version`, `boot_time_unchanged`, `firewall_rule_count`, `eventlog_source_present`를 유지한다.

- [x] **Step 6: GPU JSONL read를 degraded-aware로 변경**

`BuildGpuSnapshotSummary`가 malformed line을 throw하지 않고 `GpuSnapshotReadResult`를 반환하게 바꾼다.

```csharp
private GpuSnapshotReadResult BuildGpuSnapshotSummary(string path)
{
    if (!IsReadableEvidencePath(path))
    {
        return new GpuSnapshotReadResult(
            new SortedDictionary<string, object?>
            {
                ["status"] = "missing",
                ["present"] = false,
                ["count"] = 0,
                ["status_counts"] = new SortedDictionary<string, int>(),
                ["peak_adapter_mib"] = null,
                ["peak_process_mib"] = null
            },
            new BatchEvidenceIssue("PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_MISSING", "GPU snapshot evidence is missing.", Redact(path)));
    }

    try
    {
        // 기존 count/peak/status_counts 계산 로직을 유지한다.
    }
    catch (JsonException error)
    {
        return new GpuSnapshotReadResult(
            new SortedDictionary<string, object?>
            {
                ["status"] = "unavailable",
                ["present"] = true,
                ["count"] = 0,
                ["status_counts"] = new SortedDictionary<string, int>(),
                ["peak_adapter_mib"] = null,
                ["peak_process_mib"] = null
            },
            new BatchEvidenceIssue("PCV_BATCH_EVIDENCE_GPU_SNAPSHOTS_PARSE_FAILED", "GPU snapshot evidence JSONL could not be parsed.", Redact(error.Message)));
    }
}
```

구현 시 `// 기존 ...` 주석만 남기지 말고 현재 `foreach (var line in File.ReadLines(path))` 계산 코드를 그대로 옮긴다.

- [x] **Step 7: Redaction 강화**

`using System.Text.RegularExpressions;`를 추가하고 `Redact`를 강화한다.

```csharp
private string Redact(string? value)
{
    if (string.IsNullOrEmpty(value))
    {
        return string.Empty;
    }

    var redacted = value.Replace(root ?? string.Empty, "[BATCH_EVIDENCE_ROOT]", StringComparison.OrdinalIgnoreCase);
    var repoRoot = FindRepoRootFromConfiguredEvidenceRoot() ?? FindRepoRoot();
    if (!string.IsNullOrWhiteSpace(repoRoot))
    {
        redacted = redacted.Replace(repoRoot, "[REPO_ROOT]", StringComparison.OrdinalIgnoreCase);
    }

    redacted = Regex.Replace(
        redacted,
        @"(?i)\bBearer\s+[A-Za-z0-9._~+/=-]+",
        "Bearer [REDACTED_TOKEN]");
    redacted = Regex.Replace(
        redacted,
        @"(?i)\b(api[_-]?token|apiToken|ApiTokenProtectedFile|token[_-]?file)\b\s*[:=]\s*[^,\s;]+",
        "$1=[REDACTED_TOKEN]");

    return redacted;
}
```

- [x] **Step 8: API targeted GREEN 실행**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~OpsSummary"
```

예상 결과: OpsSummary 관련 테스트가 모두 통과한다.

---

## Task 3: Web contract와 fixture 갱신

**파일:**

- 수정: `web/src/api-types.ts`
- 수정: `web/src/served-app.ts`
- 수정: `web/src/user-visible-fixtures.ts`
- 수정: `web/scripts/verify-browser-fixture.mjs`
- 생성/갱신: `web/app.js`는 기존 build flow로 갱신한다.

- [x] **Step 1: TypeScript type에 `degraded` 추가**

`web/src/api-types.ts`에 status alias를 추가하거나 기존 union에 `degraded`를 포함한다.

```ts
export type BatchEvidenceStatus =
  | "not_configured"
  | "missing"
  | "available"
  | "degraded"
  | "unavailable"
  | string;
```

`BatchEvidenceSummary.status`, `BatchEvidenceRouteMsiHypervSummary.status`, `BatchEvidenceOsMutationSummary.status`에 `BatchEvidenceStatus`를 사용한다. Child summary에는 `errors?: ApiError[]`를 허용한다.

- [x] **Step 2: served app status label 추가**

`web/src/served-app.ts`의 evidence status 처리에 `degraded`를 추가한다. 현재 `evidenceStatusMessage`에는 `degraded` message가 없다.

```ts
function evidenceStatusMessage(status) {
  return {
    not_configured: 'Batch evidence root is not configured.',
    missing: 'Configured evidence root has no readable batch summary.',
    degraded: 'Latest batch supervisor evidence is loaded with child evidence warnings.',
    unavailable: 'Batch evidence summary could not be read.',
    available: 'Latest batch supervisor evidence is loaded.'
  }[status] || 'Batch evidence summary could not be read.';
}
```

`evidenceStatusLabel` 또는 badge helper가 있으면 `degraded`를 `Degraded`로 표시한다. 없다면 기존 fallback이 lowercase만 보여주지 않는지 확인하고 fixture assertion을 맞춘다.

- [x] **Step 3: 사용자 fixture에 degraded case 추가**

`web/src/user-visible-fixtures.ts`의 ops summary fixture 중 하나에 다음 구조를 추가한다.

```ts
batch_evidence: {
  schema_version: 1,
  configured: true,
  status: "degraded",
  artifact_root: "[BATCH_EVIDENCE_ROOT]",
  latest: {
    batch_id: "full-admin-host-mutation-gate-20260506-145506-0382",
    ok: true,
    status: "completed",
    total_steps: 2,
    executed_steps: 2,
    route_msi_hyperv: {
      status: "available",
      ok: true,
      version: "0.38.2-admin-smoke",
      boot_time_unchanged: true,
    },
    os_mutation: {
      status: "unavailable",
      ok: null,
    },
    gpu_snapshots: {
      status: "available",
      present: true,
      count: 18,
      status_counts: { collected: 18 },
      peak_adapter_mib: 3912.45,
      peak_process_mib: 1512.12,
    },
    release: {
      version: "0.38.2-admin-smoke",
      signing_mode: "AllowUnsignedDev",
      public_trusted_signing: "excluded",
      external_stable_publication: "not-claimed",
    },
    host_final_state: {
      service_state: "Running",
      firewall_rule_count: 0,
      eventlog_source_present: false,
      trust_root_present: true,
      trust_publisher_present: true,
      boot_time_unchanged: true,
    },
  },
  errors: [
    {
      code: "PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED",
      message: "os_mutation evidence JSON could not be parsed.",
    },
  ],
}
```

- [x] **Step 4: browser fixture assertion 추가**

`web/scripts/verify-browser-fixture.mjs`에서 degraded batch evidence fixture를 만들고 다음을 검증한다.

```js
const degradedEvidenceRun = await runFixture({
  batchEvidence: buildBatchEvidenceFixture("degraded")
});
requireEvidencePanel(
  degradedEvidenceRun.document,
  ["Degraded", "PCV_BATCH_EVIDENCE_OS_SUMMARY_PARSE_FAILED"],
  "degraded evidence panel"
);
const degradedEvidenceText = [...degradedEvidenceRun.document.elements.values()]
  .map((element) => element.innerHTML || element.textContent || "")
  .join("\n");
requireNotIncludes(degradedEvidenceText, "C:\\\\Users\\\\", "degraded evidence redaction");
requireNotIncludes(degradedEvidenceText, "secret-token-value", "degraded evidence redaction");
requireNotIncludes(degradedEvidenceText, "Authorization: Bearer", "degraded evidence redaction");
```

- [x] **Step 5: Web 검증 실행**

```powershell
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
```

예상 결과: TypeScript, served asset freshness, static parity, browser fixture가 모두 통과한다.

---

## Task 4: 제품 문서 갱신

**파일:**

- 수정: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- 수정: `packaging/windows-desktop-node/README.md`
- 조건부 수정: `docs/DEVELOPER_INDEX.md`가 status enum을 직접 열거하고 있으면 갱신한다.

- [x] **Step 1: 한국어 계약 설명 추가**

다음 문장을 Batch Supervisor 또는 ops summary 설명 근처에 추가한다.

```markdown
- `ops.summary.batch_evidence`는 configured root가 있어도 최신 Batch Supervisor summary 자체가 파싱 가능하면 HTTP 200을 유지한다.
- route/MSI/Hyper-V, OS mutation, provenance, MSI lifecycle, GPU snapshots child evidence가 누락, malformed, unreadable, containment rejected 상태이면 `batch_evidence.status="degraded"`와 sanitized `PCV_BATCH_EVIDENCE_*` error code를 반환한다.
- 최신 batch summary 자체가 누락되면 `missing`, 파싱 또는 읽기 실패이면 `unavailable`이다.
- 응답은 configured artifact root, repository root, stdout/stderr, command arguments, bearer token, protected token file 값을 노출하지 않는다.
```

- [x] **Step 2: canonical evidence wording 유지 확인**

문서에서 최신 canonical evidence가 계속 `0.38.2-admin-smoke`와 다음 artifact root를 가리키는지 확인한다.

```text
artifacts/batch-runs/full-admin-host-mutation-gate-20260506-145506-0382
artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-145506-0382
artifacts/os-mutation-gates-batch-profile-20260506-145506-0382
```

`0.38.1` standalone canonical evidence page를 새로 만들거나 최신으로 표기하지 않는다.

---

## Task 5: 전체 비파괴 검증

실제 host mutation은 실행하지 않는다. 다음 비파괴 검증만 수행한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
dotnet test src/DesktopNode.sln
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

예상 결과: 모두 통과한다. Hyper-V, service install/start/stop/delete, MSI install/repair/uninstall, firewall, Event Log, trust-store, reboot이 필요한 검증은 실행하지 않고 별도 관리자 opt-in gate로 남긴다.

---

## Task 6: Review Checklist

- [x] `ops.summary`가 missing/malformed child evidence에서도 HTTP 200을 반환한다.
- [x] Malformed latest top-level batch summary는 계속 `batch_evidence.status="unavailable"`이다.
- [x] Missing latest summary는 계속 `batch_evidence.status="missing"`이다.
- [x] `degraded`는 latest batch identity와 읽을 수 있는 child data를 유지한다.
- [x] Degraded child error는 stable `PCV_BATCH_EVIDENCE_*` code를 사용한다.
- [x] Response JSON에 configured temp artifact root가 없다.
- [x] Response JSON에 repository root가 없다.
- [x] Response JSON에 `stdout`, `stderr`, `arguments` key가 없다.
- [x] Response JSON에 bearer token 값 또는 API token 값이 없다.
- [x] Web Console이 `Degraded`를 `Unavailable`과 구분해 렌더링한다.
- [x] 문서는 `0.38.2-admin-smoke`를 canonical으로 유지하고 `0.38.1`은 ledger/history reference로만 남긴다.
