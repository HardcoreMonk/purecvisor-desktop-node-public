# SERVICE_PLAN P2 Off VM checkpoint-schedule actual-VM 2026-09-27 `0.42.78`

evidence_id: `service-plan-p2-offvm-checkpoint-schedule-actual-vm-2026-09-27-04278`
result: `PASS`
evidence_scope: `installed-actual-vm-service-plan-p2-offvm-checkpoint-schedule-candidate`
version: `0.42.78-admin-smoke`
runner: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP2OffVmActualVmSmoke.ps1 -Family checkpoint-schedule`
runner_commit: `f14978b`
artifact_root: `artifacts/service-plan-p2-offvm-checkpoint-schedule-actual-vm-20260927-04278`
artifact_summary: `artifacts/service-plan-p2-offvm-checkpoint-schedule-actual-vm-20260927-04278/summary.json`
summary_sha256: `937b6183d73bf6a8d6a44698a611677d8ffac106df6e274e76588608e113d949`
runner_sha256: `14401aa74eca9fce11a8a89cd0ca91a70ab32dc93b6f371c3549bb10b67b0b8f` (실행 시 working tree)
installed_cli_sha256: `cdf68bfa72f5d56b21fdddec6af1cac6ef674fecfcc27982bcecd0eebf1eaba1`
iso_path: `D:\Downloads\ubuntu-26.04-live-server-amd64.iso`
vm_root: `D:\data\pcv-p2-offvm-04278-sched`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
promotion_eligible_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

설치본 `0.42.78-admin-smoke`에서 P2-12 checkpoint schedule 기능군 한 프로브를 새 artifact root와 새 VM root로
실행했다. `overall_verdict=PASS`, `cleanup.verdict=PASS`, `secret_observed=false`다. current-evidence와 feature
ledger는 바꾸지 않았다. 승인: `User-Approval: 2026-09-27 host mutation ok, Off VM 기능군부터`.

| 항목 | 값 |
| --- | --- |
| VM | `pcv-p2-offvm-04278-67d60f26-sched` / `4fd59957-6dc7-4262-9a4b-a73609f6584c` |
| create | `job-cd27d10f329040fa98813f9390cc91d5` `succeeded` |
| schedule set | `job-024ba57c8647437a9963e3f63e89ad0b` `succeeded` |
| schedule clear | `job-553284bbb9164b609ec3b0539ec6401c` `succeeded` |
| delete | `job-e3412e7656114cdf86c676c32ce77411` `succeeded` |
| 잔여 `pcv-p2-*` VM | `0` |
| VM 디렉터리 | 삭제됨 |

## slice

| slice | 관측 | verdict |
| --- | --- | --- |
| `source_create` | Hyper-V `Off`, 제품 `stopped`, managed, Gen2, checkpoint `0` | `PASS` |
| `schedule_preview` | `dry_run=true`, `60`분, retention `2`, job 없음, `vm get` `checkpoint_schedule` `disabled` | `PASS` |
| `schedule_interval_invalid` | `30`분 set → exit `1`, `PCV_CHECKPOINT_SCHEDULE_INTERVAL_INVALID`, job 없음, `disabled` | `PASS` |
| `schedule_set` | job succeeded, readback `enabled=true`, `status=waiting`, `60`분, retention `2`, checkpoint `0` | `PASS` |
| `schedule_clear` | job succeeded, readback `enabled=false`, `status=disabled`, checkpoint `0` | `PASS` |
| `cleanup` | 스케줄은 clear slice에서 이미 해제, 제품 delete, native fallback 없음 | `PASS` |

Hyper-V checkpoint 수는 `root\virtualization\v2` `Msvm_SnapshotOfVirtualSystem`에서 읽었다.

## Nonclaims

- due worker tick과 scheduled `checkpoint.create`: 최소 주기 `60`분이라 run 안에서 관측하지 않았다.
- retention에 따른 `pcv-schedule-*` 삭제: 관측하지 않았다.
- operational current는 `0.42.78-admin-smoke` 그대로다. Lane 3, feature ledger pass를 하지 않았다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
