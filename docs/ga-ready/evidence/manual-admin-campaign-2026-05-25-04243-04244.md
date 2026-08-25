# Manual-admin campaign 2026-05-25 0.42.43 -> 0.42.44

evidence_id: `manual-admin-campaign-2026-05-25-04243-04244`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.43-admin-smoke -> 0.42.44-admin-smoke`
baseline_version: `0.42.43-admin-smoke`
target_version: `0.42.44-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260525-04243-04244`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260525-04243-04244-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260525-04243-04244/manual-admin-campaign-descriptor-windows-update/summary.json`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
runner_count: `6`
missing_count: `0`
not_pass_count: `0`
target_package_root: `artifacts/admin-smoke-package-20260525-04244`
target_msi_sha256: `eb9b6232a7c61431e2289850eecaba1c9a1d92bc93b88ce8eb4bd6f2ed3e8fe2`
baseline_msi_sha256: `38be93dd0d944e3657ea6fea2f3e0f922ab4577c09d57183b5be299de90297b1`
update_zip_sha256: `0af708044505c4d0661b30154914a908ebb77cf721eaaf14671cdc5c9b13c864`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260525-04244-r2`
clean_host_windows_update: `requested-and-pass`
burn_bundle_sha256: `2ca8fe93db12d56cbd300479945221ddd0bcaa79c0136dc73ac089fbfd89c76c`
msix_v1_sha256: `5bc8daebb529b8c44f319aef08ea35d53182edb69231488f1cc8cd6f8b58e0a6`
msix_v2_sha256: `6a08c531f60d965e476d60136d69e27fc757f0f0c6460807c461eb784faa5bf4`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 campaign은 `0.42.43-admin-smoke`를 baseline으로, PCVCLI read-only surface rendering
payload가 들어간 `0.42.44-admin-smoke`를 target으로 한 manual-admin package-pair
closure다. Readiness, installed update/rollback, dedicated clean-host Windows Update,
Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS했고
descriptor `missing_count=0`, `not_pass_count=0`으로 닫혔다.

## PASS bucket

| Bucket | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260525-04243-04244/manual-admin-rebaseline-readiness/summary.json` | `PASS`, installed baseline `0.42.43-admin-smoke`, target package ready |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260525-04243-04244/lifecycle/product-update-rollback/summary.json` | `PASS`, update `0.42.43 -> 0.42.44`, rollback `0.42.44 -> 0.42.43`, final update `0.42.44` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260525-04243-04244/clean-host-windows-update/summary.json` | `PASS`, KB5087545 적용, NoContact recovery 1회, blocker `none` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260525-04243-04244/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `2ca8fe93db12d56cbd300479945221ddd0bcaa79c0136dc73ac089fbfd89c76c` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260525-04243-04244/summary.json` | `PASS`, `0.42.43.0 -> 0.42.44.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260525-04243-04244/installed-runtime-ops-summary/summary.json` | `PASS`, installed runtime ops summary captured |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260525-04243-04244/manual-admin-campaign-descriptor-windows-update/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |

## Descriptor closure

Descriptor `manual-admin-campaign-descriptor-20260525-04243-04244-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`다.
설치본 ops summary/current-card는 같은 descriptor id와 package pair
`0.42.43-admin-smoke -> 0.42.44-admin-smoke`를 노출한다.

## 경계

이 evidence는 internal manual-admin admin-smoke closure다. Public trusted signing, public
stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
