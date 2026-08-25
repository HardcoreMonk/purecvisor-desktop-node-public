# Manual-admin campaign 2026-05-20 0.42.34 -> 0.42.35

evidence_id: `manual-admin-campaign-2026-05-20-04234-04235`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.34-admin-smoke -> 0.42.35-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260520-04234-04235`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04234-04235-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260520-04234-04235/manual-admin-campaign-descriptor/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
baseline_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`
target_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260520-04235`
baseline_msi_sha256: `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`
target_msi_sha256: `12d05f2d783dfdb1db3f1596cd266af17578e33fca3f4fec272aac7df5e22697`
target_payload_aggregate_sha256: `ba966f3c41d81579dc6f065988c5fc015d47a9b0c8c77b4f4c3bf5962c1806a1`
update_zip_sha256: `71ccbe6188de9a52465beae9afc165f7777631bacbbc14a3137d0f9a6379994d`
burn_bundle_sha256: `20423c2c07226a3775a64e347b30fa27aca7135c8228ea0f07d379f80070bf27`
msix_v1_sha256: `912eb9d76eb517848e445507aed4f1fe8f0ccc8b01fdae85efdc744f711306f7`
msix_v2_sha256: `592c51cbb0c4cc3ea1804a469abe7df99a550181132f24e5a793f3c6c1405c56`
target_provenance_commit: `51a21d7c8612f598b85eeb58818ad3d61136c320`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260520-04235`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04237.md`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.34-admin-smoke` baseline과 `0.42.35-admin-smoke` target package로
installed update/rollback, dedicated clean-host with Windows Update, Burn, MSIX,
installed runtime ops summary, descriptor generation v2를 닫은 manual-admin package-pair
closure다.

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260520-04234-04235/manual-admin-rebaseline-readiness/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260520-04234-04235/lifecycle/product-update-rollback/summary.json` | `PASS`, before `0.42.34`, update `0.42.35`, rollback `0.42.34`, final current `0.42.35` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260520-04234-04235/clean-host-updated-os/summary.json` | `PASS`, `KB5087545`, UBR `5139`, blocker `none` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260520-04234-04235/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `20423c2c07226a3775a64e347b30fa27aca7135c8228ea0f07d379f80070bf27` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260520-04234-04235/summary.json` | `PASS`, `0.42.34.0 -> 0.42.35.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260520-04234-04235/installed-runtime-ops-summary/summary.json` | `PASS`, descriptor/current-card package-pair 노출 확인 |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260520-04234-04235/manual-admin-campaign-descriptor/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |
| descriptor batch supervisor | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260520-04234-04235-closed/summary.json` | `PASS`, non-mutating descriptor profile |

## Clean-host Windows Update

Dedicated clean-host run은 Windows Update `2026-05 Cumulative Update for Microsoft server
operating system version 21H2 for x64-based Systems (KB5087545)` 적용 후 UBR `5139`에서
install/update/rollback exit code `0`을 확인했다. Windows Update 이후 heartbeat
`NoContact`와 CPU idle 조건으로 automatic recovery가 1회 수행됐고, 최종 blocker는
`none`이다. Guest rollback 후 final guest manifest는 baseline `0.42.34-admin-smoke`였다.

## Descriptor closure

Descriptor `manual-admin-campaign-descriptor-20260520-04234-04235-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`,
`runner_count=6`, `missing_count=0`, `not_pass_count=0`로 닫혔다. Runtime/API current-card는
`manual_admin.latest_package_pair`를 `0.42.34-admin-smoke -> 0.42.35-admin-smoke`로 노출한다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
