# PureCVisor Desktop Node Phase 3A Web Console Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first static Desktop Node Web Console screen served by the existing Local API `-WebRootPath`.

**Architecture:** Phase 3A creates an isolated `spikes/purecvisor-desktop-node/web/` static app and keeps it separate from the Linux Single Edge `ui/` surface. The app uses plain HTML/CSS/JavaScript, calls existing Phase 2H endpoints with an optional bearer token, renders host status, VM inventory, session-tracked jobs, and submits VM create jobs without adding API routes.

**Tech Stack:** Vanilla HTML/CSS/JavaScript, PowerShell/Pester static smoke tests, Node syntax checking, existing Desktop Node Local API static serving.

---

## Scope

Included:
- Static web root: `spikes/purecvisor-desktop-node/web/`
- Dashboard + VM table first screen
- API base URL field with current-origin default
- optional bearer token support through `Authorization: Bearer <token>`
- `GET /api/v1/host/status`
- `GET /api/v1/vms`
- `POST /api/v1/vms`
- `GET /api/v1/jobs/{job_id}`
- `POST /api/v1/jobs/{job_id}/cancel`
- `POST /api/v1/jobs/{job_id}/retry`
- session-only tracked jobs with polling
- user/API string escaping before DOM insertion
- static smoke tests and documentation updates

Excluded:
- changes to Linux Single Edge `ui/` implementation files
- frontend framework, bundler, npm package, or TypeScript
- VM lifecycle action UI
- checkpoint UI
- VM detail route
- VMConnect launch
- login/token issuance
- persistent browser job history
- new Local API endpoints

## File Map

- `spikes/purecvisor-desktop-node/web/index.html`: static app shell and modal form markup.
- `spikes/purecvisor-desktop-node/web/styles.css`: Desktop Node console layout, table, controls, state badges, modal, and responsive rules.
- `spikes/purecvisor-desktop-node/web/app.js`: state, API client, render functions, create form, job polling, cancel/retry actions.
- `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`: static smoke tests and Node syntax check.
- `spikes/purecvisor-desktop-node/api/README.md`: document the new bundled web root and launch command.
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`: update current implementation status and roadmap.
- `README.md`, `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`, `ui/guide-content.md`: update Phase 3A references and verification commands.
- `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3a-web-console.md`: record completion evidence.

## Tasks

### Task 1: RED Static Web Tests

**Files:**
- Create: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [x] **Step 1: Write failing static smoke tests**

Create `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`:

```powershell
Describe 'PcvDesktopWeb static console assets' {
    BeforeAll {
        $script:WebRoot = Split-Path -Parent $PSScriptRoot
        $script:IndexPath = Join-Path $script:WebRoot 'index.html'
        $script:StylesPath = Join-Path $script:WebRoot 'styles.css'
        $script:AppPath = Join-Path $script:WebRoot 'app.js'
    }

    It 'ships index, stylesheet, and script assets under the Desktop Node web root' {
        Test-Path -LiteralPath $script:IndexPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:StylesPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:AppPath -PathType Leaf | Should -BeTrue

        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $index | Should -Match 'PureCVisor Desktop Node'
        $index | Should -Match 'styles\.css'
        $index | Should -Match 'app\.js'
        $index | Should -Match 'id="app-root"'
    }

    It 'keeps the Desktop Node web console isolated from the Single Edge ui tree' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $styles = Get-Content -LiteralPath $script:StylesPath -Raw

        $index | Should -Not -Match '\.\./\.\./ui/'
        $app | Should -Not -Match '\.\./\.\./ui/'
        $styles | Should -Not -Match '\.\./\.\./ui/'
    }

    It 'declares the Phase 2H API endpoints used by the console' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match '/api/v1/host/status'
        $app | Should -Match '/api/v1/vms'
        $app | Should -Match '/api/v1/jobs/'
        $app | Should -Match '/cancel'
        $app | Should -Match '/retry'
    }

    It 'supports optional bearer token requests' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match 'Authorization'
        $app | Should -Match 'Bearer'
        $app | Should -Match 'apiToken'
    }

    It 'declares the VM create payload fields expected by POST /api/v1/vms' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match 'iso_path'
        $app | Should -Match 'memory_mb'
        $app | Should -Match 'disk_gb'
        $app | Should -Match 'vm_root'
        $app | Should -Match 'generation'
    }

    It 'passes JavaScript syntax validation' {
        $output = & node --check $script:AppPath 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($output | Out-String)
    }
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
```

Expected before implementation: discovery finds 6 tests and failures occur because `index.html`, `styles.css`, and `app.js` do not exist.

### Task 2: Static App Shell

**Files:**
- Create: `spikes/purecvisor-desktop-node/web/index.html`
- Create: `spikes/purecvisor-desktop-node/web/styles.css`

- [x] **Step 1: Create `index.html`**

Create `spikes/purecvisor-desktop-node/web/index.html` with:

```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>PureCVisor Desktop Node</title>
  <link rel="stylesheet" href="/styles.css">
