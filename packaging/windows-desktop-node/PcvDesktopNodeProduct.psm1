Set-StrictMode -Version Latest

$script:PcvDesktopNodeDiagnosticsDecision = 'DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred'

function Join-PcvProductPath {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][string[]]$ChildPath
    )

    $path = $Root
    foreach ($child in $ChildPath) {
        $path = Join-Path $path $child
    }

    $path
}

function Get-PcvDesktopNodeProductDefaults {
    $programData = [Environment]::GetEnvironmentVariable('ProgramData')
    if ([string]::IsNullOrWhiteSpace($programData)) {
        $programData = 'C:\ProgramData'
    }

    $dataRoot = Join-Path $programData 'PureCVisor\desktop-node'

    [ordered]@{
        service_name = 'PureCVisorDesktopNode'
        display_name = 'PureCVisor Desktop Node'
        prefix = 'http://127.0.0.1:7777/'
        web_prefix = 'http://127.0.0.1:80/'
        product_root = 'C:\Program Files\PureCVisor\DesktopNode'
        data_root = $dataRoot
        token_protected_file = Join-Path $dataRoot 'api-token.dpapi.json'
        token_file = Join-Path $dataRoot 'api-token.txt'
        account_file = Join-Path $dataRoot 'accounts.json'
        jwt_signing_key_file = Join-Path $dataRoot 'jwt-signing-key.txt'
        job_store = Join-Path $dataRoot 'jobs.json'
        job_store_legacy_temp = Join-Path $dataRoot 'jobs.json.tmp'
        job_store_pending_commit = Join-Path $dataRoot 'jobs.json.commit-pending'
        job_store_temp_pattern = Join-Path $dataRoot 'jobs.json.tmp.*'
        job_store_pending_commit_temp_pattern = Join-Path $dataRoot 'jobs.json.commit-pending.tmp.*'
        event_log = Join-Path $dataRoot 'events.jsonl'
        install_log = Join-Path $dataRoot 'install.jsonl'
        diagnostics_root = Join-Path $dataRoot 'diagnostics'
        winsw_exe_name = 'PureCVisorDesktopNode.exe'
        winsw_xml_name = 'PureCVisorDesktopNode.xml'
        service_logs_root = Join-Path $dataRoot 'service-logs'
        service_exe_name = 'DesktopNode.Host.exe'
        cli_exe_name = 'pcvcli.exe'
        route_timeout_seconds = 30
        request_limit_per_minute = 120
        request_burst_limit = 20
        retry_after_seconds = 15
        max_request_body_bytes = 1048576
    }
}

function Resolve-PcvDesktopNodeProductPaths {
    param(
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root,
        [string]$DataRoot = (Get-PcvDesktopNodeProductDefaults).data_root
    )

    $defaults = Get-PcvDesktopNodeProductDefaults
    [ordered]@{
        source_root = $SourceRoot
        product_root = $ProductRoot
        previous_product_root = "$ProductRoot.previous"
        data_root = $DataRoot
        web_root = Join-PcvProductPath -Root $ProductRoot -ChildPath @('web')
        token_protected_file = Join-Path $DataRoot 'api-token.dpapi.json'
        token_file = Join-Path $DataRoot 'api-token.txt'
        account_file = Join-Path $DataRoot 'accounts.json'
        jwt_signing_key_file = Join-Path $DataRoot 'jwt-signing-key.txt'
        job_store = Join-Path $DataRoot 'jobs.json'
        job_store_legacy_temp = Join-Path $DataRoot 'jobs.json.tmp'
        job_store_pending_commit = Join-Path $DataRoot 'jobs.json.commit-pending'
        job_store_temp_pattern = Join-Path $DataRoot 'jobs.json.tmp.*'
        job_store_pending_commit_temp_pattern = Join-Path $DataRoot 'jobs.json.commit-pending.tmp.*'
        event_log = Join-Path $DataRoot 'events.jsonl'
        install_log = Join-Path $DataRoot 'install.jsonl'
        diagnostics_root = Join-Path $DataRoot 'diagnostics'
        update_transaction_journal = Join-Path $DataRoot 'update-transaction.json'
        manifest_path = Join-PcvProductPath -Root $ProductRoot -ChildPath @('product-manifest.json')
        winsw_dir = Join-PcvProductPath -Root $ProductRoot -ChildPath @('winsw')
        winsw_exe = Join-PcvProductPath -Root $ProductRoot -ChildPath @('winsw', $defaults.winsw_exe_name)
        winsw_xml = Join-PcvProductPath -Root $ProductRoot -ChildPath @('winsw', $defaults.winsw_xml_name)
        service_exe = Join-PcvProductPath -Root $ProductRoot -ChildPath @($defaults.service_exe_name)
        cli_exe = Join-PcvProductPath -Root $ProductRoot -ChildPath @($defaults.cli_exe_name)
        service_logs_root = Join-PcvProductPath -Root $DataRoot -ChildPath @('service-logs')
    }
}

function Get-PcvDesktopNodeDiagnosticsPolicy {
    param(
        [Parameter(Mandatory)]$Paths
    )

    [ordered]@{
        schema_version = 1
        mode = 'windows-event-log-default-jsonl-retained'
        diagnostic_bundle_schema_version = 1
        redaction_version = 1
        event_log = [ordered]@{
            mode = 'jsonl-retained-fallback'
            path = $Paths.event_log
            max_file_bytes = 5242880
            retained_files = 5
        }
        install_log = [ordered]@{
            mode = 'jsonl'
            path = $Paths.install_log
            max_file_bytes = 5242880
            retained_files = 5
        }
        service_logs = [ordered]@{
            path = $Paths.service_logs_root
            max_file_bytes = 10485760
            retained_files = 10
        }
        windows_event_log = [ordered]@{
            enabled_by_default = $true
            source = 'PureCVisor Desktop Node'
            log_name = 'Application'
            registration_owner = 'msi-deferred-local-system-native-service-action'
            default_writer = 'windows-event-log'
            schema_version = 1
        }
    }
}

function Get-PcvDesktopNodeLanSecurityPolicy {
    [ordered]@{
        schema_version = 1
        decision = 'DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required'
        default_exposure = 'loopback'
        lan_mode = [ordered]@{
            state = 'preview-admin-opt-in'
            enabled_by_default = $false
            requires_allow_lan = $true
            requires_bearer_token = $true
            token_source = 'dpapi-local-machine-protected-file'
            non_loopback_static_auth = 'bearer-required'
        }
        tls = [ordered]@{
            provided_by_product_wrapper = $false
            required_for_lan = $true
            termination = 'external-reverse-proxy-or-tls-terminator'
        }
        firewall = [ordered]@{
            enabled_by_default = $false
            lifecycle_owner = 'admin-opt-in-product-action-or-manual-command'
            installer_auto_enable = $false
            default_profile = 'private'
        }
        diagnostics = [ordered]@{
            record_exposure = $true
            record_tls_stance = $true
            record_firewall_plan = $true
            record_token_storage = $true
        }
    }
}

function Get-PcvDesktopNodeDataAclPolicy {
    param(
        [Parameter(Mandatory)]$Paths
    )

    [ordered]@{
        schema_version = 1
        owner = 'product-wrapper'
        data_root = $Paths.data_root
        wix_manages_data_root_acl = $false
        installer_contract = 'msi-computes-programdata-path-product-action-manages-sensitive-file-acl'
        default_service_account = 'LocalSystem'
        required_principals = @(
            [ordered]@{
                identity = 'BUILTIN\Administrators'
                role = 'operator-remove-data-and-diagnostics'
            },
            [ordered]@{
                identity = 'NT AUTHORITY\SYSTEM'
                role = 'service-runtime-and-msi-deferred-action'
            }
        )
        sensitive_files = @(
            [ordered]@{
                name = 'api-token.dpapi.json'
                path = $Paths.token_protected_file
                storage = 'dpapi-local-machine'
                raw_token_on_command_line = $false
                acl_owner = 'service-token-helper'
                remove_data_acl_repair_required = $true
            },
            [ordered]@{
                name = 'api-token.txt'
                path = $Paths.token_file
                storage = 'legacy-plain-file'
                raw_token_on_command_line = $false
                acl_owner = 'service-token-helper'
                remove_data_acl_repair_required = $true
            },
            [ordered]@{
                name = 'accounts.json'
                path = $Paths.account_file
                storage = 'local-json-password-hashes'
                raw_token_on_command_line = $false
                acl_owner = 'account-auth-helper'
                remove_data_acl_repair_required = $true
            },
            [ordered]@{
                name = 'jwt-signing-key.txt'
                path = $Paths.jwt_signing_key_file
                storage = 'local-high-entropy-signing-key'
                raw_token_on_command_line = $false
                acl_owner = 'account-auth-helper'
                remove_data_acl_repair_required = $true
            }
        )
        non_mutating_verification = @(
            'product-plan-contract',
            'installer-wix-path-contract',
            'remove-data-acl-repair-tests'
        )
        host_acl_inspection = 'administrator-opt-in-only'
    }
}

function Get-PcvDesktopNodeUpdatePolicy {
    param(
        [Parameter(Mandatory)]$Paths
    )

    [ordered]@{
        schema_version = 1
        decision = 'DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration'
        version_source = 'product-wrapper-version-parameter'
        installed_manifest_is_source_of_truth = $true
        payload_version_must_match_manifest = $true
        rollback = [ordered]@{
            previous_root = $Paths.previous_product_root
            failed_root_suffix = '.failed'
            retained_previous_roots = 1
            rollback_requires_health_check = $true
        }
        config_migration = [ordered]@{
            mode = 'validate-before-service-start'
            dry_run_required = $true
            block_service_start_on_failure = $true
            data_backup_required_before_mutation = $true
        }
        job_store = [ordered]@{
            destructive_rewrite_by_default = $false
            schema_mismatch_mode = 'read-only-or-blocked-with-diagnostics'
        }
        provenance = [ordered]@{
            signed_release_required_for_release_channel = $true
            unsigned_dev_allowed_for_dev_channel = $true
        }
        source_resolution = [ordered]@{
            mode = 'local-or-https-package-with-sha256'
            allowed_schemes = @('file', 'https')
            expected_sha256_required = $true
            extracts_before_service_stop = $true
            host_mutation_before_validation = $false
        }
        catalog = [ordered]@{
            mode = 'file-or-https-json-channel-catalog'
            schema_version = 1
            allowed_schemes = @('file', 'https')
            channel_required = $true
            package_sha256_required = $true
            resolves_before_service_stop = $true
            host_mutation_before_validation = $false
            publication_claim = 'internal-catalog-only'
        }
        transaction_journal = [ordered]@{
            mode = 'single-active-update-journal'
            path = $Paths.update_transaction_journal
            write_before_service_stop = $true
            record_stage_transitions = $true
            full_transactional_filesystem = $true
        }
    }
}

function Get-PcvDesktopNodeProductAssets {
    param(
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root
    )

    [ordered]@{
        name = 'web'
        source = Join-PcvProductPath -Root $SourceRoot -ChildPath @('web')
        destination = Join-PcvProductPath -Root $ProductRoot -ChildPath @('web')
    }
}

