# MSI Repair and Batch Supervisor Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `0.37.0-admin-smoke`에서 관측한 first-attempt MSI repair `1603` recovered transient를 Batch Supervisor와 route parity smoke가 자동 감지, 증거화, 재시도할 수 있게 한다.

**Architecture:** Batch Supervisor는 product runtime이 아니라 repo-local verification runner이므로 step retry, attempt artifact, heartbeat, resume contract만 소유한다. MSI lifecycle의 domain-specific 판단은 `Invoke-PcvRouteParityMutationSmoke.ps1`에 두고, repair `1603` 같은 transient 후보는 partial lifecycle JSON과 classification으로 남긴다. 실제 Hyper-V, MSI, firewall, LAN, Event Log, trust-store mutation은 이 구현 batch에서 재실행하지 않고, Pester와 self-test/dry-run으로 닫는다.

**Tech Stack:** PowerShell 7, Pester 5, Batch Supervisor module, route parity admin smoke script, Markdown docs.

**구현 상태:** `main`에 구현 완료. Batch Supervisor retry attempt, admin profile retry 기본값, route parity MSI lifecycle partial evidence/classification, 문서 hardening은 packaging 테스트로 확인된다. 2026-05-07에는 실제 host mutation 없이 문서 상태 정리로 checkbox closure만 반영했다.

---

## Background

`0.37.0-admin-smoke` full admin host mutation gate evidence:

- First batch attempt failed in `service-msi-hyperv-admin-smoke` at MSI repair with `PCV_SMOKE_MSI_STEP_FAILED|repair exited 1603.`
- Direct `DesktopNode.Host.exe service-action repair-installed` returned exit `0`.
- Manual MSI repair returned exit `0`.
- Batch Supervisor `-Resume` completed Service/MSI/Hyper-V and OS mutation gate with final `ok=true`.

Current gaps:

- `retry_count` exists in `New-PcvBatchSupervisorStep`, but `Invoke-PcvBatchSupervisor` executes each step only once.
- `Invoke-PcvRouteParityMutationSmoke.ps1` writes `msi-lifecycle-smoke.json` only after the entire lifecycle passes, so a failure inside repair loses the per-step lifecycle JSON.
- MSI repair failures are not classified as transient/retryable versus hard failures.
- Full admin profile does not encode the known safe retry policy for the Service/MSI/Hyper-V step.

## Non-Goals

- Do not change `DesktopNode.Host.exe` product runtime behavior.
- Do not change MSI custom actions.
- Do not run actual Hyper-V VM creation, MSI install/repair/uninstall, firewall, LAN, Event Log, trust-store mutation in this implementation batch.
- Do not add automatic reboot, Task Scheduler, background services, or persistent OS monitors.
- Do not classify all MSI `1603` failures as retryable. Retryable classification is narrow and evidence-based.

## File Structure

- Modify: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`
  - Add step retry execution, attempt result artifacts, retry heartbeat, final aggregate step result.
  - Add admin profile retry defaults/options.
- Modify: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`
  - Add retry behavior tests and admin profile retry contract tests.
- Modify: `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`
  - Add MSI failure classification.
  - Persist partial `msi-lifecycle-smoke.json` after every MSI step and before throwing.
  - Extend `-SelfTest` to cover classifier behavior without host mutation.
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
  - Add static/self-test contract checks for route parity MSI lifecycle persistence/classification.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Document retry policy and host mutation boundary.
- Modify: `packaging/windows-desktop-node/README.md`
  - Document Batch Supervisor retry artifacts and `0.37.0` transient handling.
- Modify: `packaging/windows-desktop-node/installer/README.md`
  - Document MSI repair transient classification and what evidence is required.

