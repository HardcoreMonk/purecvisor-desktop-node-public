# Batch Supervisor / Hang Guard v1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 장시간 Desktop Node 개발/검증 배치를 repo-local supervisor로 실행해 hang, timeout, 재개 지점, host mutation gate, evidence artifact를 표준화한다.

**Architecture:** `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`가 manifest schema, command execution, heartbeat, timeout, resume, redaction을 소유한다. `Invoke-PcvBatchSupervisor.ps1`는 CLI entrypoint이며 product runtime이 아니라 개발/검증 runner다. v1은 Task Scheduler, reboot, background service를 만들지 않고 현재 shell에서 child process를 감시한다.

**Tech Stack:** PowerShell 7, Pester 5, JSON artifact files, .NET `System.Diagnostics.Process`, existing Desktop Node packaging tool/test conventions.

**구현 상태:** `main`에 구현 완료. `PcvBatchSupervisor.psm1`, `Invoke-PcvBatchSupervisor.ps1`, profile builder, hang/timeout/resume/redaction 테스트, packaging 문서가 현재 존재한다. 2026-05-07에는 실제 host mutation 없이 문서 상태 정리로 checkbox closure만 반영했다.

---

## Scope

포함:

- Repo-local batch manifest schema v1.
- `current-step.json`, `heartbeat.jsonl`, `step-results/*.json`, `summary.json` artifact contract.
- Step timeout과 process tree kill.
- Step-level heartbeat while command is running.
- Resume: command fingerprint가 같은 successful step은 skip.
- Host mutation/admin gate: manifest step이 `requires_admin=true` 또는 `mutates_host=true`면 explicit CLI approval 없이는 실행 전 차단.
- Secret/path redaction for arguments, stdout, stderr, summary.
- Non-mutating profile smoke와 self-test.

제외:

- Product Local API runtime 변경.
- Web Dashboard UI/UX.
- Product config/job store apply write.
- Task Scheduler 등록, reboot, post-reboot continuation.
- Firewall/trust-store/Event Log/Hyper-V/MSI mutation smoke 자체 구현. v1은 그런 명령을 감싸는 supervisor만 제공한다.

## File Structure

- Create `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`
  - Manifest validation, command execution, heartbeat, resume, redaction, summary generation.
- Create `packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1`
  - CLI wrapper around the module.
- Create `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`
  - Pester contract tests for schema, gate, timeout, resume, redaction, artifacts.
- Modify `packaging/windows-desktop-node/README.md`
  - Document supervisor usage and examples.
- Modify `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Add batch supervisor verification rule.
- Optional after implementation evidence: update `AGENTS.md` only if this becomes a required default loop. Do not update high-level docs with pass counts.

## Manifest Contract

Example manifest:

```json
{
  "schema_version": 1,
  "batch_id": "packaging-regression",
  "created_by": "operator",
  "artifact_root": "artifacts/batch-runs/packaging-regression-20260505-000000",
  "heartbeat_interval_seconds": 5,
  "default_timeout_seconds": 1800,
  "steps": [
    {
      "id": "packaging-product-tests",
      "working_directory": ".",
      "file_name": "pwsh",
      "arguments": [
        "-NoProfile",
        "-Command",
        "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
      ],
      "timeout_seconds": 1800,
      "required": true,
      "allow_failure": false,
      "requires_admin": false,
      "mutates_host": false,
      "retry_count": 0
    }
  ]
}
```

Artifact files:

- `batch-manifest.resolved.json`: normalized manifest with absolute repo/artifact paths.
- `current-step.json`: overwritten on batch start, step start, step completion/failure, batch completion.
- `heartbeat.jsonl`: append-only line records.
- `step-results/001-packaging-product-tests.json`: command result with redacted arguments/stdout/stderr.
- `summary.json`: final batch status and next resume point.

## Task 1: RED - Batch Supervisor Contract Tests

**Files:**

- Create: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`

- [x] **Step 1: Write failing tests for manifest, dry-run, mutation gate, timeout, resume, and redaction**

Create `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`:

