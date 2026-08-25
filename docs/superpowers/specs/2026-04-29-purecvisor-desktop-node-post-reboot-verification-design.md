# PureCVisor Desktop Node Post-Reboot Verification 설계

## 목적

Post-reboot verification은 관리자 smoke 중 Windows server reboot가 필요한 경우에도 검증 흐름을 사람이 수동으로 다시 이어 붙이지 않도록 만든다.

현재 Phase 20, Phase 21, Phase 23 runbook은 실제 host mutation을 administrator opt-in gate로 분리한다. 이 원칙은 유지한다. 다만 장시간 smoke, service recovery, installer lifecycle, Hyper-V lifecycle 검증 도중 reboot가 필요하면 사용자가 매번 관리자 PowerShell을 다시 열고 Codex를 재개한 뒤 진행 상태를 재확인해야 한다. 이 설계는 reboot 이후 후속 검증 명령과 evidence 기록을 1회성 elevated scheduled task가 자동으로 처리하도록 정의한다.

Codex TUI는 자동으로 재개하지 않는다. 자동화 대상은 post-reboot 검증 명령 실행과 redacted evidence 기록이다.

## 범위

포함한다.

- pre-reboot 상태 파일 생성
- post-reboot 검증 command plan 작성
- Windows Task Scheduler 1회성 elevated task 등록 계획
- reboot 후 자동 검증 runner
- command별 exit code, stdout/stderr summary, 실행 시간 evidence 기록
- Windows boot time, PowerShell version, git commit, dirty working tree summary 기록
- task 자기 정리와 중복 실행 방지
- non-admin Pester에서 검증 가능한 command builder/runner contract
- administrator opt-in smoke 절차

제외한다.

- Codex TUI 또는 Codex app 자동 실행
- 자동 로그인, 계정 password 저장, credential persistence
- reboot 자체를 기본 동작으로 수행
- Windows service install/start/stop/delete를 기본 검증으로 실행
- Hyper-V VM 생성, 삭제, power lifecycle을 기본 검증으로 실행
- MSI `msiexec` install/repair/uninstall을 기본 검증으로 실행
- Windows Firewall rule 변경 또는 Event Log source 등록을 기본 검증으로 실행
- Linux `purecvisor-single`, Linux `purecvisorsd`, KVM/libvirt/LXC/ZFS/OVS/OVN runtime 변경

## 결정

```text
DESKTOP_NODE_POST_REBOOT_VERIFICATION_DECISION: one-shot-elevated-task-with-stateful-evidence-runner
```

Post-reboot verification은 1회성 elevated scheduled task와 상태 파일 기반 runner를 사용한다.

설계 원칙:

1. Reboot 전 작업은 명시적 administrator opt-in에서만 task를 등록한다.
2. Reboot 실행은 별도 `-Reboot` opt-in이 있을 때만 수행한다.
3. Reboot 후 task는 검증 명령과 evidence 기록만 수행한다.
4. Task는 완료 후 등록을 제거한다.
5. Task는 상태 파일의 repo boundary, task id, phase id, command allowlist를 검증한 뒤에만 실행한다.
6. Evidence에는 token, protected token blob, signing secret, Authorization header, private key, password를 남기지 않는다.

## 구성 요소

### Pre-Reboot 준비 스크립트

후속 구현은 다음 스크립트를 추가한다.

```text
packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1
```

책임:

- 관리자 권한 확인
- 저장소 root 확인
- phase id와 evidence directory 확인
- post-reboot command plan 생성
- 상태 파일 생성
- task principal 선택과 제약 검증
- Task Scheduler 1회성 task 등록
- `-Reboot`가 명시된 경우에만 Windows reboot 요청

기본 실행은 task 등록까지만 수행한다. Reboot는 기본값이 아니다.

### Post-Reboot Runner

후속 구현은 다음 스크립트를 추가한다.

```text
packaging/windows-desktop-node/tools/Invoke-PcvPostRebootVerification.ps1
```

책임:

- 상태 파일 로드
- repo boundary 재검증
- task id와 machine marker 확인
- 이미 완료된 run인지 확인
- Windows boot time 기록
- git commit과 dirty summary 기록
- command plan 순차 실행
- command별 stdout/stderr redaction
- JSON evidence와 Markdown summary 작성
- task unregister 시도
- 완료 marker 작성

Runner는 Codex를 실행하지 않는다.

### 상태 파일

