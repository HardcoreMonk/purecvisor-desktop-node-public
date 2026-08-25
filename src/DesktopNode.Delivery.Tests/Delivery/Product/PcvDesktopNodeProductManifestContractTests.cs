using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Product;

[Trait("Category", "Delivery")]
public sealed class PcvDesktopNodeProductManifestContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.001",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        1,
        "builds a product manifest with product-owned asset files only")]
    public void Contract001() =>
        ProductDescriptorContractVerifier.Verify("manifest", 1);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.002",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        2,
        "records the CLI and Web-only schema v2 contract without TUI metadata")]
    public void Contract002() =>
        ProductDescriptorContractVerifier.Verify("manifest", 2);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.003",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        3,
        "reads migrated product manifest schema v2 for update and rollback compatibility")]
    public void Contract003() =>
        ProductDescriptorContractVerifier.Verify("manifest", 3);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.004",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        4,
        "does not expose spikes paths as standalone product asset sources")]
    public void Contract004() =>
        ProductDescriptorContractVerifier.Verify("manifest", 4);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.005",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        5,
        "excludes source tests directories from manifest and copied product assets")]
    public void Contract005() =>
        ProductDescriptorContractVerifier.Verify("manifest", 5);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.006",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        6,
        "uses terminating errors for product asset filesystem mutations")]
    public void Contract006() =>
        ProductDescriptorContractVerifier.Verify("manifest", 6);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.007",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        7,
        "copies product assets and writes a product manifest")]
    public void Contract007() =>
        ProductDescriptorContractVerifier.Verify("manifest", 7);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.008",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        8,
        "copies packaged runtime payload files when the source root is an MSI payload")]
    public void Contract008() =>
        ProductDescriptorContractVerifier.Verify("manifest", 8);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.009",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        9,
        "blocks partial root runtime payload copy when the CLI executable is missing")]
    public void Contract009() =>
        ProductDescriptorContractVerifier.Verify("manifest", 9);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.010",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        10,
        "does not stage WinSW executable or XML into the product root")]
    public void Contract010() =>
        ProductDescriptorContractVerifier.Verify("manifest", 10);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.011",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        11,
        "records .NET service host metadata in product-manifest.json")]
    public void Contract011() =>
        ProductDescriptorContractVerifier.Verify("manifest", 11);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.012",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        12,
        "records active .NET CLI metadata in product-manifest.json")]
    public void Contract012() =>
        ProductDescriptorContractVerifier.Verify("manifest", 12);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.013",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        13,
        "records diagnostics policy v1 in product-manifest.json")]
    public void Contract013() =>
        ProductDescriptorContractVerifier.Verify("manifest", 13);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.014",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        14,
        "records LAN security policy v1 in product-manifest.json")]
    public void Contract014() =>
        ProductDescriptorContractVerifier.Verify("manifest", 14);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.015",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        15,
        "records ProgramData ACL ownership policy in product-manifest.json")]
    public void Contract015() =>
        ProductDescriptorContractVerifier.Verify("manifest", 15);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-manifest.016",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Manifest.Tests.ps1",
        16,
        "records update policy v1 in product-manifest.json")]
    public void Contract016() =>
        ProductDescriptorContractVerifier.Verify("manifest", 16);
}
