# P1-5 clone Lane 2 프로브 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 설치본 `pcvcli vm clone`이 managed Gen2 Off 소스의 독립 VHDX를 복사해 새 managed VM을 만드는 clone family 한 프로브를, DryRun 계약부터 probe-vehicle `0.42.76-admin-smoke` 설치본 실행까지 연다.

**Architecture:** P0 actual-VM runner를 clone family로 축소한다. preview는 `--dry-run`, clone은 `--yes` queued job이다. 제품 get/delete/clone 인자는 표시 이름만 쓴다. 04275 설치본에는 clone이 없으므로 라이브 프로브 전에 `0.42.76-admin-smoke` MSI를 current 승격 없이 빌드·설치한다.

**Tech Stack:** PowerShell 7 runner, xUnit Delivery contract tests, Pester manual-admin tests, WiX `installer/build.ps1` `AllowUnsignedDev`.

**Spec:** `docs/superpowers/specs/2026-08-28-purecvisor-desktop-node-p1-clone-lane2-probe-design.md`

## Global Constraints

- 한국어 본문. 식별자, route, `PCV_*`, 파일 경로는 원문.
- 즉시 실행 범위는 **Task 1–3 (checkpoint A)** 뿐이다. Task 4–6는 스펙 §4의 별도 승인.
- `docs/ga-ready/current-evidence.json`과 AGENTS.md generated current 블록을 쓰지 않는다.
- fullgate, `0.42.75 -> 0.42.76` pair, feature ledger pass 승격, Full P0 연쇄를 이 계획이 실행하지 않는다.
- 제품 CLI 인자에 Hyper-V GUID를 넣지 않는다. `vm get` / `vm delete` / `vm clone`은 표시 이름.
- 사용자 VM을 소스로 쓰지 않는다. 접두사 `pcv-p1-clone-`만 만든다.
- public trusted signing / external stable publication `not-claimed`.
- 커밋 접두사 `test:` / `feat:` / `docs:`.
- Required CI: Task 1–3은 focused `dotnet test` Delivery. `installer-policy`는 clean HEAD 후.
- manual-admin Pester는 Required CI가 아니다.

---

## File map

| File | Responsibility |
| --- | --- |
| `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP1CloneActualVmSmokeContractTests.cs` | runner 토큰, DryRun 경계, 표시 이름, current-evidence 부재 |
| `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1` | clone family runner |
| `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP1CloneActualVmSmoke.Tests.ps1` | DryRun + adapter 행위. Required CI 아님 |
| `docs/ga-ready/evidence/admin-smoke-package-YYYY-MM-DD-04276.md` | Task 4. probe-vehicle package. current 금지 |
| `docs/ga-ready/evidence/service-plan-p1-clone-actual-vm-YYYY-MM-DD-04276.md` | Task 6. Lane 2 evidence. current 금지 |

---

### Task 1: Clone runner C# contract tests (RED)

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP1CloneActualVmSmokeContractTests.cs`
- Create: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1` (empty stub only if test cannot load missing file — prefer test first against missing path FAIL, then Task 2 creates the runner)

**Interfaces:**
- Consumes: `RepositoryContractContext.Find().ReadUtf8Text`, P0 contract test helper 패턴.
- Produces: Facts that pin runner source tokens Task 2가 만족해야 한다.

- [ ] **Step 1: Write the contract tests**

