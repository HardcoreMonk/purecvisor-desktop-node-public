# 0.38 Evidence to Release Sequence Implementation Plan

> Superseded note: this plan captured the next-batch sequence when `0.38.0-admin-smoke` was the latest full admin host mutation evidence. The current canonical sequence baseline is `0.38.2-admin-smoke` and is specified in `docs/superpowers/specs/2026-05-06-purecvisor-desktop-node-next-batch-0382-canonical-evidence-and-evidence-api-hardening-design.md`.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Subagents are optional only when the user explicitly asks for parallel agent execution.

**Goal:** 사용자가 확정한 `1 -> 2 -> 3 -> 4` 순서대로 historical baseline `0.38.0-admin-smoke` evidence closure, Batch Supervisor evidence UX/API, Web Dashboard 제품형 UI, Internal `RequireSigned` release gate 준비를 배치 단위로 진행한다. 이 historical plan의 evidence baseline은 `0.38.2-admin-smoke`로 superseded됐다.

**Architecture:** 이 plan은 네 개 배치를 순차 gate로 고정한다. Batch 1은 당시 최신 host mutation evidence를 문서/guard에 고정하는 non-mutating closure이고, Batch 2는 evidence를 read-only data contract로 노출하는 기반 작업이며, Batch 3은 dashboard 정보 구조를 제품형으로 정리한다. Batch 4는 내부 서비스 운영용 `RequireSigned` release gate를 준비하되 실제 signed MSI/admin smoke는 별도 사용자 승인 전까지 실행하지 않는다.

**Tech Stack:** Markdown docs, PowerShell 7, Pester 5, C#/.NET API/Host, TypeScript Web Console, Node test/verify scripts, Batch Supervisor artifacts.

---

## 2026-05-06 종료 정리 업데이트

이 historical sequence는 `0.38.0-admin-smoke` 기준으로 작성됐고, 현재 canonical baseline은 `0.38.2-admin-smoke`다. 이 문서의 0.38.0 Batch 1 세부 체크박스는 historical/superseded 항목이라 완료 처리하지 않는다.

- [x] Batch 2 evidence API hardening은 PR `#4`로 병합했다. 구현 commit은 `c3163e23fad504677aac5d55f07c8124b9fb4d56`, merge commit은 `49dae6a5a6c1d79cd0deb936475ac4a8fe8f8940`이다.
- [x] Batch 3-A/3-B Web evidence dashboard/troubleshooting surface는 Batch 2 병합 상태에서 검증됐고, 별도 고유 diff 없이 완료 상태로 정리했다.
- [x] Batch 4 internal `RequireSigned` gate prep은 PR `#5`로 병합했다. 구현 commit은 `97b6fd892eca874486efdc6cd09cea9247c0c910`, merge commit은 `d9c833e70834647e6ff907ac6dc48745dcdf2adf`이다.
- [x] 전체 종료 evidence는 `docs/ga-ready/evidence/batch-follow-up-closure-2026-05-06.md`에 기록한다.

## 현재 기준점

`0.38.0-admin-smoke` was the latest Batch Supervisor full admin host mutation gate evidence when this historical plan was written. It has been superseded by `0.38.2-admin-smoke`.

- 현재 HEAD: `267fe6afa0480ebc3b03431490bc37fa251261ae`
- historical admin smoke baseline: `0.38.0-admin-smoke`
- Batch artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-001432-0380`
- Route/MSI/Hyper-V artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380`
- OS mutation artifact: `artifacts/os-mutation-gates-batch-profile-20260506-001432-0380`
- MSI SHA-256: `b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9`
- Signing mode: `AllowUnsignedDev`
- 최종 상태: service `Running`, installed DisplayVersion `0.38.0`, firewall final count `0`, Event Log source absent, internal Root/TrustedPublisher cert present, boot time unchanged, `pcv-spike-*` VM count `0`

## 실행 원칙

