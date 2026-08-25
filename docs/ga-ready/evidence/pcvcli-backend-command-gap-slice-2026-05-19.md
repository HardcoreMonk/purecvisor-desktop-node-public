# PCVCLI backend command gap slice 2026-05-19

evidence_id: `pcvcli-backend-command-gap-slice-2026-05-19`
result: `PASS_CODE_LEVEL_SPLIT_STATS_LIFECYCLE_AND_04238_MEDIA_RESOURCE_PROMOTED`
scope: `pcvcli-linux-pcvctl-compatible-command-gap-triage`
base_evidence: `docs/ga-ready/evidence/pcvcli-linux-command-coverage-matrix-2026-05-19-04232.md`
current_installed_anchor: `0.42.38-admin-smoke`
next_remaining_slice: `docs/ga-ready/evidence/pcvcli-linux-parity-remaining-slice-2026-05-20.md`
host_mutation_performed: `false`
package_build_performed: `false`
public_release: `not-claimed`

이 문서는 Linux `pcvctl` command table 중 Desktop Node Hyper-V Local API가 아직
실동작 backend route로 제공하지 않는 후보를 다음 backend slice로 분리한다. 현재
`pcvcli.exe`는 Desktop Node가 제품 claim으로 제공하는 host/runtime/ops/network,
VM lifecycle, VM pause/resume/rename, console/noVNC, read-only VM stats,
checkpoint, job, diagnostics surface를 모두 호출할 수 있다. 아래 명령은 100%
coverage claim의 누락이 아니라, 별도 backend/API product decision이 필요한 후보,
manual-admin gate가 필요한 host mutation, 또는 Windows Desktop Node 제품 범위 밖
명령이다.

2026-05-19 후속 backend slice에서 `vm memory-stats`와 `vm cpu-stats`는
read-only Hyper-V route/API/CLI/help contract로 승격했다. 같은 slice에서
`vm rename/pause/resume`은 queued mutation route/API/job policy/Hyper-V adapter/CLI
contract로 승격했다. 설치본 package evidence는 다음 admin-smoke package에서 별도
current-card로 닫는다.

