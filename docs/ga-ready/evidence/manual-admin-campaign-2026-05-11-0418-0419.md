# MANUAL-ADMIN 1-2-3-4 캠페인 증거 - 2026-05-11 0.41.8 to 0.41.9

```text
evidence_id: manual-admin-campaign-2026-05-11-0418-0419
scope: manual-admin-groups-1-2-3-4
result: PASS_WITH_DOCUMENTED_OPERATOR_NOTES
artifact_root: artifacts/manual-admin-campaign-20260511-0418-0419
summary: artifacts/manual-admin-campaign-20260511-0418-0419/summary.json
baseline_version: 0.41.8-admin-smoke
target_version: 0.41.9-admin-smoke
host_mutation_performed: true
final_service_state: Running
final_manifest_version: 0.41.8-admin-smoke
web_loopback_status: 200
api_unauthenticated_status: 401
public_trusted_signing: out-of-scope
external_stable_publication: out-of-scope
```

## 판정

사용자 승인 범위인 MANUAL-ADMIN 1-2-3-4 묶음은 PASS로 판정한다.

판정 근거는 개별 runner summary, OS/AppX/Event Log evidence, 최종 service 상태 확인이다. 최종 설치본은 `PureCVisorDesktopNode` `Running`, `StartMode=Auto`, product manifest `0.41.8-admin-smoke`이며 service `PathName`은 `--api-token-credential-target`와 `--max-request-body-bytes 1048576`을 유지했다. Web listener `http://127.0.0.1/`는 HTTP `200`, API unauthenticated boundary는 HTTP `401`이었다.

## 1. Baseline Host Gate 기준선 확인

전체 관리자 host mutation gate는 PASS다.

- Batch summary: `artifacts/batch-runs/full-admin-host-mutation-gate-20260511-120516-0418/summary.json`
- Route parity/MSI/Hyper-V artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260511-120516-0418`
- OS mutation gate artifact: `artifacts/os-mutation-gates-batch-profile-20260511-120516-0418`
- Batch 결과: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`
- Service/MSI/Hyper-V step: `ok=true`, `exit_code=0`
- OS mutation gate step: `ok=true`, `exit_code=0`
- Baseline MSI SHA-256: `4a324b67d9f376825d8b0a15d0f01c54cf334f084042bafd758bb888f77d344f`

## 2. Operator Access 운영자 접근

설치본 account login과 target-backed noVNC installed streaming은 PASS다.

- Account login artifact: `artifacts/installed-account-login-smoke-20260511-120914-0418`
- Login/session/RBAC/console status: 모두 HTTP `200`
- Runtime auth mode: `account_rbac_jwt`
- Restore status: `restored`
- Service restart status: `restarted-after-restore`
- noVNC artifact: `artifacts/target-backed-novnc-installed-streaming-smoke-20260511-120926-0418`
- noVNC target: `127.0.0.1:8004`
- Target frame SHA-256와 echoed frame SHA-256: `c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106`
- `path_name_restored=true`, final service `Running`
- Token/password value exposure: `false`

## 3. Internal Service Hardening 내부 서비스 강화

Internal HTTPS/TLS lifecycle, Credential Manager default transition, Event Log default transition, service token rotation/revoke는 PASS다.

- TLS lifecycle artifact: `artifacts/internal-https-tls-lifecycle-installed-20260511-121039-0418`
- TLS 결과: `generate-bind-rotate-remove-pass`, initial HTTPS `200`, rotated HTTPS `200`, restored HTTP `200`
- Credential Manager artifact: `artifacts/windows-credential-manager-default-transition-installed-20260511-121055-0418`
- Credential Manager 결과: `system-context-proof-pass`, `protected-file-to-credential-manager`, `service_reload_status=restarted`, runtime policy `200`, `token_storage=windows-credential-manager`
- Event Log artifact: `artifacts/windows-event-log-default-transition-installed-20260511-121143-0418`
- Event Log 결과: `default-writer-pass`, `provider-repair-pass`, `write-query-pass`, `volume-guard-pass`, `provider-remove-pass`, final provider `present`
- Service token rotation canonical artifact: `artifacts/service-token-rotation-revoke-installed-20260511-121324-0418`
- Token rotation 결과: `ServiceTokenMutation=performed`, `ServiceReloadStatus=restarted`, `OldTokenRejectionStatus=old-token-rejected-after-reload`, token hash changed `true`
- Post-token-rotation Credential Manager resync: `artifacts/windows-credential-manager-default-transition-installed-20260511-121352-0418-post-token-rotation`

