# Installed operator surface current-card 2026-08-05 `0.42.69`

evidence_id: `installed-operator-surface-current-card-2026-08-05-04269`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.69-admin-smoke`
installed_manifest_version: `0.42.69-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260805-04269`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260805-04269/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260805-04269`
clean_package_msi_sha256: `7a3729224d4a66df9a28b9e8f4f2649949361d9ca66bfce34d04caed390e198b`
operational_fullgate_msi_sha256: `07e30ca90d96747f5cc5f5e76a2a2556198356cf51db354356f795d9d3cc1a3a`
clean_package_payload_aggregate_sha256: `a6a2408a3e0b3bbe293a83b7133f1ef45aa97da034aaca9fce8b7bda2856070b`
operational_fullgate_payload_aggregate_sha256: `d0ee0d0593f28603fd59daa90e9fa7a2fd24316f957423f5393db4f82d730db3`
provenance_commit: `7236b813d6a4f594abb8e126b2b5dfb2ad56c1e9`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false-read-only-smoke-after-full-gate`
latest_manual_admin_package_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260529-04258-04259-closed`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

- Active surface: Web Console과 PCVCLI만 설치본 PASS. `pcvtui.exe`는 존재하지 않는다.
- CLI: `--protected-token-file`로 `host status`, `runtime policy`, `network inventory` 모두 exit `0`
- Web: `/`, `/pcv-config.js` 모두 HTTP `200`
- Service: `PureCVisorDesktopNode`, `Running`, `Automatic`
- Cleanup: 남은 검증 VM `0`, 방화벽 규칙 `0`, 임시 Event Log source 없음
- Secret observation: token/password 값을 출력하거나 기록하지 않았고 `secret_observed=false`

## Anchor 승격

이 current-card로 operational anchor를 `0.42.65-admin-smoke`에서 `0.42.69-admin-smoke`로
승격한다. 세 평면 중 anchor와 설치본이 정렬됐다.

| 평면 | 승격 전 | 승격 후 |
| --- | --- | --- |
| canonical anchor | `0.42.65-admin-smoke` | `0.42.69-admin-smoke` |
| 설치본 | `0.42.68-admin-smoke` | `0.42.69-admin-smoke` |
| manual-admin closure | `0.42.58 -> 0.42.59` | 변경 없음 |

manual-admin closure는 여전히 `0.42.58 -> 0.42.59`다. 다만 설치본이 `0.42.69`가 됐으므로
`0.42.69`를 baseline으로 하는 pair는 `installed == baseline` 전제를 만족한다. 실제 closure는
`PCV_MANUAL_ADMIN_BASELINE_HOST`와 `PCV_MANUAL_ADMIN_CREDENTIAL_REF` 구성이 선행 조건이다.

## Nonclaims

- 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 external stable
  publication evidence가 아니다.
- manual-admin package-pair closure를 승격하지 않는다.
