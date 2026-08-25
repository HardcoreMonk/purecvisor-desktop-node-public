# PureCVisor Desktop Node 유저 가이드

작성 기준: 2026-07-14

운영자 대상 용어와 제품 경계 문구는 `docs/OPERATOR_SURFACE_TERMS.md`에 모은다.

PureCVisor Desktop Node는 Windows 10/11 Pro/Enterprise + Hyper-V host를 로컬에서 관리하는 내부 전용 서비스다. ADR-0011에 따라 활성 운영자 표면은 Web Console과 PCVCLI다. 일반 사용자는 Web Console을 먼저 사용하고, terminal automation은 PCVCLI를 사용하며, service/MSI/firewall/trust-store/LAN 변경은 관리자 opt-in 운영 절차로만 실행한다.

이 문서는 설치된 제품을 사용하는 방법을 설명한다. 기능별 사용 계약, 권한, 차단/실패 메시지 기준은 `docs/USER_FEATURE_USAGE_SPEC.md`를 따른다. 개발 검증, ADR, release evidence는 `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`를 따른다.

## 한눈에 보기

| 항목 | 값 |
|------|----|
| Windows service | `PureCVisorDesktopNode` |
| Web Console | `http://127.0.0.1/` |
| Web API | `http://127.0.0.1:7777/api/v1/...` |
| Command-line client | `pcvcli.exe` (`C:\Program Files\PureCVisor\DesktopNode\pcvcli.exe`) |
| 제품 루트 | `C:\Program Files\PureCVisor\DesktopNode` |
| 데이터 루트 | `%ProgramData%\PureCVisor\desktop-node` |
| Service host | `C:\Program Files\PureCVisor\DesktopNode\DesktopNode.Host.exe` |
| Protected token file | `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` |
| 설치 로그 | `%ProgramData%\PureCVisor\desktop-node\install.jsonl` |
| Service 로그 | `%ProgramData%\PureCVisor\desktop-node\service-logs\` |
| Diagnostic bundle | `%ProgramData%\PureCVisor\desktop-node\diagnostics\` |

배포 범위는 ADR-0006 기준 내부 사설망 전용 서비스다. Public trusted signing, trusted timestamp, 외부 stable publication/catalog upload, winget public submission, public stable installer URL, clean-host public signed smoke, 일반 사용자 대상 public release는 `out-of-scope`다.

2026-05-10의 0.41.5 설치본 운영 evidence는 당시 account login, target-backed noVNC와 운영자 client, service token rotation/revoke, Credential Manager, internal HTTPS/TLS lifecycle, Event Log default transition PASS를 기록한 historical predecessor다. Lifecycle/Packaging rebaseline도 `0.41.5-admin-smoke`에서 `0.41.6-admin-smoke`로 update 후 `0.41.5-admin-smoke` rollback까지 PASS했다. 이 기록은 내부 사설망 전용 historical evidence이며 현재 운영자 표면이나 public release를 정의하지 않는다.

## 시작하기

설치가 끝난 뒤 브라우저에서 Web Console을 연다.

```powershell
Start-Process "http://127.0.0.1/"
```

서비스 상태는 Windows service 기준으로 확인한다.

```powershell
Get-Service PureCVisorDesktopNode
```

Repository checkout이 있는 운영자/개발자 환경에서는 product wrapper status action으로 bearer-protected runtime policy까지 확인할 수 있다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
```

설치본은 제품 경로를 machine `PATH`에 등록하므로 새 터미널에서는 `pcvcli`를 전체 경로 없이 실행할 수 있다. CLI에서 token source를 생략하면 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`을 자동으로 읽는다.

## 명령줄 CLI

`pcvcli.exe`는 설치된 Local API를 호출하는 command-line client다. Web Console과 같은 API contract를 사용하고, service/MSI/firewall/trust-store/LAN mutation을 직접 실행하지 않는다.

```powershell
pcvcli host status
pcvcli --json vm list
```

전체 명령어 사용 설명서는 `docs/CLI_COMMAND_USAGE.md`를 따른다. Inline `--token <token>`은 지원하지만 반복 실행 script에서는 token source를 생략해 기본 protected token file을 사용하고, 별도 검증이 필요할 때만 `--protected-token-file`, `--token-env`, `--token-file`을 override로 지정한다.

## 웹 콘솔 연결

Web Console 상단의 connection form에서 다음 값을 사용한다. 설치 listener는 Web Console 포트와 Web API 포트를 분리하며, `/pcv-config.js`가 기본 API 값을 채운다.

| 필드 | 입력 |
|------|------|
| API | 기본값 `http://127.0.0.1:7777` |
| Token | 내부 운영자가 제공한 bearer token |

