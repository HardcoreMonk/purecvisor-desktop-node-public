using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class VmExportImportPolicyTests
{
    [Fact]
    public void ExportPreviewAcceptsManagedGen2OffVmInsideAllowlist()
    {
        var result = VmExportImportPolicy.EvaluateExportPreview(ValidExport());

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Equal("lab-vm", result.VmName);
        Assert.Equal(VmExportImportPolicy.PackageHyperVExport, result.PackageKind);
        Assert.False(result.GenerateNewId);
        Assert.True(result.ApplyManagedMarker);
        Assert.Equal(VmExportImportPolicy.ActionExportPreview, result.Action);
        Assert.StartsWith(ValidRoot(), result.Directory, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExportAcceptsServiceBearerAndTrimsVmName()
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with
        {
            VmName = "  lab-vm  ",
            ConfirmName = "lab-vm",
            Auth = new VmExportImportAuthContext(HasServiceBearer: true)
        });

        Assert.True(result.Ok);
        Assert.Equal("lab-vm", result.VmName);
        Assert.Equal(VmExportImportPolicy.ActionExport, result.Action);
    }

    [Theory]
    [InlineData("export-preview")]
    [InlineData("export")]
    [InlineData("import-preview")]
    [InlineData("import")]
    public void RejectsMissingOperateBeforePathValidation(string operation)
    {
        var result = operation.StartsWith("import", StringComparison.Ordinal)
            ? EvaluateImport(operation, ValidImport() with
            {
                Auth = new VmExportImportAuthContext(),
                TargetName = null,
                Directory = null
            })
            : EvaluateExport(operation, ValidExport() with
            {
                Auth = new VmExportImportAuthContext(),
                VmName = null,
                Directory = null
            });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.Forbidden, result.ErrorCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExportRejectsMissingVmName(string? vmName)
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with { VmName = vmName });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.VmRequired, result.ErrorCode);
    }

    [Fact]
    public void ExportRejectsConfirmationMismatchBeforeUnmanaged()
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with
        {
            ConfirmName = "other-vm",
            Managed = false
        });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.ConfirmationMismatch, result.ErrorCode);
    }

    [Fact]
    public void ExportRejectsUnmanagedVm()
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with { Managed = false });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.NotManaged, result.ErrorCode);
    }

    [Fact]
    public void ExportAcceptsLowercaseOffPowerState()
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with { PowerState = "off" });

        Assert.True(result.Ok);
        Assert.Equal(VmExportImportPolicy.ActionExport, result.Action);
    }

    [Fact]
    public void ExportRejectsGenerationOne()
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with { Generation = 1 });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.GenerationUnsupported, result.ErrorCode);
    }

    [Theory]
    [InlineData("Running")]
    [InlineData("Saved")]
    [InlineData("Paused")]
    public void ExportRejectsNonOffPowerState(string powerState)
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with { PowerState = powerState });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.SourceNotOff, result.ErrorCode);
    }

    [Fact]
    public void ExportRejectsTpmAndKeyProtector()
    {
        var result = VmExportImportPolicy.EvaluateExport(ValidExport() with { SecurityFeaturesPresent = true });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.SecurityFeaturesUnsupported, result.ErrorCode);
    }

    [Fact]
    public void ExportRejectsUncAndRootEscape()
    {
        var unc = VmExportImportPolicy.EvaluateExport(ValidExport() with
        {
            Directory = @"\\server\share\lab-vm"
        });
        Assert.False(unc.Ok);
        Assert.Equal(VmExportImportProblemCodes.PathNotAllowed, unc.ErrorCode);

        var escaped = VmExportImportPolicy.EvaluateExport(ValidExport() with
        {
            Directory = Path.GetFullPath(Path.Combine(ValidRoot(), "..", "outside"))
        });
        Assert.False(escaped.Ok);
        Assert.Equal(VmExportImportProblemCodes.PathNotAllowed, escaped.ErrorCode);
    }

    [Fact]
    public void ImportPreviewAcceptsHyperVExportWithNewIdAndMarker()
    {
        var result = VmExportImportPolicy.EvaluateImportPreview(ValidImport());

        Assert.True(result.Ok);
        Assert.Equal("lab-vm-restored", result.VmName);
        Assert.Equal(VmExportImportPolicy.PackageHyperVExport, result.PackageKind);
        Assert.True(result.GenerateNewId);
        Assert.True(result.ApplyManagedMarker);
        Assert.Equal(VmExportImportPolicy.ActionImportPreview, result.Action);
    }

    [Fact]
    public void ImportRejectsOvfBeforePathCheck()
    {
        var result = VmExportImportPolicy.EvaluateImport(ValidImport() with
        {
            PackageKind = "ovf",
            Directory = null
        });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.OvfForbidden, result.ErrorCode);
    }

    [Fact]
    public void ImportRejectsMissingVmcxAsInvalidPackage()
    {
        var result = VmExportImportPolicy.EvaluateImport(ValidImport() with { HasVmcx = false });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.PackageInvalid, result.ErrorCode);
    }

    [Fact]
    public void ImportRejectsSecurityFeaturesAndInPlaceAndExistingName()
    {
        Assert.Equal(
            VmExportImportProblemCodes.ImportSecurityFeaturesUnsupported,
            VmExportImportPolicy.EvaluateImport(ValidImport() with { SecurityFeaturesPresent = true }).ErrorCode);
        Assert.Equal(
            VmExportImportProblemCodes.InPlaceForbidden,
            VmExportImportPolicy.EvaluateImport(ValidImport() with { GenerateNewId = false }).ErrorCode);
        Assert.Equal(
            VmExportImportProblemCodes.AlreadyExists,
            VmExportImportPolicy.EvaluateImport(ValidImport() with { TargetExists = true }).ErrorCode);
    }

    [Fact]
    public void ImportRejectsReservedTargetName()
    {
        var result = VmExportImportPolicy.EvaluateImport(ValidImport() with
        {
            TargetName = "..",
            ConfirmName = ".."
        });

        Assert.False(result.Ok);
        Assert.Equal(VmExportImportProblemCodes.NameInvalid, result.ErrorCode);
    }

    [Fact]
    public void BoundsMatchTheDesignContract()
    {
        Assert.Equal("pcv-vm-export-import-v1", VmExportImportPolicy.Schema);
        Assert.Equal("operate", VmExportImportPolicy.PermissionOperate);
        Assert.Equal(2, VmExportImportPolicy.RequiredGeneration);
        Assert.Equal("Off", VmExportImportPolicy.RequiredPowerState);
        Assert.Equal("hyperv-export", VmExportImportPolicy.PackageHyperVExport);
        Assert.EndsWith(
            Path.Combine("PureCVisor", "desktop-node", "exports"),
            VmExportImportPolicy.DefaultExportRoot,
            StringComparison.OrdinalIgnoreCase);
    }

    private static VmExportImportEvaluation EvaluateExport(string operation, VmExportRequest request)
    {
        return operation == "export-preview"
            ? VmExportImportPolicy.EvaluateExportPreview(request)
            : VmExportImportPolicy.EvaluateExport(request);
    }

    private static VmExportImportEvaluation EvaluateImport(string operation, VmImportRequest request)
    {
        return operation == "import-preview"
            ? VmExportImportPolicy.EvaluateImportPreview(request)
            : VmExportImportPolicy.EvaluateImport(request);
    }

    private static string ValidRoot()
    {
        return Path.GetFullPath(Path.Combine(Path.GetTempPath(), "pcv-export-allowlist"));
    }

    private static VmExportRequest ValidExport()
    {
        var root = ValidRoot();
        return new VmExportRequest(
            "lab-vm",
            "lab-vm",
            Path.Combine(root, "lab-vm"),
            root,
            new VmExportImportAuthContext(HasOperate: true),
            Managed: true,
            Generation: 2,
            PowerState: "Off");
    }

    private static VmImportRequest ValidImport()
    {
        var root = ValidRoot();
        return new VmImportRequest(
            "lab-vm-restored",
            "lab-vm-restored",
            Path.Combine(root, "lab-vm"),
            root,
            new VmExportImportAuthContext(HasOperate: true),
            PackageKind: VmExportImportPolicy.PackageHyperVExport,
            HasVmcx: true,
            GenerateNewId: true);
    }
}