```powershell
Set-StrictMode -Version Latest

Describe 'PcvBatchSupervisor v1 contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1'
    }

    BeforeEach {
        Import-Module $script:ModulePath -Force
    }

    It 'builds a non-mutating packaging regression manifest' {
        $artifactRoot = Join-Path $TestDrive 'batch-packaging'

        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'packaging-regression' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Profile PackagingRegression

        $manifest.schema_version | Should -Be 1
        $manifest.batch_id | Should -Be 'packaging-regression'
        $manifest.steps.Count | Should -Be 3
        @($manifest.steps | Where-Object { $_.requires_admin -or $_.mutates_host }).Count | Should -Be 0
        ($manifest.steps | ConvertTo-Json -Depth 12) | Should -Not -Match 'Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule'
    }

    It 'writes dry-run artifacts without executing commands' {
        $artifactRoot = Join-Path $TestDrive 'batch-dry-run'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'dry-run' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'echo-secret' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Output should-not-run') `
                    -TimeoutSeconds 30)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest -DryRun

        $result.ok | Should -BeTrue
        $result.dry_run | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'batch-manifest.resolved.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results') | Should -BeFalse
    }

    It 'rejects host mutation steps without explicit allowance' {
        $artifactRoot = Join-Path $TestDrive 'batch-mutation-block'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'mutation-block' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'msi-install' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'msiexec.exe' `
                    -Arguments @('/i', 'fake.msi') `
                    -TimeoutSeconds 30 `
                    -RequiresAdmin $true `
                    -MutatesHost $true)
            )

        {
            Invoke-PcvBatchSupervisor -Manifest $manifest
        } | Should -Throw -ExpectedMessage '*PCV_BATCH_HOST_MUTATION_APPROVAL_REQUIRED*'

        Test-Path -LiteralPath (Join-Path $artifactRoot 'step-results') | Should -BeFalse
    }

    It 'rejects automatic reboot capable commands even when host mutation is allowed' {
        $artifactRoot = Join-Path $TestDrive 'batch-reboot-block'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'reboot-block' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'reboot' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Restart-Computer') `
                    -TimeoutSeconds 30 `
                    -RequiresAdmin $true `
                    -MutatesHost $true)
            )

        {
            Invoke-PcvBatchSupervisor -Manifest $manifest -AllowHostMutation
        } | Should -Throw -ExpectedMessage '*PCV_BATCH_REBOOT_COMMAND_FORBIDDEN*'
    }

    It 'times out a hanging process and records heartbeat plus failed summary' {
        $artifactRoot = Join-Path $TestDrive 'batch-timeout'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'timeout' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -HeartbeatIntervalSeconds 1 `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'sleep-too-long' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Start-Sleep -Seconds 10') `
                    -TimeoutSeconds 1)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest

        $result.ok | Should -BeFalse
        $result.status | Should -Be 'failed'
        $result.failed_step_id | Should -Be 'sleep-too-long'
        $stepResult = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'step-results/001-sleep-too-long.json') | ConvertFrom-Json
        $stepResult.timed_out | Should -BeTrue
        $stepResult.exit_code | Should -BeNullOrEmpty
        Get-Content -LiteralPath (Join-Path $artifactRoot 'heartbeat.jsonl') | Should -Match 'sleep-too-long'
    }

    It 'resumes by skipping successful matching command fingerprints' {
        $artifactRoot = Join-Path $TestDrive 'batch-resume'
        $marker = Join-Path $TestDrive 'marker.txt'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'resume' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'write-once' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', "Add-Content -LiteralPath '$marker' -Value once") `
                    -TimeoutSeconds 30),
                (New-PcvBatchSupervisorStep `
                    -Id 'write-twice' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', "Add-Content -LiteralPath '$marker' -Value twice") `
                    -TimeoutSeconds 30)
            )

        $first = Invoke-PcvBatchSupervisor -Manifest $manifest
        $second = Invoke-PcvBatchSupervisor -Manifest $manifest -Resume

        $first.ok | Should -BeTrue
        $second.ok | Should -BeTrue
        @(Get-Content -LiteralPath $marker | Where-Object { $_ -eq 'once' }).Count | Should -Be 1
        @(Get-Content -LiteralPath $marker | Where-Object { $_ -eq 'twice' }).Count | Should -Be 1
        $second.skipped_steps | Should -Contain 'write-once'
        $second.skipped_steps | Should -Contain 'write-twice'
    }

    It 'redacts tokens and known paths from arguments and captured output' {
        $artifactRoot = Join-Path $TestDrive 'batch-redaction'
        $manifest = New-PcvBatchSupervisorManifest `
            -BatchId 'redaction' `
            -RepoRoot $script:RepoRoot `
            -ArtifactRoot $artifactRoot `
            -PathRedactions @{
                $script:RepoRoot = '[REPO_ROOT]'
                'C:\ProgramData\PureCVisor\desktop-node' = '[DATA_ROOT]'
            } `
            -Steps @(
                (New-PcvBatchSupervisorStep `
                    -Id 'print-secret' `
                    -WorkingDirectory $script:RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @('-NoProfile', '-Command', 'Write-Output "Authorization: Bearer abc.def.secret"; Write-Output "C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json"') `
                    -TimeoutSeconds 30)
            )

        $result = Invoke-PcvBatchSupervisor -Manifest $manifest

        $result.ok | Should -BeTrue
        $text = Get-Content -Raw -LiteralPath (Join-Path $artifactRoot 'step-results/001-print-secret.json')
        $text | Should -Match 'Bearer \[REDACTED\]'
        $text | Should -Match '\[DATA_ROOT\]'
        $text | Should -Not -Match 'abc\.def\.secret|api-token\.dpapi\.json'
    }
}
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: FAIL because `PcvBatchSupervisor.psm1` does not exist.

## Task 2: Batch Supervisor Module

**Files:**

