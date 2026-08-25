# Full admin host mutation gate 2026-05-28 0.42.56

evidence_id: `full-admin-host-mutation-gate-2026-05-28-04256-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate-manual-admin-next-package-pair-operator-surface-rollforward`
version: `0.42.56-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260528-04256`
batch_artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260528-04256`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260528-04256`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260528-04256`
full_gate_msi_sha256: `085792312b3bba3ba241882156212b40f936748b08a0ad56ae4a877b24759dec`
full_gate_payload_aggregate_sha256: `98057c20aacd109d451a4b18b5ecb16b012d46bc85443562d3be149be0a0a7f2`
clean_package_msi_sha256: `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`
provenance_commit: `5594adc55b013a2bf3ade9c6ae7171ca37bdbeb0`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

## Batch 결과

| step | 결과 | duration |
| --- | --- | --- |
| `service-msi-hyperv-admin-smoke` | `PASS` | `236397 ms` |
| `os-mutation-gate` | `PASS` | `11057 ms` |

Service/MSI/Hyper-V route smoke와 OS mutation gate가 모두 통과했다. 최종 service state는
`Running`, boot time은 `2026-05-27T16:47:53.5+09:00`로 unchanged, VM smoke cleanup 후
`remaining_pcv_vms=[]`로 닫혔다. OS mutation gate는 final firewall rule count `0`,
Event Log source absent, trust store root/publisher restored 상태를 확인했다.

## 산출물 해시

| 산출물 | SHA-256 |
| --- | --- |
| MSI | `085792312b3bba3ba241882156212b40f936748b08a0ad56ae4a877b24759dec` |
| Payload aggregate | `98057c20aacd109d451a4b18b5ecb16b012d46bc85443562d3be149be0a0a7f2` |
| Host EXE | `09bc89f0f3660dc12845629013c7fa2f3a4cd9b1ef3437e1073fac3e3011736d` |
| PCVCLI EXE | `91d54c317ac726db36a49170f22474c7182132e4a3379fb52ce436e4640d5958` |
| PCVTUI EXE | `77c25f3d306e851fdb226b5b0e77b24721e684dd2d6632385ce5cb375f20eb4d` |

## 경계

이 fullgate는 `0.42.56-admin-smoke` operational anchor를 0.42.55에서 0.42.56으로 승격한다.
Public trusted signing, external stable publication, winget public submission claim은 계속 범위 밖이다.