function Get-PcvDesktopNodeAssetFileList {
    param(
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root
    )

    $assets = @(Get-PcvDesktopNodeProductAssets -SourceRoot $SourceRoot -ProductRoot $ProductRoot)
    foreach ($asset in $assets) {
        if (-not (Test-Path -LiteralPath $asset.source -PathType Container)) {
            throw "PCV_PRODUCT_ASSET_SOURCE_MISSING|Desktop Node product asset source directory is missing.|$($asset.source)"
        }

        $sourceRootPath = (Resolve-Path -LiteralPath $asset.source).Path.TrimEnd('\', '/')
        $files = Get-ChildItem -LiteralPath $sourceRootPath -File -Recurse -ErrorAction Stop |
            Where-Object { $_.FullName -notmatch '[\\/]tests[\\/]' } |
            Sort-Object FullName

        foreach ($file in $files) {
            $relativeWithinAsset = $file.FullName.Substring($sourceRootPath.Length).TrimStart('\', '/') -replace '/', '\'
            $relativePath = Join-PcvProductPath -Root $asset.name -ChildPath @($relativeWithinAsset)
            $destination = Join-PcvProductPath -Root $asset.destination -ChildPath @($relativeWithinAsset)

            [ordered]@{
                asset = $asset.name
                source = $file.FullName
                destination = $destination
                relative_path = $relativePath
                length = $file.Length
            }
        }
    }
}

function New-PcvDesktopNodeProductManifest {
    param(
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root,
        [string]$DataRoot = (Get-PcvDesktopNodeProductDefaults).data_root,
        [string]$Version = '0.12.0',
        [AllowNull()][string]$WinSwPath
    )

    $paths = Resolve-PcvDesktopNodeProductPaths `
        -SourceRoot $SourceRoot `
        -ProductRoot $ProductRoot `
        -DataRoot $DataRoot
    $servicePlan = New-PcvDesktopNodeServicePlan `
        -SourceRoot $SourceRoot `
        -Paths $paths `
        -ServiceName (Get-PcvDesktopNodeProductDefaults).service_name `
        -DisplayName (Get-PcvDesktopNodeProductDefaults).display_name `
        -Prefix (Get-PcvDesktopNodeProductDefaults).prefix `
        -WinSwPath $WinSwPath
    $assets = @(Get-PcvDesktopNodeAssetFileList -SourceRoot $SourceRoot -ProductRoot $ProductRoot)

    [ordered]@{
        schema_version = 2
        product = 'PureCVisor Desktop Node'
        version = $Version
        source_root = $SourceRoot
        generated_at = [DateTime]::UtcNow.ToString('o')
        paths = $paths
        assets = $assets
        service_host = [ordered]@{
            mode = $servicePlan.mode
            executable_path = $servicePlan.host.executable_path
            default_owner = 'dotnet-windows-service-host'
        }
        cli = [ordered]@{
            mode = 'dotnet-local-api-client'
            command_name = 'pcvcli'
            executable_path = $paths.cli_exe
            default_owner = 'desktop-node-product-cli'
            token_sources = @(
                '--token',
                '--token-file',
                '--token-env',
                '--protected-token-file'
            )
        }
        auth = [ordered]@{
            api_token_source = 'protected_file'
            api_token_storage = 'dpapi-local-machine'
            api_token_protected_file = $paths.token_protected_file
            legacy_api_token_file = $paths.token_file
            account_auth_source = 'local-json-and-jwt-signing-key'
            account_file = $paths.account_file
            jwt_signing_key_file = $paths.jwt_signing_key_file
            account_auth_default_state = 'not-configured-until-accounts-exist'
            rbac_roles = @('viewer', 'operator', 'admin')
        }
        data_acl = Get-PcvDesktopNodeDataAclPolicy -Paths $paths
        network = Get-PcvDesktopNodeLanSecurityPolicy
        update = Get-PcvDesktopNodeUpdatePolicy -Paths $paths
        diagnostics = Get-PcvDesktopNodeDiagnosticsPolicy -Paths $paths
    }
}

function Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths {
    @(
        'DesktopNode.Host.exe',
        'pcvcli.exe',
        'Invoke-PcvDesktopNodeProduct.ps1',
        'PcvDesktopNodeProduct.psm1'
    )
}

function Get-PcvDesktopNodeRuntimePayloadFileList {
    param(
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root
    )

    foreach ($relativePath in @(Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths)) {
        $source = Join-PcvProductPath -Root $SourceRoot -ChildPath @($relativePath)
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            [ordered]@{
                source = $source
                destination = Join-PcvProductPath -Root $ProductRoot -ChildPath @($relativePath)
                relative_path = $relativePath
            }
        }
    }
}

function Test-PcvDesktopNodeUpdatePayload {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [Parameter(Mandatory)][string]$ProductRoot,
        [Parameter(Mandatory)][string]$Version
    )

    if (-not (Test-Path -LiteralPath $SourceRoot -PathType Container)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_PAYLOAD_SOURCE_MISSING' `
                -Message 'Desktop Node update payload source is missing.' `
                -Detail $SourceRoot)
    }

    $sourceFull = (Resolve-Path -LiteralPath $SourceRoot -ErrorAction Stop).Path.TrimEnd('\', '/')
    $productFull = [System.IO.Path]::GetFullPath($ProductRoot).TrimEnd('\', '/')
    $productPrefix = $productFull + [System.IO.Path]::DirectorySeparatorChar
    if ([string]::Equals($sourceFull, $productFull, [System.StringComparison]::OrdinalIgnoreCase) -or
        $sourceFull.StartsWith($productPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_PAYLOAD_SOURCE_ACTIVE_ROOT' `
                -Message 'Desktop Node update payload source must be staged outside the active product root.' `
                -Detail $SourceRoot)
    }

    $manifestPath = Join-PcvProductPath -Root $SourceRoot -ChildPath @('product-manifest.json')
    try {
        $manifest = Read-PcvDesktopNodeProductManifest -Path $manifestPath
    }
    catch {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_PAYLOAD_MANIFEST_INVALID' `
                -Message 'Desktop Node update payload manifest is missing or invalid.' `
                -Detail $_.Exception.Message)
    }

    if ([string]$manifest.version -ne $Version) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_PAYLOAD_VERSION_MISMATCH' `
                -Message 'Desktop Node update payload manifest version does not match the requested update version.' `
                -Detail "manifest=$($manifest.version); requested=$Version")
    }

    $requiredRelativePaths = @(
        @(Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths)
        'web\app.js',
        'web\index.html',
        'web\styles.css'
    )
    $missing = @()
    $files = @()
    foreach ($relativePath in $requiredRelativePaths) {
        $source = Join-PcvProductPath -Root $SourceRoot -ChildPath @($relativePath)
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
            $missing += $relativePath
            continue
        }

        $files += [ordered]@{
            relative_path = $relativePath
            sha256 = Get-PcvFileSha256 -Path $source
        }
    }

    if ($missing.Count -gt 0) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_PAYLOAD_FILE_MISSING' `
                -Message 'Desktop Node update payload is missing required runtime files.' `
                -Detail ($missing -join ', '))
    }

    [ordered]@{
        ok = $true
        source_root = $SourceRoot
        manifest_path = $manifestPath
        version = [string]$manifest.version
        file_count = $files.Count
        files = $files
    }
}

function Copy-PcvDesktopNodeRuntimePayloadFiles {
    param(
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root
    )

    $requiredRootRelativePaths = @(Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths)
    $existingRootRelativePaths = @()
    $missingRootRelativePaths = @()
    foreach ($relativePath in $requiredRootRelativePaths) {
        $source = Join-PcvProductPath -Root $SourceRoot -ChildPath @($relativePath)
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            $existingRootRelativePaths += $relativePath
        }
        else {
            $missingRootRelativePaths += $relativePath
        }
    }

    if ($existingRootRelativePaths.Count -gt 0 -and $missingRootRelativePaths.Count -gt 0) {
        throw "PCV_PRODUCT_RUNTIME_PAYLOAD_FILE_MISSING|Desktop Node runtime payload is missing required root files.|$($missingRootRelativePaths -join ', ')"
    }

    $runtimePayloadFiles = @(Get-PcvDesktopNodeRuntimePayloadFileList -SourceRoot $SourceRoot -ProductRoot $ProductRoot)
    foreach ($file in $runtimePayloadFiles) {
        $destinationDirectory = Split-Path -Parent $file.destination
        New-Item -ItemType Directory -Path $destinationDirectory -Force -ErrorAction Stop | Out-Null
        Copy-Item -LiteralPath $file.source -Destination $file.destination -Force -ErrorAction Stop
    }

    [ordered]@{
        ok = $true
        file_count = $runtimePayloadFiles.Count
        files = $runtimePayloadFiles
    }
}

function Copy-PcvDesktopNodeWinSwArtifact {
    param(
        [Parameter(Mandatory)]$ServicePlan,
        [Parameter(Mandatory)]$Paths
    )

    if ($null -ne $ServicePlan.winsw.missing_source_error) {
        throw "$($ServicePlan.winsw.missing_source_error.code)|$($ServicePlan.winsw.missing_source_error.message)|$($ServicePlan.winsw.missing_source_error.detail)"
    }

    New-Item -ItemType Directory -Path $Paths.winsw_dir -Force -ErrorAction Stop | Out-Null
    New-Item -ItemType Directory -Path $Paths.service_logs_root -Force -ErrorAction Stop | Out-Null
    Copy-Item `
        -LiteralPath $ServicePlan.winsw.source_path `
        -Destination $ServicePlan.winsw.staged_path `
        -Force `
        -ErrorAction Stop
    $config = Write-PcvDesktopNodeWinSwConfiguration -ServicePlan $ServicePlan

    [ordered]@{
        ok = $true
        executable = $ServicePlan.winsw.staged_path
        xml = $config.xml
        source_sha256 = $ServicePlan.winsw.source_sha256
    }
}

function Write-PcvDesktopNodeWinSwConfiguration {
    param([Parameter(Mandatory)]$ServicePlan)

    $xmlDirectory = Split-Path -Parent $ServicePlan.winsw.xml_path
    New-Item -ItemType Directory -Path $xmlDirectory -Force -ErrorAction Stop | Out-Null
    Set-Content `
        -LiteralPath $ServicePlan.winsw.xml_path `
        -Value $ServicePlan.winsw.xml `
        -Encoding UTF8 `
        -ErrorAction Stop

    [ordered]@{
        ok = $true
        executable = $ServicePlan.winsw.staged_path
        xml = $ServicePlan.winsw.xml_path
    }
}

function Copy-PcvDesktopNodeProductAssets {
    param(
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root,
        [string]$DataRoot = (Get-PcvDesktopNodeProductDefaults).data_root,
        [string]$Version = '0.12.0',
        [AllowNull()][string]$WinSwPath
    )

    $manifest = New-PcvDesktopNodeProductManifest `
        -SourceRoot $SourceRoot `
        -ProductRoot $ProductRoot `
        -DataRoot $DataRoot `
        -Version $Version `
        -WinSwPath $WinSwPath

    foreach ($asset in $manifest.assets) {
        $destinationDirectory = Split-Path -Parent $asset.destination
        New-Item -ItemType Directory -Path $destinationDirectory -Force -ErrorAction Stop | Out-Null
        Copy-Item -LiteralPath $asset.source -Destination $asset.destination -Force -ErrorAction Stop
    }
    $runtimePayloadResult = Copy-PcvDesktopNodeRuntimePayloadFiles `
        -SourceRoot $SourceRoot `
        -ProductRoot $ProductRoot

    $paths = Resolve-PcvDesktopNodeProductPaths `
        -SourceRoot $SourceRoot `
        -ProductRoot $ProductRoot `
        -DataRoot $DataRoot
    $servicePlan = New-PcvDesktopNodeServicePlan `
        -SourceRoot $SourceRoot `
        -Paths $paths `
        -ServiceName (Get-PcvDesktopNodeProductDefaults).service_name `
        -DisplayName (Get-PcvDesktopNodeProductDefaults).display_name `
        -Prefix (Get-PcvDesktopNodeProductDefaults).prefix `
        -WinSwPath $WinSwPath
    New-Item -ItemType Directory -Path $ProductRoot -Force -ErrorAction Stop | Out-Null
    $manifestPath = $manifest.paths.manifest_path
    $manifest | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $manifestPath -Encoding utf8 -ErrorAction Stop

    [ordered]@{
        ok = $true
        product_root = $ProductRoot
        manifest_path = $manifestPath
        asset_count = @($manifest.assets).Count
        runtime_payload_count = [int]$runtimePayloadResult.file_count
        service_host = $servicePlan.host
    }
}

function Read-PcvDesktopNodeProductManifest {
    param(
        [Parameter(Mandatory)][string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "PCV_PRODUCT_MANIFEST_MISSING|Product manifest is missing.|$Path"
    }

    try {
        $manifest = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "PCV_PRODUCT_MANIFEST_INVALID_JSON|Product manifest is not valid JSON.|$Path"
    }

    $propertyNames = @($manifest.PSObject.Properties.Name)
    $hasRequiredProperties = $propertyNames -contains 'schema_version' -and
        $propertyNames -contains 'product' -and
        $propertyNames -contains 'version'
    $supportedSchemaVersion = $manifest.schema_version -in @(1, 2)
    if (-not $hasRequiredProperties -or
        -not $supportedSchemaVersion -or
        $manifest.product -ne 'PureCVisor Desktop Node' -or
        [string]::IsNullOrWhiteSpace([string]$manifest.version)) {
        throw "PCV_PRODUCT_MANIFEST_INVALID|Product manifest schema, product, or version is invalid.|$Path"
    }

    $manifest
}

function New-PcvDesktopNodeConfigMigrationPlan {
    param(
        [Parameter(Mandatory)][string]$FromVersion,
        [Parameter(Mandatory)][string]$ToVersion,
        [Parameter(Mandatory)]$Paths,
        [switch]$DryRun
    )

    [ordered]@{
        schema_version = 1
        from_version = $FromVersion
        to_version = $ToVersion
        dry_run = [bool]$DryRun
        service_start_allowed = $true
        steps = @(
            [ordered]@{
                name = 'validate-protected-token-source'
                mutation = $false
                required = $true
                status = 'planned'
            },
            [ordered]@{
                name = 'validate-job-store-compatibility'
                mutation = $false
                required = $true
                status = 'planned'
            }
        )
        backups = @(
            [ordered]@{
                source = 'jobs.json'
                artifact = ('jobs.json.pre-{0}.bak' -f $ToVersion)
                required_before_mutation = $true
            }
        )
        paths = [ordered]@{
            job_store = $Paths.job_store
        }
    }
}

function Test-PcvDesktopNodeJobStorePendingCommitGuard {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return [ordered]@{
            ok = $false
            path = $Path
            status = 'inspection-failed'
            pending_commit_present = $null
            error = [ordered]@{
                code = 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
                message = 'Desktop Node job store pending-commit state could not be inspected.'
                detail = 'The pending-commit marker path is missing from the product plan.'
            }
        }
    }

    try {
        [void][System.IO.File]::GetAttributes($Path)
        return [ordered]@{
            ok = $false
            path = $Path
            status = 'present'
            pending_commit_present = $true
            error = [ordered]@{
                code = 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
                message = 'Desktop Node product mutation is blocked by an unresolved job store commit.'
                detail = "Reconcile the pending commit with the current runtime before retrying: $Path"
            }
        }
    }
    catch [System.IO.FileNotFoundException] {
        return [ordered]@{
            ok = $true
            path = $Path
            status = 'absent'
            pending_commit_present = $false
            error = $null
        }
    }
    catch [System.IO.DirectoryNotFoundException] {
        return [ordered]@{
            ok = $true
            path = $Path
            status = 'absent'
            pending_commit_present = $false
            error = $null
        }
    }
    catch {
        return [ordered]@{
            ok = $false
            path = $Path
            status = 'inspection-failed'
            pending_commit_present = $null
            error = [ordered]@{
                code = 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED'
                message = 'Desktop Node job store pending-commit state could not be inspected.'
                detail = $_.Exception.Message
            }
        }
    }
}

function Assert-PcvDesktopNodeJobStorePendingCommitGuard {
    param(
        [Parameter(Mandatory)]$Result
    )

    if ($Result.Contains('ok') -and [bool]$Result.ok) {
        return
    }

    if ($Result.Contains('error') -and $null -ne $Result.error) {
        throw "$($Result.error.code)|$($Result.error.message)|$($Result.error.detail)"
    }

    throw 'PCV_PRODUCT_JOB_STORE_PENDING_COMMIT_UNRESOLVED|Desktop Node job store pending-commit state could not be inspected.|The pending-commit guard returned an invalid result.'
}

function Find-PcvDesktopNodeJobStoreOrphanTempPaths {
    param(
        [Parameter(Mandatory)]$Paths
    )

    $definitions = @(
        [ordered]@{
            base_path = [string]$Paths.job_store
            pattern = [string]$Paths.job_store_temp_pattern
        },
        [ordered]@{
            base_path = [string]$Paths.job_store_pending_commit
            pattern = [string]$Paths.job_store_pending_commit_temp_pattern
        }
    )
    $candidates = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)

    try {
        foreach ($definition in $definitions) {
            if ([string]::IsNullOrWhiteSpace($definition.base_path) -or
                [string]::IsNullOrWhiteSpace($definition.pattern)) {
                throw 'The job-store orphan temporary-file paths are missing from the product plan.'
            }

            $expectedPattern = $definition.base_path + '.tmp.*'
            if (-not [string]::Equals(
                    $definition.pattern,
                    $expectedPattern,
                    [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "The job-store orphan temporary-file pattern is outside the allowlist: $($definition.pattern)"
            }

            $directory = [System.IO.Path]::GetDirectoryName($definition.base_path)
            $baseName = [System.IO.Path]::GetFileName($definition.base_path)
            if ([string]::IsNullOrWhiteSpace($directory) -or
                [string]::IsNullOrWhiteSpace($baseName)) {
                throw "The job-store orphan temporary-file base path is invalid: $($definition.base_path)"
            }
            if (-not [System.IO.Directory]::Exists($directory)) {
                continue
            }

            $ownedPrefix = $baseName + '.tmp.'
            foreach ($candidate in [System.IO.Directory]::EnumerateFiles(
                    $directory,
                    ($ownedPrefix + '*'),
                    [System.IO.SearchOption]::TopDirectoryOnly)) {
                $candidateName = [System.IO.Path]::GetFileName($candidate)
                if (-not $candidateName.StartsWith(
                        $ownedPrefix,
                        [System.StringComparison]::OrdinalIgnoreCase)) {
                    continue
                }

                $suffix = $candidateName.Substring($ownedPrefix.Length)
                $ownedGuid = [Guid]::Empty
                if ([Guid]::TryParseExact($suffix, 'N', [ref]$ownedGuid)) {
                    [void]$candidates.Add($candidate)
                }
            }
        }
    }
    catch {
        return [ordered]@{
            ok = $false
            paths = @()
            patterns = @($definitions | ForEach-Object { [string]$_.pattern })
            error = [ordered]@{
                code = 'PCV_PRODUCT_JOB_STORE_ORPHAN_TEMP_DISCOVERY_FAILED'
                message = 'Desktop Node job-store orphan temporary files could not be inspected.'
                detail = $_.Exception.Message
            }
        }
    }

    [ordered]@{
        ok = $true
        paths = @($candidates | Sort-Object)
        patterns = @($definitions | ForEach-Object { [string]$_.pattern })
        error = $null
    }
}

function Assert-PcvDesktopNodeJobStoreOrphanTempDiscovery {
    param(
        [Parameter(Mandatory)]$Result
    )

    if ($Result.Contains('ok') -and [bool]$Result.ok) {
        return
    }

    if ($Result.Contains('error') -and $null -ne $Result.error) {
        throw "$($Result.error.code)|$($Result.error.message)|$($Result.error.detail)"
    }

    throw 'PCV_PRODUCT_JOB_STORE_ORPHAN_TEMP_DISCOVERY_FAILED|Desktop Node job-store orphan temporary files could not be inspected.|The orphan temporary-file discovery returned an invalid result.'
}

function Invoke-PcvDesktopNodeRollbackQuiesce {
    param(
        [Parameter(Mandatory)]$Plan,
        [Parameter(Mandatory)][scriptblock]$InvokeProcess
    )

    $stopResult = Invoke-PcvProductProcessCommand `
        -Commands $Plan.service.commands.stop `
        -InvokeProcess $InvokeProcess
    $stopResult = Update-PcvWinSwStopCommandResultAllowed `
        -CommandResult $stopResult
    if (-not [bool]$stopResult.ok) {
        $errorResult = $stopResult.error
        if ($null -eq $errorResult) {
            $errorResult = [ordered]@{
                code = 'PCV_PRODUCT_COMMAND_FAILED'
                message = 'Desktop Node product rollback service stop failed.'
                detail = 'The service must be stopped before the previous product root can be restored.'
            }
        }

        return [ordered]@{
            ok = $false
            stop = $stopResult
            stop_wait = $null
            error = $errorResult
        }
    }

    $stopWaitResult = Wait-PcvWinSwServiceStopped `
        -Plan $Plan `
        -InvokeProcess $InvokeProcess
    if ($stopWaitResult.Contains('ok') -and -not [bool]$stopWaitResult.ok) {
        $errorResult = $stopWaitResult.error
        if ($null -eq $errorResult) {
            $errorResult = [ordered]@{
                code = 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT'
                message = 'Desktop Node service did not stop before the timeout.'
                detail = 'The service must report stopped before the previous product root can be restored.'
            }
        }

        return [ordered]@{
            ok = $false
            stop = $stopResult
            stop_wait = $stopWaitResult
            error = $errorResult
        }
    }

    [ordered]@{
        ok = $true
        stop = $stopResult
        stop_wait = $stopWaitResult
        error = $null
    }
}

function Assert-PcvDesktopNodeRollbackQuiesced {
    param(
        [Parameter(Mandatory)]$Result
    )

    if ($Result.Contains('ok') -and [bool]$Result.ok) {
        return
    }

    if ($Result.Contains('error') -and $null -ne $Result.error) {
        throw "$($Result.error.code)|$($Result.error.message)|$($Result.error.detail)"
    }

    throw 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT|Desktop Node service did not stop before rollback.|Rollback quiescence returned an invalid result.'
}

function Get-PcvProductTokenStoreEntropy {
    [System.Text.Encoding]::UTF8.GetBytes('PureCVisor Desktop Node API Token Store v1')
}

function New-PcvProductToken {
    param([ValidateRange(16, 128)][int]$ByteLength = 32)

    $bytes = [byte[]]::new($ByteLength)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
    }
    finally {
        $rng.Dispose()
    }

    [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function Get-PcvProductTokenSha256 {
    param([Parameter(Mandatory)][string]$Token)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Token)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
    }
    finally {
        $sha.Dispose()
    }
}

function Initialize-PcvProductProtectedDataSupport {
    if ('System.Security.Cryptography.ProtectedData' -as [type]) {
        return
    }

    try {
        Add-Type -AssemblyName System.Security -ErrorAction Stop
    }
    catch {
        throw "PCV_PRODUCT_PROTECTED_DATA_UNAVAILABLE|DPAPI ProtectedData support could not be loaded.|$($_.Exception.Message)"
    }

    if (-not ('System.Security.Cryptography.ProtectedData' -as [type])) {
        throw 'PCV_PRODUCT_PROTECTED_DATA_UNAVAILABLE|DPAPI ProtectedData support is unavailable.|System.Security.Cryptography.ProtectedData type was not found after loading System.Security.'
    }
}

function Protect-PcvProductToken {
    param([Parameter(Mandatory)][string]$Token)

    if ([string]::IsNullOrWhiteSpace($Token)) {
        throw 'PCV_PRODUCT_TOKEN_EMPTY|The service API token must not be empty.|Pass a non-empty token or omit -Token to generate one.'
    }

    Initialize-PcvProductProtectedDataSupport
    $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes($Token)
    $protectedBytes = [System.Security.Cryptography.ProtectedData]::Protect(
        $tokenBytes,
        (Get-PcvProductTokenStoreEntropy),
        [System.Security.Cryptography.DataProtectionScope]::LocalMachine
    )
    [Convert]::ToBase64String($protectedBytes)
}

function Unprotect-PcvProductToken {
    param([Parameter(Mandatory)][string]$ProtectedToken)

    if ([string]::IsNullOrWhiteSpace($ProtectedToken)) {
        throw 'PCV_PRODUCT_PROTECTED_TOKEN_EMPTY|The protected service API token is empty.|The protected token file must contain a non-empty protected_token value.'
    }

    try {
        Initialize-PcvProductProtectedDataSupport
        $protectedBytes = [Convert]::FromBase64String($ProtectedToken)
        $tokenBytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
            $protectedBytes,
            (Get-PcvProductTokenStoreEntropy),
            [System.Security.Cryptography.DataProtectionScope]::LocalMachine
        )
        [System.Text.Encoding]::UTF8.GetString($tokenBytes)
    }
    catch {
        throw "PCV_PRODUCT_PROTECTED_TOKEN_UNPROTECT_FAILED|The protected service API token could not be read.|$($_.Exception.Message)"
    }
}

function Get-PcvProductObjectPropertyValue {
    param(
        [Parameter(Mandatory)]$InputObject,
        [Parameter(Mandatory)][string]$Name
    )

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    $property.Value
}

function Resolve-PcvProductServiceAccountAclPrincipal {
    param([AllowNull()][string]$ServiceAccount = 'LocalSystem')

    if ([string]::IsNullOrWhiteSpace($ServiceAccount)) {
        return 'NT AUTHORITY\SYSTEM'
    }

    switch ($ServiceAccount.ToLowerInvariant()) {
        'localsystem' { 'NT AUTHORITY\SYSTEM'; break }
        'system' { 'NT AUTHORITY\SYSTEM'; break }
        'nt authority\system' { 'NT AUTHORITY\SYSTEM'; break }
        'localservice' { 'NT AUTHORITY\LOCAL SERVICE'; break }
        'nt authority\local service' { 'NT AUTHORITY\LOCAL SERVICE'; break }
        'networkservice' { 'NT AUTHORITY\NETWORK SERVICE'; break }
        'nt authority\network service' { 'NT AUTHORITY\NETWORK SERVICE'; break }
        default { $ServiceAccount }
    }
}

function Invoke-PcvProductProtectedTokenAclApply {
    param(
        [Parameter(Mandatory)][string]$Path,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$AdminPrincipal = 'BUILTIN\Administrators',
        [scriptblock]$InvokeProcess
    )

    if ($null -eq $InvokeProcess) {
        $InvokeProcess = {
            param([string]$FileName, [string[]]$Arguments)
            Invoke-PcvProductNativeProcess -FileName $FileName -Arguments $Arguments
        }
    }

    $servicePrincipal = Resolve-PcvProductServiceAccountAclPrincipal -ServiceAccount $ServiceAccount
    $readerGrants = @("${AdminPrincipal}:R")
    if (-not [string]::Equals($servicePrincipal, $AdminPrincipal, [System.StringComparison]::OrdinalIgnoreCase)) {
        $readerGrants += "${servicePrincipal}:R"
    }

    $commands = @(
        [ordered]@{
            file_name = 'icacls.exe'
            arguments = @($Path, '/inheritance:r')
            action = 'disable_inheritance'
        },
        [ordered]@{
            file_name = 'icacls.exe'
            arguments = @($Path, '/grant:r') + $readerGrants
            action = 'grant_read'
        }
    )

    $results = @()
    foreach ($command in $commands) {
        $result = & $InvokeProcess -FileName $command.file_name -Arguments $command.arguments
        $results += [ordered]@{
            action = $command.action
            command = $command
            result = $result
            ok = ([int]$result.exit_code -eq 0)
        }
    }

    $failed = @($results | Where-Object { -not [bool]$_.ok })
    $aclResult = [ordered]@{
        ok = ($failed.Count -eq 0)
        path = $Path
        service_account = $ServiceAccount
        service_acl_principal = $servicePrincipal
        admin_principal = $AdminPrincipal
        commands = $commands
        results = $results
    }
    if ($failed.Count -gt 0) {
        $aclResult.error = [ordered]@{
            code = 'PCV_PRODUCT_PROTECTED_TOKEN_ACL_FAILED'
            message = 'Desktop Node protected token ACL hardening failed.'
            detail = "icacls.exe returned exit code $($failed[0].result.exit_code)."
        }
    }

    $aclResult
}

function New-PcvDesktopNodeProductProtectedTokenFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Token,
        [ValidateRange(16, 128)][int]$TokenByteLength = 32,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$AdminPrincipal = 'BUILTIN\Administrators',
        [switch]$Force,
        [scriptblock]$InvokeProcess
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_PRODUCT_PROTECTED_TOKEN_FILE_PATH_REQUIRED|The protected token file path is required.|Pass -ApiTokenProtectedFile or use the default ProgramData protected token path.'
    }

    if ($PSBoundParameters.ContainsKey('Token') -and [string]::IsNullOrWhiteSpace($Token)) {
        throw 'PCV_PRODUCT_TOKEN_EMPTY|The service API token must not be empty.|Pass a non-empty token or omit -Token to generate one.'
    }

    if ((Test-Path -LiteralPath $Path -PathType Leaf) -and -not $Force) {
        throw "PCV_PRODUCT_PROTECTED_TOKEN_FILE_EXISTS|The protected service API token file already exists.|Pass -Force to rotate '$Path'."
    }

    $tokenValue = $Token
    if (-not $PSBoundParameters.ContainsKey('Token')) {
        $tokenValue = New-PcvProductToken -ByteLength $TokenByteLength
    }

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $record = [ordered]@{
        schema_version = 1
        storage = 'dpapi-local-machine'
        scope = 'LocalMachine'
        created_at = (Get-Date).ToUniversalTime().ToString('o')
        token_sha256 = Get-PcvProductTokenSha256 -Token $tokenValue
        protected_token = Protect-PcvProductToken -Token $tokenValue
    }
    $record | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $Path -Encoding UTF8 -ErrorAction Stop

    $aclParams = @{
        Path = $Path
        ServiceAccount = $ServiceAccount
        AdminPrincipal = $AdminPrincipal
    }
    if ($null -ne $InvokeProcess) {
        $aclParams.InvokeProcess = $InvokeProcess
    }
    $aclResult = Invoke-PcvProductProtectedTokenAclApply @aclParams
    $errorValue = if ($aclResult.Contains('error')) { $aclResult.error } else { $null }

    [ordered]@{
        ok = $aclResult.ok
        path = $Path
        storage = 'dpapi-local-machine'
        scope = 'LocalMachine'
        token_length = $tokenValue.Length
        service_account = $ServiceAccount
        service_acl_principal = (Resolve-PcvProductServiceAccountAclPrincipal -ServiceAccount $ServiceAccount)
        admin_principal = $AdminPrincipal
        acl = $aclResult
        error = $errorValue
    }
}

function Read-PcvDesktopNodeProductProtectedTokenFile {
    param([Parameter(Mandatory)][string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_PRODUCT_PROTECTED_TOKEN_FILE_PATH_REQUIRED|The protected token file path is required.|Pass -ApiTokenProtectedFile or use the default ProgramData protected token path.'
    }

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "PCV_PRODUCT_PROTECTED_TOKEN_FILE_NOT_FOUND|The protected API token file was not found.|Create the protected token file before starting the listener: '$Path'."
    }

    try {
        $json = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "PCV_PRODUCT_PROTECTED_TOKEN_FILE_INVALID|The protected API token file is not valid JSON.|$($_.Exception.Message)"
    }

    $schemaVersion = Get-PcvProductObjectPropertyValue -InputObject $json -Name 'schema_version'
    $storage = [string](Get-PcvProductObjectPropertyValue -InputObject $json -Name 'storage')
    $scope = [string](Get-PcvProductObjectPropertyValue -InputObject $json -Name 'scope')
    $protectedToken = [string](Get-PcvProductObjectPropertyValue -InputObject $json -Name 'protected_token')

    if ([int]$schemaVersion -ne 1 -or
        $storage -ne 'dpapi-local-machine' -or
        $scope -ne 'LocalMachine' -or
        [string]::IsNullOrWhiteSpace($protectedToken)) {
        throw "PCV_PRODUCT_PROTECTED_TOKEN_FILE_INVALID|The protected API token file schema is invalid.|Expected schema_version 1, storage dpapi-local-machine, scope LocalMachine, and protected_token in '$Path'."
    }

    $token = Unprotect-PcvProductToken -ProtectedToken $protectedToken
    if ([string]::IsNullOrWhiteSpace($token)) {
        throw "PCV_PRODUCT_PROTECTED_TOKEN_EMPTY|The protected service API token resolved to an empty value.|Rotate the protected token file: '$Path'."
    }

    [ordered]@{
        ok = $true
        path = $Path
        storage = 'dpapi-local-machine'
        scope = 'LocalMachine'
        token = $token
        token_length = $token.Length
        token_sha256 = Get-PcvProductTokenSha256 -Token $token
    }
}

