# PCVCLI Linux command coverage matrix 2026-05-19 0.42.32

evidence_id: `pcvcli-linux-command-coverage-matrix-2026-05-19-04232`
result: `PASS_DESKTOP_NODE_HYPERVISOR_SERVICE_SURFACE_100_PERCENT`
scope: `pcvcli-linux-pcvctl-compatible-command-coverage`
target_version: `0.42.32-admin-smoke`
source_repository: `HardcoreMonk/purecvisor`
source_commit: `abc76d364b716ea4bfca322e914bf8803f013bf6`
source_cli_file: `src/cli/purecvisorctl.c`
target_cli: `src/DesktopNode.Cli`
installed_cli: `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`
installed_smoke: `docs/ga-ready/evidence/installed-pcvcli-neon-vm-list-smoke-2026-05-19-04232.md`
public_release: `not-claimed`

이 문서는 Linux 기반 PureCVisor `pcvctl` command table을 Desktop Node Windows
Hyper-V 제품 경계에 맞춰 재분류한 coverage matrix다. 판정 기준은 Linux 전체
runtime surface 이식률이 아니라, Desktop Node Local API가 제공하는 Hyper-V
service surface를 `pcvcli.exe`가 모두 호출할 수 있는지다.

## 판정

`pcvcli.exe`는 Desktop Node Windows Hyper-V Local API가 제공하는 hypervisor
service surface를 100% 호출할 수 있다. 이 판정은 아래 first-class command와
Linux-compatible alias가 실제 API route로 매핑되는지, 그리고 설치본
`0.42.32-admin-smoke`에서 자동 token 기반 smoke가 통과했는지를 기준으로 한다.

Linux `purecvisor`의 KVM/libvirt/LXC/ZFS/OVS/OVN/DPDK/SR-IOV/cloud/template/backup
surface는 Desktop Node 제품 범위가 아니므로 미구현 결함으로 보지 않는다.

## Desktop Node API coverage

| Desktop Node operation | PCVCLI command | API route | Coverage |
| --- | --- | --- | --- |
| `host.status` | `pcvcli host status` | `GET /api/v1/host/status` | `covered` |
| `runtime.policy` | `pcvcli runtime policy` | `GET /api/v1/runtime/policy` | `covered` |
| `ops.summary` | `pcvcli ops summary` | `GET /api/v1/ops/summary` | `covered` |
| `network.inventory` | `pcvcli network inventory`, `pcvcli network list` | `GET /api/v1/network/inventory` | `covered` |
| `vm.list` | `pcvcli vm list` | `GET /api/v1/vms` | `covered` |
| `vm.get` | `pcvcli vm get <vm>` | `GET /api/v1/vms/{vm}` | `covered` |
| `vm.create` | `pcvcli vm create --name ...`, `pcvcli vm create <name> --vcpu ...` | `POST /api/v1/vms` | `covered` |
| `vm.start` | `pcvcli vm start <vm>` | `POST /api/v1/vms/{vm}/start` | `covered` |
| `vm.stop` alias | `pcvcli vm stop <vm>` | `POST /api/v1/vms/{vm}/poweroff` | `covered-as-poweroff` |
| `vm.shutdown` | `pcvcli vm shutdown <vm>`, `pcvcli vm guest-shutdown <vm>` | `POST /api/v1/vms/{vm}/shutdown` | `covered` |
| `vm.poweroff` | `pcvcli vm poweroff <vm>` | `POST /api/v1/vms/{vm}/poweroff` | `covered` |
| `vm.restart` | `pcvcli vm restart <vm>` | `POST /api/v1/vms/{vm}/restart` | `covered` |
| `vm.console` | `pcvcli vm console <vm>`, `pcvcli vm vnc <vm>` | `GET /api/v1/vms/{vm}/console` | `covered` |
| `vm.delete` | `pcvcli vm delete <vm> --yes` | `DELETE /api/v1/vms/{vm}` | `covered-with-confirmation` |
| `checkpoint.list` | `pcvcli vm checkpoint list <vm>`, `pcvcli snapshot list <vm>` | `GET /api/v1/vms/{vm}/checkpoints` | `covered` |
| `checkpoint.create` | `pcvcli vm checkpoint create <vm> --name ...`, `pcvcli snapshot create <vm> --name ...` | `POST /api/v1/vms/{vm}/checkpoints` | `covered` |
| `checkpoint.restore` | `pcvcli vm checkpoint restore <vm> <checkpoint>`, `pcvcli snapshot rollback <vm> <checkpoint>` | `POST /api/v1/vms/{vm}/checkpoints/{checkpoint}/restore` | `covered` |
| `checkpoint.delete` | `pcvcli vm checkpoint delete <vm> <checkpoint>`, `pcvcli snapshot delete <vm> <checkpoint>` | `DELETE /api/v1/vms/{vm}/checkpoints/{checkpoint}` | `covered` |
| `job.list` | `pcvcli job list` | `GET /api/v1/jobs` | `covered` |
| `job.get` | `pcvcli job get <job_id>` | `GET /api/v1/jobs/{job_id}` | `covered` |
| `job.cancel` | `pcvcli job cancel <job_id>` | `POST /api/v1/jobs/{job_id}/cancel` | `covered` |
| `job.retry` | `pcvcli job retry <job_id>` | `POST /api/v1/jobs/{job_id}/retry` | `covered` |
| `diagnostics.bundle.create` | `pcvcli diagnostics bundle create` | `POST /api/v1/diagnostics/bundles` | `covered` |
| `diagnostics.bundle.download` | `pcvcli diagnostics bundle download <bundle_id> --output <path>` | `GET /api/v1/diagnostics/bundles/{bundle_id}/download` | `covered` |

