namespace DesktopNode.Verification.Tests;

public sealed class CutoverGitBoundaryTests
{
    [Fact]
    public async Task AcceptsBranchMergeAndLaterDocumentationHeads()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var boundary = new CutoverGitBoundary(new SystemProcessRunner());

        var branch = await boundary.ValidateAsync(
            repository.Root,
            repository.CutoverSha,
            repository.ShadowSha,
            CancellationToken.None);
        Assert.Equal(repository.CutoverSha, branch.CutoverSha);
        Assert.Equal([CutoverRepository.WorkflowPath], branch.ChangedPaths);

        var mergeSha = await repository.MergeWorkIntoMainAsync();
        var merge = await boundary.ValidateAsync(
            repository.Root,
            mergeSha,
            repository.ShadowSha,
            CancellationToken.None);
        Assert.Equal(repository.CutoverSha, merge.CutoverSha);

        var documentationSha = await repository.CommitDocumentationAfterMergeAsync();
        var later = await boundary.ValidateAsync(
            repository.Root,
            documentationSha,
            repository.ShadowSha,
            CancellationToken.None);
        Assert.Equal(repository.CutoverSha, later.CutoverSha);
    }

    [Fact]
    public async Task RejectsZeroOrMultipleDirectChildren()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var boundary = new CutoverGitBoundary(new SystemProcessRunner());

        await AssertRejectedAsync(
            boundary,
            repository,
            repository.ShadowSha,
            "cutover-history=direct-child-count");

        var multipleHead = await repository.CreateMultipleDirectChildrenHeadAsync();
        await AssertRejectedAsync(
            boundary,
            repository,
            multipleHead,
            "cutover-history=direct-child-count");
    }

    [Fact]
    public async Task RejectsHeadThatDoesNotContainShadow()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var unrelated = await repository.CommitUnrelatedMainAsync();

        await AssertRejectedAsync(
            new CutoverGitBoundary(new SystemProcessRunner()),
            repository,
            unrelated,
            "cutover-history=shadow-not-ancestor");
    }

    [Fact]
    public async Task RejectsDirtyIndexOrWorktree()
    {
        using var repository = await CutoverRepository.CreateAsync();
        File.AppendAllText(Path.Combine(repository.Root, "README.md"), "dirty\n");

        await AssertRejectedAsync(
            new CutoverGitBoundary(new SystemProcessRunner()),
            repository,
            repository.CutoverSha,
            "cutover-worktree=dirty");
    }

    [Fact]
    public async Task RejectsPathOutsideFrozenAllowlist()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var invalidHead = await repository.CommitFromShadowAsync("unexpected.txt", "not allowed\n");

        await AssertRejectedAsync(
            new CutoverGitBoundary(new SystemProcessRunner()),
            repository,
            invalidHead,
            "cutover-diff=path");
    }

    [Fact]
    public async Task RejectsRenameInsideAllowlist()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var (renameShadow, invalidHead) = await repository.CommitRenameAsync();

        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new CutoverGitBoundary(new SystemProcessRunner()).ValidateAsync(
                repository.Root, invalidHead, renameShadow, CancellationToken.None));
        Assert.Equal("cutover-diff=rename-copy", exception.Detail);
    }

    [Fact]
    public async Task RejectsCopyInsideAllowlist()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var (copyShadow, invalidHead) = await repository.CommitCopyAsync();

        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new CutoverGitBoundary(new SystemProcessRunner()).ValidateAsync(
                repository.Root, invalidHead, copyShadow, CancellationToken.None));
        Assert.Equal("cutover-diff=rename-copy", exception.Detail);
    }

    [Fact]
    public async Task RejectsMergeCommitAsDirectCutoverChild()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var invalidHead = await repository.CreateMergeCutoverChildAsync();

        await AssertRejectedAsync(
            new CutoverGitBoundary(new SystemProcessRunner()),
            repository,
            invalidHead,
            "cutover-history=cutover-merge");
    }

    [Fact]
    public async Task RejectsGitlinkAtAllowedPath()
    {
        using var repository = await CutoverRepository.CreateAsync();
        var invalidHead = await repository.CommitGitlinkFromShadowAsync();

        await AssertRejectedAsync(
            new CutoverGitBoundary(new SystemProcessRunner()),
            repository,
            invalidHead,
            "cutover-diff=gitlink");
    }

    [Fact]
    public async Task RejectsShallowHistory()
    {
        using var source = await CutoverRepository.CreateAsync();
        using var shallow = await source.CreateShallowCloneAsync();

        await AssertRejectedAsync(
            new CutoverGitBoundary(new SystemProcessRunner()),
            shallow,
            shallow.CutoverSha,
            "cutover-history=shadow-missing");
    }

    [Theory]
    [InlineData("HEAD")]
    [InlineData("abc")]
    [InlineData("000000000000000000000000000000000000000g")]
    public async Task RejectsNonFortyHexVerificationHead(string head)
    {
        using var repository = await CutoverRepository.CreateAsync();

        await AssertRejectedAsync(
            new CutoverGitBoundary(new SystemProcessRunner()),
            repository,
            head,
            "cutover-head=invalid");
    }

    [Fact]
    public async Task RejectsNonFortyHexShadow()
    {
        using var repository = await CutoverRepository.CreateAsync();

        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new CutoverGitBoundary(new SystemProcessRunner()).ValidateAsync(
                repository.Root, repository.CutoverSha, "HEAD", CancellationToken.None));

        Assert.Equal("cutover-shadow=invalid", exception.Detail);
    }

    [Fact]
    public async Task RejectsAbsoluteDiffOutputAndUsesOnlyGitArgumentArrays()
    {
        const string shadow = "1111111111111111111111111111111111111111";
        const string cutover = "2222222222222222222222222222222222222222";
        var invocations = new List<ProcessInvocation>();
        var call = 0;
        var runner = new RecordingProcessRunner(handler: (invocation, _) =>
        {
            invocations.Add(invocation);
            var output = call++ switch
            {
                0 => cutover + "\n",
                1 => string.Empty,
                2 => shadow + "\n",
                3 => string.Empty,
                4 => $"{cutover} {shadow}\n",
                5 => string.Empty,
                6 => "M\tC:/escape.yml\n",
                _ => string.Empty
            };
            return Task.FromResult(new ProcessExecutionResult(
                0, 1, false, false, output, string.Empty, new string('0', 64)));
        });
        var root = Path.GetFullPath(Path.GetTempPath());

        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            new CutoverGitBoundary(runner).ValidateAsync(
                root, cutover, shadow, CancellationToken.None));

        Assert.Equal("cutover-diff=absolute-path", exception.Detail);
        Assert.All(invocations, invocation =>
        {
            Assert.Equal("git", invocation.FileName);
            Assert.Equal(root, invocation.WorkingDirectory);
            Assert.DoesNotContain(invocation.Arguments, argument =>
                argument.Contains('|') || argument.Contains('>') ||
                argument.Contains("pwsh", StringComparison.OrdinalIgnoreCase) ||
                argument.Contains("powershell", StringComparison.OrdinalIgnoreCase));
        });
    }

    private static async Task AssertRejectedAsync(
        CutoverGitBoundary boundary,
        CutoverRepository repository,
        string head,
        string detail)
    {
        var exception = await Assert.ThrowsAsync<VerificationException>(() =>
            boundary.ValidateAsync(repository.Root, head, repository.ShadowSha, CancellationToken.None));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal(detail, exception.Detail);
    }
}

