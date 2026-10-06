# Manual-admin campaign descriptor `0.42.90-admin-smoke` -> `0.42.91-admin-smoke`

evidence_id: `manual-admin-campaign-descriptor-2026-10-06-04290-04291`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
descriptor_batch_id: `manual-admin-campaign-20261006-04290-04291-closed`
artifact_root: `artifacts/manual-admin-campaign-20261006-04290-04291/manual-admin-campaign-descriptor`
descriptor_path: `artifacts/manual-admin-campaign-20261006-04290-04291/manual-admin-campaign-descriptor/manual-admin-campaign.descriptor.json`
descriptor_sha256: `6f4fc0de291a7b8c3d9dd255ca355b4a3c0d44e333dc8ccf2eb0539a42cb0866`
summary_sha256: `5f1e059b5037c06bc72afaff188591fb479519d8b01ca371d0fdfbcb78611e29`
readiness_root: `artifacts/manual-admin-campaign-20261006-04290-04291/manual-admin-rebaseline-readiness`
readiness_summary_sha256: `fa3d5b8ddf0476cc86ea680d5f9a2f2da626073f9b698fac90933a465430da0b`
plan_only: `true`
host_mutation_performed: `false`
overall_status: `pass`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
canonical_current_evidence: `0.42.90-admin-smoke`
canonical_current_changed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 입력

release train `0.42.91` pair orchestrator가 여섯 bucket을 돈 뒤 닫은 descriptor다.

| runner | status | evidence |
| --- | --- | --- |
| `manual-admin-readiness` | `pass` | `ready-current-baseline-target-package-pair` |
| `installed-product-update-rollback` | `pass` | `product-update-rollback-2026-10-06-04290-04291` |
| `clean-host-install-update-rollback` | `pass` | `internal-clean-host-install-update-rollback-smoke-2026-10-06-04290-04291` |
| `burn-install-repair-remove` | `pass` | `burn-bootstrapper-lifecycle-smoke-2026-10-06-04291` |
| `msix-build-install-update-remove` | `pass` | `msix-package-lifecycle-smoke-2026-10-06-04290-04291` |
| `installed-runtime-ops-summary` | `pass` | `installed-runtime-ops-summary-2026-10-06-04291` |

## 판정

descriptor는 `overall_status=pass`, runner `6/6`, `missing_count=0`, `not_pass_count=0`이다. 이 생성은 로컬 evidence를 읽기만 했다. `release_candidate.next_candidate_version`은 `0.42.91-admin-smoke`다. `docs/ga-ready/current-evidence.json`은 `0.42.90-admin-smoke` 그대로다.

## Nonclaims

- public trusted signing과 external stable publication을 주장하지 않는다.
- operational current를 `0.42.91-admin-smoke`로 올리지 않았다.
