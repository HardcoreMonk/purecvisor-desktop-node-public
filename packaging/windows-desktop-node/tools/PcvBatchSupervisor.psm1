Set-StrictMode -Version Latest

function Get-PcvBatchPropertyValue {
    param(
        [AllowNull()]$InputObject,
        [Parameter(Mandatory)][string]$Name,
        [AllowNull()]$Default = $null
    )

    if ($null -eq $InputObject) {
        return $Default
    }
    if ($InputObject -is [System.Collections.IDictionary]) {
        if ($InputObject.Contains($Name)) {
            return $InputObject[$Name]
        }
        return $Default
    }

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -ne $property) {
        return $property.Value
    }
    $Default
}

function Get-PcvBatchProfileOptionValue {
    param(
        [AllowNull()]$ProfileOptions,
        [Parameter(Mandatory)][string]$Name,
        [AllowNull()]$Default = $null
    )

    Get-PcvBatchPropertyValue -InputObject $ProfileOptions -Name $Name -Default $Default
}

function Require-PcvBatchProfileOption {
    param(
        [AllowNull()]$ProfileOptions,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Profile
    )

    $value = Get-PcvBatchProfileOptionValue -ProfileOptions $ProfileOptions -Name $Name
    if ($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)) {
        throw "PCV_BATCH_PROFILE_OPTION_REQUIRED|Batch supervisor profile '$Profile' requires profile option '$Name'."
    }
    [string]$value
}

function Get-PcvBatchProfileOptionInt {
    param(
        [AllowNull()]$ProfileOptions,
        [Parameter(Mandatory)][string]$Name,
        [int]$Default
    )

    $value = Get-PcvBatchProfileOptionValue -ProfileOptions $ProfileOptions -Name $Name
    if ($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)) {
        return $Default
    }
    [int]$value
}

function ConvertTo-PcvBatchPathRedactionMap {
    param([AllowNull()]$PathRedactions)

    $map = [ordered]@{}
    if ($null -eq $PathRedactions) {
        return $map
    }
    if ($PathRedactions -is [System.Collections.IDictionary]) {
        foreach ($key in $PathRedactions.Keys) {
            $map[[string]$key] = [string]$PathRedactions[$key]
        }
        return $map
    }

    foreach ($property in @($PathRedactions.PSObject.Properties)) {
        $map[[string]$property.Name] = [string]$property.Value
    }
    $map
}

function Resolve-PcvBatchRepoRoot {
    param([Parameter(Mandatory)][string]$RepoRoot)

    $resolved = (Resolve-Path -LiteralPath $RepoRoot -ErrorAction Stop).Path
    foreach ($relative in @('AGENTS.md', 'packaging/windows-desktop-node/README.md', 'src/DesktopNode.sln')) {
        if (-not (Test-Path -LiteralPath (Join-Path $resolved $relative))) {
            throw "PCV_BATCH_REPO_BOUNDARY|Repository boundary check failed.|Missing '$relative' under '$resolved'."
        }
    }
    $resolved
}

function Resolve-PcvBatchPath {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$BasePath
    )

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }
    [System.IO.Path]::GetFullPath((Join-Path $BasePath $Path))
}

function Test-PcvBatchSensitiveKey {
    param([AllowNull()][string]$Key)

    -not [string]::IsNullOrWhiteSpace($Key) -and $Key -match '(?i)(authorization|token|api_token|password|secret|private_key|pfx|thumbprint)'
}

function ConvertTo-PcvBatchRedactedText {
    param(
        [AllowNull()][string]$Text,
        [AllowNull()]$PathRedactions
    )

    if ($null -eq $Text) {
        return $null
    }

    $redacted = [string]$Text
    $redacted = $redacted -replace '(?i)\b(Bearer)\s+([A-Za-z0-9._~+/=-]*\.[A-Za-z0-9._~+/=-]+|[A-Za-z0-9._~+/=-]{16,})', '$1 [REDACTED]'
    $redacted = [regex]::Replace(
        $redacted,
        '(?i)(\b(?:token|api_token|password|secret|private_key|pfx|thumbprint)\b\s*[:=]\s*)(?:"[^"]*"|''[^'']*''|[^\s,;}\]]+)',
        { param($Match) $Match.Groups[1].Value + '[REDACTED]' }
    )
    $redacted = $redacted -replace '(?i)api-token(?:\.dpapi)?\.json', '[REDACTED_TOKEN_FILE]'
    $redacted = $redacted -replace '(?i)api-token\.txt', '[REDACTED_TOKEN_FILE]'

    $map = ConvertTo-PcvBatchPathRedactionMap -PathRedactions $PathRedactions
    foreach ($path in (@($map.Keys) | Sort-Object { ([string]$_).Length } -Descending)) {
        if (-not [string]::IsNullOrWhiteSpace([string]$path)) {
            $redacted = $redacted.Replace([string]$path, [string]$map[$path])
        }
    }
    $redacted
}

function ConvertTo-PcvBatchRedactedObject {
    param(
        [AllowNull()]$InputObject,
        [AllowNull()]$PathRedactions
    )

    if ($null -eq $InputObject) {
        return $null
    }
    if ($InputObject -is [string]) {
        return ConvertTo-PcvBatchRedactedText -Text $InputObject -PathRedactions $PathRedactions
    }
    if ($InputObject -is [System.Collections.IDictionary]) {
        $out = [ordered]@{}
        foreach ($key in $InputObject.Keys) {
            $out[$key] = if (Test-PcvBatchSensitiveKey -Key ([string]$key)) {
                '[REDACTED]'
            } else {
                ConvertTo-PcvBatchRedactedObject -InputObject $InputObject[$key] -PathRedactions $PathRedactions
            }
        }
        return $out
    }
    if ($InputObject -is [pscustomobject]) {
        $out = [ordered]@{}
        foreach ($property in $InputObject.PSObject.Properties) {
            $out[$property.Name] = if (Test-PcvBatchSensitiveKey -Key $property.Name) {
                '[REDACTED]'
            } else {
                ConvertTo-PcvBatchRedactedObject -InputObject $property.Value -PathRedactions $PathRedactions
            }
        }
        return $out
    }
    if ($InputObject -is [System.Collections.IEnumerable] -and $InputObject -isnot [string]) {
        $items = @()
        foreach ($item in $InputObject) {
            $items += ConvertTo-PcvBatchRedactedObject -InputObject $item -PathRedactions $PathRedactions
        }
        return $items
    }
    $InputObject
}

