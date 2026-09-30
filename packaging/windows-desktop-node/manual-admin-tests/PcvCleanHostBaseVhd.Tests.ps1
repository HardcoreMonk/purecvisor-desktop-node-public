Set-StrictMode -Version Latest

Describe 'PcvCleanHostBaseVhd contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/New-PcvCleanHostBaseVhd.ps1'

        # The tool defines these helpers itself; stubs let Pester mock them for the in-process invocation.
        function Test-PcvElevated { $true }
        function Get-PcvOfflineImageBuild { param([string]$MountPath) }

        function Get-TestSha256([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }

        function New-TestInputs {
            param([Parameter(Mandatory)][string]$Root)
            $cache = Join-Path $Root 'image-cache'
            New-Item -ItemType Directory -Force -Path $cache | Out-Null
            $source = Join-Path $cache '20348.169.amd64fre.fe_release_svc_refresh.210806-2348_server_serverdatacentereval_en-us.vhd'
            Set-Content -LiteralPath $source -Value 'fake-evaluation-vhd' -Encoding utf8
            $package = Join-Path $Root 'windows10.0-kb5122882-x64_0123456789abcdef.msu'
            Set-Content -LiteralPath $package -Value 'fake-lcu-package' -Encoding utf8
            [ordered]@{
                SourceVhdPath = $source
                PackagePath = $package
                ExpectedKb = 'KB5122882'
                ExpectedPackageSha256 = Get-TestSha256 $package
                ExpectedUbr = 5622
                BuildDate = '20260930'
                MountRoot = Join-Path $Root 'mount'
            }
        }

        function Get-TestTree([string]$Root) {
            @(Get-ChildItem -LiteralPath $Root -Recurse -Force | ForEach-Object { $_.FullName } | Sort-Object)
        }
    }

    BeforeEach {
        $script:Root = Join-Path $TestDrive ([Guid]::NewGuid().ToString('n'))
        $script:Inputs = New-TestInputs -Root $script:Root
        $script:SourceSha = Get-TestSha256 $script:Inputs.SourceVhdPath
        $script:BasePath = Join-Path (Split-Path -Parent $script:Inputs.SourceVhdPath) '20348.5622-20260930.vhd'
        Mock Test-PcvElevated { $true }
        Mock Mount-WindowsImage { }
        Mock Add-WindowsPackage { }
        Mock Dismount-WindowsImage { }
        Mock Get-PcvOfflineImageBuild { [pscustomobject]@{ build = 20348; ubr = 5622; edition_id = 'ServerDatacenterEval' } }
    }

    It 'reports a plan without writing anything unless -Execute is passed' {
        $before = Get-TestTree $script:Root

        $plan = & $script:EntryPoint @script:Inputs

        $plan.mode | Should -Be 'plan'
        $plan.writes_performed | Should -BeFalse
        $plan.download_performed | Should -BeFalse
        $plan.source_modified | Should -BeFalse
        $plan.current_base_changed | Should -BeFalse
        $plan.base_path | Should -Be $script:BasePath
        $plan.steps | Should -Be @('copy-source', 'mount-image', 'add-package', 'verify-offline-ubr', 'dismount-save', 'hash-base', 'write-sidecar')
        Get-TestTree $script:Root | Should -Be $before
        Should -Invoke Mount-WindowsImage -Times 0 -Exactly
        Should -Invoke Test-PcvElevated -Times 0 -Exactly
    }

    It 'rejects <name> before writing anything' -TestCases @(
        @{ name = 'a package hash mismatch'; key = 'ExpectedPackageSha256'; value = ('0' * 64); code = 'PCV_BASE_VHD_PACKAGE_HASH_MISMATCH' }
        @{ name = 'a malformed KB'; key = 'ExpectedKb'; value = '5122882'; code = 'PCV_BASE_VHD_KB_INVALID' }
        @{ name = 'a package named for another KB'; key = 'ExpectedKb'; value = 'KB5099999'; code = 'PCV_BASE_VHD_PACKAGE_KB_MISMATCH' }
        @{ name = 'a malformed build date'; key = 'BuildDate'; value = '2026-09-30'; code = 'PCV_BASE_VHD_DATE_INVALID' }
    ) {
        param($name, $key, $value, $code)
        $before = Get-TestTree $script:Root
        $script:Inputs[$key] = $value

        { & $script:EntryPoint @script:Inputs -Execute } | Should -Throw "*$code*"

        Get-TestTree $script:Root | Should -Be $before
    }

    It 'never overwrites an existing base or the source' {
        Set-Content -LiteralPath $script:BasePath -Value 'older-base' -Encoding utf8

        { & $script:EntryPoint @script:Inputs -Execute } | Should -Throw '*PCV_BASE_VHD_TARGET_EXISTS*'

        Get-Content -Raw -LiteralPath $script:BasePath | Should -Match 'older-base'
        Get-TestSha256 $script:Inputs.SourceVhdPath | Should -Be $script:SourceSha
        Should -Invoke Mount-WindowsImage -Times 0 -Exactly
    }

    It 'requires elevation before copying the source' {
        Mock Test-PcvElevated { $false }

        { & $script:EntryPoint @script:Inputs -Execute } | Should -Throw '*PCV_BASE_VHD_ELEVATION_REQUIRED*'

        Test-Path -LiteralPath $script:BasePath | Should -BeFalse
    }

    It 'services a copy, saves it after the offline UBR check, and writes the sidecar' {
        $result = & $script:EntryPoint @script:Inputs -Execute

        $result.mode | Should -Be 'execute'
        $result.offline_ubr | Should -Be 5622
        Should -Invoke Mount-WindowsImage -Times 1 -Exactly -ParameterFilter { $ImagePath -eq $script:BasePath -and $Index -eq 1 }
        Should -Invoke Add-WindowsPackage -Times 1 -Exactly -ParameterFilter { $PackagePath -eq $script:Inputs.PackagePath }
        Should -Invoke Dismount-WindowsImage -Times 1 -Exactly -ParameterFilter { $Save }
        Should -Invoke Dismount-WindowsImage -Times 0 -Exactly -ParameterFilter { $Discard }
        Get-TestSha256 $script:Inputs.SourceVhdPath | Should -Be $script:SourceSha
        Test-Path -LiteralPath $script:Inputs.MountRoot | Should -BeFalse

        $sidecar = Get-Content -Raw -LiteralPath "$($script:BasePath).base.json" | ConvertFrom-Json
        $sidecar.schema | Should -Be 'pcv-clean-host-base-vhd-v1'
        $sidecar.base_file | Should -Be '20348.5622-20260930.vhd'
        $sidecar.base_sha256 | Should -Be (Get-TestSha256 $script:BasePath)
        $sidecar.base_size_bytes | Should -Be (Get-Item -LiteralPath $script:BasePath).Length
        $sidecar.build | Should -Be 20348
        $sidecar.ubr | Should -Be 5622
        $sidecar.kb | Should -Be 'KB5122882'
        $sidecar.package_sha256 | Should -Be $script:Inputs.ExpectedPackageSha256
        $sidecar.source_sha256 | Should -Be $script:SourceSha
        $sidecar.component_cleanup | Should -BeFalse
        $sidecar.download_performed | Should -BeFalse
        Test-Path -LiteralPath (Join-Path (Split-Path -Parent $script:BasePath) 'current-base.json') | Should -BeFalse
    }

    It 'discards the mount and deletes the copy when <name>' -TestCases @(
        @{ name = 'the offline UBR does not match'; failure = 'ubr'; code = 'PCV_BASE_VHD_UBR_MISMATCH' }
        @{ name = 'the package fails to apply'; failure = 'package'; code = 'DISM add package failed' }
    ) {
        param($name, $failure, $code)
        if ($failure -eq 'ubr') {
            Mock Get-PcvOfflineImageBuild { [pscustomobject]@{ build = 20348; ubr = 5000; edition_id = 'ServerDatacenterEval' } }
        }
        else {
            Mock Add-WindowsPackage { throw 'DISM add package failed' }
        }

        { & $script:EntryPoint @script:Inputs -Execute } | Should -Throw "*$code*"

        Should -Invoke Dismount-WindowsImage -Times 1 -Exactly -ParameterFilter { $Discard }
        Should -Invoke Dismount-WindowsImage -Times 0 -Exactly -ParameterFilter { $Save }
        Test-Path -LiteralPath $script:BasePath | Should -BeFalse
        Test-Path -LiteralPath "$($script:BasePath).base.json" | Should -BeFalse
        Get-TestSha256 $script:Inputs.SourceVhdPath | Should -Be $script:SourceSha
    }

    Context 'current base' {
        BeforeEach {
            $script:Cache = Split-Path -Parent $script:Inputs.SourceVhdPath
            function New-TestBase([string]$Name, [string]$CreatedUtc) {
                $path = Join-Path $script:Cache $Name
                Set-Content -LiteralPath $path -Value "base-$Name" -Encoding utf8
                ([ordered]@{
                    schema = 'pcv-clean-host-base-vhd-v1'
                    base_file = $Name
                    base_sha256 = Get-TestSha256 $path
                    base_size_bytes = (Get-Item -LiteralPath $path).Length
                    build = 20348
                    ubr = [int]($Name -replace '^20348\.(\d+)-.*$', '$1')
                    kb = 'KB5122882'
                    created_utc = $CreatedUtc
                } | ConvertTo-Json) | Set-Content -LiteralPath "$path.base.json" -Encoding utf8
                $path
            }
            New-TestBase '20348.5000-20260601.vhd' '2026-06-01T00:00:00Z' | Out-Null
            New-TestBase '20348.5200-20260701.vhd' '2026-07-01T00:00:00Z' | Out-Null
            New-TestBase '20348.5400-20260801.vhd' '2026-08-01T00:00:00Z' | Out-Null
            $script:Newest = New-TestBase '20348.5622-20260930.vhd' '2026-09-30T00:00:00Z'
            $script:CurrentPath = Join-Path $script:Cache 'current-base.json'
            (@{ schema = 'pcv-clean-host-current-base-v1'; base_file = '20348.5400-20260801.vhd' } | ConvertTo-Json) |
                Set-Content -LiteralPath $script:CurrentPath -Encoding utf8
        }

        It 'plans the current switch without writing' {
            $before = Get-Content -Raw -LiteralPath $script:CurrentPath

            $plan = & $script:EntryPoint -SetCurrentBasePath $script:Newest

            $plan.mode | Should -Be 'plan'
            $plan.writes_performed | Should -BeFalse
            $plan.current.base_file | Should -Be '20348.5622-20260930.vhd'
            Get-Content -Raw -LiteralPath $script:CurrentPath | Should -Be $before
        }

        It 'writes current-base.json and only reports bases older than the two previous ones' {
            $result = & $script:EntryPoint -SetCurrentBasePath $script:Newest -Execute

            $result.prune_candidates | Should -Be @('20348.5000-20260601.vhd')
            $result.prune_performed | Should -BeFalse
            Test-Path -LiteralPath (Join-Path $script:Cache '20348.5000-20260601.vhd') | Should -BeTrue
            $current = Get-Content -Raw -LiteralPath $script:CurrentPath | ConvertFrom-Json
            $current.schema | Should -Be 'pcv-clean-host-current-base-v1'
            $current.base_file | Should -Be '20348.5622-20260930.vhd'
            $current.sidecar_file | Should -Be '20348.5622-20260930.vhd.base.json'
            $current.ubr | Should -Be 5622
            $current.previous_base_file | Should -Be '20348.5400-20260801.vhd'
        }

        It 'refuses a base whose bytes no longer match the sidecar' {
            Add-Content -LiteralPath $script:Newest -Value 'tampered'

            { & $script:EntryPoint -SetCurrentBasePath $script:Newest -Execute } | Should -Throw '*PCV_BASE_VHD_SIDECAR_MISMATCH*'

            (Get-Content -Raw -LiteralPath $script:CurrentPath | ConvertFrom-Json).base_file | Should -Be '20348.5400-20260801.vhd'
        }

        It 'refuses a VHD without a sidecar such as the evaluation source' {
            { & $script:EntryPoint -SetCurrentBasePath $script:Inputs.SourceVhdPath -Execute } | Should -Throw '*PCV_BASE_VHD_SIDECAR_MISSING*'
        }

        Context 'clean-host runner base selection' {
            BeforeAll {
                $runnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1'
                $ast = [System.Management.Automation.Language.Parser]::ParseFile($runnerPath, [ref]$null, [ref]$null)
                foreach ($name in @('Resolve-PcvFilePath', 'Resolve-PcvCleanHostBaseVhd')) {
                    $definition = $ast.Find({
                            param($node)
                            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name
                        }, $true)
                    . ([scriptblock]::Create($definition.Extent.Text))
                }
            }

            It 'uses the base named by current-base.json and reads its sidecar' {
                $selected = Resolve-PcvCleanHostBaseVhd -ExplicitPath '' -DefaultSourcePath $script:Inputs.SourceVhdPath

                $selected.source | Should -Be 'current-base'
                $selected.path | Should -Be (Join-Path $script:Cache '20348.5400-20260801.vhd')
                $selected.ubr | Should -Be 5400
                $selected.kb | Should -Be 'KB5122882'
            }

            It 'lets an explicit path win over current-base.json' {
                $selected = Resolve-PcvCleanHostBaseVhd -ExplicitPath $script:Newest -DefaultSourcePath $script:Inputs.SourceVhdPath

                $selected.source | Should -Be 'explicit'
                $selected.ubr | Should -Be 5622
            }

            It 'falls back to the evaluation VHD without current-base.json' {
                Remove-Item -LiteralPath $script:CurrentPath

                $selected = Resolve-PcvCleanHostBaseVhd -ExplicitPath '' -DefaultSourcePath $script:Inputs.SourceVhdPath

                $selected.source | Should -Be 'default-source'
                $selected.path | Should -Be $script:Inputs.SourceVhdPath
                $selected.ubr | Should -BeNullOrEmpty
            }

            It 'rejects a current-base.json that points outside the image cache' {
                (@{ schema = 'pcv-clean-host-current-base-v1'; base_file = '..\escape.vhd' } | ConvertTo-Json) |
                    Set-Content -LiteralPath $script:CurrentPath -Encoding utf8

                { Resolve-PcvCleanHostBaseVhd -ExplicitPath '' -DefaultSourcePath $script:Inputs.SourceVhdPath } |
                    Should -Throw '*PCV_CLEAN_HOST_CURRENT_BASE_INVALID*'
            }
        }
    }
}
