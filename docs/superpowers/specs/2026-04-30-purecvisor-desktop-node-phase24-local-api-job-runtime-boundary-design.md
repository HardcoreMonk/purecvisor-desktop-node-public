# PureCVisor Desktop Node Phase 24 Local API Job Runtime Boundary 설계

## 목적

Phase 24 후보는 Local API job runtime의 공개 경계를 먼저 고정한다. 목표는 PowerShell 구현을 바로 C++23으로 옮기는 것이 아니라, Windows Hyper-V orchestration이 주된 문제라는 점을 runtime policy에 명시하고 HTTP/API/CLI/diagnostics에서 관찰 가능한 job runtime 계약을 안정화하는 것이다.

Phase 2B-2H는 VM create job, in-memory queue, JSON file persistence, cancel/retry, bounded worker tick을 단계적으로 추가했다. 이후 Phase 12-23은 제품 wrapper, installer, diagnostics, LAN, update/rollback, release evidence에 집중했다. Phase 24는 다시 Local API runtime 내부로 돌아와 job state, persistence, dispatch, host mutation boundary를 명시적 public policy로 고정한다.

## 결정

```text
DESKTOP_NODE_PHASE24_JOB_RUNTIME_BOUNDARY_CANDIDATE: local-api-job-runtime-contract-first
```

Phase 24는 현재 적용 ADR을 새로 만들지 않는다. 이 결정은 공개 출시 경계, 제품 승격 gate, installer/service/update/security policy를 바꾸지 않고, `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`를 유지한다.

2026-05-02 Phase 25 후속 delta로 `host.status` read route는 `src/DesktopNode.Api/**`의 C# registry/WMI/service/admin native adapter가 처리하고, `network.inventory` read route는 C# native WMI adapter를 먼저 시도하되 switch topology parity field가 불완전하면 PowerShell helper로 fallback하는 경로가 됐다. 2026-05-03 후속 delta로 `vm.list`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints`도 C# native-first read path를 먼저 시도하되 VM identity/state, summary field, checkpoint list parity가 불완전하면 PowerShell helper로 fallback한다. 이 delta는 Phase 24의 PowerShell helper baseline을 지우지 않으며, 현재 runtime policy는 `dotnet-native-read-plus-hyperv-helper-process` hybrid boundary, `read-route-started` native core 상태, `host.status,network.inventory,vm.list,checkpoint.list` reason을 보고한다.

> 현행화 메모(2026-05-05/06): 이 문서의 `unsupported_future_version = quarantine-and-start-empty` 계약은 Phase 24 당시 PowerShell component baseline이다. 현재 .NET 제품 경로는 unsupported future job-store schema를 quarantine/move 없이 blocked diagnostics/no-mutation으로 처리한다. 2026-05-06 후속에서 schema v2 migration store load와 `job-store-migration-apply` code-level actual path가 추가됐다. Corrupt JSON quarantine과 Phase 24 component baseline은 당시 기록으로 보존한다.

ADR은 다음 조건이 충족될 때만 후속으로 검토한다.

- job runtime 계약이 CLI/API/diagnostics에서 안정적으로 사용된다.
- PowerShell runtime과 C++23 native core 사이의 책임 경계가 실제 구현 후보로 좁혀진다.
- 기존 Phase 19/22 결정과 충돌하는 durable tradeoff가 생긴다.

## 경계

포함 범위:

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
- `spikes/purecvisor-desktop-node/api/tests/**`
- `spikes/purecvisor-desktop-node/api/README.md`
- `spikes/purecvisor-desktop-node/cli/**`
- 관련 root documentation/index/roadmap/follower 문서

제외 범위:

- 실제 Hyper-V VM 생성/삭제
- service install/start/stop/delete
- MSI install/repair/uninstall
- Windows Firewall 변경
- Event Log source 등록
- network download updater
- C++23 native runtime 구현
- Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime

## Job runtime public contract

Phase 24의 첫 계약은 `GET /api/v1/runtime/policy`에 `job_runtime` object를 노출하는 것이다.

초기 계약:

```json
{
  "contract_version": 1,
  "owner": "local-api",
  "state_store": {
    "backend": "script-scope-memory",
    "persistence": "json-file-snapshot",
    "corrupt_store": "quarantine-and-start-empty",
    "unsupported_future_version": "quarantine-and-start-empty"
  },
  "dispatch": {
    "mode": "bounded-synchronous-worker-tick",
    "helper_boundary": "hyperv-helper-process"
  },
  "control": {
    "cancel": {
      "queued_only": true,
      "running_interrupt": false
    },
    "retry": {
      "manual_only": true,
      "failed_error_retryable_only": true,
      "max_attempts": 3,
      "creates_new_job": true
    }
  },
  "host_mutation": "helper-process-only",
  "orchestration": {
    "primary": "powershell",
    "contract": "plan-contract-injectable-runner-diagnostics"
  },
  "native_core": {
    "status": "not-planned-unless-runtime-boundary-deepens",
    "reason": "windows-hyperv-orchestration-not-dataplane",
    "revisit_when": "state-machine-or-supervision-outgrows-powershell"
  }
}
```

이 계약은 다음 의미를 갖는다.

- Local API가 job runtime의 owner다.
- 현재 state store는 PowerShell script-scope memory이며, `-JobStorePath`가 JSON snapshot persistence를 제공한다.
- corrupt job store와 future version job store는 quarantine하고 빈 store로 시작한다.
- worker dispatch는 threaded runtime이 아니라 bounded synchronous tick이다.
- cancel은 queued job에만 허용하고 running helper interruption은 지원하지 않는다.
- retry는 failed job 중 error가 `retryable = true`인 경우에만 수동으로 허용하며, 기존 job을 바꾸지 않고 새 queued job을 만든다.
- host mutation은 worker가 호출하는 Hyper-V helper process boundary 뒤에서만 발생한다.
- 기본 구현 방향은 PowerShell orchestration, Pester contract, injectable runner, diagnostics evidence다.
- C++23 native core는 Windows Hyper-V orchestration 문제가 state machine/supervision 문제로 깊어질 때만 재검토한다.

## C++23 판단 기준

Phase 24는 C++23 도입을 전제하지 않는다. 현재 판단은 `not-planned-unless-runtime-boundary-deepens`다. 다음 조건이 모두 명확해질 때만 C++23 core 후보를 다시 평가한다.

- job state machine이 PowerShell orchestration에서 분리할 만큼 깊은 module behavior를 가진다.
- CLI/API/diagnostics가 내부 PowerShell helper가 아니라 public job runtime contract만 의존한다.
- persistence migration, retry/cancel semantics, running job recovery가 public behavior로 검증된다.
- Windows host mutation은 계속 PowerShell/Hyper-V helper adapter 뒤에 남길 수 있다.

PowerShell에 남길 가능성이 높은 영역:

- `HttpListener` lifecycle
- Hyper-V helper process invocation
- token/protected token source wiring
- JSONL diagnostics/event writing
- Windows service/product wrapper orchestration

C++23 후보가 될 수 있는 영역:

- job state transition validator
- persisted job store schema migration
- retry/cancel/recovery policy evaluator
- long-running runtime supervision core

## TDD 적용 방식

Phase 24 구현은 작은 vertical slice로 진행한다.

1. public behavior 하나를 Pester로 먼저 고정한다.
2. 좁은 API suite를 실행해 실패를 확인한다.
3. 최소 구현으로 통과시킨다.
4. 관련 README/phase 문서를 업데이트한다.
5. API suite, root documentation suite, `git diff --check`를 실행한다.

첫 slice는 `runtime.policy`의 `job_runtime` contract다. 두 번째 slice는 실제 host mutation 없이 Hyper-V helper boundary를 관찰하는 `network.inventory` read-only contract다. 세 번째 runtime slice는 persisted `running` job을 restart 이후 `PCV_JOB_INTERRUPTED` retryable failure로 복구하고 자동 재큐잉하지 않는 recovery contract다. 네 번째 slice는 unsupported future job store version을 quarantine하고 빈 store로 시작하는 persistence schema compatibility contract다. 다섯 번째 slice는 retry/cancel semantics를 `job_runtime.control` public policy와 non-retryable failed job rejection으로 고정한다. 여섯 번째 slice는 product wrapper diagnostic bundle이 runtime policy의 Phase 24 `job_runtime` 계약을 자체 점검하는 diagnostics self-audit contract다. 일곱 번째 slice는 CLI thin client가 내부 helper가 아니라 `GET /api/v1/runtime/policy` public contract를 그대로 조회하는 consumer contract다.

## Read-only network inventory slice

`GET /api/v1/network/inventory`는 Local API에서 Hyper-V helper의 `network.inventory` operation으로 전달된다. helper는 `Get-VMSwitch` 결과를 다음 계약으로 반환한다.

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

이 slice의 목적은 Windows Hyper-V가 제공하는 네트워크 가상화 상태를 PowerShell orchestration public boundary로 읽는 것이다. switch 생성, NAT 구성, firewall 변경, SR-IOV/SET 변경 같은 mutation은 포함하지 않는다.

