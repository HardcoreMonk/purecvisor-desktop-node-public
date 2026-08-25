# Admin-smoke 패키지 2026-05-26 0.42.47

evidence_id: `admin-smoke-package-2026-05-26-04247`
result: `PASS`
scope: `internal-admin-smoke-hyperv-qos-mutation`
version: `0.42.47-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260526-04247`
msi_sha256: `9589086d092ee902b72ff7790cac5a25e6d806cdaac0d98e431a27048dc5e197`
payload_aggregate_sha256: `b206399efff98c9abf598580051ee9b81d87cc8450c4991de7d1944dafbb4aac`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `4adfc1acca292430c38afe3b447effd4107cc7b22ff42db7e1b65ced32bea92d`
cli_sha256: `b8b335313e4e847240badd87584d49f0858e9e1f02e5b51466f3af0e51677def`
tui_sha256: `f428fcfe9bf9a1b93dedf76ad3c891eef751ce629da007ba12909cfa9b1a75e6`
provenance_commit: `77f1a3f291b4f736218cb5110dcecd3b464860d4`
build_utc: `2026-05-25T19:12:28.7491545Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Phase 2 Hyper-V QoS mutation을 설치본 payload로 승격한
`0.42.47-admin-smoke` package 기록이다. `pcvcli vm blkio-set`,
`pcvcli vm bandwidth-set`, QoS preview/apply/readback/rollback native WMI path가 이
payload에 포함된다.

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.47-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260526-04247 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- Full admin host mutation gate: `artifacts/batch-runs/full-admin-host-mutation-gate-20260526-04247/summary.json`, `PASS`
- 실제 VM 대상 설치본 QoS mutation smoke: `artifacts/installed-cli-qos-mutation-smoke-20260526-04247/summary.json`, `PASS`
- Manual-admin package-pair closure: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md`, `PASS`
- 설치본 Web/TUI/CLI current-card smoke: `artifacts/installed-operator-surface-current-card-20260526-04247/summary.json`, `PASS`

## 경계

이 package build는 internal admin-smoke evidence다. Full admin host mutation과
manual-admin package-pair closure는 각각
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04247-hostmutation.md`,
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md`가 current
anchor로 소유한다. Public trusted signing, public stable installer URL, winget submission,
외부 stable publication은 주장하지 않는다.
