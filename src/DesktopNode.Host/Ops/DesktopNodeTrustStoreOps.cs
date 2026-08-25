using DesktopNode.Host;

namespace DesktopNode.Host.Ops;

internal static class DesktopNodeTrustStoreOps
{
    public const string OperationFamily = "trust-store";

    public static bool Owns(string? operation)
    {
        return DesktopNodeHostOpsCatalog.OperationBelongsTo(operation, OperationFamily);
    }

    public static DesktopNodeHostServiceActionResult Execute(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IDesktopNodeWindowsTrustStoreController trustStoreController)
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

        if (!plan.ReleaseApproved)
        {
            return NativeTrustStoreFailure(
                options,
                plan,
                [],
                "PCV_HOST_TRUST_STORE_RELEASE_APPROVAL_REQUIRED",
                "Trust store mutation requires explicit --release-approved approval.");
        }

        var certificates = plan.TrustStoreCertificates ?? [];
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> current = [];
        try
        {
            current = trustStoreController.Query(certificates);
            if (current.Any(certificate => certificate.SubjectCollision || certificate.Exists && !certificate.Owned))
            {
                return NativeTrustStoreFailure(
                    options,
                    plan,
                    current,
                    "PCV_HOST_TRUST_STORE_CERTIFICATE_OWNERSHIP_MISMATCH",
                    "Certificate store contains a conflicting certificate for the Desktop Node trust contract.");
            }

            if (string.Equals(plan.NativeTrustStoreOperation, "trust-store-install", StringComparison.OrdinalIgnoreCase))
            {
                var next = current.All(certificate => certificate.Exists && certificate.Owned)
                    ? current
                    : trustStoreController.Install(certificates);
                var ok = next.Count == certificates.Count && next.All(certificate => certificate.Exists && certificate.Owned);
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_TRUST_STORE_INSTALL_FAILED",
                    ErrorMessage: ok ? null : "Certificate store install did not produce the expected Root/TrustedPublisher bindings.",
                    TrustStoreCertificates: next);
            }

            if (string.Equals(plan.NativeTrustStoreOperation, "trust-store-remove", StringComparison.OrdinalIgnoreCase))
            {
                var next = current.Any(certificate => certificate.Exists)
                    ? trustStoreController.Remove(certificates)
                    : current;
                var ok = next.Count == certificates.Count && next.All(certificate => !certificate.Exists);
                return new DesktopNodeHostServiceActionResult(
                    Ok: ok,
                    Action: options.ServiceAction ?? string.Empty,
                    Plan: plan,
                    Commands: [],
                    RemovedPaths: [],
                    PreparedTokenPath: null,
                    Service: null,
                    ServiceOwnerVerified: false,
                    ErrorCode: ok ? null : "PCV_HOST_TRUST_STORE_REMOVE_FAILED",
                    ErrorMessage: ok ? null : "Certificate store remove did not clear the expected Root/TrustedPublisher bindings.",
                    TrustStoreCertificates: next);
            }

            return NativeTrustStoreFailure(
                options,
                plan,
                current,
                "PCV_HOST_TRUST_STORE_ACTION_INVALID",
                $"Trust store action '{plan.NativeTrustStoreOperation}' is not supported.");
        }
        catch (DesktopNodeWindowsTrustStoreControllerException error)
        {
            return NativeTrustStoreFailure(options, plan, current, error.Code, error.Message);
        }
    }

    private static DesktopNodeHostServiceActionResult NativeTrustStoreFailure(
        DesktopNodeHostOptions options,
        DesktopNodeHostServiceActionPlan plan,
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> trustStoreCertificates,
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
            TrustStoreCertificates: trustStoreCertificates);
    }
}
