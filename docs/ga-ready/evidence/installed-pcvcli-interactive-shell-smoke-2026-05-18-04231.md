# Installed PCVCLI interactive shell smoke 2026-05-18 0.42.31

evidence_id: `installed-pcvcli-interactive-shell-smoke-2026-05-18-04231`
result: `PASS-with-tab-completion-capture-note`
scope: `installed-pcvcli-pcvtui-operator-smoke`
version: `0.42.31-admin-smoke`
package_build_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-18-04231.md`
artifact_root: `artifacts/installed-pcvcli-interactive-shell-smoke-20260518-04231`
summary: `artifacts/installed-pcvcli-interactive-shell-smoke-20260518-04231/summary.json`
msi_install_exit_code: `0`
installed_manifest_version: `0.42.31-admin-smoke`
service_state: `Running`
machine_path_contains_install_dir: `true`
pcvcli_resolved_from_machine_path: `true`
pcvtui_resolved_from_machine_path: `true`
protected_token_file_exists: `true`
token_source: `default-protected-token-file-auto-discovery`
token_value_observed: `false`
host_mutation_performed: `true`
full_admin_host_mutation_gate: `not-run`
manual_admin_package_pair: `not-run`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 목적

`0.42.31-admin-smoke` 설치본에 PCVCLI interactive shell과 자동 token 흐름이 실제
반영됐는지 확인한다.

## 설치 결과

| 항목 | 결과 |
| --- | --- |
| MSI install/update | exit `0` |
| Installed manifest | `0.42.31-admin-smoke` |
| Service | `PureCVisorDesktopNode` `Running` |
| Machine PATH | `C:\Program Files\PureCVisor\DesktopNode` 포함 |
| `pcvcli` resolve | `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe` |
| `pcvtui` resolve | `C:\Program Files\PureCVisor\DesktopNode\pcvtui.exe` |
| Protected token file | present |

## Smoke 결과

| 확인 항목 | 명령 | 결과 |
| --- | --- | --- |
| no-args REPL 진입 | `pcvcli`, stdin `exit` | exit `0`, banner와 `(pcv) >` prompt 출력 |
| long interactive flag | `pcvcli --interactive --no-color`, stdin `help`, `exit` | exit `0`, command table 출력 |
| short interactive flag | `pcvcli -i --no-color`, stdin `exit` | exit `0` |
| 자동 token host status | `pcvcli host status` | exit `0`, `operation=host.status` |
| 자동 token VM list | `pcvcli --json vm list` | exit `0`, JSON `ok=true`, `operation=vm.list` |
| REPL host status dispatch | REPL 안에서 `--json host status` | exit `0`, JSON `ok=true`, `operation=host.status` |
| REPL VM list dispatch | REPL 안에서 `--json vm list` | exit `0`, JSON `ok=true`, `operation=vm.list` |
| TUI runtime smoke | `pcvtui --smoke-once --no-color runtime` | exit `0` |

Smoke 명령은 `--token`, `--token-file`, `--token-env`, `--protected-token-file` 없이
실행했다. CLI/TUI는 기본 protected token file auto discovery를 사용했고 token 값은
stdout/stderr/evidence에 기록하지 않았다.

## Tab completion 확인

Code-level Tab completion은
`src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs`의
`CompletesKnownInteractiveCommandPrefixes`에서 `host`, `network l`, `snapshot roll`
prefix를 검증한다. 설치본 ConPTY smoke도 시도했으나 Windows terminal control stream이
capture pipe와 현재 console로 갈라져 completion suffix를 안정적으로 파일에 남기지
못했다. 따라서 이 evidence의 installed PASS claim은 REPL 진입, help, exit,
one-shot parser reuse, 자동 token dispatch에 한정하고, 실제 사람이 보는 terminal
Tab completion은 code-level test와 동일 구현 경로를 사용한다.

## 경계

이 smoke는 설치본 MSI update/install과 operator command 확인을 수행했지만 full admin
host mutation gate 전체와 `0.42.30-admin-smoke -> 0.42.31-admin-smoke` manual-admin
package-pair campaign은 실행하지 않았다. Current full-gate/manual-admin ledger 승격은
별도 campaign evidence가 필요하다.
