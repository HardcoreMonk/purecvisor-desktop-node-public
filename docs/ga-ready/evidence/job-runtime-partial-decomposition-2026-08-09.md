# DesktopNodeJobRuntime partial 도메인 분해 (2026-08-09)

evidence_id: `job-runtime-partial-decomposition-2026-08-09`
result: `PASS`
evidence_scope: `source-decomposition-pure-partial-move`
host_mutation_performed: `false`
guest_command_performed: `false`
package_build_performed: `false`
installed_product_changed: `false`
secret_observed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

후속 큐의 대형 모듈 분해 연속 작업이다. Hyper-V NativeAdapter / HostServiceAction partial
분해 이후, Runtime 핵심 `DesktopNodeJobRuntime`(`1,466`줄)을 같은 클래스의 `partial`로 나눈다.

## 1. 결과

| 파일 | 줄 수 | 소유 |
| --- | ---: | --- |
| `DesktopNodeJobRuntime.cs` (core) | **116** | 필드, ctor, `CreateDefault`, `Create`, `Get`, `Snapshot` |
| `DesktopNodeJobRuntime.Commands.cs` | **469** | `Cancel`, `Retry`, `Reconcile`, `TryStartNext`, `Complete` |
| `DesktopNodeJobRuntime.Persistence.cs` | **643** | store snapshot write/load, retention, recovery |
| `DesktopNodeJobRuntime.Shared.cs` | **263** | policy/project/sanitize helpers, `MutableJob` |
| **합계** | **1,491** | 단일 파일 `1,466` 대비 `+25` (partial 선언·using 복제) |

`module-size-ratchet.json`:
- core 신규 등록 `max_lines=116`
- Persistence(`643` > 500) 생성 시점 등록
- Commands `469` / Shared `263`는 `500`줄 미만이라 신규 등록 대상 아님

## 2. 순수 이동

- `public sealed class` → `public sealed partial class`
- 공개 표면·시그니처·lock 순서·store commit 경로를 바꾸지 않았다
- 새 타입/오류 코드/정책 변경 없음

## 3. 검증

| 명령 | 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.Runtime.Tests/...` | 통과 `126`, 실패 `0` |
| `PcvModuleSizeRatchet` | 통과 (core `116`, Persistence `643`) |

## 4. Nonclaims

- package/fullgate/installed surface를 열지 않았다
- operational anchor는 `0.42.71-admin-smoke` 그대로다
- public trusted signing / external stable publication을 주장하지 않는다
