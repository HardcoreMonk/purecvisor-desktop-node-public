# DesktopNodeHostServiceAction static partial 분해 (2026-08-09)

evidence_id: `host-service-action-partial-decomposition-2026-08-09`
result: `PASS`
evidence_scope: `source-decomposition-pure-partial-move`
host_mutation_performed: `false`
guest_command_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

선행: `docs/ga-ready/evidence/host-service-action-decomposition-2026-08-06.md`가 Ops 도메인
왕복 제거로 `4,069` → `1,174`줄을 닫았다. 이번 작업은 남은 공유 표면을 같은 `static`
클래스의 `partial` 파일로만 나눈다.

## 1. 결과

| 파일 | 줄 수 | 소유 |
| --- | ---: | --- |
| `DesktopNodeHostServiceAction.cs` (core) | **563** | DTO record `11`종, `CreatePlan`, `ExecuteAsync` dispatch |
| `DesktopNodeHostServiceAction.ServiceConfig.cs` | **187** | `IsStopped`, `CreateServiceConfiguration`, binPath 인자 helper |
| `DesktopNodeHostServiceAction.Shared.cs` | **224** | failure/ownership/path/native-action 판별, firewall/trust spec |
| `DesktopNodeHostServiceAction.Token.cs` | **125** | protected token / account bootstrap 공개·internal 표면 |
| `DesktopNodeHostServiceAction.Commands.cs` | **103** | 비 native command `sc.exe` invoke / stop wait |
| **합계** | **1,202** | 단일 파일 `1,174` 대비 `+28` (partial 선언·using 복제) |

`module-size-ratchet.json` core 상한: `1,174` → **`563`**. 나머지 partial은 `500`줄 미만이라
생성 시점 신규 등록 대상이 아니다.

## 2. 순수 이동 범위

- `public static class` → `public static partial class`
- 공개 표면 불변: `CreatePlan`, `ExecuteAsync`(4 오버로드), `EnsureProtectedTokenFile`,
  `EnsureAccountAuthBootstrapFiles`
- Ops 호출 표면(`Require`, `IsOwnedService`, `NativeServiceFailure`, token write 등) 시그니처
  불변
- 새 타입·오류 코드·Ops 경계를 추가하지 않았다
- ownership guard `NoOpsForwarderRemainsOnHostServiceAction` /
  `HostServiceActionKeepsOnlyItsPublicSurface` 유지(타입 metadata 합산이 partial을 병합)

## 3. 검증

| 실제 실행 명령 | 관찰 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj` | 수정 후 전체 suite stress `10/10` PASS (동일 세션) |
| `pwsh ... PcvModuleSizeRatchet.Tests.ps1` | ceiling `563` 기준 통과 |
| 라인 수 측정 (Pester 동일 방식) | 위 표 |

## 4. Nonclaims

- Ops 도메인 재이동이나 boomerang 재도입을 하지 않았다
- package/fullgate/installed surface를 열지 않았다. operational anchor는
  `0.42.71-admin-smoke` 그대로다
- public trusted signing / external stable publication을 주장하지 않는다

관련 ServiceToken rotation IO 수정은 별도 evidence
`docs/ga-ready/evidence/service-token-rotation-replace-hardening-2026-08-09.md`가 소유한다.
