using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DesktopNode.Contracts;

public static class GuestExecutionSecurityContract
{
    public const string AdrPath = "docs/adr/0009-guest-execution-security-boundary.md";
    public const string GuestExecPreviewContract = "guest-execution-preview.v1";
    public const string GuestChannelPreviewContract = "guest-channel-preview.v1";
    public const string AuditSchema = "guest-execution-audit-v1";
    public const string RedactionPolicy = "guest-execution-redaction-v1";
    public const string DisabledStatus = "accepted-boundary-contract-route-not-implemented";
    public const string PreviewStatus = "accepted-boundary-contract-preview-enabled-execute-disabled";
    public const string ProviderEnabledStatus = "provider-verify-repair-queued-execution-enabled";

    public static readonly IReadOnlyList<string> RequiredCapabilities =
    [
        "operate",
        "guest.exec",
        "guest.channel.configure",
        "job.cancel"
    ];

    public static readonly IReadOnlyList<string> ProblemCodes =
    [
        GuestExecutionProblemCodes.Disabled,
        GuestExecutionProblemCodes.PermissionDenied,
        GuestExecutionProblemCodes.ChannelUnavailable,
        GuestExecutionProblemCodes.Timeout,
        GuestExecutionProblemCodes.Cancelled,
        GuestExecutionProblemCodes.OutputLimitExceeded,
        GuestExecutionProblemCodes.SecretRedactionRequired,
        GuestExecutionProblemCodes.TransportUnsupported,
        GuestExecutionProblemCodes.CredentialRefRequired,
        GuestExecutionProblemCodes.CommandRequired
    ];
}

public static class GuestExecutionProblemCodes
{
    public const string Disabled = "PCV_GUEST_EXEC_DISABLED";
    public const string PermissionDenied = "PCV_GUEST_EXEC_PERMISSION_DENIED";
    public const string ChannelUnavailable = "PCV_GUEST_EXEC_CHANNEL_UNAVAILABLE";
    public const string Timeout = "PCV_GUEST_EXEC_TIMEOUT";
    public const string Cancelled = "PCV_GUEST_EXEC_CANCELLED";
    public const string OutputLimitExceeded = "PCV_GUEST_EXEC_OUTPUT_LIMIT_EXCEEDED";
    public const string SecretRedactionRequired = "PCV_GUEST_EXEC_SECRET_REDACTION_REQUIRED";
    public const string TransportUnsupported = "PCV_GUEST_EXEC_TRANSPORT_UNSUPPORTED";
    public const string CredentialRefRequired = "PCV_GUEST_EXEC_CREDENTIAL_REF_REQUIRED";
    public const string CommandRequired = "PCV_GUEST_EXEC_COMMAND_REQUIRED";
}

public sealed record GuestExecutionCredentialReferenceResolution(
    bool Ok,
    string? Storage,
    string? Target,
    string? ErrorCode,
    string? Secret);

public static class GuestExecutionCredentialReferenceResolver
{
    public static GuestExecutionCredentialReferenceResolution Resolve(string? credentialRef)
    {
        if (string.IsNullOrWhiteSpace(credentialRef))
        {
            return Reject();
        }

        var trimmed = credentialRef.Trim();
        if (TryStripPrefix(trimmed, "wincred:", out var winCredTarget) ||
            TryStripPrefix(trimmed, "credential-manager:", out winCredTarget))
        {
            return string.IsNullOrWhiteSpace(winCredTarget)
                ? Reject()
                : new GuestExecutionCredentialReferenceResolution(true, "windows-credential-manager", winCredTarget, null, null);
        }

        if (TryStripPrefix(trimmed, "dpapi:", out var dpapiTarget))
        {
            return string.IsNullOrWhiteSpace(dpapiTarget)
                ? Reject()
                : new GuestExecutionCredentialReferenceResolution(true, "dpapi-local-machine", dpapiTarget, null, null);
        }

        return Reject();
    }

    private static bool TryStripPrefix(string value, string prefix, out string stripped)
    {
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            stripped = value[prefix.Length..];
            return true;
        }

