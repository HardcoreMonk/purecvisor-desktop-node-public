# MANUAL-ADMIN 캠페인 2026-05-17 04227->04228

evidence_id: `manual-admin-campaign-2026-05-17-04227-04228`
result: `PASS`
package_pair: `0.42.27-admin-smoke -> 0.42.28-admin-smoke`
baseline_version: `0.42.27-admin-smoke`
target_version: `0.42.28-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260517-04227-04228`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04227-04228-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260517-04227-04228/manual-admin-campaign-descriptor/summary.json`
descriptor_overall_status: `pass`
descriptor_missing_count: `0`
descriptor_not_pass_count: `0`
baseline_msi_sha256: `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`
target_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
update_zip_sha256: `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c`
burn_bundle_sha256: `9732074563d71344f6a8d19216134510f049844c2bb6eda28cc79520a4a4d37b`
msix_v1_sha256: `ba5b817276ea201e7010374bfef7ae126d7ad5388d52798027227e82c07291b3`
msix_v2_sha256: `e4bd703170c881400bb88f728bb44a9cf410957c9a53d35db729e16267232b8e`
target_provenance_commit: `b9676f6dc37d667ae0d60367e9f4e576a27e3864`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.27-admin-smoke -> 0.42.28-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign closure다. Readiness, installed product
update/rollback, Windows Update 포함 dedicated clean-host, Burn bootstrapper lifecycle,
MSIX lifecycle, installed runtime ops summary, descriptor generation v2, installed
Web/TUI/CLI current-card recheck가 모두 PASS였다.

## Artifact

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260517-04227-04228` |
| baseline package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04227` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04228` |
| readiness summary | `artifacts/manual-admin-campaign-20260517-04227-04228/manual-admin-rebaseline-readiness/summary.json` |
| product lifecycle summary | `artifacts/manual-admin-campaign-20260517-04227-04228/lifecycle/product-update-rollback/summary.json` |
| clean-host summary | `artifacts/manual-admin-campaign-20260517-04227-04228/clean-host-updated-os/summary.json` |
| Burn summary | `artifacts/manual-admin-campaign-20260517-04227-04228/burn-bootstrapper-lifecycle/summary.json` |
| MSIX summary | `artifacts/msix-package-lifecycle-smoke-20260517-04227-04228/summary.json` |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260517-04227-04228/installed-runtime-ops-summary/summary.json` |
| current-card recheck | `artifacts/manual-admin-campaign-20260517-04227-04228/installed-runtime-ops-summary/current-card-recheck-after-descriptor/summary.json` |
| account login smoke | `artifacts/installed-account-login-smoke-20260517-04228-packagepair/summary.json` |
| noVNC installed smoke | `artifacts/target-backed-novnc-installed-streaming-smoke-20260517-04228-packagepair/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260517-04227-04228/manual-admin-campaign-descriptor/summary.json` |

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| readiness | `pass` | installed baseline `0.42.27-admin-smoke`, target `0.42.28-admin-smoke`, package-pair input `ready-current-baseline-target-package-pair` |
| installed update/rollback | `pass` | update `0.42.27 -> 0.42.28`, rollback `0.42.28 -> 0.42.27`, final host restore `0.42.28-admin-smoke`, update ZIP SHA-256 `e54a7104a20b3a2b2dc8b6e34f38d9b829ba123ea10d0850439711453e57ac3c` |
| clean-host install/update/rollback | `pass-with-windows-update` | Windows Update `KB5087545`, post update UBR `5139`, `post-windows-update-heartbeat-no-contact-cpu-idle` automatic recovery 1회, final rollback `0.42.27-admin-smoke`, Web `200` |
| Burn install/repair/remove | `pass` | bundle SHA-256 `9732074563d71344f6a8d19216134510f049844c2bb6eda28cc79520a4a4d37b`, install/repair/remove exit `0`, final restored service `Running` |
| MSIX build/install/update/remove | `pass` | `0.42.27.0 -> 0.42.28.0`, v1 SHA-256 `ba5b817276ea201e7010374bfef7ae126d7ad5388d52798027227e82c07291b3`, v2 SHA-256 `e4bd703170c881400bb88f728bb44a9cf410957c9a53d35db729e16267232b8e` |
| installed runtime ops summary | `pass` | installed manifest `0.42.28-admin-smoke`, latest full gate `full-admin-host-mutation-gate-20260517-04228`, Host Ops bucket count `6` |
| descriptor generation | `pass` | `manual-admin-descriptor-generation-contract-v2`, schema version `2`, `missing_count=0`, `not_pass_count=0` |
| installed current-card recheck | `pass` | descriptor `manual-admin-campaign-descriptor-20260517-04227-04228-closed`, package pair `0.42.27-admin-smoke -> 0.42.28-admin-smoke` |
| installed account/noVNC | `pass` | account login smoke와 target-backed noVNC installed streaming smoke 재확인, final service `Running`, token/password value not observed |

Dedicated clean-host VM `pcv-cleanhost-20260517-04227-04228`는 summary closure 후
`-RemoveVmOnSuccess`로 제거됐다. Windows Update reboot 이후 `NoContact` idle recovery는
runner summary의 `recovery_actions`에 기록되어 있으며 blocker는 `none`이다.

이 evidence는 internal admin-smoke 범위다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
