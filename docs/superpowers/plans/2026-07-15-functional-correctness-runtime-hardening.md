# Functional Correctness Runtime Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 확인된 FC-01, FC-02, FC-04, FC-16, FC-18 기능 정확성 결함을 회귀 테스트로 고정하고, 실패 시 운영 데이터가 보존되는 방향으로 수정한다.

**Architecture:** PowerShell 제품 전환은 이동 동작을 private helper로 분리해 보상 가능한 상태 머신으로 만든다. Hyper-V는 단위 변환 정책과 가상 디스크 작업 경계를 내부 타입으로 분리해 호스트 없이 검증한다. Evidence 선택은 읽을 수 없는 후보를 가장 오래된 항목으로 정렬한다. 공개 API, CLI/Web 계약, 설치본 및 호스트 상태는 변경하지 않는다.

**Tech Stack:** PowerShell 7/Pester, .NET 10, C#, xUnit, System.Management WMI, Git.

---

## Task 1: FC-02 rollback 보상 동작

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: 실패를 재현하는 Pester 테스트 작성**

  `InModuleScope PcvDesktopNodeProduct`에서 임시 `ProductRoot`, `.previous`를 만들고, 이전 버전 승격 단계만 실패시키는 `MovePath` scriptblock을 주입한다. 테스트는 `Restore-PcvDesktopNodePreviousProductRoot`가 `PCV_PRODUCT_RESTORE_FAILED`를 반환하고 현재 제품 파일을 원래 `ProductRoot`에 되돌리며 이전 제품과 부분 승격 파일을 보존하는지 검증한다.

  ```powershell
  $result = Restore-PcvDesktopNodePreviousProductRoot `
      -ProductRoot $productRoot `
      -MovePath {
          param($Source, $Destination)
          if ($Source -eq $previousRoot -and $Destination -eq $productRoot) {
              New-Item -ItemType Directory -Path $productRoot -Force | Out-Null
              Set-Content -LiteralPath (Join-Path $productRoot 'partial.txt') -Value 'partial'
              throw 'simulated promotion failure'
          }
          Move-Item -LiteralPath $Source -Destination $Destination -Force
      }

  $result.Code | Should -Be 'PCV_PRODUCT_RESTORE_FAILED'
  (Get-Content (Join-Path $productRoot 'current.txt') -Raw).Trim() | Should -Be 'current'
  (Get-Content (Join-Path $previousRoot 'previous.txt') -Raw).Trim() | Should -Be 'previous'
  @(Get-ChildItem -LiteralPath $testRoot -Directory -Filter '*.restore-partial.*').Count | Should -Be 1
  ```

- [ ] **Step 2: 대상 테스트를 실행해 RED 확인**

  Run:

  ```powershell
  pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -FullNameFilter '*restores the active product when previous promotion fails*' -Output Detailed"
  ```

  Expected: `Restore-PcvDesktopNodePreviousProductRoot`가 아직 없어 테스트 실패.

- [ ] **Step 3: private restore helper 구현**

  `PcvDesktopNodeProduct.psm1`에 `Restore-PcvDesktopNodePreviousProductRoot`를 추가한다. 기본 동작은 active→`.failed`, previous→active이고, 두 번째 이동 실패 시 생성된 active를 `<ProductRoot>.restore-partial.<guid>`로 옮긴 뒤 `.failed`를 active로 되돌린다. 첫 실패의 상세는 반환 객체 `Detail`에 보존한다. 보상까지 실패하면 코드 `PCV_PRODUCT_RESTORE_COMPENSATION_FAILED`, 보상 성공이면 `PCV_PRODUCT_RESTORE_FAILED`를 반환한다.

  ```powershell
  [pscustomobject]@{
      Ok = $false
      Code = 'PCV_PRODUCT_RESTORE_FAILED'
      Detail = [string]$promotionError.Exception.Message
  }
  ```

  `Invoke-PcvDesktopNodeProductAction`의 기본 `$RestoreProductRoot` scriptblock은 helper만 호출하도록 변경하고, 기존 사용자 주입 scriptblock 계약은 유지한다.

- [ ] **Step 4: 대상 Pester 테스트를 실행해 GREEN 확인**

  Run: Step 2와 동일.

  Expected: 새 FC-02 테스트 PASS.

- [ ] **Step 5: 제품 action 테스트 파일 전체 실행**

  Run:

  ```powershell
  pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
  ```

  Expected: 전체 PASS.

- [ ] **Step 6: FC-02 변경 커밋**

  ```powershell
  git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
  git commit -m "fix: compensate failed product rollback"
  ```

## Task 2: FC-16 backup 보존과 복구

