# PCVCLI Diagnostics List and Console Capabilities Boundary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `pcvcli diagnostics bundle list [--limit N] [--offset N]` while keeping global `console capabilities` explicitly API/Web Console-only.

**Architecture:** Extend the existing `DesktopNodeCliCommandCatalog` diagnostics branch with a read-only list request that reuses the `job list` pagination pattern and the common CLI response renderer. Add the command to interactive help/completion, then synchronize the four operator documents and their executable contract test. Do not change API handlers, schemas, authorization, retention logic, or add a top-level `console` command.

**Tech Stack:** .NET/C# 12, xUnit, PowerShell/Pester, Markdown documentation

---

## File map

- Modify `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs`: route `diagnostics bundle list`, parse pagination, and publish exact usage.
- Modify `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs`: add the list command to completion and help rows.
- Modify `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`: pin no-query/query routing, integer validation, usage, and the absent global console command.
- Modify `src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs`: pin help and completion discovery.
- Modify `src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs`: turn the former two-API-only documentation assertion into the approved asymmetric surface contract.
- Modify `docs/CLI_COMMAND_USAGE.md`: document the list command, pagination, retention side effect, and API/Web-only console discovery.
- Modify `docs/USER_GUIDE.md`: add the operator CLI listing flow and clarify global versus per-VM console ownership.
- Modify `docs/USER_FEATURE_USAGE_SPEC.md`: update the feature matrix and detailed diagnostics/console contracts.
- Modify `src/DesktopNode.Cli/README.md`: update the supported command inventory and discovery-route classification.
- Keep `src/DesktopNode.Api/**` and `src/DesktopNode.Api.Tests/**` unchanged; existing tests remain regression coverage.

The four documentation files and `DesktopNodeCliProjectContractTests.cs` already contain uncommitted advanced-CLI synchronization work. Preserve those edits and patch only the listed discovery sections; do not reset or replace either file wholesale.

### Task 1: Add failing command-catalog contracts

**Files:**
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs:8-50`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs:439-470`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs:523-538`

- [ ] **Step 1: Pin the no-query diagnostics list route**

Add the following theory row immediately before the existing diagnostics create row:

```csharp
[InlineData("diagnostics bundle list", "GET", "/api/v1/diagnostics/bundles")]
```

- [ ] **Step 2: Pin deterministic pagination and invalid integers**

Add these tests after `BuildsJobListPaginationQuery`:

```csharp
[Fact]
public void BuildsDiagnosticBundleListPaginationQuery()
{
    var request = DesktopNodeCliCommandCatalog.CreateRequest([
        "diagnostics",
        "bundle",
        "list",
        "--limit",
        "25",
        "--offset",
        "50"
    ]);

    Assert.Equal("GET", request.Method);
    Assert.Equal("/api/v1/diagnostics/bundles?limit=25&offset=50", request.Path);
}

[Theory]
[InlineData("--limit", "many")]
[InlineData("--offset", "later")]
public void RejectsNonIntegerDiagnosticBundlePagination(string option, string value)
{
    var error = Assert.Throws<ArgumentException>(() =>
        DesktopNodeCliCommandCatalog.CreateRequest([
            "diagnostics",
            "bundle",
            "list",
            option,
            value
        ]));

    Assert.Contains("PCV_CLI_USAGE", error.Message, StringComparison.Ordinal);
    Assert.Contains($"Option {option} must be an integer.", error.Message, StringComparison.Ordinal);
}
```

- [ ] **Step 3: Pin usage and the intentional console omission**

Add these assertions to `UsageShowsPcvCliCommandName`:

```csharp
Assert.Contains("pcvcli diagnostics bundle list [--limit N] [--offset N]", usage, StringComparison.Ordinal);
Assert.Contains("pcvcli diagnostics bundle create", usage, StringComparison.Ordinal);
Assert.Contains("pcvcli diagnostics bundle download <bundle_id> --output <path>", usage, StringComparison.Ordinal);
Assert.DoesNotContain("pcvcli console capabilities", usage, StringComparison.Ordinal);
```

Add this focused boundary test after `UsageShowsPcvCliCommandName`:

```csharp
[Fact]
public void KeepsGlobalConsoleCapabilitiesOutOfCliCatalog()
{
    var error = Assert.Throws<ArgumentException>(() =>
        DesktopNodeCliCommandCatalog.CreateRequest(["console", "capabilities"]));

    Assert.Contains("PCV_CLI_USAGE", error.Message, StringComparison.Ordinal);
    Assert.Contains("Unknown command group 'console'", error.Message, StringComparison.Ordinal);
}
```

- [ ] **Step 4: Run the focused tests and observe RED**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliCommandCatalogTests"
```