- Create: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`

- [x] **Step 1: Implement manifest construction, validation, redaction, and artifact helpers**

Create `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1` with these public functions:

```powershell
Set-StrictMode -Version Latest

function Resolve-PcvBatchRepoRoot {
    param([Parameter(Mandatory)][string]$RepoRoot)
    $resolved = (Resolve-Path -LiteralPath $RepoRoot -ErrorAction Stop).Path
    foreach ($relative in @('AGENTS.md', 'packaging/windows-desktop-node/README.md', 'src/DesktopNode.sln')) {
        if (-not (Test-Path -LiteralPath (Join-Path $resolved $relative))) {
            throw "PCV_BATCH_REPO_BOUNDARY|Repository boundary check failed.|Missing '$relative' under '$resolved'."
        }
    }
    $resolved
}

function Test-PcvBatchSensitiveKey {
    param([AllowNull()][string]$Key)
    -not [string]::IsNullOrWhiteSpace($Key) -and $Key -match '(?i)(authorization|token|api_token|password|secret|private_key|pfx|thumbprint)'
}

function ConvertTo-PcvBatchRedactedText {
    param(
        [AllowNull()][string]$Text,
        [AllowNull()][System.Collections.IDictionary]$PathRedactions
    )
    if ($null -eq $Text) { return $null }
    $redacted = [string]$Text
    $redacted = $redacted -replace '(?i)(Bearer)\s+[A-Za-z0-9._~+/=-]+', '$1 [REDACTED]'
    $redacted = [regex]::Replace(
        $redacted,
        '(?i)(\b(?:token|api_token|password|secret|private_key|pfx|thumbprint)\b\s*[:=]\s*)(?:"[^"]*"|''[^'']*''|[^\s,;}\]]+)',
        { param($Match) $Match.Groups[1].Value + '[REDACTED]' }
    )
    if ($null -ne $PathRedactions) {
        foreach ($path in (@($PathRedactions.Keys) | Sort-Object { ([string]$_).Length } -Descending)) {
            if (-not [string]::IsNullOrWhiteSpace([string]$path)) {
                $redacted = $redacted.Replace([string]$path, [string]$PathRedactions[$path])
            }
        }
    }
    $redacted
}

function ConvertTo-PcvBatchRedactedObject {
    param(
        [AllowNull()]$InputObject,
        [AllowNull()][System.Collections.IDictionary]$PathRedactions
    )
    if ($null -eq $InputObject) { return $null }
    if ($InputObject -is [string]) {
        return ConvertTo-PcvBatchRedactedText -Text $InputObject -PathRedactions $PathRedactions
    }
    if ($InputObject -is [System.Collections.IDictionary]) {
        $out = [ordered]@{}
        foreach ($key in $InputObject.Keys) {
            $out[$key] = if (Test-PcvBatchSensitiveKey -Key ([string]$key)) {
                '[REDACTED]'
            } else {
                ConvertTo-PcvBatchRedactedObject -InputObject $InputObject[$key] -PathRedactions $PathRedactions
            }
        }
        return $out
    }
    if ($InputObject -is [pscustomobject]) {
        $out = [ordered]@{}
        foreach ($property in $InputObject.PSObject.Properties) {
            $out[$property.Name] = if (Test-PcvBatchSensitiveKey -Key $property.Name) {
                '[REDACTED]'
            } else {
                ConvertTo-PcvBatchRedactedObject -InputObject $property.Value -PathRedactions $PathRedactions
            }
        }
        return $out
    }
    if ($InputObject -is [System.Collections.IEnumerable]) {
        $items = @()
        foreach ($item in $InputObject) {
            $items += ConvertTo-PcvBatchRedactedObject -InputObject $item -PathRedactions $PathRedactions
        }
        return $items
    }
    $InputObject
}

function Write-PcvBatchJsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value,
        [int]$Depth = 32
    )
    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $Value | ConvertTo-Json -Depth $Depth | Set-Content -LiteralPath $Path -Encoding UTF8
}

