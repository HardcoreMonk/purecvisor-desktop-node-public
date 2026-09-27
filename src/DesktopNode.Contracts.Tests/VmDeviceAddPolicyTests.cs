using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class VmDeviceAddPolicyTests
{
    [Fact]
    public void PreviewAcceptsOneAdditionalNicOnAnExistingSwitch()
    {
        var result = VmDeviceAddPolicy.EvaluatePreview(ValidNic());

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Equal("lab-vm", result.VmName);
        Assert.Equal(VmDeviceAddPolicy.KindNic, result.DeviceKind);
        Assert.Equal("Default Switch", result.SwitchName);
        Assert.Equal(1, result.Quantity);
        Assert.Equal(VmDeviceAddPolicy.ActionPreview, result.Action);
    }

    [Fact]
    public void AddAcceptsServiceBearerAndOneMissingDvd()
    {
        var result = VmDeviceAddPolicy.EvaluateAdd(ValidDvd() with
        {
            VmName = "  lab-vm  ",
            DeviceKind = "DVD",
            Auth = new VmDeviceAddAuthContext(HasServiceBearer: true)
        });

        Assert.True(result.Ok);
        Assert.Equal("lab-vm", result.VmName);
        Assert.Equal(VmDeviceAddPolicy.KindDvd, result.DeviceKind);
        Assert.Null(result.SwitchName);
        Assert.Equal(VmDeviceAddPolicy.ActionAdd, result.Action);
        Assert.True(VmDeviceAddPolicy.EvaluateAdd(ValidDvd() with { PowerState = "stopped" }).Ok);
    }

    [Theory]
    [InlineData("preview")]
    [InlineData("add")]
    public void AuthFailsBeforeDeviceKind(string operation)
    {
        var request = ValidNic() with
        {
            DeviceKind = "usb",
            Auth = new VmDeviceAddAuthContext()
        };
        var result = operation == "preview"
            ? VmDeviceAddPolicy.EvaluatePreview(request)
            : VmDeviceAddPolicy.EvaluateAdd(request);

        Assert.False(result.Ok);
        Assert.Equal(VmDeviceAddProblemCodes.Forbidden, result.ErrorCode);
    }

    [Fact]
    public void RejectsDeviceShopKindsAndMultipleQuantity()
    {
        Assert.Equal(
            VmDeviceAddProblemCodes.KindUnsupported,
            VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { DeviceKind = "usb" }).ErrorCode);
        Assert.Equal(
            VmDeviceAddProblemCodes.KindUnsupported,
            VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { DeviceKind = "3d" }).ErrorCode);
        Assert.Equal(
            VmDeviceAddProblemCodes.QuantityInvalid,
            VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { Quantity = 2 }).ErrorCode);
    }

    [Fact]
    public void RejectsUnpreparedVmBeforeDeviceRules()
    {
        Assert.Equal(VmDeviceAddProblemCodes.VmRequired, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { VmName = " " }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.NotManaged, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { Managed = false }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.TemplateLocked, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { TemplateLocked = true }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.SourceNotOff, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { PowerState = "Running" }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.GenerationUnsupported, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { Generation = 1 }).ErrorCode);
    }

    [Fact]
    public void NicRejectsNatDhcpMissingSwitchAndSecondExtraNic()
    {
        Assert.Equal(VmDeviceAddProblemCodes.NatForbidden, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { NatEnabled = true }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.DhcpForbidden, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { DhcpEnabled = true }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.SwitchRequired, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { SwitchName = null }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.SwitchNotFound, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { SwitchExists = false }).ErrorCode);
        Assert.Equal(VmDeviceAddProblemCodes.Limit, VmDeviceAddPolicy.EvaluateAdd(ValidNic() with { ExistingNicCount = VmDeviceAddPolicy.MaxNicCount }).ErrorCode);
    }

    [Fact]
    public void DvdRejectsIsoAttachAndASecondDrive()
    {
        Assert.Equal(VmDeviceAddProblemCodes.IsoForbidden, VmDeviceAddPolicy.EvaluateAdd(ValidDvd() with { IsoRequested = true }).ErrorCode);
        Assert.Equal(
            VmDeviceAddProblemCodes.AlreadyPresent,
            VmDeviceAddPolicy.EvaluateAdd(ValidDvd() with { ExistingDvdCount = VmDeviceAddPolicy.MaxDvdCount }).ErrorCode);
    }

    private static VmDeviceAddRequest ValidNic()
    {
        return new VmDeviceAddRequest(
            "lab-vm",
            "nic",
            new VmDeviceAddAuthContext(HasOperate: true),
            Quantity: 1,
            Managed: true,
            PowerState: "off",
            Generation: 2,
            ExistingNicCount: 1,
            SwitchName: " Default Switch ",
            SwitchExists: true);
    }

    private static VmDeviceAddRequest ValidDvd()
    {
        return new VmDeviceAddRequest(
            "lab-vm",
            "dvd",
            new VmDeviceAddAuthContext(HasOperate: true),
            Quantity: 1,
            Managed: true,
            PowerState: "Off",
            Generation: 2,
            ExistingDvdCount: 0);
    }
}
