# Installed operator surface current-card 2026-07-15 0.42.64

evidence_id: `installed-operator-surface-current-card-2026-07-15-04264`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.64-admin-smoke`
installed_manifest_version: `0.42.64-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260715-04264-r2`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260715-04264-r2/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260715-04264`
clean_package_msi_sha256: `8ba9714995d153e97a84c90afcf01b3ab1a612a166089e764b7046aae46c1cb7`
operational_fullgate_msi_sha256: `540f5c5fc8bc78a7c07f950cf9c39002491e69308dc264112a42ad0b510f50bf`
clean_package_payload_aggregate_sha256: `d3070394a44d09d34b78a3c06b4e7f99a5bc266ba91306ae41dd1bacf611487f`
operational_fullgate_payload_aggregate_sha256: `d02aec33be7d8f12348e242336604bb453b63b5b0d2cc139f6ced1ef15287cc0`
provenance_commit: `a0491e39992093b9ad506619cfacb1675939d6a3`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
remaining_test_vm_count: `0`
secret_observed: `false`
host_mutation_performed: `false-read-only-smoke-after-fullgate`
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
- Cleanup: 남은 검증 VM `0`
- Secret observation: token/password 값을 출력하거나 기록하지 않았고 `secret_observed=false`

Summary contract는 schema `1`, `operator_surfaces=[web,cli]`, `tui_present=false`, manifest root
`tui`/`paths.tui_exe` 부재, `service_state=Running`, `service_start_mode=Auto`,
`secret_observed=false`, `ok=true`다. 0.42.63 CLI/Web current-card와 0.42.62 Web/TUI/CLI
current-card는 historical predecessor로 보존한다.

이 evidence는 installed internal admin-smoke operator surface 증거이며 public trusted signing
또는 외부 stable publication을 주장하지 않는다.
