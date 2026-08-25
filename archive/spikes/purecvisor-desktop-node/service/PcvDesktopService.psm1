Set-StrictMode -Version Latest

function Quote-PcvServiceArgument {
    param([Parameter(Mandatory)][string]$Value)

    '"' + ($Value -replace '"', '\"') + '"'
}

function New-PcvServiceError {
    param(
        [Parameter(Mandatory)][string]$Code,
        [Parameter(Mandatory)][string]$Message,
        [Parameter(Mandatory)][string]$Detail
    )

    [ordered]@{
        code = $Code
        message = $Message
        detail = $Detail
        retryable = $false
    }
}

function Get-PcvDesktopServiceDefaultTokenFilePath {
    $programData = [Environment]::GetEnvironmentVariable('ProgramData')
    if ([string]::IsNullOrWhiteSpace($programData)) {
        $programData = 'C:\ProgramData'
    }

    Join-Path (Join-Path $programData 'PureCVisor\desktop-node') 'api-token.txt'
}

function Get-PcvDesktopServiceDefaultProtectedTokenFilePath {
    $programData = [Environment]::GetEnvironmentVariable('ProgramData')
    if ([string]::IsNullOrWhiteSpace($programData)) {
        $programData = 'C:\ProgramData'
    }

    Join-Path (Join-Path $programData 'PureCVisor\desktop-node') 'api-token.dpapi.json'
}

function New-PcvDesktopServiceToken {
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

function Get-PcvDesktopServiceTokenStoreEntropy {
    [System.Text.Encoding]::UTF8.GetBytes('PureCVisor Desktop Node API Token Store v1')
}

function Get-PcvDesktopServiceTokenSha256 {
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

function Initialize-PcvDesktopServiceProtectedDataSupport {
    if ('System.Security.Cryptography.ProtectedData' -as [type]) {
        return
    }

    try {
        Add-Type -AssemblyName System.Security -ErrorAction Stop
    }
    catch {
        throw "PCV_SERVICE_PROTECTED_DATA_UNAVAILABLE|DPAPI ProtectedData support could not be loaded.|$($_.Exception.Message)"
    }

    if (-not ('System.Security.Cryptography.ProtectedData' -as [type])) {
        throw 'PCV_SERVICE_PROTECTED_DATA_UNAVAILABLE|DPAPI ProtectedData support is unavailable.|System.Security.Cryptography.ProtectedData type was not found after loading System.Security.'
    }
}

function Protect-PcvDesktopServiceToken {
    param([Parameter(Mandatory)][string]$Token)

    if ([string]::IsNullOrWhiteSpace($Token)) {
        throw 'PCV_SERVICE_TOKEN_EMPTY|The service API token must not be empty.|Pass a non-empty token or omit -Token to generate one.'
    }

    Initialize-PcvDesktopServiceProtectedDataSupport
    $tokenBytes = [System.Text.Encoding]::UTF8.GetBytes($Token)
    $protectedBytes = [System.Security.Cryptography.ProtectedData]::Protect(
        $tokenBytes,
        (Get-PcvDesktopServiceTokenStoreEntropy),
        [System.Security.Cryptography.DataProtectionScope]::LocalMachine
    )
    [Convert]::ToBase64String($protectedBytes)
}

function Unprotect-PcvDesktopServiceToken {
    param([Parameter(Mandatory)][string]$ProtectedToken)

    if ([string]::IsNullOrWhiteSpace($ProtectedToken)) {
        throw 'PCV_SERVICE_PROTECTED_TOKEN_EMPTY|The protected service API token is empty.|The protected token file must contain a non-empty protected_token value.'
    }

    try {
        Initialize-PcvDesktopServiceProtectedDataSupport
        $protectedBytes = [Convert]::FromBase64String($ProtectedToken)
        $tokenBytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
            $protectedBytes,
            (Get-PcvDesktopServiceTokenStoreEntropy),
            [System.Security.Cryptography.DataProtectionScope]::LocalMachine
        )
        [System.Text.Encoding]::UTF8.GetString($tokenBytes)
    }
    catch {
        throw "PCV_SERVICE_PROTECTED_TOKEN_UNPROTECT_FAILED|The protected service API token could not be read.|$($_.Exception.Message)"
    }
}

function Get-PcvObjectPropertyValue {
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

function Read-PcvDesktopServiceProtectedTokenFile {
    param([string]$Path = (Get-PcvDesktopServiceDefaultProtectedTokenFilePath))

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_SERVICE_PROTECTED_TOKEN_FILE_PATH_REQUIRED|The protected token file path is required.|Pass -ApiTokenProtectedFile or use the default ProgramData protected token path.'
    }

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "PCV_SERVICE_PROTECTED_TOKEN_FILE_NOT_FOUND|The protected API token file was not found.|Create the protected token file before starting the listener: '$Path'."
    }

    try {
        $json = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        throw "PCV_SERVICE_PROTECTED_TOKEN_FILE_INVALID|The protected API token file is not valid JSON.|$($_.Exception.Message)"
    }

    $schemaVersion = Get-PcvObjectPropertyValue -InputObject $json -Name 'schema_version'
    $storage = [string](Get-PcvObjectPropertyValue -InputObject $json -Name 'storage')
    $scope = [string](Get-PcvObjectPropertyValue -InputObject $json -Name 'scope')
    $protectedToken = [string](Get-PcvObjectPropertyValue -InputObject $json -Name 'protected_token')

    if ([int]$schemaVersion -ne 1 -or
        $storage -ne 'dpapi-local-machine' -or
        $scope -ne 'LocalMachine' -or
        [string]::IsNullOrWhiteSpace($protectedToken)) {
        throw "PCV_SERVICE_PROTECTED_TOKEN_FILE_INVALID|The protected API token file schema is invalid.|Expected schema_version 1, storage dpapi-local-machine, scope LocalMachine, and protected_token in '$Path'."
    }

    $token = Unprotect-PcvDesktopServiceToken -ProtectedToken $protectedToken
    if ([string]::IsNullOrWhiteSpace($token)) {
        throw "PCV_SERVICE_PROTECTED_TOKEN_EMPTY|The protected service API token resolved to an empty value.|Rotate the protected token file: '$Path'."
    }

    [ordered]@{
        ok = $true
        path = $Path
        storage = 'dpapi-local-machine'
        scope = 'LocalMachine'
        token = $token
        token_length = $token.Length
        token_sha256 = Get-PcvDesktopServiceTokenSha256 -Token $token
    }
}

