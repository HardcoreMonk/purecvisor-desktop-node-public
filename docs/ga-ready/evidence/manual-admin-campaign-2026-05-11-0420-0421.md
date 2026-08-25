# MANUAL-ADMIN 1-2-3-4 캠페인 증거 - 2026-05-11 0.42.0 to 0.42.1

```text
evidence_id: manual-admin-campaign-2026-05-11-0420-0421
scope: manual-admin-groups-1-2-3-4
result: PASS_WITH_DOCUMENTED_OPERATOR_NOTES
artifact_root: artifacts/manual-admin-campaign-20260511-0420-0421
summary: artifacts/manual-admin-campaign-20260511-0420-0421/summary.json
baseline_version: 0.42.0-admin-smoke
target_version: 0.42.1-admin-smoke
host_mutation_performed: true
final_service_state: Running
final_manifest_version: 0.42.0-admin-smoke
web_loopback_status: 200
api_unauthenticated_status: 401
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
```

## 판정

사용자 승인 범위인 MANUAL-ADMIN 1-2-3-4 묶음은 PASS로 판정한다.

판정 근거는 각 runner의 `summary.json`, 설치 서비스 최종 상태, Web/API listener 상태, 그리고 dedicated clean-host guest evidence다. 최종 설치본은 `PureCVisorDesktopNode` `Running`, `StartMode=Auto`, product manifest `0.42.0-admin-smoke`이며 service `PathName`은 `--api-token-credential-target`와 `--max-request-body-bytes 1048576`을 유지했다. Web listener `http://127.0.0.1/`는 HTTP `200`, unauthenticated API boundary는 HTTP `401`이었다.

## 1. Baseline Host Gate 기준선 확인

전체 관리자 host mutation gate는 PASS다.

- Batch summary: `artifacts/batch-runs/full-admin-host-mutation-gate-20260511-145303-0420/summary.json`
- Route parity/MSI/Hyper-V artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260511-145303-0420`
- OS mutation gate artifact: `artifacts/os-mutation-gates-batch-profile-20260511-145303-0420`
- Batch 결과: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Service/MSI/Hyper-V step: `ok=true`, `exit_code=0`
- OS mutation gate step: `ok=true`, `exit_code=0`
- Baseline MSI SHA-256: `17ddc83eefd78cbdc312f5e3fd9414072e6913522341293c6140d206a6548cea`
- LAN listener prefix: `http://[redacted-private-endpoint]:7777/`

## 2. Operator Access 운영자 접근

설치본 account login과 target-backed noVNC installed streaming은 PASS다.

- Account login artifact: `artifacts/installed-account-login-smoke-20260511-145303-0420`
- Login/session/RBAC/console status: 모두 HTTP `200`
- Runtime auth mode: `account_rbac_jwt`
- Restore status: `restored`
- Service restart status: `restarted-after-restore`
- noVNC artifact: `artifacts/target-backed-novnc-installed-streaming-smoke-20260511-145303-0420`
- Target frame SHA-256와 echoed frame SHA-256: `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106`
- `path_name_restored=true`, final service `Running`
- Token/password value exposure: `false`

## 3. Internal Service Hardening 내부 서비스 강화

Internal HTTPS/TLS lifecycle, Credential Manager default transition, Event Log default transition, service token rotation/revoke는 PASS다.

