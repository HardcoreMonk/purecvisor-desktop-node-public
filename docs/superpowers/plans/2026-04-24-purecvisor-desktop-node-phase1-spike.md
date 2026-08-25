# PureCVisor Desktop Node Phase 1 Spike Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Phase 1 Hyper-V PowerShell helper spike for PureCVisor Desktop Node, proving the JSON contract for host diagnostics, VM inventory, ISO-based VM creation, lifecycle actions, and checkpoint actions.

**Architecture:** This spike is isolated under `spikes/purecvisor-desktop-node/hyperv/` and does not modify the existing Linux daemon. A single runner script reads JSON input, dispatches only allowlisted operations, calls a focused PowerShell module, and emits one JSON response shape for success and failure.

**Tech Stack:** PowerShell 7, Hyper-V PowerShell cmdlets, Pester 5, JSON stdin/file contracts, Windows 10/11 Pro/Enterprise with Hyper-V for gated integration tests.

---

## Completion Status

Phase 1 is complete and merged to `main`.

- Final hardening commit: `1804d7e fix: harden Hyper-V spike failure cleanup`
- Implemented path: `spikes/purecvisor-desktop-node/hyperv/`
- Implemented operations: `host.status`, `vm.list`, `vm.create`, `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart`, `checkpoint.list`, `checkpoint.create`, `checkpoint.restore`, `checkpoint.delete`
- Verification: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"` reports 41 passed, 0 failed, 1 NotRun
- Real Hyper-V integration remains gated by `PCV_HYPERV_INTEGRATION=1` and `PCV_HYPERV_TEST_ISO`; 준비된 관리자 Hyper-V 호스트에서 전체 suite는 42 passed, 0 failed를 보고한다
- Current hardening: host readiness는 optional-feature 상태 조회가 불가능할 때 Hyper-V cmdlet 사용 가능 여부를 사용하고, VM lookup은 로컬라이즈된 Hyper-V not-found 메시지를 VM 부재로 처리하며, unit test는 실제 Hyper-V cmdlet parameter metadata를 shadowing해 Pester mock을 결정적으로 유지한다
- Next phase: design and implement the Local API daemon that calls this helper contract

---

## Scope Check

The approved product spec covers the full Desktop Node MVP, including API daemon, Web Console, CLI, LAN mode, and long-running jobs. That is too large for one implementation plan. This plan covers only the first independently testable increment: the Hyper-V helper spike that later API, CLI, and UI layers can call.

This plan intentionally does not build the REST API daemon, Web Console, CLI, authentication, LAN binding, or Windows service. Those components should each receive their own plan after this helper contract is validated.

## File Structure

Create the following files:

```text
spikes/purecvisor-desktop-node/hyperv/
  README.md
  Invoke-PcvHyperV.ps1
  PcvHyperV.psm1
  examples/
    host-status.json
    vm-list.json
    vm-create.json
    vm-start.json
    checkpoint-create.json
  tests/
    PcvHyperV.Contract.Tests.ps1
    PcvHyperV.HostStatus.Tests.ps1
    PcvHyperV.Inventory.Tests.ps1
    PcvHyperV.Provisioning.Tests.ps1
    PcvHyperV.LifecycleCheckpoint.Tests.ps1
    PcvHyperV.Integration.Tests.ps1
```

Responsibilities:

- `Invoke-PcvHyperV.ps1`: thin command runner. Reads input JSON from `-InputPath` or stdin, dispatches an allowlisted operation, and writes compact JSON to stdout.
- `PcvHyperV.psm1`: all spike logic. Contains response helpers, validation helpers, host diagnostics, inventory, provisioning, lifecycle, and checkpoint functions.
- `examples/*.json`: concrete payloads for manual spike testing.
- `tests/*.Tests.ps1`: Pester contract tests and gated Hyper-V integration tests.

## Response Contract

Every operation must return exactly one JSON object.

Success:

```json
{
  "ok": true,
  "operation": "host.status",
  "data": {},
  "error": null
}
```

Failure:

```json
{
  "ok": false,
  "operation": "vm.start",
  "data": null,
  "error": {
    "code": "PCV_VM_NOT_FOUND",
    "message": "VM 'ubuntu-lab-01' was not found.",
    "detail": "Hyper-V Get-VM did not return a VM with this name.",
    "retryable": false
  }
}
```

## Operation Names

The runner allowlist is fixed for this spike:

```text
host.status
vm.list
vm.create
vm.start
vm.shutdown
vm.poweroff
vm.restart
checkpoint.list
checkpoint.create
checkpoint.restore
checkpoint.delete
```

---

### Task 1: Runner Contract And Example Payloads

**Files:**
- Create: `spikes/purecvisor-desktop-node/hyperv/Invoke-PcvHyperV.ps1`
- Create: `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`
- Create: `spikes/purecvisor-desktop-node/hyperv/examples/host-status.json`
- Create: `spikes/purecvisor-desktop-node/hyperv/examples/vm-list.json`
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1`

- [ ] **Step 1: Create the failing contract tests**

Create `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1`:

```powershell
$Root = Split-Path -Parent $PSScriptRoot
$Runner = Join-Path $Root 'Invoke-PcvHyperV.ps1'

Describe 'Invoke-PcvHyperV contract' {
    It 'returns PCV_INPUT_MISSING when no input is provided' {
        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'unknown'
        $json.error.code | Should -Be 'PCV_INPUT_MISSING'
        $json.error.retryable | Should -BeFalse
    }

    It 'rejects operations outside the allowlist' {
        $payload = @{ operation = 'shell.exec'; params = @{ command = 'Get-Process' } } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'shell.exec'
        $json.error.code | Should -Be 'PCV_OPERATION_NOT_ALLOWED'
        $json.error.retryable | Should -BeFalse
    }

    It 'returns a successful host.status response shape' {
        $payload = @{ operation = 'host.status'; params = @{} } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'host.status'
        $json.error | Should -BeNullOrEmpty
        $json.data.hyperv | Should -Not -BeNullOrEmpty
        $json.data.admin | Should -Not -BeNullOrEmpty
    }

    It 'accepts an input file path' {
        $inputPath = Join-Path $TestDrive 'host-status.json'
        @{ operation = 'host.status'; params = @{} } | ConvertTo-Json -Depth 8 | Set-Content -Path $inputPath -Encoding UTF8

        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner -InputPath $inputPath | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'host.status'
    }
}
```

- [ ] **Step 2: Run the contract tests and verify the runner is missing**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1' -Output Detailed"
```

Expected: FAIL with an error containing `Invoke-PcvHyperV.ps1` because the runner file does not exist yet.

