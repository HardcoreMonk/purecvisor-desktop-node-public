Set-StrictMode -Version Latest

Describe 'PcvManualAdminPackagePairCampaign contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:EntryPoint = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvManualAdminPackagePairCampaign.ps1'
        $script:BaselineVersion = '0.42.75-admin-smoke'
        $script:TargetVersion = '0.42.76-admin-smoke'

        function Write-TestJson {
            param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)]$Value)
            New-Item -ItemType Directory -Path (Split-Path -Parent $Path) -Force | Out-Null
            $Value | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $Path -Encoding utf8
        }

        function New-TestPackage {
            param([Parameter(Mandatory)][string]$Root, [Parameter(Mandatory)][string]$Version)
            $payload = Join-Path $Root 'payload'
            New-Item -ItemType Directory -Path $payload -Force | Out-Null
            $msi = Join-Path $Root "PureCVisorDesktopNode-$Version-windows-x64.msi"
            Set-Content -LiteralPath $msi -Value "msi-$Version" -Encoding utf8
            $msiSha = (Get-FileHash -LiteralPath $msi -Algorithm SHA256).Hash.ToLowerInvariant()
            Set-Content -LiteralPath "$msi.sha256" -Value "$msiSha *$(Split-Path -Leaf $msi)" -Encoding utf8
            Write-TestJson -Path (Join-Path $Root "PureCVisorDesktopNode-$Version-windows-x64.publication.json") -Value ([ordered]@{
                product = [ordered]@{ version = $Version }
                artifact = [ordered]@{ msi_path = $msi; msi_sha256 = $msiSha; signing_mode = 'AllowUnsignedDev' }
                publication = [ordered]@{ public_trusted_signing = 'not-claimed'; external_stable_publication = 'not-claimed' }
            })
            Write-TestJson -Path (Join-Path $payload 'product-manifest.json') -Value ([ordered]@{
                schema_version = 1
                product = 'PureCVisor Desktop Node'
                version = $Version
            })
        }

        function New-TestInputs {
            param([Parameter(Mandatory)][string]$Root, [string]$InstalledVersion = $script:BaselineVersion)
            $baseline = Join-Path $Root 'baseline-package'
            $target = Join-Path $Root 'target-package'
            New-TestPackage -Root $baseline -Version $script:BaselineVersion
            New-TestPackage -Root $target -Version $script:TargetVersion
            $installedManifest = Join-Path $Root 'installed/product-manifest.json'
            Write-TestJson -Path $installedManifest -Value ([ordered]@{ schema_version = 1; version = $InstalledVersion })
            $baseVhd = Join-Path $Root 'base.vhdx'
            Set-Content -LiteralPath $baseVhd -Value 'fake-vhd' -Encoding utf8
            $baselineUpdatePackage = Join-Path $baseline "PureCVisorDesktopNode-$($script:BaselineVersion)-update.zip"
            $targetUpdatePackage = Join-Path $target "PureCVisorDesktopNode-$($script:TargetVersion)-update.zip"
            Set-Content -LiteralPath $baselineUpdatePackage -Value 'fake-baseline-update-package' -Encoding utf8
            Set-Content -LiteralPath $targetUpdatePackage -Value 'fake-target-update-package' -Encoding utf8
            $tools = @{}
            foreach ($name in @('wix.exe', 'makeappx.exe', 'signtool.exe')) {
                $path = Join-Path $Root $name
                Set-Content -LiteralPath $path -Value 'fake-tool' -Encoding utf8
                $tools[$name] = $path
            }
            $baselineCatalog = Join-Path $baseline 'update-catalog.json'
            $targetCatalog = Join-Path $target 'update-catalog.json'
            foreach ($catalogInput in @(
                    @{ path = $baselineCatalog; version = $script:BaselineVersion; package = $baselineUpdatePackage },
                    @{ path = $targetCatalog; version = $script:TargetVersion; package = $targetUpdatePackage })) {
                Write-TestJson -Path $catalogInput.path -Value ([ordered]@{
                    schema_version = 1; product = 'PureCVisor Desktop Node'
                    channels = @([ordered]@{
                        name = 'admin-smoke'; version = $catalogInput.version
                        channel = 'admin-smoke'
                        source_uri = ([Uri](Get-Item -LiteralPath $catalogInput.package).FullName).AbsoluteUri
                        expected_sha256 = (Get-FileHash -LiteralPath $catalogInput.package -Algorithm SHA256).Hash.ToLowerInvariant()
                        release_channel = 'admin-smoke'; signing_mode = 'AllowUnsignedDev'
                    })
                })
            }
            $pcvcli = Join-Path $Root 'pcvcli.exe'
            Set-Content -LiteralPath $pcvcli -Value 'fake-pcvcli' -Encoding utf8
            $templateLayout = Join-Path $Root 'msix-template-layout'
            New-Item -ItemType Directory -Path $templateLayout -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $templateLayout 'AppxManifest.xml.template') -Value '<Package />' -Encoding utf8
            [ordered]@{
                CampaignId = 'manual-admin-test-04275-04276'
                BaselineVersion = $script:BaselineVersion
                TargetVersion = $script:TargetVersion
                BaselinePackageRoot = $baseline
                TargetPackageRoot = $target
                InstalledManifestPath = $installedManifest
                ArtifactRoot = (Join-Path $Root 'campaign')
                BaseVhdPath = $baseVhd
                BaselineUpdatePackagePath = $baselineUpdatePackage
                TargetUpdatePackagePath = $targetUpdatePackage
                BaselineUpdateCatalogPath = $baselineCatalog
                TargetUpdateCatalogPath = $targetCatalog
                InstalledPcvCliPath = $pcvcli
                VMSwitchName = 'Test Switch'
                WixPath = $tools['wix.exe']
                MakeAppxPath = $tools['makeappx.exe']
                SignToolPath = $tools['signtool.exe']
                SigningCertificateThumbprint = '00112233445566778899AABBCCDDEEFF00112233'
                MsixTemplateLayoutRoot = $templateLayout
            }
        }
    }

    It 'requires exactly one explicit mode before creating an artifact root' {
        $inputs = New-TestInputs -Root (Join-Path $TestDrive 'mode')
        Test-Path -LiteralPath $script:EntryPoint | Should -BeTrue
        { & $script:EntryPoint @inputs } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_MODE_REQUIRED*'
        Test-Path -LiteralPath $inputs.ArtifactRoot | Should -BeFalse
        { & $script:EntryPoint @inputs -PlanOnly -Execute } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_MODE_CONFLICT*'
        Test-Path -LiteralPath $inputs.ArtifactRoot | Should -BeFalse
    }

    It 'writes a deterministic non-mutating plan with the exact six ordered buckets under ArtifactRoot' {
        $inputs = New-TestInputs -Root (Join-Path $TestDrive 'plan') -InstalledVersion $script:TargetVersion
        & $script:EntryPoint @inputs -PlanOnly | Out-Null
        $firstPlanText = Get-Content -Raw -LiteralPath (Join-Path $inputs.ArtifactRoot 'plan.json')
        & $script:EntryPoint @inputs -PlanOnly | Out-Null
        $secondPlanText = Get-Content -Raw -LiteralPath (Join-Path $inputs.ArtifactRoot 'plan.json')
        $firstPlanText | Should -BeExactly $secondPlanText

        $plan = $firstPlanText | ConvertFrom-Json
        $summary = Get-Content -Raw -LiteralPath (Join-Path $inputs.ArtifactRoot 'summary.json') | ConvertFrom-Json
        $plan.mode | Should -Be 'PlanOnly'
        $plan.host_mutation_performed | Should -BeFalse
        $plan.baseline_alignment.required | Should -BeTrue
        @($plan.baseline_alignment.actions | ForEach-Object id) | Should -Be @(
            'align-installed-target-to-baseline', 'verify-baseline-manifest-and-service',
            'create-or-validate-dedicated-host-reservation', 'run-readiness'
        )
        @($plan.buckets | ForEach-Object relative_summary_path) | Should -Be @(
            'manual-admin-rebaseline-readiness/summary.json',
            'lifecycle/product-update-rollback/summary.json',
            'clean-host-windows-update/summary.json',
            'burn-bootstrapper-lifecycle/summary.json',
            'msix-package-lifecycle-smoke/summary.json',
            'installed-runtime-ops-summary/summary.json'
        )
        foreach ($bucket in $plan.buckets) {
            [IO.Path]::GetFullPath([string]$bucket.summary_path).StartsWith(
                [IO.Path]::GetFullPath([string]$inputs.ArtifactRoot) + [IO.Path]::DirectorySeparatorChar,
                [StringComparison]::OrdinalIgnoreCase) | Should -BeTrue
            @($bucket.actions).Count | Should -BeGreaterThan 0
        }
        $plan.buckets[0].pass_contract | Should -Contain 'reservation_status=reserved-and-matched'
        $plan.buckets[0].pass_contract | Should -Contain 'actual_execution_eligible=true'
        $plan.buckets[0].actions[0] | Should -Match "-Version '0\.42\.75-admin-smoke'"
        $plan.buckets[1].actions[0] | Should -Match "-UpdateCatalogUri 'file:"
        $plan.buckets[1].actions[0] | Should -Match "-UpdateChannel 'admin-smoke'"
        $plan.buckets[5].actions[0] | Should -Match "pcvcli\.exe' --json ops summary"
        ($plan.buckets[2].actions -join ' ') | Should -Match 'RemoveVmOnSuccess'
        ($plan.buckets[2].actions -join ' ') | Should -Match 'RemoveVmOnFailure'
        ($plan.buckets[2].actions -join ' ') | Should -Match "UpdateChannel 'admin-smoke'"
        ($plan.buckets[2].actions -join ' ') | Should -Match "TargetSigningMode 'AllowUnsignedDev'"
        ($plan.buckets[3].actions -join ' ') | Should -Match 'Invoke-PcvBurnBootstrapperLifecycle\.ps1'
        ($plan.buckets[4].actions -join ' ') | Should -Match 'Invoke-PcvMsixPackageLifecycleSmoke\.ps1'
        ($plan.buckets[3..4].actions -join ' ') | Should -Not -Match '<burn-lifecycle-runner>|<msix-lifecycle-runner>'
        $summary.closed_descriptor_emitted | Should -BeFalse
        $summary.public_trusted_signing | Should -Be 'not-claimed'
        $summary.external_stable_publication | Should -Be 'out-of-scope'
        Test-Path -LiteralPath (Join-Path $inputs.ArtifactRoot 'manual-admin-campaign-descriptor') | Should -BeFalse
    }

    It 'uses validated catalog JSON file URIs for PlanOnly lifecycle updates' {
        $inputs = New-TestInputs -Root (Join-Path $TestDrive 'catalog-action-uri') -InstalledVersion $script:TargetVersion
        & $script:EntryPoint @inputs -PlanOnly | Out-Null
        $plan = Get-Content -Raw -LiteralPath (Join-Path $inputs.ArtifactRoot 'plan.json') | ConvertFrom-Json
        $baselineCatalogUri = ([uri](Get-Item -LiteralPath $inputs.BaselineUpdateCatalogPath).FullName).AbsoluteUri
        $targetCatalogUri = ([uri](Get-Item -LiteralPath $inputs.TargetUpdateCatalogPath).FullName).AbsoluteUri
        $baselineZipUri = ([uri](Get-Item -LiteralPath $inputs.BaselineUpdatePackagePath).FullName).AbsoluteUri
        $targetZipUri = ([uri](Get-Item -LiteralPath $inputs.TargetUpdatePackagePath).FullName).AbsoluteUri

        $plan.baseline_alignment.actions[0].command | Should -Match ([regex]::Escape($baselineCatalogUri))
        $targetUpdateActions = @($plan.buckets[1].actions | Where-Object { $_ -match '-Action Update' })
        $targetUpdateActions.Count | Should -Be 2
        foreach ($action in $targetUpdateActions) {
            $action | Should -Match ([regex]::Escape("-UpdateCatalogUri '$targetCatalogUri'"))
        }
        ($plan.baseline_alignment.actions.command -join ' ') | Should -Not -Match ([regex]::Escape($baselineZipUri))
        ($plan.buckets[1].actions -join ' ') | Should -Not -Match ([regex]::Escape($targetZipUri))
    }

    It 'serializes PlanOnly summary ok as the JSON boolean true' {
        $inputs = New-TestInputs -Root (Join-Path $TestDrive 'summary-boolean')
        & $script:EntryPoint @inputs -PlanOnly | Out-Null
        $summaryText = Get-Content -Raw -LiteralPath (Join-Path $inputs.ArtifactRoot 'summary.json')
        $document = [Text.Json.JsonDocument]::Parse($summaryText)
        try {
            $document.RootElement.GetProperty('ok').ValueKind | Should -Be ([Text.Json.JsonValueKind]::True)
            (ConvertFrom-Json $summaryText).ok.GetType() | Should -Be ([bool])
        } finally {
            $document.Dispose()
        }
    }

    It 'accepts the canonical clean-host final service state contract' {
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint
        $tokens = $null; $errors = $null
        $ast = [Management.Automation.Language.Parser]::ParseInput($scriptText, [ref]$tokens, [ref]$errors)
        $bucketPassFunction = $ast.Find({
            param($node)
            $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Test-PcvExactBucketPass'
        }, $true)
        $bucketPassFunction | Should -Not -BeNullOrEmpty
        . ([scriptblock]::Create($bucketPassFunction.Extent.Text))

        $BaselineVersion = '0.42.75-admin-smoke'
        $TargetVersion = '0.42.77-admin-smoke'
        $cleanHostSummary = [pscustomobject]@{
            ok = $true
            internal_clean_host_install_update_rollback_smoke = 'pass'
            blocker = 'none'
            install_exit_code = 0
            update_exit_code = 0
            rollback_exit_code = 0
            baseline_manifest_version = $BaselineVersion
            updated_manifest_version = $TargetVersion
            final_manifest_version = $BaselineVersion
            final_service = [pscustomobject]@{ state = 'Running' }
            final_web_status_code = 200
        }

        Test-PcvExactBucketPass -Id 'clean-host-windows-update' -Summary $cleanHostSummary | Should -BeTrue
    }

    It 'rejects matching versions, mixed payload versions, and missing required tools before output' {
        $same = New-TestInputs -Root (Join-Path $TestDrive 'same')
        $same.TargetVersion = $same.BaselineVersion
        { & $script:EntryPoint @same -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_VERSION_INVALID*'
        Test-Path -LiteralPath $same.ArtifactRoot | Should -BeFalse

        $mixed = New-TestInputs -Root (Join-Path $TestDrive 'mixed')
        Write-TestJson -Path (Join-Path $mixed.TargetPackageRoot 'payload/product-manifest.json') -Value @{ version = '0.42.99-admin-smoke' }
        { & $script:EntryPoint @mixed -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_PACKAGE_VERSION_MISMATCH*'
        Test-Path -LiteralPath $mixed.ArtifactRoot | Should -BeFalse

        $missing = New-TestInputs -Root (Join-Path $TestDrive 'missing')
        Remove-Item -LiteralPath $missing.MakeAppxPath
        { & $script:EntryPoint @missing -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_INPUT_MISSING*makeappx*'
        Test-Path -LiteralPath $missing.ArtifactRoot | Should -BeFalse
    }

    It 'rejects a reverse package pair before creating campaign output' {
        $reverse = New-TestInputs -Root (Join-Path $TestDrive 'reverse-order') -InstalledVersion $script:TargetVersion
        $reverse.BaselineVersion = $script:TargetVersion
        $reverse.TargetVersion = $script:BaselineVersion
        $reverse.BaselinePackageRoot, $reverse.TargetPackageRoot = $reverse.TargetPackageRoot, $reverse.BaselinePackageRoot
        $reverse.BaselineUpdatePackagePath, $reverse.TargetUpdatePackagePath = $reverse.TargetUpdatePackagePath, $reverse.BaselineUpdatePackagePath
        $reverse.BaselineUpdateCatalogPath, $reverse.TargetUpdateCatalogPath = $reverse.TargetUpdateCatalogPath, $reverse.BaselineUpdateCatalogPath

        { & $script:EntryPoint @reverse -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_VERSION_INVALID*'
        Test-Path -LiteralPath $reverse.ArtifactRoot | Should -BeFalse
    }

    It 'removes a generated descriptor and persists fail-closed summary state when reservation consumption fails' {
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint
        $tokens = $null; $errors = $null
        $ast = [Management.Automation.Language.Parser]::ParseInput($scriptText, [ref]$tokens, [ref]$errors)
        $closureFunction = $ast.Find({
            param($node)
            $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Complete-PcvCampaignClosure'
        }, $true)
        $closureFunction | Should -Not -BeNullOrEmpty
        foreach ($dependencyName in @('Resolve-PcvPath', 'Test-PcvChildPath', 'Write-PcvJson')) {
            $dependency = $ast.Find({
                param($node)
                $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $dependencyName
            }, $true)
            $dependency | Should -Not -BeNullOrEmpty
            . ([scriptblock]::Create($dependency.Extent.Text))
        }
        . ([scriptblock]::Create($closureFunction.Extent.Text))

        $root = Join-Path $TestDrive 'reservation-consume-failure'
        $descriptorRoot = Join-Path $root 'manual-admin-campaign-descriptor'
        $summaryPath = Join-Path $root 'summary.json'
        New-Item -ItemType Directory -Force -Path $descriptorRoot | Out-Null
        Set-Content -LiteralPath (Join-Path $descriptorRoot 'closed.json') -Value 'closed descriptor'
        $summary = [ordered]@{ ok=$true; descriptor_eligible=$true; closed_descriptor_emitted=$true; descriptor_path=(Join-Path $descriptorRoot 'closed.json') }
        function Set-PcvManualAdminBaselineReservationState { throw 'simulated reservation consume failure' }

        { Complete-PcvCampaignClosure -Summary $summary -SummaryPath $summaryPath -CampaignRoot $root -DescriptorRoot $descriptorRoot `
            -DescriptorPath (Join-Path $descriptorRoot 'closed.json') -ReservationPath (Join-Path $root 'reservation.json') `
            -DescriptorValidated } | Should -Throw '*simulated reservation consume failure*'
        Test-Path -LiteralPath $descriptorRoot | Should -BeFalse
        $persisted = Get-Content -Raw -LiteralPath $summaryPath | ConvertFrom-Json
        $persisted.ok | Should -BeFalse
        $persisted.descriptor_eligible | Should -BeFalse
        $persisted.closed_descriptor_emitted | Should -BeFalse
        $persisted.descriptor_path | Should -BeNullOrEmpty
        $persisted.closure_error | Should -Match 'simulated reservation consume failure'
    }

    It 'rejects an installed version outside the package pair but keeps PlanOnly certificate-store hermetic' {
        $installed = New-TestInputs -Root (Join-Path $TestDrive 'installed') -InstalledVersion '0.42.74-admin-smoke'
        { & $script:EntryPoint @installed -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_INSTALLED_VERSION_INVALID*'
        Test-Path -LiteralPath $installed.ArtifactRoot | Should -BeFalse

        $certificate = New-TestInputs -Root (Join-Path $TestDrive 'certificate')
        { & $script:EntryPoint @certificate -PlanOnly } | Should -Not -Throw
    }

    It 'never serializes guest credentials or certificate-generation actions' {
        $inputs = New-TestInputs -Root (Join-Path $TestDrive 'secret')
        $secret = 'never-serialize-this-password-7c6a'
        $credential = [pscredential]::new('Administrator', (ConvertTo-SecureString $secret -AsPlainText -Force))
        & $script:EntryPoint @inputs -GuestCredential $credential -PlanOnly | Out-Null
        $serialized = (Get-Content -Raw -LiteralPath (Join-Path $inputs.ArtifactRoot 'plan.json')) +
            (Get-Content -Raw -LiteralPath (Join-Path $inputs.ArtifactRoot 'summary.json'))
        $serialized | Should -Not -Match ([regex]::Escape($secret))
        $serialized | Should -Not -Match 'New-SelfSignedCertificate|New-PcvInternalCodeSigningTrust|certificate-generation'
        $serialized | Should -Match '<guest-credential-at-execution-boundary>'
    }

    It 'rejects MSI hash and catalog mismatches before output' {
        $hash = New-TestInputs -Root (Join-Path $TestDrive 'hash')
        Set-Content -LiteralPath (Join-Path $hash.TargetPackageRoot "PureCVisorDesktopNode-$($script:TargetVersion)-windows-x64.msi") -Value 'tampered'
        { & $script:EntryPoint @hash -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_MSI_HASH_MISMATCH*'
        Test-Path -LiteralPath $hash.ArtifactRoot | Should -BeFalse

        $catalog = New-TestInputs -Root (Join-Path $TestDrive 'catalog')
        $value = Get-Content -Raw -LiteralPath $catalog.TargetUpdateCatalogPath | ConvertFrom-Json
        $value.channels[0].version = $script:BaselineVersion
        Write-TestJson -Path $catalog.TargetUpdateCatalogPath -Value $value
        { & $script:EntryPoint @catalog -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_CATALOG_INVALID*'
        Test-Path -LiteralPath $catalog.ArtifactRoot | Should -BeFalse
    }

    It 'accepts only the nested publication and source_uri catalog schemas' {
        $real = New-TestInputs -Root (Join-Path $TestDrive 'real-schema')
        { & $script:EntryPoint @real -PlanOnly } | Should -Not -Throw

        $legacy = New-TestInputs -Root (Join-Path $TestDrive 'legacy-schema')
        $catalog = Get-Content -Raw $legacy.TargetUpdateCatalogPath | ConvertFrom-Json
        $entry = $catalog.channels[0]
        $entry | Add-Member package_uri $entry.source_uri
        $entry | Add-Member sha256 $entry.expected_sha256
        $entry.PSObject.Properties.Remove('source_uri')
        $entry.PSObject.Properties.Remove('expected_sha256')
        Write-TestJson $legacy.TargetUpdateCatalogPath $catalog
        { & $script:EntryPoint @legacy -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_CATALOG_INVALID*'
        Test-Path $legacy.ArtifactRoot | Should -BeFalse

        $legacyPublication = New-TestInputs -Root (Join-Path $TestDrive 'legacy-publication-schema')
        $publicationPath = Join-Path $legacyPublication.TargetPackageRoot "PureCVisorDesktopNode-$($script:TargetVersion)-windows-x64.publication.json"
        $publication = Get-Content -Raw $publicationPath | ConvertFrom-Json
        $publication | Add-Member version $publication.product.version
        $publication | Add-Member msi_sha256 $publication.artifact.msi_sha256
        $publication | Add-Member signing_mode $publication.artifact.signing_mode
        $publication.PSObject.Properties.Remove('product')
        $publication.PSObject.Properties.Remove('artifact')
        Write-TestJson $publicationPath $publication
        { & $script:EntryPoint @legacyPublication -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_PUBLICATION_INVALID*'
        Test-Path $legacyPublication.ArtifactRoot | Should -BeFalse

        $wrongPublication = New-TestInputs -Root (Join-Path $TestDrive 'wrong-publication-content')
        $wrongPublicationPath = Join-Path $wrongPublication.TargetPackageRoot "PureCVisorDesktopNode-$($script:TargetVersion)-windows-x64.publication.json"
        $wrong = Get-Content -Raw $wrongPublicationPath | ConvertFrom-Json
        $wrong.artifact.msi_sha256 = ('0' * 64)
        Write-TestJson $wrongPublicationPath $wrong
        { & $script:EntryPoint @wrongPublication -PlanOnly } | Should -Throw '*PCV_MANUAL_ADMIN_CAMPAIGN_PUBLICATION_INVALID*'
        Test-Path $wrongPublication.ArtifactRoot | Should -BeFalse
    }

    It 'removes arbitrary execution seams and uses distinct descriptor lifecycle summaries' {
        $scriptText = Get-Content -Raw -LiteralPath $script:EntryPoint
        $scriptText | Should -Not -Match 'BucketExecutor|RuntimeOpsRunnerPath|BurnLifecycleRunnerPath|MsixLifecycleRunnerPath|SimulatedBucketResults'
        $scriptText | Should -Match '-ProductUpdateSummaryPath\s+\$productUpdateSummaryPath'
        $scriptText | Should -Match '-ProductRollbackSummaryPath\s+\$productRollbackSummaryPath'
        $scriptText | Should -Match 'missing_count.*0'
        $scriptText | Should -Match 'runner_count.*6'
        $scriptText | Should -Match "'lifecycle-product-update-rollback'\s*\{\s*\`$mutationStarted\s*=\s*\`$true"
    }
}