- TLS lifecycle artifact: `artifacts/internal-https-tls-lifecycle-installed-20260511-145303-0420`
- TLS 결과: `generate-bind-rotate-remove-pass`, initial HTTPS `200`, rotated HTTPS `200`, restored HTTP `200`
- Credential Manager artifact: `artifacts/windows-credential-manager-default-transition-installed-20260511-145303-0420`
- Credential Manager 결과: `system-context-proof-pass`, `protected-file-to-credential-manager`, `service_reload_status=restarted`, runtime policy `200`, `token_storage=windows-credential-manager`
- Event Log artifact: `artifacts/windows-event-log-default-transition-installed-20260511-145303-0420`
- Event Log 결과: `default-writer-pass`, `provider-repair-pass`, `write-query-pass`, `volume-guard-pass`, `provider-remove-pass`, final provider `present`
- Service token rotation canonical artifact: `artifacts/service-token-rotation-revoke-installed-20260511-145303-0420-retry`
- Token rotation 결과: `ServiceTokenMutation=performed`, `ServiceReloadStatus=restarted`, `OldTokenRejectionStatus=old-token-rejected-after-reload`, token hash changed `true`
- Protected file token runtime check after rotation: HTTP `403`, expected because installed runtime was on Credential Manager target
- Post-token-rotation Credential Manager resync: `artifacts/windows-credential-manager-default-transition-installed-20260511-145303-0420-post-token-rotation`

주의사항: 첫 service token rotation attempt `artifacts/service-token-rotation-revoke-installed-20260511-145303-0420`는 `--product-root`와 `--service-exe` 인자 누락으로 invalid 처리했다. 이 attempt는 최종 PASS 근거로 사용하지 않는다.

## 4. Lifecycle / Packaging 생명주기와 패키징

Lifecycle/Packaging 묶음은 PASS다.

- Product update/rollback artifact: `artifacts/lifecycle-packaging-rebaseline-20260511-145303-0420-0421/product-update-rollback/summary.json`
- Update/rollback 결과: `0.42.0-admin-smoke -> 0.42.1-admin-smoke -> 0.42.0-admin-smoke`, update exit `0`, rollback exit `0`, runtime policy `200`
- Target MSI SHA-256: `043b616c511538acf173d591cc1b654b50f06db3a61b447ef53594240d8be0c0`
- Update ZIP SHA-256: `57ce18d4489ec5d2bf9cb3bde7937e443c48f72f99d5aa755b42e77704eeb201`
- Burn artifact: `artifacts/burn-bootstrapper-lifecycle-20260511-0421-restore-0420`
- Burn 결과: bundle build/install/repair/remove PASS, baseline MSI restore PASS, final manifest `0.42.0-admin-smoke`
- Burn bundle SHA-256: `574ec1b5017e0ccc9d89d6a1848b5deb826fb59069219bf88bbd8d6b39b4ecb4`
- MSIX artifact: `artifacts/msix-package-lifecycle-smoke-20260511-0420-0421`
- MSIX 결과: build/sign/verify/install `0.42.0.0`, update `0.42.1.0`, remove PASS, final package absent `true`, final smoke service absent `true`
- Internal clean-host artifact: `artifacts/internal-clean-host-install-update-rollback-smoke-20260511-0420-0421`
- Clean-host 결과: dedicated Hyper-V guest, Windows Update UBR `169 -> 5020`, install exit `0`, update exit `0`, rollback exit `0`, final manifest `0.42.0-admin-smoke`, final Web Console `200`, blocker `none`
- MSI/update package apply composed artifact: `artifacts/msi-update-package-apply-20260511-0420-0421`
- MSI/update composed 결과: baseline MSI lifecycle install/repair/final restore exit `0`, update package apply exit `0`, rollback exit `0`

주의사항:

- WiX `WixToolset.Bal.wixext` cache가 `damaged`로 확인되어 Burn build는 `WixToolset.BootstrapperApplications.wixext/5.0.2`로 수행했다. 최종 bundle build/install/repair/remove 결과는 PASS다.
- MSIX runner는 이번 run에서 timeout 없이 정상 종료했다.
- Clean-host Windows Update reboot는 runner 내부 재접속으로 완료됐고, 별도 수동 `Restart-VM -Force` 개입은 없었다.

## 경계

이 evidence는 내부 관리자 opt-in host mutation evidence다. Public trusted signing, trusted timestamping, winget submission, external stable publication, public catalog upload, public stable installer URL, public clean-host signed install/update/rollback evidence를 주장하지 않는다.