</head>
<body>
  <div id="app-root" class="shell">
    <header class="topbar">
      <div>
        <p class="eyebrow">Local Hyper-V Node</p>
        <h1>PureCVisor Desktop Node</h1>
      </div>
      <form id="connection-form" class="connection-form">
        <label>
          API
          <input id="api-base-url" name="apiBaseUrl" type="url" autocomplete="off">
        </label>
        <label>
          Token
          <input id="api-token" name="apiToken" type="password" autocomplete="off" aria-label="optional API token">
        </label>
        <button type="submit">Save</button>
        <button id="clear-token" type="button">Clear</button>
        <button id="refresh-all" type="button">Refresh</button>
      </form>
      <div id="connection-state" class="connection-state state-idle">Idle</div>
    </header>

    <aside class="sidebar">
      <strong>Desktop Node</strong>
      <nav>
        <a href="#dashboard">Dashboard</a>
        <a href="#vms">Virtual Machines</a>
        <a href="#jobs">Jobs</a>
      </nav>
    </aside>

    <main class="main">
      <section id="alert-region" class="alert-region" aria-live="polite"></section>
      <section id="dashboard" class="section">
        <div class="section-header">
          <div>
            <p class="eyebrow">Dashboard</p>
            <h2>Host Overview</h2>
          </div>
          <button id="open-create-vm" type="button">Create VM</button>
        </div>
        <div id="metric-grid" class="metric-grid"></div>
        <div id="host-details" class="details-grid"></div>
      </section>

      <section id="vms" class="section">
        <div class="section-header">
          <div>
            <p class="eyebrow">Inventory</p>
            <h2>Virtual Machines</h2>
          </div>
        </div>
        <div id="vm-table" class="table-wrap"></div>
      </section>

      <section id="jobs" class="section">
        <div class="section-header">
          <div>
            <p class="eyebrow">Session</p>
            <h2>Tracked Jobs</h2>
          </div>
        </div>
        <div id="jobs-panel" class="jobs-panel"></div>
      </section>
    </main>
  </div>

  <dialog id="create-vm-dialog">
    <form id="create-vm-form" method="dialog" class="create-form">
      <div class="section-header">
        <div>
          <p class="eyebrow">Provisioning</p>
          <h2>Create Linux VM</h2>
        </div>
        <button id="close-create-vm" type="button">Close</button>
      </div>
      <label>Name<input name="name" required value="ubuntu-lab-01"></label>
      <label>ISO path<input name="iso_path" required value="D:\iso\ubuntu-24.04-live-server-amd64.iso"></label>
      <label>VM root<input name="vm_root" required value="D:\PureCVisor\VMs"></label>
      <div class="form-grid">
        <label>CPU<input name="cpu" type="number" min="1" value="2"></label>
        <label>Memory MB<input name="memory_mb" type="number" min="512" value="4096"></label>
        <label>Disk GB<input name="disk_gb" type="number" min="1" value="40"></label>
        <label>Generation<input name="generation" type="number" min="1" max="2" value="2"></label>
      </div>
      <button type="submit">Queue Create Job</button>
    </form>
  </dialog>

  <script src="/app.js" defer></script>
</body>
</html>
```

- [x] **Step 2: Create `styles.css`**

Create `spikes/purecvisor-desktop-node/web/styles.css` with a restrained operations UI:

```css
:root {
  color-scheme: light;
  --bg: #eef2f6;
  --panel: #ffffff;
  --panel-soft: #f8fafc;
  --text: #142033;
  --muted: #657184;
  --line: #d8dee8;
  --accent: #0f766e;
  --danger: #b42318;
  --warn: #a15c07;
  --ok: #0f7a3a;
}

