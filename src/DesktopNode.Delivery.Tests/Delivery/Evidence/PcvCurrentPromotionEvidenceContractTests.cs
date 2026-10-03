namespace DesktopNode.Delivery.Tests.Delivery.Evidence;

// Replaces the per-version Pcv04277/Pcv04278 promotion tests: the current and immediate
// predecessor checks read their expectations from current-evidence.json and the ledger head
// chain, so a promotion no longer edits C#.
[Trait("Category", "Delivery")]
public sealed class PcvCurrentPromotionEvidenceContractTests
{
    [Fact]
    public void CurrentPromotionAgreesAcrossDescriptorIndexesAndLedger()
    {
        D2EvidenceContractVerifier.VerifyCurrentPromotion();
    }

    [Fact]
    public void ImmediatePredecessorStaysOnTheChainAndLedger()
    {
        D2EvidenceContractVerifier.VerifyImmediatePredecessor();
    }

    [Fact]
    public void DescriptorExpectationsComeFromTheRecordValues()
    {
        var expected = D2EvidenceContractVerifier.ExpectedCurrentDescriptorMetadata(
            "0.42.77-admin-smoke",
            "0.42.78-admin-smoke",
            "manual-admin-campaign-descriptor-20260927-04277-04278",
            "clean-sha",
            "full-admin-host-mutation-gate-20260927-04278-r2",
            "provenance");

        Assert.Equal(5, expected.Count);
        Assert.Equal("0.42.77-admin-smoke -> 0.42.78-admin-smoke", expected["current_manual_admin_package_pair"]);
        Assert.Equal("manual-admin-campaign-descriptor-20260927-04277-04278", expected["current_manual_admin_descriptor_batch_manifest"]);
        Assert.Equal("clean-sha", expected["current_manual_admin_target_msi_sha256"]);
        Assert.Equal("full-admin-host-mutation-gate-20260927-04278-r2", expected["current_full_admin_host_mutation_batch"]);
        Assert.Equal("provenance", expected["current_full_admin_host_mutation_provenance_commit"]);
    }

    [Theory]
    [InlineData("0.42.78-admin-smoke", "04278")]
    [InlineData("0.42.5-admin-smoke", "04205")]
    public void VersionTagJoinsTheVersionParts(string version, string tag)
    {
        Assert.Equal(tag, D2EvidenceContractVerifier.VersionTag(version));
    }

    [Fact]
    public void VersionTagRejectsANonAdminSmokeVersion()
    {
        Assert.ThrowsAny<Exception>(() => D2EvidenceContractVerifier.VersionTag("0.42.78"));
    }

    [Fact]
    public void ReadsTheImmediatePredecessorFromTheLineBelowTheCurrentKey()
    {
        const string ledger =
            "ledger_id: `l`\r\n" +
            "current_full_admin_host_mutation: `0.42.78-admin-smoke`\r\n" +
            "previous_04277_current_full_admin_host_mutation: `0.42.77-admin-smoke`\r\n" +
            "previous_04275_current_full_admin_host_mutation: `0.42.75-admin-smoke`\r\n";

        var (tag, version) = D2EvidenceContractVerifier.ReadImmediatePredecessor(ledger);

        Assert.Equal("04277", tag);
        Assert.Equal("0.42.77-admin-smoke", version);
    }

    [Theory]
    [InlineData("ledger_id: `l`\n")]
    [InlineData("current_full_admin_host_mutation: `0.42.78-admin-smoke`\nledger_id: `l`\n")]
    [InlineData("current_full_admin_host_mutation: `0.42.78-admin-smoke`")]
    public void RejectsALedgerWithoutTheChainLine(string ledger)
    {
        Assert.ThrowsAny<Exception>(() => D2EvidenceContractVerifier.ReadImmediatePredecessor(ledger));
    }
}
