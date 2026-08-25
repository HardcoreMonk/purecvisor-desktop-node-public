# PureCVisor Desktop Node Phase 16 Event Log와 Long-Term Diagnostics 설계

## 목적

Phase 16은 Desktop Node 제품 wrapper의 운영 진단 기준을 확정한다.

Phase 5에서 Local API는 `-EventLogPath`를 통해 JSONL 이벤트 로그를 남기기 시작했고, Phase 12/13/14/15에서 product wrapper는 `CollectDiagnostics` bundle, service log 수집, runtime policy redaction, protected token redaction을 단계적으로 추가했다. 하지만 장기 운영 기준에서는 아직 다음 항목이 명확하지 않았다.

- Windows Event Log provider/source를 누가 언제 등록하는가.
- Phase 5 JSONL event log를 유지할지, Event Log로 대체할지 결정한다.
- `events.jsonl`, `install.jsonl`, service host logs의 rotation/retention 기준을 정한다.
- diagnostic bundle schema와 redaction 규칙을 versioned contract로 고정한다.
- failed install, failed health check, failed rollback, service lifecycle artifact가 같은 bundle에서 재현 가능한지 보장한다.

Phase 16도 Desktop Node 전체를 제품 런타임으로 GA 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지하고, 변경 범위는 `packaging/windows-desktop-node/**` 중심으로 둔다. 필요 시 기존 spike README와 검증 문서만 현재 상태에 맞춘다.

## 결정

```text
DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred
```

Phase 16은 JSONL 파일 로그를 1차 제품 운영 로그로 유지한다. Windows Event Log provider/source 등록은 기본 install, repair, diagnostics, test 경로에 넣지 않고 관리자 권한 opt-in 등록 계획과 smoke 절차로 분리한다.

직접 효과:

- `events.jsonl`은 Local API listener/firewall/runtime event의 1차 로그다.
- `install.jsonl`은 product wrapper action lifecycle의 1차 로그다.
- Service host logs는 `%ProgramData%\PureCVisor\desktop-node\service-logs` 아래 제품 service log로 유지한다. Phase 13 당시 WinSW logs였고, 2026-05-01 replacement slice 이후 기본 host는 `DesktopNode.Host.exe`다.
- product wrapper는 versioned diagnostics policy를 plan과 manifest에 포함한다.
- diagnostic bundle은 schema version, redaction version, log retention policy, source artifact 목록을 manifest로 남긴다.
- bundle 내부 artifact는 token 값, protected token blob/hash, Authorization header, product root/data root/source root 같은 host-sensitive path를 redaction한다.
- Event Log provider/source는 `PureCVisor Desktop Node` source를 대상으로 한 등록 계획만 생성한다. 실제 등록은 관리자 opt-in smoke에서만 수행한다.

## Windows Event Log를 보류하는 이유

Windows Event Log source/provider 등록은 관리자 권한과 설치 프로그램 책임이 강하게 묶인다. MSI custom action, product wrapper action, 수동 관리자 명령 중 어느 계층이 source lifecycle을 소유할지 확정하지 않은 상태에서 기본 경로에 등록을 넣으면 다음 문제가 생긴다.

1. non-elevated Pester와 dry-run smoke에서 같은 경로를 검증하기 어렵다.
2. source 삭제와 repair semantics가 Windows Event Log 보존 정책과 충돌할 수 있다.
3. service 계정과 installer 실행 계정의 기록 책임이 섞인다.
4. Single Edge 공개 릴리스 경계와 무관한 Windows machine-wide mutation이 기본 검증에 들어온다.

JSONL first 결정은 Event Log 통합을 배제하지 않는다. Phase 16은 등록 계획, source 이름, redaction, bundle schema를 먼저 고정하고, 실제 Event Log writer/provider 전환은 Phase 17 이후 LAN/security 또는 제품 승격 재판정 단계에서 관리자 integration evidence와 함께 다룬다.

## Diagnostics Policy v1

product plan과 manifest는 diagnostics policy v1을 포함한다.