* { box-sizing: border-box; }
body {
  margin: 0;
  min-height: 100vh;
  font: 14px/1.45 "Segoe UI", system-ui, sans-serif;
  color: var(--text);
  background: var(--bg);
}
button, input {
  font: inherit;
}
button {
  border: 1px solid var(--line);
  background: var(--panel);
  color: var(--text);
  padding: 7px 10px;
  border-radius: 6px;
  cursor: pointer;
}
button:hover { border-color: var(--accent); }
input {
  border: 1px solid var(--line);
  border-radius: 6px;
  padding: 7px 9px;
  min-width: 0;
}
.shell {
  display: grid;
  grid-template-columns: 220px 1fr;
  grid-template-rows: auto 1fr;
  min-height: 100vh;
}
.topbar {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: minmax(220px, 1fr) minmax(420px, 720px) auto;
  gap: 16px;
  align-items: center;
  padding: 14px 18px;
  background: #101827;
  color: #fff;
}
.topbar h1, .section h2 { margin: 0; font-size: 20px; }
.eyebrow {
  margin: 0 0 3px;
  font-size: 11px;
  text-transform: uppercase;
  color: var(--muted);
}
.topbar .eyebrow { color: #a7b2c4; }
.connection-form {
  display: grid;
  grid-template-columns: minmax(180px, 1fr) minmax(140px, 220px) auto auto auto;
  gap: 8px;
  align-items: end;
}
.connection-form label {
  display: grid;
  gap: 4px;
  font-size: 12px;
  color: #cbd5e1;
}
.connection-state {
  border-radius: 999px;
  padding: 6px 10px;
  font-size: 12px;
  background: #334155;
}
.state-connected { background: var(--ok); }
.state-auth { background: var(--warn); }
.state-error { background: var(--danger); }
.sidebar {
  padding: 18px;
  background: var(--panel);
  border-right: 1px solid var(--line);
}
.sidebar nav {
  display: grid;
  gap: 8px;
  margin-top: 18px;
}
.sidebar a {
  color: var(--text);
  text-decoration: none;
  padding: 7px 8px;
  border-radius: 6px;
}
.sidebar a:hover { background: var(--panel-soft); }
.main {
  padding: 18px;
  display: grid;
  gap: 16px;
}
.section {
  background: var(--panel);
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 14px;
}
.section-header {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
  margin-bottom: 12px;
}
.metric-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(120px, 1fr));
  gap: 10px;
  margin-bottom: 12px;
}
.metric {
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 10px;
  background: var(--panel-soft);
}
.metric strong { display: block; font-size: 22px; }
.details-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(220px, 1fr));
  gap: 8px;
}
.kv {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  border-bottom: 1px solid var(--line);
  padding: 6px 0;
}
.table-wrap { overflow-x: auto; }
table {
  width: 100%;
  border-collapse: collapse;
}
th, td {
  border-bottom: 1px solid var(--line);
  padding: 9px 8px;
  text-align: left;
  vertical-align: top;
}
th { color: var(--muted); font-size: 12px; }
.badge {
  display: inline-block;
  border-radius: 999px;
  padding: 2px 7px;
  background: #e2e8f0;
  font-size: 12px;
}
.badge-ok { background: #dff7e8; color: var(--ok); }
.badge-warn { background: #fff2d6; color: var(--warn); }
.badge-error { background: #ffe4e0; color: var(--danger); }
.alert-region:not(:empty) {
  border: 1px solid #f1b4ac;
  background: #fff1f0;
  color: var(--danger);
  border-radius: 8px;
  padding: 10px 12px;
}
.jobs-panel {
  display: grid;
  gap: 8px;
}
.job-row {
  display: grid;
  grid-template-columns: minmax(180px, 1fr) 100px auto;
  gap: 8px;
  align-items: center;
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 9px;
}
dialog {
  width: min(720px, calc(100vw - 28px));
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 0;
}
.create-form {
  padding: 16px;
  display: grid;
  gap: 10px;
}
.create-form label {
  display: grid;
  gap: 4px;
}
.form-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 10px;
}
.muted { color: var(--muted); }

@media (max-width: 900px) {
  .shell { grid-template-columns: 1fr; }
  .topbar { grid-template-columns: 1fr; }
  .connection-form { grid-template-columns: 1fr; }
  .sidebar { border-right: 0; border-bottom: 1px solid var(--line); }
  .metric-grid, .details-grid, .form-grid { grid-template-columns: 1fr; }
  .job-row { grid-template-columns: 1fr; }
}
```

- [x] **Step 3: Run RED tests again**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
```

Expected after HTML/CSS only: fewer failures remain because `app.js` is still missing and `node --check` cannot pass.

### Task 3: JavaScript App

**Files:**
- Create: `spikes/purecvisor-desktop-node/web/app.js`

- [x] **Step 1: Create `app.js` state and API client**

Create `spikes/purecvisor-desktop-node/web/app.js` with:

```javascript
const state = {
  apiBaseUrl: window.location.origin,
  apiToken: '',
  host: null,
  vms: [],
  trackedJobs: [],
  loading: false,
  error: null,
  connectionState: 'idle',
  pollTimer: null
};

const els = {};

function byId(id) {
  return document.getElementById(id);
}

function escapeHtml(value) {
  return String(value ?? '-')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

function asArray(value) {
  if (Array.isArray(value)) return value;
  if (Array.isArray(value?.vms)) return value.vms;
  if (Array.isArray(value?.items)) return value.items;
  if (value && typeof value === 'object') return Object.values(value).filter((item) => item && typeof item === 'object');
  return [];
}

function normalizeError(error) {
  if (error?.normalized) return error;
  return {
    normalized: true,
    status: error?.status ?? 0,
    operation: error?.operation ?? 'web.request',
    code: error?.code ?? 'PCV_NETWORK_ERROR',
    message: error?.message ?? 'The Desktop Node API request failed.',
    detail: error?.detail ?? String(error),
    retryable: Boolean(error?.retryable)
  };
}

async function apiFetch(path, options = {}) {
  const base = state.apiBaseUrl.replace(/\/$/, '');
  const headers = { Accept: 'application/json', ...(options.headers || {}) };
  if (state.apiToken.trim()) {
    headers.Authorization = `Bearer ${state.apiToken.trim()}`;
  }
  if (options.body && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json';
  }

  let response;
  try {
    response = await fetch(`${base}${path}`, { ...options, headers });
  } catch (error) {
    throw normalizeError({ code: 'PCV_NETWORK_ERROR', message: 'Network request failed.', detail: error.message });
  }

  let payload;
  try {
    payload = await response.json();
  } catch (error) {
    throw normalizeError({
      status: response.status,
      code: 'PCV_RESPONSE_INVALID',
      message: 'The API returned a malformed response.',
      detail: error.message
    });
  }

  if (!response.ok || payload.ok === false) {
    const apiError = payload.error || {};
    throw normalizeError({
      status: response.status,
      operation: payload.operation,
      code: apiError.code,
      message: apiError.message,
      detail: apiError.detail,
      retryable: apiError.retryable
    });
  }

  return payload.data;
}
```

- [x] **Step 2: Add render functions**

Append render helpers that fill the sections:

```javascript
function stateBadge(value) {
  const text = String(value ?? 'unknown');
  const normalized = text.toLowerCase();
  const cls = normalized.includes('running') || normalized.includes('ready') || normalized === 'ok'
    ? 'badge-ok'
    : normalized.includes('fail') || normalized.includes('error') || normalized.includes('forbidden')
      ? 'badge-error'
      : 'badge-warn';
  return `<span class="badge ${cls}">${escapeHtml(text)}</span>`;
}

function renderMetrics() {
  const host = state.host || {};
  const vms = asArray(state.vms);
  const runningCount = vms.filter((vm) => String(vm.state || vm.status || '').toLowerCase().includes('running')).length;
  const activeJobs = state.trackedJobs.filter((job) => ['queued', 'running'].includes(String(job.status).toLowerCase())).length;
  els.metricGrid.innerHTML = [
    ['Host', host.supported === false ? 'Needs attention' : 'Ready'],
    ['VMs', vms.length],
    ['Running', runningCount],
    ['Active Jobs', activeJobs]
  ].map(([label, value]) => `<div class="metric"><span class="muted">${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`).join('');
}

function renderHost() {
  const host = state.host || {};
  const entries = Object.entries(host).slice(0, 16);
  if (entries.length === 0) {
    els.hostDetails.innerHTML = '<p class="muted">Host status has not been loaded.</p>';
    return;
  }
  els.hostDetails.innerHTML = entries
    .map(([key, value]) => `<div class="kv"><span>${escapeHtml(key)}</span><strong>${escapeHtml(typeof value === 'object' ? JSON.stringify(value) : value)}</strong></div>`)
    .join('');
}

function renderVms() {
  const vms = asArray(state.vms);
  if (vms.length === 0) {
    els.vmTable.innerHTML = '<p class="muted">No VMs returned by the Desktop Node API.</p>';
    return;
  }
  const rows = vms.map((vm) => `
    <tr>
      <td>${escapeHtml(vm.name || vm.id)}</td>
      <td>${stateBadge(vm.state || vm.status)}</td>
      <td>${escapeHtml(vm.cpu || vm.vcpu || vm.processor_count)}</td>
      <td>${escapeHtml(vm.memory_mb || vm.memory || vm.memory_assigned_mb)}</td>
      <td>${escapeHtml(vm.generation)}</td>
      <td>${escapeHtml(vm.uptime || vm.updated_at || vm.created_at)}</td>
      <td>${escapeHtml(vm.error?.message || vm.notes || '-')}</td>
    </tr>`).join('');
  els.vmTable.innerHTML = `
    <table>
      <thead><tr><th>Name</th><th>State</th><th>CPU</th><th>Memory</th><th>Gen</th><th>Updated</th><th>Notes</th></tr></thead>
      <tbody>${rows}</tbody>
    </table>`;
}

function renderJobs() {
  if (state.trackedJobs.length === 0) {
    els.jobsPanel.innerHTML = '<p class="muted">Jobs created from this browser session will appear here.</p>';
    return;
  }
  els.jobsPanel.innerHTML = state.trackedJobs.map((job) => {
    const status = String(job.status || 'unknown').toLowerCase();
    const actions = [
      ['queued', 'running'].includes(status) ? `<button data-action="cancel-job" data-job-id="${escapeHtml(job.job_id)}">Cancel</button>` : '',
      status === 'failed' ? `<button data-action="retry-job" data-job-id="${escapeHtml(job.job_id)}">Retry</button>` : ''
    ].join('');
    return `<div class="job-row"><div><strong>${escapeHtml(job.job_id)}</strong><div class="muted">${escapeHtml(job.operation || 'vm.create')}</div></div><div>${stateBadge(job.status)}</div><div>${actions}</div></div>`;
  }).join('');
}

function renderError() {
  if (!state.error) {
    els.alertRegion.innerHTML = '';
    return;
  }
  els.alertRegion.innerHTML = `<strong>${escapeHtml(state.error.code)}</strong> ${escapeHtml(state.error.message)}<div>${escapeHtml(state.error.detail)}</div>`;
}

function renderConnectionState() {
  const label = {
    idle: 'Idle',
    connected: 'Connected',
    auth: 'Auth required',
    error: 'Error'
  }[state.connectionState] || 'Idle';
  els.connectionState.className = `connection-state state-${state.connectionState}`;
  els.connectionState.textContent = label;
}

function render() {
  renderError();
  renderConnectionState();
  renderMetrics();
  renderHost();
  renderVms();
  renderJobs();
}
```

- [x] **Step 3: Add load, create, polling, cancel, and retry actions**

Append action functions:

```javascript
async function loadHost() {
  state.host = await apiFetch('/api/v1/host/status');
}

async function loadVms() {
  state.vms = await apiFetch('/api/v1/vms');
}

async function refreshAll() {
  state.loading = true;
  state.error = null;
  render();
  try {
    await Promise.all([loadHost(), loadVms(), pollTrackedJobs()]);
    state.connectionState = 'connected';
  } catch (error) {
    state.error = normalizeError(error);
    state.connectionState = state.error.code === 'PCV_AUTH_REQUIRED' || state.error.code === 'PCV_AUTH_FORBIDDEN' ? 'auth' : 'error';
  } finally {
    state.loading = false;
    render();
  }
}

function readCreatePayload(form) {
  const data = new FormData(form);
  const payload = {
    name: String(data.get('name') || '').trim(),
    iso_path: String(data.get('iso_path') || '').trim(),
    cpu: Number(data.get('cpu')),
    memory_mb: Number(data.get('memory_mb')),
    disk_gb: Number(data.get('disk_gb')),
    vm_root: String(data.get('vm_root') || '').trim(),
    generation: Number(data.get('generation'))
  };
  if (!payload.name || !payload.iso_path || !payload.vm_root) {
    throw normalizeError({ code: 'PCV_FORM_INVALID', message: 'Required fields are missing.', detail: 'Name, ISO path, and VM root are required.' });
  }
  return payload;
}

async function submitCreateVm(event) {
  event.preventDefault();
  state.error = null;
  try {
    const payload = readCreatePayload(event.currentTarget);
    const job = await apiFetch('/api/v1/vms', { method: 'POST', body: JSON.stringify(payload) });
    trackJob(job);
    els.createDialog.close();
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  }
  render();
}

function trackJob(job) {
  if (!job?.job_id) return;
  const existingIndex = state.trackedJobs.findIndex((item) => item.job_id === job.job_id);
  if (existingIndex >= 0) state.trackedJobs[existingIndex] = job;
  else state.trackedJobs.unshift(job);
}

async function pollTrackedJobs() {
  const ids = state.trackedJobs.map((job) => job.job_id).filter(Boolean);
  for (const jobId of ids) {
    const job = await apiFetch(`/api/v1/jobs/${encodeURIComponent(jobId)}`);
    trackJob(job);
  }
}

function startPolling() {
  if (state.pollTimer) return;
  state.pollTimer = window.setInterval(async () => {
    const active = state.trackedJobs.some((job) => ['queued', 'running'].includes(String(job.status).toLowerCase()));
    if (!active) {
      window.clearInterval(state.pollTimer);
      state.pollTimer = null;
      return;
    }
    try {
      await pollTrackedJobs();
      render();
    } catch (error) {
      state.error = normalizeError(error);
      render();
    }
  }, 2000);
}

async function cancelJob(jobId) {
  const job = await apiFetch(`/api/v1/jobs/${encodeURIComponent(jobId)}/cancel`, { method: 'POST' });
  trackJob(job);
  render();
}

async function retryJob(jobId) {
  const job = await apiFetch(`/api/v1/jobs/${encodeURIComponent(jobId)}/retry`, { method: 'POST' });
  trackJob(job);
  startPolling();
  render();
}
```

- [x] **Step 4: Add event binding and initialization**

Append initialization:

```javascript
function bindEvents() {
  els.connectionForm.addEventListener('submit', (event) => {
    event.preventDefault();
    state.apiBaseUrl = els.apiBaseUrl.value.trim() || window.location.origin;
    state.apiToken = els.apiToken.value.trim();
    refreshAll();
  });
  els.clearToken.addEventListener('click', () => {
    state.apiToken = '';
    els.apiToken.value = '';
    refreshAll();
  });
  els.refreshAll.addEventListener('click', refreshAll);
  els.openCreateVm.addEventListener('click', () => els.createDialog.showModal());
  els.closeCreateVm.addEventListener('click', () => els.createDialog.close());
  els.createVmForm.addEventListener('submit', submitCreateVm);
  els.jobsPanel.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    state.error = null;
    try {
      if (button.dataset.action === 'cancel-job') await cancelJob(button.dataset.jobId);
      if (button.dataset.action === 'retry-job') await retryJob(button.dataset.jobId);
    } catch (error) {
      state.error = normalizeError(error);
      render();
    }
  });
}

function init() {
  Object.assign(els, {
    connectionForm: byId('connection-form'),
    apiBaseUrl: byId('api-base-url'),
    apiToken: byId('api-token'),
    clearToken: byId('clear-token'),
    refreshAll: byId('refresh-all'),
    connectionState: byId('connection-state'),
    alertRegion: byId('alert-region'),
    metricGrid: byId('metric-grid'),
    hostDetails: byId('host-details'),
    vmTable: byId('vm-table'),
    jobsPanel: byId('jobs-panel'),
    openCreateVm: byId('open-create-vm'),
    closeCreateVm: byId('close-create-vm'),
    createDialog: byId('create-vm-dialog'),
    createVmForm: byId('create-vm-form')
  });
  els.apiBaseUrl.value = state.apiBaseUrl;
  bindEvents();
  render();
  refreshAll();
}

document.addEventListener('DOMContentLoaded', init);
```

- [x] **Step 5: Run GREEN static web tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

Expected: static web suite reports 6 passed, 0 failed; node syntax check exits 0.

### Task 4: Documentation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- Modify: `README.md`
- Modify: `AGENTS.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/GUIDE.md`
- Modify: `ui/guide-content.md`
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3a-web-console.md`

- [x] **Step 1: Update Desktop Node API README**

Update `spikes/purecvisor-desktop-node/api/README.md`:
- change status from Phase 2H to Phase 3A
- add `spikes/purecvisor-desktop-node/web/` as the bundled Web Console root
- update the `-WebRootPath` example to use `spikes/purecvisor-desktop-node/web`
- confirm the outdated Web Console UI exclusion was removed from the API README
- add the static web test command

- [x] **Step 2: Update repository docs**

Update the repository docs:
- `README.md`: add the Phase 3A plan link and web static test command
- `AGENTS.md`: change Local API spike wording to `Phase 2A/.../2H + Phase 3A Web Console`
- `docs/DEVELOPER_INDEX.md`: add Phase 3A Web Console to the Desktop Node path
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: add a Desktop Node Web Console row requiring static web suite plus API suite
- `docs/PUBLIC_RELEASE_BOUNDARY.md`: state that the Desktop Node static Web Console remains under `spikes/` and is not Single Edge `ui/`
- `docs/GUIDE.md` and `ui/guide-content.md`: mention the Desktop Node static Web Console spike
- `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`: mark Phase 3A as completed after implementation

- [x] **Step 3: Run doc check**

Run:

```powershell
rg -n "Phase 3A|Desktop Node Web Console|spikes/purecvisor-desktop-node/web|PcvDesktopWeb" README.md AGENTS.md docs spikes/purecvisor-desktop-node/api/README.md ui/guide-content.md
git diff --check
```

Expected: new Phase 3A references appear in the intended docs; diff check exits 0.

### Task 5: Final Verification and Commit

**Files:**
- All Phase 3A files above

- [x] **Step 1: Run final verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

Expected:
- Web static suite: 6 passed, 0 failed
- Node syntax check: exit 0
- API suite: 46 passed, 0 failed
- Hyper-V non-integration suite: 41 passed, 0 failed, 1 NotRun
- diff check exits 0

- [x] **Step 2: Record completion evidence**

Update this plan's Completion Status section with:
- RED web test result
- GREEN web test result
- final web/API/Hyper-V/diff verification results

- [x] **Step 3: Stage exact files**

Run:

```powershell
git add -- `
  AGENTS.md `
  README.md `
  docs/DEVELOPER_INDEX.md `
  docs/DEVELOPMENT_VERIFICATION_POLICY.md `
  docs/GUIDE.md `
  docs/PUBLIC_RELEASE_BOUNDARY.md `
  docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md `
  docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3a-web-console.md `
  spikes/purecvisor-desktop-node/api/README.md `
  spikes/purecvisor-desktop-node/web/index.html `
  spikes/purecvisor-desktop-node/web/styles.css `
  spikes/purecvisor-desktop-node/web/app.js `
  spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1 `
  ui/guide-content.md
```

- [x] **Step 4: Check staged diff and commit**

Run:

```powershell
git diff --cached --stat
git diff --cached --check
git commit -m "feat: add Desktop Node web console shell"
git push origin main
```

Expected: staged diff contains only Phase 3A files, staged diff check exits 0, commit and push succeed.

## Completion Status

Phase 3A implementation is complete through local verification.

Evidence:
- RED static web tests before production assets: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed` discovered 6 tests, with 1 passed and 5 failed because `index.html`, `styles.css`, and `app.js` were not present yet.
- Static shell interim check: the same suite still failed with 1 passed and 5 failed after `index.html` and `styles.css`, confirming `app.js` contract coverage before implementation.
- GREEN static web tests: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed` passed 6, failed 0.
- JavaScript syntax: `node --check spikes/purecvisor-desktop-node/web/app.js` exited 0.
- Local API regression: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed` passed 46, failed 0.
- Hyper-V helper regression: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed` passed 41, failed 0, NotRun 1.
- Diff check: `git diff --check` exited 0.

## Self-Review

- Spec coverage: covers isolated static web root, Dashboard + VM table, host status, VM list, create jobs, session-tracked jobs, token support, error handling, docs, and verification.
- Filler scan: no incomplete requirement language remains.
- Type consistency: paths, endpoint strings, payload fields, and parameter names match the Phase 3A design and Phase 2H API contract.
