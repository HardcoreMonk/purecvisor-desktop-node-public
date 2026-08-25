# Functional Correctness SUSPECTED Runtime Verification Results

- 검증일: 2026-07-15 (Asia/Seoul)
- 대상 기준선: `main` / `999f4efaf1c2894edef049b6fb1529c23f5b7bd1`
- 대상 호스트: `AMD_5800X` / `[redacted-private-endpoint]` / Windows 11 / Hyper-V
- 검증 범위: FC-01, FC-02, FC-04, FC-05, FC-12(b), FC-13, FC-16, FC-18
- 변경 범위: 제품 코드 변경 없음. 이 결과 문서만 추가함.

## 1. 선행 문서 접근 상태

지시서가 지정한 `codex/audit-docs-2026-07-15` / `593bdea7`과 다음 두 문서는 검증 시점의
로컬 객체, 로컬/원격 ref, `origin`, GitHub commit API에서 찾을 수 없었다.

- `docs/functional-correctness-audit-2026-07-15.md`
- `docs/security-architecture-audit-2026-07-15.md`

따라서 첨부된 작업 지시서의 발견 설명과 `main`의 현재 소스를 실측 기준으로 사용했다. 이
제약은 아래 판정에서 정적 감사 원문을 읽었다는 주장으로 대체하지 않는다.

## 2. 최종 판정

| ID | 판정 | 근거(명령 출력 요약) | 조건/비고 |
|---|---|---|---|
| FC-01 | **조건부** | 100GB 동적 VHDX의 최대 NTFS 파티션을 8GB로 `Resize-VHD` 했으나 `0xC03A0025`로 거부됐다. 전후 `Size=107374182400`, `MinimumSize=107373150720`로 동일했다. | 이 호스트와 해당 fully-partitioned VHDX 조건에서는 truncation이 재현되지 않았다. 제품 코드의 사전 축소 가드 부재는 남아 있으므로 다른 VHD 레이아웃까지 일반화한 REFUTED 판정은 하지 않는다. |
| FC-02 | **CONFIRMED** | 격리 `ProductRoot.previous`의 실행 파일 핸들을 열어 두고 rollback을 실행했다. `PCV_PRODUCT_RESTORE_FAILED` 후 현재 payload는 `ProductRoot.failed`로 이동했고 `ProductRoot`는 빈 디렉터리, `.previous`는 이전 payload를 보존했다. | 첫 이동 성공 후 두 번째 이동 실패가 원상 복구되지 않아 원자적 rollback이 아니다. |
| FC-04 | **CONFIRMED** | 격리 VM에 제품 API로 `maximum_kbps=2048`을 적용한 job이 성공했다. `Get-VMNetworkAdapter`는 `MaximumBandwidth_bps=2048`, WMI는 `Limit=2048`을 반환했다. | 정상 기대는 2,048,000 bps이다. Kbps 값이 변환 없이 bps 필드에 들어가는 1000배 단위 오류다. |
| FC-05 | **미검증** | PowerShell Direct를 실행할 부팅 가능한 격리 Windows guest와 전용 guest credential이 준비되지 않았다. | 운영/기존 VM과 자격증명을 사용하지 않는 안전 규칙에 따라 skip했다. |
| FC-12(b) | **미검증** | 비 ASCII stdin/credential을 검증할 부팅 가능한 격리 guest와 전용 credential이 준비되지 않았다. | 운영/기존 VM과 자격증명을 사용하지 않는 안전 규칙에 따라 skip했다. |
| FC-13 | **미검증** | 제품 Gen2 create 경로에 제공할 부팅 가능한 throwaway ISO가 호스트의 지정 후보 경로에 없었다. | 빈 파일을 부팅 ISO로 가장하지 않았으며 기존 ISO/VM을 사용하지 않았다. |
| FC-16 | **CONFIRMED** | 격리 update에서 현재 payload 실행 파일 핸들로 이동 실패를 강제했다. journal은 `stage=backup-product-root`, `status=failed`, `PCV_PRODUCT_UPDATE_BACKUP_FAILED`, rollback 미시도였다. 기존 `.previous` marker는 삭제됐고 `.previous`는 파일 0개 빈 디렉터리였다. | 기존 backup을 먼저 삭제한 후 새 backup 이동이 실패해 사용 가능한 이전 backup이 사라진다. |
| FC-18 | **CONFIRMED** | 유효한 최신 run만 있을 때 `latest=pcv-verify-new-run`이었다. 24시간 더 오래된 reparse-point `summary.json`을 추가하자 `status=unavailable`, `latest=null`, `PCV_BATCH_EVIDENCE_REPARSE_POINT_REJECTED`가 됐다. | unreadable 경로의 sort time이 `DateTime.MaxValue`가 되어 유효한 최신 run을 가렸다. |

