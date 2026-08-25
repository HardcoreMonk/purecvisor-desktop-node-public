# PureCVisor Desktop Node C# 구조 개선 및 ASP.NET Core 도입 작업 계획

> **실행 지침:** 이 문서는 체크박스(`- [ ]`) 단위로 진행한다. 한 PR에서 동작 보존형 구조 이동, 동시성·영속성 의미 변경, HTTP transport 기본값 전환을 서로 섞지 않는다.

- 작성 기준: 2026-08-02
- 상태: 진행 중 / Wave 0·Wave 1A·Wave 1B·Wave 1C·Wave 1D ops dispatch `code_complete`; Wave 2A persistence·restart·single-writer·FI-02·FI-04 `code_complete`, Wave 2B operation reconciliation decision `code_complete`, Wave 2C `vm.rename`·`vm.delete`·`checkpoint.create` reconciliation `code_complete`; legacy 설치본 checkpoint `PASS` (`2026-08-03` post-reboot, operational promotion 아님); `0.42.68-admin-smoke` package와 관리자 installed Web/API/elevated PCVCLI smoke `PASS` (operational promotion 아님); ADR-0012 read-concurrency alternative `closed-not-adopted`; Wave 5A bounded admission/task tracking code slice `code_ready_operational_pending`; Wave 5A full lifetime/concurrency and Wave 6 ASP.NET Core transport pending
- 감사 기준 base commit: `2e98ff4f2df2` (Wave 2A physical writer predecessor `4d3a0d9782ee5e40fc35df51f44a36bf04a15034`; completion source `f3d5d7be4bb24b80fc2fa11be1cee93be13b4362`; final package provenance `3c16f78568cfb54a0cbe586449a540df3596bcf1`)
- 제품 기준선: `0.42.65-admin-smoke`
- 활성 운영자 표면: Web Console, PCVCLI
- 배포 경계: internal/private network only
- public trusted signing: `false`
- external stable publication: `false`

**기술 스택:** C# / .NET 10 Windows, ASP.NET Core 10, xUnit, `System.Text.Json`, `System.Management`, Windows Service, TypeScript Web Console, 기존 PowerShell packaging/Pester 검증.

**기준 문서:**

- [Stabilize Then Split 재설계](../specs/2026-05-11-purecvisor-desktop-node-stabilize-then-split-redesign-design.md)
- [Runtime Core Boundary 1차 계획](2026-05-11-purecvisor-desktop-node-runtime-core-boundary.md)
- [Hyper-V Domain Split 1차 계획](2026-05-11-purecvisor-desktop-node-hyperv-domain-split.md)
- [Host Ops Domain Split 1차 계획](2026-05-11-purecvisor-desktop-node-host-ops-domain-split.md)
- [개발 검증 정책](../../DEVELOPMENT_VERIFICATION_POLICY.md)
- [변경 등급 정책](../../DEVELOPMENT_CHANGE_CLASSIFICATION.md)
- [ADR-0004 GA-ready product runtime](../../adr/0004-ga-ready-product-runtime-candidate.md)
- [ADR-0011 CLI/Web-only surface](../../adr/0011-cli-web-only-operator-surface.md)
- [ADR-0013 Job store transaction lease/CAS](../../adr/0013-job-store-single-writer-transaction-lease.md)
- [ASP.NET Core Windows Service](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-10.0)
- [ASP.NET Core HTTP.sys](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/httpsys?view=aspnetcore-10.0)
- [Kestrel endpoint 구성](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0)
- [ASP.NET Core middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-10.0)
- [ASP.NET Core WebSockets](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-10.0)

## 1. 목적

현재 C# 제품 경계와 외부 계약을 유지하면서 다음 구조적 부채를 단계적으로 해소한다.

1. `DesktopNodeApiRequestProcessor`에 집중된 route, job state, queue, persistence, diagnostics 책임을 실제 소유 모듈로 이동한다.
2. `DesktopNodeHostServiceAction`으로 되돌아가는 얕은 Host Ops wrapper를 실제 operation-family owner로 심화한다.
3. Hyper-V operation catalog 중복과 거대 adapter 조립을 단순화하고 WMI provider를 직접 검증할 수 있는 seam을 만든다.
4. Job 저장 실패, process crash, timeout 이후 실행, service shutdown 경계를 명시적인 상태·수명주기 정책으로 고정한다.
5. 수동 `HttpListener` transport를 ASP.NET Core 기반 Windows Service transport로 단계적으로 교체하되 기존 route·JSON·인증·동시성 계약을 보존한다.
6. 테스트를 실제 코드 소유 프로젝트에 배치하고 coverage·analyzer를 점진적 품질 ratchet으로 사용한다.

이 계획은 새 제품 재작성 계획이 아니다. 2026-05-11 승인된 `Stabilize Then Split`의 첫 번째 façade/bridge 분리 이후 실제 동작 소유권을 깊게 만들고, 검증된 application core 위의 HTTP transport만 ASP.NET Core로 교체하는 후속 계획이다.

## 2. 현재 기준선

| 항목 | 현재 값 |
|---|---:|
| 제품 C# | 81파일 / 25,061 physical LOC |
| C# 테스트 | 45파일 / 19,388 physical LOC |
| .NET 검증 기준 | 684 tests, skip 0 (감사 전 591) |
| API route contract | 54 routes |
| 현재 HTTP transport | `System.Net.HttpListener` 직접 구현 |
| 현재 service host | Generic Host + `AddWindowsService` + `BackgroundService` |
| 현재 host 배포 | `win-x64` self-contained single-file publish |
| 목표 HTTP transport | ASP.NET Core, ADR-0014로 HTTP.sys/Kestrel 확정 |
| Web Console | TypeScript build 정적 자산, 교체 대상 아님 |
| Hyper-V operation contract | 34 operations |
| Host Ops contract | 9 families / 22 actions |
| 상위 3개 제품 파일 집중도 | 8,355 LOC / 33.338654% |
| 상위 2개 테스트 파일 집중도 | 7,078 LOC / 36.507118% |

주요 집중점은 다음과 같다.

- `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`: 2,420줄
- `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`: 3,897줄
- `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs`: 2,038줄
- `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`: 3,660줄
- `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`: 3,418줄

## 3. 유지해야 할 불변조건

모든 wave는 다음 조건을 기본으로 유지한다.

- Windows Desktop Node 전용 경계를 유지한다.
- Linux/KVM/libvirt/LXC/ZFS/OVS/OVN runtime 코드를 추가하지 않는다.
- Web Console과 PCVCLI만 활성 운영자 표면으로 유지하며 TUI를 되살리지 않는다.
- `DesktopNode.Host.exe`, Local API route, CLI exit code, JSON key와 `PCV_*` 오류 계약을 보존한다.
- 55개 API route(2C `job.reconcile` additive route 포함), 34개 Hyper-V operation, 22개 Host service-action을 승인 없이 추가·삭제하지 않는다.
- ASP.NET Core 도입은 backend HTTP transport 교체이며 TypeScript Web Console의 언어·빌드·브라우저 실행 경계를 대체하지 않는다.
- transport가 바뀌어도 API/Web `127.0.0.1:7777`/`127.0.0.1:80` 분리, LAN opt-in, explicit host/IP binding과 현재 TLS 수명주기 계약을 보존한다.
- ASP.NET Core의 기본 request 병렬성이 application state owner를 우회하지 않도록 Wave 5에서 결정된 serialization 또는 bounded-read 정책을 동일하게 적용한다.
- 동일 process에서 legacy와 ASP.NET Core listener가 같은 제품 port를 동시에 bind하거나 mutation 요청을 한 transport에서 다른 transport로 자동 재실행하지 않는다.
- Hyper-V mutation worker는 기본적으로 단일 consumer를 유지한다.
- generic PowerShell helper fallback을 제품 runtime에 다시 추가하지 않는다.
- Guest Execution의 승인된 PowerShell Direct provider 경계는 별도 결정 없이 변경하지 않는다.
- Host mutation, MSI lifecycle, firewall, Event Log, trust store, 실제 VM mutation은 관리자 명시 승인 없이 실행하지 않는다.
- evidence를 삭제하지 않는다. 필요하면 `current`, `historical`, `supporting`, `closed-not-adopted`로 재분류한다.
- public trusted signing 또는 외부 stable publication을 새로 주장하지 않는다.

## 4. 목표 구조

```text
DesktopNode.Host.exe / WindowsServiceLifetime
└─ ASP.NET Core WebApplication (composition/lifetime root)
   ├─ selected server binding owner (ADR-0014: HTTP.sys 권장 후보 또는 Kestrel)
   ├─ exception/redaction + request-id + admission/body-limit middleware
   ├─ CORS/preflight + current auth/rate-limit/timeout adapter
   ├─ TypeScript Web Console static asset endpoint
   ├─ noVNC WebSocket-to-TCP bridge endpoint
   ├─ DesktopNodeApiTransportAdapter
   │  └─ DesktopNodeApiRequestProcessor (API façade)
   │     ├─ authoritative route registry
   │     ├─ auth/session/RBAC owner
   │     ├─ job runtime owner
   │     │  ├─ state transitions
   │     │  ├─ queue + single mutation worker
   │     │  └─ durable JSON job store
   │     ├─ diagnostics owner
   │     ├─ ops-summary/evidence query owner
   │     └─ Hyper-V adapter façade
   │        ├─ canonical operation registry projections
   │        ├─ domain handlers
   │        └─ provider set
   │           └─ WMI execution seam / concrete WMI providers
   └─ hosted worker + child fault/shutdown supervision

전환 기간에만 transport selector가 위 ASP.NET Core root와 legacy
DesktopNodeHostApplication(HttpListener) 중 정확히 하나를 process startup 시 선택한다.

DesktopNodeHostServiceAction (public service-action façade)
└─ Host Ops catalog
   ├─ service lifecycle owner
   ├─ data-root/migration owner
   ├─ credential/token owner
   ├─ Event Log owner
   ├─ firewall owner
   └─ trust-store owner
      └─ existing Windows controller adapters
```

목표는 클래스 수를 늘리는 것이 아니라, 작은 façade 뒤에 충분한 정책과 불변조건을 숨기는 것이다.

## 5. 범위 제외

- C++ 또는 다른 언어로의 전환
- TypeScript Web Console을 Razor Pages, MVC, Blazor 또는 C# client UI로 교체
- ASP.NET Identity/Entity Framework를 도입하거나 기존 token/account/JWT/RBAC 의미를 같은 transport PR에서 교체
- ASP.NET Core 도입과 동시에 route 계약, JSON schema, 병렬 read 정책 또는 public API version 변경
- IIS/IIS Express를 제품 runtime 또는 배포 전제조건으로 추가
- JSON job store를 즉시 SQLite로 교체
- worker 병렬 mutation 실행
- public API version 변경
- Web Console 화면 재설계
- installer 제품 모델 변경
- public signing, winget submission, 외부 publication 재개
- archive/spike 코드를 활성 실행 경로로 복귀
- 실제 host mutation 자동 실행

## 6. 작업 순서와 의존성

```text
Wave 0 기준선·테스트 안전망
  ├─ Wave 1 Runtime/Core 동작 보존형 분리
  │    └─ Wave 2 Job 영속성·복구 hardening
  │          └─ Wave 5A API/Host async lifetime, serialization 유지
  │                └─ ADR-0012 결과 확정
  │                     ├─ 채택 → Wave 5B bounded concurrent-read `code_complete` ─┐
  │                     └─ 기각 → Wave 5B `closed-not-adopted`, serialization 고정 ─┤
  │                                                                               └─ Wave 6 ASP.NET Core transport 도입
  ├─ Wave 3 Host Ops 실제 소유권 이동
  └─ Wave 4 Hyper-V registry·adapter·WMI seam

Wave 7 Evidence reader·역사적 scaffold·CI 정리
```

`ApiRuntimePolicyRequestProcessorTests.cs`를 동시에 수정하는 workstream은 허용하지 않는다. 먼저 테스트 소유권을 분리한 뒤 병렬 작업을 시작한다.

Wave 6 진입 전 Wave 5A는 반드시 `code_complete`여야 하고 ADR-0012는 채택 또는 기각으로 종결돼야 한다. 5B 구현 채택 여부와 무관하게 확정된 concurrency policy를 transport parity 기준선으로 사용한다. Wave 6의 server 선정 spike, transport 구현, 기본값 전환과 legacy 제거는 각각 분리한다.

---

## Wave 0. 기준선과 리팩터링 안전망

### 목표

동작을 변경하지 않고 현재 계약, 실패 경계, 테스트 소유권과 최초 coverage를 고정한다.

### 변경 등급

- 기본: `M / Full`
- host mutation 수행: `false`
- package/current evidence anchor 변경: `false`

### 대상 파일

- `src/DesktopNode.Api/**` (동작 보존형 job/evidence seam 포함)
- `src/DesktopNode.Api.Tests/**` (golden, recording fake, 소유권 이동 포함)
- `src/DesktopNode.Host.Tests/**`
- `src/DesktopNode.HyperV.Tests/**`
- `packaging/windows-desktop-node/tools/Invoke-PcvDotNetQualityCapture.ps1` (신규)
- `packaging/windows-desktop-node/tools/Test-PcvDotNetQualityRatchet.ps1` (신규)
- `packaging/windows-desktop-node/tests/**` (quality/gap guard와 추적 fixture 포함)
- `docs/DEVELOPER_INDEX.md`
- `docs/superpowers/plans/2026-08-02-purecvisor-desktop-node-csharp-architecture-gap-registry.md` (신규)
- `docs/ga-ready/evidence/csharp-architecture-baseline-<date>.md` (신규 code-level evidence만)

