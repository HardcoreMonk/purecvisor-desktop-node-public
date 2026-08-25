# PureCVisor Desktop Node Phase 16 Event Log and Long-Term Diagnostics Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node 제품 wrapper에 JSONL first 장기 로그 정책, versioned diagnostic bundle schema, Windows Event Log opt-in 등록 계획을 추가한다.

**Architecture:** `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`가 product data root의 diagnostics policy, file rotation, diagnostic bundle manifest, Event Log registration plan을 소유한다. Local API와 service spike의 기존 `-EventLogPath` JSONL 계약은 유지하고, 문서와 root boundary suite가 Phase 16 상태를 단일하게 가리키도록 갱신한다.

**Tech Stack:** PowerShell 7, Pester 5, service host logs, JSONL file logs, versioned JSON diagnostic bundle manifest. Phase 13 당시 service host logs는 WinSW stdout/stderr였고, 2026-05-01 replacement slice 이후 기본 host는 `DesktopNode.Host.exe`다.

---

## 설계 기준

- 설계 문서: `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics-design.md`
- 결정 토큰: `DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred`
- 유지 결정: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 경계: `packaging/windows-desktop-node/**` 중심으로 구현한다. `spikes/purecvisor-desktop-node/**`의 Local API/service JSONL 계약은 깨지 않는다.

## 파일 구조

수정:

- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`: diagnostics policy v1, log rotation helper, Event Log registration plan, diagnostic bundle manifest를 추가한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`: policy/manifest/redaction/rotation/Event Log plan 테스트를 추가한다.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`: product manifest에 diagnostics policy가 포함되는지 검증한다.
- `packaging/windows-desktop-node/README.md`: Phase 16 diagnostics policy, rotation, Event Log opt-in 경계를 설명한다.
- `packaging/windows-desktop-node/installer/README.md`: MSI가 Event Log provider를 기본 등록하지 않는 이유와 opt-in smoke를 설명한다.
- `spikes/purecvisor-desktop-node/README.md`: Phase 16 상태와 제품 승격 gate를 갱신한다.
- `spikes/purecvisor-desktop-node/api/README.md`: JSONL event log가 Phase 16에서도 1차 로그로 유지됨을 명시한다.
- `spikes/purecvisor-desktop-node/service/README.md`: service log와 Event Log 보류 경계를 명시한다.
- `docs/DEVELOPER_INDEX.md`: Phase 16 spec/plan과 제품 wrapper README 진입점을 추가한다.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: Phase 16 검증 기준과 expected suite count를 갱신한다.
- `docs/PUBLIC_RELEASE_BOUNDARY.md`: Desktop Node Phase 16이 Single Edge 공개 표면과 분리됨을 갱신한다.
- `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`: Phase 16 상태를 기본 구현/검증 완료로 갱신한다.
- `AGENTS.md`: Phase 16 문서 진입점과 Desktop Node product wrapper 경계를 갱신한다.
- `follower.md`: 다음 실행 대기열을 Phase 17 이후로 이동한다.

## 작업 원칙

- 새 production helper는 실패 테스트를 먼저 추가하고 RED를 확인한 뒤 구현한다.
- Windows Event Log 실제 등록은 수행하지 않는다. 관리자 opt-in 계획 object와 문서만 추가한다.
- diagnostic bundle manifest는 host absolute path를 artifact file name으로만 참조한다.
- redaction된 bundle artifact에 raw token, protected token blob, token hash, source/product/data root 원문이 남지 않아야 한다.

---

### Task 1: Diagnostics Policy와 Bundle Manifest

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: diagnostic bundle manifest 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Diagnostics.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'writes a versioned diagnostics manifest with redacted policy and source artifacts' {
    $outRoot = Join-Path $TestDrive 'diagnostics-manifest'
    $productRoot = Join-Path $TestDrive 'DesktopNodeManifestDiag'
    $dataRoot = Join-Path $TestDrive 'data-manifest-diag'
    $plan = New-PcvDesktopNodeProductPlan `
        -Action CollectDiagnostics `
        -SourceRoot $script:RepoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot
    New-Item -ItemType Directory -Path $plan.data_root -Force | Out-Null
    Set-Content -LiteralPath $plan.paths.event_log -Value '{"Authorization":"Bearer manifest-secret","path":"'$dataRoot'"}' -Encoding UTF8

    $runner = {
        param([string]$FileName, [string[]]$Arguments)
        [ordered]@{ exit_code = 0; stdout = 'Stopped'; stderr = '' }
    }
    $runtimePolicy = {
        param($Plan)
        [ordered]@{ ok = $false; error = [ordered]@{ detail = $Plan.product_root } }
    }

    $bundle = New-PcvDesktopNodeDiagnosticBundle `
        -Plan $plan `
        -OutputRoot $outRoot `
        -InvokeProcess $runner `
        -CollectRuntimePolicy $runtimePolicy

    $manifestPath = Join-Path $bundle.path 'diagnostics-manifest.json'
    Test-Path -LiteralPath $manifestPath | Should -BeTrue
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $manifest.schema_version | Should -Be 1
    $manifest.decision | Should -Be 'DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred'
    $manifest.policy.mode | Should -Be 'jsonl-primary-eventlog-deferred'
    $manifest.policy.windows_event_log.enabled_by_default | Should -BeFalse
    @($manifest.sources.name) | Should -Contain 'summary'
    @($manifest.sources.name) | Should -Contain 'events'
    ($manifest | ConvertTo-Json -Depth 16) | Should -Not -Match 'manifest-secret'
    ($manifest | ConvertTo-Json -Depth 16) | Should -Not -Match ([regex]::Escape($productRoot))
    ($manifest | ConvertTo-Json -Depth 16) | Should -Not -Match ([regex]::Escape($dataRoot))
}
```

