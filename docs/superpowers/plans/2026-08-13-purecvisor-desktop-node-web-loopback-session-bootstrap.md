# Web Loopback Session Bootstrap Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 기본 `no-default-account` 설치의 loopback Web Console이 service token을 HTML/`pcv-config.js`에 넣지 않고 짧은 JWT session으로 보호 API를 호출하게 하고, 자격 증명 없는 `refreshAll()` 401 fan-out을 없앤다.

**Architecture:** `POST /api/v1/auth/loopback-session`이 remote loopback이고 계정이 없을 때만 `typ=loopback_access`/`loopback_refresh` JWT를 발급한다. Host `Authorize`와 API handler가 둘 다 loopback을 검사한다. Web은 `sessionStorage`의 기존 account session 키를 재사용하고, 자격 증명이 없으면 보호 route를 호출하지 않는다.

**Tech Stack:** C# / .NET 10, `System.Text.Json`, xUnit, TypeScript served concatenation (`build-served-asset.mjs`), Node `vm` browser fixture, Pester 5.

## Global Constraints

- Source spec: `docs/superpowers/specs/2026-08-13-purecvisor-desktop-node-web-loopback-session-bootstrap-design.md`
- Approval: `User-Approval: web-loopback-session-bootstrap-20260813`
- Operational current remains `0.42.72-admin-smoke`. Do not edit `docs/ga-ready/current-evidence.json` or generated current blocks.
- Change tier `M`, verification lane `Full`. No MSI, service, Hyper-V, firewall, or other host mutation.
- Do not put a service token in HTML, `pcv-config.js`, or fixture rendered text.
- Do not write accounts into `accounts.json`. Keep `bootstrap_state=no-default-account`.
- Do not change `DesktopNodeAccountAuthOptions.Ready` (still requires at least one account).
- Do not add a PCVCLI `auth loopback-session` command.
- Do not add Playwright E2E or open `0.42.73` campaign.
- New documents use Korean body; keep identifiers, routes, and problem codes in the original form.
- Every task is RED then GREEN. Do not Skip expectation tests.
- Adding `POST /api/v1/auth/loopback-session` is an approved catalog increment 55 → 56.

---

## File map

| File | Responsibility |
| --- | --- |
| `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs` | Add `RemoteIsLoopback` to `DesktopNodeApiRequest`. |
| `src/DesktopNode.Api/ApiHandlerAdapterContract.cs` | Register the new auth route. |
| `src/DesktopNode.Api/DesktopNodeAccountAuth.cs` | `CanIssueLoopbackSession`, issue/validate/refresh loopback JWT, policy flag. |
| `src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs` | Dispatch `POST /api/v1/auth/loopback-session` and loopback refresh. |
| `src/DesktopNode.Contracts/RuntimePolicy.cs` | Additive `loopback_session_available` on `RuntimePolicyAuthPolicy`. |
| `src/DesktopNode.Host/DesktopNodeHostApplication.Request.cs` | Pass `RemoteIsLoopback` from `RemoteEndPoint`. |
| `src/DesktopNode.Host/DesktopNodeHostApplication.StaticAuth.cs` | Host allowlist + loopback JWT accept/reject. |
| `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs` | Route count 56 and new row. |
| `src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs` | Create. API issue/reject/refresh/policy tests. |
| `src/DesktopNode.Host.Tests/DesktopNodeHostLoopbackSessionTests.cs` | Create. In-process listener tests. |
| `web/src/served/routes.ts` | Route + coverage row. |
| `web/src/served/api-client.ts` | `createLoopbackSession()`. |
| `web/src/served/actions.ts` | `ensureLoopbackSession()`, `isLoopbackHostname()`. |
| `web/src/served/job-polling.ts` | Gate `refreshAll()`. |
| `web/src/served/render-shell.ts` | Auth gate banner. |
| `web/src/served-app.ts` | Call `ensureLoopbackSession()` before first `refreshAll()`. |
| `web/app.js` | Regenerated only via `node scripts/build-served-asset.mjs --write`. |
| `web/scripts/verify-browser-fixture.mjs` | Fan-out and bootstrap fixture cases. |
| `web/tests/PcvDesktopWeb.Static.Tests.ps1` | Route string + token absence. |
| `docs/USER_GUIDE.md`, `docs/CLI_COMMAND_USAGE.md` | Web-only boundary sentence. |
| `docs/ga-ready/evidence/web-loopback-session-bootstrap-code-level-2026-08-13.md` | Code-level evidence. Do not promote current. |

---

### Task 1: Request field and approved route catalog increment

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs:11-17`
- Modify: `src/DesktopNode.Api/ApiHandlerAdapterContract.cs:84-88`
- Modify: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs:14-35`
- Test: `src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs`

**Interfaces:**
- Consumes: existing `RuntimeProductOperation(...)` helper and `ApiHandlerAdapterContract.CreateDefault()`.
- Produces: `DesktopNodeApiRequest(..., bool RemoteIsLoopback = false)` as the last parameter. Catalog row `POST /api/v1/auth/loopback-session`, operation `CreateLoopbackSession`, family `auth`, auth policy `NoBearerTokenRequired`, `requiredPermission: null`. Route count `56`.

- [ ] **Step 1: Write the failing catalog assertions**

In `DefaultContractMapsPhase25RouteCandidates`, change the count and add the new row next to the other auth routes:

```csharp
Assert.Equal(56, contract.Routes.Count);
Assert.Equal(56, routes.Count);
AssertRoute(routes[("POST", "/api/v1/auth/loopback-session")], "POST", "CreateLoopbackSession", MutationStance.ProductOperation);
```

In the existing NoBearer assertion block around line 133, add:

```csharp
Assert.Equal("NoBearerTokenRequired", routes[("POST", "/api/v1/auth/loopback-session")].AuthPolicy);
Assert.Null(routes[("POST", "/api/v1/auth/loopback-session")].RequiredPermission);
Assert.Equal("auth", routes[("POST", "/api/v1/auth/loopback-session")].RouteFamily);
```

- [ ] **Step 2: Run the catalog test and confirm RED**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests.DefaultContractMapsPhase25RouteCandidates --nologo`
Expected: FAIL because the route is missing and count is still 55.

- [ ] **Step 3: Add the request field and catalog row**

Update the record to:

```csharp
public sealed record DesktopNodeApiRequest(
    string Method,
    string Path,
    string? Body = null,
    string? RequestId = null,
    string? ClientIdentity = null,
    string? Authorization = null,
    bool RemoteIsLoopback = false);
