# Functional correctness actual-host validation 2026-08-14 `0.42.73`

evidence_id: `functional-correctness-actual-host-validation-2026-08-14-04273`
result: `PASS`
evidence_scope: `installed-actual-vm-functional-correctness`
version: `0.42.73-admin-smoke`
source_commit: `b84441f0750a9f77fd0588a86912dbdb68b94f0c`
artifact_root: `artifacts/functional-correctness-carryforward-20260814-04273`
artifact_summary: `artifacts/functional-correctness-carryforward-20260814-04273/summary.json`
summary_sha256: `09a571235524b1a32c6066b7ef8c3c4ab4a425a7016ef4ccd1d284f75f9e6fac`
runner_sha256: `b0ac6cf563df637a9df42dfd8ab7f575bd7d8abc07329edcdcf3f84e90cf06ae`
installed_cli_sha256: `b8a7374e843999d2979ba5181d18fb91909a375ef0482b840cb942c253b40bc2`
operational_msi_sha256: `3151807589504f1ede79592cf0bb077a9cb6da3b54206f89002df5d63b30dac1`
operational_payload_sha256: `a5d74ed394c4fc3d230457fb24059aab658fa621abbba630ce1d113a21a75d85`
vm_name: `pcv-fc-cf-04273`
host_mutation_performed: `true`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.73-admin-smoke` 설치본 PCVCLI로 실제 Hyper-V VM을 생성해 QoS와 disk resize
경계를 재실행했다. 이 문서는 `0.42.72` carry-forward가 아니라 새 actual-VM 실행 evidence다.

| 계약 | 관측 | verdict |
| --- | --- | --- |
| command graph | `10` steps: primary `5` + job readback `5` | `PASS` |
| exit/readback | `10/10` exit `0`, `10/10` ok | `PASS` |
| QoS maximum | `2048 Kbps -> 2,048,000 bps` WMI readback | `PASS` |
| disk shrink | `10 GiB -> 9 GiB` 요청, `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` | `PASS` |
| shrink size guard | `10,737,418,240` bytes 유지 | `PASS` |
| disk expansion | `10 GiB -> 11 GiB`, `11,811,160,064` bytes | `PASS` |
| cleanup | VM 제거, VM folder/root 제거 | `PASS` |

## 실행

- runner: `packaging/windows-desktop-node/tools/Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`
- create job: `job-32f287116a90453f8e42201de650bc4f`
- QoS job: `job-6029db92c4fd454b98414d368532e8b1`
- shrink job: `job-471da015c43a46eda6a2b20ee2afd564`
- expand job: `job-80c5f168e52c477286c7ff02d867e632`
- 실행 후 `pcv-fc-cf-*` / `pcv-spike-*` 잔여 VM `0`
- raw secret은 artifact에 persisted되지 않았다.

## Nonclaims

- 이 검증은 actual VM을 만들고 삭제했으므로 host mutation을 수행했다.
- guest OS credentialed `guest-exec` smoke나 token rotation을 이 evidence가 주장하지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
- canonical `current-evidence.json`은 이 functional만으로 승격하지 않는다.
