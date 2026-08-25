# PureCVisor Desktop Node 설계

## 목적

이 문서는 Windows 10/11 기반 홈랩/인프라 운영자용 가상화 제품인 `PureCVisor Desktop Node`의 MVP 설계를 정의한다.

이번 설계는 기존 `purecvisor-single` Linux KVM 제품을 Windows로 단순 포팅하는 작업이 아니다. 목표는 PureCVisor의 운영 철학, API 중심 구조, Web Console 경험을 계승하되, Windows에서는 Hyper-V를 실행 엔진으로 사용하는 새 제품 방향을 먼저 확정하는 것이다.

구현 저장소 구조는 이 문서의 범위에서 확정하지 않는다. 새 저장소, 기존 저장소 확장, 일부 UI/API 코드 이관 여부는 후속 구현 계획에서 결정한다.

## 현재 구현 상태

Phase 1 Hyper-V PowerShell helper spike는 완료되어 `spikes/purecvisor-desktop-node/hyperv/`에 병합됐다. 현재 구현은 host diagnostics, VM inventory, ISO 기반 VM 생성, lifecycle action, checkpoint action의 JSON helper 계약을 검증한다. Host readiness는 Hyper-V cmdlet 사용 가능 여부를 우선 신호로 삼고, Windows 로캘별 Hyper-V not-found 오류를 VM 부재로 분류한다.

Phase 2A/2B/2C/2D/2E/2F/2G/2H Local API daemon spike도 `spikes/purecvisor-desktop-node/api/`에 추가됐다. 현재 구현은 기본 loopback-only HTTP listener, `GET /api/v1/host/status`, `GET /api/v1/vms`, `POST /api/v1/vms`, VM detail/lifecycle/checkpoint routes, `GET /api/v1/jobs/{job_id}`, `POST /api/v1/jobs/{job_id}/cancel`, `POST /api/v1/jobs/{job_id}/retry`, optional static Web Console serving, optional bearer-token/token-file gate, explicit LAN mode opt-in, JSONL event log, Windows Firewall rule ensure, Hyper-V helper process invocation, in-memory job store, FIFO worker queue, bounded worker-pool tick, optional JSON job persistence, corrupt store quarantine, structured API error mapping을 검증한다.

Phase 3A/3B/10 Web Console spike는 원래 `spikes/purecvisor-desktop-node/web/`에 추가됐고, 2026-05-03 served asset/root migration slice 이후 제품 Web Console source는 repo-root `web/`로 이동했다. 현재 `web/src/served-app.ts`가 served `web/app.js`를 생성하며, Local API `-WebRootPath web` 경계에서 host dashboard, VM table, VM detail drawer, VM create job form, lifecycle job actions, checkpoint controls, browser-local job history, job cancel/retry controls, optional bearer token request를 검증한다. Web Console 첫 화면 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3a-web-console-design.md`, VM detail/lifecycle 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase3b-vm-detail-lifecycle-design.md`, Phase 10 제품화 후속 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase10-web-cli-productization-design.md`에 둔다.

Phase 4 CLI MVP spike는 `spikes/purecvisor-desktop-node/cli/`에 추가됐다. 현재 구현은 Local API thin client이며 host, VM list/detail/create, lifecycle, checkpoint, job control command와 `--json`, `--api`, `--token` 옵션을 검증한다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase4-cli-mvp-design.md`에 둔다.

Phase 5 LAN mode hardening은 Local API에 추가됐다. 현재 구현은 non-loopback prefix를 기본 거부하고, `-AllowLan`과 non-empty `-ApiToken`이 함께 있을 때만 LAN 접근을 허용한다. `-EventLogPath`는 JSONL listener/firewall event를 기록하고, `-EnsureFirewallRule`은 opt-in Windows Firewall inbound TCP rule을 ensure한다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase5-lan-mode-hardening-design.md`에 둔다.

Phase 6 Windows service packaging spike는 `spikes/purecvisor-desktop-node/service/`에 추가됐다. 현재 구현은 Local API listener를 Windows 서비스로 등록하기 위한 `sc.exe` command builder, `-WhatIf` preview, injectable process runner, LAN service mode token 필수 조건을 검증한다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase6-service-packaging-design.md`에 둔다.

