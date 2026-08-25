# PureCVisor Desktop Node TUI Removal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Project instructions prohibit subagents unless the user explicitly requests them. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove the active .NET TUI, `pcvtui.exe`, and all current product/package contracts for it while retaining Web Console and PCVCLI functionality and preserving historical evidence.

**Architecture:** The final operator path has two clients: Web Console for interactive operations and PCVCLI for terminal/automation. Both continue to use the unchanged Local API, job runtime, auth boundary, and Hyper-V/Host providers. Product manifest schema 2 and the MSI payload declare only Host, Web, and CLI runtime assets; historical TUI evidence remains immutable.

**Tech Stack:** .NET 10/C#, PowerShell 7 and Windows PowerShell/Pester 5.7.1, WiX Toolset v4, TypeScript/Node 24, Windows MSI/SCM/Hyper-V admin-smoke tooling.

---

## Scope and execution boundary

This is one coordinated product-boundary change. Source removal, product manifest changes, MSI changes,
current documentation, and installed evidence cannot be shipped independently because an intermediate
state would either require a missing TUI binary or continue to advertise a removed surface.

Implement Tasks 1-5 in a fresh `codex/` worktree. Commit each task separately. Tasks 6-8 are explicit
package/host-mutation checkpoints and must run only after the repository gates are green. Do not edit
historical files under `docs/ga-ready/evidence/` except to create the new 0.42.63 evidence files.

## File map

### Delete

- `src/DesktopNode.Tui/DesktopNode.Tui.csproj`
- `src/DesktopNode.Tui/Program.cs`
- `src/DesktopNode.Tui/README.md`
- `src/DesktopNode.Tui/TuiApiClient.cs`
- `src/DesktopNode.Tui/TuiApiRoutes.cs`
- `src/DesktopNode.Tui/TuiApplication.cs`
- `src/DesktopNode.Tui/TuiApplicationResult.cs`
- `src/DesktopNode.Tui/TuiKeys.cs`
- `src/DesktopNode.Tui/TuiOptions.cs`
- `src/DesktopNode.Tui/TuiPoller.cs`
- `src/DesktopNode.Tui/TuiRenderer.cs`
- `src/DesktopNode.Tui/TuiState.cs`
- `src/DesktopNode.Tui/TuiTokenResolver.cs`
- `src/DesktopNode.Tui/TuiTransport.cs`
- `src/DesktopNode.Tui/TuiWidgets.cs`
- `src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj`
- `src/DesktopNode.Tui.Tests/TuiApiClientTests.cs`
- `src/DesktopNode.Tui.Tests/TuiApplicationTests.cs`
- `src/DesktopNode.Tui.Tests/TuiOptionsTests.cs`
- `src/DesktopNode.Tui.Tests/TuiRendererTests.cs`
- `src/DesktopNode.Tui.Tests/TuiStateTests.cs`
- `src/DesktopNode.Tui.Tests/TuiTokenResolverTests.cs`
- `packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1`
- `packaging/windows-desktop-node/tests/PcvInstalledNoVncTuiSmoke.Tests.ps1`

### Create

- `packaging/windows-desktop-node/tests/PcvInstalledNoVncSmoke.Tests.ps1`: noVNC-only smoke contract.
- `docs/adr/0011-cli-web-only-operator-surface.md`: applied operator-surface decision.
- `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`: repository-level closure.
- `docs/ga-ready/evidence/admin-smoke-package-2026-07-14-04263.md`: package result.
- `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-14-04263-hostmutation.md`: full-gate result.
- `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-14-04263.md`: Web/CLI installed result.
- `docs/ga-ready/evidence/manual-admin-campaign-2026-07-14-04262-04263.md`: update/rollback result or exact blocker.

### Modify: product and installer

- `src/DesktopNode.sln`
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- `packaging/windows-desktop-node/installer/build.ps1`
- `packaging/windows-desktop-node/installer/Product.wxs`
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`
- `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1`
- `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

### Modify: active documentation and control plane

