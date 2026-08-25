# Pester-free C# verification Wave A foundation 기록 (2026-08-24)

evidence_id: `pester-free-csharp-verification-wave-a-foundation-2026-08-24`
판정: `WAVE_A_CODE_LEVEL_FOUNDATION_ONLY`
Design-ID: `purecvisor-desktop-node-pester-free-csharp-verification-20260824-v1`
설계 경로: `docs/superpowers/specs/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-design.md`
Wave A 계획 경로: `docs/superpowers/plans/2026-08-24-purecvisor-desktop-node-pester-free-csharp-verification-wave-a.md`
비교 기준: `bee07214cd4f2f061b30996f766b9976a9527abd`
evidence-input HEAD: `84f16b3e9f06b0b526814f71c7b78c919de0b133`

`evidence-input HEAD`는 Task 9 문서·테스트 커밋 전 입력 commit이다. 최종 Task 9 commit은 이
문서 안에서 자기참조하지 않는다.

## 범위와 판정

이 기록은 C# verification runner의 Wave A code-level foundation만 다룬다. Web, Installer,
Packaging/evidence migration인 Wave B~D와 required CI cutover인 Wave E를 실행하거나 완료로
판정하지 않았다. migration 전체 PASS, Pester-free, non-admin PowerShell-free, cutover, 제품
승격 또는 public release를 주장하지 않는다. `docs/ga-ready/current-evidence.json`은 변경하지
않았으며 operational current는 `0.42.74-admin-smoke` 그대로다.

## RED와 사전 verification assembly

evidence 파일을 만들기 전 다음 focused test를 실행했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationArchitectureBoundaryTests --nologo
```

전체 `4`개 중 production project/source/catalog 경계 `3`개는 통과했고
`EvidenceRefusesCutoverMutationAndPromotionClaims`만 evidence 파일 부재
`FileNotFoundException`으로 실패했다. 관측값은 passed `3`, failed `1`, skipped `0`이다. 빈
evidence 파일로 우회하지 않았다.

그 뒤 evidence assertion 하나만 제외한 Release assembly를 정확히 한 번 실행했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName!~EvidenceRefusesCutoverMutationAndPromotionClaims"
```

관측값은 passed `460`, failed `0`, skipped `0`, exit `0`이다. 정확한 단일 exclusion은
`FullyQualifiedName!~EvidenceRefusesCutoverMutationAndPromotionClaims`이며 이 결과를 최종 전체
assembly 결과로 해석하지 않는다.

## Full/M plan-only 관측

```text
dotnet run --project src/DesktopNode.Verification -c Release -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-verification-csharp-wave-a-plan --plan-only
```

- summary locator: `artifacts/development-verification-csharp-wave-a-plan/summary.json`
- exit: `0`
- contract: `pcv-development-verification-summary-v2`
- requested/effective lane: `Full` / `Full`
- change tier: `M`
- execution scope: `lane`
- activation state: `plan-only-foundation`
- ok: `true`; 이는 plan/catalog 검증 성공만 뜻한다.
- ordered suite IDs: `dotnet`, `web-typecheck`, `web-parity`, `delivery-contracts`,
  `installer-contracts`, `evidence-check`, `policy-boundaries`
- 모든 `7`개 result status: `planned`
- ordered shard IDs: `dotnet`, `web`, `delivery`, `installer-policy`
- raw summary bytes SHA-256:
  `06be56e4cbbe6bcbbcf28b1472871c7edc5854bea480f69d34928f6aaa6b6959`

Node로 contract, ok, requested/effective lane, change tier, activation state, 정확한 일곱 suite
순서와 모든 `planned` status를 검사한 뒤 원본 summary bytes의 lowercase 64자리 SHA-256을
계산했다.

### summary 재현성 경계

보존된 위 summary instance의 변동 필드와 원본 크기는 다음과 같다.

- `started_at=2026-08-24T00:01:22.3299541+00:00`
- `completed_at=2026-08-24T00:01:22.4325657+00:00`
- `duration_ms=102`
- raw byte length: `1961`

