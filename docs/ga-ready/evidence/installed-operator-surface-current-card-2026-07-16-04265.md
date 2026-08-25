# Installed operator surface current-card 2026-07-16 0.42.65

evidence_id: `installed-operator-surface-current-card-2026-07-16-04265`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.65-admin-smoke`
installed_manifest_version: `0.42.65-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260716-04265`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260716-04265/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260716-04265`
clean_package_msi_sha256: `5709edb0d5f265393c8690c212dd6d1f61873f7cbbaa110b1654a2e380e6b748`
operational_fullgate_msi_sha256: `9786e1327db676f541961981f08cbd1c2ba53382aac127e2d9f404f9ffba5c30`
clean_package_payload_aggregate_sha256: `3b4fefb3c03c1a70ba804e959931bdec0ee36923139a84602e85be69e96e251a`
operational_fullgate_payload_aggregate_sha256: `5eecd064b38da2a45afdf6957f9e43a26077927af8dee8478bc2823f9b1f8b28`
provenance_commit: `4855947fe0199cedc978e8b40ffb45e96ced6876`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false-read-only-smoke-after-functional-validation`
latest_manual_admin_package_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260529-04258-04259-closed`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

- Active surface: Web Console과 PCVCLI만 설치본 PASS; `pcvtui.exe`는 존재하지 않음
- CLI: explicit protected token file로 `host status`, `runtime policy`, `network inventory` 모두
  exit `0`
- Web: `/`, `/pcv-config.js` 모두 HTTP `200`
- Network: `Default Switch`와 `WSL (Hyper-V firewall)` 모두 `internal`,
  `allow_management_os=true`
- Service: `PureCVisorDesktopNode`, `Running`, `Automatic`
- Functional correctness: 실제 VM QoS 변환, disk shrink guard/expansion, cleanup PASS
- Cleanup: 남은 검증 VM `0`
- Secret observation: token/password 값을 출력하거나 기록하지 않았고 `secret_observed=false`

Summary contract는 schema `1`, `operator_surfaces=[web,cli]`, `tui_present=false`, manifest root
`tui`/`paths.tui_exe` 부재, `service_state=Running`, `service_start_mode=Auto`,
`secret_observed=false`, `ok=true`다. 0.42.64 CLI/Web actual-VM functional current-card는 immediate
historical predecessor이며 0.42.62 Web/TUI/CLI current-card는 historical TUI predecessor다.

이 evidence는 installed internal admin-smoke operator surface 증거이며 public trusted signing
또는 외부 stable publication을 주장하지 않는다.
