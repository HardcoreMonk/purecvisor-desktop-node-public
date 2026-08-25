# Operator Ops Console P0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node Web Console에 P0 Operator Activity와 Troubleshooting Center를 추가하고, server-side job activity를 읽기 위한 `GET /api/v1/jobs` read-only route를 구현한다.

**Architecture:** P0는 read-only 운영 visibility slice다. Local API는 기존 in-memory/job store snapshot에서 job list만 읽어 반환하고, Web Console은 server-side job list와 browser-local `Tracked Jobs`를 함께 보여준다. Troubleshooting Center는 host/runtime policy/error guide만 표시하며 Hyper-V, service, MSI, firewall, trust-store, LAN, Event Log mutation은 실행하지 않는다.

**Tech Stack:** C#/.NET 10 xUnit, TypeScript-owned `web/src/served-app.ts`, generated `web/app.js`, Node `vm` browser fixture, Pester static asset tests, Korean Markdown docs.

**구현 상태:** `main`에서 `a01a4f5 Add read-only job activity list route`, `5ff3911 Add operator activity and troubleshooting console`로 완료됐다. 2026-05-07에는 문서 상태 정리로 checkbox closure만 반영했다.

---

## File Structure

- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
  - Add `GET /api/v1/jobs` read-only list route.
  - Include `/api/v1/jobs` in job-store blocked diagnostics routing.
  - Preserve existing `GET /api/v1/jobs/{job_id}`, cancel, retry behavior.
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
  - Add RED tests for `job.list`.
  - Add unsupported future job store block test for `GET /api/v1/jobs`.
- Modify: `web/index.html`
  - Add sidebar links and mount points for `Operator Activity` and `Troubleshooting`.
- Modify: `web/src/served-app.ts`
  - Load runtime policy and server job list.
  - Render activity timeline and troubleshooting cards.
  - Keep token values out of DOM and preserve existing `Tracked Jobs`.
- Generate: `web/app.js`
  - Built from `web/src/served-app.ts`.
- Modify: `web/styles.css`
  - Add dense operational styles for activity rows and troubleshooting cards.
- Modify: `web/scripts/verify-browser-fixture.mjs`
  - Add fixture responses for `/api/v1/runtime/policy` and `/api/v1/jobs`.
  - Assert Activity/Troubleshooting render without secret or forbidden host mutation command text.
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - Add static assertions for mount points, read-only routes, render functions, and forbidden term guard.
- Modify: `docs/USER_GUIDE.md`
  - Document Activity and Troubleshooting usage.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Link the P0 umbrella spec and this plan.
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`
  - Mark P0 implementation plan location and keep P1/P2 as follow-up candidates.

## Task 1: Local API RED Tests for `GET /api/v1/jobs`

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

- [x] **Step 1: Add job list success RED test**

Add this test near the existing job runtime tests:

```csharp
[Fact]
public void JobListReturnsReadOnlyServerSideSnapshot()
{
    var processor = DesktopNodeApiRequestProcessor.CreateDefault();

    var create = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/vms",
        """{"name":"activity-vm","iso_path":"D:\\iso\\activity.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}"""));

    Assert.Equal(202, create.StatusCode);

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));

    Assert.Equal(200, response.StatusCode);
    Assert.Equal("application/json", response.ContentType);

    using var document = JsonDocument.Parse(response.Body);
    var root = document.RootElement;
    Assert.True(root.GetProperty("ok").GetBoolean());
    Assert.Equal("job.list", root.GetProperty("operation").GetString());
    Assert.Equal(1, root.GetProperty("data").GetProperty("count").GetInt32());

    var job = root.GetProperty("data").GetProperty("jobs")[0];
    Assert.Equal("vm.create", job.GetProperty("operation").GetString());
    Assert.Equal("queued", job.GetProperty("status").GetString());
    Assert.Equal(1, job.GetProperty("attempt").GetInt32());
    Assert.True(job.TryGetProperty("created_at", out _));
    Assert.True(job.TryGetProperty("updated_at", out _));
}
```

- [x] **Step 2: Add future-schema blocked RED test**

Add this test near `JobStoreUnsupportedFutureVersionReturnsBlockedDiagnosticsWithoutQuarantine`:

```csharp
[Fact]
public void JobListBlocksUnsupportedFutureJobStoreWithoutMutation()
{
    var jobStorePath = Path.Combine(Path.GetTempPath(), "pcv-dotnet-api-future-job-list-" + Guid.NewGuid().ToString("N") + ".json");
    try
    {
        File.WriteAllText(jobStorePath, """{"version":99,"jobs":[],"queue":[]}""");
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(jobStorePath: jobStorePath);

        var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/jobs"));

        Assert.Equal(409, response.StatusCode);
        Assert.Contains("PCV_JOB_STORE_SCHEMA_UNSUPPORTED", response.Body);
        Assert.Contains("No quarantine, migration, or job store write was performed", response.Body);
        Assert.True(File.Exists(jobStorePath));
    }
    finally
    {
        if (File.Exists(jobStorePath))
        {
            File.Delete(jobStorePath);
        }
    }
}
```

- [x] **Step 3: Run focused RED**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~JobListReturnsReadOnlyServerSideSnapshot|FullyQualifiedName~JobListBlocksUnsupportedFutureJobStoreWithoutMutation"
```

