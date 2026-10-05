# Installed update/rollback `0.42.89-admin-smoke` -> `0.42.90-admin-smoke`

evidence_id: `product-update-rollback-2026-10-05-04289-04290`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261005-04289-04290/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261005-04290/PureCVisorDesktopNode-0.42.90-admin-smoke-update-catalog.json`
target_msi_sha256: `54277baafea5be820572c082a874b75c23f55ca3fab0c374cd2b6ca357012146`
target_provenance_commit: `0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c`
update_summary_sha256: `5a7e7aa2e5c0fb8b7b53902955fd3bb206457e410cd9fb274bff12aaf60777c3`
rollback_summary_sha256: `4055ae4d796065d0782c07212a2cf0d22424150d46a31a4cef6287612173d8d2`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.90` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.89-admin-smoke`(Host `+b7fe7b2`)였다. `DesktopNode.previous`: `0.42.88-admin-smoke`, `DesktopNode.failed`: `0.42.89-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261005-04290/PureCVisorDesktopNode-0.42.90-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.90-admin-smoke
host_after_update: 0.42.90-admin-smoke+0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261005-04290/PureCVisorDesktopNode-0.42.90-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.90-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.90-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.90-admin-smoke+0bcc328ba1fcec2fbd8f98e44cf1ec97c8d4569c` |
| `DesktopNode.failed` | `0.42.90-admin-smoke` |
| `DesktopNode.previous` | `0.42.89-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.89-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
