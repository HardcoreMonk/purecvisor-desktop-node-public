# Admin-smoke 패키지 2026-05-20 0.42.38

evidence_id: `admin-smoke-package-2026-05-20-04238`
result: `PASS`
scope: `internal-admin-smoke-vm-media-resource-mutation-routes`
version: `0.42.38-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260520-04238`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04238`
operational_full_gate_batch_id: `full-admin-host-mutation-gate-20260520-04238`
manual_admin_candidate_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04237-04238.md`
msi_sha256: `2ae739cee46780b01d1c3873d8186c30761243df578ecf7ab1e9d66a19f572b4`
operational_full_gate_msi_sha256: `b3090de88edb4724d99bc33c65a046b2fc9184f7ccc6a1f37b50e7ce07685f1f`
payload_aggregate_sha256: `40ec6157c99dffaf29bf9d0dcd1c513ba99fee77c21bb883976aa03eb3b73ca7`
operational_full_gate_payload_aggregate_sha256: `ab5cb6404e8f482ad3ecb32b087cb7e5020aceca595adb0fa01e3aa26d2317b8`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `800ca88c16899f5b81d74700318561bee1ffa34a12cc7ae1d244a93d82077c77`
cli_sha256: `08af93c271c8eab12abf6d8e6ef10e73eea8fcbdf975fefb02d2d0ba1c17d6c8`
tui_sha256: `980b0d856b442e7c07d5877ba7a09547773c31c2876d5a612c66d65aaae3828a`
provenance_commit: `3c49b9a010c57e4a8637cb32ed17cd432dd0cd6f`
build_utc: `2026-05-20T09:51:39.3580040Z`
operational_full_gate_build_utc: `2026-05-20T09:52:54.2904475Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 PCVCLI Linux compatibility gap 중 Desktop Node Hyper-V backend로 승격 가능한
media/resource mutation slice를 `0.42.38-admin-smoke` package로 묶은 기록이다. 승격 범위는
`vm eject`, `vm delete-status`, `vm set-memory`, `vm set-vcpu`, `vm disk-resize`이며,
CLI/TUI/Web/API/job runtime/Hyper-V native adapter route contract를 함께 갱신했다.

## 구현 범위

| 영역 | 승격 내용 |
| --- | --- |
| CLI | `pcvcli vm eject|delete-status <vm>`, `pcvcli vm set-memory|set-vcpu|disk-resize <vm> <value>` API route 연결 |
| Runtime/API | `GET /api/v1/vms/{vmId}/delete-status`, `POST /api/v1/vms/{vmId}/eject|set-memory|set-vcpu|disk-resize` |
| Hyper-V domain | `vm-media-provider`, `vm-resource-mutation-provider` WMI provider 분리 |
| TUI | VM inspector action/help와 API client method 추가 |
| Web Console | VM detail action button과 memory/vCPU/disk resize form 추가 |
| Contract docs | `docs/CLI_COMMAND_USAGE.md`, `src/DesktopNode.Cli/README.md`, `docs/ga-ready/EVIDENCE_INDEX.md` 갱신 |

## 검증

- `dotnet test src\DesktopNode.sln --no-restore`: Runtime `17`, Contracts `7`, Service `11`, CLI `87`, TUI `130`, Host `148`, API `214` passed
- `npm test --prefix web`: passed
- `Invoke-Pester -Path web\tests\PcvDesktopWeb.Static.Tests.ps1 -Output Detailed`: `45` passed
- `git diff --check`: passed
- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.38-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260520-04238 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`

## 운영 승격 상태

Clean package artifact의 MSI SHA-256은 `2ae739cee46780b01d1c3873d8186c30761243df578ecf7ab1e9d66a19f572b4`다. Full admin host mutation gate는 같은 provenance commit으로 별도 operational MSI를 재빌드했고, 해당 MSI SHA-256은 `b3090de88edb4724d99bc33c65a046b2fc9184f7ccc6a1f37b50e7ce07685f1f`다. 두 package 모두 CLI/TUI/Host binary hash는 동일하고, operational current 판단은 `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04238-hostmutation.md`가 소유한다.

`0.42.37-admin-smoke -> 0.42.38-admin-smoke` manual-admin package-pair candidate는 readiness, installed update/rollback, Burn, MSIX, installed runtime ops summary를 PASS했지만 dedicated clean-host baseline MSI install이 `1603`으로 실패해 closure가 아니다. 해당 blocker는 `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04237-04238.md`에 남긴다.

## 경계

이 evidence는 internal admin-smoke package build다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
