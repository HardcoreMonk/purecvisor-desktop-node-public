using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DesktopNode.Host;

public sealed record DesktopNodeWindowsTrustStoreCertificateSpec(
    string StoreName,
    string StoreLocation,
    string ExpectedSubject,
    string Thumbprint,
    string? CertificatePath);

public sealed record DesktopNodeWindowsTrustStoreCertificateSnapshot(
    string StoreName,
    string StoreLocation,
    string Thumbprint,
    bool Exists,
    string? Subject,
    string? Issuer,
    string? SerialNumber,
    bool Owned,
    bool SubjectCollision = false,
    string? CollisionThumbprint = null);

public interface IDesktopNodeWindowsTrustStoreController
{
    IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Query(
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates);

    IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Install(
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates);

    IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Remove(
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates);
}

public sealed class DesktopNodeWindowsTrustStoreController : IDesktopNodeWindowsTrustStoreController
{
    public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Query(
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates)
    {
        return certificates.Select(QueryOne).ToArray();
    }

    public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Install(
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates)
    {
        try
        {
            foreach (var spec in certificates)
            {
                if (string.IsNullOrWhiteSpace(spec.CertificatePath))
                {
                    throw new DesktopNodeWindowsTrustStoreControllerException(
                        "PCV_HOST_TRUST_STORE_CERTIFICATE_PATH_REQUIRED",
                        $"Certificate path is required for LocalMachine\\{spec.StoreName} trust install.");
                }

                var certificate = LoadCertificate(spec.CertificatePath);
                ValidateCertificateIdentity(spec, certificate);
                var current = QueryOne(spec);
                if (current.SubjectCollision || current.Exists && !current.Owned)
                {
                    throw new DesktopNodeWindowsTrustStoreControllerException(
                        "PCV_HOST_TRUST_STORE_CERTIFICATE_OWNERSHIP_MISMATCH",
                        $"Certificate store LocalMachine\\{spec.StoreName} contains a conflicting certificate for '{spec.ExpectedSubject}'.");
                }

                if (current.Exists && current.Owned)
                {
                    continue;
                }

                using var store = OpenStore(spec, OpenFlags.ReadWrite);
                store.Add(certificate);
            }

            return Query(certificates);
        }
        catch (DesktopNodeWindowsTrustStoreControllerException)
        {
            throw;
        }
        catch (Exception error) when (error is CryptographicException or UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            throw new DesktopNodeWindowsTrustStoreControllerException(
                "PCV_HOST_TRUST_STORE_INSTALL_FAILED",
                $"Certificate store install failed: {error.Message}",
                error);
        }
    }

