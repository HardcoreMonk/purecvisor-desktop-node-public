using System.Text.Json.Nodes;

namespace DesktopNode.Verification.Tests;

public sealed class RequiredCiPolicyTests
{
    private const string Checkout = "actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd";
    private const string SetupDotNet = "actions/setup-dotnet@d4c94342e560b34958eacfc5d055d21461ed1c5d";
    private const string SetupNode = "actions/setup-node@2028fbc5c25fe9cf00d9f06a71cc4710d4507903";
    private const string UploadArtifact = "actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f";

    [Fact]
    public void ValidShadowWorkflowReportsLegacyAndReplacementBoundaries()
    {
        var result = RequiredCiPolicy.Validate(ShadowWorkflow(), Catalog("shadow-ready"));

        Assert.Equal(RequiredCiMode.Shadow, result.Mode);
        Assert.Equal(
            ["dotnet-tests", "web-tests", "packaging-pester", "installer-web-pester"],
            result.JobIds);
        Assert.Equal(["dotnet", "web", "delivery", "installer-policy"], result.Shards);
        Assert.Equal(2, result.PesterInvocationCount);
        Assert.Equal(2, result.NonAdminPowerShellInvocationCount);
        Assert.Equal(0, result.HostMutationInvocationCount);
    }

    [Fact]
    public void ValidActiveWorkflowHasExactZeroInvocationUnion()
    {
        var result = RequiredCiPolicy.Validate(ActiveWorkflow(), Catalog("active"));

        Assert.Equal(RequiredCiMode.Active, result.Mode);
        Assert.Equal(["dotnet", "web", "delivery", "installer-policy"], result.JobIds);
        Assert.Equal(["dotnet", "web", "delivery", "installer-policy"], result.Shards);
        Assert.Equal(0, result.PesterInvocationCount);
        Assert.Equal(0, result.NonAdminPowerShellInvocationCount);
        Assert.Equal(0, result.HostMutationInvocationCount);
    }

    [Fact]
    public void DuplicateSemanticJobKeyIsRejected()
    {
        var yaml = ShadowWorkflow() + "\n  dotnet-tests:\n    runs-on: windows-latest\n    steps: []\n";

        AssertRejected(yaml, Catalog("shadow-ready"), "required-ci-yaml=duplicate-key");
    }

    [Fact]
    public void AnchorsAndAliasesAreRejected()
    {
        var yaml = ShadowWorkflow()
            .Replace("jobs:\n", "shared-runner: &shared-runner windows-latest\njobs:\n", StringComparison.Ordinal)
            .Replace("runs-on: windows-latest", "runs-on: *shared-runner", StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("shadow-ready"), "required-ci-yaml=anchor-alias");
    }

    [Fact]
    public void MalformedYamlIsRejected() =>
        AssertRejected("jobs: [", Catalog("shadow-ready"), "required-ci-yaml=malformed");

    [Fact]
    public void MissingRequiredJobIsRejected() =>
        AssertRejected(
            ShadowWorkflow(["dotnet-tests", "web-tests", "packaging-pester"]),
            Catalog("shadow-ready"),
            "required-ci-jobs=mismatch");

    [Fact]
    public void ExtraRequiredJobIsRejected() =>
        AssertRejected(
            ShadowWorkflow([
                "dotnet-tests", "web-tests", "packaging-pester", "installer-web-pester", "extra-job"]),
            Catalog("shadow-ready"),
            "required-ci-jobs=mismatch");

    [Fact]
    public void ConditionalRequiredJobIsRejected()
    {
        var yaml = ShadowWorkflow().Replace(
            "  dotnet-tests:\n    runs-on:",
            "  dotnet-tests:\n    if: false\n    runs-on:",
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("shadow-ready"), "required-ci-job:dotnet-tests=conditional");
    }

