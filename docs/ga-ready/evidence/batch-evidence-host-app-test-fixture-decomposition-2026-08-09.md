# BatchEvidence / HostApplication / 대형 테스트 fixture 분해 (2026-08-09)

evidence_id: `batch-evidence-host-app-test-fixture-decomposition-2026-08-09`
result: `PASS`
evidence_scope: `source-decomposition-pure-partial-move-and-test-fixture-split`
host_mutation_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

후속 후보 `1` `BatchEvidenceSummaryReader`, `2` `DesktopNodeHostApplication`,
`3` 대형 테스트 fixture 정리를 한 세션에서 닫는다.

## 1. `BatchEvidenceSummaryReader` (1,350 → partial 4)

| 파일 | 줄 | 소유 |
| --- | ---: | --- |
| `BatchEvidenceSummaryReader.cs` | **112** | ctor, `Read` |
| `.Summary.cs` | **441** | available/route/OS/manual-admin/public-boundary summary builders |
| `.Pathing.cs` | **697** | artifact path enum, child resolve, redaction, repo root |
| `.Json.cs` | **127** | JSON scalar/array readers |

합계 `1,377` (`+27` partial 선언·using). ratchet: core `112`, Pathing `697`.

## 2. `DesktopNodeHostApplication` (859 → partial 4)

| 파일 | 줄 | 소유 |
| --- | ---: | --- |
| `DesktopNodeHostApplication.cs` | **201** | fields, ctor, `StartAsync`, `Dispose` |
| `.Request.cs` | **281** | accept/handle/admission/body |
| `.NoVnc.cs` | **161** | WebSocket bridge |
| `.StaticAuth.cs` | **252** | static files, authorize, CORS, helpers |

합계 `895` (`+36`). ratchet: core `201`.

## 3. 대형 테스트 fixture 정리

| 파일 | 착수 | 종료 | 내용 |
| --- | ---: | ---: | --- |
| `ApiRuntimePolicyRequestProcessorTests.cs` | 4,128 | **3,625** | 테스트 본체 |
| `.Fakes.cs` | — | **512** | Hyper-V recording adapters |
| `DesktopNodeHostServiceActionTests.cs` | 3,556 | **2,965** | 테스트 본체 |
| `.Fakes.cs` | — | **388** | fake controllers + helpers |
| `DesktopNodeHostServiceActionTestExtensions.cs` | (동일 파일 말미) | **216** | options builder extensions |

**의도적 최소 변경 1건:** `file static class DesktopNodeHostServiceActionTestExtensions`를
`internal static class`로 바꿨다. `file` 한정자는 **같은 소스 파일 안에서만** 보이므로
partial 분리 후에는 extension이 테스트 본체에서 보이지 않는다. 동작은 동일하고 가시성만
assembly-internal로 넓힌다.

## 4. 검증

| 명령 | 결과 |
| --- | --- |
| `dotnet test DesktopNode.Api.Tests` | 통과 `251`, 실패 `0` |
| `dotnet test DesktopNode.Host.Tests` | 통과 `198`, 실패 `0` |
| `PcvModuleSizeRatchet` | 신규 상한 포함 통과 |

## 5. Nonclaims

- product payload 동작 변경 없음 (extension 가시성 제외)
- package/fullgate/anchor 승격 없음 → `0.42.71-admin-smoke` 유지
- public trusted signing / external stable publication 미주장
