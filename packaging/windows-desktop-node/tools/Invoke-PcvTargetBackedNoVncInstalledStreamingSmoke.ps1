[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("target-backed-novnc-installed-streaming-smoke-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [string]$ApiBaseUri = 'http://127.0.0.1:7777',
    [string]$TokenProtectedFile = 'C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json',
    [string]$NoVncWebSocketPath = '/api/v1/console/novnc/{vm_id}',
    [string]$VmId = 'pcv-novnc-smoke',
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

$modulePath = Join-Path (Split-Path -Parent $PSScriptRoot) 'PcvDesktopNodeProduct.psm1'
Import-Module $modulePath -Force

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
            throw "PCV_NOVNC_SMOKE_SERVICE_STOP_TIMEOUT|Service did not stop.|$Name"
        }
    }

    Start-Service -Name $Name -ErrorAction Stop
    if (-not (Wait-ServiceState -Name $Name -Expected 'Running')) {
        throw "PCV_NOVNC_SMOKE_SERVICE_START_TIMEOUT|Service did not start.|$Name"
    }
}

function Set-ServicePath {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$PathName
    )

    $output = & sc.exe config $Name binPath= $PathName 2>&1
    $exitCode = $LASTEXITCODE
    [pscustomobject][ordered]@{
        ok = ($exitCode -eq 0)
        exit_code = $exitCode
        stdout = ($output -join [Environment]::NewLine)
    }
}

function Get-InstalledServicePath {
    param([Parameter(Mandatory)][string]$Name)

    $service = Get-CimInstance Win32_Service -Filter "Name='$Name'" -ErrorAction Stop
    if ($null -eq $service -or [string]::IsNullOrWhiteSpace([string]$service.PathName)) {
        throw "PCV_NOVNC_SMOKE_SERVICE_PATH_MISSING|Installed service PathName was not found.|$Name"
    }

    [string]$service.PathName
}

function Get-Sha256Hex {
    param([Parameter(Mandatory)][byte[]]$Bytes)

    $hash = [System.Security.Cryptography.SHA256]::HashData($Bytes)
    ([System.BitConverter]::ToString($hash)).Replace('-', '').ToLowerInvariant()
}

function Test-ByteArrayEqual {
    param(
        [AllowNull()][byte[]]$Left,
        [AllowNull()][byte[]]$Right
    )

    if ($null -eq $Left -or $null -eq $Right -or $Left.Length -ne $Right.Length) {
        return $false
    }

    for ($index = 0; $index -lt $Left.Length; $index++) {
        if ($Left[$index] -ne $Right[$index]) {
            return $false
        }
    }

    return $true
}

function Receive-ExactBytes {
    param(
        [Parameter(Mandatory)][System.IO.Stream]$Stream,
        [Parameter(Mandatory)][int]$Length
    )

    $buffer = [byte[]]::new($Length)
    $offset = 0
    while ($offset -lt $Length) {
        $read = $Stream.Read($buffer, $offset, $Length - $offset)
        if ($read -le 0) {
            throw "PCV_NOVNC_SMOKE_TARGET_READ_EOF|Target TCP stream closed before all bytes were read.|read=$offset expected=$Length"
        }
        $offset += $read
    }

    $buffer
}

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$started = Get-Date
$ok = $false
$errorRecord = $null
$originalPathName = $null
$novncPathName = $null
$pathRestored = $false
$listener = $null
$tcpClient = $null
$webSocket = $null
$targetPort = $null
$bytesSent = $null
$bytesEchoed = $null
$webSocketReceiveCount = $null
$setNoVncPathResult = $null
$restorePathResult = $null
$finalService = $null
$tokenSource = 'protected-token-file'

