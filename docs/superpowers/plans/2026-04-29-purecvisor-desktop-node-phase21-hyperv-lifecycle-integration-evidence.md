# PureCVisor Desktop Node Phase 21 Hyper-V Lifecycle Integration Evidence Plan

> **For agentic workers:** 이 plan은 실제 Hyper-V host lifecycle evidence를 수집하기 위한 runbook이다. 실제 Hyper-V VM command, elevated product service mutation, `msiexec` 실행은 사용자 opt-in 이후에만 실행한다. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Phase 19 이후 남은 GA 차단 gate 중 Hyper-V create/start/poweroff/checkpoint/remove lifecycle과 failure/retry/job-store consistency evidence를 수집한다.

**Architecture:** 기본 검증은 non-integration Pester와 root documentation suite로 제한한다. 실제 evidence는 signed/elevated product install flow가 준비된 Windows Hyper-V host에서 Local API job route와 explicit cleanup checklist를 통해 수집한다.

**Tech Stack:** PowerShell 7, Pester 5, Hyper-V PowerShell cmdlet, Windows service, WinSW, protected token file, Local API job store, JSONL diagnostics.

---

## 상태

- 작성 기준: 2026-04-29
- 현재 상태: product-flow lifecycle evidence 완료
- 제품 승격 판단: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- Phase 21 gate 상태: local product-flow smoke 완료, GA 판단은 public signing/stable/운영 evidence와 함께 재판정
- 관련 설계: `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase21-hyperv-lifecycle-integration-evidence-design.md`

## 실행 원칙

- 이 plan 작성과 기본 검증은 관리자 권한을 요구하지 않는다.
- 실제 Hyper-V lifecycle smoke는 지원 host와 administrator opt-in이 있을 때만 실행한다.
- 테스트 VM 기본 prefix는 `pcv-phase21-`이다.
- prefix와 ownership marker/path가 확인되지 않은 VM은 삭제하지 않는다.
- signed/elevated product install flow와 연결하되, Phase 20 signed MSI/elevated lifecycle gate를 지금 실행하라고 요구하지 않는다.
- evidence에는 raw token, protected token blob, signing secret, guest secret을 남기지 않는다.

## Task 1: Non-Integration Preflight

**Files:**
- Read: `follower.md`
- Read: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Read: `spikes/purecvisor-desktop-node/hyperv/README.md`
- Verify: `spikes/purecvisor-desktop-node/hyperv/tests`
- Verify: `spikes/purecvisor-desktop-node/tests`

- [ ] **Step 1: Confirm workspace scope**

Run:

```powershell
git status --short --branch
```

Expected: current documentation edits are limited to this Phase 21 spec and plan unless another worker has unrelated edits.

- [ ] **Step 2: Run Hyper-V non-integration suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

Expected: Hyper-V helper contract, host status, inventory, lifecycle/checkpoint, provisioning cleanup tests pass. Record actual pass counts only after running.

- [ ] **Step 3: Run root documentation suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: root boundary and documentation sync tests pass. Record actual pass counts only after running.

- [ ] **Step 4: Run markdown whitespace check**

Run:

```powershell
git diff --check
```

Expected: exit code 0.

## Task 2: Host Capability Preflight

**Admin gate:** Do not run this task without explicit administrator opt-in on a supported Windows Hyper-V host.

**Inputs to record before execution:**
- evidence directory
- local Linux ISO path
- product install source or installed product version
- test VM name prefix, default `pcv-phase21-`
- VM root directory dedicated to this smoke

- [ ] **Step 1: Confirm elevated shell**

Run in elevated PowerShell only:

```powershell
([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
  [Security.Principal.WindowsBuiltInRole]::Administrator)
```

Expected: `True`.

- [ ] **Step 2: Confirm Hyper-V host readiness through helper**

Run:

```powershell
@{ operation = 'host.status'; params = @{} } | ConvertTo-Json -Depth 8 |
  pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/hyperv/Invoke-PcvHyperV.ps1
```

Expected:

- `ok = true`
- Hyper-V feature is enabled or cmdlets are available
- VMMS is running
- host is supported

- [ ] **Step 3: Confirm ISO and VM root**

Run:

```powershell
Test-Path -LiteralPath '<local-linux.iso>' -PathType Leaf
Test-Path -LiteralPath '<phase21-vm-root>' -PathType Container
```

Expected: ISO exists. VM root exists or is created explicitly as a dedicated Phase 21 directory.

## Task 3: Signed/Elevated Product Flow Preflight

**Admin gate:** Do not run this task without explicit opt-in. If Phase 20 signed/elevated evidence is still pending, leave this task pending.

- [ ] **Step 1: Confirm product service status**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

Expected: installed product service is present and running.

- [ ] **Step 2: Read protected token without recording the value**

Run:

```powershell
$serviceModule = Resolve-Path 'spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1'
Import-Module $serviceModule -Force
$tokenInfo = Read-PcvDesktopServiceProtectedTokenFile -Path "$env:ProgramData\PureCVisor\desktop-node\api-token.dpapi.json"
$headers = @{ Authorization = "Bearer $($tokenInfo.token)" }
'TOKEN_LOADED_WITHOUT_PRINTING_VALUE'
```

Expected: token loads, but the token value is not printed or copied into evidence.

- [ ] **Step 3: Confirm runtime policy**

Run:

```powershell
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/runtime/policy' -Headers $headers
```

Expected:

- HTTP request succeeds
- `token_storage` is `dpapi-local-machine`
- persistence/job policy matches current Local API runtime policy

- [ ] **Step 4: Confirm Hyper-V status through product API**

Run:

```powershell
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/host/status' -Headers $headers
```

Expected: Product API reports supported Hyper-V host state.

## Task 4: Lifecycle Smoke Evidence

**Admin gate:** Do not run this task without explicit opt-in. This task creates and removes a real test VM.

**Safety variables:**

```powershell
$testVmName = "pcv-phase21-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
$checkpointName = 'pcv-phase21-before-poweroff'
$vmRoot = '<phase21-vm-root>'
$isoPath = '<local-linux.iso>'
```

- [ ] **Step 1: Assert test VM name is safe and unused**

Run:

```powershell
if ($testVmName -notlike 'pcv-phase21-*') { throw "Unsafe test VM name: $testVmName" }
if (Get-VM -Name $testVmName -ErrorAction SilentlyContinue) { throw "Test VM already exists: $testVmName" }
```

Expected: no existing VM has the generated test name.

- [ ] **Step 2: Submit VM create job through Product API**

Run:

```powershell
$createBody = @{
  name = $testVmName
  iso_path = $isoPath
  cpu = 1
  memory_mb = 1024
  disk_gb = 8
  vm_root = $vmRoot
  generation = 2
} | ConvertTo-Json -Depth 8

$createJob = Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7777/api/v1/vms' -Headers $headers -ContentType 'application/json' -Body $createBody
$createJob.data.job_id
```

Expected: response contains a queued job id.

- [ ] **Step 3: Poll create job**

