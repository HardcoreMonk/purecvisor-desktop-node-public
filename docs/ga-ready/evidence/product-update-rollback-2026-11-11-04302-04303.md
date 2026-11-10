# Installed update/rollback `0.43.2-admin-smoke` -> `0.43.3-admin-smoke`

evidence_id: `product-update-rollback-2026-11-11-04302-04303`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/manual-admin-campaign-20261111-04302-04303/lifecycle/product-update-rollback`
source_payload: `artifacts/admin-smoke-package-20261111-04303/PureCVisorDesktopNode-0.43.3-admin-smoke-update-catalog.json`
target_msi_sha256: `66546521fb0fea092de8d0fd390153cc337108d9c17d5622015b748d901de060`
target_provenance_commit: `e1e1b8b47c68a4d54af4d1fe8e85b9a3c2355eeb`
update_summary_sha256: `62c608ac99867155c76ceebe43f738c32fcf71ab92715cdd0b056d6b62d76da7`
rollback_summary_sha256: `be2d9ea832a62becc27dffe87d61283891bc65af2591a8f104b283416b08aef3`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.43.3` pair orchestrator의 update/rollback bucket이다. 시작 전 설치본은 baseline `0.43.2-admin-smoke`(Host `+2f19778`)였다. `DesktopNode.previous`: `0.43.1-admin-smoke`, `DesktopNode.failed`: `0.43.2-admin-smoke`, service Running/Automatic, VM `pcv-guest-installed-04253-r1` Off, `pcv-it-s2-source` Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04303/PureCVisorDesktopNode-0.43.3-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: update-summary.json
manifest_after_update: 0.43.3-admin-smoke
host_after_update: 0.43.3-admin-smoke+e1e1b8b47c68a4d54af4d1fe8e85b9a3c2355eeb
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
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -UpdateCatalogUri artifacts/admin-smoke-package-20261111-04303/PureCVisorDesktopNode-0.43.3-admin-smoke-update-catalog.json -UpdateChannel admin-smoke
exit: 0
ok: true
summary: final-update-summary.json, manifest 0.43.3-admin-smoke
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.43.3-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.43.3-admin-smoke+e1e1b8b47c68a4d54af4d1fe8e85b9a3c2355eeb` |
| `DesktopNode.failed` | `0.43.3-admin-smoke` |
| `DesktopNode.previous` | `0.43.2-admin-smoke` |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | 변화 없음 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.43.2-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