- 배치는 반드시 `1 -> 2 -> 3 -> 4` 순서로 진행한다.
- 각 배치 종료 시 commit/push까지 수행한다.
- Batch 1, 2, 3은 실제 Hyper-V/MSI/service/firewall/Event Log/trust-store mutation을 실행하지 않는다.
- Batch 4는 내부 `RequireSigned` gate 준비까지만 기본 범위다. 실제 signed MSI build, LocalMachine trust import, MSI lifecycle/admin smoke는 별도 사용자 승인 전까지 실행하지 않는다.
- Public trusted signing과 외부 stable publication은 scope 밖이다.

## File Structure

### Batch 1: `0.38.0-admin-smoke` Evidence Closure

- Create: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0380.md`
  - `0.38.0-admin-smoke` evidence summary 단일 문서.
- Modify: `AGENTS.md`
  - 당시 evidence anchor를 `0.38.0-admin-smoke`로 갱신.
- Modify: `README.md`
  - root evidence summary 최신 항목 추가.
- Modify: `docs/ADR_INDEX.md`
  - ADR-0004 evidence note를 `0.38.0`로 승격.
- Modify: `docs/DEVELOPER_INDEX.md`
  - 최신 evidence 문서 링크 추가.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - 당시 full admin gate 기준을 `0.38.0`로 갱신.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - `AllowUnsignedDev` admin-smoke와 public release boundary 구분 갱신.
- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
  - aggregate evidence historical pointer 추가.
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
  - ledger에 `0.38.0` entry 추가.
- Modify: `packaging/windows-desktop-node/README.md`
  - packaging evidence summary 당시 항목 추가.
- Modify: `packaging/windows-desktop-node/installer/README.md`
  - MSI lifecycle evidence 당시 항목 추가.
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`
  - `0.38.0` evidence 문서/anchor guard 추가.

### Batch 2: Batch Supervisor Evidence UX/API Foundation

- Modify: `src/DesktopNode.Api/**`
  - read-only evidence summary route를 추가할지 기존 diagnostics route에 포함할지 코드 구조를 보고 결정.
- Modify: `src/DesktopNode.Api.Tests/**`
  - artifact summary parsing, redaction, missing artifact structured failure test.
- Modify: `src/DesktopNode.Contracts/**`
  - 필요한 경우 read-only DTO 추가.
- Modify: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`
  - 필요 시 artifact summary schema 또는 compact report writer 보강.
- Modify: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`
  - attempt/retry/failure classification report guard 추가.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - read-only evidence report boundary 문서화.

### Batch 3: Web Dashboard Product UI

- Modify: `web/src/served-app.ts`
  - A main dashboard + B/C sub pages 구조에 evidence/batch status surface 연결.
- Modify: `web/src/app.ts`
  - fixture/view-model update.
- Modify: `web/index.html`
  - dashboard/sub page containers, nav labels, accessibility attributes.
- Modify: `web/styles.css`
  - 제품형 layout, dense ops UI, responsive constraints.
- Modify: `web/tests/**`
  - static/fixture/browser parity guard 갱신.
- Build output: `web/app.js`
  - repo 기존 방식에 따라 generated served app 갱신.

### Batch 4: Internal `RequireSigned` Release Gate Preparation

- Modify: `packaging/windows-desktop-node/installer/README.md`
  - current HEAD 기준 internal `RequireSigned` runbook 갱신.
- Modify: `docs/adr/0003-internal-trusted-signing-policy.md`
  - 필요한 경우 runbook pointer만 보강. 정책 자체 변경은 별도 ADR 없이는 하지 않는다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - signed build/admin smoke opt-in gate 명확화.
- Modify: `packaging/windows-desktop-node/installer/tests/**`
  - `RequireSigned` dry-run/provenance guard 보강.
- Modify: `packaging/windows-desktop-node/installer/build.ps1`
  - 필요한 경우 current gate를 깨지 않는 범위에서 missing validation만 보강.

## Batch 1: `0.38.0-admin-smoke` Evidence Closure

**Files:**
- Create: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0380.md`
- Modify: `AGENTS.md`
- Modify: `README.md`
- Modify: `docs/ADR_INDEX.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md`
- Modify: `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: evidence artifact 값을 다시 추출한다**

Run:

```powershell
$batch = 'artifacts/batch-runs/full-admin-host-mutation-gate-20260506-001432-0380'
$route = 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380'
$os = 'artifacts/os-mutation-gates-batch-profile-20260506-001432-0380'
$batchSummary = Get-Content -Raw -LiteralPath (Join-Path $batch 'summary.json') | ConvertFrom-Json
$routeSummary = Get-Content -Raw -LiteralPath (Join-Path $route 'summary.json') | ConvertFrom-Json
$lifecycle = Get-Content -Raw -LiteralPath (Join-Path $route 'msi-lifecycle-smoke.json') | ConvertFrom-Json
$provenance = Get-Content -Raw -LiteralPath (Join-Path $route 'PureCVisorDesktopNode-0.38.0-admin-smoke-windows-x64.provenance.json') | ConvertFrom-Json
$osSummary = Get-Content -Raw -LiteralPath (Join-Path $os 'summary.json') | ConvertFrom-Json
[pscustomobject]@{
  batch_ok = $batchSummary.ok
  route_ok = $routeSummary.ok
  lifecycle_ok = $lifecycle.ok
  os_ok = $osSummary.ok
  git_commit = $provenance.git_commit
  msi_sha256 = $provenance.msi.sha256
  signing_mode = $provenance.signing_mode
  installed_version = '0.38.0'
  final_service_state = $osSummary.final_service.state
  firewall_final_count = $osSummary.final_firewall_rule_count
  eventlog_final_present = $osSummary.final_eventlog_source_present
  root_present = $osSummary.final_trust_store.root_present
  publisher_present = $osSummary.final_trust_store.publisher_present
  boot_time_unchanged = $osSummary.boot_time_unchanged
} | ConvertTo-Json -Depth 8
```

Expected:

```text
batch_ok=true
route_ok=true
lifecycle_ok=true
os_ok=true
git_commit=267fe6afa0480ebc3b03431490bc37fa251261ae
msi_sha256=b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9
signing_mode=AllowUnsignedDev
final_service_state=Running
firewall_final_count=0
eventlog_final_present=false
root_present=true
publisher_present=true
boot_time_unchanged=true
```

- [ ] **Step 2: docs guard failing test를 먼저 갱신한다**

In `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`, add or update assertions so the docs must contain:

```powershell
'0.38.0-admin-smoke'
'full-admin-host-mutation-gate-20260506-001432-0380'
'routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380'
'os-mutation-gates-batch-profile-20260506-001432-0380'
'267fe6afa0480ebc3b03431490bc37fa251261ae'
'b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9'
'AllowUnsignedDev'
'public trusted signing 또는 외부 stable publication'
```

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
```

Expected: fail before docs are updated.

- [ ] **Step 3: evidence detail 문서를 생성한다**

Create `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0380.md` with this structure:

```markdown
# 0.38.0 Full Admin Host Mutation Gate Evidence

## Summary

- Version: `0.38.0-admin-smoke`
- Commit: `267fe6afa0480ebc3b03431490bc37fa251261ae`
- MSI SHA-256: `b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9`
- Signing mode: `AllowUnsignedDev`
- Batch artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-001432-0380`
- Route/MSI/Hyper-V artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-001432-0380`
- OS mutation artifact: `artifacts/os-mutation-gates-batch-profile-20260506-001432-0380`

## Result

`summary.json` recorded `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`.

## Route/MSI/Hyper-V

Service-action, MSI lifecycle, installed Hyper-V API route smoke all passed. MSI lifecycle install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, and final-restore-install all exited `0`.

## OS Mutation Gate

Config migration guard, Event Log register/remove, firewall enable/remove, LAN listener IP smoke, and internal trust-store install/remove/restore all passed.

## Final Host State

- Service: `PureCVisorDesktopNode` `Running`, `Auto`, loopback-only
- Installed DisplayVersion: `0.38.0`
- Firewall final count: `0`
- Event Log source final present: `false`
- Internal Root cert final present: `true`
- Internal TrustedPublisher cert final present: `true`
- Boot time unchanged: `true`
- `pcv-spike-*` VM count: `0`

## Boundary

