# Pester-free Installer Wave C Implementation Plan

> **Completion status (2026-08-25): completed.** Installer 6 files / 49 contracts가 replacement
> `49/49` local PASS로 닫혔고 후속 Wave E에서 CI PASS/cutover로 승격됐다. 아래 CI pending과
> Packaging unmapped 문구는 실행 전 snapshot이다. Wave C 증빙은
> `docs/ga-ready/evidence/pester-free-installer-wave-c-2026-08-25.md`다.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development and superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Installer legacy Pester 6파일/49계약을 shell-free .NET 10 xUnit 계약으로 1:1 이전하고, 62파일/627계약 ledger v2를 도입하여 Installer는 local parity PASS, Packaging은 정직한 unmapped, Web은 기존 local PASS로 표현한다.

**Architecture:** `DesktopNode.Delivery.Tests`는 제품 project를 참조하지 않는 read-only delivery verification assembly다. Custom xUnit fact metadata가 legacy path/ordinal/exact name/replacement ID를 소유하고, C# literal-Pester parser와 reflection inventory가 manifest v2를 독립 검증한다. Node generator/verifier는 전체 ledger JSON과 Web registry를 소유하지만 C# assembly metadata를 대신 신뢰하지 않는다. 각 legacy Installer 파일은 하나의 C# fixture로 보존한다.

**Tech Stack:** C# / .NET 10, xUnit 2.9.3, `System.Text.Json`, `System.Xml.Linq`, Node.js 24, JSON Schema draft 2020-12, Pester 5.7.1 reference-only dual run, Visual Studio 2026 또는 `dotnet` CLI.

---

## Preconditions and constraints

- Plan 1 completion on `HardcoreMonk/purecvisor-desktop-node-public`: the sanitized parentless authority is public, current four checks protect main, draft cutover PR exists, and active branch is `codex/pester-free-verification-cutover`.
- The original private archive and its dirty main remain read-only; no command in this plan targets that remote.
- Source design: `docs/superpowers/specs/2026-08-25-purecvisor-desktop-node-pester-free-required-ci-cutover-design.md`.
- Existing Wave B result remains 1 Web file / 50 contracts local PASS and CI pending.
- Installer scope is exactly 6 files / 49 literal `It` declarations; skipped replacement tests are forbidden.
- Keep every legacy `.Tests.ps1` assertion name, count, execution behavior, and runnability unchanged. The
  sanitized root's approved comment-only public-safety markers and runtime-built synthetic material are the
  baseline and must not be reverted; this plan makes no further legacy semantic change and does not cut over
  required CI.
- Pester is executed only as a temporary reference oracle. C#/Node replacement code must never start `pwsh`, `powershell`, `Invoke-Pester`, `msiexec`, `sc.exe`, service commands, or VM commands.
- No administrator elevation, installer invocation, service/host/VM mutation, package build, version bump, public signing claim, or external publication claim.
- Catalog activation remains non-cutover. `installer-contracts` may move to `mapped`; `active` is reserved for Wave E.
- Every legacy contract gets exactly one custom fact and one manifest contract row. Aggregate counts are derived and cross-checked, not typed in two unrelated places.
- All edits use `apply_patch`; the manifest regeneration command is an approved bulk mechanical rewrite.

## Fixed identity rules

For Installer and Delivery IDs:

1. Take the legacy filename without `.Tests.ps1`.
2. Remove the leading `Pcv`.
3. Convert `.`/`_` and lower-or-digit to upper transitions to a single `-`.
4. Lowercase invariantly and collapse repeated `-`.
5. Append a one-based, three-digit ordinal.

Examples:

```text
PcvDesktopNodeInstaller.InternalTrust.Tests.ps1, ordinal 1
=> pcv.installer.desktop-node-installer-internal-trust.001

Pcv04273PromotionEvidence.Tests.ps1, ordinal 7
=> pcv.delivery.04273-promotion-evidence.007
```

Web IDs stay exactly `web.static.*`; they are never regenerated into `pcv.*` IDs.

## Installer file ledger

