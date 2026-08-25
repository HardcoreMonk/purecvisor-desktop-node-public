Describe 'PcvDesktopApi bounded worker pool' {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $ModulePath = Join-Path $Root 'PcvDesktopApi.psm1'
        Import-Module $ModulePath -Force
    }

    BeforeEach {
        Clear-PcvApiJobStore
        $script:HelperCalls = @()
        $script:NewPoolVmCreateBody = {
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

    It 'processes up to WorkerCount queued jobs in FIFO order' {
        $first = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'first') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $second = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'second') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $third = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'third') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $firstId = ($first.body | ConvertFrom-Json).data.job_id
        $secondId = ($second.body | ConvertFrom-Json).data.job_id
        $thirdId = ($third.body | ConvertFrom-Json).data.job_id

        $pool = Invoke-PcvApiWorkerPoolTick `
            -WorkerCount 2 `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $pool.processed | Should -BeTrue
        $pool.processed_count | Should -Be 2
        @($pool.jobs).Count | Should -Be 2
        $pool.jobs[0].job_id | Should -Be $firstId
        $pool.jobs[1].job_id | Should -Be $secondId
        $pool.jobs[0].status | Should -Be 'succeeded'
        $pool.jobs[1].status | Should -Be 'succeeded'
        $pool.remaining_queue | Should -Contain $thirdId
        @($pool.remaining_queue).Count | Should -Be 1
        $script:HelperCalls[0].params.name | Should -Be 'first'
        $script:HelperCalls[1].params.name | Should -Be 'second'
        $script:HelperCalls.Count | Should -Be 2
        (Get-PcvApiJob -JobId $thirdId).status | Should -Be 'queued'
    }

    It 'processes all available jobs when WorkerCount is larger than the queue' {
        $first = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'alpha') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $second = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'beta') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $firstId = ($first.body | ConvertFrom-Json).data.job_id
        $secondId = ($second.body | ConvertFrom-Json).data.job_id

        $pool = Invoke-PcvApiWorkerPoolTick `
            -WorkerCount 10 `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $pool.processed | Should -BeTrue
        $pool.processed_count | Should -Be 2
        @($pool.jobs).Count | Should -Be 2
        $pool.jobs[0].job_id | Should -Be $firstId
        $pool.jobs[1].job_id | Should -Be $secondId
        @($pool.remaining_queue).Count | Should -Be 0
        $script:HelperCalls.Count | Should -Be 2
    }

    It 'reports no work when the queue is empty' {
        $pool = Invoke-PcvApiWorkerPoolTick `
            -WorkerCount 4 `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $pool.processed | Should -BeFalse
        $pool.processed_count | Should -Be 0
        @($pool.jobs).Count | Should -Be 0
        @($pool.remaining_queue).Count | Should -Be 0
        $script:HelperCalls.Count | Should -Be 0
    }

    It 'preserves one-job behavior when WorkerCount is one' {
        $first = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'single-first') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper
        $second = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'single-second') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $firstId = ($first.body | ConvertFrom-Json).data.job_id
        $secondId = ($second.body | ConvertFrom-Json).data.job_id

        $pool = Invoke-PcvApiWorkerPoolTick `
            -WorkerCount 1 `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper

        $pool.processed | Should -BeTrue
        $pool.processed_count | Should -Be 1
        @($pool.jobs).Count | Should -Be 1
        $pool.jobs[0].job_id | Should -Be $firstId
        $pool.remaining_queue | Should -Contain $secondId
        @($pool.remaining_queue).Count | Should -Be 1
        $script:HelperCalls.Count | Should -Be 1
    }

    It 'persists each completed job and drains the persisted queue' {
        $storePath = Join-Path $TestDrive 'jobs.json'
        $first = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'persist-first') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $storePath
        $second = Invoke-PcvApiRequest `
            -Method 'POST' `
            -Path '/api/v1/vms' `
            -Body (& $script:NewPoolVmCreateBody 'persist-second') `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $storePath

        $firstId = ($first.body | ConvertFrom-Json).data.job_id
        $secondId = ($second.body | ConvertFrom-Json).data.job_id

        $pool = Invoke-PcvApiWorkerPoolTick `
            -WorkerCount 2 `
            -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
            -InvokeHelper $script:Helper `
            -JobStorePath $storePath

        $pool.processed_count | Should -Be 2
        @($pool.remaining_queue).Count | Should -Be 0

        $store = Get-Content -LiteralPath $storePath -Raw | ConvertFrom-Json
        @($store.queue).Count | Should -Be 0
        ($store.jobs | Where-Object job_id -EQ $firstId).status | Should -Be 'succeeded'
        ($store.jobs | Where-Object job_id -EQ $secondId).status | Should -Be 'succeeded'
    }
}
