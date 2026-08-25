# PureCVisor Desktop Node Phase 3B VM Detail + Lifecycle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add VM detail and lifecycle job actions to the Desktop Node Local API and static Web Console.

**Architecture:** Phase 3B keeps the existing spike boundaries. The Local API adds VM detail and lifecycle routes while reusing the existing `vm.list` helper result, job store, worker tick, bearer-token gate, and static serving. The Web Console adds a selected-VM detail drawer and queues lifecycle jobs through the new API routes without adding a framework or new static route.

**Tech Stack:** PowerShell 7, Pester 5, vanilla HTML/CSS/JavaScript, existing Desktop Node Hyper-V helper JSON contract.

---

## Source Spec

- `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle-design.md`

## File Map

- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - Add VM route id decoding.
  - Add VM list data normalization.
  - Add `GET /api/v1/vms/{id}`.
  - Add lifecycle job creation for `POST /api/v1/vms/{id}/start|shutdown|poweroff|restart`.
  - Keep auth gate before route handling.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
  - Add VM detail route tests and lifecycle unsupported route test.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1`
  - Add lifecycle job creation tests.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Worker.Tests.ps1`
  - Add lifecycle worker dispatch test.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1`
  - Add bearer-token coverage for VM detail and lifecycle routes.
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`
  - Add static contract tests for detail mount point, route strings, lifecycle actions, destructive confirmation, and syntax.
- Modify: `spikes/purecvisor-desktop-node/web/index.html`
  - Add VM detail section mount point.
- Modify: `spikes/purecvisor-desktop-node/web/app.js`
  - Add selected VM state, VM detail loading, table row selection, lifecycle job requests, destructive confirmations, and detail rendering.
- Modify: `spikes/purecvisor-desktop-node/web/styles.css`
  - Add desktop drawer and mobile panel styling.
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
  - Document Phase 3B routes, UI behavior, and verification counts.
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
  - Mark Phase 3B Web Console lifecycle/detail as complete after implementation.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Add Phase 3B plan entry after implementation.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Keep Desktop Node Web Console/API verification instructions current.
- Modify: `README.md`, `AGENTS.md`, `ui/guide-content.md`
  - Refresh Desktop Node Phase 3B references after implementation.

## Task 1: Add API Contract Tests

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Worker.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1`

- [ ] **Step 1: Add VM detail helper fixtures to contract tests**

In `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`, replace the `BeforeEach` helper response with this version so `vm.list` can return real VM objects:

```powershell
    BeforeEach {
        $script:HelperCalls = @()
        $script:VmListData = @(
            [ordered]@{
                id = 'ubuntu-lab-01'
                name = 'ubuntu-lab-01'
                state = 'running'
                cpu = [ordered]@{ count = 2 }
                memory = [ordered]@{ startup_mb = 4096; assigned_mb = 2048 }
                generation = 2
                storage = @([ordered]@{ path = 'D:\PureCVisor\VMs\ubuntu-lab-01\disk.vhdx'; size_gb = 40; attached = $true })
                network = @([ordered]@{ name = 'Network Adapter'; switch = 'Default Switch' })
                checkpoints = [ordered]@{ count = 1 }
                console = [ordered]@{ mode = 'vmconnect'; available = $true }
                managed_by_purecvisor = $true
            },
            [ordered]@{
                id = 'debian-lab-02'
                name = 'debian-lab-02'
                state = 'stopped'
                cpu = [ordered]@{ count = 1 }
                memory = [ordered]@{ startup_mb = 2048; assigned_mb = 0 }
                generation = 2
                storage = @()
                network = @()
                checkpoints = [ordered]@{ count = 0 }
                console = [ordered]@{ mode = 'vmconnect'; available = $true }
                managed_by_purecvisor = $true
            }
        )
        $script:Helper = {
            param(
                [string]$Operation,
                [AllowNull()]$Params,
                [string]$HelperScriptPath,
                [int]$TimeoutSec
            )

            $script:HelperCalls += [pscustomobject]@{
                operation = $Operation
                params = $Params
                helper_script_path = $HelperScriptPath
                timeout_sec = $TimeoutSec
            }

            $data = [ordered]@{ marker = $Operation }
            if ($Operation -eq 'vm.list') {
                $data = $script:VmListData
            }

            [ordered]@{
                ok = $true
                operation = $Operation
                data = $data
                error = $null
            }
        }
    }