| Legacy file | Contracts | C# replacement owner |
| --- | ---: | --- |
| `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1` | 4 | `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerInternalTrustContractTests.cs` |
| `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Lifecycle.Tests.ps1` | 5 | `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerLifecycleContractTests.cs` |
| `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1` | 21 | `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerPlanContractTests.cs` |
| `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Signing.Tests.ps1` | 6 | `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerSigningContractTests.cs` |
| `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1` | 10 | `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerWixSourceContractTests.cs` |
| `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Wrapper.Tests.ps1` | 3 | `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerWrapperContractTests.cs` |
| **Total** | **49** | **6 fixture owners** |

## Task 1: Add the isolated Delivery test project

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj`
- Create: `src/DesktopNode.Delivery.Tests/GlobalUsings.cs`
- Create: `src/DesktopNode.Delivery.Tests/DeliveryProjectContractTests.cs`
- Modify: `src/DesktopNode.sln`
- Modify: `src/DesktopNode.Verification.Tests/VerificationProjectContractTests.cs`

- [ ] **Step 1: Write the project-contract RED test**

Create the test project first, then add `DeliveryProjectContractTests` that requires target `net10.0`, exact four xUnit packages, no `ProjectReference`, no `OutputType=Exe`, and one solution entry. Before adding the solution entry, run:

```powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj --filter FullyQualifiedName~DeliveryProjectContractTests --nologo
```

Expected: test discovery succeeds and the solution-entry assertion fails.

- [ ] **Step 2: Use the exact project file**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
```

`GlobalUsings.cs` contains only:

```csharp
global using Xunit;
```

- [ ] **Step 3: Add the project to the solution through the .NET CLI**

```powershell
dotnet sln src/DesktopNode.sln add src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj --filter FullyQualifiedName~DeliveryProjectContractTests --nologo
```

Expected: focused tests PASS and the solution contains the project exactly once. Update the existing verification project-contract test to assert the new entry once without weakening its existing assertions.

- [ ] **Step 4: Commit**

```powershell
git add src/DesktopNode.Delivery.Tests src/DesktopNode.sln src/DesktopNode.Verification.Tests/VerificationProjectContractTests.cs
git diff --cached --check
git commit -m "test: add delivery contract assembly"
```

## Task 2: Implement fail-closed legacy parsing and metadata discovery

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Contracts/PcvLegacyContractAttribute.cs`
- Create: `src/DesktopNode.Delivery.Tests/Contracts/LegacyContractId.cs`
- Create: `src/DesktopNode.Delivery.Tests/Contracts/LegacyPesterContractParser.cs`
- Create: `src/DesktopNode.Delivery.Tests/Contracts/LegacyPesterContractParserTests.cs`
- Create: `src/DesktopNode.Delivery.Tests/Contracts/LegacyContractMetadataTests.cs`
- Create: `src/DesktopNode.Delivery.Tests/Infrastructure/RepositoryContractContext.cs`

- [ ] **Step 1: Write parser and ID RED tests**

Cover exact single/double-quoted `It` declarations, doubled single quotes, backtick escapes, line/block comments, single/double here-strings, duplicate literal names, dynamic/interpolated names, unsupported multiline declarations, unmatched quotes/comments/here-strings, Windows newlines, and deterministic IDs. Dynamic, duplicate, malformed, or multiline input must throw one of the fixed details `dynamic-name`, `duplicate-name`, `multiline-declaration`, `unmatched-quote`, `unmatched-comment`, or `unmatched-here-string` under error code `PCV_DELIVERY_LEGACY_PARSE_INVALID`, without absolute paths.

- [ ] **Step 2: Define the custom fact metadata**

Use this complete public shape inside the test assembly:

```csharp
namespace DesktopNode.Delivery.Tests.Contracts;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PcvLegacyContractAttribute : FactAttribute
{
    public PcvLegacyContractAttribute(
        string contractId,
        string legacyPath,
        int legacyOrdinal,
        string legacyName)
    {
        ContractId = contractId;
        LegacyPath = legacyPath;
        LegacyOrdinal = legacyOrdinal;
        LegacyName = legacyName;
    }

