Set-StrictMode -Version Latest

Describe 'PcvDesktopNodeProduct diagnostics' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'redacts tokens and Authorization headers from diagnostic objects' {
        $inputObject = [ordered]@{
            token = 'secret-token'
            api_token_file = 'C:\ProgramData\PureCVisor\desktop-node\api-token.txt'
            api_token_protected_file = 'C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json'
            protected_token = 'dpapi-blob-secret'
            token_sha256 = 'hash-secret'
            headers = [ordered]@{
                Authorization = 'Bearer secret-token'
                Accept = 'application/json'
            }
            nested = [ordered]@{
                access_token = 'nested-secret'
                message = 'safe'
            }
        }

        $redacted = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject -InputObject $inputObject
        $json = $redacted | ConvertTo-Json -Depth 16

        $json | Should -Not -Match 'secret-token'
        $json | Should -Not -Match 'nested-secret'
        $json | Should -Not -Match 'dpapi-blob-secret'
        $json | Should -Not -Match 'hash-secret'
        $redacted.token | Should -Be '[REDACTED]'
        $redacted.api_token_file | Should -Be '[REDACTED]'
        $redacted.api_token_protected_file | Should -Be '[REDACTED]'
        $redacted.protected_token | Should -Be '[REDACTED]'
        $redacted.token_sha256 | Should -Be '[REDACTED]'
        $redacted.headers.Authorization | Should -Be '[REDACTED]'
        $redacted.headers.Accept | Should -Be 'application/json'
        $redacted.nested.access_token | Should -Be '[REDACTED]'
    }

    It 'redacts bearer tokens inside diagnostic strings' {
        $redacted = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
            -InputObject 'Authorization: Bearer abc.def-ghi_123'

        $redacted | Should -Be 'Authorization: Bearer [REDACTED]'
    }

    It 'preserves null values while redacting diagnostic objects' {
        $redacted = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
            -InputObject ([ordered]@{
                detail = $null
                token = 'secret-token'
            })

        $null -eq $redacted.detail | Should -BeTrue
        $redacted.token | Should -Be '[REDACTED]'
    }

    It 'writes a diagnostic bundle without token file content' {
        $outRoot = Join-Path $TestDrive 'diagnostics'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        New-Item -ItemType Directory -Path $plan.data_root -Force | Out-Null
        Set-Content -LiteralPath $plan.auth.api_token_protected_file -Value '{"protected_token":"super-secret-token","token_sha256":"super-secret-token"}' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath $plan.auth.legacy_api_token_file -Value 'super-secret-token' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath $plan.paths.event_log -Value '{"Authorization":"Bearer super-secret-token","message":"boot"}' -Encoding UTF8
        Set-Content -LiteralPath $plan.paths.install_log -Value '{"step":"install","token":"super-secret-token"}' -Encoding UTF8
        [ordered]@{
            jobs = @(
                [ordered]@{
                    id = 'job-1'
                    status = 'queued'
                    access_token = 'super-secret-token'
                    detail = $null
                }
            )
        } | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $plan.paths.job_store -Encoding UTF8
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = "$FileName $($Arguments -join ' ')"; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{
                ok = $true
                body = '{"Authorization":"Bearer super-secret-token","detail":null}'
            }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        $bundle.ok | Should -BeTrue
        Test-Path -LiteralPath $bundle.path | Should -BeTrue
        $combined = Get-ChildItem -LiteralPath $bundle.path -File |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } |
            Out-String
        $combined | Should -Not -Match 'super-secret-token'
        $combined | Should -Match '\[REDACTED\]'
        Test-Path -LiteralPath (Join-Path $bundle.path 'token-file.txt') | Should -BeFalse
    }

    It 'includes service status and runtime policy artifacts' {
        $outRoot = Join-Path $TestDrive 'diagnostics-status'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeStatus') `
            -DataRoot (Join-Path $TestDrive 'data-status')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'STATE              : 4  RUNNING'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{
                ok = $true
                uri = ($Plan.service.config.prefix.TrimEnd('/') + '/api/v1/runtime/policy')
                body = '{"Authorization":"Bearer policy-secret","detail":null}'
            }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        Test-Path -LiteralPath (Join-Path $bundle.path 'service-status-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'runtime-policy-redacted.json') | Should -BeTrue
        $combined = Get-ChildItem -LiteralPath $bundle.path -File |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } |
            Out-String
        $combined | Should -Match 'RUNNING'
        $combined | Should -Not -Match 'policy-secret'
    }

    It 'redacts known root paths from diagnostic bundle artifacts' {
        $outRoot = Join-Path $TestDrive 'diagnostics-paths'
        $productRoot = Join-Path $TestDrive 'DesktopNodePaths'
        $dataRoot = Join-Path $TestDrive 'data-paths'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot
        New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
        [ordered]@{
            source_root = $script:RepoRoot
            paths = [ordered]@{
                product_root = $productRoot
                data_root = $dataRoot
                service_exe = $plan.paths.service_exe
            }
        } | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $plan.paths.manifest_path -Encoding UTF8
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{ ok = $true; body = $Plan.product_root }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        $combined = Get-ChildItem -LiteralPath $bundle.path -File |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } |
            Out-String
        foreach ($rawPath in @($script:RepoRoot, $productRoot, $dataRoot)) {
            $escapedJsonPath = ($rawPath | ConvertTo-Json -Compress).Trim('"')
            $combined | Should -Not -Match ([regex]::Escape($rawPath))
            $combined | Should -Not -Match ([regex]::Escape($escapedJsonPath))
        }
        $combined | Should -Match '\[SOURCE_ROOT\]'
        $combined | Should -Match '\[PRODUCT_ROOT\]'
        $combined | Should -Match '\[DATA_ROOT\]'
    }

    It 'runs CollectDiagnostics through the product action orchestrator' {
        $outRoot = Join-Path $TestDrive 'diagnostics-action'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeAction') `
            -DataRoot (Join-Path $TestDrive 'data-action')

        New-Item -ItemType Directory -Path $plan.data_root -Force | Out-Null
        Set-Content -LiteralPath $plan.paths.event_log -Value '{"Authorization":"Bearer action-secret","message":"boot"}' -Encoding UTF8

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -CollectDiagnostics {
                param($Plan)
                $runner = {
                    param([string]$FileName, [string[]]$Arguments)
                    [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
                }
                $runtimePolicy = {
                    param($Plan)
                    [ordered]@{ ok = $true; body = '{"message":"ok"}' }
                }
                New-PcvDesktopNodeDiagnosticBundle `
                    -Plan $Plan `
                    -OutputRoot $outRoot `
                    -InvokeProcess $runner `
                    -CollectRuntimePolicy $runtimePolicy
            }

        $result.ok | Should -BeTrue
        $result.action | Should -Be 'CollectDiagnostics'
        $diagnosticsStep = @($result.executed | Where-Object { $_.step -eq 'diagnostics' })[0]
        Test-Path -LiteralPath $diagnosticsStep.result.path | Should -BeTrue
        $diagnosticsStep.result.actual_execution | Should -Be 'code-level-product-wrapper'
        $diagnosticsStep.result.diagnostic_bundle_product_wrapper_delegation | Should -Be 'code-level-product-action-orchestrator'
        $diagnosticsStep.result.host_mutation_performed | Should -BeFalse
        $delegationStatusPath = Join-Path $diagnosticsStep.result.path 'product-wrapper-delegation-redacted.json'
        Test-Path -LiteralPath $delegationStatusPath | Should -BeTrue
        $delegationStatus = Get-Content -LiteralPath $delegationStatusPath -Raw | ConvertFrom-Json
        $delegationStatus.actual_execution | Should -Be 'code-level-product-wrapper'
        $delegationStatus.diagnostic_bundle_product_wrapper_delegation | Should -Be 'code-level-product-action-orchestrator'
        $delegationStatus.public_trusted_signing | Should -Be 'not-claimed'
        $delegationStatus.external_stable_publication | Should -Be 'not-claimed'
        $combined = Get-ChildItem -LiteralPath $diagnosticsStep.result.path -File |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } |
            Out-String
        $combined | Should -Not -Match 'action-secret'
    }

    It 'includes redacted service host logs, status, and executable hash in diagnostics' {
        $outRoot = Join-Path $TestDrive 'diagnostics-service-host'
        $productRoot = Join-Path $TestDrive 'DesktopNodeServiceHostDiag'
        $dataRoot = Join-Path $TestDrive 'data-service-host-diag'
        $winSwSource = Join-Path $TestDrive 'winsw-diag.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -WinSwPath $winSwSource
        New-Item -ItemType Directory -Path $plan.paths.service_logs_root -Force | Out-Null
        New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
        Set-Content -LiteralPath $plan.paths.service_exe -Value 'fake-dotnet-host' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'DesktopNode.Host.log') -Value 'host started Authorization: Bearer diag-secret' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.out.log') -Value 'stdout ready' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.err.log') -Value 'stderr ready' -Encoding UTF8

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Started'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{ ok = $true; body = '{"ok":true}' }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        Test-Path -LiteralPath (Join-Path $bundle.path 'service-host-status-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'service-host-metadata-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'service-log-DesktopNode.Host.log') | Should -BeTrue
        $combined = Get-ChildItem -LiteralPath $bundle.path -File |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } |
            Out-String
        $combined | Should -Match 'Started'
        $combined | Should -Match 'stdout ready'
        $combined | Should -Not -Match 'diag-secret'
        $combined | Should -Match '\[PRODUCT_ROOT\]'
        $combined | Should -Match '\[DATA_ROOT\]'
        $combined | Should -Match 'staged_sha256'
    }

    It 'summarizes Phase 23 service recovery and log retention evidence without mutating the host' {
        $outRoot = Join-Path $TestDrive 'diagnostics-operational-evidence'
        $productRoot = Join-Path $TestDrive 'DesktopNodeOperationalEvidence'
        $dataRoot = Join-Path $TestDrive 'data-operational-evidence'
        $winSwSource = Join-Path $TestDrive 'winsw-operational.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -WinSwPath $winSwSource
        New-Item -ItemType Directory -Path $plan.paths.service_logs_root -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.wrapper.log') -Value 'wrapper started' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.out.log.1') -Value 'rotated stdout' -Encoding UTF8

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Stopped'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{ ok = $true; body = '{"ok":true}' }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        $evidencePath = Join-Path $bundle.path 'operational-evidence-redacted.json'
        Test-Path -LiteralPath $evidencePath | Should -BeTrue
        $evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
        $evidence.schema_version | Should -Be 1
        $evidence.host_mutation.service_mutation_performed | Should -BeFalse
        $evidence.host_mutation.event_log_registration_performed | Should -BeFalse
        $evidence.service_recovery.source | Should -Be 'scm-failure-actions'
        $evidence.service_recovery.on_failure.configured | Should -BeTrue
        $evidence.service_recovery.on_failure.actions[0].action | Should -Be 'restart'
        $evidence.service_recovery.on_failure.actions[0].delay | Should -Be '60000 ms'
        $evidence.service_recovery.on_failure.actions[2].action | Should -Be 'none'
        $evidence.service_logs.retention.max_file_bytes | Should -Be 10485760
        $evidence.service_logs.retention.retained_files | Should -Be 10
        $evidence.service_logs.observed.count | Should -Be 2
        @($evidence.service_logs.observed.files.diagnostic_artifact) | Should -Contain 'service-log-PureCVisorDesktopNode.wrapper.log'

        $manifest = Get-Content -LiteralPath (Join-Path $bundle.path 'diagnostics-manifest.json') -Raw | ConvertFrom-Json
        @($manifest.sources.name) | Should -Contain 'operational_evidence'
    }

    It 'uses installed root-level .NET host paths when collecting diagnostics from an MSI layout' {
        $outRoot = Join-Path $TestDrive 'diagnostics-installed-service-host'
        $productRoot = Join-Path $TestDrive 'DesktopNodeInstalledServiceHostDiag'
        $dataRoot = Join-Path $TestDrive 'data-installed-service-host-diag'
        $installedHost = Join-Path $productRoot 'DesktopNode.Host.exe'
        New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
        Set-Content -LiteralPath $installedHost -Value 'fake-installed-host-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Installed status'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{ ok = $true; body = '{"ok":true}' }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        $metadata = Get-Content -LiteralPath (Join-Path $bundle.path 'service-host-metadata-redacted.json') -Raw | ConvertFrom-Json
        $metadata.staged_sha256 | Should -Be (Get-PcvFileSha256 -Path $installedHost)
        $metadata.staged_path | Should -Match '\[PRODUCT_ROOT\]'
        $metadata.staged_path | Should -Match 'DesktopNode\.Host\.exe'
        $metadata.config_path | Should -BeNullOrEmpty
    }

    It 'writes a versioned diagnostics manifest with redacted policy and source artifacts' {
        $outRoot = Join-Path $TestDrive 'diagnostics-manifest'
        $productRoot = Join-Path $TestDrive 'DesktopNodeManifestDiag'
        $dataRoot = Join-Path $TestDrive 'data-manifest-diag'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot
        New-Item -ItemType Directory -Path $plan.data_root -Force | Out-Null
        [ordered]@{
            Authorization = 'Bearer manifest-secret'
            path = $dataRoot
        } | ConvertTo-Json -Compress |
            Set-Content -LiteralPath $plan.paths.event_log -Encoding UTF8

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Stopped'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{ ok = $false; error = [ordered]@{ detail = $Plan.product_root } }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        $manifestPath = Join-Path $bundle.path 'diagnostics-manifest.json'
        Test-Path -LiteralPath $manifestPath | Should -BeTrue
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        $manifest.schema_version | Should -Be 1
        $manifest.decision | Should -Be 'DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred'
        $manifest.policy.mode | Should -Be 'windows-event-log-default-jsonl-retained'
        $manifest.policy.windows_event_log.enabled_by_default | Should -BeTrue
        $manifest.policy.windows_event_log.default_writer | Should -Be 'windows-event-log'
        $manifest.policy.windows_event_log.schema_version | Should -Be 1
        @($manifest.sources.name) | Should -Contain 'summary'
        @($manifest.sources.name) | Should -Contain 'events'

        $manifestJson = $manifest | ConvertTo-Json -Depth 16
        $manifestJson | Should -Not -Match 'manifest-secret'
        $manifestJson | Should -Not -Match ([regex]::Escape($productRoot))
        $manifestJson | Should -Not -Match ([regex]::Escape($dataRoot))
    }

    It 'includes LAN security policy in diagnostic bundle manifest without enabling LAN' {
        $outRoot = Join-Path $TestDrive 'diagnostics-lan-policy'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeLanDiag') `
            -DataRoot (Join-Path $TestDrive 'data-lan-diag')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Stopped'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{
                ok = $true
                body = '{"network":{"current_exposure":"loopback","tls":{"required_for_lan":true}}}'
            }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        $manifest = Get-Content -LiteralPath (Join-Path $bundle.path 'diagnostics-manifest.json') -Raw | ConvertFrom-Json
        $manifest.policy.network.default_exposure | Should -Be 'loopback'
        $manifest.policy.network.lan_mode.enabled_by_default | Should -BeFalse
        $manifest.policy.network.tls.required_for_lan | Should -BeTrue
        @($manifest.sources.name) | Should -Contain 'lan_security_policy'
    }

    It 'self-audits the Phase 24 runtime policy contract in diagnostic bundles' {
        $outRoot = Join-Path $TestDrive 'diagnostics-runtime-self-audit'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeRuntimeAudit') `
            -DataRoot (Join-Path $TestDrive 'data-runtime-audit')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Stopped'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{
                ok = $true
                body = (@{
                    ok = $true
                    operation = 'runtime.policy'
                    data = @{
                        job_runtime = @{
                            contract_version = 1
                            owner = 'local-api'
                            state_store = @{
                                persistence = 'json-file-snapshot'
                            }
                            dispatch = @{
                                helper_boundary = 'dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation'
                                native_probe_operations = @('host.status', 'network.inventory', 'vm.list', 'checkpoint.list')
                                native_mutation_operations = @('vm.create', 'vm.start', 'vm.shutdown', 'vm.poweroff', 'vm.restart', 'vm.delete', 'checkpoint.create', 'checkpoint.restore', 'checkpoint.delete')
                                mutation_dispatch = 'native-vm-create-lifecycle-delete-checkpoint-mutation'
                            }
                            control = @{
                                retry = @{
                                    failed_error_retryable_only = $true
                                }
                            }
                            host_mutation = 'native-read-routes-vm-create-lifecycle-delete-and-checkpoint-mutation'
                        }
                    }
                } | ConvertTo-Json -Depth 16 -Compress)
            }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy

        $selfAudit = Get-Content -LiteralPath (Join-Path $bundle.path 'diagnostics-self-audit.json') -Raw | ConvertFrom-Json
        $selfAudit.schema_version | Should -Be 1
        $selfAudit.runtime_policy.available | Should -BeTrue
        $selfAudit.runtime_policy.job_runtime.present | Should -BeTrue
        $selfAudit.runtime_policy.job_runtime.contract_version | Should -Be 1
        $selfAudit.runtime_policy.job_runtime.owner | Should -Be 'local-api'
        $selfAudit.runtime_policy.job_runtime.contract_ok | Should -BeTrue

        $manifest = Get-Content -LiteralPath (Join-Path $bundle.path 'diagnostics-manifest.json') -Raw | ConvertFrom-Json
        @($manifest.sources.name) | Should -Contain 'diagnostics_self_audit'
        $manifest.self_audit.runtime_policy.job_runtime.contract_ok | Should -BeTrue
    }

    It 'includes update policy and migration artifacts in diagnostic bundle manifest' {
        $outRoot = Join-Path $TestDrive 'diagnostics-update'
        $productRoot = Join-Path $TestDrive 'product-update-diag'
        $dataRoot = Join-Path $TestDrive 'data-update-diag'
        New-Item -ItemType Directory -Path $outRoot, $productRoot, $dataRoot -Force | Out-Null

        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'
        [ordered]@{
            schema_version = 1
            action = 'Update'
            status = 'failed-rolled-back'
            stage = 'service-start'
            product_root = $productRoot
            data_root = $dataRoot
            error = [ordered]@{
                code = 'PCV_PRODUCT_UPDATE_START_FAILED'
                message = 'Desktop Node product update service start failed.'
            }
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $plan.paths.update_transaction_journal -Encoding UTF8
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Stopped'; stderr = '' }
        }
        $runtimePolicy = {
            param($Plan)
            [ordered]@{ ok = $true; body = '{"ok":true}' }
        }

        $bundle = New-PcvDesktopNodeDiagnosticBundle `
            -Plan $plan `
            -OutputRoot $outRoot `
            -InvokeProcess $runner `
            -CollectRuntimePolicy $runtimePolicy
        $manifest = Get-Content -LiteralPath (Join-Path $bundle.path 'diagnostics-manifest.json') -Raw | ConvertFrom-Json

        $manifest.policy.update.decision | Should -Be 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
        @($manifest.sources.name) | Should -Contain 'update_policy'
        @($manifest.sources.name) | Should -Contain 'migration_plan'
        @($manifest.sources.name) | Should -Contain 'rollback_state'
        @($manifest.sources.name) | Should -Contain 'update_transaction_journal'

        Test-Path -LiteralPath (Join-Path $bundle.path 'update-policy-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'migration-plan-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'rollback-state-redacted.json') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $bundle.path 'update-transaction-journal-redacted.json') | Should -BeTrue

        $migration = Get-Content -LiteralPath (Join-Path $bundle.path 'migration-plan-redacted.json') -Raw | ConvertFrom-Json
        $migration.to_version | Should -Be '0.18.0-dev'
        $rollbackState = Get-Content -LiteralPath (Join-Path $bundle.path 'rollback-state-redacted.json') -Raw | ConvertFrom-Json
        $rollbackState.failed_root_preserved_for_diagnostics | Should -BeTrue
        $journal = Get-Content -LiteralPath (Join-Path $bundle.path 'update-transaction-journal-redacted.json') -Raw | ConvertFrom-Json
        $journal.status | Should -Be 'failed-rolled-back'
        $journal.error.code | Should -Be 'PCV_PRODUCT_UPDATE_START_FAILED'
    }

    It 'rotates JSONL and service logs according to diagnostics policy' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action CollectDiagnostics `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeRotate') `
            -DataRoot (Join-Path $TestDrive 'data-rotate')
        New-Item -ItemType Directory -Path $plan.data_root -Force | Out-Null
        New-Item -ItemType Directory -Path $plan.paths.service_logs_root -Force | Out-Null
        Set-Content -LiteralPath $plan.paths.event_log -Value ('e' * 32) -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath "$($plan.paths.event_log).1" -Value 'older-event' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.wrapper.log') -Value ('s' * 32) -Encoding UTF8 -NoNewline

        $result = Invoke-PcvDesktopNodeLogRotation `
            -Plan $plan `
            -MaxFileBytes 16 `
            -RetainedFiles 2 `
            -ServiceMaxFileBytes 16 `
            -ServiceRetainedFiles 2

        $result.ok | Should -BeTrue
        Test-Path -LiteralPath $plan.paths.event_log | Should -BeFalse
        Test-Path -LiteralPath "$($plan.paths.event_log).1" | Should -BeTrue
        Test-Path -LiteralPath "$($plan.paths.event_log).2" | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.wrapper.log.1') | Should -BeTrue
        @($result.rotated | Where-Object { $_.name -eq 'event_log' }).Count | Should -Be 1
        @($result.rotated | Where-Object { $_.name -eq 'service_log' }).Count | Should -Be 1
    }

    It 'builds a default Windows Event Log registration plan without mutating the host' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Plan `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeEventLog') `
            -DataRoot (Join-Path $TestDrive 'data-eventlog')

        $eventLogPlan = New-PcvDesktopNodeEventLogRegistrationPlan -Plan $plan

        $eventLogPlan.enabled_by_default | Should -BeTrue
        $eventLogPlan.registration_owner | Should -Be 'msi-deferred-local-system-native-service-action'
        $eventLogPlan.log_name | Should -Be 'Application'
        $eventLogPlan.source | Should -Be 'PureCVisor Desktop Node'
        $eventLogPlan.native_owner | Should -Be 'dotnet-host-service-action'
        $eventLogPlan.commands.default_transition.file_name | Should -Be $plan.paths.service_exe
        $eventLogPlan.commands.default_transition.arguments -join ' ' | Should -Be 'service-action eventlog-default-transition'
        $eventLogPlan.commands.register.file_name | Should -Be $plan.paths.service_exe
        $eventLogPlan.commands.register.arguments -join ' ' | Should -Be 'service-action eventlog-register'
        $eventLogPlan.commands.unregister.file_name | Should -Be $plan.paths.service_exe
        $eventLogPlan.commands.unregister.arguments -join ' ' | Should -Be 'service-action eventlog-remove'
        $eventLogPlan.commands.register.file_name | Should -Not -Match 'powershell\.exe'
        $eventLogPlan.commands.register.arguments -join ' ' | Should -Not -Match 'New-EventLog|Remove-EventLog'
    }
}
