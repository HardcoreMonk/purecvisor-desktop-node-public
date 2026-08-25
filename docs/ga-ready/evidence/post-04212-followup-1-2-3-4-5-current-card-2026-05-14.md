# Post-04212 1-2-3-4-5 Current-card Follow-up - 2026-05-14

evidence_id: post-04212-followup-1-2-3-4-5-current-card-2026-05-14
scope: post-04212-followup-current-card-smoke-no-new-product-payload
status: pass-dashboard-current-card-smoke-deferred-product-chain
user_approved_followup_bundle: 1-2-3-4-5
latest_product_payload_package_build: 0.42.12-admin-smoke
latest_product_payload_package_build_evidence: docs/ga-ready/evidence/ops-summary-data-builder-package-2026-05-13-04212.md
latest_product_payload_provenance_commit: 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3
latest_full_admin_host_mutation_evidence: docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md
latest_full_admin_host_mutation_batch: full-admin-host-mutation-gate-20260514-04212-rerun
main_commit_checked: 8224af81c00482145b6c08dcde8c92a039b2aa26
product_payload_change_detected: false
product_payload_diff_scope: no-changes-in-src-web-product-wrapper-installer
next_candidate_version_hint: 0.42.13-admin-smoke
package_build_decision: deferred-until-next-product-payload-change
full_admin_host_mutation_campaign_decision: not-run-no-product-payload
manual_admin_package_pair_campaign_decision: deferred-until-next-product-payload-change
clean_host_recovery_summary_key_decision: not-executed-no-package-pair-campaign
dashboard_current_card_smoke: pass
evidence_view_current_card_smoke: pass
dashboard_current_card_artifact_root: artifacts/web-console-current-card-20260514-04212-rerun-followup
dashboard_current_card_summary: artifacts/web-console-current-card-20260514-04212-rerun-followup/summary.json
dashboard_current_card_screenshots: dashboard-current-card.png, evidence-view.png
token_value_observed: false
host_mutation_performed: false
guest_product_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
public_release: not-claimed

## 요약

사용자가 다시 승인한 `1-2-3-4-5` 묶음은 다음 product payload 변경이 있을 때
`0.42.13-admin-smoke` package build, `0.42.12-admin-smoke -> 0.42.13-admin-smoke`
manual-admin package-pair, clean-host recovery summary 검증, full admin host mutation
gate, Dashboard/Web Console current-card smoke를 순서대로 실행하는 후속 작업이다.

이번 실행에서는 먼저 최신 product payload provenance commit
`8f694dc2494314a6ddd7223f46ec0ba0ca8523e3` 이후 현재 `main`
`8224af81c00482145b6c08dcde8c92a039b2aa26`까지의 product payload 변경을
확인했다. `src`, `web`, product wrapper, installer payload 범위에서 변경이 없으므로
새 `0.42.13-admin-smoke` package build, package-pair campaign, clean-host campaign,
full admin host mutation gate는 열지 않았다.

대신 5번 항목인 Web Console current-card smoke는 설치본 listener에 대해 실행했다.
Dashboard와 Evidence view 모두 최신 batch
`full-admin-host-mutation-gate-20260514-04212-rerun`과 version
`0.42.12-admin-smoke`를 표시했다. Token 값은 UI text와 evidence summary에 기록하지
않았다.

## 실행 판단

| 항목 | 결과 |
| --- | --- |
| `0.42.13-admin-smoke` product package build | `deferred-until-next-product-payload-change` |
| `0.42.12-admin-smoke -> 0.42.13-admin-smoke` package-pair campaign | `deferred-until-next-product-payload-change` |
| Clean-host Windows Update recovery summary key 검증 | `not-executed-no-package-pair-campaign` |
| `0.42.13-admin-smoke` full admin host mutation gate | `not-run-no-product-payload` |
| Dashboard/Web Console current-card smoke | `pass` |
| Host mutation | `false` |

Product payload 변경 확인 명령:

```powershell
git diff --name-only 8f694dc2494314a6ddd7223f46ec0ba0ca8523e3..HEAD -- src web packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 packaging/windows-desktop-node/installer
```

반환된 path는 없었다. 따라서 current claim은 계속 아래 evidence가 소유한다.

- Product payload package build: `docs/ga-ready/evidence/ops-summary-data-builder-package-2026-05-13-04212.md`
- Full admin host mutation: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-14-04212-rerun-hostmutation.md`
- Manual-admin package-pair: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-14-04211-04212.md`

## Web Console Current-card Smoke

Artifact root:
`artifacts/web-console-current-card-20260514-04212-rerun-followup`

Summary:
`artifacts/web-console-current-card-20260514-04212-rerun-followup/summary.json`

Screenshots:

- `artifacts/web-console-current-card-20260514-04212-rerun-followup/dashboard-current-card.png`
- `artifacts/web-console-current-card-20260514-04212-rerun-followup/evidence-view.png`

주요 check:

- `dashboard_mentions_latest_evidence`: `true`
- `dashboard_mentions_expected_batch`: `true`
- `dashboard_mentions_expected_version`: `true`
- `evidence_view_present`: `true`
- `evidence_view_mentions_expected_batch`: `true`
- `evidence_view_mentions_expected_version`: `true`
- `token_value_observed_in_ui_text`: `false`

Evidence view에 표시된 현재 batch:

```text
Batch full-admin-host-mutation-gate-20260514-04212-rerun
Version 0.42.12-admin-smoke
Public signing: excluded
External publication: not-claimed
```

## 후속 작업 목록

다음 실제 product payload 변경이 발생할 때만 아래 순서로 다시 연다.

1. `0.42.13-admin-smoke` package build 후보 생성
2. `0.42.12-admin-smoke -> 0.42.13-admin-smoke` manual-admin package-pair campaign 실행
3. clean-host Windows Update recovery summary의 `recovery_actions`, `automatic_recovery_performed` 검증
4. 새 package 기준 full admin host mutation gate 실행
5. Dashboard/Web Console current-card smoke 재실행

## 경계

이 evidence는 product payload 변경 없음 판단과 설치본 Web Console current-card smoke를
기록한다. 새 MSI, update ZIP, Burn/MSIX package, dedicated clean-host VM 실행,
Hyper-V/firewall/LAN/Event Log/internal trust-store mutation은 수행하지 않았다. Public
trusted signing, external stable publication, public release는 계속 `not-claimed`다.
