# P2 Off VM 기능군 Lane 2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** guest OS 없이 검증할 수 있는 P2 기능군(P2-15 NIC/DVD 추가, P2-12 checkpoint schedule, P2-13 export/import, P2-14 network connect)을 설치본 actual-VM으로 검증하고, 그 과정에서 드러난 제품 결함을 Lane 1에서 고친다.

**Architecture:** 한 task가 한 checkpoint다. Lane 1 task는 제품 코드와 runner를 바꾸고, Lane 2 task는 설치본에서 기능군 하나, artifact root 하나, VM root 하나로 runner를 돌린다. current-evidence와 Lane 3 승격은 이 계획 밖이다.

**Tech Stack:** C# / .NET 10 xUnit, PowerShell 7 + Pester 5, PCVCLI, Hyper-V

## 사용자 결정 (2026-09-27)

| 항목 | 결정 |
| --- | --- |
| host mutation | `host mutation ok`: Lane 2 actual-VM 검증용 테스트 VM과 `pcv-` switch 생성·삭제 |
| 범위 | Off VM으로 가능한 기능군부터(P2-15, P2-12, P2-13, P2-14). guest OS가 필요한 P1-8 guest file은 나중 |

## Lane 0 관측 (2026-09-27)

```text
ledger_current=0.42.78-admin-smoke
installed_current=0.42.78-admin-smoke
source_head=986ea3d
```

- 제품 inventory(`vm.list`/`vm get`)는 Off VM의 `state`를 `stopped`로 낸다(`MapEnabledState` `3 => "stopped"`).
- `VmExportImportPolicy`와 `NetworkChangePolicy`는 `PowerState`를 `"Off"`와 정확히 비교한다. `VmDeviceAddPolicy`만 `off`/`stopped`를 둘 다 받는다. clone은 provider가 `stopped`를 `Off`로 바꿔 통과한다.
- Lane 2 진단(evidence 아님): 설치본 `0.42.78`에서 runner가 아닌 일회용 managed Gen2 VM(`pcv-p2-diag-04278-a1`, Hyper-V `Off`, 제품 `stopped`)에 `vm export preview`를 부르면 `PCV_VM_EXPORT_SOURCE_NOT_OFF`다. 파일 write 없음, VM은 제품 delete로 지웠고 VmRoot도 지웠다.
- 따라서 설치본 `0.42.78`에서 P2-13 export와 P2-14 network connect는 Off VM에서도 항상 거절된다. 이 둘의 Lane 2 PASS에는 수정이 든 새 package가 필요하다.

## Global Constraints

- task 하나가 checkpoint 하나다. Lane 1은 30분/tool batch 18회, Lane 2는 45분/tool batch 12회.
- Lane 2는 기능군 하나, artifact root 하나, VM root 하나다. 같은 artifact root나 VM 이름을 재사용하지 않는다.
- runner는 표시 이름 operator id만 쓰고 제품 delete로 정리한다. native `Remove-VM` fallback을 쓰지 않는다.
- current-evidence, feature ledger pass 승격, fullgate, manual-admin pair는 이 계획 밖이다.
- MSI 설치/업그레이드는 host mutation 승인과 별개로 명시 승인을 받는다.
- commit은 campaign `commit_policy`를 따른다. push/PR/merge는 사용자가 부를 때만 한다.

## Task 1: Off 전원 상태 어휘 결함 수정

**수정:** `src/DesktopNode.Contracts/VmExportImportPolicy.cs`, `src/DesktopNode.Contracts/NetworkChangePolicy.cs`, `src/DesktopNode.Contracts/VmDeviceAddPolicy.cs`, 새 `src/DesktopNode.Contracts/VmPowerStates.cs`, 해당 `*Tests.cs`

- [x] `off`와 `stopped`를 Off로 보는 공용 판정 `VmPowerStates.IsOff`를 Contracts에 둔다.
- [x] export/export preview와 VM network connect 정책이 그 판정을 쓰게 한다. device add의 사설 판정도 같은 판정으로 바꾼다.
- [x] 제품 inventory 어휘(`stopped`)로 accept하는 회귀 테스트와 `running`/`saved` 거절 테스트를 더한다.
- [x] `dotnet test src/DesktopNode.Contracts.Tests`, `dotnet test src/DesktopNode.Api.Tests`, 마무리 `dotnet test src/DesktopNode.sln` 실패 `0`.

