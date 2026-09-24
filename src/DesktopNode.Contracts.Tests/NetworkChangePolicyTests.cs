using DesktopNode.Contracts;

namespace DesktopNode.Contracts.Tests;

public sealed class NetworkChangePolicyTests
{
    [Fact]
    public void SwitchCreatePreviewAcceptsProductInternalWithAdmin()
    {
        var result = NetworkChangePolicy.EvaluateSwitchCreatePreview(ValidCreate());

        Assert.True(result.Ok);
        Assert.Null(result.ErrorCode);
        Assert.Equal("pcv-lab-internal", result.SwitchName);
        Assert.Equal(NetworkChangePolicy.TypeInternal, result.SwitchType);
        Assert.True(result.AllowManagementOs);
        Assert.Equal(NetworkChangePolicy.ActionSwitchCreatePreview, result.Action);
    }

    [Fact]
    public void SwitchCreateAcceptsServiceBearerAndPrivateSwitch()
    {
        var result = NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with
        {
            SwitchName = "  pcv-lab-private  ",
            SwitchType = "Private",
            AllowManagementOs = false,
            Auth = new NetworkChangeAuthContext(HasServiceBearer: true)
        });

        Assert.True(result.Ok);
        Assert.Equal("pcv-lab-private", result.SwitchName);
        Assert.Equal(NetworkChangePolicy.TypePrivate, result.SwitchType);
        Assert.False(result.AllowManagementOs);
        Assert.Equal(NetworkChangePolicy.ActionSwitchCreate, result.Action);
    }

    [Theory]
    [InlineData("create-preview")]
    [InlineData("create")]
    [InlineData("remove")]
    public void SwitchMutationRejectsOperateBeforeNameValidation(string operation)
    {
        var auth = new NetworkChangeAuthContext(HasOperate: true);
        var result = operation switch
        {
            "create-preview" => NetworkChangePolicy.EvaluateSwitchCreatePreview(ValidCreate() with
            {
                SwitchName = null,
                Auth = auth
            }),
            "create" => NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with
            {
                SwitchName = null,
                Auth = auth
            }),
            _ => NetworkChangePolicy.EvaluateSwitchRemove(ValidRemove() with
            {
                SwitchName = null,
                Auth = auth
            })
        };

        Assert.False(result.Ok);
        Assert.Equal(NetworkChangeProblemCodes.Forbidden, result.ErrorCode);
    }

    [Fact]
    public void SwitchCreateRejectsReservedAndNonProductNames()
    {
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchNameReserved,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { SwitchName = "Default Switch" }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchNameReserved,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { SwitchName = "WSL (Hyper-V firewall)" }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchNameNotProduct,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { SwitchName = "lab-internal" }).ErrorCode);
    }

    [Fact]
    public void SwitchCreateRejectsExternalNatDhcpAndWrongManagementOs()
    {
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchTypeUnsupported,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { SwitchType = "external" }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.NatForbidden,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { NatEnabled = true }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.DhcpForbidden,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { DhcpEnabled = true }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.ManagementOsInvalid,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { AllowManagementOs = false }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.ManagementOsInvalid,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with
            {
                SwitchType = "private",
                AllowManagementOs = true
            }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.AlreadyExists,
            NetworkChangePolicy.EvaluateSwitchCreate(ValidCreate() with { Exists = true }).ErrorCode);
    }

    [Fact]
    public void SwitchRemoveAcceptsUnusedProductSwitch()
    {
        var result = NetworkChangePolicy.EvaluateSwitchRemove(ValidRemove());

        Assert.True(result.Ok);
        Assert.Equal("pcv-lab-internal", result.SwitchName);
        Assert.Equal(NetworkChangePolicy.ActionSwitchRemove, result.Action);
    }

    [Fact]
    public void SwitchRemoveRejectsMissingInUseAndReserved()
    {
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchNotFound,
            NetworkChangePolicy.EvaluateSwitchRemove(ValidRemove() with { Exists = false }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchInUse,
            NetworkChangePolicy.EvaluateSwitchRemove(ValidRemove() with { AttachedVmCount = 1 }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchNameReserved,
            NetworkChangePolicy.EvaluateSwitchRemove(ValidRemove() with { SwitchName = "Default Switch" }).ErrorCode);
    }

    [Fact]
    public void VmConnectPreviewAcceptsManagedOffVmAndExistingSwitch()
    {
        var result = NetworkChangePolicy.EvaluateVmConnectPreview(ValidConnect());

        Assert.True(result.Ok);
        Assert.Equal("lab-vm", result.VmName);
        Assert.Equal("pcv-lab-internal", result.SwitchName);
        Assert.Equal(NetworkChangePolicy.ActionVmConnectPreview, result.Action);
    }

    [Fact]
    public void VmConnectAcceptsDefaultSwitchAndOffIgnoreCase()
    {
        var result = NetworkChangePolicy.EvaluateVmConnect(ValidConnect() with
        {
            SwitchName = "Default Switch",
            PowerState = "off",
            Auth = new NetworkChangeAuthContext(HasServiceBearer: true)
        });

        Assert.True(result.Ok);
        Assert.Equal("Default Switch", result.SwitchName);
        Assert.Equal(NetworkChangePolicy.ActionVmConnect, result.Action);
    }

    [Fact]
    public void VmConnectRejectsAuthUnmanagedLockRunningAndMissingSwitch()
    {
        Assert.Equal(
            NetworkChangeProblemCodes.Forbidden,
            NetworkChangePolicy.EvaluateVmConnect(ValidConnect() with
            {
                Auth = new NetworkChangeAuthContext(HasAdmin: true)
            }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.NotManaged,
            NetworkChangePolicy.EvaluateVmConnect(ValidConnect() with { Managed = false }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.TemplateLocked,
            NetworkChangePolicy.EvaluateVmConnect(ValidConnect() with { TemplateLocked = true }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.SourceNotOff,
            NetworkChangePolicy.EvaluateVmConnect(ValidConnect() with { PowerState = "running" }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.SwitchNotFound,
            NetworkChangePolicy.EvaluateVmConnect(ValidConnect() with { SwitchExists = false }).ErrorCode);
        Assert.Equal(
            NetworkChangeProblemCodes.VmRequired,
            NetworkChangePolicy.EvaluateVmConnect(ValidConnect() with { VmName = "  " }).ErrorCode);
    }

    [Fact]
    public void SchemaAndPrefixAreStable()
    {
        Assert.Equal("pcv-network-change-v1", NetworkChangePolicy.Schema);
        Assert.Equal("pcv-", NetworkChangePolicy.ProductPrefix);
        Assert.Equal("Default Switch", NetworkChangePolicy.DefaultSwitchName);
        Assert.True(NetworkChangePolicy.IsProductSwitchName("pcv-lab-internal"));
        Assert.True(NetworkChangePolicy.IsReservedSwitchName("Default Switch"));
        Assert.True(NetworkChangePolicy.IsReservedSwitchName("WSL (Hyper-V firewall)"));
        Assert.False(NetworkChangePolicy.IsReservedSwitchName("pcv-lab-internal"));
    }

    private static NetworkSwitchChangeRequest ValidCreate()
    {
        return new NetworkSwitchChangeRequest(
            "pcv-lab-internal",
            NetworkChangePolicy.TypeInternal,
            new NetworkChangeAuthContext(HasAdmin: true),
            AllowManagementOs: true);
    }

    private static NetworkSwitchChangeRequest ValidRemove()
    {
        return new NetworkSwitchChangeRequest(
            "pcv-lab-internal",
            NetworkChangePolicy.TypeInternal,
            new NetworkChangeAuthContext(HasAdmin: true),
            Exists: true);
    }

    private static VmNetworkConnectRequest ValidConnect()
    {
        return new VmNetworkConnectRequest(
            "lab-vm",
            "pcv-lab-internal",
            new NetworkChangeAuthContext(HasOperate: true),
            Managed: true,
            PowerState: "Off",
            SwitchExists: true);
    }
}
