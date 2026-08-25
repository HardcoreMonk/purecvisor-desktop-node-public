# Manual Admin Operator/Hardening 후속 - 2026-05-10 0.41.5

```text
evidence_id: manual-admin-operator-hardening-followup-2026-05-10-0415
artifact_root: artifacts/manual-admin-followup-20260510-0415
summary: artifacts/manual-admin-followup-20260510-0415/summary.json
status: operator-access-hardening-and-lifecycle-packaging-rebaseline-pass
operator_access_ok: true
internal_service_hardening_ok: true
lifecycle_packaging_ok: true
lifecycle_packaging_rebaseline_evidence: docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md
lifecycle_packaging_rebaseline_artifact_root: artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416
lifecycle_packaging_rebaseline_baseline_version: 0.41.5-admin-smoke
lifecycle_packaging_rebaseline_target_version: 0.41.6-admin-smoke
host_mutation_performed: true
installed_version: 0.41.5-admin-smoke
final_service_state: Running
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

## Operator Access

Installed account login smoke는 `artifacts/manual-admin-followup-20260510-0415/installed-account-login-smoke-rerun`에서 PASS했다.

- `login_status_code=200`
- `session_status_code=200`
- `rbac_status_code=200`
- `console_capabilities_status_code=200`
- `runtime_auth_mode=account_rbac_jwt`
- `restore_status=restored`
- `service_restart_status=restarted-after-restore`
- `token_value_observed=false`

Target-backed noVNC installed streaming smoke는 `artifacts/manual-admin-followup-20260510-0415/target-backed-novnc-installed-streaming-smoke-rerun`에서 PASS했다.

- WebSocket path `/api/v1/console/novnc/{vm_id}` echoed the target-backed binary frame.
- `target_frame_sha256=c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106`
- `echoed_frame_sha256=c9b287180324ac478bd231ed9e6405e5b968f81bb266741ccc30c03d8dc98106`
- `path_name_restored=true`
- final service `Running`
- `token_value_observed=false`

## 내부 Service Hardening

Service token rotation/revoke는 `artifacts/manual-admin-followup-20260510-0415/service-token-rotation-revoke`에서 PASS했다.

- `service_token_mutation=performed`
- `backup_write_status=written`
- `atomic_replace_status=completed`
- `service_reload_status=restarted`
- `old_token_rejection_status=old-token-rejected-after-reload`
- `token_rotation_audit_status=written`
- `token_value_observed=false`

Installed service가 Windows Credential Manager token target을 사용하므로 protected token file rotation 이후 Credential Manager default transition을 다시 실행했다. 이로써 회전된 protected token을 `PureCVisor/PureCVisorDesktopNode/api-token`에 다시 동기화했다.

Windows Credential Manager default transition은 `artifacts/manual-admin-followup-20260510-0415/windows-credential-manager-default-transition-installed`에서 PASS했다.

- Version `0.41.5-admin-smoke`
- MSI SHA-256 `6684061dd248ff2a9567bc251bf45b73ba1ef8174ed92e3f6cd24b2de3dfa615`
- Provenance commit `484ed04a28fbb8dd07f513463a2a5bf77ecfa61e`
- `system_proof_status=system-context-proof-pass`
- `token_source_migration=protected-file-to-credential-manager`
- `service_reload_status=restarted`
- `old_source_rejection_status=protected-file-source-rejected-after-reload`
- `rollback_diagnostics_status=written`
- runtime policy health `200`, `token_storage=windows-credential-manager`

Internal HTTPS/TLS lifecycle installed smoke는 `artifacts/manual-admin-followup-20260510-0415/internal-https-tls-lifecycle-installed`에서 PASS했다.

- `certificate_lifecycle=generate-bind-rotate-remove-pass`
- initial HTTPS runtime policy `200`
- rotated HTTPS runtime policy `200`
- final restored HTTP runtime policy `200`
- SSL binding removed and temporary certificates removed
- final service `Running`, `path_name_restored=true`

Windows Event Log default transition은 `artifacts/manual-admin-followup-20260510-0415/windows-event-log-default-transition-installed`에서 PASS했다.

- Version `0.41.5-admin-smoke`
- MSI SHA-256 `b191c45c66a57f987e262d491eeb6d22ea7af5745c93c120d02e41f18592e4ab`
- Provenance commit `484ed04a28fbb8dd07f513463a2a5bf77ecfa61e`
- `default_writer_status=default-writer-pass`
- `provider_repair_status=provider-repair-pass`
- `event_write_status=write-query-pass`
- `volume_guard_status=volume-guard-pass`
- `provider_remove_status=provider-remove-pass`
- `final_provider_status=provider-present`
- runtime policy health `200`

## Lifecycle / Packaging

Lifecycle/Packaging current rebaseline은 `docs/ga-ready/evidence/lifecycle-packaging-rebaseline-2026-05-10-0415-0416.md` / `artifacts/lifecycle-packaging-rebaseline-20260510-0415-0416`에서 PASS했다.

- Current package pair: `0.41.5-admin-smoke` baseline to `0.41.6-admin-smoke` target.
- Baseline MSI SHA-256 `add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6`.
- Target MSI SHA-256 `967ac29bf2928f1fec3a0bb72425d15d2eda65a2466b1cb29dd9183bb18928a3`.
- Update ZIP SHA-256 `4e54c19ca6e6a9beec506613d66220c8b0bbbb579d0926d1d840f2cde7592161`.
- Installed product update/rollback passed: update `0.41.5-admin-smoke` to `0.41.6-admin-smoke`, rollback to `0.41.5-admin-smoke`, final service `Running`, Web Console `200`, protected API unauthenticated boundary `401`, failed root preserved with `0.41.6-admin-smoke`.
- Internal clean-host install/update/rollback current rebaseline passed on a Windows-updated dedicated Hyper-V guest: final manifest `0.41.5-admin-smoke`, final Web Console `200`, final API unauthenticated boundary `401`, failed root preserved with `0.41.6-admin-smoke`, VM removed after evidence.
- The clean-host runner Web Console health check was updated for the Web/API split: Web Console is `http://127.0.0.1/`; API remains protected at `http://127.0.0.1:7777/`.
- Burn bootstrapper and MSIX lifecycle were regenerated for the current package pair after this slice: `docs/ga-ready/evidence/burn-bootstrapper-lifecycle-smoke-2026-05-10-0416.md` and `docs/ga-ready/evidence/msix-package-lifecycle-smoke-2026-05-10-0416.md`.

## 최종 Host 상태

최종 installed service는 `PureCVisorDesktopNode` `Running`, `StartMode=Auto`였고 installed manifest는 `0.41.5-admin-smoke`였다. 최종 service path는 Web `http://127.0.0.1:80/`, API `http://127.0.0.1:7777/`, Windows Event Log writer/provider arguments, Credential Manager token target, account/JWT files, diagnostics root, route timeout, request limit, burst limit, retry-after arguments를 유지했다.

## 경계

이 문서는 내부 administrator opt-in host mutation evidence다. Public trusted signing, trusted timestamping, winget submission, external stable publication, public catalog upload, public stable installer URL, public clean-host signed install/update/rollback evidence가 아니다.