- [ ] **Step 3: Create the minimal module**

Create `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`:

```powershell
Set-StrictMode -Version Latest

function New-PcvError {
    param(
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][string]$Detail,
        [Parameter(Mandatory)][bool]$Retryable
    )

    [ordered]@{
        code = $Code
        message = $Message
        detail = $Detail
        retryable = $Retryable
    }
}

function New-PcvResponse {
    param(
        [Parameter(Mandatory)][bool]$Ok,
        [Parameter(Mandatory)][string]$Operation,
        [AllowNull()]$Data,
        [AllowNull()]$ErrorObject
    )

    [ordered]@{
        ok = $Ok
        operation = $Operation
        data = $Data
        error = $ErrorObject
    }
}

function ConvertTo-PcvJson {
    param([Parameter(Mandatory)]$Value)
    $Value | ConvertTo-Json -Depth 20 -Compress
}

function Test-PcvAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

function Get-PcvHostStatus {
    $isAdmin = Test-PcvAdmin
    [ordered]@{
        windows = [ordered]@{
            caption = $null
            version = $null
            edition = $null
        }
        admin = [ordered]@{
            elevated = $isAdmin
        }
        hyperv = [ordered]@{
            feature_enabled = $false
            vmms_running = $false
            default_switch_present = $false
        }
    }
}

function Invoke-PcvOperation {
    param([Parameter(Mandatory)]$Request)

    $operation = [string]$Request.operation
    $allowed = @(
        'host.status',
        'vm.list',
        'vm.create',
        'vm.start',
        'vm.shutdown',
        'vm.poweroff',
        'vm.restart',
        'checkpoint.list',
        'checkpoint.create',
        'checkpoint.restore',
        'checkpoint.delete'
    )

    if ($allowed -notcontains $operation) {
        return New-PcvResponse -Ok $false -Operation $operation -Data $null -ErrorObject (
            New-PcvError `
                -Code 'PCV_OPERATION_NOT_ALLOWED' `
                -Message "Operation '$operation' is not allowed." `
                -Detail 'The runner only dispatches the fixed Desktop Node Hyper-V spike operation allowlist.' `
                -Retryable $false
        )
    }

    if ($operation -eq 'host.status') {
        return New-PcvResponse -Ok $true -Operation $operation -Data (Get-PcvHostStatus) -ErrorObject $null
    }

    return New-PcvResponse -Ok $false -Operation $operation -Data $null -ErrorObject (
        New-PcvError `
            -Code 'PCV_OPERATION_NOT_IMPLEMENTED' `
            -Message "Operation '$operation' is allowed but is not implemented in this task." `
            -Detail 'Implement the operation in the dedicated follow-up task before using it.' `
            -Retryable $false
    )
}

Export-ModuleMember -Function `
    New-PcvError, `
    New-PcvResponse, `
    ConvertTo-PcvJson, `
    Test-PcvAdmin, `
    Get-PcvHostStatus, `
    Invoke-PcvOperation
```

- [ ] **Step 4: Create the runner**

Create `spikes/purecvisor-desktop-node/hyperv/Invoke-PcvHyperV.ps1`:

```powershell
[CmdletBinding()]
param(
    [string]$InputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ModulePath = Join-Path $PSScriptRoot 'PcvHyperV.psm1'
Import-Module $ModulePath -Force

function Read-PcvInputJson {
    param([string]$Path)

    if ($Path) {
        if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
            return $null
        }
        return Get-Content -LiteralPath $Path -Raw
    }

    if ([Console]::IsInputRedirected) {
        return [Console]::In.ReadToEnd()
    }

    return $null
}

$raw = Read-PcvInputJson -Path $InputPath
if ([string]::IsNullOrWhiteSpace($raw)) {
    New-PcvResponse `
        -Ok $false `
        -Operation 'unknown' `
        -Data $null `
        -ErrorObject (New-PcvError `
            -Code 'PCV_INPUT_MISSING' `
            -Message 'No JSON request was provided.' `
            -Detail 'Pass -InputPath or pipe a JSON request to stdin.' `
            -Retryable $false) |
        ConvertTo-PcvJson
    exit 2
}

try {
    $request = $raw | ConvertFrom-Json -Depth 20
    if (-not $request.PSObject.Properties.Name.Contains('operation')) {
        New-PcvResponse `
            -Ok $false `
            -Operation 'unknown' `
            -Data $null `
            -ErrorObject (New-PcvError `
                -Code 'PCV_OPERATION_MISSING' `
                -Message 'Request JSON does not contain an operation field.' `
                -Detail 'The request must include an operation string and a params object.' `
                -Retryable $false) |
            ConvertTo-PcvJson
        exit 2
    }

    Invoke-PcvOperation -Request $request | ConvertTo-PcvJson
    exit 0
}
catch {
    New-PcvResponse `
        -Ok $false `
        -Operation 'unknown' `
        -Data $null `
        -ErrorObject (New-PcvError `
            -Code 'PCV_RUNNER_EXCEPTION' `
            -Message 'The Hyper-V helper runner failed before completing the operation.' `
            -Detail $_.Exception.Message `
            -Retryable $false) |
        ConvertTo-PcvJson
    exit 1
}
```

- [ ] **Step 5: Create example payloads**

Create `spikes/purecvisor-desktop-node/hyperv/examples/host-status.json`:

```json
{
  "operation": "host.status",
  "params": {}
}
```

Create `spikes/purecvisor-desktop-node/hyperv/examples/vm-list.json`:

```json
{
  "operation": "vm.list",
  "params": {}
}
```

- [ ] **Step 6: Run the contract tests and verify they pass**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1' -Output Detailed"
```

Expected: PASS with 4 tests passing.

- [ ] **Step 7: Commit**

```powershell
git add spikes/purecvisor-desktop-node/hyperv/Invoke-PcvHyperV.ps1 spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1 spikes/purecvisor-desktop-node/hyperv/examples/host-status.json spikes/purecvisor-desktop-node/hyperv/examples/vm-list.json spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1
git commit -m "spike: add Hyper-V helper contract runner"
```

---

### Task 2: Host Diagnostics

**Files:**
- Modify: `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1`

- [ ] **Step 1: Write the failing host diagnostics tests**

Create `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1`:

```powershell
$Root = Split-Path -Parent $PSScriptRoot
$ModulePath = Join-Path $Root 'PcvHyperV.psm1'

