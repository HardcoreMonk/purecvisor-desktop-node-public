BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $script:RepoRoot = (Resolve-Path (Join-Path $script:InstallerRoot '..\..\..')).Path
    $script:BuildScript = Join-Path $script:InstallerRoot 'build.ps1'
    $script:BuildModule = Join-Path $script:InstallerRoot 'PcvDesktopNodeInstaller.Build.psm1'
    . (Join-Path $PSScriptRoot 'PcvDesktopNodeInstaller.TestSupport.ps1') -InstallerRoot $script:InstallerRoot

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
if not "%PCV_WIX_STDOUT%"=="" echo %PCV_WIX_STDOUT%
if not "%PCV_WIX_STDERR%"=="" echo %PCV_WIX_STDERR% 1>&2
break > "%PCV_WIX_ARG_FILE%"
:next
if "%~1"=="" goto done
>> "%PCV_WIX_ARG_FILE%" echo(%~1
if /I "%~1"=="-out" goto write_output
if /I "%~1"=="-o" goto write_output
shift
goto next
:write_output
shift
if "%~1"=="" exit /b 91
>> "%PCV_WIX_ARG_FILE%" echo(%~1
< nul set /p=fake-msi > "%~1"
shift
goto next
:done
if not "%PCV_WIX_EXIT_CODE%"=="" exit /b %PCV_WIX_EXIT_CODE%
exit /b 0
'@ | Set-Content -LiteralPath $Path -Encoding ASCII
    }

}

