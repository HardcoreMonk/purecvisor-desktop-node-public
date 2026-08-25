BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $script:BuildScript = Join-Path $script:InstallerRoot 'build.ps1'
    . (Join-Path $PSScriptRoot 'PcvDesktopNodeInstaller.TestSupport.ps1') -InstallerRoot $script:InstallerRoot
    $script:SchemaPath = Join-Path $script:InstallerRoot 'installer-provenance.schema.json'

    $script:NewFakeWixCli = {
        param(
            [Parameter(Mandatory)]
            [string]$Path
        )

        @'
@echo off
setlocal
if "%~1"=="--version" (
  echo fake-wix-5.0.2
  exit /b 0
)
:next
if "%~1"=="" goto done
if /I "%~1"=="-out" goto write_output
if /I "%~1"=="-o" goto write_output
shift
goto next
:write_output
shift
if "%~1"=="" exit /b 91
< nul set /p=fake-msi > "%~1"
shift
goto next
:done
exit /b 0
'@ | Set-Content -LiteralPath $Path -Encoding ASCII
    }
}

Describe 'Desktop Node installer signing and provenance contract' {
    It 'defines required provenance fields' {
        $schema = Get-Content -Raw -LiteralPath $script:SchemaPath | ConvertFrom-Json

        $schema.'$schema' | Should -Be 'https://json-schema.org/draft/2020-12/schema'
        $schema.required | Should -Contain 'schema_version'
        $schema.required | Should -Contain 'product'
        $schema.required | Should -Contain 'git_commit'
        $schema.required | Should -Contain 'build_utc'
        $schema.required | Should -Contain 'wix'
        $schema.required | Should -Contain 'msi'
        $schema.required | Should -Contain 'payload'
        $schema.required | Should -Contain 'service_host'
        $schema.required | Should -Contain 'cli'
        $schema.required | Should -Contain 'signing_mode'
        $schema.required | Should -Contain 'signing_trust_model'
        $schema.required | Should -Contain 'host'
        $schema.properties.product.required | Should -Contain 'msi_product_version'
        $schema.properties.product.required | Should -Contain 'release_channel'
        $schema.properties.wix.required | Should -Contain 'source_files'
        $schema.properties.wix.required | Should -Not -Contain 'source_project'
        $schema.properties.signing_trust_model.enum | Should -Contain 'LocalTest'
        $schema.properties.signing_trust_model.enum | Should -Contain 'InternalEnterprise'
        $schema.properties.signing_trust_model.enum | Should -Contain 'PublicTrusted'
    }

    It 'accepts release signing input without writing certificate secrets into dry-run output' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $signtool = Join-Path $TestDrive 'signtool.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        Set-Content -LiteralPath $signtool -Value 'fake-signtool' -NoNewline

        $json = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode RequireSigned `
            -SigningTrustModel LocalTest `
            -SignToolPath $signtool `
            -CertificateThumbprint '00112233445566778899AABBCCDDEEFF00112233' `
            -TimestampUrl 'https://timestamp.example.invalid' `
            -DryRun

        $LASTEXITCODE | Should -Be 0
        $json | Should -Not -Match '00112233445566778899AABBCCDDEEFF00112233'
        $json | Should -Not -Match 'pfx'

        $output = $json | ConvertFrom-Json
        $output.ok | Should -BeTrue
        $output.plan.signing_mode | Should -Be 'RequireSigned'
        $output.plan.signing_trust_model | Should -Be 'LocalTest'
        $output.plan.signing_inputs.has_signtool | Should -BeTrue
        $output.plan.signing_inputs.has_certificate | Should -BeTrue
        $output.plan.signing_inputs.has_timestamp | Should -BeTrue
    }

    It 'records InternalEnterprise provenance without writing certificate secrets' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $fakeWix = Join-Path $TestDrive 'wix.cmd'
        $signtool = Join-Path $TestDrive 'signtool.cmd'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        & $script:NewFakeWixCli -Path $fakeWix
        @'
@echo off
echo fake signtool signed
exit /b 0
'@ | Set-Content -LiteralPath $signtool -Encoding ASCII

        $secretThumbprint = @(
            '0011223344556677'
            '8899AABBCCDDEEFF'
            '00112233'
        ) -join ''
        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.24.0-rc.1' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'internal-enterprise-out') `
            -SigningMode RequireSigned `
            -SigningTrustModel InternalEnterprise `
            -SignToolPath $signtool `
            -CertificateThumbprint $secretThumbprint `
            -TimestampUrl 'https://timestamp.example.invalid' `
            -WixPath $fakeWix

        $LASTEXITCODE | Should -Be 0
        $jsonText | Should -Not -Match $secretThumbprint
        $jsonText | Should -Not -Match '(?i)pfx|password|private key'

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeTrue
        $output.provenance.product.release_channel | Should -Be 'rc'
        $output.provenance.signing_mode | Should -Be 'RequireSigned'
        $output.provenance.signing_trust_model | Should -Be 'InternalEnterprise'
        $output.provenance.msi.signed | Should -BeTrue
        ($output.tool_output.signtool.arguments -join ' ') | Should -Match '\[redacted\]'
        ($output.tool_output.signtool.arguments -join ' ') | Should -Not -Match $secretThumbprint
    }

    It 'requires an explicit trust model for signed release builds' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $signtool = Join-Path $TestDrive 'signtool.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        Set-Content -LiteralPath $signtool -Value 'fake-signtool' -NoNewline

        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode RequireSigned `
            -SignToolPath $signtool `
            -CertificateThumbprint '00112233445566778899AABBCCDDEEFF00112233' `
            -TimestampUrl 'https://timestamp.example.invalid' `
            -DryRun 2>$null

        $LASTEXITCODE | Should -Be 1
        $jsonText | Should -Not -BeNullOrEmpty

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_SIGNING_TRUST_MODEL_REQUIRED'
    }

    It 'returns structured JSON when release SignTool input is missing' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $missingSigntool = Join-Path $TestDrive 'missing-signtool.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode RequireSigned `
            -SignToolPath $missingSigntool `
            -CertificateThumbprint '00112233445566778899AABBCCDDEEFF00112233' `
            -TimestampUrl 'https://timestamp.example.invalid' `
            -DryRun 2>$null

        $LASTEXITCODE | Should -Be 1
        $jsonText | Should -Not -BeNullOrEmpty

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_SIGNTOOL_NOT_FOUND'
    }

    It 'returns parseable JSON with captured SignTool output when signing fails' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $fakeWix = Join-Path $TestDrive 'wix.cmd'
        $signtool = Join-Path $TestDrive 'signtool.cmd'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        & $script:NewFakeWixCli -Path $fakeWix
        @'
@echo off
echo fake signtool stdout progress
echo fake signtool stderr detail 1>&2
exit /b 7
'@ | Set-Content -LiteralPath $signtool -Encoding ASCII

        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'sign-fail-out') `
            -SigningMode RequireSigned `
            -SigningTrustModel LocalTest `
            -SignToolPath $signtool `
            -CertificateThumbprint '00112233445566778899AABBCCDDEEFF00112233' `
            -TimestampUrl 'https://timestamp.example.invalid' `
            -WixPath $fakeWix 2>$null

        $LASTEXITCODE | Should -Be 7
        $jsonText.TrimStart() | Should -Match '^\{"ok":false'

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_SIGNING_FAILED'
        $output.tool_output.signtool.stdout | Should -Match 'fake signtool stdout progress'
        $output.tool_output.signtool.stderr | Should -Match 'fake signtool stderr detail'
        ($output.tool_output.signtool.arguments -join ' ') | Should -Match '\[redacted\]'
        ($output.tool_output.signtool.arguments -join ' ') | Should -Not -Match '00112233445566778899AABBCCDDEEFF00112233'
    }
}
