# PureCVisor Desktop Node Phase 20 Signed Release/MSI Lifecycle Evidence Plan

> **For agentic workers:** 이 plan은 signed release build와 elevated MSI lifecycle evidence를 수집하기 위한 실행 runbook이다. 실제 signing secret, elevated `msiexec`, service mutation은 사용자 opt-in 이후에만 실행한다.

**Goal:** Phase 19 이후 남은 GA 차단 gate 중 signed release build와 elevated MSI lifecycle smoke evidence를 같은 절차와 문서 위치에 수집한다.

**Architecture:** 제품 후보 installer build는 `packaging/windows-desktop-node/installer/build.ps1`의 `RequireSigned` signing mode를 사용한다. Phase 22 후속 개발 이후 signed MSI artifact는 `PureCVisorDesktopNode-<version>-windows-x64.msi` naming을 따른다. elevated lifecycle smoke는 같은 MSI artifact로 install, repair, 기본 uninstall, remove-data uninstall을 반복하고, 결과는 이 plan의 `완료 증거`에만 기록한다.

**Tech Stack:** PowerShell 7, Pester 5, WiX CLI, SignTool, Windows Installer `msiexec`, WinSW, Desktop Node product wrapper.

---

## 상태

- 작성 기준: 2026-04-29
- 현재 상태: local signed RC lifecycle evidence 완료, current-head local `RequireSigned` MSI lifecycle/update smoke 재확인 완료, internal enterprise `RequireSigned` MSI lifecycle evidence 완료, release approval evidence 기록 완료, public trusted signing evidence는 없음
- 제품 승격 판단: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- 관련 설계: `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase20-signed-release-msi-lifecycle-evidence-design.md`

## Task 1: Non-Admin Preflight

**Files:**
- Read: `packaging/windows-desktop-node/installer/build.ps1`
- Read: `packaging/windows-desktop-node/installer/README.md`
- Verify: `packaging/windows-desktop-node/installer/tests`
- Verify: `packaging/windows-desktop-node/tests`
- Verify: `spikes/purecvisor-desktop-node/tests`

- [x] **Step 1: 깨끗한 작업 트리 확인**

Run:

```powershell
git status --short --branch
```

Expected: no uncommitted changes except the current Phase 20 documentation branch when documenting the run.

- [x] **Step 2: root 문서 suite 실행**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: all root documentation tests pass.

- [x] **Step 3: installer suite 실행**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected: installer source, signing, and provenance contract tests pass.

- [x] **Step 4: packaging suite 실행**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Expected: product wrapper plan, manifest, invoke, diagnostics tests pass.

## Task 2: Signed Release Build Evidence

**Files and artifacts:**
- Build script: `packaging/windows-desktop-node/installer/build.ps1`
- Output root: `artifacts/windows-desktop-node-release`
- Evidence target: this plan, `완료 증거`

- [x] **Step 1: 저장소 밖 signing 입력 확인**

Required inputs:

- `signtool.exe` path
- certificate thumbprint or external certificate path
- timestamp URL
- release WinSW executable path
- release version string

Do not write private key, PFX password, raw API token, protected token blob, or signing secret into this repo.

- [x] **Step 2: RequireSigned build 실행**

Example command:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version 0.20.0-rc.1 `
  -WinSwPath '<winsw.exe>' `
  -OutputRoot artifacts/windows-desktop-node-release `
  -SigningMode RequireSigned `
  -SigningTrustModel InternalEnterprise `
  -SignToolPath '<signtool.exe>' `
  -CertificateThumbprint '<thumbprint>' `
  -TimestampUrl '<timestamp-url>'
```

Expected:

- command exits 0
- MSI artifact exists
- provenance manifest exists
- provenance has `signing_mode = RequireSigned`
- provenance has `signing_trust_model = InternalEnterprise` 또는 실제 선택된 trust model
- SignTool output is captured without certificate secret material

- [x] **Step 3: signed build 증거 기록**

Record in `완료 증거`:

- command date/time
- Windows version
- PowerShell version
- WiX version
- SignTool path summary without secret values
- certificate input type without private key material
- MSI path and SHA-256
- provenance path and SHA-256
- WinSW source artifact path and SHA-256
- command exit code