function Write-PcvBatchJsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value,
        [int]$Depth = 32
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force -ErrorAction Stop | Out-Null
    }
    $Value | ConvertTo-Json -Depth $Depth | Set-Content -LiteralPath $Path -Encoding UTF8 -ErrorAction Stop
}

function New-PcvBatchSupervisorStep {
    param(
        [Parameter(Mandatory)][string]$Id,
        [Parameter(Mandatory)][string]$WorkingDirectory,
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [int]$TimeoutSeconds = 1800,
        [bool]$Required = $true,
        [bool]$AllowFailure = $false,
        [bool]$RequiresAdmin = $false,
        [bool]$MutatesHost = $false,
        [int]$RetryCount = 0
    )

    [pscustomobject]([ordered]@{
        id = $Id
        working_directory = $WorkingDirectory
        file_name = $FileName
        arguments = @($Arguments)
        timeout_seconds = $TimeoutSeconds
        required = $Required
        allow_failure = $AllowFailure
        requires_admin = $RequiresAdmin
        mutates_host = $MutatesHost
        retry_count = $RetryCount
    })
}

function Get-PcvBatchNodePackageManagerCommand {
    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        $command = Get-Command -Name 'npm.cmd' -ErrorAction SilentlyContinue
        if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace([string]$command.Source)) {
            return [string]$command.Source
        }

        return 'npm.cmd'
    }

    'npm'
}

