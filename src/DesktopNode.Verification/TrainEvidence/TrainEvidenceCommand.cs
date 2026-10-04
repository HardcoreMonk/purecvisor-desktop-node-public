using System.Text;
using System.Text.Json;

namespace DesktopNode.Verification;

// pcvverify train-evidence --facts <path> (--check | --write [--allow-update <evidence path>]...)
internal static class TrainEvidenceCommand
{
    internal const string Name = "train-evidence";
    internal const string ResultContract = "pcv-train-evidence-result-v1";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    internal static int Run(IReadOnlyList<string> args, string currentDirectory, TextWriter standardOutput)
    {
        try
        {
            var request = Parse(args);
            var repositoryRoot = RepositoryLocator.Find(currentDirectory);
            var factsPath = Path.IsPathRooted(request.FactsPath)
                ? request.FactsPath
                : Path.Combine(repositoryRoot, request.FactsPath);
            if (!File.Exists(factsPath))
            {
                throw TrainEvidenceFactsReader.Invalid("facts-missing", request.FactsPath);
            }

            var facts = TrainEvidenceFactsReader.Parse(File.ReadAllText(factsPath, Encoding.UTF8));
            var unknownUpdate = request.AllowUpdate.FirstOrDefault(path =>
                !facts.Documents.Any(document => string.Equals(document.Path, path, StringComparison.Ordinal)));
            if (unknownUpdate is not null)
            {
                throw TrainEvidenceFactsReader.Invalid("allow-update-unknown", unknownUpdate);
            }

            var results = facts.Documents
                .Select(document => Evaluate(repositoryRoot, document))
                .ToList();

            if (!request.Write)
            {
                var ok = results.All(result => result.Status == "current");
                WriteResult(standardOutput, ok, "check", request.FactsPath, facts.Version, results);
                return ok ? 0 : 1;
            }

            var blocked = results.FirstOrDefault(result =>
                result.Status == "stale" && !request.AllowUpdate.Contains(result.Document.Path, StringComparer.Ordinal));
            if (blocked is not null)
            {
                throw TrainEvidenceFactsReader.Invalid("existing-differs", blocked.Document.Path);
            }

            var written = new List<Evaluation>();
            foreach (var result in results)
            {
                if (result.Status == "current")
                {
                    written.Add(result);
                    continue;
                }

                File.WriteAllText(result.FullPath, result.Rendered, Utf8NoBom);
                written.Add(result with { Status = result.Status == "missing" ? "written" : "updated" });
            }

            WriteResult(standardOutput, true, "write", request.FactsPath, facts.Version, written);
            return 0;
        }
        catch (VerificationException exception)
        {
            WriteError(standardOutput, exception);
            return 2;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            WriteError(standardOutput, new VerificationException(VerificationErrorCodes.ConfigInvalid, "train-evidence:io-failure"));
            return 2;
        }
    }

    private sealed record Request(string FactsPath, bool Write, IReadOnlyList<string> AllowUpdate);

    private sealed record Evaluation(TrainEvidenceDocument Document, string FullPath, string Rendered, string Status);

    private static Request Parse(IReadOnlyList<string> args)
    {
        string? facts = null;
        bool? write = null;
        var allowUpdate = new List<string>();
        for (var index = 1; index < args.Count; index++)
        {
            switch (args[index])
            {
                case "--facts" when facts is null && index + 1 < args.Count:
                    facts = args[++index];
                    break;
                case "--check" when write is null:
                    write = false;
                    break;
                case "--write" when write is null:
                    write = true;
                    break;
                case "--allow-update" when index + 1 < args.Count:
                    allowUpdate.Add(args[++index]);
                    break;
                default:
                    throw TrainEvidenceFactsReader.Invalid("cli-invalid", args[index]);
            }
        }

        if (facts is null || write is null || (write == false && allowUpdate.Count > 0))
        {
            throw TrainEvidenceFactsReader.Invalid("cli-invalid", "usage");
        }

        return new Request(facts, write.Value, allowUpdate);
    }

    private static Evaluation Evaluate(string repositoryRoot, TrainEvidenceDocument document)
    {
        var templatePath = Path.Combine(
            repositoryRoot, "docs", "ga-ready", "trains", "templates", document.Template + ".md.tmpl");
        if (!File.Exists(templatePath))
        {
            throw TrainEvidenceFactsReader.Invalid("template-missing", document.Template);
        }

        var rendered = TrainEvidenceTemplate.Render(
            File.ReadAllText(templatePath, Encoding.UTF8), document.Values, document.Path);
        var fullPath = Path.Combine(repositoryRoot, document.Path.Replace('/', Path.DirectorySeparatorChar));
        var status = !File.Exists(fullPath)
            ? "missing"
            : string.Equals(File.ReadAllText(fullPath, Encoding.UTF8), rendered, StringComparison.Ordinal)
                ? "current"
                : "stale";
        return new Evaluation(document, fullPath, rendered, status);
    }

    private static void WriteResult(
        TextWriter output,
        bool ok,
        string mode,
        string factsPath,
        string version,
        IEnumerable<Evaluation> results)
    {
        var payload = new Dictionary<string, object?>
        {
            ["schema_version"] = 1,
            ["contract"] = ResultContract,
            ["ok"] = ok,
            ["mode"] = mode,
            ["facts"] = factsPath.Replace('\\', '/'),
            ["version"] = version,
            ["documents"] = results.Select(result => new Dictionary<string, string>
            {
                ["path"] = result.Document.Path,
                ["template"] = result.Document.Template,
                ["status"] = result.Status
            }).ToList()
        };
        output.WriteLine(JsonSerializer.Serialize(payload));
    }

    private static void WriteError(TextWriter output, VerificationException exception)
    {
        var payload = new Dictionary<string, object?>
        {
            ["schema_version"] = 1,
            ["contract"] = ResultContract,
            ["ok"] = false,
            ["error_code"] = exception.Code,
            ["error_detail"] = exception.Detail
        };
        output.WriteLine(JsonSerializer.Serialize(payload));
    }
}
