Describe 'PcvDesktopApi background worker queue' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    BeforeEach {
        Clear-PcvApiJobStore
        $script:HelperCalls = @()
        $script:NewWorkerVmCreateBody = {
            param([Parameter(Mandatory)][string]$Name)

            @{
                name = $Name
                iso_path = 'D:\iso\ubuntu-24.04-live-server-amd64.iso'
                cpu = 2
                memory_mb = 4096
                disk_gb = 40
                vm_root = 'D:\PureCVisor\VMs'
                generation = 2
            } | ConvertTo-Json -Depth 8
        }

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

    It 'processes one queued VM create job and stores a succeeded result' {
        $createResponse = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewWorkerVmCreateBody 'alpha') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $jobId = ($createResponse.body | ConvertFrom-Json).data.job_id
        $script:HelperCalls.Count | Should -Be 0

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $tick.processed | Should -BeTrue
        $tick.job.job_id | Should -Be $jobId
        $tick.job.status | Should -Be 'succeeded'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'vm.create'
        $script:HelperCalls[0].params.name | Should -Be 'alpha'

        $jobResponse = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path "/api/v1/jobs/$jobId" `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $jobJson = $jobResponse.body | ConvertFrom-Json
        $jobJson.data.status | Should -Be 'succeeded'
        $jobJson.data.result.data.name | Should -Be 'alpha'
    }

    It 'stores helper failures as failed jobs' {
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

        $createResponse = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewWorkerVmCreateBody 'beta') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $jobId = ($createResponse.body | ConvertFrom-Json).data.job_id

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $failingHelper

        $tick.processed | Should -BeTrue
        $tick.job.status | Should -Be 'failed'
        $tick.job.error.code | Should -Be 'PCV_HOST_NOT_READY'

        $jobResponse = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path "/api/v1/jobs/$jobId" `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $jobJson = $jobResponse.body | ConvertFrom-Json
        $jobJson.data.status | Should -Be 'failed'
        $jobJson.data.error.code | Should -Be 'PCV_HOST_NOT_READY'
        $jobJson.data.result | Should -BeNullOrEmpty
    }

    It 'processes queued jobs in FIFO order' {
        $first = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewWorkerVmCreateBody 'first') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $second = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewWorkerVmCreateBody 'second') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $firstId = ($first.body | ConvertFrom-Json).data.job_id
        $secondId = ($second.body | ConvertFrom-Json).data.job_id

        $firstTick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $secondTick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $firstTick.job.job_id | Should -Be $firstId
        $secondTick.job.job_id | Should -Be $secondId
        $script:HelperCalls[0].params.name | Should -Be 'first'
        $script:HelperCalls[1].params.name | Should -Be 'second'
    }

    It 'reports no work when the queue is empty' {
        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $tick.processed | Should -BeFalse
        $tick.job | Should -BeNullOrEmpty
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'dispatches queued lifecycle jobs to the Hyper-V helper' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/start' `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $createdJob = $response.body | ConvertFrom-Json
        $createdJob.operation | Should -Be 'job.create'
        $createdJob.data.job_id | Should -Not -BeNullOrEmpty
        $jobId = $createdJob.data.job_id

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

    It 'dispatches queued checkpoint jobs to the Hyper-V helper' {
        $body = @{ name = 'before-upgrade' } | ConvertTo-Json -Depth 4
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms/ubuntu-lab-01/checkpoints' `
            -Body $body `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $response.status | Should -Be 202
        $createdJob = $response.body | ConvertFrom-Json
        $createdJob.operation | Should -Be 'job.create'
        $jobId = $createdJob.data.job_id

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $tick.processed | Should -BeTrue
        $tick.job.job_id | Should -Be $jobId
        $tick.job.status | Should -Be 'succeeded'
        $script:HelperCalls.Count | Should -Be 1
        $script:HelperCalls[0].operation | Should -Be 'checkpoint.create'
        $script:HelperCalls[0].params.vm_name | Should -Be 'ubuntu-lab-01'
        $script:HelperCalls[0].params.checkpoint_name | Should -Be 'before-upgrade'
    }
}