```

Insert this line immediately after the login catalog row:

```csharp
RuntimeProductOperation("/api/v1/auth/loopback-session", "CreateLoopbackSession", "auth", "NoBearerTokenRequired", requiredPermission: null),
```

Do not add a new route family. `IsAccountAuthRoute` already matches family `auth`.

- [ ] **Step 4: Re-run the catalog test**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiHandlerAdapterContractTests --nologo`
Expected: PASS. If another test still asserts 55, update that assertion in this task — the increment is approved.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Api/DesktopNodeApiRequestProcessor.cs src/DesktopNode.Api/ApiHandlerAdapterContract.cs src/DesktopNode.Api.Tests/ApiHandlerAdapterContractTests.cs
git commit -m "feat(api): register loopback-session route and request flag"
```

---

### Task 2: Issue and reject loopback sessions in the API owner

**Files:**
- Modify: `src/DesktopNode.Api/DesktopNodeAccountAuth.cs` (`Ready`, `CreateRuntimePolicy`, add `CanIssueLoopbackSession`, `CreateLoopbackSession`, `ValidateLoopbackAccessToken`)
- Modify: `src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs:24-42`
- Create: `src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs`
- Test: `src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs`

**Interfaces:**
- Consumes: `DesktopNodeApiRequest.RemoteIsLoopback`, existing `IssueToken`, `ValidateToken`, `Success`/`Error`, `AuthResult`.
- Produces:
  - `DesktopNodeAccountAuthOptions.CanIssueLoopbackSession` → `Enabled && !string.IsNullOrWhiteSpace(SigningKey) && !Ready`
  - `DesktopNodeAccountAuthService.CanIssueLoopbackSession` → same
  - `DesktopNodeAccountAuthService.CreateLoopbackSession(bool remoteIsLoopback)` → `DesktopNodeAuthActionResult`
  - `DesktopNodeAccountAuthService.ValidateLoopbackAccessToken(string? authorization)` → `DesktopNodeAuthValidationResult` that does **not** require `Ready`
  - Handler `TryHandle` owns `POST /api/v1/auth/loopback-session`
  - Success `data.grant_type` = `"loopback_session"`; JWT `typ` = `loopback_access` / `loopback_refresh`
  - Synthetic principal: username/sub `loopback-session`, role `operator`, display name `Loopback session`

- [ ] **Step 1: Write failing processor tests**

Create `src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs`:

```csharp
using System.Text;
using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiLoopbackSessionRequestProcessorTests
{
    private static readonly string SigningKey = SyntheticAuthMaterial.Value;

    [Fact]
    public void LoopbackSessionIssuesOperatorJwtWithoutWritingAccounts()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: UnsignedEmptyAccounts());

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        Assert.Equal("auth.loopback-session", document.RootElement.GetProperty("operation").GetString());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Bearer", data.GetProperty("token_type").GetString());
        Assert.Equal("loopback_session", data.GetProperty("grant_type").GetString());
        Assert.Equal("loopback-session", data.GetProperty("session").GetProperty("username").GetString());
        Assert.Equal("operator", data.GetProperty("session").GetProperty("role").GetString());
        Assert.Equal("loopback-session", data.GetProperty("session").GetProperty("subject").GetString());
        Assert.Equal("loopback_access", ReadJwtTyp(data.GetProperty("access_token").GetString()!));
        Assert.Equal("loopback_refresh", ReadJwtTyp(data.GetProperty("refresh_token").GetString()!));
        Assert.Equal(900, data.GetProperty("expires_in").GetInt32());
        Assert.Equal(28800, data.GetProperty("refresh_expires_in").GetInt32());
        Assert.False(UnsignedEmptyAccounts().Ready);
    }

    [Fact]
    public void NonLoopbackRemoteIsRejectedAndIssuesNoToken()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: UnsignedEmptyAccounts());

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session"));

        Assert.Equal(403, response.StatusCode);
        Assert.Contains("PCV_LOOPBACK_SESSION_NOT_LOOPBACK", response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("access_token", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadyAccountsDisableLoopbackSession()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: ReadyOperator());

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));

        Assert.Equal(409, response.StatusCode);
        Assert.Contains("PCV_LOOPBACK_SESSION_DISABLED", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingSigningKeyReturnsStructuredConflict()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: new DesktopNodeAccountAuthOptions(Enabled: true, Accounts: []));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));

        Assert.Equal(409, response.StatusCode);
        Assert.Contains("PCV_ACCOUNT_AUTH_SIGNING_KEY_EMPTY", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void GetIsNotAnIssuePath()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: UnsignedEmptyAccounts());

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));

        Assert.Equal(404, response.StatusCode);
    }

    private static DesktopNodeAccountAuthOptions UnsignedEmptyAccounts()
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SigningKey,
            Accounts: []);
    }

    private static DesktopNodeAccountAuthOptions ReadyOperator()
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SigningKey,
            Accounts:
            [
                new DesktopNodeAccountUser(
                    "operator",
                    "operator",
                    DesktopNodeAccountPassword.HashPassword("correct horse battery staple", "pcv-test-salt"),
                    "operator",
                    "operator user")
            ]);
    }

    private static string ReadJwtTyp(string token)
    {
        var payload = token.Split('.')[1];
        var padded = payload.Replace('-', '+').Replace('_', '/');
        padded = (padded.Length % 4) switch
        {
            2 => padded + "==",
            3 => padded + "=",
            _ => padded
        };
        using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(padded)));
        return document.RootElement.GetProperty("typ").GetString()!;
    }
}
```

- [ ] **Step 2: Run the new tests and confirm RED**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiLoopbackSessionRequestProcessorTests --nologo`
Expected: FAIL with 404 / missing operation (handler does not own the path yet).

- [ ] **Step 3: Implement issue/reject on the auth owner**

Add on `DesktopNodeAccountAuthOptions`:

```csharp
public bool CanIssueLoopbackSession =>
    Enabled && !string.IsNullOrWhiteSpace(SigningKey) && !Ready;
```

Add on `DesktopNodeAccountAuthService`:

```csharp
public bool CanIssueLoopbackSession => options.CanIssueLoopbackSession;

private static DesktopNodeAccountUser LoopbackSessionUser()
{
    return new DesktopNodeAccountUser(
        Id: "loopback-session",
        Username: "loopback-session",
        PasswordHash: "unused",
        Role: "operator",
        DisplayName: "Loopback session");
}

public DesktopNodeAuthActionResult CreateLoopbackSession(bool remoteIsLoopback)
{
    if (options.Ready)
    {
        return Error(409, "auth.loopback-session", "PCV_LOOPBACK_SESSION_DISABLED",
            "Loopback session is disabled because account auth is configured.",
            "Use POST /api/v1/auth/login with an account.");
    }

    if (!remoteIsLoopback)
    {
        return Error(403, "auth.loopback-session", "PCV_LOOPBACK_SESSION_NOT_LOOPBACK",
            "Loopback session requires a loopback remote address.",
            "Call this route from 127.0.0.1 or ::1. X-Forwarded-For is ignored.");
    }

    if (!CanIssueLoopbackSession)
    {
        return Error(409, "auth.loopback-session", "PCV_ACCOUNT_AUTH_SIGNING_KEY_EMPTY",
            "JWT signing key file is empty.",
            "Write a high-entropy local signing key before issuing a loopback session.");
    }

    return Success("auth.loopback-session", BuildLoopbackTokenPair());
}
```

