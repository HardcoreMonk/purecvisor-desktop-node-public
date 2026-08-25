[CmdletBinding()]
param(
    [string]$ArtifactRoot = (Join-Path 'artifacts' ("internal-https-tls-lifecycle-installed-" + (Get-Date -Format 'yyyyMMdd-HHmmss'))),
    [string]$ServiceName = 'PureCVisorDesktopNode',
    [string]$HttpsPrefix = 'https://127.0.0.1:7443/',
    [string]$HttpRestoreUri = 'http://127.0.0.1:7777/api/v1/runtime/policy',
    [string]$ProtectedTokenPath = 'C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json'
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
        [int]$TimeoutSeconds = 120
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
        try { $process.Kill($true) } catch {}
        throw "PCV_PROCESS_TIMEOUT|Process timed out.|$FileName $($Arguments -join ' ')"
    }

    [pscustomobject][ordered]@{
        file_name = $FileName
        arguments = @($Arguments)
        exit_code = [int]$process.ExitCode
        stdout = $stdoutTask.GetAwaiter().GetResult()
        stderr = $stderrTask.GetAwaiter().GetResult()
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
    param([Parameter(Mandatory)][string]$Name)
    Get-CimInstance Win32_Service -Filter "Name='$Name'" -ErrorAction SilentlyContinue
}

function Assert-ProtectedFileTokenSource {
    param(
        [Parameter(Mandatory)]$Service,
        [Parameter(Mandatory)][string]$ExpectedProtectedTokenPath
    )

    $pathName = [string]$Service.PathName
    Assert-True -Condition ($pathName.Contains('--api-token-protected-file', [System.StringComparison]::OrdinalIgnoreCase)) -Message 'PCV_TLS_SMOKE_TOKEN_SOURCE_MISMATCH|TLS lifecycle requires a protected-file token source before binding mutation.'
    Assert-True -Condition (-not $pathName.Contains('--api-token-credential-target', [System.StringComparison]::OrdinalIgnoreCase)) -Message 'PCV_TLS_SMOKE_TOKEN_SOURCE_MISMATCH|TLS lifecycle refuses to use a Credential Manager service token source with a protected-token smoke.'
    Assert-True -Condition ($pathName.Contains($ExpectedProtectedTokenPath, [System.StringComparison]::OrdinalIgnoreCase)) -Message 'PCV_TLS_SMOKE_TOKEN_SOURCE_PATH_MISMATCH|Service PathName protected token path does not match the declared smoke baseline.'
}

function Wait-ServiceState {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Expected,
        [int]$TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $service = Get-ServiceInfo -Name $Name
        if ($null -ne $service -and [string]$service.State -eq $Expected) {
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
        Assert-True -Condition (Wait-ServiceState -Name $Name -Expected 'Stopped' -TimeoutSeconds 90) -Message "PCV_TLS_SMOKE_SERVICE_STOP_TIMEOUT|Service did not stop.|$Name"
    }

    Start-Service -Name $Name -ErrorAction Stop
    Assert-True -Condition (Wait-ServiceState -Name $Name -Expected 'Running' -TimeoutSeconds 90) -Message "PCV_TLS_SMOKE_SERVICE_START_TIMEOUT|Service did not start.|$Name"
}

function Read-ProtectedTokenValue {
    param([Parameter(Mandatory)][string]$Path)

    $modulePath = Join-Path $repoRoot 'packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1'
    Import-Module $modulePath -Force
    $record = Read-PcvDesktopNodeProductProtectedTokenFile -Path $Path
    [string]$record['token']
}

