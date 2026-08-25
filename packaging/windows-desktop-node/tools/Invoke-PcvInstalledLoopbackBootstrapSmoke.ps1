[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ('installed-loopback-bootstrap-smoke-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$WebBaseUri = 'http://127.0.0.1/',
    [string]$ApiBaseUri = '',
    [string]$AccountsPath = 'C:\ProgramData\PureCVisor\desktop-node\accounts.json',
    [switch]$SkipBrowser
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
    $Value | ConvertTo-Json -Depth 18 | Set-Content -LiteralPath $Path -Encoding utf8
}

function Get-JwtPayloadMap {
    param([Parameter(Mandatory)][string]$Token)

    $parts = $Token.Split('.')
    if ($parts.Count -lt 2) {
        throw 'PCV_LOOPBACK_SMOKE_INVALID_JWT'
    }
    $payload = $parts[1].Replace('-', '+').Replace('_', '/')
    switch ($payload.Length % 4) {
        2 { $payload += '==' }
        3 { $payload += '=' }
    }
    $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
    $json | ConvertFrom-Json
}

function Find-PcvBrowser {
    $candidates = @(
        $env:PCV_BROWSER_QA_CHROME
        'C:\Program Files\Microsoft\Edge\Application\msedge.exe'
        'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
        'C:\Program Files\Google\Chrome\Application\chrome.exe'
        'C:\Program Files (x86)\Google\Chrome\Application\chrome.exe'
    )
    foreach ($path in $candidates) {
        if (-not [string]::IsNullOrWhiteSpace($path) -and (Test-Path -LiteralPath $path -PathType Leaf)) {
            return $path
        }
    }
    throw 'PCV_LOOPBACK_SMOKE_BROWSER_MISSING'
}

function Get-PcvFreeLoopbackPort {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    $listener.Start()
    try {
        return ([System.Net.IPEndPoint]$listener.LocalEndpoint).Port
    }
    finally {
        $listener.Stop()
    }
}

function Invoke-PcvCdpEvaluate {
    param(
        [Parameter(Mandatory)][System.Net.WebSockets.ClientWebSocket]$Socket,
        [Parameter(Mandatory)][string]$Expression
    )

    $id = Get-Random -Minimum 1 -Maximum ([int]::MaxValue)
    $payload = @{
        id = $id
        method = 'Runtime.evaluate'
        params = @{
            expression = $Expression
            awaitPromise = $true
            returnByValue = $true
        }
    } | ConvertTo-Json -Compress -Depth 8
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($payload)
    $segment = [ArraySegment[byte]]::new($bytes)
    $Socket.SendAsync($segment, [System.Net.WebSockets.WebSocketMessageType]::Text, $true, [Threading.CancellationToken]::None).GetAwaiter().GetResult() | Out-Null

    $buffer = [byte[]]::new(65536)
    while ($true) {
        $memory = [System.IO.MemoryStream]::new()
        try {
            do {
                $receiveBuffer = [ArraySegment[byte]]::new($buffer)
                $result = $Socket.ReceiveAsync($receiveBuffer, [Threading.CancellationToken]::None).GetAwaiter().GetResult()
                $memory.Write($buffer, 0, $result.Count)
            } while (-not $result.EndOfMessage)
            $document = $memory.ToArray()
        }
        finally {
            $memory.Dispose()
        }
        $parsed = [System.Text.Encoding]::UTF8.GetString($document) | ConvertFrom-Json
        if ([int]$parsed.id -ne $id) {
            continue
        }
        if ($parsed.PSObject.Properties.Name -contains 'error') {
            throw ("CDP evaluate failed: " + ($parsed.error | ConvertTo-Json -Compress))
        }
        $inner = $parsed.result.result
        if ($inner.PSObject.Properties.Name -contains 'value') {
            return $inner.value
        }
        return $null
    }
}

function Invoke-PcvInstalledLoopbackBrowser {
    param([Parameter(Mandatory)][uri]$WebUri)

    $browser = Find-PcvBrowser
    $debugPort = Get-PcvFreeLoopbackPort
    $userData = Join-Path ([System.IO.Path]::GetTempPath()) ('pcv-loopback-installed-' + [guid]::NewGuid().ToString('N'))
    $null = New-Item -ItemType Directory -Path $userData
    $process = $null
    $socket = $null
    $snapshot = $null
    try {
        $start = [System.Diagnostics.ProcessStartInfo]::new()
        $start.FileName = $browser
        $start.Arguments = @(
            '--headless=new'
            '--disable-gpu'
            '--disable-extensions'
            '--no-first-run'
            '--no-default-browser-check'
            "--remote-debugging-port=$debugPort"
            "--user-data-dir=`"$userData`""
            'about:blank'
        ) -join ' '
        $start.UseShellExecute = $false
        $start.CreateNoWindow = $true
        $process = [System.Diagnostics.Process]::Start($start)
        if ($null -eq $process) {
            throw 'PCV_LOOPBACK_SMOKE_BROWSER_START_FAILED'
        }

        $deadline = [datetime]::UtcNow.AddSeconds(20)
        $ready = $false
        while ([datetime]::UtcNow -lt $deadline) {
            try {
                $version = Invoke-WebRequest -UseBasicParsing -TimeoutSec 2 -Uri ("http://127.0.0.1:{0}/json/version" -f $debugPort)
                if ([int]$version.StatusCode -eq 200) {
                    $ready = $true
                    break
                }
            }
            catch {
            }
            Start-Sleep -Milliseconds 150
        }
        if (-not $ready) {
            throw "PCV_LOOPBACK_SMOKE_DEVTOOLS_TIMEOUT|$debugPort"
        }

        $created = Invoke-WebRequest -UseBasicParsing -Method PUT -TimeoutSec 10 -Uri (
            "http://127.0.0.1:{0}/json/new?{1}" -f $debugPort, [uri]::EscapeDataString($WebUri.AbsoluteUri))
        $createdJson = $created.Content | ConvertFrom-Json
        $socket = [System.Net.WebSockets.ClientWebSocket]::new()
        $socket.ConnectAsync([uri]$createdJson.webSocketDebuggerUrl, [Threading.CancellationToken]::None).GetAwaiter().GetResult()

        $snapshotDeadline = [datetime]::UtcNow.AddSeconds(25)
        $last = $null
        while ([datetime]::UtcNow -lt $snapshotDeadline) {
            $last = Invoke-PcvCdpEvaluate -Socket $socket -Expression @'
(() => ({
  connection: document.querySelector('#connection-state')?.textContent || '',
  statusVm: document.querySelector('#status-vm-count')?.textContent || '',
  authGate: Boolean(document.querySelector('[data-auth-gate]')),
  hasAccessToken: Boolean((sessionStorage.getItem('pcvDesktopAccountSession.v1') || '').includes('access_token')),
  bodyHasAuthRequired: /Auth required/i.test(document.body?.innerText || ''),
  bodyHasFixtureVm: (document.body?.innerText || '').includes('pcv-node-a')
}))()
'@
            if ($last.hasAccessToken -and -not $last.authGate -and -not $last.bodyHasAuthRequired) {
                $snapshot = [ordered]@{
                    browser = $browser
                    connection = [string]$last.connection
                    status_vm = [string]$last.statusVm
                    auth_gate = [bool]$last.authGate
                    has_access_token = [bool]$last.hasAccessToken
                    body_has_auth_required = [bool]$last.bodyHasAuthRequired
                    body_has_fixture_vm = [bool]$last.bodyHasFixtureVm
                }
                break
            }
            Start-Sleep -Milliseconds 200
        }
        if ($null -eq $snapshot) {
            throw ("PCV_LOOPBACK_SMOKE_BROWSER_BOOTSTRAP_TIMEOUT|" + ($last | ConvertTo-Json -Compress))
        }
    }
    finally {
        if ($null -ne $socket) {
            try { $socket.Dispose() } catch { }
        }
        if ($null -ne $process -and -not $process.HasExited) {
            try { $process.Kill($true) } catch { }
        }
        if ($null -ne $process) {
            $process.Dispose()
        }
        try { Remove-Item -LiteralPath $userData -Recurse -Force } catch { }
    }
    return $snapshot
}

$webUri = [uri]$WebBaseUri
$configResponse = Invoke-WebRequest -UseBasicParsing -TimeoutSec 15 -Uri ([uri]::new($webUri, '/pcv-config.js'))
$rootResponse = Invoke-WebRequest -UseBasicParsing -TimeoutSec 15 -Uri $webUri
$configText = [string]$configResponse.Content
if ($configText -notmatch '"apiBaseUrl"\s*:\s*"(?<url>http://127\.0\.0\.1(?::\d+)?)"') {
    throw 'PCV_LOOPBACK_SMOKE_CONFIG_API_BASE_MISSING'
}
$resolvedApi = if ([string]::IsNullOrWhiteSpace($ApiBaseUri)) { $Matches.url } else { $ApiBaseUri.TrimEnd('/') }
$configHasTokenLiteral = $configText -match 'access_token|protected_token|Bearer '
$rootHasTokenLiteral = [string]$rootResponse.Content -match 'access_token|protected_token'

$accountCount = $null
$bootstrapState = $null
$accountsReadable = $false
if (Test-Path -LiteralPath $AccountsPath -PathType Leaf) {
    $accounts = Get-Content -Raw -LiteralPath $AccountsPath | ConvertFrom-Json
    $accountCount = @($accounts.accounts).Count
    $bootstrapState = [string]$accounts.bootstrap_state
    $accountsReadable = $true
}

$issue = Invoke-WebRequest -UseBasicParsing -Method POST -TimeoutSec 15 -Uri ($resolvedApi.TrimEnd('/') + '/api/v1/auth/loopback-session')
$issueJson = $issue.Content | ConvertFrom-Json
$accessToken = [string]$issueJson.data.access_token
if ([string]::IsNullOrWhiteSpace($accessToken)) {
    throw 'PCV_LOOPBACK_SMOKE_ACCESS_TOKEN_MISSING'
}
$jwt = Get-JwtPayloadMap -Token $accessToken
$sessionHeaders = @{ Authorization = ('Bearer ' + $accessToken) }
$sessionResponse = Invoke-WebRequest -UseBasicParsing -TimeoutSec 15 -Headers $sessionHeaders -Uri ($resolvedApi.TrimEnd('/') + '/api/v1/auth/session')
$policyUnauth = $null
try {
    Invoke-WebRequest -UseBasicParsing -TimeoutSec 15 -Uri ($resolvedApi.TrimEnd('/') + '/api/v1/runtime/policy') | Out-Null
    $policyUnauth = 200
}
catch {
    if ($_.Exception.Response) {
        $policyUnauth = [int]$_.Exception.Response.StatusCode
    }
    else {
        throw
    }
}
$policyAuth = Invoke-WebRequest -UseBasicParsing -TimeoutSec 15 -Headers $sessionHeaders -Uri ($resolvedApi.TrimEnd('/') + '/api/v1/runtime/policy')

$browserResult = $null
if (-not $SkipBrowser) {
    $browserOutput = @(Invoke-PcvInstalledLoopbackBrowser -WebUri $webUri)
    foreach ($item in $browserOutput) {
        if ($item -is [System.Collections.IDictionary] -and $item.Contains('has_access_token')) {
            $browserResult = $item
        }
    }
    if ($null -eq $browserResult) {
        $kinds = @($browserOutput | ForEach-Object { $_.GetType().FullName }) -join ','
        throw "PCV_LOOPBACK_SMOKE_BROWSER_RESULT_MISSING|$kinds"
    }
}

$ok = (
    [int]$configResponse.StatusCode -eq 200 -and
    [int]$rootResponse.StatusCode -eq 200 -and
    -not $configHasTokenLiteral -and
    -not $rootHasTokenLiteral -and
    [bool]$issueJson.ok -and
    [string]$issueJson.operation -eq 'auth.loopback-session' -and
    [string]$issueJson.data.grant_type -eq 'loopback_session' -and
    [string]$issueJson.data.session.username -eq 'loopback-session' -and
    [string]$issueJson.data.session.role -eq 'operator' -and
    [string]$jwt.typ -eq 'loopback_access' -and
    [int]$sessionResponse.StatusCode -eq 200 -and
    [int]$policyUnauth -eq 401 -and
    [int]$policyAuth.StatusCode -eq 200 -and
    (-not $accountsReadable -or ($accountCount -eq 0 -and $bootstrapState -eq 'no-default-account')) -and
    ($SkipBrowser -or (
        $null -ne $browserResult -and
        [bool]$browserResult['has_access_token'] -and
        -not [bool]$browserResult['auth_gate'] -and
        -not [bool]$browserResult['body_has_auth_required'] -and
        -not [bool]$browserResult['body_has_fixture_vm']
    ))
)

$summary = [ordered]@{
    schema_version = 1
    ok = [bool]$ok
    evidence_id = 'installed-loopback-bootstrap-smoke-2026-08-14-04273'
    web_base_uri = $webUri.AbsoluteUri
    api_base_uri = $resolvedApi
    config_status_code = [int]$configResponse.StatusCode
    root_status_code = [int]$rootResponse.StatusCode
    config_has_token_literal = [bool]$configHasTokenLiteral
    root_has_token_literal = [bool]$rootHasTokenLiteral
    loopback_status_code = [int]$issue.StatusCode
    loopback_operation = [string]$issueJson.operation
    grant_type = [string]$issueJson.data.grant_type
    session_username = [string]$issueJson.data.session.username
    session_role = [string]$issueJson.data.session.role
    jwt_typ = [string]$jwt.typ
    access_token_present = $true
    refresh_token_present = -not [string]::IsNullOrWhiteSpace([string]$issueJson.data.refresh_token)
    session_status_code = [int]$sessionResponse.StatusCode
    unauthenticated_policy_status_code = [int]$policyUnauth
    authenticated_policy_status_code = [int]$policyAuth.StatusCode
    accounts_readable = [bool]$accountsReadable
    account_count = $accountCount
    bootstrap_state = $bootstrapState
    browser = $browserResult
    token_value_observed = $false
    host_mutation_performed = $false
    public_trusted_signing = 'not-claimed'
    external_stable_publication = 'not-claimed'
    generated_at = (Get-Date).ToString('o')
}
$summaryPath = Join-Path $artifactRootFull 'summary.json'
Write-JsonFile -Path $summaryPath $summary
if (-not $ok) {
    throw ("PCV_LOOPBACK_SMOKE_FAILED|" + $summaryPath)
}
[pscustomobject]$summary
