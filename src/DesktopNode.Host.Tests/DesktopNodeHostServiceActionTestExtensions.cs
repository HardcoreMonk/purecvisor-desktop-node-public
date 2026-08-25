using System.Text;
using System.Text.Json;
using DesktopNode.Host;
using DesktopNode.Host.Ops;

namespace DesktopNode.Host.Tests;
// file-local visibility is dropped so test partials can share these extensions after the
// pure-move split of DesktopNodeHostServiceActionTests into multiple source files.
internal static class DesktopNodeHostServiceActionTestExtensions
{
    public static DesktopNodeHostOptions WithAction(this DesktopNodeHostOptions options, string action)
    {
        return new DesktopNodeHostOptions
        {
            Mode = options.Mode,
            Prefix = options.Prefix,
            WebRootPath = options.WebRootPath,
            JobStorePath = options.JobStorePath,
            EventLogPath = options.EventLogPath,
            ApiTokenFile = options.ApiTokenFile,
            ApiTokenProtectedFile = options.ApiTokenProtectedFile,
            ApiTokenCredentialTarget = options.ApiTokenCredentialTarget,
            AllowLan = options.AllowLan,
            ServiceAction = action,
            ProductRoot = options.ProductRoot,
            DataRoot = options.DataRoot,
            ServiceExecutablePath = options.ServiceExecutablePath,
            RemoveData = options.RemoveData,
            DryRun = options.DryRun,
            ReleaseApproved = options.ReleaseApproved,
            FirewallRuleName = options.FirewallRuleName,
            FirewallLocalPort = options.FirewallLocalPort,
            FirewallProfile = options.FirewallProfile,
            FirewallRemoteAddress = options.FirewallRemoteAddress,
            TrustRootCertificatePath = options.TrustRootCertificatePath,
            TrustRootCertificateThumbprint = options.TrustRootCertificateThumbprint,
            TrustPublisherCertificatePath = options.TrustPublisherCertificatePath,
            TrustPublisherCertificateThumbprint = options.TrustPublisherCertificateThumbprint,
            MigrationPlanId = options.MigrationPlanId,
            MigrationPlanVersion = options.MigrationPlanVersion,
            CredentialTarget = options.CredentialTarget,
            EventLogDefaultTransitionTimeoutSeconds = options.EventLogDefaultTransitionTimeoutSeconds
        };
    }

    public static DesktopNodeHostOptions WithEventLogDefaultTransitionTimeoutSeconds(this DesktopNodeHostOptions options, int seconds)
    {
        return new DesktopNodeHostOptions
        {
            Mode = options.Mode,
            Prefix = options.Prefix,
            WebRootPath = options.WebRootPath,
            JobStorePath = options.JobStorePath,
            EventLogPath = options.EventLogPath,
            ApiTokenFile = options.ApiTokenFile,
            ApiTokenProtectedFile = options.ApiTokenProtectedFile,
            ApiTokenCredentialTarget = options.ApiTokenCredentialTarget,
            AllowLan = options.AllowLan,
            ServiceAction = options.ServiceAction,
            ProductRoot = options.ProductRoot,
            DataRoot = options.DataRoot,
            ServiceExecutablePath = options.ServiceExecutablePath,
            RemoveData = options.RemoveData,
            DryRun = options.DryRun,
            ReleaseApproved = options.ReleaseApproved,
            FirewallRuleName = options.FirewallRuleName,
            FirewallLocalPort = options.FirewallLocalPort,
            FirewallProfile = options.FirewallProfile,
            FirewallRemoteAddress = options.FirewallRemoteAddress,
            TrustRootCertificatePath = options.TrustRootCertificatePath,
            TrustRootCertificateThumbprint = options.TrustRootCertificateThumbprint,
            TrustPublisherCertificatePath = options.TrustPublisherCertificatePath,
            TrustPublisherCertificateThumbprint = options.TrustPublisherCertificateThumbprint,
            MigrationPlanId = options.MigrationPlanId,
            MigrationPlanVersion = options.MigrationPlanVersion,
            CredentialTarget = options.CredentialTarget,
            EventLogDefaultTransitionTimeoutSeconds = seconds
        };
    }

    public static DesktopNodeHostOptions WithDataRoot(this DesktopNodeHostOptions options, string dataRoot, bool removeData = false)
    {
        return new DesktopNodeHostOptions
        {
            Mode = options.Mode,
            Prefix = options.Prefix,
            WebRootPath = options.WebRootPath,
            JobStorePath = options.JobStorePath,
            EventLogPath = options.EventLogPath,
            ApiTokenFile = options.ApiTokenFile,
            ApiTokenProtectedFile = options.ApiTokenProtectedFile,
            ApiTokenCredentialTarget = options.ApiTokenCredentialTarget,
            AllowLan = options.AllowLan,
            ServiceAction = options.ServiceAction,
            ProductRoot = options.ProductRoot,
            DataRoot = dataRoot,
            ServiceExecutablePath = options.ServiceExecutablePath,
            RemoveData = removeData,
            DryRun = options.DryRun,
            ReleaseApproved = options.ReleaseApproved,
            FirewallRuleName = options.FirewallRuleName,
            FirewallLocalPort = options.FirewallLocalPort,
            FirewallProfile = options.FirewallProfile,
            FirewallRemoteAddress = options.FirewallRemoteAddress,
            TrustRootCertificatePath = options.TrustRootCertificatePath,
            TrustRootCertificateThumbprint = options.TrustRootCertificateThumbprint,
            TrustPublisherCertificatePath = options.TrustPublisherCertificatePath,
            TrustPublisherCertificateThumbprint = options.TrustPublisherCertificateThumbprint,
            MigrationPlanId = options.MigrationPlanId,
            MigrationPlanVersion = options.MigrationPlanVersion,
            CredentialTarget = options.CredentialTarget
        };
    }

