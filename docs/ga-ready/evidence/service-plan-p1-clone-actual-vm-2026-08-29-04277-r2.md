# SERVICE_PLAN P1 clone actual-VM 2026-08-29 `0.42.77` r2

evidence_id: `service-plan-p1-clone-actual-vm-2026-08-29-04277-r2`
result: `PASS`
evidence_scope: `installed-actual-vm-service-plan-p1-clone-candidate`
version: `0.42.77-admin-smoke`
source_commit: `e48709a0658e96f291a2111eb1796d7db3a34272`
artifact_root: `artifacts/service-plan-p1-clone-actual-vm-20260829-04277-r2`
artifact_summary: `artifacts/service-plan-p1-clone-actual-vm-20260829-04277-r2/summary.json`
summary_sha256: `69af73fb52b51a74b5be483196e1d6e777cb552efa405e18f638ebf60d3ed17f`
runner_sha256: `53b82fe9f42bd63249be5fd37c40d6ac1039387be9c8a4a65662105375ab9f8a`
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

설치본 `0.42.77-admin-smoke`에서 clone family 한 프로브를 새 artifact root로 실행했다.
`overall_verdict=PASS`, `cleanup.verdict=PASS`, `secret_observed=false`다.
`docs/ga-ready/current-evidence.json`은 `0.42.75-admin-smoke`로 유지한다.

| 항목 | 값 |
| --- | --- |
| create | `job-cf8478aba87440b183003b3c4feda4ab` `succeeded` |
| clone | `job-a2db869c20a5436187cfeba16f8772d0` `succeeded` |
| source | `pcv-p1-clone-04277-5234b19e-src` / `a1208e1f-d602-4fd8-b744-4ef0f9400c8f` |
| target | `pcv-p1-clone-04277-5234b19e-dst` / `526dd399-feda-4d12-9e7f-f57990837624` |
| target disk | `D:\data\pcv-p1-clone-04277\pcv-p1-clone-04277-5234b19e-dst\disk0.vhdx` |
| leftover `pcv-p1-clone-*` VM | `0` |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, 시작하지 않음 | `PASS` |
| `preview_mismatch` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, 대상 없음 | `PASS` |
| `preview_ok` | `planned_copy_bytes=4194304`, 파일 write 0 | `PASS` |
| `clone_ok` | job succeeded, managed true, 전용 VmRoot `disk0.vhdx`, 소스 Off | `PASS` |
| `cleanup` | 대상 다음 소스 product delete, native fallback 없음 | `PASS` |

선행 `20260829-04277` FAIL는 `$TargetVm`을 Hyper-V 객체로 덮어 `vm-get-target`이
`PCV_VM_NOT_FOUND`를 낸 것이다. r2 runner는 표시 이름 운영자 id만 get에 쓴다.

## Nonclaims

- operational current는 `0.42.75-admin-smoke`다. Lane 3 pair/fullgate/current 승격을 하지 않았다.
- feature ledger를 pass로 바꾸지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
