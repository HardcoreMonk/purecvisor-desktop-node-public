# PureCVisor Desktop Node Development Gate Recovery Design

date: `2026-07-13`
status: `approved-design`
scope: `non-admin-dotnet-test-isolation-and-development-ci`
source_audit: `docs/project-status-audit-2026-07-13.md`
product_payload_change: `true-code-level-host-cli-testability-and-cli-error-contract`
host_mutation_performed: `false`
package_build_performed: `false`
next_installed_candidate: `0.42.60-admin-smoke-separate-approval`
public_trusted_signing: `not-claimed`
external_stable_publication: `not-claimed`

## 목표

일반 비상승 Windows 개발 환경과 설치본이 존재하는 개발 workstation에서 동일하게
`dotnet test src/DesktopNode.sln`을 통과시키고, .NET/Web/PowerShell 전체 비파괴 개발
검증을 GitHub Actions의 독립적인 development gate로 승격한다.

이번 slice는 제품 보안 경계를 약화하지 않는다. 설치본의 DPAPI LocalMachine token,
Administrators/SYSTEM 전용 ACL, loopback/LAN 정책, service/Hyper-V/OS mutation gate는
그대로 유지한다. 테스트는 제품 기본 경로와 실제 Windows ACL에 암묵적으로 의존하지
않도록 명시적 seam을 사용한다.

## 현재 문제

2026-07-13 비상승 계정에서 전체 .NET solution은 restore/build를 완료했지만 test run은
CLI 13건과 Host 6건의 `UnauthorizedAccessException`으로 실패했다.

### CLI 환경 결합

`DesktopNodeCliApplication.RunAsync`와 interactive shell 테스트는 token source를 명시하지
않는다. `environment: _ => null`은 환경 변수만 격리하고, resolver의 기본 경로
`%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` 탐색은 막지 않는다.

설치본 token이 없는 runner에서는 통과하지만, token이 존재하고 현재 비상승 사용자가 읽을
수 없는 workstation에서는 application test가 실제 설치본 파일을 읽다가 실패한다. 테스트
결과가 repository input이 아니라 host 설치 상태에 의해 달라진다.

### Host ACL 결합

Host service-action 테스트는 temp data root에 protected token, account bootstrap, JWT signing
key를 만든 뒤 내용을 다시 읽는다. 제품 구현은 파일 생성 직후 inheritance를 끊고
Builtin Administrators와 LocalSystem에만 read를 허용한다. 비상승 test process는 자신이 만든
fixture를 더 이상 읽을 수 없다.

ACL 정책은 제품 요구사항과 일치한다. 문제는 단위 테스트가 실제 OS ACL mutation과 파일
내용 assertion을 같은 process에서 결합한 점이다.

## 선택한 접근

승인된 접근은 **명시적 테스트 seam + 전체 development CI**다.

| 접근 | 장점 | 거부 이유/선택 이유 |
| --- | --- | --- |
| 테스트 argument에 임의 token만 반복 추가 | 변경량 최소 | default-path 환경 결합과 access-denied 오류 계약을 검증하지 못하므로 거부 |
| 명시적 token-path/ACL seam과 CI | 원인을 제거하고 제품 기본값 유지 | 선택 |
| 문서·artifact·0.42.60까지 동시 처리 | 전체 상태를 한 번에 정리 | 1차 gate 복구의 완료를 늦추므로 후속으로 분리 |

## 설계 원칙

1. 제품 실행은 기존 secure default를 사용한다.
2. 테스트는 host 설치 상태를 읽지 않는다.
3. ACL을 skip하는 경로는 test assembly의 internal seam으로만 노출한다.
4. access denied는 raw exception이나 local path를 노출하지 않는 stable problem code로 바꾼다.
5. unit test와 elevated installed smoke의 검증 소유권을 섞지 않는다.
6. CI는 non-mutating command만 실행한다.
7. 한 suite의 실패가 다른 suite 결과 수집을 막지 않도록 CI job을 분리한다.

## CLI 설계

### Default protected-token path seam

`DesktopNodeCliTokenResolver.Resolve`에는 이미 `defaultProtectedTokenFilePath` override가 있다.
이 값을 application과 interactive shell 경계까지 전달한다.

변경 대상:

- `DesktopNodeCliApplication.RunAsync`
- `DesktopNodeCliInteractiveShell.RunAsync`
- 두 경계를 호출하는 unit tests

Production `Program.Main`은 override를 전달하지 않는다. 따라서 installed CLI는 계속
ProgramData의 기본 protected token을 자동 탐색한다.

Application/interactive tests는 test마다 고유한 nonexistent path를 전달한다. 이 경로는
temp root 아래에 만들되 파일은 생성하지 않는다. 테스트는 실제 ProgramData token 존재와
무관하게 no-default-token 상태를 재현한다.

