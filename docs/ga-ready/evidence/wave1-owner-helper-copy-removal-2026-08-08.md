# wave 1 소유자 helper 사본 제거 (2026-08-08)

evidence_id: `wave1-owner-helper-copy-removal-2026-08-08`
result: `PASS`
evidence_scope: `source-deduplication-with-il-level-ownership-guard`
host_mutation_performed: `false`
guest_command_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

계획서: `docs/followup-work-plan-2026-08-07.md` §2
선행 evidence: `docs/ga-ready/evidence/api-request-processor-decomposition-2026-08-06.md` §8.2

2026-08-06 분해가 `DesktopNodeApiResponseFactory`와 `DesktopNodeApiRequestParsing`을 만들면서도
wave 1 소유자에 남겨 둔 helper 사본을 제거한다. 사본이 남아 있는 동안 정규 버전을 고쳐도 세
소유자는 옛 동작을 유지하므로, "떼어냈다"는 단언이 실제로는 성립하지 않는 상태였다.

## 1. 결과

| 항목 | 착수 | 종료 |
| --- | ---: | ---: |
| 제거한 helper 사본 | `14` | `0` |
| `DesktopNodeApiAuthSessionHandler.cs` | `276`줄 | **`189`줄** |
| `DesktopNodeApiDiagnosticsHandler.cs` | `565`줄 | **`523`줄** |
| `DesktopNodeApiOpsSummaryHandler.cs` | `156`줄 | **`133`줄** |
| `DesktopNode.Api` 전체 | `7,927`줄 | **`7,775`줄** |
| 솔루션 테스트 | `856` | **`857`** |

착수 기준선 `856`은 `0ad320dd`에서 직접 측정했다. 인용이 아니다. 증가분 `1`은 §4의 guard다.
`DesktopNode.Api` 전체가 `152`줄 줄었다 — 2026-08-06 분해가 만든 `292`줄 순증의 절반이
사본 비용이었다는 뜻이다.

## 2. 계획서 §2.2 대조표의 정정 — 사본은 `11`개가 아니라 `14`개였다

계획서는 제거 대상을 `11`개로 적었다. **착수 시 다시 세었더니 `14`개다.** 계획서가 빠뜨린
`3`개는 아래와 같고, 셋 다 정규 버전과 관측 동작이 같아 함께 제거했다.

| 사본 | 위치 | 계획서 누락 사유 | 정규 버전과의 차이 |
| --- | --- | --- | --- |
| `JsonFromObject` | `DesktopNodeApiDiagnosticsHandler.cs:482` | 표에 열거되지 않았다 | 지역 `SerializeResponsePayload`를 경유. 출력 동일 |
| `JobData` | `DesktopNodeApiOpsSummaryHandler.cs:98` | `DesktopNodeApiOpsSummaryQuery` 소속이다. 계획서는 handler 클래스만 훑었다 | **없음** |
| `EmptyObject` | `DesktopNodeApiOpsSummaryHandler.cs:103` | 위와 같다 | auth 쪽 `EmptyObject`와 동일한 `SerializeToElement` 구현 |

`JobData`와 `EmptyObject`는 파일 이름과 클래스 이름이 어긋나는 자리에 있었다
(`DesktopNodeApiOpsSummaryHandler.cs` 안의 `DesktopNodeApiOpsSummaryQuery` 클래스). 파일 단위가
아니라 클래스 단위로 세지 않으면 다음에도 같은 자리를 놓친다.

이것은 계획서 부록이 요구한 "수치를 인용하지 말고 다시 측정한다"가 실제로 값을 찾아낸 `5`번째
사례다. 앞선 `4`건은 계획서 부록 말미에 열거돼 있다.

## 3. `EmptyObject` 등가성은 추론이 아니라 측정으로 닫았다

