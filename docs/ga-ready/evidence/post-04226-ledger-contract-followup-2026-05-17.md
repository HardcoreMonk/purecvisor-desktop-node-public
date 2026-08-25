# Post-04226 Ledger Contract Follow-up

evidence_id: `post-04226-ledger-contract-followup-2026-05-17`
created_at: `2026-05-17T13:15:00+09:00`
result: `CODE_LEVEL_PASS_PENDING_POSTMERGE_PACKAGE_TRIGGER`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 범위

사용자 승인 `1-2-3-4-5`에 따라 04226 closure 이후 후속 5개 항목을 진행했다.

- Runtime/API current evidence rollup에서 manual-admin closure descriptor batch id를
  current-card contract에 직접 노출한다.
- GA-ready evidence의 current/historical 중복 설명을 `CURRENT_EVIDENCE_LEDGER.md`로
  압축한다.
- Manual-admin package-pair descriptor generator를 schema v2 필드와 Batch Supervisor
  batch id 전달로 정렬한다.
- 다음 개발 slice를 Runtime/API current evidence hardening 완료 후 Host Ops lifecycle
  descriptor bridge로 선정한다.

## Product Payload 판단

pre_branch_product_payload_change_detected: `false`
pre_branch_checked_range: `d6500c01c972cbc7ca1e290e51120181ceea1501..origin/main`
pre_branch_changed_payload_paths: `none`
pre_branch_changed_non_payload_paths: `packaging/windows-desktop-node/README.md`, `packaging/windows-desktop-node/installer/README.md`, `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

이번 branch 자체는 `src/DesktopNode.Api`, `web`, `packaging/windows-desktop-node/tools`,
tests, docs를 변경하므로 post-merge product payload trigger를 연다.

next_product_payload_package_build_trigger: `post-04226-ledger-contract-merge`
next_product_payload_candidate: `0.42.26-admin-smoke -> next`
package_build_decision_this_branch: `not-run-pending-postmerge-versioned-payload`
full_admin_host_mutation_decision_this_branch: `not-run-pending-postmerge-versioned-payload`
manual_admin_package_pair_decision_this_branch: `not-run-pending-postmerge-versioned-payload`

## Contract 변경

| Contract | 변경 |
| --- | --- |
| `runtime-api-current-evidence-rollup-v1` | `manual_admin.latest_package_pair.current_card_descriptor_batch_id`, `descriptor_summary`, `descriptor_source`, `descriptor_overall_status`, count fields를 stable JSON으로 노출한다. |
| `BatchEvidenceSummaryReader` | campaign descriptor summary에 `descriptor_batch_id`가 없더라도 `manual-admin-campaign-*` path에서 closure id를 추론한다. |
| `ManualAdminCampaignDescriptor` | `descriptor_schema_version=2`, `descriptor_contract_key=manual-admin-descriptor-generation-contract-v2`, `descriptor_batch_id`를 descriptor와 summary에 기록한다. |
| `Batch Supervisor ManualAdminCampaignDescriptor` | manifest `BatchId`를 `New-PcvManualAdminCampaignDescriptor.ps1 -DescriptorBatchId`로 전달한다. |
| Web current-card | `current_card_descriptor_batch_id`를 우선 표시하고 기존 `descriptor_batch_id`를 fallback으로 유지한다. |

## Current Ledger

current_ledger: `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
current_full_admin_host_mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-16-04226-hostmutation.md`
current_manual_admin_package_pair: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-17-04225-04226.md`
current_descriptor_batch_id: `manual-admin-campaign-descriptor-20260517-04225-04226-closed`
current_descriptor_summary: `artifacts/manual-admin-campaign-20260517-04225-04226/manual-admin-campaign-descriptor/summary.json`
current_runtime_card_contract: `runtime-api-current-evidence-rollup-v1`

## 다음 개발 Slice

선정: `Host Ops lifecycle descriptor bridge`

다음 slice는 service-action, Event Log, firewall, trust store, Credential Manager,
data-root lifecycle evidence를 current evidence ledger에 연결하고, Host Ops descriptor가
manual-admin/package/fullgate evidence와 같은 방식으로 current/historical 상태를 말하게 만든다.
