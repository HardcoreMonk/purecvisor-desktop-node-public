# PureCVisor Desktop Node Development Gate Recovery Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `dotnet test src/DesktopNode.sln` pass from a normal non-administrator Windows checkout, preserve the installed product's ProgramData token and ACL security defaults, and add non-mutating CI coverage for the active .NET, Web, packaging Pester, installer Pester, and Web Pester gates.

**Architecture:** Carry the CLI's existing protected-token path seam through the application and interactive-shell entry points; normalize protected-token read failures into stable, redacted CLI error codes; extract Host ACL mutation behind an internal hardener interface that production uses and tests replace with a recording no-op; then enforce the recovered local gates with a dedicated GitHub Actions workflow and code-level evidence. The installed `0.42.59-admin-smoke` anchor remains unchanged, and opening `0.42.60-admin-smoke` requires separate approval.

**Tech Stack:** .NET 10 / C# / xUnit, Windows DPAPI and `System.Security.AccessControl`, PowerShell 7 / Pester 5.7.1, Node.js 24 / npm / TypeScript, GitHub Actions YAML, Markdown evidence.

---

## Execution constraints

- Repository rule: do not use subagents unless the user explicitly authorizes them. Execute this plan inline with `executing-plans`, preferably in a new Codex task because the design stage is complete.
- Review once after all local changes. Do not enter an inspect/fix/reinspect loop.
- Preserve unrelated user work. In particular, `docs/project-status-audit-2026-07-13.md` begins this plan as an intentional untracked audit artifact and is only staged in the documentation task.
- Do not run service install/start/stop/delete, MSI/Burn/MSIX lifecycle, Hyper-V mutation, firewall/trust-store/Event Log mutation, package build, signing, publication, or release commands.
- Do not create an `0.42.60-admin-smoke` package candidate. The current installed operational anchor remains `0.42.59-admin-smoke`.
- A remote push and CI observation are an explicit checkpoint because they change GitHub state. Complete all local work first, then request authorization before Task 6.

## Target file map

| Area | Files |
| --- | --- |
| CLI path seam | `src/DesktopNode.Cli/DesktopNodeCliApplication.cs`, `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs`, `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`, `src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs` |
| CLI error contract | `src/DesktopNode.Cli/DesktopNodeCliTokenResolver.cs`, `src/DesktopNode.Cli.Tests/DesktopNodeCliTokenResolverTests.cs`, `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs` |
| Host ACL seam | `src/DesktopNode.Host/DesktopNodeHostFileAclHardener.cs`, `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`, `src/DesktopNode.Host/Ops/DesktopNodeServiceLifecycleOps.cs`, `src/DesktopNode.Host/Ops/DesktopNodeServiceTokenOps.cs`, `src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs`, `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs` |
| CI contract | `.github/workflows/development-gates.yml`, `packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1` |
| Policy/evidence | `docs/DEVELOPMENT_VERIFICATION_POLICY.md`, `docs/DEVELOPER_INDEX.md`, `docs/ga-ready/EVIDENCE_INDEX.md`, `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md`, `docs/project-status-audit-2026-07-13.md` |

## Task 1: Isolate CLI tests from the installed ProgramData token

**Files:**

- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs`
- Modify: `src/DesktopNode.Cli/DesktopNodeCliApplication.cs`
- Modify: `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs`
- Verify unchanged production entry point: `src/DesktopNode.Cli/Program.cs`

- [ ] **Step 1: Add a unique missing-path helper to both CLI test classes**

Add this helper to each class:

```csharp
private static string MissingDefaultProtectedTokenPath()
{
    return Path.Combine(
        Path.GetTempPath(),
        "pcv-cli-default-token-tests",
        Guid.NewGuid().ToString("N"),
        "api-token.dpapi.json");
}
```

Pass `defaultProtectedTokenFilePath: MissingDefaultProtectedTokenPath()` to every `DesktopNodeCliApplication.RunAsync` and `DesktopNodeCliInteractiveShell.RunAsync` call in these two test files. Use named arguments so adding the seam cannot swap the path with the cancellation token.

- [ ] **Step 2: Add an application-level isolation test**

Add to `DesktopNodeCliApplicationTests.cs`:

```csharp
[Fact]
public async Task UsesInjectedMissingDefaultProtectedTokenPathWithoutReadingMachineState()
{
    var transport = new RecordingTransport(
        new DesktopNodeCliTransportResponse(200, "application/json", "{\"ok\":true}"));
    var missingPath = MissingDefaultProtectedTokenPath();

    var result = await DesktopNodeCliApplication.RunAsync(
        ["host", "status"],
        transport,
        environment: _ => null,
        defaultProtectedTokenFilePath: missingPath,
        cancellationToken: CancellationToken.None);

    Assert.Equal(0, result.ExitCode);
    Assert.True(transport.Called);
    Assert.Null(transport.BearerToken);
    Assert.False(File.Exists(missingPath));
}
```

- [ ] **Step 3: Run the CLI tests and confirm the RED state**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj -c Release
```

