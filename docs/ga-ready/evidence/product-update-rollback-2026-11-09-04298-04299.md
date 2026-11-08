# Installed update/rollback `0.42.98-admin-smoke` -> `0.42.99-admin-smoke`

evidence_id: `product-update-rollback-2026-11-09-04298-04299`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261109-04298-04299/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261109-04299/PureCVisorDesktopNode-0.42.99-admin-smoke-update-catalog.json`
target_msi_sha256: `7946f464245edf9f7ab84685add805fa7656274a8511525fade9a2c0c6842af0`
target_provenance_commit: `4931d6d59c45a0775f443aa87ea5fe9208467203`
update_summary_sha256: `8411a6c3fc346676c1e59df56e1f67e9b7d154f7b5fe2b9b3aca39cbfee06e73`
rollback_summary_sha256: `8d45b78ce069ade63624f6c80a4532c3ac0b7714b933bc11db058fb65f65f615`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.98-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.99` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.98-admin-smoke`(Host `+2dfe2ac`)였다. `DesktopNode.previous`: `0.42.96-admin-smoke`, `DesktopNode.failed`: `0.42.98-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261109-04299/PureCVisorDesktopNode-0.42.99-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.99-admin-smoke
host_after_update: 0.42.99-admin-smoke+4931d6d59c45a0775f443aa87ea5fe9208467203
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261109-04299/PureCVisorDesktopNode-0.42.99-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.99-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.99-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.99-admin-smoke+4931d6d59c45a0775f443aa87ea5fe9208467203` |
| `DesktopNode.failed` | `0.42.99-admin-smoke` |
| `DesktopNode.previous` | `0.42.98-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.98-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