`Save`를 누르면 host status, VM inventory, tracked job 상태를 불러온다. `Refresh`는 현재 host와 VM 목록을 다시 조회한다. `Clear`는 브라우저에 입력된 token 값을 지운다.

Token 값은 command line, issue, 문서, diagnostic bundle에 기록하지 않는다. 설치된 service와 CLI 기본 실행은 protected token file을 사용하고, Web Console에는 운영자가 별도로 제공한 token만 입력한다.

`https://127.0.0.1:443/`은 현재 기본 Web Console listener가 아니다. HTTPS/TLS binding은 별도 운영 gate가 닫힌 뒤 적용할 수 있으며, 지금 사용자는 loopback HTTP Web Console과 bearer-protected Web API를 기준으로 한다.

### 탐색/Search

Web Console은 global search와 command palette를 제공한다. Search는 VM, job, network, troubleshooting surface를 빠르게 찾는 용도이고, command palette는 Windows Desktop Node에서 허용된 local view/action만 표시한다. Job list는 server-side pagination과 browser-local tracked jobs를 함께 보여주며, network inventory는 switch source/type/default/management OS/external adapter field를 filter 기준으로 볼 수 있다.

### 계정/RBAC/JWT

Web Console은 account login, JWT refresh/logout, session role, RBAC permission 상태를 표시한다. API route는 다음 contract를 사용한다.

| Route | 용도 |
|------|------|
| `POST /api/v1/auth/login` | username/password로 access/refresh JWT 발급 |
| `POST /api/v1/auth/refresh` | refresh token으로 access/refresh JWT 회전 |
| `POST /api/v1/auth/logout` | browser session token clear와 refresh/session revoke handoff |
| `GET /api/v1/auth/session` | 현재 account session 확인 |
| `GET /api/v1/auth/rbac` | role/permission matrix 확인 |

`login`, `refresh`, `logout` route는 bearer token 없이 호출할 수 있지만 각각 username/password, refresh token, session/revoke handoff 입력이 필요하다. 설치 service는 `%ProgramData%\PureCVisor\desktop-node\accounts.json`와 `%ProgramData%\PureCVisor\desktop-node\jwt-signing-key.txt` 경로를 알고 있다. 기본 bootstrap은 `no-default-account`다. loopback Web Console(`127.0.0.1` / `localhost` / `::1`)은
`POST /api/v1/auth/loopback-session`으로 짧은 JWT를 받으며 service token을 페이지에 넣지 않는다.
계정이 구성되면 이 경로는 `409 PCV_LOOPBACK_SESSION_DISABLED`로 닫히고 `POST /api/v1/auth/login`만
남는다. LAN 또는 비-loopback remote는 기존 service bearer 또는 계정 JWT가 필요하다.
`pcvcli`는 이 발급 route를 노출하지 않는다. Password/JWT/token 값은 Web Console, diagnostic bundle, 문서에 표시하지 않는다.

기본 role:

| Role | 권한 |
|------|------|
| `viewer` | read-only 상태 조회 |
| `operator` | read, VM/checkpoint/job/diagnostic 작업 queue, console handoff |
| `admin` | 전체 권한 |

### 콘솔/noVNC

Web Console은 선택된 VM의 console capability를 표시한다. 현재 Windows Desktop Node의 실제 console handoff는 Hyper-V `vmconnect` 기준이다.

| 항목 | 상태 |
|------|------|
| Windows console | `vmconnect` handoff |
| noVNC | Explicit noVNC target host/port가 구성되기 전까지 `not_configured`; 구성되면 WebSocket-to-VNC TCP bridge |
| Required permission | `console.view` |

noVNC bridge는 Windows Desktop Node listener의 opt-in bridge이며 기본 disabled다. Linux noVNC/WebSocket backend, KVM/libvirt console, browser-started host mutation은 이 제품 범위에 포함하지 않는다.

연결 상태는 다음처럼 해석한다.

