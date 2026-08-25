# PureCVisor Desktop Node Product TUI Service Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for closure tracking.

**Goal:** Add `pcvtui.exe`, a Windows Desktop Node product TUI client whose UI structure borrows the Linux TUI layout while using only Desktop Node Local API routes and product packaging contracts.

**Architecture:** Create a new `src/DesktopNode.Tui` .NET console app plus tests. The TUI keeps a testable `Model / Update / View` shape: options and token resolution feed an HTTP API client, a poller builds route snapshots, a reducer handles keyboard intent and confirmation state, and a renderer writes a panel/table/help layout through a console abstraction. Packaging treats `pcvtui.exe` as a product payload peer of `DesktopNode.Host.exe` and `pcvcli.exe`.

**Tech Stack:** C#/.NET `net10.0-windows`, xUnit, `System.Text.Json`, `HttpClient`, DPAPI `ProtectedData`, PowerShell/Pester packaging tests, WiX source contract tests.

---

## Closure Synchronization - 2026-05-10

This plan is synchronized as closed against the current repository state. The `src/DesktopNode.Tui` and `src/DesktopNode.Tui.Tests` projects exist, `pcvtui.exe` is wired into the solution, installer payload, product manifest, runtime payload copy, update payload validation, user/operator documentation, packaging documentation, and installed `pcvtui.exe --smoke-once runtime` operator smoke evidence.

The RED/FAIL expectations below are retained as historical TDD checkpoints. Current focused verification is `dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore`.

Observed closure result: PASS, 115 tests. Installed operator smoke PASS is recorded separately in `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`. No firewall, trust-store, LAN, Event Log, Hyper-V, public upload, winget, or public trusted signing mutation is claimed by this closure sync.

Canonical closure evidence:

- `docs/ga-ready/evidence/product-tui-service-plan-closure-2026-05-10.md`
- `docs/ga-ready/evidence/installed-novnc-tui-operator-smoke-2026-05-10-0411.md`
- `docs/ga-ready/evidence/ga-ready-evidence-ledger-2026-05-04.md`

## File Structure

Create:

- `src/DesktopNode.Tui/DesktopNode.Tui.csproj`: product executable project with assembly name `pcvtui`.
- `src/DesktopNode.Tui/Program.cs`: process entrypoint and exit-code bridge.
- `src/DesktopNode.Tui/TuiApplication.cs`: interactive loop, key dispatch, poll scheduling, mutation confirmation.
- `src/DesktopNode.Tui/TuiApplicationResult.cs`: testable exit result for non-interactive failure paths.
- `src/DesktopNode.Tui/TuiOptions.cs`: command-line option parser.
- `src/DesktopNode.Tui/TuiTokenResolver.cs`: token source resolution and DPAPI protected-token reader.
- `src/DesktopNode.Tui/TuiApiRoutes.cs`: Desktop Node Local API route registry.
- `src/DesktopNode.Tui/TuiApiClient.cs`: route methods and mutation request methods.
- `src/DesktopNode.Tui/TuiTransport.cs`: injectable transport and `HttpClient` transport.
- `src/DesktopNode.Tui/TuiState.cs`: immutable-ish UI state records, tab enum, route snapshots, degradation state.
- `src/DesktopNode.Tui/TuiPoller.cs`: background snapshot refresh orchestration.
- `src/DesktopNode.Tui/TuiRenderer.cs`: string-frame renderer used by both tests and console output.
- `src/DesktopNode.Tui/TuiWidgets.cs`: panels, tables, bars, help popup text, safe text helpers.
- `src/DesktopNode.Tui/TuiKeys.cs`: key intent mapping.
- `src/DesktopNode.Tui/README.md`: user-facing command and boundary notes.
- `src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj`: xUnit test project.
- `src/DesktopNode.Tui.Tests/TuiOptionsTests.cs`: parser and token source conflict tests.
- `src/DesktopNode.Tui.Tests/TuiTokenResolverTests.cs`: token source and redaction tests.
- `src/DesktopNode.Tui.Tests/TuiApiClientTests.cs`: route mapping and bearer transport tests.
- `src/DesktopNode.Tui.Tests/TuiStateTests.cs`: tab switching, filtering, selected row, last-success retention.
- `src/DesktopNode.Tui.Tests/TuiRendererTests.cs`: tab labels, layout text, degraded panels, no token leakage.
- `src/DesktopNode.Tui.Tests/TuiApplicationTests.cs`: key dispatch, confirmation, mutation guard flow.

