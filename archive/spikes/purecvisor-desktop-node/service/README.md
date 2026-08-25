# PureCVisor Desktop Node Windows Service Packaging Spike

이 디렉터리는 Desktop Node Phase 6 Windows service packaging, Phase 7 service token file hardening, Phase 8 installer hardening, Phase 15 secure token storage, Phase 16 JSONL first diagnostics, Phase 17 LAN security policy, Phase 18 product wrapper update/rollback handoff, Phase 19 제품 승격 재판정 경계, Phase 25 service host candidate 경계를 검증한다. 목표는 Local API listener를 Windows 서비스로 등록할 수 있는 명령 계약과 protected token file 준비 계약을 만들되, 기본 검증에서는 실제 서비스 설치, 삭제, ACL 변경, 방화벽 변경, Event Log source 등록을 수행하지 않는 것이다.

Phase 19 기준 Desktop Node는 제품 런타임으로 승격하지 않고 `archive/spikes/purecvisor-desktop-node/**` 격리 spike를 구현 원천으로 유지한다. Phase 13 제품 wrapper의 실제 service host는 `packaging/windows-desktop-node/` 아래 WinSW wrapper가 담당했지만, 2026-05-01 Phase 25 replacement slice 이후 기본 제품 service host와 MSI installed custom action runner는 `DesktopNode.Host.exe`다. Phase 15 제품 기본 token source는 DPAPI LocalMachine protected token file이다. Phase 16 제품 diagnostics policy는 JSONL event/install log와 service host logs를 1차 운영 진단 경계로 유지하고, Phase 18 update/rollback은 product wrapper 경계에서만 다룬다. 이 service spike는 기존 `sc.exe` command builder와 token/protected token file 준비 계약을 component 경계로 유지한다. local test signer 기준 signed release build, elevated MSI lifecycle, Hyper-V lifecycle integration, Event Log source lifecycle, JSONL 장기 운영 evidence, service recovery smoke, `.NET Host` service/MSI admin-smoke는 2026-04-30/2026-05-01 evidence로 일부 닫혔지만, public trusted signing, stable publication, Event Log writer/provider 기본 전환, GA 승격은 별도 gate이며 root 결정은 `archive/spikes/purecvisor-desktop-node/README.md`와 `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase19-runtime-promotion-redecision-design.md`를 따른다.

Phase 25 기준 `src/DesktopNode.Service/**`는 .NET Service host contract를 제공하고 `src/DesktopNode.Host/**`가 기본 product service host 실행 파일을 제공한다. 이 service spike의 PowerShell helper는 component/manual smoke용으로 유지되며, 제품 설치된 `PureCVisorDesktopNode` service 재구성은 product wrapper 또는 MSI의 `DesktopNode.Host.exe service-action` 경로를 사용한다.

## 범위

- `PcvDesktopService.psm1`
  - Local API listener 실행용 service binary path 생성
  - loopback 기본값 유지
  - LAN service mode의 `-AllowLan` + `-ApiToken`, `-ApiTokenFile`, 또는 `-ApiTokenProtectedFile` 필수 조건 검증
  - long-lived service command line에 token 값 대신 `-ApiTokenFile` 또는 `-ApiTokenProtectedFile` 경로를 전달하는 계약 검증
  - `%ProgramData%\PureCVisor\desktop-node\api-token.txt` 기본 token file 경로 제공
  - `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` 기본 protected token file 경로 제공
  - token file 생성과 base64url 난수 token 생성
  - DPAPI LocalMachine protected token file 생성, 읽기, rotation, revoke
  - token 값을 stdout JSON에 직접 출력하지 않는 token file 준비 결과
  - `icacls.exe` 기반 token file ACL command builder
  - 기본 service account `LocalSystem`과 ACL principal `NT AUTHORITY\SYSTEM` 매핑
  - `sc.exe create`, `description`, `failure`, `query`, `start`, `stop`, `delete` 명령 배열 생성
  - injectable process runner 기반 install/uninstall/start/stop/status 검증
- `Invoke-PcvDesktopService.ps1`
  - service config 출력
  - token/protected token file 준비와 ACL command preview
  - `-WhatIf` 명령 preview
  - 실제 `sc.exe` 실행 entrypoint

Windows 서비스 설치, 삭제, 시작, 중지, token file ACL 적용, Windows Firewall rule 적용은 관리자 권한이 필요할 수 있다. 기본 Pester suite는 실제 시스템 변경을 실행하지 않고 command builder와 runner contract만 검증한다.

## 사용 예

서비스 설정만 확인한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Config
```

설치 명령을 preview한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Install -WhatIf
```

기본 token file 준비 명령을 preview한다. 이 명령은 파일을 만들지 않고 적용 예정인 `icacls.exe` 명령만 출력한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareTokenFile -WhatIf
```

기본 protected token file 준비 명령을 preview한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile -WhatIf
```

