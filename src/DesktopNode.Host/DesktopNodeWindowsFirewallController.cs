using System.Globalization;
using System.Runtime.InteropServices;

namespace DesktopNode.Host;

public sealed record DesktopNodeWindowsFirewallRuleSpec(
    string RuleName,
    string Direction,
    string Protocol,
    int LocalPort,
    string Profile,
    string RemoteAddress,
    string OwnerDescription);

public sealed record DesktopNodeWindowsFirewallRuleSnapshot(
    string RuleName,
    bool Exists,
    bool Enabled,
    string? Direction,
    string? Protocol,
    int? LocalPort,
    string? Profile,
    string? RemoteAddress,
    string? Description,
    bool Owned)
{
    public static DesktopNodeWindowsFirewallRuleSnapshot Missing(
        string ruleName,
        string direction,
        string protocol,
        int localPort,
        string profile,
        string remoteAddress)
    {
        return new DesktopNodeWindowsFirewallRuleSnapshot(
            RuleName: ruleName,
            Exists: false,
            Enabled: false,
            Direction: direction,
            Protocol: protocol,
            LocalPort: localPort,
            Profile: profile,
            RemoteAddress: remoteAddress,
            Description: null,
            Owned: false);
    }
}

public interface IDesktopNodeWindowsFirewallController
{
    DesktopNodeWindowsFirewallRuleSnapshot Query(DesktopNodeWindowsFirewallRuleSpec spec);
    DesktopNodeWindowsFirewallRuleSnapshot Enable(DesktopNodeWindowsFirewallRuleSpec spec);
    DesktopNodeWindowsFirewallRuleSnapshot Remove(DesktopNodeWindowsFirewallRuleSpec spec);
}

public sealed class DesktopNodeWindowsFirewallController : IDesktopNodeWindowsFirewallController
{
    private const int NetFwRuleDirIn = 1;
    private const int NetFwActionAllow = 1;
    private const int NetFwIpProtocolTcp = 6;
    private const int NetFwProfile2Domain = 1;
    private const int NetFwProfile2Private = 2;
    private const int NetFwProfile2Public = 4;
    private const int NetFwProfile2All = int.MaxValue;
    private const string OwnerMarker = "PureCVisor Desktop Node managed firewall rule";

    public DesktopNodeWindowsFirewallRuleSnapshot Query(DesktopNodeWindowsFirewallRuleSpec spec)
    {
        try
        {
            dynamic policy = CreateComObject("HNetCfg.FwPolicy2", "PCV_HOST_FIREWALL_POLICY_UNAVAILABLE");
            var rule = TryGetRule(policy.Rules, spec.RuleName);
            return rule is null ? Missing(spec) : SnapshotFromRule(spec, rule);
        }
        catch (DesktopNodeWindowsFirewallControllerException)
        {
            throw;
        }
        catch (Exception error) when (error is UnauthorizedAccessException or COMException or InvalidOperationException)
        {
            throw new DesktopNodeWindowsFirewallControllerException(
                "PCV_HOST_FIREWALL_QUERY_FAILED",
                $"Windows Firewall rule '{spec.RuleName}' could not be queried: {error.Message}",
                error);
        }
    }

    public DesktopNodeWindowsFirewallRuleSnapshot Enable(DesktopNodeWindowsFirewallRuleSpec spec)
    {
        try
        {
            dynamic policy = CreateComObject("HNetCfg.FwPolicy2", "PCV_HOST_FIREWALL_POLICY_UNAVAILABLE");
            dynamic rules = policy.Rules;
            var existing = TryGetRule(rules, spec.RuleName);
            if (existing is not null)
            {
                var current = SnapshotFromRule(spec, existing);
                if (!current.Owned)
                {
                    throw new DesktopNodeWindowsFirewallControllerException(
                        "PCV_HOST_FIREWALL_RULE_OWNERSHIP_MISMATCH",
                        $"Windows Firewall rule '{spec.RuleName}' exists but does not match the Desktop Node ownership contract.");
                }

                dynamic existingRule = existing;
                existingRule.Enabled = true;
                return Query(spec);
            }

            dynamic rule = CreateComObject("HNetCfg.FWRule", "PCV_HOST_FIREWALL_RULE_CREATE_FAILED");
            rule.Name = spec.RuleName;
            rule.Description = spec.OwnerDescription;
            rule.Direction = NetFwRuleDirIn;
            rule.Protocol = NetFwIpProtocolTcp;
            rule.LocalPorts = spec.LocalPort.ToString(CultureInfo.InvariantCulture);
            rule.Profiles = ProfileToMask(spec.Profile);
            rule.RemoteAddresses = spec.RemoteAddress;
            rule.Action = NetFwActionAllow;
            rule.Enabled = true;
            rules.Add(rule);
            return Query(spec);
        }
        catch (DesktopNodeWindowsFirewallControllerException)
        {
            throw;
        }
        catch (Exception error) when (error is UnauthorizedAccessException or COMException or InvalidOperationException)
        {
            throw new DesktopNodeWindowsFirewallControllerException(
                "PCV_HOST_FIREWALL_RULE_ENABLE_FAILED",
                $"Windows Firewall rule '{spec.RuleName}' could not be enabled: {error.Message}",
                error);
        }
    }

