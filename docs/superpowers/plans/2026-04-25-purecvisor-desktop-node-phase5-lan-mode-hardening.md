# PureCVisor Desktop Node Phase 5 LAN mode hardening 구현 계획

## 목표

Desktop Node Local API에 명시적 LAN mode를 추가한다. 기본값은 기존처럼 loopback-only로 유지하고, LAN 접근은 `-AllowLan`과 `-ApiToken`이 함께 있을 때만 허용한다.

## 구현 범위

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - `Assert-PcvApiPrefix`와 prefix exposure 판정
  - LAN mode token 필수화
  - JSONL API event log writer
  - Windows Firewall rule command builder와 ensure helper
  - `Start-PcvDesktopApi`의 `-AllowLan`, `-EventLogPath`, `-EnsureFirewallRule` 옵션
- `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`
  - Phase 5 listener/firewall/event log 옵션 전달
- `spikes/purecvisor-desktop-node/api/tests/PcvDesktopApi.Lan.Tests.ps1`
  - LAN opt-in, token 필수, event log, firewall command contract 검증
- 문서
  - Phase 5 설계 문서
  - API README
  - 상위 문서 진입점과 검증 정책
  - 공개 릴리스 경계 문서

## 완료 상태

- [x] Phase 5 범위 확인
- [x] worktree `phase5-lan-mode-hardening` 생성
- [x] API/CLI baseline 검증
- [x] LAN hardening red test 작성
- [x] prefix policy, event log, firewall helper 구현
- [x] focused LAN/API test green 확인
- [x] 전체 Desktop Node 검증 실행
- [x] 문서 최종 수치 확인
- [x] 커밋, 푸시, PR 생성/병합

## 검증 명령

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

## 완료 증거

최종 검증 후 이 절을 최신 수치로 유지한다.

- Local API: 79 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0
- CLI entrypoint help: exit 0
- API entrypoint/module parser: exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- `git diff --check`: exit 0
- 병합 근거: PR #4, merge commit `c9fb188`