function Invoke-PolicyRequest {
    param(
        [Parameter(Mandatory)][string]$Uri,
        [Parameter(Mandatory)][string]$Token
    )

    $requestArgs = @{
        Uri = $Uri
        Headers = @{ Authorization = "Bearer $Token" }
        TimeoutSec = 20
        ErrorAction = 'Stop'
    }
    if ((Get-Command Invoke-WebRequest).Parameters.ContainsKey('UseBasicParsing')) {
        $requestArgs.UseBasicParsing = $true
    }
    if ($Uri.StartsWith('https://', [System.StringComparison]::OrdinalIgnoreCase) -and
        (Get-Command Invoke-WebRequest).Parameters.ContainsKey('SkipCertificateCheck')) {
        $requestArgs.SkipCertificateCheck = $true
    }

    $response = Invoke-WebRequest @requestArgs
    $body = $response.Content | ConvertFrom-Json
    $data = $null
    if ($body.PSObject.Properties.Name -contains 'data') {
        $data = $body.data
    }

    $tokenStorage = $null
    if ($null -ne $data -and
        $data.PSObject.Properties.Name -contains 'auth' -and
        $null -ne $data.auth -and
        $data.auth.PSObject.Properties.Name -contains 'token_storage') {
        $tokenStorage = [string]$data.auth.token_storage
    }

    $routePrefix = $null
    if ($null -ne $data -and
        $data.PSObject.Properties.Name -contains 'routes' -and
        $null -ne $data.routes -and
        $data.routes.PSObject.Properties.Name -contains 'prefix') {
        $routePrefix = [string]$data.routes.prefix
    }

    [pscustomobject][ordered]@{
        status_code = [int]$response.StatusCode
        token_storage = $tokenStorage
        route_prefix = $routePrefix
    }
}