| 상태 | 의미 | 조치 |
|------|------|------|
| `Connected` | API와 인증이 정상이다. | 그대로 사용한다. |
| `Auth required` | token이 없거나 거부됐다. | token 입력값을 확인한다. |
| `Error` | service/API/host 작업 중 오류가 발생했다. | alert의 `PCV_*` error code와 service 상태를 확인한다. |
| `Idle` | 아직 연결 요청 전이다. | API와 token을 입력하고 `Save` 또는 `Refresh`를 누른다. |

현재 Web Console에서 직접 제공하는 범위:

- host status와 dashboard summary
- VM create/list/detail
- VM start/shutdown/poweroff/restart/delete queued job
- VM media attach/eject queued job
- checkpoint create/list/restore/delete queued job
- 브라우저 세션 기준 `Tracked Jobs` get/cancel/retry/polling
- Operator Activity: paged server-side job list와 browser-local `Tracked Jobs`를 함께 표시
- Troubleshooting Center: host readiness, runtime/auth/network policy, token rotation operator handoff, diagnostic bundle operator handoff, common `PCV_*` error guide 표시
- Monitoring: service/API, VMMS, job backlog, failed job, token policy, LAN exposure, checkpoint warning 표시
- Network Inventory: Hyper-V switch source/type/default/management OS/external adapter field 표시
- VM filter, safer destructive confirmations, Web asset status 표시

다음 항목은 API 직접 호출 또는 운영자 도구 범위다.

- service token file rotation/revoke mutation
- Event Log source, firewall, trust-store, LAN, MSI/service mutation gate

### 운영 화면 구조

Web Console은 `Dashboard`, `Virtual Machines`, `Network`, `Jobs`, `Activity`, `Evidence`, `Troubleshooting` 화면으로 운영 흐름을 나눈다.

- `Dashboard`는 Ops Cockpit 메인 화면이다. Host readiness, VM/job count, runtime policy, priority warning, 최근 activity를 확인한다.
- `Virtual Machines`는 VM Workbench다. VM 검색, 선택된 VM 상세, lifecycle/checkpoint action, VM-local activity context를 확인한다.
- `Network`는 read-only Network Inventory 화면이다. Hyper-V switch topology를 확인한다.
- `Jobs`는 현재 브라우저 세션의 tracked job history를 확인한다.
- `Activity`는 server-side job snapshot과 request/correlation id를 확인한다.
- `Evidence`는 Batch Supervisor evidence 요약을 확인한다.
- `Troubleshooting`은 Incident Command 화면이다. 실패 job, runtime/auth/LAN/VMMS/checkpoint risk, token rotation handoff, diagnostic bundle handoff와 read-only 진단 가이드를 확인한다.

이 화면 구조는 새 OS mutation을 실행하지 않는다. 실제 VM lifecycle/checkpoint/delete action은 기존 queued job route와 확인 dialog를 그대로 사용한다.

## 대시보드

`Dashboard`는 Ops Cockpit 메인 화면이다. Host readiness, VM/job count, runtime policy, priority warning, recent activity를 한 화면에 묶어 운영자가 먼저 볼 위험을 정한다. Host details에는 Hyper-V 지원 여부, 관리자 권한, VMMS 상태, network inventory 같은 Local API 응답 필드가 표시된다.

Host가 ready가 아니면 먼저 Windows 기능과 service 상태를 확인한다.

```powershell
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All
Get-Service vmms
Get-Service PureCVisorDesktopNode
```

## 가상 머신 목록과 상세 보기

`Virtual Machines`는 VM Workbench 화면이다. VM search/filter로 inventory를 좁히고, VM 이름을 클릭하면 선택된 VM detail panel이 열린다. Lifecycle/checkpoint action과 VM-local activity context는 선택된 VM 기준으로 확인한다.

Detail panel에서 확인할 수 있는 주요 값:

- VM state/status
- VM id/name
- CPU, startup memory, assigned memory
- Hyper-V generation
- storage path
- network switch mapping
- checkpoint count
- PureCVisor managed marker

Native inventory parity가 불완전하면 API는 PowerShell helper fallback 없이 structured failure를 반환한다. 이 경우 alert의 error code를 운영자에게 전달한다.

## 네트워크 인벤토리

`Network`는 `GET /api/v1/network/inventory`를 읽어 Hyper-V switch inventory를 표시한다. 화면은 source, mutation mode, 전체 switch 수, default switch 수를 먼저 보여주고, switch별 name/type/default/management OS/external adapter field를 표로 표시한다.

