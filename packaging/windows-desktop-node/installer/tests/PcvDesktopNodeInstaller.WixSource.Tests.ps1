BeforeAll {
    $script:InstallerRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

Describe 'Desktop Node WiX source contract' {
    It 'defines a per-machine MSI product with a fixed UpgradeCode' {
        $product = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $product | Should -Match '<Package'
        $product | Should -Match 'Name="PureCVisor Desktop Node"'
        $product | Should -Match 'Manufacturer="PureCVisor"'
        $product | Should -Match 'Version="\$\(var\.MsiProductVersion\)"'
        $product | Should -Not -Match 'Version="\$\(var\.ProductVersion\)"'
        $product | Should -Match 'UpgradeCode="\{[0-9A-Fa-f-]{36}\}"'
        $product | Should -Match 'Scope="perMachine"'
        $product | Should -Match '<StandardDirectory Id="ProgramFiles64Folder">'
        $product | Should -Match '<Directory Id="PURECVISORFOLDER" Name="PureCVisor">'
        $product | Should -Match '<Directory Id="INSTALLFOLDER" Name="DesktopNode"'
    }

    It 'keeps MSI file ownership separate from service configuration actions' {
        $product = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $product | Should -Match 'ComponentGroupRef Id="DesktopNodePayloadComponents"'
        $product | Should -Match 'ComponentGroupRef Id="DesktopNodeProductWrapperComponents"'
        $product | Should -Match 'CustomActionRef Id="ConfigureInstalled"'
        $product | Should -Match 'CustomActionRef Id="RepairInstalled"'
        $product | Should -Match 'CustomActionRef Id="EventLogDefaultTransition"'
        $product | Should -Match 'CustomActionRef Id="EventLogDefaultTransitionRepair"'
        $product | Should -Match 'CustomActionRef Id="CredentialManagerDefaultTransition"'
        $product | Should -Not -Match 'CustomActionRef Id="CredentialManagerDefaultTransitionRepair"'
        $product | Should -Match 'CustomActionRef Id="RemoveInstalled"'
        $product | Should -Match 'CustomActionRef Id="DataRootRemove"'
    }

    It 'maps install repair uninstall and remove-data custom actions without raw token properties' {
        $actions = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'ProductActions.wxs')

        $actions | Should -Match 'Id="ConfigureInstalled"'
        $actions | Should -Match 'Id="RepairInstalled"'
        $actions | Should -Match 'Id="EventLogDefaultTransition"'
        $actions | Should -Match 'Id="EventLogDefaultTransitionRepair"'
        $actions | Should -Match 'Id="CredentialManagerDefaultTransition"'
        $actions | Should -Not -Match 'Id="CredentialManagerDefaultTransitionRepair"'
        $actions | Should -Match 'Id="RemoveInstalled"'
        $actions | Should -Match 'Id="DataRootRemove"'
        $actions | Should -Match 'REMOVE_DATA'
        $actions | Should -Match 'BATCH_EVIDENCE_ROOT'
        $actions | Should -Match 'ConfigureInstalledData'
        $actions | Should -Match 'RepairInstalledData'
        $actions | Should -Match 'EventLogDefaultTransitionData'
        $actions | Should -Match 'EventLogDefaultTransitionRepairData'
        $actions | Should -Match 'CredentialManagerDefaultTransitionData'
        $actions | Should -Not -Match 'CredentialManagerDefaultTransitionRepairData'
        $actions | Should -Match 'RemoveInstalledData'
        $actions | Should -Match 'DataRootRemoveData'
        $actions | Should -Match 'Id="REMOVE_DATA_SWITCH"'
        $actions | Should -Match 'Value="--remove-data"'
        $actions | Should -Match 'Id="BATCH_EVIDENCE_ROOT_SWITCH"'
        $actions | Should -Match 'Value="--batch-evidence-root &quot;\[BATCH_EVIDENCE_ROOT\]&quot;"'
        $actions | Should -Match 'ExeCommand="[^"]*\[REMOVE_DATA_SWITCH\]'
        $actions | Should -Match 'configure-installed.*\[BATCH_EVIDENCE_ROOT_SWITCH\]'
        $actions | Should -Match 'repair-installed.*\[BATCH_EVIDENCE_ROOT_SWITCH\]'
        $actions | Should -Not -Match 'ExeCommand="[^"]*\[REMOVE_DATA\]'
        $actions | Should -Match '--product-root &quot;\[INSTALLFOLDER\]\.&quot;'
        $actions | Should -Match '--data-root &quot;\[DESKTOP_NODE_DATA_ROOT\]&quot;'
        $actions | Should -Match '--service-exe &quot;\[INSTALLFOLDER\]DesktopNode\.Host\.exe&quot;'
        $actions | Should -Match 'DesktopNode\.Host\.exe&quot; service-action configure-installed'
        $actions | Should -Match 'DesktopNode\.Host\.exe&quot; service-action repair-installed'
        $actions | Should -Match 'DesktopNode\.Host\.exe&quot; service-action eventlog-default-transition'
        @($actions -split "`r?`n" | Where-Object {
                $_ -match 'eventlog-default-transition' -and $_ -match '--eventlog-default-transition-timeout-seconds 60'
            }).Count | Should -Be 2
        $actions | Should -Match 'DesktopNode\.Host\.exe&quot; service-action credential-manager-default-transition'
        $actions | Should -Match 'DesktopNode\.Host\.exe&quot; service-action remove-installed'
        $actions | Should -Match 'DesktopNode\.Host\.exe&quot; service-action data-root-remove'
        $actions | Should -Match 'Action="ConfigureInstalled" After="InstallFiles" Condition="NOT Installed"'
        $actions | Should -Match 'Action="EventLogDefaultTransition" After="ConfigureInstalled" Condition="NOT Installed"'
        $actions | Should -Match 'Action="EventLogDefaultTransitionRepair" After="CredentialManagerDefaultTransition" Condition="Installed AND NOT REMOVE~=&quot;ALL&quot;"'
        $actions | Should -Match 'Action="RepairInstalled" After="EventLogDefaultTransitionRepair" Condition="Installed AND NOT REMOVE~=&quot;ALL&quot;"'
        $actions | Should -Match 'Action="CredentialManagerDefaultTransition" After="EventLogDefaultTransition" Condition="NOT Installed"'
        $actions | Should -Not -Match 'Action="CredentialManagerDefaultTransitionRepair"'
        $actions | Should -Match 'Action="DataRootRemove" After="RemoveInstalled" Condition="REMOVE~=&quot;ALL&quot; AND REMOVE_DATA=&quot;1&quot;"'
        $actions | Should -Not -Match 'POWERSHELLEXE'
        $actions | Should -Not -Match 'powershell\.exe'
        $actions | Should -Not -Match 'WinSwPath'
        $actions | Should -Not -Match 'ApiToken='
        $actions | Should -Not -Match 'API_TOKEN'
    }

    It 'passes the installed payload root as SourceRoot for MSI product actions' {
        $actions = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'ProductActions.wxs')

        @($actions -split "`r?`n" | Where-Object {
                $_ -match 'ExeCommand=' -and $_ -match '--product-root\s+&quot;\[INSTALLFOLDER\]\.&quot;'
            }).Count | Should -Be 7
    }

    It 'runs deferred product actions from the installed payload directory' {
        $actions = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'ProductActions.wxs')

        foreach ($actionId in @('ConfigureInstalled', 'RepairInstalled', 'EventLogDefaultTransition', 'EventLogDefaultTransitionRepair', 'CredentialManagerDefaultTransition', 'RemoveInstalled', 'DataRootRemove')) {
            $actionBlock = [regex]::Match(
                $actions,
                "(?s)<CustomAction\s+Id=`"$actionId`".*?/>"
            ).Value

            $actionBlock | Should -Match 'Directory="INSTALLFOLDER"'
            $actionBlock | Should -Not -Match 'Property="[^"]+Data"'
        }
    }

    It 'calculates ProgramData paths without illegal property references' {
        $actions = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'ProductActions.wxs')

        $actions | Should -Match 'SetProperty\s+Id="DESKTOP_NODE_DATA_ROOT"'
        $actions | Should -Match 'Value="\[CommonAppDataFolder\]PureCVisor\\desktop-node"'
        $actions | Should -Not -Match '<Property\s+Id="DESKTOP_NODE_DATA_ROOT"\s+Value="\[CommonAppDataFolder\]'
    }

    It 'does not pass a quoted trailing-backslash INSTALLFOLDER as ProductRoot' {
        $actions = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'ProductActions.wxs')

        $actions | Should -Not -Match '--product-root\s+&quot;\[INSTALLFOLDER\]&quot;'
        $actions | Should -Match '--product-root\s+&quot;\[INSTALLFOLDER\]\.&quot;'
    }

    It 'installs only product-owned Desktop Node MSI payload assets' {
        $product = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $product | Should -Match 'Directory Id="DesktopNodeWebFolder"'
        $product | Should -Match 'Source="\$\(var\.PayloadRoot\)\\DesktopNode\.Host\.exe"'
        $product | Should -Match 'Source="\$\(var\.PayloadRoot\)\\pcvcli\.exe"'
        $product | Should -Match '<File Id="DesktopNodeCli"'
        $product | Should -Match '<File Id="DesktopNodeWebApp"'
        $product | Should -Match 'Source="\$\(var\.PayloadRoot\)\\Invoke-PcvDesktopNodeProduct\.ps1"'
        $product | Should -Match 'Source="\$\(var\.PayloadRoot\)\\PcvDesktopNodeProduct\.psm1"'
        $product | Should -Match 'Source="\$\(var\.PayloadRoot\)\\web\\index\.html"'
        $product | Should -Not -Match 'Directory Id="DesktopNodeApiFolder"'
        $product | Should -Not -Match 'Directory Id="DesktopNodeHyperVFolder"'
        $product | Should -Not -Match 'Directory Id="DesktopNodeServiceFolder"'
        $product | Should -Not -Match 'Source="\$\(var\.PayloadRoot\)\\api\\'
        $product | Should -Not -Match 'Source="\$\(var\.PayloadRoot\)\\hyperv\\'
        $product | Should -Not -Match 'Source="\$\(var\.PayloadRoot\)\\service\\'
        $product | Should -Not -Match 'DesktopNodeTui'
        $product | Should -Not -Match 'pcvtui\.exe'
    }

    It 'adds the installed Desktop Node folder to the machine PATH for CLI discovery' {
        $product = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'Product.wxs')

        $product | Should -Match '<Environment\s+Id="DesktopNodeMachinePath"'
        $product | Should -Match 'Name="PATH"'
        $product | Should -Match 'Value="\[INSTALLFOLDER\]"'
        $product | Should -Match 'Part="last"'
        $product | Should -Match 'Action="set"'
        $product | Should -Match 'System="yes"'
        $product | Should -Match 'Permanent="no"'
    }

    It 'includes all WiX source files in the project' {
        $project = Get-Content -Raw -LiteralPath (Join-Path $script:InstallerRoot 'PureCVisorDesktopNode.wixproj')

        $project | Should -Match '<Project'
        $project | Should -Match 'Product.wxs'
        $project | Should -Match 'ProductActions.wxs'
        $project | Should -Match 'WixToolset.Sdk'
    }
}
