# PureCVisor Desktop Node Phase 18 Update/Rollback/Config Migration 설계

## 목적

Phase 18은 Desktop Node 제품 후보 wrapper의 update, rollback, config migration 기준을 제품 수준으로 고정한다.

Phase 12/13은 Service-first product wrapper와 WinSW service host를 만들었고, Phase 14는 WiX MSI-first installer source/build와 repair/uninstall UX를 추가했다. Phase 15는 DPAPI LocalMachine protected token file을 기본 token source로 전환했고, Phase 16/17은 diagnostics와 LAN security policy를 manifest/runtime policy/diagnostic bundle에 고정했다.

남은 문제는 설치된 제품 루트와 데이터 루트가 여러 버전을 거칠 때 어떤 항목을 버전의 단일 진실로 볼지, update 실패가 어디까지 자동 rollback되어야 하는지, config와 job store migration 실패가 silent data loss로 이어지지 않게 할지를 정하는 것이다.

Phase 18도 Desktop Node 전체를 GA 제품 런타임으로 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지한다. 구현 범위는 후속 구현 시에도 `packaging/windows-desktop-node/**`와 필요한 `spikes/purecvisor-desktop-node/**` 검증 문서로 제한하고, Linux `purecvisorsd` 또는 Single Edge 공개 UI/API 표면과 연결하지 않는다.

## 결정

```text
DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration
```

Phase 18의 결정은 다음과 같다.

- 설치된 `product-manifest.json`을 설치 버전과 product root 상태의 단일 진실로 둔다.
- Phase 18 기본 update는 네트워크 다운로드형 updater가 아니라 local payload/source 기반 product wrapper action이다.
- update는 현재 product root를 단일 rollback slot인 `DesktopNode.previous`로 보존한 뒤 새 product root를 staging/replace 방식으로 적용한다.
- rollback slot 보존 수는 기본 1개다. 다중 이전 버전 보존, delta update, background update는 후속 Phase 범위다.
- config migration은 service start 전에 dry-run과 validation을 통과해야 한다.
- config migration 실패 시 새 service를 시작하지 않고 이전 product root로 rollback한다.
- job store는 기본적으로 파괴적 schema rewrite를 하지 않는다. schema mismatch가 있으면 read-only compatibility 또는 migration 보류 상태로 진단한다.
- installer, wrapper, payload provenance는 같은 version/signing policy를 따라야 하며, unsigned dev artifact와 signed release artifact의 경계를 manifest와 provenance에서 구분한다.
- update/rollback/migration 결과는 `install.jsonl`, diagnostic bundle, redacted manifest artifact에 남긴다.

## 제품 Manifest Contract

Phase 18은 top-level `product-manifest.json`의 현재 `schema_version = 1`을 유지한다. Backward-incompatible manifest 구조 변경이 생기기 전까지 top-level schema를 올리지 않는다. 대신 update 정책은 manifest 안에 별도 `update` object로 추가한다.

```json
{
  "schema_version": 1,
  "product": "PureCVisor Desktop Node",
  "version": "0.18.0-dev",
  "update": {
    "schema_version": 1,
    "decision": "DESKTOP_NODE_PHASE18_UPDATE_DECISION: manifest-first-safe-update-with-validated-config-migration",
    "version_source": "product-wrapper-version-parameter",
    "installed_manifest_is_source_of_truth": true,
    "payload_version_must_match_manifest": true,
    "rollback": {
      "previous_root": "C:\\Program Files\\PureCVisor\\DesktopNode.previous",
      "failed_root_suffix": ".failed",
      "retained_previous_roots": 1,
      "rollback_requires_health_check": true
    },
    "config_migration": {
      "mode": "validate-before-service-start",
      "dry_run_required": true,
      "block_service_start_on_failure": true,
      "data_backup_required_before_mutation": true
    },
    "job_store": {
      "destructive_rewrite_by_default": false,
      "schema_mismatch_mode": "read-only-or-blocked-with-diagnostics"
    },
    "provenance": {
      "signed_release_required_for_release_channel": true,
      "unsigned_dev_allowed_for_dev_channel": true
    }
  }
}
```

