# PureCVisor Desktop Node 사용자 기능 사용 명세서

작성 기준: 2026-07-14

이 문서는 설치된 PureCVisor Desktop Node를 사용하는 사람이 어떤 기능을 어느 화면/도구에서 실행할 수 있는지, 실행 전제조건과 차단/실패 메시지가 어떻게 보여야 하는지를 정의한다. 세부 절차는 `docs/USER_GUIDE.md`, CLI 명령어 형식은 `docs/CLI_COMMAND_USAGE.md`, 운영 runbook과 host mutation gate는 `docs/OPERATIONS_GUIDE.md`를 따른다.

## 제품 범위

PureCVisor Desktop Node는 Windows 10/11 Pro/Enterprise + Hyper-V host를 로컬에서 관리하는 내부 사설망 전용 서비스다. ADR-0011에 따라 활성 사용자 표면은 Web Console과 PCVCLI이며 Local API/backend 기능은 그대로 유지한다.

| 항목 | 기준 |
|------|------|
| Web Console | `http://127.0.0.1/` |
| Web API | `http://127.0.0.1:7777/api/v1/...` |
| CLI | `C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe` |
| Service | `PureCVisorDesktopNode` |
| Product root | `C:\Program Files\PureCVisor\DesktopNode` |
| Data root | `%ProgramData%\PureCVisor\desktop-node` |
| Protected token file | `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` |

다음 항목은 사용자 기능 범위 밖이다.

