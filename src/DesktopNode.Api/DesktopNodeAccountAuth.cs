using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

public sealed record DesktopNodeAccountAuthOptions(
    bool Enabled = false,
    string Issuer = "purecvisor-desktop-node",
    string Audience = "desktop-node-local-api",
    string? SigningKey = null,
    IReadOnlyList<DesktopNodeAccountUser>? Accounts = null,
    TimeSpan? AccessTokenLifetime = null,
    TimeSpan? RefreshTokenLifetime = null,
    Func<DateTimeOffset>? Clock = null)
{
    public static DesktopNodeAccountAuthOptions Disabled { get; } = new();

    public bool Ready => Enabled &&
        !string.IsNullOrWhiteSpace(SigningKey) &&
        (Accounts?.Count ?? 0) > 0;

    public bool CanIssueLoopbackSession =>
        Enabled && !string.IsNullOrWhiteSpace(SigningKey) && !Ready;

    public DateTimeOffset Now() => Clock?.Invoke() ?? DateTimeOffset.UtcNow;

    public TimeSpan EffectiveAccessTokenLifetime => AccessTokenLifetime ?? TimeSpan.FromMinutes(15);

    public TimeSpan EffectiveRefreshTokenLifetime => RefreshTokenLifetime ?? TimeSpan.FromHours(8);

    public static DesktopNodeAccountAuthOptions FromFiles(string? accountFile, string? signingKeyFile)
    {
        if (string.IsNullOrWhiteSpace(accountFile) && string.IsNullOrWhiteSpace(signingKeyFile))
        {
            return Disabled;
        }

        if (string.IsNullOrWhiteSpace(accountFile) || string.IsNullOrWhiteSpace(signingKeyFile))
        {
            throw new ArgumentException("PCV_ACCOUNT_AUTH_CONFIG_INCOMPLETE|Account auth requires both --account-file and --jwt-signing-key-file.|Pass both files or neither.");
        }

        var accountFileExists = File.Exists(accountFile);
        var signingKeyFileExists = File.Exists(signingKeyFile);
        var config = accountFileExists
            ? JsonSerializer.Deserialize<DesktopNodeAccountAuthFile>(
                File.ReadAllText(accountFile),
                RuntimePolicyContract.JsonOptions) ??
                throw new ArgumentException("PCV_ACCOUNT_AUTH_CONFIG_INVALID|Account auth config could not be parsed.|Use a JSON object with accounts.")
            : new DesktopNodeAccountAuthFile(null, null, []);
        var signingKey = signingKeyFileExists ? File.ReadAllText(signingKeyFile).Trim() : null;
        if (signingKeyFileExists && string.IsNullOrWhiteSpace(signingKey))
        {
            throw new ArgumentException("PCV_ACCOUNT_AUTH_SIGNING_KEY_EMPTY|JWT signing key file is empty.|Write a high-entropy local signing key before enabling account auth.");
        }

        return new DesktopNodeAccountAuthOptions(
            Enabled: true,
            Issuer: string.IsNullOrWhiteSpace(config.Issuer) ? "purecvisor-desktop-node" : config.Issuer!,
            Audience: string.IsNullOrWhiteSpace(config.Audience) ? "desktop-node-local-api" : config.Audience!,
            SigningKey: signingKey,
            Accounts: config.Accounts ?? []);
    }
}

public sealed record DesktopNodeAccountUser(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password_hash")] string PasswordHash,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("display_name")] string? DisplayName = null,
    [property: JsonPropertyName("permissions")] IReadOnlyList<string>? Permissions = null);

public sealed record DesktopNodeAccountAuthFile(
    [property: JsonPropertyName("issuer")] string? Issuer,
    [property: JsonPropertyName("audience")] string? Audience,
    [property: JsonPropertyName("accounts")] IReadOnlyList<DesktopNodeAccountUser>? Accounts);

public sealed record DesktopNodeConsoleOptions(
    bool Enabled = true,
    bool NoVncEnabled = false,
    string? NoVncWebSocketPath = null,
    string NoVncBridgeMode = "disabled");

public static class DesktopNodeAccountPassword
{
    private const int DefaultIterations = 100_000;
    private const int KeySizeBytes = 32;

