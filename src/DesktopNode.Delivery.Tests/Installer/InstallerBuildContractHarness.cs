using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DesktopNode.Delivery.Tests.Infrastructure;

namespace DesktopNode.Delivery.Tests.Installer;

internal sealed record InstallerBuildInput(string Version, string OutputRoot)
{
    internal string? MsiProductVersion { get; init; }
    internal string? DesktopNodeHostPath { get; init; }
    internal string? DesktopNodeCliPath { get; init; }
    internal string SigningMode { get; init; } = "RequireSigned";
    internal string SigningTrustModel { get; init; } = "Unspecified";
    internal string? SignToolPath { get; init; }
    internal string? CertificateThumbprint { get; init; }
    internal string? CertificatePath { get; init; }
    internal string? TimestampUrl { get; init; }
    internal bool DryRun { get; init; }
    internal int WixExitCode { get; init; }
    internal string WixStdout { get; init; } = string.Empty;
    internal string WixStderr { get; init; } = string.Empty;
}

internal sealed record InstallerPublicationProjection(
    string SchemaVersion,
    string Mode,
    string PublicTrustedSigning,
    string ExternalStablePublication,
    string BurnBootstrapper,
    string Msix,
    string WingetManifest,
    string CatalogPublication);

internal sealed record InstallerBuildPlanProjection(
    string ProductName,
    string Version,
    string ReleaseChannel,
    string MsiProductVersion,
    string ArtifactArchitecture,
    string ArtifactBaseName,
    string OutputRoot,
    string PayloadRoot,
    string MsiPath,
    string ProvenancePath,
    string MsiSha256Path,
    string PublicationPath,
    IReadOnlyList<string> WixSourceFiles,
    string SigningMode,
    string SigningTrustModel,
    string ServiceHostSource,
    string? ServiceHostPath,
    string? ServiceHostSha256,
    string CliSource,
    string? CliPath,
    string? CliSha256,
    InstallerPublicationProjection Publication);

internal sealed record InstallerBuildErrorProjection(string Code, string Message);

internal sealed record InstallerToolOutputProjection(
    int ExitCode,
    string Stdout,
    string Stderr,
    IReadOnlyList<string> Arguments);

internal sealed record InstallerBuildProvenanceProjection(
    string GitCommit,
    string ReleaseChannel,
    string MsiPath,
    string MsiSha256,
    int PayloadFileCount,
    string ServiceHostSource,
    string ServiceHostPath,
    string ServiceHostSha256,
    string CliSource,
    string CliPath,
    string CliSha256,
    string SigningTrustModel,
    InstallerPublicationProjection Publication,
    bool HasTuiProperty);

internal sealed record InstallerPublicationDescriptorProjection(
    string SchemaVersion,
    string ProductVersion,
    string ArtifactBaseName,
    string MsiSha256,
    string ProvenancePath,
    InstallerPublicationProjection Publication);

internal sealed record InstallerBuildResultProjection(
    bool Ok,
    int ExitCode,
    string Json,
    InstallerBuildErrorProjection? Error,
    InstallerBuildPlanProjection? Plan,
    InstallerBuildProvenanceProjection? Provenance,
    InstallerPublicationDescriptorProjection? PublicationDescriptor,
    InstallerToolOutputProjection? WixOutput,
    IReadOnlyList<string> WixArguments,
    bool DotnetPublishRequired);

internal sealed record InstallerBuildSourceBoundary(
    IReadOnlySet<string> Parameters,
    bool SelfContained,
    bool SingleFile,
    bool HasMonotonicFileVersion,
    bool HasCliPayload,
    bool HasWebPayload,
    bool HasTuiReference,
    bool HasSpikePayloadSource);

internal sealed partial class InstallerBuildContractHarness
{
    private const string BuildScriptPath = "packaging/windows-desktop-node/installer/build.ps1";
    private const string BuildModulePath =
        "packaging/windows-desktop-node/installer/PcvDesktopNodeInstaller.Build.psm1";
    private const string ProductWxsPath = "packaging/windows-desktop-node/installer/Product.wxs";
    private const string ErrorCode = "PCV_INSTALLER_PLAN_INVALID";

