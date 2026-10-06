# 끊긴 vm.create의 고아 디스크 처리 설계

- Design-ID: `pcv-interrupted-create-residue-v1`
- 작성일: `2026-10-06`
- 문서 상태: `proposed` (다음 train 후보, 구현 전)
- 변경 등급: M (Hyper-V provider 동작, reconcile 안내 문구)
- host/VM/service/package mutation: `false` (이 설계 checkpoint)
- 출처: train `0.42.91` Task 8 probe P1-10(`lane2-completion-family-reconcile-actual-vm-2026-10-06-04291`), 사용자 승인 `1,2`(2026-10-06)의 2

## 1. 문제

`vm.create` provider(`DesktopNodeHyperVWmiVmCreateProvider.Invoke`)는 `<vm_root>\<name>` 폴더를 만들고
`CreateVirtualHardDisk`로 `disk0.vhdx`를 만든 뒤 `DefineSystem`으로 VM을 등록한다. 실패 정리(`Cleanup`)는 같은
프로세스 안의 예외(`DesktopNodeHyperVNativeOperationException`, `OperationCanceledException`, 그 밖의 예외)에서만
돈다. 서비스 프로세스가 끊기면 정리가 돌지 않는다.

Task 8 r2·r3에서 본 순서:

1. running 중인 `vm.create`를 서비스 프로세스 종료로 끊었다. VM은 등록되지 않았고 `D:\PureCVisor\VMs\<name>\disk0.vhdx`(4 MB)가 남았다.
2. `job reconcile`은 `vm.list`에 VM이 없으므로 `not-applied`로 판정했다. 남은 디스크는 언급하지 않는다.
3. 같은 이름의 다음 `vm.create`는 `CreateVirtualHardDisk` 앞의 존재 검사에서 `PCV_VHD_ALREADY_EXISTS`로 막혔다.
   운영자가 파일을 손으로 지워야 했다.

managed delete는 0.42.88 package부터 VM 전용 디렉터리의 디스크를 지운다(`pcv-managed-delete-storage-cleanup-v1`). 그러나
VM이 등록되기 전에 끊긴 create는 지울 VM이 없어 그 경로로도 정리되지 않는다.

## 2. 결정

### 2.1 소유 표식

create provider는 `disk0.vhdx`를 만들기 전에 `<vm_root>\<name>\.pcv-create-pending.json`을 쓴다. 이 파일이 "이
폴더의 `disk0.vhdx`는 PureCVisor create가 만들었다"는 유일한 증거다.

| 필드 | 값 |
| --- | --- |
| `schema` | `pcv-vm-create-pending/v1` |
| `vm_name` | 요청 VM 이름 |
| `vhd_file` | `disk0.vhdx` (폴더 기준 상대 이름) |
| `directory_created` | 이 create가 폴더를 만들었으면 `true` |
| `owner_process_id`, `owner_process_start_utc` | 쓰는 서비스 프로세스의 id와 시작 시각(ISO 8601 UTC) |
| `created_utc` | 표식을 쓴 시각 |

- 성공한 create는 마지막 단계에서 표식을 지운다. 표식 삭제 실패는 create를 실패로 만들지 않고 결과 `steps`에 남긴다.
- 같은 프로세스 안의 실패 정리(`Cleanup`)는 지금처럼 폴더나 디스크를 지우고, 표식도 함께 지운다.
- 표식에는 token, credential, password, 사용자 입력 경로 밖의 값을 쓰지 않는다.

### 2.2 같은 이름의 다음 create에서 회수

`FindVm`이 VM 없음을 확인한 뒤, 폴더에 `disk0.vhdx`나 표식이 있으면 회수 판정을 한다. 판정은 아래 순서이고, 하나라도
걸리면 아무것도 지우지 않고 지금처럼 `PCV_VHD_ALREADY_EXISTS`로 거절한다. 거절 detail에 이유 코드를 붙인다.

| 순서 | 조건 | 거절 이유 코드 |
| --- | --- | --- |
| 1 | 표식이 없다 (사용자가 둔 파일일 수 있음) | `no-create-marker` |
| 2 | 표식을 읽을 수 없거나 `schema`, `vm_name`, `vhd_file`이 맞지 않는다 | `marker-invalid` |
| 3 | 표식 주인 프로세스(id와 시작 시각이 같은 프로세스)가 살아 있거나, 살아 있는지 판정하지 못했다 | `create-in-progress` |
| 4 | 남은 VM이나 checkpoint가 `disk0.vhdx`를 참조한다 | `referenced-by-vm` |
| 5 | 참조 조회가 실패했다 | `reference-check-failed` |
| 6 | 파일 삭제가 실패했다 (VMMS가 아직 `CreateVirtualHardDisk` job으로 파일을 잡고 있는 경우 포함) | `delete-failed` |

모두 통과하면 `disk0.vhdx`와 표식을 지우고, `directory_created=true`이고 폴더가 비었을 때만 폴더도 지운 뒤 정상 create를
이어 간다. 디스크가 아닌 다른 파일은 지우지 않는다. 회수 결과는 create 결과의 `steps`에
`Remove interrupted create residue`로, `recovered_residue`(`removed_files`, `removed_directory`)에 남는다.

- 프로세스 판정은 `System.Diagnostics.Process`의 id와 `StartTime`으로 한다. 새 `Add-Type`/P/Invoke/native ACL은 없다.
  `StartTime` 읽기가 예외를 내면 "판정 못 함"으로 보고 거절한다(fail-closed).