    public static string HashPassword(string password, string? saltText = null, int iterations = DefaultIterations)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password must be non-empty.", nameof(password));
        }

        var salt = saltText is null ? RandomNumberGenerator.GetBytes(16) : Encoding.UTF8.GetBytes(saltText);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            KeySizeBytes);
        return string.Join(
            '$',
            "pbkdf2-sha256",
            iterations.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Base64UrlEncode(salt),
            Base64UrlEncode(hash));
    }

    public static bool Verify(string password, string passwordHash)
    {
        var parts = (passwordHash ?? string.Empty).Split('$');
        if (parts.Length != 4 ||
            !string.Equals(parts[0], "pbkdf2-sha256", StringComparison.Ordinal) ||
            !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Base64UrlDecode(parts[2]);
        var expected = Base64UrlDecode(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password ?? string.Empty),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    internal static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    internal static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch
        {
            2 => "==",
            3 => "=",
            _ => string.Empty
        };
        return Convert.FromBase64String(padded);
    }
}

public sealed record DesktopNodeAccountPrincipal(
    string Subject,
    string Username,
    string Role,
    string DisplayName,
    IReadOnlyList<string> Permissions);

public sealed record DesktopNodeAuthActionResult(
    int StatusCode,
    string Operation,
    object? Data,
    DesktopNodeApiError? Error)
{
    public bool Ok => Error is null && StatusCode < 400;
}

public sealed record DesktopNodeAuthValidationResult(
    bool Ok,
    DesktopNodeAccountPrincipal? Principal,
    DesktopNodeApiError? Error,
    int StatusCode = 200);

public sealed class DesktopNodeAccountAuthService
{
    private readonly DesktopNodeAccountAuthOptions options;
    private readonly Dictionary<string, DesktopNodeAccountUser> accounts;
    private readonly HashSet<string> revokedRefreshTokenIds = new(StringComparer.Ordinal);

