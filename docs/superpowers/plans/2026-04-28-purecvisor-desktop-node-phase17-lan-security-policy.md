# PureCVisor Desktop Node Phase 17 LAN Security Policy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node LAN mode를 loopback-default, admin opt-in preview, reverse-proxy-required 정책으로 고정하고 product manifest, runtime policy, diagnostics, 문서에 반영한다.

**Architecture:** `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`가 제품 후보 배포 계층의 LAN security policy v1을 소유한다. `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`는 runtime policy에 network policy를 노출하고, 기존 Phase 5/13 LAN/static auth behavior는 깨지 않는다.

**Tech Stack:** PowerShell 7, Pester 5, Windows `HttpListener`, WinSW product wrapper, JSON manifest, JSONL diagnostics.

---

## 설계 기준

- 설계 문서: `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy-design.md`
- 결정 토큰: `DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required`
- 유지 결정: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 경계: `packaging/windows-desktop-node/**`와 `spikes/purecvisor-desktop-node/**`만 수정한다. Linux `purecvisorsd`, Single Edge `ui/**`, Single Edge API 공개 표면은 변경하지 않는다.

## 파일 구조

수정:

- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`: LAN security policy helper, product plan/manifest field, diagnostic bundle source를 추가한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`: product plan의 loopback default, LAN policy, service host non-LAN default를 검증한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`: product manifest의 LAN security policy v1을 검증한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`: diagnostic bundle이 LAN security policy를 redaction된 artifact로 포함하는지 검증한다.
- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`: runtime policy network object를 추가한다.
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`: runtime policy network fields를 검증한다.
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Lan.Tests.ps1`: Phase 17 LAN policy helper와 non-loopback static bearer policy를 명시적으로 고정한다.
- `docs/DEVELOPER_INDEX.md`: Phase 17 spec/plan 진입점을 추가한다.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: Phase 17 검증 mapping을 추가한다.
- `docs/PUBLIC_RELEASE_BOUNDARY.md`: Phase 17 LAN policy가 Single Edge 공개 표면과 분리됨을 명시한다.
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`: Phase 17 상태를 갱신한다.
- `packaging/windows-desktop-node/README.md`: Phase 17 product wrapper LAN policy를 설명한다.
- `spikes/purecvisor-desktop-node/README.md`: Desktop Node root 경계를 Phase 17까지 갱신한다.
- `spikes/purecvisor-desktop-node/api/README.md`: Local API LAN runtime policy와 static auth boundary를 설명한다.
- `AGENTS.md`, `README.md`, `follower.md`: Phase 17 문서 링크와 후속 실행 순서를 갱신한다.

## 작업 원칙

- LAN mode는 기본 install, repair, service start path에서 켜지지 않는다.
- Service host 기본 인자에 `-AllowLan`, `-EnsureFirewallRule`, non-loopback prefix를 추가하지 않는다. Phase 13 당시 WinSW XML과 2026-05-01 이후 `DesktopNode.Host.exe listen` 경로 모두 같은 기본 정책을 따른다.
- TLS는 product wrapper 내장 기능으로 구현하지 않는다.
- 실제 firewall rule 적용이나 LAN listener start는 기본 검증에서 실행하지 않는다.
- runtime policy와 diagnostic bundle은 token value, protected token blob, host-sensitive path를 노출하지 않는다.

---

### Task 1: Product Wrapper LAN Security Policy

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: product plan 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Plan.Tests.ps1`의 install plan test에 다음 assertions를 추가한다.

```powershell
$plan.network.schema_version | Should -Be 1
$plan.network.decision | Should -Be 'DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required'
$plan.network.default_exposure | Should -Be 'loopback'
$plan.network.lan_mode.state | Should -Be 'preview-admin-opt-in'
$plan.network.lan_mode.enabled_by_default | Should -BeFalse
$plan.network.lan_mode.requires_allow_lan | Should -BeTrue
$plan.network.lan_mode.requires_bearer_token | Should -BeTrue
$plan.network.lan_mode.non_loopback_static_auth | Should -Be 'bearer-required'
$plan.network.tls.provided_by_product_wrapper | Should -BeFalse
$plan.network.tls.required_for_lan | Should -BeTrue
$plan.network.firewall.enabled_by_default | Should -BeFalse
$plan.network.firewall.installer_auto_enable | Should -BeFalse
$plan.service.config.exposure | Should -Be 'loopback'
$plan.service.winsw.xml | Should -Not -Match '-AllowLan'
$plan.service.winsw.xml | Should -Not -Match '-EnsureFirewallRule'
```

- [x] **Step 2: product manifest 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Manifest.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'records LAN security policy v1 in product-manifest.json' {
    $manifest = New-PcvDesktopNodeProductManifest `
        -SourceRoot $script:RepoRoot `
        -ProductRoot 'C:\Program Files\PureCVisor\DesktopNode' `
        -DataRoot 'C:\ProgramData\PureCVisor\desktop-node' `
        -Version '0.17.0'

    $manifest.network.schema_version | Should -Be 1
    $manifest.network.default_exposure | Should -Be 'loopback'
    $manifest.network.lan_mode.state | Should -Be 'preview-admin-opt-in'
    $manifest.network.lan_mode.enabled_by_default | Should -BeFalse
    $manifest.network.lan_mode.token_source | Should -Be 'dpapi-local-machine-protected-file'
    $manifest.network.tls.termination | Should -Be 'external-reverse-proxy-or-tls-terminator'
    $manifest.network.firewall.lifecycle_owner | Should -Be 'admin-opt-in-product-action-or-manual-command'
}
```

