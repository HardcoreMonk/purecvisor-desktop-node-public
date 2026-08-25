# ADR-0008: Hyper-V QoS Mutation Policy

상태: 적용 중
일자: 2026-05-26

## 결정 마커

```text
DESKTOP_NODE_HYPERV_QOS_MUTATION_POLICY_DECISION: installed-package-fullgate-actual-vm-manual-admin-closed
phase: Phase 2 Hyper-V QoS Mutation Policy
implementation_status: installed-actual-vm-fullgate-and-manual-admin-closed
adr0007_boundary_change: false
cli_storage_mutation_command: pcvcli vm blkio-set
cli_network_mutation_command: pcvcli vm bandwidth-set
api_storage_preview_route: POST /api/v1/vms/{vm}/qos/storage/preview
api_storage_apply_route: POST /api/v1/vms/{vm}/qos/storage
api_network_preview_route: POST /api/v1/vms/{vm}/qos/network/preview
api_network_apply_route: POST /api/v1/vms/{vm}/qos/network
preview_contract: hyperv-qos-mutation-preview.v1
apply_job_contracts: vm.qos.storage.set, vm.qos.network.set
rollback_contract_required: true
readback_after_apply_required: true
web_tui_direct_control: opened-phase3-qos-only
web_tui_direct_control_phase3: pass-0.42.48-package-fullgate-current-card-manual-admin-closed
host_mutation_code_path: implemented-wmi-storage-iops-and-network-port-bandwidth
host_mutation_performed: true
package_build_performed: 0.42.47-admin-smoke
phase3_package_build_performed: 0.42.48-admin-smoke
installed_actual_vm_smoke: pass-installed-cli-qos-mutation-04247
full_admin_host_mutation_gate: pass-full-admin-host-mutation-gate-20260526-04247
phase3_full_admin_host_mutation_gate: pass-full-admin-host-mutation-gate-20260526-04248
phase3_installed_current_card: pass-installed-operator-surface-current-card-04248
manual_admin_package_pair: closed-0.42.45-to-0.42.47
phase3_manual_admin_package_pair: closed-0.42.47-to-0.42.48
manual_admin_campaign_evidence: docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04245-04247.md
phase3_manual_admin_campaign_evidence: docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04247-04248.md
value_boundary_hardening_decision: api-cli-preflight-reject-invalid-ranges
value_boundary_hardening_status: pass-code-level-next-package-required
value_boundary_hardening_evidence: docs/ga-ready/evidence/hyperv-qos-mutation-value-hardening-code-level-2026-05-29.md
value_boundary_problem_codes: PCV_VM_QOS_STORAGE_RANGE_INVALID, PCV_VM_QOS_NETWORK_RANGE_INVALID
next_product_payload_gate: 0.42.59-admin-smoke-package-fullgate-manual-admin
public_release: not-claimed
```

## 맥락

ADR-0007은 `vm blkio-get`, `vm bandwidth`, `vm guest-agent-status`,
`vm guest-ping`을 Hyper-V readback-first semantics로 승격했지만 `vm blkio-set`,
switch port bandwidth mutation, guest execution은 닫아 두었다. 그 이유는 Linux
`pcvctl`의 `blkio-set`이 cgroup/libvirt throttle 의미를 갖고 있고, Windows Hyper-V
제품에서는 Storage QoS와 network adapter bandwidth policy가 별도 의미를 갖기 때문이다.

이번 ADR은 Phase 2의 구현 착수 조건과 product route를 고정한다. 현재 payload는 preview
route, queued apply route, Hyper-V native adapter WMI code path, PCVCLI
`--dry-run|--yes` UX를 포함한다. 0.42.47-admin-smoke 기준 package build, full admin host
mutation gate, 실제 VM 대상 설치본 PCVCLI storage/network QoS mutation smoke와
manual-admin package-pair closure는 닫혔다. Phase 3는 같은 route contract를 Web/TUI direct
control surface로 열었고 0.42.48-admin-smoke package/fullgate/current-card/manual-admin
package-pair closure까지 PASS했다.