Resolver 자체의 default-path behavior는 `DesktopNodeCliTokenResolverTests`가 소유한다.
Application formatter/transport tests는 resolver의 Windows 설치 상태를 다시 검증하지 않는다.

### Access-denied error contract

Protected token file read 단계에서 다음 failure를 stable CLI error로 정규화한다.

| Failure | Problem code | Exit |
| --- | --- | ---: |
| file access denied/security failure | `PCV_CLI_PROTECTED_TOKEN_ACCESS_DENIED` | 2 |
| malformed JSON/base64/DPAPI payload | `PCV_CLI_PROTECTED_TOKEN_INVALID` | 2 |
| DPAPI unprotect failure | `PCV_CLI_PROTECTED_TOKEN_DECRYPT_FAILED` | 2 |

메시지는 token value, protected blob, absolute path, Windows account SID를 포함하지 않는다.
`DesktopNodeCliApplication`의 기존 `ArgumentException` redaction 경계가 이 오류를 표준 CLI
result로 변환한다. raw `UnauthorizedAccessException`, `JsonException`,
`CryptographicException`이 application boundary 밖으로 나오지 않아야 한다.

## Host ACL 설계

### Internal ACL hardener boundary

Host project에 internal file ACL hardener contract를 둔다.

```csharp
internal interface IDesktopNodeHostFileAclHardener
{
    void Harden(string path);
}
```

Production 구현은 현재 `HardenTokenFileAcl`의 Windows ACL 동작을 그대로 소유한다.

- inheritance protection: enabled, inherited rule preservation: false
- Builtin Administrators: read allow
- LocalSystem: read allow
- current interactive user 자동 허용: 없음

Public product entry는 production hardener를 항상 사용한다. Host test assembly는 이미
`InternalsVisibleTo`로 연결되어 있으므로 별도 public test API를 만들지 않는다.

### Service-action injection

기존 public `ExecuteAsync` overload는 production hardener를 사용하는 현재 동작을 유지한다.
internal test overload만 `IDesktopNodeHostFileAclHardener`를 받는다.

다음 파일 생성 경로가 동일한 hardener instance를 사용해야 한다.

- `api-token.dpapi.json`
- `accounts.json`
- `jwt-signing-key.txt`
- service-token rotation으로 생성되는 replacement token file

Unit tests는 recording no-op hardener를 주입한다. 파일 내용과 DPAPI record를 읽을 수 있고,
동시에 hardener가 정확한 파일 path마다 호출됐는지 검증한다.

### 검증 소유권

| 검증 | 소유자 |
| --- | --- |
| 파일 내용, DPAPI record, service configuration arguments | non-admin xUnit |
| hardener 호출 path와 호출 횟수 | non-admin xUnit recording hardener |
| Windows ACL rule shape의 정적 계약 | focused Host test 또는 packaging contract test |
| 실제 Administrators/SYSTEM ACL과 service identity read | 별도 elevated installed smoke |

Non-admin unit test를 통과시키기 위해 production ACL을 완화하거나 current user rule을 추가하는
변경은 금지한다.

## CI 설계

새 workflow는 `.github/workflows/development-gates.yml`이 소유한다. 기존
`.github/workflows/public-boundary.yml`은 그대로 유지한다.

### Trigger

- `pull_request`
- `push` to `main`
- `push` to `codex/**`
- `workflow_dispatch`

### 병렬 job

| Job | Runner | 명령 |
| --- | --- | --- |
| `dotnet-tests` | `windows-latest` | restore + Release `dotnet test src/DesktopNode.sln` |
| `web-tests` | `ubuntu-latest` | `npm ci`, `npm test`, `npm run verify:parity` |
| `packaging-pester` | `windows-latest` | packaging Pester suite |
| `installer-web-pester` | `windows-latest` | installer Pester, web Pester |

Job을 분리해 packaging suite가 다른 결과를 가리지 않게 하고 wall-clock latency를 줄인다.
한 job 실패가 이미 실행 중인 다른 job을 취소하지 않도록 matrix fail-fast에 의존하지 않는다.

### Toolchain

- checkout action은 repository의 현재 major 계약을 따른다.
- .NET SDK: `10.0.x`
- Node.js: `24.x`
- npm install: `npm ci --prefix web`
- Pester: exact `5.7.1`
- NuGet/npm cache 사용

Pester job은 `-PassThru` aggregate의 `FailedCount`가 0이 아닐 때 명시적으로 exit 1을 반환한다.
출력은 suite별 Total/Passed/Failed/Skipped와 duration을 남긴다.

### Mutation 금지

Development workflow는 다음을 호출하지 않는다.

