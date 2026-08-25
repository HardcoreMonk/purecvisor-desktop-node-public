# MANUAL-ADMIN 캠페인 2026-05-17 04228->04229

evidence_id: `manual-admin-campaign-2026-05-17-04228-04229`
result: `PASS`
package_pair: `0.42.28-admin-smoke -> 0.42.29-admin-smoke`
baseline_version: `0.42.28-admin-smoke`
target_version: `0.42.29-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260517-04228-04229`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04228-04229-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260517-04228-04229/manual-admin-campaign-descriptor/summary.json`
descriptor_overall_status: `pass`
descriptor_missing_count: `0`
descriptor_not_pass_count: `0`
baseline_msi_sha256: `223a0023fc5d95e9c46e21471872d4bbc5e8e0cbda6c85925d6d598bf02f886e`
target_msi_sha256: `2abfec0cab616d9bc76c1f54d8343e6849bce66e6317baf76c59f7271fdc9b1d`
update_zip_sha256: `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542`
burn_bundle_sha256: `6cb6e84e8636c5a55c886125235be62fda2505e3a969c336b50c827f9e63b462`
msix_v1_sha256: `9a6c6a50bd9212e43dea2f0250387002cf00998f5969dc8c57697da2ca587c41`
msix_v2_sha256: `49131f6321a68050609bc377e782b99e80c1a190b9a78080d1229dfdaad12c79`
target_provenance_commit: `d306712ad671c8a00d5c560765b8952e24a07502`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.28-admin-smoke -> 0.42.29-admin-smoke` 내부
MANUAL-ADMIN package-pair campaign closure다. Readiness, installed product
update/rollback, Windows Update 포함 dedicated clean-host, Burn bootstrapper lifecycle,
MSIX lifecycle, installed runtime ops summary, descriptor generation v2, installed
Web/TUI/CLI current-card smoke가 모두 PASS였다.

## Artifact

| 항목 | 값 |
| --- | --- |
| campaign root | `artifacts/manual-admin-campaign-20260517-04228-04229` |
| baseline package root | `D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\04228-operator-surface-admin-smoke\artifacts\routeparity-service-msi-hyperv-batch-profile-20260517-04228` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260517-04229` |
| readiness summary | `artifacts/manual-admin-campaign-20260517-04228-04229/manual-admin-rebaseline-readiness/summary.json` |
| product lifecycle summary | `artifacts/manual-admin-campaign-20260517-04228-04229/lifecycle/product-update-rollback/summary.json` |
| clean-host summary | `artifacts/manual-admin-campaign-20260517-04228-04229/clean-host-updated-os/summary.json` |
| Burn summary | `artifacts/manual-admin-campaign-20260517-04228-04229/burn-bootstrapper-lifecycle/summary.json` |
| MSIX summary | `artifacts/msix-package-lifecycle-smoke-20260517-04228-04229/summary.json` |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260517-04228-04229/installed-runtime-ops-summary/summary.json` |
| installed current-card smoke | `artifacts/installed-operator-surface-current-card-20260517-04229/summary.json` |
| account login smoke | `artifacts/installed-account-login-smoke-20260517-04229/summary.json` |
| noVNC installed smoke | `artifacts/target-backed-novnc-installed-streaming-smoke-20260517-04229/summary.json` |
| descriptor summary | `artifacts/manual-admin-campaign-20260517-04228-04229/manual-admin-campaign-descriptor/summary.json` |

## PASS Bucket

| Bucket | 결과 | 핵심 값 |
| --- | --- | --- |
| readiness | `pass` | installed baseline `0.42.28-admin-smoke`, target `0.42.29-admin-smoke`, package-pair input `ready-current-baseline-target-package-pair` |
| installed update/rollback | `pass` | update `0.42.28 -> 0.42.29`, rollback `0.42.29 -> 0.42.28`, final manifest `0.42.29-admin-smoke`, update ZIP SHA-256 `3b399d92107c10f16f4788acafbcfe0a1174a92fd3329bd0f5789b8a1651f542` |
| clean-host install/update/rollback | `pass-with-windows-update` | Windows Update `KB5087545`, post update UBR `5139`, automatic recovery `true`, final rollback `0.42.28-admin-smoke`, Web `200`, blocker `none` |
| Burn install/repair/remove | `pass` | bundle SHA-256 `6cb6e84e8636c5a55c886125235be62fda2505e3a969c336b50c827f9e63b462`, install/repair/remove exit `0`, final restored service `Running` |
| MSIX build/install/update/remove | `pass` | `0.42.28.0 -> 0.42.29.0`, v1 SHA-256 `9a6c6a50bd9212e43dea2f0250387002cf00998f5969dc8c57697da2ca587c41`, v2 SHA-256 `49131f6321a68050609bc377e782b99e80c1a190b9a78080d1229dfdaad12c79` |
| installed runtime ops summary | `pass` | installed manifest `0.42.29-admin-smoke`, latest full gate `full-admin-host-mutation-gate-20260517-04229`, Host Ops bucket count `6` |
| descriptor generation | `pass` | `manual-admin-descriptor-generation-contract-v2`, schema version `2`, `missing_count=0`, `not_pass_count=0` |
| installed current-card smoke | `pass` | Web/TUI/CLI current-card latest batch `full-admin-host-mutation-gate-20260517-04229` |
| installed account/noVNC | `pass` | account login smoke와 target-backed noVNC installed streaming smoke 재확인, final service `Running`, token/password value not observed |

Dedicated clean-host VM `pcv-cleanhost-20260517-04228-04229`는 summary closure 후
`-RemoveVmOnSuccess`로 제거됐다. Windows Update reboot 이후 no-contact idle recovery는
runner summary의 `recovery_actions`에 기록되어 있으며 blocker는 `none`이다.

이 evidence는 internal admin-smoke 범위다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
