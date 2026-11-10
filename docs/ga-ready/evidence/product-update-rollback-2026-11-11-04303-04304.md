# Installed update/rollback `0.43.3-admin-smoke` -> `0.43.4-admin-smoke`

evidence_id: `product-update-rollback-2026-11-11-04303-04304`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04303-04304/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261111-04304/PureCVisorDesktopNode-0.43.4-admin-smoke-update-catalog.json`
target_msi_sha256: `c24ef8317c838d659834b7e77446625d9377e5774f9057fedc813318a739fccb`
target_provenance_commit: `54a817c4ed0a8ab14056405310a998571c99d578`
update_summary_sha256: `396ae9d26dd2d7534fc15e1936ecce08c7cdbfe48edb415c3ddbb2ac67824ef1`
rollback_summary_sha256: `fb4264446982600315d6d41993274cada812cc88a17e02294ea874f0b4e46b97`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.3-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.4` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.43.3-admin-smoke`(Host `+db1a762`)였다. `DesktopNode.previous`: `0.43.2-admin-smoke`, `DesktopNode.failed`: `0.43.3-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04304/PureCVisorDesktopNode-0.43.4-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.4-admin-smoke
host_after_update: 0.43.4-admin-smoke+54a817c4ed0a8ab14056405310a998571c99d578
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04304/PureCVisorDesktopNode-0.43.4-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.4-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.4-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.4-admin-smoke+54a817c4ed0a8ab14056405310a998571c99d578` |
| `DesktopNode.failed` | `0.43.4-admin-smoke` |
| `DesktopNode.previous` | `0.43.3-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.43.3-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
