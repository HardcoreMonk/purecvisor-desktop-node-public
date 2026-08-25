Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-PcvJsonAndExit {
    param(
        [Parameter(Mandatory)]
        [object]$Payload,

        [Parameter(Mandatory)]
        [int]$ExitCode
    )

    $exception = [System.Exception]::new('__PCV_INSTALLER_BUILD_RESULT__')
    $exception.Data['Payload'] = $Payload
    $exception.Data['ExitCode'] = $ExitCode
    throw $exception
}

function New-PcvInstallerError {
    param(
        [Parameter(Mandatory)]
        [string]$Code,

        [Parameter(Mandatory)]
        [string]$Message,

        [string]$Detail = ''
    )

    $payload = [ordered]@{
        ok = $false
        error = [ordered]@{
            code = $Code
            message = $Message
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($Detail)) {
        $payload.error.detail = $Detail
    }

    $payload
}

function New-PcvInstallerErrorFromException {
    param(
        [Parameter(Mandatory)]
        [object]$ErrorRecord,

        [Parameter(Mandatory)]
        [string]$DefaultCode,

        [Parameter(Mandatory)]
        [string]$DefaultMessage
    )

    $rawMessage = [string]$ErrorRecord
    if ($ErrorRecord.Exception -and -not [string]::IsNullOrWhiteSpace($ErrorRecord.Exception.Message)) {
        $rawMessage = $ErrorRecord.Exception.Message
    }

    $parts = $rawMessage -split '\|', 3
    if ($parts.Count -eq 3 -and $parts[0] -match '^PCV_INSTALLER_[A-Z0-9_]+$') {
        return New-PcvInstallerError -Code $parts[0] -Message $parts[1] -Detail $parts[2]
    }

    New-PcvInstallerError -Code $DefaultCode -Message $DefaultMessage -Detail $rawMessage
}

function ConvertTo-PcvRedactedText {
    param(
        [string]$Text,
        [string[]]$RedactionValues = @()
    )

    if ($null -eq $Text) {
        return ''
    }

    $redacted = $Text
    foreach ($value in $RedactionValues) {
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            $redacted = $redacted.Replace($value, '[redacted]')
        }
    }

    $maxLength = 8192
    if ($redacted.Length -gt $maxLength) {
        return ($redacted.Substring(0, $maxLength) + '...[truncated]')
    }

    $redacted
}

function ConvertTo-PcvInstallerToolOutput {
    param(
        [Parameter(Mandatory)]
        [int]$ExitCode,

        [string]$Stdout = '',
        [string]$Stderr = '',
        [string[]]$Arguments = @(),
        [string[]]$RedactionValues = @()
    )

    [ordered]@{
        exit_code = $ExitCode
        stdout = ConvertTo-PcvRedactedText -Text $Stdout -RedactionValues $RedactionValues
        stderr = ConvertTo-PcvRedactedText -Text $Stderr -RedactionValues $RedactionValues
        arguments = @($Arguments | ForEach-Object {
            ConvertTo-PcvRedactedText -Text ([string]$_) -RedactionValues $RedactionValues
        })
    }
}