    private readonly RepositoryContractContext repository;
    private readonly string allowedTemporaryRoot;
    private readonly string buildScript;
    private readonly string buildModule;
    private readonly string productWxs;

    internal InstallerBuildContractHarness(
        RepositoryContractContext repository,
        string allowedTemporaryRoot)
    {
        this.repository = repository;
        this.allowedTemporaryRoot = Path.GetFullPath(allowedTemporaryRoot);
        buildScript = repository.ReadUtf8Text(BuildScriptPath);
        buildModule = repository.ReadUtf8Text(BuildModulePath);
        productWxs = repository.ReadUtf8Text(ProductWxsPath);
        SourceBoundary = InstallerBuildSourcePolicy.Validate(buildScript, buildModule, productWxs);
    }

    internal InstallerBuildSourceBoundary SourceBoundary { get; }

    internal string CurrentGitCommit => ReadGitCommit(repository.RootPath);

    internal string ProductWxs => productWxs;

    internal InstallerBuildResultProjection Execute(InstallerBuildInput input)
    {
        var signingMode = string.IsNullOrWhiteSpace(input.SigningMode) ? "RequireSigned" : input.SigningMode;
        var trustModel = string.IsNullOrWhiteSpace(input.SigningTrustModel) ? "Unspecified" : input.SigningTrustModel;
        if (signingMode is not ("RequireSigned" or "AllowUnsignedDev"))
        {
            return Failure(1, "PCV_INSTALLER_SIGNING_MODE_INVALID", "Signing mode is invalid.");
        }

        if (signingMode == "RequireSigned")
        {
            var hasCertificate = !string.IsNullOrWhiteSpace(input.CertificateThumbprint) ||
                !string.IsNullOrWhiteSpace(input.CertificatePath);
            if (string.IsNullOrWhiteSpace(input.SignToolPath) ||
                !hasCertificate ||
                string.IsNullOrWhiteSpace(input.TimestampUrl))
            {
                return Failure(
                    1,
                    "PCV_INSTALLER_SIGNING_REQUIRED",
                    "RequireSigned builds require SignToolPath, certificate input, and TimestampUrl.");
            }

            if (trustModel == "Unspecified")
            {
                return Failure(
                    1,
                    "PCV_INSTALLER_SIGNING_TRUST_MODEL_REQUIRED",
                    "RequireSigned builds require an explicit SigningTrustModel.");
            }
        }

        if (!TryResolveMsiProductVersion(input.Version, input.MsiProductVersion, out var msiVersion))
        {
            return Failure(1, "PCV_INSTALLER_INVALID_VERSION", "Version is not MSI compatible.");
        }

        if (!TryResolveReleaseChannel(input.Version, out var releaseChannel))
        {
            return Failure(1, "PCV_INSTALLER_INVALID_RELEASE_CHANNEL", "Release channel is invalid.");
        }

        if (releaseChannel is "rc" or "stable" && signingMode != "RequireSigned")
        {
            return Failure(
                1,
                "PCV_INSTALLER_RELEASE_SIGNING_REQUIRED",
                "RC and stable installer artifacts require RequireSigned signing mode.");
        }

        string? hostPath = null;
        string? hostHash = null;
        var hostSource = "dotnet-publish";
        if (!string.IsNullOrWhiteSpace(input.DesktopNodeHostPath))
        {
            if (!File.Exists(input.DesktopNodeHostPath))
            {
                return Failure(
                    1,
                    "PCV_INSTALLER_SERVICE_HOST_NOT_FOUND",
                    $"DesktopNode.Host payload was not found: {input.DesktopNodeHostPath}");
            }
            hostPath = Path.GetFullPath(input.DesktopNodeHostPath);
            hostHash = Sha256(hostPath);
            hostSource = "explicit-path";
        }

        string? cliPath = null;
        string? cliHash = null;
        var cliSource = "dotnet-publish";
        if (!string.IsNullOrWhiteSpace(input.DesktopNodeCliPath))
        {
            if (!File.Exists(input.DesktopNodeCliPath))
            {
                return Failure(
                    1,
                    "PCV_INSTALLER_CLI_NOT_FOUND",
                    $"DesktopNode.Cli payload was not found: {input.DesktopNodeCliPath}");
            }
            cliPath = Path.GetFullPath(input.DesktopNodeCliPath);
            cliHash = Sha256(cliPath);
            cliSource = "explicit-path";
        }

        var outputRoot = Path.GetFullPath(input.OutputRoot);
        EnsureWithin(allowedTemporaryRoot, outputRoot, "output-root");
        var payloadRoot = Path.Combine(outputRoot, "payload");
        EnsurePayloadRootContained(outputRoot, payloadRoot);
        var artifactBaseName = $"PureCVisorDesktopNode-{input.Version}-windows-x64";
        var msiPath = Path.Combine(outputRoot, $"{artifactBaseName}.msi");
        var provenancePath = Path.Combine(outputRoot, $"{artifactBaseName}.provenance.json");
        var shaPath = Path.Combine(outputRoot, $"{artifactBaseName}.msi.sha256");
        var publicationPath = Path.Combine(outputRoot, $"{artifactBaseName}.publication.json");
        var wixSources = new[]
        {
            Path.Combine(repository.RootPath, "packaging", "windows-desktop-node", "installer", "Product.wxs"),
            Path.Combine(repository.RootPath, "packaging", "windows-desktop-node", "installer", "ProductActions.wxs"),
        };
        var publication = Publication();
        var plan = new InstallerBuildPlanProjection(
            "PureCVisor Desktop Node",
            input.Version,
            releaseChannel,
            msiVersion,
            "windows-x64",
            artifactBaseName,
            outputRoot,
            payloadRoot,
            msiPath,
            provenancePath,
            shaPath,
            publicationPath,
            wixSources,
            signingMode,
            trustModel,
            hostSource,
            hostPath,
            hostHash,
            cliSource,
            cliPath,
            cliHash,
            publication);

        if (input.DryRun)
        {
            return Success(plan, null, null, null, [], false);
        }

        Directory.CreateDirectory(outputRoot);
        var dotnetRequired = hostPath is null || cliPath is null;
        if (hostPath is null)
        {
            var hostPublishRoot = Path.Combine(outputRoot, "host-publish");
            Directory.CreateDirectory(hostPublishRoot);
            hostPath = Path.Combine(hostPublishRoot, "DesktopNode.Host.exe");
            File.WriteAllText(hostPath, "fake-DesktopNode.Host.exe", new UTF8Encoding(false));
            hostHash = Sha256(hostPath);
        }
        if (cliPath is null)
        {
            var cliPublishRoot = Path.Combine(outputRoot, "cli-publish");
            Directory.CreateDirectory(cliPublishRoot);
            cliPath = Path.Combine(cliPublishRoot, "pcvcli.exe");
            File.WriteAllText(cliPath, "fake-pcvcli.exe", new UTF8Encoding(false));
            cliHash = Sha256(cliPath);
        }

        if (Directory.Exists(payloadRoot))
        {
            EnsurePayloadRootContained(outputRoot, payloadRoot);
            Directory.Delete(payloadRoot, recursive: true);
        }
        Directory.CreateDirectory(payloadRoot);
        CopyPayload("packaging/windows-desktop-node/Invoke-PcvDesktopNodeProduct.ps1", payloadRoot, "Invoke-PcvDesktopNodeProduct.ps1");
        CopyPayload("packaging/windows-desktop-node/PcvDesktopNodeProduct.psm1", payloadRoot, "PcvDesktopNodeProduct.psm1");
        File.Copy(hostPath, Path.Combine(payloadRoot, "DesktopNode.Host.exe"), overwrite: true);
        File.Copy(cliPath, Path.Combine(payloadRoot, "pcvcli.exe"), overwrite: true);
        CopyPayload("web/app.js", payloadRoot, Path.Combine("web", "app.js"));
        CopyPayload("web/index.html", payloadRoot, Path.Combine("web", "index.html"));
        CopyPayload("web/styles.css", payloadRoot, Path.Combine("web", "styles.css"));

        var productManifest = new JsonObject
        {
            ["schema_version"] = 2,
            ["version"] = input.Version,
            ["service_host"] = new JsonObject { ["mode"] = "dotnet-windows-service" },
            ["update"] = new JsonObject { ["installed_manifest_is_source_of_truth"] = true },
        };
        File.WriteAllText(
            Path.Combine(payloadRoot, "product-manifest.json"),
            productManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
            new UTF8Encoding(false));

        var wixArguments = new[]
        {
            "build",
            wixSources[0],
            wixSources[1],
            "-arch",
            "x64",
            "-define",
            $"MsiProductVersion={msiVersion}",
            "-define",
            $"PayloadRoot={payloadRoot}",
            "-out",
            msiPath,
        };
        var wixOutput = new InstallerToolOutputProjection(
            input.WixExitCode,
            input.WixStdout,
            input.WixStderr,
            wixArguments);
        if (input.WixExitCode != 0)
        {
            return Failure(
                input.WixExitCode,
                "PCV_INSTALLER_WIX_BUILD_FAILED",
                "WiX build failed.",
                wixOutput,
                wixArguments,
                dotnetRequired);
        }

        File.WriteAllText(msiPath, "fake-msi", new UTF8Encoding(false));
        var msiHash = Sha256(msiPath);
        var payloadCount = Directory.EnumerateFiles(payloadRoot, "*", SearchOption.AllDirectories).Count();
        var provenance = new InstallerBuildProvenanceProjection(
            CurrentGitCommit,
            releaseChannel,
            msiPath,
            msiHash,
            payloadCount,
            hostSource,
            hostPath,
            hostHash!,
            cliSource,
            cliPath,
            cliHash!,
            trustModel,
            publication,
            false);
        var descriptor = new InstallerPublicationDescriptorProjection(
            "1",
            input.Version,
            artifactBaseName,
            msiHash,
            provenancePath,
            publication);

        File.WriteAllText(provenancePath, SerializeProvenance(provenance), new UTF8Encoding(false));
        File.WriteAllText(publicationPath, SerializeDescriptor(descriptor), new UTF8Encoding(false));
        File.WriteAllText(shaPath, $"{msiHash}  {Path.GetFileName(msiPath)}{Environment.NewLine}", Encoding.ASCII);

        var finalPlan = plan with
        {
            ServiceHostPath = hostPath,
            ServiceHostSha256 = hostHash,
            CliPath = cliPath,
            CliSha256 = cliHash,
        };
        return Success(finalPlan, provenance, descriptor, wixOutput, wixArguments, dotnetRequired);
    }