function New-PcvBatchSupervisorProfileSteps {
    param(
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$RepoRoot,
        [AllowNull()]$ProfileOptions
    )

    switch ($Profile) {
        'PackagingRegression' {
            @(
                (New-PcvBatchSupervisorStep -Id 'packaging-product-tests' -WorkingDirectory $RepoRoot -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed") -TimeoutSeconds 1800),
                (New-PcvBatchSupervisorStep -Id 'packaging-installer-tests' -WorkingDirectory $RepoRoot -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed") -TimeoutSeconds 1800),
                (New-PcvBatchSupervisorStep -Id 'public-boundary-ci-required' -WorkingDirectory $RepoRoot -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', '$env:PUBLIC_BOUNDARY_CI_CONTRACT = ''public-boundary-ci-required''; Invoke-Pester -Path $args[0] -Output Detailed', 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1') -TimeoutSeconds 900),
                (New-PcvBatchSupervisorStep -Id 'git-diff-check' -WorkingDirectory $RepoRoot -FileName 'git' -Arguments @('diff', '--check') -TimeoutSeconds 120)
            )
        }
        'WebRegression' {
            $npmCommand = Get-PcvBatchNodePackageManagerCommand
            @(
                (New-PcvBatchSupervisorStep -Id 'web-pester' -WorkingDirectory $RepoRoot -FileName 'pwsh' -Arguments @('-NoProfile', '-Command', "Invoke-Pester -Path 'web/tests' -Output Detailed") -TimeoutSeconds 600),
                (New-PcvBatchSupervisorStep -Id 'web-npm-test' -WorkingDirectory $RepoRoot -FileName $npmCommand -Arguments @('test', '--prefix', 'web') -TimeoutSeconds 600),
                (New-PcvBatchSupervisorStep -Id 'web-verify-parity' -WorkingDirectory $RepoRoot -FileName $npmCommand -Arguments @('run', 'verify:parity', '--prefix', 'web') -TimeoutSeconds 600),
                (New-PcvBatchSupervisorStep -Id 'web-node-check' -WorkingDirectory $RepoRoot -FileName 'node' -Arguments @('--check', 'web/app.js') -TimeoutSeconds 120)
            )
        }
        'ServiceMsiHyperVAdminSmoke' {
            $version = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'version' -Profile $Profile
            $isoPath = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'iso_path' -Profile $Profile
            $routeArtifact = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'routeparity_artifact_root' -Profile $Profile) -BasePath $RepoRoot
            $timeoutSeconds = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'timeout_seconds' -Default 3600
            $retryCount = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'service_msi_hyperv_retry_count' -Default 1
            $arguments = @(
                '-NoProfile',
                '-ExecutionPolicy',
                'Bypass',
                '-File',
                'packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1',
                '-Version',
                $version,
                '-IsoPath',
                $isoPath,
                '-ArtifactRoot',
                $routeArtifact
            )
            $batchEvidenceRoot = Get-PcvBatchProfileOptionValue -ProfileOptions $ProfileOptions -Name 'batch_evidence_root'
            if ($null -ne $batchEvidenceRoot -and -not [string]::IsNullOrWhiteSpace([string]$batchEvidenceRoot)) {
                $arguments += @('-BatchEvidenceRoot', (Resolve-PcvBatchPath -Path ([string]$batchEvidenceRoot) -BasePath $RepoRoot))
            }
            @(
                (New-PcvBatchSupervisorStep `
                    -Id 'service-msi-hyperv-admin-smoke' `
                    -WorkingDirectory $RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments $arguments `
                    -TimeoutSeconds $timeoutSeconds `
                    -RequiresAdmin $true `
                    -MutatesHost $true `
                    -RetryCount $retryCount)
            )
        }
        'OsMutationGate' {
            $version = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'version' -Profile $Profile
            $routeArtifact = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'routeparity_artifact_root' -Profile $Profile) -BasePath $RepoRoot
            $osArtifact = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'os_gate_artifact_root' -Profile $Profile) -BasePath $RepoRoot
            $lanPrefix = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'lan_prefix' -Profile $Profile
            $timeoutSeconds = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'timeout_seconds' -Default 1800
            $retryCount = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'os_gate_retry_count' -Default 0
            $arguments = @(
                '-NoProfile',
                '-ExecutionPolicy',
                'Bypass',
                '-File',
                'packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1',
                '-Version',
                $version,
                '-RouteParityArtifactRoot',
                $routeArtifact,
                '-ArtifactRoot',
                $osArtifact,
                '-LanPrefix',
                $lanPrefix
            )
            $productRoot = Get-PcvBatchProfileOptionValue -ProfileOptions $ProfileOptions -Name 'product_root'
            if ($null -ne $productRoot -and -not [string]::IsNullOrWhiteSpace([string]$productRoot)) {
                $arguments += @('-ProductRoot', [string]$productRoot)
            }
            $dataRoot = Get-PcvBatchProfileOptionValue -ProfileOptions $ProfileOptions -Name 'data_root'
            if ($null -ne $dataRoot -and -not [string]::IsNullOrWhiteSpace([string]$dataRoot)) {
                $arguments += @('-DataRoot', [string]$dataRoot)
            }
            @(
                (New-PcvBatchSupervisorStep `
                    -Id 'os-mutation-gate' `
                    -WorkingDirectory $RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments $arguments `
                    -TimeoutSeconds $timeoutSeconds `
                    -RequiresAdmin $true `
                    -MutatesHost $true `
                    -RetryCount $retryCount)
            )
        }
        'ManualAdminCampaignDescriptor' {
            $descriptorArtifact = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'descriptor_artifact_root' -Profile $Profile) -BasePath $RepoRoot
            $campaignArtifact = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'campaign_artifact_root' -Profile $Profile) -BasePath $RepoRoot
            $baselineVersion = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'baseline_version' -Profile $Profile
            $targetVersion = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'target_version' -Profile $Profile
            $readinessSummary = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'readiness_summary_path' -Profile $Profile) -BasePath $RepoRoot
            $productUpdateSummary = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'product_update_summary_path' -Profile $Profile) -BasePath $RepoRoot
            $productRollbackSummary = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'product_rollback_summary_path' -Profile $Profile) -BasePath $RepoRoot
            $cleanHostSummary = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'clean_host_summary_path' -Profile $Profile) -BasePath $RepoRoot
            $burnLifecycleSummary = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'burn_lifecycle_summary_path' -Profile $Profile) -BasePath $RepoRoot
            $msixLifecycleSummary = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'msix_lifecycle_summary_path' -Profile $Profile) -BasePath $RepoRoot
            $installedRuntimeOpsSummary = Resolve-PcvBatchPath -Path (Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'installed_runtime_ops_summary_path' -Profile $Profile) -BasePath $RepoRoot
            $timeoutSeconds = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'timeout_seconds' -Default 300
            @(
                (New-PcvBatchSupervisorStep `
                    -Id 'manual-admin-campaign-descriptor' `
                    -WorkingDirectory $RepoRoot `
                    -FileName 'pwsh' `
                    -Arguments @(
                        '-NoProfile',
                        '-ExecutionPolicy',
                        'Bypass',
                        '-File',
                        'packaging/windows-desktop-node/tools/New-PcvManualAdminCampaignDescriptor.ps1',
                        '-ArtifactRoot',
                        $descriptorArtifact,
                        '-CampaignArtifactRoot',
                        $campaignArtifact,
                        '-BaselineVersion',
                        $baselineVersion,
                        '-TargetVersion',
                        $targetVersion,
                        '-ReadinessSummaryPath',
                        $readinessSummary,
                        '-ProductUpdateSummaryPath',
                        $productUpdateSummary,
                        '-ProductRollbackSummaryPath',
                        $productRollbackSummary,
                        '-CleanHostSummaryPath',
                        $cleanHostSummary,
                        '-BurnLifecycleSummaryPath',
                        $burnLifecycleSummary,
                        '-MsixLifecycleSummaryPath',
                        $msixLifecycleSummary,
                        '-InstalledRuntimeOpsSummaryPath',
                        $installedRuntimeOpsSummary,
                        '-DescriptorBatchId',
                        $BatchId,
                        '-PlanOnly'
                    ) `
                    -TimeoutSeconds $timeoutSeconds)
            )
        }
        'FullAdminHostMutationGate' {
            $serviceOptions = [ordered]@{
                version = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'version' -Profile $Profile
                iso_path = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'iso_path' -Profile $Profile
                routeparity_artifact_root = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'routeparity_artifact_root' -Profile $Profile
                timeout_seconds = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'timeout_seconds_routeparity' -Default 3600
                service_msi_hyperv_retry_count = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'service_msi_hyperv_retry_count' -Default 1
            }
            $batchEvidenceRoot = Get-PcvBatchProfileOptionValue -ProfileOptions $ProfileOptions -Name 'batch_evidence_root'
            if ($null -ne $batchEvidenceRoot -and -not [string]::IsNullOrWhiteSpace([string]$batchEvidenceRoot)) {
                $serviceOptions['batch_evidence_root'] = [string]$batchEvidenceRoot
            }
            $osOptions = [ordered]@{
                version = $serviceOptions.version
                routeparity_artifact_root = $serviceOptions.routeparity_artifact_root
                os_gate_artifact_root = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'os_gate_artifact_root' -Profile $Profile
                lan_prefix = Require-PcvBatchProfileOption -ProfileOptions $ProfileOptions -Name 'lan_prefix' -Profile $Profile
                timeout_seconds = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'timeout_seconds_os_gate' -Default 1800
                os_gate_retry_count = Get-PcvBatchProfileOptionInt -ProfileOptions $ProfileOptions -Name 'os_gate_retry_count' -Default 0
            }
            foreach ($optionalName in @('product_root', 'data_root')) {
                $optionalValue = Get-PcvBatchProfileOptionValue -ProfileOptions $ProfileOptions -Name $optionalName
                if ($null -ne $optionalValue -and -not [string]::IsNullOrWhiteSpace([string]$optionalValue)) {
                    $osOptions[$optionalName] = [string]$optionalValue
                }
            }
            @(
                (New-PcvBatchSupervisorProfileSteps -Profile ServiceMsiHyperVAdminSmoke -RepoRoot $RepoRoot -ProfileOptions $serviceOptions),
                (New-PcvBatchSupervisorProfileSteps -Profile OsMutationGate -RepoRoot $RepoRoot -ProfileOptions $osOptions)
            )
        }
        default {
            throw "PCV_BATCH_PROFILE_UNKNOWN|Unknown batch supervisor profile '$Profile'.|Allowed profiles: PackagingRegression, WebRegression, ServiceMsiHyperVAdminSmoke, OsMutationGate, ManualAdminCampaignDescriptor, FullAdminHostMutationGate."
        }
    }
}

function New-PcvBatchSupervisorManifest {
    param(
        [Parameter(Mandatory)][string]$BatchId,
        [Parameter(Mandatory)][string]$RepoRoot,
        [Parameter(Mandatory)][string]$ArtifactRoot,
        [string]$Profile,
        [AllowNull()]$ProfileOptions,
        [object[]]$Steps,
        [int]$HeartbeatIntervalSeconds = 5,
        [int]$GpuSnapshotIntervalSeconds = 5,
        [int]$DefaultTimeoutSeconds = 1800,
        [AllowNull()]$PathRedactions
    )

    $resolvedRepoRoot = Resolve-PcvBatchRepoRoot -RepoRoot $RepoRoot
    $resolvedArtifactRoot = Resolve-PcvBatchPath -Path $ArtifactRoot -BasePath $resolvedRepoRoot
    $pathRedactionMap = ConvertTo-PcvBatchPathRedactionMap -PathRedactions $PathRedactions
    if (-not $pathRedactionMap.Contains($resolvedRepoRoot)) {
        $pathRedactionMap[$resolvedRepoRoot] = '[REPO_ROOT]'
    }

    $resolvedSteps = @()
    if (-not [string]::IsNullOrWhiteSpace($Profile)) {
        $resolvedSteps += @(New-PcvBatchSupervisorProfileSteps -Profile $Profile -RepoRoot $resolvedRepoRoot -ProfileOptions $ProfileOptions)
    }
    if ($null -ne $Steps) {
        $resolvedSteps += @($Steps)
    }
    if (@($resolvedSteps).Count -eq 0) {
        throw "PCV_BATCH_STEPS_REQUIRED|Batch manifest requires at least one step."
    }

    $normalizedSteps = @()
    foreach ($step in $resolvedSteps) {
        $workingDirectory = [string](Get-PcvBatchPropertyValue -InputObject $step -Name 'working_directory' -Default $resolvedRepoRoot)
        $normalizedSteps += [pscustomobject]([ordered]@{
            id = [string](Get-PcvBatchPropertyValue -InputObject $step -Name 'id')
            working_directory = Resolve-PcvBatchPath -Path $workingDirectory -BasePath $resolvedRepoRoot
            file_name = [string](Get-PcvBatchPropertyValue -InputObject $step -Name 'file_name')
            arguments = @((Get-PcvBatchPropertyValue -InputObject $step -Name 'arguments' -Default @()) | ForEach-Object { [string]$_ })
            timeout_seconds = [int](Get-PcvBatchPropertyValue -InputObject $step -Name 'timeout_seconds' -Default $DefaultTimeoutSeconds)
            required = [bool](Get-PcvBatchPropertyValue -InputObject $step -Name 'required' -Default $true)
            allow_failure = [bool](Get-PcvBatchPropertyValue -InputObject $step -Name 'allow_failure' -Default $false)
            requires_admin = [bool](Get-PcvBatchPropertyValue -InputObject $step -Name 'requires_admin' -Default $false)
            mutates_host = [bool](Get-PcvBatchPropertyValue -InputObject $step -Name 'mutates_host' -Default $false)
            retry_count = [int](Get-PcvBatchPropertyValue -InputObject $step -Name 'retry_count' -Default 0)
        })
    }

    $manifest = [pscustomobject]([ordered]@{
        schema_version = 1
        batch_id = $BatchId
        created_by = $env:USERNAME
        repo_root = $resolvedRepoRoot
        artifact_root = $resolvedArtifactRoot
        heartbeat_interval_seconds = [Math]::Max(1, $HeartbeatIntervalSeconds)
        gpu_snapshot_interval_seconds = [Math]::Max(1, $GpuSnapshotIntervalSeconds)
        default_timeout_seconds = [Math]::Max(1, $DefaultTimeoutSeconds)
        path_redactions = [pscustomobject]$pathRedactionMap
        steps = @($normalizedSteps)
    })
    Assert-PcvBatchManifestValid -Manifest $manifest
    $manifest
}

function Assert-PcvBatchManifestValid {
    param([Parameter(Mandatory)]$Manifest)

    if ([int](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'schema_version' -Default 0) -ne 1) {
        throw "PCV_BATCH_SCHEMA_VERSION_UNSUPPORTED|Only batch manifest schema_version=1 is supported."
    }
    if ([string]::IsNullOrWhiteSpace([string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'batch_id'))) {
        throw "PCV_BATCH_ID_REQUIRED|Batch manifest requires batch_id."
    }
    if ([string]::IsNullOrWhiteSpace([string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'artifact_root'))) {
        throw "PCV_BATCH_ARTIFACT_ROOT_REQUIRED|Batch manifest requires artifact_root."
    }
    $steps = @(Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'steps' -Default @())
    if ($steps.Count -eq 0) {
        throw "PCV_BATCH_STEPS_REQUIRED|Batch manifest requires at least one step."
    }
    foreach ($step in $steps) {
        foreach ($field in @('id', 'working_directory', 'file_name')) {
            if ([string]::IsNullOrWhiteSpace([string](Get-PcvBatchPropertyValue -InputObject $step -Name $field))) {
                throw "PCV_BATCH_STEP_FIELD_REQUIRED|Batch step requires '$field'."
            }
        }
        $arguments = @(Get-PcvBatchPropertyValue -InputObject $step -Name 'arguments' -Default @())
        if ($arguments.Count -eq 0) {
            throw "PCV_BATCH_STEP_ARGUMENTS_REQUIRED|Batch step requires arguments."
        }
    }
}

function Save-PcvBatchSupervisorManifest {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)][string]$Path
    )

    Write-PcvBatchJsonFile -Path $Path -Value $Manifest
    [pscustomobject]([ordered]@{ ok = $true; path = [System.IO.Path]::GetFullPath($Path) })
}

function Test-PcvBatchRebootForbiddenCommand {
    param([Parameter(Mandatory)]$Step)

    $fileName = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'file_name')
    $arguments = @((Get-PcvBatchPropertyValue -InputObject $Step -Name 'arguments' -Default @()) | ForEach-Object { [string]$_ })
    $commandText = ($fileName + ' ' + ($arguments -join ' '))

    if ($commandText -match '(?i)\bRestart-Computer\b|\bStop-Computer\b') {
        return $true
    }
    if ($fileName -match '(?i)(^|[\\/])shutdown(?:\.exe)?$') {
        return $true
    }
    if ($fileName -match '(?i)(^|[\\/])schtasks(?:\.exe)?$') {
        return $true
    }
    $false
}

function Assert-PcvBatchExecutionAllowed {
    param(
        [Parameter(Mandatory)]$Manifest,
        [switch]$AllowHostMutation,
        [bool]$IsAdministrator = $false
    )

    Assert-PcvBatchManifestValid -Manifest $Manifest
    foreach ($step in @(Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'steps' -Default @())) {
        if (Test-PcvBatchRebootForbiddenCommand -Step $step) {
            throw "PCV_BATCH_REBOOT_COMMAND_FORBIDDEN|Automatic reboot or scheduled-task commands are forbidden in Batch Supervisor v1.|Step '$([string](Get-PcvBatchPropertyValue -InputObject $step -Name 'id'))'."
        }
    }
    foreach ($step in @(Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'steps' -Default @())) {
        $requiresAdmin = [bool](Get-PcvBatchPropertyValue -InputObject $step -Name 'requires_admin' -Default $false)
        $mutatesHost = [bool](Get-PcvBatchPropertyValue -InputObject $step -Name 'mutates_host' -Default $false)
        if (($requiresAdmin -or $mutatesHost) -and -not $AllowHostMutation) {
            throw "PCV_BATCH_HOST_MUTATION_APPROVAL_REQUIRED|Host-mutating batch steps require explicit -AllowHostMutation.|Step '$([string](Get-PcvBatchPropertyValue -InputObject $step -Name 'id'))'."
        }
        if ($requiresAdmin -and -not $IsAdministrator) {
            throw "PCV_BATCH_ADMIN_REQUIRED|Batch step requires an elevated shell.|Step '$([string](Get-PcvBatchPropertyValue -InputObject $step -Name 'id'))'."
        }
    }
}

function Test-PcvBatchIsAdministrator {
    try {
        $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = [Security.Principal.WindowsPrincipal]::new($identity)
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    catch {
        return $false
    }
}

function Get-PcvBatchCommandFingerprint {
    param([Parameter(Mandatory)]$Step)

    $payload = [ordered]@{
        id = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'id')
        working_directory = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'working_directory')
        file_name = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'file_name')
        arguments = @((Get-PcvBatchPropertyValue -InputObject $Step -Name 'arguments' -Default @()) | ForEach-Object { [string]$_ })
    } | ConvertTo-Json -Compress -Depth 16
    $hash = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($payload))
    -join ($hash | ForEach-Object { $_.ToString('x2') })
}

function Add-PcvBatchHeartbeat {
    param(
        [Parameter(Mandatory)][string]$HeartbeatPath,
        [Parameter(Mandatory)][string]$BatchId,
        [Parameter(Mandatory)][string]$StepId,
        [Parameter(Mandatory)][string]$Status,
        [AllowNull()][string]$Detail
    )

    $line = [ordered]@{
        ts = (Get-Date).ToUniversalTime().ToString('o')
        batch_id = $BatchId
        step_id = $StepId
        status = $Status
        detail = $Detail
    } | ConvertTo-Json -Compress
    Add-Content -LiteralPath $HeartbeatPath -Value $line -Encoding UTF8 -ErrorAction Stop
}

function Get-PcvBatchGpuCounterName {
    param([Parameter(Mandatory)][string]$Path)

    $parts = $Path -split '\\'
    [string]($parts[-1])
}

function ConvertTo-PcvBatchGpuCounterSample {
    param(
        [Parameter(Mandatory)]$Sample,
        [Parameter(Mandatory)][string]$CounterSet,
        [AllowNull()]$ProcessNamesById
    )

    $path = [string]$Sample.Path
    $instanceName = [string]$Sample.InstanceName
    $bytes = [int64][Math]::Round([double]$Sample.CookedValue)
    $processId = $null
    $processName = $null
    $processMatch = [regex]::Match($instanceName, '(?i)(?:^|_)pid_(\d+)(?:_|$)')
    if ($processMatch.Success) {
        $processId = [int]$processMatch.Groups[1].Value
        if ($null -ne $ProcessNamesById -and $ProcessNamesById.ContainsKey($processId)) {
            $processName = [string]$ProcessNamesById[$processId]
        }
    }

    [pscustomobject]([ordered]@{
        counter_set = $CounterSet
        counter = Get-PcvBatchGpuCounterName -Path $path
        instance = $instanceName
        process_id = $processId
        process_name = $processName
        bytes = $bytes
        mib = [Math]::Round(($bytes / 1MB), 2)
        path = $path
    })
}

function Get-PcvBatchGpuCounterSnapshot {
    param(
        [Parameter(Mandatory)][string]$BatchId,
        [Parameter(Mandatory)][string]$StepId,
        [Parameter(Mandatory)][int]$Ordinal,
        [Parameter(Mandatory)][int]$Attempt,
        [Parameter(Mandatory)][int]$IntervalSeconds,
        [AllowNull()]$PathRedactions
    )

    $snapshot = [ordered]@{
        schema_version = 1
        ts = (Get-Date).ToUniversalTime().ToString('o')
        batch_id = $BatchId
        step_id = $StepId
        ordinal = $Ordinal
        attempt = $Attempt
        interval_seconds = $IntervalSeconds
        status = 'unavailable'
        adapter_memory = @()
        process_memory = @()
        error = $null
    }

    if ($null -eq (Get-Command -Name Get-Counter -ErrorAction SilentlyContinue)) {
        $snapshot.error = 'Get-Counter is not available in this PowerShell session.'
        return [pscustomobject]$snapshot
    }

    $processNamesById = @{}
    try {
        foreach ($process in @(Get-Process -ErrorAction SilentlyContinue)) {
            $processNamesById[[int]$process.Id] = [string]$process.ProcessName
        }
    }
    catch {
        $processNamesById = @{}
    }

    $adapterCounters = @(
        '\GPU Adapter Memory(*)\Dedicated Usage',
        '\GPU Adapter Memory(*)\Shared Usage',
        '\GPU Adapter Memory(*)\Total Committed'
    )
    $processCounters = @(
        '\GPU Process Memory(*)\Dedicated Usage',
        '\GPU Process Memory(*)\Shared Usage',
        '\GPU Process Memory(*)\Local Usage',
        '\GPU Process Memory(*)\Non Local Usage'
    )

    try {
        $counterResult = Get-Counter -Counter @($adapterCounters + $processCounters) -ErrorAction Stop
        $adapterSamples = @()
        $processSamples = @()
        foreach ($sample in @($counterResult.CounterSamples)) {
            $path = [string]$sample.Path
            if ($path -match '(?i)\\gpu adapter memory\(') {
                $adapterSamples += ConvertTo-PcvBatchGpuCounterSample -Sample $sample -CounterSet 'GPU Adapter Memory' -ProcessNamesById $processNamesById
            }
            elseif ($path -match '(?i)\\gpu process memory\(') {
                $processSamples += ConvertTo-PcvBatchGpuCounterSample -Sample $sample -CounterSet 'GPU Process Memory' -ProcessNamesById $processNamesById
            }
        }
        $snapshot.status = 'collected'
        $snapshot.adapter_memory = @($adapterSamples)
        $snapshot.process_memory = @($processSamples)
        return [pscustomobject]$snapshot
    }
    catch {
        $snapshot.error = ConvertTo-PcvBatchRedactedText -Text ([string]$_) -PathRedactions $PathRedactions
        return [pscustomobject]$snapshot
    }
}

function Add-PcvBatchGpuSnapshot {
    param(
        [Parameter(Mandatory)][string]$SnapshotPath,
        [Parameter(Mandatory)][string]$BatchId,
        [Parameter(Mandatory)][string]$StepId,
        [Parameter(Mandatory)][int]$Ordinal,
        [Parameter(Mandatory)][int]$Attempt,
        [Parameter(Mandatory)][int]$IntervalSeconds,
        [AllowNull()]$PathRedactions
    )

    $snapshot = Get-PcvBatchGpuCounterSnapshot `
        -BatchId $BatchId `
        -StepId $StepId `
        -Ordinal $Ordinal `
        -Attempt $Attempt `
        -IntervalSeconds $IntervalSeconds `
        -PathRedactions $PathRedactions
    $line = $snapshot | ConvertTo-Json -Compress -Depth 32
    Add-Content -LiteralPath $SnapshotPath -Value $line -Encoding UTF8 -ErrorAction Stop
}

function Invoke-PcvBatchStepAttemptProcess {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)]$Step,
        [Parameter(Mandatory)][int]$Ordinal,
        [Parameter(Mandatory)][int]$Attempt,
        [Parameter(Mandatory)][string]$ResultPath,
        [scriptblock]$ProcessFactory = {
            param($StartInfo)
            $createdProcess = [System.Diagnostics.Process]::new()
            $createdProcess.StartInfo = $StartInfo
            $createdProcess
        },
        [scriptblock]$NowProvider = { Get-Date },
        [scriptblock]$WaitAction = {
            param([int]$Milliseconds)
            Start-Sleep -Milliseconds $Milliseconds
        }
    )

    $artifactRoot = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'artifact_root')
    $pathRedactions = Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'path_redactions'
    $batchId = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'batch_id')
    $heartbeatPath = Join-Path $artifactRoot 'heartbeat.jsonl'
    $currentStepPath = Join-Path $artifactRoot 'current-step.json'
    $gpuSnapshotPath = Join-Path $artifactRoot 'gpu-snapshots.jsonl'

    $stepId = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'id')
    $defaultTimeout = [int](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'default_timeout_seconds' -Default 1800)
    $stepTimeout = [int](Get-PcvBatchPropertyValue -InputObject $Step -Name 'timeout_seconds' -Default $defaultTimeout)
    $timeoutSeconds = if ($stepTimeout -gt 0) { $stepTimeout } else { $defaultTimeout }
    $heartbeatSeconds = [Math]::Max(1, [int](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'heartbeat_interval_seconds' -Default 5))
    $gpuSnapshotSeconds = [Math]::Max(1, [int](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'gpu_snapshot_interval_seconds' -Default 5))
    $fingerprint = Get-PcvBatchCommandFingerprint -Step $Step

    Write-PcvBatchJsonFile -Path $currentStepPath -Value ([ordered]@{
        ts = (Get-Date).ToUniversalTime().ToString('o')
        batch_id = $batchId
        step_id = $stepId
        ordinal = $Ordinal
        attempt = $Attempt
        status = 'running'
        timeout_seconds = $timeoutSeconds
    })
    Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $batchId -StepId $stepId -Status 'started' -Detail "attempt=$Attempt"

    $started = & $NowProvider
    $stdout = ''
    $stderr = ''
    $timedOut = $false
    $exitCode = $null
    $startFailure = $null
    $gpuSnapshotCount = 0
    $nextGpuSnapshotAt = $started.AddSeconds($gpuSnapshotSeconds)

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'file_name')
    $startInfo.WorkingDirectory = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'working_directory')
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    foreach ($argument in @((Get-PcvBatchPropertyValue -InputObject $Step -Name 'arguments' -Default @()))) {
        [void]$startInfo.ArgumentList.Add([string]$argument)
    }

    $process = & $ProcessFactory $startInfo
    try {
        try {
            [void]$process.Start()
            $stdoutTask = $process.StandardOutput.ReadToEndAsync()
            $stderrTask = $process.StandardError.ReadToEndAsync()
            while (-not $process.HasExited) {
                $now = & $NowProvider
                $elapsed = [int](($now - $started).TotalSeconds)
                Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $batchId -StepId $stepId -Status 'running' -Detail "attempt=$Attempt elapsed_seconds=$elapsed"
                if ($elapsed -ge $timeoutSeconds) {
                    $timedOut = $true
                    try {
                        $process.Kill($true)
                        [void]$process.WaitForExit(5000)
                    } catch {}
                    break
                }
                if ($now -ge $nextGpuSnapshotAt) {
                    Add-PcvBatchGpuSnapshot `
                        -SnapshotPath $gpuSnapshotPath `
                        -BatchId $batchId `
                        -StepId $stepId `
                        -Ordinal $Ordinal `
                        -Attempt $Attempt `
                        -IntervalSeconds $gpuSnapshotSeconds `
                        -PathRedactions $pathRedactions
                    $gpuSnapshotCount++
                    $nextGpuSnapshotAt = (& $NowProvider).AddSeconds($gpuSnapshotSeconds)
                }
                & $WaitAction ($heartbeatSeconds * 1000)
            }
            if (-not $timedOut) {
                $exitCode = $process.ExitCode
            }
            $stdout = $stdoutTask.GetAwaiter().GetResult()
            $stderr = $stderrTask.GetAwaiter().GetResult()
        }
        catch {
            $startFailure = [string]$_
            $stderr = $startFailure
            if ($null -eq $exitCode) {
                $exitCode = -1
            }
        }
    }
    finally {
        $process.Dispose()
    }

    $finished = & $NowProvider
    $ok = -not $timedOut -and $exitCode -eq 0
    $result = [pscustomobject]([ordered]@{
        schema_version = 1
        step_id = $stepId
        ordinal = $Ordinal
        attempt = $Attempt
        command_fingerprint = $fingerprint
        file_name = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'file_name')
        arguments = ConvertTo-PcvBatchRedactedObject -InputObject @((Get-PcvBatchPropertyValue -InputObject $Step -Name 'arguments' -Default @())) -PathRedactions $pathRedactions
        working_directory = ConvertTo-PcvBatchRedactedText -Text ([string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'working_directory')) -PathRedactions $pathRedactions
        started_at = $started.ToUniversalTime().ToString('o')
        finished_at = $finished.ToUniversalTime().ToString('o')
        duration_ms = [int](($finished - $started).TotalMilliseconds)
        timeout_seconds = $timeoutSeconds
        timed_out = $timedOut
        exit_code = $exitCode
        ok = $ok
        stdout = ConvertTo-PcvBatchRedactedText -Text $stdout -PathRedactions $pathRedactions
        stderr = ConvertTo-PcvBatchRedactedText -Text $stderr -PathRedactions $pathRedactions
        start_failure = ConvertTo-PcvBatchRedactedText -Text $startFailure -PathRedactions $pathRedactions
        gpu_snapshot_path = ConvertTo-PcvBatchRedactedText -Text $gpuSnapshotPath -PathRedactions $pathRedactions
        gpu_snapshot_count = $gpuSnapshotCount
    })
    Write-PcvBatchJsonFile -Path $resultPath -Value $result
    $result
}

