# PureCVisor Desktop Node Phase 23 Windows Operational Evidence Plan

> **For agentic workers:** 이 plan은 Windows 장기 운영 증거를 수집하기 위한 실행 runbook이다. 실제 service mutation, Event Log source registration, firewall 변경, administrator action은 사용자 opt-in 이후에만 실행한다.

**Goal:** Phase 19 이후 남은 GA 차단 gate 중 Windows operational evidence를 JSONL first 장기 로그, service recovery, service log retention, diagnostic bundle, optional LAN/TLS preview 기준으로 수집한다.

**Architecture:** Phase 16 diagnostics policy를 유지하고, `packaging/windows-desktop-node/**` product wrapper, `src/DesktopNode.Host/**` default service host, `spikes/purecvisor-desktop-node/service/**` component service helper의 contract를 증거 수집 대상으로 둔다. 기본 검증은 non-admin Pester와 diff hygiene만 실행하고, Windows host mutation은 administrator opt-in runbook으로 분리한다.

**Tech Stack:** PowerShell 7, Pester 5, .NET Windows Service host, historical WinSW evidence, Windows service, JSONL diagnostics, diagnostic bundle manifest, optional Windows Event Log source plan, optional reverse proxy/TLS smoke.

---

## 상태

- 작성 기준: 2026-04-29
- 현재 상태: 비관리자 diagnostics validation과 operational evidence summary artifact 준비 완료, 2026-04-30 관리자 opt-in Event Log/source lifecycle evidence 수집 완료, 2026-05-01 관리자 opt-in service/Hyper-V/firewall/LAN/TLS preview 운영 hardening evidence 수집 완료. 이후 .NET Host replacement slice에서 기본 service host가 `DesktopNode.Host.exe`로 전환됐고 `artifacts/dotnet-host-admin-smoke-20260501-213444`에 service/MSI/Hyper-V helper smoke evidence를 기록했다.
- 제품 승격 판단: `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`
- Phase 23 결정: `DESKTOP_NODE_PHASE23_OPERATIONAL_EVIDENCE_DECISION: jsonl-first-long-run-evidence-with-eventlog-transition-deferred`
- 관련 설계: `docs/superpowers/specs/2026-04-29-purecvisor-desktop-node-phase23-windows-operational-evidence-design.md`

## 실행 금지 기본값

명시적 administrator opt-in 없이 실행하지 않는다.

- service install/start/stop/uninstall
- service failure 유도
- Event Log source registration/unregistration
- firewall rule 생성, 변경, 삭제
- elevated MSI install/repair/uninstall
- machine-wide ACL 변경

기본 문서/검증 작업에서는 pass count를 쓰지 않는다. 실제 명령을 실행한 경우에만 `완료 증거`에 결과를 기록한다.

## Task 1: Non-Admin Diagnostics Validation

**Files and commands:**
- Read: `packaging/windows-desktop-node/README.md`
- Read: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Verify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1`
- Verify: `spikes/purecvisor-desktop-node/tests`

- [ ] **Step 1: Confirm workspace scope**

Run:

```powershell
git status --short --branch
```

Expected: no unrelated edits are touched while collecting Phase 23 evidence. Phase 23 evidence records stay in the Phase 23 spec and plan files.

- [ ] **Step 2: Run packaging diagnostics tests**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
```

Expected: diagnostics policy, diagnostic bundle, redaction, log rotation, Event Log registration plan tests pass.

Phase 23 후속 개발 기준으로 이 suite는 `operational-evidence-redacted.json`도 검증한다. 해당 artifact는 SCM failure action recovery policy, service log retention policy, observed service log artifact names, Event Log deferred policy, host mutation 미수행 여부를 기록한다.

- [ ] **Step 3: Run root docs suite**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

Expected: root documentation and boundary tests pass.

- [ ] **Step 4: Run diff hygiene**

Run:

```powershell
git diff --check
```

