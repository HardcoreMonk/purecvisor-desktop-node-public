# PureCVisor Desktop Node Phase 5 LAN mode hardening 설계

## 목적

Phase 5는 Desktop Node Local API를 기본 loopback-only 개발 모드에서 벗어나, 사용자가 명시적으로 선택한 경우에만 LAN에서 접근 가능한 관리 API로 열 수 있게 한다.

목표는 LAN 접근을 편하게 켜는 것이 아니라, 노출 확대가 코드와 문서 모두에서 눈에 보이도록 만드는 것이다. 따라서 non-loopback prefix는 기본적으로 계속 거부하고, `-AllowLan`과 `-ApiToken`이 함께 있을 때만 허용한다.

## 현재 구현 상태

Phase 5 구현은 `spikes/purecvisor-desktop-node/api/`에 추가됐다.

- 기본 listener는 기존과 같이 loopback prefix만 허용한다.
- non-loopback prefix는 `-AllowLan`과 non-empty `-ApiToken`이 함께 있을 때만 허용한다.
- `-EventLogPath`를 지정하면 listener start/stop과 firewall ensure 결과를 JSONL 이벤트로 남긴다.
- `-EnsureFirewallRule`을 지정하면 Windows Firewall inbound TCP rule을 `netsh.exe` argv 기반으로 ensure한다.
- 방화벽 rule ensure는 기존 rule을 지운 뒤 같은 이름과 port로 다시 추가해 중복 rule 생성을 피한다.

## 포함 범위

Phase 5에 포함한다.

- `Assert-PcvApiPrefix` 기반 prefix 정책
- loopback 기본값 유지
- `-AllowLan` 기반 non-loopback opt-in
- LAN mode의 `-ApiToken` 필수화
- `-EventLogPath` JSONL audit/event log
- `-EnsureFirewallRule`, `-FirewallRuleName`, `-FirewallProfile`
- 방화벽 명령 builder와 injectable process runner 단위 테스트
- Local API README와 상위 문서의 Phase 5 상태 반영

## 제외 범위

Phase 5에서 제외한다.

- Windows 서비스 설치와 service account hardening
- 토큰 생성, 저장, Windows Credential Manager 연동
- TLS, mTLS, certificate lifecycle
- multi-user auth와 RBAC
- CORS/OPTIONS 공개
- Windows Event Log provider 등록
- CLI token persistence
- Web Console 원격 접속 UX 변경
- Linux Single Edge REST/API 공개 표면 변경

## 보안 정책

LAN mode는 다음 정책을 따른다.

```text
loopback prefix + no token      -> 허용, local development mode
loopback prefix + token         -> 허용, local authenticated mode
LAN prefix + no -AllowLan       -> 거부, PCV_PREFIX_NOT_LOOPBACK
LAN prefix + -AllowLan no token -> 거부, PCV_LAN_TOKEN_REQUIRED
LAN prefix + -AllowLan + token  -> 허용, explicit LAN authenticated mode
```

`-AllowLan`은 prefix 검증을 완화하는 opt-in이며 인증을 대체하지 않는다. LAN mode에서 `-ApiToken`은 필수이고, 모든 API route와 static file serving 전에 `Authorization: Bearer <token>` 검증을 통과해야 한다.

## 방화벽 정책

Windows Firewall rule 관리는 `-EnsureFirewallRule`을 지정한 경우에만 실행한다. 기본 listener start는 방화벽을 변경하지 않는다.

기본 rule은 다음 형태다.

```text
netsh.exe advfirewall firewall delete rule name=<rule> protocol=TCP localport=<port>
netsh.exe advfirewall firewall add rule name=<rule> dir=in action=allow protocol=TCP localport=<port> profile=private enable=yes
```

`-FirewallProfile`은 `private`, `domain`, `public`, `any` 중 하나를 받는다. 기본값은 홈랩 LAN 사용을 기준으로 `private`이다.

## 이벤트 로그 정책

`-EventLogPath`를 지정하면 JSONL 형식으로 기록한다.

```json
{"timestamp":"2026-04-25T00:00:00.0000000Z","event":"api.listener.start","data":{"prefix":"http://0.0.0.0:7777/","exposure":"lan","auth_required":true}}
```

현재 이벤트는 다음을 포함한다.

- `api.firewall.ensure`
- `api.listener.start`
- `api.listener.stop`

Windows Event Log provider 등록은 관리자 권한과 설치 절차가 필요하므로 Phase 5 범위에서는 제외하고, 후속 Windows service packaging 단계에서 다시 다룬다.

## 검증

Phase 5 기본 검증은 다음을 요구한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
```

현재 기대 결과:

- Local API: 79 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun

실제 firewall rule 적용은 관리자 권한이 필요하므로 기본 Pester suite에서는 injectable runner와 command builder만 검증한다.

## 완료 기준

Phase 5는 다음을 만족하면 완료다.

- non-loopback prefix가 기본값에서 계속 거부된다.
- LAN mode가 `-AllowLan`과 `-ApiToken` 없이는 열리지 않는다.
- `-EventLogPath`가 JSONL 이벤트를 기록한다.
- `-EnsureFirewallRule`이 idempotent하게 inbound TCP rule을 ensure한다.
- API/CLI/Web/Hyper-V non-integration 검증이 통과한다.
- 상위 문서와 spike README가 Phase 5 상태를 반영한다.
