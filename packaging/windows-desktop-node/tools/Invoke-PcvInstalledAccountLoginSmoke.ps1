[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("installed-account-login-smoke-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [string]$ApiBaseUri = 'http://127.0.0.1:7777',
    [string]$DataRoot = 'C:\ProgramData\PureCVisor\desktop-node',
    [string]$Username = 'pcv-smoke-operator',
    [switch]$RunBrowserQa,
    [string]$BrowserQaUrl = 'http://127.0.0.1/',
    [string]$BrowserQaArtifactRoot = '',
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$artifactRootFull = if ([System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    [System.IO.Path]::GetFullPath($ArtifactRoot)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtifactRoot))
}
New-Item -ItemType Directory -Path $artifactRootFull -Force | Out-Null

function Write-JsonFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Value
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $Value | ConvertTo-Json -Depth 18 | Set-Content -LiteralPath $Path -Encoding UTF8
}

function New-RandomSecret {
    param([int]$ByteCount = 32)

    $bytes = [byte[]]::new($ByteCount)
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function ConvertTo-Base64Url {
    param([Parameter(Mandatory)][byte[]]$Bytes)
    [Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function New-PcvPasswordHash {
    param(
        [Parameter(Mandatory)][string]$Password,
        [Parameter(Mandatory)][string]$SaltText
    )

    $iterations = 100000
    $salt = [System.Text.Encoding]::UTF8.GetBytes($SaltText)
    $hash = [System.Security.Cryptography.Rfc2898DeriveBytes]::Pbkdf2(
        [System.Text.Encoding]::UTF8.GetBytes($Password),
        $salt,
        $iterations,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256,
        32)
    @(
        'pbkdf2-sha256'
        [string]$iterations
        (ConvertTo-Base64Url -Bytes $salt)
        (ConvertTo-Base64Url -Bytes $hash)
    ) -join '$'
}

function Backup-OptionalFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$BackupPath
    )

    if (Test-Path -LiteralPath $Path) {
        $acl = Get-Acl -LiteralPath $Path
        Copy-Item -LiteralPath $Path -Destination $BackupPath -Force
        return [pscustomobject][ordered]@{
            existed = $true
            backup_path = $BackupPath
            acl_sddl = $acl.Sddl
            temporary_write_grant_status = 'not-run'
            temporary_takeown_status = 'not-run'
            acl_restore_status = 'not-run'
        }
    }

    [pscustomobject][ordered]@{
        existed = $false
        backup_path = $null
        acl_sddl = $null
        temporary_write_grant_status = 'not-needed'
        temporary_takeown_status = 'not-needed'
        acl_restore_status = 'not-needed'
    }
}

function Grant-TemporaryFileWriteAccess {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Backup
    )

    if (-not [bool]$Backup.existed -or -not (Test-Path -LiteralPath $Path)) {
        return
    }

    $takeownOutput = & takeown.exe /F $Path /A 2>&1
    if ($LASTEXITCODE -ne 0) {
        $Backup.temporary_takeown_status = 'failed'
        $detail = (($takeownOutput | Out-String).Trim())
        throw "PCV_ACCOUNT_LOGIN_SMOKE_ACL_TAKEOWN_FAILED|Could not take temporary ownership of protected account smoke file.|$Path|$detail"
    }

    $Backup.temporary_takeown_status = 'taken'
    $grantOutput = & icacls.exe $Path /grant:r '*S-1-5-32-544:F' '*S-1-5-18:F' 2>&1
    if ($LASTEXITCODE -ne 0) {
        $Backup.temporary_write_grant_status = 'grant-failed-after-takeown'
        $detail = (($grantOutput | Out-String).Trim())
        throw "PCV_ACCOUNT_LOGIN_SMOKE_ACL_GRANT_FAILED|Could not grant temporary write access to protected account smoke file.|$Path|$detail"
    }

    $acl = Get-Acl -LiteralPath $Path
    $hasAdminFullControl = @($acl.Access) | Where-Object {
        $identitySid = $_.IdentityReference.Translate([System.Security.Principal.SecurityIdentifier]).Value
        $identitySid -eq 'S-1-5-32-544' -and
        ($_.FileSystemRights -band [System.Security.AccessControl.FileSystemRights]::FullControl) -eq [System.Security.AccessControl.FileSystemRights]::FullControl
    }
    $hasSystemFullControl = @($acl.Access) | Where-Object {
        $identitySid = $_.IdentityReference.Translate([System.Security.Principal.SecurityIdentifier]).Value
        $identitySid -eq 'S-1-5-18' -and
        ($_.FileSystemRights -band [System.Security.AccessControl.FileSystemRights]::FullControl) -eq [System.Security.AccessControl.FileSystemRights]::FullControl
    }
    if (@($hasAdminFullControl).Count -eq 0 -or @($hasSystemFullControl).Count -eq 0) {
        $Backup.temporary_write_grant_status = 'grant-verification-failed'
        throw "PCV_ACCOUNT_LOGIN_SMOKE_ACL_GRANT_VERIFY_FAILED|Temporary write access was not visible after ACL grant.|$Path"
    }

    $Backup.temporary_write_grant_status = 'granted'
}