Describe 'Desktop Node installer build plan' {
    It 'exposes explicit version service host output signing and dry-run parameters' {
        $scriptText = (Get-Content -Raw -LiteralPath $script:BuildScript) + "`n" +
            (Get-Content -Raw -LiteralPath $script:BuildModule)

        $scriptText | Should -Match '\[string\]\$Version'
        $scriptText | Should -Match '\[string\]\$DesktopNodeHostPath'
        $scriptText | Should -Match '\[string\]\$DesktopNodeCliPath'
        $scriptText | Should -Match '\[string\]\$OutputRoot'
        $scriptText | Should -Match '\[string\]\$MsiProductVersion'
        $scriptText | Should -Match 'RequireSigned'
        $scriptText | Should -Match 'AllowUnsignedDev'
        $scriptText | Should -Match '\[string\]\$SigningTrustModel'
        $scriptText | Should -Match 'InternalEnterprise'
        $scriptText | Should -Match 'PublicTrusted'
        $scriptText | Should -Match "(?s)'--self-contained',\s*'true'"
        $scriptText | Should -Match 'PublishSingleFile=true'
        $scriptText | Should -Match '\$dotnetPublishFileVersion'
        $scriptText | Should -Match '\$dotnetPublishAssemblyVersion = "\$msiProductVersion\.0"'
        $scriptText | Should -Match '\[int\]\$msiProductVersionParts\[0\] \+ 1'
        $scriptText | Should -Match '-p:AssemblyVersion=\$dotnetPublishAssemblyVersion'
        $scriptText | Should -Match '-p:FileVersion=\$dotnetPublishFileVersion'
        $scriptText | Should -Match '-p:InformationalVersion=\$Version'
        $scriptText | Should -Match '\[switch\]\$DryRun'
    }

    It 'keeps the active installer boundary CLI and Web only without TUI references' {
        $scriptText = (Get-Content -Raw -LiteralPath $script:BuildScript) + "`n" +
            (Get-Content -Raw -LiteralPath $script:BuildModule)
        $productWxs = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $scriptText | Should -Not -Match 'DesktopNodeTuiPath'
        $scriptText | Should -Not -Match 'DesktopNode\.Tui'
        $scriptText | Should -Not -Match 'pcvtui\.exe'
        $scriptText | Should -Not -Match 'PCV_INSTALLER_TUI_'
        $productWxs | Should -Not -Match 'DesktopNodeTui'
        $productWxs | Should -Not -Match 'pcvtui\.exe'
        $productWxs | Should -Match '<File Id="DesktopNodeCli"'
        $productWxs | Should -Match '<File Id="DesktopNodeWebApp"'
    }

    It 'returns structured JSON when release signing input is missing' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $output = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode RequireSigned `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 1
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_SIGNING_REQUIRED'
    }

    It 'returns structured JSON when service host input is missing' {
        $missingHost = Join-Path $TestDrive 'missing-DesktopNode.Host.exe'

        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0-dev' `
            -DesktopNodeHostPath $missingHost `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun 2>$null

        $LASTEXITCODE | Should -Be 1
        $jsonText | Should -Not -BeNullOrEmpty

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_SERVICE_HOST_NOT_FOUND'
    }

    It 'returns structured JSON when CLI input is missing' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $missingCli = Join-Path $TestDrive 'missing-DesktopNode.Cli.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0-dev' `
            -DesktopNodeHostPath $hostPath `
            -DesktopNodeCliPath $missingCli `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun 2>$null

        $LASTEXITCODE | Should -Be 1
        $jsonText | Should -Not -BeNullOrEmpty

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_CLI_NOT_FOUND'
    }

    It 'rejects unsigned release-candidate builds' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.22.0-rc.1' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun 2>$null

        $LASTEXITCODE | Should -Be 1
        $jsonText | Should -Not -BeNullOrEmpty

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_RELEASE_SIGNING_REQUIRED'
    }

    It 'rejects unsigned stable builds' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $jsonText = Invoke-PcvInstallerModuleJson `
            -Version '0.22.0' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun 2>$null

        $LASTEXITCODE | Should -Be 1
        $jsonText | Should -Not -BeNullOrEmpty

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_RELEASE_SIGNING_REQUIRED'
    }

    It 'emits a dry-run plan without requiring WiX for unsigned developer builds' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $output = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0-dev' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        $output.plan.product_name | Should -Be 'PureCVisor Desktop Node'
        $output.plan.version | Should -Be '0.14.0-dev'
        $output.plan.release_channel | Should -Be 'dev'
        $output.plan.msi_product_version | Should -Be '0.14.0'
        $output.plan.signing_mode | Should -Be 'AllowUnsignedDev'
        $output.plan.signing_trust_model | Should -Be 'Unspecified'
        $output.plan.service_host_sha256 | Should -Match '^[0-9A-Fa-f]{64}$'
    }

    It 'records an explicit CLI payload in dry-run output' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $cliPath = Join-Path $TestDrive 'DesktopNode.Cli.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        Set-Content -LiteralPath $cliPath -Value 'fake-cli' -NoNewline

        $output = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0-dev' `
            -DesktopNodeHostPath $hostPath `
            -DesktopNodeCliPath $cliPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        $output.plan.cli_source | Should -Be 'explicit-path'
        $output.plan.cli_path | Should -Be $cliPath
        $output.plan.cli_sha256 | Should -Match '^[0-9A-Fa-f]{64}$'
    }

    It 'describes internal publication boundaries in dry-run output' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $output = Invoke-PcvInstallerModuleJson `
            -Version '0.39.0-dev' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'publication-plan-out') `
            -SigningMode AllowUnsignedDev `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        $output.plan.PSObject.Properties.Name | Should -Contain 'publication_path'
        Split-Path -Leaf $output.plan.publication_path |
            Should -Be 'PureCVisorDesktopNode-0.39.0-dev-windows-x64.publication.json'
        $output.plan.publication.schema_version | Should -Be '1'
        $output.plan.publication.mode | Should -Be 'internal-artifact-descriptor-only'
        $output.plan.publication.public_trusted_signing | Should -Be 'not-claimed'
        $output.plan.publication.external_stable_publication | Should -Be 'not-claimed'
        $output.plan.publication.burn_bootstrapper | Should -Be 'not-built'
        $output.plan.publication.msix | Should -Be 'not-built'
        $output.plan.publication.winget_manifest | Should -Be 'not-generated'
        $output.plan.publication.catalog_publication | Should -Be 'not-published'
    }

    It 'accepts an explicit MSI product version override in the build plan' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $output = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0-dev' `
            -MsiProductVersion '0.14.7' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        $output.plan.version | Should -Be '0.14.0-dev'
        $output.plan.msi_product_version | Should -Be '0.14.7'
    }

    It 'keeps absolute output roots absolute in the build plan' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        $outputRoot = Join-Path $TestDrive 'absolute-out'

        $output = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0-dev' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot $outputRoot `
            -SigningMode AllowUnsignedDev `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.plan.output_root | Should -Be ([System.IO.Path]::GetFullPath($outputRoot))
        $output.plan.payload_root | Should -Be (Join-Path ([System.IO.Path]::GetFullPath($outputRoot)) 'payload')
    }

    It 'records the actual WiX source files used for build input' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $output = Invoke-PcvInstallerModuleJson `
            -Version '0.14.0-dev' `
            -DesktopNodeHostPath $hostPath `
            -OutputRoot (Join-Path $TestDrive 'out') `
            -SigningMode AllowUnsignedDev `
            -DryRun |
            ConvertFrom-Json

        $LASTEXITCODE | Should -Be 0
        $output.plan.PSObject.Properties.Name | Should -Contain 'wix_source_files'
        $output.plan.PSObject.Properties.Name | Should -Not -Contain 'wix_project'
        $output.plan.wix_source_files | Where-Object { $_ -like '*Product.wxs' } | Should -Not -BeNullOrEmpty
        $output.plan.wix_source_files | Where-Object { $_ -like '*ProductActions.wxs' } | Should -Not -BeNullOrEmpty
    }

    It 'invokes WiX CLI with WiX source files instead of the project file' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $fakeWix = Join-Path $TestDrive 'wix.cmd'
        $argFile = Join-Path $TestDrive 'wix-args.json'
        & $script:NewFakeWixCli -Path $fakeWix

        $previousArgFile = $env:PCV_WIX_ARG_FILE
        $env:PCV_WIX_ARG_FILE = $argFile
        try {
            Push-Location $TestDrive
            try {
                $output = Invoke-PcvInstallerModuleJson `
                    -Version '0.14.0-dev' `
                    -DesktopNodeHostPath $hostPath `
                    -OutputRoot (Join-Path $TestDrive 'out') `
                    -SigningMode AllowUnsignedDev `
                    -WixPath $fakeWix |
                    ConvertFrom-Json
            }
            finally {
                Pop-Location
            }
        }
        finally {
            $env:PCV_WIX_ARG_FILE = $previousArgFile
        }

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue

        $arguments = @(Get-Content -LiteralPath $argFile)
        $arguments[0] | Should -Be 'build'
        $arguments | Where-Object { $_ -like '*.wixproj' } | Should -BeNullOrEmpty
        $arguments | Where-Object { $_ -like '*Product.wxs' } | Should -Not -BeNullOrEmpty
        $arguments | Where-Object { $_ -like '*ProductActions.wxs' } | Should -Not -BeNullOrEmpty
        $archIndex = [array]::IndexOf($arguments, '-arch')
        $archIndex | Should -BeGreaterThan -1
        $arguments[$archIndex + 1] | Should -Be 'x64'
        if ($arguments -contains 'MsiProductVersion=0.14.0') {
            $arguments | Should -Contain 'MsiProductVersion=0.14.0'
        }
        else {
            $versionDefineIndex = [array]::IndexOf($arguments, 'MsiProductVersion')
            $versionDefineIndex | Should -BeGreaterThan -1
            $arguments[$versionDefineIndex + 1] | Should -Be '0.14.0'
        }

        $expectedCommit = (& git -C $script:RepoRoot rev-parse HEAD | Select-Object -First 1)
        $output.provenance.git_commit | Should -Be $expectedCommit
    }

    It 'emits Phase 22 windows-x64 artifact names for MSI provenance and hash sidecar' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $fakeWix = Join-Path $TestDrive 'wix-artifacts.cmd'
        $argFile = Join-Path $TestDrive 'wix-artifacts-args.txt'
        $outputRoot = Join-Path $TestDrive 'artifact-out'
        & $script:NewFakeWixCli -Path $fakeWix

        $previousArgFile = $env:PCV_WIX_ARG_FILE
        $env:PCV_WIX_ARG_FILE = $argFile
        try {
            $output = Invoke-PcvInstallerModuleJson `
                -Version '0.22.0-dev.1' `
                -DesktopNodeHostPath $hostPath `
                -OutputRoot $outputRoot `
                -SigningMode AllowUnsignedDev `
                -WixPath $fakeWix |
                ConvertFrom-Json
        }
        finally {
            $env:PCV_WIX_ARG_FILE = $previousArgFile
        }

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue

        $expectedBase = 'PureCVisorDesktopNode-0.22.0-dev.1-windows-x64'
        Split-Path -Leaf $output.msi_path | Should -Be "$expectedBase.msi"
        Split-Path -Leaf $output.provenance_path | Should -Be "$expectedBase.provenance.json"
        Split-Path -Leaf $output.msi_sha256_path | Should -Be "$expectedBase.msi.sha256"

        Test-Path -LiteralPath $output.msi_path | Should -BeTrue
        Test-Path -LiteralPath $output.provenance_path | Should -BeTrue
        Test-Path -LiteralPath $output.msi_sha256_path | Should -BeTrue

        $expectedMsiHash = (Get-FileHash -LiteralPath $output.msi_path -Algorithm SHA256).Hash.ToLowerInvariant()
        $shaSidecar = Get-Content -Raw -LiteralPath $output.msi_sha256_path
        $shaSidecar | Should -Match $expectedMsiHash
        $shaSidecar | Should -Match "$expectedBase\.msi"
        Split-Path -Leaf $output.provenance.msi.path | Should -Be "$expectedBase.msi"
        $output.provenance.product.release_channel | Should -Be 'dev'
        $output.provenance.signing_trust_model | Should -Be 'Unspecified'
    }

    It 'writes an internal publication descriptor sidecar for built MSI artifacts' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $fakeWix = Join-Path $TestDrive 'wix-publication.cmd'
        $argFile = Join-Path $TestDrive 'wix-publication-args.txt'
        $outputRoot = Join-Path $TestDrive 'publication-out'
        & $script:NewFakeWixCli -Path $fakeWix

        $previousArgFile = $env:PCV_WIX_ARG_FILE
        $env:PCV_WIX_ARG_FILE = $argFile
        try {
            $output = Invoke-PcvInstallerModuleJson `
                -Version '0.39.0-dev' `
                -DesktopNodeHostPath $hostPath `
                -OutputRoot $outputRoot `
                -SigningMode AllowUnsignedDev `
                -WixPath $fakeWix |
                ConvertFrom-Json
        }
        finally {
            $env:PCV_WIX_ARG_FILE = $previousArgFile
        }

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue

        $expectedBase = 'PureCVisorDesktopNode-0.39.0-dev-windows-x64'
        Split-Path -Leaf $output.publication_path | Should -Be "$expectedBase.publication.json"
        Test-Path -LiteralPath $output.publication_path | Should -BeTrue

        $descriptor = Get-Content -Raw -LiteralPath $output.publication_path | ConvertFrom-Json
        $descriptor.schema_version | Should -Be '1'
        $descriptor.product.version | Should -Be '0.39.0-dev'
        $descriptor.artifact.base_name | Should -Be $expectedBase
        $descriptor.artifact.msi_sha256 | Should -Be $output.provenance.msi.sha256
        $descriptor.artifact.provenance_path | Should -Be $output.provenance_path
        $descriptor.publication.mode | Should -Be 'internal-artifact-descriptor-only'
        $descriptor.publication.public_trusted_signing | Should -Be 'not-claimed'
        $descriptor.publication.external_stable_publication | Should -Be 'not-claimed'
        $descriptor.publication.burn_bootstrapper | Should -Be 'not-built'
        $descriptor.publication.msix | Should -Be 'not-built'
        $descriptor.publication.winget_manifest | Should -Be 'not-generated'
        $descriptor.publication.catalog_publication | Should -Be 'not-published'
        $output.provenance.publication.public_trusted_signing | Should -Be 'not-claimed'
        $output.publication_descriptor.artifact.msi_sha256 | Should -Be $output.provenance.msi.sha256
    }

    It 'returns parseable JSON with captured WiX output when WiX build fails' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $fakeWix = Join-Path $TestDrive 'wix-fail.cmd'
        $argFile = Join-Path $TestDrive 'wix-fail-args.txt'
        & $script:NewFakeWixCli -Path $fakeWix

        $previousArgFile = $env:PCV_WIX_ARG_FILE
        $previousStdout = $env:PCV_WIX_STDOUT
        $previousStderr = $env:PCV_WIX_STDERR
        $previousExitCode = $env:PCV_WIX_EXIT_CODE
        $env:PCV_WIX_ARG_FILE = $argFile
        $env:PCV_WIX_STDOUT = 'fake wix stdout progress'
        $env:PCV_WIX_STDERR = 'fake wix stderr detail'
        $env:PCV_WIX_EXIT_CODE = '42'
        try {
            $jsonText = Invoke-PcvInstallerModuleJson `
                -Version '0.14.0-dev' `
                -DesktopNodeHostPath $hostPath `
                -OutputRoot (Join-Path $TestDrive 'wix-fail-out') `
                -SigningMode AllowUnsignedDev `
                -WixPath $fakeWix 2>$null
        }
        finally {
            $env:PCV_WIX_ARG_FILE = $previousArgFile
            $env:PCV_WIX_STDOUT = $previousStdout
            $env:PCV_WIX_STDERR = $previousStderr
            $env:PCV_WIX_EXIT_CODE = $previousExitCode
        }

        $LASTEXITCODE | Should -Be 42
        $jsonText.TrimStart() | Should -Match '^\{"ok":false'

        $output = $jsonText | ConvertFrom-Json
        $output.ok | Should -BeFalse
        $output.error.code | Should -Be 'PCV_INSTALLER_WIX_BUILD_FAILED'
        $output.tool_output.wix.stdout | Should -Match 'fake wix stdout progress'
        $output.tool_output.wix.stderr | Should -Match 'fake wix stderr detail'
    }

    It 'builds explicit Host and CLI payloads without requiring dotnet' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        $cliPath = Join-Path $TestDrive 'pcvcli.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline
        Set-Content -LiteralPath $cliPath -Value 'fake-cli' -NoNewline

        $fakeWix = Join-Path $TestDrive 'wix-all-explicit.cmd'
        $argFile = Join-Path $TestDrive 'wix-all-explicit-args.txt'
        $outputRoot = Join-Path $TestDrive 'all-explicit-out'
        $pathWithoutDotnet = Join-Path $TestDrive 'path-without-dotnet'
        New-Item -ItemType Directory -Path $pathWithoutDotnet -Force | Out-Null
        & $script:NewFakeWixCli -Path $fakeWix

        $pwshPath = (Get-Command -Name pwsh -ErrorAction Stop).Source
        $previousPath = $env:PATH
        $previousArgFile = $env:PCV_WIX_ARG_FILE
        $env:PATH = $pathWithoutDotnet
        $env:PCV_WIX_ARG_FILE = $argFile
        try {
            $output = Invoke-PcvInstallerModuleJson `
                -Version '0.14.0-dev' `
                -DesktopNodeHostPath $hostPath `
                -DesktopNodeCliPath $cliPath `
                -OutputRoot $outputRoot `
                -SigningMode AllowUnsignedDev `
                -WixPath $fakeWix |
                ConvertFrom-Json
        }
        finally {
            $env:PATH = $previousPath
            $env:PCV_WIX_ARG_FILE = $previousArgFile
        }

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        $payloadRoot = Join-Path $outputRoot 'payload'
        Test-Path -LiteralPath (Join-Path $payloadRoot 'DesktopNode.Host.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvtui.exe') | Should -BeFalse
        $output.provenance.service_host.source | Should -Be 'explicit-path'
        $output.provenance.cli.source | Should -Be 'explicit-path'
        $output.provenance.PSObject.Properties.Name | Should -Not -Contain 'tui'
    }

    It 'cleans an existing payload directory before staging build files' {
        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $outputRoot = Join-Path $TestDrive 'reused-out'
        $payloadRoot = Join-Path $outputRoot 'payload'
        New-Item -ItemType Directory -Path $payloadRoot -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $payloadRoot 'stale.txt') -Value 'stale'

        $fakeWix = Join-Path $TestDrive 'wix-clean.cmd'
        $argFile = Join-Path $TestDrive 'wix-clean-args.txt'
        & $script:NewFakeWixCli -Path $fakeWix

        $previousArgFile = $env:PCV_WIX_ARG_FILE
        $env:PCV_WIX_ARG_FILE = $argFile
        try {
            $output = Invoke-PcvInstallerModuleJson `
                -Version '0.14.0-dev' `
                -DesktopNodeHostPath $hostPath `
                -OutputRoot $outputRoot `
                -SigningMode AllowUnsignedDev `
                -WixPath $fakeWix |
                ConvertFrom-Json
        }
        finally {
            $env:PCV_WIX_ARG_FILE = $previousArgFile
        }

        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'stale.txt') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvtui.exe') | Should -BeFalse
        $output.provenance.payload.file_count | Should -Be 8
        $output.provenance.cli.sha256 | Should -Match '^[0-9A-Fa-f]{64}$'
        $output.provenance.PSObject.Properties.Name | Should -Not -Contain 'tui'
    }

    It 'stages only product-owned MSI runtime files without active spike payload sources' {
        $scriptText = (Get-Content -Raw -LiteralPath $script:BuildScript) + "`n" +
            (Get-Content -Raw -LiteralPath $script:BuildModule)
        $scriptText | Should -Not -Match 'spikes[\\/]purecvisor-desktop-node'

        $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
        Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

        $outputRoot = Join-Path $TestDrive 'runtime-out'
        $fakeWix = Join-Path $TestDrive 'wix-runtime.cmd'
        $argFile = Join-Path $TestDrive 'wix-runtime-args.txt'
        & $script:NewFakeWixCli -Path $fakeWix

        $previousArgFile = $env:PCV_WIX_ARG_FILE
        $env:PCV_WIX_ARG_FILE = $argFile
        try {
            $output = Invoke-PcvInstallerModuleJson `
                -Version '0.14.0-dev' `
                -DesktopNodeHostPath $hostPath `
                -OutputRoot $outputRoot `
                -SigningMode AllowUnsignedDev `
                -WixPath $fakeWix |
                ConvertFrom-Json
        }
        finally {
            $env:PCV_WIX_ARG_FILE = $previousArgFile
        }

        $payloadRoot = Join-Path $outputRoot 'payload'
        $LASTEXITCODE | Should -Be 0
        $output.ok | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'DesktopNode.Host.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvtui.exe') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $payloadRoot 'Invoke-PcvDesktopNodeProduct.ps1') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'PcvDesktopNodeProduct.psm1') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'web\index.html') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $payloadRoot 'api') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $payloadRoot 'hyperv') | Should -BeFalse
        Test-Path -LiteralPath (Join-Path $payloadRoot 'service') | Should -BeFalse
        $manifest = Get-Content -LiteralPath (Join-Path $payloadRoot 'product-manifest.json') -Raw | ConvertFrom-Json
        $manifest.schema_version | Should -Be 2
        $manifest.version | Should -Be '0.14.0-dev'
        $manifest.service_host.mode | Should -Be 'dotnet-windows-service'
        $manifest.update.installed_manifest_is_source_of_truth | Should -BeTrue
        @(Get-ChildItem -LiteralPath $payloadRoot -Recurse -Directory |
            Where-Object { $_.Name -eq 'tests' }).Count | Should -Be 0
        $output.provenance.payload.file_count | Should -Be 8
        $output.provenance.cli.source_path | Should -Match 'pcvcli\.exe$'
        $output.provenance.PSObject.Properties.Name | Should -Not -Contain 'tui'
    }

    It 'removes legacy WinSW root files during current MSI install' {
        $productWxs = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $productWxs | Should -Match '<RemoveFile Id="RemoveLegacyWinSwRootExe" Name="PureCVisorDesktopNode\.exe" On="install" />'
        $productWxs | Should -Match '<RemoveFile Id="RemoveLegacyWinSwRootXml" Name="PureCVisorDesktopNode\.xml" On="install" />'
    }
}