```csharp
using System.Text.RegularExpressions;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.ManualAdmin;

[Trait("Category", "Delivery")]
public sealed class PcvServicePlanP1CloneActualVmSmokeContractTests
{
    private const string RunnerPath =
        "packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1";

    [Fact]
    public void PublishesValidatedInputsAndStrictDryRunBoundary()
    {
        var source = Source();
        RequireTokens(
            source,
            "[Parameter(Mandatory)]",
            "[string]$Version",
            "$ArtifactRoot",
            "$ProductRoot",
            "$IsoPath",
            "$VmRoot",
            "$SourceVm",
            "$TargetVm",
            "$JobTimeoutSeconds",
            "$CommandTimeoutSeconds",
            "$DryRun",
            "$RuntimeAdapter",
            "$SummaryWriter",
            "Invoke-RuntimeOperation",
            "PCV_P1_CLONE_INSTALLED_VERSION_MISMATCH",
            "PCV_P1_CLONE_VM_NAME_INVALID",
            "dry-run-no-installed-cli-or-hyperv",
            "artifact_root_resolved",
            "vm_root_resolved");
        Assert.DoesNotContain("[ValidateSet('SavedOnly', 'Full')]", source, StringComparison.Ordinal);
        AssertOrdered(source, "if ($DryRun.IsPresent)", "Assert-InstalledProduct");
    }

    [Fact]
    public void PinsCloneFamilySliceOrderAndOperatorIds()
    {
        var source = Source();
        RequireTokens(
            source,
            "'source_create'",
            "'preview_mismatch'",
            "'preview_ok'",
            "'clone_ok'",
            "'cleanup'",
            "'vm', 'create'",
            "'vm', 'clone'",
            "'--dry-run'",
            "'--yes'",
            "PCV_CLI_CONFIRMATION_REQUIRED",
            "'vm', 'get', $OperatorId",
            "'vm', 'delete', $record.name",
            "'vm', 'clone', $SourceVm",
            "Assert-SlicePassed",
            "Invoke-TrackedSlice");
        AssertOrdered(
            source,
            "'source_create'",
            "'preview_mismatch'",
            "'preview_ok'",
            "'clone_ok'",
            "'cleanup'");
        Assert.DoesNotContain("'vm', 'get', $Id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm', 'delete', $record.id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm', 'clone', $record.id", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm-start'", source, StringComparison.Ordinal);
        Assert.DoesNotContain("'vm-save'", source, StringComparison.Ordinal);
    }

    [Fact]
    public void PinsSummaryAtomicityAndDoesNotWriteCurrentEvidence()
    {
        var source = Source();
        RequireTokens(
            source,
            "installed_cli_sha256",
            "queued_jobs",
            "cleanup",
            "host_mutation_performed",
            "secret_observed",
            "overall_verdict",
            "PCV_P1_CLONE_SUMMARY_WRITE_FAILED",
            "summary.json.tmp",
            "Move-Item -LiteralPath",
            "Get-CliProblemCode");
        Assert.DoesNotContain("docs/ga-ready/current-evidence.json", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Update-PcvCurrentEvidence", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove-VM -Name", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(
            new Regex(@"(?i)bearer\s+[A-Za-z0-9._~+/\-]+=*", RegexOptions.CultureInvariant),
            source);
    }

    private static string Source() =>
        RepositoryContractContext.Find().ReadUtf8Text(RunnerPath);

    private static void RequireTokens(string source, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            Assert.Contains(token, source, StringComparison.Ordinal);
        }
    }

    private static void AssertOrdered(string source, params string[] tokens)
    {
        var offset = 0;
        foreach (var token in tokens)
        {
            var index = source.IndexOf(token, offset, StringComparison.Ordinal);
            Assert.True(index >= 0, $"Missing or out-of-order source token: {token}");
            offset = index + token.Length;
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```powershell
dotnet test src/DesktopNode.Delivery.Tests --filter FullyQualifiedName~PcvServicePlanP1CloneActualVmSmokeContractTests --nologo
```

Expected: FAIL (`source-not-found` 또는 file missing / tokens missing).

- [ ] **Step 3: Commit**

```powershell
git add src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP1CloneActualVmSmokeContractTests.cs
git commit -m "test: add P1 clone actual-VM runner contract"
```

---

### Task 2: Clone family runner (GREEN)

**Files:**
- Create: `packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1`

**Interfaces:**
- Consumes: Task 1 token 목록. P0 runner의 다음 함수를 **같은 이름·같은 비밀 가드**로 옮긴다: `Get-AbsolutePath`, `Get-ShortHash`, `Assert-DedicatedVmRoot`, `Assert-ValidatedChildPath`, `Write-AtomicSummary`, `Test-SecretMaterial`, `Get-SafeFailureCode`, `Invoke-RuntimeOperation`, `Invoke-PcvCliJson`, `Get-CliProblemCode`, `Start-PcvCliJob`, `Invoke-TrackedSlice`, `Assert-SlicePassed`, `Get-ProductVmState`. throw 접두사는 `PCV_P1_CLONE_*`.
- Produces: `-DryRun` 시 설치본/Hyper-V 호출 0. 라이브 시 slice 5개 fail-stop.

- [ ] **Step 1: Param and DryRun boundary**

```powershell
#requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Version,

    [string]$ArtifactRoot = '',
    [string]$ProductRoot = 'C:\Program Files\PureCVisor\DesktopNode',
    [string]$IsoPath = '',
    [string]$VmRoot = '',
    [string]$SourceVm = '',
    [string]$TargetVm = '',

    [ValidateRange(1, 3600)]
    [int]$JobTimeoutSeconds = 180,
    [ValidateRange(1, 1800)]
    [int]$CommandTimeoutSeconds = 120,

    [switch]$DryRun,
    [Parameter(DontShow)][scriptblock]$RuntimeAdapter,
    [Parameter(DontShow)][scriptblock]$SummaryWriter
)
```

이름:

```powershell
$versionTag = (($Version.Split('-')[0]) -replace '[^0-9A-Za-z]', '').ToLowerInvariant()
# 0.42.76-admin-smoke -> 04276
$campaignKey = Get-ShortHash -Value "$Version|$artifactRootFull"
if ([string]::IsNullOrWhiteSpace($SourceVm)) {
    $SourceVm = "pcv-p1-clone-$versionTag-$campaignKey-src"
}
if ([string]::IsNullOrWhiteSpace($TargetVm)) {
    $TargetVm = "pcv-p1-clone-$versionTag-$campaignKey-dst"
}
```

`Assert-VmName`: `^pcv-p1-clone-[A-Za-z0-9][A-Za-z0-9._-]{5,60}$` 이고 `pcv-p1-clone-$versionTag-*`.

`$plannedSlices = @('source_create', 'preview_mismatch', 'preview_ok', 'clone_ok', 'cleanup')`

DryRun은 `Assert-InstalledProduct` **앞**에서:

```powershell
if ($DryRun.IsPresent) {
    $summary.ok = $true
    $summary.overall_verdict = 'NOT_RUN'
    $summary.actual_execution = 'dry-run-no-installed-cli-or-hyperv'
    $summary.completed_at = (Get-Date).ToUniversalTime().ToString('o')
    Write-AtomicSummary
    return [pscustomobject]$summary
}
```

- [ ] **Step 2: Slice actions (operator id = 표시 이름)**

`source_create` — 시작하지 않는다. Off 유지.

```powershell
Start-PcvCliJob -StepName 'vm-create' -Arguments @(
    'vm', 'create', '--name', $SourceVm, '--iso', $summary.iso_path_resolved,
    '--cpu', '1', '--memory-mb', '1024', '--disk-gb', '8', '--vm-root', $vmRootFull)