Wave 0에서는 `docs/ga-ready/EVIDENCE_INDEX.md`, `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, `docs/ga-ready/current-evidence.json`과 `docs/ga-ready/current-evidence.schema.json`을 변경하지 않는다.

### 작업

- [x] 54개 API route와 route family, permission, mutation stance snapshot을 고정한다.
- [x] 34개 Hyper-V operation과 domain/dispatch/provider-boundary projection을 고정한다.
- [x] 9개 Host Ops family와 22개 action, mutation boundary, approval guard를 고정한다.
- [x] job store schema v1/v2 load, running recovery, queue order, retention JSON golden을 기록한다.
- [x] API/Host/Hyper-V의 안정된 JSON key, HTTP status, `PCV_*` code만 선택적 golden으로 고정한다.
- [x] `http-transport-contract-v1` manifest에 method, RawTarget/absolute/normalized path, query/trailing-slash/case/percent-encoding 처리, 404/405, exact body/content-type/product header, OPTIONS/CORS, bearer/account auth precedence, known/unknown-length body cap, `/`/missing static/`pcv-config.js`와 noVNC handshake/close 계약을 기록한다.
- [x] static bypass, OPTIONS, noVNC 전용 인증, 일반 API 인증, body read/cap과 route 처리의 현재 우선순위를 실행 가능한 Host characterization으로 고정한다.
- [x] API 테스트 파일에 있는 Hyper-V adapter/WMI mapping 테스트를 `DesktopNode.HyperV.Tests`로 이동한다.
- [x] 테스트 이동 후 전체 test case 수가 감소하지 않는지 확인한다.
- [x] 실제 파일을 생성하는 모든 filesystem test가 per-test GUID sandbox/root를 사용하도록 고정한다.
- [x] job store 저장 실패와 evidence metadata/path 오류에 recording fake를 도입한다. 실제 atomic file 동작 자체가 계약인 테스트는 GUID sandbox의 real filesystem을 유지한다.
- [x] test body의 `Directory.SetCurrentDirectory`를 0건으로 만든다. 남은 process-global CWD fixture는 이름 있는 `Batch evidence CWD isolation` xUnit collection에서 병렬화를 끄고 Wave 7의 configured child-root/path resolver 제거 조건을 기록한다.
- [x] injectable job store/clock/cancellation seam을 동작 보존형으로 추가하고 store call order, queue snapshot과 timeout/cancel propagation의 현재 동작을 실행 가능한 characterization test로 고정한다.
- [x] private reflection과 production source-text 검사 각각을 직접 policy/adapter 테스트로 교체할 후보를 표시한다.
- [x] 각 test project의 TRX와 Cobertura를 고정 results root에 수집하는 quality capture runner를 추가한다.
- [x] 프로젝트별 total/executed/skipped와 line/branch coverage를 읽어 기준 JSON을 생성·비교하는 guard를 추가한다.
- [x] 원본 TRX/Cobertura는 `artifacts/`에 두되, 다음 PR에서도 읽을 수 있는 기준 요약은 추적 fixture에 commit한다.
- [x] test 삭제/이동 시 old test ID, replacement test ID, owner와 coverage boundary를 기록하는 machine-readable migration manifest를 추가한다.

### 필수 fault-injection 테스트 명세

이 wave에서는 아래 실패 시나리오의 재현 조건, 현재 trace, 기대 결과와 담당 후속 wave를 gap registry로 고정한다. 현재 동작과 기대 결과가 다른 테스트를 skipped 상태로 commit하지 않는다. 현재 unsafe trace를 고정하는 임시 characterization test에는 replacement wave와 제거 조건을 migration manifest에 기록한다. 각 후속 wave에서 원하는 안전 동작 테스트를 먼저 RED로 만들고 같은 PR에서 GREEN으로 닫는다.

아래 `[x]`는 안전 동작 구현 완료가 아니라 gap registry에 재현·기대 결과·후속 RED/GREEN 책임을 등록했다는 뜻이다.

- [x] job 생성 저장 실패 후 memory queue에 작업이 남지 않아야 한다. (`W0-FI-01` 재현·기대 안전 결과·Wave 2A RED/GREEN 등록)
- [x] job start/cancel/complete 저장 실패 후 memory/disk 의미가 갈라지지 않아야 한다. (`W0-FI-02` 재현·기대 안전 결과·Wave 2A RED/GREEN 등록)
- [x] GET timeout 응답 후 이전 작업이 새 요청과 겹쳐 상태를 commit하지 않아야 한다. (`W0-FI-03` 재현·기대 안전 결과·Wave 5A RED/GREEN 등록)
- [x] malformed/root-non-object job store가 service startup을 비구조적 예외로 종료하지 않아야 한다. (`W0-FI-04` 재현·기대 안전 결과·Wave 2A RED/GREEN 등록)
- [x] shutdown 중 HTTP request와 noVNC bridge task가 추적되고 종료되어야 한다. (`W0-FI-05` 재현·기대 안전 결과·Wave 5A RED/GREEN 등록)
- [x] listener 또는 worker fault가 service health에 관찰 가능해야 한다. (`W0-FI-06` 재현·기대 안전 결과·Wave 5A RED/GREEN 등록)

### 검증

```powershell
$qualityRoot = 'artifacts/csharp-architecture-quality-baseline-2e98ff4f2df2'
$testProjects = @(Get-ChildItem src -Recurse -Filter '*.Tests.csproj' | Sort-Object FullName)
foreach ($project in $testProjects) {
  $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project.Name)
  dotnet test $project.FullName -c Release `
    --collect:"XPlat Code Coverage" `
    --logger "trx;LogFileName=$projectName.trx" `
    --results-directory "$qualityRoot/test-results"
  if ($LASTEXITCODE -ne 0) { throw "test failed: $projectName" }
}

& packaging/windows-desktop-node/tools/Test-PcvDotNetQualityRatchet.ps1 `
  -ResultsRoot "$qualityRoot/test-results" `
  -BaselinePath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json `
  -MigrationManifestPath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json `
  -AuditBaseCommit 2e98ff4f2df250c36700e86ace0db46ef0aca420 `
  -WriteBaseline

& packaging/windows-desktop-node/tools/Test-PcvDotNetQualityRatchet.ps1 `
  -ResultsRoot "$qualityRoot/test-results" `
  -BaselinePath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json `
  -MigrationManifestPath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json `
  -AuditBaseCommit 2e98ff4f2df250c36700e86ace0db46ef0aca420

1..3 | ForEach-Object {
  dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --no-restore
  if ($LASTEXITCODE -ne 0) { throw "parallel repeat failed: $_" }
}

dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj -c Release
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release
dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj -c Release
dotnet test src/DesktopNode.sln -c Release

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath @('src/DesktopNode.Api','src/DesktopNode.Api.Tests','src/DesktopNode.Host.Tests','src/DesktopNode.HyperV.Tests','src/DesktopNode.Runtime.Tests','packaging/windows-desktop-node') `
  -ArtifactRoot artifacts/development-verification-csharp-architecture-baseline

git diff --check
```

### Wave 0 테스트 안전망 2차 batch 완료 조건

- [x] 이 test-safety batch에서 제품 코드는 동작 변경이 없다.
- [x] 추적 기준 JSON이 감사 전 591 대비 현재 project별 합계 606개와 skip 0, line/branch coverage, SDK/collector version, audit base commit과 현재 .NET source snapshot SHA-256을 기록한다.
- [x] TRX guard가 total/executed/skipped를 기준 JSON과 비교해 무단 test 삭제와 skip 추가를 실패 처리한다.
- [x] Hyper-V 테스트가 소유 프로젝트에서 독립 실행된다.
- [x] per-test unique root가 적용되고 CWD mutation은 0건이거나 명시적으로 직렬화된 예외만 남는다.
- [x] API test suite 3회 반복 실행이 process-global state 경합 없이 PASS한다.
- [x] 이후 wave가 사용할 실패 시나리오마다 재현 조건, 기대 결과, 담당 wave가 기록된다.
- [x] `http-transport-contract-v1`이 현재 `HttpListener` fixture에서 PASS하고 transport-owned 비계약 header allowlist를 별도로 기록한다.
- [x] commit된 test에는 skip이 추가되지 않는다.
- [x] 2차 batch Full runner summary가 `ok=true`이고 선택된 suite가 모두 PASS한다.

### Wave 0 전체 완료 조건

- [x] job store/clock/cancellation seam과 store call-order·timeout/cancel propagation characterization을 동작 보존형 제품-source slice에서 완료한다.
- [x] save-failure와 evidence metadata/path 오류용 recording fake를 추가하고 atomic file 계약만 real filesystem test로 남긴다.
- [x] 위 제품-source slice 적용 후 `M / Full` runner를 다시 실행하고 summary `ok=true`를 확인한다.

Wave 0 최종 product-source checkpoint는 API 174/174 3회 반복, 전체 .NET 611/611·skip 0, quality ratchet line `48.833445%`/branch `39.917355%`, gap registry 8/8, quality tooling 20/20과 `artifacts/development-verification-csharp-architecture-wave0-seams-final-r3/summary.json`의 7-suite `ok=true`를 확인했다. 저장·취소의 현재 unsafe 선후관계는 수정하지 않고 Wave 2A/5A replacement 조건과 함께 characterization으로 남겼다.

### 롤백

테스트 이동만 되돌릴 수 있어야 한다. 제품 source와 테스트 이동을 같은 commit에 섞지 않는다.

---

## Wave 1. Runtime/Core 실제 소유권 분리

### 목표

`DesktopNodeApiRequestProcessor`의 public façade와 현재 직렬화 동작을 유지한 채 job runtime, diagnostics, auth/session·ops dispatch를 독립 PR로 이동한다.

### 변경 등급

- 1A job, 1B diagnostics, 1D ops dispatch: `M / Full`
- 1C auth/session/RBAC owner 이동: 최소 `L / Release`
- API contract 변경: 없음
- 동시성 의미 변경: 없음

### 대상 파일

- `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- `src/DesktopNode.Api/DesktopNodeApiRuntimeCoreHandlers.cs`
- `src/DesktopNode.Api/DesktopNodeApiRuntimeRoutes.cs`
- `src/DesktopNode.Api/DesktopNodeAccountAuth.cs`
- `src/DesktopNode.Api/DesktopNodeApiOpsSummary*.cs`
- `src/DesktopNode.Runtime/JobStateTransitionPolicy.cs`
- 관련 API/Runtime tests

### 작업 1A: job runtime owner

- [x] `DesktopNodeApiRequestProcessor.CreateDefault`와 `Handle`을 호환 façade로 유지한다.
- [x] job dictionary, queue, running cancellation, retention, load/save/recovery를 하나의 실제 `DesktopNodeJobRuntime` owner로 이동한다.
- [x] job 생성은 `DesktopNodeJob.CreateQueued`, 정상 lifecycle 전이는 `JobStateTransitionPolicy`를 통하게 하고 저장 실패·invalid state·running cancellation acknowledgement의 기존 호환 대입은 owner 내부로 제한한다.
- [x] request id/correlation id를 ambient `AsyncLocal` 의존 대신 명시적 request context로 전달한다.
- [x] owner-only state/store/clock/cancellation 검증은 `DesktopNode.Runtime.Tests`에 배치하고 API route/status/JSON/linked-token façade 계약은 `DesktopNode.Api.Tests`에 유지한다.

### 작업 1B: diagnostics owner

- [x] diagnostics handler가 create/list/download, redaction, retention/pagination을 소유하게 한다.
- [x] diagnostics façade의 callback/pass-through를 삭제 테스트로 평가하고 deepen, merge 또는 delete를 선택한다.

삭제 테스트는 기존 3-callback 정적 wrapper가 독립 정책을 갖지 않음을 확인했다. Wave 1B는
wrapper를 삭제하거나 다른 전달 계층으로 병합하지 않고 options, file I/O, redaction,
retention/pagination과 response 조립을 직접 소유하는 instance owner로 `deepen`했다.

### 작업 1C: auth/session/RBAC owner

- [x] auth/session/RBAC handler가 callback 전달자가 아니라 validation과 action 결과 조립을 소유하게 한다.
- [x] JWT expiry, refresh rotation/revoke, role/permission, bearer bootstrap, token/redaction contract를 owner 이동 전후 golden으로 고정한다.
- [x] auth slice는 동작 보존형이어도 security blast radius 때문에 L/Release로 분리하고 1A/1B와 같은 commit에 섞지 않는다.

### 작업 1D: ops dispatch

- [x] ops-summary route에는 명시적 query seam을 만들고 현재 response characterization을 고정한다.
- [x] `BatchEvidenceSummaryReader` 내부 구조 변경은 Wave 7에서만 수행한다.
- [x] 기존 `DesktopNodeApiRuntimeCoreHandlers`의 각 wrapper를 삭제 테스트로 평가하고 deepen, merge 또는 delete를 선택한다.
- [x] 각 1A/1B/1C/1D PR에서 관련 `ApiRuntimePolicyRequestProcessorTests.cs` 부분만 owner 단위 test fixture로 이동한다.

Wave 1D 삭제 테스트는 ops 정적 callback wrapper와 delegate 기반 data builder를 하나의
callback-free query owner와 instance route owner로 `merge + deepen`했다. Job wrapper는 Runtime
state owner 위의 callback-free API job handler, console wrapper는 options/route-ID/capability
projection을 소유하는 instance handler로 각각 `deepen`하는 후속 선택을 기록했으며 ops
변경과 섞지 않았다. `BatchEvidenceSummaryReader`의 path/selection/redaction 내부는 Wave 7에
그대로 남겼다. Owner 전용 fixture와 compiled metadata/IL guard가 source-text guard를
대체하고 `TM-SOURCE-OPS-007`을 `completed`로 닫았다.

### 금지사항

- [x] 전역 request serialization을 이 wave에서 제거하지 않는다.
- [x] JSON job store schema를 변경하지 않는다.
- [x] timeout/rate-limit 동작을 변경하지 않는다.
- [x] route 또는 `PCV_*` code를 변경하지 않는다.

### 검증

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release
dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj -c Release
dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj -c Release
dotnet test src/DesktopNode.sln -c Release

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath @('src/DesktopNode.Api','src/DesktopNode.Runtime','src/DesktopNode.Api.Tests','src/DesktopNode.Runtime.Tests','src/DesktopNode.Host.Tests','packaging/windows-desktop-node') `
  -ArtifactRoot artifacts/development-verification-csharp-runtime-core-wave1a-20260802-final

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath @('src/DesktopNode.Api','src/DesktopNode.Api.Tests') `
  -ArtifactRoot artifacts/development-verification-csharp-diagnostics-owner-wave1b-20260802-final-r3

$wave1dBase = 'e5a3dd0d3001991b3d56415a8ab21f2c6aa21728'
$wave1dPaths = @(& git diff --name-only "$wave1dBase...HEAD" | Sort-Object -Unique)
& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath $wave1dPaths `
  -ArtifactRoot artifacts/development-verification-csharp-ops-dispatch-owner-wave1d-20260802-final

dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release `
  --filter "Auth|Account|Session|Rbac|Jwt|Bearer|Redaction"

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Api/DesktopNodeAccountAuth.cs','src/DesktopNode.Api.Tests') `
  -ArtifactRoot artifacts/development-verification-csharp-auth-owner
