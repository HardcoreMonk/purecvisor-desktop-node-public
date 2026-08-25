# 0.39.1 Full Admin Host Mutation Gate Evidence

evidence_id: full-admin-host-mutation-gate-2026-05-09-0391
created_at: 2026-05-09T01:05:00+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260509-010131-0391
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-010131-0391
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260509-010131-0391
admin_smoke_version: 0.39.1-admin-smoke
source_commit_sha: 0815a6281bcb98b5b1795e8d054073e1c9fb4892
admin_msi_sha256: 19b93e72f567e1d5598c7998da2385edde574732284c3ff82a1a5954857f915d
admin_signing_mode: AllowUnsignedDev
public_trusted_signing: excluded
external_stable_publication: not-claimed
execution_status: pass
host_mutation_performed: true
no_auto_reboot_status: pass
rollback_final_state_status: pass

## 범위

사용자 관리자 opt-in 범위에서 `0.39.1-admin-smoke` Batch Supervisor full admin host mutation gate를 실행했다.

Gate surface는 Service/MSI/Hyper-V route parity, firewall enable/remove, LAN listener IP smoke, Event Log source register/remove, ADR-0003 internal Root/TrustedPublisher trust-store install/remove/restore다.

이 evidence는 내부 전용 서비스의 `AllowUnsignedDev` admin-smoke 범위다. Public trusted signing, public stable channel, 외부 stable publication claim은 제외한다. Public `.cer`/thumbprint와 provenance만 evidence에 기록하며 private key, PFX, password, bearer token, protected token blob은 기록하지 않는다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-010131-0391`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Next resume step: `null`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `retry_count=1`, `attempt_count=1`, `final_attempt=1`, `duration_ms=72934`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `retry_count=0`, `attempt_count=1`, `final_attempt=1`, `duration_ms=11100`

## Service/MSI/Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260509-010131-0391`
- MSI: `PureCVisorDesktopNode-0.39.1-admin-smoke-windows-x64.msi`
- MSI SHA-256: `19b93e72f567e1d5598c7998da2385edde574732284c3ff82a1a5954857f915d`
- MSI provenance commit: `0815a6281bcb98b5b1795e8d054073e1c9fb4892`
- MSI signing mode: `AllowUnsignedDev`
- Payload aggregate SHA-256: `acc1d4bf07a9da4e44a690fc0d632db557a676c71541d8cd5eb5eb631de9d7f2`
- Service host source SHA-256: `5db6f6a38db5feedd58205555fbe77a616097e3b83e25fcf132cdc45dbbdaa5f`
- Product wrapper SHA-256: `7dbd8cadb81b75044f9afdb14fdc0834e835a9db7bc9e8609d937e69fc948250`
- MSI lifecycle: install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, final-restore-install all exit `0`
- Service-action smoke: pass
- Installed Hyper-V API route smoke: pass
- VM lifecycle routes: create, start, restart, poweroff, delete pass
- Checkpoint routes: create, restore, delete pass
- Final proof: service `Running`, startup `Auto`, boot time unchanged, `remaining_pcv_vms=[]`

## Firewall, LAN, Event Log, Trust Store 결과

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260509-010131-0391`
- OS mutation summary: `ok=true`, `plan_only=false`, `actual_execution=completed`, `host_mutation_performed=true`
- LAN: `http://[redacted-private-endpoint]:7777/`
- LAN smoke: `/api/v1/runtime/policy`, `/`, `/index.html`, `/app.js` all HTTP `200`
- LAN token handling: `token_redacted=true`, `token_length=43`
- Firewall: owned rule enable/remove pass, final rule count `0`
- Event Log: register/remove pass, final source absent
- Trust store: ADR-0003 internal Root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E` and TrustedPublisher leaf `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6` install/remove/restore pass
- Final trust store: Root present `true`, TrustedPublisher present `true`
- Final service: `PureCVisorDesktopNode` `Running`, loopback-only `http://127.0.0.1:7777/`
- Boot time unchanged: before/after `2026-05-04T22:19:06.5+09:00`

## 이전 evidence 대비

`0.38.9-admin-smoke` full admin host mutation gate는 historical PASS evidence로 보존한다. 최신 actual full admin host mutation PASS evidence는 `0.39.1-admin-smoke`의 `artifacts/batch-runs/full-admin-host-mutation-gate-20260509-010131-0391`이다.

별도 `docs/ga-ready/evidence/msi-update-package-apply-2026-05-09-0391.md`는 `artifacts/msi-update-package-20260509-0391`에서 MSI/update package apply만 확인한 evidence다. 이번 full gate는 같은 display version `0.39.1-admin-smoke`를 새 Batch Supervisor route artifact에서 빌드해 Service/MSI/Hyper-V/firewall/LAN/Event Log/internal trust-store mutation까지 실행했다.

## 판정

`0.39.1-admin-smoke` full admin host mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
