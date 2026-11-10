# Manual-admin campaign descriptor `0.43.1-admin-smoke` -> `0.43.2-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-10-04301-04302`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261110-04301-04302-closed`
artifact_root: `artifacts/manual-admin-campaign-20261110-04301-04302/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261110-04301-04302/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `2e1ec0f15139d0f7b874b59e4193d3acf65718ca2e56b00902fbcb1c7183ce7d`
summary_sha256: `2fc1ce203cb19648d33dad95ca6e4740d80751a0088fa9f2116402fd7077327e`
readiness_root: `artifacts/manual-admin-campaign-20261110-04301-04302/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `fd0014cf26afeb77b9e668217023043df7e34e182714d0dc334666c0cd846d70`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.43.1-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.43.2` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-10-04301-04302` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-10-04301-04302` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-10-04302` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-10-04301-04302` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-10-04302` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.2-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.43.1-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.2-admin-smoke`로 올리지 않았다.
