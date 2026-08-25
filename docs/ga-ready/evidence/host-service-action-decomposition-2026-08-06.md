# DesktopNodeHostServiceAction 도메인 분해 (2026-08-06)

evidence_id: `host-service-action-decomposition-2026-08-06`
result: `PASS`
evidence_scope: `code-level-refactor-no-host-mutation`
host_mutation_performed: `false`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 분해 결과

| 파일 | 착수 전 | 종료 후 |
| --- | ---: | ---: |
| `DesktopNodeHostServiceAction.cs` | `4,069` | `1,174` (71% 감소) |
| `Ops/` 9개 합계 | `199` | `3,040` |

`Ops/` 9개 도메인 클래스의 종료 후 라인 수는 TrustStore `132`, Firewall `133`,
DataRootLifecycle `250`, ServiceToken `266`, ServiceLifecycle `356`,
ConfigMigration `391`, EventLog `417`, JobStoreMigration `431`,
CredentialManager `664`다. dispatch 전용인 `DesktopNodeHostOpsCatalog.cs`(`176`줄)는
이 작업 대상이 아니므로 위 합계에 포함하지 않았다.

## 제거한 구조

`ExecuteAsync -> Ops.X.Execute -> DesktopNodeHostServiceAction.ExecuteNativeXActionForOps` 왕복을
도메인 `9`개 전부에서 제거했다. `*ForOps` forwarder는 `0`개다.
`NoOpsForwarderRemainsOnHostServiceAction` 테스트가 이를 기계로 잠근다.

## 불변으로 유지한 공개 표면

`CreatePlan`, `ExecuteAsync`(4 오버로드), `EnsureProtectedTokenFile`,
`EnsureAccountAuthBootstrapFiles`. 호출자 `Program.cs` `1`곳과 테스트 `69`곳은 수정하지 않았다.

## `DesktopNodeHostServiceAction`에 의도적으로 남은 cross-domain 공유 표면

아래 멤버는 forwarder가 아니라 여러 도메인이 공유하는 `internal` 헬퍼로, 계획이 금지한
"새 계층 생성" 없이 남긴 것이다. 완료하지 못해 남은 잔재가 아니라 의도적 설계 결정이다.

- 순수 공유 헬퍼 10개: `Require`, `IsOwnedService`, `NativeServiceFailure`, `IsStopped`,
  `IsSupportedMigrationPlan`, `OwnedFileExists`, `IsOwnedFileAccessFailure`,
  `CreateServiceConfiguration`, `ExtractNamedArgumentValue`,
  `UsesProtectedFileTokenSource`.
- 공개 token 표면 6개: `EnsureProtectedTokenFile`, `EnsureAccountAuthBootstrapFiles`,
  `WriteProtectedTokenFile`, `ReadProtectedTokenSha256`, `CreateToken`,
  `EnsureResultTokenPath`.

## 검증

Step 3에서 실행한 명령과 실제 관찰 결과는 다음과 같다. 모두 이 worktree
(`D:\data\projects\codex-zone\purecvisor-desktop-node\.worktrees\host-service-action-decomposition`)에서
실행했다.

| 실제 실행 명령 | 관찰 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter "NoOpsForwarderRemainsOnHostServiceAction\|HostServiceActionKeepsOnlyItsPublicSurface"` | 통과 `2`, 실패 `0` |
| `dotnet test src/DesktopNode.sln` | 통과 `836`, 실패 `0`, 건너뜀 `0` (Contracts `21`, Service `11`, Runtime `126`, HyperV `131`, Cli `113`, Host `198`, Api `236`) |
| `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"` | Passed `477`, Failed `0`, Skipped `2` |
| `pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'"` | Passed `49`, Failed `0`, Skipped `0` |
| `pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests'"` | Passed `49`, Failed `0`, Skipped `0` |
| `npm test --prefix web` | TypeScript 오류 `0`; served `app.js` current; completion batch `5`, work item `25` |
| `npm run verify:parity --prefix web` | served asset, static manifest, static parity, browser fixture 모두 PASS |
| `npm run browser:fixture --prefix web` | `browser fixture verification passed` |
| `node --check web/app.js` | 구문 오류 없음, exit `0` |
| `git diff --check` | exit `0`, 출력 없음 |

솔루션 `836`건은 이번 task에서 추가한 `NoOpsForwarderRemainsOnHostServiceAction`,
`HostServiceActionKeepsOnlyItsPublicSurface` 2건을 포함한다(Task 9 종료 시점 `834`건,
`DesktopNode.Host.Tests` `196`건에서 각각 `+2`).

`installer/tests` `49/49`, `web/tests` `49/49`는 이 작업으로 변경되지 않았다(요구 조건 그대로
유지).

`web/node_modules`는 이 worktree에 사전 설치되어 있지 않아 `npm test`/`npm run verify:parity`가
처음에는 `typescript` 미발견으로 실패했다. `npm ci --prefix web`(패키지 `1`개 추가, `2`개 감사,
취약점 `0`)으로 worktree-local 의존성을 설치한 뒤 재실행하여 위 결과를 얻었다. 이는 이
worktree의 사전 설치 상태 문제이며 코드 변경과 무관하다.

패키징 Pester의 건너뜀 `2`건은 `PcvJobStore04265ReaderCompatibility`다. 이 branch 이전부터
존재하던 worktree 환경 차이이며 이 task 이전에도 동일하게 건너뛴다. 통과로 취급하지 않는다.

## Nonclaims

- 동작 변경을 하지 않았다. 순수 이동이며 새 기능이나 오류 코드 변경을 주장하지 않는다.
- 이 evidence는 code-level 범위이며 설치본 관측이나 anchor 승격을 주장하지 않는다.
- `ServiceTokenRotationRevokeReplacesProtectedTokenFileRestartsServiceAndWritesRedactedAudit`은
  간헐적으로 실패한다. 이 branch 이전부터 존재했고 이동 전 커밋에서도 재현했으며, Task 9의
  독립 재실행 5회에서는 재현하지 않았다. 근본 원인은 미상이며 이 task에서 수정을 시도하지
  않았다.
- `DesktopNodeCredentialManagerOps`는 자기 클래스 내부에 private
  `ExecuteNativeCredentialManagerActionForOps` wrapper를 유지한다. private이고 같은 클래스
  내부이므로 boomerang(`ExecuteAsync -> Ops.X.Execute -> ServiceAction` 왕복)은 아니지만, 형제
  Ops 클래스 8개 중 7개는 dispatch를 `Execute`에 바로 접는다. 이 불일치는 미관상 문제로 남겨
  두었고 수정하지 않았다.