function ConvertTo-PcvNativeProcessArgument {
    param([AllowNull()][string]$Value)

    if ($null -eq $Value) {
        $Value = ''
    }
    if ($Value.Length -gt 0 -and $Value -notmatch '[\s"]') {
        return $Value
    }

    $builder = [System.Text.StringBuilder]::new()
    [void]$builder.Append('"')
    $backslashes = 0
    foreach ($character in $Value.ToCharArray()) {
        if ($character -eq '\') {
            $backslashes += 1
            continue
        }

        if ($character -eq '"') {
            if ($backslashes -gt 0) {
                [void]$builder.Append('\' * ($backslashes * 2))
                $backslashes = 0
            }
            [void]$builder.Append('\"')
            continue
        }

        if ($backslashes -gt 0) {
            [void]$builder.Append('\' * $backslashes)
            $backslashes = 0
        }
        [void]$builder.Append($character)
    }

    if ($backslashes -gt 0) {
        [void]$builder.Append('\' * ($backslashes * 2))
    }
    [void]$builder.Append('"')
    $builder.ToString()
}

function ConvertTo-PcvNativeProcessArgumentString {
    param([string[]]$Arguments)

    (@($Arguments) | ForEach-Object {
        ConvertTo-PcvNativeProcessArgument -Value $_
    }) -join ' '
}

function Invoke-PcvProductNativeProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = $FileName
    $process.StartInfo.Arguments = ConvertTo-PcvNativeProcessArgumentString -Arguments $Arguments
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    $process.StartInfo.CreateNoWindow = $true

    [void]$process.Start()
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $process.WaitForExit()
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()

    [ordered]@{
        exit_code = $process.ExitCode
        stdout = $stdout
        stderr = $stderr
    }
}

function Invoke-PcvProductProcessCommand {
    param(
        [Parameter(Mandatory)]$Commands,
        [scriptblock]$InvokeProcess
    )

    if ($null -eq $InvokeProcess) {
        $InvokeProcess = {
            param([string]$FileName, [string[]]$Arguments)
            Invoke-PcvProductNativeProcess -FileName $FileName -Arguments $Arguments
        }
    }

    $results = @()
    foreach ($command in @($Commands)) {
        $rebootGuard = Test-PcvProductAutoRebootCommand -FileName $command.file_name -Arguments ([string[]]$command.arguments)
        if (-not [bool]$rebootGuard.ok) {
            return [ordered]@{
                ok = $false
                results = $results
                error = $rebootGuard.error
            }
        }

        $processResult = & $InvokeProcess `
            -FileName $command.file_name `
            -Arguments ([string[]]$command.arguments)

        $result = [ordered]@{
            file_name = $command.file_name
            arguments = @($command.arguments)
            exit_code = [int]$processResult.exit_code
            stdout = [string]$processResult.stdout
            stderr = [string]$processResult.stderr
        }
        $results += $result

        if ($result.exit_code -ne 0) {
            return [ordered]@{
                ok = $false
                results = $results
                error = [ordered]@{
                    code = 'PCV_PRODUCT_COMMAND_FAILED'
                    message = 'Desktop Node product command failed.'
                    detail = "Command '$($command.file_name) $($command.arguments -join ' ')' exited with code $($result.exit_code)."
                }
            }
        }
    }

    [ordered]@{
        ok = $true
        results = $results
        error = $null
    }
}

function Test-PcvProductAutoRebootCommand {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [string[]]$Arguments = @()
    )

    $leaf = Split-Path -Leaf $FileName
    $commandText = @($FileName) + @($Arguments) -join ' '
    $forbidden = $false
    if ($leaf -match '^(Restart-Computer|shutdown|shutdown\.exe)$') {
        $forbidden = $true
    }
    elseif ($leaf -match '^(msiexec|msiexec\.exe)$') {
        foreach ($argument in @($Arguments)) {
            if ($argument -match '^(?i)(/forcerestart|REBOOT=Force|REBOOT=ForceRestart)$') {
                $forbidden = $true
                break
            }
        }
    }

    if ($forbidden) {
        return [ordered]@{
            ok = $false
            error = [ordered]@{
                code = 'PCV_PRODUCT_AUTO_REBOOT_FORBIDDEN'
                message = 'Automatic reboot commands are forbidden for Desktop Node product actions.'
                detail = $commandText
            }
        }
    }

    [ordered]@{
        ok = $true
        error = $null
    }
}

function Get-PcvFileSha256 {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "PCV_PRODUCT_FILE_NOT_FOUND|The file was not found.|Path: '$Path'."
    }

    $resolved = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
    $stream = [System.IO.File]::OpenRead($resolved)
    try {
        $sha = [System.Security.Cryptography.SHA256]::Create()
        try {
            [System.BitConverter]::ToString($sha.ComputeHash($stream)).
                Replace('-', '').
                ToLowerInvariant()
        }
        finally {
            $sha.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Resolve-PcvDesktopNodeUpdateSourceUri {
    param([Parameter(Mandatory)][string]$SourceUri)

    $uri = $null
    try {
        $uri = [System.Uri]$SourceUri
    }
    catch {
        $uri = $null
    }

    if ($null -eq $uri -or -not $uri.IsAbsoluteUri -or
        ($uri.IsAbsoluteUri -and $uri.Scheme.Length -eq 1 -and (Test-Path -LiteralPath $SourceUri -PathType Leaf))) {
        if (-not (Test-Path -LiteralPath $SourceUri -PathType Leaf)) {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_UPDATE_SOURCE_URI_INVALID' `
                    -Message 'Desktop Node update package source URI is invalid.' `
                    -Detail $SourceUri)
        }

        $resolvedPath = (Resolve-Path -LiteralPath $SourceUri -ErrorAction Stop).Path
        $uri = [System.Uri]$resolvedPath
    }

    if ($uri.Scheme -notin @('file', 'https')) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_SOURCE_URI_UNTRUSTED' `
                -Message 'Desktop Node update package source must use file or HTTPS.' `
                -Detail $SourceUri)
    }

    [ordered]@{
        ok = $true
        uri = $uri
        scheme = $uri.Scheme
    }
}

function Resolve-PcvDesktopNodeUpdateCatalogUri {
    param([Parameter(Mandatory)][string]$CatalogUri)

    $uri = $null
    try {
        $uri = [System.Uri]$CatalogUri
    }
    catch {
        $uri = $null
    }

    if ($null -eq $uri -or -not $uri.IsAbsoluteUri -or
        ($uri.IsAbsoluteUri -and $uri.Scheme.Length -eq 1 -and (Test-Path -LiteralPath $CatalogUri -PathType Leaf))) {
        if (-not (Test-Path -LiteralPath $CatalogUri -PathType Leaf)) {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_UPDATE_CATALOG_URI_INVALID' `
                    -Message 'Desktop Node update catalog URI is invalid.' `
                    -Detail $CatalogUri)
        }

        $resolvedPath = (Resolve-Path -LiteralPath $CatalogUri -ErrorAction Stop).Path
        $uri = [System.Uri]$resolvedPath
    }

    if ($uri.Scheme -notin @('file', 'https')) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_URI_UNTRUSTED' `
                -Message 'Desktop Node update catalog must use file or HTTPS.' `
                -Detail $CatalogUri)
    }

    [ordered]@{
        ok = $true
        uri = $uri
        scheme = $uri.Scheme
    }
}

function Resolve-PcvDesktopNodeUpdateCatalog {
    param(
        [Parameter(Mandatory)][string]$CatalogUri,
        [Parameter(Mandatory)][string]$Channel
    )

    if ([string]::IsNullOrWhiteSpace($Channel)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_CHANNEL_REQUIRED' `
                -Message 'Desktop Node update catalog channel is required.' `
                -Detail 'Pass -UpdateChannel with the catalog channel to select.')
    }

    $catalogResolution = Resolve-PcvDesktopNodeUpdateCatalogUri -CatalogUri $CatalogUri
    if ($catalogResolution.Contains('ok') -and -not [bool]$catalogResolution.ok) {
        return $catalogResolution
    }

    try {
        if ([string]$catalogResolution.scheme -eq 'file') {
            $catalogText = Get-Content -LiteralPath $catalogResolution.uri.LocalPath -Raw -ErrorAction Stop
        }
        else {
            $requestArgs = @{
                Uri = [string]$catalogResolution.uri.AbsoluteUri
                TimeoutSec = 15
                ErrorAction = 'Stop'
            }
            if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
                $requestArgs.UseBasicParsing = $true
            }
            $catalogText = (Invoke-WebRequest @requestArgs).Content
        }

        $catalog = $catalogText | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_READ_FAILED' `
                -Message 'Desktop Node update catalog could not be read.' `
                -Detail $_.Exception.Message)
    }

    $schemaVersion = Get-PcvProductObjectPropertyValue -InputObject $catalog -Name 'schema_version'
    $schemaVersionNumber = 0
    if ($null -eq $schemaVersion -or
        -not [int]::TryParse([string]$schemaVersion, [ref]$schemaVersionNumber) -or
        $schemaVersionNumber -ne 1) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_SCHEMA_UNSUPPORTED' `
                -Message 'Desktop Node update catalog schema is unsupported.' `
                -Detail "schema_version=$schemaVersion")
    }

    $product = Get-PcvProductObjectPropertyValue -InputObject $catalog -Name 'product'
    if (-not [string]::Equals([string]$product, 'PureCVisor Desktop Node', [System.StringComparison]::Ordinal)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_PRODUCT_MISMATCH' `
                -Message 'Desktop Node update catalog product does not match.' `
                -Detail ([string]$product))
    }

    $channels = @(Get-PcvProductObjectPropertyValue -InputObject $catalog -Name 'channels')
    if ($channels.Count -eq 0) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_EMPTY' `
                -Message 'Desktop Node update catalog does not contain channels.' `
                -Detail $CatalogUri)
    }

    $selected = @($channels | Where-Object {
            [string]::Equals([string](Get-PcvProductObjectPropertyValue -InputObject $_ -Name 'name'), $Channel, [System.StringComparison]::OrdinalIgnoreCase) -or
            [string]::Equals([string](Get-PcvProductObjectPropertyValue -InputObject $_ -Name 'channel'), $Channel, [System.StringComparison]::OrdinalIgnoreCase)
        } | Select-Object -First 1)
    if ($selected.Count -eq 0) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_CHANNEL_NOT_FOUND' `
                -Message 'Desktop Node update catalog channel was not found.' `
                -Detail $Channel)
    }

    $entry = $selected[0]
    $version = [string](Get-PcvProductObjectPropertyValue -InputObject $entry -Name 'version')
    if ([string]::IsNullOrWhiteSpace($version)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_VERSION_MISSING' `
                -Message 'Desktop Node update catalog channel is missing version.' `
                -Detail $Channel)
    }

    $sourceUri = [string](Get-PcvProductObjectPropertyValue -InputObject $entry -Name 'source_uri')
    if ([string]::IsNullOrWhiteSpace($sourceUri)) {
        $sourceUri = [string](Get-PcvProductObjectPropertyValue -InputObject $entry -Name 'package_uri')
    }
    if ([string]::IsNullOrWhiteSpace($sourceUri)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_PACKAGE_URI_MISSING' `
                -Message 'Desktop Node update catalog channel is missing package URI.' `
                -Detail $Channel)
    }

    $expectedSha256 = [string](Get-PcvProductObjectPropertyValue -InputObject $entry -Name 'expected_sha256')
    if ([string]::IsNullOrWhiteSpace($expectedSha256)) {
        $expectedSha256 = [string](Get-PcvProductObjectPropertyValue -InputObject $entry -Name 'sha256')
    }
    $expectedSha256 = $expectedSha256.Trim().ToLowerInvariant()
    if ($expectedSha256 -notmatch '^[0-9a-f]{64}$') {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_CATALOG_HASH_INVALID' `
                -Message 'Desktop Node update catalog channel SHA-256 is invalid.' `
                -Detail $Channel)
    }

    $publication = Get-PcvProductObjectPropertyValue -InputObject $catalog -Name 'publication'
    $publicTrustedSigning = $(if ($null -ne $publication) { Get-PcvProductObjectPropertyValue -InputObject $publication -Name 'public_trusted_signing' } else { $null })
    $externalStablePublication = $(if ($null -ne $publication) { Get-PcvProductObjectPropertyValue -InputObject $publication -Name 'external_stable_publication' } else { $null })
    if ([string]::IsNullOrWhiteSpace([string]$publicTrustedSigning)) {
        $publicTrustedSigning = 'not-claimed'
    }
    if ([string]::IsNullOrWhiteSpace([string]$externalStablePublication)) {
        $externalStablePublication = 'not-claimed'
    }

    [ordered]@{
        ok = $true
        catalog_uri = $CatalogUri
        scheme = [string]$catalogResolution.scheme
        channel = $Channel
        version = $version
        release_channel = [string](Get-PcvProductObjectPropertyValue -InputObject $entry -Name 'release_channel')
        signing_mode = [string](Get-PcvProductObjectPropertyValue -InputObject $entry -Name 'signing_mode')
        source_uri = $sourceUri
        expected_sha256 = $expectedSha256
        publication = [ordered]@{
            public_trusted_signing = [string]$publicTrustedSigning
            external_stable_publication = [string]$externalStablePublication
        }
        mutates_host = $false
    }
}

function Resolve-PcvDesktopNodeUpdatePackage {
    param(
        [Parameter(Mandatory)][string]$SourceUri,
        [Parameter(Mandatory)][string]$ExpectedSha256,
        [Parameter(Mandatory)][string]$DownloadRoot,
        [string]$ProductRoot = ''
    )

    if ([string]::IsNullOrWhiteSpace($ExpectedSha256)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_HASH_REQUIRED' `
                -Message 'Desktop Node update package expected SHA-256 is required.' `
                -Detail 'Pass -ExpectedSha256 for downloaded or file URI update packages.')
    }

    $expected = $ExpectedSha256.Trim().ToLowerInvariant()
    if ($expected -notmatch '^[0-9a-f]{64}$') {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_HASH_INVALID' `
                -Message 'Desktop Node update package expected SHA-256 is invalid.' `
                -Detail $ExpectedSha256)
    }

    $sourceResolution = Resolve-PcvDesktopNodeUpdateSourceUri -SourceUri $SourceUri
    if ($sourceResolution.Contains('ok') -and -not [bool]$sourceResolution.ok) {
        return $sourceResolution
    }

    $downloadRootFull = [System.IO.Path]::GetFullPath($DownloadRoot).TrimEnd('\', '/')
    if (-not [string]::IsNullOrWhiteSpace($ProductRoot)) {
        $productRootFull = [System.IO.Path]::GetFullPath($ProductRoot).TrimEnd('\', '/')
        $productRootPrefix = $productRootFull + [System.IO.Path]::DirectorySeparatorChar
        if ([string]::Equals($downloadRootFull, $productRootFull, [System.StringComparison]::OrdinalIgnoreCase) -or
            $downloadRootFull.StartsWith($productRootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_UPDATE_DOWNLOAD_ROOT_ACTIVE_ROOT' `
                    -Message 'Desktop Node update download root must be outside the active product root.' `
                    -Detail $DownloadRoot)
        }
    }

    $packageRoot = Join-PcvProductPath -Root $downloadRootFull -ChildPath @('packages')
    $extractRoot = Join-PcvProductPath -Root $downloadRootFull -ChildPath @('payloads', $expected)
    New-Item -ItemType Directory -Path $packageRoot -Force -ErrorAction Stop | Out-Null
    New-Item -ItemType Directory -Path (Split-Path -Parent $extractRoot) -Force -ErrorAction Stop | Out-Null

    $packagePath = Join-PcvProductPath -Root $packageRoot -ChildPath @("update-$expected.zip")
    try {
        if ([string]$sourceResolution.scheme -eq 'file') {
            $sourcePath = $sourceResolution.uri.LocalPath
            if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
                return (New-PcvProductError `
                        -Code 'PCV_PRODUCT_UPDATE_SOURCE_FILE_MISSING' `
                        -Message 'Desktop Node update package source file is missing.' `
                        -Detail $sourcePath)
            }

            $sourceFull = (Resolve-Path -LiteralPath $sourcePath -ErrorAction Stop).Path
            if (-not [string]::Equals($sourceFull, $packagePath, [System.StringComparison]::OrdinalIgnoreCase)) {
                Copy-Item -LiteralPath $sourceFull -Destination $packagePath -Force -ErrorAction Stop
            }
        }
        else {
            $requestArgs = @{
                Uri = [string]$sourceResolution.uri.AbsoluteUri
                OutFile = $packagePath
                ErrorAction = 'Stop'
            }
            if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
                $requestArgs.UseBasicParsing = $true
            }
            Invoke-WebRequest @requestArgs | Out-Null
        }
    }
    catch {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_DOWNLOAD_FAILED' `
                -Message 'Desktop Node update package download or staging failed.' `
                -Detail $_.Exception.Message)
    }

    $actual = Get-PcvFileSha256 -Path $packagePath
    if ($actual -ne $expected) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_HASH_MISMATCH' `
                -Message 'Desktop Node update package SHA-256 does not match the expected value.' `
                -Detail "expected=$expected; actual=$actual")
    }

    try {
        if (Test-Path -LiteralPath $extractRoot) {
            Remove-Item -LiteralPath $extractRoot -Recurse -Force -ErrorAction Stop
        }
        New-Item -ItemType Directory -Path $extractRoot -Force -ErrorAction Stop | Out-Null
        Expand-Archive -LiteralPath $packagePath -DestinationPath $extractRoot -Force -ErrorAction Stop
    }
    catch {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_PACKAGE_EXTRACT_FAILED' `
                -Message 'Desktop Node update package extraction failed.' `
                -Detail $_.Exception.Message)
    }

    [ordered]@{
        ok = $true
        source_uri = $SourceUri
        scheme = [string]$sourceResolution.scheme
        expected_sha256 = $expected
        actual_sha256 = $actual
        package_path = $packagePath
        download_root = $downloadRootFull
        source_root = $extractRoot
        extracted = $true
        mutates_host = $false
    }
}

function ConvertTo-PcvXmlEscapedText {
    param([AllowNull()][string]$Value)

    if ($null -eq $Value) {
        return ''
    }

    $Value.Replace('&', '&amp;').Replace('<', '&lt;').Replace('>', '&gt;')
}

function Quote-PcvWinSwArgument {
    param([Parameter(Mandatory)][string]$Value)

    if ($Value -match '^([A-Za-z]:\\|\\\\)') {
        return '"' + ($Value -replace '"', '\"') + '"'
    }

    if ($Value -match '^[A-Za-z0-9._:/\\-]+$') {
        return $Value
    }

    '"' + ($Value -replace '"', '\"') + '"'
}

function New-PcvDesktopNodeWinSwArguments {
    param(
        [Parameter(Mandatory)]$Paths,
        [Parameter(Mandatory)][string]$Prefix,
        [Parameter(Mandatory)][int]$WorkerCount,
        [Parameter(Mandatory)][int]$TimeoutSec
    )

    $null = $Paths, $Prefix, $WorkerCount, $TimeoutSec
    throw "PCV_WINSW_POWERSHELL_LOCAL_API_RETIRED|WinSW PowerShell Local API arguments are retired.|Use New-PcvDesktopNodeServicePlan, which points the service at DesktopNode.Host.exe."
}

function New-PcvDesktopNodeWinSwXml {
    param(
        [Parameter(Mandatory)][string]$ServiceName,
        [Parameter(Mandatory)][string]$DisplayName,
        [Parameter(Mandatory)][string]$Description,
        [Parameter(Mandatory)][string]$PwshPath,
        [Parameter(Mandatory)][string]$Arguments,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$LogPath
    )

    $null = $ServiceName, $DisplayName, $Description, $PwshPath, $Arguments, $WorkingDirectory, $LogPath
    throw "PCV_WINSW_POWERSHELL_LOCAL_API_RETIRED|WinSW PowerShell Local API XML generation is retired.|Use New-PcvDesktopNodeServicePlan, which points the service at DesktopNode.Host.exe."
}

function Resolve-PcvDesktopNodeWinSwArtifact {
    param([AllowNull()][string]$WinSwPath)

    if ([string]::IsNullOrWhiteSpace($WinSwPath)) {
        return [ordered]@{
            source_path = $null
            source_sha256 = $null
            missing_source_error = [ordered]@{
                code = 'PCV_PRODUCT_WINSW_PATH_REQUIRED'
                message = 'WinSW executable path is required for mutating product install.'
                detail = 'Pass -WinSwPath with a local WinSW executable prepared by the operator or packaging step.'
            }
        }
    }

    $resolved = (Resolve-Path -LiteralPath $WinSwPath -ErrorAction Stop).Path
    [ordered]@{
        source_path = $resolved
        source_sha256 = Get-PcvFileSha256 -Path $resolved
        missing_source_error = $null
    }
}

function Resolve-PcvDesktopNodeBatchEvidenceRoot {
    param(
        [Parameter(Mandatory)][string]$SourceRoot,
        [AllowNull()][string]$BatchEvidenceRoot
    )

    if ([string]::IsNullOrWhiteSpace($BatchEvidenceRoot)) {
        return $null
    }

    if ([System.IO.Path]::IsPathRooted($BatchEvidenceRoot)) {
        return [System.IO.Path]::GetFullPath($BatchEvidenceRoot)
    }

    $sourceRootPath = [System.IO.Path]::GetFullPath($SourceRoot)
    return [System.IO.Path]::GetFullPath((Join-Path $sourceRootPath $BatchEvidenceRoot))
}

function New-PcvDesktopNodeServicePlan {
    param(
        [Parameter(Mandatory)]$Paths,
        [Parameter(Mandatory)][string]$SourceRoot,
        [string]$ServiceName = (Get-PcvDesktopNodeProductDefaults).service_name,
        [string]$DisplayName = (Get-PcvDesktopNodeProductDefaults).display_name,
        [string]$Description = 'PureCVisor Desktop Node Local API service.',
        [string]$Prefix = (Get-PcvDesktopNodeProductDefaults).prefix,
        [string]$WebPrefix = (Get-PcvDesktopNodeProductDefaults).web_prefix,
        [string]$ServiceAccount = 'LocalSystem',
        [ValidateRange(1, 64)][int]$WorkerCount = 1,
        [ValidateRange(1, 600)][int]$TimeoutSec = 30,
        [ValidateRange(1, 600)][int]$RouteTimeoutSeconds = (Get-PcvDesktopNodeProductDefaults).route_timeout_seconds,
        [ValidateRange(1, 100000)][int]$RequestLimitPerMinute = (Get-PcvDesktopNodeProductDefaults).request_limit_per_minute,
        [ValidateRange(0, 10000)][int]$RequestBurstLimit = (Get-PcvDesktopNodeProductDefaults).request_burst_limit,
        [ValidateRange(1, 600)][int]$RetryAfterSeconds = (Get-PcvDesktopNodeProductDefaults).retry_after_seconds,
        [ValidateRange(1024, 67108864)][int]$MaxRequestBodyBytes = (Get-PcvDesktopNodeProductDefaults).max_request_body_bytes,
        [AllowNull()][string]$BatchEvidenceRoot,
        [AllowNull()][string]$WinSwPath
    )

    $resolvedBatchEvidenceRoot = Resolve-PcvDesktopNodeBatchEvidenceRoot `
        -SourceRoot $SourceRoot `
        -BatchEvidenceRoot $BatchEvidenceRoot

    $hostArguments = @(
        'listen',
        '--prefix',
        $Prefix,
        '--web-prefix',
        $WebPrefix,
        '--web-root',
        $Paths.web_root,
        '--job-store',
        $Paths.job_store,
        '--event-log',
        $Paths.event_log,
        '--event-log-provider-source',
        'PureCVisor Desktop Node',
        '--event-log-provider-log',
        'Application',
        '--event-log-writer',
        'windows-event-log',
        '--event-log-schema-version',
        '1',
        '--diagnostics-root',
        $Paths.diagnostics_root,
        '--api-token-protected-file',
        $Paths.token_protected_file,
        '--account-file',
        $Paths.account_file,
        '--jwt-signing-key-file',
        $Paths.jwt_signing_key_file,
        '--route-timeout-seconds',
        [string]$RouteTimeoutSeconds,
        '--request-limit-per-minute',
        [string]$RequestLimitPerMinute,
        '--request-burst-limit',
        [string]$RequestBurstLimit,
        '--retry-after-seconds',
        [string]$RetryAfterSeconds,
        '--max-request-body-bytes',
        [string]$MaxRequestBodyBytes
    )
    if (-not [string]::IsNullOrWhiteSpace($resolvedBatchEvidenceRoot)) {
        $hostArguments += @(
            '--batch-evidence-root',
            $resolvedBatchEvidenceRoot
        )
    }
    $binaryPath = (@($Paths.service_exe) + $hostArguments | ForEach-Object { Quote-PcvWinSwArgument -Value ([string]$_) }) -join ' '

    [ordered]@{
        mode = 'dotnet-windows-service'
        config = [ordered]@{
            service_name = $ServiceName
            display_name = $DisplayName
            description = $Description
            service_account = $ServiceAccount
            prefix = $Prefix
            api_prefix = $Prefix
            web_prefix = $WebPrefix
            exposure = 'loopback'
            auth_required = $true
            api_token_source = 'protected_file'
            batch_evidence_root = $resolvedBatchEvidenceRoot
            binary_path = $binaryPath
        }
        host = [ordered]@{
            executable_path = $Paths.service_exe
            arguments = $hostArguments
            default_owner = 'dotnet-windows-service-host'
        }
        hardening = [ordered]@{
            schema_version = 1
            route_timeout_seconds = $RouteTimeoutSeconds
            request_limit_per_minute = $RequestLimitPerMinute
            burst_limit = $RequestBurstLimit
            retry_after_seconds = $RetryAfterSeconds
            max_request_body_bytes = $MaxRequestBodyBytes
            route_timeout_policy = 'code-level-read-route-response-deadline'
            request_limit_policy = 'code-level-per-client-window'
            request_body_cap_policy = 'code-level-streaming-body-cap'
            error_contract = 'problem-details-json'
            server_config_mutation = 'service-binary-path-arguments'
        }
        commands = [ordered]@{
            install = @(
                [ordered]@{ file_name = 'sc.exe'; arguments = @('create', $ServiceName, 'binPath=', $binaryPath, 'DisplayName=', $DisplayName, 'start=', 'auto', 'obj=', $ServiceAccount) },
                [ordered]@{ file_name = 'sc.exe'; arguments = @('description', $ServiceName, $Description) },
                [ordered]@{ file_name = 'sc.exe'; arguments = @('failure', $ServiceName, 'reset=', '86400', 'actions=', 'restart/60000/restart/60000/""/60000') }
            )
            start = @([ordered]@{ file_name = 'sc.exe'; arguments = @('start', $ServiceName) })
            stop = @([ordered]@{ file_name = 'sc.exe'; arguments = @('stop', $ServiceName) })
            uninstall = @([ordered]@{ file_name = 'sc.exe'; arguments = @('delete', $ServiceName) })
            status = @([ordered]@{ file_name = 'sc.exe'; arguments = @('query', $ServiceName) })
        }
    }
}

function New-PcvDesktopNodeProductPlan {
    param(
        [ValidateSet(
            'Plan',
            'Install',
            'Update',
            'Rollback',
            'Uninstall',
            'Status',
            'CollectDiagnostics',
            'ConfigureInstalled',
            'RepairInstalled',
            'RemoveInstalled'
        )]
        [string]$Action = 'Plan',
        [string]$SourceRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
        [string]$ProductRoot = (Get-PcvDesktopNodeProductDefaults).product_root,
        [string]$DataRoot = (Get-PcvDesktopNodeProductDefaults).data_root,
        [string]$ServiceName = (Get-PcvDesktopNodeProductDefaults).service_name,
        [string]$DisplayName = (Get-PcvDesktopNodeProductDefaults).display_name,
        [string]$Prefix = (Get-PcvDesktopNodeProductDefaults).prefix,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$Version = '0.12.0',
        [ValidateRange(1, 64)][int]$WorkerCount = 1,
        [ValidateRange(1, 600)][int]$TimeoutSec = 30,
        [AllowNull()][string]$ApiToken,
        [AllowNull()][string]$WinSwPath,
        [AllowNull()][string]$SourceUri,
        [AllowNull()][string]$ExpectedSha256,
        [AllowNull()][string]$DownloadRoot,
        [AllowNull()][string]$UpdateCatalogUri,
        [AllowNull()][string]$UpdateChannel,
        [AllowNull()][string]$BatchEvidenceRoot,
        [switch]$RemoveData
    )

    if (-not [string]::IsNullOrWhiteSpace($ApiToken)) {
        throw 'PCV_PRODUCT_INLINE_TOKEN_FORBIDDEN|Inline API tokens are forbidden for the Desktop Node product plan.|Use the generated token file path instead.'
    }

    $paths = Resolve-PcvDesktopNodeProductPaths `
        -SourceRoot $SourceRoot `
        -ProductRoot $ProductRoot `
        -DataRoot $DataRoot
    $resolvedBatchEvidenceRoot = Resolve-PcvDesktopNodeBatchEvidenceRoot `
        -SourceRoot $SourceRoot `
        -BatchEvidenceRoot $BatchEvidenceRoot
    $assets = @(Get-PcvDesktopNodeProductAssets -SourceRoot $SourceRoot -ProductRoot $ProductRoot)
    $service = New-PcvDesktopNodeServicePlan `
        -Paths $paths `
        -SourceRoot $SourceRoot `
        -ServiceName $ServiceName `
        -DisplayName $DisplayName `
        -Prefix $Prefix `
        -ServiceAccount $ServiceAccount `
        -WorkerCount $WorkerCount `
        -TimeoutSec $TimeoutSec `
        -BatchEvidenceRoot $resolvedBatchEvidenceRoot `
        -WinSwPath $WinSwPath
    if ($Action -in @('ConfigureInstalled', 'RepairInstalled')) {
        $nativeAction = if ($Action -eq 'ConfigureInstalled') { 'configure-installed' } else { 'repair-installed' }
        $nativeArguments = @(
            'service-action',
            $nativeAction,
            '--product-root',
            $ProductRoot,
            '--data-root',
            $DataRoot,
            '--service-exe',
            $paths.service_exe
        )
        if (-not [string]::IsNullOrWhiteSpace($resolvedBatchEvidenceRoot)) {
            $nativeArguments += @(
                '--batch-evidence-root',
                $resolvedBatchEvidenceRoot
            )
        }

        $service.native_service_action = [ordered]@{
            action = $nativeAction
            executable_path = $paths.service_exe
            batch_evidence_root = $resolvedBatchEvidenceRoot
        }
        $service.commands.install = @(
            [ordered]@{
                file_name = $paths.service_exe
                arguments = $nativeArguments
            }
        )
    }
    $installedServiceExecutable = Join-PcvProductPath `
        -Root $ProductRoot `
        -ChildPath @((Get-PcvDesktopNodeProductDefaults).service_exe_name)
    $usesInstalledServiceExecutable = $false
    if ($service.mode -eq 'winsw' -and $Action -in @('ConfigureInstalled', 'RepairInstalled', 'RemoveInstalled')) {
        $usesInstalledServiceExecutable = $true
    }
    if (
        $service.mode -eq 'winsw' -and
        -not $usesInstalledServiceExecutable -and
        $Action -in @('Status', 'CollectDiagnostics') -and
        -not [string]::IsNullOrWhiteSpace($WinSwPath) -and
        [string]::Equals(
            (Resolve-Path -LiteralPath $WinSwPath -ErrorAction Stop).Path,
            $installedServiceExecutable,
            [System.StringComparison]::OrdinalIgnoreCase)
    ) {
        $usesInstalledServiceExecutable = $true
    }
    elseif (
        $service.mode -eq 'winsw' -and
        -not $usesInstalledServiceExecutable -and
        $Action -in @('Status', 'CollectDiagnostics') -and
        (Test-Path -LiteralPath $installedServiceExecutable -PathType Leaf) -and
        -not (Test-Path -LiteralPath $paths.winsw_exe -PathType Leaf)
    ) {
        $usesInstalledServiceExecutable = $true
    }

    $service.executable_path = $(if ($usesInstalledServiceExecutable) {
            $installedServiceExecutable
        }
        else {
            $service.host.executable_path
        })
    if ($usesInstalledServiceExecutable) {
        $service.winsw.staged_path = $installedServiceExecutable
        foreach ($commandName in @('install', 'start', 'stop', 'uninstall', 'status')) {
            foreach ($command in @($service.commands[$commandName])) {
                $command.file_name = $service.executable_path
            }
        }
    }

    $requiresElevation = $Action -in @(
        'Install',
        'Update',
        'Rollback',
        'Uninstall',
        'ConfigureInstalled',
        'RepairInstalled',
        'RemoveInstalled'
    )
    $deletePaths = @()
    $deletePathPatterns = @()
    if ($RemoveData) {
        $deletePaths = @(
            $paths.token_protected_file,
            $paths.token_file,
            $paths.account_file,
            $paths.jwt_signing_key_file,
            $paths.job_store,
            $paths.job_store_legacy_temp,
            $paths.job_store_pending_commit,
            $paths.event_log,
            $paths.install_log,
            $paths.diagnostics_root
        )
        $deletePathPatterns = @(
            $paths.job_store_temp_pattern,
            $paths.job_store_pending_commit_temp_pattern
        )

        if ($Action -eq 'Uninstall') {
            $deletePaths = @($ProductRoot) + $deletePaths
        }
    }
    $defaultDownloadRoot = Join-PcvProductPath -Root $DataRoot -ChildPath @('updates')
    if ([string]::IsNullOrWhiteSpace($DownloadRoot)) {
        $DownloadRoot = $defaultDownloadRoot
    }
    $updateSourceEnabled = -not [string]::IsNullOrWhiteSpace($SourceUri)
    $updateCatalogEnabled = -not [string]::IsNullOrWhiteSpace($UpdateCatalogUri)
    if ($updateSourceEnabled -and $updateCatalogEnabled) {
        throw 'PCV_PRODUCT_UPDATE_SOURCE_CONFLICT|Specify either direct update source or update catalog, not both.|Use -SourceUri/-ExpectedSha256 or -UpdateCatalogUri/-UpdateChannel for one update source resolver.'
    }
    if ($updateCatalogEnabled -and [string]::IsNullOrWhiteSpace($UpdateChannel)) {
        throw 'PCV_PRODUCT_UPDATE_CATALOG_CHANNEL_REQUIRED|Desktop Node update catalog channel is required.|Pass -UpdateChannel with -UpdateCatalogUri.'
    }

    [ordered]@{
        schema_version = 1
        action = $Action
        version = $Version
        source_root = $SourceRoot
        requires_elevation = $requiresElevation
        product_root = $ProductRoot
        data_root = $DataRoot
        release = [ordered]@{
            product_root = $ProductRoot
        }
        paths = $paths
        assets = $assets
        auth = [ordered]@{
            api_token_source = 'protected_file'
            api_token_storage = 'dpapi-local-machine'
            api_token_protected_file = $paths.token_protected_file
            legacy_api_token_file = $paths.token_file
            account_auth_source = 'local-json-and-jwt-signing-key'
            account_file = $paths.account_file
            jwt_signing_key_file = $paths.jwt_signing_key_file
            account_auth_default_state = 'not-configured-until-accounts-exist'
            rbac_roles = @('viewer', 'operator', 'admin')
        }
        token_file = $paths.token_file
        job_store = $paths.job_store
        job_store_legacy_temp = $paths.job_store_legacy_temp
        job_store_pending_commit = $paths.job_store_pending_commit
        job_store_temp_pattern = $paths.job_store_temp_pattern
        job_store_pending_commit_temp_pattern = $paths.job_store_pending_commit_temp_pattern
        event_log = $paths.event_log
        install_log = $paths.install_log
        diagnostics_root = $paths.diagnostics_root
        data_acl = Get-PcvDesktopNodeDataAclPolicy -Paths $paths
        network = Get-PcvDesktopNodeLanSecurityPolicy
        no_auto_reboot = [ordered]@{
            enabled = $true
            enforcement = 'product-process-command-guard'
            forbidden_commands = @(
                'Restart-Computer',
                'shutdown.exe',
                'msiexec.exe /forcerestart',
                'msiexec.exe REBOOT=Force'
            )
        }
        update = Get-PcvDesktopNodeUpdatePolicy -Paths $paths
        service = $service
        diagnostics = Get-PcvDesktopNodeDiagnosticsPolicy -Paths $paths
        update_source = [ordered]@{
            enabled = $updateSourceEnabled
            source_uri = $(if ($updateSourceEnabled) { $SourceUri } else { $null })
            expected_sha256 = $(if (-not [string]::IsNullOrWhiteSpace($ExpectedSha256)) { $ExpectedSha256.Trim().ToLowerInvariant() } else { $null })
            download_root = $DownloadRoot
            allowed_schemes = @('file', 'https')
            expected_sha256_required = $true
            resolution_stage = 'before-service-stop'
            mutates_host = $false
        }
        update_catalog = [ordered]@{
            enabled = $updateCatalogEnabled
            catalog_uri = $(if ($updateCatalogEnabled) { $UpdateCatalogUri } else { $null })
            channel = $(if (-not [string]::IsNullOrWhiteSpace($UpdateChannel)) { $UpdateChannel } else { $null })
            download_root = $DownloadRoot
            allowed_schemes = @('file', 'https')
            schema_version = 1
            resolution_stage = 'before-service-stop'
            mutates_host = $false
            publication = [ordered]@{
                public_trusted_signing = 'not-claimed'
                external_stable_publication = 'not-claimed'
            }
        }
        remove_data = [bool]$RemoveData
        delete_paths = $deletePaths
        delete_path_patterns = $deletePathPatterns
    }
}

function Get-PcvDesktopNodePlanDiagnosticsPolicy {
    param([Parameter(Mandatory)]$Plan)

    if ($Plan.Contains('diagnostics') -and $null -ne $Plan.diagnostics) {
        return $Plan.diagnostics
    }

    Get-PcvDesktopNodeDiagnosticsPolicy -Paths $Plan.paths
}

function Get-PcvDesktopNodePlanNetworkPolicy {
    param([Parameter(Mandatory)]$Plan)

    if ($Plan.Contains('network') -and $null -ne $Plan.network) {
        return $Plan.network
    }

    Get-PcvDesktopNodeLanSecurityPolicy
}

function New-PcvDesktopNodeEventLogRegistrationPlan {
    param([Parameter(Mandatory)]$Plan)

    $policy = Get-PcvDesktopNodePlanDiagnosticsPolicy -Plan $Plan
    $source = [string]$policy.windows_event_log.source
    $logName = [string]$policy.windows_event_log.log_name

    [ordered]@{
        enabled_by_default = [bool]$policy.windows_event_log.enabled_by_default
        registration_owner = [string]$policy.windows_event_log.registration_owner
        log_name = $logName
        source = $source
        native_owner = 'dotnet-host-service-action'
        commands = [ordered]@{
            default_transition = [ordered]@{
                file_name = [string]$Plan.paths.service_exe
                arguments = @(
                    'service-action',
                    'eventlog-default-transition'
                )
            }
            register = [ordered]@{
                file_name = [string]$Plan.paths.service_exe
                arguments = @(
                    'service-action',
                    'eventlog-register'
                )
            }
            unregister = [ordered]@{
                file_name = [string]$Plan.paths.service_exe
                arguments = @(
                    'service-action',
                    'eventlog-remove'
                )
            }
        }
    }
}

function Rotate-PcvDesktopNodeLogFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Name,
        [ValidateRange(1, [int]::MaxValue)][int]$MaxFileBytes,
        [ValidateRange(1, 1000)][int]$RetainedFiles
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [ordered]@{
            name = $Name
            path = $Path
            rotated = $false
            reason = 'missing'
        }
    }

    $item = Get-Item -LiteralPath $Path -ErrorAction Stop
    if ([int64]$item.Length -le [int64]$MaxFileBytes) {
        return [ordered]@{
            name = $Name
            path = $Path
            rotated = $false
            reason = 'below-threshold'
            length = [int64]$item.Length
            max_file_bytes = $MaxFileBytes
        }
    }

    $oldest = "$Path.$RetainedFiles"
    if (Test-Path -LiteralPath $oldest) {
        Remove-Item -LiteralPath $oldest -Force -ErrorAction Stop
    }

    for ($index = $RetainedFiles; $index -ge 2; $index--) {
        $source = "$Path.$($index - 1)"
        $target = "$Path.$index"
        if (Test-Path -LiteralPath $source) {
            if (Test-Path -LiteralPath $target) {
                Remove-Item -LiteralPath $target -Force -ErrorAction Stop
            }
            Move-Item -LiteralPath $source -Destination $target -Force -ErrorAction Stop
        }
    }

    Move-Item -LiteralPath $Path -Destination "$Path.1" -Force -ErrorAction Stop

    [ordered]@{
        name = $Name
        path = $Path
        rotated = $true
        rotated_to = "$Path.1"
        length = [int64]$item.Length
        max_file_bytes = $MaxFileBytes
        retained_files = $RetainedFiles
    }
}