Expected: FAIL because `GET /api/v1/jobs` currently returns route-not-found and unsupported job-store blocking does not include the collection route.

- [x] **Step 4: Keep RED uncommitted**

Do not commit the failing test state on `main`. Proceed directly to Task 2 and commit after the focused tests pass.

## Task 2: Implement `GET /api/v1/jobs`

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`

- [x] **Step 1: Include the collection route in job-store block detection**

Change `UsesJobStore` so `/api/v1/jobs` is covered:

```csharp
private static bool UsesJobStore(string method, string path, bool isCheckpointDelete, bool isVmDelete)
{
    if (path == "/api/v1/jobs" || path.StartsWith("/api/v1/jobs/", StringComparison.Ordinal))
    {
        return true;
    }

    return method == "POST" && (
        path == "/api/v1/vms" ||
        Regex.IsMatch(path, "^/api/v1/vms/([^/]*)/(start|shutdown|poweroff|restart)$", RegexOptions.CultureInvariant) ||
        Regex.IsMatch(path, "^/api/v1/vms/([^/]*)/checkpoints$", RegexOptions.CultureInvariant) ||
        Regex.IsMatch(path, "^/api/v1/vms/([^/]*)/checkpoints/([^/]*)/restore$", RegexOptions.CultureInvariant)) ||
        isCheckpointDelete ||
        isVmDelete;
}
```

- [x] **Step 2: Add read-only collection route**

Add this branch after the `/api/v1/runtime/policy` block and before the generic `POST` route-not-found branch:

```csharp
if (method == "GET" && path == "/api/v1/jobs")
{
    var jobRows = jobs.Values
        .OrderByDescending(job => job.UpdatedAt)
        .ThenByDescending(job => job.CreatedAt)
        .Select(JobData)
        .ToArray();

    return Json(200, Body(true, "job.list", JsonFromObject(new SortedDictionary<string, object?>
    {
        ["count"] = jobRows.Length,
        ["jobs"] = jobRows
    }), null));
}
```

- [x] **Step 3: Run focused GREEN**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~JobListReturnsReadOnlyServerSideSnapshot|FullyQualifiedName~JobListBlocksUnsupportedFutureJobStoreWithoutMutation"
```

Expected: PASS.

- [x] **Step 4: Run API suite**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj
```

Expected: PASS. No Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, or real OS mutation runs.

- [x] **Step 5: Commit API route**

Run:

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs
git commit -m "Add read-only job activity list route"
```

Expected: one API implementation commit.

