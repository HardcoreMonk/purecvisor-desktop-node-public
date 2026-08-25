# Manual-admin campaign 2026-05-19 0.42.32 -> 0.42.34

evidence_id: `manual-admin-campaign-2026-05-19-04232-04234`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.32-admin-smoke -> 0.42.34-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260519-04232-04234`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260519-04232-04234-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260519-04232-04234/manual-admin-campaign-descriptor/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
baseline_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04232`
target_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234`
baseline_msi_sha256: `3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9`
target_msi_sha256: `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78`
target_payload_aggregate_sha256: `a11b63d5daf36f5b61c89b961a19d44a099f98a53b1aedae1bec6a264a9120e5`
update_zip_sha256: `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad`
burn_bundle_sha256: `6d379bf63aef4729871dc89437096eb0bd35800ec220a629da753e5e09fcda79`
msix_v1_sha256: `c97954dac6cf6e716a9d32203e30a7d37d0bad4d7300eefcaaf7171ac626613c`
msix_v2_sha256: `5982680b03d48324be85e82011dc3bd21e6dc7a33241f503aef6d125f31c75c6`
target_provenance_commit: `fc8cc284b7824172b8bf035858fb86b21bd26e5d`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260519-04234`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-19-04234.md`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.32-admin-smoke` baseline과 `0.42.34-admin-smoke` target package로
installed update/rollback, dedicated clean-host with Windows Update, Burn, MSIX,
installed runtime ops summary, descriptor generation, installed Web/TUI/CLI current-card를
닫은 manual-admin package-pair closure다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| baseline | `0.42.32-admin-smoke` |
| target | `0.42.34-admin-smoke` |
| baseline package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04232` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260519-04234` |
| baseline MSI SHA-256 | `3a6d0a2140840ff52c924c8294fe0266c4ce4c5a6e08738db32b578bf35b51d9` |
| target MSI SHA-256 | `aec956b47c68ad87b33101bf5ffe61ab9dd2f1cfed6d7b216f44f6258b9d8f78` |
| update ZIP SHA-256 | `da773bed215984f28523f869f71c7dffe7f4c584667b8817506c2442e2a473ad` |
| target provenance commit | `fc8cc284b7824172b8bf035858fb86b21bd26e5d` |

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260519-04232-04234/manual-admin-rebaseline-readiness/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260519-04232-04234/lifecycle/product-update-rollback/summary.json` | `PASS`, before `0.42.32`, update `0.42.34`, rollback `0.42.32`, final current `0.42.34` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260519-04232-04234/clean-host-updated-os/summary.json` | `PASS`, `KB5087545`, UBR `5139`, blocker `none`, automatic recovery `true` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260519-04232-04234/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `6d379bf63aef4729871dc89437096eb0bd35800ec220a629da753e5e09fcda79` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260519-04232-04234/summary.json` | `PASS`, `0.42.32.0 -> 0.42.34.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260519-04232-04234/installed-runtime-ops-summary/summary.json` | `PASS`, descriptor/current-card package-pair 노출 확인 |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260519-04232-04234/manual-admin-campaign-descriptor/summary.json` | `PASS`, `missing_count=0`, `not_pass_count=0` |
| descriptor batch supervisor | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260519-04232-04234-closed/summary.json` | `PASS`, non-mutating descriptor profile |
| installed Web/TUI/CLI current-card | `artifacts/installed-operator-surface-current-card-20260519-04234/summary.json` | `PASS`, Web `200`, config `200`, unauth runtime policy `401`, CLI/TUI 자동 token smoke PASS |

## Clean-host Windows Update

Dedicated clean-host run은 Windows Update `2026-05 Cumulative Update for Microsoft server
operating system version 21H2 for x64-based Systems (KB5087545)` 적용 후 UBR `5139`에서
install/update/rollback exit code `0`을 확인했다. Windows Update 이후 heartbeat
`NoContact`와 CPU idle 조건으로 automatic recovery가 1회 수행됐고, 최종 blocker는
`none`이며 성공 후 clean-host VM은 제거됐다. Guest rollback 후 final guest manifest는
baseline `0.42.32-admin-smoke`였고, host installed package는 target
`0.42.34-admin-smoke`로 복구했다.

## Installed current-card closure

설치본 current-card는 `full-admin-host-mutation-gate-20260519-04234` batch evidence를
`available`로 표시하고, `manual_admin.latest_package_pair`를
`0.42.32-admin-smoke -> 0.42.34-admin-smoke`로 표시했다. Descriptor batch id는
`manual-admin-campaign-descriptor-20260519-04232-04234-closed`이며 missing/not-pass count는
`0/0`이다. `pcvcli.exe`와 `pcvtui.exe`는 machine `PATH`에 등록된 product root에서
resolve 가능하고, `pcvcli --interactive`는 Linux-style neon palette와 한 줄 command row를
설치본 기준으로 확인했다. `pcvcli host status`, `pcvcli --json vm list`,
`pcvcli --json ops summary`, `pcvtui --smoke-once --no-color runtime`는 token 인자를
직접 전달하지 않고 PASS했다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
