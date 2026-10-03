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
                var json = options.Format == DesktopNodeCliOutputFormat.Json
                    ? DesktopNodeCliErrorJson.FromResponse(response) + Environment.NewLine
                    : string.Empty;
                return new DesktopNodeCliApplicationResult(1, json, verbosePrefix + problem + Environment.NewLine);
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
            return Failure(args, 2, Redact(ex.Message), "PCV_CLI_ARGUMENT_INVALID");
        }
        catch (InvalidOperationException ex)
        {
            return Failure(args, 1, Redact(ex.Message), "PCV_CLI_OPERATION_FAILED");
        }
        catch (HttpRequestException ex)
        {
            return Failure(args, 1, "PCV_CLI_TRANSPORT_ERROR|" + Redact(ex.Message), "PCV_CLI_TRANSPORT_ERROR");
        }
    }

    private static DesktopNodeCliApplicationResult Failure(IReadOnlyList<string> args, int exitCode, string message, string fallbackCode)
    {
        var json = DesktopNodeCliErrorJson.Requested(args)
            ? DesktopNodeCliErrorJson.FromMessage(message, fallbackCode) + Environment.NewLine
            : string.Empty;
        return new DesktopNodeCliApplicationResult(exitCode, json, message + Environment.NewLine);
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