function New-PcvBatchSupervisorStep {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [int]$TimeoutSeconds = 1800,
        [bool]$Required = $true,
        [bool]$AllowFailure = $false,
        [bool]$RequiresAdmin = $false,
        [bool]$MutatesHost = $false,
        [int]$RetryCount = 0
    )
    [ordered]@{
        id = $Id
        working_directory = $WorkingDirectory
        file_name = $FileName
        arguments = @($Arguments)
        timeout_seconds = $TimeoutSeconds
        required = $Required
        allow_failure = $AllowFailure
        requires_admin = $RequiresAdmin
        mutates_host = $MutatesHost
        retry_count = $RetryCount
    }
}
```

- [x] **Step 2: Add profile and manifest builders**

Append:

```powershell
function New-PcvBatchSupervisorProfileSteps {
    param(
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$RepoRoot
    )
    switch ($Profile) {
        'PackagingRegression' {
            @(
                (New-PcvBatchSupervisorStep -Id 'packaging-product-tests' -WorkingDirectory $RepoRoot -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed") -TimeoutSeconds 1800),
                (New-PcvBatchSupervisorStep -Id 'packaging-installer-tests' -WorkingDirectory $RepoRoot -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed") -TimeoutSeconds 1800),
                (New-PcvBatchSupervisorStep -Id 'git-diff-check' -WorkingDirectory $RepoRoot -FileName 'git' -Arguments @('diff', '--check') -TimeoutSeconds 120)
            )
        }
        'WebRegression' {
            @(
                (New-PcvBatchSupervisorStep -Id 'web-pester' -WorkingDirectory $RepoRoot -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'web/tests' -Output Detailed") -TimeoutSeconds 600),
                (New-PcvBatchSupervisorStep -Id 'web-npm-test' -WorkingDirectory $RepoRoot -FileName 'npm' -Arguments @('test', '--prefix', 'web') -TimeoutSeconds 600),
                (New-PcvBatchSupervisorStep -Id 'web-verify-parity' -WorkingDirectory $RepoRoot -FileName 'npm' -Arguments @('run', 'verify:parity', '--prefix', 'web') -TimeoutSeconds 600),
                (New-PcvBatchSupervisorStep -Id 'web-node-check' -WorkingDirectory $RepoRoot -FileName 'node' -Arguments @('--check', 'web/app.js') -TimeoutSeconds 120)
            )
        }
        default {
            throw "PCV_BATCH_PROFILE_UNKNOWN|Unknown batch supervisor profile '$Profile'.|Allowed profiles: PackagingRegression, WebRegression."
        }
    }
}

function New-PcvBatchSupervisorManifest {
    param(
        [Parameter(Mandatory)][string]$BatchId,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$ArtifactRoot,
        [string]$Profile,
        [object[]]$Steps,
        [int]$HeartbeatIntervalSeconds = 5,
        [int]$DefaultTimeoutSeconds = 1800,
        [AllowNull()][System.Collections.IDictionary]$PathRedactions
    )
    $root = Resolve-PcvBatchRepoRoot -RepoRoot $RepoRoot
    $artifact = [System.IO.Path]::GetFullPath($ArtifactRoot)
    $resolvedSteps = if (-not [string]::IsNullOrWhiteSpace($Profile)) {
        New-PcvBatchSupervisorProfileSteps -Profile $Profile -RepoRoot $root
    } else {
        @($Steps)
    }
    if (@($resolvedSteps).Count -eq 0) {
        throw "PCV_BATCH_STEPS_REQUIRED|Batch manifest requires at least one step.|Pass -Profile or -Steps."
    }
    [ordered]@{
        schema_version = 1
        batch_id = $BatchId
        repo_root = $root
        artifact_root = $artifact
        created_at = (Get-Date).ToUniversalTime().ToString('o')
        heartbeat_interval_seconds = $HeartbeatIntervalSeconds
        default_timeout_seconds = $DefaultTimeoutSeconds
        path_redactions = if ($null -eq $PathRedactions) { [ordered]@{ $root = '[REPO_ROOT]' } } else { $PathRedactions }
        steps = @($resolvedSteps)
    }
}
```

- [x] **Step 3: Add command fingerprint and guard validation**

Append:

```powershell
function Get-PcvBatchCommandFingerprint {
    param([Parameter(Mandatory)]$Step)
    $text = ([ordered]@{
        id = [string]$Step.id
        working_directory = [string]$Step.working_directory
        file_name = [string]$Step.file_name
        arguments = @($Step.arguments)
    } | ConvertTo-Json -Depth 12 -Compress)
    $hash = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($text))
    [System.BitConverter]::ToString($hash).Replace('-', '').ToLowerInvariant()
}

function Test-PcvBatchAutomaticRebootCommand {
    param([Parameter(Mandatory)]$Step)
    $combined = ([string]$Step.file_name) + ' ' + (@($Step.arguments) -join ' ')
    $combined -match '(?i)\b(Restart-Computer|shutdown\.exe|shutdown\s+/r|Register-ScheduledTask)\b'
}

function Assert-PcvBatchExecutionAllowed {
    param(
        [Parameter(Mandatory)]$Manifest,
        [bool]$AllowHostMutation = $false,
        [bool]$IsAdministrator = $false
    )
    foreach ($step in @($Manifest.steps)) {
        if (Test-PcvBatchAutomaticRebootCommand -Step $step) {
            throw "PCV_BATCH_REBOOT_COMMAND_FORBIDDEN|Batch supervisor will not execute automatic reboot or scheduled task commands.|step=$($step.id)"
        }
        if (([bool]$step.requires_admin -or [bool]$step.mutates_host) -and -not $AllowHostMutation) {
            throw "PCV_BATCH_HOST_MUTATION_APPROVAL_REQUIRED|Batch step requires explicit host mutation allowance.|step=$($step.id)"
        }
        if ([bool]$step.requires_admin -and -not $IsAdministrator) {
            throw "PCV_BATCH_ADMIN_REQUIRED|Batch step requires an elevated session.|step=$($step.id)"
        }
    }
}

function Test-PcvBatchIsAdministrator {
    $principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
    $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}
```

- [x] **Step 4: Add process execution with heartbeat and timeout**

Append:

```powershell
function Add-PcvBatchHeartbeat {
    param(
        [Parameter(Mandatory)][string]$HeartbeatPath,
        [Parameter(Mandatory)][string]$BatchId,
        [Parameter(Mandatory)][string]$StepId,
        [Parameter(Mandatory)][string]$Status,
        [AllowNull()][string]$Detail
    )
    $line = [ordered]@{
        ts = (Get-Date).ToUniversalTime().ToString('o')
        batch_id = $BatchId
        step_id = $StepId
        status = $Status
        detail = $Detail
    } | ConvertTo-Json -Compress
    Add-Content -LiteralPath $HeartbeatPath -Value $line -Encoding UTF8
}

function Invoke-PcvBatchStepProcess {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)]$Step,
        [Parameter(Mandatory)][int]$Ordinal
    )
    $artifactRoot = [string]$Manifest.artifact_root
    $heartbeatPath = Join-Path $artifactRoot 'heartbeat.jsonl'
    $currentStepPath = Join-Path $artifactRoot 'current-step.json'
    $stepResultsRoot = Join-Path $artifactRoot 'step-results'
    New-Item -ItemType Directory -Path $stepResultsRoot -Force | Out-Null
    $safeId = ([string]$Step.id) -replace '[^A-Za-z0-9._-]', '-'
    $resultPath = Join-Path $stepResultsRoot ('{0:D3}-{1}.json' -f $Ordinal, $safeId)
    $timeoutSeconds = if ([int]$Step.timeout_seconds -gt 0) { [int]$Step.timeout_seconds } else { [int]$Manifest.default_timeout_seconds }
    $heartbeatSeconds = [Math]::Max(1, [int]$Manifest.heartbeat_interval_seconds)
    $fingerprint = Get-PcvBatchCommandFingerprint -Step $Step

    Write-PcvBatchJsonFile -Path $currentStepPath -Value ([ordered]@{
        ts = (Get-Date).ToUniversalTime().ToString('o')
        batch_id = [string]$Manifest.batch_id
        step_id = [string]$Step.id
        ordinal = $Ordinal
        status = 'running'
        timeout_seconds = $timeoutSeconds
    })
    Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $Manifest.batch_id -StepId $Step.id -Status 'started' -Detail ''

    $started = Get-Date
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = [string]$Step.file_name
    $startInfo.WorkingDirectory = [string]$Step.working_directory
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    foreach ($argument in @($Step.arguments)) {
        [void]$startInfo.ArgumentList.Add([string]$argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $timedOut = $false
    $exitCode = $null
    try {
        [void]$process.Start()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        while (-not $process.HasExited) {
            $elapsed = [int]((Get-Date) - $started).TotalSeconds
            Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $Manifest.batch_id -StepId $Step.id -Status 'running' -Detail "elapsed_seconds=$elapsed"
            if ($elapsed -ge $timeoutSeconds) {
                $timedOut = $true
                try {
                    $process.Kill($true)
                    [void]$process.WaitForExit(5000)
                } catch {}
                break
            }
            Start-Sleep -Seconds $heartbeatSeconds
        }
        if (-not $timedOut) {
            $exitCode = $process.ExitCode
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
    }
    finally {
        $process.Dispose()
    }

    $finished = Get-Date
    $ok = -not $timedOut -and $exitCode -eq 0
    $result = [ordered]@{
        schema_version = 1
        step_id = [string]$Step.id
        ordinal = $Ordinal
        command_fingerprint = $fingerprint
        file_name = [string]$Step.file_name
        arguments = ConvertTo-PcvBatchRedactedObject -InputObject @($Step.arguments) -PathRedactions $Manifest.path_redactions
        working_directory = ConvertTo-PcvBatchRedactedText -Text ([string]$Step.working_directory) -PathRedactions $Manifest.path_redactions
        started_at = $started.ToUniversalTime().ToString('o')
        finished_at = $finished.ToUniversalTime().ToString('o')
        duration_ms = [int](($finished - $started).TotalMilliseconds)
        timeout_seconds = $timeoutSeconds
        timed_out = $timedOut
        exit_code = $exitCode
        ok = $ok
        stdout = ConvertTo-PcvBatchRedactedText -Text $stdout -PathRedactions $Manifest.path_redactions
        stderr = ConvertTo-PcvBatchRedactedText -Text $stderr -PathRedactions $Manifest.path_redactions
    }
    Write-PcvBatchJsonFile -Path $resultPath -Value $result
    $finalHeartbeatStatus = if ($ok) { 'completed' } else { 'failed' }
    Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $Manifest.batch_id -StepId $Step.id -Status $finalHeartbeatStatus -Detail "result=$resultPath"
    $result
}
```

- [x] **Step 5: Add supervisor orchestration, resume, and summary**

Append:

```powershell
function Get-PcvBatchPriorSuccessfulResult {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)]$Step,
        [Parameter(Mandatory)][int]$Ordinal
    )
    $safeId = ([string]$Step.id) -replace '[^A-Za-z0-9._-]', '-'
    $path = Join-Path (Join-Path ([string]$Manifest.artifact_root) 'step-results') ('{0:D3}-{1}.json' -f $Ordinal, $safeId)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return $null }
    $prior = Get-Content -Raw -LiteralPath $path | ConvertFrom-Json
    $fingerprint = Get-PcvBatchCommandFingerprint -Step $Step
    if ([bool]$prior.ok -and [string]$prior.command_fingerprint -eq $fingerprint) {
        return $prior
    }
    $null
}

