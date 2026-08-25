# PureCVisor Desktop Node API Host Job Hardening Installed Evidence Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Local API/Host listener에 `worker 분리`, `cooperative cancellation`, `job policy 단일화`, `request body cap`, `출력 설명 문구 가독성 강화`를 적용하고 설치본 관리자 evidence까지 남긴다.

**Architecture:** Host listener는 request body cap과 service lifecycle cancellation을 소유하고, API processor는 job enqueue/state/store와 background worker orchestration을 소유한다. Job state transition은 `DesktopNode.Runtime.JobStateTransitionPolicy`를 단일 진실로 사용하며, Hyper-V native adapter/provider는 `CancellationToken`을 받아 polling 사이에 취소를 관찰한다.

**Tech Stack:** C#/.NET xUnit, `HttpListener`, Hyper-V WMI `System.Management`, PowerShell Pester, Windows service smoke evidence, 한국어 문서.

---

## 기준 문서

- Spec: `docs/superpowers/specs/2026-05-11-purecvisor-desktop-node-api-host-job-hardening-installed-evidence-design.md`
- ADR index: `docs/ADR_INDEX.md`
- Verification policy: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Installed evidence target: `docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md`
- Artifact target: `artifacts/api-host-job-hardening-installed-evidence-20260511`

## 파일 구조와 책임

- Modify: `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
  - `--max-request-body-bytes` option과 기본값/range를 소유한다.
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.cs`
  - API body bounded read, `413` response, processor worker loop start/stop을 소유한다.
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
  - installed service binary path에 body cap 기본 인자를 명시한다.
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
  - job policy adapter, background worker loop, route timeout cancellation token, readable error payload를 소유한다.
- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
  - native adapter/provider cancellation token signature와 WMI polling cancellation을 소유한다.
- Modify: `src/DesktopNode.Cli/DesktopNodeCliFormatter.cs`
  - problem/error response의 `detail`과 `recommended_action` 표시를 소유한다.
- Modify: `src/DesktopNode.Tui/TuiPoller.cs`
  - read route failure snapshot의 사람이 읽는 message 선택을 소유한다.
- Modify: `src/DesktopNode.Tui/TuiApplication.cs`
  - mutation failure message에서 `recommended_action` 표시를 소유한다.
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
  - body cap option parse와 range reject를 검증한다.
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`
  - known/unknown length request body cap과 static/noVNC regression을 검증한다.
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`
  - service binary path에 `--max-request-body-bytes 1048576`이 포함되는지 검증한다.
- Test: `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`
  - `recommended_action`, route timeout cancellation 전달, worker responsiveness를 검증한다.
- Test: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - API job cancel/retry/recovery가 runtime policy와 같은 결과를 내는지 검증한다.
- Test: `src/DesktopNode.Runtime.Tests/JobStateTransitionPolicyTests.cs`
  - 새 cancellation/readability 문구가 runtime policy의 기존 전이 규칙을 깨지 않는지 보강한다.
- Test: `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`
  - CLI error 출력이 code, message, detail, recommended action을 함께 보여주고 token을 노출하지 않는지 검증한다.
- Test: `src/DesktopNode.Tui.Tests/TuiStateTests.cs`, `src/DesktopNode.Tui.Tests/TuiApplicationTests.cs`
  - TUI route/mutation failure message가 다음 조치 문장을 포함하는지 검증한다.
- Create: `packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1`
  - installed service body cap, worker responsiveness, regression smoke evidence를 수집한다.
- Create: `packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1`
  - installed smoke runner가 필요한 route, redaction, evidence key를 포함하는지 검증한다.
- Modify: `packaging/windows-desktop-node/README.md`
  - 새 installed smoke 실행 명령과 evidence 의미를 한국어로 추가한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - body cap, worker/cancellation, installed evidence 검증 기준을 추가한다.
- Create: `docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md`
  - 실제 installed admin smoke 결과를 한국어 보고서로 기록한다.

## 구현 규칙

- 문서 본문은 한국어로 작성한다. API route, class name, command, error code, JSON field name은 원문을 유지한다.
- public trusted signing, winget public submission, external stable publication을 주장하지 않는다.
- Linux runtime, PowerShell helper fallback, KVM/libvirt/LXC/ZFS/OVS/OVN 코드를 추가하지 않는다.
- `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md`는 현재 unrelated untracked 파일이다. 이 계획 실행 중 staging하거나 수정하지 않는다.

---

### Task 1: Host request body cap

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [ ] **Step 1: `DesktopNodeHostOptionsTests`에 실패 테스트 추가**

`src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`의 `ListenOptionsParseLoopbackPrefixAndProtectedTokenFile`에 다음 argument/assertion을 추가한다.

```csharp
"--max-request-body-bytes",
"2097152"
```

```csharp
Assert.Equal(2_097_152, options.MaxRequestBodyBytes);
```

같은 파일에 range 실패 테스트를 추가한다.

```csharp
[Theory]
[InlineData("1023")]
[InlineData("67108865")]
[InlineData("not-an-int")]
public void ListenOptionsRejectInvalidMaxRequestBodyBytes(string value)
{
    var error = Assert.Throws<ArgumentException>(() =>
        DesktopNodeHostOptions.Parse([
            "listen",
            "--prefix",
            "http://127.0.0.1:7777/",
            "--max-request-body-bytes",
            value
        ]));

    Assert.Contains("PCV_HOST_ARGUMENT_VALUE_INVALID", error.Message);
    Assert.Contains("--max-request-body-bytes", error.Message);
}
```

- [ ] **Step 2: option 테스트 실패 확인**

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostOptionsTests"
```

Expected: `MaxRequestBodyBytes` property가 없어 compile fail 또는 assertion fail.

- [ ] **Step 3: `DesktopNodeHostOptions`에 cap option 구현**

`src/DesktopNode.Host/DesktopNodeHostOptions.cs`의 class 상단에 상수를 추가한다.

```csharp
public const int DefaultMaxRequestBodyBytes = 1_048_576;
public const int MinimumMaxRequestBodyBytes = 1_024;
public const int MaximumMaxRequestBodyBytes = 67_108_864;
```

기존 hardening property 근처에 property를 추가한다.

```csharp
public int MaxRequestBodyBytes { get; init; } = DefaultMaxRequestBodyBytes;
```

`Parse`의 listen return object에 다음 줄을 추가한다.

```csharp
MaxRequestBodyBytes = ParseRangedInt(
    values,
    "--max-request-body-bytes",
    DefaultMaxRequestBodyBytes,
    MinimumMaxRequestBodyBytes,
    MaximumMaxRequestBodyBytes),
```

- [ ] **Step 4: option 테스트 통과 확인**

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostOptionsTests"
```

Expected: PASS.

- [ ] **Step 5: Host listener body cap 실패 테스트 추가**

`src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`에 다음 테스트를 추가한다.

```csharp
[Fact]
public async Task ApiRequestRejectsKnownLengthBodyAboveConfiguredCap()
{
    using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
    {
        Mode = DesktopNodeHostMode.Listen,
        Prefix = "http://127.0.0.1:0/",
        MaxRequestBodyBytes = 16
    });

    using var client = new HttpClient();
    using var content = new StringContent("{\"payload\":\"this body is larger than sixteen bytes\"}", Encoding.UTF8, "application/json");
    using var response = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/login"), content);
    var body = await response.Content.ReadAsStringAsync();

    Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    Assert.Contains("PCV_REQUEST_BODY_TOO_LARGE", body);
    Assert.Contains("recommended_action", body);

    using var health = await client.GetAsync(new Uri(host.BaseUri, "/api/v1/runtime/policy"));
    Assert.Equal(HttpStatusCode.OK, health.StatusCode);
}
```

같은 파일에 unknown length stream 테스트를 추가한다.

```csharp
[Fact]
public async Task ApiRequestRejectsUnknownLengthBodyWhenBoundedReadExceedsCap()
{
    using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
    {
        Mode = DesktopNodeHostMode.Listen,
        Prefix = "http://127.0.0.1:0/",
        MaxRequestBodyBytes = 24
    });

    using var client = new HttpClient();
    using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, "/api/v1/auth/login"));
    request.Content = new PushStreamLikeContent("{\"payload\":\"unknown length payload exceeds cap\"}");

    using var response = await client.SendAsync(request);
    var body = await response.Content.ReadAsStringAsync();

    Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    Assert.Contains("PCV_REQUEST_BODY_TOO_LARGE", body);
}

private sealed class PushStreamLikeContent(string body) : HttpContent
{
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        await stream.WriteAsync(bytes);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}
```

이 테스트는 `System.Text`와 `System.Net` using이 이미 없으면 추가한다.