function New-SmokeCertificate {
    param([Parameter(Mandatory)][string]$Label)

    $subject = "CN=PureCVisor Desktop Node Internal HTTPS Smoke $Label"
    New-SelfSignedCertificate `
        -Subject $subject `
        -DnsName 'localhost' `
        -CertStoreLocation 'Cert:\LocalMachine\My' `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -KeyExportPolicy NonExportable `
        -NotAfter (Get-Date).AddDays(7)
}

function Remove-SmokeCertificate {
    param([Parameter(Mandatory)][string]$Thumbprint)

    $path = "Cert:\LocalMachine\My\$Thumbprint"
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

function Get-BindingPort {
    param([Parameter(Mandatory)][string]$Prefix)

    $uri = [System.Uri]::new($Prefix)
    if ($uri.Host -notin @('127.0.0.1', 'localhost')) {
        throw "PCV_TLS_SMOKE_UNSUPPORTED_HOST|Only loopback HTTPS smoke is supported.|$Prefix"
    }
    "127.0.0.1:$($uri.Port)"
}

function Remove-SslBinding {
    param([Parameter(Mandatory)][string]$IpPort)

    $result = Invoke-CapturedProcess -FileName 'netsh.exe' -Arguments @('http', 'delete', 'sslcert', "ipport=$IpPort") -TimeoutSeconds 30
    $result
}

function Add-SslBinding {
    param(
        [Parameter(Mandatory)][string]$IpPort,
        [Parameter(Mandatory)][string]$Thumbprint,
        [Parameter(Mandatory)][string]$AppId
    )

    $result = Invoke-CapturedProcess -FileName 'netsh.exe' -Arguments @('http', 'add', 'sslcert', "ipport=$IpPort", "certhash=$Thumbprint", "appid={$AppId}", 'certstorename=MY') -TimeoutSeconds 30
    Assert-True -Condition ([bool]$result.ok) -Message "PCV_TLS_SMOKE_SSL_BIND_FAILED|HTTP.sys SSL binding failed.|$($result.stderr)$($result.stdout)"
    $result
}

function Set-ServicePath {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$PathName
    )

    $result = Invoke-CapturedProcess -FileName 'sc.exe' -Arguments @('config', $Name, 'binPath=', $PathName) -TimeoutSeconds 30
    Assert-True -Condition ([bool]$result.ok) -Message "PCV_TLS_SMOKE_SERVICE_CONFIG_FAILED|Failed to set service PathName.|$($result.stderr)$($result.stdout)"
    $result
}

$summaryPath = Join-Path $artifactRootFull 'summary.json'
$certificatesPath = Join-Path $artifactRootFull 'certificates.json'
$servicePath = Join-Path $artifactRootFull 'service.json'
$bindingPath = Join-Path $artifactRootFull 'binding.json'
$healthPath = Join-Path $artifactRootFull 'health.json'
$cleanupPath = Join-Path $artifactRootFull 'cleanup.json'

$ipPort = Get-BindingPort -Prefix $HttpsPrefix
$httpsPolicyUri = ($HttpsPrefix.TrimEnd('/') + '/api/v1/runtime/policy')
$appId = [guid]::NewGuid().ToString()
$createdCerts = New-Object System.Collections.Generic.List[string]
$originalPathName = $null
$restored = $false

$summary = [ordered]@{
    ok = $false
    scope = 'internal-https-tls-lifecycle-installed'
    actual_execution = 'installed-service-https-binding-cert-rotation-remove-smoke'
    artifact_root = $artifactRootFull
    service_name = $ServiceName
    host_mutation_performed = $true
    public_trusted_signing = 'out-of-scope'
    external_stable_publication = 'out-of-scope'
    token_value_observed = $false
    https_prefix = $HttpsPrefix
    http_restore_uri = $HttpRestoreUri
    ssl_ipport = $ipPort
    certificate_lifecycle = $null
    https_initial = $null
    https_rotated = $null
    cleanup = $null
    final_service = $null
    evidence_paths = [ordered]@{
        certificates = $certificatesPath
        service = $servicePath
        binding = $bindingPath
        health = $healthPath
        cleanup = $cleanupPath
    }
}

try {
    $principal = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
    Assert-True -Condition ($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) -Message 'PCV_TLS_SMOKE_ADMIN_REQUIRED|Internal HTTPS/TLS smoke requires an elevated shell.'

    $service = Get-ServiceInfo -Name $ServiceName
    Assert-True -Condition ($null -ne $service) -Message "PCV_TLS_SMOKE_SERVICE_NOT_FOUND|Service was not found.|$ServiceName"
    $originalPathName = [string]$service.PathName
    Assert-True -Condition ($originalPathName -match '--prefix\s+"[^"]+"') -Message 'PCV_TLS_SMOKE_PREFIX_NOT_FOUND|Installed service PathName does not include a quoted --prefix argument.'
    Assert-ProtectedFileTokenSource -Service $service -ExpectedProtectedTokenPath $ProtectedTokenPath

    $token = Read-ProtectedTokenValue -Path $ProtectedTokenPath
    Assert-True -Condition (-not [string]::IsNullOrWhiteSpace($token)) -Message 'PCV_TLS_SMOKE_TOKEN_EMPTY|Protected token value was empty.'

    $initialCert = New-SmokeCertificate -Label 'initial'
    [void]$createdCerts.Add($initialCert.Thumbprint)
    [void](Remove-SslBinding -IpPort $ipPort)
    $initialBinding = Add-SslBinding -IpPort $ipPort -Thumbprint $initialCert.Thumbprint -AppId $appId

    $httpsPathName = [regex]::Replace($originalPathName, '--prefix\s+"[^"]+"', ('--prefix "' + $HttpsPrefix + '"'), 1)
    $setHttpsPath = Set-ServicePath -Name $ServiceName -PathName $httpsPathName
    Restart-SmokeService -Name $ServiceName
    $httpsInitial = Invoke-PolicyRequest -Uri $httpsPolicyUri -Token $token
    Assert-True -Condition ([int]$httpsInitial.status_code -eq 200) -Message 'PCV_TLS_SMOKE_HTTPS_INITIAL_FAILED|Initial HTTPS runtime policy did not return 200.'

    $rotatedCert = New-SmokeCertificate -Label 'rotated'
    [void]$createdCerts.Add($rotatedCert.Thumbprint)
    [void](Remove-SslBinding -IpPort $ipPort)
    $rotatedBinding = Add-SslBinding -IpPort $ipPort -Thumbprint $rotatedCert.Thumbprint -AppId $appId
    Restart-SmokeService -Name $ServiceName
    $httpsRotated = Invoke-PolicyRequest -Uri $httpsPolicyUri -Token $token
    Assert-True -Condition ([int]$httpsRotated.status_code -eq 200) -Message 'PCV_TLS_SMOKE_HTTPS_ROTATED_FAILED|Rotated HTTPS runtime policy did not return 200.'

    $summary.certificate_lifecycle = 'generate-bind-rotate-remove-pass'
    $summary.https_initial = $httpsInitial
    $summary.https_rotated = $httpsRotated

    Write-JsonFile -Path $certificatesPath -Value ([ordered]@{
        initial_thumbprint = $initialCert.Thumbprint
        rotated_thumbprint = $rotatedCert.Thumbprint
        private_key_exported = $false
        private_key_material_recorded = $false
    })
    Write-JsonFile -Path $servicePath -Value ([ordered]@{
        original_path_name = $originalPathName
        https_path_name = $httpsPathName
        set_https_path = $setHttpsPath
    })
    Write-JsonFile -Path $bindingPath -Value ([ordered]@{
        ipport = $ipPort
        appid = $appId
        initial_binding = $initialBinding
        rotated_binding = $rotatedBinding
    })
    Write-JsonFile -Path $healthPath -Value ([ordered]@{
        initial = $httpsInitial
        rotated = $httpsRotated
    })
}
finally {
    $cleanup = [ordered]@{
        restore_service_path_attempted = $false
        restore_service_path_ok = $false
        delete_ssl_binding_attempted = $false
        delete_ssl_binding_exit_code = $null
        removed_certificates = @()
        final_http_restore = $null
    }

    if (-not [string]::IsNullOrWhiteSpace($originalPathName)) {
        $cleanup.restore_service_path_attempted = $true
        try {
            [void](Set-ServicePath -Name $ServiceName -PathName $originalPathName)
            Restart-SmokeService -Name $ServiceName
            $cleanup.restore_service_path_ok = $true
            $restored = $true
        }
        catch {
            $cleanup.restore_service_path_error = $_.Exception.Message
        }
    }

    try {
        $cleanup.delete_ssl_binding_attempted = $true
        $deleteBinding = Remove-SslBinding -IpPort $ipPort
        $cleanup.delete_ssl_binding_exit_code = [int]$deleteBinding.exit_code
    }
    catch {
        $cleanup.delete_ssl_binding_error = $_.Exception.Message
    }

    foreach ($thumbprint in $createdCerts) {
        try {
            Remove-SmokeCertificate -Thumbprint $thumbprint
            $cleanup.removed_certificates += $thumbprint
        }
        catch {
            $cleanup.remove_certificate_error = $_.Exception.Message
        }
    }

    if ($restored) {
        try {
            $token = Read-ProtectedTokenValue -Path $ProtectedTokenPath
            $cleanup.final_http_restore = Invoke-PolicyRequest -Uri $HttpRestoreUri -Token $token
        }
        catch {
            $cleanup.final_http_restore_error = $_.Exception.Message
        }
    }

    $finalService = Get-ServiceInfo -Name $ServiceName
    $summary.cleanup = $cleanup
    $summary.final_service = if ($null -ne $finalService) {
        [ordered]@{
            state = [string]$finalService.State
            start_name = [string]$finalService.StartName
            path_name_restored = ([string]$finalService.PathName -eq $originalPathName)
            path_name = [string]$finalService.PathName
        }
    }
    else {
        $null
    }

    if ($summary.certificate_lifecycle -eq 'generate-bind-rotate-remove-pass' -and
        $cleanup.restore_service_path_ok -and
        $null -ne $cleanup.final_http_restore -and
        [int]$cleanup.final_http_restore.status_code -eq 200 -and
        $null -ne $summary.final_service -and
        [string]$summary.final_service.state -eq 'Running' -and
        [bool]$summary.final_service.path_name_restored) {
        $summary.ok = $true
    }

    Write-JsonFile -Path $cleanupPath -Value $cleanup
    Write-JsonFile -Path $summaryPath -Value $summary
}

if (-not [bool]$summary.ok) {
    throw "PCV_TLS_SMOKE_FAILED|Internal HTTPS/TLS lifecycle smoke failed.|$summaryPath"
}

$summary