실행 기록(2026-09-27): `VmPowerStates.IsOff`(`Off`/`stopped`, 대소문자·앞뒤 공백 무시)를 두고 export·export preview, VM network connect, device add가 같이 쓴다. `stopping`, `saved`, `null`은 여전히 거절한다. route 테스트 fake inventory가 `off`만 써서 결함을 가렸으므로 native 어휘 `stopped`로 export preview `200`, network connect `202`를 보는 route 회귀 테스트 두 개를 더했다. 두 정책 파일만 되돌리면 이 두 테스트가 실패(`2/2`)하고 수정 후 통과한다. Contracts.Tests `200/200`, Api.Tests `412/412`, 솔루션 전체는 dirty tree에서만 실패하는 `PolicyBoundaryMatchesCanonicalActivationState` 하나를 빼고 통과했고 그 테스트는 commit 뒤 clean HEAD에서 다시 돌린다. 설치본 `0.42.78`은 이 수정을 담지 않는다.

## Task 2: Off VM 기능군 Lane 2 runner

**생성:** `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1`, `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP2OffVmActualVmSmoke.Tests.ps1`, `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP2OffVmActualVmSmokeContractTests.cs`, 설계 문서

- [x] P1 clone runner 구조(설치본 확인, 이름·경로 가드, atomic summary, RuntimeAdapter, 정확한 identity cleanup)를 따르고 `-Family`로 기능군 하나만 고른다.
- [x] 기능군별 slice를 고정 순서, fail-stop으로 둔다. preview/거절 slice가 mutation slice보다 앞선다.
- [x] DryRun과 RuntimeAdapter Pester, C# 계약 테스트.

실행 기록(2026-09-27): 설계 `docs/superpowers/specs/2026-09-27-purecvisor-desktop-node-p2-offvm-lane2-probe-design.md`. 이 checkpoint 크기에 맞춰 runner `-Family`는 설치본 `0.42.78`에서 돌 수 있는 `device-add`, `checkpoint-schedule` 둘만 연다. export-import, network-connect는 Task 6에서 더한다. 착수 확인: 제품 create는 NIC `1`(Default Switch)과 ISO DVD `1`을 붙이고, inventory `storage`에는 DVD가 없어 route DVD guard가 `0`으로 본다. native `AddDvd`가 `PCV_VM_DEVICE_ALREADY_PRESENT`로 job을 실패시키므로 `dvd_guard`는 route 거절과 job 거절을 모두 받는다(route guard 사각은 report-only). schedule set/clear는 queued job이고 readback은 `vm get`의 `checkpoint_schedule`이다. Pester `8/8`, Delivery.Tests `709/709`. `packaging/windows-desktop-node/tests` 전체에서 `PcvCurrentEvidenceGeneration`(case-only blocked candidate)과 `PcvDevelopmentGateWorkflow`(installer-policy shard step 이름) 두 건이 실패하며, Task 2 파일을 치운 HEAD `9402774`에서도 같은 두 건이 실패하는 기존 baseline 실패다(report-only).

## Task 3: Lane 2 `device-add` (설치본 0.42.78)

- [x] `-Family device-add` 한 run. evidence 문서 새 파일. `overall_verdict`, `cleanup.verdict`, `secret_observed` 기록.

실행 기록(2026-09-27): r1은 `nic_add`에서 runner readback 결함(Hyper-V cmdlet의 process 안 device cache)으로 FAIL, cleanup PASS. 일회용 VM 두 대 진단으로 제품 NIC 추가가 정상임을 확인하고 runner device readback을 `root\virtualization\v2` WMI 직접 조회로 바꿨다(계약 테스트가 cmdlet 재사용을 막음). r2 PASS: `docs/ga-ready/evidence/service-plan-p2-offvm-device-add-actual-vm-2026-09-27-04278-r2.md`. 잔여 VM `0`.

## Task 4: Lane 2 `checkpoint-schedule` (설치본 0.42.78)

- [x] `-Family checkpoint-schedule` 한 run. due tick은 최소 주기 `60`분이라 관측하지 않는다(nonclaim).