## Task 3: Web Static RED Tests and Mount Points

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`
- Modify: `web/index.html`

- [x] **Step 1: Add static tests for Activity and Troubleshooting**

Add a new Pester test after the tracked job history test:

```powershell
It 'declares operator activity and troubleshooting console surfaces' {
    $index = Get-Content -LiteralPath $script:IndexPath -Raw
    $app = Get-Content -LiteralPath $script:AppPath -Raw

    $index | Should -Match 'id="activity"'
    $index | Should -Match 'id="activity-panel"'
    $index | Should -Match 'id="troubleshooting"'
    $index | Should -Match 'id="troubleshooting-panel"'
    $app | Should -Match '/api/v1/runtime/policy'
    $app | Should -Match '/api/v1/jobs'
    $app | Should -Match 'loadRuntimePolicy'
    $app | Should -Match 'loadServerJobs'
    $app | Should -Match 'renderActivity'
    $app | Should -Match 'renderTroubleshooting'
    $app | Should -Match 'PCV_JOB_STORE_SCHEMA_UNSUPPORTED'
    $app | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
}
```

- [x] **Step 2: Extend forbidden runtime guard**

In the existing TypeScript source guard, keep the current command literals and add Linux runtime terms:

```powershell
($apiTypes + $viewModel + $appSource + $servedSource) | Should -Not -Match 'Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule'
($apiTypes + $viewModel + $appSource + $servedSource) | Should -Not -Match 'journalctl|libvirt|KVM|ZFS|OVS|OVN|purecvisorsd'
```

- [x] **Step 3: Run Web RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
```

Expected: FAIL because mount points and render/load functions are not implemented.

- [x] **Step 4: Add HTML mount points**

In `web/index.html`, extend the sidebar:

```html
<a href="#activity">Activity</a>
<a href="#troubleshooting">Troubleshooting</a>
```

Add this section after `#jobs`:

```html
<section id="activity" class="section">
  <div class="section-header">
    <div>
      <p class="eyebrow">Operations</p>
      <h2>Operator Activity</h2>
    </div>
  </div>
  <div id="activity-panel" class="activity-panel"></div>
</section>

<section id="troubleshooting" class="section">
  <div class="section-header">
    <div>
      <p class="eyebrow">Support</p>
      <h2>Troubleshooting</h2>
    </div>
  </div>
  <div id="troubleshooting-panel" class="troubleshooting-panel"></div>
</section>
```

Expected: Pester still fails until `web/src/served-app.ts` and generated `web/app.js` are updated.

## Task 4: Web Console Activity and Troubleshooting Implementation

**Files:**
- Modify: `web/src/served-app.ts`
- Generate: `web/app.js`
- Modify: `web/styles.css`

- [x] **Step 1: Extend UI state**

Add these fields to the top-level `state` object:

```javascript
runtimePolicy: null,
serverJobs: [],
activityError: null,
```

- [x] **Step 2: Add read functions**

Add these functions near `loadVms()`:

```javascript
async function loadRuntimePolicy() {
  state.runtimePolicy = await apiFetch('/api/v1/runtime/policy');
}

async function loadServerJobs() {
  state.activityError = null;
  try {
    state.serverJobs = asArray(await apiFetch('/api/v1/jobs'));
  } catch (error) {
    state.activityError = normalizeError(error);
    state.serverJobs = [];
  }
}
```

- [x] **Step 3: Load read-only operations data during refresh**

Change `refreshAll()` to include runtime policy and server jobs:

```javascript
await Promise.all([loadHost(), loadVms(), loadRuntimePolicy(), loadServerJobs(), pollTrackedJobs()]);
```

Expected: `/api/v1/jobs` failure degrades only `state.activityError` because `loadServerJobs()` catches its own error.

- [x] **Step 4: Add activity row helpers**

Add these helpers before `renderJobs()`:

```javascript
function getJobTime(job) {
  return job?.updated_at || job?.created_at || job?.canceled_at || '-';
}

function formatJobDetail(job) {
  if (job?.error?.code) {
    return `${job.error.code}: ${job.error.message || 'Job failed'}`;
  }
  if (job?.result?.operation) {
    return `result=${job.result.operation}`;
  }
  if (job?.retry_of) {
    return `retry of ${job.retry_of}`;
  }
  return `attempt=${job?.attempt || 1}`;
}

function buildActivityRows() {
  const serverJobs = asArray(state.serverJobs);
  const serverIds = new Set(serverJobs.map((job) => job.job_id).filter(Boolean));
  const rows = serverJobs.map((job) => ({ source: 'server', job }));
  for (const job of state.trackedJobs) {
    if (!serverIds.has(job.job_id)) {
      rows.push({ source: 'browser', job });
    }
  }
  return rows.slice(0, JOB_HISTORY_LIMIT);
}
```

- [x] **Step 5: Add `renderActivity()`**

Add this function before `renderError()`:

```javascript
function renderActivity() {
  const rows = buildActivityRows();
  const degraded = state.activityError
    ? `<div class="activity-warning"><strong>${escapeHtml(state.activityError.code)}</strong> ${escapeHtml(state.activityError.message)}</div>`
    : '';

  if (rows.length === 0) {
    els.activityPanel.innerHTML = `${degraded}<p class="muted">No server or browser job activity has been loaded.</p>`;
    return;
  }

  els.activityPanel.innerHTML = degraded + rows.map(({ source, job }) => {
    const status = String(job.status || 'unknown').toLowerCase();
    const actions = source === 'browser' ? [
      ['queued', 'running'].includes(status) ? `<button data-action="cancel-job" data-job-id="${escapeHtml(job.job_id)}">Cancel</button>` : '',
      status === 'failed' ? `<button data-action="retry-job" data-job-id="${escapeHtml(job.job_id)}">Retry</button>` : ''
    ].join('') : '';

    return `<div class="activity-row">
      <div>
        <strong>${escapeHtml(job.operation || 'job')}</strong>
        <div class="muted">${escapeHtml(job.job_id || '-')}</div>
      </div>
      <div>${stateBadge(job.status)}</div>
      <div class="muted">${escapeHtml(getJobTime(job))}</div>
      <div>${escapeHtml(formatJobDetail(job))}</div>
      <div><span class="badge">${escapeHtml(source)}</span>${actions}</div>
    </div>`;
  }).join('');
}
```

- [x] **Step 6: Add runtime policy helpers and troubleshooting render**

Add these helpers before `renderActivity()`:

```javascript
function readNested(value, path) {
  return path.reduce((current, key) => current && typeof current === 'object' ? current[key] : undefined, value);
}

function formatPolicyValue(value) {
  if (value === true) return 'enabled';
  if (value === false) return 'disabled';
  if (value === null || value === undefined || value === '') return '-';
  return value;
}

function renderTroubleshooting() {
  const host = state.host || {};
  const policy = state.runtimePolicy || {};
  const cards = [
    ['Host readiness', host.supported === false ? 'Needs attention' : 'Ready', 'Check Hyper-V support, admin context, VMMS, and Default Switch state.'],
    ['VMMS', formatPolicyValue(readNested(host, ['hyperv', 'vmms_running'])), 'VM lifecycle requests require the Hyper-V management service to be available.'],
    ['Listener exposure', formatPolicyValue(readNested(policy, ['network', 'current_exposure']) || readNested(policy, ['network', 'bind'])), 'Loopback is the default. LAN mode requires explicit approval and token source proof.'],
    ['Token storage', formatPolicyValue(readNested(policy, ['auth', 'token_storage']) || readNested(policy, ['token', 'storage'])), 'Token values are never rendered in this console.'],
    ['Job store', formatPolicyValue(readNested(policy, ['job_runtime', 'state_store', 'persistence'])), 'Unsupported future schema returns PCV_JOB_STORE_SCHEMA_UNSUPPORTED without migration apply.']
  ];
  const errors = [
    ['PCV_AUTH_REQUIRED', 'Token is missing or rejected.'],
    ['PCV_JOB_STORE_SCHEMA_UNSUPPORTED', 'Job store was written by a newer runtime. Stop and investigate before any migration apply.'],
    ['PCV_VM_NOT_MANAGED_BY_PURECVISOR', 'The API blocked destructive VM mutation before provider mutation.'],
    ['PCV_VM_SHUTDOWN_NOT_AVAILABLE', 'Guest shutdown integration is unavailable for the selected VM.']
  ];

  els.troubleshootingPanel.innerHTML = `
    <div class="troubleshooting-grid">
      ${cards.map(([title, value, detail]) => `<div class="troubleshooting-card"><span class="muted">${escapeHtml(title)}</span><strong>${escapeHtml(value)}</strong><p>${escapeHtml(detail)}</p></div>`).join('')}
    </div>
    <div class="code-list">
      ${errors.map(([code, detail]) => `<div class="kv"><span>${escapeHtml(code)}</span><strong>${escapeHtml(detail)}</strong></div>`).join('')}
    </div>`;
}
```

