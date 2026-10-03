# PureCVisor Desktop Node Module Ratchet Recovery Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 2026-08-29 clone 보정 뒤 누락된 module-size ratchet pin chain을 실측값과 일치시키고 네 Required CI shard를 모두 다시 검증한다.

**Architecture:** API 동작과 `DesktopNodeApiVmMutationRouteHandler.cs`는 변경하지 않는다. 2026-08-27의 정상 선례 commit `6775228c5e7cd3b11b0100024809cd8282f2ecc0`과 동일하게, 실측 line ceiling fixture와 그 fixture를 고정하는 source SHA, 그리고 spec SHA를 한 원자적 chain으로 갱신한다. 이 checkpoint는 Lane 1 source-only이며 package, 설치, host/VM mutation, current evidence, commit, push, PR을 수행하지 않는다.

**Tech Stack:** .NET 10, xUnit, JSON policy fixtures, `DesktopNode.Verification`

---

## 확인된 RED와 범위

- `04b3c9f~1`의 대상 파일은 970줄이다.
- `04b3c9f`와 현재 HEAD의 대상 파일은 989줄이다.
- fixture ceiling은 970이며 focused `PcvModuleSizeRatchetContractTests`가 정확히 3/3 FAIL한다.
- 실패 코드는 `PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|module-ratchet-exceeded`다.
- 쓰기 허용 파일은 이 계획과 아래 Task 1의 세 파일뿐이다.
- 범위 밖 발견은 report-only다.

### Task 1: 실측 module ratchet pin chain 복구

**Files:**
- Modify: `packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json:27-30`
- Modify: `config/pcv-development-policy-contract-spec-v1.json:68-69`
- Modify: `src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs:13-14`
- Test: `src/DesktopNode.Delivery.Tests/Delivery/Verification/PcvModuleSizeRatchetContractTests.cs`

- [x] **Step 1: RED가 정확한 기존 실패인지 확인한다**

Run:

```powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --no-restore --filter 'FullyQualifiedName~PcvModuleSizeRatchetContractTests' --logger 'console;verbosity=minimal'
```

Expected: `3` FAIL, 공통 코드 `PCV_DELIVERY_DEVELOPMENT_POLICY_INVALID|module-ratchet-exceeded`.

- [x] **Step 2: fixture를 현재 실측값으로 맞춘다**

`DesktopNodeApiVmMutationRouteHandler.cs` 항목을 다음 값으로 바꾼다.

```json
{
  "path": "src/DesktopNode.Api/DesktopNodeApiVmMutationRouteHandler.cs",
  "max_lines": 989,
  "owner": "queued VM mutation routes and QoS validation",
  "note": "2026-08-06 분해가 만들어 낸 모듈이다. 2026-08-27 SERVICE_PLAN P1-5 clone preview/queue routes가 970줄로 늘렸고, 2026-08-29 P1 clone disk/Off/vm-root 보정이 989줄로 늘렸다."
}
```

- [x] **Step 3: fixture SHA pin을 갱신한다**

Run:

```powershell
$ratchetFixturePath = 'packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json'
$ratchetFixtureSha = (Get-FileHash -Algorithm SHA256 -LiteralPath $ratchetFixturePath).Hash.ToLowerInvariant()
$ratchetFixtureSha
```

`config/pcv-development-policy-contract-spec-v1.json`의 동일 경로 source row `sha256`을 출력된 64자리 lowercase 값으로 바꾼다. 다른 source row나 계약 값은 바꾸지 않는다.

- [x] **Step 4: spec SHA pin을 갱신한다**

Run:

```powershell
$developmentSpecPath = 'config/pcv-development-policy-contract-spec-v1.json'
$developmentSpecSha = (Get-FileHash -Algorithm SHA256 -LiteralPath $developmentSpecPath).Hash.ToLowerInvariant()
$developmentSpecSha
```

`DevelopmentPolicyContractVerifier.ExpectedSpecSha256`만 출력된 64자리 lowercase 값으로 바꾼다.

- [x] **Step 5: focused GREEN을 확인한다**

Run:

```powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --no-restore --filter 'FullyQualifiedName~PcvModuleSizeRatchetContractTests' --logger 'console;verbosity=minimal'
```

Expected: `3/3` PASS, warning/error 없음.

### Task 2: clean source build와 네 Required CI shard 재실행

**Files:**
- Read only: source tree
- Local ignored artifacts: `artifacts/module-ratchet-recovery-*`

- [x] **Step 1: Release build를 갱신한다**

Run:

```powershell
dotnet build src/DesktopNode.sln -c Release --no-restore
```

Expected: exit 0, warning 0, error 0.

- [ ] **Step 2: 네 shard를 실행한다**

Run:

```powershell
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path .github/workflows/development-gates.yml --artifact-root artifacts/module-ratchet-recovery-dotnet --shard dotnet
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path web/package.json --artifact-root artifacts/module-ratchet-recovery-web --shard web
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 --artifact-root artifacts/module-ratchet-recovery-delivery --shard delivery
dotnet run --project src/DesktopNode.Verification -c Release --no-build --no-restore -- verify --lane Full --change-tier M --changed-path packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 --artifact-root artifacts/module-ratchet-recovery-installer-policy --shard installer-policy
```

Expected: 네 `summary.json` 모두 `ok=true`, `failed_suite=null`, process exit 0. `installer-policy`의 resolver 승격 `L/Release`는 policy-classification shard의 정상 결과이며 candidate aggregate Lane 1 범위를 변경하지 않는다.

### Task 3: 범위·상태 인계

**Files:**
- Read only: Git diff/status와 `artifacts/module-ratchet-recovery-*/summary.json`

- [ ] **Step 1: 허용 범위와 whitespace를 확인한다**

Run:

```powershell
git diff --check
git status --short
git diff -- packaging/windows-desktop-node/tests/fixtures/module-size-ratchet.json config/pcv-development-policy-contract-spec-v1.json src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs docs/superpowers/plans/2026-08-29-purecvisor-desktop-node-module-ratchet-recovery.md
```

Expected: 허용된 네 파일만 변경되고 `git diff --check` 출력 없음.

- [ ] **Step 2: 종료 상태를 보고한다**

보고에는 `lane=Lane 1`, working authority, RED/GREEN, 네 shard 결과, `current_evidence_written=false`, host/VM mutation false, commit/push/PR 미수행, 다음 승인 필요 여부를 포함한다.