이 화면은 read-only다. Hyper-V switch 생성/삭제, IP 주소 변경, firewall rule 변경은 실행하지 않는다. Native network inventory parity가 불완전하면 API가 structured failure를 반환하고, Web Console은 오류 코드를 alert 또는 Network 화면의 warning으로 표시한다.

## 가상 머신 생성

Dashboard의 `Create VM`을 눌러 VM 생성 dialog를 연다.

필수 입력:

| 필드 | 설명 |
|------|------|
| Name | 만들 VM 이름 |
| ISO path | host에서 접근 가능한 ISO 경로 |
| VM root | VM 파일을 둘 루트 디렉터리 |
| CPU | vCPU 수 |
| Memory MB | startup memory |
| Disk GB | 생성할 disk 크기 |
| Generation | 현재 제품 path는 Hyper-V Generation 2만 지원 |

`Queue Create Job`을 누르면 VM 생성 job이 queue에 들어간다. 결과는 `Tracked Jobs`에서 확인한다. Generation 1 request는 현재 `PCV_GENERATION_INVALID` structured failure로 반환된다.

## 가상 머신 전원 작업

VM detail panel에서 전원 작업을 실행한다.

| 작업 | 설명 |
|------|------|
| `Start` | VM 시작 job을 queue한다. |
| `Shutdown` | guest shutdown integration을 사용한다. Guest가 지원하지 않으면 `PCV_VM_SHUTDOWN_NOT_AVAILABLE`이 반환될 수 있다. |
| `Power off` | 강제 전원 종료 job을 queue한다. 확인 dialog가 뜬다. |
| `Restart` | 재시작 job을 queue한다. 확인 dialog가 뜬다. |
| `Save` | Hyper-V Saved 상태 저장 job을 queue한다. pause가 아니며, 확인 dialog가 VM 표시 이름과 현재 state를 보여 준다. |
| `Resume saved` | Saved 상태에서 재개 job을 queue한다. 현재 state가 `saved`가 아니면 `PCV_VM_NOT_SAVED`다. |
| `Manage VM` | existing Hyper-V VM에 managed marker를 붙이는 job을 queue한다. 확인 dialog는 Hyper-V 표시 이름과, 성공 후 이 VM이 managed delete 가드를 통과한다는 점, unmanaged delete 거절은 유지된다는 점을 보여 준다. |
| `Delete VM` | PureCVisor managed VM delete job을 queue한다. 실행 전 확인 dialog가 뜨며, running VM은 Web Console에서 먼저 `Power off`를 요구한다. |

전원 작업은 queued job으로 처리된다. 작업 직후 VM 목록이 바로 바뀌지 않으면 `Tracked Jobs`의 job 상태를 먼저 확인한다. `Save`와 `Resume saved`는 `pcvcli vm save <vm>` / `pcvcli vm resume-saved <vm>`과 같은 route다. `vm resume saved` 두 단어는 pause resume과 충돌하므로 쓰지 않는다.

`Manage VM`과 `pcvcli vm manage <vm> --yes`는 `POST /api/v1/vms/{id}/manage`를 쓴다. 실험실에서 먼저 만든 Hyper-V VM을 제품 delete 대상으로 쓰려면 이 opt-in이 필요하다. 성공 후 그 VM만 managed delete 가드를 통과한다. unmanaged VM delete는 계속 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 거절된다.

VM delete는 destructive host mutation이다. Web Console은 running VM delete를 먼저 차단하고, API는 PureCVisor managed marker가 없는 VM을 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단한다. Delete job 결과는 `Tracked Jobs`에서 확인한다.

## 가상 머신 미디어

VM detail의 media 영역에서 기존 Virtual DVD에 ISO를 다시 연결하거나 제거한다. USB passthrough와 DVD 드라이브 추가는 이 제품 범위가 아니다.

| 작업 | 설명 |
|------|------|
| `Attach media` | host에서 접근 가능한 ISO 경로를 넣고 attach job을 queue한다. 확인 dialog는 VM 표시 이름과 ISO 경로를 보여 준다. 이미 ISO가 있으면 기존 DVD `HostResource`를 덮어쓴다. |
| `Eject media` | 연결된 ISO를 제거하는 job을 queue한다. |

Web와 `pcvcli vm attach <vm> --iso <path>`는 `POST /api/v1/vms/{id}/attach`를 쓴다. `--iso_path`는 같은 body 키 `iso_path` alias다. ISO 파일이 없으면 `PCV_ISO_NOT_FOUND`, DVD가 없으면 `PCV_VM_DVD_DRIVE_NOT_FOUND`, 경로가 비면 `PCV_VM_ATTACH_ISO_REQUIRED`다. 결과는 `Tracked Jobs`에서 확인한다.