- [x] **Step 7: Render new surfaces**

Change `render()`:

```javascript
function render() {
  renderError();
  renderConnectionState();
  renderMetrics();
  renderHost();
  renderVms();
  renderVmDetail();
  renderJobs();
  renderActivity();
  renderTroubleshooting();
}
```

- [x] **Step 8: Wire DOM elements**

Add these entries in `init()`:

```javascript
activityPanel: byId('activity-panel'),
troubleshootingPanel: byId('troubleshooting-panel'),
```

- [x] **Step 9: Share job actions from Activity panel**

Add this listener in `bindEvents()` after the existing `jobsPanel` listener:

```javascript
els.activityPanel.addEventListener('click', async (event) => {
  const button = event.target.closest('button[data-action]');
  if (!button) return;
  state.error = null;
  try {
    if (button.dataset.action === 'cancel-job') await cancelJob(button.dataset.jobId);
    if (button.dataset.action === 'retry-job') await retryJob(button.dataset.jobId);
    await loadServerJobs();
  } catch (error) {
    state.error = normalizeError(error);
  }
  render();
});
```

- [x] **Step 10: Add CSS**

Append these styles before the media query:

```css
.activity-panel, .troubleshooting-panel {
  display: grid;
  gap: 8px;
}
.activity-row {
  display: grid;
  grid-template-columns: minmax(160px, 1.2fr) 110px minmax(150px, 1fr) minmax(180px, 1.5fr) auto;
  gap: 8px;
  align-items: center;
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 9px;
}
.activity-warning {
  border: 1px solid #f1b4ac;
  background: #fff1f0;
  color: var(--danger);
  border-radius: 8px;
  padding: 8px 10px;
}
.troubleshooting-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(220px, 1fr));
  gap: 10px;
}
.troubleshooting-card {
  border: 1px solid var(--line);
  border-radius: 8px;
  background: var(--panel-soft);
  padding: 10px;
}
.troubleshooting-card strong {
  display: block;
  margin: 4px 0;
}
.troubleshooting-card p {
  margin: 0;
  color: var(--muted);
}
.code-list {
  display: grid;
  gap: 4px;
}
```

Inside the existing mobile media query, add:

```css
.activity-row, .troubleshooting-grid { grid-template-columns: 1fr; }
```

- [x] **Step 11: Regenerate served asset**

Run:

```powershell
npm run build:served --prefix web
```

Expected: command exits 0 and updates `web/app.js`.

## Task 5: Browser Fixture and Web GREEN

**Files:**
- Modify: `web/scripts/verify-browser-fixture.mjs`
- Modify: `web/generated/parity/static-asset-parity.manifest.json` only through `npm run generate:parity --prefix web` if source snapshots changed.

