param(
    [Parameter(Mandatory)][string]$Version,
    [Parameter(Mandatory)][string]$RouteParityArtifactRoot,
    [Parameter(Mandatory)][string]$ArtifactRoot,
    [Parameter(Mandatory)][string]$LanPrefix,
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$DataRoot = (Join-Path $env:ProgramData 'PureCVisor\desktop-node'),
    [switch]$PlanOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RouteParityArtifactRoot = [System.IO.Path]::GetFullPath($RouteParityArtifactRoot)
$ArtifactRoot = [System.IO.Path]::GetFullPath($ArtifactRoot)
$ProductRoot = [System.IO.Path]::GetFullPath($ProductRoot)
$DataRoot = [System.IO.Path]::GetFullPath($DataRoot)
New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null

$progressPath = Join-Path $ArtifactRoot 'progress.json'
$hostExe = Join-Path $ProductRoot 'DesktopNode.Host.exe'
$webRoot = Join-Path $ProductRoot 'web'
$protectedTokenPath = Join-Path $DataRoot 'api-token.dpapi.json'
$jobStorePath = Join-Path $DataRoot 'jobs.json'
$eventLogPath = Join-Path $DataRoot 'events.jsonl'
$serviceName = 'PureCVisorDesktopNode'
$eventSourceName = 'PureCVisor Desktop Node'
$eventLogName = 'Application'
$firewallRuleName = 'PureCVisor Desktop Node Local API LAN'
$rootSubject = 'CN=PureCVisor Internal Code Signing Root CA'
$publisherSubject = 'CN=PureCVisor Desktop Node Internal Code Signing'
$summarySteps = New-Object System.Collections.Generic.List[object]

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value,
        [int]$Depth = 32
    )

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    $Value | ConvertTo-Json -Depth $Depth | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Save-Progress {
    Write-JsonFile -Path $progressPath -Value ([pscustomobject][ordered]@{
        schema_version = 1
        artifact_root = $ArtifactRoot
        version = $Version
        updated_at = (Get-Date).ToString('o')
        steps = $summarySteps.ToArray()
    })
}

function Add-Step {
    param(
        [Parameter(Mandatory)][string]$Name,
        [bool]$Ok = $false,
        [string]$Path,
        [string]$Status = $(if ($Ok) { 'completed' } else { 'failed' }),
        [string]$Note = ''
    )

    $summarySteps.Add([pscustomobject][ordered]@{
        name = $Name
        status = $Status
        ok = $Ok
        path = $Path
        note = $Note
        timestamp = (Get-Date).ToString('o')
    }) | Out-Null
    Save-Progress
}

function New-PlannedStep {
    param([Parameter(Mandatory)][string]$Name)

    [pscustomobject][ordered]@{
        name = $Name
        status = 'planned'
        ok = $true
        path = $null
        note = 'plan-only'
    }
}

function Get-ServiceInfo {
    $service = Get-CimInstance Win32_Service -Filter "Name='$serviceName'" -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        return $null
    }

    [pscustomobject][ordered]@{
        name = $service.Name
        state = $service.State
        start_mode = $service.StartMode
        path_name = $service.PathName
        process_id = $service.ProcessId
    }
}

