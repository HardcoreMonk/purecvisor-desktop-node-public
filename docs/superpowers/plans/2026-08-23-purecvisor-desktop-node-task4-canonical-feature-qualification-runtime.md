# Canonical Feature Qualification Runtime Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the canonical current-evidence record the single build-time source for feature promotion status, reject blocked candidate promotions, and project the same immutable qualification through the installed API Ops Summary.

**Architecture:** `docs/ga-ready/current-evidence.json` stores the Task 3 evaluator result and is validated and rendered by the PowerShell generator. `DesktopNode.Api.csproj` publishes that same record as `evidence/current-evidence.json`; a construction-time C# provider turns it into an immutable snapshot that flows through the Ops Summary query and builder without depending on batch-evidence status.

**Tech Stack:** PowerShell 7, Pester 5.7, JSON Schema draft 2020-12, C#/.NET 10, xUnit, MSBuild Content assets.

---

## Preconditions and circuit boundary

- Work only in `.worktrees/ar-001-feature-evidence` on branch `codex/ar-001-feature-evidence`.
- Required predecessor commits are `9e13376`, `f723272`, and design commit `c891f97`.
- Treat each numbered task as one bounded checkpoint. Stop after its verification and commit.
- Do not run Hyper-V, service, MSI, firewall, trust-store, Event Log, reboot, install, update, or rollback commands.
- Do not modify `BatchEvidenceSummaryReader`, Web sources, CLI sources, or the feature promotion evaluator in this plan.
- Preserve `0.42.74-admin-smoke` as operational current and keep `0.42.75-admin-smoke` as a candidate.

## File responsibility map

| File | Responsibility |
| --- | --- |
| `docs/ga-ready/current-evidence.json` | Canonical operational current and evaluator decision |
| `docs/ga-ready/current-evidence.schema.json` | Full canonical JSON shape and eligibility/blocker invariant |
| `Update-PcvCurrentEvidenceDocs.ps1` | Validation, candidate guard, deterministic Markdown projection |
| `PcvCurrentEvidenceGeneration.Tests.ps1` | Canonical, no-write, generator, and publish metadata contract |
| `DesktopNode.Api.csproj` | Copy canonical source to `evidence/current-evidence.json` |
| `DesktopNodeCurrentEvidenceProvider.cs` | One-time safe load and immutable qualification snapshot |
| `DesktopNodeCurrentEvidenceProviderTests.cs` | Normal, missing, malformed, and non-disclosure tests |
| `DesktopNodeApiRequestProcessor.cs` | Optional test path seam and construction-time provider load |
| `DesktopNodeApiOpsSummaryHandler.cs` | Carry the snapshot through query and handler boundaries |
| `DesktopNodeApiOpsSummaryBuilder.cs` | Project qualification and independent signal |
| `ApiRuntimePolicyRequestProcessorTests.cs` | End-to-end Ops Summary projection with available batch evidence |

## Task 0: Clean baseline and scope lock

**Files:** Read only

- [ ] **Step 1: Verify isolation and branch state**

Run:

```powershell
git check-ignore -q .worktrees
git status --short -uall
git branch --show-current
git log -3 --oneline
```

Expected: `.worktrees` is ignored, status is empty, branch is
`codex/ar-001-feature-evidence`, and `c891f97` is HEAD.

- [ ] **Step 2: Capture focused PowerShell baseline**

Run:

```powershell
$result = Invoke-Pester `
  -Path 'packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1' `
  -Output Detailed `
  -PassThru
if ($result.FailedCount -ne 0 -or $result.SkippedCount -ne 0) {
    throw "Baseline Pester failed: passed=$($result.PassedCount) failed=$($result.FailedCount) skipped=$($result.SkippedCount)"
}
```

Expected: 5 passed, 0 failed, 0 skipped.

- [ ] **Step 3: Capture focused Ops Summary baseline**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj `
  -c Release `
  --filter 'FullyQualifiedName~OpsSummary'
```

Expected: exit 0 and no failed tests.

## Task 1: Canonical record, schema, generator guard, and generated documents

**Files:**

- Modify: `docs/ga-ready/current-evidence.json`
- Modify: `docs/ga-ready/current-evidence.schema.json`
- Modify: `packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`
- Modify generated blocks: `README.md`, `AGENTS.md`, `docs/DEVELOPER_INDEX.md`, `docs/ga-ready/EVIDENCE_INDEX.md`, `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`, `docs/ga-ready/CONTROL_PLANE_INDEX.md`, `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `packaging/windows-desktop-node/README.md`

- [ ] **Step 1: Add canonical and guard RED tests**

Add these assertions to `PcvCurrentEvidenceGeneration.Tests.ps1`:

```powershell
It 'requires a schema-valid blocked feature qualification for current 04274' {
    $json = Get-Content -Raw -LiteralPath $script:EvidencePath
    $json | Test-Json -SchemaFile $script:SchemaPath -ErrorAction Stop | Should -BeTrue
    $record = $json | ConvertFrom-Json -Depth 64

    $record.feature_qualification.schema_version | Should -Be 1
    $record.feature_qualification.contract | Should -Be 'pcv-feature-promotion-decision-v1'
    $record.feature_qualification.promotion_eligible | Should -BeFalse
    @($record.feature_qualification.blockers).Count | Should -Be 1
    $record.feature_qualification.blockers[0].feature_id | Should -Be 'pcv.vm.saved-lifecycle'
    $record.feature_qualification.blockers[0].stage | Should -Be 'actual_vm_tested'
    $record.feature_qualification.blockers[0].verdict | Should -Be 'fail'
}

It 'rejects contradictory eligibility and blocker combinations in the schema' {
    $eligible = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
    $eligible.feature_qualification.promotion_eligible = $true
    $eligible.feature_qualification.blockers = @()
    ($eligible | ConvertTo-Json -Depth 64) |
        Test-Json -SchemaFile $script:SchemaPath -ErrorAction Stop |
        Should -BeTrue

    $contradictory = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
    $contradictory.feature_qualification.promotion_eligible = $true
    ($contradictory | ConvertTo-Json -Depth 64) |
        Test-Json -SchemaFile $script:SchemaPath -ErrorAction Stop |
        Should -BeFalse
}