internal sealed class CutoverRepository : IDisposable
{
    internal const string WorkflowPath = ".github/workflows/development-gates.yml";
    private const string PolicyPath = "docs/DEVELOPMENT_VERIFICATION_POLICY.md";
    private readonly string ownedRoot;

    private CutoverRepository(
        string ownedRoot,
        string root,
        string initialSha,
        string shadowSha,
        string cutoverSha)
    {
        this.ownedRoot = ownedRoot;
        Root = root;
        InitialSha = initialSha;
        ShadowSha = shadowSha;
        CutoverSha = cutoverSha;
    }

    internal string Root { get; }
    internal string InitialSha { get; }
    internal string ShadowSha { get; }
    internal string CutoverSha { get; }

    internal static async Task<CutoverRepository> CreateAsync()
    {
        var ownedRoot = Path.Combine(Path.GetTempPath(), $"pcv-cutover-git-{Guid.NewGuid():N}");
        var root = Path.Combine(ownedRoot, "repository");
        Directory.CreateDirectory(root);
        var fixture = new CutoverRepository(ownedRoot, root, string.Empty, string.Empty, string.Empty);
        await fixture.GitAsync("init", "-b", "main");
        foreach (var path in CutoverGitBoundary.AllowedCutoverPaths)
        {
            fixture.Write(path, $"initial:{path}\n");
        }
        fixture.Write("README.md", "initial\n");
        await fixture.CommitAsync("initial");
        var initial = await fixture.ShaAsync("HEAD");

        await fixture.GitAsync("switch", "-c", "work");
        fixture.Write("README.md", "shadow\n");
        await fixture.CommitAsync("shadow");
        var shadow = await fixture.ShaAsync("HEAD");

        fixture.Write(WorkflowPath, "cutover workflow\n");
        await fixture.CommitAsync("cutover");
        var cutover = await fixture.ShaAsync("HEAD");
        return new CutoverRepository(ownedRoot, root, initial, shadow, cutover);
    }

