using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DesktopNode.Verification;

// pcvverify train-facts --input <train-facts-input.json>
// Writes docs/ga-ready/trains/<version>.evidence-facts.json: generated values plus the input's narrative values for each
// requested pair document. Documents of other templates already in that file (Lane 3, hand-written) are kept.
internal static class TrainFactsCommand
{
    internal const string Name = "train-facts";
    internal const string ResultContract = "pcv-train-facts-result-v1";

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    internal static int Run(IReadOnlyList<string> args, string currentDirectory, TextWriter standardOutput)
    {
        try
        {
            if (args.Count != 3 || args[1] != "--input")
            {
                throw TrainFactsInput.Invalid("cli-invalid", "usage");
            }

            var repositoryRoot = RepositoryLocator.Find(currentDirectory);
            var inputPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repositoryRoot, args[2]);
            if (!File.Exists(inputPath))
            {
                throw TrainFactsInput.Invalid("input-missing", args[2]);
            }

            var input = TrainFactsInput.Parse(File.ReadAllText(inputPath, Encoding.UTF8));
            var builder = new TrainFactsBuilder(repositoryRoot, input);
            var built = new List<(TrainEvidenceDocument Document, int Generated, int Narrative)>();
            foreach (var template in TrainFactsInput.BuildableTemplates.Where(input.Documents.ContainsKey))
            {
                var values = builder.Build(template);
                var generated = values.Count;
                var narrative = input.Narrative.TryGetValue(template, out var extra) ? extra : new Dictionary<string, string>();
                foreach (var (key, value) in narrative)
                {
                    if (!values.TryAdd(key, value))
                    {
                        throw TrainFactsInput.Invalid("narrative-conflict", template + ":" + key);
                    }
                }

                var document = new TrainEvidenceDocument(template, input.Documents[template], values);
                var templatePath = Path.Combine(repositoryRoot, "docs", "ga-ready", "trains", "templates", template + ".md.tmpl");
                TrainEvidenceTemplate.Render(File.ReadAllText(templatePath, Encoding.UTF8), values, document.Path);
                built.Add((document, generated, narrative.Count));
            }

            var factsRelative = $"docs/ga-ready/trains/{input.Version}.evidence-facts.json";
            var factsPath = Path.Combine(repositoryRoot, factsRelative.Replace('/', Path.DirectorySeparatorChar));
            var documents = built.Select(item => item.Document).ToList();
            if (File.Exists(factsPath))
            {
                var existing = TrainEvidenceFactsReader.Parse(File.ReadAllText(factsPath, Encoding.UTF8));
                if (existing.Version != input.Version)
                {
                    throw TrainFactsInput.Invalid("facts-version-mismatch", factsRelative);
                }

                documents.AddRange(existing.Documents.Where(document =>
                    !input.Documents.ContainsKey(document.Template) && documents.All(item => item.Path != document.Path)));
            }

            var ordered = documents
                .OrderBy(document => IndexOf(document.Template))
                .ToList();
            var text = Serialize(input.Version, ordered);
            TrainEvidenceFactsReader.Parse(text);
            File.WriteAllText(factsPath, text, Utf8NoBom);

            var payload = new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = true,
                ["version"] = input.Version,
                ["facts"] = factsRelative,
                ["documents"] = built.Select(item => new Dictionary<string, object>
                {
                    ["template"] = item.Document.Template,
                    ["path"] = item.Document.Path,
                    ["generated"] = item.Generated,
                    ["narrative"] = item.Narrative
                }).ToList()
            };
            standardOutput.WriteLine(JsonSerializer.Serialize(payload));
            return 0;
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
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            standardOutput.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["schema_version"] = 1,
                ["contract"] = ResultContract,
                ["ok"] = false,
                ["error_code"] = VerificationErrorCodes.ConfigInvalid,
                ["error_detail"] = "train-facts:io-failure"
            }));
            return 2;
        }
    }

    private static int IndexOf(string template)
    {
        var index = TrainEvidenceFactsReader.Templates.ToList().IndexOf(template);
        return index < 0 ? int.MaxValue : index;
    }

    private static string Serialize(string version, IReadOnlyList<TrainEvidenceDocument> documents)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
               {
                   Indented = true,
                   Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
               }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schema_version", 1);
            writer.WriteString("contract", TrainEvidenceFactsReader.Contract);
            writer.WriteString("version", version);
            writer.WriteStartArray("documents");
            foreach (var document in documents)
            {
                writer.WriteStartObject();
                writer.WriteString("template", document.Template);
                writer.WriteString("path", document.Path);
                writer.WriteStartObject("values");
                foreach (var (key, value) in document.Values)
                {
                    writer.WriteString(key, value);
                }

                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
    }
}
