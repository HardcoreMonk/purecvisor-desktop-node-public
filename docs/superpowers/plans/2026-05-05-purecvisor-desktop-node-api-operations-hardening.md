# API Operations Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Local API response와 queued job에 request/correlation id를 additive-only로 추가하고 Web Console Activity에서 표시한다.

**Architecture:** `DesktopNodeApiRequestProcessor`의 공통 `Body()`/`Failure()`/`JobData()` 경로를 확장한다. `DesktopNodeApiRequest`는 optional request id를 받되 기존 생성자 호출은 유지한다. Web Console은 field가 있을 때만 request/correlation id를 표시한다.

**Tech Stack:** C#/.NET 10 xUnit, TypeScript-owned `web/src/served-app.ts`, generated `web/app.js`, Web Pester, Node `vm` browser fixture, Korean Markdown docs.

**Implementation Status:** implemented in `a7b3b33 Add API request job correlation`.

**Implementation Evidence:** request id success/failure tests, queued job request/correlation tests, persisted retry correlation test, Web Activity static assertion, browser fixture rendered `req-browser-fixture`, served asset parity, and API/Web test loop passed on 2026-05-05. This evidence is internal `AllowUnsignedDev` development evidence and is not public trusted signing or external stable publication evidence.

---

## File Structure

- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
  - Extend `DesktopNodeApiRequest` with optional request id.
  - Generate request id when missing.
  - Add top-level `request_id` to every JSON response.
  - Add `request_id` and `correlation_id` to new queued jobs and job snapshots.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Add tests for request id on success/failure.
  - Add tests for queued job request/correlation fields.
  - Add old job store v1 compatibility test.
- Modify: `web/src/served-app.ts`
  - Show request/correlation id in Operator Activity when present.
- Generate: `web/app.js`
  - Built from `web/src/served-app.ts`.
- Modify: `web/scripts/verify-browser-fixture.mjs`
  - Add fixture request/correlation ids and rendered assertion.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - Add static assertions for request/correlation display and no secrets.
- Modify: `docs/USER_GUIDE.md`
  - Document request/correlation id in Activity.
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
  - Mark API Operations Hardening status after implementation.

## Task 1: API RED Tests

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

- [x] **Step 1: Add success response request id test**

Add near runtime policy tests:

```csharp
[Fact]
public void RuntimePolicyResponseIncludesRequestId()
{
    var processor = DesktopNodeApiRequestProcessor.CreateDefault();

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy", RequestId: "req-test-runtime"));

    Assert.Equal(200, response.StatusCode);
    using var document = JsonDocument.Parse(response.Body);
    Assert.Equal("req-test-runtime", document.RootElement.GetProperty("request_id").GetString());
    Assert.Equal("runtime.policy", document.RootElement.GetProperty("operation").GetString());
}
```

- [x] **Step 2: Add failure response request id test**

```csharp
[Fact]
public void FailureResponseIncludesGeneratedRequestId()
{
    var processor = DesktopNodeApiRequestProcessor.CreateDefault();

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/not-found"));

    Assert.Equal(404, response.StatusCode);
    using var document = JsonDocument.Parse(response.Body);
    var requestId = document.RootElement.GetProperty("request_id").GetString();
    Assert.StartsWith("req-", requestId);
    Assert.Equal("PCV_ROUTE_NOT_FOUND", document.RootElement.GetProperty("error").GetProperty("code").GetString());
}
```

- [x] **Step 3: Add queued job request/correlation test**

```csharp
[Fact]
public void QueuedJobStoresRequestAndCorrelationIds()
{
    var processor = DesktopNodeApiRequestProcessor.CreateDefault();

    var create = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/vms/alpha/start",
        RequestId: "req-start-alpha"));

    Assert.Equal(202, create.StatusCode);
    using var createDocument = JsonDocument.Parse(create.Body);
    var data = createDocument.RootElement.GetProperty("data");
    Assert.Equal("req-start-alpha", createDocument.RootElement.GetProperty("request_id").GetString());
    Assert.Equal("req-start-alpha", data.GetProperty("request_id").GetString());
    Assert.Equal(data.GetProperty("job_id").GetString(), data.GetProperty("correlation_id").GetString());
}
```

