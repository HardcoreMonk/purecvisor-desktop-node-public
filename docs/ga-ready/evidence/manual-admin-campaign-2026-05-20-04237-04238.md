# Manual-admin 캠페인 2026-05-20 0.42.37 -> 0.42.38

evidence_id: `manual-admin-campaign-2026-05-20-04237-04238`
result: `PASS`
scope: `manual-admin-package-pair-closure`
package_pair: `0.42.37-admin-smoke -> 0.42.38-admin-smoke`
campaign_root: `artifacts/manual-admin-campaign-20260520-04237-04238`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260520-04237-04238-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260520-04237-04238/manual-admin-campaign-descriptor-r2-windows-update/summary.json`
descriptor_schema_version: `2`
descriptor_generation_contract: `manual-admin-descriptor-generation-contract-v2`
descriptor_overall_status: `pass`
descriptor_missing_count: `0`
descriptor_not_pass_count: `0`
baseline_package_root: `artifacts/admin-smoke-package-20260520-04237`
target_package_root: `artifacts/admin-smoke-package-20260520-04238`
baseline_msi_sha256: `05dc31965af68792d21d919e19cb07997207d0514fd0ee39169d92129e95f67e`
target_msi_sha256: `2ae739cee46780b01d1c3873d8186c30761243df578ecf7ab1e9d66a19f572b4`
target_payload_aggregate_sha256: `40ec6157c99dffaf29bf9d0dcd1c513ba99fee77c21bb883976aa03eb3b73ca7`
update_zip_sha256: `122cc17bfda6a81915123ff29a8d8005acd3cbd8002137a7d953d29713dcfa39`
burn_bundle_sha256: `38ca3966a7eeb78b4bec9baed0b3a8106987bdeec67cd88be3596f211f7bc06d`
msix_v1_sha256: `0289c95f9a3ca54b380b20497e41585c46cd44253a78918a74abefaba29a619c`
msix_v2_sha256: `59c53f1d33d163cb3d020f46137f13e3cb736828e075457c95e5f15d61785fb3`
target_provenance_commit: `3c49b9a010c57e4a8637cb32ed17cd432dd0cd6f`
full_admin_host_mutation_batch: `full-admin-host-mutation-gate-20260520-04238`
installed_current_card: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04238.md`
host_mutation_performed: `true`
descriptor_host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 `0.42.37-admin-smoke` baseline과 `0.42.38-admin-smoke` target package로
manual-admin package-pair campaign을 실행하고, Windows Update 적용 clean-host rerun까지
포함해 closure로 전환한 기록이다. Readiness, installed update/rollback, dedicated
clean-host, Burn, MSIX, installed runtime ops summary, descriptor generation이 모두 PASS다.
최초 clean-host baseline MSI install `1603`은 RCA predecessor로 보존한다.

## Campaign 결과

| Gate | Artifact | 결과 |
| --- | --- | --- |
| rebaseline readiness | `artifacts/manual-admin-campaign-20260520-04237-04238/manual-admin-rebaseline-readiness-r2/summary.json` | `PASS`, `package_pair_input_status=ready-current-baseline-target-package-pair` |
| installed update/rollback lifecycle | `artifacts/manual-admin-campaign-20260520-04237-04238/lifecycle/product-update-rollback-r2/summary.json` | `PASS`, downshift `0.42.37`, update `0.42.38`, rollback `0.42.37`, final current `0.42.38` |
| dedicated clean-host initial | `artifacts/manual-admin-campaign-20260520-04237-04238/clean-host/summary.json` | `RCA predecessor`, blocker `guest smoke failed`, baseline MSI install exit `1603` |
| dedicated clean-host with Windows Update | `artifacts/manual-admin-campaign-20260520-04237-04238/clean-host-r2-windows-update/summary.json` | `PASS`, `KB5087545`, UBR `5139`, install/update/rollback exit `0`, blocker `none` |
| Burn bootstrapper lifecycle | `artifacts/manual-admin-campaign-20260520-04237-04238/burn-bootstrapper-lifecycle/summary.json` | `PASS`, bundle SHA-256 `38ca3966a7eeb78b4bec9baed0b3a8106987bdeec67cd88be3596f211f7bc06d` |
| MSIX lifecycle | `artifacts/msix-package-lifecycle-smoke-20260520-04237-04238/summary.json` | `PASS`, `0.42.37.0 -> 0.42.38.0`, install/update/remove 후 final package/service absent |
| installed runtime ops summary | `artifacts/manual-admin-campaign-20260520-04237-04238/installed-runtime-ops-summary/summary.json` | `PASS` |
| descriptor generation v2 | `artifacts/manual-admin-campaign-20260520-04237-04238/manual-admin-campaign-descriptor-r2-windows-update/summary.json` | `PASS`, `runner_count=6`, `missing_count=0`, `not_pass_count=0` |
| descriptor batch supervisor | `artifacts/batch-runs/manual-admin-campaign-descriptor-20260520-04237-04238-closed/summary.json` | `PASS`, non-mutating descriptor profile |

## Clean-host RCA

초기 dedicated clean-host guest summary는 `internal_clean_host_install_update_rollback_smoke=failed`와
`error.message=Baseline MSI install failed with exit code 1603.`을 기록했다. MSI log는
`ConfigureInstalled` custom action에서 actual error code `-2146233082`, Windows Installer
`Error 1722`, `InstallFinalize Return value 3`를 기록한다.

초기 runner plan은 Windows Server 2022 eval base VHD `20348.169`에서
`install_windows_updates=false`였다. 동일한 `0.42.37-admin-smoke` baseline MSI와
`0.42.38-admin-smoke` update package를 `clean-host-r2-windows-update`에서
`-InstallWindowsUpdates`로 재실행하자 Windows Update가 `KB5087545`를 적용하고 UBR을
`5139`로 올린 뒤 baseline MSI install, service health, update, rollback, final Web
console check가 모두 PASS했다. 따라서 1603/ConfigureInstalled는 product payload
regression이 아니라 outdated clean-host OS baseline blocker로 분류한다.

관련 artifact:

- `artifacts/manual-admin-campaign-20260520-04237-04238/clean-host/summary.json`
- `artifacts/manual-admin-campaign-20260520-04237-04238/clean-host/guest-outputs/guest-summary.json`
- `artifacts/manual-admin-campaign-20260520-04237-04238/clean-host/guest-outputs/baseline-msi-install.log`
- `artifacts/manual-admin-campaign-20260520-04237-04238/clean-host-r2-windows-update/summary.json`
- `artifacts/manual-admin-campaign-20260520-04237-04238/clean-host-r2-windows-update/guest-outputs/guest-summary.json`

Rerun VM은 success 후 제거했다. 이 RCA는 `0.42.38` full admin host mutation PASS를
유지하며 manual-admin package-pair closure 근거로 `clean-host-r2-windows-update`를
사용한다.

## Descriptor 상태

Descriptor `manual-admin-campaign-descriptor-20260520-04237-04238-closed`는 schema `2`,
contract `manual-admin-descriptor-generation-contract-v2`, `overall_status=pass`,
`missing_count=0`, `not_pass_count=0`로 닫혔다. Runtime/API current-card는 최신 closed
manual-admin package-pair를 `0.42.37-admin-smoke -> 0.42.38-admin-smoke`로 노출한다.

## 경계

이 campaign은 internal admin-smoke evidence다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
