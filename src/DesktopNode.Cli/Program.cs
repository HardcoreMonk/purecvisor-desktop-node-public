using System.Text;

namespace DesktopNode.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        ConfigureConsoleEncoding();

        using var transport = new HttpDesktopNodeCliTransport();
        if (args.Length == 0 || args.Any(IsInteractiveFlag))
        {
            return await DesktopNodeCliInteractiveShell.RunAsync(
                args,
                transport,
                DesktopNodeCliInteractiveShell.ReadLineWithCompletion,
                Console.Out.Write,
                Console.Error.Write).ConfigureAwait(false);
        }

        var result = await DesktopNodeCliApplication.RunAsync(args, transport).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(result.StandardOutput))
        {
            Console.Out.Write(result.StandardOutput);
        }

        if (!string.IsNullOrEmpty(result.StandardError))
        {
            Console.Error.Write(result.StandardError);
        }

        return result.ExitCode;
    }

    internal static void ConfigureConsoleEncoding()
    {
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.OutputEncoding = utf8;
        if (!Console.IsInputRedirected)
        {
            Console.InputEncoding = utf8;
        }
    }

    private static bool IsInteractiveFlag(string arg)
    {
        return string.Equals(arg, "--interactive", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "-i", StringComparison.OrdinalIgnoreCase);
    }
}