Expected: compilation fails because `RunAsync` does not yet define `defaultProtectedTokenFilePath`. The failure must be limited to the new named argument; do not continue if an unrelated compile error appears.

- [ ] **Step 4: Carry the seam through the application entry point**

Change `DesktopNodeCliApplication.RunAsync` to:

```csharp
public static async Task<DesktopNodeCliApplicationResult> RunAsync(
    IReadOnlyList<string> args,
    IDesktopNodeCliTransport transport,
    Func<string, string?>? environment = null,
    string? defaultProtectedTokenFilePath = null,
    CancellationToken cancellationToken = default)
```

Change token resolution to:

```csharp
var token = DesktopNodeCliTokenResolver.Resolve(
    options,
    environment,
    defaultProtectedTokenFilePath);
```

- [ ] **Step 5: Carry the same seam through the interactive shell**

Change `DesktopNodeCliInteractiveShell.RunAsync` to:

```csharp
public static async Task<int> RunAsync(
    IReadOnlyList<string> startupArgs,
    IDesktopNodeCliTransport transport,
    Func<string?> readLine,
    Action<string> writeOutput,
    Action<string> writeError,
    Func<string, string?>? environment = null,
    string? defaultProtectedTokenFilePath = null,
    CancellationToken cancellationToken = default)
```

Dispatch with named arguments:

```csharp
var result = await DesktopNodeCliApplication.RunAsync(
        args,
        transport,
        environment,
        defaultProtectedTokenFilePath,
        cancellationToken)
    .ConfigureAwait(false);
```

Do not pass a path from `Program.cs`; omission there deliberately retains the production ProgramData default.

- [ ] **Step 6: Run the CLI tests and confirm GREEN**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj -c Release
```

Expected: `Passed`, `Failed: 0`; no test reads `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json`.

- [ ] **Step 7: Commit the CLI isolation slice**

```powershell
git add src/DesktopNode.Cli/DesktopNodeCliApplication.cs src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs
git commit -m "test: isolate CLI token defaults from machine state"
```

Expected: one commit containing only the four listed files.

## Task 2: Normalize and redact protected-token failures

**Files:**

- Modify: `src/DesktopNode.Cli/DesktopNodeCliTokenResolver.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliTokenResolverTests.cs`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`

- [ ] **Step 1: Add RED tests for the three error categories**

Add resolver tests that assert these exact messages:

```csharp
[Fact]
public void NormalizesProtectedTokenAccessDeniedWithoutPathOrSid()
{
    var error = new UnauthorizedAccessException(
        @"Access denied: C:\ProgramData\PureCVisor\desktop-node\api-token.dpapi.json S-1-5-21-1234");

    var normalized = DesktopNodeCliTokenResolver.NormalizeProtectedTokenReadException(error);

    Assert.Equal(
        "PCV_CLI_PROTECTED_TOKEN_ACCESS_DENIED|Protected token file access was denied.",
        normalized.Message);
    Assert.DoesNotContain("C:\\ProgramData", normalized.Message, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("S-1-5-21", normalized.Message, StringComparison.OrdinalIgnoreCase);
}

[Theory]
[InlineData("json")]
[InlineData("base64")]
public void RejectsMalformedProtectedTokenPayloadWithStableCode(string kind)
{
    var directory = Path.Combine(Path.GetTempPath(), "pcv-cli-token-tests", Guid.NewGuid().ToString("N"));
    var path = Path.Combine(directory, "api-token.dpapi.json");
    Directory.CreateDirectory(directory);
    try
    {
        File.WriteAllText(
            path,
            kind == "json"
                ? "{not-json"
                : "{\"storage\":\"dpapi-local-machine\",\"scope\":\"LocalMachine\",\"protected_token\":\"%%%\"}");

        var options = DesktopNodeCliOptions.Parse(["--protected-token-file", path, "host", "status"]);
        var error = Assert.Throws<ArgumentException>(() =>
            DesktopNodeCliTokenResolver.Resolve(options, environment: _ => null));

        Assert.StartsWith("PCV_CLI_PROTECTED_TOKEN_INVALID|", error.Message);
        Assert.DoesNotContain(path, error.Message, StringComparison.OrdinalIgnoreCase);
    }
    finally
    {
        Directory.Delete(directory, recursive: true);
    }
}

[Fact]
public void NormalizesDpapiFailureWithStableCode()
{
    var normalized = DesktopNodeCliTokenResolver.NormalizeProtectedTokenReadException(
        new CryptographicException("machine-specific DPAPI diagnostic"));

    Assert.Equal(
        "PCV_CLI_PROTECTED_TOKEN_DECRYPT_FAILED|Protected token file could not be decrypted.",
        normalized.Message);
    Assert.DoesNotContain("machine-specific", normalized.Message, StringComparison.OrdinalIgnoreCase);
}
```

Add an application integration test that writes malformed JSON to an injected default path and asserts exit code `2`, `PCV_CLI_PROTECTED_TOKEN_INVALID`, `transport.Called == false`, and absence of the path and payload in `StandardError`.

- [ ] **Step 2: Run the focused tests and confirm RED**

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj -c Release --filter "FullyQualifiedName~DesktopNodeCliTokenResolverTests|FullyQualifiedName~DesktopNodeCliApplicationTests"
```

Expected: compilation fails because `NormalizeProtectedTokenReadException` is not implemented, or the new stable-code assertions fail against the raw framework exceptions.

- [ ] **Step 3: Implement one redacted exception mapper**

Add `using System.Security;` and implement:

```csharp
internal static ArgumentException NormalizeProtectedTokenReadException(Exception error)
{
    return error switch
    {
        UnauthorizedAccessException or SecurityException => new ArgumentException(
            "PCV_CLI_PROTECTED_TOKEN_ACCESS_DENIED|Protected token file access was denied.",
            error),
        JsonException or FormatException => new ArgumentException(
            "PCV_CLI_PROTECTED_TOKEN_INVALID|Protected token file is invalid.",
            error),
        CryptographicException => new ArgumentException(
            "PCV_CLI_PROTECTED_TOKEN_DECRYPT_FAILED|Protected token file could not be decrypted.",
            error),
        _ => throw new ArgumentException("Unsupported protected token read failure.", nameof(error))
    };
}
```

Wrap the body of `ReadProtectedTokenFile` with this filtered catch:

```csharp
catch (Exception error) when (
    error is UnauthorizedAccessException or
    SecurityException or
    JsonException or
    FormatException or
    CryptographicException)
{
    throw NormalizeProtectedTokenReadException(error);
}
```

Keep the existing stable `PCV_CLI_PROTECTED_TOKEN_FILE_NOT_FOUND`, `..._UNSUPPORTED`, `..._EMPTY`, and missing-field `..._INVALID` contracts. Do not include the source path, SID, raw JSON, base64 payload, DPAPI diagnostic, or inner exception text in the displayed message.

- [ ] **Step 4: Confirm exit-code and redaction behavior**

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj -c Release
```

Expected: `Passed`, `Failed: 0`; all protected-token category failures return through the application's `ArgumentException` handler with exit code `2`.

- [ ] **Step 5: Commit the CLI error-contract slice**

```powershell
git add src/DesktopNode.Cli/DesktopNodeCliTokenResolver.cs src/DesktopNode.Cli.Tests/DesktopNodeCliTokenResolverTests.cs src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs
git commit -m "fix: normalize protected token CLI failures"
```

## Task 3: Inject Host ACL hardening without weakening production ACLs

**Files:**

