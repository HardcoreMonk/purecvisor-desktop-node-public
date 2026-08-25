# 0.39.0 MSI/Service 설치 Listener Rerun Evidence

evidence_id: msi-service-installed-listener-rerun-2026-05-08-0390
created_at: 2026-05-08T21:33:30+09:00
batch_supervisor_artifact_root: artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390
routeparity_artifact_root: artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390
admin_smoke_version: 0.39.0-admin-smoke
source_commit_sha: 8d21654045ed75e81344556fa6444f118c62276a
admin_msi_sha256: 4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee
admin_signing_mode: AllowUnsignedDev
diagnostic_bundle_installed_listener_execution: installed-listener-pass
diagnostic_bundle_installed_listener_blocker: none
public trusted signing: excluded
external stable publication: not-claimed
execution_status: pass

## 범위

사용자 관리자 opt-in 범위에서 `0.39.0-admin-smoke` Batch Supervisor `ServiceMsiHyperVAdminSmoke` profile을 실행했다. 이 rerun은 MSI/service installed listener와 installed Hyper-V API route smoke를 확인한 범위이며 firewall, trust-store, LAN listener, Event Log OS mutation gate는 실행하지 않았다.

이 evidence는 2026-05-08 code-level native service-action config fix 이후 실제 installed SCM `PathName`에 diagnostic bundle listener와 timeout/rate-limit hardening 인자가 반영되는지 확인한다.

## Batch Supervisor 결과

- Artifact: `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390`
- Manifest: `artifacts/batch-runs/service-msi-installed-listener-rerun-20260508-212615-0390/manifest.json`
- Summary: `ok=true`, `status=completed`, `total_steps=1`, `executed_steps=1`
- Step: `service-msi-hyperv-admin-smoke`, `exit_code=0`, `timed_out=false`, `retry_count=1`, `attempt_count=1`, `duration_ms=187877`

## Service/MSI 결과

- Artifact: `artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390`
- MSI: `PureCVisorDesktopNode-0.39.0-admin-smoke-windows-x64.msi`
- MSI SHA-256: `4ecc51671b884058330b66b33a13b0d70278825367f7daf48c54ec6f1b3d0bee`
- MSI provenance commit: `8d21654045ed75e81344556fa6444f118c62276a`
- MSI signing mode: `AllowUnsignedDev`
- Payload aggregate SHA-256: `69730ed4c8e69a507bbafa8ea15f94b30597ab43b9abcd1c64614ca0f116e1c2`
- Service host SHA-256: `1b62a44048ad7923a91b12d1828a50243d5ec59851190c4d5eee96c445769c8c`
- Product wrapper SHA-256: `7dbd8cadb81b75044f9afdb14fdc0834e835a9db7bc9e8609d937e69fc948250`
- Steps: build current admin-smoke MSI, service-action smoke, MSI lifecycle smoke, installed .NET host Hyper-V API route smoke all completed with `ok=true`
- Final proof: service `Running`, startup `Auto`, boot time unchanged, `remaining_pcv_vms=[]`

## Installed Listener 결과

`artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/installed-service-listener-post-rerun.json`은 설치된 SCM 상태를 기록한다.

- Product manifest version: `0.39.0-admin-smoke`
- Product manifest schema: `1`
- `helper_script_present=false`
- `api_script_present=false`
- Service `PathName` includes `--diagnostics-root "C:\ProgramData\PureCVisor\desktop-node\diagnostics"`
- Service `PathName` includes `--api-token-protected-file "C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json"`
- Service `PathName` includes `--route-timeout-seconds 30`
- Service `PathName` includes `--request-limit-per-minute 120`
- Service `PathName` includes `--request-burst-limit 20`
- Service `PathName` includes `--retry-after-seconds 15`
- Service prefix remains loopback-only `http://127.0.0.1:7777/`

`artifacts/routeparity-service-msi-installed-listener-rerun-20260508-212615-0390/installed-diagnostic-bundle-listener-smoke.json`은 protected token 기반 실제 listener 왕복을 기록한다.

- `POST http://127.0.0.1:7777/api/v1/diagnostics/bundles`: HTTP `201`
- `GET http://127.0.0.1:7777/api/v1/diagnostics/bundles/{bundle_id}/download`: HTTP `200`
- `content_type=application/vnd.purecvisor.diagnostic-bundle+json`
- `X-PCV-Diagnostic-Bundle-Id` matched the created bundle id
- `archive_status=created`
- `download_status=served-by-download-route`
- `redaction_status=applied`
- `authz_status=token-required-route-contract`
- `actual_execution=code-level-api-action`
- Downloaded artifact copy: `installed-diagnostic-bundle-listener-download.bundle.json`
- Downloaded bundle did not contain the test secret and did contain `[REDACTED]`

## 범위 제외

- Firewall mutation
- Trust-store mutation
- LAN listener mutation
- Event Log source registration/removal
- Public trusted signing
- External stable publication

이 evidence는 internal-only `AllowUnsignedDev` admin-smoke 범위의 installed MSI/service listener evidence다. Public trusted signing 또는 외부 stable publication evidence가 아니다.
