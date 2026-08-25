# Admin-smoke package 2026-05-20 0.42.35

evidence_id: `admin-smoke-package-2026-05-20-04235`
result: `PASS`
scope: `internal-admin-smoke-product-payload-build-and-fullgate-operational-package`
version: `0.42.35-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260520-04235`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04235`
operational_full_gate_batch_id: `full-admin-host-mutation-gate-20260520-04235`
manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04234-04235-closed`
msi_sha256: `3a01cdda7757a3c13468242353547bba0ee9528dfa6d5b5c96aecce0b2e62496`
payload_aggregate_sha256: `1f71e05d1b033ce357ea0b87f3e96afc13fec02c50deb3b7a17dfc27642bb7c2`
operational_full_gate_msi_sha256: `12d05f2d783dfdb1db3f1596cd266af17578e33fca3f4fec272aac7df5e22697`
operational_full_gate_payload_aggregate_sha256: `ba966f3c41d81579dc6f065988c5fc015d47a9b0c8c77b4f4c3bf5962c1806a1`
host_exe_sha256: `83337c2c2068f844b2e89b4b2858ad034df47eaa0941701e7dadf0fe01600f98`
cli_sha256: `ba8f044e6e5b18c14cd866116a32e5d328462ad72c89f3f5aa75b014d3a5f15b`
tui_sha256: `f3073a56473c36ba1ef5f01fec589b0692a1af0f38e630a76c4b1f6f1d058780`
provenance_commit: `51a21d7c8612f598b85eeb58818ad3d61136c320`
build_utc: `2026-05-19T18:25:14.1825978Z`
operational_build_utc: `2026-05-19T18:28:06.573399Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 PR #160 merge commit `51a21d7c8612f598b85eeb58818ad3d61136c320`
기준 `0.42.35-admin-smoke` clean package와 full admin host mutation gate용 operational
package를 기록한다. Clean package는 `artifacts/admin-smoke-package-20260520-04235`에,
운영 승격 package는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04235`에
보존한다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.35-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260520-04235 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- full admin host mutation gate 내부 operational package build: exit `0`
- `dotnet publish` service host / `pcvcli.exe` / `pcvtui.exe`: exit `0`
- `wix build`: exit `0`

## 승격 관계

`0.42.35-admin-smoke` operational package는
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04235-hostmutation.md`와
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04234-04235.md`의 target package다.
이후 실제 설치본 CLI lifecycle smoke에서 Hyper-V `vm.pause` 요청 상태값/paused inventory
매핑 결함을 발견했고, 해당 code fix와 설치본 closure는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04237.md` 및
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04237.md`가
fast-follow evidence로 소유한다.

## 경계

이 package build는 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
