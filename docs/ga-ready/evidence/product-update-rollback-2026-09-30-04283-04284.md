# Installed update/rollback `0.42.83-admin-smoke` -> `0.42.84-admin-smoke`

evidence_id: `product-update-rollback-2026-09-30-04283-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20260930-04283-04284`
source_payload: `artifacts/admin-smoke-package-20260930-04284/payload`
target_msi_sha256: `12a582efe989f23de8191ce8e7e98a2fc7638bad2403481ee7384010fa830b87`
target_provenance_commit: `aab0bc1c38cf6badb9f5b31bfbeae99c26bd4063`
signing_mode: `AllowUnsignedDev`
update_summary_sha256: `d456a6b6ca313d67c90dea6314b882fd09e312927a5555cbb0f74ce5d18d958b`
rollback_summary_sha256: `36b45431b5e3431a1755be0c0a81ff2d96c2f93a2245120ec45b213fbae03cb1`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

설치본은 이미 baseline `0.42.83`이었다. 그래서 baseline 재설치는 하지 않았다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.83-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.83-admin-smoke+68462481dee72049a1c0918f201efa2e1c387160` |
| `DesktopNode.previous` | `0.42.78-admin-smoke`(이전 run의 잔여) |
| `DesktopNode.failed` | `0.42.83-admin-smoke`(이전 run의 잔여) |
| service | `PureCVisorDesktopNode` Running |
| boot | `2026-09-30T11:47:33.5000000+09:00` |
| VM | `pcv-guest-installed-04253-r1` Off |

wrapper는 저장소의 `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`다. SHA `086d4912…`로 target payload의 wrapper와 같다.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20260930-04284/payload -Version 0.42.84-admin-smoke -TimeoutSec 180
exit: 0
ok: true
executed: current-manifest, update-payload-preflight, update-transaction.begin, service.stop, service.stop.wait, job-store.pending-commit.guard-before-backup, backup-product-root, copy, config-migration, service.start, health
manifest_after_update: 0.42.84-admin-smoke
host_after_update: 0.42.84-admin-smoke+aab0bc1c38cf6badb9f5b31bfbeae99c26bd4063
service_after_update: Running
web_after_update: 200
```

## Rollback

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 180
exit: 0
ok: true
executed: service.stop, service.stop.wait, job-store.pending-commit.guard-before-restore, restore, job-store.pending-commit.guard-before-rollback-start, service.start, health
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.83-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.83-admin-smoke+68462481dee72049a1c0918f201efa2e1c387160` |
| `DesktopNode.failed` | `0.42.84-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running |
| Web Console | HTTP `200` |
| boot | 변경 없음 |
| VM | Off 유지 |

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-run` |
| MSIX build/install/update/remove | `not-run` |
| full admin host mutation | `not-run` |