Get-ProductVmState -OperatorId $SourceVm -Phase 'after-create'
# 기대: off 또는 stopped. Hyper-V EnabledState Off. Test-PcvProductOff.
```

`preview_mismatch` — `--yes`/`--dry-run` 없음. CLI는 confirm_name=source라 API mismatch를 만들 수 없다. 운영자 표면 거절은 confirmation이다.

```powershell
Invoke-PcvCliJson -StepName 'vm-clone-unconfirmed' -AllowFailure -Arguments @(
    'vm', 'clone', $SourceVm, '--name', $TargetVm)
# preview/clone --yes|--dry-run 에는 '--vm-root', $vmRootFull 을 붙인다.
# exit != 0, Get-CliProblemCode == PCV_CLI_CONFIRMATION_REQUIRED
# 대상 디렉터리/VM 없음
```

`preview_ok`:

```powershell
Invoke-PcvCliJson -StepName 'vm-clone-preview' -Arguments @(
    'vm', 'clone', $SourceVm, '--name', $TargetVm, '--dry-run')
# planned_copy_bytes > 0, 대상 경로 파일 없음
```

`clone_ok`:

```powershell
Start-PcvCliJob -StepName 'vm-clone' -Arguments @(
    'vm', 'clone', $SourceVm, '--name', $TargetVm, '--yes')
