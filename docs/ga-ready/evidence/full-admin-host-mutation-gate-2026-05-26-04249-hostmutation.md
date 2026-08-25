# Full admin host mutation gate 2026-05-26 0.42.49

evidence_id: `full-admin-host-mutation-gate-2026-05-26-04249-hostmutation`
result: `PASS`
scope: `full-admin-host-mutation-gate`
version: `0.42.49-admin-smoke`
batch_id: `full-admin-host-mutation-gate-20260526-04249`
artifact_root: `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04249`
routeparity_artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04249`
os_mutation_artifact_root: `artifacts/os-mutation-gates-batch-profile-20260526-04249`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04249.md`
manual_admin_latest_closed_descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04247-04248-closed`
operational_full_gate_msi_sha256: `465e05bbff97accbc2c9bd5cd4d8ddda8fc0e6c4a2052e7790b6fa7b2a796d32`
operational_full_gate_payload_aggregate_sha256: `d49e70c1e291dd28040821fcb659222f4ff524b9c7353994f5e5447ec08610c5`
clean_package_msi_sha256: `322bddcb89b05a882ed323429bcfce29f6a856701b801925b53c37423de0a6e2`
provenance_commit: `4e08d8020f74d4f452e6e0ff3dba0d9602073a43`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

`full-admin-host-mutation-gate-20260526-04249`는 Guest Execution disabled policy/API boundary
payload를 전체 관리자 host mutation gate로 승격한 evidence다. Batch Supervisor는
`service-msi-hyperv-admin-smoke`와 `os-mutation-gate` 두 step을 실행했고 둘 다 exit `0`으로
완료했다.

## 실행 요약

| 항목 | 결과 |
| --- | --- |
| batch status | `completed`, `ok=true`, `executed_steps=2` |
| service/MSI/Hyper-V step | exit `0`, duration `218188 ms`, GPU snapshot `35`, retry count `1` |
| OS mutation gate step | exit `0`, duration `11091 ms`, GPU snapshot `1` |
| route parity artifact | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260526-04249` |
| OS mutation artifact | `artifacts/os-mutation-gates-batch-profile-20260526-04249` |
| final service | `PureCVisorDesktopNode`, `Running`, `Auto` |
| remaining PureCVisor VM | `[]` |
| boot time | unchanged |

## 확인된 경계

Route parity smoke는 MSI build, service-action smoke, MSI install/repair/remove lifecycle,
설치본 Hyper-V API route smoke를 통과했다. OS mutation gate는 config migration,
firewall enable/remove, LAN listener IP smoke, Event Log register/remove, internal
trust-store install/remove/restore를 통과했다.

Installed listener의 `ops summary` release card는 `0.42.49-admin-smoke`,
`msi_sha256=465e05bbff97accbc2c9bd5cd4d8ddda8fc0e6c4a2052e7790b6fa7b2a796d32`,
`git_commit=4e08d8020f74d4f452e6e0ff3dba0d9602073a43`를 노출했다.
Runtime policy는 `guest_execution.enabled=false`, `guest-execution-audit-v1`,
`guest-execution-redaction-v1`, `PCV_GUEST_EXEC_DISABLED` problem code catalog를
노출한다.

이 evidence는 internal admin-smoke host mutation evidence이며 public trusted signing 또는
외부 stable publication evidence가 아니다. 최신 closed manual-admin package-pair closure는
계속 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md`가 소유한다.