Phase 7 service token file hardening은 Local API와 service packaging에 추가됐다. 현재 구현은 `-ApiTokenFile`로 bearer token을 읽고, inline token과 token file을 동시에 지정하는 ambiguous 설정을 거부하며, LAN 서비스 binary path가 token 값 대신 token file 경로만 전달하도록 검증한다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase7-service-token-file-design.md`에 둔다.

Phase 8 installer hardening은 service packaging에 추가됐다. 현재 구현은 기본 `%ProgramData%\PureCVisor\desktop-node\api-token.txt` token file 위치, 난수 token file 생성, `icacls.exe` ACL command builder, 명시적 service account, 관리자 권한 smoke 절차를 검증한다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase8-installer-hardening-design.md`에 둔다.

Phase 9 Local API runtime hardening은 Local API에 추가됐다. 현재 구현은 `GET /api/v1/runtime/policy`로 persistence, retry, cancel, worker, CORS, auth, token storage 결정을 노출하고, manual retry attempt 상한을 `3`으로 고정한다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase9-local-api-runtime-hardening-design.md`에 둔다.

Phase 10 Web Console/CLI 제품화 후속은 Web Console과 CLI에 추가됐다. 현재 구현은 Web Console VM detail panel의 checkpoint list/create/restore/delete controls, browser `localStorage` 기반 tracked job history, CLI `--token-file` UX를 검증한다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase10-web-cli-productization-design.md`에 둔다.

