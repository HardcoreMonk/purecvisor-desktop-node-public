namespace DesktopNode.Verification.Tests;

public sealed class RepositoryPathsTests
{
    [Fact]
    public void FindsWorktreeRootBySolutionAndCatalogAnchors()
    {
        using var tree = TemporaryRepository.Create();
        var nested = Directory.CreateDirectory(Path.Combine(tree.Root, "src", "nested", "deeper")).FullName;
        Assert.Equal(tree.Root, RepositoryLocator.Find(nested));
    }

    [Fact]
    public void AcceptsOnlyStrictArtifactsOrRunnerTempDescendants()
    {
        using var tree = TemporaryRepository.Create();
        var runnerTemp = Directory.CreateDirectory(Path.Combine(tree.Parent, "runner-temp")).FullName;
        Assert.Equal(Path.Combine(tree.Root, "artifacts", "wave-a"),
            ArtifactRootPolicy.ResolveAndValidate(tree.Root, "artifacts/wave-a", runnerTemp, tree.UserProfile));
        Assert.Equal(Path.Combine(runnerTemp, "wave-a"),
            ArtifactRootPolicy.ResolveAndValidate(tree.Root, Path.Combine(runnerTemp, "wave-a"), runnerTemp, tree.UserProfile));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("artifacts")]
    [InlineData("../outside")]
    [InlineData("%UNRESOLVED%/wave-a")]
    [InlineData("~/wave-a")]
    public void RejectsBroadOrUnresolvedArtifactRoots(string candidate)
    {
        using var tree = TemporaryRepository.Create();
        var exception = Assert.Throws<VerificationException>(() =>
            ArtifactRootPolicy.ResolveAndValidate(tree.Root, candidate, null, tree.UserProfile));
        Assert.Equal("PCV_VERIFY_ARTIFACT_ROOT_INVALID", exception.Code);
    }

    [Fact]
    public void FindsRepositoryWhenStartedAtRepositoryRoot()
    {
        using var tree = TemporaryRepository.Create();

        Assert.Equal(tree.Root, RepositoryLocator.Find(tree.Root));
    }

    [Theory]
    [InlineData("solution")]
    [InlineData("catalog")]
    [InlineData("both")]
    public void RequiresBothRepositoryAnchors(string missing)
    {
        using var tree = TemporaryRepository.Create();
        if (missing is "solution" or "both")
        {
            File.Delete(Path.Combine(tree.Root, "src", "DesktopNode.sln"));
        }
        if (missing is "catalog" or "both")
        {
            File.Delete(Path.Combine(tree.Root, "config", "development-verification-suites.json"));
        }

        var exception = Assert.Throws<VerificationException>(() => RepositoryLocator.Find(tree.Root));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("repository-root-not-found", exception.Detail);
    }

    [Fact]
    public void FilesystemRootWithoutAnchorsIsNotARepository()
    {
        using var tree = TemporaryRepository.Create();
        var filesystemRoot = Path.GetPathRoot(tree.Root)!;

        var exception = Assert.Throws<VerificationException>(() => RepositoryLocator.Find(filesystemRoot));

        Assert.Equal("repository-root-not-found", exception.Detail);
    }

    [Theory]
    [InlineData("$(RUNNER_TEMP)/wave-a")]
    [InlineData("${RUNNER_TEMP}/wave-a")]
    [InlineData("prefix%NAME%/wave-a")]
    [InlineData("artifacts/$RUNNER_TEMP/wave-a")]
    [InlineData("artifacts/$env:RUNNER_TEMP/wave-a")]
    public void RejectsEveryUnresolvedVariableForm(string requestedRoot)
    {
        using var tree = TemporaryRepository.Create();

        AssertArtifactRootInvalid(tree, requestedRoot, null);
    }

    [Fact]
    public void RejectsFilesystemRootAndRepositoryRoot()
    {
        using var tree = TemporaryRepository.Create();

        AssertArtifactRootInvalid(tree, Path.GetPathRoot(tree.Root)!, tree.Parent);
        AssertArtifactRootInvalid(tree, tree.Root, tree.Parent);
    }

