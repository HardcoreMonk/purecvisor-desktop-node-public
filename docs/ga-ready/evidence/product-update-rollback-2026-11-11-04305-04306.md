# Installed update/rollback `0.43.5-admin-smoke` -> `0.43.6-admin-smoke`

evidence_id: `product-update-rollback-2026-11-11-04305-04306`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04305-04306/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261111-04306/PureCVisorDesktopNode-0.43.6-admin-smoke-update-catalog.json`
target_msi_sha256: `59106d65ad70f57ab4cb3d9bd5b5f7cf6dde9f333af76d66ce44d9ba00dd98bd`
target_provenance_commit: `5758b54a230d52519437cbec7d15b58a4662419e`
update_summary_sha256: `9234092f7b477e79b131319a5cd0664d79b7c13de98a20cc071384a29158ffa5`
rollback_summary_sha256: `046f2950d688f895a3655cd40ebb1f0bae2b42557427c4b6342fe8a275cbe6c2`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.6` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.43.5-admin-smoke`(Host `+8173777`)였다. `DesktopNode.previous`: `0.43.4-admin-smoke`, `DesktopNode.failed`: `0.43.5-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04306/PureCVisorDesktopNode-0.43.6-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.6-admin-smoke
host_after_update: 0.43.6-admin-smoke+5758b54a230d52519437cbec7d15b58a4662419e
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04306/PureCVisorDesktopNode-0.43.6-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.6-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.6-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.6-admin-smoke+5758b54a230d52519437cbec7d15b58a4662419e` |
| `DesktopNode.failed` | `0.43.6-admin-smoke` |
| `DesktopNode.previous` | `0.43.5-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.43.4-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