    public string ContractId { get; }
    public string LegacyPath { get; }
    public int LegacyOrdinal { get; }
    public string LegacyName { get; }
}
```

Every migrated fixture class also has exactly one `[Trait("Category", "Installer")]` or `[Trait("Category", "Delivery")]`. Infrastructure/negative tests use category `VerificationInfrastructure`, never Installer/Delivery.

- [ ] **Step 3: Implement a token-aware parser, not an `It` line regex**

Expose this fixed API:

```csharp
internal sealed record LegacyPesterContract(int Ordinal, string Name);

internal static class LegacyPesterContractParser
{
    internal static IReadOnlyList<LegacyPesterContract> Parse(
        string repositoryRelativePath,
        string source);
}
```

The scanner tracks normal code, line comment, block comment, single/double string, and single/double here-string states. It recognizes `It` only as a command token at statement position and rejects expandable names containing an unescaped `$`. It does not execute or import PowerShell.

- [ ] **Step 4: Implement repository containment**

`RepositoryContractContext` finds the root from `src/DesktopNode.sln`, accepts forward-slash relative paths only, rejects rooted/escaping/NUL/symlink targets, and exposes read-only UTF-8 text, `JsonDocument`, and `XDocument` loads. No write or process API is exposed.

- [ ] **Step 5: Verify parser GREEN against all 577 Installer/Packaging declarations**

```powershell
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj --filter FullyQualifiedName~LegacyPesterContractParser --nologo
```

Expected: unit fixtures PASS; repository inventory is exactly Installer `49`, Packaging `528`, total `577`; no dynamic/multiline declaration accepted.

- [ ] **Step 6: Commit**

```powershell
git add src/DesktopNode.Delivery.Tests/Contracts src/DesktopNode.Delivery.Tests/Infrastructure
git diff --cached --check
git commit -m "test: parse legacy Pester contracts without PowerShell"
```

## Task 3: Upgrade the migration ledger to strict v2

**Files:**
- Modify: `config/development-verification-migration-manifest.json`
- Modify: `config/development-verification-migration-manifest.schema.json`
- Modify: `web/scripts/verify-verification-migration-manifest.mjs`
- Create: `web/scripts/regenerate-verification-migration-manifest.mjs`
- Modify: `web/node-tests/verification-migration-manifest.test.mjs`
- Modify: `web/package.json`
- Create: `src/DesktopNode.Delivery.Tests/Contracts/MigrationManifestV2.cs`
- Create: `src/DesktopNode.Delivery.Tests/Contracts/MigrationManifestV2Tests.cs`
- Create: `docs/ga-ready/evidence/pester-free-installer-wave-c-2026-08-25.md`

- [ ] **Step 1: Write v2 RED tests in Node and C#**

Require exact top-level keys `contract`, `schema_version`, `inventory`, `entries`, `contracts`; file inventory `62/55/6/1`; contract inventory `627/528/49/50`; unique and ordered entries; unique `(legacy_path, legacy_ordinal)`, exact legacy names, unique non-null replacement IDs; and state/evidence coherence. Add negative cases for one missing contract, duplicate ordinal, duplicate ID, reordered name, wrong owner, mapped-with-null replacement, unmapped-with-replacement, pass-without-evidence, unknown prefix, extra property, and schema weakening.

- [ ] **Step 2: Freeze this v2 aggregate shape**

```json
{
  "contract": "pcv-development-verification-migration-manifest-v2",
  "schema_version": 2,
  "inventory": {
    "files": { "total": 62, "packaging": 55, "installer": 6, "web": 1 },
    "contracts": { "total": 627, "packaging": 528, "installer": 49, "web": 50 }
  },
  "entries": [],
  "contracts": []
}
```

File rows contain `legacy_path`, `domain`, `legacy_contract_count`, `parity_status`, `local_parity`, and `ci_parity`. Contract rows contain `legacy_path`, `legacy_ordinal`, `legacy_name`, `domain`, nullable `replacement_owner`, nullable `replacement_contract_id`, `parity_status`, `local_parity`, and `ci_parity`. Objects reject additional properties.

- [ ] **Step 3: Implement strict transition rules**

`unmapped` requires null replacement fields and both parity statuses pending/null evidence. `mapped` requires replacement fields and allows local pending or pass while CI remains pending. `dual-run-pass` requires local/CI pass with contained evidence locators. `cutover` additionally requires the Wave E cutover locator. Generic state changes may advance only in that order. The sole exception is Wave E's atomic `mapped -> cutover` persisted transition: it is accepted only when HEAD's parent is the recorded same-SHA shadow commit and the immutable shadow run proves the otherwise transient `dual-run-pass` prerequisite. No intermediate evidence commit may break that parent relation.

- [ ] **Step 4: Implement deterministic regeneration**

The Node generator reads all 62 legacy files, Web metadata, and syntactically scans `[PcvLegacyContract(...)]` attributes. It preserves valid parity/evidence for unchanged identities, gives new mappings `mapped` with pending parity, and gives undiscovered replacements `unmapped`. `--check` fails on byte drift; `--write` performs the sole mechanical rewrite. It never invokes PowerShell.

Add scripts:

```json
"check:verification-migration-manifest": "node scripts/regenerate-verification-migration-manifest.mjs --check && node scripts/verify-verification-migration-manifest.mjs --require-web-local-pass",
"generate:verification-migration-manifest": "node scripts/regenerate-verification-migration-manifest.mjs --write"
```

- [ ] **Step 5: Generate the honest Wave C starting state**

Run the writer once. Expected: 62 file rows and 627 contract rows; Web 50 mapped/local pass; Installer 49 and Packaging 528 unmapped; all CI pending. Then run Node/C# RED tests: the ledger shape passes, while the Installer completion assertion fails with `installer_unmapped=49`.

- [ ] **Step 6: Add the Wave C evidence skeleton**

Create a document with fixed inventory, per-file table, command/result fields initialized to `not-run`, CI parity pending, host mutation false, public signing false, external stable publication false. The word `PASS` is not used for a row until its command actually ran.

- [ ] **Step 7: Commit ledger v2 foundation**

```powershell
npm run check:verification-migration-manifest --prefix web
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj --filter FullyQualifiedName~MigrationManifestV2Tests --nologo
git add config/development-verification-migration-manifest.json config/development-verification-migration-manifest.schema.json web/scripts web/node-tests/verification-migration-manifest.test.mjs web/package.json docs/ga-ready/evidence/pester-free-installer-wave-c-2026-08-25.md src/DesktopNode.Delivery.Tests/Contracts
git diff --cached --check
git commit -m "test: establish verification migration ledger v2"
```

The completion assertion is allowed to remain RED only if it is a separately named Wave C completion test excluded from the foundation filter; the solution-wide test command must not contain an intentional failure.

## Contract-port translation rules for Tasks 4-9

- `Should -Be` / `-BeExactly` becomes typed `Assert.Equal`; normalize newline only where the Pester contract did.
- `Should -Match` becomes a compiled, culture-invariant `Regex` with the same semantic anchors.
- `Should -Contain` becomes collection membership after structured parse, not whole-file substring matching.
- JSON uses `JsonDocument`/`JsonNode` with exact property/type/additional-property checks.
- WiX/XML uses `XDocument`, namespaces, exact element/attribute cardinality, and typed values.
- Markdown tables/headings use a small parser that preserves row/heading order.
- PowerShell/C# source contracts use token/block helpers and positive plus mutated negative fixtures; do not reduce them to one marker string.
- `Should -Throw` becomes `Assert.Throws` or `Assert.ThrowsAsync` with stable public error details.
- Temp fixtures are created with `Path.Combine(Path.GetTempPath(), "pcv-delivery-tests", Guid.NewGuid().ToString("N"))` and deleted in `Dispose`; repository files are read-only.
- Each legacy `It` maps to one method named `ContractNNN`, with exact metadata name and deterministic ID. No `[Theory]` aggregation and no skipped facts.

Use this method pattern without changing field order:

```csharp
[PcvLegacyContract(
    "pcv.installer.desktop-node-installer-internal-trust.001",
    "packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.InternalTrust.Tests.ps1",
    1,
    "exposes a dry-run plan for CurrentUser signing and LocalMachine trust")]