원본 SHA-256은 이 관측 instance를 식별하며 timestamp와 duration이 달라지는 재실행 사이의
안정 해시를 뜻하지 않는다. 재현 가능한 투영은 JSON을 파싱한 뒤 객체와 배열을 재귀 순회하고,
이름이 `started_at` 또는 `completed_at`인 모든 property의 값만 문자열 `<volatile>`로,
`duration_ms`인 모든 property의 값만 숫자 `0`으로 교체한다. property를 삭제하거나 다시
삽입하지 않으므로 기존 순서를 유지한 채 `JSON.stringify`하고 UTF-8 bytes에 SHA-256을 적용한다.
실행한 정확한 Node 명령은 다음과 같다.

```text
node -e "const fs=require('node:fs'),crypto=require('node:crypto');const p='artifacts/development-verification-csharp-wave-a-plan/summary.json';const b=fs.readFileSync(p);const s=JSON.parse(b);const observed={started_at:s.started_at,completed_at:s.completed_at,duration_ms:s.duration_ms};const raw=crypto.createHash('sha256').update(b).digest('hex');const normalize=x=>{if(Array.isArray(x)){x.forEach(normalize);return;}if(!x||typeof x!=='object')return;for(const k of Object.keys(x)){if(k==='started_at'||k==='completed_at')x[k]='<volatile>';else if(k==='duration_ms')x[k]=0;else normalize(x[k]);}};normalize(s);const c=Buffer.from(JSON.stringify(s),'utf8');const canonical=crypto.createHash('sha256').update(c).digest('hex');if(raw!=='06be56e4cbbe6bcbbcf28b1472871c7edc5854bea480f69d34928f6aaa6b6959'||canonical!=='e03621e99c07a66f67e80afd878bda12e691577e5fef1eea14584b302e440f6e')process.exit(1);console.log(JSON.stringify({...observed,raw_byte_length:b.length,raw_sha256:raw,canonical_byte_length:c.length,canonical_sha256:canonical}));"
```

관측된 canonical projection은 `1360` bytes이고 SHA-256은
`e03621e99c07a66f67e80afd878bda12e691577e5fef1eea14584b302e440f6e`다. 이 해시는 위 세
이름의 변동 필드만 정규화하고 나머지 summary 의미가 같을 때의 재현 가능한 비교값이다.

## actual-mode fail-closed 관측

```text
dotnet run --project src/DesktopNode.Verification -c Release -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/development-verification-csharp-wave-a-actual-blocked
```

관측값은 exit `2`, `ok=false`, `error_code=PCV_VERIFY_CONFIG_INVALID`,
`catalog_activation_state=plan-only-foundation`, results `0`개다. child process는 실행되지
않았으며 같은 Release verification assembly의 `VerificationApplicationTests`가 recording
process/managed runner 호출 수 `0`을 검증한다.

no-child ownership을 다음 exact FQN으로 별도 실행했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName=DesktopNode.Verification.Tests.VerificationApplicationTests.ActualExecutionIsLockedBeforeExecutorAndWritesFailureSummary"
```

관측값은 passed `1`, failed `0`, skipped `0`, exit `0`이다. 이 테스트는 recording process
runner의 `CallCount=0`과 managed runner의 `CallCount=0`을 모두 assertion한다. 앞 절의 실제 CLI
exit `2`와 results `0`은 별도 관측이다. 두 증거를 결합해 application의 executor 이전 activation
lock ownership을 확인하며, 그 밖의 직접적인 OS child-process telemetry를 수집했다고 주장하지
않는다.

## legacy required 경로 무변경 관측

```text
git diff --name-only bee07214cd4f2f061b30996f766b9976a9527abd -- .github/workflows/development-gates.yml
```

stdout은 비어 있었고 경로 수는 `0`이었다.

```text
git diff --name-only bee07214cd4f2f061b30996f766b9976a9527abd -- packaging/windows-desktop-node/tools/PcvDevelopmentVerification.psm1 packaging/windows-desktop-node/tools/PcvDevelopmentVerificationRunner.psm1 packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1
```

stdout은 비어 있었고 경로 수는 `0`이었다. 따라서 Wave A~D 동안 현재 required workflow와
`Invoke-PcvDevelopmentVerification.ps1`가 authoritative 경로로 남는다.

## 품질 보강 전 최초 GREEN

이 절은 synthetic architecture guard와 강화 helper를 추가하기 전 최초 Task 9 source state의
관측값이다. evidence 문서 초안 뒤 focused architecture test를 실행한 결과는 passed `4`,
failed `0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj --filter FullyQualifiedName~VerificationArchitectureBoundaryTests --nologo
```

같은 source state의 전체 Verification Release 결과는 passed `461`, failed `0`, skipped `0`,
exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
```

