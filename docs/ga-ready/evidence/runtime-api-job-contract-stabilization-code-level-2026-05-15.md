# Runtime/API Job Contract Stabilization Code-Level Evidence - 2026-05-15

```text
evidence_id: runtime-api-job-contract-stabilization-code-level-2026-05-15
scope: Runtime/API contract stabilization
artifact_or_package_version: branch product payload, package build not run
actual_execution: code-level-tests
host_mutation_performed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

## 요약

이 slice는 Runtime/API route contract가 실제 request processor의 job runtime route를 빠짐없이 노출하도록 보강했다.

- `ApiHandlerAdapterContract`에 `GET /api/v1/jobs/{jobId}`, `POST /api/v1/jobs/{jobId}/cancel`, `POST /api/v1/jobs/{jobId}/retry`를 추가했다.
- `ApiHandlerRouteContract.RequiredPermission`을 additive field로 추가해 Operator Surface가 route별 RBAC permission을 contract에서 읽을 수 있게 했다.
- Runtime policy route는 account auth bootstrap 전에도 읽을 수 있으므로 `RequiredPermission=null`로 고정했다.
- Job list/detail은 `read`, job cancel/retry는 `operate`, diagnostic bundle read/create는 `diagnostics.read`/`diagnostics.create`로 고정했다.
- `ApiHostCandidateContract.PublicRouteCandidates`에 job detail/cancel/retry route template을 추가했다.
- `docs/ga-ready/runtime-core-boundary-baseline-2026-05-11.md`에 RBAC permission contract owner를 추가했다.

## 범위

이 evidence는 code-level Runtime/API contract evidence다. 다음 항목은 이 slice에서 실행하거나 완료로 주장하지 않는다.

- served API response body 변경
- Web Console/TUI/CLI 화면 변경
- package build, package-pair campaign, clean-host campaign
- Hyper-V VM 생성/삭제, Windows service/firewall/Event Log/trust store mutation
- public trusted signing 또는 외부 stable publication

## 검증

TDD RED:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHandlerAdapterContractTests|FullyQualifiedName~ApiHostCandidateContractTests"
```

결과:

- RED: `ApiHandlerRouteContract.RequiredPermission`이 없어 컴파일 실패했다.

GREEN / 영향 범위:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "FullyQualifiedName~ApiHandlerAdapterContractTests|FullyQualifiedName~ApiHostCandidateContractTests"
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj
dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj
dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj
dotnet test src/DesktopNode.sln
git diff --check
```

결과:

- focused contract tests: PASS, 11 tests.
- API tests: PASS, 164 tests.
- Runtime tests: PASS, 17 tests.
- Contracts tests: PASS, 6 tests.
- Full .NET solution: PASS.
- `git diff --check`: PASS.

이 evidence는 `0.42.15-admin-smoke` 이후 product payload 변경 후보를 만든다. package build, package-pair campaign, full admin host mutation은 이 code-level evidence에서 실행하지 않았다.