function Resolve-PcvServiceAccountAclPrincipal {
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

function New-PcvTokenFileAclCommand {
    param(
        [Parameter(Mandatory)][string]$Path,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$AdminPrincipal = 'BUILTIN\Administrators'
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_SERVICE_TOKEN_FILE_PATH_REQUIRED|The token file path is required.|Pass -ApiTokenFile or use the default ProgramData token path.'
    }

    if ([string]::IsNullOrWhiteSpace($AdminPrincipal)) {
        throw 'PCV_SERVICE_TOKEN_ADMIN_PRINCIPAL_REQUIRED|The token file administrator principal is required.|Pass a Windows principal such as BUILTIN\Administrators.'
    }

    $servicePrincipal = Resolve-PcvServiceAccountAclPrincipal -ServiceAccount $ServiceAccount
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

    Write-Output -NoEnumerate $commands
}

function New-PcvDesktopServiceProtectedTokenFile {
    param(
        [string]$Path = (Get-PcvDesktopServiceDefaultProtectedTokenFilePath),
        [AllowNull()][string]$Token,
        [ValidateRange(16, 128)][int]$TokenByteLength = 32,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$AdminPrincipal = 'BUILTIN\Administrators',
        [switch]$Force,
        [scriptblock]$InvokeProcess
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_SERVICE_PROTECTED_TOKEN_FILE_PATH_REQUIRED|The protected token file path is required.|Pass -ApiTokenProtectedFile or use the default ProgramData protected token path.'
    }

    if ($PSBoundParameters.ContainsKey('Token') -and [string]::IsNullOrWhiteSpace($Token)) {
        throw 'PCV_SERVICE_TOKEN_EMPTY|The service API token must not be empty.|Pass a non-empty token or omit -Token to generate one.'
    }

    if ((Test-Path -LiteralPath $Path -PathType Leaf) -and -not $Force) {
        throw "PCV_SERVICE_PROTECTED_TOKEN_FILE_EXISTS|The protected service API token file already exists.|Pass -Force to rotate '$Path'."
    }

    $tokenValue = $Token
    if (-not $PSBoundParameters.ContainsKey('Token')) {
        $tokenValue = New-PcvDesktopServiceToken -ByteLength $TokenByteLength
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
        token_sha256 = Get-PcvDesktopServiceTokenSha256 -Token $tokenValue
        protected_token = Protect-PcvDesktopServiceToken -Token $tokenValue
    }
    $record | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $Path -Encoding UTF8 -ErrorAction Stop

    $aclResult = Invoke-PcvTokenFileAclApply `
        -Path $Path `
        -ServiceAccount $ServiceAccount `
        -AdminPrincipal $AdminPrincipal `
        -InvokeProcess $InvokeProcess

    [ordered]@{
        ok = $aclResult.ok
        path = $Path
        storage = 'dpapi-local-machine'
        scope = 'LocalMachine'
        token_length = $tokenValue.Length
        service_account = $ServiceAccount
        service_acl_principal = (Resolve-PcvServiceAccountAclPrincipal -ServiceAccount $ServiceAccount)
        admin_principal = $AdminPrincipal
        acl = $aclResult
        error = $aclResult.error
    }
}

function Remove-PcvDesktopServiceProtectedTokenFile {
    param([string]$Path = (Get-PcvDesktopServiceDefaultProtectedTokenFilePath))

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_SERVICE_PROTECTED_TOKEN_FILE_PATH_REQUIRED|The protected token file path is required.|Pass -ApiTokenProtectedFile or use the default ProgramData protected token path.'
    }

    if (-not (Test-Path -LiteralPath $Path)) {
        return [ordered]@{
            ok = $true
            path = $Path
            removed = $false
        }
    }

    Remove-Item -LiteralPath $Path -Force -ErrorAction Stop
    [ordered]@{
        ok = $true
        path = $Path
        removed = $true
    }
}