Modify:

- `src/DesktopNode.sln`: add TUI projects.
- `packaging/windows-desktop-node/installer/build.ps1`: publish/stage/provenance support for `pcvtui.exe`.
- `packaging/windows-desktop-node/installer/Product.wxs`: install `pcvtui.exe`.
- `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`: defaults, paths, manifest metadata, runtime payload copy, update validation.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`: manifest and runtime payload coverage.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`: update payload missing-TUI gate.
- `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`: defaults/path plan coverage.
- `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`: installer parameter, dry-run, payload, provenance coverage.
- `docs/DEVELOPER_INDEX.md`: add active .NET TUI entry.
- `docs/USER_GUIDE.md`: add `pcvtui.exe` launch and token source examples.
- `docs/OPERATIONS_GUIDE.md`: add operator boundary and failure message examples.
- `packaging/windows-desktop-node/README.md`: add packaged TUI payload note.

---

### Task 1: Project, Options, and Token Resolution

**Files:**

- Create: `src/DesktopNode.Tui/DesktopNode.Tui.csproj`
- Create: `src/DesktopNode.Tui/Program.cs`
- Create: `src/DesktopNode.Tui/TuiApplicationResult.cs`
- Create: `src/DesktopNode.Tui/TuiOptions.cs`
- Create: `src/DesktopNode.Tui/TuiTokenResolver.cs`
- Create: `src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj`
- Create: `src/DesktopNode.Tui.Tests/TuiOptionsTests.cs`
- Create: `src/DesktopNode.Tui.Tests/TuiTokenResolverTests.cs`

- [x] **Step 1: Write failing option and token tests**

Create `src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj` with the same test package versions used by `DesktopNode.Cli.Tests`, then add tests asserting default API, refresh interval, `--no-color`, help, token source conflict, environment token, plain token file, and token redaction.

Test skeleton:

```csharp
using DesktopNode.Tui;

namespace DesktopNode.Tui.Tests;

public sealed class TuiOptionsTests
{
    [Fact]
    public void DefaultsToLoopbackApiAndThreeSecondRefresh()
    {
        var options = TuiOptions.Parse(["vm"]);

        Assert.Equal("http://127.0.0.1:7777", options.ApiBaseUrl);
        Assert.Equal(TimeSpan.FromSeconds(3), options.RefreshInterval);
        Assert.Equal(TuiTab.Vm, options.InitialTab);
    }

    [Fact]
    public void RejectsAmbiguousTokenSourcesBeforeNetworkUse()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            TuiOptions.Parse(["--token", "secret", "--token-env", "PCV_TOKEN"]));

        Assert.Contains("PCV_TUI_TOKEN_SOURCE_CONFLICT", error.Message);
        Assert.DoesNotContain("secret", error.Message);
    }
}
```

```csharp
using DesktopNode.Tui;

namespace DesktopNode.Tui.Tests;

public sealed class TuiTokenResolverTests
{
    [Fact]
    public void ReadsTokenFromEnvironmentWithoutLeakingValueInErrors()
    {
        var options = TuiOptions.Parse(["--token-env", "PCV_TOKEN"]);

        var token = TuiTokenResolver.Resolve(options, name => name == "PCV_TOKEN" ? "secret-token" : null);

        Assert.Equal("secret-token", token);
    }

    [Fact]
    public void MissingPlainTokenFileUsesStableCode()
    {
        var options = TuiOptions.Parse(["--token-file", "D:\\missing-token.txt"]);

        var error = Assert.Throws<ArgumentException>(() => TuiTokenResolver.Resolve(options));

        Assert.Contains("PCV_TUI_TOKEN_FILE_NOT_FOUND", error.Message);
        Assert.DoesNotContain("missing-token-value", error.Message);
    }
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore
```

