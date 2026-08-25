# Phase 1 Account/noVNC Operator Surface Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Phase 1 Account/noVNC Operator Surface를 제품 payload로 승격한다. API, CLI, TUI, Web Console이 같은 Console Access Card 계약을 읽고, 설치본 account login/noVNC/current-card evidence로 닫는다.

**Architecture:** `DesktopNode.Api`가 console capability/session payload에 `console_access` 공통 projection을 추가한다. CLI, TUI, Web Console은 기존 noVNC 동작과 account/RBAC/JWT 모델을 바꾸지 않고 이 projection을 표시한다. noVNC bridge는 계속 opt-in이며 target host/port 설정, 계정 권한, 토큰 저장 정책은 동결한다.

**Tech Stack:** .NET 10 C#, xUnit, static TypeScript served asset, Node `npm test`, PowerShell packaging/manual-admin smoke, Hyper-V/noVNC installed evidence.

---

## 1. 범위 잠금

- [ ] `docs/superpowers/specs/2026-05-25-purecvisor-desktop-node-extension-roadmap-design.md`, `docs/superpowers/specs/2026-05-25-purecvisor-desktop-node-extension-domain-architecture.md`, `docs/superpowers/specs/2026-05-25-purecvisor-desktop-node-phase1-account-novnc-design-review.md`를 다시 읽고 Phase 1만 구현한다.
- [ ] 이번 slice에 포함한다: `console_access` API projection, `pcvcli vm console|vnc` 출력 제품화, `pcvtui` selected VM console/noVNC smoke snapshot, Web Console Account/Console 카드 정렬, installed account/noVNC/current-card evidence 갱신.
- [ ] 이번 slice에서 제외한다: Guest Exec/Guest Channel, Hyper-V QoS mutation, Web/TUI direct mutation 버튼, account/RBAC/JWT 재설계, noVNC target host/port 자동 설정, Linux Single Runtime Object 계열.
- [ ] 제품화 원칙을 고정한다: 기존 `Open selected console` handoff는 유지하고, noVNC 설정/권한/credential을 바꾸는 새 버튼은 추가하지 않는다.

## 2. API 계약 테스트 작성

- [ ] `src/DesktopNode.Api.Tests/ApiAccountAuthRequestProcessorTests.cs`에 capability projection 테스트를 먼저 추가한다.

```csharp
[Fact]
public void ConsoleCapabilityRouteIncludesConsoleAccessCardProjection()
{
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: TestAuthOptions("operator", "operator"));
    var token = Login(processor, "operator");

    var response = processor.Handle(new DesktopNodeApiRequest(
        "GET",
        "/api/v1/console/capabilities",
        Authorization: $"Bearer {token.AccessToken}"));

    Assert.Equal(200, response.StatusCode);
    using var document = JsonDocument.Parse(response.Body);
    var card = document.RootElement.GetProperty("data").GetProperty("console_access");
    Assert.Equal("console-access-card.v1", card.GetProperty("contract").GetString());
    Assert.Equal("console.view", card.GetProperty("account").GetProperty("required_permission").GetString());
    Assert.Equal("vmconnect", card.GetProperty("windows_console").GetProperty("type").GetString());
    Assert.Equal("not_configured", card.GetProperty("novnc").GetProperty("status").GetString());
    Assert.Equal("Use local vmconnect handoff; configure noVNC bridge only when browser streaming is required.", card.GetProperty("next_action").GetString());
    Assert.False(card.GetProperty("host_mutation_performed").GetBoolean());
}
```

- [ ] 같은 파일의 `ConsoleRoutesExposeConfiguredNoVncBridgeSessionPath`에 session projection assertion을 추가한다.

```csharp
var card = sessionData.GetProperty("console_access");
Assert.Equal("console-access-card.v1", card.GetProperty("contract").GetString());
Assert.Equal("available", card.GetProperty("novnc").GetProperty("status").GetString());
Assert.Equal("/api/v1/console/novnc/alpha", card.GetProperty("novnc").GetProperty("websocket_path").GetString());
Assert.Equal("Open the noVNC browser session for this VM, or use vmconnect from the host console.", card.GetProperty("next_action").GetString());
Assert.False(session.Body.Contains("correct horse battery staple", StringComparison.OrdinalIgnoreCase));
```

