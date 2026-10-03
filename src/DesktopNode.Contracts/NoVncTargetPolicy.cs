namespace DesktopNode.Contracts;

public static class NoVncTargetProblemCodes
{
    public const string HostRequired = "PCV_NOVNC_TARGET_HOST_REQUIRED";
    public const string PortInvalid = "PCV_NOVNC_TARGET_PORT_INVALID";
    public const string NotLoopback = "PCV_NOVNC_TARGET_NOT_LOOPBACK";
    public const string LanGateRequired = "PCV_NOVNC_TARGET_LAN_GATE_REQUIRED";
    public const string ReasonRequired = "PCV_NOVNC_TARGET_REASON_REQUIRED";
    public const string Incomplete = "PCV_NOVNC_TARGET_INCOMPLETE";
    public const string ConfigureForbidden = "PCV_NOVNC_CONFIGURE_FORBIDDEN";
}

public sealed record NoVncTargetAuthContext(
    bool HasConsoleConfigure = false,
    bool HasServiceBearer = false);

public sealed record NoVncTargetRequest(
    string? Host,
    int? Port,
    NoVncTargetAuthContext Auth,
    bool AllowLanTarget = false,
    bool ListenerAllowLan = false,
    string? Reason = null);

public sealed record NoVncTargetEvaluation(
    bool Ok,
    string? ErrorCode,
    string? Host,
    int? Port,
    bool AllowLanTarget,
    string? Reason,
    bool Loopback,
    string? Action);

public static class NoVncTargetPolicy
{
    public const string Schema = "pcv-novnc-target-v1";
    public const string ActionPreview = "preview";
    public const string ActionSet = "set";
    public const string ActionClear = "clear";
    public const string PermissionConfigure = "console.configure";
    public const int MinPort = 1;
    public const int MaxPort = 65535;

    public static NoVncTargetEvaluation EvaluatePreview(NoVncTargetRequest request)
    {
        return EvaluateEnabled(request, ActionPreview);
    }

    public static NoVncTargetEvaluation EvaluateSet(NoVncTargetRequest request)
    {
        return EvaluateEnabled(request, ActionSet);
    }

    public static NoVncTargetEvaluation EvaluateClear(NoVncTargetRequest request)
    {
        if (RejectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        return new NoVncTargetEvaluation(
            true,
            null,
            null,
            null,
            false,
            null,
            false,
            ActionClear);
    }

    private static NoVncTargetEvaluation EvaluateEnabled(NoVncTargetRequest request, string action)
    {
        if (RejectAuth(request.Auth) is { } authError)
        {
            return Reject(authError);
        }

        var host = NormalizeHost(request.Host);
        var hasHost = host is not null;
        var hasPort = request.Port is not null;
        if (hasHost != hasPort)
        {
            return Reject(NoVncTargetProblemCodes.Incomplete);
        }

        if (!hasHost)
        {
            return Reject(NoVncTargetProblemCodes.HostRequired);
        }

        var port = request.Port!.Value;
        if (port is < MinPort or > MaxPort)
        {
            return Reject(NoVncTargetProblemCodes.PortInvalid);
        }

        var loopback = IsLoopbackHost(host);
        if (!loopback)
        {
            if (!request.AllowLanTarget)
            {
                return Reject(NoVncTargetProblemCodes.NotLoopback);
            }

            if (!request.ListenerAllowLan)
            {
                return Reject(NoVncTargetProblemCodes.LanGateRequired);
            }

            var reason = NormalizeReason(request.Reason);
            if (reason is null)
            {
                return Reject(NoVncTargetProblemCodes.ReasonRequired);
            }

            return new NoVncTargetEvaluation(
                true,
                null,
                host,
                port,
                true,
                reason,
                false,
                action);
        }

        return new NoVncTargetEvaluation(
            true,
            null,
            host,
            port,
            request.AllowLanTarget,
            NormalizeReason(request.Reason),
            true,
            action);
    }

    private static string? RejectAuth(NoVncTargetAuthContext auth)
    {
        return auth.HasServiceBearer || auth.HasConsoleConfigure
            ? null
            : NoVncTargetProblemCodes.ConfigureForbidden;
    }

    private static string? NormalizeHost(string? host)
    {
        var trimmed = host?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string? NormalizeReason(string? reason)
    {
        var trimmed = reason?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public static bool IsLoopbackHost(string? host)
    {
        var normalized = NormalizeHost(host)?.ToLowerInvariant();
        return normalized is "127.0.0.1" or "localhost" or "::1";
    }

    private static NoVncTargetEvaluation Reject(string errorCode)
    {
        return new NoVncTargetEvaluation(false, errorCode, null, null, false, null, false, null);
    }
}
