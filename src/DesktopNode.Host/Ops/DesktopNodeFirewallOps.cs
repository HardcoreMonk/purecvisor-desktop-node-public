using DesktopNode.Host;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeFirewallOps
{
    public const string OperationFamily = "firewall";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsFirewallController firewallController)
    {
        if (options.DryRun)
        {
            return new DesktopNodeHostServiceActionResult(
                Ok: true,
                Action: options.ServiceAction ?? string.Empty,
                Plan: plan,
                Commands: [],
                RemovedPaths: [],
                PreparedTokenPath: null,
                Service: null,
                ServiceOwnerVerified: false,
                ErrorCode: null,
                ErrorMessage: null);
        }

        var spec = plan.FirewallRule ?? throw new ArgumentException("PCV_HOST_FIREWALL_RULE_REQUIRED|Firewall service action requires a rule spec.|Pass exact firewall action inputs.");
        if (string.Equals(plan.NativeFirewallOperation, "firewall-enable", StringComparison.OrdinalIgnoreCase) &&
            !plan.LanExposureApproved)
        {
            return NativeFirewallFailure(
                options,
                plan,
                null,
                "PCV_HOST_FIREWALL_LAN_APPROVAL_REQUIRED",
                "Firewall LAN exposure requires explicit --allow-lan approval.");
        }

        DesktopNodeWindowsFirewallRuleSnapshot? current = null;
        try
        {
            current = firewallController.Query(spec);
            if (current.Exists && !current.Owned)
            {
                return NativeFirewallFailure(
                    options,
                    plan,
                    current,
                    "PCV_HOST_FIREWALL_RULE_OWNERSHIP_MISMATCH",
                    $"Windows Firewall rule '{spec.RuleName}' exists but does not match the Desktop Node ownership contract.");
            }

            if (string.Equals(plan.NativeFirewallOperation, "firewall-enable", StringComparison.OrdinalIgnoreCase))
            {
                var next = current.Exists && current.Enabled
                    ? current
                    : firewallController.Enable(spec);
                var ok = next.Exists && next.Enabled && next.Owned;
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_FIREWALL_RULE_ENABLE_FAILED",
                    ErrorMessage: ok ? null : $"Windows Firewall rule '{spec.RuleName}' was not enabled with the expected binding.",
                    FirewallRule: next);
            }

            if (string.Equals(plan.NativeFirewallOperation, "firewall-remove", StringComparison.OrdinalIgnoreCase))
            {
                var next = current.Exists
                    ? firewallController.Remove(spec)
                    : current;
                var ok = !next.Exists;
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_FIREWALL_RULE_REMOVE_FAILED",
                    ErrorMessage: ok ? null : $"Windows Firewall rule '{spec.RuleName}' still exists after remove.",
                    FirewallRule: next);
            }

            return NativeFirewallFailure(
                options,
                plan,
                current,
                "PCV_HOST_FIREWALL_ACTION_INVALID",
                $"Windows Firewall action '{plan.NativeFirewallOperation}' is not supported.");
        }
        catch (DesktopNodeWindowsFirewallControllerException error)
        {
            return NativeFirewallFailure(options, plan, current, error.Code, error.Message);
        }
    }

    private static DesktopNodeHostServiceActionResult NativeFirewallFailure(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsFirewallRuleSnapshot? firewallRule,
        string errorCode,
        string errorMessage)
    {
        return new DesktopNodeHostServiceActionResult(
            Ok: false,
            Action: options.ServiceAction ?? string.Empty,
            Plan: plan,
            Commands: [],
            RemovedPaths: [],
            PreparedTokenPath: null,
            Service: null,
            ServiceOwnerVerified: false,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            FirewallRule: firewallRule);
    }
}