```

- [ ] **Step 2: Add VM detail contract tests**

Append these `It` blocks after the existing `routes GET /api/v1/vms to vm.list helper operation` test:

```powershell
    It 'routes GET /api/v1/vms/{id} through vm.list and returns the matching VM detail' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/ubuntu-lab-01' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.get'
        $json.data.id | Should -Be 'ubuntu-lab-01'
        $json.data.name | Should -Be 'ubuntu-lab-01'
        $json.data.state | Should -Be 'running'
        $json.data.cpu.count | Should -Be 2
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'vm.list'
    }

    It 'decodes VM detail route ids before matching inventory' {
        $script:VmListData = @(
            [ordered]@{
                id = 'lab.vm_01'
                name = 'lab.vm_01'
                state = 'stopped'
            }
        )

        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/lab.vm_01' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'vm.get'
        $json.data.name | Should -Be 'lab.vm_01'
    }

    It 'returns PCV_VM_NOT_FOUND when VM detail is missing from inventory' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/missing-vm' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 404
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'vm.get'
        $json.error.code | Should -Be 'PCV_VM_NOT_FOUND'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'vm.list'
    }

    It 'keeps unknown VM sub-routes as route not found' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/ubuntu-lab-01/metrics' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 404
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'api.route'
        $json.error.code | Should -Be 'PCV_ROUTE_NOT_FOUND'
        $script:HelperCalls.Count | Should -Be 0
    }
```

- [ ] **Step 3: Update unsupported route expectation**

In the existing `returns 404 for unsupported routes without calling the helper` test, change the path from `/api/v1/vms/demo` to `/api/v1/vms/demo/unknown` because `/api/v1/vms/{id}` will become supported:

```powershell
            -Path '/api/v1/vms/demo/unknown' `
```

- [ ] **Step 4: Add lifecycle job creation tests**

Append these tests to `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1`:

```powershell
    It 'queues lifecycle jobs without calling the helper during the request' {
        $cases = @(
            @{ path = '/api/v1/vms/ubuntu-lab-01/start'; operation = 'vm.start' },
            @{ path = '/api/v1/vms/ubuntu-lab-01/shutdown'; operation = 'vm.shutdown' },
            @{ path = '/api/v1/vms/ubuntu-lab-01/poweroff'; operation = 'vm.poweroff' },
            @{ path = '/api/v1/vms/ubuntu-lab-01/restart'; operation = 'vm.restart' }
        )

        foreach ($case in $cases) {
            Clear-PcvApiJobStore
            $script:HelperCalls = @()

            $response = Invoke-PcvApiRequest `
                -Method 'POST' `
                -Path $case.path `
                -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
                -InvokeHelper $script:Helper

            $response.status | Should -Be 202
            $json = $response.body | ConvertFrom-Json
            $json.ok | Should -BeTrue
            $json.operation | Should -Be 'job.create'
            $json.data.operation | Should -Be $case.operation
            $json.data.status | Should -Be 'queued'
            $json.data.params.name | Should -Be 'ubuntu-lab-01'
            $script:HelperCalls.Count | Should -Be 0
        }
    }

    It 'decodes lifecycle route ids before storing job params' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/lab.vm_01/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.data.operation | Should -Be 'vm.start'
        $json.data.params.name | Should -Be 'lab.vm_01'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'rejects unsupported lifecycle actions without creating a job' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/suspend' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 404
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'api.route'
        $json.error.code | Should -Be 'PCV_ROUTE_NOT_FOUND'
        $script:HelperCalls.Count | Should -Be 0
    }
```

- [ ] **Step 5: Add lifecycle worker dispatch test**

Append this test to `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Worker.Tests.ps1`:

```powershell
    It 'dispatches queued lifecycle jobs to the Hyper-V helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $jobId = ($response.body | ConvertFrom-Json).data.job_id

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $tick.processed | Should -BeTrue
        $tick.job.job_id | Should -Be $jobId
        $tick.job.status | Should -Be 'succeeded'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'vm.start'
        $script:HelperCalls[0].params.name | Should -Be 'ubuntu-lab-01'
    }
```

- [ ] **Step 6: Add auth tests for detail and lifecycle routes**

Append these tests to `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1`:

```powershell
    It 'requires bearer token for VM detail routes when ApiToken is configured' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/ubuntu-lab-01' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret'

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'api.auth'
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'requires bearer token for lifecycle job routes when ApiToken is configured' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret'

        $response.status | Should -Be 401
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'api.auth'
        $json.error.code | Should -Be 'PCV_AUTH_REQUIRED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'allows lifecycle job routes when bearer token matches' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -ApiToken 'secret' `
            -Headers @{ Authorization = 'Bearer secret' }

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'job.create'
        $json.data.operation | Should -Be 'vm.start'
        $script:HelperCalls.Count | Should -Be 0
    }
