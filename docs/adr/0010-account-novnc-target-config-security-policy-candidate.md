# ADR-0010: Account/noVNC Target Config Security Policy 후보

상태: 후보
일자: 2026-05-26

## 결정 마커

```text
DESKTOP_NODE_ACCOUNT_NOVNC_TARGET_CONFIG_SECURITY_POLICY_DECISION: security-policy-required-before-mutation
phase: Phase 5 Account/noVNC Operator Surface Extension Candidate
implementation_status: not-implemented
novnc_target_config_mutation: deferred
default_network_scope: loopback-only
lan_exposure_gate: explicit-operator-opt-in-required
credential_storage_change: prohibited-by-this-candidate
audit_log_schema: required-before-implementation
rollback_contract_required: true
web_tui_direct_config_control: prohibited-until-policy-applied
host_mutation_performed: false
package_build_performed: false
public_release: not-claimed
```

## 맥락

Phase 1 Account/noVNC Operator Surface는 read-only Console Access Card와 handoff clarity만
제품화했다. Installed noVNC smoke는 target-backed streaming을 검증하지만, 그 smoke에서 수행한
service PathName/target 변경은 evidence runner의 manual-admin 동작이지 제품 화면의 self-service
mutation이 아니다.

noVNC target host/port를 제품 기능으로 바꾸려면 network exposure, service reload, rollback,
audit, redaction, permission boundary를 먼저 닫아야 한다.

## 보류 결정

noVNC target config mutation은 아래 정책이 닫힐 때까지 구현하지 않는다.

| 경계 | 필요한 결정 |
| --- | --- |
| target scope | 기본은 `127.0.0.1` loopback-only, non-loopback은 explicit LAN gate 필요 |
| permission | `console.configure` 또는 별도 `novnc.configure` capability 필요 |
| audit | actor, previous target, proposed target, request id, service reload result 기록 |
| redaction | token, account secret, private host detail, credential value 미표시 |
| validation | host, port, path, scheme, listen scope, collision, firewall implication |
| apply | queued service config mutation, restart 또는 reload 방식 명시 |
| rollback | previous PathName/options restore와 post-restore probe |
| smoke | target-backed noVNC streaming, old target rejection, final PathName restore |

## 후보 Contract

```text
pcvcli console novnc-target preview --host 127.0.0.1 --port 5900
pcvcli console novnc-target set --host 127.0.0.1 --port 5900 --yes
pcvcli console novnc-target clear --yes
```

Web/TUI direct control은 정책 적용 전까지 금지한다. 정책 적용 후에도 처음에는 CLI/service-action
중심으로 열고, Web/TUI는 readback과 handoff를 먼저 유지한다.

## Loopback/LAN Gate

- 기본 target은 loopback-only다.
- LAN target은 explicit flag와 audit reason이 있어야 한다.
- LAN target을 열어도 firewall rule 생성은 별도 Host Ops policy로 분리한다.
- target probe artifact는 address/port와 stable reason code만 남기고 credential이나 token을
  남기지 않는다.

## 검증 Gate

1. Config preview route 또는 service-action dry-run.
2. Loopback-only validation tests.
3. LAN explicit gate negative/positive tests.
4. Service reload/restart rollback tests.
5. Installed target-backed streaming smoke.
6. Final PathName restore proof.
7. Full admin host mutation gate와 manual-admin package-pair closure.

## 경계

이 ADR 후보는 account/noVNC 설정 mutation의 보안 정책을 시작하기 위한 문서다. 현재 제품은
Console Access Card와 noVNC handoff/readback만 제공하며, target host/port self-service 변경은
지원하지 않는다. Public trusted signing, external stable publication도 주장하지 않는다.
