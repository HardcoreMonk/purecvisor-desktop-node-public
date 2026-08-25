namespace DesktopNode.Delivery.Tests.Delivery.Product;

[Trait("Category", "VerificationInfrastructure")]
public sealed class ProductDescriptorNegativeParityTests
{
    [Fact]
    public void AcceptsTheCanonicalManifestEntryOrder()
    {
        ProductDescriptorContractVerifier.ValidateManifestEntries(
            ProductDescriptorContractVerifier.CanonicalManifest());
    }

    [Fact]
    public void RejectsAMissingManifestEntry()
    {
        var entries = ProductDescriptorContractVerifier.CanonicalManifest().Skip(1).ToArray();

        var error = Assert.Throws<InvalidDataException>(() =>
            ProductDescriptorContractVerifier.ValidateManifestEntries(entries));

        Assert.Equal(
            "PCV_DELIVERY_PRODUCT_DESCRIPTOR_INVALID|manifest-cardinality",
            error.Message);
    }

    [Fact]
    public void RejectsADuplicateManifestEntry()
    {
        var entries = ProductDescriptorContractVerifier.CanonicalManifest().ToArray();
        entries[1] = entries[0];

        var error = Assert.Throws<InvalidDataException>(() =>
            ProductDescriptorContractVerifier.ValidateManifestEntries(entries));

        Assert.Equal(
            "PCV_DELIVERY_PRODUCT_DESCRIPTOR_INVALID|manifest-cardinality",
            error.Message);
    }

    [Fact]
    public void RejectsAPlanPathOutsideItsOwnedRoot()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ProductDescriptorContractVerifier.ValidatePlanPath(
                @"C:\ProgramData\PureCVisor\DesktopNode",
                @"C:\ProgramData\PureCVisor\Outside\token.json"));

        Assert.Equal("PCV_DELIVERY_PRODUCT_DESCRIPTOR_INVALID|plan-path", error.Message);
    }

    [Fact]
    public void RejectsADiagnosticSensitiveKey()
    {
        var projection = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["authorization"] = "[REDACTED]",
        };

        var error = Assert.Throws<InvalidDataException>(() =>
            ProductDescriptorContractVerifier.ValidateDiagnosticProjection(projection));

        Assert.Equal(
            "PCV_DELIVERY_PRODUCT_DESCRIPTOR_INVALID|diagnostics-sensitive-key",
            error.Message);
    }

    [Fact]
    public void RejectsAnUnredactedDiagnosticBearerValue()
    {
        var projection = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["message"] = "request used Bearer visible-token",
        };

        var error = Assert.Throws<InvalidDataException>(() =>
            ProductDescriptorContractVerifier.ValidateDiagnosticProjection(projection));

        Assert.Equal(
            "PCV_DELIVERY_PRODUCT_DESCRIPTOR_INVALID|diagnostics-leakage",
            error.Message);
    }
}