- [ ] **Step 6: Host listener body cap 테스트 실패 확인**

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostApplicationTests.ApiRequestRejects"
```

Expected: 현재 `200`, `400`, 또는 기존 auth error가 반환되어 FAIL.

- [ ] **Step 7: `DesktopNodeHostApplication`에 bounded body reader 구현**

`src/DesktopNode.Host/DesktopNodeHostApplication.cs`의 `ReadRequestBodyAsync`를 아래 구조로 교체한다.

```csharp
private sealed class DesktopNodeHostRequestBodyTooLargeException(long maxBytes)
    : Exception($"Request body exceeds {maxBytes} bytes.")
{
    public long MaxBytes { get; } = maxBytes;
}

private static async Task<string?> ReadRequestBodyAsync(
    HttpListenerRequest request,
    int maxRequestBodyBytes,
    CancellationToken cancellationToken)
{
    if (!request.HasEntityBody)
    {
        return null;
    }

    var maxBytes = Math.Clamp(
        maxRequestBodyBytes,
        DesktopNodeHostOptions.MinimumMaxRequestBodyBytes,
        DesktopNodeHostOptions.MaximumMaxRequestBodyBytes);

    if (request.ContentLength64 > maxBytes)
    {
        throw new DesktopNodeHostRequestBodyTooLargeException(maxBytes);
    }

    await using var memory = new MemoryStream();
    var buffer = new byte[Math.Min(8192, maxBytes)];
    while (true)
    {
        var read = await request.InputStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
        if (read == 0)
        {
            break;
        }

        if (memory.Length + read > maxBytes)
        {
            throw new DesktopNodeHostRequestBodyTooLargeException(maxBytes);
        }

        memory.Write(buffer, 0, read);
    }

    return request.ContentEncoding.GetString(memory.ToArray());
}
```

`HandleAsync`의 body read 호출을 다음으로 바꾼다.

```csharp
var body = await ReadRequestBodyAsync(request, options.MaxRequestBodyBytes, cancellation.Token).ConfigureAwait(false);
```

`HandleAsync`의 `try`에 body cap catch를 추가한다. `finally` 앞에 아래 catch를 둔다.

```csharp
catch (DesktopNodeHostRequestBodyTooLargeException ex)
{
    await WriteTextAsync(
        context.Response,
        413,
        "application/problem+json",
        RequestBodyTooLargeProblem(ex.MaxBytes, ResolveRequestId(context.Request)),
        CorsHeaders(context.Request)).ConfigureAwait(false);
}
```

같은 class에 problem JSON helper를 추가한다.

```csharp
private static string RequestBodyTooLargeProblem(long maxBytes, string? requestId)
{
    var id = string.IsNullOrWhiteSpace(requestId) ? "req-" + Guid.NewGuid().ToString("N") : requestId;
    return System.Text.Json.JsonSerializer.Serialize(new SortedDictionary<string, object?>
    {
        ["type"] = "about:blank",
        ["title"] = "Payload Too Large",
        ["status"] = 413,
        ["code"] = "PCV_REQUEST_BODY_TOO_LARGE",
        ["operation"] = "request.body",
        ["message"] = "The API request body is larger than the configured listener limit.",
        ["detail"] = $"The listener rejected the request before reading the full body. Configured limit is {maxBytes} bytes.",
        ["recommended_action"] = "Send a smaller JSON body or restart the service with a larger --max-request-body-bytes value within the supported range.",
        ["request_id"] = id,
        ["retryable"] = false,
        ["max_request_body_bytes"] = maxBytes
    }, DesktopNode.Contracts.RuntimePolicyContract.JsonOptions);
}
```

- [ ] **Step 8: body cap 테스트 통과 확인**

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostApplicationTests.ApiRequestRejects"
```

Expected: PASS.

- [ ] **Step 9: service-action plan 테스트 추가**

`src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`의 `ConfigureInstalledPlanUsesNativeServiceActionWithoutScmCommands`에 다음 assertion을 추가한다.

```csharp
Assert.Contains("--max-request-body-bytes", plan.ServiceBinaryPathName);
Assert.Contains("1048576", plan.ServiceBinaryPathName);
```

Expected: Step 10 구현 전에는 `ServiceBinaryPathName` property가 없어 compile fail.

- [ ] **Step 10: service plan에 `ServiceBinaryPathName` 추가**

`src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`의 `DesktopNodeHostServiceActionPlan` record에 `ServiceExecutablePath` 다음 인자를 추가한다.

```csharp
string ServiceBinaryPathName,
```

`CreatePlan`의 return 호출에서 `ServiceExecutablePath: serviceExe,` 다음에 다음 named argument를 추가한다.

```csharp
ServiceBinaryPathName: binPath,
```

`binPath` 배열에 `--retry-after-seconds`, `"15"` 뒤로 body cap 기본 인자를 추가한다.

```csharp
"--max-request-body-bytes",
DesktopNodeHostOptions.DefaultMaxRequestBodyBytes.ToString(System.Globalization.CultureInfo.InvariantCulture)
```

컴파일 오류가 나면 `using System.Globalization;`을 파일 상단에 추가하고 `CultureInfo.InvariantCulture`로 줄인다.

- [ ] **Step 11: Host 전체 관련 테스트 통과 확인**

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostOptionsTests|FullyQualifiedName~DesktopNodeHostApplicationTests|FullyQualifiedName~DesktopNodeHostServiceActionTests"
```

Expected: PASS.

- [ ] **Step 12: Task 1 commit**

Run:

```powershell
git add src/DesktopNode.Host/DesktopNodeHostOptions.cs src/DesktopNode.Host/DesktopNodeHostApplication.cs src/DesktopNode.Host/DesktopNodeHostServiceAction.cs src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs
git commit -m "feat: cap desktop node api request bodies"
```

Expected: commit created. Untracked unrelated evidence file remains unstaged.

---

### Task 2: API error readability and job policy adapter

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Test: `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`
- Test: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Test: `src/DesktopNode.Runtime.Tests/JobStateTransitionPolicyTests.cs`

- [ ] **Step 1: readable problem-details 실패 테스트 추가**

`src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`의 rate limit 테스트에 다음 assertion을 추가한다.

```csharp
Assert.Equal(
    "Wait for the Retry-After interval, then retry with a lower request rate.",
    document.RootElement.GetProperty("recommended_action").GetString());
```

route timeout 테스트에 다음 assertion을 추가한다.

```csharp
Assert.Equal(
    "Check the job or route status, then retry after the Retry-After interval if the operation is safe to repeat.",
    document.RootElement.GetProperty("recommended_action").GetString());
```

- [ ] **Step 2: runtime policy adapter 실패 테스트 추가**

`src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`에 cancel/retry policy alignment 테스트를 추가한다.

```csharp
[Fact]
public void JobCancelUsesRuntimePolicyForQueuedJobs()
{
    var processor = DesktopNodeApiRequestProcessor.CreateDefault();
    var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
    using var createDocument = JsonDocument.Parse(create.Body);
    var jobId = createDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

    var cancel = processor.Handle(new DesktopNodeApiRequest("POST", $"/api/v1/jobs/{jobId}/cancel"));

    Assert.Equal(200, cancel.StatusCode);
    using var cancelDocument = JsonDocument.Parse(cancel.Body);
    var data = cancelDocument.RootElement.GetProperty("data");
    Assert.Equal("canceled", data.GetProperty("status").GetString());
    Assert.Equal("PCV_JOB_CANCELED", data.GetProperty("error").GetProperty("code").GetString());
    Assert.Equal("The job was canceled before it started.", data.GetProperty("error").GetProperty("message").GetString());
}

