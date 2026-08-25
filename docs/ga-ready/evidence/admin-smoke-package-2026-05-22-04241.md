# Admin-smoke 패키지 2026-05-22 0.42.41

evidence_id: `admin-smoke-package-2026-05-22-04241`
result: `PASS`
scope: `internal-admin-smoke-installed-tui-row-projection-fix`
version: `0.42.41-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260522-04241`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260522-04241`
operational_full_gate_batch_id: `full-admin-host-mutation-gate-20260522-04241`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-22-04240-04241.md`
installed_operator_surface_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-22-04241.md`
actual_vm_row_projection_evidence: `docs/ga-ready/evidence/web-tui-qos-guest-readback-actual-vm-2026-05-22-04241.md`
msi_sha256: `d1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639`
operational_full_gate_msi_sha256: `e080dbff6525754be7a35dfe316745f9c2f8878ad286a31ea66388ba6915d8fb`
payload_aggregate_sha256: `21aeb02757495d8296151ce20dda987ef36fcb2f3320f5163131ffc90e65c361`
operational_full_gate_payload_aggregate_sha256: `132695d2e676a3b24321c08cfd783378f74b957865eda2b96b70ea91c31a3b9b`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `afe8a8543b53c215c1ee61211aa589d5c8827c13ecfd23e21173fb374be29656`
cli_sha256: `07c3878742f41f547bb1bddca2f78e9258c1fa79f4e6e7f24dd9353dae55dfb9`
tui_sha256: `a75654e119a69228ed09d87b31dab3ec61859f8aed8fbd846ac39c55f46b7305`
provenance_commit: `2f41da1073df6e65113ae8ddaeb183e9b55874f4`
build_utc: `2026-05-22T11:02:08.5455305Z`
operational_full_gate_build_utc: `2026-05-22T11:05:10.8088099Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.40-admin-smoke` 실제 VM smoke에서 발견한 설치본 TUI row projection
blocker를 설치 가능한 `0.42.41-admin-smoke` package로 반영한 기록이다. 승격 범위는
`pcvtui --smoke-once vm`이 API `vm.list` data envelope를 실제 VM table row로 투영하는지
확인하는 Operator Surface payload다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.41-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260522-04241 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- Full admin host mutation gate: `full-admin-host-mutation-gate-20260522-04241`, PASS
- Manual-admin package-pair: `0.42.40-admin-smoke -> 0.42.41-admin-smoke`, PASS
- 설치본 실제 VM TUI row projection: `pcv-ux-qos-04241`, `installed_tui_actual_vm_row_projection=pass`
- 설치본 Web/TUI/CLI current-card: `artifacts/installed-operator-surface-current-card-20260522-04241/summary.json`, PASS

## 경계

이 evidence는 internal admin-smoke package build다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