- [x] **Step 4: Run RED**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~RuntimePolicyResponseIncludesRequestId|FullyQualifiedName~FailureResponseIncludesGeneratedRequestId|FullyQualifiedName~QueuedJobStoresRequestAndCorrelationIds"
```

Expected: FAIL because request/correlation fields are absent.

## Task 2: API Implementation

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`

- [x] **Step 1: Extend request record**

Change:

```csharp
public sealed record DesktopNodeApiRequest(string Method, string Path, string? Body = null, string? RequestId = null);
```

- [x] **Step 2: Thread request id through `HandleCore`**

At the beginning of `HandleCore`:

```csharp
var requestId = NormalizeRequestId(request.RequestId);
```

Change common helper calls in this file so `Body()` and `Failure()` receive `requestId`. Keep changes local to `DesktopNodeApiRequestProcessor`.

- [x] **Step 3: Add helper**

```csharp
private static string NormalizeRequestId(string? requestId)
{
    return string.IsNullOrWhiteSpace(requestId)
        ? "req-" + Guid.NewGuid().ToString("N")
        : requestId.Trim();
}
```

- [x] **Step 4: Extend `Body()`**

Change signature:

```csharp
private static object Body(bool ok, string operation, object? data, DesktopNodeApiError? error, string requestId)
```

Return:

```csharp
return new SortedDictionary<string, object?>
{
    ["data"] = data,
    ["error"] = error,
    ["ok"] = ok,
    ["operation"] = operation,
    ["request_id"] = requestId
};
```

- [x] **Step 5: Extend jobs**

Add `RequestId` and `CorrelationId` to `DesktopNodeApiJob`. `CreateJob()` accepts `requestId`; `CorrelationId` is the new job id unless a retry carries a parent chain. `JobData()` includes:

```csharp
["correlation_id"] = job.CorrelationId,
["request_id"] = job.RequestId,
```

Old job store load sets missing request/correlation fields to null or job id without throwing.

- [x] **Step 6: Run focused GREEN**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~RuntimePolicyResponseIncludesRequestId|FullyQualifiedName~FailureResponseIncludesGeneratedRequestId|FullyQualifiedName~QueuedJobStoresRequestAndCorrelationIds"
```

Expected: PASS.

- [x] **Step 7: Run API suite**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj
```

Expected: PASS.

## Task 3: Web Activity Display

**Files:**
- Modify: `web/src/served-app.ts`
- Generate: `web/app.js`
- Modify: `web/scripts/verify-browser-fixture.mjs`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [x] **Step 1: Add Web static RED**

Add Pester assertion:

```powershell
$app | Should -Match 'formatCorrelationValue'
$app | Should -Match 'request_id'
$app | Should -Match 'correlation_id'
```

- [x] **Step 2: Add display helper**

In `web/src/served-app.ts`:

```javascript
function formatCorrelationValue(job) {
  return job?.request_id || job?.correlation_id || '-';
}
```

Add to activity row detail:

```html
<div class="muted">${escapeHtml(formatCorrelationValue(job))}</div>
```

- [x] **Step 3: Update browser fixture**

Add `request_id` and `correlation_id` to fixture job data and require rendered `req-browser-fixture`.

- [x] **Step 4: Regenerate and verify Web**

Run:

```powershell
npm run build:served --prefix web
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
git diff --check
```

Expected: PASS.

## Task 4: Docs and Commit

**Files:**
- Modify: `docs/USER_GUIDE.md`
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
- Modify: `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-api-operations-hardening.md`

- [x] **Step 1: Update docs**

Document that Activity may show request/correlation id for operator support, and that these are not secrets.

- [x] **Step 2: Run final verification**

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
git diff --check
```

- [x] **Step 3: Commit and push**

Run:

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs web/src/served-app.ts web/app.js web/scripts/verify-browser-fixture.mjs web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "Add API request job correlation"
git add docs/USER_GUIDE.md docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-api-operations-hardening.md
git commit -m "Document API operations hardening"
git push
```

## Self-Review

- Spec coverage: request id, job correlation id, failure body shape, Web Activity display, and old job store compatibility are covered.
- Scope check: HTTP listener header plumbing, public CORS, public metrics, Event Log/firewall/trust-store/LAN/MSI/service mutation, config/job-store migration apply remain excluded.
- Mutation boundary: This plan is code-level/API/Web only and does not execute host mutation.
- Placeholder scan: 이 plan에는 미확정 자리표시자가 없다.
