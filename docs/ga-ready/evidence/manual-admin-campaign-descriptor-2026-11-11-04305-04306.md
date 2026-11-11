# Manual-admin campaign descriptor `0.43.5-admin-smoke` -> `0.43.6-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-11-04305-04306`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261111-04305-04306-closed`
artifact_root: `artifacts/manual-admin-campaign-20261111-04305-04306/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261111-04305-04306/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `b24f2204b33568fcf7c612101331fdc2dde6276a587d97081e48a9259a447cdc`
summary_sha256: `809308f35c867246b9a446ab45700aa73dadb1d46c22d3e2b3b643133ae14e28`
readiness_root: `artifacts/manual-admin-campaign-20261111-04305-04306/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `6766095bc99fac3b545f0b4279ce3720c488bc0a9bd90ba9d7d36499cf09b9c1`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.43.4-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.43.6` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-11-04305-04306` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-11-04305-04306` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-11-04306` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-11-04305-04306` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-11-04306` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.6-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.43.4-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.6-admin-smoke`로 올리지 않았다.