    internal async Task<string> MergeWorkIntoMainAsync()
    {
        await GitAsync("switch", "main");
        await GitAsync(
            "-c", "user.name=PCV Test", "-c", "user.email=41898282+github-actions[bot]@users.noreply.github.com",
            "merge", "--no-ff", "work", "-m", "merge cutover");
        return await ShaAsync("HEAD");
    }

    internal async Task<string> CommitDocumentationAfterMergeAsync()
    {
        Write(PolicyPath, "post merge documentation\n");
        await CommitAsync("documentation");
        return await ShaAsync("HEAD");
    }

    internal async Task<string> CommitUnrelatedMainAsync()
    {
        await GitAsync("switch", "main");
        Write("README.md", "unrelated\n");
        await CommitAsync("unrelated");
        return await ShaAsync("HEAD");
    }

    internal async Task<string> CommitFromShadowAsync(string path, string content)
    {
        await GitAsync("switch", "-c", $"invalid-{Guid.NewGuid():N}", ShadowSha);
        Write(path, content);
        await CommitAsync("invalid path");
        return await ShaAsync("HEAD");
    }

    internal async Task<(string Shadow, string Head)> CommitRenameAsync()
    {
        await GitAsync("switch", "-c", $"rename-{Guid.NewGuid():N}", ShadowSha);
        const string destination = "docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md";
        var sourcePath = Path.Combine(Root, WorkflowPath.Replace('/', Path.DirectorySeparatorChar));
        var destinationPath = Path.Combine(Root, destination.Replace('/', Path.DirectorySeparatorChar));
        File.Delete(destinationPath);
        await CommitAsync("rename shadow boundary");
        var renameShadow = await ShaAsync("HEAD");
        File.Move(sourcePath, destinationPath);
        await CommitAsync("rename inside allowlist");
        return (renameShadow, await ShaAsync("HEAD"));
    }

    internal async Task<(string Shadow, string Head)> CommitCopyAsync()
    {
        await GitAsync("switch", "-c", $"copy-{Guid.NewGuid():N}", ShadowSha);
        const string destination = "docs/ga-ready/evidence/pester-free-required-ci-cutover-2026-08-25.md";
        var sourcePath = Path.Combine(Root, WorkflowPath.Replace('/', Path.DirectorySeparatorChar));
        var destinationPath = Path.Combine(Root, destination.Replace('/', Path.DirectorySeparatorChar));
        File.Delete(destinationPath);
        await CommitAsync("copy shadow boundary");
        var copyShadow = await ShaAsync("HEAD");
        File.Copy(sourcePath, destinationPath);
        await CommitAsync("copy inside allowlist");
        return (copyShadow, await ShaAsync("HEAD"));
    }

