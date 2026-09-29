Set-StrictMode -Version Latest

Describe 'PcvMsixPackageLifecycleSmoke PlanOnly contract' {
    BeforeAll {
        $script:MsixEntry = Join-Path $PSScriptRoot '../tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1'
        $script:Publisher = 'CN=PureCVisor Desktop Node Internal Code Signing'
        function New-MsixFixture([string]$Root, [switch]$InvalidTemplate) {
            $layout = Join-Path $Root 'layout'; $baseline = Join-Path $Root 'baseline'; $target = Join-Path $Root 'target'
            New-Item -ItemType Directory -Force -Path $layout,$baseline,$target | Out-Null
            $template = if ($InvalidTemplate) { '<Package><Identity Name="Wrong" /></Package>' } else { @'
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap10="http://schemas.microsoft.com/appx/manifest/uap/windows10/10" xmlns:desktop6="http://schemas.microsoft.com/appx/manifest/desktop/windows10/6" xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities">
  <Identity Name="{{PackageName}}" Publisher="{{Publisher}}" Version="{{Version}}" ProcessorArchitecture="x64" />
  <Dependencies><TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.19041.0" MaxVersionTested="10.0.26100.0" /></Dependencies>
  <Applications><Application Id="DesktopNode" Executable="DesktopNode.Host.exe" EntryPoint="Windows.FullTrustApplication" uap10:RuntimeBehavior="packagedClassicApp" uap10:TrustLevel="mediumIL"><Extensions><desktop6:Extension Category="windows.service" Executable="DesktopNode.Host.exe" EntryPoint="Windows.FullTrustApplication"><desktop6:Service Name="{{ServiceName}}" StartupType="manual" StartAccount="localSystem" Arguments="listen --prefix http://127.0.0.1:7789/ --product-root . --data-root %ProgramData%\PureCVisor\desktop-node-msix-smoke --token-required false --firewall-enabled false" /></desktop6:Extension></Extensions></Application></Applications>
  <Capabilities><rescap:Capability Name="runFullTrust" /><rescap:Capability Name="packagedServices" /><rescap:Capability Name="localSystemServices" /></Capabilities>
</Package>
'@ }
            Set-Content (Join-Path $layout 'AppxManifest.xml.template') $template
            $make = Join-Path $Root 'makeappx.exe'; $sign = Join-Path $Root 'signtool.exe'; Set-Content $make 'tool'; Set-Content $sign 'tool'
            $manifest = Join-Path $Root 'product-manifest.json'; @{ version = '0.42.76-admin-smoke' } | ConvertTo-Json | Set-Content $manifest
            @{ ArtifactRoot=(Join-Path $Root 'evidence');TemplateLayoutRoot=$layout;BaselinePayloadRoot=$baseline;TargetPayloadRoot=$target;BaselineVersion='0.42.75-admin-smoke';TargetVersion='0.42.76-admin-smoke';MakeAppxPath=$make;SignToolPath=$sign;SigningCertificateThumbprint='00112233445566778899AABBCCDDEEFF00112233';InstalledManifestPath=$manifest }
        }
        function New-TestCertificate {
            [pscustomobject]@{ Thumbprint='00112233445566778899AABBCCDDEEFF00112233';HasPrivateKey=$true;NotBefore=(Get-Date).AddDays(-1);NotAfter=(Get-Date).AddDays(30);EnhancedKeyUsageList=@([pscustomobject]@{ObjectId='1.3.6.1.5.5.7.3.3'});Subject=$script:Publisher;PSParentPath='Microsoft.PowerShell.Security\Certificate::CurrentUser\My' }
        }
        function Set-MsixManifestVariant([string]$Path, [string]$Variant) {
            [xml]$xml = Get-Content -Raw -LiteralPath $Path
            $packageNamespace = 'http://schemas.microsoft.com/appx/manifest/foundation/windows10'
            switch ($Variant) {
                'extra-capability' {
                    $capabilities = $xml.SelectSingleNode("//*[local-name()='Capabilities']")
                    $extra = $xml.CreateElement('rescap', 'Capability', 'http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities')
                    $extra.SetAttribute('Name', 'internetClient')
                    [void]$capabilities.AppendChild($extra)
                }
                'duplicate-application' {
                    $application = $xml.SelectSingleNode("//*[local-name()='Application']")
                    [void]$application.ParentNode.AppendChild($application.CloneNode($true))
                }
                'duplicate-extension' {
                    $extension = $xml.SelectSingleNode("//*[local-name()='Extension' and @Category='windows.service']")
                    [void]$extension.ParentNode.AppendChild($extension.CloneNode($true))
                }
                'duplicate-service' {
                    $service = $xml.SelectSingleNode("//*[local-name()='Service']")
                    [void]$service.ParentNode.AppendChild($service.CloneNode($true))
                }
            }
            $xml.Save($Path)
        }
    }
    It 'writes a contained non-mutating explicit package lifecycle plan' {
        $entry = Join-Path $PSScriptRoot '../tools/Invoke-PcvMsixPackageLifecycleSmoke.ps1'
        $root = Join-Path $TestDrive 'msix'
        $layout = Join-Path $TestDrive 'layout'; New-Item -ItemType Directory $layout | Out-Null
        Set-Content (Join-Path $layout 'AppxManifest.xml.template') '<Package />'
        $baseline = Join-Path $TestDrive 'baseline'; $target = Join-Path $TestDrive 'target'
        New-Item -ItemType Directory $baseline,$target | Out-Null
        $makeappx = Join-Path $TestDrive 'makeappx.exe'; Set-Content $makeappx 'tool'
        $signtool = Join-Path $TestDrive 'signtool.exe'; Set-Content $signtool 'tool'
        Test-Path $entry | Should -BeTrue
        & $entry -ArtifactRoot $root -TemplateLayoutRoot $layout -BaselinePayloadRoot $baseline -TargetPayloadRoot $target `
            -BaselineVersion '0.42.75-admin-smoke' -TargetVersion '0.42.76-admin-smoke' -MakeAppxPath $makeappx `
            -SignToolPath $signtool -SigningCertificateThumbprint '00112233445566778899AABBCCDDEEFF00112233' -PlanOnly | Out-Null
        $plan = Get-Content -Raw (Join-Path $root 'plan.json') | ConvertFrom-Json
        $plan.host_mutation_performed | Should -BeFalse
        @($plan.actions | ForEach-Object id) | Should -Be @('build-baseline','build-target','validate-publisher','sign-verify-baseline','sign-verify-target','install-baseline','update-target','remove-package','verify-final-state')
        ($plan.actions.command -join ' ') | Should -Match 'Add-AppxPackage.*-ForceApplicationShutdown'
        ($plan.actions.command -join ' ') | Should -Match 'Remove-AppxPackage'
        ($plan.actions.command -join ' ') | Should -Not -Match 'New-SelfSignedCertificate'
        $scriptText = Get-Content -Raw $entry
        $scriptText | Should -Not -Match '-Wait-PassThru-NoNewWindow|-ForceUpdateFromAnyVersion-ForceApplicationShutdown'
        $scriptText | Should -Match 'preexisting-smoke-identity'
        $scriptText | Should -Match 'finally\s*\{[\s\S]*Remove-AppxPackage'
        $scriptText | Should -Match 'ProcessorArchitecture.*x64'
        $scriptText | Should -Match 'runFullTrust.*packagedServices.*localSystemServices'
        $scriptText | Should -Match 'StartType.*Manual.*Account.*LocalSystem'
        $scriptText | Should -Match "Status.*Running.*StartType.*Automatic"
        $scriptText | Should -Match 'target_manifest_unchanged'
    }

    It 'rejects equal or reverse versions before starting tools' {
        $f = New-MsixFixture (Join-Path $TestDrive 'version-order')
        $f.TargetVersion = $f.BaselineVersion
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        { & $script:MsixEntry @f -PlanOnly } | Should -Throw '*VERSION_ORDER*'
        Should -Invoke Start-Process -Times 0 -Exactly
    }

    It 'rejects an invalid manifest template before starting tools' {
        $f = New-MsixFixture (Join-Path $TestDrive 'bad-template') -InvalidTemplate
        $cert = New-TestCertificate
        Mock Get-ChildItem { $cert }
        Mock Get-AppxPackage { @() }
        Mock Get-Service { $null }
        Mock Get-CimInstance { [pscustomobject]@{ State='Running'; StartMode='Auto' } }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        { & $script:MsixEntry @f -Execute } | Should -Throw '*TEMPLATE_CONTRACT*'
        Should -Invoke Start-Process -Times 0 -Exactly
    }

    It 'rejects extra or duplicate privileged manifest nodes before starting tools' -TestCases @(
        @{ Variant = 'extra-capability' },
        @{ Variant = 'duplicate-application' },
        @{ Variant = 'duplicate-extension' },
        @{ Variant = 'duplicate-service' }
    ) {
        param($Variant)
        $f = New-MsixFixture (Join-Path $TestDrive "manifest-$Variant")
        Set-MsixManifestVariant (Join-Path $f.TemplateLayoutRoot 'AppxManifest.xml.template') $Variant
        $cert = New-TestCertificate
        Mock Get-ChildItem { $cert }
        Mock Get-AppxPackage { @() }
        Mock Get-Service { $null }
        Mock Get-CimInstance { [pscustomobject]@{ State='Running'; StartMode='Auto' } }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        Mock Add-AppxPackage { throw 'unexpected host mutation attempt' }

        { & $script:MsixEntry @f -Execute } | Should -Throw '*TEMPLATE_CONTRACT*'
        Should -Invoke Start-Process -Times 0 -Exactly
        Should -Invoke Add-AppxPackage -Times 0 -Exactly
    }

    It 'cleans up and records cleanup when target update fails' {
        $f = New-MsixFixture (Join-Path $TestDrive 'update-failure')
        $cert = New-TestCertificate; $global:pcvMsixInstalled = $null; $global:pcvMsixAddCalls = 0
        Mock Get-ChildItem { $cert }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        Mock Add-AppxPackage { $global:pcvMsixAddCalls++; if($global:pcvMsixAddCalls -eq 2){throw 'simulated update failure'}; $global:pcvMsixInstalled='0.42.75.0' }
        Mock Get-AppxPackage { if($global:pcvMsixInstalled){[pscustomobject]@{Version=$global:pcvMsixInstalled;PackageFullName='smoke_1'}}else{@()} }
        Mock Remove-AppxPackage { $global:pcvMsixInstalled=$null }
        Mock Get-Service { $null }
        Mock Get-CimInstance { if($Filter -like '*MsixSmoke*'){[pscustomobject]@{StartMode='Manual';StartName='LocalSystem'}}else{[pscustomobject]@{State='Running';StartMode='Auto'}} }
        { & $script:MsixEntry @f -Execute } | Should -Throw '*simulated update failure*'
        Should -Invoke Remove-AppxPackage -Times 1 -Exactly
        $summary = Get-Content -Raw (Join-Path $f.ArtifactRoot 'summary.json') | ConvertFrom-Json
        $summary.cleanup_attempted | Should -BeTrue
        $summary.cleanup_status | Should -Be 'PASS'
        $summary.error | Should -Match 'simulated update failure'
        Remove-Variable -Name pcvMsixInstalled,pcvMsixAddCalls -Scope Global -ErrorAction SilentlyContinue
    }

    It 'cleans up a partially registered baseline package when Add-AppxPackage throws' {
        $f = New-MsixFixture (Join-Path $TestDrive 'partial-baseline-install')
        $cert = New-TestCertificate; $global:pcvMsixInstalled = $null
        Mock Get-ChildItem { $cert }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        Mock Add-AppxPackage { $global:pcvMsixInstalled = '0.42.75.0'; throw 'simulated partial baseline install failure' }
        Mock Get-AppxPackage { if ($global:pcvMsixInstalled) { [pscustomobject]@{ Version=$global:pcvMsixInstalled; PackageFullName='smoke_1' } } else { @() } }
        Mock Remove-AppxPackage { $global:pcvMsixInstalled = $null }
        Mock Get-Service { $null }
        Mock Get-CimInstance { if ($Filter -like '*MsixSmoke*') { [pscustomobject]@{ StartMode='Manual'; StartName='LocalSystem' } } else { [pscustomobject]@{ State='Running'; StartMode='Auto' } } }

        { & $script:MsixEntry @f -Execute } | Should -Throw '*simulated partial baseline install failure*'
        Should -Invoke Remove-AppxPackage -Times 1 -Exactly
        $summary = Get-Content -Raw (Join-Path $f.ArtifactRoot 'summary.json') | ConvertFrom-Json
        $summary.cleanup_attempted | Should -BeTrue
        $summary.cleanup_status | Should -Be 'PASS'
        $summary.final_package_absent | Should -BeTrue
        Remove-Variable -Name pcvMsixInstalled -Scope Global -ErrorAction SilentlyContinue
    }

    It 'persists a failed summary when the production MSI service disappears during cleanup' {
        $f = New-MsixFixture (Join-Path $TestDrive 'missing-final-msi-service')
        $cert = New-TestCertificate; $global:pcvMsixInstalled = $null; $global:pcvMsiStateCalls = 0
        Mock Get-ChildItem { $cert }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        Mock Add-AppxPackage { if ($Path -like '*baseline*') { $global:pcvMsixInstalled='0.42.75.0' } else { $global:pcvMsixInstalled='0.42.76.0' } }
        Mock Get-AppxPackage { if ($global:pcvMsixInstalled) { [pscustomobject]@{ Version=$global:pcvMsixInstalled; PackageFullName='smoke_1' } } else { @() } }
        Mock Remove-AppxPackage { $global:pcvMsixInstalled = $null }
        Mock Get-Service { $null }
        Mock Get-CimInstance {
            if ($Filter -like '*MsixSmoke*') { return [pscustomobject]@{ StartMode='Manual'; StartName='LocalSystem' } }
            $global:pcvMsiStateCalls++
            if ($global:pcvMsiStateCalls -eq 1) { return [pscustomobject]@{ State='Running'; StartMode='Auto' } }
            return $null
        }

        { & $script:MsixEntry @f -Execute } | Should -Throw
        $summaryPath = Join-Path $f.ArtifactRoot 'summary.json'
        Test-Path -LiteralPath $summaryPath | Should -BeTrue
        $summary = Get-Content -Raw $summaryPath | ConvertFrom-Json
        $summary.status | Should -Be 'FAIL'
        $summary.final_msi_service_status | Should -BeNullOrEmpty
        $summary.error | Should -Match 'final state invalid'
        Remove-Variable -Name pcvMsixInstalled,pcvMsiStateCalls -Scope Global -ErrorAction SilentlyContinue
    }

    It 'checks both package versions and smoke service states on success' {
        $f = New-MsixFixture (Join-Path $TestDrive 'success')
        $cert = New-TestCertificate; $global:pcvMsixInstalled = $null
        Mock Get-ChildItem { $cert }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        Mock Add-AppxPackage { if($Path -like '*baseline*'){$global:pcvMsixInstalled='0.42.75.0'}else{$global:pcvMsixInstalled='0.42.76.0'} }
        Mock Get-AppxPackage { if($global:pcvMsixInstalled){[pscustomobject]@{Version=$global:pcvMsixInstalled;PackageFullName='smoke_1'}}else{@()} }
        Mock Remove-AppxPackage { $global:pcvMsixInstalled=$null }
        Mock Get-Service { $null }
        Mock Get-CimInstance { if($Filter -like '*MsixSmoke*'){[pscustomobject]@{StartMode='Manual';StartName='LocalSystem'}}else{[pscustomobject]@{State='Running';StartMode='Auto'}} }
        { & $script:MsixEntry @f -Execute } | Should -Not -Throw
        Should -Invoke Get-CimInstance -ParameterFilter { $Filter -like '*MsixSmoke*' } -Times 2 -Exactly
        $summary = Get-Content -Raw (Join-Path $f.ArtifactRoot 'summary.json') | ConvertFrom-Json
        $summary.status | Should -Be 'PASS'
        $summary.target_manifest_unchanged | Should -BeTrue
        $summary.host_mutation_performed | Should -BeTrue
        Remove-Variable -Name pcvMsixInstalled -Scope Global -ErrorAction SilentlyContinue
    }

    It 'records a terminating Appx provider failure during the final absence probe' {
        $f = New-MsixFixture (Join-Path $TestDrive 'final-provider-failure')
        $cert = New-TestCertificate; $global:pcvMsixInstalled = $null; $global:pcvAppxProbeCalls = 0
        Mock Get-ChildItem { $cert }
        Mock Start-Process { [pscustomobject]@{ ExitCode = 0 } }
        Mock Add-AppxPackage { if($Path -like '*baseline*'){$global:pcvMsixInstalled='0.42.75.0'}else{$global:pcvMsixInstalled='0.42.76.0'} }
        Mock Get-AppxPackage {
            $global:pcvAppxProbeCalls++
            if ($global:pcvAppxProbeCalls -eq 5) { throw 'simulated Appx provider unavailable' }
            if($global:pcvMsixInstalled){[pscustomobject]@{Version=$global:pcvMsixInstalled;PackageFullName='smoke_1'}}else{@()}
        }
        Mock Remove-AppxPackage { $global:pcvMsixInstalled=$null }
        Mock Get-Service { $null }
        Mock Get-CimInstance { if($Filter -like '*MsixSmoke*'){[pscustomobject]@{StartMode='Manual';StartName='LocalSystem'}}else{[pscustomobject]@{State='Running';StartMode='Auto'}} }

        { & $script:MsixEntry @f -Execute } | Should -Throw '*simulated Appx provider unavailable*'
        $summary = Get-Content -Raw (Join-Path $f.ArtifactRoot 'summary.json') | ConvertFrom-Json
        $summary.status | Should -Be 'FAIL'
        $summary.final_package_absent | Should -BeFalse
        $summary.final_absence_probe_status | Should -Be 'FAIL'
        $summary.final_absence_probe_error | Should -Match 'simulated Appx provider unavailable'
        Remove-Variable -Name pcvMsixInstalled,pcvAppxProbeCalls -Scope Global -ErrorAction SilentlyContinue
    }
}
