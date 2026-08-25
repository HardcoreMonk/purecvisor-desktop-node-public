# PCVCLI Linux CLI parity 점검

status: pass-desktop-node-hyperv-cli-route-coverage
date: 2026-05-18
source_repository: HardcoreMonk/purecvisor
source_commit: abc76d364b716ea4bfca322e914bf8803f013bf6
source_cli_file: src/cli/purecvisorctl.c
target_cli: src/DesktopNode.Cli/pcvcli
public_release: not-claimed

## 목적

Linux 기반 PureCVisor의 `pcvctl` CLI command table을 확인하고, Windows Desktop Node
`pcvcli.exe`가 Desktop Node Hyper-V Local API surface를 빠짐없이 호출할 수 있는지
점검했다.

## Linux `pcvctl` 분석 요약

원본 `src/cli/purecvisorctl.c`는 JSON-RPC method를 호출하는 command router다. 주요
route group은 `vm`, `nic`, `iso`, `snapshot`, `monitor`, `network`, `storage`,
`device`, `container`, `ovn`, `dpdk`, `sriov`, `auth`, `template`, `backup`,
`alert`, `agent`, `job`, `batch`, `prometheus`, `webhook`, `security`,
`security-group`, `gpu`, `config`, `grpc`, `cloud`다.

Desktop Node Windows Hyper-V 제품과 직접 대응되는 핵심 command는 다음 범위다.

- `vm create/list/start/stop/delete`
- `vm guest-shutdown`
- `vm vnc`
- `network list`
- `snapshot list/create/rollback/delete`
- `job list`

Linux-only 또는 현재 Desktop Node backend가 제공하지 않는 command는 PCVCLI에
제품 claim으로 추가하지 않는다. 여기에는 KVM/libvirt 세부 조작, LXC, ZFS zvol,
OVS/OVN, DPDK, SR-IOV, cloud import/export, backup/template/security-group 같은
Linux Single Edge surface가 포함된다.

## Desktop Node Hyper-V API coverage

Desktop Node runtime policy와 API contract 기준 Hyper-V service route는 다음과 같다.

| Operation | PCVCLI command |
|-----------|----------------|
| `host.status` | `pcvcli host status` |
| `network.inventory` | `pcvcli network inventory`, `pcvcli network list` |
| `vm.list` | `pcvcli vm list` |
| `vm.get` | `pcvcli vm get <vm>` |
| `vm.console` | `pcvcli vm console <vm>`, `pcvcli vm vnc <vm>` |
| `vm.create` | `pcvcli vm create --name ...`, `pcvcli vm create <name> --vcpu ...` |
| `vm.start` | `pcvcli vm start <vm>` |
| `vm.shutdown` | `pcvcli vm shutdown <vm>`, `pcvcli vm guest-shutdown <vm>` |
| `vm.poweroff` | `pcvcli vm poweroff <vm>`, `pcvcli vm stop <vm>` |
| `vm.restart` | `pcvcli vm restart <vm>` |
| `vm.delete` | `pcvcli vm delete <vm> --yes` |
| `checkpoint.list` | `pcvcli vm checkpoint list <vm>`, `pcvcli snapshot list <vm>` |
| `checkpoint.create` | `pcvcli vm checkpoint create <vm> --name ...`, `pcvcli snapshot create <vm> --name ...` |
| `checkpoint.restore` | `pcvcli vm checkpoint restore <vm> <checkpoint>`, `pcvcli snapshot rollback <vm> <checkpoint>` |
| `checkpoint.delete` | `pcvcli vm checkpoint delete <vm> <checkpoint>`, `pcvcli snapshot delete <vm> <checkpoint>` |

Job control route는 queued mutation 관찰/운영을 위해 기존 `pcvcli job
list|get|cancel|retry`로 유지한다.

## 판정

PCVCLI는 Desktop Node Windows Hyper-V Local API가 제공하는 hypervisor service
surface를 100% 호출할 수 있다. 이 판정은 Desktop Node backend contract 기준이며,
Linux `purecvisor` 전체 CLI surface의 100% 이식 또는 Linux-only backend 기능 제공을
의미하지 않는다.

## 검증

- `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliCommandCatalogTests"`: 36 passed
- `dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~ApiHandlerAdapterContractTests|FullyQualifiedName~ApiRuntimePolicyRequestProcessorTests|FullyQualifiedName~HyperVDomainContractTests"`: 155 passed
- `dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore --filter "FullyQualifiedName~RuntimePolicyContractTests"`: 6 passed
- `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore`: 55 passed
- `git diff --check`: passed