        stripped = string.Empty;
        return false;
    }

    private static GuestExecutionCredentialReferenceResolution Reject()
    {
        return new GuestExecutionCredentialReferenceResolution(
            false,
            null,
            null,
            GuestExecutionProblemCodes.CredentialRefRequired,
            null);
    }
}

public sealed record GuestExecutionRedactionResult(
    [property: JsonPropertyName("policy")] string Policy,
    [property: JsonPropertyName("arguments")] IReadOnlyList<string> Arguments,
    [property: JsonPropertyName("environment")] IReadOnlyDictionary<string, string> Environment,
    [property: JsonPropertyName("redaction_applied")] bool RedactionApplied,
    [property: JsonPropertyName("redacted_argument_indexes")] IReadOnlyList<int> RedactedArgumentIndexes,
    [property: JsonPropertyName("redacted_environment_keys")] IReadOnlyList<string> RedactedEnvironmentKeys);

public static class GuestExecutionRedactor
{
    private static readonly Regex CompactSecretTokenPattern = new(
        @"\b(?:AKIA|ASIA)[0-9A-Z]{16}\b",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly string[] SecretKeyFragments =
    [
        "password",
        "passwd",
        "pwd",
        "token",
        "secret",
        "key",
        "credential",
        "authorization",
        "cookie"
    ];

    public static GuestExecutionRedactionResult Redact(
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string>? environment)
    {
        var redactedArguments = new List<string>(arguments.Count);
        var redactedArgumentIndexes = new List<int>();
        var redactNext = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index] ?? string.Empty;
            if (redactNext)
            {
                redactedArguments.Add("[REDACTED]");
                redactedArgumentIndexes.Add(index);
                redactNext = false;
                continue;
            }

            if (TryRedactAssignment(argument, out var redactedAssignment))
            {
                redactedArguments.Add(redactedAssignment);
                redactedArgumentIndexes.Add(index);
                continue;
            }

            if (LooksSecretValue(argument))
            {
                redactedArguments.Add("[REDACTED]");
                redactedArgumentIndexes.Add(index);
                continue;
            }

            redactedArguments.Add(argument);
            redactNext = IsSecretKey(argument);
        }

        var redactedEnvironment = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var redactedEnvironmentKeys = new List<string>();
        foreach (var item in environment ?? new Dictionary<string, string>())
        {
            if (IsSecretKey(item.Key) || LooksSecretValue(item.Value))
            {
                redactedEnvironment[item.Key] = "[REDACTED]";
                redactedEnvironmentKeys.Add(item.Key);
            }
            else
            {
                redactedEnvironment[item.Key] = item.Value;
            }
        }

        return new GuestExecutionRedactionResult(
            GuestExecutionSecurityContract.RedactionPolicy,
            redactedArguments,
            redactedEnvironment,
            redactedArgumentIndexes.Count > 0 || redactedEnvironmentKeys.Count > 0,
            redactedArgumentIndexes,
            redactedEnvironmentKeys.OrderBy(key => key, StringComparer.Ordinal).ToArray());
    }

    private static bool TryRedactAssignment(string value, out string redacted)
    {
        var separator = value.IndexOf('=');
        if (separator > 0 && IsSecretKey(value[..separator]))
        {
            redacted = value[..(separator + 1)] + "[REDACTED]";
            return true;
        }

        redacted = value;
        return false;
    }

    private static bool IsSecretKey(string value)
    {
        return SecretKeyFragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksSecretValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("eyJ", StringComparison.Ordinal) && value.Count(character => character == '.') >= 2 ||
            value.Contains("BEGIN PRIVATE KEY", StringComparison.OrdinalIgnoreCase) ||
            CompactSecretTokenPattern.IsMatch(value) ||
            LooksHighEntropyToken(value);
    }

    private static bool LooksHighEntropyToken(string value)
    {
        var token = value.Trim();
        if (token.Length < 24)
        {
            return false;
        }

        if (token.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var allowedCharacters = token.Count(character =>
            char.IsLetterOrDigit(character) ||
            character is '_' or '-' or '/' or '+' or '=');
        if (allowedCharacters != token.Length)
        {
            return false;
        }

        var distinctCharacters = token.Distinct().Count();
        if (distinctCharacters < 12)
        {
            return false;
        }

        var lettersOrDigits = token.Count(char.IsLetterOrDigit);
        return lettersOrDigits >= token.Length * 0.75;
    }
}