2026-05-20 `0.42.38-admin-smoke` slice에서 `vm eject`, `vm delete-status`,
`vm set-memory`, `vm set-vcpu`, `vm disk-resize`도 Local API, queued job, Hyper-V
native adapter, PCVCLI, Web/TUI operator route contract로 승격했다. 설치본 evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-20-04238.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-20-04238-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-20-04238.md`가
소유한다.

2026-05-20 ADR-0007 slice에서 `vm limit`, `vm blkio-get`, `vm bandwidth`,
`vm guest-agent-status`, `vm guest-ping`도 code-level route로 승격했다. 이 승격은
Linux cgroup/libvirt/qemu guest agent semantic 호환 claim이 아니라 Hyper-V resource
mutation/readback semantics로 제한된다. 설치본 evidence는 후속
`0.42.39-admin-smoke` package chain에서 닫는다.

## CLI 동작

`pcvcli`는 아래 후보를 `Available Commands`에 first-class command로 노출하지
않는다. 사용자가 Linux `pcvctl` 명령을 직접 입력하면 일반 오타용
`PCV_CLI_USAGE`가 아니라 `PCV_CLI_BACKEND_NOT_EXPOSED`로 거절한다. 단,
`vm set-memory/set-vcpu/disk-resize`는 일반 backend gap이 아니라 별도
`vm-resource-mutation` MANUAL-ADMIN gate가 필요한 후보이므로
`PCV_CLI_MANUAL_ADMIN_GATE_REQUIRED`로 거절한다. 이 동작은 operator가 "지원 명령을
잘못 입력한 경우", "다음 backend slice 후보를 호출한 경우", "manual-admin gate가
필요한 host mutation을 호출한 경우"를 구분하기 위한 code-level contract다.

## 다음 backend slice 후보

| 후보 | 현재 상태 | 다음 구현 조건 |
| --- | --- | --- |
| 일반 Desktop Node Hyper-V backend gap | `none-after-04238` | 0.42.38 기준 제품 claim surface의 남은 일반 backend gap은 없다. |

## 승격 완료 후보

| 후보 | 현재 상태 | 근거 |
| --- | --- | --- |
| `vm memory-stats/cpu-stats` | `code-level read-only promoted` | `RuntimePolicy.NativeProbeOperations`, Local API `GET /api/v1/vms/{vm}/memory-stats`, `GET /api/v1/vms/{vm}/cpu-stats`, Hyper-V native adapter, `pcvcli` command catalog와 interactive help에 반영했다. |
| `vm rename/pause/resume` | `code-level queued mutation promoted` | `RuntimePolicy.NativeMutationOperations`, Local API `POST /api/v1/vms/{vm}/rename|pause|resume`, job queue semantics, Hyper-V native adapter, `pcvcli` command catalog와 interactive help에 반영했다. |
| `vm eject/delete-status` | `0.42.38 promoted` | Local API `POST /api/v1/vms/{vm}/eject`, `GET /api/v1/vms/{vm}/delete-status`, Hyper-V media/delete provider, PCVCLI/Web/TUI route contract, installed current-card evidence로 닫았다. |
| `vm set-memory/set-vcpu/disk-resize` | `0.42.38 promoted` | resource mutation policy, validation, queued job contract, Hyper-V resource provider, PCVCLI/Web/TUI route contract, full admin host mutation evidence로 닫았다. |
| `vm limit` | `0.42.39 code-level promoted` | Hyper-V CPU/MEM resource mutation alias로 승격했다. Linux cgroup limit 호환은 주장하지 않는다. |
| `vm blkio-get/bandwidth` | `0.42.39 code-level readback promoted` | Hyper-V storage/network inventory readback으로 승격했다. Linux blkio/network shaping mutation은 주장하지 않는다. |
| `vm guest-agent-status/guest-ping` | `0.42.39 code-level readback promoted` | Hyper-V Integration Services readiness/readback으로 승격했다. qemu guest agent 또는 credentialless heartbeat claim은 하지 않는다. |

## MANUAL-ADMIN gate 후보

잔여 mutation 후보는 단순 CLI 누락이 아니라 `manual-admin-gate-required` 정책 판단이
필요한 영역으로 분류한다.

| 후보 | 현재 상태 | 다음 구현 조건 |
| --- | --- | --- |
| Hyper-V QoS policy parity mutation (`vm blkio-set`, bandwidth mutation) | `security-and-rollback-boundary-required` | readback은 승격했지만 host QoS mutation은 rollback/readback evidence와 product semantics가 더 필요하다. |
| Hyper-V guest service execution (`vm guest-agent-ensure-channel`, `vm guest-exec`) | `security-boundary-required` | qemu guest agent가 아니라 Hyper-V Guest Services/PowerShell Direct semantics로 재해석해야 하므로 credential/audit/permission contract가 선행되어야 한다. |

## 제품 범위 밖으로 유지

| Linux `pcvctl` area | 분류 | 근거 |
| --- | --- | --- |
| `vm guest-agent-ensure-channel/guest-exec` | `linux-qemu-guest-agent-out-of-product-scope` | qemu guest agent/libvirt channel 기반 명령이며 credential/audit/secret redaction boundary가 별도 필요하다. |
| `vm blkio-set` | `linux-only-out-of-product-scope` | libvirt/cgroup block I/O mutation surface이며 현재 Hyper-V Desktop Node 제품 경계 밖이다. |

## 다음 slice 진입 조건

다음 backend 명령을 first-class로 승격하려면 `docs/ga-ready/evidence/pcvcli-linux-parity-remaining-slice-2026-05-20.md`
의 scope-lock을 먼저 닫고, 같은 PR에서 아래 항목을 함께 닫아야 한다.

1. `RuntimePolicy.NativeProbeOperations` 또는 `NativeMutationOperations`에 operation 추가
2. `DesktopNodeApiRequestProcessor` route와 queued job semantics 추가
3. `DesktopNode.HyperV` adapter 구현과 unit/contract test 추가
4. `pcvcli` command catalog, formatter, interactive help, docs 갱신
5. Web Console/TUI operator surface가 같은 command intent를 설명하거나 명시적으로 제외
6. product payload 변경이면 새 admin-smoke package, full admin host mutation gate, manual-admin package-pair campaign 실행

이 slice는 code-level triage만 수행했고 host mutation, package build, public trusted
signing, external stable publication을 주장하지 않는다.
