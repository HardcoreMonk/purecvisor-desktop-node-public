# Admin-smoke 패키지 2026-05-20 0.42.39

evidence_id: `admin-smoke-package-2026-05-20-04239`
result: `PASS`
scope: `internal-admin-smoke-pcvcli-hyperv-qos-guest-service-parity`
version: `0.42.39-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260520-04239`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04239`
operational_full_gate_batch_id: `full-admin-host-mutation-gate-20260520-04239`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md`
msi_sha256: `b6fac120b145b5d0a8bf48a955037593756613d5bbe355bae96de59da4f0d805`
operational_full_gate_msi_sha256: `8ccf24a0a304b82dfcb0039c92149806539cf74977014bc3468c589e4ddf624f`
payload_aggregate_sha256: `359aee4c862fb4efc35a1dd631c92219e62e87adf7e96c8134d687fe38c7dede`
operational_full_gate_payload_aggregate_sha256: `cd2d820c66e6f28df8a740207c7182ab744d5d984fc3bfc6a009a35da95c0869`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `c1e58298cfe32832e033c2a2084f30247fffb9badbef2a62e53d63c6c706c580`
cli_sha256: `533e5af8040df1e7055725c7c2fb530960aa62b54f1a24bae1a4706ae695d990`
tui_sha256: `a1043f58efc18eb3f7041d3f46e72e95e14a57ef9fc51c9847a17667e396ad24`
provenance_commit: `6fd931baf3de77435d0d11b92424cf6657ea4515`
build_utc: `2026-05-20T13:33:43.3402737Z`
operational_full_gate_build_utc: `2026-05-20T13:34:42.0857967Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 ADR-0007 PCVCLI Hyper-V QoS/guest-service parity slice를 설치 가능한
`0.42.39-admin-smoke` package로 묶은 기록이다. 승격 범위는 `vm limit` mutation,
`vm blkio-get`, `vm bandwidth`, `vm guest-agent-status`, `vm guest-ping` readback이다.
Linux cgroup blkio/bandwidth와 qemu guest agent semantics는 그대로 claim하지 않는다.

## 검증

- `dotnet test src\DesktopNode.sln --no-restore`: `629` passed
- `Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed`: `74` passed, package 전 단계
- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.39-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260520-04239 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- Full admin host mutation gate: `full-admin-host-mutation-gate-20260520-04239`, PASS
- Manual-admin package-pair: `0.42.38-admin-smoke -> 0.42.39-admin-smoke`, PASS

## 경계

이 evidence는 internal admin-smoke package build다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
