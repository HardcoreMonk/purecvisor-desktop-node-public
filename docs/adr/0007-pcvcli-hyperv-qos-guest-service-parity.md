# ADR-0007: PCVCLI Hyper-V QoS와 Guest Service parity 경계

상태: 적용
일자: 2026-05-20

## 결정 마커

```text
DESKTOP_NODE_PCVCLI_HYPERV_QOS_GUEST_SERVICE_PARITY_DECISION: hyperv-semantic-readback-first
pcvcli_qos_guest_service_scope_lock: closed-code-level
first_code_level_package_candidate: 0.42.39-admin-smoke
supported_mutation: vm.limit
supported_readbacks: vm.blkio-get, vm.bandwidth, vm.guest-agent-status, vm.guest-ping
unsupported_linux_semantics: linux-blkio-set-flags, vm.guest-agent-ensure-channel, vm.guest-exec
qos_mutation_followup_adr: docs/adr/0008-hyperv-qos-mutation-policy.md
linux_cgroup_qemu_guest_agent_claim: not-claimed
installed_cli_targeted_smoke: docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md
web_tui_qos_guest_readback_decision: implemented-readback-surface-no-direct-control
web_tui_qos_guest_readback_evidence: docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md
next_product_payload_package_candidate: 0.42.41-admin-smoke
next_product_payload_package_chain_status: closed-manual-admin-package-pair-04240-04241
post_pr169_next_product_payload_package_candidate: 0.42.42-admin-smoke
post_pr169_package_chain_decision: not-run-no-product-payload-change-current-0.42.41-admin-smoke
host_mutation_gate_required_for_package: true
public_release: not-claimed
```

## 맥락

Linux `pcvctl`의 잔여 명령에는 cgroup/libvirt/qemu guest agent 의미가 섞여 있다.
Desktop Node는 Windows Hyper-V Local API 제품이므로 동일한 command 이름을 노출하더라도
Linux 의미를 그대로 구현했다고 주장할 수 없다. 2026-05-20
`pcvcli-linux-parity-remaining-slice`는 이 영역을 scope-lock 후보로 분리했다.

## 결정

Desktop Node는 Linux command name 중 Hyper-V 제품 의미로 안전하게 재해석 가능한 부분만
승격한다.

| 명령 | 결정 | 제품 의미 |
| --- | --- | --- |
| `pcvcli vm limit <vm> --cpu N [--memory-mb MB]` | 지원 | 기존 Hyper-V resource mutation provider를 통해 vCPU/startup memory 변경 job을 queue한다. Linux cgroup limit과 1:1 호환을 주장하지 않는다. |
| `pcvcli vm blkio-get <vm>` | 지원 | Hyper-V inventory의 disk/storage readback을 반환한다. Linux blkio throttle 값은 주장하지 않는다. |
| `pcvcli vm bandwidth <vm>` | 지원 | Hyper-V network adapter inventory readback을 반환한다. Linux traffic shaping semantics는 주장하지 않는다. |
| `pcvcli vm guest-agent-status <vm>` | 지원 | Hyper-V Integration Services readiness/readback을 반환한다. qemu guest agent 상태로 주장하지 않는다. |
| `pcvcli vm guest-ping <vm>` | 지원 | VM state 기반 guest service readiness readback을 반환한다. credentialless guest heartbeat 검증으로 주장하지 않는다. |
| Linux/libvirt `blkio-set` 세부 flag | 미지원 | `--read-bps`, `--write-bps`, `--read-iops`, `--write-iops` 같은 Linux/libvirt blkio mutation 의미는 제품 경계 밖이다. Hyper-V QoS mutation은 ADR-0008이 별도로 소유한다. |
| `pcvcli vm guest-agent-ensure-channel <vm>` | 미지원 | qemu guest agent channel 생성 의미가 제품 경계 밖이다. |
| `pcvcli vm guest-exec <vm>` | 미지원 | guest credential/audit/secret redaction 경계가 별도 ADR 없이는 닫히지 않는다. |

## 결과

