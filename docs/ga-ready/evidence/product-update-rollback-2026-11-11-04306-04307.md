# Installed update/rollback `0.43.6-admin-smoke` -> `0.43.7-admin-smoke`

evidence_id: `product-update-rollback-2026-11-11-04306-04307`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261111-04307/PureCVisorDesktopNode-0.43.7-admin-smoke-update-catalog.json`
target_msi_sha256: `a808cda4d54eae4caf012a8d7b012663b6caa46f683741599150dae7e0223a31`
target_provenance_commit: `74b956256c585c032536a117281dadc128d9794a`
update_summary_sha256: `c1d01f96a9bd2717708d7776b208a9fa8b1830a9b2996353268059baec7f7a50`
rollback_summary_sha256: `3f9c22e597e1f91415be912a7e4689653d45547744a26990f6bf6c04ef31e4ae`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.7` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.43.6-admin-smoke`(Host `+5758b54`)였다. `DesktopNode.previous`: `0.43.7-admin-smoke`, `DesktopNode.failed`: `0.43.7-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04307/PureCVisorDesktopNode-0.43.7-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.7-admin-smoke
host_after_update: 0.43.7-admin-smoke+74b956256c585c032536a117281dadc128d9794a
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04307/PureCVisorDesktopNode-0.43.7-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.7-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.7-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.7-admin-smoke+74b956256c585c032536a117281dadc128d9794a` |
| `DesktopNode.failed` | `0.43.7-admin-smoke` |
| `DesktopNode.previous` | `0.43.6-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.43.4-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