Describe 'Get-PcvHostStatus' {
    BeforeEach {
        Import-Module $ModulePath -Force

        Mock Test-PcvAdmin { $true } -ModuleName PcvHyperV
        Mock Get-ComputerInfo {
            [pscustomobject]@{
                WindowsProductName = 'Windows 11 Pro'
                WindowsVersion = '23H2'
                OsHardwareAbstractionLayer = '10.0.22631.1'
            }
        } -ModuleName PcvHyperV
        Mock Get-WindowsOptionalFeature {
            [pscustomobject]@{
                FeatureName = 'Microsoft-Hyper-V'
                State = 'Enabled'
            }
        } -ModuleName PcvHyperV
        Mock Get-Service {
            [pscustomobject]@{
                Name = 'vmms'
                Status = 'Running'
            }
        } -ModuleName PcvHyperV
        Mock Get-VMSwitch {
            @(
                [pscustomobject]@{ Name = 'Default Switch'; SwitchType = 'Internal' }
            )
        } -ModuleName PcvHyperV
    }

    It 'reports Windows, admin, Hyper-V, VMMS, and Default Switch state' {
        $status = Get-PcvHostStatus

        $status.windows.caption | Should -Be 'Windows 11 Pro'
        $status.windows.version | Should -Be '23H2'
        $status.windows.edition | Should -Be 'Pro'
        $status.admin.elevated | Should -BeTrue
        $status.hyperv.feature_enabled | Should -BeTrue
        $status.hyperv.vmms_running | Should -BeTrue
        $status.hyperv.default_switch_present | Should -BeTrue
    }

    It 'marks unsupported Windows Home as unsupported' {
        Mock Get-ComputerInfo {
            [pscustomobject]@{
                WindowsProductName = 'Windows 11 Home'
                WindowsVersion = '23H2'
                OsHardwareAbstractionLayer = '10.0.22631.1'
            }
        } -ModuleName PcvHyperV

        $status = Get-PcvHostStatus

        $status.windows.edition | Should -Be 'Home'
        $status.supported | Should -BeFalse
        $status.reasons | Should -Contain 'PCV_WINDOWS_EDITION_UNSUPPORTED'
    }

    It 'keeps working when Hyper-V cmdlets are unavailable' {
        Mock Get-WindowsOptionalFeature { throw 'Get-WindowsOptionalFeature unavailable' } -ModuleName PcvHyperV
        Mock Get-VMSwitch { throw 'Get-VMSwitch unavailable' } -ModuleName PcvHyperV

        $status = Get-PcvHostStatus

        $status.hyperv.feature_enabled | Should -BeFalse
        $status.hyperv.default_switch_present | Should -BeFalse
        $status.reasons | Should -Contain 'PCV_HYPERV_FEATURE_UNKNOWN'
        $status.reasons | Should -Contain 'PCV_DEFAULT_SWITCH_UNKNOWN'
    }
}
```

- [ ] **Step 2: Run the host diagnostics tests and verify they fail**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1' -Output Detailed"
```

Expected: FAIL because `Get-PcvHostStatus` still returns stub values.

- [ ] **Step 3: Replace the host diagnostics implementation**

In `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`, replace `Get-PcvHostStatus` with:

```powershell
function Get-PcvWindowsEdition {
    param([AllowNull()][string]$ProductName)

    if ($ProductName -match 'Enterprise') { return 'Enterprise' }
    if ($ProductName -match 'Professional|Pro') { return 'Pro' }
    if ($ProductName -match 'Education') { return 'Education' }
    if ($ProductName -match 'Home') { return 'Home' }
    return 'Unknown'
}

function Get-PcvHostStatus {
    $reasons = New-Object System.Collections.Generic.List[string]

    $caption = $null
    $version = $null
    $edition = 'Unknown'
    try {
        $computer = Get-ComputerInfo
        $caption = [string]$computer.WindowsProductName
        $version = [string]$computer.WindowsVersion
        $edition = Get-PcvWindowsEdition -ProductName $caption
    }
    catch {
        $reasons.Add('PCV_WINDOWS_INFO_UNKNOWN')
    }

    $isSupportedEdition = $edition -in @('Pro', 'Enterprise', 'Education')
    if (-not $isSupportedEdition) {
        $reasons.Add('PCV_WINDOWS_EDITION_UNSUPPORTED')
    }

    $featureEnabled = $false
    try {
        $feature = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V
        $featureEnabled = ([string]$feature.State -eq 'Enabled')
        if (-not $featureEnabled) {
            $reasons.Add('PCV_HYPERV_NOT_ENABLED')
        }
    }
    catch {
        $reasons.Add('PCV_HYPERV_FEATURE_UNKNOWN')
    }

    $vmmsRunning = $false
    try {
        $vmms = Get-Service -Name vmms
        $vmmsRunning = ([string]$vmms.Status -eq 'Running')
        if (-not $vmmsRunning) {
            $reasons.Add('PCV_VMMS_NOT_RUNNING')
        }
    }
    catch {
        $reasons.Add('PCV_VMMS_UNKNOWN')
    }

    $defaultSwitchPresent = $false
    try {
        $switches = @(Get-VMSwitch -ErrorAction Stop)
        $defaultSwitchPresent = [bool]($switches | Where-Object { $_.Name -eq 'Default Switch' } | Select-Object -First 1)
        if (-not $defaultSwitchPresent) {
            $reasons.Add('PCV_DEFAULT_SWITCH_MISSING')
        }
    }
    catch {
        $reasons.Add('PCV_DEFAULT_SWITCH_UNKNOWN')
    }

    $isAdmin = Test-PcvAdmin
    if (-not $isAdmin) {
        $reasons.Add('PCV_ADMIN_REQUIRED')
    }

    [ordered]@{
        supported = ($isSupportedEdition -and $featureEnabled -and $vmmsRunning -and $defaultSwitchPresent -and $isAdmin)
        reasons = @($reasons)
        windows = [ordered]@{
            caption = $caption
            version = $version
            edition = $edition
        }
        admin = [ordered]@{
            elevated = $isAdmin
        }
        hyperv = [ordered]@{
            feature_enabled = $featureEnabled
            vmms_running = $vmmsRunning
            default_switch_present = $defaultSwitchPresent
        }
    }
}
```

Add `Get-PcvWindowsEdition` to the `Export-ModuleMember` list.

- [ ] **Step 4: Run host diagnostics and contract tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1 spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1
git commit -m "spike: add Hyper-V host diagnostics"
```

---

### Task 3: VM Inventory

**Files:**
- Modify: `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Inventory.Tests.ps1`

- [ ] **Step 1: Write the failing inventory tests**

Create `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Inventory.Tests.ps1`:

```powershell
$Root = Split-Path -Parent $PSScriptRoot
$ModulePath = Join-Path $Root 'PcvHyperV.psm1'
$Runner = Join-Path $Root 'Invoke-PcvHyperV.ps1'

