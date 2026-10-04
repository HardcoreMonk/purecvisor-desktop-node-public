# Installed update/rollback `0.42.88-admin-smoke` -> `0.42.89-admin-smoke`

evidence_id: `product-update-rollback-2026-10-04-04288-04289`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20261004-04288-04289`
source_payload: `artifacts/admin-smoke-package-20261004-04289/payload`
target_msi_sha256: `e4574861a06537aacf41f16e75138e9f8cc9c5d4c8f0df7d7e977bd58e0b7391`
target_provenance_commit: `b463903153a6ffe75b438e673c231e78d4c99909`
update_summary_sha256: `0eba53110af40aa8ac9edfa201fb0cf14f84220f324a14ed034c5c95980962a2`
rollback_summary_sha256: `a148b101121d94c048a6bf38275f7799161cb8ede9d7528c195a21df4a41bf7d`
signing_mode: `AllowUnsignedDev`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

release train `0.42.89` pair다. 설치본은 baseline `0.42.88-admin-smoke`(fullgate build `+47ff198`)였고 readiness와 같아 재설치하지 않았다. `DesktopNode.previous`는 `0.42.87`, `DesktopNode.failed`는 `0.42.88`(이전 run의 잔여)이었다. service Running/Automatic, 보존 VM Off.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20261004-04289/payload -Version 0.42.89-admin-smoke -TimeoutSec 180
exit: 0
ok: true
executed: 11 steps
manifest_after_update: 0.42.89-admin-smoke
host_after_update: 0.42.89-admin-smoke+b463903153a6ffe75b438e673c231e78d4c99909
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
| product manifest | `0.42.88-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.88-admin-smoke+47ff198de86d77d25aa90fa095a294254c7ffef6` |
| `DesktopNode.failed` | `0.42.89-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running / Automatic |
| Web Console | HTTP `200` |
| VM | Off 유지 |
| 재부팅 | 없음 |

## Nonclaims

- operational current는 `0.42.88-admin-smoke`다.
- public trusted signing과 external stable publication을 주장하지 않는다.
