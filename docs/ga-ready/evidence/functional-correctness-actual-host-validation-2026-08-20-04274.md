# Functional correctness actual-host validation 2026-08-20 `0.42.74`

evidence_id: `functional-correctness-actual-host-validation-2026-08-20-04274`
result: `PASS`
evidence_scope: `installed-actual-vm-functional-correctness`
version: `0.42.74-admin-smoke`
source_commit: `adc04673b569ef9b587371fdb23bc11ceb14e2e2`
artifact_root: `artifacts/functional-correctness-carryforward-20260820-04274`
artifact_summary: `artifacts/functional-correctness-carryforward-20260820-04274/summary.json`
summary_sha256: `5395286b74ca7dabd3edccbb63c0b006c32999a4c350559e8b90ddb1ea1fb4b8`
runner_sha256: `b0ac6cf563df637a9df42dfd8ab7f575bd7d8abc07329edcdcf3f84e90cf06ae`
installed_cli_sha256: `21b22cdaa9640ea8b63a031e4a815da1f583a60ca3c6e8486595bdc4a5eb07b0`
operational_msi_sha256: `2bc46c986a629695462f6b424bb3ca963162fd59fbf6359fbcb73b38ea09b787`
operational_payload_sha256: `c7984216f1625f2570e2da8cc0428f1a9a4ef9ecf8fe049d8ccfa6d3100df71d`
vm_name: `pcv-fc-cf-04274`
host_mutation_performed: `true`
secret_observed: `false`
canonical_current_evidence: `0.42.74-admin-smoke`
canonical_current_changed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.74-admin-smoke` 설치본 PCVCLI로 실제 Hyper-V VM을 생성해 QoS와 disk resize
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
- create job: `job-c56af0bddf0749b59beade1de4fda343`
- QoS job: `job-18a30de940224d369a8ba6d5cd6a2dbc`
- shrink job: `job-8b3c5b7aa9d54fa8a4e8af85890fc3a4`
- expand job: `job-795c1eb17a584388b26ea3881dd1612d`
- 실행 후 `pcv-fc-cf-*` / `pcv-spike-*` 잔여 VM `0`
- raw secret은 artifact에 persisted되지 않았다.

## Nonclaims

- 이 검증은 actual VM을 만들고 삭제했으므로 host mutation을 수행했다.
- SERVICE_PLAN P0 attach/restore/save/manage는 이 문서가 소유하지 않는다.
- guest OS credentialed `guest-exec` smoke나 token rotation을 이 evidence가 주장하지 않는다.
- public trusted signing 또는 external stable publication을 주장하지 않는다.
- canonical `current-evidence.json` 승격은 같은 날 ledger update가 소유한다.