Describe 'VM inventory' {
    BeforeEach {
        Import-Module $ModulePath -Force

        Mock Get-VM {
            @(
                [pscustomobject]@{
                    Name = 'ubuntu-lab-01'
                    State = 'Running'
                    ProcessorCount = 2
                    MemoryStartup = 4294967296
                    Uptime = [timespan]::FromMinutes(12)
                    Notes = 'managed-by=purecvisor-desktop-node'
                }
            )
        } -ModuleName PcvHyperV
        Mock Get-VMHardDiskDrive {
            @(
                [pscustomobject]@{ Path = 'D:\PureCVisor\VMs\ubuntu-lab-01\disk0.vhdx' }
            )
        } -ModuleName PcvHyperV
        Mock Get-VHD {
            [pscustomobject]@{ Size = 42949672960 }
        } -ModuleName PcvHyperV
        Mock Get-VMNetworkAdapter {
            @(
                [pscustomobject]@{ SwitchName = 'Default Switch' }
            )
        } -ModuleName PcvHyperV
        Mock Get-VMSnapshot {
            @(
                [pscustomobject]@{ Name = 'before-upgrade' }
            )
        } -ModuleName PcvHyperV
    }

    It 'maps Hyper-V VMs into the PureCVisor VM model' {
        $list = Get-PcvVmList

        $list.Count | Should -Be 1
        $vm = $list[0]
        $vm.id | Should -Be 'ubuntu-lab-01'
        $vm.platform | Should -Be 'hyperv'
        $vm.guest_family | Should -Be 'linux'
        $vm.state | Should -Be 'running'
        $vm.cpu.count | Should -Be 2
        $vm.memory.startup_mb | Should -Be 4096
        $vm.storage[0].size_gb | Should -Be 40
        $vm.network[0].switch | Should -Be 'Default Switch'
        $vm.checkpoints.count | Should -Be 1
        $vm.managed_by_purecvisor | Should -BeTrue
    }

    It 'dispatches vm.list through the runner' {
        $payload = @{ operation = 'vm.list'; params = @{} } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $json.data | Should -Not -BeNullOrEmpty
    }
}
```

- [ ] **Step 2: Run the inventory tests and verify they fail**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Inventory.Tests.ps1' -Output Detailed"
```

Expected: FAIL because `Get-PcvVmList` does not exist and `vm.list` returns `PCV_OPERATION_NOT_IMPLEMENTED`.

- [ ] **Step 3: Add VM inventory functions**

Append these functions to `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`:

```powershell
function Convert-PcvBytesToMiB {
    param([Parameter(Mandatory)][UInt64]$Bytes)
    [int][math]::Round($Bytes / 1MB)
}

function Convert-PcvBytesToGiB {
    param([Parameter(Mandatory)][UInt64]$Bytes)
    [int][math]::Round($Bytes / 1GB)
}

function Get-PcvVmState {
    param([Parameter(Mandatory)][string]$State)
    switch ($State.ToLowerInvariant()) {
        'running' { 'running' }
        'off' { 'stopped' }
        'paused' { 'paused' }
        'saved' { 'saved' }
        default { $State.ToLowerInvariant() }
    }
}

function Test-PcvManagedVm {
    param([AllowNull()][string]$Notes)
    if ([string]::IsNullOrWhiteSpace($Notes)) { return $false }
    return ($Notes -match 'managed-by=purecvisor-desktop-node')
}

function Get-PcvVmList {
    $vms = @(Get-VM)
    $result = New-Object System.Collections.Generic.List[object]

    foreach ($vm in $vms) {
        $disks = New-Object System.Collections.Generic.List[object]
        foreach ($drive in @(Get-VMHardDiskDrive -VMName $vm.Name -ErrorAction SilentlyContinue)) {
            $sizeGb = $null
            try {
                $vhd = Get-VHD -Path $drive.Path -ErrorAction Stop
                $sizeGb = Convert-PcvBytesToGiB -Bytes ([UInt64]$vhd.Size)
            }
            catch {
                $sizeGb = $null
            }
            $disks.Add([ordered]@{
                kind = 'vhdx'
                path = $drive.Path
                size_gb = $sizeGb
            })
        }

        $networks = New-Object System.Collections.Generic.List[object]
        foreach ($adapter in @(Get-VMNetworkAdapter -VMName $vm.Name -ErrorAction SilentlyContinue)) {
            $networks.Add([ordered]@{
                switch = $adapter.SwitchName
                mode = if ($adapter.SwitchName -eq 'Default Switch') { 'default-switch' } else { 'hyperv-switch' }
            })
        }

        $snapshots = @(Get-VMSnapshot -VMName $vm.Name -ErrorAction SilentlyContinue)

        $result.Add([ordered]@{
            id = $vm.Name
            name = $vm.Name
            platform = 'hyperv'
            guest_family = 'linux'
            state = Get-PcvVmState -State ([string]$vm.State)
            cpu = [ordered]@{
                count = [int]$vm.ProcessorCount
            }
            memory = [ordered]@{
                startup_mb = Convert-PcvBytesToMiB -Bytes ([UInt64]$vm.MemoryStartup)
                dynamic = $false
            }
            storage = @($disks)
            network = @($networks)
            checkpoints = [ordered]@{
                count = $snapshots.Count
            }
            console = [ordered]@{
                type = 'vmconnect'
                available_local = $true
            }
            managed_by_purecvisor = Test-PcvManagedVm -Notes ([string]$vm.Notes)
        })
    }

    @($result)
}
```

In `Invoke-PcvOperation`, add this branch before the final not-implemented response:

```powershell
if ($operation -eq 'vm.list') {
    return New-PcvResponse -Ok $true -Operation $operation -Data (Get-PcvVmList) -ErrorObject $null
}
```

Add these functions to the `Export-ModuleMember` list:

```powershell
Convert-PcvBytesToMiB,
Convert-PcvBytesToGiB,
Get-PcvVmState,
Test-PcvManagedVm,
Get-PcvVmList
```

- [ ] **Step 4: Run inventory, host, and contract tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Inventory.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1 spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Inventory.Tests.ps1
git commit -m "spike: map Hyper-V VM inventory"
```

---

### Task 4: ISO VM Provisioning

**Files:**
- Modify: `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`
- Modify: `spikes/purecvisor-desktop-node/hyperv/examples/vm-create.json`
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Provisioning.Tests.ps1`

