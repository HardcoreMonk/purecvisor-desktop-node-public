# Installed update/rollback `0.42.91-admin-smoke` -> `0.42.92-admin-smoke`

evidence_id: `product-update-rollback-2026-10-07-04291-04292`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261007-04291-04292/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261007-04292/PureCVisorDesktopNode-0.42.92-admin-smoke-update-catalog.json`
target_msi_sha256: `dc79fdd166f0882a31e726e43909e0ab5b3dda09142de162c72dfa9821b104ec`
target_provenance_commit: `e25095029463943ad75b166f95718a77f7661467`
update_summary_sha256: `6e9977fe84cb05c697f3ba77afdf1bfa4110fc2cd748ca3ba9bb25e154511a62`
rollback_summary_sha256: `6bea809123150eb8ea2557230b779cecc569e43382f1ca5f93e949b7784f4c06`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.92` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.42.91-admin-smoke`(Host `+990a4b2`)였다. `DesktopNode.previous`: `0.42.90-admin-smoke`, `DesktopNode.failed`: `0.42.91-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261007-04292/PureCVisorDesktopNode-0.42.92-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.42.92-admin-smoke
host_after_update: 0.42.92-admin-smoke+e25095029463943ad75b166f95718a77f7661467
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261007-04292/PureCVisorDesktopNode-0.42.92-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.42.92-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.92-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.92-admin-smoke+e25095029463943ad75b166f95718a77f7661467` |
| `DesktopNode.failed` | `0.42.92-admin-smoke` |
| `DesktopNode.previous` | `0.42.91-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.91-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
