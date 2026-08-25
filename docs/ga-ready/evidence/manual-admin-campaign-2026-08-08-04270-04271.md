# Manual-admin campaign 2026-08-08 0.42.70 -> 0.42.71

evidence_id: `manual-admin-campaign-2026-08-08-04270-04271`
result: `PASS`
scope: `manual-admin-package-pair-closure`
baseline_version: `0.42.70-admin-smoke`
target_version: `0.42.71-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260808-04270-04271`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260808-04270-04271-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260808-04270-04271/manual-admin-campaign-descriptor/summary.json`
baseline_msi_sha256: `b28e18763ac01137039a9bcfafe0c151945304c8449e307b0412038d6726c86c`
target_msi_sha256: `ebb621ada454b70ce367af6cc9a59e11966c0e2299b1f75976b03adacdd24ad5`
update_zip_sha256: `836f79c2448642a05840ad4380e872b5a60c0c505c83a33e1fea07110e61ebf4`
burn_bundle_sha256: `01031c9462c31eb499830386324c48793f8a1d76df4fa76df4f5cff21d0ac155`
msix_v2_sha256: `34d9bd579125238f07789712305dc7d5969de501a59d0251129874984fb5944a`
host_mutation_performed: `true`
evidence_scope: `internal-admin-smoke-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` (`ready-current-baseline-target-package-pair`) | `artifacts/manual-admin-campaign-20260808-04270-04271/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260808-04270-04271/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5099540`, UBR `169 -> 5386` | `artifacts/manual-admin-campaign-20260808-04270-04271/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260808-04270-04271/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260808-04270-04271/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260808-04270-04271/installed-runtime-ops-summary/summary.json` |

Descriptor `manual-admin-campaign-descriptor-20260808-04270-04271-closed`는
`runner_count=6`, `missing_count=0`, `not_pass_count=0`, `overall_status=pass`로 닫혔다.

## 설치본 update/rollback

호스트가 fullgate 이후 `0.42.71`이었으므로 먼저 baseline `0.42.70`으로 align한 뒤
`Update(0.42.71) → Rollback(0.42.70) → Update(0.42.71)`를 실행했다.

| 단계 | 결과 버전 |
| --- | --- |
| baseline-align | `0.42.70-admin-smoke` |
| update | `0.42.71-admin-smoke` |
| rollback | `0.42.70-admin-smoke` |
| final update | `0.42.71-admin-smoke` |

최종 설치본은 `0.42.71-admin-smoke`다.

## Clean-host

throwaway VM `pcv-cleanhost-20260808-04270-04271`에서 Windows Update 적용 후 baseline install,
catalog update, rollback을 실행했다.

| 항목 | 값 |
| --- | --- |
| `install_exit_code` | `0` |
| `update_exit_code` | `0` |
| `rollback_exit_code` | `0` |
| `final_web_status_code` | `200` |
| `blocker` | `none` |
| Windows Update | `KB5099540`, UBR `169 → 5386` |
| `automatic_recovery_performed` | `true` (post-WU heartbeat NoContact recovery) |
| final guest manifest | baseline `0.42.70-admin-smoke` |
| `RemoveVmOnSuccess` | VM 제거 |

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- clean-host guest의 internal root certificate import는 수행되지 않았고 baseline MSI는
  `AllowUnsignedDev` 범위다.
- winget submission은 `out-of-scope`다.
- operational full-gate MSI hash와 clean package MSI hash는 다를 수 있다. 이 campaign target
  MSI는 clean package `ebb621ada4…`를 사용했다.

이 evidence는 internal admin-smoke package-pair evidence이며 public trusted signing 또는
외부 stable publication을 주장하지 않는다.