- `README.md`
- `AGENTS.md`
- `packaging/windows-desktop-node/README.md`
- `packaging/windows-desktop-node/installer/README.md`
- `docs/USER_GUIDE.md`
- `docs/USER_FEATURE_USAGE_SPEC.md`
- `docs/CLI_COMMAND_USAGE.md`
- `docs/DEVELOPER_INDEX.md`
- `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- `docs/OPERATIONS_GUIDE.md`
- `docs/PUBLIC_RELEASE_BOUNDARY.md`
- `docs/ADR_INDEX.md`
- `docs/OPERATOR_SURFACE_TERMS.md`
- `docs/ga-ready/AUTOMATED_BATCH_JOB_CLASSIFICATION.md`
- `docs/ga-ready/CONTROL_PLANE_INDEX.md`
- `docs/ga-ready/EVIDENCE_INDEX.md`
- `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`
- `docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md`
- `docs/ga-ready/VERIFICATION_OWNERSHIP.md`

## Task 1: Make product manifest schema 2 Web/CLI-only

**Files:**

- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`

- [ ] **Step 1: Replace positive TUI expectations with failing Web/CLI-only expectations**

In `PcvDesktopNodeProduct.Plan.Tests.ps1`, remove the test named
`resolves product paths for the active .NET TUI executable` and add these assertions to
`returns product defaults`:

```powershell
$defaults.PSObject.Properties.Name | Should -Not -Contain 'tui_exe_name'
$paths = Resolve-PcvDesktopNodeProductPaths -ProductRoot $productRoot -DataRoot $dataRoot
$paths.PSObject.Properties.Name | Should -Not -Contain 'tui_exe'
```

In `PcvDesktopNodeProduct.Manifest.Tests.ps1`, replace
`records active .NET TUI metadata in product-manifest.json` with:

```powershell
It 'writes schema v2 with Web and CLI as the only operator clients' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeCliWebManifest'
    $dataRoot = Join-Path $TestDrive 'data-cli-web-manifest'

    $manifest = New-PcvDesktopNodeProductManifest `
        -Version '0.42.63-admin-smoke' `
        -SourceRoot $script:RepoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot

    $manifest.schema_version | Should -Be 2
    $manifest.PSObject.Properties.Name | Should -Contain 'cli'
    $manifest.PSObject.Properties.Name | Should -Not -Contain 'tui'
    $manifest.paths.PSObject.Properties.Name | Should -Not -Contain 'tui_exe'
    @(Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths) | Should -Contain 'pcvcli.exe'
    @(Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths) | Should -Not -Contain 'pcvtui.exe'
}
```

Change the runtime payload copy fixture so it creates Host, CLI, product wrapper, and Web assets but
does not create `pcvtui.exe`. Replace `blocks partial root runtime payload copy when the TUI executable
is missing` with a test that removes `pcvcli.exe` and still expects
`PCV_PRODUCT_RUNTIME_PAYLOAD_FILE_MISSING*pcvcli.exe*`.

In `PcvDesktopNodeProduct.Invoke.Tests.ps1`, remove
`blocks Update before mutation when the payload is missing the installed TUI`. Extend the successful
update test with:

```powershell
Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvtui.exe') | Should -BeFalse
```

- [ ] **Step 2: Run the focused tests and confirm RED**

Run:

```powershell
Invoke-Pester -Path @(
  'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1',
  'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1',
  'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1'
) -Output Detailed
```

Expected: FAIL because defaults, paths, manifest, and required payload still expose TUI.

- [ ] **Step 3: Remove TUI from defaults, paths, manifest, and required payload**

In `PcvDesktopNodeProduct.psm1`:

1. Delete `tui_exe_name = 'pcvtui.exe'`.
2. Delete the `tui_exe` path from `Resolve-PcvDesktopNodeProductPaths`.
3. Set generated manifest `schema_version = 2`.
4. Delete the entire `tui = [ordered]@{ ... }` block.
5. Delete `'pcvtui.exe'` from `Get-PcvDesktopNodeRequiredRuntimePayloadRelativePaths`.
6. Keep schema 1 and schema 2 read compatibility in the existing manifest reader; do not require a
   `tui` property when reading either schema.

The generated operator-client portion must be:

```powershell
schema_version = 2
cli = [ordered]@{
    mode = 'dotnet-local-api-client'
    command_name = 'pcvcli'
    executable_path = $paths.cli_exe
    default_owner = 'desktop-node-product-cli'
    token_sources = @('--token', '--token-file', '--token-env', '--protected-token-file')
}
```

- [ ] **Step 4: Run the focused tests and confirm GREEN**

Run the Step 2 command again.

Expected: all selected Pester tests PASS; no failure mentions a missing TUI payload.

