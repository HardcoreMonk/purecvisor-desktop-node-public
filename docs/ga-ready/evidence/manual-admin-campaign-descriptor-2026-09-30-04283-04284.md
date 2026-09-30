# Manual-admin campaign descriptor `0.42.83-admin-smoke` -> `0.42.84-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-09-30-04283-04284`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260930-04283-04284`
artifact_root: `artifacts/manual-admin-campaign-descriptor-20260930-04283-04284`
descriptor_path: `artifacts/manual-admin-campaign-descriptor-20260930-04283-04284/manual-admin-campaign.descriptor.json`
descriptor_sha256: `7fa6191d2573192788d4871e74ed82bf8eef11edfbef30d985af1eb3f6979584`
summary_sha256: `f8bdd17b964a29c28c05ed433fa6874132cd6d383e1599ebc7637dad5e5fd686`
readiness_root: `artifacts/manual-admin-rebaseline-readiness-20260930-04283-04284`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.83-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

Readiness는 `PlanOnly`다. 캡처 당시 설치본 manifest는 `0.42.83-admin-smoke`로 baseline과 일치했고, package pair 입력은 `ready-current-baseline-target-package-pair`다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | Task 4a, readiness summary |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-09-30-04283-04284` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-09-30-04283-04284` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-09-30-04284` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-09-30-04283-04284` (r2 root) |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-09-30-04283` |

## 판정

descriptor는 `overall_status=pass`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.84-admin-smoke`다.

full admin host mutation과 installed current-card는 이 descriptor의 여섯 runner에 들어 있지 않다. `docs/ga-ready/current-evidence.json`은 `0.42.83-admin-smoke` 그대로다. 이번 campaign은 Lane 3 consume descriptor를 만들지 않는다. Lane 3 승격은 승인 밖이다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.84-admin-smoke`로 올리지 않았다.