function Get-PcvServiceApiTokenSource {
    param(
        [AllowNull()][string]$ApiToken,
        [AllowNull()][string]$ApiTokenFile,
        [AllowNull()][string]$ApiTokenProtectedFile
    )

    $sources = @()
    if (-not [string]::IsNullOrWhiteSpace($ApiToken)) {
        $sources += 'inline'
    }
    if (-not [string]::IsNullOrWhiteSpace($ApiTokenFile)) {
        $sources += 'file'
    }
    if (-not [string]::IsNullOrWhiteSpace($ApiTokenProtectedFile)) {
        $sources += 'protected_file'
    }

    if ($sources.Count -gt 1) {
        throw 'PCV_SERVICE_TOKEN_SOURCE_CONFLICT|Specify only one service API token source.|Use -ApiToken for short-lived tests, -ApiTokenFile for legacy packaging, or -ApiTokenProtectedFile for product service packaging.'
    }

    if ($sources.Count -eq 1) {
        return $sources[0]
    }

    'none'
}

function Get-PcvServicePrefixExposure {
    param(
        [Parameter(Mandatory)][string]$Prefix,
        [switch]$AllowLan,
        [AllowNull()][string]$ApiToken,
        [AllowNull()][string]$ApiTokenFile,
        [AllowNull()][string]$ApiTokenProtectedFile
    )

    try {
        $uri = [System.Uri]::new($Prefix, [System.UriKind]::Absolute)
    }
    catch {
        throw "PCV_SERVICE_PREFIX_INVALID|The service API prefix is not a valid absolute URI.|$($_.Exception.Message)"
    }

    if ($uri.Scheme -ne 'http') {
        throw 'PCV_SERVICE_PREFIX_INVALID|The service API prefix must use http.|Use an http://127.0.0.1:<port>/ prefix or explicit LAN prefix.'
    }

    if (-not $Prefix.EndsWith('/')) {
        throw 'PCV_SERVICE_PREFIX_INVALID|The service API prefix must end with a slash.|HttpListener prefixes require a trailing slash.'
    }

    $hostName = $uri.DnsSafeHost.ToLowerInvariant()
    $isLoopback = $hostName -in @('127.0.0.1', 'localhost', '::1')
    $tokenSource = Get-PcvServiceApiTokenSource `
        -ApiToken $ApiToken `
        -ApiTokenFile $ApiTokenFile `
        -ApiTokenProtectedFile $ApiTokenProtectedFile
    $hasToken = $tokenSource -ne 'none'

    if (-not $isLoopback -and -not $AllowLan) {
        throw "PCV_SERVICE_PREFIX_NOT_LOOPBACK|The service API prefix must stay on loopback unless LAN mode is explicitly enabled.|Rejected host '$hostName'."
    }

    if (-not $isLoopback -and -not $hasToken) {
        throw 'PCV_SERVICE_LAN_TOKEN_REQUIRED|LAN service mode requires a bearer token.|Pass -ApiTokenFile or -ApiToken with a non-empty token when -AllowLan is used.'
    }

    [ordered]@{
        host = $hostName
        port = $uri.Port
        exposure = $(if ($isLoopback) { 'loopback' } else { 'lan' })
        auth_required = $hasToken
        api_token_source = $tokenSource
    }
}

function Add-PcvServiceListenerOption {
    param(
        [Parameter(Mandatory)][System.Collections.Generic.List[string]]$Arguments,
        [Parameter(Mandatory)][string]$Name,
        [AllowNull()][string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return
    }

    $Arguments.Add($Name)
    $Arguments.Add($Value)
}

function Get-PcvDesktopServicePwshCandidateVersion {
    param([Parameter(Mandatory)][string]$Path)

    $parentName = Split-Path -Leaf (Split-Path -Parent $Path)
    $versionText = '0.0.0.0'
    if ($parentName -match '^Microsoft\.PowerShell_(?<version>[0-9]+(?:\.[0-9]+){1,3})_') {
        $versionText = $Matches.version
    } elseif ($parentName -match '^(?<version>[0-9]+(?:\.[0-9]+){0,3})') {
        $versionText = $Matches.version
    }

    try {
        [version]$versionText
    } catch {
        [version]'0.0.0.0'
    }
}

function Get-PcvDesktopServicePwshSearchRoots {
    param([string[]]$SearchRoots)

    $roots = New-Object System.Collections.Generic.List[string]
    if ($null -ne $SearchRoots -and $SearchRoots.Count -gt 0) {
        foreach ($root in $SearchRoots) {
            if (-not [string]::IsNullOrWhiteSpace($root) -and -not $roots.Contains($root)) {
                $roots.Add($root)
            }
        }
        return @($roots)
    }

    foreach ($root in @($env:ProgramFiles, $env:ProgramW6432, ${env:ProgramFiles(x86)})) {
        if (-not [string]::IsNullOrWhiteSpace($root) -and -not $roots.Contains($root)) {
            $roots.Add($root)
        }
    }

    @($roots)
}

function Find-PcvDesktopServicePwshInProgramFiles {
    param([string[]]$SearchRoots)

    $paths = New-Object System.Collections.Generic.List[string]
    foreach ($root in (Get-PcvDesktopServicePwshSearchRoots -SearchRoots $SearchRoots)) {
        $patterns = @(
            (Join-Path $root 'PowerShell\*\pwsh.exe'),
            (Join-Path $root 'WindowsApps\Microsoft.PowerShell_*__8wekyb3d8bbwe\pwsh.exe')
        )

        foreach ($pattern in $patterns) {
            $matches = @(Get-ChildItem -Path $pattern -File -ErrorAction SilentlyContinue)
            foreach ($match in $matches) {
                if (-not [string]::IsNullOrWhiteSpace($match.FullName) -and -not $paths.Contains($match.FullName)) {
                    $paths.Add($match.FullName)
                }
            }
        }
    }

    $candidates = @(
        foreach ($path in $paths) {
            if (Test-Path -LiteralPath $path -PathType Leaf) {
                [pscustomobject]@{
                    Path = $path
                    Version = Get-PcvDesktopServicePwshCandidateVersion -Path $path
                    LastWriteTimeUtc = (Get-Item -LiteralPath $path).LastWriteTimeUtc
                }
            }
        }
    )

    $selected = $candidates |
        Sort-Object `
            @{ Expression = 'Version'; Descending = $true }, `
            @{ Expression = 'LastWriteTimeUtc'; Descending = $true }, `
            @{ Expression = 'Path'; Descending = $true } |
        Select-Object -First 1

    if ($null -ne $selected) {
        [string]$selected.Path
    }
}

