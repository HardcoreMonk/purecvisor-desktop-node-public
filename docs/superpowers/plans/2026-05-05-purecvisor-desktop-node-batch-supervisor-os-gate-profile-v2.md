# Batch Supervisor v2 OS Gate Profile 구현 계획

> **Agentic worker 필수 조건:** 이 계획을 실행할 때는 superpowers:subagent-driven-development 또는 superpowers:executing-plans를 사용한다. 단계 추적은 checkbox(`- [ ]`) 형식으로 한다.

**목표:** Batch Supervisor에 host-mutating admin gate profile을 1차 제품 검증 루프에 맞게 추가한다. 다음부터 Service/MSI/Hyper-V route parity와 firewall/LAN/Event Log/trust-store OS mutation gate를 수동 ad-hoc command가 아니라 manifest/profile/heartbeat/resume artifact로 실행할 수 있어야 한다.

**아키텍처:** `PcvBatchSupervisor.psm1`는 profile expansion, execution guard, heartbeat, timeout, redaction만 소유한다. 실제 OS gate 절차는 새 repo-local runner `Invoke-PcvOsMutationGateSmoke.ps1`가 소유한다. 두 파일 모두 product runtime이 아니며 `DesktopNode.Host.exe` 제품 동작을 변경하지 않는다. v2 구현 batch에서는 실제 Hyper-V, MSI, service, firewall, LAN, Event Log, trust-store mutation을 실행하지 않고 profile dry-run과 `-PlanOnly` smoke만 검증한다.

**기술 스택:** PowerShell 7, Pester 5, Batch Supervisor JSON manifest, 기존 `DesktopNode.Host.exe service-action`, 기존 `Invoke-PcvRouteParityMutationSmoke.ps1`.

---

## 상태

- 작성 기준: 2026-05-05
- 구현 상태: 완료
- 실행 방식: non-mutating implementation batch 완료
- 구현 중 host mutation: 실행하지 않음
- Admin smoke 재실행: 구현 완료 후 별도 사용자 승인으로 `0.37.0` 및 후속 `0.38.x` gates에서 확인
- 문서 checkbox closure: 2026-05-07 문서 상태 정리로 완료
- Public trusted signing: excluded
- External stable publication: not-claimed

## 완료 증거

- `PcvBatchSupervisor.Tests.ps1`: 18 passed
- `PcvOsMutationGateSmoke.Tests.ps1`: 6 passed
- `packaging/windows-desktop-node/tests`: 134 passed
- `git diff --check`: pass, line-ending warning only
- 실제 Hyper-V, service, MSI, firewall, LAN, Event Log, trust-store, Task Scheduler, reboot action은 실행하지 않았다.
- OS mutation gate rerun은 실행하지 않았고 최신 OS gate는 `0.35.7-admin-smoke`와 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`로 유지한다.

## 현재 사실

- Batch Supervisor v1 profile은 `PackagingRegression`, `WebRegression`만 제공한다.
- v1은 host-mutating step에 `requires_admin=true` 또는 `mutates_host=true`가 있으면 `-AllowHostMutation`과 elevated shell을 요구한다.
- v1은 `Restart-Computer`, `Stop-Computer`, `shutdown.exe`, `schtasks.exe` command를 금지한다.
- `0.36.1-admin-smoke`는 manual one-step manifest로 `Invoke-PcvRouteParityMutationSmoke.ps1`를 Batch Supervisor 아래에서 실행했다.
- 최신 full OS mutation gate는 `0.35.7-admin-smoke`이며 artifact는 `artifacts/os-mutation-gates-20260505-180434-0357-rerun`이다.
- OS gate의 tracked 단일 entrypoint는 아직 없다. 최신 evidence에는 `eventlog-register`, `eventlog-remove`, `firewall-enable`, LAN bearer static asset probe, `firewall-remove`, internal trust-store install/remove/restore 결과가 artifact로 남아 있다.

## 범위

### 포함

- `ServiceMsiHyperVAdminSmoke` Batch Supervisor profile 추가
- `OsMutationGate` Batch Supervisor profile 추가
- `FullAdminHostMutationGate` composite profile 추가
- profile-specific options 입력 지원
- OS gate runner `packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1` 추가
- OS gate runner `-PlanOnly` 추가
- Pester tests for profile expansion, guard behavior, dry-run, plan-only script contract
- README와 verification policy 갱신

### 제외

- 실제 `Invoke-PcvRouteParityMutationSmoke.ps1` admin smoke 실행
- 실제 `Invoke-PcvOsMutationGateSmoke.ps1` mutating 실행
- Hyper-V VM create/start/poweroff/delete
- MSI install/repair/uninstall/`REMOVE_DATA=1`
- service install/start/stop/delete
- firewall enable/remove
- LAN listener exposure
- Event Log source register/remove
- trust-store install/remove/restore
- Task Scheduler 등록
- reboot 또는 reboot-capable command
- product runtime/API/host behavior 변경

## 파일 구조

- 수정: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`
- 수정: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`
- 생성: `packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1`
- 생성: `packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1`
- 수정: `packaging/windows-desktop-node/README.md`
- 수정: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

## 설계 세부사항

### Profile Options

`New-PcvBatchSupervisorManifest`에 선택 parameter `-ProfileOptions`를 추가하고 이를 `New-PcvBatchSupervisorProfileSteps`로 전달한다.

필수 option 이름:

```text
ServiceMsiHyperVAdminSmoke:
  version
  iso_path
  routeparity_artifact_root
  timeout_seconds 선택, 기본값 3600