Phase 11 제품 런타임 승격 판단은 Desktop Node를 제품 런타임으로 승격하지 않고 계속 `spikes/purecvisor-desktop-node/**` 격리 spike로 유지하기로 결정했다. 결정 표식은 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`이며, 이 판단은 Phase 19 evidence-first 재판정에서도 유지됐다. 설계는 `docs/superpowers/specs/2026-04-25-purecvisor-desktop-node-phase11-runtime-promotion-decision-design.md`와 `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`에 둔다.

Phase 12 Service-first 제품 wrapper는 `packaging/windows-desktop-node/`에 추가됐다. 현재 구현은 spike 자산을 제품 후보 설치 루트로 복사하고, product manifest, install/rollback/uninstall/status/diagnostic bundle 경계를 검증한다. 설계는 `docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase12-service-first-runtime-design.md`에 둔다.

Phase 13 WinSW service wrapper는 `packaging/windows-desktop-node/`에 추가됐다. 당시 구현은 WinSW executable/XML staging, service plan/action 전환, manifest/diagnostic WinSW metadata, loopback static asset 무인증과 API bearer 유지 경계를 검증했다. 2026-05-01 replacement slice 이후 기본 제품 service host는 .NET `DesktopNode.Host.exe`이며, Phase 13 문서는 service wrapper 이력과 compatibility 경계로 보존한다. 설계는 `docs/superpowers/specs/2026-04-26-purecvisor-desktop-node-phase13-winsw-service-wrapper-design.md`에 둔다.

Phase 14 WiX MSI-first installer는 `packaging/windows-desktop-node/installer/`에 추가됐다. 현재 구현은 MSI source/build script/provenance, MSI installed custom action, unsigned dev/admin-smoke MSI build, repair/uninstall/`REMOVE_DATA=1` smoke 경계를 검증한다. 2026-05-01 replacement slice 이후 MSI installed action은 `DesktopNode.Host.exe service-action`을 호출한다. 설계는 `docs/superpowers/specs/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux-design.md`에 둔다.

Phase 15 secure token storage는 제품 기본 bearer token source를 DPAPI LocalMachine protected token file로 전환했다. 설계는 `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase15-secure-token-storage-design.md`에 둔다.

Phase 16 long-term diagnostics는 JSONL first diagnostics policy, log rotation, versioned diagnostic bundle, Windows Event Log opt-in registration plan을 고정했다. 설계는 `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase16-event-log-long-term-diagnostics-design.md`에 둔다.

Phase 17 LAN security policy는 loopback 기본값, LAN preview/admin opt-in, reverse proxy/TLS 전제, non-loopback static bearer auth, firewall opt-in lifecycle을 고정했다. 설계는 `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase17-lan-security-policy-design.md`에 둔다.

Phase 18 update/rollback/config migration은 manifest-first safe update 정책과 관리자 update/rollback smoke 증거를 제품 wrapper 경계에 추가했다. 설계는 `docs/superpowers/specs/2026-04-28-purecvisor-desktop-node-phase18-update-rollback-config-migration-design.md`에 둔다.

Phase 19 제품 승격 재판정은 `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`로 결론냈다. DPAPI protected token, JSONL diagnostics/redaction, LAN preview policy, manifest-first update/rollback/config migration은 충족 gate로 본다. Phase 22 후속 개발은 release/version policy를 문서화하고 installer `windows-x64` artifact naming, provenance `release_channel`, unsigned RC/stable 차단을 build contract로 강제했으며 ADR-0002가 이를 현재 적용 결정으로 채택한다. 2026-04-30에는 local test certificate 기준 signed RC MSI lifecycle, elevated MSI lifecycle, Hyper-V product-flow lifecycle, release approval/signing preflight, firewall cleanup, 운영/Event Log source lifecycle evidence를 draft-ready 기준으로 기록했다. 2026-05-01에는 current-head `3d35aa2` 기준 `0.23.9-rc.1` local test `RequireSigned` MSI lifecycle과 product-wrapper update/rollback/config migration smoke, `0.23.10-rc.1` internal enterprise `RequireSigned` MSI lifecycle, `0.26.0-admin-smoke` .NET Windows Service Host replacement service/MSI/Hyper-V helper smoke를 추가로 기록했다. 하지만 public trusted signature, stable publication, GA 제품 런타임 승격은 별도 판단으로 남긴다.

Phase 24 Local API job runtime boundary 후보는 `GET /api/v1/runtime/policy`의 `job_runtime` object로 PowerShell orchestration, Hyper-V helper process boundary, job state/persistence/control policy를 고정했다. Phase 25 .NET/TypeScript 전환 후보는 .NET contract/runtime/API/service/host, TypeScript Web Console parity scaffold, PowerShell Windows adapter 역할 분리를 정의했다. 2026-05-01 replacement slice 이후 `DesktopNode.Host.exe`가 기본 제품 service host, loopback listener owner, SCM binary path, MSI installed custom action runner를 담당한다. Route parity 시작 slice는 .NET request processor에 helper-backed route parity, queued VM/checkpoint lifecycle routes, job get/cancel/retry, JSON job store save/load/recovery를 추가했다. 2026-05-02/2026-05-03 native adapter slices 이후 `host.status`, `network.inventory`, `vm.list`, VM detail, checkpoint list는 C# native adapter가 직접 처리하며 helper fallback 없이 native structured success/failure를 반환한다. VM create/start/shutdown/poweroff/restart/delete는 .NET request processor queue를 유지하되 C# WMI adapter가 직접 실행한다. Native VM create product path는 Hyper-V Generation 2만 지원하며, native VM delete는 managed marker guard와 missing VM idempotent `action=absent` contract를 둔다. Checkpoint create/restore/delete는 .NET request processor queue를 유지하되 C# WMI snapshot service adapter가 직접 실행한다. Web Console browser fixture parity는 served `app.js`를 Node `vm` 최소 DOM과 fixture Local API 응답으로 실행하는 code-level/npm 검증이고, served `app.js`는 repo-root `web/src/served-app.ts` TypeScript build output이다.

- 기본 검증: Pester non-integration suite
- 정상 기준과 최신 suite 기대 결과: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- 실제 Hyper-V VM 생성 검증: `PCV_HYPERV_INTEGRATION=1`과 `PCV_HYPERV_TEST_ISO`가 설정된 Windows Hyper-V 호스트에서만 실행하며, 통합 포함 전체 Hyper-V suite 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.
- 경계: Linux `purecvisorsd`와 Single Edge 공개 릴리스 산출물에는 연결하지 않음

## 사용자와 제품 포지션

1차 사용자는 홈랩/인프라 운영자다.

이 사용자는 Windows PC 한 대를 항상 켜 둔 작은 가상화 노드처럼 운영하고, 브라우저와 CLI/API로 Linux 서버 VM을 만들고 관리하기를 원한다. 따라서 제품의 기준점은 일반 데스크톱 앱보다 로컬 인프라 노드에 가깝다.

제품 포지션은 다음과 같다.

```text
PureCVisor Desktop Node

Windows 10/11 Pro/Enterprise host
  -> Hyper-V backend
  -> local management service
  -> Web Console + CLI + REST API
  -> homelab Linux VM operations