This evidence is an `AllowUnsignedDev` admin-smoke result. It is not public trusted signing evidence and does not claim external stable publication.
```

- [ ] **Step 4: high-level docs를 당시 `0.38.0` 기준으로 갱신한다**

Update docs so stale latest wording no longer points only to `0.37.0`:

```text
AGENTS.md
README.md
docs/ADR_INDEX.md
docs/DEVELOPER_INDEX.md
docs/DEVELOPMENT_VERIFICATION_POLICY.md
docs/PUBLIC_RELEASE_BOUNDARY.md
docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md
docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md
packaging/windows-desktop-node/README.md
packaging/windows-desktop-node/installer/README.md
```

Required wording:

```text
`0.38.0-admin-smoke` was the latest Batch Supervisor full admin host mutation gate evidence when this historical plan was written. It has been superseded by `0.38.2-admin-smoke`.
```

Also keep:

```text
이 evidence는 public trusted signing 또는 외부 stable publication evidence가 아니다.
```

- [ ] **Step 5: Batch 1 verification을 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Expected:

```text
PcvAdminSmokeEvidenceDocs.Tests.ps1 PASS
packaging/windows-desktop-node/tests PASS
git diff --check exit 0
```

- [ ] **Step 6: Batch 1 commit/push**

Run:

```powershell
git status -sb
git add AGENTS.md README.md docs/ADR_INDEX.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-06-0380.md docs/ga-ready/evidence/aggregate-gate-closure-2026-05-05.md docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/installer/README.md packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "Document 0.38.0 full admin smoke evidence"
git push
git status -sb
```

Expected: pushed commit and clean worktree.

## Batch 2: Batch Supervisor Evidence UX/API Foundation

**Files:** exact files are finalized after reading current API/diagnostics structure.

- [ ] **Step 1: inspect existing API and diagnostics boundaries**

Run:

```powershell
rg -n "diagnostic|evidence|artifact|summary|runtime/policy|jobs|events" src/DesktopNode.Api src/DesktopNode.Contracts src/DesktopNode.Api.Tests packaging/windows-desktop-node/tools web/src web/tests
```

Expected: identify the existing read-only route or diagnostics module that should own evidence summary.

- [ ] **Step 2: write failing tests for read-only evidence summary**

Tests must cover:

```text
missing artifact root -> structured failure, no exception leak
summary.json parse -> batch_id, status, step count, attempt_count
route parity summary -> MSI sha256, signing mode, lifecycle ok
OS gate summary -> firewall final count, Event Log final source, trust-store final state
redaction -> no API token, protected token blob, absolute token file content
```

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: fail before implementation.

- [ ] **Step 3: implement minimal read-only summary contract**

Allowed shape:

```json
{
  "ok": true,
  "data": {
    "batch_id": "full-admin-host-mutation-gate-20260506-001432-0380",
    "status": "completed",
    "version": "0.38.0-admin-smoke",
    "steps": [
      { "id": "service-msi-hyperv-admin-smoke", "ok": true, "attempt_count": 1, "retry_count": 1 },
      { "id": "os-mutation-gate", "ok": true, "attempt_count": 1, "retry_count": 0 }
    ],
    "evidence": {
      "msi_sha256": "b342ff4037ff2b4c9156f8a4556864a655b177a015bf79509ab89ac649e572e9",
      "signing_mode": "AllowUnsignedDev",
      "public_trusted_signing": "excluded"
    }
  }
}
```

Do not add write/mutation endpoints.

- [ ] **Step 4: docs and verification**

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

- [ ] **Step 5: Batch 2 commit/push**

Run:

```powershell
git add src/DesktopNode.Api src/DesktopNode.Api.Tests src/DesktopNode.Contracts packaging/windows-desktop-node/tools packaging/windows-desktop-node/tests docs/DEVELOPMENT_VERIFICATION_POLICY.md
git commit -m "Add read-only batch evidence summary contract"
git push
```

## Batch 3: Web Dashboard Product UI

**Files:** `web/src/served-app.ts`, `web/src/app.ts`, `web/index.html`, `web/styles.css`, `web/tests/**`, `web/app.js`.

- [ ] **Step 1: inspect current dashboard views**

Run:

```powershell
rg -n "VALID_VIEWS|dashboard|troubleshooting|jobs|activity|render" web/src web/tests web/index.html web/styles.css
```

- [ ] **Step 2: failing web tests**

Add tests for:

```text
A main dashboard shows latest evidence status and service state
B dedicated evidence/batch page shows attempts/retry/failure classification
C troubleshooting page keeps incident command/job diagnostics surface
mobile/desktop layout does not hide nav or overflow core status labels
```

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
```

Expected: fail before implementation.

- [ ] **Step 3: implement UI structure**

Information architecture:

```text
A Main Dashboard:
  service/runtime status
  latest evidence badge
  active jobs / recent activity

B Evidence / Batch Runs:
  batch status
  step attempt cards/table
  retry_count and final_attempt
  MSI SHA/signing mode/boundary

C Troubleshooting:
  failed jobs
  diagnostics links
  incident command actions
  structured failure guide
```

Use existing UI patterns. Do not add decorative landing page sections.

- [ ] **Step 4: web verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
git diff --check
```

- [ ] **Step 5: Batch 3 commit/push**

Run:

```powershell
git add web
git commit -m "Improve dashboard evidence and troubleshooting views"
git push
```

## Batch 4: Internal `RequireSigned` Release Gate Preparation

**Files:** exact files are finalized after installer/signing test inspection.

- [ ] **Step 1: inspect existing signing gate**

Run:

```powershell
rg -n "RequireSigned|SigningTrustModel|InternalEnterprise|SignTool|Authenticode|New-PcvInternalCodeSigningTrust" packaging/windows-desktop-node/installer docs src
```

- [ ] **Step 2: failing dry-run/provenance tests**

Tests must assert:

```text
rc/stable reject AllowUnsignedDev
RequireSigned requires SigningTrustModel
InternalEnterprise provenance records internal trust model
private key/PFX/password/token values are not written to provenance
runbook separates dry-run from LocalMachine trust import/admin smoke
```

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected: fail only where new guard is missing.

- [ ] **Step 3: implement non-mutating gate prep**

Allowed changes:

```text
dry-run validation
structured build plan/provenance fields
docs/runbook
Pester guard
```

Disallowed without separate approval:

```text
actual internal Root/leaf creation
LocalMachine trust import
signed MSI build
msiexec install/repair/uninstall
Hyper-V/service/firewall/Event Log/trust-store mutation
```

- [ ] **Step 4: verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

- [ ] **Step 5: Batch 4 commit/push**

Run:

```powershell
git add packaging/windows-desktop-node/installer docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/adr/0003-internal-trusted-signing-policy.md
git commit -m "Prepare internal RequireSigned release gate"
git push
```

## Verification Matrix

| Batch | Required verification | Host mutation |
| --- | --- | --- |
| 1 | `PcvAdminSmokeEvidenceDocs.Tests.ps1`, packaging Pester, `git diff --check` | No |
| 2 | `dotnet test src/DesktopNode.sln`, packaging Pester, `git diff --check` | No |
| 3 | web Pester, `npm test --prefix web`, `npm run verify:parity --prefix web`, `node --check web/app.js`, `git diff --check` | No |
| 4 | installer Pester, packaging Pester, `git diff --check` | No by default |

## Stop Conditions

- A test fails after two fix attempts.
- Any implementation requires actual signed MSI build or LocalMachine trust mutation before Batch 4 explicit approval.
- Any dashboard API design would require write/mutation endpoints.
- Any doc wording implies public trusted signing or external stable publication.

## Self-Review

- Spec coverage: user-selected `1 -> 2 -> 3 -> 4` is represented as four sequential batches with file scopes, tests, commit/push gates.
- Placeholder scan: no incomplete implementation placeholders are required to execute Batch 1. Batch 2-4 intentionally defer exact file ownership until the first inspect step because code ownership must follow current API/web/installer structure.
- Type consistency: evidence constants match the actual `0.38.0-admin-smoke` run artifacts and MSI provenance.
- Safety: only Batch 1 is ready for immediate execution after approval; no host mutation is included in Batch 1.