실행 기록(2026-09-27): 첫 run PASS. `docs/ga-ready/evidence/service-plan-p2-offvm-checkpoint-schedule-actual-vm-2026-09-27-04278.md`. set 뒤 readback `status=waiting`, clear 뒤 `disabled`, Hyper-V checkpoint `0`. 잔여 VM `0`.

## Task 5: runner `export-import` 기능군 (Lane 1)

순서 조정(2026-09-27): 승인이 필요 없는 runner 확장을 package/설치 승인 task보다 앞에 둔다. switch 생성이
PCVCLI가 아니라 `DesktopNode.Host.exe service-action switch-create|switch-remove`여서 network-connect는 Task 6으로 나눈다.

- [x] runner `-Family`에 `export-import`를 더한다. export 디렉터리는 전용 VmRoot 아래 allowlist root를 쓰고, import VM은 새 identity로 따로 기록해 정리한다.
- [x] Pester와 C# 계약 테스트.

실행 기록(2026-09-27): slice는 `source_create`, `export_confirm_required`, `export_path_not_allowed`, `export_preview`, `export`, `import_preview`, `import`, `cleanup`. 제품 import가 package를 제자리 등록하므로 import VM 예약 root는 `VmRoot\exports`이고 cleanup은 import VM을 소스보다 먼저 지운다. 제품 `vm delete`는 파일을 지우지 않아(`DestroySystem`만) 디렉터리는 runner가 지운다. Pester `11/11`, 계약 테스트 `5/5`. 설치본 `0.42.78`에서는 `export_preview`가 Task 1 결함으로 막히므로 실행은 Task 8이다.

## Task 6: runner `network-connect` 기능군 (Lane 1)

- [x] 전용 Private `pcv-` switch를 `DesktopNode.Host.exe service-action switch-create --switch-type private`로 만들고 cleanup에서 VM 삭제 뒤 `switch-remove`로 지운다. switch도 identity를 기록한다.
- [x] slice: 확인 없는 connect 거절, 없는 switch 거절, connect 뒤 WMI 연결 switch 이름과 제품 network 일치.
- [x] Pester와 C# 계약 테스트.

실행 기록(2026-09-27): 설치본 Host.exe로 `switch-create --dry-run`을 돌려 명령 모양을 확인했다. switch 계열도 `--product-root`, `--service-exe`가 필요하고(없으면 `PCV_HOST_PRODUCT_ROOT_REQUIRED`), 결과 JSON은 `Ok`, `ErrorCode`, `HyperVSwitch{Name, Exists, ProductOwned, AllowManagementOs}`다. dry-run은 switch를 만들지 않았다. PCVCLI와 Host.exe 실행은 `Invoke-PcvProcess` 하나를 쓴다. 설치본 `0.42.78`에서는 VM connect 정책이 switch보다 전원을 먼저 확인해 `connect_switch_missing`부터 `PCV_VM_NETWORK_SOURCE_NOT_OFF`로 막히므로 실행은 Task 8이다. Pester `14/14`.

## Task 7: 수정 package와 설치 (별도 승인)

- [x] Task 1 수정이 든 probe-vehicle package를 빌드하고 설치한다. MSI 설치 승인 없이는 시작하지 않는다.

실행 기록(2026-09-28): 승인 `빌드 + 설치 승인`. HEAD `424b515`에서 `0.42.79-admin-smoke`(MSI `0.42.79`, `AllowUnsignedDev`/`LocalTest`) 빌드, MSI `1cc71d5b…`. msiexec 업그레이드 exit `0`, ARP `0.42.79` 1개, manifest `0.42.79-admin-smoke`, 설치본 CLI/Host hash가 build와 같고 log에 `Won't Overwrite`·`another client` 없음, Web `200`, API `401`. `docs/ga-ready/evidence/admin-smoke-package-2026-09-28-04279.md`. current는 `0.42.78` 유지.

## Task 8: Lane 2 `export-import`, `network-connect` (수정 설치본)

- [x] 기능군마다 한 run. evidence 문서 새 파일.