## 체크포인트 작업

VM detail panel의 checkpoint 영역에서 작업한다.

| 작업 | 설명 |
|------|------|
| `Refresh checkpoints` | 선택한 VM의 checkpoint 목록을 다시 조회한다. |
| `Create checkpoint` | 입력한 이름으로 checkpoint 생성 job을 queue한다. |
| `Restore` | 선택한 checkpoint로 복원 job을 queue한다. 확인 dialog가 뜬다. |
| `Delete` | 선택한 checkpoint 삭제 job을 queue한다. 확인 dialog가 뜬다. |

Checkpoint restore는 VM state에 민감하다. 검증된 smoke는 `vm.poweroff-before-restore` 조건을 사용했다. 운영 중 VM에서는 복원 전 workload 영향과 VM 전원 상태를 먼저 확인한다.

## 작업 job 확인, 취소, 재시도

`Tracked Jobs`는 현재 브라우저 세션에서 만든 job을 최대 50개까지 localStorage에 보관한다.

| 상태 | 의미 | 가능한 작업 |
|------|------|-------------|
| `queued` | worker가 아직 시작하지 않았다. | `Cancel` 가능 |
| `running` | worker가 처리 중이다. | `Cancel` 요청 가능 |
| `succeeded` | 작업이 완료됐다. | VM 목록을 refresh |
| `failed` | 작업이 실패했다. | retryable failure면 `Retry` 가능 |
| `canceled` | 취소됐다. | 필요하면 새 작업 생성 |

`Tracked Jobs` 목록은 브라우저별 세션 기록이다. 다른 브라우저나 다른 사용자 세션에서 만든 job은 이 panel에 자동으로 나타나지 않을 수 있다. 전체 server-side job snapshot은 `Operator Activity`에서 확인한다.

## 운영자 활동

`Operator Activity`는 Local API의 server-side job list와 현재 브라우저의 `Tracked Jobs`를 함께 보여준다. 같은 job id가 두 source에 있으면 server-side 상태를 기준으로 보고, browser-local 기록은 현재 브라우저에서 만든 작업 추적용으로만 사용한다.

Web Console은 기본으로 `GET /api/v1/jobs?limit=50&offset=0` 첫 page를 읽는다. API는 최대 `limit=200`까지 허용하고 `count`, `returned`, `next_offset`, retention metadata를 반환한다. Terminal job `succeeded`/`failed`/`canceled`는 최신 500개를 보존하고, `queued`/`running` active job은 보존한다. Persisted job store를 로드할 때도 같은 retention 기준이 적용된다. 더 오래된 운영 이력은 service log나 diagnostic bundle을 사용해 확인한다.

Activity row는 job id와 함께 `request_id` 또는 `correlation_id`가 있으면 표시한다. 이 값은 운영 지원과 장애 대조용 식별자이며 bearer token, certificate secret, VM credential, host secret이 아니다.

Activity 화면은 read-only 운영 visibility다. Job cancel/retry button은 기존 `/api/v1/jobs/{job_id}/cancel`, `/api/v1/jobs/{job_id}/retry` contract만 사용한다. Activity 화면은 Hyper-V, service, MSI, firewall, trust-store, LAN, Event Log mutation을 자동 실행하지 않는다.

## 문제 해결 센터

`Troubleshooting`은 Incident Command 화면이다. Failed jobs, runtime/auth/LAN/VMMS/checkpoint risk, host readiness, VMMS/Hyper-V 상태, runtime policy, token storage/source 종류, LAN exposure 상태, Diagnostic Bundle handoff, common `PCV_*` error guide를 read-only 진단 흐름으로 보여준다. Token 값과 Authorization header 값은 화면에 표시하지 않는다.

Token Rotation 패널은 protected token file root `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`, runtime policy token storage, browser token presence, listener exposure, `rotation handoff`, `no service token mutation` 경계를 표시한다. `Clear browser token`은 Web Console 입력/세션 token만 지우며 service protected token file, service configuration, MSI, firewall, trust-store, LAN, Event Log, update/rollback 상태를 변경하지 않는다. 실제 service token file rotation/revoke mutation은 아직 Web Console에서 실행하지 않는다.

