# PureCVisor Desktop Node .NET Windows Service Host 교체 구현 계획

> **agentic worker 필수 기준:** 이 계획은 task 단위 구현을 전제로 한다. 병렬 작업이 필요하면 `superpowers:subagent-driven-development`를 우선 사용하고, 단일 세션 실행은 `superpowers:executing-plans`를 따른다. 진행 상태는 checkbox(`- [x]`)로 추적한다.

## 목표

기본 제품 실행 경로를 WinSW + PowerShell Local API service host에서 .NET Windows Service executable 경로로 교체한다.

## 아키텍처

`DesktopNode.Host.exe`가 listener, port bind, SCM binary path를 소유한다. Product wrapper와 MSI action은 더 이상 기본 경로에서 WinSW XML을 생성하지 않고, SCM이 .NET host를 실행하도록 구성한다. 후속 route parity/native adapter slices 이후 .NET request processor는 native read routes, VM create/start/shutdown/poweroff/restart/delete native lifecycle mutation routes, checkpoint create/restore/delete native mutation routes, job get/cancel/retry, JSON job store save/load/recovery를 처리하고 Host가 request body, helper script path, job store path를 전달하도록 확장됐다. Native VM create product path는 Hyper-V Generation 2만 지원하고 native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. `DesktopNodeApiRequestProcessor` public entrypoint는 shared job dictionary/queue/job-store snapshot 보호를 위해 직렬화된다.

## 기술 스택

.NET 10, C#, xUnit, PowerShell 7, Pester 5, WiX 5, Windows SCM, 기존 Desktop Node JSON contract.

## 파일 구조

- 생성: `src/DesktopNode.Host/DesktopNode.Host.csproj`
- 생성: `src/DesktopNode.Host/Program.cs`
- 생성: `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
- 생성: `src/DesktopNode.Host/DesktopNodeHostApplication.cs`
- 생성: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- 생성: `src/DesktopNode.Host/DesktopNodeHostTokenResolver.cs`
- 생성: `src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj`
- 생성: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- 생성: `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`
- 생성: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`
- 수정: `src/DesktopNode.sln`
- 수정: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- 수정: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- 수정: `src/DesktopNode.Contracts/RuntimePolicy.cs`
- 수정: `src/DesktopNode.Contracts.Tests/RuntimePolicyContractTests.cs`
- 수정: `src/DesktopNode.Service/ServiceHostCandidateContract.cs`
- 수정: `src/DesktopNode.Service/ServiceLifecycleAdapterContract.cs`
- 수정: `src/DesktopNode.Service.Tests/*.cs`
- 수정: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- 수정: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- 수정: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- 수정: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- 수정: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- 수정: `packaging/windows-desktop-node/installer/build.ps1`
- 수정: `packaging/windows-desktop-node/installer/Product.wxs`
- 수정: `packaging/windows-desktop-node/installer/ProductActions.wxs`
- 수정: `packaging/windows-desktop-node/installer/installer-provenance.schema.json`
- 수정: `packaging/windows-desktop-node/installer/tests/*.ps1`
- 수정: `packaging/windows-desktop-node/README.md`
- 수정: `packaging/windows-desktop-node/installer/README.md`
- 수정: `follower.md`

## 작업 1: .NET host project scaffold

- [x] command-line option parsing 실패 테스트 작성:
  - `listen --prefix http://127.0.0.1:7777/ --web-root <path> --job-store <path> --event-log <path> --api-token-protected-file <path>`
  - inline token 값 거부
  - `--allow-lan`이 token source 없이 지정되면 거부
- [x] `DesktopNode.Host`, `DesktopNode.Host.Tests` project 추가.
- [x] solution reference와 `DesktopNode.Api`, `DesktopNode.Service`, `DesktopNode.Contracts` project reference 추가.
- [x] host mutation 없는 최소 option parser 구현.
- [x] `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj` 실행.

## 작업 2: .NET listener transport

- [x] `GET /api/v1/runtime/policy`, unsupported route `404`, unsupported method `405`, static root serving 실패 테스트 작성.
- [x] `DesktopNodeApiRequestProcessor`를 순수 request processor로 확장.
- [x] `DesktopNodeHostApplication`을 `HttpListener` 기반으로 구현.
- [x] 기본 listener를 loopback-only로 유지.
- [x] token file과 DPAPI protected token file을 읽어 API route bearer auth를 적용.
- [x] loopback static asset은 기존 정책대로 bearer 없이 열 수 있고, API route는 token source가 있으면 bearer를 요구하도록 검증.
- [x] `dotnet test src/DesktopNode.sln` 실행.

## 작업 3: service contract owner 전환