function Invoke-PcvBatchSupervisor {
    param(
        [Parameter(Mandatory)]$Manifest,
        [switch]$DryRun,
        [switch]$Resume,
        [switch]$AllowHostMutation,
        [AllowNull()][Nullable[bool]]$IsAdministrator = $null
    )
    $artifactRoot = [string]$Manifest.artifact_root
    New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
    Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot 'batch-manifest.resolved.json') -Value (ConvertTo-PcvBatchRedactedObject -InputObject $Manifest -PathRedactions $Manifest.path_redactions)

    $admin = if ($null -eq $IsAdministrator) { Test-PcvBatchIsAdministrator } else { [bool]$IsAdministrator }
    Assert-PcvBatchExecutionAllowed -Manifest $Manifest -AllowHostMutation:$AllowHostMutation -IsAdministrator:$admin

    $results = New-Object System.Collections.Generic.List[object]
    $skipped = New-Object System.Collections.Generic.List[string]
    $failedStep = $null
    $status = 'completed'

    if ($DryRun) {
        $summary = [ordered]@{
            schema_version = 1
            ok = $true
            dry_run = $true
            status = 'completed'
            batch_id = [string]$Manifest.batch_id
            artifact_root = $artifactRoot
            total_steps = @($Manifest.steps).Count
            steps = @($Manifest.steps | ForEach-Object { [ordered]@{ id = $_.id; planned = $true } })
        }
        Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot 'summary.json') -Value $summary
        Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot 'current-step.json') -Value ([ordered]@{ ts = (Get-Date).ToUniversalTime().ToString('o'); batch_id = $Manifest.batch_id; status = 'completed'; dry_run = $true })
        return [pscustomobject]$summary
    }

    $ordinal = 0
    foreach ($step in @($Manifest.steps)) {
        $ordinal++
        if ($Resume) {
            $prior = Get-PcvBatchPriorSuccessfulResult -Manifest $Manifest -Step $step -Ordinal $ordinal
            if ($null -ne $prior) {
                $skipped.Add([string]$step.id) | Out-Null
                continue
            }
        }
        $result = Invoke-PcvBatchStepProcess -Manifest $Manifest -Step $step -Ordinal $ordinal
        $results.Add($result) | Out-Null
        if (-not [bool]$result.ok -and -not [bool]$step.allow_failure) {
            $failedStep = [string]$step.id
            $status = 'failed'
            break
        }
    }

    $summary = [ordered]@{
        schema_version = 1
        ok = $status -eq 'completed'
        dry_run = $false
        status = $status
        batch_id = [string]$Manifest.batch_id
        artifact_root = $artifactRoot
        total_steps = @($Manifest.steps).Count
        executed_steps = @($results).Count
        skipped_steps = @($skipped)
        failed_step_id = $failedStep
        next_resume_step_id = if ($failedStep) { $failedStep } else { $null }
        results = @($results)
    }
    Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot 'summary.json') -Value $summary
    Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot 'current-step.json') -Value ([ordered]@{ ts = (Get-Date).ToUniversalTime().ToString('o'); batch_id = $Manifest.batch_id; status = $status; failed_step_id = $failedStep })
    [pscustomobject]$summary
}

