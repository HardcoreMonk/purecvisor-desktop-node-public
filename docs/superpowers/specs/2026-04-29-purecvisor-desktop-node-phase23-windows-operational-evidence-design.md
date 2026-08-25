# PureCVisor Desktop Node Phase 23 Windows Operational Evidence 설계

## 목적

Phase 23은 Phase 19 이후 남은 GA 차단 gate 중 Windows 장기 운영 증거를 수집하기 위한 기준을 정의한다.

이 단계는 Desktop Node를 GA 제품 런타임으로 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지한다. 목표는 Windows host에서 장시간 service 운용, 로그 보존, service failure recovery, diagnostic bundle 재현성을 어떤 증거로 인정할지 명확히 하고, Event Log writer/provider 전환 여부를 증거 기반으로 판단할 수 있게 만드는 것이다.

## 범위

Phase 23에 포함한다.

- JSONL first 장기 rotation/retention 증거 기준
- Windows Event Log writer/provider 전환 여부 판단 기준
- service failure action과 recovery smoke 증거 기준
- service host log retention 증거 기준
- diagnostic bundle 증거 기준
- LAN listener + reverse proxy/TLS preview 증거 기준
- 로그와 diagnostic bundle의 redaction/secrets 규칙
- non-admin 검증과 administrator opt-in smoke의 분리

Phase 23에서 제외한다.

- 기본 install, repair, diagnostics 경로의 Windows Event Log source 자동 등록
- 기본 install 또는 diagnostics 경로의 Windows Firewall rule 자동 변경
- service 설치, 시작, 중지, 실패 유도 같은 host mutation을 기본 검증으로 실행
- 내장 TLS certificate lifecycle 구현
- Linux `purecvisor-single`, Linux `purecvisorsd`, Single Edge UI/API 변경
- Desktop Node GA 제품 런타임 승격

## 결정

```text
DESKTOP_NODE_PHASE23_OPERATIONAL_EVIDENCE_DECISION: jsonl-first-long-run-evidence-with-eventlog-transition-deferred
```

Phase 23은 Phase 16 결정을 유지한다. `events.jsonl`, `install.jsonl`, service host logs, diagnostic bundle manifest를 1차 운영 증거로 둔다. Phase 13 당시에는 WinSW logs였고, 2026-05-01 replacement slice 이후 기본 service host logs는 `DesktopNode.Host.exe` 경계다.

Windows Event Log writer/provider 전환은 Phase 23의 기본 구현 대상이 아니다. 전환은 다음 조건이 모두 충족될 때 별도 phase 또는 ADR로 다룬다.

1. Event Log source/provider lifecycle의 owner가 MSI custom action, product wrapper action, 수동 관리자 runbook 중 하나로 확정된다.
2. 등록, repair, unregister, uninstall, `REMOVE_DATA=1` semantics가 Windows Event Log 보존 정책과 충돌하지 않는다.
3. non-admin Pester와 administrator opt-in smoke가 같은 contract를 검증할 수 있다.
4. diagnostic bundle이 Event Log export를 redaction된 artifact로 포함할 수 있다.
5. source 등록 실패가 install/service start 실패와 어떻게 연결되는지 정책이 정해진다.

## Operational Evidence 기준

### JSONL 장기 rotation/retention

충족으로 인정하려면 다음 증거가 필요하다.

- `events.jsonl`과 `install.jsonl`의 rotation threshold와 retained file count
- rotation 전후 파일 목록
- 보존 수 초과 파일 제거 여부
- rotation 결과 object 또는 diagnostic artifact
- 장시간 service run 중 로그 append가 중단되지 않았다는 증거
- redaction 후에도 운영 event 순서를 추적할 수 있다는 증거

기본 기준은 Phase 16 정책을 따른다.

- `events.jsonl`: 5 MiB, 보존 파일 5개
- `install.jsonl`: 5 MiB, 보존 파일 5개
- service host logs: 10 MiB, 보존 파일 10개

### Service failure action과 recovery

충족으로 인정하려면 administrator opt-in 환경에서 다음 증거가 필요하다.

- 설치된 service의 failure action configuration
- service stop, crash, 또는 controlled failure 유도 방식
- recovery attempt 후 service 상태
- runtime policy 또는 loopback health check 결과
- failure/recovery 시점의 `install.jsonl`, `events.jsonl`, service host logs
- recovery 이후 diagnostic bundle 생성 결과

실패 유도는 기본 검증에서 실행하지 않는다. service mutation은 명시적 관리자 opt-in에서만 실행한다.

### Service log retention

충족으로 인정하려면 다음 증거가 필요하다.

- `%ProgramData%\PureCVisor\desktop-node\service-logs` 하위 로그 목록
- service host stdout/stderr 로그의 rotation 전후 상태
- retained file count 초과분 제거 여부
- diagnostic bundle의 service log artifact 포함 여부
- raw token, protected token blob, Authorization header가 service log artifact에 남지 않았다는 확인

### Diagnostic bundle

충족으로 인정하려면 다음 artifact가 redaction된 상태로 포함되어야 한다.