같은 source state의 전체 Release solution 결과는 test assembly `8`개, passed 합계 `1428`,
failed `0`, skipped `0`, exit `0`이었다. assembly별 passed 수는 Verification `461`, Contracts
`21`, Service `11`, CLI `144`, Runtime `128`, HyperV `156`, Host `206`, API `301`이다.

```text
dotnet test src/DesktopNode.sln -c Release --nologo
```

이 최초 GREEN은 아래 1차·2차 품질 보강 후 GREEN과 서로 다른 source state의 역사적 관측이며, 수치를
같은 실행 결과로 합치지 않는다.

## 2026-08-24 architecture guard 품질 보강 TDD

기존 단순 검사에 synthetic bypass fixture를 먼저 추가하고 다음 명령으로 RED를 관측했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~VerificationArchitectureBoundarySyntheticTests"
```

관측값은 passed `0`, failed `15`, skipped `0`, exit `1`이다. RED는 namespaced
`ProjectReference` `1`건, namespaced `Reference`/`Import`/`PackageReference` dependency token
`3`건, 외부·rooted·escaping `Compile` `3`건, `bin`/`obj`와
`EnableDefaultCompileItems=false` `2`건, file/directory reparse point `2`건, decoded JSON escape
`1`건, evidence key prefix/duplicate/true-shaped claim `3`건이다.

테스트는 그대로 두고 순수 helper를 강화한 뒤 같은 명령의 관측값은 passed `15`, failed `0`,
skipped `0`, exit `0`이다. 이어 canonical production 경계만 실행한 관측값은 passed `4`,
failed `0`, skipped `0`, exit `0`이다.

XML element와 attribute는 namespace와 무관하게 `Name.LocalName`으로 검사한다. SDK default
compile scan은 `bin`/`obj`를 제외하고 reparse point를 거부하며, explicit `Compile`의
`Include`/`Link`는 project root containment를 요구한다. 별도 Roslyn/MSBuild package는 추가하지
않았다. `ProductionCompileSourcesConservativelyContainNoProductWmiOrInstallerTokens`의 raw token
scan은 comment도 검사하는 의도적인 conservative guard라서 false-positive 방향은 안전하다.
catalog는 strict canonical fixture load 뒤 parsed `JsonElement`의 모든 decoded property name과
string value를 재귀 검사한다. evidence helper는 여덟 canonical false line을 각각 정확히 한 번
요구하고 true-shaped claim을 금지한다.

## 1차 품질 보강 후 GREEN

1차 품질 보강 문서 편집 뒤 같은 worktree에서 architecture production/synthetic test를 함께
실행했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~VerificationArchitectureBoundary"
```

관측값은 passed `19`, failed `0`, skipped `0`, exit `0`이다. 이어 전체 Release verification
assembly를 실행했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
```

관측값은 passed `476`, failed `0`, skipped `0`, exit `0`이다. 같은 최종 문서 상태에서 전체
Release solution도 실행했다.

```text
dotnet test src/DesktopNode.sln -c Release --nologo
```

관측값은 test assembly `8`개, passed 합계 `1443`, failed `0`, skipped `0`, exit `0`이다. 이
수치는 Wave A code-level 검증 결과이며 Wave B~E migration 또는 cutover 완료 판정이 아니다.

## 2026-08-24 evaluated-graph fail-closed 2차 품질 보강 TDD

1차 GREEN 뒤 평가되지 않은 MSBuild graph의 우회 사례 세 개를 synthetic test로 먼저 추가하고
다음 exact command로 RED를 관측했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~FailsClosedWithoutEvaluatedGraph"
```

