# PureCVisor Desktop Node Phase 15 Secure Token Storage 설계

## 목적

Phase 15는 Desktop Node의 장기 bearer token 저장 방식을 plain text token file 중심에서 Windows DPAPI 기반 protected token file로 승격한다.

Phase 7/8/13/14는 `-ApiTokenFile`과 ACL hardening으로 service command line의 raw token 노출을 제거했다. 하지만 `%ProgramData%\PureCVisor\desktop-node\api-token.txt` 자체는 plain text이고, 당시 product wrapper의 기본 service XML과 health check도 이 raw file을 활성 token source로 사용했다. Phase 15는 이 기본값을 DPAPI LocalMachine protected blob으로 바꾸고, 기존 raw token file은 명시적 호환 경로로만 유지한다. 2026-05-01 .NET Host replacement 이후에도 제품 service command line은 protected token file 경로만 전달한다.

Phase 15도 Desktop Node 전체를 제품 런타임으로 GA 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지하고, 변경 범위는 `spikes/purecvisor-desktop-node/**`와 `packaging/windows-desktop-node/**`에 격리한다.

## 결정

```text
DESKTOP_NODE_PHASE15_TOKEN_STORAGE_DECISION: dpapi-local-machine-protected-file-first
```

Phase 15는 Windows Credential Manager가 아니라 DPAPI `LocalMachine` scope protected token file을 1차 제품 경로로 채택한다.

직접 효과:

- 제품 wrapper와 service host command line은 기본적으로 protected token file 경로를 사용한다. Historical WinSW XML은 `-ApiTokenProtectedFile`을 사용했고, 현재 `DesktopNode.Host.exe listen`은 `--api-token-protected-file`을 사용한다.
- protected token file은 `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`에 둔다.
- protected token file은 DPAPI LocalMachine으로 보호된 bearer token blob, schema version, storage marker, 생성 시각, token hash metadata만 저장한다.
- Local API는 `-ApiToken`, `-ApiTokenFile`, `-ApiTokenProtectedFile` 중 정확히 하나만 허용한다.
- CLI는 기존 `--token`/`--token-file`과 별도로 `--protected-token-file`을 지원한다.
- 기존 `-ApiTokenFile`과 `--token-file`은 개발, 이전 버전, 명시적 operator override용 호환 경로로 유지한다.
- diagnostics, install log, event log, service status, runtime policy artifact는 raw token과 protected blob/hash 값을 redaction한다.

## Credential Manager를 보류하는 이유

Windows Credential Manager는 interactive user profile과 service account 경계가 제품 요구와 어긋날 수 있다. Desktop Node service 기본 계정은 `LocalSystem`이고, installer/custom action은 elevated machine context에서 실행된다. Credential Manager를 기본 저장소로 채택하면 interactive user가 등록한 credential과 service가 읽는 credential의 소유 경계가 모호해진다.

DPAPI `LocalMachine` protected file은 같은 머신의 service와 elevated product wrapper가 같은 저장소를 읽을 수 있고, 기존 ProgramData file/ACL/RemoveData 모델과도 맞다. 이 결정은 LAN multi-user 인증, Windows Credential Manager, per-user token profile을 배제하지 않으며 Phase 17 이후 별도 보안 정책으로 재평가할 수 있다.

## 저장소 계약

기본 protected token file 경로:

```text
%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json
```

JSON schema:

```json
{
  "schema_version": 1,
  "storage": "dpapi-local-machine",
  "scope": "LocalMachine",
  "created_at": "2026-04-28T00:00:00.0000000Z",
  "token_sha256": "<lowercase sha256 hex>",
  "protected_token": "<base64 DPAPI blob>"
}
```

규칙:

1. `protected_token`은 UTF-8 bearer token bytes를 DPAPI `LocalMachine` scope로 보호한 결과다.
2. entropy 값은 Desktop Node token store v1 고정 문자열에서 파생한다.
3. `token_sha256`은 raw token을 저장하지 않고 회귀 검증과 operator diff용 metadata로만 둔다.
4. protected file에도 ACL hardening을 적용한다. 기본 reader는 `BUILTIN\Administrators`와 service account principal이다.
5. product diagnostics는 `protected_token`, `token_sha256`, `api_token_protected_file`, `Authorization` 값을 redaction한다.

## Token Source 우선순위와 충돌

Local API와 service config는 source를 자동 추론하지 않는다.

| 입력 | 의미 | 제품 기본 |
|------|------|-----------|
| `-ApiToken` / `--token` | 짧은 개발 smoke용 inline token | 사용 금지 |
| `-ApiTokenFile` / `--token-file` | 기존 plain text token file | 호환 경로 |
| `-ApiTokenProtectedFile` / `--protected-token-file` | DPAPI LocalMachine protected token file | 기본 경로 |

둘 이상의 source가 동시에 지정되면 즉시 오류를 반환한다. 제품 wrapper plan과 service host command line은 protected source만 생성하므로 downgrade/rollback 중에도 활성 token source는 command line과 manifest에서 명확하다.

## Migration과 Rollback

새 설치:

- product wrapper가 `api-token.dpapi.json`을 생성한다.
- `api-token.txt`는 생성하지 않는다.

기존 설치에서 protected file이 없고 legacy `api-token.txt`가 있으면:

- product wrapper는 legacy token 값을 읽어 protected file을 생성한다.
- legacy file은 즉시 삭제하지 않는다. 이전 Phase wrapper로 rollback할 때 필요한 호환 경로가 되기 때문이다.
- 현재 활성 source는 product manifest와 service host metadata의 `api_token_source = protected_file`로 고정한다.
- `RemoveData`는 protected file과 legacy raw token file을 모두 삭제 대상으로 나열한다.

protected file과 legacy file이 둘 다 있으면:

- 현재 버전은 protected file만 사용한다.
- legacy file은 diagnostics에 복사하지 않고 presence metadata만 redaction된 summary에 남긴다.

## Rotation과 Revoke

Phase 15는 service helper에 세 command surface를 둔다.

- `PrepareProtectedTokenFile`: protected file이 없으면 생성한다. legacy raw token이 있으면 migration token으로 사용한다.
- `RotateProtectedTokenFile`: 새 token으로 protected file을 강제 교체한다.
- `RevokeProtectedTokenFile`: protected file을 삭제한다. service restart 전까지 현재 process memory의 token은 유지될 수 있으므로 운영 절차는 stop -> revoke -> prepare/rotate -> start 순서를 따른다.

LAN token rotation audit와 Event Log provider 통합은 Phase 16/17에서 확정한다.

## Runtime Policy

`GET /api/v1/runtime/policy`는 token 값이나 hash를 노출하지 않는다.

반영할 auth metadata:

```json
{
  "auth": {
    "mode": "single_bearer_token",
    "multi_user": false,
    "rbac": false,
    "token_storage": "dpapi-local-machine"
  }
}
```

호환 경로를 사용할 때 `token_storage`는 `external_token_file`, inline 개발 token은 `inline`, auth disabled 상태는 `none`으로 표시한다.

## 검증 기준

기본 검증:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/service/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/cli/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

관리자 smoke는 Phase 14 installer smoke와 같은 조건부 opt-in으로 둔다. 실제 service install/start smoke에서는 `api-token.dpapi.json`을 사용해 runtime policy 200 응답과 `token_storage = dpapi-local-machine`을 확인한다.
