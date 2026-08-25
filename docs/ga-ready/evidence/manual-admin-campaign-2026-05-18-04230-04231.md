# Manual-admin campaign 2026-05-18 0.42.30 -> 0.42.31

evidence_id: `manual-admin-campaign-2026-05-18-04230-04231`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.30-admin-smoke -> 0.42.31-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260518-04230-04231`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260518-04230-04231-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260518-04230-04231/manual-admin-campaign-descriptor/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
baseline_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`
target_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`
baseline_msi_sha256: `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`
target_msi_sha256: `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`
target_payload_aggregate_sha256: `cea7d1f798e6f0889cf0cd02da049dc7d7b0131e8df51a768c12e02ea76c22f4`
update_zip_sha256: `de258c8f58ff8fd25ea78ea74483746c89190b3a7aa84345f3789eaa02458a44`
burn_bundle_sha256: `1d9240cd95c31a2ff5e7c87f50ed9dd0980465f0e8a8bc0638c681a84ce8bf4f`
msix_v1_sha256: `ff6bee8c19d23156d32140d3e51275e87cb93cfd786da8fa03e6be6545618f28`
msix_v2_sha256: `aec084a3337b8e8991947f2b4d4a3934e678341d8f7fca2c0e2c1d0c4792e4d1`
target_provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260518-04231`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04231.md`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.30-admin-smoke` baseline과 `0.42.31-admin-smoke` target package로
installed update/rollback, dedicated clean-host with Windows Update, Burn, MSIX,
installed runtime ops summary, descriptor generation, installed Web/TUI/CLI current-card를
닫은 manual-admin package-pair closure다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| baseline | `0.42.30-admin-smoke` |
| target | `0.42.31-admin-smoke` |
| baseline package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231` |
| baseline MSI SHA-256 | `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86` |
| target MSI SHA-256 | `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f` |
| update ZIP SHA-256 | `de258c8f58ff8fd25ea78ea74483746c89190b3a7aa84345f3789eaa02458a44` |
| target provenance commit | `fc8cc284b7824172b8bf035858fb86b21bd26e5d` |

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260518-04230-04231/manual-admin-rebaseline-readiness/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260518-04230-04231/lifecycle/product-update-rollback/summary.json` | `PASS`, before `0.42.30`, update `0.42.31`, rollback `0.42.30`, final current `0.42.31` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260518-04230-04231/clean-host-updated-os/summary.json` | `PASS`, `KB5087545`, UBR `5139`, blocker `none`, automatic recovery `true` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260518-04230-04231/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `1d9240cd95c31a2ff5e7c87f50ed9dd0980465f0e8a8bc0638c681a84ce8bf4f` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260518-04230-04231/summary.json` | `PASS`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260518-04230-04231/installed-runtime-ops-summary/summary.json` | `PASS`, descriptor/current-card package-pair 노출 확인 |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260518-04230-04231/manual-admin-campaign-descriptor/summary.json` | `PASS`, `missing_count=0`, `not_pass_count=0` |
| installed Web/TUI/CLI current-card | `artifacts/installed-operator-surface-current-card-20260518-04231/summary.json` | `PASS`, Web `200`, config `200`, unauth runtime policy `401`, CLI/TUI 자동 token smoke PASS |

## Clean-host Windows Update

Dedicated clean-host run은 Windows Update `2026-05 Cumulative Update for Microsoft server
operating system version 21H2 for x64-based Systems (KB5087545)` 적용 후 UBR `5139`에서
install/update/rollback exit code `0`을 확인했다. Windows Update 이후 heartbeat
`NoContact`와 CPU idle 조건으로 automatic recovery가 1회 수행됐고, 최종 blocker는
`none`이며 성공 후 clean-host VM은 제거됐다. Guest rollback 후 final guest manifest는
baseline `0.42.30-admin-smoke`였고, host installed package는 target
`0.42.31-admin-smoke`로 복구했다.

## Installed current-card closure

설치본 current-card는 `full-admin-host-mutation-gate-20260518-04231` batch evidence를
`available`로 표시하고, `manual_admin.latest_package_pair`를
`0.42.30-admin-smoke -> 0.42.31-admin-smoke`로 표시했다. Descriptor batch id는
`manual-admin-campaign-descriptor-20260518-04230-04231-closed`이며 missing/not-pass count는
`0/0`이다. `pcvcli.exe`와 `pcvtui.exe`는 machine `PATH`에서 resolve됐고,
`pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
`pcvtui --smoke-once --no-color runtime`는 token 인자를 직접 전달하지 않고 PASS했다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
