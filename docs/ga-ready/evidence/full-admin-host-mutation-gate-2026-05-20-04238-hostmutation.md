# Full admin host mutation 게이트 2026-05-20 0.42.38

evidence_id: `full-admin-host-mutation-gate-2026-05-20-04238-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.38-admin-smoke`
manual_admin_latest_closed_package_pair: `0.42.37-admin-smoke -> 0.42.38-admin-smoke`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04237-04238-closed`
manual_admin_latest_candidate_package_pair: `0.42.37-admin-smoke -> 0.42.38-admin-smoke`
manual_admin_latest_candidate_status: `pass-closed`
batch_id: `full-admin-host-mutation-gate-20260520-04238`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260520-04238`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04238`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260520-04238`
host_mutation_performed: `true`
full_gate_msi_sha256: `b3090de88edb4724d99bc33c65a046b2fc9184f7ccc6a1f37b50e7ce07685f1f`
payload_aggregate_sha256: `ab5cb6404e8f482ad3ecb32b087cb7e5020aceca595adb0fa01e3aa26d2317b8`
provenance_commit: `3c49b9a010c57e4a8637cb32ed17cd432dd0cd6f`
build_utc: `2026-05-20T09:52:54.2904475Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.38-admin-smoke` 기준 full admin host mutation gate 실행 결과다.
Batch Supervisor는 Service/MSI/Hyper-V route parity와 OS mutation gate를 모두 완료했고
summary는 `ok=true`, `status=completed`, `executed_steps=2`를 기록했다.

## 아티팩트

| 항목 | 값 |
| --- | --- |
| batch manifest | `artifacts/batch-runs/full-admin-host-mutation-gate-20260520-04238/manifest.json` |
| batch summary | `artifacts/batch-runs/full-admin-host-mutation-gate-20260520-04238/summary.json` |
| route summary | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04238/summary.json` |
| OS summary | `artifacts/os-mutation-gates-batch-profile-20260520-04238/summary.json` |
| full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04238/PureCVisorDesktopNode-0.42.38-admin-smoke-windows-x64.msi` |

## PASS 버킷

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| Service/MSI/Hyper-V route | `pass` | step `service-msi-hyperv-admin-smoke`, exit `0`, duration `157223 ms` |
| OS mutation gate | `pass` | step `os-mutation-gate`, exit `0`, duration `11061 ms` |
| Batch supervisor | `pass` | total `2`, executed `2`, failed step 없음 |
| Package | `pass` | MSI SHA-256 `b3090de88edb4724d99bc33c65a046b2fc9184f7ccc6a1f37b50e7ce07685f1f` |

## Host 상태

OS mutation gate는 firewall enable/remove, LAN listener smoke, Event Log register/remove,
internal trust-store install/remove/restore를 완료했다. 최종 상태는 service
`PureCVisorDesktopNode` `Running`/`Auto`, firewall final count `0`, Event Log source
absent, trust Root/TrustedPublisher present, boot time unchanged다.

## Manual-admin 경계

이 full gate는 product payload와 host mutation route parity를 PASS로 올린다. 다만
`0.42.37-admin-smoke -> 0.42.38-admin-smoke` manual-admin package-pair는 dedicated
clean-host Windows Update rerun에서 PASS했고 closure로 승격했다. 최초 baseline MSI
install `1603` / `ConfigureInstalled` blocker는 old base VHD에서 Windows Update를
적용하지 않은 환경성 RCA로 보존한다. 설치본 current-card는 0.42.38 full gate와
manual-admin latest closed package-pair를 모두 0.42.38 anchor로 표시한다.

## 경계

이 evidence는 관리자 opt-in host mutation evidence지만 internal admin-smoke 범위다.
Public trusted signing은 `excluded`, 외부 stable publication은 `not-claimed`다.
