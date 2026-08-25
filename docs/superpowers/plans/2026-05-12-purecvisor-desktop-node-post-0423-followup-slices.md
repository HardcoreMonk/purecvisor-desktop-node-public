# Post-0423 Follow-up Slices Implementation Plan

> **Agentic worker 필수 하위 skill:** 이 계획을 실행할 때는 `superpowers:executing-plans` 또는 동등한 task-by-task 실행 흐름을 사용한다. 각 단계는 checkbox로 추적한다.

**Goal:** `0.42.3-admin-smoke` full host gate 이후 MANUAL-ADMIN 1-2-3-4와 ADR-0006 package-pair rebaseline을 `0.42.4-admin-smoke` target으로 다시 닫는다.

**Architecture:** Baseline host gate는 이미 `0.42.3-admin-smoke`가 소유한다. 새 작업은 target package generation, manual-admin runner input alignment, public-boundary drift guard, Runtime/Core installed summary contract를 분리해서 진행한다.

**Tech Stack:** PowerShell runner, WiX MSI build, .NET DesktopNode solution, Pester documentation/runner tests, Markdown evidence.

---

## File Structure

- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`
- Modify: `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md`
- Modify: `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Create: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`
- Create or update: `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`
- Use existing runner tools:
  - `packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1`
  - `packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1`
  - `packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1`
  - `packaging/windows-desktop-node/tools/Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1`
  - `packaging/windows-desktop-node/tools/Invoke-PcvCredentialManagerDefaultTransitionSmoke.ps1`
  - `packaging/windows-desktop-node/tools/Invoke-PcvWindowsEventLogDefaultTransitionSmoke.ps1`
  - `packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1`

## Task 1: Package Pair Input 고정

**Files:**
- Modify: `docs/ga-ready/MANUAL_ADMIN_NEXT_CAMPAIGN_DESCRIPTOR.md`
- Test: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: 0.42.4 package를 빌드한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.4-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260512-0424 -SigningMode AllowUnsignedDev -WixPath "$env:USERPROFILE\.dotnet\tools\wix.exe"
```

Expected: MSI, `.sha256`, `.provenance.json`, `.publication.json`가 생성된다.

- [ ] **Step 2: descriptor에 baseline/target을 기록한다**

Expected fields:

```text
baseline_version: `0.42.3-admin-smoke`
target_version: `0.42.4-admin-smoke`
baseline_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md`
target_package_artifact_root: `artifacts/admin-smoke-package-20260512-0424`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`
```

- [ ] **Step 3: descriptor Pester expectation을 갱신한다**

Add assertions that descriptor contains `0.42.3-admin-smoke`, `0.42.4-admin-smoke`, `AllowUnsignedDev`, and `not-claimed`.

- [ ] **Step 4: focused documentation test를 실행한다**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed"
```

Expected: PASS.

## Task 2: MANUAL-ADMIN 1-2-3-4 실행

**Files:**
- Create: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0423-0424.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Modify: `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`

- [ ] **Step 1: Baseline host gate를 확인한다**

Use:

```text
docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-12-0423-hostmutation.md
artifacts/batch-runs/full-admin-host-mutation-gate-20260512-021337-0423
```

Expected: baseline `0.42.3-admin-smoke`, final service `Running`, Web `200`, API unauthenticated `401`.

- [ ] **Step 2: Operator Access bucket을 실행한다**

Run account/noVNC/TUI smoke with artifact roots containing `20260512-0423-0424`.

Expected:

```text
installed account login: PASS
target-backed noVNC streaming: PASS
installed TUI smoke: PASS
token/password value exposure: false
```

- [ ] **Step 3: Internal Service Hardening bucket을 실행한다**

Run TLS, Credential Manager, Event Log, and service token rotation/revoke installed smoke.

Expected:

```text
tls lifecycle: generate-bind-rotate-remove-pass
credential manager: system-context-proof-pass
event log: default-writer-pass and provider-repair-pass
service token: old-token-rejected-after-reload
```

- [ ] **Step 4: Lifecycle / Packaging bucket을 실행한다**

Run update/rollback, clean-host, Burn, MSIX, and MSI/update package apply using `0.42.3 -> 0.42.4 -> 0.42.3`.

Expected:

```text
installed update exit: 0
rollback exit: 0
clean-host install/update/rollback: PASS
Burn lifecycle: PASS
MSIX lifecycle: PASS
```

- [ ] **Step 5: evidence 문서를 작성한다**

The evidence document must include:

```text
evidence_id: manual-admin-campaign-2026-05-12-0423-0424
scope: manual-admin-groups-1-2-3-4
baseline_version: 0.42.3-admin-smoke
target_version: 0.42.4-admin-smoke
host_mutation_performed: true
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
```

## Task 3: Public Boundary Drift Guard

**Files:**
- Modify: `docs/adr/0005-public-distribution-operations-expansion-candidate.md`
- Modify: `docs/ga-ready/PUBLIC_DISTRIBUTION_GATE_MATRIX.md` if drift is found
- Test: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: public claim drift scan을 실행한다**

Run:

```powershell
rg -n "public trusted signing.*pass|external stable publication.*pass|winget_submission:\s*pass|public_release:\s*claimed" docs packaging/windows-desktop-node
```

Expected: no matches.

- [ ] **Step 2: 0423-0424 evidence에 boundary 문구를 고정한다**

Expected text:

```text
이 evidence는 internal/private network administrator opt-in evidence이며 public trusted signing, external stable publication, winget submission, public clean-host signed smoke를 주장하지 않는다.
```

- [ ] **Step 3: Pester drift assertion을 추가한다**

Expected assertions: evidence doc and indexes contain `public trusted signing` and `external stable publication` with `out-of-scope` or `not-claimed`.

## Task 4: Runtime/Core, Host Ops, Packaging 다음 Slice 실행

**Files:**
- Modify or create tests in `src/DesktopNode.Api.Tests/`
- Modify or create tests in `src/DesktopNode.Host.Tests/`
- Modify packaging tests in `packaging/windows-desktop-node/tests/`

- [ ] **Step 1: Runtime/Core installed summary contract test를 추가한다**

Expected coverage: ops summary includes version, service state, auth boundary, diagnostics root, and latest full admin evidence anchor without reading public release fields as PASS.

- [ ] **Step 2: Host Ops runner preflight contract를 추가한다**

Expected coverage: Credential Manager/Event Log/TLS/service token runners fail early when service path or token source does not match the declared baseline.

- [ ] **Step 3: Packaging campaign descriptor contract를 추가한다**

Expected coverage: package pair descriptors reject mixed `0.42.3` baseline and non-`0.42.4` target inputs.

- [ ] **Step 4: verification을 실행한다**

Run:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
git diff --check
```

Expected: all pass.