## Task 3: Elevated MSI Lifecycle Smoke

**Files and artifacts:**
- MSI from Task 2
- Installer logs under an external evidence directory
- Product data root: `%ProgramData%\PureCVisor\desktop-node`

Post-reboot verification 선택지:

서명/MSI lifecycle smoke 중 Windows reboot가 필요한 경우 `packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -DryRun`으로 먼저 `ProductStatus` 또는 `PackagingRegression` state/task plan을 확인한다. Runner evidence는 state file 기반 command 실행과 redacted artifact 작성을 지원한다. `-ContinuationProfiles PackagingRegression`을 함께 사용하면 reboot 이후 `ProductStatus`가 성공한 뒤 packaging regression을 자동으로 이어 실행한다. 실제 Task Scheduler 등록은 별도 administrator opt-in이고, `-Reboot` 자동 실행은 금지한다. Signing material이나 user certificate store가 필요한 command plan은 `LocalSystemAtStartup`에서 실행하지 않고 `CurrentUserAtLogOn` opt-in으로만 다룬다.

- [x] **Step 1: 관리자 PowerShell 시작**

Confirm administrator context:

```powershell
([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
  [Security.Principal.WindowsBuiltInRole]::Administrator)
```

Expected: `True`.

- [x] **Step 2: MSI 설치**

Run:

```powershell
$msi = 'artifacts/windows-desktop-node-release/PureCVisorDesktopNode-0.20.0-rc.1-windows-x64.msi'
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install.log
```

Expected:

- exit code 0
- `PureCVisorDesktopNode` service exists and reaches Running
- product wrapper `Status` action returns OK

- [x] **Step 3: runtime policy와 Web Console 검증**

Read the protected token through the existing service helper, then check:

```powershell
Invoke-WebRequest http://127.0.0.1:7777/api/v1/runtime/policy -Headers @{ Authorization = "Bearer <token>" }
Invoke-WebRequest http://127.0.0.1:7777/
```

Expected:

- runtime policy returns HTTP 200
- `token_storage` is `dpapi-local-machine`
- Web Console root returns HTTP 200

- [x] **Step 4: MSI repair 실행**

Run:

```powershell
msiexec /i $msi REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx repair.log
```

Expected:

- exit code 0
- repair smoke는 `/fa` shorthand를 사용하지 않는다. 2026-04-30 signed RC smoke에서 `/fa`는 server command line에 `REBOOT=ReallySuppress`를 전달하지 않았고, `REINSTALLMODE=a`가 in-use file replacement와 실제 reboot를 유발했다.
- exit code 3010은 service/runtime/data preservation assertion이 모두 통과할 때만 `reboot_required=true` 성공으로 기록한다.
- exit code 1641은 Windows Installer가 실제 재부팅을 시작한 결과이므로 기본 lifecycle smoke 성공으로 닫지 않는다. 해당 run은 실패/중단 evidence로 기록하고, post-reboot verification evidence와 원인 분석을 별도로 남긴다.
- service returns Running
- protected token, job store, event log, install log, diagnostics directory are preserved

- [x] **Step 5: 기본 uninstall 실행**

Run:

```powershell
msiexec /x $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall.log
```

Expected:

- exit code 0
- service is removed
- port `7777` listener is absent
- `%ProgramData%\PureCVisor\desktop-node` is preserved

- [x] **Step 6: remove-data smoke용 재설치**

Run:

```powershell
msiexec /i $msi REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx install-remove-data.log
```

Expected: install succeeds and service reaches Running.

- [x] **Step 7: remove-data uninstall 실행**

Run:

```powershell
msiexec /x $msi REMOVE_DATA=1 REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable /qn /norestart /l*vx uninstall-remove-data.log
```

Expected:

- exit code 0
- service is removed
- port `7777` listener is absent
- protected token file is removed
- legacy raw token file is removed
- job store is removed
- `events.jsonl` is removed
- `install.jsonl` is removed
- diagnostics directory is removed

## Task 4: Evidence Documentation

