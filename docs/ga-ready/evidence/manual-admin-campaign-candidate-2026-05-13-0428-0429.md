# Manual Admin Campaign Candidate - 2026-05-13 0.42.8→0.42.9

evidence_id: `manual-admin-campaign-candidate-2026-05-13-0428-0429`
result: `CANDIDATE_UPDATE_ROLLBACK_ONLY`
baseline_version: `0.42.8-admin-smoke`
target_version: `0.42.9-admin-smoke`
host_mutation_performed: `false`
artifact_root: `artifacts/manual-admin-campaign-20260513-0428-0429`
target_package_root: `artifacts/admin-smoke-package-20260513-0429`
target_msi_sha256: `a5578d2e59620d1f49b20db3f9bfb2bc7194853e3e20ff3521aff89d923d7bfb`
target_provenance_commit: `f0620f2e18ae25de8751333684cb74b5051dcdc6`
update_zip_sha256: `7c813e94224056013d46de97199df74f3ecd3b572d7aa4fa3ac8c0b07446686f`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 판정

`0.42.8-admin-smoke -> 0.42.9-admin-smoke`는 다음 manual-admin package-pair
후보로 등록한다. 현재 실행한 범위는 installed update/rollback smoke까지이며,
dedicated clean-host, Burn, MSIX, full descriptor generation은 아직 이 evidence에서
PASS로 claim하지 않는다.

마지막 닫힌 full package-pair PASS는
`docs/ga-ready/evidence/manual-admin-campaign-2026-05-12-0427-0428.md`다.

## 실행한 Lifecycle Smoke

| 항목 | 값 |
| --- | --- |
| update package | `artifacts/manual-admin-campaign-20260513-0428-0429/lifecycle/PureCVisorDesktopNode-0.42.9-admin-smoke-update.zip` |
| update package SHA-256 | `7c813e94224056013d46de97199df74f3ecd3b572d7aa4fa3ac8c0b07446686f` |
| status before | `artifacts/manual-admin-campaign-20260513-0428-0429/lifecycle/product-update-rollback/01-status-before.json` |
| update result | `artifacts/manual-admin-campaign-20260513-0428-0429/lifecycle/product-update-rollback/02-update.json` |
| manifest after update | `artifacts/manual-admin-campaign-20260513-0428-0429/lifecycle/product-update-rollback/03-manifest-after-update.json` |
| rollback result | `artifacts/manual-admin-campaign-20260513-0428-0429/lifecycle/product-update-rollback/04-rollback.json` |
| manifest after rollback | `artifacts/manual-admin-campaign-20260513-0428-0429/lifecycle/product-update-rollback/05-manifest-after-rollback.json` |
| status after | `artifacts/manual-admin-campaign-20260513-0428-0429/lifecycle/product-update-rollback/06-status-after.json` |

관찰 결과:

- Update preflight는 expected SHA-256과 actual SHA-256이 동일했다.
- Updated manifest는 `0.42.9-admin-smoke`였다.
- Rollback 이후 manifest는 `0.42.8-admin-smoke`로 복원됐다.
- 최종 service status는 `Running`이었다.

## 남은 Package-pair Gate

이 candidate가 full package-pair PASS가 되려면 다음 evidence가 추가로 필요하다.

- dedicated clean-host install/update/rollback
- Burn install/repair/remove
- MSIX build/install/update/remove
- installed runtime ops summary capture
- `ManualAdminCampaignDescriptor` summary generation

## Release Boundary

이 evidence는 internal/admin-smoke lifecycle candidate evidence다. Public trusted
signing, external stable publication, public update channel availability, winget
submission은 claim하지 않는다.
