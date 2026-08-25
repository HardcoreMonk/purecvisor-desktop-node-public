# Manual-admin campaign 2026-08-09 0.42.71 -> 0.42.72

evidence_id: `manual-admin-campaign-2026-08-09-04271-04272`
result: `PASS`
scope: `manual-admin-package-pair-closure`
baseline_version: `0.42.71-admin-smoke`
target_version: `0.42.72-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260809-04271-04272`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260809-04271-04272-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260809-04271-04272/manual-admin-campaign-descriptor/summary.json`
baseline_msi_sha256: `ebb621ada454b70ce367af6cc9a59e11966c0e2299b1f75976b03adacdd24ad5`
target_msi_sha256: `142a9e3d8a5e2ce61f0517b10c9e1bffd9c4f618ccacdcf07aebc3774dd45a22`
update_zip_sha256: `f9dfa886dd5db2623ec63342538d775757b5f464e9eb9ca23a5206bcc1d65ba8`
burn_bundle_sha256: `502b5bc216fdb3dc2ee0db8fdf49ef5aa79d2dafef6db42b46aa5d642d774567`
msix_v2_sha256: `28f853740a7e71b13353998fd7263c3452961eee3e0747d3ba31b3e47ab7a9f2`
host_mutation_performed: `true`
evidence_scope: `internal-admin-smoke-only`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` (`ready-current-baseline-target-package-pair`) | `artifacts/manual-admin-campaign-20260809-04271-04272/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260809-04271-04272/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5099540`, UBR `169 -> 5386` | `artifacts/manual-admin-campaign-20260809-04271-04272/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260809-04271-04272/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260809-04271-04272/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260809-04271-04272/installed-runtime-ops-summary/summary.json` |

Descriptor는 `runner_count=6`, `missing_count=0`, `not_pass_count=0`,
`overall_status=pass`로 닫혔다.

## 설치본 update/rollback

| 단계 | exit | 결과 버전 |
| --- | ---: | --- |
| update | `0` | `0.42.72-admin-smoke` |
| rollback | `0` | `0.42.71-admin-smoke` |
| final update | `0` | `0.42.72-admin-smoke` |

최종 설치본은 `0.42.72-admin-smoke`다.

## Clean-host

throwaway VM `pcv-cleanhost-20260809-04271-04272`에서 Windows Update 적용 후 baseline
install, catalog update, rollback을 실행했다.

| 항목 | 값 |
| --- | --- |
| install / update / rollback exit | `0 / 0 / 0` |
| final Web | HTTP `200` |
| blocker | `none` |
| Windows Update | `KB5099540`, UBR `169 -> 5386` |
| automatic recovery | `true` (post-WU heartbeat `NoContact` recovery 1회) |
| final guest manifest | `0.42.71-admin-smoke` |
| VM cleanup | success 후 VM 제거 |

Burn bundle은 install/repair/remove와 target MSI restore/native repair가 모두 exit `0`다.
MSIX는 `0.42.71.0` install, `0.42.72.0` update, remove 후 final package absent가
`true`다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- clean-host guest의 internal root certificate import는 수행되지 않았고 baseline MSI는
  `AllowUnsignedDev` 범위다.
- winget submission은 `out-of-scope`다.
- campaign target은 clean package MSI다. operational fullgate MSI hash는 별도다.
