# Web Console 운영 상태 진실성 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Web Console이 표시하는 모든 운영값을 실제 `state`에서만 나오게 하고, 확보하지 못한 값은 `—`로 내린다.

**Architecture:** `web/index.html`에서 조작된 운영값을 제거하고 요소 `id`를 부여한다. `served-app.ts`에 `renderStatusBar()`/`renderHeroChips()`를 추가해 기존 단일 진입점 `render()`에 등록한다. 로드 여부는 신규 `state.lastRefreshedAt`과 `state.partialFailures[].operation`으로 판정한다. 생성물 `web/app.js`는 항상 `build-served-asset.mjs`로 재생성한다.

**Tech Stack:** TypeScript (`web/src/**`), 생성 JS (`web/app.js`), Node fixture 하네스 (`web/scripts/verify-browser-fixture.mjs`), Pester 5.7.1 (`web/tests/**`)

**Source spec:** `docs/superpowers/specs/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness-design.md`

**User-Approval:** `web-console-state-truthfulness-design-20260805`

## Global Constraints

- 구현 변경 허용 경로는 `web/**`뿐이다. 예외는 Task 3이 추가하는 신규 code-level evidence 문서 1건과 그에 대한 `docs/ga-ready/EVIDENCE_INDEX.md` 항목 1건이다.
- `src/**` (.NET), `packaging/**`, `.github/workflows/**`는 변경하지 않는다.
- `docs/ga-ready/current-evidence.json`과 생성기가 소유하는 current/anchor 블록은 변경하지 않는다. operational anchor는 `0.42.65-admin-smoke` 그대로 유지한다.
- `web/app.js`는 **생성물이다.** 직접 편집하지 않는다. `node scripts/build-served-asset.mjs`로만 재생성한다.
- `web/index.html`을 바꾸면 static parity manifest를 재생성해야 한다: `npm run generate:parity --prefix web`.
- 새 CI job을 만들지 않는다. 가드는 기존 required 게이트(`web/tests` Pester, `verify-browser-fixture.mjs`) 안에 넣는다.
- host mutation, 서비스 재시작, Hyper-V 조작, remote push/merge를 수행하지 않는다.
- 사용자 소유 untracked 파일(`docs/functional-correctness-verification-2026-07-15-results.md`, `docs/service-core-backend-frontend-implementation-evaluation-2026-07-16.md`, `testResults.xml`)은 staging·수정·삭제하지 않는다.
- PowerShell `-Match`는 대소문자를 무시한다. `Connected` 검사는 반드시 `>Connected<`처럼 요소 경계로 앵커한다.
- `GET /api/v1/host/status`에는 머신 이름 필드가 없다. 호스트 식별자 자리는 `windows.caption`을 쓴다.

---

## File Structure

| 파일 | 책임 | 변경 |
| --- | --- | --- |
| `web/index.html` | 정적 셸 마크업 | 조작값 제거, 요소 `id` 부여 |
| `web/src/served/types.ts` | `PcvState` 타입 | `lastRefreshedAt` 필드 타입 추가 |
| `web/src/served/state.ts` | 런타임 state 초기값 | `lastRefreshedAt: null` 추가 |
| `web/src/served-app.ts` | 렌더링과 갱신 | 렌더 함수 2개 + 헬퍼 2개 추가, `render()`/`els`/`refreshAll()` 수정 |
| `web/app.js` | 브라우저 배포 자산 (생성물) | 재생성만 |
| `web/generated/**` | static parity manifest | 재생성만 |
| `web/tests/PcvDesktopWeb.Static.Tests.ps1` | 정적 자산 계약 가드 | 조작값 금지 테스트 추가 |
| `web/scripts/verify-browser-fixture.mjs` | 렌더 동작 가드 | 미인증 시나리오 테스트 추가 |

---

