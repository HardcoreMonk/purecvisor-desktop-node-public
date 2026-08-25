Describe 'PcvDesktopApi job control' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    BeforeEach {
        Clear-PcvApiJobStore
        $script:StorePath = Join-Path $TestDrive 'jobs.json'
        $script:HelperCalls = @()
        $script:NewVmCreateBody = {
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

        $script:FailingHelper = {
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
                ok = $false
                operation = $Operation
                data = $null
                error = [ordered]@{
                    code = 'PCV_HELPER_EXIT_FAILED'
                    message = 'The Hyper-V helper process failed.'
                    detail = 'synthetic failure'
                    retryable = $true
                }
            }
        }
    }

    It 'cancels a queued job before a worker executes it' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewVmCreateBody 'cancel-me') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $jobId = ($create.body | ConvertFrom-Json).data.job_id

        $cancel = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$jobId/cancel" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $cancel.status | Should -Be 200
        $json = $cancel.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'job.cancel'
        $json.data.job_id | Should -Be $jobId
        $json.data.status | Should -Be 'canceled'
        $json.data.error.code | Should -Be 'PCV_JOB_CANCELED'
        Get-PcvApiJobQueueIds | Should -Not -Contain $jobId

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $tick.processed | Should -BeFalse
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'rejects cancellation after a job completed' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewVmCreateBody 'already-done') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $jobId = ($create.body | ConvertFrom-Json).data.job_id

        [void](Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper)

        $cancel = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$jobId/cancel" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $cancel.status | Should -Be 409
        $json = $cancel.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'job.cancel'
        $json.error.code | Should -Be 'PCV_JOB_NOT_CANCELABLE'
    }

    It 'retries a failed job as a new queued job' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewVmCreateBody 'retry-me') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $failedJobId = ($create.body | ConvertFrom-Json).data.job_id

        [void](Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:FailingHelper)

        $retry = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$failedJobId/retry" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $retry.status | Should -Be 202
        $json = $retry.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.operation | Should -Be 'job.retry'
        $json.data.job_id | Should -Not -Be $failedJobId
        $json.data.status | Should -Be 'queued'
        $json.data.retry_of | Should -Be $failedJobId
        $json.data.attempt | Should -Be 2
        $json.data.params.name | Should -Be 'retry-me'

        $original = Get-PcvApiJob -JobId $failedJobId
        $original.status | Should -Be 'failed'

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $tick.processed | Should -BeTrue
        $tick.job.job_id | Should -Be $json.data.job_id
        $tick.job.status | Should -Be 'succeeded'
        $tick.job.result.data.name | Should -Be 'retry-me'
    }

    It 'rejects retry for non-failed jobs' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewVmCreateBody 'not-failed') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $jobId = ($create.body | ConvertFrom-Json).data.job_id

        $retry = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$jobId/retry" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $retry.status | Should -Be 409
        $json = $retry.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'job.retry'
        $json.error.code | Should -Be 'PCV_JOB_NOT_RETRYABLE'
    }

    It 'rejects manual retry after the runtime retry attempt limit' {
        $job = New-PcvApiJob `
            -Operation 'vm.create' `
            -Params ([ordered]@{ name = 'retry-limit' }) `
            -Attempt 3
        $job.status = 'failed'
        $job.error = New-PcvApiError `
            -Code 'PCV_HELPER_EXIT_FAILED' `
            -Message 'The Hyper-V helper process failed.' `
            -Detail 'synthetic failure' `
            -Retryable $true

        $retry = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$($job.job_id)/retry" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $retry.status | Should -Be 409
        $json = $retry.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'job.retry'
        $json.error.code | Should -Be 'PCV_JOB_RETRY_LIMIT_REACHED'
        $json.error.retryable | Should -BeFalse
        Get-PcvApiJobQueueIds | Should -Not -Contain $job.job_id
    }

    It 'rejects manual retry when the failed job error is not retryable' {
        $job = New-PcvApiJob `
            -Operation 'vm.create' `
            -Params ([ordered]@{ name = 'non-retryable-failure' })
        $job.status = 'failed'
        $job.error = New-PcvApiError `
            -Code 'PCV_VM_CREATE_INVALID_INPUT' `
            -Message 'The VM create request is invalid.' `
            -Detail 'Synthetic non-retryable failure' `
            -Retryable $false

        $retry = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$($job.job_id)/retry" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $retry.status | Should -Be 409
        $json = $retry.body | ConvertFrom-Json
        $json.ok | Should -BeFalse
        $json.operation | Should -Be 'job.retry'
        $json.error.code | Should -Be 'PCV_JOB_NOT_RETRYABLE'
        $json.error.detail | Should -Match 'retryable'
        Get-PcvApiJobQueueIds | Should -Not -Contain $job.job_id
    }

    It 'persists cancellation state and removes the job from the persisted queue' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewVmCreateBody 'persist-cancel') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath
        $jobId = ($create.body | ConvertFrom-Json).data.job_id

        [void](Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$jobId/cancel" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath)

        $store = Get-Content -LiteralPath $script:StorePath -Raw | ConvertFrom-Json
        @($store.jobs).Count | Should -Be 1
        $store.jobs[0].job_id | Should -Be $jobId
        $store.jobs[0].status | Should -Be 'canceled'
        $store.jobs[0].error.code | Should -Be 'PCV_JOB_CANCELED'
        @($store.queue).Count | Should -Be 0
    }

    It 'persists retry jobs with the source failure preserved' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewVmCreateBody 'persist-retry') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath
        $failedJobId = ($create.body | ConvertFrom-Json).data.job_id

        [void](Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:FailingHelper `
            -JobStorePath $script:StorePath)

        $retry = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path "/api/v1/jobs/$failedJobId/retry" `
            -Body $null `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath
        $retryJobId = ($retry.body | ConvertFrom-Json).data.job_id

        $store = Get-Content -LiteralPath $script:StorePath -Raw | ConvertFrom-Json
        @($store.jobs).Count | Should -Be 2
        ($store.jobs | Where-Object job_id -EQ $failedJobId).status | Should -Be 'failed'
        ($store.jobs | Where-Object job_id -EQ $retryJobId).status | Should -Be 'queued'
        ($store.jobs | Where-Object job_id -EQ $retryJobId).retry_of | Should -Be $failedJobId
        ($store.jobs | Where-Object job_id -EQ $retryJobId).attempt | Should -Be 2
        $store.queue[0] | Should -Be $retryJobId
    }
}
