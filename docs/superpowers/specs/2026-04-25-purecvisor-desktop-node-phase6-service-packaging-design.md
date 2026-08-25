# PureCVisor Desktop Node Phase 6 Windows service packaging 설계

## 목적

Phase 6는 Desktop Node Local API listener를 Windows 서비스로 등록할 수 있는 packaging 경계를 만든다. Phase 5에서 LAN mode와 방화벽 opt-in 경계를 만들었으므로, 다음 단계는 사용자가 PowerShell 창을 열어 둔 상태가 아니어도 Local API를 백그라운드 서비스로 실행할 수 있는 명령 계약을 검증하는 것이다.

이 단계는 실제 설치 프로그램이나 tray app을 만들지 않는다. 관리자 권한이 필요한 서비스 변경은 opt-in 실행으로만 두고, 기본 검증은 `sc.exe` 명령 배열과 injectable runner로 수행한다.

## 현재 구현 상태

Phase 6 구현은 `spikes/purecvisor-desktop-node/service/`에 추가됐다.

- `PcvDesktopService.psm1`: service config, binary path, `sc.exe` command builder, injectable runner
- `Invoke-PcvDesktopService.ps1`: service config 출력, `-WhatIf` preview, 실제 command 실행 entrypoint
- `tests/PcvDesktopService.Contract.Tests.ps1`: service packaging contract 검증

Phase 7 이후 현재 service packaging은 `-ApiTokenFile` 경로 전달도 지원한다. Phase 6의 inline `-ApiToken` 지원은 짧은 수동 smoke용 호환 경로로 남기고, 장기 서비스 설치 예시는 token file을 우선한다.

## 포함 범위

Phase 6에 포함한다.

- 기본 서비스명 `PureCVisorDesktopNode`
- Local API listener를 `pwsh.exe -File Invoke-PcvDesktopApi.ps1 ...`로 실행하는 binary path 생성
- loopback 기본값 유지
- LAN service mode의 `-AllowLan` + non-empty `-ApiToken` 필수화
- `sc.exe create`, `description`, `failure`, `query`, `start`, `stop`, `delete` command builder
- `-WhatIf` 기반 명령 preview
- injectable runner 기반 install failure handling
- 서비스 README, 상위 문서, 검증 정책 업데이트

## 제외 범위

Phase 6에서 제외한다.

- MSI/MSIX/winget packaging
- tray app
- Windows Credential Manager token storage
- dedicated service account 생성/권한 부여
- Windows Event Log provider 등록
- 서비스 복구 정책의 실제 관리자 권한 통합 테스트 자동화
- production hardening installer
- Linux Single Edge runtime 또는 systemd 변경

## 서비스 명령 정책

서비스 설치 명령은 다음 구조를 따른다.

```text
sc.exe create PureCVisorDesktopNode binPath= "<pwsh command>" DisplayName= "PureCVisor Desktop Node" start= auto
sc.exe description PureCVisorDesktopNode "PureCVisor Desktop Node Local API service."
sc.exe failure PureCVisorDesktopNode reset= 86400 actions= restart/60000/restart/60000/""/60000
```

상태 확인과 제어는 다음 command builder를 사용한다.

```text
sc.exe query PureCVisorDesktopNode
sc.exe start PureCVisorDesktopNode
sc.exe stop PureCVisorDesktopNode
sc.exe delete PureCVisorDesktopNode
```

실제 실행은 elevated PowerShell을 요구할 수 있다. 일반 개발 검증은 `-WhatIf`나 Pester injectable runner를 사용한다.

## 보안 정책

서비스 packaging은 Phase 5 listener 정책을 반복 적용한다.

```text
loopback service prefix + no token      -> 허용
loopback service prefix + token         -> 허용
LAN service prefix + no -AllowLan       -> 거부
LAN service prefix + -AllowLan no token -> 거부
LAN service prefix + -AllowLan + token  -> 허용
```

서비스 계정 분리와 token 저장은 별도 packaging hardening 단계에서 다룬다. Phase 6는 token 값을 command line에 전달할 수 있음을 검증하지만, 장기 저장 방식의 최종 설계로 보지 않는다.

## 검증

Phase 6 기본 검증은 다음을 요구한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

현재 기대 결과:

- Service packaging: 8 passed, 0 failed
- Local API: 82 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0

## 완료 기준

Phase 6는 다음을 만족하면 완료다.

- service config가 Local API listener 실행 인자를 정확히 구성한다.
- LAN service mode가 token 없이 열리지 않는다.
- `sc.exe` command builder가 install/status/start/stop/uninstall을 구성한다.
- injectable runner가 성공과 실패를 구조화 결과로 반환한다.
- 서비스/API/CLI/Web 기본 검증이 통과한다.
- 상위 문서와 spike README가 Phase 6 상태를 반영한다.
