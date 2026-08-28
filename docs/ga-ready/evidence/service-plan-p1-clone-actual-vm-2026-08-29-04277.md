# SERVICE_PLAN P1 clone actual-VM 2026-08-29 `0.42.77`

evidence_id: `service-plan-p1-clone-actual-vm-2026-08-29-04277`
result: `FAIL`
evidence_scope: `installed-actual-vm-service-plan-p1-clone-candidate`
version: `0.42.77-admin-smoke`
source_commit: `51fbd88e5dbb05af28ceaff407c4662397f9b6ff`
artifact_root: `artifacts/service-plan-p1-clone-actual-vm-20260829-04277`
artifact_summary: `artifacts/service-plan-p1-clone-actual-vm-20260829-04277/summary.json`
summary_sha256: `6b32197ad0a6878d810b7c093a091cf2781b5b46d88100e3642f4a25e8f7056c`
runner_sha256: `2bbdc76b3aea317dfb31336372eafce85ff12363e95641cf31c80f0ca0981bf8`
installed_cli_sha256: `51e924c490b54a55195e9d675174dcfbcbcb3eccff758e596d6dfb2cb77f36f3`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.77-admin-smoke`에서 clone family 프로브를 새 artifact root로 실행했다.
create/preview는 PASS이고 clone job도 succeeded이며 대상 경로는 전용 VmRoot 아래다.
`clone_ok`가 `PCV_VM_NOT_FOUND`로 FAIL이다. cleanup은 PASS다. 이 summary는
`actual_vm_tested=pass` 입력이 될 수 없다.

| 항목 | 값 |
| --- | --- |
| create | `job-215baba6dcba4f21bd1076bd2ff51ac6` `succeeded` |
| clone | `job-66495cee2f76402d985ae875b1c76a4b` `succeeded` |
| source | `pcv-p1-clone-04277-5eed0b6b-src` / `373b6590-e09e-4671-9b81-0842253327c3` |
| target | `pcv-p1-clone-04277-5eed0b6b-dst` / `b83fb87a-e586-4faa-b1ed-58ec15dd0e2c` |
| target observed_path | `D:\data\pcv-p1-clone-04277\pcv-p1-clone-04277-5eed0b6b-dst` |
| Hyper-V/product after clone | `Off` / `stopped` |
| `error` | `PCV_VM_NOT_FOUND` |
| leftover `pcv-p1-clone-*` VM | `0` |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped` | `PASS` |
| `preview_mismatch` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED` | `PASS` |
| `preview_ok` | `planned_copy_bytes=4194304` | `PASS` |
| `clone_ok` | clone succeeded, 전용 VmRoot. 이후 `vm-get-target` exit 1 `PCV_VM_NOT_FOUND` | `FAIL` |
| `cleanup` | target 다음 source product delete, native fallback 없음 | `PASS` |

r3 FAIL는 대상이 `D:\PureCVisor\VMs`에 생긴 것이었다. 04277 `--vm-root`는 그 원인을 닫았다.

## 원인

runner `Invoke-CloneOkSlice`는 `Get-ProductVmState -OperatorId $TargetVm` 뒤에
`$targetVm = Get-PcvVmById`를 할당한다. PowerShell은 `$TargetVm`과 `$targetVm`을 같은
변수로 본다. 표시 이름 운영자 id가 Hyper-V VM 객체로 덮이고, 이어진
`pcvcli vm get $TargetVm`이 `PCV_VM_NOT_FOUND`를 낸다. 그 직전 `vm-get-state`는 표시
이름으로 성공했다.

다음 Lane 1은 clone_ok의 대상 get에 표시 이름 운영자 id만 쓰는 계약만이다. 같은
artifact root와 같은 VM 이름으로 D를 재실행하지 않는다.

## Nonclaims

- operational current는 `0.42.75-admin-smoke`로 유지한다.
- feature ledger를 pass로 바꾸지 않는다.
- r3 leftover `D:\PureCVisor\VMs\pcv-p1-clone-04276-34a8e66d-dst`는 이 프로브가 지우지 않았다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