```

이 제품은 VMware Workstation이나 VirtualBox처럼 자체 VMM을 새로 구현하지 않는다. MVP에서는 Hyper-V를 공식 실행 엔진으로 사용하고, PureCVisor는 그 위의 관리 계층과 운영 UX를 제공한다.

## MVP 결정

승인된 MVP 전제는 다음과 같다.

```text
호스트: Windows 10/11 Pro/Enterprise
엔진: Hyper-V 필수
사용 방식: Web Console 기본, CLI/API 함께 제공
게스트: Linux 서버 VM 우선
VM 생성: ISO 설치 중심
네트워크: Hyper-V Default Switch 우선
콘솔: Hyper-V VMConnect로 위임
API 노출: 기본 localhost, 설정으로 LAN 허용
백엔드 구현: MVP는 PowerShell 기반, 이후 C23/CIM 네이티브화 검토
저장소 전략: 설계 문서 먼저 확정, 구현 단계에서 결정
```

## MVP 포함 범위

MVP는 Hyper-V Manager 전체 대체가 아니라 Linux 서버 VM을 만들고 안정적으로 운영하는 최소 홈랩 콘솔이다.

포함 기능은 다음과 같다.

```text
- Hyper-V 설치/활성 상태 진단
- Windows edition과 관리자 권한 진단
- VMMS 서비스 상태 확인
- Hyper-V Default Switch 확인
- Linux ISO 기반 VM 생성
- VM 목록, 상태, 상세 조회
- VM start, graceful shutdown, force off, restart
- CPU, RAM, VHDX 기본 설정
- Default Switch 연결
- checkpoint 생성, 복원, 삭제
- VMConnect 실행 또는 연결 안내
- 기본 localhost Web Console
- 설정 기반 LAN 접근
- CLI/API 기본 제공
```

## MVP 제외 범위

다음은 MVP에서 제외한다.

```text
- Windows Home 지원
- Windows Hypervisor Platform 위의 자체 VMM 구현
- QEMU 내장 실행 엔진
- 자체 웹 콘솔 스트리밍
- cloud image와 cloud-init 자동 생성
- PureCVisor 자체 NAT, DHCP, DNS
- Windows guest 1급 최적화
- LXC 또는 Windows Containers
- ZFS, OVS, OVN 대응 기능
- 클러스터, 멀티 노드, 라이브 마이그레이션
- 네이티브 C23/CIM Hyper-V 백엔드
```

이 제외 범위는 영구 제외가 아니라 MVP 리스크 축소를 위한 결정이다.

## 아키텍처

MVP 아키텍처는 Hyper-V를 실행 엔진으로 두고 PureCVisor가 관리 계층을 제공한다.

```text
Browser / CLI / REST client
        |
PureCVisor Local API
        |
Job queue + validation + audit + state cache
        |
Hyper-V backend adapter
        |
PowerShell runner
        |
Hyper-V cmdlets / VMMS
        |
VMs, VHDX, Default Switch, Checkpoints
```

핵심 원칙은 다음과 같다.

- VM 실행, VHDX, checkpoint, vSwitch는 Hyper-V가 담당한다.
- PureCVisor는 API, Web Console, 작업 큐, 검증, 상태 모델, 운영 UX를 담당한다.
- PowerShell은 MVP 백엔드 구현 수단이며, 제품 API의 공개 계약이 되어서는 안 된다.
- PowerShell runner는 어댑터 뒤에 격리해 장기적으로 C23/CIM/WMI 기반 구현으로 교체 가능하게 한다.

장기적인 백엔드 경계는 다음 형태를 목표로 한다.

```text
platform/
  hyperv/
    backend interface
    powershell implementation  # MVP
    native cim implementation  # v2