- Create: `src/DesktopNode.Host/DesktopNodeHostFileAclHardener.cs`
- Modify: `src/DesktopNode.Host/DesktopNodeHostServiceAction.cs`
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeServiceLifecycleOps.cs`
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeServiceTokenOps.cs`
- Modify: `src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs`
- Modify: `src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs`

- [ ] **Step 1: Replace ACL-dependent test behavior with a recording hardener**

Add this nested fake to `DesktopNodeHostServiceActionTests.cs`:

```csharp
private sealed class RecordingFileAclHardener : IDesktopNodeHostFileAclHardener
{
    public List<string> Paths { get; } = [];

    public void Harden(string path)
    {
        Paths.Add(Path.GetFullPath(path));
    }
}
```

Update the tests that create `api-token.dpapi.json`, `accounts.json`, `jwt-signing-key.txt`, rotate a service token, or perform credential-manager default transition so they inject this hardener. Assert both content and calls:

```csharp
var hardener = new RecordingFileAclHardener();
var path = DesktopNodeHostServiceAction.EnsureProtectedTokenFile(dataRoot, hardener);

Assert.Equal(Path.GetFullPath(path), Assert.Single(hardener.Paths));
var text = File.ReadAllText(path);
Assert.Contains("\"storage\": \"dpapi-local-machine\"", text);
```

For account bootstrap, assert the recorded paths are exactly `accounts.json` and `jwt-signing-key.txt`. For the `service-token-rotation-revoke` operation, clear the recorder after initial token creation and assert the replacement token path is hardened exactly once. Remove `TryReadProtectedTokenFile`, `AssertProtectedTokenFileAcl`, and their now-unused access-control test imports; real ACL inspection belongs to the elevated installed smoke.

- [ ] **Step 2: Add a service-action test proving all three bootstrap files use the seam**

Exercise `configure-installed` through the internal `ExecuteAsync` overload with fake service controllers and the recording hardener. Assert the result is successful and the normalized recorded set equals:

```text
the normalized data-root path joined with `api-token.dpapi.json`
the normalized data-root path joined with `accounts.json`
the normalized data-root path joined with `jwt-signing-key.txt`
```

Also assert every file exists and retains its expected JSON/key content. This test must not inspect or change a real Windows ACL.

- [ ] **Step 3: Run Host tests and confirm RED**

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release --filter "FullyQualifiedName~DesktopNodeHostServiceActionTests"
```

Expected: compilation fails because the hardener interface and internal overloads do not exist yet.

- [ ] **Step 4: Add the production hardener with the unchanged ACL policy**

Create `DesktopNodeHostFileAclHardener.cs`:

```csharp
using System.Security.AccessControl;
using System.Security.Principal;

namespace DesktopNode.Host;

internal interface IDesktopNodeHostFileAclHardener
{
    void Harden(string path);
}

internal sealed class DesktopNodeHostFileAclHardener : IDesktopNodeHostFileAclHardener
{
    public static DesktopNodeHostFileAclHardener Instance { get; } = new();

    private DesktopNodeHostFileAclHardener()
    {
    }

    public void Harden(string path)
    {
        var fileInfo = new FileInfo(path);
        var security = new FileSecurity();
        security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            FileSystemRights.Read,
            AccessControlType.Allow));
        security.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
            FileSystemRights.Read,
            AccessControlType.Allow));
        fileInfo.SetAccessControl(security);
    }
}
```

This is a move of the existing policy: protected inheritance, Administrators read, SYSTEM read, and no current-user grant. Do not add test-user, Users, or Everyone access.

- [ ] **Step 5: Preserve public overloads and add one internal injected core overload**

Keep all current public signatures. Make the existing full public overload forward to this internal overload with `DesktopNodeHostFileAclHardener.Instance`:

```csharp
internal static async Task<DesktopNodeHostServiceActionResult> ExecuteAsync(
    DesktopNodeHostOptions options,
    IDesktopNodeWindowsServiceController? serviceController,
    IDesktopNodeWindowsEventLogController? eventLogController,
    IDesktopNodeWindowsFirewallController? firewallController,
    IDesktopNodeWindowsTrustStoreController? trustStoreController,
    IDesktopNodeWindowsCredentialManagerController? credentialManagerController,
    IDesktopNodeHostFileAclHardener fileAclHardener,
    CancellationToken cancellationToken = default)
