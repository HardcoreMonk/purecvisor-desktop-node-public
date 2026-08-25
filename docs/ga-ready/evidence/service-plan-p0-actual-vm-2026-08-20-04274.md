# SERVICE_PLAN P0 actual-VM 2026-08-20 `0.42.74`

evidence_id: `service-plan-p0-actual-vm-2026-08-20-04274`
result: `FAIL`
evidence_scope: `installed-actual-vm-service-plan-p0`
version: `0.42.74-admin-smoke`
source_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
artifact_root: `artifacts/service-plan-p0-actual-vm-20260820-04274`
artifact_summary: `artifacts/service-plan-p0-actual-vm-20260820-04274/summary.json`
summary_sha256: `11d8d1b34d6e6ff49e2ebb81bc234d20b7eab9f1299baa36ce8daac9c9b14e5d`
r1_summary: `artifacts/service-plan-p0-actual-vm-20260820-04274/summary-r1-eject-save-failed.json`
installed_cli_sha256: `21b22cdaa9640ea8b63a031e4a815da1f583a60ca3c6e8486595bdc4a5eb07b0`
vm_name: `pcv-p0-04274`
foreign_vm: `pcv-p0-foreign-04274`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.74-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 PCVCLI로 SERVICE_PLAN P0 네 조각을 실제 Hyper-V VM에서 실행했다. attach
overwrite, checkpoint restore `is_current`, managed import는 PASS다. Hyper-V Saved
`vm.save`는 이 호스트에서 FAIL이다. 전체 smoke는 FAIL이다.

| slice | 관측 | verdict |
| --- | --- | --- |
| P0-1 attach | `vm.attach` succeeded, DVD `HostResource` = ISO | `PASS` |
| P0-1 eject prelude | `vm.eject` `PCV_HYPERV_WMI_JOB_FAILED` (resource modify) | noted, attach overwrite 계약은 유지 |
| P0-2 restore | `checkpoint.restore` succeeded, `is_current=true` count `1` | `PASS` |
| P0-3 save | `vm.save` `PCV_HYPERV_WMI_METHOD_FAILED`, WMI `32775` | `FAIL` |
| P0-3 resume-saved | save 실패로 skipped; 선행 r1은 `PCV_VM_NOT_SAVED` | `FAIL` |
| P0-4 manage | unmanaged delete `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, manage Notes marker, 이후 delete `action` succeeded | `PASS` |
| cleanup | `pcv-p0-*` 잔여 `0` | `PASS` |

## Saved RCA

제품 `vm.save`는 `Msvm_ComputerSystem.RequestStateChange` RequestedState `32769`
(`SavedState`)를 보낸다. 이 호스트(Windows 10 25H2 / Hyper-V)에서 같은 호출은
ReturnValue `32775`다. VM `Get-VM` state는 `Running`, WMI `EnabledState`는 `2`였다.

같은 호스트에서:

- `Stop-VM -Save`는 Saved로 성공한다.
- CIM `RequestStateChange` RequestedState `6`은 ReturnValue `4096`(job started)이고
  `Get-VM` state는 `Saved`다.
- RequestedState `32769`는 반복해도 `32775`다.

`MapEnabledState`는 `32769 => saved`만 있고 `6`은 `unknown`이다. 따라서 이 호스트에서
save를 `6`으로 바꿔도 `vm.resume-saved`의 `RequireSaved`는 현재 mapping으로는
통과하지 않는다. 이 문서는 04274 설치본을 고치지 않는다. 후속 product payload
수정과 별도 package가 필요하다.

## Nonclaims

- 이 검증은 actual VM을 만들고 삭제했으므로 host mutation을 수행했다.
- functional QoS/disk carry-forward는 별도 evidence가 소유한다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
- canonical `current-evidence.json`은 `0.42.74-admin-smoke`로 승격됐지만 이 FAIL는 열린 결함이다.