**Files:**
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: 기존 backup 손실을 재현하는 Pester 테스트 작성**

  기존 `.previous`와 현재 `ProductRoot`를 만든다. 현재→previous 이동 단계에서 부분 디렉터리를 생성한 뒤 실패하도록 `MovePath`를 주입한다. `Backup-PcvDesktopNodeProductRoot`가 오류를 반환해도 기존 previous가 원래 내용으로 복원되고 부분 파일이 `<PreviousProductRoot>.partial.<guid>`에 보존되는지 검증한다.

  ```powershell
  $result = Backup-PcvDesktopNodeProductRoot -ProductRoot $productRoot -MovePath $movePath

  $result.Code | Should -Be 'PCV_PRODUCT_UPDATE_BACKUP_FAILED'
  (Get-Content (Join-Path $previousRoot 'old-previous.txt') -Raw).Trim() | Should -Be 'old-previous'
  @(Get-ChildItem -LiteralPath $testRoot -Directory -Filter '*.previous.partial.*').Count | Should -Be 1
  ```

  `.previous.staging`과 `.previous`가 모두 존재하면 무단 삭제 없이 `PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED`를 반환하는 별도 테스트도 추가한다.

- [ ] **Step 2: FC-16 테스트를 실행해 RED 확인**

  Run:

  ```powershell
  pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -FullNameFilter '*preserves the previous backup when active backup promotion fails*' -Output Detailed"
  ```

  Expected: `Backup-PcvDesktopNodeProductRoot`가 아직 없어 실패.

- [ ] **Step 3: staging 기반 backup helper 구현**

  `Backup-PcvDesktopNodeProductRoot`를 추가한다.

  1. `<PreviousProductRoot>.staging`만 있으면 previous로 복구한다.
  2. staging과 previous가 모두 있으면 `PCV_PRODUCT_UPDATE_BACKUP_RECOVERY_REQUIRED`를 반환한다.
  3. 기존 previous를 staging으로 이동한다.
  4. active를 previous로 이동한다.
  5. 성공 시 staging을 삭제한다.
  6. 4단계 실패 시 부분 previous를 `<PreviousProductRoot>.partial.<guid>`로 보존하고 staging을 previous로 복구한다.

  복구 실패 코드는 `PCV_PRODUCT_UPDATE_BACKUP_COMPENSATION_FAILED`, 복구 성공 후 원작업 실패 코드는 `PCV_PRODUCT_UPDATE_BACKUP_FAILED`로 고정한다. 기본 `$BackupProductRoot` scriptblock은 helper에 위임한다.

- [ ] **Step 4: FC-16 대상 및 전체 제품 action 테스트 실행**

  Run:

  ```powershell
  pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
  ```

  Expected: FC-02/FC-16 회귀 테스트와 기존 테스트 전체 PASS.

- [ ] **Step 5: FC-16 변경 커밋**

  ```powershell
  git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
  git commit -m "fix: preserve product backup during failed update"
  ```

## Task 3: Hyper-V 정책 테스트 경계와 RED 테스트

**Files:**
- Create: `src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj`
- Create: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVResourceMutationPolicyTests.cs`
- Create: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVWmiVmResourceMutationProviderTests.cs`
- Create: `src/DesktopNode.HyperV/Properties/AssemblyInfo.cs`
- Modify: `src/DesktopNode.sln`

- [ ] **Step 1: xUnit 프로젝트와 friend assembly 추가**

  테스트 프로젝트는 `net10.0-windows`, `Microsoft.NET.Test.Sdk 17.14.1`, `xunit 2.9.3`, `xunit.runner.visualstudio 3.1.4`, `coverlet.collector 6.0.4`를 사용하고 `DesktopNode.HyperV`를 참조한다. `AssemblyInfo.cs`에는 다음만 둔다.

  ```csharp
  using System.Runtime.CompilerServices;

  [assembly: InternalsVisibleTo("DesktopNode.HyperV.Tests")]
  ```

  Run:

  ```powershell
  dotnet sln src/DesktopNode.sln add src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj
  ```

- [ ] **Step 2: FC-04 단위 변환 RED 테스트 작성**

  ```csharp
  [Theory]
  [InlineData(0, 0UL)]
  [InlineData(1, 1_000UL)]
  [InlineData(2_048, 2_048_000UL)]
  public void KbpsToBitsPerSecondUsesDecimalKilobits(int kbps, ulong expected)
  {
      Assert.Equal(expected, DesktopNodeHyperVResourceMutationPolicy.KbpsToBitsPerSecond(kbps));
  }

  [Theory]
  [InlineData(0UL, 0UL)]
  [InlineData(2_048_000UL, 2_048UL)]
  public void BitsPerSecondToKbpsReturnsEvidenceUnits(ulong bps, ulong expected)
  {
      Assert.Equal(expected, DesktopNodeHyperVResourceMutationPolicy.BitsPerSecondToKbps(bps));
  }
  ```