```

At the start of the internal overload, call `ArgumentNullException.ThrowIfNull(fileAclHardener)`. Production callers never receive or select the internal type.

- [ ] **Step 6: Thread the hardener through every token-writing route**

Update these call edges explicitly:

| Caller | Callee change |
| --- | --- |
| internal `ExecuteAsync` legacy configure/repair branch | `EnsureProtectedTokenFile(options.DataRoot!, fileAclHardener)` |
| `DesktopNodeServiceLifecycleOps.Execute` → `ExecuteNativeServiceActionForOps` → `ExecuteNativeServiceAction` → `ExecuteNativeConfigureOrRepair` | add and forward `IDesktopNodeHostFileAclHardener` |
| `DesktopNodeServiceTokenOps.Execute` → `ExecuteNativeServiceTokenActionForOps` → `ExecuteNativeServiceTokenRotationRevoke` | add and forward the hardener; call `fileAclHardener.Harden(tokenPath)` after atomic replace |
| `DesktopNodeCredentialManagerOps.Execute` → `ExecuteNativeCredentialManagerActionForOps` → `ExecuteNativeCredentialManagerDefaultTransition` | add and forward the hardener; use it when ensuring the protected token |
| `EnsureProtectedTokenFile` | public overload forwards to internal overload with production singleton |
| `EnsureAccountAuthBootstrapFiles` | public overload forwards to internal overload with production singleton |
| `WriteProtectedTokenFile` | accept nullable hardener; initial writes pass the injected hardener, temporary rotation writes pass `null` |

Use these overload shapes:

```csharp
public static string EnsureProtectedTokenFile(string dataRoot)
{
    return EnsureProtectedTokenFile(dataRoot, DesktopNodeHostFileAclHardener.Instance);
}

internal static string EnsureProtectedTokenFile(
    string dataRoot,
    IDesktopNodeHostFileAclHardener fileAclHardener)
```

```csharp
public static void EnsureAccountAuthBootstrapFiles(string dataRoot)
{
    EnsureAccountAuthBootstrapFiles(dataRoot, DesktopNodeHostFileAclHardener.Instance);
}

internal static void EnsureAccountAuthBootstrapFiles(
    string dataRoot,
    IDesktopNodeHostFileAclHardener fileAclHardener)
```

```csharp
private static string WriteProtectedTokenFile(
    string path,
    string token,
    DateTimeOffset createdAt,
    IDesktopNodeHostFileAclHardener? fileAclHardener)
```

Replace `HardenTokenFileAcl(path)` with `fileAclHardener?.Harden(path)` and delete the old private hardening method only after every call site is routed. Existing-file reads and token-source validation stay unchanged.

- [ ] **Step 7: Run the Host project and solution gates**

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj -c Release
dotnet test src/DesktopNode.sln -c Release
```

Expected for both: `Passed`, `Failed: 0` from the current non-administrator session. The solution run must no longer fail because of the installed ProgramData token or temporary-file ACL ownership.

- [ ] **Step 8: Commit the Host ACL seam**

```powershell
git add src/DesktopNode.Host/DesktopNodeHostFileAclHardener.cs src/DesktopNode.Host/DesktopNodeHostServiceAction.cs src/DesktopNode.Host/Ops/DesktopNodeServiceLifecycleOps.cs src/DesktopNode.Host/Ops/DesktopNodeServiceTokenOps.cs src/DesktopNode.Host/Ops/DesktopNodeCredentialManagerOps.cs src/DesktopNode.Host.Tests/DesktopNodeHostServiceActionTests.cs
git commit -m "test: isolate Host ACL hardening from unit tests"
```

## Task 4: Add a non-mutating development-gates workflow

**Files:**

- Create: `packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1`
- Create: `.github/workflows/development-gates.yml`
- Preserve: `.github/workflows/public-boundary.yml`

- [ ] **Step 1: Write the workflow contract test first**

The Pester test must load `.github/workflows/development-gates.yml` as text and assert:

- workflow name `Development Gates`;
- triggers for pull requests, `main`, `codex/**`, and manual dispatch;
- jobs named `dotnet-tests`, `web-tests`, `packaging-pester`, and `installer-web-pester`;
- `windows-latest` for .NET and Pester jobs, `ubuntu-latest` for Web;
- `.NET 10.0.x`, Node `24`, and exact Pester `5.7.1`;
- the exact solution, npm, parity, packaging, installer, and Web commands;
- `contents: read`, concurrency cancellation, and job timeouts;
- absence of `msiexec`, `Invoke-PcvAdminSmokePackage`, `Invoke-PcvFullAdminHostMutationGate`, `Start-VM`, `New-VM`, service mutation, signing, release, upload, and deployment commands.

Use a repo-root helper consistent with the existing Pester suites:

```powershell
Set-StrictMode -Version Latest

Describe 'Development gates workflow contract' {
    BeforeAll {
        $script:RepoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:WorkflowPath = Join-Path $script:RepoRoot '.github\workflows\development-gates.yml'
    }

    It 'exists and covers the active non-mutating development gates' {
        $script:WorkflowPath | Should -Exist
        $workflow = Get-Content -Raw -LiteralPath $script:WorkflowPath

        $workflow | Should -Match 'name:\s*Development Gates'
        $workflow | Should -Match 'dotnet-tests:'
        $workflow | Should -Match 'dotnet test src/DesktopNode\.sln -c Release --no-restore'
        $workflow | Should -Match 'npm test --prefix web'
        $workflow | Should -Match 'npm run verify:parity --prefix web'
        $workflow | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/tests'"
        $workflow | Should -Match "Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests'"
        $workflow | Should -Match "Invoke-Pester -Path 'web/tests'"
        $workflow | Should -Not -Match 'msiexec|Start-VM|New-VM|AdminSmokePackage|FullAdminHostMutationGate|SignTool|Create-Release|deploy'
    }
}
```

- [ ] **Step 2: Confirm the workflow contract is RED**

```powershell
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
```

Expected: failure because `.github/workflows/development-gates.yml` does not exist.

- [ ] **Step 3: Implement the workflow**

Create `.github/workflows/development-gates.yml` with:

```yaml
name: Development Gates

on:
  pull_request:
  push:
    branches:
      - main
      - 'codex/**'
  workflow_dispatch:

permissions:
  contents: read

concurrency:
  group: development-gates-${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  dotnet-tests:
    runs-on: windows-latest
    timeout-minutes: 30
    steps:
      - uses: actions/checkout@v6.0.2
      - uses: actions/setup-dotnet@v5
        with:
          dotnet-version: 10.0.x
      - uses: actions/cache@v5
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('src/**/*.csproj') }}
      - run: dotnet restore src/DesktopNode.sln
      - run: dotnet test src/DesktopNode.sln -c Release --no-restore

  web-tests:
    runs-on: ubuntu-latest
    timeout-minutes: 20
    steps:
      - uses: actions/checkout@v6.0.2
      - uses: actions/setup-node@v6
        with:
          node-version: 24
          cache: npm
          cache-dependency-path: web/package-lock.json
      - run: npm ci --prefix web
      - run: npm test --prefix web
      - run: npm run verify:parity --prefix web

  packaging-pester:
    runs-on: windows-latest
    timeout-minutes: 30
    steps:
      - uses: actions/checkout@v6.0.2
      - name: Install exact Pester
        shell: pwsh
        run: Install-Module Pester -RequiredVersion 5.7.1 -Scope CurrentUser -Force -SkipPublisherCheck
      - name: Run packaging Pester
        shell: pwsh
        run: |
          $result = Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru
          if ($result.FailedCount -gt 0) { exit 1 }

  installer-web-pester:
    runs-on: windows-latest
    timeout-minutes: 30
    steps:
      - uses: actions/checkout@v6.0.2
      - name: Install exact Pester
        shell: pwsh
        run: Install-Module Pester -RequiredVersion 5.7.1 -Scope CurrentUser -Force -SkipPublisherCheck
      - name: Run installer and Web Pester
        shell: pwsh
        run: |
          $result = Invoke-Pester -Path @('packaging/windows-desktop-node/installer/tests', 'web/tests') -Output Detailed -PassThru
          if ($result.FailedCount -gt 0) { exit 1 }
```

Do not edit or weaken `public-boundary.yml`; both workflows must run on the branch.

- [ ] **Step 4: Run the contract and full non-mutating local gates**

