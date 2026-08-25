# PureCVisor Desktop Node Phase 6 Windows service packaging 구현 계획

## 목표

Desktop Node Local API listener를 Windows 서비스로 등록할 수 있는 packaging spike를 추가한다. 기본 검증은 실제 서비스 설치를 수행하지 않고, `sc.exe` command builder와 injectable process runner 계약을 검증한다.

## 구현 범위

- `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`
  - service config 생성
  - Local API listener binary path 생성
  - LAN service mode token 필수화
  - `sc.exe` command builder
  - injectable process runner
- `spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1`
  - `Config`, `Install`, `Uninstall`, `Start`, `Stop`, `Restart`, `Status`
  - `-WhatIf` preview
- `spikes/purecvisor-desktop-node/service/tests/PcvDesktopService.Contract.Tests.ps1`
  - service binary path, LAN token policy, command builder, runner failure handling 검증
- 문서
  - Phase 6 설계 문서
  - service README
  - 상위 문서 진입점과 검증 정책

## 완료 상태

- [x] Phase 6 범위 확인
- [x] worktree `phase6-service-packaging` 생성
- [x] API baseline 검증
- [x] service packaging red test 작성
- [x] service module/entrypoint 구현
- [x] focused service test green 확인
- [x] 전체 Desktop Node 검증 실행
- [x] 문서 최종 수치 확인
- [x] 커밋, 푸시, PR 생성/병합

## 검증 명령

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
git diff --check
```

## 완료 증거

최종 검증 후 이 절을 최신 수치로 유지한다.

- Service packaging: 6 passed, 0 failed
- Local API: 79 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- service entrypoint/module parser: exit 0
- service `Config`: exit 0
- service `Install -WhatIf`: exit 0
- `git diff --check`: exit 0
- 병합 근거: PR #5, merge commit `75ac5d8`
