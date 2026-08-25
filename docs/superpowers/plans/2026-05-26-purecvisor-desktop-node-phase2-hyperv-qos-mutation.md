# Phase 2 Hyper-V QoS Mutation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hyper-V storage/network QoS mutation을 preview -> queued apply -> readback -> rollback evidence 순서로 제품화한다. `pcvcli vm blkio-set`과 `pcvcli vm bandwidth-set`은 Linux 호환 claim이 아니라 Desktop Node Hyper-V policy command로 구현한다.

**Architecture:** Local API는 QoS preview/apply route를 제공하고 job runtime을 통해 apply만 host mutation을 수행한다. Hyper-V domain은 planner/executor/readback/audit를 분리한다. CLI는 dry-run과 apply를 command-specific UX로 제공한다. Web/TUI direct control은 Phase 3까지 열지 않는다.

**Tech Stack:** .NET 10 C#, xUnit, PowerShell manual-admin smoke, Hyper-V WMI/CIM provider, existing PCVCLI formatter, Pester evidence docs guard.

---

## 2026-05-29 Follow-up: Value-boundary hardening

- [x] Local API preview route가 invalid QoS range를 native adapter 호출 전에 거절한다.
- [x] Local API apply route가 invalid QoS range를 job queue 생성 전에 거절한다.
- [x] PCVCLI `vm blkio-set`과 `vm bandwidth-set`이 같은 range contract를 command-specific
  error로 반환하고 전체 `Usage:` block을 출력하지 않는다.
- [x] `0`은 rollback/manual restore 값으로 계속 허용한다.
- [x] Evidence:
  `docs/ga-ready/evidence/hyperv-qos-mutation-value-hardening-code-level-2026-05-29.md`.
- [ ] `0.42.59-admin-smoke` package build, full admin host mutation, manual-admin package-pair,
  installed Web/TUI/CLI current-card smoke로 제품화 gate를 닫는다.

## 1. Scope Lock

- [ ] `docs/adr/0008-hyperv-qos-mutation-policy.md`와 `docs/superpowers/specs/2026-05-26-purecvisor-desktop-node-phase2-hyperv-qos-mutation-design.md`를 읽고 Phase 2만 구현한다.
- [ ] 포함 범위는 Local API preview/apply, Hyper-V domain planner/executor, CLI `blkio-set`/`bandwidth-set`, actual VM admin smoke, evidence 갱신이다.
- [ ] 제외 범위는 Web/TUI direct control, Guest Execution, noVNC target config mutation, Linux Single Runtime Object 계열이다.
- [ ] ADR-0007 readback-first 경계는 Phase 2 evidence가 닫히기 전까지 유지한다.

## 2. Contract Tests First

- [ ] `src/DesktopNode.Contracts.Tests`에 `hyperv-qos-mutation-preview.v1` DTO/serialization test를 추가한다.
- [ ] validation error가 secret, credential, token, local username을 출력하지 않는 redaction test를 추가한다.
- [ ] storage/network unsupported reason code가 stable string으로 나오는지 테스트한다.
- [ ] failing test를 먼저 확인한다.

## 3. Hyper-V Domain Planner

- [ ] `DesktopNodeHyperVQosMutationPlanner`를 추가한다.
- [ ] planner는 VM, disk, adapter lookup과 current policy readback만 수행하고 host mutation을 수행하지 않는다.
- [ ] planner output에 `host_mutation_performed=false`, rollback descriptor 초안, readback route를 포함한다.
- [ ] disk/adapter ambiguity, missing VM, invalid numeric range, unsupported host capability test를 추가한다.

## 4. API Preview Routes

- [ ] `POST /api/v1/vms/{vm}/qos/storage/preview` route를 추가한다.
- [ ] `POST /api/v1/vms/{vm}/qos/network/preview` route를 추가한다.
- [ ] preview route는 auth/session/job runtime mutation을 수행하지 않고 planner 결과만 반환한다.
- [ ] problem-details contract와 request id propagation을 테스트한다.

## 5. Queued Apply Jobs

- [ ] `vm.qos.storage.set` job operation을 runtime policy에 추가한다.
- [ ] `vm.qos.network.set` job operation을 runtime policy에 추가한다.
- [ ] `DesktopNodeHyperVQosMutationExecutor`를 추가하고 apply 전 previous policy를 capture한다.
- [ ] job artifact에 `previous_policy`, `applied_policy`, `rollback_plan`, `readback_before`, `readback_after`, `audit`를 남긴다.
- [ ] rollback route를 즉시 열지 않더라도 manual restore descriptor가 충분한지 테스트한다.

## 6. CLI Commands

- [ ] `pcvcli vm blkio-set <vm> --disk <path-or-id> --maximum-iops N [--minimum-iops N] [--dry-run] [--yes]`를 구현한다.
- [ ] `pcvcli vm bandwidth-set <vm> --adapter <name-or-id> --maximum-kbps N [--minimum-kbps N] [--dry-run] [--yes]`를 구현한다.
- [ ] `--dry-run`은 preview table/json을 출력한다.
- [ ] `--yes` 없는 non-interactive apply는 command-specific confirmation error를 출력한다.
- [ ] 전체 `Usage:` block이 다시 출력되지 않도록 regression test를 추가한다.

## 7. Actual VM Admin Smoke

- [ ] 새 admin smoke script 또는 기존 manual-admin runner에 QoS mutation campaign을 추가한다.
- [ ] storage preview -> apply -> readback -> rollback/manual restore -> final readback을 기록한다.
- [ ] network preview -> apply -> readback -> rollback/manual restore -> final readback을 기록한다.
- [ ] artifact root에 summary, redacted command transcript, readback before/after, rollback descriptor를 저장한다.
- [ ] secret scan을 통과해야 closure로 인정한다.

## 8. Package/Manual-admin Closure

- [ ] Phase 2 product payload package candidate를 생성한다.
- [ ] full admin host mutation gate를 실행한다.
- [ ] manual-admin package-pair descriptor/readiness를 생성하고 clean-host with Windows Update, Burn, MSIX, installed update/rollback을 닫는다.
- [ ] installed Web/TUI/CLI current-card를 재확인한다.

## 9. Docs and Evidence

- [ ] `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`에 Phase 2 package/fullgate/manual-admin anchor를 기록한다.
- [ ] `docs/ga-ready/EVIDENCE_INDEX.md`, `docs/ga-ready/CONTROL_PLANE_INDEX.md`, `docs/ADR_INDEX.md`, `docs/DEVELOPER_INDEX.md`를 갱신한다.
- [ ] `docs/CLI_COMMAND_USAGE.md`와 `src/DesktopNode.Cli/README.md`의 지원 상태를 implementation evidence 기준으로 전환한다.
- [ ] Pester evidence docs guard를 추가/갱신한다.

## Verification

```powershell
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore --filter Qos
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter Qos
dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter Qos
pwsh -NoProfile -File packaging\windows-desktop-node\tools\Invoke-PcvInstalledQosMutationSmoke.ps1 -Version <candidate>
pwsh -NoProfile -Command "Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed"
git diff --check
```