```powershell
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
dotnet test src/DesktopNode.sln -c Release
npm ci --prefix web
npm test --prefix web
npm run verify:parity --prefix web
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'web/tests' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
git diff --check
```

Expected: every command exits `0`; xUnit and Pester report zero failures; npm parity reports no generated/static drift; `git diff --check` emits no output.

- [ ] **Step 5: Commit the CI contract**

```powershell
git add .github/workflows/development-gates.yml packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1
git commit -m "ci: add non-mutating development gates"
```

## Task 5: Record policy, evidence, and audit closure

**Files:**

- Modify: `docs/DEVELOPMENT_VERIFICATION_POLICY.md`
- Modify: `docs/DEVELOPER_INDEX.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md`
- Create: `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md`
- Modify and begin tracking: `docs/project-status-audit-2026-07-13.md`

- [ ] **Step 1: Add the current development-gate policy without changing release anchors**

At the top of `DEVELOPMENT_VERIFICATION_POLICY.md`, add a dated section stating:

- `dotnet test src/DesktopNode.sln -c Release` is required from a non-administrator Windows checkout;
- unit tests inject a missing CLI default-token path and a recording Host ACL hardener;
- production still defaults to `%ProgramData%\PureCVisor\desktop-node\api-token.dpapi.json` and preserves Administrators/SYSTEM-only ACL hardening;
- the four `Development Gates` jobs are required on PRs and pushes;
- these jobs are non-mutating and do not replace elevated installed smoke;
- installed/package anchor stays `0.42.59-admin-smoke`; `0.42.60-admin-smoke` needs separate approval.

- [ ] **Step 2: Make the new evidence discoverable**

Add a concise current entry to both `DEVELOPER_INDEX.md` and `docs/ga-ready/EVIDENCE_INDEX.md` linking:

```text
docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md
```

Do not rewrite the historical `0.42.59` hashes, provenance commit, or manual-admin closure.

- [ ] **Step 3: Create truthful code-level evidence from the completed local runs**

The evidence document must record:

```yaml
status: code-level-local-pass
scope: non-admin-dotnet-test-isolation-and-development-ci
product_payload_change: true-code-level-host-cli-testability-and-cli-error-contract
host_mutation_performed: false
package_build_performed: false
installed_anchor: 0.42.59-admin-smoke
next_installed_candidate: 0.42.60-admin-smoke-separate-approval-required
public_trusted_signing: not-claimed
external_stable_publication: not-claimed
remote_ci_status: not-run-awaiting-authorized-push
```

Below the metadata, record each exact local command, its exit code `0`, the observed passed/failed counts, the CLI error-code contract, the Host hardener call contract, and the workflow job matrix. Do not claim remote CI PASS before a run exists.

- [ ] **Step 4: Append a dated closure addendum to the audit report**

Preserve the original audit snapshot and append an addendum that distinguishes:

- resolved locally: 19 environment-coupled .NET failures;
- newly enforced: active non-mutating development workflow;
- still open: authorized remote CI observation;
- unchanged: 45-day installed-evidence staleness, internal-only release boundary, and `0.42.59` installed anchor;
- excluded from this slice: package build, full admin host mutation, and manual-admin package-pair campaign.

- [ ] **Step 5: Run documentation and diff guards once**

```powershell
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
git diff --check
```

Expected: zero Pester failures and no diff-check output.

- [ ] **Step 6: Perform the single main-agent review**

Review only the accumulated branch diff:

```powershell
git diff --stat HEAD~4
git diff HEAD~4 -- src/DesktopNode.Cli src/DesktopNode.Cli.Tests src/DesktopNode.Host src/DesktopNode.Host.Tests .github/workflows/development-gates.yml packaging/windows-desktop-node/tests/PcvDevelopmentGateWorkflow.Tests.ps1 docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md docs/project-status-audit-2026-07-13.md
```

Review criteria: production default and ACL policy unchanged; every test-only seam is internal or optional; error text is redacted; all token-writing routes receive the hardener; workflow contains no mutation path; evidence claims match executed commands. Fix only concrete findings, then run the directly affected test once.

- [ ] **Step 7: Commit documentation and local evidence**