## 3. P0 사전 점검

| 항목 | 결과 |
|---|---|
| 대상 호스트 | 로컬 호스트의 LAN 주소가 `[redacted-private-endpoint]`임을 확인 |
| 실행 계정 | `AMD_5800X\\Operator`, Administrators 그룹, elevated `True` |
| .NET SDK | `10.0.204`, `10.0.302` |
| 솔루션 | `src/DesktopNode.sln` 존재 |
| Hyper-V | `Win32_OptionalFeature.InstallState=1`, `vmms` Running/Automatic |
| Hyper-V 직접 동작 | VHD create/mount/format/resize, throwaway VM create/remove 모두 동작 |
| 제한 | `Get-WindowsOptionalFeature`와 최초 `Get-VMHost` 호출은 시간 제한을 넘겼다. 대체 CIM/service 증거와 실제 Hyper-V 작업 성공으로 기능 상태를 확인했다. |

## 4. Task A — restore/build/test

실행 명령:

```powershell
dotnet restore src\DesktopNode.sln
dotnet build src\DesktopNode.sln -c Release --no-restore
dotnet test src\DesktopNode.sln -c Release --no-build --logger "console;verbosity=minimal"
```

결과:

- restore: 성공
- Release build: 성공, 경고 0, 오류 0
- test: 실패 0, 통과 582, 건너뜀 0
  - `DesktopNode.Runtime.Tests`: 17
  - `DesktopNode.Service.Tests`: 11
  - `DesktopNode.Contracts.Tests`: 15
  - `DesktopNode.Cli.Tests`: 112
  - `DesktopNode.Host.Tests`: 150
  - `DesktopNode.Api.Tests`: 277
- `DesktopNode.HyperV`는 솔루션에 포함되지만 `DesktopNode.HyperV.Tests` 프로젝트는 없다.
- 원시 console 출력은 검증 작업 기록에 보존됐으며 본 문서에는 성공/실패 수와 핵심 출력을 요약했다.

## 5. Task B — read-only WMI 메타데이터

### B1 / FC-04

호스트의 `Msvm_EthernetSwitchPortBandwidthSettingData`에서 `Limit`과 `Reservation`은
`UInt64`, read/write로 확인됐다. 로컬 amended description은 각각 bandwidth limit와 minimum
absolute bandwidth를 설명하지만 Units qualifier는 제공하지 않았다. 로컬 `Get-Help`도 단위 설명이
비어 있었다.

Microsoft의 `Set-VMNetworkAdapter -MaximumBandwidth` 문서는 값을 bits per second로 정의한다.
제품 소스는 `maximum_kbps`와 `minimum_kbps`를 각각 WMI `Limit`과 `Reservation`에 변환 없이
대입한다. 최종 단위 판정은 C2의 실제 readback으로 확정했다.