계획서 §2.2는 `EmptyObject`를 "유일하게 실제 확인이 필요한 항목"으로 지목했다. 사본은
`JsonSerializer.SerializeToElement(...)`, 정규 버전은 `JsonFromObject(...)`
(= `JsonDocument.Parse` + `Clone`)로 **같은 코드가 아니다.**

임시 xUnit probe로 두 구현의 관측값을 직접 비교했고, 아래가 모두 일치했다.

- `ValueKind` (`Object`)
- `GetRawText()` (`{}`)
- 속성 개수 (`0`)
- `RuntimePolicyContract.JsonOptions`로 재직렬화한 문자열
- 반환 후 `Clone()` 접근 — 두 구현 모두 backing document를 스스로 소유하므로
  `ObjectDisposedException`이 나지 않는다

`JobData`도 같은 probe로 확인했다(정규 버전과 문자 단위로 동일한 코드이므로 자명하나, §2에서
새로 발견된 사본이라 함께 측정했다). probe는 측정 후 삭제했다 — 이 절이 그 결과를 소유한다.

## 4. guard와 비공허 측정

`ApiRequestProcessorDecompositionOwnershipTests`에
`Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies` `1`건을 추가했다. 네 타입
(`DesktopNodeApiAuthSessionHandler`, `DesktopNodeApiDiagnosticsHandler`,
`DesktopNodeApiOpsSummaryHandler`, `DesktopNodeApiOpsSummaryQuery`)이 `Json` / `Body` / `Failure` /
`SerializeResponsePayload` / `TryParseBody` / `EmptyObject` / `JobData` / `JsonFromObject`를
선언하지 않음을 단언하고, 도착지 `2`종이 그것을 실제로 갖고 있음도 함께 단언한다. 도착지를
확인하지 않으면 사본 제거와 기능 삭제를 구분할 수 없다.

`ParsedJson`은 메서드가 아니라 중첩 record로 복사돼 있었으므로 메서드 이름 단언으로는 잡히지
않는다. `AssertTypeDeclaresNoNestedType`을 새로 만들어 따로 막았다.

**비공허를 `2`회 측정했다.** 단언 경로가 둘이므로 각각 확인했다.

| 되살린 사본 | 결과 | 실패 지점 |
| --- | --- | --- |
| `DesktopNodeApiOpsSummaryHandler.Json` (메서드) | 실패 `1` / 통과 `14` | `AssertTypeDoesNotDeclare` |
| `DesktopNodeApiAuthSessionHandler.ParsedJson` (중첩 record) | 실패 `1` / 통과 `14` | `AssertTypeDeclaresNoNestedType` |

두 경우 모두 `Wave1OwnersDoNotCarryTheirOwnResponseHelperCopies` **하나만** 실패했다. 두 번째
측정은 중첩 타입 단언이 없었다면 `ParsedJson` 재도입이 조용히 통과했으리라는 것도 함께 보여
준다 — 새 helper가 제 몫을 한다는 근거다.

되살린 사본은 측정 직후 되돌렸다. 최종 트리에는 없다.

## 5. 유지한 `2`종

| helper | 위치 | 유지 사유 |
| --- | --- | --- |
| `AuthValidationFailure` | `DesktopNodeApiAuthSessionHandler` | `DesktopNodeAuthValidationResult`를 받는 auth 고유 wrapper. 정규 버전에 대응물이 없다 |
| `AuthResult` | `DesktopNodeApiAuthSessionHandler` | `DesktopNodeAuthActionResult`를 받는 auth 고유 wrapper. 정규 버전에 대응물이 없다 |

둘 다 정규 버전과 **이름이 겹치지 않으므로** 네 번째 진실 원본을 만들지 않는다. 본문은
정규 `Json`/`Body` 위로 옮겼다.

## 6. 계획서 범위를 넘어 함께 처리한 것