[Fact]
public void JobRetryUsesRuntimePolicyAttemptLimit()
{
    var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-runtime-policy-retry-" + Guid.NewGuid().ToString("N") + ".json");
    try
    {
        File.WriteAllText(jobStorePath, """
        {
          "version": 1,
          "jobs": [
            {
              "job_id": "job-failed",
              "operation": "vm.start",
              "status": "failed",
              "params": { "name": "alpha" },
              "result": null,
              "error": {
                "code": "PCV_TEST_RETRYABLE",
                "message": "Synthetic retryable failure.",
                "detail": "retry evidence",
                "retryable": true
              },
              "retry_of": null,
              "attempt": 3,
              "canceled_at": null,
              "created_at": "2026-05-11T00:00:00.0000000Z",
              "updated_at": "2026-05-11T00:00:00.0000000Z"
            }
          ],
          "queue": []
        }
        """);

        var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

        var retry = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/jobs/job-failed/retry"));

        Assert.Equal(409, retry.StatusCode);
        using var document = JsonDocument.Parse(retry.Body);
        Assert.Equal("PCV_JOB_RETRY_LIMIT_REACHED", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }
    finally
    {
        File.Delete(jobStorePath);
    }
}
```

- [ ] **Step 3: API policy/readability 테스트 실패 확인**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHardeningRequestProcessorTests|FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests.JobCancelUsesRuntimePolicyForQueuedJobs|FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests.JobRetryUsesRuntimePolicyAttemptLimit"
```

Expected: `recommended_action` 없음 또는 retry/cancel 기존 내부 string path로 인한 assertion fail.

- [ ] **Step 4: `DesktopNodeApiError`에 additive field 추가**

`src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`의 `DesktopNodeApiError` record를 다음으로 바꾼다.

```csharp
public sealed record DesktopNodeApiError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("detail")] string Detail,
    [property: JsonPropertyName("retryable")] bool Retryable,
    [property: JsonPropertyName("recommended_action")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RecommendedAction = null);
```

`DesktopNodeHyperVOperationResult.Failure`에 optional parameter를 추가한다.

```csharp
string? recommendedAction = null
```

그리고 error 생성부를 다음으로 바꾼다.

```csharp
Error: new DesktopNodeApiError(code, message, detail, retryable, recommendedAction));
```

`Failure` helper에도 optional parameter를 추가한다.

```csharp
private static DesktopNodeApiResponse Failure(
    int statusCode,
    string operation,
    string code,
    string message,
    string detail,
    bool retryable,
    string? recommendedAction = null)
{
    return Json(statusCode, Body(false, operation, null, new DesktopNodeApiError(code, message, detail, retryable, recommendedAction)));
}
```

- [ ] **Step 5: problem-details에 `recommended_action` 추가**

`RouteTimeoutResponse` payload에 다음 field를 추가한다.

```csharp
["message"] = "The Local API route timed out before the response deadline.",
["recommended_action"] = "Check the job or route status, then retry after the Retry-After interval if the operation is safe to repeat.",
```

`RateLimitExceededResponse` payload에 다음 field를 추가한다.

```csharp
["message"] = "The Local API request limit was exceeded for the current client identity.",
["recommended_action"] = "Wait for the Retry-After interval, then retry with a lower request rate.",
```

- [ ] **Step 6: runtime policy adapter helpers 추가**

`src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` 상단에 runtime using을 추가한다.

```csharp
using DesktopNode.Runtime;
```

`DesktopNodeApiRequestProcessor` 내부에 다음 helpers를 추가한다.

```csharp
private static DesktopNodeJob ToRuntimeJob(DesktopNodeApiJob job)
{
    return new DesktopNodeJob(
        JobId: job.JobId,
        Operation: job.Operation,
        Status: ParseRuntimeStatus(job.Status),
        Parameters: job.Params.Clone(),
        Result: job.Result,
        Error: ToRuntimeError(job.Error),
        RetryOf: job.RetryOf,
        Attempt: job.Attempt);
}

private static JobStatus ParseRuntimeStatus(string status)
{
    return status switch
    {
        "queued" => JobStatus.Queued,
        "running" => JobStatus.Running,
        "succeeded" => JobStatus.Succeeded,
        "failed" => JobStatus.Failed,
        "canceled" => JobStatus.Canceled,
        _ => JobStatus.Failed
    };
}

private static string ToApiStatus(JobStatus status)
{
    return status switch
    {
        JobStatus.Queued => "queued",
        JobStatus.Running => "running",
        JobStatus.Succeeded => "succeeded",
        JobStatus.Failed => "failed",
        JobStatus.Canceled => "canceled",
        _ => "failed"
    };
}

private static JobError? ToRuntimeError(DesktopNodeApiError? error)
{
    return error is null
        ? null
        : new JobError(error.Code, error.Message, error.Detail, error.Retryable);
}

private static DesktopNodeApiError? ToApiError(JobError? error)
{
    return error is null
        ? null
        : new DesktopNodeApiError(error.Code, error.Message, error.Detail, error.Retryable);
}

private static HelperExecutionResult ToHelperExecutionResult(DesktopNodeHyperVOperationResult result)
{
    return result.Ok
        ? HelperExecutionResult.Success(result)
        : HelperExecutionResult.Failure(ToRuntimeError(result.Error) ?? new JobError(
            "PCV_NATIVE_OPERATION_FAILED",
            "The native operation failed without a structured error.",
            "The native adapter returned ok=false and no error payload.",
            true));
}

private static void ApplyRuntimeJob(DesktopNodeApiJob target, DesktopNodeJob source)
{
    target.Status = ToApiStatus(source.Status);
    target.Result = source.Result is DesktopNodeHyperVOperationResult operationResult
        ? operationResult
        : source.Result;
    target.Error = ToApiError(source.Error);
}
```

- [ ] **Step 7: cancel/retry/recovery path를 policy로 교체**

Cancel route의 direct string mutation block을 다음 형태로 바꾼다.

```csharp
var decision = JobStateTransitionPolicy.Cancel(ToRuntimeJob(job));
if (!decision.Ok)
{
    var error = ToApiError(decision.Error)!;
    return Json(409, Body(false, "job.cancel", null, error));
}

ApplyRuntimeJob(job, decision.Job!);
job.CanceledAt = Now();
job.UpdatedAt = job.CanceledAt;
RemoveFromQueue(jobId);
SaveJobStore();
return Json(200, Body(true, "job.cancel", JobData(job), null));
```

Retry route의 validation block을 다음 형태로 바꾼다.

```csharp
var retryJobId = "job-" + Guid.NewGuid().ToString("N");
var decision = JobStateTransitionPolicy.Retry(ToRuntimeJob(job), retryJobId);
if (!decision.Ok)
{
    var error = ToApiError(decision.Error)!;
    return Json(409, Body(false, "job.retry", null, error));
}

var retryJob = CreateJob(
    decision.Job!.Operation,
    job.Params,
    decision.Job.RetryOf,
    decision.Job.Attempt,
    job.CorrelationId);
return Json(202, Body(true, "job.retry", JobData(retryJob), null));
```

`TryLoadJob`의 persisted running recovery를 `JobStateTransitionPolicy.RecoverPersistedRunningJob`로 바꾼다.

```csharp
if (status == "running")
{
    var recovered = JobStateTransitionPolicy.RecoverPersistedRunningJob(new DesktopNodeJob(
        JobId: jobId,
        Operation: ReadString(jobElement, "operation") ?? string.Empty,
        Status: JobStatus.Running,
        Parameters: ReadElement(jobElement, "params") ?? EmptyObject(),
        Result: null,
        Error: null,
        RetryOf: ReadString(jobElement, "retry_of"),
        Attempt: ReadInt(jobElement, "attempt") ?? 1));
    status = ToApiStatus(recovered.Status);
    result = null;
    error = ToApiError(recovered.Error);
    updatedAt = Now();
    sawRunningJob = true;
}
```

- [ ] **Step 8: runtime policy tests 통과 확인**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHardeningRequestProcessorTests|FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests.JobCancelUsesRuntimePolicyForQueuedJobs|FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests.JobRetryUsesRuntimePolicyAttemptLimit|FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests.JobStoreMarksPersistedRunningJobsInterrupted"
```

Expected: PASS.

- [ ] **Step 9: Runtime policy unit tests 보강**

`src/DesktopNode.Runtime.Tests/JobStateTransitionPolicyTests.cs`에 다음 test를 추가한다.

```csharp
[Fact]
public void CancelRejectsRunningJobWithStableOperatorMessage()
{
    var job = DesktopNodeJob.CreateQueued("job-running", "vm.start") with
    {
        Status = JobStatus.Running
    };

    var result = JobStateTransitionPolicy.Cancel(job);

    Assert.False(result.Ok);
    Assert.Equal("PCV_JOB_NOT_CANCELABLE", result.Error!.Code);
    Assert.Contains("Only queued jobs can be canceled", result.Error.Detail);
}
```

Run:

```powershell
dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj --filter "FullyQualifiedName~JobStateTransitionPolicyTests"
```

Expected: PASS.

- [ ] **Step 10: Task 2 commit**

Run:

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs src/DesktopNode.Runtime.Tests/JobStateTransitionPolicyTests.cs
git commit -m "feat: align api jobs with runtime policy"
```

Expected: commit created.

---

### Task 3: Background worker 분리

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.cs`
- Test: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Test: `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`

- [ ] **Step 1: worker가 read route를 막지 않는 실패 테스트 추가**

`src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`에 다음 테스트를 추가한다.

```csharp
[Fact]
public async Task BackgroundWorkerDoesNotBlockReadRoutesWhileNativeMutationRuns()
{
    var adapter = new BlockingMutationNativeHyperVAdapter();
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(nativeAdapter: adapter);
    var create = processor.Handle(new DesktopNodeApiRequest("POST", "/api/v1/vms/alpha/start"));
    Assert.Equal(202, create.StatusCode);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var worker = processor.RunWorkerLoopAsync(cts.Token, workerCount: 1, idleDelay: TimeSpan.FromMilliseconds(10));
    Assert.True(adapter.WaitForMutationEntered(TimeSpan.FromSeconds(2)));

    var stopwatch = Stopwatch.StartNew();
    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy"));
    stopwatch.Stop();

    adapter.ReleaseMutation();
    await worker.WaitAsync(TimeSpan.FromSeconds(5));

    Assert.Equal(200, response.StatusCode);
    Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(500));
}