- [ ] **Step 2: product manifest policy 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Manifest.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'records diagnostics policy v1 in product-manifest.json' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeDiagPolicy'
    $dataRoot = Join-Path $TestDrive 'data-diag-policy'
    $manifest = New-PcvDesktopNodeProductManifest `
        -SourceRoot $script:RepoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -Version '0.16.0'

    $manifest.diagnostics.schema_version | Should -Be 1
    $manifest.diagnostics.mode | Should -Be 'jsonl-primary-eventlog-deferred'
    $manifest.diagnostics.event_log.path | Should -Be (Join-Path $dataRoot 'events.jsonl')
    $manifest.diagnostics.install_log.retained_files | Should -Be 5
    $manifest.diagnostics.service_logs.retained_files | Should -Be 10
    $manifest.diagnostics.windows_event_log.enabled_by_default | Should -BeFalse
}
```

- [ ] **Step 3: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

Expected: `diagnostics-manifest.json` 또는 `manifest.diagnostics`가 없어서 실패한다.

- [ ] **Step 4: diagnostics policy와 bundle manifest를 구현한다**

`PcvDesktopNodeProduct.psm1`에 `Get-PcvDesktopNodeDiagnosticsPolicy`, redacted policy writer, bundle source manifest builder를 추가한다. `New-PcvDesktopNodeProductManifest`는 `diagnostics` object를 포함하고, `New-PcvDesktopNodeDiagnosticBundle`은 마지막에 `diagnostics-manifest.json`을 쓴다.

- [ ] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

Expected: targeted diagnostics/manifest tests가 `Failed: 0`으로 통과한다.

---

