# 0.39.0 OS Mutation Gate Evidence

evidence_id: os-mutation-gate-installed-listener-rerun-2026-05-08-0390
created_at: 2026-05-08T22:09:00+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390
os_mutation_artifact_root: artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390
routeparity_input_artifact_root: artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390
admin_smoke_version: 0.39.0-admin-smoke
lan_prefix: http://[redacted-private-endpoint]:7777/
host_mutation_performed: true
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass

## 범위

사용자 관리자 opt-in 범위에서 `0.39.0-admin-smoke` Batch Supervisor `OsMutationGate` profile을 실행했다. 이 gate는 installed `0.39.0-admin-smoke` listener evidence를 입력으로 삼아 firewall, LAN listener, Event Log source, ADR-0003 internal trust-store mutation을 확인했다.

이 evidence는 host mutation evidence다. Public trusted signing 또는 external stable publication claim은 하지 않는다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390`
- Manifest: `artifacts/batch-runs/os-mutation-gate-installed-listener-rerun-20260508-220816-0390/manifest.json`
- Summary: `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`
- Step: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `retry_count=0`, `attempt_count=1`, `duration_ms=12222`

## OS Gate 결과

`artifacts/os-mutation-gates-installed-listener-rerun-20260508-220816-0390/summary.json` 결과:

- `ok=true`
- `actual_execution=completed`
- `mutates_host=true`
- `host_mutation_performed=true`
- `public_trusted_signing=excluded`
- `external_stable_publication=not-claimed`
- Boot time unchanged: `2026-05-04T22:19:06.5+09:00`
- Final service: `PureCVisorDesktopNode`, `Running`, startup `Auto`
- Final service `PathName` includes diagnostic bundle/hardening args from the `0.39.0-admin-smoke` installed listener rerun
- Final firewall rule count: `0`
- Final Event Log source present: `false`
- Final trust store: Root present `true`, TrustedPublisher present `true`
- Root thumbprint: `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`
- TrustedPublisher thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`

Step evidence:

- `config-migration-apply-service-running`: expected blocked/no-mutation descriptor `PCV_CONFIG_MIGRATION_SERVICE_RUNNING`, mutation planned/performed `false`
- `eventlog-register`: completed
- `eventlog-remove`: completed, final source absent
- `firewall-enable`: completed
- `lan-listener-ip-smoke`: `http://[redacted-private-endpoint]:7777/` runtime policy, `/`, `/index.html`, `/app.js` all HTTP `200`; token redacted
- `firewall-remove`: completed, final rule absent
- `export-existing-internal-trust-certs`: completed
- `trust-store-install-existing`: completed
- `trust-store-remove-existing`: completed
- `trust-store-restore-existing`: completed

## 범위 제외

- Service/MSI/Hyper-V route parity rerun은 이 gate에서 다시 실행하지 않았다. 입력 artifact는 `0.39.0-admin-smoke` MSI/service installed listener rerun이다.
- Public trusted signing은 excluded다.
- External stable publication은 not-claimed다.
- Public stable channel publication, public catalog publication, public signed update/rollback smoke는 실행하지 않았다.

이 evidence는 internal-only `AllowUnsignedDev` admin-smoke host mutation evidence다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