function Invoke-PcvBatchStepProcess {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)]$Step,
        [Parameter(Mandatory)][int]$Ordinal,
        [scriptblock]$ProcessFactory = {
            param($StartInfo)
            $createdProcess = [System.Diagnostics.Process]::new()
            $createdProcess.StartInfo = $StartInfo
            $createdProcess
        },
        [scriptblock]$NowProvider = { Get-Date },
        [scriptblock]$WaitAction = {
            param([int]$Milliseconds)
            Start-Sleep -Milliseconds $Milliseconds
        }
    )

    $artifactRoot = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'artifact_root')
    $pathRedactions = Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'path_redactions'
    $batchId = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'batch_id')
    $heartbeatPath = Join-Path $artifactRoot 'heartbeat.jsonl'
    $currentStepPath = Join-Path $artifactRoot 'current-step.json'
    $stepResultsRoot = Join-Path $artifactRoot 'step-results'
    New-Item -ItemType Directory -Path $stepResultsRoot -Force -ErrorAction Stop | Out-Null

    $stepId = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'id')
    $safeId = $stepId -replace '[^A-Za-z0-9._-]', '-'
    $resultPath = Join-Path $stepResultsRoot ('{0:D3}-{1}.json' -f $Ordinal, $safeId)
    $retryCount = [Math]::Max(0, [int](Get-PcvBatchPropertyValue -InputObject $Step -Name 'retry_count' -Default 0))
    $maxAttempts = $retryCount + 1
    $attempts = New-Object System.Collections.Generic.List[object]
    $lastResult = $null

    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        $attemptPath = Join-Path $stepResultsRoot ('{0:D3}-{1}.attempt-{2:D2}.json' -f $Ordinal, $safeId, $attempt)
        $lastResult = Invoke-PcvBatchStepAttemptProcess `
            -Manifest $Manifest `
            -Step $Step `
            -Ordinal $Ordinal `
            -Attempt $attempt `
            -ResultPath $attemptPath `
            -ProcessFactory $ProcessFactory `
            -NowProvider $NowProvider `
            -WaitAction $WaitAction
        $attempts.Add($lastResult) | Out-Null

        if ([bool]$lastResult.ok -or $attempt -ge $maxAttempts) {
            break
        }

        Write-PcvBatchJsonFile -Path $currentStepPath -Value ([ordered]@{
            ts = (Get-Date).ToUniversalTime().ToString('o')
            batch_id = $batchId
            step_id = $stepId
            ordinal = $Ordinal
            attempt = $attempt
            status = 'retrying'
            retry_count = $retryCount
            next_attempt = $attempt + 1
            result = (ConvertTo-PcvBatchRedactedText -Text $attemptPath -PathRedactions $pathRedactions)
        })
        Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $batchId -StepId $stepId -Status 'retrying' -Detail "attempt=$attempt result=$attemptPath next_attempt=$($attempt + 1)"
    }

    $finalAttemptCount = @($attempts.ToArray()).Count
    $attemptItems = @($attempts.ToArray())
    $aggregate = [ordered]@{}
    foreach ($property in $lastResult.PSObject.Properties) {
        $aggregate[$property.Name] = $property.Value
    }
    $aggregate['retry_count'] = $retryCount
    $aggregate['attempt_count'] = $finalAttemptCount
    $aggregate['attempts'] = $attemptItems
    $aggregate['final_attempt'] = [int]$lastResult.attempt

    $result = [pscustomobject]$aggregate
    Write-PcvBatchJsonFile -Path $resultPath -Value $result
    $finalHeartbeatStatus = if ([bool]$result.ok) { 'completed' } else { 'failed' }
    Write-PcvBatchJsonFile -Path $currentStepPath -Value ([ordered]@{
        ts = (Get-Date).ToUniversalTime().ToString('o')
        batch_id = $batchId
        step_id = $stepId
        ordinal = $Ordinal
        attempt = [int]$result.final_attempt
        status = $finalHeartbeatStatus
        result = (ConvertTo-PcvBatchRedactedText -Text $resultPath -PathRedactions $pathRedactions)
    })
    Add-PcvBatchHeartbeat -HeartbeatPath $heartbeatPath -BatchId $batchId -StepId $stepId -Status $finalHeartbeatStatus -Detail "result=$resultPath"
    $result
}