Export-ModuleMember -Function `
    Resolve-PcvBatchRepoRoot, `
    New-PcvBatchSupervisorStep, `
    New-PcvBatchSupervisorManifest, `
    New-PcvBatchSupervisorProfileSteps, `
    Invoke-PcvBatchSupervisor, `
    ConvertTo-PcvBatchRedactedText, `
    ConvertTo-PcvBatchRedactedObject, `
    Assert-PcvBatchExecutionAllowed
```

- [x] **Step 6: Run focused tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: PASS.

## Task 3: CLI Entrypoint

**Files:**

- Create: `packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`

- [x] **Step 1: Add entrypoint test**

Append to `PcvBatchSupervisor.Tests.ps1`:

```powershell
It 'runs the CLI entrypoint from a manifest file' {
    $artifactRoot = Join-Path $TestDrive 'batch-entrypoint'
    $manifestPath = Join-Path $TestDrive 'manifest.json'
    $manifest = New-PcvBatchSupervisorManifest `
        -BatchId 'entrypoint' `
        -RepoRoot $script:RepoRoot `
        -ArtifactRoot $artifactRoot `
        -Steps @(
            (New-PcvBatchSupervisorStep `
                -Id 'echo' `
                -WorkingDirectory $script:RepoRoot `
                -FileName 'pwsh' `
                -Arguments @('-NoProfile', '-Command', 'Write-Output entrypoint-ok') `
                -TimeoutSeconds 30)
        )
    $manifest | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
    $entrypoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1'

    $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $entrypoint -ManifestPath $manifestPath | ConvertFrom-Json

    $LASTEXITCODE | Should -Be 0
    $json.ok | Should -BeTrue
    $json.batch_id | Should -Be 'entrypoint'
    Test-Path -LiteralPath (Join-Path $artifactRoot 'summary.json') | Should -BeTrue
}
```

- [x] **Step 2: Verify entrypoint test fails**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: FAIL because `Invoke-PcvBatchSupervisor.ps1` does not exist.

- [x] **Step 3: Implement entrypoint**

Create `packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1`:

```powershell
param(
    [Parameter(Mandatory)][string]$ManifestPath,
    [switch]$DryRun,
    [switch]$Resume,
    [switch]$AllowHostMutation
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot 'PcvBatchSupervisor.psm1'
Import-Module $modulePath -Force

try {
    $manifest = Get-Content -Raw -LiteralPath $ManifestPath -ErrorAction Stop | ConvertFrom-Json -Depth 32 -AsHashtable -ErrorAction Stop
    $result = Invoke-PcvBatchSupervisor `
        -Manifest $manifest `
        -DryRun:$DryRun `
        -Resume:$Resume `
        -AllowHostMutation:$AllowHostMutation
    $result | ConvertTo-Json -Depth 32
    if ([bool]$result.ok) {
        exit 0
    }
    exit 1
}
catch {
    [ordered]@{
        ok = $false
        error = [string]$_
    } | ConvertTo-Json -Depth 8
    exit 1
}
```

- [x] **Step 4: Run focused tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: PASS.

## Task 4: Built-In Batch Manifest Examples

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1`

- [x] **Step 1: Add tests for generated profile manifests**

Append:

```powershell
It 'builds WebRegression profile without host mutation commands' {
    $manifest = New-PcvBatchSupervisorManifest `
        -BatchId 'web-regression' `
        -RepoRoot $script:RepoRoot `
        -ArtifactRoot (Join-Path $TestDrive 'web-regression') `
        -Profile WebRegression

    $manifest.steps.Count | Should -Be 4
    @($manifest.steps | ForEach-Object { $_.id }) | Should -Contain 'web-pester'
    @($manifest.steps | ForEach-Object { $_.id }) | Should -Contain 'web-verify-parity'
    ($manifest.steps | ConvertTo-Json -Depth 12) | Should -Not -Match 'msiexec|New-VM|Remove-VM|New-NetFirewallRule|Restart-Computer'
}
```

- [x] **Step 2: Run focused tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: PASS because `WebRegression` was added in Task 2.

- [x] **Step 3: Add optional manifest writer helper if needed**

If CLI manifest authoring is too manual during implementation, add this helper:

```powershell
function Save-PcvBatchSupervisorManifest {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)][string]$Path
    )
    Write-PcvBatchJsonFile -Path $Path -Value $Manifest
    [ordered]@{ ok = $true; path = [System.IO.Path]::GetFullPath($Path) }
}
```

Also export it and add a Pester assertion:

```powershell
$save = Save-PcvBatchSupervisorManifest -Manifest $manifest -Path (Join-Path $TestDrive 'manifest.json')
$save.ok | Should -BeTrue
Test-Path -LiteralPath $save.path | Should -BeTrue
```

## Task 5: Documentation

**Files:**

- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

- [x] **Step 1: Document tool usage in packaging README**

Add this section to `packaging/windows-desktop-node/README.md` near verification/tooling guidance:

```markdown
## Batch Supervisor / Hang Guard

`packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1` executes repo-local development and verification batches from a JSON manifest. It is not product runtime code and does not register services, scheduled tasks, firewall rules, trust-store entries, or reboot actions.

The supervisor writes:

- `batch-manifest.resolved.json`
- `current-step.json`
- `heartbeat.jsonl`
- `step-results/*.json`
- `summary.json`

Non-mutating example:

```powershell
$manifest = New-PcvBatchSupervisorManifest `
  -BatchId 'packaging-regression' `
  -RepoRoot (Resolve-Path .).Path `
  -ArtifactRoot 'artifacts/batch-runs/packaging-regression-local' `
  -Profile PackagingRegression
$manifest | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath 'artifacts/batch-runs/packaging-regression-local/manifest.json' -Encoding UTF8
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/packaging-regression-local/manifest.json
```

Host-mutating steps must set `requires_admin=true` or `mutates_host=true` and require `-AllowHostMutation` at the entrypoint. Automatic reboot commands remain forbidden in v1.
```

- [x] **Step 2: Document verification policy**

Add to `docs/DEVELOPMENT_VERIFICATION_POLICY.md`:

```markdown
## Batch Supervisor / Hang Guard

Long-running development batches should use the repo-local Batch Supervisor when the expected runtime is long enough that hang recovery matters. The supervisor evidence root must include `current-step.json`, `heartbeat.jsonl`, per-step JSON results, and `summary.json`.

