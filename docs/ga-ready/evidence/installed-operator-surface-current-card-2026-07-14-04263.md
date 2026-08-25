# Installed operator surface current-card 2026-07-14 0.42.63

evidence_id: `installed-operator-surface-current-card-2026-07-14-04263`
result: `PASS`
evidence_scope: `internal-admin-smoke-only`
version: `0.42.63-admin-smoke`
installed_manifest_version: `0.42.63-admin-smoke`
operator_surfaces: `web,cli`
tui_present: `false`
artifact_root: `artifacts/installed-operator-surface-current-card-20260714-04263`
artifact_summary: `artifacts/installed-operator-surface-current-card-20260714-04263/summary.json`
fullgate_batch: `full-admin-host-mutation-gate-20260714-04263`
clean_package_msi_sha256: `d2f2fff7fb400647135d96449f36704af2d080e1a6a97a551354290cdf1a6f04`
operational_fullgate_msi_sha256: `6a520e52042bdca5d55b73a4614aa0ebddaf54d576ddf60739146c2ad6784589`
clean_package_payload_aggregate_sha256: `19f80f3e0b849d180a3e62461742a8a2ab7371e632dbfecfc8fad28bf59721f4`
operational_fullgate_payload_aggregate_sha256: `be53d348199ee7fab95b3b4148d805d81aa98f80aa330cc376e94216e6db210e`
provenance_commit: `9a020dec285d4fbbfe161ca2d31242f305cde572`
cli_exit_zero_count: `3`
web_http_200_count: `2`
service_state: `Running/Automatic`
secret_observed: `false`
host_mutation_performed: `false-read-only-smoke-after-fullgate`
latest_manual_admin_package_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
latest_manual_admin_descriptor: `manual-admin-campaign-descriptor-20260529-04258-04259-closed`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## Installed current-card

- Active surface: Web Console과 PCVCLI만 설치본 PASS; `pcvtui.exe`는 존재하지 않음
- CLI: explicit `--protected-token-file`로 `host status`, `runtime policy`, `network inventory`
  모두 exit `0`
- Web: `/`, `/pcv-config.js` 모두 HTTP `200`
- Network: `Default Switch`와 `WSL (Hyper-V firewall)` 모두 `internal`,
  `allow_management_os=true`
- Preferred physical LAN: `[redacted-private-endpoint]`, Realtek PCIe 2.5GbE, default gateway 있음
- Service: `PureCVisorDesktopNode`, `Running`, `Automatic`
- Secret observation: token/password 값을 출력하거나 기록하지 않았고 `secret_observed=false`

Summary contract는 schema `1`, `operator_surfaces=[web,cli]`, `tui_present=false`,
`service_state=Running`, `service_start_mode=Automatic`, `secret_observed=false`, `ok=true`다.
TUI count나 TUI smoke field는 포함하지 않는다. 0.42.62 Web/TUI/CLI current-card는
historical TUI predecessor로 보존한다.

이 evidence는 installed internal admin-smoke operator surface 증거이며 public trusted signing
또는 외부 stable publication을 주장하지 않는다.