Expected: FAIL because `DesktopNode.Tui` and its types do not exist.

- [x] **Step 3: Create the minimal project and option parser**

Create `DesktopNode.Tui.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <AssemblyName>pcvtui</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="10.0.0" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="DesktopNode.Tui.Tests" />
  </ItemGroup>

</Project>
```

Implement `TuiOptions.Parse` with these rules:

- `--api <url>` sets `ApiBaseUrl`, default `http://127.0.0.1:7777`.
- `--refresh-interval <seconds>` accepts integers `1..60`, default `3`.
- `--no-color` sets `NoColor`.
- `--help` and `-h` set `ShowHelp`.
- `--token`, `--token-file`, `--token-env`, `--protected-token-file` are mutually exclusive.
- positional first tab accepts `vm`, `net`, `job`, `diag`, `host`, `runtime`.

Implement `TuiTokenResolver` with the same DPAPI entropy string as the CLI:

```csharp
internal static readonly byte[] ProtectionEntropy =
    Encoding.UTF8.GetBytes("PureCVisor Desktop Node API Token Store v1");
```

Use TUI-specific stable codes:

```text
PCV_TUI_TOKEN_SOURCE_CONFLICT
PCV_TUI_TOKEN_ENV_EMPTY
PCV_TUI_TOKEN_FILE_NOT_FOUND
PCV_TUI_TOKEN_FILE_EMPTY
PCV_TUI_PROTECTED_TOKEN_FILE_NOT_FOUND
PCV_TUI_PROTECTED_TOKEN_UNSUPPORTED
PCV_TUI_PROTECTED_TOKEN_INVALID
PCV_TUI_PROTECTED_TOKEN_EMPTY
```

