# Manual-admin 캠페인 2026-05-21 0.42.39 -> 0.42.40

evidence_id: `manual-admin-campaign-2026-05-21-04239-04240`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.39-admin-smoke -> 0.42.40-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260521-04239-04240`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260521-04239-04240-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260521-04239-04240/manual-admin-campaign-descriptor-r2-windows-update/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
descriptor_overall_status: `pass`
descriptor_missing_count: `0`
descriptor_not_pass_count: `0`
baseline_package_root: `artifacts/admin-smoke-package-20260520-04239`
target_package_root: `artifacts/admin-smoke-package-20260521-04240`
baseline_msi_sha256: `b6fac120b145b5d0a8bf48a955037593756613d5bbe355bae96de59da4f0d805`
target_msi_sha256: `4979a3a60f96b8e8dbcda41bd722c33909c2faf39bc4cf88b8a79fb89e9628e8`
target_payload_aggregate_sha256: `0c5e566f49bd4ef5c78249b3439a4441462a3c6b54433985be4b9badb9618666`
update_zip_sha256: `96599dc4493e26e8cf467e19fabc5ab20306166896c1139bdbeb52566623ab25`
burn_bundle_sha256: `04eca236dc8bb4d4d60366192d5df7791b91518351e87a5febe97028e9276e34`
msix_v1_sha256: `6329f4cb6a1642757d8a1583195bde4c3992e72afa848dd228f29f1e6dd9dbaa`
msix_v2_sha256: `c0dc82c85864b6bc4a233d2f312ade9b5d40ba053503f61378088d5ce67fdd8e`
target_provenance_commit: `adb7b8c77ff60b64c5ac4d840e2bdfac62a3793a`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260521-04240`
installed_current_card: `not-rerun-in-this-slice`
host_mutation_performed: `true`
descriptor_host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.39-admin-smoke` baseline과 `0.42.40-admin-smoke` target package로
manual-admin package-pair campaign을 실행하고 Windows Update 적용 clean-host까지 포함해
closure로 전환한 기록이다. Readiness, installed update/rollback, dedicated clean-host,
Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다.

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260521-04239-04240/manual-admin-rebaseline-readiness-r2/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260521-04239-04240/lifecycle/product-update-rollback-r2/summary.json` | `PASS`, downshift `0.42.39`, update `0.42.40`, rollback `0.42.39`, final current `0.42.40` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260521-04239-04240/clean-host-windows-update/summary.json` | `PASS`, `KB5087545`, UBR `5139`, install/update/rollback exit `0`, blocker `none` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260521-04239-04240/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `04eca236dc8bb4d4d60366192d5df7791b91518351e87a5febe97028e9276e34` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260521-04239-04240/summary.json` | `PASS`, `0.42.39.0 -> 0.42.40.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260521-04239-04240/installed-runtime-ops-summary/summary.json` | `PASS`, installed version `0.42.40-admin-smoke` |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260521-04239-04240/manual-admin-campaign-descriptor-r2-windows-update/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |
| descriptor batch supervisor | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260521-04239-04240-closed/summary.json` | `PASS`, non-mutating descriptor profile |

## Clean-host Windows Update

Clean-host runner는 Windows Server 2022 eval base VHD `20348.169`에서 Windows Update를
적용했다. `KB5087545` 설치 후 UBR은 `5139`로 올라갔고, post-update PowerShell Direct
대기 중 heartbeat `NoContact` idle 상태가 922초 지속되어 자동 복구
`Stop-VM -TurnOff -Force; Start-VM`가 1회 수행됐다. 복구 후 baseline MSI install,
service health, catalog update, rollback, final Web console check가 모두 PASS했다.

## Descriptor 상태

Descriptor `manual-admin-campaign-descriptor-20260521-04239-04240-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`,
`missing_count=0`, `not_pass_count=0`로 닫혔다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다. 설치본
Web/TUI/CLI current-card smoke는 이 slice에서 별도로 재실행하지 않았다.