Expected: no whitespace errors.

## Task 2: Long-Running Service Evidence

**Administrator opt-in required:** yes

**Evidence target:** this plan, `완료 증거`

- [ ] **Step 1: Record preflight without secrets**

Record:

- Windows version
- PowerShell version
- product wrapper version string
- service name
- data root placeholder as `[DATA_ROOT]`
- product root placeholder as `[PRODUCT_ROOT]`
- token storage mode, without token value or protected blob

- [ ] **Step 2: Install or reuse service only after opt-in**

Use the existing product wrapper or already installed service. Do not run from this plan unless administrator opt-in is explicit.

Evidence to record:

- install or existing-service source
- service status before long run
- runtime policy HTTP result summary
- `events.jsonl`, `install.jsonl`, and service log initial file list

- [ ] **Step 3: Observe long-running service**

Run duration is selected by the operator. Record:

- start timestamp
- end timestamp
- service status at end
- runtime policy or loopback health result
- event/install log file list after run
- service log file list after run
- diagnostic bundle path summary

Post-reboot verification 선택지:

장기 service run, service recovery, reboot 이후 diagnostics 확인에는 post-reboot verification runner를 사용할 수 있다. 먼저 `Register-PcvPostRebootVerification.ps1 -DryRun`으로 `ProductStatus` state/task plan과 principal 제약을 확인하고, runner evidence는 결과 summary와 redacted artifacts를 external evidence directory에 남긴다. 실제 Task Scheduler 등록은 이 Phase plan의 administrator opt-in gate가 명시적으로 열린 경우에만 실행한다. 자동 reboot는 사용하지 않으며 `-Reboot`는 `PCV_POST_REBOOT_AUTO_REBOOT_DISABLED`로 거부한다.

- [ ] **Step 4: Validate JSONL rotation/retention evidence**

Record:

- `events.jsonl` size and retained file count
- `install.jsonl` size and retained file count
- rotation helper result if invoked
- retained files do not exceed policy
- no raw token or protected token material appears in redacted evidence

## Task 3: Service Failure/Recovery Smoke

**Administrator opt-in required:** yes

**Evidence target:** this plan, `완료 증거`

- [ ] **Step 1: Capture configured failure actions**

Record the configured service failure action evidence without changing it unless opt-in includes that mutation.

Expected evidence:

- service name
- failure action command source
- restart delay or recovery policy summary
- whether policy came from SCM, .NET Host service-action, historical WinSW, or product wrapper command plan

- [ ] **Step 2: Run controlled failure only after opt-in**

Allowed examples:

- stop service and verify configured restart behavior if policy supports restart
- terminate the service process in an isolated smoke host
- inject a controlled startup failure in a disposable install

Record:

- failure trigger used
- timestamp
- service status transition
- recovery attempt result
- runtime policy or loopback health after recovery
- relevant log artifact names

- [ ] **Step 3: Collect diagnostics after recovery**