- MSI install/repair/remove
- Windows Service create/start/stop/delete
- Hyper-V VM/checkpoint/guest mutation
- firewall/Event Log/trust-store/Credential Manager mutation
- Task Scheduler registration 또는 reboot
- admin-smoke package build/public publication

## Test Strategy

### Red tests

구현 전에 현재 failure를 좁은 test로 고정한다.

1. 설치본 default token과 무관하게 application test가 injected missing default path를 사용한다.
2. protected token access denied가 stable code와 exit 2를 반환하고 path/token을 노출하지 않는다.
3. Host bootstrap file flow가 recording no-op hardener를 사용한다.
4. production Host wrapper가 secure hardener 없이 실행되는 경로가 없음을 검증한다.

### Green verification

다음 순서로 검증한다.

1. CLI resolver/application/interactive focused tests
2. Host service-action focused tests
3. 전체 `dotnet test src/DesktopNode.sln`
4. Web npm/parity
5. packaging/installer/web Pester
6. `git diff --check`
7. GitHub development workflow의 네 job과 기존 public-boundary job

### Environment proof

전체 .NET test는 실제 ProgramData protected token이 존재하고 현재 계정이 읽을 수 없는
상태에서도 통과해야 한다. 통과 조건은 installed token을 삭제하거나 ACL을 임시 완화하는
것이 아니다.

## Failure Handling

- injected default path가 누락되면 application tests는 host default를 사용하지 않고 test helper
  작성 오류로 실패해야 한다.
- no-op hardener는 test assembly 밖에서 생성하거나 선택할 수 없어야 한다.
- production hardener 호출 누락은 recording hardener assertion과 static contract test가 잡는다.
- CI dependency setup 실패는 해당 job failure로 남기며 test PASS로 간주하지 않는다.
- CI에서 Windows-only suite가 Linux fallback으로 대체되면 gate 불충족으로 판정한다.
- access-denied 메시지에 absolute path 또는 token material이 보이면 security test failure다.

## Documentation and Evidence

구현이 green이면 다음 문서를 갱신한다.

- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`: development CI job과 non-admin ownership
- `docs/DEVELOPER_INDEX.md`: 새 workflow와 local verification entry
- `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md`: 실행 결과와 host mutation false
- `docs/project-status-audit-2026-07-13.md`: .NET gate 상태 addendum

이 code-level evidence는 current installed anchor를 `0.42.59`에서 바꾸지 않는다. Host/CLI binary
change는 다음 installed candidate를 `0.42.60-admin-smoke`로 유지하지만, package/fullgate/
manual-admin/current-card 실행은 별도 사용자 승인 작업이다.

## Scope Boundaries

### 포함

- CLI default protected-token path test seam
- CLI protected-token read failure normalization
- Host ACL hardener internal seam
- non-admin xUnit isolation
- full development GitHub Actions workflow
- verification policy와 code-level evidence

### 제외

- 제품 ACL 완화
- installed token 삭제 또는 ACL 임시 변경
- current ledger anchor 승격
- `0.42.60-admin-smoke` package/fullgate/manual-admin campaign
- current 문서 전체 중복 제거
- artifact remote/immutable store 도입
- branch/worktree 정리
- public signing/winget/external publication

## Acceptance Criteria

1. 비상승 Windows에서 `dotnet test src/DesktopNode.sln`이 실패 0으로 끝난다.
2. installed ProgramData protected token의 존재와 read denial이 test outcome에 영향을 주지 않는다.
3. production CLI는 default ProgramData token auto-discovery를 유지한다.
4. production Host는 Administrators/SYSTEM 전용 ACL을 유지한다.
5. access-denied/invalid/decrypt failure는 stable redacted CLI error가 된다.
6. Web npm/parity와 세 Pester 범위가 모두 통과한다.
7. GitHub development workflow 네 job과 public-boundary job이 모두 성공한다.
8. development workflow는 host mutation을 실행하지 않는다.
9. code-level evidence는 `host_mutation_performed=false`, `package_build_performed=false`를 기록한다.
10. installed operational anchor는 별도 package 승인 전까지 `0.42.59-admin-smoke`로 유지된다.

## Rollout

1. CLI/Host focused red tests로 환경 결합을 고정한다.
2. 최소 test seam과 error normalization을 구현한다.
3. local non-admin full verification을 실행한다.
4. development workflow를 추가하고 branch run을 확인한다.
5. code-level evidence와 verification docs를 갱신한다.
6. `0.42.60-admin-smoke` 승격 여부를 별도 작업으로 재판정한다.

## Rollback

이 slice는 host mutation과 persistent data migration을 수행하지 않는다. 문제가 생기면 code와
workflow commit을 revert한다. 설치본 `0.42.59-admin-smoke`, ProgramData token, SCM service,
Hyper-V state, firewall/Event Log/trust store에는 rollback operation이 필요하지 않다.