```

## 프로세스 구조

MVP 프로세스 구조는 다음 중 하나로 시작한다.

```text
- Windows background service 또는 tray-launched local daemon
- localhost HTTP API
- static Web Console 제공
- CLI는 같은 API 호출
```

관리 API는 기본적으로 `127.0.0.1`에만 바인딩한다. Local API spike는 `-ApiToken`, `-ApiTokenFile`, 또는 `-ApiTokenProtectedFile`로 optional bearer-token gate를 켤 수 있다. 제품 설치 기본 경로는 `DesktopNode.Host.exe listen`이며 token 값을 직접 command line에 두지 않도록 `--api-token-protected-file`을 사용한다. 사용자가 LAN 접근을 켜면 `-AllowLan`과 non-empty token source를 함께 지정해야 한다. 필요 시 `-EventLogPath`와 `-EnsureFirewallRule`로 이벤트 기록과 Windows Firewall rule을 명시적으로 설정한다.

## 컴포넌트

MVP 컴포넌트는 다음과 같이 나눈다.

```text
1. Host Diagnostics
2. VM Inventory
3. VM Lifecycle
4. VM Provisioning
5. Checkpoint Manager
6. Console Launcher
7. API/Auth
8. Web Console
9. CLI
10. PowerShell Runner
```

### Host Diagnostics

- Windows edition 확인
- Hyper-V feature 활성 여부 확인
- 관리자 권한 확인
- Default Switch 존재 확인
- VMMS 서비스 상태 확인

### VM Inventory

- Hyper-V VM 목록 조회
- PureCVisor가 만든 VM과 외부 VM 구분
- 상태, CPU, RAM, uptime, disk path, switch name 표시

### VM Lifecycle

- start, graceful shutdown, force off, restart 제공
- 작업 중복 방지
- 실패 시 Hyper-V 원본 오류와 PureCVisor 해석 메시지를 함께 제공

### VM Provisioning

- ISO 경로 선택
- VM 이름, CPU, RAM, VHDX 크기 입력
- VHDX 생성
- Hyper-V VM 생성
- Default Switch 연결
- DVD ISO 연결
- boot order 설정

### Checkpoint Manager

- checkpoint 생성
- checkpoint 목록 조회
- checkpoint 복원
- checkpoint 삭제

### Console Launcher

- 로컬 Web Console에서는 VMConnect 실행 액션 제공
- 원격 브라우저에서는 VMConnect 직접 실행이 불가능하므로 대체 명령과 안내를 표시

### PowerShell Runner

- 허용된 operation만 실행
- 입력은 JSON 파일 또는 stdin 구조체로 전달
- 출력은 JSON만 반환
- timeout, exit code, stderr, structured error를 표준화

## 데이터 모델

Hyper-V 객체를 그대로 노출하지 않고, PureCVisor용 얇은 VM 모델로 감싼다.

```json
{
  "id": "ubuntu-lab-01",
  "name": "ubuntu-lab-01",
  "platform": "hyperv",
  "guest_family": "linux",
  "state": "running",
  "cpu": {
    "count": 2
  },
  "memory": {
    "startup_mb": 4096,
    "assigned_mb": 2048,
    "dynamic": false
  },
  "generation": 2,
  "storage": [
    {
      "kind": "vhdx",
      "path": "D:\\PureCVisor\\VMs\\ubuntu-lab-01\\disk0.vhdx",
      "size_gb": 40,
      "attached": true
    }
  ],
  "network": [
    {
      "switch": "Default Switch",
      "mode": "default-switch"
    }
  ],
  "console": {
    "type": "vmconnect",
    "available_local": true
  },
  "checkpoints": {
    "count": 1
  },
  "managed_by_purecvisor": true
}
```

PureCVisor가 만든 VM에는 Hyper-V Notes 또는 별도 metadata file로 관리 표식을 남긴다. 외부에서 만든 Hyper-V VM도 읽기는 가능하지만, MVP에서는 destructive 작업 전에 외부 VM임을 UI에서 명확히 표시한다.

## API

MVP API는 UI와 CLI가 함께 쓰는 단일 계약이다.

기본 endpoint 예시는 다음과 같다.

```text
GET    /api/v1/host/status
GET    /api/v1/vms
GET    /api/v1/vms/{id}
POST   /api/v1/vms
POST   /api/v1/vms/{id}/start
POST   /api/v1/vms/{id}/shutdown
POST   /api/v1/vms/{id}/poweroff
POST   /api/v1/vms/{id}/restart
GET    /api/v1/vms/{id}/checkpoints
POST   /api/v1/vms/{id}/checkpoints
POST   /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore
DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}
POST   /api/v1/vms/{id}/console/open
GET    /api/v1/jobs/{job_id}
POST   /api/v1/jobs/{job_id}/cancel
POST   /api/v1/jobs/{job_id}/retry
```

VM 생성은 long-running job으로 처리한다.

```text
POST /api/v1/vms
  -> 입력 검증
  -> job 생성
  -> PowerShell helper 실행
  -> 단계별 상태 업데이트
  -> 완료/실패 결과 저장