Diagnostic Bundle 패널은 `%ProgramData%\PureCVisor\desktop-node\diagnostics` 출력 root, server-side bundle API, `CollectDiagnostics` product wrapper fallback, redaction boundary를 표시한다. 설치 listener가 diagnostic bundle API를 제공하면 Web Console에서 bundle 목록 조회, create, download를 실행할 수 있고, CLI에서는 `pcvcli --json diagnostics bundle list --limit 10 --offset 0`으로 같은 metadata 목록을 조회해 반환된 `bundle_id`를 download command 입력으로 사용한다. 지원되지 않는 listener에서는 product wrapper 수동 수집 절차를 안내한다. 목록은 최신순이며 기본 `limit=10`, `offset=0`, 최대 `limit=100` pagination을 사용한다. 응답은 `count`, `returned`, `next_offset`, bundle ID/시각/크기/download URL을 포함한다. 조회 시 기본 14일/최대 50개 retention을 적용하므로 만료·초과 bundle 파일이 diagnostics root에서 제거될 수 있다. 이 화면은 Hyper-V/VM, Event Log source registration, firewall rule, trust-store, MSI, service lifecycle, reboot, Task Scheduler 작업을 실행하지 않는다.

Console capability card의 전역 capability discovery는 API/Web Console 전용이며 `GET /api/v1/console/capabilities`로 listener의 local `vmconnect`
handoff와 optional noVNC bridge 상태를 조회한다. `console-access-card.v1`의 status,
`console.view` permission, token redaction, WebSocket path template, 다음 조치를 표시할 뿐 console
프로세스나 browser stream을 자동 생성하지 않는다. VM을 선택한 뒤 `GET
/api/v1/vms/{id}/console` 또는 `pcvcli vm console|vnc <vm>`으로 실제 VM별 session/handoff
metadata를 조회한다. noVNC가 `not_configured`이면 local Hyper-V `vmconnect`를 사용한다.

## 모니터링

`Monitoring`은 read-only 운영 신호를 보여준다. Service/API 연결 상태, VMMS 상태, active/failed job 수, token storage policy, LAN exposure 상태, checkpoint warning을 표시한다.

Checkpoint warning은 VM inventory의 checkpoint count와 선택된 VM checkpoint creation time을 기준으로 한다. Retention delete나 keep latest N 같은 destructive checkpoint mutation은 이 화면에서 실행하지 않는다.

## 운영자 흐름 다듬기

VM 목록의 filter는 name, id, state, note/error text를 기준으로 현재 inventory를 좁힌다. Power off와 Restart confirmation은 VM name/id/state와 queued job 결과 위치를 함께 보여준다. Sidebar의 asset status는 현재 served Web Console asset을 표시한다.

## 직접 API 호출

일반 사용은 Web Console을 권장한다. 운영자가 API를 직접 확인해야 할 때는 bearer token을 header로 넣는다. Token 값은 예시처럼 placeholder로만 다루고 기록하지 않는다.

```powershell
$headers = @{ Authorization = 'Bearer <internal-token>' }
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/runtime/policy' -Headers $headers
```

주요 API:

| Route | 용도 |
|------|------|
| `GET /api/v1/runtime/policy` | runtime/auth/job/native operation policy 확인 |
| `GET /api/v1/host/status` | host readiness 확인 |
| `GET /api/v1/network/inventory` | Hyper-V network inventory 확인 |
| `GET /api/v1/vms` | VM 목록 |
| `GET /api/v1/vms/{id}` | VM 상세 |
| `POST /api/v1/vms` | VM 생성 job queue |
| `POST /api/v1/vms/{id}/start` | VM start job queue |
| `POST /api/v1/vms/{id}/shutdown` | VM guest shutdown job queue |
| `POST /api/v1/vms/{id}/poweroff` | VM poweroff job queue |
| `POST /api/v1/vms/{id}/restart` | VM restart job queue |
| `POST /api/v1/vms/{id}/manage` | existing Hyper-V VM managed marker opt-in job queue |
| `DELETE /api/v1/vms/{id}` | managed VM delete job queue |
| `GET /api/v1/vms/{id}/checkpoints` | checkpoint 목록 |
| `POST /api/v1/vms/{id}/checkpoints` | checkpoint 생성 job queue |
| `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore` | checkpoint restore job queue |
| `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}` | checkpoint 삭제 job queue |
| `GET /api/v1/jobs` | server-side job snapshot 확인 |
| `GET /api/v1/jobs/{job_id}` | job 상태 확인 |
| `POST /api/v1/jobs/{job_id}/cancel` | job 취소 요청 |
| `POST /api/v1/jobs/{job_id}/retry` | retryable failed job 재시도 |
| `GET /api/v1/diagnostics/bundles?limit=10&offset=0` | Diagnostic bundle 목록, pagination, retention 결과 조회 |
| `POST /api/v1/diagnostics/bundles` | Redaction을 적용한 diagnostic bundle 생성 |
| `GET /api/v1/diagnostics/bundles/{bundle_id}/download` | Diagnostic bundle download |
| `GET /api/v1/console/capabilities` | vmconnect/noVNC transport와 console access 조건 조회 |
| `GET /api/v1/vms/{id}/console` | 선택 VM의 console session/handoff metadata 조회 |

