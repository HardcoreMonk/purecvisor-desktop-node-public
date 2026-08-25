# PureCVisor Desktop Node Phase 15 Secure Token Storage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node 제품 wrapper의 기본 bearer token source를 plain text token file에서 DPAPI LocalMachine protected token file로 승격한다.

**Architecture:** `spikes/purecvisor-desktop-node/service/`가 token 생성, DPAPI protect/unprotect, ACL, prepare/rotate/revoke command surface를 소유한다. Local API와 CLI는 protected token file을 읽어 bearer auth에 사용하고, `packaging/windows-desktop-node/` product wrapper는 service host command line, product plan, health check, diagnostics, RemoveData 경로를 protected token source 중심으로 바꾼다. 이 plan 작성 당시에는 WinSW XML이 service command owner였고, 2026-05-01 replacement slice 이후 기본 service host command owner는 `DesktopNode.Host.exe listen --api-token-protected-file ...`다.

**Tech Stack:** PowerShell 7, Pester 5, Windows DPAPI via `System.Security.Cryptography.ProtectedData`, service host command metadata, JSON protected token metadata.

---

## 설계 기준

- 설계 문서: `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage-design.md`
- 결정 토큰: `DESKTOP_NODE_PHASE15_TOKEN_STORAGE_DECISION: dpapi-local-machine-protected-file-first`
- 유지 결정: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 경계: `spikes/purecvisor-desktop-node/**`와 `packaging/windows-desktop-node/**` 바깥의 Linux `purecvisorsd`, libvirt/KVM, Single Edge 공개 UI/API 표면은 변경하지 않는다.

## 파일 구조

수정:

- `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`: DPAPI protected token read/write, default protected path, prepare/rotate/revoke helpers, service config `-ApiTokenProtectedFile`.
- `spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1`: protected token helper actions와 listener parameter pass-through.
- `spikes/purecvisor-desktop-node/service/tests/PcvDesktopService.Contract.Tests.ps1`: protected token file, conflict, command surface, binary path 검증.
- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`: `-ApiTokenProtectedFile` resolution과 dynamic runtime policy token storage.
- `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`: protected token parameter pass-through.
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1`: protected token auth, source conflict, runtime policy metadata 검증.
- `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`: `--protected-token-file` parsing과 protected token read.
- `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`: CLI protected token file support와 conflict 검증.
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`: product defaults/path/manifest/WinSW XML/health/diagnostics/RemoveData를 protected token source로 전환.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`: product plan auth source, WinSW XML, delete path 검증.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`: protected token prepare, legacy migration, health check 검증.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`: protected token metadata redaction 검증.
- `packaging/windows-desktop-node/README.md`, `packaging/windows-desktop-node/installer/README.md`, `spikes/purecvisor-desktop-node/api/README.md`, `spikes/purecvisor-desktop-node/cli/README.md`, `spikes/purecvisor-desktop-node/service/README.md`: Phase 15 사용법과 검증 절차.
- `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`, `follower.md`: Phase 상태와 검증 기준 현행화.

## 작업 원칙

- 각 behavior는 테스트를 먼저 추가하고 실패를 확인한 뒤 구현한다.
- protected token prepare 결과는 raw token 값을 반환하지 않는다.
- API/CLI의 protected token read 함수만 bearer token 값을 memory로 반환한다.
- WinSW XML, product manifest, diagnostics bundle에는 raw token 값, protected blob, token hash가 남지 않는다.
- 기존 `-ApiTokenFile`과 `--token-file`은 호환 경로로 유지하되 product wrapper 기본값에서는 사용하지 않는다.

---

### Task 1: Service DPAPI Token Store

**Files:**

- Modify: `spikes/purecvisor-desktop-node/service/tests/PcvDesktopService.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`
- Modify: `spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1`

- [x] **Step 1: protected token file 실패 테스트를 추가한다**

`PcvDesktopService.Contract.Tests.ps1`에 다음 behavior를 추가한다.

```powershell
It 'writes and reads a DPAPI LocalMachine protected token file without returning the token from prepare' {
    $path = Join-Path $TestDrive 'api-token.dpapi.json'
    $script:AclCalls = @()
    $runner = {
        param([string]$FileName, [string[]]$Arguments)
        $script:AclCalls += [pscustomobject]@{ file_name = $FileName; arguments = $Arguments }
        [ordered]@{ exit_code = 0; stdout = 'processed'; stderr = '' }
    }

    $created = New-PcvDesktopServiceProtectedTokenFile -Path $path -Token 'protected-secret' -InvokeProcess $runner
    $json = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    $read = Read-PcvDesktopServiceProtectedTokenFile -Path $path

    $created.ok | Should -BeTrue
    @($created.Keys) | Should -Not -Contain 'token'
    $json.storage | Should -Be 'dpapi-local-machine'
    $json.scope | Should -Be 'LocalMachine'
    $json.protected_token | Should -Not -BeNullOrEmpty
    $json.protected_token | Should -Not -Match 'protected-secret'
    $json.token_sha256 | Should -Match '^[a-f0-9]{64}$'
    $read.token | Should -Be 'protected-secret'
    $read.storage | Should -Be 'dpapi-local-machine'
    $script:AclCalls.Count | Should -Be 2
}
```

- [x] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests/PcvDesktopService.Contract.Tests.ps1' -Output Detailed"
```

Expected: `New-PcvDesktopServiceProtectedTokenFile` 함수가 없어서 실패한다.

- [x] **Step 3: service module protected token helper를 구현한다**

`PcvDesktopService.psm1`에 기본 protected path, DPAPI protect/unprotect, read/write/remove helper를 추가한다.

- [x] **Step 4: service entrypoint에 protected token action을 연결한다**

`Invoke-PcvDesktopService.ps1`의 `Action` validate set에 `PrepareProtectedTokenFile`, `RotateProtectedTokenFile`, `RevokeProtectedTokenFile`을 추가하고 `-ApiTokenProtectedFile`을 listener config와 helper action에 전달한다.

- [x] **Step 5: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests/PcvDesktopService.Contract.Tests.ps1' -Output Detailed"
```