- [ ] **Step 3: FC-01 shrink 차단 RED 테스트 작성**

  fake `IDesktopNodeVirtualDiskOperations`는 현재 `MaxInternalSize`와 `Resize` 호출 횟수를 기록한다. 요청이 현재 크기보다 작으면 `PCV_VM_DISK_SHRINK_NOT_SUPPORTED` 예외가 발생하고 `Resize`가 호출되지 않는지 검증한다. 같은 크기 또는 큰 크기에는 `Resize`가 정확히 한 번 호출되는 테스트도 추가한다.

- [ ] **Step 4: Hyper-V 대상 테스트를 실행해 RED 확인**

  Run:

  ```powershell
  dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj -c Release
  ```

  Expected: 아직 없는 정책/작업 경계 타입 때문에 컴파일 실패.

- [ ] **Step 5: 테스트 경계 커밋**

  RED 상태의 테스트만 따로 커밋하지 않는다. Task 4 GREEN 구현과 함께 커밋한다.

## Task 4: FC-04 QoS 변환과 FC-01 shrink guard 구현

**Files:**
- Create: `src/DesktopNode.HyperV/DesktopNodeHyperVResourceMutationPolicy.cs`
- Create: `src/DesktopNode.HyperV/DesktopNodeVirtualDiskOperations.cs`
- Modify: `src/DesktopNode.HyperV/DesktopNodeHyperVWmiVmResourceMutationProvider.cs`
- Modify: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVResourceMutationPolicyTests.cs`
- Modify: `src/DesktopNode.HyperV.Tests/DesktopNodeHyperVWmiVmResourceMutationProviderTests.cs`

- [ ] **Step 1: 명시적 단위 변환 정책 구현**

  ```csharp
  internal static class DesktopNodeHyperVResourceMutationPolicy
  {
      internal static ulong KbpsToBitsPerSecond(int value) => checked((ulong)value * 1_000UL);

      internal static ulong BitsPerSecondToKbps(ulong value) => value / 1_000UL;
  }
  ```

  `SetNetworkQos`와 `CreateBandwidthFeature`가 WMI `Limit`/`Reservation`에 bps 값을 기록하게 한다. 기존 WMI 값은 `BitsPerSecondToKbps`로 바꾼 뒤 기존 evidence 필드에 넣어 공개 evidence 단위가 Kbps로 유지되게 한다.

- [ ] **Step 2: 가상 디스크 작업 abstraction과 WMI 구현 작성**

  ```csharp
  internal interface IDesktopNodeVirtualDiskOperations
  {
      ulong GetMaxInternalSize(string path, CancellationToken cancellationToken);
      void Resize(string path, ulong requestedBytes, CancellationToken cancellationToken);
  }
  ```

  기본 구현은 `Msvm_ImageManagementService.GetVirtualHardDiskSettingData`를 호출하고 반환된 embedded `Msvm_VirtualHardDiskSettingData`의 `MaxInternalSize`를 읽는다. `Resize`는 현재 `ResizeVirtualHardDisk` WMI 호출을 이동한다.

- [ ] **Step 3: provider가 작업 경계를 주입받고 shrink를 차단하도록 변경**

  public 기본 생성자는 WMI 구현을 사용하고 internal 생성자는 테스트 fake를 받는다. `ResizeDisk`는 요청 bytes를 `checked((ulong)diskGb * 1024UL * 1024UL * 1024UL)`로 계산한 뒤 현재 `MaxInternalSize`보다 작으면 아래 예외를 던진다.

  ```csharp
  throw new DesktopNodeHyperVNativeOperationException(
      "PCV_VM_DISK_SHRINK_NOT_SUPPORTED",
      $"VM '{request.Name}' disk resize cannot shrink the virtual disk.",
      "Provide disk_gb greater than or equal to the current virtual disk size.",
      false);
  ```

- [ ] **Step 4: Hyper-V 테스트 GREEN 확인**

  Run:

  ```powershell
  dotnet test src/DesktopNode.HyperV.Tests/DesktopNode.HyperV.Tests.csproj -c Release
  ```

  Expected: FC-01/FC-04 테스트 전체 PASS.

- [ ] **Step 5: 관련 기존 Core/API 테스트 실행**

  Run:

  ```powershell
  dotnet test src/DesktopNode.Runtime.Tests/DesktopNode.Runtime.Tests.csproj -c Release
  dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release
  ```

  Expected: 전체 PASS.

- [ ] **Step 6: Hyper-V 변경 커밋**

  ```powershell
  git add src/DesktopNode.sln src/DesktopNode.HyperV src/DesktopNode.HyperV.Tests
  git commit -m "fix: harden Hyper-V resource mutation policies"
  ```

## Task 5: FC-18 읽을 수 없는 evidence 후보 후순위화

**Files:**
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`
- Modify: `src/DesktopNode.Api/BatchEvidenceSummaryReader.cs`