Run only after opt-in:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
```

Expected:

- command exits 0
- bundle contains service status, service-host status, runtime policy, event/install logs, service logs, diagnostics manifest
- bundle is redacted

## Task 4: Service Log Retention Evidence

**Administrator opt-in required:** maybe, depending on whether service logs already exist

- [ ] **Step 1: Capture service log directory**

Record redacted file names and sizes under:

```text
%ProgramData%\PureCVisor\desktop-node\service-logs
```

Do not paste log lines containing tokens, Authorization headers, host-specific absolute paths, or private data.

- [ ] **Step 2: Validate retention**

Expected policy:

- service host `*.log`, `*.out`, `*.err`: 10 MiB threshold
- retained file count: 10

Evidence:

- current file count by base log name
- oldest/newest timestamp summary
- rotation result if helper is invoked
- diagnostic bundle includes service log redacted artifacts

## Task 5: Optional Event Log Path

**Administrator opt-in required:** yes

Phase 23 default is no Event Log source registration. Run this task only to collect optional transition evidence.

- [ ] **Step 1: Build registration plan without mutation**

Run:

```powershell
Import-Module packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 -Force
$plan = New-PcvDesktopNodeProductPlan -Action Plan
New-PcvDesktopNodeEventLogRegistrationPlan -Plan $plan | ConvertTo-Json -Depth 16
```

Expected:

- `enabled_by_default = false`
- `registration_owner = admin-opt-in`
- source is `PureCVisor Desktop Node`
- log name is `Application`
- command object is printed but not executed

- [ ] **Step 2: Optional source registration smoke**

Do not run unless administrator opt-in explicitly includes Event Log mutation.

Evidence if run:

- source existed before smoke
- register command result
- write/read smoke result if a writer exists
- unregister or cleanup result if source was created by smoke
- source exists after cleanup

- [ ] **Step 3: Decide Event Log transition status**

Record one of:

- `keep-jsonl-first`: JSONL evidence is sufficient and Event Log transition remains deferred.
- `eventlog-transition-ready-for-design`: source lifecycle and writer evidence justify a follow-up design.
- `eventlog-transition-blocked`: source lifecycle, permissions, redaction, or diagnostics export has unresolved risk.

## Task 6: Optional LAN/TLS Preview Evidence

**Administrator opt-in required:** yes

LAN listener and reverse proxy/TLS remain preview-only.

- [ ] **Step 1: Confirm default loopback stance**

Record from plan or runtime policy:

- default exposure is loopback
- LAN mode disabled by default
- TLS is not provided by product wrapper
- firewall auto-enable is false

- [ ] **Step 2: Run LAN listener smoke only after opt-in**

Evidence:

- explicit `-AllowLan` opt-in
- token source is protected token file
- non-loopback API requires bearer token
- non-loopback static Web Console follows bearer token policy
- firewall rule was not changed unless opt-in explicitly included it

- [ ] **Step 3: Run reverse proxy/TLS smoke only after opt-in**

Evidence:

- reverse proxy or TLS terminator product/name/version
- TLS endpoint URL redacted as needed
- upstream points to loopback Desktop Node listener
- API runtime policy through TLS returns expected status
- Web Console root through TLS returns expected status

## Task 7: Docs Updates

**Files:**
- Modify only if assigned by a future worker: `follower.md`
- Modify only if assigned by a future worker: `docs/superpowers/plans/2026-04-27-purecvisor-desktop-node-phase-roadmap.md`
- Phase 23 evidence source: this plan and the Phase 23 spec

- [ ] **Step 1: Record evidence in this plan**

Add command summaries and redacted evidence under `완료 증거`.

- [ ] **Step 2: Keep shared indexes evidence-free**

Shared indexes may link to this Phase, but pass count, host details, service mutation results, and redacted artifacts stay in this plan.

- [ ] **Step 3: Keep pass counts local**

Only record pass counts for commands actually run in the current evidence update.

## Validation Commands

Run for documentation-only updates:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

Run for diagnostics policy, redaction, bundle, or rotation changes:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Run if service helper docs or service command builder changes:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
```

## 완료 증거

2026-04-29 비관리자 진단 증거만 갱신했다. service install/start/stop, Event Log source registration, firewall 변경, MSI install, 장기 실행 service smoke, failure/recovery smoke는 실행하지 않았다.

Phase 23 비관리자 검증:

- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"`: 13 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 13 passed, 0 failed
- `git diff --check`: exit 0

2026-04-30 후속 개발에서 diagnostic bundle의 Phase 23 운영 증거 요약 artifact를 추가했다.

