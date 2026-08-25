# PureCVisor Desktop Node Phase 8 installer hardening 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Desktop Node service packaging에 token file 생성, ACL 적용 helper, 명시적 서비스 계정 정책, 관리자 권한 smoke 문서를 추가한다.

**Architecture:** Phase 8은 기존 `spikes/purecvisor-desktop-node/service/` module과 entrypoint를 확장한다. 실제 Windows service, firewall, ACL 변경은 opt-in 실행으로만 두고, 기본 테스트는 command builder와 injectable runner를 검증한다.

**Tech Stack:** PowerShell 7, Pester 5, `sc.exe` command builder, `icacls.exe` command builder, Markdown 문서.

---

## 파일 구조

- Modify: `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`
  - token generation, default token path, ACL principal normalization, ACL command builder, ACL runner, token file preparation, service account config를 담당한다.
- Modify: `spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1`
  - `PrepareTokenFile` action과 service account/token file 관련 entrypoint 파라미터를 노출한다.
- Modify: `spikes/purecvisor-desktop-node/service/tests/PcvDesktopService.Contract.Tests.ps1`
  - Phase 8 contract를 red-green으로 검증한다.
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
  - Local API token file 준비와 관리자 smoke 경계를 설명한다.
- Modify: `spikes/purecvisor-desktop-node/service/README.md`
  - service helper 사용법, token file ACL, service account, smoke 절차를 설명한다.
- Modify: `docs/DEVELOPER_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/PUBLIC_RELEASE_BOUNDARY.md`, `AGENTS.md`
  - Phase 8 진입점과 검증 경계를 반영한다.

## 완료 상태

- [x] Phase 8 범위 확인
- [x] service baseline 검증
- [x] token file / ACL / service account red test 작성
- [x] red test 실패 확인
- [x] service module 구현
- [x] service entrypoint 구현
- [x] focused service test green 확인
- [x] API/service README와 상위 문서 갱신
- [x] 전체 Desktop Node 기본 검증 실행
- [x] 완료 증거 갱신

## Task 1: Phase 8 service contract red tests

**Files:**
- Modify: `spikes/purecvisor-desktop-node/service/tests/PcvDesktopService.Contract.Tests.ps1`

- [ ] **Step 1: token file helper red test 추가**

`New-PcvDesktopServiceTokenFile`이 token file을 만들고, 결과 JSON에 token 값을 직접 노출하지 않으며, `icacls.exe` runner를 호출하는지 검증한다.

- [ ] **Step 2: ACL command builder red test 추가**

`New-PcvTokenFileAclCommand`가 `LocalSystem`을 `NT AUTHORITY\SYSTEM` reader로 매핑하고, 관리자 principal과 service principal에 read 권한을 부여하는지 검증한다.

- [ ] **Step 3: service account red test 추가**

`New-PcvDesktopServiceConfig -ServiceAccount LocalSystem`과 install command가 `obj= LocalSystem`을 포함하는지 검증한다.

- [ ] **Step 4: red 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
```

Expected: 새 Phase 8 함수가 없어서 실패한다.

## Task 2: service module implementation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/service/PcvDesktopService.psm1`

- [ ] **Step 1: token helper 구현**

`New-PcvDesktopServiceToken`, `Get-PcvDesktopServiceDefaultTokenFilePath`, `New-PcvDesktopServiceTokenFile`을 추가한다.

- [ ] **Step 2: ACL helper 구현**

`Resolve-PcvServiceAccountAclPrincipal`, `New-PcvTokenFileAclCommand`, `Invoke-PcvTokenFileAclApply`를 추가한다.

- [ ] **Step 3: service account config 구현**

`New-PcvDesktopServiceConfig`에 `ServiceAccount`를 추가하고, `New-PcvDesktopServiceCommand -Action Install`의 `sc.exe create` arguments에 `obj=`를 추가한다.

- [ ] **Step 4: focused green 확인**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
```

Expected: service suite pass.

## Task 3: service entrypoint and documentation

**Files:**
- Modify: `spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1`
- Modify: `spikes/purecvisor-desktop-node/api/README.md`
- Modify: `spikes/purecvisor-desktop-node/service/README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/PUBLIC_RELEASE_BOUNDARY.md`
- Modify: `AGENTS.md`

- [ ] **Step 1: entrypoint action 추가**

`PrepareTokenFile` action을 추가한다. `-WhatIf`에서는 token file을 만들지 않고 ACL command preview를 출력한다.

- [ ] **Step 2: README 갱신**

token file 준비 명령, 서비스 설치 명령, 관리자 smoke 절차, Event Log provider 보류 판단을 기록한다.

- [ ] **Step 3: 상위 문서 갱신**

Phase 8 설계/계획 링크와 검증 정책을 추가한다.

## Task 4: verification

**Files:**
- Modify: `docs/superpowers/plans/2026-04-25-purecvisor-desktop-node-phase8-installer-hardening.md`

- [ ] **Step 1: 기본 검증 실행**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/web/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/hyperv/tests' -ExcludeTag Integration -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

- [ ] **Step 2: service smoke 실행**

Run:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action PrepareTokenFile -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Config -ApiTokenFile '<token-file>'
pwsh -NoProfile -ExecutionPolicy Bypass -File spikes/purecvisor-desktop-node/service/Invoke-PcvDesktopService.ps1 -Action Install -WhatIf -ApiTokenFile '<token-file>'
```

- [ ] **Step 3: 완료 증거 갱신**

검증 결과 수치를 이 문서의 완료 증거 절에 반영한다.

## 완료 증거

최종 검증 후 이 절을 최신 수치로 유지한다.

- Local API: 82 passed, 0 failed
- Service packaging: 12 passed, 0 failed
- CLI: 8 passed, 0 failed
- Web Console static suite: 9 passed, 0 failed
- Web JavaScript syntax: exit 0
- Hyper-V helper non-integration: 41 passed, 0 failed, 1 NotRun
- service `PrepareTokenFile -WhatIf`: exit 0, `icacls.exe` preview includes `BUILTIN\Administrators:R` and `NT AUTHORITY\SYSTEM:R`
- service `Config -ApiTokenFile`: exit 0, `service_account=LocalSystem`, `api_token_source=file`
- service `Install -WhatIf -ApiTokenFile`: exit 0, `sc.exe create` includes `obj= LocalSystem`
- `git diff --check`: exit 0
- 관리자 권한 통합 smoke: 미실행, README에 opt-in 절차로 분리
