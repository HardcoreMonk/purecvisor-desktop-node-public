# `vm.list` readback 확장 설계 (DVD media 경로, VHD 크기)

- 작성: 2026-09-30
- 상태: 설계. 구현 승인 전이다.
- 선행: 개발 완료 campaign(`development-completion-20260930`) Task 4·5 실측, PR #22
- 범위 밖: 구현, Lane 2 실행, operational current 변경

## 1. 문제

개발 완료 campaign의 reconcile 작업 중 실측한 결과다.

| 필요한 readback | 현재 `vm.list` | 결과 |
| --- | --- | --- |
| DVD에 붙은 ISO 경로 | 없음. `dvd_drives.count`만 있다 | `vm.attach`/`vm.eject`는 reconcile 비대상(`ReconcileNonTargets`) |
| VHD 크기 | `storage[].size_gb`가 항상 `null`(`DesktopNodeHyperVWmiVmProvider.MapStorage`) | `vm.disk-resize` reconcile은 실제 호스트에서 `409 readback-value-unavailable`만 낸다 |

원인은 두 가지다.

- `GetStorageSummaries`는 `Msvm_StorageAllocationSettingData`를 모두 순회한다. 하지만 `HostResource`를 `IsVhdPath`로 걸러서 ISO 경로를 버린다.
- VHD 크기는 `Msvm_ImageManagementService.GetVirtualHardDiskSettingData`의 `MaxInternalSize`로만 알 수 있다. `vm.list`는 이 호출을 하지 않는다. clone provider(`DesktopNodeHyperVWmiVmCloneProvider`)는 이미 같은 호출로 disk 유형을 검사한다.

## 2. 결정

### 2.1 DVD media: `vm.list`에 `dvd_media`를 더한다

- `GetStorageSummaries`가 `ResourceSubType`을 함께 읽는다.
  - `Microsoft:Hyper-V:Virtual Hard Disk` → 기존 `storage[]`
  - `Microsoft:Hyper-V:Virtual CD/DVD Disk` → 새 `dvd_media[]`
- 새 필드 `dvd_media`는 `[{ "path": "<ISO 경로>" }]`이다. 빈 drive는 항목이 없다. drive 수는 기존 `dvd_drives.count`가 계속 준다.
- `storage[]`의 의미(VHD만)는 바꾸지 않는다. Web, CLI formatter, 자원 reconcile(`FirstAttachedVirtualDiskPath`)이 `storage[]`를 VHD 목록으로 읽기 때문이다.
- 추가 WMI 호출이 없다. 같은 관련 객체 순회에서 subtype 분기만 늘어난다.
- 물리 drive passthrough 같은 다른 subtype은 담지 않는다. reconcile은 그런 VM을 모호함(`409`)으로 본다.

### 2.2 VHD 크기: `vm.list`가 아니라 내부 read operation `vm.disk.inspect`로 읽는다

- `vm.list`에 넣지 않는 이유가 있다. Web polling과 여러 capture가 `vm.list`를 자주 부르는데, 디스크마다 WMI method 호출이 하나씩 붙는다.
- 새 HyperV adapter read operation `vm.disk.inspect`를 둔다. 입력은 `{ "name", "path" }`, 출력은 `{ "path", "max_internal_size_bytes", "disk_type" }`이다. clone provider의 `GetVirtualHardDiskSettingData` 호출과 XML 해석을 공용 helper로 옮겨 함께 쓴다.
- 공개 route는 만들지 않는다. surface ledger와 완료 계약(`ApiSurfaceCompletionContractTests`)은 바뀌지 않는다. reconcile handler만 `operationInvoker.Invoke("vm.disk.inspect", …)`로 부른다.
- `path`는 그 VM의 `storage[]`에 있는 경로여야 한다. provider가 먼저 확인하고, 아니면 `PCV_VM_DISK_NOT_FOUND`를 낸다. 임의 host 파일을 조회하는 통로가 되지 않게 하기 위해서다.
- `storage[].size_gb`는 그대로 `null`로 둔다. 필드를 지우면 API 계약이 바뀌므로 지우지 않는다.

## 3. reconcile 전환 조건

모든 전환은 기존 규칙을 따른다. 대상은 `failed` + `PCV_JOB_INTERRUPTED` job이고, 조건은 before-state 캡처, 단일 VM identity(VM `id`), readback 판정이며, 모호하면 `409`다.

