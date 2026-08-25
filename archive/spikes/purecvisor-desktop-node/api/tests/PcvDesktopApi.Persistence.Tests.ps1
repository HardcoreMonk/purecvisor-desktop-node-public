Describe 'PcvDesktopApi persisted job store' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    BeforeEach {
        Clear-PcvApiJobStore
        $script:StorePath = Join-Path $TestDrive 'jobs.json'
        $script:HelperCalls = @()
        $script:VmCreateBody = @{
            name = 'persisted-vm'
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

    It 'persists queued jobs when POST /api/v1/vms is accepted' {
        $response = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body $script:VmCreateBody `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath

        $json = $response.body | ConvertFrom-Json
        Test-Path -LiteralPath $script:StorePath | Should -BeTrue

        $store = Get-Content -LiteralPath $script:StorePath -Raw | ConvertFrom-Json
        $store.version | Should -Be 1
        $store.jobs.Count | Should -Be 1
        $store.jobs[0].job_id | Should -Be $json.data.job_id
        $store.jobs[0].status | Should -Be 'queued'
        $store.queue[0] | Should -Be $json.data.job_id
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'loads persisted jobs so GET /api/v1/jobs/{job_id} can return them after restart' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body $script:VmCreateBody `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath
        $jobId = ($create.body | ConvertFrom-Json).data.job_id

        Clear-PcvApiJobStore
        $load = Initialize-PcvApiJobStore -Path $script:StorePath
        $load.ok | Should -BeTrue

        $get = Invoke-PcvApiRequest `
            -Method 'GET' `
            -Path "/api/v1/jobs/$jobId" `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath

        $json = $get.body | ConvertFrom-Json
        $json.ok | Should -BeTrue
        $json.data.job_id | Should -Be $jobId
        $json.data.status | Should -Be 'queued'
        $json.data.params.name | Should -Be 'persisted-vm'
    }

    It 'persists worker completion state' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body $script:VmCreateBody `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath
        $jobId = ($create.body | ConvertFrom-Json).data.job_id

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath

        $tick.processed | Should -BeTrue

        $store = Get-Content -LiteralPath $script:StorePath -Raw | ConvertFrom-Json
        $store.jobs[0].job_id | Should -Be $jobId
        $store.jobs[0].status | Should -Be 'succeeded'
        $store.jobs[0].result.data.name | Should -Be 'persisted-vm'
        @($store.queue).Count | Should -Be 0
    }

    It 'processes queued jobs loaded from disk after restart' {
        $create = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body $script:VmCreateBody `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath
        $jobId = ($create.body | ConvertFrom-Json).data.job_id

        Clear-PcvApiJobStore
        $load = Initialize-PcvApiJobStore -Path $script:StorePath
        $load.ok | Should -BeTrue

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath

        $tick.processed | Should -BeTrue
        $tick.job.job_id | Should -Be $jobId
        $tick.job.status | Should -Be 'succeeded'
        $script:HelperCalls.Count | Should -Be 1
    }

    It 'recovers persisted running jobs as interrupted failures after restart' {
        $now = [DateTimeOffset]::UtcNow.ToString('o')
        $jobId = 'job-running-restart'
        $store = [ordered]@{
            version = 1
            saved_at = $now
            jobs = @(
                [ordered]@{
                    job_id = $jobId
                    operation = 'vm.create'
                    status = 'running'
                    params = [ordered]@{
                        name = 'interrupted-vm'
                    }
                    result = [ordered]@{
                        stale = $true
                    }
                    error = $null
                    retry_of = $null
                    attempt = 1
                    canceled_at = $null
                    created_at = $now
                    updated_at = $now
                }
            )
            queue = @($jobId)
        }
        $store | ConvertTo-Json -Depth 50 | Set-Content -LiteralPath $script:StorePath -Encoding UTF8

        $load = Initialize-PcvApiJobStore -Path $script:StorePath

        $load.ok | Should -BeTrue
        $load.loaded_jobs | Should -Be 1
        $load.queued_jobs | Should -Be 0

        $job = Get-PcvApiJob -JobId $jobId
        $job.status | Should -Be 'failed'
        $job.result | Should -BeNullOrEmpty
        $job.error.code | Should -Be 'PCV_JOB_INTERRUPTED'
        $job.error.retryable | Should -BeTrue

        $tick = Invoke-PcvApiWorkerTick `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $script:StorePath

        $tick.processed | Should -BeFalse
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'quarantines corrupt job store files and starts with an empty store' {
        Set-Content -LiteralPath $script:StorePath -Value '{not-json' -Encoding UTF8

        $load = Initialize-PcvApiJobStore -Path $script:StorePath

        $load.ok | Should -BeFalse
        $load.error.code | Should -Be 'PCV_JOB_STORE_CORRUPT'
        Test-Path -LiteralPath $load.quarantine_path | Should -BeTrue
        Test-Path -LiteralPath $script:StorePath | Should -BeFalse

        $job = Get-PcvApiJob -JobId 'missing'
        $job | Should -BeNullOrEmpty
    }

    It 'quarantines unsupported future job store versions and starts with an empty store' {
        $now = [DateTimeOffset]::UtcNow.ToString('o')
        $store = [ordered]@{
            version = 999
            saved_at = $now
            jobs = @(
                [ordered]@{
                    job_id = 'job-from-future-store'
                    operation = 'vm.create'
                    status = 'queued'
                    params = [ordered]@{
                        name = 'future-store-vm'
                    }
                    result = $null
                    error = $null
                    retry_of = $null
                    attempt = 1
                    canceled_at = $null
                    created_at = $now
                    updated_at = $now
                }
            )
            queue = @('job-from-future-store')
        }
        $store | ConvertTo-Json -Depth 50 | Set-Content -LiteralPath $script:StorePath -Encoding UTF8

        $load = Initialize-PcvApiJobStore -Path $script:StorePath

        $load.ok | Should -BeFalse
        $load.error.code | Should -Be 'PCV_JOB_STORE_UNSUPPORTED_VERSION'
        $load.error.retryable | Should -BeFalse
        $load.error.detail | Should -Match 'version 999'
        Test-Path -LiteralPath $load.quarantine_path | Should -BeTrue
        Test-Path -LiteralPath $script:StorePath | Should -BeFalse

        $job = Get-PcvApiJob -JobId 'job-from-future-store'
        $job | Should -BeNullOrEmpty
    }
}
