# 개발 게이트 복구 code-level evidence 2026-07-13

```yaml
status: code-level-remote-pass
scope: non-admin-dotnet-test-isolation-and-development-ci
product_payload_change: true-code-level-host-cli-testability-and-cli-error-contract
host_mutation_performed: false
package_build_performed: false
installed_anchor: 0.42.59-admin-smoke
next_installed_candidate: 0.42.60-admin-smoke-separate-approval-required
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
remote_ci_status: pass
remote_ci_head_sha: 2f9902801124c1bf095a2b01d9c77790d37a011f
remote_ci_development_gates_run_id: 29231097324
remote_ci_public_boundary_run_id: 29231097334
```

## 범위

- CLI application/interactive shell이 테스트에서 고유한 누락 default protected-token 경로를
  주입받도록 했다. 제품 `Program.cs`는 경로를 주입하지 않으므로 기본값은 계속
  `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`이다.
- protected-token 읽기 실패는 경로, SID, JSON/base64 payload, DPAPI 진단 문자열을 노출하지
  않는 안정 code로 정규화했다.
- Host 파일 ACL 적용은 내부 `IDesktopNodeHostFileAclHardener`로 분리했다. 제품은 singleton
  구현을 사용하고 단위 테스트는 recording no-op을 사용한다.
- `.github/workflows/development-gates.yml`은 비변경 개발 gate만 실행한다. 기존
  `.github/workflows/public-boundary.yml`은 변경하지 않았다.

로컬 전체 gate를 실행한 implementation head는
`becc3890f132b2206f62a0e5725f557a981641b5`다.

최초 원격 implementation/docs head `2a73e6834172f9dcfa86e8c4ef566213bcd0d248`에서는
기존 API 동시성 테스트의 임의 `500ms` 성능 임계치가 GitHub runner 부하에서 실패했다.
생산 API 동작이나 이번 CLI/Host 변경이 아니라 test timing 계약 문제였으며, read route가
blocking mutation 종료 전에 반환하는 조건을 직접 검증하도록 test-only correction
`2f9902801124c1bf095a2b01d9c77790d37a011f`을 적용했다. 이 correction head에서 로컬
solution `737/737`과 원격 Development Gates/Public Boundary가 모두 PASS했다.

## TDD RED 관찰

- CLI 경로 seam: `defaultProtectedTokenFilePath`가 없는 상태에서 `CS1739`를 확인했다.
- CLI 오류 계약: `NormalizeProtectedTokenReadException`이 없는 상태에서 `CS0117`을 확인했다.
- Host ACL seam: `IDesktopNodeHostFileAclHardener`가 없는 상태에서 `CS0246`을 확인했다.
- workflow 계약: `.github/workflows/development-gates.yml`이 없는 상태에서 Pester
  `Passed: 0, Failed: 1`을 확인했다.

각 RED는 계획한 미구현 계약 때문에 실패했으며, 관련 없는 컴파일 또는 runtime 실패는
관찰되지 않았다.

## 로컬 검증

아래 명령은 모두 비관리자 Windows 세션에서 실행했고 exit code는 `0`이다.

| 실제 실행 명령 | 관찰 결과 |
| --- | --- |
| `dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj -c Release` | Passed `112`, Failed `0`, Skipped `0` |
| `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release --filter "FullyQualifiedName~DesktopNodeHostServiceActionTests"` | Passed `103`, Failed `0`, Skipped `0` |
| `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release` | Passed `150`, Failed `0`, Skipped `0` |
| `dotnet test src/DesktopNode.sln -c Release` | Passed `737`, Failed `0`, Skipped `0` |
| `pwsh -NoProfile -Command '$result = Invoke-Pester -Path ''packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1'' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }'` | Passed `1`, Failed `0` |
| `npm ci --prefix web` | package `1` added, package `2` audited, vulnerability `0` |
| `npm test --prefix web` | TypeScript 오류 `0`; served `app.js` current; completion batch `5`, work item `25` |
| `npm run verify:parity --prefix web` | served asset, static manifest, static parity, browser fixture PASS |
| `pwsh -NoProfile -Command '$result = Invoke-Pester -Path ''packaging/windows-desktop-node/tests'' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }'` | Passed `378`, Failed `0` |
| `pwsh -NoProfile -Command '$result = Invoke-Pester -Path ''packaging/windows-desktop-node/installer/tests'' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }'` | Passed `50`, Failed `0` |
| `pwsh -NoProfile -Command '$result = Invoke-Pester -Path ''web/tests'' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }'` | Passed `48`, Failed `0` |
| `pwsh -NoProfile -Command '$result = Invoke-Pester -Path ''packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1'' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }'` | Passed `84`, Failed `0` |
| `git diff --check` | exit `0`, 출력 없음 |

솔루션 `737`건은 Contracts `15`, Runtime `17`, Service `11`, CLI `112`, TUI `162`,
Host `150`, API `270`의 합계다.