private sealed class BlockingMutationNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
{
    private readonly ManualResetEventSlim mutationEntered = new(false);
    private readonly ManualResetEventSlim releaseMutation = new(false);

    public bool TryInvoke(string operation, JsonElement parameters, out DesktopNodeHyperVOperationResult result)
    {
        if (operation == "vm.start")
        {
            mutationEntered.Set();
            if (!releaseMutation.Wait(TimeSpan.FromSeconds(5)))
            {
                throw new TimeoutException("Mutation was not released.");
            }
        }

        result = new DesktopNodeHyperVOperationResult(
            Ok: true,
            Operation: operation,
            Data: JsonSerializer.SerializeToElement(new SortedDictionary<string, object?>
            {
                ["name"] = parameters.TryGetProperty("name", out var name) ? name.GetString() : "alpha",
                ["action"] = "start"
            }),
            Error: null);
        return true;
    }

    public bool WaitForMutationEntered(TimeSpan timeout) => mutationEntered.Wait(timeout);

    public void ReleaseMutation() => releaseMutation.Set();
}
```

이 테스트는 Task 4에서 `TryInvoke` signature가 바뀌면 `CancellationToken` parameter를 추가해 다시 갱신한다.

- [ ] **Step 2: worker responsiveness 테스트 실패 확인**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHardeningRequestProcessorTests.BackgroundWorkerDoesNotBlockReadRoutesWhileNativeMutationRuns"
```

Expected: `RunWorkerLoopAsync`가 없어 compile fail.

- [ ] **Step 3: API processor에 started job snapshot 추가**

`DesktopNodeApiRequestProcessor` class 내부에 private record를 추가한다.

```csharp
private sealed record StartedJobSnapshot(
    string JobId,
    string Operation,
    JsonElement Parameters);
```

- [ ] **Step 4: job start와 complete를 짧은 lock으로 분리**

`DesktopNodeApiRequestProcessor`에 다음 methods를 추가한다.

```csharp
private StartedJobSnapshot? TryStartNextQueuedJob()
{
    lock (sync)
    {
        while (queue.Count > 0)
        {
            var jobId = queue.Dequeue();
            if (!jobs.TryGetValue(jobId, out var job) || job.Status != "queued")
            {
                continue;
            }

            var started = JobStateTransitionPolicy.Start(ToRuntimeJob(job));
            if (!started.Ok)
            {
                job.Status = "failed";
                job.Result = null;
                job.Error = ToApiError(started.Error);
                job.UpdatedAt = Now();
                SaveJobStore();
                continue;
            }

            ApplyRuntimeJob(job, started.Job!);
            job.UpdatedAt = Now();
            SaveJobStore();
            return new StartedJobSnapshot(job.JobId, job.Operation, job.Params.Clone());
        }

        return null;
    }
}

private DesktopNodeApiWorkerTickResult CompleteStartedJob(
    StartedJobSnapshot started,
    DesktopNodeHyperVOperationResult operationResult)
{
    lock (sync)
    {
        if (!jobs.TryGetValue(started.JobId, out var job))
        {
            return new DesktopNodeApiWorkerTickResult(false, null);
        }

        var completed = JobStateTransitionPolicy.Complete(ToRuntimeJob(job), ToHelperExecutionResult(operationResult));
        if (!completed.Ok)
        {
            job.Status = "failed";
            job.Result = null;
            job.Error = ToApiError(completed.Error);
        }
        else
        {
            ApplyRuntimeJob(job, completed.Job!);
        }

        job.UpdatedAt = Now();
        SaveJobStore();
        return new DesktopNodeApiWorkerTickResult(true, JobData(job));
    }
}
```

- [ ] **Step 5: async worker tick와 loop 추가**

기존 `ProcessOneQueuedJobCore`를 lock 밖 native invocation 구조로 교체한다.

```csharp
public DesktopNodeApiWorkerTickResult ProcessOneQueuedJob()
{
    return ProcessOneQueuedJobAsync(CancellationToken.None).GetAwaiter().GetResult();
}

public async Task<DesktopNodeApiWorkerTickResult> ProcessOneQueuedJobAsync(CancellationToken cancellationToken)
{
    var started = TryStartNextQueuedJob();
    if (started is null)
    {
        return new DesktopNodeApiWorkerTickResult(false, null);
    }

    var result = await Task.Run(
        () => InvokeHyperVOperation(started.Operation, started.Parameters),
        cancellationToken).ConfigureAwait(false);
    return CompleteStartedJob(started, result);
}

public async Task RunWorkerLoopAsync(
    CancellationToken cancellationToken,
    int workerCount = 1,
    TimeSpan? idleDelay = null)
{
    var delay = idleDelay ?? TimeSpan.FromMilliseconds(250);
    var boundedWorkerCount = Math.Clamp(workerCount, 1, 1);
    while (!cancellationToken.IsCancellationRequested)
    {
        var processed = false;
        for (var index = 0; index < boundedWorkerCount; index++)
        {
            var tick = await ProcessOneQueuedJobAsync(cancellationToken).ConfigureAwait(false);
            processed |= tick.Processed;
            if (!tick.Processed)
            {
                break;
            }
        }

        if (!processed)
        {
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
    }
}
```

기존 `ProcessWorkerPool`은 tests와 compatibility를 위해 sequential wrapper로 유지한다.

```csharp
public IReadOnlyList<DesktopNodeApiWorkerTickResult> ProcessWorkerPool(int workerCount = 1)
{
    var results = new List<DesktopNodeApiWorkerTickResult>();
    for (var index = 0; index < Math.Clamp(workerCount, 1, 1); index++)
    {
        var tick = ProcessOneQueuedJob();
        if (!tick.Processed)
        {
            break;
        }

        results.Add(tick);
    }

    return results;
}
```

- [ ] **Step 6: Host에서 processor worker loop 시작**

`DesktopNodeHostApplication` constructor에서 listener task 생성 뒤 worker task를 붙인다. 기존 `loopTasks = listeners.Select(...).ToArray();`를 다음으로 바꾼다.

```csharp
var listenerTasks = listeners
    .Select(binding => Task.Run(() => RunAsync(binding, cancellation.Token)))
    .ToArray();
var workerTask = Task.Run(
    () => processor.RunWorkerLoopAsync(cancellation.Token, workerCount: 1),
    cancellation.Token);
loopTasks = listenerTasks.Append(workerTask).ToArray();
```

`HandleAsync`의 `shouldProcessJobs` 변수와 `finally` 뒤 `processor.ProcessWorkerPool()` 호출을 제거한다. Mutation route는 enqueue 후 worker loop가 처리한다.

- [ ] **Step 7: Host worker loop regression 테스트 추가**

`src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`에 다음 테스트를 추가한다.

```csharp
[Fact]
public async Task HostStartsBackgroundWorkerForQueuedJobs()
{
    using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
    {
        Mode = DesktopNodeHostMode.Listen,
        Prefix = "http://127.0.0.1:0/"
    });

    using var client = new HttpClient();
    using var create = await client.PostAsync(
        new Uri(host.BaseUri, "/api/v1/vms/alpha/start"),
        new StringContent("{}", Encoding.UTF8, "application/json"));

    Assert.Equal(HttpStatusCode.Accepted, create.StatusCode);
    using var createdDocument = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
    var jobId = createdDocument.RootElement.GetProperty("data").GetProperty("job_id").GetString()!;

    var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
    string? status = null;
    do
    {
        using var get = await client.GetAsync(new Uri(host.BaseUri, $"/api/v1/jobs/{jobId}"));
        using var getDocument = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        status = getDocument.RootElement.GetProperty("data").GetProperty("status").GetString();
        if (status is "succeeded" or "failed")
        {
            break;
        }

        await Task.Delay(100);
    } while (DateTimeOffset.UtcNow < deadline);

    Assert.Contains(status, new[] { "succeeded", "failed" });
}
```