- [ ] 테스트를 실패 상태로 확인한다.

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "ConsoleCapabilityRouteIncludesConsoleAccessCardProjection|ConsoleRoutesExposeConfiguredNoVncBridgeSessionPath" --no-restore
```

## 3. API projection 구현

- [ ] `src/DesktopNode.Api/DesktopNodeApiConsoleAccessProjection.cs`를 새로 만든다.

```csharp
namespace DesktopNode.Api;

internal static class DesktopNodeApiConsoleAccessProjection
{
    public const string Contract = "console-access-card.v1";
    private const string RequiredPermission = "console.view";

    public static SortedDictionary<string, object?> ForCapabilities(DesktopNodeConsoleOptions options)
    {
        return Build(options, websocketPath: null);
    }

    public static SortedDictionary<string, object?> ForSession(DesktopNodeConsoleOptions options, string? websocketPath)
    {
        return Build(options, websocketPath);
    }

    private static SortedDictionary<string, object?> Build(DesktopNodeConsoleOptions options, string? websocketPath)
    {
        var noVncEnabled = options.NoVncEnabled;
        var noVncStatus = noVncEnabled ? "available" : "not_configured";

        return new SortedDictionary<string, object?>
        {
            ["contract"] = Contract,
            ["account"] = new SortedDictionary<string, object?>
            {
                ["required_permission"] = RequiredPermission,
                ["auth_surface"] = "service-token-or-account-jwt",
                ["token_output"] = "redacted"
            },
            ["windows_console"] = new SortedDictionary<string, object?>
            {
                ["available"] = options.Enabled,
                ["type"] = "vmconnect",
                ["transport"] = "local-handoff"
            },
            ["novnc"] = new SortedDictionary<string, object?>
            {
                ["enabled"] = noVncEnabled,
                ["status"] = noVncStatus,
                ["bridge_mode"] = options.NoVncBridgeMode,
                ["websocket_path_template"] = noVncEnabled ? options.NoVncWebSocketPath : null,
                ["websocket_path"] = websocketPath,
                ["reason"] = noVncEnabled ? "noVNC bridge is configured." : "Windows VNC/WebSocket bridge is not configured."
            },
            ["status"] = noVncEnabled ? "browser-streaming-available" : "local-console-handoff-ready",
            ["next_action"] = noVncEnabled
                ? "Open the noVNC browser session for this VM, or use vmconnect from the host console."
                : "Use local vmconnect handoff; configure noVNC bridge only when browser streaming is required.",
            ["host_mutation_performed"] = false
        };
    }
}
```

- [ ] `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs`에서 `BuildConsoleCapabilities()`와 `BuildVmConsoleSession(string vmId)`에 `console_access`를 추가한다.

```csharp
["console_access"] = DesktopNodeApiConsoleAccessProjection.ForCapabilities(consoleOptions)
```

```csharp
["console_access"] = DesktopNodeApiConsoleAccessProjection.ForSession(consoleOptions, FormatNoVncWebSocketPath(vmId))
```

- [ ] 기존 `windows_console`, `novnc`, `console`, `required_permission`, `host_mutation_performed` 필드는 유지한다. 기존 소비자 호환성을 깨지 않는다.
- [ ] API 테스트를 통과시킨다.

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter "ConsoleCapabilityRouteDeclaresWindowsHandoffAndGatedNoVnc|ConsoleCapabilityRouteIncludesConsoleAccessCardProjection|ConsoleRoutesExposeConfiguredNoVncBridgeSessionPath|ViewerJwtCannotOpenVmConsoleSession" --no-restore
```

## 4. CLI 출력 제품화

- [ ] `src/DesktopNode.Cli.Tests/DesktopNodeCliApplicationTests.cs`에 `pcvcli vm console` table 출력 테스트를 추가한다.

