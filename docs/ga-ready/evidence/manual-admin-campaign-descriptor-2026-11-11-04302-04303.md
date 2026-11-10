# Manual-admin campaign descriptor `0.43.2-admin-smoke` -> `0.43.3-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-11-04302-04303`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261111-04302-04303-closed`
artifact_root: `artifacts/manual-admin-campaign-20261111-04302-04303/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261111-04302-04303/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `f66853ced6860e03ba39a34940aa367598a8ef06d7785160d9d7bdfc1ddb0b0c`
summary_sha256: `5b2335ceb5bedd95597b1242ce0c82ca1f6c242528125d617ee7c7bd1d6e67a1`
readiness_root: `artifacts/manual-admin-campaign-20261111-04302-04303/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `c4cb3d7aa7461412b7bfe07addf2b2b77e2241978765e39ae3e6b7c247016669`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.43.2-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.43.3` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-11-04302-04303` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-11-04302-04303` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-11-04303` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-11-04302-04303` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-11-04303` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.3-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.43.2-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.3-admin-smoke`로 올리지 않았다.
