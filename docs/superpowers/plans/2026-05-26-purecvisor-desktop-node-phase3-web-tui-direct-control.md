# Phase 3 Web/TUI Direct Control Implementation Plan

**Goal:** ADR-0008 Hyper-V QoS preview/apply contract를 Web Console과 TUI 운영자 surface에
제품화하고, Guest Execution/account-noVNC mutation은 문서화된 security boundary 안에 유지한다.

## 1. Test Contract First

- [x] Web static test에 QoS direct control route, form action, deferred security boundary assertion을 추가한다.
- [x] TUI API client test에 QoS preview/apply route와 JSON body assertion을 추가한다.
- [x] TUI application test에 preview는 preview route만 호출하고, apply는 confirmation 전 mutation을 보내지 않는다는 assertion을 추가한다.
- [x] TUI renderer test에 direct-control help/readback lines를 추가한다.

## 2. Web Console

- [x] `PcvRouteRegistry`와 `PcvDesktopApi`에 `vmQosStoragePreview`, `vmQosStorage`, `vmQosNetworkPreview`, `vmQosNetwork`를 추가한다.
- [x] selected VM readback panel 아래에 storage/network QoS direct control form을 추가한다.
- [x] Preview submit은 preview route만 호출하고 결과를 화면 상태로 남긴다.
- [x] Apply submit은 confirmation 뒤 apply route를 호출하고 returned job을 existing job tracker에 연결한다.
- [x] Guest Execution은 ADR-0009 boundary contract 적용 후에도 route 구현 전 disabled로 표기하고, account/noVNC target mutation은 ADR 보류 상태로 표기한다.

## 3. TUI

- [x] `P` key를 selected VM QoS reset preview intent로 매핑한다.
- [x] `A` key를 selected VM QoS reset apply confirmation intent로 매핑한다.
- [x] Preview는 storage/network preview route를 호출하고 readback panel에 결과를 표시한다.
- [x] Apply는 confirmation 뒤 storage/network apply route를 호출한다.
- [x] Help overlay와 VM readback panel에 direct-control boundary를 표시한다.

## 4. Evidence

- [x] generated `web/app.js`를 staged source에서 재생성한다.
- [x] Web/TUI unit/static tests를 실행한다.
- [x] `docs/ga-ready/evidence/phase3-web-tui-qos-direct-control-code-level-2026-05-26.md`를 작성한다.
- [x] `CURRENT_EVIDENCE_LEDGER.md`, `EVIDENCE_INDEX.md`, `ADR_INDEX.md`, `web/DESIGN.md`를 갱신한다.

## 5. Release Follow-up

- [x] Product payload change로 `0.42.48-admin-smoke` package chain을 연다.
- [x] 새 package 기준 installed Web/TUI/CLI current-card smoke를 재실행한다.
- [x] 새 package 기준 full admin host mutation gate를 닫는다.
- [ ] `0.42.47-admin-smoke -> 0.42.48-admin-smoke` manual-admin package-pair campaign을
  별도 gate로 닫는다.
