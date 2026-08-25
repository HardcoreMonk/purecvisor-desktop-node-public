# PureCVisor Desktop Node Active .NET CLI Implementation Plan

**Status:** Implemented on 2026-05-09 as the first active .NET CLI slice.

**Goal:** Add an active product-owned .NET CLI that borrows the `purecvisor-single` `pcvctl` command-table/transport/formatter model without reintroducing archived PowerShell helper runtime paths.

**Architecture:** Create `pcvcli.exe` from the `src/DesktopNode.Cli` project as a separate thin Local API client. Command parsing maps `object action` commands into HTTP method/path/body requests, token resolution stays in the CLI process, transport is injectable for tests, and output formatting is separate from routing. MSI payload staging, product manifest paths, and update payload validation track the same `pcvcli.exe` file contract.

**Tech Stack:** C#/.NET `net10.0-windows`, xUnit, `System.Text.Json`, `HttpClient`, DPAPI `ProtectedData`.

---

### Task 1: CLI Project and RED Routing Tests

**Files:**
- Create: `src/DesktopNode.Cli/DesktopNode.Cli.csproj`
- Create: `src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj`
- Create: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`

- [x] **Step 1: Write failing tests for command routing**

Add xUnit tests that assert `host status`, `runtime policy`, `ops summary`, `network inventory`, `vm list|get|create|delete --yes`, checkpoint commands, job commands, and diagnostics bundle commands map to the expected HTTP request contract.

- [x] **Step 2: Run RED verification**

Run: `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore`

Expected: FAIL because the CLI command catalog does not exist yet.

### Task 2: Minimal Command Catalog

**Files:**
- Create: `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs`
- Create: `src/DesktopNode.Cli/DesktopNodeCliRequest.cs`

- [x] **Step 1: Implement catalog routing**

Implement route segment escaping, named option parsing, integer validation, VM delete `--yes` gate, job list `--limit`/`--offset` query construction, and diagnostics download output path parsing.

- [x] **Step 2: Run GREEN verification**

Run: `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore`

Expected: PASS for catalog tests.

### Task 3: Token Options, Transport, Formatter, and Program Entrypoint

**Files:**
- Create: `src/DesktopNode.Cli/DesktopNodeCliOptions.cs`
- Create: `src/DesktopNode.Cli/DesktopNodeCliTokenResolver.cs`
- Create: `src/DesktopNode.Cli/DesktopNodeCliTransport.cs`
- Create: `src/DesktopNode.Cli/DesktopNodeCliFormatter.cs`
- Create: `src/DesktopNode.Cli/DesktopNodeCliApplication.cs`
- Create: `src/DesktopNode.Cli/Program.cs`
- Create: `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`
- Create: `src/DesktopNode.Cli.Tests/DesktopNodeCliTokenResolverTests.cs`

- [x] **Step 1: Write failing orchestration/token tests**

Assert inline `--token`, `--token-file`, `--token-env`, and `--protected-token-file` resolve correctly, ambiguous token sources fail before transport execution, JSON output preserves API response JSON, plain output summarizes success/failure, and verbose redaction does not expose token values.

- [x] **Step 2: Implement minimal orchestration**

Use an injectable transport for tests and an `HttpClient` transport for production. Add `Authorization: Bearer <token>` only after token resolution and never write token values to stdout/stderr.

- [x] **Step 3: Run GREEN verification**

Run: `dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore`

Expected: PASS.

### Task 4: Solution Wiring and Verification

**Files:**
- Modify: `src/DesktopNode.sln`

- [x] **Step 1: Add CLI projects to solution**

Run: `dotnet sln src\DesktopNode.sln add src\DesktopNode.Cli\DesktopNode.Cli.csproj src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj`

- [x] **Step 2: Run focused and solution tests**

Run:

```powershell
dotnet test src\DesktopNode.Cli.Tests\DesktopNode.Cli.Tests.csproj --no-restore
dotnet test src\DesktopNode.sln --no-restore
git diff --check
```

Expected: all pass.

### Task 5: Packaging, Command Name, and Product Manifest Follow-up

**Files:**
- Modify: `src/DesktopNode.Cli/DesktopNode.Cli.csproj`
- Modify: `packaging/windows-desktop-node/installer/build.ps1`
- Modify: `packaging/windows-desktop-node/installer/Product.wxs`
- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`

- [x] **Step 1: Publish the product command as `pcvcli.exe`**

Set the CLI assembly name to `pcvcli`, keep the source project name `DesktopNode.Cli`, and verify the project contract so the installed command is stable.

- [x] **Step 2: Include CLI in installer payload**

Publish the CLI during installer build, stage `pcvcli.exe` into the MSI source root, and add package contract coverage for the CLI payload path.

- [x] **Step 3: Track CLI in product manifest and update payload validation**

Add `paths.cli_exe` and `cli` manifest metadata, copy `pcvcli.exe` into standalone product runtime roots, and block update payloads that omit `pcvcli.exe` before service mutation.

- [x] **Step 4: Run follow-up verification**

Run:

```powershell
Invoke-Pester -Path 'packaging/windows-desktop-node/tests','packaging/windows-desktop-node/installer/tests' -Output Detailed
dotnet test src\DesktopNode.sln --no-restore
git diff --check
```

Expected: all pass. This follow-up does not execute installed host mutation, public trusted signing, or external publication evidence.