    public static DesktopNodeHostOptions WithAllowLan(this DesktopNodeHostOptions options)
    {
        return new DesktopNodeHostOptions
        {
            Mode = options.Mode,
            Prefix = options.Prefix,
            WebRootPath = options.WebRootPath,
            JobStorePath = options.JobStorePath,
            EventLogPath = options.EventLogPath,
            ApiTokenFile = options.ApiTokenFile,
            ApiTokenProtectedFile = options.ApiTokenProtectedFile,
            ApiTokenCredentialTarget = options.ApiTokenCredentialTarget,
            AllowLan = true,
            ServiceAction = options.ServiceAction,
            ProductRoot = options.ProductRoot,
            DataRoot = options.DataRoot,
            ServiceExecutablePath = options.ServiceExecutablePath,
            RemoveData = options.RemoveData,
            DryRun = options.DryRun,
            ReleaseApproved = options.ReleaseApproved,
            FirewallRuleName = options.FirewallRuleName,
            FirewallLocalPort = options.FirewallLocalPort,
            FirewallProfile = options.FirewallProfile,
            FirewallRemoteAddress = options.FirewallRemoteAddress,
            TrustRootCertificatePath = options.TrustRootCertificatePath,
            TrustRootCertificateThumbprint = options.TrustRootCertificateThumbprint,
            TrustPublisherCertificatePath = options.TrustPublisherCertificatePath,
            TrustPublisherCertificateThumbprint = options.TrustPublisherCertificateThumbprint,
            MigrationPlanId = options.MigrationPlanId,
            MigrationPlanVersion = options.MigrationPlanVersion,
            CredentialTarget = options.CredentialTarget
        };
    }

    public static DesktopNodeHostOptions WithReleaseApproved(this DesktopNodeHostOptions options)
    {
        return new DesktopNodeHostOptions
        {
            Mode = options.Mode,
            Prefix = options.Prefix,
            WebRootPath = options.WebRootPath,
            JobStorePath = options.JobStorePath,
            EventLogPath = options.EventLogPath,
            ApiTokenFile = options.ApiTokenFile,
            ApiTokenProtectedFile = options.ApiTokenProtectedFile,
            ApiTokenCredentialTarget = options.ApiTokenCredentialTarget,
            AllowLan = options.AllowLan,
            ServiceAction = options.ServiceAction,
            ProductRoot = options.ProductRoot,
            DataRoot = options.DataRoot,
            ServiceExecutablePath = options.ServiceExecutablePath,
            RemoveData = options.RemoveData,
            DryRun = options.DryRun,
            ReleaseApproved = true,
            FirewallRuleName = options.FirewallRuleName,
            FirewallLocalPort = options.FirewallLocalPort,
            FirewallProfile = options.FirewallProfile,
            FirewallRemoteAddress = options.FirewallRemoteAddress,
            TrustRootCertificatePath = options.TrustRootCertificatePath,
            TrustRootCertificateThumbprint = options.TrustRootCertificateThumbprint,
            TrustPublisherCertificatePath = options.TrustPublisherCertificatePath,
            TrustPublisherCertificateThumbprint = options.TrustPublisherCertificateThumbprint,
            MigrationPlanId = options.MigrationPlanId,
            MigrationPlanVersion = options.MigrationPlanVersion,
            CredentialTarget = options.CredentialTarget
        };
    }

    public static DesktopNodeHostOptions WithTrustStoreCertificateInputs(
        this DesktopNodeHostOptions options,
        bool certificatePaths = true)
    {
        return new DesktopNodeHostOptions
        {
            Mode = options.Mode,
            Prefix = options.Prefix,
            WebRootPath = options.WebRootPath,
            JobStorePath = options.JobStorePath,
            EventLogPath = options.EventLogPath,
            ApiTokenFile = options.ApiTokenFile,
            ApiTokenProtectedFile = options.ApiTokenProtectedFile,
            ApiTokenCredentialTarget = options.ApiTokenCredentialTarget,
            AllowLan = options.AllowLan,
            ServiceAction = options.ServiceAction,
            ProductRoot = options.ProductRoot,
            DataRoot = options.DataRoot,
            ServiceExecutablePath = options.ServiceExecutablePath,
            RemoveData = options.RemoveData,
            DryRun = options.DryRun,
            ReleaseApproved = options.ReleaseApproved,
            FirewallRuleName = options.FirewallRuleName,
            FirewallLocalPort = options.FirewallLocalPort,
            FirewallProfile = options.FirewallProfile,
            FirewallRemoteAddress = options.FirewallRemoteAddress,
            TrustRootCertificatePath = certificatePaths ? "C:\\certs\\PureCVisor-Internal-CodeSigning-Root.cer" : null,
            TrustRootCertificateThumbprint = "00112233445566778899AABBCCDDEEFF00112233",
            TrustPublisherCertificatePath = certificatePaths ? "C:\\certs\\PureCVisor-DesktopNode-Internal-CodeSigning.cer" : null,
            TrustPublisherCertificateThumbprint = "AABBCCDDEEFF00112233445566778899AABBCCDD",
            MigrationPlanId = options.MigrationPlanId,
            MigrationPlanVersion = options.MigrationPlanVersion,
            CredentialTarget = options.CredentialTarget
        };
    }
}