    [Fact]
    public void RejectsRunnerTempItselfBecauseContainmentMustBeStrict()
    {
        using var tree = TemporaryRepository.Create();
        var runnerTemp = Directory.CreateDirectory(Path.Combine(tree.Parent, "runner-temp")).FullName;

        AssertArtifactRootInvalid(tree, runnerTemp, runnerTemp);
    }

    [Fact]
    public void EnforcesStrictRunnerTempContainmentWhenItOverlapsArtifacts()
    {
        using var tree = TemporaryRepository.Create();
        var runnerTemp = Directory.CreateDirectory(
            Path.Combine(tree.Root, "artifacts", "runner-temp")).FullName;
        var child = Path.Combine(runnerTemp, "wave-a");

        var exception = AssertArtifactRootInvalid(tree, runnerTemp, runnerTemp);

        Assert.Equal("artifact-root-invalid:runner-temp-equal", exception.Detail);
        Assert.Equal(child,
            ArtifactRootPolicy.ResolveAndValidate(tree.Root, child, runnerTemp, tree.UserProfile));
    }

    [Fact]
    public void RejectsRelativeRunnerTempWithoutResolvingAgainstAmbientDirectory()
    {
        using var tree = TemporaryRepository.Create();

        var exception = AssertArtifactRootInvalid(tree, "artifacts/wave-a", "runner-temp");

        Assert.Equal("artifact-root-invalid:runner-temp", exception.Detail);
    }

