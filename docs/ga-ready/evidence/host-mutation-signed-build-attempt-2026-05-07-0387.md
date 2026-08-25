# 0.38.7 Signed Build and Host Mutation Attempt Evidence

evidence_id: host-mutation-signed-build-attempt-2026-05-07-0387
created_at: 2026-05-07T01:35:00+09:00
signed_rc_artifact_root: artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387
signed_rc_version: 0.38.7-rc.1
source_commit_sha: dd4e7379c515b05eb82038404519c9e63f54bf51
signed_rc_msi_sha256: c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602
signed_rc_signing_mode: RequireSigned
signed_rc_trust_model: InternalEnterprise
signed_rc_authenticode_status: Valid
signtool_verify_exit: 0
host_mutation_attempt_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260507-0387
host_mutation_version: 0.38.7-admin-smoke
host_mutation_status: blocked
host_mutation_performed: false
blocked_error_code: PCV_BATCH_ADMIN_REQUIRED
update_rollback_attempt_artifact_root: artifacts/product-update-rollback-mutation-20260507-0387
update_rollback_status: blocked-or-failed
update_rollback_host_mutation_performed: false
public trusted signing: excluded
external stable publication: not-claimed

## 범위

사용자 승인 범위는 host mutation, MSI, firewall, trust-store, LAN, signed build, updater/rollback mutation이었다.

현재 Codex shell은 non-elevated medium integrity shell이었다. `-AllowHostMutation`은 명시했지만 Batch Supervisor의 `requires_admin=true`/`mutates_host=true` preflight가 elevated shell을 요구해 Service/MSI/Hyper-V step 시작 전에 차단했다. 따라서 이번 `0.38.7-admin-smoke` attempt는 PASS evidence가 아니며 실제 host mutation evidence도 아니다.

## InternalEnterprise RequireSigned build 결과

- Artifact: `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387`
- MSI: `artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387/PureCVisorDesktopNode-0.38.7-rc.1-windows-x64.msi`
- Version: `0.38.7-rc.1`
- MSI SHA-256: `c44128cd84f6f3d93eabb8edb2a41930e4fbe16a081569a62473090d8f68f602`
- Signing mode: `RequireSigned`
- Signing trust model: `InternalEnterprise`
- Source commit: `dd4e7379c515b05eb82038404519c9e63f54bf51`
- Authenticode: `Valid`
- SignTool verify exit: `0`
- Signer thumbprint: `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`
- Payload file count: `7`
- Payload aggregate SHA-256: `ed5ab0b97646a234947a51aa10d60b06d61ba02e9dbdc794ea39a7d299df6067`
- Service host SHA-256: `f9261216e9ad25a73e3fd96169ce1900d5f4d55ea3f264978d4b8a2214100989`
- Product wrapper SHA-256: `1420f8d321ce32778a9648f039c4348cd9ebf4d5b48600c689835f9d5c512179`

## Full Admin Host Mutation attempt 결과

- Artifact: `artifacts/batch-runs/full-admin-host-mutation-gate-20260507-0387`
- Requested version: `0.38.7-admin-smoke`
- Entrypoint: `packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1`
- Allow host mutation flag: `true`
- Result: `ok=false`, `status=blocked`
- Error: `PCV_BATCH_ADMIN_REQUIRED|Batch step requires an elevated shell.|Step 'service-msi-hyperv-admin-smoke'.`
- Blocked step: `service-msi-hyperv-admin-smoke`
- Steps started: `0`
- Host mutation performed: `false`
- Route/MSI/Hyper-V artifact root intended: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260507-0387`
- OS mutation artifact root intended: `artifacts/os-mutation-gates-batch-profile-20260507-0387`
- LAN prefix selected for attempt: `http://[redacted-private-endpoint]:7777/`

이 preflight block 전에 Service/MSI/Hyper-V, firewall, trust-store, LAN, Event Log mutation은 시작되지 않았다.

## Update/Rollback mutation attempt 결과

- Artifact: `artifacts/product-update-rollback-mutation-20260507-0387`
- Update action: `Invoke-PcvDesktopNodeProduct.ps1 -Action Update -SourceRoot artifacts/internal-enterprise-requiresigned-rc-msi-20260507-0387/payload -Version 0.38.7-rc.1`
- Update result: exit `1`, `PCV_PRODUCT_COMMAND_FAILED`, detail `Command 'sc.exe stop PureCVisorDesktopNode' exited with code 5.`
- Update host mutation performed: `false`
- Rollback action: `Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -TimeoutSec 5`
- Rollback result: exit `1`, `PCV_PRODUCT_SERVICE_STOP_TIMEOUT`
- Rollback host mutation performed: `false`
- Post-state product root manifest: `0.38.6-admin-smoke`
- Post-state previous product root: absent
- Final service: `PureCVisorDesktopNode` `Running`

## 판정

`0.38.7-rc.1` InternalEnterprise `RequireSigned` build와 Authenticode/SignTool 검증은 PASS다.

`0.38.7-admin-smoke` full admin host mutation gate와 update/rollback mutation attempt는 non-elevated shell에서 차단됐고 host mutation은 수행하지 않았다. 최신 실제 full admin host mutation PASS evidence는 `0.38.9-admin-smoke`와 `artifacts/batch-runs/full-admin-host-mutation-gate-20260508-202255-0389`다.

이 evidence는 internal trusted signing 및 blocked admin preflight evidence이며 public trusted signing 또는 외부 stable publication evidence가 아니다.