It 'renders the feature qualification independently of operational current' {
    $block = ConvertTo-PcvCurrentEvidenceMarkdown -Record $script:Record

    $block | Should -Match 'Feature qualification:'
    $block | Should -Match 'promotion_eligible=false'
    $block | Should -Match 'blocker_count=1'
    $block | Should -Match 'pcv\.vm\.saved-lifecycle/actual_vm_tested/fail'
    $block | Should -Match ([regex]::Escape([string]$script:Record.current.version))
}

It 'rejects a blocked candidate before writing any source or target file' {
    $candidatePath = Join-Path $TestDrive '04275-blocked.json'
    $candidate = $script:Record | ConvertTo-Json -Depth 64 | ConvertFrom-Json -Depth 64
    $candidate.current.version = '0.42.75-admin-smoke'
    $candidate | ConvertTo-Json -Depth 64 | Set-Content -LiteralPath $candidatePath -Encoding utf8

    $ownedPaths = @(
        $candidatePath,
        $script:EvidencePath,
        (Join-Path $script:RepoRoot 'README.md'),
        (Join-Path $script:RepoRoot 'AGENTS.md'),
        (Join-Path $script:RepoRoot 'docs/DEVELOPER_INDEX.md'),
        (Join-Path $script:RepoRoot 'docs/ga-ready/EVIDENCE_INDEX.md'),
        (Join-Path $script:RepoRoot 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'),
        (Join-Path $script:RepoRoot 'docs/ga-ready/CONTROL_PLANE_INDEX.md'),
        (Join-Path $script:RepoRoot 'docs/DEVELOPMENT_VERIFICATION_POLICY.md'),
        (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/README.md')
    )
    $before = @{}
    foreach ($path in $ownedPaths) {
        $before[$path] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    }

    $output = & pwsh -NoProfile -File $script:GeneratorPath `
        -EvidencePath $candidatePath `
        -RepoRoot $script:RepoRoot `
        -Check 2>&1
    $LASTEXITCODE | Should -Be 1
    ($output -join "`n") | Should -Match 'PCV_FEATURE_PROMOTION_BLOCKED'
    foreach ($path in $ownedPaths) {
        (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash |
            Should -Be $before[$path] -Because $path
    }
}
```

- [ ] **Step 2: Run the PowerShell RED suite**

Run:

```powershell
$result = Invoke-Pester `
  -Path 'packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1' `
  -Output Detailed `
  -PassThru
if ($result.FailedCount -eq 0) { throw 'Expected feature qualification RED failures.' }
```

Expected: existing tests pass; new tests fail because `feature_qualification`, schema rules, projection, and
`PCV_FEATURE_PROMOTION_BLOCKED` do not exist.

- [ ] **Step 3: Add the current 04274 evaluator decision**

Add this top-level property between `current` and `manual_admin` in
`docs/ga-ready/current-evidence.json`:

```json
"feature_qualification": {
  "schema_version": 1,
  "contract": "pcv-feature-promotion-decision-v1",
  "promotion_eligible": false,
  "blockers": [
    {
      "feature_id": "pcv.vm.saved-lifecycle",
      "stage": "actual_vm_tested",
      "verdict": "fail"
    }
  ]
}
```

- [ ] **Step 4: Implement the JSON Schema invariant**

Add `feature_qualification` to the root `required` array and property map:

```json
"feature_qualification": { "$ref": "#/$defs/featureQualification" }
```

Add these definitions under `$defs`:

```json
"featureQualification": {
  "type": "object",
  "additionalProperties": false,
  "required": ["schema_version", "contract", "promotion_eligible", "blockers"],
  "properties": {
    "schema_version": { "const": 1 },
    "contract": { "const": "pcv-feature-promotion-decision-v1" },
    "promotion_eligible": { "type": "boolean" },
    "blockers": {
      "type": "array",
      "items": { "$ref": "#/$defs/featureQualificationBlocker" }
    }
  },
  "allOf": [
    {
      "if": {
        "properties": { "promotion_eligible": { "const": true } },
        "required": ["promotion_eligible"]
      },
      "then": { "properties": { "blockers": { "maxItems": 0 } } },
      "else": { "properties": { "blockers": { "minItems": 1 } } }
    }
  ]
},
"featureQualificationBlocker": {
  "type": "object",
  "additionalProperties": false,
  "required": ["feature_id", "stage", "verdict"],
  "properties": {
    "feature_id": { "type": "string", "pattern": "^pcv\\.[a-z0-9._-]+$" },
    "stage": {
      "enum": [
        "code_tested",
        "packaged",
        "installed_tested",
        "actual_vm_tested",
        "manual_admin_tested"
      ]
    },
    "verdict": { "enum": ["fail", "blocked", "missing"] }
  }
}
```

- [ ] **Step 5: Add PowerShell validation and candidate guard**

Add these functions to `Update-PcvCurrentEvidenceDocs.ps1` before
`Test-PcvCurrentEvidenceRecord`:

```powershell
function Test-PcvFeatureQualification {
    [CmdletBinding()]
    param([Parameter(Mandatory)][object]$Qualification)

    $schemaVersion = Get-PcvCurrentEvidenceProperty `
        -Value $Qualification -Name 'schema_version' -Field 'feature_qualification.schema_version'
    if ([int]$schemaVersion -ne 1) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.schema_version' -Detail ([string]$schemaVersion)
    }
    $contract = [string](Get-PcvCurrentEvidenceProperty `
        -Value $Qualification -Name 'contract' -Field 'feature_qualification.contract')
    if ($contract -ne 'pcv-feature-promotion-decision-v1') {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.contract' -Detail $contract
    }
    $eligible = Get-PcvCurrentEvidenceProperty `
        -Value $Qualification -Name 'promotion_eligible' -Field 'feature_qualification.promotion_eligible'
    if ($eligible -isnot [bool]) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.promotion_eligible' -Detail 'must-be-boolean'
    }
    $blockers = @(Get-PcvCurrentEvidenceProperty `
        -Value $Qualification -Name 'blockers' -Field 'feature_qualification.blockers')
    if ([bool]$eligible -and $blockers.Count -ne 0) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers' -Detail 'eligible-must-be-empty'
    }
    if (-not [bool]$eligible -and $blockers.Count -eq 0) {
        Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers' -Detail 'blocked-must-not-be-empty'
    }

    $stages = @('code_tested', 'packaged', 'installed_tested', 'actual_vm_tested', 'manual_admin_tested')
    foreach ($blocker in $blockers) {
        $featureId = [string](Get-PcvCurrentEvidenceProperty `
            -Value $blocker -Name 'feature_id' -Field 'feature_qualification.blockers.feature_id')
        $stage = [string](Get-PcvCurrentEvidenceProperty `
            -Value $blocker -Name 'stage' -Field 'feature_qualification.blockers.stage')
        $verdict = [string](Get-PcvCurrentEvidenceProperty `
            -Value $blocker -Name 'verdict' -Field 'feature_qualification.blockers.verdict')
        if ($featureId -notmatch '^pcv\.[a-z0-9._-]+$') {
            Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers.feature_id' -Detail $featureId
        }
        if ($stage -notin $stages) {
            Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers.stage' -Detail $stage
        }
        if ($verdict -notin @('fail', 'blocked', 'missing')) {
            Throw-PcvCurrentEvidenceInvalid -Field 'feature_qualification.blockers.verdict' -Detail $verdict
        }
    }

    $Qualification
}