```

VM 생성 단계는 다음 상태로 노출한다.

```text
1. Validate host
2. Validate ISO
3. Create VM folder
4. Create VHDX
5. Create Hyper-V VM
6. Attach disk
7. Attach ISO
8. Attach Default Switch
9. Set boot order
10. Ready to boot
```

## 오류 처리

오류는 세 계층으로 나눈다.

```text
Validation error
- 잘못된 VM 이름
- ISO 파일 없음
- 디스크 크기 부족
- Hyper-V 비활성화
- 관리자 권한 없음

Backend error
- PowerShell cmdlet 실패
- VMMS 서비스 응답 실패
- Default Switch 없음
- VHDX 생성 실패

Operational error
- VM이 이미 실행 중
- checkpoint 복원 전 VM 상태 부적합
- 외부 VM에 destructive 작업 시도
```

응답은 사람이 읽는 메시지와 기계가 처리할 코드를 함께 둔다.

```json
{
  "error": {
    "code": "HYPERV_NOT_ENABLED",
    "message": "Hyper-V is not enabled on this host.",
    "detail": "Enable Hyper-V and restart Windows before creating VMs.",
    "retryable": false
  }
}
```

PowerShell helper는 텍스트 출력을 직접 파싱하지 않는다. 모든 helper는 성공과 실패 모두 표준 JSON으로 반환한다. API 데몬은 exit code, timeout, stderr를 함께 저장해 진단 화면에서 볼 수 있게 한다.

## PowerShell 실행 안전 규칙

MVP에서 PowerShell을 사용하더라도 사용자 입력을 command string에 직접 이어붙이면 안 된다.

안전 규칙은 다음과 같다.

```text
- operation allowlist 사용
- 파일 경로, VM 이름, 숫자 범위 사전 검증
- JSON 입력 파일 또는 stdin으로 인자 전달
- PowerShell 출력은 JSON 계약으로 고정
- PowerShell 실행 timeout 적용
- exit code, stderr, structured error 저장
- LAN 접근은 사용자가 명시적으로 켠 경우에만 허용
```

## Web Console UX

Web Console은 홈랩 운영자가 매일 보는 노드 대시보드다. 첫 화면은 마케팅 페이지가 아니라 운영 화면으로 시작한다.

첫 화면은 다음 정보에 집중한다.

```text
- Host health: Hyper-V, VMMS, admin mode, storage path, Default Switch
- Running VMs
- Stopped VMs
- Recent jobs
- Recent errors/events
- Host resource summary
```

VM 목록은 카드보다 밀도 있는 테이블을 기본으로 한다. 여러 VM의 상태, CPU, RAM, 디스크, 네트워크, checkpoint 수, 마지막 작업을 한 줄에서 비교할 수 있어야 한다.

VM 상세 화면은 다음 탭으로 나눈다.

```text
Overview
- 상태, CPU/RAM, uptime, boot media, network

Storage
- VHDX 경로, 크기, attach 상태

Checkpoints
- checkpoint 목록, 생성, 복원, 삭제

Jobs
- 이 VM에 관련된 최근 작업

Console
- 로컬에서는 VMConnect 열기
- 원격 접속에서는 VMConnect 사용 불가 안내와 대체 명령 표시
```

VM 생성 wizard는 MVP에서 4단계로 둔다.

```text
1. Name
2. ISO
3. Resources
4. Review & Create
```

기본값은 Linux 서버 홈랩 기준으로 잡는다.

```text
CPU: 2
RAM: 2048 또는 4096 MB
Disk: 32 또는 40 GB VHDX
Network: Default Switch
Generation: Gen 2 기본, 필요 시 Gen 1 옵션
Secure Boot: Linux ISO 호환 상태에 따라 안내
```

## CLI UX

CLI는 Web Console과 같은 API를 호출한다.

MVP 명령 예시는 다음과 같다.

```powershell
pcvcli host status
pcvcli vm list
pcvcli vm create ubuntu-lab-01 --iso D:\iso\ubuntu.iso --cpu 2 --memory 4096 --disk 40
pcvcli vm start ubuntu-lab-01
pcvcli vm shutdown ubuntu-lab-01
pcvcli vm poweroff ubuntu-lab-01
pcvcli vm checkpoint create ubuntu-lab-01 --name before-upgrade
pcvcli vm checkpoint restore ubuntu-lab-01 before-upgrade
pcvcli job show <job-id>
```

CLI의 목적은 GUI 대체가 아니라 자동화다. 기본 출력은 테이블로 제공하고, `--json` 옵션을 제공한다.

```powershell
pcvcli vm list --json
```

## UX 원칙

```text
- Hyper-V Manager보다 적은 클릭으로 VM 생성
- 실패하면 PowerShell 원문 오류만 던지지 않고 복구 행동을 제시
- 원격 접속 시 사용할 수 없는 로컬 기능은 숨기지 않고 명확히 표시
- 외부 Hyper-V VM과 PureCVisor 관리 VM을 구분
- destructive action은 확인 단계를 제공
```

## 검증 전략

MVP 검증은 API 응답만이 아니라 Hyper-V 실제 동작을 기준으로 잡는다.

```text
Level 1: Contract tests
- API request/response schema
- VM name/path/size validation
- PowerShell helper JSON 입출력 계약
- error code 표준화

