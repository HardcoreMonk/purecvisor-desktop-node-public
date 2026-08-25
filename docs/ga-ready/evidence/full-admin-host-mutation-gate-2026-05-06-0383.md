# 0.38.3 Full Admin Host Mutation Gate and Internal Signed Build Evidence

evidence_id: full-admin-host-mutation-gate-2026-05-06-0383
created_at: 2026-05-06T20:43:01+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260506-203422-0383
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-203422-0383
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260506-203422-0383
admin_smoke_version: 0.38.3-admin-smoke
signed_rc_artifact_root: artifacts/internal-enterprise-requiresigned-rc-msi-20260506-203320-0383
signed_rc_version: 0.38.3-rc.1
source_commit_sha: 4d60c0a1a0e49cfd18876ece177af0c19758f75f
admin_msi_sha256: d58142d94c5c5b876c4f4cba30f387046ea11106d6d204e32fe1aca0138598fc
signed_rc_msi_sha256: c25e12eff824343714e2ab6495044f84f3d9a0591115827bedcc13629a9636cb
admin_signing_mode: AllowUnsignedDev
signed_rc_signing_mode: RequireSigned
signed_rc_trust_model: InternalEnterprise
public trusted signing: excluded
external stable publication: not-claimed
execution_status: pass
no_auto_reboot_status: pass
rollback_final_state_status: pass

## 범위

사용자 관리자 opt-in 범위에서 `0.38.3-rc.1` internal enterprise `RequireSigned` MSI build와 `0.38.3-admin-smoke` Batch Supervisor full admin host mutation gate를 실행했다.

Gate surface는 Service/MSI/Hyper-V, firewall, LAN, Event Log, trust-store다.

이 evidence는 내부 전용 서비스의 `AllowUnsignedDev` admin-smoke와 ADR-0003 internal Root/leaf `RequireSigned` signing trust model 범위다. Public trusted signing, public stable channel, 외부 stable publication claim은 제외한다. Public `.cer`/thumbprint와 provenance만 evidence에 기록하며 private key, PFX, password, bearer token, protected token blob은 기록하지 않는다.

## InternalEnterprise RequireSigned build 결과

- Artifact: `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-203320-0383`
- MSI: `artifacts/internal-enterprise-requiresigned-rc-msi-20260506-203320-0383/PureCVisorDesktopNode-0.38.3-rc.1-windows-x64.msi`
- Version: `0.38.3-rc.1`
- MSI SHA-256: `c25e12eff824343714e2ab6495044f84f3d9a0591115827bedcc13629a9636cb`
- Signing mode: `RequireSigned`
- Signing trust model: `InternalEnterprise`
- Source commit: `4d60c0a1a0e49cfd18876ece177af0c19758f75f`
- Authenticode: `Valid`
- SignTool verify exit: `0`
- Signer thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
- Provenance payload file count: `7`
- Service host SHA-256: `851071388aa1bba595478fe140db049f12696e10215556001479c524f3e023e3`
- Payload aggregate SHA-256: `287349e90a1af2183e41464a04a72e647354f34acede2d0288124cc76a571200`
- Product wrapper SHA-256: `c0be4d51fb42ab3452d9aa84120a120a0c8ba83f5b8a1a17b2ac28b9cb0deabe`

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260506-203422-0383`
- Summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Failed step: `null`
- Next resume step: `null`
- Step 1: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `retry_count=1`, `attempt_count=1`, `final_attempt=1`, `duration_ms=144886`
- Step 2: `os-mutation-gate`, `exit_code=0`, `timed_out=false`, `retry_count=0`, `attempt_count=1`, `final_attempt=1`, `duration_ms=11077`

## Service/MSI/Hyper-V 결과

- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260506-203422-0383`
- MSI: `PureCVisorDesktopNode-0.38.3-admin-smoke-windows-x64.msi`
- MSI SHA-256: `d58142d94c5c5b876c4f4cba30f387046ea11106d6d204e32fe1aca0138598fc`
- MSI provenance commit: `4d60c0a1a0e49cfd18876ece177af0c19758f75f`
- MSI signing mode: `AllowUnsignedDev`
- Payload aggregate SHA-256: `91ff03c93dca45c5da9eb160eccfa65dbe8bc5a2bf9c4a6fc935af20368fcb1f`
- Service host source SHA-256: `851071388aa1bba595478fe140db049f12696e10215556001479c524f3e023e3`
- MSI lifecycle: install, repair, uninstall-preserve, install-remove-data, uninstall-remove-data, final-restore-install all exit `0`
- MSI lifecycle reboot controls: `REBOOT=ReallySuppress`, `MSIRESTARTMANAGERCONTROL=Disable`, `/qn`, `/norestart`
- Service-action smoke: pass
- Installed Hyper-V API route smoke: pass
- VM lifecycle routes: create, start, restart, poweroff, delete pass
- Checkpoint routes: create, restore, delete pass
- Hyper-V smoke VM: `pcv-spike-api-79083f24`
- Unmanaged guard VM: `pcv-spike-api-foreign-4ab874ba`
- Final proof: service `Running`, startup `Automatic`, boot time unchanged, `remaining_pcv_vms=[]`

## Firewall, LAN, Event Log, Trust Store 결과

- Artifact: `artifacts/os-mutation-gates-batch-profile-20260506-203422-0383`
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

## 이전 0.38.2 대비

`0.38.2-admin-smoke` 이후 같은 full admin host mutation gate를 `0.38.3-admin-smoke` MSI로 재실행했고, 별도 `0.38.3-rc.1` internal enterprise `RequireSigned` MSI build와 Authenticode/SignTool 검증도 수행했다. Batch Supervisor retry contract는 route parity step에 `retry_count=1`을 유지했지만 실제 실행은 `attempt_count=1`로 완료했고 timeout은 발생하지 않았다.

## 판정

`0.38.3-admin-smoke` full admin host mutation gate와 `0.38.3-rc.1` InternalEnterprise `RequireSigned` build는 pass다. 이 pass는 내부 전용 서비스의 관리자 opt-in evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
