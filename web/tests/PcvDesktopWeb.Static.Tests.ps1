Describe 'PcvDesktopWeb static console assets' {
    BeforeAll {
        $script:WebRoot = Split-Path -Parent $PSScriptRoot
        $script:RepoRoot = Split-Path -Parent $script:WebRoot
        $script:IndexPath = Join-Path $script:WebRoot 'index.html'
        $script:StylesPath = Join-Path $script:WebRoot 'styles.css'
        $script:AppPath = Join-Path $script:WebRoot 'app.js'
        $script:PackagePath = Join-Path $script:WebRoot 'package.json'
        $script:TsConfigPath = Join-Path $script:WebRoot 'tsconfig.json'
        $script:DesignPath = Join-Path $script:WebRoot 'DESIGN.md'
        $script:MockupsRoot = Join-Path $script:WebRoot 'mockups'
        $script:FrontendCompletionMockupPath = Join-Path $script:MockupsRoot 'frontend-completion-samples.html'
        $script:SrcRoot = Join-Path $script:WebRoot 'src'
        $script:ServedSourcePath = Join-Path $script:SrcRoot 'served-app.ts'
        $script:ServedSourcePartsRoot = Join-Path $script:SrcRoot 'served'
        $script:ScriptsRoot = Join-Path $script:WebRoot 'scripts'
        $script:BuildServedAssetScriptPath = Join-Path $script:ScriptsRoot 'build-served-asset.mjs'
        # part 목록을 여기 다시 적지 않고 build-served-asset.mjs 에서 읽는다. 2026-08-08 분해
        # 전까지 이 목록은 손으로 관리하는 세 번째 사본이었고, part 가 늘어도 갱신되지 않아
        # 새 part 로 옮겨진 심볼을 아래 검사들이 보지 못했다. 빌드가 번들에 넣는 것과 테스트가
        # 읽는 것은 같아야 한다. part 가 빌드에서 빠지면 심볼도 함께 사라지므로 검사는 통과가
        # 아니라 실패로 기운다 — 파생이 단언을 약화시키지 않는 이유다.
        $script:ServedSourcePartPaths = @(
            [regex]::Matches(
                (Get-Content -LiteralPath $script:BuildServedAssetScriptPath -Raw),
                '"(src/served/[A-Za-z0-9._-]+\.ts)"'
            ) | ForEach-Object { Join-Path $script:WebRoot $_.Groups[1].Value }
        )
        $script:ParityVerifyScriptPath = Join-Path $script:ScriptsRoot 'verify-static-parity.mjs'
        $script:BrowserFixtureScriptPath = Join-Path $script:ScriptsRoot 'verify-browser-fixture.mjs'
        $script:FrontendBatchValidatorPath = Join-Path $script:ScriptsRoot 'validate-frontend-completion-batches.mjs'
        $script:ParityRegenerateScriptPath = Join-Path $script:ScriptsRoot 'regenerate-static-parity.mjs'
        $script:ParityGeneratorPath = Join-Path $script:SrcRoot 'generate-parity-manifest.ts'
        $script:UserVisibleFixturePath = Join-Path $script:SrcRoot 'user-visible-fixtures.ts'
        $script:ParityManifestPath = Join-Path $script:WebRoot 'generated/parity/static-asset-parity.manifest.json'
        $script:FrontendBatchPlanPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.json'
        $script:FrontendBatchPlanDocPath = Join-Path $script:RepoRoot 'docs/superpowers/plans/2026-05-09-purecvisor-desktop-node-frontend-completion-auto-batches.md'
        $script:FeatureSurfaceLedgerPath = Join-Path $script:RepoRoot 'config/desktop-node-feature-surface-ledger.json'
        $script:FeatureSurfaceSchemaPath = Join-Path $script:RepoRoot 'config/desktop-node-feature-surface-ledger.schema.json'
        $script:FeatureSurfaceVerifierPath = Join-Path $script:ScriptsRoot 'verify-feature-surface-parity.mjs'
    }

    It 'validates and wires the stable Feature ID surface ledger' {
        $json = Get-Content -Raw -LiteralPath $script:FeatureSurfaceLedgerPath
        $package = Get-Content -Raw -LiteralPath $script:PackagePath | ConvertFrom-Json
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")

        $json | Test-Json -SchemaFile $script:FeatureSurfaceSchemaPath -ErrorAction Stop | Should -BeTrue
        $script:FeatureSurfaceVerifierPath | Should -Exist
        $package.scripts.'check:feature-surfaces' | Should -Be 'node scripts/verify-feature-surface-parity.mjs'
        $package.scripts.test | Should -Match '^npm run check:feature-surfaces &&'
        $servedSource | Should -Match 'featureId:\s*[''"]pcv\.'
    }

    It 'ships index, stylesheet, and script assets under the Desktop Node web root' {
        Test-Path -LiteralPath $script:IndexPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:StylesPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:AppPath -PathType Leaf | Should -BeTrue

        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $index | Should -Match 'PureCVisor Desktop Node'
        $index | Should -Match 'styles\.css'
        $index | Should -Match 'app\.js'
        $index | Should -Match 'id="app-root"'
    }

    It 'declares an inline favicon to avoid favicon.ico console noise' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw

        $index | Should -Match '<link rel="icon"'
        $index | Should -Match 'data:image/svg\+xml'
        $index | Should -Not -Match 'href="/favicon\.ico"'
    }

    It 'keeps the Desktop Node web console isolated from the Single Edge ui tree' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $styles = Get-Content -LiteralPath $script:StylesPath -Raw

        $index | Should -Not -Match '\.\./\.\./ui/'
        $app | Should -Not -Match '\.\./\.\./ui/'
        $styles | Should -Not -Match '\.\./\.\./ui/'
    }

    It 'ships a Desktop Node web design contract without importing Single Edge runtime routes' {
        Test-Path -LiteralPath $script:DesignPath -PathType Leaf | Should -BeTrue

        $design = Get-Content -LiteralPath $script:DesignPath -Raw

        $design | Should -Match 'PureCVisor Desktop Node Web DESIGN\.md'
        $design | Should -Match 'Supanova'
        $design | Should -Match 'Windows Desktop Node'
        $design | Should -Match 'Single UI Clone Mapping'
        $design | Should -Match 'asset explorer'
        $design | Should -Match 'status bar'
        $design | Should -Match 'Linux runtime screens are excluded'
        $design | Should -Match 'web/src/served-app\.ts'
        $design | Should -Match 'npm run verify:parity --prefix web'
        $design | Should -Not -Match '/auth/token|/ws/events|/containers|(?<!/qos)/storage|/ovn|/networks'
        $design | Should -Not -Match 'journalctl|libvirt|purecvisorsd'
    }

    It 'uses Desktop Node Supanova operation-console tokens in the active stylesheet' {
        $styles = Get-Content -LiteralPath $script:StylesPath -Raw

        $styles | Should -Match 'color-scheme:\s*dark'
        $styles | Should -Match '--bg:\s*#0a0f1a'
        $styles | Should -Match '--bg2:\s*#0f1525'
        $styles | Should -Match '--bg3:\s*#141c2e'
        $styles | Should -Match '--bg-panel:\s*rgba\(15,\s*21,\s*37'
        $styles | Should -Match '--accent:\s*#22d3ee'
        $styles | Should -Match '--green:\s*#34d399'
        $styles | Should -Match '--yellow:\s*#fbbf24'
        $styles | Should -Match '--red:\s*#f43f5e'
        $styles | Should -Match 'var\(--font-mono\)'
        $styles | Should -Not -Match '/auth/token|/ws/events|/containers|(?<!/qos)/storage|/ovn|/networks'
        $styles | Should -Not -Match 'journalctl|libvirt|purecvisorsd'
    }

    It 'ports the Single Edge visual shell into the active Desktop Node console without importing runtime routes' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $styles = Get-Content -LiteralPath $script:StylesPath -Raw
        $combined = $index + $styles

        $index | Should -Match 'data-ui-port="single-edge-visual-shell"'
        $index | Should -Match 'class="shell supanova-console[^"]*"'
        $index | Should -Match 'class="topbar glass-topbar[^"]*"'
        $index | Should -Match 'class="connection-form control-strip"'
        $index | Should -Match 'class="sidebar nav-rail[^"]*"'
        $styles | Should -Match '--supanova-glass:\s*rgba'
        $styles | Should -Match '--supanova-rail:\s*rgba'
        $styles | Should -Match '--supanova-shadow:'
        $styles | Should -Match '\.supanova-console'
        $styles | Should -Match '\.glass-topbar'
        $styles | Should -Match '\.control-strip'
        $styles | Should -Match '\.nav-rail'
        $styles | Should -Match '\.nav-rail a\.nav-active::before'
        $styles | Should -Match 'body::before'
        $combined | Should -Not -Match '<base\s+href="/ui/"|/auth/token|/ws/events|/containers|(?<!/qos)/storage|/ovn|/networks'
        $combined | Should -Not -Match 'journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd|Create Linux VM|ubuntu-24\.04'
    }

    It 'clones the Single console workbench frame while keeping Linux service surfaces excluded' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $styles = Get-Content -LiteralPath $script:StylesPath -Raw
        $combined = $index + $styles

        $index | Should -Match 'class="menu-bar"'
        $index | Should -Match 'class="activity-rail"'
        $index | Should -Match 'class="asset-tabs"'
        $index | Should -Match 'class="asset-search"'
        $index | Should -Match 'class="asset-list"'
        $index | Should -Match 'class="workspace-tabbar"'
        $index | Should -Match 'class="status-bar"'
        $index | Should -Match 'class="dashboard-hero"'
        $index | Should -Match 'class="dashboard-pills"'
        $index | Should -Match 'class="quick-action-grid"'
        $styles | Should -Match '\.single-clone-shell'
        $styles | Should -Match '\.menu-bar'
        $styles | Should -Match '\.activity-rail'
        $styles | Should -Match '\.asset-explorer'
        $styles | Should -Match '\.workspace-tabbar'
        $styles | Should -Match '\.status-bar'
        $styles | Should -Match '\.dashboard-hero'
        $styles | Should -Match '\.quick-action-grid'
        $combined | Should -Not -Match '컨테이너|LXC|ZFS|\bOVN\b|OVS|KVM|libvirt|purecvisorsd|/containers|/storage|/ovn|/networks'
    }

    It 'ships frontend completion mockup sample screens inside the Desktop Node web root' {
        Test-Path -LiteralPath $script:FrontendCompletionMockupPath -PathType Leaf | Should -BeTrue

        $mockup = Get-Content -LiteralPath $script:FrontendCompletionMockupPath -Raw

        $mockup | Should -Match '<!doctype html>'
        $mockup | Should -Match 'Frontend Completion Samples'
        $mockup | Should -Match 'Ops Cockpit'
        $mockup | Should -Match 'VM Workbench'
        $mockup | Should -Match 'Network Evidence'
        $mockup | Should -Match 'Recovery Desk'
        $mockup | Should -Match 'PCV_RATE_LIMIT_EXCEEDED'
        $mockup | Should -Match 'PCV_ROUTE_TIMEOUT'
        $mockup | Should -Match 'Public signing: excluded'
        $mockup | Should -Match 'External publication: not-claimed'
        $mockup | Should -Match '@media \(max-width:\s*820px\)'
        $mockup | Should -Not -Match 'https?://|//cdn|src="http|href="http'
        $mockup | Should -Not -Match '/auth/token|/ws/events|/containers|(?<!/qos)/storage|/ovn|/networks'
        $mockup | Should -Not -Match 'journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd|Create Linux VM|ubuntu-24\.04'
        $mockup | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
        $mockup | Should -Not -Match 'Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM'
    }

    It 'declares the 1-25 frontend completion work as five automatic staged batches' {
        Test-Path -LiteralPath $script:FrontendBatchPlanPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:FrontendBatchPlanDocPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:FrontendBatchValidatorPath -PathType Leaf | Should -BeTrue

        $planText = Get-Content -LiteralPath $script:FrontendBatchPlanPath -Raw
        $plan = $planText | ConvertFrom-Json
        $doc = Get-Content -LiteralPath $script:FrontendBatchPlanDocPath -Raw
        $package = Get-Content -LiteralPath $script:PackagePath -Raw | ConvertFrom-Json

        $plan.schema_version | Should -Be 1
        $plan.plan_id | Should -Be 'purecvisor-desktop-node-frontend-completion-auto-batches-2026-05-09'
        $plan.batch_count | Should -Be 5
        $plan.work_item_count | Should -Be 25
        $plan.host_mutation_performed | Should -BeFalse
        $plan.host_mutation_required | Should -BeFalse
        $plan.linux_runtime_excluded | Should -BeTrue
        $plan.single_ui_clone_required | Should -BeTrue
        @($plan.batches).Count | Should -Be 5

        $items = @($plan.batches | ForEach-Object { $_.work_items })
        @($items).Count | Should -Be 25
        for ($i = 1; $i -le 25; $i++) {
            @($items.id) | Should -Contain $i
        }
        foreach ($batch in @($plan.batches)) {
            @($batch.work_items).Count | Should -Be 5
            @($batch.write_scope).Count | Should -BeGreaterThan 0
            @($batch.verification_commands).Count | Should -BeGreaterThan 0
            foreach ($item in @($batch.work_items)) {
                $item.automatable | Should -BeTrue
                @($item.target_files).Count | Should -BeGreaterThan 0
                @($item.acceptance).Count | Should -BeGreaterThan 0
                @($item.verification).Count | Should -BeGreaterThan 0
            }
        }

        $plan.final_verification_commands | Should -Contain 'Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed'
        $plan.final_verification_commands | Should -Contain 'npm test --prefix web'
        $plan.final_verification_commands | Should -Contain 'npm run verify:parity --prefix web'
        $plan.final_verification_commands | Should -Contain 'node --check web/app.js'
        $plan.final_verification_commands | Should -Contain 'git diff --check'
        $package.scripts.'check:frontend-batches' | Should -Be 'node scripts/validate-frontend-completion-batches.mjs'
        $package.scripts.test | Should -Match 'check:frontend-batches'
        $doc | Should -Match 'Batch 1: Shell And Session'
        $doc | Should -Match 'Batch 5: Parity Visual A11y Evidence'
        ($planText + $doc) | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
        $planText | Should -Not -Match 'Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM'
    }

    It 'declares the Phase 2H API endpoints used by the console' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match '/api/v1/host/status'
        $app | Should -Match '/api/v1/vms'
        $app | Should -Match '/api/v1/jobs/'
        $app | Should -Match "jobAction:.*'cancel'"
        $app | Should -Match "jobAction:.*'retry'"
        $app | Should -Match "jobAction:.*'reconcile'"
    }

    It 'centralizes Local API access behind the Desktop Node frontend service registry' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $combined = $app + $servedSource

        $combined | Should -Match 'DESKTOP_NODE_API_ROUTES'
        $combined | Should -Match 'DESKTOP_NODE_ROUTE_COVERAGE'
        $combined | Should -Match 'desktopApi'
        $combined | Should -Match 'unwrapApiEnvelope'
        $combined | Should -Match 'unwrapApiList'
        $combined | Should -Match 'hostStatus:\s*[''"]/api/v1/host/status'
        $combined | Should -Match 'runtimePolicy:\s*[''"]/api/v1/runtime/policy'
        $combined | Should -Match 'opsSummary:\s*[''"]/api/v1/ops/summary'
        $combined | Should -Match 'networkInventory:\s*[''"]/api/v1/network/inventory'
        $combined | Should -Match 'jobsPage'
        $combined | Should -Match 'vmAction'
        $combined | Should -Match 'checkpointAction'
        $combined | Should -Match 'job\.reconcile'

        $servedSource | Should -Not -Match 'apiFetch\(\s*[''"]/api/v1'
        $servedSource | Should -Not -Match 'apiFetch\(\s*`/api/v1'
        $servedSource | Should -Not -Match '/auth/token|/ws/events|/containers|(?<!/qos)/storage|/ovn|/networks'
    }

    It 'declares the Web VM QoS and guest readback operator surface as read-only routes' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $combined = $app + $servedSource

        $combined | Should -Match 'vmBlkio'
        $combined | Should -Match 'vmBandwidth'
        $combined | Should -Match 'vmGuestAgentStatus'
        $combined | Should -Match 'vmGuestAgentPing'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/blkio'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/bandwidth'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/guest-agent/status'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/guest-agent/ping'
        $combined | Should -Match 'renderVmQosGuestReadback'
        $combined | Should -Match 'loadVmQosGuestReadbacks'
        $combined | Should -Match 'data-action="vm-qos-guest-refresh"'
        $combined | Should -Match 'linux_blkio_compatible'
        $combined | Should -Match 'linux_bandwidth_compatible'
        $combined | Should -Match 'qemu_guest_agent'
        $combined | Should -Match 'guest_heartbeat_verified'
        $combined | Should -Not -Match 'blkio-set'
    }

    It 'opens Web VM QoS and ADR-0009 Guest Execution direct control routes with explicit operator controls' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $design = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'web/DESIGN.md') -Raw
        $combined = $app + $servedSource + $design

        $combined | Should -Match 'vmQosStoragePreview'
        $combined | Should -Match 'vmQosStorage'
        $combined | Should -Match 'vmQosNetworkPreview'
        $combined | Should -Match 'vmQosNetwork'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/qos/storage/preview'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/qos/storage'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/qos/network/preview'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/qos/network'
        $combined | Should -Match 'data-action="vm-qos-storage-preview"'
        $combined | Should -Match 'data-action="vm-qos-storage-apply"'
        $combined | Should -Match 'data-action="vm-qos-network-preview"'
        $combined | Should -Match 'data-action="vm-qos-network-apply"'
        $combined | Should -Match 'ADR-0009'
        $combined | Should -Match 'vmGuestExec'
        $combined | Should -Match 'vmGuestChannelVerify'
        $combined | Should -Match 'vmGuestChannelEnsure'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/guest/exec'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/guest/channel/verify'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/guest/channel'
        $combined | Should -Match 'data-action="vm-guest-exec"'
        $combined | Should -Match 'data-action="guest-agent-ensure-channel"'
        $combined | Should -Match 'guest.exec'
        $combined | Should -Match 'guest.channel.configure'
        $combined | Should -Match 'ADR-0010'
        $combined | Should -Not -Match 'data-action="novnc-target-apply"'
    }

    It 'exposes running guest execution cancel affordance on Web job rows' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $combined = $app + $servedSource

        $combined | Should -Match 'formatJobCancelLabel'
        $combined | Should -Match 'getJobCancelScope'
        $combined | Should -Match 'data-job-cancel-scope="\$\{escapeHtml\(cancelScope\)\}"'
        $combined | Should -Match 'running-guest-execution'
        $combined | Should -Match 'Cancel running guest exec'
        $combined | Should -Match 'requireRbac\(''operate'', ''running guest execution cancel''\)'
    }

    It 'adds Windows-local command palette, global search, event center, and table helpers from the Single Edge borrowing map' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $styles = Get-Content -LiteralPath $script:StylesPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $combined = $index + $styles + $app + $servedSource

        $index | Should -Match 'id="global-search-input"'
        $index | Should -Match 'id="command-palette"'
        $index | Should -Match 'id="command-palette-input"'
        $index | Should -Match 'id="command-palette-results"'
        $index | Should -Match 'id="event-center-panel"'
        $index | Should -Match 'id="job-filter"'
        $index | Should -Match 'id="network-filter"'
        $combined | Should -Match 'buildCommandPaletteItems'
        $combined | Should -Match 'renderCommandPalette'
        $combined | Should -Match 'handleCommandSearch'
        $combined | Should -Match 'buildEventCenterItems'
        $combined | Should -Match 'renderEventCenter'
        $combined | Should -Match 'filterRowsByQuery'
        $combined | Should -Match 'sortRowsByKey'
        $combined | Should -Match 'renderTableStateSummary'
        $combined | Should -Match 'data-command-palette'
        $combined | Should -Match 'event-severity-lane'
        $combined | Should -Not -Match '/auth/token|/ws/events|/containers|(?<!/qos)/storage|/ovn|/networks'
        $combined | Should -Not -Match 'journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd|Create Linux VM|ubuntu-24\.04'
    }

    It 'splits frontend service logic into staged source parts before generating app.js' {
        $expectedParts = @(
            'types.ts',
            'state.ts',
            'routes.ts',
            'errors.ts',
            'api-client.ts'
        )

        foreach ($part in $expectedParts) {
            Test-Path -LiteralPath (Join-Path $script:ServedSourcePartsRoot $part) -PathType Leaf | Should -BeTrue
        }

        $buildScript = Get-Content -LiteralPath $script:BuildServedAssetScriptPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $buildScript | Should -Match 'servedSourceParts'
        foreach ($part in $expectedParts) {
            $buildScript | Should -Match ([regex]::Escape(('src/served/' + $part)))
        }

        $app | Should -Match 'Generated from staged src/served'
    }

    It 'supports optional bearer token requests' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match 'Authorization'
        $app | Should -Match 'Bearer'
        $app | Should -Match 'apiToken'
    }

    It 'declares account RBAC JWT login, refresh, session, and console capability UX' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $combined = $app + $servedSource

        $index | Should -Match 'id="account-login-form"'
        $index | Should -Match 'id="account-console-panel"'
        $index | Should -Match 'name="username"'
        $index | Should -Match 'name="password"'
        $combined | Should -Match '/api/v1/auth/login'
        $combined | Should -Match '/api/v1/auth/loopback-session'
        $combined | Should -Match 'ensureLoopbackSession'
        $index | Should -Not -Match 'access_token\s*[:=]'
        $combined | Should -Match '/api/v1/auth/refresh'
        $combined | Should -Match '/api/v1/auth/logout'
        $combined | Should -Match '/api/v1/auth/session'
        $combined | Should -Match '/api/v1/auth/rbac'
        $combined | Should -Match '/api/v1/console/capabilities'
        $combined | Should -Match '/api/v1/vms/\$\{encodeRouteSegment\(vmId\)\}/console'
        $combined | Should -Match "id:\s*['""]auth\.logout['""]"
        $combined | Should -Match 'authAccessToken'
        $combined | Should -Match 'authRefreshToken'
        $combined | Should -Match 'refreshAccountSession'
        $combined | Should -Match 'rbacAllows'
        $combined | Should -Match 'renderAccountSession'
        $combined | Should -Match 'renderConsolePanel'
        $combined | Should -Match 'noVNC'
        $combined | Should -Match 'vmconnect'
        $combined | Should -Not -Match '/auth/token|/ws/events'
    }

    It 'loads listener-provided API base URL before the served app starts' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedState = Get-Content -LiteralPath (Join-Path $script:WebRoot 'src/served/state.ts') -Raw

        $index | Should -Match '/pcv-config\.js'
        $index.IndexOf('/pcv-config.js') | Should -BeLessThan $index.IndexOf('/app.js')
        ($app + $servedState) | Should -Match 'PCV_DESKTOP_NODE_CONFIG'
        ($app + $servedState) | Should -Match 'apiBaseUrl'
        ($app + $servedState) | Should -Match 'window\.location\.origin'
    }

    It 'declares the VM create payload fields expected by POST /api/v1/vms' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match 'iso_path'
        $app | Should -Match 'memory_mb'
        $app | Should -Match 'disk_gb'
        $app | Should -Match 'vm_root'
        $app | Should -Match 'generation'
    }

    It 'declares the Phase 3B VM detail and lifecycle endpoints used by the console' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match '/api/v1/vms/'
        $app | Should -Match 'vmAction'
        $app | Should -Match "'start'"
        $app | Should -Match "'shutdown'"
        $app | Should -Match "'poweroff'"
        $app | Should -Match "'restart'"
        $app | Should -Match '/api/v1/vms/\{vm_id\}/attach'
        $app | Should -Match '/api/v1/vms/\{vm_id\}/save'
        $app | Should -Match '/api/v1/vms/\{vm_id\}/resume-saved'
        $app | Should -Match '/api/v1/vms/\{vm_id\}/manage'
    }

    It 'ships a VM detail panel mount point' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $index | Should -Match 'id="vm-detail-panel"'
        $index | Should -Match 'id="vm-detail-content"'
        $index | Should -Match 'id="vm-state-filter"'
        $index | Should -Match 'id="vm-sort"'
        $app | Should -Match 'vmStateFilter'
        $app | Should -Match 'vmSort'
        $app | Should -Match 'compareVms'
    }

    It 'declares lifecycle action handlers and destructive confirmation' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match 'data-action="vm-start"'
        $app | Should -Match 'data-action="vm-shutdown"'
        $app | Should -Match 'data-action="vm-poweroff"'
        $app | Should -Match 'data-action="vm-restart"'
        $app | Should -Match 'data-action="vm-save"'
        $app | Should -Match 'data-action="vm-resume-saved"'
        $app | Should -Match 'Resume saved'
        $app | Should -Match 'data-action="vm-manage"'
        $app | Should -Match 'Manage VM'
        $app | Should -Match 'queueVmManage'
        $app | Should -Match 'confirm_name'
        $app | Should -Match 'data-action="vm-delete"'
        $app | Should -Match 'PCV_VM_DELETE_RUNNING_BLOCKED'
        $app | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
        $app | Should -Match 'Unmanaged delete refusal remains'
        $app | Should -Match 'confirm\('
    }

    It 'declares checkpoint UI actions used by the console' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $app | Should -Match '/checkpoints'
        $app | Should -Match 'loadCheckpoints'
        $app | Should -Match 'checkpoint-create'
        $app | Should -Match 'checkpoint-restore'
        $app | Should -Match 'checkpoint-delete'
    }

    It 'persists tracked browser job history locally' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $index | Should -Match 'id="clear-job-history"'
        $app | Should -Match 'localStorage'
        $app | Should -Match 'pcvDesktopTrackedJobs\.v1'
        $app | Should -Match 'loadTrackedJobsFromStorage'
        $app | Should -Match 'saveTrackedJobsToStorage'
        $app | Should -Match 'clearTrackedJobHistory'
    }

    It 'hardens browser job orchestration with scoped pending state, polling backoff, and next-page loading' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $combined = $app + $servedSource

        $combined | Should -Match 'pendingVmActions'
        $combined | Should -Match 'pendingCheckpoints'
        $combined | Should -Match 'isVmActionPending'
        $combined | Should -Match 'isCheckpointActionPending'
        $combined | Should -Match 'scheduleNextPoll'
        $combined | Should -Match 'jobPollDelayMs'
        $combined | Should -Match 'window\.setTimeout'
        $combined | Should -Match 'loadNextJobPage'
        $combined | Should -Match 'next_offset'
        $combined | Should -Match 'data-action="load-next-jobs"'
    }

    It 'binds the Single-style shell controls to Desktop Node view and asset state' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $combined = $app + $servedSource

        $index | Should -Match 'data-menu-command="refresh"'
        $index | Should -Match 'data-menu-command="clear-browser-state"'
        $index | Should -Match 'data-view-link="vms"'
        $index | Should -Match 'data-view-link="network"'
        $index | Should -Match 'id="vm-asset-list"'
        $index | Should -Match 'id="workspace-tabbar"'
        $index | Should -Match 'data-shell-action="open-create-vm"'
        $index | Should -Match 'id="live-viewer-state"'
        $index | Should -Match 'id="logout-boundary"'
        $combined | Should -Match 'renderVmAssetList'
        $combined | Should -Match 'renderWorkspaceTabs'
        $combined | Should -Match 'handleShellCommand'
        $combined | Should -Match 'selectVmFromShell'
        $combined | Should -Match 'clearBrowserState'
        $combined | Should -Match 'closest.*data-view-link'
        ($index + $combined) | Should -Not -Match '/auth/token|/ws/events'
    }

    It 'declares operator activity and troubleshooting console surfaces' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw

        $index | Should -Match 'id="activity"'
        $index | Should -Match 'id="activity-panel"'
        $index | Should -Match 'id="troubleshooting"'
        $index | Should -Match 'id="troubleshooting-panel"'
        $app | Should -Match '/api/v1/runtime/policy'
        $app | Should -Match '/api/v1/jobs'
        ($app + $servedSource) | Should -Match 'jobsPage'
        ($app + $servedSource) | Should -Match 'desktopApi\.listJobs\(50,\s*0(?:,\s*options)?\)'
        ($app + $servedSource) | Should -Match 'pagination|next_offset|retention'
        $app | Should -Match 'loadRuntimePolicy'
        $app | Should -Match 'loadServerJobs'
        $app | Should -Match 'renderActivity'
        $app | Should -Match 'formatCorrelationValue'
        $app | Should -Match 'request_id'
        $app | Should -Match 'correlation_id'
        $app | Should -Match 'renderTroubleshooting'
        $app | Should -Match 'PCV_JOB_STORE_SCHEMA_UNSUPPORTED'
        $app | Should -Match 'PCV_VM_NOT_MANAGED_BY_PURECVISOR'
    }

    It 'declares the ops cockpit multi-view shell and summary route' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw
        $apiTypes = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'api-types.ts') -Raw
        $appSource = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'app.ts') -Raw

        $index | Should -Match 'id="view-dashboard"'
        $index | Should -Match 'id="view-vms"'
        $index | Should -Match 'id="view-troubleshooting"'
        $index | Should -Match 'id="ops-summary-panel"'
        $index | Should -Match 'id="priority-panel"'
        $index | Should -Match 'id="dashboard-activity-panel"'
        $index | Should -Match 'id="vm-workbench-context"'
        $index | Should -Match 'id="incident-panel"'
        $index | Should -Match 'data-view-link="dashboard"'
        $index | Should -Match 'data-view-link="vms"'
        $index | Should -Match 'data-view-link="jobs"'
        $index | Should -Match 'data-view-link="activity"'
        $index | Should -Match 'data-view-link="troubleshooting"'

        ($app + $servedSource + $appSource + $apiTypes) | Should -Match '/api/v1/ops/summary'
        ($app + $servedSource + $apiTypes) | Should -Match 'checkpoint_warnings'
        ($app + $servedSource) | Should -Match 'activeView'
        ($app + $servedSource) | Should -Match 'loadOpsSummary'
        ($app + $servedSource) | Should -Match 'renderOpsCockpit'
        ($app + $servedSource) | Should -Match 'renderDashboardActivity'
        ($app + $servedSource) | Should -Match 'recent_activity'
        ($app + $servedSource) | Should -Match 'summary-errors'
        ($app + $servedSource) | Should -Match 'renderVmWorkbenchContext'
        ($app + $servedSource) | Should -Match 'renderIncidentCommand'
        ($app + $servedSource) | Should -Match 'summaryError'
        $apiTypes | Should -Match 'OpsSummaryResponse'
        $appSource | Should -Match 'opsSummary'

        ($index + $app + $servedSource + $apiTypes + $appSource) | Should -Not -Match 'Create Linux VM|ubuntu-24\.04|journalctl|libvirt|purecvisorsd|KVM|ZFS|OVS|\bOVN\b'
        ($index + $app + $servedSource + $apiTypes + $appSource) | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
    }

    It 'declares the batch evidence dashboard surface' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw
        $apiTypes = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'api-types.ts') -Raw
        $fixtures = Get-Content -LiteralPath $script:UserVisibleFixturePath -Raw

        $index | Should -Match 'data-view-link="evidence"'
        $index | Should -Match 'id="evidence"'
        $index | Should -Match 'id="evidence-panel"'
        ($app + $servedSource) | Should -Match 'renderEvidenceDashboard'
        ($app + $servedSource) | Should -Match 'renderEvidenceStatusBadge'
        ($app + $servedSource) | Should -Match 'batch_evidence'
        ($app + $servedSource) | Should -Match 'renderCurrentEvidenceStatusBadge'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'current_evidence'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'public_boundary'
        ($app + $servedSource + $fixtures) | Should -Match 'Public boundary head'
        ($app + $servedSource + $fixtures) | Should -Match '26578120570'
        ($app + $servedSource + $fixtures) | Should -Match '7a7d5de822bdb058b04149eeeef0a7eb462828b5'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'full_admin_host_mutation'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'latest_package_pair'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'next_package_pair'
        ($app + $servedSource + $fixtures) | Should -Match 'Manual admin next'
        ($app + $servedSource + $fixtures) | Should -Match 'opened-public-boundary-current-evidence-rollup-payload'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'artifact-discovered'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'batch_evidence_artifact'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'gpu_snapshots'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'route_msi_hyperv'
        ($app + $servedSource + $apiTypes + $fixtures) | Should -Match 'os_mutation'
        ($app + $servedSource + $fixtures) | Should -Match 'not_configured|missing|degraded|unavailable|available'
        $fixtures | Should -Match 'status: "available"'
        $fixtures | Should -Match 'status: "degraded"'
        $fixtures | Should -Match 'status: "missing"'
        $fixtures | Should -Match 'status: "unavailable"'
    }

    It 'declares troubleshooting evidence degradation and failed job triage surfaces' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw
        $fixtures = Get-Content -LiteralPath $script:UserVisibleFixturePath -Raw

        $index | Should -Match 'id="troubleshooting-panel"'
        ($app + $servedSource) | Should -Match 'collectEvidenceIssues'
        ($app + $servedSource) | Should -Match 'renderTroubleshootingEvidence'
        ($app + $servedSource) | Should -Match 'renderFailedJobTriageRows'
        ($app + $servedSource) | Should -Match 'batch-evidence'
        ($app + $servedSource + $fixtures) | Should -Match 'PCV_BATCH_EVIDENCE_PARSE_FAILED|PCV_BATCH_EVIDENCE_ROOT_MISSING'
        ($app + $servedSource) | Should -Match 'retryable'
        ($app + $servedSource) | Should -Not -Match 'Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule'
    }

    It 'declares diagnostic bundle API create and download UX without direct host mutation commands' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw
        $fixtures = Get-Content -LiteralPath $script:UserVisibleFixturePath -Raw

        $index | Should -Match 'id="diagnostics-panel"'
        ($app + $servedSource) | Should -Match 'renderDiagnosticsBundle'
        ($app + $servedSource) | Should -Match 'getRuntimeApiRegistryBridge'
        ($app + $servedSource) | Should -Match 'renderRuntimeApiRegistryBridge'
        ($app + $servedSource) | Should -Match 'renderHostOpsLifecycleBucketTable'
        ($app + $servedSource) | Should -Match 'renderDiagnosticBundleList'
        ($app + $servedSource) | Should -Match 'createDiagnosticBundle'
        ($app + $servedSource) | Should -Match 'listDiagnosticBundles'
        ($app + $servedSource) | Should -Match 'state\.diagnosticBundleError\s*=\s*null;\s*state\.diagnosticBundlePage\s*=\s*page'
        ($app + $servedSource) | Should -Match 'downloadDiagnosticBundle'
        ($app + $servedSource) | Should -Match 'data-action="diagnostic-create"'
        ($app + $servedSource) | Should -Match 'data-action="diagnostic-download"'
        ($app + $servedSource) | Should -Match 'data-action="diagnostic-list-next"'
        ($app + $servedSource) | Should -Match '/api/v1/diagnostics/bundles'
        ($app + $servedSource) | Should -Match 'next_offset'
        ($app + $servedSource) | Should -Match 'max_bundle_count'
        ($app + $servedSource) | Should -Match '/download'
        ($app + $servedSource) | Should -Match 'CollectDiagnostics'
        ($app + $servedSource) | Should -Match 'Runtime/API registry bridge'
        ($app + $servedSource) | Should -Match 'ops summary direct expose'
        ($app + $servedSource) | Should -Match 'route detail metadata only'
        ($app + $servedSource) | Should -Match 'diagnostics-route-list'
        ($app + $servedSource) | Should -Match 'routeDetailHtml'
        ($app + $servedSource) | Should -Match 'Host Ops lifecycle buckets'
        ($app + $servedSource) | Should -Match 'diagnostics-hostops-table'
        ($app + $servedSource + $fixtures) | Should -Match 'service-action-eventlog-firewall-truststore-credential-manager-data-root-separated'
        ($app + $servedSource + $fixtures) | Should -Match 'credential-manager-system-proof'
        ($app + $servedSource + $fixtures) | Should -Match 'allowlisted-programdata-root'
        ($app + $servedSource) | Should -Match 'Host mutation: not performed by diagnostics view'
        ($app + $servedSource) | Should -Match 'runtime_api_registry_bridge|runtimeApiRegistryBridge'
        ($app + $servedSource) | Should -Match 'PureCVisor\\\\desktop-node\\\\diagnostics'
        ($app + $servedSource) | Should -Match 'API action'
        ($app + $servedSource) | Should -Match 'token values'
        ($app + $servedSource) | Should -Match 'Authorization headers'
        ($app + $servedSource) | Should -Match 'no host mutation'
        ($app + $servedSource) | Should -Not -Match 'pwsh|powershell|Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask'
        ($app + $servedSource) | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
    }

    It "keeps operator surface terms aligned with internal distribution boundary" {
        $app = Get-Content -LiteralPath $script:ServedSourcePath -Raw
        $html = Get-Content -LiteralPath $script:IndexPath -Raw
        $servedParts = ($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n"
        $terms = Get-Content -LiteralPath (Join-Path $script:RepoRoot 'docs/OPERATOR_SURFACE_TERMS.md') -Raw
        $webSurface = $app + $html + $servedParts

        $terms | Should -Match "배포 경계: 내부 사설망 전용"
        $terms | Should -Match "Diagnostic bundle: redaction이 적용된 server-side support bundle"
        $terms | Should -Match "VM delete 확인: Web Console과 CLI는 destructive VM delete 전에 명시 확인을 요구한다"
        $terms | Should -Match "Checkpoint mutation: Web Console은 checkpoint restore/delete confirmation dialog를 요구한다"
        $webSurface | Should -Match "Diagnostic bundle|diagnostic bundle"
        $webSurface | Should -Match "Only PureCVisor-managed VMs can be deleted"
        $webSurface | Should -Match "window\.confirm\(buildVmDeleteConfirmation"
        $webSurface | Should -Match "window\.confirm\(buildCheckpointRestoreConfirmation"
        $webSurface | Should -Match "window\.confirm\(buildCheckpointDeleteConfirmation"
        $webSurface | Should -Not -Match "(?is)(public trusted signing|winget public submission|winget submission|external stable publication|public release).{0,80}(available|ready|pass|claimed|enabled)"
    }

    It 'hardens final frontend service edge cases before installed-listener evidence' {
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")
        $styles = Get-Content -LiteralPath $script:StylesPath -Raw
        $design = Get-Content -LiteralPath $script:DesignPath -Raw
        $installedQaScript = Get-Content -LiteralPath (Join-Path $script:ScriptsRoot 'capture-installed-listener-qa.mjs') -Raw
        $combined = $app + $servedSource

        $combined | Should -Match 'getDiagnosticActionPolicy'
        $combined | Should -Match 'PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED'
        $combined | Should -Match 'diagnostic-retry'
        $combined | Should -Match '401'
        $combined | Should -Match '404'
        $combined | Should -Match '500'
        $combined | Should -Match 'timeout'
        $combined | Should -Match 'tokenRequiredRouteStatus'
        $combined | Should -Match 'browser token cleared'
        $combined | Should -Match 'all views refresh'
        $combined | Should -Match 'pending jobs are rechecked'
        $combined | Should -Match 'renderJobEdgeSummary'
        $combined | Should -Match 'No jobs on this page'
        $combined | Should -Match 'retained terminal jobs'
        $combined | Should -Match 'failed job retry'
        $combined | Should -Match 'vm\.delete'
        $combined | Should -Match 'Reconcile delete'
        $combined | Should -Match 'checkpoint\.create'
        $combined | Should -Match 'Reconcile checkpoint'
        $combined | Should -Match 'checkpoint\.restore'
        $combined | Should -Match 'Reconcile restore'
        $combined | Should -Match 'reconcileSelectedVm'
        $combined | Should -Match 'PCV_SELECTED_VM_STALE'
        $combined | Should -Match 'buildCheckpointRestoreConfirmation'
        $combined | Should -Match 'buildCheckpointDeleteConfirmation'
        $combined | Should -Match 'network-empty-state'
        $combined | Should -Match 'PCV_NATIVE_PARITY_INCOMPLETE'
        $combined | Should -Match 'native parity failure'
        $styles | Should -Match ':focus-visible'
        $styles | Should -Match '@media \(prefers-reduced-motion: reduce\)'
        $styles | Should -Match '@media \(max-width: 900px\)'
        $design | Should -Match 'Static parity snapshot policy'
        $design | Should -Match 'npm run build:served'
        $design | Should -Match 'npm run generate:parity'
        $installedQaScript | Should -Match 'installed-listener-qa'
        $installedQaScript | Should -Match 'PCV_BROWSER_QA_ACCOUNT_USERNAME'
        $installedQaScript | Should -Match 'PCV_BROWSER_QA_ACCOUNT_PASSWORD'
        $installedQaScript | Should -Match 'account-login-form'
        $installedQaScript | Should -Match 'diagnostic_create_clicked'
        $installedQaScript | Should -Match 'value_observed:\s*false'
        $installedQaScript | Should -Match '--disable-background-networking'
        $installedQaScript | Should -Match 'Browser\.close'
        $installedQaScript | Should -Match 'taskkill\.exe'
        $installedQaScript | Should -Match 'SIGKILL'
        $installedQaScript | Should -Match 'removeProfileDirectory'
        $installedQaScript | Should -Match 'rmSync\(profileDir'
        $installedQaScript | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
    }

    It 'declares token rotation operator UX without service token mutation' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw

        $index | Should -Match 'id="token-rotation-panel"'
        ($app + $servedSource) | Should -Match 'renderTokenRotation'
        ($app + $servedSource) | Should -Match 'Token Rotation'
        ($app + $servedSource) | Should -Match 'api-token\.dpapi\.json'
        ($app + $servedSource) | Should -Match 'rotation handoff'
        ($app + $servedSource) | Should -Match 'Clear browser token'
        ($app + $servedSource) | Should -Match 'no service token mutation'
        ($app + $servedSource) | Should -Not -Match 'Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask'
        ($app + $servedSource) | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
    }

    It 'declares a beta follow-up status surface without browser-started host mutation' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw

        $index | Should -Match 'id="beta-followup-panel"'
        ($app + $servedSource) | Should -Match 'renderBetaFollowup'
        ($app + $servedSource) | Should -Match 'Installed listener QA automation'
        ($app + $servedSource) | Should -Match 'service token revoke handoff'
        ($app + $servedSource) | Should -Match 'diagnostic retention pagination'
        ($app + $servedSource) | Should -Match 'VM delete guarded'
        ($app + $servedSource) | Should -Match 'ops cockpit P0/P1/P2'
        ($app + $servedSource) | Should -Match 'public distribution bundle'
        ($app + $servedSource) | Should -Match 'host mutation not started from browser'
        ($app + $servedSource) | Should -Not -Match 'Restart-Computer|msiexec|New-VM|Remove-VM|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask'
        ($app + $servedSource) | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
    }

    It 'declares read-only monitoring auth and checkpoint warning surfaces' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $index | Should -Match 'id="monitoring"'
        $index | Should -Match 'id="monitoring-panel"'
        $app | Should -Match 'renderMonitoring'
        $app | Should -Match 'buildMonitoringSignals'
        $app | Should -Match 'checkpoint-warning'
        $app | Should -Match 'token-policy'
        $app | Should -Match 'lan-exposure'
        $app | Should -Match 'route-timeout'
        $app | Should -Match 'request-limit'
        $app | Should -Match 'burst-limit'
        $app | Should -Match 'retry-after'
    }

    It 'declares a read-only network inventory view' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw
        $apiTypes = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'api-types.ts') -Raw
        $fixtures = Get-Content -LiteralPath $script:UserVisibleFixturePath -Raw

        $index | Should -Match 'data-view-link="network"'
        $index | Should -Match 'id="network"'
        $index | Should -Match 'id="view-network"'
        $index | Should -Match 'id="network-inventory-panel"'
        ($app + $servedSource + $apiTypes) | Should -Match '/api/v1/network/inventory'
        ($app + $servedSource) | Should -Match 'networkInventory'
        ($app + $servedSource) | Should -Match 'loadNetworkInventory'
        ($app + $servedSource) | Should -Match 'renderNetworkInventory'
        ($app + $servedSource + $fixtures) | Should -Match 'Default Switch'
        ($app + $servedSource + $fixtures) | Should -Match 'fixture-ethernet'
        ($app + $servedSource) | Should -Match 'read-only'
        ($app + $servedSource) | Should -Not -Match 'New-VMSwitch|Remove-VMSwitch|New-NetIPAddress|Set-NetFirewallRule'
    }

    It 'declares P2 operator workflow polish and quality gates' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $app = Get-Content -LiteralPath $script:AppPath -Raw

        $index | Should -Match 'id="vm-filter"'
        $index | Should -Match 'id="asset-status"'
        $app | Should -Match 'renderAssetStatus'
        $app | Should -Match 'buildVmLifecycleConfirmation'
        $app | Should -Match 'vmFilter'
        ($index + $app) | Should -Not -Match 'Create Linux VM|ubuntu-24\.04|journalctl|libvirt|purecvisorsd|ZFS|OVS|\bOVN\b'
    }

    It 'passes JavaScript syntax validation' {
        $output = & node --check $script:AppPath 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($output | Out-String)
    }

    It 'treats the served app.js asset as TypeScript build output' {
        Test-Path -LiteralPath $script:ServedSourcePath -PathType Leaf | Should -BeTrue

        $package = Get-Content -LiteralPath $script:PackagePath -Raw | ConvertFrom-Json
        $package.scripts.'build:served' | Should -Be 'node scripts/build-served-asset.mjs --write'
        $package.scripts.'check:served' | Should -Be 'node scripts/build-served-asset.mjs --check'
        $package.scripts.test | Should -Match 'check:served'
        $package.scripts.'verify:parity' | Should -Match 'check:served'

        $manifestText = Get-Content -LiteralPath $script:ParityManifestPath -Raw
        $manifest = $manifestText | ConvertFrom-Json
        $manifest.scaffold.runtimeReplacement | Should -Be 'default'
        $manifest.replacesServedAsset | Should -BeTrue
        $manifest.servedTypeScriptEntry | Should -Be 'src/served-app.ts'
        $manifest.regeneration.replacesServedAsset | Should -BeTrue
        $manifest.browserFixture.replacesServedAsset | Should -BeTrue
    }

    It 'declares a Phase 25 TypeScript scaffold that owns the served app.js asset' {
        Test-Path -LiteralPath $script:PackagePath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:TsConfigPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $script:SrcRoot 'api-types.ts') -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $script:SrcRoot 'view-model.ts') -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $script:SrcRoot 'app.ts') -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:ServedSourcePath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:UserVisibleFixturePath -PathType Leaf | Should -BeTrue

        $package = Get-Content -LiteralPath $script:PackagePath -Raw | ConvertFrom-Json
        $package.private | Should -BeTrue
        $package.scripts.test | Should -Be 'npm run check:feature-surfaces && tsc --noEmit -p tsconfig.json && npm run check:served && npm run check:frontend-batches'

        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $index | Should -Match 'app\.js'
        $index | Should -Not -Match 'src/app\.ts'
    }

    It 'keeps TypeScript source as the Local API contract mirror and served app source' {
        $apiTypes = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'api-types.ts') -Raw
        $viewModel = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'view-model.ts') -Raw
        $appSource = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'app.ts') -Raw
        $servedSource = (Get-Content -LiteralPath $script:ServedSourcePath -Raw) + (($script:ServedSourcePartPaths | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n")

        $apiTypes | Should -Match 'RuntimePolicyResponse'
        $apiTypes | Should -Match 'JobRuntimePolicy'
        $apiTypes | Should -Match 'VirtualMachineSummary'
        $apiTypes | Should -Match 'JobSummary'
        $viewModel | Should -Match 'buildDashboardViewModel'
        $appSource | Should -Match 'static-asset-parity-scaffold-first'
        $servedSource | Should -Match 'DOMContentLoaded'
        $servedSource | Should -Match 'apiFetch'

        ($apiTypes + $viewModel + $appSource + $servedSource) | Should -Match '/api/v1/runtime/policy'
        ($apiTypes + $viewModel + $appSource + $servedSource) | Should -Match '/api/v1/host/status'
        ($apiTypes + $viewModel + $appSource + $servedSource) | Should -Match '/api/v1/vms'
        ($apiTypes + $viewModel + $appSource + $servedSource) | Should -Not -Match 'Restart-Computer|msiexec|Register-ScheduledTask|New-VM|Remove-VM|New-NetFirewallRule'
        ($apiTypes + $viewModel + $appSource + $servedSource) | Should -Not -Match 'journalctl|libvirt|KVM|ZFS|OVS|\bOVN\b|purecvisorsd'
        ($apiTypes + $viewModel + $appSource + $servedSource) | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
    }

    It 'ships a generated TypeScript parity manifest for the served static asset' {
        Test-Path -LiteralPath $script:ParityGeneratorPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:ParityManifestPath -PathType Leaf | Should -BeTrue

        $manifestText = Get-Content -LiteralPath $script:ParityManifestPath -Raw
        $manifest = $manifestText | ConvertFrom-Json

        $manifest.generatedBy | Should -Be 'src/generate-parity-manifest.ts'
        $manifest.regeneratedBy | Should -Be 'scripts/regenerate-static-parity.mjs'
        $manifest.scaffold.decisionCandidate | Should -Be 'static-asset-parity-scaffold-first'
        $manifest.scaffold.runtimeReplacement | Should -Be 'default'
        $manifest.servedAsset | Should -Be 'app.js'
        $manifest.replacesServedAsset | Should -BeTrue
        $manifest.indexScriptSrc | Should -Be '/app.js'
        $manifest.typeScriptEntry | Should -Be 'src/app.ts'
        $manifest.servedTypeScriptEntry | Should -Be 'src/served-app.ts'
        $manifest.userVisibleFixtureEntry | Should -Be 'src/user-visible-fixtures.ts'
        $manifest.userVisibleFixtureNames | Should -Contain 'emptyInventory'
        $manifest.userVisibleFixtureNames | Should -Contain 'runningVmAndJob'
        $manifest.userVisibleFixtureNames | Should -Contain 'unsupportedHost'

        $manifest.localApiRoutes.runtimePolicy | Should -Be '/api/v1/runtime/policy'
        $manifest.localApiRoutes.hostStatus | Should -Be '/api/v1/host/status'
        $manifest.localApiRoutes.networkInventory | Should -Be '/api/v1/network/inventory'
        $manifest.localApiRoutes.vmList | Should -Be '/api/v1/vms'
        $manifest.localApiRoutes.jobList | Should -Be '/api/v1/jobs'
        $manifest.localApiRoutes.jobsPage | Should -Be '/api/v1/jobs?limit={limit}&offset={offset}'
        $manifest.localApiRoutes.vmDetail | Should -Be '/api/v1/vms/{vm_id}'
        $manifest.localApiRoutes.vmAction | Should -Be '/api/v1/vms/{vm_id}/{action}'
        $manifest.localApiRoutes.vmCheckpoints | Should -Be '/api/v1/vms/{vm_id}/checkpoints'
        $manifest.localApiRoutes.checkpointAction | Should -Be '/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}/{action}'
        $manifest.localApiRoutes.jobAction | Should -Be '/api/v1/jobs/{job_id}/{action}'
        $manifest.regeneration.source | Should -Be 'src/app.ts'
        $manifest.regeneration.output | Should -Be 'generated/parity/static-asset-parity.manifest.json'
        $manifest.regeneration.writeCommand | Should -Be 'npm run generate:parity'
        $manifest.regeneration.checkCommand | Should -Be 'npm run verify:parity'
        $manifest.regeneration.replacesServedAsset | Should -BeTrue

        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $index | Should -Match '<script src="/app\.js" defer></script>'
        $index | Should -Not -Match 'src/app\.ts'

        $manifestText | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
        $manifestText | Should -Not -Match 'Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM'
    }

    It 'ships user-visible fixture parity snapshots for the TypeScript-owned app.js' {
        Test-Path -LiteralPath $script:UserVisibleFixturePath -PathType Leaf | Should -BeTrue

        $fixtureSource = Get-Content -LiteralPath $script:UserVisibleFixturePath -Raw
        $manifestText = Get-Content -LiteralPath $script:ParityManifestPath -Raw

        $fixtureSource | Should -Match 'webConsoleUserVisibleParityFixtures'
        $fixtureSource | Should -Match 'buildStaticParitySnapshot'
        $fixtureSource | Should -Match 'emptyInventory'
        $fixtureSource | Should -Match 'runningVmAndJob'
        $fixtureSource | Should -Match 'unsupportedHost'
        $fixtureSource | Should -Match 'default'
        $fixtureSource | Should -Match 'app\.js'
        $fixtureSource | Should -Match 'src/served-app\.ts'
        $fixtureSource | Should -Not -Match 'document\.|window\.|dist/'

        $manifestText | Should -Match 'src/user-visible-fixtures\.ts'
        $manifestText | Should -Match 'emptyInventory'
        $manifestText | Should -Match 'runningVmAndJob'
        $manifestText | Should -Match 'unsupportedHost'
    }

    It 'declares generated static parity and served asset verification scripts' {
        Test-Path -LiteralPath $script:ParityVerifyScriptPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:ParityRegenerateScriptPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $script:ScriptsRoot 'build-served-asset.mjs') -PathType Leaf | Should -BeTrue

        $package = Get-Content -LiteralPath $script:PackagePath -Raw | ConvertFrom-Json
        $scriptNames = @($package.scripts.PSObject.Properties.Name)
        $scriptNames | Should -Contain 'build:served'
        $scriptNames | Should -Contain 'check:served'
        $scriptNames | Should -Contain 'generate:parity'
        $scriptNames | Should -Contain 'verify:parity'
        $scriptNames | Should -Contain 'check:frontend-batches'

        $package.scripts.'build:served' | Should -Be 'node scripts/build-served-asset.mjs --write'
        $package.scripts.'check:served' | Should -Be 'node scripts/build-served-asset.mjs --check'
        $package.scripts.'generate:parity' | Should -Be 'node scripts/regenerate-static-parity.mjs --write'
        $package.scripts.'verify:parity' | Should -Match 'check:served'
        $package.scripts.'verify:parity' | Should -Match 'scripts/regenerate-static-parity\.mjs --check'
        $package.scripts.'verify:parity' | Should -Match 'scripts/verify-static-parity\.mjs'
        $package.scripts.'verify:parity' | Should -Match 'browser:fixture'
        $package.scripts.'browser:fixture' | Should -Be 'node scripts/verify-browser-fixture.mjs'
        $package.scripts.'check:frontend-batches' | Should -Be 'node scripts/validate-frontend-completion-batches.mjs'
        Test-Path -LiteralPath $script:BrowserFixtureScriptPath -PathType Leaf | Should -BeTrue
        Test-Path -LiteralPath $script:FrontendBatchValidatorPath -PathType Leaf | Should -BeTrue

        $verifyScript = Get-Content -LiteralPath $script:ParityVerifyScriptPath -Raw
        $regenerateScript = Get-Content -LiteralPath $script:ParityRegenerateScriptPath -Raw
        $servedBuilderScript = Get-Content -LiteralPath (Join-Path $script:ScriptsRoot 'build-served-asset.mjs') -Raw
        $browserFixtureScript = Get-Content -LiteralPath $script:BrowserFixtureScriptPath -Raw
        $frontendBatchValidatorScript = Get-Content -LiteralPath $script:FrontendBatchValidatorPath -Raw
        $verifyScript | Should -Match 'static-asset-parity\.manifest\.json'
        $verifyScript | Should -Match 'src/generate-parity-manifest\.ts'
        $verifyScript | Should -Match 'src/app\.ts'
        $verifyScript | Should -Match 'src/served-app\.ts'
        $verifyScript | Should -Match 'index\.html'
        $verifyScript | Should -Match 'app\.js'
        $verifyScript | Should -Match 'verify-browser-fixture\.mjs'
        $servedBuilderScript | Should -Match 'typescript'
        $servedBuilderScript | Should -Match 'src/served-app\.ts'
        $servedBuilderScript | Should -Match 'app\.js'
        $regenerateScript | Should -Match 'typescript'
        $regenerateScript | Should -Match 'phase25WebConsoleScaffold'
        $regenerateScript | Should -Match 'localApiRoutes'
        $regenerateScript | Should -Match 'browser:fixture'
        $regenerateScript | Should -Match '--write'
        $regenerateScript | Should -Match '--check'
        $browserFixtureScript | Should -Match 'DOMContentLoaded'
        $browserFixtureScript | Should -Match 'pcv-browser-fixture'
        $browserFixtureScript | Should -Match 'job-browser-fixture'
        $frontendBatchValidatorScript | Should -Match 'frontend-completion-auto-batches\.json'
        $frontendBatchValidatorScript | Should -Match 'work_item_count'
    }

    It 'keeps generated static parity artifacts aligned with the TypeScript-owned served app.js route contract' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw
        $manifestText = Get-Content -LiteralPath $script:ParityManifestPath -Raw
        $manifest = $manifestText | ConvertFrom-Json
        $verifyScript = Get-Content -LiteralPath $script:ParityVerifyScriptPath -Raw
        $regenerateScript = Get-Content -LiteralPath $script:ParityRegenerateScriptPath -Raw
        $appSource = Get-Content -LiteralPath (Join-Path $script:SrcRoot 'app.ts') -Raw
        $servedSource = Get-Content -LiteralPath $script:ServedSourcePath -Raw
        $fixtureSource = Get-Content -LiteralPath $script:UserVisibleFixturePath -Raw
        $generatorSource = Get-Content -LiteralPath $script:ParityGeneratorPath -Raw

        $index | Should -Match '<script src="/app\.js" defer></script>'
        $index | Should -Not -Match 'src/app\.ts|dist/'

        $manifest.servedAsset | Should -Be 'app.js'
        $manifest.replacesServedAsset | Should -BeTrue
        $manifest.indexScriptSrc | Should -Be '/app.js'
        $manifest.typeScriptEntry | Should -Be 'src/app.ts'
        $manifest.servedTypeScriptEntry | Should -Be 'src/served-app.ts'
        $manifest.userVisibleFixtureEntry | Should -Be 'src/user-visible-fixtures.ts'
        $manifest.scaffold.decisionCandidate | Should -Be 'static-asset-parity-scaffold-first'
        $manifest.scaffold.runtimeReplacement | Should -Be 'default'
        $manifest.localApiRoutes.runtimePolicy | Should -Be '/api/v1/runtime/policy'
        $manifest.localApiRoutes.hostStatus | Should -Be '/api/v1/host/status'
        $manifest.localApiRoutes.vmList | Should -Be '/api/v1/vms'
        $manifest.localApiRoutes.jobList | Should -Be '/api/v1/jobs'
        $manifest.localApiRoutes.vmAction | Should -Be '/api/v1/vms/{vm_id}/{action}'
        $manifest.localApiRoutes.checkpointAction | Should -Be '/api/v1/vms/{vm_id}/checkpoints/{checkpoint_id}/{action}'
        $manifest.localApiRoutes.jobAction | Should -Be '/api/v1/jobs/{job_id}/{action}'
        $manifest.regeneration.output | Should -Be 'generated/parity/static-asset-parity.manifest.json'
        $manifest.regeneration.writeCommand | Should -Be 'npm run generate:parity'
        $manifest.regeneration.checkCommand | Should -Be 'npm run verify:parity'
        $manifest.regeneration.replacesServedAsset | Should -BeTrue
        $manifest.browserFixture.script | Should -Be 'scripts/verify-browser-fixture.mjs'
        $manifest.browserFixture.command | Should -Be 'npm run browser:fixture'
        $manifest.browserFixture.mode | Should -Be 'node-vm-minimal-dom'
        $manifest.browserFixture.mutating | Should -BeFalse
        $manifest.browserFixture.replacesServedAsset | Should -BeTrue

        $browserFixtureScript = Get-Content -LiteralPath $script:BrowserFixtureScriptPath -Raw
        ($verifyScript + $regenerateScript + $browserFixtureScript + $appSource + $servedSource + $fixtureSource + $generatorSource + $manifestText) | Should -Match 'static-asset-parity-scaffold-first'
        ($verifyScript + $regenerateScript + $browserFixtureScript + $appSource + $servedSource + $fixtureSource + $generatorSource + $manifestText) | Should -Match 'src/served-app\.ts'
        ($verifyScript + $regenerateScript + $browserFixtureScript + $appSource + $servedSource + $fixtureSource + $generatorSource + $manifestText) | Should -Match '/api/v1/runtime/policy'
        ($verifyScript + $regenerateScript + $browserFixtureScript + $appSource + $servedSource + $fixtureSource + $generatorSource + $manifestText) | Should -Match '/api/v1/host/status'
        ($verifyScript + $regenerateScript + $browserFixtureScript + $appSource + $servedSource + $fixtureSource + $generatorSource + $manifestText) | Should -Match '/api/v1/vms'
        ($verifyScript + $regenerateScript + $browserFixtureScript + $appSource + $servedSource + $fixtureSource + $generatorSource + $manifestText) | Should -Match '/api/v1/jobs'
    }

    It 'does not place secrets or host mutation command strings in parity scripts, TypeScript source, or generated output' {
        $scanFiles = @(
            $script:PackagePath,
            $script:ParityVerifyScriptPath,
            $script:ParityRegenerateScriptPath,
            $script:BrowserFixtureScriptPath,
            $script:ParityGeneratorPath,
            (Join-Path $script:ScriptsRoot 'build-served-asset.mjs'),
            $script:FrontendBatchValidatorPath,
            (Join-Path $script:SrcRoot 'app.ts'),
            $script:ServedSourcePath,
            $script:ServedSourcePartPaths,
            $script:UserVisibleFixturePath,
            (Join-Path $script:SrcRoot 'api-types.ts'),
            (Join-Path $script:SrcRoot 'view-model.ts'),
            $script:ParityManifestPath
        )

        $combined = ($scanFiles | ForEach-Object { Get-Content -LiteralPath $_ -Raw }) -join "`n"
        $combined | Should -Not -Match 'Bearer\s+(?!\$\{)[A-Za-z0-9._~+/=-]{24,}'
        $combined | Should -Not -Match 'Restart-Computer|msiexec|New-NetFirewallRule|Register-EventSource|New-EventLog|Register-ScheduledTask|New-VM|Remove-VM'
    }

    It 'keeps fabricated operational values out of the static console shell' {
        $index = Get-Content -LiteralPath $script:IndexPath -Raw

        # Location-space guard: these literals must not appear ANYWHERE in the shell,
        # including inside a new element that carries no id. Keep them alongside the
        # id-anchored patterns below — the two sets cover different axes and neither
        # subsumes the other.
        #
        # '>Connected<' does match an id-carrying span, because the '>' is the open
        # tag's terminator and the id lands before it. It does not match the neutral
        # '>Not connected<' initial value, since the character before 'connected'
        # there is a space rather than '>'.
        $forbidden = @(
            '>Connected<',
            'VM: 3/3',
            'API: 10ms avg',
            'Updated 0s ago',
            '<strong>4/5</strong>',
            'pcv-node-a',
            'pcv-node-b',
            'lab-vm-01'
        )
        foreach ($pattern in $forbidden) {
            $index | Should -Not -Match ([regex]::Escape($pattern)) -Because "index.html must not ship the fabricated operational value '$pattern'"
        }

        # Value-space guard: every span this branch gave an id to is additionally
        # guarded on that id, so a reintroduction that varies the digits or wording
        # is caught even when it evades the literals above. `\w` does not match the
        # neutral em-dash placeholder.
        $forbiddenBindings = [ordered]@{
            'id="status-connection"[^>]*>\s*Connected\b'  = 'fabricated Connected footer state'
            'id="status-host"[^>]*>\s*\w'                 = 'hardcoded host identifier or Windows caption'
            'id="status-updated"[^>]*>\s*Updated\s+\d'    = 'fabricated refresh timestamp'
            'id="status-vm-count"[^>]*>\s*VM:\s*\d'       = 'fabricated VM count'
            'id="hero-workload"[^>]*>\s*\d'               = 'fabricated active workload count'
            'id="hero-host-mode"[^>]*>\s*\w'              = 'fabricated host mode/readiness'
            'id="hero-alerts"[^>]*>\s*\d'                 = 'fabricated recent alert count'
            'id="asset-count"[^>]*>\s*\d'                 = 'fabricated VM asset count seed'
        }
        foreach ($binding in $forbiddenBindings.GetEnumerator()) {
            $index | Should -Not -Match $binding.Key -Because "index.html must not seed the $($binding.Value) into a real-state-bound element"
        }

        $requiredIds = @(
            'status-connection',
            'status-host',
            'status-updated',
            'status-vm-count',
            'status-view',
            'hero-workload',
            'hero-host-mode',
            'hero-alerts'
        )
        foreach ($id in $requiredIds) {
            $index | Should -Match ([regex]::Escape("id=`"$id`"")) -Because "index.html must expose #$id for real-state binding"
        }
    }
}