| operation | 캡처 | `succeeded` 조건 | `not-applied` | 그 밖 |
| --- | --- | --- | --- | --- |
| `vm.attach` | before `dvd_media` 경로 집합, 요청 `iso_path` | after에 요청 경로가 정확히 하나 있고 before에는 없었다 | after == before | `ambiguous-media-state` |
| `vm.eject` | before `dvd_media`(비어 있으면 capture `unavailable`, `PCV_VM_MEDIA_NOT_PRESENT`) | after가 before에서 정확히 하나 빠진 부분집합이다 | after == before | `ambiguous-media-state` |
| `vm.disk-resize` | Task 4 캡처에 더해 `vm.disk.inspect`의 before bytes | after bytes == 요청 `disk_gb` × 2^30 | after bytes == before bytes | `incomplete-resource-value` |

- 경로 비교는 대소문자를 무시하고, `Path.GetFullPath`로 정규화한 값으로 한다.
- `vm.attach`/`vm.eject`는 `ReconcileNonTargets`에서 Runtime `ReconcilableMutations`로 옮긴다. `ApiReconcileClassificationTests`의 기대 목록도 함께 바꾼다(대상 `26`→`28`, 비대상 `19`→`17`).
- `vm.disk-resize`는 이미 대상이다. 판정 입력만 `size_gb`(항상 `null`)에서 `vm.disk.inspect` bytes로 바꾼다. 그래서 실제 호스트에서도 성공 판정이 가능해진다.

## 4. 변경 파일 예상

| 층 | 파일 | 변경 |
| --- | --- | --- |
| HyperV | `DesktopNodeHyperVWmiVmProvider.cs` | subtype 분기, `dvd_media` 매핑 |
| HyperV | `DesktopNodeHyperVModels.cs` | `DesktopNodeHyperVVmInfo.DvdMedia`, `DesktopNodeHyperVWmiVmStorageSummary.Subtype`, `vm.disk.inspect` 결과 record |
| HyperV | 새 `DesktopNodeHyperVVirtualDiskInspector.cs` | clone provider에서 옮긴 `GetVirtualHardDiskSettingData` helper |
| HyperV | `DesktopNodeHyperVDomain.cs`, `DesktopNodeHyperVAdapterDispatchCatalog.cs`, `DesktopNodeHyperVWmiProviderCatalog.cs` | `vm.disk.inspect` read operation 등록 |
| Api | `DesktopNodeApiHyperVOperationInvoker.cs` | 허용 operation에 `vm.disk.inspect` |
| Api | `DesktopNodeApiJobReconciliationHandler.Resource.cs`, 새 `.Media.cs`, `.Classification.cs` | 판정 변경, attach/eject reconcile |
| Runtime | `DesktopNodeJobRuntime.Persistence.cs` | `ReconcilableMutations`에 두 줄. 상한 `505`줄은 현재 `504`줄이라, 표를 한 줄 두 항목으로 줄이거나 별도 partial로 옮긴다 |
| Web | `render-vm-detail.ts` | Storage 행에 DVD media 표시(선택) |

모듈 크기 라쳇에 걸리는 파일(`DesktopNodeHyperVWmiVmProvider.cs` `573`줄 등)은 새 partial이나 helper로 나눈다.

## 5. 테스트

- HyperV.Tests: subtype 분기(VHD, ISO, passthrough 무시), `vm.disk.inspect` 경로 소속 확인, clone provider helper 공용화 회귀
- Api.Tests: attach/eject/disk-resize reconcile의 성공, `not-applied`, 모호, baseline 없음, readback 실패, 분류 계약 갱신
- 계약: adapter catalog 세 곳(domain, dispatch, provider)이 같은 operation 집합인지 확인하는 기존 테스트
- Web: DVD media 표시를 넣으면 browser fixture VM에 `dvd_media`를 더한다

## 6. Lane 2 확인 방법

설치본에서 probe VM 하나로 다음을 확인한다.
1. ISO를 attach하고 `vm.list`의 `dvd_media`를 본다.
2. eject 뒤 `dvd_media`가 비었는지 본다.
3. disk-resize 전후의 `vm.disk.inspect` bytes를 본다.

실제 interrupt는 서비스 중단 시점을 맞추기 어려우므로 판정 로직은 job store fixture 테스트가 맡는다. Lane 2는 readback이 실제 값과 맞는지만 증명한다.

## 7. 위험과 비주장

- `dvd_media`는 새 필드라서 기존 소비자에게 영향이 없다. 외부 script가 `vm.list` JSON schema를 엄격하게 검사한다면 알려야 한다.
- `vm.disk.inspect`는 VHD 파일을 열지 않고 WMI만 조회한다. 사용 중인 VHD에도 쓸 수 있는지는 Lane 2에서 확인한다.
- 이 설계는 Hyper-V exactly-once나 reconcile 완전성을 주장하지 않는다. 관찰할 수 없는 부작용은 계속 비대상이다.
