# Installed update/rollback `0.42.94-admin-smoke` -> `0.42.95-admin-smoke`

evidence_id: `product-update-rollback-2026-10-10-04294-04295`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261010-04294-04295/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261010-04295/PureCVisorDesktopNode-0.42.95-admin-smoke-update-catalog.json`
target_msi_sha256: `50680d3c757596e8fce85b3cbb41a7228e8a1ced244678d620fe912f371fab24`
target_provenance_commit: `b401d949a0d17eece428aaef83330e9e5f20ed59`
update_summary_sha256: `ca293100e9c597c99a369d35f6bfa56d9233ad143277337c639a967b85e0b5ea`
rollback_summary_sha256: `77ad142ad6c42b94703b4c20eae4f19d37506db32d8ec9facee139b58fb1785f`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.95` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.94-admin-smoke`(Host `+3d50f14`)였다. `DesktopNode.previous`: `0.42.93-admin-smoke`, `DesktopNode.failed`: `0.42.94-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261010-04295/PureCVisorDesktopNode-0.42.95-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.95-admin-smoke
host_after_update: 0.42.95-admin-smoke+b401d949a0d17eece428aaef83330e9e5f20ed59
service_after_update: Running
web_after_update: 200
```

## Rollback

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback
exit: 0
ok: true
summary: rollback-summary.json
```

## 최종 Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261010-04295/PureCVisorDesktopNode-0.42.95-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.95-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.95-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.95-admin-smoke+b401d949a0d17eece428aaef83330e9e5f20ed59` |
| `DesktopNode.failed` | `0.42.95-admin-smoke` |
| `DesktopNode.previous` | `0.42.94-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.93-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
