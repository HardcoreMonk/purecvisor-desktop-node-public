Set-StrictMode -Version Latest

Describe 'PcvDesktopNodeProduct manifest and asset copy contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $modulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        Import-Module $modulePath -Force
    }

    It 'builds a product manifest with product-owned asset files only' {
        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -Version '0.12.0-test'

        $manifest.schema_version | Should -Be 2
        $manifest.product | Should -Be 'PureCVisor Desktop Node'
        $manifest.version | Should -Be '0.12.0-test'
        $manifest.paths.product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode'
        $manifest.paths.cli_exe | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe'
        $manifest.paths.account_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\accounts.json'
        $manifest.paths.jwt_signing_key_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\jwt-signing-key.txt'
        $manifest.paths.PSObject.Properties.Name | Should -Not -Contain 'helper_script'
        $manifest.paths.PSObject.Properties.Name | Should -Not -Contain 'api_script'
        $manifest.cli.command_name | Should -Be 'pcvcli'
        $manifest.cli.executable_path | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe'
        $manifest.cli.mode | Should -Be 'dotnet-local-api-client'
        $manifest.auth.account_auth_source | Should -Be 'local-json-and-jwt-signing-key'
        $manifest.auth.account_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\accounts.json'
        $manifest.auth.jwt_signing_key_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\jwt-signing-key.txt'
        $manifest.auth.account_auth_default_state | Should -Be 'not-configured-until-accounts-exist'

        $relativePaths = @($manifest.assets | ForEach-Object { $_.relative_path })
        $relativePaths | Should -Contain 'web\index.html'
        $relativePaths | Should -Not -Contain 'api\Invoke-PcvDesktopApi.ps1'
        $relativePaths | Should -Not -Contain 'hyperv\Invoke-PcvHyperV.ps1'
        $relativePaths | Should -Not -Contain 'service\PcvDesktopService.psm1'
        @($manifest.assets | Where-Object { $_.source -match 'spikes[\\/]purecvisor-desktop-node' }).Count | Should -Be 0
    }

    It 'records the CLI and Web-only schema v2 contract without TUI metadata' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeCliWebManifest'
        $dataRoot = Join-Path $TestDrive 'data-cli-web-manifest'

        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.41.0-dev'

        $manifest.schema_version | Should -Be 2
        $manifest.paths.cli_exe | Should -Be (Join-Path $productRoot 'pcvcli.exe')
        $manifest.paths.PSObject.Properties.Name | Should -Not -Contain 'tui_exe'
        $manifest.cli.command_name | Should -Be 'pcvcli'
        $manifest.PSObject.Properties.Name | Should -Not -Contain 'tui'
        @($manifest.assets | ForEach-Object { $_.relative_path }) | Should -Contain 'web\index.html'
    }

    It 'reads migrated product manifest schema v2 for update and rollback compatibility' {
        $manifestPath = Join-Path $TestDrive 'product-manifest-v2.json'
        @'
{
  "schema_version": 2,
  "product": "PureCVisor Desktop Node",
  "version": "0.38.4",
  "migration": {
    "plan_id": "product-config-v1-to-v2",
    "source_schema_version": 1,
    "target_schema_version": 2
  }
}
'@ | Set-Content -LiteralPath $manifestPath -Encoding UTF8

        $manifest = Read-PcvDesktopNodeProductManifest -Path $manifestPath

        $manifest.schema_version | Should -Be 2
        $manifest.product | Should -Be 'PureCVisor Desktop Node'
        $manifest.version | Should -Be '0.38.4'
        $manifest.migration.plan_id | Should -Be 'product-config-v1-to-v2'
    }

    It 'does not expose spikes paths as standalone product asset sources' {
        $assets = @(Get-PcvDesktopNodeProductAssets `
                -SourceRoot $script:RepoRoot `
                -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode')

        $assets.Count | Should -BeGreaterThan 0
        @($assets | Where-Object { $_.source -match 'spikes[\\/]purecvisor-desktop-node' }).Count | Should -Be 0
        @($assets | ForEach-Object { $_.name }) | Should -Contain 'web'
    }

    It 'excludes source tests directories from manifest and copied product assets' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeNoTests'
        $dataRoot = Join-Path $TestDrive 'data-no-tests'
        $winSwSource = Join-Path $TestDrive 'winsw-existing-test.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $result = Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.12.0-test' `
            -WinSwPath $winSwSource

        $result.ok | Should -BeTrue
        $manifest = Get-Content -Raw (Join-Path $productRoot 'product-manifest.json') | ConvertFrom-Json
        $relativePaths = @($manifest.assets | ForEach-Object { $_.relative_path })
        @($relativePaths | Where-Object { $_ -match '(^|\\)tests(\\|$)' }).Count | Should -Be 0

        $copiedTestsEntries = @(Get-ChildItem -LiteralPath $productRoot -Recurse -Force |
            Where-Object { $_.FullName.Substring($productRoot.Length).TrimStart('\', '/') -match '(^|[\\/])tests([\\/]|$)' })
        $copiedTestsEntries.Count | Should -Be 0
    }

    It 'uses terminating errors for product asset filesystem mutations' {
        $modulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        $moduleText = Get-Content -Raw $modulePath
        $assetListFunction = [regex]::Match(
            $moduleText,
            'function Get-PcvDesktopNodeAssetFileList \{(?<body>[\s\S]*?)\n\}',
            [System.Text.RegularExpressions.RegexOptions]::Multiline
        ).Groups['body'].Value
        $copyFunction = [regex]::Match(
            $moduleText,
            'function Copy-PcvDesktopNodeProductAssets \{(?<body>[\s\S]*?)\n\}',
            [System.Text.RegularExpressions.RegexOptions]::Multiline
        ).Groups['body'].Value

        @($assetListFunction -split "`r?`n" | Where-Object { $_ -match '\bGet-ChildItem\b' }).Count | Should -Be 1
        @($assetListFunction -split "`r?`n" | Where-Object { $_ -match '\bGet-ChildItem\b' -and $_ -match '-ErrorAction Stop' }).Count | Should -Be 1
        @($copyFunction -split "`r?`n" | Where-Object { $_ -match '\bNew-Item\b' }).Count | Should -Be 2
        @($copyFunction -split "`r?`n" | Where-Object { $_ -match '\bNew-Item\b' -and $_ -match '-ErrorAction Stop' }).Count | Should -Be 2
        @($copyFunction -split "`r?`n" | Where-Object { $_ -match '\bCopy-Item\b' }).Count | Should -Be 1
        @($copyFunction -split "`r?`n" | Where-Object { $_ -match '\bCopy-Item\b' -and $_ -match '-ErrorAction Stop' }).Count | Should -Be 1
        @($copyFunction -split "`r?`n" | Where-Object { $_ -match '\bSet-Content\b' }).Count | Should -Be 1
        @($copyFunction -split "`r?`n" | Where-Object { $_ -match '\bSet-Content\b' -and $_ -match '-ErrorAction Stop' }).Count | Should -Be 1
    }

    It 'copies product assets and writes a product manifest' {
        $productRoot = Join-Path $TestDrive 'DesktopNode'
        $dataRoot = Join-Path $TestDrive 'data'
        $winSwSource = Join-Path $TestDrive 'winsw-existing-test.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $result = Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.12.0-test' `
            -WinSwPath $winSwSource

        $result.ok | Should -BeTrue
        Test-Path (Join-Path $productRoot 'web\index.html') | Should -BeTrue
        Test-Path (Join-Path $productRoot 'api\Invoke-PcvDesktopApi.ps1') | Should -BeFalse
        Test-Path (Join-Path $productRoot 'hyperv\Invoke-PcvHyperV.ps1') | Should -BeFalse
        Test-Path (Join-Path $productRoot 'service\PcvDesktopService.psm1') | Should -BeFalse

        $manifestPath = Join-Path $productRoot 'product-manifest.json'
        Test-Path $manifestPath | Should -BeTrue
        $manifest = Get-Content -Raw $manifestPath | ConvertFrom-Json
        $manifest.version | Should -Be '0.12.0-test'
        @($manifest.assets | Where-Object { $_.source -match 'spikes[\\/]purecvisor-desktop-node' }).Count | Should -Be 0
    }

    It 'copies packaged runtime payload files when the source root is an MSI payload' {
        $payloadRoot = Join-Path $TestDrive 'payload-runtime'
        $productRoot = Join-Path $TestDrive 'DesktopNodeRuntimePayload'
        $dataRoot = Join-Path $TestDrive 'data-runtime-payload'
        New-Item -ItemType Directory -Path (Join-Path $payloadRoot 'web') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $payloadRoot 'DesktopNode.Host.exe') -Value 'fake-host' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe') -Value 'fake-cli' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'Invoke-PcvDesktopNodeProduct.ps1') -Value 'fake-entrypoint' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'PcvDesktopNodeProduct.psm1') -Value 'fake-module' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\app.js') -Value 'console.log("pcv");' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\index.html') -Value '<div id="app"></div>' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\styles.css') -Value 'body{}' -Encoding UTF8 -NoNewline

        $result = Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $payloadRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.35.0'

        $result.ok | Should -BeTrue
        $result.runtime_payload_count | Should -Be 4
        Test-Path -LiteralPath (Join-Path $productRoot 'DesktopNode.Host.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'pcvcli.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'pcvtui.exe') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $productRoot 'Invoke-PcvDesktopNodeProduct.ps1') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'PcvDesktopNodeProduct.psm1') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'web\index.html') | Should -BeTrue
    }

    It 'blocks partial root runtime payload copy when the CLI executable is missing' {
        $payloadRoot = Join-Path $TestDrive 'payload-runtime-missing-cli'
        $productRoot = Join-Path $TestDrive 'DesktopNodeRuntimePayloadMissingCli'
        $dataRoot = Join-Path $TestDrive 'data-runtime-payload-missing-cli'
        New-Item -ItemType Directory -Path (Join-Path $payloadRoot 'web') -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $payloadRoot 'DesktopNode.Host.exe') -Value 'fake-host' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'Invoke-PcvDesktopNodeProduct.ps1') -Value 'fake-entrypoint' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'PcvDesktopNodeProduct.psm1') -Value 'fake-module' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\app.js') -Value 'console.log("pcv");' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\index.html') -Value '<div id="app"></div>' -Encoding UTF8 -NoNewline
        Set-Content -LiteralPath (Join-Path $payloadRoot 'web\styles.css') -Value 'body{}' -Encoding UTF8 -NoNewline

        {
            Copy-PcvDesktopNodeProductAssets `
                -SourceRoot $payloadRoot `
                -ProductRoot $productRoot `
                -DataRoot $dataRoot `
                -Version '0.35.0'
        } | Should -Throw 'PCV_PRODUCT_RUNTIME_PAYLOAD_FILE_MISSING*pcvcli.exe*'

        Test-Path -LiteralPath (Join-Path $productRoot 'DesktopNode.Host.exe') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $productRoot 'pcvcli.exe') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $productRoot 'Invoke-PcvDesktopNodeProduct.ps1') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $productRoot 'PcvDesktopNodeProduct.psm1') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $productRoot 'product-manifest.json') | Should -BeFalse
    }

    It 'does not stage WinSW executable or XML into the product root' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeServiceHost'
        $dataRoot = Join-Path $TestDrive 'data-service-host'
        $winSwSource = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $result = Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.13.0-test' `
            -WinSwPath $winSwSource

        $result.ok | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $productRoot 'winsw\PureCVisorDesktopNode.exe') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $productRoot 'winsw\PureCVisorDesktopNode.xml') | Should -BeFalse
        $result.service_host.default_owner | Should -Be 'dotnet-windows-service-host'
        $result.service_host.executable_path | Should -Be (Join-Path $productRoot 'DesktopNode.Host.exe')
    }

    It 'records .NET service host metadata in product-manifest.json' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeServiceHostManifest'
        $dataRoot = Join-Path $TestDrive 'data-service-host-manifest'
        $winSwSource = Join-Path $TestDrive 'winsw-manifest.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        Copy-PcvDesktopNodeProductAssets `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.13.0-test' `
            -WinSwPath $winSwSource | Out-Null

        $manifest = Get-Content -LiteralPath (Join-Path $productRoot 'product-manifest.json') -Raw | ConvertFrom-Json
        $manifest.PSObject.Properties.Name | Should -Not -Contain 'winsw'
        $manifest.service_host.mode | Should -Be 'dotnet-windows-service'
        $manifest.service_host.executable_path | Should -Be (Join-Path $productRoot 'DesktopNode.Host.exe')
        $manifest.service_host.default_owner | Should -Be 'dotnet-windows-service-host'
    }

    It 'records active .NET CLI metadata in product-manifest.json' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeCliManifest'
        $dataRoot = Join-Path $TestDrive 'data-cli-manifest'

        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.40.0-dev'

        $manifest.cli.command_name | Should -Be 'pcvcli'
        $manifest.cli.executable_path | Should -Be (Join-Path $productRoot 'pcvcli.exe')
        $manifest.cli.mode | Should -Be 'dotnet-local-api-client'
        $manifest.cli.token_sources | Should -Contain '--token'
        $manifest.cli.token_sources | Should -Contain '--protected-token-file'
    }

    It 'records diagnostics policy v1 in product-manifest.json' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeDiagPolicy'
        $dataRoot = Join-Path $TestDrive 'data-diag-policy'
        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot `
            -Version '0.16.0'

        $manifest.diagnostics.schema_version | Should -Be 1
        $manifest.diagnostics.mode | Should -Be 'windows-event-log-default-jsonl-retained'
        $manifest.diagnostics.event_log.path | Should -Be (Join-Path $dataRoot 'events.jsonl')
        $manifest.diagnostics.install_log.retained_files | Should -Be 5
        $manifest.diagnostics.service_logs.retained_files | Should -Be 10
        $manifest.diagnostics.windows_event_log.enabled_by_default | Should -BeTrue
        $manifest.diagnostics.windows_event_log.default_writer | Should -Be 'windows-event-log'
        $manifest.diagnostics.windows_event_log.schema_version | Should -Be 1
    }

    It 'records LAN security policy v1 in product-manifest.json' {
        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -Version '0.17.0'

        $manifest.network.schema_version | Should -Be 1
        $manifest.network.default_exposure | Should -Be 'loopback'
        $manifest.network.lan_mode.state | Should -Be 'preview-admin-opt-in'
        $manifest.network.lan_mode.enabled_by_default | Should -BeFalse
        $manifest.network.lan_mode.token_source | Should -Be 'dpapi-local-machine-protected-file'
        $manifest.network.tls.termination | Should -Be 'external-reverse-proxy-or-tls-terminator'
        $manifest.network.firewall.lifecycle_owner | Should -Be 'admin-opt-in-product-action-or-manual-command'
    }

    It 'records ProgramData ACL ownership policy in product-manifest.json' {
        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -Version '0.23.8-rc.1'

        $manifest.data_acl.schema_version | Should -Be 1
        $manifest.data_acl.owner | Should -Be 'product-wrapper'
        $manifest.data_acl.wix_manages_data_root_acl | Should -BeFalse
        $manifest.data_acl.installer_contract | Should -Be 'msi-computes-programdata-path-product-action-manages-sensitive-file-acl'
        $manifest.data_acl.host_acl_inspection | Should -Be 'administrator-opt-in-only'
        @($manifest.data_acl.sensitive_files | Where-Object { $_.name -eq 'api-token.dpapi.json' }).storage | Should -Be 'dpapi-local-machine'
        @($manifest.data_acl.sensitive_files | Where-Object { $_.name -eq 'accounts.json' }).storage | Should -Be 'local-json-password-hashes'
        @($manifest.data_acl.sensitive_files | Where-Object { $_.name -eq 'jwt-signing-key.txt' }).storage | Should -Be 'local-high-entropy-signing-key'
    }

    It 'records update policy v1 in product-manifest.json' {
        $manifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -Version '0.18.0-dev'

        $manifest.schema_version | Should -Be 2
        $manifest.version | Should -Be '0.18.0-dev'
        $manifest.update.schema_version | Should -Be 1
        $manifest.update.decision | Should -Be 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
        $manifest.update.version_source | Should -Be 'product-wrapper-version-parameter'
        $manifest.update.rollback.retained_previous_roots | Should -Be 1
        $manifest.update.config_migration.mode | Should -Be 'validate-before-service-start'
        $manifest.update.job_store.destructive_rewrite_by_default | Should -BeFalse
        $manifest.update.provenance.signed_release_required_for_release_channel | Should -BeTrue
    }
}
