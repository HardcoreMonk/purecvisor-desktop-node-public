# Admin-smoke package 2026-05-17 0.42.27

evidence_id: `admin-smoke-package-2026-05-17-04227`
result: `PASS`
scope: `internal-admin-smoke-package-build`
version: `0.42.27-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260517-04227`
package_build_decision: `executed-0.42.27-admin-smoke`
msi_sha256: `0084d6ded5723ceb378c0805b9e9369e6626460bd6185d98e0a1028050f6be4a`
payload_aggregate_sha256: `928f9456b00a32995a823affce2957503516057eb629f5ee6fd1dc54f5c4c418`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `f92004a5980468312ce839418b15a74e71281622e3aec6fe8575abbe0bf772a7`
cli_sha256: `499d8f4c8e473af475fc4bcf5cfead10def2d2ff881a501e0a054e4be9e28c5f`
tui_sha256: `7b5fe31a17dc322ea3b4c7a8d37cd1d8fa517f795af77c3a0a251e58a1c593e6`
provenance_commit: `69aba3eb3ff08c843f1a481818ddc86eac2f019b`
build_utc: `2026-05-16T20:53:56.6145416Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `post-04226-ledger-contract-merge` 이후 Host Ops lifecycle descriptor
bridge product payload를 포함한 `0.42.27-admin-smoke` package build 기록이다.
Build script는 unsigned internal admin-smoke 모드로 완료했고 provenance와 publication
descriptor를 `artifacts/admin-smoke-package-20260517-04227`에 남겼다.

## Artifact

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260517-04227/PureCVisorDesktopNode-0.42.27-admin-smoke-windows-x64.msi` |
| provenance | `artifacts/admin-smoke-package-20260517-04227/PureCVisorDesktopNode-0.42.27-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260517-04227/PureCVisorDesktopNode-0.42.27-admin-smoke-windows-x64.publication.json` |
| MSI SHA-256 | `0084d6ded5723ceb378c0805b9e9369e6626460bd6185d98e0a1028050f6be4a` |
| provenance commit | `69aba3eb3ff08c843f1a481818ddc86eac2f019b` |
| signing mode | `AllowUnsignedDev` |

## Operational Follow-up

이 clean package build는 product payload build record이고, 같은 version의 full admin
host mutation gate는 routeparity runner가 다시 빌드한 operational package를 사용했다.
Operational full-gate package root는
`artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04227`이고 MSI
SHA-256은 `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`다.

이 evidence는 internal admin-smoke 범위다. Public trusted signing, winget 제출,
public stable installer URL, 외부 stable publication은 주장하지 않는다.
