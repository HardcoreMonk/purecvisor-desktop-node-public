# Manual-admin campaign 2026-05-26 0.42.44 -> 0.42.45

evidence_id: `manual-admin-campaign-2026-05-26-04244-04245`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.44-admin-smoke -> 0.42.45-admin-smoke`
baseline_version: `0.42.44-admin-smoke`
target_version: `0.42.45-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260526-04244-04245`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04244-04245-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260526-04244-04245/manual-admin-campaign-descriptor-windows-update/summary.json`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
runner_count: `6`
missing_count: `0`
not_pass_count: `0`
target_package_root: `artifacts/admin-smoke-package-20260526-04245`
target_msi_sha256: `376218a0ee394e124f019e0e49a25718077585bac48f09c951da845bd96087bf`
baseline_msi_sha256: `eb9b6232a7c61431e2289850eecaba1c9a1d92bc93b88ce8eb4bd6f2ed3e8fe2`
update_zip_sha256: `08e526c3a7bccc3cdd53a1ea8d6e3917988cbb296ddfa2089aab49342fcd1641`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260526-04245`
clean_host_windows_update: `requested-and-pass`
burn_bundle_sha256: `ae06965980d7da96957ab2aa222da77f37f7ab9284894482d012f0ad174d8c79`
msix_v1_sha256: `556b54ffaa5fe5ef4cbd76f44f0aa75b21843c7b1f27da2bc1cfa87aba7da811`
msix_v2_sha256: `809044818cd07397982df88c945711f610ff6d832f38b0ba67de1ecc585438fe`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 campaign은 `0.42.44-admin-smoke`를 baseline으로, console access card/noVNC handoff
projection이 들어간 `0.42.45-admin-smoke`를 target으로 한 manual-admin package-pair
closure다. Readiness, installed update/rollback, dedicated clean-host Windows Update,
Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS했고
descriptor `missing_count=0`, `not_pass_count=0`으로 닫혔다.

## PASS bucket

| Bucket | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260526-04244-04245/manual-admin-rebaseline-readiness/summary.json` | `PASS`, installed baseline `0.42.44-admin-smoke`, target package ready |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260526-04244-04245/lifecycle/product-update-rollback/summary.json` | `PASS`, update `0.42.44 -> 0.42.45`, rollback `0.42.45 -> 0.42.44`, final update `0.42.45` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260526-04244-04245/clean-host-windows-update/summary.json` | `PASS`, Windows Update requested, reboot performed, blocker `none`, final rollback manifest `0.42.44-admin-smoke` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260526-04244-04245/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `ae06965980d7da96957ab2aa222da77f37f7ab9284894482d012f0ad174d8c79` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260526-04244-04245/summary.json` | `PASS`, `0.42.44.0 -> 0.42.45.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260526-04244-04245/installed-runtime-ops-summary/summary.json` | `PASS`, installed runtime ops summary captured |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260526-04244-04245/manual-admin-campaign-descriptor-windows-update/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |

## Descriptor closure

Descriptor `manual-admin-campaign-descriptor-20260526-04244-04245-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`다.
설치본 ops summary/current-card는 같은 descriptor id와 package pair
`0.42.44-admin-smoke -> 0.42.45-admin-smoke`를 노출한다.

## 경계

이 evidence는 internal manual-admin admin-smoke closure다. Public trusted signing, public
stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
