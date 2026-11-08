# PureCVisor Desktop Node 운영 가이드

이 문서는 설치된 PureCVisor Desktop Node를 사용하는 방법을 요약한다. 작성 정본은 저장소의 `docs/USER_GUIDE.md`이고, 제품 문서 포털(`docs.html`)은 같은 릴리스의 이 파일을 읽는다. 기능별 사용 계약, 권한, 차단/실패 메시지 기준은 `docs/USER_FEATURE_USAGE_SPEC.md`를 따른다.

PureCVisor Desktop Node는 Windows 10/11 Pro/Enterprise + Hyper-V host를 로컬에서 관리하는 내부 전용 서비스다. 활성 운영자 표면은 Web Console과 PCVCLI다. 일반 사용자는 Web Console을 먼저 사용하고, 자동화와 검증은 `pcvcli`를 사용한다.

## 1. 시작하기

### 1.1 한눈에 보기

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

### 1.2 첫 실행

설치가 끝난 뒤 브라우저에서 Web Console을 연다.

```powershell
Start-Process "http://127.0.0.1/"
```

서비스 상태는 Windows service 기준으로 확인한다.

```powershell
Get-Service PureCVisorDesktopNode
```

설치본은 제품 경로를 machine `PATH`에 등록하므로 새 터미널에서는 `pcvcli`를 전체 경로 없이 실행할 수 있다. CLI에서 token source를 생략하면 protected token file을 자동으로 사용한다.

```powershell
pcvcli host status
pcvcli --json vm list
```

### 1.3 배포 범위