Rules:

- Non-mutating verification profiles do not require admin approval.
- Steps with `requires_admin=true` or `mutates_host=true` require explicit `-AllowHostMutation` and an elevated shell.
- Automatic reboot commands are forbidden in v1.
- Resume can skip only successful steps with matching command fingerprints.
- Captured output must be redacted before being written to summary artifacts.
```

- [x] **Step 3: Run docs-adjacent tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1','archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
git diff --check
```

Expected: PASS and `git diff --check` exit 0.

## Task 6: Full Verification

**Files:**

- No new files unless docs changed during fixes.

- [x] **Step 1: Run focused supervisor suite**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvBatchSupervisor.Tests.ps1' -Output Detailed"
```

Expected: all tests pass.

- [x] **Step 2: Run packaging suite**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: all tests pass.

- [x] **Step 3: Run installer suite**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected: all tests pass.

- [x] **Step 4: Run .NET solution tests**

```powershell
dotnet test src/DesktopNode.sln
```

Expected: all tests pass.

- [x] **Step 5: Run Web verification if docs or default commands mention WebRegression**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
node --check web/app.js
```

Expected: all pass.

- [x] **Step 6: Run documentation guard and diff hygiene**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
git diff --check
```

Expected: all pass. Line-ending warnings are acceptable only if exit code is 0.

## Task 7: Optional Smoke Through the New Supervisor

**Files:**

- Generated artifact only under `artifacts/batch-runs/**`.

- [x] **Step 1: Create and run a non-mutating supervisor smoke manifest**

Run:

```powershell
pwsh -NoProfile -Command "
Import-Module './packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1' -Force
$artifact = 'artifacts/batch-runs/batch-supervisor-packaging-smoke-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$manifest = New-PcvBatchSupervisorManifest -BatchId 'batch-supervisor-packaging-smoke' -RepoRoot (Resolve-Path .).Path -ArtifactRoot $artifact -Profile PackagingRegression
$manifestPath = Join-Path $artifact 'manifest.json'
New-Item -ItemType Directory -Path $artifact -Force | Out-Null
$manifest | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $manifestPath -Encoding UTF8
pwsh -NoProfile -ExecutionPolicy Bypass -File './packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1' -ManifestPath $manifestPath
"
```

Expected:

- command exit code 0
- `summary.json` has `ok=true`
- `heartbeat.jsonl` exists
- `step-results` contains packaging product tests, installer tests, and diff check

- [x] **Step 2: Do not run mutating admin smoke in this implementation batch by default**

Admin smoke under the supervisor is a follow-up execution decision. If needed later, wrap the existing `Invoke-PcvRouteParityMutationSmoke.ps1` command in a manifest step with:

```json
{
  "id": "routeparity-service-msi-hyperv-admin-smoke",
  "working_directory": ".",
  "file_name": "pwsh",
  "arguments": [
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    "packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1",
    "-Version",
    "0.36.1-admin-smoke",
    "-IsoPath",
    "D:\\Downloads\\Rocky-10.1-x86_64-minimal.iso",
    "-ArtifactRoot",
    "artifacts/routeparity-service-msi-hyperv-batch-supervised-20260505"
  ],
  "timeout_seconds": 1800,
  "required": true,
  "allow_failure": false,
  "requires_admin": true,
  "mutates_host": true,
  "retry_count": 0
}
```

Run only with `-AllowHostMutation` from an elevated shell.

## Risks and Controls

- Risk: Process stdout/stderr deadlock.
  - Control: use redirected async `ReadToEndAsync()` and periodic `HasExited` polling.
- Risk: Resume skips changed commands.
  - Control: command fingerprint includes id, working directory, file name, and arguments.
- Risk: Sensitive token output in artifacts.
  - Control: redact arguments/stdout/stderr and path values before writing JSON.
- Risk: Host mutation without explicit approval.
  - Control: manifest flags and entrypoint `-AllowHostMutation` gate, admin check, reboot command denylist.
- Risk: v1 becomes a hidden product dependency.
  - Control: keep files under `packaging/windows-desktop-node/tools`, document as development/verification runner only.

## Self-Review Checklist

- Spec coverage: v1 covers hang detection, timeout, heartbeat, resume, evidence, host mutation gate, and redaction.
- Placeholder scan: no unresolved placeholders; optional smoke is explicitly scoped.
- Type consistency: manifest uses `schema_version`, `batch_id`, `artifact_root`, `heartbeat_interval_seconds`, `default_timeout_seconds`, and `steps` consistently.
- Scope check: does not include Task Scheduler, reboot, Web UI, product runtime, or config/job store apply writes.