- [x] `DesktopNode.Service.Tests` 기대값 전환:
  - `ServiceHostReplacementStance = default`
  - `DefaultOwner = dotnet-windows-service-host`
  - allowed launch modes에 `windows-service` 포함
  - 기본 owner string에서 WinSW 제거
- [x] service contract를 .NET default owner 기준으로 구현.
- [x] `dotnet test src/DesktopNode.Service.Tests/DesktopNode.Service.Tests.csproj` 실행.

## 작업 4: product wrapper service plan

- [x] `New-PcvDesktopNodeProductPlan`이 `service.mode = dotnet-windows-service`를 사용한다는 Pester 실패 테스트 작성.
- [x] default plan에서 WinSW XML path가 생성되지 않음을 검증.
- [x] `sc.exe create` `binPath`가 `DesktopNode.Host.exe listen`을 가리키는지 검증.
- [x] token source가 protected token file path를 사용하고 token 값을 남기지 않음을 검증.
- [x] product wrapper plan 변경 구현.
- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"` 실행.

## 작업 5: MSI payload와 custom action runner

- [x] host publish output이 MSI payload로 staging된다는 installer 실패 테스트 작성.
- [x] WiX source custom action이 `powershell.exe`가 아니라 `[INSTALLFOLDER]DesktopNode.Host.exe service-action ...`을 호출한다는 실패 테스트 작성.
- [x] `build.ps1`이 `DesktopNode.Host`를 publish/copy하도록 수정.
- [x] `Product.wxs`가 host executable/runtime file을 포함하도록 수정.
- [x] `ProductActions.wxs` custom action command를 수정.
- [x] `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"` 실행.

## 작업 6: 문서와 evidence

- [x] Phase 25 plan/spec과 `follower.md`에 실행 경로 교체 시작 slice를 반영.
- [x] product wrapper README와 installer README를 .NET host 기준으로 갱신.
- [x] MSI provenance schema를 `service_host` 기준으로 갱신.
- [x] full non-destructive verification 실행.
- [x] root README, AGENTS, developer index, release boundary, verification policy, ADR index/ADR-0001, roadmap, phase specs/plans, component README, documentation sync guard를 .NET Host replacement 현재 상태로 현행화.

## 관리자 opt-in smoke

비파괴 검증이 통과한 뒤에만 실행한다.

- [x] unsigned `admin-smoke` MSI build.
- [x] 자동 reboot 없이 service install/start/status/stop/delete smoke 실행.
- [x] 다음 argument를 포함해 MSI install/repair/uninstall/`REMOVE_DATA=1`/restore lifecycle 실행:
  - `REBOOT=ReallySuppress`
  - `MSIRESTARTMANAGERCONTROL=Disable`
  - `/norestart`
- [x] boot time before/after 기록.
- [x] `Restart-Computer`는 실행하지 않는다.

완료 evidence:

- `artifacts/dotnet-host-admin-smoke-20260501-213444/build-output.json`: `0.26.0-admin-smoke` unsigned MSI build PASS.
- `artifacts/dotnet-host-admin-smoke-20260501-213444/service-action-smoke.json`: `DesktopNode.Host.exe service-action` direct service install/start/static health/bearer runtime policy/delete PASS.
- `artifacts/dotnet-host-admin-smoke-20260501-213444/msi-lifecycle-smoke.json`: MSI install, repair, uninstall preserve, reinstall, `REMOVE_DATA=1` uninstall, final restore install 모두 exit `0`, boot time unchanged.
- `artifacts/dotnet-host-admin-smoke-20260501-213444/hyperv-integration-smoke.json`: Hyper-V `host.status`, `vm.create`, `vm.list`, `vm.start`, `checkpoint.create`, `vm.poweroff` runner integration PASS, `pcv-spike-*` VM 잔여물 없음.

이번 evidence는 `AllowUnsignedDev` `admin-smoke` 범위다. Public trusted signing, external stable publication, GA 제품 런타임 승격 evidence가 아니다.

## Route parity 시작 slice

