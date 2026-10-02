# Installed update/rollback `0.42.84-admin-smoke` -> `0.42.85-admin-smoke`

evidence_id: `product-update-rollback-2026-10-01-04284-04285`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
artifact_root: `artifacts/product-update-rollback-20261001-04284-04285`
source_payload: `artifacts/admin-smoke-package-20261001-04285/payload`
target_msi_sha256: `cba74683e6ae9f7e85e5f625f8e9b9f221ae27c8ffacf5912ce2861f1ef82828`
target_provenance_commit: `f5b6d10fad87543cc5e17c83794fa3e29d7af406`
signing_mode: `AllowUnsignedDev`
update_summary_sha256: `2d93a49fa0ee0663c6dd7f9a66aedb00ac4fffc57f4a310eaf6a5bc2c423b8db`
rollback_summary_sha256: `6e33aa0165181e54e8b9b72f8bfe757903292739951416248b45f004095e8a35`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 사전 상태

설치본은 이미 baseline `0.42.84`였다. 그래서 baseline 재설치는 하지 않았다.

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.84-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.84-admin-smoke+ee90e0ea2c042e21cd0bf0346440ae2db41dfe43` |
| `DesktopNode.previous` | `0.42.83-admin-smoke`(이전 run의 잔여) |
| `DesktopNode.failed` | `0.42.84-admin-smoke`(이전 run의 잔여) |
| service | `PureCVisorDesktopNode` Running |
| boot | `2026-09-30T11:47:33.5000000+09:00` |
| VM | `pcv-guest-installed-04253-r1` Off |

wrapper는 저장소의 `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`다. SHA `086d4912…`로 target payload의 wrapper와 같다.

## Update

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/admin-smoke-package-20261001-04285/payload -Version 0.42.85-admin-smoke -TimeoutSec 180
exit: 0
ok: true
executed: 11 steps (current-manifest ... service.start, health), same as the 0.42.84 run
manifest_after_update: 0.42.85-admin-smoke
host_after_update: 0.42.85-admin-smoke+f5b6d10fad87543cc5e17c83794fa3e29d7af406
service_after_update: Running
web_after_update: 200
```

## Rollback

```text
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 180
exit: 0
ok: true
executed: 7 steps (service.stop ... service.start, health), same as the 0.42.84 run
```

## 최종 상태

| 항목 | 값 |
| --- | --- |
| product manifest | `0.42.84-admin-smoke` |
| `DesktopNode.Host.exe` ProductVersion | `0.42.84-admin-smoke+ee90e0ea2c042e21cd0bf0346440ae2db41dfe43` |
| `DesktopNode.failed` | `0.42.85-admin-smoke` |
| `DesktopNode.previous` | 없음 |
| service | Running |
| Web Console | HTTP `200` |
| boot | 변경 없음 |
| VM | Off 유지 |
