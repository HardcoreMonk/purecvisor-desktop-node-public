Describe 'PcvDesktopApi job API' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    BeforeEach {
        Clear-PcvApiJobStore
        $script:HelperCalls = @()
        $script:VmCreateBody = @{
            name = 'ubuntu-lab-01'
            iso_path = 'D:\iso\ubuntu-24.04-live-server-amd64.iso'
            cpu = 2
            memory_mb = 4096
            disk_gb = 40
            vm_root = 'D:\PureCVisor\VMs'
            generation = 2
        } | ConvertTo-Json -Depth 8

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

            [ordered]@{
                ok = $true
                operation = $Operation
                data = [ordered]@{
                    name = $Params.name
                    id = $Params.name
                }
                error = $null
            }
        }
    }

    It 'accepts POST /api/v1/vms and stores a queued job without calling the helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body $script:VmCreateBody `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'job.create'
        $json.data.job_id | Should -Not -BeNullOrEmpty
        $json.data.status | Should -Be 'queued'

        $script:HelperCalls.Count | Should -Be 0

        $jobResponse = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path "/api/v1/jobs/$($json.data.job_id)" `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $jobResponse.status | Should -Be 200
        $jobJson = $jobResponse.body | ConvertFrom-Json
        $jobJson.ok | Should -BeTrue
        $jobJson.operation | Should -Be 'job.get'
        $jobJson.data.job_id | Should -Be $json.data.job_id
        $jobJson.data.operation | Should -Be 'vm.create'
        $jobJson.data.status | Should -Be 'queued'
        $jobJson.data.params.name | Should -Be 'ubuntu-lab-01'
        $jobJson.data.params.memory_mb | Should -Be 4096
        $jobJson.data.result | Should -BeNullOrEmpty
        $jobJson.data.error | Should -BeNullOrEmpty
    }

    It 'keeps helper failures out of the request path until a worker processes the job' {
        $failingHelper = {
            param(
                [string]$Operation,
                [AllowNull()]$Params,
                [string]$HelperScriptPath,
                [int]$TimeoutSec
            )

            [ordered]@{
                ok = $false
                operation = $Operation
                data = $null
                error = [ordered]@{
                    code = 'PCV_HOST_NOT_READY'
                    message = 'The Hyper-V host is not ready.'
                    detail = 'PCV_ADMIN_REQUIRED'
                    retryable = $false
                }
            }
        }

        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body $script:VmCreateBody `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $failingHelper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.data.status | Should -Be 'queued'
        $script:HelperCalls.Count | Should -Be 0

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $failingHelper

        $tick.processed | Should -BeTrue
        $tick.job.job_id | Should -Be $json.data.job_id

        $jobResponse = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path "/api/v1/jobs/$($json.data.job_id)" `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $jobJson = $jobResponse.body | ConvertFrom-Json
        $jobJson.data.status | Should -Be 'failed'
        $jobJson.data.error.code | Should -Be 'PCV_HOST_NOT_READY'
        $jobJson.data.result | Should -BeNullOrEmpty
    }

    It 'rejects invalid JSON before creating a job' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body '{not-json' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.error.code | Should -Be 'PCV_INVALID_JSON'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'rejects missing POST bodies before creating a job' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.error.code | Should -Be 'PCV_REQUEST_BODY_MISSING'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'returns PCV_JOB_NOT_FOUND for unknown job ids' {
        $response = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path '/api/v1/jobs/missing-job' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 404
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'job.get'
        $json.error.code | Should -Be 'PCV_JOB_NOT_FOUND'
        $script:HelperCalls.Count | Should -Be 0
    }

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
            -Path '/api/v1/vms/lab%20vm%2001/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.data.operation | Should -Be 'vm.start'
        $json.data.params.name | Should -Be 'lab vm 01'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'rejects malformed lifecycle route ids without creating a job' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/%ZZ/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'job.create'
        $json.error.code | Should -Be 'PCV_ROUTE_ID_INVALID'
        $script:HelperCalls.Count | Should -Be 0
        Get-PcvApiJobQueueIds | Should -BeNullOrEmpty
    }

    It 'rejects blank lifecycle route ids without creating a job' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms//start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'job.create'
        $json.error.code | Should -Be 'PCV_ROUTE_ID_INVALID'
        $script:HelperCalls.Count | Should -Be 0
        Get-PcvApiJobQueueIds | Should -BeNullOrEmpty
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
        Get-PcvApiJobQueueIds | Should -BeNullOrEmpty
    }

    It 'queues checkpoint create jobs without calling the helper during the request' {
        $body = @{ name = 'before-upgrade' } | ConvertTo-Json -Depth 4

        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/checkpoints' `
            -Body $body `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'job.create'
        $json.data.operation | Should -Be 'checkpoint.create'
        $json.data.status | Should -Be 'queued'
        $json.data.params.vm_name | Should -Be 'ubuntu-lab-01'
        $json.data.params.checkpoint_name | Should -Be 'before-upgrade'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'queues checkpoint restore jobs from route checkpoint ids' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/checkpoints/before-upgrade/restore' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.data.operation | Should -Be 'checkpoint.restore'
        $json.data.params.vm_name | Should -Be 'ubuntu-lab-01'
        $json.data.params.checkpoint_name | Should -Be 'before-upgrade'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'queues checkpoint delete jobs from DELETE route checkpoint ids' {
        $response = Invoke-PcvApiRequest `
            -Method 'DELETE' `
            -Path '/api/v1/vms/ubuntu-lab-01/checkpoints/before-upgrade' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.data.operation | Should -Be 'checkpoint.delete'
        $json.data.params.vm_name | Should -Be 'ubuntu-lab-01'
        $json.data.params.checkpoint_name | Should -Be 'before-upgrade'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'decodes checkpoint route ids before storing job params' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/lab%20vm%2001/checkpoints/before%20upgrade/restore' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $json = $response.body | ConvertFrom-Json
        $json.data.operation | Should -Be 'checkpoint.restore'
        $json.data.params.vm_name | Should -Be 'lab vm 01'
        $json.data.params.checkpoint_name | Should -Be 'before upgrade'
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'rejects missing checkpoint create names without creating a job' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/checkpoints' `
            -Body '{}' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'checkpoint.create'
        $json.error.code | Should -Be 'PCV_CHECKPOINT_NAME_REQUIRED'
        $script:HelperCalls.Count | Should -Be 0
        Get-PcvApiJobQueueIds | Should -BeNullOrEmpty
    }

    It 'rejects malformed checkpoint route ids without creating a job' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/checkpoints/%ZZ/restore' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 400
        $json = $response.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'checkpoint.restore'
        $json.error.code | Should -Be 'PCV_ROUTE_ID_INVALID'
        $script:HelperCalls.Count | Should -Be 0
        Get-PcvApiJobQueueIds | Should -BeNullOrEmpty
    }
}
