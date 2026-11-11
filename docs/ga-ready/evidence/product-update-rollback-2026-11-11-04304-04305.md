# Installed update/rollback `0.43.4-admin-smoke` -> `0.43.5-admin-smoke`

evidence_id: `product-update-rollback-2026-11-11-04304-04305`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04304-04305/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261111-04305/PureCVisorDesktopNode-0.43.5-admin-smoke-update-catalog.json`
target_msi_sha256: `5f4699097d267ef3bddda081d0351f5f14ae286803d1447ce1eeeba5ef727d8d`
target_provenance_commit: `884d442ef0a30079b70061ab0b0cae82c1de4e9b`
update_summary_sha256: `d27cf297c70c65712f887b7375c4fd0904bee71810c3a926439650e62bb18bbd`
rollback_summary_sha256: `7e4a61e06d9501015a48791b1c8300882c02bd0e57fb41fe1bc9126de5918560`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.5` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.43.4-admin-smoke`(Host `+7b929ec`)였다. `DesktopNode.previous`: `0.43.3-admin-smoke`, `DesktopNode.failed`: `0.43.4-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s1r-ubuntu` Running, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04305/PureCVisorDesktopNode-0.43.5-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.5-admin-smoke
host_after_update: 0.43.5-admin-smoke+884d442ef0a30079b70061ab0b0cae82c1de4e9b
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04305/PureCVisorDesktopNode-0.43.5-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.5-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.5-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.5-admin-smoke+884d442ef0a30079b70061ab0b0cae82c1de4e9b` |
| `DesktopNode.failed` | `0.43.5-admin-smoke` |
| `DesktopNode.previous` | `0.43.4-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.43.4-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
