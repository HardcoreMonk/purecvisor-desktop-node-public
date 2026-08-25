# Manual-admin campaign 2026-05-26 0.42.47 -> 0.42.48

evidence_id: `manual-admin-campaign-2026-05-26-04247-04248`
result: `PASS`
scope: `internal-manual-admin-package-pair`
baseline_version: `0.42.47-admin-smoke`
target_version: `0.42.48-admin-smoke`
campaign_artifact_root: `artifacts/manual-admin-campaign-20260526-04247-04248`
descriptor_schema_version: `2`
descriptor_contract_key: `manual-admin-descriptor-generation-contract-v2`
descriptor_batch_id: `manual-admin-campaign-descriptor-20260526-04247-04248-closed`
descriptor_summary: `artifacts/manual-admin-campaign-20260526-04247-04248/manual-admin-campaign-descriptor-windows-update/summary.json`
target_msi_sha256: `a0014960979ed23cec8d882cddd22baaaf9435a71287bdc133a79ff0b381338c`
baseline_msi_sha256: `9589086d092ee902b72ff7790cac5a25e6d806cdaac0d98e431a27048dc5e197`
update_zip_sha256: `84d8c28d3cf2e8b7a5abd91d8663e99d7809b4dcc1d9ee53e2696ae091f6e32b`
burn_bundle_sha256: `1ff09a0c02ad022775b56af8fe781fbb28c0f9522de5fb983a94343ac0ff62ac`
msix_v1_sha256: `098133f1427bffa134e0156da80063b4ddb13a9cbfdb097e30aa385849fb9991`
msix_v2_sha256: `f02854559226c3f4a46c893ebeb06620b126e64481b8c172490ed12715801ea7`
windows_update_kb: `KB5087545`
windows_update_ubr: `5139`
missing_count: `0`
not_pass_count: `0`
runner_count: `6`
host_mutation_performed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 Phase 3 Web/TUI QoS direct control payload인 `0.42.48-admin-smoke`를
manual-admin package-pair lifecycle까지 닫은 기록이다. Baseline은
`0.42.47-admin-smoke`이며 target clean package는
`artifacts/admin-smoke-package-20260526-04248`의 MSI를 사용했다.

## PASS Bucket

| Bucket | 결과 | Artifact |
| --- | --- | --- |
| readiness | `PASS` | `artifacts/manual-admin-campaign-20260526-04247-04248/manual-admin-rebaseline-readiness/summary.json` |
| installed update/rollback | `PASS` | `artifacts/manual-admin-campaign-20260526-04247-04248/lifecycle/product-update-rollback/summary.json` |
| dedicated clean-host Windows Update | `PASS`, `KB5087545`, UBR `5139` | `artifacts/manual-admin-campaign-20260526-04247-04248/clean-host-windows-update/summary.json` |
| Burn install/repair/remove | `PASS` | `artifacts/manual-admin-campaign-20260526-04247-04248/burn-bootstrapper-lifecycle/summary.json` |
| MSIX build/install/update/remove | `PASS` | `artifacts/msix-package-lifecycle-smoke-20260526-04247-04248/summary.json` |
| installed runtime ops summary | `PASS` | `artifacts/manual-admin-campaign-20260526-04247-04248/installed-runtime-ops-summary/summary.json` |

Descriptor `manual-admin-campaign-descriptor-20260526-04247-04248-closed`는
`runner_count=6`, `missing_count=0`, `not_pass_count=0`, `overall_status=pass`를 기록했다.
Clean-host runner는 Windows Update 후 heartbeat `NoContact`/CPU idle 상태에서
`automatic_recovery_performed=true`, `recovery_actions=1`로 한 번 recovery를 수행했고,
이후 PowerShell Direct 재접속, baseline install, target update, rollback, final service
health를 모두 PASS로 닫았다.

## Installed Current-card 재확인

Closure 후 설치본 Web/TUI/CLI current-card smoke는
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md`와
`artifacts/installed-operator-surface-current-card-20260526-04248-manual-admin/summary.json`에서
PASS로 재확인했다. Ops summary는
`manual_admin.latest_package_pair.package_pair=0.42.47-admin-smoke -> 0.42.48-admin-smoke`,
`descriptor_batch_id=manual-admin-campaign-descriptor-20260526-04247-04248-closed`,
`missing_count=0`, `not_pass_count=0`를 노출했다.

## 경계

이 campaign은 internal admin-smoke manual-admin evidence다. Public trusted signing, public
stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
이전 `0.42.45-admin-smoke -> 0.42.47-admin-smoke` campaign은 historical predecessor로
보존한다.
