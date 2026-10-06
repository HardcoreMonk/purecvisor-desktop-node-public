# Installed update/rollback `0.42.90-admin-smoke` -> `0.42.91-admin-smoke`

evidence_id: `product-update-rollback-2026-10-06-04290-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261006-04290-04291/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261006-04291/PureCVisorDesktopNode-0.42.91-admin-smoke-update-catalog.json`
target_msi_sha256: `bdef7609de3667298325d162641578a85e191ed31075cc6238e8e0f79fbfc12f`
target_provenance_commit: `59cd8b6cc1d95c9b4f3a566354361b3efd6076c5`
update_summary_sha256: `3f5d8022a77d53ceec478fd0b9bf9d8769363a1860b1768d8658271653411029`
rollback_summary_sha256: `1c06d28680d61827e9af17d413fdc47815a591c696eb47fa1889bb79ab8c81c6`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.91` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.90-admin-smoke`(Host `+648139d`)였다. `DesktopNode.previous`: `0.42.89-admin-smoke`, `DesktopNode.failed`: `0.42.90-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261006-04291/PureCVisorDesktopNode-0.42.91-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.91-admin-smoke
host_after_update: 0.42.91-admin-smoke+59cd8b6cc1d95c9b4f3a566354361b3efd6076c5
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261006-04291/PureCVisorDesktopNode-0.42.91-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.91-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.91-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.91-admin-smoke+59cd8b6cc1d95c9b4f3a566354361b3efd6076c5` |
| `DesktopNode.failed` | `0.42.91-admin-smoke` |
| `DesktopNode.previous` | `0.42.90-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.90-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
