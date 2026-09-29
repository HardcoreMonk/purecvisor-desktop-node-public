# Installed update/rollback `0.42.78-admin-smoke` -> `0.42.83-admin-smoke`

evidence_id: `product-update-rollback-2026-09-29-04278-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20260929-04278-04283`
source_payload: `artifacts/admin-smoke-package-20260929-04283/payload`
target_msi_sha256: `52d7cfd5b923f19ca3c48ade66f682183c5103ae8e451aa29cd4c2e8769c5524`
target_provenance_commit: `bcda14f0f02c256acc125dafbd43da3b4a93b3d0`
signing_mode: `AllowUnsignedDev`
update_summary_sha256: `cfaa54423b9b115657ed37d4a495a84440adb42bccc593d375958feb0b56c609`
rollback_summary_sha256: `c4d6d1fcb08560221d89bf4a4d0d89ee678b7811f03779f7b12b196e8e8ffb25`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

Task 2a에서 probe `0.42.82`를 MSI로 제거하고 baseline `0.42.78` MSI를 설치했다(`manual-admin-campaign-20260929-04278-04283/baseline-install`).

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.78-admin-smoke` |
| `DesktopNode.previous` | `0.42.77-admin-smoke`(이전 run의 잔여) |
| `DesktopNode.failed` | `0.42.78-admin-smoke`(이전 run의 잔여) |
| service | `PureCVisorDesktopNode` Running |
| boot | `2026-09-29T18:02:43.5000000+09:00` |
| VM | `pcv-guest-installed-04253-r1` Off |

wrapper는 저장소의 `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`다. SHA `086d4912…`로 target payload의 wrapper와 같다.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20260929-04283/payload -Version 0.42.83-admin-smoke -TimeoutSec 180
exit: 0
ok: true
from_version: 0.42.78-admin-smoke
to_version: 0.42.83-admin-smoke
rollback_attempted: false
executed: current-manifest, update-payload-preflight, update-transaction.begin, service.stop, service.stop.wait, job-store.pending-commit.guard-before-backup, backup-product-root, copy, config-migration, service.start, health
manifest_after_update: 0.42.83-admin-smoke
service_after_update: Running
```

## Rollback

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 180
exit: 0
ok: true
previous_version: 0.42.78-admin-smoke
failed_root: C:\Program Files\PureCVisor\DesktopNode.failed
failed_root_preserved_for_diagnostics: true
executed: service.stop, service.stop.wait, job-store.pending-commit.guard-before-restore, restore, job-store.pending-commit.guard-before-rollback-start, service.start, health
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.78-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.78-admin-smoke+e098e0a55333afe7eccd9150c5ef9ca14578cc40` |
| `DesktopNode.failed` | `0.42.83-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running |
| boot | 변경 없음 |
| VM | Off 유지 |

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-run` |
| MSIX build/install/update/remove | `not-run` |
| installed runtime ops summary | `not-run` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `0.42.78-admin-smoke -> 0.42.83-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.78-admin-smoke` |

## Nonclaims

- 설치본 update/rollback bucket 하나의 PASS다. pair 전체의 PASS가 아니다.
- operational current는 `0.42.78-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