    internal static void EnsurePayloadRootContained(string outputRoot, string payloadRoot)
    {
        var fullOutput = Path.GetFullPath(outputRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullPayload = Path.GetFullPath(payloadRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Path.GetFileName(fullPayload).Equals("payload", StringComparison.OrdinalIgnoreCase) ||
            !fullPayload.StartsWith(fullOutput + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid("payload-root");
        }
    }

    private void CopyPayload(string sourcePath, string payloadRoot, string relativeDestination)
    {
        var destination = Path.Combine(payloadRoot, relativeDestination);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(Path.Combine(repository.RootPath, sourcePath.Replace('/', Path.DirectorySeparatorChar)), destination, overwrite: true);
    }

    private static void EnsureWithin(string parent, string child, string detail)
    {
        var fullParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fullChild = Path.GetFullPath(child).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!fullChild.StartsWith(fullParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid(detail);
        }
    }

    private static bool TryResolveMsiProductVersion(
        string version,
        string? explicitVersion,
        out string result)
    {
        var candidate = string.IsNullOrWhiteSpace(explicitVersion)
            ? VersionPrefixRegex().Match(version).Value
            : explicitVersion;
        var match = ExactMsiVersionRegex().Match(candidate ?? string.Empty);
        if (!match.Success ||
            int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) >= 256 ||
            int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) >= 256 ||
            int.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture) >= 65536)
        {
            result = string.Empty;
            return false;
        }
        result = candidate!;
        return true;
    }

    private static bool TryResolveReleaseChannel(string version, out string channel)
    {
        if (DevVersionRegex().IsMatch(version))
        {
            channel = "dev";
            return true;
        }
        if (AdminSmokeVersionRegex().IsMatch(version))
        {
            channel = "admin-smoke";
            return true;
        }
        if (RcVersionRegex().IsMatch(version))
        {
            channel = "rc";
            return true;
        }
        if (StableVersionRegex().IsMatch(version))
        {
            channel = "stable";
            return true;
        }
        channel = string.Empty;
        return false;
    }

    private static InstallerPublicationProjection Publication() =>
        new(
            "1",
            "internal-artifact-descriptor-only",
            "not-claimed",
            "not-claimed",
            "not-built",
            "not-built",
            "not-generated",
            "not-published");

    private static InstallerBuildResultProjection Success(
        InstallerBuildPlanProjection plan,
        InstallerBuildProvenanceProjection? provenance,
        InstallerPublicationDescriptorProjection? descriptor,
        InstallerToolOutputProjection? wixOutput,
        IReadOnlyList<string> wixArguments,
        bool dotnetRequired)
    {
        var json = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["ok"] = true,
            ["dry_run"] = provenance is null,
        });
        return new InstallerBuildResultProjection(
            true,
            0,
            json,
            null,
            plan,
            provenance,
            descriptor,
            wixOutput,
            wixArguments,
            dotnetRequired);
    }

    private static InstallerBuildResultProjection Failure(
        int exitCode,
        string code,
        string message,
        InstallerToolOutputProjection? wixOutput = null,
        IReadOnlyList<string>? wixArguments = null,
        bool dotnetRequired = false)
    {
        var error = new InstallerBuildErrorProjection(code, message);
        var payload = new Dictionary<string, object?>
        {
            ["ok"] = false,
            ["error"] = new Dictionary<string, object?> { ["code"] = code, ["message"] = message },
        };
        if (wixOutput is not null)
        {
            payload["tool_output"] = new Dictionary<string, object?>
            {
                ["wix"] = new Dictionary<string, object?>
                {
                    ["exit_code"] = wixOutput.ExitCode,
                    ["stdout"] = wixOutput.Stdout,
                    ["stderr"] = wixOutput.Stderr,
                },
            };
        }
        return new InstallerBuildResultProjection(
            false,
            exitCode,
            JsonSerializer.Serialize(payload),
            error,
            null,
            null,
            null,
            wixOutput,
            wixArguments ?? [],
            dotnetRequired);
    }

    private static string SerializeProvenance(InstallerBuildProvenanceProjection value) =>
        new JsonObject
        {
            ["schema_version"] = "1",
            ["product"] = new JsonObject { ["release_channel"] = value.ReleaseChannel },
            ["git_commit"] = value.GitCommit,
            ["msi"] = new JsonObject { ["path"] = value.MsiPath, ["sha256"] = value.MsiSha256 },
            ["payload"] = new JsonObject { ["file_count"] = value.PayloadFileCount },
            ["service_host"] = new JsonObject { ["source"] = value.ServiceHostSource, ["source_path"] = value.ServiceHostPath, ["sha256"] = value.ServiceHostSha256 },
            ["cli"] = new JsonObject { ["source"] = value.CliSource, ["source_path"] = value.CliPath, ["sha256"] = value.CliSha256 },
            ["signing_trust_model"] = value.SigningTrustModel,
            ["publication"] = PublicationJson(value.Publication),
        }.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

    private static string SerializeDescriptor(InstallerPublicationDescriptorProjection value) =>
        new JsonObject
        {
            ["schema_version"] = value.SchemaVersion,
            ["product"] = new JsonObject { ["version"] = value.ProductVersion },
            ["artifact"] = new JsonObject
            {
                ["base_name"] = value.ArtifactBaseName,
                ["msi_sha256"] = value.MsiSha256,
                ["provenance_path"] = value.ProvenancePath,
            },
            ["publication"] = PublicationJson(value.Publication),
        }.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

    private static JsonObject PublicationJson(InstallerPublicationProjection value) =>
        new()
        {
            ["schema_version"] = value.SchemaVersion,
            ["mode"] = value.Mode,
            ["public_trusted_signing"] = value.PublicTrustedSigning,
            ["external_stable_publication"] = value.ExternalStablePublication,
            ["burn_bootstrapper"] = value.BurnBootstrapper,
            ["msix"] = value.Msix,
            ["winget_manifest"] = value.WingetManifest,
            ["catalog_publication"] = value.CatalogPublication,
        };

    private static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string ReadGitCommit(string root)
    {
        var gitPath = Path.Combine(root, ".git");
        if (File.Exists(gitPath))
        {
            var marker = File.ReadAllText(gitPath).Trim();
            if (!marker.StartsWith("gitdir: ", StringComparison.Ordinal))
            {
                return "unknown";
            }
            gitPath = Path.GetFullPath(Path.Combine(root, marker[8..]));
        }
        var head = File.ReadAllText(Path.Combine(gitPath, "HEAD")).Trim();
        if (!head.StartsWith("ref: ", StringComparison.Ordinal))
        {
            return head;
        }
        var reference = head[5..];
        var loose = Path.Combine(gitPath, reference.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(loose))
        {
            return File.ReadAllText(loose).Trim();
        }
        var packed = Path.Combine(gitPath, "packed-refs");
        if (File.Exists(packed))
        {
            var suffix = $" {reference}";
            var row = File.ReadLines(packed).FirstOrDefault(line => line.EndsWith(suffix, StringComparison.Ordinal));
            if (row is not null)
            {
                return row[..row.IndexOf(' ')];
            }
        }
        return "unknown";
    }

    private static InvalidDataException Invalid(string detail) => new($"{ErrorCode}|{detail}");

    [GeneratedRegex(@"^\d+\.\d+\.\d+")]
    private static partial Regex VersionPrefixRegex();

    [GeneratedRegex(@"^(\d+)\.(\d+)\.(\d+)$")]
    private static partial Regex ExactMsiVersionRegex();

    [GeneratedRegex(@"^\d+\.\d+\.\d+-dev(?:\.\d+)?$")]
    private static partial Regex DevVersionRegex();

    [GeneratedRegex(@"^\d+\.\d+\.\d+-admin-smoke(?:\.\d+)?$")]
    private static partial Regex AdminSmokeVersionRegex();

    [GeneratedRegex(@"^\d+\.\d+\.\d+-rc\.\d+$")]
    private static partial Regex RcVersionRegex();

    [GeneratedRegex(@"^\d+\.\d+\.\d+$")]
    private static partial Regex StableVersionRegex();
}