function Restore-SavedFileAcl {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Backup
    )

    if (-not [bool]$Backup.existed -or [string]::IsNullOrWhiteSpace([string]$Backup.acl_sddl)) {
        return
    }

    try {
        $security = [System.Security.AccessControl.FileSecurity]::new()
        $security.SetSecurityDescriptorSddlForm([string]$Backup.acl_sddl)
        Set-Acl -LiteralPath $Path -AclObject $security
        $Backup.acl_restore_status = 'restored'
    }
    catch {
        $Backup.acl_restore_status = 'failed'
        throw
    }
}

function Restore-OptionalFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)]$Backup
    )

    if ($Backup.existed) {
        Grant-TemporaryFileWriteAccess -Path $Path -Backup $Backup
        Copy-Item -LiteralPath ([string]$Backup.backup_path) -Destination $Path -Force
        Restore-SavedFileAcl -Path $Path -Backup $Backup
    } elseif (Test-Path -LiteralPath $Path) {
        Remove-Item -LiteralPath $Path -Force
    }
}

function Wait-ServiceState {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Expected,
        [int]$TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
        if ($null -ne $service -and [string]$service.Status -eq $Expected) {
            return $true
        }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)

    return $false
}

function Restart-SmokeService {
    param([Parameter(Mandatory)][string]$Name)

    $service = Get-Service -Name $Name -ErrorAction Stop
    if ($service.Status -ne 'Stopped') {
        Stop-Service -Name $Name -Force -ErrorAction Stop
        if (-not (Wait-ServiceState -Name $Name -Expected 'Stopped')) {
            throw "PCV_ACCOUNT_LOGIN_SMOKE_SERVICE_STOP_TIMEOUT|Service did not stop.|$Name"
        }
    }

    Start-Service -Name $Name -ErrorAction Stop
    if (-not (Wait-ServiceState -Name $Name -Expected 'Running')) {
        throw "PCV_ACCOUNT_LOGIN_SMOKE_SERVICE_START_TIMEOUT|Service did not start.|$Name"
    }
}

function Invoke-JsonRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Uri,
        [AllowNull()][string]$Body,
        [AllowNull()][string]$Bearer
    )

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($Bearer)) {
        $headers['Authorization'] = "Bearer $Bearer"
    }
    $request = @{
        Method = $Method
        Uri = $Uri
        Headers = $headers
        TimeoutSec = 20
        ErrorAction = 'Stop'
    }
    if (-not [string]::IsNullOrWhiteSpace($Body)) {
        $request.Body = $Body
        $request.ContentType = 'application/json'
    }
    if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
        $request.UseBasicParsing = $true
    }

    $response = Invoke-WebRequest @request
    [pscustomobject][ordered]@{
        status_code = [int]$response.StatusCode
        body = ($response.Content | ConvertFrom-Json)
    }
}

function Resolve-SmokePath {
    param([Parameter(Mandatory)][string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }

    [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Path))
}