    public DesktopNodeAccountAuthService(DesktopNodeAccountAuthOptions? options)
    {
        this.options = options ?? DesktopNodeAccountAuthOptions.Disabled;
        accounts = (this.options.Accounts ?? [])
            .Where(account => !string.IsNullOrWhiteSpace(account.Username))
            .GroupBy(account => account.Username.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public bool Enabled => options.Enabled;

    public bool Ready => options.Ready;

    public bool CanIssueLoopbackSession => options.CanIssueLoopbackSession;

    public RuntimePolicyAuthPolicy CreateRuntimePolicy(string tokenStorage)
    {
        if (!options.Enabled)
        {
            return new RuntimePolicyAuthPolicy(
                Mode: "single_bearer_token",
                MultiUser: false,
                Rbac: false,
                TokenStorage: tokenStorage);
        }

        if (!options.Ready)
        {
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
        }

        return new RuntimePolicyAuthPolicy(
            Mode: "account_rbac_jwt",
            MultiUser: true,
            Rbac: true,
            TokenStorage: tokenStorage,
            Roles: ["viewer", "operator", "admin"],
            GrantTypes: ["password", "refresh_token"],
            SessionStorage: "browser-session-memory",
            AccessTokenTtlSeconds: (int)options.EffectiveAccessTokenLifetime.TotalSeconds,
            RefreshTokenTtlSeconds: (int)options.EffectiveRefreshTokenLifetime.TotalSeconds,
            LoopbackSessionAvailable: false);
    }

    public DesktopNodeAuthActionResult Login(JsonElement body)
    {
        if (!options.Ready)
        {
            return Error(409, "auth.login", "PCV_ACCOUNT_AUTH_NOT_CONFIGURED", "Account auth is not configured.", "Configure account file and JWT signing key before using account login.");
        }

        var username = ReadString(body, "username");
        var password = ReadString(body, "password");
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Error(400, "auth.login", "PCV_LOGIN_REQUEST_INVALID", "Username and password are required.", "Pass a JSON body with username and password.");
        }

        if (!accounts.TryGetValue(username.Trim(), out var account) ||
            !DesktopNodeAccountPassword.Verify(password, account.PasswordHash))
        {
            return Error(401, "auth.login", "PCV_LOGIN_FAILED", "Login failed.", "Username or password was rejected.");
        }

        return Success("auth.login", BuildTokenPair(account));
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

    public DesktopNodeAuthActionResult Refresh(JsonElement body)
    {
        return Refresh(body, remoteIsLoopback: false);
    }

    public DesktopNodeAuthActionResult Refresh(JsonElement body, bool remoteIsLoopback)
    {
        var refreshToken = ReadString(body, "refresh_token");
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Error(400, "auth.refresh", "PCV_REFRESH_TOKEN_REQUIRED", "Refresh token is required.", "Pass a JSON body with refresh_token.");
        }

        if (string.Equals(ReadTokenType(refreshToken), "loopback_refresh", StringComparison.Ordinal))
        {
            if (options.Ready)
            {
                return Error(409, "auth.refresh", "PCV_LOOPBACK_SESSION_DISABLED",
                    "Loopback session is disabled because account auth is configured.",
                    "Use POST /api/v1/auth/login with an account.");
            }

            if (!remoteIsLoopback)
            {
                return Error(403, "auth.refresh", "PCV_LOOPBACK_SESSION_NOT_LOOPBACK",
                    "Loopback session requires a loopback remote address.",
                    "Call this route from 127.0.0.1 or ::1. X-Forwarded-For is ignored.");
            }

            var loopbackValidation = ValidateToken(refreshToken, expectedType: "loopback_refresh");
            if (!loopbackValidation.Ok || loopbackValidation.Jwt is null)
            {
                return Error(loopbackValidation.StatusCode, "auth.refresh", loopbackValidation.Code, loopbackValidation.Message, loopbackValidation.Detail);
            }

            if (!string.IsNullOrWhiteSpace(loopbackValidation.Jwt.JwtId))
            {
                revokedRefreshTokenIds.Add(loopbackValidation.Jwt.JwtId);
            }

            return Success("auth.refresh", BuildLoopbackTokenPair());
        }

        if (!options.Ready)
        {
            return Error(409, "auth.refresh", "PCV_ACCOUNT_AUTH_NOT_CONFIGURED", "Account auth is not configured.", "Configure account file and JWT signing key before refreshing account tokens.");
        }

        var validation = ValidateToken(refreshToken, expectedType: "refresh");
        if (!validation.Ok || validation.Jwt is null)
        {
            return Error(validation.StatusCode, "auth.refresh", validation.Code, validation.Message, validation.Detail);
        }

        if (!string.IsNullOrWhiteSpace(validation.Jwt.JwtId))
        {
            revokedRefreshTokenIds.Add(validation.Jwt.JwtId);
        }

        if (!accounts.TryGetValue(validation.Jwt.Username, out var account))
        {
            return Error(401, "auth.refresh", "PCV_REFRESH_ACCOUNT_NOT_FOUND", "Refresh token account no longer exists.", "Login again with an active account.");
        }

        return Success("auth.refresh", BuildTokenPair(account));
    }

    public DesktopNodeAuthActionResult Logout(JsonElement body)
    {
        var refreshToken = ReadString(body, "refresh_token");
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var validation = ValidateToken(refreshToken, expectedType: "refresh", allowRevoked: true);
            if (validation.Jwt is not null && !string.IsNullOrWhiteSpace(validation.Jwt.JwtId))
            {
                revokedRefreshTokenIds.Add(validation.Jwt.JwtId);
            }
        }

        return Success("auth.logout", new SortedDictionary<string, object?>
        {
            ["browser_session"] = "clear-client-token-state",
            ["refresh_token_revoked"] = !string.IsNullOrWhiteSpace(refreshToken)
        });
    }

    public DesktopNodeAuthValidationResult ValidateAccessToken(string? authorization)
    {
        if (!options.Ready)
        {
            return new DesktopNodeAuthValidationResult(
                false,
                null,
                new DesktopNodeApiError("PCV_ACCOUNT_AUTH_NOT_CONFIGURED", "Account auth is not configured.", "Configure account auth before using this route.", false),
                409);
        }

        var bearer = ReadBearer(authorization);
        if (string.IsNullOrWhiteSpace(bearer))
        {
            return new DesktopNodeAuthValidationResult(
                false,
                null,
                new DesktopNodeApiError("PCV_AUTH_REQUIRED", "Authorization bearer token is required.", "Login with account credentials or provide a valid service bearer token.", false),
                401);
        }

        var validation = ValidateToken(bearer, expectedType: "access");
        if (!validation.Ok || validation.Jwt is null)
        {
            return new DesktopNodeAuthValidationResult(
                false,
                null,
                new DesktopNodeApiError(validation.Code, validation.Message, validation.Detail, false),
                validation.StatusCode);
        }

        return new DesktopNodeAuthValidationResult(true, validation.Jwt.ToPrincipal(), null);
    }

