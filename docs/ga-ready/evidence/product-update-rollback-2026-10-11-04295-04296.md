# Installed update/rollback `0.42.95-admin-smoke` -> `0.42.96-admin-smoke`

evidence_id: `product-update-rollback-2026-10-11-04295-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261011-04295-04296/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261011-04296/PureCVisorDesktopNode-0.42.96-admin-smoke-update-catalog.json`
target_msi_sha256: `326b867a161ffa5038f4b3cecd8405ca0999cff00875c4a9fb8f5ac92f3cdeed`
target_provenance_commit: `e07113c5f555715955856897c687c7c8843db1dc`
update_summary_sha256: `940eb528095b637a714b9094a676caa4480e3152d938746f4d6f2756b8673838`
rollback_summary_sha256: `310e431caf81b109feabec49e475f6d02b4198b176d89332839ba8c8ffc189e4`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.96` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.95-admin-smoke`(Host `+b9898cf`)였다. `DesktopNode.previous`: 없음, `DesktopNode.failed`: `0.42.96-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261011-04296/PureCVisorDesktopNode-0.42.96-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.96-admin-smoke
host_after_update: 0.42.96-admin-smoke+e07113c5f555715955856897c687c7c8843db1dc
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261011-04296/PureCVisorDesktopNode-0.42.96-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.96-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.96-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.96-admin-smoke+e07113c5f555715955856897c687c7c8843db1dc` |
| `DesktopNode.failed` | `0.42.96-admin-smoke` |
| `DesktopNode.previous` | `0.42.95-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.93-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