    [Fact]
    public void RejectsFilesystemAndRepositoryRootsAsRunnerTemp()
    {
        using var tree = TemporaryRepository.Create();
        var privateCandidate = Path.Combine(tree.Root, "src", "private-customer");

        var filesystem = AssertArtifactRootInvalid(tree, privateCandidate, Path.GetPathRoot(tree.Root)!);
        var repository = AssertArtifactRootInvalid(tree, privateCandidate, tree.Root);

        Assert.Equal("artifact-root-invalid:runner-temp", filesystem.Detail);
        Assert.Equal("artifact-root-invalid:runner-temp", repository.Detail);
        Assert.DoesNotContain(privateCandidate, filesystem.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCandidate, repository.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RunnerTempInsideRepositoryCannotAuthorizeNonArtifactPaths()
    {
        using var tree = TemporaryRepository.Create();
        var runnerTemp = Directory.CreateDirectory(Path.Combine(tree.Root, "src", "runner-temp")).FullName;
        var candidate = Path.Combine(runnerTemp, "wave-a");

        var exception = AssertArtifactRootInvalid(tree, candidate, runnerTemp);

        Assert.Equal("artifact-root-invalid:outside-boundary", exception.Detail);
    }

    [Fact]
    public void ExternalRunnerTempCannotAuthorizeRepositorySourcePaths()
    {
        using var tree = TemporaryRepository.Create();
        var candidate = Path.Combine(tree.Root, "src", "wave-a");

        var exception = AssertArtifactRootInvalid(tree, candidate, tree.Parent);

        Assert.Equal("artifact-root-invalid:outside-boundary", exception.Detail);
    }

    [Fact]
    public void RejectsUserProfileItselfAndDescendantsEvenWhenOtherwiseAllowed()
    {
        using var tree = TemporaryRepository.Create();
        var profileChild = Path.Combine(tree.UserProfile, "wave-a");

        AssertArtifactRootInvalid(tree, tree.UserProfile, tree.Parent);
        AssertArtifactRootInvalid(tree, profileChild, tree.UserProfile);
    }

    [Fact]
    public void AcceptsRepositoryArtifactsWhenHostedCheckoutIsInsideUserProfile()
    {
        using var tree = TemporaryRepository.Create(repositoryInsideUserProfile: true);
        var candidate = Path.Combine(tree.Root, "artifacts", "shadow", "web");

        Assert.Equal(
            candidate,
            ArtifactRootPolicy.ResolveAndValidate(
                tree.Root,
                "artifacts/shadow/web",
                Path.Combine(tree.Parent, "runner-temp"),
                tree.UserProfile));
        AssertArtifactRootInvalid(
            tree,
            Path.Combine(tree.UserProfile, "outside-repository"),
            Path.Combine(tree.Parent, "runner-temp"));
    }

    [Fact]
    public void RejectsSiblingPrefixesOfAllowedParents()
    {
        using var tree = TemporaryRepository.Create();
        var runnerTemp = Directory.CreateDirectory(Path.Combine(tree.Parent, "runner-temp")).FullName;

        AssertArtifactRootInvalid(tree, Path.Combine(tree.Root, "artifacts-other", "wave-a"), runnerTemp);
        AssertArtifactRootInvalid(tree, Path.Combine(tree.Parent, "runner-temp-other", "wave-a"), runnerTemp);
    }

    [Fact]
    public void RejectsInvalidPathWithFixedNonSecretDetail()
    {
        using var tree = TemporaryRepository.Create();
        var privateFragment = "private-customer";
        var invalid = privateFragment + '\0';

        var exception = Assert.Throws<VerificationException>(() =>
            ArtifactRootPolicy.ResolveAndValidate(tree.Root, invalid, null, tree.UserProfile));

        Assert.Equal(VerificationErrorCodes.ArtifactRootInvalid, exception.Code);
        Assert.Equal("artifact-root-invalid:path", exception.Detail);
        Assert.DoesNotContain(privateFragment, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RejectsInvalidRepositoryLocatorPathWithoutLeakingIt()
    {
        const string privateFragment = "private-customer";
        var invalid = privateFragment + '\0';

        var exception = Assert.Throws<VerificationException>(() => RepositoryLocator.Find(invalid));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal("repository-root-not-found", exception.Detail);
        Assert.DoesNotContain(privateFragment, exception.Message, StringComparison.Ordinal);
    }

    private static VerificationException AssertArtifactRootInvalid(
        TemporaryRepository tree,
        string requestedRoot,
        string? runnerTemp)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            ArtifactRootPolicy.ResolveAndValidate(tree.Root, requestedRoot, runnerTemp, tree.UserProfile));

        Assert.Equal(VerificationErrorCodes.ArtifactRootInvalid, exception.Code);
        Assert.StartsWith("artifact-root-invalid:", exception.Detail, StringComparison.Ordinal);
        return exception;
    }

    private sealed class TemporaryRepository : IDisposable
    {
        private const string OwnedPrefix = "pcv-verify-path-tests-";
        private readonly string _ownedParent;

        private TemporaryRepository(string ownedParent, string root, string userProfile)
        {
            _ownedParent = ownedParent;
            Parent = ownedParent;
            Root = root;
            UserProfile = userProfile;
        }

        internal string Parent { get; }
        internal string Root { get; }
        internal string UserProfile { get; }

        internal static TemporaryRepository Create(bool repositoryInsideUserProfile = false)
        {
            var ownedParent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), OwnedPrefix + Guid.NewGuid().ToString("N")));
            var userProfile = Directory.CreateDirectory(Path.Combine(ownedParent, "user-profile")).FullName;
            var rootParent = repositoryInsideUserProfile ? userProfile : ownedParent;
            var root = Directory.CreateDirectory(Path.Combine(rootParent, "repository")).FullName;
            Directory.CreateDirectory(Path.Combine(root, "src"));
            Directory.CreateDirectory(Path.Combine(root, "config"));
            File.WriteAllText(Path.Combine(root, "src", "DesktopNode.sln"), string.Empty);
            File.WriteAllText(Path.Combine(root, "config", "development-verification-suites.json"), string.Empty);
            return new TemporaryRepository(ownedParent, root, userProfile);
        }

        public void Dispose()
        {
            var resolvedParent = Path.GetFullPath(_ownedParent);
            var resolvedTemp = Path.GetFullPath(Path.GetTempPath());
            var leaf = Path.GetFileName(resolvedParent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!leaf.StartsWith(OwnedPrefix, StringComparison.Ordinal) ||
                !string.Equals(Path.GetDirectoryName(resolvedParent), resolvedTemp.TrimEnd(Path.DirectorySeparatorChar), PathComparison))
            {
                throw new InvalidOperationException("Temporary repository cleanup boundary rejected.");
            }

            if (Directory.Exists(resolvedParent))
            {
                Directory.Delete(resolvedParent, recursive: true);
            }
        }

        private static StringComparison PathComparison => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }
}
