# PureCVisor Desktop Node Data Root Remove Handoff Implementation Plan

**Goal:** `REMOVE_DATA=1` uninstall 경계에서 service deletion과 ProgramData data-root deletion을 분리한다.

**Architecture:** `DesktopNode.Host.exe service-action remove-installed --remove-data`는 service stop/delete만 담당하고 ProgramData를 직접 삭제하지 않는다. Service deletion이 성공하면 `data-root-remove` handoff descriptor를 반환한다. 실제 data-root 삭제는 별도 `DesktopNode.Host.exe service-action data-root-remove --remove-data` action이 service absent precondition을 확인한 뒤 allowlist path만 삭제한다.

**Safety boundary:** 이 slice의 최초 구현은 code-level/xUnit/WiX source contract 변경이었다. 후속 관리자 opt-in smoke `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`에서 installed service/data-root lifecycle과 MSI `REMOVE_DATA=1` uninstall evidence를 수집했다. 이 evidence는 `AllowUnsignedDev` admin-smoke 범위이며 GA 승격, public trusted signing, stable publication 판단을 닫지 않는다.

---

## 범위

- 포함:
  - `RemoveDataHandoff` result contract.
  - `remove-installed --remove-data` 직접 data-root mutation 금지.
  - `data-root-remove --remove-data` 명시 opt-in.
  - service가 존재하면 data-root 삭제 차단.
  - allowlist 대상만 삭제: `api-token.txt`, `api-token.dpapi.json`, `jobs.json`, `events.jsonl`, `install.jsonl`, `diagnostics`.
  - protected token file 삭제 전 C# ACL API 기반 delete 권한 복구.
  - MSI `REMOVE_DATA=1`에서 `RemoveInstalled` 이후 `DataRootRemove` deferred custom action 호출.
- 제외:
  - public trusted/stable signed installed MSI lifecycle 실행.
  - product root removal 구현 변경.
  - GA 승격, public trusted signing, stable publication 판단.

## Task 1: Host action contract

**Files:**

- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [x] `service-action data-root-remove` parser test를 추가.
- [x] `remove-installed --remove-data`가 `RemoveDataHandoff`만 반환하고 data-root를 보존하는 regression test를 추가.
- [x] `data-root-remove`가 `--remove-data` 없이 실패하는 test를 추가.
- [x] service가 아직 있으면 `data-root-remove`가 실패하는 test를 추가.
- [x] service absent 상태에서 allowlist path만 삭제하는 test를 추가.
- [x] `data-root-remove` native service action branch와 deletion helper를 구현.

## Task 2: MSI sequence contract

**Files:**

- Modify: `packaging/windows-desktop-node/installer/Product.wxs`
- Modify: `packaging/windows-desktop-node/installer/ProductActions.wxs`
- Test: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1`

- [x] `DataRootRemove` custom action reference를 추가.
- [x] `DataRootRemoveData` deferred action data property를 추가.
- [x] `DesktopNode.Host.exe service-action data-root-remove --remove-data` ExeCommand를 추가.
- [x] `RemoveInstalled` 이후 `DataRootRemove`를 실행하도록 InstallExecuteSequence를 추가.
- [x] WiX source contract test가 `DataRootRemove` action과 installed payload root 인자를 검증하도록 갱신.

## Task 3: 문서 동기화

**Files:**

- Modify: `README.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `AGENTS.md`
- Modify: `follower.md`
- Modify: `packaging/windows-desktop-node/README.md`
- Modify: `packaging/windows-desktop-node/installer/README.md`

- [x] `remove-installed --remove-data` handoff와 `data-root-remove --remove-data` 별도 gate를 문서화.
- [x] 최초 code-level slice에서는 Installed destructive smoke가 별도 관리자 opt-in gate임을 유지.
- [x] 후속 `0.30.3-admin-smoke` evidence를 기록하고 public trusted/stable signing 또는 GA 승격 evidence가 아님을 문서화.
- [x] 문서 진입점에서 이 follow-up slice를 찾을 수 있게 연결.

## Verification Result

실행한 검증:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "DesktopNodeHostOptionsTests|DesktopNodeHostServiceActionTests" --no-restore
dotnet test src/DesktopNode.sln --no-restore
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

결과:

- Host focused xUnit: PASS.
- Full .NET solution xUnit: PASS.
- Installer Pester suite: PASS.
- Product wrapper Pester suite: PASS.
- Root boundary/documentation Pester suite: PASS.
- `git diff --check`: exit 0. Line-ending warning만 출력됐고 whitespace error는 없었다.

## Remaining Gate

- Installed destructive service/data-root lifecycle smoke는 관리자 opt-in으로 실행했고, `artifacts/routeparity-service-msi-hyperv-data-root-handoff-20260504-032646-0303`에서 PASS다.
- 이 smoke는 `service-action configure-installed`, service 존재 중 `data-root-remove --remove-data` 차단 `PCV_HOST_DATA_ROOT_REMOVE_SERVICE_EXISTS`, `remove-installed --remove-data` handoff, service absent 이후 `data-root-remove --remove-data` allowlist 삭제, non-allowlist `service-host.log` 보존, cleanup, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore, installed Hyper-V route smoke를 확인했다.
- 첫 `0.30.2-admin-smoke` 시도는 `DeleteService` 이후 service handle을 닫기 전에 missing 대기를 해서 `PCV_HOST_SERVICE_DELETE_FAILED`로 실패했다. Root cause는 `DesktopNodeWindowsServiceController.Delete`에서 `DeleteService(service)` 이후 handle이 열린 상태로 `WaitForMissing`을 호출한 것이었고, `service.Dispose()` 후 missing 대기로 수정했다.
- 이 evidence는 unsigned `AllowUnsignedDev` admin-smoke다. GA-ready aggregate gate, public trusted/stable signing, GA 제품 런타임 승격은 계속 별도 판단이다.