- [ ] **Step 8: Task 3 tests 통과 확인**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHardeningRequestProcessorTests.BackgroundWorkerDoesNotBlockReadRoutesWhileNativeMutationRuns|FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests.VmCreateQueuesJobAndWorkerDispatchesToNativeAdapterWithoutExternalFallback"
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "FullyQualifiedName~DesktopNodeHostApplicationTests.HostStartsBackgroundWorkerForQueuedJobs"
```

Expected: PASS.

- [ ] **Step 9: Task 3 commit**

Run:

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs src/DesktopNode.Host/DesktopNodeHostApplication.cs src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs
git commit -m "feat: run api jobs in background worker"
```

Expected: commit created.

---

### Task 4: Cooperative cancellation

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`

- [ ] **Step 1: native adapter cancellation 실패 테스트 추가**

`src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`에 provider-level cancellation 테스트를 추가한다.

```csharp
[Fact]
public void NativeVmPowerStateAdapterReturnsCancellationFailureWhenProviderObservesToken()
{
    using var parameters = JsonDocument.Parse("""{"name":"alpha"}""");
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var adapter = new DesktopNodeHyperVNativeAdapter(
        new RecordingHyperVSwitchProvider([]),
        new RecordingHyperVVmProvider([CompleteVm("alpha")]),
        new RecordingHyperVCheckpointProvider([]),
        new RecordingHyperVCheckpointMutationProvider(),
        new CancelingHyperVVmPowerStateProvider(),
        new RecordingHyperVVmCreateProvider(),
        new RecordingHyperVVmDeleteProvider());

    var handled = adapter.TryInvoke("vm.start", parameters.RootElement, cts.Token, out var result);

    Assert.True(handled);
    Assert.False(result.Ok);
    Assert.Equal("PCV_NATIVE_OPERATION_CANCELED", result.Error!.Code);
    Assert.True(result.Error.Retryable);
}

private sealed class CancelingHyperVVmPowerStateProvider : IDesktopNodeHyperVVmPowerStateProvider
{
    public DesktopNodeHyperVVmPowerStateInfo Invoke(string operation, string vmName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new DesktopNodeHyperVVmPowerStateInfo(vmName, "start");
    }
}
```

- [ ] **Step 2: route timeout cancellation propagation 실패 테스트 추가**

`src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`의 delayed adapter를 cancellation-aware로 바꾸고 새 테스트를 추가한다.

```csharp
[Fact]
public void RouteTimeoutPassesCancellationToNativeAdapter()
{
    var adapter = new CancellationObservingNativeHyperVAdapter();
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(
        nativeAdapter: adapter,
        hardeningOptions: new DesktopNodeApiHardeningOptions(
            RouteTimeoutSeconds: 1,
            RequestLimitPerMinute: 100,
            BurstLimit: 0,
            RetryAfterSeconds: 7));

    var response = processor.Handle(new DesktopNodeApiRequest(
        "GET",
        "/api/v1/vms",
        RequestId: "req-timeout-cancel",
        ClientIdentity: "operator-a"));

    Assert.Equal(504, response.StatusCode);
    Assert.True(adapter.WaitForCancellation(TimeSpan.FromSeconds(3)));
}

private sealed class CancellationObservingNativeHyperVAdapter : IDesktopNodeHyperVNativeAdapter
{
    private readonly ManualResetEventSlim observedCancellation = new(false);

    public bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                observedCancellation.Set();
                result = DesktopNodeHyperVOperationResult.Failure(
                    operation,
                    "PCV_NATIVE_OPERATION_CANCELED",
                    "The native operation was canceled before it completed.",
                    "The route timeout cancellation token was observed by the native adapter.",
                    true,
                    "Check route status and retry only if the operation is safe to repeat.");
                return true;
            }

            Thread.Sleep(50);
        }

        result = DesktopNodeHyperVOperationResult.Failure(operation, "PCV_TEST_TIMEOUT_NOT_OBSERVED", "Cancellation was not observed.", "The test adapter did not receive cancellation.", true);
        return true;
    }

    public bool WaitForCancellation(TimeSpan timeout) => observedCancellation.Wait(timeout);
}
```

- [ ] **Step 3: cancellation tests compile failure 확인**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~NativeVmPowerStateAdapterReturnsCancellationFailureWhenProviderObservesToken|FullyQualifiedName~RouteTimeoutPassesCancellationToNativeAdapter"
```

Expected: `TryInvoke`와 provider signatures가 아직 cancellation token을 받지 않아 compile fail.

- [ ] **Step 4: native adapter interface signature 확장**

`src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs`의 interfaces를 다음 형태로 바꾼다.

```csharp
public interface IDesktopNodeHyperVNativeAdapter
{
    bool TryInvoke(string operation, JsonElement parameters, CancellationToken cancellationToken, out DesktopNodeHyperVOperationResult result);
}

public interface IDesktopNodeHyperVSwitchProvider
{
    IReadOnlyList<DesktopNodeHyperVSwitchInfo> GetSwitches(CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVHostStatusProvider
{
    DesktopNodeHyperVHostStatusData GetStatus(CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmProvider
{
    IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVCheckpointProvider
{
    IReadOnlyList<DesktopNodeHyperVCheckpointInfo> GetCheckpoints(string vmName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVCheckpointMutationProvider
{
    DesktopNodeHyperVCheckpointMutationInfo Invoke(string operation, string vmName, string checkpointName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmPowerStateProvider
{
    DesktopNodeHyperVVmPowerStateInfo Invoke(string operation, string vmName, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmCreateProvider
{
    DesktopNodeHyperVVmCreateInfo Invoke(DesktopNodeHyperVVmCreateRequest request, CancellationToken cancellationToken);
}

public interface IDesktopNodeHyperVVmDeleteProvider
{
    DesktopNodeHyperVVmDeleteInfo Invoke(string vmName, CancellationToken cancellationToken);
}
```

- [ ] **Step 5: API processor invocation에 token 전달**

`DesktopNodeApiRequestProcessor`의 `InvokeHyperVOperation` signature를 바꾼다.

```csharp
private DesktopNodeHyperVOperationResult InvokeHyperVOperation(
    string operation,
    JsonElement parameters,
    CancellationToken cancellationToken = default)
```

native adapter 호출을 다음으로 바꾼다.

```csharp
nativeAdapter.TryInvoke(operation, parameters, cancellationToken, out var result);
```

`ProcessOneQueuedJobAsync`에서 service token을 전달한다.

```csharp
var result = await Task.Run(
    () => InvokeHyperVOperation(started.Operation, started.Parameters, cancellationToken),
    cancellationToken).ConfigureAwait(false);
```

`HandleCoreWithRouteTimeout`은 route timeout token을 만들어 `HandleCore`로 전달한다. `HandleCore` signature를 `HandleCore(DesktopNodeApiRequest request, CancellationToken cancellationToken = default)`로 바꾸고, read routes의 `InvokeHyperVOperation` 호출에 token을 넘긴다.

```csharp
var timeoutCts = new CancellationTokenSource();
timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
var routeTask = Task.Run(() => HandleCore(request, timeoutCts.Token));
if (routeTask.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
{
    timeoutCts.Dispose();
    return routeTask.GetAwaiter().GetResult();
}

timeoutCts.Cancel();
_ = routeTask.ContinueWith(
    task =>
    {
        _ = task.Exception;
        timeoutCts.Dispose();
    },
    TaskContinuationOptions.ExecuteSynchronously);
return RouteTimeoutResponse(timeoutSeconds, hardeningOptions.RetryAfterSeconds);
```

- [ ] **Step 6: native adapter methods/provider 호출 갱신**

`DesktopNodeHyperVNativeAdapter.TryInvoke`와 private methods에 `CancellationToken cancellationToken` parameter를 추가한다. 각 provider 호출은 다음처럼 token을 넘긴다.

```csharp
var switches = switchProvider.GetSwitches(cancellationToken);
var vms = vmProvider.GetVms(cancellationToken);
var checkpoints = checkpointProvider.GetCheckpoints(vm.Name, cancellationToken);
var data = checkpointMutationProvider.Invoke(operation, vmName, checkpointName, cancellationToken);
var data = vmPowerStateProvider.Invoke(operation, vmName, cancellationToken);
var data = vmDeleteProvider.Invoke(vm.Name, cancellationToken);
var data = vmCreateProvider.Invoke(request, cancellationToken);
```

각 method 시작부 또는 WMI 진입 직전에 아래 helper를 호출한다.