주의사항: 첫 service token rotation attempt `artifacts/service-token-rotation-revoke-installed-20260511-121253-0418`는 Start-Process argument quoting 문제로 invalid 처리했다. 이 attempt는 최종 PASS 근거로 사용하지 않는다.

## 4. Lifecycle / Packaging 생명주기와 패키징

Lifecycle/Packaging 묶음은 PASS다.

- Product update/rollback artifact: `artifacts/lifecycle-packaging-rebaseline-20260511-121629-0418-0419/product-update-rollback/summary.json`
- Update/rollback 결과: `0.41.8-admin-smoke -> 0.41.9-admin-smoke -> 0.41.8-admin-smoke`, update exit `0`, rollback exit `0`, runtime policy `200`
- Target MSI SHA-256: `4de39ee2d94d5db2ad6266540612d157dcd07ffef60f6cd78ab1ad0cbedf43ef`
- Update ZIP SHA-256: `23b0d574fb8a28fb64771a37663c23f3096c7b78682ea193cb137f424a98794c`
- Burn canonical artifact: `artifacts/burn-bootstrapper-lifecycle-20260511-0419-restore-0418-retry`
- Burn 결과: bundle build/install/repair/remove PASS, baseline MSI restore PASS, final manifest `0.41.8-admin-smoke`
- MSIX artifact: `artifacts/msix-package-lifecycle-smoke-20260511-0418-0419`
- MSIX 결과: build/sign/verify/install `0.41.8.0`, update `0.41.9.0`, remove PASS, final package absent `true`, final smoke service absent `true`
- Internal clean-host artifact: `artifacts/internal-clean-host-install-update-rollback-smoke-20260511-0418-0419`
- Clean-host 결과: dedicated Hyper-V guest, Windows Update UBR `169 -> 5020`, install exit `0`, update exit `0`, rollback exit `0`, final manifest `0.41.8-admin-smoke`, final Web Console `200`, failed root manifest `0.41.9-admin-smoke`
- MSI/update package apply composed artifact: `artifacts/msi-update-package-apply-20260511-0418-0419`
- MSI/update composed 결과: baseline MSI lifecycle install/repair/final restore exit `0`, update package apply exit `0`, rollback exit `0`

주의사항:

- Burn first attempt `artifacts/burn-bootstrapper-lifecycle-20260511-0419-restore-0418`는 bundle UI가 대기 상태로 남아 invalid 처리했다. Canonical retry만 PASS 근거로 사용한다.
- MSIX runner process는 AppX lifecycle 완료 후 summary 저장 전 shell timeout에 걸렸다. `summary.json`은 `Microsoft-Windows-AppXDeploymentServer/Operational` 이벤트, `makeappx`, `signtool`, final package/service absence check로 재구성했다.
- Clean-host Windows Update reboot 이후 외부 PowerShell Direct probe가 응답하지 않아 dedicated VM에 `Restart-VM -Force`를 1회 수행했다. 이 사실은 `artifacts/internal-clean-host-install-update-rollback-smoke-20260511-0418-0419/operator-note.json`에 기록했다. Runner는 이후 재접속해 PASS로 완료했다.

## 경계

이 evidence는 내부 관리자 opt-in host mutation evidence다. Public trusted signing, trusted timestamping, winget submission, external stable publication, public catalog upload, public stable installer URL, public clean-host signed install/update/rollback evidence를 주장하지 않는다.
