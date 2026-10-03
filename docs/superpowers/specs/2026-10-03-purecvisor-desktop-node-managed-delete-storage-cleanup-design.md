# Managed delete 디스크 정리 설계

- Design-ID: `pcv-managed-delete-storage-cleanup-v1`
- 작성일: `2026-10-03`
- 문서 상태: `implemented` (소스, 설치 반영은 다음 package pair)
- 변경 등급: M (Hyper-V provider 동작)
- host/VM/service/package mutation: `false` (이 설계와 구현 checkpoint)

## 1. 문제

`pcvcli vm delete --yes`(managed delete, WMI `DestroySystem`)는 VM 정의만 지우고 디스크와 VM 디렉터리를 남긴다. 0.42.84 이후 Lane 2 probe마다 `D:\PureCVisor\VMs\<name>\disk0.vhdx`를 손으로 지웠다(`lane2-development-completion-actual-vm-2026-09-30-04284`, `lane2-reconcile-wording-actual-vm-2026-10-03-04287`). 문서에는 디스크를 남긴다는 계약이 없다.

## 2. 결정

`vm.create`는 managed VM마다 전용 디렉터리 `<vm_root>\<name>`을 만들고 `ConfigurationDataRoot`와 `SnapshotDataRoot`로 쓴다. 그래서 VM이 사라진 뒤 그 디렉터리 안 디스크 이미지는 주인이 없다.

1. delete provider는 `DestroySystem` 전에 realized settings의 `ConfigurationDataRoot`와 연결 디스크 경로를 읽는다.
2. `DestroySystem` 뒤 남은 모든 VM과 checkpoint의 디스크(`Msvm_StorageAllocationSettingData` `HostResource`)를 읽는다.
3. `ConfigurationDataRoot` 안의 `.vhdx`/`.vhd`/`.avhdx`/`.avhd` 중 남은 VM이 참조하지 않는 것만 지운다. checkpoint 차등 디스크도 같은 디렉터리에 있으므로 함께 지워진다.
4. 빈 하위 디렉터리와, 비면 root를 지운다. 디스크가 아닌 파일은 지우지 않는다.
5. 지키는 것: root 밖 연결 디스크(`outside-configuration-root`), 남은 VM이 참조하는 디스크(`referenced-by-remaining-vm`), root가 없거나 상대 경로(`no-configuration-root`), drive root나 그 바로 아래(`configuration-root-too-broad`), 참조 조회 실패(`reference-check-failed`, 이때는 아무것도 지우지 않음), 지우지 못한 파일(`delete-failed: …`), 비지 않은 root(`directory-not-empty`).
6. 정리 결과는 job 결과 `storage_cleanup`(`configuration_root`, `removed_files`, `removed_directories`, `retained`)에 남는다. 정리 실패는 delete를 실패로 만들지 않는다. VM은 이미 지워졌기 때문이다.

판정과 실행은 `DesktopNodeHyperVVmStorageCleanup`이 파일 시스템 interface로 한다. Required CI는 Hyper-V를 돌리지 않으므로 이 판정은 가짜 파일 시스템으로 시험한다.

## 3. 버린 선택지

| 안 | 버린 이유 |
| --- | --- |
| 연결 디스크만 지우기 | checkpoint가 있으면 연결 디스크가 `.avhdx`라 부모 `disk0.vhdx`가 남는다 |
| `--delete-disks` 같은 opt-in | managed VM의 저장소는 제품이 만든 것이고, 남기면 매번 손으로 지워야 한다. CLI/API/Web 계약도 넓어진다 |
| 디렉터리 통째 삭제 | 사용자가 둔 다른 파일까지 지운다 |

## 4. 비목표

- 실제 VM에서의 확인은 다음 package pair의 Lane 2가 맡는다.
- managed 표식이 없는 VM의 delete 거부는 그대로다.
