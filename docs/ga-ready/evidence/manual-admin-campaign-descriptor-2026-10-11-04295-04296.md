# Manual-admin campaign descriptor `0.42.95-admin-smoke` -> `0.42.96-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-11-04295-04296`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261011-04295-04296-closed`
artifact_root: `artifacts/manual-admin-campaign-20261011-04295-04296/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261011-04295-04296/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `d96d9ec10b2e0f33bce263452e80bd2a573ad59825c4eab9b9c83727cb8a60a7`
summary_sha256: `b0603bd71eb3f7cc5d8669c3d5b83370219e6c87dc9157a9371b2ba72f8c8007`
readiness_root: `artifacts/manual-admin-campaign-20261011-04295-04296/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `af288b481f2a05e99c864a611b40d45c9171b8c16757e077fc82b2a93aa834ea`
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

release train `0.42.96` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-11-04295-04296` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-11-04295-04296` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-11-04296` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-11-04295-04296` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-11-04296` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.96-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.93-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.96-admin-smoke`로 올리지 않았다.
