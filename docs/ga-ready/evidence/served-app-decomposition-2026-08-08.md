# `web/src/served-app.ts` 도메인 분해 (2026-08-08)

evidence_id: `served-app-decomposition-2026-08-08`
result: `PASS`
evidence_scope: `source-decomposition-with-mechanically-proven-pure-move`
host_mutation_performed: `false`
guest_command_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

계획서: `docs/followup-work-plan-2026-08-07.md` §1
선행: `docs/project-status-audit-2026-08-05.md`가 기록한 대형 모듈 `2`종 중 프런트엔드 쪽.
백엔드 절반은 `docs/ga-ready/evidence/api-request-processor-decomposition-2026-08-06.md`가 닫았다.

## 1. 결과

| 항목 | 착수 | 종료 |
| --- | ---: | ---: |
| `web/src/served-app.ts` | `4,005`줄 | **`413`줄** (`-89.7`%) |
| 번들 part 수 | `6` | **`24`** |
| 가장 큰 part | `4,005` (`served-app.ts`) | **`422`** (`mutate.ts`) |
| 번들 part 합계 | `4,877`줄 | `4,905`줄 |
| `web/app.js` | `4,520`줄 | `4,566`줄 |
| 라쳇 상한 | `4,005` | **`413`** |

part 합계 `+28`줄과 `app.js` `+46`줄은 **전부 주석**이다. 내역은 §3과 §4가 소유한다.

## 2. 순수 이동을 기계로 증명했다

계획서 §1.3이 지목한 검증 수단이다. `build-served-asset.mjs`는 `module: ts.ModuleKind.None`으로
전체를 한 스코프에 담으므로, `app.js`는 모든 part의 선언을 하나의 최상위 스코프에 가진 단일
파일이다. 따라서 분해 전후 `app.js`를 대조하면 이동이 순수했는지가 드러난다.

임시 검사기를 만들어 TypeScript 파서로 양쪽 `app.js`의 최상위 선언을 추출하고, 이름으로 짝지어
**본문을 문자 단위로** 비교했다. leading trivia(주석)는 제외하고 본문만 본다.

```
baseline: 272 declarations, 1 bare statements
current : 272 declarations, 1 bare statements
PURE MOVE: every top-level declaration matches character-for-character.
```

검사기의 비공허도 실측했다. `app.js`에서 `WEB_ASSET_LABEL` 문자열 하나를 바꾼 사본을 넣으면
`BODY CHANGED: var WEB_ASSET_LABEL`로 실패한다.

추가로 텍스트 diff도 확인했다. `app.js`의 변경 줄은 예외 없이 주석이다 — `// --- <path> ---`
표식 `18`줄, `// @ts-nocheck` `19`줄, 헤더 주석 교체. **선언 순서조차 바뀌지 않았다.** 연속 구간을
원본 순서대로 잘랐기 때문이다.

검사기는 측정 후 삭제했다. 이 절이 결과를 소유한다.

## 3. 계획서에 없던 함정 (1) — `// @ts-nocheck`

**`served-app.ts`의 `1`행은 `// @ts-nocheck`였다.** `4,005`줄 UI layer 전체가 타입 검사에서
면제돼 있었고, 계획서 §1.7의 함정 목록에 이 항목은 없다.

분할하자 지시자는 원본 `1`행을 물려받은 `evidence.ts` 하나에만 남았고, 나머지 `17`개 part와
`served-app.ts`가 검사 대상이 되면서 `tsc --noEmit`이 **`328`건**을 보고했다.

| 오류 | 건수 |
| --- | ---: |
| TS7006 implicit any parameter | `159` |
| TS18047 possibly null | `85` |
| TS2339 property does not exist | `62` |
| 그 외 `8`종 | `22` |

원본이 깨끗했던 것은 코드가 타입 안전해서가 아니라 **검사를 받지 않았기 때문**이다. 원본 `6`개
part를 그대로 복원해 `tsc`를 돌리면 `exit 0`이고, 지시자를 뗀 조각들만 오류가 난다.

### 3.1 선택: part마다 지시자, tsconfig `exclude` 아님

두 방법이 같은 결과를 낸다. `exclude`는 선언이 한 곳이라 조용하지만, **그 디렉터리에 생기는
새 파일이 자동으로 검사 대상에서 빠진다.** part마다 `@ts-nocheck`를 두면 새 part는 기본으로
검사를 받고 면제는 명시해야 한다. 안전한 쪽으로 기울여 후자를 택했다.

