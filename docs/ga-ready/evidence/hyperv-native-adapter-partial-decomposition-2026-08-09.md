# DesktopNodeHyperVNativeAdapter partial 도메인 분해 (2026-08-09)

evidence_id: `hyperv-native-adapter-partial-decomposition-2026-08-09`
result: `PASS`
evidence_scope: `source-decomposition-pure-partial-move`
host_mutation_performed: `false`
guest_command_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

선행 대형 모듈 분해: `DesktopNodeHostServiceAction`(2026-08-06),
`DesktopNodeApiRequestProcessor`(2026-08-06), `web/src/served-app.ts`(2026-08-08).
이번 작업은 Hyper-V provider domain orchestration 단일 파일 `2,038`줄을 같은 클래스의
`partial` 파일로만 나눈다. 공개 타입·시그니처·동작 변경을 주장하지 않는다.

## 1. 결과

| 파일 | 줄 수 | 소유 |
| --- | ---: | --- |
| `DesktopNodeHyperVNativeAdapter.cs` (core) | **450** | 생성자, 필드, dispatch table, `TryInvoke` 진입 |
| `DesktopNodeHyperVNativeAdapter.Reads.cs` | **570** | network inventory, host status, VM/checkpoint list·detail, stats, QoS/guest readback |
| `DesktopNodeHyperVNativeAdapter.Mutations.cs` | **765** | checkpoint mutation, power state, create/delete/rename, media, resource/QoS mutation |
| `DesktopNodeHyperVNativeAdapter.Guest.cs` | **81** | guest execution invoke |
| `DesktopNodeHyperVNativeAdapter.Shared.cs` | **207** | cancellation, name validation, JSON helpers, failure mapping |
| **합계** | **2,073** | 단일 파일 `2,038` 대비 `+35` (partial class 선언·using 복제) |

선행 단일 파일 상한은 `module-size-ratchet.json`의 `2,038`이었다. 분해 후 core 상한을
`450`으로 내리고, `500`줄을 넘는 신규 partial `2`종(Reads `570`, Mutations `765`)을 **생성
시점에 등록**했다. Guest `81`·Shared `207`는 `500`줄 미만이라 ratchet 신규 등록 대상이 아니다.

## 2. 순수 이동 범위

- `public sealed class` → `public sealed partial class` (core + 4 partial).
- 메서드 본문·접근 한정자·호출 순서·dispatch 키 매핑을 바꾸지 않았다.
- 새 타입, 새 인터페이스, 새 provider, 새 오류 코드를 추가하지 않았다.
- `IDesktopNodeHyperVNativeAdapter` 공개 표면과 호출자(`DesktopNode.Api` 등)는 수정하지 않았다.
- 일회성 분할 스크립트 `artifacts/_split_hyperv_adapter.py`는 측정 도구이며 product payload가
  아니다. 커밋 대상이 아니다.

partial 경계는 기존 private 메서드 그룹 경계를 따랐다.

| partial | 대표 진입 메서드 |
| --- | --- |
| core | 생성자 오버로드, dispatch handler map, `TryInvoke` |
| Reads | `TryInvokeNetworkInventory`, `TryInvokeHostStatus`, `TryInvokeVmList`, checkpoint list, stats, QoS/guest readbacks |
| Mutations | `TryInvokeCheckpointMutation`, power state, create/delete/rename, media, resource mutation |
| Guest | `TryInvokeGuestExecution` |
| Shared | `ThrowIfNativeCanceled`, `IsValidHyperVName`, JSON property helpers, structured failure helpers |

## 3. 검증

| 실제 실행 명령 | 관찰 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj` | 통과 `137`, 실패 `0` |
| `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvModuleSizeRatchet.Tests.ps1' -Output Detailed"` | Passed `3`, Failed `0` |
| 라인 수 측정 (Pester와 동일: CRLF→LF, trailing newline trim, split) | core `450`, Reads `570`, Mutations `765`, Guest `81`, Shared `207` |

이 작업은 product payload 동작 변경이 아니므로 package build, full admin host mutation,
installed current-card, manual-admin package-pair를 열지 않는다. operational anchor는
`0.42.71-admin-smoke` 그대로다.

## 4. Nonclaims

- 동작 변경·버그 수정·guest execution 기능 확장을 주장하지 않는다. 순수 파일 이동이다.
- public trusted signing 또는 외부 stable publication evidence가 아니다.
- host mutation, package install, installed surface 변경을 실행하거나 주장하지 않는다.
- `DesktopNodeHostServiceAction.cs`(`1,174`줄) 추가 분해는 범위 밖이다.
- Guest/Shared partial에 ratchet 상한을 두지 않은 것은 현재 줄 수가 `500` 미만이기 때문이며,
  이후 `slack_lines`를 넘어 커지면 신규 등록이 필요하다.