```

### 완료 조건

- [x] processor가 job persistence와 queue collection을 직접 소유하지 않는다.
- [ ] 각 wrapper는 삭제 테스트 결과에 따라 deepen, merge 또는 delete되며 wrapper 존속 자체를 완료 조건으로 삼지 않는다.
- [x] Wave 1B diagnostics wrapper는 callback-free instance owner로 deepen되고 processor의 직접 diagnostics file I/O와 정책 helper가 제거됐다.
- [x] compiled architecture guard가 processor façade의 job store/queue concrete type 참조 0건과 owner별 허용 project reference를 검증한다.
- [x] 54개 route와 기존 JSON golden이 동일하다.
- [x] 현재 전체 serialization test는 그대로 통과한다.
- [x] 1C는 JWT expiry/refresh revoke/role/bearer bootstrap/redaction parity와 Release summary `ok=true`를 충족한다.

Wave 1A checkpoint는 request-context, Runtime owner, API façade switch와 compiled ownership guard를 각각 `f57b4fc7`, `9bfdb1a2`, `dfa15e39`, `f116de10`으로 분리하고 `a7a87c7`에서 owner failure branch coverage를 닫았다. API 179/179 3회 반복, 전체 .NET 642/642·skip 0, quality line `50.322384%`/branch `40.897689%`, Runtime owner scoped line `700/749` (`93.457944%`)/branch `215/260` (`82.692308%`)와 `artifacts/development-verification-csharp-runtime-core-wave1a-20260802-final/summary.json`의 Full/M `ok=true`를 확인했다. 전역 request serialization, JSON schema, timeout/rate-limit, 54-route/JSON/`PCV_*` 계약은 유지했고 durability 의미 변경은 Wave 2A로 남겼다.

Wave 1B checkpoint는 diagnostics 동작 보존형 owner 이동, owner test/compiled guard,
private-reflection-free metadata/IL hardening과 timeout trace observer 안정화를 각각
`c8cea02`, `ca9e013`, `91b345b`, `bfced27`으로 분리했다. API 196/196 3회 반복, 전체 .NET
659/659·skip 0, quality line `50.619499%`/branch `41.03664%`, diagnostics owner scoped line `286/299`
(`95.652174%`)/branch `76/89` (`85.393258%`)와
`artifacts/development-verification-csharp-diagnostics-owner-wave1b-20260802-final-r3/summary.json`의
Full/M `ok=true`를 확인했다. 전역 request serialization, auth/RBAC 순서, query와 기존 URI
decode 의미, route/JSON/header/`PCV_*` 계약은 유지했다. 현재 diagnostics redaction 범위의
확대는 동작 보존형 이동에 섞지 않고 별도 security behavior follow-up으로 남긴다.

Wave 1C checkpoint는 auth/JWT/RBAC golden, callback-free instance owner, owner/동시성 테스트와
compiled metadata/IL guard, Host listener JWT lifecycle coverage를 각각 `72096c8`, `e798028`,
`1088d08`로 분리했다. API 209/209 3회 반복, Host 162/162, 전체 .NET 673/673·skip 0,
quality line `51.240143%`/branch `41.651865%`, auth owner scoped line `470/514`
(`91.439689%`)/branch `188/273` (`68.864469%`)와
`artifacts/development-verification-csharp-auth-owner-wave1c-20260802-final/summary.json`의
Release/L `ok=true`를 확인했다. 전역 request serialization, auth-route 선행 순서, unknown-route
`read` fallback, refresh/logout의 메모리 revoke 의미, route/JSON/`PCV_*` 계약은 유지했다.
Account-ready service bearer 해석의 기존 문서/구현 불일치는 이번 이동에서 변경하지 않고 별도
security policy 결정으로 남긴다.

Wave 1D checkpoint는 ops response/query characterization, callback-free query/route owner,
owner direct test와 compiled metadata/IL guard를 각각 `b4f7d4d`, `894a140`으로 분리했다.
API 220/220 3회 반복, Runtime 42/42, Contracts 15/15, 전체 .NET 684/684·skip 0,
quality line `51.410248%`/branch `41.696238%`, ops owner+projection scoped line `397/417`
(`95.203837%`)/branch `128/172` (`74.418605%`)와
`artifacts/development-verification-csharp-ops-dispatch-owner-wave1d-20260802-final/summary.json`의
Full/M `ok=true`를 확인했다. `host.status`→`vm.list` 순서, 동일 timeout token, evidence
degradation의 HTTP 200, job-store pre-gate, 전역 serialization과 route/JSON/`PCV_*` 계약을
유지했다. Job/console의 남은 얕은 wrapper는 별도 callback-free owner slice로 분리하고,
`BatchEvidenceSummaryReader` 내부는 Wave 7에서만 다룬다.

### 롤백

한 route family씩 façade 뒤로 이동한다. migration 중 façade가 old/new 경로를 동시에 실행하거나 결과를 비교하는 shadow mutation은 금지한다.

---

## Wave 2. Job 영속성·복구·중복 실행 hardening

### 목표

job의 memory state, JSON store와 외부 Hyper-V side effect 사이의 실패 의미를 명시적으로 고정한다.

### 변경 등급

- 2A 저장소 seam과 결정 기록: `M / Full`
- 2A의 persistence/restart 동작 변경과 2C의 retry/reconciliation 구현: `L / Release`
- 2B operation 결정표 작성: `M / Full`, host mutation 없음
- 2C 실제 VM 검증: 별도 관리자 명시 승인 필요

### 선행조건

- [x] Wave 0 gap registry, injectable store/clock/cancellation seam과 실행 가능한 store/queue characterization test가 존재한다.
- [x] Wave 1A job runtime owner 분리가 완료됐다.
- [x] 각 2A slice는 원하는 실패 안전성 test를 먼저 RED로 확인하고 같은 PR에서 GREEN으로 닫는다.

각 하위 wave는 독립 PR로 수행한다. 2A를 닫기 전에 2C 구현을 시작하지 않으며, 2C는 승인된 operation 또는 operation family 하나씩 분리한다.

### 작업 2A: atomic store/write와 restart recovery

- [x] next-state를 먼저 계산하고 durable save 성공 후 memory publish와 HTTP success를 반환한다. (create/start/cancel/complete)
- [x] product single SCM runtime의 create/save 실패 시 rejected candidate ghost가 memory/queue/restart에 남지 않도록 한다.
- [x] cancel/start/complete save 실패 시 disk에 남은 이전 상태와 recovery action을 구조화한다.
- [x] job store write를 canonical-path transaction lease와 loaded-base CAS의 단일 writer 경계로 제한한다. (ADR-0013)
- [x] schema version뿐 아니라 job ID, queue reference, queue 중복, status/result/error/attempt 조합의 의미 무결성을 검사한다.
- [x] unique temp, backup/recovery 판정, revision/checksum과 durable flush 필요성을 구현 전 결정 기록으로 확정한다. (`docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`)
- [x] create physical writer에 unique candidate/marker temp, `Flush(true)`, fixed pending guard와 typed `Committed`/`NotCommitted`/`Indeterminate` outcome을 적용한다.
- [x] indeterminate create가 current-binary same-process/restart mutation·dispatch를 차단하고, update/rollback/migration/preserve-data remove가 marker 존재 시 fail-closed하도록 한다.
- [x] v1/v2 read compatibility와 unsupported-future no-mutation contract를 유지한다.
- [x] running cancellation 요청을 먼저 저장하고 state lock 밖에서 provider cancellation을 신호한다.
- [x] cancel requested, provider acknowledged, provider completed-before-cancel과 cancel timeout의 의미를 구분한다.
- [x] runtime policy가 실제 interrupt 가능 operation 목록을 정확히 보고하도록 한다.
- [x] 저장 실패와 restart recovery를 Event Log/diagnostics/ops-summary에서 관찰 가능하게 한다.

### 작업 2B: operation별 reconciliation 결정표

- [x] persisted `running` job을 operation별 readback으로 reconciliation할 수 있는 범위를 분류한다. (`docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2b-operation-reconciliation-decision.md`)
- [x] create/delete/rename/QoS/checkpoint 각각에 expected-before/after, idempotency 근거, readback source, timeout, operator action을 기록한다.
- [x] 결정표 PR에서는 retry/recovery product behavior나 Hyper-V host state를 변경하지 않는다. (`host_mutation_performed=false`, `hyperv_mutation_performed=false`)
- [x] Guest Execution reconciliation은 자동 retry 대상에서 제외하고 별도 설계/작업 계획으로 유지한다. (ADR-0009 boundary)

### 작업 2C: 승인된 operation별 구현

- [x] 2B에서 구현 승인된 operation 하나 또는 동일 의미 family만 L/Release PR로 처리한다. (`vm.rename`, `vm.delete`, `checkpoint.create` 단일 operation slices; actual VM/host mutation은 별도 승인 대기)
- [x] 불명확한 external side effect는 기존 public status와 구조화된 error/metadata로 표현한다. (durable `vm.list`/`checkpoint.list` baseline, `pcv-vm-rename-reconciliation/v1`, `pcv-vm-delete-reconciliation/v1`, `pcv-checkpoint-create-reconciliation/v1`, `PCV_JOB_INTERRUPTED`, `PCV_JOB_RECONCILIATION_REQUIRED`)
- [x] `PCV_JOB_RECONCILIATION_REQUIRED`를 새 error code로 도입한다면 API additive-contract review, Web rendering과 PCVCLI exit-code parity를 같은 slice에서 검증한다. (`POST /api/v1/jobs/{jobId}/reconcile`, Web `Reconcile rename`/`Reconcile delete`/`Reconcile checkpoint`, CLI `job reconcile`)
- [x] 새 public job status가 필요하면 선행 ADR과 Web/CLI 계약 검증 없이는 도입하지 않는다. (new status 없음; confirmed path는 기존 `succeeded`)
- [x] reconciliation 결과와 수동 operator action을 Event Log/diagnostics/ops-summary에 노출한다. (`job-reconciled`/`job-reconciliation-required` runtime observations)

`vm.rename` Wave 2C code-level slice의 단일 진실은
`docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-vm-rename-reconciliation.md`와
`docs/ga-ready/evidence/csharp-architecture-wave2c-vm-rename-reconciliation-2026-08-03.md`다.
이 slice는 기존 202 enqueue semantics를 유지하고, enqueue readback이 unavailable이면
baseline을 `unavailable`로 durable 기록한 뒤 explicit reconcile에서 fail-closed한다. Confirmed
postcondition만 기존 `succeeded`로 commit하며 reconcile endpoint는 provider mutation을 호출하지
않는다. Current operational anchor는 `0.42.65-admin-smoke` carry-forward이고 package candidate,
installed smoke, actual VM validation 또는 promotion은 열지 않았다.

`vm.delete` 후속 Wave 2C code-level slice의 단일 진실은
`docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-vm-delete-reconciliation.md`와
`docs/ga-ready/evidence/csharp-architecture-wave2c-vm-delete-reconciliation-2026-08-03.md`다.
이 slice는 managed ownership marker와 stable VM id를 durable before-state에 고정하고,
captured before-state와 absent inventory만 기존 `succeeded`로 확정한다. same-id 잔존,
재생성 identity, unmanaged collision, 중복 이름과 unavailable readback은 `409
PCV_JOB_RECONCILIATION_REQUIRED`/기존 `failed`로 fail-closed한다. Web `Reconcile delete`와
PCVCLI `job reconcile` parity를 유지했으며 package candidate, installed smoke, actual VM
validation 또는 promotion은 열지 않았다.

`checkpoint.create` 후속 Wave 2C code-level slice의 단일 진실은
`docs/superpowers/specs/2026-08-03-purecvisor-desktop-node-wave2c-checkpoint-create-reconciliation.md`와
`docs/ga-ready/evidence/csharp-architecture-wave2c-checkpoint-create-reconciliation-2026-08-03.md`다.
이 slice는 scoped `checkpoint.list`에서 요청 이름이 사전 부재임을 durable before-state로
고정하고, 동일 VM/이름 row가 정확히 하나일 때만 기존 `succeeded`로 확정한다. not-applied,
duplicate identity, existing-name, unavailable readback은 `409 PCV_JOB_RECONCILIATION_REQUIRED`/
기존 `failed`로 fail-closed하며 `checkpoint.restore` reconciliation은 제외한다. Web
`Reconcile checkpoint`와 PCVCLI `job reconcile` parity를 유지했으며 package candidate,
installed smoke, actual VM validation 또는 promotion은 열지 않았다.

### 저장소 결정

- [x] 이 wave의 기본 구현은 기존 JSON store를 유지한다. (`docs/superpowers/specs/2026-08-02-purecvisor-desktop-node-job-store-durability-decision.md`)
- [x] 기본 rollout은 store schema v1/v2 write compatibility를 유지하고 새 writer 출력이 실제 frozen 0.42.65 binary reader fixture에서 load되는 downgrade test를 통과한다.
- [ ] schema v3 이상이 필요하면 선행 ADR, dual-read/migration, 전환 전 backup과 구 binary rollback 절차를 별도 승인한다.
- [ ] SQLite 또는 별도 process storage로 바꾸려면 별도 ADR을 작성한다.
- [x] repository 오류가 난 mutation 요청을 old backend로 자동 재실행하지 않는다.

### 검증

```powershell
dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj -c Release
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "Job|Worker|Store|Cancel|Retry|Recovery"
dotnet test src/DesktopNode.sln -c Release

$jobHardeningPester = Invoke-Pester `
  -Path packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1 `
  -PassThru -Output Detailed
if ($jobHardeningPester.FailedCount -gt 0) { throw 'job hardening Pester failed' }

pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 `
  -ArtifactRoot artifacts/api-host-job-hardening-wave2-dryrun `
  -DryRun
$jobDryRun = Get-Content artifacts/api-host-job-hardening-wave2-dryrun/summary.json -Raw | ConvertFrom-Json
if (-not $jobDryRun.ok -or $jobDryRun.actual_execution -ne 'dry-run-no-http') {
  throw 'job hardening dry-run summary invalid'
}

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Api','src/DesktopNode.Runtime') `
  -ArtifactRoot artifacts/development-verification-job-durability-plan `
  -PlanOnly

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Api','src/DesktopNode.Runtime') `
  -ArtifactRoot artifacts/development-verification-job-durability
