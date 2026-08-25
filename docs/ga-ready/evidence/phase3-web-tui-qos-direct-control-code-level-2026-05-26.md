# Phase 3 Web/TUI QoS Direct Control Code-Level Evidence

evidence_id: `phase3-web-tui-qos-direct-control-code-level-2026-05-26`
created_at: `2026-05-26T17:18:23+09:00`
status: `PASS_CODE_LEVEL_PACKAGE_CHAIN_OPEN`
scope: `web-tui-operator-surface-qos-direct-control`
host_mutation_performed: `false`
package_build_performed: `false`
product_payload_change: `true`
next_package_chain: `0.42.48-admin-smoke-package-fullgate-current-card-pass-manual-admin-pending`
promoted_by: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md`

## 요약

ADR-0008로 닫힌 Hyper-V QoS preview/apply contract를 Web Console과 TUI 운영자 surface에
연결했다. Backend route나 Hyper-V mutation semantics는 새로 만들지 않았고, 이미 설치본
manual-admin closure가 끝난 Local API route만 사용한다.

Guest Execution, guest channel 생성, account/noVNC target config mutation은 각각 ADR-0009,
ADR-0010 경계가 닫히기 전까지 보류 상태로 유지했다.

## 구현 범위

| 영역 | 결과 |
| --- | --- |
| Web route registry | `vmQosStoragePreview`, `vmQosStorage`, `vmQosNetworkPreview`, `vmQosNetwork` 추가 |
| Web selected VM detail | storage/network QoS preview/apply form 추가 |
| Web apply guard | `operate` permission hint, `window.confirm`, tracked job 연결 |
| TUI key mapping | `P` selected VM QoS reset preview, `A` selected VM QoS reset apply confirmation |
| TUI API client | storage/network preview/apply route와 reset payload 추가 |
| TUI renderer | direct-control help line과 preview 결과 표시 추가 |
| Deferred boundary | ADR-0009/ADR-0010 보류 copy와 test assertion 유지 |

## 검증

| Command | Result |
| --- | --- |
| `dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --filter "FullyQualifiedName~TuiApiClientTests|FullyQualifiedName~TuiApplicationTests|FullyQualifiedName~TuiRendererTests"` | PASS, 114 tests |
| `pwsh -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed"` | PASS, 47 tests |
| `node --check web\app.js` | PASS |
| `npm test --prefix web` | PASS |
| `npm run verify:parity --prefix web` | PASS |

## 남은 Release Gate

이 변경은 Web/TUI product payload를 바꾸므로 0.42.47 installed anchor를 대체하지 않았다.
후속 release slice에서 `0.42.48-admin-smoke` package build, full admin host mutation gate,
installed Web/TUI/CLI current-card smoke가 PASS했고, evidence는
`docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04248.md`,
`docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-05-26-04248-hostmutation.md`,
`docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04248-manual-admin.md`가
소유한다. 남은 release gate는 `0.42.47-admin-smoke -> 0.42.48-admin-smoke`
manual-admin package-pair campaign이다.
