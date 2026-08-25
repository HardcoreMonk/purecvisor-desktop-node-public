# 전체 관리자 Host Mutation Gate - 2026-05-10 0.41.5 Host Mutation

evidence_id: full-admin-host-mutation-gate-2026-05-10-0415-hostmutation
status: pass
actual_execution: batch-supervisor-full-admin-host-mutation-gate
host_mutation_performed: true
version: 0.41.5-admin-smoke
batch_supervisor_artifact_root: artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415
routeparity_artifact_root: artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415
os_mutation_artifact_root: artifacts/os-mutation-gates-batch-profile-20260510-195837-0415
msi_sha256: add85ca6823c3f7cd33c82d60a9e85da0f4d06daf52ed649e8dd08f72edb67c6
provenance_commit: c9efe852db0e3fb4d120bc5058c56a38c7cb30db
signing_mode: AllowUnsignedDev
public_trusted_signing: excluded
external_stable_publication: not-claimed

## 범위

이 evidence는 실행 당시 `0.41.5-admin-smoke`를 manual-admin rebaseline readiness에서 최신 full admin host mutation gate PASS로 승격한 기록이다. 명시적 elevated operator opt-in으로 Batch Supervisor `FullAdminHostMutationGate` profile을 실행했으며, 범위는 Service/MSI/Hyper-V route parity와 이어지는 OS mutation gate다.

이전 `manual-admin-rebaseline-readiness-2026-05-10-0415`는 non-mutating readiness evidence로 남는다. 이 문서는 baseline host gate의 mutating PASS evidence다.

## 검증 명령

```powershell
Import-Module .\packaging\windows-desktop-node\tools\PcvBatchSupervisor.psm1 -Force
$manifest = New-PcvBatchSupervisorManifest -BatchId 'full-admin-host-mutation-gate-20260510-195837-0415' -RepoRoot (Resolve-Path '.').Path -ArtifactRoot 'artifacts/batch-runs/full-admin-host-mutation-gate-20260510-195837-0415' -Profile FullAdminHostMutationGate -ProfileOptions @{ version = '0.41.5-admin-smoke'; iso_path = 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso'; routeparity_artifact_root = 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260510-195837-0415'; os_gate_artifact_root = 'artifacts/os-mutation-gates-batch-profile-20260510-195837-0415'; lan_prefix = 'http://[redacted-private-endpoint]:7777/' }
Invoke-PcvBatchSupervisor -Manifest $manifest -AllowHostMutation
curl.exe -i --max-time 10 http://127.0.0.1/
curl.exe -i --max-time 10 http://127.0.0.1/pcv-config.js
curl.exe -i --max-time 10 http://127.0.0.1:7777/api/v1/runtime/policy
```

## 관찰 결과

- Batch summary: `ok=true`, `status=completed`, `total_steps=2`, `executed_steps=2`, `failed_step_id=null`, `next_resume_step_id=null`.
- Service/MSI/Hyper-V route step: PASS, `attempt_count=1`, timeout false, duration `103055 ms`.
- OS mutation step: PASS, `attempt_count=1`, timeout false, duration `11077 ms`.
- Route parity steps: build current MSI, service-action smoke, MSI lifecycle smoke, installed .NET Host Hyper-V API route smoke all PASS.
- MSI lifecycle steps: install, repair, uninstall preserve, install-remove-data, uninstall-remove-data, and final restore install all exit `0`; no reboot initiated.
- OS mutation steps: config migration apply, Event Log register/remove, firewall enable/remove, LAN listener IP smoke, internal trust-store install/remove/restore all PASS.
- Final service: `PureCVisorDesktopNode` `Running`, `StartMode=Auto`.
- Installed manifest version: `0.41.5-admin-smoke`.
- Final service path includes Web Console `--web-prefix "http://127.0.0.1:80/"`, API `--prefix "http://127.0.0.1:7777/"`, Windows Event Log writer/provider arguments, Credential Manager token target, account/JWT file arguments, diagnostics root, route timeout, request limit, burst limit, and retry-after arguments.
- LAN listener smoke: `http://[redacted-private-endpoint]:7777/` runtime policy, `/`, `/index.html`, and `/app.js` returned HTTP `200` with token redacted.
- Web Console smoke: `curl.exe http://127.0.0.1/` returned HTTP `200`.
- Web config smoke: `curl.exe http://127.0.0.1/pcv-config.js` returned HTTP `200` and API origin `http://127.0.0.1:7777`.
- API unauthenticated boundary: `GET http://127.0.0.1:7777/api/v1/runtime/policy` returned HTTP `401` with `PCV_AUTH_REQUIRED`, confirming the API listener remained up and auth-gated.
- Boot time unchanged: true.
- Final firewall rule count: `0`.
- Final Event Log source present: false.
- Internal trust cert present: true, root `E49CD75AF53CCF7FA73C97E47443096A4507FB7E`, publisher `8C5F3B5030D3A54B1150C2C30CFD9868800DF0C6`.
- Remaining `pcv-*` VMs: `[]`.

## 경계

이 문서는 내부 administrator opt-in host mutation evidence다. Public trusted signing, timestamp, winget submission, external stable publication, public catalog upload, public stable installer URL, public clean-host signed install/update/rollback evidence가 아니다.
