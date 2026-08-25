# PureCVisor Desktop Node Phase 12 Service-first 제품 런타임 설계

## 목적

Phase 12는 Windows Desktop Node Web Console을 최종 제품판 배포로 가져가기 위한 첫 제품 런타임 단계다.

Phase 11은 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`로 Desktop Node 전체를 `spikes/purecvisor-desktop-node/**` 격리 spike에 남겼다. 그 이유 중 제품 배포 관점에서 가장 먼저 해소해야 하는 항목은 설치, 업데이트, 롤백 정책과 로그 수집, 서비스 복구 계약이었다.

Phase 12는 MSI/MSIX 또는 signed installer까지 한 번에 구현하지 않는다. 대신 현재 동작하는 Local API, Web Console, Hyper-V helper, service packaging 자산을 제품 설치 경로로 승격해 Windows 서비스로 실행할 수 있는 Service-first 제품 런타임을 만든다.

## 결정

```text
DESKTOP_NODE_PHASE12_RUNTIME_DECISION: service-first-product-wrapper
```

Phase 12는 Promotion wrapper 접근을 채택한다. 기존 `spikes/purecvisor-desktop-node/{api,web,hyperv,service}` 코드는 즉시 이동하지 않고, 새 제품 배포 계층이 필요한 자산을 제품 경로로 복사하고 Windows service를 구성한다.

이 결정의 직접 효과:

- Desktop Node Web Console은 제품 설치 경로에서 Local API service가 same-origin으로 제공한다.
- Local API listener는 Windows service로 설치, 시작, 중지, 삭제할 수 있어야 한다.
- 제품 install/update/uninstall/rollback은 dry-run plan과 실제 실행 entrypoint를 가진다.
- `%ProgramData%\PureCVisor\desktop-node` 아래 token, job store, event log, install log, diagnostics 경로를 제품 계약으로 둔다.
- Phase 11 `keep-spike`는 Desktop Node 전체 GA 보류 결정으로 남기되, Phase 12에서 Service-first runtime 배포 단위는 제품 후보로 승격을 시작한다.

## 사용자 승인 범위

이번 설계는 2026-04-26 대화에서 승인된 다음 범위를 반영한다.

- Service-first 제품 MVP를 첫 목표로 한다.
- Phase 11 판단 근거 중 설치, 업데이트, 롤백 정책과 로그 수집, 서비스 복구 항목을 Phase 12의 필수 반영 대상으로 둔다.
- Promotion wrapper 접근을 채택한다.
- signed installer, full updater, MSI rollback, DPAPI, Windows Event Log provider는 Phase 12 당시 후속 Phase로 남겼다. 현재는 Phase 14-18에서 WiX MSI-first installer, DPAPI protected token, JSONL first diagnostics, manifest-first update/rollback을 추가했고, 2026-04-30 local test certificate 기준 signed RC MSI lifecycle과 Event Log source lifecycle evidence를 기록했다. Full updater와 public trusted/stable signing은 계속 후속 판단이다.

## 아키텍처

새 제품 배포 계층은 다음 경로를 사용한다.

| 경로 | 역할 |
|------|------|
| `packaging/windows-desktop-node/` | Service-first product wrapper, product manifest, install/update/uninstall/rollback/diagnostics entrypoint |
| `C:\Program Files\PureCVisor\DesktopNode` | 기본 제품 설치 루트 |
| `%ProgramData%\PureCVisor\desktop-node` | 기본 제품 데이터 루트 |
| `%ProgramData%\PureCVisor\desktop-node\api-token.txt` | 기본 API bearer token file |
| `%ProgramData%\PureCVisor\desktop-node\jobs.json` | 기본 persisted job store |
| `%ProgramData%\PureCVisor\desktop-node\events.jsonl` | 기본 API event log |
| `%ProgramData%\PureCVisor\desktop-node\install.jsonl` | 기본 install/update/uninstall log |
| `%ProgramData%\PureCVisor\desktop-node\diagnostics\` | diagnostic bundle 출력 위치 |

제품 wrapper는 다음 자산을 제품 루트에 배치한다.

- `api/`: Local API listener module과 launcher
- `web/`: static Web Console 자산
- `hyperv/`: Hyper-V helper
- `service/`: service config와 command builder가 필요한 경우의 support module
- product manifest: 설치된 파일, source revision, product version, schema version, runtime paths

기본 service 설정:

- service name: `PureCVisorDesktopNode`
- display name: `PureCVisor Desktop Node`
- prefix: `http://127.0.0.1:7777/`
- web root: 제품 루트의 `web\`
- job store: `%ProgramData%\PureCVisor\desktop-node\jobs.json`
- event log: `%ProgramData%\PureCVisor\desktop-node\events.jsonl`
- auth: `-ApiTokenFile` 필수
- service account: 기존 service packaging 기본값인 `LocalSystem`

## Command surface

Phase 12 제품 wrapper는 하나의 PowerShell entrypoint를 제공한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install
```

지원 action:

| Action | 의미 |
|--------|------|
| `Plan` | install/update/uninstall/rollback 전에 실행할 경로, 파일, service command, data retention 결정을 JSON으로 출력 |
| `Install` | 제품 루트와 데이터 루트 생성, 자산 복사, token 준비, service 등록, service start, health check |
| `Update` | 기존 service stop, 새 자산 staging, service binary path 갱신, service start, health check |
| `Rollback` | 실패한 update 이후 이전 제품 루트 복원, service binary path 복원, service start, health check |
| `Uninstall` | service stop/delete, 제품 루트 제거, 기본 데이터 보존 |
| `Status` | service 상태, runtime policy health, 제품 manifest 요약 |
| `CollectDiagnostics` | redaction된 diagnostic bundle 생성 |

모든 mutating action은 `-WhatIf` 또는 `-DryRun`을 지원해야 한다. 기본 검증은 실제 system mutation 없이 plan과 command builder를 검증한다.

## Install 계약

Install은 다음 순서를 따른다.

1. 제품 루트 `C:\Program Files\PureCVisor\DesktopNode`를 만든다.
2. 데이터 루트 `%ProgramData%\PureCVisor\desktop-node`를 만든다.
3. `api/`, `web/`, `hyperv/`, 필요한 `service/` support 자산을 제품 루트로 복사한다.
4. product manifest를 작성한다.
5. token file `%ProgramData%\PureCVisor\desktop-node\api-token.txt`를 준비한다.
6. token file ACL은 Administrators와 service account read-only 기준으로 적용한다.
7. service binary path가 제품 루트의 `api\Invoke-PcvDesktopApi.ps1`와 제품 루트의 `web\`을 참조하도록 구성한다.
8. `sc.exe create`, `sc.exe description`, `sc.exe failure` 명령을 적용한다.
9. service를 시작한다.
10. `GET /api/v1/runtime/policy` 또는 static root 응답으로 health check를 수행한다.

Install은 장기 실행 service command line에 inline token 값을 남기면 안 된다. 제품 install 경로는 `-ApiTokenFile`만 허용한다.

## Update 계약

Update는 기존 설치 감지와 service stop/start 순서를 제품 계약으로 둔다.

1. 현재 설치 manifest를 읽는다.
2. service가 존재하면 stop을 시도한다.
3. 현재 제품 루트를 backup 또는 previous 경로로 보존한다.
4. 새 제품 자산을 staging 경로에 먼저 복사한다.
5. staging manifest를 검증한다.
6. `%ProgramData%`의 token, job store, event log, install log는 유지한다.
7. service binary path를 새 제품 루트로 갱신한다.
8. service를 시작하고 health check를 수행한다.
9. 성공하면 backup 보존 정책에 따라 이전 제품 루트를 유지하거나 정리한다.

Phase 12의 update는 best-effort update다. signed installer와 transactional MSI rollback 수준의 보장은 후속 Phase에서 다룬다.

## Rollback 계약

Rollback은 update 실패 시 실행 가능한 명령 경로를 제공한다.

1. service stop을 시도한다.
2. 실패한 staging 또는 partially installed product root를 격리하거나 제거한다.
3. previous product root를 복원한다.
4. service binary path를 previous product root로 되돌린다.
5. service를 시작한다.
6. health check 결과와 실패 원인을 install log와 diagnostic bundle에 남긴다.

Rollback이 실패해도 token file, job store, event log는 기본적으로 삭제하지 않는다. 데이터 삭제는 사용자가 명시적으로 요청한 uninstall data removal에서만 수행한다.

## Uninstall 계약

Uninstall은 제품 root 제거와 데이터 보존을 분리한다.

- 기본 uninstall:
  - service stop
  - service delete
  - 제품 설치 루트 제거
  - `%ProgramData%` 데이터 보존
- `-RemoveData` 명시 uninstall:
  - 기본 uninstall 절차 이후 token, job store, event log, install log, diagnostics 제거

`-RemoveData`는 destructive action으로 간주하고 dry-run plan에 삭제 대상 절대 경로를 명확히 표시해야 한다.

## 로그 수집과 diagnostic bundle

Phase 12는 Windows Event Log provider를 만들지 않는다. 현재 구현과 일관되게 JSONL 로그와 diagnostic bundle을 제품 계약으로 올린다.

기본 로그:

- API event log: `%ProgramData%\PureCVisor\desktop-node\events.jsonl`
- install/update/uninstall log: `%ProgramData%\PureCVisor\desktop-node\install.jsonl`

Diagnostic bundle 포함 항목:

- product manifest
- service status
- runtime policy 응답
- 최근 install/update/uninstall log
- 최근 API event log
- job store 요약
- web asset manifest
- product file manifest

Diagnostic bundle 제외 또는 redaction 항목:

- token 값
- Authorization header
- API token file 내용
- 임의 사용자 경로 전문
- Hyper-V VM 내부 파일 또는 디스크 내용

## 서비스 복구

Phase 12는 기존 service packaging의 `sc.exe failure` command builder를 제품 계약으로 유지한다.

기본 recovery:

- 첫 번째 실패: 60초 후 restart
- 두 번째 실패: 60초 후 restart
- 이후 실패: 추가 action 없음
- reset window: 86400초

service recovery 이후 job store는 JSON file persistence 기준으로 유지한다. running helper interruption이나 running job 강제 복구는 Phase 12 범위에 넣지 않는다. 재시작 후 상태 일관성은 runtime policy, job list, event log, diagnostic bundle로 관측한다.

## 보안 기본값

Phase 12 제품 기본값:

- listener는 loopback only다.
- LAN mode는 기본 install에서 제외한다.
- 장기 실행 service는 `-ApiTokenFile`만 사용한다.
- inline `-ApiToken`은 service product install에서 금지한다.
- token file ACL은 Administrators와 service account read-only 기준이다.
- diagnostic bundle은 token과 Authorization 정보를 redaction한다.

LAN mode는 Phase 12에서 별도 opt-in preview만 허용한다. LAN mode 제품화, TLS, reverse proxy, DPAPI 또는 Windows Credential Manager, token rotation/revoke, multi-user auth는 후속 Phase에서 결정한다.

## 검증

기본 검증은 관리자 권한 없이 수행한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

새 packaging suite는 다음을 검증한다.

- product path와 data path 기본값
- product manifest 생성 계약
- install/update/uninstall/rollback dry-run plan
- service binary path가 제품 루트를 참조하는지
- Web Console asset manifest 포함 여부
- `-ApiTokenFile` 필수 계약
- inline token 금지
- diagnostic bundle redaction
- `-RemoveData` 삭제 대상 preview

관리자 opt-in smoke 후보:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Install
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Status
Invoke-WebRequest http://127.0.0.1:7777/api/v1/runtime/policy
Invoke-WebRequest http://127.0.0.1:7777/
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Uninstall
```

실제 Hyper-V VM create/start/poweroff/checkpoint lifecycle은 관리자 권한 integration gate로 별도 유지한다.

## 완료 기준

Phase 12는 다음을 만족하면 완료다.

- `packaging/windows-desktop-node/` 제품 wrapper가 존재한다.
- install/update/uninstall/rollback/status/diagnostics action이 dry-run으로 검증된다.
- 제품 설치 경로와 데이터 경로가 manifest에 남는다.
- Web Console은 제품 설치 경로에서 Local API service가 same-origin으로 제공한다.
- service command line은 제품 루트의 API script와 Web root를 참조한다.
- 장기 token 값은 command line에 남지 않고 token file 경로만 전달된다.
- `%ProgramData%` 데이터 보존과 `-RemoveData` 삭제 정책이 명시된다.
- install/update log와 diagnostic bundle 계약이 존재한다.
- service failure action이 product install plan과 service command에 포함된다.
- 관리자 권한 smoke 절차가 문서화된다.
- Phase 11 `keep-spike` 문서는 Phase 12 service-first runtime 승격 시작 상태와 충돌하지 않도록 갱신된다.

## 제외 범위

Phase 12에서 제외한다.

- signed release MSI/MSIX/winget installer
- code signing chain 운용
- full transactional rollback
- automatic updater
- DPAPI 또는 Windows Credential Manager token storage
- token rotation/revoke
- TLS 또는 reverse proxy 제품 정책
- multi-user auth/RBAC
- Windows Event Log provider/source lifecycle
- crash dump 수집
- VMConnect launch policy
- Single Edge `purecvisorsd` 런타임 변경
- Linux Single Edge Web UI 변경

## 후속 Phase 후보

- Phase 13: WinSW service wrapper로 Windows SCM service host 차단점 해소
- Phase 14: WiX MSI-first installer와 uninstall/repair flow
- Phase 15: DPAPI 또는 Windows Credential Manager token storage
- Phase 16: Windows Event Log provider, log rotation, diagnostic bundle 고도화
- Phase 17: LAN mode 제품 보안 정책과 TLS/reverse proxy 전제
- 별도 후속: update/rollback/config migration 고도화