2026-05-29 follow-up은 value-boundary hardening으로 닫는다. Storage/network QoS preview와
apply는 음수, `1,000,000,000` 초과, `minimum > maximum` 값을 Local API/CLI에서 먼저
거절한다. Invalid preview는 native adapter를 호출하지 않고, invalid apply는 queued job을
생성하지 않는다. Rollback/manual restore에 쓰는 `0` 값은 계속 유효하다.

## 결정

Hyper-V QoS mutation은 Linux cgroup/libvirt 호환 명령이 아니라 Desktop Node Hyper-V 전용
정책으로 정의한다.

1. Storage QoS는 `pcvcli vm blkio-set <vm>` 이름을 유지하되, payload는 Hyper-V
   disk/storage policy를 명시한다.
2. Network QoS mutation은 기존 readback 명령 `pcvcli vm bandwidth <vm>`을 오염시키지 않고
   `pcvcli vm bandwidth-set <vm>`로 분리한다.
3. 모든 mutation은 `--dry-run` preview를 먼저 제공한다.
4. apply는 queued job으로만 실행하고, apply response에는 이전 정책과 rollback plan을
   evidence-safe 형태로 남긴다.
5. apply 이후에는 `blkio-get` 또는 `bandwidth` readback으로 적용 결과를 확인해야 한다.
6. Web/TUI direct control은 Phase 2 backend policy와 installed mutation smoke가 닫히기 전까지
   열지 않는다.

## CLI 후보

```text
pcvcli vm blkio-set <vm> --disk <path-or-id> --maximum-iops N [--minimum-iops N] [--dry-run] [--yes]
pcvcli vm bandwidth-set <vm> --adapter <name-or-id> --maximum-kbps N [--minimum-kbps N] [--dry-run] [--yes]
```

Linux 호환 alias는 아직 열지 않는다. 예를 들어 `--read-bps`, `--write-bps`,
`--read-iops`, `--write-iops` 같은 Linux/libvirt throttle flag는 Hyper-V policy mapping이
명확해지기 전까지 `PCV_CLI_BACKEND_NOT_EXPOSED` 또는 command-specific validation error로
닫는다.

## API 후보

| Route | 의미 |
| --- | --- |
| `POST /api/v1/vms/{vm}/qos/storage/preview` | storage QoS dry-run preview |
| `POST /api/v1/vms/{vm}/qos/storage` | storage QoS queued apply |
| `POST /api/v1/vms/{vm}/qos/network/preview` | network QoS dry-run preview |
| `POST /api/v1/vms/{vm}/qos/network` | network QoS queued apply |
| `GET /api/v1/vms/{vm}/blkio` | apply 이후 storage readback |
| `GET /api/v1/vms/{vm}/bandwidth` | apply 이후 network readback |

Preview route는 host mutation을 수행하지 않는다. Apply route는 `vm.qos.storage.set` 또는
`vm.qos.network.set` queued job만 반환한다.

## Code-level 적용 범위

2026-05-26 source payload 기준 구현된 범위는 아래와 같다.

- Contract DTO: `hyperv-qos-mutation-preview.v1`,
  `hyperv-qos-mutation-apply-evidence.v1`.
- Local API: storage/network preview route와 queued apply route.
- Hyper-V domain/native adapter: `vm.qos.storage.preview`, `vm.qos.network.preview`,
  `vm.qos.storage.set`, `vm.qos.network.set`.
- Native WMI apply path: storage는 `Msvm_StorageAllocationSettingData`의
  `IOPSLimit`/`IOPSReservation`, network는 `Msvm_EthernetSwitchPortBandwidthSettingData`의
  `Limit`/`Reservation` 변경 code path. 기존 bandwidth feature가 없는 VM port는
  `AddFeatureSettings`로 feature setting을 추가한다.
- PCVCLI: `vm blkio-set ... --dry-run|--yes`,
  `vm bandwidth-set ... --dry-run|--yes`.
- Value-boundary hardening: Local API preview/apply와 PCVCLI가
  `PCV_VM_QOS_STORAGE_RANGE_INVALID` / `PCV_VM_QOS_NETWORK_RANGE_INVALID`를
  command-specific/API problem-details로 반환하며 invalid payload를 native adapter 또는
  queued job으로 넘기지 않는다.
- Runtime policy: QoS preview native probe와 QoS apply native mutation operation을 표시.

아래 범위는 Phase 2 종료 후 Phase 3/후속 gate로 분리됐다.