- RED: `operational-evidence-redacted.json` artifact가 없어 Phase 23 service recovery/log retention evidence 테스트가 실패했다.
- GREEN: bundle 생성이 WinSW XML `onfailure` policy, service log retention policy, observed service log diagnostic artifact, Event Log deferred policy, host mutation 미수행 여부를 `operational-evidence-redacted.json`으로 기록하고 manifest `operational_evidence` source에 포함하도록 수정했다.
- Host mutation: 없음. Pester `$TestDrive`의 WinSW XML과 service log fixture만 읽고 diagnostic bundle 파일만 생성했다.
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"`: 16 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"`: 88 passed, 0 failed
- `pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"`: 14 passed, 0 failed
- `git diff --check`: exit 0, CRLF warning only

2026-05-01 관리자 opt-in service/Hyper-V/firewall/LAN/TLS preview 운영 hardening evidence:

- primary evidence root: `artifacts/admin-optin-continuation-20260501-162940`
- evidence JSON: `artifacts/admin-optin-continuation-20260501-162940/admin-optin-continuation-evidence.json`
- service wrapper restore evidence: `artifacts/admin-optin-service-wrapper-restore-20260501-162904/service-wrapper-restore-evidence.json`
- TLS reverse proxy preview evidence: `artifacts/admin-optin-tls-reverse-proxy-preview-20260501-163308/tls-reverse-proxy-preview-evidence.json`
- auto reboot: disabled. `Restart-Computer`, MSI `msiexec` lifecycle, mutating update/rollback, public trusted/stable publication은 실행하지 않았다.
- protected token file ACL inspection: owner `NT AUTHORITY\SYSTEM`, explicit read grants `NT AUTHORITY\SYSTEM` and `BUILTIN\Administrators`, token value와 protected blob은 출력하지 않았다.
- service lifecycle: WinSW wrapper `PureCVisorDesktopNode.exe` 기준 stop/drain/uninstall/install/start가 통과했고 service는 `Running`, `binPath`는 `C:\Program Files\PureCVisor\DesktopNode\PureCVisorDesktopNode.exe`로 복구/확인됐다.
- service failure/recovery policy: SCM failure action은 `reset=86400`, `restart/60000`, `restart/60000`으로 실제 적용/조회했다. Controlled crash는 실행하지 않았다.
- service helper caution: `Invoke-PcvDesktopService.ps1 -Action Install`은 제품 WinSW wrapper install 경로가 아니므로 product service reinstall smoke에는 사용하지 않는다. 해당 경로가 direct `pwsh.exe -File Invoke-PcvDesktopApi.ps1` service binPath를 만들 수 있음을 확인했고, WinSW wrapper uninstall/install/start로 복구했다.
- firewall lifecycle: scoped inbound smoke rule create/update/delete 통과, cleanup 후 matching smoke firewall rule 잔여 0.
- Event Log lifecycle: scoped Application source create, Write-EventLog, Get-WinEvent readback, Remove-EventLog 통과, final source exists false.
- LAN listener/firewall preview: explicit `-AllowLan`, protected token file, scoped firewall rule, non-loopback listener `[LAN_IP]:7788`, runtime policy `current_exposure=lan`, token storage `dpapi-local-machine`, cleanup 완료.
- TLS reverse proxy preview: PowerShell/.NET ephemeral self-signed cert와 Node HTTPS reverse proxy로 loopback upstream `127.0.0.1:7777` runtime policy/host status를 `SkipCertificateCheck` preview로 확인했다. trust store에 설치하지 않았고 private key는 cleanup 후 제거했다.
- Hyper-V direct lifecycle: scoped `pcv-admin-*` VM create/start/checkpoint/poweroff/remove 통과, cleanup 후 test VM 잔여 0.
- Product API Hyper-V lifecycle: signed/elevated product service 상태에서 `pcv-api-*` VM create/start/checkpoint/poweroff를 Product API job route로 실행했고 cleanup 후 test VM 잔여 0. Direct checkpoint list에서도 checkpoint 이름 visible 확인.
- operational sampling: 75초 동안 service/runtime/host status sample과 JSONL/WinSW log tail을 수집했다.
- final health: service `Running`, runtime policy ok, host status ok, exposure `loopback`, token storage `dpapi-local-machine`, VMMS running true.
- 판정: 선택적 운영 hardening evidence는 충족한다. JSONL-first는 계속 primary이며, Event Log writer/provider 기본 활성화, public trusted/stable signing, GA 승격은 별도 판단으로 남긴다.

