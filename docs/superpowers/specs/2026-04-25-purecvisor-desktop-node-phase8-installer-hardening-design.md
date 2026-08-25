# PureCVisor Desktop Node Phase 8 installer hardening 설계

## 목적

Phase 8은 Phase 7의 `-ApiTokenFile` 전달 계약을 실제 설치 준비 단계로 끌어올린다. Phase 7은 장기 token 값이 Windows 서비스 command line에 직접 남지 않도록 만들었지만, token file 생성, 저장 위치, 파일 ACL, 서비스 실행 계정, 관리자 권한 smoke 절차는 아직 후속으로 남아 있었다.

이번 단계는 여전히 `spikes/purecvisor-desktop-node/` 아래의 Windows Desktop Node spike 범위다. MSI/MSIX 설치 프로그램을 만들지 않고, 서비스 packaging entrypoint에 설치 준비 helper를 추가한다. 실제 서비스 설치, 방화벽 적용, ACL 확인은 관리자 권한 수동 smoke로 분리한다.

## 현재 구현 상태

Phase 8 구현은 `spikes/purecvisor-desktop-node/service/`에 추가됐다.

- `PcvDesktopService.psm1`
  - `Get-PcvDesktopServiceDefaultTokenFilePath`
  - `New-PcvDesktopServiceToken`
  - `New-PcvDesktopServiceTokenFile`
  - `Resolve-PcvServiceAccountAclPrincipal`
  - `New-PcvTokenFileAclCommand`
  - `Invoke-PcvTokenFileAclApply`
  - service config의 `service_account`
- `Invoke-PcvDesktopService.ps1`
  - `-Action PrepareTokenFile`
  - `-ServiceAccount`
  - `-TokenValue`
  - `-TokenByteLength`
  - `-AdminPrincipal`
  - `-Force`
- `tests/PcvDesktopService.Contract.Tests.ps1`
  - token file 생성 helper, overwrite 정책, ACL command builder, service account install command 검증

## 포함 범위

Phase 8에 포함한다.

- 기본 token file 위치를 `%ProgramData%\PureCVisor\desktop-node\api-token.txt`로 정한다.
- token file 생성 helper를 추가한다.
- token 값은 기본 32바이트 난수에서 base64url 문자열로 생성한다.
- token file 생성 결과 JSON에는 token 값을 직접 출력하지 않는다.
- token file이 이미 있으면 기본값에서 덮어쓰지 않고, 명시적 `-Force`가 있을 때만 교체한다.
- token file ACL command builder를 추가한다.
- 기본 ACL reader는 `BUILTIN\Administrators`와 서비스 계정이다.
- 기본 서비스 계정 정책은 `LocalSystem`으로 명시한다.
- `LocalSystem`의 token file ACL principal은 `NT AUTHORITY\SYSTEM`으로 매핑한다.
- service `sc.exe create` command builder가 service account를 명시할 수 있게 한다.
- `Invoke-PcvDesktopService.ps1 -Action PrepareTokenFile`을 추가해 token file 생성과 ACL 적용을 한 entrypoint에서 실행한다.
- 기본 Pester suite는 token generation, ACL command builder, injectable ACL runner, service account command builder를 검증한다.
- 관리자 권한 smoke는 문서화된 opt-in 절차로만 둔다.

## 제외 범위

Phase 8에서 제외한다.

- MSI/MSIX/winget installer
- tray app
- Windows Credential Manager 또는 DPAPI 저장소
- dedicated service account 생성 자동화
- 사용자 계정 비밀번호를 `sc.exe` command builder에 전달하는 기능
- Windows Event Log provider 등록 자동화
- 실제 Hyper-V 작업 자동 통합 테스트
- Linux `purecvisorsd`, libvirt/KVM, Single Edge UI/API 변경

## token file 정책

기본 token file 위치는 다음과 같다.

```text
%ProgramData%\PureCVisor\desktop-node\api-token.txt
```

이 위치는 machine-wide service data로 취급한다. 사용자가 `-ApiTokenFile`을 직접 지정하면 그 경로를 우선한다. helper는 부모 디렉터리를 만들고 token file을 UTF-8, no newline으로 기록한다.