- [x] **Step 1: Add required element ids to browser fixture**

In `requiredIds`, add:

```javascript
"activity-panel",
"troubleshooting-panel",
```

- [x] **Step 2: Add runtime policy fixture response**

Add this branch in `fixtureFetch()`:

```javascript
if (path === "/api/v1/runtime/policy") {
  return ok(
    {
      auth: { token_storage: "dpapi-local-machine" },
      network: { current_exposure: "loopback", static_asset_auth: { non_loopback: "bearer-required" } },
      job_runtime: {
        state_store: { persistence: "json-file-snapshot" },
        dispatch: { helper_boundary: "dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation" }
      }
    },
    "runtime.policy"
  );
}
```

- [x] **Step 3: Add job list fixture response**

Add this branch in `fixtureFetch()` before the `job-browser-fixture` detail branch:

```javascript
if (path === "/api/v1/jobs") {
  return ok(
    {
      count: 1,
      jobs: [
        {
          job_id: "job-browser-fixture",
          operation: "vm.create",
          status: "running",
          attempt: 1,
          created_at: "2026-05-05T00:00:00.0000000Z",
          updated_at: "2026-05-05T00:00:01.0000000Z",
          error: null,
          result: null
        }
      ]
    },
    "job.list"
  );
}
```

- [x] **Step 4: Assert new rendered output**

Extend the required output list:

```javascript
for (const value of ["Host Overview", "Ready", "pcv-browser-fixture", "Delete VM", "job-browser-fixture", "Operator Activity", "Troubleshooting", "PCV_JOB_STORE_SCHEMA_UNSUPPORTED"]) {
  requireIncludes(combinedText, value, "browser fixture rendered output");
}
```

- [x] **Step 5: Keep forbidden rendered output guard**

Keep the existing token and host mutation command guards. Add Linux runtime rendered-output guards:

```javascript
for (const value of ["journalctl", "libvirt", "purecvisorsd"]) {
  requireNotIncludes(renderedText, value, "browser fixture rendered output");
}
```

- [x] **Step 6: Run Web GREEN**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
```

Expected: all pass. No Local API listener, Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, or real OS mutation runs.

- [x] **Step 7: Commit Web P0**

Run:

```powershell
git add web/index.html web/src/served-app.ts web/app.js web/styles.css web/scripts/verify-browser-fixture.mjs web/tests/PcvDesktopWeb.Static.Tests.ps1 web/generated/parity/static-asset-parity.manifest.json
git commit -m "Add operator activity and troubleshooting console"
```

Expected: one Web Console implementation commit.

## Task 6: User and Developer Docs

**Files:**
- Modify: `docs/USER_GUIDE.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`

- [x] **Step 1: Update user guide Web Console coverage**

In `docs/USER_GUIDE.md`, add these bullets to the current Web Console direct range:

```markdown
- Operator Activity: server-side job list와 browser-local `Tracked Jobs`를 함께 표시
- Troubleshooting Center: host readiness, runtime/auth/network policy, common `PCV_*` error guide 표시
```

- [x] **Step 2: Add Activity section**

Add this section after `## Job 확인, 취소, 재시도`:

```markdown
## Operator Activity

`Operator Activity`는 Local API의 server-side job list와 현재 브라우저의 `Tracked Jobs`를 함께 보여준다. 같은 job id가 두 source에 있으면 server-side 상태를 기준으로 보고, browser-local 기록은 현재 브라우저에서 만든 작업 추적용으로만 사용한다.

Activity 화면은 read-only 운영 visibility다. Job cancel/retry button은 기존 `/api/v1/jobs/{job_id}/cancel`, `/api/v1/jobs/{job_id}/retry` contract만 사용한다. Activity 화면은 Hyper-V, service, MSI, firewall, trust-store, LAN, Event Log mutation을 자동 실행하지 않는다.
```

- [x] **Step 3: Add Troubleshooting section**

