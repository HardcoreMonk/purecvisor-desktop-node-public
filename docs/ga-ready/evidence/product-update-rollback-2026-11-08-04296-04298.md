# Installed update/rollback `0.42.96-admin-smoke` -> `0.42.98-admin-smoke`

evidence_id: `product-update-rollback-2026-11-08-04296-04298`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261108-04296-04298/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261108-04298/PureCVisorDesktopNode-0.42.98-admin-smoke-update-catalog.json`
target_msi_sha256: `9a26d4cee9c4360cb8a04ce520b57de4d2e99c3e7b64997d8b4567e7c04733b0`
target_provenance_commit: `54cb61ef3128df7d57191aefd0f9a684b8de2f8f`
update_summary_sha256: `b7d8e6e21c17fe01960fc2ba283ef661cbc2241a3be2ca6c1586dd87347a2d0e`
rollback_summary_sha256: `1dc098d6ae9fbc4482d06d6e62b509b8bdabdd78fe204cef7c0fe2c1cb15a933`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.96-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.98` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.96-admin-smoke`(Host `+9ac8eb3`)였다. `DesktopNode.previous`: 없음, `DesktopNode.failed`: `0.42.97-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off, `pcv-it-s4-console` Running.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261108-04298/PureCVisorDesktopNode-0.42.98-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.98-admin-smoke
host_after_update: 0.42.98-admin-smoke+54cb61ef3128df7d57191aefd0f9a684b8de2f8f
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261108-04298/PureCVisorDesktopNode-0.42.98-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.98-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.98-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.98-admin-smoke+54cb61ef3128df7d57191aefd0f9a684b8de2f8f` |
| `DesktopNode.failed` | `0.42.98-admin-smoke` |
| `DesktopNode.previous` | `0.42.96-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.96-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