Add `BuildLoopbackTokenPair()` that copies `BuildTokenPair` but calls `IssueToken(LoopbackSessionUser(), "loopback_access", ...)` and `IssueToken(..., "loopback_refresh", ...)` and adds `["grant_type"] = "loopback_session"`.

Add `ValidateLoopbackAccessToken` that:
- does not check `Ready`
- reads the bearer
- calls `ValidateToken(bearer, expectedType: "loopback_access")`

In `DesktopNodeApiAuthSessionHandler.TryHandle`, after the login branch:

```csharp
if (method == "POST" && path == "/api/v1/auth/loopback-session")
{
    return AuthResult(accountAuth.CreateLoopbackSession(request.RemoteIsLoopback));
}
```

Do not parse a body. Do not create or write account files.

- [ ] **Step 4: Re-run the loopback processor tests**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiLoopbackSessionRequestProcessorTests --nologo`
Expected: PASS.

Also run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiAccountAuth --nologo`
Expected: existing account tests still PASS, including `AccountAuthFilePathsWithoutAccountsDoNotTakeOverBearerProtectedRoutes`.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Api/DesktopNodeAccountAuth.cs src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs
git commit -m "feat(api): issue loopback sessions only from loopback"
```

---

### Task 3: Loopback refresh and runtime policy flag

**Files:**
- Modify: `src/DesktopNode.Contracts/RuntimePolicy.cs:108-117`
- Modify: `src/DesktopNode.Api/DesktopNodeAccountAuth.cs` (`CreateRuntimePolicy`, `Refresh`)
- Modify: `src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs`
- Test: `src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs`

**Interfaces:**
- Consumes: `CreateLoopbackSession`, `ValidateToken(..., "loopback_refresh")`.
- Produces: `RuntimePolicyAuthPolicy.LoopbackSessionAvailable` (`[property: JsonPropertyName("loopback_session_available")] bool LoopbackSessionAvailable = false`). `Refresh` accepts `typ=loopback_refresh` when `!Ready && remoteIsLoopback`. Used refresh `jti` is revoked. `CreateRuntimePolicy` sets `loopback_session_available` and includes `"loopback_session"` in `grant_types` only when `CanIssueLoopbackSession`.

- [ ] **Step 1: Write failing refresh and policy tests**

Append to `ApiLoopbackSessionRequestProcessorTests`:

```csharp
[Fact]
public void LoopbackRefreshRotatesTokensOnlyFromLoopback()
{
    var processor = DesktopNodeApiRequestProcessor.CreateDefault(
        accountAuthOptions: UnsignedEmptyAccounts());
    var issued = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/auth/loopback-session",
        RemoteIsLoopback: true));
    using var issuedDocument = JsonDocument.Parse(issued.Body);
    var refreshToken = issuedDocument.RootElement.GetProperty("data").GetProperty("refresh_token").GetString()!;

    var rotated = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/auth/refresh",
        JsonSerializer.Serialize(new { refresh_token = refreshToken }),
        RemoteIsLoopback: true));
    var replay = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/auth/refresh",
        JsonSerializer.Serialize(new { refresh_token = refreshToken }),
        RemoteIsLoopback: true));
    var remoteRefresh = processor.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/auth/refresh",
        JsonSerializer.Serialize(new { refresh_token = refreshToken })));

    Assert.Equal(200, rotated.StatusCode);
    using var rotatedDocument = JsonDocument.Parse(rotated.Body);
    Assert.Equal("loopback_session", rotatedDocument.RootElement.GetProperty("data").GetProperty("grant_type").GetString());
    Assert.Equal("loopback_access", ReadJwtTyp(rotatedDocument.RootElement.GetProperty("data").GetProperty("access_token").GetString()!));
    Assert.Equal(401, replay.StatusCode);
    Assert.Contains("PCV_REFRESH_TOKEN_REVOKED", replay.Body, StringComparison.Ordinal);
    Assert.Equal(403, remoteRefresh.StatusCode);
    Assert.Contains("PCV_LOOPBACK_SESSION_NOT_LOOPBACK", remoteRefresh.Body, StringComparison.Ordinal);
}

[Fact]
public void ReadyRefreshRejectsLoopbackRefreshToken()
{
    var empty = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: UnsignedEmptyAccounts());
    var issued = empty.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/auth/loopback-session",
        RemoteIsLoopback: true));
    using var issuedDocument = JsonDocument.Parse(issued.Body);
    var refreshToken = issuedDocument.RootElement.GetProperty("data").GetProperty("refresh_token").GetString()!;

    var ready = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: ReadyOperator());
    var refresh = ready.Handle(new DesktopNodeApiRequest(
        "POST",
        "/api/v1/auth/refresh",
        JsonSerializer.Serialize(new { refresh_token = refreshToken }),
        RemoteIsLoopback: true));

    Assert.NotEqual(200, refresh.StatusCode);
    Assert.DoesNotContain("loopback_session", refresh.Body, StringComparison.Ordinal);
}

[Fact]
public void RuntimePolicyAdvertisesLoopbackSessionOnlyWhenIssuable()
{
    var available = new DesktopNodeAccountAuthService(UnsignedEmptyAccounts()).CreateRuntimePolicy("dpapi-local-machine");
    var ready = new DesktopNodeAccountAuthService(ReadyOperator()).CreateRuntimePolicy("dpapi-local-machine");

    Assert.True(available.LoopbackSessionAvailable);
    Assert.Contains("loopback_session", available.GrantTypes!);
    Assert.False(ready.LoopbackSessionAvailable);
    Assert.DoesNotContain("loopback_session", ready.GrantTypes ?? []);
}
```

- [ ] **Step 2: Run the new tests and confirm RED**

Run: `dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiLoopbackSessionRequestProcessorTests --nologo`
Expected: FAIL on missing `grant_type` during refresh, missing property, or account-refresh rejecting `loopback_refresh` as `PCV_JWT_INVALID`.

- [ ] **Step 3: Implement refresh and policy**

Add the additive property to `RuntimePolicyAuthPolicy`:

```csharp
[property: JsonPropertyName("loopback_session_available")] bool LoopbackSessionAvailable = false
```

In `CreateRuntimePolicy`, when `Enabled && !Ready`:

```csharp
return new RuntimePolicyAuthPolicy(
    Mode: "account_rbac_jwt_not_configured",
    MultiUser: true,
    Rbac: true,
    TokenStorage: tokenStorage,
    Roles: ["viewer", "operator", "admin"],
    GrantTypes: CanIssueLoopbackSession
        ? ["password", "refresh_token", "loopback_session"]
        : ["password", "refresh_token"],
    SessionStorage: "browser-session-memory",
    AccessTokenTtlSeconds: (int)options.EffectiveAccessTokenLifetime.TotalSeconds,
    RefreshTokenTtlSeconds: (int)options.EffectiveRefreshTokenLifetime.TotalSeconds,
    LoopbackSessionAvailable: CanIssueLoopbackSession);
