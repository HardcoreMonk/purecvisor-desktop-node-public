# Installed CLI usage-trim smoke 2026-05-25 0.42.43

evidence_id: `installed-cli-usage-trim-smoke-2026-05-25-04243`
result: `PASS`
scope: `installed-pcvcli-command-usage-errors-without-global-usage-block`
version: `0.42.43-admin-smoke`
artifact_root: `artifacts/installed-cli-usage-trim-smoke-20260525-04243`
summary: `artifacts/installed-cli-usage-trim-smoke-20260525-04243/summary.json`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-25-04243.md`
installed_manifest_version: `0.42.43-admin-smoke`
pcvcli_resolved_from_machine_path: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
direct_vm_get_exit: `2`
direct_vm_get_error: `PCV_CLI_USAGE|Use: vm get <vm>.`
direct_contains_usage_block: `false`
interactive_vm_get_exit: `0`
interactive_vm_get_error: `PCV_CLI_USAGE|Use: vm get <vm>.`
interactive_contains_usage_block: `false`
service_batch_evidence_root_configured: `true`
ops_summary_batch_evidence_status: `available`
ops_summary_batch_id: `full-admin-host-mutation-gate-20260522-04241`
current_full_admin_host_mutation: `0.42.41-admin-smoke`
current_manual_admin_package_pair: `0.42.40-admin-smoke -> 0.42.41-admin-smoke`
host_mutation_performed: `true`
host_mutation_scope: `msi-update-to-0.42.43-admin-smoke-and-repair-installed-artifacts-batch-evidence-root`
full_admin_host_mutation_rerun: `not-run-in-this-package-smoke`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 smoke는 설치본 `pcvcli`에서 command-specific usage error가 전체 help block을 출력하지 않는지 확인한다. `pcvcli vm get`과 interactive `vm get` 모두 `PCV_CLI_USAGE|Use: vm get <vm>.` 한 줄만 stderr에 남겼고, `Usage:` / `pcvcli [--api URL]` block은 출력하지 않았다.

## 확인

| 항목 | 값 |
| --- | --- |
| installed manifest | `0.42.43-admin-smoke` |
| command resolution | `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe` |
| direct command | `pcvcli vm get`, exit `2`, `Usage:` block absent |
| interactive command | `pcvcli --interactive --no-color`, input `vm get`, exit `0`, `Usage:` block absent |
| service repair | `--batch-evidence-root` restored to repo `artifacts` root |
| ops summary | exit `0`, batch evidence `available`, latest batch `full-admin-host-mutation-gate-20260522-04241` |

## 경계

이 evidence는 CLI usage rendering smoke다. Full admin host mutation gate, clean-host, Burn, MSIX, manual-admin package-pair lifecycle은 이번 run에서 실행하지 않았고 후속 package-pair gate로 남긴다. Public trusted signing, public stable installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
