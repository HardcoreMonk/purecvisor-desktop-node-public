# PureCVisor Desktop Node Phase 25 .NET/TypeScript 전환 설계

## 목적

Phase 25 후보는 Desktop Node의 장기 제품 코어를 PowerShell 단일 구현에서 **C#/.NET runtime core + TypeScript Web Console + PowerShell Windows adapter** 조합으로 점진 전환하는 기준을 정의한다.

이 단계는 Desktop Node를 GA 제품 런타임으로 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지한다. 목표는 Phase 24에서 고정한 Local API job runtime public contract와 Phase 23 diagnostics/operational evidence를 깨지 않으면서, 타입 안정성과 장기 서비스 운영성이 필요한 영역을 .NET으로 옮길 수 있는 작은 이행 경로를 만드는 것이다.

## 결정 후보

```text
DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first
DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start
DESKTOP_NODE_PHASE25_NATIVE_READ_PARITY_GUARD: network-inventory-vm-list-vm-detail-and-checkpoint-list-helper-fallback-on-incomplete-parity
```

Phase 25는 ADR을 즉시 추가하지 않는다. 다음 조건이 충족될 때 ADR 승격을 검토한다.

- .NET contract/runtime core가 기존 PowerShell Local API contract와 호환된다.
- PowerShell Hyper-V helper는 계속 JSON stdin/stdout adapter 경계로 남는다.
- TypeScript Web Console build output이 기존 static serving contract를 유지한다.
- Packaging/diagnostics/MSI 경계가 기존 Phase 12-24 검증을 통과한다.
- C#/.NET 전환이 GA 승격이나 stable release 발행을 암시하지 않는다.

## 역할 분리

### C#/.NET

.NET은 제품 코어 후보를 담당한다.

- job state transition validator
- job runtime policy model
- persisted job store schema model
- Local API host 후보
- Windows Service host 후보
- default Windows Service host executable
- diagnostics contract serializer 후보

초기 slice는 제품 실행 경로를 바꾸지 않고 .NET contract/runtime/API/service candidate library와 test project를 추가해 Phase 24 `job_runtime` contract, job state transition, API host stance, service host stance를 타입으로 고정했다.

후속 시작 slice에서는 `GET /api/v1/runtime/policy`의 순수 request processor를 .NET에 추가했다. 2026-05-01 replacement slice에서는 `DesktopNode.Host.exe`가 `HttpListener`, loopback port bind, Windows SCM service binary path, MSI installed custom action runner를 소유하도록 전환했다. Route parity 시작 slice에서는 `src/DesktopNode.Api/**`가 Hyper-V helper process를 호출하는 read routes, queued VM/checkpoint lifecycle routes, job get/cancel/retry, JSON job store save/load/recovery를 처리하고 `src/DesktopNode.Host/**`가 body/helper/job-store context를 전달한다. Hyper-V helper execution 자체는 PowerShell adapter 경계로 유지한다.

2026-05-01 사용자 관리자 opt-in smoke에서는 `0.26.0-admin-smoke` unsigned MSI로 service mutation, MSI lifecycle, Hyper-V helper integration을 실제 실행했다. 이 evidence는 자동 reboot 금지 조건을 만족하지만 `AllowUnsignedDev` 범위이므로 public trusted/stable signing evidence나 GA 승격 근거가 아니다.

### PowerShell

PowerShell은 Windows/Hyper-V adapter와 운영 자동화에 남긴다.

- Hyper-V cmdlet 호출
- component Local API spike와 service helper
- product wrapper diagnostics/update/rollback orchestration
- administrator opt-in smoke runbook
- Pester contract tests
- signing/MSI/reboot/firewall/Event Log 증거 수집

실제 host mutation은 계속 `-WhatIf`, injectable runner, 관리자 opt-in gate 뒤에 둔다.

### TypeScript

TypeScript는 Web Console의 장기 구현 후보를 담당한다.

- Local API response type
- VM/job/checkpoint UI state
- runtime policy/diagnostics view model
- build-time JavaScript validation

