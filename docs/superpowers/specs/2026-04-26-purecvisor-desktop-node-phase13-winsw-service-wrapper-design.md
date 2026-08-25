# PureCVisor Desktop Node Phase 13 WinSW service wrapper 설계

## 목적

Phase 13은 Phase 12 관리자 권한 smoke에서 확인한 Windows service 시작 차단점을 해소하기 위한 서비스 실행 모델 결정 단계다.

Phase 12는 `packaging/windows-desktop-node/` wrapper가 제품 루트로 Local API, Web Console, Hyper-V helper 자산을 복사하고 `sc.exe create`로 `pwsh.exe -File Invoke-PcvDesktopApi.ps1`를 서비스로 등록했다. 실환경 관리자 smoke 결과, 제품 자산 복사, token file 준비/ACL, `sc.exe create`, `Status`, `CollectDiagnostics`, 기본 `Uninstall`은 실행됐지만 `sc.exe start PureCVisorDesktopNode`는 실패했다.

확인된 원인은 두 단계다.

- `binPath`가 `"pwsh.exe"` 상대 경로일 때는 SCM이 실행 파일을 찾지 못해 오류 2가 발생했다. 이 문제는 절대 경로 resolve로 보강됐다.
- 절대 경로 보강 후에도 `sc.exe start`가 1053으로 실패했다. `pwsh.exe -File Invoke-PcvDesktopApi.ps1`는 Windows Service Control Manager에 native service process로 응답하지 않기 때문이다.

Phase 13은 signed installer나 full updater를 구현하지 않는다. 먼저 Windows service start/stop/status가 실환경에서 성립하는 제품 후보 service host를 확정한다. 현재는 Phase 14-23에서 WiX MSI-first installer, signed RC MSI lifecycle, elevated MSI lifecycle, 운영/Event Log source lifecycle evidence까지 기록했지만, full updater와 public trusted/stable signing은 계속 후속 판단이다.

## 결정

```text
DESKTOP_NODE_PHASE13_SERVICE_DECISION: winsw-service-wrapper
```

Phase 13은 WinSW 기반 service wrapper를 채택한다.

직접 `pwsh.exe`를 SCM에 등록하지 않고, 제품 루트에 배치한 WinSW wrapper executable이 Windows service process가 된다. WinSW는 child process로 Local API listener를 실행하고, service start/stop/status/uninstall command surface를 제공한다.

이 결정의 직접 효과:

- Phase 12의 PowerShell Local API listener, static Web Console, Hyper-V helper 구현은 유지한다.
- Windows SCM에는 WinSW wrapper executable을 등록한다.
- service start/stop/status는 WinSW command를 기준으로 검증한다.
- `sc.exe create` command builder는 Phase 13 제품 기본 경로에서 내려오고, 필요 시 low-level fallback 또는 legacy preview로만 남긴다.
- native .NET service host는 Phase 25에서 side-by-side service host candidate contract로만 추가됐고 제품 service host를 교체하지 않는다.
- public trusted code signing chain 운용, full updater, Windows Credential Manager, 내장 LAN TLS는 후속 판단으로 유지한다.

## 사용자 승인 범위

2026-04-26 대화에서 다음을 승인했다.

- Phase 13 범위는 Windows native service host/service wrapper 결정으로 한다.
- 외부 service wrapper binary 사용을 허용한다.
- 1차 선택안은 WinSW wrapper 허용이다.

## 외부 근거

- Microsoft Windows Service guidance는 .NET Worker Service가 Windows Service로 동작하려면 Windows service integration을 명시적으로 구성하고, service의 current directory가 `C:\WINDOWS\system32`일 수 있으므로 절대 경로 기준이 필요하다고 안내한다.
- WinSW는 임의 executable을 Windows service로 감싸고 관리하는 wrapper이며, XML config로 executable, arguments, logging, stop timeout, failure action을 정의한다.

참고 링크:

- Microsoft Learn: `https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service`
- Microsoft Learn ASP.NET Core Windows Service: `https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service`
- WinSW: `https://github.com/winsw/winsw`
- WinSW XML config: `https://github.com/winsw/winsw/blob/v3/docs/xml-config-file.md`

## 대안 비교

| 대안 | 장점 | 단점 | 판정 |
|------|------|------|------|
| WinSW wrapper | 현재 PowerShell listener를 유지하면서 SCM 1053 문제를 가장 작게 해소한다. XML config로 logs, stop timeout, failure action을 표현할 수 있다. 관리자 smoke를 빠르게 재개할 수 있다. | 외부 binary 공급, 버전 pinning, hash 검증, 향후 signing 정책이 필요하다. | 채택 |
| 자체 .NET native service host | Microsoft 공식 Worker Service 모델과 잘 맞고 장기 제품성이 좋다. 단일 self-contained executable로 배포하기 쉽다. | 새 .NET project, host lifecycle, config bridge, publish pipeline을 추가해야 한다. Phase 13 범위가 커진다. | 후속 후보 |
| NSSM 등 다른 wrapper | 빠른 검증이 가능하다. | 제품 문서화, 유지보수, 배포 신뢰도, 설정 구조 면에서 WinSW보다 낮다. | 제외 |