관측값은 passed `0`, failed `3`, skipped `0`, exit `1`이다. 실패한 세 사례는 mixed-case
`ProjectReference`, property-indirected `PackageReference`, 외부 `Compile`을 추가하는 explicit
imported props이며, 각각 기존 helper의 false-negative를 재현했다. 빈 fixture나 기대값 완화로
우회하지 않았다.

별도 MSBuild/Roslyn package나 production csproj를 추가·변경하지 않고 fail-closed helper를
강화했다. XML element/attribute 이름은 `Name.LocalName`과 `OrdinalIgnoreCase`로 비교한다.
production project의 `ProjectReference`, `PackageReference`, `Reference`, `Import` 형태는 값과
무관하게 거부한다. 실제 evaluated graph를 사용하지 않는 경계이므로 explicit `Import`,
`Directory.Build.props`/`Directory.Build.targets` implicit input, MSBuild property/item
indirection, conditioned item, 단순 `Include`/`Link` 이외의 `Compile` 표기도
`unsupported-unevaluated-build-graph` 계열 오류로 보수적으로 거부한다. project/repository root
밖 경로와 reparse point 거부는 유지한다.

같은 세 사례를 다시 실행한 관측값은 passed `3`, failed `0`, skipped `0`, exit `0`이다. 기존
synthetic 회귀를 포함한 다음 focused command의 관측값은 passed `18`, failed `0`, skipped `0`,
exit `0`이다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~DesktopNode.Verification.Tests.VerificationArchitectureBoundarySyntheticTests"
```

## 2차 품질 보강 후 GREEN

production과 synthetic architecture 경계를 함께 실행했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~VerificationArchitectureBoundary"
```

관측값은 passed `22`, failed `0`, skipped `0`, exit `0`이다. activation lock의 no-child ownership도
다음 exact FQN으로 재실행했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName=DesktopNode.Verification.Tests.VerificationApplicationTests.ActualExecutionIsLockedBeforeExecutorAndWritesFailureSummary"
```

관측값은 passed `1`, failed `0`, skipped `0`, exit `0`이다. 이 test의 recording process runner와
managed runner `CallCount=0` assertion 의미 및 실제 CLI 관측과의 구분은 위 actual-mode 절과
같으며, 추가 OS child telemetry를 주장하지 않는다.

전체 Verification Release 결과는 passed `479`, failed `0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
```

전체 Release solution 결과는 test assembly `8`개, passed 합계 `1446`, failed `0`, skipped `0`,
exit `0`이었다. assembly별 passed 수는 Verification `479`, Contracts `21`, Service `11`, CLI
`144`, Runtime `128`, HyperV `156`, Host `206`, API `301`이다.

```text
dotnet test src/DesktopNode.sln -c Release --nologo
```

이 2차 GREEN은 1차 품질 보강의 `19`/`476`/`1443` 관측을 대체해 삭제하지 않고 별도 source
state 이력으로 이어진다. 이 수치도 Wave A code-level 검증일 뿐 Wave B~E migration 또는
cutover 완료 판정이 아니다.

## 2026-08-24 static project allowlist 3차 품질 보강 TDD

2차 GREEN 뒤 custom root SDK, dynamic `Compile` 생성, executable build graph construct, supplied
repository root 위쪽 ancestor implicit input 우회 fixture를 먼저 추가했다. 다음 exact command로
RED를 관측했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~FailsClosedByStaticProjectAllowlist"
```

관측값은 passed `0`, failed `4`, skipped `0`, exit `1`이다. 네 실패는
`Contoso.Build.Sdk` custom root SDK, `Target`/`CreateItem`/`Output`을 통한 외부 `Compile` 생성,
root `UsingTask`, supplied repository root보다 위에 있는 `Directory.Build.props`의 외부
`Compile` 주입이다. 모두 기존 scanner가 예외 없이 반환한 false-negative였으며 기대값 완화나
빈 fixture로 우회하지 않았다.