`DesktopNodeApiOpsSummaryHandler.TryHandle`이 응답 봉투를 `Body(...)` 호출이 아니라
`SortedDictionary` 리터럴로 직접 만들고 있었다. 키 `4`종(`data`/`error`/`ok`/`operation`)과 값이
`Body(true, "ops.summary", data, null)`와 동일하므로 정규 호출로 바꿨다. 선언된 사본은 아니지만
같은 중복이고, 남기면 guard가 통과하는 채로 인라인 사본이 남는다.

## 7. 검증

`Invoke-PcvDevelopmentVerification.ps1`을 `2`회 실행했다. 두 번 모두 `7`개 suite 전부 passed이고
`failed_suite`는 비어 있다.

| # | 변경 경로 | 요청 | 확정 등급 | 실행 레인 | artifact |
| ---: | --- | --- | --- | --- | --- |
| 1 | 소스 `4`종만 | `Full` / `M` | `M` | `Full` | `artifacts/development-verification-full-helper-copy-removal` |
| 2 | 소스 `4`종 + 문서 `4`종 | `Full` / `M` | **`L`** | **`Release`** | `artifacts/development-verification-full-helper-copy-removal-with-docs` |

| suite | 결과 (양쪽 동일) |
| --- | --- |
| `dotnet` | passed — `857` / 실패 `0` |
| `web-npm` | passed |
| `packaging-pester` | passed — `48`개 파일 `485`건 |
| `installer-pester` | passed — `49`건 |
| `web-pester` | passed — `49`건 |
| `git-diff-check` | passed |
| `current-evidence-check` | passed — 생성 대상 `8`종 모두 `current` |

### 7.1 등급이 `M`에서 `L`로 올라간 이유

소스만 보면 `api-cli-web-contract` / `cross-module-change`로 `M`이다. 그러나 이 evidence를
`docs/ga-ready/EVIDENCE_INDEX.md`에 등재하는 순간 `current-evidence-anchor`가 붙어 `L`이 되고,
`Invoke-PcvDevelopmentVerification`이 `promotion_reason=tier-l-requires-release`로 레인을
`Release`까지 자동 승격했다. 승격된 레인으로 실행해 통과했다.

**등급이 `L`이라는 것이 anchor를 바꿨다는 뜻은 아니다.** 분류는 경로 기반이고,
`EVIDENCE_INDEX.md`는 생성 블록과 손으로 쓴 절을 함께 갖는 파일이다. 이번 편집은 새 절 추가뿐이며
생성 블록을 건드리지 않았다. `current-evidence-check`가 생성 대상 `8`종 모두 `current`임을
확인하는 것이 그 근거다. `Release` 레인은 분류 문서가 적은 대로 비변경 preflight이며, package
build·설치본 변경·host mutation 권한을 부여하지 않는다.

### 7.2 롤백 경계

`L` 등급이 요구하는 항목이다. 이 변경은 소스 `4`종과 문서 `4`종의 편집뿐이므로 revert로 완전히
되돌아간다. 설치본, 서비스 상태, 레지스트리, 인증서, 방화벽, job store 어느 것도 건드리지 않는다.
operational evidence는 필요하지 않다 — 이 작업은 code-level이고 anchor를 승격하지 않는다.

`module-size-ratchet.json`은 갱신하지 않았다. 이번에 줄어든 `3`개 파일은 라쳇 등록 대상이 아니고,
등록된 `6`개 모듈은 이 작업에서 바뀌지 않았다. `PcvModuleSizeRatchet.Tests.ps1` `3`건이 통과한다.

## Nonclaims

- 설치본을 만들지 않았고 operational anchor를 승격하지 않는다. `0.42.70-admin-smoke` 그대로다.
- 동작 변경을 주장하지 않는다. 순수 중복 제거이며 §3이 유일하게 구현이 달랐던 지점을 측정으로
  닫았다.
- 성능 개선을 주장하지 않는다. 측정하지 않았다.
- `web/src/served-app.ts` 분해(계획서 §1)는 범위 밖이며 손대지 않았다.
- public trusted signing과 external stable publication은 범위 밖이며 주장하지 않는다.