OsMutationGate:
  version
  routeparity_artifact_root
  os_gate_artifact_root
  lan_prefix
  product_root 선택, 기본값 C:\Program Files\PureCVisor\DesktopNode
  data_root 선택, 기본값 %ProgramData%\PureCVisor\desktop-node
  timeout_seconds 선택, 기본값 1800

FullAdminHostMutationGate:
  version
  iso_path
  routeparity_artifact_root
  os_gate_artifact_root
  lan_prefix
  timeout_seconds_routeparity 선택, 기본값 3600
  timeout_seconds_os_gate 선택, 기본값 1800
```

`ProfileOptions` validation은 non-empty 값과 정규화된 artifact path만 요구한다. 실제 파일 존재 여부 확인은 manifest generation이 아니라 runner preflight가 소유하므로, 테스트는 fake path를 안전하게 사용할 수 있다.

### 생성 Step

`ServiceMsiHyperVAdminSmoke`는 다음 step으로 확장된다.

```text
id: service-msi-hyperv-admin-smoke
file_name: pwsh
arguments:
  -NoProfile
  -ExecutionPolicy
  Bypass
  -File
  packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1
  -Version
  <version>
  -IsoPath
  <iso_path>
  -ArtifactRoot
  <routeparity_artifact_root>
requires_admin: true
mutates_host: true
```

`OsMutationGate`는 다음 step으로 확장된다.

```text
id: os-mutation-gate
file_name: pwsh
arguments:
  -NoProfile
  -ExecutionPolicy
  Bypass
  -File
  packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1
  -Version
  <version>
  -RouteParityArtifactRoot
  <routeparity_artifact_root>
  -ArtifactRoot
  <os_gate_artifact_root>
  -LanPrefix
  <lan_prefix>