## 아키텍처

Phase 13 제품 service 구조:

```text
C:\Program Files\PureCVisor\DesktopNode\
  api\
    Invoke-PcvDesktopApi.ps1
    PcvDesktopApi.psm1
  web\
    index.html
    app.js
    styles.css
  hyperv\
    Invoke-PcvHyperV.ps1
    PcvHyperV.psm1
  service\
    PcvDesktopService.psm1
  winsw\
    PureCVisorDesktopNode.exe
    PureCVisorDesktopNode.xml
  product-manifest.json
```

`PureCVisorDesktopNode.exe`는 WinSW executable을 제품 service name에 맞춰 배치한 파일이다. `PureCVisorDesktopNode.xml`은 같은 디렉터리에 배치하고 service id, display name, executable, arguments, log path, stop timeout, failure action을 정의한다.

제품 데이터 루트는 Phase 12 계약을 유지한다.

```text
%ProgramData%\PureCVisor\desktop-node\
  api-token.txt
  jobs.json
  events.jsonl
  install.jsonl
  diagnostics\
  service-logs\
```

## WinSW artifact 정책

Phase 13 구현은 WinSW binary를 코드에 숨겨 두지 않는다.

허용되는 입력:

- `-WinSwPath <path>`: 관리자가 명시한 WinSW executable
- 제품 packaging tool이 준비한 pinned WinSW executable

제품 manifest에는 다음 값을 기록한다.

- WinSW source path
- staged wrapper path
- WinSW file SHA-256
- WinSW version 또는 파일명에서 확인 가능한 release identifier

Phase 13에서는 WinSW binary signing 자체를 해결하지 않는다. Phase 14 installer build는 WinSW provenance와 unsigned dev MSI build를 검증하고, signed release build 단계에서 PureCVisor 배포 binary의 서명, 원본 release 검증, 재배포 정책을 확정한다. Phase 13 admin smoke는 사용한 WinSW binary의 SHA-256을 기록해야 한다.

## WinSW XML 계약

XML config는 제품 wrapper가 생성한다. 사용자가 직접 편집하는 설정 파일로 취급하지 않는다.

필수 항목:

- `id`: `PureCVisorDesktopNode`
- `name`: `PureCVisor Desktop Node`
- `description`: `PureCVisor Desktop Node Local API service.`
- `executable`: 절대 경로의 `pwsh.exe`
- `arguments`: Local API listener 실행 인자
- `workingdirectory`: 제품 루트
- `logpath`: `%ProgramData%\PureCVisor\desktop-node\service-logs`
- `log mode`: `roll`
- `stoptimeout`: `15 sec`
- `onfailure`: 1차/2차 restart, 이후 none

Local API listener arguments:

```powershell
-NoProfile
-ExecutionPolicy Bypass
-File "<ProductRoot>\api\Invoke-PcvDesktopApi.ps1"
-Prefix "http://127.0.0.1:7777/"
-HelperScriptPath "<ProductRoot>\hyperv\Invoke-PcvHyperV.ps1"
-JobStorePath "%ProgramData%\PureCVisor\desktop-node\jobs.json"
-WebRootPath "<ProductRoot>\web"
-ApiTokenFile "%ProgramData%\PureCVisor\desktop-node\api-token.txt"
-EventLogPath "%ProgramData%\PureCVisor\desktop-node\events.jsonl"
-WorkerCount 1
-TimeoutSec 30
```

인자는 XML escaping을 거친다. 경로는 모두 절대 경로로 생성한다. service current directory에 의존하지 않는다.

## Product action 변경

Phase 13 이후 제품 wrapper action은 WinSW를 기준으로 실행한다.

| Action | Phase 13 동작 |
|--------|---------------|
| `Plan` | WinSW executable, XML config, service logs, command list, SHA-256 기록 대상을 JSON으로 출력 |
| `Install` | 자산 복사, token 준비, WinSW executable/XML staging, `winsw install`, `winsw start`, health check |
| `Status` | `winsw status`와 manifest 존재 여부, service log path를 보고 |
| `CollectDiagnostics` | 기존 bundle에 WinSW XML redaction본, wrapper log, stdout/stderr log를 추가 |
| `Uninstall` | `winsw stop`, `winsw uninstall`, 제품 루트 제거, 기본 데이터 보존 |
| `Rollback` | service stop 후 previous product root 복원, WinSW XML 재생성, `winsw start`, health check |
| `Update` | full updater 전까지 계속 unsupported 또는 dry-run only |