## Task 1: Batch Supervisor Retry Contract

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`

- [x] **Step 1: Add failing retry tests**

Append these tests before the final `}` in `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`:

```powershell
    It 'retries a failed step when retry_count is configured and preserves each attempt' {
        $artifactRoot = Join-Path $TestDrive 'batch-retry'
        $marker = Join-Path $TestDrive 'attempt-count.txt'
        $script = @"
`$path = '$marker'
`$count = 0
if (Test-Path -LiteralPath `$path) {
    `$count = [int](Get-Content -LiteralPath `$path -Raw)
}
`$count++
Set-Content -LiteralPath `$path -Value `$count -NoNewline
if (`$count -lt 2) {
    Write-Error 'transient failure'
    exit 42
}
Write-Output 'retry success'
exit 0
"@
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'retry' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -HeartbeatIntervalSeconds 1 `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'flaky-step' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', $script) `
                    -TimeoutSeconds 30 `
                    -RetryCount 1)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest

        $result.ok | Should -BeTrue
        $result.executed_steps | Should -Be 1
        $result.results[0].ok | Should -BeTrue
        $result.results[0].attempt_count | Should -Be 2
        $result.results[0].retry_count | Should -Be 1
        $result.results[0].attempts[0].exit_code | Should -Be 42
        $result.results[0].attempts[0].ok | Should -BeFalse
        $result.results[0].attempts[1].exit_code | Should -Be 0
        $result.results[0].attempts[1].ok | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-flaky-step.attempt-01.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-flaky-step.attempt-02.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results/001-flaky-step.json') | Should -BeTrue
        Get-Content -LiteralPath (Join-Path $artifactRoot 'heartbeat.jsonl') | Should -Match '"status":"retrying"'
    }

    It 'fails after retry_count is exhausted and points resume at the failed step' {
        $artifactRoot = Join-Path $TestDrive 'batch-retry-exhausted'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'retry-exhausted' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -HeartbeatIntervalSeconds 1 `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'always-fails' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Error "still failing"; exit 9') `
                    -TimeoutSeconds 30 `
                    -RetryCount 2)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest

        $result.ok | Should -BeFalse
        $result.failed_step_id | Should -Be 'always-fails'
        $result.next_resume_step_id | Should -Be 'always-fails'
        $result.results[0].attempt_count | Should -Be 3
        $result.results[0].attempts[-1].exit_code | Should -Be 9
    }

    It 'sets retry_count defaults for host-mutating admin profile steps' {
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'full-admin-retry-defaults' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot (Join-Path $TestDrive 'full-admin-retry-defaults') `
            -Profile FullAdminHostMutationGate `
            -ProfileOptions @{
                version = '0.37.1-admin-smoke'
                iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'
                routeparity_artifact_root = (Join-Path $TestDrive 'routeparity-artifact')
                os_gate_artifact_root = (Join-Path $TestDrive 'os-gate-artifact')
                lan_prefix = 'http://[redacted-private-endpoint]:7777/'
            }

        $routeStep = @($manifest.steps | Where-Object { $_.id -eq 'service-msi-hyperv-admin-smoke' })[0]
        $osStep = @($manifest.steps | Where-Object { $_.id -eq 'os-mutation-gate' })[0]

        $routeStep.retry_count | Should -Be 1
        $osStep.retry_count | Should -Be 0
    }
```

- [x] **Step 2: Run retry tests and confirm red**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: fail because `retry_count` is not yet executed, attempt artifacts do not exist, and admin profile retry defaults are still `0`.

- [x] **Step 3: Implement retry wrapper**

In `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`, change `Invoke-PcvBatchStepProcess` to accept an attempt number and write attempt-specific result paths.

Add parameters:

```powershell
        [int]$Attempt = 1,
        [bool]$AttemptResult = $false
```

Replace the result path calculation with:

```powershell
    $baseResultName = '{0:D3}-{1}' -f $Ordinal, $safeId
    $resultFileName = if ($AttemptResult) {
        '{0}.attempt-{1:D2}.json' -f $baseResultName, $Attempt
    } else {
        '{0}.json' -f $baseResultName
    }
    $resultPath = Join-Path $stepResultsRoot $resultFileName
```

Add attempt fields to `current-step.json` and result objects:

```powershell
        attempt = $Attempt
```

and:

```powershell
        attempt = $Attempt
        result_path = (ConvertTo-PcvBatchRedactedText -Text $resultPath -PathRedactions $pathRedactions)
```

Then add this new helper below `Invoke-PcvBatchStepProcess`:

```powershell
function Invoke-PcvBatchStepWithRetries {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)]$Step,
        [Parameter(Mandatory)][int]$Ordinal
    )

    $artifactRoot = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'artifact_root')
    $pathRedactions = Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'path_redactions'
    $batchId = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'batch_id')
    $stepId = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'id')
    $safeId = $stepId -replace '[^A-Za-z0-9._-]', '-'
    $stepResultsRoot = Join-Path $artifactRoot 'step-results'
    $heartbeatPath = Join-Path $artifactRoot 'heartbeat.jsonl'
    $retryCount = [Math]::Max(0, [int](Get-PcvBatchPropertyValue -InputObject $Step -Name 'retry_count' -Default 0))
    $maxAttempts = 1 + $retryCount
    $attempts = New-Object System.Collections.Generic.List[object]
    $lastResult = $null

    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        $lastResult = Invoke-PcvBatchStepProcess -Manifest $Manifest -Step $Step -Ordinal $Ordinal -Attempt $attempt -AttemptResult:($maxAttempts -gt 1)
        $attempts.Add($lastResult) | Out-Null
        if ([bool]$lastResult.ok) {
            break
        }
        if ($attempt -lt $maxAttempts) {
            Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $batchId -StepId $stepId -Status 'retrying' -Detail ("attempt={0} exit_code={1} next_attempt={2}" -f $attempt, $lastResult.exit_code, ($attempt + 1))
        }
    }

    if ($maxAttempts -eq 1) {
        return $lastResult
    }

    $finalResultPath = Join-Path $stepResultsRoot ('{0:D3}-{1}.json' -f $Ordinal, $safeId)
    $aggregate = [pscustomobject]([ordered]@{
        schema_version = 1
        step_id = $stepId
        ordinal = $Ordinal
        command_fingerprint = $lastResult.command_fingerprint
        file_name = $lastResult.file_name
        arguments = $lastResult.arguments
        working_directory = $lastResult.working_directory
        started_at = $attempts[0].started_at
        finished_at = $lastResult.finished_at
        duration_ms = [int](@($attempts.ToArray()) | Measure-Object -Property duration_ms -Sum).Sum
        timeout_seconds = $lastResult.timeout_seconds
        timed_out = [bool]$lastResult.timed_out
        exit_code = $lastResult.exit_code
        ok = [bool]$lastResult.ok
        stdout = $lastResult.stdout
        stderr = $lastResult.stderr
        start_failure = $lastResult.start_failure
        retry_count = $retryCount
        attempt_count = @($attempts.ToArray()).Count
        attempts = @($attempts.ToArray())
    })
    Write-PcvBatchJsonFile -Path $finalResultPath -Value $aggregate
    return $aggregate
}
```

In `Invoke-PcvBatchSupervisor`, replace:

```powershell
        $result = Invoke-PcvBatchStepProcess -Manifest $Manifest -Step $step -Ordinal $ordinal
```

with:

```powershell
        $result = Invoke-PcvBatchStepWithRetries -Manifest $Manifest -Step $step -Ordinal $ordinal
```

- [x] **Step 4: Set admin profile retry defaults**

In `New-PcvBatchSupervisorProfileSteps`, set:

- `ServiceMsiHyperVAdminSmoke` step `RetryCount` default to `1`.
- `FullAdminHostMutationGate` route parity step `RetryCount` default to `1`.
- `OsMutationGate` step default remains `0`.
- Optional profile options:
  - `service_msi_hyperv_retry_count`
  - `os_gate_retry_count`

Use this retrieval pattern:

```powershell
$serviceRetryCount = [int](Get-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'service_msi_hyperv_retry_count' -Default 1)
$osRetryCount = [int](Get-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'os_gate_retry_count' -Default 0)
```

Pass `-RetryCount $serviceRetryCount` to the route parity step and `-RetryCount $osRetryCount` to the OS gate step.

- [x] **Step 5: Run retry tests and confirm green**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: PASS.

## Task 2: MSI Lifecycle Partial Evidence and Classification

**Files:**
- Modify: `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`

- [x] **Step 1: Add failing static/self-test contract checks**

Append this test to `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1` in the existing route parity smoke script section:

```powershell
    It 'records MSI lifecycle partial evidence and repair transient classification contract' {
        $smoke = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1')

        $smoke | Should -Match 'Get-MsiStepFailureClassification'
        $smoke | Should -Match 'msi-repair-retryable-transient'
        $smoke | Should -Match 'failed_step'
        $smoke | Should -Match 'Write-JsonFile -Path \$lifecyclePath -Value'
        $smoke | Should -Match 'msi-classifier-self-test'
    }
```

- [x] **Step 2: Run focused test and confirm red**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected: fail because the classifier and partial evidence fields do not exist yet.

- [x] **Step 3: Add MSI failure classifier**

Add this function after `Invoke-MsiStep` in `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`:

```powershell
function Get-MsiStepFailureClassification {
    param(
        [Parameter(Mandatory)] [string]$Name,
        [Parameter(Mandatory)] [int]$ExitCode,
        [string]$LogPath
    )

    $logText = ''
    if (-not [string]::IsNullOrWhiteSpace($LogPath) -and (Test-Path -LiteralPath $LogPath -PathType Leaf)) {
        $logText = Get-Content -LiteralPath $LogPath -Raw -ErrorAction SilentlyContinue
    }

    $repairRestartManagerAbort = [bool](
        $Name -eq 'repair' -and
        $ExitCode -eq 1603 -and
        ($logText -match 'RepairInstalled returned actual error code -1073741510' -or
         $logText -match 'MsiSystemRebootPending = 1' -or
         $logText -match 'Restart Manager')
    )

    if ($repairRestartManagerAbort) {
        return [pscustomobject][ordered]@{
            code = 'msi-repair-retryable-transient'
            retryable = $true
            recommendation = 'rerun-batch-step'
            reason = 'repair custom action was interrupted or Restart Manager/reboot-pending evidence was present'
        }
    }

    [pscustomobject][ordered]@{
        code = 'msi-hard-failure'
        retryable = $false
        recommendation = 'inspect-msi-log'
        reason = 'failure did not match the narrow repair transient classifier'
    }
}
```

- [x] **Step 4: Persist partial lifecycle JSON before throwing**

Initialize lifecycle fields at creation:

```powershell
        failed_step = $null
        error = $null
        failure_classification = $null
```

After appending each `$result` to `$lifecycle.steps`, immediately write partial lifecycle state:

```powershell
        Write-JsonFile -Path $lifecyclePath -Value ([pscustomobject]$lifecycle)
```

Replace the failure branch with:

```powershell
        if (-not $result.ok -or $result.actual_reboot_initiated) {
            $classification = Get-MsiStepFailureClassification -Name $step.name -ExitCode ([int]$result.exit_code) -LogPath (Join-Path $lifecycleLogRoot $step.log)
            $lifecycle.failed_step = $step.name
            $lifecycle.error = "PCV_SMOKE_MSI_STEP_FAILED|$($step.name) exited $($result.exit_code)."
            $lifecycle.failure_classification = $classification
            $lifecycle.boot_time_after = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
            $lifecycle.boot_time_unchanged = $lifecycle.boot_time_before -eq $lifecycle.boot_time_after
            Write-JsonFile -Path $lifecyclePath -Value ([pscustomobject]$lifecycle)
            throw $lifecycle.error
        }
```

Keep the final successful write at the end.

- [x] **Step 5: Extend `-SelfTest` with classifier evidence**

Inside the `if ($SelfTest)` block, add after protected token self-test:

```powershell
    Start-Step -Name 'msi-classifier-self-test'
    $msiClassifierSelfTestPath = Join-Path $ArtifactRoot 'msi-classifier-self-test.json'
    $sampleLogPath = Join-Path $ArtifactRoot 'repair-transient-sample.log'
    Set-Content -LiteralPath $sampleLogPath -Value 'MsiSystemRebootPending = 1; CustomAction RepairInstalled returned actual error code -1073741510' -NoNewline
    $retryable = Get-MsiStepFailureClassification -Name 'repair' -ExitCode 1603 -LogPath $sampleLogPath
    $hard = Get-MsiStepFailureClassification -Name 'install' -ExitCode 1603 -LogPath $sampleLogPath
    $msiClassifierOk = [bool]($retryable.retryable -and -not $hard.retryable -and $retryable.code -eq 'msi-repair-retryable-transient')
    Write-JsonFile -Path $msiClassifierSelfTestPath -Value ([pscustomobject][ordered]@{
        ok = $msiClassifierOk
        retryable = $retryable
        hard = $hard
    })
    Add-Step -Name 'msi-classifier-self-test' -Ok $msiClassifierOk -Path $msiClassifierSelfTestPath -Status $(if ($msiClassifierOk) { 'completed' } else { 'failed' })
```

Update self-test final `$ok`:

```powershell
    $ok = [bool]($captureOk -and $protectedTokenSelfTestOk -and $msiClassifierOk)
```

- [x] **Step 6: Run route parity self-test**

Run:

```powershell
$artifactRoot = Join-Path $env:TEMP ('pcv-routeparity-selftest-' + [guid]::NewGuid().ToString('N'))
pwsh -NoProfile -ExecutionPolicy Bypass -File 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1' -SelfTest -ArtifactRoot $artifactRoot
```

Expected: exit `0`; artifact root path printed; `msi-classifier-self-test.json` has `ok=true`.

- [x] **Step 7: Run focused static/self-test contract**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected: PASS.

## Task 3: Documentation for Retry Policy

**Files:**
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`

- [x] **Step 1: Update verification policy**

In `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, update the Batch Supervisor section with:

```markdown
- Step `retry_count`는 explicit retry budget이다. Default는 `0`이며, admin `ServiceMsiHyperVAdminSmoke` route parity step만 known recovered MSI repair transient 대응으로 기본 `1`을 사용한다. Retry는 같은 command fingerprint와 같은 host mutation approval 안에서만 수행되고, 각 attempt는 `step-results/<ordinal>-<step>.attempt-NN.json`와 final aggregate `step-results/<ordinal>-<step>.json`에 남아야 한다.
- MSI repair `1603`은 자동 성공으로 취급하지 않는다. `Invoke-PcvRouteParityMutationSmoke.ps1`는 repair log evidence가 narrow transient classifier와 일치할 때만 `msi-repair-retryable-transient` classification을 남기고, Batch Supervisor retry 또는 명시적 `-Resume` rerun으로만 회복할 수 있다.
```

- [x] **Step 2: Update packaging README**

In `packaging/windows-desktop-node/README.md`, update the Batch Supervisor section with:

```markdown
Retry policy:

- Non-mutating profiles default to `retry_count=0`.
- `ServiceMsiHyperVAdminSmoke` and the Service/MSI/Hyper-V step inside `FullAdminHostMutationGate` default to `retry_count=1` because `0.37.0-admin-smoke` recorded a recovered MSI repair transient.
- Retry attempts are evidence, not hidden control flow. Attempt files are kept beside the final aggregate step result.
- `OsMutationGate` remains `retry_count=0` unless a manifest explicitly overrides `os_gate_retry_count`.
```

- [x] **Step 3: Update installer README**

In `packaging/windows-desktop-node/installer/README.md`, add:

```markdown
MSI repair transient handling:

- Repair exit `1603` remains a failed MSI step.
- If the repair log includes the narrow recovered transient markers from `0.37.0-admin-smoke` such as `RepairInstalled returned actual error code -1073741510`, `MsiSystemRebootPending = 1`, or Restart Manager interruption evidence, route parity smoke records `msi-repair-retryable-transient`.
- The runner may retry the whole Service/MSI/Hyper-V step only when Batch Supervisor `retry_count` allows it. The MSI script itself does not hide the failure or mark repair `1603` as success.
```

## Task 4: Verification, Commit, Push

**Files:**
- Verify all changed files.

- [x] **Step 1: Run focused tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [x] **Step 2: Run full non-mutating packaging tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: PASS. This must remain non-mutating.

- [x] **Step 3: Run route parity self-test**

Run:

```powershell
$artifactRoot = Join-Path $env:TEMP ('pcv-routeparity-selftest-' + [guid]::NewGuid().ToString('N'))
pwsh -NoProfile -ExecutionPolicy Bypass -File 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1' -SelfTest -ArtifactRoot $artifactRoot
```

Expected: exit `0`; `msi-classifier-self-test.json` exists and has `ok=true`.

- [x] **Step 4: Run admin profile dry-run**

Generate and dry-run a full admin profile manifest without host mutation:

```powershell
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$batchRoot = "artifacts/batch-runs/full-admin-retry-hardening-dryrun-$stamp"
$routeRoot = "artifacts/routeparity-retry-hardening-dryrun-$stamp"
$osRoot = "artifacts/os-mutation-retry-hardening-dryrun-$stamp"
pwsh -NoProfile -Command "Import-Module 'packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1' -Force; `$m = New-PcvBatchSupervisorManifest -BatchId 'full-admin-retry-hardening-dryrun-$stamp' -RepoRoot (Get-Location).Path -ArtifactRoot '$batchRoot' -Profile FullAdminHostMutationGate -ProfileOptions @{ version = '0.37.1-admin-smoke'; iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'; routeparity_artifact_root = '$routeRoot'; os_gate_artifact_root = '$osRoot'; lan_prefix = 'http://[redacted-private-endpoint]:7777/' }; Save-PcvBatchSupervisorManifest -Manifest `$m -Path (Join-Path '$batchRoot' 'manifest.json') | Out-Null"
pwsh -NoProfile -ExecutionPolicy Bypass -File 'packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1' -ManifestPath (Join-Path $batchRoot 'manifest.json') -DryRun -AllowHostMutation
```

Expected: dry-run summary `ok=true`; manifest has route parity step `retry_count=1`, OS gate step `retry_count=0`; no `step-results` directory.

- [x] **Step 5: Whitespace check**

Run:

```powershell
git diff --check
```

Expected: exit `0`.

- [x] **Step 6: Commit**

Run:

```powershell
git add packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1 packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1 packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1 docs/DEVELOPMENT_VERIFICATION_POLICY.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/installer/README.md docs/superpowers/plans/2026-05-05-purecvisor-desktop-node-msi-repair-batch-supervisor-hardening.md
git commit -m "Harden MSI repair retry evidence"
```

Expected: commit created.

- [x] **Step 7: Push**

Run:

```powershell
git push
```

Expected: push succeeds.

## Optional Post-Implementation Admin Smoke

This is not part of the implementation batch. Run only after separate user approval:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/<new-full-admin-gate>/manifest.json -AllowHostMutation
```

Expected: if a transient MSI repair failure recurs, Batch Supervisor records attempt 1 failure, emits `retrying`, reruns the Service/MSI/Hyper-V step once, and writes final aggregate evidence.

## Rollback

```powershell
git revert --no-edit HEAD
```

No artifact deletion is part of rollback. Retry/self-test artifacts are evidence records and should be retained unless the user explicitly requests cleanup.

## Completion Criteria

- Batch Supervisor honors `retry_count`.
- Step retry attempts are written as attempt JSON files plus final aggregate JSON.
- `FullAdminHostMutationGate` route parity step defaults to `retry_count=1`; OS gate remains `0`.
- MSI lifecycle writes partial JSON before failure.
- MSI repair transient classification is narrow and visible in evidence.
- Pester packaging suite passes.
- Route parity self-test passes.
- Full admin profile dry-run passes without host mutation.
- Changes are committed and pushed.

## Self-Review

- Spec coverage: addresses MSI repair transient, Batch Supervisor retry, attempt artifacts, hang/retry observability, docs, verification, commit, and push.
- Placeholder scan: no TBD/TODO/fill-in steps remain.
- Type consistency: uses existing `retry_count`, `summary.json`, `current-step.json`, `heartbeat.jsonl`, and `step-results/*.json` contracts.
