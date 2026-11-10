# Installed update/rollback `0.43.0-admin-smoke` -> `0.43.1-admin-smoke`

evidence_id: `product-update-rollback-2026-11-10-04300-04301`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261110-04300-04301/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261110-04301/PureCVisorDesktopNode-0.43.1-admin-smoke-update-catalog.json`
target_msi_sha256: `37c3163fa024703c3e285d6c8f2343422437b9db933e78a08df4acce88b120f1`
target_provenance_commit: `0be6561e0a22b5163a7214e9de9b5ebb889778cc`
update_summary_sha256: `acd663edeba030acf48441ec69617516e525f4d9039d39ce5f5b980aa1365a96`
rollback_summary_sha256: `dd8d285869cf7a64cadbe9c0a5d705ba637fc3f6db96a051e8b018a4d3c4fe41`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.0-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.1` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.43.0-admin-smoke`(Host `+1f646ee`)였다. `DesktopNode.previous`: `0.42.99-admin-smoke`, `DesktopNode.failed`: `0.43.0-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261110-04301/PureCVisorDesktopNode-0.43.1-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.1-admin-smoke
host_after_update: 0.43.1-admin-smoke+0be6561e0a22b5163a7214e9de9b5ebb889778cc
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261110-04301/PureCVisorDesktopNode-0.43.1-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.1-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.1-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.1-admin-smoke+0be6561e0a22b5163a7214e9de9b5ebb889778cc` |
| `DesktopNode.failed` | `0.43.1-admin-smoke` |
| `DesktopNode.previous` | `0.43.0-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.43.0-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
