# Phase 1 Account/noVNC Operator Surface Design Review

## 목적

이 문서는 `Phase 1 Account/noVNC Operator Surface Reproductization`의
`plan-design-review` 결정 기록이다. 선행 문서는 아래 두 개다.

- `docs/superpowers/specs/2026-05-25-purecvisor-desktop-node-extension-roadmap-design.md`
- `docs/superpowers/specs/2026-05-25-purecvisor-desktop-node-extension-domain-architecture.md`

이번 review는 구현 계획이 아니다. 목적은 Web/TUI/CLI가 최신 payload 기준으로 account/noVNC
운영자 여정을 어떻게 표시해야 하는지, 그리고 어떤 동작을 열지 말아야 하는지 고정하는 것이다.

## 결정 마커

```text
DESKTOP_NODE_PHASE1_ACCOUNT_NOVNC_DESIGN_REVIEW_DECISION: console-access-card-common-model
phase1_first_implementation_slice: account-novnc-operator-surface
phase1_productization_scope: small-product-surface-change
phase1_surface_scope: web-tui-cli-status-and-handoff-clarity
phase1_novnc_behavior_change: false
phase1_account_rbac_jwt_change: false
phase1_action_buttons: false
phase1_next_action_copy: short-operator-copy-only
phase1_release_gate: full-product-payload-package-chain
host_mutation_performed: false
package_build_performed: false
public_release: not-claimed
```

## 확정 범위

Phase 1은 첫 implementation slice로 선택한다. 다만 기능 범위는 작은 제품화 변경으로 제한한다.

허용 범위는 다음이다.

- Web/TUI/CLI 모두에 account/noVNC 상태 표시와 handoff 명확화 적용
- 공통 Console Access Card projection 도입
- account/session 상태와 console/noVNC 상태를 같은 모델로 표시
- noVNC availability와 reason code를 구조화해서 표시
- target host/port configured 여부 표시
- token/password/secret value가 표시되지 않는다는 redaction proof를 smoke로 확인
- 최신 package 기준 installed account login, browser session, noVNC streaming smoke 재실행
- installed Web/TUI/CLI current-card와 ops summary/current evidence anchor 연결

금지 범위는 다음이다.

- account file schema 변경
- JWT signing/refresh semantics 변경
- role/capability model 변경
- `console.view`, `operate`, `job.control` 의미 변경
- no-default-account/bootstrap 정책 변경
- noVNC bridge behavior 확장
- noVNC target 설정 mutation
- account/RBAC 수정 버튼
- token/JWT 재발급 버튼
- service config mutation
- guest command execution
- Web/TUI direct control mutation

## Console Access Card 공통 모델

Web/TUI/CLI는 같은 Console Access Card projection을 소비한다. 각 surface는 표현 방식만 다르게
가져가며, 상태 의미를 다르게 해석하지 않는다.

최소 필드는 다음이다.

| Field | 의미 |
| --- | --- |
| `account_status` | account auth 사용 가능 여부 또는 disabled 상태 |
| `session_status` | 현재 session이 valid/required/unknown인지 |
| `permission_status` | `console.view` 등 필요한 permission 충족 여부 |
| `console_handoff_status` | Windows console handoff 가능 여부 |
| `novnc_status` | `available`, `not_configured`, `disabled`, `target_required` 중 하나 |
| `novnc_reason_code` | 운영자가 원인을 알 수 있는 stable reason code |
| `target_configured` | explicit noVNC target host/port 구성 여부 |
| `target_scope` | loopback/non-loopback/LAN gate 상태 |
| `secret_display_status` | token/password/secret value가 표시되지 않음을 나타내는 상태 |
| `next_action` | 짧은 운영자 문구 |

초기 reason code 후보는 다음이다.

```text
available
not_configured
disabled
target_required
session_required
permission_required
lan_exposure_gate_required
secret_not_displayed
```

## Surface별 표시 원칙

### Web Console

Web Console은 VM console 영역 또는 VM detail 내 Console Access Card에 account/session,
permission, console handoff, noVNC 상태를 함께 표시한다.

표시 원칙은 다음이다.

- 카드 안에 긴 설명문을 넣지 않는다.
- 상태는 scan 가능한 짧은 label과 reason code로 표시한다.
- action button은 넣지 않는다.
- noVNC target 설정, account/RBAC 수정, token/JWT 재발급으로 이어지는 mutation을 열지 않는다.
- 상세 절차 링크는 제품 화면이 아니라 docs/evidence에서 다룬다.