    public DesktopNodeWindowsFirewallRuleSnapshot Remove(DesktopNodeWindowsFirewallRuleSpec spec)
    {
        var current = Query(spec);
        if (!current.Exists)
        {
            return current;
        }

        if (!current.Owned)
        {
            throw new DesktopNodeWindowsFirewallControllerException(
                "PCV_HOST_FIREWALL_RULE_OWNERSHIP_MISMATCH",
                $"Windows Firewall rule '{spec.RuleName}' exists but does not match the Desktop Node ownership contract.");
        }

        try
        {
            dynamic policy = CreateComObject("HNetCfg.FwPolicy2", "PCV_HOST_FIREWALL_POLICY_UNAVAILABLE");
            policy.Rules.Remove(spec.RuleName);
            return Query(spec);
        }
        catch (DesktopNodeWindowsFirewallControllerException)
        {
            throw;
        }
        catch (Exception error) when (error is UnauthorizedAccessException or COMException or InvalidOperationException)
        {
            throw new DesktopNodeWindowsFirewallControllerException(
                "PCV_HOST_FIREWALL_RULE_REMOVE_FAILED",
                $"Windows Firewall rule '{spec.RuleName}' could not be removed: {error.Message}",
                error);
        }
    }

    private static DesktopNodeWindowsFirewallRuleSnapshot Missing(DesktopNodeWindowsFirewallRuleSpec spec)
    {
        return DesktopNodeWindowsFirewallRuleSnapshot.Missing(
            spec.RuleName,
            spec.Direction,
            spec.Protocol,
            spec.LocalPort,
            spec.Profile,
            spec.RemoteAddress);
    }

    private static object CreateComObject(string progId, string errorCode)
    {
        var type = Type.GetTypeFromProgID(progId) ??
            throw new DesktopNodeWindowsFirewallControllerException(
                errorCode,
                $"Windows Firewall COM object '{progId}' is unavailable.");
        return Activator.CreateInstance(type) ??
            throw new DesktopNodeWindowsFirewallControllerException(
                errorCode,
                $"Windows Firewall COM object '{progId}' could not be created.");
    }

    private static object? TryGetRule(dynamic rules, string ruleName)
    {
        try
        {
            return rules.Item(ruleName);
        }
        catch (Exception error) when (IsMissingRuleLookupFailure(error))
        {
            return null;
        }
    }

    internal static bool IsMissingRuleLookupFailure(Exception error)
    {
        return error is COMException or FileNotFoundException;
    }

    private static DesktopNodeWindowsFirewallRuleSnapshot SnapshotFromRule(
        DesktopNodeWindowsFirewallRuleSpec spec,
        object ruleObject)
    {
        dynamic rule = ruleObject;
        var direction = DirectionToName(Convert.ToInt32(rule.Direction, CultureInfo.InvariantCulture));
        var protocol = ProtocolToName(Convert.ToInt32(rule.Protocol, CultureInfo.InvariantCulture));
        var localPort = ParseSinglePort(rule.LocalPorts as string);
        var profileMask = Convert.ToInt32(rule.Profiles, CultureInfo.InvariantCulture);
        var profile = ProfileMaskToName(profileMask);
        var remoteAddress = rule.RemoteAddresses as string;
        var description = rule.Description as string;
        var enabled = Convert.ToBoolean(rule.Enabled, CultureInfo.InvariantCulture);
        var owned =
            string.Equals(direction, spec.Direction, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(protocol, spec.Protocol, StringComparison.OrdinalIgnoreCase) &&
            localPort == spec.LocalPort &&
            profileMask == ProfileToMask(spec.Profile) &&
            string.Equals(NormalizeText(remoteAddress), NormalizeText(spec.RemoteAddress), StringComparison.OrdinalIgnoreCase) &&
            description?.Contains(OwnerMarker, StringComparison.Ordinal) == true;

        return new DesktopNodeWindowsFirewallRuleSnapshot(
            RuleName: spec.RuleName,
            Exists: true,
            Enabled: enabled,
            Direction: direction,
            Protocol: protocol,
            LocalPort: localPort,
            Profile: profile,
            RemoteAddress: remoteAddress,
            Description: description,
            Owned: owned);
    }

    private static int? ParseSinglePort(string? ports)
    {
        return int.TryParse(ports, NumberStyles.None, CultureInfo.InvariantCulture, out var port) ? port : null;
    }

    private static string DirectionToName(int direction)
    {
        return direction == NetFwRuleDirIn ? "inbound" : direction.ToString(CultureInfo.InvariantCulture);
    }

    private static string ProtocolToName(int protocol)
    {
        return protocol == NetFwIpProtocolTcp ? "TCP" : protocol.ToString(CultureInfo.InvariantCulture);
    }

    private static int ProfileToMask(string profile)
    {
        return profile.ToLowerInvariant() switch
        {
            "domain" => NetFwProfile2Domain,
            "private" => NetFwProfile2Private,
            "public" => NetFwProfile2Public,
            "all" => NetFwProfile2All,
            _ => throw new DesktopNodeWindowsFirewallControllerException(
                "PCV_HOST_FIREWALL_PROFILE_INVALID",
                $"Windows Firewall profile '{profile}' is not supported.")
        };
    }

    private static string ProfileMaskToName(int profileMask)
    {
        return profileMask switch
        {
            NetFwProfile2Domain => "Domain",
            NetFwProfile2Private => "Private",
            NetFwProfile2Public => "Public",
            NetFwProfile2All => "All",
            _ => profileMask.ToString(CultureInfo.InvariantCulture)
        };
    }

    private static string NormalizeText(string? value)
    {
        return (value ?? string.Empty).Trim();
    }
}

public sealed class DesktopNodeWindowsFirewallControllerException : Exception
{
    public DesktopNodeWindowsFirewallControllerException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