실행 기록(2026-09-28, network-connect): 승인 `지금 실행`. Host service-action Private switch 생성·삭제는 정상이지만 Private switch가 있으면 `network.inventory`가 `PCV_NATIVE_NETWORK_INVENTORY_TOPOLOGY_INCOMPLETE`로 실패해 `connect_switch_missing`에서 FAIL. cleanup PASS, 잔여 VM·switch `0`. `docs/ga-ready/evidence/service-plan-p2-offvm-network-connect-actual-vm-2026-09-28-04279.md`.

실행 기록(2026-09-28, export-import): 설치본 `0.42.79`에서 Off 어휘 수정은 동작한다(export 확인·거절·preview·job PASS). r1은 runner의 `.vmgs` `0` 가정으로 FAIL, r2는 import job이 `PCV_VM_IMPORT_SECURITY_FEATURES_UNSUPPORTED`로 FAIL이다. import provider가 `.vmgs` 파일 존재를 TPM 신호로 써서 제품이 자기 Gen2 export를 import하지 못한다. `docs/ga-ready/evidence/service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04279-r2.md`. 수정은 보안 guard 변경이라 설계 결정 대기(`new-design-required`). 사용자 결정(2026-09-28): `.vmgs` 파일 검사 제거, planned VM TPM/shielding/key protector 검사 유지, 0.42.80 probe 빌드·설치 뒤 재실행.

## Task 9: import `.vmgs` guard와 Private switch 분류 수정 (Lane 1)

**수정:** `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmImportProvider.cs`, `src/DesktopNode.HyperV/DesktopNodeHyperVWmiSwitchProvider.cs`, 해당 테스트, export/import 설계 문서 결정 문장

- [x] import provider에서 `.vmgs` 파일 존재 거절을 없앤다. planned VM의 `Msvm_SecuritySettingData` 검사(TPM, shielding, key protector 있으면 거절하고 planned VM 삭제)는 그대로 둔다.
- [x] `MapSwitch`가 management port와 external binding이 없는 switch를 `private`, `AllowManagementOs=false`로 분류한다. External 분류는 이 task 밖이다(report-only).
- [x] 회귀 테스트, `dotnet test src/DesktopNode.HyperV.Tests`, 마무리 솔루션 전체.

실행 기록(2026-09-28): package 검사를 `ValidatePackageContent`(OVF/OVA만 거절)로 분리해 temp 디렉터리로 테스트한다(`.vmcx`+`.vmgs` 허용, `.ovf`/`.ova` 거절). `MapSwitch("pcv-lab-private")`는 `private`/`AllowManagementOs=false`이고 native `network.inventory`가 Default+Private 조합에서 `Ok`다. export/import 설계 문서에 2026-09-28 결정 문장을 더했다. HyperV.Tests `225/225`.

## Task 10: 0.42.80 probe-vehicle package와 설치

- [x] Task 9가 든 HEAD에서 `0.42.80-admin-smoke` 빌드, msiexec 업그레이드, package evidence. 승인: 2026-09-28 사용자 결정(0.42.80 probe 빌드·설치).

실행 기록(2026-09-28): HEAD `017c008`, MSI `e28678f2…`. msiexec exit `0`, ARP `0.42.80` 1개, manifest `0.42.80-admin-smoke`, 설치본 CLI/Host hash가 build와 같음, log skip `0`, Web `200`, API `401`. `docs/ga-ready/evidence/admin-smoke-package-2026-09-28-04280.md`.

## Task 11: Lane 2 `export-import`, `network-connect` 재실행 (0.42.80)

- [x] 기능군마다 한 run(각각 checkpoint 하나). evidence 문서 새 파일. (network-connect는 Task 16으로 옮김)

실행 기록(2026-09-28, export-import): `.vmgs` 수정은 동작해 import job이 succeeded다. 그러나 import VM이 구성을 Hyper-V 기본 위치에 두고 **소스 VM의 VHDX**를 가리킨다(package 디스크 미사용). runner는 예약 root 밖 VM을 정리하지 않았고 identity 확인 뒤 제품 delete로 수동 정리했다(잔여 `0`). `docs/ga-ready/evidence/service-plan-p2-offvm-export-import-actual-vm-2026-09-28-04280.md`. import 저장소 처리 설계 결정 대기(`new-design-required`). network-connect 재실행은 아직이다(checkpoint 한도). 사용자 결정(2026-09-28): 전용 root로 복사, 0.42.81 probe 빌드·설치와 재실행 포함. Task 11의 남은 run은 Task 15, 16으로 옮긴다.

