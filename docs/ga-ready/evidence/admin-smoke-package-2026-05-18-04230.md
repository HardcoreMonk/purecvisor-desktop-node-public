# Admin-smoke package 2026-05-18 0.42.30

evidence_id: `admin-smoke-package-2026-05-18-04230`
result: `PASS`
scope: `internal-admin-smoke-package-build`
version: `0.42.30-admin-smoke`
artifact_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`
superseded_initial_artifact_root: `artifacts/admin-smoke-package-20260518-04230`
package_build_decision: `executed-0.42.30-admin-smoke-cli-tui-auto-token-path-and-fileversion-fix`
msi_sha256: `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`
superseded_initial_msi_sha256: `c80be181ab99e9d9d5d7f59d7eb40c22841fa202dea36dcff549e5ba94552763`
payload_aggregate_sha256: `0fddc06c7ced0239ea04a89fd90cc0c152a64688904e0f58b97c3fcd5368a28c`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `edd28c27aa9e592ef0fc3113d08fee18f50ef7c5cf3e158029411a5d2e27a6ff`
cli_sha256: `d2a574d975c247a777ae3758ab8ed38ecb1861757cf7d50d1f92ae365401db1f`
tui_sha256: `b0c55ead69ba6006b72b29098c39f36134d11fdbfb83d1b9189bf606dbb83038`
provenance_commit: `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`
build_utc: `2026-05-18T09:47:12.6538779Z`
signing_mode: `AllowUnsignedDev`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260518-04230`
manual_admin_package_pair: `0.42.29-admin-smoke -> 0.42.30-admin-smoke`
public_trusted_signing: `excluded`
external_stable_publication: `not-claimed`

이 evidence는 CLI/TUI 자동 token source 탐색, MSI machine `PATH` 등록, 설치 payload
file version 단조 증가 보장을 포함한 `0.42.30-admin-smoke` package build 기록이다.
초기 clean package `artifacts/admin-smoke-package-20260518-04230`는 MSI SHA-256
`c80be181ab99e9d9d5d7f59d7eb40c22841fa202dea36dcff549e5ba94552763`로 생성됐지만,
기존 설치본의 accidental `.NET FileVersion=1.0.0.0` keyfile보다 낮은
`0.42.30.0` file version 때문에 Windows Installer가 payload 교체를 거부했다.

후속 fix에서 MSI payload file version을 `1.42.30.0`으로 올렸고, full admin host
mutation route package가 현재 operational package가 됐다. 따라서 `0.42.30` current
package claim은 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`
및 MSI SHA-256 `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`이
소유한다.

## Artifact

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230/PureCVisorDesktopNode-0.42.30-admin-smoke-windows-x64.msi` |
| provenance | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230/PureCVisorDesktopNode-0.42.30-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230/PureCVisorDesktopNode-0.42.30-admin-smoke-windows-x64.publication.json` |
| MSI SHA-256 | `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86` |
| provenance commit | `f4349cf049db66b0ae1d5d38a948a6b03a8b0648` |
| signing mode | `AllowUnsignedDev` |
| payload file version | `1.42.30.0` |

## 설치 반영

Full admin host mutation gate는 이 package를 설치해 service `PureCVisorDesktopNode`
`Running`, Web/API split, `--batch-evidence-root`, CLI/TUI payload 교체를 확인했다.
설치본 Web/TUI/CLI current-card smoke는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`에
분리해 기록한다.

## 경계

이 build와 설치 반영은 internal admin-smoke 범위다. Public trusted signing, winget
제출, public stable installer URL, 외부 stable publication은 주장하지 않는다.