    public DesktopNodeAuthValidationResult ValidateLoopbackAccessToken(string? authorization)
    {
        var bearer = ReadBearer(authorization);
        if (string.IsNullOrWhiteSpace(bearer))
        {
            return new DesktopNodeAuthValidationResult(
                false,
                null,
                new DesktopNodeApiError("PCV_AUTH_REQUIRED", "Authorization bearer token is required.", "Login with account credentials or provide a valid service bearer token.", false),
                401);
        }

        var validation = ValidateToken(bearer, expectedType: "loopback_access");
        if (!validation.Ok || validation.Jwt is null)
        {
            return new DesktopNodeAuthValidationResult(
                false,
                null,
                new DesktopNodeApiError(validation.Code, validation.Message, validation.Detail, false),
                validation.StatusCode);
        }

        return new DesktopNodeAuthValidationResult(true, validation.Jwt.ToPrincipal(), null);
    }

    public DesktopNodeAuthValidationResult ValidateSessionAccessToken(string? authorization)
    {
        if (options.Ready)
        {
            return ValidateAccessToken(authorization);
        }

        if (!CanIssueLoopbackSession)
        {
            return new DesktopNodeAuthValidationResult(
                false,
                null,
                new DesktopNodeApiError("PCV_ACCOUNT_AUTH_NOT_CONFIGURED", "Account auth is not configured.", "Configure account auth before using this route.", false),
                409);
        }

        return ValidateLoopbackAccessToken(authorization);
    }

    public bool HasPermission(DesktopNodeAccountPrincipal principal, string requiredPermission)
    {
        if (principal.Permissions.Contains("*", StringComparer.Ordinal))
        {
            return true;
        }

        return principal.Permissions.Contains(requiredPermission, StringComparer.Ordinal);
    }

    public object BuildSessionData(DesktopNodeAccountPrincipal principal)
    {
        return new SortedDictionary<string, object?>
        {
            ["display_name"] = principal.DisplayName,
            ["permissions"] = principal.Permissions,
            ["role"] = principal.Role,
            ["subject"] = principal.Subject,
            ["username"] = principal.Username
        };
    }

    public object BuildRbacData()
    {
        return new SortedDictionary<string, object?>
        {
            ["roles"] = new[]
            {
                new SortedDictionary<string, object?>
                {
                    ["role"] = "viewer",
                    ["permissions"] = ResolvePermissions("viewer"),
                    ["description"] = "Read-only host, VM, job, network, runtime, and evidence state."
                },
                new SortedDictionary<string, object?>
                {
                    ["role"] = "operator",
                    ["permissions"] = ResolvePermissions("operator"),
                    ["description"] = "Read state, queue operator actions, collect diagnostics, and open console handoff."
                },
                new SortedDictionary<string, object?>
                {
                    ["role"] = "admin",
                    ["permissions"] = ResolvePermissions("admin"),
                    ["description"] = "Full local account/RBAC administrator role."
                }
            },
            ["route_policy"] = new SortedDictionary<string, object?>
            {
                ["read"] = "viewer",
                ["operate"] = "operator",
                ["diagnostics.create"] = "operator",
                ["console.view"] = "operator",
                ["account.manage"] = "admin"
            }
        };
    }

    private static DesktopNodeAccountUser LoopbackSessionUser()
    {
        return new DesktopNodeAccountUser(
            Id: "loopback-session",
            Username: "loopback-session",
            PasswordHash: "unused",
            Role: "operator",
            DisplayName: "Loopback session");
    }

    private object BuildLoopbackTokenPair()
    {
        return BuildTokenPair(LoopbackSessionUser(), "loopback_access", "loopback_refresh", "loopback_session");
    }

    private object BuildTokenPair(DesktopNodeAccountUser account)
    {
        return BuildTokenPair(account, "access", "refresh", grantType: null);
    }

