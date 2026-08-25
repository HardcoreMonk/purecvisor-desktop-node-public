Set-StrictMode -Version Latest

function ConvertTo-PcvCliJson {
    param([Parameter(Mandatory, ValueFromPipeline)]$Value)
    $Value | ConvertTo-Json -Depth 50 -Compress
}

function New-PcvCliResult {
    param(
        [Parameter(Mandatory)][int]$ExitCode,
        [AllowNull()][string]$Stdout,
        [AllowNull()][string]$Stderr
    )

    [ordered]@{
        exit_code = $ExitCode
        stdout = if ($null -eq $Stdout) { '' } else { $Stdout }
        stderr = if ($null -eq $Stderr) { '' } else { $Stderr }
    }
}

function New-PcvCliError {
    param([Parameter(Mandatory)][string]$Message)
    New-PcvCliResult -ExitCode 2 -Stdout '' -Stderr $Message
}

function Get-PcvCliUsage {
    @'
Usage:
  pcv --api http://127.0.0.1:7777 [--token TOKEN | --token-file PATH | --protected-token-file PATH] [--json] host status
  pcv [--json] runtime policy
  pcv [--json] vm list
  pcv [--json] vm get <vm>
  pcv [--json] vm create --name <name> --iso <path> --cpu <n> --memory-mb <mb> --disk-gb <gb> [--vm-root <path>] [--generation <1|2>]
  pcv [--json] vm start|stop|shutdown|poweroff|restart <vm>
  pcv [--json] vm checkpoint list <vm>
  pcv [--json] vm checkpoint create <vm> --name <checkpoint>
  pcv [--json] vm checkpoint restore|delete <vm> <checkpoint>
  pcv [--json] job get|cancel|retry <job_id>
'@.Trim()
}

function ConvertTo-PcvCliRouteSegment {
    param([Parameter(Mandatory)][string]$Value)
    [System.Uri]::EscapeDataString($Value)
}

function Resolve-PcvCliTokenFile {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return [ordered]@{ ok = $false; error = "Token file was not found: $Path" }
    }

    try {
        $token = (Get-Content -LiteralPath $Path -Raw).Trim()
    }
    catch {
        return [ordered]@{ ok = $false; error = "Token file could not be read: $($_.Exception.Message)" }
    }

    if ([string]::IsNullOrWhiteSpace($token)) {
        return [ordered]@{ ok = $false; error = "Token file is empty: $Path" }
    }

    [ordered]@{ ok = $true; token = $token }
}

function Import-PcvCliServiceTokenSupport {
    $desktopRoot = Split-Path -Parent $PSScriptRoot
    $modulePath = Join-Path (Join-Path $desktopRoot 'service') 'PcvDesktopService.psm1'
    if (-not (Test-Path -LiteralPath $modulePath -PathType Leaf)) {
        return [ordered]@{ ok = $false; error = "Protected token support module was not found: $modulePath" }
    }

    try {
        Import-Module $modulePath -Force
        [ordered]@{ ok = $true; error = $null }
    }
    catch {
        [ordered]@{ ok = $false; error = "Protected token support module could not be loaded: $($_.Exception.Message)" }
    }
}

function Resolve-PcvCliProtectedTokenFile {
    param([Parameter(Mandatory)][string]$Path)

    $import = Import-PcvCliServiceTokenSupport
    if (-not $import.ok) {
        return $import
    }

    try {
        $resolved = Read-PcvDesktopServiceProtectedTokenFile -Path $Path
        [ordered]@{ ok = $true; token = $resolved.token }
    }
    catch {
        [ordered]@{ ok = $false; error = "Protected token file could not be read: $($_.Exception.Message)" }
    }
}