function Invoke-PcvDesktopNodeLogRotation {
    param(
        [Parameter(Mandatory)]$Plan,
        [int]$MaxFileBytes,
        [int]$RetainedFiles,
        [int]$ServiceMaxFileBytes,
        [int]$ServiceRetainedFiles
    )

    $policy = Get-PcvDesktopNodePlanDiagnosticsPolicy -Plan $Plan
    if ($MaxFileBytes -le 0) {
        $MaxFileBytes = [int]$policy.event_log.max_file_bytes
    }
    if ($RetainedFiles -le 0) {
        $RetainedFiles = [int]$policy.event_log.retained_files
    }
    if ($ServiceMaxFileBytes -le 0) {
        $ServiceMaxFileBytes = [int]$policy.service_logs.max_file_bytes
    }
    if ($ServiceRetainedFiles -le 0) {
        $ServiceRetainedFiles = [int]$policy.service_logs.retained_files
    }

    $results = @()
    $results += Rotate-PcvDesktopNodeLogFile `
        -Path $Plan.paths.event_log `
        -Name 'event_log' `
        -MaxFileBytes $MaxFileBytes `
        -RetainedFiles $RetainedFiles
    $results += Rotate-PcvDesktopNodeLogFile `
        -Path $Plan.paths.install_log `
        -Name 'install_log' `
        -MaxFileBytes $MaxFileBytes `
        -RetainedFiles $RetainedFiles

    if (Test-Path -LiteralPath $Plan.paths.service_logs_root -PathType Container) {
        Get-ChildItem -LiteralPath $Plan.paths.service_logs_root -File -ErrorAction Stop |
            Where-Object { $_.Name -match '\.(log|out|err)$' } |
            ForEach-Object {
                $results += Rotate-PcvDesktopNodeLogFile `
                    -Path $_.FullName `
                    -Name 'service_log' `
                    -MaxFileBytes $ServiceMaxFileBytes `
                    -RetainedFiles $ServiceRetainedFiles
            }
    }

    [ordered]@{
        ok = $true
        policy = $policy
        rotated = @($results | Where-Object { [bool]$_.rotated })
        checked = $results
    }
}

function Test-PcvWinSwMissingServiceResult {
    param([Parameter(Mandatory)]$Result)

    if ($Result.ok) {
        return $false
    }

    $text = (($Result.results | ForEach-Object { "$($_.stdout)`n$($_.stderr)" }) -join "`n")
    $text -match '(?i)(nonexistentservice|service.*does not exist|no such service|specified service does not exist|\b1060\b)'
}

function Test-PcvWinSwStopPendingResult {
    param([Parameter(Mandatory)]$Result)

    if ([bool]$Result.ok) {
        return $false
    }

    $text = (($Result.results | ForEach-Object { "$($_.stdout)`n$($_.stderr)" }) -join "`n")
    $text -match "(?i)(cannot\s+stop\s+'?PureCVisorDesktopNode'?\s+service\s+on\s+computer|stop[_ ]pending|service.*pending)"
}

function Test-PcvWinSwStoppedStatusResult {
    param([Parameter(Mandatory)]$Result)

    if (-not [bool]$Result.ok) {
        return (Test-PcvWinSwMissingServiceResult -Result $Result)
    }

    $text = (($Result.results | ForEach-Object { "$($_.stdout)`n$($_.stderr)" }) -join "`n").Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $true
    }

    $text -notmatch '(?i)(\b(started|running|startpending|start pending|start_pending|stoppending|stop pending|stop_pending)\b|active\s*\(\s*(running|starting|stopping|continuing|pausing|paused)\s*\))'
}

function Wait-PcvWinSwServiceStopped {
    param(
        [Parameter(Mandatory)]$Plan,
        [Parameter(Mandatory)][scriptblock]$InvokeProcess,
        [int]$MaxAttempts = 45,
        [int]$DelayMilliseconds = 500
    )

    $attempts = @()
    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $statusResults = Invoke-PcvProductProcessCommand `
            -Commands $Plan.service.commands.status `
            -InvokeProcess $InvokeProcess
        $attempts += [ordered]@{
            attempt = $attempt
            result = $statusResults
        }

        if (Test-PcvWinSwStoppedStatusResult -Result $statusResults) {
            return [ordered]@{
                ok = $true
                stopped = $true
                attempt_count = $attempt
                attempts = $attempts
            }
        }

        if ($attempt -lt $MaxAttempts) {
            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }

    [ordered]@{
        ok = $false
        stopped = $false
        attempt_count = $attempts.Count
        attempts = $attempts
        error = [ordered]@{
            code = 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT'
            message = 'Desktop Node service did not stop before the timeout.'
            detail = 'Service status still reported a running or pending service after stop.'
        }
    }
}

function ConvertTo-PcvProductJson {
    param([Parameter(Mandatory, ValueFromPipeline)]$InputObject)

    process {
        $InputObject | ConvertTo-Json -Depth 32
    }
}

function New-PcvProductError {
    param(
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [string]$Detail = ''
    )

    [ordered]@{
        ok = $false
        error = [ordered]@{
            code = $Code
            message = $Message
            detail = $Detail
        }
    }
}

function Set-PcvDesktopNodeDictionaryValue {
    param(
        [Parameter(Mandatory)][System.Collections.IDictionary]$Dictionary,
        [Parameter(Mandatory)][string]$Name,
        [AllowNull()]$Value
    )

    if ($Dictionary.Contains($Name)) {
        $Dictionary[$Name] = $Value
        return
    }

    $Dictionary.Add($Name, $Value)
}

function Write-PcvDesktopNodeUpdateTransactionJournal {
    param(
        [Parameter(Mandatory)]$Plan,
        [Parameter(Mandatory)][System.Collections.IDictionary]$Journal
    )

    $journalPath = [string]$Plan.paths.update_transaction_journal
    if ([string]::IsNullOrWhiteSpace($journalPath)) {
        throw 'PCV_PRODUCT_UPDATE_JOURNAL_PATH_MISSING|Desktop Node update transaction journal path is missing.|Plan paths did not include update_transaction_journal.'
    }

    $journalDirectory = Split-Path -Parent $journalPath
    if (-not [string]::IsNullOrWhiteSpace($journalDirectory)) {
        New-Item -ItemType Directory -Path $journalDirectory -Force -ErrorAction Stop | Out-Null
    }

    $Journal | ConvertTo-PcvProductJson |
        Set-Content -LiteralPath $journalPath -Encoding UTF8 -ErrorAction Stop

    [ordered]@{
        path = $journalPath
        transaction_id = [string]$Journal.transaction_id
        status = [string]$Journal.status
        stage = [string]$Journal.stage
        written = $true
    }
}

function New-PcvDesktopNodeUpdateTransactionJournal {
    param(
        [Parameter(Mandatory)]$Plan,
        [Parameter(Mandatory)][string]$FromVersion,
        [Parameter(Mandatory)][string]$ToVersion,
        [Parameter(Mandatory)][string]$SourceRoot,
        [AllowNull()]$UpdateSource,
        [AllowNull()]$UpdateCatalog,
        [AllowNull()]$PayloadValidation
    )

    $now = [DateTime]::UtcNow.ToString('o')
    [ordered]@{
        schema_version = 1
        transaction_id = [guid]::NewGuid().ToString('N')
        action = 'Update'
        status = 'started'
        stage = 'payload-validated'
        started_at = $now
        last_updated_at = $now
        completed_at = $null
        from_version = $FromVersion
        to_version = $ToVersion
        product_root = [string]$Plan.product_root
        previous_product_root = [string]$Plan.paths.previous_product_root
        data_root = [string]$Plan.data_root
        source_root = $SourceRoot
        journal_path = [string]$Plan.paths.update_transaction_journal
        update_source = $UpdateSource
        update_catalog = $UpdateCatalog
        payload_validation = $PayloadValidation
        service_mutation_started = $false
        host_mutation_performed = $false
        rollback_attempted = $false
        rollback_result = $null
        error = $null
        executed_steps = @()
        stages = @(
            [ordered]@{
                timestamp_utc = $now
                stage = 'payload-validated'
                status = 'started'
            }
        )
    }
}