### Task 2: File Log Rotation Policy

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: log rotation 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Diagnostics.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'rotates JSONL and service logs according to diagnostics policy' {
    $plan = New-PcvDesktopNodeProductPlan `
        -Action CollectDiagnostics `
        -SourceRoot $script:RepoRoot `
        -ProductRoot (Join-Path $TestDrive 'DesktopNodeRotate') `
        -DataRoot (Join-Path $TestDrive 'data-rotate')
    New-Item -ItemType Directory -Path $plan.data_root -Force | Out-Null
    New-Item -ItemType Directory -Path $plan.paths.service_logs_root -Force | Out-Null
    Set-Content -LiteralPath $plan.paths.event_log -Value ('e' * 32) -Encoding UTF8 -NoNewline
    Set-Content -LiteralPath "$($plan.paths.event_log).1" -Value 'older-event' -Encoding UTF8 -NoNewline
    Set-Content -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.wrapper.log') -Value ('s' * 32) -Encoding UTF8 -NoNewline

    $result = Invoke-PcvDesktopNodeLogRotation `
        -Plan $plan `
        -MaxFileBytes 16 `
        -RetainedFiles 2 `
        -ServiceMaxFileBytes 16 `
        -ServiceRetainedFiles 2

    $result.ok | Should -BeTrue
    Test-Path -LiteralPath $plan.paths.event_log | Should -BeFalse
    Test-Path -LiteralPath "$($plan.paths.event_log).1" | Should -BeTrue
    Test-Path -LiteralPath "$($plan.paths.event_log).2" | Should -BeTrue
    Test-Path -LiteralPath (Join-Path $plan.paths.service_logs_root 'PureCVisorDesktopNode.wrapper.log.1') | Should -BeTrue
    @($result.rotated | Where-Object { $_.name -eq 'event_log' }).Count | Should -Be 1
    @($result.rotated | Where-Object { $_.name -eq 'service_log' }).Count | Should -Be 1
}
```

- [ ] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: `Invoke-PcvDesktopNodeLogRotation` 함수가 없어서 실패한다.

- [ ] **Step 3: log rotation helper를 구현한다**

`Invoke-PcvDesktopNodeLogRotation`과 내부 `Rotate-PcvDesktopNodeLogFile` helper를 추가한다. helper는 보존 수를 초과한 `.N` 파일을 제거하고, 기존 `.N` 파일을 뒤에서 앞으로 이동한 뒤 현재 파일을 `.1`로 이동한다.

- [ ] **Step 4: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: diagnostics suite가 `Failed: 0`으로 통과한다.

---

### Task 3: Windows Event Log Registration Plan

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: Event Log plan 실패 테스트를 추가한다**

`PcvDesktopNodeProduct.Diagnostics.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'builds an admin opt-in Windows Event Log registration plan without mutating the host' {
    $plan = New-PcvDesktopNodeProductPlan `
        -Action Plan `
        -SourceRoot $script:RepoRoot `
        -ProductRoot (Join-Path $TestDrive 'DesktopNodeEventLog') `
        -DataRoot (Join-Path $TestDrive 'data-eventlog')

    $eventLogPlan = New-PcvDesktopNodeEventLogRegistrationPlan -Plan $plan

    $eventLogPlan.enabled_by_default | Should -BeFalse
    $eventLogPlan.registration_owner | Should -Be 'admin-opt-in'
    $eventLogPlan.log_name | Should -Be 'Application'
    $eventLogPlan.source | Should -Be 'PureCVisor Desktop Node'
    $eventLogPlan.commands.register.file_name | Should -Be 'powershell.exe'
    $eventLogPlan.commands.register.arguments -join ' ' | Should -Match 'New-EventLog'
    $eventLogPlan.commands.unregister.arguments -join ' ' | Should -Match 'Remove-EventLog'
}
```

- [ ] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: `New-PcvDesktopNodeEventLogRegistrationPlan` 함수가 없어서 실패한다.

- [ ] **Step 3: Event Log registration plan helper를 구현한다**

`New-PcvDesktopNodeEventLogRegistrationPlan`은 `Application` log와 `PureCVisor Desktop Node` source를 대상으로 하는 register/unregister command object를 반환한다. 이 helper는 command object만 만들고 실행하지 않는다.

- [ ] **Step 4: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: diagnostics suite가 `Failed: 0`으로 통과한다.

---

### Task 4: Documentation and Boundary Update

**Files:**

- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `spikes/purecvisor-desktop-node/service/README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `AGENTS.md`
- Modify: `follower.md`

- [ ] **Step 1: 문서를 Phase 16 상태로 갱신한다**

문서에 다음 문장을 반영한다.

```text
DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred
```

제품 wrapper README에는 JSONL first, rotation/retention 기본값, `diagnostics-manifest.json`, Event Log admin opt-in 경계를 기록한다. 검증 정책에는 Phase 16 packaging expected count를 실제 통과 결과에 맞춰 기록한다.

- [ ] **Step 2: stale 상태 문구를 제거한다**

다음 검색에서 Phase 16이 여전히 미작성/후속 후보로만 남지 않게 한다.

```powershell
rg -n "Phase 16 후보|Event Log provider.*후속|long-term diagnostics.*후속|장기 로그 보존.*후속" docs packaging/windows-desktop-node spikes/purecvisor-desktop-node AGENTS.md follower.md
```

- [ ] **Step 3: root boundary suite를 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: root boundary suite가 `Failed: 0`으로 통과한다.

---

### Task 5: Full Verification, Completion Evidence, Commit

**Files:**

- Modify: `docs/superpowers/plans/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics.md`

- [ ] **Step 1: 필수 검증을 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Expected: 모든 명령이 exit 0이고 Pester는 `Failed: 0`이다.

- [ ] **Step 2: dry-run smoke를 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
```

Expected: product action JSON이 exit 0으로 출력된다. 실제 Event Log 등록은 실행하지 않는다.

- [ ] **Step 3: 완료 증거를 이 문서에 기록한다**

`## 완료 증거` 섹션에 검증 명령, 결과, 관리자 smoke 보류 사유를 기록한다.

- [ ] **Step 4: 커밋하고 push한다**

Run:

```powershell
git status --short
git add packaging/windows-desktop-node docs spikes AGENTS.md follower.md
git commit -m "feat: add Desktop Node long-term diagnostics policy"
git push -u origin codex/desktop-node-phase16-diagnostics
```

Expected: commit이 생성되고 remote branch가 업데이트된다.

## 완료 증거

2026-04-28 기본 구현과 검증을 완료했다.

구현 요약:

- `PcvDesktopNodeProduct.psm1`에 diagnostics policy v1, product manifest diagnostics field, diagnostic bundle manifest, log rotation helper, Windows Event Log registration plan helper를 추가했다.
- `PcvDesktopNodeProduct.Diagnostics.Tests.ps1`에 versioned diagnostics manifest, JSONL/service log rotation, Event Log opt-in registration plan 검증을 추가했다.
- `PcvDesktopNodeProduct.Manifest.Tests.ps1`에 product manifest diagnostics policy 검증을 추가했다.
- README, 개발자 인덱스, 검증 정책, 공개 경계, Phase roadmap, AGENTS, follower 문서를 Phase 16 상태로 갱신했다.

검증 결과:

관리자 컨텍스트 확인:

```powershell
[Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent() |
    ForEach-Object { $_.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator) }
```

결과: `true`, 실행 identity는 `AMD_5800X\Operator`.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1' -Output Detailed"
```

결과: 18 passed, 0 failed.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

결과: 57 passed, 0 failed.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

결과: 10 passed, 0 failed.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

결과: 91 passed, 0 failed.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
```

결과: 17 passed, 0 failed.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

결과: 17 passed, 0 failed.

```powershell
$env:PCV_HYPERV_INTEGRATION='1'
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -Output Detailed"
```

결과: 42 passed, 0 failed, 0 skipped, 0 NotRun. `PCV_HYPERV_TEST_ISO`는 `D:\Downloads\Rocky-10.1-x86_64-minimal.iso`를 사용했다. 통합 테스트 후 남은 `pcv-spike-*` VM과 `%TEMP%\pcv-hyperv-spike` 하위 테스트 디렉터리는 없었다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
```

결과: CLI suite 12 passed, 0 failed. Web suite 11 passed, 0 failed.

```powershell
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

결과: 두 명령 모두 exit 0. `git diff --check`는 line-ending 변환 경고만 출력했다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
```

결과: 두 명령 모두 exit 0 JSON을 출력했다. `Plan`과 `CollectDiagnostics -WhatIf` 결과에 diagnostics policy v1과 `windows_event_log.enabled_by_default = false`가 포함됐다.

관리자 smoke 결과:

```powershell
Import-Module packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 -Force
$plan = New-PcvDesktopNodeProductPlan -Action Plan
$eventPlan = New-PcvDesktopNodeEventLogRegistrationPlan -Plan $plan
```

`PureCVisor Desktop Node` Event Log source는 smoke 전에는 존재하지 않았다. 관리자 smoke에서 registration plan의 register command를 실행해 `Application` log source 존재를 확인했고, 같은 plan의 unregister command를 실행해 source가 제거됐음을 확인했다.

결과 JSON:

```json
{"ok":true,"source":"PureCVisor Desktop Node","log_name":"Application","pre_existing":false,"created_by_smoke":true,"exists_after_register":true,"exists_after_cleanup":false}
```

남은 관리자 smoke 보류:

- 장기 운영 log inspection과 실제 service install/start smoke는 운영 환경이 준비된 경우 별도 opt-in으로 기록한다.
