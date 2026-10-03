# Installed update/rollback `0.42.87-admin-smoke` -> `0.42.88-admin-smoke`

evidence_id: `product-update-rollback-2026-10-03-04287-04288`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20261003-04287-04288`
source_payload: `artifacts/admin-smoke-package-20261003-04288/payload`
target_msi_sha256: `81ef85273cb9bbf9d813d4c3cce40f88c22e5d596c40906dab8766e4cab79b64`
target_provenance_commit: `ff62e596a949202d905699504cd12ce3c646dc0c`
update_summary_sha256: `fbdaef505a74bba4fe0b91c03bda857d9787222df5fb9f43b54a8f37ee091da7`
rollback_summary_sha256: `5d7950c68f289419e16869f0a00565d379a3c339dc68351f94f3ad661c37464e`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.87-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

설치본은 baseline `0.42.87-admin-smoke`(fullgate build `+8ade930`)였고 readiness와 같아 재설치하지 않았다. `DesktopNode.previous`는 `0.42.86`, `DesktopNode.failed`는 `0.42.87`(이전 run의 잔여)이었다. service Running/Automatic, 보존 VM Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20261003-04288/payload -Version 0.42.88-admin-smoke -TimeoutSec 180
exit: 0
ok: true
executed: 11 steps
manifest_after_update: 0.42.88-admin-smoke
host_after_update: 0.42.88-admin-smoke+ff62e596a949202d905699504cd12ce3c646dc0c
service_after_update: Running
web_after_update: 200
```

## Rollback

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 180
exit: 0
ok: true
executed: 7 steps
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.87-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.87-admin-smoke+8ade930587941e24451f31a352fe2a412841f001` |
| `DesktopNode.failed` | `0.42.88-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | Off 유지 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.87-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