- [ ] **Step 5: Commit the manifest change**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1
git commit -m "refactor: remove TUI from product manifest"
```

## Task 2: Remove TUI publishing and MSI payload

**Files:**

- Modify: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`
- Modify: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1`
- Modify: `packaging/windows-desktop-node/installer/build.ps1`
- Modify: `packaging/windows-desktop-node/installer/Product.wxs`

- [ ] **Step 1: Write failing installer boundary tests**

Delete TUI-path, TUI-publish, and positive TUI-component tests from
`PcvDesktopNodeInstaller.Plan.Tests.ps1`. Add:

```powershell
It 'builds a Web and CLI only payload' {
    $buildScript = Get-Content -Raw -LiteralPath $script:BuildScriptPath
    $productWxs = Get-Content -Raw -LiteralPath $script:ProductWxsPath

    $buildScript | Should -Not -Match 'DesktopNodeTuiPath'
    $buildScript | Should -Not -Match 'DesktopNode\.Tui'
    $buildScript | Should -Not -Match 'pcvtui\.exe'
    $buildScript | Should -Not -Match 'PCV_INSTALLER_TUI_'
    $productWxs | Should -Not -Match 'DesktopNodeTui'
    $productWxs | Should -Not -Match 'pcvtui\.exe'
    $productWxs | Should -Match 'DesktopNodeCli'
    $productWxs | Should -Match 'DesktopNodeWebApp'
}
```

Update the explicit-payload build fixture so it supplies only `DesktopNodeHostPath` and
`DesktopNodeCliPath`. Assert the staged payload contains Host and CLI and does not contain TUI:

```powershell
Test-Path -LiteralPath (Join-Path $payloadRoot 'DesktopNode.Host.exe') | Should -BeTrue
Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvcli.exe') | Should -BeTrue
Test-Path -LiteralPath (Join-Path $payloadRoot 'pcvtui.exe') | Should -BeFalse
$output.provenance.PSObject.Properties.Name | Should -Not -Contain 'tui'
```

Mirror the negative TUI assertions in `PcvDesktopNodeInstaller.WixSource.Tests.ps1`.

- [ ] **Step 2: Run installer tests and confirm RED**

```powershell
Invoke-Pester -Path @(
  'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1',
  'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1'
) -Output Detailed
```

Expected: FAIL because `build.ps1` and `Product.wxs` still publish/package TUI.

- [ ] **Step 3: Remove TUI from installer build and provenance**

In `installer/build.ps1`, delete:

- the `DesktopNodeTuiPath` parameter;
- `$tuiPublishRoot`, `$tuiProjectPath`, resolved TUI path/hash/source variables;
- explicit TUI path validation and all `PCV_INSTALLER_TUI_*` errors;
- the TUI publish command and `toolOutput.dotnet_tui_publish`;
- `tui_source`, `tui_path`, and `tui_sha256` plan/provenance fields;
- the `pcvtui.exe` payload copy entry.

Keep Host and CLI publish paths and Web asset staging unchanged.

In `Product.wxs`, delete only this component:

```xml
<Component Id="DesktopNodeTuiComponent" Directory="INSTALLFOLDER" Guid="{E0C310EE-6CE4-41F6-8E00-00ED5D0593E8}">
  <File Id="DesktopNodeTui" Source="$(var.PayloadRoot)\pcvtui.exe" KeyPath="yes" />
</Component>
```

Do not change the CLI-owned PATH environment component.

- [ ] **Step 4: Run installer tests and dry-run**

```powershell
Invoke-Pester -Path @(
  'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1',
  'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1'
) -Output Detailed

pwsh -NoProfile -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version '0.42.63-admin-smoke' `
  -OutputRoot 'artifacts/tui-removal-installer-dry-run-20260714' `
  -SigningMode AllowUnsignedDev `
  -SigningTrustModel LocalTest `
  -DryRun
```

Expected: tests PASS; dry-run JSON is parseable and contains no TUI path, hash, publish command, or
payload entry.

- [ ] **Step 5: Commit the installer change**

```powershell
git add packaging/windows-desktop-node/installer/build.ps1 packaging/windows-desktop-node/installer/Product.wxs packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.WixSource.Tests.ps1
git commit -m "refactor: remove TUI from Windows package"
```

## Task 3: Delete TUI projects and isolate noVNC verification

**Files:**

- Modify: `src/DesktopNode.sln`
- Delete: all files listed in the File map under `src/DesktopNode.Tui/` and
  `src/DesktopNode.Tui.Tests/`
- Delete: `packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1`
- Delete: `packaging/windows-desktop-node/tests/PcvInstalledNoVncTuiSmoke.Tests.ps1`
- Create: `packaging/windows-desktop-node/tests/PcvInstalledNoVncSmoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: Add a failing active-boundary test**