### TUI

TUI는 선택 VM detail 또는 smoke snapshot에 Console Access Card projection을 표시한다.

표시 원칙은 다음이다.

- 키보드 중심 흐름을 깨지 않는다.
- noVNC와 console 상태를 별도 문장으로 길게 설명하지 않고 compact row/detail로 표시한다.
- target 미설정, session 필요, permission 필요 상태를 smoke assertion 가능한 문자열로 남긴다.
- 설정 mutation key binding은 추가하지 않는다.

### CLI

CLI는 `pcvcli vm console <vm>`와 `pcvcli vm vnc <vm>` 출력에서 noVNC availability와 reason을
구조화해서 표시한다.

표시 원칙은 다음이다.

- table/json/plain/csv format에서 같은 필드를 제공한다.
- `ok=True`만 출력하고 실제 원인을 숨기지 않는다.
- noVNC가 미설정이면 `not_configured`와 짧은 `next_action`을 표시한다.
- token/password/secret literal은 출력하지 않는다.

## 짧은 운영자 문구

제품 surface에는 짧은 운영자 문구만 둔다. 문서 링크는 넣지 않는다.

후보 문구는 다음이다.

- `noVNC target is not configured`
- `Configure noVNC target host/port`
- `Session required for console access`
- `Permission console.view required`
- `Secrets are not displayed`

문구는 Web/TUI/CLI에서 같은 의미로 쓰되, 각 surface의 layout에 맞게 줄바꿈 또는 label만 다르게 할 수
있다.

## Evidence Gate

Phase 1은 product payload 변경으로 취급한다. 따라서 전체 package chain이 필요하다.

필수 gate는 다음이다.

1. code-level tests
2. 새 admin-smoke package build
3. installed account login smoke
4. target-backed noVNC streaming smoke
5. installed Web/TUI/CLI current-card smoke
6. full admin host mutation gate
7. manual-admin package-pair closure
8. current ledger 갱신
9. public-boundary CI guard

## Acceptance Criteria

Phase 1은 아래 조건을 모두 만족해야 PASS다.

1. Web Console에서 account/session 상태와 noVNC configured/disabled reason이 보인다.
2. TUI smoke에서 선택 VM console/noVNC handoff 상태가 snapshot에 남는다.
3. CLI `vm console` / `vm vnc`가 noVNC availability와 reason을 구조화 출력한다.
4. installed account login smoke가 token/password value exposure 없이 PASS한다.
5. target-backed noVNC streaming smoke가 frame echo/hash match로 PASS한다.
6. current-card 또는 ops summary가 최신 account/noVNC evidence anchor를 노출한다.

## Not In Scope

- noVNC bridge behavior 변경
- noVNC target self-service 설정
- account/RBAC/JWT 모델 변경
- guest-exec 또는 guest channel
- Hyper-V QoS mutation
- Web/TUI direct control
- Linux Single Runtime Object 계열
- public trusted signing 또는 external stable publication

## What Already Exists

현재 재사용해야 할 기존 구현과 evidence는 다음이다.

- `src/DesktopNode.Api/DesktopNodeAccountAuth.cs`
- `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`
- `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- `src/DesktopNode.Api/ApiHandlerAdapterContract.cs`
- `src/DesktopNode.Contracts/RuntimePolicy.cs`
- `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs`
- `docs/ga-ready/evidence/installed-account-login-novnc-bridge-code-level-2026-05-10.md`
- `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`
- `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-17-04229.md`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`

## Design Review Result

Initial score: `6/10`

Final score: `9/10`

결정으로 개선된 부분은 다음이다.

- Web/TUI/CLI 공통 Console Access Card 모델로 information architecture를 통일했다.
- account/session과 noVNC 상태를 같은 운영자 여정으로 묶었다.
- action button 없이 status와 next action copy만 제공해 slice 범위를 유지했다.
- noVNC opt-in boundary와 account/RBAC/JWT 동결을 명확히 했다.
- product payload 변경으로 보고 전체 release/evidence gate를 요구했다.

Unresolved decision: `none`

## 다음 단계

다음 lifecycle 단계는 `superpowers:writing-plans`다. writing-plans는 이 문서의 Console Access
Card model, forbidden scope, evidence gate, acceptance criteria를 구현 checklist로 분해해야 한다.
