using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security;

namespace DesktopNode.Verification;

internal sealed record VerificationCatalog(
    int SchemaVersion,
    string Contract,
    string ActivationState,
    int MaxParallelism,
    int OverallTimeoutSeconds,
    IReadOnlyList<string> AllowedExecutables,
    IReadOnlyList<SuiteDefinition> Suites,
    IReadOnlyList<ShardDefinition> Shards);

internal sealed record SuiteDefinition(
    string Id,
    string Owner,
    string MigrationState,
    string ExecutorKind,
    string? FileName,
    IReadOnlyList<string> Arguments,
    string? ManagedHandler,
    int TimeoutSeconds);

internal sealed record ShardDefinition(string Id, IReadOnlyList<string> SuiteIds);

internal sealed class VerificationCatalogLoader(IVerificationFileSystem fileSystem)
{
    private const string ExpectedSchemaId = "pcv-development-verification-suite-catalog-schema-v1";
    private const string ExpectedSchemaReference = "./development-verification-suites.schema.json";
    private const string ExpectedContract = "pcv-development-verification-suite-catalog-v1";
    private const string ExpectedActivationState = "plan-only-foundation";

    private static readonly string[] ExpectedExecutables =
        ["dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"];

    private static readonly string[] ExpectedSuiteIds =
        ["dotnet", "web-typecheck", "web-parity", "delivery-contracts", "installer-contracts", "evidence-check", "policy-boundaries"];

    private static readonly string[] ExpectedShardIds = ["dotnet", "web", "delivery", "installer-policy"];
    private static readonly string[] AllowedOwners = ["csharp", "node"];
    private static readonly string[] AllowedMigrationStates =
        ["native-existing", "wave-a-foundation", "wave-b-pending", "wave-c-pending", "wave-d-pending"];
    private static readonly string[] AllowedManagedHandlers = ["current-evidence-check", "policy-boundaries"];
    private static readonly string[] PowerShellTokens = ["pwsh", "powershell", "Invoke-Pester"];
    private static readonly string[] ForbiddenCommandTokens =
        ["msiexec", "sc.exe", "New-VM", "Start-VM", "Stop-VM", "Start-Service", "Stop-Service", "Install-Module", "AllowHostMutation"];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    internal VerificationCatalog Load(string catalogPath, string schemaPath)
    {
        ValidateSchema(schemaPath);
        var dto = DeserializeCatalog(catalogPath);
        ValidateHeader(dto);

        var allowedExecutables = ValidateExecutables(dto.AllowedExecutables);
        var suites = ValidateSuites(dto, allowedExecutables);
        var shards = ValidateShards(dto.Shards, suites);

        return new VerificationCatalog(
            dto.SchemaVersion!.Value,
            dto.Contract!,
            dto.ActivationState!,
            dto.MaxParallelism!.Value,
            dto.OverallTimeoutSeconds!.Value,
            Array.AsReadOnly(allowedExecutables),
            Array.AsReadOnly(suites),
            Array.AsReadOnly(shards));
    }

