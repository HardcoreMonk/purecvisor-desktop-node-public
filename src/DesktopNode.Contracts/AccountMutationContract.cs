using System.Text.RegularExpressions;

namespace DesktopNode.Contracts;

public static class AccountMutationProblemCodes
{
    public const string UsernameInvalid = "PCV_ACCOUNT_USERNAME_INVALID";
    public const string UsernameConflict = "PCV_ACCOUNT_USERNAME_CONFLICT";
    public const string PasswordInvalid = "PCV_ACCOUNT_PASSWORD_INVALID";
    public const string RoleInvalid = "PCV_ACCOUNT_ROLE_INVALID";
    public const string BootstrapAdminRequired = "PCV_ACCOUNT_BOOTSTRAP_ADMIN_REQUIRED";
    public const string BootstrapNotAvailable = "PCV_ACCOUNT_BOOTSTRAP_NOT_AVAILABLE";
    public const string NotFound = "PCV_ACCOUNT_NOT_FOUND";
    public const string LastAdmin = "PCV_ACCOUNT_LAST_ADMIN";
    public const string ConfirmationMismatch = "PCV_ACCOUNT_CONFIRMATION_MISMATCH";
    public const string ManageForbidden = "PCV_ACCOUNT_MANAGE_FORBIDDEN";
}

public sealed record AccountMutationExistingAccount(
    string Username,
    string Role,
    bool Enabled);

public sealed record AccountMutationAuthContext(
    bool RemoteIsLoopback = false,
    bool HasServiceBearer = false,
    bool HasAccountManage = false,
    bool IsLoopbackSession = false);

public sealed record AccountCreateRequest(
    string? Username,
    string? Password,
    string? Role,
    IReadOnlyList<AccountMutationExistingAccount> Existing,
    AccountMutationAuthContext Auth,
    string? DisplayName = null);

public sealed record AccountCreateEvaluation(
    bool Ok,
    string? ErrorCode,
    string? Username,
    string? Role,
    string? DisplayName,
    bool Enabled,
    string? BootstrapState);

public sealed record AccountDisableRequest(
    string? Username,
    string? ConfirmUsername,
    IReadOnlyList<AccountMutationExistingAccount> Existing,
    AccountMutationAuthContext Auth);

public sealed record AccountDisableEvaluation(
    bool Ok,
    string? ErrorCode,
    string? Username,
    bool Enabled,
    string? Action);

public static class AccountMutationContract
{
    public const string BootstrapNoDefaultAccount = "no-default-account";
    public const string BootstrapAccountsConfigured = "accounts-configured";
    public const string ReservedUsername = "loopback-session";
    public const string ActionDisable = "disable";
    public const string ActionAlreadyDisabled = "already-disabled";
    public const int PasswordMinLength = 12;
    public const int UsernameMaxLength = 32;

    public static readonly IReadOnlyList<string> AllowedRoles = ["viewer", "operator", "admin"];