- Linux `purecvisor-single`, `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime
- Public trusted signing, external stable publication, winget public submission, public stable installer URL
- Web Console/CLI에서 직접 수행하는 MSI install/repair/remove, firewall, trust-store, Event Log provider, LAN listener, update/rollback mutation
- 관리자 승인 없이 실행되는 host mutation gate

## 사용자 역할

| 역할 | 목적 | 허용 기능 |
|------|------|-----------|
| `viewer` | 상태 확인 | host/runtime/network/VM/job/diagnostic/console capability 조회 |
| `operator` | 일상 운영 | viewer 권한 + VM lifecycle/checkpoint/job/diagnostic/console handoff |
| `admin` | 설치본 운영 | operator 권한 + 별도 운영 runbook의 service/update/security gate |

계정이 구성되지 않은 기본 bootstrap 상태는 `no-default-account`다. loopback Web Console은 `POST /api/v1/auth/loopback-session`으로 짧은 JWT를 받으며, LAN 또는 비-loopback은 기존 service bearer 또는 계정 JWT가 필요하다. Account/RBAC/JWT가 구성되면 이 경로는 닫히고 Web Console은 login, session, RBAC 상태를 표시하며 role permission에 따라 action button을 비활성화한다.

## 진입점 선택

| 진입점 | 권장 사용자 | 사용 상황 |
|--------|-------------|-----------|
| Web Console | 일반 운영자 | host readiness, VM workbench, jobs, network, troubleshooting을 시각적으로 확인 |
| CLI `pcvcli.exe` | 자동화/스크립트 운영자 | JSON 출력, 반복 조회, job/diagnostic 작업을 script로 실행 |
| Web API | 고급 운영자/개발자 | contract 확인, 통합 테스트, 임시 진단 |
| Product wrapper | 관리자 운영자 | service lifecycle, diagnostics collection, install/update/rollback runbook |

반복 실행과 운영 증거 수집에서는 inline token 대신 `--protected-token-file`, `--token-env`, `--token-file`을 사용한다. Token/password/JWT/Authorization header 값은 화면, log, 문서, diagnostic bundle에 기록하지 않는다.

## 기능 매트릭스

| 기능 | Feature ID | Web Console | CLI | 주요 API |
|------|------------|-------------|-----|----------|
| Host status | [ `pcv.host.status` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-host-status)<br>[ `pcv.ops.summary` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-ops-summary) | Dashboard/Troubleshooting | `pcvcli host status` | `GET /host/status` |
| Runtime policy | [ `pcv.runtime.policy` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-runtime-policy) | Dashboard/Troubleshooting | `pcvcli runtime policy` | `GET /runtime/policy` |
| Network inventory | [ `pcv.network.inventory` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-network-inventory) | Network | `pcvcli network inventory` | `GET /network/inventory` |
| VM list/detail | [ `pcv.vm.inventory` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-inventory)<br>[ `pcv.vm.telemetry` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-telemetry)<br>[ `pcv.vm.rename` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-rename) | Virtual Machines | `pcvcli vm list/get` | `GET /vms`, `GET /vms/{id}` |
| VM create | [ `pcv.vm.create` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-create) | VM create dialog | `pcvcli vm create ...` | `POST /vms` |
| VM power | [ `pcv.vm.power-lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-power-lifecycle)<br>[ `pcv.vm.pause-lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-pause-lifecycle) | VM detail actions | `pcvcli vm start/shutdown/poweroff/restart` | `POST /vms/{id}/...` |
| VM Hyper-V Saved | [ `pcv.vm.saved-lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-saved-lifecycle) | VM detail `Save` / `Resume saved` | `pcvcli vm save` / `pcvcli vm resume-saved` | `POST /vms/{id}/save`, `POST /vms/{id}/resume-saved` |
| VM QoS/readback | [ `pcv.vm.qos` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-qos)<br>[ `pcv.vm.guest-service-readback` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-guest-service-readback)<br>[ `pcv.vm.guest-execution` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-guest-execution)<br>[ `pcv.vm.guest-channel` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-guest-channel)<br>[ `pcv.vm.resource-limits` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-resource-limits) | 선택 VM detail `QoS / Guest Readback` panel | `pcvcli vm limit/blkio-get/bandwidth/guest-agent-status/guest-ping` | `/vms/{id}/limit`, `/vms/{id}/blkio`, `/vms/{id}/bandwidth`, `/vms/{id}/guest-agent/...` |
| VM manage | [ `pcv.vm.managed-import` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-managed-import) | VM detail `Manage VM` | `pcvcli vm manage --yes` | `POST /vms/{id}/manage` |
| VM clone | [ `pcv.vm.clone` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-clone) | VM detail `Clone VM` | `pcvcli vm clone <source> --name <target> --yes` / `--dry-run` | `POST /vms/{id}/clone/preview`, `POST /vms/{id}/clone` |
| VM delete | [ `pcv.vm.delete` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-delete) | VM detail confirmation | `pcvcli vm delete --yes` | `DELETE /vms/{id}` |
| VM media attach/eject | [ `pcv.vm.media-attach` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-media-attach)<br>[ `pcv.vm.media-eject` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-media-eject) | Web form | `pcvcli vm attach/eject` | `POST /vms/{id}/attach`, `POST /vms/{id}/eject` |
| Checkpoints | [ `pcv.checkpoint.lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-checkpoint-lifecycle)<br>[ `pcv.checkpoint.restore` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-checkpoint-restore) | VM checkpoint panel | `pcvcli vm checkpoint ...` | `/vms/{id}/checkpoints` |
| Jobs | [ `pcv.job.lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-job-lifecycle) | Jobs/Activity | `pcvcli job ...` | `/jobs` |
| Diagnostics list | [ `pcv.diagnostics.bundle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-diagnostics-bundle) | Troubleshooting bundle 목록/pagination | `pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]` | `GET /diagnostics/bundles?limit=&offset=` |
| Diagnostics create/download | [ `pcv.diagnostics.bundle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-diagnostics-bundle) | Troubleshooting create/download | `pcvcli diagnostics bundle create/download` | `POST /diagnostics/bundles`, `GET /diagnostics/bundles/{id}/download` |
| Account/RBAC/JWT | [ `pcv.account.session` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-account-session) | Account panel | API/Web Console auth only | `/auth/...` |
| Console capability discovery | [ `pcv.console.capabilities` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-console-capabilities) | Console/Troubleshooting capability card | API/Web Console 전용 | `GET /console/capabilities` |
| VM console/noVNC handoff | [ `pcv.vm.console-handoff` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-console-handoff) | 선택 VM Console panel | `pcvcli vm console/vnc <vm>` | `GET /vms/{id}/console` |