token file 생성은 멱등성을 보수적으로 유지한다.

```text
file 없음 + no token value  -> 새 난수 token 생성 후 저장
file 없음 + token value     -> 전달 token 저장
file 있음 + no -Force       -> PCV_SERVICE_TOKEN_FILE_EXISTS
file 있음 + -Force          -> 새 token으로 교체
blank token value            -> PCV_SERVICE_TOKEN_EMPTY
```

생성 결과는 `path`, `token_length`, `service_account`, `acl` 같은 메타데이터만 출력한다. token 값은 stdout JSON에 포함하지 않는다.

## ACL 정책

ACL 적용은 `icacls.exe` command builder와 injectable runner로 구현한다.

기본 명령 흐름:

```text
icacls.exe <token-file> /inheritance:r
icacls.exe <token-file> /grant:r BUILTIN\Administrators:R NT AUTHORITY\SYSTEM:R
```

서비스 계정이 명시되면 두 번째 reader는 해당 계정으로 바뀐다. `LocalSystem`은 `NT AUTHORITY\SYSTEM`으로 매핑하고, `LocalService`와 `NetworkService`도 Windows well-known principal로 정규화한다.

기본 suite는 명령 배열과 runner 실패 처리를 검증한다. 실제 ACL 결과가 관리자와 서비스 계정만 읽을 수 있는지는 elevated PowerShell smoke에서 확인한다.

## 서비스 계정 정책

Phase 8의 기본 서비스 계정은 `LocalSystem`이다.

이유:

- Phase 6/7은 이미 실제 서비스 설치를 관리자 opt-in으로 분리했다.
- Hyper-V 관리 작업은 일반 사용자 계정보다 높은 로컬 권한이 필요하다.
- 별도 계정 생성, 비밀번호 저장, Log on as a service 권한 부여는 MSI/MSIX installer 단계에서 다뤄야 한다.

사용자가 `-ServiceAccount`를 지정하면 service config와 `sc.exe create` command에 `obj=`로 반영한다. 다만 Phase 8은 계정 생성이나 비밀번호 전달을 자동화하지 않는다. 일반 사용자 또는 도메인 계정을 쓰려면 사전에 다음을 수동으로 보장해야 한다.

- 서비스 로그온 권한
- Hyper-V 관리 권한 또는 필요한 로컬 관리자 권한
- token file 읽기 권한
- Local API job store, event log path, web root 접근 권한

## Windows Event Log provider 판단

Phase 5의 JSONL event log는 listener와 firewall 이벤트를 이미 남긴다. Phase 8에서는 Windows Event Log provider 등록을 자동화하지 않는다.

provider 등록은 관리자 권한, provider 이름, source lifecycle, 삭제 정책을 installer가 소유해야 한다. 현재 spike의 service helper가 provider 등록까지 맡으면 service packaging 경계가 넓어진다. 따라서 Phase 8은 JSONL event log를 유지하고, Event Log provider는 제품 installer 또는 제품 런타임 승격 단계에서 다시 결정한다.

## 관리자 권한 smoke 경계

기본 검증은 관리자 권한 없이 통과해야 한다. 다음은 elevated PowerShell에서만 수동으로 실행한다.

- token file 생성 후 ACL 실제 확인
- `sc.exe create/start/query/stop/delete`
- `-EnsureFirewallRule` 실제 적용
- LAN prefix listener smoke
- process list와 service config에 token 값이 직접 남지 않는지 확인

## 완료 기준

Phase 8은 다음을 만족하면 완료다.

- service helper가 token file을 만들 수 있다.
- token file helper가 token 값을 stdout JSON에 출력하지 않는다.
- token file ACL command builder가 관리자와 서비스 계정 reader만 부여한다.
- service install command가 기본 서비스 계정을 명시한다.
- service binary path에는 token 값이 아니라 `-ApiTokenFile` 경로만 남는다.
- 기본 Pester suite가 관리자 권한 없이 통과한다.
- 관리자 권한 smoke 절차가 API README, service README, 검증 정책에 분리돼 있다.