function ConvertFrom-PcvCliArguments {
    param([string[]]$Arguments)

    $remaining = [System.Collections.Generic.List[string]]::new()
    $config = [ordered]@{
        api_base_url = 'http://127.0.0.1:7777'
        api_token = $null
        api_token_file = $null
        api_token_protected_file = $null
        json = $false
    }

    for ($index = 0; $index -lt $Arguments.Count; $index++) {
        $arg = $Arguments[$index]
        switch ($arg) {
            '--json' {
                $config.json = $true
                continue
            }
            '--api' {
                if ($index + 1 -ge $Arguments.Count) {
                    return [ordered]@{ ok = $false; error = 'Missing value for --api.' }
                }
                $index++
                $config.api_base_url = $Arguments[$index]
                continue
            }
            '--token' {
                if ($index + 1 -ge $Arguments.Count) {
                    return [ordered]@{ ok = $false; error = 'Missing value for --token.' }
                }
                $index++
                $config.api_token = $Arguments[$index]
                continue
            }
            '--token-file' {
                if ($index + 1 -ge $Arguments.Count) {
                    return [ordered]@{ ok = $false; error = 'Missing value for --token-file.' }
                }
                $index++
                $config.api_token_file = $Arguments[$index]
                continue
            }
            '--protected-token-file' {
                if ($index + 1 -ge $Arguments.Count) {
                    return [ordered]@{ ok = $false; error = 'Missing value for --protected-token-file.' }
                }
                $index++
                $config.api_token_protected_file = $Arguments[$index]
                continue
            }
            '--help' {
                return [ordered]@{ ok = $true; help = $true; config = $config; args = @() }
            }
            default {
                $remaining.Add($arg)
            }
        }
    }

    $tokenSources = @()
    if (-not [string]::IsNullOrWhiteSpace($config.api_token)) {
        $tokenSources += '--token'
    }
    if (-not [string]::IsNullOrWhiteSpace($config.api_token_file)) {
        $tokenSources += '--token-file'
    }
    if (-not [string]::IsNullOrWhiteSpace($config.api_token_protected_file)) {
        $tokenSources += '--protected-token-file'
    }

    if ($tokenSources.Count -gt 1) {
        return [ordered]@{ ok = $false; error = 'Use only one of --token, --token-file, or --protected-token-file.' }
    }

    if (-not [string]::IsNullOrWhiteSpace($config.api_token_file)) {
        $resolved = Resolve-PcvCliTokenFile -Path $config.api_token_file
        if (-not $resolved.ok) {
            return [ordered]@{ ok = $false; error = $resolved.error }
        }
        $config.api_token = $resolved.token
    }
    elseif (-not [string]::IsNullOrWhiteSpace($config.api_token_protected_file)) {
        $resolved = Resolve-PcvCliProtectedTokenFile -Path $config.api_token_protected_file
        if (-not $resolved.ok) {
            return [ordered]@{ ok = $false; error = $resolved.error }
        }
        $config.api_token = $resolved.token
    }

    [ordered]@{
        ok = $true
        help = $false
        config = $config
        args = @($remaining.ToArray())
    }
}

function ConvertFrom-PcvCliNamedOptions {
    param([string[]]$Arguments)

    $options = @{}
    $positionals = [System.Collections.Generic.List[string]]::new()

    for ($index = 0; $index -lt $Arguments.Count; $index++) {
        $arg = $Arguments[$index]
        if ($arg.StartsWith('--', [System.StringComparison]::Ordinal)) {
            if ($index + 1 -ge $Arguments.Count) {
                return [ordered]@{ ok = $false; error = "Missing value for $arg." }
            }
            $index++
            $options[$arg] = $Arguments[$index]
            continue
        }

        $positionals.Add($arg)
    }

    [ordered]@{
        ok = $true
        options = $options
        positionals = @($positionals.ToArray())
    }
}

function Get-PcvCliRequiredOption {
    param(
        [Parameter(Mandatory)]$Options,
        [Parameter(Mandatory)][string]$Name
    )

    if (-not $Options.ContainsKey($Name) -or [string]::IsNullOrWhiteSpace([string]$Options[$Name])) {
        throw "Missing required option $Name."
    }

    [string]$Options[$Name]
}

function ConvertTo-PcvCliIntOption {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Value
    )

    try {
        [int]$Value
    }
    catch {
        throw "Option $Name must be an integer."
    }
}

function New-PcvCliRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Body
    )

    [ordered]@{
        method = $Method
        path = $Path
        body = $Body
    }
}

