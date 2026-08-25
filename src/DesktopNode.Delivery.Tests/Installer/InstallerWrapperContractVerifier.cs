using System.Text.RegularExpressions;

namespace DesktopNode.Delivery.Tests.Installer;

internal sealed record InstallerWrapperRequest(string Version, string OutputRoot)
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
    internal string? WixPath { get; init; }
    internal bool DryRun { get; init; }
    internal int SignToolExitCode { get; init; }
    internal string SignToolStdout { get; init; } = string.Empty;
    internal string SignToolStderr { get; init; } = string.Empty;
}

internal sealed record InstallerWrapperSourceBoundary(
    bool UsesTypedInputMap,
    bool EmitsCompressedJson,
    bool PreservesNonzeroExitCode,
    bool NormalizesFalseZeroToOne,
    bool RequestsElevation,
    bool UsesShellConcatenation);

internal sealed record InstallerWrapperResultProjection(
    IReadOnlyList<string> Arguments,
    InstallerBuildResultProjection BuildResult);

internal static partial class InstallerWrapperContractVerifier
{
    private const string ErrorCode = "PCV_INSTALLER_WRAPPER_SOURCE_INVALID";

    internal static InstallerWrapperSourceBoundary Inspect(string source)
    {
        var canonical = Canonical(source);
        foreach (var (expected, detail) in new[]
        {
            ("$buildInput=@{Version=$VersionMsiProductVersion=$MsiProductVersionDesktopNodeHostPath=$DesktopNodeHostPathDesktopNodeCliPath=$DesktopNodeCliPathOutputRoot=$OutputRootSigningMode=$SigningModeSigningTrustModel=$SigningTrustModelSignToolPath=$SignToolPathCertificateThumbprint=$CertificateThumbprintCertificatePath=$CertificatePathTimestampUrl=$TimestampUrlWixPath=$WixPathDryRun=[bool]$DryRun}", "typed-input-map"),
            ("$payload=Invoke-PcvDesktopNodeInstallerBuild-Input$buildInput", "module-input"),
            ("$payload|ConvertTo-Json-Depth12-Compress", "json"),
            ("if([bool]$payload.ok){exit0}", "success-exit"),
            ("$exitCode=[int]$payload.exit_code", "exit-propagation"),
            ("if($exitCode-eq0){$exitCode=1}", "false-zero"),
            ("exit$exitCode", "failure-exit"),
        })
        {
            if (!canonical.Contains(Canonical(expected), StringComparison.Ordinal))
            {
                throw Invalid(detail);
            }
        }

        var requestsElevation = new[]
        {
            "#requires -RunAsAdministrator", "Start-Process", "-Verb RunAs", "UseShellExecute",
        }.Any(value => source.Contains(value, StringComparison.OrdinalIgnoreCase));
        var usesShellConcatenation = Regex.IsMatch(
            source,
            @"(?:cmd(?:\.exe)?\s+/c|powershell(?:\.exe)?\s+-Command|pwsh(?:\.exe)?\s+-Command)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (requestsElevation)
        {
            throw Invalid("elevation");
        }
        if (usesShellConcatenation)
        {
            throw Invalid("shell-concatenation");
        }

        return new InstallerWrapperSourceBoundary(true, true, true, true, requestsElevation, usesShellConcatenation);
    }

    internal static InstallerWrapperResultProjection Execute(
        InstallerBuildContractHarness harness,
        InstallerWrapperSourceBoundary boundary,
        InstallerWrapperRequest request)
    {
        if (!boundary.UsesTypedInputMap || boundary.RequestsElevation || boundary.UsesShellConcatenation)
        {
            throw Invalid("boundary");
        }

        var arguments = new List<string> { "-Version", request.Version };
        Add(arguments, "-MsiProductVersion", request.MsiProductVersion);
        Add(arguments, "-DesktopNodeHostPath", request.DesktopNodeHostPath);
        Add(arguments, "-DesktopNodeCliPath", request.DesktopNodeCliPath);
        Add(arguments, "-OutputRoot", request.OutputRoot);
        Add(arguments, "-SigningMode", request.SigningMode);
        Add(arguments, "-SigningTrustModel", request.SigningTrustModel);
        Add(arguments, "-SignToolPath", request.SignToolPath);
        Add(arguments, "-CertificateThumbprint", request.CertificateThumbprint);
        Add(arguments, "-CertificatePath", request.CertificatePath);
        Add(arguments, "-TimestampUrl", request.TimestampUrl);
        Add(arguments, "-WixPath", request.WixPath);
        if (request.DryRun)
        {
            arguments.Add("-DryRun");
        }

        var result = harness.Execute(new InstallerBuildInput(request.Version, request.OutputRoot)
        {
            MsiProductVersion = request.MsiProductVersion,
            DesktopNodeHostPath = request.DesktopNodeHostPath,
            DesktopNodeCliPath = request.DesktopNodeCliPath,
            SigningMode = request.SigningMode,
            SigningTrustModel = request.SigningTrustModel,
            SignToolPath = request.SignToolPath,
            CertificateThumbprint = request.CertificateThumbprint,
            CertificatePath = request.CertificatePath,
            TimestampUrl = request.TimestampUrl,
            DryRun = request.DryRun,
            SignToolExitCode = request.SignToolExitCode,
            SignToolStdout = request.SignToolStdout,
            SignToolStderr = request.SignToolStderr,
        });
        return new InstallerWrapperResultProjection(arguments, result);
    }

    private static void Add(List<string> arguments, string name, string? value)
    {
        if (value is null)
        {
            return;
        }
        arguments.Add(name);
        arguments.Add(value);
    }

    private static string Canonical(string value) => WhitespaceRegex().Replace(value, string.Empty);

    private static InvalidDataException Invalid(string detail) => new($"{ErrorCode}|{detail}");

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
