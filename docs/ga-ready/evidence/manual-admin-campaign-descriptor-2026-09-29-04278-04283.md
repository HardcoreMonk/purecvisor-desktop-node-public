# Manual-admin campaign descriptor `0.42.78-admin-smoke` -> `0.42.83-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-09-29-04278-04283`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260929-04278-04283`
artifact_root: `artifacts/manual-admin-campaign-descriptor-20260929-04278-04283`
descriptor_path: `artifacts/manual-admin-campaign-descriptor-20260929-04278-04283/manual-admin-campaign.descriptor.json`
descriptor_sha256: `87808eba51a05699e18ec1b75ab713c675f5d2b9607a5466f6b8243f270b375f`
summary_sha256: `5c473e8c866f4f6fb7267f94fdc86be83c079a7e4fa5de3c9dacf24d5adfc4d6`
readiness_root: `artifacts/manual-admin-rebaseline-readiness-20260929-04278-04283`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.78-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

Readiness는 `PlanOnly`다. 캡처 당시 설치본 manifest는 `0.42.78-admin-smoke`로 baseline과 일치했고, package pair 입력은 `ready-current-baseline-target-package-pair`다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | Task 2a, readiness summary |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-09-29-04278-04283` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-09-29-04278-04283` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-09-29-04283` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-09-29-04278-04283` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-09-29-04278` |

## 판정

descriptor `overall_status`는 `pass`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. descriptor의 `release_candidate.next_candidate_version`은 `0.42.83-admin-smoke`다.

full admin host mutation과 installed current-card는 이 descriptor의 여섯 runner에 포함되지 않는다. `docs/ga-ready/current-evidence.json`은 `0.42.78-admin-smoke`다. Lane 3는 이 여섯 summary를 한 root로 모은 consume descriptor를 따로 만든다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.83-admin-smoke`로 올리지 않았다.
