# Manual-admin 캠페인 2026-05-20 0.42.38 -> 0.42.39

evidence_id: `manual-admin-campaign-2026-05-20-04238-04239`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.38-admin-smoke -> 0.42.39-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260520-04238-04239`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04238-04239-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260520-04238-04239/manual-admin-campaign-descriptor-r2-windows-update/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
descriptor_overall_status: `pass`
descriptor_missing_count: `0`
descriptor_not_pass_count: `0`
baseline_package_root: `artifacts/admin-smoke-package-20260520-04238`
target_package_root: `artifacts/admin-smoke-package-20260520-04239`
baseline_msi_sha256: `2ae739cee46780b01d1c3873d8186c30761243df578ecf7ab1e9d66a19f572b4`
target_msi_sha256: `b6fac120b145b5d0a8bf48a955037593756613d5bbe355bae96de59da4f0d805`
target_payload_aggregate_sha256: `359aee4c862fb4efc35a1dd631c92219e62e87adf7e96c8134d687fe38c7dede`
update_zip_sha256: `23c10a24e33ca706d7c89815b78c07b3a71a0ee94188c6d78ec188eca17ff9f4`
burn_bundle_sha256: `33d932f7b6b93239e4ce1db74a6c4681c281dfad06a11d780f9cbd2deda18699`
msix_v1_sha256: `43b124f62fa78882dda91fd5c23b1cf6a078c0d220a3ab6b22ccef934e83b61a`
msix_v2_sha256: `a49c7deea72d69f8901416c8285f1a602aca254bc99310daf242b4d476411a67`
target_provenance_commit: `6fd931baf3de77435d0d11b92424cf6657ea4515`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260520-04239`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04239.md`
host_mutation_performed: `true`
descriptor_host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.38-admin-smoke` baseline과 `0.42.39-admin-smoke` target package로
manual-admin package-pair campaign을 실행하고 Windows Update 적용 clean-host까지 포함해
closure로 전환한 기록이다. Readiness, installed update/rollback, dedicated clean-host,
Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다.

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260520-04238-04239/manual-admin-rebaseline-readiness-r2/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260520-04238-04239/lifecycle/product-update-rollback-r2/summary.json` | `PASS`, downshift `0.42.38`, update `0.42.39`, rollback `0.42.38`, final current `0.42.39` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260520-04238-04239/clean-host-windows-update/summary.json` | `PASS`, `KB5087545`, UBR `5139`, install/update/rollback exit `0`, blocker `none` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260520-04238-04239/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `33d932f7b6b93239e4ce1db74a6c4681c281dfad06a11d780f9cbd2deda18699` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260520-04238-04239/summary.json` | `PASS`, `0.42.38.0 -> 0.42.39.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260520-04238-04239/installed-runtime-ops-summary/summary.json` | `PASS` |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260520-04238-04239/manual-admin-campaign-descriptor-r2-windows-update/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |
| descriptor batch supervisor | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260520-04238-04239-closed/summary.json` | `PASS`, non-mutating descriptor profile |

## Clean-host Windows Update

Clean-host runner는 Windows Server 2022 eval base VHD `20348.169`에서 Windows Update를
적용했다. `KB5087545` 설치 후 UBR은 `5139`로 올라갔고, post-update PowerShell Direct
대기 중 heartbeat `NoContact` idle 상태가 922초 지속되어 자동 복구
`Stop-VM -TurnOff -Force; Start-VM`가 1회 수행됐다. 복구 후 baseline MSI install,
service health, catalog update, rollback, final Web console check가 모두 PASS했다.

## Descriptor 상태

Descriptor `manual-admin-campaign-descriptor-20260520-04238-04239-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`,
`missing_count=0`, `not_pass_count=0`로 닫혔다. Runtime/API current-card는 최신 closed
manual-admin package-pair를 `0.42.38-admin-smoke -> 0.42.39-admin-smoke`로 노출한다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