## Feature ID 추적

| Feature ID | 사용자 기능 의미 |
|------------|------------------|
| [ `pcv.runtime.policy` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-runtime-policy) | Runtime 정책 조회 |
| [ `pcv.host.status` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-host-status) | 호스트 상태 조회 |
| [ `pcv.vm.inventory` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-inventory) | VM 목록과 상세 조회 |
| [ `pcv.job.lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-job-lifecycle) | 비동기 job 조회·취소·재시도·조정 |
| [ `pcv.ops.summary` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-ops-summary) | 운영 상태 요약 |
| [ `pcv.diagnostics.bundle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-diagnostics-bundle) | 진단 bundle 목록·생성·다운로드 |
| [ `pcv.account.session` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-account-session) | 계정, JWT session, RBAC |
| [ `pcv.console.capabilities` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-console-capabilities) | Console capability discovery |
| [ `pcv.network.inventory` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-network-inventory) | 네트워크 inventory |
| [ `pcv.vm.delete` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-delete) | VM 삭제와 삭제 상태 |
| [ `pcv.vm.console-handoff` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-console-handoff) | VM console/noVNC handoff |
| [ `pcv.vm.telemetry` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-telemetry) | VM CPU·memory telemetry |
| [ `pcv.vm.qos` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-qos) | Storage·network QoS |
| [ `pcv.vm.guest-service-readback` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-guest-service-readback) | Guest service 상태와 ping |
| [ `pcv.vm.guest-execution` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-guest-execution) | Guest command preview·실행 |
| [ `pcv.vm.guest-channel` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-guest-channel) | Guest channel preview·verify·ensure |
| [ `pcv.checkpoint.lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-checkpoint-lifecycle) | Checkpoint 목록·생성·삭제 |
| [ `pcv.vm.create` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-create) | VM 생성 |
| [ `pcv.checkpoint.restore` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-checkpoint-restore) | Checkpoint 복원 |
| [ `pcv.vm.power-lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-power-lifecycle) | VM 시작·종료·전원 차단·재시작 |
| [ `pcv.vm.pause-lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-pause-lifecycle) | VM pause·resume |
| [ `pcv.vm.saved-lifecycle` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-saved-lifecycle) | 가상 머신 전원 작업 — Saved/Resume saved |
| [ `pcv.vm.rename` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-rename) | VM 이름 변경 |
| [ `pcv.vm.managed-import` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-managed-import) | 기존 VM 관리 편입 |
| [ `pcv.vm.clone` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-clone) | managed VM 독립 VHDX full clone |
| [ `pcv.vm.media-eject` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-media-eject) | VM media 제거 |
| [ `pcv.vm.media-attach` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-media-attach) | VM media 연결 |
| [ `pcv.vm.resource-limits` ](FEATURE_IMPLEMENTATION_LEDGER.md#pcv-vm-resource-limits) | VM CPU·memory·disk resource 변경 |

Web Console 조회 화면은 새 OS mutation을 실행하지 않는다. Mutation action은 API authorization, confirmation, job guard, provider guard를 통과한 뒤 queued job 또는 명시 handoff로 실행된다.

## 공통 사용 계약

사용자가 action을 실행하면 UI는 다음 흐름을 따른다.

1. 현재 연결/API/token 상태를 확인한다.
2. RBAC permission 또는 bearer token gate를 확인한다.
3. Destructive action은 대상 이름/상태를 보여주고 명시 confirmation을 요구한다.
4. API가 job을 반환하면 job id, 상태, 다음 확인 위치를 보여준다.
5. 실패하면 stable `PCV_*` code, 짧은 설명, 다음 행동을 같이 보여준다.

공통 실패 메시지 형식:

```text
Action blocked: <short reason>.
<next operator action>.
code=<PCV_*>
```

API problem details는 사용자 UI에서 사람이 읽을 수 있는 설명으로 줄이고, 원본 `code`, `status`, `request_id` 또는 `correlation_id`는 운영자가 대조할 수 있게 유지한다.

## 호스트와 runtime

목적: Hyper-V, VMMS, service, admin readiness, runtime/auth/job/native operation policy를 확인한다.

| 항목 | 명세 |
|------|------|
| 전제조건 | Local API reachable, token 또는 account session |
| 성공 결과 | readiness summary, VMMS/service/admin/network/native operation policy 표시 |
| 실패 처리 | `PCV_AUTH_FORBIDDEN`, `PCV_ROUTE_TIMEOUT`, `PCV_RATE_LIMIT_EXCEEDED`, `PCV_HOST_STATUS_FAILED` 등을 표시 |
| 사용자 조치 | service 상태, Hyper-V feature, VMMS, token source를 확인 |

Host/runtime 화면은 service install/start/stop, Event Log repair, Credential Manager migration, TLS binding을 실행하지 않는다. 해당 작업은 관리자 운영 runbook과 evidence gate가 소유한다.

## 네트워크 inventory

목적: Hyper-V switch source/type/default/management OS/external adapter field를 read-only로 확인한다.

| 항목 | 명세 |
|------|------|
| 전제조건 | Hyper-V inventory 조회 가능, token/account session |
| 성공 결과 | switch 목록과 topology summary 표시 |
| 실패 처리 | native parity failure는 helper fallback 없이 structured failure로 표시 |
| 사용자 조치 | VMMS/Hyper-V 상태, Default Switch, network adapter 상태 확인 |

Network 화면은 switch 생성/삭제, IP 변경, firewall 변경을 실행하지 않는다.

## 가상 머신 inventory와 상세

목적: Hyper-V VM 목록과 선택된 VM detail을 확인한다.

| 항목 | 명세 |
|------|------|
| 전제조건 | `GET /api/v1/vms` 성공 |
| 성공 결과 | VM id/name/state/cpu/memory/generation/storage/network/checkpoint count/managed marker 표시 |
| 실패 처리 | `PCV_VM_LIST_FAILED`, native parity failure, auth/timeout/rate-limit code 표시 |
| 사용자 조치 | VMMS와 Hyper-V inventory 상태 확인, 필요 시 diagnostic bundle 수집 |

선택된 VM이 refresh 후 inventory에서 사라지면 stale action을 막고 detail/checkpoint panel을 비운다.

```text
Action blocked: selected VM is stale.
Refresh the VM inventory, then choose the VM again.
code=PCV_SELECTED_VM_STALE
```

## 가상 머신 생성

목적: PureCVisor managed Hyper-V VM 생성 job을 queue한다.

| 항목 | 명세 |
|------|------|
| 전제조건 | operator 이상, VM name/ISO/CPU/memory/disk/generation/root 입력 |
| 성공 결과 | `queued` job id 반환, Jobs/Activity에서 추적 |
| Guard | invalid name, generation mismatch, existing VM, missing Default Switch, storage conflict 차단 |
| 실패 처리 | `PCV_VM_NAME_INVALID`, `PCV_GENERATION_INVALID`, `PCV_VM_ALREADY_EXISTS`, `PCV_VM_CREATE_FAILED` 등 |

Generation 2가 현재 native VM create product path다. VM root와 ISO path는 Windows host 기준 path를 사용한다.

## 가상 머신 전원 작업

목적: VM start, guest shutdown, forced poweroff, restart, Hyper-V Saved job을 queue한다.

| Action | 사용자 의미 | 주의 |
|--------|-------------|------|
| Start | 꺼진 VM을 시작 | 이미 실행 중이면 API/provider 결과를 확인 |
| Shutdown | guest integration을 통한 정상 종료 | integration 미지원이면 차단될 수 있음 |
| Poweroff | 강제 전원 종료 | workload 손상 가능성 확인 필요 |
| Restart | 재시작 | running workload 영향 확인 필요 |
| Save | Hyper-V Saved 상태로 저장 | pause가 아니다. 호스트 재부팅 뒤에도 유지 |
| Resume saved | Saved 상태에서 재개 | 현재 state가 `saved`가 아니면 `PCV_VM_NOT_SAVED` |

성공 시 job id와 queued/running/succeeded 상태를 표시한다. 실패 시 다음처럼 안내한다.

```text
Action failed: guest shutdown is not available.
Use Poweroff only after checking workload impact.
code=PCV_VM_SHUTDOWN_NOT_AVAILABLE
```

## 가상 머신 QoS와 guest service readback

목적: Linux `pcvctl` command shape 중 Hyper-V 제품 의미로 닫힌 resource/readback 명령을
CLI/API에서 확인하고, Web Console에서는 선택된 VM의 readback 상태를 조회한다.

| Action | 사용자 의미 | 주의 |
|--------|-------------|------|
| `vm limit` | Hyper-V vCPU/startup memory resource mutation job queue | Linux cgroup limit 호환 claim이 아니다. |
| `vm blkio-get` | Hyper-V disk/storage inventory readback | Linux blkio throttle 값이 아니다. |
| `vm blkio-set ... --dry-run` | Hyper-V storage QoS mutation preview | host mutation을 수행하지 않는다. |
| `vm blkio-set ... --yes` | Hyper-V storage QoS queued apply | 0.42.47 설치본 actual VM/fullgate와 0.42.45 -> 0.42.47 manual-admin closure PASS. |
| `vm bandwidth` | Hyper-V network adapter inventory readback | Linux bandwidth shaping mutation이 아니다. |
| `vm bandwidth-set ... --dry-run` | Hyper-V network bandwidth mutation preview | host mutation을 수행하지 않는다. |
| `vm bandwidth-set ... --yes` | Hyper-V network bandwidth queued apply | 0.42.47 설치본 actual VM/fullgate와 0.42.45 -> 0.42.47 manual-admin closure PASS. |
| `vm guest-agent-status` | Hyper-V Integration Services readiness readback | qemu guest agent status가 아니다. |
| `vm guest-ping` | VM state 기반 guest service readiness readback | credentialless guest heartbeat 검증 claim이 아니다. |

`vm blkio-set`과 `vm bandwidth-set`은 ADR-0008 slice에서 Hyper-V QoS mutation
preview/queued apply command로 승격했다. 0.42.47 설치본 package build, 실제 VM admin smoke,
full admin host mutation gate와 0.42.45 -> 0.42.47 manual-admin package-pair closure가
PASS했다. 2026-05-29 follow-up부터 QoS 값은 CLI/API에서 먼저 검증된다. 음수,
`1,000,000,000` 초과, `minimum > maximum`은 storage/network별
`PCV_VM_QOS_*_RANGE_INVALID`로 거절되고, invalid preview는 native adapter를 호출하지 않으며
invalid apply는 job queue를 만들지 않는다. `vm guest-agent-ensure-channel`, `vm guest-exec`은 ADR-0009 security boundary
contract와 `0.42.53-admin-smoke` provider route로 승격했다. Protected credential reference,
audit/redaction, timeout, RBAC guard를 통과한 queued provider route를 제공하며 실제 Windows
guest credentialed execution smoke는 `pcv-guest-installed-04253-r1` persistent Windows VHD
target과 DPAPI LocalMachine credential reference 기준으로 PASS했다. Running interrupt/cancel은
`0.42.54-admin-smoke` 설치본 package/current-card 및 actual long-running Windows guest smoke에서
PASS했고, `0.42.55-admin-smoke` dated predecessor current-card에서 당시 running cancel
affordance와 actual credentialed guest-exec를 재확인했다.

현재 Web Console 선택 VM detail은 ADR-0008 범위의 storage/network QoS preview/apply
form과 `QoS / Guest Readback` panel을 제공한다. 2026-05-26 `0.42.48-admin-smoke`의 TUI
검증은 historical predecessor이며 현재 제품 표면을 정의하지 않는다. Web Console panel은
다음 네 read-only route를 함께 조회한다.

- `GET /api/v1/vms/{id}/blkio`
- `GET /api/v1/vms/{id}/bandwidth`
- `GET /api/v1/vms/{id}/guest-agent/status`
- `GET /api/v1/vms/{id}/guest-agent/ping`

이 surface 추가는 Operator Surface product payload 변경이므로 `0.42.40-admin-smoke`
package/fullgate/manual-admin package-pair closure로 닫혔다. 설치본 CLI targeted smoke
`docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`는
0.42.39 설치본 CLI command path의 historical/current anchor로 유지한다.

## 가상 머신 manage

목적: 이미 있는 Hyper-V VM에 PureCVisor managed marker를 opt-in으로 붙인다. OVF/clone/export가 아니다.

| 항목 | 명세 |
|------|------|
| 전제조건 | operator 이상, 명시 confirmation, Hyper-V 표시 이름과 path identifier 일치 |
| Web Console | 선택 VM detail `Manage VM`. 확인 dialog는 표시 이름과, 성공 후 이 VM이 managed delete 가드를 통과한다는 점, unmanaged delete 거절은 유지된다는 점을 보여 준다 |
| CLI | `pcvcli vm manage <vm> --yes` |
| Guard | `--yes`/`confirm` 없음, `confirm_name`과 `{vmId}` Ordinal 불일치 |
| 성공 결과 | manage job id 표시, Activity에서 추적. 이미 managed면 `already-managed` |

Web POST `confirm_name`은 다른 lifecycle 버튼과 같은 URL path identifier다.

## 가상 머신 clone

목적: managed Generation 2 VM의 독립 VHDX를 복사해 새 managed VM을 만든다. linked clone, export/import, OVF가 아니다.

| 항목 | 명세 |
|------|------|
| 전제조건 | operator 이상, 소스 managed/Gen2/`Off`/checkpoint 0/독립 VHDX, 명시 confirmation, 소스와 다른 대상 이름 |
| Web Console | 선택 VM detail `Clone VM`. 대상 이름을 입력하고 preview로 `planned_copy_bytes`를 확인한 뒤 confirmation에 소스 표시 이름과 대상 이름을 보여 준다 |
| CLI | `pcvcli vm clone <source> --name <target> --dry-run [--vm-root <path>]`, `pcvcli vm clone <source> --name <target> --yes [--vm-root <path>]` |
| Guard | `--yes` 없음(enqueue), `confirm_name`과 `{vmId}` Ordinal 불일치, 대상 이름 없음, 소스와 같은 대상 이름, unmanaged/Gen1/not-Off/checkpoint/differencing/TPM |
| 성공 결과 | preview는 복사 계획만 반환. clone job id는 Activity에서 추적. 새 VM만 managed marker를 가진다 |

Web/CLI POST `confirm_name`은 소스 `{vmId}` 그대로다. `name`은 대상 표시 이름이다. 실패 시 대상만 rollback하고 소스 VM은 변경하지 않는다.

```text
Action blocked: VM clone requires explicit confirmation.
Use: vm clone <source> --name <target> --yes.
code=PCV_CLI_CONFIRMATION_REQUIRED
```

## 가상 머신 delete

목적: PureCVisor managed VM만 삭제 job으로 queue한다.

| 항목 | 명세 |
|------|------|
| 전제조건 | operator 이상, 명시 confirmation, managed marker |
| Web Console | 선택 VM detail에서 대상 확인 후 실행 |
| CLI | `pcvcli vm delete <vm> --yes` |
| Guard | running VM, unmanaged VM, stale selected VM, missing confirmation 차단 |
| 성공 결과 | delete job id 표시, Activity에서 추적 |

```text
Action blocked: VM is not managed by PureCVisor Desktop Node.
Delete only VMs created or marked by PureCVisor.
code=PCV_VM_NOT_MANAGED_BY_PURECVISOR
```

VM delete는 provider mutation 전에 guard가 먼저 실행되어야 한다.

## 체크포인트

목적: VM checkpoint list/create/restore/delete를 실행한다.

| 기능 | 전제조건 | 결과 |
|------|----------|------|
| List | VM 존재 확인 | checkpoint 이름/VM/creation time 표시 |
| Create | checkpoint name 입력 | create job queue |
| Restore | 대상 checkpoint 선택, confirmation | restore job queue |
| Delete | 대상 checkpoint 선택, confirmation | delete job queue |

Checkpoint restore/delete는 workload 상태에 영향을 줄 수 있으므로 UI는 대상 VM과 checkpoint 이름을 confirmation에 표시한다. Native checkpoint parity가 불완전하면 helper fallback 없이 structured failure를 표시한다.

## 작업 job과 activity

목적: queued/running/succeeded/failed/canceled job 상태를 추적하고 cancel/retry를 요청한다.

| 항목 | 명세 |
|------|------|
| Server-side list | `GET /api/v1/jobs?limit=50&offset=0`, 최대 limit 200 |
| Retention | terminal job 최신 500개 보존, active job 보존 |
| Browser tracked jobs | 현재 브라우저 localStorage에 최대 50개 |
| Action | queued/running cancel 요청, running guest execution cancel affordance, retryable failed job retry |

Activity 화면은 read-only visibility다. Cancel/retry button은 기존 job route만 호출하고
Hyper-V/service/MSI/firewall/trust-store/LAN mutation을 자동 실행하지 않는다. Running `guest.exec`
job cancel은 `Cancel running guest exec` label과 `running-guest-execution` scope를 사용한다.

## 진단 bundle

목적: 지원 요청과 장애 분석에 필요한 redacted diagnostic bundle을 생성/다운로드한다.

| 항목 | 명세 |
|------|------|
| 전제조건 | diagnostics permission 또는 bearer token, diagnostics API 지원 listener |
| Web Console | Troubleshooting에서 create/download, unsupported listener면 안내 |
| CLI | `pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]`, `create`, `download <bundle_id> --output <path>` |
| Redaction | token file body, protected token blob/hash, Authorization header, password/JWT secret 미포함 |

지원되지 않는 listener에서는 다음처럼 안내한다.

```text
Action blocked: diagnostic bundle API is unavailable.
Use the product wrapper CollectDiagnostics runbook on this host.
code=PCV_DIAGNOSTIC_BUNDLE_API_UNSUPPORTED
```

## 계정, RBAC, token

목적: account login/session/RBAC 상태를 확인하고 사용자 action 가능 여부를 결정한다.

| 기능 | 명세 |
|------|------|
| Login | `POST /api/v1/auth/login`, username/password로 JWT 발급 |
| Loopback session | `auth.loopback-session` / `POST /api/v1/auth/loopback-session`, Web-only / no CLI command |
| Refresh | `POST /api/v1/auth/refresh`, refresh token 회전 |
| Logout | `POST /api/v1/auth/logout`, browser session token clear와 refresh/session revoke handoff |
| Session | `GET /api/v1/auth/session`, 현재 username/role/permission 표시 |
| RBAC | `GET /api/v1/auth/rbac`, permission matrix 표시 |

Auth route policy:

| Route | Bearer token policy | 입력/전제 |
|------|---------------------|-----------|
| `POST /api/v1/auth/login` | `NoBearerTokenRequired` | username/password |
| `POST /api/v1/auth/loopback-session` | `NoBearerTokenRequired` | Web-only / no CLI command |
| `POST /api/v1/auth/refresh` | `NoBearerTokenRequired` | refresh token |
| `POST /api/v1/auth/logout` | `NoBearerTokenRequired` | refresh/session token handoff |
| `GET /api/v1/auth/session` | `TokenRequired` 또는 account JWT | bearer token 또는 account JWT |
| `GET /api/v1/auth/rbac` | `TokenRequired` 또는 account JWT | bearer token 또는 account JWT |

Token handling 원칙:

- Web Console은 token 값을 저장/표시하지 않고 session presence만 표시한다.
- `Clear browser token`은 브라우저 입력/세션 token만 지운다.
- Service protected token file, Credential Manager source, service config는 Web Console에서 직접 변경하지 않는다.
- Credential Manager transition, service token rotation/revoke는 관리자 runbook evidence gate가 소유한다.

## 콘솔과 noVNC

목적: listener-global console capability discovery와 선택된 VM의 handoff 상태를 보여준다.

| 항목 | 명세 |
|------|------|
| Windows console | Hyper-V `vmconnect` operator-local handoff |
| noVNC | `--novnc-target-host`/`--novnc-target-port`가 명시될 때 target-backed bridge |
| 기본 상태 | noVNC disabled 또는 `not_configured` |
| Permission | `console.view` |
| Global discovery | `GET /api/v1/console/capabilities`는 API/Web Console 전용이며 CLI 전역 command 없음 |
| VM-specific CLI | `pcvcli vm console/vnc <vm>`은 session/handoff metadata를 반환하며 GUI를 자동 실행하지 않음 |

Console 기능은 Linux console backend나 KVM/libvirt console을 포함하지 않는다. noVNC bridge는 Windows Desktop Node listener의 opt-in target-backed bridge이며, 브라우저가 host mutation을 시작하지 않는다.

## 명령줄 CLI 사용 계약

CLI는 Local API thin client다. 명령어 상세는 `docs/CLI_COMMAND_USAGE.md`가 소유한다.

```powershell
$tokenFile = Join-Path $env:ProgramData 'PureCVisor\desktop-node\api-token.dpapi.json'
pcvcli --protected-token-file $tokenFile --json vm list
pcvcli --protected-token-file $tokenFile job list --limit 50 --offset 0
```

Exit code:

| Code | 의미 |
|------|------|
| `0` | 성공 또는 help 출력 |
| `1` | API/transport/non-success response |
| `2` | CLI usage/token source 오류 |

CLI는 service install/start/stop, MSI repair/remove, firewall, trust-store, LAN listener, Event Log provider, update/rollback mutation을 직접 실행하지 않는다.

## 보안과 감사

- Token/password/JWT/certificate private key 값은 화면, stdout/stderr, 문서, evidence, diagnostic bundle에 남기지 않는다.
- Destructive VM/checkpoint action은 대상 이름과 상태를 confirmation에 표시한다.
- `request_id`, `correlation_id`, `job_id`, `bundle_id`는 운영 대조용으로 표시할 수 있지만 secret이 아니다.
- Public release claim은 하지 않는다. 내부 사설망 전용 evidence와 public distribution closed-not-adopted 기록을 구분한다.
- Host mutation이 필요한 작업은 관리자 opt-in, rollback/final-state proof, evidence 경로를 남긴다.

## 관련 문서

| 문서 | 용도 |
|------|------|
| `docs/USER_GUIDE.md` | 설치본 사용자 절차 |
| `docs/CLI_COMMAND_USAGE.md` | CLI 명령어와 option |
| `docs/OPERATIONS_GUIDE.md` | 운영 runbook, incident 대응, host mutation guard |
| `docs/DEVELOPMENT_VERIFICATION_POLICY.md` | 검증 기준 |
| `docs/PUBLIC_RELEASE_BOUNDARY.md` | public/internal release boundary |
| `docs/FEATURE_IMPLEMENTATION_LEDGER.md` | Feature ID, route surface, evidence stage 투영 |
| `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md` | 내부 사설망 배포 gate |
| `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md` | 후속 작업/자동 batch 분류 |
