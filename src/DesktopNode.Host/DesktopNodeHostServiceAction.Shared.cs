using System.Text;
using System.Text.Json;
using DesktopNode.Host.Ops;

namespace DesktopNode.Host;

public static partial class DesktopNodeHostServiceAction
{
    internal static DesktopNodeHostServiceActionResult NativeServiceFailure(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        DesktopNodeWindowsServiceSnapshot service,
        bool ownerVerified,
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
            Service: service,
            ServiceOwnerVerified: ownerVerified,
            ErrorCode: errorCode,
            ErrorMessage: errorMessage);
    }

    internal static bool OwnedFileExists(string path)
    {
        try
        {
            _ = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    internal static bool IsOwnedFileAccessFailure(Exception error)
    {
        return error is IOException or
            UnauthorizedAccessException or
            System.Security.SecurityException;
    }
    private static DesktopNodeWindowsFirewallRuleSpec CreateFirewallRuleSpec(DesktopNodeHostOptions options)
    {
        var localPort = options.FirewallLocalPort ?? 7777;
        if (localPort is < 1 or > 65535)
        {
            throw new ArgumentException($"PCV_HOST_FIREWALL_LOCAL_PORT_INVALID|Firewall local port must be between 1 and 65535.|Rejected value '{localPort}'.");
        }

        return new DesktopNodeWindowsFirewallRuleSpec(
            RuleName: string.IsNullOrWhiteSpace(options.FirewallRuleName)
                ? "PureCVisor Desktop Node Local API LAN"
                : options.FirewallRuleName,
            Direction: "inbound",
            Protocol: "TCP",
            LocalPort: localPort,
            Profile: string.IsNullOrWhiteSpace(options.FirewallProfile) ? "Private" : options.FirewallProfile,
            RemoteAddress: string.IsNullOrWhiteSpace(options.FirewallRemoteAddress) ? "LocalSubnet" : options.FirewallRemoteAddress,
            OwnerDescription: "PureCVisor Desktop Node managed firewall rule; owner=DesktopNode.Host; action=LAN Local API opt-in");
    }

    private static IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> CreateTrustStoreCertificateSpecs(
        DesktopNodeHostOptions options,
        string action)
    {
        var requiresCertificatePaths = string.Equals(action, "trust-store-install", StringComparison.OrdinalIgnoreCase);
        var rootPath = requiresCertificatePaths
            ? Require(options.TrustRootCertificatePath, "PCV_HOST_TRUST_ROOT_CERTIFICATE_REQUIRED")
            : null;
        var publisherPath = requiresCertificatePaths
            ? Require(options.TrustPublisherCertificatePath, "PCV_HOST_TRUST_PUBLISHER_CERTIFICATE_REQUIRED")
            : null;
        return
        [
            new DesktopNodeWindowsTrustStoreCertificateSpec(
                StoreName: "Root",
                StoreLocation: "LocalMachine",
                ExpectedSubject: "CN=PureCVisor Internal Code Signing Root CA",
                Thumbprint: NormalizeThumbprint(Require(options.TrustRootCertificateThumbprint, "PCV_HOST_TRUST_ROOT_THUMBPRINT_REQUIRED")),
                CertificatePath: rootPath),
            new DesktopNodeWindowsTrustStoreCertificateSpec(
                StoreName: "TrustedPublisher",
                StoreLocation: "LocalMachine",
                ExpectedSubject: "CN=PureCVisor Desktop Node Internal Code Signing",
                Thumbprint: NormalizeThumbprint(Require(options.TrustPublisherCertificateThumbprint, "PCV_HOST_TRUST_PUBLISHER_THUMBPRINT_REQUIRED")),
                CertificatePath: publisherPath)
        ];
    }

    internal static string Require(string? value, string code)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{code}|Required .NET host service action value is missing.|Pass all installed product paths from the MSI custom action.");
        }

        return value;
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }

    private static bool IsNativeServiceAction(string? action)
    {
        return Ops.DesktopNodeServiceLifecycleOps.Owns(action);
    }

    private static bool IsNativeServiceTokenAction(string? action)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(action, "service-token");
    }

    private static string ResolveOperationFamily(string action)
    {
        return DesktopNodeHostOpsCatalog.TryGetOperation(action, out var entry)
            ? entry.OperationFamily
            : "unknown";
    }

    private static bool IsNativeEventLogAction(string? action)
    {
        return Ops.DesktopNodeEventLogOps.Owns(action);
    }

    private static bool IsNativeFirewallAction(string? action)
    {
        return Ops.DesktopNodeFirewallOps.Owns(action);
    }

    private static bool IsNativeTrustStoreAction(string? action)
    {
        return Ops.DesktopNodeTrustStoreOps.Owns(action);
    }

    private static bool IsNativeConfigMigrationAction(string? action)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(action, "config-migration");
    }

    private static bool IsNativeJobStoreMigrationAction(string? action)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(action, "job-store-migration");
    }

    private static bool IsNativeCredentialManagerAction(string? action)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(action, "credential-manager");
    }

    private static bool IsNativeDataRootLifecycleAction(string? action)
    {
        return Ops.DesktopNodeDataRootLifecycleOps.Owns(action);
    }

    internal static bool UsesProtectedFileTokenSource(string? binaryPathName)
    {
        return !string.IsNullOrWhiteSpace(binaryPathName) &&
            binaryPathName.Contains("--api-token-protected-file", StringComparison.OrdinalIgnoreCase) &&
            !binaryPathName.Contains("--api-token-credential-target", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsSupportedMigrationPlan(DesktopNodeHostOptions options, string planId, int planVersion)
    {
        return string.Equals(options.MigrationPlanId, planId, StringComparison.Ordinal) &&
            options.MigrationPlanVersion == planVersion;
    }

    private static bool RequiresDataRoot(string action)
    {
        return DesktopNodeHostOpsCatalog.RequiresDataRoot(action);
    }

    internal static bool IsOwnedService(DesktopNodeWindowsServiceSnapshot snapshot, string expectedExecutablePath)
    {
        if (!snapshot.Exists || string.IsNullOrWhiteSpace(snapshot.BinaryPathName))
        {
            return false;
        }

        var actualExecutable = ExtractExecutablePath(snapshot.BinaryPathName);
        return string.Equals(
            NormalizePath(actualExecutable),
            NormalizePath(expectedExecutablePath),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractExecutablePath(string binaryPathName)
    {
        var trimmed = binaryPathName.Trim();
        if (trimmed.StartsWith('"'))
        {
            var endQuote = trimmed.IndexOf('"', 1);
            return endQuote > 1 ? trimmed[1..endQuote] : trimmed.Trim('"');
        }

        var exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        return exeIndex >= 0 ? trimmed[..(exeIndex + 4)] : trimmed.Split(' ', 2)[0];
    }

    private static string NormalizePath(string path)
    {
        return path.Trim().Trim('"').Replace('/', '\\').TrimEnd('\\');
    }

    private static string NormalizeThumbprint(string thumbprint)
    {
        return thumbprint.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(":", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();
    }
}
