# Manual-admin campaign 2026-05-18 0.42.29 -> 0.42.30

evidence_id: `manual-admin-campaign-2026-05-18-04229-04230`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.29-admin-smoke -> 0.42.30-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260518-04229-04230`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260518-04229-04230-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260518-04229-04230/manual-admin-campaign-descriptor/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
baseline_rebuild_root: `.worktrees/04229-baseline-rebuild/artifacts/admin-smoke-package-rebuild-20260518-04229`
target_package_root: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230`
baseline_msi_sha256: `53da07c3032edee5dc8fef7127be3293de355fa0d07bd96edcf6109c5039bcbc`
target_msi_sha256: `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86`
target_payload_aggregate_sha256: `0fddc06c7ced0239ea04a89fd90cc0c152a64688904e0f58b97c3fcd5368a28c`
update_zip_sha256: `f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2`
burn_bundle_sha256: `d03a1cfb36e54d92058383bdd9bb09c2e931360cc66fe7b385c2e40b4206373d`
msix_v1_sha256: `79fd8da38a550e288e0f0f3d8e9c7eea36e1d0c6b3236f37d15acf70e6acef2f`
msix_v2_sha256: `c765062a88537ce838cf443b0ba238bd2d35f677953219fdd985b5daab96f481`
target_provenance_commit: `f4349cf049db66b0ae1d5d38a948a6b03a8b0648`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260518-04230`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-18-04230.md`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.29-admin-smoke` 기준 baseline을 재구성한 뒤
`0.42.30-admin-smoke` target package로 installed update/rollback, dedicated clean-host,
Burn, MSIX, installed runtime ops summary, descriptor generation, installed Web/TUI/CLI
current-card를 닫은 manual-admin package-pair closure다.

## Package Pair

| 항목 | 값 |
| --- | --- |
| baseline | `0.42.29-admin-smoke` |
| target | `0.42.30-admin-smoke` |
| baseline package root | `.worktrees/04229-baseline-rebuild/artifacts/admin-smoke-package-rebuild-20260518-04229` |
| target package root | `artifacts/routeparity-service-msi-hyperv-batch-profile-20260518-04230` |
| baseline MSI SHA-256 | `53da07c3032edee5dc8fef7127be3293de355fa0d07bd96edcf6109c5039bcbc` |
| target MSI SHA-256 | `90b59f34ad58e0d7ad2890ea4ea464ded94923759aa9435d3fbfc4c0d1873c86` |
| update ZIP SHA-256 | `f9739db9f25622a6dc61ef9c7e00e5ba07f2c8b9020308ecfe7587162175a9c2` |
| target provenance commit | `f4349cf049db66b0ae1d5d38a948a6b03a8b0648` |

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260518-04229-04230/manual-admin-rebaseline-readiness/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260518-04229-04230/lifecycle/product-update-rollback/summary.json` | `PASS`, before `0.42.29`, update `0.42.30`, rollback `0.42.29`, final current `0.42.30` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260518-04229-04230/clean-host-updated-os/summary.json` | `PASS`, `KB5087545`, UBR `5139`, blocker `none`, automatic recovery `true` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260518-04229-04230/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `d03a1cfb36e54d92058383bdd9bb09c2e931360cc66fe7b385c2e40b4206373d` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260518-04229-04230/summary.json` | `PASS`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260518-04229-04230/installed-runtime-ops-summary/summary.json` | `PASS`, descriptor/current-card package-pair 노출 확인 |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260518-04229-04230/manual-admin-campaign-descriptor/summary.json` | `PASS`, `missing_count=0`, `not_pass_count=0` |
| installed Web/TUI/CLI current-card | `artifacts/installed-operator-surface-current-card-20260518-04230-r2/summary.json` | `PASS`, Web `200`, config `200`, unauth runtime policy `401`, CLI/TUI 자동 token smoke PASS |

## Clean-host Windows Update

Dedicated clean-host run은 Windows Update `2026-05 Cumulative Update for Microsoft server
operating system version 21H2 for x64-based Systems (KB5087545)` 적용 후 UBR `5139`에서
install/update/rollback exit code `0`을 확인했다. Windows Update 이후 heartbeat
`NoContact`와 CPU idle 조건으로 automatic recovery가 1회 수행됐고, 최종 blocker는
`none`이며 성공 후 clean-host VM은 제거됐다.

## Installed current-card closure

설치본 current-card는 `full-admin-host-mutation-gate-20260518-04230` batch evidence를
`available`로 표시하고, `manual_admin.latest_package_pair`를
`0.42.29-admin-smoke -> 0.42.30-admin-smoke`로 표시했다. Descriptor batch id는
`manual-admin-campaign-descriptor-20260518-04229-04230-closed`이며 missing/not-pass count는
`0/0`이다. `pcvcli.exe`와 `pcvtui.exe`는 machine `PATH`에서 resolve됐고,
`pcvcli host status`, `pcvcli --json vm list`, `pcvcli --json ops summary`,
`pcvtui --smoke-once --no-color runtime`는 token 인자를 직접 전달하지 않고 PASS했다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