Level 2: Mock backend tests
- Hyper-V 없이 backend adapter 동작 검증
- VM 생성 단계 state machine 검증
- job queue, timeout, retry, cancellation 검증

Level 3: Windows Hyper-V integration tests
- host status 조회
- VHDX 생성
- VM 생성
- ISO attach
- Default Switch attach
- start/stop/poweroff
- checkpoint create/restore/delete
- cleanup

Level 4: Manual homelab scenario
- Ubuntu/Debian ISO 설치
- 재부팅 후 VM 상태 정상 표시
- VMConnect 연결
- checkpoint 복원
- LAN off/on 설정 검증
```

## 로드맵

```text
Phase 0: Product spec
- 이 설계 문서
- MVP 범위와 비범위 확정
- 저장소 전략은 아직 결정하지 않음

Phase 1: Spike
- 완료: `spikes/purecvisor-desktop-node/hyperv/`
- Hyper-V PowerShell helper PoC 구현
- host status, vm list, create/start/stop/checkpoint 최소 검증
- JSON 계약과 Pester non-integration suite 확정

Phase 2: Local API daemon
- Phase 2A/2B/2C/2D/2E/2F/2G/2H 완료: `spikes/purecvisor-desktop-node/api/`
- localhost REST API skeleton
- `GET /api/v1/host/status`, `GET /api/v1/vms`
- `POST /api/v1/vms`, `GET /api/v1/jobs/{job_id}`
- Hyper-V helper process invocation
- in-memory job store
- FIFO worker queue
- bounded worker-pool tick via `-WorkerCount`
- optional JSON job persistence
- corrupt job store quarantine
- queued job cancellation
- failed job retry as a new queued job
- optional static Web Console file serving
- optional bearer-token gate for API and static file routes
- structured error
- runspace/threaded background workers는 MVP 이후로 분리

Phase 3: Web Console MVP
- Phase 3A 완료: 원래 `spikes/purecvisor-desktop-node/web/`, 2026-05-03 이후 제품 source는 `web/`
- host dashboard
- VM table
- create job form
- session job panel with cancel/retry controls
- optional bearer token request support
- Phase 3B 완료: VM detail drawer와 lifecycle job actions

Phase 4: CLI MVP
- 완료: `spikes/purecvisor-desktop-node/cli/`
- host status
- vm list/detail/create/start/stop/poweroff/restart
- checkpoint commands
- job get/cancel/retry
- --json output
- --api / --token options

Phase 5: LAN mode hardening
- 완료: LAN mode auth/token hardening
- 완료: bind address config
- 완료: Windows Firewall rule management
- 완료: audit/event log

Phase 6: Windows service packaging
- 완료: service config + binary path builder
- 완료: sc.exe command builder
- 완료: install/status/start/stop/uninstall injectable runner contract
- 완료: LAN service mode token policy

Phase 7: Service token file hardening
- 완료: Local API `-ApiTokenFile` token resolver
- 완료: inline token/token file conflict rejection
- 완료: service binary path `-ApiTokenFile` 전달
- 완료: service command line token value 노출 방지 계약

Phase 8: Installer hardening
- 완료: default token file path
- 완료: token file generation helper
- 완료: `icacls.exe` ACL command builder
- 완료: explicit service account
- 완료: elevated smoke 절차 문서화

Phase 9: Local API runtime hardening
- 완료: runtime policy read endpoint
- 완료: JSON file persistence 유지 결정
- 완료: manual retry attempt 상한
- 완료: queued-only cancel 유지 결정
- 완료: no CORS/OPTIONS, single bearer token auth 결정

