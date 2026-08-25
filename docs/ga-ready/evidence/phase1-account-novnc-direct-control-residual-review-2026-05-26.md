# Phase 1 Account/noVNC Direct-control 잔여 범위 Review

evidence_id: `phase1-account-novnc-direct-control-residual-review-2026-05-26`
result: `PASS`
scope: `phase1-direct-control-residual-review`
version_anchor: `0.42.45-admin-smoke`
decision: `read-only-console-access-card-and-open-handoff-only`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 결론

Phase 1은 account/noVNC를 새 mutation surface로 확장하지 않고, 운영자가 현재 상태와
handoff 경로를 명확히 볼 수 있게 제품화하는 slice로 닫는다. `Open selected console`은
VM console session lookup과 browser/noVNC handoff를 여는 표시/조회 동작이며, host config,
credential, 권한, noVNC target을 변경하는 direct-control 버튼은 아니다.

## 닫힌 범위

| 범위 | 상태 | 근거 |
| --- | --- | --- |
| Console Access Card 공통 projection | `closed` | API/CLI/TUI/Web product payload, `0.42.45-admin-smoke` package chain |
| Web Account/Console card | `closed` | `docs/ga-ready/evidence/web-console-console-novnc-ux-qa-2026-05-26-04245.md` |
| installed account login/browser/noVNC smoke | `closed` | `artifacts/installed-account-login-smoke-20260526-04245/summary.json`, `artifacts/target-backed-novnc-installed-streaming-smoke-20260526-04245/summary.json` |
| installed Web/TUI/CLI current-card | `closed` | `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04245.md` |
| package/fullgate/manual-admin closure | `closed` | `admin-smoke-package-2026-05-26-04245`, `full-admin-host-mutation-gate-20260526-04245`, `manual-admin-campaign-2026-05-26-04244-04245` |

## 계속 닫아 둘 범위

| 후보 | 현재 결정 | 재개 조건 |
| --- | --- | --- |
| noVNC target host/port 설정 mutation | `deferred` | account/noVNC security and operator policy ADR |
| account/RBAC/JWT schema 또는 권한 수정 버튼 | `deferred` | auth/session 권한 모델 ADR |
| service token rotation/revoke direct button | `operator-owned` | 기존 elevated service-token runbook 유지 |
| Guest Execution / Guest Channel | `security-boundary-deferred` | credential, audit log, secret redaction, timeout/cancel, RBAC ADR |
| Hyper-V QoS mutation | `hyperv-qos-mutation-policy` | rollback/readback policy, actual VM mutation smoke, fullgate |
| Web/TUI direct mutation control | `backend-policy-first` | Phase 2 같은 backend mutation policy가 닫힌 뒤 Phase 3에서만 노출 |
| Linux Single Runtime Object 계열 | `out-of-product-scope` | 별도 제품 라인 ADR |

## 다음 slice 후보

1. Phase 2 Hyper-V QoS Mutation Policy ADR: `vm blkio-set`, bandwidth mutation의 dry-run,
   rollback/readback, actual VM smoke 기준을 먼저 정한다.
2. Phase 3 Web/TUI Direct Control: Phase 2에서 승인된 mutation만 작은 버튼/패널로 연다.
3. Phase 4 Guest Execution Security Boundary: guest command 실행은 credential/audit/redaction
   정책이 닫힐 때까지 구현하지 않는다.

이 review는 Phase 1의 residual direct-control 범위를 닫기 위한 문서 evidence다. 제품 코드,
host config, package artifact를 새로 변경하지 않는다.