## Linux `pcvctl` compatibility classification

| Linux `pcvctl` area | PCVCLI status | 근거 |
| --- | --- | --- |
| `vm create/list/start/stop/delete` | `supported` | Hyper-V VM lifecycle API에 직접 매핑한다. |
| `vm guest-shutdown` | `supported` | Hyper-V guest shutdown route에 매핑한다. Integration service가 준비되지 않은 VM은 API가 structured failure를 반환한다. |
| `vm rename/pause/resume` | `code-level-supported-queued-mutation` | 2026-05-19 후속 backend slice에서 Local API queued mutation, Hyper-V native adapter, PCVCLI route로 승격했다. 설치본 host mutation evidence는 다음 admin-smoke package에서 닫는다. |
| `vm vnc` | `supported-alias` | Desktop Node console/noVNC session lookup route에 매핑한다. |
| `vm memory-stats/cpu-stats` | `code-level-supported-read-only` | 2026-05-19 후속 backend slice에서 Hyper-V VM inventory summary 기반 read-only API/CLI route로 승격했다. 설치본 evidence는 다음 admin-smoke package에서 닫는다. |
| `network list` | `supported-alias` | Hyper-V switch inventory route에 매핑한다. |
| `snapshot list/create/rollback/delete` | `supported-alias` | Desktop Node checkpoint route에 매핑한다. |
| `job list` | `supported` | Desktop Node queued job snapshot route에 매핑한다. |
| `job get/cancel/retry` | `desktop-node-extension` | 운영자 job handling을 위해 Desktop Node API surface로 제공한다. |
| `diagnostics bundle create/download` | `desktop-node-extension` | evidence bundle 운영을 위해 Desktop Node API surface로 제공한다. |
| `vm set-memory/set-vcpu/disk-resize` | `manual-admin-gate-required` | live/offline VM resource mutation은 별도 `vm-resource-mutation` MANUAL-ADMIN gate와 installed host mutation evidence 전에는 제품 route로 노출하지 않는다. |
| `vm limit/eject/delete-status` | `out-of-product-scope-or-backend-not-exposed` | 현재 Desktop Node Local API가 해당 Linux cgroup 또는 virtual media/delete progress route를 제품 claim으로 제공하지 않는다. |
| `vm guest-agent-status/guest-agent-ensure-channel/guest-ping/guest-exec` | `out-of-product-scope` | Linux qemu guest agent contract이며 Desktop Node Hyper-V backend에 직접 대응되는 API가 없다. |
| `vm blkio-set/blkio-get/bandwidth` | `out-of-product-scope` | Linux cgroup/libvirt/network shaping surface다. |
| `nic/iso/storage/device/container/ovn/dpdk/sriov/template/backup/alert/agent/batch/prometheus/webhook/security/security-group/gpu/config/grpc/cloud` | `out-of-product-scope` | Linux Single Edge runtime object이며 Windows Desktop Node Hyper-V product boundary에 포함하지 않는다. |

`docs/ga-ready/evidence/pcvcli-backend-command-gap-slice-2026-05-19.md`는 위
`out-of-product-scope-or-backend-not-exposed` 항목을 다시 나눠, Hyper-V backend/API
추가 후 승격 가능한 `vm eject/delete-status`, 별도 manual-admin gate가 필요한
`vm set-memory/set-vcpu/disk-resize`, Linux-only로 유지할 항목을 분리한다.
`vm memory-stats/cpu-stats`는 후속 backend slice에서 code-level read-only 명령으로,
`vm rename/pause/resume`은 code-level queued mutation 명령으로 승격했다. 남은 일반
backend 후보는 `pcvcli` `Available Commands`에 노출하지 않고 직접 호출 시
`PCV_CLI_BACKEND_NOT_EXPOSED`로 거절한다. manual-admin gate 후보는
`PCV_CLI_MANUAL_ADMIN_GATE_REQUIRED`로 거절한다.

## 설치본 smoke 근거

`docs/ga-ready/evidence/installed-pcvcli-neon-vm-list-smoke-2026-05-19-04232.md`는
전역 `pcvcli.exe`가 기본 protected token file을 자동 사용하고 실제 Hyper-V VM을
대상으로 create/start/list/get/poweroff/delete를 통과했음을 기록한다.

주요 확인 값은 다음과 같다.

| 항목 | 값 |
| --- | --- |
| installed version | `0.42.32-admin-smoke` |
| global path | `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe` |
| real VM smoke name | `pcv-neon-list-04232` |
| `pcvcli --json host status` | exit `0` |
| `pcvcli --json vm list` before/after cleanup | exit `0`, final VM count `0` |
| `pcvcli vm list` | neon table에 실제 VM row 출력 |
| `pcvcli --no-color vm list` | `SYS_UUID | ENTITY_ID | LIFELINE` table에 실제 VM row 출력 |

## 경계

이 matrix는 internal admin-smoke evidence다. Public trusted signing, external stable
publication, winget submission, Linux 전체 `pcvctl` command surface 이식 완료를
주장하지 않는다.