public void Contract001()
{
    // The body contains the fully ported typed assertion for this legacy contract.
}
```

This concrete example is contract 001 of `PcvDesktopNodeInstaller.InternalTrust.Tests.ps1`; every later method copies its own exact parser output and the reflection inventory test rejects any mismatch.

## Task 4: Port InternalTrust 4/4

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerInternalTrustContractTests.cs`
- Create or modify: `src/DesktopNode.Delivery.Tests/Installer/InstallerNegativeParityTests.cs`
- Modify: migration manifest and Wave C evidence

- [ ] **Step 1: Add four metadata facts and confirm mapping RED**

Add exact contracts 001-004, run the generator in `--check` mode, and expect drift/missing mapping. Do not update the manifest first.

- [ ] **Step 2: Port all structured certificate/signing-policy assertions**

Preserve exact trust-mode, certificate-location, digest, and unsigned-dev boundary assertions. Add a negative fixture that changes one trust marker and prove the shared verifier rejects it.

- [ ] **Step 3: Regenerate, promote local evidence, and verify both oracles**

Run C# Installer filter, then exact legacy file under Pester 5.7.1. Expected replacement `4/4`, legacy `4/4`, failed/skipped/not-run `0`. Update the four contract rows and file row to mapped/local pass with the Wave C evidence locator; CI remains pending.