```

- [ ] **Step 7: Run API tests to verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected: FAIL. The new tests should fail because `/api/v1/vms/{id}` and lifecycle routes are not implemented yet.

- [ ] **Step 8: Commit RED tests**

Run:

```powershell
git add spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1 spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1 spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Worker.Tests.ps1 spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1
git commit -m "test: cover Desktop Node VM detail lifecycle API"
```

## Task 2: Implement API VM Detail and Lifecycle Jobs

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [ ] **Step 1: Add route helper functions**

Insert these functions after `ConvertFrom-PcvApiRequestJson`:

```powershell
function ConvertFrom-PcvApiRouteId {
    param(
        [Parameter(Mandatory)][string]$EncodedValue,
        [Parameter(Mandatory)][string]$Operation
    )

    try {
        $decoded = [System.Uri]::UnescapeDataString($EncodedValue)
    }
    catch {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 400 `
                -Operation $Operation `
                -Code 'PCV_ROUTE_ID_INVALID' `
                -Message 'The route id could not be decoded.' `
                -Detail $_.Exception.Message `
                -Retryable $false)
        }
    }

    if ([string]::IsNullOrWhiteSpace($decoded)) {
        return [ordered]@{
            ok = $false
            response = (New-PcvApiFailureResponse `
                -Status 400 `
                -Operation $Operation `
                -Code 'PCV_ROUTE_ID_INVALID' `
                -Message 'The route id is required.' `
                -Detail 'Pass a non-empty VM id or VM name in the route path.' `
                -Retryable $false)
        }
    }

    [ordered]@{
        ok = $true
        value = $decoded
    }
}

function ConvertTo-PcvVmInventoryList {
    param([AllowNull()]$Data)

    if ($null -eq $Data) {
        return @()
    }

    if ($Data -is [System.Array]) {
        return @($Data)
    }

    if ($Data -is [System.Collections.IEnumerable] -and -not ($Data -is [string]) -and -not ($Data -is [System.Collections.IDictionary])) {
        return @($Data)
    }

    $propertyNames = @()
    if ($null -ne $Data.PSObject) {
        $propertyNames = @($Data.PSObject.Properties.Name)
    }

    foreach ($propertyName in @('vms', 'items', 'data')) {
        if ($propertyNames -contains $propertyName -and $null -ne $Data.$propertyName) {
            return ConvertTo-PcvVmInventoryList -Data $Data.$propertyName
        }
    }

    @($Data)
}

function Find-PcvVmInInventoryData {
    param(
        [AllowNull()]$Data,
        [Parameter(Mandatory)][string]$VmId
    )

    $vms = ConvertTo-PcvVmInventoryList -Data $Data
    foreach ($vm in $vms) {
        if ($null -eq $vm) {
            continue
        }

        $candidateIds = @()
        if ($vm.PSObject.Properties.Name -contains 'id' -and $null -ne $vm.id) {
            $candidateIds += [string]$vm.id
        }
        if ($vm.PSObject.Properties.Name -contains 'name' -and $null -ne $vm.name) {
            $candidateIds += [string]$vm.name
        }

        foreach ($candidateId in $candidateIds) {
            if ([string]::Equals($candidateId, $VmId, [System.StringComparison]::OrdinalIgnoreCase)) {
                return $vm
            }
        }
    }

    $null
}

function New-PcvApiJobCreateResponse {
    param(
        [Parameter(Mandatory)]$Job,
        [int]$Status = 202
    )

    New-PcvApiResponse `
        -Status $Status `
        -Body (New-PcvApiBody `
            -Ok $true `
            -Operation 'job.create' `
            -Data (Convert-PcvJobToApiData -Job $Job) `
            -ErrorObject $null)
}
```

- [ ] **Step 2: Replace duplicate VM create response with helper**

In the existing `POST /api/v1/vms` route, replace the final `return New-PcvApiResponse ...` block with:

```powershell
        return New-PcvApiJobCreateResponse -Job $job
```

- [ ] **Step 3: Add lifecycle POST route before job cancel/retry routes**

In `Invoke-PcvApiRequest`, insert this block after the `POST /api/v1/vms` route and before the `/api/v1/jobs/.../cancel` route:

```powershell
    if ($normalizedMethod -eq 'POST' -and $pathOnly -match '^/api/v1/vms/([^/]+)/(start|shutdown|poweroff|restart)$') {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'job.create'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        $lifecycleOperation = switch ($Matches[2]) {
            'start' { 'vm.start'; break }
            'shutdown' { 'vm.shutdown'; break }
            'poweroff' { 'vm.poweroff'; break }
            'restart' { 'vm.restart'; break }
        }

        $job = New-PcvApiJob `
            -Operation $lifecycleOperation `
            -Params ([ordered]@{ name = $routeId.value })
        Add-PcvApiJobToQueue -Job $job
        [void](Save-PcvApiJobStore -Path $JobStorePath)

        return New-PcvApiJobCreateResponse -Job $job
    }
```

- [ ] **Step 4: Add VM detail GET route before job GET route**

In `Invoke-PcvApiRequest`, insert this block after the `if ($normalizedMethod -eq 'POST')` not-found block and before the existing `GET /api/v1/jobs/{job_id}` route:

```powershell
    if ($pathOnly -match '^/api/v1/vms/([^/]+)$') {
        $routeId = ConvertFrom-PcvApiRouteId -EncodedValue $Matches[1] -Operation 'vm.get'
        if (-not $routeId.ok) {
            return $routeId.response
        }

        if ($null -eq $InvokeHelper) {
            $helperResult = Invoke-PcvHyperVHelper `
                -Operation 'vm.list' `
                -Params ([ordered]@{}) `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }
        else {
            $helperResult = & $InvokeHelper `
                -Operation 'vm.list' `
                -Params ([ordered]@{}) `
                -HelperScriptPath $HelperScriptPath `
                -TimeoutSec $TimeoutSec
        }

        if (-not $helperResult.ok) {
            return Convert-PcvHelperResultToApiResponse -HelperResult $helperResult
        }

        $vm = Find-PcvVmInInventoryData -Data $helperResult.data -VmId $routeId.value
        if ($null -eq $vm) {
            return New-PcvApiFailureResponse `
                -Status 404 `
                -Operation 'vm.get' `
                -Code 'PCV_VM_NOT_FOUND' `
                -Message "VM '$($routeId.value)' was not found." `
                -Detail 'The VM was not present in the current Hyper-V inventory response.' `
                -Retryable $false
        }

        return New-PcvApiResponse `
            -Status 200 `
            -Body (New-PcvApiBody `
                -Ok $true `
                -Operation 'vm.get' `
                -Data $vm `
                -ErrorObject $null)
    }