Add a new `It` block near the current 0.42.62 tests in
`PcvAdminSmokeEvidenceDocs.Tests.ps1`:

```powershell
It 'keeps the active product boundary Web and CLI only' {
    Test-Path -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Tui') | Should -BeFalse
    Test-Path -LiteralPath (Join-Path $script:RepoRoot 'src/DesktopNode.Tui.Tests') | Should -BeFalse
    Test-Path -LiteralPath (Join-Path $script:RepoRoot 'packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1') | Should -BeFalse

    $solution = Get-RepoText -RelativePath 'src/DesktopNode.sln'
    $solution | Should -Not -Match 'DesktopNode\.Tui'
    $solution | Should -Not -Match 'F1471821-A992-4D9C-856E-CC104CF12239'
    $solution | Should -Not -Match '51F1CC27-5E90-4ACB-91CE-F2137119B66E'
}
```

- [ ] **Step 2: Run the boundary test and confirm RED**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -FullName '*active product boundary Web and CLI only*' -Output Detailed
```

Expected: FAIL because both projects and the smoke tool still exist.

- [ ] **Step 3: Remove solution entries and delete active TUI files**

Apply a patch to `src/DesktopNode.sln` that removes:

- project entries for GUIDs `{F1471821-A992-4D9C-856E-CC104CF12239}` and
  `{51F1CC27-5E90-4ACB-91CE-F2137119B66E}`;
- configuration mappings for the first GUID at lines 190-201;
- configuration mappings for the second GUID at lines 202-213.

Use `apply_patch` file deletions for every TUI production/test file and the installed TUI smoke tool
listed in the File map. Do not move them under `archive/`.

- [ ] **Step 4: Replace the mixed noVNC/TUI test with a noVNC-only test**

Create `PcvInstalledNoVncSmoke.Tests.ps1` with the existing assertions for
`Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1`. Exclude all references to
`Invoke-PcvInstalledTuiOperatorSmoke.ps1`, `pcvtui.exe`, and `PCV_TUI_*`. The central contract is:

```powershell
Describe 'Installed target-backed noVNC smoke contract' {
    It 'keeps noVNC target validation and secret redaction independent of TUI' {
        $scriptPath = Join-Path $PSScriptRoot '../tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1'
        $text = Get-Content -Raw -LiteralPath $scriptPath

        $text | Should -Match 'novnc'
        $text | Should -Match 'target'
        $text | Should -Match 'redact|secret|token'
        $text | Should -Not -Match 'pcvtui\.exe|PCV_TUI_'
    }
}
```

Delete the old mixed test after the noVNC assertions have been retained.

- [ ] **Step 5: Run source and smoke tests**

```powershell
dotnet test src/DesktopNode.sln -c Release
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvInstalledNoVncSmoke.Tests.ps1 -Output Detailed
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -FullName '*active product boundary Web and CLI only*' -Output Detailed
```

Expected: .NET solution passes without TUI projects; both Pester selections PASS.

- [ ] **Step 6: Commit the source removal**

```powershell
git add src/DesktopNode.sln src/DesktopNode.Tui src/DesktopNode.Tui.Tests packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1 packaging/windows-desktop-node/tests/PcvInstalledNoVncTuiSmoke.Tests.ps1 packaging/windows-desktop-node/tests/PcvInstalledNoVncSmoke.Tests.ps1 packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "refactor: remove the Desktop Node TUI"
```

## Task 4: Establish the CLI/Web-only decision in current docs

**Files:**

- Create: `docs/adr/0011-cli-web-only-operator-surface.md`
- Create: `docs/ga-ready/evidence/tui-removal-cli-web-only-code-level-2026-07-14.md`
- Modify: all active documentation and control-plane files listed in the File map
- Modify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`

- [ ] **Step 1: Add failing current-decision assertions**

Extend the boundary test from Task 3:

```powershell
$adr = Get-RepoText -RelativePath 'docs/adr/0011-cli-web-only-operator-surface.md'
$ledger = Get-RepoText -RelativePath 'docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md'
$usage = Get-RepoText -RelativePath 'docs/USER_FEATURE_USAGE_SPEC.md'
$guide = Get-RepoText -RelativePath 'docs/USER_GUIDE.md'

$adr | Should -Match 'DESKTOP_NODE_OPERATOR_SURFACE_DECISION:\s*cli-web-only'
$adr | Should -Match 'DESKTOP_NODE_TUI_DECISION:\s*removed'
$ledger | Should -Match 'active_operator_surface_decision:\s*`cli-web-only`'
$ledger | Should -Match 'tui_product_status:\s*`removed-from-active-product`'
$usage | Should -Not -Match '(?m)^\| TUI\s'
$guide | Should -Not -Match '(?m)^## 터미널 TUI|pcvtui\.exe'
```