```csharp
private static void ThrowIfNativeCanceled(CancellationToken cancellationToken, string operation)
{
    if (!cancellationToken.IsCancellationRequested)
    {
        return;
    }

    throw new DesktopNodeHyperVNativeOperationException(
        "PCV_NATIVE_OPERATION_CANCELED",
        "The native Hyper-V operation was canceled before it completed.",
        $"Cancellation was requested while operation '{operation}' was waiting for Hyper-V/WMI.",
        true);
}
```

`OperationCanceledException` catch를 native adapter private methods에 추가한다.

```csharp
catch (OperationCanceledException)
{
    result = DesktopNodeHyperVOperationResult.Failure(
        operation,
        "PCV_NATIVE_OPERATION_CANCELED",
        "The native Hyper-V operation was canceled before it completed.",
        "Cancellation was requested by the Local API worker or route timeout boundary.",
        true,
        "Check the job status before retrying; Hyper-V may already have accepted part of the operation.");
    return true;
}
```

- [ ] **Step 7: WMI polling wait loops에 cancellation 적용**

`WaitForMethodResult` 계열 methods의 signature에 `CancellationToken cancellationToken`을 추가한다.

```csharp
private static void WaitForMethodResult(
    ManagementBaseObject outParams,
    string operation,
    CancellationToken cancellationToken)
```

polling loop의 `Thread.Sleep(500)`를 다음으로 바꾼다.

```csharp
if (cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500)))
{
    throw new DesktopNodeHyperVNativeOperationException(
        "PCV_NATIVE_OPERATION_CANCELED",
        "The native Hyper-V operation was canceled before it completed.",
        $"Cancellation was requested while waiting for '{operation}' to complete.",
        true);
}
```

모든 caller를 다음 형태로 바꾼다.

```csharp
WaitForMethodResult(outParams, "checkpoint.create", cancellationToken);
WaitForMethodResult(outParams, operation, cancellationToken);
WaitForShutdownResult(outParams, vmName, cancellationToken);
```

- [ ] **Step 8: test fixtures signature 갱신**

`src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`와 `src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs`의 모든 fake adapter/provider signature를 새 interface에 맞춘다.

예시:

```csharp
public bool TryInvoke(
    string operation,
    JsonElement parameters,
    CancellationToken cancellationToken,
    out DesktopNodeHyperVOperationResult result)
```

```csharp
public IReadOnlyList<DesktopNodeHyperVVmInfo> GetVms(CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();
    return vms;
}
```

- [ ] **Step 9: cancellation tests 통과 확인**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~NativeVmPowerStateAdapterReturnsCancellationFailureWhenProviderObservesToken|FullyQualifiedName~RouteTimeoutPassesCancellationToNativeAdapter|FullyQualifiedName~ApiHardeningRequestProcessorTests.RouteTimeoutReturnsProblemDetailsWhenNativeRouteExceedsDeadline"
```

Expected: PASS.

- [ ] **Step 10: Task 4 commit**

Run:

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api/DesktopNodeHyperVNativeAdapter.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs src/DesktopNode.Api.Tests/ApiHardeningRequestProcessorTests.cs
git commit -m "feat: add cooperative cancellation to api workers"
```

Expected: commit created.

---

### Task 5: CLI/TUI 출력 설명 문구 가독성 강화

**Files:**
- Modify: `src/DesktopNode.Cli/DesktopNodeCliFormatter.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`
- Modify: `src/DesktopNode.Tui/TuiPoller.cs`
- Modify: `src/DesktopNode.Tui/TuiApplication.cs`
- Modify: `src/DesktopNode.Tui.Tests/TuiStateTests.cs`
- Modify: `src/DesktopNode.Tui.Tests/TuiApplicationTests.cs`
- Modify: `src/DesktopNode.Tui/README.md`

- [ ] **Step 1: CLI readable error 실패 테스트 추가**

`src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`의 `ReturnsFailureForApiProblemJson` body를 다음으로 바꾼다.

```csharp
"{\"code\":\"PCV_RATE_LIMIT_EXCEEDED\",\"message\":\"The Local API request limit was exceeded.\",\"detail\":\"The current client identity exceeded the request window.\",\"recommended_action\":\"Wait for Retry-After, then retry with a lower request rate.\"}"
```

같은 테스트에 assertions를 추가한다.

```csharp
Assert.Contains("The Local API request limit was exceeded.", result.StandardError);
Assert.Contains("The current client identity exceeded the request window.", result.StandardError);
Assert.Contains("Wait for Retry-After", result.StandardError);
```

- [ ] **Step 2: CLI formatter 구현**

`src/DesktopNode.Cli/DesktopNodeCliFormatter.cs`의 `FormatProblem`을 다음으로 바꾼다.

```csharp
public static string FormatProblem(DesktopNodeCliTransportResponse response)
{
    if (TryParseProblem(response.Body, out var code, out var message, out var detail, out var recommendedAction))
    {
        var lines = new List<string> { code + ": " + message };
        if (!string.IsNullOrWhiteSpace(detail))
        {
            lines.Add("Detail: " + detail);
        }

        if (!string.IsNullOrWhiteSpace(recommendedAction))
        {
            lines.Add("Next action: " + recommendedAction);
        }

        return string.Join(Environment.NewLine, lines);
    }

    return $"PCV_CLI_HTTP_{response.StatusCode}: {response.Body.Trim()}";
}
```

`TryParseProblem` signature와 body를 다음 구조로 바꾼다.

```csharp
private static bool TryParseProblem(
    string body,
    out string code,
    out string message,
    out string? detail,
    out string? recommendedAction)
{
    code = "PCV_CLI_API_ERROR";
    message = body.Trim();
    detail = null;
    recommendedAction = null;

    try
    {
        using var document = JsonDocument.Parse(body);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var root = document.RootElement;
        var source = root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object
            ? error
            : root;

        if (source.TryGetProperty("code", out var codeValue))
        {
            code = codeValue.GetString() ?? code;
        }

        if (source.TryGetProperty("message", out var messageValue))
        {
            message = messageValue.GetString() ?? message;
        }
        else if (source.TryGetProperty("detail", out var detailAsMessage))
        {
            message = detailAsMessage.GetString() ?? message;
        }

        if (source.TryGetProperty("detail", out var detailValue))
        {
            detail = detailValue.GetString();
        }

        if (source.TryGetProperty("recommended_action", out var recommendedActionValue))
        {
            recommendedAction = recommendedActionValue.GetString();
        }

        return code != "PCV_CLI_API_ERROR" || message != body.Trim();
    }
    catch (JsonException)
    {
        return false;
    }
}
```

- [ ] **Step 3: TUI route message 실패 테스트 추가**

`src/DesktopNode.Tui.Tests/TuiStateTests.cs`의 problem response body에 `recommended_action`을 포함하는 새 assertion을 추가한다.

```csharp
var response = new TuiTransportResponse(
    429,
    "application/problem+json",
    "{\"code\":\"PCV_RATE_LIMIT_EXCEEDED\",\"message\":\"Too many requests.\",\"detail\":\"The current client identity exceeded the request window.\",\"recommended_action\":\"Wait for Retry-After, then retry.\"}",
    new Dictionary<string, string> { ["Retry-After"] = "7" });
```

그리고 route error message assertion을 추가한다.

```csharp
Assert.Contains("Wait for Retry-After", route.ErrorMessage);
```

- [ ] **Step 4: TUI poller readable message 구현**

`src/DesktopNode.Tui/TuiPoller.cs`의 `TryReadProblemDetails`를 `recommended_action`까지 읽도록 바꾼다.

```csharp
private static (string? Code, string? Message) TryReadProblemDetails(string body)
{
    if (string.IsNullOrWhiteSpace(body))
    {
        return (null, null);
    }

    try
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var source = root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object
            ? error
            : root;
        var code = ReadString(source, "code");
        var message = ReadString(source, "message") ?? ReadString(source, "detail") ?? ReadString(source, "title");
        var detail = ReadString(source, "detail");
        var recommendedAction = ReadString(source, "recommended_action");

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(message))
        {
            parts.Add(message);
        }

        if (!string.IsNullOrWhiteSpace(detail) && !string.Equals(detail, message, StringComparison.Ordinal))
        {
            parts.Add(detail);
        }

        if (!string.IsNullOrWhiteSpace(recommendedAction))
        {
            parts.Add("Next action: " + recommendedAction);
        }

        return (code, parts.Count == 0 ? null : string.Join(" ", parts));
    }
    catch (JsonException)
    {
        return (null, null);
    }
}
```

- [ ] **Step 5: TUI mutation failure message 구현**

`src/DesktopNode.Tui/TuiApplication.cs`의 `ReadProblem` helper를 `recommended_action`까지 반환하도록 바꾼다.