- Runtime policy는 `vm.blkio-get`, `vm.bandwidth`, `vm.guest-agent-status`,
  `vm.guest-ping`을 native read operation으로 노출한다.
- Runtime policy는 `vm.limit`을 native queued mutation으로 노출한다.
- Local API는 다음 route를 소유한다.
  - `GET /api/v1/vms/{vm}/blkio`
  - `GET /api/v1/vms/{vm}/bandwidth`
  - `GET /api/v1/vms/{vm}/guest-agent/status`
  - `GET /api/v1/vms/{vm}/guest-agent/ping`
  - `POST /api/v1/vms/{vm}/limit`
- Hyper-V adapter는 readback payload에 Linux 호환 불가 flag를 명시한다.
  `linux_blkio_compatible=false`, `linux_bandwidth_compatible=false`,
  `qemu_guest_agent=false`, `guest_heartbeat_verified=false`를 포함한다.
- `vm.limit`은 기존 `SetMemory`/`SetVcpu` provider 경계를 재사용한다.

## 검증

이 ADR 자체는 host mutation을 실행하지 않는다. Code-level 검증은 아래 테스트가 소유한다.

```powershell
dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore --filter DesktopNodeCliCommandCatalogTests
dotnet test src\DesktopNode.Contracts.Tests\DesktopNode.Contracts.Tests.csproj --no-restore --filter RuntimePolicyContractTests
dotnet test src\DesktopNode.Api.Tests\DesktopNode.Api.Tests.csproj --no-restore --filter "ApiHandlerAdapterContractTests|HyperVDomainContractTests|ApiRuntimePolicyRequestProcessorTests"
```

제품 payload가 바뀌었으므로 설치본 evidence는 후속 `0.42.39-admin-smoke` package
chain, full admin host mutation gate, manual-admin package-pair, installed Web/TUI/CLI
current-card가 닫는다.

2026-05-21 후속 targeted smoke
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`는
설치본 `pcvcli`로 실제 VM을 생성해 `vm limit`, `vm blkio-get`, `vm bandwidth`,
`vm guest-agent-status`, `vm guest-ping`을 다시 확인했다. 이 후속은 docs/tools/evidence-only
변경이므로 `0.42.40-admin-smoke` package chain을 열지 않는다. Web/TUI direct control 또는
readback panel을 추가하는 경우에만 Operator Surface product payload 변경으로 간주하고
`0.42.40-admin-smoke` package chain을 연다.

2026-05-21 Web/TUI Operator Surface 후속
`docs/ga-ready/evidence/web-tui-qos-guest-readback-surface-2026-05-21.md`는 선택된 VM의
QoS/guest readback panel을 Web Console과 TUI에 추가한 code-level evidence다. 이 변경은
제품 payload 변경이므로 `0.42.40-admin-smoke` package chain을 열었고
`0.42.39-admin-smoke -> 0.42.40-admin-smoke` manual-admin package-pair closure로 닫았다.
이후 설치본 TUI row projection blocker fix는 `0.42.41-admin-smoke` package chain과
`0.42.40-admin-smoke -> 0.42.41-admin-smoke` manual-admin package-pair closure로 닫았다.
PR #169 post-merge public-boundary PASS 이후에는 이 ADR의 제품 의미나 Web/TUI surface가
바뀌지 않았으므로 `0.42.42-admin-smoke` package chain은 열지 않는다.
다만 Web/TUI는 `vm.limit`, `blkio-set`, guest channel 생성, guest command 실행 같은 direct
mutation/control을 제공하지 않는다. Web/TUI는 네 read-only route
`GET /api/v1/vms/{vm}/blkio`, `GET /api/v1/vms/{vm}/bandwidth`,
`GET /api/v1/vms/{vm}/guest-agent/status`, `GET /api/v1/vms/{vm}/guest-agent/ping`만
선택 VM readback으로 노출한다.

## 경계

이 결정은 internal admin-smoke 제품 경계다. Public trusted signing, winget public
submission, public stable installer URL, external stable publication은 주장하지 않는다.
