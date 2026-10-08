# Installed update/rollback `0.42.92-admin-smoke` -> `0.42.93-admin-smoke`

evidence_id: `product-update-rollback-2026-10-08-04292-04293`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261008-04292-04293/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261008-04293/PureCVisorDesktopNode-0.42.93-admin-smoke-update-catalog.json`
target_msi_sha256: `13d7f0d476828f865b0d4aca7331a2dcd1a10a8dbe157b9d6a93dc2217f9fb49`
target_provenance_commit: `41421d8bf3272dbdbb8dcf384bcf1ff3f329726a`
update_summary_sha256: `773420b5c352695455b6267da8328f1d40231d6f08c407f95e8bb602e0c68457`
rollback_summary_sha256: `e803058a3de384157ad75c25045139600739fe5cbd2a9328b8f4725bb15add12`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.93` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.92-admin-smoke`(Host `+b51b8cf`)였다. `DesktopNode.previous`: `0.42.91-admin-smoke`, `DesktopNode.failed`: `0.42.92-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261008-04293/PureCVisorDesktopNode-0.42.93-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.93-admin-smoke
host_after_update: 0.42.93-admin-smoke+41421d8bf3272dbdbb8dcf384bcf1ff3f329726a
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261008-04293/PureCVisorDesktopNode-0.42.93-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.93-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.93-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.93-admin-smoke+41421d8bf3272dbdbb8dcf384bcf1ff3f329726a` |
| `DesktopNode.failed` | `0.42.93-admin-smoke` |
| `DesktopNode.previous` | `0.42.92-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.92-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
