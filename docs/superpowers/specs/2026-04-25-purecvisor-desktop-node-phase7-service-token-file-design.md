# PureCVisor Desktop Node Phase 7 service token file hardening 설계

## 목적

Phase 7은 Desktop Node Local API를 Windows 서비스로 장시간 실행할 때 bearer token 값이 서비스 command line에 직접 남는 문제를 줄인다.

Phase 6에서는 `sc.exe` service binary path에 `-ApiToken "<token>"`을 넣을 수 있었다. 이 방식은 빠른 수동 smoke에는 단순하지만, Windows 서비스 설정과 프로세스 목록에 장기 token 값이 노출될 수 있다. Phase 7은 token 값을 파일에 두고 listener와 service packaging이 `-ApiTokenFile <path>`를 전달하는 계약을 추가한다.

## 현재 구현 상태

Phase 7 구현은 기존 Local API와 service packaging spike에 추가됐다.

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - `Resolve-PcvApiToken`
  - `-ApiToken` / `-ApiTokenFile` conflict 검증
  - token file missing/empty 검증
- `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`
  - `-ApiTokenFile` entrypoint 파라미터
- `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`
  - service config의 token source 판정
  - service binary path의 `-ApiTokenFile` 전달
  - inline token/token file conflict 검증
- `spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1`
  - `-ApiTokenFile` entrypoint 파라미터

## 포함 범위

Phase 7에 포함한다.

- Local API listener의 `-ApiTokenFile` 지원
- token file에서 읽은 값의 trailing newline trim
- missing token file 거부
- empty token file 거부
- `-ApiToken`과 `-ApiTokenFile` 동시 지정 거부
- LAN mode에서 `-AllowLan` + non-empty token source 필수 유지
- service config가 `api_token_source = file`을 표시
- service binary path가 token 값 대신 `-ApiTokenFile "<path>"`를 포함
- API/service README와 상위 문서 인덱스 업데이트

## 제외 범위

Phase 7에서 제외한다.

- token 생성기
- token file ACL 자동 적용
- Windows Credential Manager 통합
- DPAPI 암호화 저장
- dedicated service account 생성/권한 부여
- 실제 `sc.exe` install/start/stop/delete 통합 자동화
- Linux Single Edge runtime 또는 systemd 변경

token file ACL은 관리자 권한, 서비스 계정, 설치 위치 정책이 함께 결정되어야 하므로 후속 installer hardening 단계에서 다룬다.

## 보안 정책

token source는 하나만 허용한다.

```text
no token source                  -> loopback 허용, LAN 거부
-ApiToken                        -> loopback/LAN auth source inline
-ApiTokenFile                    -> loopback/LAN auth source file
-ApiToken + -ApiTokenFile        -> 거부
missing -ApiTokenFile path       -> 거부
empty -ApiTokenFile content      -> 거부
```

서비스 설치 예시는 `-ApiTokenFile`을 우선한다. `-ApiToken`은 짧은 수동 smoke나 기존 Phase 6 호환 경로로만 남긴다.

## 검증

Phase 7 기본 검증은 다음을 요구한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

현재 기대 결과:

- Local API: 82 passed, 0 failed
- Service packaging: 8 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- Web JavaScript syntax: exit 0

## 완료 기준

Phase 7은 다음을 만족하면 완료다.

- Local API가 token file 값을 읽어 bearer auth에 사용한다.
- 빈 token file과 누락 token file을 구조화된 코드로 거부한다.
- inline token과 token file의 동시 지정이 API/service 양쪽에서 거부된다.
- LAN service mode가 token source 없이 열리지 않는다.
- service binary path가 token 값을 직접 포함하지 않고 token file 경로만 전달한다.
- 상위 문서와 spike README가 Phase 7 상태를 반영한다.
