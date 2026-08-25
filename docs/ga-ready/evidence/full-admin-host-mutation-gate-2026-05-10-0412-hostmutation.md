# Full Admin Host Mutation Gate - 2026-05-10 0.41.2 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-10-0412-hostmutation
status: pass
actual_execution: batch-supervisor-full-admin-host-mutation-gate
host_mutation_performed: true
version: 0.41.2-admin-smoke
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260510-161416-0412
msi_sha256: ba54a4d10c7ca0eb51f0f68f4948cf637a614834edab097e5888192a293a3cf0
provenance_commit: d098f0fc631ff1799d7dd238a84e896fe8616230
signing_mode: AllowUnsignedDev
public_trusted_signing: not-claimed
external_stable_publication: not-claimed

## Scope

This evidence records an explicit elevated operator opt-in host mutation run for the latest full admin gate after document-based batch classification approval. It covers the Batch Supervisor `FullAdminHostMutationGate` profile: Service/MSI/Hyper-V route parity followed by OS mutation gates.

Installed account login smoke was not rerun in this slice. The latest installed account login smoke remains `artifacts/installed-account-login-smoke-20260510-0410-final`.

## Verified Commands

```powershell
Import-Module packaging/windows-desktop-node/tools/PcvBatchSupervisor.psm1 -Force
$manifest = New-PcvBatchSupervisorManifest -BatchId 'full-admin-host-mutation-gate-20260510-161416-0412' -RepoRoot (Resolve-Path '.').Path -ArtifactRoot 'artifacts/batch-runs/full-admin-host-mutation-gate-20260510-161416-0412' -Profile FullAdminHostMutationGate -ProfileOptions @{ version = '0.41.2-admin-smoke'; iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'; routeparity_artifact_root = 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-161416-0412'; os_gate_artifact_root = 'artifacts/os-mutation-gates-batch-profile-20260510-161416-0412'; lan_prefix = 'http://[redacted-private-endpoint]:7777/' }
Invoke-PcvBatchSupervisor -Manifest $manifest -AllowHostMutation
curl.exe -i --max-time 10 http://127.0.0.1/
curl.exe -i --max-time 10 http://127.0.0.1/pcv-config.js
curl.exe -i --max-time 10 http://127.0.0.1:7777/api/v1/runtime/policy
```

## Observed Result

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, `failed_step_id=null`, `next_resume_step_id=null`.
- Service/MSI/Hyper-V route step: PASS, `attempt_count=1`, timeout false, duration `90843 ms`.
- OS mutation step: PASS, `attempt_count=1`, timeout false, duration `11059 ms`.
- Route parity steps: build current MSI, service-action smoke, MSI lifecycle smoke, installed .NET Host Hyper-V API route smoke all PASS.
- OS mutation steps: config migration apply, Event Log register/remove, firewall enable/remove, LAN listener IP smoke, internal trust-store install/remove/restore all PASS.
- Final service: `PureCVisorDesktopNode` `Running`, `StartMode=Auto`.
- Final service path includes Web Console `--web-prefix "http://127.0.0.1:80/"`, API `--prefix "http://127.0.0.1:7777/"`, account/JWT file arguments, diagnostics root, route timeout, request limit, burst limit, and retry-after arguments.
- Web Console smoke: `curl.exe http://127.0.0.1/` returned HTTP `200`.
- Web config smoke: `curl.exe http://127.0.0.1/pcv-config.js` returned HTTP `200` and API origin `http://127.0.0.1:7777`.
- API unauthenticated boundary: `GET http://127.0.0.1:7777/api/v1/runtime/policy` returned HTTP `401` with `PCV_AUTH_REQUIRED`, confirming the API listener remained up and auth-gated.
- Boot time unchanged: true.
- Final firewall rule count: `0`.
- Final Event Log source present: false.
- Internal trust cert present: true, root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`, publisher `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`.
- Remaining `pcv-*` VMs: `[]`.

## Boundaries

This is internal administrator opt-in host mutation evidence. It is not public trusted signing, timestamp, winget submission, external stable publication, public catalog upload, public stable installer URL, or public clean-host signed install/update/rollback evidence.

