namespace DesktopNode.Contracts;

public static class NetworkChangeProblemCodes
{
    public const string Forbidden = "PCV_NETWORK_CHANGE_FORBIDDEN";
    public const string SwitchNameRequired = "PCV_NETWORK_SWITCH_NAME_REQUIRED";
    public const string SwitchNameReserved = "PCV_NETWORK_SWITCH_NAME_RESERVED";
    public const string SwitchNameNotProduct = "PCV_NETWORK_SWITCH_NAME_NOT_PRODUCT";
    public const string SwitchTypeUnsupported = "PCV_NETWORK_SWITCH_TYPE_UNSUPPORTED";
    public const string NatForbidden = "PCV_NETWORK_NAT_FORBIDDEN";
    public const string DhcpForbidden = "PCV_NETWORK_DHCP_FORBIDDEN";
    public const string AlreadyExists = "PCV_NETWORK_SWITCH_ALREADY_EXISTS";
    public const string SwitchNotFound = "PCV_NETWORK_SWITCH_NOT_FOUND";
    public const string SwitchInUse = "PCV_NETWORK_SWITCH_IN_USE";
    public const string ManagementOsInvalid = "PCV_NETWORK_MANAGEMENT_OS_INVALID";
    public const string VmRequired = "PCV_VM_NETWORK_VM_REQUIRED";
    public const string NotManaged = "PCV_VM_NOT_MANAGED_BY_PURECVISOR";
    public const string TemplateLocked = "PCV_VM_TEMPLATE_LOCKED";
    public const string SourceNotOff = "PCV_VM_NETWORK_SOURCE_NOT_OFF";
    public const string SwitchRequired = "PCV_VM_NETWORK_SWITCH_REQUIRED";
}

public sealed record NetworkChangeAuthContext(
    bool HasOperate = false,
    bool HasAdmin = false,
    bool HasServiceBearer = false);

public sealed record NetworkSwitchChangeRequest(
    string? SwitchName,
    string? SwitchType,
    NetworkChangeAuthContext Auth,
    bool AllowManagementOs = false,
    bool NatEnabled = false,
    bool DhcpEnabled = false,
    bool Exists = false,
    int AttachedVmCount = 0);

public sealed record VmNetworkConnectRequest(
    string? VmName,
    string? SwitchName,
    NetworkChangeAuthContext Auth,
    bool Managed = false,
    bool TemplateLocked = false,
    string? PowerState = null,
    bool SwitchExists = false);

public sealed record NetworkChangeEvaluation(
    bool Ok,
    string? ErrorCode,
    string? SwitchName,
    string? SwitchType,
    bool AllowManagementOs,
    string? VmName,
    string? Action);

public static class NetworkChangePolicy
{
    public const string Schema = "pcv-network-change-v1";
    public const string ActionSwitchCreatePreview = "switch-create-preview";
    public const string ActionSwitchCreate = "switch-create";
    public const string ActionSwitchRemove = "switch-remove";
    public const string ActionVmConnectPreview = "vm-connect-preview";
    public const string ActionVmConnect = "vm-connect";
    public const string TypeInternal = "internal";
    public const string TypePrivate = "private";
    public const string TypeExternal = "external";
    public const string ProductPrefix = "pcv-";
    public const string DefaultSwitchName = "Default Switch";
    public const string RequiredPowerState = "Off";
    public const string PermissionAdmin = "admin";
    public const string PermissionOperate = "operate";

    public static NetworkChangeEvaluation EvaluateSwitchCreatePreview(NetworkSwitchChangeRequest request)
    {
        return EvaluateSwitchCreate(request, ActionSwitchCreatePreview);
    }

    public static NetworkChangeEvaluation EvaluateSwitchCreate(NetworkSwitchChangeRequest request)
    {
        return EvaluateSwitchCreate(request, ActionSwitchCreate);
    }