    private void ValidateSchema(string schemaPath)
    {
        bool schemaExists;
        try
        {
            schemaExists = fileSystem.FileExists(schemaPath);
        }
        catch (Exception exception) when (IsExpectedReadFailure(exception))
        {
            throw Invalid("schema-file=read-failed");
        }

        if (!schemaExists)
        {
            throw Invalid("schema-file=missing");
        }

        string contents;
        try
        {
            contents = fileSystem.ReadAllText(schemaPath);
        }
        catch (Exception exception) when (IsExpectedReadFailure(exception))
        {
            throw Invalid("schema-file=read-failed");
        }

        try
        {
            using var document = JsonDocument.Parse(contents);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw Invalid("schema-id=root-not-object");
            }

            if (!document.RootElement.TryGetProperty("$id", out var id) ||
                id.ValueKind != JsonValueKind.String ||
                !string.Equals(id.GetString(), ExpectedSchemaId, StringComparison.Ordinal))
            {
                throw Invalid("schema-id=mismatch");
            }
        }
        catch (VerificationException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw Invalid("schema-file=invalid-json");
        }
    }

    private CatalogDto DeserializeCatalog(string catalogPath)
    {
        string contents;
        try
        {
            contents = fileSystem.ReadAllText(catalogPath);
        }
        catch (Exception exception) when (IsExpectedReadFailure(exception))
        {
            throw Invalid("catalog-json=read-failed");
        }

        try
        {
            RejectDuplicateProperties(contents);
            var dto = JsonSerializer.Deserialize<CatalogDto>(contents, SerializerOptions);
            return dto ?? throw Invalid("catalog-json=null-root");
        }
        catch (VerificationException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw Invalid("catalog-json=invalid-json");
        }
    }

    private static void RejectDuplicateProperties(string contents)
    {
        using var document = JsonDocument.Parse(contents);
        RejectDuplicateProperties(document.RootElement);
    }

    private static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var propertyNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!propertyNames.Add(property.Name))
                {
                    throw Invalid("catalog-json=duplicate-property");
                }

                RejectDuplicateProperties(property.Value);
            }

            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                RejectDuplicateProperties(item);
            }
        }
    }

    private static void ValidateHeader(CatalogDto dto)
    {
        if (!string.Equals(dto.Schema, ExpectedSchemaReference, StringComparison.Ordinal) ||
            dto.SchemaVersion != 1 ||
            !string.Equals(dto.Contract, ExpectedContract, StringComparison.Ordinal) ||
            !string.Equals(dto.ActivationState, ExpectedActivationState, StringComparison.Ordinal) ||
            dto.MaxParallelism != 4 ||
            dto.OverallTimeoutSeconds is null or < 1 or > 3600)
        {
            throw Invalid("catalog-header=mismatch");
        }
    }

    private static string[] ValidateExecutables(string?[]? executables)
    {
        var normalized = executables?
            .Select(executable => Path.GetFileName(executable ?? string.Empty).ToLowerInvariant())
            .ToArray();

        if (normalized is null || normalized.Length != ExpectedExecutables.Length)
        {
            throw Invalid("executable-allowlist=mismatch");
        }

        var normalizedSet = normalized.ToHashSet(StringComparer.Ordinal);
        if (normalizedSet.Count != ExpectedExecutables.Length || !normalizedSet.SetEquals(ExpectedExecutables))
        {
            throw Invalid("executable-allowlist=mismatch");
        }

        return normalized;
    }

    private static SuiteDefinition[] ValidateSuites(CatalogDto dto, IReadOnlyList<string> allowedExecutables)
    {
        if (dto.Suites is null || dto.Suites.Length != ExpectedSuiteIds.Length)
        {
            throw Invalid("suite-definition:<catalog>=count");
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var suites = new SuiteDefinition[dto.Suites.Length];
        for (var index = 0; index < dto.Suites.Length; index++)
        {
            var suite = dto.Suites[index];
            if (suite is null)
            {
                throw Invalid("suite-definition:<missing>=invalid");
            }

            var id = suite.Id ?? "<missing>";
            var prefix = $"suite-definition:{id}";

            if (!string.Equals(id, ExpectedSuiteIds[index], StringComparison.Ordinal) ||
                !seen.Add(id) ||
                !AllowedOwners.Contains(suite.Owner, StringComparer.Ordinal) ||
                !AllowedMigrationStates.Contains(suite.MigrationState, StringComparer.Ordinal) ||
                suite.TimeoutSeconds is null or < 1 ||
                suite.TimeoutSeconds > dto.OverallTimeoutSeconds ||
                suite.Executor is null)
            {
                throw Invalid($"{prefix}=invalid");
            }

            var executor = suite.Executor;
            var isProcess = string.Equals(executor.Kind, "process", StringComparison.Ordinal);
            var isManaged = string.Equals(executor.Kind, "managed", StringComparison.Ordinal);
            if ((isProcess &&
                 (string.IsNullOrWhiteSpace(executor.FileName) ||
                  executor.Arguments is null ||
                  executor.Arguments.Any(argument => argument is null) ||
                  executor.ManagedHandler is not null)) ||
                (isManaged &&
                 (!AllowedManagedHandlers.Contains(executor.ManagedHandler, StringComparer.Ordinal) ||
                  executor.FileName is not null || executor.Arguments is not null)) ||
                (!isProcess && !isManaged))
            {
                throw Invalid($"{prefix}=executor-union");
            }

            var arguments = executor.Arguments?.Select(argument => argument!).ToArray() ?? [];
            suites[index] = new SuiteDefinition(
                id,
                suite.Owner!,
                suite.MigrationState!,
                executor.Kind!,
                executor.FileName,
                Array.AsReadOnly(arguments.ToArray()),
                executor.ManagedHandler,
                suite.TimeoutSeconds.Value);
        }

        for (var index = 0; index < suites.Length; index++)
        {
            var suite = suites[index];
            suites[index] = suite with
            {
                FileName = ValidateCommand(suite.Id, suite.FileName, suite.Arguments, allowedExecutables)
            };
        }

        return suites;
    }

    private static string? ValidateCommand(
        string suiteId,
        string? fileName,
        IReadOnlyList<string> arguments,
        IReadOnlyList<string> allowedExecutables)
    {
        if (fileName is null)
        {
            return null;
        }

        if (Path.IsPathRooted(fileName) || fileName.IndexOfAny(['\\', '/']) >= 0)
        {
            throw Invalid($"catalog-command-forbidden:{suiteId}");
        }

        var commandParts = new[] { fileName }.Concat(arguments).ToArray();
        if (commandParts.Any(part => PowerShellTokens.Any(token =>
                part.Contains(token, StringComparison.OrdinalIgnoreCase))))
        {
            throw new VerificationException(
                VerificationErrorCodes.NonAdminPowerShellForbidden,
                $"catalog-command-forbidden:{suiteId}");
        }

        var normalizedFileName = Path.GetFileName(fileName).ToLowerInvariant();
        if (!allowedExecutables.Contains(normalizedFileName, StringComparer.Ordinal) ||
            commandParts.Any(part => ForbiddenCommandTokens.Any(token =>
                part.Contains(token, StringComparison.OrdinalIgnoreCase))))
        {
            throw Invalid($"catalog-command-forbidden:{suiteId}");
        }

        return normalizedFileName;
    }

    private static ShardDefinition[] ValidateShards(ShardDto?[]? shardDtos, IReadOnlyList<SuiteDefinition> suites)
    {
        if (shardDtos is null || shardDtos.Length != ExpectedShardIds.Length)
        {
            throw Invalid("shard-union=count");
        }

        var knownSuiteIds = suites.Select(suite => suite.Id).ToHashSet(StringComparer.Ordinal);
        var seenShardIds = new HashSet<string>(StringComparer.Ordinal);
        var union = new HashSet<string>(StringComparer.Ordinal);
        var shards = new ShardDefinition[shardDtos.Length];

        for (var index = 0; index < shardDtos.Length; index++)
        {
            var shard = shardDtos[index];
            if (shard is null)
            {
                throw Invalid("shard-union=invalid-shard");
            }

            if (string.IsNullOrWhiteSpace(shard.Id) ||
                !string.Equals(shard.Id, ExpectedShardIds[index], StringComparison.Ordinal) ||
                !seenShardIds.Add(shard.Id) ||
                shard.SuiteIds is null or { Length: 0 })
            {
                throw Invalid("shard-union=invalid-shard");
            }

            var shardMembers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var suiteId in shard.SuiteIds)
            {
                if (string.IsNullOrWhiteSpace(suiteId) ||
                    !knownSuiteIds.Contains(suiteId) ||
                    !shardMembers.Add(suiteId) ||
                    !union.Add(suiteId))
                {
                    throw Invalid("shard-union=invalid-member");
                }
            }

            shards[index] = new ShardDefinition(
                shard.Id,
                Array.AsReadOnly(shard.SuiteIds.Select(suiteId => suiteId!).ToArray()));
        }

        if (union.Count != knownSuiteIds.Count || !union.SetEquals(knownSuiteIds))
        {
            throw Invalid("shard-union=incomplete");
        }

        return shards;
    }

    private static bool IsExpectedReadFailure(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or SecurityException;

    private static VerificationException Invalid(string detail) => new(VerificationErrorCodes.ConfigInvalid, detail);

    private sealed record CatalogDto
    {
        [JsonPropertyName("$schema")]
        public string? Schema { get; init; }

        [JsonPropertyName("schema_version")]
        public int? SchemaVersion { get; init; }

        [JsonPropertyName("contract")]
        public string? Contract { get; init; }

        [JsonPropertyName("activation_state")]
        public string? ActivationState { get; init; }

        [JsonPropertyName("max_parallelism")]
        public int? MaxParallelism { get; init; }

        [JsonPropertyName("overall_timeout_seconds")]
        public int? OverallTimeoutSeconds { get; init; }

        [JsonPropertyName("allowed_executables")]
        public string?[]? AllowedExecutables { get; init; }

        [JsonPropertyName("suites")]
        public SuiteDto?[]? Suites { get; init; }

        [JsonPropertyName("shards")]
        public ShardDto?[]? Shards { get; init; }
    }

    private sealed record SuiteDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("owner")]
        public string? Owner { get; init; }

        [JsonPropertyName("migration_state")]
        public string? MigrationState { get; init; }

        [JsonPropertyName("timeout_seconds")]
        public int? TimeoutSeconds { get; init; }

        [JsonPropertyName("executor")]
        public ExecutorDto? Executor { get; init; }
    }

    private sealed record ExecutorDto
    {
        [JsonPropertyName("kind")]
        public string? Kind { get; init; }

        [JsonPropertyName("file_name")]
        public string? FileName { get; init; }

        [JsonPropertyName("arguments")]
        public string?[]? Arguments { get; init; }

        [JsonPropertyName("managed_handler")]
        public string? ManagedHandler { get; init; }
    }

    private sealed record ShardDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("suite_ids")]
        public string?[]? SuiteIds { get; init; }
    }
}