상태 파일은 evidence directory 아래에 둔다.

```text
post-reboot-state.json
```

필수 필드:

- `schema_version`
- `phase_id`
- `task_name`
- `repo_root`
- `evidence_dir`
- `created_at_utc`
- `created_by_user`
- `machine_name`
- `pre_reboot_boot_time_utc`
- `commands`
- `redaction`
- `cleanup`
- `principal`

`commands` 항목은 다음 필드를 가진다.

- `id`
- `working_directory`
- `file_name`
- `arguments`
- `timeout_seconds`
- `required`
- `allow_failure`
- `summary_pattern`

Command plan은 repo 내부 상대 경로와 명시적 executable만 허용한다. Shell free-form string은 기본으로 금지한다.

`principal` 항목은 다음 필드를 가진다.

- `mode`: `LocalSystemAtStartup` 또는 `CurrentUserAtLogOn`
- `requires_user_profile`
- `requires_network_drive`
- `requires_signing_material`

기본 mode는 `LocalSystemAtStartup`이다. 사용자 profile, mapped network drive, user certificate store, signing material이 필요한 command plan은 기본 mode에서 거부하고 `CurrentUserAtLogOn` opt-in을 요구한다.

### Evidence 출력

Runner는 같은 evidence directory에 다음 파일을 작성한다.

```text
post-reboot-result.json
post-reboot-summary.md
post-reboot-stdout-<command-id>.log
post-reboot-stderr-<command-id>.log
```

`post-reboot-result.json` 필수 필드:

- `schema_version`
- `phase_id`
- `task_name`
- `started_at_utc`
- `finished_at_utc`
- `ok`
- `windows_boot_time_utc`
- `powershell_version`
- `git_commit`
- `git_status_summary`
- `commands`
- `cleanup`

Command result 필드:

- `id`
- `exit_code`
- `duration_ms`
- `timed_out`
- `stdout_artifact`
- `stderr_artifact`
- `summary`
- `ok`

`post-reboot-summary.md`는 phase plan의 `완료 증거`에 붙일 수 있는 짧은 한국어 요약으로 작성한다. Host absolute path는 placeholder로 치환한다.

## 기본 Command Profile

Post-reboot runner는 임의 shell command를 받지 않고 allowlist된 profile을 명시적으로 선택한다. 후속 검증은 `-ContinuationProfiles`로 같은 allowlist profile 이름을 나열한다.

### ProductStatus profile

목적: reboot 후 설치된 Desktop Node service와 product root 상태를 확인한다.

사용 예:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase23 -EvidenceDir '<evidence-dir>' -Profile ProductStatus -DryRun
```

Profile 내부 allowlist는 product status와 diagnostics 수집으로 제한한다.

### PackagingRegression profile

목적: reboot 후 product wrapper와 installer contract가 계속 통과하는지 확인한다.

사용 예:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase23 -EvidenceDir '<evidence-dir>' -Profile ProductStatus -ContinuationProfiles PackagingRegression -DryRun
```

Profile 내부 allowlist는 packaging product test, installer test, `git diff --check`로 제한한다.

### HyperVNonIntegration profile

목적: Phase 21에서 reboot 이후 Hyper-V helper non-integration regression 또는 opt-in lifecycle 후 상태를 확인한다.