```

When `Ready`, set `LoopbackSessionAvailable: false` and keep grant types `password`/`refresh_token` only.

Change `Refresh` to:

1. Read `refresh_token` as today.
2. If `!options.Ready`: require a later handler-supplied loopback check. Put the remote check on the handler, not inside `Refresh` alone, because `Refresh(JsonElement)` has no remote flag today.
3. Prefer a new overload: `Refresh(JsonElement body, bool remoteIsLoopback)`.

`Refresh(JsonElement body, bool remoteIsLoopback)`:

- If the token `typ` is `loopback_refresh`:
  - `Ready` → `Error(409, "auth.refresh", "PCV_LOOPBACK_SESSION_DISABLED", ...)`
  - `!remoteIsLoopback` → `Error(403, "auth.refresh", "PCV_LOOPBACK_SESSION_NOT_LOOPBACK", ...)`
  - validate with `ValidateToken(refreshToken, "loopback_refresh")`
  - revoke `jti`
  - return `Success("auth.refresh", BuildLoopbackTokenPair())`
- Else keep the existing account refresh path (including `Ready` 409).

Update the handler refresh branch:

```csharp
return !parsed.Ok ? parsed.Response! : AuthResult(accountAuth.Refresh(parsed.Value!.Value, request.RemoteIsLoopback));
```

Keep a `Refresh(JsonElement body)` wrapper that calls `Refresh(body, remoteIsLoopback: false)` so existing tests compile.

Also extend `ValidateToken` revoke check: treat `expectedType` `loopback_refresh` like `refresh` for `revokedRefreshTokenIds`.

- [ ] **Step 4: Re-run loopback and existing auth tests**

Run:

```powershell
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiLoopbackSessionRequestProcessorTests --nologo
dotnet test src/DesktopNode.Api.Tests/DesktopNode.Api.Tests.csproj --filter FullyQualifiedName~ApiAccountAuth --nologo
dotnet test src/DesktopNode.Contracts.Tests/DesktopNode.Contracts.Tests.csproj --filter FullyQualifiedName~RuntimePolicyContractTests --nologo
```

Expected: PASS. The new policy field is optional with default `false`, so existing contract snapshots that omit it remain valid.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Contracts/RuntimePolicy.cs src/DesktopNode.Api/DesktopNodeAccountAuth.cs src/DesktopNode.Api/DesktopNodeApiAuthSessionHandler.cs src/DesktopNode.Api.Tests/ApiLoopbackSessionRequestProcessorTests.cs
git commit -m "feat(api): rotate loopback sessions and advertise the grant"
```

---

### Task 4: Host authorize loopback sessions and pass RemoteIsLoopback

**Files:**
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.Request.cs:136-142`
- Modify: `src/DesktopNode.Host/DesktopNodeHostApplication.StaticAuth.cs:79-123`
- Create: `src/DesktopNode.Host.Tests/DesktopNodeHostLoopbackSessionTests.cs`
- Test: `src/DesktopNode.Host.Tests/DesktopNodeHostLoopbackSessionTests.cs`

**Interfaces:**
- Consumes: `accountAuthService.ValidateLoopbackAccessToken`, `accountAuthService.CanIssueLoopbackSession`, `accountAuthReady`.
- Produces: Host `Authorize` table from the spec §4.3. `DesktopNodeApiRequest.RemoteIsLoopback` is set from `request.RemoteEndPoint` via `IsLoopbackRemote(EndPoint?)`. `X-Forwarded-For` is ignored. `/pcv-config.js` still serializes only `apiBaseUrl`.

- [ ] **Step 1: Write failing in-process listener tests**

Create `src/DesktopNode.Host.Tests/DesktopNodeHostLoopbackSessionTests.cs` using the same `StartAsync` + `HttpClient` pattern as `DesktopNodeHttpTransportContractTests`:

```csharp
[Fact]
public async Task LoopbackSessionThenRuntimePolicySucceedsWithoutServiceToken()
{
    using var host = await StartNotConfiguredHostAsync();
    using var client = new HttpClient();
    using var issue = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/loopback-session"), null);
    var issuedBody = await issue.Content.ReadAsStringAsync();
    Assert.Equal(HttpStatusCode.OK, issue.StatusCode);
    using var document = JsonDocument.Parse(issuedBody);
    var access = document.RootElement.GetProperty("data").GetProperty("access_token").GetString();

    using var policyRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(host.BaseUri, "/api/v1/runtime/policy"));
    policyRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + access);
    using var policy = await client.SendAsync(policyRequest);
    Assert.Equal(HttpStatusCode.OK, policy.StatusCode);
}

[Fact]
public async Task LoopbackSessionDoesNotChangeEmptyAccountsFile()
{
    var (host, accountFile) = await StartNotConfiguredHostWithAccountFileAsync();
    using (host)
    {
        using var client = new HttpClient();
        using var issue = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/loopback-session"), null);
        Assert.Equal(HttpStatusCode.OK, issue.StatusCode);
        using var accounts = JsonDocument.Parse(await File.ReadAllTextAsync(accountFile));
        Assert.Equal("no-default-account", accounts.RootElement.GetProperty("bootstrap_state").GetString());
        Assert.Equal(0, accounts.RootElement.GetProperty("accounts").GetArrayLength());
    }
}

[Fact]
public async Task ReadyHostDisablesLoopbackSession()
{
    using var host = await StartReadyHostAsync();
    using var client = new HttpClient();
    using var issue = await client.PostAsync(new Uri(host.BaseUri, "/api/v1/auth/loopback-session"), null);
    var body = await issue.Content.ReadAsStringAsync();
    Assert.Equal(HttpStatusCode.Conflict, issue.StatusCode);
    Assert.Contains("PCV_LOOPBACK_SESSION_DISABLED", body, StringComparison.Ordinal);
}

