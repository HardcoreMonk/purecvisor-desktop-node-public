using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DesktopNode.Verification;

// pcvverify train-host-inputs --input <host-inputs.json> --kind current-card (--plan | --write)
// Renders a release train host input from its tracked template and the train facts into the ignored artifacts/ tree
// (design pcv-train-host-inputs-v1). --write never replaces an existing file; --plan reports the output and its SHA-256.
internal static class TrainHostInputsCommand
{
    internal const string Name = "train-host-inputs";
    internal const string ResultContract = "pcv-train-host-inputs-result-v1";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    internal static int Run(IReadOnlyList<string> args, string currentDirectory, TextWriter standardOutput)
    {
        try
        {
            if (args.Count != 6 || args[1] != "--input" || args[3] != "--kind" || args[5] is not ("--plan" or "--write"))
            {
                throw TrainHostInputs.Invalid("cli-invalid", "usage");
            }

            var repositoryRoot = RepositoryLocator.Find(currentDirectory);
            var inputPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repositoryRoot, args[2]);
            if (!File.Exists(inputPath))
            {
                throw TrainHostInputs.Invalid("input-missing", args[2]);
            }

            var input = TrainHostInputs.Parse(File.ReadAllText(inputPath, Encoding.UTF8));
            var (output, text) = args[4] switch
            {
                "current-card" => (input.CurrentCardOutput, input.RenderCurrentCard(repositoryRoot, repositoryRoot)),
                _ => throw TrainHostInputs.Invalid("kind-unknown", args[4])
            };

            var outputPath = Path.Combine(repositoryRoot, output.Replace('/', Path.DirectorySeparatorChar));
            var exists = File.Exists(outputPath);
            var write = args[5] == "--write";
            if (write && exists)
            {
                throw TrainHostInputs.Invalid("output-exists", output);
            }

            if (write)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                File.WriteAllText(outputPath, text, Utf8NoBom);
            }

            standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = true,
                ["version"] = input.Version,
                ["kind"] = args[4],
                ["output"] = output,
                ["status"] = write ? "written" : exists ? "exists" : "planned",
                ["sha256"] = Sha256(text)
            }));
            return 0;
        }
        catch (VerificationException exception)
        {
            return Fail(standardOutput, exception.Code, exception.Detail);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return Fail(standardOutput, VerificationErrorCodes.ConfigInvalid, "train-host-inputs:io-failure");
        }
    }

    internal static string Sha256(string text) =>
        Convert.ToHexString(SHA256.HashData(Utf8NoBom.GetBytes(text))).ToLowerInvariant();

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
