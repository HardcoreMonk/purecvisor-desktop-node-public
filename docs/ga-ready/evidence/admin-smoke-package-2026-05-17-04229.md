# Admin-smoke package 2026-05-17 0.42.29

evidence_id: `admin-smoke-package-2026-05-17-04229`
result: `PASS`
scope: `internal-admin-smoke-package-build`
version: `0.42.29-admin-smoke`
manual_admin_package_pair: `0.42.28-admin-smoke -> 0.42.29-admin-smoke`
manual_admin_campaign: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04228-04229.md`
manual_admin_descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
manual_admin_update_zip_sha256: `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`
artifact_root: `artifacts/admin-smoke-package-20260517-04229`
package_build_decision: `executed-0.42.29-admin-smoke`
msi_sha256: `2031c4b669e9a6bf18019302b7291f7484588548ca64bfeb4afa2abf2a09bf77`
payload_aggregate_sha256: `f18dbe5a813a55bc42698b9cd13275cf10265ea1dffed43cfccbba15fe15a085`
product_wrapper_sha256: `0931a7b782693d4ef19c7f6092e61bf67f13e2af57106521bfab96b4574bd59f`
host_exe_sha256: `8dfd2c896138b480ec561747c19a93344cd053319f25f78165e2035cabae124b`
cli_sha256: `d038d5d6e8dd33ebd6a3f27740eeb976c33dff3c988cec6cc4d339f39c315d4a`
tui_sha256: `44621afdc0449c022d614091056a654dec0bee14b57e868eaa05a5fc8913d31e`
provenance_commit: `d306712ad671c8a00d5c560765b8952e24a07502`
build_utc: `2026-05-17T10:05:25.4291499Z`
signing_mode: `AllowUnsignedDev`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260517-04229`
full_gate_msi_sha256: `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`
host_ops_web_diagnostics_bucket_table_contract: `host-ops-web-diagnostics-bucket-table-v1`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 PR #153 post-merge main 위에서 selector/package-chain follow-up payload를
포함해 만든 `0.42.29-admin-smoke` clean package build 기록이다. Build script는
unsigned internal admin-smoke 모드로 완료했고 provenance와 publication descriptor를
`artifacts/admin-smoke-package-20260517-04229`에 남겼다.

## Artifact

| 항목 | 값 |
| --- | --- |
| MSI | `artifacts/admin-smoke-package-20260517-04229/PureCVisorDesktopNode-0.42.29-admin-smoke-windows-x64.msi` |
| provenance | `artifacts/admin-smoke-package-20260517-04229/PureCVisorDesktopNode-0.42.29-admin-smoke-windows-x64.provenance.json` |
| publication descriptor | `artifacts/admin-smoke-package-20260517-04229/PureCVisorDesktopNode-0.42.29-admin-smoke-windows-x64.publication.json` |
| MSI SHA-256 | `2031c4b669e9a6bf18019302b7291f7484588548ca64bfeb4afa2abf2a09bf77` |
| provenance commit | `d306712ad671c8a00d5c560765b8952e24a07502` |
| signing mode | `AllowUnsignedDev` |

## Operational Follow-up

이 clean package build는 product payload build record이고, 같은 version의 full admin
host mutation gate는 routeparity runner가 다시 빌드한 operational package를 사용했다.
Operational full-gate batch는 `full-admin-host-mutation-gate-20260517-04229`이고 package
root는 `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04229`이며 MSI
SHA-256은 `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`다.
Host Ops Web diagnostics bucket table contract는
`host-ops-web-diagnostics-bucket-table-v1`로 current-card evidence와 연결한다.

이 evidence는 internal admin-smoke 범위다. Public trusted signing, winget 제출,
public stable installer URL, 외부 stable publication은 주장하지 않는다.