    internal async Task<string> CreateMergeCutoverChildAsync()
    {
        await GitAsync("switch", "-c", $"side-{Guid.NewGuid():N}", InitialSha);
        Write("side.txt", "side branch\n");
        await CommitAsync("side branch");
        var side = await ShaAsync("HEAD");

        await GitAsync("switch", "-c", $"merge-child-{Guid.NewGuid():N}", ShadowSha);
        await GitAsync(
            "-c", "user.name=PCV Test", "-c", "user.email=41898282+github-actions[bot]@users.noreply.github.com",
            "merge", "--no-ff", side, "-m", "merge direct child");
        return await ShaAsync("HEAD");
    }

    internal async Task<string> CommitGitlinkFromShadowAsync()
    {
        await GitAsync("switch", "-c", $"gitlink-{Guid.NewGuid():N}", ShadowSha);
        await GitAsync("rm", "--", PolicyPath);
        await GitAsync("update-index", "--add", "--cacheinfo", $"160000,{ShadowSha},{PolicyPath}");
        await GitAsync(
            "-c", "user.name=PCV Test", "-c", "user.email=41898282+github-actions[bot]@users.noreply.github.com",
            "commit", "-m", "gitlink inside allowlist");
        await GitAsync("clone", "--no-checkout", ".", PolicyPath);
        await GitAsync("-C", PolicyPath, "checkout", ShadowSha);
        return await ShaAsync("HEAD");
    }

    internal async Task<string> CreateMultipleDirectChildrenHeadAsync()
    {
        await GitAsync("switch", "-c", $"child-a-{Guid.NewGuid():N}", ShadowSha);
        Write(WorkflowPath, "child a\n");
        await CommitAsync("child a");
        var childA = await ShaAsync("HEAD");

        await GitAsync("switch", "-c", $"child-b-{Guid.NewGuid():N}", ShadowSha);
        Write(PolicyPath, "child b\n");
        await CommitAsync("child b");

        await GitAsync(
            "-c", "user.name=PCV Test", "-c", "user.email=41898282+github-actions[bot]@users.noreply.github.com",
            "merge", "--no-ff", childA, "-m", "merge two children");
        return await ShaAsync("HEAD");
    }

    internal async Task<CutoverRepository> CreateShallowCloneAsync()
    {
        var cloneRoot = Path.Combine(Path.GetTempPath(), $"pcv-cutover-shallow-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cloneRoot);
        var clone = new CutoverRepository(cloneRoot, cloneRoot, InitialSha, ShadowSha, CutoverSha);
        var sourceUri = new Uri(Root).AbsoluteUri;
        await clone.GitAsync("clone", "--depth", "1", "--branch", "work", sourceUri, ".");
        return clone;
    }

    private void Write(string relativePath, string contents)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
    }

    private async Task CommitAsync(string message)
    {
        await GitAsync("add", "--all");
        await GitAsync(
            "-c", "user.name=PCV Test", "-c", "user.email=41898282+github-actions[bot]@users.noreply.github.com",
            "commit", "-m", message);
    }

    private async Task<string> ShaAsync(string revision) =>
        (await GitAsync("rev-parse", revision)).Trim().ToLowerInvariant();

    private async Task<string> GitAsync(params string[] arguments)
    {
        var result = await new SystemProcessRunner().RunAsync(
            new ProcessInvocation(
                "cutover-fixture",
                "git",
                Array.AsReadOnly(arguments),
                Root,
                TimeSpan.FromSeconds(30),
                VerificationCatalogFixture.AllowedExecutables),
            CancellationToken.None);
        if (result.ExitCode != 0 || result.TimedOut || result.Cancelled)
        {
            throw new InvalidOperationException($"fixture-git-failed:{string.Join(',', arguments)}:{result.StandardError}");
        }

        return result.StandardOutput;
    }

    public void Dispose()
    {
        var root = Path.GetFullPath(ownedRoot);
        var temporaryRoot = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        if (!root.StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileName(root).StartsWith("pcv-cutover-", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("cutover-fixture-cleanup-boundary");
        }

        if (Directory.Exists(root))
        {
            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(root, recursive: true);
        }
    }
}