## Task 12: import를 전용 VM root 복사로 수정 (Lane 1)

**수정:** `DesktopNodeHyperVWmiVmImportProvider.cs`, `DesktopNodeHyperVModels.cs`, native adapter import, API import route, CLI `vm import`, 테스트, CLI 문서, export/import 설계 결정 문장, module ratchet

- [x] import가 planned VM 디스크를 package `Virtual Hard Disks`에서 `vm_root\<대상>`으로 복사하고 HostResource, 구성·checkpoint·swap 위치를 그 디렉터리로 바꾼다. 실패 시 planned VM 삭제와 복사본 rollback.
- [x] API `vm_root`, CLI `--vm-root`, native 기본값 `D:\PureCVisor\VMs`.
- [x] 테스트와 ratchet.

실행 기록(2026-09-28): 디스크 대응은 순수 함수 `PlanDiskCopies`(package 파일 없음·파일 이름 중복은 `PCV_VM_IMPORT_PACKAGE_INVALID`)로 두고 temp 디렉터리로 테스트했다. 대상 디렉터리가 이미 있으면 `PCV_VM_ALREADY_EXISTS`. 복사와 rollback은 clone의 `CopyVhdx`, `TryRollbackCloneArtifacts`, 경계 검사는 clone guard를 쓴다. 바뀐 세 파일이 ratchet 상한을 넘어(CLI catalog `1418/1412`, models `542/533`, adapter mutations `870/867`) export/import 부분을 partial 파일(`DesktopNodeCliCommandCatalog.ExportImport.cs`, `DesktopNodeHyperVNativeAdapter.ExportImport.cs`, `DesktopNodeHyperVExportImportModels.cs`)로 옮겼고, 50줄 이상 여유가 생긴 두 상한을 새 크기로 낮췄다(`1299`, `763`). fixture pin `8d1da12b` → `f280fe43`, `ExpectedSpecSha256` `0485756a` → `4ec4eec3`.

## Task 13: runner import 검증 갱신 (Lane 1)

- [x] import에 `--vm-root`를 넘기고 import VM 예약 root를 `VmRoot\<import 이름>`으로 둔다. import VM 디스크가 그 root 아래이고 소스 디스크와 다르며 package 디스크가 남아 있는지 WMI로 확인한다.
- [x] 소스 root 삭제 전 다른 VM이 그 아래 디스크를 참조하지 않는지 확인한다.
- [x] Pester와 C# 계약 테스트.

실행 기록(2026-09-28): 제품 import가 대상 디렉터리를 새로 만들어야 하므로 runner는 import root를 만들지 않고 예약만 한다. 디렉터리 삭제는 모두 `Remove-UnreferencedRoot`(host 전체 `Msvm_StorageAllocationSettingData` 참조 검사)를 거친다. 0.42.80 형태(import VM이 Hyper-V 기본 위치에 있고 소스 디스크를 가리킴)를 흉내 낸 Pester에서 cleanup이 `PCV_P2_OFFVM_CLEANUP_ROOT_REFERENCED`로 소스 root를 남긴다. 기존 VM으로 CIM 디스크 readback과 참조 수를 읽기 전용 확인했다. Pester `15/15`, 계약 `6/6`.

## Task 14: 0.42.81 probe-vehicle package와 설치

- [x] Task 12, 13이 든 HEAD에서 `0.42.81-admin-smoke` 빌드, msiexec 업그레이드, package evidence. 승인: 2026-09-28 사용자 결정(전용 root 복사, 0.42.81 빌드·설치).

실행 기록(2026-09-28): HEAD `0c2f84b`, MSI `abf50b21…`. msiexec exit `0`, ARP `0.42.81` 1개, manifest `0.42.81-admin-smoke`, 설치본 CLI/Host hash가 build와 같음, log skip `0`, Web `200`, API `401`. `docs/ga-ready/evidence/admin-smoke-package-2026-09-28-04281.md`.

## Task 15: Lane 2 `export-import` 재실행 (0.42.81)

- [ ] 한 run, evidence 문서 새 파일.

## Task 16: Lane 2 `network-connect` 재실행 (0.42.81)

- [ ] 한 run, evidence 문서 새 파일.
