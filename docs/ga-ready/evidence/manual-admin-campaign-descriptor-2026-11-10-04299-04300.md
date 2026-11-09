# Manual-admin campaign descriptor `0.42.99-admin-smoke` -> `0.43.0-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-10-04299-04300`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261110-04299-04300-closed`
artifact_root: `artifacts/manual-admin-campaign-20261110-04299-04300/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261110-04299-04300/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `8ee611f2782d121e03d0c89295280786c5df46ed683b3ff7dda40a79156d39a0`
summary_sha256: `cbea4b31b875c96862a54002e1bd049a4548881f895f43e82d35522e6eaca0da`
readiness_root: `artifacts/manual-admin-campaign-20261110-04299-04300/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `9122ce23e2b0a3029e2e0a28b0832e793ce0d2fb8f0f72b8a123de78c449a3ed`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.99-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.43.0` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-10-04299-04300` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-10-04299-04300` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-10-04300` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-10-04299-04300` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-10-04300` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.0-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.99-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.0-admin-smoke`로 올리지 않았다.
