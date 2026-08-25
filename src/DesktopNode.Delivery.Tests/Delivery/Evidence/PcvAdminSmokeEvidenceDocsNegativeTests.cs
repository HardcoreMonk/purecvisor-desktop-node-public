using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

[Trait("Category", "VerificationInfrastructure")]
public sealed class PcvAdminSmokeEvidenceDocsNegativeTests
{
    private const string ErrorPrefix =
        "PCV_DELIVERY_CONTRACT_INVALID|" +
        PcvAdminSmokeEvidenceDocsVerifier.SpecPath +
        "|";

    [Fact]
    public void RejectsStaleCurrentAnchor()
    {
        var repository = RepositoryContractContext.Find();
        const string readmePath = "README.md";
        var currentRecord = repository.ReadUtf8Text("docs/ga-ready/current-evidence.json");
        using var record = JsonContract.Parse("docs/ga-ready/current-evidence.json", currentRecord);
        var current = record.RequireObject(record.Root, "current");
        var version = record.RequireString(current, "version");
        var readme = repository.ReadUtf8Text(readmePath);
        var stale = readme.Replace(
            version,
            "0.0.0-stale-anchor",
            StringComparison.Ordinal);

        Assert.NotEqual(readme, stale);
        var verifier = PcvAdminSmokeEvidenceDocsVerifier.Create(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [readmePath] = stale,
            });

        AssertInvalid(
            () => verifier.Verify(88),
            "current-version-README.md");
    }

    [Fact]
    public void RejectsLegacyShaDrift()
    {
        var repository = RepositoryContractContext.Find();
        var legacy = repository.ReadUtf8Text(PcvAdminSmokeEvidenceDocsVerifier.LegacyPath);
        var verifier = PcvAdminSmokeEvidenceDocsVerifier.Create(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [PcvAdminSmokeEvidenceDocsVerifier.LegacyPath] =
                    legacy + "\n# injected legacy drift\n",
            });

        AssertInvalid(() => verifier.Verify(1), "legacy-sha");
    }

    [Fact]
    public void RejectsWrongEvidenceShaLengthOrCase()
    {
        var repository = RepositoryContractContext.Find();
        const string evidencePath =
            "docs/ga-ready/evidence/" +
            "internal-clean-host-install-update-rollback-smoke-2026-05-10-0417.md";
        const string digest =
            "9b266867129cbf07abb8da7e2a26799d1221a16d955348505416810c48de12b1";
        var evidence = repository.ReadUtf8Text(evidencePath);

        foreach (var invalidDigest in new[] { digest[..^1], digest.ToUpperInvariant() })
        {
            var mutated = evidence.Replace(digest, invalidDigest, StringComparison.Ordinal);
            Assert.NotEqual(evidence, mutated);
            var verifier = PcvAdminSmokeEvidenceDocsVerifier.Create(
                new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    [evidencePath] = mutated,
                });

            AssertInvalid(
                () => verifier.Verify(1),
                "sha-format-baseline_msi_sha256");
        }
    }

    [Fact]
    public void RejectsMissingHistoricalPredecessorLabel()
    {
        var repository = RepositoryContractContext.Find();
        const string matrixPath =
            "docs/ga-ready/INTERNAL_PRIVATE_NETWORK_DISTRIBUTION_MATRIX.md";
        const string predecessor =
            "installed_tui_operator_smoke_scope: historical-predecessor-only";
        var matrix = repository.ReadUtf8Text(matrixPath);
        var missingLabel = matrix.Replace(
            predecessor,
            "installed_tui_operator_smoke_scope: current-only",
            StringComparison.Ordinal);

        Assert.NotEqual(matrix, missingLabel);
        var verifier = PcvAdminSmokeEvidenceDocsVerifier.Create(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [matrixPath] = missingLabel,
            });

        AssertInvalid(() => verifier.Verify(4), "regex-4-267");
    }

    [Fact]
    public void RejectsMissingHistoricalPredecessor()
    {
        const string predecessor =
            "docs/ga-ready/evidence/" +
            "public-boundary-ci-main-push-2026-07-13-pr171-postmerge-pass.md";
        var verifier = PcvAdminSmokeEvidenceDocsVerifier.Create(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [predecessor] = null,
            });

        AssertInvalid(() => verifier.Verify(90), "source-90-");
    }

    [Fact]
    public void RejectsFalsePublicSigningClaim()
    {
        var repository = RepositoryContractContext.Find();
        const string recordPath = "docs/ga-ready/current-evidence.json";
        var record = repository.ReadUtf8Text(recordPath);
        var falseClaim = record.Replace(
            "\"public_trusted_signing\": false",
            "\"public_trusted_signing\": true",
            StringComparison.Ordinal);

        Assert.NotEqual(record, falseClaim);
        var verifier = PcvAdminSmokeEvidenceDocsVerifier.Create(
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [recordPath] = falseClaim,
            });

        AssertInvalid(() => verifier.Verify(88), "current-public-signing");
    }

    private static void AssertInvalid(Action action, string detailPrefix)
    {
        var error = Assert.Throws<InvalidDataException>(action);
        Assert.StartsWith(ErrorPrefix + detailPrefix, error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(
            RepositoryContractContext.Find().RootPath,
            error.Message,
            StringComparison.OrdinalIgnoreCase);
    }
}
