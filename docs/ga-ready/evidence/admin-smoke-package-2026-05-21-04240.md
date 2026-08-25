# Admin-smoke 패키지 2026-05-21 0.42.40

evidence_id: `admin-smoke-package-2026-05-21-04240`
result: `PASS`
scope: `internal-admin-smoke-web-tui-qos-guest-readback-surface`
version: `0.42.40-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260521-04240`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260521-04240`
operational_full_gate_batch_id: `full-admin-host-mutation-gate-20260521-04240`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-21-04239-04240.md`
msi_sha256: `4979a3a60f96b8e8dbcda41bd722c33909c2faf39bc4cf88b8a79fb89e9628e8`
operational_full_gate_msi_sha256: `eaf2d08e650779ed3f07bbd71f8067fe591a0277a5399f647b6511cb15b86c41`
payload_aggregate_sha256: `0c5e566f49bd4ef5c78249b3439a4441462a3c6b54433985be4b9badb9618666`
operational_full_gate_payload_aggregate_sha256: `cd49f061dfd0e2e5afe45cd34befcfb28e02bbd9038eff1fbaef34f8c9616ea5`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `7cd6cbd58d92f32e9c213e24b4ff87b03dfd12d8d83faf553274d01f99c31054`
cli_sha256: `6fa4554b653ee23a472f1ff8bd291ba19ee8742e3f4355eb3bc91ccf61733af0`
tui_sha256: `65d63f2bd675237c1117971b3bda81dc8139f4ff09d666fbf4b76b14ea89791a`
provenance_commit: `adb7b8c77ff60b64c5ac4d840e2bdfac62a3793a`
build_utc: `2026-05-21T12:05:16.6327874Z`
operational_full_gate_build_utc: `2026-05-21T12:07:07.4648275Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Web/TUI QoS/guest readback Operator Surface slice를 설치 가능한
`0.42.40-admin-smoke` package로 묶은 기록이다. 승격 범위는 선택 VM의 read-only
`vm.blkio-get`, `vm.bandwidth`, `vm.guest-agent-status`, `vm.guest-ping` surface다.
Web/TUI direct QoS mutation control, Linux cgroup QoS 호환, qemu guest agent 호환 claim은
하지 않는다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.40-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260521-04240 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- Full admin host mutation gate: `full-admin-host-mutation-gate-20260521-04240`, PASS
- Manual-admin package-pair: `0.42.39-admin-smoke -> 0.42.40-admin-smoke`, PASS

## 경계

이 evidence는 internal admin-smoke package build다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