function Assert-PcvFeaturePromotionAllowed {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][object]$ProposedRecord,
        [Parameter(Mandatory)][object]$CanonicalRecord
    )

    $proposedCurrent = Get-PcvCurrentEvidenceProperty -Value $ProposedRecord -Name 'current' -Field 'current'
    $canonicalCurrent = Get-PcvCurrentEvidenceProperty -Value $CanonicalRecord -Name 'current' -Field 'current'
    $qualification = Get-PcvCurrentEvidenceProperty `
        -Value $ProposedRecord -Name 'feature_qualification' -Field 'feature_qualification'
    [void](Test-PcvFeatureQualification -Qualification $qualification)
    $proposedVersion = [string](Get-PcvCurrentEvidenceProperty `
        -Value $proposedCurrent -Name 'version' -Field 'current.version')
    $canonicalVersion = [string](Get-PcvCurrentEvidenceProperty `
        -Value $canonicalCurrent -Name 'version' -Field 'current.version')

    if ($proposedVersion -ne $canonicalVersion -and -not [bool]$qualification.promotion_eligible) {
        throw "PCV_FEATURE_PROMOTION_BLOCKED|$proposedVersion|blockers=$(@($qualification.blockers).Count)"
    }
}
```

In `Test-PcvCurrentEvidenceRecord`, require and validate the new property:

```powershell
$qualification = Get-PcvCurrentEvidenceProperty `
    -Value $Record -Name 'feature_qualification' -Field 'feature_qualification'
[void](Test-PcvFeatureQualification -Qualification $qualification)
```

Before calling `Test-PcvCurrentEvidenceRecord` in the executable script body, load the repository baseline
and apply the guard:

```powershell
$canonicalEvidencePath = Join-Path $resolvedRepoRoot 'docs/ga-ready/current-evidence.json'
$canonicalRecord = Get-Content -Raw -LiteralPath $canonicalEvidencePath | ConvertFrom-Json -Depth 64
Assert-PcvFeaturePromotionAllowed -ProposedRecord $record -CanonicalRecord $canonicalRecord
[void](Test-PcvCurrentEvidenceRecord -Record $record -RepoRoot $resolvedRepoRoot)
```

- [ ] **Step 6: Add deterministic Markdown projection**

At the start of `ConvertTo-PcvCurrentEvidenceMarkdown`, calculate:

```powershell
$qualification = $Record.feature_qualification
$blockers = @($qualification.blockers)
$blockerText = if ($blockers.Count -eq 0) {
    'none'
}
else {
    ($blockers | ForEach-Object { "$($_.feature_id)/$($_.stage)/$($_.verdict)" }) -join ','
}
$qualificationLine = "- Feature qualification: ``contract=$($qualification.contract)``; " +
    "``promotion_eligible=$(([string]([bool]$qualification.promotion_eligible)).ToLowerInvariant())``; " +
    "``blocker_count=$($blockers.Count)``; ``blockers=$blockerText``."
```

Insert `$qualificationLine` after the Actual-VM functional evidence line in the generated block.

- [ ] **Step 7: Regenerate all eight owned blocks**

Run:

```powershell
pwsh -NoProfile -File `
  'packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1'
if ($LASTEXITCODE -ne 0) { throw 'Current evidence generation failed.' }
```

Expected: JSON output has `ok=true`; all eight targets report `updated`.

- [ ] **Step 8: Run the Task 1 GREEN gate**

Run:

```powershell
$result = Invoke-Pester `
  -Path 'packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1' `
  -Output Detailed `
  -PassThru
if ($result.FailedCount -ne 0 -or $result.SkippedCount -ne 0) {
    throw "Task 1 failed: passed=$($result.PassedCount) failed=$($result.FailedCount) skipped=$($result.SkippedCount)"
}
pwsh -NoProfile -File `
  'packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1' `
  -Check
if ($LASTEXITCODE -ne 0) { throw 'Generated current evidence is stale.' }
git diff --check
```

Expected: all focused tests pass, zero skipped, generator check reports eight current targets, and diff check exits 0.

- [ ] **Step 9: Commit Task 1**

Run:

```powershell
git add -- `
  docs/ga-ready/current-evidence.json `
  docs/ga-ready/current-evidence.schema.json `
  packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1 `
  packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1 `
  README.md AGENTS.md docs/DEVELOPER_INDEX.md `
  docs/ga-ready/EVIDENCE_INDEX.md `
  docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md `
  docs/ga-ready/CONTROL_PLANE_INDEX.md `
  docs/DEVELOPMENT_VERIFICATION_POLICY.md `
  packaging/windows-desktop-node/README.md
git diff --cached --check
git commit -m 'feat: fail closed on feature promotion blockers'
```

Expected: commit succeeds and the worktree is clean.

## Task 2: Publish asset and immutable C# provider

**Files:**

- Create: `src/DesktopNode.Api/DesktopNodeCurrentEvidenceProvider.cs`
- Create: `src/DesktopNode.Api.Tests/DesktopNodeCurrentEvidenceProviderTests.cs`
- Modify: `src/DesktopNode.Api/DesktopNode.Api.csproj`
- Modify: `packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1`

- [ ] **Step 1: Add provider and publish RED tests**

Create `DesktopNodeCurrentEvidenceProviderTests.cs`:

```csharp
using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeCurrentEvidenceProviderTests
{
    [Fact]
    public void LoadPreservesBlockedQualificationAndBlockerOrder()
    {
        var path = WriteEvidence(BlockedEvidence("pcv.vm.saved-lifecycle", "pcv.vm.media-attach"));
        try
        {
            var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);

            Assert.Equal(1, snapshot.SchemaVersion);
            Assert.Equal("pcv-feature-promotion-decision-v1", snapshot.Contract);
            Assert.Equal("blocked", snapshot.Status);
            Assert.False(snapshot.PromotionEligible);
            Assert.Null(snapshot.ErrorCode);
            Assert.Equal(
                ["pcv.vm.saved-lifecycle", "pcv.vm.media-attach"],
                snapshot.Blockers.Select(blocker => blocker.FeatureId).ToArray());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadReturnsUnavailableForMissingAssetWithoutDisclosingPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "pcv-current-evidence-missing-" + Guid.NewGuid().ToString("N") + ".json");

        var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);
        var serialized = JsonSerializer.Serialize(snapshot);

        Assert.Equal("unavailable", snapshot.Status);
        Assert.False(snapshot.PromotionEligible);
        Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", snapshot.ErrorCode);
        Assert.Empty(snapshot.Blockers);
        Assert.DoesNotContain(path, serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadReturnsUnavailableForMalformedAssetWithoutDisclosingParserInput()
    {
        var path = WriteEvidence("{broken-current-evidence");
        try
        {
            var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);
            var serialized = JsonSerializer.Serialize(snapshot);

            Assert.Equal("unavailable", snapshot.Status);
            Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", snapshot.ErrorCode);
            Assert.DoesNotContain(path, serialized, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("broken-current-evidence", serialized, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadReturnsUnavailableForAnInvalidCurrentEvidenceContract()
    {
        var path = WriteEvidence(BlockedEvidence("pcv.vm.saved-lifecycle")
            .Replace("pcv-current-evidence-v1", "pcv-current-evidence-v99", StringComparison.Ordinal));
        try
        {
            var snapshot = DesktopNodeCurrentEvidenceProvider.Load(path);

            Assert.Equal("unavailable", snapshot.Status);
            Assert.False(snapshot.PromotionEligible);
            Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", snapshot.ErrorCode);
            Assert.Empty(snapshot.Blockers);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string WriteEvidence(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "pcv-current-evidence-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, content);
        return path;
    }

    private static string BlockedEvidence(params string[] featureIds)
    {
        var blockers = featureIds.Select(featureId => new
        {
            feature_id = featureId,
            stage = "actual_vm_tested",
            verdict = "fail"
        });
        return JsonSerializer.Serialize(new
        {
            schema_version = 1,
            contract = "pcv-current-evidence-v1",
            current = new { version = "0.42.74-admin-smoke" },
            feature_qualification = new
            {
                schema_version = 1,
                contract = "pcv-feature-promotion-decision-v1",
                promotion_eligible = false,
                blockers
            }
        });
    }
}
```

Add this Pester test:

```powershell
It 'publishes the canonical record as the API current evidence asset' {
    $projectPath = Join-Path $script:RepoRoot 'src/DesktopNode.Api/DesktopNode.Api.csproj'
    [xml]$project = Get-Content -Raw -LiteralPath $projectPath
    $content = @($project.Project.ItemGroup.Content | Where-Object {
            $_.Include -eq '..\..\docs\ga-ready\current-evidence.json'
        })

    $content.Count | Should -Be 1
    [string]$content[0].Link | Should -Be 'evidence\current-evidence.json'
    [string]$content[0].CopyToOutputDirectory | Should -Be 'PreserveNewest'
    [string]$content[0].CopyToPublishDirectory | Should -Be 'PreserveNewest'
}
```

- [ ] **Step 2: Run the provider RED suites**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj `
  -c Release `
  --filter 'FullyQualifiedName~DesktopNodeCurrentEvidenceProviderTests'
$dotnetExit = $LASTEXITCODE
$pester = Invoke-Pester `
  -Path 'packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1' `
  -Output Detailed `
  -PassThru
if ($dotnetExit -eq 0 -or $pester.FailedCount -eq 0) {
    throw 'Expected missing provider type and publish metadata RED failures.'
}
```

Expected: C# fails because `DesktopNodeCurrentEvidenceProvider` is undefined; Pester fails because the
Content asset metadata is absent.

- [ ] **Step 3: Implement the immutable provider**

Create `DesktopNodeCurrentEvidenceProvider.cs`:

```csharp
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Api;

internal sealed record DesktopNodeFeatureQualificationBlocker(
    string FeatureId,
    string Stage,
    string Verdict);

internal sealed record DesktopNodeFeatureQualificationSnapshot(
    int SchemaVersion,
    string Contract,
    string Status,
    bool PromotionEligible,
    IReadOnlyList<DesktopNodeFeatureQualificationBlocker> Blockers,
    string? ErrorCode)
{
    public static DesktopNodeFeatureQualificationSnapshot Unavailable() => new(
        1,
        "pcv-feature-promotion-decision-v1",
        "unavailable",
        false,
        Array.Empty<DesktopNodeFeatureQualificationBlocker>(),
        "PCV_CURRENT_EVIDENCE_UNAVAILABLE");
}

internal static partial class DesktopNodeCurrentEvidenceProvider
{
    private static readonly HashSet<string> Stages =
    [
        "code_tested",
        "packaged",
        "installed_tested",
        "actual_vm_tested",
        "manual_admin_tested"
    ];

    private static readonly HashSet<string> BlockerVerdicts = ["fail", "blocked", "missing"];

    public static DesktopNodeFeatureQualificationSnapshot Load(string? path = null)
    {
        var resolvedPath = path ?? Path.Combine(
            AppContext.BaseDirectory,
            "evidence",
            "current-evidence.json");
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(resolvedPath));
            return Parse(document.RootElement);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            return DesktopNodeFeatureQualificationSnapshot.Unavailable();
        }
    }

    private static DesktopNodeFeatureQualificationSnapshot Parse(JsonElement root)
    {
        if (RequireInt32(root, "schema_version") != 1)
        {
            throw new InvalidDataException("schema_version");
        }
        RequireString(root, "contract", "pcv-current-evidence-v1");
        var current = RequireObject(root, "current");
        var version = RequireString(current, "version");
        if (!AdminSmokeVersion().IsMatch(version))
        {
            throw new InvalidDataException("current.version");
        }

        var qualification = RequireObject(root, "feature_qualification");
        var schemaVersion = RequireInt32(qualification, "schema_version");
        if (schemaVersion != 1)
        {
            throw new InvalidDataException("feature_qualification.schema_version");
        }
        var contract = RequireString(
            qualification,
            "contract",
            "pcv-feature-promotion-decision-v1");
        var promotionEligible = RequireBoolean(qualification, "promotion_eligible");
        var blockerRows = RequireArray(qualification, "blockers");
        var blockers = new List<DesktopNodeFeatureQualificationBlocker>();
        foreach (var row in blockerRows.EnumerateArray())
        {
            var featureId = RequireString(row, "feature_id");
            var stage = RequireString(row, "stage");
            var verdict = RequireString(row, "verdict");
            if (!FeatureId().IsMatch(featureId) || !Stages.Contains(stage) || !BlockerVerdicts.Contains(verdict))
            {
                throw new InvalidDataException("feature_qualification.blockers");
            }
            blockers.Add(new DesktopNodeFeatureQualificationBlocker(featureId, stage, verdict));
        }
        if (promotionEligible != (blockers.Count == 0))
        {
            throw new InvalidDataException("feature_qualification.invariant");
        }

        ReadOnlyCollection<DesktopNodeFeatureQualificationBlocker> immutable = blockers.AsReadOnly();
        return new DesktopNodeFeatureQualificationSnapshot(
            schemaVersion,
            contract,
            promotionEligible ? "eligible" : "blocked",
            promotionEligible,
            immutable,
            null);
    }

    private static JsonElement RequireObject(JsonElement element, string name)
    {
        var value = RequireProperty(element, name);
        return value.ValueKind == JsonValueKind.Object
            ? value
            : throw new InvalidDataException(name);
    }

    private static JsonElement RequireArray(JsonElement element, string name)
    {
        var value = RequireProperty(element, name);
        return value.ValueKind == JsonValueKind.Array
            ? value
            : throw new InvalidDataException(name);
    }

    private static string RequireString(JsonElement element, string name, string? expected = null)
    {
        var value = RequireProperty(element, name);
        var text = value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
        if (string.IsNullOrWhiteSpace(text) || (expected is not null && !string.Equals(text, expected, StringComparison.Ordinal)))
        {
            throw new InvalidDataException(name);
        }
        return text;
    }

    private static int RequireInt32(JsonElement element, string name)
    {
        var value = RequireProperty(element, name);
        return value.TryGetInt32(out var result)
            ? result
            : throw new InvalidDataException(name);
    }

    private static bool RequireBoolean(JsonElement element, string name)
    {
        return RequireProperty(element, name).ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new InvalidDataException(name)
        };
    }

    private static JsonElement RequireProperty(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value
            : throw new InvalidDataException(name);
    }

    [GeneratedRegex("^pcv\\.[a-z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex FeatureId();

    [GeneratedRegex("^0\\.\\d+\\.\\d+-admin-smoke$", RegexOptions.CultureInvariant)]
    private static partial Regex AdminSmokeVersion();
}
```

- [ ] **Step 4: Publish the canonical source asset**

Add this item to `DesktopNode.Api.csproj`:

```xml
<ItemGroup>
  <Content Include="..\..\docs\ga-ready\current-evidence.json">
    <Link>evidence\current-evidence.json</Link>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </Content>
</ItemGroup>
```

- [ ] **Step 5: Run the provider GREEN gate and verify the copied bytes**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj `
  -c Release `
  --filter 'FullyQualifiedName~DesktopNodeCurrentEvidenceProviderTests'
if ($LASTEXITCODE -ne 0) { throw 'Provider tests failed.' }

$pester = Invoke-Pester `
  -Path 'packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1' `
  -Output Detailed `
  -PassThru
if ($pester.FailedCount -ne 0 -or $pester.SkippedCount -ne 0) { throw 'Publish metadata Pester failed.' }

dotnet build src/DesktopNode.Api/DesktopNode.Api.csproj -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'API build failed.' }
$source = 'docs/ga-ready/current-evidence.json'
$copied = 'src/DesktopNode.Api/bin/Release/net10.0-windows/evidence/current-evidence.json'
if (-not (Test-Path -LiteralPath $copied -PathType Leaf)) { throw 'Published current evidence asset is missing.' }
$sourceHash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$copiedHash = (Get-FileHash -LiteralPath $copied -Algorithm SHA256).Hash
if ($copiedHash -ne $sourceHash) { throw 'Published current evidence asset differs from canonical source.' }
```

Expected: provider tests and Pester pass with zero skipped, build exits 0, and source/copy SHA-256 values match.

- [ ] **Step 6: Commit Task 2**

Run:

```powershell
git add -- `
  src/DesktopNode.Api/DesktopNodeCurrentEvidenceProvider.cs `
  src/DesktopNode.Api.Tests/DesktopNodeCurrentEvidenceProviderTests.cs `
  src/DesktopNode.Api/DesktopNode.Api.csproj `
  packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1
git diff --cached --check
git commit -m 'feat: load canonical current evidence asset'
```

Expected: commit succeeds and generated `bin`/`obj` paths remain untracked or ignored.

## Task 3: Ops Summary snapshot and signal integration

**Files:**

- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiOpsSummaryHandler.cs`
- Modify: `src/DesktopNode.Api/DesktopNodeApiOpsSummaryBuilder.cs`
- Modify: `src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs`

- [ ] **Step 1: Add end-to-end RED assertions for blocked and unavailable states**

In `OpsSummaryIncludesBatchEvidenceWhenRootIsConfigured`, write a current evidence asset inside its temporary
root before constructing the processor:

```csharp
var currentEvidencePath = Path.Combine(root, "current-evidence.json");
File.WriteAllText(currentEvidencePath, """
{
  "schema_version": 1,
  "contract": "pcv-current-evidence-v1",
  "current": { "version": "0.42.74-admin-smoke" },
  "feature_qualification": {
    "schema_version": 1,
    "contract": "pcv-feature-promotion-decision-v1",
    "promotion_eligible": false,
    "blockers": [
      {
        "feature_id": "pcv.vm.saved-lifecycle",
        "stage": "actual_vm_tested",
        "verdict": "fail"
      }
    ]
  }
}
""");
```

Pass `currentEvidencePath: currentEvidencePath` as the last named argument to `CreateDefault`, then add:

Immediately after constructing the processor, overwrite the source file with an eligible decision. The response
must still use the construction-time blocked snapshot:

```csharp
File.WriteAllText(currentEvidencePath, """
{
  "schema_version": 1,
  "contract": "pcv-current-evidence-v1",
  "current": { "version": "0.42.74-admin-smoke" },
  "feature_qualification": {
    "schema_version": 1,
    "contract": "pcv-feature-promotion-decision-v1",
    "promotion_eligible": true,
    "blockers": []
  }
}
""");
```

Then add these response assertions:

```csharp
var qualification = currentEvidence.GetProperty("feature_qualification");
Assert.Equal("pcv-feature-promotion-decision-v1", qualification.GetProperty("contract").GetString());
Assert.Equal("blocked", qualification.GetProperty("status").GetString());
Assert.False(qualification.GetProperty("promotion_eligible").GetBoolean());
var blocker = Assert.Single(qualification.GetProperty("blockers").EnumerateArray());
Assert.Equal("pcv.vm.saved-lifecycle", blocker.GetProperty("feature_id").GetString());
Assert.Equal("actual_vm_tested", blocker.GetProperty("stage").GetString());
Assert.Equal("fail", blocker.GetProperty("verdict").GetString());
Assert.Contains(
    document.RootElement.GetProperty("data").GetProperty("signals").EnumerateArray(),
    signal => signal.GetProperty("key").GetString() == "feature-promotion" &&
              signal.GetProperty("tone").GetString() == "error" &&
              signal.GetProperty("value").GetInt32() == 1);
```

Add this complete test:

```csharp
[Fact]
public void OpsSummaryKeepsRunningWithUnavailableCurrentEvidenceWithoutLeakingPath()
{
    var root = Path.Combine(Path.GetTempPath(), "pcv-current-evidence-api-missing-" + Guid.NewGuid().ToString("N"));
    var missingPath = Path.Combine(root, "secret-current-evidence.json");
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(
        nativeAdapter: new RecordingNativeHyperVAdapter(new List<string>(), new Dictionary<string, string>
        {
            ["host.status"] = """{"ok":true,"operation":"host.status","data":{"supported":true},"error":null}""",
            ["vm.list"] = """{"ok":true,"operation":"vm.list","data":[],"error":null}"""
        }),
        currentEvidencePath: missingPath);

    var response = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/ops/summary"));

    Assert.Equal(200, response.StatusCode);
    using var document = JsonDocument.Parse(response.Body);
    var data = document.RootElement.GetProperty("data");
    var qualification = data.GetProperty("current_evidence").GetProperty("feature_qualification");
    Assert.Equal("unavailable", qualification.GetProperty("status").GetString());
    Assert.False(qualification.GetProperty("promotion_eligible").GetBoolean());
    Assert.Equal("PCV_CURRENT_EVIDENCE_UNAVAILABLE", qualification.GetProperty("error_code").GetString());
    Assert.Empty(qualification.GetProperty("blockers").EnumerateArray());
    Assert.Contains(
        data.GetProperty("signals").EnumerateArray(),
        signal => signal.GetProperty("key").GetString() == "feature-promotion" &&
                  signal.GetProperty("tone").GetString() == "error" &&
                  signal.GetProperty("value").GetString() == "unavailable");
    Assert.DoesNotContain(root, response.Body, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("secret-current-evidence.json", response.Body, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Run the Ops Summary RED suite**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj `
  -c Release `
  --filter 'FullyQualifiedName~OpsSummary'
```

Expected: compilation or assertions fail because `currentEvidencePath`, snapshot transport,
`current_evidence.feature_qualification`, and `feature-promotion` are absent.

- [ ] **Step 3: Load the provider once in the processor factory**

Add `string? currentEvidencePath` as the final private-constructor parameter, final
`CreateWithDependencies` optional parameter, and final `CreateDefault` optional parameter after
`jobRuntimeEventSink`:

```csharp
string? currentEvidencePath = null
```

Forward it through both factory calls and construct the immutable snapshot once:

```csharp
var featureQualification = DesktopNodeCurrentEvidenceProvider.Load(currentEvidencePath);
opsSummaryHandler = new DesktopNodeApiOpsSummaryHandler(
    new DesktopNodeApiOpsSummaryQuery(
        nativeAdapter,
        jobRuntime,
        authSessionHandler,
        tokenStorage,
        currentExposure,
        resolvedConsoleOptions,
        batchEvidenceRoot,
        diagnosticsHandler.DiagnosticsRoot,
        featureQualification));
```

- [ ] **Step 4: Carry the immutable snapshot through the query**

Change the snapshot record in `DesktopNodeApiOpsSummaryHandler.cs`:

```csharp
internal sealed record DesktopNodeApiOpsSummarySnapshot(
    DesktopNodeHyperVOperationResult HostResult,
    DesktopNodeHyperVOperationResult VmResult,
    IReadOnlyList<JsonElement> JobRows,
    RuntimePolicyData RuntimePolicy,
    JsonElement BatchEvidence,
    string? DiagnosticsRoot,
    DesktopNodeJobStoreHealthSnapshot? JobStoreHealth = null,
    DesktopNodeFeatureQualificationSnapshot? FeatureQualification = null);
```

Add the optional query constructor parameter and field:

```csharp
private readonly DesktopNodeFeatureQualificationSnapshot featureQualification;

public DesktopNodeApiOpsSummaryQuery(
    IDesktopNodeHyperVNativeAdapter nativeAdapter,
    DesktopNodeJobRuntime jobRuntime,
    DesktopNodeApiAuthSessionHandler authSessionHandler,
    string tokenStorage,
    string currentExposure,
    DesktopNodeConsoleOptions consoleOptions,
    string? batchEvidenceRoot,
    string? diagnosticsRoot,
    DesktopNodeFeatureQualificationSnapshot? featureQualification = null)
{
    this.nativeAdapter = nativeAdapter;
    this.jobRuntime = jobRuntime;
    this.authSessionHandler = authSessionHandler;
    this.tokenStorage = tokenStorage;
    this.currentExposure = currentExposure;
    this.consoleOptions = consoleOptions;
    batchEvidenceReader = new BatchEvidenceSummaryReader(batchEvidenceRoot);
    this.diagnosticsRoot = diagnosticsRoot;
    this.featureQualification = featureQualification ?? DesktopNodeFeatureQualificationSnapshot.Unavailable();
}
```

Pass `featureQualification` as the final argument when constructing `DesktopNodeApiOpsSummarySnapshot`.
Pass `snapshot.FeatureQualification` as the final argument to `DesktopNodeApiOpsSummaryBuilder.Build`.

- [ ] **Step 5: Project qualification and signal without batch coercion**

Add the optional final builder parameter and normalize it once:

```csharp
public static JsonElement Build(
    DesktopNodeHyperVOperationResult hostResult,
    DesktopNodeHyperVOperationResult vmResult,
    IReadOnlyList<JsonElement> jobRows,
    object runtimePolicy,
    JsonElement batchEvidence,
    string? diagnosticsRoot = null,
    DesktopNodeJobStoreHealthSnapshot? jobStoreHealth = null,
    DesktopNodeFeatureQualificationSnapshot? featureQualification = null)
{
    var qualification = featureQualification ?? DesktopNodeFeatureQualificationSnapshot.Unavailable();
    var vms = vmResult.Ok && vmResult.Data is not null && vmResult.Data.Value.ValueKind == JsonValueKind.Array
        ? vmResult.Data.Value.EnumerateArray().Select(vm => vm.Clone()).ToArray()
        : [];
```

Pass `qualification` to `BuildCurrentEvidenceRollup` and `BuildOpsSignals`. Add this helper:

```csharp
private static SortedDictionary<string, object?> BuildFeatureQualification(
    DesktopNodeFeatureQualificationSnapshot qualification)
{
    var result = new SortedDictionary<string, object?>
    {
        ["blockers"] = qualification.Blockers
            .Select(blocker => new SortedDictionary<string, object?>
            {
                ["feature_id"] = blocker.FeatureId,
                ["stage"] = blocker.Stage,
                ["verdict"] = blocker.Verdict
            })
            .ToArray(),
        ["contract"] = qualification.Contract,
        ["promotion_eligible"] = qualification.PromotionEligible,
        ["schema_version"] = qualification.SchemaVersion,
        ["status"] = qualification.Status
    };
    if (!string.IsNullOrWhiteSpace(qualification.ErrorCode))
    {
        result["error_code"] = qualification.ErrorCode;
    }
    return result;
}
```

Add this entry to `BuildCurrentEvidenceRollup`:

```csharp
["feature_qualification"] = BuildFeatureQualification(qualification),
```

Add this signal after the existing batch signal:

```csharp
new SortedDictionary<string, object?>
{
    ["key"] = "feature-promotion",
    ["label"] = "Feature promotion",
    ["tone"] = qualification.Status == "eligible" ? "ok" : "error",
    ["value"] = qualification.Status == "unavailable"
        ? "unavailable"
        : qualification.Blockers.Count
}
```

Change the private method signatures so `BuildCurrentEvidenceRollup` and `BuildOpsSignals` each receive
`DesktopNodeFeatureQualificationSnapshot qualification`. Do not read batch evidence inside the feature
projection helper.

- [ ] **Step 6: Run the Ops Summary GREEN gate**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj `
  -c Release `
  --filter 'FullyQualifiedName~OpsSummary|FullyQualifiedName~CurrentEvidenceProvider'
if ($LASTEXITCODE -ne 0) { throw 'Focused API tests failed.' }

rg -n 'pcv\.vm\.saved-lifecycle|actual_vm_tested' `
  src/DesktopNode.Api `
  --glob '*.cs'
if ($LASTEXITCODE -eq 0) { throw 'Product C# contains a hardcoded current blocker.' }
git diff --check
```

Expected: focused tests pass; product C# contains no Saved/current-stage constants; diff check exits 0.

- [ ] **Step 7: Commit Task 3**

Run:

```powershell
git add -- `
  src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs `
  src/DesktopNode.Api/DesktopNodeApiOpsSummaryHandler.cs `
  src/DesktopNode.Api/DesktopNodeApiOpsSummaryBuilder.cs `
  src/DesktopNode.Api.Tests/ApiRuntimePolicyRequestProcessorTests.cs
git diff --cached --check
git commit -m 'feat: project feature qualification in ops summary'
```

Expected: commit succeeds and the worktree is clean.

## Task 4: Full non-mutating verification and handoff

**Files:** Read only unless a failure points to a changed line from Tasks 1-3.

- [ ] **Step 1: Run focused contracts with explicit counts**

Run:

```powershell
$pester = Invoke-Pester `
  -Path 'packaging/windows-desktop-node/tests/PcvCurrentEvidenceGeneration.Tests.ps1' `
  -Output Detailed `
  -PassThru
if ($pester.FailedCount -ne 0 -or $pester.SkippedCount -ne 0) {
    throw "Focused Pester failed: passed=$($pester.PassedCount) failed=$($pester.FailedCount) skipped=$($pester.SkippedCount)"
}

dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj `
  -c Release `
  --filter 'FullyQualifiedName~OpsSummary|FullyQualifiedName~CurrentEvidenceProvider'
if ($LASTEXITCODE -ne 0) { throw 'Focused API verification failed.' }
```

Expected: zero failures and zero focused skips.

- [ ] **Step 2: Verify generator drift and publish bytes**

Run:

```powershell
pwsh -NoProfile -File `
  'packaging/windows-desktop-node/tools/Update-PcvCurrentEvidenceDocs.ps1' `
  -Check
if ($LASTEXITCODE -ne 0) { throw 'Current evidence drift detected.' }

dotnet build src/DesktopNode.Api/DesktopNode.Api.csproj -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'API build failed.' }
$sourceHash = (Get-FileHash 'docs/ga-ready/current-evidence.json' -Algorithm SHA256).Hash
$assetHash = (Get-FileHash 'src/DesktopNode.Api/bin/Release/net10.0-windows/evidence/current-evidence.json' -Algorithm SHA256).Hash
if ($sourceHash -ne $assetHash) { throw 'Published current evidence asset differs from canonical source.' }
Write-Output "CURRENT_EVIDENCE_ASSET_SHA256=$($sourceHash.ToLowerInvariant())"
```

Expected: eight generated targets are current and source/published hashes are identical.

- [ ] **Step 3: Run the full .NET solution**

Run:

```powershell
dotnet test src/DesktopNode.sln -c Release --no-restore
```

Expected: exit 0 with no failed tests.

- [ ] **Step 4: Run the full non-mutating packaging Pester suite**

Run:

```powershell
$result = Invoke-Pester `
  -Path 'packaging/windows-desktop-node/tests' `
  -Output Detailed `
  -PassThru
if ($result.FailedCount -ne 0) {
    throw "Packaging Pester failed: passed=$($result.PassedCount) failed=$($result.FailedCount) skipped=$($result.SkippedCount)"
}
$targetSkips = @($result.Tests | Where-Object {
        $_.Result -eq 'Skipped' -and
        [System.IO.Path]::GetFileName($_.Path) -in @(
            'PcvCurrentEvidenceGeneration.Tests.ps1',
            'PcvFeatureEvidencePromotion.Tests.ps1'
        )
    })
if ($targetSkips.Count -ne 0) { throw 'A changed feature-evidence suite was skipped.' }
Write-Output "PACKAGING_PESTER=PASS passed=$($result.PassedCount) skipped_existing=$($result.SkippedCount)"
```

Expected: zero failures and no skip in either changed feature-evidence suite. Existing unrelated conditional
skips are reported, not converted into success claims about their unexecuted behavior.

- [ ] **Step 5: Run the fixed-diff review gate**

Run:

```powershell
git diff --check HEAD~3..HEAD
git status --short -uall
rg -n 'pcv\.vm\.saved-lifecycle|actual_vm_tested' src/DesktopNode.Api --glob '*.cs'
rg -n 'Set-Content|File\.WriteAllText|File\.Delete|FileSystemWatcher' `
  src/DesktopNode.Api/DesktopNodeCurrentEvidenceProvider.cs
```

Expected:

- diff check exits 0;
- worktree status is empty;
- blocker constants do not appear in product C#;
- provider contains no write/delete/watcher primitive;
- review scope is the three implementation commits only.

- [ ] **Step 6: Record the checkpoint result without push or host mutation**

Generate the exact report values from Git and the canonical asset:

```powershell
$implementationCommits = (git log -3 --format='%H') -join ','
$assetHash = (Get-FileHash 'docs/ga-ready/current-evidence.json' -Algorithm SHA256).Hash.ToLowerInvariant()
$record = Get-Content -Raw 'docs/ga-ready/current-evidence.json' | ConvertFrom-Json -Depth 64
@"
Task 4 implementation commits: $implementationCommits
Canonical version: $($record.current.version)
Promotion eligible: $(([string][bool]$record.feature_qualification.promotion_eligible).ToLowerInvariant())
Blocker count: $(@($record.feature_qualification.blockers).Count)
Published asset SHA-256: $assetHash
Host/VM/service/MSI mutation count: 0
Push performed: false
"@
```

Do not create an evidence claim for installed runtime or actual VM behavior from these code-level tests.

## Plan self-review checklist

- Spec sections 3-8 map to Tasks 1-3.
- Canonical JSON is the only product source of the Saved blocker.
- The API asset is a byte copy, not a second authored decision.
- Provider failure is unavailable, not a synthetic feature blocker.
- Candidate guard runs before evidence-reference validation and before writes.
- `CreateDefault` compatibility is preserved by one final optional parameter.
- Batch evidence reader and mutation surfaces stay outside the diff.
- Full verification distinguishes existing conditional skips from changed-suite skips.