초기 TypeScript 전환은 static asset parity scaffold로 시작했다. 2026-05-03 served asset/root migration slice 이후 `web/src/served-app.ts`가 served `web/app.js`를 생성하며, source/type scaffold, generated parity manifest, `verify:parity` script는 repo-root `web/**`에서 사용자 표시 계약과 served asset freshness를 검증한다.

## 경계

포함 범위:

- `src/DesktopNode.Contracts/**` 후보
- `src/DesktopNode.Runtime/**` 후보
- `src/DesktopNode.Api/**` 후보
- `src/DesktopNode.Service/**` 후보
- `src/DesktopNode.Host/**` default Windows Service host 후보
- `web/**` TypeScript source와 served static asset
- `packaging/windows-desktop-node/**` wrapper integration
- `docs/**` phase/verification/roadmap 문서

제외 범위:

- Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- C++23 native runtime 구현
- 기본 비파괴 검증에서의 실제 Hyper-V VM lifecycle mutation
- 기본 비파괴 검증에서의 service install/start/stop/delete
- 기본 비파괴 검증에서의 MSI install/repair/uninstall
- Windows Firewall 변경
- Event Log source 등록
- stable release 발행 또는 GA 승격

## 전환 원칙

1. Public contract가 먼저다.
   - `/api/v1/runtime/policy`, job API, CLI, diagnostics artifact contract를 깨지 않는다.

2. .NET은 side-by-side로 시작한다.
   - 첫 .NET slice는 기존 PowerShell request path를 대체하지 않는다.
   - `dotnet test`와 Pester가 같은 contract를 검증해야 한다.

3. PowerShell adapter는 의도적으로 유지한다.
   - Hyper-V cmdlet은 PowerShell이 가장 직접적인 공식 관리 경로다.
   - .NET은 helper process를 JSON adapter로 호출하거나 같은 contract를 consume한다.

4. TypeScript는 build artifact contract를 먼저 지킨다.
   - Web Console은 Local API static serving 아래에서 계속 동작해야 한다.
   - TypeScript build가 추가되어도 사용자는 별도 dev server를 요구받지 않는다.

5. Packaging 전환은 evidence 기준으로 진행한다.
   - 초기 slice에서는 product wrapper/MSI가 기존 PowerShell entrypoint를 유지했다.
   - 2026-05-01 replacement slice 이후 기본 제품 service host와 MSI installed action runner는 `DesktopNode.Host.exe`다.
   - WinSW는 기본 plan이 아니라 Phase 13 이력/compatibility 경계로 남긴다.

## 구현 slice 상태

Phase 25의 첫 구현 slice는 `.NET contract mirror`였고, 현재 다음 side-by-side slice가 구현돼 있다.

- `src/DesktopNode.Contracts/**`: Phase 24 runtime policy contract mirror
- `src/DesktopNode.Runtime/**`: 순수 job state transition validator
- `src/DesktopNode.Api/**`: API host candidate contract
- `src/DesktopNode.Service/**`: Service host candidate contract
- `src/DesktopNode.Host/**`: `DesktopNode.Host.exe listen`과 `service-action` runner
- `web/src/**`: TypeScript API/view-model/source scaffold와 served asset source
- `web/generated/parity/**`: static asset parity manifest
- `web/scripts/**`: parity verification flow

유지 조건:

- PowerShell Local API spike와 Hyper-V helper는 component/adapter 경계로 유지한다.
- Packaging/MSI 기본 service-action 경로는 `DesktopNode.Host.exe`로 교체한다.
- WinSW 경로는 기본값이 아니라 Phase 13 이력/compatibility 경계로 유지한다.
- Web Console은 2026-05-03 served asset/root migration slice 이후 repo-root `web/src/served-app.ts`가 generated `web/app.js`를 소유한다.
 - API host route parity 시작 slice는 helper-backed routes와 queued job runtime을 구현했다. 2026-05-02/2026-05-03 native adapter slices 이후 `host.status`, `network.inventory`, `vm.list`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints`는 C# native adapter가 helper fallback 없이 structured success/failure를 반환한다. VM create/start/shutdown/poweroff/restart/delete는 .NET request processor queue를 유지하되 C# WMI adapter가 직접 실행한다. Native VM create product path는 Hyper-V Generation 2만 지원하며, native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. Checkpoint create/restore/delete는 .NET request processor queue를 유지하되 C# WMI snapshot service adapter가 직접 실행한다. 실제 MSI 설치본 기준 route mutation evidence hardening은 별도 관리자 opt-in 검증으로 수행한다.
- `GET /api/v1/runtime/policy` pure request processor와 .NET Host listener는 replacement slice에서 기본 제품 경로에 들어갔다.

## Runtime policy 변경 방향

Phase 24의 `job_runtime.native_core.status`는 `not-planned-unless-runtime-boundary-deepens`였다. Phase 25 후보의 service host replacement와 native adapter slices 이후 현재 runtime policy는 `read-route-vm-create-lifecycle-and-checkpoint-mutation-started`와 operation 기준 `host.status,network.inventory,vm.list,checkpoint.list,vm.create,vm.start,vm.shutdown,vm.poweroff,vm.restart,vm.delete,checkpoint.create,checkpoint.restore,checkpoint.delete` reason을 보고한다. `dispatch.native_probe_operations`는 read operations인 `host.status`, `network.inventory`, `vm.list`, `checkpoint.list`로 제한하고, `dispatch.native_mutation_operations`는 `vm.create`, `vm.start`, `vm.shutdown`, `vm.poweroff`, `vm.restart`, `vm.delete`, `checkpoint.create`, `checkpoint.restore`, `checkpoint.delete`로 제한한다. `dispatch.mutation_dispatch`는 `native-vm-create-lifecycle-delete-checkpoint-mutation`이다.

```json
{
  "job_runtime": {
    "managed_core": {
      "candidate": "dotnet",
        "status": "service-host-default",
        "host_replacement": "dotnet-windows-service-host"
    }
  }
}
```

이 필드는 .NET contract/runtime policy test와 product runtime policy smoke에서 검증한다. `owner = local-api`와 Hyper-V helper dispatch는 아직 PowerShell adapter 경계를 가리킨다.

## 검증 기준

Phase 25 문서/계획 변경:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Phase 25 .NET contract/runtime 변경:

```powershell
dotnet test src/DesktopNode.sln
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Phase 25 TypeScript Web Console 변경:

```powershell
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
git diff --check
```

Phase 25 packaging integration 변경:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
git diff --check
```

관리자 권한 검증은 기본 비파괴 검증이 아니다. 실제 Hyper-V lifecycle, service mutation, MSI lifecycle, firewall, Event Log, reboot 검증은 기존 Phase 20/21/23 gate와 .NET Host replacement plan의 opt-in smoke 기준을 따른다. 자동 reboot 실행은 금지한다.

## 완료 기준

Phase 25 설계 시작 작업은 다음을 만족하면 완료다.

- 이 spec과 구현 plan이 존재한다.
- `.NET + TypeScript + PowerShell` 역할 분리가 문서화되어 있다.
- 첫 slice가 .NET contract mirror였고, 후속 slice가 side-by-side contract/scaffold로 제한되어 있다.
- PowerShell Hyper-V adapter 유지와 기본 비파괴 검증에서의 host mutation 금지가 명시되어 있다.
- `DesktopNode.Host.exe` replacement slice의 범위와 evidence가 별도 문서로 연결되어 있다.
- GA 승격, stable release, C++23 전환을 의미하지 않는다고 명시되어 있다.

Phase 25 evidence gate 자체는 다음을 만족해야 닫힌다.

- .NET contract/runtime/API/service tests가 기존 Pester contract와 같은 behavior 또는 stance를 검증한다.
- TypeScript Web Console typecheck와 `verify:parity`가 기존 static serving contract를 유지한다.
- Packaging wrapper와 MSI가 기본 `DesktopNode.Host.exe` service host path를 검증한다.
- 기존 Phase 20/21/23 관리자 opt-in evidence gate와 충돌하지 않는다.