[Fact]
public async Task WebConfigScriptDoesNotContainTokens()
{
    var webRoot = Directory.CreateTempSubdirectory().FullName;
    File.WriteAllText(Path.Combine(webRoot, "index.html"), "<html></html>");
    using var host = await DesktopNodeHostApplication.StartAsync(new DesktopNodeHostOptions
    {
        Mode = DesktopNodeHostMode.Listen,
        Prefix = "http://127.0.0.1:0/",
        WebPrefix = "http://127.0.0.1:0/",
        WebRootPath = webRoot,
        ApiTokenFile = WriteServiceTokenFile(),
        AccountAuthOptions = NotConfiguredOptions()
    });
    using var client = new HttpClient();
    var script = await client.GetStringAsync(new Uri(host.WebBaseUri, "/pcv-config.js"));
    Assert.Contains("apiBaseUrl", script, StringComparison.Ordinal);
    Assert.DoesNotContain("access_token", script, StringComparison.Ordinal);
    Assert.DoesNotContain("protected_token", script, StringComparison.Ordinal);
    Assert.DoesNotContain(ServiceTokenValue, script, StringComparison.Ordinal);
}

[Fact]
public async Task ReadyHostRejectsPreviousLoopbackAccessToken()
{
    using var issuer = await StartNotConfiguredHostAsync();
    using var client = new HttpClient();
    using var issue = await client.PostAsync(new Uri(issuer.BaseUri, "/api/v1/auth/loopback-session"), null);
    using var document = JsonDocument.Parse(await issue.Content.ReadAsStringAsync());
    var access = document.RootElement.GetProperty("data").GetProperty("access_token").GetString();
    issuer.Dispose();

    using var ready = await StartReadyHostAsync();
    using var policyRequest = new HttpRequestMessage(HttpMethod.Get, new Uri(ready.BaseUri, "/api/v1/runtime/policy"));
    policyRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + access);
    using var policy = await client.SendAsync(policyRequest);
    Assert.Equal(HttpStatusCode.Unauthorized, policy.StatusCode);
    Assert.Contains("PCV_LOOPBACK_SESSION_DISABLED", await policy.Content.ReadAsStringAsync(), StringComparison.Ordinal);
}
```

Helpers: write a temp service token file (same as `DesktopNodeHttpTransportContractTests`), `NotConfiguredOptions()` with signing key and empty accounts, `ReadyOptions()` with one operator. `StartAsync` `Prefix = "http://127.0.0.1:0/"`.

Non-loopback remote cannot be forged through `HttpClient` to a `127.0.0.1` listener (`RemoteEndPoint` stays loopback). Do **not** invent a fake forwarded-header test. The API-layer `RemoteIsLoopback=false` test already owns that rejection. Host must still ignore `X-Forwarded-For`; add:

```csharp
[Fact]
public async Task ForwardedForHeaderDoesNotBypassLoopbackCheckOnIssuePath()
{
    using var host = await StartNotConfiguredHostAsync();
    using var client = new HttpClient();
    using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(host.BaseUri, "/api/v1/auth/loopback-session"));
    request.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.9");
    using var issue = await client.SendAsync(request);
    // Real remote is still 127.0.0.1, so issue stays 200. The header must not cause 403.
    Assert.Equal(HttpStatusCode.OK, issue.StatusCode);
}
```

- [ ] **Step 2: Run the Host tests and confirm RED**

Run: `dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~DesktopNodeHostLoopbackSessionTests --nologo`
Expected: FAIL with 401 `PCV_AUTH_REQUIRED` on issue (current Host allowlist does not include the path when `!Ready`).

- [ ] **Step 3: Implement Host authorize and request mapping**

In `DesktopNodeHostApplication.Request.cs`, pass:

```csharp
Authorization: request.Headers["Authorization"],
RemoteIsLoopback: IsLoopbackRemote(request.RemoteEndPoint));
```

In `StaticAuth.cs` replace `Authorize` with the spec table:

```csharp
private DesktopNodeApiResponse? Authorize(HttpListenerRequest request)
{
    var path = request.Url?.AbsolutePath ?? "/";
    var remoteIsLoopback = IsLoopbackRemote(request.RemoteEndPoint);

    if (IsLoopbackSessionPath(path))
    {
        if (!remoteIsLoopback)
        {
            return Json(403, "PCV_LOOPBACK_SESSION_NOT_LOOPBACK", "Loopback session requires a loopback remote address.");
        }

        if (accountAuthReady)
        {
            return Json(409, "PCV_LOOPBACK_SESSION_DISABLED", "Loopback session is disabled because account auth is configured.");
        }

        return null;
    }

    if (IsUnauthenticatedAuthPath(path))
    {
        return null;
    }

    if (string.IsNullOrWhiteSpace(token.Value))
    {
        return null;
    }

    var authorization = request.Headers["Authorization"];
    if (string.IsNullOrWhiteSpace(authorization) ||
        !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        return accountAuthReady ? null : Json(401, "PCV_AUTH_REQUIRED", "Authorization bearer token is required.");
    }

    var providedToken = authorization["Bearer ".Length..].Trim();
    if (string.Equals(providedToken, token.Value, StringComparison.Ordinal))
    {
        return null;
    }

    var loopback = accountAuthService.ValidateLoopbackAccessToken(authorization);
    if (loopback.Ok)
    {
        if (accountAuthReady)
        {
            return Json(401, "PCV_LOOPBACK_SESSION_DISABLED", "Loopback session is disabled because account auth is configured.");
        }

        if (!remoteIsLoopback)
        {
            return Json(403, "PCV_LOOPBACK_SESSION_NOT_LOOPBACK", "Loopback session requires a loopback remote address.");
        }

        return null;
    }

    if (accountAuthReady)
    {
        return null;
    }

    return Json(403, "PCV_AUTH_FORBIDDEN", "Authorization bearer token was rejected.");
}

private static bool IsLoopbackSessionPath(string path) =>
    path.Equals("/api/v1/auth/loopback-session", StringComparison.OrdinalIgnoreCase);