별도 MSBuild/Roslyn package, production csproj 또는 production code를 변경하지 않고 정적
fail-closed allowlist를 적용했다. root는 namespace 없는 `Project`와 단 하나의 literal
`Sdk=Microsoft.NET.Sdk` attribute만 허용한다. custom/multiple/property-indirected SDK는 거부한다.
root direct child는 attribute 없는 `PropertyGroup`/`ItemGroup`만 허용한다. property는 현재 Wave A
project에 필요한 `OutputType`, `TargetFramework`, `AssemblyName`, `ImplicitUsings`, `Nullable`,
`EnableDefaultCompileItems`의 중복 없는 literal leaf만 허용한다. item은 literal `Compile`과
`InternalsVisibleTo`만 허용하며, `Compile`은 정확히 하나의 literal `Include`와 최대 하나의
literal `Link`만 허용한다. unknown element/attribute, namespace, `Condition`, `Choose`, `Target`,
`UsingTask`, task/`Output`, custom item, `Import`, `Remove`/`Update`/`Exclude`, glob과 MSBuild
property/item indirection은 `unsupported-unevaluated-build-graph` 오류로 거부한다. 이는 evaluated
MSBuild graph가 아니라 canonical static Wave A project shape의 allowlist다.

implicit input 검사는 project directory에서 supplied repository root에서 멈추지 않고 filesystem
root까지 실제 ancestor chain의 `Directory.Build.props`와 `Directory.Build.targets`를 모두
검사한다. existing ancestor directory의 reparse point도 거부한다. synthetic `.props`/`.targets`와
외부 source는 각 fixture가 소유한 임시 root 안에만 만들고 cleanup도 그 root만 삭제한다.

구현 뒤 같은 focused command는 passed `4`, failed `0`, skipped `0`, exit `0`이었다. 최종 fixture는
custom/multiple/property-indirected SDK 세 형태와 ancestor `.props`/`.targets` 두 형태를 같은 test
contract 안에서 모두 확인한다. 전체 synthetic architecture guard 관측값은 passed `22`, failed
`0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~DesktopNode.Verification.Tests.VerificationArchitectureBoundarySyntheticTests"
```

## 3차 품질 보강 후 GREEN

production과 synthetic architecture 경계를 함께 실행한 관측값은 passed `26`, failed `0`, skipped
`0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~VerificationArchitectureBoundary"
```

activation lock no-child exact FQN은 passed `1`, failed `0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName=DesktopNode.Verification.Tests.VerificationApplicationTests.ActualExecutionIsLockedBeforeExecutorAndWritesFailureSummary"
```

전체 Verification Release 결과는 passed `483`, failed `0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
```

전체 Release solution 결과는 test assembly `8`개, passed 합계 `1450`, failed `0`, skipped `0`,
exit `0`이었다. assembly별 passed 수는 Verification `483`, Contracts `21`, Service `11`, CLI
`144`, Runtime `128`, HyperV `156`, Host `206`, API `301`이다.

```text
dotnet test src/DesktopNode.sln -c Release --nologo
```

보존된 plan summary는 raw `1961` bytes와 SHA-256
`06be56e4cbbe6bcbbcf28b1472871c7edc5854bea480f69d34928f6aaa6b6959`, canonical projection
`1360` bytes와 SHA-256
`e03621e99c07a66f67e80afd878bda12e691577e5fef1eea14584b302e440f6e`가 그대로임을 재확인했다.
이 3차 GREEN은 이전 `3`/`1`, `460`, `461`/`1428`, `476`/`1443`, `479`/`1446` 관측을 삭제하거나
같은 source state 결과로 재해석하지 않는다. 이 수치도 Wave A code-level 검증일 뿐 Wave B~E,
cutover, Pester-free required CI 또는 public release 완료 판정이 아니다.

## 2026-08-24 Directory.Packages.props 4차 품질 보강 TDD

