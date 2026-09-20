# ADR-0010: noVNC Target Config Security Policy

상태: 적용 중
일자: 2026-09-21

## 결정 마커

```text
DESKTOP_NODE_ACCOUNT_NOVNC_TARGET_CONFIG_SECURITY_POLICY_DECISION: accepted-file-persist-in-process-reload
phase: P2-11 noVNC target config
supersedes: docs/adr/0010-account-novnc-target-config-security-policy-candidate.md
design: docs/superpowers/specs/2026-09-21-purecvisor-desktop-node-p2-novnc-target-config-design.md
implementation_status: code-level-preview-set-clear-web-readback
product_payload_change: true
feature_id: pcv.vm.console-handoff
permission: console.configure
durable_source: %ProgramData%\PureCVisor\desktop-node\novnc-target.json
pathname_fallback: file-absent-only
clear_contract: enabled-false-file-not-delete
reload: in-process-no-scm-stop-start
default_network_scope: loopback-only
lan_exposure_gate: listener-allow-lan-and-request-allow_lan_target-and-reason
web_direct_config_control: prohibited-readback-only
cli_preview: pcvcli console novnc-target preview --host 127.0.0.1 --port 5900
cli_set: pcvcli console novnc-target set --host 127.0.0.1 --port 5900 --yes
cli_clear: pcvcli console novnc-target clear --yes
audit_log_schema: pcv-novnc-target-audit/v1
reconciliation_schema: pcv-novnc-target-reconciliation/v1
rollback_contract: previous-file-bytes
installed_streaming_smoke: lane-2-not-claimed
host_mutation_performed: false
package_build_performed: false
public_release: not-claimed
```

## 맥락

후보 ADR은 noVNC target host/port를 제품 기능으로 열기 전에 loopback 기본값, LAN gate,
audit, rollback, reload를 닫으라고 했다. 설치본 PathName rewrite는 evidence runner이지
제품 표면이 아니다.

P2-11은 그 정책을 source_head에 구현했다. durable source는
`%ProgramData%\PureCVisor\desktop-node\novnc-target.json`이고, 파일이 있으면 PathName
`--novnc-target-*`보다 이긴다. clear는 파일을 지워서 PathName이 부활하게 두지 않고
`enabled=false`를 남긴다. set/clear는 queued mutation이며 listen 프로세스를 SCM
stop/start 하지 않는다.

## 결정

1. permission은 `console.configure`다. admin과 `*`와 service bearer만 갖는다.
2. 기본 target은 loopback (`127.0.0.1`, `localhost`, `::1`)이다. LAN target은 기존
   listener `--allow-lan`과 요청 `allow_lan_target=true`와 비어 있지 않은 `reason`이
   모두 있을 때만 허용한다. set은 `--allow-lan`을 켜거나 firewall rule을 만들지 않는다.
3. preview는 동기 ProductOperation이다. set/clear는 queued mutation이다.
4. worker는 파일을 원자 교체한 뒤 in-memory `DesktopNodeConsoleOptions`만 reload한다.
5. 쓰기 실패 시 이전 파일 바이트를 되돌린다. extra JSON은 보존한다.
6. interrupted job reconcile은 파일을 읽고 mutation을 다시 쓰지 않는다.
7. Web Console Access Card는 `enabled`/`status`/`reason_code` readback만 유지한다.
   target 입력, 저장 버튼, LAN 토글은 열지 않는다. `console.view` 응답에 host/port를
   넣지 않는다.
8. 설치본 target-backed streaming smoke와 PathName rewrite smoke는 Lane 2이며 이 ADR
   적용 자체가 current-evidence 또는 package-pair를 열지 않는다.

## 거절 코드

`PCV_NOVNC_TARGET_HOST_REQUIRED`, `PCV_NOVNC_TARGET_PORT_INVALID`,
`PCV_NOVNC_TARGET_NOT_LOOPBACK`, `PCV_NOVNC_TARGET_LAN_GATE_REQUIRED`,
`PCV_NOVNC_TARGET_REASON_REQUIRED`, `PCV_NOVNC_TARGET_INCOMPLETE`,
`PCV_NOVNC_CONFIGURE_FORBIDDEN`.

## 남은 게이트

- 설치본 file-backed set/clear 후 capabilities/bridge readback smoke
- 기존 target-backed streaming smoke를 file overlay로 재실행
- full admin host mutation / manual-admin package-pair는 다음 product payload가 열릴 때

이 남은 게이트는 적용 결정을 되돌리지 않는다. Public trusted signing과 외부 stable
publication은 주장하지 않는다.