2026-05-01 관리자 opt-in service/MSI/firewall/Event Log 보강 evidence:

- evidence root: `artifacts/admin-optin-hyperv-service-msi-firewall-eventlog-20260501-185911`
- scope: 기존 2026-05-01 관리자 opt-in hardening evidence를 보강하는 bounded smoke
- service: final restore 후 `PureCVisorDesktopNode` service `Running`, Web root HTTP `200`
- service failure action: `final/sc-qfailure-final.out.txt`에서 `reset=86400`, restart/restart action 조회 성공
- MSI: internal enterprise `RequireSigned` `0.23.10-rc.1` install/repair/uninstall/install-remove-data/uninstall-remove-data/final restore 성공
- firewall: scoped smoke rule 생성 후 cleanup, final matching rule count `0`
- Event Log: scoped Application source create/write/read/remove 완료, final source absent
- Hyper-V: create/start/checkpoint list/poweroff/remove 성공, checkpoint list와 direct snapshot 모두 이름 확인
- reboot policy: `auto_reboot_disabled = true`, `reboot_observed = false`
- 판정: 운영 hardening evidence를 보강한다. JSONL-first는 계속 primary이며 Event Log 기본 활성화, public trusted signing/stable publication, GA 승격은 별도 판단으로 남긴다.

2026-05-01 .NET Windows Service Host replacement 후속 evidence:

- evidence root: `artifacts/dotnet-host-admin-smoke-20260501-213444`
- service host: 기본 제품 service host와 MSI installed custom action runner가 `DesktopNode.Host.exe`로 전환됐다.
- service/MSI: direct service-action install/start/delete, MSI install/repair/uninstall/`REMOVE_DATA=1`/final restore가 자동 reboot 없이 통과했다.
- Hyper-V: helper integration smoke에서 `host.status`, `vm.create`, `vm.list`, `vm.start`, `checkpoint.create`, `vm.poweroff`가 통과했고 `pcv-spike-*` VM 잔여물은 없었다.
- 판정: WinSW wrapper 운영 hardening evidence 이후 기본 service host가 .NET Host로 교체됐음을 기록한다. 이 evidence는 `AllowUnsignedDev` admin-smoke 범위이므로 public trusted/stable signing 또는 GA 승격 evidence가 아니다.

남은 gate:

- public trusted/stable signing과 stable publication evidence
- GA 제품 런타임 승격 재판정

2026-04-30 관리자 opt-in operational/Event Log lifecycle evidence:

- evidence root: `artifacts/p1-operational-eventlog-lifecycle-20260430-2050`
- service: `PureCVisorDesktopNode` Running
- observed running duration: 2398 seconds
- ProductStatus: exit `0`
- CollectDiagnostics: exit `0`
- diagnostics bundle: `%ProgramData%\PureCVisor\desktop-node\diagnostics\bundle-20260430-105731-74c8d759`
- Web root: HTTP `200`
- Event Log source: `PureCVisor Desktop Node`
- source existed before: false
- source registered by smoke: true
- Write-EventLog / Get-WinEvent readback: true / true
- cleanup: source removed after smoke, final source exists false
- transition status: `eventlog-source-lifecycle-verified-jsonl-first-kept`
- host mutation scope: Event Log source registration/write/removal only. service/MSI/firewall/reboot mutation 없음.
- 판정: draft-ready 운영 evidence는 충족한다. JSONL-first는 계속 primary이며, Event Log writer/provider 기본 활성화는 후속 설계 판단으로 남긴다.
