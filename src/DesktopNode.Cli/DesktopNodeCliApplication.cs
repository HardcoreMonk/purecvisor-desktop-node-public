using System.Text.Json;

namespace DesktopNode.Cli;

public static class DesktopNodeCliApplication
{
    public static async Task<DesktopNodeCliApplicationResult> RunAsync(
        IReadOnlyList<string> args,
        IDesktopNodeCliTransport transport,
        Func<string, string?>? environment = null,
        string? defaultProtectedTokenFilePath = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = DesktopNodeCliOptions.Parse(args);
            if (options.ShowHelp)
            {
                return new DesktopNodeCliApplicationResult(0, DesktopNodeCliCommandCatalog.GetUsage() + Environment.NewLine, string.Empty);
            }

            var request = DesktopNodeCliCommandCatalog.CreateRequest(options.CommandArguments);
            var token = DesktopNodeCliTokenResolver.Resolve(
                options,
                environment,
                defaultProtectedTokenFilePath);
            var verbosePrefix = options.Verbose
                ? $"request {request.Method} {request.Path} token={(string.IsNullOrWhiteSpace(token) ? "none" : "[redacted]")}{Environment.NewLine}"
                : string.Empty;

            var response = await transport.SendAsync(request, options, token, cancellationToken).ConfigureAwait(false);
            if (request.OutputPath is not null && IsSuccess(response))
            {
                var outputDirectory = Path.GetDirectoryName(request.OutputPath);
                if (!string.IsNullOrWhiteSpace(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                await File.WriteAllTextAsync(request.OutputPath, response.Body, cancellationToken).ConfigureAwait(false);
                return new DesktopNodeCliApplicationResult(
                    0,
                    $"diagnostics.bundle.download: ok {request.OutputPath}{Environment.NewLine}",
                    verbosePrefix);
            }

            if (!IsSuccess(response))
            {
                var problem = DesktopNodeCliFormatter.FormatProblem(response);
                return new DesktopNodeCliApplicationResult(1, string.Empty, verbosePrefix + problem + Environment.NewLine);
            }

            var noColor = options.NoColor ||
                string.Equals(environment?.Invoke("NO_COLOR"), "1", StringComparison.OrdinalIgnoreCase);

            return new DesktopNodeCliApplicationResult(
                0,
                DesktopNodeCliFormatter.FormatSuccess(response, options.Format, noColor) + Environment.NewLine,
                verbosePrefix);
        }
        catch (ArgumentException ex)
        {
            return new DesktopNodeCliApplicationResult(2, string.Empty, Redact(ex.Message) + Environment.NewLine);
        }
        catch (InvalidOperationException ex)
        {
            return new DesktopNodeCliApplicationResult(1, string.Empty, Redact(ex.Message) + Environment.NewLine);
        }
        catch (HttpRequestException ex)
        {
            return new DesktopNodeCliApplicationResult(1, string.Empty, "PCV_CLI_TRANSPORT_ERROR|" + Redact(ex.Message) + Environment.NewLine);
        }
    }

    private static bool IsSuccess(DesktopNodeCliTransportResponse response)
    {
        if (response.StatusCode is < 200 or >= 300)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(response.Body))
        {
            return true;
        }

        try
        {
            using var document = JsonDocument.Parse(response.Body);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("ok", out var ok) &&
                ok.ValueKind == JsonValueKind.False)
            {
                return false;
            }
        }
        catch (JsonException)
        {
            return true;
        }

        return true;
    }

    private static string Redact(string value)
    {
        return value.Replace("Authorization", "[redacted-header]", StringComparison.OrdinalIgnoreCase);
    }
}
