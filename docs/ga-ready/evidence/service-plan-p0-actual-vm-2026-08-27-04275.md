# SERVICE_PLAN P0 actual-VM 2026-08-27 `0.42.75`

evidence_id: `service-plan-p0-actual-vm-2026-08-27-04275`
result: `PASS`
evidence_scope: `installed-actual-vm-service-plan-p0-candidate`
version: `0.42.75-admin-smoke`
source_commit: `f30c94683b41a1d46ba6ab3a8fd6c735e76996a1`
installed_cli_sha256: `7e2b99bc0eda1fb11dcaac40b24b829581de7167d79552e0c48c40decdf1211d`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.74-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.75-admin-smoke`에서 Lane 2 SavedOnly와 Full P0를 실제 Hyper-V VM으로
실행했다. Saved 왕복, DVD attach, restore, managed import, cleanup이 PASS다. 이 증거는
04275 candidate actual-VM이지 operational current 전환이 아니다. `0.42.74-admin-smoke`
ledger와 `pcv.vm.saved-lifecycle/actual_vm_tested/fail` blocker는 유지한다.

| 실행 | artifact | summary SHA-256 | overall_verdict |
| --- | --- | --- | --- |
| SavedOnly r2 | `artifacts/service-plan-p0-actual-vm-20260827-04275-savedonly-r2/summary.json` | `3c614d183e9fb37377895a677e9dffc0cc683df613c1fd1c5fac659d411cde60` | `PASS` |
| Full r4 | `artifacts/service-plan-p0-actual-vm-20260827-04275-full-r4/summary.json` | `ab6b8e44042f6c89735d0dddf6a3c56f8ff490329f6e1df253e6f33bbb22aff7` | `PASS` |

## Full r4 slice

| slice | 관측 | verdict |
| --- | --- | --- |
| saved_lifecycle | Hyper-V `Saved` / 제품 `saved` 후 `Running` / `running` | `PASS` |
| media_attach | `vm.attach` succeeded, DVD `HostResource` = ISO | `PASS` |
| checkpoint_restore | restore 전 `vm.poweroff` → Hyper-V `Off`, restore succeeded, `p0-restore` `is_current=true` count `1` | `PASS` |
| managed_import | unmanaged delete `PCV_VM_NOT_MANAGED_BY_PURECVISOR`, manage marker, managed delete | `PASS` |
| cleanup | product delete 성공, native fallback 없음, `pcv-p0-*` 잔여 `0` | `PASS` |

Full r4 managed VM은 `pcv-p0-04275-e57c1d2c-managed` /
`855b1964-e096-44f0-ba3d-ea103325c5ff`다. SavedOnly r2 managed VM은
`pcv-p0-04275-39c57e15-managed` / `101e3a31-9227-41e1-b2e1-9bbde3842c61`다.

## 선행 FAIL와 runner 계약

같은 호스트 Full r2는 Running VM에서 `ApplySnapshot`이
`PCV_HYPERV_WMI_METHOD_FAILED`로 실패했다. Full r3는 restore job은 성공했지만
`vm checkpoint list`를 Hyper-V GUID로 호출해 `PCV_VM_NOT_FOUND`가 났다. r4 runner는
restore 전 `vm poweroff`와 Off 대기, list는 표시 이름 운영자 id를 쓴다.

04274 P0 `vm.save` WMI `32775` FAIL는
`docs/ga-ready/evidence/service-plan-p0-actual-vm-2026-08-20-04274.md`가 계속 소유한다.
이 문서는 그 FAIL를 pass로 재해석하지 않는다.

## 아직 열리지 않은 04275 승격 입력

- functional carry-forward actual-VM
- `0.42.74-admin-smoke -> 0.42.75-admin-smoke` manual-admin package-pair
- clean-target SavedOnly
- final installed current-card

위가 모두 PASS하기 전에는 `docs/ga-ready/current-evidence.json`의 `current.version`과
`feature_qualification.promotion_eligible`을 바꾸지 않는다.

## Nonclaims

- host mutation을 수행했다. leftover `pcv-p0-*` VM은 없다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
- 04274 operational current를 소급 삭제하거나 04275로 바꾸지 않는다.
