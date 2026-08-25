# Update Transaction Journal Diagnostics Evidence - 2026-05-07

```text
evidence_id: update-transaction-journal-diagnostics-2026-05-07
```

## 요약

2026-05-07 후속 slice는 packaging/distribution future phase의 `full transactional rollback` 중 update transaction journal diagnostics만 code-level로 구현했다.

- Product plan은 `%ProgramData%\PureCVisor\desktop-node\update-transaction.json` 단일 active update journal 경로를 노출한다.
- `Invoke-PcvDesktopNodeProduct.ps1 -Action Update`는 update payload validation이 끝난 직후, service stop 전에 `update-transaction.begin`을 실행하고 journal을 쓴다.
- Journal은 `from_version`, `to_version`, source root, payload validation, stage transition, `service_mutation_started`, `host_mutation_performed`, `rollback_attempted`, rollback result, structured `PCV_*` error를 기록한다.
- service start 실패나 health 실패로 rollback이 시도되면 final status를 `failed-rolled-back`으로 남긴다.
- successful update는 final status `succeeded`, stage `health`로 journal을 갱신한다.
- Diagnostic bundle은 기존 update policy, migration plan, rollback state에 더해 `update-transaction-journal-redacted.json`을 선택적으로 포함한다.

## 범위

이 evidence는 update/rollback diagnostics code-level evidence다. 다음 항목은 완료로 주장하지 않는다.

- full transactional filesystem rollback
- service/data/config/job-store 전체 transaction manager
- post-crash resume/reconcile engine
- external stable publication
- public trusted signing
- installed destructive update/rollback smoke
- MSI/service/firewall/trust-store/LAN/Event Log mutation

실제 host mutation은 수행하지 않았다.

## 구현 계약

Product plan의 `update.transaction_journal`은 다음 계약을 노출한다.

```text
mode: single-active-update-journal
path: %ProgramData%\PureCVisor\desktop-node\update-transaction.json
write_before_service_stop: true
record_stage_transitions: true
full_transactional_filesystem: false
```

`Update` 실행 결과는 journal descriptor를 반환한다.

```text
update.transaction_journal.path
update.transaction_journal.transaction_id
update.transaction_journal.status
update.transaction_journal.stage
```

## 검증

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

결과:

```text
Tests Passed: 82
Failed: 0
```

후속 전체 영향 범위 확인:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/tests/PcvDesktopNode.DocumentationSync.Tests.ps1' -Output Detailed"
git diff --check
```

결과:

```text
Packaging tests passed: 153
Documentation sync tests passed: 15
git diff --check: pass
```

검증된 behavior:

- plan에 update transaction journal path와 policy contract가 포함된다.
- successful update는 `update-transaction.begin`을 service stop 전에 실행하고 journal을 `succeeded`로 마감한다.
- service start failure는 previous root rollback을 시도하고 journal을 `failed-rolled-back`과 `PCV_PRODUCT_UPDATE_START_FAILED`로 마감한다.
- diagnostic bundle manifest는 existing journal이 있으면 `update_transaction_journal` source를 포함한다.
- redacted diagnostic artifact는 `update-transaction-journal-redacted.json`으로 생성된다.

## 판정

`full transactional rollback` 전체는 아직 future distribution phase다. 다만 update mutation 전후 상태와 rollback/error diagnostics를 operator evidence로 남기는 단일 active journal은 code-level로 구현됐다.

이 evidence는 internal code-level packaging evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.

후속 product root filesystem rollback code-level evidence는 `docs/ga-ready/evidence/full-transactional-filesystem-rollback-2026-05-07.md`에 별도로 기록한다.