function Get-PcvCliRequestFromCommand {
    param([string[]]$Arguments)

    if ($Arguments.Count -eq 0) {
        return [ordered]@{ ok = $false; error = (Get-PcvCliUsage) }
    }

    if ($Arguments[0] -eq 'host') {
        if ($Arguments.Count -eq 2 -and $Arguments[1] -eq 'status') {
            return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'GET' -Path '/api/v1/host/status' -Body $null) }
        }
        return [ordered]@{ ok = $false; error = 'Unknown host command. Use: host status.' }
    }

    if ($Arguments[0] -eq 'runtime') {
        if ($Arguments.Count -eq 2 -and $Arguments[1] -eq 'policy') {
            return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'GET' -Path '/api/v1/runtime/policy' -Body $null) }
        }
        return [ordered]@{ ok = $false; error = 'Unknown runtime command. Use: runtime policy.' }
    }

    if ($Arguments[0] -eq 'job') {
        if ($Arguments.Count -ne 3) {
            return [ordered]@{ ok = $false; error = 'Use: job get|cancel|retry <job_id>.' }
        }

        $jobId = ConvertTo-PcvCliRouteSegment -Value $Arguments[2]
        switch ($Arguments[1]) {
            'get' { return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'GET' -Path "/api/v1/jobs/$jobId" -Body $null) } }
            'cancel' { return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'POST' -Path "/api/v1/jobs/$jobId/cancel" -Body $null) } }
            'retry' { return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'POST' -Path "/api/v1/jobs/$jobId/retry" -Body $null) } }
            default { return [ordered]@{ ok = $false; error = 'Unknown job command. Use: job get|cancel|retry <job_id>.' } }
        }
    }

    if ($Arguments[0] -ne 'vm') {
        return [ordered]@{ ok = $false; error = (Get-PcvCliUsage) }
    }

    if ($Arguments.Count -lt 2) {
        return [ordered]@{ ok = $false; error = 'Use: vm list|get|create|start|stop|shutdown|poweroff|restart|checkpoint.' }
    }

    switch ($Arguments[1]) {
        'list' {
            if ($Arguments.Count -ne 2) {
                return [ordered]@{ ok = $false; error = 'Use: vm list.' }
            }
            return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'GET' -Path '/api/v1/vms' -Body $null) }
        }
        'get' {
            if ($Arguments.Count -ne 3) {
                return [ordered]@{ ok = $false; error = 'Use: vm get <vm>.' }
            }
            $vmId = ConvertTo-PcvCliRouteSegment -Value $Arguments[2]
            return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'GET' -Path "/api/v1/vms/$vmId" -Body $null) }
        }
        'create' {
            $parsed = ConvertFrom-PcvCliNamedOptions -Arguments @($Arguments | Select-Object -Skip 2)
            if (-not $parsed.ok) {
                return [ordered]@{ ok = $false; error = $parsed.error }
            }

            try {
                $body = [ordered]@{
                    name = Get-PcvCliRequiredOption -Options $parsed.options -Name '--name'
                    iso_path = Get-PcvCliRequiredOption -Options $parsed.options -Name '--iso'
                    cpu = ConvertTo-PcvCliIntOption -Name '--cpu' -Value (Get-PcvCliRequiredOption -Options $parsed.options -Name '--cpu')
                    memory_mb = ConvertTo-PcvCliIntOption -Name '--memory-mb' -Value (Get-PcvCliRequiredOption -Options $parsed.options -Name '--memory-mb')
                    disk_gb = ConvertTo-PcvCliIntOption -Name '--disk-gb' -Value (Get-PcvCliRequiredOption -Options $parsed.options -Name '--disk-gb')
                }
                if ($parsed.options.ContainsKey('--vm-root')) {
                    $body.vm_root = [string]$parsed.options['--vm-root']
                }
                if ($parsed.options.ContainsKey('--generation')) {
                    $body.generation = ConvertTo-PcvCliIntOption -Name '--generation' -Value ([string]$parsed.options['--generation'])
                }
            }
            catch {
                return [ordered]@{ ok = $false; error = $_.Exception.Message }
            }

            return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'POST' -Path '/api/v1/vms' -Body ($body | ConvertTo-PcvCliJson)) }
        }
        { $_ -in @('start', 'stop', 'shutdown', 'poweroff', 'restart') } {
            if ($Arguments.Count -ne 3) {
                return [ordered]@{ ok = $false; error = "Use: vm $($Arguments[1]) <vm>." }
            }

            $action = $Arguments[1]
            if ($action -eq 'stop') {
                $action = 'shutdown'
            }

            $vmId = ConvertTo-PcvCliRouteSegment -Value $Arguments[2]
            return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'POST' -Path "/api/v1/vms/$vmId/$action" -Body $null) }
        }
        'checkpoint' {
            if ($Arguments.Count -lt 4) {
                return [ordered]@{ ok = $false; error = 'Use: vm checkpoint list|create|restore|delete ...' }
            }

            $checkpointAction = $Arguments[2]
            $vmId = ConvertTo-PcvCliRouteSegment -Value $Arguments[3]

            switch ($checkpointAction) {
                'list' {
                    if ($Arguments.Count -ne 4) {
                        return [ordered]@{ ok = $false; error = 'Use: vm checkpoint list <vm>.' }
                    }
                    return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'GET' -Path "/api/v1/vms/$vmId/checkpoints" -Body $null) }
                }
                'create' {
                    $parsed = ConvertFrom-PcvCliNamedOptions -Arguments @($Arguments | Select-Object -Skip 4)
                    if (-not $parsed.ok) {
                        return [ordered]@{ ok = $false; error = $parsed.error }
                    }

                    try {
                        $checkpointName = Get-PcvCliRequiredOption -Options $parsed.options -Name '--name'
                    }
                    catch {
                        return [ordered]@{ ok = $false; error = $_.Exception.Message }
                    }

                    $body = [ordered]@{ name = $checkpointName } | ConvertTo-PcvCliJson
                    return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'POST' -Path "/api/v1/vms/$vmId/checkpoints" -Body $body) }
                }
                { $_ -in @('restore', 'delete') } {
                    if ($Arguments.Count -ne 5) {
                        return [ordered]@{ ok = $false; error = "Use: vm checkpoint $checkpointAction <vm> <checkpoint>." }
                    }

                    $checkpointId = ConvertTo-PcvCliRouteSegment -Value $Arguments[4]
                    if ($checkpointAction -eq 'restore') {
                        return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'POST' -Path "/api/v1/vms/$vmId/checkpoints/$checkpointId/restore" -Body $null) }
                    }

                    return [ordered]@{ ok = $true; request = (New-PcvCliRequest -Method 'DELETE' -Path "/api/v1/vms/$vmId/checkpoints/$checkpointId" -Body $null) }
                }
                default {
                    return [ordered]@{ ok = $false; error = 'Unknown checkpoint command. Use: list, create, restore, or delete.' }
                }
            }
        }
        default {
            return [ordered]@{ ok = $false; error = 'Unknown vm command. Use: vm list|get|create|start|stop|shutdown|poweroff|restart|checkpoint.' }
        }
    }
}