`sc.exe query`는 status 보조 정보로 사용할 수 있지만 install/start/stop/uninstall의 기준 command는 WinSW다.

## Web Console 접근 방식

Phase 13은 service wrapper 결정이 중심이지만, 관리자 smoke에서 Web Console root 응답도 검증해야 한다.

제품 기본값은 `-ApiTokenFile`을 유지한다. API route는 bearer token을 계속 요구한다.

Phase 13은 loopback prefix의 Web Console static asset을 무인증으로 제공한다. API route는 bearer token을 계속 요구한다. 이유는 사용자가 브라우저에서 `/`를 먼저 열어 token을 입력해야 하기 때문이다. non-loopback LAN mode에서는 static asset도 bearer token 정책을 유지하거나 별도 TLS/auth 정책이 결정될 때까지 제품 기본값으로 열지 않는다.

static asset 인증 예외는 WinSW wrapper 전환과 독립된 구현 task로 분리한다. Phase 13 관리자 smoke에서 Web Console root는 무인증 `GET /` 200 응답으로 검증한다.

## 로그와 진단

Phase 13 diagnostic bundle은 Phase 12 항목에 다음을 추가한다.

- WinSW XML redaction본
- WinSW wrapper log
- child stdout/stderr log
- WinSW executable path와 SHA-256
- `winsw status` 결과
- 마지막 service start/stop command 결과

redaction 대상:

- token 값
- Authorization header
- API token file 내용
- source/product/data root의 전체 경로
- WinSW XML 내부의 민감 경로는 `[PRODUCT_ROOT]`, `[DATA_ROOT]`, `[SOURCE_ROOT]`로 치환

## 보안 경계

Phase 13 기본값:

- listener는 loopback only다.
- service account 기본값은 Phase 12와 동일하게 `LocalSystem`이다.
- token file은 `%ProgramData%\PureCVisor\desktop-node\api-token.txt`를 유지한다.
- inline token은 service product install에서 계속 금지한다.
- WinSW binary는 SHA-256을 기록하고, 향후 signed installer 단계에서 서명과 원본 검증을 확정한다.
- WinSW XML은 제품 wrapper가 생성하며, token 값을 포함하지 않는다.
- service logs는 token과 Authorization header를 diagnostic bundle에서 redaction한다.

## 검증 기준

관리자 권한 없는 기본 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

새 packaging tests는 다음을 검증한다.

- WinSW artifact path가 없으면 mutating install이 명확한 오류로 실패한다.
- `Plan`은 WinSW executable/XML/log path와 SHA-256 기록 대상을 포함한다.
- WinSW XML은 절대 경로, token file, event log, web root, worker/timeout 인자를 포함한다.
- XML은 token 값을 포함하지 않는다.
- `Install` orchestration은 copy, token, winsw stage, winsw install, winsw start, health check 순서를 따른다.
- `Status`, `CollectDiagnostics`, `Uninstall`, `Rollback`은 WinSW command 결과를 포함한다.
- 기존 `sc.exe create` 기반 assertion은 WinSW 기반 assertion으로 교체한다.

관리자 권한 opt-in smoke:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install -WinSwPath '<winsw.exe>'
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
$token = Get-Content "$env:ProgramData\PureCVisor\desktop-node\api-token.txt" -Raw
$headers = @{ Authorization = "Bearer $token" }
Invoke-WebRequest http://127.0.0.1:7777/api/v1/runtime/policy -Headers $headers
Invoke-WebRequest http://127.0.0.1:7777/
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall
```

성공 기준:

- service가 `RUNNING` 상태가 된다.
- token 포함 runtime policy 요청이 200을 반환한다.
- Web Console root 무인증 요청이 loopback에서 200을 반환한다.
- diagnostic bundle에 WinSW 관련 artifact가 포함되고 token 값은 포함되지 않는다.
- 기본 uninstall 후 service 등록과 제품 루트가 제거되고 데이터 루트는 보존된다.

서비스 시작 차단점이 해소된 뒤 이어서 판정할 항목:

- 실제 `Rollback`
- 실제 `Uninstall -RemoveData`
- 실제 Hyper-V VM create/start/poweroff/checkpoint lifecycle integration

## 제외 범위

Phase 13에서 하지 않는다.

- signed release MSI/MSIX 작성
- code signing
- full updater
- DPAPI 또는 Windows Credential Manager
- Windows Event Log provider
- LAN TLS
- multi-user auth
- native .NET service host 구현
- Hyper-V lifecycle 통합 성공 판정

## 후속 단계

- Phase 13 구현 계획: WinSW command builder, XML generator, product action 전환, admin smoke 기록
- Phase 14: WiX MSI-first installer와 repair/uninstall UX
- Phase 15 후보: DPAPI 또는 Windows Credential Manager 기반 token storage
- Phase 16 후보: Windows Event Log provider와 long-term diagnostics
