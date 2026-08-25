# Admin-smoke package 2026-05-19 0.42.34

evidence_id: `admin-smoke-package-2026-05-19-04234`
result: `PASS`
scope: `internal-admin-smoke-product-payload-build-and-fullgate-operational-package`
version: `0.42.34-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260519-04234`
operational_full_gate_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`
package_build_decision: `pcvcli-linux-palette-and-utf8-interactive-shell`
msi_sha256: `cfd46fb46c1eb886d91112b22a0a21790ad1c4d9d856d5817798edac5167c6f5`
payload_aggregate_sha256: `ca1394ac3e219548da275a1e792a21296d82af4038c554363dfb70789b57eed0`
operational_full_gate_msi_sha256: `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`
operational_full_gate_payload_aggregate_sha256: `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `76e3de6c00a9532bd6ae7709e0cf243cafec65fe8399cd9c2a89d796548241af`
cli_sha256: `84d38979cb2b4cfab4060022a11d86e5db0f7b4ed7f87c2d90ad6ab377cec9f3`
tui_sha256: `0100291dc1752b7f9a819e6792754228e1fb1b575b4350ddd1c0ca992acab78c`
provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
build_utc: `2026-05-19T09:59:50.4360853Z`
signing_mode: `AllowUnsignedDev`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Windows `pcvcli.exe` interactive shell을 Linux `pcvctl` cyber palette와
동일한 256-color ANSI 스타일로 맞추고, redirected stdout에서도 prompt glyph `❯`가
깨지지 않도록 UTF-8 console output encoding을 설정한 뒤 생성한 product payload
package build 기록이다.

## Artifact

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260519-04234/PureCVisorDesktopNode-0.42.34-admin-smoke-windows-x64.msi` |
| provenance | `artifacts/admin-smoke-package-20260519-04234/PureCVisorDesktopNode-0.42.34-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260519-04234/PureCVisorDesktopNode-0.42.34-admin-smoke-windows-x64.publication.json` |
| MSI SHA-256 | `cfd46fb46c1eb886d91112b22a0a21790ad1c4d9d856d5817798edac5167c6f5` |
| payload aggregate SHA-256 | `ca1394ac3e219548da275a1e792a21296d82af4038c554363dfb70789b57eed0` |
| payload file version | `1.42.34.0` |
| operational full-gate MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234/PureCVisorDesktopNode-0.42.34-admin-smoke-windows-x64.msi` |
| operational full-gate MSI SHA-256 | `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78` |
| operational full-gate payload aggregate SHA-256 | `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5` |

## 검증

- `packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.34-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260519-04234 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe`: exit `0`
- `dotnet publish` service host: exit `0`
- `dotnet publish` `pcvcli.exe`: exit `0`
- `dotnet publish` `pcvtui.exe`: exit `0`
- `wix build`: exit `0`

## Superseded package

`0.42.33-admin-smoke`는 같은 Linux palette 변경을 포함한 중간 package였지만,
설치본 redirected interactive smoke에서 prompt glyph `❯`가 `?`로 기록되는 UTF-8
출력 문제가 확인되어 최종 evidence로 승격하지 않았다. `0.42.34-admin-smoke`는
`Program.ConfigureConsoleEncoding()`으로 UTF-8 output encoding을 고정한 뒤 재빌드한
superseding package다.

## Operational 승격

후속 full admin host mutation gate에서 같은 `0.42.34-admin-smoke` version을 operational
package로 재빌드했다. `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`
의 MSI SHA-256은 `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`,
payload aggregate SHA-256은 `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`다.
이 operational package는 `full-admin-host-mutation-gate-20260519-04234`와
`0.42.32-admin-smoke -> 0.42.34-admin-smoke` manual-admin package-pair closure의 target이다.

## 경계

이 package build는 internal admin-smoke anchor다. Public trusted signing, winget 제출,
public stable installer URL, 외부 stable publication은 주장하지 않는다.
