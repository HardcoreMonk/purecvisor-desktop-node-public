# Web/TUI QoS/guest readback Operator Surface 2026-05-21

evidence_id: `web-tui-qos-guest-readback-surface-2026-05-21`
result: `PASS_CODE_LEVEL_AND_04240_PACKAGE_CHAIN_CLOSED`
scope: `web-tui-selected-vm-qos-guest-readback-surface`
product_payload_change_detected: `true`
next_product_payload_package_candidate: `0.42.40-admin-smoke`
package_chain_status: `closed-manual-admin-package-pair-04239-04240`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

이 evidence는 ADR-0007의 Hyper-V QoS/guest-service readback route를 Web Console과 TUI의
운영자 화면에 read-only surface로 노출한 code-level 기록이다. 설치본 0.42.39 PCVCLI
targeted smoke는 `docs/ga-ready/evidence/installed-pcvcli-qos-guest-targeted-smoke-2026-05-21-04239.md`가
계속 소유한다. 이번 변경은 Web/TUI 제품 payload를 바꾸므로 `0.42.40-admin-smoke`
package chain을 열었고, 이후 `admin-smoke-package-2026-05-21-04240`,
`full-admin-host-mutation-gate-2026-05-21-04240-hostmutation`,
`manual-admin-campaign-2026-05-21-04239-04240`로 닫았다.

## Surface

| 진입점 | 구현 |
| --- | --- |
| Web Console | 선택 VM detail에 `QoS / Guest Readback` panel 추가 |
| TUI | 선택 VM에서 `G read selected VM QoS/guest` action 추가 |
| Direct mutation/control | 제공하지 않음 |

Web/TUI가 조회하는 route는 다음 네 read-only route로 제한한다.

- `GET /api/v1/vms/{vm}/blkio`
- `GET /api/v1/vms/{vm}/bandwidth`
- `GET /api/v1/vms/{vm}/guest-agent/status`
- `GET /api/v1/vms/{vm}/guest-agent/ping`

CLI/API alias 용어는 `vm.blkio-get`, `vm.bandwidth`, `vm.guest-agent-status`,
`vm.guest-ping`이며, Web/TUI는 이 alias를 direct command 실행 버튼으로 제공하지 않는다.

## 경계

- `vm.limit`은 CLI/API queued mutation으로 유지하며 Web/TUI direct control로 열지 않는다.
- `vm.blkio-set`은 제품 범위 밖이다.
- `vm.guest-agent-ensure-channel`은 제품 범위 밖이다.
- `vm.guest-exec`은 제품 범위 밖이다.
- Linux cgroup QoS, libvirt blkio mutation, qemu guest agent 호환 claim은 하지 않는다.
- `linux_blkio_compatible=false`, `linux_bandwidth_compatible=false`,
  `qemu_guest_agent=false`, `guest_heartbeat_verified=false` 같은 readback flag는
  Hyper-V 의미의 제한을 드러내기 위한 운영자 설명이다.

## Code-level 변경

- TUI route/client: `src/DesktopNode.Tui/TuiApiRoutes.cs`, `src/DesktopNode.Tui/TuiApiClient.cs`
- TUI operator key/state/render: `src/DesktopNode.Tui/TuiKeys.cs`,
  `src/DesktopNode.Tui/TuiApplication.cs`, `src/DesktopNode.Tui/TuiState.cs`,
  `src/DesktopNode.Tui/TuiRenderer.cs`, `src/DesktopNode.Tui/TuiWidgets.cs`
- Web route/client/state/render: `web/src/served/routes.ts`, `web/src/served/api-client.ts`,
  `web/src/served/state.ts`, `web/src/served/types.ts`, `web/src/served-app.ts`,
  `web/src/app.ts`, `web/styles.css`
- Static/browser fixture: `web/scripts/verify-browser-fixture.mjs`,
  `web/tests/PcvDesktopWeb.Static.Tests.ps1`

## 검증

| Command | Result |
| --- | --- |
| `dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter "TuiApiClientTests|TuiApplicationTests|TuiRendererTests"` | PASS, 96 tests |
| `npm run build:served --prefix web` | PASS |
| `npm run generate:parity --prefix web` | PASS |
| `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"` | PASS, 46 tests |
| `npm test --prefix web` | PASS |

## 다음 단계

1. 설치본 Web/TUI/CLI current-card smoke 재확인
2. Web/TUI selected VM readback UX의 실제 VM 대상 no-overlap 화면 확인

이 evidence는 internal admin-smoke code-level evidence다. Public trusted signing, winget
public submission, public stable installer URL, external stable publication은 주장하지
않는다.