`served-app.ts` 헤더에 그 취지를 적었다 — "새 part를 만들 때 지시자를 그대로 복사하지 말 것."

### 3.2 이 작업은 타입 부채를 갚지 않았다

`328`건은 고치지 않았다. 순수 이동의 범위를 벗어나고, 고치는 순간 §2의 증명이 성립하지 않는다.
**분해는 부채를 줄이지 않았고 `1`곳에 숨어 있던 것을 `19`곳에 드러냈을 뿐이다.** 타입을 실제로
붙이는 것은 별도 작업이며 이 evidence는 그것을 주장하지 않는다.

## 4. 계획서에 없던 함정 (2) — part 목록이 세 곳에 복제돼 있다

계획서 §1.7은 `build-served-asset.mjs`의 `servedSourceParts`만 경고했다. **같은 목록이 두 곳 더
있었고 둘 다 stale이 됐다.**

| 위치 | 역할 | 착수 시 상태 | 조치 |
| --- | --- | --- | --- |
| `scripts/build-served-asset.mjs` | 번들에 넣을 part | 계획서가 경고함 | 손으로 `18`개 추가 |
| `scripts/verify-static-parity.mjs` | parity 검사가 읽을 part | **경고 없음** | 손으로 `18`개 추가 |
| `web/tests/PcvDesktopWeb.Static.Tests.ps1` | Pester가 읽을 part | **경고 없음** | **빌드 스크립트에서 파생하도록 변경** |

두 번째가 stale이면 `renderOpsCockpit` / `renderIncidentCommand`처럼 새 part로 옮겨진 심볼을
`requireIncludes`가 보지 못해 `verify:parity`가 실패한다. 세 번째가 stale이면
`window.confirm(buildVmDeleteConfirmation` 단언이 실패한다. 둘 다 실제로 실패했고, 그래서
발견했다.

Pester 쪽은 네 번째 사본을 만들지 않고 `build-served-asset.mjs`를 정규식으로 읽어 파생시켰다.
파생이 단언을 약화시키지 않는다 — part가 빌드에서 빠지면 심볼이 번들과 테스트 시야에서 함께
사라지므로 검사는 **통과가 아니라 실패로 기운다.** 파생 결과가 `23`개이고 전부 디스크에
존재함을 실측했다.

`scripts/verify-static-parity.mjs`는 파생시키지 않았다. 그 파일은 빌드 스크립트 자체를 검사
대상으로 삼으므로, 검사 대상에서 기대값을 읽어 오면 검사가 자기 자신을 증명하는 형태가 된다.

## 5. 계획서 §1.4의 순서 제약은 실재하지 않았다

계획서는 top-level `const` `CONNECTION_STATE_LABELS`가 TDZ에 있으므로 배치 순서를 확인하라고
적었다. 실측 결과 **`served-app.ts`의 최상위 실행문은 마지막 `1`줄뿐**이다.

```js
document.addEventListener('DOMContentLoaded', init);
```

이 줄은 `init`을 **등록만** 하고 호출하지 않는다. 즉 스크립트 평가 중에는 어떤 함수도 실행되지
않으므로, `const`를 읽는 함수는 전부 번들 전체가 평가된 뒤에 돌아간다. `function` 선언 `217`개는
호이스팅되므로 순서와 무관하다. 순서 제약은 `0`개다.

그럼에도 part 순서는 원본 선언 순서를 그대로 유지했다. 동작 때문이 아니라 §2의 대조를 쉽게
만들기 위해서다. `served-app.ts`만 마지막에 둔다 — 위 실행문이 그 파일 끝에 있다.

## 6. 분해 축

원본이 이미 도메인별로 뭉쳐 있어 **연속 구간**으로 잘랐다. 경계 `18`곳이 모두 빈 줄이고 다음
줄이 함수 선언 시작임을 자르기 전에 확인했다. 연속 구간이라 이동이 기계적이고, 그래서 §2의
증명이 순서 차이조차 없이 나왔다.