```csharp
[Fact]
public async Task VmConsoleTableRendersConsoleAccessCard()
{
    var transport = new RecordingTransport(new DesktopNodeCliTransportResponse(
        200,
        "application/json",
        """
        {"ok":true,"operation":"console.session","request_id":"req-console","data":{
          "vm_id":"alpha",
          "console_access":{
            "contract":"console-access-card.v1",
            "account":{"required_permission":"console.view","auth_surface":"service-token-or-account-jwt","token_output":"redacted"},
            "windows_console":{"available":true,"type":"vmconnect","transport":"local-handoff"},
            "novnc":{"enabled":true,"status":"available","bridge_mode":"websocket-to-vnc-tcp","websocket_path":"/api/v1/console/novnc/alpha","reason":"noVNC bridge is configured."},
            "status":"browser-streaming-available",
            "next_action":"Open the noVNC browser session for this VM, or use vmconnect from the host console.",
            "host_mutation_performed":false
          }
        },"error":null}
        """));

    var result = await DesktopNodeCliApplication.RunAsync(
        ["--no-color", "vm", "console", "alpha"],
        transport,
        environment: _ => null,
        cancellationToken: CancellationToken.None);

    Assert.Equal(0, result.ExitCode);
    Assert.Contains("Console Access", result.StandardOutput, StringComparison.Ordinal);
    Assert.Contains("account.permission", result.StandardOutput, StringComparison.Ordinal);
    Assert.Contains("console.view", result.StandardOutput, StringComparison.Ordinal);
    Assert.Contains("novnc.websocket_path", result.StandardOutput, StringComparison.Ordinal);
    Assert.Contains("/api/v1/console/novnc/alpha", result.StandardOutput, StringComparison.Ordinal);
    Assert.DoesNotContain("ok=True | operation=console.session", result.StandardOutput, StringComparison.Ordinal);
}
```

- [ ] `src/DesktopNode.Cli/DesktopNodeCliFormatter.cs`에 `TryFormatConsoleSessionTable`을 추가하고 `FormatTable()`에서 `TryParseOkSummary()`보다 먼저 호출한다.
- [ ] 출력 row는 다음 key를 고정한다: `vm.id`, `account.permission`, `windows_console.type`, `windows_console.transport`, `novnc.status`, `novnc.bridge_mode`, `novnc.websocket_path`, `status`, `next_action`, `host_mutation_performed`.
- [ ] JSON 출력은 원본 payload를 그대로 반환한다. plain/csv는 기존 fallback을 유지한다.
- [ ] CLI 테스트를 통과시킨다.

```powershell
dotnet test src/DesktopNode.Cli.Tests/DesktopNode.Cli.Tests.csproj --filter "VmConsoleTableRendersConsoleAccessCard|RuntimePolicyTableRendersPolicyFields|NetworkInventoryTableRendersSwitchRows" --no-restore
```

## 5. TUI selected VM console/noVNC snapshot

- [ ] `src/DesktopNode.Tui.Tests/TuiApiClientTests.cs`에 `FetchVmConsoleAsync` route mapping 테스트를 추가한다.

```csharp
[Fact]
public async Task FetchVmConsoleAsyncMapsToReadOnlyConsoleRoute()
{
    var transport = new RecordingTransport();
    var client = NewClient(transport);

    await client.FetchVmConsoleAsync("vm 1", CancellationToken.None);

    Assert.Equal("GET", transport.LastRequest?.Method);
    Assert.Equal("/api/v1/vms/vm%201/console", transport.LastRequest?.Path);
    Assert.Null(transport.LastRequest?.Body);
}
```