Run:

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($createJob.data.job_id)" -Headers $headers
```

Expected: final state is `succeeded`; record job id and final status.

- [ ] **Step 4: Confirm VM inventory**

Run:

```powershell
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/vms/$testVmName" -Headers $headers
```

Expected: VM detail exists and reports `platform = hyperv`.

- [ ] **Step 5: Submit start job**

Run:

```powershell
$startJob = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:7777/api/v1/vms/$testVmName/start" -Headers $headers
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($startJob.data.job_id)" -Headers $headers
```

Expected: final state is `succeeded`.

- [ ] **Step 6: Submit checkpoint create job**

Run:

```powershell
$checkpointBody = @{ name = $checkpointName } | ConvertTo-Json
$checkpointJob = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:7777/api/v1/vms/$testVmName/checkpoints" -Headers $headers -ContentType 'application/json' -Body $checkpointBody
$checkpointJobResult = Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($checkpointJob.data.job_id)" -Headers $headers
$checkpointList = Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/vms/$testVmName/checkpoints" -Headers $headers
$directSnapshots = Get-VMSnapshot -VMName $testVmName | Select-Object Name, VMName, CreationTime
$checkpointJobResult | ConvertTo-Json -Depth 20
$checkpointList | ConvertTo-Json -Depth 20
$directSnapshots | ConvertTo-Json -Depth 10
$assessmentModule = (Resolve-Path 'spikes/purecvisor-desktop-node/hyperv/PcvHyperVEvidence.psm1').Path
Import-Module $assessmentModule -Force
Get-PcvPhase21CheckpointEvidenceAssessment -Evidence @{
    lifecycle = @{
        checkpoint_job_result = $checkpointJobResult
        checkpoint_list_response = $checkpointList
        direct_snapshots = $directSnapshots
    }
} -CheckpointName $checkpointName | ConvertTo-Json -Depth 10
```

Expected:

- checkpoint job succeeds.
- checkpoint job result body is preserved in redacted evidence.
- Product API checkpoint list raw response is preserved and contains `pcv-phase21-before-poweroff`.
- direct `Get-VMSnapshot` evidence is preserved and contains `pcv-phase21-before-poweroff`.
- checkpoint evidence assessment returns `ok = true`, `status = verified_visible`.
- If create returns `PCV_CHECKPOINT_NOT_VISIBLE`, keep the run as failed/retryable evidence instead of treating the checkpoint job as successful.

- [ ] **Step 7: Submit poweroff job**

Run:

```powershell
$poweroffJob = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:7777/api/v1/vms/$testVmName/poweroff" -Headers $headers
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($poweroffJob.data.job_id)" -Headers $headers
```

Expected: final state is `succeeded`.

- [ ] **Step 8: Cleanup test VM explicitly**

Run only after checking prefix and ownership/path:

```powershell
$vm = Get-VM -Name $testVmName -ErrorAction SilentlyContinue
if ($vm -and $testVmName -like 'pcv-phase21-*') {
  $ownedByMarker = [string]$vm.Notes -match 'managed-by=purecvisor-desktop-node'
  $ownedByPath = $false
  foreach ($propertyName in @('Path', 'ConfigurationLocation')) {
    if ($vm.PSObject.Properties.Name.Contains($propertyName) -and $null -ne $vm.$propertyName) {
      $pathValue = [string]$vm.$propertyName
      if ($pathValue.StartsWith($vmRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $ownedByPath = $true
      }
    }
  }
  if (-not ($ownedByMarker -or $ownedByPath)) {
    throw "Refusing to remove VM without ownership evidence: $testVmName"
  }
  Stop-VM -Name $testVmName -TurnOff -Force -ErrorAction SilentlyContinue
  Remove-VM -Name $testVmName -Force
}
```

Expected: test VM is removed only when prefix and ownership/path checks pass.

- [ ] **Step 9: Cleanup VM directory**

Run only if the resolved path is under the dedicated Phase 21 VM root:

```powershell
$testVmDir = Join-Path $vmRoot $testVmName
if ((Test-Path -LiteralPath $testVmDir) -and $testVmDir.StartsWith($vmRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
  Remove-Item -LiteralPath $testVmDir -Recurse -Force
}
```

Expected: only the dedicated test VM directory is removed.

## Task 5: Failure, Interruption, Retry Evidence

**Admin gate:** Use the least destructive failure case that proves job consistency. Do not kill services or reboot without separate opt-in.

Post-reboot verification 선택지:

Hyper-V lifecycle 또는 service interruption smoke 중 Windows reboot가 별도 opt-in으로 승인된 경우 `packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -DryRun`으로 먼저 `HyperVNonIntegration` 또는 `ProductStatus` state/task plan을 확인한다. Runner evidence는 reboot 이후 command 실행 결과를 `post-reboot-result.json`, `post-reboot-summary.md`, command별 stdout/stderr artifact로 남기는 계약을 제공한다. 실제 VM 생성/삭제 command와 Task Scheduler 등록은 기본 profile에 포함하지 않고 명시적 administrator opt-in command plan으로만 다룬다. 자동 reboot는 사용하지 않으며 `-Reboot`는 `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`로 거부한다.

- [ ] **Step 1: Submit intentionally invalid create job**

Run:

```powershell
$badCreateBody = @{
  name = "pcv-phase21-fail-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
  iso_path = 'Z:\does-not-exist\missing.iso'
  cpu = 1
  memory_mb = 1024
  disk_gb = 8
  vm_root = $vmRoot
  generation = 2
} | ConvertTo-Json -Depth 8

$failedJob = Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7777/api/v1/vms' -Headers $headers -ContentType 'application/json' -Body $badCreateBody
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($failedJob.data.job_id)" -Headers $headers
```

Expected: job reaches `failed` with a structured error code; no VM is created.

- [ ] **Step 2: Retry failed job**

Run:

```powershell
$retryJob = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:7777/api/v1/jobs/$($failedJob.data.job_id)/retry" -Headers $headers
$retryJob.data.retry_of
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($retryJob.data.job_id)" -Headers $headers
```

Expected:

- retry response has a new `job_id`
- `retry_of` equals the original failed job id
- original failed job remains failed
- retry attempt follows the configured retry limit

- [ ] **Step 3: Restart service and confirm job store consistency**

Run only if product service restart is explicitly allowed:

```powershell
Restart-Service -Name 'PureCVisorDesktopNode'
Start-Sleep -Seconds 5
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($failedJob.data.job_id)" -Headers $headers
Invoke-RestMethod -Uri "http://127.0.0.1:7777/api/v1/jobs/$($retryJob.data.job_id)" -Headers $headers
```

Expected:

- completed or failed jobs remain queryable
- no job is stuck in `running` after restart
- retry relationship remains visible

- [ ] **Step 4: Collect diagnostics**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
```

Expected: diagnostic bundle exists and does not contain raw token or protected token blob.

## Task 6: Evidence Documentation And Redaction

**Files:**
- Modify: this plan only when recording Phase 21 evidence

- [ ] **Step 1: Redact evidence**

Before recording evidence:

- remove raw bearer token values
- remove protected token blob values
- remove signing secret, PFX password, private key material
- remove guest OS password, SSH private key, cloud-init secret
- avoid full host-sensitive absolute paths unless needed to prove cleanup; prefer summarized path roots

- [ ] **Step 2: Record lifecycle evidence**

Record in `완료 증거`:

- host readiness result
- product install/source artifact summary
- VM prefix, VM root summary, ISO path summary
- lifecycle job ids and final states
- checkpoint name, job result body summary, Product API list raw response summary, direct `Get-VMSnapshot` summary
- cleanup checklist result

- [ ] **Step 3: Record failure/retry/job-store evidence**

Record in `완료 증거`:

- failed job id and sanitized error code
- retry job id and `retry_of`
- service restart or interruption behavior if executed
- persisted job store consistency result
- diagnostics redaction result

- [ ] **Step 4: Leave remaining gates explicit**

If evidence is not fully collected, keep the gate pending and list remaining gates without claiming pass counts.

## 기본 검증

Default documentation validation:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Actual Hyper-V integration validation, administrator opt-in only:

```powershell
$env:PCV_HYPERV_INTEGRATION='1'
$env:PCV_HYPERV_TEST_ISO='<local-linux.iso>'
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1' -Tag Integration -Output Detailed"
```

## 완료 증거

Hyper-V helper 계층의 게이트형 통합 스모크 검증은 실행했다. 다만 서명/관리자 권한 제품 API 수명주기 증거는 아직 실행하지 않았으므로 Phase 21 제품 흐름 게이트는 미완료로 유지한다.

Phase 21 시작 문서화 검증:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"`: 41 passed, 0 failed, 1 not run
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 13 passed, 0 failed
- `git diff --check`: exit 0

Phase 21 Hyper-V helper 계층 통합 스모크 검증:

- 실행 명령: `PCV_HYPERV_INTEGRATION=1`, local Rocky minimal ISO를 사용해 `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1`의 `Integration` tag를 실행
- 결과: 1 passed, 0 failed, 0 skipped, 0 not run, duration 17s, exit code 0
- 포함 작업: Hyper-V helper runner 경유 `host.status`, `vm.create`, `vm.list`, `vm.start`, `checkpoint.create`, `vm.poweroff`
- 경고: `가상 컴퓨터가 이미 지정된 상태입니다.` 경고 1건이 있었으나 test failure로 처리되지 않았다.
- 정리 확인: `pcv-spike-*`와 일치하는 VM 없음, safe temp cleanup summary 기준 `pcv-hyperv-spike` temp root는 존재하고 ItemCount 0

2026-04-30 후속 evidence-hardening:

- Product API checkpoint create 증거에서 `checkpoint_status = succeeded`인데 list assertion이 false였던 이전 smoke는 raw job/list/direct snapshot body가 부족해 원인 판정이 불충분했다. 기존 artifact `artifacts/host-mutation-20260430-131658/phase21-product-hyperv-evidence.json`에는 `checkpoint_job_result`, `checkpoint_list_response`, `direct_snapshots`가 없으므로 `Get-PcvPhase21CheckpointEvidenceAssessment` 기준 `status = inconclusive_missing_raw_evidence`, `root_cause = evidence_capture_incomplete`로 분류한다.
- 따라서 현재 확인 가능한 원인은 Hyper-V checkpoint 유실이나 Product API list 버그가 아니라 evidence capture incomplete다. 실제 생성/목록 불일치 여부는 다음 관리자 opt-in product-flow rerun에서 raw evidence 3종을 캡처해야 판정한다.
- Hyper-V helper `checkpoint.create`는 이제 `Checkpoint-VM` 이후 `Get-VMSnapshot` read-after-write 확인을 최대 3회 수행하고, 새 checkpoint가 보이지 않으면 `PCV_CHECKPOINT_NOT_VISIBLE` retryable failure로 반환한다.
- 다음 Phase 21 product-flow rerun은 checkpoint job result, Product API checkpoint list raw response, direct `Get-VMSnapshot` 결과를 모두 evidence로 남기고 checkpoint evidence assessment `verified_visible`을 기록해야 한다.

2026-04-30 product API Hyper-V lifecycle rerun:

- evidence root: `artifacts/phase21-product-flow-rerun-20260430-190840`
- installed product: signed RC MSI `0.23.8-rc.1`, protected token file source, loopback API `127.0.0.1:7777`
- VM: `pcv-phase21-27ac195a`
- checkpoint: `pcv-phase21-before-poweroff`
- lifecycle: Product API VM create/start/checkpoint/poweroff 실행 후 explicit cleanup 완료
- checkpoint evidence: checkpoint job result, Product API checkpoint list raw response, direct `Get-VMSnapshot` snapshots를 모두 preserved
- checkpoint assessment: `ok = true`, `status = verified_visible`, job result/list/direct snapshot 모두 checkpoint name 포함
- cleanup: VM removed true, VM root removed true, `Get-VM -Name 'pcv-phase21-*'` 결과 없음
- failure evidence: missing ISO create job은 `PCV_ISO_NOT_FOUND`, `retryable = false`로 failed; retry 요청은 current runtime policy `failed_error_retryable_only` 계약에 따라 `409/PCV_JOB_NOT_RETRYABLE` 반환
- automatic reboot: not used
- 판정: 이전 `inconclusive_missing_raw_evidence` 원인은 evidence capture incomplete였다. 새 product-flow rerun은 checkpoint 생성과 list/direct visibility를 verified로 닫았다.

2026-05-01 관리자 opt-in Hyper-V 보강 evidence:

- evidence root: `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911`
- scope: latest admin opt-in hardening evidence를 보강하는 direct Hyper-V/service/MSI/firewall/Event Log smoke
- lifecycle: host.status, VM create/list/start/checkpoint create/checkpoint list/poweroff/remove 성공
- checkpoint evidence: checkpoint list response와 direct snapshot evidence 모두 checkpoint 이름을 확인했고 direct snapshot count는 `2`였다.
- cleanup: test VM과 VM directory cleanup 완료
- reboot policy: `auto_reboot_disabled = true`, `reboot_observed = false`
- 판정: 이전 checkpoint list 누락 이슈는 이번 evidence에서 checkpoint list와 direct snapshot 양쪽 이름 확인으로 닫혔다. 이 결과는 제품 runtime replacement 또는 GA 승격을 의미하지 않으며, public trusted/stable signing 판단은 별도다.

아직 실행하지 않은 관리자 opt-in gate:

- 장기 service run/recovery smoke
- Event Log provider/source lifecycle evidence
- public trusted/stable signing과 stable release approval

남은 GA 차단 gate:

- Phase 20 public trusted/stable signing evidence 또는 release approval
- 장기 운영 로그 evidence 또는 Event Log provider/source lifecycle evidence