- [ ] **Step 1: private sort-time 회귀 테스트 작성**

  기존 reflection 테스트 스타일로 `GetEvidenceSummarySortTime`을 호출한다. 정상 `summary.json`에는 명시적 최근 수정 시간을 주고, reparse 또는 읽기 거부 후보에는 `DateTime.MinValue`가 반환되는지 검증한다.

  ```csharp
  var method = typeof(BatchEvidenceSummaryReader).GetMethod(
      "GetEvidenceSummarySortTime",
      BindingFlags.NonPublic | BindingFlags.Instance);

  var sortTime = (DateTime)method!.Invoke(reader, [unreadableRunRoot])!;

  Assert.Equal(DateTime.MinValue, sortTime);
  ```

- [ ] **Step 2: FC-18 대상 테스트를 실행해 RED 확인**

  Run:

  ```powershell
  dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release --filter "FullyQualifiedName~BatchEvidenceSortTimePlacesUnreadableSummaryLast"
  ```

  Expected: 현재 구현이 `DateTime.MaxValue`를 반환해 assertion 실패.

- [ ] **Step 3: 정렬 기본값을 MinValue로 변경**

  ```csharp
  return IsReadableEvidencePath(summaryPath)
      ? File.GetLastWriteTimeUtc(summaryPath)
      : DateTime.MinValue;
  ```

- [ ] **Step 4: API 테스트 전체 GREEN 확인**

  Run:

  ```powershell
  dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj -c Release
  ```

  Expected: 신규 FC-18 테스트와 기존 reparse guard 테스트 포함 전체 PASS.

- [ ] **Step 5: FC-18 변경 커밋**

  ```powershell
  git add src/DesktopNode.Api/BatchEvidenceSummaryReader.cs src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs
  git commit -m "fix: sort unreadable evidence summaries last"
  ```

## Task 6: 검증 보고서와 전체 회귀 검증

**Files:**
- Create: `docs/ga-ready/evidence/functional-correctness-runtime-hardening-2026-07-15.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`

- [ ] **Step 1: code-level evidence 문서 작성**

  문서에 각 finding과 수정 매핑을 기록한다.

  - FC-01: 현재 VHD 최대 내부 크기를 읽고 shrink 호출 전 차단.
  - FC-02: rollback 승격 실패 시 active 복원 및 partial 보존.
  - FC-04: API Kbps를 WMI bps로 변환하고 evidence는 Kbps 유지.
  - FC-16: previous staging/partial 보존과 복구 필요 상태 구분.
  - FC-18: 읽을 수 없는 후보를 최신으로 선택하지 않음.

  실행한 테스트 명령, PASS 개수, commit SHA를 실제 결과로 적는다. 이 검증이 code-level이고 호스트 mutation, 설치본 변경, package build, public trusted signing, 외부 stable publication을 수행하거나 주장하지 않았음을 명시한다.

- [ ] **Step 2: 전체 solution 검증 실행**

  Run:

  ```powershell
  dotnet test src/DesktopNode.sln -c Release
  pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1' -Output Detailed"
  ```

  Expected: 모든 .NET/Pester 테스트 PASS.

- [ ] **Step 3: 변경 범위와 금지 항목 1회 검토**

  Run:

  ```powershell
  git diff --check
  git status --short
  git diff --stat HEAD~3..HEAD
  rg -n "PCV_PRODUCT_RESTORE_COMPENSATION_FAILED|PCV_PRODUCT_UPDATE_BACKUP_COMPENSATION_FAILED|PCV_VM_DISK_SHRINK_NOT_SUPPORTED|KbpsToBitsPerSecond|DateTime.MinValue" packaging src docs/ga-ready
  ```

  Expected: whitespace 오류 없음, TUI 복원 없음, 임시 비밀/바이너리/빌드 산출물 없음, 다섯 finding의 코드·테스트·evidence 연결 확인.

- [ ] **Step 4: evidence 문서 커밋**

  ```powershell
  git add docs/ga-ready/evidence/functional-correctness-runtime-hardening-2026-07-15.md docs/ga-ready/EVIDENCE_INDEX.md
  git commit -m "docs: record runtime hardening verification"
  ```

- [ ] **Step 5: 최종 branch 상태 확인**

  ```powershell
  git status --short
  git log --oneline --decorate -6
  ```

  Expected: clean worktree. 원격 push, main merge, package build, 설치본 변경 및 호스트 mutation은 수행하지 않음.
