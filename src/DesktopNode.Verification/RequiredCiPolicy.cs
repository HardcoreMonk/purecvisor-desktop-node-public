using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace DesktopNode.Verification;

internal enum RequiredCiMode
{
    Shadow,
    Active
}

internal sealed record RequiredCiPolicyResult(
    RequiredCiMode Mode,
    IReadOnlyList<string> JobIds,
    IReadOnlyList<string> Shards,
    int PesterInvocationCount,
    int NonAdminPowerShellInvocationCount,
    int HostMutationInvocationCount);

internal static partial class RequiredCiPolicy
{
    private const string Checkout = "actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd";
    private const string SetupDotNet = "actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d";
    private const string Cache = "actions/cache@a7833574556fa59680c1b7cb190c1735db73ebf0";
    private const string SetupNode = "actions/setup-node@2028fbc5c25fe9cf00d9f06a71cc4710d4507903";
    private const string UploadArtifact = "actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f";

    private static readonly string[] ActiveJobIds = ["dotnet", "web", "delivery", "installer-policy"];
    private static readonly string[] ShadowJobIds =
        ["dotnet-tests", "web-tests", "packaging-pester", "installer-web-pester"];
    private static readonly string[] ExpectedShards = ["dotnet", "web", "delivery", "installer-policy"];
    private static readonly HashSet<string> ReviewedActions = new(
        [Checkout, SetupDotNet, Cache, SetupNode, UploadArtifact],
        StringComparer.Ordinal);
    private static readonly string[] HostMutationTokens =
    [
        "msiexec", "sc.exe", "new-vm", "start-vm", "stop-vm", "remove-vm",
        "start-service", "stop-service", "restart-service", "install-module hyper-v",
        "allowhostmutation", "invoke-command -vmname"
    ];

    internal static RequiredCiPolicyResult Validate(
        string workflowYaml,
        VerificationCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(workflowYaml);
        ArgumentNullException.ThrowIfNull(catalog);

        var mode = Mode(catalog);
        ValidateCatalog(catalog, mode);
        RejectAnchorsAndAliases(workflowYaml);
        var root = ParseRoot(workflowYaml);
        var jobs = RequiredMapping(root, "jobs", "required-ci-yaml=jobs");
        var expectedJobIds = mode == RequiredCiMode.Shadow ? ShadowJobIds : ActiveJobIds;
        var jobIds = ScalarKeys(jobs);
        if (!jobIds.SequenceEqual(expectedJobIds, StringComparer.Ordinal))
        {
            throw Invalid("required-ci-jobs=mismatch");
        }

        var shards = new List<string>(expectedJobIds.Length);
        var pesterCount = 0;
        var powerShellCount = 0;
        var mutationCount = 0;
        for (var index = 0; index < expectedJobIds.Length; index++)
        {
            var jobId = expectedJobIds[index];
            var shard = ExpectedShards[index];
            var job = RequiredMapping(jobs, jobId, $"required-ci-job:{jobId}=mapping");
            ValidateJobHeader(jobId, job, mode);
            var steps = RequiredSequence(job, "steps", $"required-ci-job:{jobId}=steps");
            var stepMaps = steps.Children
                .Select((node, stepIndex) => AsMapping(node, $"required-ci-job:{jobId}=step-{stepIndex}"))
                .ToArray();

            var jobShard = ExtractShard(jobId, stepMaps);
            ValidateActionsAndArtifacts(jobId, shard, stepMaps, mode);
            ValidateStepNames(jobId, shard, jobShard, stepMaps, mode);
            shards.Add(jobShard);
            foreach (var step in stepMaps)
            {
                var executable = ExecutableText(step);
                var hasPester = PesterRegex().IsMatch(executable);
                var hasPowerShell = PowerShellRegex().IsMatch(executable);
                var hasMutation = HostMutationTokens.Any(token =>
                    executable.Contains(token, StringComparison.OrdinalIgnoreCase));
                var stepName = OptionalScalar(step, "name") ?? string.Empty;

                if (mode == RequiredCiMode.Active &&
                    stepName.Contains("legacy", StringComparison.OrdinalIgnoreCase))
                {
                    throw Invalid("required-ci-active=legacy-step");
                }

                if (hasPester)
                {
                    pesterCount++;
                    if (mode == RequiredCiMode.Active)
                    {
                        throw Invalid("required-ci-active=pester");
                    }
                }

                if (hasPowerShell)
                {
                    powerShellCount++;
                    if (mode == RequiredCiMode.Active)
                    {
                        throw Invalid("required-ci-active=nonadmin-powershell");
                    }
                }

                if (hasMutation)
                {
                    mutationCount++;
                    throw Invalid("required-ci=host-mutation");
                }

                if (mode == RequiredCiMode.Shadow && (hasPester || hasPowerShell) &&
                    !IsAllowedShadowLegacyStep(jobId, stepName))
                {
                    throw Invalid($"required-ci-shadow:{jobId}=legacy-boundary");
                }
            }
        }

        if (!shards.SequenceEqual(ExpectedShards, StringComparer.Ordinal) ||
            !shards.SequenceEqual(catalog.Shards.Select(shard => shard.Id), StringComparer.Ordinal))
        {
            throw Invalid("required-ci-shards=mismatch");
        }

        return new RequiredCiPolicyResult(
            mode,
            Array.AsReadOnly(jobIds),
            Array.AsReadOnly(shards.ToArray()),
            pesterCount,
            powerShellCount,
            mutationCount);
    }

