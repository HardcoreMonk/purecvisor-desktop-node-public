# Full admin host mutation gate 2026-05-20 0.42.35

evidence_id: `full-admin-host-mutation-gate-2026-05-20-04235-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.35-admin-smoke`
manual_admin_package_pair: `0.42.34-admin-smoke -> 0.42.35-admin-smoke`
manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04234-04235-closed`
batch_id: `full-admin-host-mutation-gate-20260520-04235`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260520-04235`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04235`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260520-04235`
host_mutation_performed: `true`
full_gate_msi_sha256: `12d05f2d783dfdb1db3f1596cd266af17578e33fca3f4fec272aac7df5e22697`
payload_aggregate_sha256: `ba966f3c41d81579dc6f065988c5fc015d47a9b0c8c77b4f4c3bf5962c1806a1`
provenance_commit: `51a21d7c8612f598b85eeb58818ad3d61136c320`
build_utc: `2026-05-19T18:28:06.573399Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.35-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고
summary는 `ok=true`, `status=completed`, `executed_steps=2`를 기록했다.

## Artifact

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260520-04235/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260520-04235/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04235/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260520-04235/summary.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04235/PureCVisorDesktopNode-0.42.35-admin-smoke-windows-x64.msi` |

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `pass` | step `service-msi-hyperv-admin-smoke`, exit `0`, duration `151286 ms` |
| OS mutation gate | `pass` | step `os-mutation-gate`, exit `0`, duration `11088 ms` |
| Batch supervisor | `pass` | total `2`, executed `2`, failed step 없음 |
| Package | `pass` | MSI SHA-256 `12d05f2d783dfdb1db3f1596cd266af17578e33fca3f4fec272aac7df5e22697` |

## 후속 fast-follow

이 full gate 자체는 PR #160 merge commit 기준 `0.42.35-admin-smoke` product payload를 검증한다.
이후 설치본 실제 VM `pause/resume/rename` smoke에서 Hyper-V pause 상태값 결함을 발견해
`0.42.37-admin-smoke` fast-follow package와 source fix로 닫았다. 따라서 operational
full gate current anchor는 이 파일이 소유하고, 최신 설치본 CLI lifecycle closure는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04237.md`가 소유한다.

## 경계

이 evidence는 관리자 opt-in host mutation evidence지만 internal admin-smoke 범위다.
Public trusted signing은 `excluded`, 외부 stable publication은 `not-claimed`다.