## Running job recovery slice

`Initialize-PcvApiJobStore -Path <jobs.json>`가 persisted store를 읽을 때 `status = "running"` job을 그대로 유지하지 않는다. API process restart 이후 PowerShell worker tick이 중단된 job을 안전하게 이어받을 수 없으므로, 해당 job은 다음 계약으로 복구한다.

```json
{
  "status": "failed",
  "error": {
    "code": "PCV_JOB_INTERRUPTED",
    "retryable": true
  },
  "result": null
}
```

복구된 job은 queue에 자동으로 다시 들어가지 않는다. 운영자는 기존 manual retry route로 새 retry job을 명시적으로 만들 수 있다. 이 slice는 JSON job store load path만 다루며 Hyper-V helper, service, MSI, firewall, Event Log, reboot mutation을 호출하지 않는다.

## Persistence schema compatibility slice

`Initialize-PcvApiJobStore -Path <jobs.json>`는 현재 `version = 1` job store를 지원한다. `version`이 없거나 1 이하인 store는 기존 v1 호환 경로로 로드한다. 현재 runtime보다 큰 future version은 조용히 로드하지 않는다. 해당 파일은 다음 형태의 quarantine path로 이동하고, Local API는 빈 job store로 시작한다.

```text
<jobs.json>.unsupported.<version>.<timestamp>
```

반환 error code는 `PCV_JOB_STORE_UNSUPPORTED_VERSION`이며 `retryable = false`다. 이 계약은 downgrade 또는 future schema 오염으로 잘못된 job state를 재개하지 않게 하는 방어선이다. 기존 corrupt JSON 처리와 마찬가지로 Hyper-V helper, service, MSI, firewall, Event Log, reboot mutation을 호출하지 않는다.

## Retry/cancel semantics slice

`POST /api/v1/jobs/{job_id}/cancel`은 `queued` job에만 허용한다. `running`, `succeeded`, `failed`, `canceled` job은 `409 PCV_JOB_NOT_CANCELABLE`을 반환한다. 현재 runtime은 running Hyper-V helper process를 interrupt하지 않는다.

`POST /api/v1/jobs/{job_id}/retry`는 `failed` job 중 `error.retryable = true`인 job에만 허용한다. `error.retryable = false`이거나 retryability를 판단할 수 없는 failed job은 `409 PCV_JOB_NOT_RETRYABLE`을 반환한다. retry는 원본 job을 수정하지 않고 `retry_of`와 증가된 `attempt`를 가진 새 queued job을 만든다. `attempt = 3` 이후 retry는 `409 PCV_JOB_RETRY_LIMIT_REACHED`로 거부한다.

## Diagnostics bundle self-audit slice

Product wrapper의 `CollectDiagnostics`는 runtime policy 응답을 `runtime-policy-redacted.json`으로 남긴다. Phase 24 self-audit slice는 여기에 `diagnostics-self-audit.json`을 추가해 bundle 생성 시점의 runtime policy가 `job_runtime` public contract를 포함하는지 요약한다.

Self-audit artifact는 다음 정보를 남긴다.

- runtime policy 수집 결과가 존재하고 사용 가능한지
- runtime policy body가 JSON으로 parse 가능한지
- `job_runtime` object 존재 여부
- `contract_version`, `owner`, 핵심 state/dispatch/control/host mutation 필드가 Phase 24 기대값과 맞는지

`diagnostics-manifest.json`은 `sources`에 `diagnostics_self_audit` artifact를 포함하고, `self_audit.runtime_policy.job_runtime.contract_ok` 요약을 함께 기록한다. 이 slice는 diagnostic bundle 파일만 쓰며 service install/start/stop, Hyper-V mutation, MSI lifecycle, firewall, Event Log source 등록, reboot mutation을 호출하지 않는다.

## CLI runtime policy consumer slice

CLI thin client는 `runtime policy` 명령을 `GET /api/v1/runtime/policy`로만 변환한다. 이 명령은 Local API job runtime public boundary를 사람이 직접 확인하는 소비자 경로이며, Hyper-V helper, service, MSI, firewall, Event Log, reboot mutation을 호출하지 않는다.

## 검증 기준

Phase 24 Local API job runtime 변경 후 기본 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

CLI 또는 Web Console 표시가 바뀌면 각각 다음을 추가한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
node --check web/app.js
```

관리자 권한 검증은 기본값이 아니다. 실제 Hyper-V lifecycle smoke는 Phase 21 runbook과 사용자 opt-in을 따른다.
