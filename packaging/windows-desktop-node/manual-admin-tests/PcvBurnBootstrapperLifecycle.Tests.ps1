Set-StrictMode -Version Latest

Describe 'PcvBurnBootstrapperLifecycle PlanOnly contract' {
    BeforeAll {
        $script:BurnEntry = Join-Path $PSScriptRoot '../tools/Invoke-PcvBurnBootstrapperLifecycle.ps1'
        function New-BurnFixture([string]$Root, [string]$Version = '0.42.76-admin-smoke') {
            New-Item -ItemType Directory -Force -Path $Root | Out-Null
            $manifest = Join-Path $Root 'product-manifest.json'
            @{ version = $Version; paths = @{ product_root = (Join-Path $Root 'product') } } | ConvertTo-Json -Depth 4 | Set-Content $manifest
            New-Item -ItemType Directory -Force -Path (Join-Path $Root 'product') | Out-Null
            $msi = Join-Path $Root 'target.msi'; Set-Content $msi 'msi'
            $wix = Join-Path $Root 'wix.exe'; Set-Content $wix 'tool'
            $marker = Join-Path $Root 'repair.marker'
            $helper = Join-Path $Root 'product.ps1'
            "param([string]`$Action); Set-Content -LiteralPath '$marker' -Value `$Action; @{ok=`$true}|ConvertTo-Json" | Set-Content $helper
            $script:burnMarker = $marker
            @{ ArtifactRoot = (Join-Path $Root 'evidence'); TargetMsiPath = $msi; TargetVersion = '0.42.76-admin-smoke'; WixPath = $wix; ProductHelperPath = $helper; InstalledManifestPath = $manifest; ExpectedProductRoot = (Join-Path $Root 'product') }
        }
    }
    It 'writes a contained non-mutating explicit lifecycle plan' {
        $entry = Join-Path $PSScriptRoot '../tools/Invoke-PcvBurnBootstrapperLifecycle.ps1'
        $root = Join-Path $TestDrive 'burn'
        $msi = Join-Path $TestDrive 'target.msi'; Set-Content $msi 'msi'
        $wix = Join-Path $TestDrive 'wix.exe'; Set-Content $wix 'tool'
        $product = Join-Path $TestDrive 'Invoke-PcvDesktopNodeProduct.ps1'; Set-Content $product 'tool'
        Test-Path $entry | Should -BeTrue
        & $entry -ArtifactRoot $root -TargetMsiPath $msi -TargetVersion '0.42.76-admin-smoke' -WixPath $wix -ProductHelperPath $product -PlanOnly | Out-Null
        $plan = Get-Content -Raw (Join-Path $root 'plan.json') | ConvertFrom-Json
        $plan.host_mutation_performed | Should -BeFalse
        @($plan.actions | ForEach-Object id) | Should -Be @('build-target-bundle','install','repair','remove','restore-target-msi','repair-installed','verify-target-running')
        ($plan.actions.command -join ' ') | Should -Match '/quiet /norestart'
        ($plan.actions.command -join ' ') | Should -Not -Match 'New-SelfSignedCertificate'
        $scriptText = Get-Content -Raw $entry
        $scriptText | Should -Match '8F455BB4-640E-47A2-A982-338C7A6318B5'
        $scriptText | Should -Match 'hyperlinkLicense'
        $scriptText | Should -Match 'Compressed=.*yes.*Vital=.*yes.*Permanent=.*no'
        $scriptText | Should -Match 'WixToolset\.BootstrapperApplications\.wixext'
        $scriptText | Should -Match 'REBOOT=ReallySuppress.*MSIRESTARTMANAGERCONTROL=Disable.*/L\*v'
        $scriptText | Should -Match 'finally\s*\{[\s\S]*restore-target-msi'
        $scriptText | Should -Match "Status.*Running.*StartType.*Automatic"
        $scriptText | Should -Match 'install-state|repair-state|remove-absence'
    }

    It 'rejects an invalid installed prestate before starting a process' {
        $f = New-BurnFixture (Join-Path $TestDrive 'prestate') '0.42.75-admin-smoke'
        Mock Get-Service { [pscustomobject]@{ Status = 'Running' } }
        Mock Get-CimInstance { [pscustomobject]@{ State = 'Running'; StartMode = 'Auto' } }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        { & $script:BurnEntry @f -Execute } | Should -Throw '*PRESTATE*'
        Should -Invoke Start-Process -Times 0 -Exactly
    }

    It 'restores the target and records restoration when repair fails' {
        $f = New-BurnFixture (Join-Path $TestDrive 'repair-failure')
        Mock Get-Service { [pscustomobject]@{ Status = 'Running' } }
        Mock Get-CimInstance { [pscustomobject]@{ State = 'Running'; StartMode = 'Auto' } }
        Mock Start-Process {
            if (@($ArgumentList) -contains '/repair') { return [pscustomobject]@{ ExitCode = 23 } }
            [pscustomobject]@{ ExitCode = 0 }
        }
        { & $script:BurnEntry @f -Execute } | Should -Throw '*repair*23*'
        Should -Invoke Start-Process -ParameterFilter { $FilePath -eq 'msiexec.exe' } -Times 1 -Exactly
        Test-Path $script:burnMarker | Should -BeTrue
        $summary = Get-Content -Raw (Join-Path $f.ArtifactRoot 'summary.json') | ConvertFrom-Json
        $summary.restoration_attempted | Should -BeTrue
        $summary.restoration_status | Should -Be 'PASS'
        $summary.error | Should -Match 'repair'
    }

    It 'requires product root service and manifest absence after remove' {
        $f = New-BurnFixture (Join-Path $TestDrive 'remove-state')
        $global:pcvBurnRemoved = $false
        Mock Start-Process {
            if (@($ArgumentList) -contains '/uninstall') {
                $global:pcvBurnRemoved = $true
                Remove-Item -LiteralPath $f.InstalledManifestPath -Force
                Remove-Item -LiteralPath $f.ExpectedProductRoot -Recurse -Force
            }
            if ($FilePath -eq 'msiexec.exe') {
                $global:pcvBurnRemoved = $false
                New-Item -ItemType Directory -Force -Path $f.ExpectedProductRoot | Out-Null
                @{ version = $f.TargetVersion; paths = @{ product_root = $f.ExpectedProductRoot } } | ConvertTo-Json | Set-Content $f.InstalledManifestPath
            }
            [pscustomobject]@{ ExitCode = 0 }
        }
        Mock Get-Service { if ($global:pcvBurnRemoved) { return $null }; [pscustomobject]@{ Status = 'Running' } }
        Mock Get-CimInstance { [pscustomobject]@{ State = 'Running'; StartMode = 'Auto' } }
        { & $script:BurnEntry @f -Execute } | Should -Not -Throw
        $summary = Get-Content -Raw (Join-Path $f.ArtifactRoot 'summary.json') | ConvertFrom-Json
        $summary.states.'remove-absence'.Absent | Should -BeTrue
        $summary.host_mutation_performed | Should -BeTrue
        Remove-Variable -Name pcvBurnRemoved -Scope Global -ErrorAction SilentlyContinue
    }
}
