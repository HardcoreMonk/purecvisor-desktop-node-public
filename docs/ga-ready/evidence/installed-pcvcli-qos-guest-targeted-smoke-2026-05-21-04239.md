# 설치본 PCVCLI QoS/guest targeted smoke 2026-05-21 0.42.39

evidence_id: `installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239`
result: `PASS`
scope: `installed-pcvcli-hyperv-qos-guest-targeted-smoke-after-pr164`
version: `0.42.39-admin-smoke`
tool: `packaging/windows-desktop-node/tools/Invoke-PcvInstalledCliQosGuestSmoke.ps1`
artifact_root: `artifacts/installed-cli-qos-guest-smoke-20260521-04239`
summary: `artifacts/installed-cli-qos-guest-smoke-20260521-04239/summary.json`
installed_pcvcli: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
installed_pcvcli_version: `0.42.39.0`
installed_pcvtui_version: `0.42.39.0`
token_source: `default-protected-token-file-auto-discovery`
token_value_observed: `false`
password_value_observed: `false`
host_mutation_performed: `true`
vm_name: `pcv-cli-qos-guest-d34eea84`
cleanup_vm_removed: `true`
cleanup_vm_root_removed: `true`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`
web_tui_qos_guest_readback_decision: `defer-direct-web-tui-control-no-product-payload-change`
unsupported_linux_semantics_decision: `keep-out-of-scope-without-new-adr`
next_product_payload_package_candidate: `0.42.40-admin-smoke`
package_chain_decision: `not-opened-no-product-payload-change-docs-tools-evidence-only-after-pr164`

이 evidence는 `0.42.39-admin-smoke` 설치본 `pcvcli`가 실제 Hyper-V VM을 대상으로
ADR-0007 QoS/guest-service command를 실행할 수 있음을 추가 확인한 targeted smoke다.
Full admin host mutation gate와 manual-admin package-pair closure는 이미
`0.42.39-admin-smoke`에서 닫혔고, 이 문서는 그 설치본 CLI command path를 더 좁게
재검증한다.

## 실행 결과

| 명령 | 결과 | 확인 |
| --- | --- | --- |
| `pcvcli --json host status` | PASS | Hyper-V feature enabled, VMMS running |
| `pcvcli --json vm create pcv-cli-qos-guest-d34eea84 ...` | PASS | job `succeeded`, managed VM 생성 |
| `pcvcli --json vm list` | PASS | 생성 VM row 확인 |
| `pcvcli --json vm get pcv-cli-qos-guest-d34eea84` | PASS | `operation=vm.get` |
| `pcvcli --json vm limit ... --cpu 1 --memory-mb 1024` | PASS | job result `action=limit` |
| `pcvcli --json vm blkio-get ...` | PASS | `operation=vm.blkio-get`, `linux_blkio_compatible=false` |
| `pcvcli --json vm bandwidth ...` | PASS | `operation=vm.bandwidth`, `linux_bandwidth_compatible=false` |
| `pcvcli --json vm guest-agent-status ...` | PASS | `operation=vm.guest-agent-status`, `qemu_guest_agent=false` |
| `pcvcli --json vm start ...` | PASS | job result `action=start` |
| `pcvcli --json vm guest-ping ...` | PASS | `operation=vm.guest-ping`, `guest_heartbeat_verified=false` |
| `pcvcli --json vm poweroff ...` | PASS | job result `action=poweroff` |
| `pcvcli --json vm delete ... --yes` | PASS | job result `action=delete` |
| `pcvcli --json vm list` | PASS | 삭제 후 VM 목록에서 대상 absent |

## Operator Surface 결정

Web/TUI에 QoS/guest-service direct control을 추가하지 않는다. 현 상태에서 Web/TUI는
ops summary current-card와 VM inventory/detail 중심의 운영자 surface를 유지하고,
QoS/guest-service command 실행은 CLI와 Local API route가 소유한다. Web/TUI에 직접
버튼, table, readback panel을 추가하는 일은 Operator Surface product payload 변경으로
분류하며, 그때 `0.42.40-admin-smoke` package chain을 연다.

## ADR-0007 경계 유지

`vm blkio-set`, `vm guest-agent-ensure-channel`, `vm guest-exec`는 별도 ADR 없이
지원하지 않는다. 현재 evidence는 Hyper-V disk/network/integration-service readback과
CPU/MEM resource mutation만 확인한다. Linux cgroup QoS, libvirt blkio mutation, qemu
guest agent channel/exec, credentialless guest heartbeat는 주장하지 않는다.

## Package Chain 결정

이번 변경은 문서, 증거, 설치본 smoke 도구 추가이며 설치 payload에는 포함되지 않는다.
따라서 `0.42.40-admin-smoke` package chain은 열지 않는다. 다음 Web/TUI/CLI/Host/API
product payload 변경이 발생하면 `0.42.40-admin-smoke`를 package candidate로 연다.

## Secret/cleanup

Smoke는 token 인자를 직접 전달하지 않고 default protected token file discovery를
사용했다. Bearer token, password, refresh token, JWT signing key 값은 stdout/stderr 또는
summary에 기록하지 않았다. 테스트 VM과 VM root는 smoke 종료 시 삭제했다.

## 경계

이 evidence는 internal admin-smoke 설치본 smoke다. Public trusted signing, public stable
installer URL, winget submission, 외부 stable publication은 주장하지 않는다.