VM delete는 managed marker guard를 둔다. PureCVisor가 관리하지 않는 VM은 provider mutation 전에 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단된다.

## 네트워크 LAN 노출

기본 실행은 loopback-only다. LAN exposure는 설치 기본값이 아니며 다음 조건을 모두 만족할 때만 운영한다.

- 관리자 opt-in
- `-AllowLan` 또는 동등한 explicit LAN mode
- token source
- firewall approval gate
- rollback/final-state proof
- reverse proxy 또는 외부 TLS terminator 계획

LAN mode에서는 non-loopback listener도 bearer token 정책을 따른다. Windows HttpListener에서 `http://0.0.0.0:7777/` 같은 wildcard prefix는 지원하지 않으므로 실제 LAN IP prefix를 사용한다.

## 서비스 운영

서비스 재시작:

```powershell
Restart-Service PureCVisorDesktopNode
```

서비스 상태 확인:

```powershell
Get-Service PureCVisorDesktopNode
```

Repository checkout이 있는 운영자 환경에서 diagnostic bundle을 수집한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
```

Diagnostic bundle은 token file 내용, protected token blob/hash, Authorization header를 복사하지 않는다.

## 제거와 데이터 보존

기본 uninstall은 `%ProgramData%\PureCVisor\desktop-node` 데이터를 보존한다.

MSI `REMOVE_DATA=1` 또는 product wrapper `-RemoveData`는 protected token file, legacy raw token file, job store, event log, install log, diagnostics allowlist를 삭제 대상으로 삼는다. MSI 경로에서는 `remove-installed --remove-data`가 바로 ProgramData를 삭제하지 않고 handoff descriptor를 만들며, 실제 삭제는 service absent precondition을 확인한 `data-root-remove --remove-data` action이 수행한다.

운영 로그를 보존해야 하면 remove-data를 실행하지 않는다.

## 문제 해결

| 증상 | 확인할 것 |
|------|-----------|
| Web Console이 열리지 않음 | `Get-Service PureCVisorDesktopNode`, `http://127.0.0.1/`, service logs |
| `Auth required` | token 입력 누락, 잘못된 token, token rotation 여부 |
| `PCV_AUTH_FORBIDDEN` | token이 service token과 다름 |
| Host not ready | Hyper-V feature, `vmms` service, 관리자 권한 |
| VM 목록이 비어 있음 | Hyper-V inventory, Default Switch, API error code |
| `PCV_GENERATION_INVALID` | VM 생성 request가 Generation 2인지 확인 |
| `PCV_VM_SHUTDOWN_NOT_AVAILABLE` | guest shutdown integration 설치/상태 확인, 필요 시 poweroff 영향 평가 |
| Job이 실패함 | `Tracked Jobs`의 code/detail, service logs, diagnostic bundle |
| LAN 접속 불가 | LAN opt-in 여부, 실제 LAN IP prefix, firewall rule final state, token |

## 보안 원칙

- Token 값을 command line, 문서, issue, diagnostic bundle에 남기지 않는다.
- 기본 exposure는 loopback-only다.
- VM delete는 Web Console에서 명시적 확인, bearer token, API managed marker guard를 거쳐 queued job으로 실행한다. 실제 installed destructive smoke나 OS mutation gate 재검증은 관리자 opt-in gate에서만 실행한다.
- LAN, firewall, trust-store, MSI install/remove 같은 OS mutation은 관리자 opt-in gate에서만 실행한다.
- Public trusted signing과 외부 stable publication은 현재 내부 전용 서비스 scope 밖이다.
- 실제 host mutation을 실행하면 rollback 또는 final-state proof를 남긴다.