Expected: FAIL because `diagnostics bundle list` still returns the old `create|download` usage error and the usage text does not contain the new command. The console-omission test should already pass.

### Task 2: Implement diagnostics list routing

**Files:**
- Modify: `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs:614-642`
- Modify: `src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs:760-786`
- Test: `src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs`

- [ ] **Step 1: Register `list` and update diagnostics usage errors**

Replace `DiagnosticsRequest` with:

```csharp
private static DesktopNodeCliRequest DiagnosticsRequest(IReadOnlyList<string> args)
{
    if (args.Count < 3 || !Is(args[1], "bundle"))
    {
        throw Usage("Use: diagnostics bundle list|create|download.");
    }

    return args[2].ToLowerInvariant() switch
    {
        "list" => DiagnosticsList(args),
        "create" => Fixed(args, 3, new DesktopNodeCliRequest("POST", "/api/v1/diagnostics/bundles"), "diagnostics bundle create"),
        "download" => DiagnosticsDownload(args),
        _ => throw Usage("Use: diagnostics bundle list|create|download.")
    };
}
```

- [ ] **Step 2: Add the pagination request builder**

Insert this method immediately before `DiagnosticsDownload`:

```csharp
private static DesktopNodeCliRequest DiagnosticsList(IReadOnlyList<string> args)
{
    var parsed = ParseOptions(args.Skip(3).ToArray(), allowFlags: false);
    var query = new List<string>();
    if (parsed.Options.TryGetValue("--limit", out var limit) && !string.IsNullOrWhiteSpace(limit))
    {
        query.Add("limit=" + ParseInt("--limit", limit));
    }

    if (parsed.Options.TryGetValue("--offset", out var offset) && !string.IsNullOrWhiteSpace(offset))
    {
        query.Add("offset=" + ParseInt("--offset", offset));
    }

    var path = "/api/v1/diagnostics/bundles" +
        (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
    return new DesktopNodeCliRequest("GET", path);
}
```

- [ ] **Step 3: Publish exact non-interactive usage lines**

Replace the single diagnostics usage line with these three entries:

```csharp
"  pcvcli diagnostics bundle list [--limit N] [--offset N]",
"  pcvcli diagnostics bundle create",
"  pcvcli diagnostics bundle download <bundle_id> --output <path>"
```

- [ ] **Step 4: Run command-catalog tests and observe GREEN**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliCommandCatalogTests"
```

Expected: PASS, including the no-query route, ordered pagination, integer errors, create/download regression, and absent global console group.

- [ ] **Step 5: Commit the routed command and tests**

```powershell
git add -- src/DesktopNode.Cli/DesktopNodeCliCommandCatalog.cs src/DesktopNode.Cli.Tests/DesktopNodeCliCommandCatalogTests.cs
git commit -m "feat(cli): expose diagnostic bundle list"
```

### Task 3: Add interactive help and completion

**Files:**
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs:70-91`
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs:169-178`
- Modify: `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs:18-67`
- Modify: `src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs:69-115`

- [ ] **Step 1: Write failing interactive discovery tests**

In `HelpListsAvailableCommandsAsSingleCommandRows`, add:

```csharp
Assert.Contains(lines, line => string.Equals(
    line.Trim(),
    "diagnostics bundle list | List diagnostic bundles",
    StringComparison.Ordinal));
```

Add this theory row to `CompletesKnownInteractiveCommandPrefixes`:

```csharp
[InlineData("diagnostics bundle l", "diagnostics bundle list")]
```

- [ ] **Step 2: Run focused interactive tests and observe RED**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliInteractiveShellTests.HelpListsAvailableCommandsAsSingleCommandRows|FullyQualifiedName~DesktopNodeCliInteractiveShellTests.CompletesKnownInteractiveCommandPrefixes"
```

Expected: FAIL because neither the help row nor completion candidate exists.

- [ ] **Step 3: Add the completion candidate and help row**

Insert this completion candidate before diagnostics create:

```csharp
"diagnostics bundle list",
```

Insert this help row before diagnostics create:

```csharp
new("diagnostics bundle list", "List diagnostic bundles"),
```