Run the focused test and expect failure because ADR-0011 and the new metadata do not exist.

- [ ] **Step 2: Create ADR-0011**

Write `docs/adr/0011-cli-web-only-operator-surface.md` with:

```markdown
# ADR-0011: CLI/Web 전용 Operator Surface

- 상태: 적용 중
- 날짜: 2026-07-14
- 결정 마커:
  - `DESKTOP_NODE_OPERATOR_SURFACE_DECISION: cli-web-only`
  - `DESKTOP_NODE_TUI_DECISION: removed`

PureCVisor Desktop Node의 active 사용자 표면은 Web Console과 PCVCLI다.
TUI source, package payload, smoke와 current 문서 계약을 제거한다.
Local API와 backend route는 유지하며 historical TUI evidence는 predecessor로 보존한다.
```

Include consequences for manifest schema 2, MSI upgrade cleanup, no compatibility shim, and
0.42.63 installed verification.

- [ ] **Step 3: Rewrite current user and operator documentation**

Apply these exact semantic changes:

- `USER_GUIDE.md`: delete the Terminal UI section; state Web Console is the primary interactive
  surface and PCVCLI is terminal/automation.
- `USER_FEATURE_USAGE_SPEC.md`: remove the TUI entry point and TUI matrix column; retain each backend
  function in Web and/or CLI columns.
- `CLI_COMMAND_USAGE.md`: remove claims of TUI parity but do not change CLI commands.
- `OPERATOR_SURFACE_TERMS.md`: define only Web Console and PCVCLI as current surfaces.
- `README.md`, packaging READMEs, `AGENTS.md`, `DEVELOPER_INDEX.md`, verification/operations/public
  boundary docs: replace active `Web/TUI/CLI` wording with `Web/CLI`.

Do not rewrite dated historical paragraphs. Prefix still-linked TUI items with `historical` or
`predecessor` when needed to prevent a current claim.

- [ ] **Step 4: Update current control-plane metadata without falsifying installed state**

Add to `CURRENT_EVIDENCE_LEDGER.md`:

```markdown
active_operator_surface_decision: `cli-web-only`
tui_product_status: `removed-from-active-product`
tui_removal_source_status: `code-level-pass-awaiting-0.42.63-installed-promotion`
```

Keep the installed `0.42.62` current-card row as a dated predecessor that truthfully recorded TUI.
Do not claim 0.42.63 package/full-gate/current-card PASS before Tasks 6-7 run.

Update `ADR_INDEX.md`, `CONTROL_PLANE_INDEX.md`, `EVIDENCE_INDEX.md`, distribution matrix,
verification ownership, and automated classification with ADR-0011 and the code-level evidence.

- [ ] **Step 5: Create code-level evidence**

Write `tui-removal-cli-web-only-code-level-2026-07-14.md` with:

```markdown
evidence_id: `tui-removal-cli-web-only-code-level-2026-07-14`
result: `PASS`
host_mutation_performed: `false`
operator_surface_decision: `cli-web-only`
tui_product_status: `removed-from-active-product`
installed_promotion_status: `pending-0.42.63-admin-smoke`
```

Record the exact repository test counts observed in Tasks 1-5; never pre-fill expected counts as
observed counts.

- [ ] **Step 6: Run documentation and boundary tests**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
git diff --check
```

Expected: all evidence tests PASS. Historical evidence assertions may still contain TUI strings, but
no assertion may require deleted active source such as `src/DesktopNode.Tui/TuiRenderer.cs`.

- [ ] **Step 7: Commit the product-boundary documentation**

```powershell
git add README.md AGENTS.md packaging/windows-desktop-node/README.md packaging/windows-desktop-node/installer/README.md docs packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1
git commit -m "docs: adopt CLI and Web only operator surface"
```

## Task 5: Run the complete non-mutating repository gate

**Files:**

- Modify only files with failures directly caused by the TUI removal.

- [ ] **Step 1: Run .NET tests**

```powershell
dotnet test src/DesktopNode.sln -c Release
```

Expected: all remaining .NET tests PASS and the output contains no TUI project build.

- [ ] **Step 2: Run Web tests and parity**

```powershell
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
```

Expected: all commands exit 0; served Web assets remain unchanged by the TUI deletion.

- [ ] **Step 3: Run all Pester suites**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests -Output Detailed
Invoke-Pester -Path @(
  'packaging/windows-desktop-node/installer/tests',
  'web/tests'
) -Output Detailed
```

