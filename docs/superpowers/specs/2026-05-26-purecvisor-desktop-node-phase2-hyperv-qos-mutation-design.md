# Phase 2 Hyper-V QoS Mutation Design

## 목적

이 문서는 확장 로드맵의 Phase 2 `Hyper-V QoS Mutation Policy`를 구현 가능한 수준으로
쪼개기 위한 design spec이다. 선행 기준은 다음이다.

- `docs/superpowers/specs/2026-05-25-purecvisor-desktop-node-extension-roadmap-design.md`
- `docs/adr/0007-pcvcli-hyperv-qos-guest-service-parity.md`
- `docs/adr/0008-hyperv-qos-mutation-policy.md`

## 결정 마커

```text
DESKTOP_NODE_PHASE2_HYPERV_QOS_MUTATION_DESIGN_DECISION: preview-queued-apply-readback-rollback
phase2_first_surface: CLI-and-Local-API
phase2_web_tui_direct_control: deferred-to-phase3
storage_mutation_command: pcvcli vm blkio-set
network_mutation_command: pcvcli vm bandwidth-set
preview_contract: hyperv-qos-mutation-preview.v1
apply_job_contracts: vm.qos.storage.set, vm.qos.network.set
rollback_descriptor_required: true
actual_vm_smoke_required: true
host_mutation_performed: false
package_build_performed: false
public_release: not-claimed
```

## 제품 의미

Phase 2는 Linux cgroup/libvirt `blkio-set`을 그대로 이식하지 않는다. Desktop Node의 제품 의미는
Hyper-V provider가 설명할 수 있는 Storage QoS와 VM network adapter bandwidth policy다.

| Surface | Phase 2 의미 |
| --- | --- |
| `pcvcli vm blkio-set` | Hyper-V storage QoS mutation 후보. Disk/VHDX/VM policy level은 implementation에서 하나로 선택한다. |
| `pcvcli vm bandwidth-set` | Hyper-V VM network adapter bandwidth mutation 후보. 기존 `bandwidth` readback은 그대로 유지한다. |
| Local API preview | host mutation 없는 validation과 rollback/readback preview |
| Local API apply | queued job으로만 mutation 수행 |
| Web/TUI | Phase 2에서는 readback만 유지. Direct control은 Phase 3 |

## API Contract

Phase 2의 후보 route는 다음이다.

```text
POST /api/v1/vms/{vm}/qos/storage/preview
POST /api/v1/vms/{vm}/qos/storage
POST /api/v1/vms/{vm}/qos/network/preview
POST /api/v1/vms/{vm}/qos/network
GET /api/v1/vms/{vm}/blkio
GET /api/v1/vms/{vm}/bandwidth
```

Preview route는 `hyperv-qos-mutation-preview.v1`을 반환한다. Apply route는
`vm.qos.storage.set` 또는 `vm.qos.network.set` job id를 반환하고, job artifact는 previous
policy와 rollback descriptor를 포함한다.

## Hyper-V Domain 설계

Phase 2 implementation은 Hyper-V domain 내부에 planner와 executor를 분리한다.

| Component | 책임 |
| --- | --- |
| `DesktopNodeHyperVQosMutationPlanner` | VM/disk/adapter lookup, current policy readback, validation, preview payload 생성 |
| `DesktopNodeHyperVQosMutationExecutor` | queued job에서 policy apply, previous policy capture, rollback descriptor 생성 |
| `DesktopNodeHyperVQosMutationReadback` | apply 전후 readback normalization |
| `DesktopNodeHyperVQosMutationAudit` | redacted audit payload 생성 |

Planner는 host mutation을 수행하지 않는다. Executor만 host mutation을 수행할 수 있으며 반드시
job runtime에서 호출한다.

## CLI UX

CLI는 command-specific usage만 출력한다. 전체 Usage block을 다시 출력하지 않는다.

```text
pcvcli vm blkio-set <vm> --disk <path-or-id> --maximum-iops N [--minimum-iops N] [--dry-run] [--yes]
pcvcli vm bandwidth-set <vm> --adapter <name-or-id> --maximum-kbps N [--minimum-kbps N] [--dry-run] [--yes]
```

`--dry-run`은 preview table/json을 반환한다. `--yes` 없는 apply는 interactive shell에서는 confirm을
요구하고 non-interactive mode에서는 command-specific error를 반환한다.

## Evidence Contract

Actual VM smoke는 아래를 증명해야 한다.

1. Preview는 `host_mutation_performed=false`.
2. Apply job은 `succeeded` terminal state.
3. Readback before/after가 artifact에 남는다.
4. Rollback 또는 manual restore 후 final readback이 baseline과 일치한다.
5. Artifact에 credential, token, secret-like string이 없다.
6. Full admin host mutation gate와 manual-admin package-pair가 닫힌다.

## Phase 3 연결

Phase 3 Web/TUI Direct Control은 Phase 2가 설치본 mutation evidence로 닫힌 뒤에만 시작한다.
Phase 3의 첫 UI surface는 Phase 2에서 승인된 mutation만 열 수 있으며, preview, confirm, queued
job progress, cancel/refresh, readback, rollback descriptor link가 없는 버튼은 금지한다.

## Non-goals

- Linux cgroup/libvirt QoS 호환 claim.
- Guest Execution / Guest Channel.
- Web/TUI direct mutation button.
- noVNC target host/port mutation.
- Public trusted signing 또는 external stable publication.