```powershell
git add docs/DEVELOPMENT_VERIFICATION_POLICY.md docs/DEVELOPER_INDEX.md docs/ga-ready/EVIDENCE_INDEX.md docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md docs/project-status-audit-2026-07-13.md
git commit -m "docs: record development gate recovery"
```

Expected: the previously untracked audit report becomes tracked in this documentation commit; no package artifact or installed evidence is staged.

## Task 6: Authorized remote CI proof

**Files:**

- Modify after observed runs: `docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md`
- Optionally update the remote-CI line only: `docs/project-status-audit-2026-07-13.md`

- [ ] **Step 1: Stop and request explicit authorization to push**

Report the local commit list and verification results. Do not push, create a PR, or write GitHub state until the user authorizes it.

- [ ] **Step 2: Push the existing branch after authorization**

```powershell
git push -u origin codex/development-gate-recovery-design
```

Expected: the branch push succeeds and triggers both `Development Gates` and `Public Boundary Contract`.

- [ ] **Step 3: Observe both workflow families to terminal success**

```powershell
gh run list --branch codex/development-gate-recovery-design --limit 10
gh run watch --exit-status
```

Use `gh run view --json databaseId,headSha,status,conclusion,jobs,url` for each relevant run. Required terminal evidence:

- `dotnet-tests`: success;
- `web-tests`: success;
- `packaging-pester`: success;
- `installer-web-pester`: success;
- `public-boundary-ci-required`: success.

If a job fails, diagnose once from the failing step, make one scoped correction, rerun its local equivalent, and push one correction commit. Do not loop indefinitely.

- [ ] **Step 4: Replace the truthful pending state with observed remote evidence**

Update the evidence metadata to `remote_ci_status: pass` and record the actual head SHA, workflow run IDs, job IDs, conclusions, and URLs returned by GitHub. Update the audit addendum's remote-CI line to closed. Never invent IDs or reuse the historical `0.42.59` public-boundary run as proof for this branch.

- [ ] **Step 5: Commit and push the evidence closure**

```powershell
git add docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md docs/project-status-audit-2026-07-13.md
git commit -m "docs: close development gate CI evidence"
git push
```

Expected: the documentation-only follow-up triggers both workflows again.

- [ ] **Step 6: Verify the final head, not only the predecessor**

Observe the two workflows for the new documentation commit and require terminal success again. Do not create another evidence-only commit merely to embed that commit's own run ID: the repository evidence records the verified implementation-head run, while the final handoff records the verified evidence-closure head SHA and workflow URLs. This ends the self-referential evidence chain after one closure commit.

## Task 7: Final local and boundary verification

**Files:** all files changed by Tasks 1–6.

- [ ] **Step 1: Run the final local gate once**

```powershell
dotnet test src/DesktopNode.sln -c Release
npm test --prefix web
npm run verify:parity --prefix web
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/tests' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'packaging/windows-desktop-node/installer/tests' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
pwsh -NoProfile -Command "$result = Invoke-Pester -Path 'web/tests' -Output Detailed -PassThru; if ($result.FailedCount -gt 0) { exit 1 }"
git diff --check
```

Expected: all commands exit `0`, all test suites report zero failures, and the diff check emits no output.

- [ ] **Step 2: Prove the package boundary stayed closed**

```powershell
git status --short --branch
git log --oneline --decorate -8
rg -n "installed_anchor: 0\.42\.59-admin-smoke|package_build_performed: false|host_mutation_performed: false|0\.42\.60-admin-smoke-separate-approval-required" docs/ga-ready/evidence/development-gate-recovery-code-level-2026-07-13.md
```

Expected: the worktree is clean; the evidence has all four boundary markers; no new MSI, update ZIP, admin-smoke artifact, full-admin-host-mutation evidence, or manual-admin campaign exists in the branch diff.

- [ ] **Step 3: Handoff the result**

Report:

- local gate outcome and exact zero-failure counts;
- remote workflow URLs and conclusions if Task 6 was authorized;
- stable CLI error codes added;
- production ACL and ProgramData defaults preserved;
- `host_mutation_performed=false`, `package_build_performed=false`;
- installed anchor still `0.42.59-admin-smoke`;
- separate next decision: whether to authorize an `0.42.60-admin-smoke` installed/package validation chain.

Do not describe this code-level slice as public trusted signing, external stable publication, or installed-host validation.
