# Lane 2 vm.list readback actual-VM `0.42.85-admin-smoke` (2026-10-01)

evidence_id: `lane2-vm-list-readback-actual-vm-2026-10-01-04285`
result: `PARTIAL`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.85-admin-smoke`
installed_product_version: `0.42.85-admin-smoke+62a0a1e32e3a8686eedc6e0a3da1406f6c0f3e80`
probe_artifact_root: `artifacts/lane2-vm-list-readback-actual-vm-20261001-04285`
diagnosis_artifact_root: `artifacts/lane2-vm-eject-diagnosis-20261001-04285`
probe_vm: `pcv-lane2-readback-1001`
diagnosis_vm: `pcv-lane2-eject-1001`
new_feature_checks: `PASS`
preexisting_defects_confirmed: `vm.eject-modify-empty-host-resource`, `vm.attach-empty-drive-not-found`
token_value_observed: `false`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

설치본 `0.42.85`에서 이번 개발분을 실제 VM으로 확인했다. 대상은 다음과 같다:

- `vm list`의 `dvd_media` readback
- media job의 capture baseline(`before_media`)
- disk-resize capture의 byte 단위 `before_value`(`vm.disk.inspect`)
- interrupt된 media job의 reconcile 판정

CLI는 보호된 token 파일(`--protected-token-file`)로 인증했다. token 값은 출력하지 않았다.

## probe (`15:54:30Z`–`15:54:54Z`)

VM은 서버 ISO, CPU `2`, memory `2048` MB, disk `20` GB로 만들었다. VM은 끝까지 켜지 않았다(`stopped`).

| 확인 | 관측 | 판정 |
| --- | --- | --- |
| 생성 뒤 `dvd_media` | `[ubuntu-26.04-live-server-amd64.iso]` | `PASS` |
| eject job | `failed`, `PCV_HYPERV_WMI_JOB_FAILED` ("리소스를 수정하지 못했습니다"), media 그대로 | `FAIL` (기존 결함, 아래) |
| eject capture | `before_media=[서버 ISO]`, `capture_status=captured` | `PASS` |
| attach(데스크톱 ISO) job | `succeeded` | `PASS` |
| attach capture | `before_media=[서버 ISO]`, `expected_after.iso_path=데스크톱 ISO` | `PASS` |
| attach 뒤 `dvd_media` | `[ubuntu-26.04-desktop-amd64.iso]` | `PASS` |
| disk-resize `20 -> 24` GB | job `succeeded` | `PASS` |
| resize capture `before_value` | `21474836480` = `Get-VHD` 크기 = `20 * 2^30` | `PASS` |
| resize 뒤 VHD 크기 | `25769803776` = `24 * 2^30` | `PASS` |

### interrupt reconcile

서버 ISO로 `vm.attach` job을 queue했다. job이 `running`인 것을 본 직후 `DesktopNode.Host` 프로세스를 강제 종료했다(`15:54:45Z`). 서비스가 다시 올라온 뒤의 관측은 다음과 같다:

- job `failed`, `PCV_JOB_INTERRUPTED`
- 저장된 capture: `before_media=[데스크톱 ISO]`, `expected_after.iso_path=서버 ISO`
- 실제 readback `dvd_media=[데스크톱 ISO]`. attach는 적용되지 않았다.
- `job reconcile`: HTTP 409 `PCV_JOB_RECONCILIATION_REQUIRED`, classification `not-applied`
- reconcile 뒤 job은 `failed`로 남는다.

readback이 capture와 같으므로 `not-applied`가 맞다. `not-applied`에 409를 주고 job을 `failed`로 두는 것은 wave2c reconcile 설계 표(rename, delete, checkpoint)와 같은 규칙이다.

## eject 진단 (`16:16:39Z`–`16:16:54Z`)

eject 실패는 이번 개발분과 관계없다. `service-plan-p0-actual-vm-2026-08-20-04274`(`0.42.74`)에 같은 `PCV_HYPERV_WMI_JOB_FAILED`가 "noted"로 이미 기록돼 있다. 원인을 좁히려고 probe VM 하나를 새로 만들어 CIM으로 확인했다.

| 단계 | CIM / readback |
| --- | --- |
| 생성 뒤 | drive `Msvm_ResourceAllocationSettingData` "Synthetic DVD Drive" 1개. media `Msvm_StorageAllocationSettingData` "Virtual CD/DVD Disk" 1개(`HostResource=[서버 ISO]`, Parent=drive) |
| 제품 `vm eject` | job `failed`, `PCV_HYPERV_WMI_JOB_FAILED`. media 그대로 |
| 진단용 CIM `RemoveResourceSettings`(media SASD) | ReturnValue `0`. media `0`개, drive 1개는 남음 |
| 그 뒤 `vm list` | `dvd_media=[]`, `dvd_drives.count=1` (readback 정확) |
| 빈 drive에 제품 `vm attach` | job `failed`, `PCV_VM_DVD_DRIVE_NOT_FOUND` |

진단용 `RemoveResourceSettings`는 이 probe VM에만 호출했다. 제품 코드는 바꾸지 않았다.

### 원인

`DesktopNodeHyperVWmiVmMediaProvider`의 `FindDvdDrive`는 drive가 아니라 media SASD("Virtual CD/DVD Disk")를 찾는다. 두 결함이 여기서 나온다:

1. eject: media SASD에 `HostResource=[]`를 넣고 `ModifyResourceSettings`를 부른다. Hyper-V는 이를 거부한다. media를 빼려면 media SASD에 `RemoveResourceSettings`를 불러야 한다. 진단에서 이 방식이 ReturnValue `0`으로 동작했다.
2. attach: media가 없는 drive를 찾지 못한다. 그래서 eject에 성공한 뒤의 drive나 `vm device add --kind dvd`로 만든 빈 drive에는 attach할 수 없다. 이 경우 drive RASD를 Parent로 하는 새 media SASD를 `AddResourceSettings`로 추가해야 한다.

`AddDvd`의 "기존 drive" 확인도 같은 `FindDvdDrive`를 쓴다. 그래서 빈 drive가 있으면 두 번째 drive를 추가하려 할 수 있다. 이것은 추론이며 실행해 확인하지 않았다.

수정에 새 `Add-Type`/P/Invoke는 필요 없다(`System.Management`로 충분하다). 하지만 제품 코드 수정과 새 package가 필요하므로 이 campaign(0.42.85 pair)의 범위 밖이다.

## 정리

두 VM 모두 제품 `vm delete --yes`로 지웠다. 그 뒤 남은 VM 디렉터리(`D:\PureCVisor\VMs\<vm>`)도 지웠다. 끝난 뒤 상태는 다음과 같다:

- VM은 `pcv-guest-installed-04253-r1`(Off)만 남았다.
- 서비스 Running, Web `200`
- product manifest는 `0.42.85-admin-smoke` 그대로다.

## 판정

이번 개발분은 실제 VM에서 모두 기대대로 동작했다:

- `dvd_media` readback
- media/resource capture baseline
- byte 단위 `before_value`
- `not-applied` reconcile

eject는 `0.42.74`부터 있던 결함 때문에 실패한다. 이번 진단에서 원인과 수정 방향을 확인했다. 그래서 결과는 `PARTIAL`이다.

## Nonclaims

- operational current는 `0.42.84-admin-smoke`다. Lane 3 승격은 이 campaign의 승인 밖이다.
- eject와 빈 drive attach는 고치지 않았다. 수정은 별도 승인이 필요하다.
- public trusted signing과 external stable publication을 주장하지 않는다.
