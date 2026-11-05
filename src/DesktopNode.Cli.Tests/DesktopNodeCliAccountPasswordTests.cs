using System.Text.Json;

namespace DesktopNode.Cli.Tests;

public sealed class DesktopNodeCliAccountPasswordTests
{
    private const string Secret = "correct-horse-marker";

    [Fact]
    public void ResetPasswordRejectsPasswordArgumentsWithoutEchoingTheValue()
    {
        string[][] commands =
        [
            ["account", "reset-password", "lab-operator", "--password", Secret, "--yes"],
            ["account", "reset-password", "lab-operator", "--password=" + Secret, "--yes"],
            ["account", "reset-password", "lab-operator", "--new-password", Secret, "--yes"],
            ["account", "change-password", "--current-password=" + Secret, "--password-env", "PCV_ACCOUNT_PASSWORD_TEST"]
        ];

        foreach (var command in commands)
        {
            var error = Assert.Throws<ArgumentException>(() => DesktopNodeCliCommandCatalog.CreateRequest(command));
            Assert.Contains("PCV_CLI_USAGE|Passwords are not accepted as arguments.", error.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(Secret, error.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ResetPasswordRequiresYesAndDropsUnexpectedPositionals()
    {
        var missingYes = Assert.Throws<ArgumentException>(() => DesktopNodeCliCommandCatalog.CreateRequest(
            ["account", "reset-password", "lab-operator", "--password-env", "PCV_ACCOUNT_PASSWORD_TEST"]));
        Assert.Contains("PCV_CLI_CONFIRMATION_REQUIRED", missingYes.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(Secret, missingYes.Message, StringComparison.Ordinal);

        var positional = Assert.Throws<ArgumentException>(() => DesktopNodeCliCommandCatalog.CreateRequest(
            ["account", "reset-password", "lab-operator", Secret, "--password-env", "PCV_ACCOUNT_PASSWORD_TEST", "--yes"]));
        Assert.Equal("PCV_CLI_USAGE|Unexpected positional argument.", positional.Message);
        Assert.DoesNotContain(Secret, positional.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParsedPasswordCommandsKeepTheValueOutOfThePathAndUsage()
    {
        Environment.SetEnvironmentVariable("PCV_ACCOUNT_PASSWORD_TEST", Secret);
        Environment.SetEnvironmentVariable("PCV_ACCOUNT_CURRENT_PASSWORD_TEST", Secret);
        try
        {
            var reset = DesktopNodeCliCommandCatalog.CreateRequest(
            [
                "account", "reset-password", "lab-operator", "--password-env", "PCV_ACCOUNT_PASSWORD_TEST", "--yes"
            ]);
            Assert.Equal("POST", reset.Method);
            Assert.Equal("/api/v1/accounts/lab-operator/password", reset.Path);
            Assert.DoesNotContain(Secret, reset.Path, StringComparison.Ordinal);
            AssertPasswordField(reset.Body, "new_password");
            Assert.DoesNotContain(Secret, DesktopNodeCliCommandCatalog.GetUsage(), StringComparison.Ordinal);

            var change = DesktopNodeCliCommandCatalog.CreateRequest(
            [
                "account", "change-password",
                "--current-password-env", "PCV_ACCOUNT_CURRENT_PASSWORD_TEST",
                "--password-env", "PCV_ACCOUNT_PASSWORD_TEST"
            ]);
            Assert.Equal("POST", change.Method);
            Assert.Equal("/api/v1/auth/password", change.Path);
            Assert.DoesNotContain(Secret, change.Path, StringComparison.Ordinal);
            AssertPasswordField(change.Body, "current_password");
            AssertPasswordField(change.Body, "new_password");
        }
        finally
        {
            Environment.SetEnvironmentVariable("PCV_ACCOUNT_PASSWORD_TEST", null);
            Environment.SetEnvironmentVariable("PCV_ACCOUNT_CURRENT_PASSWORD_TEST", null);
        }
    }

    private static void AssertPasswordField(string? body, string field)
    {
        using var document = JsonDocument.Parse(body!);
        var actual = document.RootElement.GetProperty(field).GetString();
        Assert.Equal(Secret, actual);
    }
}
