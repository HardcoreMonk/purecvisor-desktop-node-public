# ADR-0011: CLI와 Web Console 전용 운영자 표면

상태: 적용 중
일자: 2026-07-14

## 결정 마커

```text
DESKTOP_NODE_OPERATOR_SURFACE_DECISION: cli-web-only
DESKTOP_NODE_TUI_DECISION: removed
```

## 맥락

Desktop Node의 운영자 기능은 Local API와 backend contract를 중심으로 Web Console과
PCVCLI에 모두 제공된다. 별도 TUI는 같은 API 기능을 다시 투영하면서 source, package,
smoke, 문서 계약을 중복시켰다.

`0.42.62-admin-smoke` 설치본 current-card는 당시 포함된 Web/TUI/CLI를 검증한 dated
evidence다. 이 사실과 이전 TUI evidence는 predecessor로 보존하지만 현재 제품 표면을
정의하지 않는다.

## 결정

- 활성 운영자 표면은 Web Console과 `pcvcli.exe`다.
- Web Console은 기본 대화형 운영 표면이고 PCVCLI는 terminal automation, JSON 출력,
  반복 운영의 표면이다.
- TUI source, tests, package payload, installed smoke runner, active current 문서 계약을
  제거한다.
- Local API, job runtime, Hyper-V provider와 다른 backend 기능은 유지한다. TUI 제거를
  backend 기능 제거로 해석하지 않는다.
- dated TUI evidence, spec, plan은 historical predecessor로 보존하며 재작성하지 않는다.
- TUI 호환 shim이나 `pcvtui.exe` 대체 launcher는 제공하지 않는다.

## 결과

- Product manifest는 schema version `2`를 유지하고 CLI/Web-only payload를 기록한다.
- MSI upgrade는 이전 설치본에 남은 `pcvtui.exe`를 정리해야 한다.
- TUI command 또는 key binding에 대한 compatibility surface는 없다. 같은 기능은 Web
  Console이나 PCVCLI를 사용한다.
- Source/code-level PASS만으로 설치본 승격을 주장하지 않는다. `0.42.63-admin-smoke`
  package, full admin host mutation, CLI/Web installed current-card 검증이 완료되어야 새
  installed anchor로 승격할 수 있다.

## 검증과 증거

- Code-level evidence:
  `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`
- Current evidence ledger:
  `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- `0.42.62-admin-smoke` installed evidence는 dated predecessor로 유지한다.
