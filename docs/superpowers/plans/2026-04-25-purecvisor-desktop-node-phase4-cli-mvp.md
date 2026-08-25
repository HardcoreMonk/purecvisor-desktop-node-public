# PureCVisor Desktop Node Phase 4 CLI MVP 구현 계획

## 목표

Desktop Node spike에 Local API 기반 CLI MVP를 추가한다. Phase 4는 사용자가 PowerShell에서 host, VM, lifecycle, checkpoint, job 작업을 수행할 수 있게 하는 얇은 명령행 계층이다.

## 구현 범위

- `spikes/purecvisor-desktop-node/api/PcvDesktopApi.psm1`
  - checkpoint list/create/restore/delete route 추가
  - checkpoint mutation은 queued job으로 저장
  - worker가 기존 Hyper-V helper allowlist의 checkpoint operation을 실행
- `spikes/purecvisor-desktop-node/cli/PcvDesktopCli.psm1`
  - CLI argument parser
  - Local API HTTP transport
  - `--json`, `--api`, `--token`
- `spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1`
  - script entrypoint
- `spikes/purecvisor-desktop-node/cli/tests/PcvDesktopCli.Contract.Tests.ps1`
  - route/method/body 변환 계약 검증
- 문서
  - Phase 4 설계 문서
  - CLI README
  - API README endpoint와 검증 수치
  - 상위 문서 진입점과 검증 정책

## 완료 상태

- [x] Phase 4 범위 확인
- [x] worktree `phase4-cli-mvp` 생성
- [x] API checkpoint route red test 작성
- [x] CLI contract red test 작성
- [x] Local API checkpoint route 구현
- [x] CLI module/entrypoint 구현
- [x] focused API/CLI test green 확인
- [x] 전체 Desktop Node 검증 실행
- [x] 문서 최종 수치 확인
- [x] 커밋, 푸시, main 병합

## 검증 명령

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
PCV_HYPERV_INTEGRATION=1 PCV_HYPERV_TEST_ISO=<iso> pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/cli/Invoke-PcvDesktopCli.ps1 --help
git diff --check
```

## 완료 증거

최종 검증 후 이 절을 최신 수치로 유지한다.

- Local API: 72 passed, 0 failed
- CLI: 8 passed, 0 failed
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- Hyper-V helper integration: 42 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0
- CLI entrypoint help: exit 0
- `git diff --check`: exit 0
- 병합 근거: `d6a178f` (`feat: add Desktop Node CLI MVP`)로 main 병합 완료
