# Manual-admin campaign 2026-05-29 0.42.57 -> 0.42.58

evidence_id: `manual-admin-campaign-2026-05-29-04257-04258`
result: `PASS`
scope: `internal-manual-admin-package-pair`
baseline_version: `0.42.57-admin-smoke`
target_version: `0.42.58-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260529-04257-04258`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260529-04257-04258-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260529-04257-04258/manual-admin-campaign-descriptor/summary.json`
target_msi_sha256: `6ae889eeb1b7134fab9618941748528f6260727abbc8ff36eee301b59dff6c0b`
baseline_msi_sha256: `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`
update_zip_sha256: `941190ac595db165c0ab7bc9d8c75c140208ae492780a8684dad19463913b16f`
burn_bundle_sha256: `97cc6292db711e6964a5a2e2fcea56620edd722c538510a672b840040f0eabc7`
msix_v1_sha256: `353c961c491d337554a330e6f4d9056865c32e3d994484b40647ecdda066be7e`
msix_v2_sha256: `c65decc2f98aa4fcc37494ea116c7a41d021210874cf1057053f18f9a4f9e90e`
windows_update_kb: `KB5087545`
windows_update_ubr: `5139`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` | `artifacts/manual-admin-campaign-20260529-04257-04258/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260529-04257-04258/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5087545`, UBR `5139` | `artifacts/manual-admin-campaign-20260529-04257-04258/clean-host-windows-update-r2/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260529-04257-04258/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260529-04257-04258/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260529-04257-04258/installed-runtime-ops-summary/summary.json` |

Descriptor `manual-admin-campaign-descriptor-20260529-04257-04258-closed`는
`runner_count=6`, `missing_count=0`, `not_pass_count=0`, `overall_status=pass`를 기록했다.
Clean-host runner는 Windows Update 후 `NoContact`/CPU idle recovery를 한 번 수행했고
baseline install, target update, rollback, final service health를 PASS로 닫았다.

## Installed Current-card 재확인

Closure 후 설치본 Web/TUI/CLI current-card smoke는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-29-04258.md`와
`artifacts/installed-operator-surface-current-card-20260529-04258/summary.json`에서 PASS로
재확인했다. Ops summary는 최신 full admin host mutation batch
`full-admin-host-mutation-gate-20260529-04258`와 최신 manual-admin descriptor
`manual-admin-campaign-descriptor-20260529-04257-04258-closed`를 표시한다.

## 경계

이 campaign은 internal admin-smoke manual-admin evidence다. Public trusted signing,
public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
이전 `0.42.56-admin-smoke -> 0.42.57-admin-smoke` campaign은 historical predecessor로
보존한다.
