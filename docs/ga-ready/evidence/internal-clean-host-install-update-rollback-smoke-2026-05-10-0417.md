# 내부 Clean-Host Install/Update/Rollback Smoke - 2026-05-10 0417

```text
evidence_id: internal-clean-host-install-update-rollback-smoke-2026-05-10-0417
artifact_root: artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417
package_artifact_root: artifacts/internal-clean-host-packages-20260510-0414
scope: internal-clean-host-install-update-rollback-smoke
actual_execution: hyper-v-dedicated-clean-host-installed-smoke
host_mutation_performed: true
guest_product_mutation_performed: true
internal_clean_host_install_update_rollback_smoke: pass
baseline_version: 0.39.6-admin-smoke
target_version: 0.39.7-admin-smoke
update_channel: admin-smoke
baseline_msi_sha256: 9b266867129cbf07abb8da7e2a26799d1221a16d955348505416810c48de12b1
target_msi_sha256: 983d1eb64329928b69765a662605d29c3d2aaaa39d1a5857f990e5519438f91a
update_package_sha256: 1807d61f9d953c978cf382b5f447c02ebc6a12fbbecbc54c58c30f472084d40e
provenance_commit: e9ff332ad2a0e33e6d6ae09b80d42fa961849494
signing_mode: RequireSigned
signing_trust_model: InternalEnterprise
internal_root_thumbprint: E49CD75AF53CCF7FA73C97E47443096A4507FB7E
internal_leaf_thumbprint: 8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6
install_exit_code: 0
update_exit_code: 0
rollback_exit_code: 0
baseline_manifest_version: 0.39.6-admin-smoke
updated_manifest_version: 0.39.7-admin-smoke
final_manifest_version: 0.39.6-admin-smoke
final_service_state: Running
final_web_status_code: 200
failed_root_exists_after_rollback: true
failed_root_manifest_version: 0.39.7-admin-smoke
token_value_observed: false
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
winget_submission: out-of-scope
public_release: not-claimed
blocker: none
```

## 결과

내부 clean-host install/update/rollback smoke는 PASS로 승격됐다. 이 실행은 캐시된 Windows Server 2022 evaluation VHD에서 전용 Hyper-V Generation 1 VM을 만들고, `Default Switch`에 연결한 뒤, guest `LocalMachine Root` store에 내부 root certificate를 설치했다. 이후 내부 signed MSI를 Authenticode `Valid`로 검증하고 `0.39.6-admin-smoke`를 설치했으며, 내부 file catalog를 통해 `0.39.7-admin-smoke`로 update한 다음 `0.39.6-admin-smoke`로 rollback했다.

최종 guest service는 `Running`이었고, loopback Web Console/API health는 HTTP `200`을 반환했다. Rollback diagnostics를 위해 `DesktopNode.failed`에는 `0.39.7-admin-smoke` product root가 보존됐다.

## 실행 메모

Clean Server 2022 VHD는 .NET 10 self-contained host를 실행하기 전에 OS cumulative update가 필요했다. Smoke는 `2026-04 Cumulative Update for Microsoft server operating system version 21H2 for x64-based Systems (KB5082142)`를 설치했고, guest UBR은 `169`에서 `5020`으로 올라갔다. Update 이후 일반 PowerShell Direct session이 반환되지 않아 runner가 VM을 강제 restart한 뒤 성공적으로 resume했다.

Guest-local internal catalog는 SHA-256 `1807d61f9d953c978cf382b5f447c02ebc6a12fbbecbc54c58c30f472084d40e`인 `file:///C:/PcvCleanHostSmoke/target-update.zip`을 가리켰다. Install/update/rollback 전에 Event Log Application volume guard 준비도 성공했다.

## 명령 모드

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInternalCleanHostInstallUpdateRollbackSmoke.ps1 -ArtifactRoot artifacts/internal-clean-host-install-update-rollback-smoke-20260510-0417 -BaseVhdPath D:\data\projects\codex-zone\purecvisor-desktop-node\artifacts\image-cache\windows-server-2022-eval-vhd\20348.169.amd64fre.fe_release_svc_refresh.210806-2348_server_serverdatacentereval_en-us.vhd -BaselineMsiPath artifacts/internal-clean-host-packages-20260510-0414/baseline/PureCVisorDesktopNode-0.39.6-admin-smoke-windows-x64.msi -TargetUpdatePackagePath artifacts/internal-clean-host-packages-20260510-0414/target/PureCVisorDesktopNode-0.39.7-admin-smoke-update.zip -InternalRootCertificatePath artifacts/internal-clean-host-packages-20260510-0414/PureCVisorInternalCodeSigningRoot.cer -VmName pcv-cleanhost-0417 -VmSwitchName "Default Switch" -InstallWindowsUpdates -RemoveVmOnSuccess
```

## 경계

이 문서는 내부 사설망 운영 evidence다. Public trusted signing, trusted timestamping, external stable publication, winget submission, public stable installer URL, public clean-host signed install/update/rollback evidence를 주장하지 않는다.