- [ ] **Step 1: Write the failing provisioning tests**

Create `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Provisioning.Tests.ps1`:

```powershell
$Root = Split-Path -Parent $PSScriptRoot
$ModulePath = Join-Path $Root 'PcvHyperV.psm1'

Describe 'New-PcvVmFromIso' {
    BeforeEach {
        Import-Module $ModulePath -Force
        Mock Get-PcvHostStatus {
            [ordered]@{
                supported = $true
                reasons = @()
                hyperv = [ordered]@{
                    feature_enabled = $true
                    vmms_running = $true
                    default_switch_present = $true
                }
                admin = [ordered]@{ elevated = $true }
                windows = [ordered]@{ edition = 'Pro'; caption = 'Windows 11 Pro'; version = '23H2' }
            }
        } -ModuleName PcvHyperV
        Mock Test-Path {
            param([string]$LiteralPath, [string]$PathType)
            return ($LiteralPath -like '*.iso')
        } -ModuleName PcvHyperV
        Mock Get-VM { throw 'not found' } -ModuleName PcvHyperV
        Mock New-Item {
            [pscustomobject]@{ FullName = $Path }
        } -ModuleName PcvHyperV
        Mock New-VHD {
            [pscustomobject]@{ Path = $Path; SizeBytes = $SizeBytes }
        } -ModuleName PcvHyperV
        Mock New-VM {
            [pscustomobject]@{ Name = $Name }
        } -ModuleName PcvHyperV
        Mock Set-VM {} -ModuleName PcvHyperV
        Mock Set-VMProcessor {} -ModuleName PcvHyperV
        Mock Add-VMDvdDrive {} -ModuleName PcvHyperV
        Mock Set-VMDvdDrive {} -ModuleName PcvHyperV
        Mock Connect-VMNetworkAdapter {} -ModuleName PcvHyperV
        Mock Set-VMFirmware {} -ModuleName PcvHyperV
    }

    It 'rejects invalid VM names before calling Hyper-V cmdlets' {
        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'bad name!'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_VM_NAME_INVALID'
        Should -Invoke New-VM -Times 0 -ModuleName PcvHyperV
    }

    It 'creates a Generation 2 VM from an ISO using Default Switch' {
        $result = New-PcvVmFromIso -Params ([pscustomobject]@{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        })

        $result.ok | Should -BeTrue
        $result.data.name | Should -Be 'ubuntu-lab-01'
        $result.data.steps | Should -Contain 'Create VHDX'
        $result.data.steps | Should -Contain 'Create Hyper-V VM'
        $result.data.steps | Should -Contain 'Attach Default Switch'
        Should -Invoke New-VHD -Times 1 -ModuleName PcvHyperV
        Should -Invoke New-VM -Times 1 -ModuleName PcvHyperV
        Should -Invoke Add-VMDvdDrive -Times 1 -ModuleName PcvHyperV
        Should -Invoke Connect-VMNetworkAdapter -Times 1 -ModuleName PcvHyperV
    }
}
```

- [ ] **Step 2: Run the provisioning tests and verify they fail**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Provisioning.Tests.ps1' -Output Detailed"
```

Expected: FAIL because `New-PcvVmFromIso` does not exist.

- [ ] **Step 3: Add provisioning functions**

Append these functions to `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`:

```powershell
function Test-PcvVmName {
    param([Parameter(Mandatory)][string]$Name)
    return ($Name -match '^[A-Za-z0-9][A-Za-z0-9._-]{0,62}$')
}

function New-PcvFailureResult {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][string]$Detail,
        [Parameter(Mandatory)][bool]$Retryable
    )

    New-PcvResponse -Ok $false -Operation $Operation -Data $null -ErrorObject (
        New-PcvError -Code $Code -Message $Message -Detail $Detail -Retryable $Retryable
    )
}