- Phase 3 Web/TUI direct mutation control: `0.42.48-admin-smoke` package/fullgate/current-card PASS.
- `0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair closure: PASS.

## Preview Contract

`hyperv-qos-mutation-preview.v1`은 최소한 아래 필드를 포함한다.

```yaml
contract: hyperv-qos-mutation-preview.v1
mode: dry-run
provider: hyperv
request_id: req-...
actor: local-api-operator
vm:
  id: <vm-id>
  name: <vm-name>
storage:
  target_disk: <path-or-id>
  current_policy: <redacted-current-policy-or-null>
  proposed_policy:
    minimum_iops: <number-or-null>
    maximum_iops: <number-or-null>
  supported: <true-or-false>
  unsupported_reason: <stable-code-or-null>
network:
  adapter: <name-or-id-or-null>
  current_policy: <redacted-current-policy-or-null>
  proposed_policy:
    minimum_kbps: <number-or-null>
    maximum_kbps: <number-or-null>
  supported: <true-or-false>
  unsupported_reason: <stable-code-or-null>
validation:
  requires_admin: true
  live_vm_allowed: <true-or-false>
  restart_required: <true-or-false>
  host_mutation_performed: false
rollback_plan:
  previous_policy_captured: <true-or-false>
  rollback_operation: vm.qos.storage.rollback-or-vm.qos.network.rollback
readback_routes:
  - GET /api/v1/vms/{vm}/blkio
  - GET /api/v1/vms/{vm}/bandwidth
```

Preview payload는 secret, local username, credential, internal host detail을 노출하지 않는다.

## Rollback/Readback Gate

Apply job은 아래 evidence를 남겨야 한다.

- `previous_policy`: redacted previous value 또는 `unset`
- `applied_policy`: redacted applied value
- `rollback_plan`: previous policy restore 또는 policy remove 방식
- `readback_before`: apply 직전 readback 요약
- `readback_after`: apply 직후 readback 요약
- `audit`: actor, request id, operation id, timestamp, target VM, redacted args

Rollback command는 Phase 2 첫 implementation에서 직접 route로 열지 않을 수 있다. 다만 apply
job evidence에는 사람이 동일 정책을 되돌릴 수 있는 rollback descriptor가 반드시 있어야 하며,
manual-admin campaign에서는 rollback 또는 manual restore를 readback으로 확인한다.

## 검증 Gate

Phase 2 구현은 아래 순서로만 닫는다.

1. Contract/DTO unit test: preview payload, validation error, redaction, unsupported flag.
2. Hyper-V domain planner test: host mutation 없는 policy planning과 previous policy capture.
3. API route test: preview는 mutation 없음, apply는 queued job, error는 problem-details.
4. CLI test: `blkio-set`, `bandwidth-set`, `--dry-run`, command-specific usage.
5. Actual VM admin smoke: storage/network QoS apply, readback, rollback 또는 restore. `0.42.47-admin-smoke` PASS.
6. Full admin host mutation gate. `full-admin-host-mutation-gate-20260526-04247` PASS.
7. Manual-admin package-pair closure. `0.42.45-admin-smoke -> 0.42.47-admin-smoke` PASS.
8. Installed Web/TUI/CLI current-card evidence. `0.42.48-admin-smoke` Phase 3 current-card PASS.
9. Phase 3 manual-admin package-pair closure. `0.42.47-admin-smoke -> 0.42.48-admin-smoke` PASS.

## 경계

- ADR-0007의 readback-first 결정은 `vm blkio-get`, `vm bandwidth`, guest-service readback 명령에 대해 유지된다.
- `vm blkio-set`과 `vm bandwidth-set`은 설치본 actual VM smoke, full admin host mutation gate,
  manual-admin package-pair closure까지 PASS했다.
- Web/TUI mutation button은 Phase 3에서 QoS direct control 범위로 열렸고 0.42.48
  package/fullgate/current-card/manual-admin package-pair closure PASS까지 확인했다.
- Guest Execution / Guest Channel은 ADR-0009 후보의 security boundary가 닫히기 전까지 열지 않는다.
- Public trusted signing, winget public submission, public stable installer URL, external stable
  publication은 이 ADR 범위가 아니다.