try {
    $originalPathName = Get-InstalledServicePath -Name $ServiceName
    if ($DryRun) {
        $ok = $true
        return
    }

    $tokenResult = Read-PcvDesktopNodeProductProtectedTokenFile -Path $TokenProtectedFile
    $token = [string]$tokenResult.token
    if ([string]::IsNullOrWhiteSpace($token)) {
        throw "PCV_NOVNC_SMOKE_TOKEN_EMPTY|Protected token file resolved to an empty token.|$TokenProtectedFile"
    }

    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $targetPort = [int]([System.Net.IPEndPoint]$listener.LocalEndpoint).Port

    $novncPathName = $originalPathName +
        ' --novnc-target-host "127.0.0.1"' +
        " --novnc-target-port $targetPort" +
        " --novnc-websocket-path `"$NoVncWebSocketPath`""

    $setNoVncPathResult = Set-ServicePath -Name $ServiceName -PathName $novncPathName
    if (-not [bool]$setNoVncPathResult.ok) {
        throw "PCV_NOVNC_SMOKE_SERVICE_CONFIG_FAILED|Failed to configure installed service PathName.|$($setNoVncPathResult.stdout)"
    }

    Restart-SmokeService -Name $ServiceName

    $webSocket = [System.Net.WebSockets.ClientWebSocket]::new()
    $webSocket.Options.SetRequestHeader('Authorization', "Bearer $token")
    $wsUri = [Uri](($ApiBaseUri.TrimEnd('/')).Replace('http://', 'ws://').Replace('https://', 'wss://') + $NoVncWebSocketPath.Replace('{vm_id}', [Uri]::EscapeDataString($VmId)))
    $connectTask = $webSocket.ConnectAsync($wsUri, [Threading.CancellationToken]::None)
    if (-not $connectTask.Wait([TimeSpan]::FromSeconds(20))) {
        throw "PCV_NOVNC_SMOKE_WEBSOCKET_CONNECT_TIMEOUT|Timed out connecting to noVNC WebSocket.|$wsUri"
    }

    $tcpClient = $listener.AcceptTcpClient()
    $stream = $tcpClient.GetStream()

    $bytesSent = [System.Text.Encoding]::UTF8.GetBytes("pcv-target-backed-novnc-installed-streaming-smoke")
    $sendTask = $webSocket.SendAsync(
        [ArraySegment[byte]]::new($bytesSent),
        [System.Net.WebSockets.WebSocketMessageType]::Binary,
        $true,
        [Threading.CancellationToken]::None)
    if (-not $sendTask.Wait([TimeSpan]::FromSeconds(20))) {
        throw "PCV_NOVNC_SMOKE_WEBSOCKET_SEND_TIMEOUT|Timed out sending WebSocket smoke frame.|$wsUri"
    }

    $targetReceived = Receive-ExactBytes -Stream $stream -Length $bytesSent.Length
    if (-not (Test-ByteArrayEqual -Left $bytesSent -Right $targetReceived)) {
        throw "PCV_NOVNC_SMOKE_TARGET_BYTES_MISMATCH|Target TCP server did not receive the expected frame.|length=$($bytesSent.Length)"
    }

    $stream.Write($targetReceived, 0, $targetReceived.Length)
    $stream.Flush()

    $receiveBuffer = [byte[]]::new(4096)
    $receiveTask = $webSocket.ReceiveAsync([ArraySegment[byte]]::new($receiveBuffer), [Threading.CancellationToken]::None)
    if (-not $receiveTask.Wait([TimeSpan]::FromSeconds(20))) {
        throw "PCV_NOVNC_SMOKE_WEBSOCKET_RECEIVE_TIMEOUT|Timed out receiving echoed WebSocket frame.|$wsUri"
    }
    $receiveResult = $receiveTask.Result
    $webSocketReceiveCount = [int]$receiveResult.Count
    $bytesEchoed = if ($webSocketReceiveCount -le 0) { [byte[]]::new(0) } else { [byte[]]$receiveBuffer[0..($webSocketReceiveCount - 1)] }
    if ($webSocketReceiveCount -ne $bytesSent.Length -or -not (Test-ByteArrayEqual -Left $bytesSent -Right $bytesEchoed)) {
        throw "PCV_NOVNC_SMOKE_WEBSOCKET_BYTES_MISMATCH|WebSocket echo frame did not match the target-backed frame.|received=$webSocketReceiveCount expected=$($bytesSent.Length)"
    }

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
    if ($null -ne $webSocket) {
        try { $webSocket.Dispose() } catch {}
    }
    if ($null -ne $tcpClient) {
        try { $tcpClient.Dispose() } catch {}
    }
    if ($null -ne $listener) {
        try { $listener.Stop() } catch {}
    }

    if (-not $DryRun -and -not [string]::IsNullOrWhiteSpace($originalPathName)) {
        try {
            $restorePathResult = Set-ServicePath -Name $ServiceName -PathName $originalPathName
            if ([bool]$restorePathResult.ok) {
                Restart-SmokeService -Name $ServiceName
                $pathRestored = ((Get-InstalledServicePath -Name $ServiceName) -eq $originalPathName)
            }
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

    try {
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        $finalService = if ($null -eq $service) {
            [ordered]@{ exists = $false; status = $null }
        } else {
            [ordered]@{ exists = $true; status = [string]$service.Status }
        }
    }
    catch {
        $finalService = [ordered]@{ exists = $false; status = 'unknown' }
    }

    $summary = [ordered]@{
        schema_version = 1
        ok = [bool]$ok
        target_backed_novnc_installed_streaming_smoke = if ($DryRun) { 'dry-run' } elseif ($ok) { 'pass' } else { 'failed' }
        actual_execution = if ($DryRun) { 'dry-run-no-mutation' } else { 'installed-service-target-backed-novnc-streaming-smoke' }
        service_name = $ServiceName
        api_base_uri = $ApiBaseUri
        websocket_path = $NoVncWebSocketPath
        vm_id = $VmId
        target_host = '127.0.0.1'
        target_port = $targetPort
        token_source = $tokenSource
        token_value_observed = $false
        password_value_observed = $false
        target_frame_length = $(if ($null -eq $bytesSent) { $null } else { $bytesSent.Length })
        target_frame_sha256 = $(if ($null -eq $bytesSent) { $null } else { Get-Sha256Hex -Bytes $bytesSent })
        echoed_frame_length = $webSocketReceiveCount
        echoed_frame_sha256 = $(if ($null -eq $bytesEchoed) { $null } else { Get-Sha256Hex -Bytes $bytesEchoed })
        path_name_restored = [bool]$pathRestored
        original_path_name = '[REDACTED_PATHNAME]'
        novnc_path_name = '[REDACTED_PATHNAME]'
        set_novnc_path_result = $setNoVncPathResult
        restore_path_result = $restorePathResult
        final_service = $finalService
        host_mutation_performed = -not [bool]$DryRun
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        started_at = $started.ToString('o')
        completed_at = (Get-Date).ToString('o')
        error = $errorRecord
    }
    Write-JsonFile -Path $summaryPath -Value $summary
}