- [ ] **Step 4: Commit**

Commit message: `test: port installer internal trust contracts`.

## Task 5: Port Lifecycle 5/5

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerLifecycleContractTests.cs`
- Modify: `InstallerNegativeParityTests.cs`, manifest, Wave C evidence

- [ ] **Step 1: Add metadata and confirm RED before regeneration**
- [ ] **Step 2: Port install/update/repair/remove planning and rollback assertions with valid/invalid fixture pairs**
- [ ] **Step 3: Run replacement `5/5` and legacy `5/5`; require failed/skipped/not-run `0`; update local-pass evidence**
- [ ] **Step 4: Commit with `test: port installer lifecycle contracts`**

## Task 6: Port Plan 21/21

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerPlanContractTests.cs`
- Modify: `InstallerNegativeParityTests.cs`, manifest, Wave C evidence

- [ ] **Step 1: Add metadata and confirm 21-row RED before regeneration**
- [ ] **Step 2: Port every plan schema, property, ordering, path-containment, default, and fail-closed assertion**
- [ ] **Step 3: Mutate one required plan property, one ordering edge, and one escaping path in negative fixtures and require deterministic rejection**
- [ ] **Step 4: Run replacement `21/21` and legacy `21/21`; require failed/skipped/not-run `0`; update local-pass evidence**
- [ ] **Step 5: Commit with `test: port installer plan contracts`**

## Task 7: Port Signing 6/6

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerSigningContractTests.cs`
- Modify: `InstallerNegativeParityTests.cs`, manifest, Wave C evidence

- [ ] **Step 1: Add metadata and confirm six-row RED**
- [ ] **Step 2: Port signing-mode, digest algorithm, certificate reference, unsigned-dev, and non-publication assertions without reading a certificate store**
- [ ] **Step 3: Run replacement `6/6` and legacy `6/6`; require failed/skipped/not-run `0`; update local-pass evidence**
- [ ] **Step 4: Commit with `test: port installer signing contracts`**

## Task 8: Port WixSource 10/10

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerWixSourceContractTests.cs`
- Modify: `InstallerNegativeParityTests.cs`, manifest, Wave C evidence

- [ ] **Step 1: Add metadata and confirm ten-row RED**
- [ ] **Step 2: Port every WiX namespace, component, feature, service, firewall/custom-action, upgrade, and source-path assertion with `XDocument` cardinality checks**
- [ ] **Step 3: Add missing-element, duplicate-element, and wrong-namespace negative fixtures**
- [ ] **Step 4: Run replacement `10/10` and legacy `10/10`; require failed/skipped/not-run `0`; update local-pass evidence**
- [ ] **Step 5: Commit with `test: port installer WiX source contracts`**

## Task 9: Port Wrapper 3/3 and close Wave C locally

**Files:**
- Create: `src/DesktopNode.Delivery.Tests/Installer/PcvDesktopNodeInstallerWrapperContractTests.cs`
- Modify: `InstallerNegativeParityTests.cs`, manifest, Wave C evidence
- Modify: `config/development-verification-suites.json`
- Modify: `config/development-verification-suites.schema.json`
- Modify: `src/DesktopNode.Verification/VerificationCatalog.cs`
- Modify: related `DesktopNode.Verification.Tests` catalog/summary/architecture tests

