# PCVCLI Hyper-V QoS/Guest Service slice 2026-05-20

evidence_id: `pcvcli-hyperv-qos-guest-service-slice-2026-05-20`
result: `PASS_CODE_LEVEL_AND_04239_PACKAGE_CHAIN_CLOSED`
scope: `pcvcli-linux-parity-hyperv-qos-guest-service-code-level`
adr: `docs/adr/0007-pcvcli-hyperv-qos-guest-service-parity.md`
base_evidence: `docs/ga-ready/evidence/pcvcli-linux-parity-remaining-slice-2026-05-20.md`
base_installed_anchor: `0.42.38-admin-smoke`
next_package_decision: `0.42.39-admin-smoke-required-after-merge`
package_chain_status: `closed-0.42.39-admin-smoke-pass`
package_evidence: `docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04239.md`
full_admin_host_mutation_evidence: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04239-hostmutation.md`
manual_admin_campaign_evidence: `docs/ga-ready/evidence/manual-admin-campaign-2026-05-20-04238-04239.md`
installed_current_card_evidence: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04239.md`
host_mutation_performed: `true`
package_build_performed: `true`
public_release: `not-claimed`

이 slice는 Linux `pcvctl` 잔여 명령 중 Desktop Node Hyper-V 제품 의미로 닫을 수 있는
부분을 code-level route로 승격했다. Linux cgroup/libvirt/qemu guest agent semantic을
그대로 구현했다고 주장하지 않고, Hyper-V readback과 기존 resource mutation provider
경계로 재해석한 범위만 first-class PCVCLI 명령으로 노출한다.

## 승격한 명령

| 명령 | Local API route | backend contract |
| --- | --- | --- |
| `pcvcli vm limit <vm> --cpu N [--memory-mb MB]` | `POST /api/v1/vms/{vm}/limit` | queued `vm.limit` mutation. 기존 Hyper-V memory/vCPU provider를 재사용한다. |
| `pcvcli vm blkio-get <vm>` | `GET /api/v1/vms/{vm}/blkio` | Hyper-V storage inventory readback. Linux blkio compatibility는 `false`다. |
| `pcvcli vm bandwidth <vm>` | `GET /api/v1/vms/{vm}/bandwidth` | Hyper-V network adapter inventory readback. Linux bandwidth shaping compatibility는 `false`다. |
| `pcvcli vm guest-agent-status <vm>` | `GET /api/v1/vms/{vm}/guest-agent/status` | Hyper-V Integration Services readiness readback. qemu guest agent claim은 `false`다. |
| `pcvcli vm guest-ping <vm>` | `GET /api/v1/vms/{vm}/guest-agent/ping` | VM state 기반 guest service readiness readback. credentialless heartbeat 검증 claim은 `false`다. |

## 미지원 유지

| 명령 | 상태 | 이유 |
| --- | --- | --- |
| `pcvcli vm blkio-set <vm>` | `PCV_CLI_BACKEND_NOT_EXPOSED` | Windows Storage QoS mutation 또는 VHDX policy는 별도 product semantics와 rollback/readback evidence가 필요하다. |
| `pcvcli vm guest-agent-ensure-channel <vm>` | `PCV_CLI_BACKEND_NOT_EXPOSED` | qemu guest agent channel 생성 의미가 Hyper-V 제품 경계 밖이다. |
| `pcvcli vm guest-exec <vm>` | `PCV_CLI_BACKEND_NOT_EXPOSED` | guest credential, audit log, command output redaction, permission boundary가 별도 ADR 없이는 닫히지 않았다. |

## 구현 변경

- `DesktopNodeCliCommandCatalog`와 interactive shell help/completion에 Hyper-V QoS/guest
  service 명령을 추가했다.
- `ApiHandlerAdapterContract`와 `DesktopNodeApiRequestProcessor`에 QoS/guest service
  route와 `QueueSetVmLimit` mutation contract를 추가했다.
- `RuntimePolicy` native read/mutation operation set에 `vm.blkio-get`, `vm.bandwidth`,
  `vm.guest-agent-status`, `vm.guest-ping`, `vm.limit`을 추가했다.
- `DesktopNode.HyperV` domain/provider/dispatch catalog와 native adapter에 readback
  payload, provider boundary, resource mutation dispatch를 추가했다.

## 검증

```powershell
dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter DesktopNodeCliCommandCatalogTests
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore --filter RuntimePolicyContractTests
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "ApiHandlerAdapterContractTests|HyperVDomainContractTests|ApiRuntimePolicyRequestProcessorTests"
```

결과는 모두 PASS다. 전체 solution, packaging documentation guard, package/fullgate/manual-admin
검증은 후속 `0.42.39-admin-smoke` chain에서 닫았다.

## 다음 package gate 판단

이번 변경은 API route, runtime policy, Hyper-V adapter, CLI command catalog/help,
문서 evidence가 모두 바뀐 product payload change다. 아래 package gate는 실행 완료됐다.

1. `0.42.39-admin-smoke` package build
2. 새 package 기준 full admin host mutation gate
3. `0.42.38-admin-smoke -> 0.42.39-admin-smoke` manual-admin package-pair descriptor/readiness/campaign
4. 설치본 `pcvcli host status`, `pcvcli --json vm list`, `pcvcli vm blkio-get|bandwidth|guest-agent-status|guest-ping`, `pcvcli vm limit` route smoke
5. installed Web/TUI/CLI current-card 갱신

이 evidence는 internal admin-smoke/code-level 범위이며 public trusted signing 또는
external stable publication을 주장하지 않는다.
