# PureCVisor Desktop Node Phase 20 Signed Release/MSI Lifecycle Evidence 설계

## 목적

Phase 20은 Phase 19에서 남은 GA 차단 gate 중 첫 번째 묶음인 signed release build evidence와 elevated MSI lifecycle smoke evidence를 수집하기 위한 실행 경계를 정의한다.

이 단계는 Desktop Node를 GA 제품 런타임으로 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지한다. Phase 20의 목표는 제품 후보 installer가 release signing 입력과 elevated Windows Installer lifecycle에서 어떤 증거를 충족해야 하는지 명확히 하고, 실제 관리자 opt-in 실행 결과를 같은 위치에 기록할 수 있게 만드는 것이다.

## 범위

Phase 20에 포함한다.

- `packaging/windows-desktop-node/installer/build.ps1 -SigningMode RequireSigned -SigningTrustModel <LocalTest|InternalEnterprise|PublicTrusted>` release build evidence 수집 절차
- SignTool, certificate input, timestamp URL, WinSW source artifact provenance 확인 절차
- elevated `msiexec /i`, repair, uninstall, `REMOVE_DATA=1` uninstall smoke 절차
- installer log, product wrapper status, runtime policy, Web Console root, ProgramData 보존/삭제 결과를 완료 증거로 남기는 기준
- 실패 시 cleanup, retry, evidence 보존 기준
- 문서와 root/installer 검증 suite 동기화

Phase 20에서 제외한다.

- signing secret, PFX password, private key, API token 값을 repo에 기록
- 실제 elevated `msiexec` 실행을 기본 검증으로 강제
- GitHub Actions release signing workflow 추가
- release artifact 배포 또는 publish
- Hyper-V VM lifecycle integration
- Desktop Node GA 제품 런타임 승격
- Linux `purecvisor-single`, Linux `purecvisorsd`, Single Edge UI/API 변경

## 실행 원칙

Phase 20은 evidence-first gate다.

- signing secret과 elevated PowerShell이 준비되지 않으면 실제 release build와 MSI lifecycle smoke를 실행하지 않는다.
- `RequireSigned` build evidence는 `signtool.exe`, certificate thumbprint 또는 external certificate path, timestamp URL 입력을 요구한다.
- certificate private key, PFX password, raw API token, protected token blob은 문서, provenance, diagnostic bundle, commit에 남기지 않는다.
- elevated smoke는 host mutation이다. 실행 전 관리자 opt-in을 명시하고, 실행 후 service/listener/data root 상태를 정리한다.
- 실패 로그도 증거다. 실패한 installer log와 product wrapper JSON은 redaction 후 plan의 완료 증거에 요약한다.

## Evidence 기준

### Signed release build

충족으로 인정하려면 다음 증거가 필요하다.

- build command와 실행 환경 요약
- generated MSI path와 SHA-256
- provenance manifest path와 SHA-256
- provenance `signing_mode = RequireSigned`
- SignTool 실행 exit code
- timestamp URL 사용 여부
- certificate input 방식이 thumbprint인지 external certificate path인지에 대한 비밀 없는 요약
- WinSW source artifact path, SHA-256, provenance 확인 결과

### Elevated MSI lifecycle

충족으로 인정하려면 같은 MSI artifact로 다음 순서를 검증한다.

1. `msiexec /i`
2. product wrapper `Status`
3. protected token을 사용한 `GET /api/v1/runtime/policy`
4. loopback Web Console root HTTP 200
5. `msiexec /i ... REINSTALL=ALL REINSTALLMODE=vomus REBOOT=ReallySuppress MSIRESTARTMANAGERCONTROL=Disable` repair
6. repair 후 service/runtime/data 보존 확인
7. 기본 `msiexec /x` uninstall
8. 기본 uninstall 후 ProgramData 보존과 service/listener 제거 확인
9. 재설치
10. `msiexec /x REMOVE_DATA=1` uninstall
11. protected token, legacy raw token, job store, event log, install log, diagnostics 제거 확인

## 실패 처리

- install 실패 시 service 상태, installer log, `%ProgramData%\PureCVisor\desktop-node\install.jsonl`, product wrapper JSON을 확인한다.
- repair 실패 시 설치 상태를 보존하고 uninstall cleanup을 먼저 시도한다.
- uninstall 실패 시 WinSW service 상태와 process lock을 확인하고, 수동 삭제 전에 evidence를 기록한다.
- `REMOVE_DATA=1` 실패 시 ProgramData의 남은 파일 목록을 기록하되 token 값과 protected token blob은 기록하지 않는다.

## 문서화 기준

실행 전에는 Phase 20 plan의 checklist만 추가한다. 실행 후에는 같은 plan의 `완료 증거` 섹션에 결과를 추가한다.

완료 증거에는 pass count를 high-level docs에 복제하지 않는다. Pass count와 command output 요약은 Phase 20 plan에만 둔다.

## 검증 기준

문서/runbook 변경의 기본 검증은 다음이다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

installer script나 MSI source를 바꾸는 경우 다음을 추가한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

실제 signed release build와 elevated MSI lifecycle smoke는 관리자 opt-in 완료 후 별도 evidence로 기록한다.

## 완료 기준

Phase 20 문서 시작 작업은 다음을 만족하면 완료다.

- Phase 20 spec과 plan이 존재한다.
- roadmap, developer index, follow-up queue가 Phase 20 진입점을 가리킨다.
- 실제 host mutation은 여전히 administrator opt-in gate로 분리되어 있다.
- root documentation suite와 `git diff --check`가 통과한다.

Phase 20 evidence gate 자체는 다음을 만족해야 닫힌다.

- signed release build evidence가 Phase 20 plan에 기록된다.
- elevated MSI install/repair/uninstall/`REMOVE_DATA=1` smoke evidence가 Phase 20 plan에 기록된다.
- 증거에 secret, private key, raw token, protected token blob이 포함되지 않는다.