function Invoke-PcvInstallerProcess {
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [string[]]$ArgumentList = @(),
        [string[]]$RedactionValues = @()
    )

    $processPath = $FilePath
    $processArgs = @($ArgumentList)
    $extension = [System.IO.Path]::GetExtension($FilePath)
    $runningOnWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::Windows)

    if ($runningOnWindows -and ($extension -ieq '.cmd' -or $extension -ieq '.bat')) {
        $processPath = Join-Path $env:SystemRoot 'System32\cmd.exe'
        $processArgs = @('/d', '/c', $FilePath) + $ArgumentList
    }

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $processPath
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    foreach ($arg in $processArgs) {
        [void]$startInfo.ArgumentList.Add($arg)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo

    try {
        [void]$process.Start()
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()

        ConvertTo-PcvInstallerToolOutput `
            -ExitCode $process.ExitCode `
            -Stdout $stdout `
            -Stderr $stderr `
            -Arguments $ArgumentList `
            -RedactionValues $RedactionValues
    }
    finally {
        $process.Dispose()
    }
}

function Get-PcvFileSha256 {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Test-PcvChildPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$Parent
    )

    $trimChars = [char[]]@(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    )
    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd($trimChars)
    $fullParent = [System.IO.Path]::GetFullPath($Parent).TrimEnd($trimChars)
    $parentWithSeparator = $fullParent + [System.IO.Path]::DirectorySeparatorChar

    $fullPath.StartsWith($parentWithSeparator, [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-PcvGitCommit {
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot
    )

    try {
        $value = (& git -C $RepositoryRoot rev-parse HEAD 2>$null)
        if ($LASTEXITCODE -eq 0 -and $value) {
            return ($value | Select-Object -First 1)
        }
        Write-Verbose "Get-PcvGitCommit returned no commit. RepositoryRoot=$RepositoryRoot ExitCode=$LASTEXITCODE Value=$value"
    }
    catch {
        Write-Verbose "Get-PcvGitCommit failed. RepositoryRoot=$RepositoryRoot Error=$_"
    }

    return 'unknown'
}

function Get-PcvWixVersion {
    param(
        [Parameter(Mandatory)]
        [string]$Command
    )

    try {
        $value = (& $Command --version 2>$null)
        if ($LASTEXITCODE -eq 0 -and $value) {
            return ($value | Select-Object -First 1)
        }
    }
    catch {
    }

    return 'not-detected'
}

function Test-PcvHasSigningCertificateInput {
    param(
        [string]$Thumbprint,
        [string]$Path
    )

    -not [string]::IsNullOrWhiteSpace($Thumbprint) -or -not [string]::IsNullOrWhiteSpace($Path)
}

function Resolve-PcvCommandPath {
    param(
        [Parameter(Mandatory)]
        [string]$Command
    )

    if ([string]::IsNullOrWhiteSpace($Command)) {
        return $null
    }

    if (Test-Path -LiteralPath $Command -PathType Leaf) {
        return (Resolve-Path -LiteralPath $Command).Path
    }

    $resolvedCommand = Get-Command -Name $Command -ErrorAction SilentlyContinue
    if (-not $resolvedCommand) {
        return $null
    }

    $resolvedPath = $resolvedCommand.Source
    if ([string]::IsNullOrWhiteSpace($resolvedPath)) {
        $resolvedPath = $resolvedCommand.Path
    }

    if ([string]::IsNullOrWhiteSpace($resolvedPath)) {
        return $null
    }

    $resolvedPath
}

function ConvertTo-PcvMsiProductVersion {
    param(
        [Parameter(Mandatory)]
        [string]$InputVersion
    )

    if ($InputVersion -notmatch '^(\d+)\.(\d+)\.(\d+)') {
        return $null
    }

    $major = [int]$Matches[1]
    $minor = [int]$Matches[2]
    $build = [int]$Matches[3]

    if ($major -ge 256 -or $minor -ge 256 -or $build -ge 65536) {
        return $null
    }

    "$major.$minor.$build"
}

function Resolve-PcvMsiProductVersion {
    param(
        [Parameter(Mandatory)]
        [string]$ReleaseVersion,

        [string]$ExplicitMsiProductVersion
    )

    $candidate = $ExplicitMsiProductVersion
    if ([string]::IsNullOrWhiteSpace($candidate)) {
        $candidate = ConvertTo-PcvMsiProductVersion -InputVersion $ReleaseVersion
    }

    if ([string]::IsNullOrWhiteSpace($candidate)) {
        throw 'PCV_INSTALLER_INVALID_VERSION|Version must begin with an MSI-compatible numeric major.minor.build value.|'
    }

    $normalized = ConvertTo-PcvMsiProductVersion -InputVersion $candidate
    if ($normalized -ne $candidate) {
        throw "PCV_INSTALLER_INVALID_MSI_PRODUCT_VERSION|MsiProductVersion must be an MSI-compatible major.minor.build value.|$candidate"
    }

    $candidate
}

function Resolve-PcvReleaseChannel {
    param(
        [Parameter(Mandatory)]
        [string]$ReleaseVersion
    )

    if ($ReleaseVersion -match '^\d+\.\d+\.\d+-dev(\.\d+)?$') {
        return 'dev'
    }

    if ($ReleaseVersion -match '^\d+\.\d+\.\d+-admin-smoke(\.\d+)?$') {
        return 'admin-smoke'
    }

    if ($ReleaseVersion -match '^\d+\.\d+\.\d+-rc\.\d+$') {
        return 'rc'
    }

    if ($ReleaseVersion -match '^\d+\.\d+\.\d+$') {
        return 'stable'
    }

    throw "PCV_INSTALLER_INVALID_RELEASE_CHANNEL|Version must use dev, admin-smoke, rc, or stable channel naming.|$ReleaseVersion"
}

function Get-PcvPayloadAggregateSha256 {
    param(
        [Parameter(Mandatory)]
        [string]$PayloadRoot
    )

    $payloadFiles = @(Get-ChildItem -LiteralPath $PayloadRoot -File -Recurse)
    $aggregateInput = ($payloadFiles | Sort-Object FullName | ForEach-Object {
        $relativePath = $_.FullName.Substring($PayloadRoot.Length).TrimStart('\', '/').Replace('\', '/')
        "$(Get-PcvFileSha256 -Path $_.FullName)  $relativePath"
    }) -join "`n"
    $aggregateHash = [System.BitConverter]::ToString(
        [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($aggregateInput))
    ).Replace('-', '').ToLowerInvariant()

    [ordered]@{
        files = $payloadFiles
        aggregate_sha256 = $aggregateHash
    }
}

function Invoke-PcvInstallerBuildCore {
    param(
        [Parameter(Mandatory)][hashtable]$BuildInput,
        [Parameter(Mandatory)][scriptblock]$ToolRunner
    )

    $Version = [string]$BuildInput['Version']
    $MsiProductVersion = [string]$BuildInput['MsiProductVersion']
    $DesktopNodeHostPath = [string]$BuildInput['DesktopNodeHostPath']
    $DesktopNodeCliPath = [string]$BuildInput['DesktopNodeCliPath']
    $OutputRoot = [string]$BuildInput['OutputRoot']
    $SigningMode = if ([string]::IsNullOrWhiteSpace([string]$BuildInput['SigningMode'])) { 'RequireSigned' } else { [string]$BuildInput['SigningMode'] }
    $SigningTrustModel = if ([string]::IsNullOrWhiteSpace([string]$BuildInput['SigningTrustModel'])) { 'Unspecified' } else { [string]$BuildInput['SigningTrustModel'] }
    $SignToolPath = [string]$BuildInput['SignToolPath']
    $CertificateThumbprint = [string]$BuildInput['CertificateThumbprint']
    $CertificatePath = [string]$BuildInput['CertificatePath']
    $TimestampUrl = [string]$BuildInput['TimestampUrl']
    $WixPath = if ([string]::IsNullOrWhiteSpace([string]$BuildInput['WixPath'])) { 'wix' } else { [string]$BuildInput['WixPath'] }
    $DryRun = [bool]$BuildInput['DryRun']

    $resolvedSignToolPath = $null
    if ($SigningMode -eq 'RequireSigned') {
        $hasSignTool = -not [string]::IsNullOrWhiteSpace($SignToolPath)
        if ($hasSignTool) {
            $resolvedSignToolPath = Resolve-PcvCommandPath -Command $SignToolPath
            if ([string]::IsNullOrWhiteSpace($resolvedSignToolPath)) {
                Write-PcvJsonAndExit `
                    -ExitCode 1 `
                    -Payload (New-PcvInstallerError `
                        -Code 'PCV_INSTALLER_SIGNTOOL_NOT_FOUND' `
                        -Message "SignTool was not found: $SignToolPath")
            }
        }
    
        $hasCertificate = Test-PcvHasSigningCertificateInput -Thumbprint $CertificateThumbprint -Path $CertificatePath
        $hasTimestamp = -not [string]::IsNullOrWhiteSpace($TimestampUrl)
    
        if (-not ($hasSignTool -and $hasCertificate -and $hasTimestamp)) {
            Write-PcvJsonAndExit `
                -ExitCode 1 `
                -Payload (New-PcvInstallerError `
                    -Code 'PCV_INSTALLER_SIGNING_REQUIRED' `
                    -Message 'RequireSigned builds require SignToolPath, certificate input, and TimestampUrl.')
        }
    
        if ($SigningTrustModel -eq 'Unspecified') {
            Write-PcvJsonAndExit `
                -ExitCode 1 `
                -Payload (New-PcvInstallerError `
                    -Code 'PCV_INSTALLER_SIGNING_TRUST_MODEL_REQUIRED' `
                    -Message 'RequireSigned builds require an explicit SigningTrustModel: LocalTest, InternalEnterprise, or PublicTrusted.')
        }
    }
    
    try {
        $msiProductVersion = Resolve-PcvMsiProductVersion `
            -ReleaseVersion $Version `
            -ExplicitMsiProductVersion $MsiProductVersion
        $releaseChannel = Resolve-PcvReleaseChannel -ReleaseVersion $Version
    }
    catch {
        Write-PcvJsonAndExit `
            -ExitCode 1 `
            -Payload (New-PcvInstallerErrorFromException `
                -ErrorRecord $_ `
                -DefaultCode 'PCV_INSTALLER_INVALID_VERSION' `
                -DefaultMessage 'Version must begin with an MSI-compatible numeric major.minor.build value.')
    }
    
    if (($releaseChannel -eq 'rc' -or $releaseChannel -eq 'stable') -and $SigningMode -ne 'RequireSigned') {
        Write-PcvJsonAndExit `
            -ExitCode 1 `
            -Payload (New-PcvInstallerError `
                -Code 'PCV_INSTALLER_RELEASE_SIGNING_REQUIRED' `
                -Message 'RC and stable installer artifacts require RequireSigned signing mode.' `
                -Detail $Version)
    }
    
    $installerRoot = $PSScriptRoot
    $repoRoot = (Resolve-Path (Join-Path $installerRoot '..\..\..')).Path
    $outputRootFull = [System.IO.Path]::GetFullPath($OutputRoot)
    $payloadRoot = Join-Path $outputRootFull 'payload'
    $artifactArchitecture = 'windows-x64'
    $artifactBaseName = "PureCVisorDesktopNode-$Version-$artifactArchitecture"
    $msiPath = Join-Path $outputRootFull "$artifactBaseName.msi"
    $provenancePath = Join-Path $outputRootFull "$artifactBaseName.provenance.json"
    $msiSha256Path = Join-Path $outputRootFull "$artifactBaseName.msi.sha256"
    $publicationPath = Join-Path $outputRootFull "$artifactBaseName.publication.json"
    $toolOutput = [ordered]@{}
    $wixSourcePaths = @(
        (Join-Path $installerRoot 'Product.wxs'),
        (Join-Path $installerRoot 'ProductActions.wxs')
    )
    $modulePath = Join-Path $repoRoot 'packaging\windows-desktop-node\PcvDesktopNodeProduct.psm1'
    $moduleHash = Get-PcvFileSha256 -Path $modulePath
    $hostPublishRoot = Join-Path $outputRootFull 'host-publish'
    $hostProjectPath = Join-Path $repoRoot 'src\DesktopNode.Host\DesktopNode.Host.csproj'
    $cliPublishRoot = Join-Path $outputRootFull 'cli-publish'
    $cliProjectPath = Join-Path $repoRoot 'src\DesktopNode.Cli\DesktopNode.Cli.csproj'
    $msiProductVersionParts = $msiProductVersion.Split('.')
    $dotnetPublishAssemblyVersion = "$msiProductVersion.0"
    # Early dev payloads used the .NET default FileVersion=1.0.0.0. Keep MSI key files monotonic for 0.x packages.
    $dotnetPublishFileVersion = '{0}.{1}.{2}.0' -f (
        ([int]$msiProductVersionParts[0] + 1),
        $msiProductVersionParts[1],
        $msiProductVersionParts[2])
    $dotnetPublishVersionArgs = @(
        "-p:Version=$msiProductVersion",
        "-p:AssemblyVersion=$dotnetPublishAssemblyVersion",
        "-p:FileVersion=$dotnetPublishFileVersion",
        "-p:InformationalVersion=$Version"
    )
    $resolvedDesktopNodeHostPath = $null
    $desktopNodeHostHash = $null
    $desktopNodeHostSource = 'dotnet-publish'
    $resolvedDesktopNodeCliPath = $null
    $desktopNodeCliHash = $null
    $desktopNodeCliSource = 'dotnet-publish'
    $publicationPlan = [ordered]@{
        schema_version = '1'
        mode = 'internal-artifact-descriptor-only'
        public_trusted_signing = 'not-claimed'
        external_stable_publication = 'not-claimed'
        burn_bootstrapper = 'not-built'
        msix = 'not-built'
        winget_manifest = 'not-generated'
        network_download_updater = 'catalog-channel-code-level-partial'
        catalog_publication = 'not-published'
        package_uri = $null
        installer_url = $null
    }
    
    if (-not [string]::IsNullOrWhiteSpace($DesktopNodeHostPath)) {
        try {
            $resolvedDesktopNodeHostPath = (Resolve-Path -LiteralPath $DesktopNodeHostPath -ErrorAction Stop).Path
            $desktopNodeHostHash = Get-PcvFileSha256 -Path $resolvedDesktopNodeHostPath
            $desktopNodeHostSource = 'explicit-path'
        }
        catch {
            Write-PcvJsonAndExit `
                -ExitCode 1 `
                -Payload (New-PcvInstallerError `
                    -Code 'PCV_INSTALLER_SERVICE_HOST_NOT_FOUND' `
                    -Message "DesktopNode.Host payload was not found: $DesktopNodeHostPath")
        }
    }
    
    if (-not [string]::IsNullOrWhiteSpace($DesktopNodeCliPath)) {
        try {
            $resolvedDesktopNodeCliPath = (Resolve-Path -LiteralPath $DesktopNodeCliPath -ErrorAction Stop).Path
            $desktopNodeCliHash = Get-PcvFileSha256 -Path $resolvedDesktopNodeCliPath
            $desktopNodeCliSource = 'explicit-path'
        }
        catch {
            Write-PcvJsonAndExit `
                -ExitCode 1 `
                -Payload (New-PcvInstallerError `
                    -Code 'PCV_INSTALLER_CLI_NOT_FOUND' `
                    -Message "DesktopNode.Cli payload was not found: $DesktopNodeCliPath")
        }
    }
    
    $plan = [ordered]@{
        product_name = 'PureCVisor Desktop Node'
        version = $Version
        release_channel = $releaseChannel
        msi_product_version = $msiProductVersion
        artifact_architecture = $artifactArchitecture
        artifact_base_name = $artifactBaseName
        output_root = $outputRootFull
        payload_root = $payloadRoot
        msi_path = $msiPath
        provenance_path = $provenancePath
        msi_sha256_path = $msiSha256Path
        publication_path = $publicationPath
        wix_source_files = $wixSourcePaths
        wix_version = Get-PcvWixVersion -Command $WixPath
        signing_mode = $SigningMode
        signing_trust_model = $SigningTrustModel
        signing_inputs = [ordered]@{
            has_signtool = -not [string]::IsNullOrWhiteSpace($SignToolPath)
            has_certificate = Test-PcvHasSigningCertificateInput -Thumbprint $CertificateThumbprint -Path $CertificatePath
            has_timestamp = -not [string]::IsNullOrWhiteSpace($TimestampUrl)
        }
        service_host_source = $desktopNodeHostSource
        service_host_path = $resolvedDesktopNodeHostPath
        service_host_sha256 = $desktopNodeHostHash
        cli_source = $desktopNodeCliSource
        cli_path = $resolvedDesktopNodeCliPath
        cli_sha256 = $desktopNodeCliHash
        product_wrapper_sha256 = $moduleHash
        publication = $publicationPlan
    }
    
    if ($DryRun) {
        Write-PcvJsonAndExit -ExitCode 0 -Payload ([ordered]@{
            ok = $true
            dry_run = $true
            plan = $plan
        })
    }
    
    $wixExecutable = Resolve-PcvCommandPath -Command $WixPath
    if ([string]::IsNullOrWhiteSpace($wixExecutable)) {
        Write-PcvJsonAndExit `
            -ExitCode 1 `
            -Payload (New-PcvInstallerError -Code 'PCV_INSTALLER_WIX_NOT_FOUND' -Message 'WiX CLI was not found. Install WiX or pass -WixPath.')
    }
    
    $requiresDotnetPublish = (
        [string]::IsNullOrWhiteSpace($resolvedDesktopNodeHostPath) -or
        [string]::IsNullOrWhiteSpace($resolvedDesktopNodeCliPath)
    )
    $dotnetExecutable = $null
    if ($requiresDotnetPublish) {
        $dotnetExecutable = Resolve-PcvCommandPath -Command 'dotnet'
        if ([string]::IsNullOrWhiteSpace($dotnetExecutable)) {
            Write-PcvJsonAndExit `
                -ExitCode 1 `
                -Payload (New-PcvInstallerError -Code 'PCV_INSTALLER_DOTNET_NOT_FOUND' -Message '.NET SDK CLI was not found.')
        }
    }
    
    if ([string]::IsNullOrWhiteSpace($resolvedDesktopNodeHostPath)) {
        $dotnetArgs = @(
            'publish',
            $hostProjectPath,
            '-c',
            'Release',
            '-r',
            'win-x64',
            '--self-contained',
            'true',
            '-p:PublishSingleFile=true',
            '-p:IncludeNativeLibrariesForSelfExtract=true'
        ) + $dotnetPublishVersionArgs + @(
            '-o',
            $hostPublishRoot
        )
        $toolOutput.dotnet_publish = & $ToolRunner -FilePath $dotnetExecutable -ArgumentList $dotnetArgs
    
        if ($toolOutput.dotnet_publish.exit_code -ne 0) {
            $payload = New-PcvInstallerError `
                -Code 'PCV_INSTALLER_SERVICE_HOST_PUBLISH_FAILED' `
                -Message 'DesktopNode.Host publish failed.' `
                -Detail $toolOutput.dotnet_publish.stderr
            $payload.tool_output = $toolOutput
    
            Write-PcvJsonAndExit `
                -ExitCode $toolOutput.dotnet_publish.exit_code `
                -Payload $payload
        }
    
        $resolvedDesktopNodeHostPath = Join-Path $hostPublishRoot 'DesktopNode.Host.exe'
        if (-not (Test-Path -LiteralPath $resolvedDesktopNodeHostPath -PathType Leaf)) {
            Write-PcvJsonAndExit `
                -ExitCode 1 `
                -Payload (New-PcvInstallerError `
                    -Code 'PCV_INSTALLER_SERVICE_HOST_PUBLISH_MISSING_EXE' `
                    -Message "DesktopNode.Host publish did not produce $resolvedDesktopNodeHostPath.")
        }
    
        $desktopNodeHostHash = Get-PcvFileSha256 -Path $resolvedDesktopNodeHostPath
        $plan.service_host_path = $resolvedDesktopNodeHostPath
        $plan.service_host_sha256 = $desktopNodeHostHash
    }
    
    if ([string]::IsNullOrWhiteSpace($resolvedDesktopNodeCliPath)) {
        $dotnetCliArgs = @(
            'publish',
            $cliProjectPath,
            '-c',
            'Release',
            '-r',
            'win-x64',
            '--self-contained',
            'true',
            '-p:PublishSingleFile=true',
            '-p:IncludeNativeLibrariesForSelfExtract=true'
        ) + $dotnetPublishVersionArgs + @(
            '-o',
            $cliPublishRoot
        )
        $toolOutput.dotnet_cli_publish = & $ToolRunner -FilePath $dotnetExecutable -ArgumentList $dotnetCliArgs
    
        if ($toolOutput.dotnet_cli_publish.exit_code -ne 0) {
            $payload = New-PcvInstallerError `
                -Code 'PCV_INSTALLER_CLI_PUBLISH_FAILED' `
                -Message 'DesktopNode.Cli publish failed.' `
                -Detail $toolOutput.dotnet_cli_publish.stderr
            $payload.tool_output = $toolOutput
    
            Write-PcvJsonAndExit `
                -ExitCode $toolOutput.dotnet_cli_publish.exit_code `
                -Payload $payload
        }
    
        $resolvedDesktopNodeCliPath = Join-Path $cliPublishRoot 'pcvcli.exe'
        if (-not (Test-Path -LiteralPath $resolvedDesktopNodeCliPath -PathType Leaf)) {
            Write-PcvJsonAndExit `
                -ExitCode 1 `
                -Payload (New-PcvInstallerError `
                    -Code 'PCV_INSTALLER_CLI_PUBLISH_MISSING_EXE' `
                    -Message "DesktopNode.Cli publish did not produce $resolvedDesktopNodeCliPath.")
        }
    
        $desktopNodeCliHash = Get-PcvFileSha256 -Path $resolvedDesktopNodeCliPath
        $plan.cli_path = $resolvedDesktopNodeCliPath
        $plan.cli_sha256 = $desktopNodeCliHash
    }
    
    try {
        if (Test-Path -LiteralPath $payloadRoot) {
            $resolvedPayloadRoot = (Resolve-Path -LiteralPath $payloadRoot).Path
            if (
                (Split-Path -Leaf $resolvedPayloadRoot) -ne 'payload' -or
                -not (Test-PcvChildPath -Path $resolvedPayloadRoot -Parent $outputRootFull)
            ) {
                throw "PCV_INSTALLER_INVALID_PAYLOAD_ROOT|Refusing to clean unexpected payload root.|$resolvedPayloadRoot"
            }
    
            Remove-Item -LiteralPath $resolvedPayloadRoot -Recurse -Force
        }
    
        New-Item -ItemType Directory -Path $payloadRoot -Force | Out-Null
    
        $payloadFilesToCopy = @(
            [ordered]@{
                source = Join-Path $repoRoot 'packaging\windows-desktop-node\Invoke-PcvDesktopNodeProduct.ps1'
                destination = Join-Path $payloadRoot 'Invoke-PcvDesktopNodeProduct.ps1'
            },
            [ordered]@{
                source = $modulePath
                destination = Join-Path $payloadRoot 'PcvDesktopNodeProduct.psm1'
            },
            [ordered]@{
                source = $resolvedDesktopNodeHostPath
                destination = Join-Path $payloadRoot 'DesktopNode.Host.exe'
            },
            [ordered]@{
                source = $resolvedDesktopNodeCliPath
                destination = Join-Path $payloadRoot 'pcvcli.exe'
            }
        )
    
        $webRuntimeFiles = @(
            'web\app.js',
            'web\index.html',
            'web\styles.css'
        )
        foreach ($relativePath in $webRuntimeFiles) {
            $payloadFilesToCopy += [ordered]@{
                source = Join-Path $repoRoot $relativePath
                destination = Join-Path $payloadRoot $relativePath
            }
        }
    
        foreach ($payloadFile in $payloadFilesToCopy) {
            if (-not (Test-Path -LiteralPath $payloadFile.source -PathType Leaf)) {
                throw "PCV_INSTALLER_PAYLOAD_SOURCE_MISSING|Payload source file was not found.|$($payloadFile.source)"
            }
    
            New-Item -ItemType Directory -Path (Split-Path -Parent $payloadFile.destination) -Force | Out-Null
            Copy-Item -LiteralPath $payloadFile.source -Destination $payloadFile.destination -Force -ErrorAction Stop
        }
    
        Import-Module $modulePath -Force
        $payloadManifest = New-PcvDesktopNodeProductManifest `
            -SourceRoot $payloadRoot `
            -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
            -DataRoot (Join-Path $env:ProgramData 'PureCVisor\desktop-node') `
            -Version $Version
        $payloadManifest | ConvertTo-Json -Depth 32 |
            Set-Content -LiteralPath (Join-Path $payloadRoot 'product-manifest.json') -Encoding UTF8 -ErrorAction Stop
    }
    catch {
        Write-PcvJsonAndExit `
            -ExitCode 1 `
            -Payload (New-PcvInstallerErrorFromException `
                -ErrorRecord $_ `
                -DefaultCode 'PCV_INSTALLER_PAYLOAD_STAGE_FAILED' `
                -DefaultMessage 'Payload staging failed.')
    }
    
    $wixArgs = @(
        'build'
    ) + $wixSourcePaths + @(
        '-arch',
        'x64',
        '-define',
        "MsiProductVersion=$msiProductVersion",
        '-define',
        "PayloadRoot=$payloadRoot",
        '-out',
        $msiPath
    )
    $toolOutput.wix = & $ToolRunner -FilePath $wixExecutable -ArgumentList $wixArgs
    
    if ($toolOutput.wix.exit_code -ne 0) {
        $payload = New-PcvInstallerError `
            -Code 'PCV_INSTALLER_WIX_BUILD_FAILED' `
            -Message 'WiX build failed.' `
            -Detail $toolOutput.wix.stderr
        $payload.tool_output = $toolOutput
    
        Write-PcvJsonAndExit `
            -ExitCode $toolOutput.wix.exit_code `
            -Payload $payload
    }
    
    if ($SigningMode -eq 'RequireSigned') {
        $signArgs = @('sign', '/fd', 'SHA256', '/tr', $TimestampUrl, '/td', 'SHA256')
        if (-not [string]::IsNullOrWhiteSpace($CertificateThumbprint)) {
            $signArgs += @('/sha1', $CertificateThumbprint)
        }
        else {
            $signArgs += @('/f', $CertificatePath)
        }
        $signArgs += $msiPath
    
        $signRedactions = @($CertificateThumbprint, $CertificatePath)
        $toolOutput.signtool = & $ToolRunner `
            -FilePath $resolvedSignToolPath `
            -ArgumentList $signArgs `
            -RedactionValues $signRedactions
    
        if ($toolOutput.signtool.exit_code -ne 0) {
            $payload = New-PcvInstallerError `
                -Code 'PCV_INSTALLER_SIGNING_FAILED' `
                -Message 'MSI signing failed.' `
                -Detail $toolOutput.signtool.stderr
            $payload.tool_output = $toolOutput
    
            Write-PcvJsonAndExit `
                -ExitCode $toolOutput.signtool.exit_code `
                -Payload $payload
        }
    }
    
    $payloadDigest = Get-PcvPayloadAggregateSha256 -PayloadRoot $payloadRoot
    $payloadFiles = $payloadDigest.files
    $aggregateHash = $payloadDigest.aggregate_sha256
    $msiHash = Get-PcvFileSha256 -Path $msiPath
    $buildUtc = (Get-Date).ToUniversalTime().ToString('o')
    
    $provenance = [ordered]@{
        schema_version = '1'
        product = [ordered]@{
            name = 'PureCVisor Desktop Node'
            version = $Version
            release_channel = $releaseChannel
            msi_product_version = $msiProductVersion
        }
        git_commit = Get-PcvGitCommit -RepositoryRoot $repoRoot
        build_utc = $buildUtc
        wix = [ordered]@{
            version = Get-PcvWixVersion -Command $WixPath
            source_files = $wixSourcePaths
        }
        msi = [ordered]@{
            path = $msiPath
            sha256 = $msiHash
            signed = ($SigningMode -eq 'RequireSigned')
        }
        payload = [ordered]@{
            root = $payloadRoot
            file_count = $payloadFiles.Count
            aggregate_sha256 = $aggregateHash
            product_wrapper_sha256 = $moduleHash
        }
        service_host = [ordered]@{
            source_path = $resolvedDesktopNodeHostPath
            source = $desktopNodeHostSource
            sha256 = $desktopNodeHostHash
            service_mode = 'dotnet-windows-service'
            signature_status = 'not-verified-by-installer-script'
        }
        cli = [ordered]@{
            source_path = $resolvedDesktopNodeCliPath
            source = $desktopNodeCliSource
            sha256 = $desktopNodeCliHash
            mode = 'dotnet-local-api-client'
            signature_status = 'not-verified-by-installer-script'
        }
        signing_mode = $SigningMode
        signing_trust_model = $SigningTrustModel
        host = [ordered]@{
            os = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
            powershell = $PSVersionTable.PSVersion.ToString()
        }
        publication = $publicationPlan
    }
    
    $publicationDescriptor = [ordered]@{
        schema_version = '1'
        product = [ordered]@{
            name = 'PureCVisor Desktop Node'
            version = $Version
            release_channel = $releaseChannel
            msi_product_version = $msiProductVersion
        }
        generated_utc = $buildUtc
        artifact = [ordered]@{
            base_name = $artifactBaseName
            architecture = $artifactArchitecture
            msi_path = $msiPath
            msi_sha256 = $msiHash
            msi_sha256_path = $msiSha256Path
            provenance_path = $provenancePath
            signing_mode = $SigningMode
            signing_trust_model = $SigningTrustModel
        }
        publication = $publicationPlan
    }
    
    $provenance | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $provenancePath -Encoding UTF8
    $publicationDescriptor | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $publicationPath -Encoding UTF8
    "$msiHash  $(Split-Path -Leaf $msiPath)" | Set-Content -LiteralPath $msiSha256Path -Encoding ASCII
    
    Write-PcvJsonAndExit -ExitCode 0 -Payload ([ordered]@{
        ok = $true
        msi_path = $msiPath
        provenance_path = $provenancePath
        msi_sha256_path = $msiSha256Path
        publication_path = $publicationPath
        provenance = $provenance
        publication_descriptor = $publicationDescriptor
        tool_output = $toolOutput
    })
}

function Invoke-PcvDesktopNodeInstallerBuild {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][Alias('Input')][hashtable]$BuildInput,
        [scriptblock]$ToolRunner = {
            param(
                [Parameter(Mandatory)][string]$FilePath,
                [string[]]$ArgumentList = @(),
                [string[]]$RedactionValues = @()
            )
            Invoke-PcvInstallerProcess -FilePath $FilePath -ArgumentList $ArgumentList -RedactionValues $RedactionValues
        }
    )

    try {
        Invoke-PcvInstallerBuildCore -BuildInput $BuildInput -ToolRunner $ToolRunner
    }
    catch {
        if ($_.Exception.Message -eq '__PCV_INSTALLER_BUILD_RESULT__') {
            $payload = $_.Exception.Data['Payload']
            $payload.exit_code = [int]$_.Exception.Data['ExitCode']
            return $payload
        }

        $payload = New-PcvInstallerErrorFromException `
            -ErrorRecord $_ `
            -DefaultCode 'PCV_INSTALLER_BUILD_FAILED' `
            -DefaultMessage 'Installer build failed.'
        $payload.exit_code = 1
        $payload
    }
}

Export-ModuleMember -Function Invoke-PcvDesktopNodeInstallerBuild
