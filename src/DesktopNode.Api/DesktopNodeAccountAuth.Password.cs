using System.Text;
using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Api;

public sealed partial class DesktopNodeAccountAuthService
{
    internal const string AccountMutationAuditFileName = "account-mutation-audit.jsonl";

    public DesktopNodeAuthActionResult ResetPassword(
        string? username,
        string? confirmUsername,
        string? newPassword,
        AccountMutationAuthContext auth,
        string actor)
    {
        lock (gate)
        {
            var evaluation = AccountMutationContract.EvaluatePasswordReset(new AccountPasswordResetRequest(
                username,
                confirmUsername,
                newPassword,
                SnapshotExisting(),
                auth));
            if (!evaluation.Ok)
            {
                return MutationError("account.password.reset", evaluation.ErrorCode!);
            }

            if (!accounts.TryGetValue(evaluation.Username!, out var account))
            {
                return MutationError("account.password.reset", AccountMutationProblemCodes.NotFound);
            }

            return ReplacePassword(account, newPassword!, "account.password.reset", actor, returnTokenPair: false);
        }
    }

    public DesktopNodeAuthActionResult ChangePassword(
        string username,
        string? currentPassword,
        string? newPassword)
    {
        lock (gate)
        {
            if (!accounts.TryGetValue(username, out var account))
            {
                return MutationError("auth.password.change", AccountMutationProblemCodes.NotFound);
            }

            if (!DesktopNodeAccountPassword.Verify(currentPassword ?? string.Empty, account.PasswordHash))
            {
                return MutationError("auth.password.change", AccountMutationProblemCodes.PasswordMismatch);
            }

            var passwordError = AccountMutationContract.EvaluateNewPassword(newPassword, account.Username);
            if (passwordError is not null)
            {
                return MutationError("auth.password.change", passwordError);
            }

            return ReplacePassword(account, newPassword!, "auth.password.change", account.Username, returnTokenPair: true);
        }
    }

    public DesktopNodeAuthActionResult RejectPasswordChange()
    {
        return MutationError("auth.password.change", AccountMutationProblemCodes.PasswordChangeForbidden);
    }

    private DesktopNodeAuthActionResult ReplacePassword(
        DesktopNodeAccountUser account,
        string newPassword,
        string operation,
        string actor,
        bool returnTokenPair)
    {
        if (string.IsNullOrWhiteSpace(options.AccountFilePath))
        {
            return Error(
                409,
                operation,
                "PCV_ACCOUNT_AUTH_CONFIG_INCOMPLETE",
                "Account file path is required.",
                "Pass --account-file before changing an account password.");
        }

        var updated = account with
        {
            PasswordHash = DesktopNodeAccountPassword.HashPassword(newPassword),
            CredentialVersion = account.CredentialVersion + 1
        };
        var next = CloneAccounts();
        next[account.Username] = updated;
        var persist = TryPersist(next.Values.ToArray(), AccountMutationContract.BootstrapAccountsConfigured);
        if (persist is not null)
        {
            return persist;
        }

        accounts = next;
        var audit = TryAppendAudit(operation, actor, account.Username);
        if (audit is not null)
        {
            return audit;
        }

        if (returnTokenPair)
        {
            return Success(operation, BuildTokenPair(updated));
        }

        return Success(operation, new SortedDictionary<string, object?>
        {
            ["enabled"] = updated.Enabled,
            ["username"] = updated.Username
        });
    }

    private DesktopNodeAuthActionResult? TryAppendAudit(string operation, string actor, string target)
    {
        var accountPath = options.AccountFilePath!;
        var directory = Path.GetDirectoryName(accountPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = ".";
        }

        var line = JsonSerializer.Serialize(new SortedDictionary<string, string>
        {
            ["actor"] = SanitizeAuditField(actor),
            ["at"] = options.Now().ToUniversalTime().ToString("o"),
            ["operation"] = operation,
            ["target"] = SanitizeAuditField(target)
        });
        try
        {
            File.AppendAllText(
                Path.Combine(directory, AccountMutationAuditFileName),
                line + "\n",
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Error(
                500,
                operation,
                "PCV_ACCOUNT_AUDIT_WRITE_FAILED",
                "The account audit record could not be written.",
                "The password change was saved, but the audit line was not.");
        }
    }

    private static string SanitizeAuditField(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Trim())
        {
            if (character is '\r' or '\n' or '\0')
            {
                continue;
            }

            builder.Append(character);
        }

        return builder.Length == 0 ? "unknown" : builder.ToString();
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
}