    private static readonly Regex UsernamePattern = new(
        @"^[A-Za-z][A-Za-z0-9._-]{2,31}$",
        RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1));

    public static AccountCreateEvaluation EvaluateCreate(AccountCreateRequest request)
    {
        var existing = request.Existing ?? [];
        if (!TryNormalizeUsername(request.Username, out var username) ||
            string.Equals(username, ReservedUsername, StringComparison.OrdinalIgnoreCase))
        {
            return CreateReject(AccountMutationProblemCodes.UsernameInvalid);
        }

        if (!IsPasswordValid(request.Password, username))
        {
            return CreateReject(AccountMutationProblemCodes.PasswordInvalid);
        }

        if (!TryNormalizeRole(request.Role, out var role))
        {
            return CreateReject(AccountMutationProblemCodes.RoleInvalid);
        }

        var storeReady = existing.Count > 0;
        var authError = storeReady
            ? RejectReadyAuth(request.Auth)
            : RejectBootstrapAuth(request.Auth);
        if (authError is not null)
        {
            return CreateReject(authError);
        }

        if (!storeReady && role != "admin")
        {
            return CreateReject(AccountMutationProblemCodes.BootstrapAdminRequired);
        }

        if (FindAccount(existing, username) is not null)
        {
            return CreateReject(AccountMutationProblemCodes.UsernameConflict);
        }

        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? null
            : request.DisplayName.Trim();
        return new AccountCreateEvaluation(
            true,
            null,
            username,
            role,
            displayName,
            true,
            BootstrapAccountsConfigured);
    }

    public static AccountDisableEvaluation EvaluateDisable(AccountDisableRequest request)
    {
        var authError = RejectReadyAuth(request.Auth);
        if (authError is not null)
        {
            return DisableReject(authError);
        }

        var username = request.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            return DisableReject(AccountMutationProblemCodes.UsernameInvalid);
        }

        var confirm = request.ConfirmUsername?.Trim();
        if (!string.Equals(username, confirm, StringComparison.OrdinalIgnoreCase))
        {
            return DisableReject(AccountMutationProblemCodes.ConfirmationMismatch);
        }

        var existing = request.Existing ?? [];
        var account = FindAccount(existing, username);
        if (account is null)
        {
            return DisableReject(AccountMutationProblemCodes.NotFound);
        }

        if (!account.Enabled)
        {
            return new AccountDisableEvaluation(
                true,
                null,
                account.Username,
                false,
                ActionAlreadyDisabled);
        }

        if (IsEnabledAdmin(account) && CountEnabledAdmins(existing) == 1)
        {
            return DisableReject(AccountMutationProblemCodes.LastAdmin);
        }

        return new AccountDisableEvaluation(
            true,
            null,
            account.Username,
            false,
            ActionDisable);
    }

    private static string? RejectBootstrapAuth(AccountMutationAuthContext auth)
    {
        if (auth.HasServiceBearer)
        {
            return null;
        }

        if (auth.RemoteIsLoopback && auth.IsLoopbackSession)
        {
            return null;
        }

        return AccountMutationProblemCodes.BootstrapNotAvailable;
    }

    private static string? RejectReadyAuth(AccountMutationAuthContext auth)
    {
        if (auth.IsLoopbackSession && !auth.HasServiceBearer)
        {
            return AccountMutationProblemCodes.BootstrapNotAvailable;
        }

        if (auth.HasServiceBearer || auth.HasAccountManage)
        {
            return null;
        }

        return AccountMutationProblemCodes.ManageForbidden;
    }

    private static bool TryNormalizeUsername(string? username, out string normalized)
    {
        normalized = (username ?? string.Empty).Trim();
        if (normalized.Length is < 3 or > UsernameMaxLength)
        {
            return false;
        }

        try
        {
            return UsernamePattern.IsMatch(normalized);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static bool TryNormalizeRole(string? role, out string normalized)
    {
        normalized = (role ?? string.Empty).Trim().ToLowerInvariant();
        return AllowedRoles.Contains(normalized, StringComparer.Ordinal);
    }

    private static bool IsPasswordValid(string? password, string username)
    {
        if (password is null ||
            password.Length < PasswordMinLength ||
            string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        return !string.Equals(password, username, StringComparison.OrdinalIgnoreCase);
    }

    private static AccountMutationExistingAccount? FindAccount(
        IReadOnlyList<AccountMutationExistingAccount> existing,
        string username)
    {
        return existing.FirstOrDefault(account =>
            string.Equals(account.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsEnabledAdmin(AccountMutationExistingAccount account)
    {
        return account.Enabled &&
            string.Equals(account.Role.Trim(), "admin", StringComparison.OrdinalIgnoreCase);
    }

    private static int CountEnabledAdmins(IReadOnlyList<AccountMutationExistingAccount> existing)
    {
        return existing.Count(IsEnabledAdmin);
    }

    private static AccountCreateEvaluation CreateReject(string code)
    {
        return new AccountCreateEvaluation(false, code, null, null, null, false, null);
    }

    private static AccountDisableEvaluation DisableReject(string code)
    {
        return new AccountDisableEvaluation(false, code, null, false, null);
    }
}