    public static NetworkChangeEvaluation EvaluateSwitchRemove(NetworkSwitchChangeRequest request)
    {
        if (RejectSwitchAdminAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var switchName = NormalizeName(request.SwitchName);
        if (switchName is null)
        {
            return Reject(NetworkChangeProblemCodes.SwitchNameRequired);
        }

        if (IsReservedSwitchName(switchName))
        {
            return Reject(NetworkChangeProblemCodes.SwitchNameReserved);
        }

        if (!IsProductSwitchName(switchName))
        {
            return Reject(NetworkChangeProblemCodes.SwitchNameNotProduct);
        }

        if (!request.Exists)
        {
            return Reject(NetworkChangeProblemCodes.SwitchNotFound);
        }

        if (request.AttachedVmCount > 0)
        {
            return Reject(NetworkChangeProblemCodes.SwitchInUse);
        }

        return new NetworkChangeEvaluation(
            true,
            null,
            switchName,
            null,
            false,
            null,
            ActionSwitchRemove);
    }

    public static NetworkChangeEvaluation EvaluateVmConnectPreview(VmNetworkConnectRequest request)
    {
        return EvaluateVmConnect(request, ActionVmConnectPreview);
    }

    public static NetworkChangeEvaluation EvaluateVmConnect(VmNetworkConnectRequest request)
    {
        return EvaluateVmConnect(request, ActionVmConnect);
    }

    private static NetworkChangeEvaluation EvaluateSwitchCreate(NetworkSwitchChangeRequest request, string action)
    {
        if (RejectSwitchAdminAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var switchName = NormalizeName(request.SwitchName);
        if (switchName is null)
        {
            return Reject(NetworkChangeProblemCodes.SwitchNameRequired);
        }

        if (IsReservedSwitchName(switchName))
        {
            return Reject(NetworkChangeProblemCodes.SwitchNameReserved);
        }

        if (!IsProductSwitchName(switchName))
        {
            return Reject(NetworkChangeProblemCodes.SwitchNameNotProduct);
        }

        var switchType = NormalizeType(request.SwitchType);
        if (switchType is null)
        {
            return Reject(NetworkChangeProblemCodes.SwitchTypeUnsupported);
        }

        if (request.NatEnabled)
        {
            return Reject(NetworkChangeProblemCodes.NatForbidden);
        }

        if (request.DhcpEnabled)
        {
            return Reject(NetworkChangeProblemCodes.DhcpForbidden);
        }

        if (switchType == TypeInternal && !request.AllowManagementOs)
        {
            return Reject(NetworkChangeProblemCodes.ManagementOsInvalid);
        }

        if (switchType == TypePrivate && request.AllowManagementOs)
        {
            return Reject(NetworkChangeProblemCodes.ManagementOsInvalid);
        }

        if (request.Exists)
        {
            return Reject(NetworkChangeProblemCodes.AlreadyExists);
        }

        return new NetworkChangeEvaluation(
            true,
            null,
            switchName,
            switchType,
            request.AllowManagementOs,
            null,
            action);
    }

    private static NetworkChangeEvaluation EvaluateVmConnect(VmNetworkConnectRequest request, string action)
    {
        if (RejectVmConnectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var vmName = NormalizeName(request.VmName);
        if (vmName is null)
        {
            return Reject(NetworkChangeProblemCodes.VmRequired);
        }

        var switchName = NormalizeName(request.SwitchName);
        if (switchName is null)
        {
            return Reject(NetworkChangeProblemCodes.SwitchRequired);
        }

        if (!request.Managed)
        {
            return Reject(NetworkChangeProblemCodes.NotManaged);
        }

        if (request.TemplateLocked)
        {
            return Reject(NetworkChangeProblemCodes.TemplateLocked);
        }

        if (!VmPowerStates.IsOff(request.PowerState))
        {
            return Reject(NetworkChangeProblemCodes.SourceNotOff);
        }

        if (!request.SwitchExists)
        {
            return Reject(NetworkChangeProblemCodes.SwitchNotFound);
        }

        return new NetworkChangeEvaluation(
            true,
            null,
            switchName,
            null,
            false,
            vmName,
            action);
    }

    private static string? RejectSwitchAdminAuth(NetworkChangeAuthContext auth)
    {
        return auth.HasAdmin || auth.HasServiceBearer
            ? null
            : NetworkChangeProblemCodes.Forbidden;
    }

    private static string? RejectVmConnectAuth(NetworkChangeAuthContext auth)
    {
        return auth.HasOperate || auth.HasServiceBearer
            ? null
            : NetworkChangeProblemCodes.Forbidden;
    }

    private static string? NormalizeName(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeType(string? value)
    {
        var trimmed = NormalizeName(value);
        if (trimmed is null)
        {
            return null;
        }

        if (string.Equals(trimmed, TypeInternal, StringComparison.OrdinalIgnoreCase))
        {
            return TypeInternal;
        }

        if (string.Equals(trimmed, TypePrivate, StringComparison.OrdinalIgnoreCase))
        {
            return TypePrivate;
        }

        return null;
    }

    public static bool IsProductSwitchName(string name)
    {
        return name.StartsWith(ProductPrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsReservedSwitchName(string name)
    {
        return string.Equals(name, DefaultSwitchName, StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("WSL", StringComparison.OrdinalIgnoreCase);
    }

    private static NetworkChangeEvaluation Reject(string code)
    {
        return new NetworkChangeEvaluation(false, code, null, null, false, null, null);
    }
}
