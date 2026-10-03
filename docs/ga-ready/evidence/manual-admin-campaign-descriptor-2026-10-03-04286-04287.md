# Manual-admin campaign descriptor `0.42.86-admin-smoke` -> `0.42.87-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-03-04286-04287`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-descriptor-20261003-04286-04287`
artifact_root: `artifacts/manual-admin-campaign-descriptor-20261003-04286-04287`
descriptor_path: `artifacts/manual-admin-campaign-descriptor-20261003-04286-04287/manual-admin-campaign.descriptor.json`
descriptor_sha256: `8d5fe1fb7ba531e34a9e5d58d11b1e72ab2fe08624a23c0d3450f5329d8f9ce5`
summary_sha256: `d0832bdc73336706f806ee5be65fa090940c911667e27dc635cb6716eb2d455f`
readiness_root: `artifacts/manual-admin-rebaseline-readiness-20261003-04286-04287`
readiness_summary_sha256: `e460a21e3395712836697b90e630d51a446406c1ee4dd2fc9c73a77372173db2`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.86-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-03-04286-04287` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-03-04286-04287` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-03-04287` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-03-04286-04287` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-03-04286` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.87-admin-smoke`다.

full admin host mutation과 installed current-card는 이 descriptor의 여섯 runner에 들어 있지 않다. `docs/ga-ready/current-evidence.json`은 `0.42.86-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.87-admin-smoke`로 올리지 않았다.
