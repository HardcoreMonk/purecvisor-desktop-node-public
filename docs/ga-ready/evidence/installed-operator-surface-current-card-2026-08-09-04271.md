# Installed operator surface current-card 2026-08-09 `0.42.71`

evidence_id: `installed-operator-surface-current-card-2026-08-09-04271`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.71-admin-smoke`
installed_manifest_version: `0.42.71-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260809-04271`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260809-04271/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260808-04271`
clean_package_msi_sha256: `ebb621ada454b70ce367af6cc9a59e11966c0e2299b1f75976b03adacdd24ad5`
operational_fullgate_msi_sha256: `4748cc7453ac85178830c179533e7236ed4d3eb15ddb3f968e1dbd4934c27156`
clean_package_payload_aggregate_sha256: `4a333d60c8f9e10ea4c356f58913e8893d43be644c4736e7ed272e03c3f5a0af`
operational_fullgate_payload_aggregate_sha256: `6f325c245808d5d3bb6ead60184cb9c0c2065d79552e22b673ba1be7a010ca16`
provenance_commit: `80f69f31464ce07b2c9eca19211adf1232ea75f6`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false-read-only-smoke-after-full-gate`
latest_manual_admin_package_pair: `0.42.70-admin-smoke -> 0.42.71-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260808-04270-04271-closed`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

- Active surface: Web Console과 PCVCLI만 설치본 PASS. `pcvtui.exe`는 존재하지 않는다.
- CLI: elevated 세션에서 `--protected-token-file`로 `host status`, `runtime policy`,
  `network inventory` 모두 exit `0` (`--format json`).
- Web: `/`, `/pcv-config.js` 모두 HTTP `200`.
- Service: `PureCVisorDesktopNode`, `Running`, `Automatic`.
- Cleanup: 남은 `pcv-spike-*` 검증 VM `0`.
- Secret observation: 캡처한 CLI 출력에서 token/password 값이 관측되지 않았고
  `secret_observed=false`.

## Anchor 승격

이 current-card로 operational anchor를 `0.42.70-admin-smoke`에서 `0.42.71-admin-smoke`로
승격한다.

| 평면 | 승격 전 | 승격 후 |
| --- | --- | --- |
| canonical anchor | `0.42.70-admin-smoke` | `0.42.71-admin-smoke` |
| 설치본 | `0.42.71-admin-smoke` | 변경 없음 |
| manual-admin closure | `0.42.70 -> 0.42.71` | 변경 없음 |

## Nonclaims

- internal admin-smoke 범위이며 public trusted signing 또는 external stable publication
  evidence가 아니다.
- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다. 선행 host mutation은
  `full-admin-host-mutation-gate-20260808-04271`가 소유한다.
