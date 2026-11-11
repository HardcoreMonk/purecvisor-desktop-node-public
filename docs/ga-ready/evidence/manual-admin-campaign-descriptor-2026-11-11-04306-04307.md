# Manual-admin campaign descriptor `0.43.6-admin-smoke` -> `0.43.7-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-11-11-04306-04307`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261111-04306-04307-r2-closed`
artifact_root: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `ac229c81fd78b79659368a9d39b34cf4509badc577e09da17e555975c24a49c3`
summary_sha256: `d1cb9a2cb65880c79ca51a7d72e589c725636e6f1d13068dae3b5782756e9787`
readiness_root: `artifacts/manual-admin-campaign-20261111-04306-04307-r2/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `1dbceb8574885f184b895bc8847d3e29c468a7cc0e8abc1a8fe3615d40436b02`
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

release train `0.43.7` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-11-11-04306-04307` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-11-11-04306-04307` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-11-11-04307` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-11-11-04306-04307` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-11-11-04307` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.43.7-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.43.4-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.43.7-admin-smoke`로 올리지 않았다.
