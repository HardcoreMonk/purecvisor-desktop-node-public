using System.Text.Json;
using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class GuestExecutionSecurityContractTests
{
    [Fact]
    public void CredentialReferenceResolverAcceptsOnlyProtectedReferences()
    {
        var resolved = GuestExecutionCredentialReferenceResolver.Resolve("wincred:PureCVisor/guest/admin");
        var rejected = GuestExecutionCredentialReferenceResolver.Resolve("raw-password-value");

        Assert.True(resolved.Ok);
        Assert.Equal("windows-credential-manager", resolved.Storage);
        Assert.Equal("PureCVisor/guest/admin", resolved.Target);
        Assert.Null(resolved.Secret);
        Assert.False(rejected.Ok);
        Assert.Equal("PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED", rejected.ErrorCode);
    }

    [Fact]
    public void RedactorMasksSecretLikeArgumentsAndEnvironment()
    {
        var result = GuestExecutionRedactor.Redact(
            [
                "powershell",
                "--password=super-secret-value",
                "--token",
                "eyJhbGciOi.fake.jwt",
                "plain"
            ],
            new Dictionary<string, string>
            {
                ["API_TOKEN"] = "super-secret-token",
                ["PATH"] = "C:\\Windows"
            });

        Assert.Equal("guest-execution-redaction-v1", result.Policy);
        Assert.True(result.RedactionApplied);
        Assert.Equal(
            ["powershell", "--password=[REDACTED]", "--token", "[REDACTED]", "plain"],
            result.Arguments);
        Assert.Equal("[REDACTED]", result.Environment["API_TOKEN"]);
        Assert.Equal("C:\\Windows", result.Environment["PATH"]);
        Assert.Contains("API_TOKEN", result.RedactedEnvironmentKeys);

        var json = JsonSerializer.Serialize(result, RuntimePolicyContract.JsonOptions);
        Assert.DoesNotContain("super-secret", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fake.jwt", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RedactorMasksHighEntropyTokensBeforeGuestExecutionPreview()
    {
        var result = GuestExecutionRedactor.Redact(
            [
                "powershell",
                "AKIAIOSFODNN7EXAMPLE",
                "plain"
            ],
            new Dictionary<string, string>
            {
                ["SESSION_ID"] = "N0j4pX8wQ2sL6mR9vT1zY3cB5nH7kD4f"
            });

        Assert.True(result.RedactionApplied);
        Assert.Equal(["powershell", "[REDACTED]", "plain"], result.Arguments);
        Assert.Equal("[REDACTED]", result.Environment["SESSION_ID"]);

        var json = JsonSerializer.Serialize(result, RuntimePolicyContract.JsonOptions);
        Assert.DoesNotContain("AKIAIOSFODNN7EXAMPLE", json, StringComparison.Ordinal);
        Assert.DoesNotContain("N0j4pX8wQ2sL6mR9vT1zY3cB5nH7kD4f", json, StringComparison.Ordinal);
    }

    [Fact]
    public void AuditRecordSerializesReferenceHashesWithoutRawSecrets()
    {
        var redaction = GuestExecutionRedactor.Redact(
            ["powershell", "--password=super-secret-value"],
            new Dictionary<string, string>());
        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation: "vm.guest.exec.preview",
            requestId: "req-guest-preview",
            actor: "local-api-operator",
            vmId: "alpha",
            credentialRef: "wincred:PureCVisor/guest/admin",
            redaction: redaction,
            status: "blocked_disabled");

        var json = JsonSerializer.Serialize(audit, RuntimePolicyContract.JsonOptions);

        Assert.Contains("guest-execution-audit-v1", json, StringComparison.Ordinal);
        Assert.Contains("credential_ref_hash", json, StringComparison.Ordinal);
        Assert.DoesNotContain("PureCVisor/guest/admin", json, StringComparison.Ordinal);
        Assert.DoesNotContain("super-secret", json, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("req-guest-preview", audit.RequestId);
        Assert.Equal("blocked_disabled", audit.Status);
    }

    [Fact]
    public void PreviewResultSerializesWithoutRawCredentialReferenceOrSecrets()
    {
        var redaction = GuestExecutionRedactor.Redact(
            ["powershell", "--password=super-secret-value", "Get-Process"],
            new Dictionary<string, string>
            {
                ["API_TOKEN"] = "Bearer abc.def.ghi"
            });
        var audit = GuestExecutionAuditWriter.CreateRecord(
            operation: "vm.guest.exec.preview",
            requestId: "req-guest-preview",
            actor: "admin",
            vmId: "alpha",
            credentialRef: "wincred:PureCVisor/guest/admin",
            redaction: redaction,
            status: "preview_only_execute_available");
        var result = new GuestExecutionPreviewResult(
            GuestExecutionSecurityContract.GuestExecPreviewContract,
            "dry-run",
            "alpha",
            PreviewEnabled: true,
            ExecuteEnabled: true,
            ExecutionQueued: false,
            HostMutationPerformed: false,
            RequiredCapability: "guest.exec",
            CredentialReference: new GuestExecutionCredentialReferenceProjection(
                Supplied: true,
                Valid: true,
                Storage: "windows-credential-manager",
                TargetHash: GuestExecutionContractHasher.Hash("PureCVisor/guest/admin"),
                ErrorCode: null),
            CommandHash: audit.CommandHash,
            Redaction: redaction,
            AuditPreview: audit,
            TimeoutSeconds: 60,
            NextAction: "preview only");

        var json = JsonSerializer.Serialize(result, RuntimePolicyContract.JsonOptions);

        Assert.Contains("guest-execution-preview.v1", json, StringComparison.Ordinal);
        Assert.Contains("command_hash", json, StringComparison.Ordinal);
        Assert.Contains("target_hash", json, StringComparison.Ordinal);
        Assert.DoesNotContain("PureCVisor/guest/admin", json, StringComparison.Ordinal);
        Assert.DoesNotContain("super-secret-value", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("abc.def.ghi", json, StringComparison.OrdinalIgnoreCase);
        Assert.True(result.ExecuteEnabled);
        Assert.False(result.HostMutationPerformed);
    }
}
