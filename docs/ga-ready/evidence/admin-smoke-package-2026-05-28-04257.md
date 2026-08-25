# Admin smoke package 2026-05-28 0.42.57

evidence_id: `admin-smoke-package-2026-05-28-04257`
result: `PACKAGE_BUILD_PASS`
scope: `internal-admin-smoke-public-boundary-current-evidence-rollup-payload`
version: `0.42.57-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260528-04257`
provenance: `artifacts/admin-smoke-package-20260528-04257/PureCVisorDesktopNode-0.42.57-admin-smoke-windows-x64.provenance.json`
msi: `artifacts/admin-smoke-package-20260528-04257/PureCVisorDesktopNode-0.42.57-admin-smoke-windows-x64.msi`
msi_sha256: `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`
payload_aggregate_sha256: `c24512aec2dae7e73da4af24778451b3b3dfdc52d2c7914db61ceaaefae67e07`
provenance_commit: `16cc0d6b592d7f2f9ead14c41d8f4ad0e1f28b76`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 포함 payload

`0.42.57-admin-smoke` package는 0.42.56 manual-admin closure main push의
public-boundary CI evidence를 Runtime/API `current_evidence.public_boundary.latest_main_push`
fallback과 Web/TUI/CLI current-card 표시로 승격한다. 근거 public-boundary run은
`26578120570`, job은 `78303066840`, head SHA는
`7a7d5de822bdb058b04149eeeef0a7eb462828b5`다.

| 산출물 | SHA-256 |
| --- | --- |
| Host EXE | `9434e1d8d2d3d52928ab14227581a67dcb7352b6e9a00a6df4e0a55a29c2dc6d` |
| PCVCLI EXE | `2ef327140aa2a43e1ea236f44c217705dd79a33f561a10c29f773e921b17e20c` |
| PCVTUI EXE | `0bd58072b1a7a596524ab9cfde8f2336380a318941d44f5499dc4ffdc0bf39ef` |

## 빌드 명령

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.57-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260528-04257 -SigningMode AllowUnsignedDev -SigningTrustModel LocalTest
```

이 evidence는 internal admin-smoke package build이며 public trusted signing 또는 외부 stable
publication을 주장하지 않는다.
