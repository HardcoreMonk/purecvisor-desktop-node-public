Set-StrictMode -Version Latest

Describe 'PcvDesktopNodeProduct entrypoint command surface' {
    BeforeAll {
        $script:RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')
        $script:Entrypoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1'
        $script:ModulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        Import-Module $script:ModulePath -Force
    }

    function script:New-PcvTestUpdatePayload {
        param(
            [Parameter(Mandatory)][string]$Version,
            [Parameter(Mandatory)][string]$ProductRoot,
            [Parameter(Mandatory)][string]$DataRoot
        )

        $payloadRoot = Join-Path $TestDrive ("payload-$($Version -replace '[^A-Za-z0-9_.-]', '-')-$([guid]::NewGuid().ToString('N'))")
        New-Item -ItemType Directory -Path (Join-Path $payloadRoot 'web') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $payloadRoot 'DesktopNode.Host.exe') -Value 'fake-host' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe') -Value 'fake-cli' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'Invoke-PcvDesktopNodeProduct.ps1') -Value 'fake-entrypoint' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'PcvDesktopNodeProduct.psm1') -Value 'fake-module' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\app.js') -Value 'console.log("pcv");' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\index.html') -Value '<div id="app"></div>' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\styles.css') -Value 'body{}' -Encoding UTF8 -NoNewline

        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $payloadRoot `
            -ProductRoot $ProductRoot `
            -DataRoot $DataRoot `
            -Version $Version
        $manifest | ConvertTo-Json -Depth 32 |
            Set-Content -LiteralPath (Join-Path $payloadRoot 'product-manifest.json') -Encoding UTF8

        $payloadRoot
    }

    It 'outputs a plan for the Plan action' {
        $output = & pwsh -NoProfile -File $script:Entrypoint `
            -Action Plan `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node'
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.action | Should -Be 'Plan'
        $json.product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode'
        $json.auth.api_token_source | Should -Be 'protected_file'
        $json.auth.api_token_storage | Should -Be 'dpapi-local-machine'
    }

    It 'keeps the product entrypoint on the .NET Windows service host plan' {
        $winSwSource = Join-Path $TestDrive 'winsw-entrypoint.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $output = & pwsh -NoProfile -File $script:Entrypoint `
            -Action Plan `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WinSwPath $winSwSource
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.service.mode | Should -Be 'dotnet-windows-service'
        $json.service.host.default_owner | Should -Be 'dotnet-windows-service-host'
        $json.service.PSObject.Properties.Name | Should -Not -Contain 'winsw'
        $json.service.config.binary_path | Should -Match 'DesktopNode\.Host\.exe'
    }

    It 'defaults SourceRoot to the installed MSI payload root when no repo layout exists' {
        $installedRoot = Join-Path $TestDrive 'InstalledPayloadRoot'
        $dataRoot = Join-Path $TestDrive 'InstalledPayloadData'
        New-Item -ItemType Directory -Path $installedRoot -Force | Out-Null
        Copy-Item -LiteralPath $script:Entrypoint -Destination (Join-Path $installedRoot 'Invoke-PcvDesktopNodeProduct.ps1') -Force
        Copy-Item -LiteralPath $script:ModulePath -Destination (Join-Path $installedRoot 'PcvDesktopNodeProduct.psm1') -Force
        Set-Content -LiteralPath (Join-Path $installedRoot 'DesktopNode.Host.exe') -Value 'fake-dotnet-host' -Encoding UTF8 -NoNewline

        $output = & pwsh -NoProfile -File (Join-Path $installedRoot 'Invoke-PcvDesktopNodeProduct.ps1') `
            -Action Plan `
            -ProductRoot $installedRoot `
            -DataRoot $dataRoot
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.source_root | Should -Be $installedRoot
        $json.service.mode | Should -Be 'dotnet-windows-service'
        $json.service.host.executable_path | Should -Be (Join-Path $installedRoot 'DesktopNode.Host.exe')
        $json.service.config.binary_path | Should -Match ([regex]::Escape("--web-root `"$installedRoot\web`""))
        Test-Path -LiteralPath (Join-Path $installedRoot 'service/PcvDesktopService.psm1') | Should -BeFalse
    }

    It 'runs product native process commands under Windows PowerShell' {
        $scriptPath = Join-Path $TestDrive 'invoke-native-process.ps1'
        $escapedModulePath = $script:ModulePath -replace "'", "''"
        Set-Content -LiteralPath $scriptPath -Encoding UTF8 -Value @"
`$ErrorActionPreference = 'Stop'
Import-Module '$escapedModulePath' -Force
`$commands = @(
    [ordered]@{
        file_name = `$env:ComSpec
        arguments = @('/c', 'echo', 'pcv-native-ok')
    }
)
Invoke-PcvProductProcessCommand -Commands `$commands | ConvertTo-Json -Depth 16
"@

        $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.ok | Should -BeTrue
        $json.results[0].exit_code | Should -Be 0
        $json.results[0].stdout.Trim() | Should -Be 'pcv-native-ok'
    }

    It 'handles localized access denied retry text under Windows PowerShell' {
        $scriptPath = Join-Path $TestDrive 'invoke-localized-remove-retry.ps1'
        $escapedModulePath = $script:ModulePath -replace "'", "''"
        $escapedRepoRoot = $script:RepoRoot -replace "'", "''"
        Set-Content -LiteralPath $scriptPath -Encoding UTF8 -Value @"
`$ErrorActionPreference = 'Stop'
Import-Module '$escapedModulePath' -Force
`$dataRoot = Join-Path '$TestDrive' 'data-windows-powershell-localized-remove'
`$plan = New-PcvDesktopNodeProductPlan -Action RemoveInstalled -SourceRoot '$escapedRepoRoot' -ProductRoot (Join-Path '$TestDrive' 'DesktopNodeWinPsLocalizedRemove') -DataRoot `$dataRoot -RemoveData
`$localizedAccessDenied = [regex]::Unescape('\uC561\uC138\uC2A4\uAC00 \uAC70\uBD80\uB418\uC5C8\uC2B5\uB2C8\uB2E4')
`$removeAttempts = 0
`$result = Invoke-PcvDesktopNodeProductAction -Plan `$plan -InvokeProcess {
    param([string]`$FileName, [string[]]`$Arguments)
    if (`$Arguments[0] -eq 'query') {
        return [ordered]@{ exit_code = 0; stdout = "Stopped``r``n"; stderr = '' }
    }
    [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
} -RemovePath {
    param([string]`$Path)
    `$script:removeAttempts += 1
    if (`$script:removeAttempts -eq 1) {
        return [ordered]@{
            ok = `$false
            path = `$Path
            error = [ordered]@{
                code = 'PCV_PRODUCT_REMOVE_FAILED'
                message = 'Desktop Node product path removal failed.'
                detail = `$localizedAccessDenied
            }
        }
    }
    [ordered]@{ ok = `$true; path = `$Path; removed = `$true }
}
`$result | ConvertTo-Json -Depth 20
"@

        $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.ok | Should -BeTrue
        $firstRemove = @($json.executed | Where-Object { $_.step -eq 'remove' })[0].result[0]
        $firstRemove.attempt_count | Should -Be 2
    }

    It 'outputs a dry-run result for Install with WhatIf' {
        $output = & pwsh -NoProfile -File $script:Entrypoint `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WhatIf
        $exitCode = $LASTEXITCODE
        $json = $output | ConvertFrom-Json

        $exitCode | Should -Be 0
        $json.action | Should -Be 'Install'
        $json.dry_run | Should -BeTrue
        $json.execution_skipped | Should -BeTrue
    }

    It 'invokes native process commands without ProcessStartInfo ArgumentList' {
        $moduleText = Get-Content -Raw -LiteralPath $script:ModulePath
        $commands = @(
            [ordered]@{
                file_name = 'powershell.exe'
                arguments = @(
                    '-NoProfile',
                    '-Command',
                    '[Console]::Out.Write("value with spaces")'
                )
            }
        )

        $moduleText | Should -Not -Match '\.ArgumentList\b'
        $result = Invoke-PcvProductProcessCommand -Commands $commands

        $result.ok | Should -BeTrue
        $result.results[0].stdout | Should -Be 'value with spaces'
    }

    It 'blocks automatic reboot capable commands before process execution' {
        $called = $false
        $commands = @(
            [ordered]@{
                file_name = 'Restart-Computer'
                arguments = @()
            }
        )

        $result = Invoke-PcvProductProcessCommand -Commands $commands -InvokeProcess {
            $called = $true
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }

        $called | Should -BeFalse
        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_AUTO_REBOOT_FORBIDDEN'
        $result.error.detail | Should -Match 'Restart-Computer'
    }

    It 'rejects Update when installed product manifest is missing' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        $result = Invoke-PcvDesktopNodeProductAction -Plan $plan

        $result.ok | Should -BeFalse
        $result.action | Should -Be 'Update'
        $result.error.code | Should -Be 'PCV_PRODUCT_MANIFEST_MISSING'
    }

    It 'fails closed when the pending-commit marker cannot be inspected' {
        $module = Get-Module PcvDesktopNodeProduct
        $invalidPath = "invalid$([char]0)pending-commit"

        $guard = & $module {
            param([string]$Path)
            Test-PcvDesktopNodeJobStorePendingCommitGuard -Path $Path
        } $invalidPath

        $guard.ok | Should -BeFalse
        $guard.status | Should -Be 'inspection-failed'
        $guard.pending_commit_present | Should -BeNullOrEmpty
        $guard.error.code | Should -Be 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
    }

    It 'blocks Update before product-root backup when a pending commit exists after service stop' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdatePendingCommit'
        $dataRoot = Join-Path $TestDrive 'data-update-pending-commit'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.18.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot
        $calls = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'
        Set-Content -LiteralPath $plan.paths.job_store_pending_commit -Value '{"version":1}' -Encoding UTF8

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param([string]$FileName, [string[]]$Arguments)
                $calls.Add("$FileName $($Arguments -join ' ')")
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -BackupProductRoot {
                throw 'Product-root backup must not run while a pending commit exists.'
            } `
            -CopyAssets {
                throw 'Asset copy must not run while a pending commit exists.'
            } `
            -TestHealth {
                throw 'Health must not run while a pending commit exists.'
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
        @($result.executed.step) | Should -Contain 'service.stop.wait'
        @($result.executed.step) | Should -Contain 'job-store.pending-commit.guard-before-backup'
        @($result.executed.step) | Should -Not -Contain 'backup-product-root'
        @($result.executed.step) | Should -Not -Contain 'service.start'
        @($result.executed.step) | Should -Not -Contain 'health'
        $guard = @($result.executed | Where-Object { $_.step -eq 'job-store.pending-commit.guard-before-backup' })[0]
        $guard.result.status | Should -Be 'present'
        Test-Path -LiteralPath $plan.paths.job_store_pending_commit | Should -BeTrue
    }

    It 'blocks service start when config migration validation fails during Update' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeMigrationFailure'
        $dataRoot = Join-Path $TestDrive 'data-migration-failure'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.18.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'start') {
                throw 'Service start must be blocked before migration succeeds.'
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            [ordered]@{ ok = $true; copied = $true; product_root = $ProductRoot; version = $Version }
        }
        $migration = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            [ordered]@{
                schema_version = 1
                from_version = $FromVersion
                to_version = $ToVersion
                dry_run = $DryRun
                service_start_allowed = $false
                steps = @([ordered]@{ name = 'validate-job-store-schema'; mutation = $false; required = $true; status = 'failed' })
                error = [ordered]@{ code = 'PCV_PRODUCT_CONFIG_MIGRATION_BLOCKED'; message = 'Config migration validation failed.' }
            }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -CopyAssets $copy `
            -NewConfigMigrationPlan $migration

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_CONFIG_MIGRATION_BLOCKED'
        @($result.executed.step) | Should -Not -Contain 'service.start'
    }

    It 'orchestrates Update with manifest validation, migration dry-run, service start, and health check' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdate'
        $dataRoot = Join-Path $TestDrive 'data-update'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.18.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot

        $calls = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'stop') {
                $calls.Add('service.stop')
            }
            elseif ($Arguments[0] -eq 'query') {
                $calls.Add('service.status')
            }
            elseif ($Arguments[0] -eq 'start') {
                $calls.Add('service.start')
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $backup = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('backup-product-root')
            [ordered]@{ ok = $true; backed_up = $true; product_root = $ProductRoot; previous_product_root = $PreviousProductRoot }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            $calls.Add('copy-assets')
            [ordered]@{ ok = $true; copied = $true; version = $Version }
        }
        $migration = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            $calls.Add('migration-plan')
            [ordered]@{ schema_version = 1; from_version = $FromVersion; to_version = $ToVersion; dry_run = $DryRun; service_start_allowed = $true; steps = @(); backups = @() }
        }
        $health = {
            param([string]$Prefix)
            $calls.Add('health-check')
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -BackupProductRoot $backup `
            -CopyAssets $copy `
            -NewConfigMigrationPlan $migration `
            -TestHealth $health

        $result.ok | Should -BeTrue
        $calls | Should -Be @(
            'service.stop',
            'service.status',
            'backup-product-root',
            'copy-assets',
            'migration-plan',
            'service.start',
            'health-check'
        )
        @($result.executed.step)[0..2] | Should -Be @('current-manifest', 'update-payload-preflight', 'update-transaction.begin')
        $result.update.from_version | Should -Be '0.17.0-dev'
        $result.update.to_version | Should -Be '0.18.0-dev'
        $result.update.rollback_attempted | Should -BeFalse
        $result.update.transaction_journal.path | Should -Be $plan.paths.update_transaction_journal
        $result.update.transaction_journal.status | Should -Be 'succeeded'
        Test-Path -LiteralPath $plan.paths.update_transaction_journal | Should -BeTrue
        $journal = Get-Content -LiteralPath $plan.paths.update_transaction_journal -Raw | ConvertFrom-Json
        $journal.schema_version | Should -Be 1
        $journal.action | Should -Be 'Update'
        $journal.status | Should -Be 'succeeded'
        $journal.stage | Should -Be 'health'
        $journal.from_version | Should -Be '0.17.0-dev'
        $journal.to_version | Should -Be '0.18.0-dev'
        $journal.service_mutation_started | Should -BeTrue
        $journal.host_mutation_performed | Should -BeTrue
        $journal.rollback_attempted | Should -BeFalse
    }

    It 'resolves a verified file URI update package before mutating Update' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdatePackage'
        $dataRoot = Join-Path $TestDrive 'data-update-package'
        $downloadRoot = Join-Path $TestDrive 'update-downloads'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.18.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.19.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot
        $zipPath = Join-Path $TestDrive 'PureCVisorDesktopNode-0.19.0-dev.zip'
        Compress-Archive -Path (Join-Path $payloadRoot '*') -DestinationPath $zipPath
        $expectedSha256 = Get-PcvFileSha256 -Path $zipPath
        $sourceUri = ([System.Uri](Get-Item -LiteralPath $zipPath).FullName).AbsoluteUri

        $calls = [System.Collections.Generic.List[string]]::new()
        $script:capturedCopySourceRoot = $null
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -SourceUri $sourceUri `
            -ExpectedSha256 $expectedSha256 `
            -DownloadRoot $downloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.19.0-dev'

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'stop') {
                $calls.Add('service.stop')
            }
            elseif ($Arguments[0] -eq 'query') {
                $calls.Add('service.status')
            }
            elseif ($Arguments[0] -eq 'start') {
                $calls.Add('service.start')
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $backup = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('backup-product-root')
            [ordered]@{ ok = $true; backed_up = $true; product_root = $ProductRoot; previous_product_root = $PreviousProductRoot }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            $script:capturedCopySourceRoot = $SourceRoot
            $calls.Add('copy-assets')
            [ordered]@{ ok = $true; copied = $true; source_root = $SourceRoot; version = $Version }
        }
        $migration = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            $calls.Add('migration-plan')
            [ordered]@{ schema_version = 1; from_version = $FromVersion; to_version = $ToVersion; dry_run = $DryRun; service_start_allowed = $true; steps = @(); backups = @() }
        }
        $health = {
            param([string]$Prefix)
            $calls.Add('health-check')
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -BackupProductRoot $backup `
            -CopyAssets $copy `
            -NewConfigMigrationPlan $migration `
            -TestHealth $health

        $result.ok | Should -BeTrue
        $sourceStep = @($result.executed | Where-Object { $_.step -eq 'update-source-preflight' })[0]
        $payloadStep = @($result.executed | Where-Object { $_.step -eq 'update-payload-preflight' })[0]
        $sourceStep.result.ok | Should -BeTrue
        $sourceStep.result.actual_sha256 | Should -Be $expectedSha256
        $sourceStep.result.source_root | Should -Match ([regex]::Escape($downloadRoot))
        Test-Path -LiteralPath (Join-Path $sourceStep.result.source_root 'product-manifest.json') | Should -BeTrue
        $payloadStep.result.source_root | Should -Be $sourceStep.result.source_root
        $script:capturedCopySourceRoot | Should -Be $sourceStep.result.source_root
        @($result.executed.step)[0..2] | Should -Be @('current-manifest', 'update-source-preflight', 'update-payload-preflight')
        $calls | Should -Contain 'service.stop'
        $result.update.update_source.source_root | Should -Be $sourceStep.result.source_root
    }

    It 'blocks Update before mutation when the payload is missing the installed CLI' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateMissingCli'
        $dataRoot = Join-Path $TestDrive 'data-update-missing-cli'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.18.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.19.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot
        Remove-Item -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe') -Force

        $calls = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.19.0-dev'

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $calls.Add("$FileName $($Arguments -join ' ')")
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_PAYLOAD_FILE_MISSING'
        $result.error.detail | Should -Match 'pcvcli\.exe'
        $calls.Count | Should -Be 0
        @($result.executed.step) | Should -Contain 'update-payload-preflight'
    }


    It 'resolves a full updater catalog channel before mutating Update' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateCatalog'
        $dataRoot = Join-Path $TestDrive 'data-update-catalog'
        $downloadRoot = Join-Path $TestDrive 'catalog-downloads'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.18.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.20.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot
        $zipPath = Join-Path $TestDrive 'PureCVisorDesktopNode-0.20.0-dev.zip'
        Compress-Archive -Path (Join-Path $payloadRoot '*') -DestinationPath $zipPath
        $expectedSha256 = Get-PcvFileSha256 -Path $zipPath
        $packageUri = ([System.Uri](Get-Item -LiteralPath $zipPath).FullName).AbsoluteUri
        $catalogPath = Join-Path $TestDrive 'catalog.json'
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            publication = [ordered]@{
                public_trusted_signing = 'not-claimed'
                external_stable_publication = 'not-claimed'
            }
            channels = @(
                [ordered]@{
                    name = 'internal-dev'
                    version = '0.20.0-dev'
                    package_uri = $packageUri
                    sha256 = $expectedSha256
                    release_channel = 'dev'
                    signing_mode = 'AllowUnsignedDev'
                }
            )
        } | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $catalogPath -Encoding UTF8

        $calls = [System.Collections.Generic.List[string]]::new()
        $script:capturedCatalogCopySourceRoot = $null
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -UpdateCatalogUri ([System.Uri](Get-Item -LiteralPath $catalogPath).FullName).AbsoluteUri `
            -UpdateChannel 'internal-dev' `
            -DownloadRoot $downloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'stop') {
                $calls.Add('service.stop')
            }
            elseif ($Arguments[0] -eq 'query') {
                $calls.Add('service.status')
            }
            elseif ($Arguments[0] -eq 'start') {
                $calls.Add('service.start')
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $backup = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('backup-product-root')
            [ordered]@{ ok = $true; backed_up = $true; product_root = $ProductRoot; previous_product_root = $PreviousProductRoot }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            $script:capturedCatalogCopySourceRoot = $SourceRoot
            $calls.Add('copy-assets')
            [ordered]@{ ok = $true; copied = $true; source_root = $SourceRoot; version = $Version }
        }
        $migration = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            $calls.Add('migration-plan')
            [ordered]@{ schema_version = 1; from_version = $FromVersion; to_version = $ToVersion; dry_run = $DryRun; service_start_allowed = $true; steps = @(); backups = @() }
        }
        $health = {
            param([string]$Prefix)
            $calls.Add('health-check')
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -BackupProductRoot $backup `
            -CopyAssets $copy `
            -NewConfigMigrationPlan $migration `
            -TestHealth $health

        $result.ok | Should -BeTrue
        @($result.executed.step)[0..3] | Should -Be @('current-manifest', 'update-catalog-preflight', 'update-source-preflight', 'update-payload-preflight')
        $catalogStep = @($result.executed | Where-Object { $_.step -eq 'update-catalog-preflight' })[0]
        $sourceStep = @($result.executed | Where-Object { $_.step -eq 'update-source-preflight' })[0]
        $catalogStep.result.ok | Should -BeTrue
        $catalogStep.result.channel | Should -Be 'internal-dev'
        $catalogStep.result.version | Should -Be '0.20.0-dev'
        $catalogStep.result.publication.external_stable_publication | Should -Be 'not-claimed'
        $sourceStep.result.expected_sha256 | Should -Be $expectedSha256
        $result.update.to_version | Should -Be '0.20.0-dev'
        $result.update.update_catalog.channel | Should -Be 'internal-dev'
        $script:capturedCatalogCopySourceRoot | Should -Be $sourceStep.result.source_root
        $journal = Get-Content -LiteralPath $plan.paths.update_transaction_journal -Raw | ConvertFrom-Json
        $journal.update_catalog.channel | Should -Be 'internal-dev'
        $journal.update_catalog.publication.external_stable_publication | Should -Be 'not-claimed'
        $calls | Should -Contain 'service.stop'
    }

    It 'blocks missing full updater catalog channels before service stop' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateCatalogMissing'
        $dataRoot = Join-Path $TestDrive 'data-update-catalog-missing'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.18.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $catalogPath = Join-Path $TestDrive 'missing-channel-catalog.json'
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            channels = @(
                [ordered]@{
                    name = 'internal-dev'
                    version = '0.20.0-dev'
                    package_uri = 'https://updates.example.invalid/PureCVisorDesktopNode.zip'
                    sha256 = '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef'
                }
            )
        } | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $catalogPath -Encoding UTF8

        $called = $false
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -UpdateCatalogUri ([System.Uri](Get-Item -LiteralPath $catalogPath).FullName).AbsoluteUri `
            -UpdateChannel 'internal-stable' `
            -DownloadRoot (Join-Path $TestDrive 'catalog-missing-downloads') `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                $called = $true
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_CATALOG_CHANNEL_NOT_FOUND'
        $called | Should -BeFalse
        @($result.executed.step) | Should -Contain 'update-catalog-preflight'
        @($result.executed.step) | Should -Not -Contain 'service.stop'
    }

    It 'blocks unsupported full updater catalog schemas before service stop' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateCatalogBadSchema'
        $dataRoot = Join-Path $TestDrive 'data-update-catalog-bad-schema'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.18.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $catalogPath = Join-Path $TestDrive 'bad-schema-catalog.json'
        [ordered]@{
            schema_version = 'future'
            product = 'PureCVisor Desktop Node'
            channels = @(
                [ordered]@{
                    name = 'internal-dev'
                    version = '0.20.0-dev'
                    package_uri = 'https://updates.example.invalid/PureCVisorDesktopNode.zip'
                    sha256 = '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef'
                }
            )
        } | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $catalogPath -Encoding UTF8

        $called = $false
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -UpdateCatalogUri ([System.Uri](Get-Item -LiteralPath $catalogPath).FullName).AbsoluteUri `
            -UpdateChannel 'internal-dev' `
            -DownloadRoot (Join-Path $TestDrive 'catalog-bad-schema-downloads') `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                $called = $true
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_CATALOG_SCHEMA_UNSUPPORTED'
        $called | Should -BeFalse
        @($result.executed.step) | Should -Contain 'update-catalog-preflight'
        @($result.executed.step) | Should -Not -Contain 'service.stop'
    }

    It 'blocks untrusted HTTP update package sources before service stop' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateHttpBlocked'
        $dataRoot = Join-Path $TestDrive 'data-update-http-blocked'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.18.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -SourceUri 'http://updates.example.invalid/PureCVisorDesktopNode.zip' `
            -ExpectedSha256 '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef' `
            -DownloadRoot (Join-Path $TestDrive 'blocked-downloads') `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.19.0-dev'

        $called = $false
        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                $called = $true
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED'
        $called | Should -BeFalse
        @($result.executed.step) | Should -Contain 'update-source-preflight'
        @($result.executed.step) | Should -Not -Contain 'service.stop'
    }

    It 'blocks update package download roots inside the active product root before service stop' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateDownloadRootBlocked'
        $dataRoot = Join-Path $TestDrive 'data-update-download-root-blocked'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.18.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8

        $packagePath = Join-Path $TestDrive 'blocked-root-package.zip'
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.19.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot
        Compress-Archive -Path (Join-Path $payloadRoot '*') -DestinationPath $packagePath
        $expectedSha256 = Get-PcvFileSha256 -Path $packagePath
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -SourceUri ([System.Uri](Get-Item -LiteralPath $packagePath).FullName).AbsoluteUri `
            -ExpectedSha256 $expectedSha256 `
            -DownloadRoot (Join-Path $productRoot 'updates') `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.19.0-dev'

        $called = $false
        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                $called = $true
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_DOWNLOAD_ROOT_ACTIVE_ROOT'
        $called | Should -BeFalse
        @($result.executed.step) | Should -Contain 'update-source-preflight'
        @($result.executed.step) | Should -Not -Contain 'service.stop'
        Test-Path -LiteralPath (Join-Path $productRoot 'updates') | Should -BeFalse
    }

    It 'restores previous product root when Update copy fails after backup' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateCopyFail'
        $dataRoot = Join-Path $TestDrive 'data-update-copy-fail'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.18.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot

        $calls = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'stop') {
                $calls.Add('service.stop')
            }
            elseif ($Arguments[0] -eq 'query') {
                $calls.Add('service.status')
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $backup = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('backup-product-root')
            [ordered]@{ ok = $true; backed_up = $true }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            $calls.Add('copy-assets')
            [ordered]@{ ok = $false; error = [ordered]@{ code = 'PCV_PRODUCT_COPY_FAILED'; message = 'copy failed'; detail = 'simulated copy failure' } }
        }
        $restore = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('restore-previous-root')
            [ordered]@{ ok = $true; restored = $true }
        }
        $migration = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            throw 'Migration must not run when copy fails.'
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -BackupProductRoot $backup `
            -CopyAssets $copy `
            -NewConfigMigrationPlan $migration `
            -RestorePreviousProductRoot $restore

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_COPY_FAILED'
        $calls | Should -Be @(
            'service.stop',
            'service.status',
            'backup-product-root',
            'copy-assets',
            'service.stop',
            'service.status',
            'restore-previous-root'
        )
        $rollbackSteps = @($result.executed.step)
        $rollbackSteps.IndexOf('rollback.service.stop') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.service.stop.wait')
        $rollbackSteps.IndexOf('rollback.service.stop.wait') | Should -BeLessThan $rollbackSteps.IndexOf('job-store.pending-commit.guard-before-automatic-rollback')
        $rollbackSteps.IndexOf('job-store.pending-commit.guard-before-automatic-rollback') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.restore')
        $result.update.rollback_attempted | Should -BeTrue
        $result.update.transaction_journal.status | Should -Be 'failed-rolled-back'
        Test-Path -LiteralPath $plan.paths.update_transaction_journal | Should -BeTrue
        $journal = Get-Content -LiteralPath $plan.paths.update_transaction_journal -Raw | ConvertFrom-Json
        $journal.status | Should -Be 'failed-rolled-back'
        $journal.stage | Should -Be 'copy'
        $journal.rollback_attempted | Should -BeTrue
        $journal.rollback_result.restored | Should -BeTrue
        $journal.error.code | Should -Be 'PCV_PRODUCT_COPY_FAILED'
    }

    It 'restores previous product root when Update service start fails' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateStartFail'
        $dataRoot = Join-Path $TestDrive 'data-update-start-fail'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.18.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot

        $calls = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'start') {
                $calls.Add('service.start')
                return [ordered]@{ exit_code = 5; stdout = ''; stderr = 'start failed' }
            }
            if ($Arguments[0] -eq 'stop') {
                $calls.Add('service.stop')
            }
            elseif ($Arguments[0] -eq 'query') {
                $calls.Add('service.status')
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $backup = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('backup-product-root')
            [ordered]@{ ok = $true; backed_up = $true }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            $calls.Add('copy-assets')
            [ordered]@{ ok = $true; copied = $true }
        }
        $migration = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            $calls.Add('migration-plan')
            [ordered]@{ service_start_allowed = $true; dry_run = $DryRun; steps = @(); backups = @() }
        }
        $restore = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('restore-previous-root')
            [ordered]@{ ok = $true; restored = $true }
        }
        $health = {
            param([string]$Prefix)
            throw 'Health must not run when service start fails.'
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -BackupProductRoot $backup `
            -CopyAssets $copy `
            -NewConfigMigrationPlan $migration `
            -RestorePreviousProductRoot $restore `
            -TestHealth $health

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_START_FAILED'
        $calls | Should -Contain 'restore-previous-root'
        $calls | Should -Be @(
            'service.stop',
            'service.status',
            'backup-product-root',
            'copy-assets',
            'migration-plan',
            'service.start',
            'service.stop',
            'service.status',
            'restore-previous-root'
        )
        $rollbackSteps = @($result.executed.step)
        $rollbackSteps.IndexOf('rollback.service.stop') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.service.stop.wait')
        $rollbackSteps.IndexOf('rollback.service.stop.wait') | Should -BeLessThan $rollbackSteps.IndexOf('job-store.pending-commit.guard-before-rollback-restore')
        $rollbackSteps.IndexOf('job-store.pending-commit.guard-before-rollback-restore') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.restore')
        $result.update.rollback_attempted | Should -BeTrue
        $result.update.transaction_journal.path | Should -Be $plan.paths.update_transaction_journal
        $result.update.transaction_journal.status | Should -Be 'failed-rolled-back'
        Test-Path -LiteralPath $plan.paths.update_transaction_journal | Should -BeTrue
        $journal = Get-Content -LiteralPath $plan.paths.update_transaction_journal -Raw | ConvertFrom-Json
        $journal.status | Should -Be 'failed-rolled-back'
        $journal.stage | Should -Be 'service-start'
        $journal.rollback_attempted | Should -BeTrue
        $journal.rollback_result.restored | Should -BeTrue
        $journal.error.code | Should -Be 'PCV_PRODUCT_UPDATE_START_FAILED'
        $journal.host_mutation_performed | Should -BeTrue
    }

    It 'restores previous product root when Update health check fails' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateHealthFail'
        $dataRoot = Join-Path $TestDrive 'data-update-health-fail'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.18.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot

        $calls = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'stop') {
                $calls.Add('service.stop')
            }
            elseif ($Arguments[0] -eq 'query') {
                $calls.Add('service.status')
            }
            elseif ($Arguments[0] -eq 'start') {
                $calls.Add('service.start')
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $backup = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('backup-product-root')
            [ordered]@{ ok = $true; backed_up = $true }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            $calls.Add('copy-assets')
            [ordered]@{ ok = $true; copied = $true }
        }
        $migration = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            $calls.Add('migration-plan')
            [ordered]@{ service_start_allowed = $true; dry_run = $DryRun; steps = @(); backups = @() }
        }
        $restore = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $calls.Add('restore-previous-root')
            [ordered]@{ ok = $true; restored = $true }
        }
        $health = {
            param([string]$Prefix)
            $calls.Add('health-check')
            [ordered]@{ ok = $false; status_code = 503 }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -BackupProductRoot $backup `
            -CopyAssets $copy `
            -NewConfigMigrationPlan $migration `
            -RestorePreviousProductRoot $restore `
            -TestHealth $health

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_HEALTH_CHECK_FAILED'
        $calls | Should -Contain 'restore-previous-root'
        $calls | Should -Be @(
            'service.stop',
            'service.status',
            'backup-product-root',
            'copy-assets',
            'migration-plan',
            'service.start',
            'health-check',
            'service.stop',
            'service.status',
            'restore-previous-root'
        )
        $rollbackSteps = @($result.executed.step)
        $rollbackSteps.IndexOf('health') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.service.stop')
        $rollbackSteps.IndexOf('rollback.service.stop') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.service.stop.wait')
        $rollbackSteps.IndexOf('rollback.service.stop.wait') | Should -BeLessThan $rollbackSteps.IndexOf('job-store.pending-commit.guard-before-rollback-restore')
        $rollbackSteps.IndexOf('job-store.pending-commit.guard-before-rollback-restore') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.restore')
        $result.update.rollback_attempted | Should -BeTrue
    }

    It 'rechecks pending commit after rollback stop wait when Update health fails' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeUpdateHealthPendingCommit'
        $dataRoot = Join-Path $TestDrive 'data-update-health-pending-commit'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Encoding UTF8
        $payloadRoot = New-PcvTestUpdatePayload -Version '0.18.0-dev' -ProductRoot $productRoot -DataRoot $dataRoot
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.18.0-dev'
        $calls = [System.Collections.Generic.List[string]]::new()
        $state = [ordered]@{ health_failed = $false }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param([string]$FileName, [string[]]$Arguments)
                $calls.Add("service.$($Arguments[0])")
                if ($Arguments[0] -eq 'query' -and [bool]$state.health_failed) {
                    Set-Content -LiteralPath $plan.paths.job_store_pending_commit -Value '{"version":1}' -Encoding UTF8
                }
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -BackupProductRoot {
                [ordered]@{ ok = $true; backed_up = $true }
            } `
            -CopyAssets {
                [ordered]@{ ok = $true; copied = $true }
            } `
            -NewConfigMigrationPlan {
                [ordered]@{ service_start_allowed = $true; steps = @(); backups = @() }
            } `
            -TestHealth {
                $state.health_failed = $true
                [ordered]@{ ok = $false; status_code = 503 }
            } `
            -RestorePreviousProductRoot {
                $calls.Add('restore')
                throw 'Automatic rollback restore must not run while a pending commit exists.'
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
        $calls | Should -Not -Contain 'restore'
        $result.update.rollback_attempted | Should -BeFalse
        $result.update.rollback_blocked.status | Should -Be 'present'
        @($result.executed.step) | Should -Contain 'job-store.pending-commit.guard-before-rollback-restore'
        @($result.executed.step) | Should -Contain 'job-store.pending-commit.guard-before-automatic-rollback'
        @($result.executed.step) | Should -Not -Contain 'rollback.restore'
        $rollbackSteps = @($result.executed.step)
        $rollbackSteps.IndexOf('health') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.service.stop')
        $rollbackSteps.IndexOf('rollback.service.stop') | Should -BeLessThan $rollbackSteps.IndexOf('rollback.service.stop.wait')
        $rollbackSteps.IndexOf('rollback.service.stop.wait') | Should -BeLessThan $rollbackSteps.IndexOf('job-store.pending-commit.guard-before-rollback-restore')
        Test-Path -LiteralPath $plan.paths.job_store_pending_commit | Should -BeTrue
    }

    It 'orchestrates Install in meaningful order with full service command arguments' {
        $steps = [System.Collections.Generic.List[object]]::new()
        $winSwSource = Join-Path $TestDrive 'winsw-install-test.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data') `
            -WinSwPath $winSwSource

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $steps.Add([ordered]@{
                    label = "$leafName $($Arguments[0])"
                    command_line = "$leafName $($Arguments -join ' ')"
                })
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath)
            $steps.Add([ordered]@{ label = 'copy'; command_line = "copy $ProductRoot $WinSwPath" })
            [ordered]@{ ok = $true; product_root = $ProductRoot; winsw = [ordered]@{ source_path = $WinSwPath } }
        }
        $token = {
            param([string]$Path)
            $steps.Add([ordered]@{ label = 'token'; command_line = "token $Path" })
            [ordered]@{ ok = $true; path = $Path }
        }
        $health = {
            param([string]$Prefix)
            $steps.Add([ordered]@{ label = 'health'; command_line = "health $Prefix" })
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -CopyAssets $copy `
            -PrepareTokenFile $token `
            -InvokeProcess $runner `
            -TestHealth $health

        $result.ok | Should -BeTrue
        $result.action | Should -Be 'Install'
        $result.executed.Count | Should -BeGreaterThan 0
        @($steps.label) | Should -Be @(
            'copy',
            'token',
            'sc.exe create',
            'sc.exe description',
            'sc.exe failure',
            'sc.exe start',
            'health'
        )
        $steps[2].command_line | Should -Match 'sc\.exe create PureCVisorDesktopNode'
        $steps[3].command_line | Should -Match 'sc\.exe description PureCVisorDesktopNode'
        $steps[4].command_line | Should -Match 'sc\.exe failure PureCVisorDesktopNode'
        $steps[5].command_line | Should -Be 'sc.exe start PureCVisorDesktopNode'
        $steps[6].command_line | Should -Be 'health http://127.0.0.1:7777/'
    }

    It 'uses the protected token file bearer token during the default product health check' {
        $script:CapturedHealthUri = $null
        $script:CapturedHealthHeaders = $null
        $script:CapturedHealthUseBasicParsing = $false
        Mock -CommandName Invoke-WebRequest -ModuleName PcvDesktopNodeProduct -MockWith {
            param(
                [string]$Uri,
                [hashtable]$Headers,
                [int]$TimeoutSec,
                [switch]$UseBasicParsing
            )
            $script:CapturedHealthUri = $Uri
            $script:CapturedHealthHeaders = $Headers
            $script:CapturedHealthUseBasicParsing = $UseBasicParsing.IsPresent
            [pscustomobject]@{ StatusCode = 200 }
        }

        $winSwSource = Join-Path $TestDrive 'winsw-default-health.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
        $dataRoot = Join-Path $TestDrive 'data-default-health'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeDefaultHealth') `
            -DataRoot $dataRoot `
            -WinSwPath $winSwSource
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath)
            [ordered]@{ ok = $true; product_root = $ProductRoot }
        }
        $token = {
            param([string]$Path)
            New-PcvDesktopNodeProductProtectedTokenFile `
                -Path $Path `
                -Token 'persisted-token' `
                -InvokeProcess {
                    [ordered]@{ exit_code = 0; stdout = 'processed'; stderr = '' }
                }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -CopyAssets $copy `
            -PrepareTokenFile $token `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        Should -Invoke -CommandName Invoke-WebRequest -ModuleName PcvDesktopNodeProduct -Times 1 -Exactly
        $script:CapturedHealthUri | Should -Be 'http://127.0.0.1:7777/api/v1/runtime/policy'
        $script:CapturedHealthHeaders.Authorization | Should -Be 'Bearer persisted-token'
        $script:CapturedHealthUseBasicParsing | Should -BeTrue
        $healthStep = @($result.executed | Where-Object { $_.step -eq 'health' })[0]
        $healthStep.result.auth | Should -Be 'bearer-protected-token-file'
    }

    It 'migrates an existing legacy token into the protected token file during default Install token preparation' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $dataRoot = Join-Path $TestDrive 'data'
        $tokenPath = Join-Path $dataRoot 'api-token.txt'
        $protectedTokenPath = Join-Path $dataRoot 'api-token.dpapi.json'
        $winSwSource = Join-Path $TestDrive 'winsw-token-test.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
        New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
        Set-Content -LiteralPath $tokenPath -Value 'persisted-token' -Encoding UTF8 -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot $dataRoot `
            -WinSwPath $winSwSource

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $steps.Add("$FileName $($Arguments -join ' ')")
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath)
            [ordered]@{ ok = $true; product_root = $ProductRoot }
        }
        $health = {
            param([string]$Prefix)
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -CopyAssets $copy `
            -InvokeProcess $runner `
            -TestHealth $health

        $result.ok | Should -BeTrue
        (Get-Content -LiteralPath $tokenPath -Raw) | Should -Be 'persisted-token'
        (Read-PcvDesktopNodeProductProtectedTokenFile -Path $protectedTokenPath).token | Should -Be 'persisted-token'
        $tokenStep = @($result.executed | Where-Object { $_.step -eq 'token' })[0]
        $tokenStep.result.ok | Should -BeTrue
        $tokenStep.result.path | Should -Be $protectedTokenPath
        $tokenStep.result.storage | Should -Be 'dpapi-local-machine'
        $tokenStep.result.migrated_from_legacy_token_file | Should -Be $tokenPath
        $tokenStep.result.legacy_token_file_retained | Should -BeTrue
    }

    It 'orchestrates Rollback with injectable product action dependencies' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $steps.Add("$leafName $($Arguments -join ' ')")
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $restore = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $steps.Add("restore $ProductRoot")
            [ordered]@{ ok = $true; product_root = $ProductRoot; restored = $true }
        }
        $health = {
            param([string]$Prefix)
            $steps.Add("health $Prefix")
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -TestHealth $health `
            -RestorePreviousProductRoot $restore

        $result.ok | Should -BeTrue
        $result.action | Should -Be 'Rollback'
        $result.executed.Count | Should -BeGreaterThan 0
        $meaningfulSteps = @($steps | Where-Object {
                $_ -in @(
                    'sc.exe stop PureCVisorDesktopNode',
                    "restore $($plan.product_root)",
                    'sc.exe start PureCVisorDesktopNode',
                    'health http://127.0.0.1:7777/'
                )
            })
        $meaningfulSteps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            "restore $($plan.product_root)",
            'sc.exe start PureCVisorDesktopNode',
            'health http://127.0.0.1:7777/'
        )
        $steps | Should -Not -Contain 'sc.exe create PureCVisorDesktopNode'
    }

    It 'blocks Rollback restore and old service start while a pending commit exists' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $dataRoot = Join-Path $TestDrive 'data-rollback-pending-commit'
        New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeRollbackPendingCommit') `
            -DataRoot $dataRoot
        Set-Content -LiteralPath $plan.paths.job_store_pending_commit -Value '{"version":1}' -Encoding UTF8
        $restoreCalled = $false

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param([string]$FileName, [string[]]$Arguments)
                $steps.Add("$FileName $($Arguments -join ' ')")
                if ($Arguments[0] -eq 'start') {
                    throw 'Old service start must not run while a pending commit exists.'
                }
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -RestorePreviousProductRoot {
                $restoreCalled = $true
                throw 'Rollback restore must not run while a pending commit exists.'
            } `
            -TestHealth {
                throw 'Health must not run while a pending commit exists.'
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
        $restoreCalled | Should -BeFalse
        @($result.executed.step) | Should -Contain 'service.stop.wait'
        @($result.executed.step) | Should -Contain 'job-store.pending-commit.guard-before-restore'
        @($result.executed.step) | Should -Not -Contain 'restore'
        @($result.executed.step) | Should -Not -Contain 'service.start'
        @($result.executed.step) | Should -Not -Contain 'health'
        Test-Path -LiteralPath $plan.paths.job_store_pending_commit | Should -BeTrue
    }

    It 'continues Rollback after a nonzero stop command and still restores, starts, and checks health' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $line = "$leafName $($Arguments -join ' ')"
            $steps.Add($line)
            if ($line -eq 'sc.exe stop PureCVisorDesktopNode') {
                return [ordered]@{ exit_code = 1062; stdout = ''; stderr = 'service is not active' }
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $restore = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $steps.Add("restore $ProductRoot")
            [ordered]@{ ok = $true; product_root = $ProductRoot; restored = $true }
        }
        $health = {
            param([string]$Prefix)
            $steps.Add("health $Prefix")
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -TestHealth $health `
            -RestorePreviousProductRoot $restore

        $result.ok | Should -BeTrue
        $steps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            "restore $($plan.product_root)",
            'sc.exe start PureCVisorDesktopNode',
            'health http://127.0.0.1:7777/'
        )
        $stopStep = @($result.executed | Where-Object { $_.step -eq 'service.stop' })[0]
        $stopStep.result.ok | Should -BeFalse
        $stopStep.result.results[0].exit_code | Should -Be 1062
        $stopStep.result.results[0].stderr | Should -Be 'service is not active'
    }

    It 'validates previous product manifest before restoring Rollback target' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeRollbackManifest'
        $previousProductRoot = "$productRoot.previous"
        New-Item -ItemType Directory -Path $productRoot, $previousProductRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $productRoot 'current.txt') -Value 'current' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $previousProductRoot 'marker.txt') -Value 'previous' -Encoding UTF8
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $previousProductRoot 'product-manifest.json') -Encoding UTF8

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data-rollback-manifest')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $health = {
            param([string]$Prefix)
            [ordered]@{ ok = $true }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -TestHealth $health

        $result.action | Should -Be 'Rollback'
        $result.ok | Should -BeTrue
        $result.rollback.previous_version | Should -Be '0.17.0-dev'
        $result.rollback.failed_root | Should -Match '\.failed$'
        $result.rollback.failed_root_preserved_for_diagnostics | Should -BeTrue
    }

    It 'restores the active product when previous promotion fails' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeRollbackCompensation'
        $previousProductRoot = "$productRoot.previous"
        New-Item -ItemType Directory -Path $productRoot, $previousProductRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $productRoot 'current.txt') -Value 'current' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $previousProductRoot 'previous.txt') -Value 'previous' -Encoding UTF8
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $previousProductRoot 'product-manifest.json') -Encoding UTF8

        $movePath = {
            param([string]$Source, [string]$Destination)

            if ($Source -eq $previousProductRoot -and $Destination -eq $productRoot) {
                New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
                Set-Content -LiteralPath (Join-Path $productRoot 'partial.txt') -Value 'partial' -Encoding UTF8
                throw 'simulated previous promotion failure'
            }

            Move-Item -LiteralPath $Source -Destination $Destination -Force -ErrorAction Stop
        }

        $module = Get-Module PcvDesktopNodeProduct
        $result = & $module {
            param([string]$ProductRoot, [string]$PreviousProductRoot, [scriptblock]$MovePath)
            Restore-PcvDesktopNodePreviousProductRoot `
                -ProductRoot $ProductRoot `
                -PreviousProductRoot $PreviousProductRoot `
                -MovePath $MovePath
        } $productRoot $previousProductRoot $movePath

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_RESTORE_FAILED'
        (Get-Content -LiteralPath (Join-Path $productRoot 'current.txt') -Raw).Trim() | Should -Be 'current'
        (Get-Content -LiteralPath (Join-Path $previousProductRoot 'previous.txt') -Raw).Trim() | Should -Be 'previous'
        @(Get-ChildItem -LiteralPath $TestDrive -Directory -Filter 'DesktopNodeRollbackCompensation.restore-partial.*').Count | Should -Be 1
    }

    It 'preserves the previous backup when active backup promotion fails' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeBackupCompensation'
        $previousProductRoot = "$productRoot.previous"
        $stagingProductRoot = "$previousProductRoot.staging"
        New-Item -ItemType Directory -Path $productRoot, $previousProductRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $productRoot 'current.txt') -Value 'current' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $previousProductRoot 'old-previous.txt') -Value 'old-previous' -Encoding UTF8

        $movePath = {
            param([string]$Source, [string]$Destination)

            if ($Source -eq $productRoot -and $Destination -eq $previousProductRoot) {
                New-Item -ItemType Directory -Path $previousProductRoot -Force | Out-Null
                Set-Content -LiteralPath (Join-Path $previousProductRoot 'partial.txt') -Value 'partial' -Encoding UTF8
                throw 'simulated active backup promotion failure'
            }

            Move-Item -LiteralPath $Source -Destination $Destination -Force -ErrorAction Stop
        }

        $module = Get-Module PcvDesktopNodeProduct
        $result = & $module {
            param([string]$ProductRoot, [string]$PreviousProductRoot, [scriptblock]$MovePath)
            Backup-PcvDesktopNodeProductRoot `
                -ProductRoot $ProductRoot `
                -PreviousProductRoot $PreviousProductRoot `
                -MovePath $MovePath
        } $productRoot $previousProductRoot $movePath

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_BACKUP_FAILED'
        (Get-Content -LiteralPath (Join-Path $productRoot 'current.txt') -Raw).Trim() | Should -Be 'current'
        (Get-Content -LiteralPath (Join-Path $previousProductRoot 'old-previous.txt') -Raw).Trim() | Should -Be 'old-previous'
        Test-Path -LiteralPath $stagingProductRoot | Should -BeFalse
        @(Get-ChildItem -LiteralPath $TestDrive -Directory -Filter 'DesktopNodeBackupCompensation.previous.partial.*').Count | Should -Be 1
    }

    It 'requires explicit recovery when previous backup and staging both exist' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeBackupRecoveryRequired'
        $previousProductRoot = "$productRoot.previous"
        $stagingProductRoot = "$previousProductRoot.staging"
        New-Item -ItemType Directory -Path $productRoot, $previousProductRoot, $stagingProductRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $productRoot 'current.txt') -Value 'current' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $previousProductRoot 'previous.txt') -Value 'previous' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $stagingProductRoot 'staging.txt') -Value 'staging' -Encoding UTF8

        $module = Get-Module PcvDesktopNodeProduct
        $result = & $module {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            Backup-PcvDesktopNodeProductRoot `
                -ProductRoot $ProductRoot `
                -PreviousProductRoot $PreviousProductRoot
        } $productRoot $previousProductRoot

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED'
        Test-Path -LiteralPath (Join-Path $productRoot 'current.txt') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $previousProductRoot 'previous.txt') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $stagingProductRoot 'staging.txt') | Should -BeTrue
    }

    It 'prioritizes backup recovery conflicts when the active product root is missing' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeBackupRecoveryWithoutActive'
        $previousProductRoot = "$productRoot.previous"
        $stagingProductRoot = "$previousProductRoot.staging"
        New-Item -ItemType Directory -Path $previousProductRoot, $stagingProductRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $previousProductRoot 'previous.txt') -Value 'previous' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $stagingProductRoot 'staging.txt') -Value 'staging' -Encoding UTF8

        $module = Get-Module PcvDesktopNodeProduct
        $result = & $module {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            Backup-PcvDesktopNodeProductRoot `
                -ProductRoot $ProductRoot `
                -PreviousProductRoot $PreviousProductRoot
        } $productRoot $previousProductRoot

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED'
        Test-Path -LiteralPath (Join-Path $previousProductRoot 'previous.txt') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $stagingProductRoot 'staging.txt') | Should -BeTrue
    }

    It 'restores the previous product root during default Rollback' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNode'
        $previousProductRoot = "$productRoot.previous"
        $failedProductRoot = "$productRoot.failed"
        New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $previousProductRoot 'api') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $productRoot 'current.txt') -Value 'current' -Encoding UTF8
        Set-Content -LiteralPath (Join-Path $previousProductRoot 'marker.txt') -Value 'previous' -Encoding UTF8
        [ordered]@{
            schema_version = 1
            product = 'PureCVisor Desktop Node'
            version = '0.17.0-dev'
        } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $previousProductRoot 'product-manifest.json') -Encoding UTF8

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data')

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $steps.Add("$leafName $($Arguments -join ' ')")
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $health = {
            param([string]$Prefix)
            $steps.Add("health $Prefix")
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -TestHealth $health

        $result.ok | Should -BeTrue
        $plan.paths.previous_product_root | Should -Be $previousProductRoot
        Test-Path -LiteralPath $previousProductRoot | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $productRoot 'marker.txt') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'current.txt') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $failedProductRoot 'current.txt') | Should -BeTrue
        $restoreStep = @($result.executed | Where-Object { $_.step -eq 'restore' })[0]
        $restoreStep.result.restored | Should -BeTrue
        $restoreStep.result.previous_product_root | Should -Be $previousProductRoot
        $restoreStep.result.failed_root | Should -Match '\.failed$'
        $restoreStep.result.failed_root_preserved_for_diagnostics | Should -BeTrue
        $steps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            'sc.exe start PureCVisorDesktopNode',
            'health http://127.0.0.1:7777/'
        )
    }

    It 'waits for SCM status to stop before restoring during Rollback' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $script:rollbackStatusCalls = 0
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeRollbackWait') `
            -DataRoot (Join-Path $TestDrive 'data-rollback-wait')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $line = "$leafName $($Arguments -join ' ')"
            $steps.Add($line)
            if ($Arguments[0] -eq 'query') {
                $script:rollbackStatusCalls += 1
                if ($script:rollbackStatusCalls -eq 1) {
                    return [ordered]@{ exit_code = 0; stdout = "Started`r`n"; stderr = '' }
                }
                return [ordered]@{ exit_code = 0; stdout = "Stopped`r`n"; stderr = '' }
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $restore = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $steps.Add("restore $ProductRoot")
            [ordered]@{ ok = $true; product_root = $ProductRoot; restored = $true }
        }
        $health = {
            param([string]$Prefix)
            $steps.Add("health $Prefix")
            [ordered]@{ ok = $true; uri = ($Prefix.TrimEnd('/') + '/api/v1/runtime/policy') }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RestorePreviousProductRoot $restore `
            -TestHealth $health

        $result.ok | Should -BeTrue
        $steps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            "restore $($plan.product_root)",
            'sc.exe start PureCVisorDesktopNode',
            'health http://127.0.0.1:7777/'
        )
        $waitStep = @($result.executed | Where-Object { $_.step -eq 'service.stop.wait' })[0]
        $waitStep.result.ok | Should -BeTrue
        $waitStep.result.attempt_count | Should -Be 2
    }

    It 'fails Rollback before service start when the previous product root is missing' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNode'
        New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data')

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $line = "$leafName $($Arguments -join ' ')"
            $steps.Add($line)
            if ($line -eq 'sc.exe start PureCVisorDesktopNode') {
                throw 'Service start should not run when rollback restore fails.'
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $health = {
            param([string]$Prefix)
            throw 'Health should not run when rollback restore fails.'
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -TestHealth $health

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_PREVIOUS_ROOT_MISSING'
        $steps | Should -Be @('sc.exe stop PureCVisorDesktopNode', 'sc.exe query PureCVisorDesktopNode')
        $restoreStep = @($result.executed | Where-Object { $_.step -eq 'restore' })[0]
        $restoreStep.result.ok | Should -BeFalse
        $restoreStep.result.error.code | Should -Be 'PCV_PRODUCT_PREVIOUS_ROOT_MISSING'
        @($result.executed | Where-Object { $_.step -eq 'service.start' }).Count | Should -Be 0
        @($result.executed | Where-Object { $_.step -eq 'health' }).Count | Should -Be 0
    }

    It 'rejects rollback when previous manifest is missing or invalid' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeInvalidRollback'
        $previousRoot = "$productRoot.previous"
        New-Item -ItemType Directory -Path $productRoot, $previousRoot -Force | Out-Null
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data-invalid-rollback')

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'start') {
                throw 'Service start must not run when previous manifest is invalid.'
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_PREVIOUS_MANIFEST_INVALID'
        @($result.executed | Where-Object { $_.step -eq 'service.start' }).Count | Should -Be 0
    }

    It 'rejects Rollback restore dependencies that do not report restored true' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Rollback `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data')

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $line = "$leafName $($Arguments -join ' ')"
            $steps.Add($line)
            if ($line -eq 'sc.exe start PureCVisorDesktopNode') {
                throw 'Service start should not run when restore did not happen.'
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $restore = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            $steps.Add("restore $ProductRoot")
            [ordered]@{ ok = $true; product_root = $ProductRoot; restored = $false }
        }
        $health = {
            param([string]$Prefix)
            throw 'Health should not run when restore did not happen.'
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RestorePreviousProductRoot $restore `
            -TestHealth $health

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_RESTORE_NOT_PERFORMED'
        $steps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            "restore $($plan.product_root)"
        )
        @($result.executed | Where-Object { $_.step -eq 'service.start' }).Count | Should -Be 0
        @($result.executed | Where-Object { $_.step -eq 'health' }).Count | Should -Be 0
    }

    It 'returns partial command diagnostics when the SCM create command fails' {
        $winSwSource = Join-Path $TestDrive 'winsw-install-failure.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNode') `
            -DataRoot (Join-Path $TestDrive 'data') `
            -WinSwPath $winSwSource

        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'create') {
                return [ordered]@{ exit_code = 5; stdout = 'before failure'; stderr = 'install failed' }
            }
            [ordered]@{ exit_code = 0; stdout = "ok $($Arguments[0])"; stderr = '' }
        }
        $copy = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath)
            [ordered]@{ ok = $true; product_root = $ProductRoot }
        }
        $token = {
            param([string]$Path)
            [ordered]@{ ok = $true; path = $Path }
        }
        $health = {
            param([string]$Prefix)
            throw 'Health should not run after service install failure.'
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -CopyAssets $copy `
            -PrepareTokenFile $token `
            -InvokeProcess $runner `
            -TestHealth $health

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_COMMAND_FAILED'
        $installStep = @($result.executed | Where-Object { $_.step -eq 'service.install' })[0]
        $installStep.result.ok | Should -BeFalse
        $installStep.result.results.Count | Should -Be 1
        $installStep.result.results[0].arguments[0] | Should -Be 'create'
        $installStep.result.results[0].exit_code | Should -Be 5
        $installStep.result.results[0].stdout | Should -Be 'before failure'
        $installStep.result.results[0].stderr | Should -Be 'install failed'
        @($result.executed | Where-Object { $_.step -eq 'health' }).Count | Should -Be 0
    }

    It 'orchestrates Status with service query and manifest presence' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNodeStatus'
        New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Value '{}' -Encoding UTF8
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Status `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data-status')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $steps.Add("$FileName $($Arguments -join ' ')")
            [ordered]@{ exit_code = 0; stdout = 'STATE              : 4  RUNNING'; stderr = '' }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        $result.action | Should -Be 'Status'
        $steps | Should -Be @('sc.exe query PureCVisorDesktopNode')
        $manifestStep = @($result.executed | Where-Object { $_.step -eq 'manifest' })[0]
        $manifestStep.result.exists | Should -BeTrue
    }

    It 'writes install log start and success events without command secrets' {
        $dataRoot = Join-Path $TestDrive 'data-install-log'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Status `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeInstallLog') `
            -DataRoot $dataRoot
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = 'Started'; stderr = '' }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        Test-Path -LiteralPath $plan.paths.install_log | Should -BeTrue
        $events = @(Get-Content -LiteralPath $plan.paths.install_log | ForEach-Object { $_ | ConvertFrom-Json })
        $events.event | Should -Be @('product.action.start', 'product.action.success')
        $events[1].data.executed_steps | Should -Be @('service.status', 'manifest')
        $combined = Get-Content -LiteralPath $plan.paths.install_log -Raw
        $combined | Should -Not -Match 'Bearer '
        $combined | Should -Not -Match 'protected_token'
    }

    It 'writes install log failure events for RemoveInstalled RemoveData failures' {
        $dataRoot = Join-Path $TestDrive 'data-remove-failure-log'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeRemoveFailureLog') `
            -DataRoot $dataRoot `
            -RemoveData
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'query') {
                return [ordered]@{ exit_code = 0; stdout = "Stopped`r`n"; stderr = '' }
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $removePath = {
            param([string]$Path)
            [ordered]@{
                ok = $false
                path = $Path
                error = [ordered]@{
                    code = 'PCV_PRODUCT_REMOVE_FAILED'
                    message = 'Desktop Node product path removal failed.'
                    detail = 'simulated remove-data failure'
                }
            }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RemovePath $removePath

        $result.ok | Should -BeFalse
        Test-Path -LiteralPath $plan.paths.install_log | Should -BeTrue
        $events = @(Get-Content -LiteralPath $plan.paths.install_log | ForEach-Object { $_ | ConvertFrom-Json })
        $events.event | Should -Be @('product.action.start', 'product.action.failure')
        $events[1].data.error_code | Should -Be 'PCV_PRODUCT_REMOVE_FAILED'
        $events[1].data.error_detail | Should -Be 'simulated remove-data failure'
        $events[1].data.executed_steps | Should -Be @(
            'service.stop',
            'service.stop.wait',
            'service.uninstall',
            'job-store.orphan-temp.discovery'
        )
    }

    It 'reports Status even when the service query returns missing service' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Status `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeStatusMissing') `
            -DataRoot (Join-Path $TestDrive 'data-status-missing')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 1060; stdout = ''; stderr = 'service does not exist' }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        $statusStep = @($result.executed | Where-Object { $_.step -eq 'service.status' })[0]
        $statusStep.result.ok | Should -BeFalse
        $statusStep.result.results[0].exit_code | Should -Be 1060
        $manifestStep = @($result.executed | Where-Object { $_.step -eq 'manifest' })[0]
        $manifestStep.result.exists | Should -BeFalse
    }

    It 'orchestrates Uninstall while preserving data by default' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $removed = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNodeUninstall'
        $dataRoot = Join-Path $TestDrive 'data-uninstall'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $steps.Add("$leafName $($Arguments -join ' ')")
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $removePath = {
            param([string]$Path)
            $removed.Add($Path)
            [ordered]@{ ok = $true; path = $Path; removed = $true }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RemovePath $removePath

        $result.ok | Should -BeTrue
        $steps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            'sc.exe delete PureCVisorDesktopNode'
        )
        @($removed) | Should -Be @($productRoot)
        @($removed | Where-Object { $_ -like "$dataRoot*" }).Count | Should -Be 0
    }

    It 'blocks preserve-data Uninstall before service and product removal while a pending commit exists' {
        $calls = [System.Collections.Generic.List[string]]::new()
        $removed = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNodeUninstallPendingCommit'
        $dataRoot = Join-Path $TestDrive 'data-uninstall-pending-commit'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot
        Set-Content -LiteralPath $plan.paths.job_store_pending_commit -Value '{"version":1}' -Encoding UTF8

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param([string]$FileName, [string[]]$Arguments)
                $calls.Add("$FileName $($Arguments -join ' ')")
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -RemovePath {
                param([string]$Path)
                $removed.Add($Path)
                [ordered]@{ ok = $true; path = $Path; removed = $true }
            }

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
        @($result.executed.step) | Should -Contain 'service.stop.wait'
        @($result.executed.step) | Should -Contain 'job-store.pending-commit.guard-before-service-removal'
        @($result.executed.step) | Should -Not -Contain 'service.uninstall'
        @($result.executed.step) | Should -Not -Contain 'remove'
        @($calls | Where-Object { $_ -match '\sdelete\s' }).Count | Should -Be 0
        $removed.Count | Should -Be 0
        Test-Path -LiteralPath $productRoot | Should -BeTrue
        Test-Path -LiteralPath $plan.paths.job_store_pending_commit | Should -BeTrue
    }

    It 'retries product root removal when the service host lock delays deletion' {
        $attempts = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNodeLockedUninstall'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data-locked-uninstall')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $removePath = {
            param([string]$Path)
            $attempts.Add($Path)
            if ($attempts.Count -eq 1) {
                return [ordered]@{
                    ok = $false
                    path = $Path
                    error = [ordered]@{
                        code = 'PCV_PRODUCT_REMOVE_FAILED'
                        message = 'Desktop Node product path removal failed.'
                        detail = "Access to the path '$Path\DesktopNode.Host.exe' is denied."
                    }
                }
            }
            [ordered]@{ ok = $true; path = $Path; removed = $true }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RemovePath $removePath

        $result.ok | Should -BeTrue
        @($attempts) | Should -Be @($productRoot, $productRoot)
        $removeStep = @($result.executed | Where-Object { $_.step -eq 'remove' })[0]
        $removeStep.result[0].attempt_count | Should -Be 2
        $removeStep.result[0].ok | Should -BeTrue
    }

    It 'waits for SCM status to stop before uninstalling and removing product files' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $script:statusCalls = 0
        $productRoot = Join-Path $TestDrive 'DesktopNodeStopWait'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data-stop-wait')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $line = "$leafName $($Arguments -join ' ')"
            $steps.Add($line)
            if ($Arguments[0] -eq 'query') {
                $script:statusCalls += 1
                if ($script:statusCalls -eq 1) {
                    return [ordered]@{ exit_code = 0; stdout = "Started`r`n"; stderr = '' }
                }
                return [ordered]@{ exit_code = 0; stdout = "Stopped`r`n"; stderr = '' }
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $removePath = {
            param([string]$Path)
            $steps.Add("remove $Path")
            [ordered]@{ ok = $true; path = $Path; removed = $true }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RemovePath $removePath

        $result.ok | Should -BeTrue
        $steps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            'sc.exe delete PureCVisorDesktopNode',
            "remove $productRoot"
        )
        $waitStep = @($result.executed | Where-Object { $_.step -eq 'service.stop.wait' })[0]
        $waitStep.result.ok | Should -BeTrue
        $waitStep.result.attempt_count | Should -Be 2
    }

    It 'orchestrates remove-data Uninstall with explicit data paths' {
        $removed = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNodeRemoveData'
        $dataRoot = Join-Path $TestDrive 'data-remove'
        New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
        $legacyTempPath = Join-Path $dataRoot 'jobs.json.tmp'
        $jobTempPath = Join-Path $dataRoot 'jobs.json.tmp.11111111111111111111111111111111'
        $pendingTempPath = Join-Path $dataRoot 'jobs.json.commit-pending.tmp.22222222222222222222222222222222'
        $nearMissTempPath = Join-Path $dataRoot 'jobs.json.tmp.not-a-guid'
        Set-Content -LiteralPath $legacyTempPath, $jobTempPath, $pendingTempPath, $nearMissTempPath -Value 'orphan' -Encoding UTF8
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -RemoveData
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $removePath = {
            param([string]$Path)
            $removed.Add($Path)
            [ordered]@{ ok = $true; path = $Path; removed = $true }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RemovePath $removePath

        $result.ok | Should -BeTrue
        @($removed) | Should -Contain $productRoot
        @($removed) | Should -Contain (Join-Path $dataRoot 'api-token.txt')
        @($removed) | Should -Contain (Join-Path $dataRoot 'jobs.json')
        @($removed) | Should -Contain $legacyTempPath
        @($removed) | Should -Contain $jobTempPath
        @($removed) | Should -Contain $pendingTempPath
        @($removed) | Should -Not -Contain $nearMissTempPath
        @($removed) | Should -Contain (Join-Path $dataRoot 'events.jsonl')
        @($removed) | Should -Contain (Join-Path $dataRoot 'install.jsonl')
        @($removed) | Should -Contain (Join-Path $dataRoot 'diagnostics')
    }

    It 'repairs hardened token ACL before RemoveData deletes the token file' {
        $processCalls = [System.Collections.Generic.List[object]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNodeAclRemoveData'
        $dataRoot = Join-Path $TestDrive 'data-acl-remove'
        $tokenPath = Join-Path $dataRoot 'api-token.txt'
        New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
        Set-Content -LiteralPath $tokenPath -Value 'persisted-token' -Encoding UTF8 -NoNewline
        $script:TokenPathForAclRepair = $tokenPath
        $script:TokenRemoveAttempts = 0
        Mock -CommandName Remove-Item -ModuleName PcvDesktopNodeProduct -MockWith {
            param([string]$LiteralPath)

            if ($LiteralPath -eq $script:TokenPathForAclRepair) {
                $script:TokenRemoveAttempts += 1
                if ($script:TokenRemoveAttempts -eq 1) {
                    throw [System.UnauthorizedAccessException]::new("Access to the path '$LiteralPath' is denied.")
                }
            }
        }
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -RemoveData
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $processCalls.Add([pscustomobject]@{
                    file_name = $FileName
                    arguments = $Arguments
                })
            if ((Split-Path -Leaf $FileName) -eq 'icacls.exe') {
                return [ordered]@{ exit_code = 0; stdout = 'processed file: 1'; stderr = '' }
            }
            if ($Arguments[0] -eq 'query') {
                return [ordered]@{ exit_code = 0; stdout = "Stopped`r`n"; stderr = '' }
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner

        $result.ok | Should -BeTrue
        $script:TokenRemoveAttempts | Should -Be 2
        $icaclsCalls = @($processCalls | Where-Object { (Split-Path -Leaf $_.file_name) -eq 'icacls.exe' })
        $icaclsCalls.Count | Should -Be 1
        $icaclsCalls[0].arguments | Should -Be @($tokenPath, '/grant:r', 'BUILTIN\Administrators:F', 'NT AUTHORITY\SYSTEM:F')
        $removeStep = @($result.executed | Where-Object { $_.step -eq 'remove' })[0]
        $tokenRemove = @($removeStep.result | Where-Object { $_.path -eq $tokenPath })[0]
        $tokenRemove.ok | Should -BeTrue
        $tokenRemove.acl_repair.ok | Should -BeTrue
    }

    It 'repairs hardened protected token ACL when localized access denied text is returned during MSI RemoveInstalled RemoveData' {
        $processCalls = [System.Collections.Generic.List[object]]::new()
        $dataRoot = Join-Path $TestDrive 'data-acl-remove-localized'
        $protectedTokenPath = Join-Path $dataRoot 'api-token.dpapi.json'
        $winSwPath = Join-Path $TestDrive 'winsw-msi-remove-localized-acl.exe'
        New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
        Set-Content -LiteralPath $protectedTokenPath -Value '{}' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath $winSwPath -Value 'fake-winsw' -Encoding UTF8 -NoNewline
        $script:ProtectedTokenPathForLocalizedAclRepair = $protectedTokenPath
        $script:ProtectedTokenRemoveAttempts = 0
        $script:LocalizedAccessDeniedText = [regex]::Unescape('\uC561\uC138\uC2A4\uAC00 \uAC70\uBD80\uB418\uC5C8\uC2B5\uB2C8\uB2E4')
        Mock -CommandName Remove-Item -ModuleName PcvDesktopNodeProduct -MockWith {
            param([string]$LiteralPath)

            if ($LiteralPath -eq $script:ProtectedTokenPathForLocalizedAclRepair) {
                $script:ProtectedTokenRemoveAttempts += 1
                if ($script:ProtectedTokenRemoveAttempts -eq 1) {
                    throw [System.UnauthorizedAccessException]::new("$($script:LocalizedAccessDeniedText): '$LiteralPath'")
                }
            }
        }
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeMsiRemoveLocalizedAcl') `
            -DataRoot $dataRoot `
            -WinSwPath $winSwPath `
            -RemoveData

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param([string]$FileName, [string[]]$Arguments)
                $processCalls.Add([pscustomobject]@{
                        file_name = $FileName
                        arguments = $Arguments
                    })
                if ((Split-Path -Leaf $FileName) -eq 'icacls.exe') {
                    return [ordered]@{ exit_code = 0; stdout = 'processed file: 1'; stderr = '' }
                }
                if ($Arguments[0] -eq 'query') {
                    return [ordered]@{ exit_code = 0; stdout = "Stopped`r`n"; stderr = '' }
                }
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            }

        $result.ok | Should -BeTrue
        $script:ProtectedTokenRemoveAttempts | Should -Be 2
        $icaclsCalls = @($processCalls | Where-Object { (Split-Path -Leaf $_.file_name) -eq 'icacls.exe' })
        $icaclsCalls.Count | Should -Be 1
        $icaclsCalls[0].arguments | Should -Be @($protectedTokenPath, '/grant:r', 'BUILTIN\Administrators:F', 'NT AUTHORITY\SYSTEM:F')
        $removeStep = @($result.executed | Where-Object { $_.step -eq 'remove' })[0]
        $tokenRemove = @($removeStep.result | Where-Object { $_.path -eq $protectedTokenPath })[0]
        $tokenRemove.ok | Should -BeTrue
        $tokenRemove.acl_repair.ok | Should -BeTrue
    }

    It 'continues Uninstall removal when the service is already missing' {
        $removed = [System.Collections.Generic.List[string]]::new()
        $productRoot = Join-Path $TestDrive 'DesktopNodeMissingService'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot (Join-Path $TestDrive 'data-missing-service')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            if ($Arguments[0] -eq 'delete') {
                return [ordered]@{ exit_code = 1; stdout = ''; stderr = 'NonExistentService' }
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $removePath = {
            param([string]$Path)
            $removed.Add($Path)
            [ordered]@{ ok = $true; path = $Path; removed = $true }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RemovePath $removePath

        $result.ok | Should -BeTrue
        @($removed) | Should -Be @($productRoot)
        $deleteStep = @($result.executed | Where-Object { $_.step -eq 'service.uninstall' })[0]
        $deleteStep.result.ok | Should -BeFalse
        $deleteStep.result.results[0].exit_code | Should -Be 1
    }

    It 'blocks Uninstall removal after an unexpected stop failure' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $removed = [System.Collections.Generic.List[string]]::new()
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeStopFailure') `
            -DataRoot (Join-Path $TestDrive 'data-stop-failure')
        $runner = {
            param([string]$FileName, [string[]]$Arguments)
            $leafName = Split-Path -Leaf $FileName
            $line = "$leafName $($Arguments -join ' ')"
            $steps.Add($line)
            if ($Arguments[0] -eq 'stop') {
                return [ordered]@{ exit_code = 5; stdout = ''; stderr = 'access denied' }
            }
            [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
        }
        $removePath = {
            param([string]$Path)
            $removed.Add($Path)
            [ordered]@{ ok = $true; path = $Path; removed = $true }
        }

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess $runner `
            -RemovePath $removePath

        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_COMMAND_FAILED'
        $steps | Should -Be @('sc.exe stop PureCVisorDesktopNode')
        @($removed).Count | Should -Be 0
        @($result.executed | Where-Object { $_.step -eq 'service.uninstall' }).Count | Should -Be 0
        @($result.executed | Where-Object { $_.step -eq 'remove' }).Count | Should -Be 0
    }

    It 'configures an already installed MSI payload without copying assets' {
        $calls = [System.Collections.Generic.List[object]]::new()
        $winSwPath = Join-Path $TestDrive 'winsw-msi-configure.exe'
        Set-Content -LiteralPath $winSwPath -Value 'fake-winsw' -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action ConfigureInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeMsiConfigure') `
            -DataRoot (Join-Path $TestDrive 'data-msi-configure') `
            -WinSwPath $winSwPath

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param($FileName, $Arguments)
                $calls.Add([ordered]@{ file = $FileName; args = $Arguments })
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -CopyAssets {
                throw 'ConfigureInstalled must not copy assets'
            } `
            -PrepareTokenFile {
                param($Path)
                $calls.Add([ordered]@{ op = 'prepare-token'; path = $Path })
                [ordered]@{ ok = $true; path = $Path }
            } `
            -TestHealth {
                param($Prefix)
                $calls.Add([ordered]@{ op = 'health'; prefix = $Prefix })
                [ordered]@{ ok = $true; status_code = 200 }
            }

        $result.action | Should -Be 'ConfigureInstalled'
        $result.ok | Should -BeTrue
        @($result.executed).Where({ $_['step'] -eq 'token' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.configure' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.install' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.start' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'health' }).Count | Should -Be 1
        @($calls | Where-Object { $_.op -eq 'prepare-token' })[0].path | Should -Be $plan.paths.token_protected_file
        $configureStep = @($result.executed).Where({ $_['step'] -eq 'service.configure' })[0]
        $configureStep.result.executable_path | Should -Be $plan.service.host.executable_path
        $configureStep.result.binary_path | Should -Match 'DesktopNode\.Host\.exe'
    }

    It 'repairs service configuration without copying assets or deleting product root' {
        $calls = [System.Collections.Generic.List[object]]::new()
        $winSwPath = Join-Path $TestDrive 'winsw-msi-repair.exe'
        Set-Content -LiteralPath $winSwPath -Value 'fake-winsw' -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RepairInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeMsiRepair') `
            -DataRoot (Join-Path $TestDrive 'data-msi-repair') `
            -WinSwPath $winSwPath

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param($FileName, $Arguments)
                $calls.Add([ordered]@{ file = $FileName; args = $Arguments })
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -CopyAssets {
                throw 'RepairInstalled must not copy assets'
            } `
            -PrepareTokenFile {
                param($Path)
                $calls.Add([ordered]@{ op = 'prepare-token'; path = $Path })
                [ordered]@{ ok = $true; path = $Path }
            } `
            -TestHealth {
                param($Prefix)
                $calls.Add([ordered]@{ op = 'health'; prefix = $Prefix })
                [ordered]@{ ok = $true; status_code = 200 }
            } `
            -RemovePath {
                param($Path)
                throw "RepairInstalled must not remove paths: $Path"
            }

        $result.action | Should -Be 'RepairInstalled'
        $result.ok | Should -BeTrue
        @($result.executed).Where({ $_['step'] -eq 'service.stop' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.stop.wait' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.configure' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.install' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.start' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'health' }).Count | Should -Be 1
        @($calls | Where-Object { $_.op -eq 'prepare-token' })[0].path | Should -Be $plan.paths.token_protected_file
        $configureStep = @($result.executed).Where({ $_['step'] -eq 'service.configure' })[0]
        $configureStep.result.executable_path | Should -Be $plan.service.host.executable_path
        $configureStep.result.binary_path | Should -Match 'DesktopNode\.Host\.exe'
    }

    It 'repairs MSI-installed service through native service-action so BatchEvidenceRoot updates PathName' {
        $calls = [System.Collections.Generic.List[object]]::new()
        $batchEvidenceRoot = 'D:\PureCVisorEvidence\batch-runs'
        $productRoot = Join-Path $TestDrive 'DesktopNodeMsiRepairNative'
        $dataRoot = Join-Path $TestDrive 'data-msi-repair-native'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RepairInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -BatchEvidenceRoot $batchEvidenceRoot

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param($FileName, $Arguments)
                $calls.Add([ordered]@{ file = $FileName; args = @($Arguments) })
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -CopyAssets {
                throw 'RepairInstalled must not copy assets'
            } `
            -PrepareTokenFile {
                param($Path)
                $calls.Add([ordered]@{ op = 'prepare-token'; path = $Path })
                [ordered]@{ ok = $true; path = $Path }
            } `
            -TestHealth {
                param($Prefix)
                $calls.Add([ordered]@{ op = 'health'; prefix = $Prefix })
                [ordered]@{ ok = $true; status_code = 200 }
            } `
            -RemovePath {
                param($Path)
                throw "RepairInstalled must not remove paths: $Path"
            }

        $result.ok | Should -BeTrue
        $nativeCalls = @($calls | Where-Object {
                $_.Contains('file') -and
                (Split-Path -Leaf $_.file) -eq 'DesktopNode.Host.exe' -and
                @($_.args) -contains 'service-action'
            })
        $nativeCalls.Count | Should -Be 1
        $nativeCalls[0].args | Should -Be @(
            'service-action',
            'repair-installed',
            '--product-root',
            $productRoot,
            '--data-root',
            $dataRoot,
            '--service-exe',
            $plan.paths.service_exe,
            '--batch-evidence-root',
            $batchEvidenceRoot
        )
        @($calls | Where-Object { $_.Contains('file') -and (Split-Path -Leaf $_.file) -eq 'sc.exe' -and @($_.args) -contains 'create' }).Count | Should -Be 0
        @($calls | Where-Object { $_.Contains('file') -and (Split-Path -Leaf $_.file) -eq 'sc.exe' -and @($_.args) -contains 'start' }).Count | Should -Be 0
        $startStep = @($result.executed | Where-Object { $_.step -eq 'service.start' })[0]
        $startStep.result.skipped | Should -BeTrue
        $startStep.result.reason | Should -Be 'native-service-action-controls-final-state'
    }

    It 'removes MSI-installed service while preserving data by default' {
        $calls = [System.Collections.Generic.List[object]]::new()
        $winSwPath = Join-Path $TestDrive 'winsw-msi-remove.exe'
        Set-Content -LiteralPath $winSwPath -Value 'fake-winsw' -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeMsiRemove') `
            -DataRoot (Join-Path $TestDrive 'data-msi-remove') `
            -WinSwPath $winSwPath

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param($FileName, $Arguments)
                $calls.Add([ordered]@{ file = $FileName; args = $Arguments })
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -RemovePath {
                param($Path)
                throw "RemoveInstalled without RemoveData must not remove paths: $Path"
            }

        $result.action | Should -Be 'RemoveInstalled'
        $result.ok | Should -BeTrue
        $result.removed_paths | Should -BeNullOrEmpty
        @($result.executed).Where({ $_['step'] -eq 'service.stop' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.stop.wait' }).Count | Should -Be 1
        @($result.executed).Where({ $_['step'] -eq 'service.uninstall' }).Count | Should -Be 1
    }

    It 'blocks preserve-data RemoveInstalled before service removal while a pending commit exists' {
        $calls = [System.Collections.Generic.List[object]]::new()
        $removed = [System.Collections.Generic.List[string]]::new()
        $winSwPath = Join-Path $TestDrive 'winsw-msi-remove-pending-commit.exe'
        $productRoot = Join-Path $TestDrive 'DesktopNodeMsiRemovePendingCommit'
        $dataRoot = Join-Path $TestDrive 'data-msi-remove-pending-commit'
        New-Item -ItemType Directory -Path $productRoot, $dataRoot -Force | Out-Null
        Set-Content -LiteralPath $winSwPath -Value 'fake-winsw' -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -WinSwPath $winSwPath
        Set-Content -LiteralPath $plan.paths.job_store_pending_commit -Value '{"version":1}' -Encoding UTF8

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param($FileName, $Arguments)
                $calls.Add([ordered]@{ file = $FileName; args = @($Arguments) })
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -RemovePath {
                param($Path)
                $removed.Add($Path)
                [ordered]@{ ok = $true; path = $Path; removed = $true }
            }

        $result.action | Should -Be 'RemoveInstalled'
        $result.ok | Should -BeFalse
        $result.error.code | Should -Be 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
        @($result.executed.step) | Should -Contain 'service.stop.wait'
        @($result.executed.step) | Should -Contain 'job-store.pending-commit.guard-before-service-removal'
        @($result.executed.step) | Should -Not -Contain 'service.uninstall'
        @($result.executed.step) | Should -Not -Contain 'remove'
        @($calls | Where-Object { @($_.args) -contains 'delete' }).Count | Should -Be 0
        $removed.Count | Should -Be 0
        Test-Path -LiteralPath $plan.paths.job_store_pending_commit | Should -BeTrue
    }

    It 'continues MSI RemoveInstalled when Restart Manager already has the SCM service stopping' {
        $steps = [System.Collections.Generic.List[string]]::new()
        $script:RemoveInstalledStatusCalls = 0
        $winSwPath = Join-Path $TestDrive 'winsw-msi-remove-stop-pending.exe'
        Set-Content -LiteralPath $winSwPath -Value 'fake-winsw' -NoNewline
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeMsiRemoveStopPending') `
            -DataRoot (Join-Path $TestDrive 'data-msi-remove-stop-pending') `
            -WinSwPath $winSwPath

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                param($FileName, $Arguments)
                $leafName = Split-Path -Leaf $FileName
                $steps.Add("$leafName $($Arguments -join ' ')")
                if ($Arguments[0] -eq 'stop') {
                    return [ordered]@{
                        exit_code = 1
                        stdout = ''
                        stderr = "FATAL - Cannot stop 'PureCVisorDesktopNode' service on computer '.'."
                    }
                }
                if ($Arguments[0] -eq 'query') {
                    $script:RemoveInstalledStatusCalls += 1
                    if ($script:RemoveInstalledStatusCalls -eq 1) {
                        return [ordered]@{ exit_code = 0; stdout = "StopPending`r`n"; stderr = '' }
                    }
                    return [ordered]@{ exit_code = 0; stdout = "Stopped`r`n"; stderr = '' }
                }
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -RemovePath {
                param($Path)
                throw "RemoveInstalled without RemoveData must not remove paths: $Path"
            }

        $result.action | Should -Be 'RemoveInstalled'
        $result.ok | Should -BeTrue
        $steps | Should -Be @(
            'sc.exe stop PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            'sc.exe query PureCVisorDesktopNode',
            'sc.exe delete PureCVisorDesktopNode'
        )
        $stopStep = @($result.executed | Where-Object { $_.step -eq 'service.stop' })[0]
        $stopStep.result.ok | Should -BeTrue
        $stopStep.result.results[0].exit_code | Should -Be 1
        $waitStep = @($result.executed | Where-Object { $_.step -eq 'service.stop.wait' })[0]
        $waitStep.result.attempt_count | Should -Be 2
    }

    It 'removes only ProgramData paths for MSI RemoveInstalled -RemoveData' {
        $removed = [System.Collections.Generic.List[string]]::new()
        $winSwPath = Join-Path $TestDrive 'winsw-msi-remove-data.exe'
        Set-Content -LiteralPath $winSwPath -Value 'fake-winsw' -NoNewline
        $dataRoot = Join-Path $TestDrive 'data-msi-remove-data'
        New-Item -ItemType Directory -Path $dataRoot -Force | Out-Null
        $legacyTempPath = Join-Path $dataRoot 'jobs.json.tmp'
        $jobTempPath = Join-Path $dataRoot 'jobs.json.tmp.33333333333333333333333333333333'
        $pendingTempPath = Join-Path $dataRoot 'jobs.json.commit-pending.tmp.44444444444444444444444444444444'
        $nearMissTempPath = Join-Path $dataRoot 'jobs.json.commit-pending.tmp.44444444444444444444444444444444.extra'
        Set-Content -LiteralPath $legacyTempPath, $jobTempPath, $pendingTempPath, $nearMissTempPath -Value 'orphan' -Encoding UTF8
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot (Join-Path $TestDrive 'DesktopNodeMsiRemoveData') `
            -DataRoot $dataRoot `
            -WinSwPath $winSwPath `
            -RemoveData

        $result = Invoke-PcvDesktopNodeProductAction `
            -Plan $plan `
            -InvokeProcess {
                [ordered]@{ exit_code = 0; stdout = ''; stderr = '' }
            } `
            -RemovePath {
                param($Path)
                $removed.Add($Path)
                [ordered]@{ path = $Path; removed = $true }
            } `
            -GrantAdministratorsFullControl {
                param($Path)
                [ordered]@{ ok = $true; path = $Path }
            }

        $result.action | Should -Be 'RemoveInstalled'
        $result.ok | Should -BeTrue
        $removed | Should -Contain (Join-Path $plan.data_root 'api-token.dpapi.json')
        $removed | Should -Contain (Join-Path $plan.data_root 'api-token.txt')
        $removed | Should -Contain (Join-Path $plan.data_root 'accounts.json')
        $removed | Should -Contain (Join-Path $plan.data_root 'jwt-signing-key.txt')
        $removed | Should -Contain (Join-Path $plan.data_root 'jobs.json')
        $removed | Should -Contain $legacyTempPath
        $removed | Should -Contain (Join-Path $plan.data_root 'jobs.json.commit-pending')
        $removed | Should -Contain $jobTempPath
        $removed | Should -Contain $pendingTempPath
        $removed | Should -Not -Contain $nearMissTempPath
        $removed | Should -Contain (Join-Path $plan.data_root 'events.jsonl')
        $removed | Should -Contain (Join-Path $plan.data_root 'install.jsonl')
        $removed | Should -Contain (Join-Path $plan.data_root 'diagnostics')
        $removed | Should -Not -Contain $plan.product_root
    }
}
