using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Delivery.Product;

[Trait("Category", "VerificationInfrastructure")]
public sealed class ProductInvokeNegativeParityTests
{
    private static readonly RepositoryContractContext Repository =
        RepositoryContractContext.Find();

    [Fact]
    public void RejectsArgumentInjectionInACommandArray()
    {
        var error = Assert.Throws<InvalidDataException>(() =>
            ProductInvokeContractVerifier.ValidateCommandPlan([
                new ProductCommandContract(
                    "sc.exe",
                    ["query", "PureCVisor" + Environment.NewLine + "unexpected"]),
            ]));

        Assert.Equal("PCV_DELIVERY_PRODUCT_INVOKE_INVALID|argument-injection", error.Message);
    }

    [Fact]
    public void RejectsAMissingProductActionRoute()
    {
        var module = Repository.ReadUtf8Text(ProductInvokeContractVerifier.ModulePath)
            .Replace(
                "function Invoke-PcvDesktopNodeProductAction",
                "function Invoke-PcvDesktopNodeProductActioX",
                StringComparison.Ordinal);

        var error = Assert.Throws<InvalidDataException>(() =>
            Create(module: module).VerifyContract(1));

        Assert.Equal("PCV_DELIVERY_PRODUCT_INVOKE_INVALID|missing-route", error.Message);
    }

    [Fact]
    public void RejectsADuplicateEntrypointAction()
    {
        var entrypoint = Repository.ReadUtf8Text(ProductInvokeContractVerifier.EntrypointPath)
            .Replace(
                "        'Plan',",
                "        'Plan'," + Environment.NewLine + "        'Plan',",
                StringComparison.Ordinal);

        var error = Assert.Throws<InvalidDataException>(() =>
            Create(entrypoint: entrypoint).VerifyContract(1));

        Assert.Equal("PCV_DELIVERY_PRODUCT_INVOKE_INVALID|entrypoint-actions", error.Message);
    }

    [Fact]
    public void RejectsAnUnredactedBearerProjection()
    {
        var safeMarker = "Bearer [REDACTED]";
        var unsafeMarker = "Bearer [" + "VISIBLE]";
        var module = Repository.ReadUtf8Text(ProductInvokeContractVerifier.ModulePath)
            .Replace(safeMarker, unsafeMarker, StringComparison.Ordinal);

        var error = Assert.Throws<InvalidDataException>(() =>
            Create(module: module).VerifyContract(43));

        Assert.Equal("PCV_DELIVERY_PRODUCT_INVOKE_INVALID|redaction-bearer", error.Message);
    }

    [Fact]
    public void RejectsAnExecutableMutationCommand()
    {
        var mutation = "Invoke" + "-Expression";
        var module = Repository.ReadUtf8Text(ProductInvokeContractVerifier.ModulePath) +
            Environment.NewLine + mutation + " $command";

        Assert.Throws<InvalidDataException>(() =>
            Create(module: module).VerifyContract(8));
        Assert.Throws<InvalidDataException>(() =>
            ProductInvokeContractVerifier.ValidateCommandPlan([
                new ProductCommandContract(mutation, ["$command"]),
            ]));
    }

    private static ProductInvokeContractVerifier Create(
        string? entrypoint = null,
        string? module = null)
    {
        var overrides = new Dictionary<string, string?>(StringComparer.Ordinal);
        if (entrypoint is not null)
        {
            overrides[ProductInvokeContractVerifier.EntrypointPath] = entrypoint;
        }

        if (module is not null)
        {
            overrides[ProductInvokeContractVerifier.ModulePath] = module;
        }

        return ProductInvokeContractVerifier.Create(overrides, enforceSourceHashes: false);
    }
}
