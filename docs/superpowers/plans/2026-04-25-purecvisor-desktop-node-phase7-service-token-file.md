# PureCVisor Desktop Node Phase 7 service token file hardening 구현 계획

## 목표

Desktop Node Local API와 Windows service packaging에 `-ApiTokenFile` 계약을 추가해, 장기 실행 서비스의 binary path에 bearer token 값이 직접 남는 운영 리스크를 줄인다.

## 구현 범위

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - `Resolve-PcvApiToken` 추가
  - `-ApiToken` / `-ApiTokenFile` 동시 지정 거부
  - missing/empty token file 거부
  - `Start-PcvDesktopApi`가 startup 시 token file을 해석하고 요청 처리에는 해석된 token 값을 전달
- `spikes/purecvisor-desktop-node/api/Invoke-PcvDesktopApi.ps1`
  - `-ApiTokenFile` 파라미터 전달
- `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`
  - service token source 판정
  - LAN service mode token source 필수 조건 유지
  - service binary path에 `-ApiTokenFile` 전달
  - inline token/token file 동시 지정 거부
- `spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1`
  - `-ApiTokenFile` 파라미터 전달
- 문서
  - Phase 7 설계 문서
  - API/service README
  - 상위 문서 진입점과 검증 정책

## 완료 상태

- [x] Phase 7 범위 확인
- [x] worktree `phase7-service-token-file` 생성
- [x] API/service baseline 검증
- [x] API token file red test 작성
- [x] service token file red test 작성
- [x] API token resolver와 entrypoint 구현
- [x] service config와 entrypoint 구현
- [x] focused API/service test green 확인
- [x] 전체 Desktop Node 검증 실행
- [x] 문서 최종 수치 확인
- [x] 커밋, 푸시, PR 생성/병합

## 검증 명령

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
pwsh -NoProfile -Command "$ErrorActionPreference = 'Stop'; $p = Join-Path $env:TEMP 'pcv-token.txt'; Set-Content -LiteralPath $p -Value 'file-secret' -Encoding UTF8 -NoNewline; $module = (Resolve-Path 'spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1').Path; Import-Module $module -Force; Resolve-PcvApiToken -ApiTokenFile $p | ConvertTo-Json -Compress; Remove-Item -LiteralPath $p -Force"
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Config -ApiTokenFile '<token-file>'
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Install -WhatIf -ApiTokenFile '<token-file>'
git diff --check
```

## 완료 증거

최종 검증 후 이 절을 최신 수치로 유지한다.

- Local API: 82 passed, 0 failed
- Service packaging: 8 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- API entrypoint/module parser: exit 0
- service entrypoint/module parser: exit 0
- API token file resolver smoke: exit 0, `source=file`
- service `Config -ApiTokenFile`: exit 0, `api_token_source=file`
- service `Install -WhatIf -ApiTokenFile`: exit 0
- `git diff --check`: exit 0
- 병합 근거: PR #6, merge commit `43fd3c4`
