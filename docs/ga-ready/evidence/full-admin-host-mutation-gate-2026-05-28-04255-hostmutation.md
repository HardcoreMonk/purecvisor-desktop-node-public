# Full admin host mutation gate 2026-05-28 0.42.55

evidence_id: `full-admin-host-mutation-gate-2026-05-28-04255-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate-web-tui-running-cancel-affordance-rollforward`
version: `0.42.55-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260528-04255`
batch_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260528-04255`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260528-04255`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260528-04255`
full_gate_msi_sha256: `cfd4d3c1cc22fff41f5c9b0f79f2a40df17b4ae91b3f4e0e24f43e4d096230eb`
full_gate_payload_aggregate_sha256: `69019129347920bba88c269a4828dae5b214eace8a6d31bd60bc7fa7f1b81934`
clean_package_msi_sha256: `530d5605a99ff607a8030192a23fd4ba8bdb703793290b3e09e446dc61121627`
provenance_commit: `958052181012f7d1be6ccff535316bfaeeef07df`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## Batch 결과

| step | 결과 | duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `PASS` | `217044 ms` |
| `os-mutation-gate` | `PASS` | `11070 ms` |

Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 통과했다. 최종 service state는
`Running`, boot time은 `2026-05-27T16:47:53.5+09:00`로 unchanged, VM smoke cleanup 후
`remaining_pcv_vms=[]`로 닫혔다. OS mutation gate는 final firewall rule count `0`,
Event Log source absent, trust store root/publisher restored 상태를 확인했다.

## 산출물 해시

| 산출물 | SHA-256 |
| --- | --- |
| MSI | `cfd4d3c1cc22fff41f5c9b0f79f2a40df17b4ae91b3f4e0e24f43e4d096230eb` |
| Payload aggregate | `69019129347920bba88c269a4828dae5b214eace8a6d31bd60bc7fa7f1b81934` |
| Host EXE | `058ea3fc138b2d3d9fccbef17d40703461215f7e154e3e8e0a3ead665db5bf1b` |
| PCVCLI EXE | `d2355a4222bc7aa909907369d1b3b26c0027249c45a097049d16b2f3a5b65c91` |
| PCVTUI EXE | `dbbcd57b4ad40311d3967e745a9595108d11c00e8f73e084a68c0ab05046885f` |

## 경계

이 fullgate는 `0.42.55-admin-smoke` operational anchor를 0.42.54에서 0.42.55로 승격한다.
Web/TUI running guest execution cancel affordance는 이 package/current-card에서 설치본으로
확인한다. Public trusted signing, external stable publication, winget public submission claim은
계속 범위 밖이다.