Expected: failed count 0.

- [ ] **Step 4: Run active-boundary searches**

```powershell
$activeCodeHits = rg -n -i 'pcvtui|DesktopNodeTui|DesktopNode\.Tui|PCV_TUI_' `
  src `
  packaging/windows-desktop-node `
  --glob '!tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' `
  --glob '!README.md'

if ($LASTEXITCODE -eq 0) { $activeCodeHits; throw 'Active TUI code/package reference remains.' }
if ($LASTEXITCODE -ne 1) { throw "rg failed with exit code $LASTEXITCODE" }

$solution = Get-Content -Raw -LiteralPath 'src/DesktopNode.sln'
if ($solution -match 'DesktopNode\.Tui|F1471821|51F1CC27') { throw 'TUI remains in solution.' }
```

Expected: no active code/package hits and no solution match. Historical docs/evidence are not part of
this negative code boundary.

- [ ] **Step 5: Verify diff integrity and commit direct corrections**

```powershell
git diff --check
git status --short
```

Expected: the working tree is clean after the Task 1-4 commits. If any command fails, return to the
task that owns the failing file, correct it there, rerun that task's focused test, amend that task's
commit, and repeat Task 5 once. Do not create a catch-all correction commit and do not stage unrelated
files.

## Task 6: Build and inspect the 0.42.63 package

**Files:**

- Create: `docs/ga-ready/evidence/admin-smoke-package-2026-07-14-04263.md`
- Artifact: `artifacts/admin-smoke-package-20260714-04263/**`
- Modify: current evidence indexes only after PASS.

- [ ] **Step 1: Build the internal admin-smoke package**

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/installer/build.ps1 `
  -Version '0.42.63-admin-smoke' `
  -OutputRoot 'artifacts/admin-smoke-package-20260714-04263' `
  -SigningMode AllowUnsignedDev `
  -SigningTrustModel LocalTest
```

Expected: exit 0 and a generated MSI, provenance JSON, publication JSON, SHA-256 sidecar, and payload.

- [ ] **Step 2: Assert the clean payload has no TUI**

```powershell
$root = 'artifacts/admin-smoke-package-20260714-04263'
$payload = Join-Path $root 'payload'
$manifest = Get-Content -Raw -LiteralPath (Join-Path $payload 'product-manifest.json') | ConvertFrom-Json

Test-Path -LiteralPath (Join-Path $payload 'DesktopNode.Host.exe') | Should -BeTrue
Test-Path -LiteralPath (Join-Path $payload 'pcvcli.exe') | Should -BeTrue
Test-Path -LiteralPath (Join-Path $payload 'pcvtui.exe') | Should -BeFalse
$manifest.schema_version | Should -Be 2
$manifest.PSObject.Properties.Name | Should -Not -Contain 'tui'
$manifest.paths.PSObject.Properties.Name | Should -Not -Contain 'tui_exe'
```

Expected: every assertion passes.

- [ ] **Step 3: Record package hashes and evidence**

Read actual values from the generated provenance and SHA-256 files. Create the package evidence with
`PACKAGE_BUILD_PASS`, `0.42.63-admin-smoke`, `AllowUnsignedDev`, `LocalTest`, Host/CLI hashes,
schema 2, and `pcvtui_present: false`. Do not include a TUI hash field.

- [ ] **Step 4: Run package evidence guards and commit**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
git diff --check
git add docs/ga-ready/evidence/admin-smoke-package-2026-07-14-04263.md docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/CONTROL_PLANE_INDEX.md docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md
git commit -m "docs: record 0.42.63 CLI Web package"
```

Expected: evidence guards PASS. Promote package-build status only; do not claim full-gate/current-card
PASS yet.

## Task 7: Run installed Web/CLI and full-admin gates

**Files:**

- Create: `docs/ga-ready/evidence/full-admin-host-mutation-gate-2026-07-14-04263-hostmutation.md`
- Create: `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-07-14-04263.md`
- Artifact: `artifacts/routeparity-service-msi-hyperv-batch-profile-20260714-04263/**`
- Artifact: `artifacts/os-mutation-gates-batch-profile-20260714-04263/**`
- Artifact: `artifacts/installed-operator-surface-current-card-20260714-04263/**`