- [ ] `src/DesktopNode.Tui/TuiApiRoutes.cs`에 `VmConsole(string vmId)`를 추가하고 `src/DesktopNode.Tui/TuiApiClient.cs`에 `FetchVmConsoleAsync`를 추가한다.
- [ ] `src/DesktopNode.Tui/TuiState.cs`에 `TuiVmConsoleAccessState`와 `ApplyVmConsoleAccessSnapshot()` / `ClearVmConsoleAccess()`를 추가한다. 기존 `VmReadback` state는 그대로 둔다.
- [ ] `src/DesktopNode.Tui/TuiApplication.cs`에서 `--smoke-once vm`일 때 VM 목록 refresh 후 selected row가 있으면 `FetchVmConsoleAsync(selectedVmId)`를 한 번 호출한다. 실패 시 exit code는 기존 smoke failure 정책을 따른다.
- [ ] `src/DesktopNode.Tui/TuiRenderer.cs`에 `CONSOLE ACCESS` panel을 VM surface에 추가한다. 표시 필드는 `vm`, `account.permission`, `windows_console.type`, `novnc.status`, `novnc.websocket_path`, `next_action`이다.
- [ ] `src/DesktopNode.Tui.Tests/TuiRendererTests.cs`에 projection render 테스트를 추가한다.
- [ ] `src/DesktopNode.Tui.Tests/TuiApplicationTests.cs`에 `SmokeOnceVmIncludesSelectedConsoleAccessSnapshot` 테스트를 추가한다. 예상 요청 순서는 `/api/v1/vms`, `/api/v1/vms/<selected>/console`이다.
- [ ] TUI 테스트를 통과시킨다.

```powershell
dotnet test src/DesktopNode.Tui.Tests/DesktopNode.Tui.Tests.csproj --filter "FetchVmConsoleAsyncMapsToReadOnlyConsoleRoute|SmokeOnceVmIncludesSelectedConsoleAccessSnapshot|RendersSelectedVmQosGuestReadbackPanel" --no-restore
```

## 6. Web Console Account/Console 카드 정렬

- [ ] `web/scripts/verify-browser-fixture.mjs`의 `/api/v1/console/capabilities`와 `/api/v1/vms/pcv-browser-fixture/console` fixture payload에 `console_access`를 추가한다.
- [ ] `web/src/served-app.ts`의 `renderConsolePanel()`에서 `capabilities.console_access`와 `state.consoleSession.console_access`를 우선 사용한다. 기존 payload만 있는 경우 기존 `windows_console`/`novnc` fallback을 유지한다.
- [ ] 카드 문구를 짧게 고정한다: account permission, current role, Windows console, noVNC status, noVNC path/reason, next action.
- [ ] `Open selected console` handoff 버튼은 유지한다. noVNC 설정 변경, credential 입력, 권한 변경 버튼은 추가하지 않는다.
- [ ] `web/app.js`는 직접 수정하지 않고 served build script로 갱신한다.

```powershell
Push-Location web
npm run build:served
npm test
Pop-Location
```

- [ ] fixture 검증에서 account session, console capability, console session이 모두 redaction-safe로 통과하는지 확인한다.

```powershell
Push-Location web
npm run browser:fixture
Pop-Location
```

## 7. 통합 검증

- [ ] 전체 .NET 테스트를 실행한다.

```powershell
dotnet test src/DesktopNode.sln --no-restore
```

- [ ] Web served asset parity를 다시 확인한다.

```powershell
Push-Location web
npm run check:served
npm run verify:parity
Pop-Location
```

- [ ] 문서와 whitespace 검증을 실행한다.

```powershell
git diff --check
git status --short
```

## 8. 패키지와 설치본 evidence

- [ ] product payload 변경이므로 `0.42.45-admin-smoke` package chain을 연다. 현재 latest package anchor가 `0.42.44-admin-smoke`이므로 target은 `0.42.45-admin-smoke`다.

```powershell
packaging/windows-desktop-node/installer/build.ps1 -Version 0.42.45-admin-smoke -OutputRoot artifacts/admin-smoke-package-20260526-04245 -SigningMode AllowUnsignedDev -SigningTrustModel Unspecified -WixPath C:\Users\Operator\.dotnet\tools\wix.exe
```

- [ ] 설치본 CLI/TUI/Web smoke를 새 payload 기준으로 실행한다.

```powershell
packaging/windows-desktop-node/tools/Invoke-PcvInstalledAccountLoginSmoke.ps1 -ArtifactRoot artifacts/installed-account-login-smoke-20260526-04245
packaging/windows-desktop-node/tools/Invoke-PcvTargetBackedNoVncInstalledStreamingSmoke.ps1 -ArtifactRoot artifacts/target-backed-novnc-installed-streaming-smoke-20260526-04245
packaging/windows-desktop-node/tools/Invoke-PcvInstalledTuiOperatorSmoke.ps1 -ArtifactRoot artifacts/installed-tui-console-access-smoke-20260526-04245 -InitialTab vm
```