배포 범위는 ADR-0006 기준 내부 사설망 전용 서비스다. Public trusted signing, trusted timestamp, 외부 stable publication, public installer URL은 현재 범위 밖이며 이 문서는 그것을 주장하지 않는다. 기본 listener는 loopback-only이고, LAN 노출은 관리자 opt-in gate 뒤에서만 운영한다([9.3 네트워크 LAN 노출](#93-네트워크-lan-노출)).

## 2. 웹 콘솔

### 2.1 접속과 세션

Web Console은 `http://127.0.0.1/`에서 열리고 `/pcv-config.js`가 기본 Local API 주소(`http://127.0.0.1:7777`)를 채운다. loopback 접속은 `POST /api/v1/auth/loopback-session`으로 짧은 JWT를 받으므로 service token을 페이지에 넣지 않는다. 계정이 구성되면 이 경로는 `409 PCV_LOOPBACK_SESSION_DISABLED`로 닫히고 로그인 페이지의 `POST /api/v1/auth/login`만 남는다.

환경설정 대화상자(`Ctrl+P`)에서 테마, 언어, Local API 주소, 선택적 브라우저 bearer token을 바꾼다. Token 값은 화면, 기록, 문서, diagnostic bundle에 남기지 않는다.

새 셸은 `Connected` / `Auth required` / `Error` / `Idle` 연결 배지를 그리지 않는다. 화면에 남는 문구는 다음이다. `Connected`와 `Auth required`는 모니터링 카드와 인증 게이트에만 남는다.

| 위치 | 보이는 문구 | 의미와 조치 |
|------|-------------|-------------|
| 로그인 카드 | `관리자 인증`, `보안 로그인`. `로컬 운영 계정으로 접속하세요. loopback 접속은 세션이 자동으로 열립니다.` 노드 상태 초기값은 `노드 응답 대기`다. | 계정이 있으면 로그인한다. loopback이고 계정이 없으면 세션이 자동으로 열린다. |
| 대시보드 hero chip | `활성 워크로드`, `호스트 상태`, `최근 경고`. 갱신 전 값은 `—`다. 호스트 상태는 요약 값이고, host status가 없으면 `—`, 있으면 `Needs attention` 또는 `Ready`다. | 대시보드가 호스트·VM·경고를 읽은 뒤에 숫자와 준비 상태가 채워진다. |
| 인증 게이트 | `#alert-region`의 `Auth required`. 기본 코드 `PCV_AUTH_REQUIRED`, 기본 메시지 `Authorization bearer token is required.` 안내 `Use the header api-token field or the account login form. Service tokens stay out of HTML.` troubleshooting이 아니면 `Username` / `Password` / `Login`. | access token과 브라우저 token이 둘 다 없고 연결 상태가 auth일 때 나온다. 로그인하거나 환경설정(`Ctrl+P`)의 브라우저 token을 확인한다. |
| 오류 배너 | `#alert-region`에 code, message, detail. 배지 단어 `Error`는 쓰지 않는다. | alert의 `PCV_*` code와 service 상태를 확인한다. |
| Monitoring Service/API | `Connected` 또는 `Not connected`. | 연결이 `connected`이면 `Connected`, 그 외는 `Not connected`다. |
| 이벤트 푸터 | `Live` 또는 `Offline`. | WebSocket이 열려 있으면 `Live`, 아니면 `Offline`이다. |

### 2.2 화면 구조

Web Console 사이드바는 `운영`(대시보드), `워크로드`(가상 머신, 템플릿), `인프라`(네트워크), `관제`(작업, 이벤트, 증적), `시스템`(계정, 진단), `도움말` 묶음으로 화면을 나눈다. 주소 hash `#/<view>`(예: `http://127.0.0.1/#/vms`)로 화면을 바로 연다.

- `대시보드`(`dashboard`)는 Ops Cockpit 메인 화면이다. Host readiness, VM/job count, runtime policy, priority warning, 최근 activity를 확인한다.
- `가상 머신`(`vms`)은 VM 목록과 상세다. 상태 칩으로 목록을 좁히고 VM을 고르면 상세 탭이 열린다(3.1).
- `템플릿`(`templates`)은 템플릿으로 잠긴 VM을 모아 보여 준다. 잠금은 VM 상세의 `더 보기` → `템플릿으로 잠금…`이다.
- `네트워크`(`network`)는 read-only Network Inventory 화면이다. Hyper-V switch topology를 확인한다.
- `작업`(`jobs`)은 현재 브라우저 세션의 tracked job history를 확인한다.
- `이벤트`(`activity`)는 server-side job snapshot과 request/correlation id를 확인한다.
- `증적`(`evidence`)은 Batch Supervisor evidence 요약을 확인한다.
- `계정`(`accounts`)은 계정 목록, 계정 만들기, 끄기, 비밀번호 재설정 화면이다. `account.manage` 권한이 있는 계정에만 보인다(2.4).
- `진단`(`troubleshooting`)은 Incident Command 화면이다. 실패 job, runtime/auth/LAN/VMMS/checkpoint risk, token rotation handoff, diagnostic bundle handoff와 read-only 진단 가이드를 확인한다.
- `도움말` 묶음(`helppage`)은 Local API route, 권한, CLI, Web Console 화면을 묶은 참조 카탈로그다.

이 화면 구조는 새 OS mutation을 실행하지 않는다. 실제 VM lifecycle/checkpoint/delete action은 queued job route를 쓰고, 확인이 필요한 작업은 브라우저 대화상자가 아니라 앱 안 확인 창으로 묻는다.

### 2.3 탐색과 명령 팔레트

사이드바 검색과 command palette(`Ctrl+K`)로 VM, job, network, troubleshooting 화면을 빠르게 찾는다. Command palette는 Windows Desktop Node에서 허용된 local view/action만 표시한다. 키보드 단축키 목록은 `?` 키로 연다.

### 2.4 계정, RBAC, JWT

Web Console은 account login, JWT refresh/logout, session role, RBAC permission 상태를 표시한다.

| Route | 용도 |
|------|------|
| `POST /api/v1/auth/login` | username/password로 access/refresh JWT 발급 |
| `POST /api/v1/auth/refresh` | refresh token으로 access/refresh JWT 회전 |
| `POST /api/v1/auth/logout` | browser session token clear와 refresh/session revoke handoff |
| `GET /api/v1/auth/session` | 현재 account session 확인 |
| `GET /api/v1/auth/rbac` | role/permission matrix 확인 |
| `POST /api/v1/auth/password` | 로그인한 계정이 자기 비밀번호를 바꾼다. 계정 access JWT만 받는다 |
| `POST /api/v1/accounts/{username}/password` | 관리자 비밀번호 재설정. admin(`account.manage`) 또는 서비스 bearer만, body에 `confirm_username`. loopback 세션은 거절 |

기본 role은 다음과 같다.

| Role | 권한 |
|------|------|
| `viewer` | read-only 상태 조회 |
| `operator` | read, VM/checkpoint/job/diagnostic 작업 queue, console handoff |
| `admin` | 전체 권한 |

Password/JWT/token 값은 Web Console, diagnostic bundle, 문서에 표시하지 않는다.

`계정` 화면에서 계정을 만들고 끄며, `비밀번호 재설정…`으로 새 비밀번호를 정한다. 비밀번호는 12자 이상이고 사용자 이름과 달라야 한다. 로그인한 계정은 상단 바의 `비밀번호 변경`에서 현재 비밀번호와 새 비밀번호를 넣어 바꾼다. 비밀번호가 바뀌면 `credential_version`이 올라가 이전 token은 `PCV_TOKEN_CREDENTIAL_STALE`로 거절되므로 다시 로그인한다. CLI는 `pcvcli account reset-password`, `pcvcli account change-password`이고 비밀번호는 인자가 아니라 환경 변수나 stdin으로만 넘긴다(`change-password`는 환경 변수만).

### 2.5 콘솔과 noVNC

Web Console은 선택된 VM의 console capability를 표시한다. Windows Desktop Node의 기본 console handoff는 Hyper-V `vmconnect`다.

| 항목 | 상태 |
|------|------|
| Windows console | `vmconnect` handoff |
| Browser console | VM 상세의 `콘솔` 탭. 기본 `1024x768`이고 `창에 맞춤`, `실제 크기`, `전체 화면`을 고른다. 입력 상태 칩은 `입력 중`, `콘솔을 클릭하면 입력`, `입력 권한 없음`이다. 화면은 `console.view`, 키 입력은 `console.input` 권한 |
| noVNC | Explicit noVNC target host/port가 구성되기 전까지 `not_configured`; 구성되면 WebSocket-to-VNC TCP bridge |
| Required permission | `console.view` |

noVNC bridge는 listener의 opt-in bridge이며 기본 disabled다. target 설정은 `pcvcli console novnc-target set|clear --yes`다. VM을 선택한 뒤 `GET /api/v1/vms/{id}/console` 또는 `pcvcli vm console <vm>`으로 VM별 session/handoff metadata를 조회한다.

## 3. 가상 머신

### 3.1 목록과 상세 보기

`가상 머신` 화면은 이름 검색과 상태 칩(`전체`, `실행 중`, `꺼짐`, `템플릿`, `실패`)으로 목록을 좁히고, VM을 고르면 상세가 열린다. 상세는 `요약`, `콘솔`, `체크포인트`, `하드웨어`, `게스트`, `작업 기록` 탭이고, 머리글에 상태에 맞는 전원 버튼과 `더 보기` 메뉴가 있다. 상세에서 VM state/status, id/name, CPU, startup/assigned memory, Hyper-V generation, storage path, network switch mapping, checkpoint count, PureCVisor managed marker를 확인한다.

Native inventory parity가 불완전하면 API는 PowerShell helper fallback 없이 structured failure를 반환한다. 이 경우 alert의 error code를 운영자에게 전달한다.

### 3.2 가상 머신 생성

`가상 머신` 화면의 `+ 새 VM`, 명령 팔레트의 `새 VM`, 또는 `Ctrl+N`으로 네 단계 마법사를 연다.

| 단계 | 입력 |
|------|------|
| `기본` | 이름, 세대. 서버는 Hyper-V Generation 2만 받는다 |
| `자원` | vCPU 1–32, 메모리 512–262144 MB, 디스크 8–4096 GB |
| `설치와 네트워크` | ISO 경로(필수), 스위치, 저장 위치(VM 루트. 비워 두면 서버 기본 위치) |
| `확인` | 입력 요약을 보고 `만들기`를 누른다 |

`만들기`를 누르면 VM 생성 job이 queue에 들어가고, 끝나면 `새 VM 열기`로 바로 연다. 각 단계는 요청 전에 값을 검사하고, 실패하면 코드와 고칠 칸을 알려 준다. 선택한 스위치는 만들기 요청에 실리지 않는다. 새 VM은 Default Switch에 연결되며 만든 뒤 `하드웨어` 탭에서 바꿀 수 있다. Generation 1 request는 `PCV_GENERATION_INVALID` structured failure로 반환된다.

### 3.3 전원 작업

VM 상세 머리글에서 전원 작업을 실행한다. 머리글은 상태에 맞는 버튼만 보여 준다(꺼짐 `시작`, 실행 중 `종료`·`다시 시작`, 일시 정지 `다시 실행`, 저장됨 `저장 상태 복귀`). 그 밖의 조작은 머리글의 `더 보기` 메뉴(`복제…`, `템플릿으로 잠금…`/`잠금 해제…`, `내보내기…`, `vmconnect로 열기`, `강제 끄기…`, `삭제…`)에 있고, `저장`, `미디어 꺼내기`, `VM 관리` 버튼도 상세에 있다. 모든 작업은 queued job으로 처리되고, VM 목록이 바로 바뀌지 않으면 `작업` 화면의 job 상태를 먼저 확인한다.

| 작업 | 설명 |
|------|------|
| `시작` | VM 시작 job을 queue한다. |
| `종료` | guest shutdown integration을 사용한다. Guest가 지원하지 않으면 `PCV_VM_SHUTDOWN_NOT_AVAILABLE`이 반환될 수 있다. |
| `강제 끄기…` | 강제 전원 종료 job을 queue한다. 앱 안 확인 창이 뜬다. |
| `다시 시작` | 재시작 job을 queue한다. 앱 안 확인 창이 뜬다. |
| `저장` | Hyper-V Saved 상태 저장 job을 queue한다. `pcvcli vm save <vm>`과 같은 route다. |
| `저장 상태 복귀` | Saved 상태에서 재개 job을 queue한다. 현재 state가 `saved`가 아니면 `PCV_VM_NOT_SAVED`다. |
| `VM 관리` | existing Hyper-V VM에 managed marker를 붙이는 job을 queue한다(`POST /api/v1/vms/{id}/manage`). |
| `복제…` | 복제 창의 첫 칸은 `새 VM 이름`이고 제안값은 `<원본>-clone`이다. `미리보기`가 복사 예정 바이트를 보여 주고, `복제 시작`이 managed VM의 독립 VHDX를 복사해 새 managed VM을 만드는 job을 queue한다(`POST /api/v1/vms/{id}/clone`). 끝나면 `새 VM 열기`로 연다. |
| `템플릿으로 잠금…` / `잠금 해제…` | VM을 템플릿으로 잠그거나 푼다. 잠긴 VM은 변경 작업을 거절하고 `템플릿` 화면에 모인다. |
| `삭제…` | managed VM delete job을 queue한다. 앱 안 확인 창에 VM 이름을 그대로 입력해야 실행된다. running VM은 먼저 꺼야 한다. |

VM delete는 destructive host mutation이다. Web Console은 running VM delete를 먼저 차단하고, API는 PureCVisor managed marker가 없는 VM을 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단한다.

### 3.4 미디어

VM 상세 `하드웨어` 탭의 ISO 칸에서 기존 Virtual DVD에 ISO를 다시 연결하거나 제거한다. USB passthrough와 DVD 드라이브 추가는 이 제품 범위가 아니다.

| 작업 | 설명 |
|------|------|
| `미디어 연결` | host에서 접근 가능한 ISO 경로를 넣고 attach job을 queue한다. 이미 ISO가 있으면 기존 DVD `HostResource`를 덮어쓴다. |
| `미디어 꺼내기` | 연결된 ISO를 제거하는 job을 queue한다. |

Web과 `pcvcli vm attach <vm> --iso <path>`는 `POST /api/v1/vms/{id}/attach`를 쓴다. ISO 파일이 없으면 `PCV_ISO_NOT_FOUND`, DVD가 없으면 `PCV_VM_DVD_DRIVE_NOT_FOUND`다.

### 3.5 체크포인트

VM 상세의 `체크포인트` 탭에서 작업한다.

| 작업 | 설명 |
|------|------|
| `목록 다시 읽기` | 선택한 VM의 checkpoint 목록을 다시 조회한다. |
| `체크포인트 만들기` | 첫 칸에 이름(1~128자, 같은 이름 불가)을 넣고 생성 job을 queue한다. |
| `복원…` | 선택한 checkpoint로 복원 job을 queue한다. 확인 창이 현재 상태를 잃는다는 점과 실행 중 여부를 알린다. |
| `삭제…` | 선택한 checkpoint 삭제 job을 queue한다. 확인 창이 뜬다. |
| 예약 | `간격(분)` 60~10080, `보존 개수` 1~32를 넣고 `미리보기` 뒤 `저장`한다. `예약 끄기`로 끈다. 저장하거나 끄면 `다음 실행`, `마지막 결과`, `상태`를 바로 다시 읽어 보여 준다. |

Checkpoint restore는 VM state에 민감하다. 운영 중 VM에서는 복원 전 workload 영향과 VM 전원 상태를 먼저 확인한다.

## 4. 네트워크 인벤토리

`Network`는 `GET /api/v1/network/inventory`를 읽어 Hyper-V switch inventory를 표시한다. 화면은 source, mutation mode, 전체 switch 수, default switch 수를 먼저 보여주고, switch별 name/type/default/management OS/external adapter field를 표로 보여준다.

이 화면은 read-only다. Hyper-V switch 생성/삭제, IP 주소 변경, firewall rule 변경은 실행하지 않는다. Native network inventory parity가 불완전하면 API가 structured failure를 반환하고, Web Console은 오류 코드를 alert로 보여준다.

## 5. 작업과 활동

### 5.1 Tracked Jobs

`Tracked Jobs`는 현재 브라우저 세션에서 만든 job을 최대 50개까지 localStorage에 보관한다.

| 상태 | 의미 | 가능한 작업 |
|------|------|-------------|
| `queued` | worker가 아직 시작하지 않았다. | `Cancel` 가능 |
| `running` | worker가 처리 중이다. | `Cancel` 요청 가능 |
| `succeeded` | 작업이 완료됐다. | VM 목록을 refresh |
| `failed` | 작업이 실패했다. | retryable failure면 `Retry` 가능 |
| `canceled` | 취소됐다. | 필요하면 새 작업 생성 |

다른 브라우저나 다른 사용자 세션에서 만든 job은 이 panel에 자동으로 나타나지 않을 수 있다. 전체 server-side job snapshot은 `Activity`에서 확인한다.

### 5.2 운영자 활동

`Activity`는 Local API의 server-side job list와 현재 브라우저의 `Tracked Jobs`를 함께 보여준다. 같은 job id가 두 source에 있으면 server-side 상태를 기준으로 본다. Web Console은 기본으로 `GET /api/v1/jobs?limit=50&offset=0` 첫 page를 읽고, API는 최대 `limit=200`까지 허용한다.

Activity row는 job id와 함께 `request_id` 또는 `correlation_id`가 있으면 표시한다. 이 값은 운영 지원과 장애 대조용 식별자이며 bearer token, certificate secret, VM credential, host secret이 아니다. Job cancel/retry button은 `/api/v1/jobs/{job_id}/cancel`, `/api/v1/jobs/{job_id}/retry` contract만 사용한다.

### 5.3 Evidence

`Evidence`는 Batch Supervisor evidence 요약을 읽기 전용으로 보여준다. 운영 current 수치(버전, MSI SHA, blocker)는 저장소의 current evidence 문서가 기준이며 이 화면은 그 요약을 표시한다.

## 6. 문제 해결과 모니터링

### 6.1 문제 해결 센터

`Troubleshooting`은 Incident Command 화면이다. Failed jobs, runtime/auth/LAN/VMMS/checkpoint risk, host readiness, runtime policy, token storage/source 종류, LAN exposure 상태, Diagnostic Bundle handoff, common `PCV_*` error guide를 보여준다.

Token Rotation 패널은 protected token file root, runtime policy token storage, browser token presence, listener exposure, `rotation handoff`, `no service token mutation` 경계를 표시한다. Diagnostic Bundle 패널은 diagnostics 출력 root, server-side bundle API, product wrapper fallback, redaction boundary를 표시한다.

Console capability card는 `GET /api/v1/console/capabilities`로 listener의 local `vmconnect` handoff와 optional noVNC bridge 상태를 조회한다. 콘솔 프로세스나 browser stream을 자동 생성하지 않는다.

### 6.2 모니터링

Monitoring 패널은 read-only 운영 신호를 보여준다. Service/API 연결 상태, VMMS 상태, active/failed job 수, token storage policy, LAN exposure 상태, checkpoint warning을 표시한다. Retention delete나 keep latest N 같은 destructive checkpoint mutation은 이 화면에서 실행하지 않는다.

### 6.3 증상별 확인

| 증상 | 확인할 것 |
|------|-----------|
| Web Console이 열리지 않음 | `Get-Service PureCVisorDesktopNode`, `http://127.0.0.1/`, service logs |
| `Auth required` | 세션 만료, token 입력 누락, 잘못된 token, token rotation 여부 |
| `PCV_AUTH_FORBIDDEN` | token이 service token과 다름 |
| Host not ready | Hyper-V feature, `vmms` service, 관리자 권한 |
| VM 목록이 비어 있음 | Hyper-V inventory, Default Switch, API error code |
| `PCV_GENERATION_INVALID` | VM 생성 request가 Generation 2인지 확인 |
| `PCV_VM_SHUTDOWN_NOT_AVAILABLE` | guest shutdown integration 설치/상태 확인, 필요 시 poweroff 영향 평가 |
| Job이 실패함 | `Tracked Jobs`의 code/detail, service logs, diagnostic bundle |
| LAN 접속 불가 | LAN opt-in 여부, 실제 LAN IP prefix, firewall rule final state, token |

Host가 ready가 아니면 먼저 Windows 기능과 service 상태를 확인한다.

```powershell
Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All
Get-Service vmms
Get-Service PureCVisorDesktopNode
```

## 7. 명령줄 CLI

`pcvcli.exe`는 설치된 Local API를 호출하는 command-line client다. Web Console과 같은 API contract를 사용하고, service/MSI/firewall/trust-store/LAN mutation을 직접 실행하지 않는다.

```powershell
pcvcli host status
pcvcli --json vm list
pcvcli vm save <vm>
pcvcli vm resume-saved <vm>
pcvcli vm manage <vm> --yes
pcvcli vm clone <source> --name <target> --yes
pcvcli vm attach <vm> --iso <path>
pcvcli vm console <vm>
```

전체 명령어 사용 설명서는 저장소 문서 `docs/CLI_COMMAND_USAGE.md`를 따른다. Inline `--token <token>`은 지원하지만 반복 실행 script에서는 token source를 생략해 기본 protected token file을 사용하고, 별도 검증이 필요할 때만 `--token-file`로 protected file 경로를 넘긴다.

## 8. 직접 API 호출

일반 사용은 Web Console을 권장한다. 운영자가 API를 직접 확인해야 할 때는 bearer token을 header로 넣는다. Token 값은 예시처럼 placeholder로만 다루고 기록하지 않는다.

```powershell
$headers = @{ Authorization = 'Bearer <internal-token>' }
Invoke-RestMethod -Uri 'http://127.0.0.1:7777/api/v1/runtime/policy' -Headers $headers
```

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
| `POST /api/v1/vms/{id}/clone/preview` | clone 미리보기(복사 예정 바이트) |
| `POST /api/v1/vms/{id}/clone` | managed VM clone job queue |

VM delete는 managed marker guard를 둔다. PureCVisor가 관리하지 않는 VM은 provider mutation 전에 `PCV_VM_NOT_MANAGED_BY_PURECVISOR`로 차단된다.

## 9. 서비스 운영

### 9.1 서비스 상태와 재시작

```powershell
Get-Service PureCVisorDesktopNode
Restart-Service PureCVisorDesktopNode
```

### 9.2 진단 번들

Diagnostic bundle은 Troubleshooting 화면의 handoff 또는 `POST /api/v1/diagnostics/bundles`로 만든다. Repository checkout이 있는 운영자 환경에서는 product wrapper의 `CollectDiagnostics` action을 쓴다. Diagnostic bundle은 token file 내용, protected token blob/hash, Authorization header를 복사하지 않는다.

### 9.3 네트워크 LAN 노출

기본 실행은 loopback-only다. LAN exposure는 설치 기본값이 아니며 다음 조건을 모두 만족할 때만 운영한다.

- 관리자 opt-in
- `-AllowLan` 또는 동등한 explicit LAN mode
- token source
- firewall approval gate
- rollback/final-state proof
- reverse proxy 또는 외부 TLS terminator 계획

LAN mode에서는 non-loopback listener도 bearer token 정책을 따른다. Windows HttpListener는 wildcard prefix를 지원하지 않으므로 실제 LAN IP prefix를 사용한다.

### 9.4 제거와 데이터 보존

기본 uninstall은 `%ProgramData%\PureCVisor\desktop-node` 데이터를 보존한다. MSI `REMOVE_DATA=1` 또는 product wrapper `-RemoveData`는 protected token file, legacy raw token file, job store, event log, install log, diagnostics allowlist를 삭제 대상으로 삼는다. 운영 로그를 보존해야 하면 remove-data를 실행하지 않는다.

## 10. 보안 원칙

- Token 값을 command line, 문서, issue, diagnostic bundle에 남기지 않는다.
- 기본 exposure는 loopback-only다.
- VM delete는 Web Console에서 명시적 확인, bearer token, API managed marker guard를 거쳐 queued job으로 실행한다.
- LAN, firewall, trust-store, MSI install/remove 같은 OS mutation은 관리자 opt-in gate에서만 실행한다.
- Public trusted signing과 외부 stable publication은 현재 내부 전용 서비스 scope 밖이다.
- 실제 host mutation을 실행하면 rollback 또는 final-state proof를 남긴다.