function Get-PcvBatchPriorSuccessfulResult {
    param(
        [Parameter(Mandatory)]$Manifest,
        [Parameter(Mandatory)]$Step,
        [Parameter(Mandatory)][int]$Ordinal
    )

    $artifactRoot = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'artifact_root')
    $stepId = [string](Get-PcvBatchPropertyValue -InputObject $Step -Name 'id')
    $safeId = $stepId -replace '[^A-Za-z0-9._-]', '-'
    $path = Join-Path (Join-Path $artifactRoot 'step-results') ('{0:D3}-{1}.json' -f $Ordinal, $safeId)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return $null
    }
    $prior = Get-Content -Raw -LiteralPath $path -ErrorAction Stop | ConvertFrom-Json
    $fingerprint = Get-PcvBatchCommandFingerprint -Step $Step
    if ([bool]$prior.ok -and [string]$prior.command_fingerprint -eq $fingerprint) {
        return $prior
    }
    $null
}

function Invoke-PcvBatchSupervisor {
    param(
        [Parameter(Mandatory)]$Manifest,
        [switch]$DryRun,
        [switch]$Resume,
        [switch]$AllowHostMutation,
        [AllowNull()][Nullable[bool]]$IsAdministrator = $null,
        [scriptblock]$ProcessFactory = {
            param($StartInfo)
            $createdProcess = [System.Diagnostics.Process]::new()
            $createdProcess.StartInfo = $StartInfo
            $createdProcess
        },
        [scriptblock]$NowProvider = { Get-Date },
        [scriptblock]$WaitAction = {
            param([int]$Milliseconds)
            Start-Sleep -Milliseconds $Milliseconds
        }
    )

    Assert-PcvBatchManifestValid -Manifest $Manifest
    $artifactRoot = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'artifact_root')
    $pathRedactions = Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'path_redactions'
    $batchId = [string](Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'batch_id')
    New-Item -ItemType Directory -Path $artifactRoot -Force -ErrorAction Stop | Out-Null
    # Dry-run writes its own file names so a dry-run can never overwrite (or be misread as)
    # the summary/current-step/resolved-manifest artifacts of a completed real run that shares
    # the same artifact_root.
    $summaryFileName = if ($DryRun) { 'summary.dry-run.json' } else { 'summary.json' }
    $currentStepFileName = if ($DryRun) { 'current-step.dry-run.json' } else { 'current-step.json' }
    $resolvedManifestFileName = if ($DryRun) { 'batch-manifest.resolved.dry-run.json' } else { 'batch-manifest.resolved.json' }
    Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot $currentStepFileName) -Value ([ordered]@{
        ts = (Get-Date).ToUniversalTime().ToString('o')
        batch_id = $batchId
        status = 'starting'
        dry_run = [bool]$DryRun
    })
    Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot $resolvedManifestFileName) -Value (ConvertTo-PcvBatchRedactedObject -InputObject $Manifest -PathRedactions $pathRedactions)

    $admin = if ($null -eq $IsAdministrator) { Test-PcvBatchIsAdministrator } else { [bool]$IsAdministrator }
    Assert-PcvBatchExecutionAllowed -Manifest $Manifest -AllowHostMutation:$AllowHostMutation -IsAdministrator:$admin

    $results = New-Object System.Collections.Generic.List[object]
    $skipped = New-Object System.Collections.Generic.List[string]
    $failedStep = $null
    $status = 'completed'
    $steps = @(Get-PcvBatchPropertyValue -InputObject $Manifest -Name 'steps' -Default @())

    if ($DryRun) {
        $summary = [pscustomobject]([ordered]@{
            schema_version = 1
            ok = $true
            dry_run = $true
            status = 'completed'
            batch_id = $batchId
            artifact_root = $artifactRoot
            total_steps = $steps.Count
            steps = @($steps | ForEach-Object { [pscustomobject]([ordered]@{ id = (Get-PcvBatchPropertyValue -InputObject $_ -Name 'id'); planned = $true; retry_count = [int](Get-PcvBatchPropertyValue -InputObject $_ -Name 'retry_count' -Default 0) }) })
        })
        Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot $summaryFileName) -Value (ConvertTo-PcvBatchRedactedObject -InputObject $summary -PathRedactions $pathRedactions)
        Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot $currentStepFileName) -Value ([ordered]@{ ts = (Get-Date).ToUniversalTime().ToString('o'); batch_id = $batchId; status = 'completed'; dry_run = $true })
        return $summary
    }

    $gpuSnapshotPath = Join-Path $artifactRoot 'gpu-snapshots.jsonl'
    if (-not (Test-Path -LiteralPath $gpuSnapshotPath -PathType Leaf)) {
        New-Item -ItemType File -Path $gpuSnapshotPath -Force -ErrorAction Stop | Out-Null
    }

    $ordinal = 0
    foreach ($step in $steps) {
        $ordinal++
        if ($Resume) {
            $prior = Get-PcvBatchPriorSuccessfulResult -Manifest $Manifest -Step $step -Ordinal $ordinal
            if ($null -ne $prior) {
                $skipped.Add([string](Get-PcvBatchPropertyValue -InputObject $step -Name 'id')) | Out-Null
                continue
            }
        }
        $result = Invoke-PcvBatchStepProcess `
            -Manifest $Manifest `
            -Step $step `
            -Ordinal $ordinal `
            -ProcessFactory $ProcessFactory `
            -NowProvider $NowProvider `
            -WaitAction $WaitAction
        $results.Add($result) | Out-Null
        if (-not [bool]$result.ok -and -not [bool](Get-PcvBatchPropertyValue -InputObject $step -Name 'allow_failure' -Default $false)) {
            $failedStep = [string](Get-PcvBatchPropertyValue -InputObject $step -Name 'id')
            $status = 'failed'
            break
        }
    }

    $summaryOk = $status -eq 'completed'
    $nextResumeStepId = if ($failedStep) { $failedStep } else { $null }
    $resultItems = @($results.ToArray())
    $skippedItems = @($skipped.ToArray())
    $summary = [pscustomobject]([ordered]@{
        schema_version = 1
        ok = $summaryOk
        dry_run = $false
        status = $status
        batch_id = $batchId
        artifact_root = $artifactRoot
        total_steps = $steps.Count
        executed_steps = $resultItems.Count
        skipped_steps = $skippedItems
        failed_step_id = $failedStep
        next_resume_step_id = $nextResumeStepId
        results = $resultItems
    })
    Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot $summaryFileName) -Value (ConvertTo-PcvBatchRedactedObject -InputObject $summary -PathRedactions $pathRedactions)
    Write-PcvBatchJsonFile -Path (Join-Path $artifactRoot $currentStepFileName) -Value ([ordered]@{ ts = (Get-Date).ToUniversalTime().ToString('o'); batch_id = $batchId; status = $status; failed_step_id = $failedStep })
    $summary
}

Export-ModuleMember -Function `
    Resolve-PcvBatchRepoRoot, `
    New-PcvBatchSupervisorStep, `
    New-PcvBatchSupervisorManifest, `
    New-PcvBatchSupervisorProfileSteps, `
    Save-PcvBatchSupervisorManifest, `
    Invoke-PcvBatchSupervisor, `
    ConvertTo-PcvBatchRedactedText, `
    ConvertTo-PcvBatchRedactedObject, `
    Assert-PcvBatchExecutionAllowed