requires_admin: true
mutates_host: true
```

`FullAdminHostMutationGate`는 위 두 step을 이 순서로 확장한다. Resume은 command fingerprint와 successful step result가 일치할 때만 route parity step을 skip할 수 있다.

### OS Gate Runner 계약

`Invoke-PcvOsMutationGateSmoke.ps1` parameter:

```powershell
param(
    [Parameter(Mandatory)][string]$Version,
    [Parameter(Mandatory)][string]$RouteParityArtifactRoot,
    [Parameter(Mandatory)][string]$ArtifactRoot,
    [Parameter(Mandatory)][string]$LanPrefix,
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$DataRoot = (Join-Path $env:ProgramData 'PureCVisor\desktop-node'),
    [switch]$PlanOnly
)
```

`PlanOnly`가 아닐 때 실행 순서:

1. Preflight에서 admin 상태, route parity summary `ok=true`, installed product root, `DesktopNode.Host.exe`, protected token file, service running, boot time snapshot을 확인한다.
2. Service running 상태에서 `DesktopNode.Host.exe service-action config-migration-apply`를 실행하고 `PCV_CONFIG_MIGRATION_SERVICE_RUNNING`, `MutationPlanned=false`, `MutationPerformed=false`를 요구한다.
3. `eventlog-register`를 실행하고 source가 존재하며 owned 상태인지 검증한다.
4. `eventlog-remove`를 실행하고 source absence를 검증한다.
5. `firewall-enable --allow-lan`을 실행하고 owned rule enabled 상태를 검증한다.
6. `LanPrefix`에 temporary `DesktopNode.Host.exe listen` process를 `--allow-lan`, installed web root, installed job store, installed event log, protected token file로 시작한다. Bearer Authorization으로 `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js`를 probe하고 HTTP `200`을 요구한다. 종료 시 이 temporary listener process만 중지한다.
7. `firewall-remove`를 실행하고 final rule count `0`을 검증한다.
8. 현재 ADR-0003 internal Root와 TrustedPublisher certificate를 `existing-trust-certs`로 export한다.
9. Export된 certificate와 exact thumbprint로 `trust-store-install --release-approved`를 실행한다.
10. Exact thumbprint로 `trust-store-remove --release-approved`를 실행하고 temporary absence를 검증한다.
11. Internal Root/TrustedPublisher final state 복구를 위해 `trust-store-install --release-approved`를 다시 실행한다.
12. Final service loopback `Running`, final firewall rule count `0`, Event Log source absent, internal trust Root/TrustedPublisher present, boot time unchanged를 기록한다.

`PlanOnly`는 모든 would-run command와 plan-only summary의 `mutates_host=false`를 `summary.json`에 기록한다. `DesktopNode.Host.exe`, `msiexec.exe`, Hyper-V cmdlet, firewall COM, Event Log registry write, X509Store write를 실행하면 안 된다.

## Task 1: RED - Batch Supervisor Profile Test

**파일:**

- Modify: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`

- [x] `ServiceMsiHyperVAdminSmoke`가 `Invoke-PcvRouteParityMutationSmoke.ps1`, `-Version`, `-IsoPath`, `-ArtifactRoot`를 포함한 admin/mutating step 1개로 확장되는 test를 추가한다.
- [x] `OsMutationGate`가 `Invoke-PcvOsMutationGateSmoke.ps1`, `-Version`, `-RouteParityArtifactRoot`, `-ArtifactRoot`, `-LanPrefix`를 포함한 admin/mutating step 1개로 확장되는 test를 추가한다.
- [x] `FullAdminHostMutationGate`가 순서가 고정된 admin/mutating step 2개로 확장되는 test를 추가한다.
- [x] 모든 admin profile이 `-AllowHostMutation` 없이 reject되는 test를 추가한다.
- [x] 모든 admin profile이 `-AllowHostMutation`과 `-IsAdministrator $false` 조합에서 reject되는 test를 추가한다.
- [x] `Invoke-PcvBatchSupervisor -DryRun -AllowHostMutation -IsAdministrator $true`가 admin profile summary를 쓰되 `step-results`를 만들지 않는 test를 추가한다.
- [x] 생성된 admin profile arguments에 `Restart-Computer`, `Stop-Computer`, `shutdown.exe`, `schtasks.exe`, `Register-ScheduledTask`가 없음을 확인하는 test를 추가한다.
- [x] 필수 profile option 누락 시 `PCV_BATCH_PROFILE_OPTION_REQUIRED`를 throw하는 test를 추가한다.

