# Manual-admin campaign 2026-08-20 0.42.73 -> 0.42.74

evidence_id: `manual-admin-campaign-2026-08-20-04273-04274`
result: `PASS`
scope: `manual-admin-package-pair-closure`
baseline_version: `0.42.73-admin-smoke`
target_version: `0.42.74-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260820-04273-04274`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260820-04273-04274-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260820-04273-04274/manual-admin-campaign-descriptor/summary.json`
baseline_msi_sha256: `03244819d1850bc9cd5cf01f1141091c41e95dce6208c7f82601f99e1cf69cee`
target_msi_sha256: `f4d0fcb75bc463676b831a4f871c402636039a7f1bbaf3780b24d10eceae1b8e`
update_zip_sha256: `cac208cacc9a773893e710b773ca56bc6b3fcd1e315b1d1a28a5099cee7f78f1`
burn_bundle_sha256: `7e06928f629143bbca85f8941b55ffaa21f0a26a217dccac9029001e55199b2d`
msix_v2_sha256: `906501de60f478be65b00b2c491da9c4efdc4adb001a26a22c8f198c4d7f4560`
host_mutation_performed: `true`
canonical_current_evidence: `0.42.74-admin-smoke`
canonical_current_changed: `true`
evidence_scope: `internal-admin-smoke-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` (`ready-current-baseline-target-package-pair`) | `artifacts/manual-admin-campaign-20260820-04273-04274/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260820-04273-04274/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5120242`, UBR `169 -> 5499` | `artifacts/manual-admin-campaign-20260820-04273-04274/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260820-04273-04274/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260820-04273-04274/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260820-04273-04274/installed-runtime-ops-summary/summary.json` |

Descriptor는 `runner_count=6`, `missing_count=0`, `not_pass_count=0`,
`overall_status=pass`로 닫혔다.

## 설치본 update/rollback

| 단계 | exit | 결과 버전 |
| --- | ---: | --- |
| baseline-align | `0` | `0.42.73-admin-smoke` |
| update | `0` | `0.42.74-admin-smoke` |
| rollback | `0` | `0.42.73-admin-smoke` |
| final update | `0` | `0.42.74-admin-smoke` |

최종 설치본은 `0.42.74-admin-smoke`다.

## Clean-host

throwaway VM `pcv-cleanhost-20260820-04273-04274`에서 Windows Update 적용 후 baseline
install, catalog update, rollback을 실행했다.

| 항목 | 값 |
| --- | --- |
| install / update / rollback exit | `0 / 0 / 0` |
| final Web | HTTP `200` |
| blocker | `none` |
| Windows Update | `KB5120242`, UBR `169 -> 5499` |
| automatic recovery | `true` (post-WU heartbeat `NoContact` recovery 1회) |
| final guest manifest | `0.42.73-admin-smoke` |
| VM cleanup | success 후 VM과 differencing VHD 제거 |

clean-host VM 디렉터리에 하위 항목 1개가 남았다. VM과 differencing VHD는 없다.
readback은 `vm_absent=true`, `diff_vhd_absent=true`로 PASS다.

Burn bundle은 install/repair/remove와 target MSI restore/native repair가 모두 exit `0`다.
MSIX는 `0.42.73.0` install, `0.42.74.0` update, remove 후 final package absent가
`true`다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- clean-host guest의 internal root certificate import는 수행되지 않았고 baseline MSI는
  `AllowUnsignedDev` 범위다.
- winget submission은 `out-of-scope`다.
- campaign target은 clean package MSI다. operational fullgate MSI hash는 별도다.
- SERVICE_PLAN P0 `vm.save` actual-VM FAIL는 이 pair가 고치지 않았고, 승격 후에도 열린 결함이다.
- canonical `current-evidence.json` 승격은 같은 날 ledger update가 소유한다.
