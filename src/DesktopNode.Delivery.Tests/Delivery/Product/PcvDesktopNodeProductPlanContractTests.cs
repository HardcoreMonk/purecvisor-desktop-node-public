using DesktopNode.Delivery.Tests.Contracts;

namespace DesktopNode.Delivery.Tests.Delivery.Product;

[Trait("Category", "Delivery")]
public sealed class PcvDesktopNodeProductPlanContractTests
{
    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.001",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        1,
        "returns product defaults")]
    public void Contract001() =>
        ProductDescriptorContractVerifier.Verify("plan", 1);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.002",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        2,
        "resolves CLI and Web product paths without a TUI executable")]
    public void Contract002() =>
        ProductDescriptorContractVerifier.Verify("plan", 2);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.003",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        3,
        "computes product hashes without the Get-FileHash cmdlet")]
    public void Contract003() =>
        ProductDescriptorContractVerifier.Verify("plan", 3);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.004",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        4,
        "builds an install product plan with file auth, assets, and service commands")]
    public void Contract004() =>
        ProductDescriptorContractVerifier.Verify("plan", 4);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.005",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        5,
        "includes optional batch evidence root in the product service host arguments")]
    public void Contract005() =>
        ProductDescriptorContractVerifier.Verify("plan", 5);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.006",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        6,
        "normalizes relative batch evidence root against SourceRoot before writing service arguments")]
    public void Contract006() =>
        ProductDescriptorContractVerifier.Verify("plan", 6);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.007",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        7,
        "records network download update source gate inputs without resolving them in the plan")]
    public void Contract007() =>
        ProductDescriptorContractVerifier.Verify("plan", 7);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.008",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        8,
        "records full updater catalog channel inputs without resolving them in the plan")]
    public void Contract008() =>
        ProductDescriptorContractVerifier.Verify("plan", 8);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.009",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        9,
        "includes remove-data delete paths for uninstall")]
    public void Contract009() =>
        ProductDescriptorContractVerifier.Verify("plan", 9);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.010",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        10,
        "rejects inline API tokens")]
    public void Contract010() =>
        ProductDescriptorContractVerifier.Verify("plan", 10);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.011",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        11,
        "builds a .NET Windows service plan with stable paths and command names")]
    public void Contract011() =>
        ProductDescriptorContractVerifier.Verify("plan", 11);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.012",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        12,
        "does not generate WinSW XML for the default .NET service host")]
    public void Contract012() =>
        ProductDescriptorContractVerifier.Verify("plan", 12);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.013",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        13,
        "rejects retired WinSW PowerShell Local API generation functions")]
    public void Contract013() =>
        ProductDescriptorContractVerifier.Verify("plan", 13);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.014",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        14,
        "does not require a WinSW artifact for the .NET service host plan")]
    public void Contract014() =>
        ProductDescriptorContractVerifier.Verify("plan", 14);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.015",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        15,
        "keeps route parity admin smoke aligned with remove-data handoff and data-root gate")]
    public void Contract015() =>
        ProductDescriptorContractVerifier.Verify("plan", 15);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.016",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        16,
        "reads installed protected tokens in route parity smoke without importing the spike service module")]
    public void Contract016() =>
        ProductDescriptorContractVerifier.Verify("plan", 16);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.017",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        17,
        "covers protected token round trip in the route parity smoke self-test")]
    public void Contract017() =>
        ProductDescriptorContractVerifier.Verify("plan", 17);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.018",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        18,
        "checks the Web Console root on the split web port in route parity health")]
    public void Contract018() =>
        ProductDescriptorContractVerifier.Verify("plan", 18);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.019",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        19,
        "records partial MSI lifecycle evidence and classifies repair retry transients")]
    public void Contract019() =>
        ProductDescriptorContractVerifier.Verify("plan", 19);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.020",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        20,
        "prepares product protected tokens without the spike service module")]
    public void Contract020() =>
        ProductDescriptorContractVerifier.Verify("plan", 20);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.021",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        21,
        "marks ConfigureInstalled as elevated service configuration without file copy delete paths")]
    public void Contract021() =>
        ProductDescriptorContractVerifier.Verify("plan", 21);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.022",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        22,
        "supports MSI installed payload root as SourceRoot for ConfigureInstalled")]
    public void Contract022() =>
        ProductDescriptorContractVerifier.Verify("plan", 22);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.023",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        23,
        "uses explicit root-level .NET host executable for installed MSI Status")]
    public void Contract023() =>
        ProductDescriptorContractVerifier.Verify("plan", 23);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.024",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        24,
        "marks RepairInstalled as elevated and preserves token, jobs, events, and diagnostics")]
    public void Contract024() =>
        ProductDescriptorContractVerifier.Verify("plan", 24);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.025",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        25,
        "keeps RemoveInstalled default uninstall data-preserving and product-root neutral")]
    public void Contract025() =>
        ProductDescriptorContractVerifier.Verify("plan", 25);

    [PcvLegacyContract(
        "pcv.delivery.desktop-node-product-plan.026",
        "packaging/windows-desktop-node/tests/PcvDesktopNodeProduct.Plan.Tests.ps1",
        26,
        "lists only ProgramData paths for RemoveInstalled -RemoveData")]
    public void Contract026() =>
        ProductDescriptorContractVerifier.Verify("plan", 26);
}