**Files:**
- Modify: this plan
- Optionally modify: `follower.md`
- Optionally modify: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`

- [x] **Step 1: 증거 redaction**

Before committing any evidence:

- remove raw token values
- remove protected token blob values
- remove PFX password or certificate private key material
- avoid full certificate thumbprint if not needed; use a short non-secret summary
- keep SHA-256 of public artifacts

- [x] **Step 2: `완료 증거` 갱신**

Record:

- command outcomes
- artifact paths and hashes
- service/runtime/Web Console checks
- data preservation/removal checks
- failures and cleanup if any

- [x] **Step 3: 후속 queue 갱신**

If both signed build and elevated MSI lifecycle evidence pass, move this gate out of `follower.md` next priority and promote Hyper-V lifecycle integration evidence.

## 기본 검증

Run after documentation updates:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Run after installer/script changes:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

## 완료 증거

2026-04-30 P0 signed RC MSI build:

- commit: `2cc1873fc99acbd905c0c348988e0bf403e8571f` (이 브랜치에는 요청 기준 commit `7d9b778`가 포함됨)
- version: `0.23.8-rc.1`
- build command: `packaging/windows-desktop-node/installer/build.ps1 -SigningMode RequireSigned`
- signing trust model: historical local test signer before explicit `signing_trust_model` provenance
- SignTool: Windows Kits 10 `signtool.exe`
- certificate input: CurrentUser code signing certificate thumbprint 입력 사용, private key/PFX password 미기록
- timestamp: DigiCert timestamp URL 사용
- output root: `artifacts/p0-signed-rc-msi-20260430`
- MSI: `PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi`
- MSI SHA-256: `fba84c3dbc85e6edb6467d98f0ba42301dc87a8f8b7bfd58107cb198cf2d8e76`
- provenance: `PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.provenance.json`
- provenance fields: `release_channel = rc`, `signing_mode = RequireSigned`, `msi.signed = true`, `git_commit = 2cc1873fc99acbd905c0c348988e0bf403e8571f`
- build exit code: 0
- SignTool sign exit code: 0
- trust note: local test code-signing certificate로 서명되어 Authenticode trust provider 체인은 신뢰 루트에서 실패할 수 있음. 이 상태는 public trusted release certificate evidence가 아니라 signed RC smoke evidence로만 기록한다.

2026-04-30 P0 signed MSI lifecycle smoke 중단 evidence:

- MSI: `artifacts/p0-signed-rc-msi-20260430/PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi`
- evidence/log root: `artifacts/p0-signed-msi-lifecycle-20260430`
- install log: `install-0.23.8-rc.1.log`
- repair log: `repair-0.23.8-rc.1.log`
- repair result: Windows Installer `MainEngineThread is returning 1641`
- System Event Log: `2026-04-30 16:45:42` Event ID `1074`, process `msiexec.exe`, user `NT AUTHORITY\SYSTEM`, message states Windows Installer initiated restart to complete/continue configuration of `PureCVisor Desktop Node`
- post-reboot boot time observed: `2026-04-30 16:46:15`
- after reboot: service `PureCVisorDesktopNode` observed `Running`, `product-manifest.json` version `0.23.8-rc.1`, loopback port `127.0.0.1:7777` listening
- 판정: signed RC build gate는 evidence 생성 완료. signed MSI lifecycle smoke는 repair `1641`로 실패/중단이며, install/repair/uninstall/`REMOVE_DATA=1` 전부 exit 0 조건을 닫지 못했다.
- 후속: repair smoke는 자동 reboot 없이 재설계해야 한다. `1641`은 성공 허용값이 아니며, post-reboot verification과 원인 분석 evidence를 붙인 별도 incident로 남긴다.

2026-04-30 repair `1641` 원인 분석 및 non-mutating 계약 고정:

- repair log line 42: Windows Installer server command line은 `REINSTALL=ALL REINSTALLMODE=a ...`였고, install/uninstall log와 달리 `REBOOT=ReallySuppress`가 보이지 않았다.
- repair log line 73: `MsiSystemRebootPending = 1`.
- repair log line 254-262: Restart Manager가 no-UI mode에서 file holder 종료를 시도했고 service session 종료 일부가 실패했다.
- repair log line 411-512: `REINSTALLMODE=a` 때문에 installed payload 파일들이 force overwrite 대상으로 잡혔다.
- repair log line 538/668: `ReplacedInUseFiles = 1`.
- 결론: `/fa` shorthand repair는 이번 smoke에서 public property 전달과 in-use file 교체 측면 모두 위험했다. 재실행 전 계약은 `/i` + `REINSTALL=ALL` + `REINSTALLMODE=vomus` + `REBOOT=ReallySuppress` + `MSIRESTARTMANAGERCONTROL=Disable`로 고정한다.
- 테스트: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1`가 repair command shape, reboot suppression, `3010` 조건부 성공, `3010` conditional exit 분리, `1641` 실패/중단 분류를 검증한다.
- 구현: `packaging/windows-desktop-node/installer/PcvDesktopNodeMsiLifecycle.psm1`는 실제 `msiexec` 실행 없이 lifecycle smoke plan과 exit-code classification만 생성한다.