function Invoke-BrowserLiveQa {
    param(
        [Parameter(Mandatory)][string]$AccessToken,
        [Parameter(Mandatory)][string]$AccountUsername,
        [Parameter(Mandatory)][string]$AccountPassword,
        [Parameter(Mandatory)][string]$Url,
        [Parameter(Mandatory)][string]$OutDir
    )

    $repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
    $captureScript = Join-Path $repoRoot 'web/scripts/capture-installed-listener-qa.mjs'
    if (-not (Test-Path -LiteralPath $captureScript -PathType Leaf)) {
        throw "PCV_ACCOUNT_LOGIN_BROWSER_QA_SCRIPT_MISSING|Installed listener browser QA script was not found.|$captureScript"
    }

    $previousToken = [System.Environment]::GetEnvironmentVariable('PCV_BROWSER_QA_TOKEN', 'Process')
    $previousUsername = [System.Environment]::GetEnvironmentVariable('PCV_BROWSER_QA_ACCOUNT_USERNAME', 'Process')
    $previousPassword = [System.Environment]::GetEnvironmentVariable('PCV_BROWSER_QA_ACCOUNT_PASSWORD', 'Process')
    try {
        [System.Environment]::SetEnvironmentVariable('PCV_BROWSER_QA_TOKEN', $null, 'Process')
        [System.Environment]::SetEnvironmentVariable('PCV_BROWSER_QA_ACCOUNT_USERNAME', $AccountUsername, 'Process')
        [System.Environment]::SetEnvironmentVariable('PCV_BROWSER_QA_ACCOUNT_PASSWORD', $AccountPassword, 'Process')
        $output = & node $captureScript "--url=$Url" "--out=$OutDir" 2>&1
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            $detail = (($output | Out-String).Trim())
            throw "PCV_ACCOUNT_LOGIN_BROWSER_QA_FAILED|Installed listener browser QA failed.|exit=$exitCode|$detail"
        }

        $summaryPath = Join-Path $OutDir 'summary.json'
        if (-not (Test-Path -LiteralPath $summaryPath -PathType Leaf)) {
            throw "PCV_ACCOUNT_LOGIN_BROWSER_QA_SUMMARY_MISSING|Installed listener browser QA did not write summary.json.|$summaryPath"
        }

        $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
        if (-not [bool]$summary.ok) {
            throw "PCV_ACCOUNT_LOGIN_BROWSER_QA_NOT_OK|Installed listener browser QA summary reported ok=false.|$summaryPath"
        }

        [pscustomobject][ordered]@{
            ok = $true
            exit_code = $exitCode
            artifact_root = $OutDir
            screenshot_count = @($summary.screenshots).Count
            actions = $summary.actions
            accessibility_probe = $summary.accessibility_probe
        }
    }
    finally {
        [System.Environment]::SetEnvironmentVariable('PCV_BROWSER_QA_TOKEN', $previousToken, 'Process')
        [System.Environment]::SetEnvironmentVariable('PCV_BROWSER_QA_ACCOUNT_USERNAME', $previousUsername, 'Process')
        [System.Environment]::SetEnvironmentVariable('PCV_BROWSER_QA_ACCOUNT_PASSWORD', $previousPassword, 'Process')
    }
}

$accountFile = Join-Path $DataRoot 'accounts.json'
$signingKeyFile = Join-Path $DataRoot 'jwt-signing-key.txt'
$accountBackupPath = Join-Path $artifactRootFull 'accounts.json.backup'
$signingKeyBackupPath = Join-Path $artifactRootFull 'jwt-signing-key.txt.backup'
$summaryPath = Join-Path $artifactRootFull 'summary.json'
$started = Get-Date
$accountBackup = $null
$signingKeyBackup = $null
$restored = $false
$ok = $false
$errorRecord = $null
$loginStatus = $null
$sessionStatus = $null
$rbacStatus = $null
$consoleStatus = $null
$runtimeAuthMode = $null
$browserQaStatus = if ($RunBrowserQa) { 'pending' } else { 'not-run' }
$browserQaExitCode = $null
$browserQaArtifactRootFull = $null
$browserQaScreenshotCount = $null
$browserQaActions = $null
$browserQaAccessibilityProbe = $null