사용 예:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/tools/Register-PcvPostRebootVerification.ps1 -PhaseId phase21 -EvidenceDir '<evidence-dir>' -Profile ProductStatus -ContinuationProfiles HyperVNonIntegration -DryRun
```

Profile 내부 allowlist는 Hyper-V non-integration regression으로 제한한다. 실제 Hyper-V lifecycle 명령은 profile 기본값에 넣지 않는다. 실제 VM 생성/삭제는 별도 administrator opt-in command plan으로만 허용한다.

## Task Scheduler 계약

Task Scheduler 등록은 다음 contract를 따른다.

- task name은 `PureCVisorDesktopNode-PostRebootVerification-<short-id>` 형식이다.
- 기본 trigger는 사람 로그온이 필요 없는 `AtStartup`이다.
- 기본 principal은 `LocalSystem`이다.
- 기본 mode 이름은 `LocalSystemAtStartup`이다.
- `RunLevel`은 highest로 등록한다.
- task action은 `pwsh.exe -NoProfile -ExecutionPolicy Bypass -File <runner> -StateFile <state>` 형식이다.
- task는 runner 완료 후 unregister한다.
- unregister 실패는 evidence에 기록하되 command 결과를 덮어쓰지 않는다.

`CurrentUserAtLogOn` mode는 별도 opt-in으로만 사용한다. 이 mode는 사용자가 Windows에 로그인해야 실행되지만, 관리자 PowerShell을 다시 열 필요는 없다. 암호 저장, 자동 로그인, credential persistence는 이 설계에서 금지한다.

LocalSystem은 사용자 profile, mapped network drive, user certificate store, signing material, Codex credentials에 접근하지 않는다는 전제를 둔다. 따라서 기본 profile은 repo-local Pester, product status, diagnostics 같은 local machine 검증에만 사용한다.

## Safety와 Redaction

Evidence에는 다음 값을 기록하지 않는다.

- raw API token
- `Authorization` header 값
- `Bearer <token>` 원문
- protected token blob
- token hash
- PFX password, private key, signing secret
- service account password
- certificate private material

Redaction 규칙:

- `token`, `access_token`, `api_token`, `api_token_file`, `api_token_protected_file`, `protected_token`, `token_sha256`, `Authorization`, `password`, `secret` 계열 key는 `[REDACTED]`로 치환한다.
- 문자열 내부 `Bearer <token>`은 `Bearer [REDACTED]`로 치환한다.
- repo root, product root, data root, evidence root는 각각 `[REPO_ROOT]`, `[PRODUCT_ROOT]`, `[DATA_ROOT]`, `[EVIDENCE_ROOT]`로 치환한다.
- stdout/stderr 원문은 redaction 후 artifact로 저장한다.
- Markdown summary는 command line 전체 대신 command id와 sanitized summary를 우선 기록한다.

## 오류 처리

Runner는 command를 순차 실행한다.

- `required = true` command가 실패하면 전체 결과는 실패다.
- `allow_failure = true` command 실패는 evidence에 기록하되 전체 실패로 만들지 않는다.
- Timeout은 command 실패로 기록한다.
- 상태 파일이 없거나 repo boundary 검증이 실패하면 어떤 검증 명령도 실행하지 않는다.
- 완료 marker가 있으면 재실행하지 않고 already-completed result를 기록한다.
- task unregister 실패는 warning으로 기록한다.

## 구현 경계

후속 구현은 product runtime promotion을 의미하지 않는다.

이 기능은 Phase 20 signed release/MSI lifecycle evidence, Phase 21 Hyper-V lifecycle integration evidence, Phase 23 Windows operational evidence의 runbook 품질을 높이는 보조 도구다. Desktop Node는 계속 `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike` 상태다.

## 검증 기준

기본 구현 검증:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
git diff --check
```

Task Scheduler command builder를 추가하는 경우:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

Service status 또는 diagnostics profile을 바꾸는 경우:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
```

관리자 opt-in smoke:

1. 관리자 PowerShell에서 pre-reboot script를 `-Reboot:$false`로 실행해 task plan과 상태 파일만 생성한다.
2. Task action command를 injectable runner 또는 수동 invocation으로 검증한다.
3. 명시적 승인 후 `-Reboot`로 실제 reboot smoke를 실행한다.
4. Reboot 후 `post-reboot-result.json`과 `post-reboot-summary.md`를 확인한다.
5. Task가 unregister됐는지 확인한다.

실제 reboot, Task Scheduler 등록, MSI lifecycle, service mutation, Hyper-V lifecycle은 기본 검증에서 실행하지 않는다.

## 완료 기준

설계 완료 기준:

- post-reboot 자동화 범위가 검증 명령과 evidence 기록으로 제한되어 있다.
- Codex 자동 재개와 자동 로그인은 제외되어 있다.
- Task Scheduler elevated 1회성 task 계약이 명확하다.
- 상태 파일과 evidence 파일 contract가 명확하다.
- redaction/secrets 규칙이 Phase 15/16/23과 일관된다.
- administrator opt-in 경계가 유지된다.

구현 완료 기준:

- pre-reboot script와 post-reboot runner가 존재한다.
- command plan과 Task Scheduler 등록 계획이 Pester로 검증된다.
- runner가 상태 파일을 읽고 command 결과 JSON/Markdown evidence를 생성한다.
- task cleanup이 command plan 또는 injectable runner로 검증된다.
- 기본 Pester와 `git diff --check`가 통과한다.
- 실제 reboot smoke는 실행한 경우에만 해당 phase plan의 `완료 증거`에 기록한다.