```json
{
  "schema_version": 1,
  "mode": "jsonl-primary-eventlog-deferred",
  "diagnostic_bundle_schema_version": 1,
  "redaction_version": 1,
  "event_log": {
    "mode": "jsonl",
    "path": "%ProgramData%\\PureCVisor\\desktop-node\\events.jsonl",
    "max_file_bytes": 5242880,
    "retained_files": 5
  },
  "install_log": {
    "mode": "jsonl",
    "path": "%ProgramData%\\PureCVisor\\desktop-node\\install.jsonl",
    "max_file_bytes": 5242880,
    "retained_files": 5
  },
  "service_logs": {
    "path": "%ProgramData%\\PureCVisor\\desktop-node\\service-logs",
    "max_file_bytes": 10485760,
    "retained_files": 10
  },
  "windows_event_log": {
    "enabled_by_default": false,
    "source": "PureCVisor Desktop Node",
    "log_name": "Application",
    "registration_owner": "admin-opt-in"
  }
}
```

경로 값은 실제 plan에서는 resolved absolute path를 사용하고, diagnostic bundle에서는 redaction된 값으로 출력한다.

## Rotation과 Retention

Phase 16 rotation은 파일 기반 정책으로 제한한다.

- rotation 대상은 `events.jsonl`, `install.jsonl`, service host `*.log`, `*.out`, `*.err` 파일이다.
- 기본 threshold는 event/install log 5 MiB, service log 10 MiB다.
- 기본 보존 수는 event/install log 5개, service log 10개다.
- rotation은 `file`, `file.1`, `file.2` 순으로 밀어내고, 보존 수를 초과한 파일은 제거한다.
- rotation 실행 결과는 diagnostic-friendly object로 반환하고, raw token이나 host path를 반환하지 않는다.

Local API의 JSONL writer는 기존 계약을 유지한다. Phase 16 product wrapper가 data root 운영 정책과 diagnostic bundle schema를 소유한다.

## Diagnostic Bundle Schema v1

bundle은 기존 artifact를 유지하면서 다음 manifest를 추가한다.

```json
{
  "schema_version": 1,
  "generated_at": "2026-04-28T00:00:00.0000000Z",
  "decision": "DESKTOP_NODE_PHASE16_DIAGNOSTICS_DECISION: jsonl-first-versioned-diagnostics-with-eventlog-deferred",
  "redaction_version": 1,
  "policy": {},
  "sources": [
    {
      "name": "summary",
      "artifact": "summary.json",
      "required": true,
      "redacted": true
    }
  ]
}
```

필수 artifact:

- `diagnostics-manifest.json`
- `summary.json`
- `service-status-redacted.json`
- `service-host-status-redacted.json` 또는 historical `winsw-status-redacted.json`
- `runtime-policy-redacted.json`
- `service-host-metadata-redacted.json` 또는 historical `winsw-metadata-redacted.json`

조건부 artifact:

- `product-manifest-redacted.json`
- `events-redacted.jsonl`
- `install-redacted.jsonl`
- `jobs-redacted.json`
- `winsw-xml-redacted.xml`
- `service-log-*` 또는 historical `winsw-log-*`

## 실패 재현 기준

운영자는 product root와 data root만으로 다음 상태를 재현할 수 있어야 한다.

1. service lifecycle: SCM/service host status, service host logs, staged executable hash.
2. Local API runtime: runtime policy response 또는 unavailable error.
3. install/repair/uninstall action: install log와 product manifest.
4. rollback failure: previous product root path, product root path, service status, install log.
5. health check failure: runtime policy unavailable error와 auth mode metadata.

Phase 16은 실패를 새 telemetry backend로 전송하지 않는다. 모든 evidence는 local bundle에 남긴다.

## Redaction 규칙

Phase 15 redaction을 유지하고 다음 규칙을 diagnostics contract로 고정한다.

- `token`, `access_token`, `api_token_file`, `api_token_protected_file`, `protected_token`, `token_sha256`, `Authorization` 값은 `[REDACTED]`다.
- 문자열 내부 `Bearer <token>`은 `Bearer [REDACTED]`로 바꾼다.
- source root, product root, data root는 각각 `[SOURCE_ROOT]`, `[PRODUCT_ROOT]`, `[DATA_ROOT]`로 바꾼다.
- path가 JSON escape된 문자열로 등장해도 같은 placeholder로 바꾼다.
- bundle manifest의 artifact path도 host absolute path 대신 artifact file name을 사용한다.

## 검증 기준

기본 검증:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

관리자 smoke는 조건부 opt-in이다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
```

Windows Event Log source 실제 등록은 Phase 16 기본 완료 조건이 아니다. 등록 계획 출력과 문서화까지만 기본 검증으로 다룬다.
