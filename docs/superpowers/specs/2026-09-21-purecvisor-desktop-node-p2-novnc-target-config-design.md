# Desktop Node P2-11 noVNC target 설정 설계

- Design-ID: `purecvisor-desktop-node-p2-novnc-target-config-v1`
- 작성일: `2026-09-21`
- 문서 상태: `implemented-slice-4`
- 소스 기획: `docs/SERVICE_PLAN.md` §7.1 P2-11
- 선행: ADR-0010 후보, `GET /api/v1/console/capabilities`, Host `--novnc-target-host`/`--novnc-target-port`
- host mutation: `false`
- 변경 등급: `M`
- public trusted signing: `false`
- external stable publication: `false`

## 문제

noVNC bridge는 Host argv가 둘 다 있을 때만 켜진다. 설치본 smoke가 PathName을 바꾸는 것은
evidence runner이지 제품 표면이 아니다. SERVICE_PLAN P2-11은 target을 제품 기능으로 열기
**전에** ADR-0010의 audit / rollback / loopback 기본 / reload를 닫으라고 한다. 화면에서
target을 저장하거나 LAN을 기본으로 켜면 안 된다.

이 설계가 ADR-0010 후보의 보류 결정을 닫는다. 적용 ADR 파일 승격은 set/clear가
source_head에 들어간 뒤에 한다.

## 현재 계약

- Feature `pcv.console.capabilities` (`GET /api/v1/console/capabilities`)와
  `pcv.vm.console-handoff` (`GET /api/v1/vms/{vmId}/console`). permission `console.view`.
- Bridge는 `DesktopNodeHostOptions.NoVncBridgeEnabled` = host와 port가 둘 다 있을 때만 true.
- 비-loopback target은 기존 `--allow-lan` 없이 `PCV_HOST_NOVNC_TARGET_NOT_LOOPBACK`.
- WebSocket 경로는 `/api/v1/console/novnc/{vm_id}`. 운영자가 바꾸지 않는다.
- catalog routes 68. 28 feature 유지. P0 evidence 후보는 04275 기준 4개 그대로.
- `console.configure` permission은 없다. admin 권한 목록은 `console.view`와 `account.manage`까지다.

## 결정

- Feature는 새 ID를 만들지 않는다. `pcv.vm.console-handoff`에 붙인다.
- permission은 `console.configure`다. admin과 `*`만 갖는다. operator의 `console.view`로는
  preview/set/clear를 할 수 없다. service bearer는 받는다.
- 기본 target scope는 loopback (`127.0.0.1`, `::1`, `localhost`). LAN target은 기존 listener
  `--allow-lan`이 **이미** 있을 때 + 요청 `allow_lan_target=true` + 비어 있지 않은 `reason`이
  있을 때만 허용한다. set이 `--allow-lan`을 켜거나 firewall rule을 만들지 않는다.
- 제품 durable source는 `%ProgramData%\PureCVisor\desktop-node\novnc-target.json`이다.
  파일이 있으면 PathName `--novnc-target-*`보다 이긴다. 파일이 없을 때만 PathName fallback.
- clear는 파일을 지워서 PathName이 부활하게 두지 않는다. `{ "enabled": false }`를 남겨
  다음 listen도 bridge를 끈다.
- listen 프로세스를 stop/start 하지 않는다. set/clear는 파일 원자 교체 후 in-process reload다.
  같은 프로세스 queued worker가 SCM stop을 호출하면 job store commit 전에 listener가 죽는다.
- preview는 동기 ProductOperation. set/clear는 queued mutation. worker는 파일을 쓰고
  in-memory `DesktopNodeConsoleOptions`만 갱신한다. mutation을 재호출하지 않는 reconcile은
  파일이 expected_after와 같으면 `succeeded`다.
- Web은 target을 저장하는 폼을 열지 않는다. 기존 Console Access Card readback만 유지한다.
  `console.view` 응답에 host/port를 넣지 않는다.
- websocket path, scheme, listen prefix는 이 설계 밖이다.
- P2-12 주기 checkpoint, P2-13 export/import, P2-14 네트워크 변경, P2-15 NIC/DVD add는
  이 설계 밖이다.

## 파일 스키마 `pcv-novnc-target-v1`

```json
{
  "schema": "pcv-novnc-target-v1",
  "enabled": true,
  "host": "127.0.0.1",
  "port": 5900,
  "allow_lan_target": false,
  "reason": null
}
```

`enabled=false`이면 host/port는 없어도 된다. `enabled=true`이면 host와 port가 둘 다 필요하다.
원자 쓰기는 temp+move, extra JSON 보존.

## Route (slice 2~3)