function Resolve-PcvDesktopServicePwshPath {
    param(
        [string]$CommandName = 'pwsh.exe',
        [string[]]$SearchRoots
    )

    if ([System.IO.Path]::IsPathRooted($CommandName)) {
        return $CommandName
    }

    $commands = @(Get-Command $CommandName -All -ErrorAction SilentlyContinue |
        Where-Object {
            $_.CommandType -eq 'Application' -and
            -not [string]::IsNullOrWhiteSpace([string]$_.Source)
        })

    foreach ($command in $commands) {
        $source = [string]$command.Source
        if ($source -match '\\AppData\\Local\\Microsoft\\WindowsApps\\pwsh\.exe$') {
            continue
        }
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            return $source
        }
    }

    $programFilesPwsh = Find-PcvDesktopServicePwshInProgramFiles -SearchRoots $SearchRoots
    if (-not [string]::IsNullOrWhiteSpace($programFilesPwsh)) {
        return $programFilesPwsh
    }

    $CommandName
}

function New-PcvDesktopServiceConfig {
    param(
        [string]$ServiceName = 'PureCVisorDesktopNode',
        [string]$DisplayName = 'PureCVisor Desktop Node',
        [string]$Description = 'PureCVisor Desktop Node Local API service.',
        [string]$PwshPath = (Resolve-PcvDesktopServicePwshPath),
        [Parameter(Mandatory)][string]$ApiScriptPath,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$Prefix = 'http://127.0.0.1:7777/',
        [string]$HelperScriptPath,
        [string]$JobStorePath,
        [string]$WebRootPath,
        [string]$ApiToken,
        [string]$ApiTokenFile,
        [string]$ApiTokenProtectedFile,
        [switch]$AllowLan,
        [string]$EventLogPath,
        [switch]$EnsureFirewallRule,
        [string]$FirewallRuleName = 'PureCVisor Desktop Node API',
        [ValidateSet('private', 'domain', 'public', 'any')][string]$FirewallProfile = 'private',
        [ValidateRange(1, 64)][int]$WorkerCount = 1,
        [ValidateRange(1, 600)][int]$TimeoutSec = 30,
        [ValidateSet('auto', 'demand', 'disabled')][string]$StartupType = 'auto'
    )

    if ($ServiceName -notmatch '^[A-Za-z][A-Za-z0-9_.-]{0,79}$') {
        throw 'PCV_SERVICE_NAME_INVALID|The Windows service name is invalid.|Use 1-80 characters: letters, digits, underscore, dash, or dot, starting with a letter.'
    }

    if ([string]::IsNullOrWhiteSpace($ServiceAccount)) {
        throw 'PCV_SERVICE_ACCOUNT_REQUIRED|The Windows service account is required.|Use LocalSystem or a preconfigured service account.'
    }

    $prefixPolicy = Get-PcvServicePrefixExposure `
        -Prefix $Prefix `
        -AllowLan:$AllowLan `
        -ApiToken $ApiToken `
        -ApiTokenFile $ApiTokenFile `
        -ApiTokenProtectedFile $ApiTokenProtectedFile

    $listenerArguments = [System.Collections.Generic.List[string]]::new()
    $listenerArguments.Add('-NoProfile')
    $listenerArguments.Add('-ExecutionPolicy')
    $listenerArguments.Add('Bypass')
    $listenerArguments.Add('-File')
    $listenerArguments.Add($ApiScriptPath)
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-Prefix' -Value $Prefix
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-HelperScriptPath' -Value $HelperScriptPath
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-JobStorePath' -Value $JobStorePath
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-WebRootPath' -Value $WebRootPath
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-ApiToken' -Value $ApiToken
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-ApiTokenFile' -Value $ApiTokenFile
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-ApiTokenProtectedFile' -Value $ApiTokenProtectedFile
    if ($AllowLan) {
        $listenerArguments.Add('-AllowLan')
    }
    Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-EventLogPath' -Value $EventLogPath
    if ($EnsureFirewallRule) {
        $listenerArguments.Add('-EnsureFirewallRule')
        Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-FirewallRuleName' -Value $FirewallRuleName
        Add-PcvServiceListenerOption -Arguments $listenerArguments -Name '-FirewallProfile' -Value $FirewallProfile
    }
    $listenerArguments.Add('-WorkerCount')
    $listenerArguments.Add([string]$WorkerCount)
    $listenerArguments.Add('-TimeoutSec')
    $listenerArguments.Add([string]$TimeoutSec)

    $binaryPathParts = @(Quote-PcvServiceArgument -Value $PwshPath)
    foreach ($argument in $listenerArguments) {
        if ($argument.StartsWith('-')) {
            $binaryPathParts += $argument
        }
        elseif ($argument -match '^\d+$') {
            $binaryPathParts += $argument
        }
        else {
            $binaryPathParts += (Quote-PcvServiceArgument -Value $argument)
        }
    }

    [ordered]@{
        service_name = $ServiceName
        display_name = $DisplayName
        description = $Description
        service_account = $ServiceAccount
        startup_type = $StartupType
        prefix = $Prefix
        exposure = $prefixPolicy.exposure
        auth_required = $prefixPolicy.auth_required
        api_token_source = $prefixPolicy.api_token_source
        binary_path = ($binaryPathParts -join ' ')
    }
}

function New-PcvDesktopServiceCommand {
    param(
        [Parameter(Mandatory)]$Config,
        [ValidateSet('Install', 'Uninstall', 'Start', 'Stop', 'Restart', 'Status')][string]$Action
    )

    $serviceName = [string]$Config.service_name
    $commands = @()

    if ($Action -eq 'Install') {
        if ([string]$Config.api_token_source -eq 'inline') {
            throw 'PCV_SERVICE_INLINE_TOKEN_INSTALL_FORBIDDEN|Inline API tokens cannot be persisted in a Windows service binary path.|Prepare a token file or protected token file and pass -ApiTokenFile or -ApiTokenProtectedFile for service install.'
        }

        $createArguments = @(
            'create',
            $serviceName,
            'binPath=',
            [string]$Config.binary_path,
            'DisplayName=',
            [string]$Config.display_name,
            'start=',
            [string]$Config.startup_type,
            'obj=',
            [string]$Config.service_account
        )

        $commands = @(
            [ordered]@{
                file_name = 'sc.exe'
                arguments = $createArguments
            },
            [ordered]@{
                file_name = 'sc.exe'
                arguments = @('description', $serviceName, [string]$Config.description)
            },
            [ordered]@{
                file_name = 'sc.exe'
                arguments = @('failure', $serviceName, 'reset=', '86400', 'actions=', 'restart/60000/restart/60000/""/60000')
            }
        )
    }
    elseif ($Action -eq 'Uninstall') {
        $commands = @([ordered]@{ file_name = 'sc.exe'; arguments = @('delete', $serviceName) })
    }
    elseif ($Action -eq 'Start') {
        $commands = @([ordered]@{ file_name = 'sc.exe'; arguments = @('start', $serviceName) })
    }
    elseif ($Action -eq 'Stop') {
        $commands = @([ordered]@{ file_name = 'sc.exe'; arguments = @('stop', $serviceName) })
    }
    elseif ($Action -eq 'Restart') {
        $commands = @(
            [ordered]@{ file_name = 'sc.exe'; arguments = @('stop', $serviceName) },
            [ordered]@{ file_name = 'sc.exe'; arguments = @('start', $serviceName) }
        )
    }
    elseif ($Action -eq 'Status') {
        $commands = @([ordered]@{ file_name = 'sc.exe'; arguments = @('query', $serviceName) })
    }

    Write-Output -NoEnumerate $commands
}

function Invoke-PcvServiceNativeProcess {
    param(
        [Parameter(Mandatory)][string]$FileName,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $process.StartInfo.FileName = $FileName
    foreach ($argument in $Arguments) {
        [void]$process.StartInfo.ArgumentList.Add($argument)
    }
    $process.StartInfo.UseShellExecute = $false
    $process.StartInfo.RedirectStandardOutput = $true
    $process.StartInfo.RedirectStandardError = $true
    $process.StartInfo.CreateNoWindow = $true

    [void]$process.Start()
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    [ordered]@{
        exit_code = $process.ExitCode
        stdout = $stdout
        stderr = $stderr
    }
}

function Invoke-PcvTokenFileAclApply {
    param(
        [Parameter(Mandatory)][string]$Path,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$AdminPrincipal = 'BUILTIN\Administrators',
        [scriptblock]$InvokeProcess
    )

    $commands = New-PcvTokenFileAclCommand `
        -Path $Path `
        -ServiceAccount $ServiceAccount `
        -AdminPrincipal $AdminPrincipal
    $results = @()

    foreach ($command in $commands) {
        if ($null -eq $InvokeProcess) {
            $processResult = Invoke-PcvServiceNativeProcess `
                -FileName $command.file_name `
                -Arguments ([string[]]$command.arguments)
        }
        else {
            $processResult = & $InvokeProcess `
                -FileName $command.file_name `
                -Arguments ([string[]]$command.arguments)
        }

        $results += [ordered]@{
            file_name = $command.file_name
            arguments = @($command.arguments)
            action = $command.action
            exit_code = [int]$processResult.exit_code
            stdout = [string]$processResult.stdout
            stderr = [string]$processResult.stderr
        }

        if ([int]$processResult.exit_code -ne 0) {
            return [ordered]@{
                ok = $false
                operation = 'token_file.acl'
                path = $Path
                results = $results
                error = (New-PcvServiceError `
                    -Code 'PCV_SERVICE_TOKEN_ACL_FAILED' `
                    -Message 'Token file ACL application failed.' `
                    -Detail "Command '$($command.file_name) $($command.arguments -join ' ')' exited with code $($processResult.exit_code).")
            }
        }
    }

    [ordered]@{
        ok = $true
        operation = 'token_file.acl'
        path = $Path
        results = $results
        error = $null
    }
}

function New-PcvDesktopServiceTokenFile {
    param(
        [string]$Path = (Get-PcvDesktopServiceDefaultTokenFilePath),
        [AllowNull()][string]$Token,
        [ValidateRange(16, 128)][int]$TokenByteLength = 32,
        [string]$ServiceAccount = 'LocalSystem',
        [string]$AdminPrincipal = 'BUILTIN\Administrators',
        [switch]$Force,
        [scriptblock]$InvokeProcess
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw 'PCV_SERVICE_TOKEN_FILE_PATH_REQUIRED|The token file path is required.|Pass -ApiTokenFile or use the default ProgramData token path.'
    }

    if ($PSBoundParameters.ContainsKey('Token') -and [string]::IsNullOrWhiteSpace($Token)) {
        throw 'PCV_SERVICE_TOKEN_EMPTY|The service API token must not be empty.|Pass a non-empty token or omit -Token to generate one.'
    }

    if ((Test-Path -LiteralPath $Path -PathType Leaf) -and -not $Force) {
        throw "PCV_SERVICE_TOKEN_FILE_EXISTS|The service API token file already exists.|Pass -Force to rotate '$Path'."
    }

    $tokenValue = $Token
    if (-not $PSBoundParameters.ContainsKey('Token')) {
        $tokenValue = New-PcvDesktopServiceToken -ByteLength $TokenByteLength
    }

    $directory = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Set-Content -LiteralPath $Path -Value $tokenValue -Encoding UTF8 -NoNewline

    $aclResult = Invoke-PcvTokenFileAclApply `
        -Path $Path `
        -ServiceAccount $ServiceAccount `
        -AdminPrincipal $AdminPrincipal `
        -InvokeProcess $InvokeProcess

    [ordered]@{
        ok = $aclResult.ok
        path = $Path
        token_length = $tokenValue.Length
        service_account = $ServiceAccount
        service_acl_principal = (Resolve-PcvServiceAccountAclPrincipal -ServiceAccount $ServiceAccount)
        admin_principal = $AdminPrincipal
        acl = $aclResult
        error = $aclResult.error
    }
}

function Invoke-PcvDesktopServiceCommand {
    param(
        [Parameter(Mandatory)]$Config,
        [ValidateSet('Install', 'Uninstall', 'Start', 'Stop', 'Restart', 'Status')][string]$Action,
        [scriptblock]$InvokeProcess
    )

    $commands = New-PcvDesktopServiceCommand -Config $Config -Action $Action
    $results = @()

    foreach ($command in $commands) {
        if ($null -eq $InvokeProcess) {
            $processResult = Invoke-PcvServiceNativeProcess `
                -FileName $command.file_name `
                -Arguments ([string[]]$command.arguments)
        }
        else {
            $processResult = & $InvokeProcess `
                -FileName $command.file_name `
                -Arguments ([string[]]$command.arguments)
        }

        $results += [ordered]@{
            file_name = $command.file_name
            arguments = @($command.arguments)
            exit_code = [int]$processResult.exit_code
            stdout = [string]$processResult.stdout
            stderr = [string]$processResult.stderr
        }

        if ([int]$processResult.exit_code -ne 0) {
            return [ordered]@{
                ok = $false
                action = $Action.ToLowerInvariant()
                service_name = $Config.service_name
                results = $results
                error = (New-PcvServiceError `
                    -Code 'PCV_SERVICE_COMMAND_FAILED' `
                    -Message 'A Windows service command failed.' `
                    -Detail "Command '$($command.file_name) $($command.arguments -join ' ')' exited with code $($processResult.exit_code).")
            }
        }
    }

    [ordered]@{
        ok = $true
        action = $Action.ToLowerInvariant()
        service_name = $Config.service_name
        results = $results
        error = $null
    }
}

Export-ModuleMember -Function `
    Get-PcvDesktopServiceDefaultProtectedTokenFilePath, `
    Get-PcvDesktopServiceDefaultTokenFilePath, `
    Get-PcvServicePrefixExposure, `
    Get-PcvServiceApiTokenSource, `
    Invoke-PcvDesktopServiceCommand, `
    Invoke-PcvTokenFileAclApply, `
    New-PcvDesktopServiceToken, `
    New-PcvDesktopServiceProtectedTokenFile, `
    New-PcvDesktopServiceTokenFile, `
    New-PcvDesktopServiceCommand, `
    New-PcvDesktopServiceConfig, `
    New-PcvTokenFileAclCommand, `
    Read-PcvDesktopServiceProtectedTokenFile, `
    Remove-PcvDesktopServiceProtectedTokenFile, `
    Resolve-PcvDesktopServicePwshPath, `
    Resolve-PcvServiceAccountAclPrincipal