function Update-PcvDesktopNodeUpdateTransactionJournal {
    param(
        [Parameter(Mandatory)]$Plan,
        [AllowNull()][System.Collections.IDictionary]$Journal,
        [Parameter(Mandatory)][string]$Stage,
        [Parameter(Mandatory)][string]$Status,
        [AllowNull()]$ServiceMutationStarted,
        [AllowNull()]$HostMutationPerformed,
        [AllowNull()]$RollbackAttempted,
        [AllowNull()]$RollbackResult,
        [AllowNull()]$Error,
        [AllowNull()][string[]]$ExecutedSteps,
        [switch]$Completed
    )

    if ($null -eq $Journal) {
        return $null
    }

    $now = [DateTime]::UtcNow.ToString('o')
    Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'stage' -Value $Stage
    Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'status' -Value $Status
    Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'last_updated_at' -Value $now
    if ($Completed) {
        Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'completed_at' -Value $now
    }
    if ($null -ne $ServiceMutationStarted) {
        Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'service_mutation_started' -Value ([bool]$ServiceMutationStarted)
    }
    if ($null -ne $HostMutationPerformed) {
        Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'host_mutation_performed' -Value ([bool]$HostMutationPerformed)
    }
    if ($null -ne $RollbackAttempted) {
        Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'rollback_attempted' -Value ([bool]$RollbackAttempted)
    }
    if ($null -ne $RollbackResult) {
        Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'rollback_result' -Value $RollbackResult
    }
    if ($null -ne $Error) {
        Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'error' -Value $Error
    }
    if ($null -ne $ExecutedSteps) {
        Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'executed_steps' -Value @($ExecutedSteps)
    }

    $transition = [ordered]@{
        timestamp_utc = $now
        stage = $Stage
        status = $Status
    }
    if ($null -ne $Error -and $Error.Contains('code')) {
        $transition.error_code = [string]$Error.code
    }
    Set-PcvDesktopNodeDictionaryValue -Dictionary $Journal -Name 'stages' -Value (@($Journal.stages) + $transition)

    Write-PcvDesktopNodeUpdateTransactionJournal -Plan $Plan -Journal $Journal
}

function Write-PcvDesktopNodeInstallLogEvent {
    param(
        [Parameter(Mandatory)]$Plan,
        [Parameter(Mandatory)][string]$EventName,
        [AllowNull()]$Data
    )

    try {
        if ($null -eq $Plan -or -not $Plan.Contains('paths')) {
            return
        }

        $logPath = [string]$Plan.paths.install_log
        if ([string]::IsNullOrWhiteSpace($logPath)) {
            return
        }

        $logDirectory = Split-Path -Parent $logPath
        if (-not [string]::IsNullOrWhiteSpace($logDirectory)) {
            New-Item -ItemType Directory -Path $logDirectory -Force -ErrorAction Stop | Out-Null
        }

        $record = [ordered]@{
            timestamp_utc = [DateTime]::UtcNow.ToString('o')
            event = $EventName
            action = [string]$Plan.action
            version = [string]$Plan.version
            service_name = [string]$Plan.service.config.service_name
            data = $Data
        }

        $line = $record | ConvertTo-Json -Depth 12 -Compress
        Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8 -ErrorAction Stop
    }
    catch {
        Write-Verbose "Install log event write failed: $($_.Exception.Message)"
    }
}

function New-PcvDesktopNodeDryRunResult {
    param([Parameter(Mandatory)]$Plan)

    $result = [ordered]@{}
    foreach ($key in $Plan.Keys) {
        $result[$key] = $Plan[$key]
    }

    $result.ok = $true
    $result.dry_run = $true
    $result.execution_skipped = $true
    $result
}

function ConvertTo-PcvProductActionError {
    param([Parameter(Mandatory)][string]$Message)

    $parts = $Message -split '\|', 3
    if ($parts.Count -ge 2 -and $parts[0] -match '^PCV_') {
        return [ordered]@{
            code = $parts[0]
            message = $parts[1]
            detail = $(if ($parts.Count -ge 3) { $parts[2] } else { '' })
        }
    }

    [ordered]@{
        code = 'PCV_PRODUCT_ACTION_FAILED'
        message = 'Desktop Node product action failed.'
        detail = $Message
    }
}

function Throw-PcvProductCommandResult {
    param([Parameter(Mandatory)]$CommandResult)

    if ($null -ne $CommandResult.error) {
        throw "$($CommandResult.error.code)|$($CommandResult.error.message)|$($CommandResult.error.detail)"
    }

    throw 'PCV_PRODUCT_COMMAND_FAILED|Desktop Node product command failed.|Command group returned ok false.'
}

function Update-PcvProductCommandResultAllowed {
    param(
        [Parameter(Mandatory)]$CommandResult,
        [Parameter(Mandatory)][int[]]$AllowedExitCodes
    )

    if ([bool]$CommandResult.ok) {
        return $CommandResult
    }

    $failedResults = @($CommandResult.results | Where-Object { [int]$_.exit_code -ne 0 })
    $unexpectedResults = @($failedResults | Where-Object { [int]$_.exit_code -notin $AllowedExitCodes })
    if ($failedResults.Count -gt 0 -and $unexpectedResults.Count -eq 0) {
        $CommandResult.ok = $true
        $CommandResult.error = $null
    }

    $CommandResult
}

function Update-PcvWinSwStopCommandResultAllowed {
    param([Parameter(Mandatory)]$CommandResult)

    $result = Update-PcvProductCommandResultAllowed `
        -CommandResult $CommandResult `
        -AllowedExitCodes @(1060, 1062)
    if ([bool]$result.ok) {
        return $result
    }

    if (Test-PcvWinSwStopPendingResult -Result $result) {
        $result.ok = $true
        $result.error = $null
        $result['deferred_stop_verification'] = $true
    }

    $result
}

function Assert-PcvProductCommandResultAllowed {
    param(
        [Parameter(Mandatory)]$CommandResult,
        [Parameter(Mandatory)][int[]]$AllowedExitCodes
    )

    $result = Update-PcvProductCommandResultAllowed `
        -CommandResult $CommandResult `
        -AllowedExitCodes $AllowedExitCodes
    if (-not [bool]$result.ok) {
        Throw-PcvProductCommandResult -CommandResult $result
    }

    $result
}

function Test-PcvProductRemoveRetryable {
    param([Parameter(Mandatory)]$RemoveResult)

    if ($RemoveResult.Contains('ok') -and [bool]$RemoveResult.ok) {
        return $false
    }

    $parts = @()
    if ($RemoveResult.Contains('path')) {
        $parts += [string]$RemoveResult.path
    }
    if ($RemoveResult.Contains('error') -and $null -ne $RemoveResult.error) {
        $parts += [string]$RemoveResult.error.code
        $parts += [string]$RemoveResult.error.message
        $parts += [string]$RemoveResult.error.detail
    }

    $text = ($parts -join "`n")
    $text -match '(?i)(access\s+to\s+the\s+path.*denied|access\s+is\s+denied|being\s+used\s+by\s+another\s+process|cannot\s+access.*being\s+used|locked|permission\s+denied|\uC561\uC138\uC2A4\uAC00\s*\uAC70\uBD80\uB418\uC5C8\uC2B5\uB2C8\uB2E4)'
}

function Test-PcvProductAccessDeniedText {
    param([AllowNull()][string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $false
    }

    $Text -match '(?i)(access\s+to\s+the\s+path.*denied|access\s+is\s+denied|permission\s+denied|unauthorized|\uC561\uC138\uC2A4\uAC00\s*\uAC70\uBD80\uB418\uC5C8\uC2B5\uB2C8\uB2E4)'
}

function Grant-PcvProductAdministratorsFullControl {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][scriptblock]$InvokeProcess
    )

    $result = & $InvokeProcess -FileName 'icacls.exe' -Arguments @(
        $Path,
        '/grant:r',
        'BUILTIN\Administrators:F',
        'NT AUTHORITY\SYSTEM:F'
    )
    $ok = [int]$result.exit_code -eq 0

    $grantResult = [ordered]@{
        ok = $ok
        path = $Path
        result = $result
    }
    if (-not $ok) {
        $grantResult.error = [ordered]@{
            code = 'PCV_PRODUCT_ACL_REPAIR_FAILED'
            message = 'Desktop Node product path ACL repair failed.'
            detail = "icacls.exe returned exit code $($result.exit_code)."
        }
    }

    $grantResult
}

function Invoke-PcvProductRemovePathWithRetry {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][scriptblock]$RemovePath,
        [int]$MaxAttempts = 3,
        [int]$InitialDelayMilliseconds = 250
    )

    $attempts = @()
    $finalResult = $null
    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        try {
            $finalResult = & $RemovePath -Path $Path
        }
        catch {
            $finalResult = New-PcvProductError `
                -Code 'PCV_PRODUCT_REMOVE_FAILED' `
                -Message 'Desktop Node product path removal failed.' `
                -Detail $_.Exception.Message
            $finalResult.path = $Path
        }

        $attempts += [ordered]@{
            attempt = $attempt
            result = $finalResult
        }

        if (-not (Test-PcvProductRemoveRetryable -RemoveResult $finalResult)) {
            break
        }
        if ($attempt -ge $MaxAttempts) {
            break
        }

        Start-Sleep -Milliseconds ($InitialDelayMilliseconds * $attempt)
    }

    if ($attempts.Count -eq 1) {
        return $finalResult
    }

    $wrapped = [ordered]@{
        ok = $finalResult.Contains('ok') -and [bool]$finalResult.ok
        path = $Path
        attempt_count = $attempts.Count
        attempts = $attempts
        final_result = $finalResult
    }
    if ($finalResult.Contains('removed')) {
        $wrapped.removed = [bool]$finalResult.removed
    }
    if ($finalResult.Contains('error') -and $null -ne $finalResult.error) {
        $wrapped.error = $finalResult.error
    }

    $wrapped
}

function Test-PcvDiagnosticSensitiveKey {
    param([Parameter(Mandatory)][string]$Key)

    $Key -match '(?i)(^token$|api_token|access_token|authorization|password|secret|protected_token|token_sha256|jwt_signing_key)'
}

function Get-PcvDesktopNodeDiagnosticPathRedactions {
    param([Parameter(Mandatory)]$Plan)

    $entries = @(
        [ordered]@{ value = $Plan.paths.previous_product_root; replacement = '[PREVIOUS_PRODUCT_ROOT]' },
        [ordered]@{ value = $Plan.source_root; replacement = '[SOURCE_ROOT]' },
        [ordered]@{ value = $Plan.product_root; replacement = '[PRODUCT_ROOT]' },
        [ordered]@{ value = $Plan.data_root; replacement = '[DATA_ROOT]' }
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.value) } |
        Sort-Object { ([string]$_.value).Length } -Descending

    $redactions = [ordered]@{}
    foreach ($entry in $entries) {
        $value = [string]$entry.value
        $redactions[$value] = $entry.replacement
        $jsonEscapedValue = ($value | ConvertTo-Json -Compress).Trim('"')
        if ($jsonEscapedValue -ne $value) {
            $redactions[$jsonEscapedValue] = $entry.replacement
        }
    }

    $redactions
}

function ConvertTo-PcvDesktopNodeDiagnosticRedactedString {
    param(
        [AllowNull()][string]$Value,
        [System.Collections.IDictionary]$PathRedactions
    )

    if ($null -eq $Value) {
        return $null
    }

    $redacted = $Value -replace '(?i)Bearer\s+[A-Za-z0-9._~+\/=-]+', 'Bearer [REDACTED]'
    if ($null -ne $PathRedactions) {
        foreach ($path in $PathRedactions.Keys) {
            if (-not [string]::IsNullOrWhiteSpace([string]$path)) {
                $redacted = $redacted.Replace([string]$path, [string]$PathRedactions[$path])
            }
        }
    }

    $redacted
}

function ConvertTo-PcvDesktopNodeDiagnosticRedactedObject {
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $InputObject,
        [System.Collections.IDictionary]$PathRedactions
    )

    if ($null -eq $InputObject) {
        return $null
    }

    if ($InputObject -is [string]) {
        return (ConvertTo-PcvDesktopNodeDiagnosticRedactedString `
                -Value $InputObject `
                -PathRedactions $PathRedactions)
    }

    if ($InputObject -is [System.Collections.IDictionary]) {
        $out = [ordered]@{}
        foreach ($key in $InputObject.Keys) {
            if (Test-PcvDiagnosticSensitiveKey -Key ([string]$key)) {
                $out[$key] = '[REDACTED]'
            }
            else {
                $out[$key] = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
                    -InputObject $InputObject[$key] `
                    -PathRedactions $PathRedactions
            }
        }
        return $out
    }

    if ($InputObject -is [pscustomobject]) {
        $out = [ordered]@{}
        foreach ($property in $InputObject.PSObject.Properties) {
            if (Test-PcvDiagnosticSensitiveKey -Key $property.Name) {
                $out[$property.Name] = '[REDACTED]'
            }
            else {
                $out[$property.Name] = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
                    -InputObject $property.Value `
                    -PathRedactions $PathRedactions
            }
        }
        return $out
    }

    if ($InputObject -is [System.Collections.IEnumerable] -and $InputObject -isnot [string]) {
        $items = @()
        foreach ($item in $InputObject) {
            $items += ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
                -InputObject $item `
                -PathRedactions $PathRedactions
        }
        return $items
    }

    return $InputObject
}

