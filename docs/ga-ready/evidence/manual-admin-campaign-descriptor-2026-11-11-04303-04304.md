# Manual-admin campaign descriptor `0.43.3-admin-smoke` -> `0.43.4-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-11-04303-04304`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261111-04303-04304-closed`
artifact_root: `artifacts/manual-admin-campaign-20261111-04303-04304/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261111-04303-04304/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `574b22c1200ad59e19dda450bf1dd6fefe4dd52dbe4e16f3c17999a374addb1d`
summary_sha256: `56e1ca96ba7c900526dbee9cd5166392a24b3f7f3bbfa8ee3fd723a9393c4fe5`
readiness_root: `artifacts/manual-admin-campaign-20261111-04303-04304/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `92fb654ed7ca48753a63b5abf45d9ba8c45a5c3c5d033a18e4f73fe825a99a4d`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.43.3-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.43.4` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-11-04303-04304` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-11-04303-04304` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-11-04304` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-11-04303-04304` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-11-04304` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.4-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.43.3-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.4-admin-smoke`로 올리지 않았다.
