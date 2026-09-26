# 기존 실패 테스트 복구 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 모듈 크기 라쳇 복구 뒤에도 남은 기존 실패 `5`건(.NET `3`, Pester `2`)을 고쳐 `dotnet test src/DesktopNode.sln`과 관련 Pester를 green으로 만든다.

**Architecture:** 제품 코드는 바꾸지 않는다. 다섯 건 모두 2026-08-27~09-25 기능 commit이 테스트 기대값이나 단언을 함께 갱신하지 않아 생긴 실패다. 각 task는 원인 commit과 근거를 기록하고, 현재 계약을 기대값으로 수락하거나 잘못 쓴 단언을 바로잡는다.

**Tech Stack:** C# / .NET 10, xUnit, Pester 5

## 현재 상태

기준: public 저장소 `feat/p1-9-account-crud` HEAD `e5ef657` (2026-09-27).

| # | 테스트 | 증상 | 원인 commit | 판단 |
| --- | --- | --- | --- | --- |
| 1 | `ApiHandlerAdapterContractTests.DefaultContractPinsCompleteRoutePermissionAndMutationSnapshot` | 항상 실패하며 digest `40fb6cd6…`를 출력 | `e098e0a` | 이전 pin `Assert.Equal("e23882ba…", digest)`를 digest 출력용 `Assert.Fail(digest)`로 바꾼 채 commit했다. 뒤따르는 개수 단언(`23`/`20`/`37`)은 실행되지 않았다. |
| 2 | `ApiHandlerAdapterContractTests.RuntimeRouteRegistryPublishesNativeQueuedMutationMatchers` | `Assert.False` 실패 (L733) | `e098e0a` | `/api/v1/vms/{vmId}/qos/network`가 queued mutation이 아니라고 단언한다. 같은 파일 L199와 계약은 이 경로를 `QueueSetVmNetworkQos` queued mutation으로 둔다. 새 `/network` 경로 옆에 쓴 단언이 경로를 잘못 골랐다. `network/preview` 경로는 계약에 없다. |
| 3 | `CurrentEvidenceVerifierTests.CanonicalRepositoryPassesWithoutWritingOwnedFiles` | `0.42.75-admin-smoke` 기대, 실제 `0.42.77-admin-smoke` | `a842ede` | 2026-09-20 Lane 3가 current를 `0.42.77`로 승격했지만 `bb15f66`(2026-08-27)의 기대값이 남았다. |
| 4 | Pester `records the completed Hyper-V domain ownership move without losing its 35 cases` | `Expected 39, but got 49` | `a842ede`, `e622f44`, `7e24c75`, `e098e0a` | 기능 commit이 `HyperVDomainContractTests` case를 늘렸지만 registry manifest와 Pester 기대값을 갱신하지 않았다. |
| 5 | Pester `inventories source-text checks and remaining ownership candidates with migration links` | `Expected 42, but got 49` | 같은 계열 | native adapter test의 `Native*` method 수가 늘었다. 뒤따르는 wmi/case 개수 단언도 같은 이유로 확인이 필요하다. |

## Global Constraints

- 제품 코드(`src/DesktopNode.*` 비테스트 프로젝트, `web/src`)를 바꾸지 않는다.
- 기대값을 현재 값으로 바꿀 때는 그 값이 기능 commit의 의도된 결과임을 commit 메시지나 diff로 확인하고 plan 실행 기록에 남긴다. 확인되지 않으면 `new-design-required`로 멈춘다.
- `config/pcv-development-policy-contract-spec-v1.json`의 `source_files`에 있는 파일(`packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json` 등)을 바꾸면 spec SHA-256과 `DevelopmentPolicyContractVerifier.ExpectedSpecSha256`을 같은 task에서 갱신한다.
- task 하나가 checkpoint 하나다(30분, tool batch 18회). 캠페인 러너(`pcv-campaign-runner-v1`)로 실행하며 commit은 `docs/ga-ready/active-campaign.json`의 `commit_policy`를 따른다.
- host mutation, 설치본, current-evidence 쓰기, push/PR은 범위 밖이다.

## Task 1: Api 계약 테스트 단언 복구

**수정:** `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`