2026-04-30 MSI lifecycle repair 계약 검증:

- RED: lifecycle test 추가 직후 `New-PcvMsiLifecycleSmokePlan`/`ConvertTo-PcvMsiLifecycleExitClassification` 부재로 4개 테스트 실패 확인.
- RED: conditional exit 분리 테스트 추가 직후 `success_exit_codes` 부재로 1개 테스트 실패 확인.
- GREEN: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1' -Output Detailed"`: 5 passed, 0 failed.
- installer suite: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 31 passed, 0 failed.
- packaging suite: `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 96 passed, 0 failed.
- root boundary suite: `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 15 passed, 0 failed.
- `git diff --check`: exit 0.

Phase 20 시작 문서화 검증:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 13 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 24 passed, 0 failed
- `git diff --check`: exit 0

2026-04-29 후속 병렬 개발 중 non-admin preflight 재검증:

- `git status --short --branch`: `## codex/followup-parallel-evidence`
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 13 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"`: 26 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 72 passed, 0 failed

2026-04-30 P0 signed MSI lifecycle rerun 완료:

- MSI: `artifacts/p0-signed-rc-msi-20260430/PureCVisorDesktopNode-0.23.8-rc.1-windows-x64.msi`
- evidence root: `artifacts/p0-signed-msi-lifecycle-rerun-20260430-191040`
- preclean uninstall preserve: exit `0`, classification `success`
- install: exit `0`, classification `success`, assertion `installed-runtime-healthy`
- repair: exit `0`, classification `success`, assertion `repair-runtime-healthy`, reboot required false
- 기본 uninstall: exit `0`, classification `success`, assertion `uninstalled-data-preserved`
- remove-data smoke용 reinstall: exit `0`, assertion `reinstalled-runtime-healthy`
- `REMOVE_DATA=1` uninstall: exit `0`, assertion `uninstalled-remove-data-clean`
- restore install: exit `0`, assertion `restored-runtime-healthy`
- reboot controls: `REBOOT=ReallySuppress`, `MSIRESTARTMANAGERCONTROL=Disable`, `/norestart`
- automatic reboot: not used
- rebooted during run: false
- 판정: local test certificate 기준 signed RC build와 elevated MSI lifecycle exit-0 smoke는 완료됐다. 이 결과는 public trusted/stable signing evidence가 아니며 GA 승격을 의미하지 않는다.

2026-05-01 current-head local `RequireSigned` lifecycle/update smoke 완료:

- evidence root: `artifacts/p0-local-requiresigned-rc-msi-20260501-165251`
- commit: `3d35aa247363a93c89c33b210f640048b3211c34`
- version: `0.23.9-rc.1`
- MSI: `PureCVisorDesktopNode-0.23.9-rc.1-windows-x64.msi`
- MSI SHA-256: `418e1d6d7bad5d5e6e333051810e696b3a549839ce5ab5efcb0de2336069fb8a`
- signing mode: `RequireSigned`
- signer: local self-signed `CN=PureCVisor Desktop Node Test Code Signing`
- SignTool verify exit: `0`
- Authenticode: `Valid`
- preclean uninstall current `0.23.8-rc.1`: exit `0`
- install: exit `0`, runtime healthy
- repair: exit `0`, runtime healthy, reboot required false
- 기본 uninstall: exit `0`, protected token preserved
- remove-data smoke용 reinstall: exit `0`, runtime healthy
- `REMOVE_DATA=1` uninstall: exit `0`, protected token, legacy token, job store, events/install logs, diagnostics removed
- product-wrapper update/config migration/rollback/CollectDiagnostics smoke: all PASS
- final MSI restore install: exit `0`, service `Running`, BinPath `C:\Program Files\PureCVisor\DesktopNode\PureCVisorDesktopNode.exe`
- automatic reboot: not used
- boot time unchanged: true
- 판정: current-head local test `RequireSigned` build와 elevated MSI lifecycle/update compatibility smoke는 완료됐다. Public trusted certificate/PFX/private key가 local environment에 없어 stable publication은 실행하지 않았고, 이 결과는 public trusted/stable signing evidence가 아니며 GA 승격을 의미하지 않는다.