Phase 10: Web Console/CLI productization follow-up
- 완료: Web Console checkpoint controls
- 완료: browser-local tracked job history
- 완료: CLI `--token-file`
- 보류: VMConnect launch, shell completion, interactive prompt

Phase 11: Product runtime promotion decision
- 완료: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 완료: `spikes/purecvisor-desktop-node/**` 격리 유지
- 완료: Single Edge release gate와 Desktop Node validation gate 분리
- Phase 14/20/22/25에서 일부 해소: WiX MSI-first installer source, unsigned dev/admin-smoke build, signed/internal `RequireSigned` MSI lifecycle evidence, .NET Windows Service Host replacement admin-smoke
- 보류: public trusted/stable signing, stable publication, Event Log writer/provider 기본 전환, route parity live MSI/admin-smoke evidence hardening, GA 제품 런타임 승격 재판정
```

## v2 후보

다음 기능은 MVP 이후로 미룬다.

```text
- PureCVisor-managed NAT + DHCP/DNS
- cloud image + cloud-init
- SSH quick-connect
- bundled static Web Console의 제품급 UX polish와 remote console 대안
- external switch / bridge mode
- Windows guest profile
- backup/export/import
- native C23/CIM backend
- reusable Linux/Windows common API model
```

## 리스크와 대응

### Hyper-V Default Switch 편차

리스크:

- Default Switch 동작이 Windows 버전과 호스트 네트워크 상태에 따라 달라질 수 있다.

대응:

- MVP에서는 Default Switch 존재와 연결 가능 여부만 진단한다.
- PureCVisor-managed NAT는 v2로 분리한다.

### ISO 설치 UX의 수동성

리스크:

- ISO 설치는 cloud-init보다 사용자의 수동 단계가 많다.

대응:

- MVP에서는 생성 wizard와 VMConnect 연결을 단순화한다.
- cloud image와 cloud-init은 v2 자동화 축으로 둔다.

### VMConnect 원격 제약

리스크:

- 원격 브라우저에서는 VMConnect를 직접 실행할 수 없다.

대응:

- 로컬 접속과 원격 접속 상태를 UI에서 구분한다.
- 원격 접속에는 대체 명령과 안내를 제공한다.
- 웹 콘솔은 v2로 미룬다.

### PowerShell 백엔드 안정성

리스크:

- PowerShell helper가 느리거나 오류 형식이 흔들릴 수 있다.

대응:

- JSON 입출력 계약을 강제한다.
- timeout, exit code, stderr, structured error를 저장한다.
- PowerShell runner를 백엔드 어댑터 뒤에 격리한다.

### 관리자 권한과 서비스 계정

리스크:

- Hyper-V 작업에는 관리자 권한과 적절한 서비스 계정 권한이 필요하다.

대응:

- Host Diagnostics에서 권한 상태를 먼저 확인한다.
- MVP에서는 권한 부족을 명확한 validation error로 처리한다.
- Phase 5에서는 방화벽 규칙을 opt-in으로 검증하고, Phase 6에서는 Windows service packaging 명령 계약을 검증한다. Phase 7에서는 서비스 command line에서 token 값 노출을 줄이기 위해 `-ApiTokenFile` 전달을 우선한다. Phase 8에서는 token file 생성과 ACL command builder를 service spike 안에서 검증한다. Phase 14에서는 WiX MSI-first installer source와 install/uninstall smoke를 packaging 경계에서 검증한다. Phase 11 기준 Desktop Node는 제품 런타임으로 승격하지 않으며, service 계정/ACL의 실제 적용, signed release build, update/rollback, service recovery는 제품 승격 gate로 남긴다.

## 참고 근거

- Microsoft Hyper-V 설치와 요구사항: https://learn.microsoft.com/en-us/windows-server/virtualization/hyper-v/get-started/Install-Hyper-V
- Microsoft Hyper-V NAT 네트워크 구성: https://learn.microsoft.com/en-us/virtualization/hyper-v-on-windows/user-guide/setup-nat-network
- Microsoft Windows Hypervisor Platform API 개요: https://learn.microsoft.com/en-us/virtualization/api/
- VMware Workstation Host VBS Mode와 WHP 사용: https://blogs.vmware.com/cloud-foundation/2020/05/28/vmware-workstation-now-supports-hyper-v-mode/
- Oracle VirtualBox 기술 배경: https://docs.oracle.com/en/virtualization/virtualbox/7.1/user/TechnicalBackground.html