실행:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

예상 결과: 구현 전에는 실패한다.

## Task 2: GREEN - Batch Supervisor Profile Expansion

**파일:**

- Modify: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`

- [x] `Get-PcvBatchProfileOptionValue` helper를 추가한다.
- [x] `PCV_BATCH_PROFILE_OPTION_REQUIRED`를 throw하는 `Require-PcvBatchProfileOption` helper를 추가한다.
- [x] `New-PcvBatchSupervisorManifest`에 `-ProfileOptions`를 추가한다.
- [x] `ProfileOptions`를 `New-PcvBatchSupervisorProfileSteps`로 전달한다.
- [x] `ServiceMsiHyperVAdminSmoke`, `OsMutationGate`, `FullAdminHostMutationGate` case를 추가한다.
- [x] Unknown profile error text가 5개 profile을 모두 표시하도록 갱신한다.
- [x] Schema version은 `1`로 유지한다. Persisted manifest는 이미 expanded step을 포함하므로 consumer가 `ProfileOptions`를 이해할 필요가 없다.

실행:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

예상 결과: 통과한다.

## Task 3: RED - OS Gate Runner Plan-Only Test

**파일:**

- Create: `packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1`

- [x] `Invoke-PcvOsMutationGateSmoke.ps1 -PlanOnly`가 `summary.json`을 생성하는 test를 추가한다.
- [x] Plan-only summary가 정확한 step 이름 `preflight`, `config-migration-apply-service-running`, `eventlog-register`, `eventlog-remove`, `firewall-enable`, `lan-listener-ip-smoke`, `firewall-remove`, `export-existing-internal-trust-certs`, `trust-store-install-existing`, `trust-store-remove-existing`, `trust-store-restore-existing`를 포함하는 test를 추가한다.
- [x] Plan-only summary가 `routeparity_artifact`, `version`, `lan_prefix`, `public_trusted_signing=excluded`, `external_stable_publication=not-claimed`를 포함하는 test를 추가한다.
- [x] Plan-only command plan이 `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js`에 대한 bearer-required LAN probe를 포함하는 test를 추가한다.
- [x] Plan-only summary가 host mutation 미수행을 명시하는 test를 추가한다.
- [x] Script text에 `Restart-Computer`, `Stop-Computer`, `shutdown.exe`, `schtasks.exe`가 없음을 확인하는 test를 추가한다.

실행:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1' -Output Detailed"
```

예상 결과: script가 아직 없으므로 실패한다.

## Task 4: GREEN - OS Gate Runner

**파일:**

- Create: `packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1`

- [x] `-PlanOnly`를 먼저 구현하고 Task 3을 통과시킨다.
- [x] Async stdout/stderr drain을 사용하는 shared JSON writer와 captured process helper를 구현한다.
- [x] 기존 DPAPI LocalMachine schema를 사용해 protected token file read를 구현한다.
- [x] Artifact별 `process`, `parsed`, `ok` field를 기록하는 service-action invocation helper를 구현한다.
- [x] Missing installed host, missing protected token file, non-running service, route parity summary `ok=true` 불일치에서 fail fast하는 preflight를 구현한다.
- [x] `System.Diagnostics.ProcessStartInfo`, `UseShellExecute=false`, `CreateNoWindow=true`를 사용하는 LAN temporary listener helper를 구현한다.
- [x] LAN probe가 static web asset에도 bearer Authorization을 포함하게 한다.
- [x] Temporary LAN listener는 `finally`에서 중지한다.
- [x] `firewall-enable` 이후 `finally`에서 firewall cleanup을 시도한다.
- [x] Trust-store removal 이후 `finally`에서 trust-store restore를 시도한다.
- [x] 각 step 이후 `progress.json`을 쓰고 final `summary.json`을 쓴다.
- [x] `Restart-Computer`, `Stop-Computer`, `shutdown.exe`, `schtasks.exe`, Task Scheduler API를 호출하지 않는다.

