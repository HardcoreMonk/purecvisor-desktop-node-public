# Manual-admin campaign descriptor `0.42.89-admin-smoke` -> `0.42.90-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-05-04289-04290`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261005-04289-04290-closed`
artifact_root: `artifacts/manual-admin-campaign-20261005-04289-04290/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261005-04289-04290/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `9ab32dc3cc895c8cdfbe6192416f3a122c57429f63df3dae15d0e89699e29581`
summary_sha256: `8e3e135b1782d3ed1b77904e92d693a7a523f301c772a6b2d67d6a13c3c653f4`
readiness_root: `artifacts/manual-admin-campaign-20261005-04289-04290/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `d22813d40bd964a9c4e0e853d3c15d7fec366af8092738622688f59037ed58fe`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.89-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.42.90` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-05-04289-04290` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-05-04289-04290` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-05-04290` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-05-04289-04290` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-05-04290` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.90-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.89-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.90-admin-smoke`로 올리지 않았다.
