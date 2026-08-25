# PureCVisor Desktop Node Job Store Atomic Save

**Goal:** .NET Local API job store snapshot write를 partial write/stale temp 파일에 더 강하게 만든다.

**Architecture:** `DesktopNodeApiRequestProcessor.SaveJobStore()`는 기존 JSON shape와 queue semantics를 유지한다. 저장 시 `jobs.json.tmp`에 먼저 snapshot을 쓰고 같은 디렉터리에서 `File.Move(..., overwrite: true)`로 최종 경로를 교체한다. 이전 실패에서 남은 stale temp 파일은 다음 저장 전에 삭제하고, 저장 실패 시 temp 파일을 best-effort cleanup한다.

## Scope

- C# request processor job store save path
- stale temp cleanup
- 기존 job store JSON schema/version 유지

## Out of Scope

- job store schema migration
- destructive migration apply
- service stop/runtime writer coordination
- installed service/MSI mutation smoke

## Tasks

- [x] RED: stale `jobs.json.tmp`가 있는 상태에서 job enqueue 후 temp가 정리되고 valid JSON snapshot이 남아야 한다.
- [x] GREEN: temp write + same-directory replace/move를 `SaveJobStore()`에 적용.
- [x] GREEN: 실패 시 temp cleanup best-effort를 추가.

## Verification

- `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter JobStoreSaveUsesAtomicTempReplaceAndCleansStaleTemp`: RED 확인 후 PASS.
