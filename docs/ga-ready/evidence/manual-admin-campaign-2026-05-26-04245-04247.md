# Manual-admin campaign 2026-05-26 0.42.45 -> 0.42.47

evidence_id: `manual-admin-campaign-2026-05-26-04245-04247`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.45-admin-smoke -> 0.42.47-admin-smoke`
baseline_version: `0.42.45-admin-smoke`
target_version: `0.42.47-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260526-04245-04247`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04245-04247-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260526-04245-04247/manual-admin-campaign-descriptor-windows-update/summary.json`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
runner_count: `6`
missing_count: `0`
not_pass_count: `0`
target_package_root: `artifacts/admin-smoke-package-20260526-04247`
target_msi_sha256: `9589086d092ee902b72ff7790cac5a25e6d806cdaac0d98e431a27048dc5e197`
baseline_msi_sha256: `376218a0ee394e124f019e0e49a25718077585bac48f09c951da845bd96087bf`
update_zip_sha256: `69fda75fc32a187364ac870dac01118bc4c548bebfe596660a5cd70085610a0d`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260526-04247`
clean_host_windows_update: `requested-and-pass`
burn_bundle_sha256: `a46baa2908ec5a29d7b70ecac220102655ec654e1feb4533fb675b7548246701`
msix_v1_sha256: `0416a37f0e64afa87db43023f7ab41000e33d3c2527823f9ada94a8dd8bc1108`
msix_v2_sha256: `5cb63d5ce53259875f90f3d00d0df4a2ccd83d87910672a6fe379212a9278e89`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 campaign은 `0.42.45-admin-smoke`를 baseline으로, Hyper-V QoS mutation payload가 들어간
`0.42.47-admin-smoke`를 target으로 한 manual-admin package-pair closure다. Readiness,
installed update/rollback, dedicated clean-host Windows Update, Burn, MSIX, installed
runtime ops summary, descriptor generation이 모두 PASS했고 descriptor `missing_count=0`,
`not_pass_count=0`으로 닫혔다.

## PASS bucket

| Bucket | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260526-04245-04247/manual-admin-rebaseline-readiness/summary.json` | `PASS`, installed baseline `0.42.45-admin-smoke`, target package ready |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260526-04245-04247/lifecycle/product-update-rollback/summary.json` | `PASS`, update `0.42.45 -> 0.42.47`, rollback `0.42.47 -> 0.42.45`, final update `0.42.47` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260526-04245-04247/clean-host-windows-update-corrected/summary.json` | `PASS`, Windows Update requested, reboot performed, automatic no-contact recovery performed, blocker `none`, final rollback manifest `0.42.45-admin-smoke` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260526-04245-04247/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `a46baa2908ec5a29d7b70ecac220102655ec654e1feb4533fb675b7548246701` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260526-04245-04247/summary.json` | `PASS`, `0.42.45.0 -> 0.42.47.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260526-04245-04247/installed-runtime-ops-summary/summary.json` | `PASS`, installed runtime ops summary captured |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260526-04245-04247/manual-admin-campaign-descriptor-windows-update/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |

## Descriptor closure

Descriptor `manual-admin-campaign-descriptor-20260526-04245-04247-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`다.
설치본 ops summary/current-card는 같은 descriptor id와 package pair
`0.42.45-admin-smoke -> 0.42.47-admin-smoke`를 노출한다.

## 경계

이 evidence는 internal manual-admin admin-smoke closure다. Public trusted signing, public
stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