```

- [ ] **Step 5: Update route-not-found detail text**

In the `POST` route-not-found response, replace `-Message` and `-Detail` strings with:

```powershell
            -Message "No Phase 3B POST route matches '$Path'." `
            -Detail 'Available POST routes: POST /api/v1/vms, POST /api/v1/vms/{id}/start, POST /api/v1/vms/{id}/shutdown, POST /api/v1/vms/{id}/poweroff, POST /api/v1/vms/{id}/restart, POST /api/v1/jobs/{job_id}/cancel, POST /api/v1/jobs/{job_id}/retry.' `
```

In the `GET` route-not-found response, replace `-Message` and `-Detail` strings with:

```powershell
            -Message "No Phase 3B route matches '$Path'." `
            -Detail 'Available routes: GET /api/v1/host/status, GET /api/v1/vms, GET /api/v1/vms/{id}, GET /api/v1/jobs/{job_id}, POST /api/v1/vms, POST /api/v1/vms/{id}/start, POST /api/v1/vms/{id}/shutdown, POST /api/v1/vms/{id}/poweroff, POST /api/v1/vms/{id}/restart, POST /api/v1/jobs/{job_id}/cancel, POST /api/v1/jobs/{job_id}/retry.' `
```

- [ ] **Step 6: Run focused API tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Job.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Worker.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1' -Output Detailed"
```

Expected: PASS for all tests in those files.

- [ ] **Step 7: Run full API suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected: PASS. Test count increases from 46 to 59 if the tests above are added exactly.

- [ ] **Step 8: Commit API implementation**

Run:

```powershell
git add spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1
git commit -m "feat: add Desktop Node VM detail lifecycle API"
```

## Task 3: Add Web Static Tests

**Files:**
- Modify: `spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [ ] **Step 1: Add Web Console Phase 3B static contracts**

Append these tests before the existing JavaScript syntax validation test:

```powershell
    It 'declares the Phase 3B VM detail and lifecycle endpoints used by the console' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match '/api/v1/vms/'
        $app | Should -Match '/start'
        $app | Should -Match '/shutdown'
        $app | Should -Match '/poweroff'
        $app | Should -Match '/restart'
    }

    It 'ships a VM detail panel mount point' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw

        $index | Should -Match 'id="vm-detail-panel"'
        $index | Should -Match 'id="vm-detail-content"'
    }

    It 'declares lifecycle action handlers and destructive confirmation' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match 'data-action="vm-start"'
        $app | Should -Match 'data-action="vm-shutdown"'
        $app | Should -Match 'data-action="vm-poweroff"'
        $app | Should -Match 'data-action="vm-restart"'
        $app | Should -Match 'confirm\('
    }
```

- [ ] **Step 2: Run Web static tests to verify RED**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
```

Expected: FAIL because the detail panel mount point and lifecycle action strings do not exist yet.

