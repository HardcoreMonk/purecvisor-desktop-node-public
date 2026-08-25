# PureCVisor Desktop Node .NET Windows Service Host 교체 설계

작성 기준: 2026-05-01

## 목표

Desktop Node의 기본 제품 실행 경로를 WinSW + PowerShell `HttpListener`에서 .NET Windows Service executable로 교체한다. 이 변경은 listener/port bind, SCM service binary path, MSI install/repair/remove custom action runner의 기본 owner를 .NET으로 옮기는 시작점이다.

## 결정

- service host: `DesktopNode.Host.exe`
- listener owner: .NET host가 `http://127.0.0.1:7777/`를 직접 bind한다.
- WinSW: 기본 product/service plan과 MSI payload에서 제거한다.
- MSI custom action: `powershell.exe -File Invoke-PcvDesktopNodeProduct.ps1` 직접 호출 대신 .NET service action runner를 호출한다.
- PowerShell: Hyper-V helper와 Windows adapter로 유지한다. service host 또는 default listener owner가 아니다.
- reboot policy: 자동 reboot는 금지한다. `Restart-Computer`, `shutdown.exe`, reboot-forcing `msiexec.exe` argument는 계속 차단한다.
- route parity marker: `DESKTOP_NODE_PHASE25_ROUTE_PARITY_START: dotnet-helper-backed-routes-job-runtime-start`
- native adapter marker: `DESKTOP_NODE_PHASE25_NATIVE_READ_START: host-status-network-inventory-vm-list-vm-detail-checkpoint-list-dotnet-native-adapter`
- native parity guard marker: `DESKTOP_NODE_PHASE25_NATIVE_READ_PARITY_GUARD: network-inventory-vm-list-vm-detail-and-checkpoint-list-helper-fallback-on-incomplete-parity`

## 1차 기능 범위

1차 교체 slice는 실행 경로 소유권을 바꾸는 데 집중한다.

- .NET host는 `GET /api/v1/runtime/policy`를 직접 처리한다.
- .NET host는 Web Console static root의 `index.html`, `app.js`, `styles.css`를 직접 제공할 수 있다.
- loopback prefix는 기본 허용하고 non-loopback prefix는 기존과 같이 명시 LAN opt-in과 token source가 있을 때만 허용한다.
- protected token file source는 command line에 token 값을 노출하지 않는다.
- `--api-token-file`과 `--api-token-protected-file`은 .NET host가 직접 읽고, API route는 bearer token을 요구한다.
- loopback static asset은 기존 정책대로 bearer 없이 열 수 있지만 API route는 token source가 있으면 bearer token을 요구한다.
- route parity 시작 slice는 helper-backed route parity, queued VM/checkpoint lifecycle routes, job get/cancel/retry, JSON job store save/load/recovery를 .NET request processor에 추가한다. 2026-05-02/2026-05-03 후속 native adapter slices 이후 `host.status`, `network.inventory`, `vm.list`, `GET /api/v1/vms/{id}`, `GET /api/v1/vms/{id}/checkpoints`는 C# native adapter가 helper fallback 없이 structured success/failure를 반환한다. VM create/start/shutdown/poweroff/restart/delete는 .NET request processor queue를 유지하되 C# WMI adapter가 직접 실행한다. Native VM create product path는 Hyper-V Generation 2만 지원하며, native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. Checkpoint create/restore/delete는 .NET request processor queue를 유지하되 C# WMI snapshot service adapter가 직접 실행한다.

## 구성 요소

### `src/DesktopNode.Host`

.NET executable이다. 두 실행 모드를 가진다.

- `listen`: console 또는 Windows Service entrypoint에서 Local API listener를 실행한다.
- `service-action`: installer/custom action에서 service install/configure/repair/remove plan을 실행한다.

이 프로젝트는 `DesktopNode.Api`, `DesktopNode.Service`, `DesktopNode.Contracts`를 참조한다.

