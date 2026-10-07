using System.Text.Json;
using System.Text.RegularExpressions;

namespace DesktopNode.Verification;

// pcvverify train-path-check --payload <commit> --head <commit|HEAD>
// A single-PR train (design pcv-single-pr-train-v1) cites the main push run of its payload commit, so Lane 3 must show
// that the train branch changed no product path after that commit. Exit 0 when no product path changed, 1 when one did.
internal static class TrainPathCheckCommand
{
    internal const string Name = "train-path-check";
    internal const string ResultContract = "pcv-train-path-check-result-v1";

    internal static readonly IReadOnlyList<string> ProductPathPrefixes = ["src/", "web/src/", "config/", ".github/"];

    // Design pcv-single-pr-train-v2: every Lane 3 promotion updates these SHA-256 pins; none of them is MSI payload.
    internal static readonly IReadOnlyList<string> Lane3PinPaths =
    [
        "config/pcv-development-policy-contract-spec-v1.json",
        "config/pcv-installed-smoke-contract-spec-v1.json",
        "config/pcv-manual-admin-readiness-contract-spec-v1.json",
        "src/DesktopNode.Delivery.Tests/Delivery/Installed/InstalledContractVerifier.cs",
        "src/DesktopNode.Delivery.Tests/Delivery/ManualAdmin/ManualAdminContractVerifier.cs",
        "src/DesktopNode.Delivery.Tests/Delivery/Verification/DevelopmentPolicyContractVerifier.cs"
    ];

    private static readonly Regex CommitPattern = new("^[0-9a-f]{7,40}$", RegexOptions.CultureInvariant);
    private static readonly Regex PinLine = new(@"^[+-]\s*(""sha256"":\s*)?""[0-9a-f]{64}""[,;]?\s*$", RegexOptions.CultureInvariant);

    // The process policy accepts only the full canonical allowlist, the same one CutoverGitBoundary passes.
    private static readonly IReadOnlyList<string> AllowedExecutables = Array.AsReadOnly([
        "dotnet", "dotnet.exe", "node", "node.exe", "npm", "npm.cmd", "git", "git.exe"
    ]);

    internal static int Run(
        IReadOnlyList<string> args,
        string currentDirectory,
        TextWriter standardOutput,
        IProcessRunner? processRunner = null)
    {
        try
        {
            if (args.Count != 5 || args[1] != "--payload" || args[3] != "--head" ||
                !CommitPattern.IsMatch(args[2]) || !(args[4] == "HEAD" || CommitPattern.IsMatch(args[4])))
            {
                throw Invalid("cli-invalid", "usage");
            }

            var repositoryRoot = RepositoryLocator.Find(currentDirectory);
            var runner = processRunner ?? new SystemProcessRunner();
            var range = $"{args[2]}..{args[4]}";
            var changed = Git(runner, repositoryRoot, "train-path-check-git-diff", ["diff", "--name-only", range], range)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            var productPaths = changed
                .Where(path => ProductPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.Ordinal)))
                .ToList();
            var allowedPinPaths = productPaths
                .Where(path => Lane3PinPaths.Contains(path, StringComparer.Ordinal) &&
                    OnlyPinLinesChanged(Git(runner, repositoryRoot, "train-path-check-git-diff-pin", ["diff", "-U0", range, "--", path], path)))
                .ToList();
            productPaths = productPaths.Except(allowedPinPaths, StringComparer.Ordinal).ToList();
            standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = productPaths.Count == 0,
                ["payload"] = args[2],
                ["head"] = args[4],
                ["changed_path_count"] = changed.Count,
                ["product_path_prefixes"] = ProductPathPrefixes,
                ["product_paths"] = productPaths,
                ["allowed_pin_paths"] = allowedPinPaths
            }));
            return productPaths.Count == 0 ? 0 : 1;
        }
        catch (VerificationException exception)
        {
            standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = false,
                ["error_code"] = exception.Code,
                ["error_detail"] = exception.Detail
            }));
            return 2;
        }
    }

    private static string Git(IProcessRunner runner, string repositoryRoot, string id, IReadOnlyList<string> arguments, string subject)
    {
        var result = runner.RunAsync(
            new ProcessInvocation(id, "git", arguments, repositoryRoot, TimeSpan.FromSeconds(30), AllowedExecutables, OutputLimitCharacters: 1_000_000),
            CancellationToken.None).GetAwaiter().GetResult();
        if (result.TimedOut || result.Cancelled || result.ExitCode != 0)
        {
            throw Invalid("git-diff-failed", subject);
        }

        return result.StandardOutput;
    }

    // A pin update replaces SHA-256 values one line for one line; any other changed line keeps the file a product path.
    internal static bool OnlyPinLinesChanged(string unifiedDiff)
    {
        var changed = unifiedDiff
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => (line.StartsWith('+') || line.StartsWith('-')) &&
                !line.StartsWith("+++", StringComparison.Ordinal) && !line.StartsWith("---", StringComparison.Ordinal))
            .ToList();
        return changed.Count > 0 &&
            changed.Count(line => line.StartsWith('+')) == changed.Count(line => line.StartsWith('-')) &&
            changed.All(PinLine.IsMatch);
    }

    private static VerificationException Invalid(string reason, string subject) =>
        new(VerificationErrorCodes.ConfigInvalid, $"train-path-check:{reason}:{subject}");
}