- [ ] **Step 3: Commit RED Web tests**

Run:

```powershell
git add spikes/purecvisor-desktop-node/web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "test: cover Desktop Node web VM detail lifecycle"
```

## Task 4: Implement Web Console VM Detail and Lifecycle UI

**Files:**
- Modify: `spikes/purecvisor-desktop-node/web/index.html`
- Modify: `spikes/purecvisor-desktop-node/web/app.js`
- Modify: `spikes/purecvisor-desktop-node/web/styles.css`

- [ ] **Step 1: Add detail panel markup**

In `spikes/purecvisor-desktop-node/web/index.html`, replace the VM section with this version:

```html
      <section id="vms" class="section">
        <div class="section-header">
          <div>
            <p class="eyebrow">Inventory</p>
            <h2>Virtual Machines</h2>
          </div>
        </div>
        <div class="vm-workspace">
          <div id="vm-table" class="table-wrap"></div>
          <aside id="vm-detail-panel" class="vm-detail-panel" aria-live="polite">
            <div class="section-header">
              <div>
                <p class="eyebrow">Selected VM</p>
                <h2 id="vm-detail-title">No VM selected</h2>
              </div>
              <button id="close-vm-detail" type="button">Close</button>
            </div>
            <div id="vm-detail-content">
              <p class="muted">Select a VM row to inspect lifecycle controls and inventory details.</p>
            </div>
          </aside>
        </div>
      </section>
```

- [ ] **Step 2: Extend state and element bindings**

In `spikes/purecvisor-desktop-node/web/app.js`, replace the `state` object with:

```javascript
const state = {
  apiBaseUrl: window.location.origin,
  apiToken: '',
  host: null,
  vms: [],
  selectedVmId: '',
  selectedVm: null,
  trackedJobs: [],
  loading: false,
  actionPending: false,
  error: null,
  connectionState: 'idle',
  pollTimer: null
};
```

In `init()`, add these element bindings:

```javascript
    vmDetailPanel: byId('vm-detail-panel'),
    vmDetailTitle: byId('vm-detail-title'),
    vmDetailContent: byId('vm-detail-content'),
    closeVmDetail: byId('close-vm-detail'),
```

The `Object.assign(els, { ... })` block should include the new keys next to `vmTable`.

- [ ] **Step 3: Add value helpers**

Insert these helpers after `asArray()`:

```javascript
function getVmId(vm) {
  return String(vm?.id || vm?.name || '');
}

function getVmName(vm) {
  return String(vm?.name || vm?.id || 'Unknown VM');
}

function formatObjectValue(value) {
  if (value === null || value === undefined || value === '') return '-';
  if (typeof value === 'object') return JSON.stringify(value);
  return value;
}

function flattenNamedList(value, keys) {
  const items = asArray(value);
  if (items.length === 0) return '-';
  return items.map((item) => {
    if (!item || typeof item !== 'object') return String(item);
    return keys
      .map((key) => item[key])
      .filter((part) => part !== null && part !== undefined && part !== '')
      .join(' / ');
  }).filter(Boolean).join(', ') || '-';
}
```

- [ ] **Step 4: Replace `renderVms()` with selectable rows**

Replace the current `renderVms()` with:

```javascript
function renderVms() {
  const vms = asArray(state.vms);
  if (vms.length === 0) {
    els.vmTable.innerHTML = '<p class="muted">No VMs returned by the Desktop Node API.</p>';
    return;
  }
  const rows = vms.map((vm) => {
    const vmId = getVmId(vm);
    const selected = vmId && vmId === state.selectedVmId ? ' class="selected-row"' : '';
    return `
    <tr${selected} data-vm-id="${escapeHtml(vmId)}">
      <td><button type="button" class="link-button" data-action="select-vm" data-vm-id="${escapeHtml(vmId)}">${escapeHtml(getVmName(vm))}</button></td>
      <td>${stateBadge(vm.state || vm.status)}</td>
      <td>${escapeHtml(vm.cpu?.count || vm.cpu || vm.vcpu || vm.processor_count)}</td>
      <td>${escapeHtml(vm.memory?.startup_mb || vm.memory_mb || vm.memory || vm.memory_assigned_mb)}</td>
      <td>${escapeHtml(vm.generation)}</td>
      <td>${escapeHtml(vm.uptime || vm.updated_at || vm.created_at)}</td>
      <td>${escapeHtml(vm.error?.message || vm.notes || '-')}</td>
    </tr>`;
  }).join('');
  els.vmTable.innerHTML = `
    <table>
      <thead><tr><th>Name</th><th>State</th><th>CPU</th><th>Memory</th><th>Gen</th><th>Updated</th><th>Notes</th></tr></thead>
      <tbody>${rows}</tbody>
    </table>`;
}
```