```csharp
private static (string? Code, string? Message, string? RecommendedAction) ReadProblem(string body)
```

source selection은 `TuiPoller`와 같은 구조를 사용한다. `MutationFailureMessage`는 다음 형태로 바꾼다.

```csharp
private static string MutationFailureMessage(TuiTransportResponse response)
{
    var (code, message, recommendedAction) = ReadProblem(response.Body);
    code ??= "PCV_TUI_MUTATION_FAILED";
    message ??= "Mutation request returned HTTP " + response.StatusCode + ".";
    recommendedAction ??= "Check the Local API response, then retry the action.";

    return "Action failed: mutation request failed." + "\n" +
        "Next action: " + TuiWidgets.Redact(recommendedAction) + "\n" +
        "code=" + code + " message=" + TuiWidgets.Redact(message) + "\n";
}
```

- [ ] **Step 6: CLI/TUI tests 통과 확인**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --filter "FullyQualifiedName~DesktopNodeCliApplicationTests.ReturnsFailureForApiProblemJson"
dotnet test src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj --filter "FullyQualifiedName~TuiStateTests|FullyQualifiedName~TuiApplicationTests"
```

Expected: PASS.

- [ ] **Step 7: TUI README 한국어/원문 code 혼합 설명 갱신**

`src/DesktopNode.Tui/README.md`의 failure output section에 다음 문장을 추가한다.

```markdown
Failure output keeps the stable `PCV_*` code and adds a short next action when the API returns `recommended_action`. Token values, passwords, Authorization headers, and protected token blobs remain redacted.
```

문서의 기존 언어가 영어이므로 README의 주변 문맥은 영어를 유지한다. 새 evidence/docs는 한국어로 작성한다.

- [ ] **Step 8: Task 5 commit**

Run:

```powershell
git add src/DesktopNode.Cli/DesktopNodeCliFormatter.cs src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs src/DesktopNode.Tui/TuiPoller.cs src/DesktopNode.Tui/TuiApplication.cs src/DesktopNode.Tui.Tests/TuiStateTests.cs src/DesktopNode.Tui.Tests/TuiApplicationTests.cs src/DesktopNode.Tui/README.md
git commit -m "feat: improve operator error readability"
```

Expected: commit created.

---

### Task 6: Packaging smoke runner and Korean verification docs

**Files:**
- Create: `packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1`
- Create: `packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

- [ ] **Step 1: Pester contract 실패 테스트 추가**

새 파일 `packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1`을 만든다.

```powershell
Set-StrictMode -Version Latest

Describe 'API host job hardening installed smoke runner' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ScriptPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1'
    }

    It 'ships a runner with body cap, worker, cancellation, regression, and redaction evidence fields' {
        Test-Path -LiteralPath $script:ScriptPath | Should -BeTrue
        $content = Get-Content -LiteralPath $script:ScriptPath -Raw

        $content | Should -Match 'PCV_REQUEST_BODY_TOO_LARGE'
        $content | Should -Match '/api/v1/runtime/policy'
        $content | Should -Match '/api/v1/jobs'
        $content | Should -Match '/api/v1/diagnostics/bundles'
        $content | Should -Match '/api/v1/console/capabilities'
        $content | Should -Match 'worker_responsiveness'
        $content | Should -Match 'cooperative_cancellation_scope'
        $content | Should -Match 'wmi_abort_claim'
        $content | Should -Match 'token_value_observed\s*=\s*\$false|token_value_observed'
        $content | Should -Match 'public_trusted_signing'
        $content | Should -Match 'external_stable_publication'
    }
}
```

- [ ] **Step 2: Pester 실패 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1' -Output Detailed"
```

Expected: script file이 없어 FAIL.

- [ ] **Step 3: installed smoke runner 생성**

`packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1`을 생성한다.

```powershell
[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("api-host-job-hardening-installed-evidence-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [string]$ApiBaseUri = 'http://127.0.0.1:7777',
    [string]$BearerToken = '',
    [int]$OversizedBodyBytes = 2097152,
    [int]$ResponsivenessTimeoutSeconds = 20,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$artifactRootFull = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $Value | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Invoke-PcvSmokeRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Body,
        [int]$TimeoutSec = 20
    )

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
        $headers['Authorization'] = "Bearer $BearerToken"
    }

    $request = @{
        Method = $Method
        Uri = ($ApiBaseUri.TrimEnd('/') + $Path)
        Headers = $headers
        TimeoutSec = $TimeoutSec
        ErrorAction = 'Stop'
    }
    if ($null -ne $Body) {
        $request.Body = $Body
        $request.ContentType = 'application/json'
    }
    if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
        $request.UseBasicParsing = $true
    }

    $started = Get-Date
    try {
        $response = Invoke-WebRequest @request
        $finished = Get-Date
        [pscustomobject][ordered]@{
            ok = $true
            method = $Method
            path = $Path
            status_code = [int]$response.StatusCode
            duration_ms = [int]($finished - $started).TotalMilliseconds
            body = [string]$response.Content
            error_code = $null
        }
    }
    catch {
        $finished = Get-Date
        $statusCode = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
        $body = ''
        try {
            if ($_.Exception.Response) {
                $stream = $_.Exception.Response.GetResponseStream()
                if ($stream) {
                    $reader = [System.IO.StreamReader]::new($stream)
                    $body = $reader.ReadToEnd()
                    $reader.Dispose()
                }
            }
        }
        catch {
        }

        [pscustomobject][ordered]@{
            ok = $false
            method = $Method
            path = $Path
            status_code = $statusCode
            duration_ms = [int]($finished - $started).TotalMilliseconds
            body = $body
            error_code = if ($body -match 'PCV_[A-Z0-9_]+') { $Matches[0] } else { $null }
        }
    }
}

function Get-ServiceSnapshot {
    $service = Get-CimInstance Win32_Service -Filter "Name='$ServiceName'" -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        return $null
    }

    [pscustomobject][ordered]@{
        name = $service.Name
        state = $service.State
        start_mode = $service.StartMode
        process_id = $service.ProcessId
        path_name = $service.PathName
    }
}

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$beforeService = Get-ServiceSnapshot

if ($DryRun) {
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $true
        actual_execution = 'dry-run'
        artifact_root = $artifactRootFull
        service_name = $ServiceName
        token_value_observed = $false
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
    }
    Write-JsonFile -Path $summaryPath -Value $summary
    Write-Output ($summary | ConvertTo-Json -Depth 20)
    exit 0
}

$oversizedBody = '{"payload":"' + ('x' * [Math]::Max(1, $OversizedBodyBytes)) + '"}'
$bodyCap = Invoke-PcvSmokeRequest -Method 'POST' -Path '/api/v1/diagnostics/bundles' -Body $oversizedBody -TimeoutSec 20
$runtimePolicy = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/runtime/policy' -Body $null -TimeoutSec 20
$jobs = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/jobs' -Body $null -TimeoutSec 20
$diagnosticsList = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/diagnostics/bundles' -Body $null -TimeoutSec 20
$consoleCapabilities = Invoke-PcvSmokeRequest -Method 'GET' -Path '/api/v1/console/capabilities' -Body $null -TimeoutSec 20

$workerResponsiveness = [pscustomobject][ordered]@{
    runtime_policy_status_code = $runtimePolicy.status_code
    runtime_policy_duration_ms = $runtimePolicy.duration_ms
    jobs_status_code = $jobs.status_code
    jobs_duration_ms = $jobs.duration_ms
    threshold_ms = $ResponsivenessTimeoutSeconds * 1000
    observed_nonblocking = ($runtimePolicy.duration_ms -lt ($ResponsivenessTimeoutSeconds * 1000)) -and ($jobs.duration_ms -lt ($ResponsivenessTimeoutSeconds * 1000))
}

$afterService = Get-ServiceSnapshot
$ok = $bodyCap.status_code -eq 413 -and
    $bodyCap.body -match 'PCV_REQUEST_BODY_TOO_LARGE' -and
    $runtimePolicy.status_code -eq 200 -and
    $jobs.status_code -eq 200 -and
    $null -ne $afterService -and
    $afterService.state -eq 'Running'

$summary = [pscustomobject][ordered]@{
    schema_version = 1
    ok = [bool]$ok
    actual_execution = 'installed-service-listener-smoke'
    artifact_root = $artifactRootFull
    service_name = $ServiceName
    api_base_uri = $ApiBaseUri
    before_service = $beforeService
    after_service = $afterService
    body_cap = $bodyCap
    runtime_policy = $runtimePolicy
    jobs = $jobs
    diagnostics_list = $diagnosticsList
    console_capabilities = $consoleCapabilities
    worker_responsiveness = $workerResponsiveness
    cooperative_cancellation_scope = 'worker-token-and-route-timeout-token-code-level; installed smoke does not claim WMI abort'
    wmi_abort_claim = 'not-claimed'
    host_mutation_performed = $false
    token_value_observed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
}