## CLI protected-token 오류 계약

| Code | 표시 메시지 | application exit |
| --- | --- | ---: |
| `PCV_CLI_PROTECTED_TOKEN_ACCESS_DENIED` | `Protected token file access was denied.` | `2` |
| `PCV_CLI_PROTECTED_TOKEN_INVALID` | `Protected token file is invalid.` | `2` |
| `PCV_CLI_PROTECTED_TOKEN_DECRYPT_FAILED` | `Protected token file could not be decrypted.` | `2` |

기존 `PCV_CLI_PROTECTED_TOKEN_FILE_NOT_FOUND`, `PCV_CLI_PROTECTED_TOKEN_UNSUPPORTED`,
`PCV_CLI_PROTECTED_TOKEN_EMPTY`, 누락/빈 필드의 `PCV_CLI_PROTECTED_TOKEN_INVALID` 계약도
유지한다. 표시 문자열에는 source path, SID, raw JSON/base64, DPAPI 진단, inner exception
text가 포함되지 않는다.

## Host ACL hardener 호출 계약

- 새 `api-token.dpapi.json` 쓰기는 해당 정규화 경로를 정확히 한 번 harden한다.
- 새 account bootstrap은 `accounts.json`, `jwt-signing-key.txt`를 각각 한 번 harden한다.
- `configure-installed`는 위 세 파일을 모두 recording hardener로 관찰했다.
- `service-token-rotation-revoke`는 임시 파일에는 ACL을 적용하지 않고 atomic replace 뒤 최종
  token 경로를 정확히 한 번 harden한다.
- `credential-manager-default-transition`이 protected token을 새로 만들 때 같은 hardener를
  사용한다.
- 제품 singleton 정책은 ACL inheritance 차단, Builtin Administrators read, LocalSystem read,
  현재 사용자/Users/Everyone grant 없음으로 기존과 동일하다.

## Development Gates job matrix

| Job | Runner | Timeout | 비변경 명령 |
| --- | --- | ---: | --- |
| `dotnet-tests` | `windows-latest` | 30분 | .NET `10.0.x`, solution restore/test |
| `web-tests` | `ubuntu-latest` | 20분 | Node `24`, npm ci/test/parity |
| `packaging-pester` | `windows-latest` | 30분 | Pester `5.7.1`, packaging tests |
| `installer-web-pester` | `windows-latest` | 30분 | Pester `5.7.1`, installer/Web tests |

workflow는 pull request, `main` push, manual dispatch에서 실행한다. 기능 브랜치 push는 같은
head의 pull request run과 중복되므로 제외한다. required job 이름과 위 비변경 gate matrix는
유지하며 `permissions.contents=read`, concurrency cancellation을 사용한다. `msiexec`, VM
생성/시작, service mutation, admin-smoke/full-admin gate, signing, release, artifact upload,
deployment 명령은 포함하지 않는다.

## 원격 CI 관찰

최초 원격 run `29230775541`은 `dotnet-tests` job `86754314126`의 기존 API test timing
failure로 실패했고 Web/Packaging/Installer-Web job과 Public Boundary run `29230775468`은
PASS했다. 위 test-only correction 뒤 head
`2f9902801124c1bf095a2b01d9c77790d37a011f`에서 다음 결과를 관찰했다.

| Workflow / job | Run ID | Job ID | 결론 | URL |
| --- | ---: | ---: | --- | --- |
| Development Gates / `dotnet-tests` | `29231097324` | `86755308592` | PASS | [private-archive-repository]/actions/runs/29231097324/job/86755308592 |
| Development Gates / `web-tests` | `29231097324` | `86755308582` | PASS | [private-archive-repository]/actions/runs/29231097324/job/86755308582 |
| Development Gates / `packaging-pester` | `29231097324` | `86755308560` | PASS | [private-archive-repository]/actions/runs/29231097324/job/86755308560 |
| Development Gates / `installer-web-pester` | `29231097324` | `86755308561` | PASS | [private-archive-repository]/actions/runs/29231097324/job/86755308561 |
| Public Boundary / `public-boundary-ci-required` | `29231097334` | `86755308201` | PASS | [private-archive-repository]/actions/runs/29231097334/job/86755308201 |

- Development Gates run: [private-archive-repository]/actions/runs/29231097324
- Public Boundary run: [private-archive-repository]/actions/runs/29231097334

## 경계

이번 실행은 service/MSI/Burn/MSIX/Hyper-V/firewall/trust-store/Event Log mutation, package
build, signing, publication을 수행하지 않았다. 설치본 운영 anchor는 계속
`0.42.59-admin-smoke`다. 원격 CI PASS는 위 correction head의 실제 run/job 결과에만 한정하며
installed-host validation, public trusted signing 또는 external stable publication을 의미하지
않는다.
