# PureCVisor Desktop Node Phase 14 후속 개발 진행 내역

## 저장 시점

- 작성 시각: 2026-04-28 16:10 KST
- 브랜치: `codex/phase14-installer`
- PR: `https://github.com/HardcoreMonk/purecvisor-single/pull/11`
- 최신 커밋: `9125784 docs: update Desktop Node phase 14 state`
- 직전 핵심 수정 커밋: `fd4f9a2 Fix MSI service custom actions`

## 현재 상태 요약

Phase 14 WiX MSI-first installer 후속 개발은 unsigned dev MSI build와 관리자 install/uninstall smoke까지 완료됐다. 이번 저장 시점 기준으로 active 문서, Phase 11-14 spec/plan, packaging README, Desktop Node spike README, `follower.md`, PR 본문은 같은 상태를 가리킨다.

현재 남은 git working tree 항목은 수동 검증 산출물뿐이다.

- `artifacts/`
- `install-fixed.log`
- `install.log`

위 항목은 MSI build와 관리자 smoke 과정의 산출물/로그이며 커밋하지 않는다.

## 완료된 후속 작업

1. MSI 설치 실패 root cause 수정
   - Deferred custom action source를 installed payload directory 기준으로 고정했다.
   - MSI payload에 runtime assets를 포함했다.
   - installed wrapper가 repo-style path에 의존하지 않도록 service module resolution을 보강했다.
   - Windows PowerShell 5.1 custom action 호환성을 보강했다.
   - LocalSystem에서 bare `pwsh.exe`가 해석되지 않는 환경에 대비해 absolute fallback을 추가했다.
   - MSI Restart Manager가 WinSW를 `STOP_PENDING`으로 만든 uninstall race를 WinSW status wait로 처리했다.

2. Phase 14 문서 현행화
   - `README.md`, `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `docs/GUIDE.md`를 갱신했다.
   - Phase 11-14 spec/plan을 현재 상태에 맞췄다.
   - `packaging/windows-desktop-node/README.md`와 `packaging/windows-desktop-node/installer/README.md`에 troubleshooting을 추가했다.
   - `spikes/purecvisor-desktop-node/README.md`, `api/README.md`, `service/README.md`를 Phase 14 packaging installer handoff 기준으로 갱신했다.
   - `follower.md`에 Phase 14 완료 상태와 다음 gate를 기록했다.

3. PR 본문 갱신
   - Documentation 섹션을 추가했다.
   - Remaining Manual Gates를 repair, `REMOVE_DATA=1`, signed release build, WinSW release-signature verification으로 정리했다.

## 검증 결과

다음 검증을 완료했다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

결과:

- Desktop Node root boundary suite: 8 passed, 0 failed
- Installer suite: 21 passed, 0 failed
- Packaging suite: 56 passed, 0 failed
- Service suite: 14 passed, 0 failed
- Web JS syntax check: exit 0
- `git diff --check`: exit 0

관리자 smoke 결과:

- `msiexec /i artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi`: exit 0
- 설치 후 `PureCVisorDesktopNode` service: Running
- token 포함 `GET http://127.0.0.1:7777/api/v1/runtime/policy`: HTTP 200
- `msiexec /x artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi`: exit 0
- 제거 후 `PureCVisorDesktopNode` service: removed

최종 unsigned dev MSI:

- 경로: `artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi`
- SHA-256: `7d2ad13f831598717a2988f62020a117c1e2a0b81e37546849d819e1e2ad5043`
- provenance git commit: `fd4f9a28f7e326214b94466b7775622265fa910a`

## 남은 작업

1. Repair smoke
   - 관리자 PowerShell에서 `msiexec /i artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx repair.log` 실행.
   - token/job/event/diagnostics 보존과 service 재구성을 확인한다.

2. `REMOVE_DATA=1` uninstall smoke
   - MSI 재설치 후 `msiexec /x artifacts/windows-desktop-node/PureCVisorDesktopNode-0.14.0-dev.msi REMOVE_DATA=1 /qn /norestart /l*vx uninstall-remove-data.log` 실행.
   - token, job store, event log, install log, diagnostics 제거와 service/listener 부재를 확인한다.

3. Signed release build
- signing secret, `signtool.exe`, certificate input, timestamp URL, explicit `-SigningTrustModel`이 있는 환경에서 `-SigningMode RequireSigned` build를 실행한다.
   - real WinSW source artifact의 release signature/provenance를 확인한다.

4. Phase 15 spec/plan
   - Phase 14 남은 release-candidate gate가 정리되면 DPAPI 또는 Windows Credential Manager 기반 secure token storage spec/plan으로 이동한다.

## 재개 시 첫 확인

1. `git status --short`로 untracked 산출물과 문서 변경 여부를 확인한다.
2. PR #11 head가 `9125784` 이후인지 확인한다.
3. repair와 `REMOVE_DATA=1` smoke를 실행할지, signed release build 입력이 준비됐는지 먼저 결정한다.
4. 새 검증 결과는 `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase14-signed-installer-repair-ux.md`의 `완료 증거`와 이 문서에 함께 반영한다.