```

`PlanOnly`는 suite 선택 확인용이다. 각 하위 wave 완료 시 `-PlanOnly` 없는 실행의 summary `ok=true`와 선택된 7개 suite PASS를 요구한다. Release lane은 mutation 권한을 부여하지 않는다. 실제 VM reconciliation smoke는 별도 승인된 operational plan에서만 실행한다.

Wave 2A 첫 checkpoint는 결정 commit `1c69ac2a`와 구현 commit `6a4735ef`로 분리했다.
`W0-FI-01`의 store acknowledgement 전 injected failure에서 candidate snapshot을 live state와
분리하고, 정상 store 반환 뒤에만 memory/queue를 publish한다. 실패 시 typed/redacted
`PCV_JOB_STORE_SAVE_FAILED`를 HTTP 503으로 반환하며, 빈 state와 이미 승인된 기존 state 모두
same-process/disk/restart에서 보존되고 worker/native invoke는 0이다. Runtime 42/42, API
220/220 3회, 전체 .NET 684/684·skip 0, quality line `51.518699%`/branch `41.780458%`,
job-hardening Pester 10/10, dry-run `actual_execution=dry-run-no-http`, Release/L 7/7 suite
`ok=true`를 확인했다. Evidence는
`docs/ga-ready/evidence/csharp-architecture-wave2a-job-create-preack-durability-2026-08-02.md`가
소유한다. 이 checkpoint는 physical unique-temp/`Flush(true)`와 post-replace
`indeterminate`, start/cancel/complete, malformed startup을 닫지 않는다.

Wave 2A 두 번째 checkpoint는 구현 commit `4d3a0d9`에서 create physical protocol을 닫았다.
`jobs.json.tmp.<GUID-N>` candidate와 `jobs.json.commit-pending.tmp.<GUID-N>` marker를 각각
exclusive write/`Flush(true)`한 뒤 fixed marker→primary 순서로 publish하고, candidate/previous
SHA-256·length reconciliation으로 typed outcome을 결정한다. unresolved marker나 primary access
failure는 mutation/dispatch를 막으며 Update/Rollback/job-store migration/preserve-data removal도
stop+wait 뒤 marker를 fail-closed 검사한다. Runtime 55/55, API 221/221 3회, Host 164/164,
전체 .NET 700/700·skip 0, product Plan/Invoke 87/87와 quality line `51.492417%`/branch
`41.561001%`를 확인했다. Evidence는
`docs/ga-ready/evidence/csharp-architecture-wave2a-physical-job-store-durability-2026-08-02.md`가
소유한다. Lifetime path lease/CAS, 실제 frozen 0.42.65 reader, FI-02/FI-04와 power-loss/exactly-once는
닫지 않는다.

Wave 2A 세 번째 checkpoint는 start/cancel/complete persist-before-publish, persisted-running
non-retry recovery, semantic integrity, canonical-path transaction lease/loaded-base CAS,
cancel provider signal ordering과 redacted Event Log/diagnostics/ops-summary 관찰성을 닫는다. 실제 frozen
`0.42.65-admin-smoke` reader는 current writer가 생성한 v1/v2 terminal+2-entry FIFO queue store의
initial/rollback-restored 8/8을 읽고 모든 input/output hash를 보존했다. API worker는 일시적 start
commit 실패에서 poll을 계속하되 provider를 한 번만 실행하고,
completion uncertainty에서는 load block으로 provider replay를 막는다. Focused API failure test 11/11과
frozen reader Pester 5/5는 PASS했으며 전체 Release/L 검증과 legacy 설치본 checkpoint 결과는 completion
evidence에서 exact artifact/hash와 함께 고정한다. 이 checkpoint는 process lifetime lease,
mixed-version concurrent writer, Hyper-V side-effect exactly-once 또는 Wave 2B/2C reconciliation을
주장하지 않는다.

초기 2026-08-02 legacy 설치 시도는 host TCP excluded range `7765-7864`가 고정 API 7777을
포함해 `blocked-by-host-tcp-excluded-port-7777`였다. 승인된 재부팅 후 covering range 부재를
재확인하고 동일 `0.42.66-admin-smoke` MSI를 설치해 exit 0, service
`Running`/`Auto`/`LocalSystem`, Web/API/PCVCLI provider-free smoke와 ProgramData store hash 불변을
확인했다. PASS 단일 진실은
`docs/ga-ready/evidence/csharp-architecture-wave2a-legacy-installed-checkpoint-2026-08-03.md`가
소유한다. 이는 Hyper-V/actual-VM/full-admin/package-pair 승격이 아니며 operational 제품 기준선은
계속 `0.42.65-admin-smoke`다. 초기 failed-install orphan-service cleanup 결함은 historical/open으로
보존한다.

### 완료 조건

- [x] product single-runtime create에서 durable enqueue가 실패한 요청은 202를 생성하지 않고 rejected candidate worker invoke count가 0이다.
- [x] durable enqueue 이후 HTTP 응답 전달 실패는 correlation/job 조회로 복구하며, `응답 미수신 = 미실행`을 주장하지 않는다.
- [x] create indeterminate 저장 실패 후 restart해도 unresolved rejected candidate가 무단 실행되지 않는다.
- [x] 실제 side effect 성공 여부가 불명확한 job은 자동 retry되지 않고 승인된 구조화 error/metadata와 operator action을 남긴다.
- [x] exactly-once를 보장할 수 없는 operation에 잘못된 exactly-once claim을 하지 않는다.
- [x] 새 writer store를 실제 frozen 0.42.65 binary reader fixture가 읽으며, rollback backup restore 후 queue/job 의미 무결성이 동일하다.
- [x] focused installed-smoke Pester와 dry-run summary가 PASS하고 `host_mutation_performed=false`다. (code-level dry-run; installed listener 미실행)
- [x] 동일 0.42.66 MSI의 post-reboot legacy 설치본 checkpoint가 service/Web/API/PCVCLI provider-free 검증과 ProgramData hash 불변을 PASS하고, `hyperv_mutation_performed=false`와 operational promotion 비주장을 기록한다.

### 롤백

전환 전 store backup과 양방향 v1/v2 compatibility를 유지한다. rollback은 service stop+wait와 pending marker absence, running job reconciliation, queue drain, backup checksum 검증과 실제 0.42.65 reader load 확인 후 service restart로 수행하며 요청 단위 자동 fallback은 금지한다. marker가 있으면 current marker-aware binary로 먼저 reconciliation하고 구 binary를 시작하지 않는다.

---

## Wave 3. Host Ops operation-family 심화

### 목표

`Host/Ops/*.cs`가 실제 validation, dry-run, mutation, rollback diagnostics와 결과 조립을 소유하게 한다.

### 변경 등급

- 동작 보존형 body 이동: 최소 `M / Full`
- 보안/SCM/firewall/trust-store/credential/host mutation 의미 변경: `L / Release`
- 실제 mutation: 별도 관리자 명시 승인 필요

### 대상 파일

- `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- `src/DesktopNode.Host/Ops/*.cs`
- `src/DesktopNode.Host/DesktopNodeWindows*.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`
- packaging service-plan 관련 Pester tests

### family별 이동 순서

1. config migration / job-store migration
2. Event Log
3. firewall / trust store
4. credential manager / service token
5. data-root lifecycle
6. service lifecycle

각 family는 별도 PR 또는 독립 rollback 가능한 commit으로 이동한다.

### 공통 작업

- [ ] `DesktopNodeHostServiceAction.CreatePlan`과 `ExecuteAsync`를 public façade로 유지한다.
- [ ] `DesktopNodeHostOpsCatalog`를 action/family/mutation-boundary 단일 진실로 유지한다.
- [ ] 해당 family의 `ExecuteNative*ForOps` 본문을 실제 Ops owner로 이동한다.
- [ ] Ops owner가 다시 `DesktopNodeHostServiceAction` 실행 메서드를 호출하지 않도록 한다.
- [ ] 승인 guard, ownership guard, no-PowerShell fallback, redaction과 rollback diagnostics를 이동 전후 비교한다.
- [ ] giant test 파일을 family별 test class/file로 이동한다.
- [ ] façade test에는 catalog completeness, dispatch, public result contract만 남긴다.
- [ ] 모든 family 이동 후 unreachable legacy command branch를 reachability test로 확인하고 별도 삭제 여부를 결정한다.

### family별 완료 조건

- [ ] façade는 family interface/DTO만 참조하고 family owner는 façade implementation type/namespace를 참조하지 않는다는 compiled architecture rule이 PASS한다.
- [ ] family별 production type과 focused test의 owner mapping이 machine-readable rule에 존재하며 허용되지 않은 project reference가 0건이다.
- [ ] 22개 action과 operation-family 이름이 동일하다.
- [ ] `DesktopNodeHostServiceActionTests` focused suite가 PASS한다.
- [ ] packaging service binary-path/plan contract가 동일하다.

### 검증

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release
dotnet test src/DesktopNode.sln -c Release

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Host','src/DesktopNode.Host.Tests') `
  -ArtifactRoot artifacts/development-verification-host-ops-plan `
  -PlanOnly

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Host','src/DesktopNode.Host.Tests') `
  -ArtifactRoot artifacts/development-verification-host-ops
```

`PlanOnly`는 suite 선택 확인용이다. wave 완료 시 `-PlanOnly` 없는 실행의 summary `ok=true`와 선택된 7개 suite PASS를 요구한다. 실제 admin smoke가 필요한 milestone에서는 변경된 family만 먼저 focused smoke하고, package payload가 승격되는 시점에 full admin host mutation gate를 실행한다.

### 롤백

family별 façade dispatch를 이전 구현으로 되돌릴 수 있어야 한다. mutation을 old/new 구현에 이중 실행하는 비교 방식은 금지한다.

---

## Wave 4. Hyper-V registry·adapter·WMI seam 개선

### 목표

기존 public adapter와 structured error contract를 유지하면서 operation 정의 중복, constructor 조립 비용과 WMI 직접 테스트 공백을 줄인다.

### 변경 등급

- catalog projection/adapter body 이동: `M / Full`
- WMI query/invoke/wait 의미 변경: `L / Release + actual-VM evidence`

### 대상 파일

- `src/DesktopNode.HyperV/DesktopNodeHyperVDomain.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVAdapterDispatchCatalog.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVWmiProviderCatalog.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVNativeAdapter.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVProviderSet.cs`
- `src/DesktopNode.HyperV/DesktopNodeHyperVWmi*.cs`
- `src/DesktopNode.HyperV.Tests/**`
- API native-candidate 판정 코드와 contract tests
- `packaging/windows-desktop-node/tools/Invoke-PcvHyperVWmiProviderActualVmParity.ps1` (신규, L slice 전용)

### 작업 A: canonical operation projection

- [ ] 34개 operation을 한 canonical 정의에서 Domain/Dispatch/Provider/API view로 projection한다.
- [ ] 기존 contract key, telemetry name, provider boundary와 handler-registry 결과를 유지한다.
- [ ] catalog 간 drift 비교 test를 projection completeness test로 대체한다.
- [ ] operation 추가 시 수정 지점이 하나인지 test로 고정한다.

### 작업 B: adapter 조립과 domain handler

- [ ] 제품 기본 조립은 기존 `DesktopNodeHyperVProviderSet` 경로로 통일한다.
- [ ] 기존 constructor overload는 caller migration 후 단계적으로 축소하되 즉시 호환성을 깨지 않는다.
- [ ] 테스트는 shared provider-set builder를 사용한다.
- [ ] adapter façade에서 parameter parsing, validation, provider call, result mapping을 domain별 owner로 이동한다.
- [ ] adapter façade에는 catalog lookup, cancellation/error boundary와 dispatch만 남긴다.

### 작업 C: WMI 실행 seam

- [ ] query, method invoke, async WMI job wait, cancellation, disposal을 fake 가능한 실행 경계로 모은다.
- [ ] read-only switch/VM inventory provider부터 적용한다.
- [ ] checkpoint/power/create/delete/resource mutation은 read provider 검증 후 순차 적용한다.
- [ ] HRESULT, async job failure, timeout, malformed property, cancellation과 resource disposal을 직접 테스트한다.
- [ ] 공용 provider-contract suite는 fake executor로 Hyper-V 없는 CI에서 typed mapping을 검증한다.
- [ ] 실제 WMI provider integration은 승인된 actual-VM parity runner에서 별도로 검증하고 fake PASS를 actual provider PASS로 주장하지 않는다.

### 검증

```powershell
dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj -c Release
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "HyperV|Native|Wmi|Domain|Dispatch"
dotnet test src/DesktopNode.sln -c Release

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.HyperV','src/DesktopNode.HyperV.Tests','src/DesktopNode.Api') `
  -ArtifactRoot artifacts/development-verification-hyperv-plan `
  -PlanOnly

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.HyperV','src/DesktopNode.HyperV.Tests','src/DesktopNode.Api') `
  -ArtifactRoot artifacts/development-verification-hyperv
```

`PlanOnly`는 suite 선택 확인용이다. wave 완료 시 `-PlanOnly` 없는 실행의 summary `ok=true`와 선택된 7개 suite PASS를 요구한다.

M body-move PR은 위 fake provider-contract suite까지만 요구한다. WMI query/invoke/wait 의미를 바꾸는 L PR은 먼저 다음 non-mutating plan을 생성하고, 명시 승인된 operational plan에서만 `-AllowHostMutation` actual run을 수행한다.

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/tools/Invoke-PcvHyperVWmiProviderActualVmParity.ps1 `
  -ArtifactRoot artifacts/hyperv-wmi-provider-actual-vm-parity-plan `
  -PlanOnly

# 별도 관리자 승인 후에만 실행
pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/tools/Invoke-PcvHyperVWmiProviderActualVmParity.ps1 `
  -ArtifactRoot artifacts/hyperv-wmi-provider-actual-vm-parity `
  -AllowHostMutation
```

### 완료 조건

- [ ] 34개 operation이 단일 source에서 projection된다.
- [ ] public adapter façade와 `PCV_*` 결과가 동일하다.
- [ ] Hyper-V provider 핵심 분기가 Hyper-V 없는 CI에서도 직접 검증된다.
- [ ] 실제 VM mutation은 단일 worker와 기존 approval guard를 유지한다.
- [ ] WMI 의미를 변경한 경우 touched read provider의 actual-VM projection parity와 touched mutation의 pre/post/cleanup evidence가 PASS한다.
- [ ] actual-VM artifact는 provider 종류, pre/post state, cleanup, host mutation 여부와 fake/actual suite 결과를 구분해 기록한다.
- [ ] catalog projection 또는 adapter body 이동만 수행한 PR에는 actual mutation을 완료 조건으로 강제하지 않는다.

### 롤백

read provider와 mutation provider를 분리해 rollout한다. provider별 default composition을 이전 concrete WMI 구현으로 되돌릴 수 있어야 한다.

---

## Wave 5. API/Host async pipeline과 service lifetime hardening

### 목표

5A에서 전역 request serialization을 유지한 채 중첩 `Task.Run + Wait`, 무제한 fire-and-forget request와 미추적 shutdown을 명시적인 비동기 수명주기로 바꾼다. 5B의 bounded concurrent-read 전환은 별도 ADR 채택 후에만 수행한다.

### 변경 등급

- 5A async lifetime/추적, bounded admission, serialization 유지: `L / Release`
- 5B bounded concurrent-read 의미 변경: `L / Release`
- installed listener load/account/noVNC smoke 필요

### 선행조건

- [ ] Wave 1에서 상태 owner가 processor façade 밖으로 분리됐다.
- [ ] Wave 2에서 job durable commit 경계가 고정됐다.
- [ ] timeout 이후 중첩 실행과 shutdown fault-injection test가 존재한다.
- [x] Wave 5 진입 시 신규 `ADR-0012 API read concurrency policy`를 작성해 read 허용 목록, fairness, cancellation과 mutation single-consumer 대안을 검토한다. (`docs/adr/0012-api-read-concurrency-policy.md`)
- [x] ADR-0012가 `적용 중`으로 채택되면 5B를 실행한다. 직렬화 유지 결정이면 ADR을 `폐기됨`으로 종결하고 5B를 `closed-not-adopted`로 닫는다. (`serialized` 결정 및 5B `closed-not-adopted` 완료)

### 작업 5A: async lifetime와 task supervision, serialization 유지

- [ ] request 처리 경로를 end-to-end async로 전환하고 sync-over-async를 제거한다.
- [ ] route timeout, service stop, client disconnect를 request-scoped linked cancellation 경계로 연결한다.
- [ ] client disconnect는 durable enqueue commit 전 request 작업까지만 취소한다. commit된 job은 response 202 전 연결이 끊겨도 유지하며 명시적 job-cancel 또는 service-stop/reconciliation 정책으로만 취소한다.
- [ ] job state, rate limiter, auth revoke state가 각각 독립된 동기화 owner를 갖게 한다.
- [x] request body read와 processor 실행 전에 listener admission을 bounded하게 만들고 request/noVNC task를 모두 추적한다. (`tracked_async_serialized` opt-in code-level slice; legacy default 유지)
- [x] admission 기본값은 active 32, waiting 64로 시작하고 body read 전에 초과 요청을 HTTP 503, `PCV_REQUEST_ADMISSION_LIMIT_EXCEEDED`, `Retry-After`로 거절한다. (Host unit/in-process HTTP test PASS)
- [ ] 새 overload error contract의 Web rendering과 PCVCLI exit-code parity를 같은 5A L slice에서 검증한다.
- [ ] admission은 latest approved transport manifest의 static/Web-only/OPTIONS/noVNC/auth 우선순위를 보존한다. 의도적 우선순위 변경이 필요하면 별도 L contract PR에서 다음 version manifest를 추가한다.
- [x] shutdown 시 새 요청을 차단하고 in-flight request를 취소·drain한 후 listener를 dispose한다. (tracked request task snapshot/drain code-level; installed stop smoke pending)
- [ ] noVNC `WhenAny` 이후 반대 방향 copy task를 취소하고 두 task의 예외를 모두 관찰한다.
- [ ] listener/worker completion 또는 fault를 `DesktopNodeWindowsService`가 관찰하도록 한다.
- [x] listener 두 번째 bind 또는 processor 생성 실패 시 이미 열린 listener를 정리한다. (`fbd4b90`; 점유 Web prefix 회귀 테스트와 API 재바인드 PASS)
- [ ] 5A에서는 명시적 async serialization gate로 기존 전체 request 직렬화 의미를 보존한다.
- [ ] in-flight request, admission reject, queue oldest age, worker heartbeat/current-job duration과 store latency/failure를 관찰 가능하게 한다.
- [ ] 5A는 기존 `HttpListener` transport에서 lifetime 정책을 먼저 안정화하며 ASP.NET Core package/framework reference나 server 기본값을 변경하지 않는다.

### 작업 5B: ADR 승인 bounded concurrent-read

- [ ] ADR 허용 목록의 read-only route만 bounded 병행하고 Hyper-V mutation은 단일 consumer를 유지한다.
- [x] ADR-0012가 직렬화 유지를 결정한 경우 이 구현 체크박스를 적용하지 않고 `closed-not-adopted` 근거를 남긴다.

Rate limiter의 stale identity cleanup, 실제 burst 의미와 동적 `Retry-After`는 async lifetime/concurrent-read와 섞지 않고 별도 L behavior-change 계획으로 분리한다.

### 전환 설정과 제거 기준

- [x] Host 설정 owner가 `request_lifetime_mode=legacy|tracked_async_serialized`를 소유하고, ADR-0012 채택 시에만 `read_concurrency_mode=serialized|bounded`를 추가한다. (`read_concurrency_mode`는 아직 추가하지 않음)
- [x] 5A 초기 기본값은 `legacy`, 5B 전까지 read concurrency 기본값은 `serialized`다. (ADR-0012 최종 불변조건)
- [ ] legacy/tracked mode가 동일한 route/JSON/status/error parity suite를 통과하고, 5B 채택 시 serialized/bounded mode에도 같은 suite를 적용한다.
- [ ] 각 기본값 전환 commit을 구조 이동 commit과 분리한다.
- [ ] legacy lifetime mode는 `tracked_async_serialized` default installed load/shutdown/account/noVNC parity, product-payload package-pair closure, 7일 관찰 기간과 P0/P1 회귀 0건을 충족한 뒤 별도 commit에서 제거한다.
- [x] `read_concurrency_mode`는 ADR-0012가 채택된 경우에만 추가하며, ADR이 기각되면 `serialized`를 설정이 아닌 최종 불변조건으로 유지한다. (`read_concurrency_mode` 미추가)
- [ ] 설정 이름, owner, 기본값 전환 commit, 제거 gate와 긴급 rollback 절차를 code-level evidence에 기록한다.

### 필수 동시성 테스트

- [ ] mutation worker는 항상 최대 1개 operation만 실행한다.
- [ ] 5A에서는 모든 request가 기존처럼 직렬화되고, 5B에서는 ADR 허용 read route만 병행한다.
- [ ] timeout된 route는 이후 상태를 commit하지 않는다.
- [ ] active 32/waiting 64를 넘는 요청은 body read 전에 HTTP 503/`PCV_REQUEST_ADMISSION_LIMIT_EXCEEDED`/`Retry-After`를 반환한다.
- [ ] service stop 중 request body read, native read, diagnostics download와 noVNC가 10초 내 종료되며 초과 시 service health 실패가 기록된다.
- [ ] listener fault 시 service가 `Running but dead` 상태로 남지 않는다.
- [ ] cancellation callback은 job state lock을 보유한 상태에서 실행되지 않는다.
- [ ] disconnect-before-commit은 enqueue/worker invoke 0건이고, disconnect-after-durable-commit-before-202는 저장된 job 1건과 single dispatch를 유지한다.

### 검증

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "Hardening|Concurrency|Timeout|Worker|Cancel"
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release --filter "Application|Listener|NoVnc|Body|Shutdown"
dotnet test src/DesktopNode.sln -c Release

$jobHardeningPester = Invoke-Pester `
  -Path packaging/windows-desktop-node/tests/PcvApiHostJobHardeningInstalledSmoke.Tests.ps1 `
  -PassThru -Output Detailed
if ($jobHardeningPester.FailedCount -gt 0) { throw 'job hardening Pester failed' }

pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/tools/Invoke-PcvApiHostJobHardeningInstalledSmoke.ps1 `
  -ArtifactRoot artifacts/api-host-job-hardening-wave5-dryrun `
  -DryRun
$jobDryRun = Get-Content artifacts/api-host-job-hardening-wave5-dryrun/summary.json -Raw | ConvertFrom-Json
if (-not $jobDryRun.ok -or $jobDryRun.actual_execution -ne 'dry-run-no-http') {
  throw 'job hardening dry-run summary invalid'
}

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Api','src/DesktopNode.Host') `
  -ArtifactRoot artifacts/development-verification-api-host-async-plan `
  -PlanOnly

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Api','src/DesktopNode.Host') `
  -ArtifactRoot artifacts/development-verification-api-host-async
```

`PlanOnly`는 suite 선택 확인용이다. wave 완료 시 `-PlanOnly` 없는 실행의 summary `ok=true`와 선택된 7개 suite PASS를 요구한다.

### 설치본 검증

별도 승인된 package candidate에서 다음을 확인한다.

- [x] Web HTTP 200, Local API auth boundary, PCVCLI exit 0 (`0.42.68-admin-smoke` 관리자 설치 smoke; `docs/ga-ready/evidence/csharp-architecture-wave5a-installed-cli-smoke-2026-08-03-04268.md`)
- [ ] 5A installed listener serialized/admission load, 5B 채택 시 concurrent-read load
- [ ] service stop/start와 in-flight request drain
- [ ] account login/session/RBAC
- [ ] target-backed noVNC bridge
- [ ] diagnostics create/list/download
- [ ] queued mutation와 cancel/recovery
- [x] final service `Running/Automatic` (`0.42.68-admin-smoke` 관리자 설치 smoke)
- [ ] 동일 host 10회 service-start 측정에서 boot-to-listener p95가 기준 대비 `max(10%, 1초)`를 넘지 않는다.

### 완료 조건

- [ ] 5A는 L/Release, installed-smoke Pester/dry-run, overload contract와 serialization parity를 모두 PASS한다.
- [ ] request cancellation과 committed job lifetime 분리 및 before/after-commit disconnect test가 PASS한다.
- [ ] ADR-0012가 `적용 중`이면 5B와 concurrent-read 검증이 PASS한다.
- [x] ADR-0012가 `폐기됨`이면 5B는 `closed-not-adopted`이고 전체 serialization test를 최종 불변조건으로 유지한다. (ADR-0012)
- [ ] Wave 6이 재사용할 admission, serialization/bounded-read, cancellation, worker supervision owner가 transport 내부 객체가 아닌 application lifetime scope에 존재한다.

### 롤백

초기 rollout은 위 전환 설정으로 되돌린다. 단, mutation 요청 하나를 실패 후 다른 backend로 자동 재실행하는 fallback은 금지한다.

---

## Wave 6. ASP.NET Core transport 도입과 HttpListener 퇴역

### 목표

현재 `HttpListener`가 직접 소유하는 API/Web listener, CORS, body limit, static file, noVNC WebSocket과 service lifetime을 ASP.NET Core `WebApplication`으로 옮긴다. `DesktopNodeApiRequestProcessor`와 TypeScript Web Console은 그대로 유지하고 transport adapter만 교체한다. 최종 제품 기본값은 `aspnet_core`이며 legacy `HttpListener`는 검증·관찰 기간 뒤 제거한다.

ASP.NET Core 도입 자체는 확정 범위다. 신규 `ADR-0014 ASP.NET Core server and rollout policy`는 도입 여부가 아니라 HTTP.sys/Kestrel 선택, endpoint/TLS ownership과 단계적 전환 방법을 결정한다. 현재 Windows-only, `HttpListener`, `netsh sslcert`, WebSocket 운영 계약과의 변화가 가장 작은 HTTP.sys를 1차 권장 후보로 삼고 Kestrel을 비교 후보로 검증한다. 기존 `ADR-0013`은 job-store single-writer 결정으로 이미 적용 중이므로 번호를 재사용하지 않는다.

### 기대 효과와 비용

| 항목 | 기대 효과 | 도입 비용/주의점 |
|---|---|---|
| service lifetime | listener·worker fault와 graceful shutdown을 Generic Host 수명주기에 통합 | 기존 `BackgroundService`/`DesktopNodeHostApplication` 책임 재배치 필요 |
| HTTP pipeline | middleware 순서, DI/options, request cancellation과 통합 테스트가 표준화 | 자동 concurrency·body limit·header/JSON 동작이 기존 계약을 바꿀 수 있음 |
| API/static/noVNC | endpoint, static file, WebSocket 지원을 제품 owner별로 분리 가능 | CORS는 WebSocket을 보호하지 않으므로 별도 Origin 검증 필요 |
| 운영·보안 | explicit endpoint, logging/health/metrics 확장 경로가 명확해짐 | HTTP.sys/Kestrel에 따라 URL ACL, TLS certificate, publish/installer 경계가 달라짐 |
| frontend | 기존 TypeScript build 산출물을 그대로 제공 | TypeScript를 제거하거나 client-side 기능을 C#으로 옮기는 효과는 없음 |
| rollout | legacy와 동일 application core를 사용해 계약 비교 가능 | 일시적으로 두 transport 구현을 유지하고 package-pair/rollback evidence를 수집해야 함 |

### 변경 등급

- production에서 도달하지 않는 server compatibility spike와 publish 조사만 `M / Full`
- `--http-transport aspnet-core`로 선택 가능한 제품 경로가 생기는 순간부터 기본값 여부와 무관하게 `L / Release`
- 인증·WebSocket·TLS·service endpoint·기본 transport·installer payload 전환: `L / Release`
- package candidate, service reconfiguration, URL/certificate binding과 installed smoke는 별도 관리자 승인 필요

### 선행조건

- [ ] Wave 1/2의 application state owner와 durable commit 경계가 `code_complete`다.
- [ ] Wave 5A가 `code_complete`이고 request admission, child task supervision과 shutdown drain이 transport-independent owner로 고정됐다.
- [ ] ADR-0012가 채택 또는 기각으로 종결됐으며 5B는 `code_complete` 또는 `closed-not-adopted`다.
- [ ] product-selectable ASP.NET Core slice 전에는 Wave 5의 `request_lifetime_mode=legacy`가 제거되고 `tracked_async_serialized`가 유일한 lifetime이다. 제거 전에는 6A 비제품 spike만 허용한다.
- [ ] Wave 0에서 시작해 Wave 5의 승인된 차이만 version-up한 latest transport manifest가 55-route(2C `job.reconcile` additive route 포함), static, CORS, body cap, account/RBAC와 noVNC `HttpListener` 기준선에서 PASS한다.
- [ ] transport 전환 PR과 concurrency/auth/route/schema 변경 PR을 분리하는 review gate가 존재한다.

### 대상 파일

수정 후보:

- `src/DesktopNode.Host/DesktopNode.Host.csproj`
- `src/DesktopNode.Host/Program.cs`
- `src/DesktopNode.Host/DesktopNodeWindowsService.cs`
- `src/DesktopNode.Host/DesktopNodeHostOptions.cs`
- `src/DesktopNode.Host/DesktopNodeHostApplication.cs` — 전환 기간 legacy owner, 최종 제거 대상
- `src/DesktopNode.Host.Tests/DesktopNodeHostApplicationTests.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeHostOptionsTests.cs`
- `packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1`
- `packaging/windows-desktop-node/installer/PcvDesktopNodeInstaller.Build.psm1`
- `packaging/windows-desktop-node/installer/Product.wxs`
- `packaging/windows-desktop-node/installer/ProductActions.wxs`
- 관련 product/installer Pester와 installed current-card 도구

신규 후보:

- `src/DesktopNode.Host/AspNetCore/DesktopNodeAspNetCoreApplication.cs` — composition, endpoint와 lifetime owner
- `src/DesktopNode.Host/AspNetCore/DesktopNodeApiTransportAdapter.cs` — `HttpContext`와 기존 request/response contract의 단일 mapping owner
- `src/DesktopNode.Host/AspNetCore/DesktopNodeTransportPolicy.cs` — endpoint, admission, middleware 순서와 선택된 server 정책
- `src/DesktopNode.Host/AspNetCore/DesktopNodeNoVncEndpoint.cs` — WebSocket-to-TCP bridge와 양방향 cancellation owner
- `src/DesktopNode.Host.Tests/DesktopNodeAspNetCoreApplicationTests.cs`
- `src/DesktopNode.Host.Tests/DesktopNodeHttpTransportParityTests.cs`
- `packaging/windows-desktop-node/tools/Invoke-PcvAspNetCoreTransportInstalledSmoke.ps1`
- `packaging/windows-desktop-node/tests/PcvAspNetCoreTransportInstalledSmoke.Tests.ps1`

파일 이름은 구현 시 소유권을 더 명확히 표현하도록 조정할 수 있다. 단, 얕은 middleware class를 다수 만드는 대신 위 owner가 body cap, contract passthrough, static containment, noVNC cancellation처럼 함께 변하는 정책을 응집해야 한다.

### 작업 6A: ADR-0014와 server compatibility spike

- [ ] `docs/adr/0014-aspnet-core-server-and-rollout-policy.md`를 작성하고 `docs/ADR_INDEX.md`에 연결한다.
- [ ] ADR은 `ASP.NET Core 채택`을 결정으로 고정하고 `HTTP.sys`와 `Kestrel` 중 product server 하나만 선택한다.
- [ ] 1차 권장안인 HTTP.sys가 아래 gate를 통과하면 채택한다. HTTP.sys가 명시적 gate를 실패한 경우에만 Kestrel 채택 근거와 TLS/package 차이를 ADR에 기록한다.
- [ ] loopback API `127.0.0.1:7777`, loopback Web `127.0.0.1:80`, opt-in explicit LAN IP, 두 listener 동시 시작과 partial-bind cleanup을 비교한다.
- [ ] 기존 HTTP/HTTPS prefix, `netsh sslcert`, certificate store/private-key access, URL ACL, firewall와 LocalSystem service account 수명주기 차이를 비교한다.
- [ ] noVNC WebSocket, request abort, graceful shutdown, response header, static file, content root와 publish output을 비교한다.
- [ ] 현재 `win-x64` self-contained single-file 배포를 유지하고, 선택 server별 package size, cold start와 boot-to-listener p95를 같은 publish 설정으로 측정한다.
- [ ] top-level wildcard(`*`, `+`)와 암묵적 `localhost:5000` bind를 금지하고 명시적 host/IP endpoint만 허용한다.
- [ ] IIS/IIS Express, Razor/MVC/Blazor, ASP.NET Identity가 필요 없음을 ADR에 확인한다.
- [ ] spike는 동적 loopback port, fixture processor와 임시 data root만 사용하며 실제 product job store 또는 host mutation route를 호출하지 않는다.

### 작업 6B: exclusive transport seam과 기존 계약 고정

- [ ] transport parity 전에 noVNC Origin 검증과 static reparse-point hardening을 legacy transport에 각각 독립된 L PR로 먼저 적용하고 보안/installed fixture를 통과한다.
- [ ] 승인된 동작 차이는 기존 `http-transport-contract-v1`을 덮어쓰지 않고 다음 version manifest와 migration mapping으로 기록한다.
- [ ] 두 legacy 보안 hardening, ASP.NET Core adapter 추가와 transport 기본값 전환은 서로 다른 commit/package trigger로 유지한다.
- [ ] startup 설정 owner에 임시 `--http-transport legacy-http-listener|aspnet-core`를 추가하고 evidence key는 `http_transport=legacy_http_listener|aspnet_core`로 고정한다.
- [ ] 최초 opt-in slice의 기본값은 `legacy-http-listener`이며 ASP.NET Core는 명시적 선택과 동적/격리 port에서만 시작한다.
- [ ] selector는 process 시작 전에 transport 하나만 선택한다. 동일 process에서 두 transport를 동시에 시작하거나 한 요청을 실패 후 다른 transport로 재실행하지 않는다.
- [ ] API processor, job runtime/store, mutation worker, auth revoke state, rate limiter와 Wave 5 admission/concurrency gate는 transport별로 복제하지 않고 application lifetime의 단일 shared owner를 사용한다.
- [ ] `DesktopNodeApiTransportAdapter`는 method, normalized path, raw body, request ID, client identity와 Authorization을 기존 `DesktopNodeApiRequest`로 변환한다.
- [ ] adapter는 legacy의 `AbsolutePath.TrimEnd('/')`와 query 제외 의미, percent-encoded slash와 단일 decode 경계를 먼저 보존한다. query 전달/정규화 버그 수정은 별도 behavior PR과 route contract version 없이 transport parity에 섞지 않는다.
- [ ] 기존 `DesktopNodeApiResponse`의 status, exact body, content type와 product headers를 직접 기록하고 자동 `ProblemDetails`, JSON 재직렬화, redirect, response compression을 적용하지 않는다.
- [ ] transport가 자체 추가하는 `Date`, `Server`, connection/transfer header는 golden manifest에서 제품 계약과 분리해 비교·기록한다.
- [ ] 기존 CLI/service command line은 temporary selector 외에 호환성을 유지한다.
- [ ] raw 제품 argv를 `WebApplication.CreateBuilder(args)`에 전달하지 않고 검증된 `DesktopNodeHostOptions`만 endpoint를 소유한다. `--urls`, `ASPNETCORE_URLS`, `HTTP_PORTS`, `HTTPS_PORTS`, `Kestrel:Endpoints`와 appsettings endpoint가 제품 bind 정책을 덮어쓰지 못하게 한다.
- [ ] forwarded-header middleware는 별도 proxy/security 결정 없이 활성화하지 않고 remote client identity와 Host header 정책을 현재 explicit binding 계약에 맞춘다.

### 작업 6C: ASP.NET Core API pipeline과 lifetime

- [ ] `Microsoft.NET.Sdk.Web` 또는 `Microsoft.AspNetCore.App` framework reference 중 선택 결과와 publish 영향은 ADR-0014에 기록하고 선택된 한 방식만 적용한다.
- [ ] `Program.Main`은 `service-action`을 ASP.NET Core builder 생성 전에 분기한다. service-action mode에서는 server, endpoint configuration과 hosted mutation worker를 생성하지 않는다.
- [ ] listen mode는 하나의 `WebApplication`/`WindowsServiceLifetime`만 구성하고 ASP.NET Core를 기존 `BackgroundService` 안의 nested host로 실행하지 않는다.
- [ ] `WebApplication`과 `AddWindowsService`를 하나의 composition root에서 구성하고 service stop token, `RequestAborted`, route timeout을 linked cancellation로 연결한다.
- [ ] ASP.NET Core `RequestAborted`는 Wave 5에서 고정한 request/job lifetime 분리를 그대로 사용하며 endpoint별 worker CTS를 새로 만들지 않는다.
- [ ] 공통 pipeline은 exception redaction과 request ID까지만 공유하고, 이후 순서는 latest transport manifest에서 생성한 endpoint branch policy가 소유한다. 모든 요청을 단일 global middleware 순서로 평탄화하지 않는다.
- [ ] 초기 branch 순서는 loopback static terminal → Web-only route rejection → OPTIONS 204 terminal → noVNC 전용 auth/Origin/WebSocket → Host service-token pre-gate/account-ready pass-through → 비-loopback static terminal → normal API admission/body cap → processor-owned account/JWT/RBAC/rate-limit/timeout/response passthrough를 보존한다.
- [ ] CORS는 current manifest가 정한 API branch/오류 response에만 장식하고 global static middleware 정책으로 확대하지 않는다.
- [ ] 포화 상태 OPTIONS가 204 대신 admission 503으로 바뀌지 않는지 검증한다. oversized 조합은 service-token missing/wrong, account JWT invalid, account-ready pass-through와 login/refresh/runtime-policy bootstrap별로 기존 401/403 대 413 우선순위를 고정한다.
- [ ] Web port의 `/api/**`는 현재 `PCV_API_ROUTE_ON_WEB_PORT`, API port의 잘못된 route와 Web-only route는 현재 status/body 계약을 유지한다.
- [ ] `OPTIONS` 204, allowed origin, `Access-Control-*`, `X-PCV-Request-Id`/`X-Request-Id`, 401/403와 `WWW-Authenticate` 유무를 byte/semantic golden으로 고정한다.
- [ ] ASP.NET Core는 `/api` terminal transport adapter만 소유한다. 55개 method/path 판정은 기존 authoritative registry가 계속 소유하며 Controllers/Minimal API endpoint 목록으로 route catalog를 복제하지 않는다.
- [ ] compiled architecture guard가 ASP.NET Core layer의 route literal/catalog 중복과 endpoint별 concurrency policy 선언을 차단한다.
- [ ] `Content-Length`가 cap을 넘거나 chunked body가 cap을 초과할 때 모두 413/`PCV_REQUEST_BODY_TOO_LARGE`를 반환하고 전체 body를 먼저 buffering하지 않는다.
- [ ] API endpoint의 effective ASP.NET Core server body limit를 `null`로 두고 bounded admission 뒤 제품 streaming reader가 cap+1 byte에서 중단하게 한다. framework generic 413이 제품 최대 `67,108,864` bytes보다 먼저 응답하지 않으며 제품 owner가 모든 초과 413 body를 만든다.
- [ ] known-length와 chunked 각각에 대해 30,000,000 bytes, 64 MiB-1, 64 MiB, 64 MiB+1 경계를 실제 선택 server로 검증하고 cap 초과 payload 전체를 buffering하지 않는다.
- [ ] Wave 5A 최종 정책이 serialization이면 ASP.NET Core에서도 processor 진입 최대 동시성 1을 유지한다. 5B가 채택됐으면 ADR-0012 allowlist read만 bounded 병행한다.
- [ ] mutation queue consumer는 transport와 무관하게 정확히 하나이며 listener/server fault가 Windows Service lifetime에 전파된다.
- [ ] shutdown은 HTTP admission close → body/native-read/download/response와 noVNC session drain → 별도 mutation worker idle/cancel/reconciliation 확인 → terminal persistence/checksum 확인 → server dispose 순서로 수행한다.
- [ ] HTTP in-flight 0과 mutation worker idle을 별도 상태로 측정하고, 정상 WebSocket close와 기한 초과 강제 종료를 구분해 service health에 기록한다.

### 작업 6D: TypeScript static Web과 noVNC parity

- [ ] `web/**`의 TypeScript source, npm build와 browser runtime은 유지하고 ASP.NET Core는 기존 build output만 정적 자산으로 제공한다. 설치 host에 Node.js runtime을 추가하지 않는다.
- [ ] `AppContext.BaseDirectory`/명시적 `--web-root`를 기준으로 content root를 해석하고 Windows Service의 `C:\Windows\System32` current directory에 의존하지 않는다.
- [ ] static endpoint는 현재처럼 GET-only이며 `/`만 `index.html`로 mapping한다. HEAD, Range, directory browsing, SPA missing-route fallback과 `/api/**`→index fallback을 자동 활성화하지 않는다.
- [ ] missing static은 404/`PCV_STATIC_FILE_NOT_FOUND`를 유지하고 `pcv-config.js`는 API base를 동적으로 생성하며 명시적 cache policy를 golden에 고정한다.
- [ ] `/`, `pcv-config.js`, content type, query string, cache/Last-Modified header, missing file와 path traversal/reparse-point 차단 계약을 갱신된 legacy security 기준선과 동일하게 검증한다.
- [ ] `--web-root` 밖 접근과 reparse escape를 차단하고 served asset bytes의 SHA-256을 packaged Web payload와 비교한다.
- [ ] loopback/비-loopback static asset 인증 규칙과 Web/API port split을 유지한다.
- [ ] noVNC endpoint는 기존 bearer/account `console.view` 권한, VM id decode, target loopback/LAN 정책과 404/401/403 오류 body를 유지한다.
- [ ] WebSocket은 CORS header에 의존하지 않는다. legacy-first L PR은 configured Web origin 일치+유효 auth를 허용하고, non-empty invalid origin은 403, missing origin은 loopback client+유효 bearer/RBAC에서만 허용하며 LAN missing origin은 거절하는 초기 정책을 검토·확정한다.
- [ ] `Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1`와 browser fixture에 allowed/invalid/missing Origin, 101/401/403/404, fragmented binary frame, half-close와 service-stop handle leak 검증을 추가한다.
- [ ] subprotocol, binary frame, close status/reason, TCP connect timeout, client disconnect와 target disconnect를 양방향 fixture로 검증한다.
- [ ] 한쪽 copy가 끝나면 반대쪽을 cancel하고 두 task의 예외를 관찰하며 service stop 시 WebSocket/TCP handle이 남지 않는다.

### 작업 6E: code-level parity와 품질 gate

- [ ] 55-route manifest의 method/path별 legacy/ASP.NET Core status, content type, exact JSON/body와 product header를 비교한다.
- [ ] 정상·오류·malformed JSON·body cap·timeout·rate limit·CORS·auth·request-id·diagnostic download를 양 transport fixture에 적용한다.
- [ ] reset 가능한 동일 fixture/state snapshot을 사용해 `legacy-http-listener`와 `aspnet-core`를 순차 실행하고, mutation 요청을 mirror하거나 두 transport에 이중 전송하지 않는다.
- [ ] 각 transport에서 선택된 Wave 5 concurrency 정책, global active/waiting admission cap과 shutdown fault injection을 실행한다.
- [ ] ASP.NET Core TestServer/WebApplication fixture와 선택 server의 실제 동적 loopback socket test를 둘 다 둔다. TestServer PASS만으로 Windows binding parity를 주장하지 않는다.
- [ ] browser fixture로 Dashboard/login/session/RBAC/diagnostics/static fallback과 noVNC handshake를 검증하고 TypeScript parity count를 보존한다.
- [ ] publish output에 필요한 ASP.NET Core runtime/framework 자산이 포함되며 `web.config`, IIS 또는 Node.js가 제품 전제조건으로 추가되지 않았음을 검사한다.
- [ ] Web SDK를 선택하면 `IsTransformWebConfigDisabled=true`를 명시하고 현재 self-contained single-file publish, hash/provenance와 clean-host 실행 계약을 보존한다.

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release --filter "AspNetCore|TransportParity|Listener|Static|NoVnc|Cors|Body|Shutdown"
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "Contract|Auth|Hardening|Concurrency|Timeout"
dotnet test src/DesktopNode.sln -c Release

npm ci --prefix web
npm test --prefix web
npm run verify:parity --prefix web

foreach ($pesterPath in @(
  'packaging/windows-desktop-node/tests',
  'packaging/windows-desktop-node/installer/tests',
  'web/tests'
)) {
  $pester = Invoke-Pester -Path $pesterPath -PassThru -Output Detailed
  if ($pester.FailedCount -gt 0) { throw "Pester failed: $pesterPath" }
}

$transportPester = Invoke-Pester `
  -Path packaging/windows-desktop-node/tests/PcvAspNetCoreTransportInstalledSmoke.Tests.ps1 `
  -PassThru -Output Detailed
if ($transportPester.FailedCount -gt 0) { throw 'ASP.NET Core transport Pester failed' }

pwsh -NoProfile -ExecutionPolicy Bypass `
  -File packaging/windows-desktop-node/tools/Invoke-PcvAspNetCoreTransportInstalledSmoke.ps1 `
  -ArtifactRoot artifacts/aspnet-core-transport-wave6-dryrun `
  -DryRun
$transportDryRun = Get-Content artifacts/aspnet-core-transport-wave6-dryrun/summary.json -Raw | ConvertFrom-Json
if (-not $transportDryRun.ok -or $transportDryRun.actual_execution -ne 'dry-run-no-http') {
  throw 'ASP.NET Core transport dry-run summary invalid'
}

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Host','src/DesktopNode.Host.Tests','web','packaging/windows-desktop-node') `
  -ArtifactRoot artifacts/development-verification-aspnet-core-transport-plan `
  -PlanOnly

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Release -ChangeTier L `
  -ChangedPath @('src/DesktopNode.Host','src/DesktopNode.Host.Tests','web','packaging/windows-desktop-node') `
  -ArtifactRoot artifacts/development-verification-aspnet-core-transport
```

`PlanOnly`와 dry-run은 실제 port/service/host mutation 성공 evidence가 아니다. 구현된 파일의 실제 changed-path 목록으로 `-PlanOnly` 없는 Release preflight를 PASS해야 code-level gate를 닫는다.

### 작업 6F: package 기본값 전환과 installed 검증

- [ ] 첫 candidate는 legacy 기본값을 유지한 채 ASP.NET Core opt-in publish/MSI payload와 dry-run 계약만 검증한다.
- [ ] update 전에 SCM `PathName`, configured/effective transport, URL ACL, SSL certificate binding과 제품 소유 firewall rule snapshot을 캡처한다. rollback은 직전 legacy package의 `PathName`을 byte-for-byte 재구성해 `--http-transport`를 이해하지 못하는 구 binary에 새 flag를 남기지 않는다.
- [ ] 별도 승인된 격리 smoke에서는 mutation-disabled fixture/data root와 비제품 loopback port로 ASP.NET Core를 실행하며 설치 서비스와 동일 product port에 동시 bind하지 않는다.
- [ ] code-level parity가 PASS한 뒤 별도 L commit/package candidate에서 service command line 기본값을 `--http-transport aspnet-core`로 전환한다.
- [ ] service plan, repair/update/rollback, MSI payload manifest, dependency inventory와 uninstall cleanup이 선택 server에 맞게 갱신된다.
- [ ] self-contained single-file publish의 실제 파일 inventory를 비교하고 sidecar가 추가되면 WiX payload, manifest와 payload aggregate hash 산출을 갱신한다.
- [ ] 선택 server가 HTTP.sys이면 explicit URL prefix/SSL certificate binding의 install/repair/remove idempotency를 확인한다. Kestrel이면 기존 `netsh sslcert` 경계를 대체하는 certificate private-key access와 rollback을 별도 보안 gate로 검증한다.
- [ ] `Invoke-PcvInternalHttpsTlsLifecycleSmoke.ps1`의 certificate generate/bind/rotate/remove와 HTTP restore 경로를 다시 실행한다.
- [ ] explicit LAN IP opt-in, token requirement, firewall apply/readback/restore와 wildcard binding 0건을 확인한다.
- [ ] installed Web `http://127.0.0.1:80/` 200, API `http://127.0.0.1:7777/api/v1/runtime/policy` 200, Web-port API rejection, OPTIONS 204와 PCVCLI exit 0을 확인한다.
- [ ] Web 80/API 7777 split에서 `/pcv-config.js`, static fallback/cache/content type와 API base projection을 확인한다.
- [ ] account login/session/RBAC, diagnostics create/list/download, target-backed noVNC, queued mutation/cancel/recovery와 service stop/start drain을 확인한다.
- [ ] listen/service-action mode의 exit 0/1/2와 구조화 오류 parity, bind 둘 중 하나 실패 시 전체 service non-running/fault 관찰을 확인한다.
- [ ] service stop은 HTTP/noVNC drain과 worker reconciliation을 포함해 10초 policy를 지키고, 동일 host 10회 start/stop 및 reboot 후 `Running/Automatic`과 endpoint readiness를 확인한다.
- [ ] Event Log가 startup/bind/shutdown fault를 token·credential·Authorization 없이 기록하고 기존 service recovery/failure-action 계약을 유지하는지 확인한다.
- [ ] current-card와 ops summary가 `http_transport=aspnet_core`, 선택 server, 두 explicit endpoint, active surface Web/CLI와 TUI absent를 표시한다.
- [ ] current-card는 configured/effective transport 불일치, 알 수 없는 transport 값 또는 예상 server process 부재를 실패 처리하며 조용한 legacy fallback을 허용하지 않는다.
- [ ] 동일 host 10회 service-start 측정에서 boot-to-listener p95가 legacy 기준 대비 `max(10%, 1초)`를 넘지 않고 sustained fixture load의 error/latency/admission 지표가 합의된 budget을 통과한다.
- [ ] package/full admin host mutation gate는 새 candidate가 열리고 관리자가 명시 승인한 경우에만 실행하며 `AllowUnsignedDev` 범위를 public release evidence로 확장하지 않는다.
- [ ] clean-host에 사전 설치된 .NET/ASP.NET Core runtime이 없어도 self-contained `DesktopNode.Host.exe`가 시작되는지 확인한다.
- [ ] clean install, legacy→candidate update, candidate repair/rollback/uninstall과 정책상 해당되는 Burn/MSIX gate를 실행하고 최종 제품 소유 orphan URL ACL/SSL binding/firewall rule/process/file 0건을 확인한다.

### 전환·제거 gate

- [ ] 운영 상태 이름은 API/Web port 구성을 뜻하는 `surface_topology=combined|api_web_split`과 transport rollout을 뜻하는 `http_transport_rollout=legacy_default|aspnet_opt_in|aspnet_default_legacy_retained|aspnet_only`로 분리한다.
- [ ] ASP.NET Core 기본값 candidate의 package build, 필요한 full admin host mutation, installed current-card와 touched actual-VM smoke가 PASS한다.
- [ ] 기본값 전환 후 한 개의 closed manual-admin package-pair, 최소 7일 관찰과 P0/P1 transport 회귀 0건을 충족한다.
- [ ] 최초 ASP.NET Core 기본값 package는 직전 검증된 legacy package와 install/update/rollback pair를 닫는다. 0.42.65에서 중간 schema/package 변경 없이 직접 rollback 가능하다고 주장하려면 0.42.65 대상 실제 rollback을 별도로 PASS해야 한다.
- [ ] 관찰 기간에는 startup selector로만 legacy rollback을 허용하고 state schema/job store는 양 transport가 동일하게 사용한다.
- [ ] 제거 PR에서 `DesktopNodeHostApplication(HttpListener)`, selector의 legacy 값과 관련 package/test branch를 삭제하고 ASP.NET Core를 유일한 runtime transport로 만든다.
- [ ] legacy 제거를 새 product payload candidate로 취급하고 final MSI/hash에 대해 package build, 필요한 full gate, `aspnet_only` installed current-card, clean install/update/repair/uninstall과 legacy-retained 직전 package로의 rollback rehearsal을 PASS한다.
- [ ] legacy-removal candidate가 직전 legacy-retained package와 별도 closed package-pair를 닫고, 제거 후 7일 P0/P1 transport 회귀 0건을 기록한 뒤에만 final `promotion_complete`를 부여한다.
- [ ] legacy characterization은 삭제하지 않고 historical fixture/evidence로 보존하되 production project reachability는 0건이어야 한다.
- [ ] selector 제거 뒤 emergency rollback은 검증된 이전 MSI/package 또는 revert build로만 수행하며 request-level/server-level 자동 fallback을 추가하지 않는다.

허용 상태 전이는 다음과 같고, 단계를 건너뛰는 package/current-card는 guard가 실패 처리한다.

| 현재 상태 | 허용 다음 상태 | 실패 시 복귀 |
|---|---|---|
| `legacy_default` | `aspnet_opt_in` | 변경 없음 |
| `aspnet_opt_in` | `aspnet_default_legacy_retained` | admission drain 후 `legacy_default` |
| `aspnet_default_legacy_retained` | `aspnet_only` | 검증된 legacy-retained 직전 package/MSI 재설치 또는 확인된 MSI transactional rollback → exact SCM `PathName` 복원 → `legacy_default` |
| `aspnet_only` | in-place 역전이 없음 | 검증된 legacy-retained 이전 package/MSI 재설치 또는 revert 뒤 exact SCM `PathName` 복원 |

### 완료 조건

- [ ] ADR-0014가 적용 중이며 선택 server, endpoint/TLS owner, middleware 순서, 관찰·rollback 기준을 명시한다.
- [ ] ASP.NET Core가 유일한 제품 기본 transport이고 암묵적 `localhost:5000`, wildcard binding, IIS/Node.js runtime 의존성이 없다.
- [ ] TypeScript Web Console과 55-route/JSON/PCV error/CLI 계약이 보존된다.
- [ ] ADR-0012의 최종 concurrency 정책과 single mutation worker가 transport parity/load test에서 유지된다.
- [ ] API/static/noVNC/service/package installed 검증과 요구된 promotion evidence가 PASS한다.
- [ ] `promotion_complete`가 default-switch predecessor가 아니라 legacy-removal final package의 version/MSI SHA-256/payload SHA-256/provenance commit에 귀속된다.
- [ ] legacy HttpListener production path 제거와 historical evidence 보존이 architecture guard로 확인된다.

### 롤백

기본값 전환 전에는 selector를 `legacy-http-listener`로 되돌리고 service를 drain/restart한다. package rollback은 사전 캡처한 이전 SCM `PathName`을 복원해 구 binary에 새 option을 전달하지 않는다. 같은 요청이나 uncertain mutation을 다른 transport로 재실행하지 않으며 job store/schema를 변환하지 않는다. legacy 제거 후에는 검증된 직전 package 복원 또는 revert candidate만 사용하고, HTTP.sys↔Kestrel 자동 전환은 허용하지 않는다. 각 rollback은 선택된 package hash, configured/effective transport, service endpoint final state, job recovery 결과와 `host_mutation_performed`를 evidence에 남긴다.

---

## Wave 7. Evidence reader·역사적 scaffold·CI 품질 정리

### 목표

남은 얕은 모듈과 역사적 candidate contract를 정리하고 구조 회귀를 자동 감지한다.

### 변경 등급

- `M / Full`
- 프로젝트/패키지 payload 변경 시 packaging/installer suite 포함

### 작업 A: Batch evidence reader

- [ ] configured root/reparse-point/path containment 정책을 독립된 깊은 모듈로 이동한다.
- [ ] artifact discovery/latest selection을 schema projection과 분리한다.
- [ ] manual-admin/public-boundary/operational evidence projector를 각 schema owner로 분리한다.
- [x] path security test에서 private reflection을 제거하고 internal policy/file-access seam을 직접 검증한다. (Wave 0 완료)
- [ ] malformed child artifact가 ops-summary 전체를 실패시키지 않는 degraded contract를 유지한다.

### 작업 B: 역사적 scaffold

- [ ] `DesktopNode.Service`의 실제 production caller가 없는지 다시 확인한다.
- [ ] `ServiceHostCandidateContract`와 `ServiceLifecycleAdapterContract`를 current owner로 병합할지 historical 문서로 전환할지 결정한다.
- [ ] Host의 unused `DesktopNode.Service` project reference를 제거한다.
- [ ] `ApiHostCandidateContract`를 authoritative 55-route contract로 대체하고 historical evidence link를 보존한다.
- [ ] 제거되는 scaffold test의 각 불변조건을 살아 있는 contract/owner test로 이전한다.
- [ ] solution/test count의 예상 증감, packaging payload와 documentation guard를 갱신한다.

### 작업 C: CI 품질 ratchet

- [ ] shared build 설정을 도입해 nullable/analyzer/warnings 정책을 한 곳에서 관리한다.
- [ ] `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-rules.json`과 `src/DesktopNode.Contracts.Tests/CSharpArchitectureGuardTests.cs`를 architecture rule의 추적 source로 추가한다.
- [ ] raw TRX/Cobertura는 `artifacts/csharp-architecture-quality-<audit-base>`에 보존하고, audit base commit·deterministic .NET source snapshot·SDK/collector version·test count·프로젝트별 line/branch coverage 요약은 추적 baseline fixture로 관리한다.
- [ ] touched production project는 line과 branch coverage 모두 기준 대비 허용 하락폭 `0.0%p`를 기본값으로 적용한다.
- [ ] generated code, designer, platform interop thin wrapper처럼 제외가 필요한 파일은 경로·사유·owner·만료 조건이 있는 allowlist로만 제외한다.
- [ ] 불가피한 하락 예외는 수치, 대체 failure-boundary test와 만료 wave를 PR evidence에 기록하고 무기한 전역 완화는 금지한다.
- [ ] 전체 line coverage 숫자만 목표로 삼지 않고 job state, host mutation guard, WMI failure branch를 우선한다.
- [ ] façade 금지 type/namespace dependency, owner별 허용 project reference와 composition-root reachability를 machine-readable rule로 정의한다.
- [ ] API→Hyper-V 테스트 역배치, Host Ops→façade callback과 orphan production project를 compiled metadata/Roslyn architecture guard로 차단하고 production source-text 검사는 사용하지 않는다.
- [ ] production 파일 1,000 LOC 또는 기준 대비 15% 증가, test 파일 2,500 LOC 또는 150 test case 초과를 hotspot으로 기록한다.
- [ ] threshold를 넘는 신규 파일은 실패 처리하고 기존 hotspot은 증가 금지 ratchet과 owner/분할 wave를 CI artifact에 남긴다.

### 검증

```powershell
$qualityRoot = 'artifacts/csharp-architecture-quality-final'
& packaging/windows-desktop-node/tools/Invoke-PcvDotNetQualityCapture.ps1 `
  -SolutionPath src/DesktopNode.sln `
  -ArtifactRoot $qualityRoot

& packaging/windows-desktop-node/tools/Test-PcvDotNetQualityRatchet.ps1 `
  -ResultsRoot "$qualityRoot/test-results" `
  -BaselinePath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-quality-baseline.json `
  -MigrationManifestPath packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json

dotnet build src/DesktopNode.sln -c Release -warnaserror

$changedPaths = @(& git diff --name-only origin/main...HEAD)
if ($LASTEXITCODE -ne 0 -or $changedPaths.Count -eq 0) {
  throw 'actual changed-path list is required'
}

& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath $changedPaths `
  -ArtifactRoot artifacts/development-verification-csharp-architecture-final

$verification = Get-Content artifacts/development-verification-csharp-architecture-final/summary.json -Raw | ConvertFrom-Json
if (-not $verification.ok) { throw 'development verification failed' }
if ($changedPaths -contains 'docs/ga-ready/EVIDENCE_INDEX.md' -and
    $verification.tier_reasons -notcontains 'current-evidence-anchor') {
  throw 'current evidence anchor was not classified as L'
}

git diff --check
```

### 완료 조건

- [ ] composition-root reachability guard가 active production project의 runtime caller를 compiled metadata로 증명한다.
- [ ] evidence reader의 path security와 schema projection이 독립 테스트된다.
- [ ] TRX guard가 test count/skip, coverage guard가 line/branch `0.0%p` 하락, analyzer build가 warning을 차단한다.
- [ ] 실제 changed-path 분류 summary가 L anchor/installer/security 경로의 tier reason을 누락하지 않는다.
- [ ] 문서가 제거된 candidate contract를 current 제품 경계로 계속 주장하지 않는다.

---

## 7. PR/commit 분할 권고

| 순서 | 변경 단위 | 권장 등급 | 병렬 가능 여부 |
|---:|---|---|---|
| 1 | Baseline snapshot과 fault-injection test | M / Full | 단독 |
| 2 | Hyper-V 테스트 소유권 이동 | M / Full | 1 이후 |
| 3 | Wave 1A Job runtime 동작 보존형 추출 | M / Full | 단독 |
| 4 | Wave 1B Diagnostics owner 이동 | M / Full | 3 이후 1C/1D와 분리 |
| 5 | Wave 1C Auth/session/RBAC owner 이동 | L / Release | 3 이후 단독 |
| 6 | Wave 1D Ops dispatch 이동 | M / Full | 3 이후 1B/1C와 분리 |
| 7 | Wave 2A Job atomic store/restart recovery | M 또는 L | 3 이후 |
| 8 | Wave 2B Operation reconciliation 결정표 | M / Full | 7 이후, 코드 변경과 분리 |
| 9 | Wave 2C 승인 operation별 reconciliation | L / Release | 8 이후 operation별 단독 |
| 10 | Host Ops family별 body 이동 | M 또는 L | 2 이후 API 작업과 병렬 가능 |
| 11 | Hyper-V canonical registry/adapter split | M / Full | 2 이후 |
| 12 | WMI read seam | M / Full | 11 이후 |
| 13 | WMI mutation seam | L / Release | 12 이후 |
| 14 | Wave 5A API/Host async lifetime/admission, serialization 유지 | L / Release | 7 이후 |
| 15 | Wave 5B bounded concurrent-read 또는 closed-not-adopted | L / Release + ADR-0012 | 14 이후, ADR 결정에 따름 |
| 16 | ADR-0014와 HTTP.sys/Kestrel compatibility spike | M / Full | 15의 결과 확정 후 단독 |
| 17 | legacy noVNC Origin hardening과 manifest version-up | L / Release | 16 이후, transport 변경과 분리 |
| 18 | legacy static reparse containment hardening과 manifest version-up | L / Release | 17과 독립된 PR |
| 19 | exclusive transport seam과 legacy parity fixture | L / Release | 17~18 이후, 기본값 유지 |
| 20 | ASP.NET Core API pipeline/lifetime | L / Release | 19 이후, concurrency 변경 금지 |
| 21 | TypeScript static/noVNC transport parity | L / Release | 20 이후, UI 언어 변경 금지 |
| 22 | ASP.NET Core package 기본값 전환 | L / Release | 21 및 명시 승인 후 단독 |
| 23 | 관찰·rollback rehearsal 뒤 legacy HttpListener 제거 | L / Release | 22의 제거 gate 충족 후 |
| 24 | Evidence/scaffold/CI 정리 | M / Full | 주요 이동과 Wave 6 완료 후 |

각 commit은 다음 중 하나만 수행한다.

- test characterization
- 동작 보존형 코드 이동
- behavior change
- transport opt-in/default 전환 또는 legacy 제거
- evidence/docs 갱신

## 8. 위험 등록부

| 위험 | 영향 | 완화책 | 검출 기준 |
|---|---|---|---|
| 구조 이동 중 JSON/오류 계약 drift | Web/CLI 회귀 | golden contract와 55-route snapshot | API tests/installed current-card |
| job save 실패 후 ghost mutation | 승인 없는 host side effect | persist-before-publish, fault injection | worker invoke count 0 |
| mutation 성공 후 terminal save 전 crash | 중복 실행 | operation별 reconciliation/idempotency | restart recovery smoke |
| timeout 후 inner task 계속 실행 | 상태 경합/WMI 중첩 | async cancellation + post-timeout commit guard | controlled timeout test |
| unbounded request task | thread-pool/메모리 고갈 | bounded admission + task supervision | concurrent load test |
| service Running but listener dead | 운영 불능 | completion/fault propagation | listener fault test/current-card |
| ASP.NET Core 기본 병렬성이 state owner를 우회 | state 경합/중복 mutation | Wave 5 policy를 shared application gate로 재사용 | handler/native max-concurrency test |
| `RequestAborted`가 committed job까지 취소 | 승인된 job 유실/불확실 상태 | durable commit 전 request와 job lifetime 분리 | disconnect-before/after-commit test |
| 자동 JSON/ProblemDetails/header 동작으로 계약 drift | Web/CLI 및 audit 회귀 | raw response passthrough와 transport-header 분류 | 55-route cross-transport golden |
| middleware 순서 오류 | CORS/auth/body-limit 우회 | ADR 순서와 pipeline-order test | OPTIONS/401/403/413 security suite |
| WebSocket에 CORS만 적용 | cross-origin noVNC 접근 | 명시적 Origin + bearer/RBAC 검증 | invalid/missing-origin handshake test |
| endpoint/TLS owner 변경 | port bind 실패 또는 보안 회귀 | HTTP.sys 우선 compatibility gate와 explicit binding | installed HTTP/HTTPS lifecycle smoke |
| Windows Service content root drift | static/config 파일 미발견 | `AppContext.BaseDirectory`/absolute web root | installed service static smoke |
| legacy/ASP.NET 동시 product 실행 | 이중 state/worker 또는 port 충돌 | process 시작 시 exclusive selector | duplicate bind/single-worker test |
| Host Ops body 이동 중 approval guard 손실 | 보안/OS mutation 위험 | family별 characterization + L gate | dry-run/mutation evidence reason |
| WMI seam이 실제 COM 의미를 왜곡 | 실제 VM 실패 | read-first rollout + actual-VM smoke | provider parity evidence |
| stale scaffold 제거로 evidence link 단절 | 감사 추적성 손실 | historical reclassification | docs guard/EVIDENCE_INDEX |
| coverage 목표가 의미 없는 test를 유도 | 유지보수 비용 증가 | branch/failure-boundary 중심 ratchet | review checklist |

## 9. ADR 작성 조건

다음 변경은 기존 구현 계획만으로 처리하지 않고 ADR 또는 명시적 follow-up design을 작성한다.

- JSON job store를 SQLite/다른 storage engine으로 교체
- job worker를 다중 mutation worker로 전환
- Hyper-V mutation을 별도 process로 격리
- public API route/version/JSON 계약 변경
- 장기 concurrency 정책을 전체 직렬화에서 병행 read 모델로 변경
- ASP.NET Core server, endpoint/TLS owner와 transport rollout 정책 결정
- ADR-0014 이후 선택 server 또는 certificate binding owner 변경
- auth refresh-token revoke persistence 정책 변경
- Host Ops owner 또는 installer/service process boundary 변경
- public signing/publication 범위 재개

단순한 동작 보존형 body 이동, test 재배치, unused reference 제거는 ADR 대상이 아니다.

## 10. Evidence와 문서 갱신

### 완료 상태 모델

구현 상태와 승격 상태는 서로 독립된 두 축으로 기록한다.

| 축 | 상태 | 의미 |
|---|---|---|
| 구현 | `code_ready_operational_pending` | source/test/등급별 preflight는 PASS했지만 해당 wave가 명시한 actual-host 검증이 남았다. |
| 구현 | `code_complete` | code-level 검증과 해당 wave 고유 완료 조건이 모두 PASS했다. |
| 구현 | `closed-not-adopted` | ADR이 대안을 기각해 조건부 구현을 하지 않기로 명시적으로 종결했다. Wave 5B에만 허용한다. |
| 승격 | `promotion_not_triggered` | package candidate를 열지 않았다. 0.42.65 anchor carry-forward와 stale trigger를 기록한다. |
| 승격 | `promotion_pending` | candidate를 열었지만 package/fullgate/current-card/필수 actual-VM gate 일부가 남았다. |
| 승격 | `promotion_complete` | package build, 필요한 full admin host mutation gate, Web/CLI installed current-card와 touched operation actual-VM evidence가 PASS했다. |

예를 들어 일반 M body-move wave는 `code_complete + promotion_not_triggered`가 될 수 있다. 반면 Wave 4에서 WMI 의미를 바꾼 L slice는 candidate 생성 여부와 무관하게 actual-VM 조건이 남으면 `code_ready_operational_pending`이며 `code_complete`로 올리지 않는다. 승인된 source/installed actual-host run으로 이 구현 조건을 닫을 수 있고, package promotion은 별도 축에서 판정한다. Manual-admin package-pair는 정책 trigger가 발생한 경우에만 승격 조건에 포함한다.

Wave 6은 계획상 채택이 확정된 필수 wave이므로 `closed-not-adopted`를 사용할 수 없다. code-level opt-in만 PASS한 동안은 `code_ready_operational_pending`; 기본값 candidate가 열렸으나 installed/관찰/제거 gate가 남으면 `promotion_pending`; ASP.NET Core 유일 transport와 요구 evidence가 모두 닫힌 뒤에만 `code_complete + promotion_complete`다. `http_transport_rollout`은 진행 단계 표기이며 세 번째 완료 축이 아니다.

각 wave 완료 시 변경 등급과 자동 분류 결과에 맞춰 다음을 갱신한다.

- `docs/DEVELOPER_INDEX.md`
- `docs/ADR_INDEX.md` — ADR이 실제 추가된 경우만
- 해당 code-level evidence 문서 — M wave에서는 여기까지만 같은 code PR에 포함한다.
- `docs/ga-ready/EVIDENCE_INDEX.md` — 별도 `L / Release` 문서 commit으로 갱신하거나 해당 wave 전체를 `L / Release`로 재분류한 경우만 포함한다.
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md` — 검증 정책 자체가 변경된 경우만
- package/fullgate/current-card/actual-VM evidence — 새 payload candidate가 생성된 경우만

문서에는 항상 다음을 명시한다.

- `host_mutation_performed`
- `public_trusted_signing`
- `external_stable_publication`
- 검증 commit과 test counts
- 기존 operational anchor를 carry-forward하는지 여부
- actual-VM/admin smoke 실행 여부

## 11. 최종 수용 기준

### 구조

- [ ] API processor가 route façade 역할에 집중하며 job store/queue 파일 I/O를 직접 소유하지 않는다.
- [ ] compiled architecture guard가 Host Ops owner→giant façade implementation callback 0건을 확인한다.
- [ ] Hyper-V 34개 operation 정의가 단일 source에서 projection된다.
- [ ] ASP.NET Core composition root가 모든 HTTP/noVNC/worker child task와 fault를 추적한다.
- [ ] ASP.NET Core가 유일한 production HTTP transport이고 legacy `HttpListener` reachability가 0건이다.
- [ ] TypeScript Web Console source/build/browser runtime은 유지되고 ASP.NET Core static endpoint가 build output을 제공한다.
- [ ] active project마다 실제 production caller 또는 명시된 독립 제품 계약이 존재한다.

### 정확성

- [ ] durable enqueue 저장이 실패한 job은 worker에서 실행되지 않는다.
- [ ] memory/disk job state divergence가 recovery contract로 처리된다.
- [ ] uncertain external side effect가 자동 중복 실행되지 않는다.
- [ ] timeout/cancellation/service stop 이후 뒤늦은 commit이 없다.
- [ ] durable commit된 job은 client disconnect로 취소되지 않으며 rollback 중 uncertain side effect가 자동 재실행되지 않는다.
- [ ] single mutation worker invariant가 유지된다.

### 테스트

- [ ] 전체 .NET test는 skip 0이며, 기준선 591 대비 count 증감과 제거된 test의 대체 coverage가 문서화된다.
- [ ] Hyper-V provider 주요 실패 branch가 `DesktopNode.HyperV.Tests`에서 직접 검증된다.
- [ ] Host Ops tests가 family별로 독립 실행된다.
- [ ] job save/restart/reconciliation fault-injection tests가 PASS한다.
- [ ] 55-route, static/CORS/body/auth, noVNC와 Wave 5 concurrency의 legacy/ASP.NET Core parity suite가 PASS한다.
- [ ] ASP.NET Core TestServer와 선택 server의 실제 loopback binding test가 모두 PASS한다.
- [ ] package candidate를 생성한 경우 Web/CLI/current-card와 touched operation에 필요한 actual-VM/admin evidence가 PASS한다.
- [ ] candidate를 생성하지 않은 경우 0.42.65 carry-forward와 stale trigger가 `promotion_not_triggered`로 기록된다.

### 제품 경계

- [ ] Web Console과 PCVCLI만 active surface다.
- [ ] IIS, Razor/MVC/Blazor, ASP.NET Identity, Node.js runtime이 제품 의존성으로 추가되지 않았다.
- [ ] API/Web canonical endpoint, LAN opt-in과 명시적 host/IP binding이 유지된다.
- [ ] TUI source/test/package가 재도입되지 않았다.
- [ ] Linux runtime과 generic PowerShell fallback이 추가되지 않았다.
- [ ] public trusted signing/external stable publication claim이 변경되지 않았다.
- [ ] 관리자 승인 없는 host mutation이 실행되지 않았다.

## 12. Definition of Done

이 계획은 다음 조건을 모두 만족할 때 완료한다.

- [ ] Wave 0~7의 완료 조건이 모두 충족됐다.
- [ ] 모든 동작 변경은 동작 보존형 이동과 분리된 PR/commit으로 추적된다.
- [ ] `dotnet test src/DesktopNode.sln -c Release`가 PASS한다.
- [ ] 최종 TRX quality guard가 skip 0, 기준 대비 test count 증감과 모든 제거 test의 migration mapping을 PASS한다.
- [ ] 최종 coverage ratchet이 touched project의 line/branch 허용 하락폭 `0.0%p`를 PASS한다.
- [ ] 최종 변경 등급에 맞는 Full 또는 Release preflight가 PASS한다.
- [ ] 모든 필수 wave가 `code_complete`이고, ADR로 기각된 Wave 5B만 `closed-not-adopted`를 허용한다.
- [ ] Wave 6은 `code_complete + promotion_complete`, `http_transport_rollout=aspnet_only`이며 ADR-0014와 installed/관찰/rollback evidence가 닫혔다.
- [ ] candidate를 생성한 wave만 명시 승인 하에 `promotion_complete` evidence를 수집했으며, candidate 미생성 wave는 `promotion_not_triggered`를 기록했다.
- [ ] `git diff --check`가 PASS한다.
- [ ] current/historical evidence link와 문서 인덱스가 일치한다.
- [ ] 남은 deferred 항목은 별도 backlog 또는 ADR 후보로 기록됐다.