- [ ] **Step 1: Add metadata and confirm three-row RED**
- [ ] **Step 2: Port wrapper argument-array, quoting, contained-path, exit propagation, and non-elevation assertions; never execute the wrapper**
- [ ] **Step 3: Run replacement `3/3` and legacy `3/3`; require failed/skipped/not-run `0`; update local-pass evidence**
- [ ] **Step 4: Advance only Installer catalog state**

Set `installer-contracts.migration_state` to `mapped`. Add `mapped` to the strict allowed state set/schema. Keep `delivery-contracts=wave-d-pending`, `evidence-check=wave-d-pending`, catalog activation non-active, and all CI parity pending.

- [ ] **Step 5: Run the exact manifest completion assertions**

Expected summary:

```text
files_total=62 contracts_total=627
web_mapped=50 web_local_pass=50 web_ci_pending=50
installer_mapped=49 installer_local_pass=49 installer_ci_pending=49
packaging_unmapped=528
missing=0 duplicate=0 order_drift=0
```

- [ ] **Step 6: Commit**

Commit message: `test: port installer wrapper and close Wave C mapping`.

## Task 10: Run Wave C completion verification and review

**Files:**
- Finalize: `docs/ga-ready/evidence/pester-free-installer-wave-c-2026-08-25.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`

- [ ] **Step 1: Run all replacement and repository gates**

```powershell
dotnet restore src/DesktopNode.sln
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --filter Category=Installer --no-restore --nologo
dotnet test src/DesktopNode.Delivery.Tests/DesktopNode.Delivery.Tests.csproj -c Release --no-restore --nologo
dotnet test src/DesktopNode.sln -c Release --no-restore --nologo
npm ci --prefix web
npm test --prefix web
npm run verify:parity --prefix web
npm run test:web-contracts --prefix web
npm run check:verification-migration-manifest --prefix web
git diff --check
```

Expected: Installer category exactly `49/49`, no skip; infrastructure and full solution PASS; Node/Web PASS.

- [ ] **Step 2: Run the complete Installer legacy reference once**

Use Pester 5.7.1 against `packaging/windows-desktop-node/installer/tests`. Expected: `49/49`, failed `0`, skipped `0`, not-run `0`. Record duration and summary digest. This is reference evidence, not final Required CI.

- [ ] **Step 3: Audit process and mutation boundaries**

Search new C#/Node/catalog executable fields for `pwsh`, `powershell`, `Invoke-Pester`, mutation commands, shell concatenation, `UseShellExecute=true`, and product `ProjectReference`. Expected executable occurrences `0`; documentation/reference locator occurrences are classified separately.

- [ ] **Step 4: Perform a fixed-diff review**

Compare from the sanitized public root seed through HEAD. Require missing/duplicate mapping `0`, Installer behavioral omissions `0`, P0/P1 findings `0`, and no required workflow change.

- [ ] **Step 5: Finalize and commit evidence**

Record observed test counts, legacy/replacement summary hashes, manifest counts, commit range, duration, review result, host mutation false, required-CI cutover false, public trusted signing false, and external stable publication false. Commit message: `docs: record Installer Wave C parity`.

- [ ] **Step 6: Push the draft cutover branch**

```text
git push origin codex/pester-free-verification-cutover
gh pr view --repo HardcoreMonk/purecvisor-desktop-node-public --json number,url,headRefName,baseRefName
gh pr checks <cutover-pr-number> --repo HardcoreMonk/purecvisor-desktop-node-public
```

Current protected jobs may pass, but do not mark the PR ready and do not change branch-protection contexts.

## Completion checkpoint

- Installer legacy/replacement parity is `49/49`, failed/skipped/not-run `0`.
- Ledger is strict v2 with 62 files and 627 contracts; missing/duplicate/order drift `0`.
- Web 50 and Installer 49 are mapped/local PASS/CI pending; Packaging 528 remains honestly unmapped.
- Required workflow and current protected check identities are unchanged.
- New replacement executable invocation of Pester/PowerShell/mutation tools is `0`.
- Stop and execute the Wave D plan. Do not claim Required CI Pester/PowerShell zero yet.
