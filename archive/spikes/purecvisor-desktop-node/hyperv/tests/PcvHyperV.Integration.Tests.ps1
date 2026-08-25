Describe 'Hyper-V integration spike' -Tag Integration -Skip:($env:PCV_HYPERV_INTEGRATION -ne '1') {
    BeforeAll {
        $Root = Split-Path -Parent $PSScriptRoot
        $script:Runner = Join-Path $Root 'Invoke-PcvHyperV.ps1'
        $script:VmName = "pcv-spike-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
        $script:VmRoot = Join-Path $env:TEMP 'pcv-hyperv-spike'
        $script:VmDir = Join-Path $script:VmRoot $script:VmName
        $script:IsoPath = $env:PCV_HYPERV_TEST_ISO
        $script:VmNameReserved = $false

        if ([string]::IsNullOrWhiteSpace($script:IsoPath)) {
            throw 'Set PCV_HYPERV_TEST_ISO to a local Linux ISO path before running integration tests.'
        }

        if (-not (Test-Path -LiteralPath $script:IsoPath -PathType Leaf)) {
            throw "PCV_HYPERV_TEST_ISO '$script:IsoPath' is not a file. Set it to a local Linux ISO path before running integration tests."
        }

        $existingVm = Get-VM -Name $script:VmName -ErrorAction SilentlyContinue
        if ($existingVm) {
            throw "Generated test VM name '$script:VmName' already exists. Re-run the integration test to generate a new name."
        }
        $script:VmNameReserved = $true

        function Invoke-PcvIntegrationRequest {
            param([Parameter(Mandatory)]$Payload)

            $payloadJson = $Payload | ConvertTo-Json -Depth 8
            $rawOutput = $payloadJson | & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:Runner
            $rawText = ($rawOutput -join [Environment]::NewLine).Trim()

            [pscustomobject]@{
                Raw = $rawText
                Json = ($rawText | ConvertFrom-Json)
            }
        }

        function Assert-PcvIntegrationOk {
            param(
                [Parameter(Mandatory)]$Response,
                [Parameter(Mandatory)][string]$Operation
            )

            $errorJson = if ($Response.Json.error) {
                $Response.Json.error | ConvertTo-Json -Depth 8 -Compress
            }
            else {
                '<none>'
            }
            $because = "raw stdout: $($Response.Raw); error: $errorJson"

            $Response.Json.ok | Should -BeTrue -Because $because
            $Response.Json.operation | Should -Be $Operation -Because $because
        }
    }

    AfterAll {
        if ($script:VmNameReserved -and $script:VmName -and $script:VmName -like 'pcv-spike-*') {
            try {
                $vm = Get-VM -Name $script:VmName -ErrorAction SilentlyContinue
                $ownedByMarker = ($vm -and [string]$vm.Notes -match 'managed-by=purecvisor-desktop-node')
                $ownedByPath = $false
                if ($vm -and $script:VmDir) {
                    foreach ($propertyName in @('Path', 'ConfigurationLocation')) {
                        if ($vm.PSObject.Properties.Name.Contains($propertyName) -and $null -ne $vm.$propertyName) {
                            $pathValue = [string]$vm.$propertyName
                            if ($pathValue.StartsWith($script:VmDir, [System.StringComparison]::OrdinalIgnoreCase) -or
                                $pathValue.StartsWith($script:VmRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
                                $ownedByPath = $true
                            }
                        }
                    }
                }

                if ($vm -and ($ownedByMarker -or $ownedByPath)) {
                    Stop-VM -Name $script:VmName -TurnOff -Force -ErrorAction SilentlyContinue
                    Remove-VM -Name $script:VmName -Force -ErrorAction SilentlyContinue
                }

                if ($script:VmDir -and (Test-Path -LiteralPath $script:VmDir)) {
                    Remove-Item -LiteralPath $script:VmDir -Recurse -Force -ErrorAction SilentlyContinue
                }
            }
            catch {
            }
        }
    }

    It 'runs host.status, vm.create, vm.list, vm.start, checkpoint.create, and vm.poweroff through the runner' {
        $hostStatus = Invoke-PcvIntegrationRequest -Payload @{ operation = 'host.status'; params = @{} }
        Assert-PcvIntegrationOk -Response $hostStatus -Operation 'host.status'

        $hostStatus.Json.data.hyperv.feature_enabled | Should -BeTrue -Because $hostStatus.Raw
        $hostStatus.Json.data.hyperv.vmms_running | Should -BeTrue -Because $hostStatus.Raw

        $created = Invoke-PcvIntegrationRequest -Payload @{
            operation = 'vm.create'
            params = @{
                name = $script:VmName
                iso_path = $script:IsoPath
                cpu = 1
                memory_mb = 1024
                disk_gb = 8
                vm_root = $script:VmRoot
                generation = 2
            }
        }
        Assert-PcvIntegrationOk -Response $created -Operation 'vm.create'

        $created.Json.data.name | Should -Be $script:VmName -Because $created.Raw

        $listed = Invoke-PcvIntegrationRequest -Payload @{ operation = 'vm.list'; params = @{} }
        Assert-PcvIntegrationOk -Response $listed -Operation 'vm.list'

        @($listed.Json.data | Where-Object { $_.name -eq $script:VmName }).Count | Should -Be 1 -Because $listed.Raw

        $started = Invoke-PcvIntegrationRequest -Payload @{ operation = 'vm.start'; params = @{ name = $script:VmName } }
        Assert-PcvIntegrationOk -Response $started -Operation 'vm.start'

        $checkpoint = Invoke-PcvIntegrationRequest -Payload @{
            operation = 'checkpoint.create'
            params = @{
                vm_name = $script:VmName
                checkpoint_name = 'before-install'
            }
        }
        Assert-PcvIntegrationOk -Response $checkpoint -Operation 'checkpoint.create'

        $checkpoint.Json.data.name | Should -Be 'before-install' -Because $checkpoint.Raw

        $poweroff = Invoke-PcvIntegrationRequest -Payload @{ operation = 'vm.poweroff'; params = @{ name = $script:VmName } }
        Assert-PcvIntegrationOk -Response $poweroff -Operation 'vm.poweroff'
    }
}
