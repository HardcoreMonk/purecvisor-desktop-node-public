# Admin-smoke package 2026-05-19 0.42.32

evidence_id: `admin-smoke-package-2026-05-19-04232`
result: `PASS`
scope: `internal-admin-smoke-package-build`
version: `0.42.32-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260519-04232`
package_build_decision: `pcvcli-neon-vm-list-and-installed-real-vm-smoke`
msi_sha256: `8d8c585fe73c605bd938705ef63790768348791cb479bf42c4bbbf8b31af14dc`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04232`
operational_full_gate_msi_sha256: `3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9`
operational_full_gate_payload_aggregate_sha256: `21e2f8136ac53384bf86966e51f9040f7bbb37e62bc9e761640c0d1aeff35956`
operational_full_gate_provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
payload_aggregate_sha256: `b17130829d9851410a9d4c31a7b44a3e85d31ed78d15bb2d6ba024423240ddc6`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `603fccabdf548d1f651d8da30f01b8b60e837cc91436d10e4b84eac367c9e7d1`
cli_sha256: `a227de915d298e45bdc92d6f8a5341f54f7ee0785c2621dcfc8af0551afa6239`
tui_sha256: `c00060df8a8ad2a45e8c361efe61d95e46fd1aa93d83bfff28b3fa3f23b62399`
provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
build_utc: `2026-05-18T23:11:26.3753940Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `pcvcli vm list`의 실제 VM table 렌더링과 neon ANSI 컬러 출력을
포함한 `0.42.32-admin-smoke` package build 기록이다. Build는 내부 admin-smoke
범위이며 public trusted signing, winget 제출, public stable installer URL, 외부
stable publication은 주장하지 않는다.

## Artifact

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260519-04232/PureCVisorDesktopNode-0.42.32-admin-smoke-windows-x64.msi` |
| provenance | `artifacts/admin-smoke-package-20260519-04232/PureCVisorDesktopNode-0.42.32-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260519-04232/PureCVisorDesktopNode-0.42.32-admin-smoke-windows-x64.publication.json` |
| MSI SHA-256 | `8d8c585fe73c605bd938705ef63790768348791cb479bf42c4bbbf8b31af14dc` |
| payload aggregate SHA-256 | `b17130829d9851410a9d4c31a7b44a3e85d31ed78d15bb2d6ba024423240ddc6` |
| payload file version | `1.42.32.0` |

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.32-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260519-04232 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- `dotnet publish` service host: exit `0`
- `dotnet publish` `pcvcli.exe`: exit `0`
- `dotnet publish` `pcvtui.exe`: exit `0`
- `wix build`: exit `0`

## 경계

이 package build 자체는 product payload smoke anchor다. 이후 full admin host mutation
gate에서 같은 `0.42.32-admin-smoke` version을 operational package로 재빌드했다.
Current installed/package anchor는
`artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04232`의 MSI SHA-256
`3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9`와 payload aggregate
SHA-256 `21e2f8136ac53384bf86966e51f9040f7bbb37e62bc9e761640c0d1aeff35956`가 소유한다.
설치본 `pcvcli vm list` 실제 VM smoke는
`docs/ga-ready/evidence/installed-pcvcli-neon-vm-list-smoke-2026-05-19-04232.md`가
소유한다.
