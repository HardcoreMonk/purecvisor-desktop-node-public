using System.Text.Json;
using DesktopNode.Api;

namespace DesktopNode.Api.Tests;

public sealed class DesktopNodeApiAuthSessionHandlerTests
{
    private const string Password = "correct horse battery staple";
    private static readonly DateTimeOffset FixedTime = new(2026, 8, 2, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TryHandleOwnsAuthDispatchWithoutCallbackOrRequestIdAttachment()
    {
        var handler = new DesktopNodeApiAuthSessionHandler(TestOptions("admin", "admin"));

        var nonAuth = handler.TryHandle(
            new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy"),
            "GET",
            "/api/v1/runtime/policy");
        var login = handler.TryHandle(
            new DesktopNodeApiRequest(
                "POST",
                "/api/v1/auth/login",
                JsonSerializer.Serialize(new { username = "admin", password = Password })),
            "POST",
            "/api/v1/auth/login");

        Assert.Null(nonAuth);
        Assert.NotNull(login);
        Assert.Equal(200, login.StatusCode);
        Assert.Contains("\"operation\":\"auth.login\"", login.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("request_id", login.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(Password, login.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthorizeUsesExactCustomPermissionSnapshotAndUnknownRouteFallback()
    {
        var handler = new DesktopNodeApiAuthSessionHandler(TestOptions(
            "custom",
            "viewer",
            permissions: ["operate"]));
        var accessToken = Login(handler, "custom");
        var request = new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms",
            Authorization: $"Bearer {accessToken}");

        var allowedMutation = handler.Authorize(request, "POST", "/api/v1/vms");
        var deniedRead = handler.Authorize(
            request with { Method = "GET", Path = "/api/v1/vms" },
            "GET",
            "/api/v1/vms");
        var deniedUnknown = handler.Authorize(
            request with { Method = "GET", Path = "/api/v1/not-a-route" },
            "GET",
            "/api/v1/not-a-route");

        Assert.Null(allowedMutation);
        Assert.NotNull(deniedRead);
        Assert.Equal(403, deniedRead.StatusCode);
        Assert.Contains("Required permission: read", deniedRead.Body, StringComparison.Ordinal);
        Assert.NotNull(deniedUnknown);
        Assert.Equal(403, deniedUnknown.StatusCode);
        Assert.Contains("Required permission: read", deniedUnknown.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void DisabledOwnerPreservesBootstrapAndRuntimePolicyProjection()
    {
        var handler = new DesktopNodeApiAuthSessionHandler(DesktopNodeAccountAuthOptions.Disabled);
        var request = new DesktopNodeApiRequest("GET", "/api/v1/runtime/policy");

        var authorization = handler.Authorize(request, "GET", "/api/v1/runtime/policy");
        var policy = handler.CreateRuntimePolicy("credential-manager");

        Assert.Null(authorization);
        Assert.Equal("single_bearer_token", policy.Mode);
        Assert.False(policy.MultiUser);
        Assert.False(policy.Rbac);
        Assert.Equal("credential-manager", policy.TokenStorage);
    }

    [Fact]
    public void ResolveActorPrefersJwtUsernameAndFallsBackToTransportIdentity()
    {
        var handler = new DesktopNodeApiAuthSessionHandler(TestOptions("operator", "operator"));
        var accessToken = Login(handler, "operator");

        var jwtActor = handler.ResolveActor(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec/preview",
            ClientIdentity: "transport-operator",
            Authorization: $"Bearer {accessToken}"));
        var transportActor = handler.ResolveActor(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec/preview",
            ClientIdentity: "transport-operator",
            Authorization: "Bearer invalid.token.value"));
        var defaultActor = handler.ResolveActor(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/vms/alpha/guest/exec/preview"));

        Assert.Equal("operator", jwtActor);
        Assert.Equal("transport-operator", transportActor);
        Assert.Equal("local-api-operator", defaultActor);
    }

    [Fact]
    public async Task ProcessorSerializesConcurrentRefreshRotationThroughOwner()
    {
        var processor = DesktopNodeApiRequestProcessor.CreateDefault(
            accountAuthOptions: TestOptions("operator", "operator"));
        var login = processor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            JsonSerializer.Serialize(new { username = "operator", password = Password })));
        var refreshToken = ReadToken(login, "refresh_token");
        using var start = new ManualResetEventSlim(false);

        var requests = Enumerable.Range(0, 2)
            .Select(index => Task.Run(() =>
            {
                start.Wait();
                return processor.Handle(new DesktopNodeApiRequest(
                    "POST",
                    "/api/v1/auth/refresh",
                    JsonSerializer.Serialize(new { refresh_token = refreshToken }),
                    RequestId: $"req-concurrent-refresh-{index}"));
            }))
            .ToArray();
        start.Set();
        var responses = await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal([200, 401], responses.Select(response => response.StatusCode).Order().ToArray());
        var rejected = Assert.Single(responses, response => response.StatusCode == 401);
        Assert.Contains("PCV_REFRESH_TOKEN_REVOKED", rejected.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(refreshToken, rejected.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void SeparateProcessorsKeepRefreshRevocationStateIsolated()
    {
        var options = TestOptions("operator", "operator");
        var firstProcessor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: options);
        var secondProcessor = DesktopNodeApiRequestProcessor.CreateDefault(accountAuthOptions: options);
        var login = firstProcessor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/login",
            JsonSerializer.Serialize(new { username = "operator", password = Password })));
        var originalRefreshToken = ReadToken(login, "refresh_token");

        var firstRotation = firstProcessor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            JsonSerializer.Serialize(new { refresh_token = originalRefreshToken })));
        var firstReplay = firstProcessor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            JsonSerializer.Serialize(new { refresh_token = originalRefreshToken })));
        var secondRotation = secondProcessor.Handle(new DesktopNodeApiRequest(
            "POST",
            "/api/v1/auth/refresh",
            JsonSerializer.Serialize(new { refresh_token = originalRefreshToken })));

        Assert.Equal(200, firstRotation.StatusCode);
        Assert.Equal(401, firstReplay.StatusCode);
        Assert.Contains("PCV_REFRESH_TOKEN_REVOKED", firstReplay.Body, StringComparison.Ordinal);
        Assert.Equal(200, secondRotation.StatusCode);
    }

    private static DesktopNodeAccountAuthOptions TestOptions(
        string username,
        string role,
        IReadOnlyList<string>? permissions = null)
    {
        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: "pcv-owner-test",
            Audience: "pcv-local-api",
            SigningKey: SyntheticAuthMaterial.Value,
            Accounts:
            [
                new DesktopNodeAccountUser(
                    Id: $"{username}-id",
                    Username: username,
                    PasswordHash: DesktopNodeAccountPassword.HashPassword(Password, "pcv-owner-test-salt"),
                    Role: role,
                    DisplayName: $"{role} user",
                    Permissions: permissions)
            ],
            Clock: () => FixedTime);
    }

    private static string Login(DesktopNodeApiAuthSessionHandler handler, string username)
    {
        var response = handler.TryHandle(
            new DesktopNodeApiRequest(
                "POST",
                "/api/v1/auth/login",
                JsonSerializer.Serialize(new { username, password = Password })),
            "POST",
            "/api/v1/auth/login");
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        return ReadToken(response, "access_token");
    }

    private static string ReadToken(DesktopNodeApiResponse response, string propertyName)
    {
        using var document = JsonDocument.Parse(response.Body);
        return document.RootElement.GetProperty("data").GetProperty(propertyName).GetString()!;
    }
}