Get-ProductVmState -OperatorId $TargetVm -Phase 'after-clone'
Get-ProductVmState -OperatorId $SourceVm -Phase 'after-clone-source'
# 대상 managed, disk0.vhdx under VmRoot/TargetVm, 소스 Off 불변
```

`cleanup` — 대상 다음 소스. 제품 delete만. `Remove-VM -Name` 금지.

```powershell
Invoke-PcvCliJson -StepName 'vm-delete-target' -Arguments @('vm', 'delete', $TargetVm, '--yes')
Invoke-PcvCliJson -StepName 'vm-delete-source' -Arguments @('vm', 'delete', $SourceVm, '--yes')
```

`Get-ProductVmState` 고정:

```powershell
$result = Invoke-PcvCliJson -StepName 'vm-get-state' -Arguments @('vm', 'get', $OperatorId)
```

버전 불일치: `Assert-InstalledProduct`가 manifest version `-cne $Version`이면 `PCV_P1_CLONE_INSTALLED_VERSION_MISMATCH`. mutation 0.

- [ ] **Step 3: Run contract tests**

```powershell
dotnet test src/DesktopNode.Delivery.Tests --filter FullyQualifiedName~PcvServicePlanP1CloneActualVmSmokeContractTests --nologo
```

Expected: PASS.

- [ ] **Step 4: Commit**

```powershell
git add packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1 src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/PcvServicePlanP1CloneActualVmSmokeContractTests.cs
git commit -m "feat: add P1 clone actual-VM runner"
```

---

### Task 3: Pester DryRun and adapter behavior

**Files:**
- Create: `packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP1CloneActualVmSmoke.Tests.ps1`

**Interfaces:**
- Consumes: Task 2 runner `-DryRun`, `-RuntimeAdapter`.
- Produces: DryRun `overall_verdict=NOT_RUN`. version mismatch는 `create-directory`/`invoke-cli` 없음.

이 파일은 Required CI가 아니다. 로컬에서만 돌린다.

- [ ] **Step 1: Write tests**

```powershell
Describe 'SERVICE_PLAN P1 clone actual-VM runner contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
        $script:RunnerPath = Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1'
    }

    It 'emits a deterministic non-mutating dry-run summary without installed product access' {
        $artifactRoot = Join-Path $TestDrive 'p1-clone-plan'
        $vmRoot = Join-Path $TestDrive 'dedicated-vm-root/clone'
        $result = & $script:RunnerPath `
            -Version '0.42.76-admin-smoke' `
            -ArtifactRoot $artifactRoot `
            -ProductRoot (Join-Path $TestDrive 'product-not-installed') `
            -IsoPath (Join-Path $TestDrive 'media-not-present.iso') `
            -VmRoot $vmRoot `
            -DryRun

        $summary = Get-Content -LiteralPath (Join-Path $artifactRoot 'summary.json') -Raw |
            ConvertFrom-Json -Depth 100
        $summary.ok | Should -BeTrue
        $summary.overall_verdict | Should -Be 'NOT_RUN'
        $summary.host_mutation_performed | Should -BeFalse
        $summary.secret_observed | Should -BeFalse
        $summary.actual_execution | Should -Be 'dry-run-no-installed-cli-or-hyperv'
        @($summary.plan | ForEach-Object slice) | Should -Be @(
            'source_create', 'preview_mismatch', 'preview_ok', 'clone_ok', 'cleanup')
        @($result).Count | Should -Be 1
    }
}
```

Adapter 시나리오는 P0 `New-P0BehaviorRuntime`을 clone용으로 축소한다. `InstalledVersion='0.42.76-admin-smoke'`. mismatch 시 `invoke-cli` step `vm-clone-unconfirmed`가 exit 1 / stderr `PCV_CLI_CONFIRMATION_REQUIRED`. version mismatch는 `create-directory` 없음.

- [ ] **Step 2: Run Pester locally**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP1CloneActualVmSmoke.Tests.ps1 -Output Detailed
```

Expected: PASS. Hyper-V/설치본 호출 없음.

- [ ] **Step 3: Focused Delivery still PASS**

```powershell
dotnet test src/DesktopNode.Delivery.Tests --filter FullyQualifiedName~PcvServicePlanP1CloneActualVmSmokeContractTests --nologo
git diff --check
```

- [ ] **Step 4: Commit**

```powershell
git add packaging/windows-desktop-node/manual-admin-tests/PcvServicePlanP1CloneActualVmSmoke.Tests.ps1
git commit -m "test: add P1 clone actual-VM DryRun pester"
```

---

### Task 4: Probe-vehicle package (checkpoint B, 별도 승인)

**Files:**
- Create: `docs/ga-ready/evidence/admin-smoke-package-YYYY-MM-DD-04276.md` (실행일에 날짜 고정)
- Artifact: `artifacts/admin-smoke-package-YYYYMMDD-04276`

**하지 않음:** current-evidence, AGENTS.md generated, fullgate, pair.

- [ ] **Step 1: 사용자 승인 확인.** 이 Task는 checkpoint A merge 뒤 새 승인 없이 시작하지 않는다.

- [ ] **Step 2: Publish + MSI** (관리자 불필요, host mutation false)