Add this section after Activity:

```markdown
## Troubleshooting Center

`Troubleshooting`은 host readiness, VMMS/Hyper-V 상태, runtime policy, token storage/source 종류, LAN exposure 상태, common `PCV_*` error guide를 보여준다. Token 값과 Authorization header 값은 화면에 표시하지 않는다.

Diagnostic bundle이 필요하면 운영자가 기존 product wrapper 절차로 수동 수집한다. 이 화면은 Event Log source registration, firewall rule, trust-store, MSI, service lifecycle, reboot, Task Scheduler 작업을 실행하지 않는다.
```

- [x] **Step 4: Update developer index**

In `docs/DEVELOPER_INDEX.md`, add a row near the guide-based backlog rows:

```markdown
| guide 기반 운영 콘솔 P0 확장 확인 | `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-operator-ops-console-expansion-design.md`, `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p0.md` |
```

- [x] **Step 5: Update backlog status**

In the P0 rows of `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md`, mention that P0 has an umbrella spec and first implementation plan:

```markdown
P0 Operator Activity / Troubleshooting 구현 경계는 `docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-operator-ops-console-expansion-design.md`와 `docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-operator-ops-console-p0.md`를 따른다.
```

- [x] **Step 6: Commit docs**

Run:

```powershell
git add docs/USER_GUIDE.md docs/DEVELOPER_INDEX.md docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md
git commit -m "Document operator ops console P0"
```

Expected: one documentation commit.

## Task 7: Full Verification and Main Sync

**Files:**
- All modified files

- [x] **Step 1: Run full product verification for this slice**

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

Expected: all pass. This remains non-mutating product verification.

- [x] **Step 2: Run documentation guard if docs changed**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
```

Expected: PASS. This is component/archive documentation guard, not active product runtime execution.

- [x] **Step 3: Inspect final diff**

Run:

```powershell
git status --short --branch
git diff --stat
```

Expected: only P0 Activity/Troubleshooting/API/docs files are changed.

- [x] **Step 4: Final commit if previous task commits were skipped**

Run:

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs web/index.html web/src/served-app.ts web/app.js web/styles.css web/scripts/verify-browser-fixture.mjs web/tests/PcvDesktopWeb.Static.Tests.ps1 web/generated/parity/static-asset-parity.manifest.json docs/USER_GUIDE.md docs/DEVELOPER_INDEX.md docs/superpowers/specs/2026-05-05-purecvisor-desktop-node-guide-based-ops-expansion-backlog-design.md
git commit -m "Add operator ops console P0"
```

Expected: one final implementation commit if earlier commits were not created.

- [x] **Step 5: Push**

Run:

```powershell
git push
```

Expected: `origin/main` or the active implementation branch receives the P0 commits, depending on the branch at execution time.

## Self-Review

- Spec coverage: P0 Operator Activity is covered by read-only `GET /api/v1/jobs`, server/browser activity rendering, cancel/retry reuse, and job-store future schema blocking. P0 Troubleshooting is covered by runtime policy, host readiness, token storage/source display, and common `PCV_*` guide.
- Scope check: P1 monitoring/auth/checkpoint retention and P2 API hardening/workflow polish/quality gates are intentionally not implemented in this P0 plan. They remain separate follow-up plans under the umbrella spec.
- Mutation boundary: The plan does not execute Hyper-V, service, MSI, firewall, Event Log, trust-store, LAN, Task Scheduler, reboot, update, rollback, config migration apply, or job-store migration apply.
- Placeholder scan: 이 plan에는 미확정 자리표시자가 없다.
- Type consistency: Web code uses existing `state`, `apiFetch`, `asArray`, `normalizeError`, `stateBadge`, `trackJob`, `cancelJob`, `retryJob`, `render`, and `els` patterns. API code uses existing `Body`, `Json`, `JsonFromObject`, `JobData`, and `UsesJobStore` patterns.