| operation_id | method / template | stance | permission |
| --- | --- | --- | --- |
| `console.novnc-target.preview` | `POST /api/v1/console/novnc-target/preview` | ProductOperation | `console.configure` |
| `console.novnc-target.set` | `POST /api/v1/console/novnc-target` | QueuedMutation | `console.configure` |
| `console.novnc-target.clear` | `POST /api/v1/console/novnc-target/clear` | QueuedMutation | `console.configure` |

이후 catalog: routes 68→71, ProductOperation 16→17, queued 29→31, ReadOnly 23 유지.
Digest는 HTTP slice에서 pin한다. Hyper-V domain/dispatch는 건드리지 않는다.

Set body: `host`, `port`, optional `allow_lan_target` (기본 false), optional `reason`.
Clear body: 없음. CLI는 `--yes`.
Preview body는 set과 같고 `dry_run`이다. host mutation 없음.

Preview/set 응답의 configure 표면에만 host/port를 넣는다. LAN host는 audit/configure 응답에
남기되 diagnostics bundle과 `console.view` card에는 넣지 않는다.

## Audit `pcv-novnc-target-audit/v1`

actor, request_id, previous enabled/host/port, proposed enabled/host/port, reload result.
token, password, JWT, credential 값은 금지. LAN host는 Event Log/diagnostics에서
`redacted-non-loopback`으로 줄일 수 있다. loopback host는 그대로 둔다.

## 거절 코드

| code | 때 |
| --- | --- |
| `PCV_NOVNC_TARGET_HOST_REQUIRED` | enabled set인데 host 없음 |
| `PCV_NOVNC_TARGET_PORT_INVALID` | port 없거나 1~65535 밖 |
| `PCV_NOVNC_TARGET_NOT_LOOPBACK` | 비-loopback인데 LAN gate 없음 |
| `PCV_NOVNC_TARGET_LAN_GATE_REQUIRED` | 비-loopback인데 listener `--allow-lan`이 없음 |
| `PCV_NOVNC_TARGET_REASON_REQUIRED` | 비-loopback인데 reason이 비어 있음 |
| `PCV_NOVNC_TARGET_INCOMPLETE` | host/port 중 하나만 있음 |
| `PCV_NOVNC_CONFIGURE_FORBIDDEN` | `console.configure` 없음 |

## Slice 1 — 정책 계약

- `NoVncTargetPolicy.EvaluatePreview` / `EvaluateSet` / `EvaluateClear` (순수, IO 없음).
- loopback/LAN gate, port, incomplete pair, reason, permission 규칙을 테스트로 고정한다.
- HTTP, persist, CLI, Web, SCM PathName 쓰기는 이 slice가 아니다.

## Slice 2 — preview HTTP/CLI

- `POST /api/v1/console/novnc-target/preview`.
- `pcvcli console novnc-target preview --host 127.0.0.1 --port 5900`.
- catalog 68→69, ProductOperation 16→17, digest `8e6eb61af8042c81d68abdd1c29040f635a2c8be886e2c9f7c1ebc26e0f247d0`.
  set/clear와 Web 저장 폼은 이 slice가 아니다.

## Slice 3 — queued set/clear

- 두 queued route, 파일 persist, in-process reload, 이전 파일 rollback, audit.
- `pcvcli console novnc-target set --host 127.0.0.1 --port 5900 --yes`
- `pcvcli console novnc-target clear --yes`
- `--yes` 없으면 `PCV_CLI_CONFIRMATION_REQUIRED`.
- interrupted job reconcile은 파일을 읽고 mutation을 다시 쓰지 않는다.
- catalog 69→71, queued 29→31.
- 설치본 PathName rewrite smoke는 Lane 2이며 이 캠페인이 열지 않는다.

## Slice 4 — Web readback only

- Console Access Card는 기존 `enabled`/`status`/`reason_code`만 유지한다.
- target 입력, 저장 버튼, LAN 토글을 추가하지 않는다.

## 비목표

- Web/TUI에서 target 저장, LAN 기본 on, firewall 생성
- websocket path/scheme/listen prefix 변경
- 서비스 SCM stop/start로 bridge를 켜고 끄기
- 주기 checkpoint, export/import, 스위치 편집, NIC/DVD add
- current-evidence write, package-pair, Lane 2 Hyper-V 복구
- 29번째 feature, P0 evidence 후보 변경
- ADR-0010 적용 파일 승격 (set/clear 이후)

## 검증

- Slice 1: Contracts 또는 Api 단위 테스트. host mutation 없음.
- Slice 2~3: focused `dotnet test` + catalog pin. dirty-tree는 four-shard PASS가 아니다.
- Slice 4: `npm run test:required --prefix web`.
- 설치본 target-backed streaming smoke는 Lane 2이며 이 캠페인이 열지 않는다.
