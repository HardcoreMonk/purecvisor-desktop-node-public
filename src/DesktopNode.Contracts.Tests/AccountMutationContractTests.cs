using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class AccountMutationContractTests
{
    [Fact]
    public void BootstrapCreateAcceptsFirstAdminViaLoopbackSession()
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate());

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Equal("lab-admin", result.Username);
        Assert.Equal("admin", result.Role);
        Assert.True(result.Enabled);
        Assert.Equal(AccountMutationContract.BootstrapAccountsConfigured, result.BootstrapState);
        Assert.Equal("Lab Admin", result.DisplayName);
    }

    [Fact]
    public void BootstrapCreateAcceptsFirstAdminViaServiceBearer()
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with
        {
            Auth = new AccountMutationAuthContext(HasServiceBearer: true)
        });

        Assert.True(result.Ok);
        Assert.Equal("admin", result.Role);
    }

    [Fact]
    public void ReadyCreateAcceptsOperatorWhenCallerHasAccountManage()
    {
        var result = AccountMutationContract.EvaluateCreate(ReadyCreate() with
        {
            Username = "lab-operator",
            Role = "Operator",
            DisplayName = "  "
        });

        Assert.True(result.Ok);
        Assert.Equal("lab-operator", result.Username);
        Assert.Equal("operator", result.Role);
        Assert.Null(result.DisplayName);
        Assert.Equal(AccountMutationContract.BootstrapAccountsConfigured, result.BootstrapState);
    }

    [Fact]
    public void ReadyCreateAcceptsViewerViaServiceBearerWithoutAccountManage()
    {
        var result = AccountMutationContract.EvaluateCreate(ReadyCreate() with
        {
            Username = "lab-viewer",
            Role = "viewer",
            Auth = new AccountMutationAuthContext(HasServiceBearer: true)
        });

        Assert.True(result.Ok);
        Assert.Equal("viewer", result.Role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("1admin")]
    [InlineData("has space")]
    [InlineData("user@host")]
    [InlineData("loopback-session")]
    [InlineData("Loopback-Session")]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456")]
    public void CreateRejectsInvalidOrReservedUsername(string? username)
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with { Username = username });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.UsernameInvalid, result.ErrorCode);
    }

    [Fact]
    public void CreateAcceptsUsernameAtMaxLength()
    {
        var username = new string('a', AccountMutationContract.UsernameMaxLength);
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with
        {
            Username = username,
            Password = "not-the-username-1"
        });

        Assert.True(result.Ok);
        Assert.Equal(username, result.Username);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short-pass")]
    [InlineData("lab-admin")]
    [InlineData("LAB-ADMIN")]
    public void CreateRejectsInvalidPassword(string? password)
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with { Password = password });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.PasswordInvalid, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("root")]
    [InlineData("superuser")]
    public void CreateRejectsInvalidRole(string? role)
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with { Role = role });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.RoleInvalid, result.ErrorCode);
    }

    [Fact]
    public void BootstrapCreateRejectsNonAdminRole()
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with { Role = "operator" });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.BootstrapAdminRequired, result.ErrorCode);
    }

    [Fact]
    public void BootstrapCreateRejectsMissingLoopbackAndBearer()
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with
        {
            Auth = new AccountMutationAuthContext()
        });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.BootstrapNotAvailable, result.ErrorCode);
    }

    [Fact]
    public void BootstrapCreateRejectsLoopbackSessionFromNonLoopbackRemote()
    {
        var result = AccountMutationContract.EvaluateCreate(BootstrapCreate() with
        {
            Auth = new AccountMutationAuthContext(IsLoopbackSession: true)
        });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.BootstrapNotAvailable, result.ErrorCode);
    }

    [Fact]
    public void ReadyCreateRejectsLoopbackSession()
    {
        var result = AccountMutationContract.EvaluateCreate(ReadyCreate() with
        {
            Auth = new AccountMutationAuthContext(
                RemoteIsLoopback: true,
                IsLoopbackSession: true)
        });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.BootstrapNotAvailable, result.ErrorCode);
    }

    [Fact]
    public void ReadyCreateRejectsCallerWithoutAccountManage()
    {
        var result = AccountMutationContract.EvaluateCreate(ReadyCreate() with
        {
            Auth = new AccountMutationAuthContext()
        });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.ManageForbidden, result.ErrorCode);
    }

    [Fact]
    public void CreateRejectsCaseInsensitiveUsernameConflict()
    {
        var result = AccountMutationContract.EvaluateCreate(ReadyCreate() with { Username = "Lab-Admin" });
        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.UsernameConflict, result.ErrorCode);
    }

    [Fact]
    public void DisableSucceedsWhenAnotherEnabledAdminRemains()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "lab-operator",
            "lab-operator",
            [
                Admin("lab-admin"),
                Operator("lab-operator")
            ]));

        Assert.True(result.Ok);
        Assert.Equal("lab-operator", result.Username);
        Assert.False(result.Enabled);
        Assert.Equal(AccountMutationContract.ActionDisable, result.Action);
    }

    [Fact]
    public void DisableIsIdempotentWhenAlreadyDisabled()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "old-admin",
            "old-admin",
            [
                Admin("lab-admin"),
                Admin("old-admin", enabled: false)
            ]));

        Assert.True(result.Ok);
        Assert.Equal(AccountMutationContract.ActionAlreadyDisabled, result.Action);
        Assert.False(result.Enabled);
    }

    [Fact]
    public void DisableRejectsLastEnabledAdmin()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "lab-admin",
            "lab-admin",
            [
                Admin("lab-admin"),
                Operator("lab-operator"),
                Admin("retired", enabled: false)
            ]));

        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.LastAdmin, result.ErrorCode);
    }

    [Fact]
    public void DisableAllowsOneAdminWhenAnotherEnabledAdminExists()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "lab-admin",
            "LAB-ADMIN",
            [
                Admin("lab-admin"),
                Admin("other-admin")
            ]));

        Assert.True(result.Ok);
        Assert.Equal("lab-admin", result.Username);
        Assert.Equal(AccountMutationContract.ActionDisable, result.Action);
    }

    [Fact]
    public void DisableRejectsConfirmationMismatch()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "lab-operator",
            "other",
            [Admin("lab-admin"), Operator("lab-operator")]));

        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.ConfirmationMismatch, result.ErrorCode);
    }

    [Fact]
    public void DisableRejectsUnknownUsername()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "missing",
            "missing",
            [Admin("lab-admin")]));

        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.NotFound, result.ErrorCode);
    }

    [Fact]
    public void DisableRejectsEmptyUsername()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "  ",
            "  ",
            [Admin("lab-admin")]));

        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.UsernameInvalid, result.ErrorCode);
    }

    [Fact]
    public void DisableRejectsLoopbackSession()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "lab-operator",
            "lab-operator",
            [Admin("lab-admin"), Operator("lab-operator")],
            new AccountMutationAuthContext(RemoteIsLoopback: true, IsLoopbackSession: true)));

        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.BootstrapNotAvailable, result.ErrorCode);
    }

    [Fact]
    public void DisableRejectsCallerWithoutAccountManage()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "lab-operator",
            "lab-operator",
            [Admin("lab-admin"), Operator("lab-operator")],
            new AccountMutationAuthContext()));

        Assert.False(result.Ok);
        Assert.Equal(AccountMutationProblemCodes.ManageForbidden, result.ErrorCode);
    }

    [Fact]
    public void DisableAcceptsServiceBearerWithoutAccountManage()
    {
        var result = AccountMutationContract.EvaluateDisable(DisableRequest(
            "lab-operator",
            "lab-operator",
            [Admin("lab-admin"), Operator("lab-operator")],
            new AccountMutationAuthContext(HasServiceBearer: true)));

        Assert.True(result.Ok);
        Assert.Equal(AccountMutationContract.ActionDisable, result.Action);
    }

    private static AccountCreateRequest BootstrapCreate()
    {
        return new AccountCreateRequest(
            "lab-admin",
            "correct-horse",
            "admin",
            [],
            new AccountMutationAuthContext(RemoteIsLoopback: true, IsLoopbackSession: true),
            "Lab Admin");
    }

    private static AccountCreateRequest ReadyCreate()
    {
        return new AccountCreateRequest(
            "lab-operator",
            "correct-horse",
            "operator",
            [Admin("lab-admin")],
            new AccountMutationAuthContext(HasAccountManage: true));
    }

    private static AccountDisableRequest DisableRequest(
        string? username,
        string? confirmUsername,
        IReadOnlyList<AccountMutationExistingAccount> existing,
        AccountMutationAuthContext? auth = null)
    {
        return new AccountDisableRequest(
            username,
            confirmUsername,
            existing,
            auth ?? new AccountMutationAuthContext(HasAccountManage: true));
    }

    private static AccountMutationExistingAccount Admin(string username, bool enabled = true)
    {
        return new AccountMutationExistingAccount(username, "admin", enabled);
    }

    private static AccountMutationExistingAccount Operator(string username, bool enabled = true)
    {
        return new AccountMutationExistingAccount(username, "operator", enabled);
    }
}