- 참조 조회는 managed delete 정리(`DesktopNodeHyperVVmStorageCleanup`)가 쓰는 남은 VM·checkpoint의
  `Msvm_StorageAllocationSettingData` `HostResource` 읽기를 같이 쓴다.
- 판정은 provider 밖의 순수 class(`DesktopNodeHyperVVmCreateResidue`)가 파일 시스템 interface와 프로세스 판정
  interface로 한다. Required CI는 Hyper-V를 돌리지 않으므로 가짜 파일 시스템과 가짜 프로세스 판정으로 시험한다.

### 2.3 등록 뒤 끊긴 create

`DefineSystem` 뒤에 끊기면 VM이 남으므로 다음 create는 지금처럼 `PCV_VM_ALREADY_EXISTS`로 거절한다. 운영자는 managed
delete로 지우고, managed delete 정리는 `ConfigurationDataRoot` 안의 디스크와 함께 `.pcv-create-pending.json`도 지운다.
이 파일 하나만 디스크가 아닌 파일 중 예외로 지운다. 그 밖의 비디스크 파일은 지금처럼 남긴다(`directory-not-empty`).

### 2.4 reconcile 안내

`vm.create` reconcile은 지금처럼 provider `vm.list` readback만 한다. 파일 시스템을 읽거나 지우지 않는다.
`not-applied` 판정의 hint에 다음 뜻의 문구를 더한다: "VM 등록 전에 끊긴 create는 `<vm_root>\<name>`에 디스크를 남길 수
있다. 같은 이름의 다음 `vm.create`가 PureCVisor 표식이 있는 잔여물을 지우고 진행한다. 표식이 없으면 파일을 확인해
손으로 지운다." `<vm_root>`는 job 파라미터 `vm_root`가 있으면 그 값, 없으면 기본 VM root다.

## 3. 버린 선택지

| 안 | 버린 이유 |
| --- | --- |
| reconcile이 잔여물을 지움 | reconcile은 readback 판정만 하고 mutation을 하지 않는다는 경계가 깨진다 |
| 서비스 시작 때 고아 폴더 sweep | 운영자 동작 없이 디스크를 지운다. 끊긴 create가 없는 시작에서도 VM root 전체를 훑는다 |
| 표식 없이 "VM 없음 + `disk0.vhdx` 있음"이면 지움 | 사용자가 둔 디스크나 다른 도구의 파일을 지울 수 있다 |
| reconcile에 provider 잔여물 readback operation 추가 | 내부 operation 목록과 catalog/parity 계약이 넓어진다. 안내 문구와 다음 create 회수로 운영자 수작업이 없어진다 |
| `--force`/`--reuse-disk` 같은 opt-in | CLI/API/Web 계약이 넓어지고, 4 MB 빈 디스크를 재사용할 이유가 없다 |
| job id를 표식에 쓰고 job store 상태로 판정 | provider(HyperV)가 job store(Runtime/Api)를 알아야 한다. 프로세스 id와 시작 시각으로 "끊긴 주인"을 판정할 수 있다 |

## 4. 시험과 확인

- `DesktopNode.HyperV.Tests`: 회수 판정 여섯 거절 이유와 회수 성공(`directory_created` 참·거짓), 성공 create가 표식을
  지움, 같은 프로세스 실패 정리가 표식을 지움, managed delete 정리가 표식을 지우고 다른 비디스크 파일은 남김.
- `DesktopNode.Api.Tests`: `vm.create` reconcile `not-applied` hint 문구와 `vm_root` 반영. reconcile wording을 pin한 곳이
  있으면 같은 변경 묶음에서 바꾼다.
- 다음 train의 Lane 2 probe: probe VM `pcv-probe-c4-*`로 running 중 서비스 프로세스를 끊어 `vm.create`를
  `PCV_JOB_INTERRUPTED`로 만들고, reconcile `not-applied`와 hint를 확인한 뒤, 같은 이름 create가 `recovered_residue`를
  남기고 성공하는지, managed delete 뒤 폴더가 없는지 본다. host mutation은 probe VM 생성·삭제와 서비스 재시작이다.

## 5. 다음 train 적재

- 구현은 Lane 1 checkpoint 하나(provider, 회수 class, delete 정리 예외, reconcile 문구, 시험)로 하고 PR로 merge한다.
  merge된 PR은 `docs/ga-ready/release-train.json` `queue` 행이 되어 다음 출발 train에 탄다.
- 구현 승인 문장: "Lane 1, host mutation 없음, `pcv-interrupted-create-residue-v1` 구현과 시험, push, PR, green CI 뒤
  merge. 실제 확인은 다음 train의 Lane 2 probe(probe VM 생성·삭제, 서비스 재시작)."

## 6. 범위 밖 관측 (report-only)

- `DefineSystem` 뒤 장치 연결 전에 끊긴 create는 managed 표식과 generation만 맞으면 reconcile이
  `postcondition-confirmed`로 판정한다(`CreateFingerprintMatches`). 디스크, ISO, `Default Switch` 연결은 지문에 없다.
  이 설계는 이를 바꾸지 않는다. 바꾸려면 create 지문에 연결 장치를 넣는 별도 설계가 필요하다.
- Task 8 report-only였던 QoS readback `mutation_supported: false`도 이 설계 범위 밖이다.

## 7. 비목표

- 실제 VM에서의 확인은 다음 train의 Lane 2가 맡는다.
- `vm.import`, `vm.clone`처럼 다른 경로가 만든 폴더의 잔여물은 다루지 않는다.
- public trusted signing과 external stable publication을 주장하지 않는다.