internal static partial class InstallerBuildSourcePolicy
{
    private const string ErrorCode = "PCV_INSTALLER_BUILD_SOURCE_INVALID";

    internal static InstallerBuildSourceBoundary Validate(
        string buildScript,
        string buildModule,
        string productWxs)
    {
        var combined = string.Join('\n', buildScript, buildModule);
        var parameters = ParameterRegex().Matches(combined)
            .Select(match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var required in new[]
        {
            "Version", "DesktopNodeHostPath", "DesktopNodeCliPath", "OutputRoot",
            "MsiProductVersion", "SigningTrustModel", "DryRun",
        })
        {
            if (!parameters.Contains(required))
            {
                throw Invalid($"parameter:{required}");
            }
        }

        var canonical = Canonical(combined);
        foreach (var (expected, detail) in new[]
        {
            ("'--self-contained','true'", "self-contained"),
            ("'-p:PublishSingleFile=true'", "single-file"),
            ("[ValidateSet('RequireSigned','AllowUnsignedDev')]", "signing-modes"),
            ("[ValidateSet('Unspecified','LocalTest','InternalEnterprise','PublicTrusted')]", "trust-models"),
            ("$dotnetPublishAssemblyVersion=\"$msiProductVersion.0\"", "assembly-version"),
            ("([int]$msiProductVersionParts[0]+1)", "file-version"),
            ("\"-p:AssemblyVersion=$dotnetPublishAssemblyVersion\"", "publish-assembly-version"),
            ("\"-p:FileVersion=$dotnetPublishFileVersion\"", "publish-file-version"),
            ("\"-p:InformationalVersion=$Version\"", "publish-informational-version"),
            ("PCV_INSTALLER_SIGNING_REQUIRED", "signing-error"),
            ("PCV_INSTALLER_SERVICE_HOST_NOT_FOUND", "host-error"),
            ("PCV_INSTALLER_CLI_NOT_FOUND", "cli-error"),
            ("PCV_INSTALLER_RELEASE_SIGNING_REQUIRED", "release-signing-error"),
            ("PCV_INSTALLER_WIX_BUILD_FAILED", "wix-error"),
            ("product_name='PureCVisorDesktopNode'", "plan-product-name"),
            ("output_root=$outputRootFull", "plan-output-root"),
            ("payload_root=$payloadRoot", "plan-payload-root"),
            ("wix_source_files=$wixSourcePaths", "plan-wix-sources"),
            ("mode='internal-artifact-descriptor-only'", "publication-mode"),
            ("public_trusted_signing='not-claimed'", "publication-signing"),
            ("external_stable_publication='not-claimed'", "publication-external"),
            ("$wixSourcePaths=@((Join-Path$installerRoot'Product.wxs'),(Join-Path$installerRoot'ProductActions.wxs'))", "wix-source-order"),
            ("$wixArgs=@('build')+$wixSourcePaths+@('-arch','x64','-define',\"MsiProductVersion=$msiProductVersion\",'-define',\"PayloadRoot=$payloadRoot\",'-out',$msiPath)", "wix-argument-order"),
            ("(Split-Path-Leaf$resolvedPayloadRoot)-ne'payload'-or-not(Test-PcvChildPath-Path$resolvedPayloadRoot-Parent$outputRootFull)", "payload-containment"),
            ("Remove-Item-LiteralPath$resolvedPayloadRoot-Recurse-Force", "payload-clean"),
            ("destination=Join-Path$payloadRoot'DesktopNode.Host.exe'", "host-payload"),
            ("destination=Join-Path$payloadRoot'pcvcli.exe'", "cli-payload"),
            ("'web\\app.js','web\\index.html','web\\styles.css'", "web-payload"),
            ("git_commit=Get-PcvGitCommit-RepositoryRoot$repoRoot", "git-provenance"),
            ("\"$msiHash$(Split-Path-Leaf$msiPath)\"|Set-Content-LiteralPath$msiSha256Path-EncodingASCII", "hash-sidecar"),
            ("$publicationDescriptor|ConvertTo-Json-Depth8|Set-Content-LiteralPath$publicationPath-EncodingUTF8", "publication-sidecar"),
        })
        {
            if (!canonical.Contains(Canonical(expected), StringComparison.Ordinal))
            {
                throw Invalid(detail);
            }
        }

        var hasTui = new[] { "DesktopNodeTuiPath", "DesktopNode.Tui", "pcvtui.exe", "PCV_INSTALLER_TUI_" }
            .Any(value => combined.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
            productWxs.Contains("DesktopNodeTui", StringComparison.OrdinalIgnoreCase) ||
            productWxs.Contains("pcvtui.exe", StringComparison.OrdinalIgnoreCase);
        var hasSpike = Regex.IsMatch(
            combined,
            @"spikes[\\/]purecvisor-desktop-node",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (hasTui)
        {
            throw Invalid("tui-boundary");
        }
        if (hasSpike)
        {
            throw Invalid("spike-source");
        }

        var wix = XDocument.Parse(productWxs, LoadOptions.None);
        XNamespace ns = "http://wixtoolset.org/schemas/v4/wxs";
        var fileIds = wix.Descendants(ns + "File")
            .Select(element => (string?)element.Attribute("Id"))
            .Where(value => value is not null)
            .ToHashSet(StringComparer.Ordinal);
        if (!fileIds.Contains("DesktopNodeCli") || !fileIds.Contains("DesktopNodeWebApp"))
        {
            throw Invalid("product-wxs-active-payloads");
        }

        return new InstallerBuildSourceBoundary(
            parameters,
            true,
            true,
            true,
            true,
            true,
            hasTui,
            hasSpike);
    }

    private static string Canonical(string value) =>
        WhitespaceRegex().Replace(
            value.Replace("`\r\n", string.Empty, StringComparison.Ordinal)
                .Replace("`\n", string.Empty, StringComparison.Ordinal),
            string.Empty);

    private static InvalidDataException Invalid(string detail) => new($"{ErrorCode}|{detail}");

    [GeneratedRegex(@"\[(?:string|switch)\]\$(?<name>[A-Za-z][A-Za-z0-9]*)", RegexOptions.CultureInvariant)]
    private static partial Regex ParameterRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