function New-PcvVmFromIso {
    param([Parameter(Mandatory)]$Params)

    $operation = 'vm.create'
    $name = [string]$Params.name
    $isoPath = [string]$Params.iso_path
    $cpu = [int]$Params.cpu
    $memoryMb = [int]$Params.memory_mb
    $diskGb = [int]$Params.disk_gb
    $vmRoot = if ($Params.PSObject.Properties.Name.Contains('vm_root')) { [string]$Params.vm_root } else { 'D:\PureCVisor\VMs' }
    $generation = if ($Params.PSObject.Properties.Name.Contains('generation')) { [int]$Params.generation } else { 2 }

    if (-not (Test-PcvVmName -Name $name)) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_NAME_INVALID' -Message "VM name '$name' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
    }
    if (-not (Test-Path -LiteralPath $isoPath -PathType Leaf)) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_ISO_NOT_FOUND' -Message "ISO '$isoPath' was not found." -Detail 'Provide a local ISO file path visible to the Hyper-V host.' -Retryable $false
    }
    if ($cpu -lt 1 -or $cpu -gt 32) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_CPU_OUT_OF_RANGE' -Message "CPU count '$cpu' is outside the supported spike range." -Detail 'Use a CPU count from 1 through 32.' -Retryable $false
    }
    if ($memoryMb -lt 512 -or $memoryMb -gt 262144) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_MEMORY_OUT_OF_RANGE' -Message "Memory '$memoryMb' MB is outside the supported spike range." -Detail 'Use memory from 512 MB through 262144 MB.' -Retryable $false
    }
    if ($diskGb -lt 8 -or $diskGb -gt 4096) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_DISK_OUT_OF_RANGE' -Message "Disk '$diskGb' GB is outside the supported spike range." -Detail 'Use disk size from 8 GB through 4096 GB.' -Retryable $false
    }
    if ($generation -notin @(1, 2)) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_GENERATION_INVALID' -Message "Generation '$generation' is invalid." -Detail 'Use Hyper-V generation 1 or 2.' -Retryable $false
    }

    $hostStatus = Get-PcvHostStatus
    if (-not $hostStatus.supported) {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_HOST_NOT_READY' -Message 'The Hyper-V host is not ready for VM creation.' -Detail (($hostStatus.reasons -join ', ')) -Retryable $false
    }

    try {
        Get-VM -Name $name -ErrorAction Stop | Out-Null
        return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_ALREADY_EXISTS' -Message "VM '$name' already exists." -Detail 'Choose a different VM name or remove the existing Hyper-V VM.' -Retryable $false
    }
    catch {
    }

    $vmDir = Join-Path $vmRoot $name
    $vhdPath = Join-Path $vmDir 'disk0.vhdx'
    $steps = New-Object System.Collections.Generic.List[string]

    try {
        New-Item -ItemType Directory -Path $vmDir -Force | Out-Null
        $steps.Add('Create VM folder')

        New-VHD -Path $vhdPath -SizeBytes ([Int64]$diskGb * 1GB) -Dynamic | Out-Null
        $steps.Add('Create VHDX')

        New-VM -Name $name -Generation $generation -MemoryStartupBytes ([Int64]$memoryMb * 1MB) -VHDPath $vhdPath -Path $vmDir | Out-Null
        $steps.Add('Create Hyper-V VM')

        Set-VMProcessor -VMName $name -Count $cpu
        Set-VM -Name $name -Notes 'managed-by=purecvisor-desktop-node'
        $steps.Add('Set resources')

        Add-VMDvdDrive -VMName $name -Path $isoPath
        $steps.Add('Attach ISO')

        Connect-VMNetworkAdapter -VMName $name -SwitchName 'Default Switch'
        $steps.Add('Attach Default Switch')

        if ($generation -eq 2) {
            $dvd = Get-VMDvdDrive -VMName $name
            Set-VMFirmware -VMName $name -FirstBootDevice $dvd
            $steps.Add('Set boot order')
        }

        return New-PcvResponse -Ok $true -Operation $operation -Data ([ordered]@{
            name = $name
            vm_dir = $vmDir
            vhd_path = $vhdPath
            iso_path = $isoPath
            switch = 'Default Switch'
            generation = $generation
            steps = @($steps)
        }) -ErrorObject $null
    }
    catch {
        return New-PcvFailureResult -Operation $operation -Code 'PCV_VM_CREATE_FAILED' -Message "VM '$name' creation failed." -Detail $_.Exception.Message -Retryable $true
    }
}
```

In `Invoke-PcvOperation`, add this branch:

```powershell
if ($operation -eq 'vm.create') {
    return New-PcvVmFromIso -Params $Request.params
}
```

Add these functions to the `Export-ModuleMember` list:

```powershell
Test-PcvVmName,
New-PcvFailureResult,
New-PcvVmFromIso
```

- [ ] **Step 4: Create the VM create example**

Create `spikes/purecvisor-desktop-node/hyperv/examples/vm-create.json`:

```json
{
  "operation": "vm.create",
  "params": {
    "name": "ubuntu-lab-01",
    "iso_path": "D:\\iso\\ubuntu-24.04-live-server-amd64.iso",
    "cpu": 2,
    "memory_mb": 4096,
    "disk_gb": 40,
    "vm_root": "D:\\PureCVisor\\VMs",
    "generation": 2
  }
}
```

- [ ] **Step 5: Run provisioning and regression tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Provisioning.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Inventory.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1 spikes/purecvisor-desktop-node/hyperv/examples/vm-create.json spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Provisioning.Tests.ps1
git commit -m "spike: create Hyper-V Linux VM from ISO"
```

---

### Task 5: Lifecycle And Checkpoint Operations

**Files:**
- Modify: `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`
- Create: `spikes/purecvisor-desktop-node/hyperv/examples/vm-start.json`
- Create: `spikes/purecvisor-desktop-node/hyperv/examples/checkpoint-create.json`
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.LifecycleCheckpoint.Tests.ps1`

- [ ] **Step 1: Write the failing lifecycle and checkpoint tests**

Create `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.LifecycleCheckpoint.Tests.ps1`:

```powershell
$Root = Split-Path -Parent $PSScriptRoot
$ModulePath = Join-Path $Root 'PcvHyperV.psm1'

Describe 'VM lifecycle and checkpoints' {
    BeforeEach {
        Import-Module $ModulePath -Force
        Mock Get-VM {
            [pscustomobject]@{ Name = $Name; State = 'Off' }
        } -ModuleName PcvHyperV
        Mock Start-VM {} -ModuleName PcvHyperV
        Mock Stop-VM {} -ModuleName PcvHyperV
        Mock Restart-VM {} -ModuleName PcvHyperV
        Mock Checkpoint-VM {
            [pscustomobject]@{ Name = $SnapshotName; VMName = $Name }
        } -ModuleName PcvHyperV
        Mock Get-VMSnapshot {
            @(
                [pscustomobject]@{ Name = 'before-upgrade'; VMName = $VMName; CreationTime = [datetime]'2026-04-24T00:00:00Z' }
            )
        } -ModuleName PcvHyperV
        Mock Restore-VMSnapshot {} -ModuleName PcvHyperV
        Mock Remove-VMSnapshot {} -ModuleName PcvHyperV
    }

    It 'starts a VM' {
        $result = Invoke-PcvVmLifecycle -Operation 'vm.start' -Params ([pscustomobject]@{ name = 'ubuntu-lab-01' })

        $result.ok | Should -BeTrue
        $result.data.action | Should -Be 'start'
        Should -Invoke Start-VM -Times 1 -ModuleName PcvHyperV
    }

    It 'performs a graceful shutdown' {
        $result = Invoke-PcvVmLifecycle -Operation 'vm.shutdown' -Params ([pscustomobject]@{ name = 'ubuntu-lab-01' })

        $result.ok | Should -BeTrue
        $result.data.action | Should -Be 'shutdown'
        Should -Invoke Stop-VM -ParameterFilter { $Shutdown -eq $true } -Times 1 -ModuleName PcvHyperV
    }

    It 'creates a named checkpoint' {
        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.create' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
            checkpoint_name = 'before-upgrade'
        })

        $result.ok | Should -BeTrue
        $result.data.name | Should -Be 'before-upgrade'
        Should -Invoke Checkpoint-VM -Times 1 -ModuleName PcvHyperV
    }

    It 'lists checkpoints' {
        $result = Invoke-PcvCheckpointOperation -Operation 'checkpoint.list' -Params ([pscustomobject]@{
            vm_name = 'ubuntu-lab-01'
        })

        $result.ok | Should -BeTrue
        $result.data[0].name | Should -Be 'before-upgrade'
    }
}
```

- [ ] **Step 2: Run the lifecycle tests and verify they fail**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.LifecycleCheckpoint.Tests.ps1' -Output Detailed"
```

Expected: FAIL because lifecycle and checkpoint functions do not exist.

- [ ] **Step 3: Add lifecycle and checkpoint functions**

Append these functions to `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`:

```powershell
function Get-PcvRequiredVm {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Operation
    )

    try {
        return Get-VM -Name $Name -ErrorAction Stop
    }
    catch {
        throw [System.InvalidOperationException]::new("PCV_VM_NOT_FOUND|VM '$Name' was not found.|Hyper-V Get-VM did not return a VM with this name.")
    }
}

function Invoke-PcvVmLifecycle {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)]$Params
    )

    $name = [string]$Params.name
    if (-not (Test-PcvVmName -Name $name)) {
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_VM_NAME_INVALID' -Message "VM name '$name' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
    }

    try {
        Get-PcvRequiredVm -Name $name -Operation $Operation | Out-Null

        switch ($Operation) {
            'vm.start' {
                Start-VM -Name $name
                $action = 'start'
            }
            'vm.shutdown' {
                Stop-VM -Name $name -Shutdown
                $action = 'shutdown'
            }
            'vm.poweroff' {
                Stop-VM -Name $name -TurnOff -Force
                $action = 'poweroff'
            }
            'vm.restart' {
                Restart-VM -Name $name -Force
                $action = 'restart'
            }
            default {
                return New-PcvFailureResult -Operation $Operation -Code 'PCV_OPERATION_NOT_ALLOWED' -Message "Operation '$Operation' is not a lifecycle operation." -Detail 'Use vm.start, vm.shutdown, vm.poweroff, or vm.restart.' -Retryable $false
            }
        }

        return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{
            name = $name
            action = $action
        }) -ErrorObject $null
    }
    catch {
        $parts = $_.Exception.Message -split '\|', 3
        if ($parts.Count -eq 3 -and $parts[0] -eq 'PCV_VM_NOT_FOUND') {
            return New-PcvFailureResult -Operation $Operation -Code $parts[0] -Message $parts[1] -Detail $parts[2] -Retryable $false
        }
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_LIFECYCLE_FAILED' -Message "Lifecycle operation '$Operation' failed for VM '$name'." -Detail $_.Exception.Message -Retryable $true
    }
}

function Invoke-PcvCheckpointOperation {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)]$Params
    )

    $vmName = [string]$Params.vm_name
    if (-not (Test-PcvVmName -Name $vmName)) {
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_VM_NAME_INVALID' -Message "VM name '$vmName' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
    }

    try {
        Get-PcvRequiredVm -Name $vmName -Operation $Operation | Out-Null

        switch ($Operation) {
            'checkpoint.list' {
                $snapshots = @(Get-VMSnapshot -VMName $vmName -ErrorAction SilentlyContinue)
                $data = @($snapshots | ForEach-Object {
                    [ordered]@{
                        name = $_.Name
                        vm_name = $vmName
                        created_at = if ($_.PSObject.Properties.Name.Contains('CreationTime')) { $_.CreationTime.ToString('o') } else { $null }
                    }
                })
                return New-PcvResponse -Ok $true -Operation $Operation -Data $data -ErrorObject $null
            }
            'checkpoint.create' {
                $checkpointName = [string]$Params.checkpoint_name
                if (-not (Test-PcvVmName -Name $checkpointName)) {
                    return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_NAME_INVALID' -Message "Checkpoint name '$checkpointName' is invalid." -Detail 'Use 1-63 characters: letters, numbers, dot, underscore, or hyphen. The first character must be alphanumeric.' -Retryable $false
                }
                Checkpoint-VM -Name $vmName -SnapshotName $checkpointName | Out-Null
                return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{ vm_name = $vmName; name = $checkpointName }) -ErrorObject $null
            }
            'checkpoint.restore' {
                $checkpointName = [string]$Params.checkpoint_name
                Restore-VMSnapshot -VMName $vmName -Name $checkpointName -Confirm:$false
                return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{ vm_name = $vmName; name = $checkpointName; action = 'restore' }) -ErrorObject $null
            }
            'checkpoint.delete' {
                $checkpointName = [string]$Params.checkpoint_name
                Remove-VMSnapshot -VMName $vmName -Name $checkpointName -Confirm:$false
                return New-PcvResponse -Ok $true -Operation $Operation -Data ([ordered]@{ vm_name = $vmName; name = $checkpointName; action = 'delete' }) -ErrorObject $null
            }
            default {
                return New-PcvFailureResult -Operation $Operation -Code 'PCV_OPERATION_NOT_ALLOWED' -Message "Operation '$Operation' is not a checkpoint operation." -Detail 'Use checkpoint.list, checkpoint.create, checkpoint.restore, or checkpoint.delete.' -Retryable $false
            }
        }
    }
    catch {
        $parts = $_.Exception.Message -split '\|', 3
        if ($parts.Count -eq 3 -and $parts[0] -eq 'PCV_VM_NOT_FOUND') {
            return New-PcvFailureResult -Operation $Operation -Code $parts[0] -Message $parts[1] -Detail $parts[2] -Retryable $false
        }
        return New-PcvFailureResult -Operation $Operation -Code 'PCV_CHECKPOINT_FAILED' -Message "Checkpoint operation '$Operation' failed for VM '$vmName'." -Detail $_.Exception.Message -Retryable $true
    }
}
```

In `Invoke-PcvOperation`, add these branches:

```powershell
if ($operation -in @('vm.start', 'vm.shutdown', 'vm.poweroff', 'vm.restart')) {
    return Invoke-PcvVmLifecycle -Operation $operation -Params $Request.params
}

if ($operation -in @('checkpoint.list', 'checkpoint.create', 'checkpoint.restore', 'checkpoint.delete')) {
    return Invoke-PcvCheckpointOperation -Operation $operation -Params $Request.params
}
```

Add these functions to the `Export-ModuleMember` list:

```powershell
Get-PcvRequiredVm,
Invoke-PcvVmLifecycle,
Invoke-PcvCheckpointOperation
```

- [ ] **Step 4: Create lifecycle and checkpoint example payloads**

Create `spikes/purecvisor-desktop-node/hyperv/examples/vm-start.json`:

```json
{
  "operation": "vm.start",
  "params": {
    "name": "ubuntu-lab-01"
  }
}
```

Create `spikes/purecvisor-desktop-node/hyperv/examples/checkpoint-create.json`:

```json
{
  "operation": "checkpoint.create",
  "params": {
    "vm_name": "ubuntu-lab-01",
    "checkpoint_name": "before-upgrade"
  }
}
```

- [ ] **Step 5: Run lifecycle and regression tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.LifecycleCheckpoint.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Provisioning.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Inventory.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.HostStatus.Tests.ps1','spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Contract.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1 spikes/purecvisor-desktop-node/hyperv/examples/vm-start.json spikes/purecvisor-desktop-node/hyperv/examples/checkpoint-create.json spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.LifecycleCheckpoint.Tests.ps1
git commit -m "spike: add VM lifecycle and checkpoints"
```

---

### Task 6: Gated Hyper-V Integration Test And Spike README

**Files:**
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1`
- Create: `spikes/purecvisor-desktop-node/hyperv/README.md`