- [x] **Step 4: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore
```

Expected: PASS for options and token tests.

- [x] **Step 5: Commit**

```powershell
git add src\DesktopNode.Tui src\DesktopNode.Tui.Tests
git commit -m "feat: add desktop node tui options"
```

---

### Task 2: Route Registry, Transport, and API Client

**Files:**

- Create: `src/DesktopNode.Tui/TuiApiRoutes.cs`
- Create: `src/DesktopNode.Tui/TuiApiClient.cs`
- Create: `src/DesktopNode.Tui/TuiTransport.cs`
- Create: `src/DesktopNode.Tui.Tests/TuiApiClientTests.cs`

- [x] **Step 1: Write failing route and bearer tests**

Add tests that assert each tab route maps to the Desktop Node Local API contract and bearer tokens are passed only to transport, not renderer or errors.

Test routes:

```csharp
[Theory]
[InlineData(TuiTab.Vm, "GET", "/api/v1/vms")]
[InlineData(TuiTab.Net, "GET", "/api/v1/network/inventory")]
[InlineData(TuiTab.Job, "GET", "/api/v1/jobs?limit=50&offset=0")]
[InlineData(TuiTab.Diag, "GET", "/api/v1/diagnostics/bundles?limit=10&offset=0")]
[InlineData(TuiTab.Host, "GET", "/api/v1/host/status")]
[InlineData(TuiTab.Runtime, "GET", "/api/v1/runtime/policy")]
public async Task FetchesTabRoute(TuiTab tab, string method, string path)
{
    var transport = new RecordingTuiTransport(new TuiTransportResponse(200, "application/json", "{\"ok\":true}"));
    var client = new TuiApiClient(transport, new TuiApiClientOptions("http://127.0.0.1:7777", "secret"));

    await client.FetchTabAsync(tab, CancellationToken.None);

    Assert.Equal(method, transport.Request!.Method);
    Assert.Equal(path, transport.Request.Path);
    Assert.Equal("secret", transport.BearerToken);
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiApiClientTests
```

Expected: FAIL because the route registry and client do not exist.

- [x] **Step 3: Implement registry and transport**

Implement:

```csharp
public enum TuiTab
{
    Vm,
    Net,
    Job,
    Diag,
    Host,
    Runtime
}

public sealed record TuiRequest(string Method, string Path, string? Body = null);

public sealed record TuiTransportResponse(
    int StatusCode,
    string ContentType,
    string Body,
    IReadOnlyDictionary<string, string>? Headers = null);

public interface ITuiTransport
{
    Task<TuiTransportResponse> SendAsync(
        TuiRequest request,
        string apiBaseUrl,
        string? bearerToken,
        CancellationToken cancellationToken);
}
```

Implement `HttpTuiTransport` using `HttpClient`, absolute URI construction, `Authorization: Bearer`, JSON request bodies, and response header capture. Implement `TuiApiClient.FetchTabAsync`, `FetchVmDetailAsync`, `FetchJobAsync`, `StartVmAsync`, `ShutdownVmAsync`, `PowerOffVmAsync`, `RestartVmAsync`, `DeleteVmAsync`, `CancelJobAsync`, `RetryJobAsync`, `CreateDiagnosticBundleAsync`, and `DownloadDiagnosticBundleAsync`.

- [x] **Step 4: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiApiClientTests
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\DesktopNode.Tui src\DesktopNode.Tui.Tests
git commit -m "feat: add desktop node tui api client"
```

---

### Task 3: State Model, Reducer, and Poller

**Files:**

- Create: `src/DesktopNode.Tui/TuiState.cs`
- Create: `src/DesktopNode.Tui/TuiPoller.cs`
- Create: `src/DesktopNode.Tui.Tests/TuiStateTests.cs`

- [x] **Step 1: Write failing state tests**

Test behaviors:

- `F1`..`F6` tab intents select `VM/NET/JOB/DIAG/HOST/RUNTIME`.
- selection clamps to available rows.
- `/` filter narrows current table.
- route failure marks only that tab degraded.
- last successful snapshot stays visible after a route failure.
- 429 stores `Retry-After`.
- 504 stores `PCV_ROUTE_TIMEOUT`.

Example:

```csharp
[Fact]
public void RouteFailureKeepsLastSuccessfulSnapshot()
{
    var initial = TuiState.Empty with { ActiveTab = TuiTab.Vm };
    var success = initial.ApplySnapshot(TuiTab.Vm, TuiRouteSnapshot.Success(
        "{\"ok\":true,\"vms\":[{\"id\":\"vm-1\",\"state\":\"Off\"}]}", DateTimeOffset.Parse("2026-05-10T01:02:03Z")));

    var failed = success.ApplySnapshot(TuiTab.Vm, TuiRouteSnapshot.Failure(
        "PCV_ROUTE_TIMEOUT", "Request failed: route timed out.", retryAfter: TimeSpan.FromSeconds(15)));

    Assert.True(failed.Routes[TuiTab.Vm].IsDegraded);
    Assert.Contains("vm-1", failed.Routes[TuiTab.Vm].LastSuccessBody);
    Assert.Equal("PCV_ROUTE_TIMEOUT", failed.Routes[TuiTab.Vm].ErrorCode);
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiStateTests
```

Expected: FAIL because state and poller types do not exist.

- [x] **Step 3: Implement state records and poller**

Implement:

```csharp
public sealed record TuiRouteState(
    bool HasSuccess,
    string LastSuccessBody,
    DateTimeOffset? LastSuccessAt,
    bool IsDegraded,
    string? ErrorCode,
    string? ErrorMessage,
    TimeSpan? RetryAfter);

public sealed record TuiState(
    TuiTab ActiveTab,
    int SelectedRow,
    string Filter,
    bool HelpVisible,
    TuiConfirmation? Confirmation,
    IReadOnlyDictionary<TuiTab, TuiRouteState> Routes)
{
    public static TuiState Empty { get; }
}
```

`TuiPoller` should fetch the common header routes and active tab route, convert non-2xx and problem-details bodies into `TuiRouteSnapshot.Failure`, and leave successful previous state in place on failure.

- [x] **Step 4: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiStateTests
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\DesktopNode.Tui src\DesktopNode.Tui.Tests
git commit -m "feat: add desktop node tui state model"
```

---

### Task 4: Renderer, Widgets, and Linux TUI Structure Port

**Files:**

- Create: `src/DesktopNode.Tui/TuiRenderer.cs`
- Create: `src/DesktopNode.Tui/TuiWidgets.cs`
- Create: `src/DesktopNode.Tui.Tests/TuiRendererTests.cs`

- [x] **Step 1: Write failing renderer tests**

Test:

- header contains `PureCVisor Desktop Node TUI`.
- tabs render as `F1 VM | F2 NET | F3 JOB | F4 DIAG | F5 HOST | F6 RUNTIME`.
- VM tab renders a main table and inspector panel.
- route degraded panel includes stable code and last-success timestamp.
- help overlay includes `q`, `R`, `/`, `Esc`, and mutation boundary text.
- rendered output does not include raw token value or `Authorization`.

Example:

```csharp
[Fact]
public void RendersDesktopNodeTabsAndNoLinuxRuntimeTabs()
{
    var frame = TuiRenderer.Render(TuiState.Empty, new TuiRenderOptions(Width: 120, Height: 32, NoColor: true));

    Assert.Contains("F1 VM | F2 NET | F3 JOB | F4 DIAG | F5 HOST | F6 RUNTIME", frame.Text);
    Assert.DoesNotContain("STG", frame.Text);
    Assert.DoesNotContain("CTR", frame.Text);
    Assert.DoesNotContain("OVN", frame.Text);
    Assert.DoesNotContain("purecvisorsd", frame.Text);
    Assert.DoesNotContain("libvirt", frame.Text);
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiRendererTests
```

Expected: FAIL because renderer and widget code does not exist.

- [x] **Step 3: Implement renderer and widgets**

Use built-in `System.Console` capabilities only. `TuiRenderer.Render` returns a `TuiFrame` record for tests:

```csharp
public sealed record TuiRenderOptions(int Width, int Height, bool NoColor);

public sealed record TuiFrame(string Text);
```

Implement widget helpers:

- `Panel(title, lines, width)`
- `Table(headers, rows, selectedRow, width, height)`
- `StatusBar(items, width)`
- `ProgressBar(percent, width)`
- `HelpOverlay(activeTab, width, height)`
- `Redact(value)` for token-like values

Use ASCII borders by default so Windows console output is stable under tests:

```text
+-- HOST -------------------------------------------------------------+
| service=Running api=reachable degraded=0 refresh=3s                 |
+---------------------------------------------------------------------+
```

- [x] **Step 4: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiRendererTests
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\DesktopNode.Tui src\DesktopNode.Tui.Tests
git commit -m "feat: render desktop node tui layout"
```

---

### Task 5: Interactive Application Loop and Mutation Confirmation

**Files:**

- Create: `src/DesktopNode.Tui/TuiApplication.cs`
- Create: `src/DesktopNode.Tui/TuiKeys.cs`
- Modify: `src/DesktopNode.Tui/Program.cs`
- Create: `src/DesktopNode.Tui.Tests/TuiApplicationTests.cs`

- [x] **Step 1: Write failing application tests**

Test:

- `--help` returns exit `0` with usage text.
- auth parse failure exits `2` and redacts token values.
- `q` exits the loop.
- `F2` selects `NET`.
- `h` toggles help.
- `R` triggers refresh.
- VM delete on running VM is locally blocked with `PCV_TUI_VM_DELETE_RUNNING`.
- job cancel without selected job uses `PCV_TUI_JOB_SELECTION_REQUIRED`.
- diagnostic download without bundle uses `PCV_TUI_DIAGNOSTIC_BUNDLE_REQUIRED`.

Example:

```csharp
[Fact]
public async Task BlocksRunningVmDeleteBeforeTransportMutation()
{
    var transport = new RecordingTuiTransport(new TuiTransportResponse(200, "application/json", "{\"ok\":true}"));
    var input = new ScriptedTuiConsole(["Delete", "Enter", "q"]);
    var state = TuiStateFixtures.RunningVmSelected("vm-running-01");

    var result = await TuiApplication.RunAsync(
        ["--token", "secret"],
        transport,
        input,
        initialState: state,
        cancellationToken: CancellationToken.None);

    Assert.Equal(0, result.ExitCode);
    Assert.Contains("PCV_TUI_VM_DELETE_RUNNING", input.Output);
    Assert.DoesNotContain("secret", input.Output);
    Assert.DoesNotContain("DELETE", transport.MutationMethods);
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiApplicationTests
```

Expected: FAIL because loop and key dispatch do not exist.

- [x] **Step 3: Implement application and console abstraction**

Implement `ITuiConsole`:

```csharp
public interface ITuiConsole
{
    int Width { get; }
    int Height { get; }
    bool KeyAvailable { get; }
    ConsoleKeyInfo ReadKey(bool intercept);
    void Clear();
    void Write(string text);
}
```

`SystemTuiConsole` wraps `System.Console`. `TuiApplication.RunAsync` parses options, resolves token, creates `TuiApiClient`, renders frames, dispatches key intents, and returns `TuiApplicationResult`.

Do not start background tasks in unit tests unless cancellation is controlled by the test. Allow tests to inject an initial state and scripted console.

- [x] **Step 4: Verify GREEN**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore --filter TuiApplicationTests
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add src\DesktopNode.Tui src\DesktopNode.Tui.Tests
git commit -m "feat: add interactive desktop node tui loop"
```

---

### Task 6: Solution and Installer Build Packaging

**Files:**

- Modify: `src/DesktopNode.sln`
- Modify: `packaging/windows-desktop-node/installer/build.ps1`
- Modify: `packaging/windows-desktop-node/installer/Product.wxs`
- Modify: `packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1`

- [x] **Step 1: Add failing installer tests**

Extend installer plan tests to assert:

- `build.ps1` exposes `[string]$DesktopNodeTuiPath`.
- missing explicit TUI path returns `PCV_INSTALLER_TUI_NOT_FOUND`.
- dry-run records `tui_source`, `tui_path`, and `tui_sha256`.
- real build payload contains `pcvtui.exe`.
- provenance contains `tui.mode = dotnet-local-api-interactive-client`.
- `Product.wxs` contains `DesktopNodeTuiComponent` and `pcvtui.exe`.

Pester snippet:

```powershell
It 'returns structured JSON when TUI input is missing' {
    $hostPath = Join-Path $TestDrive 'DesktopNode.Host.exe'
    $tuiPath = Join-Path $TestDrive 'missing-pcvtui.exe'
    Set-Content -LiteralPath $hostPath -Value 'fake-host' -NoNewline

    $jsonText = pwsh -NoProfile -ExecutionPolicy Bypass -File $script:BuildScript `
        -Version '0.41.0-dev' `
        -DesktopNodeHostPath $hostPath `
        -DesktopNodeTuiPath $tuiPath `
        -OutputRoot (Join-Path $TestDrive 'out') `
        -SigningMode AllowUnsignedDev `
        -DryRun 2>$null

    $LASTEXITCODE | Should -Be 1
    $output = $jsonText | ConvertFrom-Json
    $output.error.code | Should -Be 'PCV_INSTALLER_TUI_NOT_FOUND'
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1' -Output Detailed"
```

Expected: FAIL on missing TUI packaging support.

- [x] **Step 3: Implement installer packaging**

In `build.ps1`:

- Add parameter `[string]$DesktopNodeTuiPath`.
- Add `$tuiPublishRoot`, `$tuiProjectPath`, `$resolvedDesktopNodeTuiPath`, `$desktopNodeTuiHash`, `$desktopNodeTuiSource`.
- Resolve explicit path with `PCV_INSTALLER_TUI_NOT_FOUND`.
- Add dry-run plan fields `tui_source`, `tui_path`, `tui_sha256`.
- Publish `DesktopNode.Tui.csproj` when no path is provided.
- Use `PCV_INSTALLER_TUI_PUBLISH_FAILED` and `PCV_INSTALLER_TUI_PUBLISH_MISSING_EXE`.
- Copy `pcvtui.exe` into `$payloadRoot`.
- Add `tui` to provenance.

In `Product.wxs`, add:

```xml
<Component Id="DesktopNodeTuiComponent" Directory="INSTALLFOLDER" Guid="{E0C310EE-6CE4-41F6-8E00-00ED5D0593E8}">
  <File Id="DesktopNodeTui" Source="$(var.PayloadRoot)\pcvtui.exe" KeyPath="yes" />
</Component>
```

Use this GUID for the WiX component so future upgrades keep a stable component identity.

- [x] **Step 4: Add projects to solution**

Run:

```powershell
dotnet sln src\DesktopNode.sln add src\DesktopNode.Tui\DesktopNode.Tui.csproj src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj
```

- [x] **Step 5: Verify GREEN**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1' -Output Detailed"
dotnet test src\DesktopNode.sln --no-restore
```

Expected: PASS.

- [x] **Step 6: Commit**

```powershell
git add src/DesktopNode.sln packaging/windows-desktop-node/installer/build.ps1 packaging/windows-desktop-node/installer/Product.wxs packaging/windows-desktop-node/installer/tests/PcvDesktopNodeInstaller.Plan.Tests.ps1 src/DesktopNode.Tui src/DesktopNode.Tui.Tests
git commit -m "feat: package desktop node tui payload"
```

---

### Task 7: Product Manifest, Runtime Payload, and Update Gate

**Files:**

- Modify: `packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1`
- Modify: `packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1`

- [x] **Step 1: Write failing product wrapper tests**

Add tests asserting:

- defaults include `tui_exe_name = pcvtui.exe`.
- paths include `tui_exe`.
- manifest includes `tui.command_name = pcvtui`.
- manifest includes `tui.ui_structure_source = purecvisor-single-tui-structure-port`.
- runtime payload copy copies `pcvtui.exe`.
- update payload validation requires `pcvtui.exe`.
- missing `pcvtui.exe` returns `PCV_PRODUCT_UPDATE_PAYLOAD_FILE_MISSING` and detail includes `pcvtui.exe`.

Pester snippet:

```powershell
It 'records active .NET TUI metadata in product-manifest.json' {
    $productRoot = Join-Path $TestDrive 'DesktopNodeTuiManifest'
    $dataRoot = Join-Path $TestDrive 'data-tui-manifest'

    $manifest = New-PcvDesktopNodeProductManifest `
        -SourceRoot $script:RepoRoot `
        -ProductRoot $productRoot `
        -DataRoot $dataRoot `
        -Version '0.41.0-dev'

    $manifest.paths.tui_exe | Should -Be (Join-Path $productRoot 'pcvtui.exe')
    $manifest.tui.command_name | Should -Be 'pcvtui'
    $manifest.tui.mode | Should -Be 'dotnet-local-api-interactive-client'
    $manifest.tui.ui_structure_source | Should -Be 'purecvisor-single-tui-structure-port'
    $manifest.tui.runtime_boundary | Should -Be 'windows-desktop-node-local-api-only'
}
```

- [x] **Step 2: Run RED verification**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected: FAIL on missing TUI defaults, manifest metadata, runtime copy, and update validation.

- [x] **Step 3: Implement product wrapper support**

Update `Get-PcvDesktopNodeProductDefaults`:

```powershell
tui_exe_name = 'pcvtui.exe'
```

Update `Resolve-PcvDesktopNodeProductPaths`:

```powershell
tui_exe = Join-PcvProductPath -Root $ProductRoot -ChildPath @($defaults.tui_exe_name)
```

Update `New-PcvDesktopNodeProductManifest`:

```powershell
tui = [ordered]@{
    mode = 'dotnet-local-api-interactive-client'
    command_name = 'pcvtui'
    executable_path = $paths.tui_exe
    default_owner = 'desktop-node-product-tui'
    ui_structure_source = 'purecvisor-single-tui-structure-port'
    runtime_boundary = 'windows-desktop-node-local-api-only'
    token_sources = @(
        '--token',
        '--token-file',
        '--token-env',
        '--protected-token-file'
    )
}
```

Update runtime payload lists and update payload required files by adding `pcvtui.exe` next to `pcvcli.exe`.

- [x] **Step 4: Verify GREEN**

Run:

```powershell
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1','packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1' -Output Detailed"
```

Expected: PASS.

- [x] **Step 5: Commit**

```powershell
git add packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Invoke.Tests.ps1 packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1
git commit -m "feat: track desktop node tui in product manifest"
```

---

### Task 8: Documentation and Full Verification

**Files:**

- Create: `src/DesktopNode.Tui/README.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/USER_GUIDE.md`
- Modify: `docs/OPERATIONS_GUIDE.md`
- Modify: `packaging/windows-desktop-node/README.md`

- [x] **Step 1: Write documentation updates**

Document:

- `pcvtui.exe` is an interactive Local API TUI client, not a Windows SCM service.
- It uses `VM/NET/JOB/DIAG/HOST/RUNTIME` tabs.
- It borrows Linux TUI structure only.
- It supports token sources `--token`, `--token-file`, `--token-env`, `--protected-token-file`.
- Failure messages use stable codes such as `PCV_TUI_AUTH_REQUIRED`, `PCV_TUI_VM_DELETE_RUNNING`, `PCV_RATE_LIMIT_EXCEEDED`, and `PCV_PRODUCT_UPDATE_PAYLOAD_FILE_MISSING`.
- Public trusted signing and external stable publication are not claimed by this code-level implementation.

`src/DesktopNode.Tui/README.md` minimum content:

````markdown
# DesktopNode.Tui

`pcvtui.exe` is the Windows Desktop Node interactive TUI client. It connects to
the installed Local API service over loopback HTTP and never embeds Linux
`purecvisorsd`, libvirt, KVM, LXC, ZFS, OVS, or OVN runtime behavior.

## Examples

```powershell
pcvtui.exe --protected-token-file C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json
pcvtui.exe --api http://127.0.0.1:7777 --token-env PCV_TOKEN vm
```

## Tabs

`VM`, `NET`, `JOB`, `DIAG`, `HOST`, and `RUNTIME`.
```
````

- [x] **Step 2: Run focused verification**

Run:

```powershell
dotnet test src\DesktopNode.Tui.Tests\DesktopNode.Tui.Tests.csproj --no-restore
pwsh -NoProfile -Command "Invoke-Pester -Path 'packaging/windows-desktop-node/tests','packaging/windows-desktop-node/installer/tests' -Output Detailed"
```

Expected: PASS.

- [x] **Step 3: Run solution verification**

Run:

```powershell
dotnet test src\DesktopNode.sln --no-restore
git diff --check
```

Expected: PASS.

- [x] **Step 4: Commit**

```powershell
git add src/DesktopNode.Tui/README.md docs/DEVELOPER_INDEX.md docs/USER_GUIDE.md docs/OPERATIONS_GUIDE.md packaging/windows-desktop-node/README.md
git commit -m "docs: document desktop node tui"
```

---

## Self-Review Checklist

- [x] Spec coverage: `pcvtui.exe`, tab names, Linux structure-only boundary, Local API route mapping, error messages, packaging, manifest, update gate, and docs are covered.
- [x] Completion scan: every path, command, error code, and type name needed by the plan is explicit.
- [x] Type consistency: `TuiTab`, `TuiState`, `TuiRouteState`, `TuiApiClient`, and `ITuiTransport` names match across tasks.
- [x] Boundary guard: no Linux runtime code, UDS, `purecvisorsd`, libvirt, KVM, LXC, ZFS, OVS, or OVN is added to active product paths.
- [x] Redaction guard: no token value or `Authorization` header appears in renderer output or error messages.

## Execution Options

This plan has been executed and closed. The options below are retained as historical execution guidance:

1. **Subagent-Driven**: dispatch one worker per task, review each task, then integrate. This is best for packaging and TUI implementation running in parallel.
2. **Inline Execution**: execute tasks in this session in order, using the TDD RED/GREEN/REFACTOR sequence and committing at each task boundary.
