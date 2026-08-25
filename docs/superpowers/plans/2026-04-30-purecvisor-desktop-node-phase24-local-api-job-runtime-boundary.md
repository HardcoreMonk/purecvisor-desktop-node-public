# PureCVisor Desktop Node Phase 24 Local API Job Runtime Boundary 구현 계획

> **Agent 작업자 필수 지침:** 이 계획을 단계별로 구현할 때는 `superpowers:subagent-driven-development`(권장) 또는 `superpowers:executing-plans`를 사용한다. 진행 상태는 checkbox(`- [ ]`) 문법으로 추적한다.

**목표:** Local API job runtime의 public boundary를 안정화해 PowerShell 유지와 C++23 전환 판단을 observable behavior 기준으로 분리한다.

**구조:** Phase 24는 `spikes/purecvisor-desktop-node/api/**` 안에서 job state, persistence, dispatch, host mutation boundary를 먼저 공개 runtime policy로 고정한다. 실제 Hyper-V mutation은 계속 helper process 뒤에 두고, C++23 native core 구현은 public contract가 안정화될 때까지 보류한다.

**기술 기준:** PowerShell 7, Pester 5, Windows `HttpListener`, JSON job store, Hyper-V helper process boundary.

---

## 상태

- 작성 기준: 2026-04-30
- 현재 상태: Phase 24 후보 문서화, runtime policy contract slice, read-only network inventory slice, running job recovery slice, persistence schema compatibility slice, retry/cancel semantics slice, diagnostics bundle self-audit slice, CLI runtime policy consumer slice 완료
- 관련 설계: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md`
- 제품 승격 판단: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- Phase 24 후보 마커: `DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first`
- Phase 25 이후 현재 delta: 2026-05-02 `src/DesktopNode.Api/**`에서 `host.status` read route가 C# registry/WMI/service/admin native adapter로 전환됐고, `network.inventory` read route가 C# native WMI adapter를 먼저 시도하고 topology parity가 불완전하면 PowerShell helper로 fallback하는 경로로 전환됐다. 2026-05-03 후속 slice에서 `vm.list`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints`도 C# native-first read path를 먼저 시도하고 VM identity/state, summary field, checkpoint list parity가 불완전하면 PowerShell helper로 fallback한다. Phase 24 PowerShell helper contract와 historical test snippets는 원래 baseline으로 유지하며, 현재 runtime policy는 `dotnet-native-read-plus-hyperv-helper-process` hybrid boundary와 `native_core.reason=host.status,network.inventory,vm.list,checkpoint.list`를 보고한다.
- 2026-05-05 현행화: 이 계획의 `unsupported_future_version = quarantine-and-start-empty` 및 `.unsupported.<version>` quarantine task는 Phase 24 당시 component baseline이다. 현재 .NET 제품 경로는 unsupported future job-store schema를 quarantine/move 없이 blocked diagnostics/no-mutation으로 처리한다. 2026-05-06 후속에서는 schema v2를 `job-store-v1-to-v2` migration target으로 지원하고 `job-store-migration-apply` code-level actual path를 추가했다. Corrupt JSON quarantine과 Phase 24 baseline snippets는 과거 component 기록으로 보존한다.

## 파일 구조

- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - `Get-PcvApiRuntimePolicy`가 `job_runtime` contract를 반환한다.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
  - `GET /api/v1/runtime/policy`가 Phase 24 job runtime boundary를 노출하는지 검증한다.
  - `GET /api/v1/network/inventory`가 Hyper-V helper `network.inventory` operation으로 라우팅되는지 검증한다.
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Persistence.Tests.ps1`
  - Persisted `running` job을 restart 이후 interrupted failed job으로 복구하고 자동 재큐잉하지 않는지 검증한다.
- Modify: `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`
  - `network.inventory` read-only helper operation을 제공한다.
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.NetworkInventory.Tests.ps1`
  - Hyper-V switch inventory contract와 runner dispatch를 검증한다.
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
  - runtime policy의 job runtime boundary 의미를 문서화한다.
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
  - diagnostic bundle self-audit artifact와 manifest 요약을 생성한다.
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
  - Phase 24 runtime policy self-audit artifact와 manifest contract를 검증한다.
- Modify: `packaging/windows-desktop-node/README.md`
  - diagnostic bundle self-audit artifact를 문서화한다.
- Modify: `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`
  - `runtime policy` CLI 명령을 `GET /api/v1/runtime/policy`로 라우팅한다.
- Modify: `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`
  - CLI가 runtime policy public contract endpoint를 thin client로 조회하는지 검증한다.
- Modify: `spikes/purecvisor-desktop-node/cli/README.md`
  - `runtime policy` CLI 사용 예를 문서화한다.
- Modify: `spikes/purecvisor-desktop-node/hyperv/README.md`
  - `network.inventory` helper operation을 문서화한다.
- Modify: `spikes/purecvisor-desktop-node/README.md`
  - Phase 24 후보가 제품 승격이 아니라 Local API 경계 안정화임을 요약한다.
- Modify: `docs/DEVELOPER_INDEX.md`
  - Phase 24 설계/계획 문서 진입점을 추가한다.
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
  - Phase 24 Local API job runtime 변경 검증 기준을 추가한다.
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
  - Phase 24가 공개 출시/GA 경계를 바꾸지 않음을 기록한다.
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
  - Phase 24 후보 row를 추가한다.
- Modify: `follower.md`
  - 다음 Phase 후보를 Phase 24로 올리고 기존 GA evidence gate는 유지한다.

## Task 1: Runtime policy job boundary TDD slice

**Files:**

- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [x] **Step 1: Write the failing test**

Add one behavior test to `PcvDesktopApi.Contract.Tests.ps1`.

```powershell
It 'exposes the Phase 24 Local API job runtime boundary contract' {
    $response = Invoke-PcvApiRequest `
        -Method 'GET' `
        -Path '/api/v1/runtime/policy' `
        -HelperScriptPath 'D:\fake\Invoke-PcvHyperV.ps1' `
        -InvokeHelper $script:Helper

    $response.status | Should -Be 200
    $json = $response.body | ConvertFrom-Json
    $json.operation | Should -Be 'runtime.policy'
    $json.data.job_runtime.contract_version | Should -Be 1
    $json.data.job_runtime.owner | Should -Be 'local-api'
    $json.data.job_runtime.state_store.backend | Should -Be 'script-scope-memory'
    $json.data.job_runtime.state_store.persistence | Should -Be 'json-file-snapshot'
    $json.data.job_runtime.state_store.corrupt_store | Should -Be 'quarantine-and-start-empty'
    $json.data.job_runtime.state_store.unsupported_future_version | Should -Be 'quarantine-and-start-empty'
    $json.data.job_runtime.dispatch.mode | Should -Be 'bounded-synchronous-worker-tick'
    $json.data.job_runtime.dispatch.helper_boundary | Should -Be 'hyperv-helper-process'
    $json.data.job_runtime.host_mutation | Should -Be 'helper-process-only'
    $json.data.job_runtime.orchestration.primary | Should -Be 'powershell'
    $json.data.job_runtime.orchestration.contract | Should -Be 'plan-contract-injectable-runner-diagnostics'
    $json.data.job_runtime.native_core.status | Should -Be 'not-planned-unless-runtime-boundary-deepens'
    $json.data.job_runtime.native_core.reason | Should -Be 'windows-hyperv-orchestration-not-dataplane'
    $json.data.job_runtime.native_core.revisit_when | Should -Be 'state-machine-or-supervision-outgrows-powershell'
    $script:HelperCalls.Count | Should -Be 0
}
```

- [x] **Step 2: Run test to verify it fails**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1' -Output Detailed"
```

Observed result:

```text
Failed: exposes the Phase 24 Local API job runtime boundary contract
Expected 1, but got $null.
```

- [x] **Step 3: Write minimal implementation**

Add `job_runtime` to `Get-PcvApiRuntimePolicy`.

```powershell
job_runtime = [ordered]@{
    contract_version = 1
    owner = 'local-api'
    state_store = [ordered]@{
        backend = 'script-scope-memory'
        persistence = 'json-file-snapshot'
        corrupt_store = 'quarantine-and-start-empty'
        unsupported_future_version = 'quarantine-and-start-empty'
    }
    dispatch = [ordered]@{
        mode = 'bounded-synchronous-worker-tick'
        helper_boundary = 'hyperv-helper-process'
    }
    host_mutation = 'helper-process-only'
    orchestration = [ordered]@{
        primary = 'powershell'
        contract = 'plan-contract-injectable-runner-diagnostics'
    }
    native_core = [ordered]@{
        status = 'not-planned-unless-runtime-boundary-deepens'
        reason = 'windows-hyperv-orchestration-not-dataplane'
        revisit_when = 'state-machine-or-supervision-outgrows-powershell'
    }
}
```

- [x] **Step 4: Run test to verify it passes**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1' -Output Detailed"
```

Observed result:

```text
Tests Passed: 18, Failed: 0
```

## Task 1B: Read-only network inventory TDD slice

**Files:**

- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- Modify: `spikes/purecvisor-desktop-node/hyperv/PcvHyperV.psm1`
- Create: `spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.NetworkInventory.Tests.ps1`

- [x] **Step 1: Write failing tests**

Add Pester coverage for two observable contracts:

- Hyper-V helper maps `Get-VMSwitch` into read-only `network.inventory` data and returns structured `PCV_NETWORK_INVENTORY_FAILED` on inventory failure.
- Local API routes `GET /api/v1/network/inventory` to helper operation `network.inventory`.

- [x] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.NetworkInventory.Tests.ps1' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1' -FullName '*network.inventory*' -Output Detailed"
```

Observed result:

```text
Get-PcvNetworkInventory is not recognized.
Expected 'PCV_NETWORK_INVENTORY_FAILED', but got 'PCV_OPERATION_NOT_ALLOWED'.
Expected 200, but got 404.
```

- [x] **Step 3: Write minimal implementation**

Add `Get-PcvNetworkInventory`, allowlist `network.inventory`, and route `GET /api/v1/network/inventory`.

Contract:

```json
{
  "source": "hyperv",
  "mutating": false,
  "switches": [
    {
      "name": "Default Switch",
      "type": "internal",
      "is_default": true,
      "allow_management_os": true,
      "net_adapter_interface_description": null
    }
  ]
}
```

- [x] **Step 4: Run GREEN verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests/PcvHyperV.NetworkInventory.Tests.ps1' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1' -FullName '*network.inventory*' -Output Detailed"
```

Observed result:

```text
Hyper-V network inventory tests: 3 passed, 0 failed
API network.inventory route test: 1 passed, 0 failed
```

## Task 1C: Running job recovery TDD slice

**Files:**

- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Persistence.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [x] **Step 1: Write failing persistence recovery test**

Add one behavior test: when `Initialize-PcvApiJobStore -Path <jobs.json>` loads a persisted job with `status = 'running'`, the job should become `failed` with `PCV_JOB_INTERRUPTED`, `retryable = true`, `result = $null`, and should not be re-enqueued.

- [x] **Step 2: Run RED verification**

Observed result:

```text
Expected 'failed', but got 'running'.
```

- [x] **Step 3: Write minimal recovery implementation**

Normalize loaded `running` jobs inside `Initialize-PcvApiJobStore` to interrupted retryable failures before rebuilding the queue.

- [x] **Step 4: Run GREEN verification**

Observed result:

```text
PcvDesktopApi persisted job store: 6 passed, 0 failed
```

## Task 1D: Persistence schema compatibility TDD slice

**Files:**

- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Persistence.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [x] **Step 1: Write failing future-version quarantine test**

Add one behavior test: when `Initialize-PcvApiJobStore -Path <jobs.json>` loads a store with `version` greater than the supported runtime version, the Local API should not load jobs from that file. It should move the file to an `.unsupported.<version>.<timestamp>` quarantine path, return `PCV_JOB_STORE_UNSUPPORTED_VERSION`, and leave the in-memory job store empty.

- [x] **Step 2: Run RED verification**

Observed result:

```text
Expected $false, but got $true.
Expected 'quarantine-and-start-empty', but got $null.
```

- [x] **Step 3: Write minimal compatibility implementation**

Add a `version` check after JSON parsing and before job normalization in `Initialize-PcvApiJobStore`. Treat missing version as v1-compatible, support `version <= 1`, and quarantine future versions with non-retryable `PCV_JOB_STORE_UNSUPPORTED_VERSION`. Expose the policy as `job_runtime.state_store.unsupported_future_version = quarantine-and-start-empty`.

- [x] **Step 4: Run GREEN verification**

Observed result:

```text
PcvDesktopApi persisted job store: 7 passed, 0 failed
Phase 24 runtime policy contract narrow suite: 1 passed, 0 failed
```

## Task 1E: Retry/cancel semantics TDD slice

**Files:**

- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.JobControl.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`

- [x] **Step 1: Write failing retryability and runtime policy tests**

Add one behavior test: when `POST /api/v1/jobs/{job_id}/retry` targets a `failed` job whose `error.retryable = false`, Local API should return `409 PCV_JOB_NOT_RETRYABLE` and should not enqueue a retry job. Extend the Phase 24 runtime policy contract test so `job_runtime.control.cancel` and `job_runtime.control.retry` expose queued-only cancel, no running interruption, manual-only retry, retryable-error-only retry, max attempts, and creates-new-job semantics.

- [x] **Step 2: Run RED verification**

Observed result:

```text
Expected 409, but got 202.
Expected $true, but got $null.
```

- [x] **Step 3: Write minimal retry/cancel policy implementation**

Update `Retry-PcvApiJob` so failed jobs with missing or false `error.retryable` return `409 PCV_JOB_NOT_RETRYABLE`. Add `job_runtime.control` to `Get-PcvApiRuntimePolicy`.

- [x] **Step 4: Run GREEN verification**

Observed result:

```text
Non-retryable failed job retry narrow suite: 1 passed, 0 failed
Phase 24 runtime policy contract narrow suite: 1 passed, 0 failed
```

## Task 1F: Diagnostics bundle self-audit TDD slice

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Modify: `packaging/windows-desktop-node/README.md`

- [x] **Step 1: Write failing self-audit artifact test**

Add one behavior test: when `New-PcvDesktopNodeDiagnosticBundle` collects a runtime policy body containing Phase 24 `job_runtime`, the bundle should write `diagnostics-self-audit.json`, include `diagnostics_self_audit` in `diagnostics-manifest.json` sources, and expose `self_audit.runtime_policy.job_runtime.contract_ok = true` in the manifest.

- [x] **Step 2: Run RED verification**

Observed result:

```text
Cannot find path ... diagnostics-self-audit.json because it does not exist.
Expected 1, but got $null.
```

- [x] **Step 3: Write minimal self-audit implementation**

Add a diagnostic self-audit helper that parses the collected runtime policy body, checks the Phase 24 `job_runtime` contract summary, writes `diagnostics-self-audit.json`, and copies the self-audit summary into `diagnostics-manifest.json`.

- [x] **Step 4: Run GREEN verification**

Observed result:

```text
Phase 24 diagnostics self-audit narrow suite: 1 passed, 0 failed
```

## Task 1G: CLI runtime policy consumer TDD slice

**Files:**

- Modify: `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`
- Modify: `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`
- Modify: `spikes/purecvisor-desktop-node/cli/README.md`

- [x] **Step 1: Write failing CLI route test**

Add one behavior test: `pcv --json runtime policy` should call `GET /api/v1/runtime/policy` through the CLI transport and should not invoke helper/process mutation paths directly.

- [x] **Step 2: Run RED verification**

Observed result:

```text
Expected 0, but got 2.
```

- [x] **Step 3: Write minimal CLI route implementation**

Add `runtime policy` to CLI usage and route it to `GET /api/v1/runtime/policy`.

- [x] **Step 4: Run GREEN verification**

Observed result:

```text
PcvDesktopCli contract: 13 passed, 0 failed
```

## Task 2: Documentation synchronization

**Files:**

- Create: `docs/superpowers/specs/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary-design.md`
- Create: `docs/superpowers/plans/2026-04-30-purecvisor-desktop-node-phase24-local-api-job-runtime-boundary.md`
- Modify: `AGENTS.md`
- Modify: `README.md`
- Modify: `docs/GUIDE.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Modify: `spikes/purecvisor-desktop-node/README.md`
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `spikes/purecvisor-desktop-node/hyperv/README.md`
- Modify: `follower.md`

- [x] **Step 1: Add Phase 24 spec and this plan**

Use the design and plan files listed above. Keep documents Korean-first, and do not add a new ADR for the candidate phase.

- [x] **Step 2: Update high-level entry points**

Add Phase 24 links to `AGENTS.md`, `README.md`, `docs/GUIDE.md`, and `docs/DEVELOPER_INDEX.md`.

- [x] **Step 3: Update policy and roadmap**

Add the Phase 24 verification row to `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, add the Phase 24 status note to `docs/PUBLIC_RELEASE_BOUNDARY.md`, and add the Phase 24 candidate row to the roadmap.

- [x] **Step 4: Update component docs**

Update `spikes/purecvisor-desktop-node/README.md`, `spikes/purecvisor-desktop-node/api/README.md`, and `spikes/purecvisor-desktop-node/hyperv/README.md` so the new `job_runtime` runtime policy object and `network.inventory` read-only helper route are documented where operators and contributors look first.

## Task 3: Verification

**Files:**

- Test: `spikes/purecvisor-desktop-node/api/tests/**`
- Test: `spikes/purecvisor-desktop-node/hyperv/tests/**`
- Test: `spikes/purecvisor-desktop-node/tests/**`

- [x] **Step 1: Run the API suite**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
```

Expected: Local API contract, auth, job, job control, persistence, static, worker, worker pool tests pass.

- [x] **Step 2: Run the Hyper-V non-integration suite**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

Expected: Hyper-V helper contract, host status, inventory, network inventory, lifecycle/checkpoint, and provisioning tests pass without real VM mutation.

- [x] **Step 3: Run the root documentation suite**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: Desktop Node boundary and documentation synchronization tests pass.

- [x] **Step 4: Run affected existing product wrapper suite**

The working tree already contains product wrapper changes outside Phase 24. Keep them intact and run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: Product wrapper tests pass.

- [x] **Step 5: Run whitespace check**

```powershell
git diff --check
```

Expected: exit 0. CRLF warnings from Git are acceptable if there are no whitespace errors.

## 완료 증거

Phase 24 첫 TDD slice:

- RED: `runtime.policy`에 `job_runtime`이 없어 `Expected 1, but got $null`로 실패했다.
- GREEN: `Get-PcvApiRuntimePolicy`에 `job_runtime` contract를 추가한 뒤 단일 contract suite가 통과했다.

Phase 24 read-only network inventory slice:

- RED: `Get-PcvNetworkInventory` 미정의, `network.inventory` allowlist 부재, API route 404를 확인했다.
- GREEN: Hyper-V helper `network.inventory`와 Local API `GET /api/v1/network/inventory` route를 추가한 뒤 narrow suite가 통과했다.
- Host mutation: 없음. `Get-VMSwitch` read-only inventory만 사용한다.

Phase 24 running job recovery slice:

- RED: persisted `running` job load 후 `Expected 'failed', but got 'running'`로 실패했다.
- GREEN: `Initialize-PcvApiJobStore`가 loaded `running` job을 `PCV_JOB_INTERRUPTED` retryable failure로 복구하고 queue에 다시 넣지 않도록 수정한 뒤 persistence suite가 통과했다.
- Host mutation: 없음. Pester `$TestDrive`의 JSON job store만 읽고 썼다.

Phase 24 persistence schema compatibility slice:

- RED: future `version = 999` job store를 loader가 조용히 로드해 `Expected $false, but got $true`로 실패했다.
- RED: runtime policy가 `job_runtime.state_store.unsupported_future_version`을 아직 노출하지 않아 `Expected 'quarantine-and-start-empty', but got $null`로 실패했다.
- GREEN: `Initialize-PcvApiJobStore`가 supported version보다 큰 store를 `.unsupported.<version>.<timestamp>`로 quarantine하고 `PCV_JOB_STORE_UNSUPPORTED_VERSION`을 반환하도록 수정했으며, runtime policy가 `unsupported_future_version`을 노출하도록 갱신한 뒤 persistence/contract narrow suite가 통과했다.
- Host mutation: 없음. Pester `$TestDrive`의 JSON job store만 읽고 이동했다.

Phase 24 retry/cancel semantics slice:

- RED: failed job의 `error.retryable = false`에도 retry route가 `202`를 반환해 실패했다.
- RED: runtime policy가 `job_runtime.control`을 아직 노출하지 않아 `Expected $true, but got $null`로 실패했다.
- GREEN: `Retry-PcvApiJob`이 non-retryable failed job을 `PCV_JOB_NOT_RETRYABLE`로 거부하고, runtime policy가 cancel/retry semantics를 노출하도록 갱신한 뒤 narrow suite가 통과했다.
- Host mutation: 없음. In-memory job store와 route handler만 사용했다.

Phase 24 diagnostics bundle self-audit slice:

- RED: diagnostic bundle에 `diagnostics-self-audit.json` artifact가 없어 `Cannot find path ... diagnostics-self-audit.json`와 `Expected 1, but got $null`로 실패했다.
- GREEN: `New-PcvDesktopNodeDiagnosticBundle`이 runtime policy body를 self-audit하고, `diagnostics-self-audit.json`과 manifest `self_audit`/`diagnostics_self_audit` source를 기록하도록 수정한 뒤 narrow suite가 통과했다.
- Host mutation: 없음. Pester `$TestDrive` diagnostic bundle 파일만 생성했다.

Phase 24 CLI runtime policy consumer slice:

- RED: CLI가 `runtime policy` 명령을 알지 못해 exit code `2`로 실패했다.
- GREEN: `runtime policy` 명령을 `GET /api/v1/runtime/policy`로 라우팅하고 CLI README 예시를 추가한 뒤 CLI contract suite가 통과했다.
- Host mutation: 없음. CLI transport spy만 사용했다.

문서 동기화:

- Phase 24 spec/plan을 추가했다.
- AGENTS, README, GUIDE, DEVELOPER_INDEX, DEVELOPMENT_VERIFICATION_POLICY, PUBLIC_RELEASE_BOUNDARY, roadmap, root/API/Hyper-V README, follower를 Phase 24 후보에 맞춰 갱신했다.
- ADR_INDEX에는 Phase 24를 ADR 미채택 후보 진입점으로만 연결했다.

남은 작업:

- Phase 24 후보의 현재 planned TDD slices와 CLI/diagnostics consumer 사용성 slice는 완료됐다. 다음 작업은 Phase 20/21/23 관리자 opt-in evidence 또는 release/version stable gate 중 하나를 별도 계획으로 선택한다.
- C++23 core 후보는 state machine 또는 supervision 문제가 PowerShell orchestration 경계를 넘어설 때만 재평가

검증:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 87 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 26 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"`: 97 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"`: 20 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"`: 13 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 14 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"`: 11 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"`: 44 passed, 0 failed, 1 not run
- `node --check spikes/purecvisor-desktop-node/web/app.js`: exit 0
- `git diff --check`: exit 0, CRLF warning only