- [x] **Step 3: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

Expected: `network` field가 없어서 관련 assertions가 실패한다.

- [x] **Step 4: LAN security policy helper를 구현한다**

`PcvDesktopNodeProduct.psm1`에 다음 helper를 추가한다.

```powershell
function Get-PcvDesktopNodeLanSecurityPolicy {
    [ordered]@{
        schema_version = 1
        decision = 'DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required'
        default_exposure = 'loopback'
        lan_mode = [ordered]@{
            state = 'preview-admin-opt-in'
            enabled_by_default = $false
            requires_allow_lan = $true
            requires_bearer_token = $true
            token_source = 'dpapi-local-machine-protected-file'
            non_loopback_static_auth = 'bearer-required'
        }
        tls = [ordered]@{
            provided_by_product_wrapper = $false
            required_for_lan = $true
            termination = 'external-reverse-proxy-or-tls-terminator'
        }
        firewall = [ordered]@{
            enabled_by_default = $false
            lifecycle_owner = 'admin-opt-in-product-action-or-manual-command'
            installer_auto_enable = $false
            default_profile = 'private'
        }
        diagnostics = [ordered]@{
            record_exposure = $true
            record_tls_stance = $true
            record_firewall_plan = $true
            record_token_storage = $true
        }
    }
}
```

`New-PcvDesktopNodeProductManifest`와 `New-PcvDesktopNodeProductPlan` 반환 object에 다음 field를 추가한다.

```powershell
network = Get-PcvDesktopNodeLanSecurityPolicy
```

- [x] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

Expected: targeted product wrapper tests가 `Failed: 0`으로 통과한다.

- [x] **Step 6: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1
git commit -m "feat: add Desktop Node LAN security policy"
```

---

### Task 2: Local API Runtime Network Policy

**Files:**

- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Lan.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [x] **Step 1: runtime policy 실패 테스트를 추가한다**

`PcvDesktopApi.Contract.Tests.ps1`의 `exposes the current Local API runtime hardening policy` test에 다음 assertions를 추가한다.

```powershell
$json.data.network.default_exposure | Should -Be 'loopback'
$json.data.network.current_exposure | Should -Be 'loopback'
$json.data.network.lan_mode | Should -Be 'preview-admin-opt-in'
$json.data.network.static_asset_auth.loopback | Should -Be 'unauthenticated-static-only'
$json.data.network.static_asset_auth.non_loopback | Should -Be 'bearer-required'
$json.data.network.tls.provided_by_product_wrapper | Should -BeFalse
$json.data.network.tls.required_for_lan | Should -BeTrue
$json.data.network.tls.termination | Should -Be 'external-reverse-proxy-or-tls-terminator'
$json.data.network.firewall.enabled_by_default | Should -BeFalse
$json.data.network.firewall.lifecycle_owner | Should -Be 'admin-opt-in-product-action-or-manual-command'
```

- [x] **Step 2: LAN static auth boundary test를 명확히 추가한다**

`PcvDesktopApi.Lan.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'keeps non-loopback static assets behind bearer auth by policy' {
    $policy = Get-PcvApiRuntimePolicy -TokenStorage 'dpapi-local-machine' -CurrentExposure 'lan'

    $policy.network.current_exposure | Should -Be 'lan'
    $policy.network.static_asset_auth.non_loopback | Should -Be 'bearer-required'
    $policy.network.static_asset_auth.loopback | Should -Be 'unauthenticated-static-only'
    $policy.network.tls.required_for_lan | Should -BeTrue
}
```

- [x] **Step 3: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Lan.Tests.ps1' -Output Detailed"
```

