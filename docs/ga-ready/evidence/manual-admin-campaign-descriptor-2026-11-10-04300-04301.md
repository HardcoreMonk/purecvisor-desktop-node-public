# Manual-admin campaign descriptor `0.43.0-admin-smoke` -> `0.43.1-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-10-04300-04301`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261110-04300-04301-closed`
artifact_root: `artifacts/manual-admin-campaign-20261110-04300-04301/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261110-04300-04301/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `a3d3aabefb6965e226b5422c07473b9df02dca36f6dcdb4cc9bf24a11ae28e3b`
summary_sha256: `76eef5fc18c229ba16c4f74f998152e3d088c1923bdce3d819ae747b5c7922a6`
readiness_root: `artifacts/manual-admin-campaign-20261110-04300-04301/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `189be30c7819080552693cd217511d8980656552bbfa3253c95fc5c68dd95ea5`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.43.0-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.43.1` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-10-04300-04301` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-10-04300-04301` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-10-04301` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-10-04300-04301` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-10-04301` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.1-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.43.0-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.1-admin-smoke`로 올리지 않았다.