    private static RequiredCiMode Mode(VerificationCatalog catalog) => catalog.ActivationState switch
    {
        "shadow-ready" => RequiredCiMode.Shadow,
        "active" => RequiredCiMode.Active,
        _ => throw Invalid("required-ci-catalog=activation")
    };

    private static void ValidateCatalog(VerificationCatalog catalog, RequiredCiMode mode)
    {
        if (!catalog.Shards.Select(shard => shard.Id).SequenceEqual(ExpectedShards, StringComparer.Ordinal))
        {
            throw Invalid("required-ci-catalog=shards");
        }

        var allowedStates = mode == RequiredCiMode.Shadow
            ? new HashSet<string>(["native-existing", "mapped"], StringComparer.Ordinal)
            : new HashSet<string>(["cutover"], StringComparer.Ordinal);
        if (catalog.Suites.Any(suite => !allowedStates.Contains(suite.MigrationState)))
        {
            throw Invalid("required-ci-catalog=migration-state");
        }

        foreach (var suite in catalog.Suites.Where(suite => suite.ExecutorKind == "process"))
        {
            var executable = string.Join(' ', new[] { suite.FileName ?? string.Empty }.Concat(suite.Arguments));
            if (PesterRegex().IsMatch(executable) || PowerShellRegex().IsMatch(executable) ||
                HostMutationTokens.Any(token => executable.Contains(token, StringComparison.OrdinalIgnoreCase)))
            {
                throw Invalid("required-ci-catalog=executable");
            }
        }
    }

    private static void RejectAnchorsAndAliases(string yaml)
    {
        try
        {
            var parser = new Parser(new StringReader(yaml));
            while (parser.MoveNext())
            {
                if (parser.Current is AnchorAlias ||
                    parser.Current is NodeEvent nodeEvent && !nodeEvent.Anchor.IsEmpty)
                {
                    throw Invalid("required-ci-yaml=anchor-alias");
                }
            }
        }
        catch (VerificationException)
        {
            throw;
        }
        catch (YamlException)
        {
            throw Invalid("required-ci-yaml=malformed");
        }
    }

    private static YamlMappingNode ParseRoot(string yaml)
    {
        var stream = new YamlStream();
        try
        {
            stream.Load(new StringReader(yaml));
        }
        catch (ArgumentException)
        {
            throw Invalid("required-ci-yaml=duplicate-key");
        }
        catch (YamlException exception) when (
            exception.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid("required-ci-yaml=duplicate-key");
        }
        catch (YamlException)
        {
            throw Invalid("required-ci-yaml=malformed");
        }

        if (stream.Documents.Count != 1 || stream.Documents[0].RootNode is not YamlMappingNode root)
        {
            throw Invalid("required-ci-yaml=root");
        }

        RejectDuplicateSemanticKeys(root);
        return root;
    }