- [x] L232 `Assert.Fail(digest);`를 현재 계약 digest pin `Assert.Equal("40fb6cd685c3f1d8fb8afd6f75677a8ec7054b7072347bc09a7ea6930b966b89", digest);`로 바꾼다. 착수 시 `e098e0a` diff에서 추가된 route와 `80` route / queued `37` 기록을 확인한다.
- [x] L733을 새 `/network` 경로가 `/qos/network`를 가로채지 않는지 확인하는 단언으로 바꾼다: `TryMatchQueuedMutation("POST", "/api/v1/vms/lab%20vm/qos/network", ...)`가 `QueueSetVmNetworkQos`로 매칭된다.
- [x] `dotnet test src/DesktopNode.Api.Tests` 실패 `0`.

실행 기록(2026-09-27): `e098e0a`는 `NativeQueuedMutation` `/api/v1/vms/{vmId}/network`(`QueueConnectVmNetwork`)와 `/api/v1/vms/{vmId}/devices`(`QueueAddVmDevice`) 두 route를 추가했고 commit 본문이 `Catalog 80 routes, queued 37`을 기록한다. digest pin 뒤의 개수 단언(ReadOnly `23`, ProductOperation `20`, QueuedMutation `37`)이 처음으로 실행되어 통과했다. Api.Tests `410/410`.

## Task 2: current evidence 기대 버전 갱신

**수정:** `src/DesktopNode.Verification.Tests/CurrentEvidenceVerifierTests.cs`

- [x] 기대 버전을 `docs/ga-ready/current-evidence.json`의 현재 값 `0.42.77-admin-smoke`로 바꾼다. 같은 테스트의 target 개수와 상태 단언이 통과하는지 확인한다.
- [x] `dotnet test src/DesktopNode.Verification.Tests` 실패 `0`.

실행 기록(2026-09-27): `current-evidence.json`의 `current.version`은 `a842ede`(2026-09-20) 이후 `0.42.77-admin-smoke`다. 기대값만 바꿨고 target `8`개 `current` 단언은 그대로 통과한다. `policy-boundaries` suite는 clean committed HEAD를 요구하므로(AGENTS.md) 변경 중에는 `CurrentEvidenceVerifierTests`만 돌리고, commit 뒤 clean HEAD에서 Verification.Tests 전체를 확인한다.

## Task 3: C# architecture gap registry 개수 갱신

**수정:** `packaging/windows-desktop-node/tests/fixtures/csharp-architecture-test-migration.json`, `packaging/windows-desktop-node/tests/PcvCSharpArchitectureGapRegistry.Tests.ps1`, `config/pcv-development-policy-contract-spec-v1.json`, `src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs`

- [ ] `Get-LiveXunitSourceInventory`로 `HyperVDomainContractTests.cs`, `DesktopNodeHyperVNativeAdapterTests.cs`, `DesktopNodeHyperVWmiProviderTests.cs`의 현재 case/method 수를 잰다.
- [ ] manifest의 개수 필드와 Pester의 고정 기대값을 현재 값으로 갱신한다. 착수 시 manifest 이력에서 기존 case 추가 때 같은 필드를 갱신한 전례를 확인하고 그 방식을 따른다.
- [ ] manifest가 spec에 pin되어 있으므로 spec SHA와 `ExpectedSpecSha256`을 갱신한다.
- [ ] `Invoke-Pester packaging/windows-desktop-node/tests/PcvCSharpArchitectureGapRegistry.Tests.ps1`, `dotnet test src/DesktopNode.Delivery.Tests` 실패 `0`.

## Task 4: 종료 검증

- [ ] `dotnet test src/DesktopNode.sln` 실패 `0`.
- [ ] `Invoke-Pester`로 `PcvModuleSizeRatchet`, `PcvCSharpArchitectureGapRegistry`, `PcvAdminSmokeEvidenceDocs` 실패 `0`.
- [ ] `npm run test:required --prefix web`.
- [ ] `git diff --check`, clean HEAD에서 AGENTS.md의 Required CI 네 shard.

## Nonclaims

- 제품 동작, route 계약, 설치본, current-evidence를 바꾸지 않는다. 테스트가 현재 계약을 기록하도록 맞춘다.
- 줄 번호와 개수는 HEAD `e5ef657` 기준이며 각 task 착수 시 다시 확인한다.