try {
    if ($DryRun) {
        $ok = $true
        return
    }

    New-Item -ItemType Directory -Path $DataRoot -Force | Out-Null
    $accountBackup = Backup-OptionalFile -Path $accountFile -BackupPath $accountBackupPath
    $signingKeyBackup = Backup-OptionalFile -Path $signingKeyFile -BackupPath $signingKeyBackupPath
    Grant-TemporaryFileWriteAccess -Path $accountFile -Backup $accountBackup
    Grant-TemporaryFileWriteAccess -Path $signingKeyFile -Backup $signingKeyBackup

    $password = New-RandomSecret -ByteCount 24
    $signingKey = New-RandomSecret -ByteCount 48
    $passwordHash = New-PcvPasswordHash -Password $password -SaltText "pcv-installed-account-login-smoke"
    $accountConfig = [ordered]@{
        issuer = 'purecvisor-desktop-node'
        audience = 'desktop-node-local-api'
        accounts = @(
            [ordered]@{
                id = 'installed-account-login-smoke'
                username = $Username
                password_hash = $passwordHash
                role = 'operator'
                display_name = 'Installed Account Login Smoke'
            }
        )
    }

    Write-JsonFile -Path $accountFile -Value $accountConfig
    Set-Content -LiteralPath $signingKeyFile -Value $signingKey -Encoding UTF8
    Restart-SmokeService -Name $ServiceName

    $loginPayload = @{ username = $Username; password = $password } | ConvertTo-Json -Compress
    $login = Invoke-JsonRequest -Method 'POST' -Uri "$ApiBaseUri/api/v1/auth/login" -Body $loginPayload -Bearer $null
    $loginStatus = $login.status_code
    $accessToken = [string]$login.body.data.access_token
    $refreshToken = [string]$login.body.data.refresh_token

    $session = Invoke-JsonRequest -Method 'GET' -Uri "$ApiBaseUri/api/v1/auth/session" -Body $null -Bearer $accessToken
    $rbac = Invoke-JsonRequest -Method 'GET' -Uri "$ApiBaseUri/api/v1/auth/rbac" -Body $null -Bearer $accessToken
    $console = Invoke-JsonRequest -Method 'GET' -Uri "$ApiBaseUri/api/v1/console/capabilities" -Body $null -Bearer $accessToken
    $runtime = Invoke-JsonRequest -Method 'GET' -Uri "$ApiBaseUri/api/v1/runtime/policy" -Body $null -Bearer $accessToken

    $sessionStatus = $session.status_code
    $rbacStatus = $rbac.status_code
    $consoleStatus = $console.status_code
    $runtimeAuthMode = [string]$runtime.body.data.auth.mode
    if ($loginStatus -ne 200 -or $sessionStatus -ne 200 -or $rbacStatus -ne 200 -or $consoleStatus -ne 200) {
        throw "PCV_ACCOUNT_LOGIN_SMOKE_HTTP_STATUS|Installed account login smoke route returned unexpected status.|login=$loginStatus session=$sessionStatus rbac=$rbacStatus console=$consoleStatus"
    }

    if ($RunBrowserQa) {
        $browserQaArtifactRootFull = if ([string]::IsNullOrWhiteSpace($BrowserQaArtifactRoot)) {
            Join-Path $artifactRootFull 'browser-live-qa'
        } else {
            Resolve-SmokePath -Path $BrowserQaArtifactRoot
        }

        $browserQa = Invoke-BrowserLiveQa -AccessToken $accessToken -AccountUsername $Username -AccountPassword $password -Url $BrowserQaUrl -OutDir $browserQaArtifactRootFull
        $browserQaStatus = 'pass'
        $browserQaExitCode = $browserQa.exit_code
        $browserQaScreenshotCount = $browserQa.screenshot_count
        $browserQaActions = $browserQa.actions
        $browserQaAccessibilityProbe = $browserQa.accessibility_probe
    }

    $null = $refreshToken
    $ok = $true
}
catch {
    $errorRecord = [pscustomobject][ordered]@{
        message = $_.Exception.Message
        category = [string]$_.CategoryInfo.Category
    }
    throw
}
finally {
    if (-not $DryRun -and $null -ne $accountBackup -and $null -ne $signingKeyBackup) {
        try {
            Restore-OptionalFile -Path $accountFile -Backup $accountBackup
            Restore-OptionalFile -Path $signingKeyFile -Backup $signingKeyBackup
            Restart-SmokeService -Name $ServiceName
            $restored = $true
        }
        catch {
            if ($null -eq $errorRecord) {
                $errorRecord = [pscustomobject][ordered]@{
                    message = $_.Exception.Message
                    category = [string]$_.CategoryInfo.Category
                }
            }
        }
    }

    $summary = [ordered]@{
        schema_version = 1
        ok = [bool]$ok
        installed_account_login_smoke = if ($DryRun) { 'dry-run' } elseif ($ok) { 'pass' } else { 'failed' }
        actual_execution = if ($DryRun) { 'dry-run-no-mutation' } else { 'installed-service-account-login-smoke' }
        service_name = $ServiceName
        api_base_uri = $ApiBaseUri
        account_file = $accountFile
        jwt_signing_key_file = $signingKeyFile
        account_file_backup = $accountBackup
        jwt_signing_key_backup = $signingKeyBackup
        restore_status = if ($DryRun) { 'not-run' } elseif ($restored) { 'restored' } else { 'restore-not-confirmed' }
        service_restart_status = if ($DryRun) { 'not-run' } elseif ($restored) { 'restarted-after-restore' } else { 'attempted' }
        login_status_code = $loginStatus
        session_status_code = $sessionStatus
        rbac_status_code = $rbacStatus
        console_capabilities_status_code = $consoleStatus
        runtime_auth_mode = $runtimeAuthMode
        token_value_observed = $false
        password_value_observed = $false
        refresh_token_value_observed = $false
        browser_qa = [ordered]@{
            execution = if ($DryRun) { 'dry-run' } elseif ($RunBrowserQa) { 'installed-listener-browser-live-smoke' } else { 'not-run' }
            status = if ($DryRun) { 'dry-run' } else { $browserQaStatus }
            url = if ($RunBrowserQa) { $BrowserQaUrl } else { $null }
            artifact_root = $browserQaArtifactRootFull
            exit_code = $browserQaExitCode
            screenshot_count = $browserQaScreenshotCount
            actions = $browserQaActions
            accessibility_probe = $browserQaAccessibilityProbe
            token_value_observed = $false
            password_value_observed = $false
        }
        host_mutation_performed = -not [bool]$DryRun
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        started_at = $started.ToString('o')
        completed_at = (Get-Date).ToString('o')
        error = $errorRecord
    }
    Write-JsonFile -Path $summaryPath -Value $summary
}
