using System.Text;
using System.Text.Json;

namespace DesktopNode.Verification;

// pcvverify lane3-spec --input <lane3-spec-input.json> (--write | --check)
// Builds packaging/windows-desktop-node/tests/fixtures/lane3-promotion-docs-spec-<tag>.json from the previous spec and
// the train facts (design pcv-train-lane3-spec-generator-v1). --check fails unless the committed spec is byte-identical.
internal static class TrainLane3SpecCommand
{
    internal const string Name = "lane3-spec";
    internal const string ResultContract = "pcv-train-lane3-spec-result-v1";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    internal static int Run(IReadOnlyList<string> args, string currentDirectory, TextWriter standardOutput)
    {
        try
        {
            if (args.Count != 4 || args[1] != "--input" || args[3] is not ("--write" or "--check"))
            {
                throw TrainLane3SpecInput.Invalid("cli-invalid", "usage");
            }

            var repositoryRoot = RepositoryLocator.Find(currentDirectory);
            var inputPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repositoryRoot, args[2]);
            if (!File.Exists(inputPath))
            {
                throw TrainLane3SpecInput.Invalid("input-missing", args[2]);
            }

            var input = TrainLane3SpecInput.Parse(File.ReadAllText(inputPath, Encoding.UTF8));
            var text = TrainLane3SpecBuilder.Serialize(new TrainLane3SpecBuilder(repositoryRoot, input).Build());
            var outputPath = Path.Combine(repositoryRoot, input.Output.Replace('/', Path.DirectorySeparatorChar));
            var existing = File.Exists(outputPath) ? File.ReadAllText(outputPath, Encoding.UTF8) : null;
            var status = existing is null ? "missing" : existing == text ? "current" : "stale";
            var write = args[3] == "--write";
            if (write && status != "current")
            {
                File.WriteAllText(outputPath, text, Utf8NoBom);
                status = status == "missing" ? "written" : "updated";
            }

            var ok = write || status == "current";
            standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = ok,
                ["version"] = input.Version,
                ["output"] = input.Output,
                ["status"] = status
            }));
            return ok ? 0 : 1;
        }
        catch (VerificationException exception)
        {
            return Fail(standardOutput, exception.Code, exception.Detail);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return Fail(standardOutput, VerificationErrorCodes.ConfigInvalid, "lane3-spec:io-failure");
        }
    }

    private static int Fail(TextWriter standardOutput, string code, string detail)
    {
        standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["schema_version"] = 1,
            ["contract"] = ResultContract,
            ["ok"] = false,
            ["error_code"] = code,
            ["error_detail"] = detail
        }));
        return 2;
    }
}
