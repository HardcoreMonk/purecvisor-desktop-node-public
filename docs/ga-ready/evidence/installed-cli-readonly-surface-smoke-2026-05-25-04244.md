# Installed CLI read-only surface smoke 2026-05-25 0.42.44

evidence_id: `installed-cli-readonly-surface-smoke-2026-05-25-04244`
result: `PASS`
scope: `installed-pcvcli-runtime-ops-network-table-rendering`
version: `0.42.44-admin-smoke`
artifact_root: `artifacts/installed-cli-readonly-surface-smoke-20260525-04244`
summary: `artifacts/installed-cli-readonly-surface-smoke-20260525-04244/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04244.md`
installed_manifest_version: `0.42.44-admin-smoke`
pcvcli_resolved_from_machine_path: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
direct_runtime_policy: `pass-table`
direct_ops_summary: `pass-table`
direct_network_inventory: `pass-table`
direct_network_list: `pass-table`
interactive_runtime_policy: `pass-table`
interactive_ops_summary: `pass-table`
interactive_network_inventory: `pass-table`
interactive_network_list: `pass-table`
ok_summary_fallback_observed: `false`
service_batch_evidence_root_configured: `true`
host_mutation_performed: `true`
host_mutation_scope: `msi-update-to-0.42.44-admin-smoke-and-repair-installed-artifacts-batch-evidence-root`
full_admin_host_mutation_rerun: `not-run-in-this-package-smoke`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 smoke는 설치본 `pcvcli.exe`에서 `runtime policy`, `ops summary`, `network inventory`, `network list`가 실제 data table을 출력하는지 확인한다. Direct command와 redirected interactive REPL 모두 `Runtime Policy`, `Ops Summary`, `Network Inventory` table을 출력했고, `ok=True | operation=...` 요약 fallback은 관찰되지 않았다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.44-admin-smoke` |
| command resolution | `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe` |
| direct commands | `runtime policy`, `ops summary`, `network inventory`, `network list`, all exit `0` |
| REPL commands | `runtime policy`, `ops summary`, `network inventory`, `network list`, exit `0` |
| table headers | `Runtime Policy`, `Ops Summary`, `Network Inventory` |
| forbidden fallback | `ok=True | operation=...` absent |
| service repair | `--batch-evidence-root` restored to repo `artifacts` root |

## 경계

이 evidence는 CLI read-only surface rendering smoke다. Full admin host mutation gate, clean-host, Burn, MSIX, manual-admin package-pair lifecycle은 이번 run에서 실행하지 않았고 후속 package-pair gate로 남긴다. Public trusted signing, public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