    private object BuildTokenPair(
        DesktopNodeAccountUser account,
        string accessTokenType,
        string refreshTokenType,
        string? grantType)
    {
        var access = IssueToken(account, accessTokenType, options.EffectiveAccessTokenLifetime);
        var refresh = IssueToken(account, refreshTokenType, options.EffectiveRefreshTokenLifetime);
        var principal = ToPrincipal(account);
        var now = options.Now().ToUniversalTime();
        var data = new SortedDictionary<string, object?>
        {
            ["access_expires_at"] = now.Add(options.EffectiveAccessTokenLifetime).ToString("o"),
            ["access_token"] = access,
            ["expires_in"] = (int)options.EffectiveAccessTokenLifetime.TotalSeconds,
            ["refresh_expires_at"] = now.Add(options.EffectiveRefreshTokenLifetime).ToString("o"),
            ["refresh_expires_in"] = (int)options.EffectiveRefreshTokenLifetime.TotalSeconds,
            ["refresh_token"] = refresh,
            ["session"] = BuildSessionData(principal),
            ["token_type"] = "Bearer"
        };
        if (grantType is not null)
        {
            data["grant_type"] = grantType;
        }

        return data;
    }

    private string IssueToken(DesktopNodeAccountUser account, string tokenType, TimeSpan lifetime)
    {
        var now = options.Now().ToUniversalTime();
        var principal = ToPrincipal(account);
        var header = new SortedDictionary<string, object?>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new SortedDictionary<string, object?>
        {
            ["aud"] = options.Audience,
            ["exp"] = now.Add(lifetime).ToUnixTimeSeconds(),
            ["iat"] = now.ToUnixTimeSeconds(),
            ["iss"] = options.Issuer,
            ["jti"] = Guid.NewGuid().ToString("N"),
            ["name"] = principal.DisplayName,
            ["permissions"] = principal.Permissions,
            ["role"] = principal.Role,
            ["sub"] = principal.Subject,
            ["typ"] = tokenType,
            ["username"] = principal.Username
        };
        var headerSegment = DesktopNodeAccountPassword.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header, RuntimePolicyContract.JsonOptions));
        var payloadSegment = DesktopNodeAccountPassword.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload, RuntimePolicyContract.JsonOptions));
        var signingInput = $"{headerSegment}.{payloadSegment}";
        var signature = Sign(signingInput);
        return $"{signingInput}.{signature}";
    }

    private JwtValidation ValidateToken(string token, string expectedType, bool allowRevoked = false)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return JwtValidation.Fail(401, "PCV_JWT_INVALID", "JWT token is invalid.", "Token must have header, payload, and signature segments.");
        }

        var signingInput = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Sign(signingInput);
        var expectedBytes = Encoding.ASCII.GetBytes(expectedSignature);
        var actualBytes = Encoding.ASCII.GetBytes(parts[2]);
        if (expectedBytes.Length != actualBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
        {
            return JwtValidation.Fail(401, "PCV_JWT_INVALID", "JWT token is invalid.", "Token signature was rejected.");
        }

        JsonDocument payloadDocument;
        try
        {
            payloadDocument = JsonDocument.Parse(DesktopNodeAccountPassword.Base64UrlDecode(parts[1]));
        }
        catch (JsonException error)
        {
            return JwtValidation.Fail(401, "PCV_JWT_INVALID", "JWT token is invalid.", error.Message);
        }

        using (payloadDocument)
        {
            var payload = payloadDocument.RootElement;
            var issuer = ReadString(payload, "iss");
            var audience = ReadString(payload, "aud");
            var type = ReadString(payload, "typ");
            var jwtId = ReadString(payload, "jti");
            if (!string.Equals(issuer, options.Issuer, StringComparison.Ordinal) ||
                !string.Equals(audience, options.Audience, StringComparison.Ordinal) ||
                !string.Equals(type, expectedType, StringComparison.Ordinal))
            {
                return JwtValidation.Fail(401, "PCV_JWT_INVALID", "JWT token is invalid.", "Issuer, audience, or token type did not match this listener.");
            }

            if (!allowRevoked &&
                (expectedType == "refresh" || expectedType == "loopback_refresh") &&
                !string.IsNullOrWhiteSpace(jwtId) &&
                revokedRefreshTokenIds.Contains(jwtId))
            {
                return JwtValidation.Fail(401, "PCV_REFRESH_TOKEN_REVOKED", "Refresh token was revoked.", "Login again to create a new refresh session.");
            }

            var expires = ReadLong(payload, "exp");
            if (expires is null || DateTimeOffset.FromUnixTimeSeconds(expires.Value) <= options.Now().ToUniversalTime())
            {
                return JwtValidation.Fail(401, "PCV_JWT_EXPIRED", "JWT token expired.", "Login or refresh the session before retrying.");
            }

            var subject = ReadString(payload, "sub");
            var username = ReadString(payload, "username");
            var role = ReadString(payload, "role");
            if (string.IsNullOrWhiteSpace(subject) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(role))
            {
                return JwtValidation.Fail(401, "PCV_JWT_INVALID", "JWT token is invalid.", "Subject, username, and role claims are required.");
            }

            var displayName = ReadString(payload, "name") ?? username;
            var permissions = ReadStringArray(payload, "permissions");
            return JwtValidation.Success(new JwtPrincipal(subject, username, role, displayName, permissions, jwtId));
        }
    }

    private string Sign(string signingInput)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.SigningKey!));
        return DesktopNodeAccountPassword.Base64UrlEncode(hmac.ComputeHash(Encoding.ASCII.GetBytes(signingInput)));
    }

    private static DesktopNodeAccountPrincipal ToPrincipal(DesktopNodeAccountUser account)
    {
        return new DesktopNodeAccountPrincipal(
            Subject: string.IsNullOrWhiteSpace(account.Id) ? account.Username : account.Id,
            Username: account.Username,
            Role: NormalizeRole(account.Role),
            DisplayName: string.IsNullOrWhiteSpace(account.DisplayName) ? account.Username : account.DisplayName!,
            Permissions: account.Permissions is { Count: > 0 }
                ? account.Permissions.Select(permission => permission.Trim()).Where(permission => permission.Length > 0).Distinct(StringComparer.Ordinal).ToArray()
                : ResolvePermissions(account.Role));
    }

    private static IReadOnlyList<string> ResolvePermissions(string role)
    {
        return NormalizeRole(role) switch
        {
            "admin" => ["*", "read", "operate", "job.control", "diagnostics.read", "diagnostics.create", "console.view", "account.manage"],
            "operator" => ["read", "operate", "job.control", "diagnostics.read", "diagnostics.create", "console.view"],
            _ => ["read"]
        };
    }

    private static string NormalizeRole(string role)
    {
        var normalized = string.IsNullOrWhiteSpace(role) ? "viewer" : role.Trim().ToLowerInvariant();
        return normalized is "admin" or "operator" or "viewer" ? normalized : "viewer";
    }

    private static DesktopNodeAuthActionResult Success(string operation, object data)
    {
        return new DesktopNodeAuthActionResult(200, operation, data, null);
    }

    private static DesktopNodeAuthActionResult Error(int statusCode, string operation, string code, string message, string detail)
    {
        return new DesktopNodeAuthActionResult(statusCode, operation, null, new DesktopNodeApiError(code, message, detail, false));
    }

    private static string? ReadBearer(string? authorization)
    {
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authorization["Bearer ".Length..].Trim();
    }

    private static string? ReadTokenType(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            using var payloadDocument = JsonDocument.Parse(DesktopNodeAccountPassword.Base64UrlDecode(parts[1]));
            return ReadString(payloadDocument.RootElement, "typ");
        }
        catch (Exception exception) when (exception is JsonException or FormatException or ArgumentException)
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static long? ReadLong(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.TryGetInt64(out var parsed)
            ? parsed
            : null;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(item => item.Length > 0)
            .ToArray();
    }

    private sealed record JwtPrincipal(
        string Subject,
        string Username,
        string Role,
        string DisplayName,
        IReadOnlyList<string> Permissions,
        string? JwtId)
    {
        public DesktopNodeAccountPrincipal ToPrincipal() => new(
            Subject,
            Username,
            Role,
            DisplayName,
            Permissions);
    }

    private sealed record JwtValidation(
        bool Ok,
        JwtPrincipal? Jwt,
        int StatusCode,
        string Code,
        string Message,
        string Detail)
    {
        public static JwtValidation Success(JwtPrincipal jwt) => new(true, jwt, 200, string.Empty, string.Empty, string.Empty);

        public static JwtValidation Fail(int statusCode, string code, string message, string detail) => new(false, null, statusCode, code, message, detail);
    }
}