Expected: `network` field 또는 `CurrentExposure` parameter가 없어서 실패한다.

- [x] **Step 4: runtime policy helper를 구현한다**

`PcvDesktopApi.psm1`에서 `Get-PcvApiRuntimePolicy`에 optional `CurrentExposure` parameter를 추가하고, 반환 object에 다음 field를 추가한다.

```powershell
network = [ordered]@{
    default_exposure = 'loopback'
    current_exposure = $CurrentExposure
    lan_mode = 'preview-admin-opt-in'
    static_asset_auth = [ordered]@{
        loopback = 'unauthenticated-static-only'
        non_loopback = 'bearer-required'
    }
    tls = [ordered]@{
        provided_by_product_wrapper = $false
        required_for_lan = $true
        termination = 'external-reverse-proxy-or-tls-terminator'
    }
    firewall = [ordered]@{
        enabled_by_default = $false
        lifecycle_owner = 'admin-opt-in-product-action-or-manual-command'
        installer_auto_enable = $false
        default_profile = 'private'
    }
}
```

기본값은 다음처럼 둔다.

```powershell
[ValidateSet('loopback', 'lan')][string]$CurrentExposure = 'loopback'
```

`Start-PcvDesktopApi`의 runtime policy call path에서는 `$prefixPolicy.exposure`를 넘길 수 있도록 필요한 context 전달을 추가한다. `Invoke-PcvApiRequest`에서 prefix context가 없으면 기본값 `loopback`을 사용한다.

- [x] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1','spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Lan.Tests.ps1' -Output Detailed"
```

Expected: targeted API tests가 `Failed: 0`으로 통과한다.

- [x] **Step 6: Commit**

```powershell
git add spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1 spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1 spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Lan.Tests.ps1
git commit -m "feat: expose Desktop Node LAN runtime policy"
```

---

### Task 3: Diagnostics and Redaction

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: diagnostic bundle 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Diagnostics.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'includes LAN security policy in diagnostic bundle manifest without enabling LAN' {
    $outRoot = Join-Path $TestDrive 'diagnostics-lan-policy'
    $plan = New-PcvDesktopNodeProductPlan `
        -Action CollectDiagnostics `
        -SourceRoot $script:RepoRoot `
        -ProductRoot (Join-Path $TestDrive 'DesktopNodeLanDiag') `
        -DataRoot (Join-Path $TestDrive 'data-lan-diag')
    $runner = {
        param([string]$FileName, [string[]]$Arguments)
        [ordered]@{ exit_code = 0; stdout = 'Stopped'; stderr = '' }
    }
    $runtimePolicy = {
        param($Plan)
        [ordered]@{
            ok = $true
            body = '{"network":{"current_exposure":"loopback","tls":{"required_for_lan":true}}}'
        }
    }

    $bundle = New-PcvDesktopNodeDiagnosticBundle `
        -Plan $plan `
        -OutputRoot $outRoot `
        -InvokeProcess $runner `
        -CollectRuntimePolicy $runtimePolicy

    $manifest = Get-Content -LiteralPath (Join-Path $bundle.path 'diagnostics-manifest.json') -Raw | ConvertFrom-Json
    $manifest.policy.network.default_exposure | Should -Be 'loopback'
    $manifest.policy.network.lan_mode.enabled_by_default | Should -BeFalse
    $manifest.policy.network.tls.required_for_lan | Should -BeTrue
    @($manifest.sources.name) | Should -Contain 'lan_security_policy'
}
```

- [x] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: `lan_security_policy` source 또는 manifest `policy.network`가 없어서 실패한다.

- [x] **Step 3: diagnostic bundle source를 구현한다**

`New-PcvDesktopNodeDiagnosticBundleSourceList`가 `lan_security_policy` source를 포함하도록 추가한다.

```powershell
[ordered]@{
    name = 'lan_security_policy'
    artifact = 'lan-security-policy-redacted.json'
    required = $true
    redacted = $true
}
```

`New-PcvDesktopNodeDiagnosticBundle`에서 다음 artifact를 쓴다.

```powershell
Write-PcvDesktopNodeDiagnosticJson `
    -Path (Join-Path $bundlePath 'lan-security-policy-redacted.json') `
    -InputObject (ConvertTo-PcvDesktopNodeDiagnosticRedactedObject -InputObject $Plan.network -Plan $Plan)
```

diagnostics manifest의 `policy` object가 `diagnostics`와 `network`를 함께 포함하도록 구성한다.

- [x] **Step 4: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: diagnostics suite가 `Failed: 0`으로 통과한다.