public sealed record GuestExecutionAuditRecord(
    [property: JsonPropertyName("schema_version")] string SchemaVersion,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("actor")] string Actor,
    [property: JsonPropertyName("vm_id")] string VmId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("command_hash")] string CommandHash,
    [property: JsonPropertyName("redaction_policy")] string RedactionPolicy,
    [property: JsonPropertyName("redacted_argv")] IReadOnlyList<string> RedactedArguments,
    [property: JsonPropertyName("redacted_env_keys")] IReadOnlyList<string> RedactedEnvironmentKeys,
    [property: JsonPropertyName("credential_ref_hash")] string? CredentialRefHash);

public sealed record GuestExecutionCredentialReferenceProjection(
    [property: JsonPropertyName("supplied")] bool Supplied,
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("storage")] string? Storage,
    [property: JsonPropertyName("target_hash")] string? TargetHash,
    [property: JsonPropertyName("error_code")] string? ErrorCode);

public sealed record GuestExecutionPreviewResult(
    [property: JsonPropertyName("contract")] string Contract,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("vm_id")] string VmId,
    [property: JsonPropertyName("preview_enabled")] bool PreviewEnabled,
    [property: JsonPropertyName("execute_enabled")] bool ExecuteEnabled,
    [property: JsonPropertyName("execution_queued")] bool ExecutionQueued,
    [property: JsonPropertyName("host_mutation_performed")] bool HostMutationPerformed,
    [property: JsonPropertyName("required_capability")] string RequiredCapability,
    [property: JsonPropertyName("credential_ref")] GuestExecutionCredentialReferenceProjection CredentialReference,
    [property: JsonPropertyName("command_hash")] string CommandHash,
    [property: JsonPropertyName("redaction")] GuestExecutionRedactionResult Redaction,
    [property: JsonPropertyName("audit_preview")] GuestExecutionAuditRecord AuditPreview,
    [property: JsonPropertyName("timeout_sec")] int TimeoutSeconds,
    [property: JsonPropertyName("next_action")] string NextAction);

public sealed record GuestChannelPreviewResult(
    [property: JsonPropertyName("contract")] string Contract,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("vm_id")] string VmId,
    [property: JsonPropertyName("preview_enabled")] bool PreviewEnabled,
    [property: JsonPropertyName("verify_enabled")] bool VerifyEnabled,
    [property: JsonPropertyName("repair_enabled")] bool RepairEnabled,
    [property: JsonPropertyName("host_mutation_performed")] bool HostMutationPerformed,
    [property: JsonPropertyName("guest_command_performed")] bool GuestCommandPerformed,
    [property: JsonPropertyName("required_capability")] string RequiredCapability,
    [property: JsonPropertyName("candidate_transports")] IReadOnlyList<string> CandidateTransports,
    [property: JsonPropertyName("audit_preview")] GuestExecutionAuditRecord AuditPreview,
    [property: JsonPropertyName("next_action")] string NextAction);

public static class GuestExecutionContractHasher
{
    public static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string HashValues(IEnumerable<string> values)
    {
        return Hash(string.Join("\0", values));
    }
}

public static class GuestExecutionAuditWriter
{
    public static GuestExecutionAuditRecord CreateRecord(
        string operation,
        string requestId,
        string actor,
        string vmId,
        string? credentialRef,
        GuestExecutionRedactionResult redaction,
        string status)
    {
        return new GuestExecutionAuditRecord(
            GuestExecutionSecurityContract.AuditSchema,
            operation,
            requestId,
            actor,
            vmId,
            status,
            GuestExecutionContractHasher.HashValues(redaction.Arguments),
            redaction.Policy,
            redaction.Arguments,
            redaction.RedactedEnvironmentKeys,
            string.IsNullOrWhiteSpace(credentialRef) ? null : GuestExecutionContractHasher.Hash(credentialRef.Trim()));
    }
}
