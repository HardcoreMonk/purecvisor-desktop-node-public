BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $script:BuildScript = Join-Path $script:InstallerRoot 'build.ps1'

    function New-PcvInstallerWrapperFakeWix {
        param([Parameter(Mandatory)][string]$Path)
        @'
@echo off
if "%~1"=="--version" (
  echo fake-wix-5.0.2
  exit /b 0
)
:next
if "%~1"=="" exit /b 0
if /I "%~1"=="-out" goto write_output
shift
goto next
:write_output
shift
if "%~1"=="" exit /b 91
< nul set /p=fake-msi > "%~1"
exit /b 0
'@ | Set-Content -LiteralPath $Path -Encoding ASCII
    }
}

Describe 'Desktop Node installer process wrapper' {
    It 'returns JSON and exit zero for an unsigned dry run' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $cliPath = Join-Path $TestDrive 'pcvcli.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        Set-Content -LiteralPath $cliPath -Value 'fake-cli' -NoNewline

        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:BuildScript `
            -Version '0.42.65-dev' `
            -DesktopNodeHostPath $hostPath `
            -DesktopNodeCliPath $cliPath `
            -OutputRoot (Join-Path $TestDrive 'dry-run-out') `
            -SigningMode AllowUnsignedDev `
            -DryRun

        $LASTEXITCODE | Should -Be 0
        $payload = $json | ConvertFrom-Json
        $payload.ok | Should -BeTrue
        $payload.dry_run | Should -BeTrue
    }

    It 'returns structured JSON for a missing service host' {
        $missingHost = Join-Path $TestDrive 'missing-DesktopNode.Host.exe'

        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:BuildScript `
            -Version '0.42.65-dev' `
            -DesktopNodeHostPath $missingHost `
            -OutputRoot (Join-Path $TestDrive 'missing-host-out') `
            -SigningMode AllowUnsignedDev `
            -DryRun 2>$null

        $LASTEXITCODE | Should -Be 1
        $payload = $json | ConvertFrom-Json
        $payload.ok | Should -BeFalse
        $payload.error.code | Should -Be 'PCV_INSTALLER_SERVICE_HOST_NOT_FOUND'
    }

    It 'preserves signing failure exit code while redacting the certificate thumbprint' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $cliPath = Join-Path $TestDrive 'pcvcli.exe'
        $wixPath = Join-Path $TestDrive 'wix.cmd'
        $signToolPath = Join-Path $TestDrive 'signtool.cmd'
        $thumbprint = '00112233445566778899AABBCCDDEEFF00112233'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        Set-Content -LiteralPath $cliPath -Value 'fake-cli' -NoNewline
        New-PcvInstallerWrapperFakeWix -Path $wixPath
        @'
@echo off
echo wrapper signtool stdout
echo wrapper signtool stderr 1>&2
exit /b 7
'@ | Set-Content -LiteralPath $signToolPath -Encoding ASCII

        $json = & pwsh -NoProfile -ExecutionPolicy Bypass -File $script:BuildScript `
            -Version '0.42.65' `
            -DesktopNodeHostPath $hostPath `
            -DesktopNodeCliPath $cliPath `
            -OutputRoot (Join-Path $TestDrive 'signing-failure-out') `
            -SigningMode RequireSigned `
            -SigningTrustModel LocalTest `
            -SignToolPath $signToolPath `
            -CertificateThumbprint $thumbprint `
            -TimestampUrl 'https://timestamp.example.invalid' `
            -WixPath $wixPath 2>$null

        $LASTEXITCODE | Should -Be 7
        $json | Should -Not -Match $thumbprint
        $payload = $json | ConvertFrom-Json
        $payload.error.code | Should -Be 'PCV_INSTALLER_SIGNING_FAILED'
        ($payload.tool_output.signtool.arguments -join ' ') | Should -Match '\[redacted\]'
    }
}
