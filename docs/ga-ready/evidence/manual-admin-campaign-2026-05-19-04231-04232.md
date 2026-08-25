# Manual-admin campaign 2026-05-19 0.42.31 -> 0.42.32

evidence_id: `manual-admin-campaign-2026-05-19-04231-04232`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.31-admin-smoke -> 0.42.32-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260519-04231-04232`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260519-04231-04232-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260519-04231-04232/manual-admin-campaign-descriptor/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
baseline_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231`
target_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04232`
baseline_msi_sha256: `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f`
target_msi_sha256: `3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9`
target_payload_aggregate_sha256: `21e2f8136ac53384bf86966e51f9040f7bbb37e62bc9e761640c0d1aeff35956`
update_zip_sha256: `c2e5c577d1a9bbec1ce6ca7ca2f79588d17b908d4aa639adb7968e5a09ce38da`
burn_bundle_sha256: `c50bbd8c682169698a5cfce633b387b7f9a0d90a203751168e237fba96e805ce`
msix_v1_sha256: `d83d347a8b1e96c632483f3c76a4e7a2f5f17715bc808d0462de0e7441ed4da3`
msix_v2_sha256: `7a37e36850e0dc6f0a6881a54f14f0e9fe6d64f87ec2dfae7e4127347bf1d1f1`
target_provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260519-04232`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04232.md`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.31-admin-smoke` baseline과 `0.42.32-admin-smoke` target package로
installed update/rollback, dedicated clean-host with Windows Update, Burn, MSIX,
installed runtime ops summary, descriptor generation, installed Web/TUI/CLI current-card를
닫은 manual-admin package-pair closure다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| baseline | `0.42.31-admin-smoke` |
| target | `0.42.32-admin-smoke` |
| baseline package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04231` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04232` |
| baseline MSI SHA-256 | `c03fab45ffec262ead1d4c41cb650a2c9b52c1030a5d7cbf461bd7c78a46499f` |
| target MSI SHA-256 | `3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9` |
| update ZIP SHA-256 | `c2e5c577d1a9bbec1ce6ca7ca2f79588d17b908d4aa639adb7968e5a09ce38da` |
| target provenance commit | `fc8cc284b7824172b8bf035858fb86b21bd26e5d` |

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260519-04231-04232/manual-admin-rebaseline-readiness/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260519-04231-04232/lifecycle/product-update-rollback/summary.json` | `PASS`, before `0.42.31`, update `0.42.32`, rollback `0.42.31`, final current `0.42.32` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260519-04231-04232/clean-host-updated-os/summary.json` | `PASS`, `KB5087545`, UBR `5139`, blocker `none`, automatic recovery `true` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260519-04231-04232/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `c50bbd8c682169698a5cfce633b387b7f9a0d90a203751168e237fba96e805ce` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260519-04231-04232/summary.json` | `PASS`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260519-04231-04232/installed-runtime-ops-summary/summary.json` | `PASS`, descriptor/current-card package-pair 노출 확인 |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260519-04231-04232/manual-admin-campaign-descriptor/summary.json` | `PASS`, `missing_count=0`, `not_pass_count=0` |
| installed Web/TUI/CLI current-card | `artifacts/installed-operator-surface-current-card-20260519-04232/summary.json` | `PASS`, Web `200`, config `200`, unauth runtime policy `401`, CLI/TUI 자동 token smoke PASS |

## Clean-host Windows Update

Dedicated clean-host run은 Windows Update `2026-05 Cumulative Update for Microsoft server
operating system version 21H2 for x64-based Systems (KB5087545)` 적용 후 UBR `5139`에서
install/update/rollback exit code `0`을 확인했다. Windows Update 이후 heartbeat
`NoContact`와 CPU idle 조건으로 automatic recovery가 1회 수행됐고, 최종 blocker는
`none`이며 성공 후 clean-host VM은 제거됐다. Guest rollback 후 final guest manifest는
baseline `0.42.31-admin-smoke`였고, host installed package는 target
`0.42.32-admin-smoke`로 복구했다.

## Installed current-card closure

설치본 current-card는 `full-admin-host-mutation-gate-20260519-04232` batch evidence를
`available`로 표시하고, `manual_admin.latest_package_pair`를
`0.42.31-admin-smoke -> 0.42.32-admin-smoke`로 표시했다. Descriptor batch id는
`manual-admin-campaign-descriptor-20260519-04231-04232-closed`이며 missing/not-pass count는
`0/0`이다. `pcvcli.exe`와 `pcvtui.exe`는 machine `PATH`에 등록된 product root에서
resolve 가능하고, current Codex 프로세스의 stale PATH 문제를 피하기 위해 smoke는
설치된 전체 경로를 사용했다. `pcvcli host status`, `pcvcli --json vm list`,
`pcvcli --json ops summary`, `pcvtui --smoke-once --no-color runtime`는 token 인자를
직접 전달하지 않고 PASS했다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