```powershell
$out = Join-Path (Get-Location) 'artifacts/admin-smoke-package-YYYYMMDD-04276'
New-Item -ItemType Directory -Path $out -Force | Out-Null
dotnet publish src/DesktopNode.Host/DesktopNode.Host.csproj -c Release -r win-x64 --self-contained true -o (Join-Path $out 'host-publish')
dotnet publish src/DesktopNode.Cli/DesktopNode.Cli.csproj -c Release -r win-x64 --self-contained true -o (Join-Path $out 'cli-publish')
pwsh -NoProfile -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version '0.42.76-admin-smoke' `
  -MsiProductVersion '0.42.76' `
  -DesktopNodeHostPath (Join-Path $out 'host-publish/DesktopNode.Host.exe') `
  -DesktopNodeCliPath (Join-Path $out 'cli-publish/pcvcli.exe') `
  -OutputRoot $out `
  -SigningMode AllowUnsignedDev `
  -SigningTrustModel LocalTest
```

- [ ] **Step 3: Evidence metadata (current 유지)**

```text
version: 0.42.76-admin-smoke
provenance: git rev-parse HEAD (clone-preserving)
signing_mode: AllowUnsignedDev
host_mutation_performed: false
package_installed: false
canonical_current_evidence: 0.42.75-admin-smoke
canonical_current_changed: false
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
```

MSI SHA-256과 payload aggregate를 실측으로 적는다. `current-evidence.json`을 맞추지 않는다.

- [ ] **Step 4: Commit** `docs: record 0.42.76 probe-vehicle package (current unchanged)`

---

### Task 5: Install 04276 (checkpoint C, 관리자 opt-in)

**하지 않음:** current-evidence, pair, fullgate, UAC 자동화.

- [ ] **Step 1: 사용자가 연 관리자 PowerShell 7에서만 실행.** controller는 RunAs를 하지 않는다.

명령은 Task 4 artifact의 실제 MSI 경로로 치환한다. 기존 04275 제품 wrapper Update 경로가 있으면 그것을 우선한다. 없으면 사용자에게 고정된 elevated apply 한 줄을 제시하고 출력을 읽는다.

전제 확인 (Lane 0):

```powershell
(Get-Content 'C:\Program Files\PureCVisor\DesktopNode\product-manifest.json' -Raw | ConvertFrom-Json).version
# 기대: 0.42.76-admin-smoke
Get-Service PureCVisorDesktopNode | Select-Object Status, StartType
# 기대: Running, Automatic
```

실패하면 D를 열지 않는다.

---

### Task 6: Live clone family (checkpoint D, 관리자 opt-in)

**Files:**
- Create: `docs/ga-ready/evidence/service-plan-p1-clone-actual-vm-YYYY-MM-DD-04276.md`

- [ ] **Step 1: 설치본 version이 `0.42.76-admin-smoke`인지 재확인.** 아니면 중단.

- [ ] **Step 2: 관리자 PowerShell**

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvServicePlanP1CloneActualVmSmoke.ps1 `
  -Version '0.42.76-admin-smoke' `
  -ArtifactRoot 'D:\data\projects\codex-zone\purecvisor-desktop-node-public\artifacts\service-plan-p1-clone-actual-vm-YYYYMMDD-04276' `
  -VmRoot 'D:\data\pcv-p1-clone-04276' `
  -IsoPath '<existing lab ISO used by P0>'
```

PASS: `overall_verdict=PASS`, `cleanup.verdict=PASS`, `secret_observed=false`, leftover `pcv-p1-clone-*` = 0.

FAIL summary는 current/`actual_vm_tested=pass` 입력이 될 수 없다.

- [ ] **Step 3: Evidence.** `canonical_current_changed: false`, `canonical_current_evidence: 0.42.75-admin-smoke`. feature ledger를 pass로 바꾸지 않는다.

- [ ] **Step 4: Commit** `docs: record P1 clone actual-VM probe` (current-evidence 제외)

---

## Self-review

| Spec 요구 | Task |
| --- | --- |
| DryRun, 설치본 호출 0 | 1, 2, 3 |
| 표시 이름 get/delete/clone | 1, 2 |
| slice 순서 source_create → mismatch → preview → clone → cleanup | 1, 2, 3 |
| `PCV_CLI_CONFIRMATION_REQUIRED` write 0 | 2 |
| 소스 Off, 시작 안 함 | 2 |
| current-evidence 금지 | 1, 2, 4, 6 |
| 04276 probe-vehicle, ledger 04275 | 4, 5 |
| Lane 2 `-Version` 설치본 일치 | 2, 5, 6 |
| Full P0 / fullgate / pair 금지 | Global, 4–6 |
| GUID CLI 금지 | 1, 2 |

즉시 실행은 Task 1–3. Task 4–6는 스펙 §4 별도 승인.
