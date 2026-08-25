# Manual-admin campaign 2026-05-28 0.42.55 -> 0.42.56

evidence_id: `manual-admin-campaign-2026-05-28-04255-04256`
result: `PASS`
scope: `internal-manual-admin-package-pair`
baseline_version: `0.42.55-admin-smoke`
target_version: `0.42.56-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260528-04255-04256`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260528-04255-04256-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260528-04255-04256/manual-admin-campaign-descriptor/summary.json`
target_msi_sha256: `25f389ac183cd9f00c0223f4cca73c6ba3ff59397fe07dc24b19ea6bdfd440ae`
baseline_msi_sha256: `530d5605a99ff607a8030192a23fd4ba8bdb703793290b3e09e446dc61121627`
update_zip_sha256: `073a3d3d0a1e6ce6d4e09d2b66154ed957b42fe2bba6e30e4b101a9beac85a24`
burn_bundle_sha256: `f10204ab9e17a300c97b4e7e81e22a53ba5ca3db252a1bf7aff9b1bc48db729e`
msix_v1_sha256: `d61788ec1cdf794e02891b13ce583826f9bb09b3b87fb4684c3c9590889169bd`
msix_v2_sha256: `44db00ac736568b0de185711e099c2b109afddb4de97b2fcb6a5f163050c1e08`
windows_update_kb: `KB5087545`
windows_update_ubr: `5139`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Runtime/API ops summary의 manual-admin next package-pair projection과
Web/TUI/CLI current-card 표시 payload인 `0.42.56-admin-smoke`를 manual-admin package-pair
lifecycle까지 닫은 기록이다.

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` | `artifacts/manual-admin-campaign-20260528-04255-04256/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260528-04255-04256/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5087545`, UBR `5139` | `artifacts/manual-admin-campaign-20260528-04255-04256/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260528-04255-04256/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260528-04255-04256/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260528-04255-04256/installed-runtime-ops-summary/summary.json` |

Descriptor `manual-admin-campaign-descriptor-20260528-04255-04256-closed`는
`runner_count=6`, `missing_count=0`, `not_pass_count=0`, `overall_status=pass`를 기록했다.
Clean-host runner는 Windows Update 후 heartbeat `NoContact`/CPU idle 상태에서
`automatic_recovery_performed=true`, `recovery_actions=1`로 한 번 recovery를 수행했고,
이후 PowerShell Direct 재접속, baseline install, target update, rollback, final service
health를 모두 PASS로 닫았다.

## Installed Current-card 재확인

Closure 후 설치본 Web/TUI/CLI current-card smoke는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-28-04256.md`와
`artifacts/installed-operator-surface-current-card-20260528-04256/summary.json`에서 PASS로
재확인했다. Ops summary는 `next_package_pair`를 노출하고, CLI는
`current.manual_admin_next_package_pair`, TUI는 `MANUAL ADMIN NEXT`, Web은 `Manual admin next`를
표시한다.

## 경계

이 campaign은 internal admin-smoke manual-admin evidence다. Public trusted signing, public
stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
이전 `0.42.47-admin-smoke -> 0.42.48-admin-smoke` campaign은 historical predecessor로
보존한다.