- Microsoft 문서: <https://learn.microsoft.com/en-us/powershell/module/hyper-v/set-vmnetworkadapter?view=windowsserver2025-ps>
- 코드: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmResourceMutationProvider.cs`의 network QoS apply/CreateBandwidthFeature 경로

### B2 / FC-03 보강

`Msvm_VirtualSystemSettingData` 실측 결과:

- 대상 system setting 수: 1
- `InstanceID`에 `\Realized`가 포함된 수: 0
- `VirtualSystemType=Microsoft:Hyper-V:System:Realized`: 1
- 샘플 `InstanceID`: `Microsoft:030CCF40-0E00-4227-87AB-7BB4B83C3066`

즉 realized 여부는 `VirtualSystemType`에 있고 `InstanceID` substring에는 없었다.

## 6. Task C — throwaway Hyper-V 검증

모든 리소스는 `D:\pcv-verify-20260715-codex-01`과
`pcv-verify-20260715-qos01` 아래에서만 생성했다.

### C1 / FC-01

1. `New-VHD -SizeBytes 100GB -Dynamic`
2. disk 5로 mount, GPT/NTFS 최대 파티션 생성
3. dismount 후 `Resize-VHD -SizeBytes 8GB`
4. Hyper-V가 데이터 잘림 위험으로 작업을 거부

핵심 출력:

```text
RESIZE_OUTCOME=RESIZE_REJECTED
... 사용자 데이터가 잘릴 수 있으므로 ... (0xC03A0025)
BEFORE Size=107374182400 MinimumSize=107373150720
AFTER  Size=107374182400 MinimumSize=107373150720
```

### C2 / FC-04

- raw Hyper-V로 Gen2/NoVHD/Default Switch throwaway VM 생성
- source-built isolated host를 `127.0.0.1:17777`에서 별도 process로 실행
- 제품 API `vm.qos.network.set` 사용
- `maximum_kbps=2048, minimum_kbps=256` 요청은 Hyper-V `0x80070057`로 실패
- `maximum_kbps=2048` 요청은 성공
- readback: `MaximumBandwidth_bps=2048`, WMI `Limit=2048`, `Reservation=0`

## 7. Task D — 격리 설치 복사본

라이브 설치 루트 `C:\Program Files\PureCVisor\DesktopNode`는 read-only source로만 사용했다.
8개 파일, 약 144.23MB, version `0.42.63-admin-smoke`를 다음 격리 경계에 복사했다.

```text
D:\pcv-verify-20260715-taskd-01\fc02\ProductRoot
D:\pcv-verify-20260715-taskd-01\fc02\ProductRoot.previous
D:\pcv-verify-20260715-taskd-01\fc16\ProductRoot
D:\pcv-verify-20260715-taskd-01\fc16\ProductRoot.previous
D:\pcv-verify-20260715-taskd-01\fc16\UpdatePayload
```

서비스 stop/status/start는 외부 process를 호출하지 않는 `pcv-verify-noop` scriptblock으로 대체했다.
따라서 live service control은 실행되지 않았다.

### FC-02

`RestorePreviousProductRoot` 기본 경로는 현재 root를 `.failed`로 먼저 이동한 뒤 previous를 현재
root로 이동한다. 두 번째 이동 실패를 열린 파일 핸들로 강제했다.

```text
ResultOk=False
ErrorCode=PCV_PRODUCT_RESTORE_FAILED
ErrorDetail=The process cannot access the file because it is being used by another process.
ProductRoot: exists, file count 0
ProductRoot.failed: file count 9, current marker present
ProductRoot.previous: file count 9, previous marker present
```

### FC-16

첫 Delete ACL 주입은 directory move를 차단하지 못해 판정에서 제외했다. 격리 상태를 복구한 뒤
현재 root의 실행 파일 핸들을 열어 표적 재검증했다.

```text
ResultOk=False
ErrorCode=PCV_PRODUCT_UPDATE_BACKUP_FAILED
Journal stage=backup-product-root
Journal status=failed
RollbackAttempted=False
ProductRoot: current marker present
ProductRoot.previous: exists but file count 0
Previous marker: absent
```

기존 previous 삭제와 새 backup 이동 사이에 복구 단계가 없으므로 이전 backup 축출이 확정됐다.

## 8. Task E — crafted evidence root

제어군:

```text
status=degraded
latest.batch_id=pcv-verify-new-run
```

실험군은 24시간 더 오래된 target을 가리키는 reparse-point `summary.json`을 추가했다.

```text
Attributes=Archive, ReparsePoint
status=unavailable
latest=null
error=PCV_BATCH_EVIDENCE_REPARSE_POINT_REJECTED
```

`BatchEvidenceSummaryReader.GetEvidenceSummarySortTime`이 unreadable path에
`DateTime.MaxValue`를 부여하는 코드 경로와 실측 결과가 일치한다.

## 9. Cleanup 및 비변경 확인

### Task C/E

- `D:\pcv-verify-20260715-codex-01`: 삭제 확인
- `pcv-verify-20260715-qos01`: VM count 0
- shrink VHDX: 삭제 확인
- isolated host process: 0
- port 17777 listener: 없음

### Task D

- 삭제 전 root 절대 경로가 `D:\pcv-verify-20260715-taskd-01`과 정확히 일치함을 확인
- parent `D:\`, leaf `pcv-verify-*` 확인
- 약 721.34MB 격리 복사본 전체 삭제
- 삭제 후 root 존재 `False`

### 라이브 상태

- `PureCVisorDesktopNode`: Running / Automatic
- live product root를 이동·수정·삭제하지 않음
- 기존 운영 VM을 시작·중지·수정하지 않음
- 자격증명 수집/추측 없음
- remote push/merge 없음

## 10. 관련 코드 위치

- FC-01: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmResourceMutationProvider.cs` — `ResizeDisk`
- FC-02: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` — 기본 `RestorePreviousProductRoot`
- FC-04: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmResourceMutationProvider.cs` — `SetNetworkQos`, `CreateBandwidthFeature`
- FC-13: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmCreateProvider.cs` — `AttachDiskAndDvd`
- FC-16: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1` — 기본 `BackupProductRoot`
- FC-18: `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs` — `ResolveLatestRunRoot`, `GetEvidenceSummarySortTime`
