Set-StrictMode -Version Latest

Describe 'PcvDesktopNodeProduct plan contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $modulePath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
        Import-Module $modulePath -Force
    }

    It 'returns product defaults' {
        $defaults = Get-PcvDesktopNodeProductDefaults
        $dataRoot = Join-Path $env:ProgramData 'PureCVisor\desktop-node'

        $defaults.service_name | Should -Be 'PureCVisorDesktopNode'
        $defaults.display_name | Should -Be 'PureCVisor Desktop Node'
        $defaults.prefix | Should -Be 'http://127.0.0.1:7777/'
        $defaults.web_prefix | Should -Be 'http://127.0.0.1:80/'
        $defaults.product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode'
        $defaults.data_root | Should -Be $dataRoot
        $defaults.token_protected_file | Should -Be (Join-Path $dataRoot 'api-token.dpapi.json')
        $defaults.token_file | Should -Be (Join-Path $dataRoot 'api-token.txt')
        $defaults.account_file | Should -Be (Join-Path $dataRoot 'accounts.json')
        $defaults.jwt_signing_key_file | Should -Be (Join-Path $dataRoot 'jwt-signing-key.txt')
        $defaults.job_store | Should -Be (Join-Path $dataRoot 'jobs.json')
        $defaults.job_store_legacy_temp | Should -Be (Join-Path $dataRoot 'jobs.json.tmp')
        $defaults.job_store_pending_commit | Should -Be (Join-Path $dataRoot 'jobs.json.commit-pending')
        $defaults.job_store_temp_pattern | Should -Be (Join-Path $dataRoot 'jobs.json.tmp.*')
        $defaults.job_store_pending_commit_temp_pattern | Should -Be (Join-Path $dataRoot 'jobs.json.commit-pending.tmp.*')
        $defaults.event_log | Should -Be (Join-Path $dataRoot 'events.jsonl')
        $defaults.install_log | Should -Be (Join-Path $dataRoot 'install.jsonl')
        $defaults.diagnostics_root | Should -Be (Join-Path $dataRoot 'diagnostics')
        $defaults.service_exe_name | Should -Be 'DesktopNode.Host.exe'
        $defaults.cli_exe_name | Should -Be 'pcvcli.exe'
        $defaults.PSObject.Properties.Name | Should -Not -Contain 'tui_exe_name'
        $defaults.service_logs_root | Should -Be (Join-Path $dataRoot 'service-logs')
    }

    It 'resolves CLI and Web product paths without a TUI executable' {
        $productRoot = Join-Path $TestDrive 'DesktopNodeCliWebPaths'
        $dataRoot = Join-Path $TestDrive 'data-cli-web-paths'

        $paths = Resolve-PcvDesktopNodeProductPaths `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $productRoot `
            -DataRoot $dataRoot

        $paths.cli_exe | Should -Be (Join-Path $productRoot 'pcvcli.exe')
        $paths.job_store_legacy_temp | Should -Be (Join-Path $dataRoot 'jobs.json.tmp')
        $paths.job_store_pending_commit | Should -Be (Join-Path $dataRoot 'jobs.json.commit-pending')
        $paths.job_store_temp_pattern | Should -Be (Join-Path $dataRoot 'jobs.json.tmp.*')
        $paths.job_store_pending_commit_temp_pattern | Should -Be (Join-Path $dataRoot 'jobs.json.commit-pending.tmp.*')
        $paths.PSObject.Properties.Name | Should -Not -Contain 'tui_exe'
    }

    It 'computes product hashes without the Get-FileHash cmdlet' {
        $moduleText = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1')
        $hashInput = Join-Path $TestDrive 'hash-input.bin'
        [System.IO.File]::WriteAllBytes($hashInput, [byte[]](0x70, 0x63, 0x76))
        $sha = [System.Security.Cryptography.SHA256]::Create()
        try {
            $expected = [System.BitConverter]::ToString($sha.ComputeHash([System.IO.File]::ReadAllBytes($hashInput))).
                Replace('-', '').
                ToLowerInvariant()
        }
        finally {
            $sha.Dispose()
        }

        $moduleText | Should -Not -Match '\bGet-FileHash\b'
        $moduleText | Should -Match 'System\.Security\.Cryptography\.SHA256'
        Get-PcvFileSha256 -Path $hashInput | Should -Be $expected
    }

    It 'builds an install product plan with file auth, assets, and service commands' {
        $winSwSource = Join-Path $TestDrive 'winsw-install-plan.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WinSwPath $winSwSource

        $plan.action | Should -Be 'Install'
        $plan.requires_elevation | Should -BeTrue
        $plan.product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode'
        $plan.paths.previous_product_root | Should -Be 'C:\Program Files\PureCVisor\DesktopNode.previous'
        $plan.paths.update_transaction_journal | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\update-transaction.json'
        $plan.paths.job_store_legacy_temp | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\jobs.json.tmp'
        $plan.paths.job_store_pending_commit | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\jobs.json.commit-pending'
        $plan.paths.job_store_temp_pattern | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\jobs.json.tmp.*'
        $plan.paths.job_store_pending_commit_temp_pattern | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\jobs.json.commit-pending.tmp.*'
        $plan.paths.service_exe | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe'
        $plan.paths.PSObject.Properties.Name | Should -Not -Contain 'api_script'
        $plan.data_root | Should -Be 'C:\ProgramData\PureCVisor\desktop-node'
        $plan.auth.api_token_source | Should -Be 'protected_file'
        $plan.auth.api_token_storage | Should -Be 'dpapi-local-machine'
        $plan.auth.api_token_protected_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json'
        $plan.auth.legacy_api_token_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\api-token.txt'
        $plan.auth.account_auth_source | Should -Be 'local-json-and-jwt-signing-key'
        $plan.auth.account_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\accounts.json'
        $plan.auth.jwt_signing_key_file | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\jwt-signing-key.txt'
        $plan.auth.account_auth_default_state | Should -Be 'not-configured-until-accounts-exist'
        @($plan.auth.rbac_roles) | Should -Be @('viewer', 'operator', 'admin')
        $plan.data_acl.schema_version | Should -Be 1
        $plan.data_acl.wix_manages_data_root_acl | Should -BeFalse
        $plan.data_acl.installer_contract | Should -Be 'msi-computes-programdata-path-product-action-manages-sensitive-file-acl'
        @($plan.data_acl.required_principals | ForEach-Object { $_.identity }) | Should -Contain 'BUILTIN\Administrators'
        @($plan.data_acl.required_principals | ForEach-Object { $_.identity }) | Should -Contain 'NT AUTHORITY\SYSTEM'
        @($plan.data_acl.sensitive_files | Where-Object { $_.name -eq 'api-token.dpapi.json' }).remove_data_acl_repair_required | Should -BeTrue
        @($plan.data_acl.sensitive_files | Where-Object { $_.name -eq 'api-token.txt' }).remove_data_acl_repair_required | Should -BeTrue
        @($plan.data_acl.sensitive_files | Where-Object { $_.name -eq 'accounts.json' }).storage | Should -Be 'local-json-password-hashes'
        @($plan.data_acl.sensitive_files | Where-Object { $_.name -eq 'jwt-signing-key.txt' }).storage | Should -Be 'local-high-entropy-signing-key'
        $plan.data_acl.host_acl_inspection | Should -Be 'administrator-opt-in-only'
        $plan.network.schema_version | Should -Be 1
        $plan.network.decision | Should -Be 'DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required'
        $plan.network.default_exposure | Should -Be 'loopback'
        $plan.network.lan_mode.state | Should -Be 'preview-admin-opt-in'
        $plan.network.lan_mode.enabled_by_default | Should -BeFalse
        $plan.network.lan_mode.requires_allow_lan | Should -BeTrue
        $plan.network.lan_mode.requires_bearer_token | Should -BeTrue
        $plan.network.lan_mode.non_loopback_static_auth | Should -Be 'bearer-required'
        $plan.network.tls.provided_by_product_wrapper | Should -BeFalse
        $plan.network.tls.required_for_lan | Should -BeTrue
        $plan.network.firewall.enabled_by_default | Should -BeFalse
        $plan.network.firewall.installer_auto_enable | Should -BeFalse
        $plan.no_auto_reboot.enabled | Should -BeTrue
        $plan.no_auto_reboot.enforcement | Should -Be 'product-process-command-guard'
        $plan.no_auto_reboot.forbidden_commands | Should -Contain 'Restart-Computer'
        $plan.no_auto_reboot.forbidden_commands | Should -Contain 'shutdown.exe'
        $plan.update.schema_version | Should -Be 1
        $plan.update.decision | Should -Be 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
        $plan.update.version_source | Should -Be 'product-wrapper-version-parameter'
        $plan.update.installed_manifest_is_source_of_truth | Should -BeTrue
        $plan.update.payload_version_must_match_manifest | Should -BeTrue
        $plan.update.rollback.previous_root | Should -Be $plan.paths.previous_product_root
        $plan.update.rollback.retained_previous_roots | Should -Be 1
        $plan.update.rollback.rollback_requires_health_check | Should -BeTrue
        $plan.update.config_migration.mode | Should -Be 'validate-before-service-start'
        $plan.update.config_migration.dry_run_required | Should -BeTrue
        $plan.update.config_migration.block_service_start_on_failure | Should -BeTrue
        $plan.update.config_migration.data_backup_required_before_mutation | Should -BeTrue
        $plan.update.job_store.destructive_rewrite_by_default | Should -BeFalse
        $plan.update.job_store.schema_mismatch_mode | Should -Be 'read-only-or-blocked-with-diagnostics'
        $plan.update.provenance.unsigned_dev_allowed_for_dev_channel | Should -BeTrue
        $plan.update.source_resolution.mode | Should -Be 'local-or-https-package-with-sha256'
        $plan.update.source_resolution.allowed_schemes | Should -Be @('file', 'https')
        $plan.update.source_resolution.expected_sha256_required | Should -BeTrue
        $plan.update.source_resolution.extracts_before_service_stop | Should -BeTrue
        $plan.update.transaction_journal.mode | Should -Be 'single-active-update-journal'
        $plan.update.transaction_journal.path | Should -Be $plan.paths.update_transaction_journal
        $plan.update.transaction_journal.write_before_service_stop | Should -BeTrue
        $plan.update.transaction_journal.record_stage_transitions | Should -BeTrue
        $plan.update.transaction_journal.full_transactional_filesystem | Should -BeTrue

        $plan.service.mode | Should -Be 'dotnet-windows-service'
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('"C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe" listen'))
        $plan.service.config.exposure | Should -Be 'loopback'
        $plan.service.config.prefix | Should -Be 'http://127.0.0.1:7777/'
        $plan.service.config.api_prefix | Should -Be 'http://127.0.0.1:7777/'
        $plan.service.config.web_prefix | Should -Be 'http://127.0.0.1:80/'
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--prefix http://127.0.0.1:7777/'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--web-prefix http://127.0.0.1:80/'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--web-root "C:\Program Files\PureCVisor\DesktopNode\web"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--job-store "C:\ProgramData\PureCVisor\desktop-node\jobs.json"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--event-log "C:\ProgramData\PureCVisor\desktop-node\events.jsonl"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--event-log-provider-source "PureCVisor Desktop Node"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--event-log-provider-log Application'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--event-log-writer windows-event-log'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--event-log-schema-version 1'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--diagnostics-root "C:\ProgramData\PureCVisor\desktop-node\diagnostics"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--api-token-protected-file "C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--account-file "C:\ProgramData\PureCVisor\desktop-node\accounts.json"'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--jwt-signing-key-file "C:\ProgramData\PureCVisor\desktop-node\jwt-signing-key.txt"'))
        $plan.diagnostics.windows_event_log.enabled_by_default | Should -BeTrue
        $plan.diagnostics.windows_event_log.default_writer | Should -Be 'windows-event-log'
        $plan.diagnostics.windows_event_log.schema_version | Should -Be 1
        $plan.service.hardening.schema_version | Should -Be 1
        $plan.service.hardening.route_timeout_seconds | Should -Be 30
        $plan.service.hardening.request_limit_per_minute | Should -Be 120
        $plan.service.hardening.burst_limit | Should -Be 20
        $plan.service.hardening.retry_after_seconds | Should -Be 15
        $plan.service.hardening.max_request_body_bytes | Should -Be 1048576
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--route-timeout-seconds 30'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--request-limit-per-minute 120'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--request-burst-limit 20'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--retry-after-seconds 15'))
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--max-request-body-bytes 1048576'))
        $plan.service.config.binary_path | Should -Not -Match '--allow-lan'
        $plan.service.config.binary_path | Should -Not -Match '--api-token-file'
        $plan.service.config.binary_path | Should -Not -Match '--api-token '

        @($plan.assets | ForEach-Object { $_.name }) | Should -Be @('web')
        @($plan.assets | Where-Object { $_.source -match 'spikes[\\/]purecvisor-desktop-node' }).Count | Should -Be 0
        $plan.service.commands.install[0].file_name | Should -Be 'sc.exe'
        $plan.service.commands.install[0].arguments | Should -Contain 'create'
        $plan.service.commands.install[0].arguments | Should -Contain 'binPath='
        $plan.service.commands.start[0].arguments | Should -Be @('start', 'PureCVisorDesktopNode')
    }

    It 'includes optional batch evidence root in the product service host arguments' {
        $winSwSource = Join-Path $TestDrive 'winsw-batch-evidence-plan.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -BatchEvidenceRoot 'D:\PureCVisorEvidence\batch-runs' `
            -WinSwPath $winSwSource

        $plan.service.config.batch_evidence_root | Should -Be 'D:\PureCVisorEvidence\batch-runs'
        $plan.service.host.arguments | Should -Contain '--batch-evidence-root'
        $plan.service.host.arguments | Should -Contain 'D:\PureCVisorEvidence\batch-runs'
        $plan.service.config.binary_path | Should -Match ([regex]::Escape('--batch-evidence-root "D:\PureCVisorEvidence\batch-runs"'))
    }

    It 'normalizes relative batch evidence root against SourceRoot before writing service arguments' {
        $winSwSource = Join-Path $TestDrive 'winsw-batch-evidence-relative-plan.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline
        $expectedRoot = Join-Path $script:RepoRoot 'artifacts'

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -BatchEvidenceRoot 'artifacts' `
            -WinSwPath $winSwSource

        $plan.service.config.batch_evidence_root | Should -Be $expectedRoot
        $plan.service.host.arguments | Should -Contain '--batch-evidence-root'
        $plan.service.host.arguments | Should -Contain $expectedRoot
        $plan.service.config.binary_path | Should -Match ([regex]::Escape("--batch-evidence-root `"$expectedRoot`""))
    }

    It 'records network download update source gate inputs without resolving them in the plan' {
        $downloadRoot = 'C:\ProgramData\PureCVisor\desktop-node\updates'
        $expectedSha256 = '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -SourceUri 'https://updates.example.invalid/PureCVisorDesktopNode-0.39.0.zip' `
            -ExpectedSha256 $expectedSha256 `
            -DownloadRoot $downloadRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -Version '0.39.0'

        $plan.action | Should -Be 'Update'
        $plan.requires_elevation | Should -BeTrue
        $plan.update_source.enabled | Should -BeTrue
        $plan.update_source.source_uri | Should -Be 'https://updates.example.invalid/PureCVisorDesktopNode-0.39.0.zip'
        $plan.update_source.expected_sha256 | Should -Be $expectedSha256
        $plan.update_source.download_root | Should -Be $downloadRoot
        $plan.update_source.resolution_stage | Should -Be 'before-service-stop'
        $plan.update_source.mutates_host | Should -BeFalse
    }

    It 'records full updater catalog channel inputs without resolving them in the plan' {
        $downloadRoot = 'C:\ProgramData\PureCVisor\desktop-node\updates'
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Update `
            -SourceRoot $script:RepoRoot `
            -UpdateCatalogUri 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json' `
            -UpdateChannel 'internal-dev' `
            -DownloadRoot $downloadRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node'

        $plan.action | Should -Be 'Update'
        $plan.update_catalog.enabled | Should -BeTrue
        $plan.update_catalog.catalog_uri | Should -Be 'https://updates.example.invalid/purecvisor-desktop-node/catalog.json'
        $plan.update_catalog.channel | Should -Be 'internal-dev'
        $plan.update_catalog.download_root | Should -Be $downloadRoot
        $plan.update_catalog.resolution_stage | Should -Be 'before-service-stop'
        $plan.update_catalog.mutates_host | Should -BeFalse
        $plan.update_catalog.publication.external_stable_publication | Should -Be 'not-claimed'
        $plan.update_source.enabled | Should -BeFalse
    }

    It 'includes remove-data delete paths for uninstall' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Uninstall `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -RemoveData

        $plan.delete_paths | Should -Contain 'C:\Program Files\PureCVisor\DesktopNode'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\api-token.txt'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\accounts.json'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\jwt-signing-key.txt'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\jobs.json'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\jobs.json.tmp'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\jobs.json.commit-pending'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\events.jsonl'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\install.jsonl'
        $plan.delete_paths | Should -Contain 'C:\ProgramData\PureCVisor\desktop-node\diagnostics'
        $plan.delete_path_patterns | Should -Be @(
            'C:\ProgramData\PureCVisor\desktop-node\jobs.json.tmp.*',
            'C:\ProgramData\PureCVisor\desktop-node\jobs.json.commit-pending.tmp.*'
        )
    }

    It 'rejects inline API tokens' {
        {
            New-PcvDesktopNodeProductPlan `
                -Action Install `
                -SourceRoot $script:RepoRoot `
                -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
                -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
                -ApiToken 'inline-secret'
        } | Should -Throw 'PCV_PRODUCT_INLINE_TOKEN_FORBIDDEN*'
    }

    It 'builds a .NET Windows service plan with stable paths and command names' {
        $winSwSource = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WinSwPath $winSwSource

        $plan.service.mode | Should -Be 'dotnet-windows-service'
        $plan.paths.service_exe | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe'
        $plan.paths.service_logs_root | Should -Be 'C:\ProgramData\PureCVisor\desktop-node\service-logs'
        $plan.service.host.executable_path | Should -Be $plan.paths.service_exe
        $plan.service.host.default_owner | Should -Be 'dotnet-windows-service-host'
        $plan.service.config.binary_path | Should -Match 'DesktopNode\.Host\.exe'

        $install = @($plan.service.commands.install)
        $install.Count | Should -Be 3
        $install[0].file_name | Should -Be 'sc.exe'
        $install[0].arguments | Should -Contain 'create'

        $plan.service.commands.start[0].arguments | Should -Be @('start', 'PureCVisorDesktopNode')
        $plan.service.commands.stop[0].arguments | Should -Be @('stop', 'PureCVisorDesktopNode')
        $plan.service.commands.status[0].arguments | Should -Be @('query', 'PureCVisorDesktopNode')
        $plan.service.commands.uninstall[0].arguments | Should -Be @('delete', 'PureCVisorDesktopNode')
    }

    It 'does not generate WinSW XML for the default .NET service host' {
        $winSwSource = Join-Path $TestDrive 'winsw.exe'
        Set-Content -LiteralPath $winSwSource -Value 'fake-winsw-binary' -Encoding UTF8 -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
            -WinSwPath $winSwSource

        $plan.service.PSObject.Properties.Name | Should -Not -Contain 'winsw'
        $plan.service.config.binary_path | Should -Match 'DesktopNode\.Host\.exe'
        $plan.service.config.binary_path | Should -Not -Match 'Invoke-PcvDesktopApi\.ps1'
        $plan.service.config.binary_path | Should -Not -Match '--api-token '
    }

    It 'rejects retired WinSW PowerShell Local API generation functions' {
        $paths = Resolve-PcvDesktopNodeProductPaths `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node'

        {
            New-PcvDesktopNodeWinSwArguments `
                -Paths $paths `
                -Prefix 'http://127.0.0.1:7777/' `
                -WorkerCount 1 `
                -TimeoutSec 30
        } | Should -Throw 'PCV_WINSW_POWERSHELL_LOCAL_API_RETIRED*'

        {
            New-PcvDesktopNodeWinSwXml `
                -ServiceName 'PureCVisorDesktopNode' `
                -DisplayName 'PureCVisor Desktop Node' `
                -Description 'Desktop Node' `
                -PwshPath 'pwsh.exe' `
                -Arguments '-File Invoke-PcvDesktopApi.ps1' `
                -WorkingDirectory 'C:\Program Files\PureCVisor\DesktopNode' `
                -LogPath 'C:\ProgramData\PureCVisor\desktop-node\service-logs'
        } | Should -Throw 'PCV_WINSW_POWERSHELL_LOCAL_API_RETIRED*'
    }

    It 'does not require a WinSW artifact for the .NET service host plan' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action Install `
            -SourceRoot $script:RepoRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot 'C:\ProgramData\PureCVisor\desktop-node'

        $plan.service.mode | Should -Be 'dotnet-windows-service'
        $plan.service.host.executable_path | Should -Be 'C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe'
        $plan.service.PSObject.Properties.Name | Should -Not -Contain 'winsw'
    }

    It 'keeps route parity admin smoke aligned with remove-data handoff and data-root gate' {
        $smoke = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1')

        $smoke | Should -Match 'remove_data_handoff'
        $smoke | Should -Match 'data_root_remove_blocked_while_service_exists'
        $smoke | Should -Match 'PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS'
        $smoke | Should -Match "service-action', 'data-root-remove'"
        $smoke | Should -Match 'data_root_remove'
        $smoke | Should -Match 'unrelated_path_exists_after_data_root_remove'
        $smoke | Should -Match 'data_root_exists_after_data_root_remove'
    }

    It 'reads installed protected tokens in route parity smoke without importing the spike service module' {
        $smoke = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1')

        $smoke | Should -Not -Match 'spikes[\\/]+purecvisor-desktop-node[\\/]+service[\\/]+PcvDesktopService\.psm1'
        $smoke | Should -Not -Match '\bImport-Module\b'
        $smoke | Should -Match 'Read-ProtectedToken'
        $smoke | Should -Match 'ProtectedData'
        $smoke | Should -Match 'PureCVisor Desktop Node API Token Store v1'
        $smoke | Should -Match 'dpapi-local-machine'
    }

    It 'covers protected token round trip in the route parity smoke self-test' {
        $smoke = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1')

        $smoke | Should -Match 'protected-token-self-test'
        $smoke | Should -Match 'Read-ProtectedToken -Path'
        $smoke | Should -Match 'ProtectedData\]::Protect'
    }

    It 'checks the Web Console root on the split web port in route parity health' {
        $smoke = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1')

        $smoke | Should -Match "Invoke-WebRequest -Uri 'http://127\.0\.0\.1/' -TimeoutSec 30"
    }

    It 'records partial MSI lifecycle evidence and classifies repair retry transients' {
        $smoke = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1')

        $smoke | Should -Match 'function Get-MsiStepFailureClassification'
        $smoke | Should -Match 'function Write-MsiLifecycleEvidence'
        $smoke | Should -Match 'msi-repair-retryable-transient'
        $smoke | Should -Match 'rerun-batch-step'
        $smoke | Should -Match 'failure_classification'
        $smoke | Should -Match 'PCV_SMOKE_MSI_STEP_FAILED'
        $smoke | Should -Match 'msi-classifier-self-test'
        $smoke | Should -Match 'Write-MsiLifecycleEvidence -Path \$lifecyclePath -Lifecycle \$lifecycle'
    }

    It 'prepares product protected tokens without the spike service module' {
        $moduleText = Get-Content -Raw -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1')

        $moduleText | Should -Match 'New-PcvDesktopNodeProductProtectedTokenFile'
        $moduleText | Should -Match 'Read-PcvDesktopNodeProductProtectedTokenFile'
        $moduleText | Should -Match 'PureCVisor Desktop Node API Token Store v1'
        $moduleText | Should -Not -Match 'Import-PcvDesktopServiceSupport'
        $moduleText | Should -Not -Match 'New-PcvDesktopServiceProtectedTokenFile'
        $moduleText | Should -Not -Match 'Read-PcvDesktopServiceProtectedTokenFile'
        $moduleText | Should -Not -Match 'PcvDesktopService\.psm1'
    }
}

Describe 'New-PcvDesktopNodeProductPlan MSI installed actions' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:ProductRoot = Join-Path $TestDrive 'product-root'
        $script:DataRoot = Join-Path $TestDrive 'data-root'
        $script:WinSwPath = Join-Path $TestDrive 'winsw.exe'
        New-Item -ItemType Directory -Path $script:ProductRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $script:DataRoot -Force | Out-Null
        Set-Content -LiteralPath $script:WinSwPath -Value 'fake-winsw' -NoNewline
    }

    It 'marks ConfigureInstalled as elevated service configuration without file copy delete paths' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action ConfigureInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath

        $plan.action | Should -Be 'ConfigureInstalled'
        $plan.requires_elevation | Should -BeTrue
        $plan.delete_paths | Should -BeNullOrEmpty
        $plan.release.product_root | Should -Be $script:ProductRoot
        $plan.service.executable_path | Should -Be (Join-Path $script:ProductRoot 'DesktopNode.Host.exe')
    }

    It 'supports MSI installed payload root as SourceRoot for ConfigureInstalled' {
        $installedRoot = Join-Path $TestDrive 'installed-payload-root'
        New-Item -ItemType Directory -Path $installedRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $installedRoot 'DesktopNode.Host.exe') -Value 'fake-host' -NoNewline

        $previousErrorActionPreference = $ErrorActionPreference
        $ErrorActionPreference = 'Stop'
        $Error.Clear()
        try {
            $plan = New-PcvDesktopNodeProductPlan `
                -Action ConfigureInstalled `
                -SourceRoot $installedRoot `
                -ProductRoot $installedRoot `
                -DataRoot $script:DataRoot `
                -WinSwPath $script:WinSwPath
        }
        finally {
            $ErrorActionPreference = $previousErrorActionPreference
        }

        $plan.action | Should -Be 'ConfigureInstalled'
        $plan.source_root | Should -Be $installedRoot
        $plan.service.config.service_name | Should -Be 'PureCVisorDesktopNode'
        $plan.service.commands.install[0].file_name | Should -Be (Join-Path $installedRoot 'DesktopNode.Host.exe')
        $plan.service.commands.install[0].arguments | Should -Be @(
            'service-action',
            'configure-installed',
            '--product-root',
            $installedRoot,
            '--data-root',
            $script:DataRoot,
            '--service-exe',
            (Join-Path $installedRoot 'DesktopNode.Host.exe')
        )
        $plan.service.host.executable_path | Should -Be (Join-Path $installedRoot 'DesktopNode.Host.exe')
        @($Error | Where-Object {
                $_.Exception.Message -like '*PcvDesktopService.psm1*'
            }).Count | Should -Be 0
    }

    It 'uses explicit root-level .NET host executable for installed MSI Status' {
        $installedRoot = Join-Path $TestDrive 'installed-status-root'
        $installedHost = Join-Path $installedRoot 'DesktopNode.Host.exe'
        New-Item -ItemType Directory -Path $installedRoot -Force | Out-Null
        Set-Content -LiteralPath $installedHost -Value 'fake-installed-host' -NoNewline

        $plan = New-PcvDesktopNodeProductPlan `
            -Action Status `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $installedRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $installedHost

        $plan.action | Should -Be 'Status'
        $plan.service.executable_path | Should -Be $installedHost
        $plan.service.commands.status[0].file_name | Should -Be 'sc.exe'
        $plan.service.commands.status[0].arguments | Should -Be @('query', 'PureCVisorDesktopNode')
        $plan.service.host.executable_path | Should -Be $installedHost
    }

    It 'marks RepairInstalled as elevated and preserves token, jobs, events, and diagnostics' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RepairInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath

        $plan.action | Should -Be 'RepairInstalled'
        $plan.requires_elevation | Should -BeTrue
        @($plan.delete_paths).Count | Should -Be 0
        $plan.paths.token_protected_file | Should -Be (Join-Path $script:DataRoot 'api-token.dpapi.json')
        $plan.paths.token_file | Should -Be (Join-Path $script:DataRoot 'api-token.txt')
        $plan.paths.job_store | Should -Be (Join-Path $script:DataRoot 'jobs.json')
        $plan.paths.job_store_legacy_temp | Should -Be (Join-Path $script:DataRoot 'jobs.json.tmp')
        $plan.paths.job_store_pending_commit | Should -Be (Join-Path $script:DataRoot 'jobs.json.commit-pending')
        $plan.paths.job_store_temp_pattern | Should -Be (Join-Path $script:DataRoot 'jobs.json.tmp.*')
        $plan.paths.job_store_pending_commit_temp_pattern | Should -Be (Join-Path $script:DataRoot 'jobs.json.commit-pending.tmp.*')
        $plan.paths.event_log | Should -Be (Join-Path $script:DataRoot 'events.jsonl')
        $plan.paths.diagnostics_root | Should -Be (Join-Path $script:DataRoot 'diagnostics')
        $plan.token_file | Should -Be (Join-Path $script:DataRoot 'api-token.txt')
        $plan.job_store | Should -Be (Join-Path $script:DataRoot 'jobs.json')
        $plan.job_store_legacy_temp | Should -Be (Join-Path $script:DataRoot 'jobs.json.tmp')
        $plan.job_store_pending_commit | Should -Be (Join-Path $script:DataRoot 'jobs.json.commit-pending')
        $plan.job_store_temp_pattern | Should -Be (Join-Path $script:DataRoot 'jobs.json.tmp.*')
        $plan.job_store_pending_commit_temp_pattern | Should -Be (Join-Path $script:DataRoot 'jobs.json.commit-pending.tmp.*')
        $plan.event_log | Should -Be (Join-Path $script:DataRoot 'events.jsonl')
        $plan.diagnostics_root | Should -Be (Join-Path $script:DataRoot 'diagnostics')
    }

    It 'keeps RemoveInstalled default uninstall data-preserving and product-root neutral' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath

        $plan.action | Should -Be 'RemoveInstalled'
        $plan.requires_elevation | Should -BeTrue
        $plan.remove_data | Should -BeFalse
        $plan.delete_paths | Should -BeNullOrEmpty
    }

    It 'lists only ProgramData paths for RemoveInstalled -RemoveData' {
        $plan = New-PcvDesktopNodeProductPlan `
            -Action RemoveInstalled `
            -SourceRoot $script:RepoRoot `
            -ProductRoot $script:ProductRoot `
            -DataRoot $script:DataRoot `
            -WinSwPath $script:WinSwPath `
            -RemoveData

        $expected = @(
            (Join-Path $script:DataRoot 'api-token.dpapi.json'),
            (Join-Path $script:DataRoot 'api-token.txt'),
            (Join-Path $script:DataRoot 'accounts.json'),
            (Join-Path $script:DataRoot 'jwt-signing-key.txt'),
            (Join-Path $script:DataRoot 'jobs.json'),
            (Join-Path $script:DataRoot 'jobs.json.tmp'),
            (Join-Path $script:DataRoot 'jobs.json.commit-pending'),
            (Join-Path $script:DataRoot 'events.jsonl'),
            (Join-Path $script:DataRoot 'install.jsonl'),
            (Join-Path $script:DataRoot 'diagnostics')
        )

        $plan.action | Should -Be 'RemoveInstalled'
        $plan.requires_elevation | Should -BeTrue
        $plan.remove_data | Should -BeTrue
        $plan.delete_paths | Should -Be $expected
        $plan.delete_paths | Should -Not -Contain $script:ProductRoot
        $plan.delete_path_patterns | Should -Be @(
            (Join-Path $script:DataRoot 'jobs.json.tmp.*'),
            (Join-Path $script:DataRoot 'jobs.json.commit-pending.tmp.*')
        )
    }
}
