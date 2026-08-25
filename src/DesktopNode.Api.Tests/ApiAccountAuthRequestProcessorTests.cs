using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiAccountAuthRequestProcessorTests
{
    [Fact]
    public void AccountLoginReturnsJwtSessionWithoutPasswordLeak()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: TestAuthOptions("admin", "admin"));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            """{"username":"admin","password":"correct horse battery staple"}"""));

        Assert.Equal(200, response.StatusCode);
        Assert.DoesNotContain("correct horse battery staple", response.Body, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Bearer", data.GetProperty("token_type").GetString());
        Assert.Contains(".", data.GetProperty("access_token").GetString());
        Assert.Contains(".", data.GetProperty("refresh_token").GetString());
        Assert.Equal("admin", data.GetProperty("session").GetProperty("role").GetString());
    }

    [Fact]
    public void ViewerJwtCanReadButCannotQueueMutation()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: TestAuthOptions("viewer", "viewer"));
        var token = Login(processor, "viewer");

        var read = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/runtime/policy",
            Authorization: $"Bearer {token.AccessToken}"));
        var mutation = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            """{"name":"blocked","iso_path":"D:\\iso\\blocked.iso","cpu":1,"memory_mb":1024,"disk_gb":8,"vm_root":"D:\\VMs","generation":2}""",
            Authorization: $"Bearer {token.AccessToken}"));

        Assert.Equal(200, read.StatusCode);
        Assert.Equal(403, mutation.StatusCode);
        Assert.Contains("PCV_RBAC_FORBIDDEN", mutation.Body);
        Assert.DoesNotContain("blocked.iso", mutation.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RefreshTokenIssuesNewAccessTokenAndKeepsSessionRole()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: TestAuthOptions("operator", "operator"));
        var login = Login(processor, "operator");

        var refresh = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            JsonSerializer.Serialize(new { refresh_token = login.RefreshToken })));

        Assert.Equal(200, refresh.StatusCode);
        using var document = JsonDocument.Parse(refresh.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Contains(".", data.GetProperty("access_token").GetString());
        Assert.Contains(".", data.GetProperty("refresh_token").GetString());
        Assert.Equal("operator", data.GetProperty("session").GetProperty("role").GetString());
    }

    [Fact]
    public void ConsoleCapabilityRouteDeclaresWindowsHandoffAndGatedNoVnc()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: TestAuthOptions("operator", "operator"));
        var token = Login(processor, "operator");

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/console/capabilities",
            Authorization: $"Bearer {token.AccessToken}"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("console.capabilities", document.RootElement.GetProperty("operation").GetString());
        Assert.Equal("vmconnect", data.GetProperty("windows_console").GetProperty("type").GetString());
        Assert.False(data.GetProperty("novnc").GetProperty("enabled").GetBoolean());
        Assert.Equal("not_configured", data.GetProperty("novnc").GetProperty("status").GetString());
    }

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
        Assert.Equal("service-token-or-account-jwt", card.GetProperty("account").GetProperty("auth_surface").GetString());
        Assert.Equal("redacted", card.GetProperty("account").GetProperty("token_output").GetString());
        Assert.Equal("vmconnect", card.GetProperty("windows_console").GetProperty("type").GetString());
        Assert.Equal("not_configured", card.GetProperty("novnc").GetProperty("status").GetString());
        Assert.Equal("Use local vmconnect handoff; configure noVNC bridge only when browser streaming is required.", card.GetProperty("next_action").GetString());
        Assert.False(card.GetProperty("host_mutation_performed").GetBoolean());
        Assert.DoesNotContain("correct horse battery staple", response.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConsoleSessionCardOmitsNoVncPathWhenBridgeIsNotConfigured()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: TestAuthOptions("operator", "operator"),
            consoleOptions: new DesktopNodeConsoleOptions(
                Enabled: true,
                NoVncEnabled: false,
                NoVncWebSocketPath: "/api/v1/console/novnc/{vm_id}",
                NoVncBridgeMode: "websocket-to-vnc-tcp"));
        var token = Login(processor, "operator");

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/vms/alpha/console",
            Authorization: $"Bearer {token.AccessToken}"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var card = document.RootElement.GetProperty("data").GetProperty("console_access");
        var noVnc = card.GetProperty("novnc");
        Assert.Equal("not_configured", noVnc.GetProperty("status").GetString());
        Assert.Equal("not_configured", noVnc.GetProperty("reason_code").GetString());
        Assert.Equal(JsonValueKind.Null, noVnc.GetProperty("websocket_path").ValueKind);
        Assert.Equal("local-console-handoff-ready", card.GetProperty("status").GetString());
    }

    [Fact]
    public void ConsoleCapabilityCardReportsDisabledConsoleWithoutNoVncPath()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: TestAuthOptions("operator", "operator"),
            consoleOptions: new DesktopNodeConsoleOptions(
                Enabled: false,
                NoVncEnabled: false,
                NoVncWebSocketPath: "/api/v1/console/novnc/{vm_id}",
                NoVncBridgeMode: "websocket-to-vnc-tcp"));
        var token = Login(processor, "operator");

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/console/capabilities",
            Authorization: $"Bearer {token.AccessToken}"));

        Assert.Equal(200, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var card = document.RootElement.GetProperty("data").GetProperty("console_access");
        var noVnc = card.GetProperty("novnc");
        Assert.Equal("disabled", card.GetProperty("status").GetString());
        Assert.False(card.GetProperty("windows_console").GetProperty("available").GetBoolean());
        Assert.Equal("disabled", noVnc.GetProperty("status").GetString());
        Assert.Equal("disabled", noVnc.GetProperty("reason_code").GetString());
        Assert.Equal(JsonValueKind.Null, noVnc.GetProperty("websocket_path").ValueKind);
        Assert.Equal("Enable console access on the listener before opening VM console handoff.", card.GetProperty("next_action").GetString());
    }

    [Fact]
    public void ConsoleRoutesExposeConfiguredNoVncBridgeSessionPath()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: TestAuthOptions("operator", "operator"),
            consoleOptions: new DesktopNodeConsoleOptions(
                NoVncEnabled: true,
                NoVncWebSocketPath: "/api/v1/console/novnc/{vm_id}",
                NoVncBridgeMode: "websocket-to-vnc-tcp"));
        var token = Login(processor, "operator");

        var capabilities = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/console/capabilities",
            Authorization: $"Bearer {token.AccessToken}"));
        var session = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/vms/alpha/console",
            Authorization: $"Bearer {token.AccessToken}"));

        Assert.Equal(200, capabilities.StatusCode);
        Assert.Equal(200, session.StatusCode);
        using var capabilitiesDocument = JsonDocument.Parse(capabilities.Body);
        var noVnc = capabilitiesDocument.RootElement.GetProperty("data").GetProperty("novnc");
        Assert.True(noVnc.GetProperty("enabled").GetBoolean());
        Assert.Equal("available", noVnc.GetProperty("status").GetString());
        Assert.Equal("websocket-to-vnc-tcp", noVnc.GetProperty("bridge_mode").GetString());
        Assert.Equal("/api/v1/console/novnc/{vm_id}", noVnc.GetProperty("websocket_path_template").GetString());

        using var sessionDocument = JsonDocument.Parse(session.Body);
        var sessionData = sessionDocument.RootElement.GetProperty("data");
        Assert.Equal("websocket-vnc-bridge", sessionData.GetProperty("console").GetProperty("transport").GetString());
        Assert.Equal("/api/v1/console/novnc/alpha", sessionData.GetProperty("novnc").GetProperty("websocket_path").GetString());
        var card = sessionData.GetProperty("console_access");
        Assert.Equal("console-access-card.v1", card.GetProperty("contract").GetString());
        Assert.Equal("console.view", card.GetProperty("account").GetProperty("required_permission").GetString());
        Assert.Equal("service-token-or-account-jwt", card.GetProperty("account").GetProperty("auth_surface").GetString());
        Assert.Equal("redacted", card.GetProperty("account").GetProperty("token_output").GetString());
        Assert.Equal("available", card.GetProperty("novnc").GetProperty("status").GetString());
        Assert.Equal("/api/v1/console/novnc/alpha", card.GetProperty("novnc").GetProperty("websocket_path").GetString());
        Assert.Equal("Open the noVNC browser session for this VM, or use vmconnect from the host console.", card.GetProperty("next_action").GetString());
        Assert.False(card.GetProperty("host_mutation_performed").GetBoolean());
        Assert.False(session.Body.Contains("{vm_id}", StringComparison.Ordinal));
        Assert.False(session.Body.Contains("correct horse battery staple", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ViewerJwtCannotOpenVmConsoleSession()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: TestAuthOptions("viewer", "viewer"));
        var token = Login(processor, "viewer");

        var response = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/vms/alpha/console",
            Authorization: $"Bearer {token.AccessToken}"));

        Assert.Equal(403, response.StatusCode);
        Assert.Contains("console.view", response.Body);
    }

    [Fact]
    public void OperatorJwtCannotOpenGuestExecutionPreviewWithoutExplicitCapability()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: TestAuthOptions("operator", "operator"));
        var token = Login(processor, "operator");

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec/preview",
            """{"command":["powershell","--password=super-secret-value"],"credential_ref":"wincred:PureCVisor/guest/admin"}""",
            Authorization: $"Bearer {token.AccessToken}",
            RequestId: "req-guest-denied"));

        Assert.Equal(403, response.StatusCode);
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        Assert.Equal("vm.guest.exec.preview", root.GetProperty("operation").GetString());
        Assert.Equal("PCV_GUEST_EXEC_PERMISSION_DENIED", root.GetProperty("error").GetProperty("code").GetString());
        Assert.DoesNotContain("super-secret-value", response.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PureCVisor/guest/admin", response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountAuthFilePathsWithoutAccountsDoNotTakeOverBearerProtectedRoutes()
    {
        var accountFile = Path.Combine(Path.GetTempPath(), "pcv-accounts-" + Guid.NewGuid().ToString("N") + ".json");
        var signingKeyFile = Path.Combine(Path.GetTempPath(), "pcv-jwt-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            var options = DesktopNodeAccountAuthOptions.FromFiles(accountFile, signingKeyFile);
            var processor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: options);

            var read = processor.Handle(new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy"));
            var login = processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/auth/login",
                """{"username":"admin","password":"missing"}"""));

            Assert.Equal(200, read.StatusCode);
            Assert.Contains("account_rbac_jwt_not_configured", read.Body);
            Assert.Equal(409, login.StatusCode);
            Assert.Contains("PCV_ACCOUNT_AUTH_NOT_CONFIGURED", login.Body);
        }
        finally
        {
            if (File.Exists(accountFile)) File.Delete(accountFile);
            if (File.Exists(signingKeyFile)) File.Delete(signingKeyFile);
        }
    }

    private static DesktopNodeAccountAuthOptions TestAuthOptions(string username, string role)
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
            Accounts:
            [
                new DesktopNodeAccountUser(
                    Id: username,
                    Username: username,
                    PasswordHash: DesktopNodeAccountPassword.HashPassword("correct horse battery staple", "pcv-test-salt"),
                    Role: role,
                    DisplayName: $"{role} user")
            ]);
    }

    private static AuthTokens Login(DesktopNodeApiRequestProcessor processor, string username)
    {
        var login = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            JsonSerializer.Serialize(new
            {
                username,
                password = "correct horse battery staple"
            })));
        using var document = JsonDocument.Parse(login.Body);
        var data = document.RootElement.GetProperty("data");
        return new AuthTokens(
            data.GetProperty("access_token").GetString()!,
            data.GetProperty("refresh_token").GetString()!);
    }

    private sealed record AuthTokens(string AccessToken, string RefreshToken);
}