- [ ] **Step 5: Add VM detail rendering**

Insert this function after `renderVms()`:

```javascript
function renderVmDetail() {
  const vm = state.selectedVm;
  if (!vm) {
    els.vmDetailTitle.textContent = 'No VM selected';
    els.vmDetailContent.innerHTML = '<p class="muted">Select a VM row to inspect lifecycle controls and inventory details.</p>';
    return;
  }

  els.vmDetailTitle.textContent = getVmName(vm);
  const actionDisabled = state.actionPending ? ' disabled' : '';
  const storage = flattenNamedList(vm.storage, ['path', 'size_gb', 'attached']);
  const network = flattenNamedList(vm.network, ['name', 'switch', 'mode']);
  const details = [
    ['State', vm.state || vm.status],
    ['ID', vm.id],
    ['CPU', vm.cpu?.count || vm.cpu || vm.vcpu || vm.processor_count],
    ['Startup Memory MB', vm.memory?.startup_mb || vm.memory_mb],
    ['Assigned Memory MB', vm.memory?.assigned_mb || vm.memory_assigned_mb],
    ['Generation', vm.generation],
    ['Storage', storage],
    ['Network', network],
    ['Checkpoints', vm.checkpoints?.count],
    ['Console', vm.console?.mode || vm.console?.available],
    ['Managed', vm.managed_by_purecvisor],
    ['Notes', vm.error?.message || vm.notes]
  ];

  els.vmDetailContent.innerHTML = `
    <div class="lifecycle-actions">
      <button data-action="vm-start" data-vm-id="${escapeHtml(getVmId(vm))}"${actionDisabled}>Start</button>
      <button data-action="vm-shutdown" data-vm-id="${escapeHtml(getVmId(vm))}"${actionDisabled}>Shutdown</button>
      <button class="danger-button" data-action="vm-poweroff" data-vm-id="${escapeHtml(getVmId(vm))}"${actionDisabled}>Power off</button>
      <button class="danger-button" data-action="vm-restart" data-vm-id="${escapeHtml(getVmId(vm))}"${actionDisabled}>Restart</button>
    </div>
    <div class="details-grid detail-grid">
      ${details.map(([label, value]) => `<div class="kv"><span>${escapeHtml(label)}</span><strong>${escapeHtml(formatObjectValue(value))}</strong></div>`).join('')}
    </div>`;
}
```

- [ ] **Step 6: Call detail renderer**

In `render()`, add `renderVmDetail();` after `renderVms();`:

```javascript
function render() {
  renderError();
  renderConnectionState();
  renderMetrics();
  renderHost();
  renderVms();
  renderVmDetail();
  renderJobs();
}
```

- [ ] **Step 7: Add VM detail and lifecycle API functions**

Insert these functions after `loadVms()`:

```javascript
function findCachedVm(vmId) {
  return asArray(state.vms).find((vm) => getVmId(vm) === vmId || getVmName(vm) === vmId) || null;
}

async function loadVmDetail(vmId) {
  state.selectedVmId = vmId;
  state.selectedVm = findCachedVm(vmId);
  render();
  state.selectedVm = await apiFetch(`/api/v1/vms/${encodeURIComponent(vmId)}`);
}

async function refreshSelectedVm() {
  if (!state.selectedVmId) return;
  try {
    state.selectedVm = await apiFetch(`/api/v1/vms/${encodeURIComponent(state.selectedVmId)}`);
  } catch (error) {
    state.error = normalizeError(error);
  }
}

async function queueVmLifecycle(vmId, action) {
  const destructive = action === 'poweroff' || action === 'restart';
  if (destructive && !window.confirm(`${action} ${vmId}?`)) {
    return;
  }
  state.actionPending = true;
  state.error = null;
  render();
  try {
    const job = await apiFetch(`/api/v1/vms/${encodeURIComponent(vmId)}/${action}`, { method: 'POST' });
    trackJob(job);
    state.connectionState = 'connected';
    startPolling();
  } catch (error) {
    state.error = normalizeError(error);
  } finally {
    state.actionPending = false;
    render();
  }
}
```

- [ ] **Step 8: Refresh selected VM after refresh and polling**

In `refreshAll()`, after `await Promise.all([loadHost(), loadVms(), pollTrackedJobs()]);`, add:

```javascript
    await refreshSelectedVm();
```

In `startPolling()`, after `await pollTrackedJobs();`, add:

```javascript
      await loadVms();
      await refreshSelectedVm();
```

- [ ] **Step 9: Bind VM table and detail actions**

In `bindEvents()`, add these event listeners before the `jobsPanel` listener:

```javascript
  els.vmTable.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action="select-vm"]');
    if (!button) return;
    state.error = null;
    try {
      await loadVmDetail(button.dataset.vmId);
      state.connectionState = 'connected';
    } catch (error) {
      state.error = normalizeError(error);
    }
    render();
  });
  els.closeVmDetail.addEventListener('click', () => {
    state.selectedVmId = '';
    state.selectedVm = null;
    render();
  });
  els.vmDetailPanel.addEventListener('click', async (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    const actionMap = {
      'vm-start': 'start',
      'vm-shutdown': 'shutdown',
      'vm-poweroff': 'poweroff',
      'vm-restart': 'restart'
    };
    const action = actionMap[button.dataset.action];
    if (!action) return;
    await queueVmLifecycle(button.dataset.vmId, action);
  });
```

- [ ] **Step 10: Add detail drawer styles**

Append these CSS rules before the `@media` block in `spikes/purecvisor-desktop-node/web/styles.css`:

```css
.vm-workspace {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(320px, 380px);
  gap: 14px;
  align-items: start;
}
.vm-detail-panel {
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 12px;
  background: var(--panel-soft);
  position: sticky;
  top: 14px;
}
.selected-row {
  background: #e8f5f3;
}
.link-button {
  border: 0;
  background: transparent;
  color: var(--accent);
  padding: 0;
  text-align: left;
  font-weight: 600;
}
.link-button:hover {
  text-decoration: underline;
}
.lifecycle-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
}
.danger-button {
  border-color: #f1b4ac;
  color: var(--danger);
}
.danger-button:hover {
  border-color: var(--danger);
}
.detail-grid {
  grid-template-columns: 1fr;
}
```

In the existing `@media (max-width: 900px)` block, add:

```css
  .vm-workspace { grid-template-columns: 1fr; }
  .vm-detail-panel { position: static; }
```

- [ ] **Step 11: Run Web static tests and syntax check**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

Expected: PASS. Web static test count increases from 6 to 9.

- [ ] **Step 12: Run API suite to catch integration drift**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected: PASS. API test count is 62 after Task 2.

- [ ] **Step 13: Commit Web implementation**

Run:

```powershell
git add spikes/purecvisor-desktop-node/web/index.html spikes/purecvisor-desktop-node/web/app.js spikes/purecvisor-desktop-node/web/styles.css
git commit -m "feat: add Desktop Node VM detail lifecycle UI"
```

## Task 5: Documentation Updates

**Files:**
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/GUIDE.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `README.md`
- Modify: `AGENTS.md`
- Modify: `ui/guide-content.md`

- [ ] **Step 1: Update API README route table**

In `spikes/purecvisor-desktop-node/api/README.md`, update the status paragraph to say Phase 3B adds VM detail and lifecycle job routes. Add these rows to the endpoint table:

```markdown
| `GET` | `/api/v1/vms/{id}` | VM detail from `vm.list` inventory |
| `POST` | `/api/v1/vms/{id}/start` | queued `vm.start` lifecycle job |
| `POST` | `/api/v1/vms/{id}/shutdown` | queued `vm.shutdown` lifecycle job |
| `POST` | `/api/v1/vms/{id}/poweroff` | queued `vm.poweroff` lifecycle job |
| `POST` | `/api/v1/vms/{id}/restart` | queued `vm.restart` lifecycle job |
```

Update the non-integration expected counts:

```markdown
Current expected result: 62 passed, 0 failed.
Current expected result: 9 passed, 0 failed; Node syntax check exits 0.
```

In the exclusions paragraph, remove `VM detail` and `lifecycle action UI`, but keep checkpoint UI, VMConnect launch, LAN binding, and persistent browser job history excluded.

- [ ] **Step 2: Update top-level Desktop Node docs**

Update the roadmap in `docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md` under Phase 3:

```markdown
Phase 3: Web Console MVP
- Phase 3A 완료: `spikes/purecvisor-desktop-node/web/`
- Phase 3B 완료: VM detail drawer와 lifecycle job actions
- host dashboard
- VM table
- create job form
- session job panel with cancel/retry controls
- optional bearer token request support
- checkpoint operations
- VMConnect action
```

Add the Phase 3B plan to `docs/DEVELOPER_INDEX.md` next to the Phase 3A plan entry.

