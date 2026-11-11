# Manual-admin campaign descriptor `0.43.4-admin-smoke` -> `0.43.5-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-11-04304-04305`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261111-04304-04305-closed`
artifact_root: `artifacts/manual-admin-campaign-20261111-04304-04305/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261111-04304-04305/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `f218968d1fff75a32fb55ff696a1e45582bc06bb84557df4f7d1eef3e845bf66`
summary_sha256: `f0c3be062a5ce181397c06804b3e7e9c4229f606c499658f12a159c867d846fc`
readiness_root: `artifacts/manual-admin-campaign-20261111-04304-04305/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `498dd73bec11c8a2d9ac9c0e343d4dbdf51eb96216373032c81977266b2d0d6c`
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

release train `0.43.5` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-11-04304-04305` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-11-04304-04305` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-11-04305` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-11-04304-04305` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-11-04305` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.5-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.43.4-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.5-admin-smoke`로 올리지 않았다.
