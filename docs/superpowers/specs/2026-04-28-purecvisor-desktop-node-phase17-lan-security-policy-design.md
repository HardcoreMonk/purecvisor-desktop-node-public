# PureCVisor Desktop Node Phase 17 LAN Mode 제품 보안 정책 설계

## 목적

Phase 17은 Desktop Node LAN mode를 제품 후보 배포 계층에서 어떻게 취급할지 확정한다.

Phase 5에서 Local API는 `-AllowLan`과 bearer token이 함께 있을 때만 non-loopback prefix를 허용하도록 hardening됐다. Phase 13은 loopback listener에서만 static Web Console asset을 bearer token 없이 열 수 있게 했고, non-loopback static asset은 API route와 같은 bearer token 정책을 유지했다. Phase 15는 제품 기본 token source를 DPAPI LocalMachine protected token file로 전환했고, Phase 16은 JSONL first diagnostics와 diagnostic bundle manifest를 고정했다.

남은 문제는 LAN mode를 제품 옵션으로 볼지, preview/admin opt-in으로 둘지, TLS와 firewall lifecycle을 어느 계층이 책임질지이다. Phase 17은 LAN mode를 편하게 켜는 기능이 아니라, 노출 확대가 명시적이고 진단 가능한 상태로만 존재하도록 제품 정책을 고정한다.

Phase 17도 Desktop Node 전체를 제품 런타임으로 GA 승격하지 않는다. `PRODUCT_RUNTIME_PROMOTION_DECISION: keep-spike`는 유지한다. 구현 범위는 `packaging/windows-desktop-node/**`와 `spikes/purecvisor-desktop-node/**`로 제한하고, Linux `purecvisorsd` 또는 Single Edge 공개 UI/API 표면과 연결하지 않는다.

## 결정

```text
DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required
```

Phase 17의 결정은 다음과 같다.

- 기본 설치와 product wrapper 기본 plan은 계속 loopback-only다.
- LAN mode는 GA 제품 기본 기능이 아니라 administrator opt-in preview다.
- product wrapper는 TLS endpoint를 직접 제공하지 않는다.
- non-loopback 노출은 reverse proxy 또는 외부 TLS terminator가 앞단에 있다는 전제를 diagnostics와 runtime policy에 명시한다.
- non-loopback static Web Console asset은 API route와 같은 bearer token 정책을 유지한다.
- Windows Firewall rule lifecycle은 installer 자동 적용이 아니라 product action 또는 수동 관리자 명령의 explicit opt-in으로만 둔다.
- token rotation/revoke, LAN exposure 변경, firewall ensure 계획은 JSONL diagnostics와 diagnostic bundle에서 추적 가능해야 한다.

## 제품 정책 v1

product plan과 manifest는 LAN security policy v1을 포함한다.

```json
{
  "schema_version": 1,
  "decision": "DESKTOP_NODE_PHASE17_LAN_SECURITY_DECISION: loopback-default-lan-preview-reverse-proxy-required",
  "default_exposure": "loopback",
  "lan_mode": {
    "state": "preview-admin-opt-in",
    "enabled_by_default": false,
    "requires_allow_lan": true,
    "requires_bearer_token": true,
    "token_source": "dpapi-local-machine-protected-file",
    "non_loopback_static_auth": "bearer-required"
  },
  "tls": {
    "provided_by_product_wrapper": false,
    "required_for_lan": true,
    "termination": "external-reverse-proxy-or-tls-terminator"
  },
  "firewall": {
    "enabled_by_default": false,
    "lifecycle_owner": "admin-opt-in-product-action-or-manual-command",
    "installer_auto_enable": false,
    "default_profile": "private"
  },
  "diagnostics": {
    "record_exposure": true,
    "record_tls_stance": true,
    "record_firewall_plan": true,
    "record_token_storage": true
  }
}
```

이 정책은 LAN mode를 금지하지 않는다. 다만 제품 wrapper가 non-loopback HTTP listener를 직접 안전한 원격 제품 표면으로 포장하지 않는다는 점을 명확히 한다. 제품 wrapper가 자체 TLS certificate lifecycle, mTLS, multi-user auth, CORS, public Web Console UX를 제공하기 전까지 LAN mode는 preview/admin opt-in이다.

## 위협 모델

Phase 17이 다루는 공격자는 같은 LAN 또는 라우팅 가능한 사설망에서 Desktop Node listener에 접근할 수 있는 사용자다.

방어 기준:

1. 기본 설치로는 LAN에서 listener에 접근할 수 없어야 한다.
2. non-loopback prefix는 `-AllowLan`과 non-empty token source 없이는 시작할 수 없어야 한다.
3. LAN mode에서 static Web Console asset은 API와 같은 bearer token gate를 지나야 한다.
4. bearer token은 DPAPI LocalMachine protected token file을 제품 기본값으로 유지한다.
5. plain HTTP LAN 노출은 제품 wrapper가 안전하다고 주장하지 않는다. TLS는 reverse proxy 또는 외부 TLS terminator의 책임이다.
6. firewall rule 추가는 관리자 opt-in이고, 기본 install/repair가 방화벽을 열지 않는다.
7. diagnostics는 LAN exposure, token storage, TLS stance, firewall stance를 redaction된 형태로 남긴다.

