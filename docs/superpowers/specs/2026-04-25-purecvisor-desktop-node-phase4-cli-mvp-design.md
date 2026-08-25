# PureCVisor Desktop Node Phase 4 CLI MVP 설계

## 목적

Phase 4는 Desktop Node spike에 명령행 진입점을 추가한다. 목표는 새 Hyper-V backend를 직접 만들거나 Local API를 우회하는 것이 아니라, Phase 2H/3B Local API 계약을 안정적인 PowerShell CLI로 감싸는 것이다.

CLI는 운영자가 브라우저를 열지 않고도 host 상태 확인, VM 조회/생성, lifecycle job 요청, checkpoint 작업, job 조회/제어를 수행할 수 있게 한다.

## 현재 구현 상태

Phase 4 구현은 `spikes/purecvisor-desktop-node/cli/`에 추가됐다.

- CLI module: `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`
- CLI entrypoint: `spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1`
- CLI tests: `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`
- 사용법: `spikes/purecvisor-desktop-node/cli/README.md`

Phase 4를 위해 Local API에 checkpoint route도 추가됐다.

- `GET /api/v1/vms/{id}/checkpoints`
- `POST /api/v1/vms/{id}/checkpoints`
- `POST /api/v1/vms/{id}/checkpoints/{checkpoint_id}/restore`
- `DELETE /api/v1/vms/{id}/checkpoints/{checkpoint_id}`

Checkpoint create/restore/delete는 VM create/lifecycle과 같은 queued job 패턴을 따른다. Checkpoint list만 read operation으로 helper를 즉시 호출한다.

## 포함 범위

Phase 4에 포함한다.

- `host status`
- `vm list`
- `vm get <vm>`
- `vm create`
- `vm start`
- `vm stop` / `vm shutdown`
- `vm poweroff`
- `vm restart`
- `vm checkpoint list`
- `vm checkpoint create`
- `vm checkpoint restore`
- `vm checkpoint delete`
- `job get`
- `job cancel`
- `job retry`
- `--json` 원본 API 응답 출력
- `--api` Local API base URL 지정
- `--token` bearer token 전달

## 제외 범위

Phase 4에서 제외한다.

- Hyper-V helper 직접 호출 CLI
- LAN mode / non-loopback API 호출 권장
- Windows 서비스 설치
- 토큰 생성, 저장, keychain 연동
- shell completion
- interactive prompt
- TUI
- VMConnect 실행
- checkpoint Web UI
- Linux Single Edge `pcvctl`과의 통합

## 아키텍처

CLI는 다음 경계를 따른다.

```text
Invoke-PcvDesktopCli.ps1
  -> PcvDesktopCli.psm1
  -> Local API HTTP route
  -> PcvDesktopApi.psm1
  -> Hyper-V helper runner
```

CLI는 Local API의 structured response body를 신뢰한다. `--json`이 지정되면 API 응답 shape를 유지해 compact JSON으로 출력한다. `--json`이 없으면 사람이 읽기 쉬운 최소 요약을 출력한다.

## API route 원칙

Phase 4에서 추가한 checkpoint route는 기존 job semantics와 맞춘다.

- checkpoint list는 read operation이므로 `GET`과 immediate helper response를 사용한다.
- checkpoint create/restore/delete는 Hyper-V 상태를 바꾸므로 queued job으로 만든다.
- checkpoint delete는 HTTP 의미와 CLI 직관을 위해 `DELETE` route를 사용하되, 실제 처리는 즉시 삭제가 아니라 queued `checkpoint.delete` job이다.
- route id와 checkpoint id는 percent-decoding 후 빈 값과 malformed escape를 구조화 오류로 거부한다.

## 검증

Phase 4 기본 검증은 다음을 요구한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
```

Phase 4 완료 당시 기대 결과:

- Local API: 72 passed, 0 failed
- CLI: 8 passed, 0 failed
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- Hyper-V helper integration: 42 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0

실제 Hyper-V VM 생성과 checkpoint 작업은 기존 gated integration suite로 분리한다.

## 완료 기준

Phase 4는 다음을 만족하면 완료다.

- CLI command parser가 Phase 4 명령을 route/method/body로 안정적으로 변환한다.
- CLI `--json`, `--api`, `--token` 옵션이 검증된다.
- Local API checkpoint route가 helper operation과 job queue에 연결된다.
- API/CLI/Hyper-V/Web 기본 검증이 통과한다.
- 상위 문서와 spike README가 Phase 4 상태를 반영한다.