| part | 원본 행 | 줄 | 소유 |
| --- | --- | ---: | --- |
| `served/evidence.ts` | `1-189` | `188` | evidence status/badge/issue |
| `served/summary.ts` | `190-298` | `110` | summary 판독, ops signal |
| `served/table.ts` | `299-375` | `78` | 검색·필터·정렬, pending action key |
| `served/rbac.ts` | `376-431` | `57` | auth 오류 판정, RBAC |
| `served/render-ops.ts` | `432-608` | `178` | refresh 실패 수집, metric, host, ops cockpit |
| `served/render-inventory.ts` | `609-749` | `142` | VM 목록, network inventory |
| `served/render-qos.ts` | `750-931` | `183` | checkpoint 목록, QoS readback/control |
| `served/render-vm-detail.ts` | `932-1048` | `118` | VM 상세, workbench context |
| `served/render-jobs.ts` | `1049-1156` | `109` | job 행·필터·버튼 |
| `served/render-monitoring.ts` | `1157-1379` | `224` | monitoring signal, evidence dashboard |
| `served/render-panels.ts` | `1380-1606` | `228` | registry bridge, host ops, 진단 번들, token, account |
| `served/render-console.ts` | `1607-1822` | `217` | console panel, beta follow-up, troubleshooting |
| `served/render-activity.ts` | `1823-2076` | `255` | 실패 job triage, incident, event center, activity |
| `served/render-shell.ts` | `2077-2426` | `351` | status bar, 연결 상태, UI 설정, command palette, view 라우팅 |
| `served/load.ts` | `2427-2689` | `264` | 조회 loader |
| `served/mutate.ts` | `2690-3110` | `422` | queued mutation과 payload 판독 |
| `served/job-polling.ts` | `3111-3322` | `213` | refreshAll, VM 생성, job 추적·폴링 |
| `served/actions.ts` | `3323-3604` | `283` | 진단 번들·account·console·browser state·이동 |
| `served-app.ts` (잔존) | `3605-4005` | `413` | 이벤트 배선, 초기화, DOMContentLoaded |

계획서 §1.5는 `8`개를 출발점으로 제시하고 "실제 함수 목록을 다시 세어 조정한다"고 적었다.
연속 구간을 우선해 `18`개가 됐다. 각 part는 `500`줄 미만이다.

## 7. 검증

| 항목 | 결과 |
| --- | --- |
| §2 순수 이동 대조 | 선언 `272`개 문자 단위 일치, 최상위 실행문 불변 |
| `npm test` | 통과 (`tsc --noEmit` + `check:served` + `check:frontend-batches`) |
| `npm run verify:parity` | 통과 (served 자산, parity manifest, 정적 parity, browser fixture) |
| `Invoke-Pester web/tests` | **`49`/`49`** 통과 |
| Full 레인 `7`개 suite | 전부 passed, `ok=true`, `change_tier=M` |

Full 레인 artifact는 `artifacts/development-verification-served-app-decomposition`이고
`tier_reasons`는 `packaging-contract` / `api-cli-web-contract` / `cross-module-change`다.

`module-size-ratchet.json`의 `web/src/served-app.ts` 상한을 `4,005`에서 실측값 `413`으로 내렸다.
라쳇 규칙이 `slack_lines=50` 이상 줄면 하향을 요구한다. 신규 part 중 `500`줄을 넘는 것은 없으므로
신규 등록은 하지 않았다 — 최대가 `mutate.ts` `422`줄이다. `PcvModuleSizeRatchet.Tests.ps1` `3`건이
통과한다.

## Nonclaims

- 동작 변경을 주장하지 않는다. §2가 순수 이동임을 기계로 증명했고 예외는 `0`건이다.
- **타입 안전성 개선을 주장하지 않는다.** §3.2대로 UI layer는 여전히 타입 검사를 받지 않는다.
  이 작업은 그 사실을 `1`곳에서 `19`곳으로 드러냈을 뿐 부채를 갚지 않았다.
- 성능 개선을 주장하지 않는다. 측정하지 않았다. 번들 산출물은 주석 `46`줄만 늘었다.
- 설치본을 만들지 않았고 operational anchor를 승격하지 않는다. `0.42.70-admin-smoke` 그대로다.
- 실제 브라우저 조작 QA를 수행하지 않았다. `verify-browser-fixture.mjs`는 fixture 검사이며
  설치본 Web Console 조작 evidence가 아니다.
- public trusted signing과 external stable publication은 범위 밖이며 주장하지 않는다.
