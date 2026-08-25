# Post-04248 다음 개발 slice 선정

evidence_id: `post-04248-next-slice-selection-2026-05-26`
result: `PASS_DECISION`
selected_next_slice: `ADR-0009 Guest Execution security boundary`
host_mutation_performed: `false`
package_build_performed: `false`
public_release: `not-claimed`

`0.42.48-admin-smoke` Phase 3 Web/TUI QoS direct control은 package build, full admin host
mutation, `0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair closure,
설치본 Web/TUI/CLI current-card까지 닫혔다. 다음 개발 slice는 신규 mutation/control 기능 중
가장 큰 보안 경계를 먼저 닫아야 하는 `Guest Execution / Guest Channel`로 둔다.

## 선택 이유

1. `pcvcli vm guest-exec <vm> -- <command>`와
   `pcvcli vm guest-agent-ensure-channel <vm>`는 credential, audit log, secret redaction,
   timeout/cancel, 권한 모델을 먼저 정의해야 한다.
2. ADR-0009는 이미 `security-boundary-deferred`로 보류 상태이며, 구현 전 정책 ADR을
   product contract로 닫아야 한다.
3. Web/TUI direct control은 QoS 범위만 열렸고 guest command execution button/panel은
   ADR-0009가 닫히기 전까지 금지한다.

## 후속 작업

- ADR-0009를 candidate에서 applied security boundary로 승격하기 위한 spec 작성.
- guest credential source, RBAC capability `guest.exec`, audit schema, redaction rule,
  timeout/cancel semantics, evidence redaction contract를 먼저 정의.
- 그 다음 Local API preview/execute route, PCVCLI UX, Web/TUI read-only or disabled state,
  installed smoke gate를 순서대로 구현한다.

## 후속 Closure

이 selection은 `docs/ga-ready/evidence/guest-execution-security-boundary-2026-05-26.md`로
docs-contract closure가 생성되면서 실행됐다. ADR-0009 적용 문서는
`docs/adr/0009-guest-execution-security-boundary.md`이고, route/CLI/Web/TUI 구현은 다음
product payload로 남긴다.