`version`은 product wrapper `-Version` 입력과 installer payload manifest가 공유하는 제품 버전이다. 설치 후에는 installed `product-manifest.json`의 `version`이 현재 product root 버전의 단일 진실이다. Service host metadata, staged executable hash, diagnostics policy, LAN security policy는 manifest에 남되, 런타임에서 버전을 추론하기 위한 별도 파일 이름 규칙을 만들지 않는다. Phase 13 WinSW XML metadata는 historical compatibility artifact로만 남는다.

## Safe Update Flow

Phase 18 update flow는 local payload/source를 입력으로 받는 product wrapper action을 기준으로 한다.

1. update lock을 획득한다.
2. 설치된 `product-manifest.json`을 읽고 current version, schema, product root 경계를 검증한다.
3. 새 payload/source manifest를 읽고 target version, schema, signing/provenance stance를 검증한다.
4. service stop을 실행하고 stopped/missing 상태를 기다린다.
5. 현재 product root를 `DesktopNode.previous`로 이동한다. 기존 previous root가 있으면 retention policy에 따라 제거 또는 실패 처리한다.
6. 새 payload를 staging root에 복사하고 manifest를 작성한다.
7. config migration plan을 dry-run으로 생성하고 validation한다.
8. validation이 통과하면 staging root를 product root로 승격한다.
9. service를 시작하고 protected-token bearer health check로 `/api/v1/runtime/policy`를 확인한다.
10. health check가 통과하면 update success를 기록한다.
11. 어떤 단계가 실패하든 실패 원인, from/to version, service status, migration status를 diagnostics-friendly object로 남긴다.

Rollback 조건:

- payload validation 실패: product root를 바꾸지 않는다.
- service stop 실패: product root를 바꾸지 않는다.
- current root 보존 실패: update를 중단한다.
- config migration dry-run/validation 실패: 새 service를 시작하지 않고 previous root를 복원한다.
- service start 또는 health check 실패: previous root를 복원하고 rollback health check를 실행한다.

Phase 18은 완전한 Windows filesystem transaction을 제공한다고 주장하지 않는다. 목표는 실패 위치별로 product root가 어떤 상태인지 명확하고, 이전 정상 버전으로 돌아갈 수 있는 경로를 테스트로 고정하는 것이다.

## Rollback Policy

기본 rollback slot은 하나다.

- current root: `C:\Program Files\PureCVisor\DesktopNode`
- previous root: `C:\Program Files\PureCVisor\DesktopNode.previous`
- failed root: `C:\Program Files\PureCVisor\DesktopNode.failed`

기존 rollback semantics는 유지한다.

- `%ProgramData%`의 protected token file, legacy token file, job store, event log, install log는 기본 rollback에서 삭제하지 않는다.
- Rollback은 service stop 이후 product root를 failed root로 격리하고 previous root를 current root로 복원한다.
- Rollback 이후 service start와 runtime policy health check를 수행한다.
- Rollback health check 실패는 성공 rollback으로 기록하지 않는다.

새로운 정책:

- `DesktopNode.previous`가 없으면 rollback은 시작 전에 실패해야 한다.
- rollback 대상 previous manifest가 없거나 product 이름/version/schema가 유효하지 않으면 restore를 막는다.
- failed root는 diagnostics 수집 전까지 보존한다. cleanup은 명시적 product action 또는 후속 retention cleanup에서만 수행한다.
- 이전 버전이 더 낮은 config schema를 기대하더라도 data root를 자동 downgrade하지 않는다.

## Config Migration Policy

Config migration은 product root와 data root 사이의 계약을 검증하는 단계다.

Phase 18에서 migration 대상은 다음으로 제한한다.

- product wrapper가 소유한 manifest/config metadata
- diagnostics policy metadata
- LAN security policy metadata
- protected token source path와 legacy raw token compatibility metadata
- job store schema compatibility decision

Migration plan은 다음 형태의 object로 표현한다.

```json
{
  "schema_version": 1,
  "from_version": "0.17.0-dev",
  "to_version": "0.18.0-dev",
  "dry_run": true,
  "service_start_allowed": true,
  "steps": [
    {
      "name": "validate-protected-token-source",
      "mutation": false,
      "required": true,
      "status": "planned"
    }
  ],
  "backups": [
    {
      "source": "jobs.json",
      "artifact": "jobs.json.pre-0.18.0-dev.bak",
      "required_before_mutation": true
    }
  ]
}
```

규칙:

- 모든 migration은 dry-run 결과가 먼저 있어야 한다.
- dry-run에서 `service_start_allowed = false`면 service를 시작하지 않는다.
- data root 파일을 변경하는 step은 변경 전 backup artifact 계획을 가져야 한다.
- migration step은 idempotent해야 한다.
- migration 실패는 update 실패로 취급하며, update flow는 previous product root rollback을 시도한다.
- rollback은 data root를 자동 downgrade하지 않는다. data downgrade가 필요한 breaking change는 Phase 18 기본 범위 밖이다.

## Job Store Policy

Phase 18은 job store를 파괴적으로 rewrite하지 않는다.

기준:

- 기존 `jobs.json`은 product update 중 보존한다.
- job store schema가 현재 runtime과 호환되면 그대로 읽는다.
- schema mismatch가 감지되면 queued/running job mutation을 막고 diagnostics에 mismatch를 남긴다.
- completed/failed job history는 가능하면 read-only로 표시한다.
- 파괴적 migration이 필요한 schema 변경은 별도 Phase 또는 explicit admin action이 필요하다.

이 결정은 update 안정성을 우선한다. VM lifecycle job 이력이 사라지거나 queued job이 잘못 재실행되는 것보다, schema mismatch를 명시적으로 막고 운영자가 diagnostics를 보는 편이 안전하다.

## Signing과 Provenance 경계

Phase 18은 updater download/signature verification 구현을 포함하지 않는다. 다만 update policy는 installer와 같은 signing/provenance stance를 따른다.

- dev channel: unsigned dev artifact를 허용한다. Manifest와 provenance에는 dev/unsigned 상태가 명확히 남아야 한다.
- release channel: signed artifact와 provenance가 필요하다.
- product wrapper는 release channel update에서 unsigned payload를 제품 성공으로 기록하지 않는다.
- signing secret, certificate private key, API token 값은 repo, product manifest, diagnostic bundle에 기록하지 않는다.

## Diagnostics

Diagnostic bundle은 update/rollback/migration 상태를 redaction된 artifact로 남겨야 한다.

추가 artifact 후보:

- `update-policy-redacted.json`
- `update-attempt-redacted.json`
- `migration-plan-redacted.json`
- `rollback-state-redacted.json`

필수 기록:

- from version
- to version
- product manifest schema
- update decision marker
- update step status
- migration dry-run result
- rollback attempted 여부
- rollback health check result
- previous root 존재 여부는 redaction된 placeholder로 표시
- failed root 보존 여부

Raw token, protected token blob/hash, Authorization header, full product/data/source root path는 Phase 15/16 redaction 규칙대로 남기지 않는다.

## 검증 기준

Phase 18 구현 시 기본 검증은 다음을 포함해야 한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

문서만 변경하는 설계/계획 PR에서는 root boundary/documentation sync suite와 `git diff --check`를 기본으로 실행한다.

관리자 smoke는 조건부 opt-in이다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Update -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Rollback -WhatIf
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
```

실제 update, rollback, service start, protected token ACL inspection, MSI repair/update smoke는 관리자 권한과 명시적 product root/data root 변경 승인이 있을 때만 실행한다.

## 제외 범위

Phase 18에서 제외한다.

- 네트워크 다운로드형 updater
- delta update
- background update scheduler
- 다중 rollback slot
- data root 자동 downgrade
- job store 파괴적 rewrite
- Windows Credential Manager 전환
- Event Log writer/provider 전환
- 내장 TLS certificate lifecycle
- Desktop Node GA 제품 런타임 승격

## 완료 기준

Phase 18은 후속 구현에서 다음을 만족하면 완료다.

- product manifest가 update policy v1을 포함한다.
- product wrapper `Plan`/manifest/dry-run output에서 version source와 rollback policy를 확인할 수 있다.
- update path가 current manifest, target manifest, provenance stance를 검증한다.
- update 실패 지점별 rollback behavior가 Pester로 고정된다.
- config migration dry-run/validation 실패가 service start를 막는다.
- job store schema mismatch가 silent rewrite 대신 read-only 또는 blocked 상태로 진단된다.
- diagnostic bundle이 update policy, migration plan, rollback state를 redaction된 artifact로 포함한다.
- 실제 mutating update/rollback smoke는 관리자 opt-in으로 분리된다.
