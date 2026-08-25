# MANUAL-ADMIN 캠페인 2026-05-17 04226->04227

evidence_id: `manual-admin-campaign-2026-05-17-04226-04227`
result: `PASS`
package_pair: `0.42.26-admin-smoke -> 0.42.27-admin-smoke`
baseline_version: `0.42.26-admin-smoke`
target_version: `0.42.27-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260517-04226-04227`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04226-04227-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260517-04226-04227/manual-admin-campaign-descriptor/summary.json`
descriptor_overall_status: `pass`
descriptor_missing_count: `0`
descriptor_not_pass_count: `0`
host_ops_lifecycle_descriptor_contract: `host-ops-lifecycle-descriptor-bridge-v1`
host_ops_lifecycle_bucket_count: `6`
host_ops_lifecycle_bucket_contract_key: `service-action-eventlog-firewall-truststore-credential-manager-data-root-separated`
baseline_msi_sha256: `f37d730edf3d7d587e2a46de196bb80069b5794cd9a1a6314ab71d56ca7812c7`
target_msi_sha256: `7921d7ecf31a5ba61ac55e74d2f3dbe14c88a40d15e715be75e284f641ef1ab9`
update_zip_sha256: `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997`
burn_bundle_sha256: `393022dcbcfbef4f3d7cd20dedd6fbfc164a399a833a9ecfb9d6cc1f7416a59c`
msix_v1_sha256: `6fefc3b89d99331020d2303bd68acdbda9a09e0dbf86bec016467df5f451496c`
msix_v2_sha256: `825ec3b86382e4c7cf191ef16f65c2cae11a05441707010bbd0989b16256265c`
target_provenance_commit: `69aba3eb3ff08c843f1a481818ddc86eac2f019b`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.26-admin-smoke -> 0.42.27-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign closure다. Readiness, installed update/rollback,
Windows Update 포함 dedicated clean-host, Burn lifecycle, MSIX lifecycle, installed
runtime ops summary, descriptor generation v2, installed Web/TUI/CLI current-card recheck가
모두 PASS였다.

## Artifact

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260517-04226-04227` |
| baseline package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260516-04226` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04227` |
| readiness summary | `artifacts/manual-admin-campaign-20260517-04226-04227/manual-admin-rebaseline-readiness/summary.json` |
| product lifecycle summary | `artifacts/manual-admin-campaign-20260517-04226-04227/lifecycle/product-update-rollback/summary.json` |
| clean-host summary | `artifacts/manual-admin-campaign-20260517-04226-04227/clean-host-updated-os/summary.json` |
| Burn summary | `artifacts/manual-admin-campaign-20260517-04226-04227/burn-bootstrapper-lifecycle/summary.json` |
| MSIX summary | `artifacts/msix-package-lifecycle-smoke-20260517-04226-04227/summary.json` |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260517-04226-04227/installed-runtime-ops-summary/summary.json` |
| current-card recheck | `artifacts/manual-admin-campaign-20260517-04226-04227/installed-runtime-ops-summary/current-card-recheck-after-docs/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260517-04226-04227/manual-admin-campaign-descriptor/summary.json` |

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| readiness | `pass` | installed baseline `0.42.26-admin-smoke`, target `0.42.27-admin-smoke`, package-pair input `ready-current-baseline-target-package-pair` |
| installed update/rollback | `pass` | update `0.42.26 -> 0.42.27`, rollback `0.42.27 -> 0.42.26`, final current `0.42.27-admin-smoke`, update ZIP SHA-256 `5c522c759f850a32b9cd7367f7059273dcf1357d1d3ae2f337542ce865daa997` |
| clean-host install/update/rollback | `pass-with-windows-update` | Windows Update reboot 후 `post-windows-update-heartbeat-no-contact-cpu-idle` automatic recovery 1회, final rollback `0.42.26-admin-smoke`, Web `200` |
| Burn install/repair/remove | `pass` | bundle SHA-256 `393022dcbcfbef4f3d7cd20dedd6fbfc164a399a833a9ecfb9d6cc1f7416a59c`, install/repair/remove exit `0`, final restored service `Running` |
| MSIX build/install/update/remove | `pass` | `0.42.26.0 -> 0.42.27.0`, v1 SHA-256 `6fefc3b89d99331020d2303bd68acdbda9a09e0dbf86bec016467df5f451496c`, v2 SHA-256 `825ec3b86382e4c7cf191ef16f65c2cae11a05441707010bbd0989b16256265c` |
| installed runtime ops summary | `pass` | installed manifest `0.42.27-admin-smoke`, Host Ops lifecycle descriptor bridge bucket count `6` |
| descriptor generation | `pass` | `manual-admin-descriptor-generation-contract-v2`, schema version `2`, `missing_count=0`, `not_pass_count=0` |
| installed current-card recheck | `pass` | latest batch `full-admin-host-mutation-gate-20260517-04227`, descriptor `manual-admin-campaign-descriptor-20260517-04226-04227-closed` |

Dedicated clean-host VM `pcv-cleanhost-20260517-04226-04227`는 summary closure 후 cleanup을
완료했고 `cleanup-summary.json`은 `ok=true`, `after_exists=false`를 기록한다.

이 evidence는 internal admin-smoke 범위다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
