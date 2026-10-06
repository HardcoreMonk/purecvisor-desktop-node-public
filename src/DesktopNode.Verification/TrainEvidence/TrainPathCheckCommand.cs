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

    private static readonly Regex CommitPattern = new("^[0-9a-f]{7,40}$", RegexOptions.CultureInvariant);

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
            var result = (processRunner ?? new SystemProcessRunner()).RunAsync(
                new ProcessInvocation(
                    "train-path-check-git-diff",
                    "git",
                    ["diff", "--name-only", $"{args[2]}..{args[4]}"],
                    repositoryRoot,
                    TimeSpan.FromSeconds(30),
                    ["git", "git.exe"],
                    OutputLimitCharacters: 1_000_000),
                CancellationToken.None).GetAwaiter().GetResult();
            if (result.TimedOut || result.Cancelled || result.ExitCode != 0)
            {
                throw Invalid("git-diff-failed", $"{args[2]}..{args[4]}");
            }

            var changed = result.StandardOutput
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            var productPaths = changed
                .Where(path => ProductPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.Ordinal)))
                .ToList();
            standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = productPaths.Count == 0,
                ["payload"] = args[2],
                ["head"] = args[4],
                ["changed_path_count"] = changed.Count,
                ["product_path_prefixes"] = ProductPathPrefixes,
                ["product_paths"] = productPaths
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

    private static VerificationException Invalid(string reason, string subject) =>
        new(VerificationErrorCodes.ConfigInvalid, $"train-path-check:{reason}:{subject}");
}