- [x] **Step 5: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1
git commit -m "feat: include Desktop Node LAN policy diagnostics"
```

---

### Task 4: Documentation Synchronization

**Files:**

- Modify: `AGENTS.md`
- Modify: `README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `follower.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/api/README.md`

- [x] **Step 1: active docs를 Phase 17 상태로 갱신한다**

다음 문구를 기준으로 각 문서의 Phase 17 상태를 통일한다.

```text
Desktop Node Phase 17 LAN mode 제품 보안 정책은 loopback-only 기본값, LAN preview/admin opt-in, reverse proxy/TLS 전제, non-loopback static bearer auth, firewall admin opt-in lifecycle을 제품 후보 배포 계층과 Local API runtime policy에 고정한다.
```

Phase 17 완료 후 active docs는 구현 상태를 표시하되, suite pass count는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`와 이 plan의 `완료 증거`에만 기록한다.

- [x] **Step 2: 문서 동기화 guard를 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: root boundary/documentation sync suite가 `Failed: 0`으로 통과한다.

- [x] **Step 3: Commit**

```powershell
git add AGENTS.md README.md docs/DEVELOPER_INDEX.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md follower.md packaging/windows-desktop-node/README.md spikes/purecvisor-desktop-node/README.md spikes/purecvisor-desktop-node/api/README.md
git commit -m "docs: document Desktop Node phase 17 LAN policy"
```

---

### Task 5: Full Verification and PR Handoff

**Files:**

- Modify: `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy.md`

- [x] **Step 1: full Phase 17 verification을 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Expected: 모든 명령이 exit 0이고 Pester output에 `Failed: 0`이 표시된다.

- [x] **Step 2: product dry-run smoke를 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
```

Expected: 두 명령 모두 exit 0 JSON을 출력하고, `network.default_exposure = loopback`, `network.lan_mode.enabled_by_default = false`, `network.tls.required_for_lan = true`를 확인할 수 있다.

- [x] **Step 3: 완료 증거를 기록한다**

이 plan의 `완료 증거` 섹션에 실행 명령, 결과, 관리자 smoke 보류 사유를 기록한다. 실제 LAN listener/firewall/reverse proxy smoke를 실행하지 않았다면 그 이유를 명시한다.

- [x] **Step 4: 최종 커밋을 만든다**

```powershell
git add docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy.md
git commit -m "docs: record Desktop Node phase 17 verification"
```

- [x] **Step 5: PR을 준비한다**

```powershell
git status --short --branch
git push -u origin codex/desktop-node-phase17-lan-security
```

PR base는 `codex/desktop-node-phase16-diagnostics`로 둔다. Phase 14 canonical line 정리가 끝나기 전에는 draft PR로 유지한다.

## 완료 증거

Phase 17 구현과 문서 동기화를 완료했다.

구현 커밋:

- `c5d3540 docs: start Desktop Node phase 17 LAN policy`
- `fef5d82 feat: add Desktop Node LAN security policy`
- `8dc07c4 feat: expose Desktop Node LAN runtime policy`
- `e42cdf2 feat: include Desktop Node LAN policy diagnostics`
- `fe28ea1 docs: document Desktop Node phase 17 LAN policy`

검증 결과:

- `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 61 passed, 0 failed
- `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"`: 92 passed, 0 failed
- `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"`: 19 passed, 0 failed
- `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 10 passed, 0 failed
- `node --check spikes/purecvisor-desktop-node/web/app.js`: exit 0
- `git diff --check`: exit 0

Product dry-run smoke:

- `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan`: exit 0, `network.default_exposure = loopback`, `network.lan_mode.state = preview-admin-opt-in`, `network.tls.required_for_lan = true`, `network.firewall.enabled_by_default = false`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf`: exit 0, `dry_run = true`, `execution_skipped = true`, `network.default_exposure = loopback`, `network.lan_mode.state = preview-admin-opt-in`, `network.tls.required_for_lan = true`, `network.firewall.enabled_by_default = false`
- Diagnostic bundle manifest와 `lan-security-policy-redacted.json` artifact 포함 계약은 `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`의 `includes LAN security policy in diagnostic bundle manifest without enabling LAN` 테스트로 검증했다.

관리자 smoke 보류:

- 실제 LAN listener start, Windows Firewall rule ensure, reverse proxy/TLS smoke는 관리자 권한과 운영자가 제공한 네트워크/TLS endpoint가 필요하므로 실행하지 않았다.
- 실제 product install/start/rollback/uninstall, protected token ACL inspection, Event Log source 등록은 Phase 17 범위에서 기본 검증과 분리하고 관리자 opt-in 후속으로 남긴다.