- [x] `DesktopNodeApiRequestProcessor`가 `GET /api/v1/host/status`, `GET /api/v1/network/inventory`, `GET /api/v1/vms`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints`를 Hyper-V helper process boundary로 dispatch한다.
- [x] `POST /api/v1/vms`, VM lifecycle routes, checkpoint create/restore/delete routes가 queued job을 생성한다. 후속 native adapter slices 이후 VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete는 worker tick에서 C# WMI adapter를 호출한다.
- [x] `GET /api/v1/jobs/{job_id}`, `POST /api/v1/jobs/{job_id}/cancel`, `POST /api/v1/jobs/{job_id}/retry`가 Phase 24 cancel/retry contract를 따른다.
- [x] JSON job store snapshot을 저장하고, process restart 이후 queued job load와 persisted running job의 `PCV_JOB_INTERRUPTED` recovery를 수행한다.
- [x] `DesktopNode.Host`가 POST request body, helper script path, job store path를 .NET API processor로 전달한다.

이 slice의 code-level route parity 시작 evidence 이후, 설치본 기준 route mutation smoke도 관리자 opt-in gate로 실행했다.

설치본 route parity evidence:

- `packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1`: tracked route parity mutation smoke runner. stdout/stderr 비동기 drain, `progress.json` 단계 marker, `Get-VM | Where-Object Name -like 'pcv-spike-*'` 안전 조회를 포함한다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729/build-output.json`: `0.26.6-admin-smoke` unsigned MSI build PASS, provenance git commit `22c38284dcb3d3804b077c7f5c0fbf074b3ef034`, MSI SHA-256 `a468357f06c0176c75f02266b900aef17c5d0393590bb5b638797cd0345874a8`.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729/service-action-smoke.json`: direct `DesktopNode.Host.exe service-action` install/start/health/remove PASS.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729/msi-lifecycle-smoke.json`: MSI install, repair, uninstall preserve, reinstall, `REMOVE_DATA=1` uninstall, final restore install 모두 PASS, boot time unchanged.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729/hyperv-api-route-smoke.json`: 설치본 .NET Host에서 `host.status`, `network.inventory`, `vm.create`, `vm.list`, `vm.get`, `vm.start`, `checkpoint.create`, `checkpoint.list`, `checkpoint.delete`, `vm.poweroff` route smoke PASS.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-004729/summary.json`: final service `Running`, service path `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음.

이 evidence는 `AllowUnsignedDev` `admin-smoke` 범위다. Public trusted signing, external stable publication, GA 제품 런타임 승격 evidence가 아니다.

Native adapter code-level follow-up:

- `docs/superpowers/plans/2026-05-02-purecvisor-desktop-node-dotnet-native-network-inventory-adapter.md`: `host.status` C# native read adapter와 `network.inventory` C# native WMI read adapter slice 완료.
- `dotnet test src\DesktopNode.sln`: PASS.
- 컴파일된 `DesktopNode.Api.dll` native adapter 직접 호출 결과: `handled=true`, `ok=true`, `source=hyperv`, `mutating=false`, `Default Switch` 포함.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-012126`: commit `b23030efb2cc305925ea3765d5c8a341e40069a9` 기준 `0.26.8-admin-smoke` 설치본 service/MSI/Hyper-V route smoke PASS. `network.inventory` installed response는 `source=hyperv`, `mutating=false`, `Default Switch`를 반환했다.

리뷰 수정 후속:

- MSI repair resilience: `repair-installed` plan이 `stop` 이후 `create`를 다시 실행하고 기존 service의 `1073`은 허용한 뒤 `config`를 계속 수행한다.
- Native inventory parity guard: switch type, `allow_management_os`, external adapter field가 불완전하면 native adapter가 route를 처리하지 않고 helper로 fallback한다.
- Processor concurrency guard: `Handle`, `ProcessOneQueuedJob`, `ProcessWorkerPool` entrypoint가 shared request processor state 접근을 직렬화한다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-020406`: commit `352aa256b77109ea9104602aebd424c627db11ed` 기준 `0.26.9-admin-smoke` 설치본 service/MSI/Hyper-V route smoke PASS. MSI SHA-256은 `d517baee2149d9dfcf6bd34d77b4f9de8663fd7e416558c1ba0ffb3de16788e3`이며, installed `network.inventory`는 helper parity fallback으로 `Default Switch`, `type=internal`, `allow_management_os=true`를 반환했다. Final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.
- `artifacts/routeparity-service-msi-hyperv-mutation-20260502-031154`: commit `7120ef58b924cfdf664f868b857fb91537bf6be9` 기준 `0.27.1-admin-smoke` 설치본 service/MSI/Hyper-V route smoke PASS. MSI SHA-256은 `9e6c57ef852df2df7794598fd0193141ad4f95f7ec365c565453a6fc05b9c48f`이며, installed `host.status`는 native C# adapter로 Windows 10 Pro for Workstations `25H2`, `supported=true`, admin elevated, Hyper-V enabled, VMMS running, Default Switch present를 반환했다. Final service는 `Running`, boot time unchanged, `pcv-spike-*` VM 잔여물 없음으로 끝났다.

문서 현행화 evidence:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: documentation boundary/sync guard PASS.
- `git diff --check`: exit `0`, line-ending warnings only.
