# Manual-admin campaign descriptor `0.42.91-admin-smoke` -> `0.42.92-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-07-04291-04292`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261007-04291-04292-closed`
artifact_root: `artifacts/manual-admin-campaign-20261007-04291-04292/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261007-04291-04292/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `3604ac35fdeea61633883525abb31df8cbd30827c6442b5897f19d6c3f105082`
summary_sha256: `e09aa8f0c5e452aaa109f0d0e61bf91e2db39f321f9db792d65c83f3f506bc9a`
readiness_root: `artifacts/manual-admin-campaign-20261007-04291-04292/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `5b634314f04c779ec8b376582647574c35e07683a1b713e69bb95be9aed0cf02`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.91-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.42.92` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-07-04291-04292` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-07-04291-04292` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-07-04292` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-07-04291-04292` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-07-04292` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.92-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.91-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.92-admin-smoke`로 올리지 않았다.
