Describe 'PcvDesktopApi route contract' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

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

    It 'routes GET /api/v1/host/status to host.status helper operation' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/host/status' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'host.status'
        $json.data.marker | Should -Be 'host.status'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'host.status'
    }

    It 'routes GET /api/v1/vms to vm.list helper operation' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'vm.list'
        $json.data[0].id | Should -Be 'ubuntu-lab-01'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'vm.list'
    }

    It 'routes GET /api/v1/network/inventory to network.inventory helper operation' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/network/inventory' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'network.inventory'
        $json.data.marker | Should -Be 'network.inventory'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'network.inventory'
    }

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
                id = 'lab vm 01'
                name = 'lab vm 01'
                state = 'stopped'
            }
        )

        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/lab%20vm%2001' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'vm.get'
        $json.data.name | Should -Be 'lab vm 01'
    }

    It 'rejects malformed VM detail route ids before calling the helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/%ZZ' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'vm.get'
        $json.error.code | Should -Be 'PCV_ROUTE_ID_INVALID'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'rejects blank VM detail route ids before calling the helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/%20' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'vm.get'
        $json.error.code | Should -Be 'PCV_ROUTE_ID_INVALID'
        $script:HelperCalls.Count | Should -Be 0
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

    It 'routes GET /api/v1/vms/{id}/checkpoints to checkpoint.list helper operation' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/ubuntu-lab-01/checkpoints' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'checkpoint.list'
        $json.data.marker | Should -Be 'checkpoint.list'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'checkpoint.list'
        $script:HelperCalls[0].params.vm_name | Should -Be 'ubuntu-lab-01'
    }

    It 'decodes checkpoint list route ids before calling the helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/lab%20vm%2001/checkpoints' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'checkpoint.list'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].params.vm_name | Should -Be 'lab vm 01'
    }

    It 'rejects malformed checkpoint list route ids before calling the helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/%ZZ/checkpoints' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'checkpoint.list'
        $json.error.code | Should -Be 'PCV_ROUTE_ID_INVALID'
        $script:HelperCalls.Count | Should -Be 0
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

    It 'returns 404 for unsupported routes without calling the helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/vms/demo/unknown' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 404
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.error.code | Should -Be 'PCV_ROUTE_NOT_FOUND'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'returns 405 for unsupported methods without calling the helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'DELETE' `
            -Path '/api/v1/vms' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 405
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.error.code | Should -Be 'PCV_METHOD_NOT_ALLOWED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'exposes the current Local API runtime hardening policy' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/runtime/policy' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'runtime.policy'
        $json.data.persistence.backend | Should -Be 'json-file'
        $json.data.persistence.database_backed | Should -BeFalse
        $json.data.retry.automatic | Should -BeFalse
        $json.data.retry.manual | Should -BeTrue
        $json.data.retry.max_attempts | Should -Be 3
        $json.data.retry.backoff | Should -Be 'deferred'
        $json.data.cancel.queued | Should -BeTrue
        $json.data.cancel.running | Should -BeFalse
        $json.data.worker.mode | Should -Be 'bounded_tick'
        $json.data.worker.threaded | Should -BeFalse
        $json.data.cors.enabled | Should -BeFalse
        $json.data.cors.options_preflight | Should -BeFalse
        $json.data.auth.mode | Should -Be 'single_bearer_token'
        $json.data.auth.multi_user | Should -BeFalse
        $json.data.auth.rbac | Should -BeFalse
        $json.data.auth.token_storage | Should -Be 'external_token_file'
        $json.data.network.default_exposure | Should -Be 'loopback'
        $json.data.network.current_exposure | Should -Be 'loopback'
        $json.data.network.lan_mode | Should -Be 'preview-admin-opt-in'
        $json.data.network.static_asset_auth.loopback | Should -Be 'unauthenticated-static-only'
        $json.data.network.static_asset_auth.non_loopback | Should -Be 'bearer-required'
        $json.data.network.tls.provided_by_product_wrapper | Should -BeFalse
        $json.data.network.tls.required_for_lan | Should -BeTrue
        $json.data.network.tls.termination | Should -Be 'external-reverse-proxy-or-tls-terminator'
        $json.data.network.firewall.enabled_by_default | Should -BeFalse
        $json.data.network.firewall.lifecycle_owner | Should -Be 'admin-opt-in-product-action-or-manual-command'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'exposes the Phase 24 Local API job runtime boundary contract' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/runtime/policy' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 200
        $json = $response.body | ConvertFrom-Json
        $json.operation | Should -Be 'runtime.policy'
        $json.data.job_runtime.contract_version | Should -Be 1
        $json.data.job_runtime.owner | Should -Be 'local-api'
        $json.data.job_runtime.state_store.backend | Should -Be 'script-scope-memory'
        $json.data.job_runtime.state_store.persistence | Should -Be 'json-file-snapshot'
        $json.data.job_runtime.state_store.corrupt_store | Should -Be 'quarantine-and-start-empty'
        $json.data.job_runtime.state_store.unsupported_future_version | Should -Be 'quarantine-and-start-empty'
        $json.data.job_runtime.dispatch.mode | Should -Be 'bounded-synchronous-worker-tick'
        $json.data.job_runtime.dispatch.helper_boundary | Should -Be 'hyperv-helper-process'
        $json.data.job_runtime.control.cancel.queued_only | Should -BeTrue
        $json.data.job_runtime.control.cancel.running_interrupt | Should -BeFalse
        $json.data.job_runtime.control.retry.manual_only | Should -BeTrue
        $json.data.job_runtime.control.retry.failed_error_retryable_only | Should -BeTrue
        $json.data.job_runtime.control.retry.max_attempts | Should -Be 3
        $json.data.job_runtime.control.retry.creates_new_job | Should -BeTrue
        $json.data.job_runtime.host_mutation | Should -Be 'helper-process-only'
        $json.data.job_runtime.orchestration.primary | Should -Be 'powershell'
        $json.data.job_runtime.orchestration.contract | Should -Be 'plan-contract-injectable-runner-diagnostics'
        $json.data.job_runtime.native_core.status | Should -Be 'not-planned-unless-runtime-boundary-deepens'
        $json.data.job_runtime.native_core.reason | Should -Be 'windows-hyperv-orchestration-not-dataplane'
        $json.data.job_runtime.native_core.revisit_when | Should -Be 'state-machine-or-supervision-outgrows-powershell'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'keeps the runtime policy route read-only' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/runtime/policy' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 405
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.error.code | Should -Be 'PCV_METHOD_NOT_ALLOWED'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'accepts loopback HTTP prefixes for the local daemon' {
        { Assert-PcvLoopbackPrefix -Prefix 'http://127.0.0.1:7777/' } | Should -Not -Throw
        { Assert-PcvLoopbackPrefix -Prefix 'http://localhost:7777/' } | Should -Not -Throw
        { Assert-PcvLoopbackPrefix -Prefix 'http://[::1]:7777/' } | Should -Not -Throw
    }

    It 'rejects non-loopback HTTP prefixes for the local daemon' {
        { Assert-PcvLoopbackPrefix -Prefix 'http://0.0.0.0:7777/' } | Should -Throw -ExpectedMessage '*PCV_PREFIX_NOT_LOOPBACK*'
        { Assert-PcvLoopbackPrefix -Prefix 'http://192.168.1.10:7777/' } | Should -Throw -ExpectedMessage '*PCV_PREFIX_NOT_LOOPBACK*' # public-safety: synthetic-rfc1918
    }
}