Write-JsonFile -Path $summaryPath -Value $summary
Write-Output ($summary | ConvertTo-Json -Depth 20)
if (-not $ok) {
    exit 1
}
```

- [ ] **Step 4: Pester contract 통과 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 5: README에 실행 명령 추가**

`packaging/windows-desktop-node/README.md`에 다음 section을 추가한다.

```markdown
### API/Host Job Hardening Installed Smoke

관리자 opt-in 설치본 검증은 다음 명령으로 실행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -ArtifactRoot artifacts/api-host-job-hardening-installed-evidence-20260511
```

이 smoke는 `413` body cap, `GET /api/v1/runtime/policy`, `GET /api/v1/jobs`, diagnostics list, console capabilities, service running 상태를 확인한다. `wmi_abort_claim`은 `not-claimed`이며 public trusted signing과 external stable publication도 claim하지 않는다.
```

- [ ] **Step 6: verification policy에 기준 추가**

`docs/DEVELOPMENT_VERIFICATION_POLICY.md`에 다음 bullet을 추가한다.

```markdown
- API/Host job hardening 변경은 `dotnet test src/DesktopNode.sln`, 관련 Host/Api/Runtime xUnit, `PcvApiHostJobHardeningInstalledSmoke.Tests.ps1`, 그리고 설치본 관리자 opt-in evidence 문서를 요구한다. Evidence에는 `host_mutation_performed`, `cooperative_cancellation_scope`, `wmi_abort_claim`, `public_trusted_signing`, `external_stable_publication`을 명시한다.
```

- [ ] **Step 7: Task 6 commit**

Run:

```powershell
git add packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1 packaging/windows-desktop-node/README.md docs/DEVELOPMENT_VERIFICATION_POLICY.md
git commit -m "test: add api host job hardening installed smoke"
```

Expected: commit created.

---

### Task 7: Installed admin evidence 실행과 보고서 작성

**Files:**
- Create: `docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md`
- Read/Use: `artifacts/api-host-job-hardening-installed-evidence-20260511/summary.json`

- [ ] **Step 1: 전체 code-level 검증 실행**

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1' -Output Detailed"
git diff --check
```

Expected: all PASS.

- [ ] **Step 2: 설치본 smoke 실행**

관리자 권한 PowerShell에서 실행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 -ArtifactRoot artifacts/api-host-job-hardening-installed-evidence-20260511
```

Expected:

- `artifacts/api-host-job-hardening-installed-evidence-20260511/summary.json` 생성
- `ok: true`
- `body_cap.status_code: 413`
- `body_cap.error_code: PCV_REQUEST_BODY_TOO_LARGE`
- `runtime_policy.status_code: 200`
- `jobs.status_code: 200`
- `after_service.state: Running`
- `token_value_observed: false`
- `wmi_abort_claim: not-claimed`
- `public_trusted_signing: not-claimed`
- `external_stable_publication: not-claimed`

- [ ] **Step 3: evidence 보고서 작성**

`summary.json` 값을 읽어 `docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md`를 생성한다.

```powershell
$summaryPath = 'artifacts/api-host-job-hardening-installed-evidence-20260511/summary.json'
$summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
$evidencePath = 'docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md'
$content = @"
# API/Host Job Hardening Installed Evidence

작성일: 2026-05-11

## 결론

설치본 Local API/Host listener에서 request body cap, background worker responsiveness, 기본 API regression smoke를 확인했다.

## 객관적 근거

- Artifact root: ``artifacts/api-host-job-hardening-installed-evidence-20260511``
- Summary: ``artifacts/api-host-job-hardening-installed-evidence-20260511/summary.json``
- ``body_cap.status_code``: ``$($summary.body_cap.status_code)``
- ``body_cap.error_code``: ``$($summary.body_cap.error_code)``
- ``runtime_policy.status_code``: ``$($summary.runtime_policy.status_code)``
- ``jobs.status_code``: ``$($summary.jobs.status_code)``
- ``after_service.state``: ``$($summary.after_service.state)``
- ``token_value_observed``: ``$($summary.token_value_observed)``

## 판단

- 판단: cap 초과 API body는 processor 진입 전에 거부됐다.
- 관찰: 거부 후 service 상태는 ``$($summary.after_service.state)``였다.
- 권장 조치: 정상 API 호출은 1 MiB 기본 cap 안에서 유지하고, 더 큰 payload가 필요한 운영 환경은 ``--max-request-body-bytes``를 지원 범위 안에서 조정한다.

## Worker / Cancellation Boundary

- ``worker_responsiveness.observed_nonblocking``: ``$($summary.worker_responsiveness.observed_nonblocking)``
- ``cooperative_cancellation_scope``: ``$($summary.cooperative_cancellation_scope)``
- ``wmi_abort_claim``: ``$($summary.wmi_abort_claim)``

이 evidence는 WMI host mutation 강제 abort 또는 rollback을 주장하지 않는다.

## Scope Boundary

- ``host_mutation_performed``: ``$($summary.host_mutation_performed)``
- ``public_trusted_signing``: ``$($summary.public_trusted_signing)``
- ``external_stable_publication``: ``$($summary.external_stable_publication)``
- winget public submission: ``out-of-scope``
- public stable installer URL: ``out-of-scope``
"@
Set-Content -LiteralPath $evidencePath -Value $content -Encoding UTF8
```

- [ ] **Step 4: evidence 문서 값 검증**

Run:

```powershell
$badMarkers = @(('<' + 'summary'), ('T' + 'BD'), ('TO' + 'DO'), ('PLACE' + 'HOLDER'))
foreach ($marker in $badMarkers) {
    Select-String -LiteralPath 'docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md' -Pattern $marker -SimpleMatch
}
rg -n "PCV_REQUEST_BODY_TOO_LARGE|not-claimed|out-of-scope|Running" docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md
```

Expected: first command has no matches. Second command shows the expected evidence claims.

- [ ] **Step 5: Task 7 commit**

Run:

```powershell
git add docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md artifacts/api-host-job-hardening-installed-evidence-20260511/summary.json
git commit -m "docs: record api host job hardening evidence"
```

Expected: commit created. `git status --short`에는 unrelated untracked file이 남아도 된다.

---

### Task 8: Final verification and review

**Files:**
- All touched files from Tasks 1-7

- [ ] **Step 1: 전체 테스트 실행**

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: PASS.

- [ ] **Step 2: scope boundary grep**

Run:

```powershell
rg -n "KVM|libvirt|LXC|ZFS|OVS|OVN|purecvisorsd|purecvisor-single|PowerShell helper fallback|public trusted signing.*PASS|external stable publication.*PASS" src packaging docs/ga-ready/evidence/api-host-job-hardening-installed-evidence-2026-05-11.md
```

Expected: no new runtime implementation claim. Historical docs may contain preserved terms; touched files must not add Linux runtime or public publication claim.

- [ ] **Step 3: git status 확인**

Run:

```powershell
git status --short
```

Expected: only intended changes are staged/committed. `docs/ga-ready/evidence/frontend-backend-auth-console-live-smoke-2026-05-10.md` remains untouched unless the user separately asked to include it.

- [ ] **Step 4: final summary 작성**

Final response에는 다음을 포함한다.

- 구현 요약: body cap, worker 분리, cancellation, policy 단일화, readability, installed evidence
- 객관 근거: test commands와 PASS, installed artifact path, evidence doc path
- scope boundary: public trusted signing/external publication not claimed
- 남은 risk: WMI 강제 abort/rollback not claimed

---

## Self Review

- Spec coverage:
  - `request body cap`: Task 1, Task 6, Task 7.
  - `job policy 단일화`: Task 2.
  - `worker 분리`: Task 3.
  - `cooperative cancellation`: Task 4.
  - `출력 설명 문구 가독성 강화`: Task 2와 Task 5.
  - installed admin evidence: Task 6과 Task 7.
- Residual marker scan:
  - Task 7은 `summary.json`을 읽어 evidence 문서를 생성한다. Step 4에서 잔여 표식과 핵심 evidence claim을 검증한다.
- Type consistency:
  - `DesktopNodeApiError.RecommendedAction`, `DesktopNodeHostOptions.MaxRequestBodyBytes`, `IDesktopNodeHyperVNativeAdapter.TryInvoke(..., CancellationToken, out ...)`, `RunWorkerLoopAsync` 이름을 전 task에서 동일하게 사용한다.