function ConvertTo-PcvDesktopNodeDiagnosticRedactedText {
    param(
        [AllowNull()][string]$Text,
        [System.Collections.IDictionary]$PathRedactions
    )

    if ($null -eq $Text) {
        return ''
    }

    if (-not [string]::IsNullOrWhiteSpace($Text)) {
        try {
            $json = $Text | ConvertFrom-Json -ErrorAction Stop
            $redacted = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
                -InputObject $json `
                -PathRedactions $PathRedactions
            return ($redacted | ConvertTo-Json -Depth 32)
        }
        catch {
        }
    }

    $lines = $Text -split "`r?`n"
    $redactedLines = @()
    foreach ($line in $lines) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            $redactedLines += $line
            continue
        }

        try {
            $json = $line | ConvertFrom-Json -ErrorAction Stop
            $redacted = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
                -InputObject $json `
                -PathRedactions $PathRedactions
            $redactedLines += ($redacted | ConvertTo-Json -Depth 32 -Compress)
        }
        catch {
            $redactedLines += (ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
                    -InputObject $line `
                    -PathRedactions $PathRedactions)
        }
    }

    $redactedLines -join [Environment]::NewLine
}

function Write-PcvDesktopNodeDiagnosticJsonFile {
    param(
        [Parameter(Mandatory)]$InputObject,
        [Parameter(Mandatory)][string]$Path,
        [System.Collections.IDictionary]$PathRedactions
    )

    $redacted = ConvertTo-PcvDesktopNodeDiagnosticRedactedObject `
        -InputObject $InputObject `
        -PathRedactions $PathRedactions
    $redacted | ConvertTo-Json -Depth 32 | Set-Content -LiteralPath $Path -Encoding UTF8 -ErrorAction Stop
}

function Get-PcvDiagnosticObjectProperty {
    param(
        [AllowNull()]$InputObject,
        [Parameter(Mandatory)][string]$Name
    )

    if ($null -eq $InputObject) {
        return $null
    }

    if ($InputObject -is [System.Collections.IDictionary] -and $InputObject.Contains($Name)) {
        return $InputObject[$Name]
    }

    if ($InputObject.PSObject.Properties.Name -contains $Name) {
        return $InputObject.PSObject.Properties[$Name].Value
    }

    $null
}

function New-PcvDesktopNodeDiagnosticSelfAudit {
    param([AllowNull()]$RuntimePolicy)

    $runtimePolicyPresent = $null -ne $RuntimePolicy
    $runtimePolicyAvailable = [bool](Get-PcvDiagnosticObjectProperty -InputObject $RuntimePolicy -Name 'ok')
    $bodyText = [string](Get-PcvDiagnosticObjectProperty -InputObject $RuntimePolicy -Name 'body')
    $bodyParseable = $false
    $policyBody = $null
    $warnings = @()

    if (-not $runtimePolicyPresent) {
        $warnings += 'runtime-policy-result-missing'
    }
    elseif (-not $runtimePolicyAvailable) {
        $warnings += 'runtime-policy-unavailable'
    }

    if (-not [string]::IsNullOrWhiteSpace($bodyText)) {
        try {
            $policyBody = $bodyText | ConvertFrom-Json -ErrorAction Stop
            $bodyParseable = $true
        }
        catch {
            $warnings += 'runtime-policy-body-not-json'
        }
    }
    elseif ($runtimePolicyAvailable) {
        $warnings += 'runtime-policy-body-missing'
    }

    $policyData = Get-PcvDiagnosticObjectProperty -InputObject $policyBody -Name 'data'
    if ($null -eq $policyData) {
        $policyData = $policyBody
    }

    $jobRuntime = Get-PcvDiagnosticObjectProperty -InputObject $policyData -Name 'job_runtime'
    $jobRuntimePresent = $null -ne $jobRuntime
    $contractVersion = Get-PcvDiagnosticObjectProperty -InputObject $jobRuntime -Name 'contract_version'
    $owner = Get-PcvDiagnosticObjectProperty -InputObject $jobRuntime -Name 'owner'
    $stateStore = Get-PcvDiagnosticObjectProperty -InputObject $jobRuntime -Name 'state_store'
    $dispatch = Get-PcvDiagnosticObjectProperty -InputObject $jobRuntime -Name 'dispatch'
    $control = Get-PcvDiagnosticObjectProperty -InputObject $jobRuntime -Name 'control'
    $retry = Get-PcvDiagnosticObjectProperty -InputObject $control -Name 'retry'

    $stateStorePersistence = Get-PcvDiagnosticObjectProperty -InputObject $stateStore -Name 'persistence'
    $helperBoundary = Get-PcvDiagnosticObjectProperty -InputObject $dispatch -Name 'helper_boundary'
    $retryableOnly = Get-PcvDiagnosticObjectProperty -InputObject $retry -Name 'failed_error_retryable_only'
    $hostMutation = Get-PcvDiagnosticObjectProperty -InputObject $jobRuntime -Name 'host_mutation'

    $contractOk = (
        $jobRuntimePresent -and
        [int]$contractVersion -eq 1 -and
        [string]$owner -eq 'local-api' -and
        [string]$stateStorePersistence -eq 'json-file-snapshot' -and
        [string]$helperBoundary -eq 'dotnet-native-read-vm-create-lifecycle-delete-checkpoint-mutation' -and
        [bool]$retryableOnly -and
        [string]$hostMutation -eq 'native-read-routes-vm-create-lifecycle-delete-and-checkpoint-mutation'
    )

    if ($bodyParseable -and -not $jobRuntimePresent) {
        $warnings += 'job-runtime-contract-missing'
    }

    [ordered]@{
        schema_version = 1
        generated_at = (Get-Date).ToUniversalTime().ToString('o')
        runtime_policy = [ordered]@{
            present = $runtimePolicyPresent
            available = $runtimePolicyAvailable
            body_parseable = $bodyParseable
            job_runtime = [ordered]@{
                present = $jobRuntimePresent
                contract_version = $contractVersion
                owner = $owner
                contract_ok = $contractOk
            }
        }
        warnings = @($warnings)
    }
}

function Get-PcvDesktopNodeWinSwFailurePolicyFromXml {
    param([AllowNull()][string]$XmlText)

    $actions = @()
    $xmlPresent = -not [string]::IsNullOrWhiteSpace($XmlText)
    $xmlParseable = $false
    $warnings = @()

    if ($xmlPresent) {
        try {
            [xml]$xml = $XmlText
            $xmlParseable = $true
            $nodes = @($xml.service.onfailure)
            for ($index = 0; $index -lt $nodes.Count; $index++) {
                $node = $nodes[$index]
                if ($null -eq $node) {
                    continue
                }

                $actions += [ordered]@{
                    order = $index + 1
                    action = $node.GetAttribute('action')
                    delay = $(if ($node.HasAttribute('delay')) { $node.GetAttribute('delay') } else { $null })
                }
            }
        }
        catch {
            $warnings += 'winsw-xml-not-parseable'
        }
    }
    else {
        $warnings += 'winsw-xml-missing'
    }

    [ordered]@{
        xml_present = $xmlPresent
        xml_parseable = $xmlParseable
        configured = $actions.Count -gt 0
        action_count = $actions.Count
        actions = @($actions)
        warnings = @($warnings)
    }
}

function Get-PcvDesktopNodeScmFailurePolicyFromPlan {
    param([Parameter(Mandatory)]$Plan)

    $warnings = @()
    $actions = @()
    $failureCommand = @($Plan.service.commands.install) |
        Where-Object {
            [string]::Equals((Split-Path -Leaf ([string]$_.file_name)), 'sc.exe', [System.StringComparison]::OrdinalIgnoreCase) -and
            @($_.arguments).Count -gt 0 -and
            [string]::Equals([string]$_.arguments[0], 'failure', [System.StringComparison]::OrdinalIgnoreCase)
        } |
        Select-Object -First 1

    if ($null -eq $failureCommand) {
        $warnings += 'scm-failure-command-missing'
    }
    else {
        $arguments = @($failureCommand.arguments)
        $actionsIndex = [Array]::IndexOf([object[]]$arguments, 'actions=')
        if ($actionsIndex -lt 0 -or $actionsIndex + 1 -ge $arguments.Count) {
            $warnings += 'scm-failure-actions-missing'
        }
        else {
            $parts = @([string]$arguments[$actionsIndex + 1] -split '/')
            for ($index = 0; $index + 1 -lt $parts.Count; $index += 2) {
                $action = ([string]$parts[$index]).Trim('"')
                if ([string]::IsNullOrWhiteSpace($action)) {
                    $action = 'none'
                }

                $actions += [ordered]@{
                    order = [int](($index / 2) + 1)
                    action = $action
                    delay = "$($parts[$index + 1]) ms"
                }
            }
        }
    }

    [ordered]@{
        configured = $actions.Count -gt 0
        action_count = $actions.Count
        actions = @($actions)
        warnings = @($warnings)
    }
}

function New-PcvDesktopNodeOperationalEvidenceSummary {
    param(
        [Parameter(Mandatory)]$Plan,
        [AllowNull()][string]$WinSwXmlText
    )

    $diagnosticsPolicy = Get-PcvDesktopNodePlanDiagnosticsPolicy -Plan $Plan
    $serviceLogFiles = @()
    if (Test-Path -LiteralPath $Plan.paths.service_logs_root -PathType Container) {
        $serviceLogFiles = @(
            Get-ChildItem -LiteralPath $Plan.paths.service_logs_root -File -ErrorAction Stop |
                Where-Object { $_.Name -match '\.(log|out|err)(\.\d+)?$' -or $_.Extension -eq '.log' } |
                Sort-Object Name |
                ForEach-Object {
                    [ordered]@{
                        name = $_.Name
                        length = $_.Length
                        diagnostic_artifact = 'service-log-' + ($_.Name -replace '[^A-Za-z0-9_.-]', '_')
                    }
                }
        )
    }

    $serviceRecoverySource = 'winsw-xml'
    $onFailure = Get-PcvDesktopNodeWinSwFailurePolicyFromXml -XmlText $WinSwXmlText
    if ([string]$Plan.service.mode -eq 'dotnet-windows-service') {
        $serviceRecoverySource = 'scm-failure-actions'
        $onFailure = Get-PcvDesktopNodeScmFailurePolicyFromPlan -Plan $Plan
    }

    [ordered]@{
        schema_version = 1
        generated_at = (Get-Date).ToUniversalTime().ToString('o')
        decision = 'DESKTOP_NODE_PHASE23_OPERATIONAL_EVIDENCE_DECISION: jsonl-first-long-run-evidence-with-eventlog-transition-deferred'
        host_mutation = [ordered]@{
            service_mutation_performed = $false
            event_log_registration_performed = $false
            firewall_mutation_performed = $false
            elevated_msi_action_performed = $false
        }
        service_recovery = [ordered]@{
            source = $serviceRecoverySource
            on_failure = $onFailure
        }
        service_logs = [ordered]@{
            retention = [ordered]@{
                path = $diagnosticsPolicy.service_logs.path
                max_file_bytes = [int]$diagnosticsPolicy.service_logs.max_file_bytes
                retained_files = [int]$diagnosticsPolicy.service_logs.retained_files
            }
            observed = [ordered]@{
                count = $serviceLogFiles.Count
                files = @($serviceLogFiles)
            }
        }
        windows_event_log = [ordered]@{
            enabled_by_default = [bool]$diagnosticsPolicy.windows_event_log.enabled_by_default
            registration_owner = [string]$diagnosticsPolicy.windows_event_log.registration_owner
            source = [string]$diagnosticsPolicy.windows_event_log.source
            log_name = [string]$diagnosticsPolicy.windows_event_log.log_name
        }
    }
}

function New-PcvDesktopNodeDiagnosticBundleSourceList {
    param([Parameter(Mandatory)][string]$BundlePath)

    $definitions = @(
        [ordered]@{ name = 'summary'; artifact = 'summary.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'service_status'; artifact = 'service-status-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'service_host_status'; artifact = 'service-host-status-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'runtime_policy'; artifact = 'runtime-policy-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'lan_security_policy'; artifact = 'lan-security-policy-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'update_policy'; artifact = 'update-policy-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'migration_plan'; artifact = 'migration-plan-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'rollback_state'; artifact = 'rollback-state-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'diagnostics_self_audit'; artifact = 'diagnostics-self-audit.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'operational_evidence'; artifact = 'operational-evidence-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'service_host_metadata'; artifact = 'service-host-metadata-redacted.json'; required = $true; redacted = $true },
        [ordered]@{ name = 'product_manifest'; artifact = 'product-manifest-redacted.json'; required = $false; redacted = $true },
        [ordered]@{ name = 'events'; artifact = 'events-redacted.jsonl'; required = $false; redacted = $true },
        [ordered]@{ name = 'install_log'; artifact = 'install-redacted.jsonl'; required = $false; redacted = $true },
        [ordered]@{ name = 'jobs'; artifact = 'jobs-redacted.json'; required = $false; redacted = $true },
        [ordered]@{ name = 'update_transaction_journal'; artifact = 'update-transaction-journal-redacted.json'; required = $false; redacted = $true },
        [ordered]@{ name = 'service_host_config'; artifact = 'service-host-config-redacted.xml'; required = $false; redacted = $true }
    )

    $sources = @()
    foreach ($definition in $definitions) {
        $artifactPath = Join-Path $BundlePath $definition.artifact
        if ([bool]$definition.required -or (Test-Path -LiteralPath $artifactPath -PathType Leaf)) {
            $sources += [ordered]@{
                name = $definition.name
                artifact = $definition.artifact
                required = [bool]$definition.required
                redacted = [bool]$definition.redacted
                present = Test-Path -LiteralPath $artifactPath -PathType Leaf
            }
        }
    }

    Get-ChildItem -LiteralPath $BundlePath -File -Filter 'service-log-*' -ErrorAction SilentlyContinue |
        Sort-Object Name |
        ForEach-Object {
            $sources += [ordered]@{
                name = 'service_log'
                artifact = $_.Name
                required = $false
                redacted = $true
                present = $true
            }
        }

    $sources
}

function Write-PcvDesktopNodeDiagnosticBundleManifest {
    param(
        [Parameter(Mandatory)]$Plan,
        [Parameter(Mandatory)][string]$BundlePath,
        [System.Collections.IDictionary]$PathRedactions
    )

    $diagnosticsPolicy = Get-PcvDesktopNodePlanDiagnosticsPolicy -Plan $Plan
    $policy = [ordered]@{}
    foreach ($key in $diagnosticsPolicy.Keys) {
        $policy[$key] = $diagnosticsPolicy[$key]
    }
    $policy['network'] = Get-PcvDesktopNodePlanNetworkPolicy -Plan $Plan
    $policy['update'] = Get-PcvDesktopNodeUpdatePolicy -Paths $Plan.paths
    $selfAuditPath = Join-Path $BundlePath 'diagnostics-self-audit.json'
    $selfAudit = $null
    if (Test-Path -LiteralPath $selfAuditPath -PathType Leaf) {
        $selfAudit = Get-Content -LiteralPath $selfAuditPath -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    }

    $manifest = [ordered]@{
        schema_version = 1
        generated_at = (Get-Date).ToUniversalTime().ToString('o')
        decision = $script:PcvDesktopNodeDiagnosticsDecision
        redaction_version = 1
        policy = $policy
        self_audit = $selfAudit
        sources = @(New-PcvDesktopNodeDiagnosticBundleSourceList -BundlePath $BundlePath)
    }

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject $manifest `
        -Path (Join-Path $BundlePath 'diagnostics-manifest.json') `
        -PathRedactions $PathRedactions
}

function New-PcvDesktopNodeDiagnosticBundle {
    param(
        [Parameter(Mandatory)]$Plan,
        [string]$OutputRoot,
        [scriptblock]$InvokeProcess,
        [scriptblock]$CollectServiceStatus,
        [scriptblock]$CollectRuntimePolicy
    )

    if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
        $OutputRoot = $Plan.paths.diagnostics_root
    }
    if ($null -eq $InvokeProcess) {
        $InvokeProcess = {
            param([string]$FileName, [string[]]$Arguments)
            Invoke-PcvProductNativeProcess -FileName $FileName -Arguments $Arguments
        }
    }
    if ($null -eq $CollectServiceStatus) {
        $CollectServiceStatus = {
            param($Plan, [scriptblock]$InvokeProcess)
            Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.status `
                -InvokeProcess $InvokeProcess
        }
    }
    if ($null -eq $CollectRuntimePolicy) {
        $CollectRuntimePolicy = {
            param($Plan)
            $uri = $Plan.service.config.prefix.TrimEnd('/') + '/api/v1/runtime/policy'
            try {
                $response = Invoke-WebRequest -Uri $uri -TimeoutSec 5 -ErrorAction Stop
                [ordered]@{
                    ok = $true
                    uri = $uri
                    status_code = [int]$response.StatusCode
                    body = $response.Content
                }
            }
            catch {
                [ordered]@{
                    ok = $false
                    uri = $uri
                    error = [ordered]@{
                        code = 'PCV_PRODUCT_RUNTIME_POLICY_UNAVAILABLE'
                        message = 'Desktop Node runtime policy response is unavailable.'
                        detail = $_.Exception.Message
                    }
                }
            }
        }
    }

    $stamp = (Get-Date).ToUniversalTime().ToString('yyyyMMdd-HHmmss')
    $suffix = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $bundlePath = Join-Path $OutputRoot "bundle-$stamp-$suffix"
    New-Item -ItemType Directory -Path $bundlePath -Force -ErrorAction Stop | Out-Null
    $pathRedactions = Get-PcvDesktopNodeDiagnosticPathRedactions -Plan $Plan
    $wrapperDelegationStatus = [ordered]@{
        actual_execution = 'code-level-product-wrapper'
        diagnostic_bundle_product_wrapper_delegation = 'code-level-product-action-orchestrator'
        host_mutation_performed = $false
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
    }
    $hasWinSw = $null -ne $Plan.service.PSObject.Properties['winsw']
    $winSwXmlPath = if (
        $hasWinSw -and
        $Plan.service.winsw.Contains('xml_path') -and
        -not [string]::IsNullOrWhiteSpace([string]$Plan.service.winsw.xml_path)
    ) {
        [string]$Plan.service.winsw.xml_path
    }
    else {
        $null
    }
    $winSwExePath = if (
        $hasWinSw -and
        $Plan.service.winsw.Contains('staged_path') -and
        -not [string]::IsNullOrWhiteSpace([string]$Plan.service.winsw.staged_path)
    ) {
        [string]$Plan.service.winsw.staged_path
    }
    else {
        [string]$Plan.service.host.executable_path
    }

    $summary = [ordered]@{
        generated_at = (Get-Date).ToUniversalTime().ToString('o')
        product_root = $Plan.product_root
        data_root = $Plan.data_root
        service_name = $Plan.service.config.service_name
        prefix = $Plan.service.config.prefix
        protected_token_file_present = Test-Path -LiteralPath $Plan.paths.token_protected_file -PathType Leaf
        legacy_token_file_present = Test-Path -LiteralPath $Plan.paths.token_file -PathType Leaf
        event_log_present = Test-Path -LiteralPath $Plan.paths.event_log -PathType Leaf
        job_store_present = Test-Path -LiteralPath $Plan.paths.job_store -PathType Leaf
    }
    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject $summary `
        -Path (Join-Path $bundlePath 'summary.json') `
        -PathRedactions $pathRedactions

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject $wrapperDelegationStatus `
        -Path (Join-Path $bundlePath 'product-wrapper-delegation-redacted.json') `
        -PathRedactions $pathRedactions

    $serviceStatus = & $CollectServiceStatus -Plan $Plan -InvokeProcess $InvokeProcess
    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject $serviceStatus `
        -Path (Join-Path $bundlePath 'service-status-redacted.json') `
        -PathRedactions $pathRedactions

    $serviceHostStatus = Invoke-PcvProductProcessCommand `
        -Commands $Plan.service.commands.status `
        -InvokeProcess $InvokeProcess
    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject $serviceHostStatus `
        -Path (Join-Path $bundlePath 'service-host-status-redacted.json') `
        -PathRedactions $pathRedactions

    $runtimePolicy = & $CollectRuntimePolicy -Plan $Plan
    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject $runtimePolicy `
        -Path (Join-Path $bundlePath 'runtime-policy-redacted.json') `
        -PathRedactions $pathRedactions

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject (New-PcvDesktopNodeDiagnosticSelfAudit -RuntimePolicy $runtimePolicy) `
        -Path (Join-Path $bundlePath 'diagnostics-self-audit.json') `
        -PathRedactions $pathRedactions

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject (Get-PcvDesktopNodePlanNetworkPolicy -Plan $Plan) `
        -Path (Join-Path $bundlePath 'lan-security-policy-redacted.json') `
        -PathRedactions $pathRedactions

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject (Get-PcvDesktopNodeUpdatePolicy -Paths $Plan.paths) `
        -Path (Join-Path $bundlePath 'update-policy-redacted.json') `
        -PathRedactions $pathRedactions

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject (New-PcvDesktopNodeConfigMigrationPlan -FromVersion $Plan.version -ToVersion $Plan.version -Paths $Plan.paths -DryRun) `
        -Path (Join-Path $bundlePath 'migration-plan-redacted.json') `
        -PathRedactions $pathRedactions

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject ([ordered]@{
            schema_version = 1
            previous_root = $Plan.paths.previous_product_root
            previous_root_exists = Test-Path -LiteralPath $Plan.paths.previous_product_root -PathType Container
            failed_root = "$($Plan.product_root).failed"
            failed_root_preserved_for_diagnostics = $true
        }) `
        -Path (Join-Path $bundlePath 'rollback-state-redacted.json') `
        -PathRedactions $pathRedactions

    if (Test-Path -LiteralPath $Plan.paths.manifest_path -PathType Leaf) {
        $manifestText = Get-Content -LiteralPath $Plan.paths.manifest_path -Raw -ErrorAction Stop
        ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $manifestText -PathRedactions $pathRedactions |
            Set-Content -LiteralPath (Join-Path $bundlePath 'product-manifest-redacted.json') -Encoding UTF8 -ErrorAction Stop
    }

    if (Test-Path -LiteralPath $Plan.paths.event_log -PathType Leaf) {
        $eventText = Get-Content -LiteralPath $Plan.paths.event_log -Raw -ErrorAction Stop
        ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $eventText -PathRedactions $pathRedactions |
            Set-Content -LiteralPath (Join-Path $bundlePath 'events-redacted.jsonl') -Encoding UTF8 -ErrorAction Stop
    }

    if (Test-Path -LiteralPath $Plan.paths.install_log -PathType Leaf) {
        $installText = Get-Content -LiteralPath $Plan.paths.install_log -Raw -ErrorAction Stop
        ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $installText -PathRedactions $pathRedactions |
            Set-Content -LiteralPath (Join-Path $bundlePath 'install-redacted.jsonl') -Encoding UTF8 -ErrorAction Stop
    }

    if (Test-Path -LiteralPath $Plan.paths.job_store -PathType Leaf) {
        $jobText = Get-Content -LiteralPath $Plan.paths.job_store -Raw -ErrorAction Stop
        ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $jobText -PathRedactions $pathRedactions |
            Set-Content -LiteralPath (Join-Path $bundlePath 'jobs-redacted.json') -Encoding UTF8 -ErrorAction Stop
    }

    if (Test-Path -LiteralPath $Plan.paths.update_transaction_journal -PathType Leaf) {
        $journalText = Get-Content -LiteralPath $Plan.paths.update_transaction_journal -Raw -ErrorAction Stop
        ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $journalText -PathRedactions $pathRedactions |
            Set-Content -LiteralPath (Join-Path $bundlePath 'update-transaction-journal-redacted.json') -Encoding UTF8 -ErrorAction Stop
    }

    $winSwXmlText = $null
    if (-not [string]::IsNullOrWhiteSpace($winSwXmlPath) -and (Test-Path -LiteralPath $winSwXmlPath -PathType Leaf)) {
        $winSwXmlText = Get-Content -LiteralPath $winSwXmlPath -Raw -ErrorAction Stop
        ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $winSwXmlText -PathRedactions $pathRedactions |
            Set-Content -LiteralPath (Join-Path $bundlePath 'service-host-config-redacted.xml') -Encoding UTF8 -ErrorAction Stop
    }

    if (Test-Path -LiteralPath $Plan.paths.service_logs_root -PathType Container) {
        Get-ChildItem -LiteralPath $Plan.paths.service_logs_root -File -ErrorAction Stop |
            Where-Object { $_.Name -match '\.(log|out|err)(\.\d+)?$' -or $_.Extension -eq '.log' } |
            ForEach-Object {
                $targetName = 'service-log-' + ($_.Name -replace '[^A-Za-z0-9_.-]', '_')
                $logText = Get-Content -LiteralPath $_.FullName -Raw -ErrorAction Stop
                ConvertTo-PcvDesktopNodeDiagnosticRedactedText -Text $logText -PathRedactions $pathRedactions |
                    Set-Content -LiteralPath (Join-Path $bundlePath $targetName) -Encoding UTF8 -ErrorAction Stop
            }
    }

    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject (New-PcvDesktopNodeOperationalEvidenceSummary -Plan $Plan -WinSwXmlText $winSwXmlText) `
        -Path (Join-Path $bundlePath 'operational-evidence-redacted.json') `
        -PathRedactions $pathRedactions

    $metadata = [ordered]@{
        mode = [string]$Plan.service.mode
        default_owner = $(if ($Plan.service.PSObject.Properties['host']) { $Plan.service.host.default_owner } else { $null })
        source_path = $(if ($hasWinSw) { $Plan.service.winsw.source_path } else { $null })
        staged_path = $(if ($hasWinSw) { $Plan.service.winsw.staged_path } else { $Plan.service.host.executable_path })
        config_path = $(if ($hasWinSw) { $Plan.service.winsw.xml_path } else { $null })
        source_sha256 = $(if ($hasWinSw) { $Plan.service.winsw.source_sha256 } else { $null })
        staged_sha256 = $(if (Test-Path -LiteralPath $winSwExePath -PathType Leaf) {
                Get-PcvFileSha256 -Path $winSwExePath
            }
            else {
                $null
            })
    }
    Write-PcvDesktopNodeDiagnosticJsonFile `
        -InputObject $metadata `
        -Path (Join-Path $bundlePath 'service-host-metadata-redacted.json') `
        -PathRedactions $pathRedactions

    Write-PcvDesktopNodeDiagnosticBundleManifest `
        -Plan $Plan `
        -BundlePath $bundlePath `
        -PathRedactions $pathRedactions

    [ordered]@{
        ok = $true
        path = $bundlePath
        actual_execution = $wrapperDelegationStatus.actual_execution
        diagnostic_bundle_product_wrapper_delegation = $wrapperDelegationStatus.diagnostic_bundle_product_wrapper_delegation
        host_mutation_performed = $wrapperDelegationStatus.host_mutation_performed
        public_trusted_signing = $wrapperDelegationStatus.public_trusted_signing
        external_stable_publication = $wrapperDelegationStatus.external_stable_publication
    }
}

function Backup-PcvDesktopNodeProductRoot {
    param(
        [Parameter(Mandatory)][string]$ProductRoot,
        [Parameter(Mandatory)][string]$PreviousProductRoot,
        [scriptblock]$MovePath,
        [scriptblock]$RemovePath
    )

    if ($null -eq $MovePath) {
        $MovePath = {
            param([string]$Source, [string]$Destination)
            Move-Item -LiteralPath $Source -Destination $Destination -Force -ErrorAction Stop
        }
    }
    if ($null -eq $RemovePath) {
        $RemovePath = {
            param([string]$Path)
            Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction Stop
        }
    }

    if ([string]::IsNullOrWhiteSpace($ProductRoot)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_ROOT_REQUIRED' `
                -Message 'Desktop Node product root is required.' `
                -Detail 'Update cannot backup without a target product root.')
    }

    if ([string]::IsNullOrWhiteSpace($PreviousProductRoot)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_PREVIOUS_ROOT_REQUIRED' `
                -Message 'Desktop Node previous product root is required.' `
                -Detail 'Update cannot backup without a previous product root path.')
    }

    $stagingProductRoot = "$PreviousProductRoot.staging"
    $previousExists = Test-Path -LiteralPath $PreviousProductRoot
    $stagingExists = Test-Path -LiteralPath $stagingProductRoot
    if ($previousExists -and $stagingExists) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED' `
                -Message 'Desktop Node previous product backup requires explicit recovery.' `
                -Detail "Both backup paths exist: $PreviousProductRoot and $stagingProductRoot")
    }

    if ($stagingExists) {
        try {
            & $MovePath $stagingProductRoot $PreviousProductRoot
            $previousExists = $true
        }
        catch {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED' `
                    -Message 'Desktop Node previous product backup recovery failed.' `
                    -Detail $_.Exception.Message)
        }
    }

    if (-not (Test-Path -LiteralPath $ProductRoot -PathType Container)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_ROOT_MISSING' `
                -Message 'Desktop Node product root is missing.' `
                -Detail $ProductRoot)
    }

    $previousStaged = $false
    if ($previousExists) {
        try {
            & $MovePath $PreviousProductRoot $stagingProductRoot
            $previousStaged = $true
        }
        catch {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_UPDATE_BACKUP_FAILED' `
                    -Message 'Desktop Node previous product backup could not be staged.' `
                    -Detail $_.Exception.Message)
        }
    }

    try {
        & $MovePath $ProductRoot $PreviousProductRoot
    }
    catch {
        $backupError = $_
        try {
            if (Test-Path -LiteralPath $PreviousProductRoot) {
                $partialProductRoot = "$PreviousProductRoot.partial.$([guid]::NewGuid().ToString('N'))"
                & $MovePath $PreviousProductRoot $partialProductRoot
            }
            if ($previousStaged) {
                if (-not (Test-Path -LiteralPath $stagingProductRoot -PathType Container)) {
                    throw "Backup compensation source is missing: $stagingProductRoot"
                }
                & $MovePath $stagingProductRoot $PreviousProductRoot
            }
        }
        catch {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_UPDATE_BACKUP_COMPENSATION_FAILED' `
                    -Message 'Desktop Node product root backup and compensation failed.' `
                    -Detail "$($backupError.Exception.Message) Compensation: $($_.Exception.Message)")
        }

        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_UPDATE_BACKUP_FAILED' `
                -Message 'Desktop Node product root backup failed and the previous backup was restored.' `
                -Detail $backupError.Exception.Message)
    }

    if ($previousStaged -and (Test-Path -LiteralPath $stagingProductRoot)) {
        try {
            & $RemovePath $stagingProductRoot
        }
        catch {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_UPDATE_BACKUP_FAILED' `
                    -Message 'Desktop Node product root backup staging cleanup failed.' `
                    -Detail $_.Exception.Message)
        }
    }

    [ordered]@{
        ok = $true
        backed_up = $true
        product_root = $ProductRoot
        previous_product_root = $PreviousProductRoot
    }
}

function Restore-PcvDesktopNodePreviousProductRoot {
    param(
        [Parameter(Mandatory)][string]$ProductRoot,
        [Parameter(Mandatory)][string]$PreviousProductRoot,
        [scriptblock]$MovePath,
        [scriptblock]$RemovePath
    )

    if ($null -eq $MovePath) {
        $MovePath = {
            param([string]$Source, [string]$Destination)
            Move-Item -LiteralPath $Source -Destination $Destination -Force -ErrorAction Stop
        }
    }
    if ($null -eq $RemovePath) {
        $RemovePath = {
            param([string]$Path)
            Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction Stop
        }
    }

    if ([string]::IsNullOrWhiteSpace($ProductRoot)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_ROOT_REQUIRED' `
                -Message 'Desktop Node product root is required.' `
                -Detail 'Rollback cannot restore without a target product root.')
    }

    if ([string]::IsNullOrWhiteSpace($PreviousProductRoot)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_PREVIOUS_ROOT_REQUIRED' `
                -Message 'Desktop Node previous product root is required.' `
                -Detail 'Rollback cannot restore without a previous product root path.')
    }

    if (-not (Test-Path -LiteralPath $PreviousProductRoot -PathType Container)) {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_PREVIOUS_ROOT_MISSING' `
                -Message 'Desktop Node previous product root is missing.' `
                -Detail $PreviousProductRoot)
    }

    $previousManifestPath = Join-PcvProductPath -Root $PreviousProductRoot -ChildPath @('product-manifest.json')
    try {
        $previousManifest = Read-PcvDesktopNodeProductManifest -Path $previousManifestPath
    }
    catch {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_PREVIOUS_MANIFEST_INVALID' `
                -Message 'Desktop Node previous product manifest is missing or invalid.' `
                -Detail $_.Exception.Message)
    }

    $failedProductRoot = "$ProductRoot.failed"
    $activeRootMoved = $false
    try {
        if (Test-Path -LiteralPath $ProductRoot) {
            if (Test-Path -LiteralPath $failedProductRoot) {
                & $RemovePath $failedProductRoot
            }
            & $MovePath $ProductRoot $failedProductRoot
            $activeRootMoved = $true
        }
    }
    catch {
        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_RESTORE_FAILED' `
                -Message 'Desktop Node product root restore failed.' `
                -Detail $_.Exception.Message)
    }

    try {
        & $MovePath $PreviousProductRoot $ProductRoot
    }
    catch {
        $promotionError = $_
        try {
            if (Test-Path -LiteralPath $ProductRoot) {
                $partialProductRoot = "$ProductRoot.restore-partial.$([guid]::NewGuid().ToString('N'))"
                & $MovePath $ProductRoot $partialProductRoot
            }
            if ($activeRootMoved) {
                if (-not (Test-Path -LiteralPath $failedProductRoot -PathType Container)) {
                    throw "Rollback compensation source is missing: $failedProductRoot"
                }
                & $MovePath $failedProductRoot $ProductRoot
            }
        }
        catch {
            return (New-PcvProductError `
                    -Code 'PCV_PRODUCT_RESTORE_COMPENSATION_FAILED' `
                    -Message 'Desktop Node product root restore and compensation failed.' `
                    -Detail "$($promotionError.Exception.Message) Compensation: $($_.Exception.Message)")
        }

        return (New-PcvProductError `
                -Code 'PCV_PRODUCT_RESTORE_FAILED' `
                -Message 'Desktop Node product root restore failed and the active product was restored.' `
                -Detail $promotionError.Exception.Message)
    }

    [ordered]@{
        ok = $true
        product_root = $ProductRoot
        previous_product_root = $PreviousProductRoot
        previous_version = [string]$previousManifest.version
        failed_root = $failedProductRoot
        failed_product_root = $failedProductRoot
        failed_root_preserved_for_diagnostics = $true
        restored = $true
    }
}

function Invoke-PcvDesktopNodeProductAction {
    param(
        [Parameter(Mandatory)]$Plan,
        [scriptblock]$CopyAssets,
        [scriptblock]$PrepareTokenFile,
        [scriptblock]$InvokeProcess,
        [scriptblock]$TestHealth,
        [scriptblock]$RestorePreviousProductRoot,
        [scriptblock]$BackupProductRoot,
        [scriptblock]$NewConfigMigrationPlan,
        [scriptblock]$CollectDiagnostics,
        [scriptblock]$GrantAdministratorsFullControl,
        [scriptblock]$RemovePath
    )

    if ($null -eq $InvokeProcess) {
        $InvokeProcess = {
            param([string]$FileName, [string[]]$Arguments)
            Invoke-PcvProductNativeProcess -FileName $FileName -Arguments $Arguments
        }
    }
    if ($null -eq $CopyAssets) {
        $CopyAssets = {
            param([string]$SourceRoot, [string]$ProductRoot, [string]$DataRoot, [string]$WinSwPath, [string]$Version)
            if ([string]::IsNullOrWhiteSpace($Version)) {
                $Version = '0.12.0'
            }
            Copy-PcvDesktopNodeProductAssets `
                -SourceRoot $SourceRoot `
                -ProductRoot $ProductRoot `
                -DataRoot $DataRoot `
                -Version $Version `
                -WinSwPath $WinSwPath
        }
    }
    if ($null -eq $PrepareTokenFile) {
        $PrepareTokenFile = {
            param([string]$Path)
            if (Test-Path -LiteralPath $Path -PathType Leaf) {
                return [ordered]@{
                    ok = $true
                    path = $Path
                    storage = 'dpapi-local-machine'
                    existing = $true
                }
            }

            $prepareParams = @{
                Path = $Path
                ServiceAccount = $Plan.service.config.service_account
                InvokeProcess = $InvokeProcess
            }
            if (Test-Path -LiteralPath $Plan.paths.token_file -PathType Leaf) {
                $legacyToken = (Get-Content -LiteralPath $Plan.paths.token_file -Raw -ErrorAction Stop).Trim()
                if (-not [string]::IsNullOrWhiteSpace($legacyToken)) {
                    $prepareParams.Token = $legacyToken
                }
            }

            $result = New-PcvDesktopNodeProductProtectedTokenFile @prepareParams
            if ($prepareParams.ContainsKey('Token')) {
                $result['migrated_from_legacy_token_file'] = $Plan.paths.token_file
                $result['legacy_token_file_retained'] = $true
            }
            $result
        }
    }
    if ($null -eq $TestHealth) {
        $TestHealth = {
            param([string]$Prefix)
            $uri = $Prefix.TrimEnd('/') + '/api/v1/runtime/policy'
            $headers = @{}
            $auth = 'none'
            if (Test-Path -LiteralPath $Plan.paths.token_protected_file -PathType Leaf) {
                $token = Read-PcvDesktopNodeProductProtectedTokenFile -Path $Plan.paths.token_protected_file
                $headers.Authorization = "Bearer $($token.token)"
                $auth = 'bearer-protected-token-file'
            }

            $requestArgs = @{
                Uri = $uri
                TimeoutSec = 10
                ErrorAction = 'Stop'
            }
            if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
                $requestArgs.UseBasicParsing = $true
            }
            if ($headers.Count -gt 0) {
                $requestArgs.Headers = $headers
            }
            $response = Invoke-WebRequest @requestArgs
            [ordered]@{
                ok = $true
                status_code = [int]$response.StatusCode
                uri = $uri
                auth = $auth
            }
        }
    }
    if ($null -eq $RestorePreviousProductRoot) {
        $RestorePreviousProductRoot = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            Restore-PcvDesktopNodePreviousProductRoot `
                -ProductRoot $ProductRoot `
                -PreviousProductRoot $PreviousProductRoot
        }
    }
    if ($null -eq $BackupProductRoot) {
        $BackupProductRoot = {
            param([string]$ProductRoot, [string]$PreviousProductRoot)
            Backup-PcvDesktopNodeProductRoot `
                -ProductRoot $ProductRoot `
                -PreviousProductRoot $PreviousProductRoot
        }
    }
    if ($null -eq $NewConfigMigrationPlan) {
        $NewConfigMigrationPlan = {
            param([string]$FromVersion, [string]$ToVersion, $Paths, [bool]$DryRun)
            $migrationArgs = @{
                FromVersion = $FromVersion
                ToVersion = $ToVersion
                Paths = $Paths
            }
            if ($DryRun) {
                $migrationArgs.DryRun = $true
            }
            New-PcvDesktopNodeConfigMigrationPlan @migrationArgs
        }
    }
    if ($null -eq $CollectDiagnostics) {
        $CollectDiagnostics = {
            param($Plan)
            New-PcvDesktopNodeDiagnosticBundle -Plan $Plan
        }
    }
    if ($null -eq $GrantAdministratorsFullControl) {
        $GrantAdministratorsFullControl = {
            param([string]$Path)
            Grant-PcvProductAdministratorsFullControl `
                -Path $Path `
                -InvokeProcess $InvokeProcess
        }
    }
    if ($null -eq $RemovePath) {
        $RemovePath = {
            param([string]$Path)

            if (-not (Test-Path -LiteralPath $Path)) {
                return [ordered]@{
                    ok = $true
                    path = $Path
                    removed = $false
                }
            }

            $removeItem = {
                $item = Get-Item -LiteralPath $Path -Force -ErrorAction Stop
                if ($item.PSIsContainer) {
                    Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction Stop
                }
                else {
                    Remove-Item -LiteralPath $Path -Force -ErrorAction Stop
                }
            }

            $aclRepair = $null
            try {
                & $removeItem
            }
            catch {
                $isHardenedTokenFile = [bool]$Plan.remove_data -and
                    ([string]::Equals($Path, $Plan.paths.token_file, [System.StringComparison]::OrdinalIgnoreCase) -or
                        [string]::Equals($Path, $Plan.paths.token_protected_file, [System.StringComparison]::OrdinalIgnoreCase)) -and
                    (Test-PcvProductAccessDeniedText -Text $_.Exception.Message)
                if (-not $isHardenedTokenFile) {
                    throw
                }

                $aclRepair = & $GrantAdministratorsFullControl -Path $Path
                if ($aclRepair.Contains('ok') -and -not [bool]$aclRepair.ok) {
                    return (New-PcvProductError `
                            -Code $aclRepair.error.code `
                            -Message $aclRepair.error.message `
                            -Detail $aclRepair.error.detail)
                }

                & $removeItem
            }

            $result = [ordered]@{
                ok = $true
                path = $Path
                removed = $true
            }
            if ($null -ne $aclRepair) {
                $result.acl_repair = $aclRepair
            }
            $result
        }
    }

    $executed = @()
    $removedPaths = @()
    $action = [string]$Plan.action
    $updateResult = $null
    $updateTransactionJournal = $null
    $updateBackupCompleted = $false
    $rollbackState = $null
    Write-PcvDesktopNodeInstallLogEvent `
        -Plan $Plan `
        -EventName 'product.action.start' `
        -Data ([ordered]@{
            remove_data = [bool]$Plan.remove_data
            requires_elevation = [bool]$Plan.requires_elevation
        })

    try {
        if ($action -eq 'Install') {
            $winSwSourcePath = $null
            if ($Plan.service.PSObject.Properties['winsw']) {
                $winSwSourcePath = $Plan.service.winsw.source_path
            }

            if ($Plan.service.PSObject.Properties['winsw'] -and $null -ne $Plan.service.winsw.missing_source_error) {
                throw "$($Plan.service.winsw.missing_source_error.code)|$($Plan.service.winsw.missing_source_error.message)|$($Plan.service.winsw.missing_source_error.detail)"
            }

            $copyResult = & $CopyAssets `
                -SourceRoot $Plan.source_root `
                -ProductRoot $Plan.product_root `
                -DataRoot $Plan.data_root `
                -WinSwPath $winSwSourcePath
            $executed += [ordered]@{ step = 'copy'; result = $copyResult }
            if ($copyResult.Contains('ok') -and -not [bool]$copyResult.ok) {
                throw 'PCV_PRODUCT_COPY_FAILED|Desktop Node product asset copy failed.|CopyAssets returned ok false.'
            }

            $tokenResult = & $PrepareTokenFile -Path $Plan.paths.token_protected_file
            $executed += [ordered]@{ step = 'token'; result = $tokenResult }
            if ($tokenResult.Contains('ok') -and -not [bool]$tokenResult.ok) {
                throw 'PCV_PRODUCT_TOKEN_FAILED|Desktop Node product token file preparation failed.|PrepareTokenFile returned ok false.'
            }

            $installResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.install `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.install'; result = $installResults }
            if (-not [bool]$installResults.ok) {
                Throw-PcvProductCommandResult -CommandResult $installResults
            }

            $startResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.start `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.start'; result = $startResults }
            if (-not [bool]$startResults.ok) {
                Throw-PcvProductCommandResult -CommandResult $startResults
            }

            $healthResult = & $TestHealth -Prefix $Plan.service.config.prefix
            $executed += [ordered]@{ step = 'health'; result = $healthResult }
            if ($healthResult.Contains('ok') -and -not [bool]$healthResult.ok) {
                throw 'PCV_PRODUCT_HEALTH_FAILED|Desktop Node product health check failed.|TestHealth returned ok false.'
            }
        }
        elseif ($action -eq 'Update') {
            $currentManifest = Read-PcvDesktopNodeProductManifest -Path $Plan.paths.manifest_path
            $fromVersion = [string]$currentManifest.version
            $toVersion = [string]$Plan.version
            $updateResult = [ordered]@{
                from_version = $fromVersion
                to_version = $toVersion
                rollback_attempted = $false
            }
            $updateSourceRoot = [string]$Plan.source_root
            $executed += [ordered]@{
                step = 'current-manifest'
                result = [ordered]@{
                    path = $Plan.paths.manifest_path
                    version = $fromVersion
                }
            }

            if ($Plan.Contains('update_source') -and
                $null -ne $Plan.update_source -and
                [bool]$Plan.update_source.enabled) {
                $sourceUri = [string]$Plan.update_source.source_uri
                $expectedSha256 = [string]$Plan.update_source.expected_sha256
                $downloadRoot = [string]$Plan.update_source.download_root
                $sourceResolution = Resolve-PcvDesktopNodeUpdatePackage `
                    -SourceUri $sourceUri `
                    -ExpectedSha256 $expectedSha256 `
                    -DownloadRoot $downloadRoot `
                    -ProductRoot ([string]$Plan.product_root)
                $executed += [ordered]@{ step = 'update-source-preflight'; result = $sourceResolution }
                if ($sourceResolution.Contains('ok') -and -not [bool]$sourceResolution.ok) {
                    if ($null -ne $sourceResolution.error) {
                        throw "$($sourceResolution.error.code)|$($sourceResolution.error.message)|$($sourceResolution.error.detail)"
                    }
                    throw 'PCV_PRODUCT_UPDATE_SOURCE_INVALID|Desktop Node update package source validation failed.|Resolve-PcvDesktopNodeUpdatePackage returned ok false.'
                }
                $updateResult.update_source = $sourceResolution
                $updateSourceRoot = [string]$sourceResolution.source_root
            }
            elseif ($Plan.Contains('update_catalog') -and
                $null -ne $Plan.update_catalog -and
                [bool]$Plan.update_catalog.enabled) {
                $catalogResolution = Resolve-PcvDesktopNodeUpdateCatalog `
                    -CatalogUri ([string]$Plan.update_catalog.catalog_uri) `
                    -Channel ([string]$Plan.update_catalog.channel)
                $executed += [ordered]@{ step = 'update-catalog-preflight'; result = $catalogResolution }
                if ($catalogResolution.Contains('ok') -and -not [bool]$catalogResolution.ok) {
                    if ($null -ne $catalogResolution.error) {
                        throw "$($catalogResolution.error.code)|$($catalogResolution.error.message)|$($catalogResolution.error.detail)"
                    }
                    throw 'PCV_PRODUCT_UPDATE_CATALOG_INVALID|Desktop Node update catalog validation failed.|Resolve-PcvDesktopNodeUpdateCatalog returned ok false.'
                }

                $updateResult.update_catalog = $catalogResolution
                if (-not [string]::IsNullOrWhiteSpace([string]$catalogResolution.version)) {
                    $toVersion = [string]$catalogResolution.version
                    $updateResult.to_version = $toVersion
                }

                $sourceResolution = Resolve-PcvDesktopNodeUpdatePackage `
                    -SourceUri ([string]$catalogResolution.source_uri) `
                    -ExpectedSha256 ([string]$catalogResolution.expected_sha256) `
                    -DownloadRoot ([string]$Plan.update_catalog.download_root) `
                    -ProductRoot ([string]$Plan.product_root)
                $executed += [ordered]@{ step = 'update-source-preflight'; result = $sourceResolution }
                if ($sourceResolution.Contains('ok') -and -not [bool]$sourceResolution.ok) {
                    if ($null -ne $sourceResolution.error) {
                        throw "$($sourceResolution.error.code)|$($sourceResolution.error.message)|$($sourceResolution.error.detail)"
                    }
                    throw 'PCV_PRODUCT_UPDATE_SOURCE_INVALID|Desktop Node update package source validation failed.|Resolve-PcvDesktopNodeUpdatePackage returned ok false.'
                }
                $updateResult.update_source = $sourceResolution
                $updateSourceRoot = [string]$sourceResolution.source_root
            }

            $payloadValidation = Test-PcvDesktopNodeUpdatePayload `
                -SourceRoot $updateSourceRoot `
                -ProductRoot $Plan.product_root `
                -Version $toVersion
            $executed += [ordered]@{ step = 'update-payload-preflight'; result = $payloadValidation }
            if ($payloadValidation.Contains('ok') -and -not [bool]$payloadValidation.ok) {
                if ($null -ne $payloadValidation.error) {
                    throw "$($payloadValidation.error.code)|$($payloadValidation.error.message)|$($payloadValidation.error.detail)"
                }
                throw 'PCV_PRODUCT_UPDATE_PAYLOAD_INVALID|Desktop Node update payload validation failed.|Test-PcvDesktopNodeUpdatePayload returned ok false.'
            }

            $updateTransactionJournal = New-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -FromVersion $fromVersion `
                -ToVersion $toVersion `
                -SourceRoot $updateSourceRoot `
                -UpdateSource $(if ($updateResult.Contains('update_source')) { $updateResult.update_source } else { $null }) `
                -UpdateCatalog $(if ($updateResult.Contains('update_catalog')) { $updateResult.update_catalog } else { $null }) `
                -PayloadValidation $payloadValidation
            $journalResult = Write-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal
            $updateResult.transaction_journal = $journalResult
            $executed += [ordered]@{ step = 'update-transaction.begin'; result = $journalResult }

            $stopResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.stop `
                -InvokeProcess $InvokeProcess
            $stopResults = Update-PcvWinSwStopCommandResultAllowed `
                -CommandResult $stopResults
            $executed += [ordered]@{ step = 'service.stop'; result = $stopResults }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'service-stop' `
                -Status 'in-progress' `
                -ServiceMutationStarted $true `
                -HostMutationPerformed ([bool]$stopResults.ok) `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step })
            if (-not [bool]$stopResults.ok) {
                Throw-PcvProductCommandResult -CommandResult $stopResults
            }

            $stopWaitResult = Wait-PcvWinSwServiceStopped `
                -Plan $Plan `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.stop.wait'; result = $stopWaitResult }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'service-stop-wait' `
                -Status 'in-progress' `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step })
            if ($stopWaitResult.Contains('ok') -and -not [bool]$stopWaitResult.ok) {
                if ($null -ne $stopWaitResult.error) {
                    throw "$($stopWaitResult.error.code)|$($stopWaitResult.error.message)|$($stopWaitResult.error.detail)"
                }
                throw 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT|Desktop Node service did not stop before the timeout.|Service status did not become stopped.'
            }

            $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                -Path $Plan.paths.job_store_pending_commit
            $executed += [ordered]@{
                step = 'job-store.pending-commit.guard-before-backup'
                result = $pendingCommitGuard
            }
            Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard

            $backupResult = & $BackupProductRoot `
                -ProductRoot $Plan.product_root `
                -PreviousProductRoot $Plan.paths.previous_product_root
            $executed += [ordered]@{ step = 'backup-product-root'; result = $backupResult }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'backup-product-root' `
                -Status 'in-progress' `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step })
            if ($backupResult.Contains('ok') -and -not [bool]$backupResult.ok) {
                if ($null -ne $backupResult.error) {
                    throw "$($backupResult.error.code)|$($backupResult.error.message)|$($backupResult.error.detail)"
                }
                throw 'PCV_PRODUCT_UPDATE_BACKUP_FAILED|Desktop Node product root backup failed.|BackupProductRoot returned ok false.'
            }
            if (-not ($backupResult.Contains('backed_up') -and [bool]$backupResult.backed_up)) {
                throw 'PCV_PRODUCT_UPDATE_BACKUP_FAILED|Desktop Node product root backup failed.|BackupProductRoot must return backed_up true.'
            }
            $updateBackupCompleted = $true

            $servicePayloadSourcePath = $null
            if ($Plan.service.PSObject.Properties['winsw']) {
                $servicePayloadSourcePath = $Plan.service.winsw.source_path
            }

            $copyResult = & $CopyAssets `
                -SourceRoot $updateSourceRoot `
                -ProductRoot $Plan.product_root `
                -DataRoot $Plan.data_root `
                -WinSwPath $servicePayloadSourcePath `
                -Version $toVersion
            $executed += [ordered]@{ step = 'copy'; result = $copyResult }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'copy' `
                -Status 'in-progress' `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step })
            if ($copyResult.Contains('ok') -and -not [bool]$copyResult.ok) {
                throw 'PCV_PRODUCT_COPY_FAILED|Desktop Node product asset copy failed.|CopyAssets returned ok false.'
            }

            $migrationPlan = & $NewConfigMigrationPlan `
                -FromVersion $fromVersion `
                -ToVersion $toVersion `
                -Paths $Plan.paths `
                -DryRun $true
            $updateResult.migration = $migrationPlan
            $executed += [ordered]@{ step = 'config-migration'; result = $migrationPlan }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'config-migration' `
                -Status 'in-progress' `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step })

            $migrationAllowsServiceStart = $true
            if ($migrationPlan.Contains('service_start_allowed')) {
                $migrationAllowsServiceStart = [bool]$migrationPlan.service_start_allowed
            }
            if (-not $migrationAllowsServiceStart) {
                $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                    -Path $Plan.paths.job_store_pending_commit
                $executed += [ordered]@{
                    step = 'job-store.pending-commit.guard-before-rollback-restore'
                    result = $pendingCommitGuard
                }
                Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard

                $rollbackResult = & $RestorePreviousProductRoot `
                    -ProductRoot $Plan.product_root `
                    -PreviousProductRoot $Plan.paths.previous_product_root
                $updateResult.rollback_attempted = $true
                $updateResult.rollback_result = $rollbackResult
                $executed += [ordered]@{ step = 'rollback.restore'; result = $rollbackResult }

                $migrationErrorCode = 'PCV_PRODUCT_CONFIG_MIGRATION_BLOCKED'
                $migrationErrorMessage = 'Config migration validation failed.'
                if ($migrationPlan.Contains('error') -and $null -ne $migrationPlan.error) {
                    if (-not [string]::IsNullOrWhiteSpace([string]$migrationPlan.error.code)) {
                        $migrationErrorCode = [string]$migrationPlan.error.code
                    }
                    if (-not [string]::IsNullOrWhiteSpace([string]$migrationPlan.error.message)) {
                        $migrationErrorMessage = [string]$migrationPlan.error.message
                    }
                }
                throw "$migrationErrorCode|$migrationErrorMessage|Config migration from $fromVersion to $toVersion blocked service start."
            }

            $startResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.start `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.start'; result = $startResults }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'service-start' `
                -Status 'in-progress' `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step })
            if (-not [bool]$startResults.ok) {
                $rollbackQuiesceResult = Invoke-PcvDesktopNodeRollbackQuiesce `
                    -Plan $Plan `
                    -InvokeProcess $InvokeProcess
                $executed += [ordered]@{
                    step = 'rollback.service.stop'
                    result = $rollbackQuiesceResult.stop
                }
                if ($null -ne $rollbackQuiesceResult.stop_wait) {
                    $executed += [ordered]@{
                        step = 'rollback.service.stop.wait'
                        result = $rollbackQuiesceResult.stop_wait
                    }
                }
                Assert-PcvDesktopNodeRollbackQuiesced -Result $rollbackQuiesceResult

                $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                    -Path $Plan.paths.job_store_pending_commit
                $executed += [ordered]@{
                    step = 'job-store.pending-commit.guard-before-rollback-restore'
                    result = $pendingCommitGuard
                }
                Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard

                $rollbackResult = & $RestorePreviousProductRoot `
                    -ProductRoot $Plan.product_root `
                    -PreviousProductRoot $Plan.paths.previous_product_root
                $updateResult.rollback_attempted = $true
                $updateResult.rollback_result = $rollbackResult
                $executed += [ordered]@{ step = 'rollback.restore'; result = $rollbackResult }
                throw 'PCV_PRODUCT_UPDATE_START_FAILED|Desktop Node product update service start failed.|Service start command failed after update.'
            }

            $healthResult = & $TestHealth -Prefix $Plan.service.config.prefix
            $executed += [ordered]@{ step = 'health'; result = $healthResult }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'health' `
                -Status 'in-progress' `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step })
            if ($healthResult.Contains('ok') -and -not [bool]$healthResult.ok) {
                $rollbackQuiesceResult = Invoke-PcvDesktopNodeRollbackQuiesce `
                    -Plan $Plan `
                    -InvokeProcess $InvokeProcess
                $executed += [ordered]@{
                    step = 'rollback.service.stop'
                    result = $rollbackQuiesceResult.stop
                }
                if ($null -ne $rollbackQuiesceResult.stop_wait) {
                    $executed += [ordered]@{
                        step = 'rollback.service.stop.wait'
                        result = $rollbackQuiesceResult.stop_wait
                    }
                }
                Assert-PcvDesktopNodeRollbackQuiesced -Result $rollbackQuiesceResult

                $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                    -Path $Plan.paths.job_store_pending_commit
                $executed += [ordered]@{
                    step = 'job-store.pending-commit.guard-before-rollback-restore'
                    result = $pendingCommitGuard
                }
                Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard

                $rollbackResult = & $RestorePreviousProductRoot `
                    -ProductRoot $Plan.product_root `
                    -PreviousProductRoot $Plan.paths.previous_product_root
                $updateResult.rollback_attempted = $true
                $updateResult.rollback_result = $rollbackResult
                $executed += [ordered]@{ step = 'rollback.restore'; result = $rollbackResult }
                throw 'PCV_PRODUCT_UPDATE_HEALTH_CHECK_FAILED|Desktop Node product update health check failed.|TestHealth returned ok false.'
            }
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage 'health' `
                -Status 'succeeded' `
                -RollbackAttempted $false `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step }) `
                -Completed
        }
        elseif ($action -in @('ConfigureInstalled', 'RepairInstalled')) {
            if ($action -eq 'RepairInstalled') {
                $stopResults = Invoke-PcvProductProcessCommand `
                    -Commands $Plan.service.commands.stop `
                    -InvokeProcess $InvokeProcess
                $stopResults = Update-PcvWinSwStopCommandResultAllowed `
                    -CommandResult $stopResults
                $executed += [ordered]@{ step = 'service.stop'; result = $stopResults }
                if (-not [bool]$stopResults.ok) {
                    Throw-PcvProductCommandResult -CommandResult $stopResults
                }

                $stopWaitResult = Wait-PcvWinSwServiceStopped `
                    -Plan $Plan `
                    -InvokeProcess $InvokeProcess
                $executed += [ordered]@{ step = 'service.stop.wait'; result = $stopWaitResult }
                if ($stopWaitResult.Contains('ok') -and -not [bool]$stopWaitResult.ok) {
                    if ($null -ne $stopWaitResult.error) {
                        throw "$($stopWaitResult.error.code)|$($stopWaitResult.error.message)|$($stopWaitResult.error.detail)"
                    }
                    throw 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT|Desktop Node service did not stop before the timeout.|Service status did not become stopped.'
                }
            }
            $tokenResult = & $PrepareTokenFile -Path $Plan.paths.token_protected_file
            $executed += [ordered]@{ step = 'token'; result = $tokenResult }
            if ($tokenResult.Contains('ok') -and -not [bool]$tokenResult.ok) {
                throw 'PCV_PRODUCT_TOKEN_FAILED|Desktop Node product token file preparation failed.|PrepareTokenFile returned ok false.'
            }

            if ($Plan.service.PSObject.Properties['winsw']) {
                $configResult = Write-PcvDesktopNodeWinSwConfiguration -ServicePlan $Plan.service
                $executed += [ordered]@{ step = 'winsw.config'; result = $configResult }
            }
            else {
                $configResult = [ordered]@{
                    ok = $true
                    mode = [string]$Plan.service.mode
                    executable_path = [string]$Plan.service.host.executable_path
                    binary_path = [string]$Plan.service.config.binary_path
                    mutation = 'scm-service-command-plan'
                }
                $executed += [ordered]@{ step = 'service.configure'; result = $configResult }
            }

            $installResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.install `
                -InvokeProcess $InvokeProcess
            $installResults = Assert-PcvProductCommandResultAllowed `
                -CommandResult $installResults `
                -AllowedExitCodes @(1073)
            $executed += [ordered]@{ step = 'service.install'; result = $installResults }

            $nativeServiceAction = $null
            if ($Plan.service -is [System.Collections.IDictionary] -and $Plan.service.Contains('native_service_action')) {
                $nativeServiceAction = $Plan.service['native_service_action']
            }
            elseif ($Plan.service.PSObject.Properties['native_service_action']) {
                $nativeServiceAction = $Plan.service.native_service_action
            }

            if ($null -ne $nativeServiceAction) {
                $startResults = [ordered]@{
                    ok = $true
                    skipped = $true
                    reason = 'native-service-action-controls-final-state'
                    native_action = [string]$nativeServiceAction.action
                }
            }
            else {
                $startResults = Invoke-PcvProductProcessCommand `
                    -Commands $Plan.service.commands.start `
                    -InvokeProcess $InvokeProcess
            }
            $executed += [ordered]@{ step = 'service.start'; result = $startResults }
            if (-not [bool]$startResults.ok) {
                Throw-PcvProductCommandResult -CommandResult $startResults
            }

            $healthResult = & $TestHealth -Prefix $Plan.service.config.prefix
            $executed += [ordered]@{ step = 'health'; result = $healthResult }
            if ($healthResult.Contains('ok') -and -not [bool]$healthResult.ok) {
                throw 'PCV_PRODUCT_HEALTH_FAILED|Desktop Node product health check failed.|TestHealth returned ok false.'
            }
        }
        elseif ($action -eq 'Rollback') {
            $stopResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.stop `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.stop'; result = $stopResults }

            $stopWaitResult = Wait-PcvWinSwServiceStopped `
                -Plan $Plan `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.stop.wait'; result = $stopWaitResult }
            if ($stopWaitResult.Contains('ok') -and -not [bool]$stopWaitResult.ok) {
                if ($null -ne $stopWaitResult.error) {
                    throw "$($stopWaitResult.error.code)|$($stopWaitResult.error.message)|$($stopWaitResult.error.detail)"
                }
                throw 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT|Desktop Node service did not stop before the timeout.|Service status did not become stopped.'
            }

            $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                -Path $Plan.paths.job_store_pending_commit
            $executed += [ordered]@{
                step = 'job-store.pending-commit.guard-before-restore'
                result = $pendingCommitGuard
            }
            Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard

            $restoreResult = & $RestorePreviousProductRoot `
                -ProductRoot $Plan.product_root `
                -PreviousProductRoot $Plan.paths.previous_product_root
            $executed += [ordered]@{ step = 'restore'; result = $restoreResult }
            if ($restoreResult.Contains('ok') -and -not [bool]$restoreResult.ok) {
                if ($null -ne $restoreResult.error) {
                    throw "$($restoreResult.error.code)|$($restoreResult.error.message)|$($restoreResult.error.detail)"
                }
                throw 'PCV_PRODUCT_RESTORE_FAILED|Desktop Node product root restore failed.|RestorePreviousProductRoot returned ok false.'
            }
            if (-not ($restoreResult.Contains('restored') -and [bool]$restoreResult.restored)) {
                throw 'PCV_PRODUCT_RESTORE_NOT_PERFORMED|Desktop Node product root restore was not performed.|RestorePreviousProductRoot must return restored true.'
            }
            if ($restoreResult.Contains('previous_version') -or $restoreResult.Contains('failed_root')) {
                $rollbackState = [ordered]@{
                    previous_version = $(if ($restoreResult.Contains('previous_version')) { [string]$restoreResult.previous_version } else { $null })
                    failed_root = $(if ($restoreResult.Contains('failed_root')) { [string]$restoreResult.failed_root } else { $null })
                    failed_root_preserved_for_diagnostics = $(if ($restoreResult.Contains('failed_root_preserved_for_diagnostics')) { [bool]$restoreResult.failed_root_preserved_for_diagnostics } else { $false })
                }
            }

            $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                -Path $Plan.paths.job_store_pending_commit
            $executed += [ordered]@{
                step = 'job-store.pending-commit.guard-before-rollback-start'
                result = $pendingCommitGuard
            }
            Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard

            $startResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.start `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.start'; result = $startResults }
            if (-not [bool]$startResults.ok) {
                Throw-PcvProductCommandResult -CommandResult $startResults
            }

            $healthResult = & $TestHealth -Prefix $Plan.service.config.prefix
            $executed += [ordered]@{ step = 'health'; result = $healthResult }
            if ($healthResult.Contains('ok') -and -not [bool]$healthResult.ok) {
                throw 'PCV_PRODUCT_HEALTH_FAILED|Desktop Node product health check failed.|TestHealth returned ok false.'
            }
        }
        elseif ($action -eq 'RemoveInstalled') {
            $stopResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.stop `
                -InvokeProcess $InvokeProcess
            $stopResults = Update-PcvWinSwStopCommandResultAllowed `
                -CommandResult $stopResults
            $executed += [ordered]@{ step = 'service.stop'; result = $stopResults }
            if (-not [bool]$stopResults.ok) {
                Throw-PcvProductCommandResult -CommandResult $stopResults
            }

            $stopWaitResult = Wait-PcvWinSwServiceStopped `
                -Plan $Plan `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.stop.wait'; result = $stopWaitResult }
            if ($stopWaitResult.Contains('ok') -and -not [bool]$stopWaitResult.ok) {
                if ($null -ne $stopWaitResult.error) {
                    throw "$($stopWaitResult.error.code)|$($stopWaitResult.error.message)|$($stopWaitResult.error.detail)"
                }
                throw 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT|Desktop Node service did not stop before the timeout.|Service status did not become stopped.'
            }

            if (-not [bool]$Plan.remove_data) {
                $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                    -Path $Plan.paths.job_store_pending_commit
                $executed += [ordered]@{
                    step = 'job-store.pending-commit.guard-before-service-removal'
                    result = $pendingCommitGuard
                }
                Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard
            }

            $uninstallResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.uninstall `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.uninstall'; result = $uninstallResults }
            if (-not [bool]$uninstallResults.ok -and
                -not (Test-PcvWinSwMissingServiceResult -Result $uninstallResults)) {
                Throw-PcvProductCommandResult -CommandResult $uninstallResults
            }

            $removeResults = @()
            if ([bool]$Plan.remove_data) {
                $orphanTempDiscovery = Find-PcvDesktopNodeJobStoreOrphanTempPaths `
                    -Paths $Plan.paths
                $executed += [ordered]@{
                    step = 'job-store.orphan-temp.discovery'
                    result = $orphanTempDiscovery
                }
                Assert-PcvDesktopNodeJobStoreOrphanTempDiscovery `
                    -Result $orphanTempDiscovery

                $removeDataPaths = @($Plan.delete_paths) + @($orphanTempDiscovery.paths)
                foreach ($path in $removeDataPaths) {
                    $removeResult = Invoke-PcvProductRemovePathWithRetry `
                        -Path $path `
                        -RemovePath $RemovePath
                    $removeResults += $removeResult
                    if ($removeResult.Contains('ok') -and -not [bool]$removeResult.ok) {
                        if ($null -ne $removeResult.error) {
                            throw "$($removeResult.error.code)|$($removeResult.error.message)|$($removeResult.error.detail)"
                        }
                        throw "PCV_PRODUCT_REMOVE_FAILED|Desktop Node product path removal failed.|$path"
                    }
                    if ($removeResult.Contains('removed') -and [bool]$removeResult.removed) {
                        $removedPaths += $path
                    }
                }
            }
            $executed += [ordered]@{ step = 'remove'; result = $removeResults }
        }
        elseif ($action -eq 'CollectDiagnostics') {
            $diagnosticsResult = & $CollectDiagnostics -Plan $Plan
            $executed += [ordered]@{ step = 'diagnostics'; result = $diagnosticsResult }
            if ($diagnosticsResult.Contains('ok') -and -not [bool]$diagnosticsResult.ok) {
                if ($null -ne $diagnosticsResult.error) {
                    throw "$($diagnosticsResult.error.code)|$($diagnosticsResult.error.message)|$($diagnosticsResult.error.detail)"
                }
                throw 'PCV_PRODUCT_DIAGNOSTICS_FAILED|Desktop Node diagnostic bundle creation failed.|CollectDiagnostics returned ok false.'
            }
        }
        elseif ($action -eq 'Status') {
            $statusResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.status `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.status'; result = $statusResults }

            $manifest = [ordered]@{
                path = $Plan.paths.manifest_path
                exists = Test-Path -LiteralPath $Plan.paths.manifest_path -PathType Leaf
            }
            $executed += [ordered]@{ step = 'manifest'; result = $manifest }
        }
        elseif ($action -eq 'Uninstall') {
            $stopResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.stop `
                -InvokeProcess $InvokeProcess
            $stopResults = Update-PcvWinSwStopCommandResultAllowed `
                -CommandResult $stopResults
            $executed += [ordered]@{ step = 'service.stop'; result = $stopResults }
            if (-not [bool]$stopResults.ok) {
                Throw-PcvProductCommandResult -CommandResult $stopResults
            }

            $stopWaitResult = Wait-PcvWinSwServiceStopped `
                -Plan $Plan `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.stop.wait'; result = $stopWaitResult }
            if ($stopWaitResult.Contains('ok') -and -not [bool]$stopWaitResult.ok) {
                if ($null -ne $stopWaitResult.error) {
                    throw "$($stopWaitResult.error.code)|$($stopWaitResult.error.message)|$($stopWaitResult.error.detail)"
                }
                throw 'PCV_PRODUCT_SERVICE_STOP_TIMEOUT|Desktop Node service did not stop before the timeout.|Service status did not become stopped.'
            }

            if (-not [bool]$Plan.remove_data) {
                $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                    -Path $Plan.paths.job_store_pending_commit
                $executed += [ordered]@{
                    step = 'job-store.pending-commit.guard-before-service-removal'
                    result = $pendingCommitGuard
                }
                Assert-PcvDesktopNodeJobStorePendingCommitGuard -Result $pendingCommitGuard
            }

            $uninstallResults = Invoke-PcvProductProcessCommand `
                -Commands $Plan.service.commands.uninstall `
                -InvokeProcess $InvokeProcess
            $executed += [ordered]@{ step = 'service.uninstall'; result = $uninstallResults }
            if (-not [bool]$uninstallResults.ok -and
                -not (Test-PcvWinSwMissingServiceResult -Result $uninstallResults)) {
                Throw-PcvProductCommandResult -CommandResult $uninstallResults
            }

            $pathsToRemove = @($Plan.product_root)
            if ([bool]$Plan.remove_data) {
                $orphanTempDiscovery = Find-PcvDesktopNodeJobStoreOrphanTempPaths `
                    -Paths $Plan.paths
                $executed += [ordered]@{
                    step = 'job-store.orphan-temp.discovery'
                    result = $orphanTempDiscovery
                }
                Assert-PcvDesktopNodeJobStoreOrphanTempDiscovery `
                    -Result $orphanTempDiscovery

                foreach ($path in @($Plan.delete_paths)) {
                    if ($path -ne $Plan.product_root) {
                        $pathsToRemove += $path
                    }
                }
                $pathsToRemove += @($orphanTempDiscovery.paths)
            }

            $removeResults = @()
            foreach ($path in $pathsToRemove) {
                $removeResult = Invoke-PcvProductRemovePathWithRetry `
                    -Path $path `
                    -RemovePath $RemovePath
                $removeResults += $removeResult
                if ($removeResult.Contains('ok') -and -not [bool]$removeResult.ok) {
                    if ($null -ne $removeResult.error) {
                        throw "$($removeResult.error.code)|$($removeResult.error.message)|$($removeResult.error.detail)"
                    }
                    throw "PCV_PRODUCT_REMOVE_FAILED|Desktop Node product path removal failed.|$path"
                }
                if ($removeResult.Contains('removed') -and [bool]$removeResult.removed) {
                    $removedPaths += $path
                }
            }
            $executed += [ordered]@{ step = 'remove'; result = $removeResults }
        }
        else {
            throw "PCV_PRODUCT_ACTION_UNSUPPORTED|Desktop Node product action is not supported by the Phase 12 orchestrator.|$action"
        }

        $result = [ordered]@{
            ok = $true
            action = $action
            executed = $executed
            removed_paths = $removedPaths
        }
        if ($null -ne $updateResult) {
            $result.update = $updateResult
        }
        if ($null -ne $rollbackState) {
            $result.rollback = $rollbackState
        }
        if (-not ([bool]$Plan.remove_data -and $action -in @('Uninstall', 'RemoveInstalled'))) {
            Write-PcvDesktopNodeInstallLogEvent `
                -Plan $Plan `
                -EventName 'product.action.success' `
                -Data ([ordered]@{
                    executed_steps = @($executed | ForEach-Object { [string]$_.step })
                    removed_path_count = @($removedPaths).Count
                })
        }
        $result
    }
    catch {
        $actionError = ConvertTo-PcvProductActionError -Message $_.Exception.Message
        if ($null -ne $updateTransactionJournal) {
            $rollbackAttempted = $false
            $rollbackResult = $null
            if ($null -ne $updateResult -and $updateResult.Contains('rollback_attempted')) {
                $rollbackAttempted = [bool]$updateResult.rollback_attempted
            }
            if ($null -ne $updateResult -and $updateResult.Contains('rollback_result')) {
                $rollbackResult = $updateResult.rollback_result
            }
            if ($updateBackupCompleted -and -not $rollbackAttempted) {
                $rollbackQuiesceResult = Invoke-PcvDesktopNodeRollbackQuiesce `
                    -Plan $Plan `
                    -InvokeProcess $InvokeProcess
                $executed += [ordered]@{
                    step = 'rollback.service.stop'
                    result = $rollbackQuiesceResult.stop
                }
                if ($null -ne $rollbackQuiesceResult.stop_wait) {
                    $executed += [ordered]@{
                        step = 'rollback.service.stop.wait'
                        result = $rollbackQuiesceResult.stop_wait
                    }
                }

                if (-not [bool]$rollbackQuiesceResult.ok) {
                    $actionError = $rollbackQuiesceResult.error
                    if ($null -ne $updateResult) {
                        $updateResult.rollback_attempted = $false
                        $updateResult.rollback_blocked = [ordered]@{
                            reason = 'service-not-quiesced'
                            quiesce = $rollbackQuiesceResult
                            error = $rollbackQuiesceResult.error
                        }
                    }
                }
                else {
                    $pendingCommitGuard = Test-PcvDesktopNodeJobStorePendingCommitGuard `
                        -Path $Plan.paths.job_store_pending_commit
                    $executed += [ordered]@{
                        step = 'job-store.pending-commit.guard-before-automatic-rollback'
                        result = $pendingCommitGuard
                    }
                    if (-not [bool]$pendingCommitGuard.ok) {
                        $actionError = $pendingCommitGuard.error
                        if ($null -ne $updateResult) {
                            $updateResult.rollback_attempted = $false
                            $updateResult.rollback_blocked = $pendingCommitGuard
                        }
                    }
                    else {
                        $rollbackAttempted = $true
                        try {
                            $rollbackResult = & $RestorePreviousProductRoot `
                                -ProductRoot $Plan.product_root `
                                -PreviousProductRoot $Plan.paths.previous_product_root
                        }
                        catch {
                            $rollbackResult = (New-PcvProductError `
                                    -Code 'PCV_PRODUCT_TRANSACTION_ROLLBACK_FAILED' `
                                    -Message 'Desktop Node product update transactional rollback failed.' `
                                    -Detail $_.Exception.Message)
                        }
                        if ($null -ne $updateResult) {
                            $updateResult.rollback_attempted = $true
                            $updateResult.rollback_result = $rollbackResult
                        }
                        $executed += [ordered]@{ step = 'rollback.restore'; result = $rollbackResult }
                    }
                }
            }

            $rollbackSucceeded = $false
            if ($rollbackAttempted -and $null -ne $rollbackResult) {
                $rollbackSucceeded = ($rollbackResult.Contains('restored') -and [bool]$rollbackResult.restored)
            }
            $updateStatus = $(if ($rollbackAttempted) {
                    if ($rollbackSucceeded) { 'failed-rolled-back' } else { 'failed-rollback-failed' }
                }
                else {
                    'failed'
                })
            $updateResult.transaction_journal = Update-PcvDesktopNodeUpdateTransactionJournal `
                -Plan $Plan `
                -Journal $updateTransactionJournal `
                -Stage ([string]$updateTransactionJournal.stage) `
                -Status $updateStatus `
                -RollbackAttempted $rollbackAttempted `
                -RollbackResult $rollbackResult `
                -Error $actionError `
                -ExecutedSteps @($executed | ForEach-Object { [string]$_.step }) `
                -Completed
        }
        $result = [ordered]@{
            ok = $false
            action = $action
            error = $actionError
            executed = $executed
        }
        if ($null -ne $updateResult) {
            $result.update = $updateResult
        }
        if ($null -ne $rollbackState) {
            $result.rollback = $rollbackState
        }
        Write-PcvDesktopNodeInstallLogEvent `
            -Plan $Plan `
            -EventName 'product.action.failure' `
            -Data ([ordered]@{
                error_code = [string]$actionError.code
                error_message = [string]$actionError.message
                error_detail = [string]$actionError.detail
                executed_steps = @($executed | ForEach-Object { [string]$_.step })
            })
        $result
    }
}