2026-05-01 internal enterprise `RequireSigned` lifecycle smoke 완료:

- evidence root: `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021`
- commit: `318ebc39b8f224c7c24895c485089b1469c4ac66`
- version: `0.23.10-rc.1`
- MSI: `PureCVisorDesktopNode-0.23.10-rc.1-windows-x64.msi`
- MSI SHA-256: `5355507f5909d5e17280a90b8ac41af858b871633b8ec2e1b03f2b4eb26297ba`
- signing mode: `RequireSigned`
- signing trust model: `InternalEnterprise`
- signer: `CN=PureCVisor Desktop Node Internal Code Signing`
- issuer: `CN=PureCVisor Internal Code Signing Root CA`
- signing store: `Cert:\CurrentUser\My`
- trust stores: `Cert:\LocalMachine\Root`, `Cert:\LocalMachine\TrustedPublisher`
- SignTool verify exit: `0`
- Authenticode: `Valid`
- install: PASS, runtime healthy
- repair: PASS, runtime healthy
- 기본 uninstall: PASS, protected token preserved
- remove-data smoke용 reinstall: PASS, runtime healthy
- `REMOVE_DATA=1` uninstall: PASS, protected token, legacy token, job store, events/install logs, diagnostics removed
- final MSI restore install: PASS, service `Running`
- automatic reboot: not used
- boot time unchanged: true
- 판정: 내부 서비스용 internal trust release evidence는 완료됐다. 이 결과는 public trusted signing, 외부 stable publication, 또는 GA 승격 evidence가 아니다.

2026-05-01 관리자 opt-in MSI/service 보강 evidence:

- evidence root: `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911`
- MSI source: `artifacts/internal-enterprise-requiresigned-rc-msi-20260501-181021/PureCVisorDesktopNode-0.23.10-rc.1-windows-x64.msi`
- signing mode/trust model: `RequireSigned`, `InternalEnterprise`
- lifecycle: install, repair, 기본 uninstall, remove-data smoke용 install, `REMOVE_DATA=1` uninstall, final restore install 모두 성공
- final restore: service `PureCVisorDesktopNode` `Running`, Web root HTTP `200`
- reboot policy: `auto_reboot_disabled = true`, `reboot_observed = false`
- related operational evidence: final service failure action 조회는 `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911/final/sc-qfailure-final.out.txt`
- 판정: 기존 2026-05-01 관리자 opt-in hardening evidence를 보강한다. 내부 enterprise trust 운영 증거이며 public trusted signing, stable publication, 제품 runtime replacement, 또는 GA 승격을 의미하지 않는다.

2026-04-30 release approval/signing preflight 후속 evidence:

- evidence root: `artifacts/p1-release-approval-and-signing-preflight-20260430-2045`
- SignTool: Windows Kits 10 x64 `signtool.exe` 확인
- code-signing cert inventory: CurrentUser/LocalMachine `My` store의 Code Signing EKU 인증서 `0`
- repo certificate file candidates: `0`
- stable unsigned build block: `build.ps1 -Version 0.23.8 -SigningMode AllowUnsignedDev` exit `1`, `PCV_INSTALLER_RELEASE_SIGNING_REQUIRED`
- existing RC MSI Authenticode: local test certificate signer, trust provider status `UnknownError`, SignTool verify exit `1`
- 판정: public trusted/stable signing evidence는 없다. 사용자 명시 release approval로 draft-ready release signing gate를 닫되, public trusted signature 또는 stable publication으로 주장하지 않는다.

아직 실행하지 않은 gate:

- stable channel 발행 승인