- `diagnostics-manifest.json`
- `summary.json`
- `service-status-redacted.json`
- `service-host-status-redacted.json` 또는 historical `winsw-status-redacted.json`
- `runtime-policy-redacted.json`
- `service-host-metadata-redacted.json` 또는 historical `winsw-metadata-redacted.json`
- `events-redacted.jsonl`
- `install-redacted.jsonl`
- `service-log-*` 또는 historical `winsw-log-*`
- `operational-evidence-redacted.json`
- 필요 시 recovery smoke summary artifact

Bundle manifest는 host absolute path 대신 bundle 내부 artifact file name만 참조한다.

`operational-evidence-redacted.json`은 non-admin diagnostics path에서 생성되는 운영 증거 요약이다. 이 artifact는 SCM failure action recovery policy, service log retention 기준과 관찰된 service log artifact 이름, Windows Event Log deferred policy, service/Event Log/firewall/elevated MSI mutation 미수행 여부를 기록한다. 실제 service failure/recovery smoke를 대체하지는 않으며, 관리자 opt-in smoke 전후의 비교 기준으로만 사용한다.

## LAN/TLS Preview 기준

LAN listener와 reverse proxy/TLS smoke는 administrator opt-in preview로 유지한다.

- 기본 plan은 loopback-only다.
- `-AllowLan`은 explicit opt-in이어야 한다.
- non-loopback static Web Console과 API는 bearer token policy를 유지한다.
- 제품 wrapper는 TLS endpoint를 직접 제공하지 않는다.
- LAN preview는 reverse proxy 또는 외부 TLS terminator를 전제로 한다.
- Windows Firewall rule은 기본으로 생성하거나 변경하지 않는다.
- Firewall mutation은 명시적 opt-in product action 또는 수동 관리자 명령에서만 허용한다.

## Windows Event Log 정책

기본 install, repair, diagnostics, non-admin test path는 Event Log source를 등록하지 않는다.

허용되는 범위:

- Event Log registration plan object 생성
- 관리자 opt-in runbook에 source 등록/해제 절차 문서화
- 실제 등록 전후 상태와 cleanup 결과를 Phase 23 plan에 증거로 기록

금지되는 범위:

- 기본 install 중 `New-EventLog` 실행
- 기본 repair 중 source 자동 복구
- 기본 diagnostics 중 Event Log source 등록
- 기본 uninstall 중 machine-wide Event Log source 삭제

## Redaction과 Secrets 규칙

문서, 로그 요약, diagnostic bundle, 완료 증거에는 다음 값을 남기지 않는다.

- raw API token
- `Authorization` header 값
- `Bearer <token>` 원문
- protected token blob
- token hash
- PFX password, private key, signing secret
- service account password
- certificate private material

Redaction 기준:

- `token`, `access_token`, `api_token`, `api_token_file`, `api_token_protected_file`, `protected_token`, `token_sha256`, `Authorization`, `password`, `secret` 계열 key는 `[REDACTED]`다.
- 문자열 내부 `Bearer <token>`은 `Bearer [REDACTED]`로 바꾼다.
- source root, product root, data root는 각각 `[SOURCE_ROOT]`, `[PRODUCT_ROOT]`, `[DATA_ROOT]`로 바꾼다.
- JSON escape된 path에도 같은 placeholder를 적용한다.
- evidence에는 필요한 경우 artifact SHA-256과 파일명만 남긴다.

## 검증 기준

문서/runbook 변경의 기본 검증은 다음이다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
git diff --check
```

diagnostics policy, bundle, rotation contract를 바꾸는 경우 다음을 추가한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1' -Output Detailed"
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
```

service helper docs 또는 service command builder를 바꾸는 경우 다음을 추가한다.

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
```

administrator opt-in smoke는 별도 증거로만 기록한다. 기본 검증에서 service mutation, Event Log registration, firewall mutation, elevated MSI action은 실행하지 않는다.

## 완료 기준

Phase 23 문서 시작 작업은 다음을 만족하면 완료다.

- Phase 23 spec과 plan/runbook이 존재한다.
- JSONL first 운영 증거와 Event Log 전환 판단 기준이 명확하다.
- service failure/recovery, service log retention, diagnostic bundle evidence 기준이 명확하다.
- LAN/TLS preview가 administrator opt-in으로 유지된다.
- 기본 Event Log source registration과 firewall mutation이 금지되어 있다.
- redaction/secrets 규칙이 Phase 15/16 contract와 일관된다.
- 검증 명령은 실제 실행하지 않은 경우 pending으로 남긴다.

Phase 23 evidence gate 자체는 다음을 만족해야 닫힌다.

- 장시간 service run evidence가 Phase 23 plan에 기록된다.
- JSONL/service log rotation-retention evidence가 기록된다.
- service failure action/recovery smoke evidence가 기록된다.
- diagnostic bundle evidence가 redaction 검증과 함께 기록된다.
- Event Log writer/provider 전환 여부가 evidence 기준으로 판정된다.
