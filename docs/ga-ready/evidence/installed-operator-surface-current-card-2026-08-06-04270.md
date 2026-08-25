# Installed operator surface current-card 2026-08-06 `0.42.70`

evidence_id: `installed-operator-surface-current-card-2026-08-06-04270`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.70-admin-smoke`
installed_manifest_version: `0.42.70-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260806-04270`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260806-04270/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260806-04270`
clean_package_msi_sha256: `b28e18763ac01137039a9bcfafe0c151945304c8449e307b0412038d6726c86c`
operational_fullgate_msi_sha256: `90aeda60633ec7e6d32d88f71cbea2b2d5bb54eff205cf49d51cd894b44d8165`
clean_package_payload_aggregate_sha256: `e5bf399740afa6f858a9e2e5fb03317e2588bf7e78eb9342c6f5a58dc6df2a94`
operational_fullgate_payload_aggregate_sha256: `625a08ce4fcc4435c2ffa9af6804dbffc9c4b87450ea4b0613b1df52cb217f99`
provenance_commit: `e91389880febdfb3c1ba430f97c84c2f7e006591`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false-read-only-smoke-after-full-gate`
latest_manual_admin_package_pair: `0.42.69-admin-smoke -> 0.42.70-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260805-04269-04270-closed`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

- Active surface: Web Console과 PCVCLI만 설치본 PASS. `pcvtui.exe`는 존재하지 않는다.
- CLI: `--protected-token-file`로 `host status`, `runtime policy`, `network inventory` 모두 exit `0`
- Web: `/`, `/pcv-config.js` 모두 HTTP `200`
- Service: `PureCVisorDesktopNode`, `Running`, `Automatic`
- Cleanup: 남은 `pcv-spike-*` 검증 VM `0`, `PureCVisor` 방화벽 규칙 `0`, 임시 Event Log source 없음
- Secret observation: 캡처한 CLI 출력에서 token/password 값이 관측되지 않았고, protected token
  파일 내용이 artifact로 새어나오지 않음을 실측 확인해 `secret_observed=false`

## Anchor 승격

이 current-card로 operational anchor를 `0.42.69-admin-smoke`에서 `0.42.70-admin-smoke`로
승격한다. 세 평면이 모두 정렬된다.

| 평면 | 승격 전 | 승격 후 |
| --- | --- | --- |
| canonical anchor | `0.42.69-admin-smoke` | `0.42.70-admin-smoke` |
| 설치본 | `0.42.70-admin-smoke` | `0.42.70-admin-smoke` (변경 없음) |
| manual-admin closure | `0.42.69 -> 0.42.70` | 변경 없음 |

`0.42.69` 승격 때와 달리 설치본은 이미 `0.42.70`이었다. manual-admin campaign의
`installed-product-update-rollback`이 호스트를 target에 남겼기 때문이며, 이 승격은 anchor를
설치본에 맞춰 세 평면의 정렬을 완성한다. 2026-08-05 이후 anchor/설치본/closure가 동시에
같은 버전을 가리키는 첫 상태다.

## Nonclaims

- 이 evidence는 internal admin-smoke 범위이며 public trusted signing 또는 external stable
  publication evidence가 아니다.
- read-only smoke이며 이 문서 자체는 host mutation을 수행하지 않았다. 선행 host mutation은
  `full-admin-host-mutation-gate-20260806-04270`가 소유한다.