실행:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1' -Output Detailed"
```

예상 결과: 통과한다. 이 command는 `-PlanOnly`만 사용해야 하며 host를 mutation하면 안 된다.

## Task 5: 문서 갱신

**파일:**

- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

- [x] Batch Supervisor / Hang Guard section에 새 admin profile을 문서화한다.
- [x] `ServiceMsiHyperVAdminSmoke` manifest generation command 예시를 추가한다.
- [x] `OsMutationGate` manifest generation command 예시를 추가한다.
- [x] Profile generation과 dry-run은 non-mutating이지만 실제 실행은 `-AllowHostMutation`과 elevated shell을 요구한다고 명시한다.
- [x] 구현 검증은 OS mutation evidence를 재실행하지 않는다고 명시한다.
- [x] 향후 승인된 OS gate rerun이 더 최신 evidence를 만들 때까지 latest OS gate wording은 `0.35.7-admin-smoke`로 유지한다.

## Task 6: 검증

실행:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvOsMutationGateSmoke.Tests.ps1' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

예상 결과:

- 위 Pester test가 모두 통과한다.
- `git diff --check` exits `0`; PowerShell line-ending warnings may be reported by git but no whitespace errors are accepted.
- No Hyper-V, service, MSI, firewall, LAN, Event Log, trust-store, Task Scheduler, or reboot action is executed.

## Task 7: 구현 후 Admin Run 경계

이 작업은 구현 중 실행하지 않는다. Code/docs merge 이후 사용자가 host-mutating admin run을 명시 승인하면 새 profile로 다음과 유사한 manifest를 생성하고 저장한다.

```powershell
Import-Module ./packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1 -Force
$repo = (Resolve-Path .).Path
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$manifest = New-PcvBatchSupervisorManifest `
  -BatchId "full-admin-host-mutation-gate-$stamp" `
  -RepoRoot $repo `
  -ArtifactRoot "artifacts/batch-runs/full-admin-host-mutation-gate-$stamp" `
  -Profile FullAdminHostMutationGate `
  -ProfileOptions @{
    version = '0.36.2-admin-smoke'
    iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
    routeparity_artifact_root = "artifacts/routeparity-service-msi-hyperv-batch-profile-$stamp-0362"
    os_gate_artifact_root = "artifacts/os-mutation-gates-batch-profile-$stamp-0362"
    lan_prefix = 'http://[redacted-private-endpoint]:7777/'
  }
Save-PcvBatchSupervisorManifest -Manifest $manifest -Path (Join-Path $manifest.artifact_root 'manifest.json')
```

실제 실행은 elevated shell과 명시적 host mutation 승인이 필요하다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/full-admin-host-mutation-gate-<stamp>/manifest.json -AllowHostMutation
```

## 위험 제어

- Host mutation은 두 단계로 guard한다. Profile step은 `requires_admin=true`/`mutates_host=true`를 갖고, CLI는 `-AllowHostMutation`을 요구한다.
- OS gate runner는 test와 manifest review용 `-PlanOnly`를 갖는다.
- Reboot 및 scheduled-task capable command는 계속 금지한다.
- LAN probe는 Phase 17 policy를 보존하기 위해 static asset에도 bearer Authorization을 사용한다.
- Trust-store restore는 runner finalization path에 포함한다.
- Firewall removal은 runner finalization path에 포함한다.
- Latest OS gate docs는 별도 승인 rerun이 성공할 때까지 `0.35.7-admin-smoke`로 고정한다.

## 완료 보고

구현이 완료되면 다음 내용을 보고한다.

```text
Batch Supervisor v2 OS Gate Profile 구현 완료.
추가 profile: ServiceMsiHyperVAdminSmoke, OsMutationGate, FullAdminHostMutationGate.
구현 검증은 non-mutating으로만 수행.
OS mutation gate rerun은 실행하지 않음.
별도 승인 rerun 전까지 최신 OS gate는 0.35.7-admin-smoke 유지.
```