    public IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSnapshot> Remove(
        IReadOnlyList<DesktopNodeWindowsTrustStoreCertificateSpec> certificates)
    {
        try
        {
            foreach (var spec in certificates)
            {
                var current = QueryOne(spec);
                if (!current.Exists && !current.SubjectCollision)
                {
                    continue;
                }

                if (!current.Owned)
                {
                    throw new DesktopNodeWindowsTrustStoreControllerException(
                        "PCV_HOST_TRUST_STORE_CERTIFICATE_OWNERSHIP_MISMATCH",
                        $"Certificate store LocalMachine\\{spec.StoreName} contains a conflicting certificate for '{spec.ExpectedSubject}'.");
                }

                using var store = OpenStore(spec, OpenFlags.ReadWrite);
                var matches = store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    NormalizeThumbprint(spec.Thumbprint),
                    validOnly: false);
                foreach (var certificate in matches)
                {
                    store.Remove(certificate);
                }
            }

            return Query(certificates);
        }
        catch (DesktopNodeWindowsTrustStoreControllerException)
        {
            throw;
        }
        catch (Exception error) when (error is CryptographicException or UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            throw new DesktopNodeWindowsTrustStoreControllerException(
                "PCV_HOST_TRUST_STORE_REMOVE_FAILED",
                $"Certificate store remove failed: {error.Message}",
                error);
        }
    }

    private static DesktopNodeWindowsTrustStoreCertificateSnapshot QueryOne(
        DesktopNodeWindowsTrustStoreCertificateSpec spec)
    {
        try
        {
            using var store = OpenStore(spec, OpenFlags.ReadOnly);
            var expectedThumbprint = NormalizeThumbprint(spec.Thumbprint);
            var matches = store.Certificates.Find(
                X509FindType.FindByThumbprint,
                expectedThumbprint,
                validOnly: false);
            if (matches.Count > 0)
            {
                var certificate = matches[0];
                return SnapshotFromCertificate(spec, certificate, exists: true);
            }

            var collision = store.Certificates
                .OfType<X509Certificate2>()
                .FirstOrDefault(certificate =>
                    string.Equals(certificate.Subject, spec.ExpectedSubject, StringComparison.OrdinalIgnoreCase));
            return new DesktopNodeWindowsTrustStoreCertificateSnapshot(
                StoreName: spec.StoreName,
                StoreLocation: spec.StoreLocation,
                Thumbprint: expectedThumbprint,
                Exists: false,
                Subject: null,
                Issuer: null,
                SerialNumber: null,
                Owned: false,
                SubjectCollision: collision is not null,
                CollisionThumbprint: collision?.Thumbprint);
        }
        catch (DesktopNodeWindowsTrustStoreControllerException)
        {
            throw;
        }
        catch (Exception error) when (error is CryptographicException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            throw new DesktopNodeWindowsTrustStoreControllerException(
                "PCV_HOST_TRUST_STORE_QUERY_FAILED",
                $"Certificate store LocalMachine\\{spec.StoreName} could not be queried: {error.Message}",
                error);
        }
    }

    private static DesktopNodeWindowsTrustStoreCertificateSnapshot SnapshotFromCertificate(
        DesktopNodeWindowsTrustStoreCertificateSpec spec,
        X509Certificate2 certificate,
        bool exists)
    {
        var thumbprint = NormalizeThumbprint(spec.Thumbprint);
        return new DesktopNodeWindowsTrustStoreCertificateSnapshot(
            StoreName: spec.StoreName,
            StoreLocation: spec.StoreLocation,
            Thumbprint: thumbprint,
            Exists: exists,
            Subject: exists ? certificate.Subject : null,
            Issuer: exists ? certificate.Issuer : null,
            SerialNumber: exists ? certificate.SerialNumber : null,
            Owned: exists &&
                string.Equals(NormalizeThumbprint(certificate.Thumbprint), thumbprint, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(certificate.Subject, spec.ExpectedSubject, StringComparison.OrdinalIgnoreCase));
    }

    private static X509Certificate2 LoadCertificate(string certificatePath)
    {
        try
        {
            var certificate = X509CertificateLoader.LoadCertificateFromFile(certificatePath);
            if (certificate.HasPrivateKey)
            {
                throw new DesktopNodeWindowsTrustStoreControllerException(
                    "PCV_HOST_TRUST_STORE_PRIVATE_KEY_FORBIDDEN",
                    "Trust store install accepts only public certificate artifacts.");
            }

            return certificate;
        }
        catch (DesktopNodeWindowsTrustStoreControllerException)
        {
            throw;
        }
        catch (Exception error) when (error is CryptographicException or IOException or UnauthorizedAccessException)
        {
            throw new DesktopNodeWindowsTrustStoreControllerException(
                "PCV_HOST_TRUST_STORE_CERTIFICATE_LOAD_FAILED",
                $"Certificate artifact '{certificatePath}' could not be loaded: {error.Message}",
                error);
        }
    }

    private static void ValidateCertificateIdentity(
        DesktopNodeWindowsTrustStoreCertificateSpec spec,
        X509Certificate2 certificate)
    {
        if (!string.Equals(NormalizeThumbprint(certificate.Thumbprint), NormalizeThumbprint(spec.Thumbprint), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(certificate.Subject, spec.ExpectedSubject, StringComparison.OrdinalIgnoreCase))
        {
            throw new DesktopNodeWindowsTrustStoreControllerException(
                "PCV_HOST_TRUST_STORE_CERTIFICATE_IDENTITY_MISMATCH",
                $"Certificate artifact for LocalMachine\\{spec.StoreName} does not match the expected subject/thumbprint.");
        }
    }

    private static X509Store OpenStore(DesktopNodeWindowsTrustStoreCertificateSpec spec, OpenFlags flags)
    {
        var store = new X509Store(ParseStoreName(spec.StoreName), ParseStoreLocation(spec.StoreLocation));
        store.Open(flags);
        return store;
    }

    private static StoreName ParseStoreName(string storeName)
    {
        return storeName.ToLowerInvariant() switch
        {
            "root" => StoreName.Root,
            "trustedpublisher" => StoreName.TrustedPublisher,
            _ => throw new DesktopNodeWindowsTrustStoreControllerException(
                "PCV_HOST_TRUST_STORE_NAME_INVALID",
                $"Certificate store '{storeName}' is not supported.")
        };
    }

    private static StoreLocation ParseStoreLocation(string storeLocation)
    {
        return storeLocation.ToLowerInvariant() switch
        {
            "localmachine" => StoreLocation.LocalMachine,
            _ => throw new DesktopNodeWindowsTrustStoreControllerException(
                "PCV_HOST_TRUST_STORE_LOCATION_INVALID",
                $"Certificate store location '{storeLocation}' is not supported.")
        };
    }

    private static string NormalizeThumbprint(string thumbprint)
    {
        return thumbprint.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(":", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant();
    }
}

public sealed class DesktopNodeWindowsTrustStoreControllerException : Exception
{
    public DesktopNodeWindowsTrustStoreControllerException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