function Invoke-PcvDesktopApiRequest {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [AllowNull()][string]$Body,
        [Parameter(Mandatory)][string]$ApiBaseUrl,
        [AllowNull()][string]$ApiToken
    )

    $uri = $ApiBaseUrl.TrimEnd('/') + $Path
    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($ApiToken)) {
        $headers['Authorization'] = "Bearer $ApiToken"
    }

    $parameters = @{
        Uri = $uri
        Method = $Method
        Headers = $headers
        SkipHttpErrorCheck = $true
    }
    if (-not [string]::IsNullOrWhiteSpace($Body)) {
        $parameters['ContentType'] = 'application/json'
        $parameters['Body'] = $Body
    }

    $response = Invoke-WebRequest @parameters
    if ([string]::IsNullOrWhiteSpace($response.Content)) {
        return [ordered]@{
            ok = $false
            operation = 'api.request'
            data = $null
            error = [ordered]@{
                code = 'PCV_CLI_EMPTY_RESPONSE'
                message = 'Local API response body was empty.'
                detail = $uri
                retryable = $true
            }
        }
    }

    $response.Content | ConvertFrom-Json -Depth 50
}

function Format-PcvCliOutput {
    param(
        [Parameter(Mandatory)]$Response,
        [Parameter(Mandatory)][bool]$Json
    )

    if ($Json) {
        return ($Response | ConvertTo-PcvCliJson)
    }

    if ($Response.ok) {
        if ($null -ne $Response.data) {
            return ($Response.data | ConvertTo-Json -Depth 20)
        }
        return "$($Response.operation): ok"
    }

    if ($null -ne $Response.error) {
        return "$($Response.error.code): $($Response.error.message)"
    }

    "$($Response.operation): failed"
}

function Invoke-PcvDesktopCli {
    param(
        [string[]]$Arguments = @(),
        [scriptblock]$Transport
    )

    $parsed = ConvertFrom-PcvCliArguments -Arguments $Arguments
    if (-not $parsed.ok) {
        return New-PcvCliError -Message $parsed.error
    }
    if ($parsed.help) {
        return New-PcvCliResult -ExitCode 0 -Stdout (Get-PcvCliUsage) -Stderr ''
    }

    $requestResult = Get-PcvCliRequestFromCommand -Arguments $parsed.args
    if (-not $requestResult.ok) {
        return New-PcvCliError -Message $requestResult.error
    }

    $request = $requestResult.request
    try {
        if ($null -eq $Transport) {
            $response = Invoke-PcvDesktopApiRequest `
                -Method $request.method `
                -Path $request.path `
                -Body $request.body `
                -ApiBaseUrl $parsed.config.api_base_url `
                -ApiToken $parsed.config.api_token
        }
        else {
            $response = & $Transport `
                -Method $request.method `
                -Path $request.path `
                -Body $request.body `
                -ApiBaseUrl $parsed.config.api_base_url `
                -ApiToken $parsed.config.api_token
        }
    }
    catch {
        return New-PcvCliResult -ExitCode 1 -Stdout '' -Stderr $_.Exception.Message
    }

    $stdout = Format-PcvCliOutput -Response $response -Json ([bool]$parsed.config.json)
    $exitCode = if ($response.ok) { 0 } else { 1 }
    $stderr = if ($response.ok) { '' } elseif ($null -ne $response.error) { "$($response.error.code): $($response.error.message)" } else { 'Local API request failed.' }

    New-PcvCliResult -ExitCode $exitCode -Stdout $stdout -Stderr $stderr
}

Export-ModuleMember -Function `
    ConvertFrom-PcvCliArguments, `
    ConvertFrom-PcvCliNamedOptions, `
    ConvertTo-PcvCliJson, `
    Get-PcvCliRequestFromCommand, `
    Invoke-PcvDesktopApiRequest, `
    Invoke-PcvDesktopCli
