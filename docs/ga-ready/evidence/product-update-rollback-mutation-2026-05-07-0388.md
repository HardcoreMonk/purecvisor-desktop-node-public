# Product Update/Rollback Mutation Evidence - 2026-05-07 0.38.8

```text
evidence_id: product-update-rollback-mutation-2026-05-07-0388
build_and_blocked_attempt_root: artifacts/product-update-rollback-mutation-20260507-0388
elevated_pass_root: artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass
```

## 요약

2026-05-07 `0.38.8-admin-smoke` installed destructive update/rollback smoke는 두 단계로 확인했다. 최초 `artifacts/product-update-rollback-mutation-20260507-0388` attempt는 non-elevated shell에서 service stop 권한이 없어 blocked evidence로 남긴다. 이후 elevated shell에서 같은 `0.38.8-admin-smoke` payload를 사용해 실제 update와 rollback을 실행했고 PASS했다.

- Build: PASS
- Elevated update: PASS, `0.38.6-admin-smoke -> 0.38.8-admin-smoke`
- Update journal: `succeeded/health`
- Health: `200`
- Elevated rollback: PASS, current product root restored to `0.38.6-admin-smoke`
- Final service: `Running`
- Final installed product manifest: `0.38.6-admin-smoke`
- Rolled-back product root: `C:\Program Files\PureCVisor\DesktopNode.failed`, manifest `0.38.8-admin-smoke`, preserved for diagnostics
- Host mutation performed: `true`
- `host_mutation_performed=true`
- Boot time unchanged: `true`

## Build Evidence

```text
version: 0.38.8-admin-smoke
artifact_root: artifacts/product-update-rollback-mutation-20260507-0388
build_ok: true
signing_mode: AllowUnsignedDev
provenance_commit: fd4f854646fc159d54f7578230f00c51f80e201f
msi_sha256: 163baa1df75b5810efa49d6347f482077421b1665f29a7adc2e501cdbc3a7564
payload_aggregate_sha256: 57be028cc5b9f9bf1b2a371c597f339b49e7b376bd60d0ff9bb6e854964260a6
```

## Elevated Update PASS

```text
artifact_root: artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/product-update-rollback-mutation-20260507-0388/payload -Version 0.38.8-admin-smoke -TimeoutSec 60
exit: 0
result_ok: true
from_version: 0.38.6-admin-smoke
to_version: 0.38.8-admin-smoke
executed_steps: current-manifest, update-payload-preflight, update-transaction.begin, service.stop, service.stop.wait, backup-product-root, copy, config-migration, service.start, health
transaction_journal_status: succeeded
transaction_journal_stage: health
health_status_code: 200
```

Update는 service stop/wait, product root backup, payload copy, config migration dry-run, service start, bearer-protected runtime policy health check를 완료했다.

## Elevated Rollback PASS

```text
artifact_root: artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass
command: Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 60
exit: 0
result_ok: true
previous_version: 0.38.6-admin-smoke
executed_steps: service.stop, service.stop.wait, restore, service.start, health
final_manifest_version: 0.38.6-admin-smoke
failed_root_manifest_version: 0.38.8-admin-smoke
previous_root_exists_after_rollback: false
failed_root_exists_after_rollback: true
final_service_state: Running
```

Rollback은 previous product root를 current root로 복원하고 service health check까지 완료했다. Rollback된 `0.38.8-admin-smoke` root는 `DesktopNode.failed`로 보존되어 rollback diagnostics를 제공한다.

## Non-Elevated Blocked Attempt

```text
artifact_root: artifacts/product-update-rollback-mutation-20260507-0388
update_exit: 1
update_error_code: PCV_PRODUCT_COMMAND_FAILED
update_detail: sc.exe stop PureCVisorDesktopNode exited with code 5
rollback_exit: 1
rollback_error_code: PCV_PRODUCT_SERVICE_STOP_TIMEOUT
host_mutation_performed: false
final_manifest_version: 0.38.6-admin-smoke
```

이 blocked attempt는 elevation preflight/evidence 이력으로만 유지한다. PASS 판정의 근거는 `artifacts/product-update-rollback-mutation-20260507-0388-elevated-pass`다.

## 판정

`0.38.8-admin-smoke` installed destructive update/rollback smoke는 elevated PASS다. 이 evidence는 `AllowUnsignedDev` admin-smoke 범위이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