function Test-IsAdministrator {
    try {
        $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = [Security.Principal.WindowsPrincipal]::new($identity)
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    catch {
        return $false
    }
}

function Assert-True {
    param(
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-CapturedProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [int]$TimeoutSeconds = 120
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    $startInfo.WorkingDirectory = $ProductRoot
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $started = Get-Date
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        [void]$process.Start()
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
        if ($timedOut) {
            try {
                $process.Kill($true)
                [void]$process.WaitForExit(5000)
            }
            catch {
            }
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        $finished = Get-Date

        [pscustomobject][ordered]@{
            file_name = $FileName
            arguments = @($Arguments)
            exit_code = if ($timedOut) { -1 } else { $process.ExitCode }
            stdout = $stdout
            stderr = $stderr
            timed_out = $timedOut
            duration_ms = [int]($finished - $started).TotalMilliseconds
            ok = -not $timedOut -and $process.ExitCode -eq 0
        }
    }
    finally {
        $process.Dispose()
    }
}

function Invoke-ServiceAction {
    param(
        [Parameter(Mandatory)][string]$StepName,
        [Parameter(Mandatory)][string]$Action,
        [string[]]$ExtraArguments = @(),
        [scriptblock]$Validate
    )

    $path = Join-Path $ArtifactRoot "$StepName.json"
    $arguments = @(
        'service-action',
        $Action,
        '--product-root',
        $ProductRoot,
        '--data-root',
        $DataRoot,
        '--service-exe',
        $hostExe
    ) + @($ExtraArguments)
    $process = Invoke-CapturedProcess -FileName $hostExe -Arguments $arguments -TimeoutSeconds 240
    $parsed = $null
    if (-not [string]::IsNullOrWhiteSpace($process.stdout)) {
        $parsed = $process.stdout | ConvertFrom-Json
    }
    $ok = if ($null -ne $Validate) {
        [bool](& $Validate $process $parsed)
    } else {
        [bool]($process.ok -and $null -ne $parsed -and [bool]$parsed.Ok)
    }
    Write-JsonFile -Path $path -Value ([pscustomobject][ordered]@{
        process = $process
        parsed = $parsed
        ok = $ok
    })
    Add-Step -Name $StepName -Ok $ok -Path $path
    Assert-True -Condition $ok -Message "PCV_OS_GATE_STEP_FAILED|OS gate step '$StepName' failed."
    $parsed
}

function Initialize-ProtectedDataSupport {
    if ('System.Security.Cryptography.ProtectedData' -as [type]) {
        return
    }
    foreach ($assemblyName in @('System.Security.Cryptography.ProtectedData', 'System.Security')) {
        try {
            Add-Type -AssemblyName $assemblyName -ErrorAction Stop
            if ('System.Security.Cryptography.ProtectedData' -as [type]) {
                return
            }
        }
        catch {
        }
    }
    throw 'PCV_OS_GATE_PROTECTED_DATA_UNAVAILABLE|DPAPI ProtectedData support is unavailable.'
}

function Get-ProtectedTokenEntropy {
    [System.Text.Encoding]::UTF8.GetBytes('PureCVisor Desktop Node API Token Store v1')
}

function Read-ProtectedToken {
    param([Parameter(Mandatory)][string]$Path)

    $json = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    Initialize-ProtectedDataSupport
    $protectedBytes = [Convert]::FromBase64String([string]$json.protected_token)
    $tokenBytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
        $protectedBytes,
        (Get-ProtectedTokenEntropy),
        [System.Security.Cryptography.DataProtectionScope]::LocalMachine)
    $token = [System.Text.Encoding]::UTF8.GetString($tokenBytes)
    if ([string]::IsNullOrWhiteSpace($token)) {
        throw 'PCV_OS_GATE_TOKEN_EMPTY|Protected token resolved to an empty value.'
    }
    $token
}

function Get-InternalCertificate {
    param(
        [Parameter(Mandatory)][string]$StorePath,
        [Parameter(Mandatory)][string]$Subject
    )

    Get-ChildItem -LiteralPath $StorePath -ErrorAction Stop |
        Where-Object { $_.Subject -eq $Subject } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1
}

function Export-InternalTrustCertificates {
    $certRoot = Join-Path $ArtifactRoot 'existing-trust-certs'
    New-Item -ItemType Directory -Path $certRoot -Force | Out-Null
    $rootCert = Get-InternalCertificate -StorePath 'Cert:\LocalMachine\Root' -Subject $rootSubject
    $publisherCert = Get-InternalCertificate -StorePath 'Cert:\LocalMachine\TrustedPublisher' -Subject $publisherSubject
    Assert-True -Condition ($null -ne $rootCert) -Message 'PCV_OS_GATE_ROOT_CERT_MISSING|Internal Root certificate was not found.'
    Assert-True -Condition ($null -ne $publisherCert) -Message 'PCV_OS_GATE_PUBLISHER_CERT_MISSING|Internal TrustedPublisher certificate was not found.'

    $rootPath = Join-Path $certRoot 'PureCVisor-existing-root.cer'
    $publisherPath = Join-Path $certRoot 'PureCVisor-existing-publisher.cer'
    Export-Certificate -Cert $rootCert -FilePath $rootPath -Force | Out-Null
    Export-Certificate -Cert $publisherCert -FilePath $publisherPath -Force | Out-Null

    [pscustomobject][ordered]@{
        root_path = $rootPath
        root_thumbprint = $rootCert.Thumbprint
        publisher_path = $publisherPath
        publisher_thumbprint = $publisherCert.Thumbprint
    }
}

function Test-EventSourcePresent {
    $path = "HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\$eventLogName\$eventSourceName"
    Test-Path -LiteralPath $path
}

function Get-FirewallRuleCount {
    try {
        @(Get-NetFirewallRule -DisplayName $firewallRuleName -ErrorAction SilentlyContinue).Count
    }
    catch {
        $null
    }
}

function Test-InternalTrustPresent {
    $root = Get-InternalCertificate -StorePath 'Cert:\LocalMachine\Root' -Subject $rootSubject
    $publisher = Get-InternalCertificate -StorePath 'Cert:\LocalMachine\TrustedPublisher' -Subject $publisherSubject
    [pscustomobject][ordered]@{
        root_present = $null -ne $root
        publisher_present = $null -ne $publisher
        root_thumbprint = if ($null -ne $root) { $root.Thumbprint } else { $null }
        publisher_thumbprint = if ($null -ne $publisher) { $publisher.Thumbprint } else { $null }
    }
}

function Invoke-LanListenerSmoke {
    $path = Join-Path $ArtifactRoot 'lan-listener-ip-smoke.json'
    $token = Read-ProtectedToken -Path $protectedTokenPath
    $arguments = @(
        'listen',
        '--prefix',
        $LanPrefix,
        '--web-root',
        $webRoot,
        '--job-store',
        $jobStorePath,
        '--event-log',
        $eventLogPath,
        '--api-token-protected-file',
        $protectedTokenPath,
        '--allow-lan'
    )
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $hostExe
    $startInfo.WorkingDirectory = $ProductRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    foreach ($argument in $arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $checks = @()
    try {
        [void]$process.Start()
        $headers = @{ Authorization = "Bearer $token" }
        foreach ($probePath in @('/api/v1/runtime/policy', '/', '/index.html', '/app.js')) {
            $uri = $LanPrefix.TrimEnd('/') + $probePath
            $deadline = (Get-Date).AddSeconds(30)
            $status = $null
            $errorMessage = $null
            do {
                try {
                    $response = Invoke-WebRequest -Uri $uri -Headers $headers -TimeoutSec 5
                    $status = [int]$response.StatusCode
                    $errorMessage = $null
                    break
                }
                catch {
                    $errorMessage = [string]$_
                    Start-Sleep -Seconds 1
                }
            } while ((Get-Date) -lt $deadline)
            $checks += [pscustomobject][ordered]@{
                path = $probePath
                status = $status
                ok = [bool]($status -eq 200)
                error = $errorMessage
            }
        }
    }
    finally {
        if (-not $process.HasExited) {
            try {
                $process.Kill($true)
                [void]$process.WaitForExit(5000)
            }
            catch {
            }
        }
        $process.Dispose()
    }

    $ok = @($checks | Where-Object { -not $_.ok }).Count -eq 0
    Write-JsonFile -Path $path -Value ([pscustomobject][ordered]@{
        ok = $ok
        selected = [pscustomobject][ordered]@{
            prefix = $LanPrefix
            auth = 'bearer-protected-token-file'
            non_loopback_static_auth = 'bearer-required'
            checks = @($checks)
        }
        token_length = $token.Length
        token_redacted = $true
    })
    Add-Step -Name 'lan-listener-ip-smoke' -Ok $ok -Path $path
    Assert-True -Condition $ok -Message 'PCV_OS_GATE_LAN_PROBE_FAILED|LAN listener probe failed.'
}

function New-CommandPlan {
    $serviceActionBase = @(
        '--product-root',
        $ProductRoot,
        '--data-root',
        $DataRoot,
        '--service-exe',
        $hostExe
    )
    [pscustomobject][ordered]@{
        service_actions = @(
            [pscustomobject][ordered]@{ name = 'config-migration-apply-service-running'; file_name = $hostExe; arguments = @('service-action', 'config-migration-apply') + $serviceActionBase },
            [pscustomobject][ordered]@{ name = 'eventlog-register'; file_name = $hostExe; arguments = @('service-action', 'eventlog-register') + $serviceActionBase },
            [pscustomobject][ordered]@{ name = 'eventlog-remove'; file_name = $hostExe; arguments = @('service-action', 'eventlog-remove') + $serviceActionBase },
            [pscustomobject][ordered]@{ name = 'firewall-enable'; file_name = $hostExe; arguments = @('service-action', 'firewall-enable') + $serviceActionBase + @('--allow-lan') },
            [pscustomobject][ordered]@{ name = 'firewall-remove'; file_name = $hostExe; arguments = @('service-action', 'firewall-remove') + $serviceActionBase },
            [pscustomobject][ordered]@{ name = 'trust-store-install-existing'; file_name = $hostExe; arguments = @('service-action', 'trust-store-install') + $serviceActionBase + @('--release-approved') },
            [pscustomobject][ordered]@{ name = 'trust-store-remove-existing'; file_name = $hostExe; arguments = @('service-action', 'trust-store-remove') + $serviceActionBase + @('--release-approved') },
            [pscustomobject][ordered]@{ name = 'trust-store-restore-existing'; file_name = $hostExe; arguments = @('service-action', 'trust-store-install') + $serviceActionBase + @('--release-approved') }
        )
        lan_listener = [pscustomobject][ordered]@{
            file_name = $hostExe
            arguments = @(
                'listen',
                '--prefix',
                $LanPrefix,
                '--web-root',
                $webRoot,
                '--job-store',
                $jobStorePath,
                '--event-log',
                $eventLogPath,
                '--api-token-protected-file',
                $protectedTokenPath,
                '--allow-lan'
            )
        }
        lan_probes = @(
            [pscustomobject][ordered]@{ path = '/api/v1/runtime/policy'; auth = 'bearer-required' },
            [pscustomobject][ordered]@{ path = '/'; auth = 'bearer-required' },
            [pscustomobject][ordered]@{ path = '/index.html'; auth = 'bearer-required' },
            [pscustomobject][ordered]@{ path = '/app.js'; auth = 'bearer-required' }
        )
    }
}

$plannedStepNames = @(
    'preflight',
    'config-migration-apply-service-running',
    'eventlog-register',
    'eventlog-remove',
    'firewall-enable',
    'lan-listener-ip-smoke',
    'firewall-remove',
    'export-existing-internal-trust-certs',
    'trust-store-install-existing',
    'trust-store-remove-existing',
    'trust-store-restore-existing'
)

if ($PlanOnly) {
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $true
        plan_only = $true
        actual_execution = 'not-run'
        mutates_host = $false
        host_mutation_performed = $false
        artifact_root = $ArtifactRoot
        version = $Version
        routeparity_artifact = $RouteParityArtifactRoot
        lan_prefix = $LanPrefix
        product_root = $ProductRoot
        data_root = $DataRoot
        public_trusted_signing = 'excluded'
        external_stable_publication = 'not-claimed'
        command_plan = New-CommandPlan
        steps = @($plannedStepNames | ForEach-Object { New-PlannedStep -Name $_ })
    }
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-JsonFile -Path $progressPath -Value $summary
    Write-Output $ArtifactRoot
    exit 0
}

$bootTimeBefore = $null
$firewallEnabled = $false
$trustRemoved = $false
$trustExport = $null

try {
    $bootTimeBefore = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $preflightPath = Join-Path $ArtifactRoot 'preflight.json'
    $routeSummaryPath = Join-Path $RouteParityArtifactRoot 'summary.json'
    $routeSummary = if (Test-Path -LiteralPath $routeSummaryPath -PathType Leaf) {
        Get-Content -Raw -LiteralPath $routeSummaryPath | ConvertFrom-Json
    } else {
        $null
    }
    $serviceBefore = Get-ServiceInfo
    $preflight = [pscustomobject][ordered]@{
        admin = Test-IsAdministrator
        artifact_root = $ArtifactRoot
        version = $Version
        routeparity_artifact = $RouteParityArtifactRoot
        routeparity_summary_path = $routeSummaryPath
        routeparity_ok = if ($null -ne $routeSummary) { [bool]$routeSummary.ok } else { $false }
        host_exe = $hostExe
        host_exe_exists = Test-Path -LiteralPath $hostExe -PathType Leaf
        protected_token_path = $protectedTokenPath
        protected_token_exists = Test-Path -LiteralPath $protectedTokenPath -PathType Leaf
        service_before = $serviceBefore
        boot_time_before = $bootTimeBefore
    }
    Write-JsonFile -Path $preflightPath -Value $preflight
    $preflightOk = [bool](
        $preflight.admin -and
        $preflight.routeparity_ok -and
        $preflight.host_exe_exists -and
        $preflight.protected_token_exists -and
        $null -ne $serviceBefore -and
        [string]$serviceBefore.state -eq 'Running')
    Add-Step -Name 'preflight' -Ok $preflightOk -Path $preflightPath
    Assert-True -Condition $preflightOk -Message 'PCV_OS_GATE_PREFLIGHT_FAILED|OS mutation gate preflight failed.'

    Invoke-ServiceAction `
        -StepName 'config-migration-apply-service-running' `
        -Action 'config-migration-apply' `
        -Validate {
            param($Process, $Parsed)
            $null -ne $Parsed -and
                [string]$Parsed.ErrorCode -eq 'PCV_CONFIG_MIGRATION_SERVICE_RUNNING' -and
                $null -ne $Parsed.ConfigMigration -and
                -not [bool]$Parsed.ConfigMigration.MutationPlanned -and
                -not [bool]$Parsed.ConfigMigration.MutationPerformed
        } | Out-Null

    Invoke-ServiceAction -StepName 'eventlog-register' -Action 'eventlog-register' | Out-Null
    Assert-True -Condition (Test-EventSourcePresent) -Message 'PCV_OS_GATE_EVENT_SOURCE_NOT_REGISTERED|Event source was not registered.'
    Invoke-ServiceAction -StepName 'eventlog-remove' -Action 'eventlog-remove' | Out-Null
    Assert-True -Condition (-not (Test-EventSourcePresent)) -Message 'PCV_OS_GATE_EVENT_SOURCE_STILL_PRESENT|Event source was not removed.'

    Invoke-ServiceAction -StepName 'firewall-enable' -Action 'firewall-enable' -ExtraArguments @('--allow-lan') | Out-Null
    $firewallEnabled = $true
    Invoke-LanListenerSmoke
    Invoke-ServiceAction -StepName 'firewall-remove' -Action 'firewall-remove' | Out-Null
    $firewallEnabled = $false

    $exportPath = Join-Path $ArtifactRoot 'export-existing-internal-trust-certs.json'
    $trustExport = Export-InternalTrustCertificates
    Write-JsonFile -Path $exportPath -Value $trustExport
    Add-Step -Name 'export-existing-internal-trust-certs' -Ok $true -Path $exportPath

    Invoke-ServiceAction `
        -StepName 'trust-store-install-existing' `
        -Action 'trust-store-install' `
        -ExtraArguments @(
            '--release-approved',
            '--trust-root-certificate',
            $trustExport.root_path,
            '--trust-root-thumbprint',
            $trustExport.root_thumbprint,
            '--trust-publisher-certificate',
            $trustExport.publisher_path,
            '--trust-publisher-thumbprint',
            $trustExport.publisher_thumbprint
        ) | Out-Null
    Invoke-ServiceAction `
        -StepName 'trust-store-remove-existing' `
        -Action 'trust-store-remove' `
        -ExtraArguments @(
            '--release-approved',
            '--trust-root-thumbprint',
            $trustExport.root_thumbprint,
            '--trust-publisher-thumbprint',
            $trustExport.publisher_thumbprint
        ) | Out-Null
    $trustRemoved = $true
    Invoke-ServiceAction `
        -StepName 'trust-store-restore-existing' `
        -Action 'trust-store-install' `
        -ExtraArguments @(
            '--release-approved',
            '--trust-root-certificate',
            $trustExport.root_path,
            '--trust-root-thumbprint',
            $trustExport.root_thumbprint,
            '--trust-publisher-certificate',
            $trustExport.publisher_path,
            '--trust-publisher-thumbprint',
            $trustExport.publisher_thumbprint
        ) | Out-Null
    $trustRemoved = $false

    $bootTimeAfter = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $true
        plan_only = $false
        actual_execution = 'completed'
        mutates_host = $true
        host_mutation_performed = $true
        artifact_root = $ArtifactRoot
        version = $Version
        routeparity_artifact = $RouteParityArtifactRoot
        lan_prefix = $LanPrefix
        public_trusted_signing = 'excluded'
        external_stable_publication = 'not-claimed'
        boot_time_before = $bootTimeBefore
        boot_time_after = $bootTimeAfter
        boot_time_unchanged = $bootTimeBefore -eq $bootTimeAfter
        final_service = Get-ServiceInfo
        final_firewall_rule_count = Get-FirewallRuleCount
        final_eventlog_source_present = Test-EventSourcePresent
        final_trust_store = Test-InternalTrustPresent
        steps = $summarySteps.ToArray()
    }
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-Output $ArtifactRoot
    exit 0
}
catch {
    if ($firewallEnabled) {
        try {
            Invoke-ServiceAction -StepName 'firewall-remove-cleanup' -Action 'firewall-remove' | Out-Null
        }
        catch {
        }
    }
    if ($trustRemoved -and $null -ne $trustExport) {
        try {
            Invoke-ServiceAction `
                -StepName 'trust-store-restore-cleanup' `
                -Action 'trust-store-install' `
                -ExtraArguments @(
                    '--release-approved',
                    '--trust-root-certificate',
                    $trustExport.root_path,
                    '--trust-root-thumbprint',
                    $trustExport.root_thumbprint,
                    '--trust-publisher-certificate',
                    $trustExport.publisher_path,
                    '--trust-publisher-thumbprint',
                    $trustExport.publisher_thumbprint
                ) | Out-Null
        }
        catch {
        }
    }
    $bootTimeAfter = try { (Get-CimInstance Win32_OperatingSystem).LastBootUpTime } catch { $null }
    $summary = [pscustomobject][ordered]@{
        schema_version = 1
        ok = $false
        plan_only = $false
        actual_execution = 'failed'
        mutates_host = $true
        host_mutation_performed = $true
        artifact_root = $ArtifactRoot
        version = $Version
        routeparity_artifact = $RouteParityArtifactRoot
        lan_prefix = $LanPrefix
        public_trusted_signing = 'excluded'
        external_stable_publication = 'not-claimed'
        error = [string]$_
        boot_time_before = $bootTimeBefore
        boot_time_after = $bootTimeAfter
        boot_time_unchanged = if ($null -ne $bootTimeBefore -and $null -ne $bootTimeAfter) { $bootTimeBefore -eq $bootTimeAfter } else { $false }
        final_service = Get-ServiceInfo
        final_firewall_rule_count = Get-FirewallRuleCount
        final_eventlog_source_present = Test-EventSourcePresent
        final_trust_store = Test-InternalTrustPresent
        steps = $summarySteps.ToArray()
    }
    Write-JsonFile -Path (Join-Path $ArtifactRoot 'summary.json') -Value $summary
    Write-Error $_
    exit 1
}
