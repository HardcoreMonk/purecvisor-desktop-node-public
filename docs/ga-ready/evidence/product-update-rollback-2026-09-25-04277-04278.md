# Installed update/rollback `0.42.77-admin-smoke` -> `0.42.78-admin-smoke`

evidence_id: `product-update-rollback-2026-09-25-04277-04278`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20260925-04277-04278`
source_payload: `artifacts/admin-smoke-package-20260925-04278/payload`
target_msi_sha256: `c3390c1e06f77ccb77dc30bd1354c846d09602c28900e3e9e3782d08cc8f5a6d`
target_provenance_commit: `e098e0a55333afe7eccd9150c5ef9ca14578cc40`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.77-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.77-admin-smoke` |
| `DesktopNode.previous` | `0.42.75-admin-smoke` |
| service | `PureCVisorDesktopNode` Running |
| boot | `2026-09-24T21:02:47.5000000+09:00` |
| VM | `pcv-cleanhost-20260910-r2-04274-04275` Saved, `pcv-guest-installed-04253-r1` Off |

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20260925-04278/payload -Version 0.42.78-admin-smoke -TimeoutSec 180
exit: 0
ok: true
from_version: 0.42.77-admin-smoke
to_version: 0.42.78-admin-smoke
transaction_journal_status: succeeded
transaction_journal_stage: health
steps: current-manifest, update-payload-preflight, update-transaction.begin, service.stop, service.stop.wait, job-store.pending-commit.guard-before-backup, backup-product-root, copy, config-migration, service.start, health
manifest_after_update: 0.42.78-admin-smoke
service_after_update: Running
```

Update는 기존 `DesktopNode.previous`(`0.42.75-admin-smoke`)를 staging 후 제거하고, 당시
product root `0.42.77-admin-smoke`를 previous로 옮긴 뒤 payload를 복사했다.

## Rollback

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 180
exit: 0
ok: true
previous_version: 0.42.77-admin-smoke
failed_root: C:\Program Files\PureCVisor\DesktopNode.failed
steps: service.stop, service.stop.wait, job-store.pending-commit.guard-before-restore, restore, job-store.pending-commit.guard-before-rollback-start, service.start, health
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.77-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.77-admin-smoke+04b3c9ff1fb146db42a3a08a5d8566075b7bb3a6` |
| `DesktopNode.failed` | `0.42.78-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running |
| boot | 변경 없음 |
| VM | Saved / Off 유지 |

## 아직 닫히지 않은 pair bucket

| 후속 검증 | 결과 |
| --- | --- |
| dedicated clean-host Windows Update | `not-run` |
| Burn install/repair/remove | `not-run` |
| MSIX build/install/update/remove | `not-run` |
| full admin host mutation | `not-run` |
| installed current-card | `not-run` |
| `0.42.77-admin-smoke -> 0.42.78-admin-smoke` pair | `not-closed` |
| `docs/ga-ready/current-evidence.json` | 유지 `0.42.77-admin-smoke` |

## Nonclaims

- 이 기록은 설치본 filesystem update와 rollback만 증명한다.
- operational current를 `0.42.78-admin-smoke`로 올리지 않았다.
- public trusted signing과 external stable publication을 주장하지 않는다.