- [ ] **Step 1: Run route-parity Service/MSI/Hyper-V smoke from an elevated terminal**

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvRouteParityMutationSmoke.ps1 `
  -Version '0.42.63-admin-smoke' `
  -IsoPath 'D:\Downloads\Rocky-10.1-x86_64-minimal.iso' `
  -ArtifactRoot 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260714-04263' `
  -BatchEvidenceRoot 'artifacts/batch-runs/full-admin-host-mutation-gate-20260714-04263'
```

Expected: summary `ok=true`; MSI lifecycle and Hyper-V route steps PASS; service final state is
Running; boot time is unchanged; no test VM remains.

- [ ] **Step 2: Verify installed CLI/Web and TUI absence before OS mutation**

```powershell
$productRoot = 'C:\Program Files\PureCVisor\DesktopNode'
$token = Join-Path $env:ProgramData 'PureCVisor\desktop-node\api-token.dpapi.json'

Test-Path -LiteralPath (Join-Path $productRoot 'pcvtui.exe') | Should -BeFalse
& (Join-Path $productRoot 'pcvcli.exe') --protected-token-file $token host status
& (Join-Path $productRoot 'pcvcli.exe') --protected-token-file $token runtime policy
$web = Invoke-WebRequest -UseBasicParsing -Uri 'http://127.0.0.1/' -TimeoutSec 10
$config = Invoke-WebRequest -UseBasicParsing -Uri 'http://127.0.0.1/pcv-config.js' -TimeoutSec 10
$web.StatusCode | Should -Be 200
$config.StatusCode | Should -Be 200
```

Expected: TUI absent; both CLI commands exit 0; Web responses are 200.

- [ ] **Step 3: Run the OS mutation gate with the actual preferred LAN address**

```powershell
$lanAddress = Get-NetIPAddress -AddressFamily IPv4 -AddressState Preferred |
  Where-Object {
    $_.IPAddress -notlike '127.*' -and
    $_.IPAddress -notlike '169.254.*' -and
    $_.InterfaceAlias -notmatch 'vEthernet|Loopback'
  } |
  Sort-Object InterfaceMetric |
  Select-Object -First 1 -ExpandProperty IPAddress

if ([string]::IsNullOrWhiteSpace($lanAddress)) { throw 'No preferred physical LAN IPv4 address.' }
$lanPrefix = "http://$lanAddress`:7777/"

pwsh -NoProfile -File packaging/windows-desktop-node/tools/Invoke-PcvOsMutationGateSmoke.ps1 `
  -Version '0.42.63-admin-smoke' `
  -RouteParityArtifactRoot 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260714-04263' `
  -ArtifactRoot 'artifacts/os-mutation-gates-batch-profile-20260714-04263' `
  -LanPrefix $lanPrefix
```

Expected: summary `ok=true`; firewall, LAN, Event Log, trust-store, and final-state checks PASS.

- [ ] **Step 4: Create the Web/CLI installed current-card artifact**

Write `artifacts/installed-operator-surface-current-card-20260714-04263/summary.json` from the actual
Step 2 results with this contract:

```json
{
  "schema_version": 1,
  "version": "0.42.63-admin-smoke",
  "operator_surfaces": ["web", "cli"],
  "tui_present": false,
  "service_state": "Running",
  "service_start_mode": "Automatic",
  "secret_observed": false,
  "ok": true
}
```

Add exact CLI command results, Web HTTP statuses, network inventory topology, and artifact paths; do
not add a TUI count or TUI smoke field.

- [ ] **Step 5: Write evidence and promote the installed anchor**

Create the full-gate and installed-current-card evidence from actual summaries and hashes. Update
`AGENTS.md`, verification policy, current ledger, control-plane/evidence indexes, README current blocks,
and packaging README so 0.42.63 is the package/full-gate/current-card anchor. State that 0.42.62 is the
historical TUI predecessor.

