# Manual-admin campaign descriptor `0.42.85-admin-smoke` -> `0.42.86-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-02-04285-04286`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-descriptor-20261002-04285-04286`
artifact_root: `artifacts/manual-admin-campaign-descriptor-20261002-04285-04286`
descriptor_path: `artifacts/manual-admin-campaign-descriptor-20261002-04285-04286/manual-admin-campaign.descriptor.json`
descriptor_sha256: `dc010c3d60cabda2bf327c5d631ff1411fae94bd45b69ae6e79c9b0660ef4238`
summary_sha256: `b6ea6f887a98a488f7d1856ac13a33c10028c9c02e472f4650bf35b1e5320bf7`
readiness_root: `artifacts/manual-admin-rebaseline-readiness-20261002-04285-04286`
readiness_summary_sha256: `9b1e9105d0b326c260466fe1d7f0bfb6ee42bcbdac1df72aff91816cb629acc6`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.84-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-02-04285-04286` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-02-04285-04286` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-02-04286` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-02-04285-04286` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-02-04285` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.86-admin-smoke`다.

full admin host mutation과 installed current-card는 이 descriptor의 여섯 runner에 들어 있지 않다. `docs/ga-ready/current-evidence.json`은 `0.42.84-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.86-admin-smoke`로 올리지 않았다.
