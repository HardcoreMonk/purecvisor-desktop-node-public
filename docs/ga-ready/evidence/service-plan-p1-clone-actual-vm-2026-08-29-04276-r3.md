# SERVICE_PLAN P1 clone actual-VM 2026-08-29 `0.42.76` r3

evidence_id: `service-plan-p1-clone-actual-vm-2026-08-29-04276-r3`
result: `FAIL`
evidence_scope: `installed-actual-vm-service-plan-p1-clone-candidate`
version: `0.42.76-admin-smoke`
source_commit: `48ed0b85caf3e94ef5bbcffca867cbb996707ce3`
working_tree_note: `uncommitted --disk-gb 8 and Test-PcvProductOff runner used; 20260829-04276 and r2 not reused`
artifact_root: `artifacts/service-plan-p1-clone-actual-vm-20260829-04276-r3`
artifact_summary: `artifacts/service-plan-p1-clone-actual-vm-20260829-04276-r3/summary.json`
summary_sha256: `d7a9059797995f51a1b917626bb36893c1d31852f95bb218e3e00816328a1e74`
runner_sha256: `ff3d9e5631abebcfe9024aac5fa16168b5c43d5e9dfef1096704836b982c664d`
installed_cli_sha256: `7fc2a92fcc3becceea90c0996afcbcdef863c6485542bd147760d738b3bbe77f`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.76-admin-smoke`에서 clone family 프로브를 새 artifact root로 실행했다.
`source_create`/`preview_mismatch`/`preview_ok`는 PASS다. clone job은 succeeded였지만
대상 경로가 전용 VmRoot 밖이라 `clone_ok`와 cleanup이 FAIL이다. 이 summary는
`actual_vm_tested=pass` 입력이 될 수 없다.

| 항목 | 값 |
| --- | --- |
| create | `job-095a4c7d8e644f2caa30f123642ea92d` `succeeded` |
| clone | `job-bdd0a90459a54fc5af4c043699308b1b` `succeeded` |
| product after create | `stopped` (Off 동치 PASS) |
| `preview_ok.planned_copy_bytes` | `4194304` |
| `error` | `PCV_P1_CLONE_CLEANUP_ROOT_INVALID; PCV_P1_CLONE_CLEANUP_FAILED` |
| leftover VM after runner | `pcv-p1-clone-04276-34a8e66d-dst` |
| leftover product delete | `job-ada1d7738de841eaa50816d7e713575d` `succeeded` |
| leftover Hyper-V `pcv-p1-clone-*` | `0` |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped` | `PASS` |
| `preview_mismatch` | exit `2`, `PCV_CLI_CONFIRMATION_REQUIRED`, 대상 없음 | `PASS` |
| `preview_ok` | dry-run, `planned_copy_bytes=4194304`, 파일 write 0 | `PASS` |
| `clone_ok` | clone succeeded. 대상 관측 경로 `D:\PureCVisor\VMs\pcv-p1-clone-04276-34a8e66d-dst`가 예약 root `D:\data\pcv-p1-clone-04276\...` 밖 | `FAIL` |
| `cleanup` | source product delete PASS. 대상은 id 미기록 orphan-blocker, `PCV_P1_CLONE_CLEANUP_ID_MISMATCH` | `FAIL` |

## 원인

설치본 `pcvcli vm clone`은 `--vm-root`를 보내지 않는다. 대상이 기본
`D:\PureCVisor\VMs\<name>`에 만들어졌다. runner는 전용 VmRoot 자식만 권위 경로로
인정한다. `Set-VmAuthoritativeIdentity`가 `PCV_P1_CLONE_CLEANUP_ROOT_INVALID`로
막혔고 대상 id를 기록하지 않아 cleanup이 fail-closed로 삭제를 거부했다.

선행 r2는 제품 `stopped` vs `off`였다. r3는 그 원인을 닫았고 새 원인이다.
같은 artifact root와 같은 VM 이름으로 D를 재실행하지 않는다.

프로브 종료 후 표시 이름으로 제품 delete를 한 번 수행해 Hyper-V leftover는 0이다.
기본 경로에 `disk0.vhdx` `4194304` bytes가 남아 있다. 이 checkpoint는 그 디렉터리를
`Remove-VM`/`rm`으로 지우지 않는다.

다음 Lane 1은 clone 대상을 소스 VmRoot 아래로 보내는 계약만이다. clone family를
다시 열지 않는다.

## Nonclaims

- operational current는 `0.42.75-admin-smoke`로 유지한다.
- feature ledger를 pass로 바꾸지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
