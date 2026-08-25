# PCVCLI Diagnostics List와 Console Capabilities 경계 설계

## 상태

- 날짜: 2026-08-12
- 결정: 승인됨
- 범위: Desktop Node의 기존 Local API를 PCVCLI와 운영 문서에 연결하는 후속 작업

## 문제

Desktop Node backend와 Web Console은 diagnostic bundle 목록 API와 전역 console capability discovery API를 이미 사용한다. PCVCLI는 diagnostic bundle 생성과 다운로드, VM별 console/noVNC handoff를 제공하지만 다음 두 전역 조회 기능은 독립 명령으로 제공하지 않는다.

- `GET /api/v1/diagnostics/bundles?limit=&offset=`
- `GET /api/v1/console/capabilities`

두 route를 모두 기계적으로 CLI에 노출하면 API parity는 높아지지만, console discovery는 기존 VM별 CLI handoff 및 runtime policy와 역할이 겹친다. 반대로 둘 다 API/Web 전용으로 두면 diagnostic bundle의 생성, 목록, 다운로드 자동화 흐름이 CLI에서 완결되지 않는다.

## 결정

### Diagnostic bundle 목록

PCVCLI에 다음 명령을 추가한다.

```text
pcvcli diagnostics bundle list [--limit N] [--offset N]
```

명령은 `GET /api/v1/diagnostics/bundles`에 대응한다. `--limit`과 `--offset`이 있으면 각각 동일 이름의 query parameter로 전달하며, 둘 다 있으면 `limit`, `offset` 순서로 직렬화한다.

```text
pcvcli diagnostics bundle list
  -> GET /api/v1/diagnostics/bundles

pcvcli diagnostics bundle list --limit 25 --offset 50
  -> GET /api/v1/diagnostics/bundles?limit=25&offset=50
```

CLI는 두 option의 정수 형식을 기존 `job list`와 같은 방식으로 검증한다. 지원 범위인 `limit=1..100`, `offset>=0`과 해당 `PCV_DIAGNOSTIC_BUNDLE_LIST_*` 오류는 API가 단일 진실로 유지한다. 응답 렌더링은 기존 공통 JSON/table/plain/csv 경로를 그대로 사용하며 diagnostics 전용 출력 모델을 추가하지 않는다.

목록 API는 read route이지만 조회 과정에서 기존 retention 정책을 적용한다. 기본 14일 또는 최대 50개를 초과한 bundle 파일은 diagnostics root에서 제거될 수 있다는 점을 CLI 문서에 명시한다. 이 동작은 Hyper-V, VM, service, MSI, firewall, trust-store 또는 reboot mutation을 실행하지 않는다.

### Console capabilities

`GET /api/v1/console/capabilities`는 API/Web Console 전용 discovery surface로 유지한다. `pcvcli console capabilities` 같은 새 top-level command group은 추가하지 않는다.

이 route는 `console-access-card.v1` 전역 카드에 필요한 다음 정보를 제공한다.

- local Hyper-V `vmconnect` handoff 가능 상태
- optional noVNC bridge와 WebSocket path template
- `console.view` 권한, token redaction 및 운영자 next action

CLI 운영자는 이미 다음 두 경로로 필요한 정보를 얻는다.

- `pcvcli runtime policy`: 전역 runtime/console 정책 요약
- `pcvcli vm console <vm>` 또는 `pcvcli vm vnc <vm>`: 선택 VM의 실제 session/handoff metadata

따라서 전역 capability card를 CLI에 중복 노출하지 않는다. 문서에는 이를 backend 미구현이 아니라 의도적인 surface ownership 결정으로 명시한다. CLI의 VM별 console 명령은 GUI나 browser stream을 자동 실행하지 않는 현재 계약을 유지한다.

## 코드 변경

`DesktopNodeCliCommandCatalog`의 diagnostics 하위 명령에 `list`를 추가한다. 구현은 기존 `JobList`의 option parsing 및 query 조립 패턴을 따른다. API handler, response schema, authorization 또는 retention 구현은 변경하지 않는다.

사용법 문자열은 다음 형태로 확장한다.

```text
pcvcli diagnostics bundle list [--limit N] [--offset N]
pcvcli diagnostics bundle create
pcvcli diagnostics bundle download <bundle_id> --output <path>
```

Interactive shell은 기존 usage expansion을 통해 새 leaf command를 발견해야 한다. 별도 interactive execution path는 추가하지 않는다.

## 오류 처리

- 알 수 없는 diagnostics 하위 명령은 `PCV_CLI_USAGE`와 `list|create|download` 사용법을 반환한다.
- `--limit` 또는 `--offset` 값이 정수가 아니면 기존 `ParseInt` 계약의 `PCV_CLI_USAGE`를 반환한다.
- 정수이지만 API 허용 범위를 벗어나면 API가 기존 `PCV_DIAGNOSTIC_BUNDLE_LIST_LIMIT_OUT_OF_RANGE` 또는 `PCV_DIAGNOSTIC_BUNDLE_LIST_OFFSET_OUT_OF_RANGE` 응답을 반환한다.
- `console capabilities`는 새 명령으로 등록하지 않으므로 기존 unknown command group 사용법 오류를 유지한다.

## 문서 변경

다음 문서를 하나의 surface 계약으로 동기화한다.

- `docs/CLI_COMMAND_USAGE.md`: list 명령, pagination, permission, retention side effect, 예제와 console API/Web-only 결정을 설명한다.
- `docs/USER_GUIDE.md`: 운영자 diagnostic 목록 CLI 흐름과 console surface 구분을 설명한다.
- `docs/USER_FEATURE_USAGE_SPEC.md`: Diagnostics list의 CLI 열을 새 명령으로 변경하고 console capability discovery는 API/Web 전용으로 유지한다.
- `src/DesktopNode.Cli/README.md`: 지원 명령 목록 및 backend discovery route 소유권을 갱신한다.

## 테스트

TDD 순서로 다음 계약을 고정한다.

1. `diagnostics bundle list`가 query 없는 GET route로 매핑된다.
2. `--limit 25 --offset 50`이 결정적인 query 순서로 매핑된다.
3. 잘못된 정수 option이 usage 오류가 된다.
4. CLI usage와 interactive help에 diagnostics list leaf command가 나타난다.
5. CLI usage에는 `pcvcli console capabilities`가 나타나지 않는다.
6. 문서 계약 테스트가 diagnostics list의 CLI 노출과 console capabilities의 API/Web 전용 결정을 함께 검증한다.

API 동작은 기존 API diagnostics 및 console test suite가 소유하므로 backend production code를 변경하거나 중복 테스트하지 않는다.

## 완료 조건

- `pcvcli diagnostics bundle list`와 pagination 조합이 예상 Local API request를 생성한다.
- 기존 create/download 명령과 출력 형식이 회귀하지 않는다.
- 전역 `console capabilities`는 CLI command catalog에 추가되지 않는다.
- CLI 사용 문서, 사용자 가이드, 기능 명세 및 CLI README가 동일한 결정을 표현한다.
- CLI test suite, 관련 API test suite와 문서 계약 검증이 모두 통과한다.
- public trusted signing 또는 external stable publication에 대한 새 claim을 만들지 않는다.

## 비목표

- diagnostics API response schema나 retention 정책 변경
- diagnostics 전용 CLI table renderer 추가
- console process, `vmconnect` 또는 noVNC browser 자동 실행
- `console capabilities` CLI 명령 추가
- account/RBAC/permission 모델 변경
- 새 product package, MSI 또는 host-mutation campaign 개시
