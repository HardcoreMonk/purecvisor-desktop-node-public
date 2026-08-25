# Full Admin Host Mutation Gate - 2026-05-10 0.41.0 Account Rerun

evidence_id: full-admin-host-mutation-gate-2026-05-10-0410-account-rerun
status: pass
actual_execution: batch-supervisor-full-admin-host-mutation-gate
host_mutation_performed: true
version: 0.41.0-admin-smoke
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-154831-0410-account-rerun
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260510-154831-0410-account-rerun
installed_account_login_smoke_artifact_root: artifacts/installed-account-login-smoke-20260510-0410-final
msi_sha256: cabe7d8a203dab641f0fcd4f2da5ceacb3541e6f9cd9fa6604bcc827e784454d
provenance_commit: a3226ef637ea895d2f2a9956599e0d5e79d00410
signing_mode: AllowUnsignedDev
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Scope

This evidence records the installed account login follow-up rerun after Web/API port split. The first `0.41.0-admin-smoke` full gate attempt exposed an obsolete route-parity health probe that still checked Web root on API port `7777`. The runner now checks Web Console on `http://127.0.0.1/` and Web API on `http://127.0.0.1:7777/api/v1/...`.

## Verified Commands

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvBatchSupervisor.ps1 -ManifestPath artifacts/batch-runs/full-admin-host-mutation-gate-20260510-154831-0410-account-rerun/manifest.json -AllowHostMutation
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-smoke-20260510-0410-final -ServiceName PureCVisorDesktopNode -ApiBaseUri http://127.0.0.1:7777 -DataRoot C:\ProgramData\PureCVisor\desktop-node
```

## Observed Result

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, `failed_step_id=null`.
- Service/MSI/Hyper-V route step: PASS, `attempt_count=1`, timeout false.
- OS mutation step: PASS, `attempt_count=1`, timeout false.
- Route parity steps: build current MSI, service-action smoke, MSI lifecycle smoke, installed .NET Host Hyper-V API route smoke all PASS.
- OS mutation steps: config migration apply, Event Log register/remove, firewall enable/remove, LAN listener IP smoke, internal trust-store install/remove/restore all PASS.
- Final service: `PureCVisorDesktopNode` `Running`, `StartMode=Auto`.
- Final service path includes `--web-prefix "http://127.0.0.1:80/"`, `--prefix "http://127.0.0.1:7777/"`, `--api-token-credential-target`, `--account-file`, `--jwt-signing-key-file`, diagnostics root, route timeout, request limit, burst limit, and retry-after arguments.
- Installed account login smoke: PASS after the full gate, login/session/RBAC/console `200`, `runtime_auth_mode=account_rbac_jwt`.
- Account/JWT file content hashes restored after smoke: true.
- Account/JWT file protected ACL restored after smoke: true, `SYSTEM:R` and `Administrators:R`.
- Boot time unchanged: true.
- Final firewall rule count: `0`.
- Final Event Log source present: false.
- Internal trust cert present: true.
- Remaining `pcv-*` VMs: `[]`.

## Boundaries

This is internal administrator opt-in host mutation evidence. It is not public trusted signing, timestamp, winget submission, external stable publication, public catalog upload, or public stable installer URL evidence.
