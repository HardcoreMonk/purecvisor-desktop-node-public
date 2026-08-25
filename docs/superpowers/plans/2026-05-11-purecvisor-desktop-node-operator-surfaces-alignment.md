# Operator Surfaces Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align Web Console, TUI, CLI, and user docs around one Windows operator journey without changing product scope.

**Architecture:** Add a small shared operator vocabulary document, then add static/test guards that Web/TUI/CLI/docs keep the same route names, destructive boundaries, diagnostics wording, and internal distribution boundary.

**Tech Stack:** Markdown, TypeScript static web app, C# CLI/TUI tests, Pester web static tests, npm parity fixture.

---

## File Structure

- Create: `docs/OPERATOR_SURFACE_TERMS.md`
- Modify: `docs/USER_GUIDE.md`
- Modify: `docs/CLI_COMMAND_USAGE.md`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`
- Modify: `src/DesktopNode.Tui.Tests/TuiRendererTests.cs`
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

## Task 1: Create Shared Operator Terms

**Files:**
- Create: `docs/OPERATOR_SURFACE_TERMS.md`

- [ ] **Step 1: Write operator terms document**

Create `docs/OPERATOR_SURFACE_TERMS.md`:

```markdown
# Desktop Node Operator Surface Terms

## Product

- Product name: PureCVisor Desktop Node
- Deployment boundary: internal private network only
- Public release boundary: public trusted signing, winget public submission, and external stable publication are out of scope

## Access

- Login: account/RBAC/JWT session when configured
- Fallback auth: bearer token remains authoritative when no account is configured
- Secret rule: token, password, JWT, refresh token, signing key, private key, and PFX password values are never displayed

## VM Operations

- Inventory: VM list and VM detail
- Lifecycle actions: create, start, shutdown, poweroff, restart, delete
- Delete boundary: only PureCVisor-managed VMs can be deleted
- Destructive action rule: UI and CLI require explicit confirmation before destructive mutation

## Diagnostics

- Diagnostic bundle: server-side support bundle with redaction
- Download: user-triggered bundle download
- Evidence handoff: operator-facing summary points to sanitized artifact roots, not raw secret values

## Release and Update

- Update: internal package apply through validated source or catalog
- Rollback: restore previous product root with transaction journal evidence
- Distribution: internal signed or AllowUnsignedDev admin-smoke evidence, never public distribution claim
```

- [ ] **Step 2: Run whitespace check**

```powershell
git diff --check -- docs/OPERATOR_SURFACE_TERMS.md
```

Expected: no output.

## Task 2: Add CLI Vocabulary Guard

**Files:**
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`

- [ ] **Step 1: Add CLI terms test**

Append this test:

```csharp
[Fact]
public void UsageUsesSharedOperatorTerms()
{
    var error = Assert.Throws<ArgumentException>(() => DesktopNodeCliCommandCatalog.CreateRequest(["vm", "delete", "demo"]));

    Assert.Contains("VM delete requires explicit confirmation", error.Message, StringComparison.Ordinal);
    Assert.Contains("vm delete <vm> --yes", error.Message, StringComparison.Ordinal);
    Assert.DoesNotContain("public release", error.Message, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Run CLI tests**

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --filter UsageUsesSharedOperatorTerms --no-restore
```

Expected: PASS.

## Task 3: Add TUI Vocabulary Guard

**Files:**
- Modify: `src/DesktopNode.Tui.Tests/TuiRendererTests.cs`

- [ ] **Step 1: Add TUI terms test**

Append this test:

```csharp
[Fact]
public void RendererUsesSharedOperatorTermsForDiagnosticsAndDeletion()
{
    var state = TuiState.Empty
        .SwitchTab(TuiTab.Diag)
        .ApplyRows(TuiTab.Diag, new[]
        {
            "Diagnostic bundle | redacted | ready",
            "Only PureCVisor-managed VMs can be deleted."
        })
        .ApplySnapshot(
            TuiTab.Diag,
            TuiRouteSnapshot.Success("{\"ok\":true,\"data\":{\"bundles\":[]}}", FixedNow));

    var frame = TuiRenderer.Render(state, new TuiRenderOptions(Width: 100, Height: 32)).Text;

    Assert.Contains("DIAG", frame, StringComparison.Ordinal);
    Assert.Contains("PureCVisor-managed VMs", frame, StringComparison.Ordinal);
    Assert.DoesNotContain("public trusted signing", frame, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Run TUI renderer tests**

```powershell
dotnet test src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj --filter RendererUsesSharedOperatorTermsForDiagnosticsAndDeletion --no-restore
```

Expected: PASS.

## Task 4: Add Web Static Vocabulary Guard

**Files:**
- Modify: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

- [ ] **Step 1: Add web static test**

Add this Pester test in the existing web static describe block:

```powershell
It "keeps operator surface terms aligned with internal distribution boundary" {
    $app = Get-Content -Raw -Path "web/src/served-app.ts"
    $html = Get-Content -Raw -Path "web/index.html"
    $terms = Get-Content -Raw -Path "docs/OPERATOR_SURFACE_TERMS.md"

    ($app + $html + $terms) | Should -Match "internal private network|Internal"
    ($app + $terms) | Should -Match "Diagnostic bundle|diagnostic bundle"
    ($app + $terms) | Should -Match "PureCVisor-managed VMs"
    ($app + $html) | Should -Not -Match "public trusted signing.*available|winget submission.*ready|external stable publication.*ready"
}
```

- [ ] **Step 2: Run web Pester**

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests/PcvDesktopWeb.Static.Tests.ps1' -Output Detailed"
```

Expected: PASS.

## Task 5: Link Terms from User Docs

**Files:**
- Modify: `docs/USER_GUIDE.md`
- Modify: `docs/CLI_COMMAND_USAGE.md`

- [ ] **Step 1: Add terms link to both docs**

Add this sentence near the top of both files:

```markdown
Operator-facing terms and product boundary wording are centralized in `docs/OPERATOR_SURFACE_TERMS.md`.
```

- [ ] **Step 2: Run surface verification**

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore
dotnet test src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj --no-restore
pwsh -NoProfile -Command "Invoke-Pester -Path 'web/tests' -Output Detailed"
npm test --prefix web
npm run verify:parity --prefix web
git diff --check
```

Expected: PASS and no whitespace errors.

- [ ] **Step 3: Commit**

```powershell
git add docs/OPERATOR_SURFACE_TERMS.md docs/USER_GUIDE.md docs/CLI_COMMAND_USAGE.md src/DesktopNode.Cli.Tests src/DesktopNode.Tui.Tests web/tests
git commit -m "docs: align operator surface vocabulary"
```
