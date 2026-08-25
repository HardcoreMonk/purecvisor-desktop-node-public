param(
    [Parameter(Mandatory)]
    [string]$InstallerRoot
)

Import-Module (Join-Path $InstallerRoot 'PcvDesktopNodeInstaller.Build.psm1') -Force

function ConvertTo-PcvInstallerTestToolOutput {
    param(
        [int]$ExitCode,
        [string]$Stdout = '',
        [string]$Stderr = '',
        [string[]]$ArgumentList = @(),
        [string[]]$RedactionValues = @()
    )

    foreach ($value in $RedactionValues) {
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            $Stdout = $Stdout.Replace($value, '[redacted]')
            $Stderr = $Stderr.Replace($value, '[redacted]')
        }
    }

    [ordered]@{
        exit_code = $ExitCode
        stdout = $Stdout
        stderr = $Stderr
        arguments = @($ArgumentList | ForEach-Object {
            $argument = [string]$_
            foreach ($value in $RedactionValues) {
                if (-not [string]::IsNullOrWhiteSpace($value)) {
                    $argument = $argument.Replace($value, '[redacted]')
                }
            }
            $argument
        })
    }
}

function Invoke-PcvInstallerTestTool {
    param(
        [Parameter(Mandatory)][string]$FilePath,
        [string[]]$ArgumentList = @(),
        [string[]]$RedactionValues = @()
    )

    $leaf = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
    if ($leaf -ieq 'dotnet' -and $ArgumentList.Count -gt 0 -and $ArgumentList[0] -eq 'publish') {
        $outputIndex = [array]::IndexOf($ArgumentList, '-o')
        if ($outputIndex -lt 0 -or $outputIndex + 1 -ge $ArgumentList.Count) {
            return ConvertTo-PcvInstallerTestToolOutput -ExitCode 91 -Stderr 'missing publish output' -ArgumentList $ArgumentList
        }

        $outputRoot = $ArgumentList[$outputIndex + 1]
        New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
        $fileName = if ($ArgumentList[1] -like '*DesktopNode.Host.csproj') { 'DesktopNode.Host.exe' } else { 'pcvcli.exe' }
        Set-Content -LiteralPath (Join-Path $outputRoot $fileName) -Value "fake-$fileName" -NoNewline
        return ConvertTo-PcvInstallerTestToolOutput -ExitCode 0 -Stdout "fake dotnet publish $fileName" -ArgumentList $ArgumentList
    }

    $processPath = $FilePath
    $processArguments = @($ArgumentList)
    $extension = [System.IO.Path]::GetExtension($FilePath)
    if ($extension -ieq '.cmd' -or $extension -ieq '.bat') {
        $processPath = Join-Path $env:SystemRoot 'System32\cmd.exe'
        $processArguments = @('/d', '/c', $FilePath) + $ArgumentList
    }

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $processPath
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $processArguments) {
        [void]$startInfo.ArgumentList.Add([string]$argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        [void]$process.Start()
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        ConvertTo-PcvInstallerTestToolOutput `
            -ExitCode $process.ExitCode `
            -Stdout $stdout `
            -Stderr $stderr `
            -ArgumentList $ArgumentList `
            -RedactionValues $RedactionValues
    }
    finally {
        $process.Dispose()
    }
}

function Invoke-PcvInstallerModuleJson {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Version,
        [string]$MsiProductVersion,
        [string]$DesktopNodeHostPath,
        [string]$DesktopNodeCliPath,
        [Parameter(Mandatory)][string]$OutputRoot,
        [ValidateSet('RequireSigned', 'AllowUnsignedDev')][string]$SigningMode = 'RequireSigned',
        [ValidateSet('Unspecified', 'LocalTest', 'InternalEnterprise', 'PublicTrusted')]
        [string]$SigningTrustModel = 'Unspecified',
        [string]$SignToolPath,
        [string]$CertificateThumbprint,
        [string]$CertificatePath,
        [string]$TimestampUrl,
        [string]$WixPath = 'wix',
        [switch]$DryRun
    )

    $buildInput = @{
        Version = $Version
        MsiProductVersion = $MsiProductVersion
        DesktopNodeHostPath = $DesktopNodeHostPath
        DesktopNodeCliPath = $DesktopNodeCliPath
        OutputRoot = $OutputRoot
        SigningMode = $SigningMode
        SigningTrustModel = $SigningTrustModel
        SignToolPath = $SignToolPath
        CertificateThumbprint = $CertificateThumbprint
        CertificatePath = $CertificatePath
        TimestampUrl = $TimestampUrl
        WixPath = $WixPath
        DryRun = [bool]$DryRun
    }

    $payload = Invoke-PcvDesktopNodeInstallerBuild `
        -Input $buildInput `
        -ToolRunner ${function:Invoke-PcvInstallerTestTool}
    $global:LASTEXITCODE = [int]$payload.exit_code
    $payload | ConvertTo-Json -Depth 12 -Compress
}
