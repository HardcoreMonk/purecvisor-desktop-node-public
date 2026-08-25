# Full admin host mutation gate 2026-05-28 0.42.54

evidence_id: `full-admin-host-mutation-gate-2026-05-28-04254-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate-running-guest-cancel-rollforward`
version: `0.42.54-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260528-04254`
batch_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260528-04254`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260528-04254`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260528-04254`
full_gate_msi_sha256: `937ac686aa782a69dc41d06d8694a020cf4a78b45cf7a6674e85593cce3c4cb1`
full_gate_payload_aggregate_sha256: `bdcb61002f5e3e739ca3db5cb0a189548b9c9b25ef5747c437c7b23d615fef84`
provenance_commit: `2c11e359709c775be7a57ea9624716720c5b62d6`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## Batch 결과

| step | 결과 | duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `PASS` | `114497 ms` |
| `os-mutation-gate` | `PASS` | `11109 ms` |

Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 통과했다. 최종 service state는
`Running`, boot time은 `2026-05-27T16:47:53.5+09:00`로 unchanged, VM smoke cleanup 후
`remaining_pcv_vms=[]`로 닫혔다. OS mutation gate는 final firewall rule count `0`,
Event Log source absent, trust store root/publisher restored 상태를 확인했다.

## 산출물 해시

| 산출물 | SHA-256 |
| --- | --- |
| MSI | `937ac686aa782a69dc41d06d8694a020cf4a78b45cf7a6674e85593cce3c4cb1` |
| Payload aggregate | `bdcb61002f5e3e739ca3db5cb0a189548b9c9b25ef5747c437c7b23d615fef84` |
| Host EXE | `72dea6ec683066754c6c17f6c80546b954abcfccc890dbe9482ecddcc69281bc` |
| PCVCLI EXE | `cde40a7a2e8b146697c5a3590266fbfe2764828dde1b0262597ce6392078f86f` |
| PCVTUI EXE | `ab0146a4882f2b796660c780e2f3287c9638e1a3d8dfa5bbe1f460c94a2eefef` |

## 경계

이 fullgate는 `0.42.54-admin-smoke` operational anchor를 0.42.53에서 0.42.54로 승격한다.
이후 들어간 Web/TUI running job cancel affordance 변경은
`docs/ga-ready/evidence/web-tui-running-job-cancel-affordance-code-level-2026-05-28.md`의
code-level evidence이며, 다음 package/current-card에서 설치본으로 승격해야 한다.
Public trusted signing, external stable publication, winget public submission claim은 계속 범위 밖이다.
