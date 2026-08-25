namespace DesktopNode.Verification;

internal static class VerificationOptions
{
    // Bootstrap CLI grammar is fixed; planning validates catalog suite and shard existence later.
    private static readonly HashSet<string> ApprovedShards =
    [
        "dotnet", "web", "delivery", "installer-policy"
    ];

    internal static VerificationRequest Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || !string.Equals(args[0], "verify", StringComparison.Ordinal))
        {
            throw Invalid("cli:unknown-command");
        }

        VerificationLane requestedLane = default;
        ChangeTier requestedChangeTier = default;
        string? artifactRoot = null;
        string? shardId = null;
        var laneSeen = false;
        var changeTierSeen = false;
        var artifactRootSeen = false;
        var shardSeen = false;
        var planOnly = false;
        var changedPaths = new List<string>();
        var changedPathSet = new HashSet<string>(StringComparer.Ordinal);
        var suiteIds = new List<string>();
        var suiteIdSet = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 1; index < args.Count; index++)
        {
            var option = args[index];
            switch (option)
            {
                case "--lane":
                    RejectDuplicate(laneSeen, option);
                    laneSeen = true;
                    var lane = RequiredValue(args, ref index, option);
                    if (!TryParseNamedEnum(lane, out requestedLane))
                    {
                        throw Invalid($"cli:invalid-lane={lane}");
                    }
                    break;

                case "--change-tier":
                    RejectDuplicate(changeTierSeen, option);
                    changeTierSeen = true;
                    var tier = RequiredValue(args, ref index, option);
                    if (!TryParseNamedEnum(tier, out requestedChangeTier))
                    {
                        throw Invalid($"cli:invalid-change-tier={tier}");
                    }
                    break;

                case "--changed-path":
                    var changedPath = NormalizeChangedPath(RequiredValue(args, ref index, option));
                    if (changedPathSet.Add(changedPath))
                    {
                        changedPaths.Add(changedPath);
                    }
                    break;

                case "--artifact-root":
                    RejectDuplicate(artifactRootSeen, option);
                    artifactRootSeen = true;
                    artifactRoot = RequiredValue(args, ref index, option);
                    break;

                case "--suite":
                    var suiteId = RequiredValue(args, ref index, option);
                    if (!suiteIdSet.Add(suiteId))
                    {
                        throw Invalid($"cli:duplicate-suite={suiteId}");
                    }
                    suiteIds.Add(suiteId);
                    break;

                case "--shard":
                    RejectDuplicate(shardSeen, option);
                    shardSeen = true;
                    shardId = RequiredValue(args, ref index, option);
                    if (!ApprovedShards.Contains(shardId))
                    {
                        throw Invalid($"cli:invalid-shard={shardId}");
                    }
                    break;

                case "--plan-only":
                    RejectDuplicate(planOnly, option);
                    planOnly = true;
                    break;

                default:
                    throw Invalid($"cli:unknown-option={option}");
            }
        }

        Require(laneSeen, "--lane");
        Require(changeTierSeen, "--change-tier");
        Require(changedPaths.Count > 0, "--changed-path");
        Require(artifactRootSeen, "--artifact-root");

        if (suiteIds.Count > 0 && shardId is not null)
        {
            throw Invalid("cli:suite-and-shard-mutually-exclusive");
        }

        return new VerificationRequest(
            requestedLane,
            requestedChangeTier,
            Array.AsReadOnly(changedPaths.ToArray()),
            artifactRoot!,
            Array.AsReadOnly(suiteIds.ToArray()),
            shardId,
            planOnly);
    }

    private static string RequiredValue(IReadOnlyList<string> args, ref int index, string option)
    {
        if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new VerificationException(VerificationErrorCodes.ConfigInvalid, $"cli:missing-value={option}");
        }
        var value = args[++index];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new VerificationException(VerificationErrorCodes.ConfigInvalid, $"cli:empty-value={option}");
        }
        return value;
    }

    private static string NormalizeChangedPath(string value)
    {
        var slashNormalized = value.Replace('\\', '/');
        if (Path.IsPathRooted(value) ||
            slashNormalized.StartsWith("/", StringComparison.Ordinal) ||
            IsDriveQualified(slashNormalized))
        {
            throw Invalid("cli:invalid-changed-path=rooted");
        }

        var segments = new List<string>();
        foreach (var segment in slashNormalized.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                throw Invalid("cli:invalid-changed-path=traversal-or-empty");
            }

            segments.Add(segment);
        }

        var normalized = string.Join('/', segments);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw Invalid("cli:invalid-changed-path=traversal-or-empty");
        }

        return normalized;
    }

    private static bool IsDriveQualified(string value) =>
        value.Length >= 2 && char.IsAsciiLetter(value[0]) && value[1] == ':';

    private static bool TryParseNamedEnum<TEnum>(string value, out TEnum result)
        where TEnum : struct, Enum
    {
        if (Enum.GetNames<TEnum>().Any(name => string.Equals(name, value, StringComparison.OrdinalIgnoreCase)))
        {
            return Enum.TryParse(value, ignoreCase: true, out result);
        }

        result = default;
        return false;
    }

    private static void RejectDuplicate(bool seen, string option)
    {
        if (seen)
        {
            throw Invalid($"cli:duplicate-option={option}");
        }
    }

    private static void Require(bool present, string option)
    {
        if (!present)
        {
            throw Invalid($"cli:missing-required-option={option}");
        }
    }

    private static VerificationException Invalid(string detail) =>
        new(VerificationErrorCodes.ConfigInvalid, detail);
}