Export-ModuleMember -Function `
    ConvertTo-PcvProductJson, `
    ConvertTo-PcvDesktopNodeDiagnosticRedactedObject, `
    Copy-PcvDesktopNodeProductAssets, `
    Copy-PcvDesktopNodeWinSwArtifact, `
    Get-PcvFileSha256, `
    Get-PcvDesktopNodeDiagnosticsPolicy, `
    Get-PcvDesktopNodeDataAclPolicy, `
    Get-PcvDesktopNodeAssetFileList, `
    Get-PcvDesktopNodeProductAssets, `
    Get-PcvDesktopNodeProductDefaults, `
    Get-PcvDesktopNodeRuntimePayloadFileList, `
    Invoke-PcvDesktopNodeProductAction, `
    Invoke-PcvDesktopNodeLogRotation, `
    Invoke-PcvProductProcessCommand, `
    New-PcvDesktopNodeDiagnosticBundle, `
    New-PcvDesktopNodeConfigMigrationPlan, `
    New-PcvDesktopNodeDryRunResult, `
    New-PcvDesktopNodeEventLogRegistrationPlan, `
    New-PcvDesktopNodeProductProtectedTokenFile, `
    New-PcvDesktopNodeProductManifest, `
    New-PcvDesktopNodeProductPlan, `
    New-PcvDesktopNodeWinSwArguments, `
    New-PcvDesktopNodeWinSwXml, `
    New-PcvProductError, `
    Read-PcvDesktopNodeProductProtectedTokenFile, `
    Read-PcvDesktopNodeProductManifest, `
    Resolve-PcvDesktopNodeUpdateCatalog, `
    Resolve-PcvDesktopNodeUpdatePackage, `
    Resolve-PcvDesktopNodeWinSwArtifact, `
    Resolve-PcvDesktopNodeProductPaths, `
    Test-PcvDiagnosticSensitiveKey, `
    Test-PcvDesktopNodeUpdatePayload, `
    Test-PcvWinSwMissingServiceResult
