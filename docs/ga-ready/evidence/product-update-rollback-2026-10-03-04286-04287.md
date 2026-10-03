# Installed update/rollback `0.42.86-admin-smoke` -> `0.42.87-admin-smoke`

evidence_id: `product-update-rollback-2026-10-03-04286-04287`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20261003-04286-04287`
source_payload: `artifacts/admin-smoke-package-20261003-04287/payload`
target_msi_sha256: `a0041c9f70c6e9003950bb672c74be4d41a8ad2e06fe690481d63266f0dbbcc1`
target_provenance_commit: `8d940dab1e2aae1ad1e4cac5def45eb26a4343ae`
update_summary_sha256: `1b2d120dba38e9aa79b91ee1b0c87eb9e9b8fb4a062799dae8445ea716669241`
rollback_summary_sha256: `fcf7f467308ac035a719fd1ebf9f35efc60437ad93953893ad4f314805ba46af`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

설치본은 baseline `0.42.86-admin-smoke`(fullgate build)였고 readiness와 같아 재설치하지 않았다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.86-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.86-admin-smoke+b807803f778e29c206f1bb2ba8277d2a1136198f` |
| `DesktopNode.previous` | `0.42.85-admin-smoke`(이전 run의 잔여) |
| `DesktopNode.failed` | `0.42.86-admin-smoke`(이전 run의 잔여) |
| service | `PureCVisorDesktopNode` Running / Automatic |
| VM | `pcv-guest-installed-04253-r1` Off |

wrapper는 저장소의 `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`다.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20261003-04287/payload -Version 0.42.87-admin-smoke -TimeoutSec 180
exit: 0
ok: true
executed: 11 steps (current-manifest, update-payload-preflight, update-transaction.begin, service.stop, service.stop.wait, job-store.pending-commit.guard-before-backup, backup-product-root, copy, config-migration, service.start, health)
manifest_after_update: 0.42.87-admin-smoke
host_after_update: 0.42.87-admin-smoke+8d940dab1e2aae1ad1e4cac5def45eb26a4343ae
service_after_update: Running
web_after_update: 200
```

## Rollback

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 180
exit: 0
ok: true
executed: 7 steps (service.stop, service.stop.wait, job-store.pending-commit.guard-before-restore, restore, job-store.pending-commit.guard-before-rollback-start, service.start, health)
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.86-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.86-admin-smoke+b807803f778e29c206f1bb2ba8277d2a1136198f` |
| `DesktopNode.failed` | `0.42.87-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | Off 유지 |
| 재부팅 | 없음 (boot `2026-10-03T14:25:44+09:00` 유지) |

## Nonclaims

- operational current는 `0.42.86-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
