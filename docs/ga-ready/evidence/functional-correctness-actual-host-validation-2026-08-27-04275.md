# Functional correctness actual-host validation 2026-08-27 `0.42.75`

evidence_id: `functional-correctness-actual-host-validation-2026-08-27-04275`
result: `PASS`
evidence_scope: `installed-actual-vm-functional-correctness`
version: `0.42.75-admin-smoke`
source_commit: `dbe1b48cf8bfc45fe7c431fac30ff498dfc9bbe4`
artifact_root: `artifacts/functional-correctness-carryforward-20260827-04275`
artifact_summary: `artifacts/functional-correctness-carryforward-20260827-04275/summary.json`
summary_sha256: `a907535a5868d0e9a16095f2cf933dc2a8348a947d09af7537e038af4cf16ed5`
runner_sha256: `b0ac6cf563df637a9df42dfd8ab7f575bd7d8abc07329edcdcf3f84e90cf06ae`
installed_cli_sha256: `7e2b99bc0eda1fb11dcaac40b24b829581de7167d79552e0c48c40decdf1211d`
operational_msi_sha256: `d5afd8774ca5c33b84b10faa771703dcdba37c96d816be4dbb8f9a886f7c967b`
operational_payload_sha256: `b6882c9ab40dffc2a9a15785841a097140c23fef6eba26dc76bc892107c2c9b7`
vm_name: `pcv-fc-cf-04275`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.75-admin-smoke` 설치본 PCVCLI로 실제 Hyper-V VM을 생성해 QoS와 disk resize
경계를 재실행했다.

| 계약 | 관측 | verdict |
| --- | --- | --- |
| command graph | create / QoS / shrink / expand / delete + job readback | `PASS` |
| QoS maximum | `2048 Kbps -> 2,048,000 bps` WMI readback | `PASS` |
| disk shrink | `10 GiB -> 9 GiB` 요청, `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` | `PASS` |
| shrink size guard | `10,737,418,240` bytes 유지 | `PASS` |
| disk expansion | `10 GiB -> 11 GiB`, `11,811,160,064` bytes | `PASS` |
| cleanup | VM 제거, VM folder/root 제거 | `PASS` |

## 실행

- runner: `packaging/windows-desktop-node/tools/Invoke-PcvFunctionalCorrectnessCarryForwardSmoke.ps1`
- create job: `job-92866a12787c4daf9e96578b30d93bd6`
- QoS job: `job-eaa8d985cbbb48089462d680d104ec4f`
- shrink job: `job-7ce11054e73846cfb6f465ea19b76572`
- expand job: `job-891e86b0cfea40f5bd4132538ea07f75`
- 실행 후 `pcv-fc-cf-*` / `pcv-spike-*` 잔여 VM `0`
- raw secret은 artifact에 persisted되지 않았다.

## Nonclaims

- 이 검증은 actual VM을 만들고 삭제했으므로 host mutation을 수행했다.
- SERVICE_PLAN P0 attach/restore/save/manage는 이 문서가 소유하지 않는다.
- guest OS credentialed `guest-exec` smoke나 token rotation을 이 evidence가 주장하지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
- canonical `current-evidence.json` 승격은 같은 Lane 3 ledger update가 소유한다.