- [ ] **Step 6: Run guards and commit**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
dotnet test src/DesktopNode.sln -c Release
git diff --check
git add AGENTS.md README.md packaging/windows-desktop-node/README.md docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/PUBLIC_RELEASE_BOUNDARY.md docs/ga-ready
git commit -m "docs: promote 0.42.63 CLI Web installed anchor"
```

Expected: all commands PASS and current documents agree on 0.42.63.

## Task 8: Close or honestly block the 0.42.62 to 0.42.63 package pair

**Files:**

- Create: `docs/ga-ready/evidence/manual-admin-campaign-2026-07-14-04262-04263.md`
- Artifact: `artifacts/manual-admin-campaign-20260714-04262-04263/**`
- Modify: current ledger/indexes only according to PASS or blocker result.

- [ ] **Step 1: Generate readiness without mutation**

```powershell
pwsh -NoProfile -File packaging/windows-desktop-node/tools/New-PcvManualAdminRebaselineReadiness.ps1 `
  -ArtifactRoot 'artifacts/manual-admin-campaign-20260714-04262-04263/readiness' `
  -BaselineVersion '0.42.62-admin-smoke' `
  -TargetVersion '0.42.63-admin-smoke' `
  -RouteParityArtifactRoot 'artifacts/routeparity-service-msi-hyperv-batch-profile-20260713-04262' `
  -TargetPackageArtifactRoot 'artifacts/admin-smoke-package-20260714-04263' `
  -PlanOnly
```

Expected before Task 7 mutation: `ready-current-baseline-target-package-pair` only when the installed
manifest is 0.42.62 and both package roots are complete. If the installed host is already 0.42.63,
record `blocked-by-installed-baseline-version-mismatch` and do not fabricate a PASS.

- [ ] **Step 2: Classify the deterministic post-install baseline boundary**

Task 7 installs `0.42.63-admin-smoke` before this readiness check. Therefore the expected result on the
same host is `blocked-by-installed-baseline-version-mismatch`: the requested baseline is 0.42.62 but
the installed manifest is 0.42.63. Do not downshift this host merely to manufacture a campaign PASS.

The repository does not contain tracked executable runners for the Burn and MSIX lifecycle entries;
the last closed campaign consumed externally operated summaries. Consequently, do not invent runner
commands or synthesize their JSON. A future dedicated 0.42.62 baseline host plus the approved external
Burn/MSIX runners is required to close the six-runner descriptor.

- [ ] **Step 3: Write the canonical blocker result**

Create `manual-admin-campaign-2026-07-14-04262-04263.md` with:

```markdown
result: `BLOCKED`
blocker: `blocked-by-installed-baseline-version-mismatch`
baseline_version: `0.42.62-admin-smoke`
target_version: `0.42.63-admin-smoke`
current_closed_pair: `0.42.58-admin-smoke -> 0.42.59-admin-smoke`
host_mutation_performed_by_readiness: `false`
next_action: `run-on-dedicated-0.42.62-baseline-host-with-approved-burn-msix-runners`
```

Link the readiness `summary.json`. Keep `0.42.58 -> 0.42.59` as the current closed pair and add the new
blocker as follow-up evidence; do not create a closed descriptor.

- [ ] **Step 4: Final verification and commit**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
dotnet test src/DesktopNode.sln -c Release
npm test --prefix web
git diff --check
git status --short
```

Expected: all repository gates PASS; current manual-admin metadata matches the actual campaign result.

Stage only the new campaign evidence and the current documents it changes, then commit:

```powershell
git add docs/ga-ready/evidence/manual-admin-campaign-2026-07-14-04262-04263.md docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md docs/ga-ready/CONTROL_PLANE_INDEX.md docs/ga-ready/EVIDENCE_INDEX.md AGENTS.md README.md packaging/windows-desktop-node/README.md
git commit -m "docs: record 0.42.62 to 0.42.63 campaign"
```

## Final completion checklist

- [ ] `src/DesktopNode.Tui/` and `src/DesktopNode.Tui.Tests/` do not exist.
- [ ] `DesktopNode.sln` contains no TUI project/GUID.
- [ ] Product manifest generator emits schema 2 with CLI and no TUI field.
- [ ] Runtime payload and MSI contain no `pcvtui.exe`.
- [ ] Installer build/provenance exposes no TUI input, output, or error contract.
- [ ] Web Console and PCVCLI retain existing API functionality.
- [ ] noVNC verification is independent of TUI.
- [ ] Current user docs advertise only Web and CLI.
- [ ] Historical TUI evidence remains intact and is labeled predecessor when indexed.
- [ ] Non-mutating .NET, Web, packaging, installer, and evidence gates PASS.
- [ ] 0.42.63 package evidence proves `pcvtui_present=false`.
- [ ] Installed 0.42.63 current-card proves Web/CLI PASS and TUI absence.
- [ ] Full admin host mutation gate PASSes or records an exact non-PASS boundary without promotion.
- [ ] Manual-admin package pair closes or records an honest blocker without replacing the last PASS.
