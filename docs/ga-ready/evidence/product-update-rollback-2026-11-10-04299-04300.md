# Installed update/rollback `0.42.99-admin-smoke` -> `0.43.0-admin-smoke`

evidence_id: `product-update-rollback-2026-11-10-04299-04300`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261110-04299-04300/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261110-04300/PureCVisorDesktopNode-0.43.0-admin-smoke-update-catalog.json`
target_msi_sha256: `1eb5d98424ba72c6cb42b7ef5d9c7bbe3778c29ab49a0ae28494bb0b9e1997ec`
target_provenance_commit: `87eb923659ff55d24cf30674b27dd32b19be8c13`
update_summary_sha256: `17ed39f8b6e842e2f521c7168964dfbd85e59a6e799d788a91a1ad64a5427ece`
rollback_summary_sha256: `7ffa4d53f4133a805a5bfd72a77e62718527c56c893fef97f4025f7b6b1309c9`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.99-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.0` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.99-admin-smoke`(Host `+a95987b`)였다. `DesktopNode.previous`: `0.42.98-admin-smoke`, `DesktopNode.failed`: `0.42.99-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261110-04300/PureCVisorDesktopNode-0.43.0-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.0-admin-smoke
host_after_update: 0.43.0-admin-smoke+87eb923659ff55d24cf30674b27dd32b19be8c13
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261110-04300/PureCVisorDesktopNode-0.43.0-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.0-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.0-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.0-admin-smoke+87eb923659ff55d24cf30674b27dd32b19be8c13` |
| `DesktopNode.failed` | `0.43.0-admin-smoke` |
| `DesktopNode.previous` | `0.42.99-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.99-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
