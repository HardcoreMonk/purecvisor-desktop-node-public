using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DesktopNode.Host.Tests;

public sealed class DesktopNodeWindowsServiceControllerTests
{
    [Fact]
    public void DeleteClosesServiceHandleBeforeWaitingForMissing()
    {
        var source = File.ReadAllText(ResolveControllerSourcePath());

        Assert.Matches(
            new Regex(
                @"DeleteService\(service\).*?service\.Dispose\(\);.*?return WaitForMissing\(serviceName, timeout\);",
                RegexOptions.Singleline),
            source);
    }

    private static string ResolveControllerSourcePath([CallerFilePath] string testFilePath = "")
    {
        var testDirectory = Path.GetDirectoryName(testFilePath) ?? throw new InvalidOperationException("Test source path is unavailable.");
        return Path.GetFullPath(Path.Combine(testDirectory, "..", "DesktopNode.Host", "DesktopNodeWindowsServiceController.cs"));
    }
}