`service-action configure-installed|repair-installed`는 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`이 없으면 DPAPI LocalMachine protected token file을 생성하고, SYSTEM/Administrators read ACL을 적용한다. 기존 service가 남아 있는 상태에서 `configure-installed` 또는 `repair-installed`가 실행되면 `sc.exe create`가 `1073`을 반환하더라도 `sc.exe config`로 `binPath`를 `DesktopNode.Host.exe listen ...` 기준으로 갱신한다. Repair 직전에 SCM service가 삭제됐거나 손상된 경우도 `repair-installed`가 먼저 `sc.exe create`를 실행한 뒤 config/description/failure/start를 이어간다. `sc.exe query` stopped 판정은 localized 문자열이 아니라 numeric state `1`도 허용한다.

### `src/DesktopNode.Api`

순수 request processor를 확장한다. listener transport와 handler를 분리해 unit test에서 실제 port bind 없이 behavior를 검증한다.

### `src/DesktopNode.Service`

SCM service command plan을 소유한다. WinSW XML과 WinSW command plan을 생성하지 않는다.

### `packaging/windows-desktop-node`

product plan의 `service.mode`는 `dotnet-windows-service`가 된다. install/start/stop/uninstall/status command는 `sc.exe`를 사용하되 executable path는 `DesktopNode.Host.exe`다.

### `packaging/windows-desktop-node/installer`

MSI payload는 .NET host publish output을 포함한다. 기본 build는 framework-dependent single-file `DesktopNode.Host.exe`를 publish해 WiX payload에 staging한다. `ProductActions.wxs`는 installed .NET action runner를 호출한다.

## 데이터 흐름

1. MSI가 `C:\Program Files\PureCVisor\DesktopNode`에 payload를 설치한다.
2. MSI deferred configure action이 installed `DesktopNode.Host.exe service-action configure-installed`를 호출한다.
3. action runner가 protected token file을 준비하고 service binary path를 `DesktopNode.Host.exe listen --prefix http://127.0.0.1:7777/ ...`로 구성한다.
4. SCM이 `DesktopNode.Host.exe`를 실행한다.
5. .NET host가 loopback listener를 bind하고 runtime policy/static assets를 제공한다.
6. .NET request processor가 Local API route를 판정하고 route별 native adapter 또는 helper transition boundary를 선택한다.
7. `host.status`, `network.inventory`, `vm.list`, VM detail, checkpoint list read route는 native adapter가 직접 처리하고 parity failure도 native structured failure로 반환한다.
8. VM create/start/shutdown/poweroff/restart/delete와 checkpoint create/restore/delete mutation route는 queued job으로 저장된 뒤 C# WMI adapter가 실행한다. Native VM create product path는 Hyper-V Generation 2만 지원하고 native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. Job store는 JSON snapshot으로 저장/로드하며 persisted running job은 restart 이후 `PCV_JOB_INTERRUPTED` failed 상태로 복구한다. Shared processor access는 request/worker tick entrypoint에서 직렬화한다.

## 오류 처리

- prefix validation 실패는 service start 전에 명확한 nonzero exit와 JSONL install log로 남긴다.
- protected token file이 없거나 DPAPI unprotect에 실패하면 listener start 전에 명확한 오류로 실패한다.
- service command 실패는 `PCV_PRODUCT_SERVICE_COMMAND_FAILED` 계열 오류로 반환한다.
- MSI `1641`은 실패로 분류하고 자동 reboot evidence로 사용하지 않는다.
- unsupported route는 `404` JSON response로 반환한다.

## 검증

필수 비파괴 검증:

- `dotnet test src/DesktopNode.sln`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"`
- `npm test --prefix web`
- `npm run verify:parity --prefix web`
- `npm run browser:fixture --prefix web`
- `node --check web/app.js`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"`
- `git diff --check`

관리자 opt-in 검증:

- service install/start/status/stop/delete
- MSI install/repair/uninstall/`REMOVE_DATA=1` lifecycle
- final restore install

관리자 opt-in 검증은 `REBOOT=ReallySuppress`, `MSIRESTARTMANAGERCONTROL=Disable`, `/norestart`와 boot time unchanged evidence를 함께 기록해야 한다.

## 비목표

- Hyper-V helper를 C#으로 즉시 교체하지 않는다.
- Event Log provider 기본 등록을 켜지 않는다.
- firewall 변경을 기본 install path에 넣지 않는다.
- public trusted signing 또는 stable publication을 주장하지 않는다.