- [ ] **Step 1: Create the gated integration test**

Create `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1`:

```powershell
$Root = Split-Path -Parent $PSScriptRoot
$Runner = Join-Path $Root 'Invoke-PcvHyperV.ps1'

Describe 'Hyper-V integration spike' -Tag Integration {
    BeforeAll {
        if ($env:PCV_HYPERV_INTEGRATION -ne '1') {
            Set-ItResult -Skipped -Because 'Set PCV_HYPERV_INTEGRATION=1 on a Windows 10/11 Pro/Enterprise Hyper-V host to run this suite.'
            return
        }

        $script:VmName = "pcv-spike-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
        $script:Root = Join-Path $env:TEMP 'pcv-hyperv-spike'
        $script:IsoPath = $env:PCV_HYPERV_TEST_ISO
        if ([string]::IsNullOrWhiteSpace($script:IsoPath)) {
            throw 'Set PCV_HYPERV_TEST_ISO to a local Linux ISO path before running integration tests.'
        }
    }

    AfterAll {
        if ($env:PCV_HYPERV_INTEGRATION -eq '1' -and $script:VmName) {
            try {
                Stop-VM -Name $script:VmName -TurnOff -Force -ErrorAction SilentlyContinue
                Remove-VM -Name $script:VmName -Force -ErrorAction SilentlyContinue
            }
            catch {
            }
        }
    }

    It 'reports host.status on a real Hyper-V host' {
        $payload = @{ operation = 'host.status'; params = @{} } | ConvertTo-Json -Depth 8
        $json = $payload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json

        $json.ok | Should -BeTrue
        $json.data.hyperv.feature_enabled | Should -BeTrue
        $json.data.hyperv.vmms_running | Should -BeTrue
    }

    It 'creates, lists, starts, checkpoints, powers off, and lists a spike VM' {
        $createPayload = @{
            operation = 'vm.create'
            params = @{
                name = $script:VmName
                iso_path = $script:IsoPath
                cpu = 1
                memory_mb = 1024
                disk_gb = 8
                vm_root = $script:Root
                generation = 2
            }
        } | ConvertTo-Json -Depth 8

        $created = $createPayload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json
        $created.ok | Should -BeTrue
        $created.data.name | Should -Be $script:VmName

        $listPayload = @{ operation = 'vm.list'; params = @{} } | ConvertTo-Json -Depth 8
        $listed = $listPayload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json
        @($listed.data | Where-Object { $_.name -eq $script:VmName }).Count | Should -Be 1

        $startPayload = @{ operation = 'vm.start'; params = @{ name = $script:VmName } } | ConvertTo-Json -Depth 8
        $started = $startPayload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json
        $started.ok | Should -BeTrue

        $checkpointPayload = @{ operation = 'checkpoint.create'; params = @{ vm_name = $script:VmName; checkpoint_name = 'before-install' } } | ConvertTo-Json -Depth 8
        $checkpoint = $checkpointPayload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json
        $checkpoint.ok | Should -BeTrue

        $poweroffPayload = @{ operation = 'vm.poweroff'; params = @{ name = $script:VmName } } | ConvertTo-Json -Depth 8
        $poweroff = $poweroffPayload | & pwsh -NoProfile -ExecutionPolicy Bypass -File $Runner | ConvertFrom-Json
        $poweroff.ok | Should -BeTrue
    }
}
```

- [ ] **Step 2: Create the README**

Create `spikes/purecvisor-desktop-node/hyperv/README.md`:

```markdown
# PureCVisor Desktop Node Hyper-V Spike

This spike validates the Phase 1 PowerShell helper contract for the Windows Hyper-V backend.

## Supported Host

- Windows 10 or 11 Pro, Enterprise, or Education
- Hyper-V enabled
- PowerShell 7 available as `pwsh`
- Administrator shell for real Hyper-V operations

## Contract Test

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

Expected: all non-integration tests pass without creating real VMs.

## Manual Host Status

Run:

```powershell
Get-Content spikes/purecvisor-desktop-node/hyperv/examples/host-status.json -Raw |
  pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/hyperv/Invoke-PcvHyperV.ps1
```

Expected: one compact JSON object with `ok=true` and a `data.hyperv` object.

## Gated Hyper-V Integration Test

Run from an elevated PowerShell session:

```powershell
$env:PCV_HYPERV_INTEGRATION='1'
$env:PCV_HYPERV_TEST_ISO='D:\iso\ubuntu-24.04-live-server-amd64.iso'
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1' -Tag Integration -Output Detailed"
```

Expected: a temporary `pcv-spike-*` VM is created, listed, started, checkpointed, powered off, and removed by cleanup.
```

- [ ] **Step 3: Run non-integration tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

Expected: PASS for all non-integration tests.

- [ ] **Step 4: Run integration tests on a Hyper-V host**

Run from an elevated PowerShell session with a local Linux ISO path:

```powershell
$env:PCV_HYPERV_INTEGRATION='1'
$env:PCV_HYPERV_TEST_ISO='D:\iso\ubuntu-24.04-live-server-amd64.iso'
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1' -Tag Integration -Output Detailed"
```

Expected: PASS. If the host does not have Hyper-V enabled or no ISO path is provided, the suite fails before creating a VM and prints the missing prerequisite.

- [ ] **Step 5: Commit**

```powershell
git add spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1 spikes/purecvisor-desktop-node/hyperv/README.md
git commit -m "spike: document and gate Hyper-V integration tests"
```

---

## Completion Verification

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git status --short
```

Expected:

```text
Tests Passed
```

and `git status --short` prints no changed files after the final commit.

On a real Windows Hyper-V host, also run:

```powershell
$env:PCV_HYPERV_INTEGRATION='1'
$env:PCV_HYPERV_TEST_ISO='D:\iso\ubuntu-24.04-live-server-amd64.iso'
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.Integration.Tests.ps1' -Tag Integration -Output Detailed"
```

Expected: the temporary VM is cleaned up even if a test fails.

## Self-Review Notes

Spec coverage:

- Hyper-V required host diagnostics: Task 2.
- PowerShell JSON helper contract: Task 1.
- VM inventory: Task 3.
- ISO VM creation with VHDX and Default Switch: Task 4.
- Lifecycle and checkpoint operations: Task 5.
- Gated real Hyper-V verification: Task 6.

This plan does not implement API daemon, Web Console, CLI, LAN mode, auth, or Windows service because the approved spec places those after the Phase 1 helper spike.