Expected: service contract suite가 `Failed: 0`으로 통과한다.

---

### Task 2: Local API Protected Token Source

**Files:**

- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- Modify: `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`

- [x] **Step 1: protected token auth 실패 테스트를 추가한다**

`PcvDesktopApi.Auth.Tests.ps1`에 protected token file을 생성하고 `Resolve-PcvApiToken -ApiTokenProtectedFile`로 읽는 테스트를 추가한다.

- [x] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1' -Output Detailed"
```

Expected: `ApiTokenProtectedFile` parameter가 없어서 실패한다.

- [x] **Step 3: API resolver와 runtime policy를 구현한다**

`Resolve-PcvApiToken`이 inline/plain/protected source 충돌을 검사하고, protected source는 service module의 `Read-PcvDesktopServiceProtectedTokenFile`로 읽도록 구현한다. `Get-PcvApiRuntimePolicy`와 `Invoke-PcvApiRequest`는 token storage metadata를 전달받아 `dpapi-local-machine`을 노출한다.

- [x] **Step 4: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Auth.Tests.ps1' -Output Detailed"
```

Expected: auth suite가 `Failed: 0`으로 통과한다.

---

### Task 3: Product Wrapper Default Protected Token

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [x] **Step 1: product plan 실패 테스트를 갱신한다**

기존 product plan 테스트의 expected token path를 `api-token.dpapi.json`과 `api_token_source = protected_file`로 바꾸고, WinSW XML이 `-ApiTokenProtectedFile`을 포함하며 `-ApiTokenFile`을 포함하지 않는지 검증한다.

- [x] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected: 현재 product plan은 `api-token.txt`와 `-ApiTokenFile`을 사용하므로 실패한다.

- [x] **Step 3: product wrapper 기본 token source를 protected file로 전환한다**

defaults, resolved paths, service arguments, product manifest, default token preparation, health check, RemoveData delete paths를 protected token file 중심으로 변경한다. legacy `api-token.txt`는 migration/read compatibility와 RemoveData 삭제 대상으로 유지한다.

- [x] **Step 4: product invoke와 diagnostics 테스트를 통과시킨다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: packaging suite가 `Failed: 0`으로 통과한다.

---

### Task 4: CLI Protected Token File

**Files:**

- Modify: `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`

- [x] **Step 1: CLI protected token file 실패 테스트를 추가한다**

`--protected-token-file`이 DPAPI protected token file을 읽고 transport에 bearer token을 전달하는 테스트와 `--token`, `--token-file`, `--protected-token-file` conflict 테스트를 추가한다.

- [x] **Step 2: RED를 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1' -Output Detailed"
```

Expected: CLI parser가 `--protected-token-file`을 모르는 상태로 실패한다.

- [x] **Step 3: CLI parser와 help text를 구현한다**

CLI는 sibling service module을 import해 `Read-PcvDesktopServiceProtectedTokenFile`로 protected token을 읽는다.

- [x] **Step 4: GREEN을 확인한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1' -Output Detailed"
```

Expected: CLI suite가 `Failed: 0`으로 통과한다.

---

### Task 5: Documentation and Verification Policy

**Files:**

- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `spikes/purecvisor-desktop-node/cli/README.md`
- Modify: `spikes/purecvisor-desktop-node/service/README.md`
- Modify: `follower.md`

- [x] **Step 1: 문서를 protected token storage 기준으로 갱신한다**

Phase 15 spec/plan 링크, `api-token.dpapi.json`, `-ApiTokenProtectedFile`, `--protected-token-file`, Prepare/Rotate/Revoke helper, RemoveData 삭제 대상, 관리자 smoke 기준을 문서에 반영한다.

- [x] **Step 2: 전체 검증을 실행한다**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

Expected: 모든 명령이 exit code 0으로 완료된다.

## 완료 증거

2026-04-28 기준 기본 검증:

- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -PassThru`: 17 passed, 0 failed
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -PassThru`: 91 passed, 0 failed
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -PassThru`: 12 passed, 0 failed
- `Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -PassThru`: 53 passed, 0 failed
- `Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -PassThru`: 17 passed, 0 failed
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed`: 10 passed, 0 failed
- `Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -PassThru`: 11 passed, 0 failed
- `node --check spikes/purecvisor-desktop-node/web/app.js`: exit 0
- `git diff --check`: exit 0

Dry-run smoke:

- `Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile -WhatIf`: exit 0, `storage = dpapi-local-machine`, `api-token.dpapi.json` ACL command preview
- `Invoke-PcvDesktopService.ps1 -Action Config`: exit 0
- `Invoke-PcvDesktopService.ps1 -Action Install -WhatIf`: exit 0
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Plan`: exit 0, `api_token_source = protected_file`, `api_token_storage = dpapi-local-machine`
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WinSwPath 'C:\Windows\System32\notepad.exe' -WhatIf`: exit 0, WinSW XML includes `-ApiTokenProtectedFile`
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall -RemoveData -WhatIf`: exit 0, delete paths include `api-token.dpapi.json` and legacy `api-token.txt`
- `Invoke-PcvDesktopCli.ps1 --help`: exit 0, usage includes `--protected-token-file`

실제 service install/start, protected token ACL inspection, signed release build, elevated `msiexec` install/repair/uninstall smoke는 관리자 권한과 signing/runtime 환경이 준비된 경우에만 별도 완료 증거로 추가한다.
