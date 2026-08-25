# Manual-admin campaign 2026-05-28 0.42.56 -> 0.42.57

evidence_id: `manual-admin-campaign-2026-05-28-04256-04257`
result: `PASS`
scope: `internal-manual-admin-package-pair`
baseline_version: `0.42.56-admin-smoke`
target_version: `0.42.57-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260528-04256-04257`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260528-04256-04257-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260528-04256-04257/manual-admin-campaign-descriptor/summary.json`
target_msi_sha256: `2eaa6fa9d22fcc72fad5994ebed397a2c3aead5a0311f32a3b9e013616b246f9`
baseline_msi_sha256: `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`
update_zip_sha256: `c50e846e51a568a184cd706dc71506cdad95d8248c4e89713f2f52b690236946`
burn_bundle_sha256: `a6d6f6d2378e57feafb6ca346464c08258a8822120458204f51570a2a96d0d04`
msix_v1_sha256: `6fa8eaefa49c7f5761b4f051ed8e30c055e7dfcfd5cd9f1b515cebc6eed5fea5`
msix_v2_sha256: `c6345a59f533af24abcdce33deab0e6d0f43f6da33accab72baa1ac44e36fa3b`
windows_update_kb: `KB5087545`
windows_update_ubr: `5139`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 0.42.56 public-boundary main push evidence를 operator current-card에 노출하는
`0.42.57-admin-smoke` payload를 manual-admin package-pair lifecycle까지 닫은 기록이다.

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` | `artifacts/manual-admin-campaign-20260528-04256-04257/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260528-04256-04257/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5087545`, UBR `5139` | `artifacts/manual-admin-campaign-20260528-04256-04257/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260528-04256-04257/burn-bootstrapper-lifecycle-r2/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260528-04256-04257/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260528-04256-04257/installed-runtime-ops-summary/summary.json` |

Descriptor `manual-admin-campaign-descriptor-20260528-04256-04257-closed`는
`runner_count=6`, `missing_count=0`, `not_pass_count=0`, `overall_status=pass`를 기록했다.
Clean-host runner는 Windows Update 후 heartbeat `NoContact`/CPU idle 상태에서
`automatic_recovery_performed=true`, `recovery_actions=1`로 한 번 recovery를 수행했고,
이후 PowerShell Direct 재접속, baseline install, target update, rollback, final service
health를 모두 PASS로 닫았다.

## Installed Current-card 재확인

Closure 후 설치본 Web/TUI/CLI current-card smoke는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04257.md`와
`artifacts/installed-operator-surface-current-card-20260528-04257/summary.json`에서 PASS로
재확인했다. Ops summary/CLI/TUI는 `public_boundary.latest_main_push` run `26578120570`과
head `7a7d5de822bdb058b04149eeeef0a7eb462828b5`를 표시한다.

## 경계

이 campaign은 internal admin-smoke manual-admin evidence다. Public trusted signing, public
stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
이전 `0.42.55-admin-smoke -> 0.42.56-admin-smoke` campaign은 historical predecessor로
보존한다.
