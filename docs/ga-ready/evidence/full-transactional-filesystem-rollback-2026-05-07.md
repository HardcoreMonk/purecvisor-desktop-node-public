# Full Transactional Filesystem Rollback Evidence - 2026-05-07

```text
evidence_id: full-transactional-filesystem-rollback-2026-05-07
```

## 요약

2026-05-07 후속 slice는 packaging/distribution future phase의 transactional rollback 중 product root filesystem rollback 보장을 code-level로 확장했다.

- Product update policy의 `update.transaction_journal.full_transactional_filesystem`은 `true`다.
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`는 payload/source/catalog preflight와 journal begin 이후 service stop, service stopped wait, product root backup, copy, config migration dry-run, service start, health 순서로 실행한다.
- Product root backup이 완료된 뒤 copy/config/start/health 등 후속 단계에서 실패하면 catch 경로가 previous product root restore를 시도한다.
- Rollback restore가 성공하면 journal final status는 `failed-rolled-back`이다.
- Rollback restore 자체가 실패하면 journal final status는 `failed-rollback-failed`이고 original `PCV_*` error와 rollback result를 함께 남긴다.
- Copy 실패처럼 기존에 explicit rollback path가 없던 파일 단계도 `rollback.restore` executed step과 `rollback_result.restored=true`를 기록한다.

## 범위

이 evidence는 product root filesystem rollback code-level evidence다. 다음 항목은 완료로 주장하지 않는다.

- post-crash resume/reconcile engine
- service/data/config/job-store 전체 transaction manager
- rollback 후 previous service health 재검증 자동화
- external stable publication
- public trusted signing
- installed destructive catalog/update rollback smoke
- MSI/service/firewall/trust-store/LAN/Event Log mutation

실제 host mutation은 수행하지 않았다.

## 구현 계약

Product plan의 `update.transaction_journal`은 다음 계약을 노출한다.

```text
mode: single-active-update-journal
path: %ProgramData%\PureCVisor\desktop-node\update-transaction.json
write_before_service_stop: true
record_stage_transitions: true
full_transactional_filesystem: true
```

Backup 이후 update failure는 다음 final-state를 남긴다.

```text
update.rollback_attempted: true
update.rollback_result.restored: true
update.transaction_journal.status: failed-rolled-back
update.transaction_journal.rollback_attempted: true
update.transaction_journal.rollback_result.restored: true
```

## 검증

RED:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

결과:

```text
Expected $true, but got $false.
Expected 'restore-previous-root' to be found in collection @('service.stop', 'service.status', 'backup-product-root', 'copy-assets'), but it was not found.
```

GREEN:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
```

결과:

```text
Tests Passed: 71
Failed: 0
```

검증된 behavior:

- product plan은 `full_transactional_filesystem: true`를 노출한다.
- backup 이후 copy failure는 service start 또는 health 단계까지 가지 않고 previous root restore를 시도한다.
- copy failure rollback은 `rollback.restore` executed step을 기록한다.
- journal은 `stage=copy`, `status=failed-rolled-back`, original `PCV_PRODUCT_COPY_FAILED`, `rollback_result.restored=true`를 기록한다.
- 기존 service start failure와 health failure rollback behavior는 계속 통과한다.

## 판정

Product root filesystem rollback은 code-level로 확장됐다. Post-crash resume/reconcile, service/data/config/job-store 전체 transaction manager, rollback 후 previous service health 재검증 자동화, installed destructive catalog/update rollback smoke는 별도 future gate다.

이 evidence는 internal code-level packaging evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