- [ ] **Step 4: Run the interactive shell tests and observe GREEN**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliInteractiveShellTests"
```

Expected: PASS. Help contains one leaf row per command, completion expands `diagnostics bundle l`, and existing snapshot-removal assertions remain green.

- [ ] **Step 5: Commit the interactive surface**

```powershell
git add -- src/DesktopNode.Cli/DesktopNodeCliInteractiveShell.cs src/DesktopNode.Cli.Tests/DesktopNodeCliInteractiveShellTests.cs
git commit -m "feat(cli): list diagnostics in interactive help"
```

### Task 4: Convert the documentation contract test to the approved boundary

**Files:**
- Modify: `src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs:67-89`

- [ ] **Step 1: Replace the former API-only test with the asymmetric contract**

Replace `DocumentsApiOnlyDiscoveryRoutesAndExistingCliHandoffs` with:

```csharp
[Fact]
public void DocumentsDiagnosticsListCliAndApiOnlyConsoleCapabilities()
{
    var usage = File.ReadAllText(FindRepoFile("docs/CLI_COMMAND_USAGE.md"));
    var featureUsage = File.ReadAllText(FindRepoFile("docs/USER_FEATURE_USAGE_SPEC.md"));
    var userGuide = File.ReadAllText(FindRepoFile("docs/USER_GUIDE.md"));
    var readme = File.ReadAllText(FindRepoFile("src/DesktopNode.Cli/README.md"));

    Assert.Contains("pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]", usage, StringComparison.Ordinal);
    Assert.Contains("GET /api/v1/diagnostics/bundles?limit=<n>&offset=<n>", usage, StringComparison.Ordinal);
    Assert.Contains("diagnostics.read", usage, StringComparison.Ordinal);
    Assert.Contains("기본 retention은", usage, StringComparison.Ordinal);
    Assert.Contains("14일 또는 최대 50개", usage, StringComparison.Ordinal);
    Assert.Contains("PCV_DIAGNOSTIC_BUNDLE_LIST_LIMIT_OUT_OF_RANGE", usage, StringComparison.Ordinal);
    Assert.DoesNotContain("현재 `pcvcli diagnostics bundle list` command는 없으며", usage, StringComparison.Ordinal);

    Assert.Contains("GET /api/v1/console/capabilities", usage, StringComparison.Ordinal);
    Assert.Contains("API/Web Console 전용", usage, StringComparison.Ordinal);
    Assert.Contains("console-access-card.v1", usage, StringComparison.Ordinal);
    Assert.Contains("console.view", usage, StringComparison.Ordinal);
    Assert.Contains("pcvcli --json vm console ubuntu-lab-01", usage, StringComparison.Ordinal);
    Assert.Contains("GUI를 자동 실행하지 않고", usage, StringComparison.Ordinal);

    Assert.Contains("| Diagnostics list |", featureUsage, StringComparison.Ordinal);
    Assert.Contains("`pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]`", featureUsage, StringComparison.Ordinal);
    Assert.Contains("| Console capability discovery |", featureUsage, StringComparison.Ordinal);
    Assert.Contains("API/Web Console 전용", featureUsage, StringComparison.Ordinal);
    Assert.Contains("pcvcli vm console/vnc <vm>", featureUsage, StringComparison.Ordinal);

    Assert.Contains("pcvcli --json diagnostics bundle list --limit 10 --offset 0", userGuide, StringComparison.Ordinal);
    Assert.Contains("전역 capability discovery는 API/Web Console 전용", userGuide, StringComparison.Ordinal);
    Assert.Contains("pcvcli diagnostics bundle list [--limit N] [--offset N]", readme, StringComparison.Ordinal);
    Assert.Contains("API/Web Console 전용 discovery card", readme, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the contract test and observe RED**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliProjectContractTests.DocumentsDiagnosticsListCliAndApiOnlyConsoleCapabilities"
```

Expected: FAIL because the current documents still classify diagnostics list as API/Web-only and omit the new command.

### Task 5: Synchronize operator documentation

**Files:**
- Modify: `docs/CLI_COMMAND_USAGE.md:390-488`
- Modify: `docs/USER_GUIDE.md:270-277`
- Modify: `docs/USER_FEATURE_USAGE_SPEC.md:51-67`
- Modify: `docs/USER_FEATURE_USAGE_SPEC.md:263-272`
- Modify: `docs/USER_FEATURE_USAGE_SPEC.md:311-322`
- Modify: `src/DesktopNode.Cli/README.md:45-60`
- Modify: `src/DesktopNode.Cli/README.md:104-109`
- Test: `src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs`

- [ ] **Step 1: Add list to the CLI command table and example flow**

In `docs/CLI_COMMAND_USAGE.md`, add this first diagnostics table row:

```markdown
| `pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]` | `GET /api/v1/diagnostics/bundles?limit=&offset=` | Diagnostic bundle metadata 목록과 pagination 조회 |
```

Replace the diagnostics example with:

```powershell
$page = pcvcli --json diagnostics bundle list --limit 10 --offset 0 | ConvertFrom-Json
$page.data.bundles | Select-Object bundle_id, created_at, size_bytes, download_url

$created = pcvcli --json diagnostics bundle create | ConvertFrom-Json
pcvcli diagnostics bundle download $created.bundle_id --output D:\evidence\$($created.bundle_id).json
```

Replace the sentence claiming the list command is absent with:

```markdown
`pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]`는 이 route를 공통 CLI 출력 형식으로 조회한다. Account/RBAC mode에서는 `diagnostics.read` permission이 필요하다.
```

Keep the existing response-field table, default `limit=10`, `offset=0`, maximum `limit=100`, retention behavior, and `PCV_DIAGNOSTIC_BUNDLE_LIST_*` error descriptions. Remove the direct `Invoke-RestMethod` list example because the supported CLI command now owns that operator flow.

- [ ] **Step 2: Make console discovery the only API/Web-only section**

Rename the old two-route section to:

```markdown
## API/Web Console 전용 discovery 기능

### Console capabilities
```

Add this ownership paragraph before the console response-field table:

```markdown
이 전역 capability discovery는 API/Web Console 전용으로 유지한다. PCVCLI는 전역 policy를 `pcvcli runtime policy`로, 실제 VM별 handoff를 `pcvcli vm console|vnc <vm>`으로 제공하므로 별도 `pcvcli console capabilities` command를 추가하지 않는다. 이는 backend 미구현이 아니라 의도적인 surface ownership 결정이다.
```

Keep the existing `console-access-card.v1`, `console.view`, direct API example, and the statement that VM console commands do not automatically launch a GUI.

- [ ] **Step 3: Update the user guide**

Append this CLI flow to the diagnostic bundle paragraph in `docs/USER_GUIDE.md`:

```markdown
CLI에서는 `pcvcli --json diagnostics bundle list --limit 10 --offset 0`으로 같은 page를 조회하고, 반환된 `bundle_id`를 `pcvcli diagnostics bundle download`에 전달한다.
```

Replace the opening of the console paragraph with:

```markdown
Console capability card의 전역 capability discovery는 API/Web Console 전용이다. `GET /api/v1/console/capabilities`로 listener의 local `vmconnect` handoff와 optional noVNC bridge 상태를 조회한다.
```

Retain the per-VM `pcvcli vm console|vnc <vm>` instructions and no-auto-launch behavior.

- [ ] **Step 4: Update the user feature usage specification**

Replace the diagnostics and console feature-matrix rows with:

```markdown
| Diagnostics list | Troubleshooting bundle 목록/pagination | `pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]` | `GET /diagnostics/bundles?limit=&offset=` |
| Diagnostics create/download | Troubleshooting create/download | `pcvcli diagnostics bundle create/download` | `POST /diagnostics/bundles`, `GET /diagnostics/bundles/{id}/download` |
| Account/RBAC/JWT | Account panel | API/Web Console auth only | `/auth/...` |
| Console capability discovery | Console/Troubleshooting capability card | API/Web Console 전용 | `GET /console/capabilities` |
| VM console/noVNC handoff | 선택 VM Console panel | `pcvcli vm console/vnc <vm>` | `GET /vms/{id}/console` |
```

Replace the diagnostics detail table CLI row with:

```markdown
| CLI | `pcvcli diagnostics bundle list [--limit <n>] [--offset <n>]`, `create`, `download <bundle_id> --output <path>` |
```

Extend the console detail table with:

```markdown
| Global discovery | `GET /console/capabilities`; API/Web Console 전용 |
| VM별 CLI | `pcvcli vm console/vnc <vm>`; session/handoff metadata 조회, 자동 GUI 실행 없음 |
```

- [ ] **Step 5: Update the CLI README**

Insert this command before diagnostics create:

```text
pcvcli diagnostics bundle list [--limit N] [--offset N]
```

Replace the discovery-route paragraph with:

```markdown
Backend discovery route 중 `GET /api/v1/diagnostics/bundles?limit=&offset=`은 `pcvcli diagnostics bundle list [--limit N] [--offset N]`으로 노출해 create/list/download 자동화 흐름을 완성한다. 조회는 server retention을 적용하므로 diagnostics root의 만료·초과 bundle이 제거될 수 있다.

`GET /api/v1/console/capabilities`는 vmconnect/noVNC/`console.view` 상태를 설명하는 API/Web Console 전용 discovery card로 유지한다. 전역 CLI command는 추가하지 않으며, 실제 VM별 console handoff는 `pcvcli vm console|vnc <vm>`으로 제공한다. 응답 필드와 surface ownership은 `docs/CLI_COMMAND_USAGE.md`를 따른다.
```

- [ ] **Step 6: Run the documentation contract test and observe GREEN**

Run:

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeCliProjectContractTests.DocumentsDiagnosticsListCliAndApiOnlyConsoleCapabilities"
```

Expected: PASS. The four documents agree that diagnostics list is a CLI command and global console capabilities is API/Web Console-only.

- [ ] **Step 7: Review the complete existing document changes before committing**

Run:

```powershell
git diff -- docs/CLI_COMMAND_USAGE.md docs/USER_FEATURE_USAGE_SPEC.md docs/USER_GUIDE.md src/DesktopNode.Cli/README.md src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs
```

Expected: the earlier advanced-CLI synchronization remains present, and the new changes only replace the outdated discovery classification.

- [ ] **Step 8: Commit the synchronized documentation contract**

```powershell
git add -- docs/CLI_COMMAND_USAGE.md docs/USER_FEATURE_USAGE_SPEC.md docs/USER_GUIDE.md src/DesktopNode.Cli/README.md src/DesktopNode.Cli.Tests/DesktopNodeCliProjectContractTests.cs
git commit -m "docs(cli): finalize diagnostics and console ownership"
```

### Task 6: Run regression and policy verification

**Files:**
- Verify: `src/DesktopNode.Cli/**`
- Verify: `src/DesktopNode.Cli.Tests/**`
- Verify: `src/DesktopNode.Api/**`
- Verify: `src/DesktopNode.Api.Tests/**`
- Verify: `packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1`
- Verify: repository diff and status

- [ ] **Step 1: Run the complete CLI suite**

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --no-restore
```

Expected: all CLI tests pass, including routing, application output, interactive shell, options, token resolver, and project contracts.

- [ ] **Step 2: Run the existing diagnostics and console API regression tests**

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~DesktopNodeApiDiagnosticsHandlerTests|FullyQualifiedName~ApiAccountAuthRequestProcessorTests|FullyQualifiedName~ApiHandlerAdapterContractTests"
```

Expected: all selected tests pass without API production changes.

- [ ] **Step 3: Run the full .NET solution**

```powershell
dotnet test src/DesktopNode.sln --no-restore
```

Expected: all solution tests pass with zero failures.

- [ ] **Step 4: Run the documentation/evidence boundary suite**

```powershell
Invoke-Pester -Path packaging/windows-desktop-node/tests/PcvAdminSmokeEvidenceDocs.Tests.ps1 -Output Detailed
```

Expected: all Pester tests pass; current 0.42.72 operational evidence and public non-claims remain unchanged.

- [ ] **Step 5: Check formatting, forbidden surface drift, and worktree state**

```powershell
git diff --check
rg -n '현재 `pcvcli diagnostics bundle list` command는 없으며|Diagnostics list.*현재 API/Web 전용' docs/CLI_COMMAND_USAGE.md docs/USER_FEATURE_USAGE_SPEC.md docs/USER_GUIDE.md src/DesktopNode.Cli/README.md
rg -n 'pcvcli console capabilities' src/DesktopNode.Cli
git status --short
```

Expected:

- `git diff --check` exits 0.
- The stale diagnostics/API-only phrases have zero matches outside the approved design/plan history.
- Production CLI source contains no `pcvcli console capabilities` usage line; the command-catalog test proves the top-level group remains unavailable.
- No unrelated untracked files are staged or modified.

- [ ] **Step 6: Record the package boundary without starting host mutation**

Document in the handoff that the CLI source change is a product-payload change and therefore makes the next package candidate required by release policy. Do not build/install an MSI, mutate the service/host, or advance current operational evidence in this implementation task.
