[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("windows-credential-manager-default-transition-installed-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$Version = '0.39.4-admin-smoke',
    [int]$BuildTimeoutSeconds = 900,
    [int]$MsiTimeoutSeconds = 900
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

$programData = [Environment]::GetEnvironmentVariable('ProgramData')
if ([string]::IsNullOrWhiteSpace($programData)) {
    $programData = 'C:\ProgramData'
}
$dataRoot = Join-Path $programData 'PureCVisor\desktop-node'
$protectedTokenPath = Join-Path $dataRoot 'api-token.dpapi.json'
$transitionEvidencePath = Join-Path $dataRoot 'credential-manager-transition.json'
$rollbackDiagnosticsPath = Join-Path $dataRoot 'credential-manager-transition.rollback.json'

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    $Value | ConvertTo-Json -Depth 16 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function Invoke-CapturedProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments,
        [int]$TimeoutSeconds = 900
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FileName
    foreach ($argument in $Arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.UseShellExecute = $false
    $process = [System.Diagnostics.Process]::Start($startInfo)
    if ($null -eq $process) {
        throw "PCV_PROCESS_START_FAILED|Failed to start process.|$FileName"
    }

    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        try {
            $process.Kill($true)
        }
        catch {
        }
        throw "PCV_PROCESS_TIMEOUT|Process timed out.|$FileName $($Arguments -join ' ')"
    }

    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    [pscustomobject][ordered]@{
        file_name = $FileName
        arguments = @($Arguments)
        exit_code = [int]$process.ExitCode
        stdout = $stdout
        stderr = $stderr
        ok = [bool]($process.ExitCode -eq 0)
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

function Get-ServiceInfo {
    Get-CimInstance Win32_Service -Filter "Name='PureCVisorDesktopNode'" -ErrorAction SilentlyContinue
}

function Wait-ServiceState {
    param(
        [Parameter(Mandatory)][string]$Expected,
        [int]$TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $service = Get-ServiceInfo
        if ($Expected -eq 'Missing') {
            if ($null -eq $service) {
                return $true
            }
        }
        elseif ($null -ne $service -and [string]$service.State -eq $Expected) {
            return $true
        }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Read-ProtectedToken {
    param([Parameter(Mandatory)][string]$Path)

    $modulePath = Join-Path $repoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
    Import-Module $modulePath -Force
    $token = Read-PcvDesktopNodeProductProtectedTokenFile -Path $Path
    [string]$token.token
}

function Invoke-Api {
    param(
        [Parameter(Mandatory)][string]$Token,
        [string]$Path = '/api/v1/runtime/policy'
    )

    $uri = 'http://127.0.0.1:7777' + $Path
    $requestArgs = @{
        Uri = $uri
        Headers = @{ Authorization = "Bearer $Token" }
        TimeoutSec = 15
        ErrorAction = 'Stop'
    }
    if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
        $requestArgs.UseBasicParsing = $true
    }
    $response = Invoke-WebRequest @requestArgs
    [pscustomobject][ordered]@{
        status_code = [int]$response.StatusCode
        body = ($response.Content | ConvertFrom-Json)
    }
}

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$buildPath = Join-Path $artifactRootFull 'build.json'
$installPath = Join-Path $artifactRootFull 'install.json'
$installLogPath = Join-Path $artifactRootFull 'install.log'
$transitionCopyPath = Join-Path $artifactRootFull 'credential-manager-transition.json'
$rollbackCopyPath = Join-Path $artifactRootFull 'credential-manager-transition.rollback.json'

$summary = [ordered]@{
    ok = $false
    scope = 'windows-credential-manager-default-transition-installed'
    version = $Version
    artifact_root = $artifactRootFull
    actual_execution = 'installed-msi-local-system-custom-action'
    host_mutation_performed = $true
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    build = $null
    install = $null
    transition = $null
    rollback_diagnostics = $null
    service = $null
    health = $null
    evidence_paths = [ordered]@{
        transition = $transitionCopyPath
        rollback_diagnostics = $rollbackCopyPath
    }
}

try {
    $build = Invoke-CapturedProcess `
        -FileName 'pwsh' `
        -Arguments @(
            '-NoProfile',
            '-ExecutionPolicy',
            'Bypass',
            '-File',
            (Join-Path $repoRoot 'packaging/windows-desktop-node/installer/build.ps1'),
            '-Version',
            $Version,
            '-OutputRoot',
            $artifactRootFull,
            '-SigningMode',
            'AllowUnsignedDev'
        ) `
        -TimeoutSeconds $BuildTimeoutSeconds
    Write-JsonFile -Path $buildPath -Value $build
    Assert-True -Condition $build.ok -Message "PCV_CREDENTIAL_MANAGER_SMOKE_BUILD_FAILED|build.ps1 exited $($build.exit_code)."
    $buildOutput = $build.stdout | ConvertFrom-Json
    $summary.build = [ordered]@{
        msi_path = [string]$buildOutput.msi_path
        msi_sha256 = [string]$buildOutput.provenance.msi.sha256
        provenance_commit = [string]$buildOutput.provenance.git_commit
        signing_mode = [string]$buildOutput.provenance.signing_mode
    }

    $install = Invoke-CapturedProcess `
        -FileName 'msiexec.exe' `
        -Arguments @(
            '/i',
            [string]$buildOutput.msi_path,
            'REBOOT=ReallySuppress',
            'MSIRESTARTMANAGERCONTROL=Disable',
            '/qn',
            '/norestart',
            '/l*vx',
            $installLogPath
        ) `
        -TimeoutSeconds $MsiTimeoutSeconds
    Write-JsonFile -Path $installPath -Value $install
    Assert-True -Condition $install.ok -Message "PCV_CREDENTIAL_MANAGER_SMOKE_INSTALL_FAILED|msiexec exited $($install.exit_code)."
    $summary.install = [ordered]@{
        exit_code = $install.exit_code
        log_path = $installLogPath
    }

    Assert-True -Condition (Wait-ServiceState -Expected 'Running' -TimeoutSeconds 120) -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_SERVICE_NOT_RUNNING|Service did not reach Running after install.'
    Assert-True -Condition (Test-Path -LiteralPath $transitionEvidencePath -PathType Leaf) -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_TRANSITION_EVIDENCE_MISSING|credential-manager-transition.json was not written.'
    Assert-True -Condition (Test-Path -LiteralPath $rollbackDiagnosticsPath -PathType Leaf) -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_ROLLBACK_DIAGNOSTICS_MISSING|credential-manager-transition.rollback.json was not written.'
    Copy-Item -LiteralPath $transitionEvidencePath -Destination $transitionCopyPath -Force
    Copy-Item -LiteralPath $rollbackDiagnosticsPath -Destination $rollbackCopyPath -Force

    $transition = Get-Content -LiteralPath $transitionEvidencePath -Raw | ConvertFrom-Json
    $rollback = Get-Content -LiteralPath $rollbackDiagnosticsPath -Raw | ConvertFrom-Json
    $service = Get-ServiceInfo
    $token = Read-ProtectedToken -Path $protectedTokenPath
    $health = Invoke-Api -Token $token
    $runtimeTokenStorage = [string]$health.body.data.auth.token_storage

    $summary.transition = [ordered]@{
        ok = [bool]$transition.ok
        identity = [string]$transition.identity
        credential_target = [string]$transition.credential_target
        system_proof_status = [string]$transition.system_proof_status
        credential_write_status = [string]$transition.credential_write_status
        credential_read_status = [string]$transition.credential_read_status
        credential_delete_status = [string]$transition.credential_delete_status
        token_source_migration = [string]$transition.token_source_migration
        service_reload_status = [string]$transition.service_reload_status
        old_source_rejection_status = [string]$transition.old_source_rejection_status
        rollback_diagnostics_status = [string]$transition.rollback_diagnostics_status
        token_value_observed = [bool]$transition.token_value_observed
        host_mutation_performed = [bool]$transition.host_mutation_performed
    }
    $summary.rollback_diagnostics = [ordered]@{
        schema_version = [int]$rollback.schema_version
        rollback_attempted = [bool]$rollback.rollback_attempted
        rollback_succeeded = [bool]$rollback.rollback_succeeded
        token_value_observed = [bool]$rollback.token_value_observed
        fallback_token_source = [string]$rollback.fallback_token_source
    }
    $summary.service = [ordered]@{
        name = [string]$service.Name
        state = [string]$service.State
        start_name = [string]$service.StartName
        path_name = [string]$service.PathName
        uses_credential_manager_target = ([string]$service.PathName).Contains('--api-token-credential-target', [System.StringComparison]::OrdinalIgnoreCase)
        uses_protected_token_file = ([string]$service.PathName).Contains('--api-token-protected-file', [System.StringComparison]::OrdinalIgnoreCase)
    }
    $summary.health = [ordered]@{
        runtime_policy_status_code = [int]$health.status_code
        token_storage = $runtimeTokenStorage
        token_value_observed = $false
    }

    Assert-True -Condition ([bool]$transition.ok) -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_TRANSITION_NOT_OK|Transition descriptor did not report ok.'
    Assert-True -Condition ([string]$transition.identity -eq 'NT AUTHORITY\SYSTEM') -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_SYSTEM_IDENTITY_REQUIRED|Transition did not run as LocalSystem.'
    Assert-True -Condition ([string]$transition.system_proof_status -eq 'system-context-proof-pass') -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_SYSTEM_PROOF_REQUIRED|System proof did not pass.'
    Assert-True -Condition ([string]$transition.token_source_migration -eq 'protected-file-to-credential-manager') -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_MIGRATION_REQUIRED|Token source migration did not pass.'
    Assert-True -Condition ([string]$transition.service_reload_status -eq 'restarted') -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_RELOAD_REQUIRED|Service reload did not pass.'
    Assert-True -Condition ([string]$transition.old_source_rejection_status -eq 'protected-file-source-rejected-after-reload') -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_OLD_SOURCE_REJECTION_REQUIRED|Old source rejection did not pass.'
    Assert-True -Condition ([string]$transition.rollback_diagnostics_status -eq 'written') -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_ROLLBACK_DIAGNOSTICS_REQUIRED|Rollback diagnostics were not written.'
    Assert-True -Condition (-not [bool]$transition.token_value_observed) -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_TOKEN_VALUE_OBSERVED|Transition descriptor exposed a token value.'
    Assert-True -Condition ([bool]$summary.service.uses_credential_manager_target) -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_SERVICE_PATH_TARGET_REQUIRED|Service PathName does not use Credential Manager target.'
    Assert-True -Condition (-not [bool]$summary.service.uses_protected_token_file) -Message 'PCV_CREDENTIAL_MANAGER_SMOKE_SERVICE_PATH_OLD_SOURCE_PRESENT|Service PathName still uses protected token file.'
    Assert-True -Condition ($runtimeTokenStorage -eq 'windows-credential-manager') -Message "PCV_CREDENTIAL_MANAGER_SMOKE_RUNTIME_POLICY_STORAGE_REQUIRED|Runtime token storage was '$runtimeTokenStorage'."

    $summary.ok = $true
    Write-JsonFile -Path $summaryPath -Value $summary
    $summary | ConvertTo-Json -Depth 16
}
catch {
    $summary.error = [ordered]@{
        message = [string]$_.Exception.Message
        detail = [string]$_
    }
    Write-JsonFile -Path $summaryPath -Value $summary
    throw
}
