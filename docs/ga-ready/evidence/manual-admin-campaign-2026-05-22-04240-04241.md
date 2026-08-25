# Manual-admin campaign 2026-05-22 0.42.40 -> 0.42.41

evidence_id: `manual-admin-campaign-2026-05-22-04240-04241`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.40-admin-smoke -> 0.42.41-admin-smoke`
baseline_version: `0.42.40-admin-smoke`
target_version: `0.42.41-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260522-04240-04241`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260522-04240-04241-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260522-04240-04241/manual-admin-campaign-descriptor-r2-windows-update/summary.json`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
runner_count: `6`
missing_count: `0`
not_pass_count: `0`
target_package_root: `artifacts/admin-smoke-package-20260522-04241`
target_msi_sha256: `d1a36e3efb1f7ae8588f34f4d70acb01037c41abcde4f40a35df669b5c31c639`
baseline_msi_sha256: `4979a3a60f96b8e8dbcda41bd722c33909c2faf39bc4cf88b8a79fb89e9628e8`
update_zip_sha256: `9ab7e266c093b98982aa854c19f901a6bb133f51c66904b9bfcdf56d538fee73`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260522-04241`
clean_host_windows_update: `requested-and-pass`
burn_bundle_sha256: `cbec6f5ee552229ec086a520ec6a530a922483cea519714e2a7ecb8797fd3a3f`
msix_v1_sha256: `6be5780e4efc37157020b40a82f07e4f544d368d7a0b064fe6f83ac6cf657b81`
msix_v2_sha256: `832604c1f8af235358594469a002971220710970b3590100372526d83c08817a`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 campaign은 `0.42.40-admin-smoke`를 baseline으로, 설치본 TUI row projection fix가 들어간
`0.42.41-admin-smoke`를 target으로 한 manual-admin package-pair closure다.
Readiness, installed update/rollback, dedicated clean-host Windows Update, Burn, MSIX,
installed runtime ops summary, descriptor generation이 모두 PASS했고 descriptor
`missing_count=0`, `not_pass_count=0`으로 닫혔다.

## PASS bucket

| Bucket | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260522-04240-04241/manual-admin-rebaseline-readiness-r2/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260522-04240-04241/lifecycle/product-update-rollback-r2/summary.json` | `PASS`, downshift `0.42.41 -> 0.42.40`, update `0.42.41`, rollback `0.42.40`, final current `0.42.41` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260522-04240-04241/clean-host-windows-update/summary.json` | `PASS`, install/update/rollback exit `0`, final Web `200`, blocker `none` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260522-04240-04241/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `cbec6f5ee552229ec086a520ec6a530a922483cea519714e2a7ecb8797fd3a3f` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260522-04240-04241/summary.json` | `PASS`, `0.42.40.0 -> 0.42.41.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260522-04240-04241/installed-runtime-ops-summary/summary.json` | `PASS`, installed version `0.42.41-admin-smoke` |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260522-04240-04241/manual-admin-campaign-descriptor-r2-windows-update/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |
| descriptor batch supervisor | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260522-04240-04241-closed/summary.json` | `PASS`, non-mutating descriptor profile |

## Descriptor closure

Descriptor `manual-admin-campaign-descriptor-20260522-04240-04241-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`다.
`manual_admin.latest_package_pair`는 설치본 ops summary/current-card에서 같은 descriptor id와
package pair `0.42.40-admin-smoke -> 0.42.41-admin-smoke`로 확인됐다.

## 경계

이 evidence는 internal manual-admin admin-smoke closure다. Public trusted signing, public
stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