- [ ] installed CLI smoke에는 다음 명령을 포함한다.

```powershell
pcvcli --no-color vm console <existing-vm>
pcvcli --json vm vnc <existing-vm>
pcvcli --no-color ops summary
```

- [ ] full admin host mutation gate를 새 package 기준으로 승격한다.
- [ ] `0.42.44-admin-smoke -> 0.42.45-admin-smoke` manual-admin package-pair descriptor/readiness/update-rollback/clean-host/Burn/MSIX/ops-summary/descriptor closure를 실행한다.
- [ ] `docs/ga-ready/evidence/admin-smoke-package-2026-05-26-04245.md`, `docs/ga-ready/evidence/installed-account-novnc-operator-surface-smoke-2026-05-26-04245.md`, `docs/ga-ready/evidence/installed-operator-surface-current-card-2026-05-26-04245.md`, `docs/ga-ready/evidence/manual-admin-campaign-2026-05-26-04244-04245.md`를 작성한다.
- [ ] `docs/ga-ready/CONTROL_PLANE_INDEX.md`, `docs/DEVELOPER_INDEX.md`, `docs/ga-ready/CURRENT_EVIDENCE_LEDGER.md`의 current anchor를 `0.42.45-admin-smoke`로 갱신한다.

## 9. 완료 기준

- [ ] `GET /api/v1/console/capabilities`와 `GET /api/v1/vms/{vm}/console`이 `console_access.contract=console-access-card.v1`을 반환한다.
- [ ] `pcvcli vm console <vm>` table 출력이 실제 VM console/noVNC 상태를 보여 주며 `ok=True | operation=...` fallback으로 떨어지지 않는다.
- [ ] `pcvtui --smoke-once vm` 출력에 `CONSOLE ACCESS`, selected VM, noVNC status/path 또는 reason이 포함된다.
- [ ] Web Console Account/Console panel이 current role과 console/noVNC status/next action을 같은 카드 흐름으로 보여 준다.
- [ ] installed account login smoke와 target-backed noVNC streaming smoke가 PASS다.
- [ ] current-card/ops summary evidence가 새 installed account/noVNC evidence anchor를 가리킨다.
- [ ] account password, bearer token, refresh/access token, protected token path 값이 CLI/TUI/Web/evidence stdout에 노출되지 않는다.

## 10. 커밋 순서

- [ ] Commit 1: `test: cover console access card contract`
- [ ] Commit 2: `feat: expose console access card projection`
- [ ] Commit 3: `feat: render console access across operator surfaces`
- [ ] Commit 4: `docs: record account novnc operator surface evidence`

각 commit 전에 관련 테스트를 다시 실행하고 `git diff --check`를 통과시킨다.

## GSTACK REVIEW REPORT

| Review | Trigger | Why | Runs | Status | Findings |
|--------|---------|-----|------|--------|----------|
| CEO Review | `/plan-ceo-review` | Scope & strategy | 0 | skipped | Phase 1 범위는 roadmap/design review에서 이미 account/noVNC로 고정 |
| Codex Review | `codex review` | Independent 2nd opinion | 0 | not-run | implementation diff 생성 전 |
| Eng Review | `/plan-eng-review` | Architecture & tests (required) | 1 | clear | 1 issue found, 0 critical gaps; package/evidence artifact date corrected to 2026-05-26 |
| Design Review | `/plan-design-review` | UI/UX gaps | 1 | clear | score: 6/10 -> 9/10, Console Access Card decisions accepted |
| DX Review | `/plan-devex-review` | Developer experience gaps | 0 | skipped | CLI/TUI/Web operator smoke가 implementation acceptance에 포함됨 |

- **UNRESOLVED:** 0
- **VERDICT:** DESIGN + ENG CLEARED - ready to implement with `superpowers:subagent-driven-development`.
