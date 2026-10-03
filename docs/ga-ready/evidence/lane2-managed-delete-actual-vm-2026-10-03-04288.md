# Lane 2 managed delete 디스크 정리 actual-VM 확인 `0.42.88-admin-smoke` (2026-10-03)

evidence_id: `lane2-managed-delete-actual-vm-2026-10-03-04288`
result: `PASS`
status: `installed_non_promoted_candidate`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.88-admin-smoke`
installed_product_version: `0.42.88-admin-smoke+47ff198de86d77d25aa90fa095a294254c7ffef6`
source_fix: `96e8570` (PR #30 merge `ed4a4fa`)
artifact_root: `artifacts/lane2-managed-delete-actual-vm-20261003-04288`
probe_script_sha256: `42cb6422b52119084f45723fece1496129e09d6cbe8c991e0ef4e4c09e0d5c47`
probe_summary_sha256: `553b8ed3bcc48d8688a07c1f612e62fa49fc923af070d41713dc4641ebbfda3c`
judgment_sha256: `5db8e115e331933442dd662beee3f50703ba4746c943565bf13160b8c3d45e2c`
delete_job_sha256: `5e291d1550780f1ace9ae3f93c76c09ac1835aad691791f091fd70c562c1bfac`
probe_vm: `pcv-lane2-delete-1003`
host_mutation_performed: `true`
manual_cleanup_performed: `false`
secret_observed: `false`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

fullgate 설치본 `0.42.88`에서 기본 VM root `D:\PureCVisor\VMs`에 probe VM을 만들고, checkpoint 하나를 남긴 채 `pcvcli vm delete --yes`를 실행했다. 실행 스크립트는 artifact root의 `probe.ps1`이고 2026-10-03 `12:03:04Z`부터 `12:03:16Z`까지 돌았다. ISO는 fullgate와 같은 최소 ISO 9660이다.

## 결과

| 단계 | 관측 |
| --- | --- |
| `vm create` (Gen2, 1 vCPU, 1024 MB, 8 GB) | job `succeeded`. `pcv-lane2-delete-1003\disk0.vhdx`와 `Virtual Machines\` 생성 |
| `vm checkpoint create --name pcv-lane2-cp1` | job `succeeded`. `disk0_E90B0A8E-….avhdx`와 `Snapshots\` 생성 |
| `vm delete --yes` | job `succeeded` |
| `storage_cleanup.configuration_root` | `D:\PureCVisor\VMs\pcv-lane2-delete-1003` |
| `storage_cleanup.removed_files` | `disk0.vhdx` |
| `storage_cleanup.removed_directories` | `Snapshots`, `Virtual Machines`, VM root |
| `storage_cleanup.retained` | 없음 |
| delete 뒤 VM 디렉터리 | 없음 |
| `D:\PureCVisor\VMs` 하위 디렉터리 | 사전과 같음(이전 run 잔여 `pcv-p1-clone-04276-34a8e66d-dst` 하나) |
| VM 수 | 사전과 같음(보존 VM 하나) |
| 손 정리 | 하지 않음 |

checkpoint 차등 디스크 `.avhdx`는 delete 전에는 있었지만 `removed_files`에 없다. Hyper-V가 `DestroySystem` 중 checkpoint를 정리하면서 먼저 없앴고, 정리 단계가 디렉터리를 열거할 때는 `disk0.vhdx`만 남아 있었다. 정리 결과로 디렉터리 전체가 없어졌다.

`probe.ps1`이 자동으로 쓴 `summary.json`은 `result=FAIL`이다. 판정 조건에 "`removed_files`에 `.avhdx`가 있다"를 넣었는데, 위 이유로 그 조건이 사실과 맞지 않았기 때문이다. 같은 관측으로 목표 기준(delete 성공, `disk0.vhdx`와 root 제거, retained 없음, 디렉터리 없음, VM root 하위 디렉터리와 VM 수 불변, 손 정리 없음, secret 없음)을 다시 판정한 결과가 `judgment.json`(`result=PASS`)이다.

같은 날 fullgate route smoke도 설치본 `0.42.88`로 checkpoint create/restore/delete 뒤 VM을 지웠고, `storage_cleanup`이 `disk0.vhdx`와 디렉터리를 지웠다(`full-admin-host-mutation-gate-2026-10-03-04288-hostmutation`).

## report-only 발견

- `D:\PureCVisor\VMs\pcv-p1-clone-04276-34a8e66d-dst`는 이 수정 전(0.42.76 clone probe)에 남은 디렉터리다. 이번에는 건드리지 않았다.

## Nonclaims

- 상태는 `installed_non_promoted_candidate`다. operational current는 `0.42.87-admin-smoke`다.
- 다른 VM이 같은 디스크를 참조하는 경우와 root 밖 디스크는 단위 테스트로만 확인했다.
- public trusted signing과 external stable publication을 주장하지 않는다.