    private static void RejectDuplicateSemanticKeys(YamlNode node)
    {
        if (node is YamlMappingNode mapping)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var (key, value) in mapping.Children)
            {
                if (key is not YamlScalarNode scalar || scalar.Value is null || !keys.Add(scalar.Value))
                {
                    throw Invalid("required-ci-yaml=duplicate-key");
                }

                RejectDuplicateSemanticKeys(value);
            }
        }
        else if (node is YamlSequenceNode sequence)
        {
            foreach (var child in sequence.Children)
            {
                RejectDuplicateSemanticKeys(child);
            }
        }
    }

    private static void ValidateJobHeader(
        string jobId,
        YamlMappingNode job,
        RequiredCiMode mode)
    {
        if (TryGet(job, "if", out _))
        {
            throw Invalid($"required-ci-job:{jobId}=conditional");
        }

        var expectedRunner = jobId is "web" or "web-tests" ? "ubuntu-latest" : "windows-latest";
        if (!string.Equals(OptionalScalar(job, "runs-on"), expectedRunner, StringComparison.Ordinal))
        {
            throw Invalid($"required-ci-job:{jobId}=runner");
        }

        if (mode == RequiredCiMode.Active &&
            !string.Equals(OptionalScalar(job, "name"), jobId, StringComparison.Ordinal))
        {
            throw Invalid($"required-ci-job:{jobId}=name");
        }
    }

    private static void ValidateActionsAndArtifacts(
        string jobId,
        string shard,
        IReadOnlyList<YamlMappingNode> steps,
        RequiredCiMode mode)
    {
        var actionSteps = steps
            .Select(step => (Step: step, Uses: OptionalScalar(step, "uses")))
            .Where(item => item.Uses is not null)
            .ToArray();
        if (actionSteps.Any(item => !ReviewedActions.Contains(item.Uses!)))
        {
            throw Invalid("required-ci-action=unreviewed");
        }

        var uses = actionSteps.Select(item => item.Uses!).ToArray();
        var needsNode = jobId is "web" or "web-tests";
        if (uses.Count(action => action == Checkout) != 1 ||
            uses.Count(action => action == SetupDotNet) != 1 ||
            uses.Count(action => action == SetupNode) != (needsNode ? 1 : 0))
        {
            throw Invalid($"required-ci-job:{jobId}=setup");
        }

        var uploads = actionSteps.Where(item => item.Uses == UploadArtifact).Select(item => item.Step).ToArray();
        var expectedArtifacts = mode == RequiredCiMode.Active
            ? new[]
            {
                ($"development-gates-{jobId}-${{{{ github.run_id }}}}", $"artifacts/development-gates-{jobId}")
            }
            : ShadowArtifacts(jobId, shard);
        if (uploads.Length != expectedArtifacts.Length)
        {
            throw Invalid($"required-ci-job:{jobId}=artifact");
        }

        var actualArtifacts = uploads.Select(upload =>
        {
            var with = RequiredMapping(upload, "with", $"required-ci-job:{jobId}=artifact");
            return (OptionalScalar(with, "name") ?? string.Empty, OptionalScalar(with, "path") ?? string.Empty);
        }).ToArray();
        if (!actualArtifacts.SequenceEqual(expectedArtifacts))
        {
            throw Invalid($"required-ci-job:{jobId}=artifact");
        }

        foreach (var step in steps)
        {
            if (!TryGet(step, "if", out var condition))
            {
                continue;
            }

            var usesAction = OptionalScalar(step, "uses");
            if (usesAction != UploadArtifact ||
                condition is not YamlScalarNode scalar ||
                !string.Equals(scalar.Value, "always()", StringComparison.Ordinal))
            {
                throw Invalid($"required-ci-job:{jobId}=conditional-step");
            }
        }
    }

    private static (string Name, string Path)[] ShadowArtifacts(string jobId, string shard)
    {
        var names = jobId switch
        {
            "dotnet-tests" => ("legacy-dotnet", "replacement-dotnet"),
            "web-tests" => ("legacy-web", "replacement-web"),
            "packaging-pester" => ("legacy-packaging", "replacement-delivery"),
            "installer-web-pester" => ("legacy-installer-web", "replacement-installer-policy"),
            _ => throw Invalid("required-ci-jobs=mismatch")
        };
        return
        [
            (names.Item1, $"artifacts/shadow/{shard}/legacy"),
            (names.Item2, $"artifacts/shadow/{shard}/replacement")
        ];
    }

    private static void ValidateStepNames(
        string jobId,
        string expectedShard,
        string actualShard,
        IReadOnlyList<YamlMappingNode> steps,
        RequiredCiMode mode)
    {
        var names = steps.Select(step => OptionalScalar(step, "name")).Where(name => name is not null).ToArray();
        var replacement = mode == RequiredCiMode.Shadow
            ? $"Run replacement {expectedShard}"
            : jobId == "installer-policy"
                ? "Run installer and policy shard"
                : $"Run {jobId} shard";
        var replacementSteps = steps.Where(step =>
        {
            var run = OptionalScalar(step, "run");
            return run is not null && ShardRegex().Matches(run).Any(match =>
                string.Equals(match.Groups[1].Value, actualShard, StringComparison.Ordinal));
        }).ToArray();
        if (replacementSteps.Length != 1 ||
            !string.Equals(OptionalScalar(replacementSteps[0], "name"), replacement, StringComparison.Ordinal))
        {
            throw Invalid($"required-ci-job:{jobId}=replacement-step");
        }

        if (mode == RequiredCiMode.Shadow)
        {
            var legacy = jobId switch
            {
                "dotnet-tests" => "Run legacy dotnet",
                "web-tests" => "Run legacy web",
                "packaging-pester" => "Run legacy packaging Pester",
                "installer-web-pester" => "Run legacy installer and Web Pester",
                _ => throw Invalid("required-ci-jobs=mismatch")
            };
            if (names.Count(name => string.Equals(name, legacy, StringComparison.Ordinal)) != 1)
            {
                throw Invalid($"required-ci-job:{jobId}=legacy-step");
            }
        }
    }

    private static string ExtractShard(string jobId, IReadOnlyList<YamlMappingNode> steps)
    {
        var shards = steps
            .Select(step => OptionalScalar(step, "run"))
            .Where(run => run is not null)
            .SelectMany(run => ShardRegex().Matches(run!).Select(match => match.Groups[1].Value))
            .ToArray();
        if (shards.Length != 1)
        {
            throw Invalid("required-ci-shards=mismatch");
        }

        return shards[0];
    }

    private static bool IsAllowedShadowLegacyStep(string jobId, string stepName) =>
        (jobId == "packaging-pester" && stepName == "Run legacy packaging Pester") ||
        (jobId == "installer-web-pester" && stepName == "Run legacy installer and Web Pester");

    private static string ExecutableText(YamlMappingNode step) => string.Join(
        '\n',
        new[] { OptionalScalar(step, "shell"), OptionalScalar(step, "run"), OptionalScalar(step, "uses") }
            .Where(value => value is not null));

    private static string[] ScalarKeys(YamlMappingNode mapping) => mapping.Children.Keys
        .Select(key => key is YamlScalarNode scalar && scalar.Value is not null
            ? scalar.Value
            : throw Invalid("required-ci-yaml=non-scalar-key"))
        .ToArray();

    private static YamlMappingNode RequiredMapping(
        YamlMappingNode mapping,
        string key,
        string detail) =>
        TryGet(mapping, key, out var node) && node is YamlMappingNode child
            ? child
            : throw Invalid(detail);

    private static YamlSequenceNode RequiredSequence(
        YamlMappingNode mapping,
        string key,
        string detail) =>
        TryGet(mapping, key, out var node) && node is YamlSequenceNode child
            ? child
            : throw Invalid(detail);

    private static YamlMappingNode AsMapping(YamlNode node, string detail) =>
        node as YamlMappingNode ?? throw Invalid(detail);

    private static string? OptionalScalar(YamlMappingNode mapping, string key) =>
        TryGet(mapping, key, out var node) && node is YamlScalarNode scalar
            ? scalar.Value
            : null;

    private static bool TryGet(YamlMappingNode mapping, string key, out YamlNode value)
    {
        foreach (var pair in mapping.Children)
        {
            if (pair.Key is YamlScalarNode scalar && string.Equals(scalar.Value, key, StringComparison.Ordinal))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null!;
        return false;
    }

    private static VerificationException Invalid(string detail) =>
        new(VerificationErrorCodes.ConfigInvalid, detail);

    [GeneratedRegex(@"(?:^|\s)--shard\s+([a-z0-9-]+)(?:\s|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ShardRegex();

    [GeneratedRegex(@"\b(?:Invoke-Pester|Pester\b|Install-Module\s+Pester\b)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PesterRegex();

    [GeneratedRegex(@"\b(?:pwsh|powershell)(?:\.exe)?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PowerShellRegex();
}