범위 밖:

- product wrapper 내장 TLS server
- certificate issuance/renewal/revocation
- mTLS
- multi-user auth/RBAC
- CORS/OPTIONS 공개
- remote browser session history sync
- Windows Credential Manager 전환
- installer 기본 firewall open
- LAN mode GA 승격

## Static Web Console Auth Boundary

Phase 13의 loopback UX 결정은 유지한다.

```text
loopback static asset + token configured     -> bearer token 없이 허용
loopback API route + token configured        -> bearer token 필요
non-loopback static asset + token configured -> bearer token 필요
non-loopback API route + token configured    -> bearer token 필요
```

이 경계는 browser가 `http://127.0.0.1:7777/`을 먼저 열고 이후 token을 입력할 수 있게 하기 위한 local UX이다. 같은 예외를 LAN에 확장하지 않는다.

## TLS와 Reverse Proxy

Phase 17에서 product wrapper는 TLS를 직접 제공하지 않는다.

이유:

- `HttpListener` 기반 spike에 certificate binding과 lifecycle을 넣으면 제품 wrapper, installer, service account, machine certificate store 책임이 섞인다.
- 인증서 발급과 갱신 정책은 LAN preview보다 큰 제품 결정이다.
- reverse proxy를 전제로 두면 Desktop Node는 loopback 또는 restricted non-loopback backend로 남고, TLS/certificate/header policy는 앞단에서 관리할 수 있다.

따라서 LAN mode diagnostics는 `tls.required_for_lan = true`, `tls.provided_by_product_wrapper = false`, `tls.termination = external-reverse-proxy-or-tls-terminator`를 노출한다.

## Firewall Lifecycle

Windows Firewall rule은 계속 opt-in이다.

- installer는 기본 install/repair에서 firewall rule을 열지 않는다.
- Local API의 `-EnsureFirewallRule` command builder는 유지한다.
- product wrapper는 LAN plan 또는 diagnostics에 firewall plan을 기록할 수 있지만, 실제 rule ensure는 명시적 product action 또는 수동 관리자 명령으로만 실행한다.
- default profile은 Phase 5와 같이 `private`이다.
- `public` 또는 `any` profile은 운영자가 명시적으로 선택해야 한다.

## Runtime Policy와 Diagnostics

`GET /api/v1/runtime/policy`는 기존 persistence/retry/cancel/worker/CORS/auth 정책에 network policy를 추가해야 한다.

필수 필드:

```json
{
  "network": {
    "default_exposure": "loopback",
    "current_exposure": "loopback",
    "lan_mode": "preview-admin-opt-in",
    "static_asset_auth": {
      "loopback": "unauthenticated-static-only",
      "non_loopback": "bearer-required"
    },
    "tls": {
      "provided_by_product_wrapper": false,
      "required_for_lan": true,
      "termination": "external-reverse-proxy-or-tls-terminator"
    },
    "firewall": {
      "enabled_by_default": false,
      "lifecycle_owner": "admin-opt-in-product-action-or-manual-command"
    }
  }
}
```

Product diagnostic bundle은 product manifest의 LAN security policy와 runtime policy network object를 redaction된 artifact로 포함해야 한다.

## 검증 기준

기본 검증:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/api/tests' -Output Detailed"
pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path 'spikes/purecvisor-desktop-node/tests' -Output Detailed"
node --check spikes/purecvisor-desktop-node/web/app.js
git diff --check
```

관리자 smoke는 조건부 opt-in이다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action Plan
pwsh -NoProfile -ExecutionPolicy Bypass -File packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1 -Action CollectDiagnostics -WhatIf
```

실제 LAN listener start, firewall rule ensure, reverse proxy integration smoke는 관리자 권한과 운영자가 제공한 네트워크/TLS 환경에서만 실행한다. Phase 17 기본 완료 조건은 policy object, runtime policy, diagnostics, 문서, non-integration suite 검증이다.

## 완료 기준

Phase 17은 다음을 만족하면 완료다.

- product plan과 manifest가 LAN security policy v1을 포함한다.
- product wrapper 기본 prefix는 loopback-only로 유지된다.
- Service host 기본 인자에 `-AllowLan`이나 firewall 자동 ensure가 들어가지 않는다. Phase 13 당시 WinSW XML과 2026-05-01 이후 `DesktopNode.Host.exe listen` 경로 모두 같은 기본 loopback-only 정책을 따른다.
- Local API runtime policy가 LAN static auth, TLS stance, firewall stance를 노출한다.
- non-loopback static asset bearer-token-required 정책이 테스트로 고정된다.
- diagnostic bundle이 LAN security policy와 runtime network policy를 redaction된 형태로 포함한다.
- 문서가 Phase 17을 구현 완료가 아니라 LAN security policy 구현/검증 대상으로 정확히 가리킨다.