로컬 설치 SDK `10.0.400`의
`C:\Program Files\dotnet\sdk\10.0.400\NuGet.props`를 확인했다. 이 파일은 line `22`에서
`ImportDirectoryPackagesProps`의 미지정 기본값을 `true`로 두고, line `30`~`31`에서
`Directory.Packages.props`와 `GetDirectoryNameOfFileAbove`를 사용해 project directory 위쪽 파일을
찾으며, line `35`에서 발견한 경로를 자동 `Import`한다. 따라서 static source boundary의
filesystem-root ancestor 검사에도 이 파일명이 포함되어야 한다.

ancestor `Directory.Packages.props`가 외부 `Compile`을 주입하는 최소 fixture를 먼저 추가하고
다음 exact command로 RED를 관측했다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~DirectoryPackagesPropsFailsClosedByAutomaticBuildInputGuard"
```

관측값은 passed `0`, failed `1`, skipped `0`, exit `1`이다. 기존 automatic input 검사는
`Directory.Build.props`와 `Directory.Build.targets`만 거부했으므로 fixture가 예외 없이 반환된
false-negative였다.

case-insensitive automatic-build-input filename set에 `Directory.Packages.props`를 추가했다. 별도
package, production csproj, production code는 변경하지 않았다. 같은 exact command의 GREEN
관측값은 passed `1`, failed `0`, skipped `0`, exit `0`이다.

## 4차 품질 보강 후 최종 GREEN

전체 synthetic architecture guard는 passed `23`, failed `0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~DesktopNode.Verification.Tests.VerificationArchitectureBoundarySyntheticTests"
```

production과 synthetic architecture 경계를 함께 실행한 관측값은 passed `27`, failed `0`, skipped
`0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~VerificationArchitectureBoundary"
```

activation lock no-child exact FQN은 passed `1`, failed `0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo --filter "FullyQualifiedName=DesktopNode.Verification.Tests.VerificationApplicationTests.ActualExecutionIsLockedBeforeExecutorAndWritesFailureSummary"
```

전체 Verification Release 결과는 passed `484`, failed `0`, skipped `0`, exit `0`이었다.

```text
dotnet test src/DesktopNode.Verification.Tests/DesktopNode.Verification.Tests.csproj -c Release --nologo
```

전체 Release solution 결과는 test assembly `8`개, passed 합계 `1451`, failed `0`, skipped `0`,
exit `0`이었다. assembly별 passed 수는 Verification `484`, Contracts `21`, Service `11`, CLI
`144`, Runtime `128`, HyperV `156`, Host `206`, API `301`이다.

```text
dotnet test src/DesktopNode.sln -c Release --nologo
```

보존된 plan summary raw SHA-256
`06be56e4cbbe6bcbbcf28b1472871c7edc5854bea480f69d34928f6aaa6b6959`와 canonical projection
SHA-256 `e03621e99c07a66f67e80afd878bda12e691577e5fef1eea14584b302e440f6e`는 그대로다. 이 4차
GREEN은 이전 `3`/`1`, `460`, `461`/`1428`, `476`/`1443`, `479`/`1446`, `483`/`1450` 관측을
보존하며 같은 source state로 합치지 않는다. 이 수치도 Wave A code-level 검증일 뿐 migration,
cutover, Pester-free required CI 또는 public release 완료 판정이 아니다.

## mutation과 비주장 경계

- `host_mutation_performed=false`
- `msi_or_service_mutation=false`
- `actual_vm_tested=false`
- `required_ci_pester_zero=false`
- `required_ci_nonadmin_powershell_zero=false`
- `cutover_completed=false`
- `public_trusted_signing=false`
- `external_stable_publication=false`
- operational current: `0.42.74-admin-smoke` unchanged
- current evidence: unchanged

제품 API/host/admin 동작과 ADR-0009 Guest PowerShell Direct transport는 변경하지 않았다. 이
code-level Wave A foundation 기록은 Wave B~E 완료, migration PASS, cutover 또는 public claim의
근거가 아니다.
