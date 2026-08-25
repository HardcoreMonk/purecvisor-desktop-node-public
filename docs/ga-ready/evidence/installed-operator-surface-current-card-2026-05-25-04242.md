# Installed Operator Surface current-card 2026-05-25 0.42.42

evidence_id: `installed-operator-surface-current-card-2026-05-25-04242`
result: `PASS`
scope: `installed-web-tui-cli-current-card-after-pcvcli-snapshot-surface-removal`
version: `0.42.42-admin-smoke`
artifact_root: `artifacts/installed-operator-surface-current-card-20260525-04242`
summary: `artifacts/installed-operator-surface-current-card-20260525-04242/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04242.md`
installed_manifest_version: `0.42.42-admin-smoke`
current_full_admin_host_mutation: `0.42.41-admin-smoke`
batch_evidence_batch_id: `full-admin-host-mutation-gate-20260522-04241`
current_manual_admin_package_pair: `0.42.40-admin-smoke -> 0.42.41-admin-smoke`
cli_help_snapshot_surface: `pass`
cli_interactive_help_snapshot_surface: `pass`
cli_top_level_snapshot_rejected: `pass`
cli_host_status: `pass`
cli_json_vm_list: `pass`
cli_ops_summary: `pass`
tui_smoke_runtime: `pass`
web_index: `pass`
web_config: `pass`
api_runtime_policy_boundary: `401-pass`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
token_value_observed: `false`
password_value_observed: `false`
host_mutation_performed: `true`
host_mutation_scope: `msi-update-to-0.42.42-admin-smoke-and-repair-installed-artifacts-batch-evidence-root`
full_admin_host_mutation_rerun: `not-run-in-this-package-smoke`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 current-card는 `0.42.42-admin-smoke` 설치본에서 PCVCLI snapshot command surface 변경이 설치본 Web/TUI/CLI 운영자 여정과 충돌하지 않음을 확인한 기록이다. MSI update 후 service `PathName`의 `--batch-evidence-root`를 repo `artifacts` root로 복원해 ops summary가 최신 closed operational anchor인 `full-admin-host-mutation-gate-20260522-04241`를 `available`로 읽는지 함께 확인했다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.42-admin-smoke` |
| service | `PureCVisorDesktopNode`, `Running`, `Auto`, `--batch-evidence-root` configured to `artifacts` |
| command resolution | `pcvcli.exe`, `pcvtui.exe` both resolve from `C:\Program Files\PureCVisor\DesktopNode` |
| Web | `/` HTTP `200`, `/pcv-config.js` HTTP `200` |
| API boundary | unauthenticated `/api/v1/runtime/policy` returns auth boundary `401` |
| ops summary full gate | `full-admin-host-mutation-gate-20260522-04241`, status `available` |
| ops summary manual-admin | `0.42.40-admin-smoke -> 0.42.41-admin-smoke`, descriptor `manual-admin-campaign-descriptor-20260522-04240-04241-closed` |
| CLI help | top-level `pcvcli snapshot list|create|rollback|delete` absent, `pcvcli vm snapshot list|create|rollback|delete` present |
| CLI REPL help | no command row starts with `snapshot `, `vm snapshot list | List VM checkpoints` remains present |
| removed command behavior | `pcvcli snapshot list demo` exits `2` with `Unknown command group 'snapshot'` |
| TUI smoke | `pcvtui --smoke-once runtime` exit `0` |

Tab completion keypress itself is not automated by redirected stdin; the command completion contract is code-level verified by `DesktopNodeCliInteractiveShellTests.CompletesKnownInteractiveCommandPrefixes`.

## 경계

이 evidence는 0.42.42 package/update/current-card smoke다. Full admin host mutation gate, clean-host, Burn, MSIX, manual-admin package-pair lifecycle은 이번 run에서 실행하지 않았고 후속 package-pair gate로 남긴다. Public trusted signing, public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