Update `README.md`, `AGENTS.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, and `ui/guide-content.md` wherever they name the current Desktop Node Web Console scope so they include Phase 3B VM detail and lifecycle job actions. Keep the Markdown prose Korean except the API README, which is already English.

- [ ] **Step 3: Run documentation grep and diff checks**

Run:

```powershell
rg -n "Phase 3B|VM detail|lifecycle|/api/v1/vms/\\{id\\}|PcvDesktopWeb" README.md AGENTS.md docs spikes/purecvisor-desktop-node/api/README.md ui/guide-content.md
git diff --check
```

Expected: Phase 3B references appear in the intended docs; `git diff --check` exits 0.

- [ ] **Step 4: Commit documentation updates**

Run:

```powershell
git add spikes/purecvisor-desktop-node/api/README.md docs/superpowers/specs/2026-04-24-purecvisor-desktop-node-design.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/GUIDE.md docs/PUBLIC_RELEASE_BOUNDARY.md README.md AGENTS.md ui/guide-content.md
git commit -m "docs: refresh Desktop Node phase 3b documentation"
```

## Task 6: Final Verification

**Files:**
- Verify all Phase 3B files.
- Update this plan completion evidence after all commands pass.

- [x] **Step 1: Run complete Phase 3B verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
git status --short --branch
```

Expected:

- API suite: 62 passed, 0 failed.
- Web suite: 9 passed, 0 failed.
- Hyper-V helper non-integration suite: all non-integration tests pass.
- Node syntax check exits 0.
- `git diff --check` exits 0.
- `git status --short --branch` shows no uncommitted changes after final plan evidence is committed.

- [x] **Step 2: Update this plan with completion evidence**

Append this section to the end of this file, replacing command counts with the fresh output:

```markdown
## Completion Status

Phase 3B implementation is complete through local non-integration verification.

Evidence:
- API suite: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed` -> 62 passed, 0 failed.
- Web static suite: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed` -> 9 passed, 0 failed.
- Hyper-V helper suite: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed` -> all non-integration tests passed, 0 failed.
- JavaScript syntax: `node --check spikes/purecvisor-desktop-node/web/app.js` -> exit 0.
- Diff hygiene: `git diff --check` -> exit 0.

Integration note:
- Actual Hyper-V VM lifecycle operations remain gated integration validation requiring administrator privileges and a test VM.
```

- [x] **Step 3: Commit final plan evidence**

Run:

```powershell
git add docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle.md
git commit -m "docs: record Desktop Node phase 3b verification"
```

- [x] **Step 4: Confirm final branch state**

Run:

```powershell
git log -6 --oneline --decorate
git status --short --branch
```

Expected: recent commits include API tests, API implementation, Web tests, Web implementation, docs, and verification evidence; working tree is clean.

## Self-Review

- Spec coverage: Tasks cover VM detail API, lifecycle job routes, auth coverage, worker dispatch, VM row selection, detail drawer, lifecycle buttons, destructive confirmation, docs, and verification.
- Scope control: Checkpoint UI, checkpoint API routes, VMConnect launch, LAN mode, token issuance, persistent browser job history, threaded workers, Windows service setup, and Linux Single Edge UI changes remain outside this plan.
- Type consistency: API route operations use `vm.get`, `job.create`, `vm.start`, `vm.shutdown`, `vm.poweroff`, and `vm.restart`; Web route calls use `/api/v1/vms/${id}` and `/api/v1/vms/${id}/${action}` with `action` values `start`, `shutdown`, `poweroff`, and `restart`.
- Verification sequence: API RED tests precede API implementation; Web RED tests precede Web implementation; final verification runs API, Web, Hyper-V helper non-integration, Node syntax, and diff hygiene.

## Completion Status

Phase 3B implementation is complete through local non-integration verification.
Fresh verification was rerun after the inventory detail contract fix commit (`90bb32f`).

Evidence:
- API suite: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed` -> 62 passed, 0 failed.
- Web static suite: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed` -> 9 passed, 0 failed.
- Hyper-V helper suite: `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed` -> 39 passed, 0 failed, 1 integration test not run.
- JavaScript syntax: `node --check spikes/purecvisor-desktop-node/web/app.js` -> exit 0.
- Diff hygiene: `git diff --check` -> exit 0.

Integration note:
- Actual Hyper-V VM lifecycle operations remain gated integration validation requiring administrator privileges and a test VM.

## Documentation Refresh Status

Phase 3B documentation was refreshed after the final inventory detail contract fix.

Evidence:
- Active entrypoint docs now link the Phase 3B design and plan, and describe the Hyper-V helper as the current inventory/lifecycle/checkpoint contract source.
- Desktop Node MVP and Phase 3A/3B design docs now include current Phase 3B scope and verification counts.
- API and Hyper-V spike READMEs now document VM detail/lifecycle routes, inventory detail fields, current expected test counts, and example request files.
- Documentation hygiene: `git diff --check` -> exit 0.
- Stale phrase scan: `rg -n "Desktop Node Phase 1 spike|Expected result after Phase 1|gap: Local API|아직 제공하지 않고|구현 전 기준" ...` -> no matches in active entrypoint/spec/spike docs.
