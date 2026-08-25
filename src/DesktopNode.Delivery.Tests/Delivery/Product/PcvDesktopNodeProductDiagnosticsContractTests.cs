using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Product;

[Trait("Category", "Delivery")]
public sealed class PcvDesktopNodeProductDiagnosticsContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.001",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        1,
        "redacts tokens and Authorization headers from diagnostic objects")]
    public void Contract001() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 1);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.002",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        2,
        "redacts bearer tokens inside diagnostic strings")]
    public void Contract002() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 2);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.003",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        3,
        "preserves null values while redacting diagnostic objects")]
    public void Contract003() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 3);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.004",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        4,
        "writes a diagnostic bundle without token file content")]
    public void Contract004() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 4);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.005",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        5,
        "includes service status and runtime policy artifacts")]
    public void Contract005() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 5);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.006",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        6,
        "redacts known root paths from diagnostic bundle artifacts")]
    public void Contract006() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 6);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.007",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        7,
        "runs CollectDiagnostics through the product action orchestrator")]
    public void Contract007() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 7);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.008",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        8,
        "includes redacted service host logs, status, and executable hash in diagnostics")]
    public void Contract008() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 8);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.009",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        9,
        "summarizes Phase 23 service recovery and log retention evidence without mutating the host")]
    public void Contract009() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 9);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.010",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        10,
        "uses installed root-level .NET host paths when collecting diagnostics from an MSI layout")]
    public void Contract010() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 10);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.011",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        11,
        "writes a versioned diagnostics manifest with redacted policy and source artifacts")]
    public void Contract011() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 11);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.012",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        12,
        "includes LAN security policy in diagnostic bundle manifest without enabling LAN")]
    public void Contract012() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 12);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.013",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        13,
        "self-audits the Phase 24 runtime policy contract in diagnostic bundles")]
    public void Contract013() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 13);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.014",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        14,
        "includes update policy and migration artifacts in diagnostic bundle manifest")]
    public void Contract014() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 14);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.015",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        15,
        "rotates JSONL and service logs according to diagnostics policy")]
    public void Contract015() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 15);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-diagnostics.016",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Diagnostics.Tests.ps1",
        16,
        "builds a default Windows Event Log registration plan without mutating the host")]
    public void Contract016() =>
        ProductDescriptorContractVerifier.Verify("diagnostics", 16);
}