    [Fact]
    public void WrongRunnerIsRejected()
    {
        var yaml = ActiveWorkflow().Replace(
            "  web:\n    name: web\n    runs-on: ubuntu-latest",
            "  web:\n    name: web\n    runs-on: windows-latest",
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-job:web=runner");
    }

    [Fact]
    public void MissingRequiredSetupIsRejected()
    {
        var yaml = ActiveWorkflow().Replace(
            $"      - uses: {SetupNode}\n",
            string.Empty,
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-job:web=setup");
    }

    [Fact]
    public void MutableActionTagIsRejected()
    {
        var yaml = ActiveWorkflow().Replace(Checkout, "actions/checkout@v6.0.2", StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-action=unreviewed");
    }

    [Theory]
    [InlineData("shell: pwsh", "required-ci-active=nonadmin-powershell")]
    [InlineData("run: powershell -NoProfile -Command dotnet --info", "required-ci-active=nonadmin-powershell")]
    [InlineData("run: Invoke-Pester -Path tests", "required-ci-active=pester")]
    [InlineData("run: Install-Module Pester", "required-ci-active=pester")]
    public void ActiveExecutablePowerShellOrPesterIsRejected(string executableField, string detail)
    {
        var yaml = ActiveWorkflow() +
            "      - name: Forbidden executable\n" +
            $"        {executableField}\n";

        AssertRejected(yaml, Catalog("active"), detail);
    }

    [Fact]
    public void FoldedYamlCannotHideForbiddenExecutable()
    {
        var yaml = ActiveWorkflow().Replace(
            "        run: dotnet run --project",
            "        run: >-\n          dotnet --info && Invoke-Pester; dotnet run --project",
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-active=pester");
    }

    [Fact]
    public void HostMutationTokenIsRejected()
    {
        var yaml = ActiveWorkflow().Replace(
            "        run: dotnet run --project",
            "        run: sc.exe start PurecVisor && dotnet run --project",
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci=host-mutation");
    }

    [Fact]
    public void DuplicateShardIsRejected()
    {
        var yaml = ActiveWorkflow().Replace(
            "--shard installer-policy",
            "--shard delivery",
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-shards=mismatch");
    }

    [Fact]
    public void MissingArtifactIsRejected()
    {
        var yaml = ActiveWorkflow().Replace(
            ActiveArtifactStep("installer-policy"),
            string.Empty,
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-job:installer-policy=artifact");
    }

    [Fact]
    public void WrongArtifactPathIsRejected()
    {
        var yaml = ActiveWorkflow().Replace(
            "path: artifacts/development-gates-delivery",
            "path: artifacts/wrong-delivery",
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-job:delivery=artifact");
    }

    [Fact]
    public void CatalogWorkflowModeMismatchIsRejected() =>
        AssertRejected(
            ActiveWorkflow(),
            Catalog("plan-only-foundation"),
            "required-ci-catalog=activation");

    [Fact]
    public void CanonicalWaveDLedgerIsValidForShadowPendingCi()
    {
        var json = File.ReadAllText(Path.Combine(
            VerificationCatalogFixture.RepositoryRoot,
            "config",
            "development-verification-migration-manifest.json"));

        var result = RequiredCiMigrationLedger.Validate(json, RequiredCiMode.Shadow);

        Assert.Equal(62, result.FileCount);
        Assert.Equal(627, result.ContractCount);
        Assert.Equal(627, result.LocalPassCount);
        Assert.Equal(0, result.CiPassCount);
        Assert.Equal(627, result.CiPendingCount);
    }

    [Fact]
    public void ActiveLedgerRequiresEveryFileAndContractAtCutoverCiPass()
    {
        var path = Path.Combine(
            VerificationCatalogFixture.RepositoryRoot,
            "config",
            "development-verification-migration-manifest.json");
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        foreach (var row in root["entries"]!.AsArray().Concat(root["contracts"]!.AsArray()))
        {
            row!["parity_status"] = "cutover";
            row["ci_parity"]!["status"] = "pass";
            row["ci_parity"]!["evidence"] = "shadow-run:1";
        }
        root["cutover_locator"] = new JsonObject
        {
            ["shadow_sha"] = "1111111111111111111111111111111111111111",
            ["shadow_run_id"] = 123,
            ["shadow_run_url"] = "https://github.com/HardcoreMonk/purecvisor-desktop-node-public/actions/runs/123",
            ["parity_status"] = "dual-run-pass"
        };

        var result = RequiredCiMigrationLedger.Validate(root.ToJsonString(), RequiredCiMode.Active);

        Assert.Equal(627, result.CiPassCount);
        Assert.Equal(0, result.CiPendingCount);
        Assert.Equal("1111111111111111111111111111111111111111", result.ShadowSha);

        root["contracts"]![0]!["ci_parity"]!["evidence"] = null;
        var exception = Assert.Throws<VerificationException>(() =>
            RequiredCiMigrationLedger.Validate(root.ToJsonString(), RequiredCiMode.Active));
        Assert.Equal("required-ci-ledger:ci_parity=evidence", exception.Detail);
    }

    [Fact]
    public void ActiveWorkflowCannotRetainShadowLegacyStep()
    {
        var legacy = """
              - name: Run legacy packaging Pester
                shell: pwsh
                run: Invoke-Pester -Path packaging/windows-desktop-node/tests
        """;
        var yaml = ActiveWorkflow().Replace(
            "      - name: Run delivery shard",
            legacy + "\n      - name: Run delivery shard",
            StringComparison.Ordinal);

        AssertRejected(yaml, Catalog("active"), "required-ci-active=legacy-step");
    }

    private static VerificationCatalog Catalog(string activation)
    {
        var canonical = VerificationCatalogFixture.LoadCanonical();
        var migrationState = activation == "active" ? "cutover" : "mapped";
        return canonical with
        {
            ActivationState = activation,
            Suites = canonical.Suites
                .Select(suite => suite with { MigrationState = migrationState })
                .ToArray()
        };
    }

    private static void AssertRejected(
        string yaml,
        VerificationCatalog catalog,
        string expectedDetail)
    {
        var exception = Assert.Throws<VerificationException>(() =>
            RequiredCiPolicy.Validate(yaml, catalog));

        Assert.Equal(VerificationErrorCodes.ConfigInvalid, exception.Code);
        Assert.Equal(expectedDetail, exception.Detail);
    }

    private static string ShadowWorkflow(IReadOnlyList<string>? jobIds = null)
    {
        jobIds ??= ["dotnet-tests", "web-tests", "packaging-pester", "installer-web-pester"];
        var jobs = jobIds.Select(ShadowJob);
        return Header() + string.Join("", jobs);
    }

    private static string ShadowJob(string jobId)
    {
        if (jobId == "extra-job")
        {
            return "  extra-job:\n    runs-on: windows-latest\n    steps: []\n";
        }

        var shard = jobId switch
        {
            "dotnet-tests" => "dotnet",
            "web-tests" => "web",
            "packaging-pester" => "delivery",
            "installer-web-pester" => "installer-policy",
            _ => throw new ArgumentOutOfRangeException(nameof(jobId))
        };
        var runner = jobId == "web-tests" ? "ubuntu-latest" : "windows-latest";
        var setupNode = jobId == "web-tests" ? $"      - uses: {SetupNode}\n" : string.Empty;
        var legacy = jobId switch
        {
            "dotnet-tests" => """
                  - name: Run legacy dotnet
                    shell: cmd
                    run: dotnet test src/DesktopNode.sln -c Release
            """,
            "web-tests" => """
                  - name: Run legacy web
                    run: npm test --prefix web && npm run verify:parity --prefix web
            """,
            "packaging-pester" => """
                  - name: Run legacy packaging Pester
                    shell: pwsh
                    run: Install-Module Pester -RequiredVersion 5.7.1; Invoke-Pester -Path packaging/windows-desktop-node/tests
            """,
            "installer-web-pester" => """
                  - name: Run legacy installer and Web Pester
                    shell: pwsh
                    run: Install-Module Pester -RequiredVersion 5.7.1; Invoke-Pester -Path packaging/windows-desktop-node/installer/tests,web/tests
            """,
            _ => throw new ArgumentOutOfRangeException(nameof(jobId))
        };
        var names = jobId switch
        {
            "dotnet-tests" => ("legacy-dotnet", "replacement-dotnet"),
            "web-tests" => ("legacy-web", "replacement-web"),
            "packaging-pester" => ("legacy-packaging", "replacement-delivery"),
            "installer-web-pester" => ("legacy-installer-web", "replacement-installer-policy"),
            _ => throw new ArgumentOutOfRangeException(nameof(jobId))
        };

        return $"""
              {jobId}:
                runs-on: {runner}
                steps:
                  - uses: {Checkout}
                  - uses: {SetupDotNet}
            {setupNode}{legacy}
                  - name: Run replacement {shard}
                    shell: cmd
                    run: dotnet run --project src/DesktopNode.Verification -- verify --shard {shard}
                  - name: Upload {names.Item1}
                    uses: {UploadArtifact}
                    with:
                      name: {names.Item1}
                      path: artifacts/shadow/{shard}/legacy
                  - name: Upload {names.Item2}
                    uses: {UploadArtifact}
                    with:
                      name: {names.Item2}
                      path: artifacts/shadow/{shard}/replacement
            """ + "\n";
    }

    private static string ActiveWorkflow() =>
        Header() +
        ActiveJob("dotnet", "windows-latest", false) +
        ActiveJob("web", "ubuntu-latest", true) +
        ActiveJob("delivery", "windows-latest", false) +
        ActiveJob("installer-policy", "windows-latest", false);

    private static string ActiveJob(string jobId, string runner, bool setupNode)
    {
        var node = setupNode ? $"      - uses: {SetupNode}\n" : string.Empty;
        var shell = runner == "windows-latest" ? "        shell: cmd\n" : string.Empty;
        return $"""
              {jobId}:
                name: {jobId}
                runs-on: {runner}
                steps:
                  - uses: {Checkout}
                  - uses: {SetupDotNet}
            {node}      - name: Run {jobId} shard
            {shell}
                    run: dotnet run --project src/DesktopNode.Verification -- verify --shard {jobId}
            {ActiveArtifactStep(jobId)}
            """ + "\n";
    }

    private static string ActiveArtifactStep(string jobId) =>
        $"      - name: Upload {jobId}\n" +
        $"        uses: {UploadArtifact}\n" +
        "        with:\n" +
        $"          name: development-gates-{jobId}-" + "${{ github.run_id }}\n" +
        $"          path: artifacts/development-gates-{jobId}";

    private static string Header() => """
        name: Development Gates
        on:
          pull_request:
        permissions:
          contents: read
        jobs:
        """ + "\n";
}
