# Manual-admin campaign 2026-08-27 0.42.74 -> 0.42.75

evidence_id: `manual-admin-campaign-2026-08-27-04274-04275`
result: `PASS`
scope: `manual-admin-package-pair-closure`
baseline_version: `0.42.74-admin-smoke`
target_version: `0.42.75-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260827-04274-04275`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260827-04274-04275`
descriptor_summary: `artifacts/manual-admin-campaign-20260827-04274-04275/manual-admin-campaign-descriptor/summary.json`
baseline_msi_sha256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
target_msi_sha256: `3d3ee255f7a16c90715da27c436a9ebce479b5ae91f1f4a7067a47dc6dbc0fb6`
update_zip_sha256: `ecae6e9fc7f2f3c49e12a7fec5b4e6d7ca0ce8ba017adf7970cb516a7b5e15df`
burn_bundle_sha256: `e0cd30dd697372c46f0a40282dae44789a9604e8528b9937b836fccee9faa2f8`
msix_v2_sha256: `c2a068ce5c4218341db88839f19344366dd25d966f0dcd0e2cd755f362b06a28`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.75-admin-smoke`
canonical_current_changed: `true`
evidence_scope: `internal-admin-smoke-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` (`ready-current-baseline-target-package-pair`) | `artifacts/manual-admin-campaign-20260827-04274-04275/manual-admin-rebaseline-readiness-after-align/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260827-04274-04275/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5120242`, UBR `169 -> 5499` | `artifacts/manual-admin-campaign-20260827-04274-04275/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260827-04274-04275/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/manual-admin-campaign-20260827-04274-04275/msix-package-lifecycle-smoke/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260827-04274-04275/installed-runtime-ops-summary/summary.json` |

Descriptor는 `runner_count=6`, `missing_count=0`, `not_pass_count=0`,
`overall_status=pass`로 닫혔다.

## 설치본 update/rollback

| 단계 | exit | 결과 버전 |
| --- | ---: | --- |
| update | `0` | `0.42.75-admin-smoke` |
| rollback | `0` | `0.42.74-admin-smoke` |
| final update | `0` | `0.42.75-admin-smoke` |

최종 설치본은 `0.42.75-admin-smoke`다.

## Clean-host

throwaway VM `pcv-cleanhost-20260827-04274-04275`에서 Windows Update 적용 후 baseline
install, catalog update, rollback을 실행했다.

| 항목 | 값 |
| --- | --- |
| install / update / rollback exit | `0 / 0 / 0` |
| final Web | HTTP `200` |
| blocker | `none` |
| Windows Update | `KB5120242`, UBR `169 -> 5499` |
| automatic recovery | `true` (post-WU heartbeat `NoContact` recovery 1회) |
| final guest manifest | `0.42.74-admin-smoke` |
| VM name | `pcv-cleanhost-20260827-04274-04275` |

Burn bundle은 install/repair/remove와 target MSI restore/native repair가 모두 exit `0`다.
MSIX는 `0.42.74.0` install, `0.42.75.0` update, remove 후 final package absent가
`true`다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- clean-host guest의 internal root certificate import는 수행되지 않았고 baseline MSI는
  `AllowUnsignedDev` 범위다.
- winget submission은 `out-of-scope`다.
- campaign target은 clean package MSI다. operational fullgate MSI hash는 별도다.
- canonical `current-evidence.json` 승격은 같은 Lane 3 ledger update가 소유한다.
