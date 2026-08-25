# PureCVisor Desktop Node P2 100% 완료 디자인 목업 패키지

## 목적

이 문서는 Web Console 최종 화면, installer 상태 흐름, service 설명 화면과 문구를 제품/개발/QA가 같은 기준으로 검토할 수 있게 만드는 P2 디자인 목업 handoff package다.

이 패키지는 구현 지시서가 아니라 화면 inventory와 상태별 copy 기준이다. Desktop Node의 현재 저장소 경계와 phase 결정을 유지하며, GA 승격이나 제품 runtime 교체를 주장하지 않는다.

## 결정 경계

이 문서는 다음 결정을 변경하지 않는다.

- `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- `DESKTOP_NODE_PHASE19_PROMOTION_REDECISION: evidence-first-keep-spike`
- `DESKTOP_NODE_PHASE22_RELEASE_VERSION_DECISION: channel-version-artifact-policy-with-keep-spike`
- `DESKTOP_NODE_PHASE25_MIXED_RUNTIME_TRANSITION_CANDIDATE: dotnet-core-typescript-web-powershell-adapter-first`

Desktop Node는 Windows 전용 독립 저장소다. Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime 화면이나 copy를 추가하지 않는다.

## 대상 사용자

- Windows PC를 홈랩 또는 개발용 로컬 virtualization node로 쓰는 운영자
- MSI 설치/복구/삭제 결과를 확인해야 하는 검증자
- Service, Local API, Web Console의 관계를 사용자에게 설명해야 하는 support/문서 작성자

## 화면 Inventory

### Web Console

필수 화면과 패널:

- Home dashboard
- Runtime connection banner
- Host status summary
- VM inventory table
- VM detail drawer 또는 detail panel
- Lifecycle job panel
- Create VM entry form
- Token/auth setup panel
- Diagnostics/download evidence panel
- Empty state
- Error state
- Unauthorized state
- LAN mode warning state

### Installer

필수 상태 화면:

- Install 준비
- Install 진행 중
- Install 성공
- Install 실패
- Repair 준비
- Repair 진행 중
- Repair 성공
- Repair 실패
- Uninstall 준비
- Uninstall 진행 중
- 기본 uninstall 성공
- 기본 uninstall 실패
- `REMOVE_DATA=1` uninstall 경고
- `REMOVE_DATA=1` uninstall 진행 중
- `REMOVE_DATA=1` uninstall 성공
- `REMOVE_DATA=1` uninstall 실패

### Service 설명

필수 설명 화면:

- Service status 설명
- Local API listener 설명
- Token source 설명
- Recovery policy 설명
- Diagnostics/event log 설명
- Admin opt-in 작업 설명

## Web Console 최종 UI 상태

### Home Dashboard 정상 상태

목표:

- 사용자가 현재 Windows host가 Desktop Node로 동작 중인지 즉시 알 수 있어야 한다.
- Web Console이 Local API와 연결됐는지, bearer token이 필요한지 명확히 보여준다.
- VM과 job 상태는 운영 표면으로 표시하되 GA 안정성을 암시하지 않는다.

주요 구성:

- 상단: 제품명 `PureCVisor Desktop Node`
- 상태 badge: `Connected`, `Token required`, `Service offline`, `LAN mode`
- Host card: Hyper-V 가능 여부, service 상태, API listener binding, version/channel
- VM table: 이름, 상태, CPU, memory, uptime, last job
- Job panel: 최근 job, 상태, 생성 시각, 실패 code, retry/cancel 가능 여부
- Diagnostics panel: evidence bundle, log 위치, 마지막 검증 시각

한국어 copy:

- 제목: `PureCVisor Desktop Node`
- 보조 설명: `이 Windows PC의 로컬 Desktop Node 상태를 확인합니다.`
- 연결 성공: `Local API에 연결되었습니다.`
- loopback 정책: `기본 연결은 이 PC의 loopback 주소에서만 허용됩니다.`
- LAN mode 표시: `LAN 모드는 명시적으로 허용된 경우에만 사용됩니다.`
- GA 비주장 문구: `이 화면은 Desktop Node 후보 runtime의 운영 증거를 보여주며 GA 승격을 의미하지 않습니다.`

English copy:

- Title: `PureCVisor Desktop Node`
- Subtitle: `View the local Desktop Node status for this Windows PC.`
- Connected: `Connected to the Local API.`
- Loopback policy: `The default listener is loopback-only on this PC.`
- LAN mode: `LAN mode is available only when explicitly enabled.`
- Non-GA note: `This console shows candidate runtime evidence and does not indicate GA promotion.`

### VM Inventory Empty State

조건:

- `GET /api/v1/vms`가 성공했지만 VM 목록이 비어 있다.

한국어 copy:

- 제목: `아직 표시할 VM이 없습니다.`
- 본문: `이 화면은 Local API가 반환한 VM inventory를 표시합니다. 실제 Hyper-V VM 생성은 관리자 opt-in 검증 또는 명시적 실행 경로에서만 수행합니다.`
- CTA: `VM 생성 요청 준비`

English copy:

- Title: `No VMs to show yet.`
- Body: `This view displays the VM inventory returned by the Local API. Real Hyper-V VM creation is limited to explicit or administrator-approved paths.`
- CTA: `Prepare VM request`

### Job Empty State

한국어 copy:

- 제목: `최근 job이 없습니다.`
- 본문: `VM lifecycle 또는 검증 작업을 실행하면 이 영역에 job 상태와 결과 code가 표시됩니다.`

English copy:

- Title: `No recent jobs.`
- Body: `Lifecycle and verification jobs will appear here with status and result codes.`

### Unauthorized State

조건:

- Local API가 `401` 또는 token required error를 반환한다.

한국어 copy:

- 제목: `API token이 필요합니다.`
- 본문: `보호된 token file 또는 승인된 token source를 사용해 Web Console 요청을 인증하세요. 장기 token 값을 command line이나 문서에 노출하지 마세요.`
- 입력 label: `Bearer token`
- 보조 action: `Token 저장 없이 이번 세션에만 사용`

English copy:

- Title: `API token required.`
- Body: `Authenticate Web Console requests with a protected token file or an approved token source. Do not expose long-lived token values on the command line or in documentation.`
- Field label: `Bearer token`
- Secondary action: `Use for this session only`

### Local API Offline/Error State

조건:

- Web Console static asset은 열렸지만 Local API 요청이 실패한다.

한국어 copy:

- 제목: `Local API에 연결할 수 없습니다.`
- 본문: `PureCVisor Desktop Node service 상태와 listener 설정을 확인하세요. 기본 listener는 loopback-only입니다.`
- 세부 정보: `오류 code와 HTTP 상태는 diagnostics에 남겨 support handoff에 사용합니다.`

English copy:

- Title: `Cannot reach the Local API.`
- Body: `Check the PureCVisor Desktop Node service status and listener configuration. The default listener is loopback-only.`
- Details: `Keep the error code and HTTP status in diagnostics for support handoff.`

### API Error State

조건:

- API가 `PCV_*` error code를 반환한다.

한국어 copy:

- 제목: `요청을 완료하지 못했습니다.`
- 본문: `오류 code, message, job id를 확인한 뒤 같은 요청을 재시도할 수 있는지 판단하세요.`
- 필드: `Code`, `Message`, `Job ID`, `Retry available`

English copy:

- Title: `The request could not be completed.`
- Body: `Review the error code, message, and job id before deciding whether the request can be retried.`
- Fields: `Code`, `Message`, `Job ID`, `Retry available`

### LAN Mode Warning State

조건:

- `-AllowLan` 또는 LAN listener 정책이 감지된다.

한국어 copy:

- 제목: `LAN mode가 활성화되어 있습니다.`
- 본문: `LAN mode는 명시적 opt-in과 token source가 있을 때만 허용됩니다. 방화벽 변경은 관리자 opt-in 검증으로만 수행합니다.`

English copy:

- Title: `LAN mode is enabled.`
- Body: `LAN mode is allowed only with explicit opt-in and a token source. Firewall changes are administrator opt-in verification only.`

## Installer 상태 Flow

### 공통 원칙

- installer copy는 자동 reboot를 약속하지 않는다.
- `REBOOT=ReallySuppress`와 같은 suppress 정책이 사용되더라도 사용자에게 "자동 재부팅 없음"을 명확히 표시한다.
- MSI install, repair, uninstall, `REMOVE_DATA=1`은 host mutation이다. 관리자 권한 opt-in 검증으로만 실행한다.
- Event Log source 등록, firewall 변경, service install/start/stop/delete는 관리자 opt-in 작업으로 표시한다.

### Install 준비

한국어 copy:

- 제목: `PureCVisor Desktop Node 설치 준비`
- 본문: `이 설치는 Windows service, Local API, Web Console wrapper를 구성합니다. 실제 Hyper-V 또는 firewall 변경은 명시적으로 승인된 경로에서만 수행합니다.`
- 체크 항목: `관리자 권한`, `설치 경로`, `token source`, `자동 재부팅 없음`

English copy:

- Title: `Ready to install PureCVisor Desktop Node`
- Body: `This installation configures the Windows service, Local API, and Web Console wrapper. Real Hyper-V or firewall changes are limited to explicitly approved paths.`
- Checklist: `Administrator privileges`, `Install path`, `Token source`, `No automatic reboot`

### Install 진행 중

한국어 copy:

- 상태 1: `파일을 설치하는 중`
- 상태 2: `Windows service wrapper를 구성하는 중`
- 상태 3: `Local API policy를 기록하는 중`
- 상태 4: `Web Console asset을 준비하는 중`
- 상태 5: `설치 evidence를 기록하는 중`

English copy:

- Step 1: `Installing files`
- Step 2: `Configuring the Windows service wrapper`
- Step 3: `Writing Local API policy`
- Step 4: `Preparing Web Console assets`
- Step 5: `Recording installation evidence`

### Install 성공

한국어 copy:

- 제목: `설치가 완료되었습니다.`
- 본문: `Desktop Node service와 Local API 상태를 확인하세요. Web Console은 기본적으로 이 PC에서만 접근하는 loopback 연결을 사용합니다.`
- 보조 문구: `GA 승격 여부는 별도 evidence gate에서 판단합니다.`

English copy:

- Title: `Installation complete.`
- Body: `Check the Desktop Node service and Local API status. By default, the Web Console uses a loopback connection on this PC only.`
- Secondary: `GA promotion is evaluated through a separate evidence gate.`

### Install 실패

한국어 copy:

- 제목: `설치를 완료하지 못했습니다.`
- 본문: `installer log, service 상태, Local API policy file, product wrapper status를 확인하세요. 실패 로그도 검증 evidence로 보존합니다.`

English copy:

- Title: `Installation could not be completed.`
- Body: `Review the installer log, service status, Local API policy file, and product wrapper status. Failed logs are preserved as verification evidence.`

### Repair 준비/진행/성공

한국어 copy:

- 준비 제목: `설치 복구 준비`
- 준비 본문: `복구는 설치된 파일과 service wrapper를 다시 확인합니다. 사용자 data와 protected token은 보존해야 합니다.`
- 진행 상태: `설치 상태 확인`, `파일 복구`, `service wrapper 확인`, `Local API 설정 보존 확인`
- 성공 제목: `복구가 완료되었습니다.`
- 성공 본문: `service, runtime policy, ProgramData 보존 상태를 확인하세요.`

English copy:

- Ready title: `Ready to repair the installation`
- Ready body: `Repair checks installed files and the service wrapper. User data and protected tokens must be preserved.`
- Progress: `Checking installation state`, `Repairing files`, `Checking service wrapper`, `Confirming Local API settings were preserved`
- Success title: `Repair complete.`
- Success body: `Verify service state, runtime policy, and ProgramData preservation.`

### Repair 실패

한국어 copy:

- 제목: `복구를 완료하지 못했습니다.`
- 본문: `설치 상태를 보존한 뒤 uninstall cleanup 가능 여부를 확인하세요. token 값이나 protected token blob은 로그에 노출하지 않습니다.`

English copy:

- Title: `Repair could not be completed.`
- Body: `Preserve the installation state before checking uninstall cleanup options. Do not expose token values or protected token blobs in logs.`

### 기본 Uninstall

한국어 copy:

- 준비 제목: `Desktop Node 제거 준비`
- 준비 본문: `기본 제거는 service와 설치 파일을 제거하지만 ProgramData의 운영 data는 보존합니다.`
- 진행 상태: `service 중지`, `service 등록 제거`, `설치 파일 제거`, `ProgramData 보존 확인`
- 성공 제목: `제거가 완료되었습니다.`
- 성공 본문: `기본 제거에서는 ProgramData가 보존됩니다. data까지 삭제하려면 명시적으로 REMOVE_DATA=1 경로를 사용해야 합니다.`

English copy:

- Ready title: `Ready to uninstall Desktop Node`
- Ready body: `Default uninstall removes the service and installed files but preserves operational data in ProgramData.`
- Progress: `Stopping service`, `Removing service registration`, `Removing installed files`, `Confirming ProgramData preservation`
- Success title: `Uninstall complete.`
- Success body: `Default uninstall preserves ProgramData. To remove data, use the explicit REMOVE_DATA=1 path.`

### 기본 Uninstall 실패

한국어 copy:

- 제목: `제거를 완료하지 못했습니다.`
- 본문: `service lock, 실행 중인 process, installer log를 확인하세요. 수동 정리 전 실패 evidence를 먼저 남깁니다.`

English copy:

- Title: `Uninstall could not be completed.`
- Body: `Check service locks, running processes, and installer logs. Record failure evidence before manual cleanup.`

### REMOVE_DATA=1 Uninstall

한국어 copy:

- 경고 제목: `운영 data까지 삭제합니다.`
- 경고 본문: `REMOVE_DATA=1 제거는 protected token, legacy raw token, job store, diagnostics, install log 등 Desktop Node data를 삭제 대상으로 포함합니다. 이 작업은 명시적 관리자 opt-in으로만 실행합니다.`
- 확인 문구: `Desktop Node data 삭제를 이해했습니다.`
- 진행 상태: `service 중지`, `설치 파일 제거`, `ProgramData 삭제`, `token artifact 삭제 확인`, `diagnostics 삭제 확인`
- 성공 제목: `Desktop Node data가 제거되었습니다.`
- 성공 본문: `REMOVE_DATA=1 제거 결과와 남은 파일 목록을 확인하세요. token 값은 evidence에 기록하지 않습니다.`

English copy:

- Warning title: `Operational data will be removed.`
- Warning body: `REMOVE_DATA=1 uninstall includes protected tokens, legacy raw tokens, job store, diagnostics, and install logs as removal targets. This path requires explicit administrator opt-in.`
- Confirmation text: `I understand Desktop Node data will be removed.`
- Progress: `Stopping service`, `Removing installed files`, `Removing ProgramData`, `Confirming token artifact removal`, `Confirming diagnostics removal`
- Success title: `Desktop Node data removed.`
- Success body: `Review the REMOVE_DATA=1 result and remaining file list. Do not record token values in evidence.`

### REMOVE_DATA=1 실패

한국어 copy:

- 제목: `data 제거를 완료하지 못했습니다.`
- 본문: `남은 ProgramData 파일 목록을 기록하되 token 값과 protected token blob은 기록하지 않습니다. process lock 또는 권한 문제를 먼저 확인하세요.`

English copy:

- Title: `Data removal could not be completed.`
- Body: `Record the remaining ProgramData file list, but do not record token values or protected token blobs. Check process locks or permission issues first.`

## Service 설명 화면과 Copy

### Service Status

한국어 copy:

- 제목: `Windows service가 Desktop Node runtime을 관리합니다.`
- 본문: `service는 Local API와 Web Console wrapper의 실행 상태를 관리합니다. service install/start/stop/delete는 host mutation이며 관리자 opt-in 검증에서만 수행합니다.`
- 상태 label: `Running`, `Stopped`, `Start pending`, `Stop pending`, `Unknown`

English copy:

- Title: `A Windows service manages the Desktop Node runtime.`
- Body: `The service manages the Local API and Web Console wrapper runtime state. Service install/start/stop/delete are host mutations and are limited to administrator opt-in verification.`
- Status labels: `Running`, `Stopped`, `Start pending`, `Stop pending`, `Unknown`

### Local API Listener

한국어 copy:

- 제목: `Local API는 기본적으로 loopback-only입니다.`
- 본문: `기본 listener는 이 PC 내부 요청만 받습니다. LAN mode는 명시적 -AllowLan 설정과 token source가 있을 때만 허용됩니다.`

English copy:

- Title: `The Local API is loopback-only by default.`
- Body: `The default listener accepts requests from this PC only. LAN mode is allowed only with explicit -AllowLan configuration and a token source.`

### Token Source

한국어 copy:

- 제목: `장기 token은 노출하지 않습니다.`
- 본문: `장기 token 값은 command line, log, 문서에 기록하지 않습니다. 보호된 token file 또는 승인된 token source를 사용합니다.`

English copy:

- Title: `Long-lived tokens are not exposed.`
- Body: `Long-lived token values are not written to the command line, logs, or documentation. Use a protected token file or an approved token source.`

### Recovery Policy

한국어 copy:

- 제목: `복구는 evidence를 보존한 뒤 수행합니다.`
- 본문: `service 시작 실패나 Local API 실패가 발생하면 installer log, service status, runtime policy, diagnostics를 먼저 보존합니다. 이후 retry, repair, uninstall cleanup 여부를 결정합니다.`

English copy:

- Title: `Recovery preserves evidence first.`
- Body: `When service startup or Local API checks fail, preserve installer logs, service status, runtime policy, and diagnostics before deciding on retry, repair, or uninstall cleanup.`

### Diagnostics/Event Log

한국어 copy:

- 제목: `진단 정보는 검증 handoff를 위한 evidence입니다.`
- 본문: `Event Log source 등록과 diagnostic bundle 생성은 관리자 opt-in 경계에 맞춰 수행합니다. token 값과 protected token blob은 evidence에 포함하지 않습니다.`

English copy:

- Title: `Diagnostics are verification handoff evidence.`
- Body: `Event Log source registration and diagnostic bundle creation follow administrator opt-in boundaries. Token values and protected token blobs are excluded from evidence.`

## 접근성 및 화면 규칙

- 상태는 색상만으로 구분하지 않고 label과 icon text를 함께 제공한다.
- error code, job id, service name은 복사 가능한 text로 제공한다.
- destructive action인 `REMOVE_DATA=1`은 별도 확인 문구를 요구한다.
- 긴 token 또는 path는 줄바꿈 가능 영역에 표시하고 기본적으로 masking한다.
- 한국어를 기본으로 두되 support handoff를 위해 핵심 English copy를 같은 의미로 유지한다.

## 검증 Handoff Checklist

목업 검토자는 다음을 확인한다.

- Web Console normal, empty, error, unauthorized, LAN mode 상태가 모두 존재한다.
- Installer install/repair/uninstall/`REMOVE_DATA=1` 흐름이 각각 준비, 진행, 성공, 실패 상태를 가진다.
- 기본 uninstall은 ProgramData 보존으로 설명된다.
- `REMOVE_DATA=1`은 data 삭제와 token evidence 제외를 명확히 설명한다.
- service 설명은 Local API, token source, recovery, diagnostics를 구분한다.
- copy가 GA 승격, Linux runtime, 자동 reboot를 암시하지 않는다.
- MSI, Hyper-V, firewall, Event Log, service mutation은 관리자 opt-in 경계로 표시된다.

## 명시적 Non-goals

이 문서에서 제외한다.

- Desktop Node GA 승격 선언
- spike path 제거 또는 product runtime 교체
- Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime UX
- Single Edge UI/API 통합
- 실제 이미지, Figma, screenshot, browser QA 산출물 생성
- Web Console 구현 변경
- Installer WiX/MSI 구현 변경
- Windows service install/start/stop/delete 실행
- Hyper-V VM 생성/삭제 실행
- Windows Firewall 변경
- Event Log source 등록 실행
- Task Scheduler 등록 실행
- `Restart-Computer` 또는 자동 reboot
- signed release build, MSI install/repair/uninstall, mutating update/rollback 실행
