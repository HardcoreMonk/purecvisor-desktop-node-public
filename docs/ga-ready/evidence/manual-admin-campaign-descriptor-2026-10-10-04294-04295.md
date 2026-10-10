# Manual-admin campaign descriptor `0.42.94-admin-smoke` -> `0.42.95-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-10-04294-04295`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261010-04294-04295-closed`
artifact_root: `artifacts/manual-admin-campaign-20261010-04294-04295/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261010-04294-04295/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `0f212038e7d1eb165d537c8ff46613cdf53c515c80630848e33bb464e4ec0065`
summary_sha256: `6fe0b7c18185ddcca597c72305d8ad3be72e90226aa63edae150339e6fdf1ef6`
readiness_root: `artifacts/manual-admin-campaign-20261010-04294-04295/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `f5af8608596a389f01a0ee1c339880e7e25d1baba170f5b6829879656f8e1f32`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.93-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.42.95` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-10-04294-04295` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-10-04294-04295` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-10-04295` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-10-04294-04295` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-10-04295` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.95-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.93-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.95-admin-smoke`로 올리지 않았다.