private static bool IsUnauthenticatedAuthPath(string path) =>
    path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) ||
    path.Equals("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase) ||
    path.Equals("/api/v1/auth/logout", StringComparison.OrdinalIgnoreCase);

private static bool IsLoopbackRemote(EndPoint? endPoint)
{
    if (endPoint is not IPEndPoint ip)
    {
        return false;
    }

    return IPAddress.IsLoopback(ip.Address);
}
```

Remove `/api/v1/runtime/policy` from any `!Ready` allowlist. Keep the existing Ready-only bootstrap path for login/refresh/policy as today by letting `accountAuthReady` fall through to the API for JWT. `IsAccountAuthBootstrapPath` may stay for Ready login/refresh/policy; do not add loopback-session there in the Ready branch (Ready must 409 the issue path, which `IsLoopbackSessionPath` already handles).

`ValidateLoopbackAccessToken` must not return `PCV_ACCOUNT_AUTH_NOT_CONFIGURED` when `!Ready`. That is why Task 2 added a dedicated method.

- [ ] **Step 4: Re-run Host loopback and transport tests**

Run:

```powershell
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~DesktopNodeHostLoopbackSessionTests --nologo
dotnet test src/DesktopNode.Host.Tests/DesktopNode.Host.Tests.csproj --filter FullyQualifiedName~DesktopNodeHttpTransportContractTests --nologo
```

Expected: PASS. Transport fixture cases that require a service token when `!Ready` still 401 without one.

- [ ] **Step 5: Commit**

```powershell
git add src/DesktopNode.Host/DesktopNodeHostApplication.Request.cs src/DesktopNode.Host/DesktopNodeHostApplication.StaticAuth.cs src/DesktopNode.Host.Tests/DesktopNodeHostLoopbackSessionTests.cs
git commit -m "feat(host): accept loopback sessions without exposing the service token"
```

---

### Task 5: Web client, session ensure, and refreshAll gate

**Files:**
- Modify: `web/src/served/routes.ts` (registry + coverage)
- Modify: `web/src/served/api-client.ts` (after `loginAccount`)
- Modify: `web/src/served/actions.ts` (add `isLoopbackHostname`, `ensureLoopbackSession`)
- Modify: `web/src/served/job-polling.ts` (`refreshAll`)
- Modify: `web/src/served-app.ts:406-410`
- Modify: `web/src/served/types.ts` (`createLoopbackSession` on the api type)
- Test: `web/scripts/verify-browser-fixture.mjs`
- Test: `web/tests/PcvDesktopWeb.Static.Tests.ps1`

**Interfaces:**
- Consumes: `applyAccountSessionPayload`, `desktopApi`, `state.authAccessToken`, `state.apiToken`.
- Produces:
  - `DESKTOP_NODE_API_ROUTES.authLoopbackSession = '/api/v1/auth/loopback-session'`
  - coverage row `{ id: 'auth.loopback-session', method: 'POST', route: ..., view: 'troubleshooting', mutating: false, tokenRequired: false }`
  - `desktopApi.createLoopbackSession()` → `POST` with `skipAuth: true`
  - `isLoopbackHostname(hostname: string): boolean`
  - `async function ensureLoopbackSession(): Promise<void>`
  - `refreshAll()` awaits `ensureLoopbackSession()` first and does not start protected steps when both tokens are empty

- [ ] **Step 1: Write the failing fixture and static guards**

In `web/tests/PcvDesktopWeb.Static.Tests.ps1`, next to the `/api/v1/auth/login` assertion:

```powershell
$combined | Should -Match '/api/v1/auth/loopback-session'
$combined | Should -Match 'ensureLoopbackSession'
$index | Should -Not -Match 'access_token\s*[:=]'
```

In `createFixtureFetch`, count fetches:

```javascript
const fetchCalls = [];
return async function fixtureFetch(rawUrl, options = {}) {
  fetchCalls.push({ url: String(rawUrl), method: String(options.method || "GET").toUpperCase() });
  // existing routing...
```

Expose `fetchCalls` on the fixture context (`window.__pcvFetchCalls = fetchCalls` or return it from `runFixture`).

Add a handler **before** `failAllAuth` so bootstrap can succeed when requested:

```javascript
if (path === "/api/v1/auth/loopback-session" && method === "POST") {
  if (options.allowLoopbackSession === false) {
    return failResponse(409, "auth.loopback-session", "PCV_LOOPBACK_SESSION_DISABLED", "Loopback session is disabled because account auth is configured.");
  }
  return ok({
    token_type: "Bearer",
    grant_type: "loopback_session",
    access_token: "loopback-access-token",
    refresh_token: "loopback-refresh-token",
    expires_in: 900,
    refresh_expires_in: 28800,
    session: { username: "loopback-session", role: "operator", subject: "loopback-session" }
  }, "auth.loopback-session");
}
```

Add these fixture cases after the existing unauthenticated run:

```javascript
const noSessionRun = await runFixture({
  failAllAuth: true,
  initialAccountSession: null,
  skipVmSelect: true,
  locationHostname: "127.0.0.1"
});
const noSessionCalls = noSessionRun.fetchCalls.filter((call) => !call.url.includes("/api/v1/auth/loopback-session"));
if (noSessionCalls.some((call) => call.url.includes("/api/v1/ops/summary") || call.url.includes("/api/v1/vms") || call.url.includes("/api/v1/host/status"))) {
  fail("refreshAll must not fan out protected routes without credentials");
}

const lanRun = await runFixture({
  failAllAuth: true,
  initialAccountSession: null,
  skipVmSelect: true,
  locationHostname: "[redacted-private-endpoint]"
});
if (lanRun.fetchCalls.some((call) => call.url.includes("/api/v1/auth/loopback-session"))) {
  fail("non-loopback hostname must not POST loopback-session");
}

const bootstrapRun = await runFixture({
  initialAccountSession: null,
  skipVmSelect: true,
  locationHostname: "127.0.0.1",
  allowLoopbackSession: true
});
if (!bootstrapRun.fetchCalls.some((call) => call.url.includes("/api/v1/auth/loopback-session") && call.method === "POST")) {
  fail("loopback hostname with empty session must POST loopback-session");
}
if (!bootstrapRun.fetchCalls.some((call) => call.url.includes("/api/v1/ops/summary"))) {
  fail("successful loopback session must continue refreshAll");
}
```

Set `window.location.hostname` from `options.locationHostname || "127.0.0.1"` in `runFixture`. Default fixture runs keep `initialAccountSession` so they do not change.

- [ ] **Step 2: Run fixture/static and confirm RED**

Run:

```powershell
npm run browser:fixture --prefix web
Invoke-Pester -Path web/tests/PcvDesktopWeb.Static.Tests.ps1 -Output Detailed
```

Expected: FAIL — `ensureLoopbackSession` / route string missing, and unauthenticated/no-session fixture still fans out.

- [ ] **Step 3: Implement the Web gate**

`routes.ts`: add `authLoopbackSession` and the coverage row next to `auth.login`.

`api-client.ts`:

```javascript
createLoopbackSession: () => apiFetch(DESKTOP_NODE_API_ROUTES.authLoopbackSession, {
  method: 'POST',
  skipAuth: true
} as RequestInit & { skipAuth: boolean }),
```

`actions.ts`:

```javascript
function isLoopbackHostname(hostname) {
  const value = String(hostname || '').replace(/^\[|\]$/g, '').toLowerCase();
  return value === '127.0.0.1' || value === 'localhost' || value === '::1';
}

async function ensureLoopbackSession() {
  if (state.authAccessToken.trim() || state.apiToken.trim()) {
    return;
  }
  if (!isLoopbackHostname(window.location.hostname)) {
    return;
  }
  state.authPending = true;
  state.authError = null;
  try {
    const result = await desktopApi.createLoopbackSession();
    applyAccountSessionPayload(result);
  } catch (error) {
    state.authError = normalizeError(error);
    state.connectionState = 'auth';
  } finally {
    state.authPending = false;
  }
}
```

`job-polling.ts` `refreshAll`:

```javascript
async function refreshAll() {
  if (state.refreshController) {
    state.refreshController.abort();
  }
  const requestId = state.refreshRequestId + 1;
  const controller = new AbortController();
  state.refreshRequestId = requestId;
  state.refreshController = controller;
  const requestOptions = { signal: controller.signal };
  state.loading = true;
  state.error = null;
  state.partialFailures = [];
  render();
  try {
    await ensureLoopbackSession();
    if (requestId !== state.refreshRequestId) return;
    if (!state.authAccessToken.trim() && !state.apiToken.trim()) {
      state.connectionState = 'auth';
      state.lastRefreshedAt = Date.now();
      state.loading = false;
      state.refreshController = null;
      render();
      return;
    }

    const steps = [
      { label: 'ops.summary', run: () => loadOpsSummary(requestOptions) },
      { label: 'host.status', run: () => loadHost(requestOptions) },
      { label: 'vm.list', run: () => loadVms(requestOptions) },
      { label: 'network.inventory', run: () => loadNetworkInventory(requestOptions) },
      { label: 'runtime.policy', run: () => loadRuntimePolicy(requestOptions) },
      { label: 'auth.session', run: () => loadAccountSession(requestOptions) },
      { label: 'console.capabilities', run: () => loadConsoleCapabilities(requestOptions) },
      { label: 'job.list', run: () => loadServerJobs(requestOptions) },
      { label: 'diagnostic.bundle.list', run: () => loadDiagnosticBundleList(requestOptions) },
      { label: 'job.poll', run: () => pollTrackedJobs(requestOptions) },
      { label: 'vm.selected.refresh', run: () => refreshSelectedVm(requestOptions) }
    ];

    const failures = [];
    for (const step of steps) {
      if (requestId !== state.refreshRequestId) return;
      try {
        await step.run();
      } catch (error) {
        const normalized = normalizeError(error);
        if (normalized.code === 'PCV_REQUEST_ABORTED') {
          continue;
        }
        failures.push({ ...normalized, label: step.label });
        if (isAuthError(normalized)) {
          controller.abort();
          break;
        }
      }
    }

    if (requestId !== state.refreshRequestId) return;
    state.lastRefreshedAt = Date.now();
    state.partialFailures = failures;
    if (failures.length > 0) {
      state.error = buildPartialRefreshError(failures);
      state.connectionState = failures.some(isAuthError) ? 'auth' : 'degraded';
    } else {
      state.error = null;
      state.connectionState = 'connected';
    }
  } catch (error) {
    if (requestId !== state.refreshRequestId) return;
    state.error = normalizeError(error);
    state.connectionState = isAuthError(state.error) ? 'auth' : 'error';
  } finally {
    if (requestId === state.refreshRequestId) {
      state.loading = false;
      state.refreshController = null;
      render();
    }
  }
}
```

Adapt `collectRefreshFailures` if the sequential loop already builds `failures` in that shape. Keep the existing failure object fields (`code`, `operation`, `message`) so current fixture assertions still match.

`served-app.ts` `init` stays `loadAccountSessionFromStorage(); ... render(); refreshAll();` — `refreshAll` now calls `ensureLoopbackSession`.

Regenerate `web/app.js`:

```powershell
node scripts/build-served-asset.mjs --write
```

from `web/`.

- [ ] **Step 4: Re-run Web gates**

```powershell
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
Invoke-Pester -Path web/tests -Output Detailed
```

Expected: PASS, including the existing unauthenticated `—` assertions. Default fixture still has `initialAccountSession`, so happy-path screens stay connected.

- [ ] **Step 5: Commit**

```powershell
git add web/src/served/routes.ts web/src/served/api-client.ts web/src/served/types.ts web/src/served/actions.ts web/src/served/job-polling.ts web/src/served-app.ts web/app.js web/scripts/verify-browser-fixture.mjs web/tests/PcvDesktopWeb.Static.Tests.ps1
git commit -m "feat(web): bootstrap loopback sessions and stop 401 fan-out"
```

---

### Task 6: Auth gate banner and operator docs

**Files:**
- Modify: `web/src/served/render-shell.ts` (`render` path that fills `#alert-region`)
- Modify: `docs/USER_GUIDE.md` §계정/RBAC/JWT
- Modify: `docs/CLI_COMMAND_USAGE.md` opening paragraph
- Modify: `docs/USER_FEATURE_USAGE_SPEC.md` only if it lists auth routes
- Test: `web/scripts/verify-browser-fixture.mjs` (gate visible when no session)

**Interfaces:**
- Consumes: `state.connectionState`, `state.authError`, `state.authAccessToken`, `state.apiToken`, existing `#account-login-form` on troubleshooting.
- Produces: when `connectionState === 'auth'` and both tokens are empty, `#alert-region` contains the existing login form **or** a banner that points at `#account-login-form` and `#api-token`. Do not create a second `#account-login-form` while troubleshooting is also mounted. Prefer: if `els.accountSessionPanel` already rendered the form, the banner only shows `state.authError.code` and tells the operator to use the header token field or the Account card. If the current view is not troubleshooting, inject one form with id `account-login-form` into `#alert-region` and skip injecting it again in `renderAccountSession` that paint.

- [ ] **Step 1: Write the failing fixture assertion**

```javascript
const gateRun = await runFixture({
  failAllAuth: true,
  initialAccountSession: null,
  skipVmSelect: true,
  locationHostname: "127.0.0.1"
});
const alertHtml = gateRun.document.getElementById("alert-region").innerHTML;
requireIncludes(alertHtml, "account-login-form", "auth gate");
requireIncludes(alertHtml, "api-token", "auth gate points at browser token");
```

If the implementation points at the existing header `#api-token` by text rather than cloning the input, assert that text (`browser token` / `api-token`) instead of a second input. Do not require new marketing copy.

- [ ] **Step 2: Run the fixture and confirm RED**

Run: `npm run browser:fixture --prefix web`
Expected: FAIL — `#alert-region` does not contain the gate.

- [ ] **Step 3: Implement the banner and docs**

In `render-shell.ts`, after `renderConnectionState()`:

```javascript
function needsAuthGate() {
  return state.connectionState === 'auth'
    && !state.authAccessToken.trim()
    && !state.apiToken.trim();
}

function renderAuthGate() {
  if (!els.alertRegion) return;
  if (!needsAuthGate()) {
    return;
  }
  const code = state.authError?.code || 'PCV_AUTH_REQUIRED';
  const message = state.authError?.message || 'Authorization bearer token is required.';
  const formHtml = state.activeView === 'troubleshooting'
    ? ''
    : `<form id="account-login-form" class="account-login-form" autocomplete="off">
        <label>Username<input id="account-username" name="username" type="text" autocomplete="username"></label>
        <label>Password<input id="account-password" name="password" type="password" autocomplete="current-password"></label>
        <button type="submit">Login</button>
      </form>`;
  els.alertRegion.innerHTML = `<div class="diagnostics-result error" data-auth-gate="true">
    <span class="muted">Auth required</span>
    <strong>${escapeHtml(code)}</strong>
    <p>${escapeHtml(message)} Use the header browser token field or the account login form. Service tokens stay out of HTML.</p>
    ${formHtml}
  </div>`;
}
```

Call `renderAuthGate()` from the existing `render()` sequence. If `render()` already writes `#alert-region` for other errors, prepend the gate rather than wiping those errors.

When `state.activeView === 'troubleshooting'`, `renderAccountSession()` keeps the only `#account-login-form`.

`docs/USER_GUIDE.md` replace the sentence that says protected bearer remains the only pre-account path with:

```markdown
기본 bootstrap은 `no-default-account`다. loopback Web Console(`127.0.0.1` / `localhost` / `::1`)은
`POST /api/v1/auth/loopback-session`으로 짧은 JWT를 받으며 service token을 페이지에 넣지 않는다.
계정이 구성되면 이 경로는 `409 PCV_LOOPBACK_SESSION_DISABLED`로 닫히고 `POST /api/v1/auth/login`만
남는다. LAN 또는 비-loopback remote는 기존 service bearer 또는 계정 JWT가 필요하다.
`pcvcli`는 이 발급 route를 노출하지 않는다.
```

`docs/CLI_COMMAND_USAGE.md` first paragraph, after the account sentence:

```markdown
`POST /api/v1/auth/loopback-session`도 Web Console 전용이다. PCVCLI는 설치본 protected token file을 계속 사용한다.
```

If `docs/USER_FEATURE_USAGE_SPEC.md` has an auth route table, add one row for `auth.loopback-session` / Web-only / no CLI command.

- [ ] **Step 4: Re-run Web verification**

```powershell
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
Invoke-Pester -Path web/tests -Output Detailed
```

Expected: PASS. Unauthenticated `—` footer/hero assertions remain.

- [ ] **Step 5: Commit**

```powershell
git add web/src/served/render-shell.ts web/src/served/render-panels.ts web/app.js web/scripts/verify-browser-fixture.mjs docs/USER_GUIDE.md docs/CLI_COMMAND_USAGE.md docs/USER_FEATURE_USAGE_SPEC.md
git commit -m "feat(web): show a single auth gate and document the Web-only session"
```

---

### Task 7: Full lane and code-level evidence

**Files:**
- Create: `docs/ga-ready/evidence/web-loopback-session-bootstrap-code-level-2026-08-13.md`
- Modify: `docs/ga-ready/EVIDENCE_INDEX.md` (add the record; do not change current)
- Modify: `docs/DEVELOPER_INDEX.md` (point at this plan as the implementation owner)

**Interfaces:**
- Consumes: test output from this plan. No SHA/MSI/package pair updates.
- Produces: evidence with `host_mutation_performed: false`, `public_trusted_signing: not-claimed`, `external_stable_publication: not-claimed`, `operational_current_changed: false`.

- [ ] **Step 1: Run the required Full lane**

```powershell
dotnet test src/DesktopNode.sln --nologo
npm test --prefix web
npm run verify:parity --prefix web
npm run browser:fixture --prefix web
node --check web/app.js
Invoke-Pester -Path web/tests -Output Detailed
& packaging/windows-desktop-node/tools/Invoke-PcvDevelopmentVerification.ps1 `
  -Lane Full -ChangeTier M `
  -ChangedPath @(
    'src/DesktopNode.Api/DesktopNodeAccountAuth.cs',
    'src/DesktopNode.Host/DesktopNodeHostApplication.StaticAuth.cs',
    'web/src/served/job-polling.ts')
git diff --check
```

Expected: all PASS. Record the exact command results in the evidence file. If Full lane fails, fix in the owning task; do not weaken the gate.

- [ ] **Step 2: Write evidence**

Korean body. Include: Design-ID, approval locator, route, problem codes, `accounts.json` unchanged, `/pcv-config.js` token-free, 401 fan-out closed, nonclaims (no installed E2E, no 0.42.73, no public signing).

- [ ] **Step 3: Index the evidence**

Add one `EVIDENCE_INDEX.md` bullet. Add one `DEVELOPER_INDEX.md` dated line under the 2026-08-13 design section pointing at this plan. Do not touch generated current blocks.

- [ ] **Step 4: Commit**

```powershell
git add docs/ga-ready/evidence/web-loopback-session-bootstrap-code-level-2026-08-13.md docs/ga-ready/EVIDENCE_INDEX.md docs/DEVELOPER_INDEX.md
git commit -m "docs: record loopback session bootstrap code-level evidence"
```

---

## Self-review

| Spec section | Task |
| --- | --- |
| §4.1 issue route, loopback/Ready/empty-key, no GET | Task 2 |
| §4.2 memory principal, `loopback_access`/`loopback_refresh`, `Ready` unchanged | Task 2 |
| §4.3 Host authorize table, policy still token-required when `!Ready` | Task 4 |
| §4.4 `RemoteIsLoopback` | Task 1 + Task 4 |
| §4.5 refresh loopback + revoke | Task 3 |
| §4.6 Web ensure + refreshAll + no token in config | Task 5 + Task 4 config test |
| §4.6 auth gate | Task 6 |
| §4.7 catalog + policy flag | Task 1 + Task 3 |
| §4.8 problem codes | Task 2–4 |
| §6 tests / §7 non-promotion | Task 7 |
| CLI not added | Task 6 docs |

No Playwright, no package campaign, no `current-evidence.json` edits.
