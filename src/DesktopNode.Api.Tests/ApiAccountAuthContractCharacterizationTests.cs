using System.Text.Json;
using System.Text.Json.Nodes;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiAccountAuthContractCharacterizationTests
{
    private const string FixtureName = "account-auth-contract-v1.json";
    private const string Password = "correct horse battery staple";
    private static readonly DateTimeOffset InitialTime = new(2026, 8, 2, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AuthAndAuthorizationFailuresMatchGoldenEnvelopes()
    {
        using var fixture = LoadFixture();
        var errors = fixture.RootElement.GetProperty("error_envelopes");
        var disabled = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: DesktopNodeAccountAuthOptions.Disabled);

        AssertResponse(errors.GetProperty("disabled_login"), disabled.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            """{"username":"admin","password":"secret"}""",
            RequestId: "req-disabled-login")));

        var viewer = CreateProcessor("viewer", "viewer");
        var viewerTokens = Login(viewer, "viewer");
        AssertResponse(errors.GetProperty("missing_login_body"), viewer.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            RequestId: "req-missing-login")));
        AssertResponse(errors.GetProperty("missing_session_bearer"), viewer.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            RequestId: "req-missing-session")));
        AssertResponse(errors.GetProperty("viewer_mutation_forbidden"), viewer.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            """{"name":"must-not-leak"}""",
            RequestId: "req-viewer-mutation",
            Authorization: $"Bearer {viewerTokens.AccessToken}")));

        var operatorProcessor = CreateProcessor("operator", "operator");
        var operatorTokens = Login(operatorProcessor, "operator");
        AssertResponse(errors.GetProperty("guest_execution_forbidden"), operatorProcessor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec/preview",
            """{"command":["powershell","--password=must-not-leak"]}""",
            RequestId: "req-guest-forbidden",
            Authorization: $"Bearer {operatorTokens.AccessToken}")));

        AssertResponse(errors.GetProperty("uppercase_auth_route"), viewer.Handle(new DesktopNodeApiRequest(
            "POST",
            "/API/V1/AUTH/LOGIN",
            """{"username":"viewer","password":"correct horse battery staple"}""",
            RequestId: "req-uppercase-auth")));
    }

    [Fact]
    public void LoginTokenPairMatchesGoldenProjectionWithoutSecretLeak()
    {
        using var fixture = LoadFixture();
        var passwordHash = DesktopNodeAccountPassword.HashPassword(Password, "pcv-test-salt");
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: TestAuthOptions("admin", "admin", () => InitialTime, passwordHash));

        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            JsonSerializer.Serialize(new { username = "admin", password = Password }),
            RequestId: "req-token-projection"));
        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var data = root.GetProperty("data");
        var accessToken = data.GetProperty("access_token").GetString()!;
        var refreshToken = data.GetProperty("refresh_token").GetString()!;
        var actual = ToElement(new SortedDictionary<string, object?>
        {
            ["access_expires_at"] = data.GetProperty("access_expires_at").GetString(),
            ["access_token_segments"] = accessToken.Split('.').Length,
            ["expires_in"] = data.GetProperty("expires_in").GetInt32(),
            ["operation"] = root.GetProperty("operation").GetString(),
            ["refresh_expires_at"] = data.GetProperty("refresh_expires_at").GetString(),
            ["refresh_expires_in"] = data.GetProperty("refresh_expires_in").GetInt32(),
            ["refresh_token_segments"] = refreshToken.Split('.').Length,
            ["request_id"] = root.GetProperty("request_id").GetString(),
            ["session"] = data.GetProperty("session").Clone(),
            ["status_code"] = response.StatusCode,
            ["token_type"] = data.GetProperty("token_type").GetString()
        });

        AssertGolden(fixture.RootElement.GetProperty("token_projection"), actual);
        Assert.DoesNotContain(Password, response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(passwordHash, response.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(SyntheticAuthMaterial.Value, response.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void ExactExpiryBoundaryMatchesGoldenForAccessAndRefreshTokens()
    {
        using var fixture = LoadFixture();
        var errors = fixture.RootElement.GetProperty("error_envelopes");

        var accessClock = new MutableClock(InitialTime);
        var accessProcessor = CreateProcessor("viewer", "viewer", accessClock.Read);
        var accessTokens = Login(accessProcessor, "viewer");
        accessClock.Now = InitialTime.AddMinutes(15);
        AssertResponse(errors.GetProperty("access_expired"), accessProcessor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            RequestId: "req-access-expired",
            Authorization: $"Bearer {accessTokens.AccessToken}")));

        var refreshClock = new MutableClock(InitialTime);
        var refreshProcessor = CreateProcessor("operator", "operator", refreshClock.Read);
        var refreshTokens = Login(refreshProcessor, "operator");
        refreshClock.Now = InitialTime.AddHours(8);
        AssertResponse(errors.GetProperty("refresh_expired"), refreshProcessor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            JsonSerializer.Serialize(new { refresh_token = refreshTokens.RefreshToken }),
            RequestId: "req-refresh-expired")));
    }

    [Fact]
    public void RefreshRotationAndLogoutPreserveCurrentRevocationSemantics()
    {
        using var fixture = LoadFixture();
        var processor = CreateProcessor("operator", "operator");
        var login = Login(processor, "operator");
        var rotated = Refresh(processor, login.RefreshToken);

        Assert.NotEqual(login.AccessToken, rotated.AccessToken);
        Assert.NotEqual(login.RefreshToken, rotated.RefreshToken);
        AssertResponse(
            fixture.RootElement.GetProperty("error_envelopes").GetProperty("revoked_refresh"),
            processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/auth/refresh",
                JsonSerializer.Serialize(new { refresh_token = login.RefreshToken }),
                RequestId: "req-revoked-refresh")));

        var logout = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/logout",
            JsonSerializer.Serialize(new { refresh_token = rotated.RefreshToken }),
            RequestId: "req-rotated-logout"));
        Assert.Equal(200, logout.StatusCode);
        Assert.DoesNotContain(rotated.RefreshToken, logout.Body, StringComparison.Ordinal);

        var survivingAccess = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            Authorization: $"Bearer {rotated.AccessToken}"));
        Assert.Equal(200, survivingAccess.StatusCode);

        var revokedAfterLogout = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            JsonSerializer.Serialize(new { refresh_token = rotated.RefreshToken })));
        Assert.Equal(401, revokedAfterLogout.StatusCode);
        Assert.Contains("PCV_REFRESH_TOKEN_REVOKED", revokedAfterLogout.Body, StringComparison.Ordinal);

        AssertBodyGolden(
            fixture.RootElement.GetProperty("logout_responses").GetProperty("blank"),
            processor.Handle(new DesktopNodeApiRequest(
                "POST",
                "/api/v1/auth/logout",
                RequestId: "req-blank-logout")));
        var invalidToken = "invalid.refresh.token";
        var invalidLogout = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/logout",
            JsonSerializer.Serialize(new { refresh_token = invalidToken }),
            RequestId: "req-invalid-logout"));
        AssertBodyGolden(fixture.RootElement.GetProperty("logout_responses").GetProperty("invalid"), invalidLogout);
        Assert.DoesNotContain(invalidToken, invalidLogout.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void SessionAndRbacResponsesMatchGoldenRoleProjection()
    {
        using var fixture = LoadFixture();
        var processor = CreateProcessor("admin", "admin");
        var tokens = Login(processor, "admin");

        AssertBodyGolden(fixture.RootElement.GetProperty("session_response"), processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            RequestId: "req-session-projection",
            Authorization: $"Bearer {tokens.AccessToken}")));
        AssertBodyGolden(fixture.RootElement.GetProperty("rbac_response"), processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/rbac",
            RequestId: "req-rbac-projection",
            Authorization: $"Bearer {tokens.AccessToken}")));
    }

    [Fact]
    public void BootstrapUnknownRouteAndInvalidTokenPrecedenceRemainStable()
    {
        using var fixture = LoadFixture();
        var disabled = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: DesktopNodeAccountAuthOptions.Disabled);
        var disabledUnknown = disabled.Handle(new DesktopNodeApiRequest("GET", "/api/v1/not-a-route"));
        Assert.Equal(404, disabledUnknown.StatusCode);

        var ready = CreateProcessor("viewer", "viewer");
        var missingAuthUnknown = ready.Handle(new DesktopNodeApiRequest("GET", "/api/v1/not-a-route"));
        Assert.Equal(401, missingAuthUnknown.StatusCode);
        Assert.Contains("PCV_AUTH_REQUIRED", missingAuthUnknown.Body, StringComparison.Ordinal);

        var tokens = Login(ready, "viewer");
        var authenticatedUnknown = ready.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/not-a-route",
            Authorization: $"Bearer {tokens.AccessToken}"));
        Assert.Equal(404, authenticatedUnknown.StatusCode);

        var invalidToken = "top-secret.invalid.token";
        var invalid = ready.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            RequestId: "req-invalid-access",
            Authorization: $"Bearer {invalidToken}"));
        AssertResponse(
            fixture.RootElement.GetProperty("error_envelopes").GetProperty("invalid_access"),
            invalid);
        Assert.DoesNotContain(invalidToken, invalid.Body, StringComparison.Ordinal);
    }

    private static JsonDocument LoadFixture()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Auth", FixtureName);
        Assert.True(File.Exists(path), $"Tracked auth fixture was not copied to '{path}'.");
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static DesktopNodeApiRequestProcessor CreateProcessor(
        string username,
        string role,
        Func<DateTimeOffset>? clock = null)
    {
        return DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: TestAuthOptions(username, role, clock ?? (() => InitialTime)));
    }

    private static DesktopNodeAccountAuthOptions TestAuthOptions(
        string username,
        string role,
        Func<DateTimeOffset> clock,
        string? passwordHash = null)
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
            Accounts:
            [
                new DesktopNodeAccountUser(
                    Id: $"{username}-id",
                    Username: username,
                    PasswordHash: passwordHash ?? DesktopNodeAccountPassword.HashPassword(Password, "pcv-test-salt"),
                    Role: role,
                    DisplayName: $"{role} user")
            ],
            Clock: clock);
    }

    private static AuthTokens Login(DesktopNodeApiRequestProcessor processor, string username)
    {
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            JsonSerializer.Serialize(new { username, password = Password })));
        Assert.Equal(200, response.StatusCode);
        return ReadTokens(response);
    }

    private static AuthTokens Refresh(DesktopNodeApiRequestProcessor processor, string refreshToken)
    {
        var response = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            JsonSerializer.Serialize(new { refresh_token = refreshToken })));
        Assert.Equal(200, response.StatusCode);
        return ReadTokens(response);
    }

    private static AuthTokens ReadTokens(DesktopNodeApiResponse response)
    {
        using var document = JsonDocument.Parse(response.Body);
        var data = document.RootElement.GetProperty("data");
        return new AuthTokens(
            data.GetProperty("access_token").GetString()!,
            data.GetProperty("refresh_token").GetString()!);
    }

    private static void AssertResponse(JsonElement expected, DesktopNodeApiResponse actual)
    {
        Assert.Equal(expected.GetProperty("status_code").GetInt32(), actual.StatusCode);
        AssertBodyGolden(expected.GetProperty("body"), actual);
    }

    private static void AssertBodyGolden(JsonElement expected, DesktopNodeApiResponse actual)
    {
        using var actualDocument = JsonDocument.Parse(actual.Body);
        AssertGolden(expected, actualDocument.RootElement);
    }

    private static void AssertGolden(JsonElement expected, JsonElement actual)
    {
        var expectedNode = JsonNode.Parse(expected.GetRawText());
        var actualNode = JsonNode.Parse(actual.GetRawText());
        Assert.True(
            JsonNode.DeepEquals(expectedNode, actualNode),
            $"Golden JSON mismatch.{Environment.NewLine}Expected: {expected.GetRawText()}{Environment.NewLine}Actual: {actual.GetRawText()}");
    }

    private static JsonElement ToElement(object value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    private sealed record AuthTokens(string AccessToken, string RefreshToken);

    private sealed class MutableClock(DateTimeOffset now)
    {
        public DateTimeOffset Now { get; set; } = now;

        public DateTimeOffset Read() => Now;
    }
}
