using System.Text;
using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class ApiLoopbackSessionRequestProcessorTests
{

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
    public void LoopbackAccessTokenCanReadSessionAndRbacWhenNotReady()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: UnsignedEmptyAccounts());
        var issued = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));
        using var issuedDocument = JsonDocument.Parse(issued.Body);
        var accessToken = issuedDocument.RootElement.GetProperty("data").GetProperty("access_token").GetString()!;

        var session = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            Authorization: $"Bearer {accessToken}"));
        var rbac = processor.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/rbac",
            Authorization: $"Bearer {accessToken}"));

        Assert.Equal(200, session.StatusCode);
        using var sessionDocument = JsonDocument.Parse(session.Body);
        var data = sessionDocument.RootElement.GetProperty("data");
        Assert.Equal("auth.session", sessionDocument.RootElement.GetProperty("operation").GetString());
        Assert.Equal("loopback-session", data.GetProperty("username").GetString());
        Assert.Equal("operator", data.GetProperty("role").GetString());
        Assert.Equal("loopback-session", data.GetProperty("subject").GetString());
        Assert.Equal(200, rbac.StatusCode);
        using var rbacDocument = JsonDocument.Parse(rbac.Body);
        Assert.Equal("auth.rbac", rbacDocument.RootElement.GetProperty("operation").GetString());
        Assert.False(UnsignedEmptyAccounts().Ready);
    }

    [Fact]
    public void ReadySessionRejectsLoopbackAccessToken()
    {
        var empty = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: UnsignedEmptyAccounts());
        var issued = empty.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/loopback-session",
            RemoteIsLoopback: true));
        using var issuedDocument = JsonDocument.Parse(issued.Body);
        var accessToken = issuedDocument.RootElement.GetProperty("data").GetProperty("access_token").GetString()!;

        var ready = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: ReadyOperator());
        var session = ready.Handle(new DesktopNodeApiRequest(
            "GET",
            "/api/v1/auth/session",
            Authorization: $"Bearer {accessToken}"));

        Assert.NotEqual(200, session.StatusCode);
        Assert.DoesNotContain("loopback-session", session.Body, StringComparison.Ordinal);
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

    private static DesktopNodeAccountAuthOptions UnsignedEmptyAccounts()
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
            Accounts: []);
    }

    private static DesktopNodeAccountAuthOptions ReadyOperator()
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-test",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
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