elevated PowerShell에서 기본 token file을 생성하고 ACL을 적용한다. 결과 JSON에는 token 값이 포함되지 않는다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareTokenFile
```

elevated PowerShell에서 기본 protected token file을 생성하고 ACL을 적용한다. 결과 JSON에는 token 값, protected token blob, token hash가 포함되지 않는다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile
```

사용자 지정 token file 경로와 service account를 지정할 수 있다. `LocalSystem`은 token file ACL에서 `NT AUTHORITY\SYSTEM`으로 매핑된다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 `
  -Action PrepareTokenFile `
  -ApiTokenFile 'D:\PureCVisor\desktop-node\api-token.txt' `
  -ServiceAccount 'LocalSystem'
```

사용자 지정 protected token file 경로를 지정할 수 있다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 `
  -Action PrepareProtectedTokenFile `
  -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json' `
  -ServiceAccount 'LocalSystem'
```

loopback Local API listener를 서비스로 설치한다. 실제 실행은 elevated PowerShell에서만 수행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 `
  -Action Install `
  -ServiceAccount 'LocalSystem' `
  -Prefix 'http://127.0.0.1:7777/' `
  -JobStorePath 'D:\PureCVisor\desktop-node\jobs.json' `
  -WebRootPath 'web' `
  -EventLogPath 'D:\PureCVisor\desktop-node\events.jsonl'
```

LAN mode listener를 서비스로 설치하려면 `-AllowLan`과 token source를 함께 지정한다. 장기 제품 서비스 설치에는 command line에 token 값을 직접 남기지 않도록 `-ApiTokenProtectedFile`을 우선한다.

먼저 protected token file을 준비한다. Phase 15 helper를 쓰면 기본 난수 token 생성, DPAPI 보호, ACL command 적용이 함께 수행된다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 `
  -Action PrepareProtectedTokenFile `
  -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json'
```

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 `
  -Action Install `
  -ServiceAccount 'LocalSystem' `
  -Prefix 'http://0.0.0.0:7777/' `
  -AllowLan `
  -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json' `
  -EnsureFirewallRule `
  -EventLogPath 'D:\PureCVisor\desktop-node\events.jsonl'
```

짧은 수동 smoke의 config 확인에는 `-ApiToken`도 사용할 수 있지만, inline token은 개발자/manual smoke 전용이다. `Install` command 생성은 inline token을 Windows service binary path에 영구 기록하지 않도록 `PCV_SERVICE_INLINE_TOKEN_INSTALL_FORBIDDEN`으로 거부한다. 제품 wrapper, MSI installed action, 장기 Windows service 경로는 command line에 token 값을 직접 남기지 않도록 `-ApiTokenProtectedFile`을 사용해야 한다. `-ApiToken`, `-ApiTokenFile`, `-ApiTokenProtectedFile` 중 둘 이상을 동시에 지정하면 `PCV_SERVICE_TOKEN_SOURCE_CONFLICT`로 거부된다.

## 서비스 계정과 ACL

기본 service account는 `LocalSystem`이다. `sc.exe create` command builder는 `obj= LocalSystem`을 명시한다. 이 계정의 token file ACL principal은 `NT AUTHORITY\SYSTEM`이다.

사용자 지정 계정을 쓰려면 계정 생성과 권한 부여를 이 helper 밖에서 먼저 끝내야 한다.

- 서비스 로그온 권한
- Hyper-V 관리 권한 또는 필요한 로컬 관리자 권한
- token/protected token file 읽기 권한
- job store, event log path, web root 접근 권한

token/protected token file ACL helper는 다음 형태의 명령을 만든다.

```text
icacls.exe <token-file> /inheritance:r
icacls.exe <token-file> /grant:r BUILTIN\Administrators:R NT AUTHORITY\SYSTEM:R
```

기본 suite는 위 명령 배열과 injectable runner만 검증한다. 실제 ACL 결과 확인은 elevated PowerShell에서 `icacls <token-file>` 또는 `icacls <protected-token-file>`로 수행한다.

Windows Event Log provider 등록은 기본 service spike에서 실행하지 않는다. Phase 16 기준 현재는 Phase 5 JSONL event log를 1차 운영 로그로 유지하고, provider/source lifecycle은 product wrapper의 admin opt-in registration plan으로만 노출한다.

서비스 상태 확인, 시작, 중지, 삭제:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Status
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Start
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Stop
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Uninstall
```

## 검증

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'archive/spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareTokenFile -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile -WhatIf
```

현재 기대 결과는 `docs/DEVELOPMENT_VERIFICATION_POLICY.md`를 따른다.

관리자 권한 통합 smoke 후보:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareTokenFile -ApiTokenFile 'D:\PureCVisor\desktop-node\api-token.txt'
icacls 'D:\PureCVisor\desktop-node\api-token.txt'
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareProtectedTokenFile -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json'
icacls 'D:\PureCVisor\desktop-node\api-token.dpapi.json'
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Install -ApiTokenProtectedFile 'D:\PureCVisor\desktop-node\api-token.dpapi.json'
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Start
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Status
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Stop
pwsh -NoProfile -ExecutionPolicy Bypass -File archive/spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Uninstall
```
