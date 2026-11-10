# Installed update/rollback `0.43.1-admin-smoke` -> `0.43.2-admin-smoke`

evidence_id: `product-update-rollback-2026-11-10-04301-04302`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261110-04301-04302/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261110-04302/PureCVisorDesktopNode-0.43.2-admin-smoke-update-catalog.json`
target_msi_sha256: `fa6afbce618568a95bb941c34ded38c188d7a8c221bfc26810d3ac14111b1c0c`
target_provenance_commit: `b45e8215db21ecd952da38d4c0ebbdbc194a4d89`
update_summary_sha256: `1f316d348e613ee0eb8db137fa1dcf7d25016947eb9ed0984f829a6ea1755e88`
rollback_summary_sha256: `4addc2eefac81fb4cafb9ea8ae067043ce2d176e235262be56665b7fe0e327ad`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.1-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.2` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.43.1-admin-smoke`(Host `+143a55b`)였다. `DesktopNode.previous`: `0.43.0-admin-smoke`, `DesktopNode.failed`: `0.43.1-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261110-04302/PureCVisorDesktopNode-0.43.2-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.2-admin-smoke
host_after_update: 0.43.2-admin-smoke+b45e8215db21ecd952da38d4c0ebbdbc194a4d89
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261110-04302/PureCVisorDesktopNode-0.43.2-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.2-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.2-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.2-admin-smoke+b45e8215db21ecd952da38d4c0ebbdbc194a4d89` |
| `DesktopNode.failed` | `0.43.2-admin-smoke` |
| `DesktopNode.previous` | `0.43.1-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.43.1-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
