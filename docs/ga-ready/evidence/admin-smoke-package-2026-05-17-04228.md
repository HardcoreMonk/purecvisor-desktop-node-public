# Admin-smoke package 2026-05-17 0.42.28

evidence_id: `admin-smoke-package-2026-05-17-04228`
result: `PASS`
scope: `internal-admin-smoke-package-build`
version: `0.42.28-admin-smoke`
artifact_root: `artifacts/admin-smoke-package-20260517-04228`
package_build_decision: `executed-0.42.28-admin-smoke`
msi_sha256: `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74`
payload_aggregate_sha256: `b603621dad0419829ba73a65d73c1a6fdb1dd1d347386aa5c4dbe197b7606649`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `0de404228b1fc951b8389a41992cd61c3610da3d0d0ca3187195fe895f6a0e1b`
cli_sha256: `04a6d5dd5d7a41583468ad60b43ed65dbb8507db9767d7fd65cb49cbf4b64d04`
tui_sha256: `6d79f0463d990944d4e8497154ba41f2ec3518e156955b71ad17d3bded38af61`
provenance_commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
build_utc: `2026-05-17T07:46:06.4353981Z`
signing_mode: `AllowUnsignedDev`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260517-04228`
full_gate_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
host_ops_web_diagnostics_bucket_table_contract: `host-ops-web-diagnostics-bucket-table-v1`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 PR #151 post-merge main 위에서 Host Ops Web diagnostics bucket table product
payload를 포함해 만든 `0.42.28-admin-smoke` clean package build 기록이다. Build script는
unsigned internal admin-smoke 모드로 완료했고 provenance와 publication descriptor를
`artifacts/admin-smoke-package-20260517-04228`에 남겼다.

## Artifact

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260517-04228/PureCVisorDesktopNode-0.42.28-admin-smoke-windows-x64.msi` |
| provenance | `artifacts/admin-smoke-package-20260517-04228/PureCVisorDesktopNode-0.42.28-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260517-04228/PureCVisorDesktopNode-0.42.28-admin-smoke-windows-x64.publication.json` |
| MSI SHA-256 | `a3093d329005b0ea98c9a28af4fddfd8f6e710c923b53b9435422c9423962d74` |
| provenance commit | `b9676f6dc37d667ae0d60367e9f4e576a27e3864` |
| signing mode | `AllowUnsignedDev` |

## Operational Follow-up

이 clean package build는 product payload build record이고, 같은 version의 full admin
host mutation gate는 routeparity runner가 다시 빌드한 operational package를 사용했다.
Operational full-gate batch는 `full-admin-host-mutation-gate-20260517-04228`이고 package root는
`artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04228`이고 MSI
SHA-256은 `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`다.
Host Ops Web diagnostics bucket table contract는
`host-ops-web-diagnostics-bucket-table-v1`로 current-card evidence와 연결한다.

이 evidence는 internal admin-smoke 범위다. Public trusted signing, winget 제출,
public stable installer URL, 외부 stable publication은 주장하지 않는다.
