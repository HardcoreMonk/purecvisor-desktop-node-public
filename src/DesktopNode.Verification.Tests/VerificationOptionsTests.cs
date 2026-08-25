namespace DesktopNode.Verification.Tests;

public sealed class VerificationOptionsTests
{
    [Fact]
    public void ParsesRepeatedPathsAndSuitesWithoutShellExpansion()
    {
        var request = VerificationOptions.Parse([
            "verify", "--lane", "Full", "--change-tier", "M",
            "--changed-path", "src/DesktopNode.Api/Program.cs",
            "--changed-path", @"web\src\app.ts",
            "--artifact-root", "artifacts/verification-wave-a",
            "--suite", "dotnet", "--suite", "web-typecheck", "--plan-only"
        ]);
        Assert.Equal(VerificationLane.Full, request.RequestedLane);
        Assert.Equal(ChangeTier.M, request.RequestedChangeTier);
        Assert.Equal(["src/DesktopNode.Api/Program.cs", "web/src/app.ts"], request.ChangedPaths);
        Assert.Equal(["dotnet", "web-typecheck"], request.SuiteIds);
        Assert.Null(request.ShardId);
        Assert.True(request.PlanOnly);
    }

    [Theory]
    [InlineData("--suite-and-shard")]
    [InlineData("--duplicate-suite")]
    [InlineData("--missing-changed-path")]
    [InlineData("--rooted-changed-path")]
    [InlineData("--traversal-changed-path")]
    [InlineData("--unknown-option")]
    public void RejectsInvalidGrammarBeforePlanning(string mutation)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(InvalidArguments.For(mutation)));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.StartsWith("cli:", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ParsesShardAndEnumsCaseInsensitively()
    {
        var request = VerificationOptions.Parse([
            "verify", "--lane", "release", "--change-tier", "l",
            "--changed-path", "src/a.cs", "--artifact-root", "artifacts/wave-a",
            "--shard", "installer-policy"
        ]);

        Assert.Equal(VerificationLane.Release, request.RequestedLane);
        Assert.Equal(ChangeTier.L, request.RequestedChangeTier);
        Assert.Equal("installer-policy", request.ShardId);
    }

    [Fact]
    public void NormalizesAndDeduplicatesChangedPathsOrdinallyInFirstSeenOrder()
    {
        var request = VerificationOptions.Parse([
            "verify", "--lane", "Fast", "--change-tier", "S",
            "--changed-path", @".\src\one.cs",
            "--changed-path", "./src/one.cs",
            "--changed-path", "././web/App.ts",
            "--changed-path", "web/app.ts",
            "--artifact-root", "artifacts/wave-a"
        ]);

        Assert.Equal(["src/one.cs", "web/App.ts", "web/app.ts"], request.ChangedPaths);
    }

    [Fact]
    public void CanonicalizesDotAndEmptySegmentsBeforeOrdinalDeduplication()
    {
        var request = VerificationOptions.Parse([
            "verify", "--lane", "Fast", "--change-tier", "S",
            "--changed-path", "src/./installer/file.cs",
            "--changed-path", "src//installer/file.cs",
            "--changed-path", @"src\installer\file.cs",
            "--changed-path", "src/installer/File.cs",
            "--artifact-root", "artifacts/wave-a"
        ]);

        Assert.Equal(["src/installer/file.cs", "src/installer/File.cs"], request.ChangedPaths);
    }

    [Fact]
    public void ReturnedPathAndSuiteCollectionsAreReadOnlySnapshots()
    {
        var request = VerificationOptions.Parse(ValidArguments([
            "--changed-path", "web/app.ts", "--suite", "dotnet"
        ]));

        Assert.False(request.ChangedPaths is List<string>);
        Assert.False(request.SuiteIds is List<string>);
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<string>)request.ChangedPaths).Add("outside.cs"));
        Assert.Throws<NotSupportedException>(() =>
            ((ICollection<string>)request.SuiteIds).Add("web-typecheck"));
        Assert.Equal(["src/a.cs", "web/app.ts"], request.ChangedPaths);
        Assert.Equal(["dotnet"], request.SuiteIds);
    }

    [Theory]
    [InlineData("--lane")]
    [InlineData("--change-tier")]
    [InlineData("--artifact-root")]
    public void RejectsDuplicateSingletonValueOptions(string option)
    {
        var args = ValidArguments([option, DuplicateValueFor(option)]);

        var exception = Assert.Throws<VerificationException>(() => VerificationOptions.Parse(args));

        Assert.Equal($"cli:duplicate-option={option}", exception.Detail);
    }

    [Fact]
    public void RejectsDuplicatePlanOnly()
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(ValidArguments(["--plan-only", "--plan-only"])));

        Assert.Equal("cli:duplicate-option=--plan-only", exception.Detail);
    }

    [Theory]
    [InlineData("--lane")]
    [InlineData("--change-tier")]
    [InlineData("--changed-path")]
    [InlineData("--artifact-root")]
    [InlineData("--suite")]
    [InlineData("--shard")]
    public void RejectsMissingOrOptionAsValue(string option)
    {
        var missing = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(["verify", option]));
        var optionAsValue = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(["verify", option, "--plan-only"]));

        Assert.Equal($"cli:missing-value={option}", missing.Detail);
        Assert.Equal($"cli:missing-value={option}", optionAsValue.Detail);
    }

    [Theory]
    [InlineData("--lane")]
    [InlineData("--change-tier")]
    [InlineData("--changed-path")]
    [InlineData("--artifact-root")]
    [InlineData("--suite")]
    [InlineData("--shard")]
    public void RejectsEmptyValues(string option)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(["verify", option, "  "]));

        Assert.Equal($"cli:empty-value={option}", exception.Detail);
    }

    [Theory]
    [InlineData("--lane", "Fast")]
    [InlineData("--change-tier", "S")]
    [InlineData("--artifact-root", "artifacts/wave-a")]
    [InlineData("--changed-path", "src/a.cs")]
    public void RequiresEveryMandatoryOption(string optionToKeep, string valueToKeep)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(["verify", optionToKeep, valueToKeep]));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.StartsWith("cli:missing-required-option=", exception.Detail, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("run")]
    [InlineData("Verify")]
    [InlineData("")]
    public void RequiresExactVerifyCommand(string command)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse([command]));

        Assert.Equal("cli:unknown-command", exception.Detail);
    }

    [Fact]
    public void RejectsMissingCommand()
    {
        var exception = Assert.Throws<VerificationException>(() => VerificationOptions.Parse([]));

        Assert.Equal("cli:unknown-command", exception.Detail);
    }

    [Theory]
    [InlineData("--lane=Fast")]
    [InlineData("--changed-path=src/a.cs")]
    [InlineData("positional")]
    public void RejectsInlineAndUnexpectedArguments(string argument)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(ValidArguments([argument])));

        Assert.Equal($"cli:unknown-option={argument}", exception.Detail);
    }

    [Theory]
    [InlineData("fastest", "S", "cli:invalid-lane=fastest")]
    [InlineData("Fast", "XL", "cli:invalid-change-tier=XL")]
    public void RejectsInvalidEnumValues(string lane, string tier, string detail)
    {
        var exception = Assert.Throws<VerificationException>(() => VerificationOptions.Parse([
            "verify", "--lane", lane, "--change-tier", tier,
            "--changed-path", "src/a.cs", "--artifact-root", "artifacts/wave-a"
        ]));

        Assert.Equal(detail, exception.Detail);
    }

    [Theory]
    [InlineData("./")]
    [InlineData(".")]
    [InlineData("src/../outside.cs")]
    [InlineData(@"src\..\outside.cs")]
    [InlineData("src/a/../../outside.cs")]
    [InlineData("/outside/file.cs")]
    [InlineData(@"\outside\file.cs")]
    [InlineData(@"C:\outside\file.cs")]
    [InlineData("C:/outside/file.cs")]
    [InlineData(@"\\server\share\file.cs")]
    [InlineData("//server/share/file.cs")]
    [InlineData(@"\\?\C:\outside\file.cs")]
    [InlineData(@"\\.\pipe\pcv")]
    public void RejectsBlankTraversalOrRootedChangedPaths(string changedPath)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(ValidArguments(["--changed-path", changedPath])));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.StartsWith("cli:invalid-changed-path", exception.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsDuplicateShard()
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(ValidArguments(["--shard", "web", "--shard", "dotnet"])));

        Assert.Equal("cli:duplicate-option=--shard", exception.Detail);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("Web")]
    public void RejectsShardOutsideApprovedGrammar(string shard)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            VerificationOptions.Parse(ValidArguments(["--shard", shard])));

        Assert.Equal($"cli:invalid-shard={shard}", exception.Detail);
    }

    private static string DuplicateValueFor(string option) => option switch
    {
        "--lane" => "Full",
        "--change-tier" => "M",
        "--artifact-root" => "artifacts/wave-b",
        "--shard" => "web",
        _ => throw new ArgumentOutOfRangeException(nameof(option))
    };

    private static string[] ValidArguments(string[] additions) =>
    [
        "verify", "--lane", "Fast", "--change-tier", "S",
        "--changed-path", "src/a.cs", "--artifact-root", "artifacts/wave-a",
        .. additions
    ];

    private static class InvalidArguments
    {
        internal static string[] For(string mutation)
        {
            string[] valid =
            [
                "verify", "--lane", "Fast", "--change-tier", "S",
                "--changed-path", "src/a.cs", "--artifact-root", "artifacts/wave-a"
            ];

            return mutation switch
            {
                "--suite-and-shard" => [.. valid, "--suite", "dotnet", "--shard", "web"],
                "--duplicate-suite" => [.. valid, "--suite", "dotnet", "--suite", "dotnet"],
                "--missing-changed-path" => valid.Where(value => value is not "--changed-path" and not "src/a.cs").ToArray(),
                "--rooted-changed-path" => ReplaceChangedPath(valid, @"D:\outside\file.cs"),
                "--traversal-changed-path" => ReplaceChangedPath(valid, "../outside/file.cs"),
                "--unknown-option" => [.. valid, "--base-ref", "main"],
                _ => throw new ArgumentOutOfRangeException(nameof(mutation))
            };
        }

        private static string[] ReplaceChangedPath(string[] values, string replacement)
        {
            var copy = values.ToArray();
            copy[Array.IndexOf(copy, "--changed-path") + 1] = replacement;
            return copy;
        }
    }
}
