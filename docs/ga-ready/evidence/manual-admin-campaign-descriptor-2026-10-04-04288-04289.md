# Manual-admin campaign descriptor `0.42.88-admin-smoke` -> `0.42.89-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-04-04288-04289`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-descriptor-20261004-04288-04289`
artifact_root: `artifacts/manual-admin-campaign-descriptor-20261004-04288-04289`
descriptor_path: `artifacts/manual-admin-campaign-descriptor-20261004-04288-04289/manual-admin-campaign.descriptor.json`
descriptor_sha256: `fbc3fea31a720494afcf41686320700c7579507419b4e24bdfc26e3db3908a7c`
summary_sha256: `42ed137818480fc66348896129a41c98b66f1ab729a6111c84eace5988e08a09`
readiness_root: `artifacts/manual-admin-rebaseline-readiness-20261004-04288-04289`
readiness_summary_sha256: `489958e1b985b7f841681db7cef796df5e11fa02c7a2fc38c101a73e1bb046ac`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.88-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.42.89` pair의 여섯 bucket이다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-04-04288-04289` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-04-04288-04289` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-04-04289` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-04-04288-04289` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-04-04288` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.89-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.88-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.89-admin-smoke`로 올리지 않았다.
