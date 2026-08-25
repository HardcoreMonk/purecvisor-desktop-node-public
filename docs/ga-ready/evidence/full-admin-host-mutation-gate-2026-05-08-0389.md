# 0.38.9 Full Admin Host Mutation Gate Evidence

evidence_id: full-admin-host-mutation-gate-2026-05-08-0389
created_at: 2026-05-08T20:27:00+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260508-202255-0389
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260508-202255-0389
admin_smoke_version: 0.38.9-admin-smoke
source_commit_sha: 159fa7ac8e1b8f9a6c144d44b0cefef6a26ac0ce
admin_msi_sha256: 86fbd831ae58251d4ff8b44471a794122a9f2c4c4faa451376a267dfc34572e3
admin_signing_mode: AllowUnsignedDev
public trusted signing: excluded
external stable publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass

## 범위

사용자 관리자 opt-in 범위에서 `0.38.9-admin-smoke` Batch Supervisor full admin host mutation gate를 실행했다.

Gate surface는 installed service listener, Service/MSI/Hyper-V route parity, firewall, LAN, Event Log, trust-store다.

이 문서의 installed service listener는 runtime policy/Web asset/LAN listener sanity를 뜻한다. 0.38.9 final SCM `PathName`에는 당시 `--diagnostics-root`, route timeout, request limit, burst, retry-after 인자가 포함되지 않았으므로 ADR-0005 diagnostic bundle installed listener PASS로 해석하지 않는다. Diagnostic bundle native service-action config gap은 `docs/ga-ready/evidence/diagnostic-bundle-native-service-action-config-code-level-2026-05-08.md`에서 code-level로 보강했고, 후속 `docs/ga-ready/evidence/msi-service-installed-listener-rerun-2026-05-08-0390.md`의 `0.39.0-admin-smoke` elevated MSI/service rerun에서 `diagnostic_bundle_installed_listener_execution: installed-listener-pass`로 닫았다.

이 evidence는 내부 전용 서비스의 `AllowUnsignedDev` admin-smoke 범위다. Public trusted signing, public stable channel, 외부 stable publication claim은 제외한다. Public `.cer`/thumbprint와 provenance만 evidence에 기록하며 private key, PFX, password, bearer token, protected token blob은 기록하지 않는다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Next resume step: `null`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `retry_count=1`, `attempt_count=1`, `final_attempt=1`, `duration_ms=194045`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `retry_count=0`, `attempt_count=1`, `final_attempt=1`, `duration_ms=11074`

## Service/MSI/Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260508-202255-0389`
- MSI: `PureCVisorDesktopNode-0.38.9-admin-smoke-windows-x64.msi`
- MSI SHA-256: `86fbd831ae58251d4ff8b44471a794122a9f2c4c4faa451376a267dfc34572e3`
- MSI provenance commit: `159fa7ac8e1b8f9a6c144d44b0cefef6a26ac0ce`
- MSI signing mode: `AllowUnsignedDev`
- Payload aggregate SHA-256: `7670bc20c6a4979f1d5fc37d922ac9c213f1ed07093a28a7e000e76b1a8d7c0b`
- Service host source SHA-256: `c9fbbb1f310acbac882f11b9b6f67ed0711f32f3783ae021d404d49aaa5c7a5c`
- Product wrapper SHA-256: `7dbd8cadb81b75044f9afdb14fdc0834e835a9db7bc9e8609d937e69fc948250`
- MSI lifecycle: install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, final-restore-install all exit `0`
- Service-action smoke: pass
- Installed Hyper-V API route smoke: pass
- VM lifecycle routes: create, start, restart, poweroff, delete pass
- Checkpoint routes: create, restore, delete pass
- Final proof: service `Running`, startup `Auto`, boot time unchanged, `remaining_pcv_vms=[]`

## Firewall, LAN, Event Log, Trust Store 결과

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260508-202255-0389`
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

`0.38.4-admin-smoke` 이후 같은 full admin host mutation gate를 `0.38.9-admin-smoke` MSI로 재실행했다. `0.38.7-rc.1` internal enterprise `RequireSigned` build evidence는 최신 signed build 기준으로 별도 보존한다.

## 판정

`0.38.9-admin-smoke` full admin host mutation gate는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
