# Manual-admin campaign descriptor `0.42.92-admin-smoke` -> `0.42.93-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-08-04292-04293`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261008-04292-04293-closed`
artifact_root: `artifacts/manual-admin-campaign-20261008-04292-04293/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261008-04292-04293/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `4a86e474d30cedb46da21de7d532c82e371ad303c5634753cf940d3e114adb91`
summary_sha256: `7fb71b4723717a1a87f4bf29bb20821e7cb66a6ac12da55d678362e7d9b6be9a`
readiness_root: `artifacts/manual-admin-campaign-20261008-04292-04293/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `d002165f79f9ee5c344d34bca9ec4d5a7e617979b6ac61b558934acea0a6fd7e`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.92-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.42.93` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-08-04292-04293` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-08-04292-04293` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-08-04293` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-08-04292-04293` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-08-04293` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.93-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.92-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.93-admin-smoke`로 올리지 않았다.
