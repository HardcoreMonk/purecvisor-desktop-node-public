# Installed update/rollback `0.42.85-admin-smoke` -> `0.42.86-admin-smoke`

evidence_id: `product-update-rollback-2026-10-02-04285-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20261002-04285-04286`
source_payload: `artifacts/admin-smoke-package-20261002-04286/payload`
target_msi_sha256: `8edb19ce8f1b4fc550b6b455acb0fd2509c4bb29b5e3a8b544d8009b6d17b9b6`
target_provenance_commit: `1c488b62bc68783f4673bb4f441f65d5de5c561c`
update_summary_sha256: `e2f40246f9aecadb38968879a2678f0bf7747415ee041b4da062046aeda15aa1`
rollback_summary_sha256: `32b16be5f5202a02015f6e70ec606638c8950f929cd6ddd16859fc0998b33b2f`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

pair readiness 전에 설치본은 `0.42.86-admin-smoke`였다. readiness는 설치본이 baseline과 같을 것을 요구하므로, `REMOVE_DATA` 없이 `{EE323D7C-2343-4110-958A-0453E5D1D39A}`를 제거하고 clean MSI `0.42.85`를 다시 설치했다. 제거 exit `0`, 설치 exit `0`이다. data root의 protected token 파일은 남았다. 보존 VM `pcv-guest-installed-04253-r1`은 Off다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.85-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.85-admin-smoke+f5b6d10fad87543cc5e17c83794fa3e29d7af406` |
| `DesktopNode.previous` | `0.42.84-admin-smoke`(이전 run의 잔여) |
| `DesktopNode.failed` | `0.42.85-admin-smoke`(이전 run의 잔여) |
| service | `PureCVisorDesktopNode` Running |
| VM | `pcv-guest-installed-04253-r1` Off |

wrapper는 저장소의 `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`다.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20261002-04286/payload -Version 0.42.86-admin-smoke -TimeoutSec 180
exit: 0
ok: true
executed: 11 steps (current-manifest, update-payload-preflight, update-transaction.begin, service.stop, service.stop.wait, job-store.pending-commit.guard-before-backup, backup-product-root, copy, config-migration, service.start, health)
manifest_after_update: 0.42.86-admin-smoke
host_after_update: 0.42.86-admin-smoke+1c488b62bc68783f4673bb4f441f65d5de5c561c
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
| product manifest | `0.42.85-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.85-admin-smoke+f5b6d10fad87543cc5e17c83794fa3e29d7af406` |
| `DesktopNode.failed` | `0.42.86-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | Off 유지 |

## Nonclaims

- operational current는 `0.42.84-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
