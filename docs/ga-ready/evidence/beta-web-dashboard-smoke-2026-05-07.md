# Beta Web Dashboard Smoke Evidence - 2026-05-07

```text
evidence_id: beta-web-dashboard-smoke-2026-05-07
artifact_root: artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025743
```

## 요약

2026-05-07 Beta-0 Web Dashboard smoke는 Batch Supervisor `WebRegression` profile로 실행했고 PASS했다.

- `web/tests` Pester: PASS, 26 tests
- `npm test --prefix web`: PASS
- `npm run verify:parity --prefix web`: PASS
- `node --check web/app.js`: PASS
- Batch summary: `ok=true`, `status=completed`, `total_steps=4`, `executed_steps=4`

이 smoke는 Web Dashboard/Ops Cockpit, VM Workbench, Activity/Troubleshooting, batch evidence dashboard, monitoring/auth/checkpoint warning, TypeScript served asset parity, browser fixture를 read-only/static fixture 범위에서 확인한다.

## 범위

이 evidence는 internal Beta-0 Web Dashboard readiness evidence다. 다음 항목은 실행하지 않았다.

- Hyper-V VM 생성/삭제
- service/MSI install/repair/uninstall
- firewall/trust-store/LAN mutation
- Event Log source mutation
- installed update/rollback mutation
- public trusted signing
- 외부 stable publication

## Runner 보강

첫 beta smoke attempt `artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025504`는 Windows `ProcessStartInfo`에서 bare `npm` command를 찾지 못해 `web-npm-test`에서 실패했다.

두 번째 attempt `artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025637`는 `npm.cmd` command name으로 시작했지만 Windows batch shim resolution이 repo root 기준 `node_modules\npm`을 찾는 문제로 실패했다.

후속 code fix는 Batch Supervisor WebRegression profile이 Windows에서 `Get-Command npm.cmd`의 absolute path를 사용하도록 조정했다. 최종 attempt `artifacts/batch-runs/beta-web-dashboard-smoke-20260507-025743`은 이 수정 후 PASS했다.

## 판정

Web Dashboard는 internal operator beta의 read-only/static fixture 범위에서 테스트 가능하다. Destructive VM workflow beta, LAN beta, installed update/rollback beta는 별도 관리자 opt-in smoke로 분리한다.

이 evidence는 public release/public trusted signing/외부 stable publication evidence가 아니다.