### Task 1: 정적 HTML에서 조작된 운영값 제거

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1` (파일 끝, 마지막 `}` 직전)
- Modify: `web/index.html:91-93`, `web/index.html:108-136`, `web/index.html:162-166`, `web/index.html:362-370`
- Modify: `web/generated/**` (재생성)

**Interfaces:**
- Consumes: 없음
- Produces: 요소 `id` — `status-connection`, `status-host`, `status-updated`, `status-vm-count`, `status-view`, `hero-workload`, `hero-host-mode`, `hero-alerts`. Task 2가 이 `id`들로 `byId()` 조회를 한다.

- [ ] **Step 1: 실패하는 정적 가드 작성**

`web/tests/PcvDesktopWeb.Static.Tests.ps1`의 마지막 닫는 `}` **직전**에 추가한다.

```powershell
    It 'keeps fabricated operational values out of the static console shell' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw

        $forbidden = @(
            '>Connected<',
            'VM: 3/3',
            'API: 10ms avg',
            'Updated 0s ago',
            '<strong>4/5</strong>',
            'pcv-node-a',
            'pcv-node-b',
            'lab-vm-01'
        )
        foreach ($pattern in $forbidden) {
            $index | Should -Not -Match ([regex]::Escape($pattern)) -Because "index.html must not ship the fabricated operational value '$pattern'"
        }

        $index | Should -Not -Match '<span id="asset-count">\s*[0-9]'

        $requiredIds = @(
            'status-connection',
            'status-host',
            'status-updated',
            'status-vm-count',
            'status-view',
            'hero-workload',
            'hero-host-mode',
            'hero-alerts'
        )
        foreach ($id in $requiredIds) {
            $index | Should -Match ([regex]::Escape("id=`"$id`"")) -Because "index.html must expose #$id for real-state binding"
        }
    }
```

- [ ] **Step 2: 가드가 실패하는지 확인**

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`

Expected: FAIL. `>Connected<`, `VM: 3/3`, `API: 10ms avg`, `pcv-node-a` 등이 매치되고 `status-connection` 같은 `id`가 없다고 보고해야 한다.

- [ ] **Step 3: 푸터 마크업 교체**

`web/index.html:362-370`의 `<footer class="status-bar">` 블록 전체를 교체한다.

```html
    <footer class="status-bar">
      <span id="status-connection">Not connected</span>
      <span id="status-host">—</span>
      <span id="status-updated">Updated —</span>
      <span class="status-live" id="status-vm-count">VM: —</span>
      <span id="status-view">dashboard</span>
    </footer>
```

`WS`와 `API latency` span은 계측 인프라가 없으므로 삭제한다.

- [ ] **Step 4: hero chip 마크업 교체**

`web/index.html:162-166`의 `<div class="hero-chips">` 블록을 교체한다. 라벨이 `호스트 모드`에서 `호스트 상태`로 바뀐다.

```html
            <div class="hero-chips">
              <span>활성 워크로드 <strong id="hero-workload">—</strong></span>
              <span>호스트 상태 <strong id="hero-host-mode">—</strong></span>
              <span>최근 경고 <strong id="hero-alerts">—</strong></span>
            </div>
```

- [ ] **Step 5: asset seed 교체**

`web/index.html:93`을 교체한다.

```html
        <span id="asset-count">—</span>
```

`web/index.html:108-136`의 `<div id="vm-asset-list" …>` 블록 전체(정적 `pcv-node-a`/`pcv-node-b`/`lab-vm-01` 행 포함)를 교체한다.

```html
      <div id="vm-asset-list" class="asset-list" aria-label="Pinned desktop node assets">
        <p class="muted">VM 자산을 불러오는 중…</p>
      </div>
```

- [ ] **Step 6: parity manifest 재생성**

Run: `npm run generate:parity --prefix web`

Expected: exit 0. `web/generated/**` manifest가 갱신된다.

- [ ] **Step 7: 가드가 통과하는지 확인**

Run: `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`

Expected: PASS, 실패 0.

- [ ] **Step 8: 커밋**

```bash
git add web/index.html web/generated web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "fix(web): remove fabricated operational values from static shell"
```

---

### Task 2: 실제 state에 바인딩

**Files:**
- Modify: `web/src/served/types.ts:176` 부근 (`PcvState`)
- Modify: `web/src/served/state.ts:45` 부근 (초기 state)
- Modify: `web/src/served-app.ts` (헬퍼 2개 + 렌더 2개 추가, `els`/`render()`/`refreshAll()` 수정)
- Modify: `web/app.js` (재생성)
- Modify: `web/scripts/verify-browser-fixture.mjs`

**Interfaces:**
- Consumes: Task 1이 만든 요소 `id` 8개
- Produces: `state.lastRefreshedAt: number | null`, `hasRefreshedOperation(operation: string): boolean`, `formatRelativeTime(timestampMs: number | null): string`, `renderStatusBar(): void`, `renderHeroChips(): void`

- [ ] **Step 1: 실패하는 동작 가드 작성**

`web/scripts/verify-browser-fixture.mjs`의 `requiredIds` 배열(10행부터)에 8개를 추가한다.

```javascript
  "status-connection",
  "status-host",
  "status-updated",
  "status-vm-count",
  "status-view",
  "hero-workload",
  "hero-host-mode",
  "hero-alerts",
```

그리고 파일 끝에 미인증 시나리오 검사를 추가한다.

```javascript
const unauthenticatedRun = await runFixture({ failAllAuth: true });
const unauthDoc = unauthenticatedRun.document;

const unauthFooter = [
  "status-connection",
  "status-host",
  "status-updated",
  "status-vm-count"
].map((id) => unauthDoc.getElementById(id).textContent).join(" | ");

requireIncludes(unauthFooter, "Not connected", "unauthenticated status bar");
requireNotIncludes(unauthFooter, "pcv-node-a", "unauthenticated status bar");
requireNotIncludes(unauthFooter, "VM: 3/3", "unauthenticated status bar");
requireIncludes(unauthFooter, "VM: —", "unauthenticated status bar");
requireIncludes(unauthFooter, "Updated —", "unauthenticated status bar");

const unauthHero = [
  "hero-workload",
  "hero-host-mode",
  "hero-alerts"
].map((id) => unauthDoc.getElementById(id).textContent).join(" | ");

requireNotIncludes(unauthHero, "4/5", "unauthenticated hero chips");
requireNotIncludes(unauthHero, "Ready", "unauthenticated hero chips");
```

`failAllAuth` 옵션을 지원하도록 `createFixtureFetch`의 구조분해(457행)에 `failAllAuth = false,`를 추가하고, 반환되는 `fixtureFetch` 본문 최상단 — `const method = ...` 다음 줄 — 에 다음을 넣는다.

```javascript
  if (failAllAuth) {
    return failResponse(401, path.replace("/api/v1/", "").replace(/\//g, "."), "PCV_AUTH_REQUIRED", "Authentication is required.");
  }
```

`runFixture`가 옵션을 그대로 넘기는지 확인한다. 넘기지 않으면 `createFixtureFetch(options)` 호출에 포함시킨다.

- [ ] **Step 2: 가드가 실패하는지 확인**

Run: `npm run browser:fixture --prefix web`

Expected: FAIL. `status-connection` 요소가 없다거나(요소 조회 실패) 푸터 텍스트가 기대와 다르다고 보고해야 한다.

- [ ] **Step 3: state 타입과 초기값 추가**

`web/src/served/types.ts`의 `PcvState`에서 `refreshRequestId: number;` 줄 바로 뒤에 추가한다.

```typescript
  lastRefreshedAt: number | null;
```

`web/src/served/state.ts`의 초기 state에서 `connectionState: 'idle',` 줄 바로 뒤에 추가한다.

```javascript
  lastRefreshedAt: null,
```

- [ ] **Step 4: 헬퍼와 렌더 함수 추가**

`web/src/served-app.ts`에서 `function renderConnectionState()` 정의 **바로 앞**에 추가한다.

```javascript
function formatRelativeTime(timestampMs) {
  if (!timestampMs) return '—';
  const seconds = Math.max(0, Math.round((Date.now() - timestampMs) / 1000));
  if (seconds < 60) return `${seconds}s ago`;
  const minutes = Math.round(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  return `${Math.round(minutes / 60)}h ago`;
}

function hasRefreshedOperation(operation) {
  if (state.lastRefreshedAt === null) return false;
  return !(state.partialFailures || []).some((failure) => failure?.operation === operation);
}

function renderStatusBar() {
  if (els.statusConnection) {
    els.statusConnection.textContent = state.connectionState === 'connected' ? 'Connected' : 'Not connected';
  }
  if (els.statusHost) {
    const caption = hasRefreshedOperation('host.status')
      ? readNested(state.host, ['windows', 'caption'])
      : undefined;
    els.statusHost.textContent = caption ? String(caption) : '—';
  }
  if (els.statusUpdated) {
    els.statusUpdated.textContent = `Updated ${formatRelativeTime(state.lastRefreshedAt)}`;
  }
  if (els.statusVmCount) {
    if (hasRefreshedOperation('vm.list')) {
      const counts = getSummaryVmCounts();
      els.statusVmCount.textContent = `VM: ${counts.running}/${counts.total}`;
    } else {
      els.statusVmCount.textContent = 'VM: —';
    }
  }
  if (els.statusView) {
    els.statusView.textContent = state.activeView;
  }
}

function renderHeroChips() {
  if (els.heroWorkload) {
    if (hasRefreshedOperation('vm.list')) {
      const counts = getSummaryVmCounts();
      els.heroWorkload.textContent = `${counts.running}/${counts.total}`;
    } else {
      els.heroWorkload.textContent = '—';
    }
  }
  if (els.heroHostMode) {
    els.heroHostMode.textContent = hasRefreshedOperation('host.status')
      ? String(getHostReadinessLabel())
      : '—';
  }
  if (els.heroAlerts) {
    els.heroAlerts.textContent = state.lastRefreshedAt === null
      ? '—'
      : String((state.partialFailures || []).length);
  }
}
```

`getHostReadinessLabel()`은 `state.host`가 `null`이어도 `Ready`를 반환하므로 반드시 `hasRefreshedOperation('host.status')` 뒤에서만 호출한다.

- [ ] **Step 5: `els`에 요소 등록**

`web/src/served-app.ts`의 `els` 객체에서 `connectionState: byId('connection-state'),` 줄 바로 뒤에 추가한다.

```javascript
    statusConnection: byId('status-connection'),
    statusHost: byId('status-host'),
    statusUpdated: byId('status-updated'),
    statusVmCount: byId('status-vm-count'),
    statusView: byId('status-view'),
    heroWorkload: byId('hero-workload'),
    heroHostMode: byId('hero-host-mode'),
    heroAlerts: byId('hero-alerts'),
```

- [ ] **Step 6: `render()`에 등록**

`web/src/served-app.ts`의 `function render()` 안에서 `renderConnectionState();` 줄 바로 뒤에 추가한다.

```javascript
  renderStatusBar();
  renderHeroChips();
```

- [ ] **Step 7: `refreshAll()`에서 갱신 시각 기록**

`web/src/served-app.ts`의 `refreshAll()` 안에서 `state.partialFailures = failures;` 줄 **바로 앞**에 추가한다.

```javascript
    state.lastRefreshedAt = Date.now();
```

`if (requestId !== state.refreshRequestId) return;` 뒤에 있으므로 취소된 갱신은 시각을 기록하지 않는다.

- [ ] **Step 8: `app.js` 재생성**

Run: `node web/scripts/build-served-asset.mjs`

Expected: exit 0. `web/app.js`가 갱신된다. `web/app.js`를 직접 편집하지 않는다.

- [ ] **Step 9: 가드가 통과하는지 확인**

Run: `npm run browser:fixture --prefix web`

Expected: PASS.

- [ ] **Step 10: 타입체크와 parity 확인**

Run: `npm test --prefix web && npm run generate:parity --prefix web && npm run verify:parity --prefix web`

Expected: 모두 exit 0.

- [ ] **Step 11: 커밋**

```bash
git add web/src web/app.js web/generated web/scripts/verify-browser-fixture.mjs
git commit -m "feat(web): bind status bar and hero chips to real state"
```

---

### Task 3: 전체 게이트 검증과 code-level evidence 기록

**Files:**
- Create: `docs/ga-ready/evidence/web-console-state-truthfulness-code-level-2026-08-05.md`

**Interfaces:**
- Consumes: Task 1, Task 2의 결과
- Produces: 없음 (최종 산출물)

- [ ] **Step 1: 전체 required 게이트 실행**

Run:

```powershell
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected: 전부 exit 0, 실패 0. 실제 통과 건수를 기록해 둔다.

- [ ] **Step 2: 잔존 조작값 0건 확인**

Run:

```bash
grep -nE ">Connected<|VM: 3/3|API: 10ms avg|Updated 0s ago|pcv-node-a|<strong>4/5</strong>" web/index.html
```

Expected: 매치 없음 (grep exit 1).

- [ ] **Step 3: evidence 문서 작성**

`docs/ga-ready/evidence/web-console-state-truthfulness-code-level-2026-08-05.md`를 만든다. Step 1에서 기록한 **실제** 수치를 채운다. 추정값을 쓰지 않는다.

```markdown
# Web Console 운영 상태 진실성 code-level PASS (2026-08-05)

## Evidence boundary

- Spec: `docs/superpowers/specs/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness-design.md`
- Plan: `docs/superpowers/plans/2026-08-05-purecvisor-desktop-node-web-console-state-truthfulness.md`
- 근거 감사: `docs/project-status-audit-2026-08-05.md` §6 P1-2
- Result: `CODE_LEVEL_PASS`
- Host/service mutation performed: `false`
- Hyper-V/VM mutation performed: `false`
- Installed listener execution: `not-run`
- Actual-VM validation performed: `false`
- Public trusted signing: `false`
- External stable publication: `false`

이 evidence는 code-level 범위다. 설치본 실제 브라우저 authenticated journey를 주장하지 않으며
operational anchor를 승격하지 않는다.

## 변경 내용

- `web/index.html`에서 조작된 운영값을 제거하고 실제 바인딩용 요소 id 8개를 노출했다.
- 계측 인프라가 없는 `WS`/`API latency` span을 삭제했다.
- `host.status`에 머신 이름 필드가 없으므로 호스트 식별자 자리를 `windows.caption`으로 재바인딩하고
  hero chip 라벨을 `호스트 모드`에서 `호스트 상태`로 바꿨다.
- `state.lastRefreshedAt`과 `hasRefreshedOperation()`으로 미로드와 실제 0을 구분한다.
- `getHostReadinessLabel()`은 `state.host`가 null이어도 `Ready`를 반환하므로 로드 게이트 뒤에서만 호출한다.

## 검증 결과

| 검증 | 결과 |
| --- | --- |
| `npm test --prefix web` | PASS |
| `npm run verify:parity --prefix web` | PASS |
| `node --check web/app.js` | PASS |
| Web Pester | <실제 수치> |
| Installer Pester | <실제 수치> |
| Packaging Pester | <실제 수치> |
| `git diff --check` | PASS |
| `index.html` 잔존 조작값 | `0`건 |

## Nonclaims

- Web secure bootstrap 또는 account auth 구성을 완료하지 않았다.
- 미인증 401 fan-out을 제거하지 않았다.
- 설치본 실제 Chromium E2E를 required gate에 추가하지 않았다.
- 제품 .NET 코드, packaging 제품 wrapper, workflow를 변경하지 않았다.
```

- [ ] **Step 4: evidence 인덱스 갱신**

`docs/ga-ready/EVIDENCE_INDEX.md`의 `## 2026-08-02 C# architecture Wave 1C …` 섹션 **앞**에, 날짜 순서를 지켜 다음 섹션을 추가한다. Step 1의 실제 수치를 채운다.

```markdown
## 2026-08-05 Web Console 운영 상태 진실성 code-level pass

- `docs/ga-ready/evidence/web-console-state-truthfulness-code-level-2026-08-05.md`는
  정적 셸이 표시하던 조작된 운영값을 제거하고 footer/hero를 실제 state에 바인딩한 결과를
  `CODE_LEVEL_PASS`로 기록한다.
- `GET /api/v1/host/status`에 머신 이름 필드가 없어 정적 `pcv-node-a`는 API가 제공하지 않는
  정보였다. 해당 자리는 `windows.caption`으로 재바인딩했고 계측 인프라가 없는 `WS`/`API latency`
  span은 삭제했다.
- 정적 가드(Web Pester)와 미인증 동작 가드(browser fixture)를 기존 required 게이트에 추가했다.
  Web/Installer/Packaging Pester <실제 수치>와 npm test/parity가 PASS했다.
- `host_mutation_performed=false`, `installed_product_changed=false`이며 operational anchor는
  `0.42.65-admin-smoke` 그대로다.
- 이 evidence는 code-level 범위이며 설치본 authenticated journey, public trusted signing,
  external stable publication을 주장하지 않는다.
```

- [ ] **Step 5: 최종 whitespace 확인**

Run: `git diff --check`

Expected: exit 0.

- [ ] **Step 6: 커밋**

```bash
git add docs/ga-ready/evidence/web-console-state-truthfulness-code-level-2026-08-05.md docs/ga-ready/EVIDENCE_INDEX.md
git commit -m "docs: record Web Console state truthfulness code-level evidence"
```

---

## 완료 조건

- `web/index.html`에 조작된 운영값 `0`건
- 미인증 fixture에서 푸터가 `Not connected`, `VM: —`, `Updated —`를 표시
- 정적 가드와 동작 가드가 RED → GREEN을 거쳐 통과
- Web/Installer/Packaging Pester, npm test, parity, `git diff --check` 전건 통과
- `src/**` (.NET), `packaging/**`, `.github/workflows/**` 변경 `0`건
