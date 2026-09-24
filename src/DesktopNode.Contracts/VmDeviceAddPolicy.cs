namespace DesktopNode.Contracts;

public static class VmDeviceAddProblemCodes
{
    public const string Forbidden = "PCV_VM_DEVICE_ADD_FORBIDDEN";
    public const string VmRequired = "PCV_VM_DEVICE_VM_REQUIRED";
    public const string NotManaged = "PCV_VM_NOT_MANAGED_BY_PURECVISOR";
    public const string TemplateLocked = "PCV_VM_TEMPLATE_LOCKED";
    public const string SourceNotOff = "PCV_VM_DEVICE_SOURCE_NOT_OFF";
    public const string GenerationUnsupported = "PCV_VM_GENERATION_UNSUPPORTED";
    public const string KindUnsupported = "PCV_VM_DEVICE_KIND_UNSUPPORTED";
    public const string QuantityInvalid = "PCV_VM_DEVICE_QUANTITY_INVALID";
    public const string Limit = "PCV_VM_DEVICE_LIMIT";
    public const string AlreadyPresent = "PCV_VM_DEVICE_ALREADY_PRESENT";
    public const string SwitchRequired = "PCV_VM_NETWORK_SWITCH_REQUIRED";
    public const string SwitchNotFound = "PCV_NETWORK_SWITCH_NOT_FOUND";
    public const string NatForbidden = "PCV_NETWORK_NAT_FORBIDDEN";
    public const string DhcpForbidden = "PCV_NETWORK_DHCP_FORBIDDEN";
    public const string IsoForbidden = "PCV_VM_DEVICE_ISO_FORBIDDEN";
}

public sealed record VmDeviceAddAuthContext(
    bool HasOperate = false,
    bool HasServiceBearer = false);

public sealed record VmDeviceAddRequest(
    string? VmName,
    string? DeviceKind,
    VmDeviceAddAuthContext Auth,
    int Quantity = 1,
    bool Managed = false,
    bool TemplateLocked = false,
    string? PowerState = null,
    int Generation = 0,
    int ExistingNicCount = 0,
    int ExistingDvdCount = 0,
    string? SwitchName = null,
    bool SwitchExists = false,
    bool NatEnabled = false,
    bool DhcpEnabled = false,
    bool IsoRequested = false);

public sealed record VmDeviceAddEvaluation(
    bool Ok,
    string? ErrorCode,
    string? VmName,
    string? DeviceKind,
    string? SwitchName,
    int Quantity,
    string? Action);

public static class VmDeviceAddPolicy
{
    public const string Schema = "pcv-vm-device-add-v1";
    public const string ActionPreview = "device-add-preview";
    public const string ActionAdd = "device-add";
    public const string KindNic = "nic";
    public const string KindDvd = "dvd";
    public const string RequiredPowerState = "Off";
    public const int RequiredGeneration = 2;
    public const int MaxNicCount = 2;
    public const int MaxDvdCount = 1;

    public static VmDeviceAddEvaluation EvaluatePreview(VmDeviceAddRequest request)
    {
        return Evaluate(request, ActionPreview);
    }

    public static VmDeviceAddEvaluation EvaluateAdd(VmDeviceAddRequest request)
    {
        return Evaluate(request, ActionAdd);
    }

    private static VmDeviceAddEvaluation Evaluate(VmDeviceAddRequest request, string action)
    {
        if (!request.Auth.HasOperate && !request.Auth.HasServiceBearer)
        {
            return Reject(VmDeviceAddProblemCodes.Forbidden);
        }

        var vmName = Normalize(request.VmName);
        if (vmName is null)
        {
            return Reject(VmDeviceAddProblemCodes.VmRequired);
        }

        var kind = NormalizeKind(request.DeviceKind);
        if (kind is null)
        {
            return Reject(VmDeviceAddProblemCodes.KindUnsupported);
        }

        if (request.Quantity != 1)
        {
            return Reject(VmDeviceAddProblemCodes.QuantityInvalid);
        }

        if (!request.Managed)
        {
            return Reject(VmDeviceAddProblemCodes.NotManaged);
        }

        if (request.TemplateLocked)
        {
            return Reject(VmDeviceAddProblemCodes.TemplateLocked);
        }

        if (!IsPoweredOff(request.PowerState))
        {
            return Reject(VmDeviceAddProblemCodes.SourceNotOff);
        }

        if (request.Generation != RequiredGeneration)
        {
            return Reject(VmDeviceAddProblemCodes.GenerationUnsupported);
        }

        if (kind == KindNic)
        {
            return EvaluateNic(request, vmName, action);
        }

        return EvaluateDvd(request, vmName, action);
    }

    private static VmDeviceAddEvaluation EvaluateNic(VmDeviceAddRequest request, string vmName, string action)
    {
        if (request.NatEnabled)
        {
            return Reject(VmDeviceAddProblemCodes.NatForbidden);
        }

        if (request.DhcpEnabled)
        {
            return Reject(VmDeviceAddProblemCodes.DhcpForbidden);
        }

        var switchName = Normalize(request.SwitchName);
        if (switchName is null)
        {
            return Reject(VmDeviceAddProblemCodes.SwitchRequired);
        }

        if (!request.SwitchExists)
        {
            return Reject(VmDeviceAddProblemCodes.SwitchNotFound);
        }

        if (request.ExistingNicCount >= MaxNicCount)
        {
            return Reject(VmDeviceAddProblemCodes.Limit);
        }

        return Accept(vmName, KindNic, switchName, action);
    }

    private static VmDeviceAddEvaluation EvaluateDvd(VmDeviceAddRequest request, string vmName, string action)
    {
        if (request.IsoRequested)
        {
            return Reject(VmDeviceAddProblemCodes.IsoForbidden);
        }

        if (request.ExistingDvdCount >= MaxDvdCount)
        {
            return Reject(VmDeviceAddProblemCodes.AlreadyPresent);
        }

        return Accept(vmName, KindDvd, null, action);
    }

    private static bool IsPoweredOff(string? powerState)
    {
        var power = Normalize(powerState);
        return string.Equals(power, RequiredPowerState, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(power, "stopped", StringComparison.OrdinalIgnoreCase);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeKind(string? value)
    {
        var kind = Normalize(value);
        if (kind is null)
        {
            return null;
        }

        if (string.Equals(kind, KindNic, StringComparison.OrdinalIgnoreCase))
        {
            return KindNic;
        }

        if (string.Equals(kind, KindDvd, StringComparison.OrdinalIgnoreCase))
        {
            return KindDvd;
        }

        return null;
    }

    private static VmDeviceAddEvaluation Accept(string vmName, string kind, string? switchName, string action)
    {
        return new VmDeviceAddEvaluation(true, null, vmName, kind, switchName, 1, action);
    }

    private static VmDeviceAddEvaluation Reject(string code)
    {
        return new VmDeviceAddEvaluation(false, code, null, null, null, 0, null);
    }
}
